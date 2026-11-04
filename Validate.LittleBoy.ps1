param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\offline-validation\LittleBoy'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$fixture=Join-Path $PSScriptRoot 'evidence\little-boy-tests'
$frames=Join-Path $OutputDirectory 'native-bogie-frames.json'
$spacing=Join-Path $PSScriptRoot 'evidence\fourteen-spacing-tests\native-spacing.json'
$animation=Join-Path $PSScriptRoot 'evidence\pacific-hudson-valve-gear\native-animation.json'
& 'C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe' (Join-Path $fixture 'extract_native.py') --output $frames
if($LASTEXITCODE -ne 0){throw 'Native bogie extraction failed'}
$nativeFrames=Get-Content $frames -Raw | ConvertFrom-Json
$nativeSpacing=Get-Content $spacing -Raw | ConvertFrom-Json
if($nativeFrames.resourcesSHA256 -ne $nativeSpacing.sources.resourcesSHA256){throw 'Native spacing and bogie data have different resource-asset provenance'}
function Extract([string]$text,[string]$marker) {
 $start=$text.IndexOf($marker,[StringComparison]::Ordinal)
 if($start -lt 0){throw "Missing source member $marker"}
 $open=$text.IndexOf('{',$start);if($open -lt 0){throw "Missing body for $marker"}
 $depth=1;$end=$open+1
 while($depth -gt 0 -and $end -lt $text.Length){if($text[$end] -eq '{'){$depth++};if($text[$end] -eq '}'){$depth--};$end++}
 if($depth -ne 0){throw "Unbalanced source member $marker"}
 return $text.Substring($start,$end-$start)
}
$malletPath=Join-Path $PSScriptRoot 'RearrangedS282\WheelRearranger.Mallet.cs'
$basePath=Join-Path $PSScriptRoot 'RearrangedS282\WheelRearranger.cs'
$newPath=Join-Path $PSScriptRoot 'RearrangedS282\WheelRearranger.LittleBoy.cs'
$dispatchPath=Join-Path $PSScriptRoot 'RearrangedS282\WheelRearranger.Conventional.cs'
$cataloguePath=Join-Path $PSScriptRoot 'RearrangedS282\WheelArrangement.cs'
$mallet=Get-Content $malletPath -Raw;$base=Get-Content $basePath -Raw
$constants=@('MalletScale','MalletEngineGap','MalletTrailingRadius','MalletCylinderClearanceGap') | ForEach-Object {
 $match=[regex]::Match($mallet,'\b'+$_+'\s*=\s*([^,;]+)').Groups[1].Value
 if(-not $match){throw "Missing Mallet constant $_"}
 "private const float $_ = $match;"
}
$methods=@((Extract $mallet 'private sealed class MalletLayout'),(Extract $mallet 'private MalletLayout MeasureMallet()'),
 (Extract $base 'private void ShowTwoLeadingWheels()'),(Extract $base 'private void ShowFourLeadingWheels()'),
 (Extract $base 'private void ShowFourTrailingWheelsFor10Coupled()'))
$generated=Join-Path $OutputDirectory 'ActualNativeMethods.cs'
$dispatch=Extract (Get-Content $dispatchPath -Raw) 'if (WheelArrangement.IsArticulatedId(id))'
Set-Content $generated -Encoding UTF8 -Value ("using System; using UnityEngine; namespace RearrangedS282 { internal partial class WheelRearranger {`n"+($constants -join "`n")+"`n"+($methods -join "`n")+"`nprivate void DispatchActual(int id) { $dispatch }`n} }")
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit></PropertyGroup><ItemGroup><Compile Include="$generated"/><Compile Include="$newPath"/><Compile Include="$cataloguePath"/><Compile Include="$fixture\Program.cs"/></ItemGroup></Project>
"@
$projectPath=Join-Path $OutputDirectory 'LittleBoy.csproj'
Set-Content $projectPath $project -Encoding UTF8
$report=Join-Path $OutputDirectory 'validation.json'
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- $frames $spacing $animation $report
if($LASTEXITCODE -ne 0){throw 'Little Boy carrying truck validation failed'}
$result=Get-Content $report -Raw | ConvertFrom-Json
$result | Add-Member provenance ([ordered]@{
 sourceSHA256=[ordered]@{littleBoy=(Get-FileHash $newPath -Algorithm SHA256).Hash;mallet=(Get-FileHash $malletPath -Algorithm SHA256).Hash;wheelRearranger=(Get-FileHash $basePath -Algorithm SHA256).Hash;dispatcher=(Get-FileHash $dispatchPath -Algorithm SHA256).Hash;catalogue=(Get-FileHash $cataloguePath -Algorithm SHA256).Hash}
 nativeFramesSHA256=(Get-FileHash $frames -Algorithm SHA256).Hash
 nativeSpacingSHA256=(Get-FileHash $spacing -Algorithm SHA256).Hash
 nativeAnimationSHA256=(Get-FileHash $animation -Algorithm SHA256).Hash
 resourcesSHA256=$nativeFrames.resourcesSHA256
 note='Offline extracted-source calculation with native inputs, not an in-game measurement or full locomotive validation.'
})
$result | ConvertTo-Json -Depth 8 | Set-Content $report -Encoding UTF8
Get-Content $report -Raw
