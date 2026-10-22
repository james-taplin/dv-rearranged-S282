using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RearrangedS282;
using UnityEngine;

internal static partial class PrairieProgram
{
    static int checks, poses;
    static void Check(bool v,string message){checks++;if(!v)throw new InvalidOperationException(message);}
    static bool Near(Vector3 a,Vector3 b,float e=.00015f)=>(a-b).magnitude<e;
    static Vector3 V(JsonElement a)=>new(a[0].GetSingle(),a[1].GetSingle(),a[2].GetSingle());
    static Quaternion Q(JsonElement a)=>new(a[0].GetSingle(),a[1].GetSingle(),a[2].GetSingle(),a[3].GetSingle());
    static IEnumerable<Transform> All(Transform t){yield return t;foreach(var c in t.children)foreach(var x in All(c))yield return x;}
    static Transform Path(Transform t,string path){foreach(var n in path.Split('/'))t=t.children.FirstOrDefault(c=>c.name==n)??t.Add(n);return t;}
    sealed class Pose
    {
        internal Transform t;Vector3 p,s;Quaternion r;Mesh m;bool active;
        internal Pose(Transform t){this.t=t;p=t.localPosition;s=t.localScale;r=t.localRotation;m=t.gameObject.filter?.sharedMesh;active=t.gameObject.activeSelf;}
        internal void Restore(){t.localPosition=p;t.localScale=s;t.localRotation=r;t.gameObject.SetActive(active);if(t.gameObject.filter!=null)t.gameObject.filter.sharedMesh=m;}
        internal void Unchanged(){Check(Near(t.localPosition,p)&&Near(t.localScale,s)&&t.localRotation.x==r.x&&t.localRotation.y==r.y&&t.localRotation.z==r.z&&t.localRotation.w==r.w,"Animated transform untouched: "+t.name);}
    }
    sealed class Rig
    {
        internal WheelRearranger w=new();internal Transform body;
        internal List<(MeshFilter f,Vector3[] native,bool pipe)> fittings=new();
        internal List<(MeshFilter f,Vector3[] native)> casings=new();
        internal List<(MeshFilter f,Vector3[] native)> guides=new();
        internal List<(Transform t,Vector3 native,Vector3 scale,Quaternion rotation)> emitters=new();
        internal List<Pose> stock=new();internal List<Mesh> sources=new();
        internal float scale=1.2879f,offset=-2.1f;
        internal Rig(JsonElement geometry,JsonElement animation,JsonElement particles,JsonElement fittingGeometry,Dictionary<string,Mesh> bank,Action<Rig> preparation=null)
        {
            w.transform=new GameObject("S282 fixture").transform;
            body=Path(w.transform,"LocoS282A_Body/MovingParts_LOD0");
            w.driveR=body.Add("LocoS282A_Drivetrain R");w.driveL=body.Add("LocoS282A_Drivetrain L");w.driveL.localScale=new(-1,1,1);
            foreach(var node in animation.GetProperty("transforms").EnumerateArray())
            {
                string p=node.GetProperty("path").GetString();const string marker="/LocoS282A_Drivetrain R";
                int start=p.IndexOf(marker,StringComparison.Ordinal);if(start<0)continue;
                string relative=p[(start+marker.Length)..].TrimStart('/');if(relative.Length==0)continue;
                foreach(var drive in new[]{w.driveL,w.driveR}){var t=Path(drive,relative);t.localPosition=V(node.GetProperty("position"));t.localScale=V(node.GetProperty("scale"));t.localRotation=Q(node.GetProperty("rotation"));}
            }
            w.lCrosshead=w.driveL.Find("s282_mech_push_rod");w.rCrosshead=w.driveR.Find("s282_mech_push_rod");
            MeshFilter Add(Transform split,string name,Vector3[] local,int[] triangles)
            {
                string key=split.name+name;
                if(!bank.TryGetValue(key,out var mesh)){mesh=new Mesh{name=key,vertices=local,triangles=triangles,uv=Enumerable.Repeat(.5f,local.Length).ToArray(),colors=new[]{.4f,.8f}};bank.Add(key,mesh);}
                sources.Add(mesh);return new MeshFilter(split.Add(name),mesh);
            }
            for(int lod=0;lod<2;lod++)
            {
                var split=Path(w.transform,"LocoS282A_Body/Static_LOD"+lod+"/s282a_split_body"+(lod==0?"(Clone)":"_LOD1(Clone)"));
                float offset=lod==0?4.88f:4.885f;split.localPosition=new(0,0,offset);
                var parts=geometry.GetProperty("sources")[lod].GetProperty("parts");
                foreach(var part in parts.EnumerateArray())
                {
                    string label=part.GetProperty("name").GetString();bool cock=label.EndsWith("-cocks");
                    string name=cock?"s282a_cylinder_cocks":"s282a_cylinder_"+(label.EndsWith("-left")?"l":"r");
                    var native=part.GetProperty("vertices").EnumerateArray().Select(V).ToArray();
                    var f=Add(split,name,native.Select(p=>p-new Vector3(0,0,offset)).ToArray(),part.GetProperty("triangles").EnumerateArray().SelectMany(a=>a.EnumerateArray().Select(i=>i.GetInt32())).ToArray());
                    if(cock)fittings.Add((f,native,false));else casings.Add((f,native));
                }
                foreach(string side in new[]{"l","r"})
                {
                    string label="native-lod"+lod+"-guide-"+(side=="l"?"left":"right");
                    var data=fittingGeometry.GetProperty("parts").EnumerateArray().Single(p=>p.GetProperty("name").GetString()==label);
                    var native=data.GetProperty("vertices").EnumerateArray().Select(V).ToArray();
                    var triangles=data.GetProperty("triangles").EnumerateArray().SelectMany(a=>a.EnumerateArray().Select(i=>i.GetInt32())).ToArray();
                    var f=Add(split,"s282a_crosshead_guide_"+side,native.Select(p=>p-new Vector3(0,0,offset)).ToArray(),triangles);
                    guides.Add((f,native));
                }
                foreach(string name in new[]{"s282a_lifting_arm_support_l","s282a_lifting_arm_support_r","s282a_reach_rod_support"})split.Add(name);
                if(lod==0){w.lLiftingArmSupport=split.Find("s282a_lifting_arm_support_l");w.rLiftingArmSupport=split.Find("s282a_lifting_arm_support_r");w.reachRodSupport=split.Find("s282a_reach_rod_support");w.lfCrossheadBracket=split.Find("s282a_crosshead_guide_l");w.rfCrossheadBracket=split.Find("s282a_crosshead_guide_r");}
                foreach(string side in new[]{"l","r"})
                {
                    string name="native-lod"+lod+"-pipe-"+(side=="l"?"left":"right");
                    var data=fittingGeometry.GetProperty("parts").EnumerateArray().Single(p=>p.GetProperty("name").GetString()==name);
                    var native=data.GetProperty("vertices").EnumerateArray().Select(V).ToArray();
                    var triangles=data.GetProperty("triangles").EnumerateArray().SelectMany(a=>a.EnumerateArray().Select(i=>i.GetInt32())).ToArray();
                    var f=Add(split,"s282a_dry_pipe_"+side,native.Select(p=>p-new Vector3(0,0,offset)).ToArray(),triangles);
                    fittings.Add((f,native,true));
                }
            }
            foreach(var record in particles.GetProperty("transforms").EnumerateArray())
            {
                var t=Path(w.transform,record.GetProperty("path").GetString());t.localPosition=V(record.GetProperty("position"));
                t.localScale=V(record.GetProperty("scale"));t.localRotation=Q(record.GetProperty("rotation"));
                if(record.GetProperty("particle").GetBoolean()){_=new ParticleSystem(t);emitters.Add((t,t.position,t.localScale,t.localRotation));}
            }
            foreach(string path in new[]{"[colliders]/[walkable]/[old]/Exterior/cylinder","[colliders]/[walkable]/[old]/Exterior/cylinder (1)","[colliders]/[walkable]/[old]/Exterior/area_big_cylinder","[colliders]/[items]/[old]/Exterior/area_big_cylinder"})Path(w.transform,path);
            preparation?.Invoke(this);
            stock=All(w.transform).Select(t=>new Pose(t)).ToList();w.PreparePrairie();
        }
        internal void Restore(){w.RestoreCasings();foreach(var p in stock)p.Restore();}
        internal void Legacy(bool four=false,bool polished=true)
        {
            scale=four?1.42f:1.2879f;offset=four?-3.935f:-2.1f;
            w.driveL.localPosition=w.driveR.localPosition=new(0,0,offset);w.driveL.localScale=new(-1,scale,scale);w.driveR.localScale=new(1,scale,scale);
            if(!four && polished){w.Apply();return;}
            foreach(var item in casings)item.f.transform.localPosition=new(0,four?.3f:.21f,0);
            foreach(var item in fittings.Where(f=>!f.pipe))item.f.transform.localPosition=new(0,four?.3f:.21f,0);
            w.lCrosshead.localScale=w.rCrosshead.localScale=new(1,four?.7f:.78f,1);
            if(four)foreach(var drive in new[]{w.driveL,w.driveR})drive.Find("s282_mech_wheels_connect/s282_mech_push_rod_to_connect").localPosition=new(.1f,0,-1.55f);
        }
    }
    static void NativePose(Rig rig,JsonElement clip,JsonElement frame)
    {
        var dynamic=frame.GetProperty("values");
        foreach(var b in clip.GetProperty("bindings").EnumerateArray())
        {
            string path=b.GetProperty("path").GetString();var indices=b.GetProperty("indices").EnumerateArray().Select(x=>x.GetInt32()).ToArray();var constants=b.GetProperty("first").EnumerateArray().Select(x=>x.GetSingle()).ToArray();
            var a=indices.Select((i,k)=>dynamic.TryGetProperty(i.ToString(),out var v)?v.GetSingle():constants[k]).ToArray();
            foreach(var d in new[]{rig.w.driveL,rig.w.driveR}){var t=d.Find(path);Check(t!=null,"Native binding available");switch(b.GetProperty("attribute").GetInt32()){case 1:t.localPosition=new(a[0],a[1],a[2]);break;case 2:t.localRotation=new(a[0],a[1],a[2],a[3]);break;case 3:t.localScale=new(a[0],a[1],a[2]);break;}}
        }
    }
    static void Inspect(Rig rig)
    {
        float s=rig.scale;var map=Matrix4x4.TRS(new(0,0,rig.offset),Quaternion.identity,new(1,s,s));
        foreach(var item in rig.casings)
        {
            var current=item.f.sharedMesh.vertices.Select(v=>rig.body.InverseTransformPoint(item.f.transform.TransformPoint(v))).ToArray();
            Check(Math.Abs((current.Max(p=>p.z)-current.Min(p=>p.z))-(item.native.Max(p=>p.z)-item.native.Min(p=>p.z)))<.0001,"Whole casing has native length");
            float expectedHeight=item.native.Max(p=>p.y)-item.native.Min(p=>p.y)+(s-1)*(CylinderCasingShape.NativeValveY-CylinderCasingShape.NativePistonY);
            Check(Math.Abs(current.Max(p=>p.y)-current.Min(p=>p.y)-expectedHeight)<.0001f,"Native outer barrel height plus the retained extra bore separation");
            for(int i=0;i<current.Length;i++)Check(Near(current[i],map.MultiplyPoint3x4(CylinderCasingShape.NormalizePrairie(item.native[i],s,s))),"Production casing matches full normalization");
        }
        foreach(var item in rig.fittings)
        {
            var current=item.f.sharedMesh.vertices;float lo=item.native.Min(p=>p.y),hi=item.native.Max(p=>p.y);
            var bodyPoints=new Vector3[current.Length];
            for(int i=0;i<current.Length;i++)
            {
                var p=item.native[i];var expected=map.MultiplyPoint3x4(CylinderCasingShape.NormalizePrairie(p,s,s));
                if(item.pipe){float a=Math.Clamp((hi-p.y)/(hi-lo),0,1);a=a*a*(3-2*a);expected=p+(expected-p)*a;}
                bodyPoints[i]=rig.body.InverseTransformPoint(item.f.transform.TransformPoint(current[i]));
                Check(Near(bodyPoints[i],expected),"Pipe/cock follows normalized casing; boiler endpoint remains native");
            }
            var indices=item.f.sharedMesh.triangles;
            for(int i=0;i<indices.Length;i+=3)
            {
                int a=indices[i],b=indices[i+1],c=indices[i+2];
                var oldN=Vector3.Cross(item.native[b]-item.native[a],item.native[c]-item.native[a]);
                if(oldN.magnitude<1e-9f)continue;
                var newN=Vector3.Cross(bodyPoints[b]-bodyPoints[a],bodyPoints[c]-bodyPoints[a]);
                Check(newN.magnitude>1e-12f,"Actual fitting triangle not collapsed");
                Check(Vector3.Dot(oldN,newN)>0,"Actual fitting triangle normal did not turn over 90 degrees");
            }
        }
        foreach(var e in rig.emitters)
        {
            Check(Near(rig.body.InverseTransformPoint(e.t.position),map.MultiplyPoint3x4(CylinderCasingShape.NormalizePrairie(e.native,s,s))),"Actual native particle outlet follows casing");
            Check(Near(e.t.localScale,e.scale)&&e.t.localRotation.x==e.rotation.x&&e.t.localRotation.y==e.rotation.y&&e.t.localRotation.z==e.rotation.z&&e.t.localRotation.w==e.rotation.w,"Native emitter size and direction retained");
        }
    }
    static int Main(string[] args)
    {
        try
        {
            using var geo=JsonDocument.Parse(File.ReadAllText(args[0]));using var anim=JsonDocument.Parse(File.ReadAllText(args[1]));using var particles=JsonDocument.Parse(File.ReadAllText(args[3]));using var fittingGeometry=JsonDocument.Parse(File.ReadAllText(args[4]));
            var bank=new Dictionary<string,Mesh>();var a=new Rig(geo.RootElement,anim.RootElement,particles.RootElement,fittingGeometry.RootElement,bank);var b=new Rig(geo.RootElement,anim.RootElement,particles.RootElement,fittingGeometry.RootElement,bank);
            var sources=bank.Values.ToDictionary(m=>m,m=>m.vertices.ToArray());
            a.Legacy();b.Legacy();var bBefore=b.casings.Concat(b.fittings.Select(f=>(f.f,f.native))).Select(c=>c.f.sharedMesh).ToArray();
            var moving=All(a.w.driveR).Concat(All(a.w.driveL)).Select(t=>new Pose(t)).ToArray();
            a.w.ApplyCasingFor(44);foreach(var p in moving)p.Unchanged();Check(a.w.CylinderCasingMeshCount==4,"Both casing LODs and both sides corrected");Inspect(a);
            Check(bBefore.SequenceEqual(b.casings.Concat(b.fittings.Select(f=>(f.f,f.native))).Select(c=>c.f.sharedMesh)),"Second car untouched while source meshes shared");
            for(int cycle=0;cycle<3;cycle++)
            {
                var temporary=a.casings.Select(c=>c.f.sharedMesh).Concat(a.fittings.Select(f=>f.f.sharedMesh)).ToArray();a.Restore();
                Check(temporary.All(m=>m.destroyed),"Per-car meshes destroyed when switching");Check(a.w.CylinderCasingMeshCount==0,"Casing records cleared");
                foreach(var e in a.emitters)Check(Near(e.t.position,e.native),"Particle local pose restored");
                a.Legacy();a.w.transform.localPosition=new(12,-1,7);a.w.transform.localRotation=Quaternion.AngleAxis(37,new(0,1,0));a.w.transform.localScale=new(1.15f,1.15f,1.15f);
                a.w.ApplyCasingFor(44);Inspect(a);
            }
            a.Restore();MainSettings(false);a.Legacy();a.w.ApplyCasingFor(44);Check(a.w.CylinderCasingMeshCount==0,"Setting off retains previous geometry");MainSettings(true);
            a.Restore();a.w.ApplyCasingFor(10);Check(a.w.CylinderCasingMeshCount==0,"Stock bypass exact");
            var targets=new[]{0,1,2,3,4,5,12,13,14,27,28,29,30,34,35,44};
            foreach(int id in Enumerable.Range(0,47))Check(a.w.FullCasingFor(id)==targets.Contains(id),"Only selected resized rigid layouts use full normalization");
            foreach(int id in targets)
            {
                a.Restore();a.Legacy(four:id==0||id==1||id==2||id==35);
                if(id>=27&&id<=30)
                {
                    a.scale=.84f;a.offset=1.1f;
                    foreach(var d in new[]{a.w.driveL,a.w.driveR}){d.localPosition=new(0,0,a.offset);d.localScale=new(d==a.w.driveL?-1:1,a.scale,a.scale);}
                    a.w.Apply();
                }
                // Same application order as the actual conventional path. Each
                // fixed guide must share the animated frame, at both LODs.
                a.w.Apply();
                var guideMap=a.w.driveR.localToWorldMatrix;
                foreach(var guide in a.guides)
                {
                    var vertices=guide.f.sharedMesh.vertices;
                    for(int i=0;i<vertices.Length;i++)
                        Check(Near(guide.f.transform.TransformPoint(vertices[i]),guideMap.MultiplyPoint3x4(guide.native[i])),"Every native guide vertex retains its animated-drive relationship for layout "+id);
                }
                var before=All(a.w.driveL).Concat(All(a.w.driveR)).Select(t=>new Pose(t)).ToArray();
                a.w.ApplyCasingFor(id);foreach(var p in before)p.Unchanged();Inspect(a);
                foreach(var clip in anim.RootElement.GetProperty("clips").EnumerateArray())foreach(var frame in clip.GetProperty("frames").EnumerateArray())
                {
                    NativePose(a,clip,frame);
                    foreach(var d in new[]{a.w.driveL,a.w.driveR})
                    {
                        Check(Math.Abs(d.Find("s282_mech_push_rod").localPosition.y-CylinderCasingShape.NativePistonY)<.0001,"Native piston remains on full casing bore axis for arrangement "+id);
                        Check(Math.Abs(d.Find("s282_mech_cylinder_rod").localPosition.y-CylinderCasingShape.NativeValveY)<.0001,"Native spindle remains on full chest axis for arrangement "+id);poses++;
                    }
                }
            }
            foreach(int id in new[]{4,5,44}){a.Restore();a.Legacy(polished:false);var before=All(a.w.driveL).Concat(All(a.w.driveR)).Select(t=>new Pose(t)).ToArray();a.w.ApplyCasingFor(id);foreach(var p in before)p.Unchanged();Inspect(a);}
            foreach(var item in sources){Check(item.Key.vertices.Length==item.Value.Length,"Shared source count unchanged");for(int i=0;i<item.Value.Length;i++)Check(Near(item.Key.vertices[i],item.Value[i],1e-9f),"Shared source vertex immutable");}
            a.w.DisposeCasings();b.w.DisposeCasings();a.w.Dispose();b.w.Dispose();
            File.WriteAllText(args[2],JsonSerializer.Serialize(new{passed=true,checks,nativePoseScenarios=poses,limitations="Actual production casing and Walschaerts partials with managed Unity stand-ins and real mesh/animation data. No Unity rendering or game runtime; broad walk/item collision shells retained."},new JsonSerializerOptions{WriteIndented=true}));
            Console.WriteLine($"Prairie integration: {checks} checks, {poses} native pose scenarios passed.");return 0;
        }
        catch(Exception ex){File.WriteAllText(args[2],JsonSerializer.Serialize(new{passed=false,checks,error=ex.ToString()}));Console.Error.WriteLine(ex);return 1;}
    }
    static void MainSettings(bool value)=>RearrangedS282.Main.settings.nearVanillaCylinderCasings=value;
}
