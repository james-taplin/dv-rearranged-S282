using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using RearrangedS282;
using UnityEngine;
using UMM=UnityModManagerNet.UnityModManager;

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
            CommsRadioS282Rearranger.Controller=controller;radio.Awake();radio.SetStartingDisplay();CommsRadio_Awake_Patch.rearranger=radio;
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
        foreach(string reason in new[]{"no target","wrong type","out of range","no signal origin","destroyed target"})
        {
            cases++;var r=new Rig();int records=ConventionalSpawn.Records.Count;
            if(reason!="no target")Set(r.radio,"PointedCar",r.a);
            if(reason=="wrong type")r.a.carType=DV.ThingTypes.TrainCarType.Other;
            if(reason=="out of range")r.a.transform.position=new Vector3(0,0,100.01f);
            if(reason=="no signal origin")r.radio.signalOrigin=null;
            if(reason=="destroyed target")UnityEngine.Object.Destroy(r.a);
            r.radio.ReapplyFromSettings();Check(r.wa.Calls.Count==0&&r.wb.Calls.Count==0&&ConventionalSpawn.Records.Count==records,reason+" refuses without mutation");
            Check(CommsRadioS282Rearranger.ReapplyStatus.Contains("Highlight"),reason+" actionable status");
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
        var report=new{passed=true,checks,cases,compiledProduction=new[]{"Main.cs","CommsRadioS282Rearranger.cs","WheelArrangement.cs"},limitations="Managed stand-ins test actual mod state/input guards and GUI callback wiring only; they do not reproduce native CanScroll, hotbar input, Unity/UMM rendering, VR, native conversion, save persistence or the reported in-game cycle failure."};
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[0])));File.WriteAllText(args[0],JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"Radio/settings integration passed {checks} checks across {cases} cases.");return 0;
    }
    static string radioState(Rig r)=>Get(r.radio,"CurrentState").ToString();
}
