#if B99_ALL_STANDARD
using System.Collections.Generic;
using System.Linq;
using DV.Util;
using UnityEngine;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        private sealed class OilMount
        {
            internal Transform Provider, Guide;
            internal Vector3 NativeLocal, GuideLocal;
            internal float GuideMeshSide;
        }
        private readonly List<OilMount> nativeOilMounts = new List<OilMount>();
        internal bool NativeOilCupsVisible => currentWA != WheelArrangementType.s404 && currentWA != WheelArrangementType.s222;

        private void CacheOilCupMounts()
        {
            nativeOilMounts.Clear();
            foreach (var drive in new[] { driveL, driveR })
                foreach (string path in new[] { "Oiling0", "s282_mech_push_rod/Oiling1", "s282_mech_wheels_connect/s282_mech_push_rod_to_connect/Oiling2" })
                {
                    var provider = drive.Find(path);
                    if (provider == null) continue;
                    var guide = path == "Oiling0" ? NearestOilGuide(provider.position) : null;
                    nativeOilMounts.Add(new OilMount { Provider = provider, Guide = guide, NativeLocal = provider.localPosition,
                        GuideLocal = guide != null ? guide.InverseTransformPoint(provider.position) : Vector3.zero,
                        GuideMeshSide = guide != null ? GuideMeshSide(guide) : 0f });
                }
        }

        private void ApplyOilCupMounts()
        {
            foreach (var mount in nativeOilMounts)
            {
                if (mount.Provider == null) continue;
                mount.Provider.localPosition = mount.NativeLocal;
                if (mount.Guide != null)
                {
                    // LMS side labels and reflected articulated guides do not
                    // consistently share the drivetrain's L/R convention.
                    var guide = NearestOilGuide(mount.Provider.position);
                    if (guide == null) continue;
                    var point = mount.GuideLocal;
                    if (mount.GuideMeshSide * GuideMeshSide(guide) < 0f) point.x = -point.x;
                    mount.Provider.position = guide.TransformPoint(point);
                }
            }
        }

        private Transform NearestOilGuide(Vector3 point)
        {
            Transform best = null; float distance = float.PositiveInfinity;
            foreach (var guide in new[] { lfCrossheadBracket, rfCrossheadBracket })
            {
                if (guide == null || !guide.gameObject.activeInHierarchy) continue;
                var mesh = guide.GetComponent<MeshFilter>()?.sharedMesh;
                var centre = guide.TransformPoint(mesh != null ? mesh.bounds.center : Vector3.zero);
                float d = (centre - point).magnitude;
                if (d < distance) { best = guide; distance = d; }
            }
            return best;
        }

        private static float GuideMeshSide(Transform guide) => guide.GetComponent<MeshFilter>()?.sharedMesh?.bounds.center.x ?? 0f;

        internal float MaximumOilGuideGap()
        {
            float gap = 0f;
            if (!NativeOilCupsVisible) return gap;
            foreach (var mount in nativeOilMounts.Where(m => m.Guide != null && m.Provider != null))
            {
                var guide = NearestOilGuide(mount.Provider.position);
                var mesh = guide != null ? guide.GetComponent<MeshFilter>()?.sharedMesh : null;
                if (mesh == null) continue;
                var p = guide.InverseTransformPoint(mount.Provider.position); var b = mesh.bounds;
                var closest = new Vector3(Mathf.Max(b.min.x, Mathf.Min(b.max.x, p.x)),
                    Mathf.Max(b.min.y, Mathf.Min(b.max.y, p.y)), Mathf.Max(b.min.z, Mathf.Min(b.max.z, p.z)));
                gap = Mathf.Max(gap, (guide.TransformPoint(closest) - mount.Provider.position).magnitude);
            }
            return gap;
        }

        internal void BindOilCupPresentation(PositionSyncProvider provider, PositionSyncConsumer consumer)
        {
            var mount = nativeOilMounts.FirstOrDefault(m => m.Provider == provider.transform);
            var retarget = provider.GetComponentInParent<DriveRetarget>();
            if (mount != null && retarget != null) retarget.BindOilCup(provider.transform, mount.NativeLocal, consumer);
            // Keep the six native circuits bound even on the two renderer-only
            // novelty layouts; hide their unsupported cups and hold their oil full.
            consumer.gameObject.SetActive(NativeOilCupsVisible);
        }

    }
}
#endif
