using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RearrangedS282
{
    // Extend the original transfer asset with new lengths and circular elbows.
    // Profile, texture seams and collar topology remain authored source geometry.
    internal static class SteamDuctMesh
    {
        internal sealed class Template
        {
            internal const float ShaftRadius = .1016f, CollarDepth = .0762f;
            internal readonly Vector3[] Profile, CollarVertices, CollarNormals;
            internal readonly Vector2[] ProfileUV, CollarUV;
            internal readonly int[] CollarTriangles;
            internal readonly float EndV;
            internal readonly Vector2 CapUVMin, CapUVMax;
            internal Template(Vector3[] vertices, int[] triangles, Vector2[] uv)
            {
                if (vertices == null || triangles == null || uv == null || vertices.Length != uv.Length || triangles.Length % 3 != 0)
                    throw new ArgumentException("Original steam transfer mesh streams are incomplete.");
                var shaft = new HashSet<int>(); var collar = new List<int>();
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];
                    if (a < 0 || b < 0 || c < 0 || a >= vertices.Length || b >= vertices.Length || c >= vertices.Length)
                        throw new ArgumentException("Original steam transfer index is outside the mesh.");
                    float lo = Math.Min(vertices[a].z, Math.Min(vertices[b].z, vertices[c].z));
                    float hi = Math.Max(vertices[a].z, Math.Max(vertices[b].z, vertices[c].z));
                    if (hi - lo > .5f) { shaft.Add(a); shaft.Add(b); shaft.Add(c); }
                    if (lo >= -.00001f) { collar.Add(a); collar.Add(b); collar.Add(c); }
                }
                var rings = shaft.GroupBy(i => Math.Round(vertices[i].z, 5)).OrderByDescending(g => g.Key)
                    .Select(g => g.OrderByDescending(i => uv[i].x).ToArray()).ToArray();
                if (rings.Length < 2 || rings[0].Length < 9 || rings.Any(r => r.Length != rings[0].Length) || collar.Count < 30)
                    throw new ArgumentException("Original steam transfer shaft or end collar could not be measured.");
                Profile = rings[0].Select(i => new Vector3(vertices[i].x, vertices[i].y, 0f)).ToArray();
                ProfileUV = rings[0].Select(i => uv[i]).ToArray(); EndV = rings[rings.Length - 1].Average(i => uv[i].y);
                if ((Profile[0] - Profile[Profile.Length - 1]).magnitude > .00001f)
                    throw new ArgumentException("Original steam transfer has no explicit shaft texture seam.");
                var ids = collar.Distinct().ToArray(); var remap = ids.Select((id, i) => (id, i)).ToDictionary(v => v.id, v => v.i);
                CollarVertices = ids.Select(i => vertices[i] - new Vector3(0f, 0f, CollarDepth * .5f)).ToArray();
                CollarUV = ids.Select(i => uv[i]).ToArray(); CollarTriangles = collar.Select(i => remap[i]).ToArray();
                var cap = ids.Where(i => Math.Abs(vertices[i].z - CollarDepth) < .00001f).ToArray();
                if (cap.Length < 3) throw new ArgumentException("Original steam transfer collar has no usable end plate.");
                CapUVMin = new Vector2(cap.Min(i => uv[i].x), cap.Min(i => uv[i].y));
                CapUVMax = new Vector2(cap.Max(i => uv[i].x), cap.Max(i => uv[i].y));
                CollarNormals = new Vector3[ids.Length];
                for (int i = 0; i < CollarTriangles.Length; i += 3)
                {
                    int a = CollarTriangles[i], b = CollarTriangles[i + 1], c = CollarTriangles[i + 2];
                    var face = Vector3.Cross(CollarVertices[b] - CollarVertices[a], CollarVertices[c] - CollarVertices[a]);
                    CollarNormals[a] += face; CollarNormals[b] += face; CollarNormals[c] += face;
                }
                for (int i = 0; i < CollarNormals.Length; i++)
                {
                    float length = CollarNormals[i].magnitude;
                    if (length < .0000001f) throw new ArgumentException("Original steam collar contains a collapsed vertex fan.");
                    CollarNormals[i] /= length;
                }
            }
        }

        internal sealed class Builder
        {
            internal readonly List<Vector3> Vertices = new List<Vector3>();
            internal readonly List<Vector3> Normals = new List<Vector3>();
            internal readonly List<Vector2> UV = new List<Vector2>();
            internal readonly List<int> Triangles = new List<int>();
            private readonly List<Vector3> route = new List<Vector3>(), centres = new List<Vector3>(), tangents = new List<Vector3>();
            private readonly List<float> distances = new List<float>(), trims = new List<float>();
            private readonly int radialSegments, elbowStepsPerQuarter;
            private readonly Template original;
            internal IReadOnlyList<Vector3> LastCentreline => centres;
            internal IReadOnlyList<Vector3> LastTangents => tangents;
            internal Vector3 LastStart => centres.Count == 0 ? Vector3.zero : centres[0];
            internal Vector3 LastEnd => centres.Count == 0 ? Vector3.zero : centres[centres.Count - 1];
            internal float LastMinimumBendRadiusM { get; private set; }
            internal float LastLengthM { get; private set; }
            internal Vector3 LastAdapterStart { get; private set; }
            internal Vector3 LastAdapterEnd { get; private set; }
            internal float LastAdapterMaximumGapM { get; private set; }

            internal Builder(Template original, int elbowStepsPerQuarter = 10)
            {
                if (original == null) throw new ArgumentNullException(nameof(original));
                if (elbowStepsPerQuarter < 4 || elbowStepsPerQuarter > 32)
                    throw new ArgumentOutOfRangeException("Steam duct tessellation is outside its supported range.");
                this.original = original; radialSegments = original.Profile.Length - 1; this.elbowStepsPerQuarter = elbowStepsPerQuarter;
            }

            internal void Clear()
            {
                Vertices.Clear(); Normals.Clear(); UV.Clear(); Triangles.Clear();
                route.Clear(); centres.Clear(); tangents.Clear(); distances.Clear(); trims.Clear();
                LastMinimumBendRadiusM = float.PositiveInfinity; LastLengthM = 0f;
                LastAdapterStart = LastAdapterEnd = Vector3.zero; LastAdapterMaximumGapM = 0f;
            }

            internal void Tube(IReadOnlyList<Vector3> points, float radius, float elbowRadius, bool capStart = false, bool capEnd = false)
            {
                Positive(radius, "pipe radius"); Positive(elbowRadius, "elbow radius");
                if (points == null || points.Count < 2) throw new ArgumentException("Steam duct needs at least two route points.");
                route.Clear(); centres.Clear(); tangents.Clear(); distances.Clear(); trims.Clear();
                for (int index = 0; index < points.Count; index++)
                {
                    var point = points[index];
                    Finite(point);
                    if (route.Count == 0 || (point - route[route.Count - 1]).magnitude > .00001f) route.Add(point);
                }
                // Interior collinear waypoints must not split otherwise valid elbows.
                for (int i = route.Count - 2; i > 0; i--)
                {
                    var a = Unit(route[i] - route[i - 1]); var b = Unit(route[i + 1] - route[i]);
                    if (Vector3.Dot(a, b) > .999999f) route.RemoveAt(i);
                }
                if (route.Count < 2) throw new ArgumentException("Steam duct route has no length.");
                LastMinimumBendRadiusM = float.PositiveInfinity; LastLengthM = 0f;
                for (int i = 0; i < route.Count; i++)
                {
                    float trim = 0f;
                    if (i > 0 && i < route.Count - 1)
                    {
                        var a = Unit(route[i] - route[i - 1]); var b = Unit(route[i + 1] - route[i]);
                        float cosine = Clamp(Vector3.Dot(a, b), -1f, 1f);
                        if (cosine < -.9848077f) throw new InvalidOperationException("Steam duct elbow turns more than 170 degrees.");
                        if (elbowRadius < 1.5f * radius) throw new InvalidOperationException("Steam duct elbow is tighter than 1.5 pipe radii.");
                        trim = elbowRadius * (float)Math.Sqrt((1f - cosine) / (1f + cosine));
                        LastMinimumBendRadiusM = Math.Min(LastMinimumBendRadiusM, elbowRadius);
                    }
                    trims.Add(trim);
                }
                for (int i = 1; i < route.Count; i++)
                {
                    float length = (route[i] - route[i - 1]).magnitude;
                    if (length < .0001f || trims[i - 1] + trims[i] > length - .0001f)
                        throw new InvalidOperationException("Steam duct route is too short to seat its round elbows.");
                    LastLengthM += length;
                }
                if (LastLengthM > 60f) throw new InvalidOperationException("Steam duct route exceeds 60 metres.");
                AddSample(route[0], Unit(route[1] - route[0]));
                for (int i = 1; i < route.Count - 1; i++)
                {
                    var incoming = Unit(route[i] - route[i - 1]); var outgoing = Unit(route[i + 1] - route[i]);
                    var axis = Unit(Vector3.Cross(incoming, outgoing));
                    float angle = (float)Math.Acos(Clamp(Vector3.Dot(incoming, outgoing), -1f, 1f));
                    var start = route[i] - incoming * trims[i];
                    var centre = start + Vector3.Cross(axis, incoming) * elbowRadius;
                    var radial = start - centre;
                    AddSample(start, incoming);
                    int steps = Math.Max(1, (int)Math.Ceiling(angle / (Math.PI * .5) * elbowStepsPerQuarter));
                    for (int step = 1; step <= steps; step++)
                    {
                        float phi = angle * step / steps;
                        var point = centre + Rotate(radial, axis, phi);
                        var tangent = Rotate(incoming, axis, phi);
                        // Exact analytic exit avoids accumulating a seam offset.
                        if (step == steps) { point = route[i] + outgoing * trims[i]; tangent = outgoing; }
                        AddSample(point, tangent);
                    }
                }
                AddSample(route[route.Count - 1], Unit(route[route.Count - 1] - route[route.Count - 2]));
                LastLengthM = distances[distances.Count - 1];
                int first = Vertices.Count, width = radialSegments + 1;
                var right = Perpendicular(tangents[0]);
                for (int row = 0; row < centres.Count; row++)
                {
                    if (row > 0) right = Transport(right, tangents[row - 1], tangents[row]);
                    var up = Unit(Vector3.Cross(tangents[row], right));
                    for (int col = 0; col <= radialSegments; col++)
                    {
                        var profile = original.Profile[col];
                        var normal = Unit(right * profile.x + up * profile.y);
                        Add(centres[row] + (right * profile.x + up * profile.y) * (radius / Template.ShaftRadius), normal,
                            new Vector2(original.ProfileUV[col].x, Lerp(original.ProfileUV[col].y, original.EndV, distances[row] / LastLengthM)));
                    }
                }
                for (int row = 0; row + 1 < centres.Count; row++) for (int col = 0; col < radialSegments; col++)
                {
                    int a = first + row * width + col, b = a + 1, c = a + width, d = c + 1;
                    Face(a, b, c); Face(b, d, c);
                }
                if (capStart) Cap(first, centres[0], -tangents[0], true);
                if (capEnd) Cap(first + (centres.Count - 1) * width, LastEnd, tangents[tangents.Count - 1], false);
            }

            // Copy the complete original front collar, including its authored
            // chamfers, split-normal vertices, annular faces and exact UV islands.
            internal void Collar(Vector3 centre, Vector3 axis, float pipeRadius, bool reverse = false)
            {
                Finite(centre); axis = Unit(axis); Positive(pipeRadius, "flange bore radius");
                if (reverse) axis = -axis;
                var right = Perpendicular(axis); var up = Unit(Vector3.Cross(axis, right));
                int first = Vertices.Count; float scale = pipeRadius / Template.ShaftRadius;
                for (int i = 0; i < original.CollarVertices.Length; i++)
                {
                    var p = original.CollarVertices[i]; var n = original.CollarNormals[i];
                    Add(centre + (right * p.x + up * p.y + axis * p.z) * scale,
                        right * n.x + up * n.y + axis * n.z, original.CollarUV[i]);
                }
                foreach (int index in original.CollarTriangles) Triangles.Add(first + index);
            }

            // Extend the authored set with a paired-collar union. Both flanges
            // retain their original profile; only the intervening shaft varies.
            internal void Coupling(Vector3 centre, Vector3 axis, float pipeRadius, float length)
            {
                Finite(centre); axis = Unit(axis); Positive(pipeRadius, "coupling bore radius"); Positive(length, "coupling length");
                float depth = Template.CollarDepth * pipeRadius / Template.ShaftRadius;
                if (length < depth * 2f + .0001f) throw new ArgumentException("Steam coupling is too short to seat both original collars.");
                Collar(centre - axis * ((length - depth) * .5f), axis, pipeRadius, true);
                Collar(centre + axis * ((length - depth) * .5f), axis, pipeRadius);
                // Existing original profile, generated directly so the last
                // route diagnostics continue to refer to the main supply pipe.
                Shaft(centre - axis * (length * .5f - depth), centre + axis * (length * .5f - depth), axis, pipeRadius);
            }

            // A source-profile concentric reducer joins the smaller native boiler
            // neck to the minimum-width transfer run. Neither end fitting bends.
            internal void Reducer(Vector3 start, Vector3 end, float startRadius, float endRadius)
            {
                Finite(start); Finite(end); Positive(startRadius, "reducer inlet radius"); Positive(endRadius, "reducer outlet radius");
                float length = (end - start).magnitude;
                if (!IsFinite(length) || length < .02f || length > 60f || length < 2f * Math.Abs(endRadius - startRadius))
                    throw new ArgumentException("Steam reducer is too short for a gradual taper.");
                Shaft(start, end, Unit(end - start), startRadius, endRadius);
            }

            // A compact offset cast transition is needed where the original
            // boiler neck is already inside the bend and no straight tangent
            // remains. Keep the actual cut polygon, including all its corners,
            // and finish on the original transfer profile. Parallel horizontal
            // sections prevent the tight S-bend/fold of a swept round tube.
            // The retained native prefix keeps its own UVs. This NEW casting
            // uses only the original transfer shaft island; blending between
            // those unrelated atlas islands would paint stray body textures.
            internal void Adapter(IReadOnlyList<Vector3> boundary, IReadOnlyList<Vector2> boundaryUV,
                Vector3 endCentre, float endRadius, IReadOnlyList<Vector2> edgeEndUV = null, Vector3 midOffset = default)
            {
                if (boundary == null || boundaryUV == null || boundary.Count < 3 || boundary.Count != boundaryUV.Count)
                    throw new ArgumentException("Steam cast adapter needs a matching ordered boundary and UV loop.");
                Finite(endCentre); Positive(endRadius, "adapter outlet radius");
                Finite(midOffset);
                if (Math.Abs(midOffset.y) > .000001f || midOffset.magnitude > .5f)
                    throw new ArgumentException("Steam cast adapter clearance offset must be horizontal and below half a metre.");
                if (edgeEndUV != null && edgeEndUV.Count != boundary.Count)
                    throw new ArgumentException("Steam cast adapter edge UV count differs from its boundary.");
                var source = new List<AdapterKnot>();
                float lowX = float.PositiveInfinity, highX = float.NegativeInfinity, lowZ = float.PositiveInfinity, highZ = float.NegativeInfinity;
                float y = boundary[0].y;
                for (int i = 0; i < boundary.Count; i++)
                {
                    var point = boundary[i]; Finite(point);
                    if (!IsFinite(boundaryUV[i].x) || !IsFinite(boundaryUV[i].y) || Math.Abs(point.y - y) > .0001f)
                        throw new ArgumentException("Steam cast adapter inlet is not a finite horizontal boundary.");
                    if (edgeEndUV != null && (!IsFinite(edgeEndUV[i].x) || !IsFinite(edgeEndUV[i].y)))
                        throw new ArgumentException("Steam cast adapter edge UV is not finite.");
                    lowX = Math.Min(lowX, point.x); highX = Math.Max(highX, point.x);
                    lowZ = Math.Min(lowZ, point.z); highZ = Math.Max(highZ, point.z);
                }
                var startCentre = new Vector3((lowX + highX) * .5f, y, (lowZ + highZ) * .5f);
                float height = endCentre.y - y;
                if (Math.Abs(height) < .02f || (endCentre - startCentre).magnitude > 2f)
                    throw new ArgumentException("Steam cast adapter needs a distinct nearby horizontal outlet.");
                int boundaryCount = boundary.Count;
                if ((boundary[boundaryCount - 1] - boundary[0]).magnitude < .000001f) boundaryCount--;
                float signedArea = 0f;
                for (int i = 0; i < boundaryCount; i++)
                    signedArea += CrossXZ(boundary[i] - startCentre, boundary[(i + 1) % boundaryCount] - startCentre);
                for (int i = 0; i < boundaryCount; i++)
                {
                    var point = boundary[i];
                    var radial = point - startCentre;
                    if (radial.magnitude < .00001f) throw new ArgumentException("Steam cast adapter boundary crosses its centre.");
                    var outgoing = boundaryUV[i];
                    var incoming = edgeEndUV == null ? outgoing : edgeEndUV[(i + boundaryCount - 1) % boundaryCount];
                    source.Add(new AdapterKnot(Angle(radial), point, signedArea >= 0f ? outgoing : incoming, signedArea >= 0f ? incoming : outgoing));
                }
                // Check that the supplied polygon is star-shaped and ordered,
                // rather than silently converting several disjoint cut loops.
                float winding = 0f;
                for (int i = 0; i < source.Count; i++)
                {
                    var a = source[i].Point - startCentre; var b = source[(i + 1) % source.Count].Point - startCentre;
                    winding += (float)Math.Atan2(CrossXZ(a, b), a.x * b.x + a.z * b.z);
                }
                if (Math.Abs(Math.Abs(winding) - 2f * (float)Math.PI) > .001f)
                    throw new ArgumentException("Steam cast adapter boundary winds across itself or does not surround its centre.");
                for (int i = 0; i < source.Count; i++)
                {
                    var a = source[i].Point - startCentre; var b = source[(i + 1) % source.Count].Point - startCentre;
                    if (Math.Abs(CrossXZ(a, b)) < .00000001f) continue;
                    int sign = CrossXZ(a, b) > 0f ? 1 : -1;
                    for (int k = i + 1; k < source.Count; k++)
                    {
                        var c = source[k].Point - startCentre; var d = source[(k + 1) % source.Count].Point - startCentre;
                        if (CrossXZ(c, d) * sign < -.00000001f)
                            throw new ArgumentException("Steam cast adapter boundary is not a single star-shaped ordered loop.");
                    }
                    break;
                }
                source.Sort((a, b) => a.Angle.CompareTo(b.Angle));
                var target = new List<AdapterKnot>();
                for (int i = 0; i < radialSegments; i++)
                {
                    var p = original.Profile[i] * (endRadius / Template.ShaftRadius);
                    target.Add(new AdapterKnot(Angle(new Vector3(p.x, 0f, p.y)), endCentre + new Vector3(p.x, 0f, p.y), original.ProfileUV[i],
                        original.ProfileUV[i == 0 ? radialSegments : i]));
                }
                target.Sort((a, b) => a.Angle.CompareTo(b.Angle));
                var angles = source.Select(k => k.Angle).Concat(target.Select(k => k.Angle)).OrderBy(a => a).ToList();
                // Near-coincident authored/source angles otherwise create strips
                // narrower than float precision at body-space Z around 10 m.
                // Weld within 0.006 degrees (under 0.02 mm at these radii).
                for (int i = angles.Count - 1; i > 0; i--) if (angles[i] - angles[i - 1] < .0001f) angles.RemoveAt(i);
                int width = angles.Count + 1, rows = 17, count = width * rows;
                var positions = new Vector3[count]; var normals = new Vector3[count];
                var startRing = new AdapterKnot[width]; var endRing = new AdapterKnot[width];
                for (int col = 0; col < width; col++)
                {
                    float angle = angles[col % angles.Count];
                    startRing[col] = Sample(source, startCentre, angle); endRing[col] = Sample(target, endCentre, angle);
                }
                var centreOffset = endCentre - startCentre; centreOffset.y = 0f;
                for (int row = 0; row < rows; row++)
                {
                    float t = (float)row / (rows - 1), q = t * t * t * (10f - 15f * t + 6f * t * t);
                    float derivative = 30f * t * t * (t - 1f) * (t - 1f);
                    float bow = 16f * t * t * (1f - t) * (1f - t), bowDerivative = 32f * t * (1f - t) * (1f - 2f * t);
                    var centre = startCentre + centreOffset * q + midOffset * bow + new Vector3(0f, height * t, 0f);
                    for (int col = 0; col < width; col++)
                    {
                        var a = startRing[col]; var b = endRing[col];
                        var ar = a.Point - startCentre; var br = b.Point - endCentre;
                        int index = row * width + col;
                        positions[index] = row == 0 ? a.Point : row == rows - 1 ? b.Point : centre + ar * (1f - q) + br * q;
                        int before = (col + angles.Count - 1) % angles.Count, after = (col + 1) % angles.Count;
                        var angular = (startRing[after].Point - startRing[before].Point) * (1f - q)
                            + (endRing[after].Point - endRing[before].Point) * q;
                        var along = new Vector3(0f, height, 0f) + (centreOffset + br - ar) * derivative + midOffset * bowDerivative;
                        var normal = Vector3.Cross(angular, along);
                        normals[index] = Unit(height < 0f ? normal : -normal);
                    }
                }
                // Validate the actual triangulated loft before touching output.
                // Area and radial-facing tests catch malformed input and folds.
                for (int row = 0; row + 1 < rows; row++) for (int col = 0; col + 1 < width; col++)
                {
                    int a = row * width + col, b = a + 1, c = a + width, d = c + 1;
                    CheckAdapterFace(positions, normals, a, height < 0f ? b : c, height < 0f ? c : b);
                    CheckAdapterFace(positions, normals, b, height < 0f ? d : c, height < 0f ? c : d);
                }
                float cornerGap = 0f;
                foreach (var knot in source)
                {
                    float nearest = float.PositiveInfinity;
                    for (int col = 0; col < width; col++) nearest = Math.Min(nearest, (positions[col] - knot.Point).magnitude);
                    cornerGap = Math.Max(cornerGap, nearest);
                }
                foreach (var knot in target)
                {
                    float nearest = float.PositiveInfinity;
                    for (int col = 0; col < width; col++) nearest = Math.Min(nearest, (positions[(rows - 1) * width + col] - knot.Point).magnitude);
                    cornerGap = Math.Max(cornerGap, nearest);
                }
                LastAdapterStart = startCentre; LastAdapterEnd = endCentre; LastAdapterMaximumGapM = cornerGap;
                // Independent strip vertices preserve both sides of the authored
                // transfer UV seam. Shared positions and analytic normals keep
                // shading continuous; the native-prefix join is an intentional
                // UV seam between two separate authored atlas islands.
                for (int col = 0; col + 1 < width; col++)
                {
                    int first = Vertices.Count;
                    for (int row = 0; row < rows; row++)
                    {
                        float t = (float)row / (rows - 1);
                        int a = row * width + col, b = a + 1;
                        Add(positions[a], normals[a], new Vector2(endRing[col].UV.x, Lerp(original.EndV, endRing[col].UV.y, t)));
                        Add(positions[b], normals[b], new Vector2(endRing[col + 1].IncomingUV.x, Lerp(original.EndV, endRing[col + 1].IncomingUV.y, t)));
                    }
                    for (int row = 0; row + 1 < rows; row++)
                    {
                        int a = first + row * 2, b = a + 1, c = a + 2, d = a + 3;
                        if (height < 0f) { Face(a, b, c); Face(b, d, c); } else { Face(a, c, b); Face(b, c, d); }
                    }
                }
            }

            private readonly struct AdapterKnot
            {
                internal readonly float Angle; internal readonly Vector3 Point; internal readonly Vector2 UV, IncomingUV;
                internal AdapterKnot(float angle, Vector3 point, Vector2 uv, Vector2 incoming) { Angle = angle; Point = point; UV = uv; IncomingUV = incoming; }
            }
            private static float Angle(Vector3 p)
            {
                float angle = (float)Math.Atan2(p.z, p.x); return angle < 0f ? angle + (float)(2f * Math.PI) : angle;
            }
            private static float CrossXZ(Vector3 a, Vector3 b) => a.x * b.z - a.z * b.x;
            private static AdapterKnot Sample(List<AdapterKnot> ring, Vector3 centre, float angle)
            {
                for (int i = 0; i < ring.Count; i++) if (Math.Abs(ring[i].Angle - angle) < .000001f) return ring[i];
                int upper = 0; while (upper < ring.Count && ring[upper].Angle < angle) upper++;
                var a = ring[(upper + ring.Count - 1) % ring.Count]; var b = ring[upper % ring.Count];
                var dir = new Vector3((float)Math.Cos(angle), 0f, (float)Math.Sin(angle));
                var edge = b.Point - a.Point; float denominator = CrossXZ(dir, edge);
                if (Math.Abs(denominator) < .00000001f) throw new ArgumentException("Steam cast adapter has a singular radial edge.");
                float t = CrossXZ(a.Point - centre, dir) / denominator;
                if (t < -.0001f || t > 1.0001f) throw new ArgumentException("Steam cast adapter boundary cannot be sampled without crossing an edge.");
                t = Clamp(t, 0f, 1f);
                var uv = BlendUV(a.UV, b.IncomingUV, t);
                return new AdapterKnot(angle, a.Point + edge * t, uv, uv);
            }
            private static Vector2 BlendUV(Vector2 a, Vector2 b, float t) => t <= 0f ? a : t >= 1f ? b : new Vector2(Lerp(a.x, b.x, t), Lerp(a.y, b.y, t));
            private static void CheckAdapterFace(Vector3[] p, Vector3[] n, int a, int b, int c)
            {
                var area = Vector3.Cross(p[b] - p[a], p[c] - p[a]);
                if (area.magnitude < .000000001f || Vector3.Dot(area, n[a] + n[b] + n[c]) <= 0f)
                    throw new InvalidOperationException("Steam cast adapter contains a folded or collapsed face.");
            }

            private void AddSample(Vector3 point, Vector3 tangent)
            {
                if (centres.Count > 0 && (point - centres[centres.Count - 1]).magnitude < .000001f) return;
                distances.Add(centres.Count == 0 ? 0f : distances[distances.Count - 1] + (point - centres[centres.Count - 1]).magnitude);
                centres.Add(point); tangents.Add(Unit(tangent));
            }
            private void Cap(int ring, Vector3 centre, Vector3 normal, bool reverse)
            {
                int first = Vertices.Count; var middle = new Vector2((original.CapUVMin.x + original.CapUVMax.x) * .5f,
                    (original.CapUVMin.y + original.CapUVMax.y) * .5f);
                Add(centre, normal, middle);
                for (int col = 0; col <= radialSegments; col++)
                {
                    var p = original.Profile[col] / Template.ShaftRadius;
                    Add(Vertices[ring + col], normal, new Vector2(Lerp(original.CapUVMin.x, original.CapUVMax.x, (p.x + 1f) * .5f),
                        Lerp(original.CapUVMin.y, original.CapUVMax.y, (p.y + 1f) * .5f)));
                }
                for (int col = 0; col < radialSegments; col++)
                    if (reverse) Face(first, first + col + 2, first + col + 1); else Face(first, first + col + 1, first + col + 2);
            }
            private void Shaft(Vector3 start, Vector3 end, Vector3 axis, float radius, float? endRadius = null)
            {
                int first = Vertices.Count, width = radialSegments + 1;
                var right = Perpendicular(axis); var up = Unit(Vector3.Cross(axis, right));
                float finalRadius = endRadius ?? radius, slope = (finalRadius - radius) / (end - start).magnitude;
                for (int row = 0; row < 2; row++) for (int col = 0; col <= radialSegments; col++)
                {
                    var p = original.Profile[col]; var radial = right * p.x + up * p.y;
                    Add((row == 0 ? start : end) + radial * ((row == 0 ? radius : finalRadius) / Template.ShaftRadius), Unit(Unit(radial) - axis * slope),
                        new Vector2(original.ProfileUV[col].x, row == 0 ? original.ProfileUV[col].y : original.EndV));
                }
                for (int col = 0; col < radialSegments; col++)
                {
                    int a = first + col, b = a + 1, c = a + width, d = c + 1;
                    Face(a, b, c); Face(b, d, c);
                }
            }
            private void Add(Vector3 vertex, Vector3 normal, Vector2 uv) { Vertices.Add(vertex); Normals.Add(normal); UV.Add(uv); }
            private void Face(int a, int b, int c) { Triangles.Add(a); Triangles.Add(b); Triangles.Add(c); }
            private static Vector3 Transport(Vector3 right, Vector3 from, Vector3 to)
            {
                float cosine = Clamp(Vector3.Dot(from, to), -1f, 1f);
                var cross = Vector3.Cross(from, to);
                var mapped = right + Vector3.Cross(cross, right) + Vector3.Cross(cross, Vector3.Cross(cross, right)) / (1f + cosine);
                return Unit(mapped - to * Vector3.Dot(mapped, to));
            }
            private static Vector3 Perpendicular(Vector3 axis)
            {
                var right = Vector3.Cross(Math.Abs(axis.y) < .9f ? Vector3.up : Vector3.forward, axis);
                return Unit(right);
            }
            private static Vector3 Rotate(Vector3 vector, Vector3 axis, float angle)
                => vector * (float)Math.Cos(angle) + Vector3.Cross(axis, vector) * (float)Math.Sin(angle)
                    + axis * (Vector3.Dot(axis, vector) * (1f - (float)Math.Cos(angle)));
            private static Vector3 Unit(Vector3 vector)
            {
                Finite(vector); float length = vector.magnitude;
                if (!IsFinite(length) || length < .000001f) throw new ArgumentException("Steam duct direction is singular/nonfinite.");
                return vector / length;
            }
            private static void Finite(Vector3 point)
            {
                if (!IsFinite(point.x) || !IsFinite(point.y) || !IsFinite(point.z)) throw new ArgumentException("Steam duct coordinate is not finite.");
            }
            private static void Positive(float value, string name)
            {
                if (!IsFinite(value) || value <= .000001f) throw new ArgumentOutOfRangeException(name);
            }
            private static bool IsFinite(float f) => !float.IsNaN(f) && !float.IsInfinity(f);
            private static float Clamp(float value, float min, float max) => Math.Max(min, Math.Min(max, value));
            private static float Lerp(float a, float b, float t) => a + (b - a) * t;
        }
    }
}
