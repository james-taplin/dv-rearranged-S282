// Integration-boundary substitutes for executing the entire production
// IndependentDrive owner. Native calculations and animation run separately.
using System;
using System.Collections.Generic;
using System.Linq;
namespace UnityEngine {
 public class Object {}
 public class Component:Object {public GameObject gameObject;public Transform transform=>gameObject.transform;}
 public class MonoBehaviour:Component {}
 public sealed class Transform:Component {public Transform parent;public object localToWorldMatrix;public bool IsChildOf(Transform other)=>this==other||parent?.IsChildOf(other)==true;}
 public sealed class GameObject:Object {readonly List<Component> components=new();public Transform transform;public GameObject(){transform=new Transform{gameObject=this};}public T AddComponent<T>()where T:Component,new(){var component=new T{gameObject=this};components.Add(component);return component;}}
 public sealed class Animator:Component {public float phase;public StateInfo GetCurrentAnimatorStateInfo(int layer)=>new StateInfo{normalizedTime=phase};}
 public struct StateInfo {public float normalizedTime;}
 public static class Time {public static float unscaledTime;}
 public static class Mathf {public const float PI=(float)Math.PI;public static float Abs(float v)=>Math.Abs(v);public static float Sign(float v)=>Math.Sign(v);public static float Max(float a,float b)=>Math.Max(a,b);public static float Clamp(float v,float min,float max)=>Math.Min(max,Math.Max(min,v));public static float Lerp(float a,float b,float t)=>a+(b-a)*t;public static float SmoothDamp(float a,float b,ref float speed,float time,float maximum,float delta)=>b;}
 public sealed class Rigidbody {public bool isKinematic;}
}
namespace HarmonyLib {public sealed class Harmony {public void Patch(object target,HarmonyMethod prefix=null){}}public sealed class HarmonyMethod {public HarmonyMethod(Type type,string name){}}public static class AccessTools {public static object Method(Type type,string name)=>null;}}
namespace DV {public static class TimeUtil {public static bool IsFlowing=true;}}
namespace DV.Utils {public class SingletonBehaviour<T>where T:new(){public static T Instance=new T();}}
namespace Unity.Transforms {public struct LocalToWorld {public object Value;}}
public sealed class CarSpawner {public bool PoolSetupInProgress;}
public sealed class PausePhysicsHandler {public bool PhysicsHandlingInProcess;}
public sealed class Curve {public float Evaluate(float input)=>1;}
public sealed class EntityManager {public T GetComponentData<T>(object entity)=>default;}
public sealed class EntityWrapper {public EntityManager EntityManager=new();public object Entity=new();}
public sealed class MassController {public float WeightPerAxle=10000;}
public sealed class AdhesionController {public float wheelSlide;}
public sealed class GroundFriction {public bool IsGrounded=true;}
public sealed class Bogie {public bool enabled=true;public UnityEngine.Rigidbody rb=new();public float brakingForce,appliedForce;public void ApplyForce(float value){appliedForce+=value;}}
public sealed class TrainCar:UnityEngine.MonoBehaviour {public string CarGUID=Guid.NewGuid().ToString(),ID="test";public bool derailed,isExploded,isEligibleForSleep;public DV.Simulation.Cars.SimController SimController=new();public Bogie[] Bogies={new Bogie(),new Bogie()};public Bogie RearBogie=>Bogies[1];public GroundFriction groundFriction=new();public AdhesionController adhesionController=new();public MassController massController=new();public EntityWrapper entity=new();public float GetForwardSpeed()=>0;public float GetAbsSpeed()=>0;public void ForceOptimizationState(bool sleep){}}
namespace DV.Simulation.Cars {
 public sealed class DrivingForce {public float wheelRadius=1,generatedForce;public void FixedUpdate(){}}
 public sealed class WheelslipController {public int NumberOfPoweredAxles=4;public float SandCoef,maxWheelslipRpm=300;public bool preventWheelslip,IsEngineBraking;public Curve wheelslipToAdhesionDrop=new();}
 public sealed class SimController {public DrivingForce drivingForce=new();public WheelslipController wheelslipController=new();}
}
namespace DV.ECS.Train {
 public static class AdhesionControllerSystem {
  public struct StaticSharedAdhesionCalculationData {public int bogieCount;}
  public struct StaticAdhesionCalculationData {}
  public struct AdhesionCalculationData {public float weightPerAxle,absSpeed,sandCoef,brakingForce;public bool isDerailed,isGrounded;}
  public struct WheelSlideData {}
 }
 public static class WheelslipControllerSystem {public struct WheelslipInputData {public float generatedForce,brakingForce,maxWheelslipRpm,wheelslipToAdhesionDrop;public ushort numberOfPoweredAxles;public bool preventWheelslip,isEngineBraking;}}
}
namespace LocoSim.Definitions {
 public enum PortType {READONLY_OUT}public enum PortValueType {STATE,RPM}
 public sealed class PortDefinition {public string ID;public PortDefinition(PortType type,PortValueType value,string id){ID=id;}}
 public abstract class SimComponentDefinition:UnityEngine.Component {public string ID;}
 public sealed class ConfigurablePortDefinition:SimComponentDefinition {public float value;public PortDefinition port;}
}
namespace LocoSim.Implementations {
 public sealed class Port {public string id;public int writes;float value;public float Value{get=>value;set{this.value=value;writes++;}}public Port(string owner,LocoSim.Definitions.PortDefinition definition){id=owner+"."+definition.ID;}}
 public sealed class PortReference {public Port port;public int references;public void SetPortReference(Port value){port=value;references++;}}
 public sealed class ReciprocatingSteamEngine {public string id;public PortReference crankRpm=new();public Port crankRotationReadOut,torqueOut,intakeFlowReadOut;public ReciprocatingSteamEngine(string name){id=name;Port P(string field)=>new Port(id,new LocoSim.Definitions.PortDefinition(0,0,field));crankRotationReadOut=P("PHASE");torqueOut=P("TORQUE");intakeFlowReadOut=P("INTAKE");crankRpm.port=P("RPM");}}
 public sealed class SimulationFlow {public readonly Dictionary<string,Port> ports=new();public SimulationFlow(IEnumerable<LocoSim.Definitions.SimComponentDefinition> definitions){foreach(var d in definitions.Cast<LocoSim.Definitions.ConfigurablePortDefinition>()){var p=new Port(d.ID,d.port);p.Value=d.value;ports.Add(p.id,p);}}public bool TryGetPort(string id,out Port port)=>ports.TryGetValue(id,out port);public void Tick(float delta){}}
}
namespace LocoSim.Implementations.Wheels {public sealed class PoweredWheel {public UnityEngine.Transform wheelTransform;}}
namespace RearrangedS282 {
 internal static class MultiplayerSync {internal static bool IsSimulationAuthority=true,IsClient;internal static long Revision=1,SessionEpoch=100;internal static long RevisionFor(string guid)=>Revision;internal static Action<string> ReportFailure;}
 internal static class Main {internal static string ModPath;internal static Logger Logger=new();}
 internal sealed class Logger {internal void Warning(string value){Console.WriteLine(value);}}
 internal sealed class ArticulatedSimulation {internal LocoSim.Implementations.SimulationFlow Flow;}
 internal static class DriveQuartering {internal static float Cycle(float value)=>value-(float)Math.Floor(value);}
 internal sealed class GroupCrankAnimation:IDisposable {internal static readonly List<float[]> Offsets=new();internal static int Releases;internal static GroupCrankAnimation Bind(LocoSim.Implementations.ReciprocatingSteamEngine engine,UnityEngine.Animator[] animators,float[] normalizedPhases=null,object globalDrivers=null,float[] explicitOffsets=null){Offsets.Add(explicitOffsets?.ToArray());return new GroupCrankAnimation();}public void Dispose(){Releases++;}}
 internal static class GroupSlipEffects {internal static int Releases,Ticks;internal static void Tick(TrainCar car,IndependentDrive owner){Ticks++;}internal static void Release(TrainCar car){Releases++;}}
 internal sealed class NativeSlipRuntimeState:IDisposable {
  internal static int Created,Disposed,GroupWrites;internal static NativeSlipRuntimeState Latest;
  internal struct Output {internal float wheelslip,totalForceLimit,orientedMaxWheelslipRpm;}
  internal event Action PrepareInputs,OutputsReady;internal event Action<Exception> Failed;
  internal bool HasCompletedSample;readonly Output[] outputs;
  internal NativeSlipRuntimeState(UnityEngine.Transform parent,TrainCar car,int count,int generation){Created++;Latest=this;outputs=Enumerable.Repeat(new Output{totalForceLimit=500,orientedMaxWheelslipRpm=300},count).ToArray();}
  internal Output GetOutput(int index)=>outputs[index];
  internal void SetCarrier(object shared,object friction,object calc,object matrix){}
  internal void SetGroup(int index,object input,object shared,object slide,float grip){GroupWrites++;}
  internal void Complete(){HasCompletedSample=true;OutputsReady?.Invoke();}
  internal void Prepare()=>PrepareInputs?.Invoke();
  public void Dispose(){Disposed++;}
 }
}
