param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'build\slip-integration\probe'),
    [string]$ManagedPath = 'B:\SteamLibrary\steamapps\common\Derail Valley\DerailValley_Data\Managed'
)
$ErrorActionPreference = 'Stop'
$sourcePath = Join-Path $PSScriptRoot 'RearrangedS282\IndependentDrive.cs'
$source = Get-Content -LiteralPath $sourcePath -Raw
function Get-ProductionMethod([string]$signature) {
    $start = $source.IndexOf($signature, [StringComparison]::Ordinal)
    if ($start -lt 0) { throw "Missing production method: $signature" }
    $open = $source.IndexOf('{', $start)
    $depth = 0
    for ($end = $open; $end -lt $source.Length; $end++) {
        if ($source[$end] -eq '{') { $depth++ }
        if ($source[$end] -eq '}') { $depth--; if ($depth -eq 0) { break } }
    }
    if ($depth -ne 0) { throw "Unbalanced production method: $signature" }
    return $source.Substring($start, $end - $start + 1)
}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$outputPath = (Resolve-Path -LiteralPath $OutputDirectory).Path
$methodsPath = Join-Path $outputPath 'ActualDriveMethods.cs'
$rpm = Get-ProductionMethod 'void PublishRpm(float delta)'
$force = Get-ProductionMethod 'bool ApplyForce(DrivingForce original)'
$prepare = Get-ProductionMethod 'void PrepareCore()'
$hash = (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash
@"
// Generated verbatim from IndependentDrive.cs. Source SHA256: $hash
using System;
using System.Linq;
using DV;
using DV.Utils;
using DV.Simulation.Cars;
using DV.ECS.Train;
using Unity.Transforms;
using UnityEngine;
namespace RearrangedS282 {
    internal sealed partial class ProductionDriveBoundary {
        internal const string SourceSha256 = "$hash";
        $rpm
        $force
        $prepare
    }
}
"@ | Set-Content -LiteralPath $methodsPath -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' build (Join-Path $PSScriptRoot 'evidence\slip-integration\NativeSlipIntegrationProbe.csproj') --configuration Release --output $outputPath --ignore-failed-sources "-p:ManagedPath=$ManagedPath" "-p:ExtractedDriveMethods=$methodsPath"
if ($LASTEXITCODE -ne 0) { throw 'Native slip integration probe compilation failed.' }
Write-Output (Join-Path $outputPath 'NativeSlipIntegrationProbe.dll')
