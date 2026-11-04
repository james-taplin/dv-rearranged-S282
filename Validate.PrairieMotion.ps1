param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\offline-validation\PrairieMotion'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$fixture=Join-Path $PSScriptRoot 'evidence\cylinder-casing-tests'
$project=@"
<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources><StartupObject>MotionHarness</StartupObject></PropertyGroup>
<ItemGroup><Compile Include="$PSScriptRoot\RearrangedS282\CylinderCasingShape.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\WheelRearranger.CylinderCasings.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\WheelRearranger.Walschaerts.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\WheelRearranger.PrairieSpacing.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\WheelRearranger.LargeDriverClearance.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\WheelRearranger.OilCupMounts.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\DriveRetarget.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\RodRemesher.cs"/><Compile Include="$PSScriptRoot\evidence\fourteen-spacing-tests\Stubs.cs"/><Compile Include="$fixture\MotionOwner.cs"/><Compile Include="$fixture\PrairieProgram.cs"/><Compile Include="$fixture\MotionCases.cs"/></ItemGroup>
</Project>
"@
$projectPath=Join-Path $OutputDirectory 'Motion.Validation.csproj';$report=Join-Path $OutputDirectory 'validation.json'
Set-Content -LiteralPath $projectPath -Value $project -Encoding UTF8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- (Join-Path $fixture 'native-casings.json') (Join-Path $PSScriptRoot 'evidence\pacific-hudson-valve-gear\native-animation.json') $report (Join-Path $fixture 'native-prairie-emitters.json') (Join-Path $fixture 'native-full-casing-fittings.json') (Join-Path $PSScriptRoot 'evidence\fourteen-spacing-tests\native-spacing.json')
if($LASTEXITCODE -ne 0){throw 'Prairie forward spacing and oil presentation failed'}
Get-Content -LiteralPath $report -Raw
