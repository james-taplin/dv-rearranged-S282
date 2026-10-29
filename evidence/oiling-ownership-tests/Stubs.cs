// Only train identity and Harmony registration are stand-ins. Oil definitions,
// implementations, ports, Tick and save methods come from native DV.Simulation.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LocoSim.Definitions;
namespace DV.ThingTypes {
    public enum TrainCarType { Other, LocoSteamHeavy }
    public sealed class TrainCarLivery { }
    public sealed class DVObjectModel {
        public readonly TrainCarLivery Normal = new TrainCarLivery();
        public bool TryGetLivery(string id,out TrainCarLivery livery) { livery=id=="LocoS282A"?Normal:null; return livery!=null; }
    }
}
namespace DV { public sealed class Globals { public static Globals G=new Globals(); public DV.ThingTypes.DVObjectModel Types=new DV.ThingTypes.DVObjectModel(); } }
public sealed class TestTransform { public bool HasBody; public object Find(string name) => HasBody ? this : null; }
public sealed class TrainCar {
    public DV.ThingTypes.TrainCarType carType;
    public bool uniqueCar;
    public DV.ThingTypes.TrainCarLivery carLivery;
    public TestTransform transform = new TestTransform();
    public ManualOilingPointsDefinition Definition;
    public T[] GetComponentsInChildren<T>(bool inactive) => Definition is T value ? new[] { value } : Array.Empty<T>();
}
namespace DV.Simulation.Cars { public sealed class SimController { public void Initialize(TrainCar trainCar) {} } }
namespace HarmonyLib {
    public sealed class HarmonyMethod { public readonly string Name; public HarmonyMethod(Type type,string name) { Name=name; } }
    public static class AccessTools { public static MethodInfo Method(Type type,string name) => type.GetMethod(name, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static); }
    public sealed class Harmony {
        public readonly List<string> Targets = new List<string>();
        public void Patch(MethodBase target,HarmonyMethod prefix=null,HarmonyMethod postfix=null) {
            if(target == null) throw new Exception("Missing native patch target");
            Targets.Add(target.DeclaringType.FullName+"::"+target.Name+":"+(prefix?.Name??postfix?.Name));
        }
    }
}
namespace RearrangedS282 {
    internal static class MultiplayerSync { internal static bool IsSimulationAuthority = true; }
    internal static class Main { internal static readonly Log Logger = new Log(); internal static readonly TestSettings settings = new TestSettings(); }
    internal sealed class TestSettings { internal bool automaticCupOiling = false; }
    internal sealed class Log { internal readonly List<string> Warnings=new List<string>(); internal void Warning(string text) { Warnings.Add(text); } }
}
