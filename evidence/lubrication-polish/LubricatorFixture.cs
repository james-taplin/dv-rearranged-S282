using System;
using UnityEngine;

public sealed class LubricatorRatchetDriver : MonoBehaviour { }
public sealed class LubricatorRatchet : MonoBehaviour
{
    public Transform ratchetDriverTransform;
    public Vector3 localAxis;
    public float prevAngle;
}
public sealed class Livery { public GameObject externalInteractablesPrefab; }
namespace RearrangedS282
{
    internal enum WheelArrangementType { s282, s404, s222 }
    internal partial class WheelRearranger
    {
        private Transform driveR, lubricatorControlPivot;
        private Animator rfAnimator;
        private object rfDefaultAnimatorCtrl = new();
        private TrainCar loco = new();
        private WheelArrangementType currentWA;

        internal static int TestLubricator()
        {
            int checks=0;
            void Check(bool ok,string why){if(!ok)throw new Exception(why);checks++;}
            foreach(bool missing in new[]{false,true})
            foreach(bool hidden in new[]{false,true})
            {
                var owner=new WheelRearranger();
                owner.driveR=new GameObject("drive").transform;
                owner.rfAnimator=owner.driveR.gameObject.AddComponent<Animator>();
                owner.rfAnimator.runtimeAnimatorController=owner.rfDefaultAnimatorCtrl;
                var group=owner.driveR.Add("LubricatorParts");
                var source=group.Add("source").gameObject.AddComponent<LubricatorRatchetDriver>();
                source.transform.localEulerAngles=new Vector3(25,0,0);
                var renderer=source.gameObject.AddComponent<MeshRenderer>();
                group.gameObject.SetActive(!hidden);
                owner.lubricatorControlPivot=new GameObject("pivot").transform;
                if(!missing)owner.lubricatorControlPivot.gameObject.AddComponent<LubricatorRatchet>();
                var prefab=new GameObject("prefab");
                var template=prefab.transform.Add("Interactables").Add("lubricator pivot").gameObject.AddComponent<LubricatorRatchet>();
                template.localAxis=new Vector3(1,0,0);
                owner.loco.carLivery.externalInteractablesPrefab=prefab;
                owner.RestoreNativeLubricatorAnimation();
                var ratchet=owner.lubricatorControlPivot.GetComponent<LubricatorRatchet>();
                Check(group.gameObject.activeInHierarchy,"Native animation reference inactive");
                Check(renderer.enabled==!hidden,"Hidden geometry became visible or stock hidden");
                Check(ratchet.ratchetDriverTransform==source.transform,"Ratchet not rebound");
                Check(Math.Abs(ratchet.prevAngle-115)<.0001f,"Phase baseline wrong");
                Check(owner.NativeLubricatorAnimationBound(),"Diagnostic rejects valid binding");
                if(missing)Check(ratchet.localAxis.x==1,"Native axis not recovered");
                owner.RestoreNativeLubricatorAnimation();
                Check(owner.lubricatorControlPivot.GetComponents<LubricatorRatchet>().Length==1,"Duplicated ratchet");
                Check(renderer.enabled==!hidden,"Repeated repair exposed hidden geometry");
                group.gameObject.SetActive(false);
                Check(!owner.NativeLubricatorAnimationBound(),"Diagnostic missed hidden source");
                owner.rfAnimator.runtimeAnimatorController=new object();
                owner.RestoreNativeLubricatorAnimation();
                Check(!group.gameObject.activeSelf,"Franklin controller modified");
            }
            return checks;
        }
    }
}
