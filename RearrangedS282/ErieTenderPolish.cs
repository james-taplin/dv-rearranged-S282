#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RearrangedS282
{
    // Presentation only: native bogies, wheel contacts and the steam graph stay intact.
    internal sealed class ErieTenderPolish : MonoBehaviour
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private readonly List<Mesh> meshes = new List<Mesh>();
        private TrainCar host, tender;
        private Transform bridge, hostAnchor, tenderAnchor;
        private LODGroup bridgeLod;
        private LODGroup supportLod;
        private Renderer[] supportRenderers = Array.Empty<Renderer>();
        private readonly Transform[] spans = new Transform[2], joints = new Transform[3];
        private readonly Vector3[] bridgePoints = new Vector3[3];
        private SteamDuctMesh.Template template;
        private Material[] materials;
        private int supportCount, portCount;
        internal bool Ready { get; private set; }

        internal void Initialize(TrainCar locomotive, TrainCar car, Material[] pipeMaterials)
        {
            host = locomotive; tender = car; materials = pipeMaterials;
            var asset = SteamPipePresentation.TransferAsset;
            template = new SteamDuctMesh.Template(asset.vertices, asset.triangles, asset.uv);
            BuildTrailingSupport();
            BuildFeed();
            Ready = supportCount == 2 && portCount == 2 && SupportRegistered;
            LateUpdate();
        }

        private Transform Root(string name, Transform parent)
        {
            var go = new GameObject("[Rearranged tender polish] " + name);
            objects.Add(go); go.layer = tender.gameObject.layer;
            go.transform.SetParent(parent, false); return go.transform;
        }

        private Mesh Mesh(SteamDuctMesh.Builder builder, string name)
        {
            var mesh = new Mesh { name = name }; meshes.Add(mesh);
            mesh.SetVertices(builder.Vertices); mesh.SetNormals(builder.Normals);
            mesh.SetUVs(0, builder.UV); mesh.SetTriangles(builder.Triangles, 0);
            mesh.RecalculateBounds(); mesh.RecalculateTangents(); return mesh;
        }

        private MeshRenderer Render(Transform root, Mesh mesh, Material[] mats)
        {
            root.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = root.gameObject.AddComponent<MeshRenderer>(); renderer.sharedMaterials = mats;
            return renderer;
        }

        private static void Cull(Transform root, params Renderer[] renderers)
        {
            var lod = root.gameObject.AddComponent<LODGroup>();
            lod.SetLODs(new[] { new LOD(.015f, renderers) }); lod.RecalculateBounds();
        }

        private void BuildTrailingSupport()
        {
            var rear = tender.Bogies.SelectMany(b => b.Axles.Select(a => a.transform))
                .OrderBy(a => tender.transform.InverseTransformPoint(a.position).z).First();
            var bogie = tender.Bogies.First(b => b.Axles.Any(a => a.transform == rear));
            var frame = bogie.transform.Find("bogie_car");
            var other = bogie.Axles.Select(a => a.transform).First(a => a != rear);
            var root = Root("trailing axle cradle", frame);
            var renderers = new List<Renderer>();
            foreach (string name in new[] { "bogie2_LOD0", "bogie2_LOD1" })
            {
                var source = frame.GetComponentsInChildren<MeshFilter>(true)
                    .First(f => f.sharedMesh != null && f.sharedMesh.name == name);
                var rearLocal = source.transform.InverseTransformPoint(rear.position);
                var otherLocal = source.transform.InverseTransformPoint(other.position);
                var readable = MeshFinder.Instance.ReadableRetargetMesh(source.sharedMesh);
                Mesh clipped;
                try { clipped = ArticulatedFrameClipper.Clip(readable, (rearLocal + otherLocal) * .5f,
                    (rearLocal - otherLocal).normalized, true); }
                finally { Object.Destroy(readable); }
                meshes.Add(clipped);
                if (clipped.vertexCount == 0) throw new InvalidOperationException("Empty trailing axle cradle");
                var piece = Root(name, source.transform);
                piece.SetParent(root, true);
                renderers.Add(Render(piece, clipped, source.GetComponent<MeshRenderer>().sharedMaterials));
                supportCount++;
            }
            // Native car visibility and bogie-smoothing integrations already
            // track this LOD owner. Register the new cradle explicitly, as the
            // tender exhaust does, instead of creating another bogie LOD group
            // after those native visual owners have been initialized.
            supportLod = tender.transform.Find("LocoS282B_Body")?.GetComponent<LODGroup>();
            if (supportLod == null)
                supportLod = tender.GetComponentsInChildren<LODGroup>(true).FirstOrDefault(g =>
                    g.GetLODs().Any(l => l.renderers.Any(r => r != null && r.name.StartsWith("s282_tender"))));
            BindSupportLods(supportLod, renderers.ToArray());
        }

        private void BindSupportLods(LODGroup lod, Renderer[] renderers)
        {
            if (lod == null || renderers == null || renderers.Length != 2)
                throw new InvalidOperationException("Native tender LOD owner or trailing cradle detail/coarse mesh is missing");
            supportLod = lod; supportRenderers = renderers;
            supportLod.SetLODs(supportLod.GetLODs().Select((level, index) =>
                new LOD(level.screenRelativeTransitionHeight, level.renderers.Length == 0 ? level.renderers :
                    level.renderers.Concat(new[] { supportRenderers[index == 0 ? 0 : 1] }).Distinct().ToArray())
                { fadeTransitionWidth = level.fadeTransitionWidth }).ToArray());
            supportLod.RecalculateBounds();
            if (!SupportRegistered) throw new InvalidOperationException("Trailing cradle renderers were not registered in native tender LODs");
        }

        private bool SupportRegistered => supportLod != null && supportRenderers.Length == 2 &&
            supportRenderers.All(r => r != null && r.gameObject.activeInHierarchy) &&
            supportLod.GetLODs().Select((l, i) => (level: l, index: i)).Where(p => p.level.renderers.Length != 0)
                .All(p => p.level.renderers.Contains(supportRenderers[p.index == 0 ? 0 : 1])) &&
            supportLod.GetLODs().Any(l => l.renderers.Contains(supportRenderers[0]));

        private void UnbindSupportLods()
        {
            if (supportLod != null)
            {
                supportLod.SetLODs(supportLod.GetLODs().Select(level =>
                    new LOD(level.screenRelativeTransitionHeight, level.renderers.Where(r => !supportRenderers.Contains(r)).ToArray())
                    { fadeTransitionWidth = level.fadeTransitionWidth }).ToArray());
                supportLod.RecalculateBounds();
            }
            supportLod = null; supportRenderers = Array.Empty<Renderer>();
        }

        private void BuildFeed()
        {
            // The inlet disappears into the firebox underframe. It does not run
            // through the cab or pretend to connect to the tender water tank.
            var localCoupler = host.transform.InverseTransformPoint(host.rearCoupler.transform.position);
            var inlet = new Vector3(.50f, 1.15f, localCoupler.z + 1.7f);
            var outlet = new Vector3(.50f, 1.15f, localCoupler.z + .12f);
            hostAnchor = Root("locomotive steam feed", host.transform);
            var feed = new SteamDuctMesh.Builder(template);
            feed.Tube(new[] { inlet, outlet }, .08f, .18f);
            feed.Collar(outlet + Vector3.forward * .08f, Vector3.forward, .08f);
            Cull(hostAnchor, Render(hostAnchor, Mesh(feed, "Underframe steam inlet"), materials));
            hostAnchor.localPosition = Vector3.zero;
            // Separate anchor: fixed pipe vertices remain in vehicle coordinates.
            var anchor = Root("locomotive swivel", host.transform); anchor.localPosition = outlet;
            hostAnchor = anchor;

            tenderAnchor = Root("tender swivel", tender.transform);
            tenderAnchor.localPosition = new Vector3(.50f, 1.15f,
                tender.transform.InverseTransformPoint(tender.frontCoupler.transform.position).z - .12f);
            var distribution = new SteamDuctMesh.Builder(template);
            var ports = new List<Vector3>();
            foreach (string side in new[] { "_l", "_r" })
            {
                var cylinder = GetComponentsInChildren<MeshFilter>(true).First(f =>
                    f.name.Contains("s282a_cylinder" + side) && f.sharedMesh != null);
                // Ray against the actual upper chest triangles, not its bounding box.
                var vertices = cylinder.sharedMesh.vertices.Select(v => tender.transform.InverseTransformPoint(cylinder.transform.TransformPoint(v))).ToArray();
                var bounds = new Bounds(vertices[0], Vector3.zero);
                foreach (var v in vertices) bounds.Encapsulate(v);
                float y = Mathf.Lerp(bounds.min.y, bounds.max.y, .78f);
                float z = Mathf.Lerp(bounds.min.z, bounds.max.z, .52f);
                var port = InboardPort(vertices, cylinder.sharedMesh.triangles, y, z);
                // Small socket penetration closes the joint without a long clipped run.
                port.x += Mathf.Sign(port.x) * .025f; ports.Add(port); portCount++;
            }
            var junction = new Vector3(.50f, ports.Average(p => p.y), ports.Average(p => p.z));
            var start = tenderAnchor.localPosition;
            // A short inclined inlet avoids forcing an elbow into the limited
            // height between the scaled steam chests and the tender front beam.
            distribution.Tube(new[] { start, junction }, .08f, .16f);
            foreach (var port in ports)
            {
                distribution.Tube(new[] { junction, port }, .08f, .16f);
                var axis = (port - junction).normalized;
                distribution.Collar(port - axis * .06f, axis, .08f);
            }
            var fixedRoot = Root("tender distribution", transform);
            Cull(fixedRoot, Render(fixedRoot, Mesh(distribution, "Tender steam distribution"), materials));

            bridge = Root("articulating steam connection", transform);
            var shaft = new SteamDuctMesh.Builder(template);
            shaft.Tube(new[] { Vector3.zero, Vector3.forward }, .08f, .16f, true, true);
            var shaftMesh = Mesh(shaft, "Swivelling steam pipe");
            var collar = new SteamDuctMesh.Builder(template); collar.Collar(Vector3.zero, Vector3.forward, .08f);
            var collarMesh = Mesh(collar, "Steam swivel collar");
            var bridgeRenderers = new List<Renderer>();
            for (int i = 0; i < spans.Length; i++) { spans[i] = Root("span " + i, bridge); bridgeRenderers.Add(Render(spans[i], shaftMesh, materials)); }
            for (int i = 0; i < joints.Length; i++) { joints[i] = Root("swivel " + i, bridge); bridgeRenderers.Add(Render(joints[i], collarMesh, materials)); }
            Cull(bridge, bridgeRenderers.ToArray());
            bridgeLod = bridge.GetComponent<LODGroup>();
        }

        internal static Vector3 InboardPort(Vector3[] v, int[] triangles, float y, float z)
        {
            float best = float.PositiveInfinity, x = 0;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                var a = v[triangles[i]]; var b = v[triangles[i + 1]]; var c = v[triangles[i + 2]];
                float d = (b.y - a.y) * (c.z - a.z) - (c.y - a.y) * (b.z - a.z);
                if (Mathf.Abs(d) < 1e-8f) continue;
                float u = ((y - a.y) * (c.z - a.z) - (c.y - a.y) * (z - a.z)) / d;
                float w = ((b.y - a.y) * (z - a.z) - (y - a.y) * (b.z - a.z)) / d;
                if (u < 0 || w < 0 || u + w > 1) continue;
                float candidate = a.x + u * (b.x - a.x) + w * (c.x - a.x);
                if (Mathf.Abs(candidate) < best) { x = candidate; best = Mathf.Abs(candidate); }
            }
            if (float.IsInfinity(best)) throw new InvalidOperationException("No tender steam chest surface at requested port");
            return new Vector3(x, y, z);
        }

        private void LateUpdate()
        {
            if (bridge == null) return;
            bool connected = host != null && tender != null && host.rearCoupler.coupledTo != null &&
                host.rearCoupler.coupledTo.train == tender && hostAnchor != null && tenderAnchor != null;
            if (!connected) { bridge.gameObject.SetActive(false); return; }
            var a = hostAnchor.position; var b = tenderAnchor.position;
            float gap = Vector3.Distance(a, b);
            bridge.gameObject.SetActive(gap > .05f && gap < 3f);
            if (!bridge.gameObject.activeSelf) return;
            var mid = (a + b) * .5f - tender.transform.up * .18f;
            bridgeLod.localReferencePoint = bridge.InverseTransformPoint(mid);
            bridgeLod.size = 3f;
            var points = bridgePoints; points[0] = a; points[1] = mid; points[2] = b;
            for (int i = 0; i < 2; i++)
            {
                var delta = points[i + 1] - points[i];
                spans[i].position = points[i]; spans[i].rotation = Quaternion.LookRotation(delta, tender.transform.up);
                spans[i].localScale = new Vector3(1, 1, delta.magnitude);
            }
            for (int i = 0; i < 3; i++)
            { joints[i].position = points[i]; joints[i].rotation = spans[Math.Min(i, 1)].rotation; }
        }

        internal JObject Report() => new JObject { ["ready"] = Ready, ["trailingSupportLODs"] = supportCount,
            ["trailingSupportRegistered"] = SupportRegistered, ["trailingSupportLodOwner"] = supportLod != null ? supportLod.name : null,
            ["cylinderPorts"] = portCount, ["bridgeVisible"] = bridge != null && bridge.gameObject.activeInHierarchy };

        internal void Release()
        {
            Ready = false;
            UnbindSupportLods();
            foreach (var go in objects) if (go != null) { go.SetActive(false); Object.Destroy(go); }
            objects.Clear();
            foreach (var mesh in meshes) if (mesh != null) Object.Destroy(mesh);
            meshes.Clear(); bridge = hostAnchor = tenderAnchor = null;
        }
        private void OnDisable() { Release(); }
        private void OnDestroy() { Release(); }
    }
}
#endif
