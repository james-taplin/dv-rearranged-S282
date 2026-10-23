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
    internal partial class WheelRearranger
    {
        private sealed class CasingTemplate
        {
            internal Mesh Mesh, Readable;
            internal Matrix4x4 NativeFromMesh, MeshFromNative;
            internal Vector3[] Vertices;
            internal float MinY, RearZ, FullZ, PistonY;
            internal bool Franklin;
        }
        private sealed class CasingMesh
        {
            internal MeshFilter Filter;
            internal Mesh Original, Corrected;
        }
        private readonly Dictionary<Mesh, CasingTemplate> casingTemplates = new Dictionary<Mesh, CasingTemplate>();
        private readonly List<CasingMesh> casingMeshes = new List<CasingMesh>();
        private readonly List<Mesh> readableCasingSources = new List<Mesh>();
        private sealed class PrairieFitting
        {
            internal MeshFilter Filter;
            internal Mesh Source;
            internal Vector3[] Vertices;
            internal bool Pipe;
            internal float LowY, HighY;
        }
        private readonly List<PrairieFitting> prairieFittings = new List<PrairieFitting>();
        private readonly List<CasingMesh> prairieFittingMeshes = new List<CasingMesh>();
        private readonly List<(Transform part, Vector3 native)> prairieEmitters = new List<(Transform, Vector3)>();
        private readonly List<(Transform part, Vector3 local)> prairieEmitterBackups = new List<(Transform, Vector3)>();
        private bool franklinCasingsCached;
        internal int CylinderCasingMeshCount => casingMeshes.Count;

        // Capture the prepared native frame before any arrangement transforms.
        // Later rear/tender clones retain the same shared source mesh identity.
        private void CacheCylinderCasings()
        {
            var body = transform.Find("LocoS282A_Body/MovingParts_LOD0");
            if (body == null) return;
            foreach (string lod in new[] { "Static_LOD0", "Static_LOD1" })
            {
                var container = transform.Find("LocoS282A_Body/" + lod);
                Transform split = null;
                if (container != null)
                    foreach (Transform child in container)
                        if (child.name.StartsWith("s282a_split_body", StringComparison.Ordinal)) { split = child; break; }
                if (split == null) continue;
                foreach (string side in new[] { "l", "r" })
                {
                    var part = split.Find("s282a_cylinder_" + side);
                    var filter = part != null ? part.GetComponent<MeshFilter>() : null;
                    if (filter != null) CacheCasing(filter, body.worldToLocalMatrix * filter.transform.localToWorldMatrix, false);
                }
                foreach (string name in new[] { "s282a_cylinder_cocks", "s282a_dry_pipe_l", "s282a_dry_pipe_r" })
                {
                    var part = split.Find(name);
                    var filter = part != null ? part.GetComponent<MeshFilter>() : null;
                    if (filter == null || filter.sharedMesh == null || !filter.sharedMesh.isReadable) continue;
                    var frame = body.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                    var vertices = filter.sharedMesh.vertices.Select(frame.MultiplyPoint3x4).ToArray();
                    if (vertices.Length == 0) continue;
                    prairieFittings.Add(new PrairieFitting { Filter = filter, Source = filter.sharedMesh,
                        Vertices = vertices, Pipe = name.Contains("dry_pipe"),
                        LowY = vertices.Min(v => v.y), HighY = vertices.Max(v => v.y) });
                }
            }
            foreach (string path in new[] { "LocoS282A_Particles/CylCockSteam", "LocoS282A_Particles/CylCockWaterDrip",
                "LocoS282A_Particles/CylinderCrack" })
            {
                var holder = transform.Find(path);
                if (holder != null)
                    foreach (var system in holder.GetComponentsInChildren<ParticleSystem>(true))
                        prairieEmitters.Add((system.transform, body.InverseTransformPoint(system.transform.position)));
            }
        }

        private void CacheFranklinCasings()
        {
            if (franklinCasingsCached) return;
            foreach (var prefab in new[] { AssetBundleLoader.FranklinBValveGear, AssetBundleLoader.FranklinBValveGear4444 })
            {
                var part = prefab.transform.Find("cylinder/default");
                var filter = part != null ? part.GetComponent<MeshFilter>() : null;
                if (filter != null) CacheCasing(filter, prefab.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix, true);
            }
            franklinCasingsCached = true;
        }

        private void CacheCasing(MeshFilter filter, Matrix4x4 nativeFromMesh, bool franklin)
        {
            var mesh = filter.sharedMesh;
            if (mesh == null || casingTemplates.ContainsKey(mesh)) return;
            var readable = mesh;
            if (!mesh.isReadable && franklin)
            {
                // Original MIT mod asset, exported without changing its geometry.
                // Unity's bundled copy has CPU reads disabled. Use the existing
                // OBJ reader rather than replacing or rebuilding the asset bundle.
                string path = Path.Combine(Main.ModPath, "assets", "franklin_casing.obj");
                if (File.Exists(path))
                {
                    readable = new OBJLoader().Load(path);
                    if (readable != null) readableCasingSources.Add(readable);
                }
            }
            if (readable == null || !readable.isReadable)
            {
                Main.Logger.Warning("Cylinder casing mesh is not readable; retaining its original presentation: " + filter.name);
                return;
            }
            var vertices = readable.vertices;
            if (vertices.Length == 0) return;
            var native = vertices.Select(v => nativeFromMesh.MultiplyPoint3x4(v)).ToArray();
            float rear = native.Min(v => v.z) + .16f;
            casingTemplates.Add(mesh, new CasingTemplate { Mesh = mesh, Readable = readable, Vertices = vertices,
                NativeFromMesh = nativeFromMesh, MeshFromNative = nativeFromMesh.inverse,
                MinY = franklin ? native.Min(v => v.y) : Math.Min(.2739646f, native.Min(v => v.y)),
                RearZ = rear, FullZ = rear + .20f,
                PistonY = franklin ? .71f : CylinderCasingShape.NativePistonY, Franklin = franklin });
        }

        private static bool UsesFullCylinderCasings(int id) => id == (int)WheelArrangementType.s262 ||
            id == (int)WheelArrangementType.s280Big || id == (int)WheelArrangementType.s282Big || id == (int)WheelArrangementType.s284Big ||
            id == (int)WheelArrangementType.s462 || id == (int)WheelArrangementType.s464 ||
            id == (int)WheelArrangementType.s440 || id == (int)WheelArrangementType.s442 ||
            id == (int)WheelArrangementType.s444 || id == (int)WheelArrangementType.s240 ||
            id == (int)WheelArrangementType.s460 || id == (int)WheelArrangementType.s260 ||
            (id >= (int)WheelArrangementType.s0120 && id <= (int)WheelArrangementType.s4122);

        private void ApplyCylinderCasings(int id)
        {
            if (id == (int)WheelArrangementType.s282) return;
            if (WheelArrangement.IsArticulatedId(id)) CacheFranklinCasings();
            bool fullArrangement = UsesFullCylinderCasings(id);
            var body = transform.Find("LocoS282A_Body/MovingParts_LOD0");
            Matrix4x4 prairieMap = Matrix4x4.identity;
            float prairieY = 1f, prairieZ = 1f;
            bool normalizedCasing = false;
            foreach (var filter in GetComponentsInChildren<MeshFilter>(true))
            {
                var original = filter.sharedMesh;
                if (!filter.gameObject.activeInHierarchy || original == null || !casingTemplates.TryGetValue(original, out var template)) continue;
                // The actual filter can be a retarget proxy. Read its final frame,
                // without moving the original animation transforms or LOD renderers.
                // The old four-coupled path only translates the static casting;
                // its enlarged valve gear uses a different bore separation.
                // Use the current native drive frame for full-casing layouts,
                // including when optional valve gear is switched off. Keeping
                // the old cylinder-only Y offset leaves the valve bore on a
                // different axis from its spindle (especially on twelve drivers).
                // Move only mesh vertices into that frame, never animated joints.
                bool fullCasing = fullArrangement && !template.Franklin;
                var nativeToWorld = fullCasing ? CylinderDriveFrame(driveR)
                    : filter.transform.localToWorldMatrix * template.MeshFromNative;
                var sizeFrame = fullCasing ? body.worldToLocalMatrix * nativeToWorld : nativeToWorld;
                float sy = sizeFrame.MultiplyVector(Vector3.up).magnitude;
                float sz = sizeFrame.MultiplyVector(Vector3.forward).magnitude;
                float originY = transform.InverseTransformPoint(nativeToWorld.MultiplyPoint3x4(Vector3.zero)).y;
                float targetY = CylinderCasingShape.ComputeTargetY(sy, template.MinY, template.PistonY, originY);
                if (Mathf.Abs(targetY - sy) < .001f) continue;
                var meshFromMappedNative = filter.transform.worldToLocalMatrix * nativeToWorld;
                var vertices = new Vector3[template.Vertices.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    var point = template.NativeFromMesh.MultiplyPoint3x4(template.Vertices[i]);
                    point = fullCasing ? CylinderCasingShape.NormalizePrairie(point, sy, sz)
                        : CylinderCasingShape.TransformVertex(point, sy, sz, targetY,
                            template.RearZ, template.FullZ, template.PistonY, template.Franklin);
                    vertices[i] = fullCasing ? meshFromMappedNative.MultiplyPoint3x4(point)
                        : template.MeshFromNative.MultiplyPoint3x4(point);
                }
                var copy = Object.Instantiate(template.Readable);
                try
                {
                    copy.name = "[Rearranged cylinder casing] " + original.name;
                    copy.vertices = vertices; copy.RecalculateBounds(); copy.RecalculateNormals();
                    if (copy.uv.Length == copy.vertexCount) copy.RecalculateTangents();
                    casingMeshes.Add(new CasingMesh { Filter = filter, Original = original, Corrected = copy });
                    filter.sharedMesh = copy;
                    if (fullCasing)
                    {
                        prairieMap = body.worldToLocalMatrix * nativeToWorld;
                        prairieY = sy; prairieZ = sz; normalizedCasing = true;
                    }
                }
                catch { Object.Destroy(copy); throw; }
            }
            if (normalizedCasing)
            {
                ApplyPrairieFittings(body, prairieMap, prairieY, prairieZ);
                Main.Logger.Log($"{WheelArrangement.WheelArrangementNames[id]} cylinders: full casing length, both barrels and caps normalized; {casingMeshes.Count} casing meshes, " +
                    $"{prairieFittingMeshes.Count} cock/pipe meshes and {prairieEmitterBackups.Count} particle outlets; " +
                    "native piston/valve axes, animation and physics retained; inter-bore web follows existing axis separation.");
            }
            else if (casingMeshes.Count > 0)
                Main.Logger.Log($"Cylinder casings: {casingMeshes.Count} mesh(es) reshaped for {WheelArrangement.WheelArrangementNames[id]}; " +
                    "socket/drain/pipe mounts retained; compact engine lengths retained; motion and physics unchanged.");
        }

        private void ApplyPrairieFittings(Transform body, Matrix4x4 nativeToCurrent, float sy, float sz)
        {
            if (prairieFittings.Count != 6)
                throw new InvalidOperationException("Full cylinder normalization needs the native cocks and both dry pipes at both LODs.");
            // Use original native vertices, not the already bent Pacific pipe.
            // The boiler end stays fixed; only the cylinder end follows the
            // resized chest. Cocks and all their shared plumbing follow together.
            foreach (var fitting in prairieFittings)
            {
                if (fitting.Filter == null) continue;
                var bodyToMesh = fitting.Filter.transform.worldToLocalMatrix * body.localToWorldMatrix;
                var vertices = new Vector3[fitting.Vertices.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    var native = fitting.Vertices[i];
                    var target = nativeToCurrent.MultiplyPoint3x4(CylinderCasingShape.NormalizePrairie(native, sy, sz));
                    if (fitting.Pipe)
                    {
                        float weight = Mathf.Clamp01((fitting.HighY - native.y) / (fitting.HighY - fitting.LowY));
                        weight = weight * weight * (3f - 2f * weight);
                        target = native + (target - native) * weight;
                    }
                    vertices[i] = bodyToMesh.MultiplyPoint3x4(target);
                }
                var copy = Object.Instantiate(fitting.Source);
                try
                {
                    copy.name = "[Rearranged normalized casing fitting] " + fitting.Source.name;
                    copy.vertices = vertices; copy.RecalculateBounds(); copy.RecalculateNormals();
                    if (copy.uv.Length == copy.vertexCount) copy.RecalculateTangents();
                    prairieFittingMeshes.Add(new CasingMesh { Filter = fitting.Filter, Original = fitting.Filter.sharedMesh, Corrected = copy });
                    fitting.Filter.sharedMesh = copy;
                }
                catch { Object.Destroy(copy); throw; }
            }
            foreach (var emitter in prairieEmitters)
            {
                if (emitter.part == null) continue;
                prairieEmitterBackups.Add((emitter.part, emitter.part.localPosition));
                emitter.part.position = body.TransformPoint(nativeToCurrent.MultiplyPoint3x4(
                    CylinderCasingShape.NormalizePrairie(emitter.native, sy, sz)));
            }
        }

        private void RestoreCylinderCasings()
        {
            // Tender statics share the locomotive's corrected meshes. Detach them
            // before those meshes are released; never leave a destroyed asset there.
            if (tenderEngineRoot != null) DetachTenderEngine();
            for (int i = prairieEmitterBackups.Count - 1; i >= 0; i--)
                if (prairieEmitterBackups[i].part != null) prairieEmitterBackups[i].part.localPosition = prairieEmitterBackups[i].local;
            prairieEmitterBackups.Clear();
            foreach (var entry in prairieFittingMeshes)
            {
                if (entry.Filter != null && entry.Filter.sharedMesh == entry.Corrected) entry.Filter.sharedMesh = entry.Original;
                if (entry.Corrected != null) Object.Destroy(entry.Corrected);
            }
            prairieFittingMeshes.Clear();
            foreach (var entry in casingMeshes)
            {
                if (entry.Filter != null && entry.Filter.sharedMesh == entry.Corrected)
                    entry.Filter.sharedMesh = entry.Original;
                if (entry.Corrected != null) Object.Destroy(entry.Corrected);
            }
            casingMeshes.Clear();
        }

        private void DisposeCylinderCasings()
        {
            RestoreCylinderCasings();
            foreach (var mesh in readableCasingSources) if (mesh != null) Object.Destroy(mesh);
            readableCasingSources.Clear(); casingTemplates.Clear();
            prairieFittings.Clear(); prairieEmitters.Clear(); franklinCasingsCached = false;
        }
    }
}
#endif
