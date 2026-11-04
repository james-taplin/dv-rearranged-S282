param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\offline-validation\PacificHudsonValveGear'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$source=Join-Path $PSScriptRoot 'RearrangedS282\WheelRearranger.Walschaerts.cs'
$spacing=Join-Path $PSScriptRoot 'RearrangedS282\WheelRearranger.PrairieSpacing.cs'
$spacingText=Get-Content -LiteralPath $spacing -Raw
$signature='private static Matrix4x4 CylinderDriveFrame(Transform drive)'
$start=$spacingText.IndexOf($signature,[StringComparison]::Ordinal)
if($start -lt 0){throw 'CylinderDriveFrame signature changed; update the fixture extraction.'}
$open=$spacingText.IndexOf('{',$start)
if($open -lt 0){throw 'CylinderDriveFrame has no method body.'}
$depth=0; $end=-1
for($i=$open; $i -lt $spacingText.Length; $i++) {
    if($spacingText[$i] -eq '{'){$depth++}
    elseif($spacingText[$i] -eq '}'){$depth--;if($depth -eq 0){$end=$i;break}}
}
if($end -lt 0){throw 'Could not extract CylinderDriveFrame.'}
$frame=Join-Path $OutputDirectory 'ActualCylinderDriveFrame.cs'
Set-Content -LiteralPath $frame -Value ('using UnityEngine; namespace RearrangedS282 { internal partial class WheelRearranger {' + $spacingText.Substring($start,$end-$start+1) + '} }') -Encoding utf8
$fixture=Join-Path $PSScriptRoot 'evidence\pacific-hudson-valve-gear'
$project=@"
<Project Sdk="Microsoft.NET.Sdk">
 <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup>
 <ItemGroup><Compile Include="$source"/><Compile Include="$frame"/><Compile Include="$fixture\Stubs.cs"/><Compile Include="$fixture\Program.cs"/></ItemGroup>
</Project>
"@
$projectPath=Join-Path $OutputDirectory 'PacificHudson.Validation.csproj'
Set-Content -LiteralPath $projectPath -Value $project -Encoding utf8
$report=Join-Path $OutputDirectory 'validation.json'
& 'C:/Program Files/dotnet/dotnet.exe' run --project $projectPath --configuration Release -- (Join-Path $fixture 'native-animation.json') $report
if($LASTEXITCODE -ne 0){throw 'Pacific/Hudson valve-gear validation failed.'}
$result=Get-Content -LiteralPath $report -Raw | ConvertFrom-Json
$result|Add-Member -NotePropertyName sourceSHA256 -NotePropertyValue (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
$result|Add-Member -NotePropertyName spacingSourceSHA256 -NotePropertyValue (Get-FileHash -LiteralPath $spacing -Algorithm SHA256).Hash
$result|ConvertTo-Json -Depth 8|Set-Content -LiteralPath $report -Encoding utf8
Get-Content -LiteralPath $report -Raw
