#if B99_ALL_STANDARD
using System.Collections.Generic;
using UnityEngine;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        private const string ReachPath = "s282_mech_cutoff_rod";
        private const string LubricatorRodPath = "s282_mech_cutoff_rail/s282_mech_lubricator_rod";
        private readonly Matrix4x4[] nativeLinkageBodyFrames = new Matrix4x4[2];
        private readonly Dictionary<MeshRenderer, bool> nativeLinkageVisibility = new Dictionary<MeshRenderer, bool>();
        private readonly List<Transform> hiddenLinkageDrives = new List<Transform>();
        private Matrix4x4 nativeLinkageOilerFrame;

        // Called before any arrangement edits. All coordinates below are native
        // mesh datums; each car/arrangement supplies its own actual moving frame.
        private void CacheHiddenLinkageFrames()
        {
            int side = 0;
            foreach (var drive in new[] { driveL, driveR })
            {
                nativeLinkageBodyFrames[side++] = transform.worldToLocalMatrix * drive.localToWorldMatrix;
                foreach (var path in new[] { ReachPath, LubricatorRodPath, "LubricatorParts" })
                {
                    var part = drive.Find(path);
                    if (part == null) continue;
                    foreach (var renderer in part.GetComponentsInChildren<MeshRenderer>(true))
                        nativeLinkageVisibility[renderer] = renderer.enabled;
                }
            }
            nativeLinkageOilerFrame = lubricator.worldToLocalMatrix * driveR.localToWorldMatrix;
        }

        internal static bool HasHiddenNativeLinkages(int id)
        {
            switch (id)
            {
                case 0: case 1: case 2: case 3: case 4: case 5:
                case 12: case 13: case 14: case 27: case 28: case 29: case 30:
                case 34: case 35: case 39: case 44: case 48: case 49:
                case 37: case 41: case 42: case 43: case 45: case 46: case 50:
                    return true;
                default: return false; // stock, 2-2-2 and replaced Franklin gear
            }
        }

        private void ApplyHiddenNativeLinkages(int id)
        {
            if (!HasHiddenNativeLinkages(id)) return;
            bool articulated = WheelArrangement.IsArticulatedId(id);
            int side = 0;
            foreach (var drive in new[] { driveL, driveR })
            {
                int index = side++;
                // Use the effective geometry visibility after its existing UMM
                // policy, including polished Pacific/Hudson/Prairie settings.
                if (!drive.Find("s282_mech_cutoff_rail").gameObject.activeSelf) continue;
                hiddenLinkageDrives.Add(drive);
                ShowNativeLinkage(drive.Find(ReachPath), articulated);
                DriveRetarget.AddFixedEndpoint(drive, transform, nativeLinkageBodyFrames[index], ReachPath,
                    -6.07456827f, transform, MeshFinder.Instance.ReadableRetargetMesh);
                DriveRetarget.AddFixedEndpoint(drive, transform, nativeLinkageBodyFrames[index], ReachPath + "/s282_mech_cutoff_rod_LOD1",
                    -5.74456787f, transform, MeshFinder.Instance.ReadableRetargetMesh);
            }
            if (!driveR.Find("s282_mech_cutoff_rail").gameObject.activeSelf) return;
            ShowNativeLinkage(driveR.Find(LubricatorRodPath), articulated);
            ShowNativeLinkage(driveR.Find("LubricatorParts"), articulated);
            // Native rod/lever trajectory fit: stock's authored residual is at
            // most 2.60 mm over the three native cutoff clips. Preserve it rather
            // than impose a different motion or drive the native ratchet twice.
            DriveRetarget.AddFixedEndpoint(driveR, lubricator, nativeLinkageOilerFrame, LubricatorRodPath,
                1.9299124f, transform, MeshFinder.Instance.ReadableRetargetMesh);
            DriveRetarget.AddFixedEndpoint(driveR, lubricator, nativeLinkageOilerFrame, LubricatorRodPath + "/s282_mech_lubricator_rod_LOD1",
                new Vector3(.001080511f, -.000262856f, 1.92260874f), transform, MeshFinder.Instance.ReadableRetargetMesh);
            DriveRetarget.AddAnchoredPart(driveR, lubricator, nativeLinkageOilerFrame, "LubricatorParts",
                transform, MeshFinder.Instance.ReadableRetargetMesh);
            // The old fixed mid-shaft guide does not meet the changed straight
            // span. Keep the existing resized-gear exclusion at both body LODs.
            foreach (var support in pacificHudsonReachSupports)
                if (support != null) support.gameObject.SetActive(false);
        }

        private void ShowNativeLinkage(Transform part, bool articulated)
        {
            part.gameObject.SetActive(true);
            foreach (var renderer in part.GetComponentsInChildren<MeshRenderer>(true))
                if (nativeLinkageVisibility.TryGetValue(renderer, out bool enabled))
                    // Articulated engines retain their existing detail-through-
                    // far-cull LOD policy; do not revive the rigid coarse copies.
                    renderer.enabled = enabled && (!articulated || !renderer.name.EndsWith("_LOD1"));
        }

        private void SuspendHiddenNativeLinkages()
        {
            // Both owners can replace entries in one native LOD group. Unwind
            // the last owner first, before any geometry/LOD snapshots restore.
            for (int i = hiddenLinkageDrives.Count - 1; i >= 0; i--)
                DriveRetarget.Unbind(hiddenLinkageDrives[i]);
            hiddenLinkageDrives.Clear();
        }
    }
}
#endif
