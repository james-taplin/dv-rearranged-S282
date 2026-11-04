using System;using System.IO;using System.Text.Json;using UnityEngine;using RearrangedS282;
namespace RearrangedS282 {
 internal enum WheelArrangementType {s282,s2442,s4444,s22222,s02220,s24440,s28882}
 internal partial class WheelRearranger:MonoBehaviour {
  internal WheelArrangementType currentWA;internal Transform body,articulatedFrontFrame;
  internal Matrix4x4[] scaledEngineMaps;private Transform MalletBody=>body;
  internal bool tenderAvailable=true;internal int tenderCalls;
  internal bool TryMapTenderOutlet(Vector3 origin,Vector3 centre,out Vector3 local){tenderCalls++;local=new(1,2,3);return tenderAvailable;}
 }
 internal static partial class ArticulatedEffects {
  internal sealed class Secondary {internal int Index;internal bool Tender;}
 }
}
internal static class Program {
 static int checks,cases;static void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
 static void Near(Vector3 a,Vector3 b,string why)=>Check((a-b).magnitude<.0002f,why);
 static void Main(string[] args){
  foreach(float yaw in new[]{0f,37f,90f})foreach(bool translated in new[]{false,true})foreach(bool mapped in new[]{false,true}){
   var carObject=new GameObject("car");var car=carObject.AddComponent<TrainCar>();
   car.transform.localPosition=translated?new(50,20,-100):Vector3.zero;car.transform.localRotation=Quaternion.Euler(0,yaw,0);
   var owner=carObject.AddComponent<WheelRearranger>();owner.body=car.transform.Add("body");owner.body.localPosition=new(3,1,-2);owner.body.localRotation=Quaternion.Euler(0,15,0);
   owner.articulatedFrontFrame=mapped?owner.body:null;owner.scaledEngineMaps=mapped?new[]{Matrix4x4.identity,Matrix4x4.TRS(new(0,0,-4.65f),Quaternion.identity,Vector3.one),Matrix4x4.identity}:null;
   if(mapped)owner.scaledEngineMaps[2]=Matrix4x4.TRS(new(0,0,-1.8f),Quaternion.identity,new Vector3(1,.8365486f,.8365486f));
   var state=carObject.AddComponent<ArticulatedEffects.EffectsState>();state.Car=car;
   foreach(var id in new[]{WheelArrangementType.s282,WheelArrangementType.s2442,WheelArrangementType.s4444,WheelArrangementType.s22222,WheelArrangementType.s02220,WheelArrangementType.s24440,WheelArrangementType.s28882})foreach(int engine in new[]{0,1,2})foreach(float z in new[]{9.9037f,9.076f,9.511f})foreach(bool offset in new[]{false,true}){
    owner.currentWA=id;cases++;
    var localCentre=new Vector3(1.09f,.3f,z);var localOrigin=localCentre+(offset?new Vector3(.11f,-.07f,.04f):Vector3.zero);
    var centre=owner.body.TransformPoint(localCentre);var origin=owner.body.TransformPoint(localOrigin);
    var carCentre=car.transform.InverseTransformPoint(centre);var carOrigin=car.transform.InverseTransformPoint(origin);
    var secondary=new ArticulatedEffects.Secondary{Index=engine};
    Vector3 baseline=owner.TryMapArticulatedOutlet(origin,centre,engine,out var nativeMapped)?nativeMapped:carOrigin+Vector3.back*4.65f;
    var delta=id==WheelArrangementType.s4444&&engine==1?CylinderCasingShape.CompactFranklinRearNose(localCentre)-localCentre:Vector3.zero;
    if(mapped&&engine==2&&(id==WheelArrangementType.s22222||id==WheelArrangementType.s24440))
     delta=owner.scaledEngineMaps[2].MultiplyVector(CylinderCasingShape.CompactFranklinTriplexNose(localCentre)-localCentre);
    Near(owner.FranklinRearOutletOffset(centre,engine),delta,"Nose delta uses native body coordinates");
    var result=state.SecondaryOutletLocal(carOrigin,secondary,carCentre);
    Near(result,baseline+delta,"Mapped and fallback outlet centres follow casting nose");
    for(int i=0;i<4;i++)Near(state.SecondaryOutletLocal(carOrigin,secondary,carCentre),result,"Repeated reapply cannot accumulate outlet shift");
    Near(owner.body.InverseTransformPoint(centre),localCentre,"Authored source centre is unchanged");
    if(id==WheelArrangementType.s4444&&engine==1){
     if(z>9.8f)Check(Math.Abs(delta.z+.12f)<.0001f,"Front cock moves with the full 120 mm cap correction");
     if(z<9.49f)Check(delta.sqrMagnitude==0,"Rear cock below transition retains native seat");
     if(z==9.511f)Check(delta.z<0&&delta.z>-.003f,"Water drip follows the small local transition movement");
    }
    if(mapped&&engine==2&&(id==WheelArrangementType.s22222||id==WheelArrangementType.s24440)){
     if(z>9.85f)Check(Math.Abs(delta.z+.20f*.8365486f)<.0001f,"Only the middle triplex cock follows the scaled 200 mm nose correction");
     if(z<9.49f)Check(delta.sqrMagnitude==0,"Triplex rear cock behind the transition retains its native seat");
    }
   }
   owner.currentWA=WheelArrangementType.s4444;
   var tender=new ArticulatedEffects.Secondary{Index=1,Tender=true};
   Near(state.SecondaryOutletLocal(new(7,8,9),tender,new(1,2,9.9037f)),new(1,2,3),"Tender mapping returns before rear locomotive correction");
   Check(owner.tenderCalls==1,"Tender uses its existing mapping exactly once");
   owner.tenderAvailable=false;bool refused=false;try{state.SecondaryOutletLocal(Vector3.zero,tender,Vector3.zero);}catch(InvalidOperationException){refused=true;}
   Check(refused,"Missing tender mapping retains its existing failure");
  }
  var report=new{passed=true,checks,cases,scope="Actual production rear-outlet helper, secondary dispatch and articulated mapping with managed transform stand-ins. Rotated/translated frames, origin/centre offsets, fallback, all unaffected layouts and tender preservation. No Unity particles or audible output exercised."};
  File.WriteAllText(args[0],JsonSerializer.Serialize(report));Console.WriteLine(JsonSerializer.Serialize(report));
 }
}
