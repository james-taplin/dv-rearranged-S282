using System;
using System.Linq;
using RearrangedS282;
using UnityEngine;
using Object=UnityEngine.Object;
internal static class Program {
 static void Check(bool value,string label){if(!value)throw new Exception(label);Console.WriteLine("PASS "+label);}
 static MeshRenderer Add(Transform parent,Material material){var go=new GameObject();go.transform.parent=parent;var r=go.Add<MeshRenderer>();r.sharedMaterial=material;return r;}
 static int Main(string[] args){try{
  if(args.Length>0&&args[0].StartsWith("retarget"))return RetargetTests.Run(args[0]);
  var carGo=new GameObject();var car=carGo.Add<TrainCar>();var handler=carGo.Add<ExplosionModelHandler>();
  var body=new GameObject();body.transform.parent=carGo.transform;var normal=new Material();var burn=new Material();var native=Add(body.transform,normal);
  var stock=new ExplosionModelHandler.MaterialSwapData{swapMaterial=burn,affectedGameObjects=new[]{body}};
  var other=Add(carGo.transform,normal);var foreign=new ExplosionModelHandler.MaterialSwapData{swapMaterial=burn,affectedGameObjects=Array.Empty<GameObject>(),affectedRenderers=new System.Collections.Generic.List<(Renderer renderer,Material originalMat)>{(other,normal)}};
  handler.materialSwaps=new[]{stock,foreign};handler.gameObjectsToDisable=Array.Empty<GameObject>();handler.gameObjectSwaps=Array.Empty<ExplosionModelHandler.GameObjectSwapData>();handler.Awake();
  var owner=new B99ExplosionMaterials(car,body.transform);var pin=Add(body.transform,normal);owner.Refresh(new Renderer[]{native,pin});
  handler.HandleExplosionModelChange();Check(stock.affectedRenderers.Any(p=>ReferenceEquals(p.renderer,pin)),"Native lazy cache includes active temporary pin");
  handler.RevertToUnexplodedModel();
  var leaseType=typeof(FranklinPinPresentation).GetNestedType("Lease",System.Reflection.BindingFlags.NonPublic);
  var lease=(IDisposable)Activator.CreateInstance(leaseType,true);
  ((System.Collections.Generic.List<GameObject>)leaseType.GetField("Objects",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(lease)).Add(pin.gameObject);
  int retireCalls=0;bool activeAtRetirement=false;
  Action<System.Collections.Generic.IEnumerable<Renderer>> retire=renderers=>{retireCalls++;activeAtRetirement=pin.gameObject.activeSelf;owner.Forget(renderers);};
  leaseType.GetField("Retire",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(lease,retire);
  var route=new RouteFixture(car,lease);
  if(args.Length>0&&args[0]=="pool")Check(route.PoolReturn(),"Production pool-return restoration succeeds");
  else route.SwitchToStock();
  Check(route.Stock,"Production stock restoration reaches the stock arrangement");
  lease.Dispose();
  Check(retireCalls==1&&activeAtRetirement&&!pin.gameObject.activeSelf,"Actual Franklin lease unregisters once before deferred destruction");
  if(args.Length>0&&(args[0]=="native"||args[0]=="pool"))Check(!stock.affectedRenderers.Any(p=>ReferenceEquals(p.renderer,pin)),"Retired pin removed from the native body cache");
  if(args.Length>0&&args[0]=="sameframe")stock.affectedRenderers.RemoveAll(p=>ReferenceEquals(p.renderer,pin));
  owner.Refresh(carGo.GetComponentsInChildren<MeshRenderer>(true));
  Check(!handler.materialSwaps.Any(s=>s.affectedRenderers.Any(p=>ReferenceEquals(p.renderer,pin))),"Retired pin absent from all native lists after same-frame inactive refresh");
  Check(foreign.affectedRenderers.Count==1&&ReferenceEquals(foreign.affectedRenderers[0].renderer,other),"Retirement preserves unrelated material-swap ownership");
  Object.Flush();handler.HandleExplosionModelChange();handler.RevertToUnexplodedModel();
  Check(ReferenceEquals(native.sharedMaterial,normal),"Later native explosion and repair restore the original wheel material");
  var next=Add(body.transform,normal);owner.Refresh(new Renderer[]{next});handler.HandleExplosionModelChange();
  Check(ReferenceEquals(next.sharedMaterial,burn),"Replacement pin participates in a later native explosion");
  handler.RevertToUnexplodedModel();Check(ReferenceEquals(next.sharedMaterial,normal),"Replacement pin repairs to its original material");return 0;
 }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
}
