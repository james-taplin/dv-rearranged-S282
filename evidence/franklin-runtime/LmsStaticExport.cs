using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using HarmonyLib;
using UnityEngine;

public static class LmsNativeExport
{
    static readonly BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    static JArray Vec(Vector3 p) => new JArray(p.x,p.y,p.z);
    static JArray Quat(Quaternion q) => new JArray(q.x,q.y,q.z,q.w);
    static string Digest(string path){using(var file=File.OpenRead(path))using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(file)).Replace("-","");}
    static bool MaterialBoundary(ref Material __result)
    {
        var shader=Shader.Find("Hidden/InternalErrorShader");
        if(shader==null)throw new Exception("Standalone material boundary needs built-in error shader");
        __result=new Material(shader);return false;
    }
    public static IEnumerator Run(string output)
    {
        string run = Path.GetDirectoryName(output);
        var config = JObject.Parse(File.ReadAllText(Path.Combine(run,"probe-bin","lms-config.json")));
        string managed=(string)config["managed"], lms=(string)config["lms"];
        AppDomain.CurrentDomain.AssemblyResolve += (sender,args) => {
            string candidate=Path.Combine(managed,"UnityModManager",new AssemblyName(args.Name).Name+".dll");
            return File.Exists(candidate)?Assembly.LoadFrom(candidate):null;
        };
        Assembly.LoadFrom(Path.Combine(managed,"UnityModManager","UnityModManager.dll"));
        var assembly=Assembly.LoadFrom(Path.Combine(lms,"LocoMeshSplitter.dll"));
        var materialBoundary=new Harmony("s282.private.lms.material-boundary");
        materialBoundary.Patch(assembly.GetType("Dummiesman.OBJLoaderHelper",true).GetMethod("CreateNullMaterial",All),
            prefix:new HarmonyMethod(typeof(LmsNativeExport).GetMethod("MaterialBoundary",BindingFlags.Static|BindingFlags.NonPublic)));
        var main=assembly.GetType("LocoMeshSplitter.Main",true);
        main.GetProperty("ModPath",All).GetSetMethod(true).Invoke(null,new object[]{lms+Path.DirectorySeparatorChar});
        var logger=main.GetProperty("Logger",All);
        logger.GetSetMethod(true).Invoke(null,new[]{Activator.CreateInstance(logger.PropertyType,new object[]{"S282NativeLMSProbe"})});
        // Read only the private copied cache. Disables the export decision before
        // its first access, preventing subprocesses or cache deletion in this test.
        assembly.GetType("LocoMeshSplitter.MeshFinder",true).GetField("isMeshFindingNecessary",All).SetValue(null,(bool?)false);
        var splitter=assembly.GetType("LocoMeshSplitter.MeshSplitters.S282A.S282ALOD0MeshSplitter",true);
        splitter.GetMethod("Init",All).Invoke(null,null);
        var source=(GameObject)splitter.GetProperty("SplitLocoBodyLOD0",All).GetValue(null);
        if(source==null)throw new Exception("Actual LMS splitter returned no template");
        // S282AMeshLoader places its template under native identity Static_LOD0
        // at +4.88z. Avoid its full TrainCar constructor; keep that one placement
        // explicit and compare the actual template/part matrices independently.
        var parent=new GameObject("Static_LOD0 datum");
        var instance=UnityEngine.Object.Instantiate(source,parent.transform);
        instance.transform.localPosition=new Vector3(0,0,4.88f);
        var parts=new JArray();
        foreach(string name in new[]{"s282a_cylinder_l","s282a_cylinder_r","s282a_cylinder_cocks","s282a_crosshead_guide_l","s282a_crosshead_guide_r","s282a_dry_pipe_l","s282a_dry_pipe_r"})
        {
            var part=instance.transform.Find(name);
            if(part==null)throw new Exception("Actual LMS missing "+name);
            var mesh=part.GetComponent<MeshFilter>().sharedMesh;
            var matrix=parent.transform.worldToLocalMatrix*part.localToWorldMatrix;
            var values=new JArray();for(int i=0;i<16;i++)values.Add(matrix[i]);
            parts.Add(new JObject{["name"]=name,["vertices"]=new JArray(mesh.vertices.Select(v=>(object)Vec(v))),["triangles"]=new JArray(mesh.triangles),
                ["nativeBodyFromMesh"]=values,["position"]=Vec(part.localPosition),["rotation"]=Quat(part.localRotation),["scale"]=Vec(part.localScale),
                ["meshBoundsMin"]=Vec(mesh.bounds.min),["meshBoundsMax"]=Vec(mesh.bounds.max),["active"]=part.gameObject.activeSelf,["renderer"]=part.GetComponent<MeshRenderer>().enabled});
        }
        File.WriteAllText(output,new JObject{["passed"]=true,["actualLmsDLL"]=Path.Combine(lms,"LocoMeshSplitter.dll"),["parts"]=parts,
            ["sourceSHA256"]=Digest(Path.Combine(lms,"assets","s282_locomotive_body.obj")),["lmsSHA256"]=Digest(Path.Combine(lms,"LocoMeshSplitter.dll")),
            ["limitations"]="Actual installed LMS splitter Init, Dummiesman OBJ import, vertex edits, selection, compaction and native Unity Mesh creation executed. Source OBJ cache is a private copied file; native Static_LOD0 identity and loader root+4.88 placement supplied explicitly. Test-only fallback-material boundary supplies built-in error shader because standalone strips Standard(Specular setup); importer/splitter geometry bodies unchanged. No full LMS S282AMeshLoader/TrainCar constructor or paint lifecycle."}.ToString());
        UnityEngine.Object.Destroy(parent);UnityEngine.Object.Destroy(source);
        yield break;
    }
}
