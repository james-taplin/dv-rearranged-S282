#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        private const string PitchedWheelPrefix = "[Rearranged pitched wheel ";
        private readonly Dictionary<Transform, NativePitchWheel> pitchWheels = new Dictionary<Transform, NativePitchWheel>();
        private readonly Dictionary<Transform, Transform> presentedPitchWheels = new Dictionary<Transform, Transform>();
        private readonly Dictionary<Transform, Vector3> pitchNativeMainPins = new Dictionary<Transform, Vector3>();
        private readonly List<PitchFitting> pitchFittings = new List<PitchFitting>();
        private readonly List<Action> pitchRestore = new List<Action>();
        private readonly List<PitchLodState> pitchLods = new List<PitchLodState>();
        private readonly Dictionary<int, PitchRodMesh> pitchRodMeshes = new Dictionary<int, PitchRodMesh>();
        private readonly HashSet<int> pitchMeshSlotsUsed = new HashSet<int>();
        private DriverLayout activeDriverLayout;
        private string driverLayoutPreparationError;
        private Mesh pitchFlangedLod1;

        internal int DriverLayoutPresentationCount => presentedPitchWheels.Count;
        internal float DriverLayoutExtraPitchM => activeDriverLayout?.ExtraCoupledPitchM ?? 0f;
        internal DriverLayout ActiveDriverLayout => activeDriverLayout;

        private sealed class NativePitchWheel
        {
            internal Transform Source, Presentation;
            internal Vector3 NativeBodyCentre;
            internal MeshFilter SourceDetail, SourceCoarse, Detail, Coarse;
            internal MeshRenderer DetailRenderer, CoarseRenderer;
        }

        private sealed class PitchFitting
        {
            internal Transform Part;
            internal string Side, Kind, Lod;
            internal int Number;
            internal Vector3 NativeBodyPosition;
        }

        private sealed class PitchRodMesh
        {
            internal Mesh Source, Corrected;
            internal float[] TargetPins;
        }

        private sealed class PitchLodState
        {
            internal LODGroup Group;
            internal LOD[] Lods;
            internal Vector3 ReferencePoint;
            internal float Size;
        }

        // Called once, while the native geometry is still in its prepared pose.
        // These optional layouts must not block unrelated arrangements on a
        // different native prefab; reject only a later request for these layouts.
        private void CacheDriverLayouts()
        {
            DisposeDriverLayouts();
            driverLayoutPreparationError = null;
            try
            {
                foreach (var drive in new[] { driveL, driveR })
                {
                    var main = drive != null ? drive.Find("s282_mech_wheels_connect/s282_mech_push_rod_to_connect") : null;
                    if (main == null) throw new InvalidOperationException("The native main-rod pin is missing.");
                    // Main-pin translation has tiny authored animation, and L/R
                    // can already be at different phases when first converted.
                    // Read the unanimated native prefab datum, as the existing
                    // particle integration does, instead of freezing a live pose
                    // or the legacy setter's temporary rounded -1.55 position.
                    var prefab = loco != null ? loco.carLivery?.prefab : null;
                    var template = prefab != null ? prefab.transform.Find("LocoS282A_Body/MovingParts_LOD0/" + drive.name +
                        "/s282_mech_wheels_connect/s282_mech_push_rod_to_connect") : null;
                    if (template == null) throw new InvalidOperationException("The native main-rod prefab datum is missing.");
                    pitchNativeMainPins.Add(drive, template.localPosition);
                    for (int number = 1; number <= 4; number++)
                    {
                        var source = drive != null ? drive.Find("s282_wheels_driving_" + number) : null;
                        var detail = source != null ? source.GetComponent<MeshFilter>() : null;
                        var coarse = source != null ? source.Find("s282_wheels_driving_" + number + "_LOD1")?.GetComponent<MeshFilter>() : null;
                        if (detail == null || detail.sharedMesh == null || coarse == null || coarse.sharedMesh == null ||
                            detail.GetComponent<MeshRenderer>() == null || coarse.GetComponent<MeshRenderer>() == null)
                            throw new InvalidOperationException("A native driver detail/coarse mesh is missing: " + number);
                        pitchWheels.Add(source, new NativePitchWheel { Source = source, SourceDetail = detail,
                            SourceCoarse = coarse, NativeBodyCentre = transform.InverseTransformPoint(source.position) });
                        if (drive == driveR && number == 1) pitchFlangedLod1 = coarse.sharedMesh;
                    }
                }
                foreach (string lod in new[] { "Static_LOD0", "Static_LOD1" })
                {
                    var container = transform.Find("LocoS282A_Body/" + lod);
                    var split = container != null ? container.Cast<Transform>().FirstOrDefault(t =>
                        t.name.StartsWith("s282a_split_body", StringComparison.Ordinal)) : null;
                    if (split == null) throw new InvalidOperationException("Native split fittings are missing: " + lod);
                    foreach (string side in new[] { "l", "r" })
                        foreach (string kind in new[] { "caliper", "suspension" })
                            for (int number = 1; number <= 4; number++)
                            {
                                var part = split.Find("s282a_" + kind + "_" + side + "_" + number);
                                // Installed LMS has detailed suspension pieces,
                                // but does not split separate coarse suspensions.
                                if (part == null && kind == "suspension" && lod == "Static_LOD1") continue;
                                if (part == null) throw new InvalidOperationException("Native driver fitting is missing: " + lod + "/" + side + "/" + kind + "/" + number);
                                pitchFittings.Add(new PitchFitting { Part = part, Side = side, Kind = kind, Number = number,
                                    Lod = lod, NativeBodyPosition = transform.InverseTransformPoint(part.position) });
                            }
                }
            }
            catch (Exception ex) { driverLayoutPreparationError = "Driver pitch could not be prepared: " + ex.Message; }
        }

        internal static float RequestedDriverPitchM(int id)
        {
            switch ((WheelArrangementType)id)
            {
                case WheelArrangementType.s260: return .18f;
                case WheelArrangementType.s240: return .20f;
                case WheelArrangementType.s262: return .22f;
                case WheelArrangementType.s440: return 0f;
                default: throw new ArgumentOutOfRangeException(nameof(id));
            }
        }

        private void ApplyDriverLayout(int id)
        {
            if (!DriverLayout.Supports(id)) return;
            if (driverLayoutPreparationError != null) throw new InvalidOperationException(driverLayoutPreparationError);
            if (pitchRestore.Count != 0 || activeDriverLayout != null)
                throw new InvalidOperationException("Restore the previous driver pitch before applying another layout.");
            float requested = RequestedDriverPitchM(id);
            var layout = DriverLayout.Create(id, requested);
            var axis = transform.TransformDirection(Vector3.forward).normalized;
            float driveScale = driveR.TransformVector(Vector3.forward).magnitude;
            if (!(driveScale > .01f)) throw new InvalidOperationException("Native driver scale is invalid.");
            activeDriverLayout = layout;
            curveGroups = null;
            try
            {
                var drives = new[] { driveL, driveR };
                var replacements = new Dictionary<Renderer, Renderer[]>();
                var ownership = new Dictionary<Transform, (Renderer[] detail, Renderer[] coarse)>();
                foreach (var drive in drives)
                {
                    var detailRenderers = new List<Renderer>();
                    var coarseRenderers = new List<Renderer>();
                    foreach (int number in layout.Drivers)
                    {
                        var source = drive.Find("s282_wheels_driving_" + number);
                        if (source == null || !pitchWheels.TryGetValue(source, out var wheel) || !source.gameObject.activeInHierarchy)
                            throw new InvalidOperationException("The selected layout has no active native driver " + number);
                        PreparePitchedWheel(wheel, axis * layout.DriverOffsetM(number));
                        presentedPitchWheels.Add(source, wheel.Presentation);
                        replacements.Add(wheel.SourceDetail.GetComponent<MeshRenderer>(), new Renderer[] { wheel.DetailRenderer });
                        replacements.Add(wheel.SourceCoarse.GetComponent<MeshRenderer>(), new Renderer[] { wheel.CoarseRenderer });
                        detailRenderers.Add(wheel.DetailRenderer); coarseRenderers.Add(wheel.CoarseRenderer);
                    }
                    var side = drive == driveL ? leftSideRod : rightSideRod;
                    var rods = layout.ThreeDrivers
                        ? (drive == driveL ? new[] { lfThreeAxleSideRod, lrThreeAxleSideRod } : new[] { rfThreeAxleSideRod, rrThreeAxleSideRod })
                        : (drive == driveL ? new[] { leftTwoAxleSideRod } : new[] { rightTwoAxleSideRod });
                    var correctRods = new List<Renderer>();
                    for (int segment = 0; segment < rods.Length; segment++)
                    {
                        var rod = rods[segment];
                        var filter = rod.GetComponent<MeshFilter>();
                        var original = filter.sharedMesh;
                        RememberMesh(filter);
                        var targetPins = PitchedSegmentPins(layout, drive, side, rod, segment);
                        filter.sharedMesh = PitchedRodMesh(layout, segment, original, targetPins);
                        var renderer = rod.GetComponent<MeshRenderer>();
                        if (renderer == null || !rod.gameObject.activeInHierarchy)
                            throw new InvalidOperationException("The selected layout has no active short coupling rod.");
                        correctRods.Add(renderer);
                    }
                    var nativeCoarse = side.Find("s282_mech_wheels_connect_LOD1");
                    foreach (var native in new[] { side, nativeCoarse })
                    {
                        var filter = native != null ? native.GetComponent<MeshFilter>() : null;
                        var renderer = native != null ? native.GetComponent<MeshRenderer>() : null;
                        if (filter == null || renderer == null) throw new InvalidOperationException("Native coupling rod LOD is missing.");
                        RememberMesh(filter); filter.sharedMesh = null;
                        replacements.Add(renderer, correctRods.ToArray());
                    }
                    detailRenderers.AddRange(correctRods); coarseRenderers.AddRange(correctRods);
                    ownership.Add(drive, (detailRenderers.ToArray(), coarseRenderers.ToArray()));
                }
                ApplyPitchedFittings(layout, axis);
                BindPitchedLods(replacements, ownership);
                Main.Logger.Log("Driver pitch: " + WheelArrangement.WheelArrangementNames[id] + "; extra coupled pitch " +
                    layout.ExtraCoupledPitchM.ToString("F2") + " m; native main axle, wheel diameter, cylinder motion and simulation retained.");
            }
            catch { SuspendDriverLayout(); throw; }
        }

        // The native clips bind wheel position and scale as well as rotation.
        // A renderer-only sibling follows the existing completed native rotation
        // in LateUpdate, leaving every source transform and child joint intact.
        private void PreparePitchedWheel(NativePitchWheel wheel, Vector3 worldOffset)
        {
            var source = wheel.Source;
            if (wheel.Presentation == null)
            {
                var root = new GameObject(PitchedWheelPrefix + source.parent.name + " " + source.name + "]").transform;
                root.SetParent(source.parent, false);
                root.gameObject.layer = source.gameObject.layer;
                wheel.Detail = root.gameObject.AddComponent<MeshFilter>();
                wheel.DetailRenderer = root.gameObject.AddComponent<MeshRenderer>();
                root.gameObject.AddComponent<PoweredWheelRotater>().Init(source.gameObject);
                var coarse = new GameObject("coarse").transform;
                coarse.SetParent(root, false); coarse.gameObject.layer = source.gameObject.layer;
                wheel.Coarse = coarse.gameObject.AddComponent<MeshFilter>();
                wheel.CoarseRenderer = coarse.gameObject.AddComponent<MeshRenderer>();
                wheel.Presentation = root;
            }
            wheel.Presentation.localPosition = source.localPosition + source.parent.InverseTransformVector(worldOffset);
            wheel.Presentation.localRotation = source.localRotation;
            wheel.Presentation.localScale = source.localScale;
            wheel.Coarse.transform.localPosition = wheel.SourceCoarse.transform.localPosition;
            wheel.Coarse.transform.localRotation = wheel.SourceCoarse.transform.localRotation;
            wheel.Coarse.transform.localScale = wheel.SourceCoarse.transform.localScale;
            wheel.Detail.sharedMesh = wheel.SourceDetail.sharedMesh;
            wheel.Coarse.sharedMesh = wheel.SourceDetail.sharedMesh == flangedDriver
                ? pitchFlangedLod1 : wheel.SourceCoarse.sharedMesh;
            CopyPitchRenderer(wheel.SourceDetail.GetComponent<MeshRenderer>(), wheel.DetailRenderer);
            CopyPitchRenderer(wheel.SourceCoarse.GetComponent<MeshRenderer>(), wheel.CoarseRenderer);
            wheel.Presentation.gameObject.SetActive(true);
            wheel.Coarse.gameObject.SetActive(true);
            RememberMesh(wheel.SourceDetail); RememberMesh(wheel.SourceCoarse);
            wheel.SourceDetail.sharedMesh = null; wheel.SourceCoarse.sharedMesh = null;
        }

        private static void CopyPitchRenderer(MeshRenderer source, MeshRenderer target)
        {
            target.sharedMaterials = source.sharedMaterials;
            target.shadowCastingMode = source.shadowCastingMode;
            target.receiveShadows = source.receiveShadows;
            target.lightProbeUsage = source.lightProbeUsage;
            target.reflectionProbeUsage = source.reflectionProbeUsage;
            target.enabled = true;
        }

        private float[] PitchedSegmentPins(DriverLayout layout, Transform drive, Transform side, Transform segment, int segmentNumber)
        {
            // All the wheels in this group share the same crank displacement.
            // Centre differences cancel the parent animated translation. The
            // main attachment uses its unanimated native-prefab datum. Keep the
            // coupling rod's tiny authored rocking rotation in the vector basis;
            // its longitudinal effect is below a micrometre on these spans.
            // Cache reuse allows one micrometre, without adding a motion solver.
            var main = ResolvePresentedWheel(drive.Find("s282_wheels_driving_3"));
            var sideFromDriveVector = Matrix4x4.TRS(Vector3.zero, side.localRotation, side.localScale).inverse;
            var meshFromSide = Matrix4x4.TRS(segment.localPosition, segment.localRotation, segment.localScale).inverse;
            int[] pinDrivers = !layout.ThreeDrivers ? new[] { 3, 2 }
                : new[] { segmentNumber == 0 ? 2 : 4, 3 };
            return pinDrivers.Select(number =>
            {
                var wheel = ResolvePresentedWheel(drive.Find("s282_wheels_driving_" + number));
                var centreGap = Vector3.forward * (wheel.localPosition.z - main.localPosition.z);
                var pinInSide = pitchNativeMainPins[drive] + sideFromDriveVector.MultiplyVector(centreGap);
                return meshFromSide.MultiplyPoint3x4(pinInSide).z;
            }).ToArray();
        }

        private Mesh PitchedRodMesh(DriverLayout layout, int segment, Mesh source, float[] targetPins)
        {
            if (source == null || !source.isReadable) throw new InvalidOperationException("The selected short coupling rod is not readable.");
            int slot = layout.ArrangementId * 10 + segment;
            if (pitchRodMeshes.TryGetValue(slot, out var cached))
            {
                if (cached.Source == source && SamePitchPins(cached.TargetPins, targetPins) && cached.Corrected != null)
                {
                    pitchMeshSlotsUsed.Add(slot);
                    return cached.Corrected;
                }
                if (pitchMeshSlotsUsed.Contains(slot))
                    throw new InvalidOperationException("Left/right coupling-rod frames do not share a compatible pitch mesh.");
                if (cached.Corrected != null) Object.Destroy(cached.Corrected);
                pitchRodMeshes.Remove(slot);
            }
            var vertices = RodRemesher.Remesh(source.vertices, layout.OriginalRodPins(), targetPins);
            var copy = Object.Instantiate(source);
            try
            {
                copy.name = "[Rearranged pitched coupling rod] " + source.name;
                copy.vertices = vertices; copy.RecalculateBounds(); copy.RecalculateNormals();
                if (copy.uv.Length == copy.vertexCount) copy.RecalculateTangents();
                pitchRodMeshes.Add(slot, new PitchRodMesh { Source = source, Corrected = copy, TargetPins = (float[])targetPins.Clone() });
                pitchMeshSlotsUsed.Add(slot);
                return copy;
            }
            catch { Object.Destroy(copy); throw; }
        }

        private static bool SamePitchPins(float[] a, float[] b)
            => a != null && b != null && a.Length == b.Length && a.Zip(b, (x, y) => Mathf.Abs(x - y)).All(d => d <= .000001f);

        private void ApplyPitchedFittings(DriverLayout layout, Vector3 axis)
        {
            // LOD0 carries the original per-layout caliper translations; LMS's
            // matching coarse split parts share their native body-space frame.
            var caliperLegacy = pitchFittings.Where(p => p.Kind == "caliper" && p.Lod == "Static_LOD0")
                .ToDictionary(p => p.Side + p.Number, p => p.Part.position - transform.TransformPoint(p.NativeBodyPosition));
            foreach (var fitting in pitchFittings)
            {
                RememberPitchTransform(fitting.Part);
                int driver = !layout.ThreeDrivers && fitting.Kind == "caliper" && fitting.Number == 1
                    ? 2 : fitting.Number;
                bool active = layout.Drivers.Contains(driver) && !(!layout.ThreeDrivers &&
                    fitting.Kind == "caliper" && fitting.Number == 2);
                fitting.Part.gameObject.SetActive(active);
                if (!active) continue;
                var drive = fitting.Side == "l" ? driveL : driveR;
                var source = drive.Find("s282_wheels_driving_" + driver);
                if (fitting.Kind == "suspension")
                {
                    var native = pitchWheels[source];
                    float displacement = transform.InverseTransformPoint(ResolvePresentedWheel(source).position).z - native.NativeBodyCentre.z;
                    fitting.Part.position += transform.TransformVector(Vector3.forward * displacement);
                }
                else
                {
                    if (fitting.Lod == "Static_LOD1") fitting.Part.position += caliperLegacy[fitting.Side + fitting.Number];
                    fitting.Part.position += axis * layout.DriverOffsetM(driver);
                }
            }
            var leftShoes = layout.ThreeDrivers
                ? new[] { leftSecondBrakeShoe, leftThirdBrakeShoe, leftFourthBrakeShoe } : new[] { leftFirstBrakeShoe, leftThirdBrakeShoe };
            var rightShoes = layout.ThreeDrivers
                ? new[] { rightSecondBrakeShoe, rightThirdBrakeShoe, rightFourthBrakeShoe } : new[] { rightFirstBrakeShoe, rightThirdBrakeShoe };
            var leftSparks = new[] { spark1L, spark2L, spark3L };
            var rightSparks = new[] { spark1R, spark2R, spark3R };
            for (int i = 0; i < layout.Drivers.Count; i++)
            {
                var delta = axis * layout.DriverOffsetM(layout.Drivers[i]);
                foreach (var part in new[] { leftShoes[i], rightShoes[i], leftSparks[i], rightSparks[i] })
                {
                    if (part == null) throw new InvalidOperationException("A selected driver shoe or spark anchor is missing.");
                    RememberPitchTransform(part); part.position += delta;
                }
            }
        }

        private void BindPitchedLods(Dictionary<Renderer, Renderer[]> replacements,
            Dictionary<Transform, (Renderer[] detail, Renderer[] coarse)> presentation)
        {
            var body = transform.Find("LocoS282A_Body");
            var groups = body != null ? body.GetComponentsInChildren<LODGroup>(true).Where(g => g.GetLODs().Length != 0).ToArray()
                : Array.Empty<LODGroup>();
            var additions = new Dictionary<LODGroup, (List<Renderer> detail, List<Renderer> coarse)>();
            foreach (var entry in presentation)
            {
                var sourceRenderer = pitchWheels[entry.Key.Find("s282_wheels_driving_2")].SourceDetail.GetComponent<MeshRenderer>();
                var owners = groups.Where(g => g.GetLODs().Any(l => l.renderers.Contains(sourceRenderer))).ToArray();
                if (owners.Length != 1) throw new InvalidOperationException("A unique native drivetrain LOD owner was not found.");
                if (!additions.TryGetValue(owners[0], out var lists))
                    additions.Add(owners[0], lists = (new List<Renderer>(), new List<Renderer>()));
                lists.detail.AddRange(entry.Value.detail); lists.coarse.AddRange(entry.Value.coarse);
            }
            var targets = new HashSet<Renderer>(presentation.Values.SelectMany(p => p.detail.Concat(p.coarse)));
            foreach (var group in groups)
            {
                var original = group.GetLODs();
                bool affected = additions.ContainsKey(group) || original.Any(l => l.renderers.Any(r => r != null && (replacements.ContainsKey(r) || targets.Contains(r))));
                if (!affected) continue;
                var nativeReference = group.localReferencePoint;
                float nativeSize = group.size;
                pitchLods.Add(new PitchLodState { Group = group, Lods = original, ReferencePoint = nativeReference, Size = nativeSize });
                var updated = original.Select((lod, index) =>
                {
                    var keep = lod.renderers.Where(r => r != null && !replacements.ContainsKey(r) && !targets.Contains(r));
                    if (additions.TryGetValue(group, out var add)) keep = keep.Concat(index == 0 ? add.detail : add.coarse);
                    return new LOD(lod.screenRelativeTransitionHeight, keep.Distinct().ToArray()) { fadeTransitionWidth = lod.fadeTransitionWidth };
                }).ToArray();
                group.SetLODs(updated); group.RecalculateBounds();
                // RecalculateBounds sees only this frame's pose. Retain the
                // authored native animation envelope as well as the new pitch,
                // otherwise the crosshead can leave the bound later in its stroke.
                var nativeHalf = Vector3.one * (nativeSize * .5f);
                var posedHalf = Vector3.one * (group.size * .5f);
                var nativeMin = nativeReference - nativeHalf; var nativeMax = nativeReference + nativeHalf;
                var posedMin = group.localReferencePoint - posedHalf; var posedMax = group.localReferencePoint + posedHalf;
                var min = new Vector3(Mathf.Min(nativeMin.x, posedMin.x), Mathf.Min(nativeMin.y, posedMin.y), Mathf.Min(nativeMin.z, posedMin.z));
                var max = new Vector3(Mathf.Max(nativeMax.x, posedMax.x), Mathf.Max(nativeMax.y, posedMax.y), Mathf.Max(nativeMax.z, posedMax.z));
                var size = max - min;
                group.localReferencePoint = (min + max) * .5f;
                group.size = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
            }
        }

        internal Transform ResolvePresentedWheel(Transform source)
            => source != null && presentedPitchWheels.TryGetValue(source, out var presented) && presented != null
                ? presented : source;

        private void RememberMesh(MeshFilter filter)
        {
            var mesh = filter.sharedMesh;
            pitchRestore.Add(() => { if (filter != null) filter.sharedMesh = mesh; });
        }

        private void RememberPitchTransform(Transform part)
        {
            var position = part.localPosition; var rotation = part.localRotation; var scale = part.localScale;
            bool active = part.gameObject.activeSelf;
            pitchRestore.Add(() => { if (part != null) { part.localPosition = position; part.localRotation = rotation;
                part.localScale = scale; part.gameObject.SetActive(active); } });
        }

        private void SuspendDriverLayout()
        {
            foreach (var entry in pitchLods)
                if (entry.Group != null) { entry.Group.SetLODs(entry.Lods); entry.Group.localReferencePoint = entry.ReferencePoint; entry.Group.size = entry.Size; }
            pitchLods.Clear();
            for (int i = pitchRestore.Count - 1; i >= 0; i--) pitchRestore[i]();
            pitchRestore.Clear();
            foreach (var wheel in pitchWheels.Values) if (wheel.Presentation != null) wheel.Presentation.gameObject.SetActive(false);
            presentedPitchWheels.Clear(); pitchMeshSlotsUsed.Clear(); activeDriverLayout = null;
            curveGroups = null;
        }

        private void DisposeDriverLayouts()
        {
            SuspendDriverLayout();
            foreach (var mesh in pitchRodMeshes.Values) if (mesh.Corrected != null) Object.Destroy(mesh.Corrected);
            pitchRodMeshes.Clear();
            foreach (var wheel in pitchWheels.Values) if (wheel.Presentation != null) Object.Destroy(wheel.Presentation.gameObject);
            pitchWheels.Clear(); pitchNativeMainPins.Clear(); pitchFittings.Clear(); pitchFlangedLod1 = null;
        }
    }
}
#endif
