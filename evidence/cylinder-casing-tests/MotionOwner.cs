using System;
using System.Linq;
using UnityEngine;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        internal Transform transform,driveL,driveR,lCrosshead,rCrosshead,lLiftingArmSupport,rLiftingArmSupport,
            reachRodSupport,lfCrossheadBracket,rfCrossheadBracket,firstRightDriveWheel,secondRightDriveWheel;
        internal Transform spark1L,spark1R,spark2L,spark2R,spark3L,spark3R,spark4L,spark4R;
        internal Transform articulatedRearL,articulatedRearR,triplexMiddleL,triplexMiddleR;
        private Transform tenderEngineRoot=null;
        internal WheelArrangementType currentWA=WheelArrangementType.s262;
        internal void PreparePrairie(){CacheOilCupMounts();CacheCylinderCasings();SetupPacificHudsonValveGear();}
        internal void Apply()=>ApplyPacificHudsonValveGear();
        internal void ApplyForward(){ApplyPrairieForwardSpacing();ApplyPacificHudsonValveGear();ApplyCylinderCasings(44);ApplyOilCupMounts();}
        internal void RefreshOilMounts()=>ApplyOilCupMounts();
        internal void RestoreCasings()=>RestoreCylinderCasings();
        internal void DisposeCasings()=>DisposeCylinderCasings();
        internal void Dispose()=>DisposePacificHudsonValveGear();
        internal void ApplyCasingFor(int id)=>ApplyCylinderCasings(id);
        internal Vector3 CylinderReferencePoint()=>CylinderDriveFrame(driveR).MultiplyPoint3x4(new Vector3(0,0,CylinderCasingShape.NativeRearFaceZ));
        internal float LastRadius;
        private void setWheelRadius(float r)=>LastRadius=r;
        internal void ApplyClearanceCase(int id){ApplyLargeDriverClearance(id,id==44?.9144f:.8509f);if(id==44)ApplyForward();else{ApplyPacificHudsonValveGear(false);ApplyCylinderCasings(id);ApplyOilCupMounts();}}
        internal bool FullCasingFor(int id)=>UsesFullCylinderCasings(id);
        private T[] GetComponentsInChildren<T>(bool inactive) where T:class=>transform.GetComponentsInChildren<T>(inactive);
        private void DetachTenderEngine()=>throw new InvalidOperationException("No tender in motion fixture");
        private void ShowValveGear(){reachRodSupport.gameObject.SetActive(true);}
        private void HideReachRod(){foreach(var d in new[]{driveL,driveR})d.Find("s282_mech_cutoff_rod").gameObject.SetActive(false);}
    }
    internal enum WheelArrangementType{s440=0,s442=1,s444=2,s460=3,s462=4,s464=5,s282=10,s280Big=12,s282Big=13,s284Big=14,s0120=27,s4122=30,s4444=32,s404=33,s260=34,s240=35,s222=36,s22222=38,s262=44,s24440=47,s482Big=48}
    internal static class WheelArrangement
    {
        // This rigid-layout harness does not model articulated assembly setup.
        internal static bool IsArticulatedId(int id)=>false;
        internal static string[] WheelArrangementNames=Enumerable.Range(0,49).Select(i=>"fixture "+i).ToArray();
    }
    internal static class AssetBundleLoader{internal static GameObject FranklinBValveGear=null,FranklinBValveGear4444=null;}
}
