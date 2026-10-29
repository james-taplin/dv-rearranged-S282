param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build/tender-effect-cleanup'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
function Extract([string]$source,[string]$marker) {
 $start=$source.IndexOf($marker,[StringComparison]::Ordinal);if($start -lt 0){throw $marker}
 $opening=$source.IndexOf('{',$start);$depth=0
 for($i=$opening;$i -lt $source.Length;$i++){
  if($source[$i] -eq '{'){$depth++};if($source[$i] -eq '}'){$depth--;if($depth -eq 0){return $source.Substring($start,$i-$start+1)}}
 };throw "Unclosed source: $marker"
}
$effects=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RearrangedS282/ArticulatedEffects.cs') -Raw
$presentation=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RearrangedS282/ErieTenderPresentation.cs') -Raw
$tender=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RearrangedS282/WheelRearranger.TenderEngine.cs') -Raw
$actual=@"
using System;using System.Linq;using UnityEngine;using Object=UnityEngine.Object;
namespace RearrangedS282 {
internal static partial class ArticulatedEffects {
private static void Safe(Action action)=>action();
$(Extract $effects 'private static void DestroyCopy')
internal static void RemoveCopy(GameObject copy)=>DestroyCopy(copy);
}
internal sealed partial class ErieTenderPresentation {
$(Extract $presentation 'internal void Release()')
$(Extract $presentation 'private void OnDisable()')
$(Extract $presentation 'private void OnDestroy()')
internal void Disable()=>OnDisable(); internal void Destroyed()=>OnDestroy();
}
internal partial class WheelRearranger {
$(Extract $tender 'private void DetachTenderEngine()')
internal void Detach()=>DetachTenderEngine();
}
}
"@
Set-Content -LiteralPath (Join-Path $OutputDirectory 'ActualCleanup.cs') -Value $actual -Encoding utf8
$fixture=Join-Path $PSScriptRoot 'evidence/tender-effect-cleanup'
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup><ItemGroup><Compile Include="$OutputDirectory/ActualCleanup.cs"/><Compile Include="$fixture/Program.cs"/></ItemGroup></Project>
"@
$projectPath=Join-Path $OutputDirectory 'Cleanup.csproj'
Set-Content -LiteralPath $projectPath -Value $project -Encoding utf8
& 'C:/Program Files/dotnet/dotnet.exe' run --project $projectPath -c Release -- (Join-Path $OutputDirectory 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Tender effect cleanup regression failed'}
