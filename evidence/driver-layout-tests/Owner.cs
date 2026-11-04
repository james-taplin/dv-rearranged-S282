using System;
using System.Linq;
using UnityEngine;
#pragma warning disable CS0649 // Native fields outside this scoped layout path are deliberately unused.

namespace RearrangedS282
{
    internal partial class WheelRearranger : MonoBehaviour
    {
        internal TrainCar loco;
        internal Transform driveL,driveR,leftSideRod,rightSideRod,leftTwoAxleSideRod,rightTwoAxleSideRod,
            lfThreeAxleSideRod,lrThreeAxleSideRod,rfThreeAxleSideRod,rrThreeAxleSideRod;
        internal Transform firstLeftDriveWheel,secondLeftDriveWheel,thirdLeftDriveWheel,fourthLeftDriveWheel,
            firstRightDriveWheel,secondRightDriveWheel,thirdRightDriveWheel,fourthRightDriveWheel;
        internal Transform driveAxle1,driveAxle2,driveAxle3,driveAxle4,driveAxle5,driveAxle6;
        internal Transform leftFirstBrakeCaliper,leftSecondBrakeCaliper,leftThirdBrakeCaliper,leftFourthBrakeCaliper,leftFifthBrakeCaliper,leftSixthBrakeCaliper,
            rightFirstBrakeCaliper,rightSecondBrakeCaliper,rightThirdBrakeCaliper,rightFourthBrakeCaliper,rightFifthBrakeCaliper,rightSixthBrakeCaliper;
        internal Transform leftFirstBrakeShoe,leftSecondBrakeShoe,leftThirdBrakeShoe,leftFourthBrakeShoe,
            rightFirstBrakeShoe,rightSecondBrakeShoe,rightThirdBrakeShoe,rightFourthBrakeShoe;
        internal Transform spark1L,spark2L,spark3L,spark4L,spark5L,spark6L,spark1R,spark2R,spark3R,spark4R,spark5R,spark6R;
        internal Mesh flangedDriver;
        private object curveGroups;
        private B99PhysicsState stage2Physics=new();
        internal float LastRadius;
        private void setWheelRadius(float radius)=>LastRadius=radius;
        internal void CachePitch()=>CacheDriverLayouts();
        internal void ApplyPitch(int id)=>ApplyDriverLayout(id);
        internal void SuspendPitch()=>SuspendDriverLayout();
        internal void DisposePitch()=>DisposeDriverLayouts();
        internal void AlignPitch(int id)=>AlignAdditionalPoweredAxles(id);
        internal bool CurveCacheCleared=>curveGroups==null;
        internal void SeedCurveCache()=>curveGroups=new object();
        internal int CachedPitchMeshCount=>pitchRodMeshes.Count;
    }
    internal enum WheelArrangementType{s440=0,s280Big=12,s282Big=13,s284Big=14,s260=34,s240=35,s222=36,s262=44,s482Big=48}
    internal static class WheelArrangement
    {
        internal static string[] WheelArrangementNames=Enumerable.Range(0,49).Select(i=>"fixture "+i).ToArray();
    }
    internal sealed class B99PhysicsState{internal float DriverRadius=.712f;}
}
internal sealed class TrainCar{internal FixtureLivery carLivery;}
internal sealed class FixtureLivery{internal GameObject prefab;}
