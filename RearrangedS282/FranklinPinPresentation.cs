#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using Object=UnityEngine.Object;

namespace RearrangedS282
{
    // Existing native animation owns both clocks. The hand-authored Franklin
    // coupling path is not exactly the wheel rim's circular path; position the
    // existing native pins from that coupling bone and retain wheel rotation.
    // No source Animator, wheel transform or simulation port is rewritten.
    internal static class FranklinPinPresentation
    {
        // Independently measured inner-eye centre of the one canonical generated
        // two-axle rod (both source recipes leave this same referenced geometry).
        private const float CoupledFrontEyeZ=1.69832516f;

        private sealed class Datum
        {
            internal Vector2 Centre;
            internal float BaseX,Radius;
        }
        private sealed class Lease:IDisposable
        {
            internal readonly List<(MeshFilter filter,Mesh original)> Filters=new List<(MeshFilter,Mesh)>();
            internal readonly List<Mesh> Meshes=new List<Mesh>();
            internal readonly List<GameObject> Objects=new List<GameObject>();
            internal readonly List<(LODGroup group,LOD[] lods,Vector3 reference,float size)> Lods=new List<(LODGroup,LOD[],Vector3,float)>();
            internal Action<IEnumerable<Renderer>> Retire;
            private bool disposed;
            public void Dispose()
            {
                if(disposed)return;disposed=true;
                // Forget while the actual renderers are still alive. Destroy is
                // deferred, so a same-frame hierarchy scan can still find them.
                Retire?.Invoke(Objects.Where(item=>item!=null).SelectMany(item=>item.GetComponentsInChildren<Renderer>(true)).ToArray());
                foreach(var item in Lods)if(item.group!=null){item.group.SetLODs(item.lods);item.group.localReferencePoint=item.reference;item.group.size=item.size;}
                foreach(var item in Filters)if(item.filter!=null)item.filter.sharedMesh=item.original;
                foreach(var item in Objects)if(item!=null){item.SetActive(false);Object.Destroy(item);}
                foreach(var mesh in Meshes)if(mesh!=null)Object.Destroy(mesh);
                Lods.Clear();Filters.Clear();Objects.Clear();Meshes.Clear();
            }
        }

        internal static IDisposable Bind(Transform drive,Mesh nativeLongDriver,Transform lodRoot,Func<Mesh,Mesh> readableSource)
            => BindWithRetirement(drive,nativeLongDriver,lodRoot,readableSource,null);

        internal static IDisposable BindWithRetirement(Transform drive,Mesh nativeLongDriver,Transform lodRoot,Func<Mesh,Mesh> readableSource,Action<IEnumerable<Renderer>> retire)
        {
            if(drive==null||nativeLongDriver==null)throw new ArgumentNullException();
            var coupling=drive.Find("s282_mech_wheels_connect");
            if(coupling==null)throw new InvalidOperationException("Franklin native coupling bone is missing.");
            var lease=new Lease{Retire=retire};var replacements=new Dictionary<Renderer,List<Renderer>>();
            Mesh longReadable=null;float boundsExtension=.02f;
            try
            {
                longReadable=Read(nativeLongDriver,readableSource);
                var longDatum=Measure(longReadable);
                for(int number=1;number<=2;number++)
                {
                    var wheel=drive.Find("s282_wheels_driving_"+number);
                    if(wheel==null||!wheel.gameObject.activeSelf)continue;
                    var filter=wheel.GetComponent<MeshFilter>();var renderer=wheel.GetComponent<MeshRenderer>();
                    if(filter==null||filter.sharedMesh==null||renderer==null)throw new InvalidOperationException("Franklin driver has no native renderer/mesh.");
                    var original=filter.sharedMesh;var readable=Read(original,readableSource);
                    boundsExtension=Mathf.Max(boundsExtension,Mathf.Max(0,nativeLongDriver.bounds.max.x-original.bounds.max.x)+.02f);
                    try
                    {
                        var datum=Measure(readable);var split=PinTriangles(readable,datum);
                        var body=Object.Instantiate(readable);body.name=original.name+" (Franklin pin separated)";
                        for(int section=0;section<body.subMeshCount;section++)body.SetTriangles(readable.GetTriangles(section).Where((index,i)=>!split[section][i/3]).ToArray(),section);
                        // Preserve the native wheel's conservative bounds, UVs,
                        // material sections and every retained wheel vertex.
                        body.bounds=original.bounds;lease.Meshes.Add(body);lease.Filters.Add((filter,original));filter.sharedMesh=body;

                        var pinSource=number==2?longReadable:readable;var pinDatum=number==2?longDatum:datum;
                        if(pinSource.subMeshCount!=renderer.sharedMaterials.Length)
                            throw new InvalidOperationException("Native Franklin pin material sections differ from the driver.");
                        var pin=ExtractPin(pinSource,pinDatum);lease.Meshes.Add(pin);
                        var marker=new GameObject("[Rearranged pin position] wheel "+number);lease.Objects.Add(marker);
                        marker.transform.SetParent(coupling,false);
                        marker.transform.localPosition=new Vector3(0,0,number==1?CoupledFrontEyeZ:0);
                        var root=new GameObject("[Rearranged native driver pin] wheel "+number);lease.Objects.Add(root);
                        root.layer=wheel.gameObject.layer;root.SetActive(false);root.transform.SetParent(drive,false);
                        var rest=drive.InverseTransformPoint(marker.transform.position);
                        root.transform.localPosition=new Vector3(0,rest.y,rest.z);root.transform.localRotation=wheel.localRotation;
                        root.AddComponent<MeshFilter>().sharedMesh=pin;
                        var shown=root.AddComponent<MeshRenderer>();shown.sharedMaterials=renderer.sharedMaterials;
                        shown.shadowCastingMode=renderer.shadowCastingMode;shown.receiveShadows=renderer.receiveShadows;
                        shown.lightProbeUsage=renderer.lightProbeUsage;shown.reflectionProbeUsage=renderer.reflectionProbeUsage;shown.enabled=renderer.enabled;
                        var position=root.AddComponent<PositionConstraint>();
                        position.translationAtRest=root.transform.localPosition;position.translationOffset=Vector3.zero;
                        // Pin meshes retain native absolute drive-X coordinates.
                        // Follow only the coupling bone's native Y/Z trajectory;
                        // never capture a previous controller's animated X pose.
                        position.translationAxis=Axis.Y|Axis.Z;position.AddSource(new ConstraintSource{sourceTransform=marker.transform,weight=1});
                        position.weight=1;position.locked=true;position.constraintActive=true;
                        var rotation=root.AddComponent<RotationConstraint>();
                        rotation.rotationAtRest=wheel.localEulerAngles;rotation.rotationOffset=Vector3.zero;
                        rotation.rotationAxis=Axis.X|Axis.Y|Axis.Z;rotation.AddSource(new ConstraintSource{sourceTransform=wheel,weight=1});
                        rotation.weight=1;rotation.locked=true;rotation.constraintActive=true;
                        replacements[renderer]=new List<Renderer>{shown};root.SetActive(true);
                    }
                    finally{Object.Destroy(readable);}
                }
                if(lodRoot!=null)foreach(var group in lodRoot.GetComponentsInChildren<LODGroup>(true))
                {
                    var original=group.GetLODs();bool changed=false;
                    var lods=original.Select(l=>new LOD(l.screenRelativeTransitionHeight,l.renderers.SelectMany(r=>
                    {
                        if(r!=null&&replacements.TryGetValue(r,out var pins)){changed=true;return new[]{r}.Concat(pins);}
                        return new[]{r}.AsEnumerable();
                    }).ToArray()){fadeTransitionWidth=l.fadeTransitionWidth}).ToArray();
                    if(!changed)continue;
                    lease.Lods.Add((group,original,group.localReferencePoint,group.size));group.SetLODs(lods);
                    // Native long driver3 provides the same complete pin envelope;
                    // retain the old animation bounds plus its measured extension.
                    group.size+=2*boundsExtension;
                }
                return lease;
            }
            catch{lease.Dispose();throw;}
            finally{if(longReadable!=null)Object.Destroy(longReadable);}
        }

        private static Mesh Read(Mesh source,Func<Mesh,Mesh> provider)
        {
            var copy=source.isReadable?Object.Instantiate(source):provider?.Invoke(source);
            if(copy==null||!copy.isReadable||copy.vertexCount==0)throw new InvalidOperationException("Native Franklin wheel pin needs an exact readable mesh: "+source.name);
            return copy;
        }
        private static Vector3[] Used(Mesh mesh)
        {
            var vertices=mesh.vertices;return Enumerable.Range(0,mesh.subMeshCount).SelectMany(section=>mesh.GetTriangles(section)).Distinct().Select(i=>vertices[i]).ToArray();
        }
        private static Datum Measure(Mesh mesh)
        {
            var used=Used(mesh);
            var rings=used.Where(v=>v.x>.85f).GroupBy(v=>Mathf.RoundToInt(v.x*100000f)).Select(g=>
            {
                var v=g.ToArray();float ymin=v.Min(p=>p.y),ymax=v.Max(p=>p.y),zmin=v.Min(p=>p.z),zmax=v.Max(p=>p.z);
                return new{X=v.Average(p=>p.x),Count=v.Length,Y=(ymin+ymax)*.5f,Z=(zmin+zmax)*.5f,Dy=ymax-ymin,Dz=zmax-zmin};
            }).Where(r=>r.Count>=12&&r.Dy>.10f&&r.Dy<.13f&&r.Dz>.10f&&r.Dz<.13f&&
                r.Y*r.Y+r.Z*r.Z>.04f&&r.Y*r.Y+r.Z*r.Z<.25f).OrderBy(r=>r.X).ToArray();
            if(rings.Length<2)throw new InvalidOperationException("Native Franklin pin cylinder rings were not found: "+mesh.name);
            var first=rings[0];var last=rings[rings.Length-1];
            if(Vector2.Distance(new Vector2(first.Y,first.Z),new Vector2(last.Y,last.Z))>.002f)
                throw new InvalidOperationException("Native Franklin pin cylinder axes differ: "+mesh.name);
            return new Datum{Centre=new Vector2((first.Y+last.Y)*.5f,(first.Z+last.Z)*.5f),BaseX=first.X,Radius=Mathf.Max(first.Dy,first.Dz)*.5f};
        }
        private static bool[][] PinTriangles(Mesh mesh,Datum datum)
        {
            var vertices=mesh.vertices;var inside=vertices.Select(v=>v.x>=datum.BaseX-.00002f&&
                Vector2.Distance(new Vector2(v.y,v.z),datum.Centre)<=datum.Radius+.00005f).ToArray();
            var result=new bool[mesh.subMeshCount][];int count=0;
            for(int section=0;section<mesh.subMeshCount;section++)
            {
                var triangles=mesh.GetTriangles(section);result[section]=new bool[triangles.Length/3];
                for(int i=0;i<triangles.Length;i+=3)if(inside[triangles[i]]&&inside[triangles[i+1]]&&inside[triangles[i+2]]){result[section][i/3]=true;count++;}
            }
            if(count<32)throw new InvalidOperationException("Native Franklin pin triangle section was not isolated: "+mesh.name);
            return result;
        }
        private static Mesh ExtractPin(Mesh source,Datum datum)
        {
            var selected=PinTriangles(source,datum);var sectionTriangles=new int[source.subMeshCount][];
            for(int section=0;section<source.subMeshCount;section++)sectionTriangles[section]=source.GetTriangles(section).Where((v,i)=>selected[section][i/3]).ToArray();
            var indices=sectionTriangles.SelectMany(x=>x).Distinct().ToArray();var map=indices.Select((old,index)=>new{old,index}).ToDictionary(p=>p.old,p=>p.index);
            var oldVertices=source.vertices;var pin=new Mesh{name="Rearranged native Franklin driver pin",indexFormat=source.indexFormat};
            pin.vertices=indices.Select(i=>oldVertices[i]-new Vector3(0,datum.Centre.x,datum.Centre.y)).ToArray();
            var normals=source.normals;if(normals.Length==source.vertexCount)pin.normals=indices.Select(i=>normals[i]).ToArray();
            var tangents=source.tangents;if(tangents.Length==source.vertexCount)pin.tangents=indices.Select(i=>tangents[i]).ToArray();
            var colors=source.colors32;if(colors.Length==source.vertexCount)pin.colors32=indices.Select(i=>colors[i]).ToArray();
            for(int channel=0;channel<8;channel++){var uv=new List<Vector4>();source.GetUVs(channel,uv);if(uv.Count==source.vertexCount)pin.SetUVs(channel,indices.Select(i=>uv[i]).ToList());}
            pin.subMeshCount=source.subMeshCount;
            for(int section=0;section<pin.subMeshCount;section++)pin.SetTriangles(sectionTriangles[section].Select(i=>map[i]).ToArray(),section);
            pin.RecalculateBounds();return pin;
        }
    }
}
#endif
