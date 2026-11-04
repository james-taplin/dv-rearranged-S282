param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\renderer-lifetime-validation'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$out=(Resolve-Path -LiteralPath $OutputDirectory).Path
$native='B:\SteamLibrary\steamapps\common\Derail Valley\DerailValley_Data\Managed\Assembly-CSharp.dll'
$text=(& 'C:\Users\james\.dotnet\tools\ilspycmd.exe' -t ExplosionModelHandler $native) -join "`n"
if($LASTEXITCODE -ne 0){throw 'Native explosion handler extraction failed'}
$text.Replace('private ','internal ') | Set-Content -LiteralPath (Join-Path $out 'NativeExplosion.cs') -Encoding utf8
$source=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'RearrangedS282\FranklinPinPresentation.cs'))
$start=$source.IndexOf('private sealed class Lease:IDisposable',[StringComparison]::Ordinal)
if($start -lt 0){throw 'Franklin pin lease not found'}
$open=$source.IndexOf('{',$start);$depth=1;$end=$open+1
while($depth -gt 0 -and $end -lt $source.Length){if($source[$end] -eq '{'){$depth++};if($source[$end] -eq '}'){$depth--};$end++}
if($depth -ne 0){throw 'Unbalanced Franklin lease'}
$lease=$source.Substring($start,$end-$start)
('using System; using System.Collections.Generic; using System.Linq; using UnityEngine; using Object=UnityEngine.Object; namespace RearrangedS282 { internal static class FranklinPinPresentation {'+$lease+'}}') | Set-Content -LiteralPath (Join-Path $out 'ActualLease.cs') -Encoding utf8
$fixture=Join-Path $PSScriptRoot 'evidence\renderer-lifetime-tests'
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup><ItemGroup>
<Compile Include="$PSScriptRoot\RearrangedS282\B99ExplosionMaterials.cs"/><Compile Include="$fixture\*.cs"/><Compile Include="$out\NativeExplosion.cs"/><Compile Include="$out\ActualLease.cs"/>
</ItemGroup></Project>
"@
$path=Join-Path $out 'RendererLifetime.csproj';$project|Set-Content -LiteralPath $path -Encoding utf8
foreach($case in @('native','sameframe')){& 'C:\Program Files\dotnet\dotnet.exe' run --project $path -c Release -- $case;if($LASTEXITCODE -ne 0){throw "Renderer lifetime case failed: $case"}}
