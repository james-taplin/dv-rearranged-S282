#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RearrangedS282
{
    internal partial class WheelRearranger
    {
        IndependentDrive independentDrive;
        void ReleaseIndependentDrive()
        {
            if(independentDrive==null) return;
            independentDrive.Release(); Destroy(independentDrive); independentDrive=null;
        }
        void ConfigureIndependentDrive(float cycle)
        {
            ReleaseIndependentDrive();
            int id=(int)currentWA;
            if(!WheelArrangement.IsArticulatedId(id)) return;
            var definitions=new List<IndependentDrive.Definition>();
            var roots=ArticulatedSimulation.IsTriplexId(id)?new[]{articulatedFrontRoot,articulatedMiddleRoot,articulatedRearRoot}:new[]{articulatedFrontRoot,articulatedRearRoot};
            void Add(string role,Transform root,Transform left,Transform right,LocoSim.Implementations.ReciprocatingSteamEngine engine)
            {
                var contacts=new int[loco.Bogies.Length];
                foreach(var wheel in loco.SimController.poweredWheels.poweredWheels)
                    if(wheel.wheelTransform!=null && wheel.wheelTransform.IsChildOf(root) &&
                        !roots.Any(other=>other!=root && other.IsChildOf(root) && wheel.wheelTransform.IsChildOf(other)))
                    {
                        int b=Array.FindIndex(loco.Bogies,bogie=>wheel.transform.IsChildOf(bogie.transform));
                        if(b<0) throw new InvalidOperationException("Independent powered axle has no native bogie.");
                        contacts[b]++;
                    }
                definitions.Add(new IndependentDrive.Definition {Role=role,Engine=engine,EngineRoot=root,Contacts=contacts,InitialPhases=new[]{cycle+nativeLeftPhase,cycle+nativeRightPhase},Animators=new[]{left.GetComponent<Animator>(),right.GetComponent<Animator>()}});
            }
            Add("front",articulatedFrontRoot,driveL,driveR,articulatedSimulation.MainEngine);
            if(ArticulatedSimulation.IsTriplexId(id))
            {
                Add("middle",articulatedMiddleRoot,triplexMiddleL,triplexMiddleR,articulatedSimulation.MiddleEngine);
                Add("rear",articulatedRearRoot,triplexRearL,triplexRearR,articulatedSimulation.RearEngine);
            }
            else
            {
                Transform left,right;
                if(IsBigMallet(id) || id==ArticulatedSimulation.TenderTriplexId) {left=CurrentBigMalletRearDrive(true);right=CurrentBigMalletRearDrive(false);}
                else if(id==(int)WheelArrangementType.s2662 || id==(int)WheelArrangementType.s4664LittleBoy) {left=malletRearL;right=malletRearR;}
                else {left=articulatedRearL;right=articulatedRearR;}
                Add("rear",articulatedRearRoot,left,right,articulatedSimulation.RearEngine);
            }
            independentDrive=gameObject.AddComponent<IndependentDrive>();
            independentDrive.Initialize(loco,articulatedSimulation,definitions.ToArray());
        }
    }
}
#endif
