#if B99_ALL_STANDARD
using UnityEngine;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        // The 4-6-6-4 shares the 2-6-6-2's measured 3+3 native engines and
        // articulation pin. Keep the complete native setup and its lifetime;
        // only the carrying trucks differ.
        private void ApplyLittleBoyGeometry()
        {
            ApplyMalletGeometry();
            ShowFourLeadingWheels();
            ShowFourTrailingWheelsFor10Coupled();

            // Put the leading trailing axle at the measured Mallet clearance,
            // while retaining the working four-wheel truck's axle spacing and
            // moving its native support with the entire truck.
            float target = rearBogie.InverseTransformPoint(
                MalletBody.TransformPoint(new Vector3(0f, 0f, mallet.TrailingZ))).z;
            float shift = target - secondRearAxle.localPosition.z;
            Vector3 delta = Vector3.forward * shift;
            secondRearAxle.localPosition += delta;
            thirdRearAxle.localPosition += delta;
            secondRearAxleSupport.localPosition += delta;
        }
    }
}
#endif
