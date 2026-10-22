param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\runtime-trial-1.22.0\erie-validation'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$fixture=Join-Path $PSScriptRoot 'evidence\steam-pipe-tests'
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup>
<ItemGroup><Compile Include="$PSScriptRoot\RearrangedS282\SteamPipeShape.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\SteamDuctMesh.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\SteamPipeNativeFit.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\SteamPipeLowerCollar.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\SteamPipePresentation.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\ErieTenderPresentation.cs"/><Compile Include="$fixture\ActorStubs.cs"/><Compile Include="$fixture\ErieProgram.cs"/></ItemGroup></Project>
"@
$projectPath=Join-Path $OutputDirectory 'Erie.Validation.csproj'
Set-Content -LiteralPath $projectPath -Value $project -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- (Join-Path $PSScriptRoot 'build\steam-pipes-1.18.0\original-assets.json') (Join-Path $OutputDirectory 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Erie presentation validation failed'}
