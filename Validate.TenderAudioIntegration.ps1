param(
 [string]$AssemblyPath=(Join-Path $PSScriptRoot 'build\runtime-trial-1.22.4\bin\RearrangedS282.dll'),
 [string]$BaselineAssemblyPath=(Join-Path $PSScriptRoot 'build\runtime-trial-1.22.3\bin\RearrangedS282.dll'),
 [string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\runtime-trial-1.22.4\audio-integration')
)
$ErrorActionPreference='Stop'
Add-Type -Path (Join-Path $PSScriptRoot 'lib\AssetStudioModCLI_net472_win32_64\Mono.Cecil.dll')
# Reuse the established opcode/branch/local normalizer, without running another suite.
$validationSource=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Validate.CylinderCasingIntegration.ps1') -Raw
$start=$validationSource.IndexOf('function NormalizedMethod(')
$end=$validationSource.IndexOf('function TextHash(',$start)
Invoke-Expression $validationSource.Substring($start,$end-$start)
$modules=@();$checks=[Collections.Generic.List[string]]::new();$compared=[Collections.Generic.List[string]]::new()
function Check([bool]$ok,[string]$message){if(-not $ok){throw $message};$checks.Add($message)}
function FindType($m,$name){$m.GetTypes() | Where-Object FullName -EQ ('RearrangedS282.'+$name)}
function Method($type,$name){$result=@($type.Methods | Where-Object Name -EQ $name);if($result.Count -ne 1){throw "Ambiguous method $name"};$result[0]}
function CallNames($method){@($method.Body.Instructions | Where-Object {$_.OpCode.Name -in @('call','callvirt','ldftn','ldvirtftn')} | ForEach-Object {$_.Operand.Name})}
try {
 $new=[Mono.Cecil.ModuleDefinition]::ReadModule((Resolve-Path -LiteralPath $AssemblyPath).Path);$modules+=$new
 $old=[Mono.Cecil.ModuleDefinition]::ReadModule((Resolve-Path -LiteralPath $BaselineAssemblyPath).Path);$modules+=$old
 foreach($name in @('ArticulatedSimulation','ArticulatedRearEngine','ArticulatedRearEngineDefinition','CylinderCalibration','TractiveEffortModel','ConventionalPhysics','B99PhysicsState','WheelRearranger')){
  $prefix='RearrangedS282.'+$name
  $before=@($old.GetTypes() | Where-Object {$_.FullName -eq $prefix -or $_.FullName.StartsWith($prefix+'/')} | ForEach-Object Methods | Where-Object Name -NE TenderEngineReport)
  $after=@($new.GetTypes() | Where-Object {$_.FullName -eq $prefix -or $_.FullName.StartsWith($prefix+'/')} | ForEach-Object Methods | Where-Object Name -NE TenderEngineReport)
  Check ($before.Count -gt 0 -and $before.Count -eq $after.Count) "Protected method count retained for $name"
  foreach($method in $before){
   $match=@($after | Where-Object FullName -CEQ $method.FullName)
   Check ($match.Count -eq 1) ('Protected signature retained: '+$method.FullName)
   Check ((NormalizedMethod $method) -ceq (NormalizedMethod $match[0])) ('Protected IL retained: '+$method.FullName)
   $compared.Add($method.FullName)
  }
 }
 $effects=FindType $new ArticulatedEffects
 $calls=CallNames (Method $effects TenderChanged)
 Check ([Array]::IndexOf($calls,'ResetAudio') -lt [Array]::IndexOf($calls,'RefreshNow')) 'Tender refresh removes audio before rebuilding'
 Check ((CallNames (Method (FindType $new 'ArticulatedEffects/ChuffBinding') Add)) -contains 'SeatTenderChuffs') 'Tender chuffs are positioned at their own exhaust'
 Check ((CallNames (Method (FindType $new 'ArticulatedEffects/EffectsState') get_Secondaries)) -contains 'get_TenderEffectsRoot') 'Audio selection includes the active tender root'
 Check ((CallNames (Method (FindType $new 'ArticulatedEffects/CockBinding') Add)) -contains 'SecondaryOutletLocal') 'Tender cock audio uses existing cylinder outlet mapping'
 foreach($t in @('ChuffBinding','CockBinding')){
  $calls=CallNames (Method (FindType $new ('ArticulatedEffects/'+$t)) Remove)
  Check ($calls -contains 'Deinit' -and $calls -contains 'Forget') "$t retains native cleanup and clone disposal"
 }
 Check ((CallNames (Method (FindType $new WheelRearranger) TenderEngineReport)) -contains 'TenderAudioReady') 'Tender diagnostic reports audio readiness'
 New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
 [ordered]@{passed=$true;checks=$checks.Count;unchangedMethods=$compared.Count;methods=$compared.ToArray();candidateSHA256=(Get-FileHash -LiteralPath $AssemblyPath).Hash;baselineSHA256=(Get-FileHash -LiteralPath $BaselineAssemblyPath).Hash;limitation='Compiled preservation and audio wiring only; no audible playback, Unity clone/lifecycle or game-driving claim'} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'validation.json') -Encoding utf8
 [ordered]@{passed=$true;checks=$checks.Count;unchangedMethods=$compared.Count} | ConvertTo-Json
} finally {foreach($m in $modules){$m.Dispose()}}


