param(
    [string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\steam-duct-presentation'),
    [string]$DiagnosticsPath='B:\SteamLibrary\steamapps\common\Derail Valley\Mods\RearrangedS282\stage2-runtime-checks.json',
    [switch]$Stress,
    [switch]$StaticOnly,
    [switch]$NativeLod1,
    [switch]$RecordedBuild
)
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$fixture=Join-Path $PSScriptRoot 'evidence\steam-pipe-tests'
$sources=@('SteamPipeShape.cs','SteamDuctMesh.cs','SteamPipeNativeFit.cs','SteamPipeLowerCollar.cs','SteamPipePresentation.cs','CylinderCasingShape.cs')
$compile=($sources|ForEach-Object {'<Compile Include="'+(Join-Path $PSScriptRoot ('RearrangedS282\'+$_))+'"/>'}) -join ''
$sourcePaths=@($sources|ForEach-Object {Join-Path $PSScriptRoot ('RearrangedS282\'+$_)})+@((Join-Path $fixture 'ActorStubs.cs'),(Join-Path $fixture 'ClearanceProgram.cs'))
$sourceReceipt=@($sourcePaths|ForEach-Object { [ordered]@{path=$_;sha256=(Get-FileHash -LiteralPath $_ -Algorithm SHA256).Hash} })
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup>
<ItemGroup>$compile<Compile Include="$fixture\ActorStubs.cs"/><Compile Include="$fixture\ClearanceProgram.cs"/></ItemGroup></Project>
"@
$projectPath=Join-Path $OutputDirectory 'DuctPresentation.csproj'
Set-Content -LiteralPath $projectPath -Value $project -Encoding utf8
$mode=if($RecordedBuild){'recorded'}else{'candidate'}
$arguments=@((Join-Path $PSScriptRoot 'build\steam-pipes-1.18.0\original-assets.json'),(Join-Path $PSScriptRoot 'evidence\cylinder-casing-tests\native-full-casing-fittings.json'),$DiagnosticsPath,(Join-Path $OutputDirectory 'pipe-meshes.json'),$mode)
if($Stress){$arguments+='stress'}
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- @arguments
if($LASTEXITCODE -ne 0){throw 'Production duct presentation export failed'}
foreach($entry in $sourceReceipt){if((Get-FileHash -LiteralPath $entry.path -Algorithm SHA256).Hash -ne $entry.sha256){throw ('Source changed during export compilation: '+$entry.path)}}
[ordered]@{sources=$sourceReceipt;assemblySHA256=(Get-FileHash -LiteralPath (Join-Path $OutputDirectory 'bin\Release\net8.0\DuctPresentation.dll') -Algorithm SHA256).Hash;diagnosticsSHA256=(Get-FileHash -LiteralPath $DiagnosticsPath -Algorithm SHA256).Hash}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $OutputDirectory 'source-receipt.json') -Encoding utf8
$auditArguments=@((Join-Path $fixture 'check_clearance.py'),$OutputDirectory)
if($StaticOnly){$auditArguments+='--static-only'}
if($NativeLod1){$auditArguments+='--native-lod1'}
& 'C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe' @auditArguments
if($LASTEXITCODE -ne 0){throw 'Native duct surface-crossing audit failed'}
