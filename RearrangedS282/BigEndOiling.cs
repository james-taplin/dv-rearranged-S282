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
    // they never drain, damage or lower the oil gauge. An optional, default-off
    // assistance setting extends that maintenance to active cups on owned S282s.
    // Older saves are fitted to
    // the extended point list instead of being discarded by the native exact-length
    // check.
    internal static class BigEndOiling
    {
        private sealed class Extension { internal ManualOilingPointsDefinition.OilingPointDefinition[] Points; }
        private sealed class State { internal bool[] Active; internal ManualOilingPoints.OilingPoint[] Points; }
        private static readonly ConditionalWeakTable<ManualOilingPointsDefinition, Extension> Extensions = new ConditionalWeakTable<ManualOilingPointsDefinition, Extension>();
        private static readonly ConditionalWeakTable<ManualOilingPoints, State> States = new ConditionalWeakTable<ManualOilingPoints, State>();
        internal const string PortSuffix = "_RSBIG";

        internal static void Register(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(SimController), nameof(SimController.Initialize)),
                prefix: new HarmonyMethod(typeof(BigEndOiling), nameof(ExtendBeforeInitialize)));
            harmony.Patch(AccessTools.Method(typeof(ManualOilingPointsDefinition), nameof(ManualOilingPointsDefinition.InstantiateImplementation)),
                postfix: new HarmonyMethod(typeof(BigEndOiling), nameof(RegisterOwnedPoints)));
            harmony.Patch(AccessTools.Method(typeof(ManualOilingPoints), nameof(ManualOilingPoints.Tick)),
                postfix: new HarmonyMethod(typeof(BigEndOiling), nameof(HoldUnusedFull)));
            harmony.Patch(AccessTools.Method(typeof(ManualOilingPoints), nameof(ManualOilingPoints.SetSaveStateData)),
                prefix: new HarmonyMethod(typeof(BigEndOiling), nameof(FitSavedOil)));
        }

        private static bool IsS282(TrainCar car) => S282Target.IsNormal(car) &&
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
                ExtendDefinition(definition);
            }
            catch (Exception ex) { Main.Logger.Warning("Big-end oiling points could not be added; native cups only: " + ex.Message); }
        }

        private static void ExtendDefinition(ManualOilingPointsDefinition definition)
        {
            if (definition.oilingPoints?.Length != BigEndOilingPlan.NativePoints) return;
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
            Extensions.Remove(definition);
            Extensions.Add(definition, new Extension { Points = definition.oilingPoints });
        }

        // Native construction happens before the flow can load saved state. Cup
        // count is never proof of ownership: only definitions extended above qualify.
        private static void RegisterOwnedPoints(ManualOilingPointsDefinition __instance, SimComponent __result)
        {
            if (__instance is null || !Extensions.TryGetValue(__instance, out var extension) ||
                !ReferenceEquals(extension.Points, __instance.oilingPoints) ||
                !(__result is ManualOilingPoints points) || points.id != __instance.ID ||
                points.oilingPoints?.Length != BigEndOilingPlan.TotalPoints) return;
            for (int i = BigEndOilingPlan.NativePoints; i < extension.Points.Length; i++)
            {
                var expected = extension.Points[i];
                var actual = points.oilingPoints[i];
                bool Matches(Port port, PortDefinition definition) => port != null && definition != null &&
                    definition.ID != null && definition.ID.EndsWith(PortSuffix + i, StringComparison.Ordinal) &&
                    port.id == SimConsts.GetFullId(points.id, definition.ID);
                if (expected == null || actual == null || !Matches(actual.oilLevelReadOut, expected.oilLevelReadOut) ||
                    !Matches(actual.oilLevelNormalizedReadOut, expected.oilLevelNormalizedReadOut) ||
                    !Matches(actual.pointDoorExtIn, expected.pointDoorExtIn) ||
                    !Matches(actual.refillExtIn, expected.refillExtIn) ||
                    !Matches(actual.refillingFlowNormalizedReadOut, expected.refillingFlowNormalizedReadOut)) return;
            }
            States.Remove(points);
            States.Add(points, new State { Points = points.oilingPoints });
        }

        private static bool TryGetOwnedState(ManualOilingPoints points, out State state)
        {
            state = null;
            return points != null && States.TryGetValue(points, out state) &&
                ReferenceEquals(state.Points, points.oilingPoints);
        }

        internal static ManualOilingPoints Points(SimulationFlow flow) =>
            flow?.OrderedSimComps?.OfType<ManualOilingPoints>().FirstOrDefault(p => TryGetOwnedState(p, out _));

        // Which points the current arrangement uses; unsupported visual cups are inactive.
        internal static void SetActive(SimulationFlow flow, bool[] active)
        {
            var points = Points(flow);
            if (!TryGetOwnedState(points, out var state)) return;
            state.Active = active;
            HoldUnusedFull(points);
        }

        private static void HoldUnusedFull(ManualOilingPoints __instance)
        {
            if (!MultiplayerSync.IsSimulationAuthority) return;
            if (!TryGetOwnedState(__instance, out var state)) return;
            var points = state.Points;
            bool assist = Main.settings?.automaticCupOiling == true;
            for (int i = assist || state?.Active != null ? 0 : BigEndOilingPlan.NativePoints; i < points.Length; i++)
            {
                if (!assist && state?.Active != null && i < state.Active.Length && state.Active[i]) continue;
                var point = points[i];
                point.oilLevelReadOut.Value = __instance.capacity;
                point.oilLevelNormalizedReadOut.Value = 1f;
                point.refillingFlowNormalizedReadOut.Value = 0f;
                if (point.pointDoorExtIn.Value != 0f) point.pointDoorExtIn.Value = 0f;
            }
        }

        private static void FitSavedOil(ManualOilingPoints __instance, ref JObject __0)
        {
            try
            {
                if (__0 == null || !TryGetOwnedState(__instance, out _)) return;
                if (!(__0["oilingPoints"] is JArray saved)) return;
                // Preserve every shipped index while appending the four extra
                // points. Unknown lengths are left to the native exact-length
                // loader rather than truncating or guessing another mod's data.
                if (saved.Count != BigEndOilingPlan.NativePoints && saved.Count != BigEndOilingPlan.LegacyTotalPoints) return;
                var fitted = (JObject)__0.DeepClone();
                fitted["oilingPoints"] = BigEndOilingPlan.FitSavedPoints(saved, __instance.oilingPoints.Length);
                __0 = fitted;
            }
            catch (Exception ex) { Main.Logger.Warning("Saved oil levels could not be fitted to the big-end cups: " + ex.Message); }
        }
    }
}
#endif
