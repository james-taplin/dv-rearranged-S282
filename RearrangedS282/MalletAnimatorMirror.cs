#if B99_ALL_STANDARD
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RearrangedS282
{
    // The rear engine's Animator has no native port reader; follow the front
    // engine's cutoff/reverser parameters on the same side.
    internal sealed class MalletAnimatorMirror : MonoBehaviour
    {
        internal Animator Source, Target;
        internal bool CopySpeed = true;
        private AnimatorControllerParameter[] parameters;
        private RuntimeAnimatorController sourceController, targetController;

        private void Update()
        {
            if (Source == null || Target == null || !Target.isActiveAndEnabled) return;
            // Copy only parameters both controllers declare (the duplex Franklin
            // clip has none of the native ones); refresh when either changes.
            if (parameters == null || sourceController != Source.runtimeAnimatorController || targetController != Target.runtimeAnimatorController)
            {
                sourceController = Source.runtimeAnimatorController; targetController = Target.runtimeAnimatorController;
                var targetHashes = new HashSet<int>(Target.parameters.Select(x => x.nameHash));
                parameters = Source.parameters.Where(x => targetHashes.Contains(x.nameHash)).ToArray();
            }
            foreach (var parameter in parameters)
            {
                if (parameter.name == "SpeedMultiplier" && (!CopySpeed || GroupCrankAnimation.Owns(Target))) continue;
                switch (parameter.type)
                {
                    case AnimatorControllerParameterType.Float: Target.SetFloat(parameter.nameHash, Source.GetFloat(parameter.nameHash)); break;
                    case AnimatorControllerParameterType.Int: Target.SetInteger(parameter.nameHash, Source.GetInteger(parameter.nameHash)); break;
                    case AnimatorControllerParameterType.Bool: Target.SetBool(parameter.nameHash, Source.GetBool(parameter.nameHash)); break;
                }
            }
        }
    }
}
#endif
