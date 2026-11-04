#if B99_ALL_STANDARD
namespace RearrangedS282
{
    // The third native engine can serve a tender unless three locomotive groups
    // already own it. Unpowered novelty/unfinished giant prototype stay excluded.
    internal static class PoweredTenderPolicy
    {
        internal static bool Supports(int id) => WheelArrangement.IsSupportedId(id) &&
            id != 33 && id != 36 && id != (int)WheelArrangementType.s22222 && id != (int)WheelArrangementType.s02220 && id != (int)WheelArrangementType.s24440 &&
            id != (int)WheelArrangementType.s4664LittleBoy;
        internal static float RpmRatio(float hostRadius, float tenderRadius)
        {
            if (!(hostRadius > 0f) || !(tenderRadius > 0f) || float.IsInfinity(hostRadius) || float.IsInfinity(tenderRadius))
                throw new System.ArgumentOutOfRangeException(nameof(tenderRadius));
            return hostRadius / tenderRadius;
        }
    }
}
#endif
