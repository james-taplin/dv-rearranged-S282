using System;using System.Linq;using System.IO;using System.Text.Json;using UnityEngine;using RearrangedS282;
internal static class Pins {
 static int checks,cases;static void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
 static Transform Part(Transform p,string name){var t=p.Add(name);t.gameObject.AddComponent<MeshRenderer>();t.gameObject.AddComponent<MeshFilter>().sharedMesh=new(){name=name};return t;}
 static Transform Drive(string name,bool pinEnabled){
  var d=new GameObject(name).transform;d.gameObject.AddComponent<Animator>();
  for(int i=1;i<=6;i++){var w=Part(d,"s282_wheels_driving_"+i);w.localPosition=new(0,.712f,i==1?8.002f:i==2?6.302f:0);}
  foreach(var name2 in new[]{"s282_mech_cutoff_boomerang_high","s282_mech_cutoff_rail","s282_mech_cutoff_rod","s282_mech_cutoff_rod_hinge","s282_mech_cylinder_rod","LubricatorParts"})Part(d,name2);
  Part(Part(d,"s282_mech_push_rod"),"s282_mech_push_rod_to_cutoff_base");
  var rod=Part(d,"s282_mech_wheels_connect");Part(rod,"s282_mech_push_rod_to_connect").GetComponent<MeshRenderer>().enabled=pinEnabled;
  var pin=Part(rod,"s282_mech_connect_to_cutoff_base");pin.GetComponent<MeshRenderer>().enabled=pinEnabled;
  Part(pin,"s282_mech_connect_to_cutoff_rod");var coarse=Part(pin,"s282_mech_connect_to_cutoff_base_LOD1");
  d.gameObject.AddComponent<LODGroup>().SetLODs(new[]{new LOD(.1f,new Renderer[]{pin.GetComponent<MeshRenderer>()}),new LOD(.01f,new Renderer[]{coarse.GetComponent<MeshRenderer>()})});
  var shoes=d.Add("s282a_brake_shoes(Clone)");for(int i=1;i<=4;i++)Part(shoes,"s282a_brake_shoe_"+i);
  Part(d.Add("franklin_type_b(Clone)"),"shaft");Part(d.Add("franklin_type_b_4444(Clone)"),"shaft");return d;
 }
 static Transform Pin(Transform d)=>d.Find("s282_mech_wheels_connect/s282_mech_connect_to_cutoff_base");
 static bool Visible(Transform d)=>Pin(d).gameObject.activeInHierarchy&&Pin(d).GetComponent<MeshRenderer>().enabled&&Pin(d).GetComponent<MeshFilter>().sharedMesh!=null;
 static Transform MainRod(Transform d)=>d.Find("s282_mech_wheels_connect/s282_mech_push_rod_to_connect");
 static bool MainVisible(Transform d)=>MainRod(d).gameObject.activeInHierarchy&&MainRod(d).GetComponent<MeshRenderer>().enabled&&MainRod(d).GetComponent<MeshFilter>().sharedMesh!=null;
 static void Main(string[] args){
  foreach(string mode in new[]{"duplex","compact","single"}){
   var old=Drive("old "+mode,false);var oldOwner=new WheelRearranger();oldOwner.TwoCoupled=mode!="single";
   if(mode=="duplex")oldOwner.Duplex(old,false,true);else oldOwner.Run(old,false,true);
   Check(!MainVisible(old),"Previous "+mode+" leaves inherited hidden main rod invisible");
   Check(mode=="compact"?Visible(old):!Visible(old),"Previous compact-only pin repair does not cover duplex/single engines");
  }
  foreach(string mode in new[]{"duplex","compact","single"})foreach(bool clone in new[]{false,true})foreach(bool enabled in new[]{false,true})foreach(string side in new[]{"L","R"}){
   var d=Drive(mode+" "+(clone?"clone ":"front ")+side,enabled);d.localScale=new(side=="L"?-1:1,1,1);
   var owner=new WheelRearranger();owner.TwoCoupled=mode!="single";
   var mesh=Pin(d).GetComponent<MeshFilter>().sharedMesh;var lod=d.GetComponent<LODGroup>().GetLODs();
   for(int repeat=0;repeat<4;repeat++){
    Pin(d).GetComponent<MeshRenderer>().enabled=enabled;MainRod(d).GetComponent<MeshRenderer>().enabled=enabled;
    Pin(d).gameObject.SetActive(false);MainRod(d).gameObject.SetActive(false);
    if(mode=="duplex")owner.Duplex(d,clone);else owner.Run(d,clone);cases++;
    Check(Visible(d)&&MainVisible(d),"Selected Franklin main rod and crank restored on both sides after repeated setup");
    Check(ReferenceEquals(mesh,Pin(d).GetComponent<MeshFilter>().sharedMesh),"Existing crank mesh retained");
    Check(Pin(d).localPosition.x==.15f&&Pin(d).localPosition.z==0,"Existing crank pin datum retained");
    Check(!Pin(d).Find("s282_mech_connect_to_cutoff_rod").gameObject.activeSelf,"Unused Walschaerts rod remains hidden");
    if(mode!="single"||clone)Check(!Pin(d).Find("s282_mech_connect_to_cutoff_base_LOD1").gameObject.activeSelf,"Disabled native coarse pin retained");
    Check(d.GetComponent<LODGroup>().GetLODs()[0].renderers.Contains(Pin(d).GetComponent<MeshRenderer>())&&ReferenceEquals(lod,d.GetComponent<LODGroup>().GetLODs()),"Native detail membership retained");
    if(mode!="single")Check(d.Find("s282_mech_wheels_connect/[Rearranged two-axle rod]").gameObject.activeSelf,"Coupling rod retained");
    Check(d.Find("s282_wheels_driving_1").localPosition.z==8.002f&&d.Find("s282_wheels_driving_2").localPosition.z==6.302f,"Accepted driver positions retained");
    Check(d.GetComponentsInChildren<MeshRenderer>(true).Count(r=>r.name=="s282_mech_connect_to_cutoff_base")==1,"No duplicate crank mesh");
    Check(d.GetComponentsInChildren<MeshRenderer>(true).Count(r=>r.name=="s282_mech_push_rod_to_connect")==1,"No duplicate main rod");
    if(mode=="single")Check(!d.Find("s282_wheels_driving_1").gameObject.activeSelf&&d.Find("s282_wheels_driving_2").gameObject.activeSelf,"Single-driver selection retained");
   }
  }
  var report=new{passed=true,checks,cases,oldVisibilityGapReproduced=true,scope="Extracted actual shared duplex, compact-triplex and single-driver setup with hierarchy/render/LOD stand-ins. Confirms inherited disabled renderer defect; does not reproduce the reporter's unspecified moving clipping, native animation or Unity rendering."};
  File.WriteAllText(args[0],JsonSerializer.Serialize(report));Console.WriteLine(JsonSerializer.Serialize(report));
 }
}
