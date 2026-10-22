using UnityEngine;
using System;
using System.Collections.Generic;
namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        internal Transform lfCrossheadBracket,rfCrossheadBracket;
        internal WheelArrangementType currentWA=WheelArrangementType.s260;
        internal void CacheConnectingFixture(){CacheOilCupMounts();CacheConnectingRodSpacing();}
        internal void ApplyConnectingFixture(int id,float existingShift=0f){currentWA=(WheelArrangementType)id;ApplyConnectingRodSpacing(id,existingShift);ApplyOilCupMounts();}
        private readonly List<FixtureDriverFitting> prairieDriverFittings=new();
        private sealed class FixtureDriverFitting { internal void Apply(Transform drive){} }
        internal float PreparePrairieFixture(){ApplyLargeDriverClearance((int)WheelArrangementType.s262,.9144f);ApplyPrairieForwardSpacing(false);return PrairieCylinderForward-PrairieDriverForward+prairieClearanceCylinderShiftM;}
        internal void SuspendConnectingFixture()=>SuspendConnectingRodSpacing();
        internal void DisposeConnectingFixture()=>DisposeConnectingRodSpacing();
    }
    // Tests the provider contract using actual extracted native mesh data.
    // AssetStudio/OBJ subprocess integration remains a compiled/source check.
    internal sealed class MeshFinder
    {
        internal static readonly MeshFinder Instance=new();
        internal static readonly Dictionary<string,Mesh> ReadableSources=new();
        internal static readonly List<string> Requests=new();
        internal static int FailAtRequest=-1;
        internal Mesh ReadableRetargetMesh(Mesh source)
        {
            Requests.Add(source.name);
            if(Requests.Count==FailAtRequest || !ReadableSources.TryGetValue(source.name,out var data))return null;
            return UnityEngine.Object.Instantiate(data);
        }
    }
}
internal static class ConnectingHarness
{
    private static void Main(string[] args)=>Program.RunConnectingTests(args);
}
