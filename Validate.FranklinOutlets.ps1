param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build/franklin-outlets'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
function Extract([string]$file,[string]$marker) {
 $source=Get-Content -LiteralPath (Join-Path $PSScriptRoot $file) -Raw
 $a=$source.IndexOf($marker,[StringComparison]::Ordinal);if($a -lt 0){throw $marker}
 $b=$source.IndexOf('{',$a);$depth=0
 for($i=$b;$i -lt $source.Length;$i++){
  if($source[$i] -eq '{'){$depth++};if($source[$i] -eq '}'){$depth--;if($depth -eq 0){return $source.Substring($a,$i-$a+1)}}
 };throw "Unclosed source: $marker"
}
$mapping=Extract 'RearrangedS282/WheelRearranger.Mallet.cs' 'internal bool TryMapArticulatedOutlet(Vector3 nativeOrigin, Vector3 nativeCentre, int engine, out Vector3 engineLocal)'
$outlet=Extract 'RearrangedS282/ArticulatedEffects.cs' 'internal Vector3 SecondaryOutletLocal(Vector3 carLocal, Secondary secondary, Vector3 centreCarLocal)'
$actual=@"
using System;using UnityEngine;
namespace RearrangedS282 {
 internal partial class WheelRearranger {
 $mapping
 }
 internal static partial class ArticulatedEffects {
  internal sealed class EffectsState:MonoBehaviour {
   internal TrainCar Car; private const float EngineSeparation=4.65f;
   $outlet
  }
 }
}
"@
Set-Content -LiteralPath (Join-Path $OutputDirectory 'ActualOutletMapping.cs') -Value $actual -Encoding utf8
$stubs=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'evidence/steam-pipe-tests/ActorStubs.cs') -Raw
$stubs=$stubs.Replace('public static Vector3 forward =>','public static Vector3 back => new(0,0,-1); public static Vector3 forward =>')
Set-Content -LiteralPath (Join-Path $OutputDirectory 'SceneStubs.cs') -Value $stubs -Encoding utf8
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup><ItemGroup><Compile Include="$OutputDirectory/ActualOutletMapping.cs"/><Compile Include="$PSScriptRoot/RearrangedS282/WheelRearranger.FranklinOutlets.cs"/><Compile Include="$PSScriptRoot/RearrangedS282/CylinderCasingShape.cs"/><Compile Include="$OutputDirectory/SceneStubs.cs"/><Compile Include="$PSScriptRoot/evidence/franklin-outlets/Program.cs"/></ItemGroup></Project>
"@
$path=Join-Path $OutputDirectory 'Outlets.csproj'
Set-Content -LiteralPath $path -Value $project -Encoding utf8
& 'C:/Program Files/dotnet/dotnet.exe' run --project $path -c Release -- (Join-Path $OutputDirectory 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Franklin outlet seating regression failed'}
