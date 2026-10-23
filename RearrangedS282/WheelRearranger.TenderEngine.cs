#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.Linq;
using DV.ThingTypes;
using DV.Wheels;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RearrangedS282
{
    // 2-8-8-8-2 Erie triplex: the third engine rides under the S282 tender, a
    // separate car. Its steam comes from the loco's native graph (the middle
    // engine in TenderMode); its pull is applied to the tender's bogies and is
    // limited by the tender's own weight, so grip falls as coal and water are
    // used, as on the real P-1. Its exhaust leaves the loco's blast.
    //
    // Nothing is saved on the tender: the link is rebuilt whenever the loco is
    // in this arrangement with an S282 tender coupled behind it, and removed on
    // uncoupling, switching, derailment or destruction (polled every physics tick).
    internal partial class WheelRearranger
    {
        // Tender engine cylinders just behind the tender's front buffer beam.
        private const float TenderCylinderFrontZ = -.35f;
        // Four of the tender's five visible axles (drivers plus trailing wheel) carry the drivers.
        private const float TenderDriverWeightShare = .8f, TenderEngineFriction = .24f;

        private TrainCar tenderEngineCar;
        private BigMalletLayout tenderLayout;
        private float tenderNativeCylinderFront;
        private readonly Dictionary<Transform, Matrix4x4> tenderStaticMaps = new Dictionary<Transform, Matrix4x4>();
        private bool poweredTenderSelected;
        internal bool PoweredTenderSelected => poweredTenderSelected;
        private Transform tenderEngineRoot, tenderDriveL, tenderDriveR;
        private readonly List<GameObject> tenderHidden = new List<GameObject>();
        private float tenderForceN, tenderLimitN, tenderDemandN;
        private bool tenderSlipping;
        internal Transform TenderEffectsRoot => tenderEngineRoot != null && tenderEngineRoot.gameObject.activeInHierarchy ? tenderEngineRoot : null;
        internal Transform TenderExhaustOutlet => tenderPresentation != null ? tenderPresentation.Outlet : null;
        private ErieTenderPresentation tenderPresentation;

        internal bool TryMapTenderOutlet(Vector3 nativeOrigin, Vector3 nativeCentre, out Vector3 local)
        {
            local = default;
            if (tenderLayout == null || steamBody == null) return false;
            var layout = tenderLayout;
            var native = steamBody.InverseTransformPoint(nativeOrigin);
            var centre = steamBody.InverseTransformPoint(nativeCentre);
            var map = EngineMap(TenderCylinderFrontZ - layout.Scale * tenderNativeCylinderFront, layout.Scale);
            local = native + map.MultiplyPoint3x4(centre) - centre;
            return true;
        }

        // Called only in the prepared native pose, before changing the host.
        private void EnsurePoweredTenderSource()
        {
            if (tenderLayout != null) return;
            try
            {
                EnsureArticulatedAssets();
                tenderNativeCylinderFront = MeasureNativeCylinderFront();
                tenderLayout = MeasureBigMallet(WheelArrangementType.s28882);
                EnsureBigMalletAssets(tenderLayout);
                foreach (var source in bigMalletStatics[WheelArrangementType.s28882].Statics)
                    tenderStaticMaps[source] = MalletBody.worldToLocalMatrix * source.localToWorldMatrix;
                // Asset construction must not expose Franklin parts on a rigid host.
                foreach (var asset in articulatedAssets) if (asset != null) asset.gameObject.SetActive(false);
            }
            catch { tenderLayout = null; tenderStaticMaps.Clear(); throw; }
        }

        private void SelectPoweredTender(bool selected)
        {
            poweredTenderSelected = selected;
            if (!selected)
            {
                DetachTenderEngine();
                if (articulatedSimulation != null && !ArticulatedSimulation.IsTriplexId((int)currentWA))
                    articulatedSimulation.ConfigureTender(false, 1f);
                return;
            }
            if (tenderLayout == null) throw new InvalidOperationException("Powered tender source was not prepared.");
            articulatedSimulation = ArticulatedSimulation.Ensure(loco);
            float radius = .712f * tenderLayout.Scale;
            articulatedSimulation.ConfigureTender(true, PoweredTenderPolicy.RpmRatio(loco.SimController.drivingForce.wheelRadius, radius));
            cylinderCalibration.ApplyTender(articulatedSimulation.MiddleEngine, radius);
        }

        internal Vector3 TenderMechanismLocal
        {
            get
            {
                if (tenderDriveL == null || tenderEngineRoot == null) return Vector3.zero;
                float z = Enumerable.Range(1, 4).Average(i => tenderDriveL.Find("s282_wheels_driving_" + i).localPosition.z);
                return tenderEngineRoot.InverseTransformPoint(tenderDriveL.TransformPoint(new Vector3(0f, .7f, z)));
            }
        }

        private static bool IsS282Tender(TrainCar car) =>
            car != null && CarTypes.IsTender(car.carLivery) && car.carType == TrainCarType.Tender;

        // Called every physics tick from FixedUpdate.
        private void TickTenderEngine(TrainCar car)
        {
            TrainCar wanted = null;
            if (car != null && poweredTenderSelected && tenderLayout != null && articulatedSimulation != null && !car.derailed)
            {
                var other = car.rearCoupler != null && car.rearCoupler.coupledTo != null ? car.rearCoupler.coupledTo.train : null;
                if (IsS282Tender(other) && !other.derailed) wanted = other;
            }
            if (wanted != tenderEngineCar || wanted == null && !ReferenceEquals(tenderEngineCar, null))
            {
                DetachTenderEngine();
                if (wanted != null)
                {
                    try { AttachTenderEngine(wanted); }
                    catch (Exception ex)
                    {
                        Main.Logger.Error($"Erie triplex tender engine could not be attached to {wanted.ID}: {ex}");
                        DetachTenderEngine();
                        tenderEngineCar = wanted;   // do not retry every tick; re-armed by uncoupling
                        return;
                    }
                }
            }
            if (tenderEngineRoot != null) ApplyTenderForce(car);
        }

        private void AttachTenderEngine(TrainCar tender)
        {
            if (bigMalletRearL == null || bigMalletRearR == null) throw new InvalidOperationException("The rear engine has not been built.");
            tenderEngineCar = tender;
            float s = tenderLayout.Scale;
            float cylinderFront = tenderNativeCylinderFront;
            // The tender is rail-level at its front coupler, like the body frame.
            float offset = TenderCylinderFrontZ - s * cylinderFront;
            tenderEngineRoot = new GameObject("[Rearranged tender engine]").transform;
            tenderEngineRoot.SetParent(tender.transform, false);

            // Drives: copies of the configured four-axle rear engine.
            tenderDriveL = CloneEngineDrive(bigMalletRearL, lfAnimator, "[Rearranged tender drivetrain L]", tenderEngineRoot, d => { });
            tenderDriveR = CloneEngineDrive(bigMalletRearR, rfAnimator, "[Rearranged tender drivetrain R]", tenderEngineRoot, d => { });
            foreach (var (drive, mirrored) in new[] { (tenderDriveL, true), (tenderDriveR, false) })
            {
                drive.localPosition = new Vector3(0f, 0f, offset);
                drive.localRotation = Quaternion.identity;
                drive.localScale = new Vector3(mirrored ? -1f : 1f, s, s);
                drive.gameObject.SetActive(true);
            }

            // Cylinders, guides, brakes and springs: the rear engine's, moved by
            // the rear-to-tender engine offset (body frame = tender frame axes).
            var body = MalletBody;
            var shift = Matrix4x4.Translate(new Vector3(0f, 0f, offset - tenderLayout.RearOffset));
            foreach (var source in bigMalletStatics[WheelArrangementType.s28882].Statics.Where(t => t != null))
            {
                var bodyPose = shift * tenderStaticMaps[source];
                var clone = Object.Instantiate(source.gameObject, tenderEngineRoot).transform;
                clone.name = "[Rearranged tender] " + source.name.Replace("[Rearranged eight-coupled rear] ", "");
                if (PacificHudsonAffine.TryDecompose(bodyPose, source.lossyScale, out var position, out var rotation, out var scale))
                { clone.localPosition = position; clone.localRotation = rotation; clone.localScale = scale; }
                else clone.localPosition = bodyPose.MultiplyPoint3x4(Vector3.zero);
                clone.gameObject.SetActive(true);
                EnableGeneratedRenderers(clone);
            }

            // The tender's own bogie wheels and frames give way to the engine,
            // except the rearmost wheelset, which becomes the trailing truck.
            var rearmost = tender.Bogies.SelectMany(b => b.Axles.Select(a => a.transform))
                .OrderBy(a => tender.transform.InverseTransformPoint(a.position).z).FirstOrDefault();
            foreach (var bogie in tender.Bogies)
            {
                var frame = bogie.transform.Find("bogie_car");
                if (frame == null) continue;
                foreach (Transform child in frame)
                {
                    bool axle = child.name.StartsWith("[axle]", StringComparison.Ordinal);
                    if (axle && child == rearmost) continue;
                    var targets = axle ? child.Cast<Transform>().Select(t => t.gameObject) : new[] { child.gameObject };
                    foreach (var go in targets)
                        if (go.activeSelf && go.name != "ContactPoints") { go.SetActive(false); tenderHidden.Add(go); }
                }
            }

            // Native controller and rods retained; tender animation uses its
            // own engine frequency so different host diameters do not alter wheel speed.
            var animationClock = tenderEngineRoot.gameObject.AddComponent<TenderDriveAnimation>();
            animationClock.Initialize(articulatedSimulation, tenderDriveL.GetComponent<Animator>(), tenderDriveR.GetComponent<Animator>(), lfAnimator, rfAnimator);
            articulatedSimulation.SetTenderEngine(true);
            // Presentation failure must never remove a working tender engine.
            try
            {
                tenderPresentation = tenderEngineRoot.gameObject.AddComponent<ErieTenderPresentation>();
                tenderPresentation.Releasing = () => ArticulatedEffects.TenderChanged(GetComponent<TrainCar>());
                tenderPresentation.Initialize(tender, steamNative.First().Materials);
            }
            catch (Exception ex) { Main.Logger.LogException("Erie tender exhaust presentation", ex); }
            ArticulatedEffects.TenderChanged(GetComponent<TrainCar>());
            RefreshSteamConnectionsForTender();
            Main.Logger.Log($"Erie triplex: tender engine attached to {tender.ID}; {2f * tenderLayout.Radius:0.00} m drivers, " +
                $"cylinders at {TenderCylinderFrontZ:0.00}, drivers {offset + s * firstLeftDriveWheel.localPosition.z:0.00}..{offset + s * fourthLeftDriveWheel.localPosition.z:0.00} (tender z).");
        }

        private void DetachTenderEngine()
        {
            articulatedSimulation?.SetTenderEngine(false);
            foreach (var go in tenderHidden) if (go != null) go.SetActive(true);
            tenderHidden.Clear();
            if (tenderEngineRoot != null) tenderEngineRoot.gameObject.SetActive(false);
            ArticulatedEffects.TenderChanged(GetComponent<TrainCar>());
            if (tenderEngineRoot != null)
            {
                // Remove native readers while their leaves still exist.
                tenderPresentation?.Release();
                Object.Destroy(tenderEngineRoot.gameObject);
            }
            if (tenderEngineCar != null && tenderEngineRoot != null) Main.Logger.Log($"Erie triplex: tender engine detached from {tenderEngineCar.ID}.");
            tenderEngineRoot = tenderDriveL = tenderDriveR = null;
            tenderPresentation = null;
            tenderEngineCar = null;
            tenderForceN = tenderLimitN = tenderDemandN = 0f; tenderSlipping = false;
            RefreshSteamConnectionsForTender();
        }

        // Pull at the tender's rails, limited by the tender's own adhesion.
        private void ApplyTenderForce(TrainCar loco)
        {
            var tender = tenderEngineCar;
            if (tender == null || tender.massController == null) return;
            float radius = .712f * tenderLayout.Scale;
            tenderDemandN = radius > 0f ? articulatedSimulation.TenderTorque / radius : 0f;
            tenderLimitN = TenderEngineFriction * tender.massController.TotalMass * -Physics.gravity.y * TenderDriverWeightShare;
            tenderSlipping = Mathf.Abs(tenderDemandN) > tenderLimitN;
            tenderForceN = Mathf.Clamp(tenderDemandN, -tenderLimitN, tenderLimitN);
            if (tenderForceN == 0f || tender.Bogies == null || tender.Bogies.Length == 0) return;
            if (tender.isEligibleForSleep) tender.ForceOptimizationState(sleep: false);
            float perBogie = tenderForceN / tender.Bogies.Length;
            var forward = loco.transform.forward;
            foreach (var bogie in tender.Bogies)
            {
                if (bogie == null || !bogie.enabled || bogie.rb == null || bogie.rb.isKinematic) continue;
                var axis = bogie.transform.forward;
                bogie.rb.AddForce(axis * (Mathf.Sign(Vector3.Dot(axis, forward)) * perBogie));
            }
        }

        internal JObject TenderEngineReport() => !poweredTenderSelected ? null : new JObject
        {
            ["selected"] = poweredTenderSelected, ["hostArrangement"] = (int)currentWA,
            ["driverRadiusM"] = tenderLayout != null ? .712f * tenderLayout.Scale : (float?)null,
            ["tender"] = tenderEngineCar != null ? tenderEngineCar.ID : null, ["attached"] = tenderEngineRoot != null,
            ["exhaustOutlet"] = TenderExhaustOutlet != null, ["visualEffects"] = ArticulatedEffects.TenderParticlesReady(GetComponent<TrainCar>()),
            ["audioEffects"] = ArticulatedEffects.TenderAudioReady(GetComponent<TrainCar>()),
            ["demandN"] = tenderDemandN, ["limitN"] = tenderLimitN, ["appliedN"] = tenderForceN, ["slipping"] = tenderSlipping,
            ["tenderMassKg"] = tenderEngineCar != null && tenderEngineCar.massController != null ? tenderEngineCar.massController.TotalMass : (float?)null
        };
    }
}
#endif
