// Executes the actual production engine gate against native DV.Simulation.dll.
// Detached definitions bypass Unity's MonoBehaviour constructor only; all engine
// construction, port operations, Tick math and save methods are the game code.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using LocoSim.Definitions;
using LocoSim.Implementations;
using Newtonsoft.Json.Linq;
using RearrangedS282;

internal static class NativeProbe
{
    private static readonly List<string> Checks = new List<string>();
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static void Check(bool value, string name) { if (!value) throw new Exception(name); Checks.Add(name); }
    private static void Near(float a, float b, string name) => Check(Math.Abs(a - b) <= Math.Max(.00001f, Math.Abs(b) * .000001f), name);
    private static void Set(object target, string name, object value)
    {
        for (var type = target.GetType(); type != null; type = type.BaseType)
        {
            var field = type.GetField(name, Fields);
            if (field == null) continue;
            field.SetValue(target, value); return;
        }
        throw new MissingFieldException(name);
    }
    private static string PortName(string name) => Regex.Replace(name.Replace("ReadOut", ""), "([a-z])([A-Z])", "$1_$2").ToUpperInvariant();
    private static ArticulatedRearEngineDefinition Definition(JObject saved, float fraction = 1f)
    {
        var def = (ArticulatedRearEngineDefinition)FormatterServices.GetUninitializedObject(typeof(ArticulatedRearEngineDefinition));
        def.ID = "probe";
        foreach (var field in typeof(ReciprocatingSteamEngineDefinition).GetFields(Fields))
        {
            if (field.FieldType == typeof(float) && saved[field.Name] != null) field.SetValue(def, saved.Value<float>(field.Name));
            else if (field.FieldType == typeof(int) && saved[field.Name] != null) field.SetValue(def, saved.Value<int>(field.Name));
            else if (field.FieldType == typeof(PortReferenceDefinition))
                field.SetValue(def, new PortReferenceDefinition(PortValueType.GENERIC, PortName(field.Name)));
            else if (field.FieldType == typeof(PortDefinition))
                field.SetValue(def, new PortDefinition(field.Name == "torqueOut" ? PortType.OUT : field.Name == "healthStateExtIn" ? PortType.EXTERNAL_IN : PortType.READONLY_OUT,
                    PortValueType.GENERIC, field.Name == "torqueOut" ? "TORQUE_OUT" : PortName(field.Name)));
        }
        def.cylinderBore *= (float)Math.Sqrt(fraction);
        def.steamChestVolume *= fraction;
        def.throttleMaxFlow *= fraction;
        return def;
    }
    private static void Reference(PortReference reference, float value) => Set(reference, "defaultValue", value);
    private static void Setup(ReciprocatingSteamEngine engine, float rpm, bool warm, float throttle = 1)
    {
        engine.SetGameParams(new SimGameParams(true, true, true, 1, 1));
        Reference(engine.throttleControl, throttle); Reference(engine.reverserControl, .6f);
        Reference(engine.cylinderCockControl, 0); Reference(engine.crankRpm, rpm);
        Reference(engine.intakePressure, 16); Reference(engine.intakeTemperature, 350);
        Reference(engine.intakeQuality, 1); Reference(engine.lubricationNormalized, 1);
        engine.healthStateExtIn.ExternalValueUpdate(1);
        if (warm)
        {
            Set(engine, "steamChestSteamMass", engine.steamChestVolume / SteamTables.SteamSpecificVolume(16));
            Set(engine, "temperatureInCylinders", 300f);
            engine.steamChestPressureReadOut.Value = 16;
            engine.waterInCylindersNormalizedReadOut.Value = 0;
        }
    }
    private static float PrivateFloat(object target, string name)
    {
        for (var type = target.GetType(); type != null; type = type.BaseType)
        { var field = type.GetField(name, Fields); if (field != null) return (float)field.GetValue(target); }
        throw new MissingFieldException(name);
    }
    private static void SameOutputs(ReciprocatingSteamEngine actual, ReciprocatingSteamEngine expected, string label)
    {
        foreach (var field in typeof(ReciprocatingSteamEngine).GetFields(Fields).Where(f => f.FieldType == typeof(Port)))
            Near(((Port)field.GetValue(actual)).Value, ((Port)field.GetValue(expected)).Value, label + ": " + field.Name);
    }
    internal static int Main(string[] args)
    {
        try
        {
            var calibration = JObject.Parse(File.ReadAllText(args[1]));
            Check(calibration.Value<int>("numCylinders") == 2, "Recorded native S282 two-cylinder calibration loaded");
            var fraction = 2f * .55f * .55f * .97f * .97f * .97f * .711f /
                (calibration.Value<float>("cylinderBore") * calibration.Value<float>("cylinderBore") * calibration.Value<float>("pistonStroke")) - 1f;
            Check(fraction > .48f && fraction < .49f, "Conservative two-source-pair trial target leaves approximately .484 native rear capacity");
            var def = Definition(calibration, fraction);
            var dormant = new ArticulatedRearEngine(def);
            var actual = new ArticulatedRearEngine(def) { Active = true };
            var native = new ReciprocatingSteamEngine(def);
            Setup(dormant, 400, true); Setup(actual, 400, true); Setup(native, 400, true);
            var dormantMass = PrivateFloat(dormant, "steamChestSteamMass");
            var dormantTemperature = PrivateFloat(dormant, "temperatureInCylinders");
            int events = 0; dormant.chuffEventReadOut.ValueUpdatedInternally += _ => events++;
            for (int i = 0; i < 500; i++) { dormant.Tick(.02f); actual.Tick(.02f); native.Tick(.02f); }
            SameOutputs(actual, native, "Enabled subtype delegates exact native Tick for 500 steps");
            Check(actual.torqueOut.Value > 0 && actual.intakeFlowReadOut.Value > 0 && actual.generatedMechanicalDamageReadOut.Value > 0,
                "Active native rear generates steam-fed torque and ordinary physical wear");
            Check(dormant.torqueOut.Value == 0 && dormant.intakeFlowReadOut.Value == 0 && dormant.exhaustFlowReadOut.Value == 0 &&
                dormant.generatedMechanicalDamageReadOut.Value == 0 && dormant.chuffFrequencyReadOut.Value == 0 && events == 0,
                "Dormant hot engine produces no torque, steam consumption, exhaust, wear or chuff event changes");
            Near(PrivateFloat(dormant, "steamChestSteamMass"), dormantMass, "Dormant native chest mass remains unchanged");
            Near(PrivateFloat(dormant, "temperatureInCylinders"), dormantTemperature, "Dormant native cylinder temperature does not advance");
            // A guest receives these live ports from Multiplayer. It must not
            // advance the physical engine or overwrite received presentation.
            var observer = new ArticulatedRearEngine(def) { Active = true };
            Setup(observer, 400, true);
            observer.torqueOut.Value = 12000f;
            observer.chuffFrequencyReadOut.Value = 26f;
            observer.crankRotationReadOut.Value = .3f;
            var observerPorts = typeof(ReciprocatingSteamEngine).GetFields(Fields)
                .Where(f => f.FieldType == typeof(Port)).Select(f => (Port)f.GetValue(observer)).ToArray();
            var observerValues = observerPorts.Select(p => p.Value).ToArray();
            var observerMass = PrivateFloat(observer, "steamChestSteamMass");
            var observerTemperature = PrivateFloat(observer, "temperatureInCylinders");
            int observerEvents = 0;
            observer.chuffEventReadOut.ValueUpdatedInternally += _ => observerEvents++;
            MultiplayerSync.IsSimulationAuthority = false;
            try
            {
                for (int i = 0; i < 1000; i++) observer.Tick(.02f);
                observer.Disable();
                Check(!observer.Active, "Guest geometry can disable an engine without advancing its simulation");
            }
            finally { MultiplayerSync.IsSimulationAuthority = true; }
            for (int i = 0; i < observerPorts.Length; i++)
                Near(observerPorts[i].Value, observerValues[i], "Guest engine preserves mirrored port " + observerPorts[i].id);
            Near(PrivateFloat(observer, "steamChestSteamMass"), observerMass, "Guest engine does not consume native chest steam");
            Near(PrivateFloat(observer, "temperatureInCylinders"), observerTemperature, "Guest engine does not advance native temperature");
            Check(observerEvents == 0, "Guest engine does not emit independently simulated chuff events");
            var activeMass = PrivateFloat(actual, "steamChestSteamMass");
            var activeWater = actual.waterInCylindersNormalizedReadOut.Value;
            var activeCrank = actual.crankRotationReadOut.Value;
            actual.Disable(); actual.Disable();
            int stoppedEvents = 0; actual.chuffEventReadOut.ValueUpdatedInternally += _ => stoppedEvents++;
            for (int i = 0; i < 1000; i++) actual.Tick(.02f);
            Check(!actual.Active && actual.torqueOut.Value == 0 && actual.intakeFlowReadOut.Value == 0 && actual.exhaustFlowReadOut.Value == 0 &&
                actual.chuffFrequencyReadOut.Value == 0 && actual.cylinderCockFlowNormalizedReadOut.Value == 0 &&
                actual.generatedMechanicalDamageReadOut.Value == 0 && stoppedEvents == 0, "Repeated disable immediately clears live outputs and remains off for 1000 steps");
            Near(PrivateFloat(actual, "steamChestSteamMass"), activeMass, "Disable preserves charged native chest without refilling");
            Near(actual.waterInCylindersNormalizedReadOut.Value, activeWater, "Disable preserves condensate without implicit draining");
            Near(actual.crankRotationReadOut.Value, activeCrank, "Disable freezes native crank phase");
            actual.Active = true; actual.Tick(.02f);
            Check(actual.intakeFlowReadOut.Value >= 0 && actual.chuffFrequencyReadOut.Value > 0, "Reactivated engine resumes native port output");
            var cold = new ArticulatedRearEngine(def); Setup(cold, 800, false);
            for (int i = 0; i < 6000; i++) cold.Tick(.02f);
            Check(cold.isCylinderCrackedReadOut.Value == 0 && cold.generatedMechanicalDamageReadOut.Value == 0 && cold.waterInCylindersNormalizedReadOut.Value == 1,
                "Dormant cold rear does not accrue rolling water damage or wear over 120 seconds");
            actual.isCylinderCrackedReadOut.Value = 1;
            var crackedSave = actual.GetSaveStateData();
            var restored = new ArticulatedRearEngine(def); restored.SetSaveStateData(crackedSave);
            Check(restored.isCylinderCrackedReadOut.Value == 1, "Native rear cylinder crack persists in native save data");
            var uncracked = new ArticulatedRearEngine(def); uncracked.SetSaveStateData(new JObject());
            Check(uncracked.isCylinderCrackedReadOut.Value == 0, "Empty legacy rear state remains uncracked");
            var falseKey = new ArticulatedRearEngine(def); falseKey.SetSaveStateData(new JObject { ["cylCracked"] = false });
            Check(falseKey.isCylinderCrackedReadOut.Value == 1, "Native false-valued crack key still cracks, confirming empty-object migration requirement");
            var repair = new ArticulatedRearEngine(def); Setup(repair, 400, true);
            repair.isCylinderCrackedReadOut.Value = 1; Set(repair, "waterInCylindersDamageMeter", .8f);
            repair.Disable(); for (int i = 0; i < 10; i++) repair.Tick(.02f);
            Check(repair.isCylinderCrackedReadOut.Value == 1 && repair.GetSaveStateData().Value<bool>("cylCracked"),
                "Conversion to standard and dormant ticks do not implicitly repair an existing rear crack");
            repair.healthStateExtIn.ExternalValueUpdate(.8f); repair.healthStateExtIn.ExternalValueUpdate(.95f);
            Check(repair.isCylinderCrackedReadOut.Value == 1, "Dormant repair requires native health strictly above .95");
            float repairMass = PrivateFloat(repair, "steamChestSteamMass");
            repair.healthStateExtIn.ExternalValueUpdate(1);
            Check(repair.isCylinderCrackedReadOut.Value == 0 && PrivateFloat(repair, "waterInCylindersDamageMeter") == 0,
                "Observed native health increase while dormant clears rear crack and damage meter");
            Near(PrivateFloat(repair, "steamChestSteamMass"), repairMass, "Dormant native repair does not advance or refill the steam chest");
            repair.Active = true; repair.Tick(.02f);
            Check(repair.isCylinderCrackedReadOut.Value == 0 && repair.chuffFrequencyReadOut.Value > 0,
                "Rear remains repaired when reactivated after a dormant native repair");
            foreach (float ratio in new[] { .55f, 1f, 1.4f, 2.1f })
            {
                var tender = new ArticulatedRearEngine(def) { Active = true, TenderMode = true, TenderRpmRatio = ratio };
                var expected = new ReciprocatingSteamEngine(def);
                Setup(tender, 120f, true); Setup(expected, 120f * ratio, true);
                var shared = new Port("host", new PortDefinition(PortType.READONLY_OUT, PortValueType.RPM, "RPM"));
                shared.Value = 120f; tender.crankRpm.SetPortReference(shared);
                for (int tick = 0; tick < 150; tick++) { tender.Tick(.02f); expected.Tick(.02f); }
                Near(tender.TenderTorque, expected.torqueOut.Value, "Tender torque exact native at diameter-adjusted RPM " + ratio);
                Near(tender.intakeFlowReadOut.Value, expected.intakeFlowReadOut.Value, "Tender steam charged at diameter-adjusted RPM " + ratio);
                Near(tender.chuffFrequencyReadOut.Value, expected.chuffFrequencyReadOut.Value, "Tender sound/animation clock at correct RPM " + ratio);
                Near(tender.crankRotationReadOut.Value, expected.crankRotationReadOut.Value, "Tender phase integrates its own diameter " + ratio);
                Check(ReferenceEquals(tender.crankRpm.port, shared) && shared.Value == 120f, "Shared host input identity/value untouched " + ratio);
                Check(tender.torqueOut.Value == 0f && tender.exhaustFlowReadOut.Value == 0f && tender.TenderExhaust >= 0f,
                    "Tender outputs excluded from host pull/blast " + ratio);
                tender.Disable(); tender.Tick(.02f);
                Check(tender.TenderTorque == 0f && tender.intakeFlowReadOut.Value == 0f && shared.Value == 120f,
                    "Disabled tender cannot pull or consume after use " + ratio);
            }
            Directory.CreateDirectory(args[0]);
            var report = new JObject { ["passed"] = true, ["count"] = Checks.Count, ["checks"] = new JArray(Checks),
                ["rearCapacityFraction"] = fraction,
                ["limitations"] = "Exact production native engine gate and actual B99 engine constructors/Tick/ports/save methods executed outside Unity. Detached definition allocation only bypasses MonoBehaviour construction. Does not test full per-car graph preparation, Unity/Harmony dispatch, save lifecycle, articulation, audio/particles, RF patches, or real rail physics." };
            File.WriteAllText(Path.Combine(args[0], "validation.json"), report.ToString());
            Console.WriteLine("PASS " + Checks.Count + " native engine checks");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
