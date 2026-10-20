#if B99_ALL_STANDARD
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using DV.Simulation.Cars;
using DV.ThingTypes;
using HarmonyLib;
using LocoSim.Definitions;
using LocoSim.Implementations;
using Newtonsoft.Json.Linq;

namespace RearrangedS282
{
    // Simulation side of big-end oil cups. Every S282's native oiling component
    // (six points) gets the extra big-end points before its flow is built; points
    // the current arrangement does not use are held full and closed every tick, so
    // they never drain, damage or lower the oil gauge. Older saves are fitted to
    // the extended point list instead of being discarded by the native exact-length
    // check.
    internal static class BigEndOiling
    {
        private sealed class State { internal bool[] Active; }
        private static readonly ConditionalWeakTable<ManualOilingPoints, State> States = new ConditionalWeakTable<ManualOilingPoints, State>();
        internal const string PortSuffix = "_RSBIG";

        internal static void Register(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(SimController), nameof(SimController.Initialize)),
                prefix: new HarmonyMethod(typeof(BigEndOiling), nameof(ExtendBeforeInitialize)));
            harmony.Patch(AccessTools.Method(typeof(ManualOilingPoints), nameof(ManualOilingPoints.Tick)),
                postfix: new HarmonyMethod(typeof(BigEndOiling), nameof(HoldUnusedFull)));
            harmony.Patch(AccessTools.Method(typeof(SimulationFlow), nameof(SimulationFlow.SetSaveStateData)),
                prefix: new HarmonyMethod(typeof(BigEndOiling), nameof(FitSavedOil)));
        }

        private static bool IsS282(TrainCar car) => car != null && car.carType == TrainCarType.LocoSteamHeavy &&
            car.transform.Find("LocoS282A_Body") != null;

        internal static ManualOilingPointsDefinition Definition(TrainCar car) =>
            car.GetComponentsInChildren<ManualOilingPointsDefinition>(true).FirstOrDefault();

        private static void ExtendBeforeInitialize(TrainCar trainCar)
        {
            if (!IsS282(trainCar)) return;
            try
            {
                var definition = Definition(trainCar);
                if (definition == null || definition.oilingPoints == null) return;
                if (definition.oilingPoints.Length != BigEndOilingPlan.NativePoints) return;   // already extended, or not the native layout
                var template = definition.oilingPoints[0];
                PortDefinition Copy(PortDefinition p, int index) => new PortDefinition(p.type, p.valueType, p.ID + PortSuffix + index);
                var extra = Enumerable.Range(BigEndOilingPlan.NativePoints, BigEndOilingPlan.TotalPoints - BigEndOilingPlan.NativePoints)
                    .Select(i => new ManualOilingPointsDefinition.OilingPointDefinition
                    {
                        oilLevelReadOut = Copy(template.oilLevelReadOut, i),
                        oilLevelNormalizedReadOut = Copy(template.oilLevelNormalizedReadOut, i),
                        pointDoorExtIn = Copy(template.pointDoorExtIn, i),
                        refillExtIn = Copy(template.refillExtIn, i),
                        refillingFlowNormalizedReadOut = Copy(template.refillingFlowNormalizedReadOut, i)
                    });
                definition.oilingPoints = definition.oilingPoints.Concat(extra).ToArray();
            }
            catch (Exception ex) { Main.Logger.Warning("Big-end oiling points could not be added; native cups only: " + ex.Message); }
        }

        internal static ManualOilingPoints Points(SimulationFlow flow) =>
            flow?.OrderedSimComps?.OfType<ManualOilingPoints>().FirstOrDefault();

        // Which points the current arrangement uses (native six always).
        internal static void SetActive(SimulationFlow flow, bool[] active)
        {
            var points = Points(flow);
            if (points == null) return;
            States.Remove(points);
            States.Add(points, new State { Active = active });
        }

        private static void HoldUnusedFull(ManualOilingPoints __instance)
        {
            var points = __instance.oilingPoints;
            if (points == null || points.Length <= BigEndOilingPlan.NativePoints) return;
            States.TryGetValue(__instance, out var state);
            for (int i = BigEndOilingPlan.NativePoints; i < points.Length; i++)
            {
                if (state?.Active != null && i < state.Active.Length && state.Active[i]) continue;
                var point = points[i];
                point.oilLevelReadOut.Value = __instance.capacity;
                point.oilLevelNormalizedReadOut.Value = 1f;
                point.refillingFlowNormalizedReadOut.Value = 0f;
                if (point.pointDoorExtIn.Value != 0f) point.pointDoorExtIn.Value = 0f;
            }
        }

        private static void FitSavedOil(SimulationFlow __instance, ref JObject savedData)
        {
            try
            {
                var points = Points(__instance);
                if (savedData == null || points == null || points.oilingPoints.Length <= BigEndOilingPlan.NativePoints) return;
                if (!(savedData[points.id] is JObject component) || !(component["oilingPoints"] is JArray saved)) return;
                if (saved.Count == points.oilingPoints.Length) return;
                var fitted = (JObject)savedData.DeepClone();
                ((JObject)fitted[points.id])["oilingPoints"] = BigEndOilingPlan.FitSavedPoints(saved, points.oilingPoints.Length);
                savedData = fitted;
            }
            catch (Exception ex) { Main.Logger.Warning("Saved oil levels could not be fitted to the big-end cups: " + ex.Message); }
        }
    }
}
#endif
