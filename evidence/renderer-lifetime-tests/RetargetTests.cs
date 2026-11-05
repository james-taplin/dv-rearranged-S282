using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object=UnityEngine.Object;
namespace RearrangedS282 {
 // Restore is extracted from production. These fields supply its narrow boundary.
 internal partial class RetargetFixture {
  private sealed class Entry {internal MeshFilter Filter;internal Mesh OriginalMesh,EditedMesh;internal Transform Proxy;internal MeshRenderer ProxyRenderer;}
  private sealed class OilPoint {internal Transform Provider;internal Vector3 NativeLocal;}
  private readonly List<Entry> entries=new List<Entry>();
  private readonly List<OilPoint> oilPoints=new List<OilPoint>();
  private readonly List<(LODGroup group,LOD[] lods,Vector3 reference,float size)> lodBackups=new List<(LODGroup,LOD[],Vector3,float)>();
  private bool precisionRods,endpointBounds;
  internal Action<IEnumerable<Renderer>> Retire;
  internal void Add(MeshRenderer renderer)=>entries.Add(new Entry{Proxy=renderer.transform,ProxyRenderer=renderer});
  internal void Run()=>Restore();
 }
 internal static class RetargetTests {
  private static void Check(bool ok,string label){if(!ok)throw new Exception(label);Console.WriteLine("PASS "+label);}
  private static MeshRenderer Add(Transform parent,Material mat){var go=new GameObject();go.transform.parent=parent;var r=go.Add<MeshRenderer>();r.sharedMaterial=mat;return r;}
  internal static int Run(string mode){
   var root=new GameObject();var car=root.Add<TrainCar>();var h=root.Add<ExplosionModelHandler>();
   var body=new GameObject();body.transform.parent=root.transform;var normal=new Material();var native=Add(body.transform,normal);
   var stock=new ExplosionModelHandler.MaterialSwapData{swapMaterial=new Material(),affectedGameObjects=new[]{body}};
   h.materialSwaps=new[]{stock};h.gameObjectsToDisable=Array.Empty<GameObject>();h.gameObjectSwaps=Array.Empty<ExplosionModelHandler.GameObjectSwapData>();h.Awake();
   var owner=new B99ExplosionMaterials(car,body.transform);var proxy=Add(body.transform,normal);
   owner.Refresh(root.GetComponentsInChildren<MeshRenderer>(true));
   if(mode!="retargetfresh"){h.HandleExplosionModelChange();h.RevertToUnexplodedModel();}
   var fixture=new RetargetFixture();fixture.Add(proxy);int calls=0;bool alive=false;
   fixture.Retire=renderers=>{calls++;alive=proxy.gameObject.activeSelf;owner.Forget(renderers);};
   var route=new RouteFixture(car,null,fixture.Run);
   if(mode=="retargetpool")Check(route.PoolReturn(),"Actual pool route returns successfully");
   else if(mode=="retargetreapply")fixture.Run();
   else route.SwitchToStock();
   fixture.Run();Check(calls==1&&alive&&!proxy.gameObject.activeSelf,"Retarget cleanup retires once before deferred destruction");
   owner.Refresh(root.GetComponentsInChildren<MeshRenderer>(true));
   Check(!h.materialSwaps.Any(s=>s.affectedRenderers!=null&&s.affectedRenderers.Any(p=>ReferenceEquals(p.renderer,proxy))),"No retired retarget proxy survives same-frame inactive refresh or native cache");
   Object.Flush();h.HandleExplosionModelChange();h.RevertToUnexplodedModel();
   Check(ReferenceEquals(native.sharedMaterial,normal),"Native explosion and repair succeed after retarget destruction");
   var next=Add(body.transform,normal);owner.Refresh(new Renderer[]{next});h.HandleExplosionModelChange();
   Check(ReferenceEquals(next.sharedMaterial,stock.swapMaterial),"Replacement presentation burns");
   h.RevertToUnexplodedModel();Check(ReferenceEquals(next.sharedMaterial,normal),"Replacement presentation repairs");return 0;
  }
 }
}
