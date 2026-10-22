using System;

namespace RearrangedS282
{
    // Native mean-force comparison and design calibration; no direct force override.
    internal static class TractiveEffortModel
    {
        internal const double ReferencePressureBarAbs = 15d, ReferenceCutoff = .75d,
            ReferenceTemperatureC = 300d, Gravity = 9.81d;

        // Explicit design assumptions, not a claim about every historical class.
        // Nominal TE uses 85% of boiler gauge pressure; FA = adhesive weight / TE.
        internal const double NominalPressureFraction = .85d;
        // A modest capacity ceiling, not a requested final FoA or a power bonus.
        // Larger coupled groups lean conservative; large wheels/light driver
        // loads retain a little more starting demand. Extra cylinders share the
        // existing phase/slip group: no assumed torque-smoothing advantage.
        internal static double MinimumDesignFactorOfAdhesion(int poweredAxles, int cylinders,
            double driverDiameterM, double adhesiveMassKg)
        {
            if (poweredAxles <= 0 || cylinders <= 0 || !Finite(driverDiameterM) || driverDiameterM <= 0d ||
                !Finite(adhesiveMassKg) || adhesiveMassKg <= 0d)
                throw new ArgumentException("Invalid machinery for nominal adhesion design.");
            double group = Math.Max(-.4d, Math.Min(.6d, .2d * (poweredAxles - 4)));
            double diameter = Math.Max(-.1d, Math.Min(.1d, -.15d * (driverDiameterM / 1.424d - 1d)));
            double axleLoad = Math.Max(-.05d, Math.Min(.05d, .1d * (adhesiveMassKg / poweredAxles / 20000d - 1d)));
            double cylindersAllowance = Math.Min(.1d, .025d * Math.Max(0, cylinders - 2));
            return Math.Max(3.3d, Math.Min(4.65d, 3.9d + group + diameter + axleLoad + cylindersAllowance));
        }
        internal static double ReferenceToNominalForceRatio =>
            (ReferencePressureBarAbs * MeanPressureRatio(ReferenceCutoff, ReferenceTemperatureC).Value - 1d) /
            (NominalPressureFraction * (ReferencePressureBarAbs - 1d));

        internal static bool Finite(double x) => !double.IsNaN(x) && !double.IsInfinity(x);

        // B99 ReciprocatingSteamEngine.MeanEffectivePressureRatio, with an
        // explicit zero-cutoff limit to avoid 1/0 and 0*infinity in diagnostics.
        internal static double? MeanPressureRatio(double cutoff, double temperatureC)
        {
            if (!Finite(cutoff) || cutoff < 0d || cutoff > 1d || !Finite(temperatureC) || temperatureC <= -273.15d) return null;
            if (cutoff == 0d) return 0d;
            double expansion = Math.Pow((temperatureC + 273.15d) / 380d, 2.9999998d);
            double ratio = 1d / cutoff;
            double result = ratio <= expansion ? cutoff * (1d + Math.Log(ratio))
                : cutoff * (1d + (1d - cutoff) * Math.Log(expansion) + cutoff * Math.Log(ratio));
            return Finite(result) ? result : (double?)null;
        }

        internal static double? MeanForceN(int cylinders, double areaM2, double strokeM,
            double radiusM, double pressureBarAbs, double cutoff, double temperatureC)
        {
            if (cylinders <= 0 || !Finite(areaM2) || areaM2 <= 0d || !Finite(strokeM) || strokeM <= 0d ||
                !Finite(radiusM) || radiusM <= 0d || !Finite(pressureBarAbs) || pressureBarAbs < 0d) return null;
            double? ratio = MeanPressureRatio(cutoff, temperatureC);
            if (!ratio.HasValue) return null;
            double force = Math.Max(0d, pressureBarAbs * ratio.Value - 1d) * 1e5d * areaM2 *
                (2d / Math.PI) * (strokeM / 2d) * cylinders / radiusM;
            return Finite(force) ? force : (double?)null;
        }

        internal static double? DryGripN(double massKg, int totalAxles, int poweredAxles, double coefficient)
        {
            if (totalAxles <= 0 || poweredAxles < 0 || poweredAxles > totalAxles || !Finite(massKg) || massKg <= 0d ||
                !Finite(coefficient) || coefficient < 0d) return null;
            double force = coefficient * massKg / totalAxles * Gravity * poweredAxles;
            return Finite(force) ? force : (double?)null;
        }
    }
}
