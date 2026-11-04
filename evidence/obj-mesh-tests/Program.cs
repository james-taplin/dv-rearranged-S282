using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dummiesman;
using UnityEngine;

namespace UnityEngine {
 public struct Vector3 { public float x,y,z; public Vector3(float x,float y,float z){this.x=x;this.y=y;this.z=z;} public static Vector3 zero=>default; }
 public struct Vector2 { public float x,y; public Vector2(float x,float y){this.x=x;this.y=y;} public static Vector2 zero=>default; public static implicit operator Vector2(Vector3 v)=>new(v.x,v.y); }
 public static class Mathf { public static float Pow(float x,float y)=>MathF.Pow(x,y); }
 public class Material { public static int Created; public string name; public Material(){Created++;} }
 public class GameObject { public static int Created; public Transform transform=new(); public GameObject(string name){Created++;} public T AddComponent<T>() where T:new()=>new(); public void SetActive(bool active){} }
 public class Transform {public Vector3 localScale;}
 public class MeshRenderer {public Material[] sharedMaterials;}
 public class MeshFilter {public Mesh sharedMesh;}
 public class Mesh {
  public string name; public Rendering.IndexFormat indexFormat; public int subMeshCount; public List<Vector3> verts,normals; public List<Vector2> uvs; public readonly Dictionary<int,int[]> triangles=new(); public bool recalculated;
  public void SetVertices(List<Vector3> v)=>verts=v.ToList(); public void SetNormals(List<Vector3> v)=>normals=v.ToList(); public void SetUVs(int channel,List<Vector2> v)=>uvs=v.ToList();
  public void SetTriangles(List<int> values,int submesh)=>triangles[submesh]=values.ToArray(); public void RecalculateNormals()=>recalculated=true; public void RecalculateTangents(){} public void RecalculateBounds(){}
 }
}
namespace UnityEngine.Rendering { public enum IndexFormat { UInt16,UInt32 } }
namespace Dummiesman { public class MTLLoader { public static int Loads; public Dictionary<string,Material> Load(object source){Loads++;return new();} } public static class OBJLoaderHelper { public static Material CreateNullMaterial()=>new(); } }

class Program {
 static int checks;
 static void Check(bool value,string label){if(!value)throw new Exception(label);checks++;}
 const string Vertices="v 1 0 0\nv 0 1 0\nv 0 0 1\nv 1 1 0\n";
 static Mesh Load(string source)=>new OBJLoader().Load(new MemoryStream(Encoding.UTF8.GetBytes(source)));
 static void Main(string[] args){
  if(args.Length>0 && (args[0]=="--snapshot" || args[0]=="--snapshot-file")){
   var source=args[0]=="--snapshot-file"?File.ReadAllText(args[1]):Vertices+"vt 0.1 0.2\nvt 0.3 0.4\nvt 0.5 0.6\nvn 0 0 1\nusemtl red\nf 1/1/1 2/2/1 3/3/1\nusemtl blue\nf 1/1/1 3/3/1 4/2/1\nusemtl red\nf 1/1/1 2/2/1 3/3/1 4/2/1\n";
   var mesh=Load(source);
   Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new{mesh.verts,mesh.normals,mesh.uvs,mesh.subMeshCount,mesh.triangles,mesh.recalculated},new System.Text.Json.JsonSerializerOptions{IncludeFields=true}));return;
  }
  var a=Load(Vertices+"f 1 2 3\n");var b=Load(Vertices+"f 1 2 3");var relative=Load(Vertices+"f -4 -3 -2");
  Check(a.verts.Count==3 && a.verts[0].x==1 && a.verts[1].y==1 && a.verts[2].z==1,"positive indices retain vertex positions");
  Check(a.verts.SequenceEqual(b.verts)&&a.verts.SequenceEqual(relative.verts),"relative and no-final-newline faces match positive indices");
  Check(a.triangles[0].SequenceEqual(new[]{0,1,2})&&b.triangles[0].SequenceEqual(a.triangles[0]),"triangle topology preserved at EOF");
  Check(GameObject.Created==0 && Material.Created==0,"mesh-only imports create no GameObjects or materials");
  var attrs=Load(Vertices+"vt 0.1 0.2\nvt 0.3 0.4\nvt 0.5 0.6\nvn 0 0 1\nusemtl red\nf -4/-3/-1 -3/-2/-1 -2/-1/-1\nusemtl blue\nf 1/1/1 3/3/1 4/2/1\nusemtl red\nf 1/1/1 2/2/1 4/2/1\n");
  Check(attrs.subMeshCount==2 && attrs.triangles[0].Length==6 && attrs.triangles[1].Length==3,"material group ordering and repeated-group topology preserved");
  Check(Math.Abs(attrs.uvs[0].x-.1)<.0001 && Math.Abs(attrs.uvs[0].y-.2)<.0001 && attrs.normals[0].z==1 && !attrs.recalculated,"relative UV/normal indices preserve vertex attributes");
  var quad=Load(Vertices+"f 1 2 3 4 # trailing comment");
  Check(quad.triangles[0].SequenceEqual(new[]{0,1,2,2,3,0})&&quad.recalculated,"quad triangulation and missing-normal recalculation preserved");
  foreach(string invalid in new[]{"f 1 2 x","f 1 2 0","f 1 2 5","f 1 2 -5","f 1 2 3/","f 1 2 3//","f 1 2 3/a","f 1 2 3/1/1/1","f 1 2","f 1 2 --3","f 1 2 2147483648"}){
   bool failed=false;try{Load(Vertices+invalid);}catch(FormatException){failed=true;}Check(failed,"malformed or out-of-range token rejected: "+invalid);
  }
  bool longRejected=false;try{Load(Vertices+"f 1 2 "+new string('9',5000));}catch(InvalidDataException){longRejected=true;}Check(longRejected,"oversized token terminates with bounded error");
  var mtl=Load("mtllib nonexistent.mtl\n"+Vertices+"usemtl a\nf 1 2 3\n");Check(mtl!=null&&MTLLoader.Loads==0&&Material.Created==0,"mesh-only path does not load MTL or allocate materials");
  var loader=new OBJLoader();using var obj=new MemoryStream(Encoding.UTF8.GetBytes(Vertices+"f 1 2 3"));using var material=new MemoryStream();Check(loader.Load(obj,material)!=null&&MTLLoader.Loads==0,"material-stream overload remains mesh-only");
  Console.WriteLine("PASS "+checks+" mesh-only importer checks");
 }
}
