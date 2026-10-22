using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using RearrangedS282;
class Program {
 static int checks;static void Check(bool value,string label){checks++;if(!value)throw new Exception(label);}
 static Vector3 V(JsonElement a)=>new(a[0].GetSingle(),a[1].GetSingle(),a[2].GetSingle());
 static int[] T(JsonElement a)=>a.GetProperty("triangles").EnumerateArray().SelectMany(f=>f.EnumerateArray().Select(x=>x.GetInt32())).ToArray();
 static Vector3[] P(JsonElement a)=>a.GetProperty("vertices").EnumerateArray().Select(V).ToArray();
 static float Dot(Vector3 a,Vector3 b)=>a.x*b.x+a.y*b.y+a.z*b.z;
 static Vector3 Cross(Vector3 a,Vector3 b)=>new(a.y*b.z-a.z*b.y,a.z*b.x-a.x*b.z,a.x*b.y-a.y*b.x);
 static Vector2[] U(JsonElement a,string path){var source=new Dictionary<int,Vector2>();var uv=new List<Vector2>();foreach(string line in File.ReadLines(path)){var s=line.Split(' ',StringSplitOptions.RemoveEmptyEntries);if(s.Length==0)continue;if(s[0]=="vt")uv.Add(new(float.Parse(s[1],CultureInfo.InvariantCulture),float.Parse(s[2],CultureInfo.InvariantCulture)));if(s[0]=="f")foreach(string face in s.Skip(1)){var x=face.Split('/');int v=int.Parse(x[0])-1,t=int.Parse(x[1])-1;if(!source.ContainsKey(v))source[v]=uv[t];}}return a.GetProperty("sourceVertexIds").EnumerateArray().Select(i=>source[i.GetInt32()]).ToArray();}
 static void Main(string[] args){
  string root=args[0],output=args[1],lms=Path.Combine(args[2],"Mods/LocoMeshSplitter/assets");Directory.CreateDirectory(output);using var fittings=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"evidence/cylinder-casing-tests/native-full-casing-fittings.json")));using var casings=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"evidence/cylinder-casing-tests/native-casings.json")));
  var parts=fittings.RootElement.GetProperty("parts").EnumerateArray().ToArray();var casing=casings.RootElement.GetProperty("sources").EnumerateArray().SelectMany(s=>s.GetProperty("parts").EnumerateArray()).ToArray();var rows=new List<object>();var collars=new List<object>();
  foreach(int lod in new[]{0,1})foreach(string side in new[]{"left","right"}){
   var pipe=parts.Single(p=>p.GetProperty("name").GetString()==$"native-lod{lod}-pipe-{side}");var pv=P(pipe);var pt=T(pipe);var original=pv.ToArray();string obj=Path.Combine(lms,lod==0?"s282_locomotive_body.obj":"s282_locomotive_body_LOD1.obj");var uv=U(pipe,obj);
   var compact=SteamPipeLowerCollar.CompactLowerCollar(pv,pt);Check(!ReferenceEquals(compact,pv)&&compact.Length==pv.Length,"collarprivatecopy");Check(SteamPipeNativeFit.TrySection(pv,pt,null,1.72f,out var axisStart)&&SteamPipeNativeFit.TrySection(pv,pt,null,1.68f,out _),"collaraxis");SteamPipeNativeFit.TrySection(pv,pt,null,1.68f,out var axisEnd);var axis=axisEnd.Centre-axisStart.Centre;axis=axis*(1f/(float)Math.Sqrt(axis.sqrMagnitude));int moved=0;float minimumRatio=1;
   for(int i=0;i<pv.Length;i++){var delta=pv[i]-axisStart.Centre;float along=Dot(delta,axis);var radial=delta-axis*along;float radius=(float)Math.Sqrt(radial.sqrMagnitude);bool same=compact[i].x==pv[i].x&&compact[i].y==pv[i].y&&compact[i].z==pv[i].z;if(!same)moved++;if(radius<=.068f||along<=-.22f||along>=-.09f)Check(same,"shaftandupperflareexactidentity");}
   Check(moved>10,"nativecollarschanged");for(int i=0;i<pt.Length;i+=3){var before=Cross(pv[pt[i+1]]-pv[pt[i]],pv[pt[i+2]]-pv[pt[i]]);var after=Cross(compact[pt[i+1]]-compact[pt[i]],compact[pt[i+2]]-compact[pt[i]]);if(before.sqrMagnitude<1e-15)continue;Check(Dot(before,after)>0,"collarwinding");float ratio=(float)Math.Sqrt(after.sqrMagnitude/before.sqrMagnitude);Check(ratio>.20f,"collarnoncollapse");minimumRatio=Math.Min(minimumRatio,ratio);}
   collars.Add(new{lod,side,moved,minimumRatio,vertices=compact.Select(p=>new[]{p.x,p.y,p.z}).ToArray(),triangles=pt,originalVertices=pv.Select(p=>new[]{p.x,p.y,p.z}).ToArray()});
   if(lod==0){Check(SteamPipeNativeFit.TrySection(pv,pt,uv,2.02f,out var section),"actual 2.02 section");Check(section.Points.Length==35,"actual35corner boundary");Check(Math.Abs(Math.Abs(section.Centre.x)-.809196f)<.00001f,"measured35cornercentre");Check(section.Points.Length==section.UV.Length&&section.UV.Length==section.EdgeEndUV.Length,"orderedUVsizes");Check(section.UV.Any(p=>p.x!=0||p.y!=0),"nativeUVinterpolated");foreach(var p in section.Points)Check(Math.Abs(p.y-2.02f)<.000002f,"horizontalactualboundary");Check(SteamPipeNativeFit.TrySection(pv,pt,uv,1.95f,out var main),"splitmainsection");Check(main.Points.Length==34,"mainloopchosenoverbracket");Check(Math.Abs(Math.Abs(main.Centre.x)-.860839f)<.00001f,"actualmainloopcentre");}
   foreach(var row in new[]{(name:"four",s:1.42f,z:9.274653f,expected:true),(name:"six",s:1.2879f,z:9.94199f,expected:true),(name:"highspeed",s:1.11f,z:9.67333f,expected:true),(name:"prairie",s:1.11f,z:9.84193f,expected:true),(name:"twelve",s:.84f,z:9.182588f,expected:false)}){
    var cp=casing.Single(p=>p.GetProperty("name").GetString()==$"native-lod{lod}-{side}");var cv=P(cp).Select(p=>{var q=CylinderCasingShape.NormalizePrairie(p,row.s,row.s);return new Vector3(q.x,q.y*row.s,q.z*row.s+row.z-9.49695f-(row.s-1f)*8.84f);}).ToArray();
    bool found=SteamPipeNativeFit.TryFind(pv,pt,cv,T(cp),out var fit);if(!found&&row.expected){foreach(float probe in new[]{2.22f,2.15f,2.12f,2.110245f,2.10f,2.08f,2.02f})Console.WriteLine($"probe {probe} section {SteamPipeNativeFit.TrySection(pv,pt,uv,probe,out var debug)} n={debug?.Points.Length} centre={debug?.Centre.x}");}Check(found==row.expected,$"{lod}/{side}/{row.name}fitclassification");if(found){Check(fit.Contact.y-fit.CutY>=.01999f,"atleast20mmseat");Check(fit.Contact.y>pv.Min(p=>p.y)&&fit.Contact.y<pv.Max(p=>p.y),"withinrealbranch");rows.Add(new{lod,side,row.name,contact=new[]{fit.Contact.x,fit.Contact.y,fit.Contact.z},fit.CutY,corners=fit.Profile?.Points.Length ?? 0});}
   }
   Check(pv.Zip(original,(a,b)=>a.x==b.x&&a.y==b.y&&a.z==b.z).All(x=>x),"sourceimmutable");
  }
  Check(!SteamPipeNativeFit.TrySection(new Vector3[0],new int[0],null,2,out _),"emptyrejected");Check(!SteamPipeNativeFit.TrySection(new[]{new Vector3(float.NaN,0,0),new Vector3(),new Vector3()},new[]{0,1,2},null,2,out _),"nonfiniterejected");
  File.WriteAllText(Path.Combine(output,"lower-collars.json"),JsonSerializer.Serialize(collars));var result=new{passed=true,checks,rows,collars=collars.Select(c=>c.GetType().GetProperties().Where(p=>new[]{"lod","side","moved","minimumRatio"}.Contains(p.Name)).ToDictionary(p=>p.Name,p=>p.GetValue(c))).ToArray(),limitations="Actual native pipe/casing triangles and UVs, managed vector stand-ins. Cut boundary vertices and quarter-segments seated under actual casing top; rendering, closed-solid containment and body/air clearance remain separate checks."};string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true});File.WriteAllText(Path.Combine(output,"validation.json"),json);Console.WriteLine(json);
 }
}


