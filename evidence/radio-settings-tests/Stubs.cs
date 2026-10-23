// Input/state stand-ins only: no claim to run UMM, Unity input routing or VR.
using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS0649,CS0067
namespace DV { }
namespace DV.Localization { }
namespace DV.Logic.Job { }
namespace DV.ThingTypes.TransitionHelpers { }
namespace VLB { }
namespace DV.ThingTypes { public enum TrainCarType { LocoSteamHeavy, Other } }
namespace HarmonyLib
{
    [AttributeUsage(AttributeTargets.Class)] public sealed class HarmonyPatch:Attribute { public HarmonyPatch(Type t,string name){} }
    public sealed class Harmony { public Harmony(string id){} public Harmony CreateClassProcessor(Type t)=>this;public void Patch(){}public void PatchAll(){}public void UnpatchAll(string id){} }
}
namespace UnityEngine
{
    public class Object
    {
        public bool destroyed; public virtual string name{get;set;}
        static bool Missing(Object o)=>ReferenceEquals(o,null)||o.destroyed;
        public static implicit operator bool(Object o)=>!Missing(o);
        public static bool operator==(Object a,Object b)=>Missing(a)||Missing(b)?Missing(a)&&Missing(b):ReferenceEquals(a,b);
        public static bool operator!=(Object a,Object b)=>!(a==b);
        public override bool Equals(object o)=>ReferenceEquals(this,o);
        public override int GetHashCode()=>System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
        public static void Destroy(Object o){if(!ReferenceEquals(o,null))o.destroyed=true;}
        public static GameObject Instantiate(GameObject source){var go=new GameObject(source.name+" clone");if(source.GetComponent<MeshRenderer>()!=null)go.AddComponent<MeshRenderer>();return go;}
    }
    public class Component:Object
    {
        public GameObject gameObject; public Transform transform=>this as Transform??gameObject.transform;
        public T GetComponent<T>()where T:class=>gameObject.GetComponent<T>();
        public T GetComponentInChildren<T>(bool inactive=false)where T:class=>gameObject.GetComponentInChildren<T>(inactive);
        public override string name{get=>gameObject.name;set=>gameObject.name=value;}
    }
    public class MonoBehaviour:Component { }
    public sealed class GameObject:Object
    {
        public Transform transform;public bool activeSelf=true;internal readonly List<Component> components=new();
        public GameObject(string name){this.name=name;transform=new Transform{gameObject=this};components.Add(transform);}
        public T AddComponent<T>()where T:Component,new(){var value=new T{gameObject=this};components.Add(value);return value;}
        public T GetComponent<T>()where T:class=>components.OfType<T>().FirstOrDefault();
        public T GetComponentInChildren<T>(bool inactive=false)where T:class=>GetComponent<T>()??transform.children.Select(c=>c.gameObject.GetComponentInChildren<T>(inactive)).FirstOrDefault(c=>c!=null);
        public void SetActive(bool value)=>activeSelf=value;
    }
    public sealed class Transform:Component
    {
        public Transform parent;public List<Transform> children=new();public Vector3 localPosition,localScale=Vector3.one;public Quaternion rotation;
        public Vector3 position{get=>(parent!=null?parent.position:Vector3.zero)+localPosition;set=>localPosition=value-(parent!=null?parent.position:Vector3.zero);}
        public Transform root=>parent!=null?parent.root:this;public Vector3 up=>Vector3.up;public Vector3 forward=>Vector3.forward;
        public void SetParent(Transform value,bool worldPositionStays=true){var p=position;parent?.children.Remove(this);parent=value;parent?.children.Add(this);if(worldPositionStays)position=p;}
        public void SetPositionAndRotation(Vector3 p,Quaternion q){position=p;rotation=q;}
    }
    public struct Vector3
    {
        public float x,y,z;public Vector3(float x,float y,float z){this.x=x;this.y=y;this.z=z;}
        public static Vector3 zero=>new();public static Vector3 one=>new(1,1,1);public static Vector3 up=>new(0,1,0);public static Vector3 forward=>new(0,0,1);
        public float magnitude=>(float)Math.Sqrt(x*x+y*y+z*z);
        public static Vector3 operator+(Vector3 a,Vector3 b)=>new(a.x+b.x,a.y+b.y,a.z+b.z);public static Vector3 operator-(Vector3 a,Vector3 b)=>new(a.x-b.x,a.y-b.y,a.z-b.z);
        public static Vector3 operator*(Vector3 a,float b)=>new(a.x*b,a.y*b,a.z*b);
    }
    public struct Quaternion { }
    public struct Color { public Color(float r,float g,float b,float a=1){} }
    public struct Bounds { public Vector3 center,size; }
    public sealed class Material:Object {public Material(){}public Material(Material source){} }
    public sealed class MeshRenderer:Component {public Material material;}
    public sealed class AudioClip:Object { }
    public struct LayerMask {public static LayerMask GetMask(string[] names)=>new();}
    public struct RaycastHit {public Transform transform;}
    public static class Physics
    {
        public static Transform Hit;public static int Calls;
        public static bool Raycast(Vector3 a,Vector3 b,out RaycastHit hit,float range,LayerMask mask){Calls++;hit=new RaycastHit{transform=Hit};return Hit!=null;}
    }
    public static class Debug {public static void LogError(string s){} }
    public static class Application {public static event Action quitting;}
    public static class GUI {public static bool enabled=true;}
    public static class GUILayout
    {
        public static readonly List<string> Labels=new(),Buttons=new();public static readonly HashSet<string> Clicks=new();
        public static bool ToggleValue;
        public static void Label(string value)=>Labels.Add(value);public static void Space(float value){}
        public static bool Button(string value){Buttons.Add(value);return Clicks.Remove(value);}
        public static bool Toggle(bool value,string label)=>GUI.enabled?ToggleValue:value;
        public static void Clear(){Labels.Clear();Buttons.Clear();Clicks.Clear();ToggleValue=false;}
    }
}
namespace UnityModManagerNet
{
    public static class UnityModManager
    {
        public sealed class UI {public static UI Instance=new();public bool Opened;}
        public sealed class ModEntry
        {
            public sealed class InfoData{public string Id="fixture",DisplayName="fixture";}
            public sealed class ModLogger{public readonly List<string> Rows=new();public void Log(string s)=>Rows.Add(s);public void Warning(string s)=>Rows.Add(s);public void Error(string s)=>Rows.Add(s);public void LogException(string s,Exception e)=>Rows.Add(s+e.Message);}
            public InfoData Info=new();public ModLogger Logger=new();public string Path=".";
            public Action<ModEntry> OnGUI,OnSaveGUI;public Action<ModEntry,float> OnUpdate;public Func<ModEntry,bool> OnUnload;
        }
    }
}
public enum ButtonBehaviourType {Regular,Override}
public interface ICommsRadioMode { }
public sealed class CommsRadioDisplay:UnityEngine.Component
{
    public string Title,Content,Action;public int Updates;
    public void SetDisplay(string t,string c,string a){Title=t;Content=c;Action=a;Updates++;}
    public void SetContent(string c){Content=c;Updates++;}public void SetAction(string a){Action=a;Updates++;}
}
public sealed class CommsRadioCarDeleter:UnityEngine.Component
{
    public UnityEngine.Transform signalOrigin;public CommsRadioDisplay display;public UnityEngine.Material selectionMaterial,deleteMaterial;
    public UnityEngine.GameObject trainHighlighter;public UnityEngine.AudioClip hoverOverCar,warningSound,confirmSound,cancelSound;
}
public sealed class CommsRadioController:UnityEngine.Component
{
    public CommsRadioCarDeleter deleteControl;public static int Sounds;
    public static void PlayAudioFromRadio(UnityEngine.AudioClip sound,UnityEngine.Transform origin)=>Sounds++;
}
public sealed class TrainCar:UnityEngine.Component
{
    public string ID;public DV.ThingTypes.TrainCarType carType=DV.ThingTypes.TrainCarType.LocoSteamHeavy;
    public UnityEngine.Bounds Bounds=new(){center=new(0,0,5),size=new(3,4,18)};
    public static TrainCar Resolve(UnityEngine.Transform t)=>t?.GetComponent<TrainCar>();
}
public sealed class ExplosionModelHandler:UnityEngine.Component{public bool usingExplodedModel;}
namespace DV.Utils
{
    public static class Extension{public static T GetOrAddComponent<T>(this UnityEngine.Component c)where T:UnityEngine.Component,new()=>c.GetComponent<T>()??c.gameObject.AddComponent<T>();}
}
namespace RearrangedS282
{
    internal static class MultiplayerSync
    {
        internal static bool IsClient,InSession,AllowClientRequest;
        internal static int Requests,Updates;
        internal static string Status="fixture multiplayer";
        internal static void Initialize(){}internal static void Tick()=>Updates++;internal static void Draw(){}
        internal static bool RequestChange(TrainCar car,int id,out string reason)
        {
            Requests++;reason=null;
            if(IsClient){reason=AllowClientRequest?"Requested from host":"Host locked";return AllowClientRequest;}
            var wheel=car.GetComponent<WheelRearranger>()??car.gameObject.AddComponent<WheelRearranger>();
            bool applied=wheel.TrySwitchStage2(id);if(applied)ConventionalSpawn.RecordApplied(car,id);else reason=wheel.LastConventionalError??"stop, repair";return applied;
        }
    }
    public sealed class Settings
    {
        internal readonly List<bool> DrawEnabled=new();
        public int[] poweredTenderArrangements=new[]{43}; internal bool PoweredTenderEnabled(int id)=>poweredTenderArrangements.Contains(id);
        public static T Load<T>(UnityModManagerNet.UnityModManager.ModEntry e)where T:new()=>new();
        public bool MigrateSpawnPool()=>false;public void Save(UnityModManagerNet.UnityModManager.ModEntry e){}public void OnChange(){}public void Draw(UnityModManagerNet.UnityModManager.ModEntry e)=>DrawEnabled.Add(UnityEngine.GUI.enabled);
    }
    internal sealed class WheelRearranger:UnityEngine.MonoBehaviour
    {
        internal WheelArrangementType currentWA=WheelArrangementType.s282;internal bool AllowApply=true;internal string LastConventionalError;internal readonly List<int> Calls=new();
        internal bool TrySwitchStage2(int id){Calls.Add(id);if(AllowApply)currentWA=(WheelArrangementType)id;return AllowApply;}
        internal sealed class CurveSquealPatch{ }
    }
    internal static class ConventionalSpawn
    {
        internal static readonly List<(TrainCar car,int id)> Records=new();
        internal static void RecordApplied(TrainCar car,int id)=>Records.Add((car,id));internal static void RegisterPatches(HarmonyLib.Harmony h){}
    }
    internal static class Stage2RuntimeChecks{internal static bool Running;internal static string Status="fixture";internal static int Starts;internal static void Begin(){Starts++;Running=true;}}
    internal static class PowerDiagnostics
    {
        internal static bool Recording;internal static string Status="fixture",Latest,LastPath;internal static int Starts,Stops;
        internal static Func<TrainCar,object> CurveSampler;internal static Func<object> CurveSessionInfo;
        internal static void Update(){}internal static void Stop(string why=null){Stops++;Recording=false;}internal static void Begin(){Starts++;Recording=true;}
    }
    internal sealed class PowerDiagnosticsTick{ }
    internal static class ArticulatedSimulation{internal static void Register(HarmonyLib.Harmony h){} }
    internal static class BigEndOiling{internal static void Register(HarmonyLib.Harmony h){} }
    internal static class ArticulatedEffects{internal static void Register(HarmonyLib.Harmony h){} }
    internal static class DeltaTrailingTruckCompatibility{internal static string Status;internal static void Register(HarmonyLib.Harmony h){} }
    internal static class CurveDiagnostics{internal static object Sample(TrainCar c)=>null;internal static object SessionInfo()=>null;}
}
