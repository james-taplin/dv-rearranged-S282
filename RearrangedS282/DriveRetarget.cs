#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dummiesman;
using DV.Util;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RearrangedS282
{
    // Changes the distance between an animated drivetrain's cylinder side and
    // its drivers without touching the animation. The native clips keep driving
    // every transform; each affected renderer's mesh is drawn by a proxy child:
    // - Shift: cylinder-side parts (crosshead, valve gear at the cylinder,
    //   Franklin cylinder block) are drawn Shift metres further along drive z.
    // - Stretch: rods spanning the gap (main rod, radius rod, Franklin middle
    //   shaft) keep their pivot end, have their far end moved by Shift (mesh
    //   edit when readable, else a length scale) and are re-aimed every frame at
    //   their shifted far pin.
    // Renderers are classified by their transform path relative to the drive.
    // LOD groups are pointed at the proxies; Unbind restores everything.
    internal sealed class DriveRetarget : MonoBehaviour
    {
        internal enum Role { Shift, Stretch }

        private sealed class Entry
        {
            internal Role Role;
            internal Transform Source;          // transform carrying the original renderer (animated)
            internal MeshFilter Filter;
            internal MeshRenderer Renderer;
            internal Mesh OriginalMesh, EditedMesh;
            internal Transform Proxy;
            internal MeshRenderer ProxyRenderer;
            internal float PinLength;           // mesh-local pivot-to-far-pin distance along +z (stretch)
            internal float MeshLength;          // far pin distance the proxy mesh is built for
            internal Vector3[] NativeVertices, WorkVertices;
            internal float[] NativePins, PresentedPins;
            internal bool PreserveBosses, HasUv;
        }

        private readonly List<Entry> entries = new List<Entry>();
        private readonly List<(LODGroup group, LOD[] lods, Vector3 reference, float size)> lodBackups
            = new List<(LODGroup, LOD[], Vector3, float)>();
        private Transform drive;
        private float shift;
        private bool precisionRods;
        private Func<Mesh, Mesh> readableSource;

        internal float Shift => shift;

        // shift: drive-local metres along +z applied to the cylinder side.
        internal static DriveRetarget Bind(Transform drive, float shift, Func<string, Role?> classify, Transform lodSearchRoot)
            => BindCore(drive, shift, classify, lodSearchRoot, false, null);

        // Verified native pin distances at both LODs. Existing arrangements keep
        // their original presentation path; only the new spacing trial opts in.
        internal static DriveRetarget BindNativePrecision(Transform drive, float shift, Transform lodSearchRoot,
            Func<Mesh, Mesh> readableSource = null)
            => BindCore(drive, shift, NativeWalschaertsRole, lodSearchRoot, true, readableSource);

        private static DriveRetarget BindCore(Transform drive, float shift, Func<string, Role?> classify,
            Transform lodSearchRoot, bool precisionRods, Func<Mesh, Mesh> readableSource)
        {
            Unbind(drive);
            if (Mathf.Abs(shift) < 1e-4f) return null;
            var retarget = drive.gameObject.AddComponent<DriveRetarget>();
            retarget.drive = drive; retarget.shift = shift;
            retarget.precisionRods = precisionRods; retarget.readableSource = readableSource;
            try
            {
                foreach (var renderer in drive.GetComponentsInChildren<MeshRenderer>(true))
                {
                    var filter = renderer.GetComponent<MeshFilter>();
                    if (filter == null || filter.sharedMesh == null || renderer.name.StartsWith("[Rearranged retarget]", StringComparison.Ordinal)) continue;
                    var role = classify(RelativePath(drive, renderer.transform));
                    if (role.HasValue) retarget.Add(renderer, filter, role.Value);
                }
                retarget.RepointLods(lodSearchRoot);
                retarget.LateUpdate();
                if (precisionRods) retarget.ExpandPrecisionLodBounds();
            }
            catch
            {
                retarget.Restore(); Object.Destroy(retarget);
                throw;
            }
            return retarget;
        }

        internal static void Unbind(Transform drive)
        {
            if (drive == null) return;
            foreach (var retarget in drive.GetComponents<DriveRetarget>())
            {
                retarget.Restore();
                Object.DestroyImmediate(retarget);
            }
        }

        // Path with LOD suffixes removed, so LOD1 renderers share their LOD0 role.
        private static string RelativePath(Transform root, Transform t)
        {
            var names = new List<string>();
            for (var x = t; x != null && x != root; x = x.parent) names.Add(x.name.Replace("_LOD1", ""));
            names.Reverse();
            // "rod/rod_LOD1" is the LOD1 copy of "rod": collapse to "rod".
            var collapsed = names.Where((n, i) => i == 0 || n != names[i - 1]);
            return string.Join("/", collapsed);
        }

        private void Add(MeshRenderer renderer, MeshFilter filter, Role role)
        {
            var entry = new Entry { Role = role, Source = renderer.transform, Filter = filter, Renderer = renderer, OriginalMesh = filter.sharedMesh };
            // Register ownership before import/proxy work so a failed bind also
            // releases a readable mesh created by the precision path.
            entries.Add(entry);
            var mesh = entry.OriginalMesh;
            if (role == Role.Stretch)
            {
                if (precisionRods)
                {
                    string path = RelativePath(drive, renderer.transform);
                    entry.PinLength = path == "s282_mech_wheels_connect/s282_mech_push_rod_to_connect" ? 3.15f
                        : path == "s282_mech_cutoff_boomerang_high/s282_mech_cutoff_rail_connection/s282_mech_cutoff_to_cylinder_rod" ? 1.56f
                        : throw new InvalidOperationException("No measured native rod pin datum for " + path);
                    // Main LOD0 and radius LOD0 are not CPU-readable in B99.
                    // Reuse the existing exporter if a local fallback is absent;
                    // precision mode must never silently scale the whole rod.
                    var readable = ReadableCopy(mesh) ?? readableSource?.Invoke(mesh);
                    entry.EditedMesh = readable;
                    if (readable == null || !readable.isReadable || readable.vertexCount == 0)
                        throw new InvalidOperationException("Precision retarget needs a readable native rod: " + mesh.name);
                    entry.NativeVertices = readable.vertices;
                    entry.WorkVertices = new Vector3[entry.NativeVertices.Length];
                    entry.NativePins = new[] { 0f, entry.PinLength };
                    entry.PresentedPins = new[] { 0f, entry.PinLength };
                    entry.PreserveBosses = true; entry.HasUv = readable.uv.Length == readable.vertexCount;
                    readable.name = mesh.name + " (precision retargeted)";
                }
                else
                {
                    var b = mesh.bounds;
                    // Preserve the existing presentation on older arrangements.
                    entry.PinLength = b.max.z + Mathf.Min(0f, b.min.z);
                    if (!(entry.PinLength > .05f)) throw new InvalidOperationException("Rod " + renderer.name + " has no measurable length.");
                    entry.MeshLength = entry.PinLength;
                    var readable = ReadableCopy(mesh);
                    if (readable != null)
                    {
                        var vertices = readable.vertices;
                        float half = entry.PinLength * .5f;
                        for (int i = 0; i < vertices.Length; i++) if (vertices[i].z > half) vertices[i].z += shift;
                        readable.vertices = vertices; readable.RecalculateBounds();
                        readable.name = mesh.name + " (retargeted)";
                        entry.EditedMesh = readable; entry.MeshLength = entry.PinLength + shift;
                        if (!(entry.MeshLength > .05f)) throw new InvalidOperationException("Rod " + renderer.name + " would be too short.");
                    }
                }
            }
            var proxy = new GameObject("[Rearranged retarget] " + renderer.name).transform;
            proxy.SetParent(renderer.transform, false);
            proxy.gameObject.layer = renderer.gameObject.layer;
            proxy.gameObject.AddComponent<MeshFilter>().sharedMesh = entry.EditedMesh != null ? entry.EditedMesh : mesh;
            var proxyRenderer = proxy.gameObject.AddComponent<MeshRenderer>();
            proxyRenderer.sharedMaterials = renderer.sharedMaterials;
            proxyRenderer.shadowCastingMode = renderer.shadowCastingMode;
            proxyRenderer.receiveShadows = renderer.receiveShadows;
            proxyRenderer.lightProbeUsage = renderer.lightProbeUsage;
            proxyRenderer.reflectionProbeUsage = renderer.reflectionProbeUsage;
            entry.Proxy = proxy; entry.ProxyRenderer = proxyRenderer;
            filter.sharedMesh = null;   // the original draws nothing, whatever enables it
        }

        // Owned readable copy, or an earlier local AssetStudio export. Null if
        // neither is available; precision mode then requests the native exporter.
        private static Mesh ReadableCopy(Mesh mesh)
        {
            try
            {
                if (mesh.isReadable) return Object.Instantiate(mesh);
                string name = mesh.name.Replace(" Instance", "").Replace("(Clone)", "").Trim();
                string path = Path.Combine(Path.Combine(Main.ModPath, "assets"), name + ".obj");
                if (!File.Exists(path)) return null;
                var loaded = new OBJLoader().Load(path);
                if (loaded != null && loaded.isReadable && loaded.vertexCount > 0) return loaded;
                if (loaded != null) Object.Destroy(loaded);
                return null;
            }
            catch (Exception ex)
            {
                Main.Logger.Warning($"Retarget: readable mesh unavailable for {mesh.name} ({ex.Message}).");
                return null;
            }
        }

        private void RepointLods(Transform root)
        {
            if (root == null) return;
            var map = entries.ToDictionary(e => (Renderer)e.Renderer, e => (Renderer)e.ProxyRenderer);
            foreach (var group in root.GetComponentsInChildren<LODGroup>(true))
            {
                var lods = group.GetLODs();
                bool changed = false;
                var copy = lods.Select(l => new LOD(l.screenRelativeTransitionHeight, l.renderers.Select(r =>
                {
                    if (r != null && map.TryGetValue(r, out var proxy)) { changed = true; return proxy; }
                    return r;
                }).ToArray()) { fadeTransitionWidth = l.fadeTransitionWidth }).ToArray();
                if (!changed) continue;
                lodBackups.Add((group, lods, group.localReferencePoint, group.size));
                group.SetLODs(copy);
            }
        }

        private void ExpandPrecisionLodBounds()
        {
            var worldShift = drive.TransformVector(0f, 0f, shift);
            foreach (var state in lodBackups)
            {
                var group = state.group;
                var localShift = group.transform.InverseTransformVector(worldShift);
                // Keep the original native animation envelope AND its shifted
                // cylinder-side envelope, not only the bind frame's pose.
                var half = Vector3.one * (state.size * .5f);
                var min = state.reference - half + new Vector3(Mathf.Min(0f, localShift.x), Mathf.Min(0f, localShift.y), Mathf.Min(0f, localShift.z));
                var max = state.reference + half + new Vector3(Mathf.Max(0f, localShift.x), Mathf.Max(0f, localShift.y), Mathf.Max(0f, localShift.z));
                group.RecalculateBounds();
                var currentHalf = Vector3.one * (group.size * .5f);
                var currentMin = group.localReferencePoint - currentHalf;
                var currentMax = group.localReferencePoint + currentHalf;
                min = new Vector3(Mathf.Min(min.x, currentMin.x), Mathf.Min(min.y, currentMin.y), Mathf.Min(min.z, currentMin.z));
                max = new Vector3(Mathf.Max(max.x, currentMax.x), Mathf.Max(max.y, currentMax.y), Mathf.Max(max.z, currentMax.z));
                var size = max - min;
                group.localReferencePoint = (min + max) * .5f;
                group.size = Mathf.Max(size.x, Mathf.Max(size.y, size.z)) + .02f;
            }
        }

        private void LateUpdate()
        {
            if (drive == null) return;
            var worldShift = drive.TransformVector(0f, 0f, shift);
            foreach (var e in entries)
            {
                if (e.Source == null || e.Proxy == null) continue;
                e.ProxyRenderer.enabled = e.Renderer.enabled;
                var local = e.Source.InverseTransformVector(worldShift);
                if (e.Role == Role.Shift)
                {
                    e.Proxy.localPosition = local; e.Proxy.localRotation = Quaternion.identity; e.Proxy.localScale = Vector3.one;
                    continue;
                }
                // Far pin where the native animation puts it, plus the shift.
                var target = new Vector3(0f, 0f, e.PinLength) + local;
                float length = target.magnitude;
                if (!(length > 1e-3f)) continue;
                e.Proxy.localPosition = Vector3.zero;
                e.Proxy.localRotation = Quaternion.FromToRotation(Vector3.forward, target / length);
                if (e.PreserveBosses)
                {
                    // Retain the native crosshead trajectory. Only the straight
                    // shaft absorbs its small phase-dependent length difference;
                    // both bearing ends stay rigid instead of breathing in Z.
                    e.PresentedPins[1] = length;
                    RodRemesher.RemeshInto(e.NativeVertices, e.NativePins, e.PresentedPins, e.WorkVertices);
                    e.EditedMesh.vertices = e.WorkVertices;
                    e.EditedMesh.RecalculateBounds(); e.EditedMesh.RecalculateNormals();
                    if (e.HasUv) e.EditedMesh.RecalculateTangents();
                    e.Proxy.localScale = Vector3.one;
                }
                else e.Proxy.localScale = new Vector3(1f, 1f, length / e.MeshLength);
            }
            foreach (var point in oilPoints)
            {
                if (point.Provider == null || point.Proxy == null) continue;
                point.Provider.position = point.Proxy.TransformPoint(point.NativeLocal);
                // Native consumer LateUpdate can precede this one. Repeating
                // its idempotent position sync here prevents a rendered-frame lag.
                if (point.Consumer != null) point.Consumer.Sync();
            }
        }

        private void Restore()
        {
            foreach (var point in oilPoints)
                if (point.Provider != null) point.Provider.localPosition = point.NativeLocal;
            oilPoints.Clear();
            foreach (var state in lodBackups)
                if (state.group != null)
                {
                    state.group.SetLODs(state.lods);
                    if (precisionRods) { state.group.localReferencePoint = state.reference; state.group.size = state.size; }
                }
            lodBackups.Clear();
            foreach (var e in entries)
            {
                if (e.Filter != null && e.Filter.sharedMesh == null) e.Filter.sharedMesh = e.OriginalMesh;
                if (e.Proxy != null) { e.Proxy.gameObject.SetActive(false); Object.Destroy(e.Proxy.gameObject); }
                if (e.EditedMesh != null) Object.Destroy(e.EditedMesh);
            }
            entries.Clear();
        }

        private void OnDestroy() => Restore();

        // Native S282 Walschaerts drive: crosshead, union link, combination lever
        // and valve spindle sit with the cylinder; main and radius rods span the
        // gap; the expansion link, eccentric rod and coupling rods stay with the drivers.
        internal static Role? NativeWalschaertsRole(string path)
        {
            switch (path)
            {
                case "s282_mech_push_rod":
                case "s282_mech_push_rod/s282_mech_push_rod_to_cutoff_base":
                case "s282_mech_cutoff_rod_hinge":
                case "s282_mech_cutoff_rod_hinge/s282_mech_push_rod_to_cutoff_rod":
                case "s282_mech_cylinder_rod":
                    return Role.Shift;
                case "s282_mech_wheels_connect/s282_mech_push_rod_to_connect":
                case "s282_mech_cutoff_boomerang_high/s282_mech_cutoff_rail_connection/s282_mech_cutoff_to_cylinder_rod":
                    return Role.Stretch;
                default:
                    return null;
            }
        }

        // Franklin single-driver engine (triplex): the cylinder block with its
        // front shaft and joints, the front shaft mount and the crosshead sit with
        // the cylinder; the main rod and the middle cardan shaft span the gap.
        internal static Role? FranklinRole(string path)
        {
            if (path == "s282_mech_push_rod" || path.StartsWith("s282_mech_push_rod/", StringComparison.Ordinal)) return Role.Shift;
            if (path == "s282_mech_wheels_connect/s282_mech_push_rod_to_connect") return Role.Stretch;
            const string franklin = "franklin_type_b(Clone)/";
            if (!path.StartsWith(franklin, StringComparison.Ordinal)) return null;
            string part = path.Substring(franklin.Length);
            if (part.StartsWith("cylinder/", StringComparison.Ordinal) || part.StartsWith("driveshaft_m_mount/", StringComparison.Ordinal)
                || part.StartsWith("driveshaft_m/u_joint_f/", StringComparison.Ordinal)) return Role.Shift;
            if (part == "driveshaft_m/default") return Role.Stretch;
            return null;
        }
        // Keep native identities/ports while following the rendered gear.
        private sealed class OilPoint
        {
            internal Transform Provider, Proxy;
            internal Vector3 NativeLocal;
            internal PositionSyncConsumer Consumer;
        }
        private readonly List<OilPoint> oilPoints = new List<OilPoint>();

        internal void BindOilCup(Transform provider, Vector3 nativeLocal, PositionSyncConsumer consumer)
        {
            Entry entry = null;
            foreach (var candidate in entries)
                if (candidate.Source == provider.parent) { entry = candidate; break; }
            if (entry == null) return;
            for (int i = oilPoints.Count - 1; i >= 0; i--)
                if (oilPoints[i].Provider == provider) oilPoints.RemoveAt(i);
            oilPoints.Add(new OilPoint { Provider = provider, Proxy = entry.Proxy, NativeLocal = nativeLocal, Consumer = consumer });
            provider.position = entry.Proxy.TransformPoint(nativeLocal);
            consumer.Sync();
        }
    }
}
#endif
