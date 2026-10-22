using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RearrangedS282
{
    // Presentation geometry only; native articulation supplies every moving frame.
    internal static class SteamPipeShape
    {
        internal const float SourceFrontZ = .076200001f, SourceRearZ = -3.022599936f, CollarLength = .17f;
        internal static Vector3 NativeManifoldLower => new Vector3(-.002371f, -.352207f, -.038816f);
        internal static Vector3 NativeManifoldInlet => new Vector3(-.002371f, .003393f, .754934f);
        internal static bool CanMorphNative(float low, float high, float targetY)
            => high - low > .26f && high - targetY > .27f;

        internal static Vector3 Unit(Vector3 v)
        {
            float length = v.magnitude;
            if (!(length > 1e-7f) || float.IsInfinity(length) || float.IsNaN(length))
                throw new InvalidOperationException("Steam pipe has a singular/nonfinite direction.");
            return v / length;
        }

        internal static void MorphNative(Vector3[] source, Vector3[] destination, Vector3 lower,
            float lowY, float highY, Vector3 target)
        {
            if (source == null || destination == null || source.Length != destination.Length || ReferenceEquals(source, destination))
                throw new ArgumentException("Native steam pipe needs independent matching buffers.");
            const float band = .13f;
            var delta = target - lower;
            if (!CanMorphNative(lowY, highY, lowY + delta.y))
                throw new InvalidOperationException("Steam pipe sockets are too close to retain both fittings.");
            for (int i = 0; i < source.Length; i++)
            {
                // Translate both collar bands rigidly. Only the intervening
                // pipe changes length/shear; never resize a flange or bearing.
                float weight = Math.Max(0f, Math.Min(1f, (highY - band - source[i].y) / (highY - lowY - 2f * band)));
                destination[i] = source[i] + delta * weight;
            }
        }

        internal readonly struct Socket
        {
            internal readonly int A, B, C;
            internal readonly Vector3 Weights;
            internal Socket(int a, int b, int c, Vector3 weights) { A = a; B = b; C = c; Weights = weights; }
            internal Vector3 Evaluate(Vector3[] vertices) => vertices[A] * Weights.x + vertices[B] * Weights.y + vertices[C] * Weights.z;
        }

        internal static Socket TopSocket(Vector3[] nativeVertices, int[] triangles, float x, float z)
        {
            bool found = false; float highest = float.NegativeInfinity; Socket result = default;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int ai = triangles[i], bi = triangles[i + 1], ci = triangles[i + 2];
                var a = nativeVertices[ai]; var b = nativeVertices[bi]; var c = nativeVertices[ci];
                float denominator = (b.z - c.z) * (a.x - c.x) + (c.x - b.x) * (a.z - c.z);
                if (Math.Abs(denominator) < 1e-9f) continue;
                float u = ((b.z - c.z) * (x - c.x) + (c.x - b.x) * (z - c.z)) / denominator;
                float v = ((c.z - a.z) * (x - c.x) + (a.x - c.x) * (z - c.z)) / denominator;
                float w = 1f - u - v;
                if (u < -.0001f || v < -.0001f || w < -.0001f) continue;
                float y = u * a.y + v * b.y + w * c.y;
                if (y <= highest) continue;
                highest = y; found = true; result = new Socket(ai, bi, ci, new Vector3(u, v, w));
            }
            if (!found) throw new InvalidOperationException("No cylinder surface under the measured steam socket.");
            return result;
        }

        internal sealed class TransferTopology
        {
            internal Vector3[] Vertices;
            internal int[] Triangles;
            internal Vector2[] UV;
        }

        // The original straight transfer has only four shaft rings. Bending those
        // long triangles produces folds even when the centerline is well sampled.
        // Keep its authored flanges exactly, and rebuild only the intervening tube.
        internal static TransferTopology DenseTransfer(Vector3[] source, int[] triangles, Vector2[] uv, int shaftSegments = 192, bool retainCollars = true)
        {
            if (source == null || triangles == null || uv == null || uv.Length != source.Length || shaftSegments < 16)
                throw new ArgumentException("Steam transfer needs readable positions, triangles and matching UVs.");
            const float epsilon = .00001f;
            float front = retainCollars ? 0f : SourceFrontZ, rear = retainCollars ? -2.94639994f : SourceRearZ;
            var vertices = retainCollars ? new List<Vector3>(source) : new List<Vector3>();
            var texture = retainCollars ? new List<Vector2>(uv) : new List<Vector2>(); var faces = new List<int>();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                float a = source[triangles[i]].z, b = source[triangles[i + 1]].z, c = source[triangles[i + 2]].z;
                if (retainCollars && (Math.Min(a, Math.Min(b, c)) >= front - epsilon || Math.Max(a, Math.Max(b, c)) <= rear + epsilon))
                { faces.Add(triangles[i]); faces.Add(triangles[i + 1]); faces.Add(triangles[i + 2]); }
            }
            // Use vertices belonging to shaft faces, excluding collar atlas islands.
            // Each original ring has an explicit duplicated texture seam. Its
            // descending U order is counterclockwise in the authored XY plane.
            var shaftIds = new HashSet<int>();
            for(int i=0;i<triangles.Length;i+=3) {
                float lo=Math.Min(source[triangles[i]].z,Math.Min(source[triangles[i+1]].z,source[triangles[i+2]].z));
                float hi=Math.Max(source[triangles[i]].z,Math.Max(source[triangles[i+1]].z,source[triangles[i+2]].z));
                if(hi-lo>.5f) {shaftIds.Add(triangles[i]);shaftIds.Add(triangles[i+1]);shaftIds.Add(triangles[i+2]);}
            }
            var rings=shaftIds.GroupBy(i=>Math.Round(source[i].z,5)).OrderByDescending(g=>g.Key)
                .Select(g=>g.OrderByDescending(i=>uv[i].x).ToArray()).ToArray();
            if(rings.Length<2||rings[0].Length<9||rings.Any(r=>r.Length!=rings[0].Length))
                throw new InvalidOperationException("Original steam shaft profile could not be measured.");
            int first=vertices.Count,width=rings[0].Length;
            for(int row=0;row<=shaftSegments;row++) {
                float t=(float)row/shaftSegments,z=front+(rear-front)*t;
                // A collar-free sleeve maps the same original shaft atlas island
                // to its own full axial span, rather than sampling the whole atlas.
                float sampleZ=-2.94639994f*t;int upper=1;
                while(upper<rings.Length-1&&source[rings[upper][0]].z>sampleZ)upper++;
                var ar=rings[upper-1];var br=rings[upper];
                float q=(sampleZ-source[ar[0]].z)/(source[br[0]].z-source[ar[0]].z);
                for(int col=0;col<width;col++) {
                    var a=source[ar[col]];var b=source[br[col]];
                    float x=a.x+(b.x-a.x)*q,y=a.y+(b.y-a.y)*q;
                    float radius=(float)Math.Sqrt(x*x+y*y);x*=.1016f/radius;y*=.1016f/radius;
                    vertices.Add(new Vector3(x,y,z));
                    var ua=uv[ar[col]];var ub=uv[br[col]];
                    texture.Add(new Vector2(ua.x+(ub.x-ua.x)*q,ua.y+(ub.y-ua.y)*q));
                }
            }
            for (int row = 0; row < shaftSegments; row++) for (int col = 0; col < width-1; col++)
            {
                int a = first + row * width + col, b = a + 1, c = a + width, d = c + 1;
                // CCW in XY and descending Z gives these outward-facing quads.
                faces.Add(a); faces.Add(c); faces.Add(b); faces.Add(b); faces.Add(c); faces.Add(d);
            }
            return new TransferTopology { Vertices = vertices.ToArray(), Triangles = faces.ToArray(), UV = texture.ToArray() };
        }

        internal sealed class Sweep
        {
            private const int Steps = 48;
            private readonly Vector3[] points = new Vector3[Steps + 3], forward = new Vector3[Steps + 3],
                right = new Vector3[Steps + 3], up = new Vector3[Steps + 3];
            private readonly float[] distance = new float[Steps + 3];
            private float length, scale;
            private bool straight;
            internal Vector3 Start => points[0];
            internal Vector3 End => points[Steps + 2];
            internal float Length => length;
            internal float MinimumBendRadiusM { get; private set; }
            internal float MaximumTangentStepRadians { get; private set; }

            internal void Build(Vector3 start, Vector3 startDirection, Vector3 end, Vector3 endDirection, Vector3 referenceUp,
                float radialScale, float handle = .35f)
            {
                if (!(radialScale > .1f) || float.IsInfinity(radialScale)) throw new ArgumentException("Invalid steam pipe diameter scale.");
                straight = false; scale = radialScale;
                var t0 = Unit(startDirection); var t1 = Unit(endDirection);
                float collar = CollarLength * scale;
                var a = start + t0 * collar; var d = end - t1 * collar;
                var b = a + t0 * handle; var c = d - t1 * handle;
                points[0] = start; points[Steps + 2] = end;
                forward[0] = -t0; forward[Steps + 2] = -t1;
                for (int i = 0; i <= Steps; i++)
                {
                    float t = (float)i / Steps, s = 1f - t;
                    points[i + 1] = a * (s * s * s) + b * (3f * s * s * t) + c * (3f * s * t * t) + d * (t * t * t);
                    forward[i + 1] = -Unit((b - a) * (s * s) + (c - b) * (2f * s * t) + (d - c) * (t * t));
                }
                var r = Vector3.Cross(referenceUp, forward[0]);
                if (r.magnitude < .01f) r = Vector3.Cross(Vector3.forward, forward[0]);
                right[0] = Unit(r); up[0] = Unit(Vector3.Cross(forward[0], right[0])); distance[0] = 0f;
                MinimumBendRadiusM = float.PositiveInfinity; MaximumTangentStepRadians = 0f;
                for (int i = 1; i < points.Length; i++)
                {
                    float cosine = Vector3.Dot(forward[i - 1], forward[i]);
                    float angle = (float)Math.Acos(Math.Max(-1f, Math.Min(1f, cosine)));
                    MaximumTangentStepRadians = Math.Max(MaximumTangentStepRadians, angle);
                    if (angle > .0001f) MinimumBendRadiusM = Math.Min(MinimumBendRadiusM, (points[i] - points[i - 1]).magnitude / angle);
                    if (cosine < -.99f) throw new InvalidOperationException("Steam pipe reverses within a collar-sized bend.");
                    var cross = Vector3.Cross(forward[i - 1], forward[i]);
                    var old = right[i - 1];
                    var transported = old + Vector3.Cross(cross, old) + Vector3.Cross(cross, Vector3.Cross(cross, old)) / (1f + cosine);
                    right[i] = Unit(transported - forward[i] * Vector3.Dot(transported, forward[i]));
                    up[i] = Unit(Vector3.Cross(forward[i], right[i]));
                    distance[i] = distance[i - 1] + (points[i] - points[i - 1]).magnitude;
                }
                // A centerline tighter than the tube radius folds its inner wall,
                // regardless of mesh tessellation. Reject it before displaying.
                if (MinimumBendRadiusM < .1016f * scale * 1.25f)
                    throw new InvalidOperationException("Steam pipe bend is too tight for its diameter.");
                length = distance[Steps + 2];
                if (!(length > 2f * collar + .02f) || length > 60f) throw new InvalidOperationException("Invalid steam pipe run length.");
            }

            // Short seated sleeves use the existing boiler collar and chest as
            // their end fittings, so no second pair of collars needs to fit.
            internal void BuildStraight(Vector3 start, Vector3 end, float radialScale)
            {
                if (!(radialScale > .1f) || float.IsInfinity(radialScale)) throw new ArgumentException("Invalid steam pipe diameter scale.");
                length = (end-start).magnitude;
                if (!(length > .02f) || length > 60f) throw new InvalidOperationException("Invalid steam pipe run length.");
                straight = true; scale = radialScale;
                var f = -Unit(end-start); var r = Vector3.Cross(Vector3.up,f);
                if(r.magnitude<.01f)r=Vector3.Cross(Vector3.forward,f);
                r=Unit(r);var u=Unit(Vector3.Cross(f,r));
                for(int i=0;i<points.Length;i++) {
                    float t=(float)i/(points.Length-1);points[i]=start+(end-start)*t;
                    forward[i]=f;right[i]=r;up[i]=u;distance[i]=length*t;
                }
                MinimumBendRadiusM=float.PositiveInfinity;MaximumTangentStepRadians=0f;
            }

            internal Vector3 Closest(Vector3 point)
            {
                var result = points[0]; float best = float.PositiveInfinity;
                for (int i = 1; i < points.Length; i++)
                {
                    var edge = points[i] - points[i - 1]; float square = Vector3.Dot(edge, edge);
                    float t = square > 1e-10f ? Math.Max(0f, Math.Min(1f, Vector3.Dot(point - points[i - 1], edge) / square)) : 0f;
                    var candidate = points[i - 1] + edge * t; float error = (point - candidate).magnitude;
                    if (error < best) { best = error; result = candidate; }
                }
                return result;
            }

            internal void Deform(Vector3[] source, Vector3[] output)
            {
                if (source == null || output == null || source.Length != output.Length || ReferenceEquals(source, output))
                    throw new ArgumentException("Steam pipe sweep needs independent matching buffers.");
                float original = (SourceFrontZ - SourceRearZ) * scale, collar = CollarLength * scale;
                for (int i = 0; i < source.Length; i++)
                {
                    var v = source[i]; float oldDistance = (SourceFrontZ - v.z) * scale;
                    float target = straight ? oldDistance / original * length : oldDistance <= collar ? oldDistance : oldDistance >= original - collar
                        ? length - (original - oldDistance)
                        : collar + (oldDistance - collar) / (original - 2f * collar) * (length - 2f * collar);
                    int upper = Array.BinarySearch(distance, target);
                    if (upper < 0) upper = ~upper;
                    upper = Math.Max(1, Math.Min(distance.Length - 1, upper)); int lower = upper - 1;
                    float span = distance[upper] - distance[lower];
                    float t = span > 1e-9f ? (target - distance[lower]) / span : 0f;
                    var center = points[lower] + (points[upper] - points[lower]) * t;
                    var f = Unit(forward[lower] + (forward[upper] - forward[lower]) * t);
                    var x = Unit(right[lower] + (right[upper] - right[lower]) * t);
                    x = Unit(x - f * Vector3.Dot(x, f)); var y = Unit(Vector3.Cross(f, x));
                    output[i] = center + x * (v.x * scale) + y * (v.y * scale);
                }
            }
        }
    }
}
