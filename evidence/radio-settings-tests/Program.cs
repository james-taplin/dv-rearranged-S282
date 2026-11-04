using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using RearrangedS282;
using UnityEngine;
using UMM=UnityModManagerNet.UnityModManager;
using Core=RearrangedS282.Main;

static class Program
{
    static int checks,cases;
    static void Check(bool condition,string label){checks++;if(!condition)throw new Exception(label);}
    static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    static object Get(object o,string field)=>o.GetType().GetField(field,Private).GetValue(o);
    static void Set(object o,string field,object value)=>o.GetType().GetField(field,Private).SetValue(o,value);
    static int Selected(CommsRadioS282Rearranger radio)=>(int)Get(radio,"selectedWheelArrangement");
    sealed class Rig
    {
        internal readonly CommsRadioS282Rearranger radio;
        internal readonly TrainCar a,b;
        internal readonly WheelRearranger wa,wb;
        internal Rig()
        {
            UMM.UI.Instance=new UMM.UI();Physics.Hit=null;Stage2RuntimeChecks.Running=false;PowerDiagnostics.Recording=false;GUILayout.Clear();
            var go=new GameObject("radio");radio=go.AddComponent<CommsRadioS282Rearranger>();
            var controller=go.AddComponent<CommsRadioController>();var deleter=go.AddComponent<CommsRadioCarDeleter>();controller.deleteControl=deleter;
            deleter.signalOrigin=go.transform;deleter.display=go.AddComponent<CommsRadioDisplay>();deleter.selectionMaterial=new Material();deleter.deleteMaterial=new Material();
            deleter.trainHighlighter=new GameObject("native highlighter");deleter.trainHighlighter.AddComponent<MeshRenderer>();
            deleter.hoverOverCar=new AudioClip();deleter.warningSound=new AudioClip();deleter.confirmSound=new AudioClip();deleter.cancelSound=new AudioClip();
            radio.Awake();radio.SetStartingDisplay();CommsRadio_Awake_Patch.rearranger=radio;
            a=Car("A",10);b=Car("B",20);wa=a.gameObject.AddComponent<WheelRearranger>();wb=b.gameObject.AddComponent<WheelRearranger>();
        }
        static TrainCar Car(string id,float z){var go=new GameObject(id);var car=go.AddComponent<TrainCar>();car.ID=id;go.transform.position=new Vector3(0,0,z);go.AddComponent<ExplosionModelHandler>();return car;}
        internal void Point(TrainCar car){Physics.Hit=car?.transform;radio.OnUpdate();}
        internal void SelectA(){Point(a);radio.OnUse();Point(a);}
    }
    static void ResetCheck(Rig r,string label)
    {
        Check(r.radio.SettingsTarget==null,label+" clears target");Check(r.radio.ButtonBehaviour==ButtonBehaviourType.Regular,label+" restores regular input");
        Check(Get(r.radio,"CurrentState").ToString()=="SelectS282",label+" restores select state");Check(!r.radio.trainHighlighter.activeSelf,label+" hides highlighter");
    }
    static int Main(string[] args)
    {
        var entry=new UMM.ModEntry();var loaded=(bool)typeof(Main).GetMethod("Load",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{entry});
        Check(loaded&&entry.OnGUI!=null,"actual Main.Load installs GUI callback");
        Check(WheelArrangement.SupportedCount==51&&WheelArrangement.WheelArrangementNames.Length==51&&
            WheelArrangement.WheelArrangementNames[48]=="4-8-2 Express"&&
            WheelArrangement.WheelArrangementNames[49].StartsWith("4-6-2")&&WheelArrangement.WheelArrangementNames[50].StartsWith("4-6-6-4"),
            "51-choice catalogue preserves 48 and appends the requested Pacific and articulated IDs");
        for(int id=0;id<51;id++) {
            Check(WheelArrangement.IsSupportedId(id),"Saved catalogue ID remains supported: "+id);
            Check(WheelArrangement.NextRuntimeId(id,1)==(id+1)%51&&WheelArrangement.NextRuntimeId(id,-1)==(id+50)%51,"Radio cycle follows fixed saved-ID sequence: "+id);
        }
        Check(!WheelArrangement.IsSupportedId(-1)&&!WheelArrangement.IsSupportedId(51),"Out-of-range saved IDs remain unsupported");
        Check(WheelArrangement.LocomotiveWheels(49).SequenceEqual(new[]{4,6,2})&&WheelArrangement.LocomotiveWheels(50).SequenceEqual(new[]{4,6,6,4}),"New catalogue wheel groups are 4-6-2 and 4-6-6-4");
        foreach(var expected in new[]{(id:49,total:6,powered:3),(id:50,total:10,powered:6)}) {
            var wheels=WheelArrangement.LocomotiveWheels(expected.id);
            Check(wheels.Sum()/2==expected.total&&wheels.Skip(1).Take(wheels.Length-2).Sum()/2==expected.powered,"Physical and powered locomotive axle groups match requested wheel layout "+expected.id);
        }
        Check(WheelArrangement.IsConventionalId(49)&&!WheelArrangement.IsArticulatedId(49)&&WheelArrangement.IsArticulatedId(50)&&!WheelArrangement.IsConventionalId(50),"New IDs select their intended conventional/articulated family");
        {
            cases++;var first=new Rig();var second=new Rig();
            Check(first.radio.display!=second.radio.display&&first.radio.signalOrigin!=second.radio.signalOrigin,"Two radio modes copy each owner's display and signal origin");
            Check(first.radio.HoverCarSound!=second.radio.HoverCarSound&&first.radio.ConfirmSound!=second.radio.ConfirmSound,"Two radios keep each owner's audio clips");
            Check(first.radio.trainHighlighter!=second.radio.trainHighlighter&&first.radio.selectionMaterial!=second.radio.selectionMaterial,"Two radios own separate highlighters and materials");
            var postfix=typeof(CommsRadio_Awake_Patch).GetMethod("Postfix",BindingFlags.Static|BindingFlags.NonPublic);var firstModes=new System.Collections.Generic.List<ICommsRadioMode>();var secondModes=new System.Collections.Generic.List<ICommsRadioMode>();
            foreach(var rig in new[]{first,second}){var modes=rig==first?firstModes:secondModes;var controller=rig.radio.GetComponent<CommsRadioController>();postfix.Invoke(null,new object[]{controller,modes});postfix.Invoke(null,new object[]{controller,modes});}
            Check(firstModes.Count==1&&ReferenceEquals(firstModes[0],first.radio)&&secondModes.Count==1&&ReferenceEquals(secondModes[0],second.radio),"Repeated controller Awake registers one mode per owning radio");
            first.SelectA();second.Point(second.b);second.radio.OnUse();second.Point(second.b);
            Check(first.radio.SettingsTarget==first.a&&second.radio.SettingsTarget==second.b,"Two radios keep independent selected cars");
            int old=Selected(first.radio);second.radio.ButtonBCustomAction();Check(Selected(first.radio)==old,"Cycling one radio does not change another selection");
            Check(CommsRadio_Awake_Patch.rearranger==second.radio,"UMM pointer follows most recently used radio");
            first.radio.OnUse();Check(CommsRadio_Awake_Patch.rearranger==first.radio,"Using first radio restores its UMM pointer");
        }
        {
            cases++;var r=new Rig();r.SelectA();int selected=Selected(r.radio),updates=r.radio.display.Updates,sounds=CommsRadioController.Sounds,calls=Physics.Calls;
            UMM.UI.Instance.Opened=true;Physics.Hit=r.b.transform;r.radio.OnUpdate();r.radio.OnUse();
            Check(r.radio.ButtonACustomAction()&&r.radio.ButtonBCustomAction(),"open UMM consumes custom buttons");
            Check(Selected(r.radio)==selected,"open UMM does not cycle");Check(r.radio.SettingsTarget==r.a,"open UMM preserves selection");
            Check(radioState(r)=="SelectWheelArrangement","open UMM does not advance state");Check(r.radio.display.Updates==updates,"open UMM does not change display");
            Check(Physics.Calls==calls&&CommsRadioController.Sounds==sounds,"open UMM performs no raycast/audio");Check(r.wa.Calls.Count==0&&r.wb.Calls.Count==0,"open UMM performs no conversion");
            UMM.UI.Instance.Opened=false;r.radio.ButtonBCustomAction();Check(Selected(r.radio)==WheelArrangement.NextRuntimeId(selected,1),"closed UMM cycle resumes");
        }
        {
            cases++;var r=new Rig();r.SelectA();Check(r.radio.ButtonBehaviour==ButtonBehaviourType.Override,"selection enables cycling");
            for(int n=0;n<WheelArrangement.SupportedCount*2;n++){int old=Selected(r.radio);Check(r.radio.ButtonBCustomAction(),"closed next handled");Check(Selected(r.radio)==WheelArrangement.NextRuntimeId(old,1),"closed next native catalogue");}
            int after=Selected(r.radio);Check(r.radio.ButtonACustomAction()&&Selected(r.radio)==WheelArrangement.NextRuntimeId(after,-1),"closed previous native catalogue");
            int records=ConventionalSpawn.Records.Count,chosen=Selected(r.radio);r.radio.OnUse();
            Check(r.wa.Calls.SequenceEqual(new[]{chosen})&&r.wb.Calls.Count==0,"closed confirm converts selected car only");
            Check(ConventionalSpawn.Records.Count==records+1&&ConventionalSpawn.Records.Last()==(r.a,chosen),"closed confirm records success only");ResetCheck(r,"closed confirm");
        }
        foreach(int id in new[]{49,50}) {
            cases++;var r=new Rig();r.SelectA();Set(r.radio,"selectedWheelArrangement",id-1);
            Check(r.radio.ButtonBCustomAction()&&Selected(r.radio)==id,"Radio reaches appended arrangement "+id);
            int records=ConventionalSpawn.Records.Count;r.radio.OnUse();
            Check(r.wa.Calls.SequenceEqual(new[]{id})&&ConventionalSpawn.Records.Count==records+1&&ConventionalSpawn.Records.Last()==(r.a,id),"Radio confirms and records appended arrangement "+id);
        }
        {
            cases++;var r=new Rig();r.SelectA();Physics.Hit=null;r.radio.OnUpdate();r.radio.OnUse();Check(r.wa.Calls.Count==0,"turn-away cancel has no conversion");ResetCheck(r,"cancel");
        }
        {
            cases++;var r=new Rig();r.SelectA();r.wa.currentWA=WheelArrangementType.s260;Set(r.radio,"selectedWheelArrangement",5);int records=ConventionalSpawn.Records.Count;
            UMM.UI.Instance.Opened=true;r.radio.ReapplyFromSettings();Check(r.wa.Calls.SequenceEqual(new[]{34})&&r.wb.Calls.Count==0,"explicit UMM reapply uses current layout and one target");
            Check(ConventionalSpawn.Records.Count==records+1&&ConventionalSpawn.Records.Last()==(r.a,34),"UMM reapply records success");
            Check(CommsRadioS282Rearranger.ReapplyStatus.Contains("2-6-0")&&CommsRadioS282Rearranger.ReapplyStatus.Contains("A"),"UMM success status identifies layout and car");ResetCheck(r,"UMM reapply");
        }
        {
            cases++;var r=new Rig();r.Point(r.b);r.wb.currentWA=WheelArrangementType.s240;UMM.UI.Instance.Opened=true;r.radio.ReapplyFromSettings();
            Check(r.wb.Calls.SequenceEqual(new[]{35})&&r.wa.Calls.Count==0,"pointed-only UMM target supported");ResetCheck(r,"pointed reapply");
        }
        foreach(string reason in new[]{"no target","other heavy-steam livery","unique vehicle","unique spawn scope","out of range","no signal origin","destroyed target"})
        {
            cases++;var r=new Rig();int records=ConventionalSpawn.Records.Count;
            if(reason!="no target")Set(r.radio,"PointedCar",r.a);
            if(reason=="other heavy-steam livery")r.a.carLivery=new DV.Livery();
            if(reason=="unique vehicle")r.a.uniqueCar=true;
            if(reason=="unique spawn scope")S282Target.UniqueSpawnInProgress=true;
            if(reason=="out of range")r.a.transform.position=new Vector3(0,0,100.01f);
            if(reason=="no signal origin")r.radio.signalOrigin=null;
            if(reason=="destroyed target")UnityEngine.Object.Destroy(r.a);
            r.radio.ReapplyFromSettings();Check(r.wa.Calls.Count==0&&r.wb.Calls.Count==0&&ConventionalSpawn.Records.Count==records,reason+" refuses without mutation");
            Check(CommsRadioS282Rearranger.ReapplyStatus.Contains("Highlight"),reason+" actionable status");
            S282Target.UniqueSpawnInProgress=false;
        }
        {
            cases++;var r=new Rig();r.Point(r.a);r.a.transform.position=new Vector3(0,0,100);r.radio.ReapplyFromSettings();Check(r.wa.Calls.Count==1,"exact 100m boundary allowed");
        }
        foreach(string error in new[]{"specific fixture refusal",null})
        {
            cases++;var r=new Rig();r.SelectA();r.wa.currentWA=WheelArrangementType.s240;r.wa.AllowApply=false;r.wa.LastConventionalError=error;int records=ConventionalSpawn.Records.Count;
            r.radio.ReapplyFromSettings();Check(r.wa.Calls.SequenceEqual(new[]{35})&&r.wb.Calls.Count==0,"refusal remains target-only");Check(r.wa.currentWA==WheelArrangementType.s240&&ConventionalSpawn.Records.Count==records,"refusal does not record or change choice");
            Check(CommsRadioS282Rearranger.ReapplyStatus.Contains(error??"stop, repair"),"refusal carries useful status");ResetCheck(r,"refused UMM reapply");
        }
        {
            cases++;var r=new Rig();r.SelectA();r.radio.ResetSelectionFromSettings();ResetCheck(r,"explicit reset");Check(CommsRadioS282Rearranger.ReapplyStatus.Contains("selection reset"),"reset status");
            r.Point(r.a);Check(r.radio.SettingsTarget==r.a,"reset allows fresh hover");r.radio.OnUse();Check(radioState(r)=="SelectWheelArrangement","reset allows fresh selection");r.radio.Disable();ResetCheck(r,"disable");
        }
        {
            cases++;var r=new Rig();r.Point(r.a);r.wa.currentWA=WheelArrangementType.s260;UMM.UI.Instance.Opened=true;int records=ConventionalSpawn.Records.Count;
            GUILayout.Clicks.Add("REAPPLY CURRENT ARRANGEMENT — highlighted S282");entry.OnGUI(entry);
            Check(r.wa.Calls.SequenceEqual(new[]{34})&&ConventionalSpawn.Records.Count==records+1,"actual Main GUI button invokes explicit reapply");
            r.radio.ResetSelectionFromSettings();UMM.UI.Instance.Opened=false;r.SelectA();UMM.UI.Instance.Opened=true;GUILayout.Clicks.Add("RESET WHEEL-ARRANGER RADIO SELECTION");entry.OnGUI(entry);ResetCheck(r,"actual Main GUI reset");
        }
        foreach(bool running in new[]{true,false})
        {
            cases++;var r=new Rig();r.Point(r.a);Stage2RuntimeChecks.Running=running;PowerDiagnostics.Recording=!running;GUILayout.Clicks.Add("REAPPLY CURRENT ARRANGEMENT — highlighted S282");entry.OnGUI(entry);
            Check(r.wa.Calls.Count==(running?0:1),"Only active structural test blocks reapply; retired power flag has no effect");
            Check(!GUILayout.Buttons.Any(x=>x.Contains("POWER DIAGNOSTICS")),"Player GUI has no power recording controls");
            Check(GUILayout.Buttons.Count(x=>x.Contains("RUN STRUCTURAL TEST"))==(running?0:1),"One manual structural button when idle");
        }
        {cases++;var r=new Rig();int starts=Stage2RuntimeChecks.Starts;GUILayout.Clicks.Add("RUN STRUCTURAL TEST — two stationary S282s");entry.OnGUI(entry);Check(Stage2RuntimeChecks.Starts==starts+1,"Manual structural button invokes Begin");}
        {
            cases++;var r=new Rig();GUILayout.ToggleValue=true;entry.OnGUI(entry);
            for(int id=0;id<WheelArrangement.SupportedCount;id++)
                Check(Core.settings.PoweredTenderEnabled(id)==PoweredTenderPolicy.Supports(id),"Host powered-tender options follow supported policy for catalogue ID "+id);
            Check(Core.settings.PoweredTenderEnabled(48),"Retained Express supports the optional powered tender");
            Check(Core.settings.PoweredTenderEnabled(49)&&!Core.settings.PoweredTenderEnabled(50),"Express Pacific permits an optional powered tender; Little Boy uses its normal unpowered tender");
        }
        {
            cases++;var r=new Rig();MultiplayerSync.IsClient=MultiplayerSync.InSession=true;MultiplayerSync.AllowClientRequest=false;int records=ConventionalSpawn.Records.Count;
            r.SelectA();r.radio.OnUse();Check(r.wa.Calls.Count==0&&ConventionalSpawn.Records.Count==records,"Locked guest radio cannot convert locally");
            Stage2RuntimeChecks.Running=false;r.SelectA();MultiplayerSync.AllowClientRequest=true;r.radio.OnUse();
            Check(r.wa.Calls.Count==0&&ConventionalSpawn.Records.Count==records&&CommsRadioS282Rearranger.ReapplyStatus.Contains("Requested"),"Permitted guest radio routes request without local conversion");
            GUILayout.Clicks.Add("RUN STRUCTURAL TEST — two stationary S282s");int starts=Stage2RuntimeChecks.Starts;entry.OnGUI(entry);Check(Stage2RuntimeChecks.Starts==starts&&!GUILayout.Buttons.Any(x=>x.Contains("RUN STRUCTURAL TEST")),"Multiplayer session hides stationary structural test");
            GUILayout.ToggleValue=true;int[] tenderChoices=Core.settings.poweredTenderArrangements.ToArray();entry.OnGUI(entry);
            Check(Core.settings.DrawEnabled.Last()==false&&GUI.enabled,"Guest global settings are drawn disabled and previous GUI state restored");
            Check(Core.settings.poweredTenderArrangements.SequenceEqual(tenderChoices),"Guest powered-tender controls cannot mutate local global policy");
            MultiplayerSync.IsClient=MultiplayerSync.InSession=false;GUILayout.Clear();
        }
        var report=new{passed=true,checks,cases,compiledProduction=new[]{"Main.cs","CommsRadioS282Rearranger.cs","WheelArrangement.cs","PoweredTenderPolicy.cs","S282Target.cs"},limitations="Managed stand-ins test actual mod state/input guards, canonical-livery target selection and GUI callback wiring only; they do not reproduce native CanScroll, hotbar input, Unity/UMM rendering, VR, native conversion, save persistence or the reported in-game cycle failure."};
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[0])));File.WriteAllText(args[0],JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"Radio/settings integration passed {checks} checks across {cases} cases.");return 0;
    }
    static string radioState(Rig r)=>Get(r.radio,"CurrentState").ToString();
}
