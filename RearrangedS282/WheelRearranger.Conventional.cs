#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DV.ThingTypes;
using DV.Wheels;
using LocoSim.Implementations.Wheels;
using LocoMeshSplitter.MeshLoaders;
using UnityEngine;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        private GeometryState stockGeometry, preparedGeometry;
        private Transform[] generatedRoots = Array.Empty<Transform>();
        private bool conventionalFault;
        private ArticulatedSimulation articulatedSimulation;
        private B99ExplosionMaterials explosionMaterials;
        private CylinderCalibration cylinderCalibration;
        private float preliminaryDriverShare = float.NaN;
        internal int ExplosionMaterialRendererCount => explosionMaterials?.Count ?? 0;
        internal bool ConventionalSetupBlocked => conventionalFault;
        internal string LastConventionalError { get; private set; }
        internal bool LastSwitchRestoredStock { get; private set; }
#if B99_DIAGNOSTICS || B99_STRUCTURAL_DIAGNOSTICS
        private bool? structuralTenderOverride;
        internal bool TrySwitchStructural(int id, bool tender)
        {
            if (!Stage2RuntimeChecks.Running || tender && !PoweredTenderPolicy.Supports(id)) return false;
            structuralTenderOverride = tender;
            try { return TrySwitchStage2(id); }
            finally { structuralTenderOverride = null; }
        }
#endif

        private bool TrySwitchConventional(int id)
        {
            LastSwitchRestoredStock = false;
            bool remote = MultiplayerSync.ApplyingRemote;
            if (!WheelArrangement.IsSupportedId(id) || skipWheelArrangementChange || conventionalFault ||
                !remote && independentDrive != null && !independentDrive.Settled) return false;
            loco = GetComponent<TrainCar>();
            var sim = loco?.SimController;
            if (!S282Target.IsNormal(loco) || sim?.simFlow == null ||
                !sim.simFlow.TryGetPort("throttle.EXT_IN", out var throttle) ||
                sim.drivingForce == null || sim.wheelslipController == null || loco.adhesionController == null ||
                (!remote && (loco.derailed || loco.isExploded || Mathf.Abs(loco.GetForwardSpeed()) > .01f ||
                    throttle.Value > .001f || Mathf.Abs(sim.drivingForce.generatedForce) > 500f ||
                    sim.wheelslipController.IsWheelslipping || loco.adhesionController.IsWheelSliding)))
            {
                Main.Logger.Warning("Use a repaired, stationary S282 with its regulator closed and wheel slip/driving force settled before switching.");
                return false;
            }
            bool wantsTender = ArrangementSettings != null && ArrangementSettings.PoweredTenderEnabled(id);
#if B99_DIAGNOSTICS || B99_STRUCTURAL_DIAGNOSTICS
            if (Stage2RuntimeChecks.Running && structuralTenderOverride.HasValue) wantsTender = structuralTenderOverride.Value;
#endif
            if (stockGeometry == null && id == 10 && !wantsTender) return true;
            LastConventionalError = null;
            skipWheelArrangementChange = true;
            try
            {
                ReleaseIndependentDrive();
                DeltaTrailingTruckCompatibility.Suspend(loco);
                DetachTenderEngine();
                if (stockGeometry == null)
                {
                    splitS282A = GetComponent<S282AMeshLoader>();
                    if (splitS282A == null || transform.Find("LocoS282A_Body/Static_LOD0/s282a_split_body(Clone)") == null)
                        throw new InvalidOperationException("Loco Mesh Splitter has not prepared this S282 yet.");
                    // Extract rods before modifying the locomotive, using the original mod's helper.
                    var meshes = MeshFinder.Instance;
                    stage2Physics = new B99PhysicsState(loco);
                    stockGeometry = new GeometryState(loco);
                    explosionMaterials = new B99ExplosionMaterials(loco,
                        transform.Find("LocoS282A_Body/MovingParts_LOD0"));
                    var originalTransforms = new HashSet<Transform>(GetComponentsInChildren<Transform>(true));
                    try
                    {
                        SetupBody(); SetupLubrication(); SetupAir();
                        if (loco.AreExternalInteractablesLoaded) LoadExternalInteractables(loco.loadedExternalInteractables);
                        SetupFrontBogie(); SetupRearBogie(); SetupDrivers();
                        CacheDriverLayouts();
                        CacheConnectingRodSpacing();
                        CacheOilCupMounts();
                        CacheCylinderCasings();
                        CacheSteamConnections();
                        SetupPacificHudsonValveGear();
                        SetupArticulatedGeometry();
                        preparedGeometry = new GeometryState(loco);
                    }
                    finally
                    {
                        generatedRoots = GetComponentsInChildren<Transform>(true)
                            .Where(t => !originalTransforms.Contains(t) && originalTransforms.Contains(t.parent)).ToArray();
                    }
                }
                float quarteringCycle = CaptureQuarteringCycle();
                if (cylinderCalibration == null)
                    cylinderCalibration = new CylinderCalibration(sim.simFlow.OrderedSimComps.OfType<LocoSim.Implementations.ReciprocatingSteamEngine>().ToArray(),
                        stage2Physics.DriverRadius, stage2Physics.OriginalSlipCoefficient,
                        stage2Physics.OriginalPowered.Length, stage2Physics.OriginalTotalAxles);
                if (id == 10)
                {
                    if (wantsTender) { preparedGeometry.Restore(); EnsurePoweredTenderSource(); }
                    RestoreConventionalStock(quarteringCycle);
                }
                else
                {
                    cylinderCalibration.Restore();
                    SuspendHiddenNativeLinkages();
                    SuspendSteamConnections();
                    SuspendConnectingRodSpacing();
                    RestoreCylinderCasings();
                    ArticulatedEffects.Deinit(loco);
                    articulatedSimulation?.Restore();
                    SuspendArticulatedGeometry();
                    SuspendAdditionalGeometry();
                    Suspend4144Geometry();
                    SuspendDriverLayout();
                    preparedGeometry.Restore();
                    currentWA = WheelArrangementType.s282;
                    if (wantsTender) EnsurePoweredTenderSource();
                    if (WheelArrangement.IsArticulatedId(id))
                    {
                        articulatedSimulation = ArticulatedSimulation.Ensure(loco);
                        if (id == (int)WheelArrangementType.s2662) ApplyMalletGeometry();
                        else if (id == (int)WheelArrangementType.s4664LittleBoy) ApplyLittleBoyGeometry();
                        else if (id == (int)WheelArrangementType.s22222) ApplyTriplexGeometry(true);
                        else if (id == (int)WheelArrangementType.s02220) ApplyTriplexGeometry(false);
                        else if (id == (int)WheelArrangementType.s24440) ApplyTriplexGeometry(true, true);
                        else if (IsBigMallet(id) || id == (int)WheelArrangementType.s28882) ApplyBigMalletGeometry((WheelArrangementType)id);
                        else ApplyArticulatedGeometry(id);
                    }
                    else
                    {
                        if (id == (int)WheelArrangementType.s4144) Apply4144Geometry();
                        else
                        {
                            ApplyLegacyGeometry(id);
                            if (id == (int)WheelArrangementType.s462Scotch) ApplyScotchGliderGeometry();
                            ApplyLargeDriverClearance(id, sim.drivingForce.wheelRadius);
                            if (id == (int)WheelArrangementType.s262) ApplyPrairieForwardSpacing(false);
                            ApplyDriverLayout(id);
                            ApplyConnectingRodSpacing(id, id == (int)WheelArrangementType.s262
                                ? PrairieCylinderForward - PrairieDriverForward + prairieClearanceCylinderShiftM : 0f);
                            if (id == (int)WheelArrangementType.s262)
                            {
                                // One retarget owns the existing Prairie span plus
                                // temporary spacing after final scaling. Polish reads that frame.
                                ApplyPacificHudsonValveGear(ArrangementSettings.polishedPacificHudsonValveGear);
                            }
                            else if (UsesReservoirClearance(id))
                                ApplyPacificHudsonValveGear(ArrangementSettings.show282BigValveGear);
                            else if (id == (int)WheelArrangementType.s462 || id == (int)WheelArrangementType.s464 || id == (int)WheelArrangementType.s462Scotch)
                                ApplyPacificHudsonValveGear(ArrangementSettings.polishedPacificHudsonValveGear);
                            else if (id == (int)WheelArrangementType.s440 || id == (int)WheelArrangementType.s442 ||
                                id == (int)WheelArrangementType.s444 || id == (int)WheelArrangementType.s240)
                                ApplyPacificHudsonValveGear(ArrangementSettings.show4CoupledValveGear);
                            else if (id == (int)WheelArrangementType.s460 || id == (int)WheelArrangementType.s260)
                                ApplyPacificHudsonValveGear(ArrangementSettings.show6CoupledValveGear);
                            else if (id >= (int)WheelArrangementType.s0120 && id <= (int)WheelArrangementType.s4122)
                                ApplyPacificHudsonValveGear(ArrangementSettings.show12CoupledValveGear);
                            AlignAdditionalPoweredAxles(id);
                        }
                    }
                    ApplyCylinderCasings(id);
                    ApplyFranklinDriverPins(id);
                    foreach (var bogie in loco.Bogies)
                        bogie.axles = Enumerable.Range(0, bogie.transform.Find("bogie_car").childCount)
                            .Select(i => bogie.transform.Find("bogie_car").GetChild(i))
                            .Where(t => t.name.StartsWith("[axle]", StringComparison.Ordinal) && t.gameObject.activeInHierarchy)
                            .Select(t => new Bogie.AxleInfo(t)).ToArray();
                    var powered = new[] { driveAxle1, driveAxle2, driveAxle3, driveAxle4, driveAxle5, driveAxle6, driveAxle7 }
                        .Concat(BigMalletExtraAxles())
                        .Where(t => t != null && t.gameObject.activeInHierarchy).Select(t => t.GetComponent<PoweredWheel>()).ToArray();
                    int total = loco.Bogies.Sum(b => b.Axles.Length);
                    var wheels = WheelArrangement.LocomotiveWheels(id);
                    int expectedPowered = WheelArrangement.IsArticulatedId(id) ? wheels.Skip(1).Take(wheels.Length - 2).Sum() / 2 : wheels[1] / 2;
                    if (powered.Length != expectedPowered || total != wheels.Sum() / 2)
                        throw new InvalidOperationException("Active physical axles do not match the selected arrangement.");
                    // Effective adhesive load using existing native friction conversion; actual mass and CoM stay native.
                    var groups = WheelArrangement.IsArticulatedId(id)
                        ? wheels.Skip(1).Take(wheels.Length - 2).Select(w => w / 2).ToArray() : new[] { wheels[1] / 2 };
                    float mass = loco.massController.TotalMass;
                    float fraction = id == (int)WheelArrangementType.s222
                        ? ConventionalPhysics.DriverWeightFraction(groups, total, mass)
                        : ConventionalPhysics.TargetDriverWeightFraction(wheels, ConventionalPhysics.ReferenceOperatingMassKg, WheelArrangement.IsArticulatedId(id));
                    preliminaryDriverShare = fraction;
                    stage2Physics.Apply(sim.drivingForce.wheelRadius, powered, total, fraction);
                    Main.Logger.Log($"Driver weight: {fraction * mass / 1000f:F1} t on {powered.Length} driven axles " +
                        $"(equal share {(float)powered.Length / total * mass / 1000f:F1} t of {mass / 1000f:F0} t).");
                    currentWA = (WheelArrangementType)id;
                    if (WheelArrangement.IsArticulatedId(id)) articulatedSimulation.Apply(id);
                    if (powered.Length != 0 && id != (int)WheelArrangementType.s222)
                    {
                        var active = sim.simFlow.OrderedSimComps.OfType<LocoSim.Implementations.ReciprocatingSteamEngine>()
                            .Where(e => !(e is ArticulatedRearEngine aux) || (aux.Active && !aux.TenderMode)).ToArray();
                        cylinderCalibration.Apply(active, sim.drivingForce.wheelRadius, mass, fraction, powered.Length);
                        Main.Logger.Log($"Design cylinder calibration: area x{cylinderCalibration.AreaScale:F4}; " +
                            $"reference pull {cylinderCalibration.BeforeForceN / 1000:F1} -> {cylinderCalibration.AfterForceN / 1000:F1} kN; design ratio ceiling {cylinderCalibration.TargetRatio:F3} at 124 t rated mass.");
                    }
                    ApplySteamConnections(id);
                    RefreshConventionalRollingAudio();
                }
                if (id != 10) ApplyDriverQuartering(quarteringCycle, true);
                SelectPoweredTender(wantsTender);
                ConfigureIndependentDrive(quarteringCycle);
                UpdateExternalControlPositions();
                RestoreNativeLubricatorAnimation();
                ApplyHiddenNativeLinkages(id);
                ApplyOilCupMounts();
                B99Oilcups.Bind(loco, loco.loadedExternalInteractables);
                RefreshBigEndOiling(loco.loadedExternalInteractables);
                ArticulatedEffects.Refresh(loco, WheelArrangement.IsArticulatedId(id) || poweredTenderSelected);
                // Includes inactive conventional LODs and lazily created rear
                // engines. Exclude other integrations' separately owned effects.
                // Any proxy registered below must unregister before its owner
                // destroys it, including stock, reapply and pool-return paths.
                foreach (var retarget in GetComponentsInChildren<DriveRetarget>(true))
                    retarget.Retire = renderers => explosionMaterials?.Forget(renderers);
                explosionMaterials?.Refresh(GetComponentsInChildren<MeshRenderer>(true)
                    .Where(r => !DeltaTrailingTruckCompatibility.IsPresentation(r.transform) &&
                        !ArticulatedEffects.IsOwnedPresentation(r.transform)));
                GetComponent<LMSCustomizationPlacementMeshes>()?.ReloadAllGadgetMeshTransforms();
#if B99_DIAGNOSTICS
                LogStage2State("arrangement applied");
#endif
                return true;
            }
            catch (Exception ex)
            {
                LastConventionalError = ex.Message;
                Main.Logger.LogException("Conventional trial conversion failed; restoring stock", ex);
                try { if (stockGeometry != null) { RestoreConventionalStock(); LastSwitchRestoredStock = true; } }
                catch (Exception restore) { Main.Logger.LogException("Stock restoration failed", restore); }
                // An incomplete setup must never be repeated over partially generated geometry.
                conventionalFault = stockGeometry != null && preparedGeometry == null;
                return false;
            }
            finally { skipWheelArrangementChange = false; DeltaTrailingTruckCompatibility.Refresh(loco); }
        }

        private void RestoreConventionalStock(float? quarteringCycle = null)
        {
            float cycle = quarteringCycle ?? CaptureQuarteringCycle();
            ReleaseIndependentDrive();
            DetachTenderEngine();
            poweredTenderSelected = false;
            cylinderCalibration?.Restore();
            preliminaryDriverShare = float.NaN;
            SuspendHiddenNativeLinkages();
            SuspendSteamConnections();
            SuspendConnectingRodSpacing();
            RestoreCylinderCasings();
            ArticulatedEffects.Deinit(loco);
            articulatedSimulation?.Restore();
            SuspendArticulatedGeometry();
            SuspendAdditionalGeometry();
            Suspend4144Geometry();
            SuspendDriverLayout();
            stockGeometry.Restore();
            ApplyDriverQuartering(cycle, false);
            // Restore original parent ownership before deactivating generated frames.
            foreach (var t in generatedRoots) if (t != null) t.gameObject.SetActive(false);
            stage2Physics.Restore();
            RefreshConventionalRollingAudio();
            currentWA = WheelArrangementType.s282;
            UpdateExternalControlPositions();
            derailModifier = 0f;
        }

        // Native pooling can reuse this GameObject for a different locomotive.
        // Restore its per-car stock snapshot before native reset/pooling begins.
        internal bool ResetConventionalForPool()
        {
            try
            {
                loco = GetComponent<TrainCar>();
                ClearBigEndCups();
                DeltaTrailingTruckCompatibility.Clear(loco);
                if (stockGeometry != null) RestoreConventionalStock();
                currentWA = WheelArrangementType.s282;
                return true;
            }
            catch (Exception ex)
            {
                Main.Logger.LogException("Could not restore stock running gear before pooling this S282", ex);
                return false;
            }
        }

        private void OnDestroy()
        {
            SuspendFranklinDriverPins();
            MultiplayerSync.Forget(GetComponent<TrainCar>());
            ReleaseIndependentDrive();
            cylinderCalibration?.Restore();
            ClearBigEndCups();
            SuspendHiddenNativeLinkages();
            SuspendSteamConnections();
            DisposeConnectingRodSpacing();
            DisposeDriverLayouts();
            DisposeCylinderCasings();
            // The tender engine lives on another car: give the tender its bogies back.
            DetachTenderEngine();
            ArticulatedEffects.Deinit(loco);
            DisposeArticulatedGeometry();
            DisposePacificHudsonValveGear();
            DisposeAdditionalGeometry();
            // Unity may destroy the TrainCar component before this owner. Mesh
            // assets still need explicit cleanup even when the car compares null.
            if (loco == null) return;
            loco.ExternalInteractableLoaded -= LoadExternalInteractables;
            loco.ExternalInteractableAboutToBeUnloaded -= UnloadExternalInteractables;
        }

        // Restore only properties the legacy geometry path edits. Never rewind the
        // locomotive or bogie rigidbodies to their location at initialization.
        private sealed class GeometryState
        {
            private readonly List<Action> restore = new List<Action>();
            public GeometryState(TrainCar car)
            {
                CaptureFrontCouplingMounts(car);
                foreach (var t in car.GetComponentsInChildren<Transform>(true))
                {
                    if (DeltaTrailingTruckCompatibility.IsPresentation(t) || ArticulatedEffects.IsOwnedPresentation(t)) continue;
                    var p = t.localPosition; var q = t.localRotation; var s = t.localScale;
                    var parent = t.parent; int sibling = t.GetSiblingIndex();
                    var name = t.name; bool active = t.gameObject.activeSelf;
                    bool pose = t != car.transform && t.GetComponent<Rigidbody>() == null;
                    restore.Add(() => { if (t == null) return; t.name = name;
                        if (pose) {
                            if (t.parent != parent) t.SetParent(parent, false);
                            t.SetSiblingIndex(sibling);
                            t.localPosition = p; t.localRotation = q; t.localScale = s;
                        }
                        t.gameObject.SetActive(active); });
                }
                foreach (var f in car.GetComponentsInChildren<MeshFilter>(true))
                { if (DeltaTrailingTruckCompatibility.IsPresentation(f.transform) || ArticulatedEffects.IsOwnedPresentation(f.transform)) continue;
                    var mesh = f.sharedMesh; restore.Add(() => { if (f != null) f.sharedMesh = mesh; }); }
                foreach (var r in car.GetComponentsInChildren<Renderer>(true))
                { if (DeltaTrailingTruckCompatibility.IsPresentation(r.transform) || ArticulatedEffects.IsOwnedPresentation(r.transform)) continue;
                    bool enabled = r.enabled; restore.Add(() => { if (r != null) r.enabled = enabled; }); }
                foreach (var a in car.GetComponentsInChildren<Animator>(true))
                { if (DeltaTrailingTruckCompatibility.IsPresentation(a.transform) || ArticulatedEffects.IsOwnedPresentation(a.transform)) continue;
                    var controller = a.runtimeAnimatorController;
                    bool keepState = a.keepAnimatorControllerStateOnDisable;
                    restore.Add(() => { if (a != null) { a.runtimeAnimatorController = controller; a.keepAnimatorControllerStateOnDisable = keepState; } }); }
                foreach (var j in car.GetComponentsInChildren<Joint>(true))
                { if (DeltaTrailingTruckCompatibility.IsPresentation(j.transform)) continue;
                    var anchor = j.connectedAnchor; bool auto = j.autoConfigureConnectedAnchor;
                    restore.Add(() => { if (j != null) { j.autoConfigureConnectedAnchor = false; j.connectedAnchor = anchor; j.autoConfigureConnectedAnchor = auto; } }); }
                foreach (var b in car.Bogies)
                { var axles = b.Axles; restore.Add(() => b.axles = axles); }
                foreach (var wheel in car.GetComponentsInChildren<PoweredWheel>(true))
                {
                    var wheelTransform = wheel.wheelTransform; var axis = wheel.localRotationAxis;
                    restore.Add(() => { if (wheel != null) { wheel.wheelTransform = wheelTransform; wheel.localRotationAxis = axis; } });
                }
                // Native component-owned arrays replaced by the original setup.
                var fieldNames = new HashSet<string> { "transformsToRotate", "brakeRenderers", "wheelSparks", "sparkAnchors", "sparks" };
                foreach (var component in car.GetComponentsInChildren<MonoBehaviour>(true).Where(c => c != null))
                foreach (var field in component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (DeltaTrailingTruckCompatibility.IsPresentation(component.transform) || ArticulatedEffects.IsOwnedPresentation(component.transform)) continue;
                    if (!fieldNames.Contains(field.Name) || field.IsInitOnly) continue;
                    var value = field.GetValue(component);
                    restore.Add(() => { if (component != null) field.SetValue(component, value); });
                }
            }
            private void CaptureFrontCouplingMounts(TrainCar car)
            {
                // LMS MovePilot moves these two mount roots along with the pilot.
                // DV parents them under car.interior, a separate scene root, so
                // the car hierarchy snapshot below cannot restore them. Restore
                // only the mount positions, never native chain/hose IK or state.
                var visual = car.frontCoupler?.visualCoupler;
                foreach (var mount in new[] { visual?.chain, visual?.hoses })
                {
                    if (mount == null) continue;
                    var position = mount.localPosition;
                    restore.Add(() => { if (mount != null) mount.localPosition = position; });
                }
            }
            public void Restore() { foreach (var action in restore) action(); }
        }
    }
}
#endif
