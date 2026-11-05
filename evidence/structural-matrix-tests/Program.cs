using System;using System.Linq;using System.IO;using Newtonsoft.Json.Linq;using RearrangedS282;
internal static class Program {
 static int checks;static void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
 static bool Accept(int id,bool tender,JObject row){try{Stage2RuntimeChecks.ValidatePresentation(id,tender,row);return true;}catch(InvalidOperationException){return false;}}
 static bool AcceptOil(JObject row){try{Stage2RuntimeChecks.ValidateOilCupPresentation(row);return true;}catch(InvalidOperationException){return false;}}
 static JObject GoodOil()=>new JObject{["ready"]=true,["countsMatch"]=true,["uncoveredBigEnds"]=0,["coverageComplete"]=true,["lidControlsBound"]=true,["maximumProviderGapM"]=0f,["maximumGuideGapM"]=0f,["maximumCouplingSeatGapM"]=0f,["nativeLubricatorAnimationBound"]=true};
 static JObject Good(int id,bool tender){bool a=WheelArrangement.IsArticulatedId(id),t=WheelArrangement.LocomotiveWheels(id).Length==5;return new JObject{
  ["arrangementId"]=id,["frontFramePieces"]=a?2:0,["middleFramePieces"]=t?2:0,["rearFramePieces"]=a&&id!=47?2:0,
  ["activeLocomotiveEngines"]=t?3:a?2:1,["tenderSelected"]=tender,["hasSupportedTender"]=true,
  ["effectiveSettings"]=new JObject{["show4CoupledValveGear"]=false},
  ["animationOwnership"]=new JObject{["locomotive"]=new JArray(Enumerable.Range(0,id==33?0:2*(t?3:a?2:1)).Select(i=>new JObject{["passed"]=true,["phaseOwned"]=a})),["tender"]=new JArray(Enumerable.Range(0,tender?2:0).Select(i=>new JObject{["passed"]=true}))},
  ["independentDrive"]=a?new JObject{["active"]=true,["ready"]=true}:null,
  ["franklinLinkages"]=new JArray(Enumerable.Range(0,id==31||id==32?4:id==38||id==40||id==47?6:0).Select(i=>new JObject{["drive"]="drive"+i,["mainRod"]=new JObject{["enabled"]=true,["mesh"]=true,["detailLod"]=true},["crank"]=new JObject{["enabled"]=true,["mesh"]=true,["detailLod"]=true}})),
  ["franklinClearanceMeshes"]=id==32?2:0,
  ["malletDriverPitch"]=new JArray(Enumerable.Range(0,a?2*WheelArrangement.LocomotiveWheels(id).Skip(1).Take(WheelArrangement.LocomotiveWheels(id).Length-2).Count(n=>n==6):0).Select(i=>new JObject{["passed"]=true})),
  ["compactTriplexCrankPins"]=id==47?new JArray(Enumerable.Range(0,6).Select(i=>new JObject{["drive"]="drive"+i,["enabled"]=true,["mesh"]=true,["detailLod"]=true})):null,
  ["tender"]=tender?new JObject{["attached"]=true,["sourceReady"]=true,["sourceStaticCount"]=16,["undercarriagePresentationReady"]=true,["underframeSteamConnection"]=new JObject{["trailingSupportRegistered"]=true},["unexpectedFranklinAssemblies"]=0,["exhaustOutlet"]=true,["visualEffects"]=true,["audioEffects"]=true}:null};}
 static void Main(string[] args){
  foreach(int id in Enumerable.Range(0,51))foreach(string label in new[]{"front","middle","rear"}){
   int geometryId=id==50?37:id;
   Check(Stage2RuntimeChecks.FramePiecePrefix(id,label)=="[Rearranged "+label+" frame "+geometryId+"] ","Frame tags match the actual shared geometry cache");
  }
  Check("[Rearranged front frame 37] frame_LOD0".StartsWith(Stage2RuntimeChecks.FramePiecePrefix(50,"front"),StringComparison.Ordinal),"Little Boy discovers its reused Mallet frame");
  Check(!"[Rearranged front frame 50] frame_LOD0".StartsWith(Stage2RuntimeChecks.FramePiecePrefix(50,"front"),StringComparison.Ordinal),"Unrelated frame cache cannot hide the shared-geometry contract");
  var matrix=Stage2RuntimeChecks.StructuralCases();Check(matrix.Length==96,"96 matrix cases");Check(matrix.Count(c=>!c.Tender)==51,"All51 ordinary-tender cases");Check(matrix.Count(c=>c.Tender)==45,"All45 supported powered cases");Check(matrix.Distinct().Count()==96,"No duplicate modes");
  foreach(int id in Enumerable.Range(0,51)){Check(matrix.Count(c=>c.Id==id&&!c.Tender)==1,"Every arrangement tested ordinary");Check(matrix.Count(c=>c.Id==id&&c.Tender)==(PoweredTenderPolicy.Supports(id)?1:0),"Powered exclusions match runtime policy");}
  foreach(var c in matrix){
   Check(Accept(c.Id,c.Tender,Good(c.Id,c.Tender)),"Valid matrix presentation accepted");
   var animationBad=Good(c.Id,c.Tender);animationBad.Remove("animationOwnership");Check(!Accept(c.Id,c.Tender,animationBad),"Missing ownership cannot pass");
   animationBad=Good(c.Id,c.Tender);animationBad.Remove("effectiveSettings");Check(!Accept(c.Id,c.Tender,animationBad),"Missing effective settings cannot pass");
   foreach(string role in new[]{"locomotive","tender"}){
    animationBad=Good(c.Id,c.Tender);var drives=(JArray)animationBad["animationOwnership"][role];
    if(drives.Count>0){drives[0]["passed"]=false;Check(!Accept(c.Id,c.Tender,animationBad),"Invalid active speed/phase writer fails");}
    animationBad=Good(c.Id,c.Tender);((JArray)animationBad["animationOwnership"][role]).Add(new JObject{["passed"]=true});Check(!Accept(c.Id,c.Tender,animationBad),"Duplicate/unexpected animation drive fails");
   }
   if(WheelArrangement.IsArticulatedId(c.Id)){
    foreach(string field in new[]{"active","ready"}){animationBad=Good(c.Id,c.Tender);animationBad["independentDrive"][field]=false;Check(!Accept(c.Id,c.Tender,animationBad),"Native fallback cannot pass independent validation");}
    animationBad=Good(c.Id,c.Tender);animationBad["animationOwnership"]["locomotive"][0]["phaseOwned"]=false;Check(!Accept(c.Id,c.Tender,animationBad),"Unowned engine cannot hide behind other active groups");
   }
   Check(AcceptOil(GoodOil()),"Complete oilcup coverage accepted for every matrix case");
   var franklinBad=Good(c.Id,c.Tender);((JObject)franklinBad).Remove("franklinLinkages");Check(!Accept(c.Id,c.Tender,franklinBad),"Missing Franklin report cannot pass");
   franklinBad=Good(c.Id,c.Tender);((JArray)franklinBad["franklinLinkages"]).Add(new JObject());Check(!Accept(c.Id,c.Tender,franklinBad),"Unexpected or duplicate Franklin engine fails");
   franklinBad=Good(c.Id,c.Tender);franklinBad["franklinClearanceMeshes"]=(int)franklinBad["franklinClearanceMeshes"]+1;Check(!Accept(c.Id,c.Tender,franklinBad),"Missing or unexpected cylinder clearance correction fails");
   if(((JArray)Good(c.Id,c.Tender)["franklinLinkages"]).Count>0)foreach(string part in new[]{"mainRod","crank"})foreach(string field in new[]{"enabled","mesh","detailLod"}){
    franklinBad=Good(c.Id,c.Tender);franklinBad["franklinLinkages"][0][part][field]=false;Check(!Accept(c.Id,c.Tender,franklinBad),"Hidden/missing Franklin part fails: "+part+" "+field);
   }
   var pitchBad=Good(c.Id,c.Tender);var pitchRows=(JArray)pitchBad["malletDriverPitch"];
   if(pitchRows.Count>0)pitchRows[0]["passed"]=false;else pitchRows.Add(new JObject{["passed"]=true});
   Check(!Accept(c.Id,c.Tender,pitchBad),"Missing/misaligned or unexpected Mallet pitch hold fails");
   foreach(string field in new[]{"arrangementId","frontFramePieces","middleFramePieces","rearFramePieces"}){var row=Good(c.Id,c.Tender);row[field]=(int)row[field]+1;Check(!Accept(c.Id,c.Tender,row),"Reject fallback/missing/wrong frame: "+field);}
   var bad=Good(c.Id,c.Tender);bad["tenderSelected"]=!c.Tender;Check(!Accept(c.Id,c.Tender,bad),"Wrong tender mode fails");
   if(c.Id!=33){bad=Good(c.Id,c.Tender);bad["activeLocomotiveEngines"]=(int)bad["activeLocomotiveEngines"]+1;Check(!Accept(c.Id,c.Tender,bad),"Wrong active engine count fails");}
   if(c.Tender){foreach(string field in new[]{"attached","sourceReady","undercarriagePresentationReady","exhaustOutlet","visualEffects","audioEffects"}){bad=Good(c.Id,true);bad["tender"][field]=false;Check(!Accept(c.Id,true,bad),"Missing tender binding fails: "+field);}
    bad=Good(c.Id,true);bad["hasSupportedTender"]=false;Check(!Accept(c.Id,true,bad),"Missing physical tender fails");bad=Good(c.Id,true);((JObject)bad["tender"]).Remove("sourceStaticCount");Check(!Accept(c.Id,true,bad),"Missing source count cannot silently pass");
    bad=Good(c.Id,true);bad["tender"]["unexpectedFranklinAssemblies"]=4;Check(!Accept(c.Id,true,bad),"Extra inherited Franklin cylinder assemblies fail");
    bad=Good(c.Id,true);bad["tender"]["underframeSteamConnection"]["trailingSupportRegistered"]=false;Check(!Accept(c.Id,true,bad),"Unregistered rear trailing-axle support fails");
   }
  }
  Check(Accept(47,false,Good(47,false)),"Compact triplex correctly permits no rear bar slice");
  foreach(int pin in Enumerable.Range(0,6)) foreach(string field in new[]{"enabled","mesh","detailLod"}){
   var row=Good(47,false);row["compactTriplexCrankPins"][pin][field]=false;
   Check(!Accept(47,false,row),"Reject invisible/missing/non-LOD crank pin "+pin+" "+field);
  }
  var missing=Good(47,false);((JObject)missing).Remove("compactTriplexCrankPins");Check(!Accept(47,false,missing),"Missing pin report cannot pass");
  missing=Good(47,false);((JArray)missing["compactTriplexCrankPins"]).RemoveAt(0);Check(!Accept(47,false,missing),"Missing front pin fails");
  missing=Good(47,false);((JArray)missing["compactTriplexCrankPins"]).Add(new JObject{["enabled"]=true,["mesh"]=true,["detailLod"]=true});Check(!Accept(47,false,missing),"Extra pin fails");
  foreach(string field in new[]{"ready","countsMatch","lidControlsBound","nativeLubricatorAnimationBound","coverageComplete"}){
   var cups=GoodOil();cups[field]=false;Check(!AcceptOil(cups),"Failed oilcup binding or coverage is rejected: "+field);
   cups=GoodOil();cups.Remove(field);Check(!AcceptOil(cups),"Missing oilcup binding or coverage cannot pass: "+field);
  }
  foreach(int dropped in new[]{1,4}){var cups=GoodOil();cups["uncoveredBigEnds"]=dropped;Check(!AcceptOil(cups),"Allocated cups matching counts cannot conceal uncovered bearings");}
  var missingCoverage=GoodOil();missingCoverage.Remove("uncoveredBigEnds");Check(!AcceptOil(missingCoverage),"Legacy report without oiling coverage cannot pass");
  foreach(var gap in new[]{("maximumProviderGapM",.002f),("maximumGuideGapM",.035f),("maximumCouplingSeatGapM",.002f)}){
   var cups=GoodOil();cups[gap.Item1]=gap.Item2;Check(AcceptOil(cups),"Oilcup tolerance boundary accepted");
   cups[gap.Item1]=gap.Item2+.0001f;Check(!AcceptOil(cups),"Oilcup beyond tolerance rejected");
  }
  var report=new JObject{["passed"]=true,["checks"]=checks,["matrixCases"]=matrix.Length,["poweredTenderCases"]=matrix.Count(c=>c.Tender),["ordinaryTenderCases"]=matrix.Count(c=>!c.Tender),["scope"]="Actual production matrix and pass/fail validators with current catalogue and powered policy. No game coroutine, effects or full-world execution."};
  File.WriteAllText(args[0],report.ToString());Console.WriteLine(report.ToString());
 }
}
