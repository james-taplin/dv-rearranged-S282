param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build/runtime-trial-1.23.1/structural-matrix'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$source=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RearrangedS282/Stage2RuntimeChecks.cs') -Raw
$a=$source.IndexOf('        internal static (int Id, bool Tender)[] StructuralCases()')
$b=$source.IndexOf(';', $a)+1
$cases=$source.Substring($a,$b-$a)
$a=$source.IndexOf('        internal static void ValidatePresentation(')
$b=$source.IndexOf('{',$a);$depth=0
for($i=$b;$i -lt $source.Length;$i++){if($source[$i] -eq '{'){$depth++};if($source[$i] -eq '}'){$depth--;if($depth -eq 0){break}}}
$validator=$source.Substring($a,$i-$a+1)
$a=$source.IndexOf('        internal static void ValidateOilCupPresentation(')
$b=$source.IndexOf('{',$a);$depth=0
for($i=$b;$i -lt $source.Length;$i++){if($source[$i] -eq '{'){$depth++};if($source[$i] -eq '}'){$depth--;if($depth -eq 0){break}}}
$oilValidator=$source.Substring($a,$i-$a+1)
$a=$source.IndexOf('        internal static string FramePiecePrefix(')
$b=$source.IndexOf('{',$a);$depth=0
for($i=$b;$i -lt $source.Length;$i++){if($source[$i] -eq '{'){$depth++};if($source[$i] -eq '}'){$depth--;if($depth -eq 0){break}}}
$framePrefix=$source.Substring($a,$i-$a+1)
if(-not $source.Contains('f.name.StartsWith(FramePiecePrefix(id, label), StringComparison.Ordinal)')){throw 'Presentation capture no longer uses the tested frame prefix'}
$actual="using System;using System.Linq;using Newtonsoft.Json.Linq;namespace RearrangedS282 { internal static class Stage2RuntimeChecks {`n$cases`n$validator`n$oilValidator`n$framePrefix`n} internal static class ArticulatedSimulation {internal static bool IsTriplexId(int id)=>WheelArrangement.LocomotiveWheels(id).Length==5;} }"
Set-Content -LiteralPath (Join-Path $OutputDirectory 'ActualStructuralMatrix.cs') -Value $actual -Encoding utf8
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup><ItemGroup><Compile Include="$OutputDirectory/ActualStructuralMatrix.cs"/><Compile Include="$PSScriptRoot/RearrangedS282/WheelArrangement.cs"/><Compile Include="$PSScriptRoot/RearrangedS282/PoweredTenderPolicy.cs"/><Compile Include="$PSScriptRoot/evidence/structural-matrix-tests/Program.cs"/><Reference Include="Newtonsoft.Json"><HintPath>B:/SteamLibrary/steamapps/common/Derail Valley/DerailValley_Data/Managed/Newtonsoft.Json.dll</HintPath></Reference></ItemGroup></Project>
"@
$path=Join-Path $OutputDirectory 'StructuralMatrix.csproj';Set-Content -LiteralPath $path -Value $project -Encoding utf8
& 'C:/Program Files/dotnet/dotnet.exe' run --project $path -c Release -- (Join-Path $OutputDirectory 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Structural matrix/pass-fail regression failed'}
