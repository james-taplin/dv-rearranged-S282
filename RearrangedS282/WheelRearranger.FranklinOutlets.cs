#if B99_ALL_STANDARD
using UnityEngine;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        // Match the following cylinder's local nose correction. The emitter
        // centre, rather than its potentially offset object origin, locates the
        // fitting on that casting. This changes presentation outlets only.
        internal Vector3 FranklinRearOutletOffset(Vector3 nativeWorldCentre, int engine)
        {
            bool rear = currentWA == WheelArrangementType.s4444 && engine == 1;
            bool middle = (currentWA == WheelArrangementType.s22222 || currentWA == WheelArrangementType.s24440) &&
                engine == 2 && scaledEngineMaps != null && scaledEngineMaps.Length > engine;
            if (!rear && !middle) return Vector3.zero;
            var native = MalletBody.InverseTransformPoint(nativeWorldCentre);
            if (rear)
                return CylinderCasingShape.CompactFranklinRearNose(native) - native;
            // Simulation/effect index 2 is the triplex middle engine. Apply the
            // native profile delta through its existing cylinder map; origins
            // and particle emitter-centre offsets retain their native identity.
            return scaledEngineMaps[engine].MultiplyVector(CylinderCasingShape.CompactFranklinTriplexNose(native) - native);
        }
    }
}
#endif
