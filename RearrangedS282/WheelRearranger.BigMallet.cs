#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RearrangedS282
{
    // Table-driven Mallets, built like the 2-6-6-2 (each engine a uniformly scaled
    // copy of the native drivetrain and its static cylinder assembly). Each layout
    // picks its coupled drivers per engine (3, 4 or 5), its carrying trucks and how
    // far the front cylinders reach forward; the scale is solved so the engines
    // fill the frame up to the 2-2-2-2-2's trailing-wheel position:
    // - 0-8-8-0 "Angus": 4+4, no trucks, cylinders forward over the absent pony.
    // - 2-6-8-0 Great Northern: 3+4 with a leading truck.
    // - 2-8-8-0: the Erie 2-8-8-8-2's locomotive half (TenderEngine adds the third).
    // - 2-6-6-0: 3+3 with a leading truck.
    // - 2-10-10-2: 5+5 with both trucks; small drivers, all pull and no speed.
    // Extra physical powered axles (beyond the six native ones) are created lazily.
    internal partial class WheelRearranger
    {
        private const float TenCoupledFifthCaliperZ = -3.08f;   // as Show10Drivers

        private sealed class MalletSpec
        {
            internal int Front, Rear; internal bool Leading, Trailing; internal float FrontExtra; internal string Name;
        }

        private static MalletSpec SpecFor(WheelArrangementType kind)
        {
            switch (kind)
            {
                case WheelArrangementType.s08880: return new MalletSpec { Front = 4, Rear = 4, FrontExtra = .6f, Name = "0-8-8-0" };
                case WheelArrangementType.s2680: return new MalletSpec { Front = 3, Rear = 4, Leading = true, Name = "2-6-8-0" };
                case WheelArrangementType.s28882: return new MalletSpec { Front = 4, Rear = 4, Leading = true, Name = "2-8-8-8-2 (loco 2-8-8-0)" };
                case WheelArrangementType.s2660: return new MalletSpec { Front = 3, Rear = 3, Leading = true, Name = "2-6-6-0" };
                case WheelArrangementType.s210102: return new MalletSpec { Front = 5, Rear = 5, Leading = true, Trailing = true, FrontExtra = .6f, Name = "2-10-10-2" };
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        private sealed class BigMalletLayout
        {
            internal WheelArrangementType Kind;
            internal MalletSpec Spec;
            internal float Scale, FrontOffset, RearOffset, FrontLastDriver, RearCylinderFront, RearLastDriver, Radius, PinZ, FirstDriverShift, TrailingZ;
            internal Matrix4x4 Front => EngineMap(FrontOffset, Scale);
            internal Matrix4x4 Rear => EngineMap(RearOffset, Scale);
        }

        private sealed class BigMalletAssets
        {
            internal float Scale = float.NaN;
            internal readonly List<Transform> Statics = new List<Transform>();
        }

        // Four-coupled rear engine (also the Erie tender engine's source) and five-coupled rear engine.
        private Transform bigMalletRearL, bigMalletRearR, tenCoupledRearL, tenCoupledRearR;
        private readonly List<Transform> bigMalletFrontAxles = new List<Transform>(), bigMalletRearAxles = new List<Transform>();
        private readonly Dictionary<WheelArrangementType, BigMalletAssets> bigMalletStatics = new Dictionary<WheelArrangementType, BigMalletAssets>();
        private BigMalletLayout bigMallet;

        internal static bool IsBigMallet(int id) => id == (int)WheelArrangementType.s08880 || id == (int)WheelArrangementType.s2680 ||
            id == (int)WheelArrangementType.s2660 || id == (int)WheelArrangementType.s210102;

        // Extra powered axles in use by the current layout (for the powered list).
        private IEnumerable<Transform> BigMalletExtraAxles() => bigMalletFrontAxles.Concat(bigMalletRearAxles).Where(t => t != null);

        private float NativeDriverZ(int n) =>
            new[] { firstLeftDriveWheel, secondLeftDriveWheel, thirdLeftDriveWheel, fourthLeftDriveWheel, fifthLeftDriveWheel }[n - 1].localPosition.z;

        private Transform BigMalletRearDrive(int coupled, bool left) =>
            coupled == 3 ? (left ? malletRearL : malletRearR) : coupled == 4 ? (left ? bigMalletRearL : bigMalletRearR) : (left ? tenCoupledRearL : tenCoupledRearR);

        internal Transform CurrentBigMalletRearDrive(bool left) => bigMallet != null ? BigMalletRearDrive(bigMallet.Spec.Rear, left) : null;

        private BigMalletLayout MeasureBigMallet(WheelArrangementType kind)
        {
            var spec = SpecFor(kind);
            var body = MalletBody;
            float cylinderFront = MeasureNativeCylinderFront();
            float d2 = secondLeftDriveWheel.localPosition.z, d3 = thirdLeftDriveWheel.localPosition.z;
            float radius = secondLeftDriveWheel.localPosition.y;
            if (d2 <= d3 || d3 <= NativeDriverZ(4) || NativeDriverZ(4) <= NativeDriverZ(5)) throw new InvalidOperationException("Native S282 driver order is unexpected.");
            float front = cylinderFront + spec.FrontExtra;
            float frontLast = NativeDriverZ(spec.Front), rearLast = NativeDriverZ(spec.Rear);
            float trailingLimit = body.InverseTransformPoint(rearBogie.TransformPoint(TriplexOwnTrailingLimit)).z;
            // Where the last driver's rear edge may reach: in front of the trailing
            // wheel, or (no trailing truck) where the 2-2-2-2-2's trailing wheel ends.
            float rearEdge = spec.Trailing ? trailingLimit + MalletTrailingRadius + MalletEngineGap : trailingLimit - MalletTrailingRadius;
            float s = (front - rearEdge - MalletEngineGap) / ((cylinderFront - frontLast) + (cylinderFront - rearLast) + 2f * radius);
            if (!(s > .4f && s < 1f)) throw new InvalidOperationException($"{spec.Name} engines do not fit the S282 frame (scale {s}).");
            var layout = new BigMalletLayout { Kind = kind, Spec = spec, Scale = s, Radius = s * radius,
                FirstDriverShift = d2 + (d2 - d3) - firstLeftDriveWheel.localPosition.z };
            layout.FrontOffset = front - s * cylinderFront;
            layout.FrontLastDriver = layout.FrontOffset + s * frontLast;
            layout.RearCylinderFront = layout.FrontLastDriver - layout.Radius - MalletEngineGap;
            layout.RearOffset = layout.RearCylinderFront - s * cylinderFront;
            layout.RearLastDriver = layout.RearOffset + s * rearLast;
            layout.PinZ = (layout.FrontLastDriver + layout.RearCylinderFront) * .5f;
            layout.TrailingZ = layout.RearLastDriver - layout.Radius - MalletTrailingRadius - MalletEngineGap;
            return layout;
        }

        private void ApplyBigMalletGeometry(WheelArrangementType kind)
        {
            bigMallet = MeasureBigMallet(kind);
            var spec = bigMallet.Spec;
            if (spec.Rear == 3) { mallet = MeasureMallet(); EnsureMalletAssets(); }
            EnsureBigMalletAssets(bigMallet);
            float s = bigMallet.Scale;
            ResetBody(); ResetToFourCoupled(); HideLeadingWheels(); HideTrailingWheels();
            if (spec.Leading) ShowTwoLeadingWheels();
            if (spec.Trailing)
            {
                ShowTwoTrailingWheelsFor12Coupled();
                float trailingLocal = rearBogie.InverseTransformPoint(MalletBody.TransformPoint(new Vector3(0f, 0f, bigMallet.TrailingZ))).z;
                float trailingShift = trailingLocal - secondRearAxle.localPosition.z;
                secondRearAxle.localPosition += Vector3.forward * trailingShift;
                secondRearAxleSupport.localPosition += Vector3.forward * trailingShift;
            }
            setWheelRadius(.712f * s);

            driveL.localPosition = driveR.localPosition = new Vector3(0f, 0f, bigMallet.FrontOffset);
            driveL.localScale = new Vector3(-1f, s, s); driveR.localScale = new Vector3(1f, s, s);
            foreach (var drive in new[] { driveL, driveR }) ConfigureCoupledDrive(drive, spec.Front, false);
            leftReachRod.gameObject.SetActive(false); rightReachRod.gameObject.SetActive(false);
            foreach (var part in new[] { lDryPipe, rDryPipe, reachRodSupport, lubricatorSupport, fOilLines })
                part.gameObject.SetActive(false);
            foreach (var part in new[] { leftSixthBrakeCaliper, rightSixthBrakeCaliper }) part.gameObject.SetActive(false);
            if (spec.Front < 5) { leftFifthBrakeCaliper.gameObject.SetActive(false); rightFifthBrakeCaliper.gameObject.SetActive(false); }
            if (spec.Front < 4)
            {
                leftFourthBrakeCaliper.gameObject.SetActive(false); rightFourthBrakeCaliper.gameObject.SetActive(false);
                RequireArticulatedPart(splitLoco, "s282a_suspension_l_4").gameObject.SetActive(false);
                RequireArticulatedPart(splitLoco, "s282a_suspension_r_4").gameObject.SetActive(false);
            }

            var frontStatic = EngineStaticParts(spec.Front);
            foreach (var part in frontStatic)
            {
                PrepareEngineStatic(part, part, spec.Front);
                part.gameObject.SetActive(true);
                MapMalletPart(part, bigMallet.Front);
            }
            var colliders = MalletColliders.Select(path => transform.Find(path)).Where(t => t != null).ToArray();
            foreach (var collider in colliders) MapMalletPart(collider, bigMallet.Front);

            foreach (var asset in bigMalletStatics[kind].Statics) if (asset != null) asset.gameObject.SetActive(true);
            var rearL = BigMalletRearDrive(spec.Rear, true); var rearR = BigMalletRearDrive(spec.Rear, false);
            rearL.localPosition = rearR.localPosition = new Vector3(0f, 0f, bigMallet.RearOffset);
            rearL.localScale = new Vector3(-1f, s, s); rearR.localScale = new Vector3(1f, s, s);
            rearL.gameObject.SetActive(true); rearR.gameObject.SetActive(true);
            scaledEngineMaps = new[] { bigMallet.Front, bigMallet.Rear };
            BindScaledEngineAnimation(rearL, rearR);
            BindEngineBrakeHeat(new[] { rearL, rearR }, Enumerable.Range(1, spec.Rear).ToArray());

            // Front engine axles in the native front bogie, rear engine in the rear;
            // extra axles beyond the native six where an engine needs them.
            driveAxle5.gameObject.SetActive(true); driveAxle6.gameObject.SetActive(true);
            var frontAxles = new[] { driveAxle1, driveAxle2, driveAxle6 }.Concat(bigMalletFrontAxles).ToArray();
            var rearAxles = new[] { driveAxle3, driveAxle4, driveAxle5 }.Concat(bigMalletRearAxles).ToArray();
            var frontWheels = new[] { firstLeftDriveWheel, secondLeftDriveWheel, thirdLeftDriveWheel, fourthLeftDriveWheel, fifthLeftDriveWheel };
            for (int i = 0; i < frontAxles.Length; i++)
            {
                bool used = i < spec.Front;
                frontAxles[i].gameObject.SetActive(used);
                if (used) AlignNativeAxle(frontAxles[i], frontWheels[i], true);
            }
            for (int i = 0; i < rearAxles.Length; i++)
            {
                bool used = i < spec.Rear;
                rearAxles[i].gameObject.SetActive(used);
                if (used) AlignNativeAxle(rearAxles[i], RequireArticulatedPart(rearL, "s282_wheels_driving_" + (i + 1)), false);
            }

            foreach (var part in new[] { driveL, driveR }.Concat(frontStatic).Concat(colliders))
                MoveArticulatedPart(part, articulatedFrontRoot);
            UpdateMalletSparks();
            ConfigureNativeArticulation((int)kind, bigMallet.PinZ);
            BindArticulatedLods();
            Main.Logger.Log($"Articulated S282 {spec.Name}: scale {s:0.000}, {2f * bigMallet.Radius:0.00} m drivers; " +
                $"engines at {bigMallet.FrontOffset:0.00}/{bigMallet.RearOffset:0.00}, pin {bigMallet.PinZ:0.00}, " +
                $"rear cylinders {bigMallet.RearCylinderFront:0.00}, last driver {bigMallet.RearLastDriver:0.00}" +
                (spec.Trailing ? $", trailing axle {bigMallet.TrailingZ:0.00}" : "") + " (body z).");
        }

        // LMS static pieces of one engine with n coupled drivers.
        private Transform[] EngineStaticParts(int coupled)
        {
            if (coupled == 3) return MalletStaticParts();
            var parts = new List<Transform> { lfCylinder, rfCylinder, cylinderCocks,
                lfCrossheadBracket, rfCrossheadBracket, lLiftingArmSupport, rLiftingArmSupport,
                leftFirstBrakeCaliper, leftSecondBrakeCaliper, leftThirdBrakeCaliper, leftFourthBrakeCaliper,
                rightFirstBrakeCaliper, rightSecondBrakeCaliper, rightThirdBrakeCaliper, rightFourthBrakeCaliper };
            if (coupled == 5) { parts.Add(leftFifthBrakeCaliper); parts.Add(rightFifthBrakeCaliper); }
            parts.AddRange(new[] { "l", "r" }.SelectMany(side => Enumerable.Range(1, 4).Select(i => RequireArticulatedPart(splitLoco, "s282a_suspension_" + side + "_" + i))));
            return parts.ToArray();
        }

        // Engine-specific placement before the engine mapping: the three-axle
        // engine's first driver moves back to equal pitch; the fifth caliper of a
        // ten-coupled engine sits where Show10Drivers puts it.
        private void PrepareEngineStatic(Transform part, Transform source, int coupled)
        {
            string name = source.name;
            if (coupled == 3 && name.EndsWith("_1", StringComparison.Ordinal) && (name.Contains("caliper") || name.Contains("suspension")))
                part.localPosition += Vector3.forward * bigMallet.FirstDriverShift;
            if (coupled == 5 && (source == leftFifthBrakeCaliper || source == rightFifthBrakeCaliper))
                part.localPosition = new Vector3(0f, 0f, TenCoupledFifthCaliperZ);
        }

        private void ConfigureCoupledDrive(Transform drive, int coupled, bool rearClone)
        {
            if (coupled == 3) ConfigureMalletDrive(drive, rearClone);
            else if (coupled == 4) ConfigureEightCoupledDrive(drive, rearClone);
            else ConfigureTenCoupledDrive(drive, rearClone);
        }

        // Native four-axle engine as on the stock S282: drivers 1-4, the native
        // coupling rod, drivers 5-6 and the longer-rod variants hidden.
        private void ConfigureEightCoupledDrive(Transform drive, bool rearClone) => ConfigureLongEngine(drive, 4, rearClone);

        // Five-axle engine as on the 2-10-x: the native rod plus its fifth-axle segment.
        private void ConfigureTenCoupledDrive(Transform drive, bool rearClone) => ConfigureLongEngine(drive, 5, rearClone);

        private void ConfigureLongEngine(Transform drive, int coupled, bool rearClone)
        {
            var wheels = Enumerable.Range(1, 6).Select(i => drive.Find("s282_wheels_driving_" + i)).ToArray();
            if (wheels.Any(w => w == null)) throw new InvalidOperationException("Coupled S282 driver is missing.");
            for (int i = 0; i < 6; i++) wheels[i].gameObject.SetActive(i < coupled);
            var meshes = new[] { flangedDriver, blindDriver, blindDriverBigWeight, coupled == 4 ? flangedDriver : blindDriver, flangedDriver };
            for (int i = 0; i < coupled; i++) wheels[i].GetComponent<MeshFilter>().sharedMesh = meshes[i];
            var rod = RequireArticulatedPart(drive, "s282_mech_wheels_connect");
            rod.GetComponent<MeshRenderer>().enabled = true;
            foreach (Transform child in rod)
            {
                var filter = child.GetComponent<MeshFilter>();
                if ((filter != null && filter.sharedMesh == MeshFinder.Instance.ThreeAxleSideRodMesh) ||
                    child.name.StartsWith("s282_mech_wheels_connect", StringComparison.Ordinal) || child.name.StartsWith("[Rearranged", StringComparison.Ordinal))
                    child.gameObject.SetActive(false);
            }
            if (coupled == 5)
            {
                // The five-axle segment is a child of the coupling rod; clones keep its sibling index.
                bool left = drive == driveL || drive.name.EndsWith("L", StringComparison.Ordinal) || drive.name.EndsWith("L]", StringComparison.Ordinal);
                var nativeSegment = left ? leftFiveAxleSideRod : rightFiveAxleSideRod;
                int index = nativeSegment.GetSiblingIndex();
                if (index >= rod.childCount) throw new InvalidOperationException("Five-axle side rod segment is missing.");
                rod.GetChild(index).gameObject.SetActive(true);
            }
            SetPartActive(drive, "s282_mech_cutoff_rod", false);
            SetPartActive(drive, "s282_mech_cutoff_rail/s282_mech_lubricator_rod", false);
            SetPartActive(drive, "LubricatorParts", false);
            var shoes = drive.Find("s282a_brake_shoes(Clone)");
            if (shoes != null)
                foreach (Transform shoe in shoes)
                    shoe.gameObject.SetActive(Enumerable.Range(1, coupled).Any(i => shoe.name == "s282a_brake_shoe_" + i));
            if (rearClone)
                foreach (var child in drive.GetComponentsInChildren<Transform>(true))
                    if (child.name.EndsWith("LOD1", StringComparison.Ordinal)) child.gameObject.SetActive(false);
        }

        // Lazily clones the rear engines and extra powered axles from the prepared
        // (stock) pose; the rear static parts are mapped once per layout.
        private void EnsureBigMalletAssets(BigMalletLayout layout)
        {
            var spec = layout.Spec;
            var created = new List<Transform>();
            try
            {
                if (spec.Rear == 4 && bigMalletRearL == null)
                {
                    bigMalletRearL = CloneEngineDrive(driveL, lfAnimator, "[Rearranged eight-coupled rear drivetrain L]", articulatedRearRoot, d => ConfigureEightCoupledDrive(d, true));
                    created.Add(bigMalletRearL);
                    bigMalletRearR = CloneEngineDrive(driveR, rfAnimator, "[Rearranged eight-coupled rear drivetrain R]", articulatedRearRoot, d => ConfigureEightCoupledDrive(d, true));
                    created.Add(bigMalletRearR);
                }
                if (spec.Rear == 5 && tenCoupledRearL == null)
                {
                    tenCoupledRearL = CloneEngineDrive(driveL, lfAnimator, "[Rearranged ten-coupled rear drivetrain L]", articulatedRearRoot, d => ConfigureTenCoupledDrive(d, true));
                    created.Add(tenCoupledRearL);
                    tenCoupledRearR = CloneEngineDrive(driveR, rfAnimator, "[Rearranged ten-coupled rear drivetrain R]", articulatedRearRoot, d => ConfigureTenCoupledDrive(d, true));
                    created.Add(tenCoupledRearR);
                }
                while (bigMalletFrontAxles.Count < Math.Max(0, spec.Front - 3))
                {
                    var axle = Object.Instantiate(driveAxle2.gameObject, frontBogie).transform;
                    axle.name = "[axle] drive mallet front " + (bigMalletFrontAxles.Count + 1); axle.gameObject.SetActive(false);
                    created.Add(axle); bigMalletFrontAxles.Add(axle);
                }
                while (bigMalletRearAxles.Count < Math.Max(0, spec.Rear - 3))
                {
                    var axle = Object.Instantiate(driveAxle4.gameObject, rearBogie).transform;
                    axle.name = "[axle] drive mallet rear " + (bigMalletRearAxles.Count + 1); axle.gameObject.SetActive(false);
                    created.Add(axle); bigMalletRearAxles.Add(axle);
                }
                if (!bigMalletStatics.TryGetValue(layout.Kind, out var assets)) bigMalletStatics[layout.Kind] = assets = new BigMalletAssets();
                if (assets.Scale != layout.Scale)
                {
                    foreach (var old in assets.Statics.Where(t => t != null)) { old.gameObject.SetActive(false); Object.Destroy(old.gameObject); }
                    assets.Statics.Clear(); assets.Scale = float.NaN;
                    foreach (var source in EngineStaticParts(spec.Rear))
                    {
                        var clone = Object.Instantiate(source.gameObject, source.parent).transform;
                        created.Add(clone);
                        clone.name = "[Rearranged Mallet rear " + spec.Rear + "] " + source.name;
                        clone.gameObject.SetActive(true);
                        PrepareEngineStatic(clone, source, spec.Rear);
                        MapMalletPart(clone, layout.Rear);
                        clone.SetParent(articulatedRearRoot, true);
                        clone.gameObject.SetActive(false);
                        assets.Statics.Add(clone);
                    }
                    assets.Scale = layout.Scale;
                }
            }
            catch
            {
                foreach (var t in created.Where(t => t != null)) { t.gameObject.SetActive(false); Object.Destroy(t.gameObject); }
                if (created.Contains(bigMalletRearL)) bigMalletRearL = bigMalletRearR = null;
                if (created.Contains(tenCoupledRearL)) tenCoupledRearL = tenCoupledRearR = null;
                bigMalletFrontAxles.RemoveAll(t => created.Contains(t)); bigMalletRearAxles.RemoveAll(t => created.Contains(t));
                foreach (var a in bigMalletStatics.Values) a.Statics.RemoveAll(t => created.Contains(t));
                if (bigMalletStatics.TryGetValue(layout.Kind, out var failed)) failed.Scale = float.NaN;
                throw;
            }
        }

        private IEnumerable<Transform> BigMalletPresentationRoots() =>
            new[] { bigMalletRearL, bigMalletRearR, tenCoupledRearL, tenCoupledRearR }.Concat(bigMalletStatics.Values.SelectMany(a => a.Statics)).Where(t => t != null);

        private void SuspendBigMalletGeometry()
        {
            foreach (var asset in bigMalletStatics.Values.SelectMany(a => a.Statics)) if (asset != null) asset.gameObject.SetActive(false);
            foreach (var drive in new[] { bigMalletRearL, bigMalletRearR, tenCoupledRearL, tenCoupledRearR })
                if (drive != null) drive.gameObject.SetActive(false);
            // Extra axles must never stay active: bogie axle lists are rebuilt from active [axle] objects.
            foreach (var axle in BigMalletExtraAxles()) axle.gameObject.SetActive(false);
        }
    }
}
#endif
