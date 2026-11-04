param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\offline-validation\ScotchDispatch'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$out=(Resolve-Path -LiteralPath $OutputDirectory).Path;$root=$PSScriptRoot
function Extract([string]$text,[string]$marker){
 $a=$text.IndexOf($marker,[StringComparison]::Ordinal);if($a -lt 0){throw "Missing $marker"};$b=$text.IndexOf('{',$a);$d=0
 for($i=$b;$i -lt $text.Length;$i++){if($text[$i] -eq '{'){$d++};if($text[$i] -eq '}'){$d--;if($d -eq 0){return $text.Substring($a,$i-$a+1)}}};throw "Unbalanced $marker"
}
$legacy=Extract (Get-Content -LiteralPath (Join-Path $root 'RearrangedS282\WheelRearranger.cs') -Raw) 'private void ApplyLegacyGeometry('
$additional=Extract (Get-Content -LiteralPath (Join-Path $root 'RearrangedS282\WheelRearranger.Additional.cs') -Raw) 'private void AlignAdditionalPoweredAxles('
$conventional=Get-Content -LiteralPath (Join-Path $root 'RearrangedS282\WheelRearranger.Conventional.cs') -Raw
$first=$conventional.IndexOf('ApplyLegacyGeometry(id);');$last=$conventional.IndexOf('AlignAdditionalPoweredAxles(id);',$first)
if($first -lt 0 -or $last -lt 0){throw 'Conventional geometry branch changed'}
$chain=$conventional.Substring($first,$last-$first+'AlignAdditionalPoweredAxles(id);'.Length)
$casings=Get-Content -LiteralPath (Join-Path $root 'RearrangedS282\WheelRearranger.CylinderCasings.cs') -Raw
$a=$casings.IndexOf('private static bool UsesFullCylinderCasings(');if($a -lt 0){throw 'Missing casing predicate'}
$casings=$casings.Substring($a,$casings.IndexOf(';',$a)-$a+1)
$names=@([regex]::Matches($legacy,'\b(Show\w+|Hide\w+|ResetBody|ResetToFourCoupled|ApplyGiantSingleDriverPrototype)\(')|ForEach-Object{$_.Groups[1].Value}|Sort-Object -Unique)
$leaves=foreach($name in $names){"private void $name(){Calls.Add(`"$name`");}"}
$code=@"
using System;using System.Collections.Generic;using System.Linq;
namespace UnityEngine { public struct Vector2 {public Vector2(float x,float y){}} public class GameObject {public void SetActive(bool v){}} public class Transform {public GameObject gameObject=new();} }
namespace RearrangedS282 {
using UnityEngine;
internal sealed class Settings {internal enum X82Options{vanilla,small_wheels,small_wheels_alternate} internal enum X102Options{vanilla,small_wheels} internal X82Options x42Options,x82Options;internal X102Options x62Options,x102Options;internal bool show8CoupledValveGear=true,polishedPacificHudsonValveGear=true,show282BigValveGear=true,show4CoupledValveGear=true,show6CoupledValveGear=true,show12CoupledValveGear=true;}
internal static class Main {internal static Settings settings=new();internal static Logger Logger=new();} internal sealed class Logger{internal void Log(string s){}}
internal sealed class Split {internal void MovePilot(Vector2 p){}} internal sealed class Sim {internal Force drivingForce=new();} internal sealed class Force {internal float wheelRadius=.9144f;}
internal sealed class WheelRearranger {
 internal readonly List<string> Calls=new();internal readonly List<(Transform,Transform)> Alignments=new();
 private Settings ArrangementSettings=>Main.settings;private WheelArrangementType currentWA=WheelArrangementType.s282;private float derailModifier,prairieClearanceCylinderShiftM;private const float PrairieCylinderForward=0f,PrairieDriverForward=0f;
 private Split splitS282A=new();private Sim sim=new();private Transform frontHandrail=new();
 private Transform driveAxle1=new(),driveAxle2=new(),driveAxle3=new(),driveAxle4=new();private Transform firstRightDriveWheel=new(),secondRightDriveWheel=new(),thirdRightDriveWheel=new(),fourthRightDriveWheel=new();private Transform[] giantSingleWheels={new(),new()};
 private Transform ResolvePresentedWheel(Transform t)=>t;private void AlignAdditionalAxle(Transform a,Transform b)=>Alignments.Add((a,b));
 private void ApplyScotchGliderGeometry()=>Calls.Add("Scotch");private void ApplyLargeDriverClearance(int id,float r)=>Calls.Add("clearance");private void ApplyPrairieForwardSpacing(bool b)=>Calls.Add("prairie");private void ApplyDriverLayout(int id)=>Calls.Add("layout");private void ApplyConnectingRodSpacing(int id,float v)=>Calls.Add("rods");private void ApplyPacificHudsonValveGear(bool b)=>Calls.Add(b?"gear-on":"gear-off");private static bool UsesReservoirClearance(int id)=>false;
 $legacy
 $additional
 $casings
 $($leaves -join "`n")
 internal void Run(int id){Calls.Clear();Alignments.Clear();$chain}
 internal bool Aligned=>Alignments.SequenceEqual(new[]{(driveAxle2,secondRightDriveWheel),(driveAxle3,thirdRightDriveWheel),(driveAxle4,fourthRightDriveWheel)});
 internal static bool FullCasings(int id)=>UsesFullCylinderCasings(id);
}
internal static class Program {
 static int n;static void Check(bool b,string s){n++;if(!b)throw new Exception(s);}
 static void Main(string[] args){
  Check((int)WheelArrangementType.s462Scotch==49 && WheelArrangement.IsConventionalId(49),"ID49 conventional and stable");
  Check(string.Join("-",WheelArrangement.LocomotiveWheels(49))=="4-6-2" && WheelArrangement.NumOfNondrivenWheels[49]==6,"Expected wheel count");
  foreach(Settings.X102Options style in Enum.GetValues(typeof(Settings.X102Options))) foreach(bool gear in new[]{true,false}){
   RearrangedS282.Main.settings.x62Options=style;RearrangedS282.Main.settings.polishedPacificHudsonValveGear=gear;var x=new WheelRearranger();x.Run(49);
   Check(x.Calls.Contains("ShowProper4LeadingWheels")&&x.Calls.Contains("Show6Drivers"),"Pacific native 4-6 running gear dispatched");
   Check(x.Calls.Contains(style==Settings.X102Options.vanilla?"ShowTwoTrailingWheelsFor6CoupledVanilla":"ShowTwoTrailingWheelsFor6Coupled"),"Requested native trailing truck dispatched");
   Check(x.Calls.IndexOf("Show6Drivers")<x.Calls.IndexOf("Scotch")&&x.Calls.IndexOf("Scotch")<x.Calls.IndexOf("rods"),"Scotch hook follows legacy and precedes rod processing");
   Check(x.Calls.Contains(gear?"gear-on":"gear-off")&&x.Aligned&&WheelRearranger.FullCasings(49),"Pacific gear policy, physical axles and full casings");
   x.Run(4);Check(!x.Calls.Contains("Scotch")&&x.Calls.Contains("Show6Drivers"),"Existing Pacific remains unchanged");
  }
  var o=new{passed=true,checks=n,arrangementId=49,scope="Actual legacy and conventional branch source plus axle dispatch/predicate with leaf stand-ins; not the full TrySwitch transaction or live game."};System.IO.File.WriteAllText(args[0],System.Text.Json.JsonSerializer.Serialize(o));Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(o));
 }
}
}
"@
$actual=Join-Path $out 'ActualDispatch.cs';Set-Content -LiteralPath $actual -Value $code -Encoding utf8
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources><NoWarn>CS0649</NoWarn></PropertyGroup><ItemGroup><Compile Include="$actual"/><Compile Include="$root\RearrangedS282\WheelArrangement.cs"/></ItemGroup></Project>
"@
$path=Join-Path $out 'ScotchDispatch.csproj';Set-Content -LiteralPath $path -Value $project -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $path -c Release -- (Join-Path $out 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Scotch actual branch dispatch failed'}
