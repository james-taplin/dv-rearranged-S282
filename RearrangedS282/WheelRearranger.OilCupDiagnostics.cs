#if B99_ALL_STANDARD
using System.Linq;
using DV.Util;
using DV.Simulation.Controllers;
using DV.Simulation.Ports;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        internal JObject CaptureOilCupPresentation()
        {
            var external = loco != null ? loco.loadedExternalInteractables : null;
            if (external == null) return new JObject { ["externalLoaded"] = false };
            var readers = external.GetComponentsInChildren<OilingPointPortFeederReader>(true);
            var live = readers.Where(r => r.gameObject.activeInHierarchy).ToArray();
            var duplicatePorts = live.GroupBy(r => r.refillPortId).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();
            var plan = PlanBigEnds(loco, out int uncovered);
            int expected = (NativeOilCupsVisible ? 6 : 0) + plan.allocation.Count(a => a >= 0);
            float maxGap = 0f;
            bool lidsBound = true;
            var oiling = BigEndOiling.Points(loco.SimController.simFlow);
            var details = new JArray();
            var positions = new System.Collections.Generic.List<Vector3>();
            foreach (var reader in live)
            {
                var cup = reader.GetComponentInParent<PositionSyncConsumer>();
                if (cup == null || cup.providerTransform == null)
                    throw new System.InvalidOperationException("Visible oil cup has no live position provider.");
                var point = oiling?.oilingPoints.FirstOrDefault(p => p.refillExtIn.id == reader.refillPortId);
                var lid = cup.GetComponentsInChildren<InteractablePortFeeder>(true)
                    .FirstOrDefault(f => point != null && f.portId == point.pointDoorExtIn.id);
                var boundPort = lid != null ? typeof(InteractablePortFeeder)
                    .GetField("port", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                    ?.GetValue(lid) : null;
                bool lidBound = lid != null && lid.GetComponent<DV.CabControls.ControlImplBase>() != null &&
                    ReferenceEquals(boundPort, point.pointDoorExtIn);
                lidsBound &= lidBound;
                float gap = (cup.transform.position - cup.providerTransform.position).magnitude;
                positions.Add(cup.transform.position);
                maxGap = Mathf.Max(maxGap, gap);
                var cap = cup.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault(f => f.name == "OilCupCap_LOD0");
                Vector3 capSize = cap != null && cap.sharedMesh != null ? cap.sharedMesh.bounds.size : Vector3.zero;
                if (cap != null) capSize = new Vector3(capSize.x * cap.transform.TransformVector(Vector3.right).magnitude,
                    capSize.y * cap.transform.TransformVector(Vector3.up).magnitude, capSize.z * cap.transform.TransformVector(Vector3.forward).magnitude);
                details.Add(new JObject { ["tag"] = cup.syncTag, ["refillPort"] = reader.refillPortId,
                    ["extra"] = IsBigEndCup(cup.transform), ["providerGapM"] = gap,
                    ["lidControlBound"] = lidBound, ["doorPort"] = lid?.portId,
                    ["capSizeM"] = new JArray(capSize.x, capSize.y, capSize.z) });
            }
            int overlaps = 0;
            for (int i = 0; i < positions.Count; i++)
                for (int j = i + 1; j < positions.Count; j++) if ((positions[i] - positions[j]).magnitude < .01f) overlaps++;
            bool countMatches = live.Length == expected && duplicatePorts.Length == 0 && overlaps == 0;
            float seatGap = 0f;
            foreach (var marker in bigEndMarkers.Where(m => m != null))
            {
                if (marker.parent.name == "Oiling2" && marker.parent.parent?.name == "s282_mech_push_rod_to_connect")
                {
                    seatGap = Mathf.Max(seatGap, (marker.position - marker.parent.position).magnitude);
                    continue;
                }
                var bearing = marker.parent.Find("s282_mech_push_rod_to_connect/Oiling2");
                // Main-bearing cup mounts can have an authored Z offset from
                // their crank pin; compare against that same native mount.
                bool nativeMainSeat = bearing != null && (marker.position - bearing.position).magnitude < .002f;
                Vector3 seat = nativeMainSeat ? bearing.position : default;
                if (!nativeMainSeat && !OilCupSeat.TryCouplingTop(marker.parent, marker.position, MeshFinder.Instance.NativeSideRodMesh, out seat))
                    throw new System.InvalidOperationException("Oilcup is not above a visible coupling-rod bearing.");
                seatGap = Mathf.Max(seatGap, (marker.position - seat).magnitude);
            }
            return new JObject { ["ready"] = BigEndCupsReady, ["activeCups"] = live.Length, ["expectedActiveCups"] = expected,
                ["totalBigEndSites"] = plan.sites.Count, ["nativeCoveredBigEnds"] = plan.sites.Count(s => s.NativeCup),
                ["allocatedBigEnds"] = plan.allocation.Count(a => a >= 0), ["uncoveredBigEnds"] = uncovered,
                ["coverageComplete"] = uncovered == 0,
                ["countsMatch"] = countMatches, ["duplicateRefillPorts"] = new JArray(duplicatePorts),
                ["overlappingMountPairs"] = overlaps, ["maximumGuideGapM"] = MaximumOilGuideGap(),
                ["maximumProviderGapM"] = maxGap, ["maximumCouplingSeatGapM"] = seatGap,
                ["maximumExpressCouplingSeatGapM"] = currentWA == WheelArrangementType.s482Big ? seatGap : 0f,
                ["lidControlsBound"] = lidsBound,
                ["nativeLubricatorAnimationBound"] = NativeLubricatorAnimationBound(), ["cups"] = details };
        }
    }
}
#endif
