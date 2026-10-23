param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build/runtime-trial-1.23.0/compact-triplex'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$source=Get-Content (Join-Path $PSScriptRoot 'RearrangedS282/WheelRearranger.Triplex.cs') -Raw
function Extract([string]$marker){$a=$source.IndexOf($marker);if($a -lt 0){throw $marker};$b=$source.IndexOf('{',$a);$depth=0;for($i=$b;$i -lt $source.Length;$i++){if($source[$i] -eq '{'){$depth++};if($source[$i] -eq '}'){$depth--;if($depth -eq 0){return $source.Substring($a,$i-$a+1)}}};throw $marker}
$layout=(Extract 'private sealed class TriplexLayout').Replace('private sealed','internal sealed')
$measure=Extract 'private TriplexLayout MeasureTriplex'
$helpers=@"
using System;using UnityEngine;namespace RearrangedS282 { internal sealed class WheelRearranger {
private const float TriplexShortenNative=.45f,TriplexFrontExtra=.25f,TrucklessFrontExtra=.6f,TriplexEngineGap=.08f,TrucklessEngineGap=.25f,MalletTrailingRadius=.355f,MalletEngineGap=.1f;
private static readonly Vector3 TriplexOwnTrailingLimit=new(0,.36f,-3.7f);
private readonly Transform body=new GameObject("body").transform, rearBogie=new GameObject("rear").transform, secondLeftDriveWheel=new GameObject("driver2").transform;
private Transform MalletBody=>body;
internal WheelRearranger(){rearBogie.localPosition=new(0,0,3.2f);secondLeftDriveWheel.localPosition=new(0,.7121296f,6.3025618f);}
private float FranklinCylinderFront()=>10.085789f;
private static Matrix4x4 EngineMap(float offset,float scale)=>Matrix4x4.TRS(new(0,0,offset),Quaternion.identity,new(1,scale,scale));
$layout
$measure
internal TriplexLayout Inspect(bool trucks,bool two)=>MeasureTriplex(trucks,two);
} }
"@
Set-Content (Join-Path $OutputDirectory 'ActualLayout.cs') $helpers -Encoding utf8
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources><NoWarn>CS0649</NoWarn></PropertyGroup><ItemGroup><Compile Include="$OutputDirectory/ActualLayout.cs"/><Compile Include="$PSScriptRoot/evidence/steam-pipe-tests/ActorStubs.cs"/><Compile Include="$PSScriptRoot/evidence/compact-triplex-tests/Program.cs"/></ItemGroup></Project>
"@
$path=Join-Path $OutputDirectory 'CompactTriplex.csproj';Set-Content $path $project -Encoding utf8
& 'C:/Program Files/dotnet/dotnet.exe' run --project $path -c Release -- (Join-Path $OutputDirectory 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Compact triplex layout check failed'}
