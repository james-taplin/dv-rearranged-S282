param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\geometry-audit\pipe-clearance'),[string]$DiagnosticsPath='B:\SteamLibrary\steamapps\common\Derail Valley\Mods\RearrangedS282\stage2-runtime-checks.json',[switch]$RecordedBuild)
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$fixture=Join-Path $PSScriptRoot 'evidence\steam-pipe-tests'
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup>
<ItemGroup><Compile Include="$PSScriptRoot\RearrangedS282\SteamPipeShape.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\SteamPipePresentation.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\CylinderCasingShape.cs"/><Compile Include="$fixture\ActorStubs.cs"/><Compile Include="$fixture\ClearanceProgram.cs"/></ItemGroup></Project>
"@
$projectPath=Join-Path $OutputDirectory 'PipeClearance.csproj'
Set-Content -LiteralPath $projectPath -Value $project -Encoding utf8
$mode=if($RecordedBuild){'recorded'}else{'candidate'}
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- (Join-Path $PSScriptRoot 'build\steam-pipes-1.18.0\original-assets.json') (Join-Path $PSScriptRoot 'evidence\cylinder-casing-tests\native-full-casing-fittings.json') $DiagnosticsPath (Join-Path $OutputDirectory 'pipe-meshes.json') $mode
if($LASTEXITCODE -ne 0){throw 'Recorded pipe geometry export failed'}
& 'C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe' (Join-Path $fixture 'check_clearance.py') $OutputDirectory
if($LASTEXITCODE -ne 0){throw 'Pipe clearance analysis failed'}
