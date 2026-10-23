using System;using System.Linq;using System.IO;using Newtonsoft.Json.Linq;using RearrangedS282;
internal static class Program {
 static int checks;static void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
 static bool Accept(int id,bool tender,JObject row){try{Stage2RuntimeChecks.ValidatePresentation(id,tender,row);return true;}catch(InvalidOperationException){return false;}}
 static JObject Good(int id,bool tender){bool a=WheelArrangement.IsArticulatedId(id),t=WheelArrangement.LocomotiveWheels(id).Length==5;return new JObject{
  ["arrangementId"]=id,["frontFramePieces"]=a?2:0,["middleFramePieces"]=t?2:0,["rearFramePieces"]=a&&id!=47?2:0,
  ["activeLocomotiveEngines"]=t?3:a?2:1,["tenderSelected"]=tender,["hasSupportedTender"]=true,
  ["tender"]=tender?new JObject{["attached"]=true,["sourceReady"]=true,["sourceStaticCount"]=16,["exhaustOutlet"]=true,["visualEffects"]=true,["audioEffects"]=true}:null};}
 static void Main(string[] args){
  var matrix=Stage2RuntimeChecks.StructuralCases();Check(matrix.Length==91,"91 matrix cases");Check(matrix.Count(c=>!c.Tender)==48,"All48 ordinary-tender cases");Check(matrix.Count(c=>c.Tender)==43,"All43 supported powered cases");Check(matrix.Distinct().Count()==91,"No duplicate modes");
  foreach(int id in Enumerable.Range(0,48)){Check(matrix.Count(c=>c.Id==id&&!c.Tender)==1,"Every arrangement tested ordinary");Check(matrix.Count(c=>c.Id==id&&c.Tender)==(PoweredTenderPolicy.Supports(id)?1:0),"Powered exclusions match runtime policy");}
  foreach(var c in matrix){
   Check(Accept(c.Id,c.Tender,Good(c.Id,c.Tender)),"Valid matrix presentation accepted");
   foreach(string field in new[]{"arrangementId","frontFramePieces","middleFramePieces","rearFramePieces"}){var row=Good(c.Id,c.Tender);row[field]=(int)row[field]+1;Check(!Accept(c.Id,c.Tender,row),"Reject fallback/missing/wrong frame: "+field);}
   var bad=Good(c.Id,c.Tender);bad["tenderSelected"]=!c.Tender;Check(!Accept(c.Id,c.Tender,bad),"Wrong tender mode fails");
   if(c.Id!=33){bad=Good(c.Id,c.Tender);bad["activeLocomotiveEngines"]=(int)bad["activeLocomotiveEngines"]+1;Check(!Accept(c.Id,c.Tender,bad),"Wrong active engine count fails");}
   if(c.Tender){foreach(string field in new[]{"attached","sourceReady","exhaustOutlet","visualEffects","audioEffects"}){bad=Good(c.Id,true);bad["tender"][field]=false;Check(!Accept(c.Id,true,bad),"Missing tender binding fails: "+field);}
    bad=Good(c.Id,true);bad["hasSupportedTender"]=false;Check(!Accept(c.Id,true,bad),"Missing physical tender fails");bad=Good(c.Id,true);((JObject)bad["tender"]).Remove("sourceStaticCount");Check(!Accept(c.Id,true,bad),"Missing source count cannot silently pass");
   }
  }
  Check(Accept(47,false,Good(47,false)),"Compact triplex correctly permits no rear bar slice");
  var report=new JObject{["passed"]=true,["checks"]=checks,["matrixCases"]=matrix.Length,["poweredTenderCases"]=43,["ordinaryTenderCases"]=48,["scope"]="Actual production matrix and pass/fail validators with current catalogue and powered policy. No game coroutine, effects or full-world execution."};
  File.WriteAllText(args[0],report.ToString());Console.WriteLine(report.ToString());
 }
}
