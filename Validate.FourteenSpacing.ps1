param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\offline-validation\FourteenSpacing'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$fixture=Join-Path $PSScriptRoot 'evidence\fourteen-spacing-tests'
$native=Join-Path $fixture 'native-spacing.json'
if(-not(Test-Path -LiteralPath $native)) {
 & 'C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe' (Join-Path $fixture 'extract_native.py')
 if($LASTEXITCODE -ne 0){throw 'Native 4-14-4 fixture extraction failed'}
}
function Extract([string]$text,[string]$marker) {
 $start=$text.IndexOf($marker,[StringComparison]::Ordinal)
 if($start -lt 0){throw "Missing actual source member $marker"}
 $open=$text.IndexOf('{',$start);$depth=1;$end=$open+1
 while($depth -gt 0 -and $end -lt $text.Length){if($text[$end] -eq '{'){$depth++};if($text[$end] -eq '}'){$depth--};$end++}
 if($depth -ne 0){throw 'Unbalanced actual source member'}
 return $text.Substring($start,$end-$start)
}
$source=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RearrangedS282\WheelRearranger.FourteenCoupled.cs') -Raw
# This 1.8.1 commit specifies the original 0.25 m shift. The later layout
# added a further 0.25 m; do not replace this with the changing HEAD source.
$baselineRevision='f31e01f59bfdd8eab179a00c03111269eb59e04f'
$baseline=(& git -c "safe.directory=$($PSScriptRoot.Replace('\','/'))" show "${baselineRevision}:RearrangedS282/WheelRearranger.FourteenCoupled.cs" 2>$null) -join "`n"
if($LASTEXITCODE -ne 0 -or -not $baseline){throw "Missing historical 4-14-4 reference commit $baselineRevision; fetch the repository history before running this fixture."}
if($baseline -notmatch 'FourteenDriverShift\s*=\s*\.25f\s*;'){throw "Historical 4-14-4 reference $baselineRevision does not have the expected original 0.25 m shift."}
$mapSource=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RearrangedS282\WheelRearranger.Mallet.cs') -Raw
$map=[regex]::Match($mapSource,'private static Matrix4x4 EngineMap\(.*?;', [Text.RegularExpressions.RegexOptions]::Singleline).Value
if(-not $map){throw 'Missing actual EngineMap helper'}
$generated=''
foreach($case in @(@('Baseline',$baseline),@('Candidate',$source))) {
 $which=$case[0];$text=$case[1]
 $constants=@('FourteenTruckSpacing','FourteenDriverShift','FourteenTrailingLimit') | ForEach-Object {
  $pattern='private (?:const float|static readonly Vector3) '+$_+'\s*=.*?;'
  $match=[regex]::Match($text,$pattern).Value
  if(-not $match){throw "Missing actual source constant $_"};$match
 }
 $layout=Extract $text 'private sealed class FourteenLayout'
 $measure=Extract $text 'private FourteenLayout Measure4144()'
 $cup=if($which -eq 'Candidate'){(Extract $text 'private static void Move4144CrossheadCup(')+"`ninternal static void MoveCup(Transform drive, Vector3 shift) => Move4144CrossheadCup(drive,shift);"}else{''}
 $generated+=@"
namespace SpacingFixture.$which {
using UnityEngine;
internal sealed class WheelRearranger : SpacingFixture.RigBase {
internal WheelRearranger(System.Text.Json.JsonElement data,System.Text.Json.JsonElement animation):base(data,animation){}
$($constants -join "`n")
$map
$layout
$measure
$cup
internal SpacingFixture.Snapshot Capture() { var l=Measure4144();return new SpacingFixture.Snapshot {Scale=l.Scale,Offset=l.Offset,DriveOffset=l.DriveOffset,Radius=l.Radius,Lead1=l.Lead1,Lead2=l.Lead2,Trail1=l.Trail1,Trail2=l.Trail2,Drivers=l.Drivers}; }
}}
"@
}
$actual=Join-Path $OutputDirectory 'ActualFourteen.cs'
Set-Content -LiteralPath $actual -Value ("using System;`nusing System.Linq;`n"+$generated) -Encoding UTF8
$project=@"
<Project Sdk="Microsoft.NET.Sdk">
 <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup>
 <ItemGroup><Compile Include="$actual"/><Compile Include="$PSScriptRoot\RearrangedS282\DriveRetarget.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\RodRemesher.cs"/><Compile Include="$fixture\Stubs.cs"/><Compile Include="$fixture\Program.cs"/></ItemGroup>
</Project>
"@
$projectPath=Join-Path $OutputDirectory 'FourteenSpacing.Validation.csproj'
Set-Content -LiteralPath $projectPath -Value $project -Encoding UTF8
$report=Join-Path $OutputDirectory 'validation.json'
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- $native (Join-Path $PSScriptRoot 'evidence\pacific-hudson-valve-gear\native-animation.json') $report
if($LASTEXITCODE -ne 0){throw '4-14-4 spacing/native-pose validation failed'}
$result=Get-Content -LiteralPath $report -Raw|ConvertFrom-Json
$result|Add-Member sourceSHA256 (Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'RearrangedS282\WheelRearranger.FourteenCoupled.cs')).Hash
$result|Add-Member retargetSHA256 (Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'RearrangedS282\DriveRetarget.cs')).Hash
$result|Add-Member baselineRevision $baselineRevision
$result|ConvertTo-Json -Depth 6|Set-Content -LiteralPath $report -Encoding UTF8
Get-Content -LiteralPath $report -Raw
