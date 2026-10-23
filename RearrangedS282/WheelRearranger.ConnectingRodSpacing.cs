#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        internal const float DefaultDriverGroupRearwardM = .15f, MaximumDriverGroupRearwardM = .25f;
        private readonly List<Transform> connectingRodSupports = new List<Transform>();
        private readonly List<Transform> connectingRodRetargetDrives = new List<Transform>();
        private readonly List<Action> connectingRodSpacingRestore = new List<Action>();
        private bool connectingRodSpacingActive;
        private string connectingRodSpacingPreparationError;
        internal float DriverGroupRearwardM { get; private set; }
        internal bool ConnectingRodSpacingActive => connectingRodSpacingActive;
        internal int ConnectingRodSpacingRetargetCount => connectingRodRetargetDrives.Count;

        internal static float ClampDriverGroupRearwardMetres(float requested)
        {
            if (float.IsNaN(requested) || float.IsInfinity(requested)) requested = DefaultDriverGroupRearwardM;
            return Math.Max(0f, Math.Min(MaximumDriverGroupRearwardM,
                (float)Math.Round(requested * 100d, MidpointRounding.AwayFromZero) / 100f));
        }

        // Native motion remains owned by the existing drivetrain Animators.
        // Only the unanimated whole-drive roots and detached static fittings
        // move; the current pitch presentations retain their relative layout.
        private void CacheConnectingRodSpacing()
        {
            DisposeConnectingRodSpacing();
            connectingRodSpacingPreparationError = null;
            try
            {
                if (driveL == null || driveR == null)
                    throw new InvalidOperationException("Native drivetrain roots are missing.");
                foreach (string lod in new[] { "Static_LOD0", "Static_LOD1" })
                {
                    var container = transform.Find("LocoS282A_Body/" + lod);
                    var split = container != null ? container.Cast<Transform>().FirstOrDefault(t =>
                        t.name.StartsWith("s282a_split_body", StringComparison.Ordinal)) : null;
                    // Keep optional supports in their native visibility state.
                    // Their absence must not block unrelated arrangements.
                    foreach (string side in new[] { "l", "r" })
                    {
                        var support = split != null ? split.Find("s282a_lifting_arm_support_" + side) : null;
                        if (support != null) connectingRodSupports.Add(support);
                    }
                }
            }
            catch (Exception ex)
            {
                connectingRodSpacingPreparationError = "Main connecting-rod spacing could not be prepared: " + ex.Message;
            }
        }

        // Apply after the coupling pitch has been seated and registered in LODs.
        // A positive setting moves the whole driver group rearward; compensating
        // retargeting keeps the rendered crosshead/valve side at its old position.
        internal static float RequestedDriverRearwardM(int id)
        {
            switch ((WheelArrangementType)id)
            {
                case WheelArrangementType.s260: return .12f;
                case WheelArrangementType.s240: return .14f;
                case WheelArrangementType.s262: return .12f;
                case WheelArrangementType.s440: return 0f;
                default: throw new ArgumentOutOfRangeException(nameof(id));
            }
        }

        private void ApplyConnectingRodSpacing(int id, float cylinderSideShiftM = 0f)
        {
            if (!DriverLayout.Supports(id)) return;
            if (connectingRodSpacingPreparationError != null)
                throw new InvalidOperationException(connectingRodSpacingPreparationError);
            if (connectingRodSpacingActive || connectingRodSpacingRestore.Count != 0)
                throw new InvalidOperationException("Restore the previous main-rod spacing before applying another layout.");
            if (activeDriverLayout == null || activeDriverLayout.ArrangementId != id)
                throw new InvalidOperationException("Apply the selected coupling-rod layout before main-rod spacing.");
            float requested = RequestedDriverRearwardM(id);
            float amount = ClampDriverGroupRearwardMetres(requested);
            connectingRodSpacingActive = true;
            DriverGroupRearwardM = amount;
            curveGroups = null;
            float retargetShiftM = amount + cylinderSideShiftM;
            if (amount == 0f && Mathf.Abs(retargetShiftM) < .0001f) return;
            try
            {
                var delta = -transform.TransformDirection(Vector3.forward).normalized * amount;
                foreach (var drive in new[] { driveL, driveR })
                {
                    RememberConnectingRodPosition(drive);
                    drive.position += delta;
                }
                // Shoes and all the rotating/animated parts already inherit the
                // drive-root translation. Calipers, detailed springs, static
                // lifting-arm supports and sparks have separate native parents.
                var sparks = new[] { spark1L, spark1R, spark2L, spark2R, spark3L, spark3R }
                    .Take(activeDriverLayout.Drivers.Count * 2).ToArray();
                if (sparks.Any(s => s == null))
                    throw new InvalidOperationException("A selected driver spark anchor is missing.");
                var detached = pitchFittings.Where(p => p.Part != null && p.Part.gameObject.activeSelf)
                    .Select(p => p.Part).Concat(connectingRodSupports)
                    // Selection follows powered-manager order, independently
                    // of whether a particle effect happens to be visible now.
                    .Concat(sparks).Distinct();
                foreach (var part in detached)
                {
                    if (part == null || part.IsChildOf(driveL) || part.IsChildOf(driveR)) continue;
                    RememberConnectingRodPosition(part);
                    part.position += delta;
                }
                foreach (var drive in new[] { driveL, driveR })
                {
                    float scale = drive.TransformVector(Vector3.forward).magnitude;
                    if (!(scale > .01f) || float.IsInfinity(scale))
                        throw new InvalidOperationException("Native drivetrain scale is invalid for main-rod spacing.");
                    // Record ownership before binding, so a partially failed
                    // bind is also unbound before the pitched LODs are restored.
                    connectingRodRetargetDrives.Add(drive);
                    DriveRetarget.BindNativePrecision(drive, retargetShiftM / scale, transform, MeshFinder.Instance.ReadableRetargetMesh);
                }
                Main.Logger.Log("Main connecting-rod spacing: " + WheelArrangement.WheelArrangementNames[id] +
                    "; driver group rearward " + amount.ToString("F2") +
                    " m; cylinder/guide position, coupled pitch, wheel size and native motion retained.");
            }
            catch { SuspendConnectingRodSpacing(); throw; }
        }

        private void RememberConnectingRodPosition(Transform part)
        {
            var position = part.localPosition;
            connectingRodSpacingRestore.Add(() => { if (part != null) part.localPosition = position; });
        }

        // Precision retargeting saves the already pitched LOD list. Undo that
        // outer layer first, then undo these translations, before pitch/snapshot
        // restoration can put back the untouched native geometry and bindings.
        private void SuspendConnectingRodSpacing()
        {
            // The two drives may share a native LOD owner. Later bindings save
            // the earlier binding's proxy list, so unwind that stack in reverse.
            for (int i = connectingRodRetargetDrives.Count - 1; i >= 0; i--)
                DriveRetarget.Unbind(connectingRodRetargetDrives[i]);
            connectingRodRetargetDrives.Clear();
            for (int i = connectingRodSpacingRestore.Count - 1; i >= 0; i--) connectingRodSpacingRestore[i]();
            connectingRodSpacingRestore.Clear();
            connectingRodSpacingActive = false;
            DriverGroupRearwardM = 0f;
            curveGroups = null;
        }

        private void DisposeConnectingRodSpacing()
        {
            SuspendConnectingRodSpacing();
            connectingRodSupports.Clear();
        }
    }
}
#endif
