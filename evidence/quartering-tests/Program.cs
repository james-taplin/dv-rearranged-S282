using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using RearrangedS282;
using UnityEngine;
using DV.Wheels;
using DV.Simulation.Ports;
class Program
{
    static int checks, cases, pairs;
    static void Check(bool ok, string why) { checks++; if (!ok) throw new Exception(why); }
    static Animator Make(string name, Transform parent, RuntimeAnimatorController controller, int wheels = 4)
    {
        var go = new GameObject(name); go.transform.SetParent(parent); var a = go.Add<Animator>(); a.runtimeAnimatorController = controller;
        a.state = new() { fullPathHash = controller.State, normalizedTime = 0 };
        for (int i = 1; i <= wheels; i++) new GameObject("s282_wheels_driving_" + i).transform.SetParent(go.transform);
        var reader = go.Add<AnimatorPortReader>(); reader.parameterName = "Cutoff"; reader.portId = "reverser";
        return a;
    }
    static void Main(string[] args)
    {
        var native = new RuntimeAnimatorController { State = DriveQuartering.NativeState };
        // The old order reproduces the loss under Unity's documented state-reset contract.
        var frame = new GameObject("inactive articulation frame"); frame.SetActive(false);
        var oldL = Make("old L", null, native); var oldR = Make("old R", null, native);
        DriveQuartering.Play(oldL, native.State, .75f, false); DriveQuartering.Play(oldR, native.State, 0, false);
        oldL.transform.SetParent(frame.transform); oldR.transform.SetParent(frame.transform); frame.SetActive(true);
        oldL.Evaluate(); oldR.Evaluate();
        Check(MathF.Abs(Quaternion.Angle(oldL.transform.children[0].localRotation, oldR.transform.children[0].localRotation)) < .01f,
            "Old apply-before-parenting scenario did not reproduce lost quartering");
        Check(DriveQuartering.ErrorDegrees(0, 0, .75f, 0) > 89f, "Collapsed pair escaped phase check");

        foreach (float cycle in new[] { -12.875f, -1f, -.1f, 0, .125f, .5f, .999f, 17.25f })
        foreach (int hash in new[] { DriveQuartering.NativeState, DriveQuartering.FranklinState, DriveQuartering.Franklin4444State })
        {
            var controller = new RuntimeAnimatorController { State = hash };
            var l = Make("left", frame.transform, controller); var r = Make("right", frame.transform, controller);
            DriveQuartering.Play(l, hash, cycle + .75f, true); DriveQuartering.Play(r, hash, cycle, true);
            foreach (float delta in new[] { 0f, .021f, -.043f, .155f })
            {
                l.Evaluate(delta); r.Evaluate(delta);
                Check(DriveQuartering.ErrorDegrees(l.state.normalizedTime, r.state.normalizedTime, .75f, 0) < .002f, "Cycle/forward/reverse phase drift");
                Check(MathF.Abs(Quaternion.Angle(l.transform.children[0].localRotation, r.transform.children[0].localRotation) - 90) < .01f, "Pose quartering failed");
            }
            int count = l.plays + r.plays;
            frame.SetActive(false); frame.SetActive(true); l.Evaluate(); r.Evaluate();
            Check(DriveQuartering.ErrorDegrees(l.state.normalizedTime, r.state.normalizedTime, .75f, 0) < .002f, "Disable/re-enable lost quartering");
            Check(count == l.plays + r.plays, "Quartering seeks during normal playback");
            Check(MathF.Abs(DriveQuartering.Capture(l, .75f) - DriveQuartering.Cycle(l.state.normalizedTime - .75f)) < .0001f, "Capture failed");
        }

        foreach (int id in Enumerable.Range(0, WheelArrangement.SupportedCount))
        foreach (bool tender in PoweredTenderPolicy.Supports(id) ? new[] { false, true } : new[] { false })
        {
            cases++;
            var car = new GameObject("car"); var l = Make("native L", car.transform, native); var r = Make("native R", car.transform, native);
            var rotation = car.Add<PoweredWheelRotationViaAnimation>();
            rotation.animatorSetups = new[] { new PoweredWheelRotationViaAnimation.AnimatorStartTimeOffsetPair { animator = l, startTimeOffset = .75f },
                new PoweredWheelRotationViaAnimation.AnimatorStartTimeOffsetPair { animator = r, startTimeOffset = 0 } };
            var owner = new WheelRearranger(car, l, r);
            int engines = WheelArrangement.IsArticulatedId(id) ? WheelArrangement.LocomotiveWheels(id).Length - 2 : 1;
            var controller = id == 32 ? AssetBundleLoader.FranklinBAnimCtrl4444 :
                id == 31 || id == 38 || id == 40 || id == 47 ? AssetBundleLoader.FranklinBAnimCtrl : native;
            l.runtimeAnimatorController = r.runtimeAnimatorController = controller;
            for (int i = 1; i < engines; i++)
            {
                var a = Make("clone L", car.transform, controller, 5); var b = Make("clone R", car.transform, controller, 5);
                rotation.animatorSetups = rotation.animatorSetups.Concat(new[] {
                    new PoweredWheelRotationViaAnimation.AnimatorStartTimeOffsetPair { animator = a, startTimeOffset = .75f },
                    new PoweredWheelRotationViaAnimation.AnimatorStartTimeOffsetPair { animator = b, startTimeOffset = 0 } }).ToArray();
            }
            owner.Apply(.125f); foreach (var s in rotation.animatorSetups) s.animator.Evaluate();
            Animator tl = null, tr = null;
            if (tender)
            {
                tl = Make("tender L", null, native); tr = Make("tender R", null, native);
                // A stale, unquartered host must not contaminate the tender pair.
                r.state.normalizedTime = l.state.normalizedTime;
                var clock = new GameObject("clock").Add<TenderDriveAnimation>();
                tl.transform.SetParent(clock.transform); tr.transform.SetParent(clock.transform);
                var sim = new ArticulatedSimulation(); sim.MiddleEngine.chuffFrequencyReadOut.Value = 12;
                sim.MainEngine.crankRpm.Value = sim.MiddleEngine.crankRpm.Value = -100;
                clock.Initialize(sim, tl, tr, l, r, .75f, 0);
                tl.Evaluate(); tr.Evaluate(); owner.Apply(.125f); foreach (var s in rotation.animatorSetups) s.animator.Evaluate();
                tl.gameObject.Add<MalletAnimatorMirror>().CopySpeed = false;
                tr.gameObject.Add<MalletAnimatorMirror>().CopySpeed = false;
                int speed = Animator.StringToHash("SpeedMultiplier");
                Check(tl.floats[speed] == -3 && tr.floats[speed] == -3, "Tender uses its native frequency and signed RPM input");
                foreach (var direction in new[] { (host: 100f, tender: 100f), (host: -100f, tender: -100f), (host: 100f, tender: -100f), (host: -100f, tender: 100f) })
                {
                    sim.MainEngine.crankRpm.Value = direction.host; sim.MiddleEngine.crankRpm.Value = direction.tender;
                    typeof(TenderDriveAnimation).GetMethod("LateUpdate", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(clock, null);
                    float expected = direction.tender < 0f ? -3f : 3f;
                    Check(tl.floats[speed] == expected && tr.floats[speed] == expected,
                        "Tender follows its own native RPM sign even when host direction differs");
                }
                var lateUpdate = typeof(TenderDriveAnimation).GetMethod("LateUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
                var savedMiddle = sim.MiddleEngine;
                sim.IsMiddleActive = false; // Normal Restore retains the disabled native engine object.
                lateUpdate.Invoke(clock, null);
                Check(tl.floats[speed] == 0 && tr.floats[speed] == 0 && !clock.Drives(tl),
                    "Inactive native tender simulation stops its retained visual clock");
                sim.MiddleEngine = null; sim.IsTenderEngine = false; // Failed graph preparation/partial bind.
                tl.SetFloat(speed, 5); tr.SetFloat(speed, 5);
                lateUpdate.Invoke(clock, null);
                Check(tl.floats[speed] == 0 && tr.floats[speed] == 0 && !clock.Drives(tr),
                    "Unbound middle engine safely stops both tender sides while simulation object survives");
                var simulationField = typeof(TenderDriveAnimation).GetField("simulation", BindingFlags.NonPublic | BindingFlags.Instance);
                simulationField.SetValue(clock, null);
                tl.SetFloat(speed, 5); tr.SetFloat(speed, 5);
                lateUpdate.Invoke(clock, null);
                Check(tl.floats[speed] == 0 && tr.floats[speed] == 0 && !clock.Drives(tl),
                    "Missing simulation safely stops both tender sides");
                sim.MiddleEngine = savedMiddle; sim.IsMiddleActive = sim.IsTenderEngine = true;
                simulationField.SetValue(clock, sim); lateUpdate.Invoke(clock, null);
                Check(tl.floats[speed] == 3 && tr.floats[speed] == 3 && clock.Drives(tl),
                    "Clock resumes from its active middle engine after simulation rebind");
                GroupCrankAnimation.Owned.Add(tl); GroupCrankAnimation.Owned.Add(tr);
                tl.SetFloat(speed, 0); tr.SetFloat(speed, 0);
                typeof(TenderDriveAnimation).GetMethod("LateUpdate", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(clock, null);
                Check(tl.floats[speed] == 0 && tr.floats[speed] == 0, "Tender fallback must not overwrite an independent native phase owner");
                GroupCrankAnimation.Owned.Clear();
                typeof(TenderDriveAnimation).GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(clock, null);
                Check(tl.floats[speed] == 0 && tr.floats[speed] == 0 && !tl.GetComponents<AnimatorPortReader>().Last().bound,
                    "Tender teardown/reader cleanup failed");
                clock.Initialize(sim, tl, tr, l, r, .75f, 0);tl.Evaluate();tr.Evaluate();
            }
            owner.Arrangement(id, tl, tr);
            if(id==33)
            {
                // The actual driverless layout hides both drivetrain roots.
                l.gameObject.SetActive(false);r.gameObject.SetActive(false);
                Check((bool)owner.QuarteringReport()["passed"] && ((JArray)owner.QuarteringReport()["pairs"]).Count==0,
                    "Driverless layout must have no active engine pairs");
                l.gameObject.SetActive(true);r.gameObject.SetActive(true);owner.Arrangement(10);
                owner.Apply(.25f,false);l.Evaluate();r.Evaluate();
                Check((bool)owner.QuarteringReport()["passed"],"Driverless return to stock restores quartering");
                continue;
            }
            var report = owner.QuarteringReport();
            Check((bool)report["passed"], "Valid active-pair matrix failed: " + id + " " + report);
            var ownership = owner.AnimationOwnershipReport();
            Check(((JArray)ownership["locomotive"]).Count == engines * 2 &&
                ((JArray)ownership["tender"]).Count == (tender ? 2 : 0) &&
                ((JArray)ownership["locomotive"]).Concat((JArray)ownership["tender"]).All(d => (bool)d["passed"]),
                "Valid stationary ownership rejected");
            Check(!(bool)ownership["motionVerified"] && !(bool)ownership["clearanceVerified"], "Static checks implied driving acceptance");
            l.speed = 0;
            Check(!(bool)owner.AnimationOwnershipReport()["locomotive"][0]["passed"], "Unowned frozen Animator escaped");
            GroupCrankAnimation.Owned.Add(l);
            Check((bool)owner.AnimationOwnershipReport()["locomotive"][0]["passed"], "Native phase owner incorrectly rejected");
            GroupCrankAnimation.Owned.Remove(l); l.speed = 1;
            if (tender)
            {
                var actual = tl.GetComponent<MalletAnimatorMirror>(); actual.CopySpeed = true;
                var pending = tl.gameObject.Add<MalletAnimatorMirror>(); pending.CopySpeed = false; pending.enabled = false;
                Check(!(bool)owner.AnimationOwnershipReport()["tender"][0]["passed"], "Inactive configured mirror concealed active zero-speed writer");
                actual.CopySpeed = false;
                Check((bool)owner.AnimationOwnershipReport()["tender"][0]["passed"], "Inactive retiring mirror treated as active writer");
                pending.enabled = true;
                Check(!(bool)owner.AnimationOwnershipReport()["tender"][0]["passed"], "Duplicate active tender mirrors escaped");
                pending.enabled = false;
                var clock = tl.GetComponentsInParent<TenderDriveAnimation>(true).Single(); clock.enabled = false;
                Check(!(bool)owner.AnimationOwnershipReport()["tender"][0]["passed"], "Disabled tender clock escaped");
                clock.enabled = true;
            }
            pairs += ((JArray)report["pairs"]).Count;
            // A real phase owner leases these registrations away from the native
            // speed writer. The mechanism inventory must survive that handover.
            if (WheelArrangement.IsArticulatedId(id))
            {
                var registered = rotation.animatorSetups;
                GroupCrankAnimation.Leases[rotation] = registered;
                rotation.animatorSetups = Array.Empty<PoweredWheelRotationViaAnimation.AnimatorStartTimeOffsetPair>();
                Check((bool)owner.QuarteringReport()["passed"], "Phase ownership hid active engine pairs from structural diagnostics");
                GroupCrankAnimation.Leases.Remove(rotation); rotation.animatorSetups = registered;
            }
            r.state.normalizedTime = l.state.normalizedTime; r.Evaluate();
            Check(!(bool)owner.QuarteringReport()["passed"], "Structural phase/pose fault escaped");
            owner.Apply(.125f); foreach (var s in rotation.animatorSetups) s.animator.Evaluate();
            r.transform.children[0].localRotation = l.transform.children[0].localRotation;
            Check(!(bool)owner.QuarteringReport()["passed"], "Correct clocks hid incorrect wheel poses");
            r.Evaluate();
            foreach (var wheel in l.transform.children.Concat(r.transform.children)) wheel.gameObject.SetActive(false);
            Check(!(bool)owner.QuarteringReport()["passed"], "Quartered clocks without any visible wheels passed");
            foreach (var wheel in l.transform.children.Concat(r.transform.children)) wheel.gameObject.SetActive(true);
            r.gameObject.SetActive(false);
            Check(!(bool)owner.QuarteringReport()["passed"], "Missing right engine passed");
            r.gameObject.SetActive(true); r.Evaluate();
            // Model returning native flags with the geometry snapshot, then stock.
            l.runtimeAnimatorController = r.runtimeAnimatorController = native;
            l.keepAnimatorControllerStateOnDisable = r.keepAnimatorControllerStateOnDisable = false;
            rotation.animatorSetups = rotation.animatorSetups.Take(2).ToArray(); owner.Arrangement(10);
            owner.Apply(.25f, false); l.Evaluate(); r.Evaluate();
            Check((bool)owner.QuarteringReport()["passed"] && !l.keepAnimatorControllerStateOnDisable && !r.keepAnimatorControllerStateOnDisable,
                "Stock restore retained mod retention flags or lost quartering");
        }
        foreach (float bad in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
        { bool rejected = false; try { DriveQuartering.Cycle(bad); } catch (ArgumentOutOfRangeException) { rejected = true; } Check(rejected, "Nonfinite phase accepted"); }
        Check(DriveQuartering.Capture(null, .75f) == 0, "Uninitialized source should use a safe initial cycle");
        int linkageChecks = LinkageReportFixture.Run();
        var result = new { passed = true, checks, linkageChecks, arrangementTenderCases = cases, measuredEnginePairs = pairs,
            oldOrderFailedAsExpected = true, runtimeExercised = false,
            limitation = "Actual production phase helper, pair diagnostics and tender initialization against managed Unity lifecycle/pose stand-ins. Does not validate real Animator scheduling, rendered linkage or game lifecycle." };
        File.WriteAllText(args[0], JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine(JsonSerializer.Serialize(result));
    }
}
