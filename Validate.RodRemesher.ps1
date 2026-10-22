param(
    [string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\rod-remesher-validation'),
    [string]$NativeAssetsDirectory='B:\SteamLibrary\steamapps\common\Derail Valley\Mods\RearrangedS282\assets'
)
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$fixture=Join-Path $PSScriptRoot 'evidence\rod-remesh-tests'
$native=Join-Path $OutputDirectory 'native-rods.json'
& 'C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe' (Join-Path $fixture 'extract_native.py') --assets $NativeAssetsDirectory --output $native
if($LASTEXITCODE -ne 0){throw 'Native rod recipe fixture extraction failed'}
$project=@"
<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup>
<ItemGroup><Compile Include="$PSScriptRoot\RearrangedS282\RodRemesher.cs"/><Compile Include="$fixture\Stubs.cs"/><Compile Include="$fixture\Program.cs"/></ItemGroup>
</Project>
"@
$projectPath=Join-Path $OutputDirectory 'RodRemesher.Validation.csproj'
$report=Join-Path $OutputDirectory 'validation.json'
Set-Content -LiteralPath $projectPath -Value $project -Encoding UTF8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- $native $report
if($LASTEXITCODE -ne 0){throw 'Rod remesher validation failed'}
Get-Content -LiteralPath $report -Raw
