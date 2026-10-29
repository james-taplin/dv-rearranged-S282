#if B99_ALL_STANDARD
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Object = UnityEngine.Object;
#if B99_DIAGNOSTICS || B99_STRUCTURAL_DIAGNOSTICS
using Newtonsoft.Json.Linq;
#endif

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        private readonly List<(Transform wheel, Transform anchor, PositionConstraint constraint)> malletDriverHolds
            = new List<(Transform, Transform, PositionConstraint)>();

        private void HoldMalletDriverPitch(Transform drive)
        {
            var first = RequireArticulatedPart(drive, "s282_wheels_driving_1");
            var second = RequireArticulatedPart(drive, "s282_wheels_driving_2");
            var third = RequireArticulatedPart(drive, "s282_wheels_driving_3");
            var anchor = new GameObject("[Rearranged Mallet first driver position]").transform;
            anchor.SetParent(drive, false);
            anchor.localPosition = new Vector3(first.localPosition.x, first.localPosition.y,
                2f * second.localPosition.z - third.localPosition.z);
            var constraint = first.gameObject.AddComponent<PositionConstraint>();
            malletDriverHolds.Add((first, anchor, constraint));
            // Native clips key position as well as rotation. A native constraint
            // runs after their pose evaluation and holds only the changed axis.
            constraint.translationAtRest = first.localPosition;
            constraint.translationOffset = Vector3.zero;
            constraint.translationAxis = Axis.Z;
            constraint.AddSource(new ConstraintSource { sourceTransform = anchor, weight = 1f });
            constraint.weight = 1f; constraint.locked = true; constraint.constraintActive = true;
            first.localPosition = anchor.localPosition;
        }

        private void ReleaseMalletDriverPitch()
        {
            foreach (var hold in malletDriverHolds)
            {
                if (hold.constraint != null)
                { hold.constraint.constraintActive = false; Object.DestroyImmediate(hold.constraint); }
                if (hold.anchor != null) Object.DestroyImmediate(hold.anchor.gameObject);
            }
            malletDriverHolds.Clear();
        }

#if B99_DIAGNOSTICS || B99_STRUCTURAL_DIAGNOSTICS
        internal JArray MalletDriverPitchReport()
        {
            var result = new JArray();
            foreach (var hold in malletDriverHolds)
            {
                bool bound = hold.wheel != null && hold.anchor != null && hold.constraint != null &&
                    hold.constraint.constraintActive && hold.constraint.enabled && hold.constraint.weight == 1f &&
                    hold.constraint.sourceCount == 1 && hold.constraint.GetSource(0).sourceTransform == hold.anchor;
                float gap = bound ? hold.wheel.parent.TransformVector(Vector3.forward *
                    (hold.wheel.localPosition.z - hold.anchor.localPosition.z)).magnitude : float.PositiveInfinity;
                result.Add(new JObject { ["drive"] = hold.wheel?.parent?.name, ["bound"] = bound,
                    ["gapM"] = gap, ["passed"] = bound && gap < .002f });
            }
            return result;
        }
#endif
    }
}
#endif
