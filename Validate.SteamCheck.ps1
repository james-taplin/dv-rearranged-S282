param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\steam-pipes-1.20.0\check-validation'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$source=Get-Content (Join-Path $PSScriptRoot 'RearrangedS282\Stage2RuntimeChecks.cs') -Raw
$start=$source.IndexOf('        internal static void ValidateSteamConnections(')
$end=$source.IndexOf('        private static JObject CaptureFrontCouplingMounts', $start)
if($start -lt 0 -or $end -le $start){throw 'Actual pipe validator not found'}
$actual='using System;using Newtonsoft.Json.Linq;namespace RearrangedS282{internal static class Stage2RuntimeChecks{'+$source.Substring($start,$end-$start)+'}}'
Set-Content (Join-Path $OutputDirectory 'ActualSteamCheck.cs') $actual -Encoding utf8
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup><ItemGroup><Compile Include="$OutputDirectory\ActualSteamCheck.cs"/><Compile Include="$PSScriptRoot\evidence\steam-check-tests\Program.cs"/><Reference Include="Newtonsoft.Json"><HintPath>B:\SteamLibrary\steamapps\common\Derail Valley\DerailValley_Data\Managed\Newtonsoft.Json.dll</HintPath></Reference></ItemGroup></Project>
"@
$path=Join-Path $OutputDirectory 'SteamCheck.csproj'
Set-Content $path $project -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $path -c Release -- (Join-Path $OutputDirectory 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Structural pipe-validator regression failed'}
