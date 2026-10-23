#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        private readonly List<SteamPipePresentation.NativePipe> steamNative = new List<SteamPipePresentation.NativePipe>();
        private readonly HashSet<Mesh> steamDetailSources = new HashSet<Mesh>();
        private readonly Dictionary<Mesh, SteamPipeShape.Socket> steamSockets = new Dictionary<Mesh, SteamPipeShape.Socket>();
        private readonly List<(MeshRenderer renderer, bool enabled)> steamHidden = new List<(MeshRenderer, bool)>();
        private Transform steamBody;
        private SteamPipePresentation steamPresentation;
        private bool steamSuspended = true;
        private int steamExpected;

        private void CacheSteamConnections()
        {
            SuspendSteamConnections(); steamNative.Clear(); steamDetailSources.Clear(); steamSockets.Clear();
            steamBody = transform.Find("LocoS282A_Body/MovingParts_LOD0");
            if (steamBody == null) throw new InvalidOperationException("Steam connection body frame is missing.");
            foreach (var cylinder in new[] { lfCylinder, rfCylinder })
            {
                var mesh = cylinder.GetComponent<MeshFilter>()?.sharedMesh;
                if (mesh != null) steamDetailSources.Add(mesh);
            }
            foreach (var fitting in prairieFittings.Where(f => f.Pipe))
            {
                bool detail = false;
                for (var t = fitting.Filter.transform; t != null; t = t.parent)
                    if (t.name == "Static_LOD0") { detail = true; break; }
                if (!detail) continue;
                var face = fitting.Vertices.Where(p => Math.Abs(p.y - fitting.LowY) < .003f).Distinct().ToArray();
                if (face.Length < 3) throw new InvalidOperationException("Native steam pipe cylinder fitting could not be measured.");
                var lower = face.Aggregate(Vector3.zero, (sum, p) => sum + p) / face.Length;
                var renderer = fitting.Filter.GetComponent<MeshRenderer>();
                var owners = GetComponentsInChildren<LODGroup>(true).Where(g => g.GetLODs().Any(l => l.renderers.Contains(renderer))).ToArray();
                var owner = owners.Length == 1 ? owners[0] : renderer.GetComponentInParent<LODGroup>();
                if (owner == null)
                {
                    var body = transform.Find("LocoS282A_Body");
                    owner = body?.GetComponent<LODGroup>() ?? body?.GetComponentsInChildren<LODGroup>(true).FirstOrDefault(g => g.GetLODs().Length != 0);
                }
                steamNative.Add(new SteamPipePresentation.NativePipe { Source = fitting.Source, Vertices = fitting.Vertices,
                    Low = fitting.LowY, High = fitting.HighY, Lower = lower,
                    Materials = renderer.sharedMaterials, Owner = owner,
                    Mirrored = (steamBody.worldToLocalMatrix * fitting.Filter.transform.localToWorldMatrix).determinant < 0f });
            }
            if (steamNative.Count != 2 || steamNative.Select(p => p.Side).Distinct().Count() != 2)
                throw new InvalidOperationException("Both native steam branch fittings are required.");
        }

        private bool SteamTemplate(Mesh mesh, out CasingTemplate template)
        {
            if (casingTemplates.TryGetValue(mesh, out template)) return true;
            foreach (var entry in casingMeshes)
                if (entry.Corrected == mesh && casingTemplates.TryGetValue(entry.Original, out template)) return true;
            template = null; return false;
        }

        private SteamPipePresentation.Cylinder SteamCylinder(MeshFilter filter, CasingTemplate template)
        {
            if (!steamSockets.TryGetValue(template.Mesh, out var socket))
            {
                var vertices = template.Vertices.Select(template.NativeFromMesh.MultiplyPoint3x4).ToArray();
                Vector3 reference;
                if (template.Franklin) reference = template.NativeFromMesh.MultiplyPoint3x4(Vector3.zero);
                else
                {
                    float centre = vertices.Average(v => v.x);
                    reference = steamNative.Single(p => p.Side == (centre >= 0f ? 1 : -1)).Lower;
                }
                socket = SteamPipeShape.TopSocket(vertices, template.Readable.triangles, reference.x, reference.z);
                steamSockets.Add(template.Mesh, socket);
            }
            var current = filter.sharedMesh.isReadable ? filter.sharedMesh.vertices : template.Vertices;
            var local = socket.Evaluate(current);
            var car = filter.GetComponentInParent<TrainCar>();
            var carFrame = car != null ? car.transform : transform;
            float orientation = Vector3.Dot(carFrame.forward, transform.forward) >= 0f ? 1f : -1f;
            var nativeSocket = template.NativeFromMesh.MultiplyPoint3x4(socket.Evaluate(template.Vertices));
            var nativeRight = filter.transform.TransformVector(template.MeshFromNative.MultiplyVector(Vector3.right));
            int side = nativeSocket.x * carFrame.InverseTransformDirection(nativeRight).x * orientation >= 0f ? 1 : -1;
            var up = template.MeshFromNative.MultiplyVector(Vector3.up);
            return new SteamPipePresentation.Cylinder { Part = filter.transform, Socket = local,
                Forward = template.MeshFromNative.MultiplyVector(Vector3.forward), Up = up,
                Scale = Mathf.Clamp(filter.transform.TransformVector(up).magnitude, .55f, 1f), Side = side, Car = car,
                CasingVertices = current, CasingTriangles = filter.sharedMesh.isReadable ? filter.sharedMesh.triangles : template.Readable.triangles };
        }

        private static bool LegacySteamRenderer(Transform part)
        {
            for (var t = part; t != null; t = t.parent)
                if (t.name.StartsWith("s282a_dry_pipe_", StringComparison.Ordinal) ||
                    t.name.StartsWith("intake_manifold_", StringComparison.Ordinal) ||
                    t.name.StartsWith("dry_pipe_new", StringComparison.Ordinal)) return true;
            return false;
        }

        private void ApplySteamConnections(int id)
        {
            SuspendSteamConnections();
            if (id == (int)WheelArrangementType.s282 || id == (int)WheelArrangementType.s404 || id == (int)WheelArrangementType.s222) return;
            bool articulated = WheelArrangement.IsArticulatedId(id);
            if (articulated) CacheFranklinCasings();
            var cylinders = new List<SteamPipePresentation.Cylinder>();
            var candidates = GetComponentsInChildren<MeshFilter>(true).AsEnumerable();
            // Erie tender LP supply is internal: no exterior pipe across the cab/coupler.
            foreach (var filter in candidates)
            {
                var mesh = filter.sharedMesh;
                if (!filter.gameObject.activeInHierarchy || mesh == null || !SteamTemplate(mesh, out var template) ||
                    (!template.Franklin && !steamDetailSources.Contains(template.Mesh))) continue;
                cylinders.Add(SteamCylinder(filter, template));
            }
            // Its relocated air-compressor outlet occupies the original lower
            // boiler branch. Keep the authored upper fitting and use an offset
            // cast transition to the Prairie's actual cylinder inlet.
            foreach (var cylinder in cylinders)
            {
                cylinder.ForwardBranch = id == (int)WheelArrangementType.s262;
                cylinder.CompactCollar = id == (int)WheelArrangementType.s460 || id == (int)WheelArrangementType.s462 ||
                    id == (int)WheelArrangementType.s464 || id == (int)WheelArrangementType.s260;
            }
            var wheels = WheelArrangement.LocomotiveWheels(id);
            steamExpected = (articulated ? wheels.Length - 2 : 1) * 2;
            if (cylinders.Count != steamExpected || cylinders.Count(c => c.Side == 1) != steamExpected / 2)
                throw new InvalidOperationException("Steam connections found " + cylinders.Count + " cylinders; expected " + steamExpected + ".");
            var legacy = GetComponentsInChildren<MeshRenderer>(true).Where(r => LegacySteamRenderer(r.transform)).ToArray();
            var root = new GameObject("[Rearranged steam connections]"); root.transform.SetParent(steamBody, false);
            steamPresentation = root.AddComponent<SteamPipePresentation>();
            try
            {
                steamPresentation.Failed = RestoreSteamSources;
                steamPresentation.Initialize(steamBody, loco, cylinders.ToArray(), steamNative.ToArray(), articulated);
                foreach (var renderer in legacy) { steamHidden.Add((renderer, renderer.enabled)); renderer.enabled = false; }
                steamSuspended = false;
                explosionMaterials?.Refresh(steamPresentation.Renderers);
                Main.Logger.Log("Steam connections: " + WheelArrangement.WheelArrangementNames[id] + "; " + steamExpected +
                    " cylinder sockets; native boiler fittings, original Franklin manifolds and adaptive flanged runs.");
            }
            catch { SuspendSteamConnections(); throw; }
        }

        private void RefreshSteamConnectionsForTender()
        {
            if (!steamSuspended && currentWA == WheelArrangementType.s28882) ApplySteamConnections((int)currentWA);
        }

        private void RestoreSteamSources()
        {
            foreach (var item in steamHidden) if (item.renderer != null) item.renderer.enabled = item.enabled;
            steamHidden.Clear();
        }

        private void SuspendSteamConnections()
        {
            steamSuspended = true;
            RestoreSteamSources();
            if (steamPresentation != null)
            {
                explosionMaterials?.Forget(steamPresentation.Renderers);
                steamPresentation.Release(); Object.Destroy(steamPresentation.gameObject); steamPresentation = null;
            }
            steamExpected = 0;
        }

        internal JObject SteamConnectionReport() => new JObject {
            ["expectedCylinders"] = steamExpected, ["connectedCylinders"] = steamPresentation != null ? steamPresentation.CylinderCount : 0,
            ["renderers"] = steamPresentation != null ? steamPresentation.PieceCount : 0,
            ["maximumEndpointGapM"] = steamPresentation != null ? steamPresentation.MaximumGapM : 0f,
            ["colliders"] = steamPresentation != null ? steamPresentation.ColliderCount : 0,
            ["manifoldHeader"] = steamPresentation != null && steamPresentation.UsesHeader,
            ["error"] = steamPresentation?.Error == null ? JValue.CreateNull() : new JValue(steamPresentation.Error),
            ["internallyFedTenderCylinders"] = poweredTenderSelected && tenderEngineRoot != null ? 2 : 0,
            ["minimumVisualPipeScale"] = SteamPipePresentation.MinimumVisualScale,
            ["minimumShaftDiameterM"] = steamPresentation != null ? steamPresentation.MinimumShaftDiameterM : 0f,
            ["route"] = steamPresentation == null ? "native" : steamPresentation.UsesDirectRigid ? "compact rigid" : "locomotive manifold header",
            ["rigidConnectionStyle"] = steamPresentation != null ? steamPresentation.DirectStyles : "native",
            ["connectionPointsBodyM"] = steamPresentation == null ? new JArray() : new JArray(steamPresentation.ConnectionPoints.Select(p => new JObject {
                ["side"] = p.Side, ["cylinder"] = new JArray(p.Socket.x, p.Socket.y, p.Socket.z),
                ["boiler"] = new JArray(p.Boiler.x, p.Boiler.y, p.Boiler.z) })),
            ["limitation"] = "Endpoint/socket checks only; full body/gear clearance and appearance require in-game inspection. Intake supply presentation, not compound steam routing or a physics change."
        };
    }
}
#endif
