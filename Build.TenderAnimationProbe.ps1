param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'build/tender-animation-probe'),
    [string]$GamePath = 'B:\SteamLibrary\steamapps\common\Derail Valley',
    [switch]$LegacyLookup
)
$ErrorActionPreference = 'Stop'
$run = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Force -Path $run | Out-Null
$source = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'RearrangedS282/WheelRearranger.Mallet.cs'))
$start = $source.IndexOf('        private Transform CloneEngineDrive(')
$end = $source.IndexOf('        // One three-axle engine:', $start)
if ($start -lt 0 -or $end -le $start) { throw 'Production clone helper not found.' }
$call = if ($LegacyLookup) {
    'var clone=CloneEngineDrive(source, animator, name, parent, d=>{}); if(finalTender) clone.GetComponent<MalletAnimatorMirror>().CopySpeed=false; return clone;'
} else {
    'return CloneEngineDrive(source, animator, name, parent, d=>{}, copySpeed: !finalTender);'
}
$actual = 'using System; using UnityEngine; using Object=UnityEngine.Object; namespace RearrangedS282 { internal partial class WheelRearranger : MonoBehaviour {' +
    $source.Substring($start, $end-$start) +
    'internal Transform ProbeClone(Transform source,Animator animator,string name,Transform parent,bool finalTender){' + $call + '}}}'
[IO.File]::WriteAllText((Join-Path $run 'ActualTenderClone.cs'), $actual)
$managed = Join-Path $GamePath 'DerailValley_Data/Managed'
$project = @"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net48</TargetFramework><LangVersion>9.0</LangVersion><AssemblyName>TenderAnimationProbe</AssemblyName><DefineConstants>B99_ALL_STANDARD</DefineConstants><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup><ItemGroup>
<Compile Include="$run/ActualTenderClone.cs"/>
<Compile Include="$PSScriptRoot/evidence/tender-animation-tests/TenderFixtureBoundary.cs"/>
<Compile Include="$PSScriptRoot/evidence/tender-animation-tests/TenderAnimationProbe.cs"/>
<Compile Include="$PSScriptRoot/RearrangedS282/MalletAnimatorMirror.cs"/>
<Compile Include="$PSScriptRoot/RearrangedS282/PoweredWheelRotater.cs"/>
<Compile Include="$PSScriptRoot/RearrangedS282/TenderDriveAnimation.cs"/>
<Compile Include="$PSScriptRoot/RearrangedS282/DriveQuartering.cs"/>
<Reference Include="$managed/*.dll" Private="false"/>
</ItemGroup></Project>
"@
[IO.File]::WriteAllText((Join-Path $run 'TenderAnimationProbe.csproj'), $project)
& 'C:/Program Files/dotnet/dotnet.exe' build (Join-Path $run 'TenderAnimationProbe.csproj') -c Release -o (Join-Path $run 'bin') --ignore-failed-sources
if ($LASTEXITCODE -ne 0) { throw 'Tender native probe compilation failed.' }
