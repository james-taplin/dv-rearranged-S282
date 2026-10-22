#if B99_ALL_STANDARD
using UnityEngine;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        // 1.13.6: translate the complete 1.13.5 mechanism rearward 0.70 m.
        // Both offsets change equally, retaining its wheel size and rod span.
        private const float PrairieDriverForward = .8f, PrairieCylinderForward = -.1f;

        // Reuse the existing native-clock retarget: move the drivers as a group,
        // move the cylinder side less, and shorten only the rods spanning them.
        // The guide's sliding stroke, crank throw and all simulation stay native.
        private void ApplyPrairieForwardSpacing(bool bindRetarget = true)
        {
            float scale = driveR.localScale.z;
            foreach (var drive in new[] { driveL, driveR })
            {
                drive.localPosition += Vector3.forward * PrairieDriverForward;
                if (bindRetarget) DriveRetarget.Bind(drive, (PrairieCylinderForward - PrairieDriverForward + prairieClearanceCylinderShiftM) / scale,
                    DriveRetarget.NativeWalschaertsRole, transform);
            }
            foreach (var part in prairieDriverFittings) part.Apply(driveR);
            var sparkShift = driveR.parent.TransformVector(Vector3.forward * PrairieDriverForward);
            foreach (var spark in new[] { spark1L, spark1R, spark2L, spark2R, spark3L, spark3R })
                if (spark != null) spark.position += sparkShift;
            // The first native axle is hidden on the three-driver engine.
            foreach (string side in new[] { "l", "r" })
                foreach (string lod in new[] { "Static_LOD0", "Static_LOD1" })
                {
                    var container = transform.Find("LocoS282A_Body/" + lod);
                    if (container == null) continue;
                    foreach (Transform split in container)
                    {
                        if (!split.name.StartsWith("s282a_split_body", System.StringComparison.Ordinal)) continue;
                        var spring = split.Find("s282a_suspension_" + side + "_1");
                        if (spring != null) spring.gameObject.SetActive(false);
                    }
                }
            Main.Logger.Log("Prairie spacing: complete mechanism 0.70 m rearward from 1.13.5; front driver +0.80 m, cylinder rear datum -0.10 m; retarget span " +
                (PrairieCylinderForward - PrairieDriverForward + prairieClearanceCylinderShiftM).ToString("F3") + " m; " +
                "reservoir-clear wheel size, native animation, carrying trucks, mass centre and cylinder simulation retained.");
        }

        private static Matrix4x4 CylinderDriveFrame(Transform drive)
        {
            var retarget = drive.GetComponent<DriveRetarget>();
            float shift = retarget != null ? retarget.Shift : 0f;
            return drive.localToWorldMatrix * Matrix4x4.TRS(Vector3.forward * shift, Quaternion.identity, Vector3.one);
        }
    }
}
#endif
