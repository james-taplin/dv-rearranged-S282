param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'build\independent-multiplayer-validation'))
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$run = Join-Path $OutputDirectory ([DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $run -Force | Out-Null
$run = (Resolve-Path -LiteralPath $run).Path
$managed = 'B:\SteamLibrary\steamapps\common\Derail Valley\DerailValley_Data\Managed'
$project = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><DefineConstants>B99_ALL_STANDARD</DefineConstants><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup>
  <ItemGroup>
    <Compile Include="$root\RearrangedS282\IndependentDriveReplication.cs" />
    <Compile Include="$root\evidence\articulated-simulation-tests\GraphStubs.cs" />
    <Compile Include="$root\evidence\independent-multiplayer-tests\PortProbe.cs" />
    <Reference Include="Newtonsoft.Json"><HintPath>$managed\Newtonsoft.Json.dll</HintPath></Reference>
  </ItemGroup>
</Project>
"@
$projectPath = Join-Path $run 'NativePortAdapter.csproj'
$project | Set-Content -LiteralPath $projectPath -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath --configuration Release -- $run
if ($LASTEXITCODE -ne 0) { throw 'Production independent-drive native port validation failed.' }
Get-Content -LiteralPath (Join-Path $run 'native-port-adapter.json') -Raw
$driveProject = $project.Replace('<Compile Include="' + $root + '\evidence\articulated-simulation-tests\GraphStubs.cs" />', '<Compile Include="' + $root + '\evidence\independent-multiplayer-tests\DriveStubs.cs" />').Replace('<Compile Include="' + $root + '\evidence\independent-multiplayer-tests\PortProbe.cs" />', @"
<Compile Include="$root\evidence\independent-multiplayer-tests\DriveProbe.cs" />
    <Compile Include="$root\RearrangedS282\IndependentDrive.cs" />
    <Compile Include="$root\RearrangedS282\GroupForceRouting.cs" />
"@)
$drivePath = Join-Path $run 'DriveAuthority.csproj'
$driveProject | Set-Content -LiteralPath $drivePath -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $drivePath --configuration Release -- $run
if ($LASTEXITCODE -ne 0) { throw 'Production independent-drive authority validation failed.' }
Get-Content -LiteralPath (Join-Path $run 'drive-authority.json') -Raw
