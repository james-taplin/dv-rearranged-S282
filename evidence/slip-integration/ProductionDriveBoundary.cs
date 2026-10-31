// Only the car/bogie/replication boundary is synthetic. The companion methods
// are extracted verbatim from IndependentDrive.cs by the build script. Engine,
// RPM ports, native ECS outputs and production force routing are real.
using System;
using System.Linq;
using DV.Utils;
using DV.Simulation.Cars;
using DV.ECS.Train;
using LocoSim.Implementations;
using Unity.Entities;
using UnityEngine;

namespace RearrangedS282
{
    internal static class MultiplayerSync
    {
        internal static bool IsSimulationAuthority = true;
    }

    internal sealed partial class ProductionDriveBoundary
    {
        internal sealed class Definition
        {
            internal ReciprocatingSteamEngine Engine;
            internal int[] Contacts;
        }
        internal sealed class Group
        {
            internal Definition Definition;
            internal float Rpm, Velocity, Force, Brake, Demand;
            internal Port RpmPort;
        }
        internal sealed class Body { internal bool isKinematic; }
        internal sealed class Bogie
        {
            internal bool enabled = true;
            internal Body rb = new Body();
            internal float brakingForce;
            internal float Applied;
            internal int Calls;
            internal void ApplyForce(float force) { Applied += force; Calls++; }
        }
        internal sealed class Adhesion { internal float wheelSlide; }
        internal sealed class Mass { internal float WeightPerAxle; }
        internal sealed class Ground { internal bool IsGrounded = true; }
        internal sealed class SlipController
        {
            internal float SandCoef = 1, maxWheelslipRpm = 370;
            internal AnimationCurve wheelslipToAdhesionDrop = AnimationCurve.Linear(0, 1, 1, 1);
            internal bool preventWheelslip, IsEngineBraking;
        }
        internal sealed class Sim { internal SlipController wheelslipController = new SlipController(); }
        internal sealed class EntityReference { internal EntityManager EntityManager; internal Entity Entity; }
        internal sealed class Car
        {
            internal bool derailed, isExploded, isEligibleForSleep = true;
            internal float Speed;
            internal Adhesion adhesionController = new Adhesion();
            internal Mass massController = new Mass();
            internal Ground groundFriction = new Ground();
            internal Sim SimController = new Sim();
            internal EntityReference entity;
            internal Transform transform;
            internal Bogie[] Bogies = { new Bogie(), new Bogie() };
            internal Bogie RearBogie => Bogies[1];
            internal float GetForwardSpeed() => Speed;
            internal float GetAbsSpeed() => Mathf.Abs(Speed);
            internal void ForceOptimizationState(bool sleep) => isEligibleForSleep = sleep;
        }

        bool authority = true;
        bool released;
        int powered = 4;
        internal float DiagnosticFirstGroupGrip = 1;
        readonly float radius;
        readonly NativeSlipRuntimeState state;
        internal readonly Car car = new Car();
        internal readonly Group[] groups;
        internal bool Released;
        internal int Replications;

        internal ProductionDriveBoundary(NativeSlipRuntimeState native, ReciprocatingSteamEngine[] engines, Port[] ownedRpm, float wheelRadius, EntityManager manager, Transform parent)
        {
            state = native; radius = wheelRadius;
            car.transform = parent;
            var entity = manager.CreateEntity(typeof(AdhesionControllerSystem.StaticSharedAdhesionCalculationData),
                typeof(AdhesionControllerSystem.StaticAdhesionCalculationData), typeof(AdhesionControllerSystem.AdhesionCalculationData),
                typeof(AdhesionControllerSystem.WheelSlideData));
            manager.SetComponentData(entity, new AdhesionControllerSystem.StaticSharedAdhesionCalculationData { bogieCount = 2, numOfAxles = 4 });
            manager.SetComponentData(entity, new AdhesionControllerSystem.StaticAdhesionCalculationData { wheelslipFrictionCoef = .24f, wheelSlideFrictionCoef = .24f });
            car.entity = new EntityReference { EntityManager = manager, Entity = entity };
            groups = engines.Select((e, i) => new Group {
                Definition = new Definition { Engine = e, Contacts = new[] { i == 0 ? 2 : 0, i == 1 ? 2 : 0 } },
                Rpm = ownedRpm[i].Value, RpmPort = ownedRpm[i]
            }).ToArray();
        }
        void Release() { Released = released = true; }
        void PublishReplication() => Replications++;
        internal void TickRpm(float dt) => PublishRpm(dt);
        internal void TickInputs()
        {
            car.entity.EntityManager.SetComponentData(car.entity.Entity, new AdhesionControllerSystem.WheelSlideData { wheelSlide = car.adhesionController.wheelSlide });
            PrepareCore();
        }
        internal bool Apply(DrivingForce nativeForce)
        {
            foreach (var bogie in car.Bogies) { bogie.Applied = 0; bogie.Calls = 0; }
            return ApplyForce(nativeForce);
        }
    }
}
