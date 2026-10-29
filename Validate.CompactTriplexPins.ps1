param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build/quartering-hotfix/crank-pins'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
function Extract([string]$file,[string]$marker){
 $source=Get-Content -LiteralPath (Join-Path $PSScriptRoot $file) -Raw
 $a=$source.IndexOf($marker);if($a -lt 0){throw $marker};$b=$source.IndexOf('{',$a);$depth=0
 for($i=$b;$i -lt $source.Length;$i++){if($source[$i] -eq '{'){$depth++};if($source[$i] -eq '}'){$depth--;if($depth -eq 0){return $source.Substring($a,$i-$a+1)}}};throw $marker
}
$current=Extract 'RearrangedS282/WheelRearranger.Triplex.cs' 'private void ConfigureTriplexDrive('
$engine=Extract 'RearrangedS282/WheelRearranger.Articulated.cs' 'private void ConfigureEngineDrive('
$single=Extract 'RearrangedS282/WheelRearranger.Triplex.cs' 'private void ConfigureSingleDriverDrive('
foreach ($body in @($engine,$single)) {
 if (-not $body.Contains('main.GetComponent<MeshRenderer>().enabled = true;') -or
     -not $body.Contains('cutoff.GetComponent<MeshRenderer>().enabled = true;')) { throw 'Shared Franklin renderer restoration missing' }
}
$oldEngine=$engine.Replace('main.GetComponent<MeshRenderer>().enabled = true;','').Replace('cutoff.GetComponent<MeshRenderer>().enabled = true;','').Replace('ConfigureEngineDrive(','ConfigurePreviousEngineDrive(')
$oldSingle=$single.Replace('main.GetComponent<MeshRenderer>().enabled = true;','').Replace('cutoff.GetComponent<MeshRenderer>().enabled = true;','').Replace('ConfigureSingleDriverDrive(','ConfigurePreviousSingleDriverDrive(')
$previous=$current.Replace('ConfigureTriplexDrive(','ConfigurePreviousTriplexDrive(').Replace('ConfigureEngineDrive(','ConfigurePreviousEngineDrive(').Replace('ConfigureSingleDriverDrive(','ConfigurePreviousSingleDriverDrive(')
# The previous compact triplex already repaired its pin, but not a hidden main rod.
$previous=$previous.Replace('ConfigurePreviousEngineDrive(drive, MeshFinder.Instance.DuplexMainRodMesh, true);', 'ConfigurePreviousEngineDrive(drive, MeshFinder.Instance.DuplexMainRodMesh, true); RequireArticulatedPart(drive, "s282_mech_wheels_connect/s282_mech_connect_to_cutoff_base").GetComponent<MeshRenderer>().enabled = true;')
$stubs=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'evidence/steam-pipe-tests/ActorStubs.cs') -Raw
$stubs=$stubs.Replace('public sealed class Animator:MonoBehaviour{}','public sealed class Animator:MonoBehaviour{ public object runtimeAnimatorController; }')
$stubs=$stubs.Replace('public Quaternion localRotation = Quaternion.identity;','public Quaternion localRotation = Quaternion.identity; public Vector3 localEulerAngles { set { localRotation=Quaternion.Euler(value.x,value.y,value.z); } }')
Set-Content -LiteralPath (Join-Path $OutputDirectory 'ActorStubs.cs') -Value $stubs -Encoding utf8
$actual=@"
using System;using System.Collections.Generic;using System.Linq;using UnityEngine;
namespace RearrangedS282 {
 internal static class AssetBundleLoader { internal static object FranklinBAnimCtrl=new(); }
 internal sealed class MeshFinder { internal static MeshFinder Instance=new();internal Mesh DuplexMainRodMesh=new(){name="main"}; }
 internal sealed class WheelRearranger {
 internal bool TwoCoupled {set{triplex.TwoCoupled=value;}}
 private sealed class Layout {internal bool TwoCoupled=true;} private Layout triplex=new();
 private Vector3[] triplexCoupledDriverPositions={new(0,.712f,8.002f),new(0,.712f,6.302f)};
 private Mesh blindDriver=new(){name="blind"},flangedDriver=new(){name="flanged"},articulatedSideRod=new(){name="side"};
 private List<Transform> articulatedAssets=new();
 private static Transform RequireArticulatedPart(Transform t,string p)=>t.Find(p)??throw new Exception(p);
 private static void SetPartActive(Transform t,string p,bool on){var c=t.Find(p);if(c!=null)c.gameObject.SetActive(on);}
 private static Transform NewArticulatedRoot(string name,Transform parent)=>parent.Add(name);
 private static void EnableGeneratedRenderers(Transform t){foreach(var r in t.GetComponentsInChildren<MeshRenderer>(true))r.enabled=true;}
 $current
 $previous
 $engine
 $single
 $oldEngine
 $oldSingle
 internal void Run(Transform t,bool clone,bool old=false){if(old)ConfigurePreviousTriplexDrive(t,clone);else ConfigureTriplexDrive(t,clone);}
 internal void Duplex(Transform t,bool clone,bool old=false){if(old)ConfigurePreviousEngineDrive(t,MeshFinder.Instance.DuplexMainRodMesh,clone);else ConfigureEngineDrive(t,MeshFinder.Instance.DuplexMainRodMesh,clone);}
 }
}
"@
Set-Content -LiteralPath (Join-Path $OutputDirectory 'ActualPinSetup.cs') -Value $actual -Encoding utf8
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources><NoWarn>CS0649</NoWarn></PropertyGroup><ItemGroup><Compile Include="$OutputDirectory/ActorStubs.cs"/><Compile Include="$OutputDirectory/ActualPinSetup.cs"/><Compile Include="$PSScriptRoot/evidence/compact-triplex-tests/Pins.cs"/></ItemGroup></Project>
"@
$path=Join-Path $OutputDirectory 'CrankPins.csproj';Set-Content -LiteralPath $path -Value $project -Encoding utf8
& 'C:/Program Files/dotnet/dotnet.exe' run --project $path -c Release -- (Join-Path $OutputDirectory 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Compact triplex crank pin regression failed'}
