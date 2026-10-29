#if B99_ALL_STANDARD
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DV.ECS.Train;
using DV.Simulation.Cars;
using LocoSim.Implementations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RearrangedS282
{
    // Read-only observations. Kept OUTSIDE Stage2's exact restoration/isolation
    // snapshots: fuel, temperature and pressure legitimately change between ticks.
    internal static class TractiveDiagnostics
    {
        internal static JObject ReferenceInfo() => new JObject {
            ["schema"] = 1, ["pressureBarAbs"] = TractiveEffortModel.ReferencePressureBarAbs,
            ["cutoff"] = TractiveEffortModel.ReferenceCutoff, ["temperatureC"] = TractiveEffortModel.ReferenceTemperatureC,
            ["meaning"] = "Common native mean-force comparison, not observed starting pull. Uses each active engine's cached piston area, stroke and current wheel radius; excludes dormant and tender engines from loco totals.",
            ["grip"] = "Dry reference grip uses captured TotalMass / physical axles * 9.81 * powered axles * read-back slip coefficient. ECS live grip includes rail/sand conditions; controller limit can also include the slip drop and can be stale at rest.",
            ["limitations"] = "No tonnage prediction or sustained-grade rating. Structural runs keep regulators closed. Low-RPM native torque is instantaneous. Engines/ports/mass are sampled at different native update phases; no simulation is advanced or modified.",
            ["missingValues"] = "null is unavailable/non-finite; zero is a measured or mathematically known zero",
            ["resourceUnits"] = "Resource-container amount/capacity retain native units for the named resource; aggregate mass is kg. Steam pressures are bar absolute.",
            ["sampledMeanMeaning"] = "Theoretical mean-force magnitude at sampled steam conditions after cock/crack losses, before native torque smoothing and drivetrain-health reduction. Neutral reverser gives zero. Observed signed force is recorded separately."
        };

        internal static JObject Capture(TrainCar car)
        {
            try { return CaptureCore(car); }
            catch (Exception error) { return new JObject { ["schema"] = 1, ["error"] = error.GetType().Name + ": " + error.Message }; }
        }

        private static JObject CaptureCore(TrainCar car)
        {
            var sim = car?.SimController;
            if (sim?.simFlow == null) return new JObject { ["schema"] = 1, ["error"] = "Simulation unavailable" };
            var owner = car.GetComponent<WheelRearranger>();
            int id = owner != null ? (int)owner.currentWA : (int)WheelArrangementType.s282;
            int? powered = sim.wheelslipController != null ? sim.wheelslipController.NumberOfPoweredAxles : (int?)null;
            var mass = car.massController;
            var result = new JObject {
                ["schema"] = 1, ["car"] = car.ID, ["arrangementId"] = id,
                ["experimental"] = id == (int)WheelArrangementType.s222,
                ["driverRadiusM"] = N(sim.drivingForce?.wheelRadius),
                ["totalAxles"] = car.NumberOfAxles, ["poweredAxles"] = powered,
                ["totalMassKg"] = N(mass?.TotalMass), ["baseMassKg"] = N(mass?.CarMass),
                ["cargoMassKg"] = N(mass?.CargoMass), ["resourcesMassKg"] = N(mass?.ResourcesMass),
                ["nativeWeightPerAxleN"] = N(mass?.WeightPerAxle),
                ["observedGeneratedForceN"] = N(sim.drivingForce?.generatedForce),
                ["controllerForceLimitN"] = N(sim.wheelslipController?.TotalForceLimit),
                ["slip"] = N(sim.wheelslipController?.wheelslip), ["sandCoefficient"] = N(sim.wheelslipController?.SandCoef)
            };
            double? coefficient = null, livePerAxle = null;
            try
            {
                if (car.entity != null && car.entity.IsConverted)
                {
                    coefficient = car.entity.GetComponentData<AdhesionControllerSystem.StaticAdhesionCalculationData>().wheelslipFrictionCoef;
                    livePerAxle = car.entity.GetComponentData<AdhesionControllerSystem.AdhesionWheelslipOutputData>().wheelslipForceLimitPerAxle;
                    result["adhesionEcsEligible"] = car.entity.TryGetComponentData<TrainCarPositionMonitorSystem.IsMovingTag>().IsSome() ||
                        car.entity.TryGetComponentData<AdhesionControllerSystem.IsWheelSlidingTag>().IsSome() ||
                        car.entity.TryGetComponentData<WheelslipControllerSystem.IsWheelslippingTag>().IsSome();
                }
            }
            catch (Exception error) { result["adhesionReadError"] = error.GetType().Name + ": " + error.Message; }
            double? dryGrip = mass != null && powered.HasValue && coefficient.HasValue
                ? TractiveEffortModel.DryGripN(mass.TotalMass, car.NumberOfAxles, powered.Value, coefficient.Value) : null;
            result["slipFrictionCoefficient"] = N(coefficient);
            result["liveGripPerAxleN"] = N(livePerAxle);
            result["liveGripTotalN"] = N(livePerAxle.HasValue && powered.HasValue ? livePerAxle * powered.Value : null);
            result["referenceDryGripN"] = N(dryGrip);
            var components = sim.simFlow.OrderedSimComps;
            var boiler = components.OfType<Boiler>().FirstOrDefault();
            result["boilerPressureBarAbs"] = N(boiler?.pressureReadOut?.Value);
            result["boilerWaterMassKg"] = N(boiler?.waterMassReadOut?.Value);
            result["preliminaryPhysics"] = owner?.PreliminaryPhysicsReport();
            result["rearDriverFollowers"] = owner?.RearDriverFollowerReport();
            result["resources"] = new JArray((sim.resourceContainerController?.resourceContainers ?? Enumerable.Empty<ResourceContainer>())
                .Select(r => new JObject { ["id"] = r.id, ["type"] = r.resourceType.ToString(),
                    ["amountNativeUnits"] = N(r.amountReadOut?.Value), ["capacityNativeUnits"] = N(r.capacity) }));
            var tender = owner?.TenderEngineReport();
            result["tender"] = tender;
            result["independentDrive"] = car.GetComponent<IndependentDrive>()?.Report();
            var engines = new JArray();
            double reference = 0d, liveMean = 0d, tenderReference = 0d;
            bool referenceKnown = true, liveKnown = true, tenderKnown = true;
            foreach (var engine in components.OfType<ReciprocatingSteamEngine>())
            {
                var auxiliary = engine as ArticulatedRearEngine;
                bool active = auxiliary == null || auxiliary.Active;
                bool tenderMode = auxiliary != null && auxiliary.TenderMode;
                bool included = active && powered > 0 && !tenderMode;
                double? radius = tenderMode ? (double?)tender?["driverRadiusM"] : sim.drivingForce?.wheelRadius;
                double? refForce = radius.HasValue ? TractiveEffortModel.MeanForceN(engine.numCylinders, engine.pistonArea,
                    engine.pistonStroke, radius.Value, TractiveEffortModel.ReferencePressureBarAbs,
                    TractiveEffortModel.ReferenceCutoff, TractiveEffortModel.ReferenceTemperatureC) : null;
                // Match the native admission-pressure losses from cocks/cracks.
                double pressure = engine.admissionPressure, gauge = Math.Max(0d, pressure - 1d);
                double modified = Math.Max(1d, pressure - .1d * Math.Max(0d, Math.Min(1d, engine.cylinderCockControl.Value)) * gauge -
                    (engine.isCylinderCrackedReadOut.Value > 0f ? .5d * gauge : 0d));
                double? mean = radius.HasValue ? TractiveEffortModel.MeanForceN(engine.numCylinders, engine.pistonArea,
                    engine.pistonStroke, radius.Value, modified, engine.cutoff, engine.intakeTemperature.Value) : null;
                if (mean.HasValue && engine.reverserControl.Value == 0f) mean = 0d;
                if (included) { if (refForce.HasValue) reference += refForce.Value; else referenceKnown = false;
                    if (mean.HasValue) liveMean += mean.Value; else liveKnown = false; }
                if (active && tenderMode) { if (refForce.HasValue) tenderReference += refForce.Value; else tenderKnown = false; }
                engines.Add(new JObject { ["id"] = engine.id, ["eventPort"] = engine.chuffEventReadOut?.id,
                    ["active"] = active, ["tractionGroup"] = tenderMode ? "tender" : "locomotive", ["includedInLocomotivePull"] = included,
                    ["cylinders"] = engine.numCylinders, ["boreM"] = N(engine.cylinderBore), ["strokeM"] = N(engine.pistonStroke),
                    ["cachedPistonAreaM2"] = N(engine.pistonArea), ["cachedCylinderVolumeLitres"] = N(engine.cylinderVolume),
                    ["cutoff"] = N(engine.cutoff), ["minCutoff"] = N(engine.minCutoff), ["maxCutoff"] = N(engine.maxCutoff),
                    ["regulator"] = N(engine.throttleControl.Value), ["reverser"] = N(engine.reverserControl.Value),
                    ["admissionPressureBarAbs"] = N(pressure), ["modifiedAdmissionPressureBarAbs"] = N(modified),
                    ["chestPressureBarAbs"] = N(engine.steamChestPressureReadOut?.Value), ["intakeTemperatureC"] = N(engine.intakeTemperature.Value),
                    ["crankRpm"] = N(auxiliary != null ? auxiliary.EffectiveCrankRpm : engine.crankRpm.Value), ["instantaneousTorqueNm"] = N(tenderMode ? auxiliary.TenderTorque : engine.torqueOut?.Value),
                    ["referenceMeanForceN"] = N(active ? refForce : null), ["meanForceAtSampledSteamN"] = N(active ? mean : null)
                });
            }
            result["engines"] = engines;
            bool anyLocomotive = engines.Any(e => (bool)e["includedInLocomotivePull"]);
            referenceKnown &= powered == 0 || (powered > 0 && anyLocomotive);
            liveKnown &= powered == 0 || (powered > 0 && anyLocomotive);
            result["referenceMeanForceN"] = N(referenceKnown ? reference : (double?)null);
            result["meanForceAtSampledSteamN"] = N(liveKnown ? liveMean : (double?)null);
            result["referencePullGripRatio"] = N(referenceKnown && dryGrip > 0d ? reference / dryGrip.Value : (double?)null);
            result["referenceAvailableForceN"] = N(referenceKnown && dryGrip.HasValue ? Math.Min(reference, dryGrip.Value) : (double?)null);
            // Never silently add the Erie's separate tender force or grip to loco totals.
            if (tender != null) tender["referenceMeanForceN"] = N(tenderKnown ? tenderReference : (double?)null);
            return result;
        }

        internal static string WriteStructuralCsv(JObject report, string path)
        {
            try
            {
                using (var output = new StreamWriter(path, false, new UTF8Encoding(false)))
                {
                    string[] columns = { "driverRadiusM", "poweredAxles", "totalAxles", "totalMassKg", "baseMassKg", "resourcesMassKg",
                        "boilerPressureBarAbs", "liveGripPerAxleN", "liveGripTotalN", "controllerForceLimitN", "referenceDryGripN",
                        "referenceMeanForceN", "referenceAvailableForceN", "referencePullGripRatio", "meanForceAtSampledSteamN", "observedGeneratedForceN", "experimental", "engines", "tender", "error" };
                    output.WriteLine("arrangementId,arrangement,structuralPassed," + string.Join(",", columns));
                    foreach (var row in (JArray)report["arrangements"])
                    {
                        var measurement = row["measurements"] as JObject;
                        output.WriteLine(string.Join(",", new[] { Csv(row["id"]), Csv(row["name"]), Csv(row["passed"]) }
                            .Concat(columns.Select(c => Csv(measurement?[c])))));
                    }
                }
                return null;
            }
            catch (Exception error) { return error.GetType().Name + ": " + error.Message; }
        }

        private static string Csv(JToken value)
        {
            if (value == null || value.Type == JTokenType.Null) return "";
            string text = value is JValue scalar ? Convert.ToString(scalar.Value, CultureInfo.InvariantCulture) : value.ToString(Formatting.None);
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }
        private static JToken N(double? value) => value.HasValue && TractiveEffortModel.Finite(value.Value) ? new JValue(value.Value) : JValue.CreateNull();
    }
}
#endif
