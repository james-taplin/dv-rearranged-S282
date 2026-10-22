#if B99_ALL_STANDARD
using System;
using System.Linq;
using System.Reflection;
using LocoSim.Implementations;

namespace RearrangedS282
{
    // Per-car native engine instances only. No definitions, ports, stroke, chest
    // state, boiler, event counters or flow limits are rewritten.
    internal sealed class CylinderCalibration
    {
        private static readonly FieldInfo Area = Field("pistonArea"), Volume = Field("cylinderVolume");
        private sealed class Snapshot
        {
            internal ReciprocatingSteamEngine Engine;
            internal float Bore, Stroke, Area, Volume;
            internal int Cylinders;
        }
        private readonly Snapshot[] stock;
        private readonly double originalCoefficient;
        internal float AreaScale { get; private set; } = 1f;
        internal double ReferenceGripN { get; private set; }
        internal double TargetRatio { get; private set; }
        internal double BeforeForceN { get; private set; }
        internal double AfterForceN { get; private set; }
        internal float TenderAreaScale { get; private set; } = 1f;
        internal double MinimumFactorOfAdhesion { get; private set; }
        internal double TenderMinimumFactorOfAdhesion { get; private set; }
        internal const float TenderDesignMassKg = 50000f, TenderDesignDriverShare = .8f;

        private static FieldInfo Field(string name) => typeof(ReciprocatingSteamEngine).GetField(name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? throw new MissingFieldException(name);

        internal CylinderCalibration(ReciprocatingSteamEngine[] engines, float radius, float coefficient, int powered, int axles)
        {
            if (engines == null || engines.Length == 0 || !(radius > 0) || !(coefficient > 0) || powered <= 0 || axles < powered)
                throw new ArgumentException("Native cylinder calibration baseline is unavailable.");
            originalCoefficient = coefficient;
            stock = engines.Select(e => new Snapshot { Engine = e, Bore = e.cylinderBore, Stroke = e.pistonStroke,
                Cylinders = e.numCylinders, Area = (float)Area.GetValue(e), Volume = (float)Volume.GetValue(e) }).ToArray();
            if (stock.Any(s => !(s.Bore > 0f) || !(s.Area > 0f) || !(s.Volume > 0f))) throw new InvalidOperationException("Invalid native cylinder dimensions.");
        }

        private static double Force(Snapshot s, double radius) => TractiveEffortModel.MeanForceN(s.Cylinders,
            s.Area, s.Stroke, radius, TractiveEffortModel.ReferencePressureBarAbs,
            TractiveEffortModel.ReferenceCutoff, TractiveEffortModel.ReferenceTemperatureC)
            ?? throw new InvalidOperationException("Native reference force is unavailable.");

        internal void Apply(ReciprocatingSteamEngine[] activeLocomotiveEngines, float radius, float massKg, float driverShare, int poweredAxles)
        {
            Restore();
            if (activeLocomotiveEngines == null || activeLocomotiveEngines.Length == 0) return;
            if (!(massKg > 0f) || !(radius > 0f) || !(driverShare > 0f && driverShare <= 1f)) throw new ArgumentException("Invalid load/radius for cylinder calibration.");
            var selected = activeLocomotiveEngines.Select(e => stock.Single(s => ReferenceEquals(s.Engine, e))).ToArray();
            if (selected.Distinct().Count() != selected.Length) throw new ArgumentException("Duplicate native engine in calibration.");
            double designMass = ConventionalPhysics.ReferenceOperatingMassKg;
            // Convert historical-style nominal TE to the SAME native reference
            // conditions used by Force(). Comparing the two without this conversion
            // would confuse gauge/absolute pressure and nominal/mean pressure.
            MinimumFactorOfAdhesion = TractiveEffortModel.MinimumDesignFactorOfAdhesion(poweredAxles,
                selected.Sum(s => s.Cylinders), 2d * radius, designMass * driverShare);
            double designRatio = TractiveEffortModel.ReferenceToNominalForceRatio /
                (MinimumFactorOfAdhesion * originalCoefficient);
            // Stock is a comparison reference, not a lower FoA bound. The
            // original-cylinder limit below still prevents artificial boosting.
            TargetRatio = designRatio;
            ReferenceGripN = originalCoefficient * designMass * TractiveEffortModel.Gravity * driverShare;
            BeforeForceN = selected.Sum(s => Force(s, radius));
            // First pass only reduces excessive ideal pull; never boosts engine
            // capacity. Keep auxiliary/main area ratios, including small auxiliaries.
            AreaScale = (float)Math.Min(1d, TargetRatio * ReferenceGripN / BeforeForceN);
            if (!(AreaScale > 0f) || !TractiveEffortModel.Finite(AreaScale)) throw new InvalidOperationException("Invalid cylinder capacity scale.");
            try
            {
                foreach (var s in selected)
                {
                    float bore = s.Bore * (float)Math.Sqrt(AreaScale);
                    float area = (float)Math.PI / 4f * bore * bore;
                    s.Engine.cylinderBore = bore;
                    Area.SetValue(s.Engine, area);
                    Volume.SetValue(s.Engine, area * s.Stroke * 1000f);
                }
                AfterForceN = BeforeForceN * AreaScale;
            }
            catch { Restore(); throw; }
        }

        internal bool Consistent => stock.All(s => s.Engine.numCylinders == s.Cylinders && s.Engine.pistonStroke == s.Stroke &&
            Math.Abs((float)Area.GetValue(s.Engine) - (float)Math.PI / 4f * s.Engine.cylinderBore * s.Engine.cylinderBore) < .000001f &&
            Math.Abs((float)Volume.GetValue(s.Engine) - (float)Area.GetValue(s.Engine) * s.Engine.pistonStroke * 1000f) < .001f);

        internal void ApplyTender(ReciprocatingSteamEngine engine, float radius)
        {
            if (engine == null || !(radius > 0f) || !TractiveEffortModel.Finite(radius))
                throw new ArgumentException("Invalid tender calibration engine/radius.");
            var s = stock.Single(x => ReferenceEquals(x.Engine, engine));
            TenderMinimumFactorOfAdhesion = TractiveEffortModel.MinimumDesignFactorOfAdhesion(4, s.Cylinders,
                2d * radius, TenderDesignMassKg * TenderDesignDriverShare);
            double nominalTarget = TenderDesignMassKg * TenderDesignDriverShare * TractiveEffortModel.Gravity /
                TenderMinimumFactorOfAdhesion;
            TenderAreaScale = (float)Math.Min(1d, nominalTarget * TractiveEffortModel.ReferenceToNominalForceRatio / Force(s, radius));
            try
            {
                float bore = s.Bore * (float)Math.Sqrt(TenderAreaScale);
                float area = (float)Math.PI / 4f * bore * bore;
                engine.cylinderBore = bore;
                Area.SetValue(engine, area);
                Volume.SetValue(engine, area * s.Stroke * 1000f);
            }
            catch { Restore(); throw; }
        }

        internal void Restore()
        {
            foreach (var s in stock) { s.Engine.cylinderBore = s.Bore; Area.SetValue(s.Engine, s.Area); Volume.SetValue(s.Engine, s.Volume); }
            AreaScale = 1f; BeforeForceN = AfterForceN = ReferenceGripN = TargetRatio = 0d;
            TenderAreaScale = 1f;
            MinimumFactorOfAdhesion = TenderMinimumFactorOfAdhesion = 0d;
        }
    }
}
#endif
