param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\quartering-hotfix\tests'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD;B99_STRUCTURAL_DIAGNOSTICS</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources><NoWarn>CS0649</NoWarn></PropertyGroup><ItemGroup>
<Compile Include="$PSScriptRoot/RearrangedS282/DriveQuartering.cs"/>
<Compile Include="$PSScriptRoot/RearrangedS282/WheelRearranger.Quartering.cs"/>
<Compile Include="$PSScriptRoot/RearrangedS282/TenderDriveAnimation.cs"/>
<Compile Include="$PSScriptRoot/RearrangedS282/WheelArrangement.cs"/>
<Compile Include="$PSScriptRoot/RearrangedS282/PoweredTenderPolicy.cs"/>
<Compile Include="$PSScriptRoot/evidence/quartering-tests/Stubs.cs"/>
<Compile Include="$PSScriptRoot/evidence/quartering-tests/Program.cs"/>
<Compile Include="$PSScriptRoot/evidence/franklin-audit/LinkageReport.cs"/>
<Reference Include="Newtonsoft.Json"><HintPath>B:/SteamLibrary/steamapps/common/Derail Valley/DerailValley_Data/Managed/Newtonsoft.Json.dll</HintPath></Reference>
</ItemGroup></Project>
"@
$path=Join-Path $OutputDirectory 'Quartering.csproj'
Set-Content -LiteralPath $path -Value $project -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $path -c Release -- (Join-Path $OutputDirectory 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Quartering lifecycle/diagnostic regression failed'}
