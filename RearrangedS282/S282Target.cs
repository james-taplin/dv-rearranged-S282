using System;
using DV;

namespace RearrangedS282
{
    // The native livery registry distinguishes the ordinary S282 from other
    // heavy-steam vehicles; unique/garage vehicles are deliberately excluded.
    internal static class S282Target
    {
        [ThreadStatic] internal static bool UniqueSpawnInProgress;

        internal static bool IsNormal(TrainCar car)
        {
            if (car == null || car.uniqueCar || UniqueSpawnInProgress || car.carLivery == null) return false;
            var types = Globals.G?.Types;
            return types != null && types.TryGetLivery("LocoS282A", out var normal) &&
                ReferenceEquals(car.carLivery, normal);
        }
    }
}
