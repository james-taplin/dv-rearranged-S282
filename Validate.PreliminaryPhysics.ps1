# Supply the validation.json from each passing offline geometry fixture:
# .\Validate.PreliminaryPhysics.ps1 -ScotchGeometryReceipt <path> -LittleBoyGeometryReceipt <path>
param(
 [string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\offline-validation\PreliminaryPhysics'),
 [string]$RuntimeCapture='B:\SteamLibrary\steamapps\common\Derail Valley\Mods\RearrangedS282\stage2-runtime-checks.json',
 [string]$NativeDefinition=(Join-Path $PSScriptRoot 'build\b99-power-inspection\ReciprocatingSteamEngineDefinition.json'),
 [string]$ScotchGeometryReceipt,
 [string]$LittleBoyGeometryReceipt
)
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$RuntimeCapture=(Resolve-Path -LiteralPath $RuntimeCapture).Path
$NativeDefinition=(Resolve-Path -LiteralPath $NativeDefinition).Path
if([string]::IsNullOrWhiteSpace($ScotchGeometryReceipt) -or [string]::IsNullOrWhiteSpace($LittleBoyGeometryReceipt)) {
 throw 'Pass -ScotchGeometryReceipt and -LittleBoyGeometryReceipt from the two passing offline geometry fixtures; historical game capture does not cover IDs 49/50.'
}
$ScotchGeometryReceipt=(Resolve-Path -LiteralPath $ScotchGeometryReceipt).Path
$LittleBoyGeometryReceipt=(Resolve-Path -LiteralPath $LittleBoyGeometryReceipt).Path
function Assert-ReceiptSource([string]$receiptPath,[string]$label,[string]$recorded,[string]$sourceName) {
 $path=Join-Path $PSScriptRoot "RearrangedS282\$sourceName"
 if([string]::IsNullOrWhiteSpace($recorded)) { throw "$label receipt '$receiptPath' is missing the recorded SHA-256 for $sourceName; rerun its offline geometry fixture." }
 $current=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
 if($recorded -ine $current) { throw "$label receipt '$receiptPath' is stale: $sourceName no longer matches its recorded SHA-256. Rerun its offline geometry fixture." }
}
$scotchGeometry=Get-Content -LiteralPath $ScotchGeometryReceipt -Raw | ConvertFrom-Json
$littleBoyGeometry=Get-Content -LiteralPath $LittleBoyGeometryReceipt -Raw | ConvertFrom-Json
Assert-ReceiptSource $ScotchGeometryReceipt 'Scotch Glider' $scotchGeometry.managedSourceSHA256 'WheelRearranger.ScotchGlider.cs'
$littleBoySources=[ordered]@{
 littleBoy='WheelRearranger.LittleBoy.cs'; mallet='WheelRearranger.Mallet.cs';
 wheelRearranger='WheelRearranger.cs'; dispatcher='WheelRearranger.Conventional.cs'; catalogue='WheelArrangement.cs'
}
foreach($key in $littleBoySources.Keys) {
 Assert-ReceiptSource $LittleBoyGeometryReceipt 'Little Boy' $littleBoyGeometry.provenance.sourceSHA256.$key $littleBoySources[$key]
}
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
& 'C:\Program Files\dotnet\dotnet.exe' run --project $path -c Release -- $NativeDefinition $RuntimeCapture $report $ScotchGeometryReceipt $LittleBoyGeometryReceipt
if($LASTEXITCODE -ne 0){throw 'Preliminary physics native-engine validation failed'}
$result=Get-Content -LiteralPath $report -Raw | ConvertFrom-Json
$result | Add-Member -NotePropertyName captureProvenance -NotePropertyValue ([ordered]@{
 version=$capture.version; build=$capture.build; path=$RuntimeCapture
 sha256=(Get-FileHash -LiteralPath $RuntimeCapture -Algorithm SHA256).Hash
 nativeDefinitionPath=$NativeDefinition; nativeDefinitionSHA256=$definitionHash
 note='Historical 0–48 captured radii only; hashes do not establish current radii. No gameplay performed by this fixture.'
})
$result | Add-Member -NotePropertyName offlineGeometryProvenance -NotePropertyValue @(
 [ordered]@{ arrangementId=49; path=$ScotchGeometryReceipt; sha256=(Get-FileHash -LiteralPath $ScotchGeometryReceipt -Algorithm SHA256).Hash },
 [ordered]@{ arrangementId=50; path=$LittleBoyGeometryReceipt; sha256=(Get-FileHash -LiteralPath $LittleBoyGeometryReceipt -Algorithm SHA256).Hash }
)
$sourceHashes=[ordered]@{}
foreach($name in @('WheelArrangement.cs','B99PhysicsState.cs','WheelRearranger.cs','WheelRearranger.Conventional.cs','WheelRearranger.Additional.cs','WheelRearranger.BigMallet.cs','WheelRearranger.FourteenCoupled.cs','WheelRearranger.Mallet.cs','WheelRearranger.LargeDriverClearance.cs','WheelRearranger.Triplex.cs','CylinderCalibration.cs','ConventionalPhysics.cs','TractiveEffortModel.cs')) {
 $sourceHashes[$name]=(Get-FileHash -LiteralPath (Join-Path $PSScriptRoot "RearrangedS282\$name") -Algorithm SHA256).Hash
}
$result | Add-Member -NotePropertyName sourceSHA256 -NotePropertyValue $sourceHashes
$result | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $report -Encoding utf8
