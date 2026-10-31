param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'build/franklin-runtime/lms-static-validation'),
    [string]$PayloadDirectory = (Join-Path $PSScriptRoot 'build/franklin-runtime'),
    [string]$GamePath = 'B:\SteamLibrary\steamapps\common\Derail Valley',
    [string]$PythonPath = 'C:\Users\james\Desktop\Derail Valley Mods\Claudes Place\.venv312\Scripts\python.exe',
    [string]$IlSpy = 'C:\Users\james\.dotnet\tools\ilspycmd.exe'
)
$ErrorActionPreference='Stop'
$run=[IO.Path]::GetFullPath($OutputDirectory)
$payload=[IO.Path]::GetFullPath($PayloadDirectory)
$buildBoundary=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot 'build')).TrimEnd([char]92)+[char]92
foreach($target in @($run,$payload)){
    if(-not $target.StartsWith($buildBoundary,[StringComparison]::OrdinalIgnoreCase)){throw 'Native LMS evidence must stay inside this repository build directory.'}
}
$lms=Join-Path $GamePath 'Mods/LocoMeshSplitter'
$privateCache=Join-Path $run 'LocoMeshSplitter'
New-Item -ItemType Directory -Path (Join-Path $privateCache 'assets') -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $lms 'LocoMeshSplitter.dll') -Destination $privateCache -Force
Copy-Item -LiteralPath (Join-Path $lms 'assets/s282_locomotive_body.obj') -Destination (Join-Path $privateCache 'assets') -Force
$managed=Join-Path $GamePath 'DerailValley_Data/Managed'
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net48</TargetFramework><LangVersion>9</LangVersion><AssemblyName>LmsNativeExport</AssemblyName><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup><ItemGroup><Compile Include="$PSScriptRoot/evidence/franklin-runtime/LmsStaticExport.cs"/><Reference Include="$managed/*.dll" Private="false"/><Reference Include="$managed/UnityModManager/0Harmony.dll"/></ItemGroup></Project>
"@
$projectPath=Join-Path $run 'LmsStaticExport.csproj'
[IO.File]::WriteAllText($projectPath,$project)
& 'C:/Program Files/dotnet/dotnet.exe' build $projectPath -c Release -o (Join-Path $run 'bin') --ignore-failed-sources
if($LASTEXITCODE -ne 0){throw 'Actual LMS native fixture compilation failed.'}
@{lms=$privateCache;managed=$managed} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $run 'bin/lms-config.json') -Encoding utf8
$nativeRun=Join-Path $run ('native-run-'+[DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fff'))
& (Join-Path $PSScriptRoot 'Run.NativeRuntimeProbe.ps1') -ProbeDirectory (Join-Path $run 'bin') -AssemblyName LmsNativeExport.dll -TypeName LmsNativeExport -OutputDirectory $nativeRun -GamePath $GamePath
& $PythonPath (Join-Path $PSScriptRoot 'evidence/franklin-runtime/extract_wals_static.py') --game $GamePath --output-directory $payload --ilspy $IlSpy --native-report (Join-Path $nativeRun 'result.json')
if($LASTEXITCODE -ne 0){throw 'Actual LMS mesh comparison/payload extraction failed.'}
Get-Content -LiteralPath (Join-Path $payload 'lms-native-comparison.json') -Raw
