#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dummiesman;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RearrangedS282
{
    // Read completed native articulation/retarget poses. This deliberately runs
    // after their default-order LateUpdates; it never owns a hinge or rigidbody.
    [DefaultExecutionOrder(30000)]
    internal sealed class SteamPipePresentation : MonoBehaviour
    {
        internal sealed class NativePipe
        {
            internal Mesh Source;
            internal Vector3[] Vertices;
            internal Material[] Materials;
            internal Vector3 Lower;
            internal float Low, High;
            internal bool Mirrored;
            internal LODGroup Owner;
            internal int Side => Lower.x >= 0f ? 1 : -1;
        }
        internal sealed class Cylinder
        {
            internal Transform Part;
            internal Vector3 Socket;
            internal Vector3 Forward = Vector3.forward, Up = Vector3.up;
            internal float Scale;
            internal int Side;
            internal TrainCar Car;
            internal Matrix4x4 Previous;
            internal Transform Manifold;
            internal Run Branch;
        }
        internal sealed class Run
        {
            internal Mesh Mesh;
            internal Vector3[] Source, Work;
            internal int[] Front, Rear;
            internal bool HasUv;
            internal readonly SteamPipeShape.Sweep Path = new SteamPipeShape.Sweep();
            internal void Upload() { Mesh.vertices = Work; Mesh.RecalculateBounds(); Mesh.RecalculateNormals(); if (HasUv) Mesh.RecalculateTangents(); }
            internal float Gap()
            {
                Vector3 Mean(int[] ids) { var sum = Vector3.zero; foreach (int i in ids) sum += Work[i]; return sum / ids.Length; }
                return Math.Max((Mean(Front) - Path.Start).magnitude, (Mean(Rear) - Path.End).magnitude);
            }
        }
        private sealed class Header { internal NativePipe Native; internal Vector3 Boiler; internal Run Pipe; internal Cylinder[] Cylinders; internal float Scale => Math.Max(MinimumVisualScale, Cylinders.Max(c => c.Scale)); }
        private sealed class Direct { internal Cylinder Cylinder; internal Run Pipe; internal Vector3 Start, Direction; internal float Scale; internal string Style = "curved"; internal bool Plain; }
        private readonly List<Direct> direct = new List<Direct>();
        internal const float MinimumVisualScale = .8f;
        private static Mesh manifoldAsset, transferAsset;
        private readonly List<Mesh> owned = new List<Mesh>();
        private readonly List<MeshRenderer> renderers = new List<MeshRenderer>();
        private Cylinder[] cylinders;
        private readonly List<Header> headers = new List<Header>();
        private readonly List<Run> boilerFeeds = new List<Run>();
        private Transform body;
        private TrainCar locomotive;
        private LODGroup lod;
        private LOD[] originalLods;
        private Vector3 originalReference;
        private float originalSize;
        private bool initialized;
        internal Action Failed;
        internal string Error { get; private set; }
        internal int CylinderCount => cylinders?.Length ?? 0;
        internal int PieceCount => renderers.Count;
        internal float MaximumGapM { get; private set; }
        internal int ColliderCount => GetComponentsInChildren<Collider>(true).Length;
        internal bool UsesHeader => headers.Count != 0;
        internal bool UsesDirectRigid => direct.Count != 0;
        internal string DirectStyles => string.Join(", ", direct.Select(d => (d.Cylinder.Side > 0 ? "right " : "left ") + d.Style));
        internal float MinimumShaftDiameterM => .2032f * (direct.Count != 0 ? direct.Min(d => d.Scale) : headers.Min(h => h.Scale));
        internal IEnumerable<(int Side, Vector3 Socket, Vector3 Boiler)> ConnectionPoints => (cylinders ?? Array.Empty<Cylinder>())
            .Select(c => (c.Side, c.Previous.MultiplyPoint3x4(c.Socket), direct.Count != 0
                ? direct.Single(d => d.Cylinder == c).Start
                : headers.Single(h => h.Native.Side == c.Side).Boiler));
        internal IEnumerable<Renderer> Renderers => renderers;

        internal static Mesh ManifoldAsset => manifoldAsset != null ? manifoldAsset : manifoldAsset = Load("steam_manifold.obj");
        internal static Mesh TransferAsset => transferAsset != null ? transferAsset : transferAsset = Load("steam_transfer.obj");
        private static Mesh Load(string filename)
        {
            var mesh = new OBJLoader().Load(Path.Combine(Main.ModPath, "assets", filename));
            if (mesh == null || !mesh.isReadable || mesh.vertexCount == 0) throw new InvalidOperationException("Missing/read-protected original steam asset: " + filename);
            return mesh;
        }

        internal void Initialize(Transform frame, TrainCar car, Cylinder[] targets, NativePipe[] native, bool articulated)
        {
            body = frame; locomotive = car; cylinders = targets;
            if (!articulated)
            {
                // Every rigid engine gets one compact branch per side. The
                // original boiler fitting stays fixed; only its shaft is replaced.
                foreach (var target in cylinders)
                {
                    var source = native.Single(p => p.Side == target.Side);
                    NativeNeck(source, out var start, out var direction, out float radius);
                    var pipe = NewRun("compact rigid steam branch", source.Materials);
                    direct.Add(new Direct { Cylinder = target, Pipe = pipe, Start = start, Direction = direction,
                        Scale = Math.Max(MinimumVisualScale, target.Scale) });
                }
                Tick(true);
            }
            else
            {
                _ = TransferAsset;
                foreach (var target in cylinders)
                {
                    var material = native.Single(p => p.Side == target.Side).Materials;
                    target.Manifold = Visual("original Franklin intake manifold", ManifoldAsset, material);
                }
                foreach (var source in native)
                {
                    var side = cylinders.Where(c => c.Side == source.Side).ToArray();
                    float width = Math.Max(MinimumVisualScale, side.Max(c => c.Scale));
                    NativeNeck(source, out var attachment, out _, out _);
                    var feed = NewRun("boiler header feed", source.Materials, false, true);
                    var inlet = new Vector3(source.Side * 1.16f, 1.95f, source.Lower.z);
                    feed.Path.BuildStraight(attachment, inlet, width);
                    feed.Path.Deform(feed.Source, feed.Work); feed.Upload(); boilerFeeds.Add(feed);
                    foreach (var target in side) target.Branch = NewRun("articulating steam branch", source.Materials);
                    headers.Add(new Header { Native = source, Boiler = attachment, Pipe = NewRun("main steam header", source.Materials, true), Cylinders = side });
                }
                Tick(true);
            }
            // Use one existing native owner, retaining its transition/cull/fade
            // settings. Never create an independent distant floating-pipe LOD.
            lod = native[0].Owner;
            if (lod == null) throw new InvalidOperationException("Native steam-pipe LOD owner is unavailable.");
            originalLods = lod.GetLODs(); originalReference = lod.localReferencePoint; originalSize = lod.size;
            lod.SetLODs(originalLods.Select(level => new LOD(level.screenRelativeTransitionHeight,
                level.renderers.Length == 0 ? level.renderers : level.renderers.Concat(renderers).Distinct().ToArray())
                { fadeTransitionWidth = level.fadeTransitionWidth }).ToArray());
            RefreshBounds();
            initialized = true;
        }

        // Clip the original native shaft below its boiler collar. The new
        // attachment and tangent come from two actual mesh sections, not a guessed
        // topmost vertex (the boiler flange is inclined, not a horizontal ring).
        private Mesh NativeNeck(NativePipe source, out Vector3 joint, out Vector3 direction, out float radius)
        {
            float cut = source.High - .28f;
            Vector3 Section(float y, out float sectionRadius)
            {
                var crossings = new List<Vector3>(); var indices = source.Source.triangles;
                for (int i = 0; i < indices.Length; i += 3)
                    for (int e = 0; e < 3; e++)
                    {
                        var a = source.Vertices[indices[i + e]]; var b = source.Vertices[indices[i + (e + 1) % 3]];
                        if ((a.y < y) == (b.y < y) || Math.Abs(a.y - b.y) < 1e-8f) continue;
                        crossings.Add(a + (b - a) * ((y - a.y) / (b.y - a.y)));
                    }
                if (crossings.Count < 6) throw new InvalidOperationException("Native boiler neck has no usable shaft section.");
                var centre = new Vector3((crossings.Min(v => v.x) + crossings.Max(v => v.x)) * .5f, y,
                    (crossings.Min(v => v.z) + crossings.Max(v => v.z)) * .5f);
                sectionRadius = crossings.Max(v => (v - centre).magnitude);
                return centre;
            }
            joint = Section(cut, out radius);
            direction = SteamPipeShape.Unit(joint - Section(cut + .015f, out _));
            var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
            var originalUv = source.Source.uv; bool hasUv = originalUv.Length == source.Vertices.Length;
            var faces = source.Source.triangles;
            for (int i = 0; i < faces.Length; i += 3)
            {
                var polygon = new List<(Vector3 p, Vector2 uv)>();
                for (int e = 0; e < 3; e++)
                {
                    int ia = faces[i + e], ib = faces[i + (e + 1) % 3];
                    var a = source.Vertices[ia]; var b = source.Vertices[ib];
                    var auv = hasUv ? originalUv[ia] : Vector2.zero; var buv = hasUv ? originalUv[ib] : Vector2.zero;
                    if (a.y >= cut) polygon.Add((a, auv));
                    if ((a.y >= cut) == (b.y >= cut)) continue;
                    float t = (cut - a.y) / (b.y - a.y);
                    polygon.Add((a + (b - a) * t, auv + (buv - auv) * t));
                }
                for (int e = 1; e + 1 < polygon.Count; e++)
                {
                    int begin = vertices.Count;
                    foreach (int k in new[] { 0, e, e + 1 }) { vertices.Add(polygon[k].p); uv.Add(polygon[k].uv); }
                    triangles.Add(begin); triangles.Add(source.Mirrored ? begin + 2 : begin + 1); triangles.Add(source.Mirrored ? begin + 1 : begin + 2);
                }
            }
            var centreOfCut = joint;
            var boundary = vertices.Where(v => Math.Abs(v.y - cut) < .00001f).Distinct()
                .OrderBy(v => Math.Atan2(v.z - centreOfCut.z, v.x - centreOfCut.x)).ToArray();
            for (int i = 0; i < boundary.Length; i++)
            {
                int begin = vertices.Count; vertices.Add(joint); vertices.Add(boundary[i]); vertices.Add(boundary[(i + 1) % boundary.Length]);
                uv.Add(Vector2.zero); uv.Add(Vector2.zero); uv.Add(Vector2.zero);
                triangles.Add(begin); triangles.Add(begin + 1); triangles.Add(begin + 2);
            }
            // Weld only equal position/UV pairs. Triangle clipping otherwise
            // duplicates every corner and would flat-shade the retained neck.
            var welded = new Dictionary<(Vector3 p, Vector2 uv), int>();
            var smoothVertices = new List<Vector3>(); var smoothUv = new List<Vector2>();
            var remap = new int[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                var key = (vertices[i], uv[i]);
                if (!welded.TryGetValue(key, out int id))
                { id = smoothVertices.Count; welded.Add(key, id); smoothVertices.Add(vertices[i]); smoothUv.Add(uv[i]); }
                remap[i] = id;
            }
            var mesh = Object.Instantiate(source.Source); owned.Add(mesh); mesh.Clear();
            mesh.name = "[Rearranged steam] original boiler collar and neck";
            mesh.vertices = smoothVertices.ToArray(); mesh.triangles = triangles.Select(i => remap[i]).ToArray(); if (hasUv) mesh.uv = smoothUv.ToArray();
            mesh.RecalculateBounds(); mesh.RecalculateNormals(); if (hasUv) mesh.RecalculateTangents();
            Visual("original boiler fitting", mesh, source.Materials); return mesh;
        }

        private Mesh NativeCopy(NativePipe source)
        {
            var mesh = Object.Instantiate(source.Source); owned.Add(mesh);
            mesh.name = "[Rearranged steam] native branch, intact fittings";
            if (source.Mirrored)
            {
                var triangles = mesh.triangles;
                for (int i = 0; i < triangles.Length; i += 3) { int a = triangles[i]; triangles[i] = triangles[i + 1]; triangles[i + 1] = a; }
                mesh.triangles = triangles;
            }
            Visual("boiler branch", mesh, source.Materials);
            return mesh;
        }

        private Transform Visual(string name, Mesh mesh, Material[] materials)
        {
            var part = new GameObject("[Rearranged steam] " + name).transform;
            part.SetParent(transform, false); part.gameObject.layer = body.gameObject.layer;
            part.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = part.gameObject.AddComponent<MeshRenderer>(); renderer.sharedMaterials = materials;
            renderers.Add(renderer); return part;
        }

        private Run NewRun(string name, Material[] materials, bool closeFront = false, bool plain = false)
        {
            var mesh = Object.Instantiate(TransferAsset); owned.Add(mesh);
            var topology = SteamPipeShape.DenseTransfer(mesh.vertices, mesh.triangles, mesh.uv, retainCollars: !plain);
            if (closeFront)
            {
                // The main header is fed through its side by the native boiler
                // neck. Close its unused front bore instead of showing an open
                // flange ahead of that T connection.
                var vertices = topology.Vertices.ToList(); var uv = topology.UV.ToList(); var faces = topology.Triangles.ToList();
                var ring = Enumerable.Range(0, vertices.Count).Where(i => Math.Abs(vertices[i].z - SteamPipeShape.SourceFrontZ) < .00001f)
                    .GroupBy(i => vertices[i]).Select(g => g.First()).OrderBy(i => Math.Atan2(vertices[i].y, vertices[i].x)).ToArray();
                int centre = vertices.Count; vertices.Add(new Vector3(0f, 0f, SteamPipeShape.SourceFrontZ));
                uv.Add(ring.Aggregate(Vector2.zero, (sum, i) => sum + uv[i]) * (1f / ring.Length));
                for (int i = 0; i < ring.Length; i++) { faces.Add(centre); faces.Add(ring[i]); faces.Add(ring[(i + 1) % ring.Length]); }
                topology.Vertices = vertices.ToArray(); topology.UV = uv.ToArray(); topology.Triangles = faces.ToArray();
            }
            mesh.Clear(); mesh.vertices = topology.Vertices; mesh.triangles = topology.Triangles; mesh.uv = topology.UV;
            var source = mesh.vertices; var ids = Enumerable.Range(0, source.Length).ToArray();
            int[] Ring(float z) => ids.Where(i => Math.Abs(source[i].z - z) < .00001f)
                .GroupBy(i => source[i]).Select(g => g.First()).ToArray();
            var run = new Run { Mesh = mesh, Source = source, Work = new Vector3[source.Length],
                HasUv = mesh.uv.Length == source.Length,
                Front = Ring(SteamPipeShape.SourceFrontZ), Rear = Ring(SteamPipeShape.SourceRearZ) };
            if (run.Front.Length < 3 || run.Rear.Length < 3) throw new InvalidOperationException("Original steam pipe collars could not be measured.");
            Visual(name, mesh, materials); return run;
        }

        private void LateUpdate()
        {
            if (!initialized || Error != null || headers.Count == 0 && direct.Count == 0) return;
            try { Tick(false); }
            catch (Exception error)
            {
                Error = error.Message;
                gameObject.SetActive(false);
                Failed?.Invoke();
                Main.Logger.Warning("Steam presentation stopped; native simulation unaffected: " + error);
            }
        }

        private static bool Same(Matrix4x4 a, Matrix4x4 b)
        {
            for (int i = 0; i < 16; i++) if (Math.Abs(a[i] - b[i]) > .00002f) return false;
            return true;
        }

        private void Tick(bool force)
        {
            bool changed = force;
            foreach (var cylinder in cylinders)
            {
                if (cylinder.Part == null) throw new InvalidOperationException("A steam-connected cylinder was unloaded.");
                var frame = body.worldToLocalMatrix * cylinder.Part.localToWorldMatrix;
                changed |= !Same(frame, cylinder.Previous); cylinder.Previous = frame;
            }
            if (!changed) return;
            MaximumGapM = boilerFeeds.Count == 0 ? 0f : boilerFeeds.Max(feed => feed.Gap());
            foreach (var branch in direct)
            {
                var target = branch.Cylinder;
                var end = target.Previous.MultiplyPoint3x4(target.Socket);
                var up = SteamPipeShape.Unit(target.Previous.MultiplyVector(target.Up));
                bool curved = false;
                float distance = (end - branch.Start).magnitude;
                // A compact branch must not loop merely to preserve optional
                // transfer flanges. Try bounded smooth elbows where there is room.
                if (!branch.Plain && branch.Start.y - end.y > .45f)
                {
                    foreach (float factor in new[] { .18f, .25f, .34f, .45f, .6f })
                    {
                        float handle = Math.Max(.08f, Math.Min(.85f, distance * factor));
                        try { branch.Pipe.Path.Build(branch.Start, branch.Direction, end, -up, Vector3.up, branch.Scale, handle); curved = true; break; }
                        catch (InvalidOperationException error) when (error.Message == "Steam pipe bend is too tight for its diameter." ||
                            error.Message == "Steam pipe reverses within a collar-sized bend." || error.Message == "Invalid steam pipe run length.") { }
                    }
                }
                if (!curved)
                {
                    if (!branch.Plain)
                    {
                        var topology = SteamPipeShape.DenseTransfer(TransferAsset.vertices, TransferAsset.triangles, TransferAsset.uv, retainCollars: false);
                        branch.Pipe.Mesh.Clear(); branch.Pipe.Mesh.vertices = topology.Vertices;
                        branch.Pipe.Mesh.triangles = topology.Triangles; branch.Pipe.Mesh.uv = topology.UV;
                        branch.Pipe.Source = topology.Vertices; branch.Pipe.Work = new Vector3[topology.Vertices.Length];
                        var all = Enumerable.Range(0, topology.Vertices.Length);
                        branch.Pipe.Front = all.Where(i => Math.Abs(topology.Vertices[i].z - SteamPipeShape.SourceFrontZ) < .00001f)
                            .GroupBy(i => topology.Vertices[i]).Select(g => g.First()).ToArray();
                        branch.Pipe.Rear = all.Where(i => Math.Abs(topology.Vertices[i].z - SteamPipeShape.SourceRearZ) < .00001f)
                            .GroupBy(i => topology.Vertices[i]).Select(g => g.First()).ToArray();
                        branch.Plain = true; branch.Style = "seated straight sleeve";
                    }
                    branch.Pipe.Path.BuildStraight(branch.Start, end, branch.Scale);
                }
                branch.Pipe.Path.Deform(branch.Pipe.Source, branch.Pipe.Work); branch.Pipe.Upload();
                MaximumGapM = Math.Max(MaximumGapM, branch.Pipe.Gap());
            }
            foreach (var cylinder in cylinders.Where(c => c.Manifold != null))
            {
                var forward = SteamPipeShape.Unit(cylinder.Previous.MultiplyVector(cylinder.Forward));
                var up = SteamPipeShape.Unit(cylinder.Previous.MultiplyVector(cylinder.Up));
                // Face the final manifold towards the preceding engines. The
                // header enters naturally from the front, without a 180-degree
                // loop behind the rear chest or an open extension into the cab.
                var socket = cylinder.Previous.MultiplyPoint3x4(cylinder.Socket);
                bool rear = !cylinders.Any(other => other.Side == cylinder.Side &&
                    other.Previous.MultiplyPoint3x4(other.Socket).z < socket.z - .001f);
                var rotation = Quaternion.LookRotation(rear ? forward : -forward, up);
                float visualScale = Math.Max(MinimumVisualScale, cylinder.Scale);
                var scale = new Vector3(cylinder.Side * visualScale, visualScale, visualScale);
                cylinder.Manifold.localRotation = rotation; cylinder.Manifold.localScale = scale;
                cylinder.Manifold.localPosition = socket - rotation * Vector3.Scale(SteamPipeShape.NativeManifoldLower, scale);
                var drawn = transform.InverseTransformPoint(cylinder.Manifold.TransformPoint(SteamPipeShape.NativeManifoldLower));
                MaximumGapM = Math.Max(MaximumGapM, (drawn - socket).magnitude);
            }
            foreach (var header in headers)
            {
                var side = header.Cylinders;
                var last = side[0]; var lastInlet = transform.InverseTransformPoint(last.Manifold.TransformPoint(SteamPipeShape.NativeManifoldInlet));
                foreach (var target in side)
                {
                    var inlet = transform.InverseTransformPoint(target.Manifold.TransformPoint(SteamPipeShape.NativeManifoldInlet));
                    if (inlet.z < lastInlet.z) { last = target; lastInlet = inlet; }
                }
                // Terminate at the final manifold rather than leaving an open
                // extension beyond the rear engine (and never bridge to a tender).
                var end = lastInlet;
                var direction = -SteamPipeShape.Unit(last.Previous.MultiplyVector(last.Forward));
                var start = new Vector3(header.Native.Side * 1.16f, 1.95f, header.Native.Lower.z + .16f);
                float handle = Math.Max(.6f, Math.Min(1.2f, (end - start).magnitude * .15f));
                header.Pipe.Path.Build(start, -Vector3.forward, end, direction, Vector3.up, header.Scale, handle);
                header.Pipe.Path.Deform(header.Pipe.Source, header.Pipe.Work); header.Pipe.Upload();
                MaximumGapM = Math.Max(MaximumGapM, header.Pipe.Gap());
                foreach (var cylinder in side)
                {
                    if (cylinder == last) { cylinder.Branch.Mesh.Clear(); continue; }
                    var inlet = transform.InverseTransformPoint(cylinder.Manifold.TransformPoint(SteamPipeShape.NativeManifoldInlet));
                    var f = -SteamPipeShape.Unit(cylinder.Previous.MultiplyVector(cylinder.Forward));
                    var joint = header.Pipe.Path.Closest(inlet + f * 1.2f);
                    var arrival = Math.Abs(joint.y - inlet.y) < .3f ? -Vector3.forward : joint.y >= inlet.y ? Vector3.up : -Vector3.up;
                    bool found = false;
                    foreach (float branchHandle in new[] { .22f, .3f, .4f, .52f })
                    {
                        try { cylinder.Branch.Path.Build(inlet, f, joint, arrival, Vector3.up, Math.Max(MinimumVisualScale, cylinder.Scale), branchHandle); found = true; break; }
                        catch (InvalidOperationException error) when (error.Message == "Steam pipe bend is too tight for its diameter." ||
                            error.Message == "Steam pipe reverses within a collar-sized bend." || error.Message == "Invalid steam pipe run length.") { }
                    }
                    if (!found) throw new InvalidOperationException("Articulating steam branch cannot retain a safe bend radius at its measured sockets.");
                    cylinder.Branch.Path.Deform(cylinder.Branch.Source, cylinder.Branch.Work); cylinder.Branch.Upload();
                    MaximumGapM = Math.Max(MaximumGapM, cylinder.Branch.Gap());
                }
            }
            if (lod != null) RefreshBounds();
        }

        private void RefreshBounds()
        {
            lod.RecalculateBounds();
            var a = new Bounds(originalReference, Vector3.one * originalSize);
            var b = new Bounds(lod.localReferencePoint, Vector3.one * lod.size);
            a.Encapsulate(b); a.Expand(.2f);
            lod.localReferencePoint = a.center; lod.size = Math.Max(a.size.x, Math.Max(a.size.y, a.size.z));
        }

        internal void Release()
        {
            initialized = false;
            gameObject.SetActive(false);
            if (lod != null && originalLods != null) { lod.SetLODs(originalLods); lod.localReferencePoint = originalReference; lod.size = originalSize; }
            lod = null; originalLods = null;
            foreach (var mesh in owned) if (mesh != null) Object.Destroy(mesh);
            owned.Clear();
        }
        private void OnDestroy() => Release();
    }
}
#endif
