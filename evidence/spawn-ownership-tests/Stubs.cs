// Managed train/lifecycle stand-ins. No Unity or native save dispatch is claimed.
using System;
using System.Collections;
using System.Collections.Generic;
namespace UnityEngine {
    public class Object { public static void Destroy(Object value) {} }
    public class GameObject : Object {
        private readonly Dictionary<Type,Component> components = new();
        public T AddComponent<T>() where T:Component,new() { var c=new T { gameObject=this }; components[typeof(T)]=c; return c; }
        public T GetComponent<T>() where T:class => components.TryGetValue(typeof(T),out var c)?c as T:null;
    }
    public class Component : Object { public GameObject gameObject; public Transform transform=new(); public T GetComponent<T>() where T:class => gameObject.GetComponent<T>(); }
    public class Transform { public object Find(string path)=>this; }
    public sealed class Coroutine { public IEnumerator Routine; }
    public class MonoBehaviour : Component {
        public bool isActiveAndEnabled=true;
        public readonly List<Coroutine> Started=new(); public int Stopped;
        public Coroutine StartCoroutine(IEnumerator routine) { var c=new Coroutine{Routine=routine}; Started.Add(c); return c; }
        public void StopCoroutine(Coroutine coroutine) { Stopped++; }
    }
    public sealed class WaitForSecondsRealtime { public WaitForSecondsRealtime(float seconds) {} }
    public static class Mathf { public static float Abs(float value)=>Math.Abs(value); }
}
namespace DV.ThingTypes {
    public enum TrainCarType { Other,LocoSteamHeavy }
    public sealed class TrainCarLivery { public string id; }
    public sealed class DVObjectModel {
        public TrainCarLivery Normal=new(){id="LocoS282A"}; public bool Available=true;
        public bool TryGetLivery(string id,out TrainCarLivery livery) { livery=Available&&id=="LocoS282A"?Normal:null; return livery!=null; }
    }
}
namespace DV { public sealed class Globals { public static Globals G=new(); public DV.ThingTypes.DVObjectModel Types=new(); } }
public sealed class TrainCar : UnityEngine.Component {
    public bool uniqueCar,derailed,isExploded; public object logicCar=new(); public string CarGUID="A",ID="S282";
    public DV.ThingTypes.TrainCarType carType=DV.ThingTypes.TrainCarType.LocoSteamHeavy;
    public DV.ThingTypes.TrainCarLivery carLivery=DV.Globals.G.Types.Normal;
    public Entity entity=new(); public Adhesion adhesionController=new(); public DV.Simulation.Cars.SimController SimController=new();
    public UnityEngine.Component interior;
    public bool AreBogiesFullyInitialized()=>true; public float GetForwardSpeed()=>0; public void PrepareForDestroy() {}
    public void InitializeNewLogicCar() {} public void ReturnCarToPool() {}
}
public sealed class Entity { public bool IsConverted=true; }
public sealed class Adhesion { public bool IsWheelSliding; }
public sealed class SimFlow { public bool TryGetPort(string id,out Port value) { value=new(); return true; } }
public sealed class Port { public float Value; }
public sealed class DrivingForce { public float generatedForce; }
public sealed class Slip { public bool IsWheelslipping; }
namespace DV.Simulation.Cars { public sealed class SimController { public SimFlow simFlow=new(); public DrivingForce drivingForce=new(); public Slip wheelslipController=new(); } }
public sealed class SimCarStateSave : UnityEngine.Component { public void GetStateSaveData() {} }
public static class CarsSaveManager { public static void RestoreCarState() {} }
public sealed class CarSpawner {
    private void BaseSpawn(bool uniqueCar) {} public void SpawnLoadedCar(bool uniqueCar) {} public void SpawnLoadedCarAtNearestAvailableSpace(bool uniqueCar) {}
}
namespace LocoMeshSplitter.MeshLoaders { public sealed class S282AMeshLoader : UnityEngine.Component {} }
namespace HarmonyLib {
    [AttributeUsage(AttributeTargets.Class)] public sealed class HarmonyPatch : Attribute { public HarmonyPatch(Type type,string method) {} }
    [AttributeUsage(AttributeTargets.Method)] public sealed class HarmonyAfter : Attribute { public HarmonyAfter(params string[] owners) {} }
    public sealed class HarmonyMethod { public HarmonyMethod(Type type,string name) {} }
    public static class AccessTools { public static System.Reflection.MethodInfo Method(Type type,string name)=>type.GetMethod(name,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic); }
    public sealed class Harmony {
        public int SpawnTargets;
        public Harmony CreateClassProcessor(Type type)=>this; public void Patch() {}
        public void Patch(System.Reflection.MethodBase method,HarmonyMethod prefix=null,HarmonyMethod finalizer=null) { if(method==null)throw new Exception("Missing spawn target"); SpawnTargets++; }
    }
}
namespace RearrangedS282 {
    internal sealed class Settings { internal bool spawnRandomWA; internal int Choice=34; internal int PickRandomArrangement(Random random)=>Choice; internal bool PoweredTenderEnabled(int id)=>false; }
    internal static class Main { internal static Settings settings=new(); internal static Logger Logger=new(); }
    internal static class MultiplayerSync {
        internal static bool Guest;
        internal static readonly List<TrainCar> Forgotten=new();
        internal static bool DeferAutomatic(TrainCar car)=>Guest;
        internal static void Forget(TrainCar car)=>Forgotten.Add(car);
    }
    internal sealed class Logger { internal void Log(string message) {} internal void Warning(string message) {} internal void Error(string message) {} internal void LogException(string message,Exception error) {} }
    internal sealed class WheelRearranger : UnityEngine.Component {
        internal WheelArrangementType currentWA=WheelArrangementType.s282; internal int Applied,Reset;
        internal bool TrySwitchStage2(int id) { Applied++; currentWA=(WheelArrangementType)id; return true; }
        internal bool ResetConventionalForPool() { Reset++; currentWA=WheelArrangementType.s282; return true; }
    }
}
