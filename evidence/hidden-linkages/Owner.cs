using System;
using System.Collections.Generic;
using UnityEngine;
namespace RearrangedS282
{
    internal partial class WheelRearranger:MonoBehaviour
    {
        internal Transform driveL,driveR,lubricator;
        private readonly List<Transform> pacificHudsonReachSupports=new();
        internal void CacheFixture()=>CacheHiddenLinkageFrames();
        internal void ApplyFixture(int id)=>ApplyHiddenNativeLinkages(id);
        internal void SuspendFixture()=>SuspendHiddenNativeLinkages();
        internal void SupportFixture(Transform t)=>pacificHudsonReachSupports.Add(t);
    }
    internal sealed partial class MeshFinder
    {
        internal static readonly MeshFinder Instance=new();
        internal static readonly Dictionary<Mesh,Mesh> ReadableSources=new();
        internal Mesh ReadableRetargetMesh(Mesh source)=>ReadableSources.TryGetValue(source,out var m)?UnityEngine.Object.Instantiate(m):null;
    }
}
