param([string]$AssemblyPath=(Join-Path $PSScriptRoot 'build\runtime-trial-1.19.0\bin\RearrangedS282.dll'))
$ErrorActionPreference='Stop'
Add-Type -Path (Join-Path $PSScriptRoot 'lib\AssetStudioModCLI_net472_win32_64\Mono.Cecil.dll')
$module=[Mono.Cecil.ModuleDefinition]::ReadModule($AssemblyPath)
$checks=[Collections.Generic.List[string]]::new()
function Check([bool]$ok,[string]$message){if(-not $ok){throw $message};$checks.Add($message)}
function Method([string]$type,[string]$name){($module.GetTypes() | Where-Object FullName -eq ('RearrangedS282.'+$type)).Methods | Where-Object Name -eq $name}
function CallIndex($method,[string]$type,[string]$name){$i=0;foreach($op in $method.Body.Instructions){if($op.OpCode.Name -in @('call','callvirt') -and $op.Operand.DeclaringType.Name -eq $type -and $op.Operand.Name -eq $name){return $i};$i++};return -1}
try {
    $switch=Method 'WheelRearranger' 'TrySwitchConventional'
    $cal=CallIndex $switch 'CylinderCalibration' 'Apply'
    Check ($cal -gt (CallIndex $switch 'B99PhysicsState' 'Apply')) 'Cylinder calibration follows committed native axle/radius/adhesion selection'
    Check ($cal -gt (CallIndex $switch 'ArticulatedSimulation' 'Apply')) 'Active native engines selected before cylinder calibration'
    Check ((CallIndex $switch 'CylinderCalibration' 'Restore') -lt (CallIndex $switch 'WheelRearranger' 'RestoreCylinderCasings')) 'Previous dimensions restored before conversion'
    foreach($name in @('RestoreConventionalStock','OnDestroy')){Check ((CallIndex (Method 'WheelRearranger' $name) 'CylinderCalibration' 'Restore') -ge 0) "$name restores per-car engine dimensions"}
    Check ((CallIndex (Method 'WheelRearranger' 'ResetConventionalForPool') 'WheelRearranger' 'RestoreConventionalStock') -ge 0) 'Pooling uses exact stock restoration'
    $calType=$module.GetTypes() | Where-Object FullName -eq 'RearrangedS282.CylinderCalibration'
    $methods=@($module.GetTypes() | Where-Object {$_.FullName -eq $calType.FullName -or $_.FullName.StartsWith($calType.FullName+'/')} | ForEach-Object Methods | Where-Object HasBody)
    $stores=@($methods.Body.Instructions | Where-Object {$_.OpCode.Name -eq 'stfld' -and $_.Operand.DeclaringType.FullName -notlike 'RearrangedS282.CylinderCalibration*'})
    Check (@($stores | Where-Object {$_.Operand.FullName -ne 'System.Single LocoSim.Implementations.ReciprocatingSteamEngine::cylinderBore'}).Count -eq 0) 'Direct foreign field stores limited to per-instance cylinder bore'
    $reflectionNames=@((Method 'CylinderCalibration' '.cctor').Body.Instructions | Where-Object {$_.OpCode.Name -eq 'ldstr'} | ForEach-Object Operand)
    Check (($reflectionNames | Sort-Object) -join ',' -eq 'cylinderVolume,pistonArea') 'Reflection fields limited to two required native cached dimensions'
    $allOps=($methods.Body.Instructions | ForEach-Object {[string]$_.Operand}) -join "`n"
    Check ($allOps -notmatch 'parentType|set_CarMass|AddForce|AddTorque|SetComponentData|set_Value|throttleMaxFlow|steamChestVolume') 'Calibration does not rewrite shared definitions, mass, flow/chest limits, ports or forces'
    $clone=Method 'WheelRearranger' 'CloneEngineDrive'
    Check ((CallIndex $clone 'PoweredWheelRotater' 'BindClonedDrive') -ge 0) 'Rear cloned drive rebinds its retained added-wheel rotation followers'
    $runType=$module.GetTypes() | Where-Object FullName -like 'RearrangedS282.Stage2RuntimeChecks/<RunAll>*'
    $literals=@($runType.Methods.Body.Instructions | Where-Object {$_.OpCode.Name -eq 'ldstr'} | ForEach-Object Operand)
    Check ($literals -contains 'physicsCalibration' -and $literals -contains 'rearDriverFollowers') 'Structural test checks physics calibration and rear driver follower state'
    [ordered]@{passed=$true;checks=$checks.Count;dllSHA256=(Get-FileHash -LiteralPath $AssemblyPath).Hash;results=$checks.ToArray();limitations='Compiled ownership/wiring plus separate native engine and managed clone fixtures. Live driving and save/reload acceptance pending.'}|ConvertTo-Json -Depth 4
} finally {$module.Dispose()}
