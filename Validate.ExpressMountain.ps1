param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\quartering-hotfix\express'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
function Extract([string]$text,[string]$marker){
 $a=$text.IndexOf($marker);if($a -lt 0){throw $marker};$b=$text.IndexOf('{',$a);$depth=0
 for($i=$b;$i -lt $text.Length;$i++){if($text[$i] -eq '{'){$depth++};if($text[$i] -eq '}'){$depth--;if($depth -eq 0){return $text.Substring($a,$i-$a+1)}}};throw $marker
}
function Expression([string]$file,[string]$marker){$s=Get-Content -LiteralPath (Join-Path $PSScriptRoot $file) -Raw;$a=$s.IndexOf($marker);if($a -lt 0){throw $marker};$s.Substring($a,$s.IndexOf(';',$a)-$a+1)}
$source=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RearrangedS282/WheelRearranger.cs') -Raw
$dispatch=Extract $source 'private void ApplyLegacyGeometry('
$leading=(Extract $source 'private void ShowFourLeadingWheels()')+"`n"+(Extract $source 'private void ShowTwoLeadingWheels()')
$additional=Extract (Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RearrangedS282/WheelRearranger.Additional.cs') -Raw) 'private void AlignAdditionalPoweredAxles('
$clear=Expression 'RearrangedS282/WheelRearranger.LargeDriverClearance.cs' 'internal static bool UsesReservoirClearance('
$casings=Expression 'RearrangedS282/WheelRearranger.CylinderCasings.cs' 'private static bool UsesFullCylinderCasings('
$methods=@([regex]::Matches($dispatch,'\b(Show\w+|Hide\w+|ResetBody|ResetToFourCoupled|ApplyGiantSingleDriverPrototype)\(')|ForEach-Object{$_.Groups[1].Value}|Sort-Object -Unique)
$leaf=foreach($name in $methods){if($name -notin @('ShowFourLeadingWheels','ShowTwoLeadingWheels')){
 $extra=if($name -eq 'HideLeadingWheels'){'firstFrontAxle.gameObject.SetActive(false);secondFrontAxle.gameObject.SetActive(false);'}else{''}
 "private void $name(){Calls.Add(`"$name`");$extra}"
}}
$actual=@"
using System;using System.Collections.Generic;using System.Linq;using UnityEngine;
namespace UnityEngine {
 public struct Vector2 { public Vector2(float x,float y){} }
 public struct Vector3 { public float x,y,z;public Vector3(float a,float b,float c){x=a;y=b;z=c;} }
 public class GameObject { public bool activeSelf;public void SetActive(bool value)=>activeSelf=value; }
 public class Transform { public Vector3 localPosition;public GameObject gameObject=new(); }
}
namespace RearrangedS282 {
 internal sealed class Settings {internal enum X82Options{vanilla,small_wheels,small_wheels_alternate}internal enum X102Options{vanilla,small_wheels}internal X82Options x42Options,x82Options;internal X102Options x62Options,x102Options;internal bool show8CoupledValveGear=true;}
 internal static class Main {internal static Settings settings=new();internal static Logger Logger=new();}
 internal sealed class Logger{internal void Log(string text){}}
 internal sealed class Split{internal void MovePilot(Vector2 position){}}
 internal sealed class WheelRearranger {
 private Settings ArrangementSettings=>Main.settings;
 private WheelArrangementType currentWA=WheelArrangementType.s282;private float derailModifier;private Split splitS282A=new();private Transform frontHandrail=new();
 internal List<string> Calls=new();internal List<(Transform,Transform)> Alignments=new();
 internal Transform firstFrontAxle=new(),secondFrontAxle=new();
 private Transform firstFrontAxleSupport=new(),firstFrontAxleSupportLOD=new(),secondFrontAxleSupport=new(),secondFrontAxleSupportLOD=new();
 private Transform driveAxle1=new(),driveAxle2=new(),driveAxle3=new(),driveAxle4=new();
 private Transform firstRightDriveWheel=new(),secondRightDriveWheel=new(),thirdRightDriveWheel=new(),fourthRightDriveWheel=new();private Transform[] giantSingleWheels={new(),new()};
 private Transform ResolvePresentedWheel(Transform t)=>t;
 private void AlignAdditionalAxle(Transform a,Transform b)=>Alignments.Add((a,b));
 $dispatch
 $leading
 $additional
 $clear
 $casings
 $($leaf -join "`n")
 internal void Apply(int id){Calls.Clear();Alignments.Clear();ApplyLegacyGeometry(id);AlignAdditionalPoweredAxles(id);}
 internal bool CorrectFourAxles=>Alignments.SequenceEqual(new[]{(driveAxle1,firstRightDriveWheel),(driveAxle2,secondRightDriveWheel),(driveAxle3,thirdRightDriveWheel),(driveAxle4,fourthRightDriveWheel)});
 internal static bool FullCasings(int id)=>UsesFullCylinderCasings(id);
 }
 internal static class Program {
 static int checks;static void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
 static void Main(string[] args){
  Check((int)WheelArrangementType.s482Big==48 && WheelArrangement.SupportedCount==49,"Appended catalogue identity");
  Check(WheelArrangement.WheelArrangementNames[16]=="4-8-2 Mountain" && WheelArrangement.WheelArrangementNames[48]=="4-8-2 Express","Retained Mountain and distinct Express");
  Check(string.Join("-",WheelArrangement.LocomotiveWheels(48))=="4-8-2" && WheelArrangement.NumOfNondrivenWheels[48]==6,"Four drivers and three carrying axles");
  Check(PoweredTenderPolicy.Supports(48),"Powered tender is available");
  foreach(Settings.X82Options style in Enum.GetValues(typeof(Settings.X82Options))){
   RearrangedS282.Main.settings.x82Options=style;var r=new WheelRearranger();r.Apply(48);
   Check(r.Calls.Contains("Show8BigDrivers")&&!r.Calls.Contains("Show8RearDrivers"),"Reuses express drivetrain, not small Mountain drivers");
   Check(r.firstFrontAxle.gameObject.activeSelf&&r.secondFrontAxle.gameObject.activeSelf,"Both leading axles present");
   Check(r.firstFrontAxle.localPosition.z==2.75f&&r.secondFrontAxle.localPosition.z==1.45f,"Forward truck position leaves room for large drivers");
   Check(r.Calls.Contains(style==Settings.X82Options.vanilla?"ShowTwoTrailingWheelsVanilla":"ShowTwoTrailingWheels"),"Express trailing truck matches large-driver Mikado policy");
   Check(r.CorrectFourAxles,"All four physical driver anchors follow displayed wheels");
   Check(WheelRearranger.UsesReservoirClearance(48)&&WheelRearranger.FullCasings(48),"Reservoir clearance and normalized cylinder casings included");
   r.Apply(16);Check(r.Calls.Contains("Show8RearDrivers")&&r.Calls.Contains("ShowProper4LeadingWheels"),"Existing Mountain route retained");
   r.Apply(13);Check(r.Calls.Contains("Show8BigDrivers")&&!r.secondFrontAxle.gameObject.activeSelf,"Existing express Mikado route retained");
  }
  var result=new{passed=true,checks,scope="Actual production arrangement dispatcher, leading truck placement, axle-alignment dispatch and appearance predicates with managed leaf stand-ins. Shared geometry helpers are protected separately by compiled comparison; live clearance/motion remains a game check."};
  System.IO.File.WriteAllText(args[0],System.Text.Json.JsonSerializer.Serialize(result));Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(result));
 }
}
}
"@
Set-Content -LiteralPath (Join-Path $OutputDirectory 'ActualExpress.cs') -Value $actual -Encoding utf8
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><DefineConstants>B99_ALL_STANDARD</DefineConstants><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources><NoWarn>CS0649</NoWarn></PropertyGroup><ItemGroup><Compile Include="$OutputDirectory/ActualExpress.cs"/><Compile Include="$PSScriptRoot/RearrangedS282/WheelArrangement.cs"/><Compile Include="$PSScriptRoot/RearrangedS282/PoweredTenderPolicy.cs"/></ItemGroup></Project>
"@
$path=Join-Path $OutputDirectory 'Express.csproj';Set-Content -LiteralPath $path -Value $project -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $path -c Release -- (Join-Path $OutputDirectory 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Express arrangement integration validation failed'}
