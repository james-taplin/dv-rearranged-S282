#if B99_ALL_STANDARD
using System;
using UnityEngine;

namespace RearrangedS282
{
    // One-shot presentation initialization. Native wheel rotation still owns speed;
    // this never seeks a running Animator or changes a simulation crank/force port.
    internal static class DriveQuartering
    {
        internal static readonly int NativeState = Animator.StringToHash("Base Layer.Blend Tree");
        internal static readonly int FranklinState = Animator.StringToHash("Base Layer.LocoS282A_AnimationDriveGear");
        internal static readonly int Franklin4444State = Animator.StringToHash("Base Layer.LocoS282A_AnimationDriveGear4444");

        internal static float Cycle(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(nameof(value), "Drive phase must be finite.");
            return value - (float)Math.Floor(value);
        }

        internal static float Capture(Animator source, float offset)
        {
            if (source == null || !source.isInitialized) return 0f;
            var state = source.GetCurrentAnimatorStateInfo(0);
            return state.fullPathHash == 0 ? 0f : Cycle(Cycle(state.normalizedTime) - offset);
        }

        internal static void Play(Animator target, int state, float phase, bool retainOnDisable)
        {
            if (target == null || target.runtimeAnimatorController == null || !target.isActiveAndEnabled)
                throw new InvalidOperationException("Quartering requires an active, configured drivetrain Animator.");
            // Do not evaluate with Animator.Update: let the normal player loop
            // sample the complete mechanism before followers/retargets LateUpdate.
            if (retainOnDisable) target.keepAnimatorControllerStateOnDisable = true;
            target.Play(state, 0, Cycle(phase));
        }

        internal static float ErrorDegrees(float left, float right, float leftOffset, float rightOffset)
        {
            float difference = Cycle(Cycle(right) - Cycle(left) - Cycle(rightOffset - leftOffset));
            return Mathf.Min(difference, 1f - difference) * 360f;
        }
    }
}
#endif
