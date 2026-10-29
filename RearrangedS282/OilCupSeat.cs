#if B99_ALL_STANDARD
using System;
using UnityEngine;

namespace RearrangedS282
{
    // Vertical intersection with the actual bearing mesh, not its bounding box.
    internal static class OilCupSeat
    {
        // Coupling rods may be the native one-piece mesh or mirrored segments.
        // Only inspect the rod itself and its named direct segment children;
        // main rods, valve gear and hidden LODs are not mounting surfaces.
        internal static bool TryCouplingTop(Transform rod, Vector3 pinWorld, Mesh nativeReadable, out Vector3 seatWorld)
        {
            seatWorld = default;
            // The connecting rod sits outside the coupling rod. A cup on its
            // own crank must keep that rod's authored mount, including on a
            // rear engine; using the coupling bearing can bury it in the big end.
            var main = rod.Find("s282_mech_push_rod_to_connect");
            var oil = main != null ? main.Find("Oiling2") : null;
            if (oil != null && main.gameObject.activeInHierarchy &&
                Math.Abs(rod.InverseTransformPoint(pinWorld).z - main.localPosition.z) < .025f)
            { seatWorld = oil.position; return true; }
            float nearest = float.PositiveInfinity;
            for (int i = -1; i < rod.childCount; i++)
            {
                var part = i < 0 ? rod : rod.GetChild(i);
                bool segment = part.name.StartsWith("s282_mech_wheels_connect", StringComparison.Ordinal) ||
                    part.name == "[Rearranged two-axle rod]" || part.name == "[Rearranged seven-axle rod]";
                if (i >= 0 && (!segment ||
                    part.name.Contains("LOD"))) continue;
                if (!part.gameObject.activeInHierarchy || part.GetComponent<MeshRenderer>()?.enabled != true) continue;
                var mesh = part.GetComponent<MeshFilter>()?.sharedMesh;
                if (mesh == null) continue;
                if (!mesh.isReadable) mesh = i < 0 ? nativeReadable : null;
                if (mesh == null || !mesh.isReadable) continue;
                var local = part.InverseTransformPoint(pinWorld);
                local.x = mesh.bounds.center.x;
                bool localTop = Vector3.Dot(part.TransformVector(Vector3.up), rod.TransformVector(Vector3.up)) >= 0f;
                if (!TrySurface(mesh.vertices, mesh.triangles, local.x, local.z, localTop, out var top)) continue;
                local.y = top;
                var seat = part.TransformPoint(local);
                float distance = (seat - pinWorld).sqrMagnitude;
                if (distance < nearest) { nearest = distance; seatWorld = seat; }
            }
            return !float.IsInfinity(nearest);
        }

        internal static bool TryTop(Vector3[] vertices, int[] triangles, float x, float z, out float top)
            => TrySurface(vertices, triangles, x, z, true, out top);

        private static bool TrySurface(Vector3[] vertices, int[] triangles, float x, float z, bool upper, out float top)
        {
            top = upper ? float.NegativeInfinity : float.PositiveInfinity;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                var a = vertices[triangles[i]]; var b = vertices[triangles[i + 1]]; var c = vertices[triangles[i + 2]];
                float det = (b.z - c.z) * (a.x - c.x) + (c.x - b.x) * (a.z - c.z);
                if (Math.Abs(det) < 1e-9f) continue;
                float u = ((b.z - c.z) * (x - c.x) + (c.x - b.x) * (z - c.z)) / det;
                float v = ((c.z - a.z) * (x - c.x) + (a.x - c.x) * (z - c.z)) / det;
                float w = 1f - u - v;
                if (u < -1e-5f || v < -1e-5f || w < -1e-5f) continue;
                float hit = u * a.y + v * b.y + w * c.y;
                top = upper ? Math.Max(top, hit) : Math.Min(top, hit);
            }
            return !float.IsInfinity(top) && !float.IsNaN(top);
        }
    }
}
#endif
