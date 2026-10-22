using System;
using System.Collections.Generic;

namespace RearrangedS282
{
    // Scoped presentation pitch, expressed in metres along the locomotive.
    // Driver 3 remains the native main axle; this does not change wheel size,
    // crank throw, cylinder dimensions or any simulation definition.
    internal sealed class DriverLayout
    {
        internal const float DefaultExtraPitchM = .15f, MaximumExtraPitchM = .20f;
        internal const float MaximumPrairieExtraPitchM = .22f;
        internal const int MainDriverNumber = 3;
        private readonly IReadOnlyList<int> drivers;

        internal int ArrangementId { get; }
        internal float ExtraCoupledPitchM { get; }
        internal IReadOnlyList<int> Drivers => drivers;
        internal bool ThreeDrivers => IsThreeDriverId(ArrangementId);
        internal static bool Supports(int id) => id == 34 || id == 35 || id == 0 || id == 44;
        internal static bool IsThreeDriverId(int id) => id == 34 || id == 44;

        private DriverLayout(int id, float pitch)
        {
            ArrangementId = id;
            ExtraCoupledPitchM = pitch;
            drivers = Array.AsReadOnly(ThreeDrivers ? new[] { 2, 3, 4 } : new[] { 2, 3 });
        }

        internal static DriverLayout Create(int id, float requestedPitchM)
        {
            if (!Supports(id)) throw new ArgumentOutOfRangeException(nameof(id));
            return new DriverLayout(id, ClampExtraPitch(requestedPitchM, id));
        }

        internal static float ClampExtraPitch(float requestedPitchM, int id = -1)
        {
            if (float.IsNaN(requestedPitchM) || float.IsInfinity(requestedPitchM)) requestedPitchM = DefaultExtraPitchM;
            return Math.Max(0f, Math.Min(id == 44 ? MaximumPrairieExtraPitchM : MaximumExtraPitchM,
                (float)Math.Round(requestedPitchM * 100d, MidpointRounding.AwayFromZero) / 100f));
        }

        internal float DriverOffsetM(int number)
        {
            if (number == 2) return ExtraCoupledPitchM;
            if (number == 3) return 0f;
            if (number == 4 && ThreeDrivers) return -ExtraCoupledPitchM;
            throw new ArgumentOutOfRangeException(nameof(number));
        }

        internal float[] OriginalRodPins()
            // Measured bore centres of the original generated meshes, rather
            // than the rounded coordinates used by their historical recipes.
            => ThreeDrivers ? new[] { -3.105580162f, -1.55294514f }
                : new[] { -.169580162f, 1.69832514f };
    }
}
