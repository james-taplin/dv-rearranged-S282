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
        internal void Initialize(ArticulatedSimulation sim, Animator l, Animator r, Animator sourceL, Animator sourceR)
        {
            simulation = sim; left = l; right = r;
            foreach (var pair in new[] { (target: l, source: sourceL), (target: r, source: sourceR) })
            {
                // Native left/right phase offset retained at attachment.
                var state = pair.source.GetCurrentAnimatorStateInfo(0);
                pair.target.Play(0, 0, state.normalizedTime % 1f);
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
            float frequency = simulation != null && simulation.IsTenderEngine && simulation.IsMiddleActive
                ? simulation.MiddleEngine.chuffFrequencyReadOut.Value / (simulation.MiddleEngine.numCylinders * 2f) : 0f;
            float direction = simulation != null ? Mathf.Sign(simulation.MainEngine.crankRpm.Value) : 1f;
            if (left != null) left.SetFloat(Speed, frequency * direction);
            if (right != null) right.SetFloat(Speed, frequency * direction);
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
