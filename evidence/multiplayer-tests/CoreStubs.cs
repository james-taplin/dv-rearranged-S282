// Only Unity/game/UMM boundaries are replaced. MultiplayerSync and Settings are actual sources.
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field,AllowMultiple=true)]public sealed class HeaderAttribute:Attribute{public HeaderAttribute(string value){}}
    [AttributeUsage(AttributeTargets.Field)]public sealed class SpaceAttribute:Attribute{public SpaceAttribute(int value){}}
    public class Component {public GameObject gameObject;public Transform transform=>gameObject.transform;public T GetComponent<T>()where T:class=>gameObject.GetComponent<T>();}
    public class MonoBehaviour:Component { }
    public sealed class GameObject
    {
        public readonly Transform transform=new();
        readonly List<Component> components=new();
        public T AddComponent<T>()where T:Component,new(){var value=new T{gameObject=this};components.Add(value);return value;}
        public T GetComponent<T>()where T:class=>components.OfType<T>().FirstOrDefault();
    }
    public sealed class Transform{public bool MeshReady;public Transform Find(string path)=>MeshReady?this:null;}
    public static class Time {public static float unscaledTime;}
    public static class GUILayout {public static readonly List<string> Labels=new();public static void Label(string value)=>Labels.Add(value);}
}
namespace UnityModManagerNet
{
    public enum DrawType{Auto,Toggle,ToggleGroup,Slider}
    [AttributeUsage(AttributeTargets.Field)]public sealed class DrawAttribute:Attribute{public int Height,Precision;public double Min,Max;public DrawAttribute(string value,DrawType type=DrawType.Auto){}}
    public interface IDrawable{}
    public static class UnityModManager
    {
        public class ModEntry{public bool Active;}
        public static ModEntry CurrentMod;
        public static ModEntry FindMod(string id)=>CurrentMod;
        public class ModSettings{public virtual void Save(ModEntry entry){}public static void Save<T>(T value,ModEntry entry){}}
    }
}
namespace DV.ThingTypes{public enum TrainCarType{LocoSteamHeavy,Other}}
namespace DV.Utils{}
namespace LocoMeshSplitter.MeshLoaders{public sealed class S282AMeshLoader:UnityEngine.Component{}}
public sealed class TrainCar:UnityEngine.Component
{
    public string ID,CarGUID;
    public DV.ThingTypes.TrainCarType carType=DV.ThingTypes.TrainCarType.LocoSteamHeavy;
    public object logicCar=new(),adhesionController=new();
    public Entity entity=new();
    public Controller SimController=new();
    public bool BogiesReady=true,Moving,isExploded;
    public bool NormalLivery=true,uniqueCar;
    public bool AreBogiesFullyInitialized()=>BogiesReady;
    public sealed class Entity{public bool IsConverted=true;}
    public sealed class Controller{public Flow simFlow=new();public PoweredWheels poweredWheels=new();public object drivingForce=new(),wheelslipController=new(),tractionPortsFeeder=new();}
    public sealed class Flow{public bool PortReady=true;public bool TryGetPort(string name,out object port){port=PortReady?new object():null;return PortReady;}}
    public sealed class PoweredWheels{public object poweredWheels=new();}
}
public sealed class ExplosionModelHandler:UnityEngine.Component { public bool usingExplodedModel; }
public sealed class CarSpawner
{
    private static CarSpawner current=new();
    public static int InstanceReads;
    public static CarSpawner Instance{get{InstanceReads++;return current;}set{current=value;}}
    public readonly List<TrainCar> AllCars=new();
    public event Action<TrainCar> CarAboutToBeDeleted;
    // Native deletion removes AllCars and emits this event before either pool
    // return or PrepareForDestroy/Object.Destroy, including world unload.
    public void Delete(TrainCar car){AllCars.Remove(car);CarAboutToBeDeleted?.Invoke(car);}
}
namespace RearrangedS282
{
    internal static class S282Target {internal static bool IsNormal(TrainCar car)=>car!=null&&car.NormalLivery&&!car.uniqueCar;}
    internal static class Main
    {
        internal static Settings settings=new();
        internal static string ModPath=System.IO.Path.GetTempPath(),RunningVersion="test-core";
        internal static readonly ModLogger Logger=new();
        internal sealed class ModLogger{internal readonly List<string> Rows=new();internal void LogException(string value,Exception error)=>Rows.Add(value+error);internal void Warning(string value)=>Rows.Add(value);internal void Error(string value)=>Rows.Add(value);}
    }
    internal sealed class WheelRearranger:UnityEngine.MonoBehaviour
    {
        internal WheelArrangementType currentWA=WheelArrangementType.s282;
        internal int Attempts;
        internal bool AllowApply=true,SawRemote,SawBeforeSwitch,LastSwitchRestoredStock;
        internal bool PoweredTenderSelected;
        internal Settings UsedSettings;
        internal string LastConventionalError;
        internal bool TrySwitchStage2(int id)
        {
            Attempts++;
            var car=GetComponent<TrainCar>();
            SawRemote=MultiplayerSync.ApplyingRemote;
            SawBeforeSwitch=MultiplayerSync.BeforeSwitch(car,id);
            UsedSettings=MultiplayerSync.SettingsFor(car);
            if(!SawBeforeSwitch||!AllowApply||(car.Moving&&!SawRemote)||car.isExploded||car.GetComponent<ExplosionModelHandler>()?.usingExplodedModel==true)
            {
                LastConventionalError="fixture not safe";
                MultiplayerSync.OnSwitchFailed(car);return false;
            }
            currentWA=(WheelArrangementType)id;
            PoweredTenderSelected=UsedSettings.PoweredTenderEnabled(id);
            MultiplayerSync.OnApplied(car,id);return true;
        }
    }
    internal static class ConventionalSpawn
    {
        internal static readonly List<(TrainCar car,int id)> Records=new();
        internal static void RecordApplied(TrainCar car,int id)=>Records.Add((car,id));
    }
    internal static class Stage2RuntimeChecks{internal static bool Running;}
}
