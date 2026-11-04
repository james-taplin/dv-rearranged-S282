param([string]$ProductionDirectory=(Join-Path $PSScriptRoot '..\..\build\hidden-linkages\integrated-bin'),
      [string]$OutputDirectory=(Join-Path $PSScriptRoot '..\..\build\hidden-linkages\native-importer'))
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$ProductionDirectory=[IO.Path]::GetFullPath($ProductionDirectory)
$OutputDirectory=[IO.Path]::GetFullPath($OutputDirectory)
$probe=Join-Path $root 'build\hidden-linkages\importer-bin'
New-Item -ItemType Directory -Path $probe -Force|Out-Null
$managed='B:\SteamLibrary\steamapps\common\Derail Valley\DerailValley_Data\Managed'
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net48</TargetFramework><LangVersion>9.0</LangVersion><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit></PropertyGroup><ItemGroup><Compile Include="$PSScriptRoot\ImporterProbe.cs"/><Reference Include="$managed\*.dll" Private="false"/><Reference Include="RearrangedS282" HintPath="$ProductionDirectory\RearrangedS282.dll" Private="true"/></ItemGroup></Project>
"@
$projectPath=Join-Path $probe 'ImporterProbe.csproj'
Set-Content -LiteralPath $projectPath -Value $project -Encoding UTF8
& 'C:\Program Files\dotnet\dotnet.exe' build $projectPath -c Release -o $probe
if($LASTEXITCODE -ne 0){throw 'Native importer probe compilation failed'}
@{geometry=(Join-Path $root 'build\franklin-runtime\geometry.json');exports=(Join-Path $root 'build\hidden-linkages\export-smoke');umm=(Join-Path $managed 'UnityModManager');splitter='B:\SteamLibrary\steamapps\common\Derail Valley\Mods\LocoMeshSplitter'}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $probe 'import-config.json') -Encoding UTF8
& (Join-Path $root 'Run.NativeRuntimeProbe.ps1') -ProbeDirectory $probe -AssemblyName ImporterProbe.dll -TypeName HiddenLinkageImporterProbe -OutputDirectory $OutputDirectory
Get-Content -LiteralPath (Join-Path $OutputDirectory 'result.json') -Raw
