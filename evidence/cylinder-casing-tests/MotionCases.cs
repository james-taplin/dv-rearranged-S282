using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using RearrangedS282;
using UnityEngine;
using DV.Util;

internal static partial class PrairieProgram
{
    internal static int RunMotion(string[] args)
    {
        int scenarios=0, oilSamples=0;
        try
        {
            using var geo=JsonDocument.Parse(File.ReadAllText(args[0]));using var anim=JsonDocument.Parse(File.ReadAllText(args[1]));
            using var particles=JsonDocument.Parse(File.ReadAllText(args[3]));using var fittings=JsonDocument.Parse(File.ReadAllText(args[4]));
            using var spacing=JsonDocument.Parse(File.ReadAllText(args[5]));
            var bank=new Dictionary<string,Mesh>();
            var nativeMeshes=spacing.RootElement.GetProperty("meshes");
            var paths=new[]{"s282_mech_push_rod","s282_mech_push_rod/s282_mech_push_rod_to_cutoff_base","s282_mech_cutoff_rod_hinge",
                "s282_mech_cutoff_rod_hinge/s282_mech_push_rod_to_cutoff_rod","s282_mech_cylinder_rod",
                "s282_mech_wheels_connect/s282_mech_push_rod_to_connect",
                "s282_mech_cutoff_boomerang_high/s282_mech_cutoff_rail_connection/s282_mech_cutoff_to_cylinder_rod"};
            var rig=new Rig(geo.RootElement,anim.RootElement,particles.RootElement,fittings.RootElement,bank,r=>
            {
                r.w.firstRightDriveWheel=Path(r.w.driveR,"s282_wheels_driving_1");r.w.secondRightDriveWheel=Path(r.w.driveR,"s282_wheels_driving_2");
                r.w.spark4L=r.body.Add("spark4L");r.w.spark4R=r.body.Add("spark4R");
                r.w.spark1L=r.body.Add("spark1L");r.w.spark1R=r.body.Add("spark1R");r.w.spark2L=r.body.Add("spark2L");r.w.spark2R=r.body.Add("spark2R");r.w.spark3L=r.body.Add("spark3L");r.w.spark3R=r.body.Add("spark3R");
                foreach(var guide in new[]{r.w.lfCrossheadBracket,r.w.rfCrossheadBracket})
                {
                    var data=fittings.RootElement.GetProperty("parts").EnumerateArray().Single(p=>p.GetProperty("name").GetString()=="native-lod0-guide-"+(guide.name.EndsWith("_l")?"left":"right"));
                    var points=data.GetProperty("vertices").EnumerateArray().Select(V).Select(p=>guide.InverseTransformPoint(r.body.TransformPoint(p))).ToArray();
                    var mesh=new Mesh{name="actual native guide",vertices=points};mesh.RecalculateBounds();guide.GetComponent<MeshFilter>().sharedMesh=mesh;
                }
                foreach(var d in new[]{r.w.driveL,r.w.driveR})
                    foreach(string path in paths)
                        foreach(bool lod in new[]{false,true})
                        {
                            var source=Path(d,path+(lod?"/"+path.Split('/').Last()+"_LOD1":""));
                            string name=path.Split('/').Last()+(lod?"_LOD1":"");
                            Mesh mesh;
                            if(nativeMeshes.TryGetProperty(name,out var data))mesh=new Mesh{name=name,vertices=data.GetProperty("vertices").EnumerateArray().Select(V).ToArray()};
                            else mesh=new Mesh{name=name,vertices=new[]{new Vector3(-.02f,-.02f,0),new Vector3(.02f,.02f,.1f)}};
                            mesh.RecalculateBounds();_=new MeshFilter(source,mesh);source.gameObject.AddComponent<MeshRenderer>();
                        }
            });
            var sourceMeshes=rig.w.transform.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.sharedMesh!=null)
                .Select(f=>f.sharedMesh).Distinct().ToDictionary(m=>m,m=>m.vertices.ToArray());
            var sourceBindings=rig.w.transform.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.sharedMesh!=null).ToDictionary(f=>f.transform,f=>f.sharedMesh);
            var cups=new List<(PositionSyncProvider provider,PositionSyncConsumer consumer,Vector3 local)>();
            foreach(var d in new[]{rig.w.driveL,rig.w.driveR})
                foreach(string p in new[]{"Oiling0","s282_mech_push_rod/Oiling1","s282_mech_wheels_connect/s282_mech_push_rod_to_connect/Oiling2"})
                {
                    var t=d.Find(p);var provider=t.gameObject.AddComponent<PositionSyncProvider>();provider.syncTag=d.name+p;
                    var c=new GameObject("native cup "+provider.syncTag).AddComponent<PositionSyncConsumer>();
                    c.syncTag=provider.syncTag;c.SetProviderTransform(provider);cups.Add((provider,c,t.localPosition));
                }
            rig.Legacy();rig.w.ApplyCasingFor(44);
            var oldCasing=rig.casings.Select(c=>c.f.sharedMesh.vertices.Select(v=>rig.body.InverseTransformPoint(c.f.transform.TransformPoint(v))).ToArray()).ToArray();
            rig.w.RestoreCasings();rig.w.ApplyForward();
            for(int n=0;n<rig.casings.Count;n++)
            {
                var c=rig.casings[n];var actual=c.f.sharedMesh.vertices.Select(v=>rig.body.InverseTransformPoint(c.f.transform.TransformPoint(v))).ToArray();
                for(int i=0;i<actual.Length;i++)Check(Near(actual[i],oldCasing[n][i]+new Vector3(0,0,-.1f)),"Accepted casing shape retained; rear datum returns 0.70 m from 1.13.5");
            }
            Check(Math.Abs(rig.w.driveR.localPosition.z-(-1.3f))<1e-5,"Driver group is 0.70 m behind 1.13.5");
            Check(Near(rig.w.driveR.localScale,new Vector3(1,1.2879f,1.2879f)),"Driver diameter and pitch scale unchanged");
            foreach(var spark in new[]{rig.w.spark1L,rig.w.spark1R,rig.w.spark2L,rig.w.spark2R,rig.w.spark3L,rig.w.spark3R})Check(Near(spark.localPosition,new Vector3(0,0,.8f)),"Contact sparks follow the 0.70 m rearward shift");
            foreach(var item in rig.fittings)
            {
                var output=item.f.sharedMesh.vertices.Select(v=>rig.body.InverseTransformPoint(item.f.transform.TransformPoint(v))).ToArray();
                var triangles=item.f.sharedMesh.triangles;
                float lo=item.native.Min(p=>p.y), hi=item.native.Max(p=>p.y);
                Vector3 Map(Vector3 p){var v=CylinderCasingShape.NormalizePrairie(p,rig.scale,rig.scale);v=new Vector3(v.x,v.y*rig.scale,v.z*rig.scale+rig.offset-.1f);if(item.pipe){float t=Math.Clamp((hi-p.y)/(hi-lo),0,1);t=t*t*(3-2*t);v=p+(v-p)*t;}return v;}
                foreach(var p in item.native){const float h=.001f;var x=(Map(p+Vector3.right*h)-Map(p-Vector3.right*h))/(2*h);var y=(Map(p+Vector3.up*h)-Map(p-Vector3.up*h))/(2*h);var z=(Map(p+Vector3.forward*h)-Map(p-Vector3.forward*h))/(2*h);Check(Vector3.Dot(x,Vector3.Cross(y,z))>.01f,"Forward fitting mapping retains positive local volume");}

                for(int i=0;i<triangles.Length;i+=3)
                {
                    int a=triangles[i],b=triangles[i+1],c=triangles[i+2];
                    var oldN=Vector3.Cross(item.native[b]-item.native[a],item.native[c]-item.native[a]);
                    if(oldN.magnitude<1e-9f)continue;
                    var newN=Vector3.Cross(output[b]-output[a],output[c]-output[a]);
                    Check(newN.magnitude>1e-12f,"Forward-layout pipe/cock triangle does not collapse");
                    var p=(item.native[a]+item.native[b]+item.native[c])/3f;
                    const float step=.01f;
                    var eb=item.native[b]-item.native[a];var ec=item.native[c]-item.native[a];
                    var tangentN=Vector3.Cross((Map(p+eb*step)-Map(p-eb*step))/(2*step),(Map(p+ec*step)-Map(p-ec*step))/(2*step));
                    Check(Vector3.Dot(tangentN,newN)>0,"Fitting triangle agrees with transported surface orientation");
                }
            }
            foreach(var cup in cups)rig.w.BindOilCupPresentation(cup.provider,cup.consumer);
            var retargets=new[]{rig.w.driveL.GetComponent<DriveRetarget>(),rig.w.driveR.GetComponent<DriveRetarget>()};
            var tick=typeof(DriveRetarget).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic);
            foreach(var clip in anim.RootElement.GetProperty("clips").EnumerateArray())
                foreach(var frame in clip.GetProperty("frames").EnumerateArray())
                {
                    NativePose(rig,clip,frame);
                    var before=All(rig.w.driveL).Concat(All(rig.w.driveR)).Where(t=>!t.name.StartsWith("Oiling")&&!t.name.StartsWith("[Rearranged retarget]"))
                        .Select(t=>new Pose(t)).ToArray();
                    // Exercise both possible consumer/retarget LateUpdate orders.
                    foreach(var cup in cups)cup.consumer.Sync();
                    foreach(var retarget in retargets)tick.Invoke(retarget,null);
                    foreach(var p in before)p.Unchanged();
                    foreach(var cup in cups)
                    {
                        var source=cup.provider.transform.parent;
                        var proxy=source.children.FirstOrDefault(t=>t.name.StartsWith("[Rearranged retarget]"));
                        if(proxy!=null)Check(Near(cup.provider.transform.position,proxy.TransformPoint(cup.local)),"Cup follows actual rendered shift/stretch proxy");
                        Check(Near(cup.consumer.transform.position,cup.provider.transform.position),"Native cup sync has no retarget-frame lag");oilSamples++;
                    }
                    foreach(var d in new[]{rig.w.driveL,rig.w.driveR})
                    {
                        foreach(string path in paths.Where(p=>DriveRetarget.NativeWalschaertsRole(p)==DriveRetarget.Role.Stretch))
                        {
                            var source=d.Find(path);var mesh=sourceBindings[source];
                            float pin=mesh.bounds.max.z+Math.Min(0,mesh.bounds.min.z);
                            var proxy=source.children.First(t=>t.name.StartsWith("[Rearranged retarget]"));
                            float shift=d.GetComponent<DriveRetarget>().Shift;
                            var actual=proxy.TransformPoint(new Vector3(0,0,pin+shift));
                            var expected=source.TransformPoint(new Vector3(0,0,pin))+d.TransformVector(new Vector3(0,0,shift));
                            Check(Near(actual,expected,.00025f),"Shortened rod far pin follows cylinder-side shift at every cutoff pose");
                        }
                        scenarios++;
                    }
                    foreach(var cup in cups)cup.consumer.Sync();
                }
            foreach(var pair in sourceMeshes)
                for(int i=0;i<pair.Value.Length;i++)Check(Near(pair.Key.vertices[i],pair.Value[i],1e-9f),"Original geometry immutable");
            Check(rig.w.MaximumOilGuideGap()<.035f,"Fixed cups have an actual guide surface below them");
            // Reflected guides exchange LMS side labels on articulated layouts.
            foreach(var guide in new[]{rig.w.lfCrossheadBracket,rig.w.rfCrossheadBracket})guide.localScale=new(-1,.65f,1.33f);
            rig.w.RefreshOilMounts();
            Check(rig.w.MaximumOilGuideGap()<.035f,"Reflected/resized guides retain seated oil cups");
            foreach(var cup in cups.Where(c=>c.provider.transform.name=="Oiling0"))
                Check(Math.Sign(rig.body.InverseTransformPoint(cup.provider.transform.position).x)==Math.Sign(cup.provider.transform.parent.localScale.x),"Guide cup stays on its physical side after guide reflection");
            foreach(var d in new[]{rig.w.driveL,rig.w.driveR})DriveRetarget.Unbind(d);
            foreach(var cup in cups.Where(c=>c.provider.transform.name!="Oiling0"))Check(Near(cup.provider.transform.localPosition,cup.local),"Retargeted oil provider local pose restored on unbind");
            Check(!All(rig.w.transform).Any(t=>t.name.StartsWith("[Rearranged retarget]")),"No rendered proxies left after cleanup");
            rig.Restore();foreach(var cup in cups)Check(Near(cup.provider.transform.localPosition,cup.local),"All native oil-provider poses restored with stock geometry");
            foreach(float shift in new[]{.5f,-1.52f})
            {
                rig.Legacy();
                foreach(var d in new[]{rig.w.driveL,rig.w.driveR})DriveRetarget.Bind(d,shift,shift>0?DriveRetarget.NativeWalschaertsRole:DriveRetarget.FranklinRole,rig.w.transform);
                rig.w.Apply();rig.w.RefreshOilMounts();
                foreach(var cup in cups)rig.w.BindOilCupPresentation(cup.provider,cup.consumer);
                foreach(var clip in anim.RootElement.GetProperty("clips").EnumerateArray())foreach(var frame in clip.GetProperty("frames").EnumerateArray())
                {
                    NativePose(rig,clip,frame);foreach(var cup in cups)cup.consumer.Sync();
                    foreach(var d in new[]{rig.w.driveL,rig.w.driveR})tick.Invoke(d.GetComponent<DriveRetarget>(),null);
                    foreach(var cup in cups){Check(Near(cup.consumer.transform.position,cup.provider.transform.position),"Positive/native and negative/Franklin cup sync stays current");oilSamples++;}
                    scenarios+=2;
                }
                Check(rig.w.MaximumOilGuideGap()<.035f,"Retargeted cylinder-side guides retain a seated fixed cup");
                foreach(var d in new[]{rig.w.driveL,rig.w.driveR})DriveRetarget.Unbind(d);rig.Restore();
            }
            foreach(int id in Enumerable.Range(0,47))Check(WheelRearranger.UsesReservoirClearance(id)==new[]{12,13,14,44}.Contains(id),"Clearance scope is exactly three high-speed variants and Prairie");
            foreach(int id in new[]{12,13,14,44})
            {
                rig.Restore();rig.w.currentWA=(WheelArrangementType)id;
                bool prairie=id==44;float oldScale=prairie?1.2879f:1.198f,oldRadius=prairie?.9144f:.8509f;
                rig.Legacy();
                if(!prairie)foreach(var d in new[]{rig.w.driveL,rig.w.driveR}){d.localPosition=new(0,0,-1.5f);d.localScale=new(d==rig.w.driveL?-1:1,oldScale,oldScale);}
                var anchor=prairie?rig.w.secondRightDriveWheel:rig.w.firstRightDriveWheel;
                float oldAnchor=rig.body.InverseTransformPoint(anchor.position).z;
                float oldRear=-2.1f-.1f+oldScale*CylinderCasingShape.NativeRearFaceZ;
                rig.w.ApplyClearanceCase(id);
                Check(Math.Abs(rig.w.driveR.localScale.z-1.11f)<1e-6f&&Math.Abs(rig.w.driveR.localScale.y-1.11f)<1e-6f,"Only selected large mechanisms use measured clearance scale");
                Check(Math.Abs(rig.w.LastRadius-oldRadius*1.11f/oldScale)<1e-6f,"Simulation radius follows wheel reduction");
                Check(Math.Abs(rig.body.InverseTransformPoint(anchor.position).z-oldAnchor-(prairie?.8f:0))<.0001f,"Front active driver is 0.70 m behind 1.13.5, high-speed positions unchanged");
                if(prairie)Check(Math.Abs(rig.body.InverseTransformPoint(rig.w.CylinderReferencePoint()).z-oldRear)<.0001f,"Prairie rear casing datum is 0.70 m behind 1.13.5");
                foreach(var c in rig.casings){var v=c.f.sharedMesh.vertices.Select(p=>rig.body.InverseTransformPoint(c.f.transform.TransformPoint(p))).ToArray();Check(Math.Abs((v.Max(p=>p.z)-v.Min(p=>p.z))-(c.native.Max(p=>p.z)-c.native.Min(p=>p.z)))<.0001f,"Resized mechanism retains native casing length");}
                foreach(var cup in cups)rig.w.BindOilCupPresentation(cup.provider,cup.consumer);
                foreach(var clip in anim.RootElement.GetProperty("clips").EnumerateArray())foreach(var frame in clip.GetProperty("frames").EnumerateArray())
                {
                    NativePose(rig,clip,frame);
                    var before=All(rig.w.driveL).Concat(All(rig.w.driveR)).Where(t=>!t.name.StartsWith("Oiling")&&!t.name.StartsWith("[Rearranged retarget]")).Select(t=>new Pose(t)).ToArray();
                    foreach(var cup in cups)cup.consumer.Sync();
                    foreach(var d in new[]{rig.w.driveL,rig.w.driveR}){var ret=d.GetComponent<DriveRetarget>();if(ret!=null)tick.Invoke(ret,null);scenarios++;}
                    foreach(var pose in before)pose.Unchanged();
                    foreach(var cup in cups){Check(Near(cup.consumer.transform.position,cup.provider.transform.position),"Clearance layout cups remain on current native/rendered motion");oilSamples++;}
                }
                Check(rig.w.MaximumOilGuideGap()<.035f,"Clearance layout guide cups retain mounting surface");
                foreach(var d in new[]{rig.w.driveL,rig.w.driveR})DriveRetarget.Unbind(d);
                rig.Restore();
                foreach(var cup in cups)Check(Near(cup.provider.transform.localPosition,cup.local),"Clearance layout oil providers restore exactly");
            }
            rig.w.DisposeCasings();rig.w.Dispose();
            File.WriteAllText(args[2],JsonSerializer.Serialize(new{passed=true,checks,nativePoseScenarios=scenarios,oilPoseSamples=oilSamples,
                driverForwardM=.8,cylinderRearDatumForwardM=-.1,completePrairieShiftFrom1135M=-.7,clearanceScale=1.11,clearanceArrangements=new[]{12,13,14,44},
                limitations="Actual production spacing, retarget, casing/support and oil-mount code with managed Unity stand-ins; native clip and rod/casing mesh data. No game rendering, oil-can interaction or native controller lifecycle run."},new JsonSerializerOptions{WriteIndented=true}));
            Console.WriteLine($"Prairie motion: {checks} checks; {scenarios} native poses; {oilSamples} cup samples passed.");return 0;
        }
        catch(Exception ex){File.WriteAllText(args[2],JsonSerializer.Serialize(new{passed=false,checks,error=ex.ToString()}));Console.Error.WriteLine(ex);return 1;}
    }
}
internal static class MotionHarness{static int Main(string[] args)=>PrairieProgram.RunMotion(args);}

