#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RearrangedS282
{
    // Read-only measurements of authored presentation geometry. Neither the
    // native pipe nor the cylinder mesh is changed by these helpers.
    internal static class SteamPipeNativeFit
    {
        internal sealed class Section
        {
            // One closed loop, without a repeated final point. UV is sampled
            // from each outgoing edge; edge UVs retain any authored seam.
            internal Vector3[] Points;
            internal Vector2[] UV, EdgeStartUV, EdgeEndUV;
            internal Vector3 Centre;
            internal float Area;
        }

        internal readonly struct Fit
        {
            internal readonly Vector3 Contact;
            internal readonly float CutY;
            internal readonly Section Profile;
            internal Fit(Vector3 contact, float cutY, Section profile)
            { Contact = contact; CutY = cutY; Profile = profile; }
        }

        private sealed class Node
        {
            internal Vector3 Point;
            internal readonly List<int> Edges = new List<int>();
        }
        private readonly struct Edge
        {
            internal readonly int A, B;
            internal readonly Vector2 UA, UB;
            internal Edge(int a, int b, Vector2 ua, Vector2 ub)
            { A = a; B = b; UA = ua; UB = ub; }
        }

        private const float JoinTolerance = .00002f;

        internal static bool TrySection(Vector3[] vertices, int[] triangles, Vector2[] uv,
            float y, out Section section)
        {
            section = null;
            if (!Valid(vertices, triangles) || !Finite(y)) return false;
            bool textured = uv != null && uv.Length == vertices.Length;
            var nodes = new List<Node>(); var edges = new List<Edge>();
            var cells = new Dictionary<(int, int, int), List<int>>();
            var used = new HashSet<(int, int)>();
            int NodeAt(Vector3 p)
            {
                int x = (int)Math.Floor(p.x / JoinTolerance),
                    yy = (int)Math.Floor(p.y / JoinTolerance), z = (int)Math.Floor(p.z / JoinTolerance);
                for (int a = -1; a <= 1; a++) for (int b = -1; b <= 1; b++) for (int c = -1; c <= 1; c++)
                    if (cells.TryGetValue((x + a, yy + b, z + c), out var near))
                        foreach (int id in near)
                            if ((nodes[id].Point - p).sqrMagnitude <= JoinTolerance * JoinTolerance) return id;
                int next = nodes.Count; nodes.Add(new Node { Point = p });
                if (!cells.TryGetValue((x, yy, z), out var bucket)) cells.Add((x, yy, z), bucket = new List<int>());
                bucket.Add(next); return next;
            }
            for (int i = 0; i < triangles.Length; i += 3)
            {
                var hits = new List<(Vector3 p, Vector2 uv)>();
                for (int e = 0; e < 3; e++)
                {
                    int ia = triangles[i + e], ib = triangles[i + (e + 1) % 3];
                    var a = vertices[ia]; var b = vertices[ib];
                    if ((a.y < y) == (b.y < y) || Math.Abs(a.y - b.y) < 1e-9f) continue;
                    float t = (y - a.y) / (b.y - a.y);
                    var p = a + (b - a) * t;
                    var texture = textured ? uv[ia] + (uv[ib] - uv[ia]) * t : Vector2.zero;
                    if (!hits.Any(h => (h.p - p).sqrMagnitude <= 1e-14f)) hits.Add((p, texture));
                }
                if (hits.Count != 2) continue;
                int first = NodeAt(hits[0].p), last = NodeAt(hits[1].p);
                if (first == last || !used.Add((Math.Min(first, last), Math.Max(first, last)))) continue;
                int edge = edges.Count;
                edges.Add(new Edge(first, last, hits[0].uv, hits[1].uv));
                nodes[first].Edges.Add(edge); nodes[last].Edges.Add(edge);
            }
            var visited = new HashSet<int>();
            for (int start = 0; start < nodes.Count; start++)
            {
                if (visited.Contains(start)) continue;
                var component = new List<int>(); var pending = new Queue<int>();
                pending.Enqueue(start); visited.Add(start);
                while (pending.Count != 0)
                {
                    int node = pending.Dequeue(); component.Add(node);
                    foreach (int id in nodes[node].Edges)
                    {
                        int other = edges[id].A == node ? edges[id].B : edges[id].A;
                        if (visited.Add(other)) pending.Enqueue(other);
                    }
                }
                if (component.Count < 3 || component.Any(n => nodes[n].Edges.Count != 2)) continue;
                var points = new List<Vector3>(); var outgoing = new List<Vector2>(); var incoming = new List<Vector2>();
                int current = start, previousEdge = -1;
                do
                {
                    points.Add(nodes[current].Point);
                    int edgeId = nodes[current].Edges.First(e => e != previousEdge);
                    var edge = edges[edgeId]; bool forward = edge.A == current;
                    outgoing.Add(forward ? edge.UA : edge.UB); incoming.Add(forward ? edge.UB : edge.UA);
                    current = forward ? edge.B : edge.A; previousEdge = edgeId;
                } while (current != start && points.Count <= component.Count);
                if (current != start || points.Count != component.Count) continue;
                float signedArea = 0f;
                for (int i = 0; i < points.Count; i++)
                { var a = points[i]; var b = points[(i + 1) % points.Count]; signedArea += a.x * b.z - b.x * a.z; }
                float area = Math.Abs(signedArea) * .5f;
                if (!(area > 1e-7f) || section != null && area <= section.Area) continue;
                // Start at an authored UV seam when present. The separate edge
                // arrays retain other atlas discontinuities without averaging.
                int seam = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    var delta = outgoing[i] - incoming[(i + points.Count - 1) % points.Count];
                    if (delta.x * delta.x + delta.y * delta.y > 1e-8f) { seam = i; break; }
                }
                T[] Rotate<T>(List<T> values) => Enumerable.Range(0, values.Count).Select(i => values[(i + seam) % values.Count]).ToArray();
                var ordered = Rotate(points); var starts = Rotate(outgoing); var ends = Rotate(incoming);
                section = new Section { Points = ordered, UV = starts, EdgeStartUV = starts, EdgeEndUV = ends,
                    Centre = new Vector3((ordered.Min(p => p.x) + ordered.Max(p => p.x)) * .5f, y,
                        (ordered.Min(p => p.z) + ordered.Max(p => p.z)) * .5f), Area = area };
            }
            return section != null;
        }

        internal static bool TryFind(Vector3[] nativeVertices, int[] nativeTriangles,
            Vector3[] casingVertices, int[] casingTriangles, out Fit fit, float seatDepth = .02f)
        {
            fit = default;
            if (!Valid(nativeVertices, nativeTriangles) || !Valid(casingVertices, casingTriangles) ||
                !Finite(seatDepth) || seatDepth < 0f) return false;
            float high = nativeVertices.Max(p => p.y) - .0001f, low = nativeVertices.Min(p => p.y) + .0001f;
            bool previous = false; float previousY = high;
            for (float y = high; y >= low; y -= .01f)
            {
                if (!Measure(nativeVertices, nativeTriangles, y, out var centre) ||
                    !Top(casingVertices, casingTriangles, centre, out float top))
                { previous = false; continue; }
                float gap = y - top;
                if (gap <= 0f && previous)
                {
                    float a = y, b = previousY;
                    for (int i = 0; i < 18; i++)
                    {
                        float middle = (a + b) * .5f;
                        if (!Measure(nativeVertices, nativeTriangles, middle, out var probe) ||
                            !Top(casingVertices, casingTriangles, probe, out float height)) return false;
                        if (middle > height) b = middle; else a = middle;
                    }
                    float contactY = (a + b) * .5f;
                    if (!Measure(nativeVertices, nativeTriangles, contactY, out var contact) || contactY - seatDepth <= low) return false;
                    // A centre ray is only a contact datum. A broad cast neck
                    // and its attached bracket can extend well outside the
                    // cylinder there. Descend until the entire real cut boundary
                    // has cover, instead of capping a visibly floating rim.
                    for (float cut = contactY - seatDepth; cut > low; cut -= .005f)
                    {
                        if (!Covered(nativeVertices, nativeTriangles, cut, casingVertices, casingTriangles)) continue;
                        TrySection(nativeVertices, nativeTriangles, null, cut, out var profile);
                        fit = new Fit(contact, cut, profile); return true;
                    }
                    return false;
                }
                previous = gap > 0f; previousY = y;
            }
            return false;
        }

        private static bool Covered(Vector3[] vertices, int[] triangles, float y,
            Vector3[] casingVertices, int[] casingTriangles)
        {
            int samples = 0;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                var hits = new List<Vector3>();
                for (int e = 0; e < 3; e++)
                {
                    var a = vertices[triangles[i + e]]; var b = vertices[triangles[i + (e + 1) % 3]];
                    if ((a.y < y) == (b.y < y) || Math.Abs(a.y - b.y) < 1e-9f) continue;
                    var p = a + (b - a) * ((y - a.y) / (b.y - a.y));
                    if (!hits.Any(h => (h - p).sqrMagnitude <= 1e-14f)) hits.Add(p);
                }
                if (hits.Count != 2) continue;
                for (int step = 0; step <= 4; step++)
                {
                    var point = hits[0] + (hits[1] - hits[0]) * (step * .25f);
                    if (!Top(casingVertices, casingTriangles, point, out float height) || height - y < .002f) return false;
                    samples++;
                }
            }
            return samples >= 6;
        }

        private static bool Measure(Vector3[] vertices, int[] triangles, float y, out Vector3 centre)
        {
            // Upper authored necks have an open inboard side where they enter
            // the boiler. Measure every crossing consistently here, matching
            // native neck clipping. Switching to one closed loop midway down
            // would jump when the attached bracket splits from the tube.
            float minX = float.PositiveInfinity, minZ = minX,
                maxX = float.NegativeInfinity, maxZ = maxX;
            int count = 0;
            for (int i = 0; i < triangles.Length; i += 3) for (int e = 0; e < 3; e++)
            {
                var a = vertices[triangles[i + e]]; var b = vertices[triangles[i + (e + 1) % 3]];
                if ((a.y < y) == (b.y < y) || Math.Abs(a.y - b.y) < 1e-9f) continue;
                var p = a + (b - a) * ((y - a.y) / (b.y - a.y));
                minX = Math.Min(minX, p.x); maxX = Math.Max(maxX, p.x);
                minZ = Math.Min(minZ, p.z); maxZ = Math.Max(maxZ, p.z); count++;
            }
            centre = count >= 6 ? new Vector3((minX + maxX) * .5f, y, (minZ + maxZ) * .5f) : default;
            return count >= 6;
        }

        private static bool Top(Vector3[] vertices, int[] triangles, Vector3 point, out float height)
        {
            height = float.NegativeInfinity;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                var a = vertices[triangles[i]]; var b = vertices[triangles[i + 1]]; var c = vertices[triangles[i + 2]];
                float denominator = (b.z - c.z) * (a.x - c.x) + (c.x - b.x) * (a.z - c.z);
                if (Math.Abs(denominator) < 1e-9f) continue;
                float u = ((b.z - c.z) * (point.x - c.x) + (c.x - b.x) * (point.z - c.z)) / denominator;
                float v = ((c.z - a.z) * (point.x - c.x) + (a.x - c.x) * (point.z - c.z)) / denominator;
                float w = 1f - u - v;
                if (u < -.0001f || v < -.0001f || w < -.0001f) continue;
                height = Math.Max(height, u * a.y + v * b.y + w * c.y);
            }
            return !float.IsNegativeInfinity(height);
        }

        private static bool Valid(Vector3[] vertices, int[] triangles)
            => vertices != null && vertices.Length >= 3 && triangles != null && triangles.Length >= 3 && triangles.Length % 3 == 0 &&
                vertices.All(p => Finite(p.x) && Finite(p.y) && Finite(p.z)) && triangles.All(i => i >= 0 && i < vertices.Length);
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
#endif
