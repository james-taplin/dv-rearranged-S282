#if B99_ALL_STANDARD
using UnityEngine;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        // Actual native flange sweep against both reservoir LODs, including the
        // mounting straps: 1.11 leaves approximately 20 mm clearance. Retain the
        // front driver's longitudinal position, rather than moving the whole set.
        internal const float ReservoirClearDriverScale = 1.11f;
        private float prairieClearanceCylinderShiftM;

        internal static bool UsesReservoirClearance(int id) => id == (int)WheelArrangementType.s262 ||
            id == (int)WheelArrangementType.s280Big || id == (int)WheelArrangementType.s282Big ||
            id == (int)WheelArrangementType.s284Big;

        private void ApplyLargeDriverClearance(int id, float radius)
        {
            prairieClearanceCylinderShiftM = 0f;
            if (!UsesReservoirClearance(id)) return;
            bool prairie = id == (int)WheelArrangementType.s262;
            float oldScale = driveR.localScale.z;
            float ratio = ReservoirClearDriverScale / oldScale;
            float anchor = (prairie ? secondRightDriveWheel : firstRightDriveWheel).localPosition.z;
            var before = driveR.worldToLocalMatrix;
            foreach (var drive in new[] { driveL, driveR })
            {
                drive.localPosition += Vector3.forward * ((oldScale - ReservoirClearDriverScale) * anchor);
                drive.localScale = new Vector3(drive.localScale.x, drive.localScale.y * ratio, ReservoirClearDriverScale);
            }
            var sparkMap = driveR.localToWorldMatrix * before;
            foreach (var spark in new[] { spark1L, spark1R, spark2L, spark2R, spark3L, spark3R, spark4L, spark4R })
                if (spark != null) spark.position = sparkMap.MultiplyPoint3x4(spark.position);
            // Keep the Prairie's accepted rear casing datum in place despite the
            // slightly shorter native mechanism. Its existing retarget does this.
            if (prairie) prairieClearanceCylinderShiftM =
                (oldScale - ReservoirClearDriverScale) * (CylinderCasingShape.NativeRearFaceZ - anchor);
            setWheelRadius(radius * ratio);
            foreach (var part in prairieDriverFittings) part.Apply(driveR);
            Main.Logger.Log("Reservoir clearance: " + WheelArrangement.WheelArrangementNames[id] +
                "; driver diameter " + (2f * radius * ratio).ToString("F3") +
                " m; native mechanism scale " + ReservoirClearDriverScale.ToString("F3") +
                "; front driver position retained, carrying trucks unchanged.");
        }
    }
}
#endif
