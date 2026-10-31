using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LocoSim.Implementations;
using Newtonsoft.Json.Linq;
using RearrangedS282;
using UnityEngine;
internal static class DriveProbe
{
    static readonly List<string> checks=new();
    static void Check(bool result,string label){if(!result)throw new Exception(label);checks.Add(label);}
    static object Invoke(object owner,string method,params object[] args)=>typeof(IndependentDrive).GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static).Invoke(owner,args);
    sealed class Car
    {
        internal readonly TrainCar Vehicle;
        internal readonly IndependentDrive Owner;
        internal readonly ArticulatedSimulation Simulation;
        internal readonly IndependentDrive.Definition[] Definitions;
        internal readonly IndependentDriveReplication Outputs;
        internal readonly Port[] OriginalRpm;
        internal Car()
        {
            var go=new GameObject();Vehicle=go.AddComponent<TrainCar>();Owner=go.AddComponent<IndependentDrive>();
            var flow=new SimulationFlow(IndependentDriveReplication.AppendDefinitions(go,Array.Empty<LocoSim.Definitions.SimComponentDefinition>()));
            Simulation=new ArticulatedSimulation{Flow=flow};Outputs=new IndependentDriveReplication(flow,Vehicle.CarGUID);
            Definitions=Enumerable.Range(0,2).Select(i=>new IndependentDrive.Definition {
                Engine=new ReciprocatingSteamEngine("engine"+i),Role="group"+i,
                Contacts=i==0?new[]{2,0}:new[]{0,2},EngineRoot=new GameObject().transform,
                Animators=new[]{new GameObject().AddComponent<Animator>(),new GameObject().AddComponent<Animator>()},
                InitialPhases=new[]{.1f,.35f}
            }).ToArray();
            OriginalRpm=Definitions.Select(d=>d.Engine.crankRpm.port).ToArray();
            Owner.Initialize(Vehicle,Simulation,Definitions);
        }
        internal void Update()=>Invoke(Owner,"Update");
        internal bool NativeForceAllowed()=>(bool)Invoke(null,"BeforeForce",Vehicle.SimController.drivingForce);
    }
    static void Role(bool authority){MultiplayerSync.IsSimulationAuthority=authority;MultiplayerSync.IsClient=!authority;}
    static int Main(string[] args)
    {
        try
        {
            Role(false);MultiplayerSync.Revision=10;
            var guest=new Car();
            Check(NativeSlipRuntimeState.Created==0,"Guest arrangement creates no local native ECS slip state");
            Check(!guest.NativeForceAllowed(),"Guest ownership blocks the whole-car native force writer before replicated state arrives");
            Check((bool)Invoke(null,"BeforeForce",new DV.Simulation.Cars.DrivingForce()),"Unowned car native force behavior is untouched");
            guest.Update();
            Check(!guest.Owner.IsOwning&&GroupCrankAnimation.Offsets.Count==0,"Guest waits for the matching host outputs without binding stale local animation");
            Role(true);
            foreach(var output in guest.Outputs.Groups)output.Publish(true,142,.5f,.2f,.45f,400,500);
            guest.Outputs.PublishMarker(MultiplayerSync.SessionEpoch,9,2);Role(false);guest.Update();
            Check(!guest.Owner.Ready,"Older native revision cannot activate the guest's newly applied arrangement");
            Role(true);guest.Outputs.PublishMarker(MultiplayerSync.SessionEpoch,10,2);Role(false);
            int[] before=guest.Simulation.Flow.ports.Values.Select(p=>p.writes).ToArray();
            guest.Update();
            Check(guest.Owner.IsOwning&&GroupCrankAnimation.Offsets.Count==2,"Matching host revision activates both guest mechanism readers");
            Check(GroupCrankAnimation.Offsets.All(o=>o.SequenceEqual(new[]{.2f,.45f})),"Guest mechanisms use exact host phase offsets rather than a local initial pose");
            Check(guest.Definitions.All(d=>d.Engine.crankRpm.references==0),"Guest never replaces native engine RPM input references");
            Check(guest.Simulation.Flow.ports.Values.Select(p=>p.writes).SequenceEqual(before),"Guest animation update never writes replicated native simulation ports");
            var wheel=new LocoSim.Implementations.Wheels.PoweredWheel{wheelTransform=new GameObject().transform};wheel.wheelTransform.parent=guest.Definitions[1].EngineRoot;
            Check(guest.Owner.TryGetWheelState(wheel,out var slip,out var rpm)&&slip==.5f&&rpm==142,"Guest spark/audio inputs read the host's matching per-group slip and RPM");
            Check(!guest.NativeForceAllowed()&&guest.Vehicle.Bogies.All(b=>b.appliedForce==0),"Active guest neither executes original drive force nor applies per-group bogie forces");
            int bound=GroupCrankAnimation.Offsets.Count;
            Role(true);guest.Outputs.Groups[0].Publish(true,142,.5f,.3f,.55f,400,500);Role(false);guest.Update();
            Check(GroupCrankAnimation.Offsets.Count==bound+2&&GroupCrankAnimation.Offsets[bound].SequenceEqual(new[]{.3f,.55f}),"Fresh native host phase baseline replaces an already active replica baseline");
            bound=GroupCrankAnimation.Offsets.Count;guest.Update();
            Check(GroupCrankAnimation.Offsets.Count==bound,"Unchanged native baseline retains existing animation leases");
            Role(true);guest.Outputs.PublishMarker(MultiplayerSync.SessionEpoch,11,2);Role(false);guest.Update();
            Check(!guest.Owner.IsOwning&&GroupCrankAnimation.Releases==4,"Revision mismatch releases guest mechanism leases immediately");
            Check(NativeSlipRuntimeState.Created==0&&NativeSlipRuntimeState.GroupWrites==0,"Guest initialization and repeated updates never construct or advance ECS group inputs");
            guest.Owner.Release();guest.Owner.Release();
            Check(guest.Definitions.All(d=>d.Engine.crankRpm.references==0),"Repeated guest teardown leaves native RPM references untouched");
            Role(true);MultiplayerSync.Revision=12;
            var host=new Car();
            Check(NativeSlipRuntimeState.Created==1,"Offline/host arrangement owns exactly one native ECS slip state");
            Check(host.NativeForceAllowed(),"Host retains native force fallback until its first completed native slip sample");
            NativeSlipRuntimeState.Latest.Complete();
            Check(host.Owner.IsOwning&&host.Definitions.All(d=>d.Engine.crankRpm.references==0 && d.Engine.crankRpm.port.id.EndsWith(".INDEPENDENT_RPM")),"First host native sample leases connected RPM inputs without calling connect-once API");
            Check(host.Outputs.Matches(MultiplayerSync.SessionEpoch,12,2),"Host publishes valid per-group outputs tied to the current arrangement revision");
            host.Definitions[0].Engine.torqueOut.Value=10000;host.Definitions[1].Engine.torqueOut.Value=20000;
            Check(!host.NativeForceAllowed()&&host.Vehicle.Bogies.All(b=>b.appliedForce==500),"Host limits and applies each native group force exactly once through its mapped bogie");
            Role(false);
            Check(!host.NativeForceAllowed(),"Authority loss blocks force application before the owner next updates");
            int writes=NativeSlipRuntimeState.GroupWrites;NativeSlipRuntimeState.Latest.Prepare();
            Check(!host.Owner.IsOwning&&NativeSlipRuntimeState.GroupWrites==writes&&NativeSlipRuntimeState.Disposed==1,"Authority loss disposes host ECS ownership before any further input or force work");
            Check(host.Definitions.Select(d=>d.Engine.crankRpm.port).SequenceEqual(host.OriginalRpm) && host.Definitions.All(d=>d.Engine.crankRpm.references==0),"Authority loss restores each former host native RPM input once");
            Role(true);
            var pending=new Car();pending.Owner.Release();pending.Owner.Release();
            Check(pending.Definitions.Select(d=>d.Engine.crankRpm.port).SequenceEqual(pending.OriginalRpm),"Teardown before first sample leaves already connected stock RPM inputs intact");
            var replaced=new Car();NativeSlipRuntimeState.Latest.Complete();
            var external=new Port("otherOwner",new LocoSim.Definitions.PortDefinition(0,0,"RPM"));
            replaced.Definitions[0].Engine.crankRpm.port=external;replaced.Owner.Release();
            Check(ReferenceEquals(replaced.Definitions[0].Engine.crankRpm.port,external),"Retiring an old RPM lease never overwrites a replacement owner");
            Check(new[]{guest,host,pending,replaced}.SelectMany(c=>c.Definitions).All(d=>d.Engine.crankRpm.connectionErrors==0),"All activation/retirement paths avoid native duplicate-connection errors");
            Directory.CreateDirectory(args[0]);File.WriteAllText(Path.Combine(args[0],"drive-authority.json"),new JObject{
                ["passed"]=true,["count"]=checks.Count,["checks"]=new JArray(checks),
                ["limitations"]="Entire production IndependentDrive, IndependentDriveReplication and GroupForceRouting execute with narrow Unity/native ECS/animation API substitutes. Confirms ownership boundaries and integration calls, not real physics magnitudes, native ECS scheduling, network delivery or native animation evaluation. Separate native World and Unity phase fixtures cover those available native boundaries."
            }.ToString());Console.WriteLine("PASS "+checks.Count+" independent-drive authority checks");return 0;
        }
        catch(Exception error){Console.Error.WriteLine(error);return 1;}
    }
}
