#if B99_ALL_STANDARD
using System;
using System.Linq;
using UnityEngine;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        // The native tread is tapered: its two measured rings straddle 0.712 m.
        // Keep the Pacific's front driver datum while enlarging the whole
        // native three-axle mechanism; this avoids moving the air reservoirs.
        internal const float ScotchDriverScale = 1.335f;
        internal const float ScotchNativeTreadRadiusM = .712f;
        private const float PacificDriverScale = 1.2879f;
        private const float PacificCylinderLiftM = .21f;

        // Invoked only after the existing Pacific geometry on a restored car.
        // GeometryState owns the modified native transforms and wheel bindings.
        private void ApplyScotchGliderGeometry()
        {
            float oldScale = driveR.localScale.z;
            if (Mathf.Abs(oldScale - PacificDriverScale) > .001f ||
                Mathf.Abs(driveR.localScale.y - PacificDriverScale) > .001f)
                throw new InvalidOperationException("Scotch Glider requires the restored native Pacific drivetrain.");

            float driverDatumZ = secondRightDriveWheel.localPosition.z;
            float shiftZ = (oldScale - ScotchDriverScale) * driverDatumZ;
            float liftY = (ScotchDriverScale - oldScale) * secondRightDriveWheel.localPosition.y;

            // These objects are outside the drivetrain hierarchy. Preserve
            // their existing relative frame while the native mechanism grows.
            var fittings = new[] { leftSecondBrakeCaliper, leftThirdBrakeCaliper, leftFourthBrakeCaliper,
                rightSecondBrakeCaliper, rightThirdBrakeCaliper, rightFourthBrakeCaliper,
                spark1L, spark1R, spark2L, spark2R, spark3L, spark3R };
            if (fittings.Any(t => t == null))
                throw new InvalidOperationException("Scotch Glider is missing a native brake or spark anchor.");
            var local = fittings.Select(t => driveR.InverseTransformPoint(t.position)).ToArray();
            foreach (var drive in new[] { driveL, driveR })
            {
                drive.localPosition += Vector3.forward * shiftZ;
                drive.localScale = new Vector3(drive.localScale.x, ScotchDriverScale, ScotchDriverScale);
            }
            for (int i = 0; i < fittings.Length; i++)
                fittings[i].position = driveR.TransformPoint(local[i]);

            // LMS applies absolute cylinder offsets, including both native
            // casing LODs and particle outlets. The common Pacific casing and
            // valve-gear paths subsequently fit to the final native frame.
            splitS282A.MoveCylinders(new Vector3(0f, PacificCylinderLiftM + liftY, shiftZ));
            lfCrossheadBracket.localPosition += new Vector3(0f, liftY, shiftZ);
            rfCrossheadBracket.localPosition += new Vector3(0f, liftY, shiftZ);

            setWheelRadius(ScotchNativeTreadRadiusM * ScotchDriverScale);
        }
    }
}
#endif
