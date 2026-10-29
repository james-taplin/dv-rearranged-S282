using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DV;
using DV.ThingTypes;
using Newtonsoft.Json.Linq;
using RearrangedS282;
using UnityEngine;

static class Program {
    static int checks;
    static void Check(bool value,string label) { if(!value)throw new Exception("FAIL: "+label); checks++; }
    static readonly BindingFlags Private=BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    static object Hook(string nested,string method,params object[] args)=>typeof(ConventionalSpawn).GetNestedType(nested,Private).GetMethod(method,Private).Invoke(null,args);
    static TrainCar Car() { var car=new GameObject().AddComponent<TrainCar>(); car.gameObject.AddComponent<LocoMeshSplitter.MeshLoaders.S282AMeshLoader>(); return car; }
    static JObject Data()=>new(){["foreign"]=new JObject{["state"]="keep"},[ConventionalArrangementState.SaveKey]=new JObject{["version"]=1,["arrangement"]=34}};
    static JObject Save(TrainCar car,JObject data=null) { data??=new JObject(); var saver=car.gameObject.GetComponent<SimCarStateSave>()??car.gameObject.AddComponent<SimCarStateSave>(); Hook("SavingCarState","Postfix",saver,data); return data; }
    static void Start(TrainCar car)=>Hook("NewLogicCar","Postfix",car);
    static ConventionalArrangementState State(ConventionalSpawn owner)=>(ConventionalArrangementState)typeof(ConventionalSpawn).GetField("state",Private).GetValue(owner);

    public static void Main(string[] args) {
        foreach(string kind in new[]{"generic heavy","custom steam","copied stock ID","demonstrator livery","unique stock","missing livery","missing logic"}) {
            var car=Car();
            if(kind=="generic heavy")car.carLivery=new(){id="LocoSteamHeavy"};
            if(kind=="custom steam")car.carLivery=new(){id="CCL_CustomSteam"};
            if(kind=="copied stock ID")car.carLivery=new(){id="LocoS282A"};
            if(kind=="demonstrator livery")car.carLivery=new(){id="LocoS282A_Demonstrator"};
            if(kind=="unique stock")car.uniqueCar=true;
            if(kind=="missing livery")car.carLivery=null;
            if(kind=="missing logic")car.logicCar=null;
            var data=Data(); var before=data.DeepClone();
            Save(car,data);
            Check(car.GetComponent<ConventionalSpawn>()==null,kind+": saving does not attach an owner");
            Check(JToken.DeepEquals(data,before),kind+": save data unchanged");
            Hook("LoadingCarState","Postfix",car,data); Start(car); Hook("TrainCarStarted","Postfix",car);
            ConventionalSpawn.RecordApplied(car,35);
            Check(car.GetComponent<ConventionalSpawn>()==null,kind+": load/spawn/radio record cannot claim car");
            Check(car.GetComponent<WheelRearranger>()==null&&JToken.DeepEquals(data,before),kind+": no conversion or save mutation");
            Check((bool)Hook("ReturningToPool","Prefix",car),kind+": native teardown allowed");
        }
        Check(MultiplayerSync.Forgotten.Count==1&&MultiplayerSync.Forgotten.All(S282Target.IsNormal),"Only the normal native livery enters multiplayer pool cleanup, including a car whose logic is already detached");
        MultiplayerSync.Guest=true;
        var guest=Car();Hook("LoadingCarState","Postfix",guest,Data());Start(guest);Hook("TrainCarStarted","Postfix",guest);
        Check(guest.GetComponent<ConventionalSpawn>()==null&&guest.GetComponent<WheelRearranger>()==null,"Guest spawn and local save restoration defer entirely to authoritative state");
        ConventionalSpawn.RecordApplied(guest,48);var guestOwner=guest.GetComponent<ConventionalSpawn>();
        Check(State(guestOwner).AppliedId==48&&State(guestOwner).PendingId==null&&guestOwner.Started.Count==0,"Guest records an authoritative Express choice without starting local conversion");
        Check((int)Save(guest)[ConventionalArrangementState.SaveKey]["arrangement"]==48,"Guest accepted arrangement is recorded instead of stale local saved choice");
        var guestGear=guest.gameObject.AddComponent<WheelRearranger>();
        Check((bool)Hook("ReturningToPool","Prefix",guest)&&guestGear.Reset==1&&!State(guestOwner).HasSelection&&MultiplayerSync.Forgotten.Contains(guest),"Guest pool return restores gear, clears saved ownership and forgets its network identity");
        MultiplayerSync.Guest=false;
        Check(!S282Target.IsNormal(null),"Null car excluded");
        var normal=Car(); Check(S282Target.IsNormal(normal),"Canonical non-unique S282 accepted");
        Check((int)Save(normal)[ConventionalArrangementState.SaveKey]["arrangement"]==10,"Normal save without spawn callback writes stock choice");
        var owner=normal.GetComponent<ConventionalSpawn>();
        ConventionalSpawn.RecordApplied(normal,35);
        Check((int)Save(normal)[ConventionalArrangementState.SaveKey]["arrangement"]==35,"Normal manual arrangement saved");
        var reload=Car(); Hook("LoadingCarState","Postfix",reload,Save(normal)); var reloaded=reload.GetComponent<ConventionalSpawn>();
        Check(State(reloaded).PendingId==35,"Normal arrangement restored as pending");
        var queued=reloaded.Started.Single().Routine; queued.MoveNext(); queued.MoveNext();
        Check(reload.GetComponent<WheelRearranger>().Applied==1&&State(reloaded).PendingId==null,"Normal saved arrangement applied through real coroutine");
        var future=new JObject{["version"]=999,["arrangement"]=999,["extra"]="keep"};
        Hook("LoadingCarState","Postfix",normal,new JObject{[ConventionalArrangementState.SaveKey]=future});
        Check(JToken.DeepEquals(Save(normal)[ConventionalArrangementState.SaveKey],future),"Normal future payload preserved verbatim");
        var nativeOnly=Car(); RearrangedS282.Main.settings.spawnRandomWA=true;
        Hook("LoadingCarState","Postfix",nativeOnly,new JObject());
        Check((int)Save(nativeOnly)[ConventionalArrangementState.SaveKey]["arrangement"]==10,"Old native save not retroactively randomized");
        var random=Car(); Start(random); var randomOwner=random.GetComponent<ConventionalSpawn>();
        Check(State(randomOwner).PendingId==34,"Normal new spawn still uses configured random pool");
        var interrupted=Car();Start(interrupted);var interruptedOwner=interrupted.GetComponent<ConventionalSpawn>();var deferred=interruptedOwner.Started.Single().Routine;
        deferred.MoveNext();MultiplayerSync.Guest=true;deferred.MoveNext();
        Check(interrupted.GetComponent<WheelRearranger>()==null&&State(interruptedOwner).PendingId==34,"An already queued local spawn cannot convert after entering guest mode");
        MultiplayerSync.Guest=false;
        random.uniqueCar=true; var late=randomOwner.Started.Single().Routine; late.MoveNext(); late.MoveNext();
        Check(random.GetComponent<WheelRearranger>()==null,"Identity change cancels queued conversion before geometry mutation");
        var unrelated=Data(); var copy=unrelated.DeepClone(); Save(random,unrelated);
        Check(JToken.DeepEquals(copy,unrelated),"Excluded car with existing owner does not overwrite saves");
        ConventionalSpawn.RecordApplied(random,35); Check(State(randomOwner).PendingId==34,"Excluded car cannot update stale owner state");
        var pool=Car(); ConventionalSpawn.RecordApplied(pool,35); var poolOwner=pool.GetComponent<ConventionalSpawn>();
        var gear=pool.gameObject.AddComponent<WheelRearranger>();
        Check((bool)Hook("ReturningToPool","Prefix",pool)&&gear.Reset==1&&!State(poolOwner).HasSelection,"Owned normal pool return restores and clears selection");
        pool.CarGUID="B"; Start(pool);
        Check(State(poolOwner).Guid=="B"&&State(poolOwner).PendingId==34,"Reused normal car starts fresh GUID and selection");
        var canonical=Globals.G.Types.Normal; var types=Globals.G.Types;
        types.Available=false; Check(!S282Target.IsNormal(Car()),"Missing native registry entry fails closed"); types.Available=true;
        Globals.G=null; Check(!S282Target.IsNormal(normal),"Missing Globals fails closed"); Globals.G=new Globals{Types=types};
        var harmony=new HarmonyLib.Harmony();S282SpawnScope.Register(harmony);
        Check(harmony.SpawnTargets==3,"All three native spawn paths registered");
        var enter=typeof(S282SpawnScope).GetMethod("Enter",Private);var exit=typeof(S282SpawnScope).GetMethod("Exit",Private);
        var uniqueArgs=new object[]{true,null};enter.Invoke(null,uniqueArgs);
        Check(!S282Target.IsNormal(normal),"Unique spawn excluded during Awake before instance flag assignment");
        var unclaimed=Car();Start(unclaimed);Check(unclaimed.GetComponent<ConventionalSpawn>()==null,"Unique spawn scope prevents premature lifecycle ownership");
        var nested=new object[]{false,null};enter.Invoke(null,nested);
        Check(S282Target.IsNormal(normal),"Nested ordinary spawn uses its own native argument");
        exit.Invoke(null,new object[]{nested[1],null});Check(!S282Target.IsNormal(normal),"Nested scope restores unique parent");
        var failure=new Exception("native spawn failure");
        Check(ReferenceEquals(exit.Invoke(null,new object[]{uniqueArgs[1],failure}),failure)&&S282Target.IsNormal(normal),"Exception finalizer restores scope and preserves native failure");
        enter.Invoke(null,uniqueArgs);exit.Invoke(null,new object[]{null,null});
        Check(!S282Target.IsNormal(normal),"Skipped prefix finalizer cannot overwrite another spawn scope");
        exit.Invoke(null,new object[]{uniqueArgs[1],null});Check(S282Target.IsNormal(normal),"Ordinary eligibility restored after spawn completes");
        Console.WriteLine(checks);
        File.WriteAllText(args[0],new JObject{["passed"]=true,["checks"]=checks,["limitations"]="Managed train/registry/coroutine stand-ins; actual production ownership and save-hook bodies, no Unity runtime"}.ToString());
    }
}
