#if B99_ALL_STANDARD
using UnityEngine;

namespace RearrangedS282
{
    internal static class ChuffStageTuning
    {
        // Only the front LP position of the Mallets and Erie. Simple duplexes
        // and fictional triplexes retain the authored native sound profile.
        internal static bool HasLowPressureFront(int id) => id == 37 || id == 41 || id == 42 || id == 43 || id == 45 || id == 46;
        private const float PressureSpan = .85f;

        // Keep atmospheric zero and sub-atmospheric authored key positions.
        internal static float Pressure(float pressure) => pressure <= 1f ? pressure : 1f + (pressure - 1f) * PressureSpan;

        internal static AnimationCurve Volume(AnimationCurve authored)
        {
            if (authored == null) return null;
            var keys = authored.keys;
            for (int i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                key.time = Pressure(key.time);
                // Account for the different scale on either side of the join;
                // preserve values, weighting and infinity/step tangents.
                if (key.time > 1f) key.inTangent /= PressureSpan;
                if (key.time >= 1f) key.outTangent /= PressureSpan;
                keys[i] = key;
            }
            return new AnimationCurve(keys) { preWrapMode = authored.preWrapMode, postWrapMode = authored.postWrapMode };
        }
    }
}
#endif
