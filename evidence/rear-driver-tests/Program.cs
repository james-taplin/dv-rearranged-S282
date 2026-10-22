using System;
using System.Linq;
using System.Text.Json;
using UnityEngine;
using RearrangedS282;
namespace RearrangedS282 { internal partial class WheelRearranger { internal Transform Clone(Transform t,Transform parent)=>CloneEngineDrive(t,t.GetComponent<Animator>(),"rear",parent,d=>{}); } }
internal static class Program
{
    static int checks;
    static void Check(bool ok,string message){checks++;if(!ok)throw new Exception(message);}
    static float Gap(Quaternion a,Quaternion b)=>Math.Abs(a.x-b.x)+Math.Abs(a.y-b.y)+Math.Abs(a.z-b.z)+Math.Abs(a.w-b.w);
    static void Main(string[] args){
        foreach(int drivers in new[]{3,4,5})foreach(int side in new[]{-1,1}){
            var source=new GameObject("front").transform;source.gameObject.AddComponent<Animator>();source.gameObject.AddComponent<FixtureOilReader>();
            for(int i=1;i<=6;i++){var wheel=source.Add("s282_wheels_driving_"+i);wheel.gameObject.SetActive(i<=drivers);if(i>4)wheel.gameObject.AddComponent<PoweredWheelRotater>().Init(source.Find("s282_wheels_driving_4").gameObject);}
            var owner=new WheelRearranger();var parent=new GameObject("native rear articulation").transform;
            var clone=owner.Clone(source,parent);clone.gameObject.SetActive(true);
            Check(clone.GetComponentsInChildren<FixtureOilReader>(true).Length==0,"Retained copied oil reader");
            Check(clone.GetComponentsInChildren<Animator>(true).Length==1,"Removed native Animator");
            var followers=clone.GetComponentsInChildren<PoweredWheelRotater>(true);Check(followers.Length==2,"Destroyed added-wheel followers");
            var fourth=clone.Find("s282_wheels_driving_4");
            foreach(var f in followers)Check(f.enabled&&f.Source==fourth.gameObject,"Follower was disabled or followed the front engine");
            foreach(float phase in new[]{0f,30f,90f,180f,270f,359f,-30f,-180f}){
                source.Find("s282_wheels_driving_4").localRotation=Quaternion.Euler(side*13,0,0);
                fourth.localRotation=Quaternion.Euler(side*phase,0,0);
                foreach(var f in followers){f.LateUpdate();Check(Gap(f.transform.localRotation,fourth.localRotation)<.000001f,"Rear driver froze or lagged local source");}
            }
        }
        var result=new{passed=true,checks,scope="Actual CloneEngineDrive and PoweredWheelRotater bodies with managed cloning/transform stand-ins; live native Animator playback remains manual"};
        System.IO.File.WriteAllText(args[0],JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(result));
    }
}
