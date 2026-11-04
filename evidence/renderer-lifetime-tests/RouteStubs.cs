// Native stock/pool bodies and the Franklin lease list are extracted from the
// production source. These narrow boundaries stand in for unrelated systems.
using System;
using System.Collections.Generic;
using UnityEngine;
namespace RearrangedS282 {
 internal enum WheelArrangementType { s282=10 }
 internal sealed class Geometry { public void Restore(){} }
 internal sealed class Physics { public void Restore(){} }
 internal sealed class Calibration { public void Restore(){} }
 internal sealed class Articulation { public void Restore(){} }
 internal static class ArticulatedEffects { internal static void Deinit(TrainCar car){} }
 internal static class DeltaTrailingTruckCompatibility { internal static void Clear(TrainCar car){} }
 internal static class Main { internal static readonly TestLogger Logger=new TestLogger(); internal sealed class TestLogger { internal void LogException(string message,Exception error){throw new Exception(message,error);} } }
 internal partial class RouteFixture {
  private readonly List<IDisposable> franklinPinLeases=new List<IDisposable>();
  private readonly TrainCar car;
  private TrainCar loco;
  private bool poweredTenderSelected;
  private float preliminaryDriverShare,derailModifier;
  private Calibration cylinderCalibration;
  private Articulation articulatedSimulation;
  private Geometry stockGeometry=new Geometry();
  private Physics stage2Physics=new Physics();
  private Transform[] generatedRoots=Array.Empty<Transform>();
  private WheelArrangementType currentWA;
  internal RouteFixture(TrainCar car,IDisposable lease){this.car=car;franklinPinLeases.Add(lease);}
  internal void SwitchToStock()=>RestoreConventionalStock();
  internal bool PoolReturn()=>ResetConventionalForPool();
  internal bool Stock=>currentWA==WheelArrangementType.s282;
  private T GetComponent<T>() where T:class => car as T;
  private float CaptureQuarteringCycle()=>0;
  private void SuspendArticulatedGeometry()=>SuspendFranklinDriverPins();
  private void ReleaseIndependentDrive(){} private void DetachTenderEngine(){} private void SuspendHiddenNativeLinkages(){}
  private void SuspendSteamConnections(){} private void SuspendConnectingRodSpacing(){} private void RestoreCylinderCasings(){}
  private void SuspendAdditionalGeometry(){} private void Suspend4144Geometry(){} private void SuspendDriverLayout(){}
  private void ApplyDriverQuartering(float cycle,bool restore){} private void RefreshConventionalRollingAudio(){}
  private void UpdateExternalControlPositions(){} private void ClearBigEndCups(){}
 }
}
