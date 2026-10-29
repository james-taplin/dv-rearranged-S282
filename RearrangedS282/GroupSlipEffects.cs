#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.Linq;
using DV.ModularAudioCar;
using DV.Wheels;
using HarmonyLib;
using UnityEngine;

namespace RearrangedS282
{
    // Reuse native leaf particles and audio definitions. Native wheel-damage
    // audio still updates normally; only the powered-slip inputs are replaced.
    internal static class GroupSlipEffects
    {
        sealed class State
        {
            internal float NextTick;
            internal readonly HashSet<WheelslipSparksController> Controllers = new();
            internal readonly HashSet<WheelsAudioModule> Audio = new();
        }
        static readonly Dictionary<TrainCar, State> States = new();

        internal static void Register(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(WheelslipSparksController), "OnWheelslipStateChanged"),
                prefix:new HarmonyMethod(typeof(GroupSlipEffects),nameof(BeforeNativeSparkEvent)));
            harmony.Patch(AccessTools.Method(typeof(WheelsAudioModule),nameof(WheelsAudioModule.UpdateModule)),
                prefix:new HarmonyMethod(typeof(GroupSlipEffects),nameof(BeforeWheelAudio)),
                postfix:new HarmonyMethod(typeof(GroupSlipEffects),nameof(AfterWheelAudio)),
                finalizer:new HarmonyMethod(typeof(GroupSlipEffects),nameof(RestoreAudioDefinitions)));
        }

        static bool BeforeNativeSparkEvent(WheelslipSparksController __instance)
        {
            var car=__instance.car;
            return car==null || car.GetComponent<IndependentDrive>()?.IsOwning!=true;
        }

        internal static void Tick(TrainCar car, IndependentDrive owner)
        {
            if(car==null || owner==null || !owner.IsOwning) return;
            if(!States.TryGetValue(car,out var state)) States.Add(car,state=new State());
            if(Time.time<state.NextTick) return;
            state.NextTick=Time.time+.1f;
            foreach(var controller in car.GetComponentsInChildren<WheelslipSparksController>(true))
            {
                if(controller==null || controller.car!=car || controller.wheelSparks==null) continue;
                if(!controller.isActiveAndEnabled)
                {
                    if(state.Controllers.Contains(controller))
                        foreach(var definition in controller.wheelSparks)
                        {
                            if(definition?.sparksLeftPS!=null) definition.sparksLeftPS.Stop();
                            if(definition?.sparksRightPS!=null) definition.sparksRightPS.Stop();
                        }
                    continue;
                }
                if(state.Controllers.Add(controller))
                {
                    // Stop an already-running global coroutine without changing
                    // controller enabled state or duplicating event subscribers.
                    controller.OnDisable();
                }
                float ignition=0;
                foreach(var definition in controller.wheelSparks)
                {
                    if(definition==null || definition.sparksLeftPS==null || definition.sparksRightPS==null) continue;
                    bool driven=definition.poweredWheel!=null && definition.poweredWheel.IsPowered;
                    float slip=0,rpm=0;
                    bool mapped=driven && owner.TryGetWheelState(definition.poweredWheel,out slip,out rpm);
                    bool emit=mapped && slip>0 && !car.derailed;
                    ignition=Mathf.Max(ignition,emit?slip:0);
                    foreach(var particle in new[]{definition.sparksLeftPS,definition.sparksRightPS})
                    {
                        if(!emit) { if(particle.isPlaying) particle.Stop(); continue; }
                        particle.transform.localRotation=Quaternion.Euler(rpm>0?controller.directionForwardSparksRotation:controller.directionReverseSparksRotation);
                        var main=particle.main; main.startSpeed=Mathf.Lerp(1.5f,3f,slip);
                        if(!particle.isPlaying) particle.Play();
                    }
                }
                // Preserve one native ignition evaluation per controller at its
                // original 0.1 s cadence, irrespective of group count.
                if(ignition>0 && MultiplayerSync.IsSimulationAuthority) controller.Ignite(ignition);
            }
        }

        static void BeforeWheelAudio(WheelsAudioModule __instance, out WheelsAudioModule.WheelslipAudioDefinition[] __state)
        {
            __state=null;
            var car=__instance.trainCar;
            if(car==null || car.GetComponent<IndependentDrive>()?.IsOwning!=true) return;
            __state=__instance.wheelslipAudioDefs;
            // Avoid starting/stopping gripping-group clips every frame in a
            // postfix after the native shared-slip writer. Damage audio remains
            // in the original method; only its slip definitions are leased.
            __instance.wheelslipAudioDefs=Array.Empty<WheelsAudioModule.WheelslipAudioDefinition>();
        }

        static void RestoreAudioDefinitions(WheelsAudioModule __instance, WheelsAudioModule.WheelslipAudioDefinition[] __state)
        {
            if(__state!=null) __instance.wheelslipAudioDefs=__state;
        }

        static void AfterWheelAudio(WheelsAudioModule __instance, WheelsAudioModule.WheelslipAudioDefinition[] __state)
        {
            RestoreAudioDefinitions(__instance,__state);
            var car=__instance.trainCar;
            var owner=car!=null?car.GetComponent<IndependentDrive>():null;
            if(owner?.IsOwning!=true || __instance.wheelslipAudioDefs==null) return;
            if(!States.TryGetValue(car,out var state)) States.Add(car,state=new State());
            state.Audio.Add(__instance);
            foreach(var definition in __instance.wheelslipAudioDefs)
            {
                if(definition==null) continue;
                float slip=0;
                foreach(var wheel in definition.poweredWheels)
                    if(wheel!=null && wheel.IsPowered && owner.TryGetWheelState(wheel,out float value,out _)) slip=Mathf.Max(slip,value);
                if(definition.wheelslipAudio!=null) definition.wheelslipAudio.Set(car.derailed?0:slip);
                if(definition.derailedWheelslipAudio!=null) definition.derailedWheelslipAudio.Set(car.derailed && car.groundFriction.IsGrounded?slip:0);
            }
        }

        internal static void Release(TrainCar car)
        {
            if(ReferenceEquals(car,null) || !States.TryGetValue(car,out var state)) return;
            States.Remove(car);
            foreach(var module in state.Audio)
                if(module!=null)
                    foreach(var definition in module.wheelslipAudioDefs??Array.Empty<WheelsAudioModule.WheelslipAudioDefinition>())
                    {
                        if(definition?.wheelslipAudio!=null) definition.wheelslipAudio.Set(0);
                        if(definition?.derailedWheelslipAudio!=null) definition.derailedWheelslipAudio.Set(0);
                    }
            foreach(var controller in state.Controllers)
            {
                if(controller==null) continue;
                foreach(var definition in controller.wheelSparks??Array.Empty<WheelslipSparksController.WheelSparksDefinition>())
                {
                    if(definition?.sparksLeftPS!=null) definition.sparksLeftPS.Stop();
                    if(definition?.sparksRightPS!=null) definition.sparksRightPS.Stop();
                }
                // Owner must relinquish IsOwning before calling Release. Resume
                // the native event/coroutine state if pooled slip is still live.
                if(car!=null && !car.derailed && controller.isActiveAndEnabled && controller.wheelslipController?.IsWheelslipping==true && controller.updateSparksCoro==null)
                    controller.OnWheelslipStateChanged(true);
            }
        }
    }
}
#endif
