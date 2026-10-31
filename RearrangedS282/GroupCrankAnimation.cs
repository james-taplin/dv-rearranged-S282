#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DV.Simulation.Ports;
using DV.Wheels;
using LocoSim.Implementations;
using UnityEngine;

namespace RearrangedS282
{
    // One simulation phase source per mechanism. Keeps the existing clips,
    // native port reader and normal Animator evaluation; no second crank clock.
    internal sealed class GroupCrankAnimation : IDisposable
    {
        internal sealed class PhaseReader : AnimatorPortReader
        {
            private void OnDestroy() { Deinit(); }
        }
        private sealed class DriverLease
        {
            internal PoweredWheelRotationViaAnimation Driver;
            internal PoweredWheelRotationViaAnimation.AnimatorStartTimeOffsetPair[] Original;
            internal readonly HashSet<Animator> Removed = new HashSet<Animator>();
            internal void Refresh()
            {
                if (Driver != null) Driver.animatorSetups = Original.Where(p => !Removed.Contains(p.animator)).ToArray();
            }
        }
        private sealed class ReaderState
        {
            internal AnimatorPortReader Reader;
            internal Port OriginalPort;
            internal bool Enabled;
        }
        private sealed class TargetState
        {
            internal Animator Target;
            internal AnimatorPortReader PhaseReader;
            internal bool HasSpeed;
            internal float OriginalSpeed;
            internal float OriginalAnimatorSpeed;
            internal readonly List<ReaderState> Readers = new List<ReaderState>();
            internal readonly List<DriverLease> Drivers = new List<DriverLease>();
        }

        private static readonly int Speed = Animator.StringToHash("SpeedMultiplier");
        private static readonly FieldInfo ReaderPort = typeof(AnimatorPortReader).GetField("port", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly Dictionary<Animator, GroupCrankAnimation> Owners = new Dictionary<Animator, GroupCrankAnimation>();
        private static readonly Dictionary<PoweredWheelRotationViaAnimation, DriverLease> Drivers = new Dictionary<PoweredWheelRotationViaAnimation, DriverLease>();
        private readonly List<TargetState> targets = new List<TargetState>();
        private bool disposed;

        internal static bool Owns(Animator animator) => animator != null && Owners.ContainsKey(animator);

        // Diagnostic inventory must include mechanisms leased away from the
        // native speed writer. Never restore those writers just to inspect them.
        internal static PoweredWheelRotationViaAnimation.AnimatorStartTimeOffsetPair[] DiagnosticSetups(PoweredWheelRotationViaAnimation driver) =>
            driver == null ? Array.Empty<PoweredWheelRotationViaAnimation.AnimatorStartTimeOffsetPair>() :
            Drivers.TryGetValue(driver, out var lease) ? lease.Original :
            driver.animatorSetups ?? Array.Empty<PoweredWheelRotationViaAnimation.AnimatorStartTimeOffsetPair>();

        // Supply explicit phases immediately after DriveQuartering.Play, whose
        // queued pose need not yet be visible through GetCurrentAnimatorStateInfo.
        // Otherwise this must be called after the initialized pose is evaluated.
        internal static GroupCrankAnimation Bind(ReciprocatingSteamEngine engine,
            Animator[] animators, float[] normalizedPhases = null,
            PoweredWheelRotationViaAnimation[] globalDrivers = null, float[] explicitOffsets = null)
        {
            if (engine?.crankRotationReadOut == null || animators == null || animators.Length == 0 ||
                animators.Any(a => a == null || !a.isActiveAndEnabled || a.runtimeAnimatorController == null) ||
                animators.Distinct().Count() != animators.Length ||
                normalizedPhases != null && normalizedPhases.Length != animators.Length ||
                explicitOffsets != null && (explicitOffsets.Length != animators.Length || explicitOffsets.Any(v => float.IsNaN(v) || float.IsInfinity(v))))
                throw new ArgumentException("Independent mechanism binding requires a native engine and distinct active animators.");
            if (ReaderPort == null) throw new MissingFieldException(typeof(AnimatorPortReader).FullName, "port");
            var binding = new GroupCrankAnimation();
            try
            {
                for (int i = 0; i < animators.Length; i++)
                {
                    var animator = animators[i];
                    if (Owns(animator)) throw new InvalidOperationException("Mechanism already has an independent phase owner.");
                    if (explicitOffsets == null && normalizedPhases == null && (!animator.isInitialized || animator.GetCurrentAnimatorStateInfo(0).fullPathHash == 0))
                        throw new InvalidOperationException("Queued mechanism initialization requires explicit quartering phases.");
                    float offset = explicitOffsets != null ? explicitOffsets[i] : DriveQuartering.Cycle(
                        (normalizedPhases != null ? normalizedPhases[i] : animator.GetCurrentAnimatorStateInfo(0).normalizedTime)
                        - engine.crankRotationReadOut.Value);
                    var state = new TargetState { Target = animator };
                    binding.targets.Add(state);
                    Owners.Add(animator, binding);
                    state.HasSpeed = animator.parameters.Any(p => p.nameHash == Speed && p.type == AnimatorControllerParameterType.Float);
                    state.OriginalAnimatorSpeed = animator.speed;
                    animator.speed = 0f;
                    if (state.HasSpeed) { state.OriginalSpeed = animator.GetFloat(Speed); animator.SetFloat(Speed, 0f); }

                    foreach (var reader in animator.GetComponents<AnimatorPortReader>())
                        if (!(reader is PhaseReader) && (reader.updateType == AnimatorPortReader.UpdateType.SET_NORMALIZED_TIME || reader.parameterName == "SpeedMultiplier"))
                        {
                            state.Readers.Add(new ReaderState { Reader = reader, OriginalPort = (Port)ReaderPort.GetValue(reader), Enabled = reader.enabled });
                            // Disabling the behaviour alone leaves its native
                            // event subscription live, so explicitly deinitialize.
                            reader.Deinit(); reader.enabled = false;
                        }
                    var car = animator.GetComponentInParent<TrainCar>();
                    if (globalDrivers == null && car == null) throw new InvalidOperationException("Mechanism is not beneath a native car.");
                    foreach (var driver in globalDrivers ?? car.GetComponentsInChildren<PoweredWheelRotationViaAnimation>(true))
                    {
                        if (!Drivers.TryGetValue(driver, out var lease))
                        {
                            if (driver.animatorSetups == null || !driver.animatorSetups.Any(p => p.animator == animator)) continue;
                            lease = new DriverLease { Driver = driver, Original = driver.animatorSetups };
                            Drivers.Add(driver, lease);
                        }
                        if (!lease.Original.Any(p => p.animator == animator)) continue;
                        lease.Removed.Add(animator); state.Drivers.Add(lease); lease.Refresh();
                    }
                    state.PhaseReader = animator.gameObject.AddComponent<PhaseReader>();
                    state.PhaseReader.updateType = AnimatorPortReader.UpdateType.SET_NORMALIZED_TIME;
                    state.PhaseReader.portId = engine.crankRotationReadOut.id;
                    state.PhaseReader.parameterName = string.Empty;
                    state.PhaseReader.valueMultiplier = 1f;
                    state.PhaseReader.valueOffset = offset;
                    state.PhaseReader.Init(engine.crankRotationReadOut);
                }
                return binding;
            }
            catch { binding.Dispose(); throw; }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            foreach (var state in targets)
            {
                if (state.PhaseReader != null) { state.PhaseReader.Deinit(); UnityEngine.Object.Destroy(state.PhaseReader); }
                if (state.Target != null)
                {
                    state.Target.speed = state.OriginalAnimatorSpeed;
                    if (state.HasSpeed) state.Target.SetFloat(Speed, state.OriginalSpeed);
                }
                foreach (var reader in state.Readers)
                    if (reader.Reader != null)
                    {
                        if (reader.OriginalPort != null) reader.Reader.Init(reader.OriginalPort);
                        reader.Reader.enabled = reader.Enabled;
                    }
                foreach (var lease in state.Drivers)
                {
                    lease.Removed.Remove(state.Target);
                    if (lease.Removed.Count == 0)
                    {
                        if (lease.Driver != null) lease.Driver.animatorSetups = lease.Original;
                        Drivers.Remove(lease.Driver);
                    }
                    else lease.Refresh();
                }
                if (state.Target != null)
                {
                    if (Owners.TryGetValue(state.Target, out var owner) && ReferenceEquals(owner, this)) Owners.Remove(state.Target);
                }
                // A destroyed Unity Animator still has a managed identity.
                else if (!ReferenceEquals(state.Target, null)) Owners.Remove(state.Target);
            }
            targets.Clear();
        }
    }
}
#endif
