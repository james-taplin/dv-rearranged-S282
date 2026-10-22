using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using RearrangedS282;
using UnityEngine;
internal static class ErieProgram
{
    static int checks;
    static void Check(bool value,string message){checks++;if(!value)throw new Exception(message);}
    static void Main(string[] args)
    {
        using var doc=JsonDocument.Parse(File.ReadAllText(args[0]));
        var source=doc.RootElement.GetProperty("meshes").EnumerateArray().Single(m=>m.GetProperty("name").GetString()=="steam_transfer");
        var mesh=new Mesh{vertices=source.GetProperty("vertices").EnumerateArray().Select(p=>new Vector3(p[0].GetSingle(),p[1].GetSingle(),p[2].GetSingle())).ToArray(),
            triangles=source.GetProperty("triangles").EnumerateArray().SelectMany(p=>p.EnumerateArray().Select(x=>x.GetInt32())).ToArray(),
            uv=source.GetProperty("uv").EnumerateArray().Select(p=>new Vector2(p[0].GetSingle(),p[1].GetSingle())).ToArray()};
        Dummiesman.OBJLoader.Assets["steam_transfer.obj"]=mesh;
        var original=mesh.vertices.ToArray();
        var tender=new GameObject("tender").AddComponent<TrainCar>();
        tender.transform.localPosition=new(120,4,37);tender.transform.localRotation=Quaternion.Euler(0,27,0);
        var body=tender.transform.Add("LocoS282B_Body");var lod=body.gameObject.AddComponent<LODGroup>();
        var native=body.Add("s282_tender").gameObject.AddComponent<MeshRenderer>();
        lod.SetLODs(new[]{new LOD(.5f,new Renderer[]{native}),new LOD(.05f,new Renderer[]{native}),new LOD(.001f,Array.Empty<Renderer>())});
        var material=new Material();
        for(int cycle=0;cycle<5;cycle++)
        {
            var root=tender.transform.Add("engine");var actor=root.gameObject.AddComponent<ErieTenderPresentation>();
            int released=0;actor.Releasing=()=>released++;
            actor.Initialize(tender,new[]{material});
            var filter=root.GetComponentsInChildren<MeshFilter>(true).Single();var renderer=filter.GetComponent<MeshRenderer>();
            Check(ReferenceEquals(renderer.sharedMaterials.Single(),material),"Shared native material is reused");
            Check(!ReferenceEquals(filter.sharedMesh,mesh),"Per-instance exhaust mesh");
            Check(filter.sharedMesh.vertices.All(v=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z)),"Finite vertical mesh");
            Check(Math.Abs(filter.sharedMesh.vertices.Max(v=>v.y)-actor.Outlet.localPosition.y)<.00001f,"Mouth matches plume outlet");
            Check(filter.sharedMesh.vertices.All(v=>v.x> -1.95f&&v.x<-.8f&&v.z> -7.18f&&v.z< -6.72f),"Pipe stays in rear side-wall footprint");
            Check(filter.sharedMesh.vertices.Where(v=>v.y>2.7f).All(v=>v.x< -1.49f),"Upright and bands stay outside the deck and water hatch");
            Check(filter.sharedMesh.vertices.Min(v=>v.y)>1.5f,"Lower elbow stays above running gear");
            Check(lod.GetLODs().Take(2).All(l=>l.renderers.Count(r=>ReferenceEquals(r,renderer))==1),"One pipe per native visible LOD");
            Check(lod.GetLODs().Last().renderers.Length==0,"Cull LOD unchanged");
            var owned=filter.sharedMesh;
            if(cycle==0)File.WriteAllText(Path.Combine(Path.GetDirectoryName(args[1]),"exhaust-mesh.json"),JsonSerializer.Serialize(new{vertices=owned.vertices.Select(v=>new[]{v.x,v.y,v.z}),triangles=owned.triangles}));
            // A foreign mod's renderer added after us must survive removal.
            var other=body.Add("other mod").gameObject.AddComponent<MeshRenderer>();
            lod.SetLODs(lod.GetLODs().Select(l=>new LOD(l.screenRelativeTransitionHeight,l.renderers.Length==0?l.renderers:l.renderers.Append(other).ToArray())).ToArray());
            actor.Release();actor.Release();
            Check(released==1,"Cleanup callback is idempotent");
            Check(owned.destroyed&&!mesh.destroyed,"Only owned mesh destroyed");
            Check(lod.GetLODs().Take(2).All(l=>l.renderers.Contains(native)&&l.renderers.Contains(other)&&!l.renderers.Contains(renderer)),"Native and foreign LOD renderers retained");
            Check(actor.Outlet==null,"Outlet cleared");
            UnityEngine.Object.Destroy(root.gameObject);
        }
        Check(mesh.vertices.SequenceEqual(original),"Original MIT mesh unchanged");
        File.WriteAllText(args[1],JsonSerializer.Serialize(new{checks,cycles=5,passed=true,limitation="Managed ownership/geometry fixture; Unity rendering and particles require in-game test"},new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"Erie presentation: {checks} checks passed");
    }
}
