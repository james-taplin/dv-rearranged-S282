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
    static string Json(string guid,int arrangement,long revision,Settings settings=null,bool removed=false,long epoch=0)=>JsonConvert.SerializeObject(new NetworkArrangement{Epoch=epoch>0?epoch:Math.Max(1,MultiplayerSync.SessionEpoch),CarGuid=guid,Arrangement=arrangement,Revision=revision,SettingsJson=removed?null:JsonConvert.SerializeObject(settings??new Settings()),Removed=removed});
    static NetworkArrangement[] CarStates()=>MultiplayerSync.Snapshot().Select(JsonConvert.DeserializeObject<NetworkArrangement>).Where(s=>!s.Reset).ToArray();
    static void Tick(){Time.unscaledTime+=1;MultiplayerSync.Tick();}
    static void Reset()
    {
        MultiplayerSync.EndSession();Core.settings=new Settings();CarSpawner.Instance.AllCars.Clear();ConventionalSpawn.Records.Clear();Stage2RuntimeChecks.Running=false;UnityModManagerNet.UnityModManager.CurrentMod=null;GUILayout.Labels.Clear();Time.unscaledTime+=1;
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
        cases++;Reset();string previousPath=Core.ModPath;Core.ModPath=Path.Combine(Path.GetTempPath(),"s282-missing-"+Guid.NewGuid().ToString("N"));int errors=Core.Logger.Rows.Count;int singletonReads=CarSpawner.InstanceReads;
        MultiplayerSync.Initialize();Tick();Check(!MultiplayerSync.InSession&&!MultiplayerSync.IsClient&&MultiplayerSync.IsSimulationAuthority,"Absent Multiplayer leaves the core offline even without adapter DLL");
        MultiplayerSync.Draw();Check(GUILayout.Labels.Count==0&&MultiplayerSync.Status==null,"Absent Multiplayer produces no network UI or status noise");
        Check(CarSpawner.InstanceReads==singletonReads,"Absent Multiplayer initialization does not query a native world singleton from the main menu");
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
        Check(!MultiplayerSync.TryHostRequest("not-a-guid",20,0,out _)&&!MultiplayerSync.TryHostRequest(car.CarGUID,WheelArrangement.SupportedCount,1,out _),"Invalid GUID and unknown arrangement refused");
        Check(!MultiplayerSync.TryHostRequest(Guid.NewGuid().ToString(),20,0,out _),"Unknown locomotive refused");
        Stage2RuntimeChecks.Running=true;Check(!MultiplayerSync.TryHostRequest(car.CarGUID,20,1,out _)&&Wheels(car).Attempts==attempts,"Structural run blocks concurrent host requests");Stage2RuntimeChecks.Running=false;
        Wheels(car).AllowApply=false;Check(!MultiplayerSync.TryHostRequest(car.CarGUID,20,1,out _)&&MultiplayerSync.RevisionFor(car.CarGUID)==1&&published.Count==1,"Failed host conversion does not publish or increment revision");
        Check(!MultiplayerSync.SettingsFor(car).bigEndOiling,"Failed conversion discards preparing profile and retains accepted one");
        Wheels(car).AllowApply=true;Check(MultiplayerSync.TryHostRequest(car.CarGUID,20,1,out _)&&MultiplayerSync.RevisionFor(car.CarGUID)==2&&MultiplayerSync.SettingsFor(car).bigEndOiling,"Successful reapply captures updated host settings");
        CarSpawner.Instance.AllCars.Remove(car);MultiplayerSync.Forget(car);Check(MultiplayerSync.RevisionFor(car.CarGUID)==3&&published.Count==3,"Host deletion publishes increasing tombstone revision");
        var tombstone=CarStates().Single();Check(tombstone.Removed&&tombstone.SettingsJson==null,"Snapshot retains tombstone against unordered stale state");
        MultiplayerSync.Forget(car);Check(published.Count==3,"Repeated deletion cannot create duplicate tombstones");
    }
    static void ExistingOfflineProfile()
    {
        cases++;Reset();var car=Car();Core.settings.bigEndOiling=false;Check(MultiplayerSync.RequestChange(car,19,out _),"Offline geometry profile installs before session starts");
        Core.settings.bigEndOiling=true;MultiplayerSync.BeginHost();Tick();
        Check(!MultiplayerSync.SettingsFor(car).bigEndOiling&&MultiplayerSync.RevisionFor(car.CarGUID)==1,"Beginning host publishes actual offline applied profile, not edited unapplied preferences");
        var snapshot=CarStates().Single();Check(!JsonConvert.DeserializeObject<Settings>(snapshot.SettingsJson).bigEndOiling,"Late-join snapshot retains the settings matching existing geometry");
        car.CarGUID=Guid.NewGuid().ToString();Tick();Check(MultiplayerSync.SettingsFor(car).bigEndOiling,"Reused native car object with a new GUID cannot inherit previous identity's applied profile");
    }
    static void HostStockRollback()
    {
        cases++;Reset();var car=Car();Core.settings.poweredTenderArrangements=new[]{10,43};MultiplayerSync.BeginHost();Check(MultiplayerSync.TryHostRequest(car.CarGUID,19,0,out _),"Host installs nonstock before rollback scenario");
        Wheels(car).AllowApply=false;Wheels(car).LastSwitchRestoredStock=true;Check(!MultiplayerSync.TryHostRequest(car.CarGUID,20,1,out _),"Native failed conversion still reports refusal");
        var state=CarStates().Single();Check(state.Arrangement==10&&state.Revision==2&&!JsonConvert.DeserializeObject<Settings>(state.SettingsJson).PoweredTenderEnabled(10),"Native stock rollback publishes stock revision without powered tender instead of stale requested geometry");
        Reset();car=Car();Core.settings.poweredTenderArrangements=new[]{10};Wheels(car).AllowApply=false;Wheels(car).LastSwitchRestoredStock=true;MultiplayerSync.RequestChange(car,19,out _);Core.settings.bigEndOiling=false;MultiplayerSync.BeginHost();Tick();state=CarStates().Single();
        Check(state.Arrangement==10&&!JsonConvert.DeserializeObject<Settings>(state.SettingsJson).PoweredTenderEnabled(10),"Offline failed native conversion records ordinary stock profile for later host snapshot");
    }
    static void NativeStockDiscovery()
    {
        cases++;Reset();var car=Car();Core.settings.poweredTenderArrangements=new[]{10,43};
        MultiplayerSync.BeginHost();Tick();var stock=CarStates().Single();var initial=JsonConvert.DeserializeObject<Settings>(stock.SettingsJson);
        Check(Wheels(car).Attempts==0&&!initial.PoweredTenderEnabled(10)&&initial.PoweredTenderEnabled(43),"Untouched stock discovery publishes actual unpowered tender while preserving other choices");
        Check(Core.settings.PoweredTenderEnabled(10),"Correcting the discovered car's profile never changes the host's preferences");
        Check(MultiplayerSync.TryHostRequest(car.CarGUID,10,stock.Revision,out _)&&Wheels(car).PoweredTenderSelected,"Explicit stock reapply still uses the selected host powered-tender preference");
        var powered=CarStates().Single();Check(JsonConvert.DeserializeObject<Settings>(powered.SettingsJson).PoweredTenderEnabled(10),"Explicit powered-tender reapply publishes enabled stock state");
        string guid=car.CarGUID;string first=JsonConvert.SerializeObject(stock);string second=JsonConvert.SerializeObject(powered);
        MultiplayerSync.BeginClient();MultiplayerSync.AcceptClient(true,"ready");CarSpawner.Instance.AllCars.Clear();var guest=Car(guid);
        MultiplayerSync.ReceiveState(first);Tick();Check(Wheels(guest).Attempts==1&&!Wheels(guest).PoweredTenderSelected,"Guest receives ordinary tender on first discovery");
        MultiplayerSync.ReceiveState(second);Tick();Check(Wheels(guest).Attempts==2&&Wheels(guest).PoweredTenderSelected,"Guest receives powered tender after explicit host reapply");
        Reset();car=Car();Wheels(car).PoweredTenderSelected=true;Core.settings.poweredTenderArrangements=Array.Empty<int>();MultiplayerSync.BeginHost();Tick();
        Check(JsonConvert.DeserializeObject<Settings>(CarStates().Single().SettingsJson).PoweredTenderEnabled(10),"Discovery mirrors actually selected powered tender when the current preference is off");
        Check(Core.settings.poweredTenderArrangements.Length==0,"Enabled actual tender inference also leaves personal preferences unchanged");
        Reset();var bare=new GameObject().AddComponent<TrainCar>();bare.CarGUID=Guid.NewGuid().ToString();bare.ID="bare-stock";CarSpawner.Instance.AllCars.Add(bare);
        Core.settings.poweredTenderArrangements=new[]{10,43};MultiplayerSync.BeginHost();Tick();
        Check(bare.GetComponent<WheelRearranger>()==null&&!JsonConvert.DeserializeObject<Settings>(CarStates().Single().SettingsJson).PoweredTenderEnabled(10),"Native stock with no rearranger component also advertises its ordinary tender");
    }
    static void ExplodedLateJoin()
    {
        cases++;Reset();var wreck=Car();var healthy=Car();wreck.entity.IsConverted=false;
        var explosion=wreck.gameObject.AddComponent<ExplosionModelHandler>();
        MultiplayerSync.BeginClient();MultiplayerSync.AcceptClient(true,"ready");int reports=0;MultiplayerSync.ReportFailure=_=>reports++;
        MultiplayerSync.ReceiveState(Json(wreck.CarGUID,37,1));MultiplayerSync.ReceiveState(Json(healthy.CarGUID,19,1));Tick();
        Time.unscaledTime+=80;wreck.isExploded=true;explosion.usingExplodedModel=true;Tick();
        MultiplayerSync.ReceiveState(Json(wreck.CarGUID,45,2));MultiplayerSync.ReceiveState(Json(healthy.CarGUID,20,2));
        Time.unscaledTime+=200;Tick();
        Check(reports==0&&Wheels(wreck).Attempts==0,"Exploded pending car waits without conversion or a readiness-timeout failure");
        Check(Wheels(healthy).Attempts==2&&Wheels(healthy).currentWA==(WheelArrangementType)20,"Other cars continue to synchronize while one car is exploded");
        wreck.isExploded=false;Tick();Check(Wheels(wreck).Attempts==0&&reports==0,"Replica still waits while the native exploded model remains active");
        explosion.usingExplodedModel=false;Tick();Time.unscaledTime+=118;Tick();
        Check(reports==0&&Wheels(wreck).Attempts==0,"Repair starts a fresh readiness timeout instead of counting time spent exploded");
        wreck.entity.IsConverted=true;Tick();
        Check(reports==0&&Wheels(wreck).Attempts==1&&Wheels(wreck).currentWA==(WheelArrangementType)45,"After repair, latest pending arrangement applies exactly once");
    }
    static void ReadyExplodedLateJoin()
    {
        cases++;Reset();var wreck=Car();var healthy=Car();wreck.isExploded=true;
        wreck.gameObject.AddComponent<ExplosionModelHandler>().usingExplodedModel=true;
        MultiplayerSync.BeginClient();MultiplayerSync.AcceptClient(true,"ready");int reports=0;MultiplayerSync.ReportFailure=_=>reports++;
        MultiplayerSync.ReceiveState(Json(wreck.CarGUID,37,1));MultiplayerSync.ReceiveState(Json(healthy.CarGUID,19,1));Tick();
        Check(reports==0&&Wheels(wreck).Attempts==0&&Wheels(healthy).Attempts==1,"Ready-but-exploded car is deferred while other cars synchronize");
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
        Wheels(replacement).AllowApply=true;MultiplayerSync.ReceiveState(Json(guid,46,3));Tick();Check(Wheels(replacement).Attempts==failedAttempts&&!MultiplayerSync.RequestChange(replacement,19,out _),"Failed replica remains locked until reconnect rather than continuing with inconsistent geometry");
        MultiplayerSync.BeginClient();MultiplayerSync.AcceptClient(true,"reconnected");CarSpawner.Instance.AllCars.Clear();replacement=Car(guid);MultiplayerSync.ReceiveState(Json(guid,46,3));Tick();Check(Wheels(replacement).currentWA==(WheelArrangementType)46&&Wheels(replacement).Attempts==1,"New session and native car can apply a corrected host state");
        cases++;Reset();MultiplayerSync.BeginClient();MultiplayerSync.AcceptClient(true,"ready");var stock=Car(mesh:false);MultiplayerSync.ReceiveState(Json(stock.CarGUID,10,1,new Settings{poweredTenderArrangements=Array.Empty<int>()}));Tick();Check(Wheels(stock).Attempts==1,"Ordinary stock layout can apply without split body");
        var powered=Car(mesh:false);MultiplayerSync.ReceiveState(Json(powered.CarGUID,10,1,new Settings{poweredTenderArrangements=new[]{10}}));Tick();Check(Wheels(powered).Attempts==0,"Stock with powered tender still waits for split body");
    }
    static void InvalidStates()
    {
        cases++;Reset();var car=Car();
        var invalid=new List<string>{null,"","{",new string('x',MultiplayerSync.MaxStateLength+1),Json("invalid",19,1),Json(car.CarGUID,WheelArrangement.SupportedCount,1),Json(car.CarGUID,19,0),Json(car.CarGUID,19,-1),Json(car.CarGUID,19,1,new Settings{x42Options=(Settings.X82Options)99}),Json(car.CarGUID,19,1,new Settings{poweredTenderArrangements=new[]{36}})};
        invalid.Add(Json(car.CarGUID,19,1).TrimEnd('}')+",\"UnexpectedField\":true}");
        invalid.Add(JsonConvert.SerializeObject(new NetworkArrangement{Epoch=1,CarGuid=car.CarGUID,Arrangement=19,Revision=1,SettingsJson="{\"unknown\":true}"}));
        invalid.Add(JsonConvert.SerializeObject(new NetworkArrangement{Epoch=1,CarGuid=car.CarGUID,Arrangement=19,Revision=1,SettingsJson=new string('x',16385)}));
        invalid.Add(JsonConvert.SerializeObject(new NetworkArrangement{Epoch=0,CarGuid=car.CarGUID,Arrangement=19,Revision=1,SettingsJson=JsonConvert.SerializeObject(new Settings())}));
        invalid.Add(JsonConvert.SerializeObject(new NetworkArrangement{Epoch=1,Reset=true,CarGuid=car.CarGUID}));
        foreach(string json in invalid)
        {
            MultiplayerSync.BeginClient();MultiplayerSync.AcceptClient(true,"ready");int reports=0;MultiplayerSync.ReportFailure=_=>reports++;
            MultiplayerSync.ReceiveState(json);Check(MultiplayerSync.RevisionFor(car.CarGUID)==0,"Malformed/oversize/unknown host data cannot advance revision");
            MultiplayerSync.ReceiveState(Json(car.CarGUID,19,1));Tick();Check(Wheels(car).Attempts==0&&reports==1&&!MultiplayerSync.BeforeSwitch(car,19),"Invalid authoritative data locks the replica and notifies the host exactly once");
        }
        MultiplayerSync.BeginClient();MultiplayerSync.AcceptClient(true,"reconnect");MultiplayerSync.ReceiveState(Json(car.CarGUID,19,1));Tick();Check(Wheels(car).Attempts==1,"Reconnect permits a valid fresh state after invalid data");
        cases++;Reset();MultiplayerSync.BeginClient();MultiplayerSync.AcceptClient(true,"ready");int overflows=0;MultiplayerSync.ReportFailure=_=>overflows++;
        for(int n=0;n<MultiplayerSync.MaxRetainedStates;n++)MultiplayerSync.ReceiveState(Json(Guid.NewGuid().ToString(),19,1));
        string excess=Guid.NewGuid().ToString();MultiplayerSync.ReceiveState(Json(excess,19,1));Check(CarStates().Length==MultiplayerSync.MaxRetainedStates&&MultiplayerSync.RevisionFor(excess)==0&&overflows==1,"Oversized active snapshot fails closed and reports, rather than silently leaving one car at stock");
    }
    static void NewCatalogueAndFuturePreferences()
    {
        cases++;Reset();var car=Car();Core.settings.poweredTenderArrangements=new[]{43,48,48,999,-1,36};
        var original=Core.settings.poweredTenderArrangements.ToArray();MultiplayerSync.BeginHost();string json=null;MultiplayerSync.BroadcastState=value=>json=value;
        Check(MultiplayerSync.TryHostRequest(car.CarGUID,19,0,out _),"Host accepts valid supported choice with retained newer/future settings");
        var sent=JsonConvert.DeserializeObject<NetworkArrangement>(json);var sentSettings=JsonConvert.DeserializeObject<Settings>(sent.SettingsJson);
        Check(sentSettings.poweredTenderArrangements.SequenceEqual(original.Where(PoweredTenderPolicy.Supports).Distinct()),"Transmitted profile filters unsupported future tender IDs and duplicate preferences only");
        Check(Core.settings.poweredTenderArrangements.SequenceEqual(original),"Normalization leaves host personal settings unchanged");
        Check(WheelArrangement.IsSupportedId(48)&&sentSettings.PoweredTenderEnabled(48),"Latest normal Express48 catalogue and tender preference remain valid");
        var guid=car.CarGUID;MultiplayerSync.BeginClient();MultiplayerSync.AcceptClient(true,"ready");CarSpawner.Instance.AllCars.Clear();car=Car(guid);int failures=0;MultiplayerSync.ReportFailure=_=>failures++;
        MultiplayerSync.ReceiveState(json);Tick();Check(Wheels(car).currentWA==(WheelArrangementType)19&&MultiplayerSync.RevisionFor(guid)==1&&failures==0,"Guest applies host state containing supported Express settings instead of silently remaining stock");
    }
    static void OptionalFailureRecoveryAndTargets()
    {
        cases++;Reset();string path=Core.ModPath;Core.ModPath=Path.Combine(Path.GetTempPath(),"s282-no-adapter-"+Guid.NewGuid().ToString("N"));
        UnityModManagerNet.UnityModManager.CurrentMod=new UnityModManagerNet.UnityModManager.ModEntry{Active=true};MultiplayerSync.Initialize();Check(MultiplayerSync.IsClient&&!MultiplayerSync.IsSimulationAuthority,"Failed optional startup locks uncertain multiplayer integration");
        for(int attempt=0;attempt<15;attempt++){Time.unscaledTime+=2;Tick();}
        Check(MultiplayerSync.Status.Contains("RearrangedS282.Multiplayer.dll")&&!MultiplayerSync.Status.Contains("complete matching test package"),"Exhausted missing-adapter retries identify the missing DLL rather than imply a peer mismatch");
        var describe=typeof(MultiplayerSync).GetMethod("BridgeFailureReason",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
        string Reason(Exception error)=>(string)describe.Invoke(null,new object[]{error});
        Check(Reason(new System.Reflection.TargetInvocationException(new InvalidOperationException("Multiplayer API is not ready."))).Contains("API is not ready"),"API readiness failures are unwrapped and distinguished from missing package files");
        Check(Reason(new FileNotFoundException("dependency missing","MultiplayerAPI, Version=1.1.0.0")).Contains("MultiplayerAPI.dll is unavailable"),"Missing native API points to Multiplayer installation rather than a peer mismatch");
        Check(Reason(new TypeInitializationException("bridge",new MissingMethodException("Missing native signature"))).Contains("MissingMethodException: Missing native signature"),"API signature failures retain their concrete root cause");
        Check(Reason(new Exception(new string('a',400)+"\nend")).Length<250&&!Reason(new Exception("a\nb")).Contains("\n"),"Startup UI bounds and flattens exception text while full exceptions remain logged");
        UnityModManagerNet.UnityModManager.CurrentMod.Active=false;Tick();MultiplayerSync.Draw();Check(!MultiplayerSync.InSession&&MultiplayerSync.IsSimulationAuthority&&MultiplayerSync.Status==null&&GUILayout.Labels.Count==0,"Disabling failed optional integration restores silent single player");
        UnityModManagerNet.UnityModManager.CurrentMod.Active=true;MultiplayerSync.Initialize();Check(MultiplayerSync.IsClient,"Reenabling can retry an earlier failed optional startup");
        UnityModManagerNet.UnityModManager.CurrentMod=null;Tick();Core.ModPath=path;
        var unique=Car();unique.uniqueCar=true;var foreign=Car();foreign.NormalLivery=false;
        Check(!MultiplayerSync.RequestChange(unique,19,out _)&&!MultiplayerSync.RequestChange(foreign,19,out _)&&Wheels(unique).Attempts==0&&Wheels(foreign).Attempts==0,"Core rejects unique/foreign heavy steam through native target policy");
    }
    static void EpochCompactionAndReordering()
    {
        cases++;Reset();MultiplayerSync.BeginHost();var live=Car();Check(MultiplayerSync.TryHostRequest(live.CarGUID,19,0,out _),"Keep one live car while deletion history compacts");
        long originalEpoch=MultiplayerSync.SessionEpoch;string old=MultiplayerSync.SnapshotFor(live.CarGUID).Single();
        for(int n=0;n<MultiplayerSync.MaxRetainedTombstones*20;n++)
        {
            var deleted=Car();MultiplayerSync.OnApplied(deleted,10);CarSpawner.Instance.AllCars.Remove(deleted);MultiplayerSync.Forget(deleted);
        }
        Check(MultiplayerSync.SessionEpoch>originalEpoch&&CarStates().Length==1,"More than old4096 historical deletions retain only live state and bounded epoch tombstones");
        var snapshot=MultiplayerSync.Snapshot().ToArray();var marker=snapshot.First();var current=MultiplayerSync.SnapshotFor(live.CarGUID).Single();long nextEpoch=MultiplayerSync.SessionEpoch;
        Check(snapshot.Length==2&&JsonConvert.DeserializeObject<NetworkArrangement>(marker).Reset,"Compact snapshot has epoch marker and one live state");
        string guid=live.CarGUID;MultiplayerSync.BeginClient();MultiplayerSync.AcceptClient(true,"ready");CarSpawner.Instance.AllCars.Clear();var guest=Car(guid);
        MultiplayerSync.ReceiveState(current);MultiplayerSync.ReceiveState(marker);MultiplayerSync.ReceiveState(old);Tick();
        Check(MultiplayerSync.SessionEpoch==nextEpoch&&Wheels(guest).currentWA==(WheelArrangementType)19&&Wheels(guest).Attempts==1,"State overtaking reset applies once; late reset and older epoch cannot erase/resurrect state");
        MultiplayerSync.ReceiveState(marker);MultiplayerSync.ReceiveState(current);Tick();Check(Wheels(guest).Attempts==1,"Duplicate epoch marker/snapshot is idempotent");
        MultiplayerSync.ReceiveState(Json(Guid.NewGuid().ToString(),0,1,removed:true,epoch:originalEpoch));Check(CarStates().Length==1,"Old-epoch tombstone cannot consume new snapshot cache");
        MultiplayerSync.BeginHost();long newSession=MultiplayerSync.SessionEpoch;Check(newSession>nextEpoch,"New host session receives a fresh monotonically advancing epoch");
    }
    static void ReadinessTimeout()
    {
        cases++;Reset();MultiplayerSync.BeginClient();MultiplayerSync.AcceptClient(true,"ready");string guid=Guid.NewGuid().ToString();int reports=0;MultiplayerSync.ReportFailure=_=>reports++;
        MultiplayerSync.ReceiveState(Json(guid,19,1));Time.unscaledTime+=MultiplayerSync.ReplicaReadyTimeout*4;Tick();Check(reports==0&&MultiplayerSync.IsClient,"Legitimately unspawned distant car does not start readiness timeout");
        var car=Car(guid,false);Tick();Time.unscaledTime+=MultiplayerSync.ReplicaReadyTimeout+1;Tick();Tick();Check(reports==1&&Wheels(car).Attempts==0&&!MultiplayerSync.BeforeSwitch(car,19),"Present car with permanently missing native gear reports once and locks instead of waiting indefinitely");
    }
    static void RuntimeAssetIdentity()
    {
        cases++;Reset();string previousPath=Core.ModPath;string previousVersion=Core.RunningVersion;
        string directory=Path.Combine(Path.GetTempPath(),"s282-runtime-identity-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Path.Combine(directory,"assets"));Core.ModPath=directory;Core.RunningVersion="identity-test";
        var cache=typeof(MultiplayerSync).GetField("buildIdentity",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
        string[] paths={"RearrangedS282.Multiplayer.dll","assets/s282_drivetrain","assets/franklin_casing.obj","assets/steam_manifold.obj","assets/steam_transfer.obj"};
        try
        {
            for(int i=0;i<paths.Length;i++)File.WriteAllBytes(Path.Combine(directory,paths[i]),new[]{(byte)i,(byte)17});
            cache.SetValue(null,null);string baseline=MultiplayerSync.BuildIdentity;Check(baseline.StartsWith("identity-test:")&&baseline.Length<512,"Compact runtime identity fits the bounded handshake");
            foreach(string relative in paths)
            {
                string file=Path.Combine(directory,relative);byte[] original=File.ReadAllBytes(file);File.WriteAllBytes(file,new byte[]{99,21});cache.SetValue(null,null);
                Check(MultiplayerSync.BuildIdentity!=baseline,"Replacing runtime payload changes handshake identity: "+relative);
                File.WriteAllBytes(file,original);
            }
            File.WriteAllText(Path.Combine(directory,"Settings.xml"),"different personal settings");File.WriteAllText(Path.Combine(directory,"README.txt"),"different docs");cache.SetValue(null,null);
            Check(MultiplayerSync.BuildIdentity==baseline,"Personal settings and documents do not split compatible runtime peers");
            File.Delete(Path.Combine(directory,"assets/s282_drivetrain"));cache.SetValue(null,null);Check(MultiplayerSync.BuildIdentity!=baseline,"Missing required runtime asset cannot match a complete package");
        }
        finally
        {
            Core.ModPath=previousPath;Core.RunningVersion=previousVersion;cache.SetValue(null,null);
            // Delete only explicitly created fixture files and their dedicated
            // directory. No recursive operation touches user mod data.
            foreach(string relative in paths.Concat(new[]{"Settings.xml","README.txt"})){string file=Path.Combine(directory,relative);if(File.Exists(file))File.Delete(file);}
            Directory.Delete(Path.Combine(directory,"assets"));Directory.Delete(directory);
        }
    }
    static void NativeDeletionLifecycle()
    {
        cases++;Reset();var car=Car();MultiplayerSync.BeginHost();Tick();var publications=new List<string>();MultiplayerSync.BroadcastState=publications.Add;
        var installed=(System.Collections.IDictionary)typeof(MultiplayerSync).GetField("lastApplied",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).GetValue(null);
        Check(installed.Contains(car)&&MultiplayerSync.RevisionFor(car.CarGUID)==1,"Host stock baseline is tracked even without a conversion callback");
        CarSpawner.Instance.Delete(car);
        Check(!installed.Contains(car)&&CarStates().Single().Removed&&MultiplayerSync.RevisionFor(car.CarGUID)==2,"Native unpooled deletion forgets its profile and publishes a tombstone before destruction");
        MultiplayerSync.Forget(car);Check(publications.Count==1,"Later pool/OnDestroy cleanup is idempotent after the native deletion event");
        MultiplayerSync.EndSession();var offline=Car();Check(MultiplayerSync.RequestChange(offline,19,out _)&&installed.Contains(offline),"Offline applied profile is retained while its car lives");
        CarSpawner.Instance.Delete(offline);Check(!installed.Contains(offline),"Offline world unload also releases the installed profile");
        var oldSpawner=CarSpawner.Instance;var oldCar=Car();Tick();CarSpawner.Instance=new CarSpawner();var replacement=Car(oldCar.CarGUID);MultiplayerSync.BeginHost();Tick();
        oldSpawner.Delete(oldCar);
        Check(CarStates().Length==1&&!CarStates().Single().Removed&&MultiplayerSync.RevisionFor(replacement.CarGUID)==1,"A replaced native singleton is unsubscribed before old-world deletion can affect the current GUID");
        CarSpawner.Instance.Delete(replacement);Check(CarStates().Single().Removed,"The replacement singleton is observed for subsequent native deletions");
    }
    static int Main(string[] args)
    {
        if(args.Length>1){if(args[1]=="stock")NativeStockDiscovery();else if(args[1]=="wreck-timeout")ExplodedLateJoin();else if(args[1]=="wreck-ready")ReadyExplodedLateJoin();else throw new ArgumentException("Unknown isolated case");return 0;}
        AbsentAndDisabledFallback();OfflineAndHandshake();HostPolicyAndProfiles();ExistingOfflineProfile();HostStockRollback();NativeStockDiscovery();ExplodedLateJoin();ReadyExplodedLateJoin();ClientRevisionsAndSettings();LateSpawnAndFailures();InvalidStates();NewCatalogueAndFuturePreferences();OptionalFailureRecoveryAndTargets();EpochCompactionAndReordering();ReadinessTimeout();RuntimeAssetIdentity();NativeDeletionLifecycle();
        var report=new{passed=true,checks,cases,compiledProduction=new[]{"MultiplayerSync.cs","Settings.cs","WheelArrangement.cs","PoweredTenderPolicy.cs"},limitations="Actual core, settings and catalogue compiled against managed Unity/game/UMM boundaries. Native conversion and guard hooks are adapter doubles; this proves policy/revision/settings/queue behaviour, not live transport, game authority, driving physics or visual acceptance."};
        File.WriteAllText(args[0],System.Text.Json.JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine($"Core multiplayer validation passed {checks} checks across {cases} cases.");return 0;
    }
}
