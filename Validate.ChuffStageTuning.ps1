param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\runtime-trial-1.22.2\chuff-validation'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$OutputDirectory=(Resolve-Path $OutputDirectory).Path
$fixture=@'
using System;
using System.Linq;
using System.Text.Json;
using System.IO;
using RearrangedS282;
namespace UnityEngine {
 public struct Keyframe {public float time,value,inTangent,outTangent,inWeight,outWeight;public int weightedMode;public Keyframe(float t,float v){time=t;value=v;inTangent=outTangent=inWeight=outWeight=0;weightedMode=0;}}
 public sealed class AnimationCurve {private Keyframe[] data;public int preWrapMode,postWrapMode;public AnimationCurve(params Keyframe[] k){data=k.ToArray();}public Keyframe[] keys=>data.ToArray();}
}
class Program {
 static int checks;static void Check(bool value,string text){checks++;if(!value)throw new Exception(text);}
 static void Main(string[] args){
  for(int id=0;id<47;id++)Check(ChuffStageTuning.HasLowPressureFront(id)==new[]{37,41,42,43,45,46}.Contains(id),"Only Mallet/Erie LP front positions selected");
  foreach(float p in new[]{0f,.5f,1f,2f,3f,4.5f,10f,40f}){float q=ChuffStageTuning.Pressure(p);Check(p<=1?q==p:q>1&&q<p,"Pressure thresholds preserve silence and ordering");}
  Check(ChuffStageTuning.Pressure(3)<ChuffStageTuning.Pressure(4.5f),"Medium below high");
  var keys=new[]{new UnityEngine.Keyframe(0,0),new UnityEngine.Keyframe(1,0),new UnityEngine.Keyframe(4,1){inTangent=.2f,outTangent=float.PositiveInfinity,inWeight=.2f,outWeight=.3f,weightedMode=3}};
  var source=new UnityEngine.AnimationCurve(keys){preWrapMode=2,postWrapMode=8};
  for(int cycle=0;cycle<5;cycle++){
   var result=ChuffStageTuning.Volume(source);Check(!ReferenceEquals(source,result),"Per-reader curve isolation");
   Check(source.keys[2].time==4&&source.keys[2].inTangent==.2f,"Authored curve unchanged across reapply");
   Check(result.keys[0].time==0&&result.keys[0].value==0&&result.keys[1].time==1,"Silent keys retained");
   Check(Math.Abs(result.keys[2].time-3.55f)<.000001f&&result.keys[2].value==1,"Pressure span changed without changing key volume");
   Check(result.keys[2].inWeight==.2f&&result.keys[2].outWeight==.3f&&result.keys[2].weightedMode==3,"Weight metadata retained");
   Check(float.IsPositiveInfinity(result.keys[2].outTangent)&&Math.Abs(result.keys[2].inTangent-.2f/.85f)<.000001f,"Step and scaled tangents retained");
   Check(result.preWrapMode==2&&result.postWrapMode==8,"Wrap behaviour retained");
  }
  File.WriteAllText(args[0],JsonSerializer.Serialize(new{passed=true,checks,limitation="Actual profile helper with managed curve metadata stand-ins; not an audio listening or Unity interpolation test"}));Console.WriteLine($"Chuff stage profile: {checks} checks passed");
 }
}
'@
Set-Content (Join-Path $OutputDirectory 'Fixture.cs') $fixture
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup><ItemGroup><Compile Include="$PSScriptRoot\RearrangedS282\ChuffStageTuning.cs"/><Compile Include="$OutputDirectory\Fixture.cs"/></ItemGroup></Project>
"@
$proj=Join-Path $OutputDirectory 'Chuff.Validation.csproj';Set-Content $proj $project
& 'C:\Program Files\dotnet\dotnet.exe' run --project $proj -c Release -- (Join-Path $OutputDirectory 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Stage chuff validation failed'}
