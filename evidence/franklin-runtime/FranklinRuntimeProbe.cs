using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using DV.Simulation.Ports;
using DV.Wheels;
using LocoSim.Definitions;
using LocoSim.Implementations;
using Newtonsoft.Json.Linq;
using RearrangedS282;
using UnityEngine;
using Object=UnityEngine.Object;

// Private regression fixture. It never modifies a game installation or save.
internal static class FixtureAssets
{
    internal static Mesh ShortMainRod,TwoAxleSideRod,CasingReadable,ThreeAxleSideRod,FiveSixAxleSideRod;
    internal static RuntimeAnimatorController Controller,NativeController;
    internal static float WalsMalletGap=float.NaN;
    internal static Mesh LoadObj(string path)=>FranklinRuntimeProbe.Obj(path);
}

public static class FranklinRuntimeProbe
{
    const BindingFlags Static=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
    const string State="LocoS282A_AnimationDriveGear";
    static Assembly production;
    static JObject data,config,walsData;
    static AssetBundle bundle;
    static readonly Dictionary<string,Mesh> nativeMeshes=new Dictionary<string,Mesh>();
    static readonly Dictionary<string,Mesh> bundleMeshes=new Dictionary<string,Mesh>();
    static readonly Dictionary<Mesh,Mesh> readableGeometry=new Dictionary<Mesh,Mesh>();
    static readonly Dictionary<Mesh,TriangleTree> trees=new Dictionary<Mesh,TriangleTree>();
    static readonly List<string> checks=new List<string>();
    static readonly List<GameObject> roots=new List<GameObject>();
    static readonly JArray results=new JArray();
    static string outputDirectory;
    public static GameObject DrivetrainSource;

    static void Require(bool value,string label)
    { if(!value)throw new InvalidOperationException(label); checks.Add(label); }
    static Vector3 V(JToken value)=>new Vector3((float)value[0],(float)value[1],(float)value[2]);
    static JArray JV(Vector3 value)=>new JArray(value.x,value.y,value.z);
    static Mesh MeshFrom(JObject bank,string key,Dictionary<string,Mesh> cache)
    {
        if(cache.TryGetValue(key,out var mesh))return mesh;
        var item=bank[key];
        mesh=new Mesh {name=(string)item["name"],indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};
        mesh.vertices=item["vertices"].Select(V).ToArray();
        mesh.triangles=item["triangles"].Select(t=>(int)t).ToArray();
        mesh.RecalculateBounds();mesh.RecalculateNormals();cache.Add(key,mesh);return mesh;
    }
    static Transform Reconstruct(JToken item,Transform parent)
    {
        var go=new GameObject((string)item["name"]);go.SetActive(false);
        var t=go.transform;t.SetParent(parent,false);
        t.localPosition=V(item["position"]);t.localScale=V(item["scale"]);
        var q=item["rotation"];t.localRotation=new Quaternion((float)q[0],(float)q[1],(float)q[2],(float)q[3]);
        if(item["mesh"]!=null)go.AddComponent<MeshFilter>().sharedMesh=MeshFrom((JObject)data["nativeMeshes"],(string)item["mesh"],nativeMeshes);
        if(item["renderer"]!=null)go.AddComponent<MeshRenderer>().enabled=(bool)item["renderer"];
        foreach(var child in item["children"])Reconstruct(child,t);
        go.SetActive((bool)item["active"]);return t;
    }
    static void ReplaceNativeMeshes(Transform drive,JToken item)
    {
        if(item["mesh"]!=null)drive.GetComponent<MeshFilter>().sharedMesh=MeshFrom((JObject)data["nativeMeshes"],(string)item["mesh"],nativeMeshes);
        foreach(var child in item["children"])
        {
            var target=drive.Find((string)child["name"]);
            if(target==null)throw new InvalidOperationException("True native drive differs from serialized hierarchy: "+child["name"]);
            ReplaceNativeMeshes(target,child);
        }
    }
    static Transform NativeDrive(string side,Transform parent,bool franklin=true)
    {
        var item=data["drives"]["LocoS282A_Drivetrain "+side];
        Transform drive;
        if(DrivetrainSource!=null)
        {
            drive=Object.Instantiate(DrivetrainSource,parent).transform;
            foreach(var b in drive.GetComponentsInChildren<MonoBehaviour>(true)){b.enabled=false;Object.Destroy(b);}
            ReplaceNativeMeshes(drive,item);
        }
        else drive=Reconstruct(item,parent);
        drive.gameObject.SetActive(false);drive.name="native drivetrain "+side;
        drive.localPosition=Vector3.zero;drive.localRotation=Quaternion.identity;drive.localScale=Vector3.one;
        for(int i=5;i<=6;i++)
        {
            var wheel=Object.Instantiate(drive.Find("s282_wheels_driving_4").gameObject,drive).transform;
            wheel.name="s282_wheels_driving_"+i;wheel.gameObject.SetActive(false);
            wheel.localPosition=new Vector3(0,.71f,i==5?1.65f:.1f);
            if(!franklin)
            {
                var type=production.GetType("RearrangedS282.PoweredWheelRotater",true);
                var rotater=wheel.gameObject.AddComponent(type);
                type.GetMethod("Init",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke(rotater,new object[]{drive.Find("s282_wheels_driving_4").gameObject});
            }
        }
        var animator=drive.GetComponent<Animator>()??drive.gameObject.AddComponent<Animator>();
        animator.enabled=true;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        animator.runtimeAnimatorController=franklin?FixtureAssets.Controller:FixtureAssets.NativeController;
        return drive;
    }
    static Transform Asset(string assetName,Transform parent)
    {
        var original=bundle.LoadAsset<GameObject>(assetName);
        if(original==null)throw new InvalidOperationException("Original bundle prefab missing: "+assetName);
        var t=Object.Instantiate(original,parent).transform;
        foreach(var f in t.GetComponentsInChildren<MeshFilter>(true))
        {
            string path=assetName+"/"+Relative(t,f.transform);
            var key=data["bundleBindings"][path];
            if(key==null)throw new InvalidOperationException("Exact bundle mesh binding missing: "+path);
            // Keep the exact bundle mesh and its CPU-readability state for the
            // production retarget code. The separate triangle bank is used by
            // this probe only; making every source readable changes its branch.
            readableGeometry[f.sharedMesh]=MeshFrom((JObject)data["bundleMeshes"],(string)key,bundleMeshes);
        }
        foreach(var r in t.GetComponentsInChildren<MeshRenderer>(true))r.enabled=true;
        return t;
    }
    static string Relative(Transform root,Transform t)
    {
        var parts=new List<string>();for(var p=t;p!=root&&p!=null;p=p.parent)parts.Add(p.name);
        parts.Reverse();return string.Join("/",parts);
    }
    internal static Mesh Obj(string path)
    {
        var type=production.GetType("Dummiesman.OBJLoader",true);
        var loader=Activator.CreateInstance(type);
        // Private headless player excludes the game's shaders. Supply null
        // material entries while retaining the production geometry parser.
        var materials=new Dictionary<string,Material>{{"default",null}};
        foreach(string line in File.ReadLines(path))if(line.StartsWith("usemtl ",StringComparison.Ordinal))materials[line.Substring(7).Trim()]=null;
        type.GetField("Materials",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(loader,materials);
        using(var stream=File.OpenRead(path))return (Mesh)type.GetMethod("Load",new[]{typeof(Stream)}).Invoke(loader,new object[]{stream});
    }
    static void PrepareRods(string assets)
    {
        FixtureAssets.ShortMainRod=Obj(Path.Combine(assets,"s282_mech_push_rod_to_connect.obj"));
        var vertices=FixtureAssets.ShortMainRod.vertices;
        Require(vertices.Any(v=>v.z>2),"Production OBJ loader reads original long main rod");
        for(int i=0;i<vertices.Length;i++)if(vertices[i].z>2)vertices[i].z-=1.52f;
        FixtureAssets.ShortMainRod.vertices=vertices;FixtureAssets.ShortMainRod.RecalculateBounds();
        FixtureAssets.ShortMainRod.name="Rearranged articulated short main rod";
        FixtureAssets.TwoAxleSideRod=Obj(Path.Combine(assets,"s282_mech_wheels_connect.obj"));
        vertices=FixtureAssets.TwoAxleSideRod.vertices;var triangles=FixtureAssets.TwoAxleSideRod.triangles;
        Require(vertices.Length==508&&triangles.Length==524*3,"Original coupling rod topology matches production index recipe");
        int[,] forward={{34,68},{269,270},{349,380},{413,445}};
        int[,] ends={{471,473},{475,476},{312,314},{316,317}};
        for(int r=0;r<forward.GetLength(0);r++)for(int i=forward[r,0];i<forward[r,1];i++)vertices[i].z+=3.256f-.32f;
        for(int i=152;i<211;i+=2)vertices[i].z+=3.256f-.32f;
        for(int r=0;r<ends.GetLength(0);r++)for(int i=ends[r,0];i<ends[r,1];i++)vertices[i].z+=.15f-.32f;
        foreach(int i in new[]{173,175,237,239})vertices[i].z+=.15f-.32f;
        int[,] hidden={{453,488},{384,421},{349,352},{230,313},{128,146},{64,96},{0,32}};
        var deleted=new HashSet<int>();
        for(int r=0;r<hidden.GetLength(0);r++)for(int i=hidden[r,0];i<hidden[r,1];i++)deleted.Add(i);
        for(int i=147;i<166;i+=2)deleted.Add(i);for(int i=202;i<229;i+=2)deleted.Add(i);
        FixtureAssets.TwoAxleSideRod.vertices=vertices;
        FixtureAssets.TwoAxleSideRod.triangles=triangles.Where((t,i)=>!deleted.Contains(i/3)).ToArray();
        FixtureAssets.TwoAxleSideRod.RecalculateBounds();
        FixtureAssets.CasingReadable=Obj(Path.Combine(assets,"franklin_casing.obj"));
        var rods=new ProductionRodRecipes(assets);
        FixtureAssets.TwoAxleSideRod=rods.TwoAxleSideRodMesh;
        FixtureAssets.ThreeAxleSideRod=rods.ThreeAxleSideRodMesh;
        FixtureAssets.FiveSixAxleSideRod=rods.FiveSixAxleSideRodMesh;
    }
    static void Retarget(Transform drive,float shift,Transform root)
    {
        var type=production.GetType("RearrangedS282.DriveRetarget",true);
        var bind=type.GetMethods(Static).Single(m=>m.Name=="Bind"&&m.GetParameters().Length==4);
        var classifier=Delegate.CreateDelegate(bind.GetParameters()[2].ParameterType,type.GetMethod("FranklinRole",Static));
        bind.Invoke(null,new object[]{drive,shift,classifier,root});
    }
    static MeshFilter Rendered(Transform source)
    {
        var own=source.GetComponent<MeshFilter>();if(own!=null&&own.sharedMesh!=null)return own;
        return source.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.sharedMesh!=null&&f.transform.parent==source&&f.name.StartsWith("[Rearranged retarget]",StringComparison.Ordinal));
    }
    static void ShapeCasing(Transform franklin,Transform drive,float scale,bool compact)
    {
        var source=franklin.Find("cylinder/default");var filter=Rendered(source);
        // CacheCasing uses the prefab's native frame; retarget translation is
        // already in the final filter transform and must not enter this map.
        var nativeFromMesh=drive.worldToLocalMatrix*source.localToWorldMatrix;
        var readable=filter.sharedMesh.isReadable?filter.sharedMesh:FixtureAssets.CasingReadable;
        var vertices=readable.vertices;
        var native=vertices.Select(nativeFromMesh.MultiplyPoint3x4).ToArray();
        float min=native.Min(v=>v.y),rear=native.Min(v=>v.z)+.16f;
        float target=CylinderCasingShape.ComputeTargetY(scale,min,.71f,0);
        if(Mathf.Abs(target-scale)<.001f&&!compact)return;
        for(int i=0;i<vertices.Length;i++)
        {
            var point=CylinderCasingShape.TransformVertex(native[i],scale,scale,target,rear,rear+.20f,.71f,true);
            if(compact)point=CylinderCasingShape.CompactFranklinRearNose(point);
            vertices[i]=nativeFromMesh.inverse.MultiplyPoint3x4(point);
        }
        var mesh=Object.Instantiate(readable);mesh.vertices=vertices;mesh.RecalculateBounds();filter.sharedMesh=mesh;
    }
    sealed class Engine
    {
        internal Transform Left,Right,Frame,Origin;
        internal Animator[] Animators;
        internal Port Phase;
        internal IDisposable Binding;
        internal Transform LeftCasing,RightCasing;
        internal AnimatorPortReader[] CutoffReaders;
        internal Port Cutoff;
    }
    sealed class Layout
    {
        internal GameObject Root;
        internal List<Engine> Engines=new List<Engine>();
        internal List<Pair> Pairs=new List<Pair>();
        internal float Scale,FrontPin,MiddlePin;
        internal bool Wals;
        internal float[] Measured;
    }
    sealed class Pair
    {
        internal string Label;
        internal MeshFilter A,B;
        internal int Intersections,AabbSamples;
        internal JObject First;
        internal Matrix4x4[] PhaseA,PhaseB;
        internal Bounds[] BoundsA,BoundsB;
        internal long IndependentPairs,IndependentBoundsPairs,IndependentContacts;
        internal readonly Dictionary<string,int> ContactsByCondition=new Dictionary<string,int>();
        internal float MinimumFollowingZGap=float.PositiveInfinity;
        internal float MinimumSurfaceDistanceLowerM=float.PositiveInfinity;
    }
    static Matrix4x4 PartMatrix(JToken part)
    {var m=new Matrix4x4();for(int i=0;i<16;i++)m[i]=(float)part["nativeBodyFromMesh"][i];return m;}
    static Transform WalsPart(string name,Transform parent,float offset,float scale,bool casing)
    {
        var item=walsData["parts"].Single(p=>(string)p["name"]==name);
        var sourceMap=PartMatrix(item);
        var mesh=new Mesh {name=name};var vertices=item["vertices"].Select(V).ToArray();
        if(casing&&((bool?)config["shapeWalsCasings"]??true))
        {
            var native=vertices.Select(sourceMap.MultiplyPoint3x4).ToArray();
            float min=Mathf.Min(.2739646f,native.Min(v=>v.y)),rear=native.Min(v=>v.z)+.16f;
            float target=CylinderCasingShape.ComputeTargetY(scale,min,CylinderCasingShape.NativePistonY,0);
            if(Mathf.Abs(target-scale)>=.001f)
                for(int i=0;i<vertices.Length;i++)vertices[i]=sourceMap.inverse.MultiplyPoint3x4(CylinderCasingShape.TransformVertex(native[i],scale,scale,target,rear,rear+.20f,CylinderCasingShape.NativePistonY,false));
        }
        mesh.vertices=vertices;mesh.triangles=item["triangles"].Select(x=>(int)x).ToArray();mesh.RecalculateBounds();
        var t=new GameObject(name).transform;t.SetParent(parent,false);
        var map=Matrix4x4.TRS(new Vector3(0,0,offset),Quaternion.identity,new Vector3(1,scale,scale))*sourceMap;
        // Current native LMS source map is exact diagonal -X,+Z4.88. Fail if
        // a future source changes this instead of silently dropping shear.
        for(int r=0;r<3;r++)for(int c=0;c<3;c++)if(r!=c&&Mathf.Abs(map[r,c])>.00001f)throw new InvalidOperationException("Wals static source requires affine decomposition: "+name);
        t.localPosition=new Vector3(map.m03,map.m13,map.m23);t.localScale=new Vector3(map.m00,map.m11,map.m22);
        t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;t.gameObject.AddComponent<MeshRenderer>();return t;
    }
    static void PrepareNativeRods(Transform drive)
    {
        var rod=drive.Find("s282_mech_wheels_connect");
        for(int i=0;i<2;i++)
        {
            var half=new GameObject("[Rearranged three-axle rod "+i+"]").transform;half.SetParent(rod,false);
            half.localPosition=new Vector3(0,0,i==0?-3.11f:0);half.localEulerAngles=new Vector3(0,180,0);
            if(i==1)half.localScale=new Vector3(1,1,-1);
            half.gameObject.AddComponent<MeshFilter>().sharedMesh=FixtureAssets.ThreeAxleSideRod;half.gameObject.AddComponent<MeshRenderer>();
        }
        var segment=new GameObject("[Rearranged five-axle segment]").transform;segment.SetParent(rod,false);
        segment.localPosition=new Vector3(.003f,.004f,-3.1f);segment.localEulerAngles=new Vector3(0,180,0);
        segment.gameObject.AddComponent<MeshFilter>().sharedMesh=FixtureAssets.FiveSixAxleSideRod;segment.gameObject.AddComponent<MeshRenderer>();
    }
    static void NativeReset(Transform drive)
    {
        drive.Find("s282_wheels_driving_1").localPosition=new Vector3(0,.71f,8);
        drive.Find("s282_wheels_driving_2").localPosition=new Vector3(0,.71f,6.3f);
        drive.Find("s282_wheels_driving_4").localPosition=new Vector3(0,.71f,3.2f);
        drive.Find("s282_mech_wheels_connect/s282_mech_connect_to_cutoff_base").localPosition=new Vector3(.15f,0,-1.55f);
        drive.Find("s282_mech_push_rod").localScale=Vector3.one;
    }
    static Layout BuildWals(int id,bool owned)
    {
        Require(DrivetrainSource!=null&&FixtureAssets.NativeController!=null,"Wals probe uses complete original serialized native controller/hierarchy");
        Require((bool?)walsData["actualUnityLmsValidated"]==true&&(string)walsData["nativeResourceSHA256"]==(string)data["nativeSHA256"],"Wals static payload is actual installed-LMS Unity output for the same native game resource");
        var go=new GameObject("Walschaerts "+id+(owned?" phase":" reference"));roots.Add(go);go.SetActive(false);
        var layout=new Layout {Root=go,Wals=true};
        var l=NativeDrive("L",go.transform,false);var r=NativeDrive("R",go.transform,false);
        PrepareNativeRods(l);PrepareNativeRods(r);
        var datum=WalsPart("s282a_cylinder_l",go.transform,0,1,false);
        var configure=new WalsConfiguration(l,datum,go.transform);var measured=configure.Measure(id);
        layout.Measured=measured;
        layout.Scale=measured[0];layout.FrontPin=measured[3];
        int count=id==10?1:id==43?3:2;
        for(int e=0;e<count;e++)
        {
            bool tender=e==2;float offset=e==0?measured[1]:e==1?measured[2]:-.35f-layout.Scale*measured[6];
            var frame=new GameObject(tender?"tender engine frame":"engine pivot "+e).transform;frame.SetParent(go.transform,false);
            var origin=new GameObject("engine origin "+e).transform;origin.SetParent(frame,false);
            float pin=e==0?layout.FrontPin:0;frame.localPosition=new Vector3(0,0,pin);origin.localPosition=new Vector3(0,0,-pin);
            var left=e==0?l:NativeDrive("L",origin,false);var right=e==0?r:NativeDrive("R",origin,false);
            if(e==0){left.SetParent(origin,false);right.SetParent(origin,false);}else{PrepareNativeRods(left);PrepareNativeRods(right);}
            int coupled=e==0?(int)measured[4]:e==1?(int)measured[5]:4;
            foreach(var drive in new[]{left,right})
            {
                if(e==0)NativeReset(drive);
                drive.localPosition=new Vector3(0,0,offset);drive.localScale=new Vector3(drive==left?-1:1,layout.Scale,layout.Scale);
                configure.Configure(drive,coupled,e!=0);
                var animator=drive.GetComponent<Animator>();animator.speed=0;animator.SetFloat("SpeedMultiplier",0);drive.gameObject.SetActive(true);
            }
            var lc=WalsPart("s282a_cylinder_l",origin,offset,layout.Scale,true);var rc=WalsPart("s282a_cylinder_r",origin,offset,layout.Scale,true);
            var engine=new Engine {Left=left,Right=right,Frame=frame,Origin=origin,LeftCasing=lc,RightCasing=rc,Animators=new[]{left.GetComponent<Animator>(),right.GetComponent<Animator>()}};
            engine.Cutoff=new Port("wals cutoff "+e,new PortDefinition(PortType.READONLY_OUT,PortValueType.GENERIC,"CUTOFF"));
            engine.CutoffReaders=engine.Animators.Select(a=>{
                string side=a==engine.Animators[0]?"L":"R";
                var authored=data["drives"]["LocoS282A_Drivetrain "+side]["readers"].Single(x=>(string)x["parameterName"]=="Cutoff");
                Require((int)authored["updateType"]==1&&(float)authored["valueMultiplier"]==1&&(float)authored["valueOffset"]==0&&(string)authored["portId"]=="reverser.REVERSER","Native authored Cutoff reader modifiers match fixture "+side);
                var reader=a.gameObject.AddComponent<AnimatorPortReader>();reader.updateType=AnimatorPortReader.UpdateType.SET_PARAMETER;reader.parameterName="Cutoff";reader.valueMultiplier=1;reader.Init(engine.Cutoff);return reader;
            }).ToArray();
            layout.Engines.Add(engine);
        }
        Object.Destroy(datum.gameObject);go.SetActive(true);
        for(int e=0;e<layout.Engines.Count;e++)foreach(bool left in new[]{true,false})
        {
            var engine=layout.Engines[e];var drive=left?engine.Left:engine.Right;var casing=Rendered(left?engine.LeftCasing:engine.RightCasing);
            var crosshead=Rendered(drive.Find("s282_mech_push_rod"));
            string[] paths={"s282_mech_wheels_connect/s282_mech_connect_to_cutoff_base","s282_mech_wheels_connect/s282_mech_connect_to_cutoff_base/s282_mech_connect_to_cutoff_rod","s282_mech_wheels_connect/s282_mech_push_rod_to_connect","s282_mech_push_rod/s282_mech_push_rod_to_cutoff_base","s282_mech_cutoff_rod_hinge","s282_mech_cutoff_rod_hinge/s282_mech_push_rod_to_cutoff_rod","s282_mech_cylinder_rod","s282_mech_cutoff_boomerang_high","s282_mech_cutoff_boomerang_high/s282_mech_cutoff_rail_connection/s282_mech_cutoff_to_cylinder_rod"};
            foreach(string path in paths)
            {
                var source=drive.Find(path);if(source==null)throw new InvalidOperationException("Native Wals part missing: "+path);
                var moving=Rendered(source);string label=e+(left?" L ":" R ")+source.name;
                layout.Pairs.Add(new Pair{Label=label+" vs own casing",A=moving,B=casing});
                if(path.EndsWith("s282_mech_connect_to_cutoff_base",StringComparison.Ordinal)||path.EndsWith("s282_mech_connect_to_cutoff_rod",StringComparison.Ordinal))
                    layout.Pairs.Add(new Pair{Label=label+" vs own crosshead",A=moving,B=crosshead});
                // Tender and locomotive are separate cars: no invented inter-car
                // coupler transform is used to assert their mutual clearance.
                if(e==0&&id!=10)
                {
                    var following=layout.Engines[1];
                    layout.Pairs.Add(new Pair{Label=label+" vs following casing",A=moving,B=Rendered(left?following.LeftCasing:following.RightCasing)});
                    layout.Pairs.Add(new Pair{Label=label+" vs following crosshead",A=moving,B=Rendered((left?following.Left:following.Right).Find("s282_mech_push_rod"))});
                }
            }
        }
        return layout;
    }
    static Layout Build(int id,bool phaseOwned)
    {
        var go=new GameObject("Franklin "+id+(phaseOwned?" phase":" reference"));roots.Add(go);go.SetActive(false);
        var layout=new Layout {Root=go,Scale=1};
        var frontLeft=NativeDrive("L",go.transform);var frontRight=NativeDrive("R",go.transform);
        var standard=Asset("franklin_type_b",frontLeft);Asset("franklin_type_b",frontRight);
        var configurer=new FranklinConfiguration(frontLeft,standard,go.transform,
            frontLeft.Find("s282_wheels_driving_2").GetComponent<MeshFilter>().sharedMesh,
            frontLeft.Find("s282_wheels_driving_1").GetComponent<MeshFilter>().sharedMesh);
        bool triplex=id==38||id==40||id==47;float[] offsets;
        float shorten=0;
        if(triplex)
        {
            var measured=configurer.Triplex(id!=40,id==47);
            layout.Scale=measured[0];offsets=measured.Skip(1).Take(3).ToArray();layout.FrontPin=measured[4];layout.MiddlePin=measured[5];shorten=measured[6];
        }
        else
        {
            offsets=new[]{0f,-4.65f};
            layout.FrontPin=((id==32?6.06f:6.3f)+8.0018787f-4.65f)*.5f;
        }
        for(int e=0;e<offsets.Length;e++)
        {
            var frame=new GameObject("engine pivot "+e).transform;frame.SetParent(go.transform,false);
            var origin=new GameObject("engine origin "+e).transform;origin.SetParent(frame,false);
            float pin=e==0?layout.FrontPin:e==1&&triplex?layout.MiddlePin:0;
            frame.localPosition=new Vector3(0,0,pin);origin.localPosition=new Vector3(0,0,-pin);
            Transform l=e==0?frontLeft:NativeDrive("L",origin),r=e==0?frontRight:NativeDrive("R",origin);
            if(e==0){l.SetParent(origin,false);r.SetParent(origin,false);}
            else{Asset("franklin_type_b",l);Asset("franklin_type_b",r);}
            foreach(var drive in new[]{l,r})
            {
                bool left=drive==l;
                drive.localPosition=new Vector3(0,0,offsets[e]);drive.localScale=new Vector3(left?-1:1,layout.Scale,layout.Scale);
                if(triplex)configurer.TriplexDrive(drive,e!=0);
                else
                {
                    configurer.Duplex(drive,e!=0);
                    if(e==0)
                    {
                        drive.Find("s282_wheels_driving_1").localPosition=new Vector3(0,.71f,id==32?7.76f:8f);
                        drive.Find("s282_wheels_driving_2").localPosition=new Vector3(0,.71f,id==32?6.06f:6.3f);
                        if(id==32)
                        {
                            drive.Find("franklin_type_b(Clone)").gameObject.SetActive(false);
                            Asset("franklin_type_b_4444",drive);
                            drive.Find("s282_mech_push_rod").localScale=new Vector3(1,1,1.35f);
                        }
                    }
                }
                var animator=drive.GetComponent<Animator>();
                animator.runtimeAnimatorController=id==32&&e==0?bundle.LoadAsset<RuntimeAnimatorController>("franklin_b_anim_ctrl_4444"):FixtureAssets.Controller;
                animator.speed=0;animator.SetFloat("SpeedMultiplier",0);drive.gameObject.SetActive(true);
            }
            layout.Engines.Add(new Engine{Left=l,Right=r,Frame=frame,Origin=origin,Animators=new[]{l.GetComponent<Animator>(),r.GetComponent<Animator>()}});
        }
        go.SetActive(true);
        foreach(var engine in layout.Engines)
            foreach(var drive in new[]{engine.Left,engine.Right})
            {
                if(triplex)Retarget(drive,-shorten,go.transform);
                var franklin=drive.Find(id==32&&engine==layout.Engines[0]?"franklin_type_b_4444(Clone)":"franklin_type_b(Clone)");
                ShapeCasing(franklin,drive,layout.Scale,id==32&&engine==layout.Engines[1]&&((bool?)config["applyNoseCorrection"]??true));
            }
        for(int e=0;e<layout.Engines.Count;e++)
            foreach(bool left in new[]{true,false})
            {
                Transform drive=left?layout.Engines[e].Left:layout.Engines[e].Right;
                var crank=Rendered(drive.Find("s282_mech_wheels_connect/s282_mech_connect_to_cutoff_base"));
                var crosshead=Rendered(drive.Find("s282_mech_push_rod"));
                var f=drive.Find(id==32&&e==0?"franklin_type_b_4444(Clone)":"franklin_type_b(Clone)");
                var casing=Rendered(f.Find("cylinder/default"));
                layout.Pairs.Add(new Pair{Label=e+(left?" L":" R")+" crank vs own crosshead",A=crank,B=crosshead});
                layout.Pairs.Add(new Pair{Label=e+(left?" L":" R")+" crank vs own casing",A=crank,B=casing});
                if(e+1<layout.Engines.Count)
                {
                    Transform following=left?layout.Engines[e+1].Left:layout.Engines[e+1].Right;
                    layout.Pairs.Add(new Pair{Label=e+(left?" L":" R")+" crank vs following casing",A=crank,B=Rendered(following.Find("franklin_type_b(Clone)/cylinder/default"))});
                    layout.Pairs.Add(new Pair{Label=e+(left?" L":" R")+" crank vs following crosshead",A=crank,B=Rendered(following.Find("s282_mech_push_rod"))});
                }
            }
        return layout;
    }
    static void Bind(Engine engine,int index)
    {
        var rse=(ReciprocatingSteamEngine)FormatterServices.GetUninitializedObject(typeof(ReciprocatingSteamEngine));
        engine.Phase=new Port("franklin "+index,new PortDefinition(PortType.READONLY_OUT,PortValueType.GENERIC,"CRANK_ROTATION"));
        typeof(ReciprocatingSteamEngine).GetField("crankRotationReadOut",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(rse,engine.Phase);
        var type=production.GetType("RearrangedS282.GroupCrankAnimation",true);
        engine.Binding=(IDisposable)type.GetMethod("Bind",Static).Invoke(null,new object[]{rse,engine.Animators,new[]{0f,.75f},Array.Empty<PoweredWheelRotationViaAnimation>(),null});
    }
    static float Repeat(float value)=>value-Mathf.Floor(value);
    static void Seek(Layout layout,float phase,int pattern,bool owned)
    {
        for(int e=0;e<layout.Engines.Count;e++)
        {
            var engine=layout.Engines[e];float p=phase+(pattern==0?0:e*.137f);
            if(owned)engine.Phase.Value=Repeat(p);
            else for(int side=0;side<2;side++)
            {
                var animator=engine.Animators[side];
                animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash,0,Repeat(p+(side==0?0:.75f)));
            }
        }
    }
    static void Yaw(Layout layout,float degrees)
    {
        bool triplex=layout.Engines.Count==3&&!layout.Wals;
        for(int e=0;e<layout.Engines.Count;e++)
            layout.Engines[e].Frame.localRotation=Quaternion.Euler(0,e==0?degrees:e==1&&triplex?degrees*.4f:0,0);
    }
    static float Compare(Layout a,Layout b)
    {
        float max=0;
        for(int e=0;e<a.Engines.Count;e++)for(int s=0;s<2;s++)
        {
            var x=s==0?a.Engines[e].Left:a.Engines[e].Right;
            var y=s==0?b.Engines[e].Left:b.Engines[e].Right;
            foreach(string path in new[]{"s282_mech_push_rod","s282_mech_wheels_connect/s282_mech_connect_to_cutoff_base","s282_wheels_driving_2"})
            {
                var p=x.Find(path);var q=y.Find(path);
                max=Mathf.Max(max,Vector3.Distance(p.position,q.position));
                max=Mathf.Max(max,Quaternion.Angle(p.rotation,q.rotation)/360f);
            }
        }
        return max;
    }
    static void CaptureIndependent(Layout layout,int phase,int steps)
    {
        foreach(var p in layout.Pairs.Where(x=>x.Label.Contains("following crosshead")))
        {
            if(phase==0){p.PhaseA=new Matrix4x4[steps];p.PhaseB=new Matrix4x4[steps];p.BoundsA=new Bounds[steps];p.BoundsB=new Bounds[steps];}
            p.PhaseA[phase]=p.A.transform.localToWorldMatrix;p.PhaseB[phase]=p.B.transform.localToWorldMatrix;
            p.BoundsA[phase]=WorldBounds(p.A);p.BoundsB[phase]=WorldBounds(p.B);
        }
    }
    static void CheckIndependent(Layout layout,int id,float yaw,float cutoff)
    {
        foreach(var p in layout.Pairs.Where(x=>x.Label.Contains("following crosshead")))
        {
            int steps=p.PhaseA.Length;p.IndependentPairs+=(long)steps*steps;
            var a=p.BoundsA[0];var b=p.BoundsB[0];foreach(var bounds in p.BoundsA)a.Encapsulate(bounds);foreach(var bounds in p.BoundsB)b.Encapsulate(bounds);
            if(!a.Intersects(b))continue;
            for(int i=0;i<steps;i++)for(int j=0;j<steps;j++)
            {
                if(!p.BoundsA[i].Intersects(p.BoundsB[j]))continue;p.IndependentBoundsPairs++;
                if(!IntersectAt(Geometry(p.A.sharedMesh),p.PhaseA[i],Geometry(p.B.sharedMesh),p.PhaseB[j],out var witness))continue;
                p.IndependentContacts++;
                if(p.First!=null)continue;
                p.First=new JObject{["phaseA"]=(float)i/steps,["phaseB"]=(float)j/steps,["yaw"]=yaw,["cutoff"]=cutoff,["independentPhaseGrid"]=true,["witness"]=JV(witness)};
                string filename="independent-contact-"+id+"-"+layout.Pairs.IndexOf(p)+".json";
                File.WriteAllText(Path.Combine(outputDirectory,filename),new JObject{["pair"]=p.Label,["pose"]=p.First,["a"]=SnapshotAt(Geometry(p.A.sharedMesh),p.PhaseA[i]),["b"]=SnapshotAt(Geometry(p.B.sharedMesh),p.PhaseB[j])}.ToString());
                p.First["geometryFile"]=filename;
            }
        }
    }
    static void Sample(Layout layout,int id,float yaw,int pattern,float phase,float cutoff)
    {
        foreach(var pair in layout.Pairs)
        {
            if(!Visible(pair.A)||!Visible(pair.B))throw new InvalidOperationException("A required probe pair is not rendered: "+pair.Label);
            var a=WorldBounds(pair.A);var b=WorldBounds(pair.B);
            if(pair.Label.Contains("following casing"))pair.MinimumFollowingZGap=Mathf.Min(pair.MinimumFollowingZGap,a.min.z-b.max.z);
            if(layout.Wals&&id==37&&pair.Label.Contains("following casing")&&(pair.A.name=="s282_mech_connect_to_cutoff_base"||pair.A.name=="s282_mech_push_rod_to_connect"))
                pair.MinimumSurfaceDistanceLowerM=SurfaceDistanceLower(pair.A,pair.B,pair.MinimumSurfaceDistanceLowerM);
            if(!a.Intersects(b))continue;pair.AabbSamples++;
            if(!Intersect(pair.A,pair.B,out var witness))continue;pair.Intersections++;
            if(!float.IsPositiveInfinity(pair.MinimumSurfaceDistanceLowerM))pair.MinimumSurfaceDistanceLowerM=0;
            string condition=yaw.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+"/"+cutoff.ToString("R",System.Globalization.CultureInfo.InvariantCulture);
            pair.ContactsByCondition.TryGetValue(condition,out int count);pair.ContactsByCondition[condition]=count+1;
            if(pair.First!=null)continue;
            pair.First=new JObject{["phase"]=phase,["yaw"]=yaw,["cutoff"]=cutoff,["pattern"]=pattern,["witness"]=JV(witness),["aPath"]=Relative(layout.Root.transform,pair.A.transform),["bPath"]=Relative(layout.Root.transform,pair.B.transform)};
            string filename="contact-"+id+"-"+layout.Pairs.IndexOf(pair)+".json";
            File.WriteAllText(Path.Combine(outputDirectory,filename),new JObject{["pair"]=pair.Label,["pose"]=pair.First,["a"]=Snapshot(pair.A),["b"]=Snapshot(pair.B)}.ToString());
            pair.First["geometryFile"]=filename;
        }
    }
    static bool Visible(MeshFilter f)=>f!=null&&f.sharedMesh!=null&&f.gameObject.activeInHierarchy&&f.GetComponent<MeshRenderer>().enabled;
    static Bounds WorldBounds(MeshFilter f)
    {
        var b=f.sharedMesh.bounds;var min=b.min;var max=b.max;var value=new Bounds(f.transform.TransformPoint(min),Vector3.zero);
        for(int i=0;i<8;i++)value.Encapsulate(f.transform.TransformPoint(new Vector3((i&1)==0?min.x:max.x,(i&2)==0?min.y:max.y,(i&4)==0?min.z:max.z)));
        return value;
    }
    static Mesh Geometry(Mesh mesh)=>mesh.isReadable?mesh:readableGeometry.TryGetValue(mesh,out var readable)?readable:throw new InvalidOperationException("No private triangle source for unreadable mesh "+mesh.name);
    static JObject Snapshot(MeshFilter f)=>new JObject{["vertices"]=new JArray(Geometry(f.sharedMesh).vertices.Select(v=>(object)JV(f.transform.TransformPoint(v)))),["triangles"]=new JArray(Geometry(f.sharedMesh).triangles)};
    static JObject SnapshotAt(Mesh mesh,Matrix4x4 map)=>new JObject{["vertices"]=new JArray(mesh.vertices.Select(v=>(object)JV(map.MultiplyPoint3x4(v)))),["triangles"]=new JArray(mesh.triangles)};
    static bool Intersect(MeshFilter a,MeshFilter b,out Vector3 witness)
    {
        var bMesh=Geometry(b.sharedMesh);var aMesh=Geometry(a.sharedMesh);
        return IntersectAt(aMesh,a.transform.localToWorldMatrix,bMesh,b.transform.localToWorldMatrix,out witness);
    }
    static bool IntersectAt(Mesh aMesh,Matrix4x4 aFrame,Mesh bMesh,Matrix4x4 bFrame,out Vector3 witness)
    {
        if(!trees.TryGetValue(bMesh,out var tree)){tree=new TriangleTree(bMesh);trees.Add(bMesh,tree);}
        var vertices=aMesh.vertices;var indices=aMesh.triangles;
        var map=bFrame.inverse*aFrame;
        for(int i=0;i<indices.Length;i+=3)
        {
            Vector3 v0=map.MultiplyPoint3x4(vertices[indices[i]]),v1=map.MultiplyPoint3x4(vertices[indices[i+1]]),v2=map.MultiplyPoint3x4(vertices[indices[i+2]]);
            if(tree.Hit(v0,v1,v2,out var p)){witness=bFrame.MultiplyPoint3x4(p);return true;}
        }
        witness=Vector3.zero;return false;
    }
    static float SurfaceDistanceLower(MeshFilter a,MeshFilter b,float bestWorld)
    {
        var am=Geometry(a.sharedMesh);var bm=Geometry(b.sharedMesh);
        if(!trees.TryGetValue(bm,out var tree)){tree=new TriangleTree(bm);trees.Add(bm,tree);}
        var scale=b.transform.lossyScale;float minimumScale=Mathf.Min(Mathf.Abs(scale.x),Mathf.Abs(scale.y),Mathf.Abs(scale.z));
        float best=bestWorld/minimumScale;best*=best;
        var map=b.transform.worldToLocalMatrix*a.transform.localToWorldMatrix;var v=am.vertices;var t=am.triangles;
        for(int i=0;i<t.Length;i+=3)tree.Distance(map.MultiplyPoint3x4(v[t[i]]),map.MultiplyPoint3x4(v[t[i+1]]),map.MultiplyPoint3x4(v[t[i+2]]),ref best);
        return Mathf.Sqrt(best)*minimumScale;
    }
    // Exact triangle surfaces with a local-space BVH. AABB overlap alone is
    // never reported as contact. Coplanar/fully enclosed volume contacts are
    // outside this surface-intersection probe and are stated in its results.
    sealed class TriangleTree
    {
        readonly Vector3[] vertices;readonly int[] triangles;readonly Node root;
        sealed class Node {internal Bounds Bounds;internal Node Left,Right;internal int[] Faces;}
        internal TriangleTree(Mesh mesh){vertices=mesh.vertices;triangles=mesh.triangles;root=Build(Enumerable.Range(0,triangles.Length/3).ToArray());}
        Bounds FaceBounds(int f){var b=new Bounds(vertices[triangles[f*3]],Vector3.zero);b.Encapsulate(vertices[triangles[f*3+1]]);b.Encapsulate(vertices[triangles[f*3+2]]);return b;}
        Node Build(int[] faces)
        {
            var n=new Node();n.Bounds=FaceBounds(faces[0]);foreach(int f in faces.Skip(1))n.Bounds.Encapsulate(FaceBounds(f));
            if(faces.Length<=12){n.Faces=faces;return n;}
            var size=n.Bounds.size;int axis=size.x>size.y?(size.x>size.z?0:2):(size.y>size.z?1:2);
            faces=faces.OrderBy(f=>FaceBounds(f).center[axis]).ToArray();int half=faces.Length/2;
            n.Left=Build(faces.Take(half).ToArray());n.Right=Build(faces.Skip(half).ToArray());return n;
        }
        internal bool Hit(Vector3 a,Vector3 b,Vector3 c,out Vector3 point)
        {var bounds=new Bounds(a,Vector3.zero);bounds.Encapsulate(b);bounds.Encapsulate(c);return Query(root,bounds,a,b,c,out point);}
        internal void Distance(Vector3 a,Vector3 b,Vector3 c,ref float best)
        {var bounds=new Bounds(a,Vector3.zero);bounds.Encapsulate(b);bounds.Encapsulate(c);DistanceNode(root,bounds,a,b,c,ref best);}
        static float BoundsDistance(Bounds a,Bounds b)
        {float x=Mathf.Max(0,Mathf.Max(a.min.x-b.max.x,b.min.x-a.max.x)),y=Mathf.Max(0,Mathf.Max(a.min.y-b.max.y,b.min.y-a.max.y)),z=Mathf.Max(0,Mathf.Max(a.min.z-b.max.z,b.min.z-a.max.z));return x*x+y*y+z*z;}
        void DistanceNode(Node n,Bounds bounds,Vector3 a,Vector3 b,Vector3 c,ref float best)
        {
            if(BoundsDistance(n.Bounds,bounds)>=best)return;
            if(n.Faces==null){DistanceNode(n.Left,bounds,a,b,c,ref best);DistanceNode(n.Right,bounds,a,b,c,ref best);return;}
            foreach(int f in n.Faces)
            {
                if(BoundsDistance(FaceBounds(f),bounds)>=best)continue;
                var x=vertices[triangles[f*3]];var y=vertices[triangles[f*3+1]];var z=vertices[triangles[f*3+2]];
                best=Mathf.Min(best,PointTriangle(a,x,y,z));best=Mathf.Min(best,PointTriangle(b,x,y,z));best=Mathf.Min(best,PointTriangle(c,x,y,z));
                best=Mathf.Min(best,PointTriangle(x,a,b,c));best=Mathf.Min(best,PointTriangle(y,a,b,c));best=Mathf.Min(best,PointTriangle(z,a,b,c));
                var av=new[]{a,b,c};var bv=new[]{x,y,z};
                for(int i=0;i<3;i++)for(int j=0;j<3;j++)best=Mathf.Min(best,SegmentDistance(av[i],av[(i+1)%3],bv[j],bv[(j+1)%3]));
            }
        }
        static float PointTriangle(Vector3 p,Vector3 a,Vector3 b,Vector3 c)
        {
            var ab=b-a;var ac=c-a;var ap=p-a;float d1=Vector3.Dot(ab,ap),d2=Vector3.Dot(ac,ap);if(d1<=0&&d2<=0)return ap.sqrMagnitude;
            var bp=p-b;float d3=Vector3.Dot(ab,bp),d4=Vector3.Dot(ac,bp);if(d3>=0&&d4<=d3)return bp.sqrMagnitude;
            float vc=d1*d4-d3*d2;if(vc<=0&&d1>=0&&d3<=0)return(p-(a+ab*(d1/(d1-d3)))).sqrMagnitude;
            var cp=p-c;float d5=Vector3.Dot(ab,cp),d6=Vector3.Dot(ac,cp);if(d6>=0&&d5<=d6)return cp.sqrMagnitude;
            float vb=d5*d2-d1*d6;if(vb<=0&&d2>=0&&d6<=0)return(p-(a+ac*(d2/(d2-d6)))).sqrMagnitude;
            float va=d3*d6-d5*d4;if(va<=0&&d4-d3>=0&&d5-d6>=0)return(p-(b+(c-b)*((d4-d3)/((d4-d3)+(d5-d6))))).sqrMagnitude;
            float sum=va+vb+vc;if(Mathf.Abs(sum)<1e-20f)return Mathf.Min(ap.sqrMagnitude,Mathf.Min(bp.sqrMagnitude,cp.sqrMagnitude));
            return(p-(a+ab*(vb/sum)+ac*(vc/sum))).sqrMagnitude;
        }
        static float SegmentDistance(Vector3 a,Vector3 b,Vector3 c,Vector3 d)
        {
            var u=b-a;var v=d-c;var w=a-c;float aa=Vector3.Dot(u,u),bb=Vector3.Dot(u,v),cc=Vector3.Dot(v,v),dd=Vector3.Dot(u,w),ee=Vector3.Dot(v,w);
            if(aa<1e-20f&&cc<1e-20f)return w.sqrMagnitude;
            float s,t;
            if(aa<1e-20f){s=0;t=Mathf.Clamp01(ee/cc);}
            else if(cc<1e-20f){t=0;s=Mathf.Clamp01(-dd/aa);}
            else{float det=aa*cc-bb*bb;s=det>1e-20f?Mathf.Clamp01((bb*ee-cc*dd)/det):0;t=(bb*s+ee)/cc;if(t<0){t=0;s=Mathf.Clamp01(-dd/aa);}else if(t>1){t=1;s=Mathf.Clamp01((bb-dd)/aa);}}
            return(w+u*s-v*t).sqrMagnitude;
        }
        bool Query(Node n,Bounds bounds,Vector3 a,Vector3 b,Vector3 c,out Vector3 p)
        {
            p=Vector3.zero;if(!n.Bounds.Intersects(bounds))return false;
            if(n.Faces==null)return Query(n.Left,bounds,a,b,c,out p)||Query(n.Right,bounds,a,b,c,out p);
            foreach(int f in n.Faces)
            {
                if(!FaceBounds(f).Intersects(bounds))continue;
                var x=vertices[triangles[f*3]];var y=vertices[triangles[f*3+1]];var z=vertices[triangles[f*3+2]];
                if(Segment(a,b,x,y,z,out p)||Segment(b,c,x,y,z,out p)||Segment(c,a,x,y,z,out p)||Segment(x,y,a,b,c,out p)||Segment(y,z,a,b,c,out p)||Segment(z,x,a,b,c,out p))return true;
            }
            return false;
        }
        static bool Segment(Vector3 p,Vector3 q,Vector3 a,Vector3 b,Vector3 c,out Vector3 point)
        {
            point=Vector3.zero;var d=q-p;var e1=b-a;var e2=c-a;var h=Vector3.Cross(d,e2);float det=Vector3.Dot(e1,h);
            if(Mathf.Abs(det)<1e-9f)return false;float inv=1/det;var s=p-a;float u=inv*Vector3.Dot(s,h);
            if(u<0||u>1)return false;var cross=Vector3.Cross(s,e1);float v=inv*Vector3.Dot(d,cross);if(v<0||u+v>1)return false;
            float t=inv*Vector3.Dot(e2,cross);if(t<=1e-6f||t>=1-1e-6f)return false;point=p+d*t;return true;
        }
    }
    public static IEnumerator Run(string outputPath)
    {
        outputDirectory=Path.GetDirectoryName(outputPath);
        string configPath=Path.Combine(outputDirectory,"franklin-config.json");
        if(!File.Exists(configPath))configPath=Path.Combine(Path.GetDirectoryName(typeof(FranklinRuntimeProbe).Assembly.Location),"franklin-config.json");
        config=JObject.Parse(File.ReadAllText(configPath));
        FixtureAssets.WalsMalletGap=(float?)config["malletGap"]??float.NaN;
        data=JObject.Parse(File.ReadAllText((string)config["geometry"]));
        string managed=Path.Combine(Directory.GetParent((string)data["nativeSource"]).FullName,"Managed");
        Assembly.LoadFrom(Path.Combine(managed,"UnityModManager","UnityModManager.dll"));
        Assembly.LoadFrom(Path.Combine(managed,"UnityModManager","0Harmony.dll"));
        production=Assembly.LoadFrom((string)config["productionDll"]);
        production.GetType("RearrangedS282.Main",true).GetProperty("ModPath",Static).GetSetMethod(true).Invoke(null,new object[]{Directory.GetParent((string)config["assets"]).FullName});
        bundle=AssetBundle.LoadFromFile((string)data["bundle"]);
        Require(bundle!=null,"Untouched original Franklin asset bundle loaded by Unity");
        FixtureAssets.Controller=bundle.LoadAsset<RuntimeAnimatorController>("franklin_b_anim_ctrl");
        Require(FixtureAssets.Controller!=null,"Original packed Franklin AnimatorController loaded");
        PrepareRods((string)config["assets"]);
        int steps=(int?)config["steps"]??720;
        var ids=config["ids"]?.Select(x=>(int)x).ToArray()??new[]{31,32,38,40,47};
        var yaws=config["yaws"]?.Select(x=>(float)x).ToArray()??new[]{-15f,0f,15f};
        if(DrivetrainSource==null)DrivetrainSource=Resources.Load<GameObject>("NativeS282DrivetrainR");
        if(DrivetrainSource!=null)FixtureAssets.NativeController=DrivetrainSource.GetComponent<Animator>().runtimeAnimatorController;
        if(config["walsStatic"]!=null)walsData=JObject.Parse(File.ReadAllText((string)config["walsStatic"]));
        try
        {
            foreach(int id in ids)
            {
                bool wals=id==10||id==37||id==41||id==42||id==43||id==45||id==46;
                var layout=wals?BuildWals(id,true):Build(id,true);var reference=wals?BuildWals(id,false):Build(id,false);
                yield return null;yield return null;
                for(int e=0;e<layout.Engines.Count;e++)Bind(layout.Engines[e],e);
                float maxDifference=0;int samples=0;
                var cutoffs=wals?(config["cutoffs"]?.Select(x=>(float)x).ToArray()??new[]{-1f,0f,1f}):new[]{0f};
                int patterns=(int?)config["patterns"]??2;
                foreach(float cutoff in cutoffs)foreach(float yaw in yaws)for(int pattern=0;pattern<patterns;pattern++)
                {
                    Yaw(layout,yaw);Yaw(reference,yaw);
                    if(wals)foreach(var engine in layout.Engines.Concat(reference.Engines))engine.Cutoff.Value=cutoff;
                    Vector3 initialCrosshead=Vector3.zero;Quaternion initialWheel=Quaternion.identity;
                    for(int i=0;i<steps;i++)
                    {
                        float phase=(float)i/steps;Seek(layout,phase,pattern,true);Seek(reference,phase,pattern,false);
                        yield return null;yield return new WaitForEndOfFrame();
                        maxDifference=Mathf.Max(maxDifference,Compare(layout,reference));
                        var drive=layout.Engines[0].Right;
                        if(i==0){initialCrosshead=drive.Find("s282_mech_push_rod").localPosition;initialWheel=drive.Find("s282_wheels_driving_2").localRotation;}
                        if(i==steps/4)
                        {
                            Require(Vector3.Distance(initialCrosshead,drive.Find("s282_mech_push_rod").localPosition)>.02f,"ID "+id+" native crosshead moves at yaw "+yaw+" pattern "+pattern);
                            Require(Quaternion.Angle(initialWheel,drive.Find("s282_wheels_driving_2").localRotation)>20f,"ID "+id+" native driver rotates at yaw "+yaw+" pattern "+pattern);
                        }
                        Sample(layout,id,yaw,pattern,phase,cutoff);samples++;
                        if(pattern==0)CaptureIndependent(layout,i,steps);
                    }
                    if(pattern==0)CheckIndependent(layout,id,yaw,cutoff);
                    if(wals)Require(layout.Engines.All(e=>e.CutoffReaders.All(r=>r.enabled)&&e.Animators.All(a=>Mathf.Abs(a.GetFloat("Cutoff")-cutoff)<.0001f)),"Wals native cutoff parameter readers retained by phase binding at "+cutoff);
                }
                Require(maxDifference<.0001f,"ID "+id+" production phase binding matches original controller pose through normal evaluation");
                long unexpected=layout.Pairs.Sum(p=>p.IndependentContacts+(p.Label.Contains("s282_mech_cylinder_rod vs own casing")&&p.Intersections==samples?0:p.Intersections));
                var nativeEntries=layout.Pairs.Where(p=>p.Label.Contains("s282_mech_cylinder_rod vs own casing")).ToArray();
                if(wals)Require(nativeEntries.Length==layout.Engines.Count*2&&nativeEntries.All(p=>p.Intersections==samples),"Native valve spindle entry remains present and matches ID10 control in every sampled pose");
                if(id==37&&unexpected==0)
                {
                    Require(layout.Pairs.Where(p=>!float.IsPositiveInfinity(p.MinimumSurfaceDistanceLowerM)).All(p=>p.MinimumSurfaceDistanceLowerM>=.01f),"2-6-6-2 crank/mainrod retain at least 10mm conservative sampled surface clearance");
                    Require(layout.Measured[7]>=.1f,"2-6-6-2 trailing axle remains ahead of the native twelve-coupled rear axle limit");
                }
                var row=new JObject{["id"]=id,["samples"]=samples,["engines"]=layout.Engines.Count,["scale"]=layout.Scale,["maxPhaseAdapterPoseDifference"]=maxDifference,
                    ["mechanism"]=wals?"Walschaerts":"Franklin",["cutoffs"]=new JArray(cutoffs),
                    ["layoutDatums"]=layout.Measured!=null?new JArray(layout.Measured):null,["malletGap"]=wals&&id==37?(float.IsNaN(FixtureAssets.WalsMalletGap)?WalsConfiguration.ProductionGap:FixtureAssets.WalsMalletGap):0,["testMalletGapOverride"]=config["malletGap"],
                    ["unexpectedContacts"]=unexpected,["clearancePassed"]=unexpected==0,
                    ["pairs"]=new JArray(layout.Pairs.Select(p=>(object)new JObject{["pair"]=p.Label,["classification"]=p.Label.Contains("s282_mech_cylinder_rod vs own casing")&&p.Intersections==samples?"native valve spindle entering its own valve chest":"clearance candidate",["surfaceIntersectionSamples"]=p.Intersections,["aabbOverlapSamples"]=p.AabbSamples,["independentPhasePairs"]=p.IndependentPairs,["independentBoundsCandidates"]=p.IndependentBoundsPairs,["independentSurfaceContacts"]=p.IndependentContacts,["minimumFollowingZGapM"]=float.IsPositiveInfinity(p.MinimumFollowingZGap)?(JToken)JValue.CreateNull():new JValue(p.MinimumFollowingZGap),["minimumSurfaceDistanceLowerM"]=float.IsPositiveInfinity(p.MinimumSurfaceDistanceLowerM)?(JToken)JValue.CreateNull():new JValue(p.MinimumSurfaceDistanceLowerM),["contactsByYawCutoff"]=JObject.FromObject(p.ContactsByCondition),["first"]=p.First}))};
                results.Add(row);File.WriteAllText(outputPath,new JObject{["complete"]=false,["checks"]=new JArray(checks),["layouts"]=results}.ToString());
                Debug.Log("FRANKLIN_LAYOUT_COMPLETE "+id+" samples="+samples+" contacts="+layout.Pairs.Sum(p=>p.Intersections));
                foreach(var e in layout.Engines)e.Binding.Dispose();
                Object.Destroy(layout.Root);Object.Destroy(reference.Root);yield return null;
            }
            long unexpectedTotal=results.Sum(row=>(long)row["unexpectedContacts"]);
            bool negative=(bool?)config["expectBaselineContacts"]??((bool?)config["applyNoseCorrection"]==false);
            File.WriteAllText(outputPath,new JObject{["complete"]=true,["clearancePassed"]=unexpectedTotal==0,["unexpectedContacts"]=unexpectedTotal,["expectedNegativeControlMatched"]=negative&&unexpectedTotal>0,["passed"]=negative?unexpectedTotal>0:unexpectedTotal==0,["checks"]=new JArray(checks),["nativeRootSource"]=DrivetrainSource!=null?"native serialized prefab clone":"complete serialized native drive hierarchy reconstructed from local game",
                ["nativeSourceSHA256"]=data["nativeSHA256"],["bundleSHA256"]=data["bundleSHA256"],["walsStaticUnityValidated"]=walsData?["actualUnityLmsValidated"],["walsStaticReportSHA256"]=walsData?["actualUnityLmsReportSHA256"],["layouts"]=results,
                ["limitations"]="Original Franklin prefab/controllers and full native Walschaerts controller/hierarchy with exact mesh triangles; exact extracted production mechanism configuration, production phase adapter and retarget LateUpdate, normal Unity Animator evaluation. Wals static cylinders are actual installed-LMS Unity output. NativeID10 is a measured mechanism control configured to four axles, not a complete stock TrainCar. Detached RSE supplies only native phase Port; no live TrainCar, wheel physics, multiplayer, camera/LOD selection, full locomotive static frame/guides, inter-car coupler clearance, coplanar or wholly enclosed volume tests. Curve poses are explicit yaw stress cases about production pin datums, not live track constraints. Independent phase grids replay captured evaluated poses; disjoint sampled swept bounds prove separation for those phase samples. Native valve spindle entry into its own chest is retained and reported separately, with every-pose matching counts required."}.ToString());
            Debug.Log("FRANKLIN_RUNTIME_COMPLETE");
        }
        finally{foreach(var root in roots)if(root!=null)Object.Destroy(root);}
    }
}
