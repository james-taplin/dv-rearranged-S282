param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'build\multiplayer-oiling-validation'))
$ErrorActionPreference = 'Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory = (Resolve-Path -LiteralPath $OutputDirectory).Path
$sourcePath = Join-Path $PSScriptRoot 'RearrangedS282\BigEndOiling.cs'
$source = [IO.File]::ReadAllText($sourcePath)
$start = $source.IndexOf('private static void HoldUnusedFull(')
if ($start -lt 0) { throw 'Production oiling gate was not found.' }
$opening = $source.IndexOf('{', $start)
$depth = 1
$end = $opening + 1
while ($depth -gt 0 -and $end -lt $source.Length) {
    if ($source[$end] -eq '{') { $depth++ }
    if ($source[$end] -eq '}') { $depth-- }
    $end++
}
if ($depth -ne 0) { throw 'Unbalanced production oiling gate.' }
$method = $source.Substring($start, $end - $start)
$harness = @'
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
namespace RearrangedS282 {
internal static class MultiplayerSync { internal static bool IsSimulationAuthority=true; }
internal static class BigEndOilingPlan { internal const int NativePoints=6; }
internal sealed class Port { internal float Value; }
internal sealed class Point {
 internal Port oilLevelReadOut=new(),oilLevelNormalizedReadOut=new(),refillingFlowNormalizedReadOut=new(),pointDoorExtIn=new();
 internal float[] Values()=>new[]{oilLevelReadOut.Value,oilLevelNormalizedReadOut.Value,refillingFlowNormalizedReadOut.Value,pointDoorExtIn.Value};
}
internal sealed class ManualOilingPoints { internal float capacity=2; internal Point[] oilingPoints; }
internal static class BigEndOilingFixture {
 private sealed class State { internal bool[] Active; }
 private static readonly ConditionalWeakTable<ManualOilingPoints,State> States=new();
 internal static void Apply(ManualOilingPoints points,bool[] active) {
  States.Remove(points);if(active!=null)States.Add(points,new State{Active=active});HoldUnusedFull(points);
 }
 // PRODUCTION_METHOD
}
internal static class Probe {
 private static int checks;
 private static void Check(bool condition,string message){if(!condition)throw new Exception(message);checks++;}
 private static ManualOilingPoints Make()=>new(){oilingPoints=Enumerable.Range(0,20).Select(i=>new Point{
  oilLevelReadOut=new Port{Value=.25f+i/100f},oilLevelNormalizedReadOut=new Port{Value=.125f+i/200f},
  refillingFlowNormalizedReadOut=new Port{Value=.6f},pointDoorExtIn=new Port{Value=1f}}).ToArray()};
 internal static int Main(string[] args){
  foreach(bool assigned in new[]{false,true}){
   var points=Make();var before=points.oilingPoints.SelectMany(p=>p.Values()).ToArray();
   var active=assigned?Enumerable.Range(0,20).Select(i=>i<7).ToArray():null;
   MultiplayerSync.IsSimulationAuthority=false;
   try{for(int i=0;i<100;i++)BigEndOilingFixture.Apply(points,active);}
   finally{MultiplayerSync.IsSimulationAuthority=true;}
   Check(points.oilingPoints.SelectMany(p=>p.Values()).SequenceEqual(before),"Guest arrangement setup and oiling gate preserve every mirrored oil/door/flow port");
   BigEndOilingFixture.Apply(points,active);
   for(int i=0;i<20;i++){
    bool held=assigned?!active[i]:i>=6;
    Check(held?points.oilingPoints[i].Values().SequenceEqual(new[]{2f,1f,0f,0f}):points.oilingPoints[i].Values().SequenceEqual(before.Skip(i*4).Take(4)),
     "Host keeps only unused points full/closed and preserves live active cups");
   }
  }
  File.WriteAllText(args[0],JsonSerializer.Serialize(new{passed=true,checks,validation="Exact production HoldUnusedFull executed with narrow port/oiling fixtures. Confirms authority and preservation; no Unity, interaction or network session execution."},new JsonSerializerOptions{WriteIndented=true}));
  Console.WriteLine("PASS "+checks+" multiplayer oiling checks");return 0;
 }
}
}
'@
$harness = $harness.Replace('// PRODUCTION_METHOD', $method)
$harnessPath = Join-Path $OutputDirectory 'ActualOilingGate.cs'
[IO.File]::WriteAllText($harnessPath, $harness, [Text.UTF8Encoding]::new($false))
$project = @"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit></PropertyGroup><ItemGroup><Compile Include="$harnessPath"/></ItemGroup></Project>
"@
$projectPath = Join-Path $OutputDirectory 'MultiplayerOiling.csproj'
[IO.File]::WriteAllText($projectPath, $project, [Text.UTF8Encoding]::new($false))
$reportPath = Join-Path $OutputDirectory 'validation.json'
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- $reportPath
if ($LASTEXITCODE -ne 0) { throw 'Multiplayer oiling validation failed.' }
$report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
$report | Add-Member sourceSHA256 (Get-FileHash -LiteralPath $sourcePath).Hash
$report | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $reportPath -Encoding utf8
Get-Content -LiteralPath $reportPath -Raw
