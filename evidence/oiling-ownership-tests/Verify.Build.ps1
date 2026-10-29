$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$work = Join-Path $root 'build\oiling-ownership-hotfix'
Add-Type -LiteralPath (Join-Path $root 'lib\AssetStudioModCLI_net472_win32_64\Mono.Cecil.dll')
Add-Type -AssemblyName System.IO.Compression.FileSystem
$publicZip = Join-Path $root 'release\2.0-beta\RearrangedS282-B99-2.0-beta.zip'
$archive = [IO.Compression.ZipFile]::OpenRead($publicZip)
$memory = [IO.MemoryStream]::new()
try { $reader = $archive.GetEntry('RearrangedS282/RearrangedS282.dll').Open(); try { $reader.CopyTo($memory) } finally { $reader.Dispose() } } finally { $archive.Dispose() }
$memory.Position = 0
$old = [Mono.Cecil.ModuleDefinition]::ReadModule($memory)
$new = [Mono.Cecil.ModuleDefinition]::ReadModule((Join-Path $work 'bin\RearrangedS282.dll'))
function Body($method) {
    if (-not $method.HasBody) { return '' }
    $instructions = @($method.Body.Instructions | ForEach-Object { $_.OpCode.Name + ' ' + [string]$_.Operand })
    $locals = @($method.Body.Variables | ForEach-Object { $_.VariableType.FullName })
    $handlers = @($method.Body.ExceptionHandlers | ForEach-Object { [string]$_.HandlerType + ' ' + $_.CatchType + ' ' + $_.TryStart + ' ' + $_.TryEnd + ' ' + $_.HandlerStart + ' ' + $_.HandlerEnd + ' ' + $_.FilterStart })
    return ($locals -join ',') + "`n" + ($instructions -join "`n") + "`n" + ($handlers -join ',')
}
function AllowedHotfixMethod([string]$name) {
    return $name -match 'RearrangedS282\.(BigEndOiling(?:/|::)|S282Target::|S282SpawnScope(?:/|::))' -or
        $name -match 'RearrangedS282\.Main::Load\(' -or
        $name -match 'RearrangedS282\.ConventionalSpawn::(IsS282|StartPending)\(' -or
        $name -match 'RearrangedS282\.ConventionalSpawn/(ReturningToPool::Prefix|<ApplyWhenReady>d__\d+::MoveNext)\(' -or
        $name -match 'RearrangedS282\.ArticulatedSimulation::IsS282\(' -or
        $name -match 'RearrangedS282\.CommsRadioS282Rearranger::(ReapplyFromSettings|HighlightCar|OnUpdate|OnUse|ApplySelectedWheelArrangement)\(' -or
        $name -match 'RearrangedS282\.Stage2RuntimeChecks/<>c::<Begin>b__\d+_0\(' -or
        $name -match 'RearrangedS282\.WheelRearranger::(TrySwitchConventional|ApplyTenderForce|FixedUpdate)\(' -or
        $name -match 'RearrangedS282\.DeltaTrailingTruckCompatibility::(IsDeltaChange|BeginApply|ApplyDelta)\('
}
try {
    $before = @{}; $after = @{}
    foreach ($type in $old.GetTypes()) { foreach ($method in $type.Methods) { $before[$method.FullName] = $method } }
    foreach ($type in $new.GetTypes()) { foreach ($method in $type.Methods) { $after[$method.FullName] = $method } }
    $changed = @(); $removed = @(); $added = @(); $unchangedProtectedMethods = 0
    foreach ($name in $before.Keys) {
        if (-not $after.ContainsKey($name)) { $removed += $name; continue }
        if ((Body $before[$name]) -cne (Body $after[$name])) { $changed += $name }
        elseif (-not (AllowedHotfixMethod $name)) { $unchangedProtectedMethods++ }
    }
    $added = @($after.Keys | Where-Object { -not $before.ContainsKey($_) })
    $unexpected = @(@($changed) + @($removed) + @($added) | Where-Object { -not (AllowedHotfixMethod $_) })
    if ($unexpected.Count) { throw "Unexpected changes outside Hotfix 1 scope: $($unexpected -join '; ')" }
    $native = [Mono.Cecil.ModuleDefinition]::ReadModule('B:\SteamLibrary\steamapps\common\Derail Valley\DerailValley_Data\Managed\Assembly-CSharp.dll')
    try {
        $helper = $native.Types.Where({ $_.FullName -eq 'Bogie' }).Methods.Where({ $_.Name -eq 'ApplyForce' })
        $ops = @($helper.Body.Instructions | ForEach-Object { $_.OpCode.Name }) -join ','
        if ($ops -ne 'ldarg.0,ldfld,ldarg.0,call,callvirt,ldarg.1,call,callvirt,ret' -or
            ([string]$helper.Body.Instructions[7].Operand) -notmatch 'Rigidbody::AddForce\(UnityEngine.Vector3\)') { throw 'Native force helper changed; re-audit its semantics.' }
        foreach ($name in @('ApplyTenderForce','FixedUpdate')) {
            $method = $new.Types.Where({ $_.Name -eq 'WheelRearranger' }).Methods.Where({ $_.Name -eq $name })
            $calls = @($method.Body.Instructions | Where-Object { $_.OpCode.Name -in @('call','callvirt') } | ForEach-Object { [string]$_.Operand })
            if (@($calls | Where-Object { $_ -match 'Bogie::ApplyForce' }).Count -ne 1 -or @($calls | Where-Object { $_ -match 'Rigidbody::AddForce' }).Count) { throw "Manual force path remains in $name" }
            $original = $old.Types.Where({ $_.Name -eq 'WheelRearranger' }).Methods.Where({ $_.Name -eq $name })
            $scalarBefore = @($original.Body.Instructions | Where-Object { $_.OpCode.Name -in @('mul','div','add','sub','neg') } | ForEach-Object { $_.OpCode.Name }) -join ','
            $scalarAfter = @($method.Body.Instructions | Where-Object { $_.OpCode.Name -in @('mul','div','add','sub','neg') } | ForEach-Object { $_.OpCode.Name }) -join ','
            if ($scalarBefore -ne $scalarAfter) { throw "Scalar force arithmetic changed in $name" }
        }
        $writers = @($native.GetTypes() | ForEach-Object Methods | Where-Object { $_.HasBody -and ($_.Body.Instructions | Where-Object { $_.OpCode.Name -eq 'stfld' -and [string]$_.Operand -eq 'System.Boolean TrainCar::uniqueCar' }) })
        if ($writers.Count -ne 3 -or @($writers | Where-Object { $_.DeclaringType.FullName -ne 'CarSpawner' -or $_.Name -notin @('BaseSpawn','SpawnLoadedCar','SpawnLoadedCarAtNearestAvailableSpace') -or -not ($_.Parameters | Where-Object { $_.Name -eq 'uniqueCar' -and $_.ParameterType.FullName -eq 'System.Boolean' }) }).Count) { throw 'Native unique-car construction paths changed; re-audit spawn scopes.' }
    } finally { $native.Dispose() }
    $informational = ($new.Assembly.CustomAttributes | Where-Object { $_.AttributeType.Name -eq 'AssemblyInformationalVersionAttribute' }).ConstructorArguments[0].Value
    if ($informational -cne '2.0.1-beta' -or $new.Assembly.Name.Version.ToString() -ne '2.0.1.0') { throw 'Hotfix assembly version mismatch.' }
    $mainLoad = $new.Types.Where({ $_.FullName -eq 'RearrangedS282.Main' }).Methods.Where({ $_.Name -eq 'Load' })
    if ($mainLoad.Body.Instructions.Operand | Where-Object { [string]$_ -match 'HarmonyLib.Harmony::PatchAll' }) { throw 'Legacy global PatchAll is active.' }
    if (-not ($mainLoad.Body.Instructions.Operand | Where-Object { [string]$_ -match 'S282SpawnScope::Register' })) { throw 'Early unique-spawn guard is not registered.' }
    $installed = 'B:\SteamLibrary\steamapps\common\Derail Valley\Mods\RearrangedS282\RearrangedS282.dll'
    $publicHash = (Get-FileHash -LiteralPath $publicZip).Hash
    $installedHash = (Get-FileHash -LiteralPath $installed).Hash
    if ($publicHash -ne '48A4C061CE75423B6DE0760AE76DC652521940B93520111856E585580E4760C0') { throw 'Existing public release was modified.' }
    if ($installedHash -ne 'C4C216972DE6687E2F4A2C3FF5862AACFAFEDC5852CC22636EFA278C77D73A1A') { throw 'Installed mod was modified.' }
    $report = [ordered]@{ passed=$true; version=$informational; unchangedProtectedMethods=$unchangedProtectedMethods; changedMethods=$changed; removedMethods=$removed; addedMethods=$added; nativeForceHelperVerified=$true; legacyPatchAllActive=$false; installedDllUnchanged=$true; publicReleaseUnchanged=$true; dllSHA256=(Get-FileHash -LiteralPath (Join-Path $work 'bin\RearrangedS282.dll')).Hash; runtimeExercised=$false }
    $report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $work 'build-validation.json') -Encoding utf8
    $report | ConvertTo-Json -Depth 6
} finally { $old.Dispose(); $new.Dispose(); $memory.Dispose() }
