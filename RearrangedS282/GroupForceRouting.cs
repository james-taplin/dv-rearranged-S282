#if B99_ALL_STANDARD
// Force allocation only: callers own adhesion limits, rigidbodies and lifecycle.
using System;

namespace RearrangedS282
{
    internal static class GroupForceRouting
    {
        // A group may span multiple native bogies; groups may share a bogie.
        // axleCounts[g,b] records that group's physical driven axles on that
        // bogie. Exactly one input force is apportioned among those contacts.
        public static float[] Distribute(float[] forces, int[,] axleCounts)
        {
            if (forces == null || axleCounts == null || axleCounts.GetLength(0) != forces.Length || axleCounts.GetLength(1) == 0)
                throw new ArgumentException("Group/bogie dimensions do not match.");
            var output = new float[axleCounts.GetLength(1)];
            for (int group = 0; group < forces.Length; group++)
            {
                float force = forces[group];
                if (float.IsNaN(force) || float.IsInfinity(force)) throw new ArgumentException("Non-finite force.");
                int count = 0;
                for (int bogie = 0; bogie < output.Length; bogie++)
                {
                    int n = axleCounts[group, bogie];
                    if (n < 0) throw new ArgumentException("Negative axle count.");
                    count = checked(count + n);
                }
                if (count == 0) throw new ArgumentException("A powered group has no contact bogie.");
                for (int bogie = 0; bogie < output.Length; bogie++)
                    output[bogie] += force * ((float)axleCounts[group, bogie] / count);
            }
            return output;
        }
    }
}

#endif
