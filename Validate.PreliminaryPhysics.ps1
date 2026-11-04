param(
 [string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\offline-validation\PreliminaryPhysics'),
 [string]$RuntimeCapture='B:\SteamLibrary\steamapps\common\Derail Valley\Mods\RearrangedS282\stage2-runtime-checks.json',
 [string]$NativeDefinition=(Join-Path $PSScriptRoot 'build\b99-power-inspection\ReciprocatingSteamEngineDefinition.json')
)
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$RuntimeCapture=(Resolve-Path -LiteralPath $RuntimeCapture).Path
$NativeDefinition=(Resolve-Path -LiteralPath $NativeDefinition).Path
$definitionHash=(Get-FileHash -LiteralPath $NativeDefinition -Algorithm SHA256).Hash
if($definitionHash -ne 'B38CAAA94EF4C45B8B81248E4DE1260B2DD4B23FD305CBD3669FEB56A04A7207') {
 throw 'Native engine input differs from the previously captured S282 definition; verify its provenance before changing the fixture baseline.'
}
$capture=Get-Content -LiteralPath $RuntimeCapture -Raw | ConvertFrom-Json
if(-not $capture.passed -or -not $capture.started -or $capture.arrangements.Count -ne $capture.expectedCases) {
 throw 'Runtime capture is incomplete or did not pass; supply a complete structural capture.'
}
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
$report=Join-Path $OutputDirectory 'validation.json'
& 'C:\Program Files\dotnet\dotnet.exe' run --project $path -c Release -- $NativeDefinition $RuntimeCapture $report
if($LASTEXITCODE -ne 0){throw 'Preliminary physics native-engine validation failed'}
$result=Get-Content -LiteralPath $report -Raw | ConvertFrom-Json
$result | Add-Member -NotePropertyName captureProvenance -NotePropertyValue ([ordered]@{
 version=$capture.version; build=$capture.build; path=$RuntimeCapture
 sha256=(Get-FileHash -LiteralPath $RuntimeCapture -Algorithm SHA256).Hash
 nativeDefinitionPath=$NativeDefinition; nativeDefinitionSHA256=$definitionHash
 note='Historical captured radii; obtain an updated capture when layout geometry or the catalogue changes. Hashes do not establish current radii. No gameplay performed by this fixture.'
})
$sourceHashes=[ordered]@{}
foreach($name in @('WheelArrangement.cs','B99PhysicsState.cs','WheelRearranger.cs','WheelRearranger.Conventional.cs','WheelRearranger.Additional.cs','WheelRearranger.BigMallet.cs','WheelRearranger.FourteenCoupled.cs','WheelRearranger.Mallet.cs','WheelRearranger.LargeDriverClearance.cs','WheelRearranger.Triplex.cs','CylinderCalibration.cs','ConventionalPhysics.cs','TractiveEffortModel.cs')) {
 $sourceHashes[$name]=(Get-FileHash -LiteralPath (Join-Path $PSScriptRoot "RearrangedS282\$name") -Algorithm SHA256).Hash
}
$result | Add-Member -NotePropertyName sourceSHA256 -NotePropertyValue $sourceHashes
$result | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $report -Encoding utf8
