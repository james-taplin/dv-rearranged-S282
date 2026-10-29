using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RearrangedS282;
using UnityEngine;

internal static class Probe {
 internal static int Checks, ParentAttempts, Rebuilds, StopReads, ActiveChangeDepth;
 internal static readonly List<string> Events=new();
 internal static void Check(bool value,string label){Checks++;if(!value)throw new Exception(label);}
 static void Main(string[] args){
  // Reproduce Unity's reported activation constraint, rather than letting a
  // permissive scene stub hide the original forbidden SetParent operation.
  var old=new GameObject("legacy leaf");ActiveChangeDepth=1;bool rejected=false;
  try{old.transform.SetParent(null,true);}catch(InvalidOperationException){rejected=true;}
  finally{ActiveChangeDepth=0;ParentAttempts=0;}
  Check(rejected,"fixture rejects the original reparent-during-deactivation operation");
  for(int cycle=0;cycle<100;cycle++)foreach(bool pooled in new[]{false,true}){
   var host=new TrainCar();var root=new GameObject("tender engine");var copy=new GameObject("owned tender effects");
   var audio=new AudioSource();var particle=new ParticleSystem();copy.Parts.Add(audio);copy.Parts.Add(particle);copy.transform.parent=root.transform;
   var presentation=new ErieTenderPresentation();presentation.Prepare();
   var w=new WheelRearranger();w.Prepare(root,presentation,host);Events.Clear();
   ArticulatedEffects.CurrentCopy=copy;ArticulatedEffects.CurrentPresentation=presentation;
   presentation.Releasing=()=>ArticulatedEffects.TenderChanged(host);
   root.OnDisable=presentation.Disable;
   int beforeReads=StopReads,beforeBuilds=Rebuilds;
   if(pooled){root.SetActive(false);Check(!copy.activeSelf&&copy.destroyed,"pool disable stops and queues owned clones");Check(presentation.Outlet!=null,"pool disable retains reusable presentation outlet");}
   w.Detach();
   Check(ParentAttempts==0,"cleanup never reparents owned clones during switch or pool disable");
   Check(audio.Stopped&&particle.Stopped&&copy.destroyed&&!copy.activeSelf,"audio and particles stop before owned destruction");
   Check(copy.transform.parent==root.transform,"queued clone stays in its owned hierarchy");
   Check(StopReads>beforeReads,"native reader cleanup callback remains active");
   Check(Rebuilds==beforeBuilds+(pooled?2:1),"one explicit detach refresh; pooling callback remains functional");
   Check(Events.IndexOf("unbind")<Events.LastIndexOf("root disabled"),"explicit unbind precedes root deactivation");
   Check(!ArticulatedEffects.CapturedOutletDuringRelease,"release clears outlet before refresh can select tender");
   Check(w.HiddenRestored&&w.RootCleared&&w.RefreshCount==1,"native rolling pieces restored and tender state cleared");
   Check(presentation.Outlet==null&&presentation.NativeLodRetained&&presentation.MeshDestroyed,"release preserves native LOD entries and releases owned mesh");
   int prior=StopReads;presentation.Release();presentation.Destroyed();
   Check(StopReads==prior,"presentation release/destroy is idempotent");
   w.Detach();Check(w.RootCleared,"repeat detach is safe after queued destruction");
  }
  // An attach that failed before presentation initialization still unbinds.
  var failed=new WheelRearranger();failed.Prepare(new GameObject("partial"),null,new TrainCar());
  int count=StopReads;failed.Detach();Check(StopReads==count+1&&failed.RootCleared,"partial attach without presentation cleans native readers");
  File.WriteAllText(args[0],JsonSerializer.Serialize(new{passed=true,checks=Checks,cycles=200,scope="Extracted production clone destruction, presentation Release/OnDisable/OnDestroy and DetachTenderEngine; activation/deferred-destruction stand-ins. Actual Unity callbacks and audible/VFX output require a fresh game run."},new JsonSerializerOptions{WriteIndented=true}));
  Console.WriteLine($"Tender effect cleanup: {Checks} checks passed");
 }
}
namespace UnityEngine {
 internal class Object {internal bool destroyed;public static void Destroy(Object value){if(value!=null){value.destroyed=true;Probe.Events.Add("destroy");}}}
 internal sealed class Transform:Object {internal GameObject gameObject;internal Transform parent;internal void SetParent(Transform value,bool preserve){Probe.ParentAttempts++;if(Probe.ActiveChangeDepth>0)throw new InvalidOperationException("Cannot set parent during activation change");parent=value;}}
 internal sealed class GameObject:Object {
  internal readonly Transform transform;internal bool activeSelf=true;internal Action OnDisable;internal readonly List<object> Parts=new();
  internal GameObject(string name){transform=new Transform{gameObject=this};}
  internal T[] GetComponentsInChildren<T>(bool inactive)=>Parts.OfType<T>().ToArray();
  internal void SetActive(bool active){if(activeSelf==active)return;activeSelf=active;Probe.Events.Add("root disabled");Probe.ActiveChangeDepth++;try{if(!active)OnDisable?.Invoke();}finally{Probe.ActiveChangeDepth--;}}
 }
 internal sealed class AudioSource {internal bool Stopped;internal void Stop(){Stopped=true;Probe.Events.Add("audio stop");}}
 internal enum ParticleSystemStopBehavior {StopEmittingAndClear}
 internal sealed class ParticleSystem {internal bool Stopped;internal void Stop(bool children,ParticleSystemStopBehavior behavior){Stopped=true;Probe.Events.Add("particle stop");}}
 internal class Renderer:Object{}
 internal sealed class MeshRenderer:Renderer{}
 internal sealed class Mesh:Object{}
 internal struct LOD {internal float screenRelativeTransitionHeight,fadeTransitionWidth;internal Renderer[] renderers;internal LOD(float h,Renderer[] r){screenRelativeTransitionHeight=h;renderers=r;fadeTransitionWidth=0;}}
 internal sealed class LODGroup {internal LOD[] Levels;internal LOD[] GetLODs()=>Levels;internal void SetLODs(LOD[] levels)=>Levels=levels;internal void RecalculateBounds(){}}
}
namespace RearrangedS282 {
 internal sealed class TrainCar {internal string ID="tender";}
 internal static class Main {internal static readonly FakeLogger Logger=new();internal sealed class FakeLogger{internal void Log(string value){}}}
 internal static partial class ArticulatedEffects {
  internal static GameObject CurrentCopy;internal static ErieTenderPresentation CurrentPresentation;internal static bool CapturedOutletDuringRelease;
  internal static void TenderChanged(TrainCar car){Probe.StopReads++;Probe.Events.Add("unbind");if(CurrentCopy!=null&&!CurrentCopy.destroyed)RemoveCopy(CurrentCopy);if(CurrentPresentation!=null&&CurrentPresentation.Releasing==null)CapturedOutletDuringRelease=CurrentPresentation.Outlet!=null;Probe.Rebuilds++;}
 }
 internal sealed partial class ErieTenderPresentation {
  internal Transform Outlet;internal Action Releasing;private Mesh mesh;private MeshRenderer pipe;private LODGroup lod,originalLod;private readonly Renderer native=new();private Mesh ownedMesh;
  internal void Prepare(){Outlet=new GameObject("outlet").transform;mesh=ownedMesh=new Mesh();pipe=new MeshRenderer();lod=originalLod=new LODGroup{Levels=new[]{new LOD(.5f,new Renderer[]{native,pipe})}};}
  internal bool NativeLodRetained=>originalLod.Levels[0].renderers.SequenceEqual(new[]{native});internal bool MeshDestroyed=>ownedMesh.destroyed;
 }
 internal sealed class Polish{internal void Release(){}}
 internal sealed class Sim{internal void SetTenderEngine(bool active){}}
 internal partial class WheelRearranger {
  private Polish tenderPolish;private Sim articulatedSimulation=new();private readonly List<GameObject> tenderHidden=new();private Transform tenderEngineRoot,tenderDriveL,tenderDriveR;private ErieTenderPresentation tenderPresentation;private TrainCar tenderEngineCar;
  private float tenderForceN,tenderLimitN,tenderDemandN;private bool tenderSlipping;internal int RefreshCount;private TrainCar host;
  internal bool HiddenRestored=>tenderHidden.Count==0;internal bool RootCleared=>tenderEngineRoot==null&&tenderEngineCar==null&&tenderDriveL==null&&tenderDriveR==null&&tenderForceN==0f&&tenderLimitN==0f&&tenderDemandN==0f&&!tenderSlipping;
  internal void Prepare(GameObject root,ErieTenderPresentation p,TrainCar car){tenderEngineRoot=root.transform;tenderPresentation=p;tenderEngineCar=car;host=car;var native=new GameObject("native wheel");native.SetActive(false);tenderHidden.Add(native);}
  private T GetComponent<T>() where T:class=>host as T;private void RefreshSteamConnectionsForTender(){RefreshCount++;}
 }
}
