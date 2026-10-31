using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using DV.Simulation.Ports;
using DV.Wheels;
using LocoSim.Definitions;
using LocoSim.Implementations;
using Newtonsoft.Json.Linq;
using RearrangedS282;
using UnityEngine;

public sealed class TenderClockTimingProbe : MonoBehaviour
{
    public Animator target;
    public float speed = 1f;
    void LateUpdate() { target.SetFloat("SpeedMultiplier", speed); }
}

public static class GroupAnimationProbe
{
    static readonly List<string> checks=new List<string>();
    static void Check(bool value,string name) { if(!value)throw new Exception(name); checks.Add(name); }
    static float Phase(Animator a)=>DriveQuartering.Cycle(a.GetCurrentAnimatorStateInfo(0).normalizedTime);
    static bool Near(float a,float b)=>Mathf.Abs(Mathf.DeltaAngle(a*360,b*360))<.05f;
    static ReciprocatingSteamEngine Engine(string id,out Port phase)
    {
        var engine=(ReciprocatingSteamEngine)FormatterServices.GetUninitializedObject(typeof(ReciprocatingSteamEngine));
        phase=new Port(id,new PortDefinition(PortType.READONLY_OUT,PortValueType.GENERIC,"CRANK_ROTATION"));
        typeof(ReciprocatingSteamEngine).GetField("crankRotationReadOut",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(engine,phase);
        return engine;
    }
    public static IEnumerator Run(string output)
    {
        var controller=Resources.Load<RuntimeAnimatorController>("GroupPhase");
        Check(controller!=null,"Real Unity phase fixture controller loaded");
        var roots=new List<GameObject>();
        var animators=new List<Animator>();
        foreach(string name in new[]{"A-left","A-right","B-left","B-right"})
        {
            var go=new GameObject(name); roots.Add(go);
            new GameObject("wheel").transform.SetParent(go.transform,false);
            var animator=go.AddComponent<Animator>(); animator.runtimeAnimatorController=controller;
            animator.cullingMode=AnimatorCullingMode.AlwaysAnimate; animator.SetFloat("SpeedMultiplier",0);
            animator.Play("Drive",0,0); animators.Add(animator);
        }
        yield return null; yield return null;
        GroupCrankAnimation a=null,b=null;
        try
        {
            var ea=Engine("a",out var pa); var eb=Engine("b",out var pb);
            var originalSpeed=new Port("legacy",new PortDefinition(PortType.READONLY_OUT,PortValueType.GENERIC,"SPEED"));
            originalSpeed.Value=0;
            var original=animators[0].gameObject.AddComponent<AnimatorPortReader>();
            original.updateType=AnimatorPortReader.UpdateType.SET_PARAMETER; original.parameterName="SpeedMultiplier"; original.Init(originalSpeed);
            a=GroupCrankAnimation.Bind(ea,new[]{animators[0],animators[1]},new[]{.1f,.35f},Array.Empty<PoweredWheelRotationViaAnimation>());
            b=GroupCrankAnimation.Bind(eb,new[]{animators[2],animators[3]},new[]{.6f,.85f},Array.Empty<PoweredWheelRotationViaAnimation>());
            pa.Value=.2f; originalSpeed.Value=3;
            yield return null; yield return null;
            Check(Near(Phase(animators[0]),.3f)&&Near(Phase(animators[1]),.55f),"Own native phase port drives first group through normal Animator evaluation");
            Check(Near(Phase(animators[2]),.6f)&&Near(Phase(animators[3]),.85f),"Second group remains at its own phase");
            Check(animators[0].GetFloat("SpeedMultiplier")==0 && animators[0].speed==0,"Competing native speed subscription cannot advance owned animation");
            Check(Mathf.Abs(Mathf.DeltaAngle(animators[0].transform.Find("wheel").localEulerAngles.z,108))<.1f,"Evaluated wheel pose follows phase, not only Animator metadata");
            pa.Value=.95f; yield return null; yield return null;
            pa.Value=.05f; yield return null; yield return null;
            Check(Near(Phase(animators[0]),.15f)&&Near(Phase(animators[1]),.4f),"Wrapped/reversed phase updates retain quartering");
            pb.Value=.7f; yield return null; yield return null;
            Check(Near(Phase(animators[2]),.3f)&&Near(Phase(animators[3]),.55f)&&Near(Phase(animators[0]),.15f),"Each group changes without seeking another group");
            a.Dispose(); a.Dispose(); a=null;
            Check(!GroupCrankAnimation.Owns(animators[0])&&original.enabled&&animators[0].GetFloat("SpeedMultiplier")==3,"Release restores original reader and its current port value once");
            // Rebind before deferred Destroy has removed the first phase reader.
            originalSpeed.Value=0;
            a=GroupCrankAnimation.Bind(ea,new[]{animators[0],animators[1]},new[]{.15f,.4f},Array.Empty<PoweredWheelRotationViaAnimation>());
            a.Dispose(); a=null;
            pa.Value=.7f; yield return null; yield return null;
            Check(Near(Phase(animators[0]),.15f),"Released phase subscription no longer seeks Animator");
            Check(animators[0].GetComponents<AnimatorPortReader>().Length==1,"Same-frame release and rebind leaves only original native reader");
            b.Dispose(); b=null;
            Check(!GroupCrankAnimation.Owns(animators[2]),"Second group releases independently");
            // Late join must use the host's baseline, even when the local pose
            // and replicated native engine phase have already moved apart.
            a=GroupCrankAnimation.Bind(ea,new[]{animators[0],animators[1]},
                globalDrivers:Array.Empty<PoweredWheelRotationViaAnimation>(),explicitOffsets:new[]{.2f,.45f});
            yield return null; yield return null;
            Check(Near(Phase(animators[0]),.9f)&&Near(Phase(animators[1]),.15f),"Replicated offsets replace the late joiner's previous local phase baseline");
            pa.Value=.05f; yield return null; yield return null;
            Check(Near(Phase(animators[0]),.25f)&&Near(Phase(animators[1]),.5f),"Replicated offsets follow later native engine phase updates and preserve quartering");
            var clockSource = new GameObject("host-speed"); roots.Add(clockSource);
            var sourceAnimator = clockSource.AddComponent<Animator>(); sourceAnimator.runtimeAnimatorController=controller;
            sourceAnimator.cullingMode=AnimatorCullingMode.AlwaysAnimate; sourceAnimator.SetFloat("SpeedMultiplier",0);
            var clockTarget = new GameObject("tender-speed"); roots.Add(clockTarget);
            new GameObject("wheel").transform.SetParent(clockTarget.transform,false);
            var targetAnimator = clockTarget.AddComponent<Animator>(); targetAnimator.runtimeAnimatorController=controller;
            targetAnimator.cullingMode=AnimatorCullingMode.AlwaysAnimate; targetAnimator.Play("Drive",0,0);
            var mirror=clockTarget.AddComponent<MalletAnimatorMirror>(); mirror.Source=sourceAnimator; mirror.Target=targetAnimator;
            mirror.CopySpeed=false; // Same configuration as AttachTenderEngine.
            var clock=clockTarget.AddComponent<TenderClockTimingProbe>(); clock.target=targetAnimator;
            yield return null; yield return null;
            float tenderStart=targetAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            yield return new WaitForSecondsRealtime(.1f);
            float tenderEnd=targetAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            Check(tenderEnd-tenderStart > .001f,"Tender animation advances when phase-owned host SpeedMultiplier is zero");
            targetAnimator.Play("Drive",0,.5f);
            clock.speed=-1f;
            yield return null; yield return null;
            tenderStart=targetAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            yield return new WaitForSecondsRealtime(.1f);
            tenderEnd=targetAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            Check(tenderEnd-tenderStart < -.001f,"Tender clock reverses its own animation while host speed remains zero: " + tenderStart + " -> " + tenderEnd + " speed=" + targetAnimator.GetFloat("SpeedMultiplier"));
            clock.enabled=false; mirror.CopySpeed=true; sourceAnimator.SetFloat("SpeedMultiplier",.6f);
            yield return null; yield return null;
            Check(Mathf.Abs(targetAnimator.GetFloat("SpeedMultiplier")-.6f)<.0001f,"Ordinary non-tender mirror continues copying host speed");
            File.WriteAllText(output,new JObject{["passed"]=true,["checks"]=new JArray(checks),["limitations"]="Real Unity player, synthetic controller/wheel, actual native AnimatorPortReader and Port, production GroupCrankAnimation, MalletAnimatorMirror and DriveQuartering. Tender clock is represented by a LateUpdate speed writer. RSE is detached with only crank phase readout initialized; no cylinder Tick, actual S282/Franklin meshes, native global driver leasing, or live car lifecycle tested."}.ToString());
            Debug.Log("GROUP_ANIMATION_PASS "+checks.Count);
        }
        finally
        {
            a?.Dispose(); b?.Dispose(); foreach(var go in roots) UnityEngine.Object.Destroy(go);
        }
    }
}
