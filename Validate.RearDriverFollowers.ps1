param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\runtime-trial-1.19.0\rear-driver-validation'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$text=Get-Content (Join-Path $PSScriptRoot 'RearrangedS282\WheelRearranger.Mallet.cs') -Raw
$start=$text.IndexOf('        private Transform CloneEngineDrive(')
$end=$text.IndexOf('        // One three-axle engine:', $start)
if($start -lt 0 -or $end -le $start){throw 'Actual clone method not found'}
$actual='using System;using UnityEngine;using Object=UnityEngine.Object;namespace RearrangedS282{internal partial class WheelRearranger{'+$text.Substring($start,$end-$start)+'}}'
Set-Content (Join-Path $OutputDirectory 'ActualClone.cs') $actual -Encoding utf8
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources><NoWarn>0649</NoWarn></PropertyGroup><ItemGroup>
<Compile Include="$OutputDirectory\ActualClone.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\PoweredWheelRotater.cs"/><Compile Include="$PSScriptRoot\evidence\rear-driver-tests\Stubs.cs"/><Compile Include="$PSScriptRoot\evidence\rear-driver-tests\Program.cs"/>
</ItemGroup></Project>
"@
$projectPath=Join-Path $OutputDirectory 'RearDrivers.csproj'
Set-Content $projectPath $project -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- (Join-Path $OutputDirectory 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Rear driver follower validation failed'}
