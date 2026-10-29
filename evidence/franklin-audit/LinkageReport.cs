using System;
using System.Collections.Generic;
using System.Linq;
using DV.Wheels;
using Newtonsoft.Json.Linq;
using RearrangedS282;
using UnityEngine;

internal static class LinkageReportFixture
{
    internal static int Run()
    {
        int checks=0;
        void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
        bool Valid(JArray rows,int count)=>rows.Count==count && rows.All(r=>
            new[]{"mainRod","crank"}.All(p=>new[]{"enabled","mesh","detailLod"}.All(f=>(bool)r[p][f])));
        Transform Child(Transform parent,string name){var go=new GameObject(name);go.transform.SetParent(parent);return go.transform;}
        foreach(int id in new[]{31,32,38,40,47}) foreach(bool tender in new[]{false,true})
        {
            var car=new GameObject("car");var rotation=car.Add<PoweredWheelRotationViaAnimation>();
            var native=new RuntimeAnimatorController{State=DriveQuartering.NativeState};
            Animator Drive(string name,RuntimeAnimatorController controller){var t=Child(car.transform,name);var a=t.gameObject.Add<Animator>();a.runtimeAnimatorController=controller;return a;}
            var left=Drive("front left",native);var right=Drive("front right",native);
            rotation.animatorSetups=new[]{new PoweredWheelRotationViaAnimation.AnimatorStartTimeOffsetPair{animator=left,startTimeOffset=.75f},new PoweredWheelRotationViaAnimation.AnimatorStartTimeOffsetPair{animator=right,startTimeOffset=0f}};
            var owner=new WheelRearranger(car,left,right);
            int count=id==31||id==32?4:6;
            var franklin=id==32?AssetBundleLoader.FranklinBAnimCtrl4444:AssetBundleLoader.FranklinBAnimCtrl;
            left.runtimeAnimatorController=right.runtimeAnimatorController=franklin;
            var drives=new List<Animator>{left,right};
            while(drives.Count<count)drives.Add(Drive("engine "+drives.Count,franklin));
            var all=drives.ToList();
            if(tender){all.Add(Drive("native tender left",native));all.Add(Drive("native tender right",native));}
            rotation.animatorSetups=all.Select((a,i)=>new PoweredWheelRotationViaAnimation.AnimatorStartTimeOffsetPair{animator=a,startTimeOffset=i%2==0?.75f:0f}).ToArray();
            owner.Arrangement(id);
            var renderers=new List<MeshRenderer>();
            foreach(var drive in drives)
            {
                var rod=Child(drive.transform,"s282_mech_wheels_connect");
                foreach(string name in new[]{"s282_mech_push_rod_to_connect","s282_mech_connect_to_cutoff_base"})
                {
                    var part=Child(rod,name);part.gameObject.Add<MeshFilter>().sharedMesh=new Mesh();
                    renderers.Add(part.gameObject.Add<MeshRenderer>());
                }
            }
            var lod=car.Add<LODGroup>();lod.SetLODs(new[]{new LOD(renderers.ToArray())});
            Check(Valid(owner.FranklinLinkageReport(),count),"Current Franklin mechanisms accepted, native tender excluded");
            var main=renderers[0];var crank=renderers[1];
            main.enabled=false;Check(!Valid(owner.FranklinLinkageReport(),count),"Hidden main rod rejected");main.enabled=true;
            crank.enabled=false;Check(!Valid(owner.FranklinLinkageReport(),count),"Hidden crank rejected");crank.enabled=true;
            var crankFilter=crank.GetComponent<MeshFilter>();var crankMesh=crankFilter.sharedMesh;
            crankFilter.sharedMesh=null;Check(!Valid(owner.FranklinLinkageReport(),count),"Missing crank mesh rejected");crankFilter.sharedMesh=crankMesh;
            lod.SetLODs(new[]{new LOD(renderers.Skip(1).ToArray())});Check(!Valid(owner.FranklinLinkageReport(),count),"Main outside detail LOD rejected");
            var mainFilter=main.GetComponent<MeshFilter>();var mainMesh=mainFilter.sharedMesh;mainFilter.sharedMesh=null;
            var stale=Child(main.transform,"[Rearranged retarget] "+main.name);stale.gameObject.SetActive(false);
            var proxy=Child(main.transform,"[Rearranged retarget] "+main.name);proxy.gameObject.Add<MeshFilter>().sharedMesh=mainMesh;var proxyRenderer=proxy.gameObject.Add<MeshRenderer>();
            lod.SetLODs(new[]{new LOD(renderers.Skip(1).Append(proxyRenderer).ToArray())});
            Check(Valid(owner.FranklinLinkageReport(),count),"Active direct retarget proxy accepted after old deferred proxy");
            proxyRenderer.enabled=false;Check(!Valid(owner.FranklinLinkageReport(),count),"Hidden presented proxy rejected");proxyRenderer.enabled=true;
            main.enabled=false;Check(!Valid(owner.FranklinLinkageReport(),count),"Hidden animated source cannot conceal next-frame proxy disable");main.enabled=true;
            proxy.gameObject.SetActive(false);Check(!Valid(owner.FranklinLinkageReport(),count),"Inactive presented proxy rejected");proxy.gameObject.SetActive(true);
            proxy.name="wrong proxy";Check(!Valid(owner.FranklinLinkageReport(),count),"Arbitrary child cannot replace owned retarget proxy");
            foreach(int ordinary in new[]{10,34,37,41,42,43,45,46}){owner.Arrangement(ordinary);Check(owner.FranklinLinkageReport().Count==0,"Other arrangements do not acquire Franklin requirements");}
        }
        return checks;
    }
}
