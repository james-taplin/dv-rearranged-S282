param([string]$OutputDirectory=(Join-Path $PSScriptRoot '../../build/angus-cup-tests'))
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$out=(Resolve-Path $OutputDirectory).Path
function Extract([string]$marker,[string]$file='RearrangedS282/WheelRearranger.Articulated.cs') {
 $source=Get-Content (Join-Path $root $file) -Raw
 $a=$source.IndexOf($marker);$b=$source.IndexOf('{',$a);$depth=0
 for($i=$b;$i -lt $source.Length;$i++) {if($source[$i]-eq'{'){$depth++};if($source[$i]-eq'}'){$depth--;if($depth-eq0){return $source.Substring($a,$i-$a+1)}}}
 throw "Missing method $marker"
}
$create=Extract 'private Mesh CreateArticulatedSideRod('
$configure=Extract 'private void ConfigureEngineDrive('
$seat=Extract 'private static bool SeatBigEndMarker(' 'RearrangedS282/WheelRearranger.BigEndOiling.cs'
$stubs=Get-Content (Join-Path $root 'evidence/steam-pipe-tests/ActorStubs.cs') -Raw
$stubs=$stubs.Replace('public sealed class Animator:MonoBehaviour{}','public sealed class Animator:MonoBehaviour{ public object runtimeAnimatorController; }')
$stubs=$stubs.Replace('public Quaternion localRotation = Quaternion.identity;','public Quaternion localRotation = Quaternion.identity; public Vector3 localEulerAngles {set{localRotation=Quaternion.Euler(value.x,value.y,value.z);}}')
Set-Content (Join-Path $out 'ActorStubs.cs') $stubs
$actual=@"
using System;using System.Collections.Generic;using System.Linq;using UnityEngine;using Object=UnityEngine.Object;
namespace RearrangedS282 {
 internal static class AssetBundleLoader { internal static object FranklinBAnimCtrl=new(); }
 internal sealed class MeshFinder { internal static MeshFinder Instance=new();internal Mesh TwoAxleSideRodMesh; }
 internal sealed class WheelRearranger {
 private Mesh blindDriver=new(),flangedDriver=new(),articulatedSideRod;
 private List<Mesh> articulatedMeshes=new(); private List<Transform> articulatedAssets=new();
 private static Transform RequireArticulatedPart(Transform t,string p)=>t.Find(p)??throw new Exception(p);
 private static void SetPartActive(Transform t,string p,bool on){var c=t.Find(p);if(c!=null)c.gameObject.SetActive(on);}
 private static Transform NewArticulatedRoot(string n,Transform p)=>p.Add(n);
 $create
 $configure
 $seat
 internal void SetUp(Mesh two){MeshFinder.Instance.TwoAxleSideRodMesh=two;articulatedSideRod=CreateArticulatedSideRod();}
 internal Mesh Side=>articulatedSideRod;
 internal void Run(Transform d)=>ConfigureEngineDrive(d,new Mesh(),true);
 internal static bool Seat(Transform marker,Transform rod,Vector3 pin,Mesh readable)=>SeatBigEndMarker(marker,rod,pin,readable);
 }
}
"@
Set-Content (Join-Path $out 'ActualEngineSetup.cs') $actual
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><NoWarn>CS0649</NoWarn></PropertyGroup><ItemGroup><Compile Include="$out/ActorStubs.cs"/><Compile Include="$out/ActualEngineSetup.cs"/><Compile Include="$root/RearrangedS282/OilCupSeat.cs"/><Compile Include="$PSScriptRoot/Program.cs"/></ItemGroup></Project>
"@
$path=Join-Path $out 'AngusCups.csproj';Set-Content $path $project
& 'C:/Program Files/dotnet/dotnet.exe' run --project $path -c Release -- (Join-Path $root 'build/geometry-audit/coupling/native-rods.json') (Join-Path $out 'validation.json')
if($LASTEXITCODE-ne0){throw 'Angus cup regression failed'}
