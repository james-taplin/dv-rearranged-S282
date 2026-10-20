#if B99_ALL_STANDARD
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DV.Simulation.Controllers;
using DV.Simulation.Ports;
using DV.Util;
using LocoSim.Definitions;
using LocoSim.Implementations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RearrangedS282
{
    // A live oil cup on every driven wheel's big end (crank pin), on every engine.
    // The front engine's main-rod big end keeps its native cup; every other driven
    // big end gets a copy of that side's native cup, instantiated from the
    // external-interactables prefab, wired to its own extra native oiling point
    // (BigEndOiling) and following a marker on the engine's coupling rod.
    // Stock 2-8-2, the 4-0-4 and the 2-2-2 prototype keep the native cups only.
    internal partial class WheelRearranger
    {
        private const string BigEndCupPrefix = "[Rearranged big end ";
        private const float BigEndCupLift = .14f;   // native main-rod cup sits this far above the pin
        private readonly List<Transform> bigEndMarkers = new List<Transform>();
        private readonly List<GameObject> bigEndCups = new List<GameObject>();

        internal static bool IsBigEndCup(Transform t)
        {
            for (var x = t; x != null; x = x.parent) if (x.name.StartsWith(BigEndCupPrefix, StringComparison.Ordinal)) return true;
            return false;
        }

        private sealed class BigEndSite
        {
            internal bool Left, NativeCup;
            internal Transform Wheel, Rod, Drive;
            internal Vector3 PinWorld;
        }

        // Called after every switch and whenever the external interactables load.
        private void RefreshBigEndOiling(GameObject external)
        {
            var car = loco;
            if (car == null || car.SimController == null || car.SimController.simFlow == null) return;
            ClearBigEndCups();
            var flow = car.SimController.simFlow;
            var points = BigEndOiling.Points(flow);
            if (points == null || points.oilingPoints.Length != BigEndOilingPlan.TotalPoints) return;
            bool enabled = Main.settings == null || Main.settings.bigEndOiling;
            var sites = enabled ? BigEndSites() : new List<BigEndSite>();
            var allocation = BigEndOilingPlan.Allocate(sites.Select(s => new BigEndOilingPlan.BigEnd(s.Left,
                car.transform.InverseTransformPoint(s.PinWorld).z, s.NativeCup)).ToList(), out int dropped);
            if (dropped > 0) Main.Logger.Warning($"Big-end oiling: {dropped} big ends have no spare oiling point on {currentWA}.");
            BigEndOiling.SetActive(flow, BigEndOilingPlan.ActivePoints(allocation));
            if (external == null) return;
            try
            {
                var created = new List<(GameObject cup, InteractablePortFeeder[] feeders)>();
                for (int i = 0; i < sites.Count; i++)
                {
                    if (allocation[i] < 0) continue;
                    var cup = CreateBigEndCup(car, external, points, sites[i], allocation[i]);
                    if (cup.cup != null) created.Add(cup);
                }
                if (created.Count > 0)
                {
                    StartCoroutine(InitBigEndControls(car, external, created));
                    Main.Logger.Log($"Big-end oiling: {created.Count} extra cups on {currentWA} " +
                        $"({allocation.Count(a => a >= 0 && a < BigEndOilingPlan.NativePoints + BigEndOilingPlan.ExtraPerSide)} left, " +
                        $"{allocation.Count(a => a >= BigEndOilingPlan.NativePoints + BigEndOilingPlan.ExtraPerSide)} right).");
                }
            }
            catch (Exception ex)
            {
                Main.Logger.Error("Big-end oil cups could not be created; native cups only: " + ex);
                ClearBigEndCups();
            }
        }

        // Driven big ends of the current arrangement: every active driving wheel of
        // every engine on the locomotive, its crank pin taken from its own engine's
        // main rod (all wheels of one engine turn in phase).
        private List<BigEndSite> BigEndSites()
        {
            var result = new List<BigEndSite>();
            if (currentWA == WheelArrangementType.s282 || currentWA == WheelArrangementType.s404 || currentWA == WheelArrangementType.s222)
                return result;
            var body = transform.Find("LocoS282A_Body/MovingParts_LOD0");
            if (body == null) return result;
            var drives = body.GetComponentsInChildren<Animator>(false).Select(a => a.transform)
                .Where(d => d.Find("s282_mech_wheels_connect/s282_mech_push_rod_to_connect") != null && d.gameObject.activeInHierarchy).ToArray();
            foreach (var drive in drives)
            {
                bool left = drive.name.EndsWith("L", StringComparison.Ordinal) || drive.name.EndsWith("L]", StringComparison.Ordinal);
                var rod = drive.Find("s282_mech_wheels_connect");
                var main = rod.Find("s282_mech_push_rod_to_connect");
                var wheels = drive.Cast<Transform>().Where(w => w.name.StartsWith("s282_wheels_driving_", StringComparison.Ordinal) &&
                    !w.name.Contains("LOD") && w.gameObject.activeInHierarchy).ToArray();
                if (wheels.Length == 0) continue;
                float Along(Transform t) => transform.InverseTransformPoint(t.position).z;
                var mainWheel = wheels.OrderBy(w => Mathf.Abs(Along(w) - Along(main))).First();
                var crank = main.position - mainWheel.position;
                bool front = drive == driveL || drive == driveR;
                foreach (var wheel in wheels)
                    result.Add(new BigEndSite { Left = left, Wheel = wheel, Rod = rod, Drive = drive,
                        NativeCup = front && wheel == mainWheel, PinWorld = wheel.position + crank });
            }
            return result;
        }

        private (GameObject cup, InteractablePortFeeder[] feeders) CreateBigEndCup(TrainCar car, GameObject external,
            ManualOilingPoints points, BigEndSite site, int pointIndex)
        {
            var flow = car.SimController.simFlow;
            // Template: this side's native main-rod cup, as authored in the prefab.
            var nativeProvider = (site.Left ? driveL : driveR).Find("s282_mech_wheels_connect/s282_mech_push_rod_to_connect/Oiling2")?.GetComponent<PositionSyncProvider>();
            if (nativeProvider == null) throw new InvalidOperationException("Native main-rod oil cup provider is missing.");
            var liveReader = external.GetComponentsInChildren<OilingPointPortFeederReader>(true)
                .FirstOrDefault(r => !IsBigEndCup(r.transform) && r.GetComponentInParent<PositionSyncConsumer>()?.syncTag == nativeProvider.syncTag);
            if (liveReader == null) throw new InvalidOperationException("Native main-rod oil cup was not found.");
            var liveCup = liveReader.GetComponentInParent<PositionSyncConsumer>().transform;
            string path = RelativeTo(external.transform, liveCup);
            var prefabRoot = car.isExploded && car.carLivery.explodedExternalInteractablesPrefab != null
                ? car.carLivery.explodedExternalInteractablesPrefab : car.carLivery.externalInteractablesPrefab;
            var template = prefabRoot != null ? prefabRoot.transform.Find(path) : null;
            if (template == null) throw new InvalidOperationException("Oil cup prefab not found at " + path);
            int nativeIndex = Enumerable.Range(0, BigEndOilingPlan.NativePoints)
                .First(i => points.id + "." + Definition(i).refillExtIn.ID == liveReader.refillPortId);
            var from = Definition(nativeIndex); var to = Definition(pointIndex);
            var portMap = new Dictionary<string, string>
            {
                [points.id + "." + from.oilLevelReadOut.ID] = points.id + "." + to.oilLevelReadOut.ID,
                [points.id + "." + from.oilLevelNormalizedReadOut.ID] = points.id + "." + to.oilLevelNormalizedReadOut.ID,
                [points.id + "." + from.pointDoorExtIn.ID] = points.id + "." + to.pointDoorExtIn.ID,
                [points.id + "." + from.refillExtIn.ID] = points.id + "." + to.refillExtIn.ID,
                [points.id + "." + from.refillingFlowNormalizedReadOut.ID] = points.id + "." + to.refillingFlowNormalizedReadOut.ID,
            };
            string tag = "RS_BIGEND_" + pointIndex;

            // Marker on the coupling rod, above the crank pin (the rod only translates).
            var marker = new GameObject("[Rearranged big end marker " + pointIndex + "]").transform;
            marker.SetParent(site.Rod, false);
            marker.position = site.PinWorld + site.Drive.TransformVector(Vector3.up * BigEndCupLift);
            var provider = marker.gameObject.AddComponent<PositionSyncProvider>();
            provider.syncTag = tag;
            bigEndMarkers.Add(marker);

            var cup = Object.Instantiate(template.gameObject, liveCup.parent, false);
            cup.name = BigEndCupPrefix + (site.Left ? "L" : "R") + " " + pointIndex + "]";
            bigEndCups.Add(cup);
            foreach (var behaviour in cup.GetComponentsInChildren<MonoBehaviour>(true)) RewritePortIds(behaviour, portMap);
            var consumer = cup.GetComponent<PositionSyncConsumer>() ?? cup.GetComponentInChildren<PositionSyncConsumer>(true);
            if (consumer != null)
            {
                consumer.syncTag = tag;
                consumer.SetProviderTransform(provider);
                consumer.Sync();
                Append(external.GetComponentInChildren<PositionSyncConsumerController>(true), consumer);
            }
            foreach (var reader in cup.GetComponentsInChildren<OilingPointPortFeederReader>(true))
                if (flow.TryGetPort(reader.refillPortId, out var refill) && flow.TryGetPort(reader.refillingFlowNormalizedPortId, out var refillFlow))
                { reader.Init(refill, refillFlow); Append(external.GetComponentInChildren<OilingPointsPortController>(true), reader); }
            foreach (var indicator in cup.GetComponentsInChildren<IndicatorPortReader>(true))
                if (flow.TryGetPort(indicator.portId, out var level))
                {
                    flow.TryGetPort(indicator.indicatorRangeScalerPortId, out var scaler, canBeNullOrEmpty: true);
                    indicator.Init(level, null, scaler);
                    Append(external.GetComponentInChildren<IndicatorPortReadersController>(true), indicator);
                }
            return (cup, cup.GetComponentsInChildren<InteractablePortFeeder>(true));

            ManualOilingPointsDefinition.OilingPointDefinition Definition(int i) => BigEndOiling.Definition(car).oilingPoints[i];
        }

        // Native feeders wait for the controls to finish spawning before binding.
        private IEnumerator InitBigEndControls(TrainCar car, GameObject external, List<(GameObject cup, InteractablePortFeeder[] feeders)> created)
        {
            yield return null; yield return null;
            var flow = car != null ? car.SimController?.simFlow : null;
            var controller = external != null ? external.GetComponentInChildren<InteractablePortFeedersController>(true) : null;
            if (flow == null) yield break;
            foreach (var (cup, feeders) in created)
            {
                if (cup == null) continue;
                foreach (var feeder in feeders)
                    if (feeder != null && flow.TryGetPort(feeder.portId, out var port))
                    { feeder.Init(port, null); feeder.SetupControlChangedListeners(); Append(controller, feeder); }
            }
        }

        private void ClearBigEndCups()
        {
            foreach (var cup in bigEndCups.Where(c => c != null))
            {
                foreach (var reader in cup.GetComponentsInChildren<OilingPointPortFeederReader>(true)) reader.Deinit();
                foreach (var feeder in cup.GetComponentsInChildren<InteractablePortFeeder>(true)) feeder.Deinit();
                foreach (var indicator in cup.GetComponentsInChildren<IndicatorPortReader>(true)) indicator.Deinit();
                var root = cup.transform.root;
                RemoveOurs(root.GetComponentInChildren<PositionSyncConsumerController>(true));
                RemoveOurs(root.GetComponentInChildren<OilingPointsPortController>(true));
                RemoveOurs(root.GetComponentInChildren<IndicatorPortReadersController>(true));
                RemoveOurs(root.GetComponentInChildren<InteractablePortFeedersController>(true));
                cup.SetActive(false); Object.Destroy(cup);
            }
            bigEndCups.Clear();
            foreach (var marker in bigEndMarkers.Where(m => m != null)) { marker.gameObject.SetActive(false); Object.Destroy(marker.gameObject); }
            bigEndMarkers.Clear();
        }

        private static string RelativeTo(Transform root, Transform t)
        {
            var names = new List<string>();
            for (var x = t; x != null && x != root; x = x.parent) names.Add(x.name);
            names.Reverse();
            return string.Join("/", names);
        }

        // Port IDs and sync tags live in plain serialized string fields.
        private static void RewritePortIds(MonoBehaviour behaviour, Dictionary<string, string> map)
        {
            if (behaviour == null) return;
            foreach (var field in behaviour.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                if (field.FieldType == typeof(string) && field.GetValue(behaviour) is string value && map.TryGetValue(value, out var replacement))
                    field.SetValue(behaviour, replacement);
        }

        private static void Append<T>(ARefreshableChildrenController<T> controller, T item) where T : MonoBehaviour
        {
            if (controller == null || item == null) return;
            controller.entries = (controller.entries ?? Array.Empty<T>()).Where(e => e != null && e != item).Concat(new[] { item }).ToArray();
        }

        private static void RemoveOurs<T>(ARefreshableChildrenController<T> controller) where T : MonoBehaviour
        {
            if (controller?.entries == null) return;
            controller.entries = controller.entries.Where(e => e != null && !IsBigEndCup(e.transform)).ToArray();
        }
    }
}
#endif
