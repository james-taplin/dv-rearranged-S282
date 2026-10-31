using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using DV.Simulation.Ports;
using LocoSim.Definitions;
using LocoSim.Implementations;
using Newtonsoft.Json.Linq;
using RearrangedS282;
using UnityEngine;
using Object = UnityEngine.Object;

public static class TenderAnimationProbe
{
    // The standalone bootstrap may supply the actual native resource child.
    // A generated native-prefab resource is also supported. Never substitute a
    // synthetic controller if the real drivetrain cannot be loaded.
    public static GameObject DrivetrainSource;
    static readonly List<string> checks = new List<string>();
    static readonly BindingFlags Fields = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    static readonly int Speed = Animator.StringToHash("SpeedMultiplier");
    static void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks.Add(name); }
    static void Set(object target, string field, object value) => target.GetType().GetField(field, Fields).SetValue(target, value);
    static ReciprocatingSteamEngine Engine(string id, out Port rpm, out Port frequency)
    {
        var engine = (ReciprocatingSteamEngine)FormatterServices.GetUninitializedObject(typeof(ReciprocatingSteamEngine));
        rpm = new Port(id, new PortDefinition(PortType.READONLY_OUT, PortValueType.RPM, "RPM"));
        frequency = new Port(id, new PortDefinition(PortType.READONLY_OUT, PortValueType.GENERIC, "CHUFF_FREQUENCY"));
        var reference = new PortReference(id, new PortReferenceDefinition(PortValueType.RPM, "CRANK_RPM"));
        reference.SetPortReference(rpm);
        Set(engine, "crankRpm", reference); Set(engine, "chuffFrequencyReadOut", frequency); Set(engine, "numCylinders", 2);
        return engine;
    }
    static Transform Wheel(Animator animator, int axle) => animator.transform.Find("s282_wheels_driving_" + axle);
    static float Phase(Animator animator) => animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    static void PrepareSourceReader(Animator source, ArticulatedSimulation simulation, Port reverser)
    {
        var reader = source.GetComponents<AnimatorPortReader>().FirstOrDefault(r => r.parameterName == "Cutoff")
            ?? source.gameObject.AddComponent<AnimatorPortReader>();
        reader.Deinit(); reader.updateType = AnimatorPortReader.UpdateType.SET_PARAMETER;
        reader.parameterName = "Cutoff"; reader.portId = "fixture.REVERSER";
        reader.valueMultiplier = 1f; reader.valueOffset = 0f;
        simulation.Flow.Ports[reader.portId] = reverser;
        reader.Init(reverser);
    }
    public static IEnumerator Run(string output)
    {
        checks.Clear();
        var native = DrivetrainSource != null ? DrivetrainSource : Resources.Load<GameObject>("NativeS282DrivetrainR");
        Check(native != null, "Actual native S282 drivetrain payload supplied");
        var nativeAnimator = native.GetComponent<Animator>();
        Check(nativeAnimator != null && nativeAnimator.runtimeAnimatorController != null, "Native S282 Animator/controller present");
        var roots = new List<GameObject>();
        try
        {
            var owner = new GameObject("Tender clone owner"); roots.Add(owner);
            var cloner = owner.AddComponent<WheelRearranger>();
            var main = Engine("main", out var mainRpm, out var mainFrequency);
            var middle = Engine("tender", out var tenderRpm, out var tenderFrequency);
            var simulation = new ArticulatedSimulation { MainEngine = main, MiddleEngine = middle };
            var reverser = new Port("fixture", new PortDefinition(PortType.READONLY_OUT, PortValueType.GENERIC, "REVERSER"));
            reverser.Value = .6f;
            var hosts = new List<Animator>();
            var templates = new List<Transform>();
            foreach (bool left in new[] { true, false })
            {
                var host = Object.Instantiate(native, owner.transform); host.name = left ? "host L" : "host R";
                host.transform.localScale = new Vector3(left ? -1f : 1f, 1f, 1f); host.SetActive(true);
                var animator = host.GetComponent<Animator>(); animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                Check(animator.HasState(0, DriveQuartering.NativeState), "Real native Walschaerts blend-tree state loaded: " + host.name);
                Check(Enumerable.Range(1, 4).All(i => host.transform.Find("s282_wheels_driving_" + i) != null), "All four actual native driving wheels exist: " + host.name);
                PrepareSourceReader(animator, simulation, reverser);
                hosts.Add(animator);
                // Exact production helper through both inactive template stages.
                var rear = cloner.ProbeClone(host.transform, animator, "rear " + host.name, owner.transform, false);
                var templateRoot = new GameObject("inactive source " + host.name); templateRoot.transform.SetParent(owner.transform, false); templateRoot.SetActive(false);
                templates.Add(cloner.ProbeClone(rear, animator, "template " + host.name, templateRoot.transform, false));
                animator.speed = 0f; animator.SetFloat(Speed, 0f); GroupCrankAnimation.Owned.Add(animator);
            }
            yield return null;
            var cases = new JArray();
            var stops = new JArray();
            for (int cycle = 0; cycle < 3; cycle++)
            {
                var tenderRoot = new GameObject("attached tender " + cycle); tenderRoot.transform.SetParent(owner.transform, false);
                var targets = templates.Select((template, side) => cloner.ProbeClone(template, hosts[side], "tender " + side, tenderRoot.transform, true).GetComponent<Animator>()).ToArray();
                bool exactMirrorsConfigured = true;
                foreach (var target in targets)
                {
                    target.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    target.gameObject.SetActive(true);
                    var active = target.GetComponents<MalletAnimatorMirror>().Where(m => m.isActiveAndEnabled).ToArray();
                    Check(active.Length == 1, "One new active mirror exists before deferred destruction: cycle " + cycle + " " + target.name);
                    exactMirrorsConfigured &= !active[0].CopySpeed;
                }
                var clock = tenderRoot.AddComponent<TenderDriveAnimation>();
                clock.Initialize(simulation, targets[0], targets[1], hosts[0], hosts[1], .75f, 0f);
                yield return new WaitForEndOfFrame(); yield return null;
                foreach (var target in targets)
                    Check(target.GetComponents<MalletAnimatorMirror>().Length == 1 && clock.Drives(target), "Retired mirrors destroyed and tender clock bound: cycle " + cycle + " " + target.name);
                Check(DriveQuartering.ErrorDegrees(Phase(targets[0]), Phase(targets[1]), .75f, 0f) < 1f,
                    "Native left/right quartering survives template attachment: cycle " + cycle);
                foreach (var signs in new[] { (tender: 120f, main: 400f), (tender: -120f, main: -400f), (tender: -120f, main: 400f), (tender: 120f, main: -400f) })
                {
                    // A moving/slipping host must not animate a stopped tender.
                    // Sample only after the normal LateUpdate/Animator boundary
                    // has consumed zero, then restart using the next sign case.
                    mainRpm.Value = signs.main; tenderRpm.Value = 0f; tenderFrequency.Value = 0f;
                    yield return null; yield return null;
                    float[] stoppedPhases = targets.Select(Phase).ToArray();
                    var stoppedWheels = targets.Select(a => Enumerable.Range(1, 4).Select(axle => Wheel(a, axle).localRotation).ToArray()).ToArray();
                    yield return new WaitForSecondsRealtime(.08f);
                    for (int side = 0; side < 2; side++)
                    {
                        Check(targets[side].GetFloat(Speed) == 0f && Mathf.Abs(Phase(targets[side]) - stoppedPhases[side]) < .0001f,
                            "Stopped tender retains native phase while host RPM is nonzero: " + cycle + " " + side + "; hostRPM=" + signs.main);
                        foreach (int axle in Enumerable.Range(1, 4))
                            Check(Quaternion.Angle(stoppedWheels[side][axle - 1], Wheel(targets[side], axle).localRotation) < .01f,
                                "Stopped native driving-wheel pose remains fixed: " + cycle + " " + side + " axle " + axle + "; hostRPM=" + signs.main);
                    }
                    stops.Add(new JObject { ["cycle"] = cycle, ["tenderRpm"] = 0f, ["tenderFrequency"] = 0f, ["hostRpm"] = signs.main });
                    mainRpm.Value = signs.main; tenderRpm.Value = signs.tender; tenderFrequency.Value = 8f;
                    foreach (var target in targets) target.Play(DriveQuartering.NativeState, 0, .5f);
                    yield return null; yield return null;
                    float[] start = targets.Select(Phase).ToArray();
                    var wheelStarts = targets.Select(a => Enumerable.Range(1, 4).Select(axle => Wheel(a, axle).localRotation).ToArray()).ToArray();
                    yield return new WaitForSecondsRealtime(.12f);
                    for (int side = 0; side < 2; side++)
                    {
                        float delta = Phase(targets[side]) - start[side];
                        Check(Mathf.Sign(delta) == Mathf.Sign(signs.tender) && Mathf.Abs(delta) > .01f,
                            "Native rendered Animator follows tender direction, including opposed host sign: " + cycle + " " + side + " " + signs +
                            "; phase delta=" + delta + "; SpeedMultiplier=" + targets[side].GetFloat(Speed));
                        foreach (int axle in Enumerable.Range(1, 4))
                            Check(Quaternion.Angle(wheelStarts[side][axle - 1], Wheel(targets[side], axle).localRotation) > 1f,
                                "Actual native driving-wheel pose advances: " + cycle + " " + side + " axle " + axle + " " + signs);
                    }
                    cases.Add(new JObject { ["cycle"] = cycle, ["tenderRpm"] = signs.tender, ["hostRpm"] = signs.main });
                }
                Check(exactMirrorsConfigured, "Exact new mirrors were configured before deferred destruction: cycle " + cycle);
                int cutoffId = Animator.StringToHash("Cutoff");
                var cutoffBeforeDetach = targets.Select(a => a.GetFloat(cutoffId)).ToArray();
                tenderRoot.SetActive(false);
                reverser.Value = -reverser.Value;
                for (int side = 0; side < targets.Length; side++)
                {
                    var target = targets[side];
                    Check(target.GetFloat(Speed) == 0f && !clock.Drives(target), "Detach clears speed and clock bindings: " + cycle + " " + target.name);
                    Check(target.GetFloat(cutoffId) == cutoffBeforeDetach[side], "Detached native reverser reader unsubscribed: " + cycle + " " + target.name);
                }
                Object.Destroy(tenderRoot);
                yield return null;
            }
            File.WriteAllText(output, new JObject { ["passed"] = true, ["checks"] = new JArray(checks), ["cases"] = cases, ["stops"] = stops,
                ["controller"] = nativeAnimator.runtimeAnimatorController.name,
                ["limitations"] = "Actual Unity lifecycle, native S282 payload/controller/wheels, production clone helper/mirror/tender clock and native readers/ports. Detached RSE ports and a minimal simulation-flow boundary provide controlled inputs. No real TrainCar coupling/forces, finite boiler or multiplayer transport exercised." }.ToString());
        }
        finally { GroupCrankAnimation.Owned.Clear(); foreach (var root in roots) if (root != null) Object.Destroy(root); }
    }
}
