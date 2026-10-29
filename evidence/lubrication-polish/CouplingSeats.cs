using System;using System.Linq;using System.Text.Json;using UnityEngine;using RearrangedS282;
internal static class CouplingSeats {
 internal static int Run(string path) {
  using var data=JsonDocument.Parse(System.IO.File.ReadAllText(path));
  var all=data.RootElement.GetProperty("meshes").EnumerateArray().ToArray();
  Mesh Read(string name){var m=all.Single(x=>x.GetProperty("name").GetString()==name);var mesh=new Mesh{
   vertices=m.GetProperty("vertices").EnumerateArray().Select(v=>new Vector3(v[0].GetSingle(),v[1].GetSingle(),v[2].GetSingle())).ToArray(),
   triangles=m.GetProperty("triangles").EnumerateArray().Select(i=>i.GetInt32()).ToArray()};mesh.RecalculateBounds();return mesh;}
  var native=Read("native");var half=Read("threeHalf");var two=Read("two");var longer=Read("fiveSix");
  int checks=0;void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
  void Part(Transform root,string name,Mesh mesh,Vector3 pos,Vector3 rotation,Vector3 scale){var p=root.Add(name);p.localPosition=pos;p.localRotation=Quaternion.Euler(rotation.x,rotation.y,rotation.z);p.localScale=scale;p.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;p.gameObject.AddComponent<MeshRenderer>();}
  foreach(int side in new[]{-1,1})foreach(float scale in new[]{.55f,.8f,1f,1.198f})foreach(float phase in new[]{0f,30f,90f,180f})foreach(string layout in new[]{"native","three","malletThree","two","shortNamed","five","sevenNamed"}) {
   var drive=new GameObject("drive").transform;drive.localPosition=new(70,2,30);drive.localScale=new(side,scale,scale);drive.localRotation=Quaternion.Euler(0,17,0);
   var rod=drive.Add("s282_mech_wheels_connect");rod.localRotation=Quaternion.Euler(.05f,0,0);rod.localPosition=new(.976f,.71f+.335f*(float)Math.Sin(phase*Math.PI/180),6.3f);
   rod.gameObject.AddComponent<MeshFilter>().sharedMesh=native;var nativeRenderer=rod.gameObject.AddComponent<MeshRenderer>();
   float[] pins={1.7f,0f,-1.55f,-3.1f};
   if(layout=="three"||layout=="malletThree"){
    nativeRenderer.enabled=false;float shift=layout=="malletThree"?1.55f:0;
    Part(rod,"s282_mech_wheels_connect front",half,new(0,0,-3.11f+shift),new(0,180,0),Vector3.one);
    Part(rod,"s282_mech_wheels_connect rear",half,new(0,0,shift),new(0,180,0),new(1,1,-1));
    pins=new[]{shift,shift-1.55f,shift-3.1f};
   }else if(layout=="two"||layout=="shortNamed"){
    nativeRenderer.enabled=false;Part(rod,layout=="two"?"s282_mech_wheels_connect short":"[Rearranged two-axle rod]",two,new(0,0,-1.38f),new(0,0,180),Vector3.one);pins=new[]{.32f,-1.55f};
   }else if(layout=="five"||layout=="sevenNamed"){
    Part(rod,layout=="five"?"s282_mech_wheels_connect extension":"[Rearranged seven-axle rod]",longer,new(.003f,.004f,-3.1f),new(0,180,0),Vector3.one);pins=new[]{1.7f,0f,-1.55f,-3.1f,-4.65f};
   }
   // Neither a nearer main rod nor a hidden/native rod may win the seat search.
   Part(rod,"s282_mech_push_rod_to_connect",native,new(.156f,.14f,0),Vector3.zero,Vector3.one);
   foreach(float z in pins){
    var pin=rod.TransformPoint(new(.156308f,0,z));
    Check(OilCupSeat.TryCouplingTop(rod,pin,native,out var seat),layout+" missing bearing at "+z);
    var local=rod.InverseTransformPoint(seat);Check(Math.Abs(local.x)<.01f,layout+" cup floats outside coupling rod");
    Check(Math.Abs(local.z-z)<.0001f,"Longitudinal pin position changed");
    Check(local.y>-.01f&&local.y<.14f,"Cup does not sit on rod top");
    Check(OilCupSeat.TryCouplingTop(rod,seat,native,out var again)&&(seat-again).magnitude<.0001f,"Seat is not stable");
   }
   Check(!OilCupSeat.TryCouplingTop(rod,rod.TransformPoint(new(0,0,30)),native,out _),"Unsupported pin silently accepted");
  }
  var single = new GameObject("single driver rod").transform;
  var mainBearing = single.Add("s282_mech_push_rod_to_connect");mainBearing.localPosition=new(.15f,0,-1.55f);
  var authored = mainBearing.Add("Oiling2");authored.localPosition=new(0,.14f,-.02f);
  Check(OilCupSeat.TryCouplingTop(single,mainBearing.position,native,out var mount),"Single-driver main bearing rejected");
  Check((mount-authored.position).magnitude<.00001f,"Authored main-bearing mount changed");
  Check(!OilCupSeat.TryCouplingTop(single,new Vector3(0,0,10),native,out _),"Main-bearing fallback accepted unrelated axle");
  return checks;
 }
}
