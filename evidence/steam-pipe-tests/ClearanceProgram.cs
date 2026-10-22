// Local audit exporter: actual production pipe builder driven by recorded
// in-game socket positions. Native geometry stays in build/, never in a ZIP.
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using UnityEngine;
using RearrangedS282;
internal static class ClearanceProgram
{
    static Vector3 V(JsonElement a)=>new(a[0].GetSingle(),a[1].GetSingle(),a[2].GetSingle());
    static Mesh MeshOf(JsonElement a)=>new Mesh{vertices=a.GetProperty("vertices").EnumerateArray().Select(V).ToArray(),triangles=a.GetProperty("triangles").EnumerateArray().SelectMany(t=>t.EnumerateArray().Select(i=>i.GetInt32())).ToArray(),uv=a.TryGetProperty("uv",out var uv)?uv.EnumerateArray().Select(p=>new Vector2(p[0].GetSingle(),p[1].GetSingle())).ToArray():new Vector2[a.GetProperty("vertices").GetArrayLength()]};
    static void Main(string[] args)
    {
        using var assets=JsonDocument.Parse(File.ReadAllText(args[0]));
        using var fitting=JsonDocument.Parse(File.ReadAllText(args[1]));
        using var capture=JsonDocument.Parse(File.ReadAllText(args[2]));
        foreach(string asset in new[]{"steam_manifold","steam_transfer"})
            Dummiesman.OBJLoader.Assets[asset+".obj"]=MeshOf(assets.RootElement.GetProperty("meshes").EnumerateArray().Single(m=>m.GetProperty("name").GetString()==asset));
        var output=new List<object>();
        foreach(var record in capture.RootElement.GetProperty("arrangements").EnumerateArray())
        {
            int id=record.GetProperty("id").GetInt32();
            var steam=record.GetProperty("steamConnections");
            var points=steam.GetProperty("connectionPointsBodyM").EnumerateArray().ToArray();
            if(points.Length==0)continue;
            var car=new GameObject("captured locomotive").AddComponent<TrainCar>();var body=car.transform.Add("body");
            var lod=body.gameObject.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.5f,new Renderer[]{body.Add("existing").gameObject.AddComponent<MeshRenderer>()})});lod.size=19;
            var native=fitting.RootElement.GetProperty("parts").EnumerateArray().Where(p=>p.GetProperty("name").GetString().StartsWith("native-lod0-pipe-")).Select(p=>{
                var mesh=MeshOf(p);float lo=mesh.vertices.Min(v=>v.y),hi=mesh.vertices.Max(v=>v.y);
                var ring=mesh.vertices.Where(v=>Math.Abs(v.y-lo)<.003f).Distinct().ToArray();
                return new SteamPipePresentation.NativePipe{Source=mesh,Vertices=mesh.vertices,Lower=ring.Aggregate(Vector3.zero,(a,v)=>a+v)/ring.Length,Low=lo,High=hi,Owner=lod,Materials=new[]{new Material()}};
            }).ToArray();
            bool articulated=steam.GetProperty("manifoldHeader").GetBoolean();
            float scale=steam.GetProperty("minimumShaftDiameterM").GetSingle()/.2032f;
            var nodes=points.Select(p=>{
                var t=body.Add("cylinder");var position=V(p.GetProperty("cylinder"));
                // These two old families lacked the common cylinder frame in
                // the recorded build. Recompute their upper socket with the
                // same production normalization used by the new candidate.
                if(id==3||id==34||id>=27&&id<=30)
                {
                    float s=id>=27&&id<=30?.84f:1.2879f,offset=id>=27&&id<=30?1.1f:-2.1f;
                    position.y-=id>=27&&id<=30?-.11f:.21f;
                    position=CylinderCasingShape.NormalizePrairie(position,s,s);
                    position=new(position.x,position.y*s,position.z*s+offset);scale=s;
                }
                t.localPosition=position;
                return new SteamPipePresentation.Cylinder{Part=t,Side=p.GetProperty("side").GetInt32(),Scale=scale,Car=car};
            }).ToArray();
            var go=body.Add("pipe presentation").gameObject;var actor=go.AddComponent<SteamPipePresentation>();
            actor.Initialize(body,car,nodes,native,articulated);
            if(actor.Error!=null)throw new Exception(record.GetProperty("name").GetString()+": "+actor.Error);
            output.Add(new{id,name=record.GetProperty("name").GetString(),articulated,
                sockets=nodes.Select(n=>new[]{n.Part.position.x,n.Part.position.y,n.Part.position.z}).ToArray(),
                meshes=go.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.sharedMesh.vertexCount!=0).Select(f=>new{name=f.name,vertices=f.sharedMesh.vertices.Select(p=>body.InverseTransformPoint(f.transform.TransformPoint(p))).Select(p=>new[]{p.x,p.y,p.z}).ToArray(),triangles=f.sharedMesh.triangles}).ToArray()});
            actor.Release();
        }
        File.WriteAllText(args[3],JsonSerializer.Serialize(new{captureVersion=capture.RootElement.GetProperty("version").GetString(),cases=output,
            limits="Production pipe geometry using recorded body-space sockets; corrected rigid sockets recomputed from production normalization. Neutral pose; no measured live articulation clearance or complete game scene."}));
        Console.WriteLine("Exported "+output.Count+" recorded arrangement pipe presentations");
    }
}
