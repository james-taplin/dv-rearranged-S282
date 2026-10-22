param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\prairie-casing-integration'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$fixture=Join-Path $PSScriptRoot 'evidence\cylinder-casing-tests'
$emitters=Join-Path $fixture 'native-prairie-emitters.json'
$fittings=Join-Path $fixture 'native-full-casing-fittings.json'
if(-not(Test-Path -LiteralPath $fittings)) {
    & 'C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe' (Join-Path $fixture 'extract_full_fittings.py')
    if($LASTEXITCODE -ne 0){throw 'Native fitting fixture extraction failed'}
}
if(-not(Test-Path -LiteralPath $emitters)) {
    & 'C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe' (Join-Path $fixture 'extract_prairie_emitters.py')
    if($LASTEXITCODE -ne 0){throw 'Native particle fixture extraction failed'}
}
$project=@"
<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup>
<ItemGroup><Compile Include="$PSScriptRoot\RearrangedS282\CylinderCasingShape.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\WheelRearranger.CylinderCasings.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\WheelRearranger.Walschaerts.cs"/><Compile Include="$PSScriptRoot\evidence\pacific-hudson-valve-gear\Stubs.cs"/><Compile Include="$fixture\PrairieProgram.cs"/></ItemGroup>
</Project>
"@
$projectPath=Join-Path $OutputDirectory 'Prairie.Validation.csproj'
$report=Join-Path $OutputDirectory 'validation.json'
Set-Content -LiteralPath $projectPath -Value $project -Encoding UTF8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- (Join-Path $fixture 'native-casings.json') (Join-Path $PSScriptRoot 'evidence\pacific-hudson-valve-gear\native-animation.json') $report $emitters $fittings
if($LASTEXITCODE -ne 0){throw 'Prairie casing integration failed'}
$result=Get-Content -LiteralPath $report -Raw|ConvertFrom-Json
$result|Add-Member sourceHashes @(Get-FileHash (Join-Path $PSScriptRoot 'RearrangedS282\CylinderCasingShape.cs'),(Join-Path $PSScriptRoot 'RearrangedS282\WheelRearranger.CylinderCasings.cs'),(Join-Path $PSScriptRoot 'RearrangedS282\WheelRearranger.Walschaerts.cs')|Select-Object Path,Hash)
$result|ConvertTo-Json -Depth 5|Set-Content -LiteralPath $report -Encoding UTF8
Get-Content -LiteralPath $report -Raw
