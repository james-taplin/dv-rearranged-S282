using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using DV.Wheels;
using RearrangedS282;
using UnityEngine;

internal static class Program {
    static int checks;
    static void Check(bool condition,string message) { checks++; if(!condition)throw new Exception(message); }
    static bool Near(float a,float b)=>Math.Abs(a-b)<.0001f;
    static bool Near(Vector3 a,Vector3 b)=>Near(a.x,b.x)&&Near(a.y,b.y)&&Near(a.z,b.z);
    static Transform Child(Transform parent,string name,Vector3 p) {
        var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=p;return t;
    }
    static Vector3 ReadVector(JsonElement item)=>new(item[0].GetSingle(),item[1].GetSingle(),item[2].GetSingle());
    static int Main(string[] args) {
      try {
        using var data=JsonDocument.Parse(File.ReadAllText(args[0]));
        var wheels=data.RootElement.GetProperty("wheels").EnumerateArray().Where(w=>w.GetProperty("lod").GetInt32()==0).ToDictionary(w=>w.GetProperty("axle").GetInt32(),w=>ReadVector(w.GetProperty("position")));
        foreach(float yaw in new[]{0f,47f}) foreach(bool alternateTrailing in new[]{false,true}) {
          var m=new GameObject("car").AddComponent<WheelRearranger>();
          m.transform.localPosition=new Vector3(80,5,-130);m.transform.localRotation=Quaternion.Euler(0,yaw,0);
          m.driveL=Child(m.transform,"left",new Vector3(0,0,-2.1f));m.driveR=Child(m.transform,"right",new Vector3(0,0,-2.1f));
          m.driveL.localScale=new Vector3(-1,1.2879f,1.2879f);m.driveR.localScale=new Vector3(1,1.2879f,1.2879f);
          m.secondRightDriveWheel=Child(m.driveR,"wheel2",wheels[2]);m.thirdRightDriveWheel=Child(m.driveR,"wheel3",wheels[3]);m.fourthRightDriveWheel=Child(m.driveR,"wheel4",wheels[4]);
          m.secondLeftDriveWheel=Child(m.driveL,"wheel2",wheels[2]);m.thirdLeftDriveWheel=Child(m.driveL,"wheel3",wheels[3]);m.fourthLeftDriveWheel=Child(m.driveL,"wheel4",wheels[4]);
          var bogie=Child(m.transform,"bogie",new Vector3(0,0,8));bogie.localRotation=Quaternion.Euler(0,17,0);
          m.driveAxle2=Child(bogie,"axle2",new Vector3(0,.71f,0));m.driveAxle3=Child(bogie,"axle3",new Vector3(0,.71f,-1));m.driveAxle4=Child(bogie,"axle4",new Vector3(0,.71f,-2));
          foreach(var physicalAxle in new[]{m.driveAxle2,m.driveAxle3,m.driveAxle4})physicalAxle.gameObject.AddComponent<PoweredWheel>();
          Transform F(string name,float x,float y,float z)=>Child(m.transform,name,new Vector3(x,y,z));
          m.leftSecondBrakeCaliper=F("l2",-.7f,.8f,6);m.leftThirdBrakeCaliper=F("l3",-.7f,.8f,4);m.leftFourthBrakeCaliper=F("l4",-.7f,.8f,2);
          m.rightSecondBrakeCaliper=F("r2",.7f,.8f,6);m.rightThirdBrakeCaliper=F("r3",.7f,.8f,4);m.rightFourthBrakeCaliper=F("r4",.7f,.8f,2);
          m.spark1L=F("s1l",-.75f,0,5.37f);m.spark1R=F("s1r",.75f,0,5.37f);m.spark2L=F("s2l",-.75f,0,3.37f);m.spark2R=F("s2r",.75f,0,3.37f);m.spark3L=F("s3l",-.75f,0,1.37f);m.spark3R=F("s3r",.75f,0,1.37f);
          m.lfCrossheadBracket=F("guideL",-.5f,.21f,0);m.rfCrossheadBracket=F("guideR",.5f,.21f,0);
          var trailing=F("trailing",0,alternateTrailing?.44f:.53f,.1f);var trailingBefore=trailing.position;
          var anchors=new[]{m.leftSecondBrakeCaliper,m.leftThirdBrakeCaliper,m.leftFourthBrakeCaliper,m.rightSecondBrakeCaliper,m.rightThirdBrakeCaliper,m.rightFourthBrakeCaliper,m.spark1L,m.spark1R,m.spark2L,m.spark2R,m.spark3L,m.spark3R};
          var baseline=anchors.Select(t=>m.transform.InverseTransformPoint(t.position)).ToArray();
          float frontBefore=m.transform.InverseTransformPoint(m.secondRightDriveWheel.position).z;
          m.ApplyScotchFixture();
          m.Align(49); // Actual shared additional-axle dispatcher, called after geometry.
          float shift=(1.2879f-1.335f)*wheels[2].z;float lift=(1.335f-1.2879f)*wheels[2].y;
          Check(Near(m.AppliedRadius,.712f*1.335f),"Physics radius is nominal measured tread radius");
          Check(Near(m.driveR.localScale,new Vector3(1,1.335f,1.335f))&&Near(m.driveL.localScale,new Vector3(-1,1.335f,1.335f)),"Both native drives grow consistently");
          Check(Near(m.transform.InverseTransformPoint(m.secondRightDriveWheel.position).z,frontBefore),"Front driver datum stays fixed");
          for(int i=0;i<anchors.Length;i++) {
            var expected=new Vector3(baseline[i].x,baseline[i].y*1.335f/1.2879f,(baseline[i].z+2.1f)*1.335f/1.2879f-2.1f+shift);
            Check(Near(m.transform.InverseTransformPoint(anchors[i].position),expected),"Detached brake/spark follows the original drivetrain frame");
          }
          Check(Near(m.splitS282A.Cylinders,new Vector3(0,.21f+lift,shift)),"LMS cylinder, particles and gear use absolute moved frame");
          Check(Near(m.lfCrossheadBracket.localPosition,new Vector3(-.5f,.21f+lift,shift))&&Near(m.rfCrossheadBracket.localPosition,new Vector3(.5f,.21f+lift,shift)),"Both guide brackets follow cylinders");
          var axle=new[]{m.driveAxle2,m.driveAxle3,m.driveAxle4};var visual=new[]{m.secondRightDriveWheel,m.thirdRightDriveWheel,m.fourthRightDriveWheel};
          for(int i=0;i<3;i++)Check(Near(axle[i].position,visual[i].position)&&ReferenceEquals(axle[i].GetComponent<PoweredWheel>().wheelTransform,visual[i]),"Physical axle follows its native visible driver");
          Check(Near(trailing.position,trailingBefore),"Native trailing option is not repositioned");
          try {m.ApplyScotchFixture();throw new Exception("Repeated apply without stock restore was accepted");} catch(InvalidOperationException) {Check(true,"Repeated apply must first restore Pacific");}
        }
        var report=new {passed=true,checks,arrangementId=49,driverRadiusM=.712f*1.335f,driverScale=1.335f,driveOffsetZ=-2.1f+(1.2879f-1.335f)*wheels[2].z,scope="Actual Scotch partial and native axle helper with managed transform and LMS stand-ins; no Unity player or integrated dispatch."};
        File.WriteAllText(args[1],JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(report));return 0;
      } catch(Exception e) {Console.Error.WriteLine(e);return 1;}
    }
}
