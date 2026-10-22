#if B99_ALL_STANDARD
using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        internal float EffectiveDriverWeightPerAxleN
        {
            get
            {
                var car = GetComponent<TrainCar>();
                int powered = car?.SimController?.poweredWheels?.poweredWheels?.Length ?? 0;
                if (car?.massController == null || powered == 0) return 0f;
                return float.IsNaN(preliminaryDriverShare) ? car.massController.WeightPerAxle
                    : car.massController.TotalMass * -Physics.gravity.y * preliminaryDriverShare / powered;
            }
        }

        internal JObject PreliminaryPhysicsReport() => new JObject {
            ["effectiveDriverShare"] = float.IsNaN(preliminaryDriverShare) ? JValue.CreateNull() : new JValue(preliminaryDriverShare),
            ["effectiveDriverWeightPerAxleN"] = EffectiveDriverWeightPerAxleN,
            ["cylinderAreaScale"] = cylinderCalibration?.AreaScale ?? 1f,
            ["dimensionsConsistent"] = cylinderCalibration == null || cylinderCalibration.Consistent,
            ["referencePullBeforeN"] = cylinderCalibration?.BeforeForceN ?? 0d,
            ["referencePullAfterN"] = cylinderCalibration?.AfterForceN ?? 0d,
            ["referenceGripN"] = cylinderCalibration?.ReferenceGripN ?? 0d,
            ["stockReferenceRatioCeiling"] = cylinderCalibration?.TargetRatio ?? 0d,
            ["designReferencePullGripCeiling"] = cylinderCalibration?.TargetRatio ?? 0d,
            ["designMassKg"] = ConventionalPhysics.ReferenceOperatingMassKg,
            ["nominalPressureFraction"] = TractiveEffortModel.NominalPressureFraction,
            ["minimumDesignFactorOfAdhesion"] = cylinderCalibration?.MinimumFactorOfAdhesion ?? 0d,
            ["tenderMinimumDesignFactorOfAdhesion"] = cylinderCalibration?.TenderMinimumFactorOfAdhesion ?? 0d,
            ["tenderCylinderAreaScale"] = cylinderCalibration?.TenderAreaScale ?? 1f,
            ["limitations"] = "Rated 124 t effective adhesion and reduction-only cylinder design. Reference grip is at rated mass; live grip follows actual resource mass and native rail/sanding state. Legacy stockReferenceRatioCeiling aliases the design ceiling. No physical axle-mass redistribution. Erie tender cylinders designed separately at 50 t / .8 driven share; its simplified traction clamp remains. No sustained-power or speed guarantee."
        };

        internal JObject RearDriverFollowerReport()
        {
            var rows = new JArray();
            if (bigMallet != null && bigMallet.Spec.Rear == 5)
                foreach (bool left in new[] { true, false })
                {
                    var drive = CurrentBigMalletRearDrive(left);
                    if (drive == null || !drive.gameObject.activeInHierarchy) continue;
                    var fourth = drive.Find("s282_wheels_driving_4");
                    var fifth = drive.Find("s282_wheels_driving_5");
                    var follower = fifth?.GetComponent<PoweredWheelRotater>();
                    rows.Add(new JObject { ["side"] = left ? "left" : "right",
                        ["bound"] = follower != null && follower.enabled && follower.Source == fourth?.gameObject,
                        ["rotationGapDegrees"] = fourth != null && fifth != null ? Quaternion.Angle(fourth.localRotation, fifth.localRotation) : 360f });
                }
            bool required = currentWA == WheelArrangementType.s210102;
            return new JObject { ["required"] = required, ["wheels"] = rows,
                ["passed"] = !required || (rows.Count == 2 && rows.All(r => (bool)r["bound"] && (float)r["rotationGapDegrees"] < .1f)) };
        }
    }
}
#endif
