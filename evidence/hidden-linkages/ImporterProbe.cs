using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dummiesman;
using Newtonsoft.Json.Linq;
using UnityEngine;

// Runs only in the preserved isolated Unity player, never in Derail Valley.
public static class HiddenLinkageImporterProbe
{
    public static void Run(string outputPath)
    {
        var config = JObject.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(outputPath), "probe-bin", "import-config.json")));
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            string name = new AssemblyName(args.Name).Name + ".dll";
            foreach (string directory in new[] { (string)config["umm"], (string)config["splitter"] })
            {
                string path = Path.Combine(directory, name);
                if (File.Exists(path)) return Assembly.LoadFrom(path);
            }
            return null;
        };
        typeof(OBJLoader).Assembly.GetType("RearrangedS282.Main", true).GetProperty("ModPath")
            .GetSetMethod(true).Invoke(null, new object[] { Path.GetDirectoryName(outputPath) });
        var data = JObject.Parse(File.ReadAllText((string)config["geometry"]));
        var normalize = typeof(OBJLoader).Assembly.GetType("RearrangedS282.MeshFinder", true)
            .GetMethod("NormalizeLinkageImport", BindingFlags.Static | BindingFlags.NonPublic);
        var rows = new JArray();
        float maximum = 0f;
        foreach (string name in new[] { "s282_mech_cutoff_rod", "s282_mech_cutoff_rod_LOD1", "s282_mech_lubricator_rod",
            "s282_mech_lubricator_rod_LOD1", "s282_mech_lubricator_part_1", "s282_mech_lubricator_part_1_LOD1" })
        {
            var native = data["nativeMeshes"].Children<JProperty>().Select(p => p.Value).Single(v => (string)v["name"] == name);
            var vertices = native["vertices"].Select(v => new Vector3((float)v[0], (float)v[1], (float)v[2])).ToArray();
            var triangles = native["triangles"].Select(v => (int)v).ToArray();
            var loader = new OBJLoader();
            // Omit shader creation only. The real importer/mesh/submesh path is
            // unchanged and the production proxy supplies the native materials.
            typeof(OBJLoader).GetField("Materials", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(loader, new Dictionary<string, Material> { ["default"] = null });
            var mesh = loader.Load(Path.Combine((string)config["exports"], name + ".obj"));
            if (mesh == null || mesh.vertexCount == 0) throw new InvalidOperationException("Empty import: " + name);
            normalize.Invoke(null, new object[] { mesh });
            var imported = mesh.vertices;
            int Closest(Vector3 point)
            {
                int closest = 0; float distance = float.PositiveInfinity;
                for (int i = 0; i < vertices.Length; i++)
                {
                    float d = (point - vertices[i]).sqrMagnitude;
                    if (d < distance) { distance = d; closest = i; }
                }
                maximum = Mathf.Max(maximum, Mathf.Sqrt(distance));
                if (distance > 9e-12f) throw new InvalidOperationException("Import vertex differs from native: " + name);
                // Native vertex splits at normals/UVs share the same position.
                for (int i = 0; i < closest; i++) if ((vertices[i] - vertices[closest]).sqrMagnitude < 1e-12f) return i;
                return closest;
            }
            string Face(int a, int b, int c)
            {
                if (a <= b && a <= c) return a + "," + b + "," + c;
                if (b <= a && b <= c) return b + "," + c + "," + a;
                return c + "," + a + "," + b;
            }
            string[] Faces(Vector3[] points, int[] indices)
            {
                var mapped = points.Select(Closest).ToArray();
                return Enumerable.Range(0, indices.Length / 3).Select(i => Face(mapped[indices[i * 3]], mapped[indices[i * 3 + 1]], mapped[indices[i * 3 + 2]])).OrderBy(v => v).ToArray();
            }
            if (!Faces(imported, mesh.triangles).SequenceEqual(Faces(vertices, triangles)))
                throw new InvalidOperationException("Imported native topology/winding differs: " + name);
            if (mesh.subMeshCount != 1) throw new InvalidOperationException("Unexpected linkage material sections: " + name);
            rows.Add(new JObject { ["mesh"] = name, ["vertices"] = mesh.vertexCount, ["triangles"] = mesh.triangles.Length / 3,
                ["submeshes"] = mesh.subMeshCount, ["nativeGeometryAndWindingMatched"] = true });
            UnityEngine.Object.Destroy(mesh);
        }
        File.WriteAllText(outputPath, new JObject { ["passed"] = true, ["actualUnityImporter"] = true,
            ["meshes"] = rows, ["maximumVertexErrorM"] = maximum, ["gameplayValidated"] = false,
            ["limits"] = "Actual compiled OBJ importer and linkage normalization in isolated Unity 2019.4 player. Null material slots avoid shader creation. No DV, live Animator, ratchet or camera test." }.ToString());
    }
}
