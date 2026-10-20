#if B99_ALL_STANDARD || OIL_PLAN_TEST
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace RearrangedS282
{
    // Pure planning for big-end oil cups: which native oiling point each driven
    // wheel's big end uses, and how saved oil levels map onto the extended
    // point list. The S282's six native points stay at indices 0-5; the extra
    // big ends are 6-12 (left, front to back) and 13-19 (right).
    public static class BigEndOilingPlan
    {
        public const int NativePoints = 6, ExtraPerSide = 7, TotalPoints = NativePoints + 2 * ExtraPerSide;

        public struct BigEnd
        {
            public bool Left; public float Z; public bool NativeCup;
            public BigEnd(bool left, float z, bool nativeCup) { Left = left; Z = z; NativeCup = nativeCup; }
        }

        public static int PointIndex(bool left, int slot)
        {
            if (slot < 0 || slot >= ExtraPerSide) throw new ArgumentOutOfRangeException(nameof(slot));
            return NativePoints + (left ? 0 : ExtraPerSide) + slot;
        }

        // Point index per big end (same order as given), -1 for big ends that
        // already have a native cup or do not fit. Slots fill front to back per side.
        public static int[] Allocate(IList<BigEnd> bigEnds, out int dropped)
        {
            var result = new int[bigEnds.Count];
            dropped = 0;
            for (int i = 0; i < result.Length; i++) result[i] = -1;
            foreach (bool left in new[] { true, false })
            {
                int slot = 0;
                foreach (int i in Enumerable.Range(0, bigEnds.Count).Where(i => bigEnds[i].Left == left && !bigEnds[i].NativeCup)
                    .OrderByDescending(i => bigEnds[i].Z).ThenBy(i => i))
                {
                    if (slot < ExtraPerSide) result[i] = PointIndex(left, slot++);
                    else dropped++;
                }
            }
            return result;
        }

        public static bool[] ActivePoints(int[] allocation)
        {
            var active = new bool[TotalPoints];
            for (int i = 0; i < NativePoints; i++) active[i] = true;
            foreach (int index in allocation) if (index >= 0) active[index] = true;
            return active;
        }

        // Saves from before the extension (6 points) or from other point counts:
        // keep what lines up, start any new point full and closed.
        public static JArray FitSavedPoints(JArray saved, int count)
        {
            var result = new JArray();
            for (int i = 0; i < count; i++)
                result.Add(saved != null && i < saved.Count && saved[i] is JObject point
                    ? point.DeepClone() : new JObject { ["open"] = false, ["oilLevel"] = 1f });
            return result;
        }
    }
}
#endif
