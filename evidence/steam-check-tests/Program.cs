using System;
using System.IO;
using Newtonsoft.Json.Linq;
using RearrangedS282;
internal static class Program
{
    static int checks;
    static void Check(bool value,string message){checks++;if(!value)throw new Exception(message);}
    static JObject Good()=>new JObject{["expectedCylinders"]=2,["connectedCylinders"]=2,["colliders"]=0,["maximumEndpointGapM"]=.00003f,["error"]=JValue.CreateNull()};
    static bool Accept(JObject row){try{Stage2RuntimeChecks.ValidateSteamConnections(row);return true;}catch(InvalidOperationException){return false;}}
    static void Main(string[] args){
        string noError=null;var row=Good();row["error"]=noError;
        Check(row["error"].Type==JTokenType.String,"Actual Newtonsoft null-string semantics changed; investigate original failure");
        Check(row["error"].ToString(Newtonsoft.Json.Formatting.None)=="null","Expected serialized-null regression case");
        Check(Accept(row),"False structural failure on a null-valued error string");
        Check(Accept(Good()),"False structural failure on explicit null");
        row=Good();row["error"]="genuine presentation failure";Check(!Accept(row),"Real error was hidden");
        row=Good();row["connectedCylinders"]=1;Check(!Accept(row),"Missing cylinder not rejected");
        row=Good();row["colliders"]=1;Check(!Accept(row),"Added collider not rejected");
        foreach(float gap in new[]{.0031f,-.001f,float.NaN,float.PositiveInfinity}){row=Good();row["maximumEndpointGapM"]=gap;Check(!Accept(row),"Invalid endpoint gap accepted");}
        row=Good();row["expectedCylinders"]=0;row["connectedCylinders"]=0;row["maximumEndpointGapM"]=0f;Check(Accept(row),"Stock/novelty no-pipe state rejected");
        var result=new JObject{["passed"]=true,["checks"]=checks,["scope"]="Actual production structural pipe validator and installed Newtonsoft assembly; original null-string false failure reproduced"};
        File.WriteAllText(args[0],result.ToString());Console.WriteLine(result.ToString());
    }
}
