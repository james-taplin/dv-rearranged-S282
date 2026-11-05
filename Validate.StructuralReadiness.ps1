param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build/structural-readiness'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$out=(Resolve-Path -LiteralPath $OutputDirectory).Path
$source=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RearrangedS282/Stage2RuntimeChecks.cs') -Raw
function Extract([string]$marker) {
 $start=$source.IndexOf($marker,[StringComparison]::Ordinal)
 if($start -lt 0){throw "Missing method $marker"}
 $open=$source.IndexOf('{',$start);$depth=1;$end=$open+1
 while($depth -gt 0 -and $end -lt $source.Length){if($source[$end] -eq '{'){$depth++};if($source[$end] -eq '}'){$depth--};$end++}
 if($depth -ne 0){throw "Unbalanced method $marker"}
 $source.Substring($start,$end-$start)
}
$methods=@('internal static void Begin()','private static bool HasTestTender(','internal static string StructuralReadinessError(','internal static bool AbortForReadiness(') | ForEach-Object { Extract $_ }
$actual='using System;using System.Linq;using UnityEngine;using Newtonsoft.Json.Linq;namespace RearrangedS282 { internal static partial class Stage2RuntimeChecks {'+($methods -join "`n")+'}}'
$actual | Set-Content -LiteralPath (Join-Path $out 'ActualReadiness.cs') -Encoding utf8
$run=Extract 'private static IEnumerator RunAll('
# Boundary routing guards complement execution of the actual selector/abort methods.
if(($run.Split(@('AbortForReadiness(report, subject, control)'),[StringSplitOptions]::None).Length-1) -lt 6){throw 'Readiness must guard baseline, both loops and post-yield capture'}
if(-not $run.Contains('if (AbortForReadiness(report, subject, control)) break;')){throw 'Sweep must stop before converting an unloaded subject'}
if(-not $run.Contains('result["status"] = "aborted-readiness";')){throw 'Interrupted case needs an explicit status'}
if(-not $run.Contains('(bool?)report["aborted"] == true ? Array.Empty<int>()')){throw 'Aborted sweeps must skip the return sequence'}
if(-not $run.Contains('(bool?)report["aborted"] != true && results.Count == cases.Length')){throw 'Aborted sweeps must not pass'}
if($run.IndexOf('Final stock restoration refused.') -lt $run.IndexOf('row["error"] = (string)report["error"]; break;')){throw 'Final restoration must remain after both loops'}
$fixture=Join-Path $PSScriptRoot 'evidence/structural-readiness-tests/Program.cs'
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup><ItemGroup><Compile Include="$out/ActualReadiness.cs"/><Compile Include="$fixture"/><Reference Include="Newtonsoft.Json"><HintPath>B:/SteamLibrary/steamapps/common/Derail Valley/DerailValley_Data/Managed/Newtonsoft.Json.dll</HintPath></Reference></ItemGroup></Project>
"@
$path=Join-Path $out 'StructuralReadiness.csproj';$project|Set-Content -LiteralPath $path -Encoding utf8
& 'C:/Program Files/dotnet/dotnet.exe' run --project $path -c Release -- $out
if($LASTEXITCODE -ne 0){throw 'Structural readiness fixture failed'}
