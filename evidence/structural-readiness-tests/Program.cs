using System;using System.Collections;using System.IO;using System.Linq;using Newtonsoft.Json.Linq;using RearrangedS282;
namespace UnityEngine {
 public class Object { public static T[] FindObjectsOfType<T>()=>TrainCar.World.Cast<T>().ToArray(); }
 public static class Mathf {public static float Abs(float value)=>Math.Abs(value);}
 public sealed class GameObject { public T AddComponent<T>() where T:new()=>new T(); }
}
internal enum TrainCarType { Loco,Tender }
internal sealed class Entity {internal bool IsConverted=true;}
internal sealed class Coupler {internal Coupler coupledTo;internal TrainCar train;}
internal sealed class TrainCar {
 internal static TrainCar[] World=Array.Empty<TrainCar>();internal string ID;internal bool derailed,isExploded,normal=true;internal float speed;
 internal Entity entity=new Entity();internal object loadedExternalInteractables;internal Coupler rearCoupler;
 internal TrainCarType carType;internal bool carLivery=true;internal UnityEngine.GameObject gameObject=new UnityEngine.GameObject();
 internal WheelRearranger owner=new WheelRearranger();internal float GetForwardSpeed()=>speed;internal T GetComponent<T>()where T:class=>owner as T;
}
namespace RearrangedS282 {
 internal enum WheelArrangementType {s282=10}
 internal sealed class WheelRearranger {internal WheelArrangementType currentWA=WheelArrangementType.s282;internal bool PoweredTenderSelected;internal void StartCoroutine(IEnumerator run){Program.Starts++;run.MoveNext();}}
 internal static class S282Target {internal static bool IsNormal(TrainCar car)=>car.normal;}
 internal static class CarTypes {internal static bool IsTender(bool livery)=>livery;}
 internal static class Main {internal static string ModPath;internal static LoggerType Logger=new LoggerType();internal sealed class LoggerType{internal void Warning(string text){}}}
 internal static partial class Stage2RuntimeChecks {
  internal static bool Running;internal static string Status;
  private static IEnumerator RunAll(WheelRearranger runner,TrainCar subject,TrainCar control){Program.Subject=subject;Program.Control=control;yield break;}
 }
}
internal static class Program {
 internal static int Starts;internal static TrainCar Subject,Control;static int checks;
 static void Check(bool ok,string label){checks++;if(!ok)throw new Exception(label);}
 static TrainCar Car(string id,bool loaded,bool tender=false){var c=new TrainCar{ID=id,loadedExternalInteractables=loaded?new object():null};if(tender)c.rearCoupler=new Coupler{coupledTo=new Coupler{train=new TrainCar{ID="tender",carType=TrainCarType.Tender}}};return c;}
 static void Begin(params TrainCar[] cars){TrainCar.World=cars;Stage2RuntimeChecks.Running=false;Starts=0;Subject=Control=null;Stage2RuntimeChecks.Begin();}
 static void Rejected(string label){Check(Starts==0&&!Stage2RuntimeChecks.Running,label);var r=JObject.Parse(File.ReadAllText(Path.Combine(RearrangedS282.Main.ModPath,"stage2-runtime-checks.json")));Check((bool?)r["started"]==false&&(bool?)r["passed"]==false,"Preflight refusal has a truthful report");}
 static void Main(string[] args){
  RearrangedS282.Main.ModPath=args[0];
  var unloaded=Car("L-033",false,true);var loaded=Car("L-093",true,true);var control=Car("L-100",true);
  Begin(control,unloaded,loaded);Check(Starts==1&&ReferenceEquals(Subject,loaded)&&ReferenceEquals(Control,control),"Lower-ID unloaded car cannot take the subject slot");
  Check(Stage2RuntimeChecks.Status.Contains("L-093")&&Stage2RuntimeChecks.Status.Contains("L-100"),"Running status identifies both cars");
  Stage2RuntimeChecks.Begin();Check(Starts==1,"A second run cannot start concurrently");
  Begin(unloaded,Car("L-200",false));Rejected("Unloaded cars rejected before coroutine starts");
  Begin(unloaded,control);Rejected("Loaded control cannot conceal unavailable tender-equipped subject");
  Begin(loaded,Car("L-001",false));Rejected("Unloaded control rejected");
  Begin(Car("L-001",true),control);Rejected("Missing regular tender rejected");
  var powered=Car("L-090",true);powered.owner.PoweredTenderSelected=true;
  Begin(powered,loaded,control);Check(ReferenceEquals(Control,control),"Powered tender cannot become the isolation control");
  var moving=Car("L-001",true,true);moving.speed=.1f;Begin(moving,loaded,control);Check(ReferenceEquals(Subject,loaded),"Moving subject remains excluded");
  var wreck=Car("L-001",true,true);wreck.isExploded=true;Begin(wreck,loaded,control);Check(ReferenceEquals(Subject,loaded),"Exploded subject remains excluded");
  var report=new JObject{["passed"]=false,["arrangements"]=new JArray(new JObject{["passed"]=true})};
  Check(!Stage2RuntimeChecks.AbortForReadiness(report,loaded,control)&&report["aborted"]==null,"Ready cars leave progress intact");
  loaded.loadedExternalInteractables=null;
  Check(Stage2RuntimeChecks.AbortForReadiness(report,loaded,control),"Subject unloading interrupts the run");
  Check((bool?)report["aborted"]==true&&(string)report["abortReason"]=="exterior-interactions-unloaded"&&((string)report["error"]).Contains("L-093"),"Abort reports the exact missing prerequisite and subject");
  Check(((JArray)report["arrangements"]).Count==1,"Abort preserves completed evidence");
  loaded.loadedExternalInteractables=new object();control.loadedExternalInteractables=null;
  Check(Stage2RuntimeChecks.AbortForReadiness(report,loaded,control)&&((string)report["error"]).Contains("Control locomotive L-100"),"Control unloading also interrupts the run");
  Check(Stage2RuntimeChecks.StructuralReadinessError(null,control)!=null,"Missing subject rejected");
  Check(Stage2RuntimeChecks.StructuralReadinessError(loaded,null)!=null,"Missing control rejected");
  var result=new JObject{["passed"]=true,["checks"]=checks,["scope"]="Actual production Begin, tender predicate and readiness/abort helpers with managed car/coroutine boundaries; source routing guards on RunAll. No live Unity loading or full sweep coroutine execution."};
  File.WriteAllText(Path.Combine(args[0],"validation.json"),result.ToString());Console.WriteLine(result);
 }
}
