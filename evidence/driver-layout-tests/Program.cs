using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DV.Wheels;
using RearrangedS282;
using UnityEngine;
using ModMain=RearrangedS282.Main;

internal static class Program
{
    private static bool Three(int id)=>id==34||id==44;
    private static bool Two(int id)=>id==35||id==0;
    private static int checks,poses,layoutCases,lifecycleCycles,phaseReapplyCases,switchCases,rollbackCases,phasedInitialCacheCases;
    private static float maximumPinAddedError,maximumSeatedPinErrorYZ,maximumSeatedPinErrorZ,maximumLegacyPinErrorYZ,maximumAddedVerticalPinError,maximumDerivedVerticalBound;
    private static readonly Dictionary<string,float> seatedByLayout=new(),legacyByLayout=new();
    private static void Check(bool ok,string message){checks++;if(!ok)throw new InvalidOperationException(message);}
    private static void Near(float a,float b,string message,float epsilon=3e-5f)=>Check(Math.Abs(a-b)<epsilon,message+$": {a:R} vs {b:R}");
    private static void Near(Vector3 a,Vector3 b,string message,float epsilon=8e-5f)=>Check((a-b).magnitude<epsilon,message+$": error {(a-b).magnitude:R}");
    private static void Near(Quaternion a,Quaternion b,string message)=>Check(Math.Abs(a.x-b.x)+Math.Abs(a.y-b.y)+Math.Abs(a.z-b.z)+Math.Abs(a.w-b.w)<1e-5,message);
    private static Vector3 V(JsonElement v)=>new(v[0].GetSingle(),v[1].GetSingle(),v[2].GetSingle());
    private static Quaternion Q(JsonElement v)=>new(v[0].GetSingle(),v[1].GetSingle(),v[2].GetSingle(),v[3].GetSingle());
    private static Transform Child(Transform parent,string name){var t=new GameObject(name).transform;t.SetParent(parent,false);return t;}
    private static Transform Path(Transform parent,string path){foreach(string name in path.Split('/'))parent=parent.Find(name)??Child(parent,name);return parent;}
    private static Mesh MeshFrom(JsonElement data,string name)
    {
        var mesh=new Mesh{name=name,vertices=data.GetProperty("vertices").EnumerateArray().Select(V).ToArray(),
            triangles=data.GetProperty("triangles").EnumerateArray().SelectMany(t=>t.ValueKind==JsonValueKind.Array?t.EnumerateArray().Select(v=>v.GetInt32()):new[]{t.GetInt32()}).ToArray()};
        mesh.uv=Enumerable.Range(0,mesh.vertexCount).Select(i=>i*.011f).ToArray();
        mesh.colors=Enumerable.Range(0,mesh.vertexCount).Select(i=>i*.013f).ToArray();
        mesh.RecalculateBounds();return mesh;
    }
    private static MeshRenderer Visual(Transform t,Mesh mesh)
    {
        t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=t.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterials=new object[] {new object()};
        renderer.shadowCastingMode=2;renderer.receiveShadows=false;renderer.lightProbeUsage=2;renderer.reflectionProbeUsage=3;
        return renderer;
    }

    private sealed class Rig
    {
        internal readonly WheelRearranger owner;
        internal readonly Transform body,moving;
        internal readonly Dictionary<string,Mesh> rods=new();
        internal readonly Dictionary<(int axle,int lod),Mesh> wheelMeshes=new();
        internal readonly Dictionary<Transform,(Vector3 position,Quaternion rotation,Vector3 scale)> nativePose=new();
        internal readonly List<LODGroup> groups=new();
        internal readonly List<(Transform t,string lod,string side,string kind,int number)> fittings=new();
        internal readonly Transform[][] wheels=new Transform[2][],shoes=new Transform[2][],sparks=new Transform[2][];
        internal readonly Transform[] axles=new Transform[6],sides=new Transform[2];
        internal readonly Mesh[][] rodSources=new Mesh[2][];
        private readonly Dictionary<Transform,Mesh> nativeFilters=new();
        internal readonly JsonElement animation;
        internal readonly Mesh[] nativeMeshes;
        internal Rig(JsonElement rodData,JsonElement anim,JsonElement wheelData,bool rotated,bool sharedLod=false,bool phasedCache=false)
        {
            animation=anim;
            owner=new GameObject("S282 managed layout fixture").AddComponent<WheelRearranger>();
            var prefab=new GameObject("independent native S282 prefab");
            var prefabMain=anim.GetProperty("transforms").EnumerateArray().Single(t=>t.GetProperty("path").GetString().EndsWith("/s282_mech_wheels_connect/s282_mech_push_rod_to_connect"));
            foreach(string side in new[]{"L","R"})
            {
                var canonical=Path(prefab.transform,"LocoS282A_Body/MovingParts_LOD0/LocoS282A_Drivetrain "+side+"/s282_mech_wheels_connect/s282_mech_push_rod_to_connect");
                canonical.localPosition=V(prefabMain.GetProperty("position"));
            }
            owner.loco=new TrainCar{carLivery=new FixtureLivery{prefab=prefab}};
            owner.transform.localPosition=rotated?new Vector3(53,4,-120):Vector3.zero;
            owner.transform.localRotation=rotated?Quaternion.Euler(0,63,0):Quaternion.identity;
            body=Child(owner.transform,"LocoS282A_Body");moving=Child(body,"MovingParts_LOD0");
            foreach(var m in rodData.GetProperty("meshes").EnumerateArray())rods.Add(m.GetProperty("name").GetString(),MeshFrom(m,m.GetProperty("name").GetString()));
            foreach(var w in wheelData.GetProperty("wheels").EnumerateArray())
                wheelMeshes.Add((w.GetProperty("axle").GetInt32(),w.GetProperty("lod").GetInt32()),MeshFrom(w,w.GetProperty("name").GetString()));
            owner.flangedDriver=wheelMeshes[(1,0)];
            for(int side=0;side<2;side++)
            {
                var drive=Child(moving,"LocoS282A_Drivetrain "+(side==0?"L":"R"));sides[side]=drive;
                drive.localScale=new Vector3(side==0?-1:1,1,1);
                foreach(var row in anim.GetProperty("transforms").EnumerateArray())
                {
                    string path=row.GetProperty("path").GetString().Split("/LocoS282A_Drivetrain R/").Last();
                    if(path.EndsWith("LocoS282A_Drivetrain R"))continue;
                    var t=Path(drive,path);t.localPosition=V(row.GetProperty("position"));t.localRotation=Q(row.GetProperty("rotation"));t.localScale=V(row.GetProperty("scale"));
                }
                drive.gameObject.AddComponent<Animator>();
                wheels[side]=Enumerable.Range(1,4).Select(n=>drive.Find("s282_wheels_driving_"+n)).ToArray();
                for(int i=0;i<4;i++)
                {
                    var wheel=wheels[side][i];var coarse=wheel.Find(wheel.name+"_LOD1")??Child(wheel,wheel.name+"_LOD1");
                    Visual(wheel,wheelMeshes[(i+1,0)]);Visual(coarse,wheelMeshes[(i+1,1)]);
                    wheel.gameObject.AddComponent<Collider>();wheel.gameObject.AddComponent<Rigidbody>();
                }
                var rod=drive.Find("s282_mech_wheels_connect");Visual(rod,rods["native"]);
                var rodCoarse=rod.Find("s282_mech_wheels_connect_LOD1")??Child(rod,"s282_mech_wheels_connect_LOD1");Visual(rodCoarse,rods["nativeLOD1"]);
                Transform Segment(string name,Mesh mesh,Vector3 position,Quaternion rotation,Vector3 scale)
                {var part=Child(rod,name);part.localPosition=position;part.localRotation=rotation;part.localScale=scale;Visual(part,mesh);part.gameObject.SetActive(false);return part;}
                var front=Segment("front three-axle segment",rods["threeHalf"],new Vector3(0,0,-3.11f),Quaternion.AngleAxis(180,Vector3.up),Vector3.one);
                var rear=Segment("rear three-axle segment",rods["threeHalf"],Vector3.zero,Quaternion.AngleAxis(180,Vector3.up),new Vector3(1,1,-1));
                var two=Segment("two-axle segment",rods["two"],new Vector3(0,0,-1.38f),Quaternion.AngleAxis(180,new Vector3(0,0,1)),Vector3.one);
                if(side==0){owner.driveL=drive;owner.leftSideRod=rod;owner.lfThreeAxleSideRod=front;owner.lrThreeAxleSideRod=rear;owner.leftTwoAxleSideRod=two;
                    owner.firstLeftDriveWheel=wheels[side][0];owner.secondLeftDriveWheel=wheels[side][1];owner.thirdLeftDriveWheel=wheels[side][2];owner.fourthLeftDriveWheel=wheels[side][3];}
                else{owner.driveR=drive;owner.rightSideRod=rod;owner.rfThreeAxleSideRod=front;owner.rrThreeAxleSideRod=rear;owner.rightTwoAxleSideRod=two;
                    owner.firstRightDriveWheel=wheels[side][0];owner.secondRightDriveWheel=wheels[side][1];owner.thirdRightDriveWheel=wheels[side][2];owner.fourthRightDriveWheel=wheels[side][3];}
                rodSources[side]=new[]{rods["threeHalf"],rods["threeHalf"],rods["two"]};
                var shoeParent=Child(drive,"s282a_brake_shoes(Clone)");
                shoes[side]=Enumerable.Range(1,4).Select(n=>{var t=Child(shoeParent,"s282a_brake_shoe_"+n);t.localPosition=new Vector3(0,0,new[]{8.18f,6.47f,4.92f,3.37f}[n-1]);return t;}).ToArray();
                sparks[side]=Enumerable.Range(1,6).Select(n=>{var t=Child(owner.transform,"spark "+side+" "+n);t.localPosition=new Vector3(side==0?-.75f:.75f,0,8-n*1.5f);return t;}).ToArray();
            }
            owner.leftFirstBrakeShoe=shoes[0][0];owner.leftSecondBrakeShoe=shoes[0][1];owner.leftThirdBrakeShoe=shoes[0][2];owner.leftFourthBrakeShoe=shoes[0][3];
            owner.rightFirstBrakeShoe=shoes[1][0];owner.rightSecondBrakeShoe=shoes[1][1];owner.rightThirdBrakeShoe=shoes[1][2];owner.rightFourthBrakeShoe=shoes[1][3];
            owner.spark1L=sparks[0][0];owner.spark2L=sparks[0][1];owner.spark3L=sparks[0][2];owner.spark4L=sparks[0][3];owner.spark5L=sparks[0][4];owner.spark6L=sparks[0][5];
            owner.spark1R=sparks[1][0];owner.spark2R=sparks[1][1];owner.spark3R=sparks[1][2];owner.spark4R=sparks[1][3];owner.spark5R=sparks[1][4];owner.spark6R=sparks[1][5];
            foreach(string lod in new[]{"Static_LOD0","Static_LOD1"})
            {
                var split=Child(Child(body,lod),"s282a_split_body(Clone)");
                foreach(string side in new[]{"l","r"})foreach(string kind in new[]{"caliper","suspension"})for(int number=1;number<=4;number++)
                {
                    if(lod=="Static_LOD1" && kind=="suspension")continue; // Actual LMS native availability.
                    var part=Child(split,"s282a_"+kind+"_"+side+"_"+number);
                    fittings.Add((part,lod,side,kind,number));
                }
            }
            var bogieFront=Child(owner.transform,"front bogie");var bogieRear=Child(owner.transform,"rear bogie");
            bogieFront.localRotation=Quaternion.AngleAxis(17,Vector3.up);bogieRear.localRotation=Quaternion.AngleAxis(-23,Vector3.up);
            for(int i=0;i<6;i++){axles[i]=Child(i<2?bogieFront:bogieRear,"axle "+i);axles[i].gameObject.AddComponent<PoweredWheel>().wheelTransform=wheels[1][Math.Min(i,3)];}
            owner.driveAxle1=axles[0];owner.driveAxle2=axles[1];owner.driveAxle3=axles[2];owner.driveAxle4=axles[3];owner.driveAxle5=axles[4];owner.driveAxle6=axles[5];
            var originalRenderers=sides.Select(d=>d.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.transform.parent==d||r.name.EndsWith("LOD1")).Cast<Renderer>().ToArray()).ToArray();
            IEnumerable<Transform> groupOwners=sharedLod?new[]{moving}:sides;
            foreach(var groupOwner in groupOwners)
            {
                var group=groupOwner.gameObject.AddComponent<LODGroup>();
                var relevant=sharedLod?originalRenderers.SelectMany(r=>r).ToArray():originalRenderers[Array.IndexOf(sides,groupOwner)];
                group.SetLODs(new[]{new LOD(.6f,relevant.Where(r=>!r.name.EndsWith("LOD1")).ToArray()){fadeTransitionWidth=.21f},new LOD(.1f,relevant.Where(r=>r.name.EndsWith("LOD1")).ToArray()){fadeTransitionWidth=.13f}});
                group.localReferencePoint=new Vector3(1,2,3);group.size=12.4f;groups.Add(group);
            }
            var unrelated=Child(body,"unrelated LOD").gameObject.AddComponent<LODGroup>();
            var sentinel=Visual(Child(unrelated.transform,"unrelated renderer"),rods["native"]);
            unrelated.SetLODs(new[]{new LOD(.2f,new Renderer[]{sentinel})});unrelated.localReferencePoint=new Vector3(4,5,6);unrelated.size=2.3f;groups.Add(unrelated);
            if(phasedCache)
            {
                var extremes=anim.GetProperty("clips").EnumerateArray().SelectMany(clip=>clip.GetProperty("frames").EnumerateArray().Select(frame=>
                {
                    var b=clip.GetProperty("bindings").EnumerateArray().Single(b=>b.GetProperty("path").GetString()=="s282_mech_wheels_connect/s282_mech_push_rod_to_connect"&&b.GetProperty("attribute").GetInt32()==1);
                    int index=b.GetProperty("indices")[2].GetInt32();float z=frame.GetProperty("values").TryGetProperty(index.ToString(),out var value)?value.GetSingle():b.GetProperty("first")[2].GetSingle();
                    return (clip,frame,z);
                })).OrderBy(row=>row.z).ToArray();
                PoseDrive(sides[0],extremes[0].clip,extremes[0].frame);
                PoseDrive(sides[1],extremes[^1].clip,extremes[^1].frame);
            }
            foreach(var t in owner.GetComponentsInChildren<Transform>(true))nativePose.Add(t,(t.localPosition,t.localRotation,t.localScale));
            foreach(var filter in owner.GetComponentsInChildren<MeshFilter>(true))nativeFilters.Add(filter.transform,filter.sharedMesh);
            nativeMeshes=owner.GetComponentsInChildren<MeshFilter>(true).Select(f=>f.sharedMesh).Distinct().ToArray();
            owner.CachePitch();
        }
        internal void Legacy(int id)
        {
            foreach(var pair in nativePose){pair.Key.localPosition=pair.Value.position;pair.Key.localRotation=pair.Value.rotation;pair.Key.localScale=pair.Value.scale;pair.Key.gameObject.SetActive(true);}
            foreach(var pair in nativeFilters)pair.Key.GetComponent<MeshFilter>().sharedMesh=pair.Value;
            foreach(var d in sides)
            {
                d.localPosition=new Vector3(0,0,Three(id)?-2.1f:-3.935f);d.localScale=new Vector3(d==owner.driveL?-1:1,Three(id)?1.2879f:1.42f,Three(id)?1.2879f:1.42f);
                d.Find("s282_wheels_driving_1").localPosition=new Vector3(0,.71f,8f);
                d.Find("s282_wheels_driving_2").localPosition=new Vector3(0,.71f,6.3f);
                d.Find("s282_wheels_driving_4").localPosition=new Vector3(0,.71f,3.2f);
                d.Find("s282_wheels_driving_1").gameObject.SetActive(false);
                d.Find("s282_wheels_driving_4").gameObject.SetActive(Three(id));
                d.Find("s282_wheels_driving_2").GetComponent<MeshFilter>().sharedMesh=owner.flangedDriver;
                if(Two(id)){d.Find("s282_wheels_driving_2").localPosition=new Vector3(0,.71f,6.62f);d.Find("s282_wheels_driving_3").GetComponent<MeshFilter>().sharedMesh=owner.flangedDriver;}
                // Existing legacy conversion rounds this authored constant;
                // the next native animation sample restores -1.55334 instead.
                var main=d.Find("s282_mech_wheels_connect/s282_mech_push_rod_to_connect");
                main.localPosition=new Vector3(.1f,0,-1.55f);
            }
            foreach(var r in new[]{owner.leftSideRod,owner.rightSideRod})r.GetComponent<MeshRenderer>().enabled=false;
            foreach(var r in new[]{owner.lfThreeAxleSideRod,owner.lrThreeAxleSideRod,owner.rfThreeAxleSideRod,owner.rrThreeAxleSideRod})r.gameObject.SetActive(Three(id));
            foreach(var r in new[]{owner.leftTwoAxleSideRod,owner.rightTwoAxleSideRod})r.gameObject.SetActive(Two(id));
            foreach(var f in fittings.Where(f=>f.lod=="Static_LOD0" && f.kind=="caliper"))
            {
                f.t.gameObject.SetActive(Three(id)?f.number!=1:f.number==1||f.number==3);
                f.t.localPosition=Three(id)?new Vector3(0,.09f,new[]{0f,-1.75f,-.49f,-2.49f}[f.number-1])
                    :new Vector3(0,.13f,new[]{-2.19f,0f,-1.59f,0f}[f.number-1]);
            }
            for(int side=0;side<2;side++)
            {
                shoes[side][0].gameObject.SetActive(Two(id));shoes[side][1].gameObject.SetActive(Three(id));shoes[side][2].gameObject.SetActive(true);shoes[side][3].gameObject.SetActive(Three(id));
                if(Two(id))shoes[side][0].localPosition=new Vector3(0,0,6.79f);
                var z=Three(id)?new[]{5.37f,3.37f,1.37f}:new[]{4.83f,2.2f};
                for(int i=0;i<6;i++){sparks[side][i].gameObject.SetActive(i<z.Length);if(i<z.Length)sparks[side][i].localPosition=new Vector3(side==0?-.75f:.75f,0,z[i]);}
            }
        }
        private static void PoseDrive(Transform drive,JsonElement clip,JsonElement frame)
        {
            var values=frame.GetProperty("values");
            foreach(var binding in clip.GetProperty("bindings").EnumerateArray())
            {
                var target=drive.Find(binding.GetProperty("path").GetString());if(target==null)continue;
                var ids=binding.GetProperty("indices").EnumerateArray().Select(v=>v.GetInt32()).ToArray();var first=binding.GetProperty("first").EnumerateArray().Select(v=>v.GetSingle()).ToArray();
                var v=ids.Select((id,i)=>values.TryGetProperty(id.ToString(),out var value)?value.GetSingle():first[i]).ToArray();
                switch(binding.GetProperty("attribute").GetInt32()){case 1:target.localPosition=new Vector3(v[0],v[1],v[2]);break;case 2:target.localRotation=new Quaternion(v[0],v[1],v[2],v[3]);break;case 3:target.localScale=new Vector3(v[0],v[1],v[2]);break;}
            }
        }
        internal void Pose(JsonElement clip,JsonElement frame)
        {
            foreach(var drive in sides)PoseDrive(drive,clip,frame);
            foreach(var rotater in owner.GetComponentsInChildren<PoweredWheelRotater>(false))rotater.LateUpdate();
        }
    }

    private static void DescriptorCases()
    {
        foreach(var pair in new[]{(float.NaN,.15f),(float.PositiveInfinity,.15f),(-100f,0f),(100f,.20f),(.154f,.15f),(.156f,.16f)})
            Near(DriverLayout.ClampExtraPitch(pair.Item1),pair.Item2,"Pitch clamp/default/quantization");
        Near(DriverLayout.Create(44,.22f).ExtraCoupledPitchM,.22f,"Prairie requested 22 cm is not clamped to 20 cm");
        Near(DriverLayout.Create(0,.22f).ExtraCoupledPitchM,.20f,"Prairie range extension does not widen other layouts");
        foreach(int id in new[]{34,35,44,0})
        {
            var layout=DriverLayout.Create(id,.15f);Near(layout.DriverOffsetM(3),0,"Descriptor main axle fixed");Near(layout.DriverOffsetM(2),.15f,"Descriptor front axle moves forward");
            if(Three(id))Near(layout.DriverOffsetM(4),-.15f,"Descriptor rear axle moves back");
        }
    }
    private static void LayoutCase(JsonElement rodData,JsonElement anim,JsonElement wheelData,int id,bool rotated,bool sharedLod,float pitch)
    {
        var rig=new Rig(rodData,anim,wheelData,rotated,sharedLod);rig.Legacy(id);
        ModMain.settings.prairieExtraCoupledPitchMetres=ModMain.settings.americanExtraCoupledPitchMetres=ModMain.settings.mogulExtraCoupledPitchMetres=ModMain.settings.twoFortyExtraCoupledPitchMetres=pitch;
        var selected=DriverLayout.Create(id,pitch);var active=selected.Drivers.ToArray();
        var originalMeshes=rig.nativeMeshes.ToDictionary(m=>m,m=>(positions:m.vertices.ToArray(),triangles:m.triangles.ToArray(),uv:m.uv.ToArray(),bounds:m.bounds));
        var lodStates=rig.groups.Select(g=>(group:g,lods:g.GetLODs(),point:g.localReferencePoint,size:g.size)).ToArray();
        var fittingBase=rig.fittings.ToDictionary(f=>f.t,f=>(position:f.t.position,local:f.t.localPosition,active:f.t.gameObject.activeSelf));
        var shoeBase=rig.shoes.SelectMany(s=>s).ToDictionary(t=>t,t=>t.position);var sparkBase=rig.sparks.SelectMany(s=>s).ToDictionary(t=>t,t=>t.position);
        var wheelBase=rig.wheels.SelectMany(w=>w).ToDictionary(t=>t,t=>(position:t.position,local:t.localPosition,scale:t.localScale));
        var originalFilter=rig.owner.GetComponentsInChildren<MeshFilter>(true).ToDictionary(f=>f,f=>f.sharedMesh);
        rig.owner.SeedCurveCache();rig.owner.ApplyPitch(id);layoutCases++;
        Check(rig.owner.CurveCacheCleared,"Curve layout cache not invalidated");
        Check(rig.owner.DriverLayoutPresentationCount==active.Length*2,"Presentation count does not match active native drivers");
        Near(rig.owner.DriverLayoutExtraPitchM,selected.ExtraCoupledPitchM,"Reported selected pitch");
        var presentations=rig.wheels.SelectMany(w=>w).Select(w=>rig.owner.ResolvePresentedWheel(w)).Where(w=>!wheelBase.ContainsKey(w)).ToArray();
        Check(presentations.Length==active.Length*2,"Resolver did not map every active driver once");
        foreach(var drive in rig.sides)
        foreach(int n in active)
        {
            var source=drive.Find("s282_wheels_driving_"+n);var visual=rig.owner.ResolvePresentedWheel(source);
            Near(visual.position,wheelBase[source].position+rig.owner.transform.TransformDirection(Vector3.forward)*selected.DriverOffsetM(n),"Driver offset is in world metres");
            Near(visual.localScale,wheelBase[source].scale,"Wheel scale retained");
            Check(visual.GetComponentsInChildren<Collider>(true).Length==0 && visual.GetComponentsInChildren<Rigidbody>(true).Length==0,"Wheel presentation copied a physical component");
            Check(source.GetComponent<MeshFilter>().sharedMesh==null && source.GetChild(0).GetComponent<MeshFilter>().sharedMesh==null,"Native wheel source mesh remains visible");
            var detail=visual.GetComponent<MeshFilter>().sharedMesh;var coarse=visual.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
            Check(ReferenceEquals(detail,originalFilter[source.GetComponent<MeshFilter>()]),"Detail mesh changed");
            Check(ReferenceEquals(coarse,ReferenceEquals(detail,rig.owner.flangedDriver)?rig.wheelMeshes[(1,1)]:rig.wheelMeshes[(n,1)]),"Coarse mesh has the wrong flange geometry");
            foreach(var group in rig.groups.Where(g=>g.GetLODs().Any(l=>l.renderers.Contains(visual.GetComponent<MeshRenderer>()))))
            {
                Check(group.GetLODs()[0].renderers.Contains(visual.GetComponent<MeshRenderer>()),"Detail wheel absent from detail LOD");
                Check(group.GetLODs()[1].renderers.Contains(visual.GetChild(0).GetComponent<MeshRenderer>()),"Coarse wheel absent from coarse LOD");
                Check(!group.GetLODs()[1].renderers.Contains(visual.GetComponent<MeshRenderer>()),"Detail wheel leaks into coarse LOD");
            }
        }
        rig.owner.AlignPitch(id);
        foreach(int n in active)
        {
            var visual=rig.owner.ResolvePresentedWheel(rig.wheels[1][n-1]);Near(rig.axles[n-1].position,visual.position,"Physical axle follows renderer across rotated bogie");
            Check(ReferenceEquals(rig.axles[n-1].GetComponent<PoweredWheel>().wheelTransform,visual),"Physical axle targets native instead of presentation");
        }
        foreach(var fitting in rig.fittings)
        {
            int n=Two(id) && fitting.kind=="caliper" && fitting.number==1?2:fitting.number;
            bool expected=active.Contains(n) && !(Two(id) && fitting.kind=="caliper" && fitting.number==2);
            Check(fitting.t.gameObject.activeSelf==expected,"Fitting active inventory mismatches selected axles");
            if(!expected)continue;
            var axis=rig.owner.transform.TransformDirection(Vector3.forward);
            if(fitting.kind=="caliper")
            {
                var baseline=fittingBase[fitting.t].position;
                if(fitting.lod=="Static_LOD1")
                {
                    var detail=rig.fittings.Single(f=>f.lod=="Static_LOD0"&&f.side==fitting.side&&f.kind==fitting.kind&&f.number==fitting.number);
                    baseline+=fittingBase[detail.t].position-rig.owner.transform.TransformPoint(Vector3.zero);
                }
                Near(fitting.t.position,baseline+axis*selected.DriverOffsetM(n),"Brake caliper detail/coarse follows selected axle");
            }
            else
            {
                int side=fitting.side=="l"?0:1;var source=rig.wheels[side][n-1];var visual=rig.owner.ResolvePresentedWheel(source);
                float sourceNativeZ=rig.nativePose[source].position.z;
                float currentZ=rig.owner.transform.InverseTransformPoint(visual.position).z;
                Near(fitting.t.position,fittingBase[fitting.t].position+axis*(currentZ-sourceNativeZ),"Spring follows actual displayed axle centre");
            }
        }
        for(int side=0;side<2;side++)for(int i=0;i<active.Length;i++)
        {
            var shoe=rig.shoes[side][Three(id)?i+1:i==0?0:2];var spark=rig.sparks[side][i];
            var shift=rig.owner.transform.TransformDirection(Vector3.forward)*selected.DriverOffsetM(active[i]);
            Near(shoe.position,shoeBase[shoe]+shift,"Brake shoe follows assigned driver");Near(spark.position,sparkBase[spark]+shift,"Spark follows assigned driver");
        }
        var positionBefore=presentations.ToDictionary(t=>t,t=>t.localPosition);var scaleBefore=presentations.ToDictionary(t=>t,t=>t.localScale);
        var mainBefore=rig.sides.ToDictionary(d=>d,d=>rig.owner.ResolvePresentedWheel(d.Find("s282_wheels_driving_3")).position);
        var renderedRods=(Three(id)?new[]{rig.owner.lfThreeAxleSideRod,rig.owner.lrThreeAxleSideRod,rig.owner.rfThreeAxleSideRod,rig.owner.rrThreeAxleSideRod}:new[]{rig.owner.leftTwoAxleSideRod,rig.owner.rightTwoAxleSideRod});
        var corrected=renderedRods.Select(r=>r.GetComponent<MeshFilter>().sharedMesh).Distinct().ToArray();
        Check(corrected.Length==(Three(id)?2:1),"Per-segment seating cache has wrong immutable mesh inventory");
        var correctedByRod=renderedRods.ToDictionary(r=>r,r=>r.GetComponent<MeshFilter>().sharedMesh);
        foreach(var mesh in corrected)
        {
            var source=Three(id)?rig.rods["threeHalf"]:rig.rods["two"];
            Check(!ReferenceEquals(mesh,source),"Remesher mutated shared rod source");Check(mesh.triangles.SequenceEqual(source.triangles)&&mesh.uv.SequenceEqual(source.uv)&&mesh.colors.SequenceEqual(source.colors),"Mesh channel/topology integrity lost");
            Near(mesh.bounds.min.z,mesh.vertices.Min(v=>v.z),"Corrected lower mesh bound");Near(mesh.bounds.max.z,mesh.vertices.Max(v=>v.z),"Corrected upper mesh bound");
        }
        foreach(var clip in anim.GetProperty("clips").EnumerateArray())foreach(var frame in clip.GetProperty("frames").EnumerateArray().Where((f,i)=>i%2==0))
        {
            rig.Pose(clip,frame);poses++;
            foreach(var visual in presentations)
            {
                Near(visual.localPosition,positionBefore[visual],"Animator constant position reset displaced wheel presentation");Near(visual.localScale,scaleBefore[visual],"Animator constant scale reset resized presentation");
                var source=rig.wheels.SelectMany(w=>w).Single(w=>ReferenceEquals(rig.owner.ResolvePresentedWheel(w),visual));
                Near(visual.localRotation,source.localRotation,"Native completed quaternion not copied");
            }
            foreach(var drive in rig.sides)Near(rig.owner.ResolvePresentedWheel(drive.Find("s282_wheels_driving_3")).position,mainBefore[drive],"Main driver changed centre during native animation");
            // Use independently measured physical bore centres, rather than
            // merely comparing production pin descriptors with themselves.
            // The source/native main crank sets phase; wheel-to-wheel deltas
            // locate every target crank while preserving the fixed main axle.
            foreach(var rod in renderedRods)
            {
                var src=Three(id)?rig.rods["threeHalf"]:rig.rods["two"];var copy=rod.GetComponent<MeshFilter>().sharedMesh;
                var before=Three(id)?new[]{-3.105580162f,-1.55294514f}:new[]{-.169580162f,1.69832514f};
                var boreY=Three(id)?new[]{-.00002527875f,-.000025020586f}:new[]{-.00002527875f,.000025619412f};
                var drive=rod.parent.parent;
                var main=drive.Find("s282_mech_wheels_connect/s282_mech_push_rod_to_connect");
                var mainSource=drive.Find("s282_wheels_driving_3");
                bool front=ReferenceEquals(rod,rig.owner.lfThreeAxleSideRod)||ReferenceEquals(rod,rig.owner.rfThreeAxleSideRod);
                int[] assigned=Two(id)?new[]{3,2}:front?new[]{2,3}:new[]{4,3};
                for(int p=0;p<before.Length;p++)
                {
                    int representative=Enumerable.Range(0,src.vertices.Length).First(i=>Math.Abs(src.vertices[i].z-before[p])<.10f);
                    float rigidShift=copy.vertices[representative].z-src.vertices[representative].z;
                    var sourceBore=new Vector3(0,boreY[p],before[p]);
                    var actual=rod.TransformPoint(sourceBore+Vector3.forward*rigidShift);
                    var visual=rig.owner.ResolvePresentedWheel(drive.Find("s282_wheels_driving_"+assigned[p]));
                    var expected=visual.position+(main.position-mainSource.position);
                    var a=rig.owner.transform.InverseTransformPoint(actual);var b=rig.owner.transform.InverseTransformPoint(expected);
                    float errorZ=Math.Abs(a.z-b.z),errorYZ=MathF.Sqrt(MathF.Pow(a.y-b.y,2)+MathF.Pow(a.z-b.z,2));
                    maximumSeatedPinErrorZ=Math.Max(maximumSeatedPinErrorZ,errorZ);maximumSeatedPinErrorYZ=Math.Max(maximumSeatedPinErrorYZ,errorYZ);
                    string label=id+" "+(Two(id)?"single":front?"front":"rear");
                    seatedByLayout[label]=Math.Max(seatedByLayout.GetValueOrDefault(label),errorYZ);
                    Check(errorZ<.001f,$"Actual coupling bearing is over1mm from its assigned crank inZ ({label} pin{p} error{errorZ:R} y{a.y-b.y:R} z{a.z-b.z:R})");
                    var legacyActual=rig.owner.transform.InverseTransformPoint(rod.TransformPoint(sourceBore));
                    var legacyExpected=rig.owner.transform.InverseTransformPoint(expected-rig.owner.transform.TransformDirection(Vector3.forward)*selected.DriverOffsetM(assigned[p]));
                    float legacyError=MathF.Sqrt(MathF.Pow(legacyActual.y-legacyExpected.y,2)+MathF.Pow(legacyActual.z-legacyExpected.z,2));
                    maximumLegacyPinErrorYZ=Math.Max(maximumLegacyPinErrorYZ,legacyError);legacyByLayout[label]=Math.Max(legacyByLayout.GetValueOrDefault(label),legacyError);
                    float addedY=Math.Abs((a.y-b.y)-(legacyActual.y-legacyExpected.y));maximumAddedVerticalPinError=Math.Max(maximumAddedVerticalPinError,addedY);
                    var rocking=rod.parent.localRotation;
                    float nativeSin=(float)(2*Math.Sqrt((double)rocking.x*rocking.x+rocking.y*rocking.y+rocking.z*rocking.z)*Math.Abs(rocking.w));
                    float derivedBound=Math.Abs(rigidShift)*drive.TransformVector(Vector3.forward).magnitude*nativeSin;
                    maximumDerivedVerticalBound=Math.Max(maximumDerivedVerticalBound,derivedBound);
                    Check(addedY<derivedBound+.000025f,$"Added transverse drift exceeds native-angle times corrected span ({label} pin{p} addedY{addedY:R} bound{derivedBound:R})");
                    foreach(int vi in Enumerable.Range(0,src.vertices.Length).Where(i=>Math.Abs(src.vertices[i].z-before[p])<.13f).Take(4))
                    {
                        var vertexExpected=rod.TransformPoint(src.vertices[vi]+Vector3.forward*rigidShift);var vertexActual=rod.TransformPoint(copy.vertices[vi]);
                        float rigidError=(vertexActual-vertexExpected).magnitude;maximumPinAddedError=Math.Max(maximumPinAddedError,rigidError);
                        Check(rigidError<.0001f,"Added boss shape distortion exceeded0.1mm");
                    }
                }
            }
        }
        foreach(var pair in originalMeshes)
        {
            Check(pair.Key.vertices.SequenceEqual(pair.Value.positions),"Original native mesh positions were mutated");
            Check(pair.Key.triangles.SequenceEqual(pair.Value.triangles)&&pair.Key.uv.SequenceEqual(pair.Value.uv),"Original native mesh channel mutated");
        }
        rig.owner.SeedCurveCache();rig.owner.SuspendPitch();Check(rig.owner.CurveCacheCleared,"Curve cache not cleared on suspension");
        Check(rig.owner.DriverLayoutPresentationCount==0&&rig.owner.ActiveDriverLayout==null,"Suspension retains active descriptor");
        Check(presentations.All(t=>!t.gameObject.activeSelf),"Suspension leaves pitched driver visible");
        foreach(var f in originalFilter)Check(ReferenceEquals(f.Key.sharedMesh,f.Value),"Source wheel/rod mesh not restored");
        foreach(var f in rig.fittings){Near(f.t.localPosition,fittingBase[f.t].local,"Fitting pose not restored");Check(f.t.gameObject.activeSelf==fittingBase[f.t].active,"Fitting active state not restored");}
        foreach(var state in lodStates)
        {
            var current=state.group.GetLODs();Check(current.Length==state.lods.Length,"LOD count not restored");
            for(int i=0;i<current.Length;i++){Check(current[i].renderers.SequenceEqual(state.lods[i].renderers),"LOD renderer array/order not restored");Near(current[i].screenRelativeTransitionHeight,state.lods[i].screenRelativeTransitionHeight,"LOD transition not restored");Near(current[i].fadeTransitionWidth,state.lods[i].fadeTransitionWidth,"LOD fade not restored");}
            Near(state.group.localReferencePoint,state.point,"LOD reference point not restored");Near(state.group.size,state.size,"LOD culling size not restored");
        }
        for(int repeat=0;repeat<3;repeat++)
        {
            rig.Legacy(id);rig.owner.ApplyPitch(id);Check(rig.wheels.SelectMany(w=>w).Select(rig.owner.ResolvePresentedWheel).Where(t=>t.name.StartsWith("[Rearranged pitched wheel")).SequenceEqual(presentations),"Repeat apply allocates duplicate driver presentations");
            Check(renderedRods.All(r=>ReferenceEquals(r.GetComponent<MeshFilter>().sharedMesh,correctedByRod[r])),"Repeat unchanged layout does not reuse immutable corrected rod");
            rig.owner.SuspendPitch();lifecycleCycles++;
        }
        rig.Legacy(id);rig.owner.ApplyPitch(id);
        ModMain.settings.prairieExtraCoupledPitchMetres=ModMain.settings.americanExtraCoupledPitchMetres=ModMain.settings.mogulExtraCoupledPitchMetres=ModMain.settings.twoFortyExtraCoupledPitchMetres=pitch==.20f?.10f:.20f;
        rig.owner.SuspendPitch();rig.Legacy(id);rig.owner.ApplyPitch(id);Check(corrected.All(m=>m.destroyed),"Changing pitch did not dispose superseded corrected mesh");
        var ownedMeshes=renderedRods.Select(r=>r.GetComponent<MeshFilter>().sharedMesh).Distinct().ToArray();rig.owner.SuspendPitch();
        rig.owner.DisposePitch();Check(presentations.All(t=>t.gameObject.destroyed),"Disposal leaked presentation GameObjects");Check(ownedMeshes.All(m=>m.destroyed),"Disposal leaked corrected mesh");
        foreach(var mesh in rig.nativeMeshes)Check(!mesh.destroyed,"Disposal destroyed native mesh asset");
    }
    private static void PhaseReapplyCases(JsonElement rodData,JsonElement anim,JsonElement wheelData)
    {
        foreach(int id in new[]{34,35,44,0})foreach(float pitch in new[]{.15f,.20f})
        {
            var rig=new Rig(rodData,anim,wheelData,false);rig.Legacy(id);
            ModMain.settings.prairieExtraCoupledPitchMetres=ModMain.settings.americanExtraCoupledPitchMetres=ModMain.settings.mogulExtraCoupledPitchMetres=ModMain.settings.twoFortyExtraCoupledPitchMetres=pitch;
            rig.owner.ApplyPitch(id);
            var rods=Three(id)?new[]{rig.owner.lfThreeAxleSideRod,rig.owner.lrThreeAxleSideRod,rig.owner.rfThreeAxleSideRod,rig.owner.rrThreeAxleSideRod}:new[]{rig.owner.leftTwoAxleSideRod,rig.owner.rightTwoAxleSideRod};
            var copies=rods.ToDictionary(r=>r,r=>r.GetComponent<MeshFilter>().sharedMesh);
            var vertices=copies.Values.Distinct().ToDictionary(m=>m,m=>m.vertices.ToArray());
            foreach(var clip in anim.GetProperty("clips").EnumerateArray())foreach(var frame in clip.GetProperty("frames").EnumerateArray().Where((f,i)=>i%2==0))
            {
                rig.Pose(clip,frame);
                var phase=rig.sides.Select(d=>d.Find("s282_mech_wheels_connect")).ToDictionary(t=>t,t=>(p:t.localPosition,q:t.localRotation));
                rig.owner.SuspendPitch();rig.Legacy(id);
                // Retain the native rod's evaluated phase while applying the
                // requested legacy driver centres, exposing phase-dependent
                // cache keys without letting constant wheel positions confound it.
                foreach(var value in phase){value.Key.localPosition=value.Value.p;value.Key.localRotation=value.Value.q;}
                rig.owner.ApplyPitch(id);phaseReapplyCases++;
                foreach(var rod in rods)Check(ReferenceEquals(rod.GetComponent<MeshFilter>().sharedMesh,copies[rod]),"Native phase changed immutable rod cache/shape");
                foreach(var value in vertices)Check(value.Key.vertices.SequenceEqual(value.Value),"Reapplying another phase mutated cached rod vertices");
            }
            rig.owner.DisposePitch();
        }
    }
    private static void ScopeAndRollbackCases(JsonElement rodData,JsonElement anim,JsonElement wheelData)
    {
        var rig=new Rig(rodData,anim,wheelData,false);var isolated=new Rig(rodData,anim,wheelData,true);
        var isolation=isolated.owner.GetComponentsInChildren<MeshFilter>(true).ToDictionary(f=>f,f=>f.sharedMesh);
        foreach(int id in new[]{34,35,44,0,34,35,44,0})
        {
            rig.Legacy(id);ModMain.settings.prairieExtraCoupledPitchMetres=ModMain.settings.americanExtraCoupledPitchMetres=ModMain.settings.mogulExtraCoupledPitchMetres=ModMain.settings.twoFortyExtraCoupledPitchMetres=.15f;
            rig.owner.ApplyPitch(id);switchCases++;
            Check(rig.owner.CachedPitchMeshCount<=6,"Arrangement switching leaves more than six cached rod variants");
            var meshes=rig.owner.GetComponentsInChildren<MeshFilter>(true).ToDictionary(f=>f,f=>f.sharedMesh);
            int count=rig.owner.DriverLayoutPresentationCount;
            rig.owner.ApplyPitch(4); // Pacific remains outside this tuning trial.
            Check(rig.owner.DriverLayoutPresentationCount==count,"Unrelated arrangement changed active layout");
            foreach(var entry in meshes)Check(ReferenceEquals(entry.Key.sharedMesh,entry.Value),"Unrelated apply changed geometry");
            foreach(var entry in isolation)Check(ReferenceEquals(entry.Key.sharedMesh,entry.Value),"Another car's geometry changed");
            Check(isolated.owner.ActiveDriverLayout==null&&isolated.owner.DriverLayoutPresentationCount==0,"Another car gained a layout");
            rig.owner.SuspendPitch();
            Check(rig.owner.ResolvePresentedWheel(rig.owner.secondRightDriveWheel)==rig.owner.secondRightDriveWheel,"Suspended resolver retains stale presentation");
        }
        Check(rig.owner.CachedPitchMeshCount==6,"Four layouts did not retain exactly their six reusable rod variants");
        rig.owner.DisposePitch();isolated.owner.DisposePitch();

        var failed=new Rig(rodData,anim,wheelData,false);failed.Legacy(34);
        var sources=failed.owner.GetComponentsInChildren<MeshFilter>(true).ToDictionary(f=>f,f=>f.sharedMesh);
        // Valid native geometry but no unique LOD owner: failure occurs after
        // wheels and corrected rods have been prepared, exercising real rollback.
        foreach(var group in failed.groups)
            group.SetLODs(group.GetLODs().Select(l=>new LOD(l.screenRelativeTransitionHeight,l.renderers.Where(r=>
                r!=failed.owner.secondLeftDriveWheel.GetComponent<MeshRenderer>()&&r!=failed.owner.secondRightDriveWheel.GetComponent<MeshRenderer>()).ToArray()){fadeTransitionWidth=l.fadeTransitionWidth}).ToArray());
        bool rejected=false;try{failed.owner.ApplyPitch(34);}catch(InvalidOperationException){rejected=true;}rollbackCases++;
        Check(rejected,"Ambiguous/missing native LOD ownership was silently accepted");
        Check(failed.owner.ActiveDriverLayout==null&&failed.owner.DriverLayoutPresentationCount==0,"Failed apply leaves active layout");
        foreach(var entry in sources)Check(ReferenceEquals(entry.Key.sharedMesh,entry.Value),"Failure rollback lost source mesh");
        Check(failed.owner.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("[Rearranged pitched wheel")).All(t=>!t.gameObject.activeSelf),"Failed apply leaves visible driver proxy");
        failed.owner.DisposePitch();

        var missing=new Rig(rodData,anim,wheelData,false);
        missing.owner.secondRightDriveWheel.GetChild(0).GetComponent<MeshFilter>().sharedMesh=null;
        missing.owner.CachePitch();missing.Legacy(35);
        rejected=false;try{missing.owner.ApplyPitch(35);}catch(InvalidOperationException){rejected=true;}rollbackCases++;
        Check(rejected,"Unsupported native driver topology was silently accepted");
        missing.owner.ApplyPitch(4);Check(missing.owner.ActiveDriverLayout==null,"Unsupported optional layout blocks unrelated arrangement");
        missing.owner.DisposePitch();

        var absentPrefab=new Rig(rodData,anim,wheelData,false);absentPrefab.owner.loco.carLivery.prefab=null;
        absentPrefab.owner.CachePitch();absentPrefab.Legacy(35);
        rejected=false;try{absentPrefab.owner.ApplyPitch(35);}catch(InvalidOperationException){rejected=true;}rollbackCases++;
        Check(rejected,"Missing canonical native prefab fell back to an animated live sample");
        absentPrefab.owner.ApplyPitch(4);Check(absentPrefab.owner.ActiveDriverLayout==null,"Missing canonical prefab blocks unrelated arrangement");
        absentPrefab.owner.DisposePitch();
    }
    private static void PhasedInitialCacheCases(JsonElement rodData,JsonElement anim,JsonElement wheelData)
    {
        foreach(int id in new[]{34,35,44,0})foreach(float pitch in new[]{.15f,.20f})foreach(bool rotated in new[]{false,true})
        {
            var rig=new Rig(rodData,anim,wheelData,rotated,false,true);
            var canonical=rig.owner.loco.carLivery.prefab.GetComponentsInChildren<Transform>(true).ToDictionary(t=>t,t=>t.localPosition);
            var left=rig.owner.driveL.Find("s282_mech_wheels_connect/s282_mech_push_rod_to_connect");
            var right=rig.owner.driveR.Find("s282_mech_wheels_connect/s282_mech_push_rod_to_connect");
            Check(Math.Abs(left.localPosition.z-right.localPosition.z)>.00025f,"Initial cache phases do not expose actual native main-pin drift");
            ModMain.settings.prairieExtraCoupledPitchMetres=ModMain.settings.americanExtraCoupledPitchMetres=ModMain.settings.mogulExtraCoupledPitchMetres=ModMain.settings.twoFortyExtraCoupledPitchMetres=pitch;
            rig.Legacy(id);
            // Keep different native rotation phases on the two coupling rods;
            // all must remain compatible with the shared geometry cache.
            rig.owner.leftSideRod.localRotation=Quaternion.AngleAxis(.021f,Vector3.right);
            rig.owner.rightSideRod.localRotation=Quaternion.AngleAxis(-.027f,Vector3.right);
            rig.owner.ApplyPitch(id);phasedInitialCacheCases++;
            Check(rig.owner.DriverLayoutPresentationCount==(Three(id)?6:4),"Different L/R cache phases prevented the selected layout");
            Check(rig.owner.CachedPitchMeshCount==(Three(id)?2:1),"Different native phases created extra shared-rod cache variants");
            var rods=Three(id)?new[]{rig.owner.lfThreeAxleSideRod,rig.owner.lrThreeAxleSideRod,rig.owner.rfThreeAxleSideRod,rig.owner.rrThreeAxleSideRod}:new[]{rig.owner.leftTwoAxleSideRod,rig.owner.rightTwoAxleSideRod};
            var copies=rods.ToDictionary(r=>r,r=>r.GetComponent<MeshFilter>().sharedMesh);
            var before=copies.Values.Distinct().ToDictionary(m=>m,m=>m.vertices.ToArray());
            foreach(var clip in anim.GetProperty("clips").EnumerateArray())foreach(var frame in clip.GetProperty("frames").EnumerateArray().Where((f,i)=>i%3==0))
            {
                rig.Pose(clip,frame);var rotations=rig.sides.Select(d=>d.Find("s282_mech_wheels_connect")).ToDictionary(t=>t,t=>t.localRotation);
                rig.owner.SuspendPitch();rig.Legacy(id);
                foreach(var rotation in rotations)rotation.Key.localRotation=rotation.Value;
                rig.owner.ApplyPitch(id);phasedInitialCacheCases++;
                foreach(var rod in rods)Check(ReferenceEquals(rod.GetComponent<MeshFilter>().sharedMesh,copies[rod]),"Native initial/evaluated phases changed shared mesh cache");
                foreach(var value in before)Check(value.Key.vertices.SequenceEqual(value.Value),"Native phases modified cached geometry");
            }
            foreach(var datum in canonical)Near(datum.Key.localPosition,datum.Value,"Canonical prefab transform was modified");
            rig.owner.DisposePitch();
        }
    }
    private static void Main(string[] args)
    {
        if(args.Length<4||args.Length>5)throw new ArgumentException("Expected native rods, native animation, native wheel data, report path and optional cache-phase-only");
        using var r=JsonDocument.Parse(File.ReadAllText(args[0]));using var a=JsonDocument.Parse(File.ReadAllText(args[1]));using var w=JsonDocument.Parse(File.ReadAllText(args[2]));
        DescriptorCases();
        PhasedInitialCacheCases(r.RootElement,a.RootElement,w.RootElement);
        if(args.Length==4)
        {
            foreach(int id in new[]{34,35,44,0})foreach(bool rotated in new[]{false,true})foreach(bool sharedLod in new[]{false,true})foreach(float pitch in id==44?new[]{0f,.15f,.20f,.22f}:new[]{0f,.15f,.20f})
                LayoutCase(r.RootElement,a.RootElement,w.RootElement,id,rotated,sharedLod,pitch);
            PhaseReapplyCases(r.RootElement,a.RootElement,w.RootElement);
            ScopeAndRollbackCases(r.RootElement,a.RootElement,w.RootElement);
        }
        var report=new{passed=true,targetedCacheOnly=args.Length==5,checks,layoutCases,nativePoseScenarios=poses,lifecycleCycles,phaseReapplyCases,switchCases,rollbackCases,phasedInitialCacheCases,maximumAddedBossShapeErrorM=maximumPinAddedError,
            maximumSeatedPinErrorZM=maximumSeatedPinErrorZ,maximumSeatedPinErrorYZM=maximumSeatedPinErrorYZ,maximumLegacyPinErrorYZM=maximumLegacyPinErrorYZ,seatedByLayout,legacyByLayout,
            maximumAddedVerticalPinErrorM=maximumAddedVerticalPinError,maximumDerivedNativeRockingBoundM=maximumDerivedVerticalBound,
            scope=args.Length==5?"Targeted live-cache-phase regression: actual native L/R main-pin min/max phases at initial cache, separate unanimated prefab datum, differing L/R coupling-rod rotations, both layouts/orientations,0.15/0.20m pitch, repeated later native phases, immutable cache reuse. Other integration checks intentionally skipped in this targeted mode.":"Actual production descriptor, owner, pure remesher, wheel quaternion copier and Additional physical axle resolver executed with managed transform/mesh/LOD stand-ins; native wheel meshes, native animation clips and exact generated rod recipes. Four layouts, both car orientations, shared/separate LOD ownership,0/.15/.20m pitches plus Prairie .22m, constants pose reset, main anchors, fittings/sparks/brakes, source/channel immutability, bounds/LOD restoration, cache replacement/disposal; independent canonical prefab and L/R live phase mismatch at initial cache.",
            limitations="Stand-ins do not reproduce Unity's renderer culling or Animator evaluation scheduling, live mesh upload, physical wheel registration, game collision clearance, oil interaction or the integrated stock/prepared GeometryState caller. Native geometry remains ignored local and is never packaged."};
        File.WriteAllText(args[3],JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(report));
    }
}
