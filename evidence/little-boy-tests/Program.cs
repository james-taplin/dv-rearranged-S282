using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using UnityEngine;

namespace UnityEngine
{
    internal struct Vector3
    {
        internal float x,y,z;
        internal Vector3(float x,float y,float z){this.x=x;this.y=y;this.z=z;}
        internal static Vector3 forward=>new(0,0,1);
        public static Vector3 operator +(Vector3 a,Vector3 b)=>new(a.x+b.x,a.y+b.y,a.z+b.z);
        public static Vector3 operator -(Vector3 a,Vector3 b)=>new(a.x-b.x,a.y-b.y,a.z-b.z);
        public static Vector3 operator *(Vector3 a,float b)=>new(a.x*b,a.y*b,a.z*b);
    }
    internal sealed class GameObject { internal bool activeSelf; internal void SetActive(bool active)=>activeSelf=active; }
    internal sealed class Transform
    {
        internal Vector3 localPosition,localScale;
        internal readonly GameObject gameObject=new();
        internal Transform transform=>this;
        internal Transform parent;
        internal Vector3 TransformPoint(Vector3 point)=>localPosition+point+(parent==null?new Vector3():parent.TransformPoint(new Vector3()));
        internal Vector3 InverseTransformPoint(Vector3 point)=>point-TransformPoint(new Vector3());
    }
    internal struct Matrix4x4 { }
}

namespace RearrangedS282
{
    internal sealed class ArticulatedSimulation { internal static ArticulatedSimulation Ensure(object loco)=>new(); }
    internal partial class WheelRearranger
    {
        private Transform firstFrontAxle=new(),secondFrontAxle=new(),firstFrontAxleSupport=new(),firstFrontAxleSupportLOD=new(),secondFrontAxleSupport=new(),secondFrontAxleSupportLOD=new();
        private Transform secondRearAxle=new(),thirdRearAxle=new(),secondRearAxleSupport=new();
        private Transform firstLeftDriveWheel=new(),secondLeftDriveWheel=new(),thirdLeftDriveWheel=new();
        private Transform body=new(),rearBogie=new(),frontBogie=new();
        private Transform MalletBody=>body;
        private MalletLayout mallet;
        private float cylinderFront;
        private int nativeMalletCalls;
        private object loco;
        private ArticulatedSimulation articulatedSimulation;
        private float MeasureNativeCylinderFront()=>cylinderFront;
        private static Matrix4x4 EngineMap(float offset,float scale)=>new();
        // The production ID37 pipeline is independently covered by the retained
        // Walschaerts clearance receipt. This stub models its measured setup.
        private void ApplyMalletGeometry(){nativeMalletCalls++;mallet=MeasureMallet();}
        private void ApplyTriplexGeometry(bool a,bool b=false)=>throw new InvalidOperationException("Wrong geometry route");
        private void ApplyBigMalletGeometry(WheelArrangementType id)=>throw new InvalidOperationException("Wrong geometry route");
        private void ApplyArticulatedGeometry(int id)=>throw new InvalidOperationException("Wrong geometry route");
        private static bool IsBigMallet(int id)=>false;

        internal object Run(float frontZ,float rearZ,float cylinder,float d1,float d2,float d3)
        {
            body.localPosition=new Vector3(7,3,-9); // translation must cancel
            frontBogie.parent=rearBogie.parent=body;
            frontBogie.localPosition=new Vector3(0,0,frontZ);rearBogie.localPosition=new Vector3(0,0,rearZ);
            firstFrontAxle.parent=secondFrontAxle.parent=frontBogie;
            secondRearAxle.parent=thirdRearAxle.parent=rearBogie;
            firstLeftDriveWheel.localPosition=new Vector3(0,.7121298f,d1);
            secondLeftDriveWheel.localPosition=new Vector3(0,.7121296f,d2);
            thirdLeftDriveWheel.localPosition=new Vector3(0,.7121294f,d3);
            cylinderFront=cylinder;
            DispatchActual(50);
            float lead1=frontZ+firstFrontAxle.localPosition.z,lead2=frontZ+secondFrontAxle.localPosition.z;
            float trail1=rearZ+secondRearAxle.localPosition.z,trail2=rearZ+thirdRearAxle.localPosition.z;
            float frontDriver=mallet.FrontOffset+MalletScale*(d2+(d2-d3));
            float rearDriver=mallet.RearOffset+MalletScale*d3;
            float radius=.712f*MalletScale;
            void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
            void Near(float actual,float expected,string message){Check(Math.Abs(actual-expected)<.0001f,message+": "+actual+" vs "+expected);}
            Check(nativeMalletCalls==1&&articulatedSimulation!=null,"Actual ID50 dispatcher did not select the native Mallet wrapper exactly once");
            Check(firstFrontAxle.gameObject.activeSelf&&secondFrontAxle.gameObject.activeSelf&&secondRearAxle.gameObject.activeSelf&&thirdRearAxle.gameObject.activeSelf,"Four-wheel carrying trucks not active");
            Check(firstFrontAxleSupport.gameObject.activeSelf&&secondFrontAxleSupport.gameObject.activeSelf&&secondRearAxleSupport.gameObject.activeSelf,"Native truck supports not active");
            Near(lead1-lead2,1.3f,"Native front truck spacing");
            Near(trail1-trail2,1f,"Native rear truck spacing");
            Near(trail1,mallet.TrailingZ,"Rear leading carrying axle differs from native Mallet datum");
            Near(secondRearAxleSupport.localPosition.z,-.5f+(trail1-rearZ+2.7f),"Rear support does not follow full truck shift");
            // Conservative same-axis tire circles: longitudinal spacing alone
            // must clear radii, irrespective of the different wheel heights.
            float leadGap=lead2-frontDriver-radius-MalletTrailingRadius;
            float trailGap=rearDriver-trail1-radius-MalletTrailingRadius;
            float rearTruckGap=trail1-trail2-2*MalletTrailingRadius;
            Check(leadGap>0,"Front carrying wheel overlaps the front driver in straight track");
            Check(trailGap>0,"Rear carrying wheel overlaps the rear driver in straight track");
            Check(rearTruckGap>0,"Rear carrying wheels overlap in straight track");
            return new {passed=true,arrangementId=50,nativeMalletCalls,lead1,lead2,frontDriver,rearDriver,trail1,trail2,driverRadiusM=radius,leadGap,trailGap,rearTruckGap,
                limitation="Extracted production articulated dispatch branch, Mallet measurement, truck methods and Little Boy wrapper with translation-only managed transform stubs and native straight-track bogie/driver inputs. The engine setup and simulation Ensure are stubs. Does not execute full TrainCar/engine setup, wheel physics, animated mesh/vehicle clearance, curves, live coupling or gameplay."};
        }
    }
}

internal static class Program
{
    private static float Coord(JsonElement item,int index)=>item[index].GetSingle();
    private static void Main(string[] args)
    {
        using var frames=JsonDocument.Parse(File.ReadAllText(args[0]));
        using var spacing=JsonDocument.Parse(File.ReadAllText(args[1]));
        using var animation=JsonDocument.Parse(File.ReadAllText(args[2]));
        var transforms=animation.RootElement.GetProperty("transforms").EnumerateArray().ToArray();
        float Driver(int n)=>Coord(transforms.Single(x=>x.GetProperty("path").GetString().EndsWith("/s282_wheels_driving_"+n)).GetProperty("position"),2);
        var result=new RearrangedS282.WheelRearranger().Run(Coord(frames.RootElement.GetProperty("frontBogie"),2),Coord(frames.RootElement.GetProperty("rearBogie"),2),spacing.RootElement.GetProperty("cylinderFront").GetSingle(),Driver(1),Driver(2),Driver(3));
        var text=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true});
        File.WriteAllText(args[3],text);Console.WriteLine(text);
    }
}
