using System;using System.IO;using System.Linq;using System.Text.Json;using UnityEngine;using RearrangedS282;
class Program {
 static int checks;static void Check(bool v,string why){checks++;if(!v)throw new Exception(why);}
 static Mesh Read(JsonElement all,string name){var m=all.EnumerateArray().Single(x=>x.GetProperty("name").GetString()==name);var mesh=new Mesh{vertices=m.GetProperty("vertices").EnumerateArray().Select(x=>new Vector3(x[0].GetSingle(),x[1].GetSingle(),x[2].GetSingle())).ToArray(),triangles=m.GetProperty("triangles").EnumerateArray().Select(x=>x.GetInt32()).ToArray()};mesh.RecalculateBounds();return mesh;}
 static Transform Part(Transform p,string n,Mesh m){var t=p.Add(n);t.gameObject.AddComponent<MeshFilter>().sharedMesh=m;t.gameObject.AddComponent<MeshRenderer>();return t;}
 static void Main(string[] args){
  using var doc=JsonDocument.Parse(File.ReadAllText(args[0]));var all=doc.RootElement.GetProperty("meshes");var native=Read(all,"native");var two=Read(all,"two");
  foreach(float scale in new[]{.55f,.7f,.9f,1f})foreach(int side in new[]{-1,1})foreach(bool franklin in new[]{false,true}){
   var drive=new GameObject("engine").transform;drive.localScale=new(side,scale,scale);drive.localPosition=new(130,4,20);drive.localRotation=Quaternion.Euler(0,13,0);drive.gameObject.AddComponent<Animator>();
   var rod=Part(drive,"s282_mech_wheels_connect",native);rod.localPosition=new(.976f,.376f,6.302f);
   var main=Part(rod,"s282_mech_push_rod_to_connect",new Mesh());main.localPosition=new(.106308f,-.00043f,franklin?0:-1.55334f);
   var oil=main.Add("Oiling2");oil.localPosition=new(-.0003836155f,.14330068f,.0001301765f);
   var cut=Part(rod,"s282_mech_connect_to_cutoff_base",new Mesh());cut.Add("s282_mech_connect_to_cutoff_rod");
   for(int i=1;i<=6;i++)Part(drive,"s282_wheels_driving_"+i,new Mesh());
   var setup=new WheelRearranger();setup.SetUp(two);
   if(franklin){
    var shortSide=Part(rod,"[Rearranged two-axle rod]",setup.Side);shortSide.localRotation=Quaternion.Euler(0,0,180);shortSide.GetComponent<MeshRenderer>().enabled=false;
    rod.GetComponent<MeshRenderer>().enabled=false;
    Check(!OilCupSeat.TryCouplingTop(rod,rod.TransformPoint(new(.106f,0,1.7f)),native,out _),"Disabled short rod unexpectedly supplied coupling mount");
    setup.Run(drive);
    Check(shortSide.GetComponent<MeshRenderer>().enabled&&shortSide.gameObject.activeInHierarchy,"Selected short rod renderer not restored");
    Check(OilCupSeat.TryCouplingTop(rod,rod.TransformPoint(new(.106f,0,1.7f)),native,out _),"Franklin front bearing still has no visible seat");
   }
   Check(OilCupSeat.TryCouplingTop(rod,main.position,native,out var seat),"Main-bearing seat missing");
   Check((seat-oil.position).magnitude<.00001f,"Main-bearing cup moved onto inner coupling bearing");
   var marker=rod.Add("test marker");Check(WheelRearranger.Seat(marker,rod,main.position,native),"Main marker could not bind");
   Check(marker.parent==oil,"Main-bearing marker does not follow its native animated mount");
   foreach(float phase in new[]{-12f,0f,12f}){
    main.localRotation=Quaternion.Euler(phase,0,0);
    Check((marker.position-oil.position).magnitude<.00001f,"Main-bearing cup drifts when the connecting rod rocks");
   }
   // Actual previous search selected the native coupling top instead of Oiling2.
   if(!franklin){
    var pin=rod.InverseTransformPoint(main.position);Check(OilCupSeat.TryTop(native.vertices,native.triangles,0,pin.z,out var top),"Old coupling search did not reproduce");
    var previous=rod.TransformPoint(new(0,top,pin.z));Check((previous-oil.position).magnitude>.09f,"Old main-bearing placement gap not reproduced");
   }
   Check(!OilCupSeat.TryCouplingTop(rod,rod.TransformPoint(new(0,0,20)),native,out _),"Main-bearing fallback claimed unrelated axle");
  }
  var report=new{passed=true,checks,source="Production OilCupSeat and extracted actual CreateArticulatedSideRod/ConfigureEngineDrive methods against native rod triangles",limitations="Managed hierarchy and vectors; native click/VR interaction and exact reported Angus pose require game confirmation"};
  File.WriteAllText(args[1],JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(report));
 }
}
