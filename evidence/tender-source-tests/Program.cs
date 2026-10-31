using System;using System.Linq;using System.Collections.Generic;using System.IO;using System.Text.Json;using UnityEngine;
using Object=UnityEngine.Object;
namespace RearrangedS282 {
 internal enum WheelArrangementType{s28882=43}
 internal static class PacificHudsonAffine {
  internal static bool TryDecompose(Matrix4x4 m,Vector3 sign,out Vector3 p,out Quaternion r,out Vector3 s){p=m.MultiplyPoint3x4(Vector3.zero);r=Quaternion.identity;s=sign;return true;}
 }
 internal partial class WheelRearranger {
  private sealed class BigMalletLayout{internal Matrix4x4 Rear=Matrix4x4.TRS(new(0,0,-.25f),Quaternion.identity,Vector3.one);}
  private sealed class Parts{internal List<Transform> Statics=new();}
  private BigMalletLayout tenderLayout;private float tenderNativeCylinderFront;
  private Transform tenderSourceRoot,tenderSourceL,tenderSourceR,bigMalletRearL,bigMalletRearR;
  private readonly Dictionary<Transform,Matrix4x4> tenderStaticMaps=new();
  private readonly Dictionary<WheelArrangementType,Parts> bigMalletStatics=new();
  private readonly List<Transform> articulatedAssets=new();
  private readonly List<Mesh> articulatedMeshes=new();
  private static readonly string[] ArticulatedSplitFrames={"left bar frame","right bar frame"};
  private readonly Animator lfAnimator=new GameObject("left animator").AddComponent<Animator>(),rfAnimator=new GameObject("right animator").AddComponent<Animator>();
  private readonly Transform body=new GameObject("loco").transform;
  private Transform nativeL,nativeR,splitLoco;
  private Transform MalletBody=>body;
  private Transform transform=>body;
  internal bool FailClone,EmptyFrame;internal int Builds;
  private void EnsureArticulatedAssets(){
   if(nativeL!=null)return;
   nativeL=body.Add("native L");nativeR=body.Add("native R");
   splitLoco=body.Add("split loco");
   for(int side=0;side<ArticulatedSplitFrames.Length;side++){
    var frame=splitLoco.Add(ArticulatedSplitFrames[side]);frame.localPosition=new(side==0?-.4f:.4f,0,0);frame.gameObject.layer=7;
    var mesh=new Mesh{name="native frame "+side,vertices=EmptyFrame?Array.Empty<Vector3>():new[]{new Vector3(0,0,0),new Vector3(0,1,10),new Vector3(0,0,10)}};
    mesh.SetTriangles(EmptyFrame?new List<int>():new List<int>{0,1,2},0);
    frame.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;frame.gameObject.AddComponent<MeshRenderer>().sharedMaterials=new[]{new Material{name="native frame material "+side}};
   }
   foreach(var drive in new[]{nativeL,nativeR}){
    drive.Add("wheel").localPosition=new(0,.712f,8.0018787f);
    foreach(string name in new[]{"franklin_type_b(Clone)","franklin_type_b_4444(Clone)"}){
     var part=drive.Add(name);part.Add("cylinder").gameObject.AddComponent<MeshFilter>().sharedMesh=new Mesh{name="Franklin cylinder"};articulatedAssets.Add(part);
    }
   }
  }
  private float MeasureNativeCylinderFront()=>10.22f;
  private float NativeDriverZ(int axle)=>10f-2f*axle;
  private static Transform RequireArticulatedPart(Transform root,string name)=>root.Find(name)??throw new InvalidOperationException(name);
  private void MapMalletPart(Transform part,Matrix4x4 map){part.localPosition=map.MultiplyPoint3x4(part.localPosition);}
  private BigMalletLayout MeasureBigMallet(WheelArrangementType kind)=>new();
  private void EnsureBigMalletAssets(BigMalletLayout layout){if(Builds==0)RegenerateHost();}
  private void EnableGeneratedRenderers(Transform t){}
  private Transform CloneEngineDrive(Transform source,Animator animator,string name,Transform parent,Action<Transform> configure){if(FailClone)throw new InvalidOperationException("injected clone failure");var clone=Object.Instantiate(source.gameObject,parent).transform;clone.name=name;configure(clone);return clone;}
  internal void RegenerateHost(){
   if(bigMalletStatics.TryGetValue(WheelArrangementType.s28882,out var old))foreach(var t in old.Statics)Object.Destroy(t.gameObject);
   Builds++;var parts=new Parts();
   for(int i=0;i<6;i++){var t=body.Add("source "+i);t.localPosition=new(i*.15f,.7f,4f-i*.5f+Builds);t.localScale=new(1,.709f,.709f);t.gameObject.AddComponent<MeshFilter>().sharedMesh=new Mesh{name="authored mesh "+i};parts.Statics.Add(t);}
   bigMalletStatics[WheelArrangementType.s28882]=parts;
   if(bigMalletRearL==null){bigMalletRearL=Object.Instantiate(nativeL.gameObject,body).transform;bigMalletRearR=Object.Instantiate(nativeR.gameObject,body).transform;}
   bigMalletRearL.Find("wheel").localPosition=new(0,.712f,8.0018787f+Builds);
  }
  internal void Ensure()=>EnsurePoweredTenderSource();
  internal Dictionary<Transform,Matrix4x4> Captured=>tenderStaticMaps;
  internal Transform SourceRoot=>tenderSourceRoot;
  internal Transform SourceL=>tenderSourceL;
  internal Transform[] SharedDrives=>new[]{bigMalletRearL,bigMalletRearR};
  internal Transform[] Shared=>bigMalletStatics[WheelArrangementType.s28882].Statics.ToArray();
  internal Transform[] NativeFrames=>ArticulatedSplitFrames.Select(name=>splitLoco.Find(name)).ToArray();
  internal Mesh[] FrameMeshes=>articulatedMeshes.ToArray();
  internal Transform Attach(){var t=new GameObject("attached tender engine").transform;CloneTenderStatics(t,Matrix4x4.TRS(new(0,0,-7),Quaternion.identity,Vector3.one));return t;}
 }
}
internal static class Program {
 static int checks;static void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
 static void Main(string[] args){
  var owner=new RearrangedS282.WheelRearranger();owner.Ensure();
  var sources=owner.Captured.ToArray();var originalWheel=owner.SourceL.Find("wheel").localPosition;
  Check(sources.Length==8,"All six static source parts and both native frame slices retained");Check(!owner.SourceRoot.gameObject.activeInHierarchy,"Template root hidden");
  var frames=sources.Where(x=>x.Key.name.StartsWith("[Rearranged tender bar frame]")).Select(x=>x.Key).ToArray();
  Check(frames.Length==2&&owner.FrameMeshes.Length==2,"Real frame-source helper owns one final clipped mesh per side");
  for(int side=0;side<2;side++){
   var frame=frames[side];var native=owner.NativeFrames[side];var mesh=frame.GetComponent<MeshFilter>().sharedMesh;
   Check(mesh.vertexCount>0&&mesh.vertices.All(v=>v.z>=1.6f-.0001f&&v.z<=8.4f+.0001f),"Actual frame clipper retains only the native driver span");
   Check(!ReferenceEquals(mesh,native.GetComponent<MeshFilter>().sharedMesh),"Frame source owns a new mesh rather than changing the native frame");
   Check(frame.gameObject.layer==native.gameObject.layer&&ReferenceEquals(frame.GetComponent<MeshRenderer>().sharedMaterials,native.GetComponent<MeshRenderer>().sharedMaterials),"Frame source retains native layer and material references");
  }
  Check(sources.All(x=>!owner.Shared.Contains(x.Key)),"Tender templates independent of disposable shared statics");
  foreach(var drive in owner.SharedDrives.Concat(new[]{owner.SourceL}))foreach(string variant in new[]{"franklin_type_b(Clone)","franklin_type_b_4444(Clone)"})
   Check(!drive.Find(variant).gameObject.activeSelf,"Source preparation must not copy active Franklin cylinders into Walschaerts drives");
  var visibleDrive=Object.Instantiate(owner.SourceL.gameObject,new GameObject("visible powered tender").transform);visibleDrive.SetActive(true);
  Check(visibleDrive.GetComponentsInChildren<MeshFilter>(false).Length==0,"Activating tender drivetrain must not expose extra Franklin cylinders");
  // The old transform-key scheme fails after the host cache rebuilds.
  var legacy=owner.Shared.ToDictionary(t=>t,t=>t.localToWorldMatrix);owner.RegenerateHost();
  Check(owner.Shared.All(t=>!legacy.ContainsKey(t)),"Reproduce original cache-key loss after host regeneration");
  for(int cycle=0;cycle<40;cycle++){
   owner.RegenerateHost();owner.Ensure();var root=owner.Attach();
   Check(root.childCount==sources.Length,"Return to Erie reconstructs every static part");
   Check(owner.SourceRoot.childCount==sources.Length+2,"Repeated switches do not duplicate templates");
   Check(owner.SourceL.Find("wheel").localPosition.Equals(originalWheel),"Host edits cannot mutate frozen driver template");
   for(int i=0;i<sources.Length;i++){
    Check(!sources[i].Key.destroyed&&!sources[i].Key.gameObject.destroyed,"Host disposal cannot destroy tender template");
    var expected=(Matrix4x4.TRS(new(0,0,-7),Quaternion.identity,Vector3.one)*sources[i].Value).MultiplyPoint3x4(Vector3.zero);
    Check((root.GetChild(i).localPosition-expected).magnitude<.0001f,"Frozen tender body pose survives repeated host switches");
    Check(root.GetChild(i).gameObject.activeInHierarchy,"Cloned tender part visible");
    Check(ReferenceEquals(root.GetChild(i).GetComponent<MeshFilter>().sharedMesh,sources[i].Key.GetComponent<MeshFilter>().sharedMesh),"Cloned tender static retains its frozen mesh reference");
   }
   Object.Destroy(root.gameObject);
  }
  var retry=new RearrangedS282.WheelRearranger{FailClone=true};bool failed=false;try{retry.Ensure();}catch(InvalidOperationException){failed=true;}
  Check(failed&&retry.SourceRoot==null&&retry.Captured.Count==0,"Partial source construction cleans up");retry.FailClone=false;retry.Ensure();Check(retry.Captured.Count==8,"Source construction can retry after cleanup");
  var empty=new RearrangedS282.WheelRearranger{EmptyFrame=true};failed=false;try{empty.Ensure();}catch(InvalidOperationException ex){failed=ex.Message.Contains("slice is empty");}
  Check(failed&&empty.SourceRoot==null&&empty.Captured.Count==0,"Empty native frame slice rejects preparation and clears partial template cache");
  var report=new{passed=true,checks,cycles=40,limitation="Actual extracted source preparation, frame-source builder, frame clipper and static clone methods; managed clone/transform/material/mesh storage boundaries. Does not execute Unity deferred destruction, Animator or full tender attachment."};
  File.WriteAllText(args[0],JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(report));
 }
}
