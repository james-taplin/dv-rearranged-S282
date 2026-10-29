#if B99_ALL_STANDARD
using UnityEngine;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        private bool NativeLubricatorAnimationBound()
        {
            if (currentWA == WheelArrangementType.s404 || currentWA == WheelArrangementType.s222) return true;
            if (rfAnimator == null || rfAnimator.runtimeAnimatorController != rfDefaultAnimatorCtrl) return true;
            var source = driveR?.Find("LubricatorParts")?.GetComponentInChildren<LubricatorRatchetDriver>(true);
            var ratchet = lubricatorControlPivot != null ? lubricatorControlPivot.GetComponent<LubricatorRatchet>() : null;
            return source != null && source.gameObject.activeInHierarchy && ratchet != null &&
                ratchet.isActiveAndEnabled && ratchet.ratchetDriverTransform == source.transform;
        }

        private void RestoreNativeLubricatorAnimation()
        {
            // The native ratchet reads this animated transform's local angle.
            // Keep it alive even when the resized linkage cannot be displayed.
            // Franklin controllers do not contain this native animation track.
            if (driveR == null || rfAnimator == null || rfAnimator.runtimeAnimatorController != rfDefaultAnimatorCtrl) return;
            var group = driveR.Find("LubricatorParts");
            var source = group != null ? group.GetComponentInChildren<LubricatorRatchetDriver>(true) : null;
            if (source == null) return;
            if (!group.gameObject.activeSelf)
            {
                foreach (var renderer in group.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
                group.gameObject.SetActive(true);
            }
            source.gameObject.SetActive(true);
            if (lubricatorControlPivot == null) return;
            var ratchet = lubricatorControlPivot.GetComponent<LubricatorRatchet>();
            if (ratchet == null)
            {
                // Native Start destroys this component when its driver was hidden
                // at external-interactable load time. Restore the native component.
                var prefab = loco.carLivery.externalInteractablesPrefab;
                var template = prefab != null ? prefab.transform.Find("Interactables/lubricator pivot")?.GetComponent<LubricatorRatchet>() : null;
                if (template == null) return;
                ratchet = lubricatorControlPivot.gameObject.AddComponent<LubricatorRatchet>();
                ratchet.localAxis = template.localAxis;
            }
            ratchet.ratchetDriverTransform = source.transform;
            ratchet.prevAngle = Mathf.Repeat(source.transform.localEulerAngles.x + 90f, 360f);
        }
    }
}
#endif
