using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using RearrangedS282;
using UnityEngine;
using DV.Util;
using ModMain=RearrangedS282.Main;

internal static partial class Program
{
    private static int connectingCases,connectingPoses,connectingOilSamples,connectingFaceChecks,connectingRollbackCases,precisionLodBoundSamples;
    private static float maximumMainCrossheadErrorYZ,maximumRadiusHingeErrorYZ,maximumCrossheadHoldError,maximumConnectingBossError;
    private static long remeshIntoAllocatedBytes;
    private static long[] allocationProbeBatches;
    private static float maximumPrecisionLodEscape;
    private static float maximumIntroducedLodEscape;
    private static string worstPrecisionLodEscape;
    private static readonly string MainRodPath="s282_mech_wheels_connect/s282_mech_push_rod_to_connect";
    private static readonly string RadiusRodPath="s282_mech_cutoff_boomerang_high/s282_mech_cutoff_rail_connection/s282_mech_cutoff_to_cylinder_rod";
    private static readonly string[] ShiftPaths={"s282_mech_push_rod","s282_mech_push_rod/s282_mech_push_rod_to_cutoff_base",
        "s282_mech_cutoff_rod_hinge","s282_mech_cutoff_rod_hinge/s282_mech_push_rod_to_cutoff_rod","s282_mech_cylinder_rod"};
    private static readonly MethodInfo RetargetTick=typeof(DriveRetarget).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic);

    private sealed class ConnectingRig
    {
        internal readonly Rig rig;
        internal readonly List<(Transform source,Mesh mesh,Vector3[] positions,int[] triangles)> motions=new();
        internal readonly List<(PositionSyncProvider provider,PositionSyncConsumer consumer,Vector3 local)> cups=new();
        internal readonly List<Transform> fixedParts=new(),supports=new();
        internal ConnectingRig(JsonElement rods,JsonElement animation,JsonElement wheels,JsonElement motion,JsonElement guideData,bool rotated,bool sharedLod,string output)
        {
            rig=new Rig(rods,animation,wheels,rotated,sharedLod,true);
            foreach(var drive in rig.sides)
            {
                var detail=new List<Renderer>();var coarse=new List<Renderer>();
                foreach(string path in ShiftPaths.Concat(new[]{MainRodPath,RadiusRodPath}))foreach(bool lod in new[]{false,true})
                {
                    string shortName=path.Split('/').Last();string name=shortName+(lod?"_LOD1":"");
                    var data=motion.GetProperty("meshes").GetProperty(name);var mesh=MeshFrom(data,name);
                    var source=Path(drive,path+(lod?"/"+name:""));var renderer=Visual(source,mesh);
                    motions.Add((source,mesh,mesh.vertices.ToArray(),mesh.triangles.ToArray()));
                    // Register the actual native topology as an exported readable
                    // copy; original non-readable CPU access must still fail.
                    var exported=UnityEngine.Object.Instantiate(mesh);
                    Dummiesman.OBJLoader.Sources[name]=exported;MeshFinder.ReadableSources[name]=exported;
                    mesh.isReadable=data.GetProperty("nativeReadable").GetBoolean();
                    (lod?coarse:detail).Add(renderer);
                }
                var group=rig.groups.Single(g=>g.GetLODs().Any(l=>l.renderers.Contains(drive.Find("s282_wheels_driving_2").GetComponent<MeshRenderer>())));
                group.SetLODs(group.GetLODs().Select((l,i)=>new LOD(l.screenRelativeTransitionHeight,l.renderers.Concat(i==0?detail:coarse).ToArray()){fadeTransitionWidth=l.fadeTransitionWidth}).ToArray());
                foreach(string path in new[]{"Oiling0","s282_mech_push_rod/Oiling1",MainRodPath+"/Oiling2"})
                {
                    var t=drive.Find(path);var provider=t.gameObject.AddComponent<PositionSyncProvider>();provider.syncTag=drive.name+path;
                    var consumer=new GameObject("native external cup "+provider.syncTag).AddComponent<PositionSyncConsumer>();consumer.SetProviderTransform(provider);
                    cups.Add((provider,consumer,t.localPosition));
                }
            }
            foreach(string side in new[]{"left","right"})
            {
                var data=guideData.GetProperty("parts").EnumerateArray().Single(p=>p.GetProperty("name").GetString()=="native-lod0-guide-"+side);
                var guide=Child(rig.body,"fixed actual native guide "+side);Visual(guide,MeshFrom(data,"native guide "+side));
                if(side=="left")rig.owner.lfCrossheadBracket=guide;else rig.owner.rfCrossheadBracket=guide;
                fixedParts.Add(guide);
            }
            foreach(string lod in new[]{"Static_LOD0","Static_LOD1"})foreach(string side in new[]{"l","r"})
            {
                var split=rig.body.Find(lod+"/s282a_split_body(Clone)");
                var casing=Child(split,"fixed accepted cylinder "+side);casing.localPosition=new Vector3(side=="l"?1:-1,1,8);fixedParts.Add(casing);
                var support=Child(split,"s282a_lifting_arm_support_"+side);support.localPosition=new Vector3(side=="l"?1:-1,1.8f,6);supports.Add(support);
                support.gameObject.SetActive(lod=="Static_LOD0");
            }
            ModMain.ModPath=System.IO.Path.GetFullPath(System.IO.Path.Combine(output,"readable-export-provider"));
            Directory.CreateDirectory(System.IO.Path.Combine(ModMain.ModPath,"assets"));
            File.WriteAllText(System.IO.Path.Combine(ModMain.ModPath,"assets","s282_mech_push_rod_to_connect.obj"),"fixture export existence marker; native data is registered in the test OBJ loader");
            rig.owner.CacheConnectingFixture();
        }
        internal void Poses(JsonElement clip,JsonElement left,JsonElement right)
        {
            Rig.PoseDrive(rig.sides[0],clip,left);Rig.PoseDrive(rig.sides[1],clip,right);
            foreach(var w in rig.owner.GetComponentsInChildren<PoweredWheelRotater>(false))w.LateUpdate();
        }
        internal void TickRetargets()
        {
            foreach(var d in rig.sides){var retarget=d.GetComponent<DriveRetarget>();if(retarget!=null)RetargetTick.Invoke(retarget,null);}
        }
        internal Transform Proxy(Transform source)=>source.children.Single(t=>t.name.StartsWith("[Rearranged retarget]",StringComparison.Ordinal));
        internal void NativeLodMetadata(JsonElement motion)
        {
            foreach(var group in rig.groups.Where(g=>g.transform==rig.moving||rig.sides.Contains(g.transform)))
            {
                var native=motion.GetProperty("lods").GetProperty("LocoS282A_Drivetrain R");
                var reference=V(native.GetProperty("referencePoint"));float size=native.GetProperty("size").GetSingle();
                if(group.transform==rig.moving)
                {
                    var points=new List<Vector3>();
                    foreach(var drive in rig.sides)foreach(float x in new[]{reference.x-size*.5f,reference.x+size*.5f})foreach(float y in new[]{reference.y-size*.5f,reference.y+size*.5f})foreach(float z in new[]{reference.z-size*.5f,reference.z+size*.5f})
                        points.Add(group.transform.InverseTransformPoint(drive.TransformPoint(new Vector3(x,y,z))));
                    var lo=new Vector3(points.Min(p=>p.x),points.Min(p=>p.y),points.Min(p=>p.z));var hi=new Vector3(points.Max(p=>p.x),points.Max(p=>p.y),points.Max(p=>p.z));
                    reference=(lo+hi)*.5f;var span=hi-lo;size=Math.Max(span.x,Math.Max(span.y,span.z));
                }
                group.localReferencePoint=reference;group.size=size;
                group.SetLODs(group.GetLODs().Select((l,i)=>new LOD(native.GetProperty("levels")[i].GetProperty("threshold").GetSingle(),l.renderers){fadeTransitionWidth=native.GetProperty("levels")[i].GetProperty("fade").GetSingle()}).ToArray());
            }
        }
    }
    private static float YzError(Transform car,Vector3 a,Vector3 b)
    {a=car.InverseTransformPoint(a);b=car.InverseTransformPoint(b);return MathF.Sqrt(MathF.Pow(a.y-b.y,2)+MathF.Pow(a.z-b.z,2));}
    private static float PrecisionPin(Mesh edited,Vector3[] native,float originalPin)
    {
        int representative=Enumerable.Range(0,native.Length).First(i=>Math.Abs(native[i].z-originalPin)<.14f);
        return originalPin+edited.vertices[representative].z-native[representative].z;
    }
    private static void IntoBufferCases(JsonElement native)
    {
        var data=native.GetProperty("meshes").GetProperty("s282_mech_push_rod_to_connect");
        var source=data.GetProperty("vertices").EnumerateArray().Select(V).ToArray();var snapshot=source.ToArray();var destination=new Vector3[source.Length];
        float[] before={0,3.15f},after={0,3.35f};
        _=GC.GetAllocatedBytesForCurrentThread();
        for(int i=0;i<1000;i++)RodRemesher.RemeshInto(source,before,after,destination);
        allocationProbeBatches=new long[6];
        for(int batch=0;batch<allocationProbeBatches.Length;batch++)
        {
            long start=GC.GetAllocatedBytesForCurrentThread();
            for(int i=0;i<1000;i++)RodRemesher.RemeshInto(source,before,after,destination);
            allocationProbeBatches[batch]=GC.GetAllocatedBytesForCurrentThread()-start;
        }
        remeshIntoAllocatedBytes=allocationProbeBatches.Skip(3).Max();
        Check(remeshIntoAllocatedBytes==0,"Caller-buffer rod deformation allocates managed memory per frame: "+remeshIntoAllocatedBytes+" bytes for1000calls");
        Check(source.SequenceEqual(snapshot),"Caller-buffer deformation mutated source");
        var expected=RodRemesher.Remesh(source,before,after);Check(destination.SequenceEqual(expected),"Reusable buffer differs from owned-array API");
        bool reject=false;try{RodRemesher.RemeshInto(source,before,after,source);}catch(ArgumentException){reject=true;}
        Check(reject,"Aliased source/output buffer accepted");
    }
    private static void ConnectingCase(JsonElement rods,JsonElement animation,JsonElement wheelData,JsonElement motion,JsonElement guides,int id,bool rotated,bool sharedLod,string output)
    {
        float rearward=ExpectedRearward(id);
        var f=new ConnectingRig(rods,animation,wheelData,motion,guides,rotated,sharedLod,output);var r=f.rig;r.Legacy(id);float cylinderShift=id==44?r.owner.PreparePrairieFixture():0f;float retargetShift=rearward+cylinderShift;f.NativeLodMetadata(motion);
        r.owner.ApplyPitch(id);
        if(sharedLod)r.body.Find("Static_LOD1").gameObject.SetActive(false);
        if(sharedLod)r.sparks[0][0].gameObject.SetActive(false); // Selected spark can be presentation-hidden.
        var cylinderFrames=r.sides.ToDictionary(d=>d,d=>d.localToWorldMatrix*Matrix4x4.TRS(Vector3.forward*(cylinderShift/d.TransformVector(Vector3.forward).magnitude),Quaternion.identity,Vector3.one));
        var fixedPositions=f.fixedParts.ToDictionary(t=>t,t=>t.position);var supportBase=f.supports.ToDictionary(t=>t,t=>(position:t.position,active:t.gameObject.activeSelf));
        var fittingBase=r.fittings.ToDictionary(p=>p.t,p=>p.t.position);var sparkBase=r.sparks.SelectMany(s=>s).ToDictionary(t=>t,t=>t.position);
        var wheelBase=r.wheels.SelectMany(s=>s).ToDictionary(t=>t,t=>r.owner.ResolvePresentedWheel(t).position);
        var lodBaseline=r.groups.ToDictionary(g=>g,g=>(lods:g.GetLODs(),point:g.localReferencePoint,size:g.size,worldToLocal:g.transform.worldToLocalMatrix));
        var sourceFilters=f.motions.ToDictionary(m=>m.source.GetComponent<MeshFilter>(),m=>m.mesh);
        var sourceFrames=f.motions.ToDictionary(m=>m.source,m=>(position:m.source.localPosition,rotation:m.source.localRotation,scale:m.source.localScale));
        r.owner.ApplyConnectingFixture(id,cylinderShift);connectingCases++;
        Check(r.owner.ConnectingRodSpacingActive,"Connecting spacing active state not set");Near(r.owner.DriverGroupRearwardM,rearward,"Requested whole-group rearward distance");
        Check(r.owner.ConnectingRodSpacingRetargetCount==(Math.Abs(retargetShift)<.0001f?0:2),"Unexpected precision retarget count");
        var axis=r.owner.transform.TransformDirection(Vector3.forward);
        foreach(var frame in cylinderFrames)foreach(var point in new[]{Vector3.zero,new Vector3(1.08f,.71f,8.8f),new Vector3(-1.08f,.4f,9.4f),new Vector3(0,1,6)})
            Near(r.owner.CylinderFrameForFixture(frame.Key).MultiplyPoint3x4(point),frame.Value.MultiplyPoint3x4(point),"Actual cylinder-frame helper did not cancel driver root/countershift",.0001f);
        foreach(var part in fixedPositions)Near(part.Key.position,part.Value,"Accepted cylinder/guide static position changed");
        foreach(var part in supportBase){Near(part.Key.position,part.Value.position-axis*rearward,"Lifting support does not follow driver group once");Check(part.Key.gameObject.activeSelf==part.Value.active,"Support visibility changed");}
        foreach(var fitting in r.fittings.Where(p=>p.t.gameObject.activeSelf))Near(fitting.t.position,fittingBase[fitting.t]-axis*rearward,"Selected spring/caliper did not follow whole group under visible/hidden LOD parent");
        foreach(var spark in r.sparks.SelectMany(s=>s.Take(r.owner.ActiveDriverLayout.Drivers.Count)))Near(spark.position,sparkBase[spark]-axis*rearward,"Selected visible/hidden spark did not follow whole group exactly once");
        foreach(var wheel in wheelBase)Near(r.owner.ResolvePresentedWheel(wheel.Key).position,wheel.Value-axis*rearward,"Whole driver presentation does not follow group");
        r.owner.AlignPitch(id);
        foreach(int n in r.owner.ActiveDriverLayout.Drivers)Near(r.axles[n-1].position,r.owner.ResolvePresentedWheel(r.wheels[1][n-1]).position,"Physical axle resolver missed whole group translation");
        foreach(var cup in f.cups)r.owner.BindOilCupPresentation(cup.provider,cup.consumer);
        var ownedMeshes=new HashSet<Mesh>();
        foreach(var clip in animation.GetProperty("clips").EnumerateArray())
        {
            var frames=clip.GetProperty("frames").EnumerateArray().ToArray();
            for(int phase=0;phase<frames.Length;phase++)
            {
                f.Poses(clip,frames[phase],frames[(phase+frames.Length/4)%frames.Length]);
                var nativeTransforms=f.motions.ToDictionary(m=>m.source,m=>(m.source.localPosition,m.source.localRotation,m.source.localScale));
                foreach(var cup in f.cups)cup.consumer.Sync(); // Native consumer may run first.
                f.TickRetargets();connectingPoses+=2;
                foreach(var native in nativeTransforms){Near(native.Key.localPosition,native.Value.localPosition,"Retarget moved an animated source");Near(native.Key.localRotation,native.Value.localRotation,"Retarget rotated an animated source");Near(native.Key.localScale,native.Value.localScale,"Retarget scaled an animated source");}
                if(Math.Abs(retargetShift)>.0001f)
                foreach(var entry in f.motions)
                {
                    var proxy=f.Proxy(entry.source);Near(proxy.localScale,Vector3.one,"Precision presentation scales bearing geometry");
                    var original=entry.mesh;var edited=proxy.GetComponent<MeshFilter>().sharedMesh;
                    bool stretch=entry.source.name.StartsWith("s282_mech_push_rod_to_connect")||entry.source.name.StartsWith("s282_mech_cutoff_to_cylinder_rod");
                    if(!stretch)
                    {
                        float error=(proxy.position-(entry.source.position+axis*retargetShift)).magnitude;maximumCrossheadHoldError=Math.Max(maximumCrossheadHoldError,error);
                        Check(error<.0001f,"Cylinder-side rendered gear changed its old world position");continue;
                    }
                    ownedMeshes.Add(edited);float pin=entry.source.name.StartsWith("s282_mech_push_rod_to_connect")?3.15f:1.56f;
                    float newPin=PrecisionPin(edited,entry.positions,pin);var outputVertices=edited.vertices;
                    Near(proxy.position,entry.source.position,"Connecting rod lost its crank/expansion-link pivot");
                    Near(proxy.TransformPoint(new Vector3(0,0,newPin)),entry.source.TransformPoint(new Vector3(0,0,pin))+axis*retargetShift,"True native far pin not held at cylinder side",.00012f);
                    for(int band=0;band<2;band++)
                    {
                        float centre=band==0?0:pin;var ids=Enumerable.Range(0,entry.positions.Length).Where(i=>Math.Abs(entry.positions[i].z-centre)<=.14f).ToArray();Check(ids.Length>0,"Native rod boss region missing");
                        float delta=band==0?0:newPin-pin;
                        foreach(int i in ids)
                        {
                            var expected=entry.positions[i]+Vector3.forward*delta;float error=(outputVertices[i]-expected).magnitude;maximumConnectingBossError=Math.Max(maximumConnectingBossError,error);
                            Check(error<.000001f,"Precision deformation distorted a bearing boss");
                        }
                    }
                    Check(edited.triangles.SequenceEqual(entry.triangles)&&edited.uv.SequenceEqual(original.uv),"Precision mesh topology or UV channel changed");
                    Near(edited.bounds.min.z,outputVertices.Min(p=>p.z),"Live rod lower bound stale");Near(edited.bounds.max.z,outputVertices.Max(p=>p.z),"Live rod upper bound stale");
                    for(int i=0;i<entry.triangles.Length;i+=3)
                    {
                        int a=entry.triangles[i],b=entry.triangles[i+1],c=entry.triangles[i+2];
                        var before=Vector3.Cross(entry.positions[b]-entry.positions[a],entry.positions[c]-entry.positions[a]);
                        if(before.magnitude<1e-10f)continue;
                        var after=Vector3.Cross(outputVertices[b]-outputVertices[a],outputVertices[c]-outputVertices[a]);
                        Check(after.magnitude>1e-10f&&Vector3.Dot(before,after)>0,"Live precision rod folded/collapsed a native face");connectingFaceChecks++;
                    }
                    var drive=entry.source;while(drive.parent!=r.moving)drive=drive.parent;
                    if(pin==3.15f)
                    {
                        var crosshead=drive.Find("s282_mech_push_rod");var crossProxy=f.Proxy(crosshead);
                        float error=YzError(r.owner.transform,proxy.TransformPoint(new Vector3(0,0,newPin)),crossProxy.TransformPoint(new Vector3(0,.013806f,-.923880f)));
                        maximumMainCrossheadErrorYZ=Math.Max(maximumMainCrossheadErrorYZ,error);Check(error<.001f,"Main far pin separates from independently fitted native crosshead attachment");
                    }
                    else
                    {
                        var hinge=f.Proxy(drive.Find("s282_mech_cutoff_rod_hinge"));
                        float error=YzError(r.owner.transform,proxy.TransformPoint(new Vector3(0,0,newPin)),hinge.position);
                        maximumRadiusHingeErrorYZ=Math.Max(maximumRadiusHingeErrorYZ,error);Check(error<.001f,"Radius rod separates from native combination-lever pivot");
                    }
                }
                foreach(var cup in f.cups)
                {
                    Near(cup.consumer.transform.position,cup.provider.transform.position,"Native cup consumer lags visible proxy");
                    if(Math.Abs(retargetShift)>.0001f&&cup.provider.transform.name!="Oiling0")Near(cup.provider.transform.position,f.Proxy(cup.provider.transform.parent).TransformPoint(cup.local),"Native oil cup does not follow rendered rod/crosshead");
                    connectingOilSamples++;
                }
                foreach(var cup in f.cups)cup.consumer.Sync(); // Retarget may run first.
                if(Math.Abs(retargetShift)>.0001f)foreach(var group in r.groups)
                foreach(var renderer in group.GetLODs().SelectMany(l=>l.renderers).Where(v=>v.name.StartsWith("[Rearranged retarget]",StringComparison.Ordinal)).Distinct())
                {
                    var mesh=renderer.GetComponent<MeshFilter>().sharedMesh;var bounds=mesh.bounds;
                    float newEscape=0,nativeEscape=0;
                    foreach(float x in new[]{bounds.min.x,bounds.max.x})foreach(float y in new[]{bounds.min.y,bounds.max.y})foreach(float z in new[]{bounds.min.z,bounds.max.z})
                    {
                        var local=group.transform.InverseTransformPoint(renderer.transform.TransformPoint(new Vector3(x,y,z)))-group.localReferencePoint;
                        float half=group.size*.5f+.0001f;
                        float escape=Math.Max(Math.Abs(local.x),Math.Max(Math.Abs(local.y),Math.Abs(local.z)))-half;
                        if(escape>maximumPrecisionLodEscape){maximumPrecisionLodEscape=escape;worstPrecisionLodEscape=$"id{id} shift{rearward} group{group.name} renderer{renderer.name} local({local.x:R},{local.y:R},{local.z:R}) half{half:R} phase{phase} clip{clip.GetProperty("name").GetString()}";}
                        newEscape=Math.Max(newEscape,escape);
                        precisionLodBoundSamples++;
                    }
                    var originalEntry=f.motions.Single(m=>m.source==renderer.transform.parent);var originalBounds=originalEntry.mesh.bounds;var baseline=lodBaseline[group];
                    foreach(float x in new[]{originalBounds.min.x,originalBounds.max.x})foreach(float y in new[]{originalBounds.min.y,originalBounds.max.y})foreach(float z in new[]{originalBounds.min.z,originalBounds.max.z})
                    {
                        var beforeWorld=originalEntry.source.TransformPoint(new Vector3(x,y,z))+axis*rearward;
                        var beforeLocal=baseline.worldToLocal.MultiplyPoint3x4(beforeWorld)-baseline.point;
                        nativeEscape=Math.Max(nativeEscape,Math.Max(Math.Abs(beforeLocal.x),Math.Max(Math.Abs(beforeLocal.y),Math.Abs(beforeLocal.z)))-baseline.size*.5f);
                    }
                    maximumIntroducedLodEscape=Math.Max(maximumIntroducedLodEscape,newEscape-nativeEscape);
                }
            }
        }
        foreach(var entry in f.motions)Check(entry.mesh.FixturePositions.SequenceEqual(entry.positions),"Original native geometry mutated");
        r.owner.SuspendConnectingFixture();Check(!r.owner.ConnectingRodSpacingActive&&r.owner.ConnectingRodSpacingRetargetCount==0,"Connecting suspension retains active state");
        Check(ownedMeshes.All(m=>m.destroyed),"Connecting suspension leaked dynamic edited mesh");
        foreach(var filter in sourceFilters)Check(ReferenceEquals(filter.Key.sharedMesh,filter.Value),"Connecting suspension failed to restore source mesh");
        foreach(var state in lodBaseline)
        {
            var actual=state.Key.GetLODs();for(int i=0;i<actual.Length;i++)Check(actual[i].renderers.SequenceEqual(state.Value.lods[i].renderers),"Outer retarget did not restore pitched LOD list");
            Near(state.Key.localReferencePoint,state.Value.point,"Outer retarget did not restore LOD reference point");Near(state.Key.size,state.Value.size,"Outer retarget did not restore culling size");
        }
        foreach(var part in fixedPositions)Near(part.Key.position,part.Value,"Connecting cleanup moved static cylinder/guide");
        for(int repeat=0;repeat<2;repeat++)
        {
            r.owner.ApplyConnectingFixture(id,cylinderShift);foreach(var cup in f.cups)r.owner.BindOilCupPresentation(cup.provider,cup.consumer);
            r.owner.SuspendConnectingFixture();Check(r.owner.DriverLayoutPresentationCount==(Three(id)?6:4),"Connecting suspend removed coupling pitch presentation");
        }
        r.owner.DisposeConnectingFixture();r.owner.SuspendPitch();r.owner.DisposePitch();
    }
    private static void ConnectingRollback(JsonElement rods,JsonElement anim,JsonElement wheels,JsonElement motion,JsonElement guides,string output)
    {
        foreach(int failAt in new[]{1,2})
        {
            var f=new ConnectingRig(rods,anim,wheels,motion,guides,false,true,output);var r=f.rig;r.Legacy(34);f.NativeLodMetadata(motion);r.owner.ApplyPitch(34);
            var before=r.sides.ToDictionary(t=>t,t=>t.position);
            var lods=r.groups.ToDictionary(g=>g,g=>(levels:g.GetLODs(),point:g.localReferencePoint,size:g.size));
            MeshFinder.FailAtRequest=MeshFinder.Requests.Count+failAt;bool rejected=false;
            try{r.owner.ApplyConnectingFixture(34);}catch(InvalidOperationException){rejected=true;}finally{MeshFinder.FailAtRequest=-1;}
            Check(rejected,"Unreadable radius rod silently fell back to scaled precision geometry");connectingRollbackCases++;
            Check(!r.owner.ConnectingRodSpacingActive&&r.owner.ConnectingRodSpacingRetargetCount==0,"Failed precision bind retains active spacing");
            foreach(var drive in before){Near(drive.Key.position,drive.Value,"Failed precision bind did not restore driver root");Check(drive.Key.GetComponents<DriveRetarget>().Length==0,"Failed precision bind leaked retarget component");}
            foreach(var state in lods)
            {
                for(int i=0;i<state.Value.levels.Length;i++)Check(state.Key.GetLODs()[i].renderers.SequenceEqual(state.Value.levels[i].renderers),"First/second-drive precision failure lost shared pitched LOD list");
                Near(state.Key.localReferencePoint,state.Value.point,"Precision failure did not restore LOD reference");Near(state.Key.size,state.Value.size,"Precision failure did not restore LOD size");
            }
            Check(r.owner.DriverLayoutPresentationCount==6,"Failed precision bind destroyed prior coupling pitch");
            r.owner.DisposeConnectingFixture();r.owner.DisposePitch();
        }
    }
    internal static void RunConnectingTests(string[] args)
    {
        if(args.Length!=7)throw new ArgumentException("Expected rod/animation/wheel/motion/report/output/guide fixture paths");
        using var rods=JsonDocument.Parse(File.ReadAllText(args[0]));using var animation=JsonDocument.Parse(File.ReadAllText(args[1]));using var wheels=JsonDocument.Parse(File.ReadAllText(args[2]));using var motion=JsonDocument.Parse(File.ReadAllText(args[3]));using var guides=JsonDocument.Parse(File.ReadAllText(args[6]));
        foreach(int id in new[]{34,35,44,0}){Near(WheelRearranger.RequestedDriverPitchM(id),ExpectedPitch(id),"Fixed pitch for named layout");Near(WheelRearranger.RequestedDriverRearwardM(id),ExpectedRearward(id),"Fixed rearward distance for named layout");}
        IntoBufferCases(motion.RootElement);
        foreach(int id in new[]{34,35,44,0})foreach(bool turned in new[]{false,true})foreach(bool sharedLod in new[]{false,true})
            ConnectingCase(rods.RootElement,animation.RootElement,wheels.RootElement,motion.RootElement,guides.RootElement,id,turned,sharedLod,args[5]);
        ConnectingRollback(rods.RootElement,animation.RootElement,wheels.RootElement,motion.RootElement,guides.RootElement,args[5]);
        Check(Dummiesman.OBJLoader.Requests.Contains("s282_mech_push_rod_to_connect"),"Unreadable native main did not use existing readable export");
        Check(MeshFinder.Requests.Contains("s282_mech_cutoff_to_cylinder_rod"),"Unreadable radius did not request runtime readable provider");
        Check(maximumIntroducedLodEscape<.0001f,$"Precision LOD envelope adds{maximumIntroducedLodEscape:R} localmetres of escape beyond native baseline: {worstPrecisionLodEscape}");
        var report=new{passed=true,checks,connectingCases,connectingPoses,connectingOilSamples,connectingFaceChecks,precisionLodBoundSamples,connectingRollbackCases,remeshIntoAllocatedBytes,allocationProbeBatches,
            maximumMainCrossheadErrorYZM=maximumMainCrossheadErrorYZ,maximumRadiusHingeErrorYZM=maximumRadiusHingeErrorYZ,maximumCrossheadHoldErrorM=maximumCrossheadHoldError,maximumConnectingBossErrorM=maximumConnectingBossError,
            maximumPrecisionLodEscapeM=maximumPrecisionLodEscape,
            maximumIntroducedLodEscapeM=maximumIntroducedLodEscape,
            limitation="Actual production spacing/pitch/retarget/deformation/oil-owner bodies with managed Unity transform/LOD/mesh stand-ins, actual unreadable flags and native geometry/animation. Readable provider uses registered native data as an explicit stand-in for AssetStudio/OBJ; actual exporter integration, native wheel-manager physics/culling, live oil interaction and game visuals require separate checks. Fitted native joint offsets are independent evidence; existing authored animation error remains."};
        File.WriteAllText(args[4],JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(report));
    }
}
