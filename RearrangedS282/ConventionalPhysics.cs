using System;

namespace RearrangedS282
{
    // Derailroader's G29/C21 convention: DV allocates equal weight to each axle.
    // These are parameter conversions for the native simulator, not a new force model.
    public static class ConventionalPhysics
    {
        // Rounded normal working S282 mass (111 t body plus ~13 t resources).
        // Arrangement dimensions and rated adhesive shares must not depend on
        // how much water happens to be aboard when the radio is used.
        public const float ReferenceOperatingMassKg = 124000f;
        public static float RpmPerMetrePerSecond(float radius)
        {
            Positive(radius, nameof(radius));
            return 60f / (2f * (float)Math.PI * radius);
        }

        public static float FrictionForDriverWeight(float coefficient, float driverWeightFraction, int poweredAxles, int totalAxles)
        {
            Positive(coefficient, nameof(coefficient));
            Positive(driverWeightFraction, nameof(driverWeightFraction));
            if (driverWeightFraction > 1f || poweredAxles <= 0 || totalAxles < poweredAxles || totalAxles > 255)
                throw new ArgumentOutOfRangeException(nameof(totalAxles));
            return coefficient * (driverWeightFraction / ((float)poweredAxles / totalAxles));
        }

        // DV spreads the whole 111 t loco over its axles, so short layouts put
        // 28-37 t on each driver. Cap the driven-axle load by coupled group size
        // (single, four- and six-coupled; larger groups uncapped). Only ever
        // lowers the equal-load fraction. Articulated groups are capped per engine.
        public static float MaxDrivenAxleLoadKg(int coupledAxles)
        {
            if (coupledAxles == 1) return 18000f;
            if (coupledAxles == 2) return 19000f;
            if (coupledAxles == 3) return 20000f;
            return float.PositiveInfinity;
        }

        public static float DriverWeightFraction(int[] coupledGroups, int totalAxles, float locoMassKg)
        {
            Positive(locoMassKg, nameof(locoMassKg));
            int powered = 0; float cap = 0f;
            foreach (int group in coupledGroups)
            {
                if (group < 0) throw new ArgumentOutOfRangeException(nameof(coupledGroups));
                powered += group;
                cap += group * MaxDrivenAxleLoadKg(group);
            }
            if (totalAxles <= 0 || totalAxles < powered || totalAxles > 255)
                throw new ArgumentOutOfRangeException(nameof(totalAxles));
            float equal = (float)powered / totalAxles;
            return powered == 0 ? 0f : Math.Min(equal, cap / locoMassKg);
        }

        // Preliminary effective adhesive load, implemented through native friction
        // conversion, not Rigidbody mass/CoM. Caps override family preferences.
        public static float TargetDriverWeightFraction(int[] wheels, float totalMassKg, bool articulated)
        {
            Positive(totalMassKg, nameof(totalMassKg));
            if (wheels == null || wheels.Length < 3 || wheels[0] < 0 || wheels[wheels.Length - 1] < 0)
                throw new ArgumentException("Invalid wheel arrangement.");
            int powered = 0, total = 0; float cap = 0f;
            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i] < 0 || wheels[i] % 2 != 0) throw new ArgumentException("Wheel counts must be nonnegative pairs.");
                total += wheels[i] / 2;
                if (i == 0 || i == wheels.Length - 1) continue;
                int group = wheels[i] / 2;
                powered += group;
                // A zero-driver novelty has no adhesive load: avoid 0 * infinity.
                if (group > 0) cap += group * Math.Min(32000f, MaxDrivenAxleLoadKg(group));
            }
            if (total <= 0 || total > 255) throw new ArgumentException("Invalid axle count.");
            if (powered == 0) return 0f;
            int leading = wheels[0], trailing = wheels[wheels.Length - 1];
            float target = articulated ? (float)powered / total
                : leading == 0 && trailing == 0 ? 1f
                : trailing == 0 ? (leading >= 4 && powered == 3 ? .75f : powered <= 3 ? .85f : .88f)
                : trailing == 2 ? .73f : .63f;
            return Math.Min(target, Math.Min(1f, cap / totalMassKg));
        }

        private static void Positive(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
