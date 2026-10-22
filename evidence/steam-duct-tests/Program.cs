using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Globalization;
using RearrangedS282;
using UnityEngine;

internal static class Program
{
    private static int checks;
    private static SteamDuctMesh.Template Original;
    private static readonly Vector2 AtlasMin = new(.362060546875f, .65478515625f), AtlasMax = new(.3818359375f, .751953125f);
    private static void Check(bool condition, string message) { checks++; if (!condition) throw new Exception(message); }
    private static float Error(Vector3 a, Vector3 b) => (a - b).magnitude;
    private static float Dot(Vector3 a, Vector3 b) => Vector3.Dot(a, b);
    private static Vector3 Unit(Vector3 p) => p / p.magnitude;
    private static void Throws(Action action, string message)
    {
        bool threw = false; try { action(); } catch (ArgumentException) { threw = true; } catch (InvalidOperationException) { threw = true; }
        Check(threw, message);
    }
    private static void Sound(SteamDuctMesh.Builder mesh)
    {
        Check(mesh.Vertices.Count == mesh.Normals.Count && mesh.Vertices.Count == mesh.UV.Count, "Mesh streams disagree");
        Check(mesh.Triangles.Count % 3 == 0, "Broken triangle stream");
        for (int i = 0; i < mesh.Vertices.Count; i++)
        {
            var vertex = mesh.Vertices[i]; var normal = mesh.Normals[i]; var uv = mesh.UV[i];
            Check(float.IsFinite(vertex.x) && float.IsFinite(vertex.y) && float.IsFinite(vertex.z), "Nonfinite vertex");
            Check(Math.Abs(normal.magnitude - 1f) < .00002f, "Normal is not unit length");
            Check(uv.x >= 0f && uv.x <= 1f && uv.y >= 0f && uv.y <= 1f, "UV escaped the native texture atlas");
        }
        for (int i = 0; i < mesh.Triangles.Count; i += 3)
        {
            int a = mesh.Triangles[i], b = mesh.Triangles[i + 1], c = mesh.Triangles[i + 2];
            Check(a >= 0 && b >= 0 && c >= 0 && a < mesh.Vertices.Count && b < mesh.Vertices.Count && c < mesh.Vertices.Count, "Triangle index is invalid");
            var face = Vector3.Cross(mesh.Vertices[b] - mesh.Vertices[a], mesh.Vertices[c] - mesh.Vertices[a]);
            Check(face.magnitude > .00000001f, "Collapsed face");
            var normal = mesh.Normals[a] + mesh.Normals[b] + mesh.Normals[c];
            Check(Dot(face, normal) > 0f, "Face winding disagrees with its outward normal");
        }
    }
    private static void Tube(Vector3[] points, float radius, float elbow, string name, bool caps = false)
    {
        var mesh = new SteamDuctMesh.Builder(Original);
        mesh.Tube(points, radius, elbow, caps, caps);
        Check(Error(mesh.LastStart, points[0]) < .000001f && Error(mesh.LastEnd, points[^1]) < .000001f, name + " endpoint moved");
        Check(Dot(mesh.LastTangents[0], Unit(points[1] - points[0])) > .999999f, name + " inlet tangent changed");
        Check(Dot(mesh.LastTangents[^1], Unit(points[^1] - points[^2])) > .999999f, name + " outlet tangent changed");
        for (int row = 0; row < mesh.LastCentreline.Count; row++)
        {
            for (int col = 0; col < Original.Profile.Length; col++)
            {
                var radial = mesh.Vertices[row * Original.Profile.Length + col] - mesh.LastCentreline[row];
                Check(Math.Abs(radial.magnitude - radius) < .000003f, name + " diameter changed");
                Check(Math.Abs(Dot(radial, mesh.LastTangents[row])) < .000003f, name + " cross-section tilted");
                Check(Error(mesh.Normals[row * Original.Profile.Length + col], radial / radius) < .00004f, name + " shaft shading differs from surface");
                Check(mesh.UV[row * Original.Profile.Length + col].x==Original.ProfileUV[col].x, name + " original shaft UV seam changed");
            }
            Check(Error(mesh.Vertices[row * Original.Profile.Length], mesh.Vertices[row * Original.Profile.Length + Original.Profile.Length - 1]) < .000001f, name + " texture seam split geometrically");
            if (row > 0 && row + 1 < mesh.LastCentreline.Count)
            {
                var a = mesh.LastCentreline[row - 1]; var b = mesh.LastCentreline[row]; var c = mesh.LastCentreline[row + 1];
                float twiceArea = Vector3.Cross(b - a, c - a).magnitude;
                if (twiceArea > .0000001f)
                {
                    float circumradius = Error(a,b) * Error(b,c) * Error(c,a) / (2f * twiceArea);
                    Check(circumradius >= elbow - .0001f, name + " curvature tighter than requested circular elbow");
                }
            }
        }
        Sound(mesh);
    }
    private static void Main(string[] args)
    {
        string output = args[0]; Directory.CreateDirectory(output);
        using(var doc = JsonDocument.Parse(File.ReadAllText(args[1]))) {
            var source=doc.RootElement.GetProperty("meshes").EnumerateArray().Single(m=>m.GetProperty("name").GetString()=="steam_transfer");
            var v=source.GetProperty("vertices").EnumerateArray().Select(p=>new Vector3(p[0].GetSingle(),p[1].GetSingle(),p[2].GetSingle())).ToArray();
            var t=source.GetProperty("triangles").EnumerateArray().SelectMany(p=>p.EnumerateArray().Select(i=>i.GetInt32())).ToArray();
            var uv=source.GetProperty("uv").EnumerateArray().Select(p=>new Vector2(p[0].GetSingle(),p[1].GetSingle())).ToArray();
            Original=new SteamDuctMesh.Template(v,t,uv);
            Check(Original.Profile.Length==14,"Authored shaft profile was replaced");
            Check(Original.CollarTriangles.Length==345,"Authored collar topology was replaced");
        }
        Tube(new[] { new Vector3(0,0,0), new Vector3(0,0,2) }, .1f, .3f, "straight", true);
        Tube(new[] { new Vector3(0,0,0), new Vector3(0,0,1), new Vector3(0,1,1) }, .1f, .3f, "round 90");
        Tube(new[] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,1,1) }, .1f, .3f, "three-plane elbows", true);
        Tube(new[] { new Vector3(0,0,0), new Vector3(-1,0,0), new Vector3(-1,1,0), new Vector3(-1,1,-1) }, .1f, .3f, "mirrored elbows", true);
        Tube(new[] { new Vector3(0,0,0), new Vector3(.001f,0,0), new Vector3(1,0,0), new Vector3(1,.8f,0) }, .08128f, .21f, "collapsed collinear waypoint");
        Tube(new[] { new Vector3(0,0,0), new Vector3(3,0,0), new Vector3(.181f,1.026f,0) }, .1f, .3f, "160 degree bend");
        Tube(new[] { new Vector3(0,0,0), new Vector3(0,0,.002f) }, .1f, .3f, "short seated sleeve", true);
        var b = new SteamDuctMesh.Builder(Original);
        Throws(() => b.Tube(new[] { Vector3.zero, Vector3.zero }, .1f, .3f), "Zero route accepted");
        Throws(() => b.Tube(new[] { Vector3.zero, new Vector3(0,0,.2f), new Vector3(0,.2f,.2f) }, .1f, .3f), "Unseated elbows accepted");
        Throws(() => b.Tube(new[] { Vector3.zero, new Vector3(0,0,1), new Vector3(0,1,1) }, .1f, .14f), "Tight radius accepted");
        Throws(() => b.Tube(new[] { Vector3.zero, new Vector3(1,0,0), new Vector3(0,0,0) }, .1f, .3f), "Reversal accepted");
        Throws(() => b.Tube(new[] { Vector3.zero, new Vector3(float.NaN,1,1) }, .1f, .3f), "NaN accepted");
        Throws(() => b.Tube(new[] { Vector3.zero, new Vector3(0,0,1) }, -.1f, .3f), "Negative radius accepted");
        Throws(() => b.Reducer(Vector3.zero,new Vector3(0,0,.025f),.0673f,.1016f), "Abrupt reducer taper accepted");
        Check(b.Vertices.Count == 0 && b.Triangles.Count == 0, "Invalid route emitted partial mesh");
        foreach (var axis in new[] { Vector3.up, Vector3.forward, Unit(new Vector3(-1,2,-3)) })
        {
            b.Clear(); var origin = new Vector3(-.7f,1.6f,-2.3f);
            b.Collar(origin, axis, .1016f); Sound(b);
            Check(b.Vertices.Count==Original.CollarVertices.Length,"Authored collar vertex count changed");
            for(int i=0;i<b.Vertices.Count;i++) {
                var vertex=b.Vertices[i]; var delta=vertex-origin;
                Check(Math.Abs(Dot(delta,axis)-Original.CollarVertices[i].z)<.000002f,"Rigid original collar axial dimension changed");
                Check(Math.Abs(delta.magnitude-Original.CollarVertices[i].magnitude)<.000002f,"Rigid original collar radial dimension changed");
                Check(b.UV[i].x==Original.CollarUV[i].x&&b.UV[i].y==Original.CollarUV[i].y,"Authored collar UV changed");
            }
            b.Clear(); b.Coupling(origin, axis, .1f, .22f); Sound(b);
            Check(b.Vertices.All(v => Math.Abs(Dot(v-origin,axis)) <= .110001f), "Coupling ridges extend beyond sleeve");
            foreach(var pair in new[]{(.0673f,.08128f),(.1016f,.0673f)}) {
                b.Clear(); b.Reducer(origin,origin+axis*.2f,pair.Item1,pair.Item2); Sound(b);
                for(int row=0;row<2;row++)for(int col=0;col<Original.Profile.Length;col++) {
                    var delta=b.Vertices[row*Original.Profile.Length+col]-(origin+axis*(row*.2f));
                    Check(Math.Abs(Dot(delta,axis))<.000002f,"Reducer end section tilted");
                    Check(Math.Abs(delta.magnitude-(row==0?pair.Item1:pair.Item2))<.000002f,"Reducer radius mismatches original-profile inlet/outlet");
                }
            }
        }
        using(var native = JsonDocument.Parse(File.ReadAllText(args[2])))
        {
            var actualUv = File.ReadLines(args[3]).Where(s=>s.StartsWith("vt ")).Select(s=>s.Split(' ',StringSplitOptions.RemoveEmptyEntries))
                .Select(v=>new Vector2(float.Parse(v[1],CultureInfo.InvariantCulture),float.Parse(v[2],CultureInfo.InvariantCulture))).ToArray();
            foreach(var side in new[]{1,-1})
            {
                var pipe=native.RootElement.GetProperty("parts").EnumerateArray().Single(p=>p.GetProperty("name").GetString()=="native-lod0-pipe-"+(side>0?"left":"right"));
                var v=pipe.GetProperty("vertices").EnumerateArray().Select(p=>new Vector3(p[0].GetSingle(),p[1].GetSingle(),p[2].GetSingle())).ToArray();
                var faces=pipe.GetProperty("triangles").EnumerateArray().SelectMany(p=>p.EnumerateArray().Select(i=>i.GetInt32())).ToArray();
                var uv=pipe.GetProperty("sourceVertexIds").EnumerateArray().Select(i=>actualUv[i.GetInt32()]).ToArray();
                Check(SteamPipeNativeFit.TrySection(v,faces,uv,2.02f,out var section),"Actual Prairie native cut did not yield a closed section");
                Check(section.Points.Length==35,"Actual Prairie native boundary changed");
                var end=new Vector3(side*.9875f,1.78292f,9.84193f);
                b.Clear(); b.Adapter(section.Points,section.UV,end,.08128f,section.EdgeEndUV); Sound(b);
                foreach(bool reversed in new[]{false,true})
                {
                    b.Clear();
                    int n=section.Points.Length;
                    var boundary=reversed?section.Points.Reverse().ToArray():section.Points;
                    var texture=reversed?Enumerable.Range(0,n).Select(i=>section.EdgeEndUV[(n-2-i+n)%n]).ToArray():section.UV;
                    var edgeEnd=reversed?Enumerable.Range(0,n).Select(i=>section.EdgeStartUV[(n-2-i+n)%n]).ToArray():section.EdgeEndUV;
                    b.Adapter(boundary,texture,end,.08128f,edgeEnd,new Vector3(side*.22f,0f,0f)); Sound(b);
                    foreach(var skinUV in b.UV)
                        Check(skinUV.x>=AtlasMin.x-.000001f&&skinUV.x<=AtlasMax.x+.000001f&&skinUV.y>=AtlasMin.y-.000001f&&skinUV.y<=AtlasMax.y+.000001f,"Adapter skin samples outside original transfer atlas island");
                    Check(b.LastAdapterMaximumGapM<.000001f,"Adapter measured endpoint-boundary gap");
                    Check(Error(b.LastAdapterStart,section.Centre)<.000001f&&Error(b.LastAdapterEnd,end)<.000001f,"Adapter endpoint diagnostics moved");
                    int strips=b.Vertices.Count/34;
                    var starts=Enumerable.Range(0,strips).SelectMany(i=>new[]{i*34,i*34+1}).ToArray();
                    var ends=Enumerable.Range(0,strips).SelectMany(i=>new[]{i*34+32,i*34+33}).ToArray();
                    foreach(int i in Enumerable.Range(0,boundary.Length))
                    {
                        var nearest=starts.Where(j=>Error(b.Vertices[j],boundary[i])<.000001f).ToArray();
                        Check(nearest.Length>0,"Adapter moved an actual original cut corner");
                        // The unchanged retained native prefix owns its original
                        // boundary UVs. The newly extended casting deliberately
                        // stays inside the transfer island instead of blending
                        // across unrelated native/transfer atlas regions.
                    }
                    for(int col=0;col<Original.Profile.Length-1;col++)
                    {
                        var profile=Original.Profile[col]*(.08128f/.1016f); var corner=end+new Vector3(profile.x,0,profile.y);
                        var nearest=ends.Where(j=>Error(b.Vertices[j],corner)<.000001f).ToArray();
                        Check(nearest.Length>0,"Adapter removed an original transfer profile corner");
                        Check(nearest.Any(j=>b.UV[j].x==Original.ProfileUV[col].x&&b.UV[j].y==Original.ProfileUV[col].y),"Adapter changed original transfer end UV");
                    }
                    for(int strip=0;strip<strips;strip++)for(int col=0;col<2;col++)
                    {
                        int first=strip*34+col;
                        Check(Math.Abs(b.Normals[first].y)<.000001f&&Math.Abs(b.Normals[first+32].y)<.000001f,"Cast transition is not vertical at an attachment");
                        for(int row=1;row<17;row++)Check(b.Vertices[first+row*2].y<b.Vertices[first+(row-1)*2].y,"Cast transition folds back on a horizontal section");
                    }
                    if(!reversed)
                    {
                        int nativeUVSeams=Enumerable.Range(0,n).Count(i=>Math.Abs(section.EdgeStartUV[i].x-section.EdgeEndUV[(i+n-1)%n].x)+Math.Abs(section.EdgeStartUV[i].y-section.EdgeEndUV[(i+n-1)%n].y)>.00001f);
                        File.WriteAllText(Path.Combine(output,"prairie-adapter-"+side+".json"),JsonSerializer.Serialize(new{side,source="actual native LOD0 pipe and installed LMS OBJ UVs; new adapter skin uses original transfer shaft island; retained native prefix keeps native UV",nativeUVSeams,midOffset=new[]{side*.22f,0f,0f},boundary=section.Points.Select(p=>new[]{p.x,p.y,p.z}),boundaryUV=section.UV.Select(p=>new[]{p.x,p.y}),boundaryEdgeEndUV=section.EdgeEndUV.Select(p=>new[]{p.x,p.y}),vertices=b.Vertices.Select(p=>new[]{p.x,p.y,p.z}),normals=b.Normals.Select(p=>new[]{p.x,p.y,p.z}),uv=b.UV.Select(p=>new[]{p.x,p.y}),triangles=b.Triangles}));
                    }
                }
            }
        }
        b.Clear(); var preview = new[] {new Vector3(0,0,0),new Vector3(0,1.2f,0),new Vector3(0,1.2f,2.5f),new Vector3(.8f,1.2f,2.5f),new Vector3(.8f,.2f,2.5f)};
        b.Tube(preview,.1f,.32f);
        b.Collar(preview[0],Vector3.up,.1f);
        b.Collar(preview[^1],Vector3.up,.1f);
        b.Coupling(new Vector3(0,1.2f,1.2f),Vector3.forward,.1f,.22f); Sound(b);
        b.Reducer(new Vector3(0,-.25f,0),Vector3.zero,.0673f,.1f); Sound(b);
        using(var obj = new StreamWriter(Path.Combine(output,"new-steam-duct-kit.obj")))
        {
            obj.WriteLine("# Original procedural mesh preview, no game mesh content");
            foreach(var v in b.Vertices) obj.WriteLine(FormattableString.Invariant($"v {v.x} {v.y} {v.z}"));
            foreach(var uv in b.UV) obj.WriteLine(FormattableString.Invariant($"vt {uv.x} {uv.y}"));
            foreach(var n in b.Normals) obj.WriteLine(FormattableString.Invariant($"vn {n.x} {n.y} {n.z}"));
            for(int i=0;i<b.Triangles.Count;i+=3) {var ids=b.Triangles.Skip(i).Take(3).Select(v=>$"{v+1}/{v+1}/{v+1}");obj.WriteLine("f "+string.Join(" ",ids));}
        }
        File.WriteAllText(Path.Combine(output,"validation.json"), JsonSerializer.Serialize(new {pass=true,checks,source="production SteamDuctMesh.cs with managed Unity vector stand-ins and measured original transfer asset", scenarios=new[]{"straight and short seated sleeve","90 and 160 degree circular elbows","three-plane and mirrored routes","native shaft UV island","constant cross-section and exact endpoint tangents","outward winding and noncollapsed triangles","rigid original collar topology and UV","original paired-collar union","source-profile reducers","actual Prairie cut-boundary adapters both sides and reversed ordering","invalid-route guards"},nativeShaftVertices=Original.Profile.Length,nativeCollarFaces=Original.CollarTriangles.Length/3,previewVertices=b.Vertices.Count,previewTriangles=b.Triangles.Count/3},new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"{checks} steam duct geometry checks passed; preview {b.Vertices.Count} vertices {b.Triangles.Count/3} triangles");
    }
}
