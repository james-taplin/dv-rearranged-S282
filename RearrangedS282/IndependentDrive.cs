#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.CompilerServices;
using DV;
using DV.ECS.Train;
using DV.Simulation.Cars;
using DV.Utils;
using HarmonyLib;
using LocoSim.Definitions;
using LocoSim.Implementations;
using Newtonsoft.Json.Linq;
using Unity.Transforms;
using UnityEngine;

namespace RearrangedS282
{
    // One force owner per arrangement. Native ECS computes slip; native cylinder
    // engines integrate phase and consume the original shared boiler supply.
    internal sealed class IndependentDrive : MonoBehaviour
    {
        internal sealed class Definition
        {
            internal ReciprocatingSteamEngine Engine;
            internal Animator[] Animators;
            internal int[] Contacts;
            internal string Role;
            internal Transform EngineRoot;
            internal float[] InitialPhases;
        }
        sealed class Group
        {
            internal Definition Definition;
            internal Port OriginalRpm, RpmPort;
            internal float Rpm, Velocity, Demand, Force, Brake;
            internal float[] PhaseOffsets;
            internal IndependentDriveReplication.Channel Replication;
            internal IDisposable Animation;
        }
        static readonly ConditionalWeakTable<SimulationFlow, IndependentDrive> Flows = new();
        static readonly Dictionary<DrivingForce, IndependentDrive> Forces = new();
        static int nextGeneration;
        TrainCar car;
        ArticulatedSimulation simulation;
        NativeSlipRuntimeState state;
        IndependentDriveReplication replication;
        Group[] groups;
        float radius;
        int powered;
        bool active, released, authority;
        internal bool Ready => !released && (authority ? state != null && state.HasCompletedSample :
            MultiplayerSync.IsClient && groups != null && replication != null &&
            replication.Matches(MultiplayerSync.SessionEpoch, MultiplayerSync.RevisionFor(car.CarGUID), groups.Length));
        internal bool Settled => !active || groups.All(g=>Mathf.Abs(g.Rpm)<.1f) &&
            groups.Select((g,i)=>Slip(i)).All(s=>s==0);
        internal float DiagnosticFirstGroupGrip = 1f;
        internal bool IsOwning => active && Ready;
        internal bool Recording;
        float recordAt;
        internal bool TryGetWheelState(LocoSim.Implementations.Wheels.PoweredWheel wheel,out float slip,out float rpm)
        {
            slip=rpm=0;
            if(!IsOwning || wheel==null || wheel.wheelTransform==null) return false;
            for(int i=0;i<groups.Length;i++)
                if(wheel.wheelTransform.IsChildOf(groups[i].Definition.EngineRoot))
                {slip=Slip(i);rpm=authority?groups[i].Rpm:groups[i].Replication.Rpm.Value;return true;}
            return false;
        }
        void Update()
        {
            if(released) return;
            if(authority && !MultiplayerSync.IsSimulationAuthority || !authority && !MultiplayerSync.IsClient)
            { Release(); return; }
            if(!authority)
            {
                try { UpdateReplica(); }
                catch(Exception error)
                {
                    Main.Logger.Warning("Independent replica animation failed: "+error);
                    Release();
                    try { MultiplayerSync.ReportFailure?.Invoke("Independent wheel animation could not follow the host: "+error.Message); }
                    catch(Exception reportError) { Main.Logger.Warning("Could not report independent replica failure: "+reportError.Message); }
                    return;
                }
            }
            if(!active || !Ready) return;
            GroupSlipEffects.Tick(car,this);
            if(Recording && Time.unscaledTime>=recordAt)
            {
                recordAt=Time.unscaledTime+.25f;
                var report=Report(); report["car"]=car.ID;report["time"]=Time.unscaledTime;
                File.AppendAllText(Path.Combine(Main.ModPath,"independent-slip.jsonl"),report.ToString(Newtonsoft.Json.Formatting.None)+Environment.NewLine);
            }
        }

        internal static void Register(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(SimulationFlow), nameof(SimulationFlow.Tick)),
                prefix:new HarmonyMethod(typeof(IndependentDrive),nameof(BeforeTick)));
            harmony.Patch(AccessTools.Method(typeof(DrivingForce),"FixedUpdate"),
                prefix:new HarmonyMethod(typeof(IndependentDrive),nameof(BeforeForce)));
        }

        internal void Initialize(TrainCar host, ArticulatedSimulation sim, Definition[] definitions)
        {
            if(groups!=null) throw new InvalidOperationException("Independent traction already owns this arrangement.");
            car=host; simulation=sim;
            authority=MultiplayerSync.IsSimulationAuthority;
            radius=host.SimController.drivingForce.wheelRadius;
            if(definitions.Length<2 || definitions.Length>IndependentDriveReplication.MaximumGroups ||
                definitions.Any(d=>d.Engine==null || d.Animators==null || d.Animators.Length!=2 ||
                    d.Contacts.Length!=host.Bogies.Length || d.Contacts.Sum()==0))
                throw new InvalidOperationException("Independent engine/contact map is incomplete.");
            powered=definitions.Sum(d=>d.Contacts.Sum());
            if(powered!=host.SimController.wheelslipController.NumberOfPoweredAxles)
                throw new InvalidOperationException("Independent contact allocation does not match native powered axles.");
            replication=new IndependentDriveReplication(sim.Flow,car.CarGUID);
            if(authority) replication.Clear();
            groups=definitions.Select((d,i)=>new Group {Definition=d, OriginalRpm=d.Engine.crankRpm.port,
                Rpm=host.GetForwardSpeed()*60f/(2f*Mathf.PI*radius),
                Replication=replication.Groups[i],
                RpmPort=authority?new Port(d.Engine.id,new PortDefinition(PortType.READONLY_OUT,PortValueType.RPM,"INDEPENDENT_RPM")):null}).ToArray();
            if(authority)
            {
                state=new NativeSlipRuntimeState(transform,host,groups.Length,++nextGeneration);
                state.PrepareInputs+=Prepare;
                state.OutputsReady+=Sample;
                state.Failed+=error=> { Main.Logger.Warning("Independent native calculation failed; restoring native drive: "+error); Release(); };
                Flows.Remove(sim.Flow); Flows.Add(sim.Flow,this);
            }
            Forces[host.SimController.drivingForce]=this;
        }

        void Prepare()
        {
            try { PrepareCore(); }
            catch(Exception error) { Main.Logger.Warning("Independent drive input failed; restoring native drive: "+error); Release(); }
        }
        void PrepareCore()
        {
            if(!authority || !MultiplayerSync.IsSimulationAuthority) { Release(); return; }
            if(released || car==null || car.derailed || car.isExploded) { Release(); return; }
            if(!DV.TimeUtil.IsFlowing || SingletonBehaviour<CarSpawner>.Instance.PoolSetupInProgress ||
                SingletonBehaviour<PausePhysicsHandler>.Instance.PhysicsHandlingInProcess) return;
            var manager=car.entity.EntityManager;
            var shared=manager.GetComponentData<AdhesionControllerSystem.StaticSharedAdhesionCalculationData>(car.entity.Entity);
            var friction=manager.GetComponentData<AdhesionControllerSystem.StaticAdhesionCalculationData>(car.entity.Entity);
            var calc=manager.GetComponentData<AdhesionControllerSystem.AdhesionCalculationData>(car.entity.Entity);
            calc.weightPerAxle=car.massController.WeightPerAxle;
            calc.absSpeed=car.GetAbsSpeed();
            calc.sandCoef=car.SimController.wheelslipController.SandCoef;
            calc.brakingForce=car.RearBogie.brakingForce;
            calc.isDerailed=car.derailed; calc.isGrounded=car.groundFriction.IsGrounded;
            state.SetCarrier(shared,friction,calc,new LocalToWorld{Value=car.transform.localToWorldMatrix});
            var slide=manager.GetComponentData<AdhesionControllerSystem.WheelSlideData>(car.entity.Entity);
            var wsc=car.SimController.wheelslipController;
            for(int i=0;i<groups.Length;i++)
            {
                var group=groups[i]; int count=group.Definition.Contacts.Sum();
                group.Demand=group.Definition.Engine.torqueOut.Value/radius;
                group.Brake=car.RearBogie.brakingForce*car.Bogies.Length*count/powered;
                state.SetGroup(i,new WheelslipControllerSystem.WheelslipInputData {
                    generatedForce=group.Demand,brakingForce=group.Brake/shared.bogieCount,
                    numberOfPoweredAxles=checked((ushort)count),maxWheelslipRpm=wsc.maxWheelslipRpm,
                    wheelslipToAdhesionDrop=wsc.wheelslipToAdhesionDrop.Evaluate(state.GetOutput(i).wheelslip),
                    preventWheelslip=wsc.preventWheelslip,isEngineBraking=wsc.IsEngineBraking },shared,slide,i==0?DiagnosticFirstGroupGrip:1f);
            }
        }

        void Sample()
        {
            if(!authority || !MultiplayerSync.IsSimulationAuthority || released || state==null || !state.HasCompletedSample) return;
            if(active) { PublishReplication(); return; }
            try
            {
                foreach(var group in groups)
                {
                    group.RpmPort.Value=group.Rpm;
                    group.Definition.Engine.crankRpm.SetPortReference(group.RpmPort);
                    group.PhaseOffsets=group.Definition.Animators.Select((animator,i)=>DriveQuartering.Cycle(
                        (group.Definition.InitialPhases!=null?group.Definition.InitialPhases[i]:animator.GetCurrentAnimatorStateInfo(0).normalizedTime)
                        -group.Definition.Engine.crankRotationReadOut.Value)).ToArray();
                    group.Animation=GroupCrankAnimation.Bind(group.Definition.Engine,group.Definition.Animators,
                        group.Definition.InitialPhases,explicitOffsets:group.PhaseOffsets);
                }
                active=true;
                PublishReplication();
            }
            catch(Exception error) { Main.Logger.Warning("Independent drive activation failed: "+error); Release(); }
        }

        static void BeforeTick(SimulationFlow __instance,float delta)
        {
            if(Flows.TryGetValue(__instance,out var owner) && owner.authority && MultiplayerSync.IsSimulationAuthority &&
                owner.active && owner.Ready) owner.PublishRpm(delta);
        }
        void PublishRpm(float delta)
        {
            if(!authority || !MultiplayerSync.IsSimulationAuthority) return;
            if(car==null || car.derailed || car.isExploded) { Release(); return; }
            float ground=car.GetForwardSpeed()*60f/(2f*Mathf.PI*radius);
            float slide=car.adhesionController.wheelSlide;
            for(int i=0;i<groups.Length;i++)
            {
                var group=groups[i]; var output=state.GetOutput(i);
                float target=slide>0?Mathf.Lerp(ground,0,slide):Mathf.Lerp(ground,output.orientedMaxWheelslipRpm,output.wheelslip);
                group.Rpm=Mathf.SmoothDamp(group.Rpm,target,ref group.Velocity,.2f,float.PositiveInfinity,delta);
                if(target==0 && Mathf.Abs(group.Rpm)<.01f) group.Rpm=group.Velocity=0;
                group.RpmPort.Value=group.Rpm;
            }
            PublishReplication();
        }
        static bool BeforeForce(DrivingForce __instance)
        {
            if(!Forces.TryGetValue(__instance,out var owner)) return true;
            if(!owner.authority || !MultiplayerSync.IsSimulationAuthority) return false;
            if(!owner.active || !owner.Ready) return true;
            return owner.ApplyForce(__instance);
        }
        bool ApplyForce(DrivingForce original)
        {
            if(!authority || !MultiplayerSync.IsSimulationAuthority) return false;
            if(car==null || car.derailed || car.isExploded) { Release(); return true; }
            if(!DV.TimeUtil.IsFlowing || SingletonBehaviour<PausePhysicsHandler>.Instance.PhysicsHandlingInProcess) return false;
            // Validate all contacts before any AddForce, so failure cannot leave a
            // partial group application followed by native whole-car application.
            for(int b=0;b<car.Bogies.Length;b++)
                if(groups.Any(g=>g.Definition.Contacts[b]>0) && (!car.Bogies[b].enabled || car.Bogies[b].rb==null || car.Bogies[b].rb.isKinematic))
                { Release(); return true; }
            float totalDemand=0;
            var counts=new int[groups.Length,car.Bogies.Length];
            var transmitted=new float[groups.Length];
            for(int i=0;i<groups.Length;i++)
            {
                var group=groups[i]; var output=state.GetOutput(i);
                float demand=group.Definition.Engine.torqueOut.Value/radius;
                totalDemand+=demand;
                if(output.wheelslip>0) demand=Mathf.Sign(demand)*Mathf.Max(0,Mathf.Abs(demand)-group.Brake);
                transmitted[i]=group.Force=car.adhesionController.wheelSlide>0?0:Mathf.Clamp(demand,-output.totalForceLimit,output.totalForceLimit);
                for(int b=0;b<car.Bogies.Length;b++) counts[i,b]=group.Definition.Contacts[b];
            }
            original.generatedForce=totalDemand;
            var forces=GroupForceRouting.Distribute(transmitted,counts);
            if((forces.Any(f=>f!=0) || groups.Select((g,i)=>state.GetOutput(i).wheelslip).Any(s=>s>0)) && car.isEligibleForSleep) car.ForceOptimizationState(sleep:false);
            for(int b=0;b<forces.Length;b++) if(forces[b]!=0) car.Bogies[b].ApplyForce(forces[b]);
            PublishReplication();
            return false;
        }

        float Slip(int index) => authority ? state?.GetOutput(index).wheelslip ?? 0f : groups[index].Replication.Slip.Value;
        float Limit(int index) => authority ? state?.GetOutput(index).totalForceLimit ?? 0f : groups[index].Replication.Limit.Value;

        void PublishReplication()
        {
            if(!authority || !MultiplayerSync.IsSimulationAuthority || replication==null || groups==null) return;
            try
            {
                for(int i=0;i<groups.Length;i++)
                {
                    var group=groups[i];
                    group.Replication.Publish(active,group.Rpm,Slip(i),group.PhaseOffsets?[0]??0f,
                        group.PhaseOffsets?[1]??0f,group.Force,Limit(i));
                }
                replication.PublishMarker(MultiplayerSync.SessionEpoch,MultiplayerSync.RevisionFor(car.CarGUID),active?groups.Length:0);
            }
            catch(Exception error)
            {
                Main.Logger.Warning("Independent drive output publication failed; restoring native drive: "+error);
                Release();
            }
        }

        void UpdateReplica()
        {
            if(!Ready)
            {
                DeactivateReplica();
                return;
            }
            for(int i=0;i<groups.Length;i++)
            {
                var group=groups[i];
                group.Rpm=group.Replication.Rpm.Value;
                group.Force=group.Replication.Force.Value;
            }
            if(active && groups.All(g=>g.PhaseOffsets[0]==g.Replication.PhaseLeft.Value &&
                g.PhaseOffsets[1]==g.Replication.PhaseRight.Value)) return;
            // A fresh native snapshot can replace a previously received phase
            // baseline. Reconcile it instead of keeping a stale local lease.
            if(active) DeactivateReplica();
            try
            {
                foreach(var group in groups)
                {
                    group.PhaseOffsets=new[] { group.Replication.PhaseLeft.Value,group.Replication.PhaseRight.Value };
                    group.Animation=GroupCrankAnimation.Bind(group.Definition.Engine,group.Definition.Animators,
                        explicitOffsets:group.PhaseOffsets);
                }
                active=true;
            }
            catch(Exception error)
            {
                DeactivateReplica();
                throw new InvalidOperationException("Independent replica animation could not bind to the host's phase outputs.",error);
            }
        }

        void DeactivateReplica()
        {
            if(!active && groups?.All(g=>g.Animation==null)!=false) return;
            active=false;
            GroupSlipEffects.Release(car);
            if(groups!=null) foreach(var group in groups) { group.Animation?.Dispose();group.Animation=null; }
        }

        internal JObject Report() => new JObject { ["active"]=active,["ready"]=Ready,["authority"]=authority,["diagnosticFirstGroupGrip"]=DiagnosticFirstGroupGrip,
            ["groups"]=new JArray(groups==null?Enumerable.Empty<JObject>():groups.Select((g,i)=>new JObject {
                ["role"]=g.Definition.Role,["engine"]=g.Definition.Engine.id,["rpm"]=g.Rpm,
                ["peripheralSpeedMs"]=g.Rpm*2f*Mathf.PI*radius/60f,["torqueNm"]=g.Definition.Engine.torqueOut.Value,
                ["forceN"]=g.Force,["limitN"]=Limit(i),["slip"]=Slip(i),
                ["intakeKgS"]=g.Definition.Engine.intakeFlowReadOut.Value,["contacts"]=new JArray(g.Definition.Contacts) })) };

        internal void Release()
        {
            if(released) return; released=true; active=false;
            if(authority) replication?.Clear();
            if(car!=null) GroupSlipEffects.Release(car);
            if(simulation?.Flow!=null && Flows.TryGetValue(simulation.Flow,out var flowOwner) && ReferenceEquals(flowOwner,this))
                Flows.Remove(simulation.Flow);
            if(car?.SimController?.drivingForce!=null && Forces.TryGetValue(car.SimController.drivingForce,out var owner) && ReferenceEquals(owner,this))
                Forces.Remove(car.SimController.drivingForce);
            if(groups!=null) foreach(var group in groups)
            {
                if(authority) group.Definition.Engine.crankRpm.SetPortReference(group.OriginalRpm);
                group.Animation?.Dispose(); group.Animation=null; group.Force=0;
            }
            state?.Dispose(); state=null;
        }
        void OnDisable()=>Release();
        void OnDestroy()=>Release();
    }
}
#endif
