using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using RearrangedS282;
using UnityEngine;

namespace SpacingFixture
{
    internal sealed class Snapshot
    {
        internal float Scale,Offset,DriveOffset,Radius,Lead1,Lead2,Trail1,Trail2;
        internal float[] Drivers;
    }
    internal class RigBase
    {
        internal readonly Transform root=new GameObject("car").transform,body,rearBogie;
        protected Transform firstLeftDriveWheel,secondLeftDriveWheel,thirdLeftDriveWheel,fourthLeftDriveWheel,fifthLeftDriveWheel,sixthLeftDriveWheel;
        protected Transform MalletBody=>body;
        protected const float MalletTrailingRadius=.355f,MalletEngineGap=.1f;
        protected static readonly Vector3 TriplexTrailingLimit=new(0,.36f,-3.1f);
        readonly float cylinderFront;
        protected RigBase(JsonElement data,JsonElement animation)
        {
            body=Program.Child(root,"body");rearBogie=Program.Child(root,"rear");rearBogie.localPosition=Program.V(data.GetProperty("rearBogiePosition"));
            cylinderFront=data.GetProperty("cylinderFront").GetSingle();
            Transform Wheel(int n)
            {
                var t=Program.Child(body,"wheel"+n);
                t.localPosition=n>4?new Vector3(0,.71f,n==5?1.65f:.1f):Program.V(animation.GetProperty("transforms").EnumerateArray().Single(x=>x.GetProperty("path").GetString().EndsWith("/s282_wheels_driving_"+n)).GetProperty("position"));
                return t;
            }
            firstLeftDriveWheel=Wheel(1);secondLeftDriveWheel=Wheel(2);thirdLeftDriveWheel=Wheel(3);fourthLeftDriveWheel=Wheel(4);fifthLeftDriveWheel=Wheel(5);sixthLeftDriveWheel=Wheel(6);
        }
        protected float MeasureNativeCylinderFront()=>cylinderFront;
    }
    internal static class Program
    {
        static int checks,poses;
        internal static void Check(bool ok,string text){checks++;if(!ok)throw new InvalidOperationException(text);}
        internal static Vector3 V(JsonElement x)=>new(x[0].GetSingle(),x[1].GetSingle(),x[2].GetSingle());
        static Quaternion Q(JsonElement x)=>new(x[0].GetSingle(),x[1].GetSingle(),x[2].GetSingle(),x[3].GetSingle());
        internal static Transform Child(Transform p,string name){var t=new GameObject(name).transform;t.SetParent(p,false);return t;}
        static Transform Path(Transform p,string path){foreach(string s in path.Split('/'))p=p.Find(s)??Child(p,s);return p;}
        static void Near(float a,float b,string text,float tolerance=3e-5f)=>Check(Math.Abs(a-b)<tolerance,text+": "+a+" vs "+b);
        static void Near(Vector3 a,Vector3 b,string text)=>Check((a-b).magnitude<.0006f,text+": error "+(a-b).magnitude);
        sealed class AnimatedRig
        {
            internal readonly Transform root=new GameObject("car").transform,drive;
            internal readonly Dictionary<string,Mesh> originals=new();
            internal readonly Dictionary<string,float> lengths=new();
            internal readonly LODGroup lod;
            internal readonly Renderer[] renderers;
            internal readonly float shift;
            internal AnimatedRig(JsonElement data,JsonElement animation,Snapshot layout,bool left)
            {
                shift=layout.Offset-layout.DriveOffset;
                drive=Child(root,"drive");drive.localPosition=new(0,0,layout.DriveOffset);drive.localScale=new(left?-1:1,layout.Scale,layout.Scale);
                foreach(var t in animation.GetProperty("transforms").EnumerateArray())
                {
                    string path=t.GetProperty("path").GetString().Split("/LocoS282A_Drivetrain R/").Last();
                    if(!path.Contains("/")&&path.EndsWith("LocoS282A_Drivetrain R"))continue;
                    var part=Path(drive,path);part.localPosition=V(t.GetProperty("position"));part.localRotation=Q(t.GetProperty("rotation"));part.localScale=V(t.GetProperty("scale"));
                }
                var meshes=data.GetProperty("meshes");
                foreach(string path in new[]{"s282_mech_push_rod","s282_mech_cutoff_rod_hinge","s282_mech_cylinder_rod",
                    "s282_mech_push_rod/s282_mech_push_rod_to_cutoff_base","s282_mech_cutoff_rod_hinge/s282_mech_push_rod_to_cutoff_rod",
                    "s282_mech_wheels_connect/s282_mech_push_rod_to_connect","s282_mech_wheels_connect/s282_mech_push_rod_to_connect/s282_mech_push_rod_to_connect_LOD1",
                    "s282_mech_cutoff_boomerang_high/s282_mech_cutoff_rail_connection/s282_mech_cutoff_to_cylinder_rod",
                    "s282_mech_cutoff_boomerang_high/s282_mech_cutoff_rail_connection/s282_mech_cutoff_to_cylinder_rod/s282_mech_cutoff_to_cylinder_rod_LOD1"})
                {
                    var part=Path(drive,path);Mesh mesh;
                    if(meshes.TryGetProperty(part.name,out var m))
                        mesh=new Mesh{name=part.name,vertices=m.GetProperty("vertices").EnumerateArray().Select(V).ToArray(),bounds=new Bounds{min=V(m.GetProperty("boundsMin")),max=V(m.GetProperty("boundsMax"))}};
                    else {mesh=new Mesh{name=part.name,vertices=new[]{Vector3.zero,new Vector3(.02f,.02f,.1f)}};mesh.RecalculateBounds();}
                    part.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;part.gameObject.AddComponent<MeshRenderer>();originals.Add(path,mesh);
                    if(path.Contains("to_connect")||path.Contains("to_cylinder_rod"))lengths[path]=mesh.bounds.max.z+Math.Min(0,mesh.bounds.min.z);
                }
                renderers=drive.GetComponentsInChildren<MeshRenderer>(true).Cast<Renderer>().ToArray();lod=root.gameObject.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.5f,renderers)});
            }
            internal void Pose(JsonElement clip,JsonElement frame)
            {
                var values=frame.GetProperty("values");
                foreach(var b in clip.GetProperty("bindings").EnumerateArray())
                {
                    var p=drive.Find(b.GetProperty("path").GetString());if(p==null)continue;
                    var ids=b.GetProperty("indices").EnumerateArray().Select(x=>x.GetInt32()).ToArray();var defaults=b.GetProperty("first").EnumerateArray().Select(x=>x.GetSingle()).ToArray();
                    var v=ids.Select((id,i)=>values.TryGetProperty(id.ToString(),out var x)?x.GetSingle():defaults[i]).ToArray();
                    switch(b.GetProperty("attribute").GetInt32()) {case 1:p.localPosition=new(v[0],v[1],v[2]);break;case 2:p.localRotation=new(v[0],v[1],v[2],v[3]);break;case 3:p.localScale=new(v[0],v[1],v[2]);break;}
                }
            }
            internal Transform Proxy(string path)=>drive.Find(path).children.Single(t=>t.name.StartsWith("[Rearranged retarget]"));
        }
        static void Main(string[] args)
        {
            using var d=JsonDocument.Parse(File.ReadAllText(args[0]));using var a=JsonDocument.Parse(File.ReadAllText(args[1]));var data=d.RootElement;var anim=a.RootElement;
            var oldRig=new Baseline.WheelRearranger(data,anim);var newRig=new Candidate.WheelRearranger(data,anim);
            foreach(float yaw in new[]{-5f,0f,5f})
            {
                oldRig.root.localPosition=new(213,4,-321);newRig.root.localPosition=oldRig.root.localPosition;
                oldRig.root.localRotation=newRig.root.localRotation=Quaternion.AngleAxis(37,Vector3.up);
                oldRig.rearBogie.localRotation=newRig.rearBogie.localRotation=Quaternion.AngleAxis(yaw,Vector3.up);
                var old=oldRig.Capture();var now=newRig.Capture();
                Near(now.Scale,old.Scale,"Driver scale retained");Near(now.Radius,old.Radius,"Driver radius retained");Near(now.Offset,old.Offset,"Cylinder mapping retained");
                Near(now.Lead1,old.Lead1,"Leading truck front axle retained");Near(now.Lead2,old.Lead2,"Leading truck rear axle retained");Near(now.Trail1,old.Trail1,"Trailing truck front axle retained");Near(now.Trail2,old.Trail2,"Trailing truck rear axle retained");
                for(int i=0;i<7;i++){Near(now.Drivers[i],old.Drivers[i]-.25f,"Driver "+i+" moves back 0.25 m");if(i>0)Near(now.Drivers[i-1]-now.Drivers[i],old.Drivers[i-1]-old.Drivers[i],"Driver pitch retained");}
                float rearGap=now.Drivers[6]-now.Radius-(now.Trail1+.355f);Check(rearGap>.18f,"Rear tyre clearance remains conservative");
            }
            oldRig.rearBogie.localRotation=newRig.rearBogie.localRotation=Quaternion.identity;
            var baseline=oldRig.Capture();var candidate=newRig.Capture();
            Near(candidate.Lead2-.355f-(candidate.Drivers[0]+candidate.Radius),.55f,"Leading tyre gap 0.55 m");
            Near(candidate.Drivers[6]-candidate.Radius-(candidate.Trail1+.355f),.20f,"Trailing tyre gap 0.20 m");
            var update=typeof(DriveRetarget).GetMethod("LateUpdate",BindingFlags.NonPublic|BindingFlags.Instance);
            float worstEndpoint=0,maxNewBreathing=0;
            foreach(bool left in new[]{false,true})
            {
                var old=new AnimatedRig(data,anim,baseline,left);var now=new AnimatedRig(data,anim,candidate,left);
                old.root.localPosition=now.root.localPosition=new(250,6,-410);old.root.localRotation=now.root.localRotation=Quaternion.AngleAxis(-71,Vector3.up);
                var originalVertices=now.originals.ToDictionary(x=>x.Key,x=>x.Value.vertices.ToArray());
                var oldRetarget=DriveRetarget.Bind(old.drive,old.shift/baseline.Scale,DriveRetarget.NativeWalschaertsRole,old.root);
                var newRetarget=DriveRetarget.Bind(now.drive,now.shift/candidate.Scale,DriveRetarget.NativeWalschaertsRole,now.root);
                var cup=now.drive.Find("s282_mech_push_rod/Oiling1");var cupLocal=cup.localPosition;
                Candidate.WheelRearranger.MoveCup(now.drive,now.root.TransformVector(Vector3.forward*now.shift));
                Check(now.lod.GetLODs()[0].renderers.All(r=>r.name.StartsWith("[Rearranged retarget]")),"LOD uses retargeted renderers");
                foreach(var clip in anim.GetProperty("clips").EnumerateArray())foreach(var frame in clip.GetProperty("frames").EnumerateArray())
                {
                    old.Pose(clip,frame);now.Pose(clip,frame);update.Invoke(oldRetarget,null);update.Invoke(newRetarget,null);poses++;
                    foreach(string path in now.originals.Keys)
                    {
                        var source=now.drive.Find(path);var proxy=now.Proxy(path);var oldProxy=old.Proxy(path);
                        if(!now.lengths.TryGetValue(path,out float pin))Near(proxy.position,oldProxy.position,"Cylinder-side rendered pivot stays fixed");
                        else
                        {
                            float newLength=pin+newRetarget.Shift,oldLength=pin+oldRetarget.Shift;
                            var end=proxy.TransformPoint(new Vector3(0,0,newLength));var oldEnd=oldProxy.TransformPoint(new Vector3(0,0,oldLength));
                            worstEndpoint=Math.Max(worstEndpoint,(end-oldEnd).magnitude);Near(end,oldEnd,"Rod cylinder-side endpoint retained across added shift");
                            Near(proxy.position,source.position,"Rod driving pin stays on native animated pivot");
                            maxNewBreathing=Math.Max(maxNewBreathing,Math.Abs(proxy.localScale.z-1f));
                        }
                    }
                    var push=now.drive.Find("s282_mech_push_rod");
                    Near(cup.position,push.TransformPoint(cupLocal)+now.root.TransformVector(Vector3.forward*now.shift),"Crosshead oil cup follows the visible shifted crosshead");
                }
                DriveRetarget.Unbind(now.drive);DriveRetarget.Unbind(old.drive);
                foreach(var p in now.originals){Check(ReferenceEquals(now.drive.Find(p.Key).GetComponent<MeshFilter>().sharedMesh,p.Value),"Original source mesh restored");Check(p.Value.vertices.SequenceEqual(originalVertices[p.Key]),"Shared rod source vertices immutable");}
                Check(now.lod.GetLODs()[0].renderers.SequenceEqual(now.renderers),"LOD renderer list restored");
                cup.localPosition=cupLocal; // The production GeometryState performs this on switching/pooling.
                for(int repeat=0;repeat<3;repeat++){DriveRetarget.Bind(now.drive,now.shift/candidate.Scale,DriveRetarget.NativeWalschaertsRole,now.root);DriveRetarget.Unbind(now.drive);}
                Check(now.drive.GetComponents<DriveRetarget>().Length==0,"Repeated retarget cleanup leaves no component");
            }
            Check(worstEndpoint<.0006f,"Native endpoint continuity tolerance");
            var report=new{passed=true,checks,nativePoseScenarios=poses,additionalDriverShiftM=.25f,frontTyreGapM=.55f,rearTyreGapM=.20f,worstEndpointDifferenceM=worstEndpoint,maximumProxyLengthScaleDeviation=maxNewBreathing,
                limitations="Managed transforms and sampled native animation. No Unity rendering/physics run; inherited variable-length visual retarget remains an approximation. In-game shoe/frame clearance and appearance need testing."};
            File.WriteAllText(args[2],JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(report));
        }
    }
}
