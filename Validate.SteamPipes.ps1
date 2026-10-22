param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\steam-pipes-1.20.0\geometry-validation'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$fixture=Join-Path $PSScriptRoot 'evidence\steam-pipe-tests'
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup>
<ItemGroup><Compile Include="$PSScriptRoot\RearrangedS282\SteamPipeShape.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\CylinderCasingShape.cs"/><Compile Include="$fixture\Stubs.cs"/><Compile Include="$fixture\Program.cs"/></ItemGroup></Project>
"@
$projectPath=Join-Path $OutputDirectory 'SteamPipes.Validation.csproj'
Set-Content -LiteralPath $projectPath -Value $project -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- (Join-Path $PSScriptRoot 'build\steam-pipes-1.18.0\original-assets.json') (Join-Path $PSScriptRoot 'evidence\cylinder-casing-tests\native-casings.json') (Join-Path $PSScriptRoot 'evidence\cylinder-casing-tests\native-full-casing-fittings.json') (Join-Path $OutputDirectory 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Steam pipe geometry validation failed'}

$actorProject=$project.Replace('SteamPipeShape.cs','SteamPipeShape.cs').Replace('<Compile Include="'+$PSScriptRoot+'\RearrangedS282\CylinderCasingShape.cs"/>','<Compile Include="'+$PSScriptRoot+'\RearrangedS282\SteamPipePresentation.cs"/>').Replace('\Stubs.cs','\ActorStubs.cs').Replace('\Program.cs','\ActorProgram.cs')
$actorPath=Join-Path $OutputDirectory 'SteamPipes.Actor.csproj'
Set-Content -LiteralPath $actorPath -Value $actorProject -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $actorPath -c Release -- (Join-Path $PSScriptRoot 'build\steam-pipes-1.18.0\original-assets.json') (Join-Path $PSScriptRoot 'evidence\cylinder-casing-tests\native-full-casing-fittings.json') (Join-Path $OutputDirectory 'actor-validation.json')
if($LASTEXITCODE -ne 0){throw 'Steam pipe presentation validation failed'}
