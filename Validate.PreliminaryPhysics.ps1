param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\runtime-trial-1.19.0\physics-validation'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$managed='B:\SteamLibrary\steamapps\common\Derail Valley\DerailValley_Data\Managed'
$publicized=Get-ChildItem (Join-Path $PSScriptRoot 'RearrangedS282\obj\Release\net48\PublicizedAssemblies') -Recurse -File -Filter DV.Simulation.dll | Select-Object -First 1 -ExpandProperty FullName
$probe=Get-Content (Join-Path $PSScriptRoot 'evidence\articulated-simulation-tests\NativeProbe.cs') -Raw
$setup=$probe.Substring(0,$probe.IndexOf('    internal static int Main(')).Replace('internal static class NativeProbe','internal static partial class Program')+'}'
Set-Content (Join-Path $OutputDirectory 'ActualNativeSetup.cs') $setup -Encoding utf8
$source=Get-Content (Join-Path $PSScriptRoot 'RearrangedS282\ArticulatedSimulation.cs') -Raw
$start=$source.IndexOf('    internal sealed class ArticulatedRearEngineDefinition')
$gate='using LocoSim.Definitions;using LocoSim.Implementations;namespace RearrangedS282{'+$source.Substring($start,$source.LastIndexOf('#endif')-$start)
Set-Content (Join-Path $OutputDirectory 'ActualEngineGate.cs') $gate -Encoding utf8
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources><NoWarn>SYSLIB0050</NoWarn></PropertyGroup><ItemGroup>
<Compile Include="$OutputDirectory\ActualNativeSetup.cs"/><Compile Include="$OutputDirectory\ActualEngineGate.cs"/>
<Compile Include="$PSScriptRoot\evidence\preliminary-physics-tests\Program.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\CylinderCalibration.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\TractiveEffortModel.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\ConventionalPhysics.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\WheelArrangement.cs"/>
<Reference Include="DV.Simulation"><HintPath>$publicized</HintPath></Reference><Reference Include="DV.Utils"><HintPath>$managed\DV.Utils.dll</HintPath></Reference><Reference Include="UnityEngine"><HintPath>$managed\UnityEngine.dll</HintPath></Reference><Reference Include="UnityEngine.CoreModule"><HintPath>$managed\UnityEngine.CoreModule.dll</HintPath></Reference><Reference Include="Newtonsoft.Json"><HintPath>$managed\Newtonsoft.Json.dll</HintPath></Reference>
</ItemGroup></Project>
"@
$path=Join-Path $OutputDirectory 'PreliminaryPhysics.csproj'
Set-Content $path $project -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $path -c Release -- (Join-Path $PSScriptRoot 'build\b99-power-inspection\ReciprocatingSteamEngineDefinition.json') 'B:\SteamLibrary\steamapps\common\Derail Valley\Mods\RearrangedS282\stage2-runtime-checks.json' (Join-Path $OutputDirectory 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Preliminary physics native-engine validation failed'}
