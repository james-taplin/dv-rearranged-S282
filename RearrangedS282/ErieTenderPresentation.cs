#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RearrangedS282
{
    // Visual adaptation of the Erie's rear exhaust pipe, not a second boiler chimney.
    internal sealed class ErieTenderPresentation : MonoBehaviour
    {
        internal Transform Outlet { get; private set; }
        private Mesh mesh;
        private MeshRenderer pipe;
        private LODGroup lod;
        internal System.Action Releasing;

        internal void Initialize(TrainCar tender, Material[] materials)
        {
            // Outside the rear left tank wall, forward of the rear ladder.
            // The original deck position crossed the upper ladder/rung area.
            var bottom = new Vector3(-1.72f, 2.65f, -6.95f);
            var top = new Vector3(-1.72f, 4.05f, -6.95f);
            var source = SteamPipePresentation.TransferAsset;
            var nativeVertices = source.vertices; var nativeFaces = source.triangles; var nativeUV = source.uv;
            const float scale = .32f / .2032f;
            var vertices = new List<Vector3>(); var triangles = new List<int>(); var uv = new List<Vector2>();
            void Append(Vector3[] points, int[] faces, Vector2[] texture)
            {
                int offset = vertices.Count; vertices.AddRange(points); uv.AddRange(texture);
                triangles.AddRange(faces.Select(i => i + offset));
            }
            void Run(SteamPipeShape.Sweep path, bool collars, int segments)
            {
                var topology = SteamPipeShape.DenseTransfer(nativeVertices, nativeFaces, nativeUV, shaftSegments: segments, retainCollars: collars);
                var points = new Vector3[topology.Vertices.Length]; path.Deform(topology.Vertices, points);
                Append(points, topology.Triangles, topology.UV);
            }
            var upright = new SteamPipeShape.Sweep(); upright.BuildStraight(bottom, top, scale);
            Run(upright, true, 24);
            // Inlet starts inside the tank. The broad bend exits its side well
            // above the wheels; the existing sweep rejects folded inner walls.
            var elbow = new SteamPipeShape.Sweep();
            elbow.Build(new Vector3(-1.05f, 1.8f, -6.95f), -Vector3.right, bottom, Vector3.up, Vector3.forward, scale, .25f);
            Run(elbow, false, 48);
            // Reuse the original pipe's collar faces as two retaining bands.
            // Their inner wall seats on the shaft; outer edge meets tank side.
            var bandFaces = Enumerable.Range(0, nativeFaces.Length / 3)
                .SelectMany(i => nativeFaces.Skip(3 * i).Take(3).All(v => nativeVertices[v].z >= -.00001f)
                    ? nativeFaces.Skip(3 * i).Take(3) : Enumerable.Empty<int>()).ToArray();
            var ids = bandFaces.Distinct().ToArray();
            var index = ids.Select((id, i) => (id, i)).ToDictionary(p => p.id, p => p.i);
            foreach (float height in new[] { 2.85f, 3.4f })
            {
                var points = ids.Select(i => nativeVertices[i]).Select(v => new Vector3(bottom.x + v.x * scale,
                    height + (v.z - .0381f) * scale, bottom.z - v.y * scale)).ToArray();
                Append(points, bandFaces.Select(i => index[i]).ToArray(), ids.Select(i => nativeUV[i]).ToArray());
            }
            mesh = new Mesh { name = "Erie rear exhaust (original transfer pipe)" };
            mesh.vertices = vertices.ToArray(); mesh.triangles = triangles.ToArray(); mesh.uv = uv.ToArray();
            mesh.RecalculateBounds(); mesh.RecalculateNormals(); mesh.RecalculateTangents();
            var part = new GameObject("[Rearranged Erie] tender exhaust pipe");
            part.transform.SetParent(transform, false); part.layer = tender.gameObject.layer;
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            pipe = part.AddComponent<MeshRenderer>(); pipe.sharedMaterials = materials;
            Outlet = new GameObject("[Rearranged Erie] exhaust outlet").transform;
            Outlet.SetParent(transform, false); Outlet.localPosition = top;
            // Retain the native tender's distance culling, without replacing its LODs.
            lod = tender.transform.Find("LocoS282B_Body")?.GetComponent<LODGroup>();
            if (lod == null) lod = tender.GetComponentsInChildren<LODGroup>(true).FirstOrDefault(g => g.GetLODs().Any(l => l.renderers.Any(r => r != null && r.name.StartsWith("s282_tender"))));
            if (lod != null)
            {
                lod.SetLODs(lod.GetLODs().Select(l => new LOD(l.screenRelativeTransitionHeight,
                    l.renderers.Length == 0 ? l.renderers : l.renderers.Append(pipe).ToArray()) { fadeTransitionWidth = l.fadeTransitionWidth }).ToArray());
                lod.RecalculateBounds();
            }
        }

        internal void Release()
        {
            Outlet = null;
            var releasing = Releasing; Releasing = null;
            releasing?.Invoke();
            if (lod != null && pipe != null)
            {
                lod.SetLODs(lod.GetLODs().Select(l => new LOD(l.screenRelativeTransitionHeight,
                    l.renderers.Where(r => r != pipe).ToArray()) { fadeTransitionWidth = l.fadeTransitionWidth }).ToArray());
                lod.RecalculateBounds();
            }
            lod = null;
            if (mesh != null) Object.Destroy(mesh);
            mesh = null;
        }
        private void OnDisable() { Releasing?.Invoke(); }
        private void OnDestroy() { Release(); }
    }
}
#endif
