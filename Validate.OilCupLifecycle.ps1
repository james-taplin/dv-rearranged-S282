param([string]$AssemblyPath=(Join-Path $PSScriptRoot 'build\runtime-trial-1.13.4\bin\RearrangedS282.dll'))
$ErrorActionPreference='Stop'
Add-Type -Path (Join-Path $PSScriptRoot 'lib\AssetStudioModCLI_net472_win32_64\Mono.Cecil.dll')
$module=[Mono.Cecil.ModuleDefinition]::ReadModule($AssemblyPath)
$checks=[Collections.Generic.List[string]]::new()
function Check([bool]$ok,[string]$label){if(-not $ok){throw $label};$checks.Add($label)}
function Method([string]$type,[string]$name){@($module.GetTypes() | Where-Object FullName -EQ ('RearrangedS282.'+$type)).Methods | Where-Object Name -EQ $name}
function Calls($method,[string]$name){@($method.Body.Instructions | Where-Object { $_.OpCode.Name -in @('call','callvirt') -and $_.Operand.Name -eq $name }).Count -gt 0}
try {
    foreach($name in @('UnloadExternalInteractables','ResetConventionalForPool','OnDestroy')) {
        $method=Method 'WheelRearranger' $name
        Check ($null -ne $method -and (Calls $method 'ClearBigEndCups')) "Added cups cleared by $name"
    }
    $clear=Method 'WheelRearranger' 'ClearBigEndCups'
    foreach($name in @('Deinit','RemoveOurs','SetActive','Destroy','Clear','GetComponentsInChildren')) {
        Check (Calls $clear $name) "Owned cup cleanup includes $name"
    }
    $bind=Method 'B99Oilcups' 'Bind'
    Check (Calls $bind 'ConsumerFor') 'Hidden native cups use direct ancestor lookup'
    Check (Calls $bind 'BindOilCupPresentation') 'Native cup binding connects presentation to current arrangement'
    $lookup=Method 'B99Oilcups' 'ConsumerFor'
    Check ((Calls $lookup 'get_parent') -and (Calls $lookup 'GetComponent')) 'Hidden cup lookup walks parents without requiring active hierarchy'
    $active=Method 'BigEndOiling' 'SetActive'
    Check (Calls $active 'HoldUnusedFull') 'Unused hidden oil circuits are primed immediately'
    $tick=Method 'DriveRetarget' 'LateUpdate'
    Check (Calls $tick 'Sync') 'Retarget tick explicitly syncs cups after moving provider'
    $restore=Method 'DriveRetarget' 'Restore'
    Check ((Calls $restore 'set_localPosition') -and (Calls $restore 'Clear')) 'Retarget restore resets oil provider local poses and bindings'
    $capture=Method 'WheelRearranger' 'CaptureOilCupPresentation'
    foreach($key in @('countsMatch','duplicateRefillPorts','overlappingMountPairs','maximumGuideGapM','maximumProviderGapM','capSizeM')) {
        Check (@($capture.Body.Instructions | Where-Object { $_.OpCode.Name -eq 'ldstr' -and $_.Operand -eq $key }).Count -gt 0) "Runtime diagnostics include $key"
    }
    $report=[ordered]@{passed=$true;checks=$checks.Count;results=$checks.ToArray();dllSHA256=(Get-FileHash $AssemblyPath).Hash;limitations='Compiled wiring inspection only; no live Unity pooling, refilling, deletion or controller dispatch.'}
    $output=Join-Path $PSScriptRoot 'build\oilcup-lifecycle-1.13.4'
    New-Item -ItemType Directory -Path $output -Force | Out-Null
    $report | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $output 'validation.json')
    $report | ConvertTo-Json -Depth 4
} finally { $module.Dispose() }

