using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using HarmonyLib;
using LocoSim.Definitions;
using LocoSim.Implementations;
using Newtonsoft.Json.Linq;
using RearrangedS282;
using static RearrangedS282.BigEndOilingPlan;

internal static class NativeProbe {
    private static readonly List<string> Checks = new List<string>();
    private const BindingFlags Flags = BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    private static void Check(bool result,string label) { if(!result) throw new Exception("FAIL: "+label); Checks.Add(label); }
    private static object Call(string name,params object[] args) {
        var method=typeof(BigEndOiling).GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public);
        if(method==null) throw new MissingMethodException(name);
        try { return method.Invoke(null,args); } catch(TargetInvocationException ex) { throw ex.InnerException??ex; }
    }
    private static PortDefinition Port(string id,bool input=false) => new PortDefinition(input?PortType.EXTERNAL_IN:PortType.READONLY_OUT,PortValueType.GENERIC,id);
    private static ManualOilingPointsDefinition Definition(int count,string id="oil") {
        var d=(ManualOilingPointsDefinition)FormatterServices.GetUninitializedObject(typeof(ManualOilingPointsDefinition));
        d.ID=id; d.capacity=2; d.consumptionPerRev=.0001f; d.leakPerHour=.01f; d.refillRate=.5f;
        foreach(var field in typeof(ManualOilingPointsDefinition).GetFields(Flags)) {
            if(field.FieldType==typeof(PortDefinition)) field.SetValue(d,Port(field.Name));
            if(field.FieldType==typeof(PortReferenceDefinition)) field.SetValue(d,new PortReferenceDefinition(PortValueType.GENERIC,field.Name));
        }
        d.oilingPoints=Enumerable.Range(0,count).Select(i=>new ManualOilingPointsDefinition.OilingPointDefinition {
            oilLevelReadOut=Port("OIL_"+i),oilLevelNormalizedReadOut=Port("NORMAL_"+i),pointDoorExtIn=Port("DOOR_"+i,true),
            refillExtIn=Port("REFILL_"+i,true),refillingFlowNormalizedReadOut=Port("FLOW_"+i)
        }).ToArray();
        return d;
    }
    private static ManualOilingPoints Create(ManualOilingPointsDefinition d,bool notify=true,bool seed=true) {
        var p=(ManualOilingPoints)d.InstantiateImplementation();
        p.SetGameParams(new SimGameParams(true,true,true,1,1));
        for(int i=0;seed&&i<p.oilingPoints.Length;i++) {
            var point=p.oilingPoints[i]; point.pointDoorExtIn.Value=1; point.oilLevelReadOut.Value=.31f+i*.01f;
            point.oilLevelNormalizedReadOut.Value=.15f; point.refillExtIn.Value=.4f; point.refillingFlowNormalizedReadOut.Value=.7f;
        }
        // Exercise the exact production postfix with the native factory's result.
        var hook=typeof(BigEndOiling).GetMethod("RegisterOwnedPoints",BindingFlags.Static|BindingFlags.NonPublic);
        if(notify && hook!=null) Call("RegisterOwnedPoints",d,p);
        return p;
    }
    private static SimulationFlow Flow(params SimComponent[] components) {
        var f=(SimulationFlow)FormatterServices.GetUninitializedObject(typeof(SimulationFlow));
        (typeof(SimulationFlow).GetField("OrderedSimComps",Flags)??typeof(SimulationFlow).GetField("<OrderedSimComps>k__BackingField",Flags)).SetValue(f,components.ToList());
        return f;
    }
    private static JArray Snapshot(ManualOilingPoints p) => new JArray(p.oilingPoints.Select(x=>new JArray(x.pointDoorExtIn.Value,x.oilLevelReadOut.Value,x.oilLevelNormalizedReadOut.Value,x.refillExtIn.Value,x.refillingFlowNormalizedReadOut.Value)));
    private static void CheckPreserved(ManualOilingPoints points,JArray expected,string stage) {
        var observed=Snapshot(points);
        for(int i=0;i<points.oilingPoints.Length;i++) {
            Check(JToken.DeepEquals(observed[i][1],expected[i][1])&&JToken.DeepEquals(observed[i][2],expected[i][2]),stage+": oil levels preserved for cup "+i);
            Check(JToken.DeepEquals(observed[i][0],expected[i][0]),stage+": lid position preserved for cup "+i);
            Check(JToken.DeepEquals(observed[i][3],expected[i][3])&&JToken.DeepEquals(observed[i][4],expected[i][4]),stage+": refill input and flow preserved for cup "+i);
        }
    }
    private static JObject Save(string id,int count) => new JObject { [id]=new JObject { ["oilingPoints"]=new JArray(Enumerable.Range(0,count).Select(i=>new JObject { ["open"]=i%2==1,["oilLevel"]=(i%7)*.1f,["custom"]="keep"+i })),["unknown"]="preserve" },["other"]=new JObject { ["value"]=123 } };
    // Model native flow dispatch to each component's loader. The production
    // prefix clones only that component's argument; the source document is intact.
    private static JObject Fit(SimulationFlow flow,JObject saved) {
        var hook=typeof(BigEndOiling).GetMethod("FitSavedOil",BindingFlags.Static|BindingFlags.NonPublic);
        if(hook.GetParameters()[0].ParameterType==typeof(SimulationFlow)) { var oldArgs=new object[]{flow,saved}; Call("FitSavedOil",oldArgs); return (JObject)oldArgs[1]; }
        if(flow==null||saved==null) return saved;
        JObject view=saved;
        foreach(var point in flow.OrderedSimComps.OfType<ManualOilingPoints>()) {
            var component=saved[point.id] as JObject; if(component==null) continue;
            var args=new object[]{point,component}; Call("FitSavedOil",args);
            if(!ReferenceEquals(args[1],component)) { if(ReferenceEquals(view,saved)) view=(JObject)saved.DeepClone(); view[point.id]=(JObject)args[1]; }
        }
        return view;
    }
    private static ManualOilingPoints Owned(string id="owned",bool seed=true) { var d=Definition(6,id); Call("ExtendDefinition",d); return Create(d,true,seed); }

    private static void WriteReport(string path) {
        var report=new JObject{["passed"]=true,["checks"]=Checks.Count,["nativeOilingAndSaveCode"]=true,["nativeSimulationAssembly"]=typeof(ManualOilingPoints).Assembly.Location,["limitations"]="Train identity and Harmony registration are stand-ins; no Unity/game runtime test",["labels"]=new JArray(Checks)};
        File.WriteAllText(path,report.ToString()); Console.WriteLine(report.ToString());
    }

    public static void Main(string[] args) {
        if(args.Length>1&&args[1]=="save-regression") {
            var regressionForeign=Create(Definition(8,"foreignSave"));
            var regressionSaved=Save(regressionForeign.id,25); var regressionOriginal=regressionSaved.DeepClone();
            var regressionAfter=Fit(Flow(regressionForeign),regressionSaved);
            Check(ReferenceEquals(regressionAfter,regressionSaved)&&JToken.DeepEquals(regressionSaved,regressionOriginal),"Foreign eight-cup component must not truncate a 25-point save");
            WriteReport(args[0]); return;
        }
        // This fails against the old production source before reaching new helpers.
        foreach(int count in new[]{8,6,7,12,20,24}) {
            var foreign=Create(Definition(count,"foreign"+count));
            var before=Snapshot(foreign);
            for(int tick=0;tick<8;tick++) Call("HoldUnusedFull",foreign);
            Check(JToken.DeepEquals(before,Snapshot(foreign)),"Unowned "+count+"-cup native component: lids, oil and refill untouched");
            foreach(int savedCount in new[]{0,6,8,20,25}) {
                var saved=Save(foreign.id,savedCount); var clone=saved.DeepClone(); var fitted=Fit(Flow(foreign),saved);
                Check(ReferenceEquals(fitted,saved)&&JToken.DeepEquals(clone,saved),"Unowned "+count+" cups / saved "+savedCount+": same input object, no save mutation");
            }
            BigEndOiling.SetActive(Flow(foreign),new bool[TotalPoints]);
            Check(JToken.DeepEquals(before,Snapshot(foreign)),"SetActive cannot claim foreign "+count+"-cup component");
            Check(BigEndOiling.Points(Flow(foreign))==null,"Foreign "+count+"-cup component not offered to cup presentation");
        }
        var nativeControl=Create(Definition(8,"nativeControl")); var nativeForeign=Create(Definition(8,"nativeForeign"));
        for(int tick=0;tick<20;tick++) {
            nativeControl.Tick(.05f); nativeForeign.Tick(.05f); Call("HoldUnusedFull",nativeForeign);
            Check(JToken.DeepEquals(Snapshot(nativeControl),Snapshot(nativeForeign)),"Native foreign Tick matches unpatched control at step "+tick);
        }
        var harmony=new Harmony(); BigEndOiling.Register(harmony);
        Check(harmony.Targets.Count==4 && harmony.Targets.Any(t=>t.Contains("ManualOilingPointsDefinition::InstantiateImplementation:RegisterOwnedPoints")),"Native creation hook registered alongside scoped tick/save hooks");
        Check(harmony.Targets.Any(t=>t.Contains("ManualOilingPoints::SetSaveStateData:FitSavedOil"))&&!harmony.Targets.Any(t=>t.Contains("SimulationFlow::SetSaveStateData")),"Save migration uses native per-component loader, no whole-flow hook");
        var own=Owned(); var flow=Flow(own);
        var nativeBefore=Snapshot(own).Take(6).Select(x=>x.DeepClone()).ToArray();
        Call("HoldUnusedFull",own);
        Check(Enumerable.Range(0,6).All(i=>JToken.DeepEquals(nativeBefore[i],Snapshot(own)[i])),"Owned default: six original cups unchanged");
        Check(own.oilingPoints.Skip(6).All(p=>p.pointDoorExtIn.Value==0&&p.oilLevelReadOut.Value==own.capacity&&p.oilLevelNormalizedReadOut.Value==1&&p.refillingFlowNormalizedReadOut.Value==0),"Owned default: unused extras held full and closed");
        var active=Enumerable.Range(0,TotalPoints).Select(i=>i<6||i==8||i==15).ToArray();
        own.oilingPoints[8].pointDoorExtIn.Value=1; own.oilingPoints[8].oilLevelReadOut.Value=.73f;
        own.oilingPoints[15].pointDoorExtIn.Value=1; own.oilingPoints[15].oilLevelReadOut.Value=.87f;
        BigEndOiling.SetActive(flow,active);
        Check(own.oilingPoints[8].pointDoorExtIn.Value==1&&own.oilingPoints[8].oilLevelReadOut.Value==.73f&&own.oilingPoints[15].pointDoorExtIn.Value==1,"Active owned cups keep lid and oil values");
        var foreignFirst=Create(Definition(12,"first"));
        Check(ReferenceEquals(BigEndOiling.Points(Flow(foreignFirst,own)),own),"Mixed flow selects registered owner, not first foreign component");
        var old=Save(own.id,6); old[foreignFirst.id]=Save(foreignFirst.id,25)[foreignFirst.id]; var original=old.DeepClone();
        var resized=Fit(Flow(foreignFirst,own),old);
        Check(!ReferenceEquals(resized,old)&&JToken.DeepEquals(original,old),"Owned legacy migration clones input; original save stays unchanged");
        Check(((JArray)resized[own.id]["oilingPoints"]).Count==TotalPoints,"Owned native six-point save expands to current capacity");
        Check(JToken.DeepEquals(resized[foreignFirst.id],old[foreignFirst.id])&&JToken.DeepEquals(resized["other"],old["other"]),"Mixed save preserves foreign oiling component and unknown fields");
        Check(Enumerable.Range(0,6).All(i=>JToken.DeepEquals(resized[own.id]["oilingPoints"][i],old[own.id]["oilingPoints"][i])),"Legacy native cup state and custom fields preserved");
        var loaded=Owned(own.id,false); loaded.SetSaveStateData((JObject)resized[own.id]);
        Check(loaded.oilingPoints[1].pointDoorExtIn.Value==1&&Math.Abs(loaded.oilingPoints[5].oilLevelReadOut.Value-1f)<.00001f,"Actual native save loader accepts migrated lid and oil levels");
        Check(loaded.oilingPoints.Skip(6).All(p=>p.pointDoorExtIn.Value==0&&p.oilLevelReadOut.Value==loaded.capacity),"Native loader restores new extras full and closed");
        var shipped=Save(own.id,LegacyTotalPoints);var shippedOriginal=shipped.DeepClone();var migrated=Fit(flow,shipped);
        Check(!ReferenceEquals(migrated,shipped)&&JToken.DeepEquals(shipped,shippedOriginal),"Twenty-point migration clones the save without modifying it");
        Check(((JArray)migrated[own.id]["oilingPoints"]).Count==TotalPoints&&Enumerable.Range(0,LegacyTotalPoints).All(i=>JToken.DeepEquals(migrated[own.id]["oilingPoints"][i],shipped[own.id]["oilingPoints"][i])),"Twenty shipped lid/oil/custom records preserve their exact indices");
        var migratedNative=Owned(own.id,false);migratedNative.SetSaveStateData((JObject)migrated[own.id]);
        Check(Enumerable.Range(0,LegacyTotalPoints).All(i=>migratedNative.oilingPoints[i].pointDoorExtIn.Value==(i%2==1?1f:0f)&&Math.Abs(migratedNative.oilingPoints[i].oilLevelReadOut.Value-(i%7)*.1f*migratedNative.capacity)<.00001f),"Native loader accepts all twenty preserved lid/oil values");
        Check(migratedNative.oilingPoints.Skip(LegacyTotalPoints).All(p=>p.pointDoorExtIn.Value==0&&p.oilLevelReadOut.Value==migratedNative.capacity),"Native loader initializes only four appended points full and closed");
        var nativeRoundTrip=migratedNative.GetSaveStateData();var roundTrip=Owned(own.id,false);roundTrip.SetSaveStateData(nativeRoundTrip);
        Check(JToken.DeepEquals(Snapshot(migratedNative),Snapshot(roundTrip)),"Native 24-point save/load roundtrip retains every cup");
        foreach(int count in new[]{0,3,8,19,21,23,24,25}) {
            var saved=Save(own.id,count); var clone=saved.DeepClone(); var after=Fit(flow,saved);
            Check(ReferenceEquals(after,saved)&&JToken.DeepEquals(saved,clone),"Owned saved count "+count+" is not truncated or guessed");
        }
        Check(Fit(flow,null)==null&&Fit(null,old)==old,"Missing flow/save safe and unchanged");
        var malformed=new JObject{[own.id]=new JObject{["oilingPoints"]="not an array"}};
        Check(ReferenceEquals(Fit(flow,malformed),malformed),"Malformed saved point list left to native loader");
        // A foreign definition can copy all names and count and must remain unowned.
        var impostorDef=Definition(TotalPoints,"impostor");
        for(int i=6;i<TotalPoints;i++) foreach(var field in typeof(ManualOilingPointsDefinition.OilingPointDefinition).GetFields(Flags)) if(field.FieldType==typeof(PortDefinition)) {
            var p=(PortDefinition)field.GetValue(impostorDef.oilingPoints[i]); p.ID+="_RSBIG"+i;
        }
        var impostor=Create(impostorDef); var impostorBefore=Snapshot(impostor); Call("HoldUnusedFull",impostor);
        Check(JToken.DeepEquals(impostorBefore,Snapshot(impostor)),"Count and copied RSBIG names do not claim an unregistered definition");
        var replaced=Owned("replaced"); typeof(ManualOilingPoints).GetField("oilingPoints",Flags).SetValue(replaced,Create(Definition(TotalPoints,"replacement"),false).oilingPoints);
        var replacementBefore=Snapshot(replaced); Call("HoldUnusedFull",replaced);
        Check(JToken.DeepEquals(replacementBefore,Snapshot(replaced))&&ReferenceEquals(Fit(Flow(replaced),old),old),"Replacing runtime array invalidates ownership");
        var changedDef=Definition(6,"changedDef"); Call("ExtendDefinition",changedDef); changedDef.oilingPoints=Definition(20,"thirdParty").oilingPoints;
        var changed=Create(changedDef); var changedBefore=Snapshot(changed); Call("HoldUnusedFull",changed);
        Check(JToken.DeepEquals(changedBefore,Snapshot(changed)),"Replacing definition array cannot transfer ownership");
        var damagedDef=Definition(6,"changedPort"); Call("ExtendDefinition",damagedDef); damagedDef.oilingPoints[6].pointDoorExtIn.ID="OTHER";
        var damaged=Create(damagedDef); var damagedBefore=Snapshot(damaged); Call("HoldUnusedFull",damaged);
        Check(JToken.DeepEquals(damagedBefore,Snapshot(damaged)),"Changed extension port rejects ownership");
        var pooled=Definition(6,"pool");
        for(int cycle=0;cycle<25;cycle++) {
            Call("ExtendDefinition",pooled); var p=Create(pooled); var initial=Snapshot(p).Take(6).Select(x=>x.DeepClone()).ToArray(); Call("HoldUnusedFull",p);
            Check(p.oilingPoints.Length==TotalPoints&&p.oilingPoints.Skip(6).All(x=>x.pointDoorExtIn.Value==0),"Pooled definition recreation "+cycle+": extras scoped");
            Check(Enumerable.Range(0,6).All(i=>JToken.DeepEquals(initial[i],Snapshot(p)[i])),"Pooled definition recreation "+cycle+": native cups preserved");
        }
        Check(!RearrangedS282.Main.settings.automaticCupOiling,"Oiling assistance defaults off");
        RearrangedS282.Main.settings.automaticCupOiling=true;
        foreach(bool assistance in new[]{false,true}) {
            RearrangedS282.Main.settings.automaticCupOiling=assistance;
            var replica=Owned("replica-"+assistance);var replicaFlow=Flow(replica);var replicaBefore=Snapshot(replica);
            var replicaMask=Enumerable.Range(0,TotalPoints).Select(i=>i<7).ToArray();
            MultiplayerSync.IsSimulationAuthority=false;
            try {
                for(int i=0;i<100;i++)Call("HoldUnusedFull",replica);
                CheckPreserved(replica,replicaBefore,"Guest before mask, assistance "+assistance);
                BigEndOiling.SetActive(replicaFlow,replicaMask);
                CheckPreserved(replica,replicaBefore,"Guest immediately after SetActive, assistance "+assistance);
                for(int i=0;i<100;i++)Call("HoldUnusedFull",replica);
                CheckPreserved(replica,replicaBefore,"Guest after masked ticks, assistance "+assistance);
            } finally { MultiplayerSync.IsSimulationAuthority=true; }
            Call("HoldUnusedFull",replica);
            var hostAfter=Snapshot(replica);
            for(int i=0;i<TotalPoints;i++) {
                bool held=assistance||!replicaMask[i];
                Check(held ? replica.oilingPoints[i].oilLevelReadOut.Value==replica.capacity &&
                    replica.oilingPoints[i].oilLevelNormalizedReadOut.Value==1f &&
                    replica.oilingPoints[i].pointDoorExtIn.Value==0f &&
                    replica.oilingPoints[i].refillingFlowNormalizedReadOut.Value==0f &&
                    JToken.DeepEquals(hostAfter[i][3],replicaBefore[i][3])
                    : JToken.DeepEquals(hostAfter[i],replicaBefore[i]),
                    "Host correctly preserves active or fills/closes inactive cup "+i+", assistance "+assistance);
            }
        }
        RearrangedS282.Main.settings.automaticCupOiling=true;
        var assisted=Owned("assisted");var assistedFlow=Flow(assisted);BigEndOiling.SetActive(assistedFlow,Enumerable.Repeat(true,TotalPoints).ToArray());
        Check(assisted.oilingPoints.All(x=>x.oilLevelReadOut.Value==assisted.capacity&&x.oilLevelNormalizedReadOut.Value==1&&x.pointDoorExtIn.Value==0),"Assistance fills and closes every owned active cup");
        for(int tick=0;tick<20;tick++) {
            assisted.Tick(.05f);Call("HoldUnusedFull",assisted);
            Check(assisted.oilingPoints.All(x=>x.oilLevelReadOut.Value==assisted.capacity&&x.pointDoorExtIn.Value==0),"Native tick retains assisted full/closed state "+tick);
        }
        var untouched=Create(Definition(20,"foreign-assisted"),false);var untouchedBefore=Snapshot(untouched);Call("HoldUnusedFull",untouched);
        Check(JToken.DeepEquals(untouchedBefore,Snapshot(untouched)),"Enabled assistance never claims a foreign locomotive");
        var replacedAssisted=Owned("assisted-replaced");typeof(ManualOilingPoints).GetField("oilingPoints",Flags).SetValue(replacedAssisted,Create(Definition(TotalPoints,"other"),false).oilingPoints);
        var replacedBefore=Snapshot(replacedAssisted);Call("HoldUnusedFull",replacedAssisted);
        Check(JToken.DeepEquals(replacedBefore,Snapshot(replacedAssisted)),"Enabled assistance respects invalidated ownership");
        RearrangedS282.Main.settings.automaticCupOiling=false;
        foreach(int i in new[]{0,6,13}) {assisted.oilingPoints[i].pointDoorExtIn.Value=1;assisted.oilingPoints[i].oilLevelReadOut.Value=.45f;}
        Call("HoldUnusedFull",assisted);
        Check(new[]{0,6,13}.All(i=>assisted.oilingPoints[i].pointDoorExtIn.Value==1&&assisted.oilingPoints[i].oilLevelReadOut.Value==.45f),"Switching assistance off immediately restores native and extra manual cups");
        Check(RearrangedS282.Main.Logger.Warnings.Count==0,"No unexpected warnings or swallowed save errors");
        WriteReport(args[0]);
    }
}
