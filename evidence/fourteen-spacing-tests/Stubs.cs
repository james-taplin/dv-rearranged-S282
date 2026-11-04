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
        public T GetComponentInParent<T>() where T:class { for(var t=transform;t!=null;t=t.parent){var found=t.GetComponent<T>();if(found!=null)return found;}return null; }
    }
    public class MonoBehaviour : Component { }
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
        public Vector3 TransformPoint(Vector3 p) => localToWorldMatrix.MultiplyPoint3x4(p);
        public Vector3 InverseTransformPoint(Vector3 p) => worldToLocalMatrix.MultiplyPoint3x4(p);
        public Vector3 TransformVector(Vector3 p) => localToWorldMatrix.MultiplyVector(p);
        public Vector3 TransformVector(float x,float y,float z) => TransformVector(new Vector3(x,y,z));
        public Vector3 InverseTransformVector(Vector3 p) => worldToLocalMatrix.MultiplyVector(p);
        public void SetParent(Transform t, bool worldPositionStays) { if (worldPositionStays) throw new NotSupportedException(); parent?.children.Remove(this); parent = t; t?.children.Add(this); }
        public bool IsChildOf(Transform t) { for(var p=this;p!=null;p=p.parent) if(ReferenceEquals(p,t)) return true; return false; }
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
        public Vector3 normalized=>this/magnitude;
        public static float Dot(Vector3 a,Vector3 b)=>a.x*b.x+a.y*b.y+a.z*b.z;
        public static Vector3 Min(Vector3 a,Vector3 b)=>new(Math.Min(a.x,b.x),Math.Min(a.y,b.y),Math.Min(a.z,b.z));
        public static Vector3 Max(Vector3 a,Vector3 b)=>new(Math.Max(a.x,b.x),Math.Max(a.y,b.y),Math.Max(a.z,b.z));
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
        public N.Quaternion Native => new(x,y,z,w);
        public static Quaternion From(N.Quaternion q) => new(q.X,q.Y,q.Z,q.W);
        public static Quaternion Inverse(Quaternion q) => From(N.Quaternion.Inverse(q.Native));
        public static Quaternion operator *(Quaternion a,Quaternion b) => From(a.Native*b.Native);
        public static Vector3 operator *(Quaternion q,Vector3 v) => Vector3.From(N.Vector3.Transform(v.Native,q.Native));
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
        public static Matrix4x4 identity => new(){n=N.Matrix4x4.Identity};
        public static Matrix4x4 TRS(Vector3 p,Quaternion q,Vector3 s) => new(){n=N.Matrix4x4.CreateScale(s.Native)*N.Matrix4x4.CreateFromQuaternion(q.Native)*N.Matrix4x4.CreateTranslation(p.Native)};
        public static Matrix4x4 operator *(Matrix4x4 a,Matrix4x4 b) => new(){n=b.n*a.n};
        public Vector3 MultiplyPoint3x4(Vector3 p) => Vector3.From(N.Vector3.Transform(p.Native,n));
        public Vector3 MultiplyVector(Vector3 p) => Vector3.From(N.Vector3.TransformNormal(p.Native,n));
        public Matrix4x4 inverse { get { if(!N.Matrix4x4.Invert(n,out var m))throw new InvalidOperationException();return new(){n=m}; } }
    }
    public static class Mathf { public static float Abs(float v) => Math.Abs(v); public static float Min(float a,float b) => Math.Min(a,b);public static float Max(float a,float b)=>Math.Max(a,b);public static float Sign(float v)=>v>=0?1:-1;public static float Clamp01(float v)=>Math.Clamp(v,0,1); }
    public struct Bounds { public Vector3 min,max;public Vector3 center=>(min+max)*.5f; }
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
        public object[] sharedMaterials=Array.Empty<object>();
        public int shadowCastingMode,lightProbeUsage,reflectionProbeUsage;
    }
    public sealed class MeshRenderer : Renderer { }
    public struct LOD
    {
        public float screenRelativeTransitionHeight,fadeTransitionWidth;
        public Renderer[] renderers;
        public LOD(float height,Renderer[] renderers){screenRelativeTransitionHeight=height;this.renderers=renderers;fadeTransitionWidth=0;}
    }
    public sealed class LODGroup : Component { LOD[] lods=Array.Empty<LOD>(); public Vector3 localReferencePoint;public float size=1f;public LOD[] GetLODs()=>lods;public void SetLODs(LOD[] value)=>lods=value;public void RecalculateBounds()=>throw new InvalidOperationException("Legacy retarget must retain its existing LOD bounds policy"); }
}
namespace Dummiesman { public sealed class OBJLoader { public UnityEngine.Mesh Load(string path)=>throw new InvalidOperationException("Unexpected fallback file IO in readable native-mesh fixture"); } }
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
namespace RearrangedS282 { internal sealed class FixtureSettings { internal bool nearVanillaCylinderCasings=true; } }
