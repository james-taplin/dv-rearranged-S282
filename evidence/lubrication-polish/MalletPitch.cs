using System;using System.Linq;using System.Text.Json;using UnityEngine;using UnityEngine.Animations;
namespace UnityEngine.Animations {
 [Flags] public enum Axis {X=1,Y=2,Z=4}
 public struct ConstraintSource { public Transform sourceTransform;public float weight; }
 public class PositionConstraint:Component {
  public Vector3 translationAtRest,translationOffset;public Axis translationAxis;public float weight;public bool locked,constraintActive,enabled=true;
  readonly System.Collections.Generic.List<ConstraintSource> sources=new();public int sourceCount=>sources.Count;
  public int AddSource(ConstraintSource s){sources.Add(s);return sources.Count-1;}public ConstraintSource GetSource(int i)=>sources[i];
  // Stand-in for native post-animation evaluation; the tests verify production
  // configuration and transformed target ownership, not Unity's C++ solver.
  public void Evaluate(){var p=transform.parent.InverseTransformPoint(sources[0].sourceTransform.position);var current=transform.localPosition;
   if((translationAxis&Axis.Z)!=0)current.z=p.z;transform.localPosition=current;}
 }
}
namespace RearrangedS282 {
 internal partial class WheelRearranger {
  static Transform RequireArticulatedPart(Transform d,string p)=>d.Find(p)??throw new Exception(p);
  internal static int TestMalletPitch(string nativeData){
   using var data=JsonDocument.Parse(System.IO.File.ReadAllText(nativeData));int checks=0;
   void Check(bool v,string why){checks++;if(!v)throw new Exception(why);}
   var owner=new WheelRearranger();
   foreach(var clip in data.RootElement.GetProperty("clips").EnumerateArray()){
    var binding=clip.GetProperty("bindings").EnumerateArray().Single(b=>b.GetProperty("path").GetString()=="s282_wheels_driving_1"&&b.GetProperty("attribute").GetInt32()==1);
    float nativeZ=binding.GetProperty("first")[2].GetSingle();
    Check(nativeZ>8f,"Native animation lacks expected keyed first-wheel position");
    foreach(int side in new[]{-1,1})foreach(float yaw in new[]{-15f,0f,15f})foreach(float scale in new[]{.55f,.75f,1f}){
     var drive=new GameObject("drive").transform;drive.localPosition=new(100,4,35);drive.localScale=new(side,scale,scale);drive.localRotation=Quaternion.Euler(0,yaw,0);
     var first=drive.Add("s282_wheels_driving_1");first.localPosition=new(0,.71f,7.85f);
     drive.Add("s282_wheels_driving_2").localPosition=new(0,.71f,6.3f);drive.Add("s282_wheels_driving_3").localPosition=new(0,.712f,4.75f);
     owner.HoldMalletDriverPitch(drive);var c=first.GetComponent<PositionConstraint>();
     Check(c!=null&&c.constraintActive&&c.translationAxis==Axis.Z,"Only first driver longitudinal position constrained");
     Check(c.locked&&c.weight==1&&c.sourceCount==1&&c.GetSource(0).sourceTransform.parent==drive,"Native constraint has one fixed engine-local source");
     for(int sample=0;sample<24;sample++){
      var rotation=Quaternion.Euler(sample*15,0,0);first.localRotation=rotation;first.localPosition=new(0,.71212983f,nativeZ);
      Check((nativeZ-7.85f)*scale>.08f,"Old animation does not reproduce visible rod gap");
      c.Evaluate();Check(Math.Abs(first.localPosition.z-7.85f)<.0001f,"Animation reset the intended pitch");
      Check(Math.Abs(first.localPosition.y-.71212983f)<.000001f,"Native wheel height changed");
      Check(first.localRotation.Equals(rotation),"Wheel rotation changed");
     }
     owner.ReleaseMalletDriverPitch();owner.ReleaseMalletDriverPitch();
     Check(first.GetComponents<PositionConstraint>().Length==0&&drive.childCount==3,"Constraint and anchor leaked after switching");
    }
   }
   return checks;
  }
 }
}
