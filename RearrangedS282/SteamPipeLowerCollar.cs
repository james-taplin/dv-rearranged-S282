#if B99_ALL_STANDARD
using System;
using System.Linq;
using UnityEngine;

namespace RearrangedS282
{
    internal static class SteamPipeLowerCollar
    {
        // The two authored lower flanges are 288 mm across on a 134.6 mm
        // inclined shaft. Only their outer annulus meets the relocated air
        // outlet. Retain that shaft, both flange faces/rims and their original
        // UV topology; reduce the annulus in a private presentation copy.
        internal static Vector3[] CompactLowerCollar(Vector3[] nativeVertices, int[] triangles,
            float maximumRadius = .108f)
        {
            if (nativeVertices == null || triangles == null || nativeVertices.Length < 3 ||
                !(maximumRadius > .068f) || float.IsNaN(maximumRadius) || float.IsInfinity(maximumRadius))
                throw new ArgumentException("Native lower collar needs a readable mesh and a radius outside its retained shaft.");
            if (!SteamPipeNativeFit.TrySection(nativeVertices, triangles, null, 1.72f, out var upper) ||
                !SteamPipeNativeFit.TrySection(nativeVertices, triangles, null, 1.68f, out var lower))
                throw new InvalidOperationException("The native steam shaft axis could not be measured below its lower collar.");
            var direction = lower.Centre - upper.Centre;
            float length = (float)Math.Sqrt(direction.sqrMagnitude);
            if (!(length > .03f && length < .06f))
                throw new InvalidOperationException("The measured native steam shaft has an invalid axis.");
            direction = direction * (1f / length);
            var result = nativeVertices.ToArray();
            const float shaft = .068f, outer = .1444f;
            if (maximumRadius >= outer) return result;
            for (int i = 0; i < result.Length; i++)
            {
                var delta = nativeVertices[i] - upper.Centre;
                float along = delta.x * direction.x + delta.y * direction.y + delta.z * direction.z;
                // Measured flange planes occupy -.1944..-.1156 m along this
                // native axis. The upper cast flare is around -.348 m and is
                // deliberately outside this short band.
                float weight = Smooth(-.22f, -.20f, along) * (1f - Smooth(-.11f, -.09f, along));
                if (weight <= 0f) continue;
                var radial = delta - direction * along;
                float radius = (float)Math.Sqrt(radial.sqrMagnitude);
                if (radius <= shaft) continue;
                float target = shaft + (radius - shaft) * ((maximumRadius - shaft) / (outer - shaft));
                float mapped = radius + (target - radius) * weight;
                result[i] = upper.Centre + direction * along + radial * (mapped / radius);
            }
            return result;
        }

        private static float Smooth(float from, float to, float value)
        {
            float t = Math.Max(0f, Math.Min(1f, (value - from) / (to - from)));
            return t * t * (3f - 2f * t);
        }
    }
}
#endif
