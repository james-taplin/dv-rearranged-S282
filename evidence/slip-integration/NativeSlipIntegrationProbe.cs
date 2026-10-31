// Runs only inside an isolated Unity release player. Native engine torque feeds
// native ECS slip, then production force transmission/routing. A separate finite
// native Boiler + MultiplePortSum + RSE comparison exercises shared steam demand.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DV;
using DV.ECS.Train;
using DV.Simulation.Cars;
using DV.Utils;
using LocoSim.Definitions;
using LocoSim.Implementations;
using Newtonsoft.Json.Linq;
using RearrangedS282;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Object = UnityEngine.Object;

public static class NativeSlipIntegrationProbe
{
    const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    const float Dt = .02f, Radius = .712f;
    static readonly List<string> checks = new List<string>();
    static readonly List<Object> environment = new List<Object>();
    static void Check(bool value, string label) { if (!value) throw new Exception(label); checks.Add(label); }
    static void Near(float a, float b, string label, float tolerance = .01f) => Check(Mathf.Abs(a-b) <= tolerance, label + " (" + a + ", " + b + ")");
    static void Set(object target, string name, object value)
    {
        for (Type t = target.GetType(); t != null; t=t.BaseType)
        { var field=t.GetField(name,Flags); if(field!=null) { field.SetValue(target,value); return; } }
        throw new MissingFieldException(target.GetType().Name,name);
    }
    static Component Singleton(string name)
    {
        var type=typeof(TrainCar).Assembly.GetTypes().Single(t=>t.Name==name);
        var go=new GameObject("[Integrated slip fixture] "+name);go.SetActive(false);environment.Add(go);
        var c=go.AddComponent(type);typeof(SingletonBehaviour<>).MakeGenericType(type).GetField("_instance",Flags).SetValue(null,c);return c;
    }
    static T Definition<T>(string name) where T:SimComponentDefinition
    {
        var go=new GameObject("[Integrated slip definition] "+name);go.SetActive(false);environment.Add(go);
        var d=go.AddComponent<T>();d.ID=name;return d;
    }
    static Port Rpm(string id,float rpm) { var p=new Port(id,new PortDefinition(PortType.READONLY_OUT,PortValueType.RPM,"RPM"));p.Value=rpm;return p; }
    static void Ref(PortReference r,float value) => Set(r,"defaultValue",value);
    static Port Bound(PortReference reference) => (Port)typeof(PortReference).GetField("port",Flags).GetValue(reference);
    static void Lease(PortReference reference,Port port) => Set(reference,"port",port);
    static ReciprocatingSteamEngine Engine(string name,float fraction,float rpm,Boiler boiler=null)
    {
        // Exact recorded stock S282 numeric calibration (the existing native
        // inspection report), with the production auxiliary capacity fraction.
        var d=Definition<ReciprocatingSteamEngineDefinition>(name);
        d.numCylinders=2;d.cylinderBore=.61f*Mathf.Sqrt(fraction);d.pistonStroke=.711f;
        d.minCutoff=.05f;d.maxCutoff=.9f;d.throttleMaxFlow=6f*fraction;d.steamChestVolume=1000f*fraction;
        d.cylinderHeatRate=.01f;d.maxCondensationRate=.015f;d.primingRate=.05f;d.maxWaterExpulsionRate=.015f;
        d.waterDrainPercentagePerSecond=.6f;d.waterDamagePercentagePerChuff=.025f;d.waterDamageRestorePerSecond=.03f;
        d.cylinderCrackDamage=100f;d.passiveDamagePerRev=.01f;d.noOilDamagePerRev=1f;d.ashChuffDamage=3f;
        var e=new ReciprocatingSteamEngine(d);e.SetGameParams(new SimGameParams(false,false,false,1,1));
        Ref(e.throttleControl,1);Ref(e.reverserControl,.6f);Ref(e.cylinderCockControl,0);Ref(e.lubricationNormalized,1);
        e.crankRpm.SetPortReference(Rpm(name,rpm));e.healthStateExtIn.ExternalValueUpdate(1);
        if(boiler==null) { Ref(e.intakePressure,16);Ref(e.intakeTemperature,350);Ref(e.intakeQuality,1); }
        else { e.intakePressure.SetPortReference(boiler.pressureReadOut);e.intakeTemperature.SetPortReference(boiler.temperatureReadOut);e.intakeQuality.SetPortReference(boiler.outletSteamQualityReadOut); }
        float pressure=boiler?.pressureReadOut.Value??16;
        Set(e,"steamChestSteamMass",e.steamChestVolume/SteamTables.SteamSpecificVolume(pressure));Set(e,"temperatureInCylinders",300f);
        e.steamChestPressureReadOut.Value=pressure;e.waterInCylindersNormalizedReadOut.Value=0;
        return e;
    }
    static Boiler FiniteBoiler(string name)
    {
        // Controlled finite vessel, not a claim to match the full native S282
        // firebox calibration. No heat, injection, cooling or safety discharge.
        var d=Definition<BoilerDefinition>(name);d.diameter=1.7f;d.length=6f;d.capacityMultiplier=.8f;d.thermalInsulance=0;
        d.spawnPressure=16f;d.spawnWaterLevel=7000f;d.waterConsumptionMultiplier=1f;
        d.safetyValveOpeningPressure=100;d.safetyValveClosingPressure=99;d.safetyValveSlop=1;d.maxSafetyValveVentRate=0;
        d.crownSheetNormalizedWaterLevel=.1f;d.crownSheetTempSmoothTime=1;d.crownSheetOverheatTemp=1000;d.minimumExplosionPressure=100;
        d.explosionPressureThreshold=AnimationCurve.Linear(0,10,1,10);d.steamOutletNormalizedWaterLevel=.95f;
        var b=new Boiler(d);b.SetGameParams(new SimGameParams(false,false,false,1,1));b.Tick(Dt);return b;
    }
    static MultiplePortSum Intake(string name,ReciprocatingSteamEngine[] engines)
    {
        var d=Definition<MultiplePortSumDefinition>(name);
        d.inputs=engines.Select((e,i)=>new PortReferenceDefinition(PortValueType.MASS_RATE,"ENGINE"+i)).ToArray();
        d.output=new PortDefinition(PortType.READONLY_OUT,PortValueType.MASS_RATE,"OUT");
        var s=new MultiplePortSum(d);for(int i=0;i<engines.Length;i++)s.inputs[i].SetPortReference(engines[i].intakeFlowReadOut);return s;
    }
    static JObject FiniteSteamCase(string name,float[] rpms)
    {
        var boiler=FiniteBoiler(name+"-boiler");
        var engines=rpms.Select((rpm,i)=>Engine(name+"-engine"+i,i==0?1f:.48392153f,rpm,boiler)).ToArray();
        var sum=Intake(name+"-intake",engines);boiler.steamConsumption.SetPortReference(sum.output);
        float massBefore=boiler.waterMassReadOut.Value,pressureBefore=boiler.pressureReadOut.Value;
        double integrated=0,engineIntegrated=0;float expectedMass=massBefore;
        for(int i=0;i<2500;i++)
        {
            // Production graph order: previous engine intake sum -> boiler ->
            // current native engines. The next tick consumes their updated sum.
            sum.Tick(Dt);float consumed=sum.output.Value*Dt;integrated+=consumed;expectedMass-=consumed;boiler.Tick(Dt);
            CheckStep(boiler.waterMassReadOut.Value==expectedMass,name+": native finite vessel mass differs from exact per-tick float consumption ledger");
            foreach(var e in engines)e.Tick(Dt);
            engineIntegrated+=engines.Sum(e=>e.intakeFlowReadOut.Value)*Dt;
        }
        float lost=massBefore-boiler.waterMassReadOut.Value;
        Check(lost>0 && boiler.pressureReadOut.Value<pressureBefore,name+": native shared finite boiler loses mass and pressure");
        Check(boiler.waterMassReadOut.Value==expectedMass,name+": actual boiler mass matches exact per-tick native float ledger");
        double lastUnconsumed=engines.Sum(e=>e.intakeFlowReadOut.Value)*Dt;
        Check(Math.Abs(engineIntegrated-integrated-lastUnconsumed)<.005,name+": intake sum accounts for every engine exactly once with one-tick delay");
        Check(engines.All(e=>ReferenceEquals(Bound(e.intakePressure),boiler.pressureReadOut)),name+": every engine observes the same falling native boiler pressure");
        return new JObject{["rpm"]=new JArray(rpms),["massLostKg"]=lost,["integratedSteamKg"]=integrated,
            ["startPressureBar"]=pressureBefore,["endPressureBar"]=boiler.pressureReadOut.Value,["engineIntakeKg"]=engineIntegrated,
            ["floatSubtractionQuantizationKg"]=lost-integrated,["exactFloatLedgerFinalMassKg"]=expectedMass};
    }
    public static void Run(string output)
    {
        World world=null;
        try
        {
            if(World.DefaultGameObjectInjectionWorld!=null)throw new InvalidOperationException("Only run this probe in its isolated player.");
            Unity.Burst.BurstCompiler.Options.EnableBurstCompilation=false;
            Singleton("AppUtil");Singleton("PausePhysicsHandler");var spawner=Singleton("CarSpawner");Set(spawner,"poolInitialized",true);
            var globals=ScriptableObject.CreateInstance<Globals>();environment.Add(globals);var gp=ScriptableObject.CreateInstance<GameParams>();environment.Add(gp);
            gp.AdhesionInfluencedByWeather=false;gp.WheelslipAllowed=true;gp.WheelSlideAllowed=true;Set(globals,"gameParamsInstance",gp);Set(null,typeof(Globals),"_current",globals);
            world=new World("Native steam-slip force integration");var manager=world.EntityManager;
            var begin=world.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();var end=world.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            var adhesion=world.GetOrCreateSystem<AdhesionControllerSystem>();var slip=world.GetOrCreateSystem<WheelslipControllerSystem>();
            typeof(DVConvertToEntity).GetMethod("OnWorldInitialized",Flags).Invoke(null,new object[]{world});
            var conversion=world.GetOrCreateSystem<DVConvertToEntitySystem>();var simulation=world.GetOrCreateSystem<SimulationSystemGroup>();
            simulation.AddSystemToUpdateList(conversion);simulation.AddSystemToUpdateList(begin);simulation.AddSystemToUpdateList(adhesion);simulation.AddSystemToUpdateList(slip);simulation.AddSystemToUpdateList(end);
            var parent=new GameObject("[Steam slip fixture]");environment.Add(parent);
            var runtime=new NativeSlipRuntimeState(parent.transform,manager,2,1);
            var liveBoiler=FiniteBoiler("feedback-boiler");
            var engines=new[]{Engine("front",1f,90,liveBoiler),Engine("rear",.48392153f,90,liveBoiler)};
            var liveSum=Intake("feedback-intake",engines);liveBoiler.steamConsumption.SetPortReference(liveSum.output);
            float liveMassBefore=liveBoiler.waterMassReadOut.Value,liveExpectedMass=liveMassBefore,livePressureBefore=liveBoiler.pressureReadOut.Value;
            double liveIntegratedSteam=0;
            var originalRpm=engines.Select(e=>Bound(e.crankRpm)).ToArray();var ownedRpm=new[]{Rpm("front-owned",90),Rpm("rear-owned",90)};
            for(int i=0;i<2;i++)Lease(engines[i].crankRpm,ownedRpm[i]);
            var drive=new ProductionDriveBoundary(runtime,engines,ownedRpm,Radius,manager,parent.transform);
            drive.car.massController.WeightPerAxle=50000f;drive.car.Speed=90*Radius*2*Mathf.PI/60;
            var forceObject=new GameObject("[Native DrivingForce data boundary]");forceObject.SetActive(false);environment.Add(forceObject);
            var nativeForce=forceObject.AddComponent<DrivingForce>();
            runtime.PrepareInputs+=drive.TickInputs;
            double elapsed=0;float[] transmitted=new float[2];bool feedback=false,observedMixed=false,observedBothSlip=false;float maxRpmDelta=0;
            Action<int> step=count=>{for(int n=0;n<count;n++){
                if(feedback && runtime.HasCompletedSample)drive.TickRpm(Dt);
                liveSum.Tick(Dt);float consumption=liveSum.output.Value*Dt;liveIntegratedSteam+=consumption;liveExpectedMass-=consumption;liveBoiler.Tick(Dt);
                CheckStep(liveBoiler.waterMassReadOut.Value==liveExpectedMass,"Integrated slip/steam path removed unaccounted boiler mass");
                foreach(var e in engines)e.Tick(Dt);
                elapsed+=Dt;world.SetTime(new TimeData(elapsed,Dt));simulation.Update();
                CheckStep(!drive.Apply(nativeForce),"Production ApplyForce unexpectedly yielded native whole-car force");
                for(int i=0;i<2;i++)transmitted[i]=drive.groups[i].Force;
                if(runtime.GetOutput(0).wheelslip>0 && runtime.GetOutput(1).wheelslip==0)observedMixed=true;
                if(runtime.GetOutput(0).wheelslip>0 && runtime.GetOutput(1).wheelslip>0)observedBothSlip=true;
                if(feedback)maxRpmDelta=Mathf.Max(maxRpmDelta,Mathf.Abs(ownedRpm[0].Value-ownedRpm[1].Value));
                CheckStep(Mathf.Abs(drive.car.Bogies.Sum(b=>b.Applied)-transmitted.Sum())<.1f,"Production ApplyForce lost/duplicated routed force");
                CheckStep(drive.car.Bogies.All(b=>b.Calls<=1),"Production ApplyForce applied a native bogie more than once");
            }};
            step(200);
            Check(runtime.HasCompletedSample && engines.All(e=>e.torqueOut.Value>0),"Real native RSE torque supplies completed native slip samples");
            Check(observedBothSlip,"Both real native engine groups exceed the same initial low-grip budget");
            float rawGrip=runtime.BasePerAxleGrip;
            Near(runtime.GetOutput(0).totalForceLimit+runtime.GetOutput(1).totalForceLimit,rawGrip*4,"Per-group limits conserve raw assigned grip");
            var demands=engines.Select(e=>e.torqueOut.Value/Radius).ToArray();
            // Select a limit between both measured native torque demands; no
            // manufactured force replaces the real engine output.
            drive.car.massController.WeightPerAxle=(demands[0]+demands[1])/(4f*.24f);step(200);
            Check(runtime.GetOutput(0).wheelslip>0 && runtime.GetOutput(1).wheelslip==0,"Measured stronger native front engine slips while native auxiliary grips");
            Check(Mathf.Abs(transmitted[0])<=runtime.GetOutput(0).totalForceLimit+.01f && Mathf.Abs(transmitted[1])<=runtime.GetOutput(1).totalForceLimit+.01f,"Native slip output limits actual production transmitted forces");
            var routed=GroupForceRouting.Distribute(transmitted,new[,]{{2,0},{0,2}});
            Near(routed.Sum(),transmitted.Sum(),"Production bogie routing conserves summed native-derived force");
            var sharedBogie=GroupForceRouting.Distribute(transmitted,new[,]{{1,1},{1,1}});
            Near(sharedBogie[0],transmitted.Sum()/2,"Shared-bogie triplex routing applies each contribution once");
            feedback=true;step(400);
            Check(observedMixed && maxRpmDelta>1,"Production RPM feedback lets real torque-fed groups run at different speeds");
            Check(!drive.Released,"Production PrepareCore/PublishRpm/ApplyForce remain active through integrated native samples");
            drive.car.RearBogie.brakingForce=1000;step(200);
            Near(drive.groups.Sum(g=>g.Brake),drive.car.RearBogie.brakingForce*2,"Production group brake compensation conserves the native car budget");
            for(int i=0;i<2;i++){var o=runtime.GetOutput(i);float d=engines[i].torqueOut.Value/Radius;float expected=Mathf.Clamp(o.wheelslip>0?Mathf.Sign(d)*Mathf.Max(0,Mathf.Abs(d)-1000):d,-o.totalForceLimit,o.totalForceLimit);Near(transmitted[i],expected,"Group "+i+" native-equivalent mixed-slip braking");}
            drive.car.SimController.wheelslipController.SandCoef=2;step(200);Check(runtime.GetOutput(0).wheelslip==0 && runtime.GetOutput(1).wheelslip==0,"Native sanding recovers both real torque-fed groups");
            drive.car.adhesionController.wheelSlide=.5f;step(100);Check(transmitted.All(f=>f==0),"Copied native slide suppresses all production powered force");
            drive.car.adhesionController.wheelSlide=0;drive.car.SimController.wheelslipController.SandCoef=1;drive.car.RearBogie.brakingForce=0;
            drive.car.SimController.wheelslipController.wheelslipToAdhesionDrop=AnimationCurve.Linear(0,1,.25f,.4f);drive.DiagnosticFirstGroupGrip=.5f;
            drive.car.Speed*=-1;foreach(var e in engines)Ref(e.reverserControl,-.6f);step(600);
            Check(engines.All(e=>e.torqueOut.Value<0) && runtime.GetOutput(0).orientedMaxWheelslipRpm==-370,"Actual native reverse engine torque produces signed reverse native slip");
            Check(transmitted.All(f=>f<=0),"Production force/routing preserves reverse sign");
            Check(liveBoiler.waterMassReadOut.Value==liveExpectedMass && liveExpectedMass<liveMassBefore,"Joined torque/slip/RPM/steam loop exactly accounts for native finite boiler depletion");
            Check(liveBoiler.pressureReadOut.Value<livePressureBefore && engines.All(e=>ReferenceEquals(Bound(e.intakePressure),liveBoiler.pressureReadOut)),"Independent production RPM feedback consumes the same falling native boiler pressure");
            for(int i=0;i<2;i++)if(ReferenceEquals(Bound(engines[i].crankRpm),ownedRpm[i]))Lease(engines[i].crankRpm,originalRpm[i]);
            Check(engines.Select(e=>Bound(e.crankRpm)).SequenceEqual(originalRpm),"Scoped RPM reference restoration returns exact prior native inputs");
            var root=(GameObject)typeof(NativeSlipRuntimeState).GetField("root",Flags).GetValue(runtime);runtime.Dispose();Object.DestroyImmediate(root);
            elapsed+=Dt;world.SetTime(new TimeData(elapsed,Dt));simulation.Update();
            Check(!runtime.HasCompletedSample && NativeSlipRuntimeState.States.Count==0,"Native group teardown invalidates samples and releases registrations");
            var normal=FiniteSteamCase("both-rolling",new[]{60f,60f});var slipping=FiniteSteamCase("front-slipping",new[]{300f,60f});
            Check((double)slipping["integratedSteamKg"]>(double)normal["integratedSteamKg"],"Faster slipping native engine increases finite shared boiler steam demand");
            Check((float)slipping["endPressureBar"]<(float)normal["endPressureBar"],"Additional slipping demand depletes the same finite boiler further");
            Directory.CreateDirectory(Path.GetDirectoryName(output));File.WriteAllText(output,new JObject{["passed"]=true,["checks"]=new JArray(checks),["rolling"]=normal,["slipping"]=slipping,
                ["sourceSha256"]=ProductionDriveBoundary.SourceSha256,["maxRpmDifference"]=maxRpmDelta,
                ["feedbackBoiler"]=new JObject{["startMassKg"]=liveMassBefore,["endMassKg"]=liveBoiler.waterMassReadOut.Value,
                    ["integratedSteamKg"]=liveIntegratedSteam,["floatSubtractionQuantizationKg"]=liveMassBefore-liveBoiler.waterMassReadOut.Value-liveIntegratedSteam,
                    ["startPressureBar"]=livePressureBefore,["endPressureBar"]=liveBoiler.pressureReadOut.Value},
                ["limits"]="Real Unity player, native jobs, production helper runtime/schema, verbatim production PrepareCore/PublishRpm/ApplyForce, native RSE/Boiler/MultiplePortSum. Car/bogie/replication boundaries and controlled finite vessel are synthetic; no real TrainCar/Bogie AddForce, rail acceleration, game player-loop scheduling, full S282 boiler/firebox calibration, animation or multiplayer transport. RPM restoration sequence is scoped fixture integration; whole IndependentDrive ownership has its separate production-body fixture."}.ToString());
        }
        catch(Exception ex){Directory.CreateDirectory(Path.GetDirectoryName(output));File.WriteAllText(output,new JObject{["passed"]=false,["checks"]=new JArray(checks),["error"]=ex.ToString()}.ToString());throw;}
        finally{if(world!=null&&world.IsCreated)world.Dispose();foreach(var o in environment)if(o!=null)Object.DestroyImmediate(o);}
    }
    static void Set(object ignored,Type type,string field,object value) => type.GetField(field,Flags).SetValue(null,value);
    static void CheckStep(bool value,string label) { if(!value)throw new Exception(label); }
}
