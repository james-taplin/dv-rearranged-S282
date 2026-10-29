param(
    [string]$SourcePath = (Join-Path $PSScriptRoot 'RearrangedS282\BigEndOiling.cs'),
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'build\oiling-ownership-hotfix\native-tests'),
    [switch]$SaveRegressionOnly
)
$ErrorActionPreference = 'Stop'
$managed = 'B:\SteamLibrary\steamapps\common\Derail Valley\DerailValley_Data\Managed'
$publicized = Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'RearrangedS282\obj\Release\net48\PublicizedAssemblies') -Recurse -File -Filter 'DV.Simulation.dll' | Select-Object -First 1 -ExpandProperty FullName
if (-not $publicized) { throw 'Build the native-reference runtime project first.' }
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$OutputDirectory = (Resolve-Path -LiteralPath $OutputDirectory).Path
$probe = Join-Path $PSScriptRoot 'evidence\oiling-ownership-tests'
$project = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><DefineConstants>B99_ALL_STANDARD</DefineConstants><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources><NoWarn>SYSLIB0050</NoWarn></PropertyGroup>
  <ItemGroup>
    <Compile Include="$SourcePath" />
    <Compile Include="$PSScriptRoot\RearrangedS282\S282Target.cs" />
    <Compile Include="$PSScriptRoot\RearrangedS282\BigEndOilingPlan.cs" />
    <Compile Include="$probe\Stubs.cs" /><Compile Include="$probe\NativeProbe.cs" />
    <Reference Include="DV.Simulation"><HintPath>$publicized</HintPath></Reference>
    <Reference Include="DV.Utils"><HintPath>$managed\DV.Utils.dll</HintPath></Reference>
    <Reference Include="UnityEngine"><HintPath>$managed\UnityEngine.dll</HintPath></Reference>
    <Reference Include="UnityEngine.CoreModule"><HintPath>$managed\UnityEngine.CoreModule.dll</HintPath></Reference>
    <Reference Include="Newtonsoft.Json"><HintPath>$managed\Newtonsoft.Json.dll</HintPath></Reference>
  </ItemGroup>
</Project>
"@
$projectPath = Join-Path $OutputDirectory 'OilingOwnership.Validation.csproj'
$project | Set-Content -LiteralPath $projectPath -Encoding utf8
$probeArgs = @((Join-Path $OutputDirectory 'validation.json'))
if ($SaveRegressionOnly) { $probeArgs += 'save-regression' }
$runOutput = & 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- @probeArgs 2>&1
if ($LASTEXITCODE -ne 0) { $runOutput | Write-Output; throw 'Native oiling ownership validation failed.' }
$report = Get-Content -LiteralPath (Join-Path $OutputDirectory 'validation.json') -Raw | ConvertFrom-Json
$report | Select-Object passed,checks,nativeOilingAndSaveCode,limitations | ConvertTo-Json
