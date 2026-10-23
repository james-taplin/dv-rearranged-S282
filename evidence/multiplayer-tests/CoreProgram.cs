using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using LocoMeshSplitter.MeshLoaders;
using Newtonsoft.Json;
using RearrangedS282;
using UnityEngine;
using Core=RearrangedS282.Main;

static class Program
{
    static int checks,cases;
    static void Check(bool value,string label){checks++;if(!value)throw new Exception(label);}
    static TrainCar Car(string guid=null,bool mesh=true)
    {
        var go=new GameObject();var car=go.AddComponent<TrainCar>();car.CarGUID=guid??Guid.NewGuid().ToString();car.ID="car-"+car.CarGUID.Substring(0,8);
        go.AddComponent<WheelRearranger>();if(mesh){go.AddComponent<S282AMeshLoader>();go.transform.MeshReady=true;}CarSpawner.Instance.AllCars.Add(car);return car;
    }
    static WheelRearranger Wheels(TrainCar car)=>car.GetComponent<WheelRearranger>();
    static string Json(string guid,int arrangement,long revision,Settings settings=null,bool removed=false)=>JsonConvert.SerializeObject(new NetworkArrangement{CarGuid=guid,Arrangement=arrangement,Revision=revision,SettingsJson=removed?null:JsonConvert.SerializeObject(settings??new Settings()),Removed=removed});
    static void Tick(){Time.unscaledTime+=1;MultiplayerSync.Tick();}
    static void Reset()
    {
        MultiplayerSync.EndSession();Core.settings=new Settings();CarSpawner.Instance.AllCars.Clear();ConventionalSpawn.Records.Clear();Stage2RuntimeChecks.Running=false;UnityModManagerNet.UnityModManager.CurrentMod=null;Time.unscaledTime+=1;
    }
    static void OfflineAndHandshake()
    {
        cases++;Reset();var car=Car();Check(MultiplayerSync.IsSimulationAuthority&&!MultiplayerSync.InSession&&!MultiplayerSync.DeferAutomatic(car),"Offline has simulation and automatic selection authority");
        Check(MultiplayerSync.RequestChange(car,19,out _),"Offline explicit choice applies");Check(Wheels(car).Attempts==1&&ConventionalSpawn.Records.Count==1&&MultiplayerSync.RevisionFor(car.CarGUID)==0,"Offline choice records without network revisions");
        MultiplayerSync.BeginClient();int sent=0;MultiplayerSync.SendRequest=(guid,id,revision)=>sent++;
        Check(MultiplayerSync.IsClient&&!MultiplayerSync.IsSimulationAuthority&&MultiplayerSync.DeferAutomatic(car),"Client defers local random/save choice and simulation authority");
        Check(!MultiplayerSync.BeforeSwitch(car,10),"Client cannot directly switch outside remote apply scope");
        Check(!MultiplayerSync.RequestChange(car,20,out _)&&sent==0&&Wheels(car).Attempts==1,"Unaccepted client cannot mutate or send request");
        MultiplayerSync.AcceptClient(false,"locked");Check(!MultiplayerSync.RequestChange(car,20,out _)&&sent==0,"Accepted locked client cannot send request");
        MultiplayerSync.AcceptClient(true,"allowed");Check(MultiplayerSync.RequestChange(car,20,out _)&&sent==1&&Wheels(car).Attempts==1,"Permitted guest sends request without local mutation");
        MultiplayerSync.RejectClient("mismatch");Check(MultiplayerSync.IsClient&&MultiplayerSync.DeferAutomatic(car)&&!MultiplayerSync.RequestChange(car,20,out _),"Rejected client remains locally locked");
        MultiplayerSync.EndSession();Check(!MultiplayerSync.IsClient&&MultiplayerSync.IsSimulationAuthority,"Disconnect restores offline authority");
    }
    static void AbsentAndDisabledFallback()
    {
        cases++;Reset();string previousPath=Core.ModPath;Core.ModPath=Path.Combine(Path.GetTempPath(),"s282-missing-"+Guid.NewGuid().ToString("N"));int errors=Core.Logger.Rows.Count;
        MultiplayerSync.Initialize();Tick();Check(!MultiplayerSync.InSession&&!MultiplayerSync.IsClient&&MultiplayerSync.IsSimulationAuthority,"Absent Multiplayer leaves the core offline even without adapter DLL");
        UnityModManagerNet.UnityModManager.CurrentMod=new UnityModManagerNet.UnityModManager.ModEntry{Active=false};MultiplayerSync.Initialize();Tick();
        Check(!MultiplayerSync.InSession&&Core.Logger.Rows.Count==errors,"Disabled Multiplayer never attempts to load the absent adapter or logs dependency errors");
        var car=Car();Check(MultiplayerSync.RequestChange(car,19,out _)&&Wheels(car).Attempts==1,"Ordinary local radio path remains usable with Multiplayer disabled");Core.ModPath=previousPath;
    }
    static void HostPolicyAndProfiles()
    {
        cases++;Reset();var car=Car();var other=Car();MultiplayerSync.BeginHost();var published=new List<string>();MultiplayerSync.BroadcastState=published.Add;
        Core.settings.bigEndOiling=false;Core.settings.poweredTenderArrangements=new[]{10,43};
        Check(MultiplayerSync.TryHostRequest(car.CarGUID,19,0,out _),"Host accepts current valid request");
        Check(MultiplayerSync.RevisionFor(car.CarGUID)==1&&published.Count==1&&ConventionalSpawn.Records.Count==1,"Accepted host change increments and broadcasts one revision");
        Check(!ReferenceEquals(MultiplayerSync.SettingsFor(car),Core.settings)&&!MultiplayerSync.SettingsFor(car).bigEndOiling,"Host captures per-car settings clone");
        Core.settings.bigEndOiling=true;Core.settings.poweredTenderArrangements[0]=19;
        Check(!MultiplayerSync.SettingsFor(car).bigEndOiling&&MultiplayerSync.SettingsFor(car).poweredTenderArrangements[0]==10,"Later global/array mutations cannot alter accepted car profile");
        Check(ReferenceEquals(MultiplayerSync.SettingsFor(other),Core.settings),"Unchanged second car retains its own fallback settings");
        int attempts=Wheels(car).Attempts;
        Check(!MultiplayerSync.TryHostRequest(car.CarGUID,20,0,out var reason)&&reason.Contains("changed")&&Wheels(car).Attempts==attempts,"Stale concurrent request refused before conversion");
        Check(!MultiplayerSync.TryHostRequest("not-a-guid",20,0,out _)&&!MultiplayerSync.TryHostRequest(car.CarGUID,48,1,out _),"Invalid GUID and unknown arrangement refused");
        Check(!MultiplayerSync.TryHostRequest(Guid.NewGuid().ToString(),20,0,out _),"Unknown locomotive refused");
        Stage2RuntimeChecks.Running=true;Check(!MultiplayerSync.TryHostRequest(car.CarGUID,20,1,out _)&&Wheels(car).Attempts==attempts,"Structural run blocks concurrent host requests");Stage2RuntimeChecks.Running=false;
        Wheels(car).AllowApply=false;Check(!MultiplayerSync.TryHostRequest(car.CarGUID,20,1,out _)&&MultiplayerSync.RevisionFor(car.CarGUID)==1&&published.Count==1,"Failed host conversion does not publish or increment revision");
        Check(!MultiplayerSync.SettingsFor(car).bigEndOiling,"Failed conversion discards preparing profile and retains accepted one");
        Wheels(car).AllowApply=true;Check(MultiplayerSync.TryHostRequest(car.CarGUID,20,1,out _)&&MultiplayerSync.RevisionFor(car.CarGUID)==2&&MultiplayerSync.SettingsFor(car).bigEndOiling,"Successful reapply captures updated host settings");
        CarSpawner.Instance.AllCars.Remove(car);MultiplayerSync.Forget(car);Check(MultiplayerSync.RevisionFor(car.CarGUID)==3&&published.Count==3,"Host deletion publishes increasing tombstone revision");
        var tombstone=JsonConvert.DeserializeObject<NetworkArrangement>(MultiplayerSync.Snapshot().Single());Check(tombstone.Removed&&tombstone.SettingsJson==null,"Snapshot retains tombstone against unordered stale state");
        MultiplayerSync.Forget(car);Check(published.Count==3,"Repeated deletion cannot create duplicate tombstones");
    }
    static void ExistingOfflineProfile()
    {
        cases++;Reset();var car=Car();Core.settings.bigEndOiling=false;Check(MultiplayerSync.RequestChange(car,19,out _),"Offline geometry profile installs before session starts");
        Core.settings.bigEndOiling=true;MultiplayerSync.BeginHost();Tick();
        Check(!MultiplayerSync.SettingsFor(car).bigEndOiling&&MultiplayerSync.RevisionFor(car.CarGUID)==1,"Beginning host publishes actual offline applied profile, not edited unapplied preferences");
        var snapshot=JsonConvert.DeserializeObject<NetworkArrangement>(MultiplayerSync.Snapshot().Single());Check(!JsonConvert.DeserializeObject<Settings>(snapshot.SettingsJson).bigEndOiling,"Late-join snapshot retains the settings matching existing geometry");
        car.CarGUID=Guid.NewGuid().ToString();Tick();Check(MultiplayerSync.SettingsFor(car).bigEndOiling,"Reused native car object with a new GUID cannot inherit previous identity's applied profile");
    }
    static void HostStockRollback()
    {
        cases++;Reset();var car=Car();Core.settings.poweredTenderArrangements=new[]{10,43};MultiplayerSync.BeginHost();Check(MultiplayerSync.TryHostRequest(car.CarGUID,19,0,out _),"Host installs nonstock before rollback scenario");
        Wheels(car).AllowApply=false;Wheels(car).LastSwitchRestoredStock=true;Check(!MultiplayerSync.TryHostRequest(car.CarGUID,20,1,out _),"Native failed conversion still reports refusal");
        var state=JsonConvert.DeserializeObject<NetworkArrangement>(MultiplayerSync.Snapshot().Single());Check(state.Arrangement==10&&state.Revision==2&&!JsonConvert.DeserializeObject<Settings>(state.SettingsJson).PoweredTenderEnabled(10),"Native stock rollback publishes stock revision without powered tender instead of stale requested geometry");
        Reset();car=Car();Core.settings.poweredTenderArrangements=new[]{10};Wheels(car).AllowApply=false;Wheels(car).LastSwitchRestoredStock=true;MultiplayerSync.RequestChange(car,19,out _);Core.settings.bigEndOiling=false;MultiplayerSync.BeginHost();Tick();state=JsonConvert.DeserializeObject<NetworkArrangement>(MultiplayerSync.Snapshot().Single());
        Check(state.Arrangement==10&&!JsonConvert.DeserializeObject<Settings>(state.SettingsJson).PoweredTenderEnabled(10),"Offline failed native conversion records ordinary stock profile for later host snapshot");
    }
    static void ClientRevisionsAndSettings()
    {
        cases++;Reset();var car=Car();var other=Car();Core.settings.bigEndOiling=true;Core.settings.poweredTenderArrangements=Array.Empty<int>();
        var hostSettings=new Settings{bigEndOiling=false,show6CoupledValveGear=true,poweredTenderArrangements=new[]{43}};
        MultiplayerSync.BeginClient();MultiplayerSync.ReceiveState(Json(car.CarGUID,19,1,hostSettings));Check(MultiplayerSync.RevisionFor(car.CarGUID)==0,"Core ignores state before acceptance");
        MultiplayerSync.AcceptClient(true,"ready");MultiplayerSync.ReceiveState(Json(car.CarGUID,19,2,hostSettings));MultiplayerSync.ReceiveState(Json(car.CarGUID,20,1));Tick();
        Check(Wheels(car).currentWA==(WheelArrangementType)19&&MultiplayerSync.RevisionFor(car.CarGUID)==2&&Wheels(car).Attempts==1,"Out-of-order earlier state cannot replace newest accepted revision");
        Check(Wheels(car).SawRemote&&Wheels(car).SawBeforeSwitch&&!MultiplayerSync.ApplyingRemote,"Remote apply scopes the direct-switch authorization then clears it");
        Check(!Wheels(car).UsedSettings.bigEndOiling&&Wheels(car).UsedSettings.show6CoupledValveGear&&Wheels(car).UsedSettings.poweredTenderArrangements.SequenceEqual(new[]{43}),"Remote conversion reads host profile");
        Check(Core.settings.bigEndOiling&&!Core.settings.show6CoupledValveGear&&Core.settings.poweredTenderArrangements.Length==0,"Host profile never mutates guest global settings");
        Check(ReferenceEquals(MultiplayerSync.SettingsFor(other),Core.settings),"Host profile does not leak to second car");
        MultiplayerSync.ReceiveState(Json(car.CarGUID,20,2));Tick();Check(Wheels(car).Attempts==1,"Duplicate revision cannot convert again");
        string requestedGuid=null;int requestedId=0;long expected=-1;MultiplayerSync.SendRequest=(guid,id,revision)=>{requestedGuid=guid;requestedId=id;expected=revision;};
        Check(MultiplayerSync.RequestChange(car,47,out _)&&requestedGuid==car.CarGUID&&requestedId==47&&expected==2&&Wheels(car).Attempts==1,"Guest request includes last accepted revision without optimistic mutation");
        MultiplayerSync.ReceiveState(Json(car.CarGUID,0,3,removed:true));MultiplayerSync.ReceiveState(Json(car.CarGUID,21,2));Tick();
        Check(MultiplayerSync.RevisionFor(car.CarGUID)==3&&Wheels(car).Attempts==1&&ReferenceEquals(MultiplayerSync.SettingsFor(car),Core.settings),"Tombstone clears profile and prevents stale resurrection");
        MultiplayerSync.ReceiveState(Json(car.CarGUID,22,4));Tick();Check(Wheels(car).currentWA==(WheelArrangementType)22&&Wheels(car).Attempts==2,"Newer explicit state may supersede tombstone");
        MultiplayerSync.EndSession();Check(MultiplayerSync.RevisionFor(car.CarGUID)==0&&MultiplayerSync.Snapshot().Count()==0,"Session end clears stale states and revisions");
    }
    static void LateSpawnAndFailures()
    {
        cases++;Reset();MultiplayerSync.BeginClient();MultiplayerSync.AcceptClient(false,"locked");string guid=Guid.NewGuid().ToString();MultiplayerSync.ReceiveState(Json(guid,37,1));Tick();
        Check(MultiplayerSync.RevisionFor(guid)==1,"Snapshot may arrive before native car exists");
        var car=Car(guid,false);car.Moving=true;car.entity.IsConverted=false;Tick();Check(Wheels(car).Attempts==0,"Late native conversion waits before applying");
        car.entity.IsConverted=true;Tick();Check(Wheels(car).Attempts==0,"Nonstock waits for native mesh splitter and body");
        car.gameObject.AddComponent<S282AMeshLoader>();car.transform.MeshReady=true;car.BogiesReady=false;Tick();Check(Wheels(car).Attempts==0,"Native bogies must be initialized");
        car.BogiesReady=true;car.SimController.tractionPortsFeeder=null;Tick();Check(Wheels(car).Attempts==0,"Native traction feeder must exist before remote conversion");
        car.SimController.tractionPortsFeeder=new object();car.SimController.poweredWheels.poweredWheels=null;Tick();Check(Wheels(car).Attempts==0,"Native powered wheels array must exist before remote conversion");
        car.SimController.poweredWheels.poweredWheels=new object();car.SimController.simFlow.PortReady=false;Tick();Check(Wheels(car).Attempts==0,"Native throttle port must exist before remote conversion");
        car.SimController.simFlow.PortReady=true;Tick();Check(Wheels(car).Attempts==1&&Wheels(car).SawRemote&&Wheels(car).currentWA==(WheelArrangementType)37,"Moving late join applies through remote authorization once native objects exist");
        Tick();Check(Wheels(car).Attempts==1,"Identical received revision applies once to current native object");
        MultiplayerSync.Forget(car);CarSpawner.Instance.AllCars.Remove(car);var replacement=Car(guid);Tick();Check(Wheels(replacement).Attempts==1,"Reloaded native object receives cached accepted snapshot");
        int reports=0;MultiplayerSync.ReportFailure=message=>{reports++;Check(message.Contains("Could not synchronize"),"Replica failure reports actionable identity/status");};
        Wheels(replacement).AllowApply=false;MultiplayerSync.ReceiveState(Json(guid,45,2));Tick();int failedAttempts=Wheels(replacement).Attempts;Tick();Check(Wheels(replacement).Attempts==failedAttempts&&reports==1,"Failed conversion reports once without repeating every half second");
        Wheels(replacement).AllowApply=true;MultiplayerSync.ReceiveState(Json(guid,46,3));Tick();Check(Wheels(replacement).currentWA==(WheelArrangementType)46&&Wheels(replacement).Attempts==failedAttempts+1,"Newer valid revision can recover failed conversion");
        cases++;Reset();MultiplayerSync.BeginClient();MultiplayerSync.AcceptClient(true,"ready");var stock=Car(mesh:false);MultiplayerSync.ReceiveState(Json(stock.CarGUID,10,1,new Settings{poweredTenderArrangements=Array.Empty<int>()}));Tick();Check(Wheels(stock).Attempts==1,"Ordinary stock layout can apply without split body");
        var powered=Car(mesh:false);MultiplayerSync.ReceiveState(Json(powered.CarGUID,10,1,new Settings{poweredTenderArrangements=new[]{10}}));Tick();Check(Wheels(powered).Attempts==0,"Stock with powered tender still waits for split body");
    }
    static void InvalidStates()
    {
        cases++;Reset();var car=Car();MultiplayerSync.BeginClient();MultiplayerSync.AcceptClient(true,"ready");
        var invalid=new List<string>{null,"","{",new string('x',MultiplayerSync.MaxStateLength+1),Json("invalid",19,1),Json(car.CarGUID,48,1),Json(car.CarGUID,19,0),Json(car.CarGUID,19,-1),Json(car.CarGUID,19,1,new Settings{x42Options=(Settings.X82Options)99}),Json(car.CarGUID,19,1,new Settings{poweredTenderArrangements=new[]{36}})};
        invalid.Add(Json(car.CarGUID,19,1).TrimEnd('}')+",\"UnexpectedField\":true}");
        invalid.Add(JsonConvert.SerializeObject(new NetworkArrangement{CarGuid=car.CarGUID,Arrangement=19,Revision=1,SettingsJson="{\"unknown\":true}"}));
        invalid.Add(JsonConvert.SerializeObject(new NetworkArrangement{CarGuid=car.CarGUID,Arrangement=19,Revision=1,SettingsJson=new string('x',16385)}));
        foreach(string json in invalid){MultiplayerSync.ReceiveState(json);Check(MultiplayerSync.RevisionFor(car.CarGUID)==0,"Malformed/oversize/unknown host data cannot advance revision");}
        Tick();Check(Wheels(car).Attempts==0,"Rejected data cannot reach conversion");
        MultiplayerSync.ReceiveState(Json(car.CarGUID,19,1));Tick();Check(Wheels(car).Attempts==1,"Valid state still works after malformed messages");
        cases++;Reset();MultiplayerSync.BeginClient();MultiplayerSync.AcceptClient(true,"ready");
        for(int n=0;n<4096;n++)MultiplayerSync.ReceiveState(Json(Guid.NewGuid().ToString(),0,1,removed:true));
        string excess=Guid.NewGuid().ToString();MultiplayerSync.ReceiveState(Json(excess,0,1,removed:true));Check(MultiplayerSync.Snapshot().Count()==4096&&MultiplayerSync.RevisionFor(excess)==0,"Bounded identity cache rejects state 4097");
    }
    static int Main(string[] args)
    {
        AbsentAndDisabledFallback();OfflineAndHandshake();HostPolicyAndProfiles();ExistingOfflineProfile();HostStockRollback();ClientRevisionsAndSettings();LateSpawnAndFailures();InvalidStates();
        var report=new{passed=true,checks,cases,compiledProduction=new[]{"MultiplayerSync.cs","Settings.cs","WheelArrangement.cs","PoweredTenderPolicy.cs"},limitations="Actual core, settings and catalogue compiled against managed Unity/game/UMM boundaries. Native conversion and guard hooks are adapter doubles; this proves policy/revision/settings/queue behaviour, not live transport, game authority, driving physics or visual acceptance."};
        File.WriteAllText(args[0],System.Text.Json.JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine($"Core multiplayer validation passed {checks} checks across {cases} cases.");return 0;
    }
}
