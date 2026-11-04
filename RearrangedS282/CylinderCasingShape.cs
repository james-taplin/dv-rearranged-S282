#if B99_ALL_STANDARD
using System;
using UnityEngine;

namespace RearrangedS282
{
    // Presentation only. Coordinates are the prepared native drivetrain frame,
    // before the existing engine transform is applied. Legacy barrel treatment
    // pins fittings; full treatment maps the fittings with the whole casting.
    internal static class CylinderCasingShape
    {
        internal const float NativePistonY = .712937f;
        internal const float RailClearance = .08f;
        internal const float BulkStrength = .85f;

        internal const float NativeValveY = 1.3470917f;
        internal const float NativeRearFaceZ = 8.84f;

        internal const float FranklinRearNoseShorteningM = .12f;

        // The 4-4-4-4 front crank can enter the following cylinder nose on
        // articulation. Shorten only the forward barrel transition: the rear
        // piston face and shaft mounts stay at their authored Z. The adaptive
        // steam inlet is reseated from the corrected mesh by SteamCylinder.
        // The cap and its bolts translate together rather than being scaled.
        internal static Vector3 CompactFranklinRearNose(Vector3 point)
        {
            Finite(point.x); Finite(point.y); Finite(point.z);
            return new Vector3(point.x, point.y,
                point.z - FranklinRearNoseShorteningM * Smooth(9.49f, 9.80f, point.z));
        }

        internal const float FranklinTriplexNoseShorteningM = .20f;

        // Only the middle 2-2-2-2-2 / 2-4-4-4-0 castings meet the front
        // crank at the measured native-constraint stresses. Their forward cap
        // needs 200 mm native shortening; the rear piston face and shaft mounts
        // remain behind 9.49. A forward-widened transition keeps dZ'/dZ >= 1/6;
        // the complete cap/bolts beyond 9.85 translate rigidly along the bore.
        internal static Vector3 CompactFranklinTriplexNose(Vector3 point)
        {
            Finite(point.x); Finite(point.y); Finite(point.z);
            return new Vector3(point.x, point.y,
                point.z - FranklinTriplexNoseShorteningM * Smooth(9.49f, 9.85f, point.z));
        }

        // First used for Prairie, now shared by Pacific/Hudson and rigid x-4-x.
        // The two bore axes stay where the native animation puts them.
        // Resize both barrels, their caps and the entire casting length; only
        // the web between the bores retains their enlarged centre separation.
        // This monotone, C1 map avoids pinning isolated surface patches (which
        // dented the earlier trial). Pipes/cocks must follow this same map.
        internal static Vector3 NormalizePrairie(Vector3 point, float scaleY, float scaleZ)
        {
            Positive(scaleY, nameof(scaleY)); Positive(scaleZ, nameof(scaleZ));
            Finite(point.x); Finite(point.y); Finite(point.z);
            float factor = 1f / scaleY;
            float y;
            if (point.y <= NativePistonY)
                y = NativePistonY + (point.y - NativePistonY) * factor;
            else if (point.y >= NativeValveY)
                y = NativeValveY + (point.y - NativeValveY) * factor;
            else
            {
                float distance = NativeValveY - NativePistonY;
                float t = (point.y - NativePistonY) / distance;
                y = NativePistonY + distance * (factor * t + (1f - factor) * t * t * (3f - 2f * t));
            }
            return new Vector3(point.x, y, NativeRearFaceZ + (point.z - NativeRearFaceZ) / scaleZ);
        }

        internal static float ComputeTargetY(float scaleY, float nativeMinY,
            float pistonY = NativePistonY, float originY = 0f)
        {
            Positive(scaleY, nameof(scaleY));
            Finite(nativeMinY); Finite(pistonY); Finite(originY);
            // The stationary socket/upper-chest interfaces limit how far a
            // compact barrel can grow while its moving gear stays unchanged.
            float target = Math.Min(1f, scaleY * 1.35f);
            float below = pistonY - nativeMinY;
            if (below > .001f)
                target = Math.Min(target, (originY + scaleY * pistonY - RailClearance) / below);
            // If even the existing shape has less clearance, do not enlarge it.
            return scaleY < 1f ? Math.Max(scaleY, target) : Math.Max(.01f, target);
        }

        internal static Vector3 TransformVertex(Vector3 point, float scaleY, float scaleZ,
            float targetY, float rearZ, float fullZ, float pistonY = NativePistonY,
            bool franklin = false)
        {
            Positive(scaleY, nameof(scaleY)); Positive(scaleZ, nameof(scaleZ));
            Positive(targetY, nameof(targetY));
            Finite(point.x); Finite(point.y); Finite(point.z);
            if (!(fullZ > rearZ)) throw new ArgumentOutOfRangeException(nameof(fullZ));
            float weight = Smooth(rearZ, fullZ, point.z);
            // Native drain pipe and cock brackets continue to mate with the
            // unchanged cock mesh and particle outlets, at every engine scale.
            float nearDrain = 1f - Smooth(.16f, .21f, Math.Abs(Math.Abs(point.x) - 1.09f));
            weight *= 1f - nearDrain * (1f - Smooth(.48f, .64f, point.y));
            // The upper valve chest must keep its existing axis and steam-pipe
            // contact. Correct the main barrel instead of denting the chest
            // around several immovable attachment patches. Franklin's separate
            // shaft/joint hardware and its upper mounting bridge stay fixed too.
            weight *= franklin ? 1f - Smooth(.85f, 1.12f, point.y)
                : 1f - Smooth(1.02f, 1.30f, point.y);
            if (weight <= 0f) return point;
            float yFactor = 1f + (targetY / scaleY - 1f) * weight * BulkStrength;
            // Retain longitudinal dimensions and all Z connections. Shrinking Z
            // around several fixed mounts can fold a short transition band; the
            // first trial restores barrel bulk without introducing that problem.
            return new Vector3(point.x, pistonY + (point.y - pistonY) * yFactor, point.z);
        }

        private static float Smooth(float from, float to, float value)
        {
            float t = Math.Max(0f, Math.Min(1f, (value - from) / (to - from)));
            return t * t * (3f - 2f * t);
        }
        private static void Positive(float value, string name)
        { if (!(value > 0f) || float.IsInfinity(value)) throw new ArgumentOutOfRangeException(name); }
        private static void Finite(float value)
        { if (float.IsNaN(value) || float.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(value)); }
    }
}
#endif
