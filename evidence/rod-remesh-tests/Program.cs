using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RearrangedS282;
using UnityEngine;

internal static class Program
{
    private static int checks, meshCases, facesChecked, bossVertices, invalidCases;
    private static double minimumFaceCosine = 1d, minimumAreaRatio = 1d, maximumBossError, maximumNativeBossAxialExtent;
    private static void Check(bool condition, string message)
    { checks++; if (!condition) throw new InvalidOperationException(message); }
    private static void Near(double a, double b, double tolerance, string message)
        => Check(Math.Abs(a-b) <= tolerance, message + $" ({a:R} != {b:R})");
    private static void Reject(Action action, string message)
    {
        bool rejected = false;
        try { action(); } catch (ArgumentException) { rejected = true; }
        Check(rejected, "Invalid input accepted: " + message); invalidCases++;
    }
    private static Vector3 V(float z) => new Vector3(.023f, -.041f, z);
    private static bool Same(Vector3 a, Vector3 b) => a.x == b.x && a.y == b.y && a.z == b.z;

    private static void ContractCases()
    {
        float[] oldPins = {-3.1f,-1.55f,0f,1.7f};
        float[] newPins = {-3.7f,-1.55f,.4f,2.5f};
        float width = RodRemesher.DefaultBossHalfWidth;
        var samples = new List<Vector3>();
        foreach (float pin in oldPins)
            foreach (float offset in new[] {-width, -.1f, 0, .1f, width}) samples.Add(V(pin+offset));
        foreach (float pin in oldPins)
            foreach (float edge in new[] {pin-width,pin+width})
            {
                samples.Add(V(MathF.BitDecrement(edge)));
                samples.Add(V(edge));
                samples.Add(V(MathF.BitIncrement(edge)));
            }
        for (float z = -4f; z < 3; z += .0031f) samples.Add(V(z));
        var source = samples.OrderBy(v=>v.z).ToArray();
        var snapshot = (Vector3[])source.Clone();
        var oldSnapshot = (float[])oldPins.Clone(); var newSnapshot = (float[])newPins.Clone();
        var result = RodRemesher.Remesh(source,oldPins,newPins);
        var reversed = RodRemesher.Remesh(source,oldPins.Reverse().ToArray(),newPins.Reverse().ToArray());
        Check(!ReferenceEquals(source,result),"Source array was returned rather than an owned result");
        Check(source.Length == result.Length,"Vertex count changed");
        for (int i=0;i<source.Length;i++)
        {
            Check(Same(source[i],snapshot[i]),"Input vertices mutated");
            Check(Same(result[i],reversed[i]),"Descending pin order changes the deformation");
            Check(result[i].x == source[i].x && result[i].y == source[i].y,"Non-longitudinal coordinate changed");
            if (i>0) Check(result[i].z >= result[i-1].z,"Mapping reverses longitudinal order");
        }
        Check(oldPins.SequenceEqual(oldSnapshot) && newPins.SequenceEqual(newSnapshot),"Pin descriptors mutated");
        var mappedPins = RodRemesher.Remesh(oldPins.Select(V).ToArray(),oldPins,newPins);
        for (int i=0;i<oldPins.Length;i++) Check(mappedPins[i].z == newPins[i],"Pin centre is not exact");
        foreach (float boundary in oldPins.SelectMany(p=>new[] {p-width,p+width}))
        {
            var v=RodRemesher.Remesh(new[] {V(boundary-1e-5f),V(boundary),V(boundary+1e-5f)},oldPins,newPins);
            Check(v[0].z <= v[1].z && v[1].z <= v[2].z,"Boss boundary is not monotone");
            Check(v[2].z-v[0].z < 4e-5,"Discontinuous jump at a boss edge");
        }
        for (int i=0;i<oldPins.Length-1;i++)
        {
            float start=oldPins[i]+width, end=oldPins[i+1]-width;
            var v=RodRemesher.Remesh(new[] {V(start),V((start+end)*.5f),V(end)},oldPins,newPins);
            Near(v[1].z,(v[0].z+v[2].z)*.5,5e-7,"Shaft midpoint is not linear");
        }
        var identity=RodRemesher.Remesh(source,oldPins,oldPins);
        for(int i=0;i<source.Length;i++) Check(Same(source[i],identity[i]),"Identity layout changed geometry");
        Check(RodRemesher.Remesh(Array.Empty<Vector3>(),oldPins,newPins).Length == 0,"Empty mesh rejected");

        Reject(()=>RodRemesher.Remesh(null,oldPins,newPins),"null vertices");
        Reject(()=>RodRemesher.Remesh(source,null,newPins),"null old pins");
        Reject(()=>RodRemesher.Remesh(source,oldPins,null),"null new pins");
        Reject(()=>RodRemesher.Remesh(source,new[] {0f},new[] {0f}),"single pin");
        Reject(()=>RodRemesher.Remesh(source,new[] {0f,1f},new[] {0f}),"mismatched pins");
        foreach(float value in new[] {0f,-.1f,float.NaN,float.PositiveInfinity})
            Reject(()=>RodRemesher.Remesh(source,oldPins,newPins,value),"invalid boss extent");
        foreach(float value in new[] {float.NaN,float.PositiveInfinity,float.NegativeInfinity})
        {
            Reject(()=>RodRemesher.Remesh(source,new[] {0f,value},new[] {0f,1f}),"non-finite old pin");
            Reject(()=>RodRemesher.Remesh(source,new[] {0f,1f},new[] {0f,value}),"non-finite new pin");
            Reject(()=>RodRemesher.Remesh(new[] {new Vector3(value,0,0)},oldPins,newPins),"non-finite vertex X");
            Reject(()=>RodRemesher.Remesh(new[] {new Vector3(0,value,0)},oldPins,newPins),"non-finite vertex Y");
            Reject(()=>RodRemesher.Remesh(new[] {V(value)},oldPins,newPins),"non-finite vertex Z");
        }
        foreach(float[] bad in new[] {new[] {0f,0f},new[] {0f,.28f},new[] {0f,.2805f},new[] {0f,1f,.5f}})
        {
            var good=Enumerable.Range(0,bad.Length).Select(i=>(float)i).ToArray();
            Reject(()=>RodRemesher.Remesh(source,bad,good),"old pin overlap/order/short shaft");
            Reject(()=>RodRemesher.Remesh(source,good,bad),"new pin overlap/order/short shaft");
        }
        Reject(()=>RodRemesher.Remesh(source,new[] {0f,1f},new[] {1f,0f}),"reversed target connectivity");
        Reject(()=>RodRemesher.Remesh(new[] {V(float.MaxValue)},new[] {0f,1f},
            new[] {float.MaxValue*.5f,float.MaxValue*.75f}),"overflowing output");
    }

    private static (double x,double y,double z) Cross(Vector3 a,Vector3 b,Vector3 c)
    {
        double ux=b.x-a.x, uy=b.y-a.y, uz=b.z-a.z;
        double vx=c.x-a.x, vy=c.y-a.y, vz=c.z-a.z;
        return (uy*vz-uz*vy,uz*vx-ux*vz,ux*vy-uy*vx);
    }
    private static double Magnitude((double x,double y,double z) v) => Math.Sqrt(v.x*v.x+v.y*v.y+v.z*v.z);
    private static double Distance(Vector3 a,Vector3 b)
        => Math.Sqrt(Math.Pow((double)a.x-b.x,2)+Math.Pow((double)a.y-b.y,2)+Math.Pow((double)a.z-b.z,2));

    private static void NativeCases(JsonElement mesh)
    {
        string name=mesh.GetProperty("name").GetString();
        var source=mesh.GetProperty("vertices").EnumerateArray().Select(v=>
            new Vector3(v[0].GetSingle(),v[1].GetSingle(),v[2].GetSingle())).ToArray();
        var pins=mesh.GetProperty("pins").EnumerateArray().Select(v=>v.GetSingle()).ToArray();
        var indices=mesh.GetProperty("triangles").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
        var sourceSnapshot=(Vector3[])source.Clone(); var indexSnapshot=(int[])indices.Clone();
        Check(indices.Length%3 == 0,"Invalid fixture triangle count");
        Check(indices.All(i=>i>=0 && i<source.Length),"Invalid fixture triangle index");
        var used=indices.Distinct().ToArray();
        Check(used.Length>0,"Fixture has no rendered vertices");
        double measuredExtent=used.Max(i=>pins.Min(p=>Math.Abs((double)source[i].z-p)));
        maximumNativeBossAxialExtent=Math.Max(maximumNativeBossAxialExtent,measuredExtent);
        Check(measuredExtent<RodRemesher.DefaultBossHalfWidth,
            "Native boss/joint geometry exceeds the measured protected window; review native topology");
        // 3-axle segment contains only half the centre boss. Both copies must
        // preserve their partial bearing before mirroring/translation in-game.
        foreach(float factor in new[] {.5f,.8f,1f,1.1f,1.25f,1.5f})
        foreach(bool asymmetric in new[] {false,true})
        foreach(float width in new[] {.13f,.14f,.16f})
        {
            int main=name == "threeHalf" ? 1 : pins.Length == 2 ? 0 : 1;
            var target=pins.Select((p,i)=>pins[main]+(p-pins[main])*factor+
                (asymmetric && i>main ? .12f*(i-main) : 0)).ToArray();
            var result=RodRemesher.Remesh(source,pins,target,width); meshCases++;
            Check(result.Length == source.Length,"Native vertex count changed");
            var mappedPin=RodRemesher.Remesh(new[] {V(pins[main])},pins,target,width)[0];
            Check(mappedPin.z == pins[main],"Main crank-pin anchor moved");
            for(int i=0;i<source.Length;i++)
            {
                Check(Same(source[i],sourceSnapshot[i]),"Native source was mutated");
                Check(result[i].x == source[i].x && result[i].y == source[i].y,"Native rod thickness/height changed");
            }
            for(int p=0;p<pins.Length;p++)
            {
                var boss=used.Where(i=>Math.Abs(source[i].z-pins[p]) <= width).ToArray();
                Check(boss.Length>0,"No boss vertices at fixture pin");
                double shift=(double)target[p]-pins[p];
                foreach(int i in boss)
                {
                    double error=Math.Abs(result[i].z-source[i].z-shift);
                    maximumBossError=Math.Max(maximumBossError,error);
                    Near(error,0,1.5e-6,"Bearing boss did not translate rigidly"); bossVertices++;
                    // Pairwise distance from the first boss vertex checks rigid
                    // shape, independent of how the interpolation is implemented.
                    Near(Distance(source[i],source[boss[0]]),Distance(result[i],result[boss[0]]),
                        2e-6,"Bearing diameter/joint shape changed");
                }
            }
            for(int i=0;i<indices.Length;i+=3)
            {
                var a=Cross(source[indices[i]],source[indices[i+1]],source[indices[i+2]]);
                var b=Cross(result[indices[i]],result[indices[i+1]],result[indices[i+2]]);
                double an=Magnitude(a),bn=Magnitude(b);
                Check(an>1e-10,"Native source has a degenerate face");
                Check(bn>1e-10,"Remeshing collapsed a face");
                double cosine=(a.x*b.x+a.y*b.y+a.z*b.z)/(an*bn);
                minimumFaceCosine=Math.Min(minimumFaceCosine,cosine);
                minimumAreaRatio=Math.Min(minimumAreaRatio,bn/an);
                Check(cosine>0,"Remeshing reversed a face"); facesChecked++;
            }
            Check(indices.SequenceEqual(indexSnapshot),"Native triangle topology/index order changed");
        }
        Console.WriteLine($"{name}: {source.Length} positions, {indices.Length/3} native faces, 36 deformation cases");
    }

    private static void Main(string[] args)
    {
        if(args.Length != 2) throw new ArgumentException("Expected local native JSON and output receipt path.");
        ContractCases();
        using var document=JsonDocument.Parse(File.ReadAllText(args[0]));
        var data=document.RootElement;
        Check(data.GetProperty("nativeDataOnly").GetBoolean(),"Fixture must identify native geometry as local-only");
        foreach(var mesh in data.GetProperty("meshes").EnumerateArray()) NativeCases(mesh);
        var receipt=new
        {
            passed=true,checks,meshCases,facesChecked,bossVertices,invalidCases,
            defaultProtectedBossHalfWidthM=RodRemesher.DefaultBossHalfWidth,
            minimumShaftLengthM=RodRemesher.MinimumShaftLength,
            maximumBossTranslationErrorM=maximumBossError,
            maximumMeasuredNativeBossAxialExtentM=maximumNativeBossAxialExtent,
            minimumSourceOutputFaceNormalDot=minimumFaceCosine,minimumAreaRatio,
            nativeBaseline=data.GetProperty("nominalToNativeBaseline"),
            recipeSourceSha256=data.GetProperty("recipeSourceSha256").GetString(),
            nativeObjSha256=data.GetProperty("nativeObjSha256").GetString(),
            validationScope="Actual pure production RodRemesher with a Vector3 value-type stand-in; native imported OBJ topology and exact existing MeshFinder recipe replay; rigid boss geometry, exact pin mapping, continuous monotone shafts, both list directions, identity, finite/invalid inputs and source/index immutability. The helper only accepts positions, so UV/index/material channel copying, animated integration, LOD coverage and game visuals require separate integration validation.",
            nativeDataPolicy="Fixture JSON remains ignored under build/ and must never be packaged."
        };
        File.WriteAllText(args[1],JsonSerializer.Serialize(receipt,new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"PASS {checks} assertions; {meshCases} native-mesh cases; {facesChecked} faces; {bossVertices} protected boss samples.");
    }
}
