#if B99_ALL_STANDARD
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        private ErieTenderPolish tenderPolish;

        // Prepared-native pose only. Reuse the same frame cutter and engine map
        // as the locomotive, retaining the authored frame openings and materials.
        private void BuildTenderFrameSources()
        {
            foreach (string name in ArticulatedSplitFrames)
            {
                var source = RequireArticulatedPart(splitLoco, name);
                var normal = source.localToWorldMatrix.transpose.MultiplyVector(transform.forward).normalized;
                var rear = source.InverseTransformPoint(MalletBody.TransformPoint(new Vector3(0, 0, NativeDriverZ(4) - .4f)));
                var front = source.InverseTransformPoint(MalletBody.TransformPoint(new Vector3(0, 0, NativeDriverZ(1) + .4f)));
                var slice = ArticulatedFrameClipper.Clip(source.GetComponent<MeshFilter>().sharedMesh, rear, normal, true);
                Mesh mesh;
                try { mesh = ArticulatedFrameClipper.Clip(slice, front, normal, false); }
                finally { Object.Destroy(slice); }
                articulatedMeshes.Add(mesh);
                if (mesh.vertexCount == 0) throw new InvalidOperationException("Tender bar-frame slice is empty: " + name);
                var clone = new GameObject("[Rearranged tender bar frame] " + name).transform;
                clone.SetParent(source.parent, false);
                clone.localPosition = source.localPosition; clone.localRotation = source.localRotation; clone.localScale = source.localScale;
                clone.gameObject.layer = source.gameObject.layer;
                clone.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                clone.gameObject.AddComponent<MeshRenderer>().sharedMaterials = source.GetComponent<MeshRenderer>().sharedMaterials;
                try
                {
                    MapMalletPart(clone, tenderLayout.Rear);
                    var map = MalletBody.worldToLocalMatrix * clone.localToWorldMatrix;
                    clone.SetParent(tenderSourceRoot, true);
                    tenderStaticMaps.Add(clone, map);
                }
                catch { Object.Destroy(clone.gameObject); throw; }
            }
        }
    }
}
#endif
