#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DV.ModularAudioCar;
using DV.Simulation.Controllers;
using DV.Simulation.Ports;
using HarmonyLib;
using LocoSim.Implementations;
using UnityEngine;

namespace RearrangedS282
{
    // Native readers own timing, mixing, cylinder masks and particle updates.
    // This bridge owns only their extra rear-engine instances and leaf placement.
    internal static class ArticulatedEffects
    {
        private const float EngineSeparation = 4.65f;
        private static readonly ConditionalWeakTable<AudioClipSimReadersController, ChuffBinding> Chuffs = new();
        private static readonly ConditionalWeakTable<LayeredAudioSimReadersController, CockBinding> Cocks = new();
        private static readonly System.Reflection.FieldInfo ParticleFlow = AccessTools.Field(typeof(ParticlesPortReadersController), "simFlow");
        private static readonly System.Reflection.MethodInfo SmokeChuff = AccessTools.Method(typeof(SteamSmokeParticlePortReader), "OnChuffEvent");

        internal static void Register(Harmony harmony)
        {
            harmony.CreateClassProcessor(typeof(ClipInit)).Patch();
            harmony.CreateClassProcessor(typeof(ClipDeinit)).Patch();
            harmony.CreateClassProcessor(typeof(LayerInit)).Patch();
            harmony.CreateClassProcessor(typeof(LayerDeinit)).Patch();
            harmony.CreateClassProcessor(typeof(ParticleInit)).Patch();
            harmony.CreateClassProcessor(typeof(ParticleDestroy)).Patch();
        }

        internal static void Refresh(TrainCar car, bool active)
        {
            if (car == null) return;
            var state = car.GetComponent<EffectsState>();
            if (!active) { if (state != null) state.Stop(); return; }
            state ??= car.gameObject.AddComponent<EffectsState>();
            state.Car = car;
            state.Active = true;
            state.RefreshNow();
        }

        internal static void Deinit(TrainCar car) { if (car != null) car.GetComponent<EffectsState>()?.Stop(); }

        internal static void TenderChanged(TrainCar car)
        {
            var state = ActiveState(car);
            if (state == null) return;
            state.ResetAudio();
            state.ResetParticles();
            state.RefreshNow();
        }
        internal static bool TenderParticlesReady(TrainCar car) => car != null && car.GetComponent<EffectsState>()?.TenderReady == true;
        internal static bool TenderAudioReady(TrainCar car) => car != null && car.GetComponent<EffectsState>()?.TenderAudioReady == true;

        internal static bool IsOwnedPresentation(Transform t)
        {
            for (; t != null; t = t.parent) if (t.GetComponent<ArticulatedEffectMarker>() != null) return true;
            return false;
        }

        private static EffectsState ActiveState(TrainCar car)
        {
            var state = car != null ? car.GetComponent<EffectsState>() : null;
            return state != null && state.Active ? state : null;
        }

        private static void Safe(Action action)
        {
            try { action(); }
            catch (Exception ex) { try { Main.Logger.Warning("Articulated effects cleanup: " + ex.Message); } catch { } }
        }

        private static void RequirePorts(SimulationFlow flow, params string[] ids)
        {
            foreach (var id in ids)
                if (!flow.TryGetPort(id, out _)) throw new InvalidOperationException("Articulated effects port missing: " + id);
        }

        private static IEnumerable<T> AudioControllers<T>(TrainCar car) where T : Component
        {
            // Native TrainCar.SetupInterior makes this a separate scene root.
            // Include it explicitly, retaining car descendants for other layouts.
            var seen = new HashSet<T>();
            foreach (var controller in car.GetComponentsInChildren<T>(true))
                if (controller != null && seen.Add(controller)) yield return controller;
            if (car.interior != null)
                foreach (var controller in car.interior.GetComponentsInChildren<T>(true))
                    if (controller != null && seen.Add(controller)) yield return controller;
        }

        private static bool AudioBindingMatches(AudioBinding binding, TrainCar car, SimulationFlow flow)
        {
            return binding.Ready && ReferenceEquals(binding.Car, car) && ReferenceEquals(binding.Flow, flow);
        }

        private static GameObject Copy(GameObject source, Transform parent, string name)
        {
            var copy = UnityEngine.Object.Instantiate(source, parent);
            copy.name = "[Rearranged articulated effects] " + name;
            copy.AddComponent<ArticulatedEffectMarker>();
            foreach (var audio in copy.GetComponentsInChildren<AudioSource>(true)) audio.Stop();
            return copy;
        }

        private static void DestroyCopy(GameObject copy)
        {
            if (copy == null) return;
            foreach (var audio in copy.GetComponentsInChildren<AudioSource>(true)) Safe(audio.Stop);
            foreach (var particle in copy.GetComponentsInChildren<ParticleSystem>(true))
                Safe(() => particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear));
            copy.SetActive(false);
            copy.transform.SetParent(null, true);
            UnityEngine.Object.Destroy(copy);
        }

        private static T Template<T>(TrainCar car, Func<T, bool> predicate) where T : Component
        {
            // A live loop source's pitch/clip may already be adapted. Clone the
            // authored native audio prefab so that transient playback state is
            // never mistaken for the new reader's baseline.
            var prefab = car.carLivery?.parentType?.audioPrefab;
            return prefab != null ? prefab.GetComponentsInChildren<T>(true).FirstOrDefault(predicate) : null;
        }

        private static void SeatTenderChuffs(ChuffClipsSimReader reader, Transform outlet)
        {
            if (outlet == null || reader.regularChuffConfig?.parent == null)
                throw new InvalidOperationException("Erie chuff outlet or native emission parent missing.");
            var root = reader.transform;
            bool Owned(Transform leaf) => leaf != null && (leaf == root || leaf.IsChildOf(root));
            foreach (var config in new[] { reader.regularChuffConfig, reader.waterChuffConfig, reader.ashChuffConfig })
                if (config != null && config.parent != null && !Owned(config.parent))
                    throw new InvalidOperationException("Erie chuff emitter refers outside its owned clone.");
            foreach (var loop in (reader.chuffLoops ?? Array.Empty<ChuffClipsSimReader.ChuffLoop>())
                .Concat(reader.waterChuffLoops ?? Array.Empty<ChuffClipsSimReader.ChuffLoop>())
                .Concat(reader.ashChuffLoops ?? Array.Empty<ChuffClipsSimReader.ChuffLoop>()))
                if (loop?.chuffLoop != null && !Owned(loop.chuffLoop.transform))
                    throw new InvalidOperationException("Erie chuff loop refers outside its owned clone.");
            root.SetParent(outlet, false);
            root.localRotation = Quaternion.identity;
            // Align the actual emitter, not an assumed prefab pivot. Preserve
            // the authored source scale and relative water/ash/loop positions.
            root.position += outlet.position - reader.regularChuffConfig.parent.position;
        }

        private sealed class Pose
        {
            internal readonly Transform Leaf, Parent;
            private readonly Vector3 position, scale;
            private readonly Quaternion rotation;
            internal Pose(Transform leaf)
            { Leaf = leaf; Parent = leaf.parent; position = leaf.localPosition; rotation = leaf.localRotation; scale = leaf.localScale; }
            internal void Restore()
            {
                if (Leaf == null) return;
                Leaf.SetParent(Parent, false); Leaf.localPosition = position; Leaf.localRotation = rotation; Leaf.localScale = scale;
            }
        }

        internal abstract class AudioBinding
        {
            internal TrainCar Car;
            internal SimulationFlow Flow;
            internal bool Ready, Initialized;
            internal readonly List<GameObject> Copies = new();
            internal EffectsState Owner;
            internal abstract void Remove();
            protected void Forget()
            {
                foreach (var copy in Copies) DestroyCopy(copy);
                Copies.Clear();
                if (Owner != null) Owner.Audio.Remove(this);
                Owner = null; Initialized = false;
            }
        }

        // One extra native engine and the transform its presentation follows.
        internal sealed class Secondary
        {
            internal ReciprocatingSteamEngine Engine;
            internal Transform Root;
            internal int Index;
            internal string Name;
            internal bool Tender;
        }

        private sealed class ChuffBinding : AudioBinding
        {
            internal AudioClipSimReadersController Controller;
            private readonly List<ChuffClipsSimReader> readers = new();
            private ChuffClipsSimReader tunedFront;
            private float originalMedium, originalHigh;
            private AnimationCurve originalVolume;
            internal bool TenderReady => Owner != null && readers.Any(r => r != null &&
                r.chuffEventPortId == Owner.Simulation.MiddleEngine.chuffEventReadOut.id);
            internal void Add(EffectsState owner)
            {
                if (Copies.Count != 0 || !Ready) return;
                var template = Template<ChuffClipsSimReader>(Car, r => r.chuffEventPortId == owner.Simulation.MainEngine.chuffEventReadOut.id);
                if (template == null) throw new InvalidOperationException("Native S282 chuff template missing.");
                Owner = owner; owner.Audio.Add(this);
                if (ChuffStageTuning.HasLowPressureFront((int)Car.GetComponent<WheelRearranger>().currentWA))
                {
                    tunedFront = Controller.entries.OfType<ChuffClipsSimReader>().FirstOrDefault(r =>
                        !IsOwnedPresentation(r.transform) && r.chuffEventPortId == owner.Simulation.MainEngine.chuffEventReadOut.id);
                    if (tunedFront == null) throw new InvalidOperationException("Native front chuff reader missing.");
                    originalMedium = tunedFront.mediumPressureThreshold; originalHigh = tunedFront.highPressureThreshold;
                    originalVolume = tunedFront.pressureToVolumeCurve;
                    // A restrained presentation profile, not simulated compound
                    // pressure. Front Mallet/Erie machinery is the LP position.
                    tunedFront.mediumPressureThreshold = ChuffStageTuning.Pressure(template.mediumPressureThreshold);
                    tunedFront.highPressureThreshold = ChuffStageTuning.Pressure(template.highPressureThreshold);
                    tunedFront.pressureToVolumeCurve = ChuffStageTuning.Volume(template.pressureToVolumeCurve);
                }
                foreach (var secondary in owner.Secondaries)
                {
                    var engine = secondary.Engine;
                    var copy = Copy(template.gameObject, Controller.transform, secondary.Name + " exhaust"); Copies.Add(copy);
                    var reader = copy.GetComponent<ChuffClipsSimReader>();
                    reader.chuffEventPortId = engine.chuffEventReadOut.id;
                    reader.exhaustPressurePortId = engine.exhaustPressureReadOut.id;
                    reader.chuffFrequencyPortId = engine.chuffFrequencyReadOut.id;
                    reader.cylinderWaterNormalizedPortId = engine.waterInCylindersNormalizedReadOut.id;
                    reader.ashesInPipesPortId = engine.ashesInPipesReadOut.id;
                    if (secondary.Tender)
                    {
                        // Reuse the native spatial/mixer configuration, but put
                        // its pooled individual chuffs and loops at the tender
                        // exhaust. Native S282 config parents are inside this
                        // cloned reader subtree; never move a prefab reference.
                        var outlet = Car.GetComponent<WheelRearranger>().TenderExhaustOutlet;
                        SeatTenderChuffs(reader, outlet);
                        reader.mediumPressureThreshold = ChuffStageTuning.Pressure(template.mediumPressureThreshold);
                        reader.highPressureThreshold = ChuffStageTuning.Pressure(template.highPressureThreshold);
                        reader.pressureToVolumeCurve = ChuffStageTuning.Volume(template.pressureToVolumeCurve);
                    }
                    RequirePorts(Flow, reader.chuffEventPortId, reader.exhaustPressurePortId, reader.chuffFrequencyPortId,
                        reader.cylinderWaterNormalizedPortId, reader.cylinderCockControlPortId, reader.ashesInPipesPortId);
                    Controller.entries = Controller.entries.Append(reader).ToArray();
                    readers.Add(reader); Initialized = true; reader.Init(Car, Flow);
                }
            }
            internal override void Remove()
            {
                if (tunedFront != null)
                {
                    tunedFront.mediumPressureThreshold = originalMedium; tunedFront.highPressureThreshold = originalHigh;
                    tunedFront.pressureToVolumeCurve = originalVolume;
                }
                tunedFront = null; originalVolume = null;
                if (Initialized) foreach (var reader in readers) Safe(reader.Deinit);
                if (Controller != null) Controller.entries = Controller.entries.Where(r => !readers.Any(x => ReferenceEquals(x, r))).ToArray();
                readers.Clear(); Forget();
            }
        }

        private sealed class CockBinding : AudioBinding
        {
            internal LayeredAudioSimReadersController Controller;
            private readonly List<CylinderCockLayeredPortReader> readers = new();
            private readonly List<LayeredAudioPortReader> mechanisms = new();
            private readonly List<Pose> primaryLeaves = new();
            private readonly List<GameObject> secondaryLeaves = new();
            internal bool TenderReady => Owner != null && readers.Any(r => r != null &&
                r.crankRotationPortId == Owner.Simulation.MiddleEngine.crankRotationReadOut.id) && mechanisms.Any(r => r != null &&
                r.portId == Owner.Simulation.MiddleEngine.chuffFrequencyReadOut.id);
            internal void Add(EffectsState owner)
            {
                if (Copies.Count != 0 || !Ready) return;
                var primary = Controller.entries.OfType<CylinderCockLayeredPortReader>()
                    .FirstOrDefault(r => !IsOwnedPresentation(r.transform) && r.crankRotationPortId == owner.Simulation.MainEngine.crankRotationReadOut.id);
                var template = Template<CylinderCockLayeredPortReader>(Car, r => r.crankRotationPortId == owner.Simulation.MainEngine.crankRotationReadOut.id);
                if (primary == null || template == null) throw new InvalidOperationException("Native S282 cylinder-cock audio template missing.");
                Owner = owner; owner.Audio.Add(this);
                foreach (var audio in owner.Simulation.IsActive ? primary.cylCockAudio : Array.Empty<LayeredAudio>())
                {
                    if (audio == null) continue;
                    primaryLeaves.Add(new Pose(audio.transform));
                    Vector3 world = audio.transform.position;
                    audio.transform.SetParent(owner.Front, true);
                    audio.transform.localPosition = owner.FrontOutletLocal(world, audio.transform.localPosition);
                }
                foreach (var secondary in owner.Secondaries)
                {
                    var engine = secondary.Engine;
                    var copy = Copy(template.gameObject, Controller.transform, secondary.Name + " cylinder-cock audio"); Copies.Add(copy);
                    var reader = copy.GetComponent<CylinderCockLayeredPortReader>();
                    reader.crankRotationPortId = engine.crankRotationReadOut.id;
                    reader.cylindersInletValveOpenPortId = engine.cylindersInletValveOpen.id;
                    reader.cylinderCockFlowNormalizedPortId = engine.cylinderCockFlowNormalizedReadOut.id;
                    RequirePorts(Flow, reader.crankRotationPortId, reader.cylindersInletValveOpenPortId,
                        reader.cylinderCockFlowNormalizedPortId, reader.cylinderCockControlPortId);
                    foreach (var audio in reader.cylCockAudio)
                    {
                        if (audio == null) continue;
                        // Templates live in audio-prefab coordinates; retain their
                        // authored car-local outlet coordinates in the live car.
                        Vector3 carPosition = template.transform.root.InverseTransformPoint(template.cylCockAudio[Array.IndexOf(reader.cylCockAudio, audio)].transform.position);
                        audio.transform.SetParent(secondary.Root, false);
                        audio.transform.localPosition = owner.SecondaryOutletLocal(carPosition, secondary);
                        audio.gameObject.AddComponent<ArticulatedEffectMarker>(); secondaryLeaves.Add(audio.gameObject);
                    }
                    Controller.entries = Controller.entries.Append(reader).ToArray();
                    readers.Add(reader); Initialized = true; reader.Init(Car, Flow);
                    if (secondary.Tender)
                    {
                        var mechanism = Template<LayeredAudioPortReader>(Car, r => r.gameObject.name == "Mechanism_Layered" &&
                            r.portId == owner.Simulation.MainEngine.chuffFrequencyReadOut.id);
                        if (mechanism == null) throw new InvalidOperationException("Native valve-gear mechanism audio template missing.");
                        var machine = Copy(mechanism.gameObject, secondary.Root, "tender driving-gear clanks"); Copies.Add(machine);
                        machine.transform.localPosition = Car.GetComponent<WheelRearranger>().TenderMechanismLocal;
                        machine.transform.localRotation = Quaternion.identity;
                        var sound = machine.GetComponent<LayeredAudioPortReader>();
                        sound.portId = engine.chuffFrequencyReadOut.id;
                        RequirePorts(Flow, sound.portId);
                        Controller.entries = Controller.entries.Append(sound).ToArray();
                        mechanisms.Add(sound); sound.Init(Car, Flow);
                    }
                }
            }
            internal override void Remove()
            {
                if (Initialized) foreach (var reader in readers) Safe(reader.Deinit);
                foreach (var mechanism in mechanisms) Safe(mechanism.Deinit);
                if (Controller != null) Controller.entries = Controller.entries.Where(r => !mechanisms.Any(x => ReferenceEquals(x, r))).ToArray();
                mechanisms.Clear();
                if (Controller != null) Controller.entries = Controller.entries.Where(r => !readers.Any(x => ReferenceEquals(x, r))).ToArray();
                foreach (var pose in primaryLeaves) Safe(pose.Restore); primaryLeaves.Clear();
                foreach (var leaf in secondaryLeaves) Safe(() => DestroyCopy(leaf)); secondaryLeaves.Clear();
                readers.Clear(); Forget();
            }
        }

        internal sealed class EffectsState : MonoBehaviour
        {
            internal TrainCar Car;
            internal bool Active;
            internal readonly HashSet<AudioBinding> Audio = new();
            internal ArticulatedSimulation Simulation => Car.GetComponent<ArticulatedSimulation>();
            internal Transform Front => Car.GetComponent<WheelRearranger>().ArticulatedFrontRoot;
            internal Transform Rear => Car.GetComponent<WheelRearranger>().ArticulatedRearRoot;
            internal Transform Middle => Car.GetComponent<WheelRearranger>().ArticulatedMiddleRoot;
            internal Vector3 FrontOutletOffset => Car.GetComponent<WheelRearranger>().currentWA == WheelArrangementType.s4444
                ? Vector3.forward * .35f : Vector3.zero;
            // MainEngine is the front unit. RearEngine is the rear unit; the
            // third engine is either a locomotive middle unit or Erie's tender.
            internal List<Secondary> Secondaries
            {
                get
                {
                    var list = new List<Secondary>();
                    if (Simulation.IsActive) list.Add(new Secondary { Engine = Simulation.RearEngine, Root = Rear, Index = 1, Name = "rear" });
                    if (Simulation.IsMiddleActive && Middle != null && !Simulation.IsTenderEngine)
                        list.Add(new Secondary { Engine = Simulation.MiddleEngine, Root = Middle, Index = 2, Name = "middle" });
                    var tender = Car.GetComponent<WheelRearranger>().TenderEffectsRoot;
                    if (Simulation.IsTenderEngine && Simulation.IsMiddleActive && tender != null &&
                        Car.GetComponent<WheelRearranger>().TenderExhaustOutlet != null)
                        list.Add(new Secondary { Engine = Simulation.MiddleEngine, Root = tender, Index = 3, Name = "tender", Tender = true });
                    return list;
                }
            }
            internal List<Secondary> ParticleSecondaries
            {
                get
                {
                    return Secondaries;
                }
            }
            // Front outlet in front-engine coordinates, from its native world position.
            internal Vector3 FrontOutletLocal(Vector3 nativeWorld, Vector3 reparentedLocal) =>
                FrontOutletLocal(nativeWorld, reparentedLocal, nativeWorld);
            internal Vector3 FrontOutletLocal(Vector3 nativeWorld, Vector3 reparentedLocal, Vector3 nativeCentre) =>
                Car.GetComponent<WheelRearranger>().TryMapArticulatedOutlet(nativeWorld, nativeCentre, 0, out var mapped)
                    ? mapped : reparentedLocal + FrontOutletOffset;
            // Extra-engine outlet in that engine's coordinates, from its authored car-local position.
            internal Vector3 SecondaryOutletLocal(Vector3 carLocal, Secondary secondary) =>
                SecondaryOutletLocal(carLocal, secondary, carLocal);
            internal Vector3 SecondaryOutletLocal(Vector3 carLocal, Secondary secondary, Vector3 centreCarLocal)
            {
                var rearranger = Car.GetComponent<WheelRearranger>();
                var origin = Car.transform.TransformPoint(carLocal); var centre = Car.transform.TransformPoint(centreCarLocal);
                if (secondary.Tender)
                {
                    if (!rearranger.TryMapTenderOutlet(origin, centre, out var tenderLocal))
                        throw new InvalidOperationException("Erie tender outlet mapping unavailable.");
                    return tenderLocal;
                }
                return rearranger.TryMapArticulatedOutlet(origin, centre, secondary.Index, out var mapped)
                    ? mapped : carLocal + Vector3.back * EngineSeparation;
            }
            private ParticleBinding particles;
            private Transform particleTender;
            internal bool TenderReady => particles != null && particles.TenderReady;
            internal bool TenderAudioReady => Audio.OfType<ChuffBinding>().Any(b => b.TenderReady) &&
                Audio.OfType<CockBinding>().Any(b => b.TenderReady);
            internal void ResetAudio()
            {
                foreach (var binding in Audio.ToArray()) Safe(binding.Remove);
                Audio.Clear();
            }
            internal void ResetParticles()
            {
                if (particles != null) Safe(particles.Remove);
                particles = null;
                particleTender = null;
            }
            private float nextSearch;
            private bool warned;

            internal void RefreshNow()
            {
                if (!Active || Car == null || Simulation == null || (!Simulation.IsActive && !Simulation.IsTenderEngine)) return;
                if (Simulation.IsActive && (Front == null || Rear == null)) return;
                try
                {
                    var tender = Car.GetComponent<WheelRearranger>().TenderEffectsRoot;
                    if (!ReferenceEquals(tender, particleTender)) { ResetAudio(); ResetParticles(); particleTender = tender; }
                    foreach (var controller in AudioControllers<AudioClipSimReadersController>(Car))
                        if (Chuffs.TryGetValue(controller, out var binding) && AudioBindingMatches(binding, Car, Simulation.Flow)) binding.Add(this);
                    foreach (var controller in AudioControllers<LayeredAudioSimReadersController>(Car))
                        if (Cocks.TryGetValue(controller, out var binding) && AudioBindingMatches(binding, Car, Simulation.Flow)) binding.Add(this);
                    if (particles == null)
                    {
                        var controller = Car.SimController.particlesController;
                        if (controller != null && ReferenceEquals(ParticleFlow?.GetValue(controller), Simulation.Flow))
                        { particles = new ParticleBinding(this, controller); particles.Add(); particleTender = tender; }
                    }
                }
                catch (Exception ex)
                {
                    if (!warned) { Main.Logger.LogException("Articulated native effects setup failed", ex); warned = true; }
                    Stop();
                }
            }
            internal void Stop()
            {
                Active = false;
                ResetAudio();
                if (particles != null) Safe(particles.Remove);
                particles = null;
            }
            internal void RemoveParticles(ParticlesPortReadersController controller)
            {
                if (particles == null || !particles.BelongsTo(controller)) return;
                Safe(particles.Remove); particles = null;
            }
            private void LateUpdate()
            {
                if (Active && Time.unscaledTime >= nextSearch) { nextSearch = Time.unscaledTime + .5f; RefreshNow(); }
            }
            private void OnDestroy() { Stop(); }
        }

        private sealed class ParticleBinding
        {
            private readonly EffectsState owner;
            private readonly ParticlesPortReadersController controller;
            private readonly SimulationFlow flow;
            private readonly List<Pose> primaryLeaves = new();
            private readonly List<GameObject> copies = new();
            private readonly List<ParticlesPortReadersController.ParticlePortReader> engineOutlets = new();
            private readonly List<CylinderCockParticlePortReader> readers = new();
            private SteamSmokeParticlePortReader tenderExhaust;
            internal bool TenderReady => tenderExhaust != null && readers.Any(r => r != null && r.crankRotationPortId == owner.Simulation.MiddleEngine.crankRotationReadOut.id);
            private readonly List<(Port events, Action<float> callback)> smoke = new();
            private bool initialized;

            internal ParticleBinding(EffectsState owner, ParticlesPortReadersController controller)
            { this.owner = owner; this.controller = controller; flow = owner.Simulation.Flow; }
            internal bool BelongsTo(ParticlesPortReadersController other) => controller == other;

            internal void Add()
            {
                var main = owner.Simulation.MainEngine;
                var secondaries = owner.ParticleSecondaries;
                var original = controller.entries.OfType<CylinderCockParticlePortReader>()
                    .FirstOrDefault(r => !IsOwnedPresentation(r.transform) && r.crankRotationPortId == main.crankRotationReadOut.id);
                var prefab = owner.Car.carLivery.prefab;
                var templateController = prefab != null ? prefab.GetComponentsInChildren<ParticlesPortReadersController>(true)
                    .FirstOrDefault(c => c.entries.OfType<CylinderCockParticlePortReader>().Any(r => r.crankRotationPortId == main.crankRotationReadOut.id)) : null;
                var template = templateController != null ? templateController.entries.OfType<CylinderCockParticlePortReader>()
                    .FirstOrDefault(r => r.crankRotationPortId == main.crankRotationReadOut.id) : null;
                if (original == null || template == null) throw new InvalidOperationException("Native cylinder-cock particle template missing.");
                // Verify every authored outlet and its complete port list, for every
                // extra engine, before moving native leaves or registering readers.
                var plans = secondaries.Select(secondary =>
                {
                    var engine = secondary.Engine;
                    RequirePorts(flow, engine.crankRotationReadOut.id, template.reverserPortId,
                        engine.cylindersInletValveOpen.id, engine.cylinderCockFlowNormalizedReadOut.id, template.forwardSpeedPortId);
                    var outletPorts = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        { main.waterInCylindersNormalizedReadOut.id, engine.waterInCylindersNormalizedReadOut.id },
                        { main.cylinderCrackFlowNormalizedReadOut.id, engine.cylinderCrackFlowNormalizedReadOut.id }
                    };
                    var outlets = controller.particlePortReaders
                        .Where(source => source.particlesParent != null && source.particleUpdaters.Any(p => outletPorts.ContainsKey(p.portId)))
                        .Select(source =>
                        {
                            var leaf = templateController.transform.Find(LocalPath(source.particlesParent.transform, controller.transform));
                            if (leaf == null) throw new InvalidOperationException("Native cylinder outlet template missing.");
                            var updaters = source.particleUpdaters.Select(p => new ParticlesPortReadersController.ParticlePortReader.PortParticleUpdateDefinition
                            {
                                portId = outletPorts.TryGetValue(p.portId, out var id) ? id : p.portId,
                                inputModifier = p.inputModifier,
                                propertiesToUpdate = p.propertiesToUpdate
                            }).ToList();
                            RequirePorts(flow, updaters.Select(p => p.portId).ToArray());
                            return (Source: source, Template: leaf, Updaters: updaters);
                        }).ToArray();
                    return (Secondary: secondary, Outlets: outlets);
                }).ToArray();
                foreach (var setup in original.cylinderSetups)
                    foreach (var leaf in new[] { setup.frontParticlesParent, setup.rearParticlesParent }) MovePrimary(leaf);
                foreach (var plan in plans)
                {
                    var secondary = plan.Secondary; var engine = secondary.Engine;
                    // A live primary's speed/size/emission may already be modulated.
                    // Each extra reader must capture authored particle baselines.
                    var root = Copy(template.gameObject, controller.transform, secondary.Name + " cylinder-cock particles"); copies.Add(root);
                    root.transform.position = original.transform.position; root.transform.rotation = original.transform.rotation;
                    var reader = root.GetComponent<CylinderCockParticlePortReader>();
                    reader.crankRotationPortId = engine.crankRotationReadOut.id;
                    reader.cylindersInletValveOpenPortId = engine.cylindersInletValveOpen.id;
                    reader.cylinderCockFlowNormalizedPortId = engine.cylinderCockFlowNormalizedReadOut.id;
                    RequirePorts(flow, reader.crankRotationPortId, reader.reverserPortId,
                        reader.cylindersInletValveOpenPortId, reader.cylinderCockFlowNormalizedPortId, reader.forwardSpeedPortId);
                    foreach (var setup in reader.cylinderSetups)
                        foreach (var leaf in new[] { setup.frontParticlesParent, setup.rearParticlesParent }) MoveCopy(leaf, secondary);
                    controller.entries = controller.entries.Append(reader).ToArray();
                    readers.Add(reader); initialized = true; reader.Init(flow);
                    foreach (var definition in plan.Outlets)
                    {
                        var source = definition.Source;
                        bool crack = source.particleUpdaters.Any(p => p.portId == main.cylinderCrackFlowNormalizedReadOut.id);
                        var parent = Copy(definition.Template.gameObject, controller.transform,
                            secondary.Name + (crack ? " cylinder crack" : " cylinder water drip")); copies.Add(parent);
                        parent.transform.position = source.particlesParent.transform.position; parent.transform.rotation = source.particlesParent.transform.rotation;
                        MovePrimary(source.particlesParent); MoveCopy(parent, secondary);
                        var outlet = new ParticlesPortReadersController.ParticlePortReader
                        {
                            particlesParent = parent,
                            particlesPortReader = controller,
                            particleUpdaters = definition.Updaters
                        };
                        engineOutlets.Add(outlet); controller.particlePortReaders.Add(outlet); outlet.Init();
                        foreach (var updater in outlet.particleUpdaters) updater.Init(outlet, flow);
                    }
                    var smokeReader = controller.entries.OfType<SteamSmokeParticlePortReader>().FirstOrDefault(r => r.chuffEventPortId == main.chuffEventReadOut.id);
                    if (secondary.Tender)
                    {
                        AddTenderExhaust(templateController, smokeReader, secondary);
                    }
                    else if (smokeReader != null && SmokeChuff != null)
                    {
                        var pulse = AccessTools.MethodDelegate<Action<float>>(SmokeChuff, smokeReader);
                        // All exhaust groups share one native chimney. Retain its
                        // main pressure response; gate each extra pulse by that
                        // engine's pressure. Never add numeric event indices.
                        Action<float> callback = value => { if (engine.exhaustPressureReadOut.Value > 1f) pulse(value); };
                        engine.chuffEventReadOut.ValueUpdatedInternally += callback;
                        smoke.Add((engine.chuffEventReadOut, callback));
                    }
                }
            }

            private void AddTenderExhaust(ParticlesPortReadersController authored, SteamSmokeParticlePortReader native, Secondary secondary)
            {
                var outlet = owner.Car.GetComponent<WheelRearranger>().TenderExhaustOutlet;
                var steam = authored.transform.Find("SteamSafetyRelease/SteamExhaust");
                if (outlet == null || steam == null || native == null)
                    throw new InvalidOperationException("Erie exhaust outlet or native white-steam template missing.");
                var root = new GameObject("[Rearranged articulated effects] tender exhaust");
                root.AddComponent<ArticulatedEffectMarker>(); root.transform.SetParent(outlet, false); copies.Add(root);
                var plume = Copy(steam.gameObject, root.transform, "tender white exhaust steam");
                plume.transform.localPosition = Vector3.zero;
                // The authored safety-valve steam is aimed along local +Z.
                plume.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                plume.transform.localScale = Vector3.one;
                foreach (var ps in plume.GetComponentsInChildren<ParticleSystem>(true))
                {
                    ps.gameObject.SetActive(true);
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    var main = ps.main; main.playOnAwake = false;
                    main.simulationSpace = ParticleSystemSimulationSpace.World;
                }
                plume.SetActive(true);
                var empty = new GameObject("no embers"); empty.transform.SetParent(root.transform, false);
                var reader = root.AddComponent<SteamSmokeParticlePortReader>();
                // A normal tender has no fire. Reuse the native broken-boiler
                // state (normally zero) to disable the reader's idle-fire plume;
                // its own native broken-boiler gate also suppresses damaged flow.
                reader.fireOnPortId = reader.isBoilerBrokenPortId = native.isBoilerBrokenPortId;
                reader.chuffEventPortId = secondary.Engine.chuffEventReadOut.id;
                reader.exhaustPressurePortId = secondary.Engine.exhaustPressureReadOut.id;
                reader.smokeParticlesParent = plume; reader.emberParticlesParent = empty;
                reader.smokeStartSpeedMultiplier = native.smokeStartSpeedMultiplier;
                reader.smokeEmissionRateMultiplier = native.smokeEmissionRateMultiplier;
                reader.smokeMaxParticlesMultiplier = native.smokeMaxParticlesMultiplier;
                reader.emberStartSpeedMultiplier = native.emberStartSpeedMultiplier;
                reader.emberEmissionRateMultiplier = native.emberEmissionRateMultiplier;
                reader.emberMaxParticlesMultiplier = native.emberMaxParticlesMultiplier;
                RequirePorts(flow, reader.fireOnPortId, reader.chuffEventPortId, reader.exhaustPressurePortId);
                tenderExhaust = reader;
                controller.entries = controller.entries.Append(reader).ToArray();
                reader.Init(flow);
            }

            private static string LocalPath(Transform leaf, Transform root)
            {
                var names = new Stack<string>();
                for (var current = leaf; current != root; current = current.parent)
                {
                    if (current == null) throw new InvalidOperationException("Native particle outlet is outside its controller.");
                    names.Push(current.name);
                }
                return string.Join("/", names);
            }

            private void MovePrimary(GameObject leaf)
            {
                if (!owner.Simulation.IsActive || leaf == null || primaryLeaves.Any(p => p.Leaf == leaf.transform)) return;
                primaryLeaves.Add(new Pose(leaf.transform));
                Vector3 world = leaf.transform.position;
                Vector3 centre = leaf.transform.TransformPoint(PacificHudsonAffine.EmitterCentreLocal(leaf.transform));
                leaf.transform.SetParent(owner.Front, true);
                leaf.transform.localPosition = owner.FrontOutletLocal(world, leaf.transform.localPosition, centre);
            }

            private void MoveCopy(GameObject leaf, Secondary secondary)
            {
                if (leaf == null) return;
                Vector3 position = owner.Car.transform.InverseTransformPoint(leaf.transform.position);
                Vector3 centre = owner.Car.transform.InverseTransformPoint(leaf.transform.TransformPoint(PacificHudsonAffine.EmitterCentreLocal(leaf.transform)));
                Quaternion rotation = leaf.transform.rotation;
                if (secondary.Tender)
                    rotation = secondary.Root.rotation * Quaternion.Inverse(owner.Car.transform.rotation) * rotation;
                leaf.transform.SetParent(secondary.Root, false);
                leaf.transform.localPosition = owner.SecondaryOutletLocal(position, secondary, centre);
                leaf.transform.rotation = rotation;
                if (leaf.GetComponent<ArticulatedEffectMarker>() == null) leaf.AddComponent<ArticulatedEffectMarker>();
                if (!copies.Contains(leaf)) copies.Add(leaf);
                foreach (var particle in leaf.GetComponentsInChildren<ParticleSystem>(true))
                {
                    for (var t = particle.transform; t != null && t != leaf.transform.parent; t = t.parent) t.gameObject.SetActive(true);
                    var settings = particle.main; settings.playOnAwake = false;
                    particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }

            internal void Remove()
            {
                if (tenderExhaust != null)
                {
                    Safe(tenderExhaust.Deinit);
                    if (controller != null) controller.entries = controller.entries.Where(r => !ReferenceEquals(r, tenderExhaust)).ToArray();
                    tenderExhaust = null;
                }
                foreach (var (events, callback) in smoke) events.ValueUpdatedInternally -= callback;
                smoke.Clear();
                if (initialized) foreach (var reader in readers) Safe(reader.Deinit);
                if (controller != null) controller.entries = controller.entries.Where(r => !readers.Any(x => ReferenceEquals(x, r))).ToArray();
                readers.Clear();
                foreach (var outlet in engineOutlets)
                {
                    foreach (var updater in outlet.particleUpdaters) Safe(() => updater.Deinit(flow));
                    Safe(outlet.OnDisable); if (controller != null) controller.particlePortReaders.Remove(outlet);
                }
                engineOutlets.Clear();
                foreach (var pose in primaryLeaves) Safe(pose.Restore); primaryLeaves.Clear();
                foreach (var copy in copies) Safe(() => DestroyCopy(copy)); copies.Clear();
                initialized = false;
            }
        }

        [HarmonyPatch(typeof(AudioClipSimReadersController), nameof(AudioClipSimReadersController.Init))]
        private static class ClipInit
        {
            private static void Prefix(AudioClipSimReadersController __instance)
            { if (Chuffs.TryGetValue(__instance, out var slot)) { Safe(slot.Remove); slot.Ready = false; } }
            private static void Postfix(AudioClipSimReadersController __instance, TrainCar car, SimulationFlow simFlow)
            {
                var slot = Chuffs.GetValue(__instance, _ => new ChuffBinding { Controller = __instance });
                slot.Car = car; slot.Flow = simFlow; slot.Ready = true;
                var state = ActiveState(car); if (state != null) Safe(state.RefreshNow);
            }
        }
        [HarmonyPatch(typeof(AudioClipSimReadersController), nameof(AudioClipSimReadersController.Deinit))]
        private static class ClipDeinit
        {
            private static void Prefix(AudioClipSimReadersController __instance)
            { if (Chuffs.TryGetValue(__instance, out var slot)) { Safe(slot.Remove); slot.Ready = false; slot.Car = null; } }
        }
        [HarmonyPatch(typeof(LayeredAudioSimReadersController), nameof(LayeredAudioSimReadersController.Init))]
        private static class LayerInit
        {
            private static void Prefix(LayeredAudioSimReadersController __instance)
            { if (Cocks.TryGetValue(__instance, out var slot)) { Safe(slot.Remove); slot.Ready = false; } }
            private static void Postfix(LayeredAudioSimReadersController __instance, TrainCar car, SimulationFlow simFlow)
            {
                var slot = Cocks.GetValue(__instance, _ => new CockBinding { Controller = __instance });
                slot.Car = car; slot.Flow = simFlow; slot.Ready = true;
                var state = ActiveState(car); if (state != null) Safe(state.RefreshNow);
            }
        }
        [HarmonyPatch(typeof(LayeredAudioSimReadersController), nameof(LayeredAudioSimReadersController.Deinit))]
        private static class LayerDeinit
        {
            private static void Prefix(LayeredAudioSimReadersController __instance)
            { if (Cocks.TryGetValue(__instance, out var slot)) { Safe(slot.Remove); slot.Ready = false; slot.Car = null; } }
        }
        [HarmonyPatch(typeof(ParticlesPortReadersController), nameof(ParticlesPortReadersController.Init))]
        private static class ParticleInit
        {
            private static void Prefix(ParticlesPortReadersController __instance)
            { ActiveState(TrainCar.Resolve(__instance.gameObject))?.RemoveParticles(__instance); }
            private static void Postfix(ParticlesPortReadersController __instance)
            { ActiveState(TrainCar.Resolve(__instance.gameObject))?.RefreshNow(); }
        }
        [HarmonyPatch(typeof(ParticlesPortReadersController), "OnDestroy")]
        private static class ParticleDestroy
        {
            private static void Prefix(ParticlesPortReadersController __instance)
            { var state = ActiveState(TrainCar.Resolve(__instance.gameObject)); if (state != null) Safe(() => state.RemoveParticles(__instance)); }
        }
    }

    internal sealed class ArticulatedEffectMarker : MonoBehaviour { }
}
#endif
