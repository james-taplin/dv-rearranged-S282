param([string]$AssemblyPath=(Join-Path $PSScriptRoot 'build\runtime-trial-1.18.0\bin\RearrangedS282.dll'))
$ErrorActionPreference='Stop'
Add-Type -Path (Join-Path $PSScriptRoot 'lib\AssetStudioModCLI_net472_win32_64\Mono.Cecil.dll')
$module=[Mono.Cecil.ModuleDefinition]::ReadModule($AssemblyPath)
$checks=[Collections.Generic.List[string]]::new()
function Check([bool]$ok,[string]$label){if(-not $ok){throw $label};$checks.Add($label)}
function Method([string]$type,[string]$name){$module.GetTypes() | Where-Object FullName -EQ ('RearrangedS282.'+$type) | ForEach-Object Methods | Where-Object Name -EQ $name}
function Calls($method){@($method.Body.Instructions | Where-Object {$_.OpCode.Name -in @('call','callvirt')} | ForEach-Object {$_.Operand.Name})}
try {
    foreach($name in @('RestoreConventionalStock','OnDestroy')){
        $calls=Calls (Method 'WheelRearranger' $name)
        Check ($calls -contains 'SuspendSteamConnections') "$name releases steam presentation"
        $later=if($name -eq 'OnDestroy'){'DisposeCylinderCasings'}else{'RestoreCylinderCasings'}
        Check ([Array]::IndexOf($calls,'SuspendSteamConnections') -lt [Array]::IndexOf($calls,$later)) "$name releases pipes before casing source cleanup"
    }
    foreach($name in @('AttachTenderEngine','DetachTenderEngine')){Check ((Calls (Method 'WheelRearranger' $name)) -contains 'RefreshSteamConnectionsForTender') "$name refreshes steam presentation"}
    $actor=$module.GetTypes() | Where-Object FullName -EQ 'RearrangedS282.SteamPipePresentation'
    Check (@($actor.CustomAttributes | Where-Object {$_.AttributeType.Name -eq 'DefaultExecutionOrder' -and $_.ConstructorArguments[0].Value -eq 30000}).Count -eq 1) 'Presentation follows native LateUpdate poses'
    $tickCalls=Calls (Method 'SteamPipePresentation' 'Tick')
    foreach($forbidden in @('AddForce','AddTorque','SetFloat','SetValue','Simulate','set_velocity','set_angularVelocity')){Check ($tickCalls -notcontains $forbidden) "Visual tick does not call $forbidden"}
    Check ((Calls (Method 'SteamPipePresentation' 'Release')) -contains 'SetLODs') 'Release restores native LOD membership'
    Check ((Calls (Method 'SteamPipePresentation' 'Release')) -contains 'Destroy') 'Release destroys owned meshes'
    Check ((Calls (Method 'WheelRearranger' 'SuspendSteamConnections')) -contains 'Forget') 'Release removes owned explosion renderers'
    Check ((Calls (Method 'WheelRearranger' 'SuspendSteamConnections')) -contains 'RestoreSteamSources') 'Release restores legacy renderer states'
    Check ((Calls (Method 'SteamPipePresentation' 'LateUpdate')) -contains 'SetActive') 'Late failure hides stale presentation'
    [ordered]@{passed=$true;checks=$checks.Count;dllSHA256=(Get-FileHash -LiteralPath $AssemblyPath).Hash;limitations='Compiled ownership wiring only, complemented by managed geometry/lifecycle fixtures; live rendering and clearance remain unverified'} | ConvertTo-Json
} finally {$module.Dispose()}
