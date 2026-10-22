param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'build\native-steam-fittings-validation'),
    [string]$GamePath = 'B:\SteamLibrary\steamapps\common\Derail Valley',
    [string]$Python = 'C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe'
)
$ErrorActionPreference = 'Stop'
$fixture = Join-Path $PSScriptRoot 'evidence\steam-duct-tests'
foreach ($inputPath in @(
    (Join-Path $PSScriptRoot 'evidence\cylinder-casing-tests\native-full-casing-fittings.json'),
    (Join-Path $PSScriptRoot 'evidence\cylinder-casing-tests\native-casings.json'),
    (Join-Path $PSScriptRoot 'evidence\cylinder-casing-tests\LMS_LOD0.reference.txt'),
    (Join-Path $GamePath 'Mods\LocoMeshSplitter\assets\s282_locomotive_body.obj'),
    (Join-Path $GamePath 'Mods\LocoMeshSplitter\assets\s282_locomotive_body_LOD1.obj')
)) {
    if (!(Test-Path -LiteralPath $inputPath)) {
        throw "Missing local native fixture: $inputPath. See evidence/steam-duct-tests/README.md."
    }
}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory = (Resolve-Path -LiteralPath $OutputDirectory).Path
$compilePaths = @(
    (Join-Path $PSScriptRoot 'RearrangedS282\SteamPipeNativeFit.cs'),
    (Join-Path $PSScriptRoot 'RearrangedS282\SteamPipeLowerCollar.cs'),
    (Join-Path $PSScriptRoot 'RearrangedS282\CylinderCasingShape.cs'),
    (Join-Path $fixture 'Stubs.cs'),
    (Join-Path $fixture 'NativeFitProgram.cs')
)
$compileItems = ($compilePaths | ForEach-Object {
    '<Compile Include="' + [System.Security.SecurityElement]::Escape($_) + '"/>'
}) -join "`n"
$project = @"
<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup>
<ItemGroup>
$compileItems
</ItemGroup>
</Project>
"@
$projectPath = Join-Path $OutputDirectory 'NativeSteamFittings.Validation.csproj'
Set-Content -LiteralPath $projectPath -Value $project -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- $PSScriptRoot $OutputDirectory $GamePath
if ($LASTEXITCODE -ne 0) { throw 'Native steam fitting geometry validation failed.' }
& $Python (Join-Path $fixture 'check_native_collar_air.py') $OutputDirectory $GamePath
if ($LASTEXITCODE -ne 0) { throw 'Native lower collar air-pipe clearance validation failed.' }
