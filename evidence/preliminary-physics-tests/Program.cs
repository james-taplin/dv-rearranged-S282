using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using LocoSim.Implementations;
using RearrangedS282;
namespace RearrangedS282 {
    internal static class ArticulatedSimulation {internal const string MainEngineId="steamEngine";}
    // This fixture exercises native calibration as host/offline; the separate
    // ArticulatedSimulation fixture exercises both authority states of the gate.
    internal static class MultiplayerSync { internal static bool IsSimulationAuthority = true; }
}
internal static partial class Program
{
    static double Force(ReciprocatingSteamEngine e,double radius)=>TractiveEffortModel.MeanForceN(e.numCylinders,PrivateFloat(e,"pistonArea"),e.pistonStroke,radius,15,.75,300).Value;
    static (float,float,float,float,int) State(ReciprocatingSteamEngine e)=>(e.cylinderBore,e.pistonStroke,PrivateFloat(e,"pistonArea"),PrivateFloat(e,"cylinderVolume"),e.numCylinders);
    static void Main(string[] args)
    {
        var definition=JObject.Parse(File.ReadAllText(args[0]));
        var capture=JObject.Parse(File.ReadAllText(args[1]));
        var capturedIds=((JArray)capture["arrangements"]).Select(r=>(int)r["id"]).Distinct().OrderBy(id=>id).ToArray();
        Check((int)capture["supportedArrangements"]==49 && capturedIds.SequenceEqual(Enumerable.Range(0,49)),
            "Historical capture must contain exactly its original IDs 0 through 48");
        Check(((JArray)capture["arrangements"]).All(r=>(bool?)r["passed"]==true),
            "Captured arrangement contains a failed or incomplete case");
        Check(WheelArrangement.SupportedCount==51,"Expected the two explicitly appended arrangements");
        var inputs=new JArray(((JArray)capture["arrangements"]).Select(r=>r.DeepClone()));
        foreach(var (id,path) in new[]{(49,args[3]),(50,args[4])}) {
            var geometry=JObject.Parse(File.ReadAllText(path));
            Check((bool?)geometry["passed"]==true&&(int?)geometry["arrangementId"]==id,"Expected a passing ID-specific offline geometry receipt "+id);
            float radius=(float?)geometry["driverRadiusM"]??float.NaN;
            Check(float.IsFinite(radius)&&radius>0,"Offline geometry receipt lacks a finite positive driver radius "+id);
            inputs.Add(new JObject{["id"]=id,["name"]=WheelArrangement.WheelArrangementNames[id],["measurements"]=new JObject{["driverRadiusM"]=radius},["radiusInputSource"]="offline geometry fixture; not game capture"});
        }
        foreach(var expected in new[]{(id:49,total:6,powered:3),(id:50,total:10,powered:6)}) {
            var w=WheelArrangement.LocomotiveWheels(expected.id);
            Check(w.Sum()/2==expected.total&&w.Skip(1).Take(w.Length-2).Sum()/2==expected.powered,"Physical/powered axle counts must match requested wheel groups "+expected.id);
        }
        var original=Definition(definition);original.ID="steamEngine";
        float ratio=2f*.55f*.55f*.97f*.97f*.97f*.711f/(original.cylinderBore*original.cylinderBore*original.pistonStroke)-1f;
        var rd=Definition(definition,ratio);rd.ID="rear";var md=Definition(definition,ratio);md.ID="middle";
        var main=new ReciprocatingSteamEngine(original);var rear=new ReciprocatingSteamEngine(rd);var middle=new ReciprocatingSteamEngine(md);
        var engines=new[]{main,rear,middle};var snapshots=engines.Select(State).ToArray();
        var other=new ReciprocatingSteamEngine(original);var otherSnapshot=State(other);
        var calibration=new CylinderCalibration(engines,.712f,.24f,4,6);
        var table=new JArray();int layouts=0,ticks=0;
        var designStates=new System.Collections.Generic.Dictionary<int,(float,float,float,float,int)[]>();
        foreach(float mass in new[]{111000f,124079.203125f,135000f})
        foreach(var record in inputs)
        {
            int id=(int)record["id"];float radius=(float)(record["measurements"]?["driverRadiusM"] ?? record["converted"]?["forceRadius"] ?? throw new Exception("Capture lacks measured driver radius"));
            var wheels=WheelArrangement.LocomotiveWheels(id);bool articulated=WheelArrangement.IsArticulatedId(id);
            int powered=wheels.Skip(1).Take(wheels.Length-2).Sum()/2;
            float share=ConventionalPhysics.TargetDriverWeightFraction(wheels,ConventionalPhysics.ReferenceOperatingMassKg,articulated);
            Check(share>=0&&share<=1,"Family share bounded");
            var groups=wheels.Skip(1).Take(wheels.Length-2).Select(x=>x/2).ToArray();
            double cap=groups.Where(g=>g>0).Sum(g=>g*Math.Min(32000,ConventionalPhysics.MaxDrivenAxleLoadKg(g)));
            Check(share*ConventionalPhysics.ReferenceOperatingMassKg<=cap+.02,"Rated effective load obeys driven-axle caps");
            calibration.Restore();
            if(id==10||id==36||powered==0){Check(engines.Select(State).SequenceEqual(snapshots),"Stock/prototype/novelty dimensions untouched");continue;}
            var selected=articulated?(id==38||id==40?engines:new[]{main,rear}):new[]{main};
            calibration.Apply(selected,radius,mass,share,powered);layouts++;
            if(id==43)calibration.ApplyTender(middle,radius);
            var state=engines.Select(State).ToArray();
            if(designStates.ContainsKey(id))Check(state.SequenceEqual(designStates[id]),"Resource state changed arrangement cylinder dimensions");
            else designStates[id]=state;
            Check(calibration.Consistent,"Native cylinder cached dimensions inconsistent");
            double actual=selected.Sum(e=>Force(e,radius));
            Check(Math.Abs(actual-calibration.AfterForceN)<.1,"Actual cached area does not produce intended reference pull");
            Check(actual/calibration.ReferenceGripN<=calibration.TargetRatio*1.00001,"Pull/grip exceeds rated design ceiling");
            double nominal=selected.Sum(e=>e.numCylinders*Math.Pow(e.cylinderBore,2)*e.pistonStroke*
                (.85*14e5)/(4*radius));
            double adhesiveWeight=share*ConventionalPhysics.ReferenceOperatingMassKg*9.81;
            Check(adhesiveWeight/nominal>=calibration.MinimumFactorOfAdhesion-1e-5,"Independent nominal cylinder formula violates machinery-specific adhesion ceiling");
            Check(selected.All(e=>e.cylinderBore<=snapshots[Array.IndexOf(engines,e)].Item1+.000001f),"Boosted cylinder capacity");
            if(selected.Length>1)Check(Math.Abs(PrivateFloat(rear,"pistonArea")/PrivateFloat(main,"pistonArea")-ratio)<.000001,"Lost auxiliary/main capacity ratio");
            Check(State(other)==otherSnapshot,"Mutated a second locomotive");
            if(id==43){
                double tenderNominal=2*Math.Pow(middle.cylinderBore,2)*middle.pistonStroke*(.85*14e5)/(4*radius);
                Check(50000*.8*9.81/tenderNominal>=calibration.TenderMinimumFactorOfAdhesion-1e-5,"Erie tender nominal pull exceeds its independent adhesive weight target");
                Check(calibration.TenderAreaScale<=1&&calibration.TenderAreaScale>0,"Invalid tender cylinder scale");
            }
            if(mass==124079.203125f){
                table.Add(new JObject{["id"]=id,["arrangement"]=(string)record["name"],["diameterM"]=2*radius,["driverShare"]=share,["ratedAdhesiveMassKg"]=share*ConventionalPhysics.ReferenceOperatingMassKg,["mainBoreM"]=main.cylinderBore,["areaScale"]=calibration.AreaScale,["referencePullKN"]=actual/1000,["nominalPullKN"]=nominal/1000,["factorOfAdhesion"]=adhesiveWeight/nominal,["referenceGripKN"]=calibration.ReferenceGripN/1000,["pullGripRatio"]=actual/calibration.ReferenceGripN});
                table.Last["radiusInputSource"]=(string)record["radiusInputSource"]??"historical native game capture";
                table.Last["minimumDesignFactorOfAdhesion"]=calibration.MinimumFactorOfAdhesion;
                if(id==43){
                    table.Last["tenderNominalPullKN"]=Force(middle,radius)/TractiveEffortModel.ReferenceToNominalForceRatio/1000;
                    table.Last["tenderAreaScale"]=calibration.TenderAreaScale;
                    table.Last["tenderMinimumDesignFactorOfAdhesion"]=calibration.TenderMinimumFactorOfAdhesion;
                }
                if(new[]{0,10,39,41,44,46,49,50}.Contains(id))foreach(var e in selected){
                    Setup(e,180,true);for(int i=0;i<200;i++){e.Tick(.02f);ticks++;Check(!float.IsNaN(e.torqueOut.Value)&&!float.IsInfinity(e.torqueOut.Value),"Native Tick torque invalid");Check(e.intakeFlowReadOut.Value>=0&&float.IsFinite(e.intakeFlowReadOut.Value),"Native steam demand invalid");}
                }
            }
            calibration.Restore();Check(engines.Select(State).SequenceEqual(snapshots),"Stock bore/stroke/cached area/volume/count not restored exactly");
        }
        try{calibration.Apply(new[]{main,main},.7f,124000,.7f,4);throw new Exception("Accepted duplicate engine");}catch(ArgumentException){}
        Check(engines.Select(State).SequenceEqual(snapshots),"Failed calibration did not restore stock");
        double stockNominal=Force(main,.712)/TractiveEffortModel.ReferenceToNominalForceRatio;
        double stockFoA=ConventionalPhysics.ReferenceOperatingMassKg*(4d/6d)*9.81/stockNominal;
        Check(table.Any(r=>(double)r["factorOfAdhesion"]<stockFoA),"All calibrated arrangements remain more conservative than stock");
        Check(table.Any(r=>(double)r["factorOfAdhesion"]>stockFoA),"No conservative/underpowered machinery character preserved");
        var result=new JObject{["passed"]=true,["checks"]=Checks.Count,["layoutMassCases"]=layouts,["nativeTicks"]=ticks,["table"]=table,["limitations"]="Real native engine construction/cache updates/restoration/Tick; IDs 0-48 use historical game-captured radii, IDs 49-50 use separate offline geometry fixture radii. No ECS adhesion or Unity driving/loaded grade test. Predicted reference forces are not measured drawbar performance."};
        File.WriteAllText(args[2],result.ToString());Console.WriteLine(new JObject{["passed"]=true,["checks"]=Checks.Count,["layoutMassCases"]=layouts,["nativeTicks"]=ticks}.ToString());
    }
}

