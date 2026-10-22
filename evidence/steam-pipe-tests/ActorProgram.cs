using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using RearrangedS282;

internal static class ActorProgram
{
    static int checks;
    static void Check(bool ok,string message){checks++;if(!ok)throw new Exception(message);}
    static Vector3 V(JsonElement v)=>new(v[0].GetSingle(),v[1].GetSingle(),v[2].GetSingle());
    static Mesh MeshOf(JsonElement data)
    {
        var mesh = new Mesh { vertices=data.GetProperty("vertices").EnumerateArray().Select(V).ToArray(),triangles=data.GetProperty("triangles").EnumerateArray().SelectMany(r=>r.EnumerateArray().Select(x=>x.GetInt32())).ToArray() };
        mesh.uv = data.TryGetProperty("uv", out var texture) ? texture.EnumerateArray().Select(v => new Vector2(v[0].GetSingle(), v[1].GetSingle())).ToArray() : new Vector2[mesh.vertexCount]; return mesh;
    }
    static void Main(string[] args)
    {
        using var original=JsonDocument.Parse(File.ReadAllText(args[0]));
        // The fixture replaces OBJ parsing only. All presentation, sweep,
        // ownership, matrix and native-LOD integration code is production code.
        var root=original.RootElement;
        foreach(var pair in new[]{("manifold","steam_manifold.obj"),("transfer","steam_transfer.obj")})
            Dummiesman.OBJLoader.Assets[pair.Item2]=MeshOf(root.GetProperty("meshes").EnumerateArray().Single(m=>m.GetProperty("name").GetString()=="steam_"+pair.Item1));
        using var fittingDoc=JsonDocument.Parse(File.ReadAllText(args[1]));
        var parts=fittingDoc.RootElement.GetProperty("parts");
        float maxGap=0; var previews = new List<object>();
        void Preview(string name, Transform body, GameObject go, SteamPipePresentation actor)
        {
            previews.Add(new { name, sockets = actor.ConnectionPoints.Select(p => new {side=p.Side, cylinder=new[]{p.Socket.x,p.Socket.y,p.Socket.z},boiler=new[]{p.Boiler.x,p.Boiler.y,p.Boiler.z}}).ToArray(),
                meshes = go.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.sharedMesh.vertices.Length!=0).Select(f=>new {name=f.name,
                    vertices=f.sharedMesh.vertices.Select(p=>body.InverseTransformPoint(f.transform.TransformPoint(p))).Select(p=>new[]{p.x,p.y,p.z}).ToArray(),triangles=f.sharedMesh.triangles}).ToArray() });
        }
        foreach(int engines in new[]{1,2,3,4})foreach(bool tender in new[]{false,true})
        foreach(float chestHeight in new[]{.9f,1.25f,1.6f,1.9f,2.2f})foreach(float inputScale in new[]{.55f,1f})
        {
            if(tender && engines<3)continue;
            var car=new GameObject("loco").AddComponent<TrainCar>();
            car.transform.localPosition=new(57,4,123);car.transform.localRotation=Quaternion.Euler(0,31,0);
            var body=car.transform.Add("native body");var lod=body.gameObject.AddComponent<LODGroup>();
            var existing=body.Add("native renderer").gameObject.AddComponent<MeshRenderer>();
            var old=new[]{new LOD(.6f,new Renderer[]{existing}){fadeTransitionWidth=.1f},new LOD(.05f,new Renderer[]{existing}),new LOD(.001f,Array.Empty<Renderer>())};
            lod.SetLODs(old);lod.localReferencePoint=new(0,2,4);lod.size=19;
            var native=new SteamPipePresentation.NativePipe[2];
            for(int side=0;side<2;side++)
            {
                var mesh=MeshOf(parts.EnumerateArray().Single(p=>p.GetProperty("name").GetString()=="native-lod0-pipe-"+(side==0?"left":"right")));
                float low=mesh.vertices.Min(v=>v.y),high=mesh.vertices.Max(v=>v.y);
                var ring=mesh.vertices.Where(v=>Math.Abs(v.y-low)<.003f).Distinct().ToArray();
                native[side]=new(){Source=mesh,Vertices=mesh.vertices.ToArray(),Lower=ring.Aggregate(Vector3.zero,(s,p)=>s+p)/ring.Length,Low=low,High=high,Owner=lod,Materials=new[]{new Material()}};
            }
            var nodes=Enumerable.Range(0,engines*2).Select(i=>{
                int side=i%2==0?1:-1;var t=body.Add("native cylinder");t.localPosition=new(side*1.09f,chestHeight,9.4f-(i/2)*4);
                return new SteamPipePresentation.Cylinder{Part=t,Socket=Vector3.zero,Scale=inputScale,Side=side,Car=car};
            }).ToArray();
            TrainCar tenderCar=null;
            if(tender){tenderCar=new GameObject("native tender").AddComponent<TrainCar>();tenderCar.transform.SetParent(car.transform,false);tenderCar.transform.localPosition=new(0,0,-15);foreach(var n in nodes.Skip(nodes.Length-2)){n.Part.SetParent(tenderCar.transform,false);n.Part.localPosition=new(n.Side*1.09f,1.25f,2);n.Car=tenderCar;}}
            var before=native.Select(n=>n.Source.vertices.ToArray()).ToArray();
            var go=body.Add("presentation").gameObject;var actor=go.AddComponent<SteamPipePresentation>();
            // Owner intentionally omits Erie tender cylinders: its LP feed is internal.
            var exteriorNodes = nodes.Where(n => n.Car == car).ToArray();
            try { actor.Initialize(body,car,exteriorNodes,native,engines>1); }
            catch(Exception e){throw new Exception($"Engine groups{engines},tender{tender},chestY{chestHeight},scale{inputScale}: {e.Message}",e);}
            if(engines<=3&&chestHeight==1.25f&&inputScale==.55f) Preview(tender?"Erie locomotive only":"neutral "+engines+" engines", body, go, actor);
            Check(actor.CylinderCount==exteriorNodes.Length,"Lost cylinder");Check(actor.ColliderCount==0,"Added collider");
            Check(actor.UsesDirectRigid == (engines == 1), "Two-cylinder rigid acquired a long header");
            Check(actor.MinimumShaftDiameterM >= .16255f, "Small-cylinder pipes thinner than minimum");
            Check(actor.ConnectionPoints.Count() == exteriorNodes.Length, "Missing diagnostic sockets");
            if(tenderCar!=null)Check(tenderCar.GetComponentsInChildren<MeshRenderer>(true).Length == 0, "Added exterior Erie tender pipe");Check(body.GetComponentsInChildren<LODGroup>(true).Length==1,"Added independent LOD");
            foreach(var f in go.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.Contains("main steam header")))
            {
                var points = f.sharedMesh.vertices; var source = native.Single(n=>n.Side==(points.Average(v=>v.x)>=0?1:-1));
                var expected = new Vector3(source.Side*1.16f,1.95f,source.Lower.z+.16f);
                var centerIds = Enumerable.Range(0,points.Length).Where(i=>(points[i]-expected).magnitude<.001f).ToArray();
                Check(centerIds.Length>0,"Header front cap center missing");
                Check(f.sharedMesh.triangles.Count(i=>centerIds.Contains(i))>=8,"Header front bore remains open");
            }
            foreach(var f in go.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.Contains("original boiler fitting")))
            {
                var mesh=f.sharedMesh; int sign=mesh.vertices.Average(v=>v.x)>=0?1:-1;
                var source=native.Single(n=>n.Side==sign);float cut=source.High-.28f;
                Check(source.Vertices.Where(v=>v.y>=cut).All(v=>mesh.vertices.Contains(v)), "Changed retained original upper boiler fitting");
            }
            Check(lod.GetLODs()[2].renderers.Length==0,"Filled native cull level");Check(lod.GetLODs()[0].fadeTransitionWidth==.1f,"Changed native fade");
            foreach(float yaw in new[]{-12f,-6f,0f,6f,12f}){
                if(engines>1){nodes[0].Part.localRotation=Quaternion.Euler(0,yaw,0);nodes[1].Part.localRotation=Quaternion.Euler(0,yaw,0);if(tenderCar!=null)tenderCar.transform.localRotation=Quaternion.Euler(0,yaw*2,0);}
                typeof(SteamPipePresentation).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(actor,null);
                Check(actor.Error==null,"Presentation failure groups="+engines+" tender="+tender+" height="+chestHeight+" scale="+inputScale+" yaw="+yaw+": "+actor.Error);Check(actor.MaximumGapM<.003f,"Endpoint detached");maxGap=Math.Max(maxGap,actor.MaximumGapM);
            }
            for(int i=0;i<2;i++)Check(native[i].Source.vertices.SequenceEqual(before[i]),"Changed shared native mesh");
            var owned=go.GetComponentsInChildren<MeshFilter>(true).Select(f=>f.sharedMesh).Where(m=>!Dummiesman.OBJLoader.Assets.Values.Contains(m)).ToArray();
            bool recovered=false;actor.Failed=()=>recovered=true;
            if(engines>1){nodes[0].Part=null;typeof(SteamPipePresentation).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(actor,null);Check(recovered&&!go.activeSelf&&actor.Error!=null,"Failed path froze visible pipes or lost error");}
            actor.Release();actor.Release();
            Check(ReferenceEquals(lod.GetLODs(),old),"Did not restore exact native LOD arrays");Check(lod.size==19&&(lod.localReferencePoint-new Vector3(0,2,4)).magnitude<.00001f,"Native bounds not restored");
            Check(owned.All(m=>m.destroyed),"Owned mesh leaked");Check(Dummiesman.OBJLoader.Assets.Values.All(m=>!m.destroyed),"Destroyed shared authored asset");
        }
        int rigidCases = 0;
        foreach(float height in new[]{.9f,1.3f,1.6f,1.9f,2.2f,2.3f})
        foreach(float z in new[]{7.2f,8.6f,9.4f})foreach(float scale in new[]{.55f,.8f,1f})
        {
            var car=new GameObject("rigid stress").AddComponent<TrainCar>();var body=car.transform.Add("body");var lod=body.gameObject.AddComponent<LODGroup>();
            var old=new[]{new LOD(.5f,new Renderer[]{body.Add("native renderer").gameObject.AddComponent<MeshRenderer>()}),new LOD(.001f,Array.Empty<Renderer>())}; lod.SetLODs(old);lod.size=19;
            var native=parts.EnumerateArray().Where(p=>p.GetProperty("name").GetString().StartsWith("native-lod0-pipe-")).Select(p=>{
                var mesh=MeshOf(p);float low=mesh.vertices.Min(v=>v.y),high=mesh.vertices.Max(v=>v.y);
                var ring=mesh.vertices.Where(v=>Math.Abs(v.y-low)<.003f).Distinct().ToArray();return new SteamPipePresentation.NativePipe{Source=mesh,Vertices=mesh.vertices.ToArray(),Lower=ring.Aggregate(Vector3.zero,(s,v)=>s+v)/ring.Length,Low=low,High=high,Owner=lod,Materials=new[]{new Material()}};
            }).ToArray();
            var nodes=new[]{1,-1}.Select(side=>{var t=body.Add("cylinder");t.localPosition=new(side*1.09f,height,z);return new SteamPipePresentation.Cylinder{Part=t,Socket=Vector3.zero,Side=side,Scale=scale,Car=car};}).ToArray();
            var go=body.Add("rigid branch").gameObject;var actor=go.AddComponent<SteamPipePresentation>();
            try{actor.Initialize(body,car,nodes,native,false);}catch(Exception e){throw new Exception($"Rigid stress Y{height},Z{z},scale{scale}: {e.Message}",e);}
            foreach(var f in go.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.Contains("original boiler fitting")))
            {
                var v = f.sharedMesh.vertices; float cut=v.Min(p=>p.y); var flat = Enumerable.Range(0,v.Length).Where(i=>Math.Abs(v[i].y-cut)<.00001f).ToArray();
                int flatFaces = Enumerable.Range(0,f.sharedMesh.triangles.Length/3).Count(i=>Enumerable.Range(0,3).All(k=>flat.Contains(f.sharedMesh.triangles[3*i+k])));
                Check(flatFaces>=8,"Rigid retained neck cut was left open");
            }
            Check(actor.Error==null&&!actor.UsesHeader&&actor.UsesDirectRigid,"Rigid acquired header/error");Check(actor.MaximumGapM<.003f,"Rigid cap detached");
            Check(actor.MinimumShaftDiameterM>=.16255f,"Rigid minOD violated");Check(actor.ColliderCount==0,"Rigid collider");
            if(scale==.8f&&((height==.9f&&z==7.2f)||(height==2.3f&&z==9.4f)))Preview(height<1?"rigid low long":"rigid high short",body,go,actor);
            actor.Release();Check(ReferenceEquals(lod.GetLODs(),old),"RigidLOD arrays leaked");rigidCases++;
        }
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(args[2]),"preview-meshes.json"), JsonSerializer.Serialize(previews));
        var report=new{passed=true,rigidCases,checks,maximumEndpointGapM=maxGap,scope="Managed presentation lifecycle and native LOD stand-ins; no Unity player/physics/visual clearance validation"};
        File.WriteAllText(args[2],JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(report));
    }
}

