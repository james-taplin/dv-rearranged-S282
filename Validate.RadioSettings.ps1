param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\radio-settings-validation'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$fixture=Join-Path $PSScriptRoot 'evidence\radio-settings-tests'
$project=@"
<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup>
<ItemGroup><Compile Include="$PSScriptRoot\RearrangedS282\Main.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\CommsRadioS282Rearranger.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\WheelArrangement.cs"/><Compile Include="$fixture\Stubs.cs"/><Compile Include="$fixture\Program.cs"/></ItemGroup>
</Project>
"@
$projectPath=Join-Path $OutputDirectory 'RadioSettings.Validation.csproj'
$report=Join-Path $OutputDirectory 'validation.json'
Set-Content -LiteralPath $projectPath -Value $project -Encoding UTF8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- $report
if($LASTEXITCODE -ne 0){throw 'Radio/settings integration validation failed'}
Get-Content -LiteralPath $report -Raw
