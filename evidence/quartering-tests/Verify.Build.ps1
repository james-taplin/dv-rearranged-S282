param([string]$AssemblyPath=(Join-Path $PSScriptRoot '..\..\build\quartering-hotfix\bin\RearrangedS282.dll'))
$ErrorActionPreference='Stop'
$root=(Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$work=Join-Path $root 'build\quartering-hotfix'
Add-Type -LiteralPath (Join-Path $root 'lib\AssetStudioModCLI_net472_win32_64\Mono.Cecil.dll')
$normalizer=Get-Content -LiteralPath (Join-Path $root 'Validate.CylinderCasingIntegration.ps1') -Raw
$a=$normalizer.IndexOf('function NormalizedMethod(');$b=$normalizer.IndexOf('function TextHash(',$a)
Invoke-Expression $normalizer.Substring($a,$b-$a)
$baselinePath=Join-Path $root 'build\oiling-ownership-hotfix\bin\RearrangedS282.dll'
$old=[Mono.Cecil.ModuleDefinition]::ReadModule($baselinePath)
$new=[Mono.Cecil.ModuleDefinition]::ReadModule((Resolve-Path -LiteralPath $AssemblyPath).Path)
$checks=[Collections.Generic.List[string]]::new()
function Check([bool]$ok,[string]$why){if(-not $ok){throw $why};$checks.Add($why)}
function Allowed([string]$name){
 return $name -match 'RearrangedS282\.WheelArrangement::(IsConventionalId|NextRuntimeId|\.cctor)\(' -or
  $name -match 'RearrangedS282\.Settings::(OnChange|\.ctor)\(' -or
  $name -match 'RearrangedS282\.Main::(Load|DrawPoweredTenderSettings)\(' -or
  $name -match 'RearrangedS282\.Main/<>c::<Load>b__\d+_\d+\(' -or
  $name -match 'RearrangedS282\.WheelRearranger::(ApplyLegacyGeometry|AlignAdditionalPoweredAxles|UsesReservoirClearance|UsesFullCylinderCasings)\(' -or
  $name -match 'RearrangedS282\.Stage2RuntimeChecks::(StructuralCases|CapturePresentation|ValidatePresentation)\(' -or
  $name -match 'RearrangedS282\.Stage2RuntimeChecks(?:/<>c(?:__DisplayClass\d+_\d+)?)?::<(CapturePresentation|ValidatePresentation)>b__' -or
  $name -match 'RearrangedS282\.Stage2RuntimeChecks/<>c__DisplayClass\d+_\d+::\.ctor\(' -or
  $name -match 'RearrangedS282\.WheelRearranger::ConfigureTriplexDrive\(' -or
  $name -match 'RearrangedS282\.DriveQuartering(?:::|/)' -or
  $name -match 'RearrangedS282\.WheelRearranger::(CacheNativeQuartering|CaptureQuarteringCycle|QuarteringState|ApplyDriverQuartering|QuarteringReport|QuarteringPairReport|SetupDrivers|ResetToFourCoupled|TrySwitchConventional|RestoreConventionalStock|ResetConventionalForPool|BindArticulatedAnimation|BindScaledEngineAnimation|ApplyTriplexGeometry|BindFranklinTriplexAnimation|AttachTenderEngine)\(' -or
  $name -match 'RearrangedS282\.WheelRearranger/GeometryState(?:/|::)' -or
  $name -match 'RearrangedS282\.WheelRearranger(?:/<>c)?::<(CacheNativeQuartering|QuarteringReport|QuarteringPairReport|BindArticulatedAnimation|BindScaledEngineAnimation|BindFranklinTriplexAnimation)>b__' -or
  $name -match 'RearrangedS282\.WheelRearranger::<BindFranklinTriplexAnimation>g__' -or
  $name -match 'RearrangedS282\.WheelRearranger/<>c__DisplayClass\d+_\d+::<(CacheNativeQuartering|QuarteringReport)>b__' -or
  $name -match 'RearrangedS282\.Stage2RuntimeChecks/<RunAll>d__\d+::MoveNext\(' -or
  $name -match 'RearrangedS282\.TenderDriveAnimation::Initialize\('
}
try{
 $before=@{};$after=@{}
 foreach($t in $old.GetTypes()){foreach($m in $t.Methods){$before[$m.FullName]=$m}}
 foreach($t in $new.GetTypes()){foreach($m in $t.Methods){$after[$m.FullName]=$m}}
 $changed=@();$removed=@();$added=@();$unchanged=0
 foreach($name in $before.Keys){
  if(-not $after.ContainsKey($name)){$removed+=$name;continue}
  if((NormalizedMethod $before[$name]) -cne (NormalizedMethod $after[$name])){$changed+=$name}else{$unchanged++}
 }
 $added=@($after.Keys|Where-Object{-not $before.ContainsKey($_)})
 $unexpected=@(@($changed)+@($removed)+@($added)|Where-Object{-not(Allowed $_)})
 Check ($unexpected.Count -eq 0) ("Compiled changes stay within quartering/lifecycle/diagnostics: "+($unexpected -join '; '))
 $mainLoad=@($before.Keys|Where-Object{$_ -match 'RearrangedS282\.Main::Load\('})[0]
 Check ((NormalizedMethod $before[$mainLoad]).Replace('ldc.i4.s 48','ldc.i4.s 49') -ceq (NormalizedMethod $after[$mainLoad])) 'Main load changes only the logged catalogue count'
 $mainLabels=@($changed|Where-Object{$_ -match 'RearrangedS282\.Main/<>c::<Load>b__\d+_\d+\('})
 Check ($mainLabels.Count -eq 1) 'One UMM header lambda changes'
 foreach($name in $mainLabels){
  Check ((NormalizedMethod $before[$name]).Replace('48 ARRANGEMENTS','49 ARRANGEMENTS') -ceq (NormalizedMethod $after[$name])) 'UMM header changes only the displayed catalogue count'
 }
 foreach($helper in @('Show8BigDrivers','ShowFourLeadingWheels','ShowTwoLeadingWheels','ShowTwoTrailingWheels','ShowTwoTrailingWheelsVanilla','ApplyLargeDriverClearance','MeasureTriplex','ConfigureEngineDrive','ConfigureSingleDriverDrive')){
  $key=@($before.Keys|Where-Object{$_ -match ('RearrangedS282\.WheelRearranger::'+$helper+'\(')})[0]
  Check ((NormalizedMethod $before[$key]) -ceq (NormalizedMethod $after[$key])) "Shared geometry unchanged: $helper"
 }
 foreach($type in @('ArticulatedSimulation','ConventionalPhysics','CylinderCalibration','B99PhysicsState','TractiveEffortModel','CurveDragModel','ChuffStageTuning','BigEndOiling','S282Target','S282SpawnScope','DriveRetarget','RodRemesher','PoweredWheelRotater')){
  $prefix='RearrangedS282.'+$type
  $methods=@($before.Keys|Where-Object{$_ -match ([regex]::Escape($prefix)+'(?:::|/)')})
  Check ($methods.Count -gt 0) "Baseline has $type"
  foreach($name in $methods){Check ($after.ContainsKey($name) -and (NormalizedMethod $before[$name]) -ceq (NormalizedMethod $after[$name])) "Unchanged: $name"}
 }
 $owner=$new.Types|Where-Object FullName -EQ 'RearrangedS282.WheelRearranger'
 $dispatch=$owner.Methods|Where-Object Name -EQ TrySwitchConventional
 function Calls($m,$name){@($m.Body.Instructions|Where-Object{$_.OpCode.Name -in @('call','callvirt') -and $_.Operand.Name -eq $name})}
 $final=(Calls $dispatch ApplyDriverQuartering)[0]
 Check ($null -ne $final) 'Successful conversion finalizes quartering'
 foreach($name in @('ApplyArticulatedGeometry','ApplyMalletGeometry','ApplyBigMalletGeometry','ApplyTriplexGeometry','ApplyLegacyGeometry','Apply4144Geometry','ApplyCylinderCasings','ApplySteamConnections')){
  $calls=Calls $dispatch $name
  Check ($calls.Count -gt 0 -and @($calls|Where-Object Offset -GE $final.Offset).Count -eq 0) "Final quartering follows $name"
 }
 Check ($final.Offset -lt (Calls $dispatch SelectPoweredTender)[0].Offset) 'Host phase finalizes before tender selection'
 foreach($name in @('BindArticulatedAnimation','BindScaledEngineAnimation','BindFranklinTriplexAnimation','ResetToFourCoupled')){
  $m=$owner.Methods|Where-Object Name -EQ $name
  Check (@(Calls $m Play).Count -eq 0) "$name no longer seeks before hierarchy activation"
 }
 $restore=$owner.Methods|Where-Object Name -EQ RestoreConventionalStock
 Check (@(Calls $restore ApplyDriverQuartering).Count -eq 1) 'Stock/pool/error restoration establishes native quartering'
 $phase=$new.Types|Where-Object FullName -EQ 'RearrangedS282.DriveQuartering'
 Check (@($phase.Methods|Where-Object Name -in @('Update','LateUpdate','FixedUpdate','OnEnable')).Count -eq 0) 'No continuously seeking phase component'
 $play=$phase.Methods|Where-Object Name -EQ Play
 Check (@(Calls $play set_keepAnimatorControllerStateOnDisable).Count -eq 1) 'Uses native state retention'
 Check (@(Calls $play Update).Count -eq 0 -and @(Calls $play Rebind).Count -eq 0) 'No manual Animator evaluation/rebind'
 $tender=$new.Types|Where-Object FullName -EQ 'RearrangedS282.TenderDriveAnimation'
 Check (@(Calls ($tender.Methods|Where-Object Name -EQ Initialize) Play).Count -eq 1) 'Tender initializes via the shared phase helper'
 $version=($new.Assembly.CustomAttributes|Where-Object{$_.AttributeType.Name -eq 'AssemblyInformationalVersionAttribute'}).ConstructorArguments[0].Value
 Check ($version -ceq '2.0.2-beta' -and $new.Assembly.Name.Version.ToString() -eq '2.0.2.0') 'Hotfix 2 identity is 2.0.2-beta'
 $installed='B:\SteamLibrary\steamapps\common\Derail Valley\Mods\RearrangedS282\RearrangedS282.dll'
 Check ((Get-FileHash -LiteralPath $installed).Hash -eq '4C14D0AAC5DE70955CEFE0B2148F936211F941A258EC2EB1CBE1D516308EC9FE') 'Installed Hotfix 1 stays untouched'
 Check ((Get-FileHash -LiteralPath (Join-Path $root 'release\2.0-beta\RearrangedS282-B99-2.0-beta.zip')).Hash -eq '48A4C061CE75423B6DE0760AE76DC652521940B93520111856E585580E4760C0') 'Public 2.0-beta archive stays untouched'
 $result=[ordered]@{passed=$true;version=$version;checks=$checks.Count;unchangedMethods=$unchanged;changedMethods=@($changed|Sort-Object);removedMethods=@($removed|Sort-Object);addedMethods=@($added|Sort-Object);dllSHA256=(Get-FileHash -LiteralPath $AssemblyPath).Hash;installedUnchanged=$true;runtimeExercised=$false}
 $result|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $work 'build-validation.json') -Encoding utf8
 [ordered]@{passed=$true;checks=$checks.Count;unchangedMethods=$unchanged;changedMethods=$changed.Count;addedMethods=$added.Count;removedMethods=$removed.Count}|ConvertTo-Json
}finally{$old.Dispose();$new.Dispose()}
