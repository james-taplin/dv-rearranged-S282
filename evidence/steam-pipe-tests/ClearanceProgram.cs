// Local audit exporter: actual production pipe builder driven by recorded
// in-game socket positions. Native geometry stays in build/, never in a ZIP.
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using System.Reflection;
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
        using var casings=JsonDocument.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(args[1]),"native-casings.json")));
        using var capture=JsonDocument.Parse(File.ReadAllText(args[2]));
        foreach(string asset in new[]{"steam_manifold","steam_transfer"})
            Dummiesman.OBJLoader.Assets[asset+".obj"]=MeshOf(assets.RootElement.GetProperty("meshes").EnumerateArray().Single(m=>m.GetProperty("name").GetString()==asset));
        var output=new List<object>();
        var failures=new List<object>();
        bool stress=args.Contains("stress");
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
                if((args.Length<5||args[4]!="recorded")&&(id==3||id==34||id>=27&&id<=30))
                {
                    float correctionScale=id>=27&&id<=30?.84f:1.2879f,offset=id>=27&&id<=30?1.1f:-2.1f;
                    position.y-=id>=27&&id<=30?-.11f:.21f;
                    position=CylinderCasingShape.NormalizePrairie(position,correctionScale,correctionScale);
                    position=new(position.x,position.y*correctionScale,position.z*correctionScale+offset);scale=Math.Max(.55f,Math.Min(1f,correctionScale));
                }
                t.localPosition=position;
                int side=p.GetProperty("side").GetInt32();
                bool franklin=id==31||id==32||id==38||id==40;
                bool full=new[]{0,1,2,3,4,5,12,13,14,27,28,29,30,34,35,44}.Contains(id);
                float s=id==0||id==1||id==2||id==35?1.42f:id==3||id==4||id==5||id==34?1.2879f:id==12||id==13||id==14||id==44?1.11f:id>=27&&id<=30?.84f:record.GetProperty("converted").GetProperty("forceRadius").GetSingle()/.712f;
                var source=franklin?casings.RootElement.GetProperty("sources")[2].GetProperty("parts")[0]:casings.RootElement.GetProperty("sources")[0].GetProperty("parts").EnumerateArray().Single(a=>a.GetProperty("name").GetString()=="native-lod0-"+(side>0?"left":"right"));
                var vertices=source.GetProperty("vertices").EnumerateArray().Select(V).ToArray();
                if(franklin && side<0)vertices=vertices.Select(v=>new Vector3(-v.x,v.y,v.z)).ToArray();
                int[] faces=source.GetProperty("triangles").EnumerateArray().SelectMany(a=>a.EnumerateArray().Select(v=>v.GetInt32())).ToArray();
                var nativeSource=native.Single(n=>n.Side==side);
                var mount=SteamPipeShape.TopSocket(vertices,faces,franklin?side*1.09f:nativeSource.Lower.x,franklin?9.49f:nativeSource.Lower.z);
                float piston=franklin?.71f:CylinderCasingShape.NativePistonY;
                float minY=franklin?vertices.Min(v=>v.y):Math.Min(.2739646f,vertices.Min(v=>v.y));
                float targetY=CylinderCasingShape.ComputeTargetY(s,minY,piston,0);
                float rear=vertices.Min(v=>v.z)+.16f;
                var corrected=vertices.Select(v=>Math.Abs(targetY-s)<.001f?v:full?CylinderCasingShape.NormalizePrairie(v,s,s):CylinderCasingShape.TransformVertex(v,s,s,targetY,rear,rear+.20f,piston,franklin)).ToArray();
                var top=mount.Evaluate(corrected);
                // Geometry is actual native casing triangles reshaped by the
                // production casing helper. Recover the original translation
                // from the recorded top inlet, retaining tiny capture offsets.
                // Cylinder.Part is placed on that inlet, so local vertices are
                // relative to it; this also lets imposed yaw carry the casing.
                var local=corrected.Select(v=>new Vector3(v.x-top.x,(v.y-top.y)*s,(v.z-top.z)*s)).ToArray();
                return new SteamPipePresentation.Cylinder{Part=t,Side=side,Scale=Math.Clamp(s,.55f,1f),Car=car,CasingVertices=local,CasingTriangles=faces,ForwardBranch=id==44,CompactCollar=id==3||id==4||id==5||id==34};
            }).ToArray();
            var go=body.Add("pipe presentation").gameObject;var actor=go.AddComponent<SteamPipePresentation>();
            try
            {
                actor.Initialize(body,car,nodes,native,articulated);
                if(actor.Error!=null)throw new Exception(actor.Error);
            }
            catch(Exception error)
            {
                failures.Add(new{id,name=record.GetProperty("name").GetString(),syntheticYawDegrees=0,error=error.Message,
                    ports=nodes.Select(n=>{var pipe=native.Single(a=>a.Side==n.Side);var bodyVertices=n.CasingVertices.Select(v=>n.Part.TransformPoint(v)).ToArray();bool fitted=SteamPipeNativeFit.TryFind(pipe.Vertices,pipe.Source.triangles,bodyVertices,n.CasingTriangles,out var fit);return new{side=n.Side,scale=n.Scale,forwardBranch=n.ForwardBranch,nativeFit=fitted,fitY=fitted?(float?)fit.CutY:null,boundsMin=new[]{bodyVertices.Min(v=>v.x),bodyVertices.Min(v=>v.y),bodyVertices.Min(v=>v.z)},boundsMax=new[]{bodyVertices.Max(v=>v.x),bodyVertices.Max(v=>v.y),bodyVertices.Max(v=>v.z)},topInlet=new[]{n.Part.position.x,n.Part.position.y,n.Part.position.z}};}).ToArray()});
                Console.WriteLine("FAILED "+record.GetProperty("name").GetString()+": "+error.Message);
                actor.Release();continue;
            }
            void Snapshot(string pose,float yaw,float? pinZ,string pinBasis)=>output.Add(new{id,name=record.GetProperty("name").GetString(),articulated,pose,syntheticYawDegrees=yaw,pivotBodyZ=pinZ,pivotBasis=pinBasis,
                renderers=actor.PieceCount,maximumEndpointGapM=actor.MaximumGapM,minimumShaftDiameterM=actor.MinimumShaftDiameterM,
                vertexCount=go.GetComponentsInChildren<MeshFilter>(true).Sum(f=>f.sharedMesh.vertexCount),triangleCount=go.GetComponentsInChildren<MeshFilter>(true).Sum(f=>f.sharedMesh.triangles.Length/3),
                sockets=actor.ConnectionPoints.Select(n=>new[]{n.Socket.x,n.Socket.y,n.Socket.z}).ToArray(),
                recordedTopInletAnchors=nodes.Select(n=>new[]{n.Part.position.x,n.Part.position.y,n.Part.position.z}).ToArray(),
                socketUp=nodes.Select(n=>body.InverseTransformVector(n.Part.TransformVector(n.Up))).Select(p=>new[]{p.x,p.y,p.z}).ToArray(),
                boilers=actor.ConnectionPoints.Select(p=>new[]{p.Boiler.x,p.Boiler.y,p.Boiler.z}).ToArray(),
                meshes=go.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.sharedMesh.vertexCount!=0).Select(f=>new{name=f.name,vertices=f.sharedMesh.vertices.Select(p=>body.InverseTransformPoint(f.transform.TransformPoint(p))).Select(p=>new[]{p.x,p.y,p.z}).ToArray(),triangles=f.sharedMesh.triangles}).ToArray()});
            Snapshot("recorded neutral",0,null,"Recorded body-space cylinder sockets; production presentation.");
            if(stress && articulated && id!=38 && id!=40)
            {
                // Recreate the source's fixed rear hinge and rigid front engine
                // parenting, then impose yaw. These are deliberately synthetic
                // stress angles, not a track/physics/native AimConstraint solve.
                var front=nodes.OrderByDescending(n=>n.Part.position.z).Take(2).ToArray();
                float pin; string basis;
                if(id==31 || id==32)
                {
                    pin=((id==32?6.06f:6.30256176f)+8.00187874f-4.65f)*.5f;
                    basis="ConfigureNativeArticulation midpoint of last front and first rear driver; native measured wheel centres plus authored 4.65m engine separation.";
                }
                else
                {
                    float s=record.GetProperty("converted").GetProperty("forceRadius").GetSingle()/.712f;
                    float rearSocket=nodes.Min(n=>n.Part.position.z);
                    // Native top inlet Z9.49695 and cylinder front Z10.220049.
                    // MeasureMallet/MeasureBigMallet puts the previous driver's
                    // rear edge 0.1m ahead of that rear cylinder front.
                    float rearFront=rearSocket+(10.220049f-9.49695f)*s;
                    pin=rearFront+(.7121296f*s+.1f)*.5f;
                    basis="MeasureMallet/MeasureBigMallet midpoint recovered from recorded rear inlet, native cylinder-front datum, driver radius and authored 0.1m engine gap; inlet reconstruction is approximate.";
                }
                var hinge=body.Add("synthetic imposed front-engine yaw");hinge.localPosition=new(0,0,pin);
                foreach(var n in front){var position=n.Part.position;var rotation=n.Part.rotation;n.Part.SetParent(hinge,false);n.Part.position=position;n.Part.rotation=rotation;}
                foreach(float yaw in new[]{-12f,-6f,6f,12f})
                {
                    hinge.localRotation=Quaternion.Euler(0,yaw,0);
                    typeof(SteamPipePresentation).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(actor,null);
                    if(actor.Error!=null){failures.Add(new{id,name=record.GetProperty("name").GetString(),syntheticYawDegrees=yaw,error=actor.Error});break;}
                    Snapshot("synthetic front-engine yaw",yaw,pin,basis);
                }
            }
            actor.Release();
        }
        File.WriteAllText(args[3],JsonSerializer.Serialize(new{captureVersion=capture.RootElement.GetProperty("version").GetString(),correctedSockets=args.Length<5||args[4]!="recorded",stress,failures,cases=output,
            limits="Production pipe geometry using recorded body-space sockets; corrected rigid sockets recomputed from production normalization. Casing triangles use native evidence plus production casing deformation and recorded-inlet translation, not entire WheelRearranger execution. Optional front-engine yaw is synthetic about source-derived hinges; not native track constraints, real scene or continuous articulation. Three-engine Franklin triplex nested hinges excluded from stress pending native scene capture."}));
        Console.WriteLine("Exported "+output.Count+" recorded arrangement pipe presentations");
    }
}
