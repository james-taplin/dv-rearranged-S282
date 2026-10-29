#if B99_ALL_STANDARD
using System;
using HarmonyLib;

namespace RearrangedS282
{
    // Awake builds the simulation during Instantiate, before CarSpawner assigns
    // TrainCar.uniqueCar. Read the native spawn argument for that earlier phase.
    internal static class S282SpawnScope
    {
        private sealed class Scope { internal bool Previous; }

        internal static void Register(Harmony harmony)
        {
            foreach (var method in new[] { "BaseSpawn", nameof(CarSpawner.SpawnLoadedCar), nameof(CarSpawner.SpawnLoadedCarAtNearestAvailableSpace) })
                harmony.Patch(AccessTools.Method(typeof(CarSpawner), method),
                    prefix: new HarmonyMethod(typeof(S282SpawnScope), nameof(Enter)),
                    finalizer: new HarmonyMethod(typeof(S282SpawnScope), nameof(Exit)));
        }

        private static void Enter(bool uniqueCar, out Scope __state)
        {
            __state = new Scope { Previous = S282Target.UniqueSpawnInProgress };
            S282Target.UniqueSpawnInProgress = uniqueCar;
        }

        private static Exception Exit(Scope __state, Exception __exception)
        {
            if (__state != null) S282Target.UniqueSpawnInProgress = __state.Previous;
            return __exception;
        }
    }
}
#endif
