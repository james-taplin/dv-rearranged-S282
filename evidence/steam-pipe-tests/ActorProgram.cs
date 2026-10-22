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
    static bool Finite(float v)=>!float.IsNaN(v)&&!float.IsInfinity(v);
    static void Streams(GameObject go)
    {
        foreach(var f in go.GetComponentsInChildren<MeshFilter>(true))
        {
            var m=f.sharedMesh;Check(m.vertices.All(v=>Finite(v.x)&&Finite(v.y)&&Finite(v.z)),"Nonfinite presentation vertices: "+f.name);
            Check(m.triangles.Length%3==0&&m.triangles.All(i=>i>=0&&i<m.vertexCount),"Invalid presentation indices: "+f.name);
            Check(m.uv.Length==0||m.uv.Length==m.vertexCount,"Presentation UV stream mismatch: "+f.name);
            Check(m.normals.Length==0||m.normals.Length==m.vertexCount,"Presentation normal stream mismatch: "+f.name);
            Check(m.normals.All(v=>Finite(v.x)&&Finite(v.y)&&Finite(v.z)),"Nonfinite presentation normals: "+f.name);
        }
    }
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
        using var casingDoc=JsonDocument.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(args[1]),"native-casings.json")));
        var casingParts=casingDoc.RootElement.GetProperty("sources")[0].GetProperty("parts");
        void RigidCasing(SteamPipePresentation.Cylinder cylinder,SteamPipePresentation.NativePipe[] native,bool franklin=false,float frameScale=1f)
        {
            var source=franklin?casingDoc.RootElement.GetProperty("sources")[2].GetProperty("parts")[0]:casingParts.EnumerateArray().Single(p=>p.GetProperty("name").GetString()=="native-lod0-"+(cylinder.Side>0?"left":"right"));
            var vertices=source.GetProperty("vertices").EnumerateArray().Select(V).ToArray();
            if(franklin&&cylinder.Side<0)vertices=vertices.Select(v=>new Vector3(-v.x,v.y,v.z)).ToArray();
            var faces=source.GetProperty("triangles").EnumerateArray().SelectMany(r=>r.EnumerateArray().Select(i=>i.GetInt32())).ToArray();
            var pipe=native.Single(n=>n.Side==cylinder.Side);
            var socket=SteamPipeShape.TopSocket(vertices,faces,franklin?cylinder.Side*1.09f:pipe.Lower.x,franklin?9.49f:pipe.Lower.z).Evaluate(vertices);
            cylinder.Part.localPosition=new(socket.x,cylinder.Part.localPosition.y,cylinder.Part.localPosition.z);
            cylinder.CasingVertices=vertices.Select(v=>new Vector3(v.x-socket.x,(v.y-socket.y)*frameScale,(v.z-socket.z)*frameScale)).ToArray();cylinder.CasingTriangles=faces;
        }
        float maxGap=0;int reachableSyntheticCases=0,rejectedSyntheticCases=0;var previews = new List<object>();
        bool RejectedGeometry(Exception error,SteamPipePresentation actor,GameObject go,LODGroup owner,LOD[] old)
        {
            if(error.Message!="Measured steam route has no room for a round elbow at its retained diameter."&&error.Message!="Steam duct elbow turns more than 170 degrees.")return false;
            var owned=go.GetComponentsInChildren<MeshFilter>(true).Select(f=>f.sharedMesh).Where(m=>!Dummiesman.OBJLoader.Assets.Values.Contains(m)).ToArray();
            actor.Release();actor.Release();Check(ReferenceEquals(owner.GetLODs(),old),"Rejected synthetic geometry changed native LOD arrays");Check(owned.All(m=>m.destroyed),"Rejected synthetic geometry leaked owned meshes");Check(Dummiesman.OBJLoader.Assets.Values.All(m=>!m.destroyed),"Rejected geometry destroyed authored asset");
            rejectedSyntheticCases++;return true;
        }
        void Preview(string name, Transform body, GameObject go, SteamPipePresentation actor)
        {
            previews.Add(new { name, sockets = actor.ConnectionPoints.Select(p => new {side=p.Side, cylinder=new[]{p.Socket.x,p.Socket.y,p.Socket.z},boiler=new[]{p.Boiler.x,p.Boiler.y,p.Boiler.z}}).ToArray(),
                meshes = go.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.sharedMesh.vertices.Length!=0).Select(f=>new {name=f.name,
                    vertices=f.sharedMesh.vertices.Select(p=>body.InverseTransformPoint(f.transform.TransformPoint(p))).Select(p=>new[]{p.x,p.y,p.z}).ToArray(),triangles=f.sharedMesh.triangles}).ToArray() });
        }
        foreach(int engines in new[]{1,2,3,4})foreach(bool tender in new[]{false,true})
        foreach(float chestHeight in new[]{.9f,1.25f,1.523f,1.9f,2.2f})foreach(float inputScale in new[]{.55f,1f})
        {
            if(tender && engines<3)continue;
            // Current articulated sockets are all below1.6m. The former
            // point-only1.9/2.2m nodes sat above the new horizontal header and
            // did not represent any shipped engine. Retain high rigid cases
            // with actual native casing triangles, exercising native seating.
            if(engines>1 && chestHeight>1.523f)continue;
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
                int side=i%2==0?1:-1;var t=body.Add("native cylinder");t.localPosition=new(side*1.09f,chestHeight,(engines==1&&chestHeight==1.523f?8.6f:9.4f)-(i/2)*4);
                return new SteamPipePresentation.Cylinder{Part=t,Socket=Vector3.zero,Scale=inputScale,Side=side,Car=car};
            }).ToArray();
            foreach(var node in nodes)RigidCasing(node,native,engines>1,engines>1?inputScale:1f);
            TrainCar tenderCar=null;
            if(tender){tenderCar=new GameObject("native tender").AddComponent<TrainCar>();tenderCar.transform.SetParent(car.transform,false);tenderCar.transform.localPosition=new(0,0,-15);foreach(var n in nodes.Skip(nodes.Length-2)){n.Part.SetParent(tenderCar.transform,false);n.Part.localPosition=new(n.Side*1.09f,1.25f,2);n.Car=tenderCar;}}
            var before=native.Select(n=>n.Source.vertices.ToArray()).ToArray();
            var beforeUV=native.Select(n=>n.Source.uv.ToArray()).ToArray();var beforeFaces=native.Select(n=>n.Source.triangles.ToArray()).ToArray();
            var go=body.Add("presentation").gameObject;var actor=go.AddComponent<SteamPipePresentation>();
            // Owner intentionally omits Erie tender cylinders: its LP feed is internal.
            var exteriorNodes = nodes.Where(n => n.Car == car).ToArray();
            try { actor.Initialize(body,car,exteriorNodes,native,engines>1); }
            catch(Exception e){if(RejectedGeometry(e,actor,go,lod,old))continue;throw new Exception($"Engine groups{engines},tender{tender},chestY{chestHeight},scale{inputScale}: {e.Message}",e);}
            reachableSyntheticCases++;
            if(engines<=3&&chestHeight==1.25f&&inputScale==.55f) Preview(tender?"Erie locomotive only":"neutral "+engines+" engines", body, go, actor);
            Check(actor.CylinderCount==exteriorNodes.Length,"Lost cylinder");Check(actor.ColliderCount==0,"Added collider");
            Check(actor.UsesDirectRigid == (engines == 1), "Two-cylinder rigid acquired a long header");
            Check(actor.MinimumShaftDiameterM >= .16255f, "Small-cylinder pipes thinner than minimum");
            Check(actor.ConnectionPoints.Count() == exteriorNodes.Length, "Missing diagnostic sockets");
            if(tenderCar!=null)Check(tenderCar.GetComponentsInChildren<MeshRenderer>(true).Length == 0, "Added exterior Erie tender pipe");Check(body.GetComponentsInChildren<LODGroup>(true).Length==1,"Added independent LOD");
            var headerFilters=go.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.Contains("matched header and cylinder branches")).ToArray();
            Check(headerFilters.Length==(actor.UsesHeader?2:0),"Header renderer ownership mismatch");
            foreach(var f in headerFilters)
            {
                var points = f.sharedMesh.vertices;
                // Source contract: each combined per-side mesh contains a
                // horizontal header at body X +/-1.45,Y2.05, with two closed
                // end caps. Feed/branch geometry shares this mesh, so its full
                // AABB is not the header's own centreline.
                float x=points.Average(p=>p.x)>=0?1.45f:-1.45f;
                var centerIds = Enumerable.Range(0,points.Length).Where(i=>Math.Abs(points[i].x-x)<.001f&&Math.Abs(points[i].y-2.05f)<.001f).ToArray();
                Check(centerIds.Length>=2,"Header end cap centres missing");
                Check(centerIds.Count(id=>f.sharedMesh.triangles.Count(i=>i==id)>=8)>=2,"Header bore remains open");
            }
            foreach(var f in go.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.Contains("original boiler fitting")))
            {
                var mesh=f.sharedMesh; int sign=mesh.vertices.Average(v=>v.x)>=0?1:-1;
                var source=native.Single(n=>n.Side==sign);float cut=source.High-.08f;
                Check(source.Vertices.Where(v=>v.y>=cut).All(v=>mesh.vertices.Contains(v)), "Changed retained original upper boiler fitting");
            }
            Check(lod.GetLODs()[2].renderers.Length==0,"Filled native cull level");Check(lod.GetLODs()[0].fadeTransitionWidth==.1f,"Changed native fade");
            Streams(go);
            foreach(float yaw in new[]{-12f,-6f,0f,6f,12f}){
                if(engines>1){nodes[0].Part.localRotation=Quaternion.Euler(0,yaw,0);nodes[1].Part.localRotation=Quaternion.Euler(0,yaw,0);if(tenderCar!=null)tenderCar.transform.localRotation=Quaternion.Euler(0,yaw*2,0);}
                typeof(SteamPipePresentation).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(actor,null);
                Check(actor.Error==null,"Presentation failure groups="+engines+" tender="+tender+" height="+chestHeight+" scale="+inputScale+" yaw="+yaw+": "+actor.Error);Check(actor.MaximumGapM<.003f,"Endpoint detached");maxGap=Math.Max(maxGap,actor.MaximumGapM);Streams(go);
            }
            for(int i=0;i<2;i++){Check(native[i].Source.vertices.SequenceEqual(before[i]),"Changed shared native mesh");Check(native[i].Source.uv.SequenceEqual(beforeUV[i]),"Changed shared native texture coordinates");Check(native[i].Source.triangles.SequenceEqual(beforeFaces[i]),"Changed shared native topology");}
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
            foreach(var node in nodes)RigidCasing(node,native);
            var go=body.Add("rigid branch").gameObject;var actor=go.AddComponent<SteamPipePresentation>();
            try{actor.Initialize(body,car,nodes,native,false);}catch(Exception e){if(RejectedGeometry(e,actor,go,lod,old))continue;throw new Exception($"Rigid stress Y{height},Z{z},scale{scale}: {e.Message}",e);}
            foreach(var f in go.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.Contains("original boiler fitting")))
            {
                var v = f.sharedMesh.vertices; float cut=v.Min(p=>p.y);
                Check(v.Where(p=>Math.Abs(p.y-cut)<.00001f).Distinct().Count()>=6,"Native branch has no complete cut profile");
            }
            Check(actor.Error==null&&!actor.UsesHeader&&actor.UsesDirectRigid,"Rigid acquired header/error");Check(actor.MaximumGapM<.003f,"Rigid cap detached");
            Streams(go);
            Check(actor.MinimumShaftDiameterM>=.16255f,"Rigid minOD violated");Check(actor.ColliderCount==0,"Rigid collider");
            if(scale==.8f&&((height==.9f&&z==7.2f)||(height==2.3f&&z==9.4f)))Preview(height<1?"rigid low long":"rigid high short",body,go,actor);
            actor.Release();Check(ReferenceEquals(lod.GetLODs(),old),"RigidLOD arrays leaked");rigidCases++;
        }
        Check(reachableSyntheticCases>=20&&rigidCases>=20,"Too few reachable synthetic lifecycle cases; catalogue validation cannot be substituted by rejection");
        // Keep two live presentations sharing the same authored assets. A
        // second locomotive must not reshape or dispose the first one's work.
        var sharedNative=parts.EnumerateArray().Where(p=>p.GetProperty("name").GetString().StartsWith("native-lod0-pipe-")).Select(MeshOf).ToArray();
        (SteamPipePresentation actor,GameObject go,SteamPipePresentation.Cylinder[] nodes,LODGroup lod,LOD[] old) Instance(string name,float z)
        {
            var car=new GameObject(name).AddComponent<TrainCar>();var frame=car.transform.Add("body");var owner=frame.gameObject.AddComponent<LODGroup>();
            var old=new[]{new LOD(.5f,new Renderer[]{frame.Add("native").gameObject.AddComponent<MeshRenderer>()}),new LOD(.001f,Array.Empty<Renderer>())};owner.SetLODs(old);owner.size=19;
            var native=sharedNative.Select(mesh=>{float low=mesh.vertices.Min(v=>v.y),high=mesh.vertices.Max(v=>v.y);var ring=mesh.vertices.Where(v=>Math.Abs(v.y-low)<.003f).Distinct().ToArray();return new SteamPipePresentation.NativePipe{Source=mesh,Vertices=mesh.vertices.ToArray(),Low=low,High=high,Lower=ring.Aggregate(Vector3.zero,(a,v)=>a+v)/ring.Length,Owner=owner,Materials=new[]{new Material()}};}).ToArray();
            var nodes=new[]{1,-1}.Select(side=>{var part=frame.Add("cylinder");part.localPosition=new(side*1.09f,.9f,z);return new SteamPipePresentation.Cylinder{Part=part,Side=side,Scale=.8f,Car=car};}).ToArray();
            foreach(var node in nodes)RigidCasing(node,native);
            var go=frame.Add("presentation").gameObject;var actor=go.AddComponent<SteamPipePresentation>();actor.Initialize(frame,car,nodes,native,false);
            return(actor,go,nodes,owner,old);
        }
        var instanceA=Instance("first live locomotive",9.4f);
        var meshesA=instanceA.go.GetComponentsInChildren<MeshFilter>(true).Select(f=>f.sharedMesh).ToArray();
        var snapshotA=meshesA.Select(m=>m.vertices.ToArray()).ToArray();
        var instanceB=Instance("second live locomotive",8.7f);
        var meshesB=instanceB.go.GetComponentsInChildren<MeshFilter>(true).Select(f=>f.sharedMesh).ToArray();
        Check(!meshesA.Intersect(meshesB).Any(),"Two locomotives share an owned presentation mesh");
        Check(meshesA.Select((m,i)=>m.vertices.SequenceEqual(snapshotA[i])).All(v=>v),"Second locomotive reshaped first locomotive");
        instanceB.nodes[0].Part.localPosition+=Vector3.forward*.02f;
        typeof(SteamPipePresentation).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(instanceB.actor,null);
        Check(instanceB.actor.Error==null&&instanceB.actor.MaximumGapM<.003f,"Second locomotive pose update failed");
        Check(meshesA.Select((m,i)=>m.vertices.SequenceEqual(snapshotA[i])).All(v=>v),"Second locomotive pose mutated first locomotive");
        instanceB.actor.Release();instanceB.actor.Release();
        Check(meshesA.All(m=>!m.destroyed)&&instanceA.go.activeSelf,"Releasing second locomotive damaged first locomotive");
        Check(meshesB.All(m=>m.destroyed)&&ReferenceEquals(instanceB.lod.GetLODs(),instanceB.old),"Second locomotive mesh/LOD cleanup failed");
        bool rigidRecovered=false;instanceA.actor.Failed=()=>rigidRecovered=true;instanceA.nodes[0].Part=null;
        typeof(SteamPipePresentation).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(instanceA.actor,null);
        Check(rigidRecovered&&instanceA.actor.Error!=null&&!instanceA.go.activeSelf,"Rigid failure did not hide presentation and restore source hook");
        instanceA.actor.Release();instanceA.actor.Release();
        Check(meshesA.All(m=>m.destroyed)&&ReferenceEquals(instanceA.lod.GetLODs(),instanceA.old),"First locomotive mesh/LOD cleanup failed");
        Check(sharedNative.All(m=>!m.destroyed),"Destroyed shared native evidence source");
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(args[2]),"preview-meshes.json"), JsonSerializer.Serialize(previews));
        var report=new{passed=true,rigidCases,reachableSyntheticCases,rejectedSyntheticCases,checks,liveInstanceIsolationCases=2,maximumEndpointGapM=maxGap,scope="Managed presentation lifecycle with actual native casing/pipe geometry and native LOD stand-ins. Synthetic poses with insufficient elbow room must reject cleanly; they are not counted as successful presentations. Actual44 catalogue ports are validated strictly by ClearanceProgram. Reachable articulated socket heights0.9–1.523m; high rigid native-casing seating retained. Former arbitrary1.9/2.2m compound point sockets are outside current catalogue. No Unity player/physics/visual clearance validation"};
        File.WriteAllText(args[2],JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(report));
    }
}

