#if B99_STAGE2 || B99_ALL_STANDARD
using System;
using System.Collections;
using System.Linq;
using DV.ECS.Train;
using DV.ThingTypes;
using DV.Wheels;
using LocoSim.Implementations;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace RearrangedS282
{
    // Explicitly invoked development test; never runs on load or writes a save.
    internal static class Stage2RuntimeChecks
    {
        internal static bool Running;
        internal static string Status = "Runtime smoke test has not run.";

        internal static void Begin()
        {
            if (Running) return;
#if B99_ALL_STANDARD && B99_DIAGNOSTICS
            if (PowerDiagnostics.Recording)
            {
                Status = "Stop power diagnostics before running the structural test.";
                return;
            }
#endif
            var cars = UnityEngine.Object.FindObjectsOfType<TrainCar>()
                .Where(c => S282Target.IsNormal(c) && !c.derailed && !c.isExploded &&
                    c.entity != null && c.entity.IsConverted && Mathf.Abs(c.GetForwardSpeed()) <= .01f &&
                    (c.GetComponent<WheelRearranger>() == null || c.GetComponent<WheelRearranger>().currentWA == WheelArrangementType.s282))
                .OrderBy(c => c.ID, StringComparer.Ordinal).ToArray();
            var readyCars = cars.Where(c => c.loadedExternalInteractables != null).ToArray();
            var subject = readyCars.FirstOrDefault(c => HasTestTender(c));
            var control = readyCars.FirstOrDefault(c => c != subject
#if B99_ALL_STANDARD
                && c.GetComponent<WheelRearranger>()?.PoweredTenderSelected != true
#endif
            );
            if (subject == null || control == null)
            {
                Status = "Need two stationary stock S282s with exterior interactions loaded. Stay near both locomotives; couple a regular S282 tender behind the test loco and keep the control unpowered. Close both regulators and use a disposable sandbox.";
                Main.Logger.Warning(Status);
                System.IO.File.WriteAllText(System.IO.Path.Combine(Main.ModPath,"stage2-runtime-checks.json"),
                    new JObject { ["passed"] = false, ["started"] = false, ["error"] = Status, ["eligibleCars"] = cars.Length, ["loadedEligibleCars"] = readyCars.Length }.ToString());
                return;
            }
            var runner = subject.GetComponent<WheelRearranger>() ?? subject.gameObject.AddComponent<WheelRearranger>();
            Running = true;
            Status = "Test running on " + subject.ID + " (control " + control.ID + "). Stay near both; close the mod manager so the native simulation can tick.";
#if B99_ALL_STANDARD
            runner.StartCoroutine(RunAll(runner, subject, control));
#else
            runner.StartCoroutine(Run(runner, subject, control));
#endif
        }

        private static bool HasTestTender(TrainCar car)
        {
            var tender = car?.rearCoupler?.coupledTo?.train;
            return tender != null && !tender.derailed && tender.carType == TrainCarType.Tender && CarTypes.IsTender(tender.carLivery);
        }

#if B99_ALL_STANDARD
        internal static string StructuralReadinessError(TrainCar subject, TrainCar control)
        {
            if (subject == null || subject.loadedExternalInteractables == null)
                return "Test locomotive " + (subject != null ? subject.ID : "(missing)") + " exterior interactions are not loaded. Stay near both locomotives and rerun the structural test.";
            if (control == null || control.loadedExternalInteractables == null)
                return "Control locomotive " + (control != null ? control.ID : "(missing)") + " exterior interactions are not loaded. Stay near both locomotives and rerun the structural test.";
            return null;
        }

        internal static bool AbortForReadiness(JObject report, TrainCar subject, TrainCar control)
        {
            string error = StructuralReadinessError(subject, control);
            if (error == null) return false;
            report["aborted"] = true;
            report["abortReason"] = "exterior-interactions-unloaded";
            report["error"] = error;
            return true;
        }

        internal static (int Id, bool Tender)[] StructuralCases() => Enumerable.Range(0, WheelArrangement.SupportedCount)
            .SelectMany(id => PoweredTenderPolicy.Supports(id) ? new[] { (id, false), (id, true) } : new[] { (id, false) }).ToArray();

        private static JObject CaptureTenderBogies(TrainCar car)
        {
            var tender = car.rearCoupler?.coupledTo?.train;
            return tender == null ? new JObject { ["missing"] = true } : new JObject {
                ["id"] = tender.ID,
                ["parts"] = new JArray(tender.Bogies.SelectMany(b => b.GetComponentsInChildren<Transform>(true))
                    .OrderBy(t => t.GetInstanceID()).Select(t => new JArray(t.GetInstanceID(), t.gameObject.activeSelf)))
            };
        }

        internal static string FramePiecePrefix(int id, string label)
        {
            // Little Boy reuses ApplyMalletGeometry, including its ID37 frame
            // cache. Its selected arrangement remains ID50; only this mesh tag
            // identifies the shared geometry rather than the selected layout.
            int geometryId = id == (int)WheelArrangementType.s4664LittleBoy ? (int)WheelArrangementType.s2662 : id;
            return "[Rearranged " + label + " frame " + geometryId + "] ";
        }

        private static JObject CapturePresentation(TrainCar car, WheelRearranger owner, int id)
        {
            int Pieces(string label) => car.GetComponentsInChildren<MeshFilter>(true)
                .Count(f => f.gameObject.activeInHierarchy && f.name.StartsWith(FramePiecePrefix(id, label), StringComparison.Ordinal) &&
                    f.sharedMesh != null && f.sharedMesh.triangles.Length != 0);
            return new JObject {
                ["arrangementId"] = (int)owner.currentWA,
                ["frontFramePieces"] = Pieces("front"), ["middleFramePieces"] = Pieces("middle"), ["rearFramePieces"] = Pieces("rear"),
                ["activeLocomotiveEngines"] = car.SimController.simFlow.OrderedSimComps.OfType<ReciprocatingSteamEngine>()
                    .Count(e => !(e is ArticulatedRearEngine aux) || aux.Active && !aux.TenderMode),
                ["tenderSelected"] = owner.PoweredTenderSelected,
                ["malletDriverPitch"] = owner.MalletDriverPitchReport(),
                ["franklinLinkages"] = owner.FranklinLinkageReport(),
                ["animationOwnership"] = owner.AnimationOwnershipReport(),
                ["independentDrive"] = car.GetComponent<IndependentDrive>()?.Report(),
                ["effectiveSettings"] = MultiplayerSync.SettingsFor(car) != null ? JObject.FromObject(MultiplayerSync.SettingsFor(car)) : null,
                ["franklinClearanceMeshes"] = owner.FranklinRearNoseClearanceCasings,
                ["tender"] = owner.TenderEngineReport(),
                ["hasSupportedTender"] = HasTestTender(car),
                ["compactFrameEndAllowanceM"] = id == 47 ? .10f : 0f,
                ["compactTriplexCrankPins"] = id != 47 ? null : new JArray(car.GetComponentsInChildren<MeshRenderer>(true)
                    .Where(r => r.name == "s282_mech_connect_to_cutoff_base" && r.gameObject.activeInHierarchy)
                    .Select(r => new JObject {
                        ["drive"] = r.transform.parent?.parent?.name,
                        ["enabled"] = r.enabled,
                        ["mesh"] = r.GetComponent<MeshFilter>()?.sharedMesh != null,
                        ["detailLod"] = car.GetComponentsInChildren<LODGroup>(true)
                            .Any(g => g.GetLODs().Length != 0 && g.GetLODs()[0].renderers.Contains(r))
                    })),
                ["pipeOverlapPolicy"] = "Small local fitting overlaps allowed; pipe socket gaps and presentation errors remain failures. Clipping appearance is a manual check."
            };
        }

        internal static void ValidatePresentation(int id, bool tender, JObject row)
        {
            if ((int?)row["arrangementId"] != id) throw new InvalidOperationException("Selected arrangement fell back or was replaced.");
            bool articulated = WheelArrangement.IsArticulatedId(id), triplex = ArticulatedSimulation.IsTriplexId(id);
            if ((int?)row["frontFramePieces"] != (articulated ? 2 : 0) ||
                (int?)row["middleFramePieces"] != (triplex ? 2 : 0) ||
                (int?)row["rearFramePieces"] != (articulated && id != 47 ? 2 : 0))
                throw new InvalidOperationException("Visible bar-frame pieces disagree with the selected engine groups.");
            if (id != 33 && (int?)row["activeLocomotiveEngines"] != (triplex ? 3 : articulated ? 2 : 1))
                throw new InvalidOperationException("Active locomotive engine count disagrees with the selected arrangement.");
            if ((bool?)row["tenderSelected"] != tender || tender && !PoweredTenderPolicy.Supports(id))
                throw new InvalidOperationException("Powered-tender selection disagrees with the structural test case.");
            var animation = row["animationOwnership"] as JObject;
            int expectedDrives = id == 33 ? 0 : 2 * (triplex ? 3 : articulated ? 2 : 1);
            var locoDrives = animation?["locomotive"] as JArray;
            var tenderDrives = animation?["tender"] as JArray;
            if (locoDrives == null || tenderDrives == null || locoDrives.Count != expectedDrives ||
                tenderDrives.Count != (tender ? 2 : 0) || locoDrives.Concat(tenderDrives).Any(d => (bool?)d["passed"] != true))
                throw new InvalidOperationException("Animation ownership is invalid; inspect active mirrors, Animator speed and phase ownership (stationary check only).");
            if (row["effectiveSettings"] is not JObject)
                throw new InvalidOperationException("Effective per-car settings were not captured.");
            if (articulated && ((bool?)row["independentDrive"]?["active"] != true ||
                (bool?)row["independentDrive"]?["ready"] != true || locoDrives.Any(d => (bool?)d["phaseOwned"] != true)))
                throw new InvalidOperationException("Independent wheelslip never acquired every active mechanism; native fallback is not independent-slip validation. Close UMM so simulation can tick and inspect the game log.");
            var state = row["tender"] as JObject;
            int franklinCount = id == 31 || id == 32 ? 4 : id == 38 || id == 40 || id == 47 ? 6 : 0;
            var linkages = row["franklinLinkages"] as JArray;
            if (linkages == null || linkages.Count != franklinCount || linkages.Any(link =>
                new[] { "mainRod", "crank" }.Any(part => new[] { "enabled", "mesh", "detailLod" }
                    .Any(field => (bool?)link[part]?[field] != true))))
                throw new InvalidOperationException("Franklin engines need visible main rods and crank pins in detail LODs.");
            if ((int?)row["franklinClearanceMeshes"] != (id == 32 ? 2 : 0))
                throw new InvalidOperationException("Franklin rear cylinder clearance treatment is missing or applied to the wrong arrangement.");
            int heldDrivers = WheelArrangement.IsArticulatedId(id)
                ? 2 * WheelArrangement.LocomotiveWheels(id).Skip(1).Take(WheelArrangement.LocomotiveWheels(id).Length - 2).Count(n => n == 6) : 0;
            var pitch = row["malletDriverPitch"] as JArray;
            if (pitch == null || pitch.Count != heldDrivers || pitch.Any(p => (bool?)p["passed"] != true))
                throw new InvalidOperationException("Mallet first driver position is not held at its coupling-rod pitch after animation.");
            if (tender && ((bool?)row["hasSupportedTender"] != true || (bool?)state?["attached"] != true ||
                (bool?)state?["sourceReady"] != true || ((int?)state?["sourceStaticCount"] ?? 0) <= 0 ||
                (bool?)state?["undercarriagePresentationReady"] != true ||
                (bool?)state?["underframeSteamConnection"]?["trailingSupportRegistered"] != true ||
                ((int?)state?["unexpectedFranklinAssemblies"] ?? -1) != 0 ||
                (bool?)state?["exhaustOutlet"] != true || (bool?)state?["visualEffects"] != true || (bool?)state?["audioEffects"] != true))
                throw new InvalidOperationException("Powered tender geometry, source cache, exhaust, particle or audio bindings are missing.");
            if (!tender && state != null) throw new InvalidOperationException("Ordinary-tender case retained a powered engine.");
            if (id == 47)
            {
                var pins = row["compactTriplexCrankPins"] as JArray;
                if (pins == null || pins.Count != 6 || pins.Any(p => (bool?)p["enabled"] != true ||
                    (bool?)p["mesh"] != true || (bool?)p["detailLod"] != true))
                    throw new InvalidOperationException("Compact triplex needs six visible, meshed crank pins in detail LODs, including both front pins.");
            }
        }

        internal static void ValidateOilCupPresentation(JObject row)
        {
            if ((bool?)row["ready"] != true)
                throw new InvalidOperationException("Oilcup presentation did not finish initializing.");
            if ((int?)row["uncoveredBigEnds"] != 0 || (bool?)row["coverageComplete"] != true)
                throw new InvalidOperationException("Driven-wheel bearings lack dedicated oiling points.");
            if ((bool?)row["countsMatch"] != true)
                throw new InvalidOperationException("Oilcup count or unique refill-port ownership disagrees with the active bearings.");
            if ((bool?)row["lidControlsBound"] != true)
                throw new InvalidOperationException("Visible oilcup lid has no native control bound to its own door port.");
            if ((float?)row["maximumProviderGapM"] > .002f)
                throw new InvalidOperationException("Visible oilcup has drifted away from its provider.");
            if ((float?)row["maximumGuideGapM"] > .035f)
                throw new InvalidOperationException("Fixed guide oilcup has no nearby guide mounting surface.");
            if ((float?)row["maximumCouplingSeatGapM"] > .002f)
                throw new InvalidOperationException("Oilcup is floating away from its coupling-rod bearing.");
            if ((bool?)row["nativeLubricatorAnimationBound"] != true)
                throw new InvalidOperationException("Native lubricator ratchet has no active animation reference.");
        }

        private static IEnumerator RunAll(WheelRearranger runner, TrainCar subject, TrainCar control)
        {
            var results = new JArray();
            var cases = StructuralCases();
            var tenderPreferences = Main.settings?.poweredTenderArrangements?.ToArray() ?? Array.Empty<int>();
            var report = new JObject { ["build"] = "articulated-trial", ["version"] = Main.RunningVersion, ["subject"] = subject.ID,
                ["control"] = control.ID, ["passed"] = false, ["started"] = true, ["expectedCases"] = cases.Length,
                ["supportedArrangements"] = WheelArrangement.SupportedCount, ["arrangements"] = results };
            report["localSettings"] = Main.settings != null ? JObject.FromObject(Main.settings) : null;
            report["motionVerified"] = false;
            report["clearanceVerified"] = false;
            report["tractiveReference"] = TractiveDiagnostics.ReferenceInfo();
            // This override is local to the subject and never writes settings or saves.
            bool prepared = false;
            try { if (!AbortForReadiness(report, subject, control)) prepared = runner.TrySwitchStructural(10, false); }
            catch (Exception ex) { report["error"] = ex.ToString(); }
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            JObject tenderBaseline = null;
            JObject before = null, controlBefore = null;
            try
            {
                if (AbortForReadiness(report, subject, control)) throw new InvalidOperationException((string)report["error"]);
                if (!prepared) throw new InvalidOperationException("Could not prepare unpowered stock reference.");
                tenderBaseline = CaptureTenderBogies(subject);
                before = Capture(subject); controlBefore = Capture(control);
            }
            catch (Exception ex) { report["error"] = ex.ToString(); }
            if (before != null && controlBefore != null)
            {
                report["before"] = before; report["controlBefore"] = controlBefore;
                foreach (var test in cases)
                {
                    if (AbortForReadiness(report, subject, control)) break;
                    int id = test.Id;
                    Status = "Structural test " + (results.Count + 1) + "/" + cases.Length + ": " + WheelArrangement.WheelArrangementNames[id] + (test.Tender ? " + powered tender" : " + ordinary tender");
                    var result = new JObject { ["id"] = id, ["name"] = WheelArrangement.WheelArrangementNames[id], ["poweredTender"] = test.Tender, ["passed"] = false };
                    results.Add(result);
                    if (runner.ConventionalSetupBlocked)
                    {
                        result["status"] = "blocked-by-initialization";
                        result["error"] = runner.LastConventionalError;
                        continue;
                    }
                    bool switched = false;
                    try { switched = runner.TrySwitchStructural(id, test.Tender); }
                    catch (Exception ex) { result["error"] = ex.ToString(); }
                    yield return new WaitForFixedUpdate();
                    yield return new WaitForFixedUpdate();
                    for (int frame = 0; frame < 120 && StructuralReadinessError(subject, control) == null && (!runner.BigEndCupsReady || test.Tender && runner.TenderEffectsRoot == null); frame++) yield return null;
                    yield return new WaitForEndOfFrame();
                    if (AbortForReadiness(report, subject, control))
                    {
                        result["status"] = "aborted-readiness";
                        result["error"] = (string)report["error"];
                        break; // Final stock restoration below still runs.
                    }
                    try
                    {
                        if (!switched) throw new InvalidOperationException(runner.LastConventionalError ?? "Conversion refused; check the game log.");
                        result["quartering"] = runner.QuarteringReport();
                        if ((bool)result["quartering"]["passed"] != true)
                            throw new InvalidOperationException("Left/right drivetrain quartering is incorrect; see quartering pairs and wheel angles.");
                        var changed = Capture(subject); result["converted"] = changed;
                        result["steamConnections"] = runner.SteamConnectionReport();
                        var connections = (JObject)result["steamConnections"];
                        ValidateSteamConnections(connections);
                        result["rearDriverFollowers"] = runner.RearDriverFollowerReport();
                        if ((bool)result["rearDriverFollowers"]["passed"] != true)
                            throw new InvalidOperationException("Rear five-axle engine driver followers are unbound or out of phase.");
                        result["physicsCalibration"] = runner.PreliminaryPhysicsReport();
                        if ((bool)result["physicsCalibration"]["dimensionsConsistent"] != true)
                            throw new InvalidOperationException("Cylinder bore, cached area/volume, stroke or count are inconsistent.");
                        result["measurements"] = TractiveDiagnostics.Capture(subject);
                        result["presentation"] = CapturePresentation(subject, runner, id);
                        ValidatePresentation(id, test.Tender, (JObject)result["presentation"]);
                        var calibration = (JObject)result["physicsCalibration"];
                        if ((double)calibration["referenceGripN"] > 0d &&
                            (double)result["measurements"]["referenceMeanForceN"] / (double)calibration["referenceGripN"] >
                            (double)calibration["designReferencePullGripCeiling"] * 1.002d)
                            throw new InvalidOperationException("Calibrated reference pull exceeds the rated design pull/grip ceiling.");
                        var cupPresentation = runner.CaptureOilCupPresentation();
                        result["oilcupPresentation"] = cupPresentation;
                        ValidateOilCupPresentation(cupPresentation);
                        if (!JToken.DeepEquals(before["oilcups"], changed["oilcups"]))
                            throw new InvalidOperationException("Native oilcup circuit or position binding changed.");
                        var wheels = WheelArrangement.LocomotiveWheels(id);
                        int total = wheels.Sum() / 2;
                        int powered = WheelArrangement.IsArticulatedId(id) ? wheels.Skip(1).Take(wheels.Length - 2).Sum() / 2 : wheels[1] / 2;
                        bool pitched = DriverLayout.Supports(id);
                        if ((int)changed["driverPitchPresentations"] != (pitched ? powered * 2 : 0))
                            throw new InvalidOperationException("Driver pitch presentation count disagrees with the selected arrangement.");
                        float expectedPitch = pitched ? DriverLayout.ClampExtraPitch(WheelRearranger.RequestedDriverPitchM(id), id) : 0f;
                        if (Mathf.Abs((float)changed["extraCoupledPitchM"] - expectedPitch) > .00001f)
                            throw new InvalidOperationException("Driver pitch does not match the selected setting.");
                        float expectedRearward = pitched ? WheelRearranger.ClampDriverGroupRearwardMetres(WheelRearranger.RequestedDriverRearwardM(id)) : 0f;
                        if ((bool)changed["connectingRodSpacingActive"] != pitched ||
                            Mathf.Abs((float)changed["driverGroupRearwardM"] - expectedRearward) > .00001f ||
                            (int)changed["precisionRodRetargets"] != (id == (int)WheelArrangementType.s262 || expectedRearward > 0f ? 2 : 0))
                            throw new InvalidOperationException("Main connecting-rod spacing does not match the selected setting.");
                        if ((int)changed["axles"] != total || (int)changed["entityAxles"] != total ||
                            (int)changed["powered"] != powered || (int)changed["poweredPort"] != powered)
                            throw new InvalidOperationException("Axle counts disagree with the selected arrangement.");
                        if ((bool)changed["articulated"] != WheelArrangement.IsArticulatedId(id))
                            throw new InvalidOperationException("Articulated frame or native constraints do not match the selected arrangement.");
                        if (powered == 0 && ((bool)changed["tractionEnabled"] ||
                            (bool)changed["engineBrakingReaderBound"] || subject.SimController.drivingForce.generatedForce != 0f))
                            throw new InvalidOperationException("The zero-driver novelty retained traction or engine braking.");
                        result["explosionMaterialRenderers"] = runner.ExplosionMaterialRendererCount;
                        if (id == (int)WheelArrangementType.s222)
                        {
                            result["prototypeDriverDiameterMetres"] = runner.GiantSingleDriverDiameter;
                            result["prototypeDriverColliderCount"] = runner.GiantSingleDriverColliderCount;
                            float expectedDiameter = 2.8f;
                            if (Mathf.Abs(runner.GiantSingleDriverDiameter - expectedDiameter) > .001f ||
                                runner.GiantSingleDriverColliderCount != 0 ||
                                Mathf.Abs(2f * subject.SimController.drivingForce.wheelRadius - expectedDiameter) > .001f)
                                throw new InvalidOperationException("Clearance prototype radius or collider-free geometry is inconsistent.");
                        }
                        if (id != 10 && runner.ExplosionMaterialRendererCount == 0)
                            throw new InvalidOperationException("Added running gear has no native explosion material registration.");
                        foreach (int index in new[] { 0, 2, 3, 4 })
                            if (!JToken.DeepEquals(before["steam"][index], changed["steam"][index]))
                                throw new InvalidOperationException("Cylinder count, stroke or cutoff limits changed.");
                        if (!JToken.DeepEquals(before["type"], changed["type"]) || !JToken.DeepEquals(before["mass"], changed["mass"]))
                            throw new InvalidOperationException("Shared type or base mass changed.");
                        if (!JToken.DeepEquals(controlBefore, Capture(control))) throw new InvalidOperationException("Control locomotive changed.");
                        if (!runner.TrySwitchStructural(10, false)) throw new InvalidOperationException("Return to stock failed.");
                        result["passed"] = true;
                    }
                    catch (Exception ex) { result["error"] = ex.ToString(); }
                    // Always attempt stock restoration, including after a failed conversion.
                    try { runner.TrySwitchStructural(10, false); }
                    catch (Exception ex) { result["error"] = ex.ToString(); result["passed"] = false; }
                    yield return new WaitForFixedUpdate();
                    yield return new WaitForFixedUpdate();
                    try
                    {
                        result["restored"] = Capture(subject);
                        result["tenderRestored"] = CaptureTenderBogies(subject);
                        if (!JToken.DeepEquals(before, result["restored"]) || !JToken.DeepEquals(controlBefore, Capture(control)) ||
                            !JToken.DeepEquals(tenderBaseline, result["tenderRestored"]) || runner.PoweredTenderSelected || runner.TenderEffectsRoot != null)
                            throw new InvalidOperationException("Stock restoration or second-car isolation failed.");
                    }
                    catch (Exception ex) { result["error"] = ex.ToString(); result["passed"] = false; }
                    // Save progress after every arrangement so an interrupted run has evidence.
                    System.IO.File.WriteAllText(System.IO.Path.Combine(Main.ModPath, "stage2-runtime-checks.json"), report.ToString());
                }
                var returns = new JArray(); report["returnToErie"] = returns;
                foreach (int id in (bool?)report["aborted"] == true ? Array.Empty<int>() : new[] { 43, 46, 47, 43, 0, 43 })
                {
                    if (AbortForReadiness(report, subject, control)) break;
                    Status = "Structural return-to-Erie check: " + WheelArrangement.WheelArrangementNames[id];
                    bool tender = id == 43, switched = false;
                    var row = new JObject { ["id"] = id, ["poweredTender"] = tender, ["passed"] = false }; returns.Add(row);
                    try { switched = runner.TrySwitchStructural(id, tender); }
                    catch (Exception ex) { row["error"] = ex.ToString(); }
                    yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
                    for (int frame = 0; frame < 120 && StructuralReadinessError(subject, control) == null && (!runner.BigEndCupsReady || tender && runner.TenderEffectsRoot == null); frame++) yield return null;
                    yield return new WaitForEndOfFrame();
                    if (AbortForReadiness(report, subject, control)) { row["error"] = (string)report["error"]; break; }
                    try
                    {
                        if (!switched) throw new InvalidOperationException(runner.LastConventionalError ?? "Repeated conversion refused.");
                        row["presentation"] = CapturePresentation(subject, runner, id);
                        ValidatePresentation(id, tender, (JObject)row["presentation"]);
                        ValidateSteamConnections(runner.SteamConnectionReport());
                        if (!JToken.DeepEquals(controlBefore, Capture(control))) throw new InvalidOperationException("Repeated switching changed the control locomotive.");
                        row["passed"] = true;
                    }
                    catch (Exception ex) { row["error"] = ex.ToString(); }
                }
                try { if (!runner.TrySwitchStructural(10, false)) throw new InvalidOperationException("Final stock restoration refused."); }
                catch (Exception ex) { report["restoreError"] = ex.ToString(); }
                yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
                bool restored = false;
                try { restored = JToken.DeepEquals(before, Capture(subject)) && JToken.DeepEquals(controlBefore, Capture(control)) && JToken.DeepEquals(tenderBaseline, CaptureTenderBogies(subject)); }
                catch (Exception ex) { report["restoreError"] = ex.ToString(); }
                report["finalStockRestored"] = restored;
                bool preferencesUnchanged = tenderPreferences.SequenceEqual(Main.settings?.poweredTenderArrangements ?? Array.Empty<int>());
                report["tenderPreferencesUnchanged"] = preferencesUnchanged;
                report["passed"] = (bool?)report["aborted"] != true && results.Count == cases.Length && results.All(r => (bool)r["passed"]) && returns.All(r => (bool)r["passed"]) && restored && preferencesUnchanged && report["restoreError"] == null;
            }
            report["limitations"] = "Stationary structural checks only. Driving, loaded pull, slip onset, rods, audio, particles and distant/unload behaviour need manual testing.";
            System.IO.File.WriteAllText(System.IO.Path.Combine(Main.ModPath, "stage2-runtime-checks.json"), report.ToString());
            Running = false;
            Status = (bool)report["passed"] ? "All " + cases.Length + " stationary structural cases and repeated Erie checks passed. Motion and crank clearance remain untested. See stage2-runtime-checks.json."
                : (bool?)report["aborted"] == true ? "Structural test stopped: " + (string)report["error"] : "Structural checks failed. See stage2-runtime-checks.json and the game log.";
            Main.Logger.Log(Status);
        }
#endif

        private static IEnumerator Run(WheelRearranger runner, TrainCar subject, TrainCar control)
        {
            var report = new JObject { ["subject"] = subject.ID, ["control"] = control.ID, ["passed"] = false };
            JObject before;
            JObject controlBefore;
            try
            {
                before = Capture(subject);
                controlBefore = Capture(control);
            }
            catch (Exception ex)
            {
                Status = "Could not capture runtime baseline: " + ex;
                Main.Logger.Log(Status);
                Running = false;
                System.IO.File.WriteAllText(System.IO.Path.Combine(Main.ModPath,"stage2-runtime-checks.json"),
                    new JObject { ["passed"] = false, ["error"] = Status }.ToString());
                yield break;
            }
            report["before"] = before;
            report["controlBefore"] = controlBefore;
            string failure = null;
            bool converted = false;
            try { converted = runner.TrySwitchStage2((int)WheelArrangementType.s284); }
            catch (Exception ex) { failure = ex.ToString(); }
            if (!converted) failure = failure ?? "Conversion refused; check regulators, driving force and the mod log.";
            if (failure == null)
            {
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                try
                {
                    var changed = Capture(subject);
                    report["berkshire"] = changed;
                    if ((int)changed["axles"] != 7 || (int)changed["entityAxles"] != 7 ||
                        (int)changed["powered"] != 4 || (int)changed["poweredPort"] != 4)
                        throw new InvalidOperationException("Converted car/manager/port/entity axle counts disagree.");
                    if (!JToken.DeepEquals(controlBefore, Capture(control)))
                        throw new InvalidOperationException("The second locomotive's configuration changed.");
                    if (!JToken.DeepEquals(before["steam"],changed["steam"]) || !JToken.DeepEquals(before["type"],changed["type"]))
                        throw new InvalidOperationException("Steam engine or shared car type changed.");
                    if (!runner.TrySwitchStage2((int)WheelArrangementType.s282))
                        throw new InvalidOperationException("Return to stock was refused.");
                }
                catch (Exception ex) { failure = ex.ToString(); }
            }
            if (failure == null)
            {
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                try
                {
                    var restored = Capture(subject);
                    report["restored"] = restored;
                    report["controlAfter"] = Capture(control);
                    if (!JToken.DeepEquals(before, restored)) throw new InvalidOperationException("Stock configuration was not restored exactly.");
                    if (!JToken.DeepEquals(controlBefore, report["controlAfter"])) throw new InvalidOperationException("Control locomotive changed.");
                    // Repeated same-frame transitions test duplicate prevention and drift.
                    for (int i = 0; i < 10; i++)
                    {
                        if (!runner.TrySwitchStage2(11) || !runner.TrySwitchStage2(10))
                            throw new InvalidOperationException("Repeated transition failed.");
                    }
                    if (!JToken.DeepEquals(before, Capture(subject))) throw new InvalidOperationException("Repeated switching accumulated changes.");
                    if (!JToken.DeepEquals(controlBefore, Capture(control))) throw new InvalidOperationException("Repeated switching changed the control car.");
                    report["passed"] = true;
                }
                catch (Exception ex) { failure = ex.ToString(); }
            }
            try
            {
                if (runner.currentWA != WheelArrangementType.s282) runner.TrySwitchStage2(10);
                if (failure != null) report["error"] = failure;
                report["limitations"] = "Structural runtime regression only: no loaded pull, wheelslip onset, audio or visual inspection measured.";
                var path = System.IO.Path.Combine(Main.ModPath, "stage2-runtime-checks.json");
                System.IO.File.WriteAllText(path, report.ToString());
                Status = failure == null ? "Runtime structural checks passed. See stage2-runtime-checks.json." : "Runtime checks failed: " + failure;
                Main.Logger.Log(Status);
            }
            finally { Running = false; }
        }

        private static JObject Capture(TrainCar car)
        {
            var sim = car.SimController;
            var shared = car.entity.GetComponentData<AdhesionControllerSystem.StaticSharedAdhesionCalculationData>();
            var friction = car.entity.GetComponentData<AdhesionControllerSystem.StaticAdhesionCalculationData>();
            var engines = sim.simFlow.OrderedSimComps.OfType<ReciprocatingSteamEngine>().ToArray();
            var engine = engines.First();
            var rotations = car.GetComponentsInChildren<PoweredWheelRotationViaAnimation>(true);
            return new JObject
            {
                ["axles"] = car.NumberOfAxles, ["entityAxles"] = shared.numOfAxles,
                ["bogies"] = new JArray(car.Bogies.Select(b => new JArray(b.Axles.Select(a => a.transform.GetInstanceID())))),
                ["powered"] = sim.poweredWheels.poweredWheels.Length, ["poweredPort"] = sim.wheelslipController.NumberOfPoweredAxles,
                ["poweredReferences"] = new JArray(sim.poweredWheels.poweredWheels.Select(w => w.GetInstanceID())),
                ["poweredStates"] = new JArray(sim.poweredWheels.poweredWheels.Select(w => (int)w.state)),
                ["forceRadius"] = sim.drivingForce.wheelRadius,
                ["tractionEnabled"] = sim.drivingForce.enabled,
                ["engineBrakingReaderBound"] = sim.wheelslipController.engineBrakingActivePort != null,
                ["rpmConstant"] = sim.tractionPortsFeeder.speedMsToWheelRpmConst,
                ["speedConstant"] = sim.tractionPortsFeeder.wheelRpmToSpeedKmhConst,
                ["animationRadii"] = new JArray(rotations.Select(r => r.wheelRadius)),
                ["circumferences"] = new JArray(rotations.Select(r => r.wheelCircumference)),
                ["slipCoefficient"] = friction.wheelslipFrictionCoef, ["slideCoefficient"] = friction.wheelSlideFrictionCoef,
                ["mass"] = car.massController.CarMass,
#if B99_ALL_STANDARD
                ["oilcups"] = CaptureOilcups(car),
                ["frontCouplingMounts"] = CaptureFrontCouplingMounts(car),
                ["cylinderCasingMeshes"] = car.GetComponent<WheelRearranger>()?.CylinderCasingMeshCount ?? 0,
                ["driverPitchPresentations"] = car.GetComponent<WheelRearranger>()?.DriverLayoutPresentationCount ?? 0,
                ["extraCoupledPitchM"] = car.GetComponent<WheelRearranger>()?.DriverLayoutExtraPitchM ?? 0f,
                ["connectingRodSpacingActive"] = car.GetComponent<WheelRearranger>()?.ConnectingRodSpacingActive == true,
                ["driverGroupRearwardM"] = car.GetComponent<WheelRearranger>()?.DriverGroupRearwardM ?? 0f,
                ["precisionRodRetargets"] = car.GetComponent<WheelRearranger>()?.ConnectingRodSpacingRetargetCount ?? 0,
                ["articulated"] = car.GetComponent<WheelRearranger>()?.ArticulatedGeometryActive == true,
                ["engines"] = new JArray(engines.Select(e => new JObject {
                    ["eventPort"] = e.chuffEventReadOut.id, ["cylinders"] = e.numCylinders,
                    ["bore"] = e.cylinderBore, ["stroke"] = e.pistonStroke,
                    ["area"] = e.pistonArea, ["volumeLitres"] = e.cylinderVolume
                })),
#endif
                ["steam"] = new JArray(engine.numCylinders, engine.cylinderBore, engine.pistonStroke, engine.minCutoff, engine.maxCutoff),
                ["type"] = new JArray(car.carLivery.parentType.GetInstanceID(), car.carLivery.parentType.wheelRadius,
                    car.carLivery.parentType.mass, car.carLivery.parentType.WheelslipFrictionCoef)
            };
        }
#if B99_ALL_STANDARD
        internal static void ValidateSteamConnections(JObject connections)
        {
            float gap = (float)connections["maximumEndpointGapM"];
            // Newtonsoft distinguishes a string-valued null from JTokenType.Null,
            // although both serialize as null. Test the error value, not its type.
            if ((int)connections["expectedCylinders"] != (int)connections["connectedCylinders"] ||
                (int)connections["colliders"] != 0 || float.IsNaN(gap) || float.IsInfinity(gap) || gap < 0f || gap > .003f ||
                !string.IsNullOrEmpty((string)connections["error"]))
                throw new InvalidOperationException("Steam-pipe sockets, endpoint gaps or collider-free presentation are inconsistent: " + connections);
        }

        private static JObject CaptureFrontCouplingMounts(TrainCar car)
        {
            var visual = car.frontCoupler?.visualCoupler;
            JToken Position(Transform mount) => mount == null ? JValue.CreateNull()
                : new JArray(mount.localPosition.x, mount.localPosition.y, mount.localPosition.z);
            return new JObject { ["chain"] = Position(visual?.chain), ["hoses"] = Position(visual?.hoses) };
        }

        private static JObject CaptureOilcups(TrainCar car)
        {
            var providers = car.GetComponent<DV.Util.PositionSyncProviderController>()?.entries;
            var external = car.loadedExternalInteractables;
            if (external == null) return new JObject { ["externalLoaded"] = false };
            // The native six only; big-end cups added by this mod are checked separately.
            var all = external.GetComponent<DV.Simulation.Controllers.OilingPointsPortController>()?.entries;
            var cups = all?.Where(c => c != null && !WheelRearranger.IsBigEndCup(c.transform)).ToArray();
            if (providers == null || cups == null || cups.Length != 6)
                throw new InvalidOperationException("Native six-cup oiling setup is missing.");
            return new JObject { ["externalLoaded"] = true, ["cups"] = new JArray(cups.Select(c => {
                var consumer = B99Oilcups.ConsumerFor(c);
                if (consumer == null || providers.Count(p => p != null && p.syncTag == consumer.syncTag && p.transform == consumer.providerTransform) != 1)
                    throw new InvalidOperationException("Oilcup has lost its unique native position binding.");
                return new JObject { ["id"] = c.GetInstanceID(), ["tag"] = consumer.syncTag,
                    ["provider"] = consumer.providerTransform.GetInstanceID(),
                    ["refillPort"] = c.refillPortId, ["flowPort"] = c.refillingFlowNormalizedPortId };
            })) };
        }
#endif
    }
}
#endif
