using System;
using UnityEngine;

namespace RearrangedS282
{
    // Presentation geometry only. Keep the bearing bosses rigid and change
    // only the straight spans between them; native transforms keep animating.
    internal static class RodRemesher
    {
        // The native S282 boss extends up to 0.123787 m along the rod from its
        // nominal pin centre, including the joint shoulder. This protects that
        // geometry on both native LODs and the original generated rod recipes.
        internal const float DefaultBossHalfWidth = .14f;
        internal const float MinimumShaftLength = .001f;

        internal static Vector3[] Remesh(Vector3[] source, float[] oldPins, float[] newPins,
            float bossHalfWidth = DefaultBossHalfWidth)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var result = new Vector3[source.Length];
            RemeshInto(source, oldPins, newPins, result, bossHalfWidth);
            return result;
        }

        // Caller-owned buffers let native-clock rod presentation change shaft
        // length each frame without allocating or scaling its bearing bosses.
        internal static void RemeshInto(Vector3[] source, float[] oldPins, float[] newPins,
            Vector3[] destination, float bossHalfWidth = DefaultBossHalfWidth)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (destination.Length != source.Length || ReferenceEquals(source, destination))
                throw new ArgumentException("Rod output must be a separate buffer matching the source length.");
            if (oldPins == null) throw new ArgumentNullException(nameof(oldPins));
            if (newPins == null) throw new ArgumentNullException(nameof(newPins));
            if (oldPins.Length < 2 || oldPins.Length != newPins.Length)
                throw new ArgumentException("Rod layouts require matching lists of at least two pins.");
            if (!Finite(bossHalfWidth) || bossHalfWidth <= 0f)
                throw new ArgumentOutOfRangeException(nameof(bossHalfWidth));

            // Traverse either order without copying or mutating the descriptors.
            int count = oldPins.Length;
            bool descending = oldPins[1] < oldPins[0];
            int Index(int i) => descending ? count - 1 - i : i;
            double Before(int i) => oldPins[Index(i)];
            double After(int i) => newPins[Index(i)];
            for (int i = 0; i < count; i++)
                if (!Finite(oldPins[i]) || !Finite(newPins[i]))
                    throw new ArgumentException("Rod pin coordinates must be finite.");
            double protectedSpan = 2d * bossHalfWidth;
            for (int i = 1; i < count; i++)
                if (Before(i) - Before(i - 1) - protectedSpan < MinimumShaftLength ||
                    After(i) - After(i - 1) - protectedSpan < MinimumShaftLength)
                    throw new ArgumentException("Rod pins cross, overlap a protected boss, or leave too little shaft.");

            for (int i = 0; i < source.Length; i++)
            {
                Vector3 vertex = source[i];
                if (!Finite(vertex.x) || !Finite(vertex.y) || !Finite(vertex.z))
                    throw new ArgumentException("Rod vertices must be finite.", nameof(source));
                double z = vertex.z;
                double mapped;
                if (z <= Before(0) + bossHalfWidth)
                    mapped = z + (After(0) - Before(0));
                else if (z >= Before(count - 1) - bossHalfWidth)
                    mapped = z + (After(count - 1) - Before(count - 1));
                else
                {
                    int next = 1;
                    while (z > Before(next) + bossHalfWidth) next++;
                    if (z >= Before(next) - bossHalfWidth)
                        mapped = z + (After(next) - Before(next));
                    else
                    {
                        double start = Before(next - 1) + bossHalfWidth;
                        double end = Before(next) - bossHalfWidth;
                        double fraction = (z - start) / (end - start);
                        // Interpolate between the boss edges, not the centres:
                        // the latter would jump at each protected boss boundary.
                        mapped = After(next - 1) + bossHalfWidth + fraction *
                            (After(next) - After(next - 1) - protectedSpan);
                    }
                }
                float newZ = (float)mapped;
                if (!Finite(newZ))
                    throw new ArgumentException("Rod layout produces an unrepresentable vertex coordinate.");
                destination[i] = new Vector3(vertex.x, vertex.y, newZ);
            }
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
