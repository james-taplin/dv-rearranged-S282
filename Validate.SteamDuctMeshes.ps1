param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\steam-duct-mesh-validation'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup>
<ItemGroup><Compile Include="$PSScriptRoot\RearrangedS282\SteamDuctMesh.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\SteamPipeNativeFit.cs"/><Compile Include="$PSScriptRoot\evidence\steam-duct-tests\Stubs.cs"/><Compile Include="$PSScriptRoot\evidence\steam-duct-tests\Program.cs"/></ItemGroup></Project>
"@
$projectPath=Join-Path $OutputDirectory 'SteamDuctMeshes.Validation.csproj'
Set-Content -LiteralPath $projectPath -Value $project -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- $OutputDirectory (Join-Path $PSScriptRoot 'build\steam-pipes-1.18.0\original-assets.json') (Join-Path $PSScriptRoot 'evidence\cylinder-casing-tests\native-full-casing-fittings.json') 'B:\SteamLibrary\steamapps\common\Derail Valley\Mods\LocoMeshSplitter\assets\s282_locomotive_body.obj'
if($LASTEXITCODE -ne 0){throw 'Steam duct geometry validation failed'}
