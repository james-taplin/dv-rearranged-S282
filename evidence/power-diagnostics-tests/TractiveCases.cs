using System;
using System.Globalization;
using System.IO;
using System.Linq;
using LocoSim.Implementations;
using Newtonsoft.Json.Linq;
using RearrangedS282;

internal static partial class Program
{
    private static void TractiveCases()
    {
        Near((float)TractiveEffortModel.MeanPressureRatio(1, 300), 1f, "Full-cutoff pressure ratio is unity");
        Near((float)TractiveEffortModel.MeanPressureRatio(.5, 300), (float)(.5 * (1 + Math.Log(2))), "Hot non-condensing expansion includes cutoff");
        Near((float)TractiveEffortModel.MeanPressureRatio(.5, 106.85), (float)(.5 * (1 + .5 * Math.Log(2))), "Native condensation branch includes second cutoff factor");
        Near((float)TractiveEffortModel.MeanForceN(2, .01, .5, .5, 15, 1, 300), (float)(28000 / Math.PI), "Mean torque to wheel force preserves bar/area/stroke/radius units");
        Check(TractiveEffortModel.MeanForceN(2, .01, .5, .5, 15, 0, 300) == 0, "Zero cutoff has a defined zero-force limit");
        Check(TractiveEffortModel.MeanForceN(2, .01, .5, .5, 1, 1, 300) == 0, "Atmospheric pressure produces no positive work");
        Check(TractiveEffortModel.MeanForceN(2, .01, .5, 0, 15, 1, 300) == null &&
            TractiveEffortModel.MeanPressureRatio(double.NaN, 300) == null &&
            TractiveEffortModel.MeanPressureRatio(2, 300) == null, "Invalid geometry/cutoff is unavailable, never fabricated zero");
        Near((float)TractiveEffortModel.DryGripN(111000, 6, 4, .24), 174225.6f, "Dry reference uses total mass and physical/powered axle ratio");
        Check(TractiveEffortModel.DryGripN(111000, 6, 0, .24) == 0 &&
            TractiveEffortModel.DryGripN(111000, 0, 0, .24) == null, "Zero drivers differs from missing axle information");

        NewCase();
        var c = Car("MEASURE"); c.sim.train.Rearranger = new WheelRearranger { currentWA = WheelArrangementType.s2662 };
        c.sim.simFlow.OrderedSimComps.Add(new Boiler());
        c.sim.resourceContainerController.resourceContainers.Add(new ResourceContainer());
        c.engine.admissionPressure = 12; c.engine.intakeTemperature.Value = 300; c.engine.cutoff = .5f;
        c.engine.cylinderCockControl.Value = .5f;
        var rear = new ArticulatedRearEngine { id = "rear", Active = true, pistonArea = c.engine.pistonArea * .5f };
        var dormant = new ArticulatedRearEngine { id = "middle", Active = false, pistonArea = c.engine.pistonArea * 10f };
        c.sim.simFlow.OrderedSimComps.Add(rear); c.sim.simFlow.OrderedSimComps.Add(dormant);
        JObject row = TractiveDiagnostics.Capture(c.sim.train);
        Check(row["error"] == null, "Complete live measurement captured");
        Check(row.Value<bool>("adhesionEcsEligible") == false, "Stationary ECS ineligibility disclosed without fabricating fresh grip");
        c.sim.train.entity.Eligible = true;
        Check(TractiveDiagnostics.Capture(c.sim.train).Value<bool>("adhesionEcsEligible"), "Moving/slip ECS eligibility captured");
        c.sim.train.entity.Eligible = false;
        Near(row.Value<float>("totalMassKg"), 111000, "Total mass is separate from base mass");
        Near(row.Value<float>("baseMassKg"), 105000, "Base mass is captured without substituting total mass");
        Near(row.Value<float>("resourcesMassKg"), 6000, "Resource mass retained");
        Near(row["resources"][0].Value<float>("amountNativeUnits"), 6000, "Resource native quantity retained");
        Near(row.Value<float>("boilerPressureBarAbs"), 15, "Native boiler pressure retained");
        Near(row.Value<float>("liveGripTotalN"), 160000, "Live ECS grip captured independently from dry reference");
        Near(row.Value<float>("controllerForceLimitN"), 150000, "Controller slip-reduced force limit remains distinct");
        Near(row.Value<float>("referenceDryGripN"), 174225.6f, "Dry comparison does not use current weather/sand output");
        double main = TractiveEffortModel.MeanForceN(2, c.engine.pistonArea, c.engine.pistonStroke, .712f, 15, .75, 300).Value;
        Near(row.Value<float>("referenceMeanForceN"), (float)(main * 1.5), "Dormant engine excluded; active rear included");
        Check(row["engines"].Count() == 3 && !row["engines"][2].Value<bool>("active"), "Dormant graph components disclosed without counting their capacity");
        Near(row["engines"][0].Value<float>("modifiedAdmissionPressureBarAbs"), 11.45f, "Mean estimate includes native cock pressure loss");
        Check(c.engine.admissionPressure == 12 && c.engine.cylinderCockControl.Value == .5f && dormant.Active == false,
            "Observation leaves steam ports, dimensions and activation unchanged");

        dormant.Active = true; dormant.TenderMode = true; dormant.TenderTorque = 123f;
        c.sim.train.Rearranger.TenderReport = new JObject { ["driverRadiusM"] = .6, ["attached"] = true, ["limitN"] = 80000 };
        JObject erie = TractiveDiagnostics.Capture(c.sim.train);
        Near(erie["engines"][2].Value<float>("instantaneousTorqueNm"),123,"Tender uses retained torque rather than its zeroed locomotive sum port");
        Near(erie.Value<float>("referenceMeanForceN"), (float)(main * 1.5), "Tender engine excluded from locomotive force");
        Check(erie["tender"].Value<double>("referenceMeanForceN") > main &&
            erie["engines"][2].Value<string>("tractionGroup") == "tender", "Tender capacity reported separately at its own radius");
        Check(c.sim.train.Rearranger.TenderReport["referenceMeanForceN"] == null, "Diagnostic annotation does not mutate source tender report");

        c.sim.wheelslipController.NumberOfPoweredAxles = 0;
        var unpowered = TractiveDiagnostics.Capture(c.sim.train);
        Check(unpowered.Value<double>("referenceMeanForceN") == 0 && unpowered.Value<double>("referenceAvailableForceN") == 0,
            "Zero-driver layout has zero loco reference force despite live boiler/engine");
        c.sim.wheelslipController.NumberOfPoweredAxles = 4;
        c.sim.train.entity.Throw = true;
        var missing = TractiveDiagnostics.Capture(c.sim.train);
        Check(missing["adhesionReadError"] != null && missing["referenceDryGripN"].Type == JTokenType.Null &&
            missing["referenceMeanForceN"].Type != JTokenType.Null, "Missing ECS grip is unavailable without losing engine measurements");
        c.sim.train.entity.Throw = false;
        c.sim.train.Rearranger.currentWA = WheelArrangementType.s222;
        Check(TractiveDiagnostics.Capture(c.sim.train).Value<bool>("experimental"), "Giant-driver prototype explicitly experimental");
        Check(TractiveDiagnostics.Capture(null)["error"] != null, "Unavailable simulation returns diagnostic error without throwing");

        var structural = new JObject { ["tractiveReference"] = TractiveDiagnostics.ReferenceInfo(), ["arrangements"] = new JArray(
            new JObject { ["id"] = 10, ["name"] = "stock, \"quoted\"", ["passed"] = true, ["measurements"] = row },
            new JObject { ["id"] = 43, ["name"] = "Erie", ["passed"] = true, ["measurements"] = erie },
            new JObject { ["id"] = 36, ["name"] = "prototype", ["passed"] = false, ["measurements"] = missing }) };
        string csv = Path.Combine(Output, "tractive.csv");
        var previousCulture = CultureInfo.CurrentCulture;
        try { CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Check(TractiveDiagnostics.WriteStructuralCsv(structural, csv) == null, "CSV writes under comma-decimal culture"); }
        finally { CultureInfo.CurrentCulture = previousCulture; }
        var lines = File.ReadAllLines(csv);
        Check(lines.Length == 4 && lines[1].Contains("\"stock, \"\"quoted\"\"\"") &&
            lines[1].Contains("174225."), "CSV quotes names/JSON and writes invariant decimal values");
        Check(TractiveDiagnostics.WriteStructuralCsv(structural, Output) != null, "CSV I/O failure remains a diagnostic error");
        File.WriteAllText(Path.Combine(Output, "tractive-fixture.json"), structural.ToString());
    }
}
