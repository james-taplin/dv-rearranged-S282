#if B99_ALL_STANDARD
using UnityEngine;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        // Match the fixed rear cylinder's local nose correction. The emitter
        // centre, rather than its potentially offset object origin, locates the
        // fitting on that casting. This changes presentation outlets only.
        internal Vector3 FranklinRearOutletOffset(Vector3 nativeWorldCentre, int engine)
        {
            if (currentWA != WheelArrangementType.s4444 || engine != 1) return Vector3.zero;
            var native = MalletBody.InverseTransformPoint(nativeWorldCentre);
            return CylinderCasingShape.CompactFranklinRearNose(native) - native;
        }
    }
}
#endif
