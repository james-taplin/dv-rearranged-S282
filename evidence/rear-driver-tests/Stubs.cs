// Managed geometry/ownership stand-ins, not a Unity physics or player simulation.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using N = System.Numerics;
namespace UnityEngine
{
    public class Object
    {
        public bool destroyed;
        public virtual string name { get; set; }
        public static Mesh Instantiate(Mesh source) => source.Clone();
        public static GameObject Instantiate(GameObject source,Transform parent){
            GameObject Copy(GameObject from,Transform p){
                var to=new GameObject(from.name);to.transform.SetParent(p,false);to.transform.localPosition=from.transform.localPosition;to.transform.localRotation=from.transform.localRotation;to.transform.localScale=from.transform.localScale;
                if(from.GetComponent<Animator>()!=null)to.AddComponent<Animator>();
                foreach(var c in from.GetComponents<MonoBehaviour>()){
                    if(c is RearrangedS282.PoweredWheelRotater f){var n=to.AddComponent<RearrangedS282.PoweredWheelRotater>();n.Init(f.Source);n.enabled=f.enabled;}
                    else to.AddComponent<FixtureOilReader>();
                }
                foreach(Transform child in from.transform)Copy(child.gameObject,to.transform);
                return to;
            }
            return Copy(source,parent);
        }
        public static void Destroy(Object value)
        {
            if (value == null) return;
            value.destroyed = true;
            if (value is Component c) c.gameObject.components.Remove(c);
            if (value is GameObject go) { go.activeSelf = false; go.transform.parent?.children.Remove(go.transform); }
        }
        public static void DestroyImmediate(Object value) => Destroy(value);
    }
    public class Component : Object
    {
        public GameObject gameObject;
        public Transform transform => this as Transform ?? gameObject.transform;
        public override string name { get => gameObject.name; set => gameObject.name = value; }
        public T GetComponent<T>() where T : class => gameObject.GetComponent<T>();
        public T[] GetComponents<T>() where T : class => gameObject.GetComponents<T>();
        public T[] GetComponentsInChildren<T>(bool inactive) where T : class => gameObject.GetComponentsInChildren<T>(inactive);
        public T GetComponentInChildren<T>(bool inactive=false) where T:class =>GetComponentsInChildren<T>(inactive).FirstOrDefault();
        public T GetComponentInParent<T>() where T:class { for(var t=transform;t!=null;t=t.parent){var found=t.GetComponent<T>();if(found!=null)return found;}return null; }
    }
    public sealed class DefaultExecutionOrder : Attribute { public DefaultExecutionOrder(int n){} }
    public class Material : Object {}
    public class MonoBehaviour : Component { public bool enabled=true; }
    public sealed class GameObject : Object
    {
        public int layer;
        public bool activeSelf = true;
        public bool activeInHierarchy=>activeSelf && (transform.parent==null || transform.parent.gameObject.activeInHierarchy);
        public readonly Transform transform;
        internal readonly List<Component> components = new();
        public GameObject(string name) { this.name = name; transform = new Transform { gameObject = this }; components.Add(transform); }
        public void SetActive(bool active) => activeSelf = active;
        public T AddComponent<T>() where T : Component, new() { var c = new T { gameObject = this }; components.Add(c); return c; }
        public T GetComponent<T>() where T : class => components.OfType<T>().FirstOrDefault();
        public T[] GetComponents<T>() where T : class => components.OfType<T>().ToArray();
        internal MeshFilter filter=>GetComponent<MeshFilter>();
        public T[] GetComponentsInChildren<T>(bool inactive) where T : class =>
            All(transform).Where(t => inactive || t.gameObject.activeInHierarchy).SelectMany(t => t.gameObject.components.OfType<T>()).ToArray();
        static IEnumerable<Transform> All(Transform t) { yield return t; foreach (var child in t.children) foreach (var x in All(child)) yield return x; }
    }
    public sealed class Transform : Component, IEnumerable<Transform>
    {
        public Transform parent;
        public readonly List<Transform> children = new();
        public Vector3 localPosition = Vector3.zero, localScale = Vector3.one;
        public Quaternion localRotation = Quaternion.identity;
        public Matrix4x4 localToWorldMatrix => (parent == null ? Matrix4x4.identity : parent.localToWorldMatrix) * Matrix4x4.TRS(localPosition, localRotation, localScale);
        public Matrix4x4 worldToLocalMatrix => localToWorldMatrix.inverse;
        public Vector3 position { get=>TransformPoint(Vector3.zero);set=>localPosition=parent==null?value:parent.InverseTransformPoint(value); }
        public int childCount=>children.Count; public Transform GetChild(int i)=>children[i];
        public Quaternion rotation { get=>parent==null?localRotation:parent.rotation*localRotation;set=>localRotation=parent==null?value:Quaternion.Inverse(parent.rotation)*value;}
        public Vector3 forward=>TransformDirection(Vector3.forward);
        public Vector3 TransformDirection(Vector3 v)=>rotation*v; public Vector3 InverseTransformDirection(Vector3 v)=>Quaternion.Inverse(rotation)*v;
        public Vector3 TransformPoint(Vector3 p) => localToWorldMatrix.MultiplyPoint3x4(p);
        public Vector3 InverseTransformPoint(Vector3 p) => worldToLocalMatrix.MultiplyPoint3x4(p);
        public Vector3 TransformVector(Vector3 p) => localToWorldMatrix.MultiplyVector(p);
        public Vector3 TransformVector(float x,float y,float z) => TransformVector(new Vector3(x,y,z));
        public Vector3 InverseTransformVector(Vector3 p) => worldToLocalMatrix.MultiplyVector(p);
        public void SetParent(Transform t, bool worldPositionStays) { if (worldPositionStays) throw new NotSupportedException(); parent?.children.Remove(this); parent = t; t?.children.Add(this); }
        public Transform Find(string path) { var t = this; foreach (string part in path.Split('/')) { t = t.children.FirstOrDefault(c => c.name == part); if (t == null) return null; } return t; }
        public Transform Add(string name){var t=new GameObject(name).transform;t.SetParent(this,false);return t;}
        public IEnumerator<Transform> GetEnumerator() => children.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x=x; this.y=y; this.z=z; }
        public static Vector3 zero => new(); public static Vector3 one => new(1,1,1);
        public static Vector3 forward => new(0,0,1); public static Vector3 up => new(0,1,0);public static Vector3 right=>new(1,0,0);
        public static Vector3 operator -(Vector3 a)=>new(-a.x,-a.y,-a.z); public static Vector3 operator *(float b,Vector3 a)=>a*b; public static Vector3 Scale(Vector3 a,Vector3 b)=>new(a.x*b.x,a.y*b.y,a.z*b.z);
        public Vector3 normalized=>this/magnitude;
        public static float Dot(Vector3 a,Vector3 b)=>a.x*b.x+a.y*b.y+a.z*b.z;
        public static Vector3 Cross(Vector3 a,Vector3 b)=>new(a.y*b.z-a.z*b.y,a.z*b.x-a.x*b.z,a.x*b.y-a.y*b.x);
        public float magnitude => (float)Math.Sqrt(x*x+y*y+z*z);
        public N.Vector3 Native => new(x,y,z);
        public static Vector3 From(N.Vector3 v) => new(v.X,v.Y,v.Z);
        public static Vector3 operator +(Vector3 a,Vector3 b) => new(a.x+b.x,a.y+b.y,a.z+b.z);
        public static Vector3 operator -(Vector3 a,Vector3 b) => new(a.x-b.x,a.y-b.y,a.z-b.z);
        public static Vector3 operator *(Vector3 a,float b) => new(a.x*b,a.y*b,a.z*b);
        public static Vector3 operator /(Vector3 a,float b) => a*(1/b);
    }
    public struct Quaternion
    {
        public float x,y,z,w;
        public Quaternion(float x,float y,float z,float w) { this.x=x;this.y=y;this.z=z;this.w=w; }
        public static Quaternion identity => new(0,0,0,1);
        public static Quaternion Inverse(Quaternion q)=>From(N.Quaternion.Inverse(q.Native));
        public static Quaternion operator *(Quaternion a,Quaternion b)=>From(a.Native*b.Native);
        public static Vector3 operator *(Quaternion q,Vector3 p)=>Vector3.From(N.Vector3.Transform(p.Native,q.Native));
        public static Quaternion Euler(float x,float y,float z)=>From(N.Quaternion.CreateFromYawPitchRoll(y*(float)Math.PI/180,x*(float)Math.PI/180,z*(float)Math.PI/180));
        public N.Quaternion Native => new(x,y,z,w);
        public static Quaternion From(N.Quaternion q) => new(q.X,q.Y,q.Z,q.W);
        public static Quaternion AngleAxis(float degrees,Vector3 axis) => From(N.Quaternion.CreateFromAxisAngle(N.Vector3.Normalize(axis.Native),degrees*(float)Math.PI/180));
        public static Quaternion FromToRotation(Vector3 from,Vector3 to)
        {
            var a=N.Vector3.Normalize(from.Native);var b=N.Vector3.Normalize(to.Native);float dot=N.Vector3.Dot(a,b);
            if(dot < -.999999f) return AngleAxis(180,new Vector3(0,1,0));
            var cross=N.Vector3.Cross(a,b);return From(N.Quaternion.Normalize(new N.Quaternion(cross,1+dot)));
        }
        public static Quaternion LookRotation(Vector3 z,Vector3 y)
        { z=z.normalized;var x=Vector3.Cross(y,z).normalized;y=Vector3.Cross(z,x);return From(N.Quaternion.CreateFromRotationMatrix(new N.Matrix4x4(x.x,x.y,x.z,0,y.x,y.y,y.z,0,z.x,z.y,z.z,0,0,0,0,1))); }
    }
    public struct Matrix4x4
    {
        internal N.Matrix4x4 n;
        public float this[int i]=>i switch {0=>n.M11,1=>n.M12,2=>n.M13,3=>n.M14,4=>n.M21,5=>n.M22,6=>n.M23,7=>n.M24,8=>n.M31,9=>n.M32,10=>n.M33,11=>n.M34,12=>n.M41,13=>n.M42,14=>n.M43,_=>n.M44};
        public static Matrix4x4 identity => new(){n=N.Matrix4x4.Identity};
        public static Matrix4x4 TRS(Vector3 p,Quaternion q,Vector3 s) => new(){n=N.Matrix4x4.CreateScale(s.Native)*N.Matrix4x4.CreateFromQuaternion(q.Native)*N.Matrix4x4.CreateTranslation(p.Native)};
        public static Matrix4x4 operator *(Matrix4x4 a,Matrix4x4 b) => new(){n=b.n*a.n};
        public Vector3 MultiplyPoint3x4(Vector3 p) => Vector3.From(N.Vector3.Transform(p.Native,n));
        public Vector3 MultiplyVector(Vector3 p) => Vector3.From(N.Vector3.TransformNormal(p.Native,n));
        public Matrix4x4 inverse { get { if(!N.Matrix4x4.Invert(n,out var m))throw new InvalidOperationException();return new(){n=m}; } }
    }
    public static class Mathf { public static float Clamp(float v,float lo,float hi)=>Math.Clamp(v,lo,hi); public static float Abs(float v) => Math.Abs(v); public static float Min(float a,float b) => Math.Min(a,b);public static float Max(float a,float b)=>Math.Max(a,b);public static float Sign(float v)=>v>=0?1:-1;public static float Clamp01(float v)=>Math.Clamp(v,0,1); }
    public struct Bounds { public Vector3 min,max;public Vector3 center=>(min+max)*.5f;public Vector3 size=>max-min; public Bounds(Vector3 c,Vector3 s){min=c-s*.5f;max=c+s*.5f;} public void Encapsulate(Bounds b){min=new(Math.Min(min.x,b.min.x),Math.Min(min.y,b.min.y),Math.Min(min.z,b.min.z));max=new(Math.Max(max.x,b.max.x),Math.Max(max.y,b.max.y),Math.Max(max.z,b.max.z));} public void Expand(float n){min-=Vector3.one*(n*.5f);max+=Vector3.one*(n*.5f);} }
    public sealed class Mesh : Object
    {
        public bool isReadable=true;
        public Vector3[] vertices=Array.Empty<Vector3>();
        public float[] uv=Array.Empty<float>();public int[] triangles=Array.Empty<int>();
        public float[] colors=Array.Empty<float>(),tangents=Array.Empty<float>();public int subMeshCount=1;
        public Bounds bounds;
        public int vertexCount=>vertices.Length;
        public Mesh Clone()=>new(){name=name,isReadable=isReadable,vertices=vertices.ToArray(),bounds=bounds,uv=uv.ToArray(),triangles=triangles.ToArray(),colors=colors.ToArray(),tangents=tangents.ToArray(),subMeshCount=subMeshCount};
        public void RecalculateBounds() { bounds=new Bounds{min=new(vertices.Min(v=>v.x),vertices.Min(v=>v.y),vertices.Min(v=>v.z)),max=new(vertices.Max(v=>v.x),vertices.Max(v=>v.y),vertices.Max(v=>v.z))}; }
        public void RecalculateNormals(){}public void RecalculateTangents(){}
    }
    public sealed class MeshFilter : Component { public Mesh sharedMesh;public MeshFilter(){}public MeshFilter(Transform t,Mesh mesh){gameObject=t.gameObject;gameObject.components.Add(this);sharedMesh=mesh;} }
    public sealed class ParticleSystem:Component {public ParticleSystem(){}public ParticleSystem(Transform t){gameObject=t.gameObject;gameObject.components.Add(this);} }
    public class Renderer : Component
    {
        public bool enabled=true,receiveShadows=true;
        public Material[] sharedMaterials=Array.Empty<Material>();
        public int shadowCastingMode,lightProbeUsage,reflectionProbeUsage;
    }
    public sealed class MeshRenderer : Renderer { }
    public struct LOD
    {
        public float screenRelativeTransitionHeight,fadeTransitionWidth;
        public Renderer[] renderers;
        public LOD(float height,Renderer[] renderers){screenRelativeTransitionHeight=height;this.renderers=renderers;fadeTransitionWidth=0;}
    }
    public sealed class LODGroup : Component {
        LOD[] lods=Array.Empty<LOD>();public Vector3 localReferencePoint; public float size;public int recalculations;
        public LOD[] GetLODs()=>lods;public void SetLODs(LOD[] value)=>lods=value;
        public void RecalculateBounds(){
            recalculations++;var points=lods.SelectMany(l=>l.renderers).Distinct().SelectMany(r=>{
                var m=r.GetComponent<MeshFilter>()?.sharedMesh;
                return m==null?Array.Empty<Vector3>():m.vertices.Select(v=>transform.InverseTransformPoint(r.transform.TransformPoint(v))).ToArray();
            }).ToArray();
            if(points.Length==0)return;
            var lo=new Vector3(points.Min(v=>v.x),points.Min(v=>v.y),points.Min(v=>v.z));
            var hi=new Vector3(points.Max(v=>v.x),points.Max(v=>v.y),points.Max(v=>v.z));
            localReferencePoint=(lo+hi)*.5f;size=Math.Max(hi.x-lo.x,Math.Max(hi.y-lo.y,hi.z-lo.z));
        }
    }
}
public sealed class TrainCar:UnityEngine.MonoBehaviour{}
namespace Dummiesman { public sealed class OBJLoader { public static readonly Dictionary<string,UnityEngine.Mesh> Assets=new(); public UnityEngine.Mesh Load(string path)=>Assets[System.IO.Path.GetFileName(path)]; } }

namespace RearrangedS282
{
    internal static class Main
    {
        internal static string ModPath="fixture-no-asset-folder";
        internal static readonly LoggerStub Logger=new();
        internal static readonly FixtureSettings settings=new();
        internal sealed class LoggerStub { internal void Warning(string text) { } internal void Log(string text){} }
    }
}
namespace DV.Util
{
    public sealed class PositionSyncProvider:UnityEngine.Component { public string syncTag; }
    public sealed class PositionSyncConsumer:UnityEngine.Component
    {
        public UnityEngine.Transform providerTransform;public string syncTag;public int syncs;
        public void SetProviderTransform(PositionSyncProvider p)=>providerTransform=p.transform;
        public void Sync(){transform.position=providerTransform.position;syncs++;}
    }
}
namespace RearrangedS282 { internal sealed class FixtureSettings { internal bool nearVanillaCylinderCasings=true; internal float prairieExtraCoupledPitchMetres,americanExtraCoupledPitchMetres; internal float mogulExtraCoupledPitchMetres=.15f,twoFortyExtraCoupledPitchMetres=.15f,giant222DriverDiameterMetres=2.8f; } }

namespace UnityEngine {
    public sealed class Collider:Component{} public sealed class Rigidbody:Component{}
    public sealed class Animator:Component{}
}
namespace DV.Wheels { public sealed class PoweredWheel:UnityEngine.Component{public UnityEngine.Transform wheelTransform;} }
namespace LocoSim.Implementations.Wheels{}

namespace UnityEngine {public sealed class FixtureOilReader:MonoBehaviour{}}
namespace RearrangedS282 {internal sealed class MalletAnimatorMirror:UnityEngine.MonoBehaviour{internal UnityEngine.Animator Source,Target;}}
