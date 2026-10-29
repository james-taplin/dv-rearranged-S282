param(
    [string]$SourcePath = (Join-Path $PSScriptRoot 'RearrangedS282\ConventionalSpawn.cs'),
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'build\oiling-ownership-hotfix\spawn-tests')
)
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$fixture=Join-Path $PSScriptRoot 'evidence\spawn-ownership-tests'
$json='B:\SteamLibrary\steamapps\common\Derail Valley\DerailValley_Data\Managed\Newtonsoft.Json.dll'
$project=@"
<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources><NoWarn>CS0649</NoWarn></PropertyGroup>
<ItemGroup><Compile Include="$SourcePath"/><Compile Include="$PSScriptRoot\RearrangedS282\S282Target.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\S282SpawnScope.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\ConventionalArrangementState.cs"/><Compile Include="$fixture\*.cs"/><Reference Include="Newtonsoft.Json"><HintPath>$json</HintPath></Reference></ItemGroup>
</Project>
"@
$path=Join-Path $OutputDirectory 'SpawnOwnership.csproj'
$project | Set-Content -LiteralPath $path -Encoding utf8
$output=& 'C:\Program Files\dotnet\dotnet.exe' run --project $path -c Release -- (Join-Path $OutputDirectory 'validation.json') 2>&1
if ($LASTEXITCODE -ne 0) { $output; throw 'Spawn/save ownership validation failed.' }
Get-Content -LiteralPath (Join-Path $OutputDirectory 'validation.json') -Raw
