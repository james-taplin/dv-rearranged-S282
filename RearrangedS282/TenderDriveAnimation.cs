#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.Linq;
using DV.Simulation.Ports;
using UnityEngine;

namespace RearrangedS282
{
    // Reuses the native Walschaerts controller; no wheel/physics transform edits.
    // Runs after the host mirror's Update, replacing only the playback speed.
    internal sealed class TenderDriveAnimation : MonoBehaviour
    {
        private ArticulatedSimulation simulation;
        private Animator left, right;
        private readonly List<AnimatorPortReader> cutoffReaders = new List<AnimatorPortReader>();
        private static readonly int Speed = Animator.StringToHash("SpeedMultiplier");
        internal bool Drives(Animator target) => (left == target || right == target) && cutoffReaders.Count == 2 &&
            simulation != null && simulation.IsTenderEngine && simulation.IsMiddleActive;
        internal void Initialize(ArticulatedSimulation sim, Animator l, Animator r, Animator sourceL, Animator sourceR,
            float leftOffset, float rightOffset)
        {
            simulation = sim; left = l; right = r;
            float cycle = DriveQuartering.Capture(sourceL, leftOffset);
            foreach (var pair in new[] { (target: l, source: sourceL), (target: r, source: sourceR) })
            {
                // The tender is its own engine. Establish quartering from the
                // native offsets even if a host Animator has a queued pose.
                DriveQuartering.Play(pair.target, DriveQuartering.NativeState,
                    cycle + (pair.target == l ? leftOffset : rightOffset), true);
                // The native host reader exists even while a Franklin controller
                // is selected (and that controller has no Cutoff parameter).
                // Reuse its authored port/modifiers with the tender's native gear.
                var native = pair.source.GetComponents<AnimatorPortReader>().FirstOrDefault(r => r.parameterName == "Cutoff");
                if (native == null || !sim.Flow.TryGetPort(native.portId, out var port))
                    throw new InvalidOperationException("Native S282 reverser animation reader missing.");
                var reader = pair.target.gameObject.AddComponent<AnimatorPortReader>();
                reader.updateType = native.updateType; reader.portId = native.portId; reader.parameterName = native.parameterName;
                reader.valueMultiplier = native.valueMultiplier; reader.valueOffset = native.valueOffset;
                cutoffReaders.Add(reader); reader.Init(port);
            }
            LateUpdate();
        }
        private void LateUpdate()
        {
            var engine = simulation != null && simulation.IsTenderEngine && simulation.IsMiddleActive
                ? simulation.MiddleEngine : null;
            float frequency = engine != null ? engine.chuffFrequencyReadOut.Value / (engine.numCylinders * 2f) : 0f;
            // The tender keeps its own native RPM feed when the locomotive's
            // front engine acquires an independent-slip input. Its diameter
            // ratio is positive, so the unscaled native input retains the exact
            // tender direction on both authority and native-port replicas.
            float direction = engine != null ? Mathf.Sign(engine.crankRpm.Value) : 1f;
            if (left != null && !GroupCrankAnimation.Owns(left)) left.SetFloat(Speed, frequency * direction);
            if (right != null && !GroupCrankAnimation.Owns(right)) right.SetFloat(Speed, frequency * direction);
        }
        private void OnDisable()
        {
            foreach (var reader in cutoffReaders) if (reader != null) reader.Deinit();
            cutoffReaders.Clear();
            if (left != null) left.SetFloat(Speed, 0f);
            if (right != null) right.SetFloat(Speed, 0f);
        }
    }
}
#endif
