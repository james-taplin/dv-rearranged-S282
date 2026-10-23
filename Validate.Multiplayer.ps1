param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'build\multiplayer-validation'),
    [string]$GamePath = 'B:\SteamLibrary\steamapps\common\Derail Valley'
)
$ErrorActionPreference = 'Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory = (Resolve-Path -LiteralPath $OutputDirectory).Path
$fixture = Join-Path $PSScriptRoot 'evidence\multiplayer-tests'
$project = @"
<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup>
<ItemGroup><Compile Include="$PSScriptRoot\RearrangedS282.Multiplayer\Bridge.cs"/><Compile Include="$PSScriptRoot\RearrangedS282.Multiplayer\Packets.cs"/><Compile Include="$fixture\BridgeStubs.cs"/><Compile Include="$fixture\BridgeProgram.cs"/></ItemGroup>
</Project>
"@
$projectPath = Join-Path $OutputDirectory 'Bridge.Validation.csproj'
$bridgeReport = Join-Path $OutputDirectory 'bridge-validation.json'
Set-Content -LiteralPath $projectPath -Value $project -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- $bridgeReport
if ($LASTEXITCODE -ne 0) { throw 'Multiplayer bridge/packet validation failed.' }
Get-Content -LiteralPath $bridgeReport -Raw
$managed = Join-Path $GamePath 'DerailValley_Data\Managed'
$coreProject = @"
<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD;B99_STRUCTURAL_DIAGNOSTICS</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources><NoWarn>1701;CS0649</NoWarn></PropertyGroup>
<ItemGroup><Compile Include="$PSScriptRoot\RearrangedS282\MultiplayerSync.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\Settings.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\WheelArrangement.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\PoweredTenderPolicy.cs"/><Compile Include="$fixture\CoreStubs.cs"/><Compile Include="$fixture\CoreProgram.cs"/></ItemGroup>
<ItemGroup><Reference Include="Newtonsoft.Json"><HintPath>$managed\Newtonsoft.Json.dll</HintPath></Reference></ItemGroup>
</Project>
"@
$coreProjectPath = Join-Path $OutputDirectory 'Core.Validation.csproj'
$coreReport = Join-Path $OutputDirectory 'core-validation.json'
Set-Content -LiteralPath $coreProjectPath -Value $coreProject -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $coreProjectPath -c Release -- $coreReport
if ($LASTEXITCODE -ne 0) { throw 'Multiplayer core policy/revision/settings validation failed.' }
Get-Content -LiteralPath $coreReport -Raw
