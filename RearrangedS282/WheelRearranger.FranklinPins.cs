#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        private readonly List<IDisposable> franklinPinLeases=new List<IDisposable>();

        private void ApplyFranklinDriverPins(int id)
        {
            SuspendFranklinDriverPins();
            if(id!=31&&id!=32&&id!=38&&id!=40&&id!=47)return;
            var drives=id==38||id==40||id==47?TriplexDrives():new[]{driveL,driveR,articulatedRearL,articulatedRearR};
            try
            {
                foreach(var drive in drives)
                    if(drive!=null)franklinPinLeases.Add(FranklinPinPresentation.Bind(drive,blindDriverBigWeight,transform,MeshFinder.Instance.ReadableRetargetMesh));
            }
            catch{SuspendFranklinDriverPins();throw;}
        }

        private void SuspendFranklinDriverPins()
        {
            // Later leases can extend a shared body LOD group. Unwind them in
            // reverse before prepared/native geometry snapshots restore meshes.
            for(int i=franklinPinLeases.Count-1;i>=0;i--)franklinPinLeases[i].Dispose();
            franklinPinLeases.Clear();
        }
    }
}
#endif
