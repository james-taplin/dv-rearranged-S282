#if B99_ALL_STANDARD
using System;
using System.Linq;
using DV.Wheels;
using UnityEngine;
#if B99_DIAGNOSTICS || B99_STRUCTURAL_DIAGNOSTICS
using Newtonsoft.Json.Linq;
#endif

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        private PoweredWheelRotationViaAnimation quarteringRotation;
        private float nativeLeftPhase, nativeRightPhase;

        private void CacheNativeQuartering()
        {
            quarteringRotation = loco.GetComponentsInChildren<PoweredWheelRotationViaAnimation>(true)
                .Single(r => r.animatorSetups != null && r.animatorSetups.Any(s => s.animator == lfAnimator));
            nativeLeftPhase = quarteringRotation.animatorSetups.Single(s => s.animator == lfAnimator).startTimeOffset;
            nativeRightPhase = quarteringRotation.animatorSetups.Single(s => s.animator == rfAnimator).startTimeOffset;
            float gap = DriveQuartering.Cycle(nativeRightPhase - nativeLeftPhase);
            if (Mathf.Abs(gap - .25f) > .001f && Mathf.Abs(gap - .75f) > .001f)
                throw new InvalidOperationException("Native S282 drivetrain offsets are not quartered.");
        }

        private float CaptureQuarteringCycle() => DriveQuartering.Capture(lfAnimator, nativeLeftPhase);

        private int QuarteringState(Animator animator)
        {
            var controller = animator.runtimeAnimatorController;
            if (controller == lfDefaultAnimatorCtrl || controller == rfDefaultAnimatorCtrl) return DriveQuartering.NativeState;
            if (controller == AssetBundleLoader.FranklinBAnimCtrl) return DriveQuartering.FranklinState;
            if (controller == AssetBundleLoader.FranklinBAnimCtrl4444) return DriveQuartering.Franklin4444State;
            throw new InvalidOperationException("Unknown S282 drivetrain controller for quartering.");
        }

        // Call only after all controller swaps, parenting and frame activation.
        // The existing native registration includes all active locomotive engines.
        private void ApplyDriverQuartering(float cycle, bool retainOnDisable)
        {
            if (quarteringRotation == null) return; // untouched/native-only stock
            foreach (var setup in quarteringRotation.animatorSetups)
                if (setup.animator != null && setup.animator.isActiveAndEnabled)
                    DriveQuartering.Play(setup.animator, QuarteringState(setup.animator),
                        cycle + setup.startTimeOffset, retainOnDisable);
        }

#if B99_DIAGNOSTICS || B99_STRUCTURAL_DIAGNOSTICS
        internal JArray FranklinLinkageReport()
        {
            var rows = new JArray();
            int id = (int)currentWA;
            if (id != 31 && id != 32 && id != 38 && id != 40 && id != 47)
                return rows;
            if (quarteringRotation?.animatorSetups == null) return rows;
            var groups = GetComponentsInChildren<LODGroup>(true);
            JObject Part(Transform source)
            {
                var presented = source;
                // Retargeting keeps the animated source but deliberately removes
                // its mesh. Validate its active, direct owned proxy in that case.
                if (source != null && source.GetComponent<MeshFilter>()?.sharedMesh == null)
                    presented = source.Cast<Transform>().FirstOrDefault(t =>
                        t.name == "[Rearranged retarget] " + source.name && t.gameObject.activeInHierarchy);
                var renderer = presented != null ? presented.GetComponent<MeshRenderer>() : null;
                return new JObject {
                    ["enabled"] = source != null && source.gameObject.activeInHierarchy &&
                        source.GetComponent<MeshRenderer>()?.enabled == true && presented != null &&
                        presented.gameObject.activeInHierarchy && renderer?.enabled == true,
                    ["mesh"] = presented != null && presented.GetComponent<MeshFilter>()?.sharedMesh != null,
                    ["detailLod"] = renderer != null && groups.Any(g =>
                        g.GetLODs().Length != 0 && g.GetLODs()[0].renderers.Contains(renderer))
                };
            }
            foreach (var animator in quarteringRotation.animatorSetups.Select(s => s.animator)
                .Where(a => a != null && a.isActiveAndEnabled &&
                    (a.runtimeAnimatorController == AssetBundleLoader.FranklinBAnimCtrl ||
                     a.runtimeAnimatorController == AssetBundleLoader.FranklinBAnimCtrl4444)).Distinct())
                rows.Add(new JObject {
                    ["drive"] = animator.name,
                    ["mainRod"] = Part(animator.transform.Find("s282_mech_wheels_connect/s282_mech_push_rod_to_connect")),
                    ["crank"] = Part(animator.transform.Find("s282_mech_wheels_connect/s282_mech_connect_to_cutoff_base"))
                });
            return rows;
        }

        internal JObject QuarteringReport()
        {
            var rows = new JArray();
            if (quarteringRotation != null)
            {
                var active = quarteringRotation.animatorSetups
                    .Where(s => s.animator != null && s.animator.isActiveAndEnabled).ToArray();
                for (int i = 0; i < active.Length; i += 2)
                    rows.Add(QuarteringPairReport("locomotive " + (i / 2 + 1), active[i].animator,
                        i + 1 < active.Length ? active[i + 1].animator : null,
                        active[i].startTimeOffset, i + 1 < active.Length ? active[i + 1].startTimeOffset : nativeRightPhase));
            }
            if (poweredTenderSelected)
                rows.Add(QuarteringPairReport("powered tender", tenderDriveL?.GetComponent<Animator>(),
                    tenderDriveR?.GetComponent<Animator>(), nativeLeftPhase, nativeRightPhase));
            int id = (int)currentWA;
            int engines = id == (int)WheelArrangementType.s404 ? 0 :
                WheelArrangement.IsArticulatedId(id) ? WheelArrangement.LocomotiveWheels(id).Length - 2 : 1;
            int expected = engines + (poweredTenderSelected ? 1 : 0);
            return new JObject { ["expectedPairs"] = expected, ["pairs"] = rows,
                ["passed"] = rows.Count == expected && rows.All(r => (bool)r["passed"]) };
        }

        private static JObject QuarteringPairReport(string name, Animator left, Animator right, float leftOffset, float rightOffset)
        {
            var row = new JObject { ["engine"] = name, ["passed"] = false };
            if (left == null || right == null || !left.isInitialized || !right.isInitialized ||
                !left.isActiveAndEnabled || !right.isActiveAndEnabled) { row["error"] = "Missing or uninitialized drivetrain pair."; return row; }
            var ls = left.GetCurrentAnimatorStateInfo(0); var rs = right.GetCurrentAnimatorStateInfo(0);
            float error = DriveQuartering.ErrorDegrees(ls.normalizedTime, rs.normalizedTime, leftOffset, rightOffset);
            row["leftCycle"] = DriveQuartering.Cycle(ls.normalizedTime);
            row["rightCycle"] = DriveQuartering.Cycle(rs.normalizedTime);
            row["phaseErrorDegrees"] = error;
            row["leftState"] = ls.fullPathHash; row["rightState"] = rs.fullPathHash;
            var wheels = new JArray();
            for (int axle = 1; axle <= 7; axle++)
            {
                var l = left.transform.Find("s282_wheels_driving_" + axle);
                var r = right.transform.Find("s282_wheels_driving_" + axle);
                bool la = l != null && l.gameObject.activeInHierarchy;
                bool ra = r != null && r.gameObject.activeInHierarchy;
                if (!la && !ra) continue;
                // Both authored drives rotate about the same LOCAL X axis. The
                // left root mirrors X only, so their Y/Z crank planes agree.
                // World rotations would include different articulation yaw.
                float gap = la && ra ? Quaternion.Angle(l.localRotation, r.localRotation) : 0f;
                wheels.Add(new JObject { ["axle"] = axle, ["bothActive"] = la && ra,
                    ["separationDegrees"] = gap, ["passed"] = la && ra && Mathf.Abs(gap - 90f) <= 1f });
            }
            row["wheels"] = wheels;
            row["passed"] = ls.fullPathHash != 0 && ls.fullPathHash == rs.fullPathHash && error <= 1f &&
                wheels.All(w => (bool)w["passed"]);
            return row;
        }
#endif
    }
}
#endif
