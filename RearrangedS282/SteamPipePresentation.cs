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
    // Follow the completed native articulation pose; never own physics or hinges.
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
            internal Vector3 Outward;
            internal Vector3[] CasingVertices;
            internal int[] CasingTriangles;
            internal bool ForwardBranch, CompactCollar;
            internal float Scale;
            internal int Side;
            internal TrainCar Car;
            internal Matrix4x4 Previous;
            internal Transform Manifold;
        }
        private sealed class Duct
        {
            internal Mesh Mesh;
            internal SteamDuctMesh.Builder Builder;
            internal void Upload()
            {
                Mesh.Clear(); Mesh.SetVertices(Builder.Vertices); Mesh.SetNormals(Builder.Normals);
                Mesh.SetUVs(0, Builder.UV); Mesh.SetTriangles(Builder.Triangles, 0);
                Mesh.RecalculateBounds(); Mesh.RecalculateTangents();
            }
        }
        private sealed class Direct
        {
            internal Cylinder Cylinder;
            internal NativePipe Native;
            internal Duct Pipe;
            internal Vector3 Start, Direction;
            internal float Radius;
            internal string Style;
        }
        private sealed class Header
        {
            internal NativePipe Native;
            internal Vector3 Boiler, Direction;
            internal float NativeRadius;
            internal Duct Pipe;
            internal Cylinder[] Cylinders;
        }
        internal const float MinimumVisualScale = .8f;
        private const float HeaderX = 1.30f, HeaderY = 2.05f;
        private static Mesh manifoldAsset, transferAsset;
        private static SteamDuctMesh.Template ductTemplate;
        private readonly List<Mesh> owned = new List<Mesh>();
        private readonly List<MeshRenderer> renderers = new List<MeshRenderer>();
        private readonly List<Direct> direct = new List<Direct>();
        private readonly List<Header> headers = new List<Header>();
        private Cylinder[] cylinders;
        private Transform body;
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
        // The floor applies to added transfer sections. Retained native boiler
        // castings keep their authored varying profile rather than being inflated.
        internal float MinimumShaftDiameterM => .2032f * MinimumVisualScale;
        internal IEnumerable<(int Side, Vector3 Socket, Vector3 Boiler)> ConnectionPoints => (cylinders ?? Array.Empty<Cylinder>())
            .Select(c => (c.Side, c.Previous.MultiplyPoint3x4(c.Socket), direct.Count != 0
                ? direct.Single(d => d.Cylinder == c).Start : headers.Single(h => h.Native.Side == c.Side).Boiler));
        internal IEnumerable<Renderer> Renderers => renderers;
        internal static Mesh ManifoldAsset => manifoldAsset != null ? manifoldAsset : manifoldAsset = Load("steam_manifold.obj");
        internal static Mesh TransferAsset => transferAsset != null ? transferAsset : transferAsset = Load("steam_transfer.obj");
        private static SteamDuctMesh.Template Template => ductTemplate ?? (ductTemplate = new SteamDuctMesh.Template(TransferAsset.vertices, TransferAsset.triangles, TransferAsset.uv));
        private static Mesh Load(string filename)
        {
            var mesh = new OBJLoader().Load(Path.Combine(Main.ModPath, "assets", filename));
            if (mesh == null || !mesh.isReadable || mesh.vertexCount == 0) throw new InvalidOperationException("Missing/read-protected original steam asset: " + filename);
            return mesh;
        }

        internal void Initialize(Transform frame, TrainCar car, Cylinder[] targets, NativePipe[] native, bool articulated)
        {
            body = frame; cylinders = targets;
            foreach (var c in cylinders) c.Previous = body.worldToLocalMatrix * c.Part.localToWorldMatrix;
            if (!articulated)
            {
                foreach (var c in cylinders)
                {
                    var source = native.Single(p => p.Side == c.Side);
                    if (c.CompactCollar) source = new NativePipe { Source = source.Source,
                        Vertices = SteamPipeLowerCollar.CompactLowerCollar(source.Vertices, source.Source.triangles),
                        Materials = source.Materials, Lower = source.Lower, Low = source.Low, High = source.High,
                        Mirrored = source.Mirrored, Owner = source.Owner };
                    var d = new Direct { Cylinder = c, Native = source };
                    var casing = c.CasingVertices?.Select(c.Previous.MultiplyPoint3x4).ToArray();
                    if (!c.ForwardBranch && casing != null && SteamPipeNativeFit.TryFind(source.Vertices, source.Source.triangles,
                        casing, c.CasingTriangles, out var fit))
                    {
                        NativeNeck(source, fit.CutY, true, out d.Start, out d.Direction, out d.Radius);
                        c.Socket = c.Previous.inverse.MultiplyPoint3x4(d.Start);
                        d.Style = "original branch seated in casting";
                    }
                    else
                    {
                        float cut = c.ForwardBranch ? 2.02f : 1.72f;
                        NativeNeck(source, cut, false, out d.Start, out d.Direction, out d.Radius);
                        d.Style = c.ForwardBranch ? "original-profile offset casting" : "original-profile round elbows";
                        d.Pipe = NewDuct("matched rigid steam branch", source.Materials);
                    }
                    direct.Add(d);
                }
            }
            else foreach (var source in native)
            {
                var side = cylinders.Where(c => c.Side == source.Side).ToArray();
                // The broad original casting fits the rear engine. Its long base
                // must not be copied onto the front pilot or through a reservoir.
                var rear = side.OrderBy(c => c.Previous.MultiplyPoint3x4(c.Socket).z).First();
                rear.Manifold = Visual("original rear intake manifold", ManifoldAsset, source.Materials);
                foreach (var c in side.Where(c => c != rear))
                {
                    // Measure a port on the outboard face of the actual resized
                    // chest. A tall top riser crosses compressor pipes and the
                    // front steps, especially on the low Big Mallet cylinders.
                    if (!OutboardPort(c, out var socket)) throw new InvalidOperationException("An articulated cylinder has no measured outboard steam port.");
                    c.Socket = c.Previous.inverse.MultiplyPoint3x4(socket);
                    c.Outward = c.Previous.inverse.MultiplyVector(Vector3.right * c.Side);
                }
                NativeNeck(source, 1.72f, false, out var attachment, out var direction, out float radius);
                headers.Add(new Header { Native = source, Boiler = attachment, Direction = direction, NativeRadius = radius,
                    Pipe = NewDuct("matched header and cylinder branches", source.Materials), Cylinders = side });
            }
            Tick(true);
            lod = native[0].Owner;
            if (lod == null) throw new InvalidOperationException("Native steam-pipe LOD owner is unavailable.");
            originalLods = lod.GetLODs(); originalReference = lod.localReferencePoint; originalSize = lod.size;
            lod.SetLODs(originalLods.Select(level => new LOD(level.screenRelativeTransitionHeight,
                level.renderers.Length == 0 ? level.renderers : level.renderers.Concat(renderers).Distinct().ToArray())
                { fadeTransitionWidth = level.fadeTransitionWidth }).ToArray());
            RefreshBounds(); initialized = true;
        }

        private Mesh NativeNeck(NativePipe source, float cut, bool cap, out Vector3 joint, out Vector3 direction, out float radius)
        {
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
            if (SteamPipeNativeFit.TrySection(source.Vertices, source.Source.triangles, source.Source.uv, cut, out var measured))
            {
                var centre = joint; var axis = direction;
                radius = measured.Points.Max(p => Vector3.Cross(p - centre, axis).magnitude);
            }
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
            for (int i = 0; cap && i < boundary.Length; i++)
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

        private Transform Visual(string name, Mesh mesh, Material[] materials)
        {
            var part = new GameObject("[Rearranged steam] " + name).transform;
            part.SetParent(transform, false); part.gameObject.layer = body.gameObject.layer;
            part.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = part.gameObject.AddComponent<MeshRenderer>(); renderer.sharedMaterials = materials;
            renderers.Add(renderer); return part;
        }
        private Duct NewDuct(string name, Material[] materials)
        {
            var mesh = new Mesh { name = "[Rearranged steam] " + name }; mesh.MarkDynamic(); owned.Add(mesh);
            Visual(name, mesh, materials); return new Duct { Mesh = mesh, Builder = new SteamDuctMesh.Builder(Template) };
        }
        private void LateUpdate()
        {
            if (!initialized || Error != null) return;
            try { Tick(false); }
            catch (Exception error)
            {
                Error = error.Message; gameObject.SetActive(false); Failed?.Invoke();
                Main.Logger.Warning("Steam presentation stopped; native simulation unaffected: " + error);
            }
        }
        private static bool Same(Matrix4x4 a, Matrix4x4 b)
        {
            for (int i = 0; i < 16; i++) if (Math.Abs(a[i] - b[i]) > .00002f) return false;
            return true;
        }
        private static Vector3 Unit(Vector3 v) => SteamPipeShape.Unit(v);
        private static bool OutboardPort(Cylinder c, out Vector3 socket)
        {
            socket = default;
            if (c.CasingVertices == null || c.CasingTriangles == null) return false;
            var vertices = c.CasingVertices.Select(c.Previous.MultiplyPoint3x4).ToArray();
            var top = c.Previous.MultiplyPoint3x4(c.Socket);
            float y = top.y - .16f, z = top.z, outer = float.NegativeInfinity;
            for (int i = 0; i < c.CasingTriangles.Length; i += 3)
            {
                var a = vertices[c.CasingTriangles[i]]; var b = vertices[c.CasingTriangles[i + 1]]; var d = vertices[c.CasingTriangles[i + 2]];
                float denominator = (b.z - d.z) * (a.y - d.y) + (d.y - b.y) * (a.z - d.z);
                if (Math.Abs(denominator) < 1e-9f) continue;
                float u = ((b.z - d.z) * (y - d.y) + (d.y - b.y) * (z - d.z)) / denominator;
                float v = ((d.z - a.z) * (y - d.y) + (a.y - d.y) * (z - d.z)) / denominator;
                float w = 1f - u - v;
                if (u < -.0001f || v < -.0001f || w < -.0001f) continue;
                outer = Math.Max(outer, c.Side * (u * a.x + v * b.x + w * d.x));
            }
            if (float.IsNegativeInfinity(outer)) return false;
            socket = new Vector3(c.Side * outer, y, z); return true;
        }
        private static float Radius(Cylinder c) => .1016f * Math.Max(MinimumVisualScale, c.Scale);
        private void EndGap(SteamDuctMesh.Builder b, Vector3 start, Vector3 end)
        {
            MaximumGapM = Math.Max(MaximumGapM, Math.Max((b.LastStart - start).magnitude, (b.LastEnd - end).magnitude));
        }
        private static void Rounded(SteamDuctMesh.Builder b, Vector3[] points, float radius, bool capStart = false, bool capEnd = false)
        {
            // Prefer generous circular elbows, reducing only their centreline
            // radius where the measured span is short. Never shrink the bore.
            foreach (float ratio in new[] { 2.25f, 1.9f, 1.65f, 1.5001f })
            {
                try { b.Tube(points, radius, radius * ratio, capStart, capEnd); return; }
                catch (InvalidOperationException error) when (error.Message == "Steam duct route is too short to seat its round elbows.") { }
            }
            throw new InvalidOperationException("Measured steam route has no room for a round elbow at its retained diameter.");
        }
        private static Vector3 FeedStart(SteamDuctMesh.Builder b, Vector3 joint, Vector3 direction, float nativeRadius, float radius)
        {
            var start = joint - direction * .015f;
            // Keep the small native shaft, then use a visible gradual reducer
            // into the authored transfer profile. The overlap is inside the neck.
            float neckRadius = Math.Min(nativeRadius, .0741f);
            float length = Math.Max(.06f, 2.1f * Math.Abs(radius - neckRadius));
            b.Reducer(start - direction * length, start, neckRadius, radius);
            return start;
        }
        private void Tick(bool force)
        {
            bool changed = force;
            foreach (var c in cylinders)
            {
                if (c.Part == null) throw new InvalidOperationException("A steam-connected cylinder was unloaded.");
                var frame = body.worldToLocalMatrix * c.Part.localToWorldMatrix;
                changed |= !Same(frame, c.Previous); c.Previous = frame;
            }
            if (!changed) return;
            MaximumGapM = 0f;
            foreach (var d in direct)
            {
                if (d.Pipe == null) continue;
                var b = d.Pipe.Builder; b.Clear();
                var c = d.Cylinder; var socket = c.Previous.MultiplyPoint3x4(c.Socket);
                float radius = Radius(c);
                if (c.ForwardBranch)
                {
                    if (!SteamPipeNativeFit.TrySection(d.Native.Vertices, d.Native.Source.triangles, d.Native.Source.uv, 2.02f, out var section))
                        throw new InvalidOperationException("Native Prairie adapter profile is unavailable.");
                    b.Adapter(section.Points, section.UV, socket, radius, section.EdgeEndUV, Vector3.right * (c.Side * .22f));
                    MaximumGapM = Math.Max(MaximumGapM, b.LastAdapterMaximumGapM);
                    b.Collar(socket + Unit(c.Previous.MultiplyVector(c.Up)) * .02f, Unit(c.Previous.MultiplyVector(c.Up)), radius);
                }
                else
                {
                    var start = FeedStart(b, d.Start, d.Direction, d.Radius, radius);
                    var up = Unit(c.Previous.MultiplyVector(c.Up));
                    var first = d.Start + d.Direction * .14f;
                    var last = socket + up * .18f;
                    Rounded(b, new[] { start, first, last, socket }, radius);
                    EndGap(b, start, socket);
                    b.Collar(socket + up * .018f, up, radius);
                }
                d.Pipe.Upload();
            }
            foreach (var h in headers)
            {
                var b = h.Pipe.Builder; b.Clear();
                float radius = .1016f * Math.Max(MinimumVisualScale, h.Cylinders.Max(c => c.Scale));
                float x = h.Native.Side * HeaderX;
                var start = FeedStart(b, h.Boiler, h.Direction, h.NativeRadius, radius);
                var low = h.Boiler + h.Direction * .115f;
                var boilerJoint = new Vector3(x, HeaderY, h.Boiler.z);
                Rounded(b, new[] { start, low, new Vector3(x, low.y, h.Boiler.z), boilerJoint }, radius);
                EndGap(b, start, boilerJoint);
                var joints = new List<Vector3> { boilerJoint };
                foreach (var c in h.Cylinders)
                {
                    var socket = c.Previous.MultiplyPoint3x4(c.Socket);
                    var up = Unit(c.Previous.MultiplyVector(c.Up));
                    float r = Radius(c);
                    if (c.Manifold != null)
                    {
                        var forward = Unit(c.Previous.MultiplyVector(c.Forward));
                        var rotation = Quaternion.LookRotation(forward, up);
                        float s = Math.Max(MinimumVisualScale, c.Scale);
                        var scale = new Vector3(c.Side * s, s, s);
                        c.Manifold.localRotation = rotation; c.Manifold.localScale = scale;
                        c.Manifold.localPosition = socket - rotation * Vector3.Scale(SteamPipeShape.NativeManifoldLower, scale);
                        var seated = transform.InverseTransformPoint(c.Manifold.TransformPoint(SteamPipeShape.NativeManifoldLower));
                        MaximumGapM = Math.Max(MaximumGapM, (seated - socket).magnitude);
                        var inlet = transform.InverseTransformPoint(c.Manifold.TransformPoint(SteamPipeShape.NativeManifoldInlet));
                        var lead = inlet + forward * .35f;
                        var joint = new Vector3(x, HeaderY, lead.z);
                        // One rounded turn into a short diagonal casting. A
                        // second right angle would force the header outboard.
                        Rounded(b, new[] { inlet, lead, joint }, r);
                        EndGap(b, inlet, joint); b.Collar(inlet + forward * .022f, forward, r); joints.Add(joint);
                    }
                    else
                    {
                        var outward = Unit(c.Previous.MultiplyVector(c.Outward));
                        float outerX = c.Side * Math.Max(HeaderX, Math.Abs(socket.x) + Math.Max(.10f, 1.6f * r));
                        // Compact side entry, with a small buried overlap at
                        // the casting. James accepts local fitting overlap;
                        // a long exposed loop around the board looks worse.
                        var entry = socket - outward * .035f;
                        var lower = socket + outward * ((outerX - socket.x) / outward.x);
                        var upper = new Vector3(lower.x, HeaderY, lower.z);
                        var joint = new Vector3(x, HeaderY, lower.z - .30f);
                        Rounded(b, new[] { entry, lower, upper, joint }, r);
                        EndGap(b, entry, joint); b.Collar(socket + outward * .008f, outward, r); joints.Add(joint);
                    }
                }
                // Keep a horizontal header above every branch. A rear cylinder
                // must not pull the complete header down into the air reservoir.
                var front = new Vector3(x, HeaderY, joints.Max(p => p.z) + .20f);
                var rear = new Vector3(x, HeaderY, joints.Min(p => p.z) - .18f);
                Rounded(b, new[] { front, rear }, radius, true, true);
                EndGap(b, front, rear);
                b.Collar(front - Vector3.forward * .04f, Vector3.forward, radius);
                b.Collar(rear + Vector3.forward * .04f, -Vector3.forward, radius);
                h.Pipe.Upload();
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
            initialized = false; gameObject.SetActive(false);
            if (lod != null && originalLods != null) { lod.SetLODs(originalLods); lod.localReferencePoint = originalReference; lod.size = originalSize; }
            lod = null; originalLods = null;
            foreach (var mesh in owned) if (mesh != null) Object.Destroy(mesh);
            owned.Clear();
        }
        private void OnDestroy() => Release();
    }
}
#endif
