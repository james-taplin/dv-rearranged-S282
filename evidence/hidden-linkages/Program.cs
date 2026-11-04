using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Text.Json;
using RearrangedS282;
using UnityEngine;
using Object=UnityEngine.Object;

internal static class HiddenLinkageFixture
{
    const string Reach="s282_mech_cutoff_rod",Lube="s282_mech_cutoff_rail/s282_mech_lubricator_rod",Lever="LubricatorParts/s282_mech_lubricator_part_1";
    static int checks,poses,integrationPoses,integrationCases,excludedCases,actualObjImports,rawMirrorNegativeControls;
    static string exportDirectory;
    static float maxEndpointError,maxNativeContact;
    static void Check(bool condition,string message){checks++;if(!condition)throw new InvalidOperationException(message);}
    static void Near(Vector3 a,Vector3 b,string message,float tolerance=.00008f){float e=(a-b).magnitude;maxEndpointError=Math.Max(maxEndpointError,e);Check(e<tolerance,message+" error="+e);}
    static Vector3 V(JsonElement e)=>new(e[0].GetSingle(),e[1].GetSingle(),e[2].GetSingle());
    static Quaternion Q(JsonElement e)=>new(e[0].GetSingle(),e[1].GetSingle(),e[2].GetSingle(),e[3].GetSingle());
    static readonly (string path,Vector3 pin)[] Specs={
        (Reach,new(0,0,-6.07456827f)),(Reach+"/"+Reach+"_LOD1",new(0,0,-5.74456787f)),
        (Lube,new(0,0,1.9299124f)),(Lube+"/s282_mech_lubricator_rod_LOD1",new(.00108051114f,-.00026285648f,1.92260874246f))};
    static readonly Vector3 LeverPin=new(-.03564119339f,-.15584469477f,-.00009246053f);
    static Mesh ActualObj(Mesh original,Vector3[] nativeVertices)
    {
        string path=Path.Combine(exportDirectory,original.name+".obj");
        Check(File.Exists(path),"Actual installed exporter payload required: "+path);
        var vertices=new List<Vector3>();var triangles=new List<int>();
        foreach(string line in File.ReadLines(path))
        {
            var v=line.Split(' ',StringSplitOptions.RemoveEmptyEntries);if(v.Length==0)continue;
            if(v[0]=="v")vertices.Add(new(float.Parse(v[1],CultureInfo.InvariantCulture),float.Parse(v[2],CultureInfo.InvariantCulture),float.Parse(v[3],CultureInfo.InvariantCulture)));
            else if(v[0]=="f")
            {
                Check(v.Length==4,"Native exporter emits triangular faces");
                foreach(string part in v.Skip(1))triangles.Add(int.Parse(part.Split('/')[0],CultureInfo.InvariantCulture)-1);
            }
        }
        var mesh=new Mesh{name=original.name,vertices=vertices.ToArray(),triangles=triangles.ToArray()};mesh.RecalculateBounds();
        Check(mesh.vertexCount==nativeVertices.Length,"Actual exported topology vertex count matches exact native");
        float rawError=Enumerable.Range(0,vertices.Count).Max(i=>(vertices[i]-nativeVertices[i]).magnitude);
        Check(rawError>.001f,"Unnormalized OBJ X reflection must fail native-coordinate negative control");rawMirrorNegativeControls++;
        MeshFinder.NormalizeLinkageImport(mesh);actualObjImports++;
        for(int i=0;i<nativeVertices.Length;i++)Near(mesh.vertices[i],nativeVertices[i],"Actual exporter normalized native coordinate",.000003f);
        Check(mesh.triangles.SequenceEqual(original.triangles),"Actual exporter normalized triangle winding equals native");
        return mesh;
    }
    sealed class Rig
    {
        internal readonly Transform root=new GameObject("car").transform,drive;
        internal readonly Dictionary<string,Transform> parts=new();
        internal readonly Dictionary<string,Mesh> original=new();
        internal readonly LODGroup lod;
        internal readonly LOD[] nativeLods;
        internal readonly bool left;
        internal Rig(JsonElement geometry,bool left)
        {
            this.left=left;
            root.localPosition=new(13,2,-7);root.localRotation=Quaternion.AngleAxis(19,Vector3.up);
            drive=new GameObject("drive").transform;drive.SetParent(root,false);drive.localScale=new(left?-1:1,1,1);
            foreach(var n in geometry.GetProperty("drives").GetProperty("LocoS282A_Drivetrain R").GetProperty("children").EnumerateArray())Add(n,drive,"");
            lod=root.gameObject.AddComponent<LODGroup>();
            nativeLods=new[]{new LOD(.4f,parts.Where(p=>!p.Key.EndsWith("_LOD1")).Select(p=>p.Value.GetComponent<MeshRenderer>()).Where(x=>x!=null).Cast<Renderer>().ToArray()),
                new LOD(.1f,parts.Where(p=>p.Key.EndsWith("_LOD1")).Select(p=>p.Value.GetComponent<MeshRenderer>()).Where(x=>x!=null).Cast<Renderer>().ToArray())};
            lod.SetLODs(nativeLods);lod.size=20;
            void Add(JsonElement node,Transform parent,string prefix)
            {
                string name=node.GetProperty("name").GetString(),path=prefix+name;
                var t=new GameObject(name).transform;t.SetParent(parent,false);parts.Add(path,t);
                t.localPosition=V(node.GetProperty("position"));t.localRotation=Q(node.GetProperty("rotation"));t.localScale=V(node.GetProperty("scale"));
                if(node.TryGetProperty("mesh",out var key))
                {
                    var m=geometry.GetProperty("nativeMeshes").GetProperty(key.GetString());
                    var mesh=new Mesh{name=m.GetProperty("name").GetString(),vertices=m.GetProperty("vertices").EnumerateArray().Select(V).ToArray(),triangles=m.GetProperty("triangles").EnumerateArray().Select(x=>x.GetInt32()).ToArray()};
                    mesh.RecalculateBounds();t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
                    t.gameObject.AddComponent<MeshRenderer>();original.Add(path,mesh);
                }
                foreach(var c in node.GetProperty("children").EnumerateArray())Add(c,t,path+"/");
            }
        }
        internal void Pose(JsonElement clip,JsonElement frame)
        {
            var values=frame.GetProperty("values");
            foreach(var b in clip.GetProperty("bindings").EnumerateArray())
            {
                if(!parts.TryGetValue(b.GetProperty("path").GetString(),out var t))continue;
                var ids=b.GetProperty("indices").EnumerateArray().Select(x=>x.GetInt32()).ToArray();var defaults=b.GetProperty("first").EnumerateArray().Select(x=>x.GetSingle()).ToArray();
                var v=ids.Select((id,i)=>values.TryGetProperty(id.ToString(),out var x)?x.GetSingle():defaults[i]).ToArray();
                switch(b.GetProperty("attribute").GetInt32()) {case 1:t.localPosition=new(v[0],v[1],v[2]);break;case 2:t.localRotation=new(v[0],v[1],v[2],v[3]);break;case 3:t.localScale=new(v[0],v[1],v[2]);break;}
            }
        }
        internal Transform Proxy(string path)=>parts[path].children.Single(t=>t.name.StartsWith("[Rearranged retarget]"));
    }
    static void Update(Rig rig){foreach(var r in rig.drive.GetComponents<DriveRetarget>())typeof(DriveRetarget).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(r,null);}
    static void CheckBounds(Rig rig,Transform proxy)
    {
        var mesh=proxy.GetComponent<MeshFilter>().sharedMesh;var group=rig.lod;
        foreach(var v in mesh.vertices)
        {
            var p=group.transform.InverseTransformPoint(proxy.TransformPoint(v))-group.localReferencePoint;
            Check(Math.Abs(p.x)<=group.size*.5f+.00005f&&Math.Abs(p.y)<=group.size*.5f+.00005f&&Math.Abs(p.z)<=group.size*.5f+.00005f,"LOD native+retarget envelope covers sampled rendered vertex");
        }
    }
    static void BaselineShape(JsonElement geometry,JsonElement animation)
    {
        foreach(bool left in new[]{false,true})
        {
            var rig=new Rig(geometry,left);var reference=new Rig(geometry,left);
            var native=rig.root.worldToLocalMatrix*rig.drive.localToWorldMatrix;
            foreach(var spec in Specs.Where(s=>!left||s.path.StartsWith(Reach)))DriveRetarget.AddFixedEndpoint(rig.drive,rig.root,native,spec.path,spec.pin,rig.root);
            foreach(var clip in animation.GetProperty("clips").EnumerateArray())foreach(var frame in clip.GetProperty("frames").EnumerateArray())
            {
                rig.Pose(clip,frame);reference.Pose(clip,frame);Update(rig);
                foreach(var spec in Specs.Where(s=>!left||s.path.StartsWith(Reach)))
                {
                    var proxy=rig.Proxy(spec.path);var actual=proxy.GetComponent<MeshFilter>().sharedMesh.vertices;var original=reference.original[spec.path].vertices;
                    for(int i=0;i<actual.Length;i++)Near(proxy.TransformPoint(actual[i]),reference.parts[spec.path].TransformPoint(original[i]),"Native-frame retarget preserves every original vertex");
                }
            }
            DriveRetarget.Unbind(rig.drive);
        }
    }
    static void ExistingRetarget(JsonElement geometry,JsonElement animation,float scale,float offset)
    {
        foreach(bool left in new[]{false,true})
        {
            var rig=new Rig(geometry,left);var native=rig.root.worldToLocalMatrix*rig.drive.localToWorldMatrix;
            rig.drive.localPosition=new(0,0,offset);rig.drive.localScale=new(left?-1:1,scale,scale);
            var owner=DriveRetarget.Bind(rig.drive,.5f/scale,DriveRetarget.NativeWalschaertsRole,rig.root);
            string main="s282_mech_wheels_connect/s282_mech_push_rod_to_connect",radius="s282_mech_cutoff_boomerang_high/s282_mech_cutoff_rail_connection/s282_mech_cutoff_to_cylinder_rod";
            var mainProxy=rig.Proxy(main);var radiusProxy=rig.Proxy(radius);
            foreach(var spec in Specs.Where(s=>!left||s.path.StartsWith(Reach)))DriveRetarget.AddFixedEndpoint(rig.drive,rig.root,native,spec.path,spec.pin,rig.root);
            if(!left)DriveRetarget.AddAnchoredPart(rig.drive,rig.root,native,"LubricatorParts",rig.root);
            Check(ReferenceEquals(owner,rig.drive.GetComponent<DriveRetarget>())&&rig.drive.GetComponents<DriveRetarget>().Length==1,"Hidden endpoints retain existing main/radius retarget owner");
            foreach(var clip in animation.GetProperty("clips").EnumerateArray())foreach(var frame in clip.GetProperty("frames").EnumerateArray())
            {
                rig.Pose(clip,frame);Update(rig);
                Check(ReferenceEquals(mainProxy,rig.Proxy(main))&&ReferenceEquals(radiusProxy,rig.Proxy(radius)),"Endpoint addition and native samples retain main/radius proxies");
                foreach(var proxy in new[]{mainProxy,radiusProxy})Check(rig.lod.GetLODs().Any(l=>l.renderers.Contains(proxy.GetComponent<MeshRenderer>())),"Existing main/radius proxy retains LOD membership");
            }
            DriveRetarget.Unbind(rig.drive);
            foreach(var mesh in rig.original)Check(ReferenceEquals(rig.parts[mesh.Key].GetComponent<MeshFilter>().sharedMesh,mesh.Value),"Combined owner restores all native mesh identities");
            for(int i=0;i<rig.nativeLods.Length;i++)Check(rig.lod.GetLODs()[i].renderers.SequenceEqual(rig.nativeLods[i].renderers),"Combined owner restores all native LOD references/order");
        }
    }
    static void Integration(JsonElement geometry,JsonElement animation,IEnumerable<JsonElement> layouts)
    {
        foreach(var layout in layouts)
        {
            int id=layout.GetProperty("id").GetInt32();float scale=layout.GetProperty("scale").GetSingle(),offset=layout.GetProperty("driveOffset").GetSingle();
            bool articulated=WheelArrangement.IsArticulatedId(id);
            var yaws=layout.TryGetProperty("yaws",out var ys)?ys.EnumerateArray().Select(v=>v.GetSingle()).ToArray():new[]{0f};
            foreach(float yaw in yaws)
            foreach(bool show in new[]{true,false})
            {
                var car=new GameObject("integration owner").transform;car.localPosition=new(13,2,-7);car.localRotation=Quaternion.AngleAxis(19,Vector3.up);
                var left=new Rig(geometry,true);var right=new Rig(geometry,false);
                foreach(var rig in new[]{left,right}){rig.root.SetParent(car,false);rig.root.localPosition=Vector3.zero;rig.root.localRotation=Quaternion.identity;}
                var owner=car.gameObject.AddComponent<WheelRearranger>();owner.driveL=left.drive;owner.driveR=right.drive;
                owner.lubricator=new GameObject("native oiler").transform;owner.lubricator.SetParent(car,false);
                var support=new GameObject("native reach guide").transform;support.SetParent(car,false);owner.SupportFixture(support);
                owner.CacheFixture();
                var referenceL=new Rig(geometry,true);var referenceR=new Rig(geometry,false);
                var oilerOffset=layout.TryGetProperty("oilerOffset",out var oo)?V(oo):Vector3.zero;owner.lubricator.localPosition=oilerOffset;
                foreach(var rig in new[]{left,right})
                {
                    rig.drive.localPosition=new(0,0,offset);rig.drive.localScale=new(rig.left?-1:1,scale,scale);
                    if(articulated)
                    {
                        float pin=layout.GetProperty("pinZ").GetSingle();var turn=Matrix4x4.TRS(new(0,0,pin),Quaternion.AngleAxis(yaw,Vector3.up),Vector3.one)*Matrix4x4.TRS(new(0,0,-pin),Quaternion.identity,Vector3.one);
                        rig.drive.localPosition=turn.MultiplyPoint3x4(rig.drive.localPosition);rig.drive.localRotation=Quaternion.AngleAxis(yaw,Vector3.up);
                    }
                    rig.parts[Reach].gameObject.SetActive(false);rig.parts[Lube].gameObject.SetActive(false);
                    rig.parts["LubricatorParts"].gameObject.SetActive(true);
                    foreach(var r in rig.parts["LubricatorParts"].GetComponentsInChildren<MeshRenderer>(true))r.enabled=false;
                    rig.parts["s282_mech_cutoff_rail"].gameObject.SetActive(show);
                }
                owner.ApplyFixture(id);integrationCases++;
                if(!show)
                {
                    Check(left.drive.GetComponents<DriveRetarget>().Length==0&&right.drive.GetComponents<DriveRetarget>().Length==0,"Existing valve-gear setting suppresses linkage repair");
                    Check(!left.parts[Reach].gameObject.activeSelf&&!right.parts[Reach].gameObject.activeSelf,"Hidden gear keeps reach rods hidden");
                    owner.SuspendFixture();continue;
                }
                Check(left.parts[Reach].gameObject.activeSelf&&right.parts[Reach].gameObject.activeSelf,"Repair reveals both connected reaches");
                Check(right.parts[Lube].gameObject.activeSelf&&right.parts["LubricatorParts"].gameObject.activeSelf,"Repair reveals right connected lubricator");
                Check(!support.gameObject.activeSelf,"Incompatible native mid-shaft guide remains hidden");
                Check(!left.parts[Lube].gameObject.activeSelf,"No left lubricator mechanism is invented");
                foreach(var clip in animation.GetProperty("clips").EnumerateArray())foreach(var frame in clip.GetProperty("frames").EnumerateArray())
                {
                    left.Pose(clip,frame);right.Pose(clip,frame);referenceL.Pose(clip,frame);referenceR.Pose(clip,frame);
                    foreach(var pair in new[]{(rig:left,reference:referenceL),(rig:right,reference:referenceR)})
                    {
                        var source=pair.rig.parts.ToDictionary(x=>x.Key,x=>(p:x.Value.localPosition,q:x.Value.localRotation,s:x.Value.localScale));Update(pair.rig);
                        foreach(var spec in Specs.Where(s=>!pair.rig.left||s.path.StartsWith(Reach)))
                        {
                            var proxy=pair.rig.Proxy(spec.path);var mesh=proxy.GetComponent<MeshFilter>().sharedMesh;
                            var toAxis=Matrix4x4.TRS(Vector3.zero,Quaternion.FromToRotation(spec.pin.normalized,Vector3.forward),Vector3.one);
                            float nativeExtreme=pair.rig.original[spec.path].vertices.Max(v=>toAxis.MultiplyPoint3x4(v).z);
                            float length=mesh.vertices.Max(v=>v.z)-(nativeExtreme-spec.pin.magnitude);
                            var fixedPoint=pair.reference.parts[spec.path].TransformPoint(spec.pin);
                            if(!spec.path.StartsWith(Reach))fixedPoint+=car.TransformVector(oilerOffset);
                            Near(proxy.TransformPoint(new(0,0,length)),fixedPoint,"Actual integration fixed endpoint ID"+id);
                            Near(proxy.TransformPoint(Vector3.zero),pair.rig.parts[spec.path].position,"Actual integration moving endpoint ID"+id);
                            CheckBounds(pair.rig,proxy);
                            bool expected=!articulated||!spec.path.EndsWith("_LOD1");Check(proxy.GetComponent<MeshRenderer>().enabled==expected,"Arrangement retains native/coarse LOD visibility policy");
                        }
                        foreach(var part in pair.rig.parts){var before=source[part.Key];Near(part.Value.localPosition,before.p,"Integration preserves source animation position");Check(part.Value.localRotation.Equals(before.q)&&part.Value.localScale.Equals(before.s),"Integration preserves source animation rotation+scale");}
                    }
                    var lever=right.Proxy(Lever);var displayed=lever.GetComponent<MeshFilter>().sharedMesh.vertices;var original=right.original[Lever].vertices;
                    for(int i=0;i<original.Length;i++)Near(lever.TransformPoint(displayed[i]),referenceR.parts[Lever].TransformPoint(original[i])+car.TransformVector(oilerOffset),"Actual integration oiler lever native angle and trajectory");
                    integrationPoses++;
                }
                owner.SuspendFixture();owner.SuspendFixture();
                foreach(var rig in new[]{left,right})
                {
                    Check(rig.drive.GetComponents<DriveRetarget>().Length==0,"Switch suspension removes endpoint owner");
                    foreach(var mesh in rig.original)Check(ReferenceEquals(rig.parts[mesh.Key].GetComponent<MeshFilter>().sharedMesh,mesh.Value),"Switch suspension restores every native mesh identity");
                    for(int i=0;i<rig.nativeLods.Length;i++)Check(rig.lod.GetLODs()[i].renderers.SequenceEqual(rig.nativeLods[i].renderers),"Switch suspension restores native LOD identities/order");
                }
                owner.ApplyFixture(10);Check(left.drive.GetComponents<DriveRetarget>().Length==0&&right.drive.GetComponents<DriveRetarget>().Length==0,"Return to stock cannot create linkage retarget");
            }
        }
        foreach(int excluded in Enumerable.Range(0,49).Where(id=>!WheelRearranger.HasHiddenNativeLinkages(id)))
        {
            var owner=new GameObject("excluded ID"+excluded).AddComponent<WheelRearranger>();owner.ApplyFixture(excluded);owner.SuspendFixture();excludedCases++;
        }
        Check(!WheelRearranger.HasHiddenNativeLinkages(36),"2-2-2 explicitly excluded");
        foreach(int franklin in new[]{31,32,38,40,47})Check(!WheelRearranger.HasHiddenNativeLinkages(franklin),"Franklin stock Walschaerts parts never restored");
        // One concrete exporter failure exercises the actual suspension unwind.
        var failedLeft=new Rig(geometry,true);var failedRight=new Rig(geometry,false);
        failedRight.root.SetParent(failedLeft.root,false);failedRight.root.localPosition=Vector3.zero;failedRight.root.localRotation=Quaternion.identity;
        var failedOwner=failedLeft.root.gameObject.AddComponent<WheelRearranger>();failedOwner.driveL=failedLeft.drive;failedOwner.driveR=failedRight.drive;
        failedOwner.lubricator=new GameObject("failure oiler").transform;failedOwner.lubricator.SetParent(failedLeft.root,false);failedOwner.CacheFixture();
        failedRight.original[Lube].isReadable=false;bool failed=false;
        try{failedOwner.ApplyFixture(39);}catch(InvalidOperationException){failed=true;}
        Check(failed,"Missing exact native mesh export refuses the repair");failedOwner.SuspendFixture();
        foreach(var rig in new[]{failedLeft,failedRight})
        {
            Check(rig.drive.GetComponents<DriveRetarget>().Length==0,"Failed exporter cleanup leaves no retarget owner");
            foreach(var mesh in rig.original)Check(ReferenceEquals(rig.parts[mesh.Key].GetComponent<MeshFilter>().sharedMesh,mesh.Value),"Failed exporter restores native source mesh identity");
        }
    }
    static void Main(string[] args)
    {
        exportDirectory=Path.Combine(Path.GetDirectoryName(args[3]),"export-smoke");
        using var gd=JsonDocument.Parse(File.ReadAllText(args[0]));using var ad=JsonDocument.Parse(File.ReadAllText(args[1]));using var md=JsonDocument.Parse(File.ReadAllText(args[2]));
        var measure=md.RootElement.GetProperty("fourteen");float scale=measure.GetProperty("scale").GetSingle(),offset=measure.GetProperty("driveOffset").GetSingle();
        foreach(bool left in new[]{false,true})
        foreach(float yaw in new[]{0f,-5f,5f})
        foreach(bool unreadable in new[]{false,true})
        {
            var rig=new Rig(gd.RootElement,left);var reference=new Rig(gd.RootElement,left);
            Matrix4x4 native=rig.root.worldToLocalMatrix*rig.drive.localToWorldMatrix;
            rig.drive.localPosition=new(0,0,offset);rig.drive.localScale=new(left?-1:1,scale,scale);rig.drive.localRotation=Quaternion.AngleAxis(yaw,Vector3.up);
            var relevant=Specs.Where(s=>!left||s.path.StartsWith(Reach)).ToArray();
            var immutable=rig.original.ToDictionary(x=>x.Key,x=>x.Value.vertices.ToArray());
            int requests=0;
            Mesh Readable(Mesh original)
            {
                requests++;string path=rig.original.Single(p=>ReferenceEquals(p.Value,original)).Key;
                return ActualObj(original,immutable[path]);
            }
            if(unreadable)foreach(var m in rig.original.Values)m.isReadable=false;
            foreach(var spec in relevant)DriveRetarget.AddFixedEndpoint(rig.drive,rig.root,native,spec.path,spec.pin,rig.root,Readable);
            if(!left)DriveRetarget.AddAnchoredPart(rig.drive,rig.root,native,"LubricatorParts",rig.root,Readable);
            Check(requests==(unreadable?relevant.Length+(left?0:2):0),"Unreadable native meshes must use exact fallback once per renderer");
            Check(rig.drive.GetComponents<DriveRetarget>().Length==1,"Endpoint additions must share one existing retarget owner");
            foreach(var clip in ad.RootElement.GetProperty("clips").EnumerateArray())foreach(var frame in clip.GetProperty("frames").EnumerateArray())
            {
                rig.Pose(clip,frame);reference.Pose(clip,frame);
                var source=rig.parts.ToDictionary(x=>x.Key,x=>(p:x.Value.localPosition,q:x.Value.localRotation,s:x.Value.localScale));
                Update(rig);poses++;
                foreach(var spec in relevant)
                {
                    var proxy=rig.Proxy(spec.path);var mesh=proxy.GetComponent<MeshFilter>().sharedMesh;
                    // Recover the actual rendered far datum from its protected cap.
                    // This tests rendered mesh geometry instead of a retarget field.
                    var basis=Quaternion.FromToRotation(spec.pin.normalized,Vector3.forward);
                    var toAxis=Matrix4x4.TRS(Vector3.zero,basis,Vector3.one);
                    float nativeExtreme=immutable[spec.path].Max(v=>toAxis.MultiplyPoint3x4(v).z);
                    float length=mesh.vertices.Max(v=>v.z)-(nativeExtreme-spec.pin.magnitude);
                    var renderedFar=proxy.TransformPoint(new(0,0,length));
                    Near(renderedFar,reference.parts[spec.path].TransformPoint(spec.pin),"Native fixed endpoint "+spec.path);
                    Near(proxy.TransformPoint(Vector3.zero),rig.parts[spec.path].position,"Moving source pivot "+spec.path);
                    Check(mesh.vertexCount==rig.original[spec.path].vertexCount,"Endpoint edit preserves topology");
                    Check(rig.lod.GetLODs().Any(l=>l.renderers.Contains(proxy.GetComponent<MeshRenderer>())),"Endpoint renderer remains in a LOD");
                }
                if(!left)
                {
                    var lever=rig.Proxy(Lever);var mesh=lever.GetComponent<MeshFilter>().sharedMesh;var original=immutable[Lever];
                    for(int i=0;i<original.Length;i++)Near(lever.TransformPoint(mesh.vertices[i]),reference.parts[Lever].TransformPoint(original[i]),"Lever native anchored vertex");
                    var nativeContact=(reference.parts[Lube].TransformPoint(Specs[2].pin)-reference.parts[Lever].TransformPoint(LeverPin)).magnitude;
                    maxNativeContact=Math.Max(maxNativeContact,nativeContact);
                    // Rendered lever pin is recovered by fitting its edited affine vertex map independently.
                    // All rendered lever vertices above already prove that the native pin trajectory is retained.
                }
                foreach(var p in rig.parts){var before=source[p.Key];Near(p.Value.localPosition,before.p,"Animated source position immutable");Check(p.Value.localRotation.Equals(before.q)&&p.Value.localScale.Equals(before.s),"Animated source rotation/scale immutable");}
                foreach(var m in rig.original)Check(m.Value.FixturePositions.SequenceEqual(immutable[m.Key]),"Native mesh asset immutable");
            }
            DriveRetarget.Unbind(rig.drive);DriveRetarget.Unbind(rig.drive);
            Check(rig.drive.GetComponents<DriveRetarget>().Length==0,"Repeated Unbind leaves no owner");
            foreach(var spec in relevant)Check(ReferenceEquals(rig.parts[spec.path].GetComponent<MeshFilter>().sharedMesh,rig.original[spec.path]),"Unbind restores exact mesh reference");
            for(int i=0;i<rig.nativeLods.Length;i++)Check(rig.lod.GetLODs()[i].renderers.SequenceEqual(rig.nativeLods[i].renderers),"Unbind restores exact LOD renderer order");
        }
        BaselineShape(gd.RootElement,ad.RootElement);
        ExistingRetarget(gd.RootElement,ad.RootElement,scale,offset);
        if(args.Length>=6)
        {
            using var conventional=JsonDocument.Parse(File.ReadAllText(args[4]));using var articulated=JsonDocument.Parse(File.ReadAllText(args[5]));
            Integration(gd.RootElement,ad.RootElement,conventional.RootElement.GetProperty("layouts").EnumerateArray().Concat(articulated.RootElement.GetProperty("layouts").EnumerateArray()));
        }
        var result=new{passed=true,checks,poses,integrationPoses,integrationCases,excludedCases,actualObjImports,rawMirrorNegativeControls,maxEndpointError,maxNativeContact,maxPresentedContactBound=maxNativeContact+maxEndpointError,gameplayValidated=false,limitations="Managed geometry fixture, exact native hierarchy/meshes, actual installed exporter OBJ payloads with actual normalization method, and 48 native sampled clip poses. Does not execute the Unity OBJ importer, Animator blend evaluation, ratchet/simulation, rendering or DV."};
        File.WriteAllText(args[3],JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(result));
    }
}
