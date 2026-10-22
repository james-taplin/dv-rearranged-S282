param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\tender-audio-validation'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$sourcePath=Join-Path $PSScriptRoot 'RearrangedS282\ArticulatedEffects.cs'
$source=Get-Content -LiteralPath $sourcePath -Raw
function Extract([string]$marker) {
 $start=$source.IndexOf($marker,[StringComparison]::Ordinal);if($start -lt 0){throw "Missing actual source: $marker"}
 $opening=$source.IndexOf('{',$start);$depth=0
 for($i=$opening;$i -lt $source.Length;$i++){
  if($source[$i] -eq '{'){$depth++};if($source[$i] -eq '}'){$depth--;if($depth -eq 0){return $source.Substring($start,$i-$start+1)}}
 };throw "Unclosed actual source: $marker"
}
$helpers=(Extract 'private static void SeatTenderChuffs')+"`n"+(Extract 'internal static void TenderChanged')+"`n"+(Extract 'private static EffectsState ActiveState')
$state=(Extract 'internal List<Secondary> Secondaries')+"`n"+(Extract 'internal List<Secondary> ParticleSecondaries')+"`n"+(Extract 'internal void ResetAudio')
$generated=@"
using System;using System.Linq;using System.Collections.Generic;using UnityEngine;using DV.ModularAudioCar;using LocoSim.Implementations;
namespace RearrangedS282 { internal static class ArticulatedEffects {
private static void Safe(Action action) => action();
$helpers
$(Extract 'internal sealed class Secondary')
internal abstract class AudioBinding { internal abstract void Remove(); }
internal sealed class EffectsState : MonoBehaviour {
internal TrainCar Car;internal bool Active=true;
internal readonly HashSet<AudioBinding> Audio=new();internal readonly List<string> Calls=new();
internal ArticulatedSimulation Simulation=>Car.GetComponent<ArticulatedSimulation>();
internal Transform Rear=>Car.GetComponent<WheelRearranger>().ArticulatedRearRoot;
internal Transform Middle=>Car.GetComponent<WheelRearranger>().ArticulatedMiddleRoot;
$state
internal void ResetParticles()=>Calls.Add("particles"); internal void RefreshNow()=>Calls.Add("refresh");
}
internal static void Seat(ChuffClipsSimReader r,Transform t)=>SeatTenderChuffs(r,t);
} }
"@
$generated | Set-Content -LiteralPath (Join-Path $OutputDirectory 'ActualTenderAudio.cs') -Encoding utf8
# Reuse existing managed scene/math stand-ins. IsChildOf is the only added API.
$stubs=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'evidence\steam-pipe-tests\ActorStubs.cs') -Raw
$stubs=$stubs.Replace('public Transform Find(string path)', 'public bool IsChildOf(Transform p){for(var t=parent;t!=null;t=t.parent)if(ReferenceEquals(t,p))return true;return false;} public Transform Find(string path)')
$stubs | Set-Content -LiteralPath (Join-Path $OutputDirectory 'SceneStubs.cs') -Encoding utf8
$fixture=Join-Path $PSScriptRoot 'evidence\tender-audio-tests'
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup><ItemGroup><Compile Include="$OutputDirectory\ActualTenderAudio.cs"/><Compile Include="$OutputDirectory\SceneStubs.cs"/><Compile Include="$fixture\Stubs.cs"/><Compile Include="$fixture\Program.cs"/></ItemGroup></Project>
"@
$projectPath=Join-Path $OutputDirectory 'TenderAudio.csproj';Set-Content -LiteralPath $projectPath -Value $project -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- (Join-Path $OutputDirectory 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Tender audio functional checks failed'}
[ordered]@{productionSourceSHA256=(Get-FileHash -LiteralPath $sourcePath).Hash;extractedSourceSHA256=(Get-FileHash -LiteralPath (Join-Path $OutputDirectory 'ActualTenderAudio.cs')).Hash} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $OutputDirectory 'source-receipt.json') -Encoding utf8

