param(
    [string]$AssemblyPath = (Join-Path $PSScriptRoot 'build\runtime-trial-1.13.0\bin\RearrangedS282.dll'),
    [string]$BaselineAssemblyPath = (Join-Path $PSScriptRoot 'build\runtime-trial-1.12.0\bin\RearrangedS282.dll'),
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'build\cylinder-casing-integration'),
    [string]$ExpectedVersion = '1.13.0.0',
    [string]$ExpectedBaselineVersion = '1.12.0.0',
    [switch]$AllowOilCupRetarget,
    [switch]$AllowConnectingRodRetarget,
    [switch]$AllowPreliminaryPhysics
)
$ErrorActionPreference = 'Stop'
Add-Type -Path (Join-Path $PSScriptRoot 'lib\AssetStudioModCLI_net472_win32_64\Mono.Cecil.dll')
$modules = [Collections.Generic.List[object]]::new()
$checks = [Collections.Generic.List[string]]::new()
$comparisons = [Collections.Generic.List[object]]::new()
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$reportPath = Join-Path $OutputDirectory 'validation.json'
function Check([bool]$condition,[string]$label) {
    if (-not $condition) { throw $label }
    $checks.Add($label)
}
function OpenModule([string]$path) {
    $module = [Mono.Cecil.ModuleDefinition]::ReadModule((Resolve-Path -LiteralPath $path).Path)
    $modules.Add($module)
    return $module
}
function TypeOf($module,[string]$name) {
    $found = @($module.GetTypes() | Where-Object FullName -CEQ $name)
    if ($found.Count -ne 1) { throw "Expected one compiled type $name; found $($found.Count)." }
    return $found[0]
}
function MethodOf($type,[string]$name) {
    $found = @($type.Methods | Where-Object Name -CEQ $name)
    if ($found.Count -ne 1) { throw "Expected one compiled $($type.FullName)::$name; found $($found.Count)." }
    return $found[0]
}
function Calls($method,[string]$name) {
    return @($method.Body.Instructions | Where-Object {
        $_.OpCode.Name -in @('call','callvirt','newobj') -and $_.Operand.Name -ceq $name
    })
}
function FirstCall($method,[string]$name) {
    $calls = @(Calls $method $name)
    if ($calls.Count -eq 0) { throw "Compiled $($method.FullName) does not call $name." }
    return $calls[0]
}
function Literals($method) {
    return @($method.Body.Instructions | Where-Object { $_.OpCode.Name -ceq 'ldstr' } | ForEach-Object Operand)
}
function Reachable($start,$target) {
    if ($null -eq $start -or $null -eq $target) { return $false }
    $queue = [Collections.Generic.Queue[object]]::new()
    $seen = [Collections.Generic.HashSet[int]]::new()
    $queue.Enqueue($start)
    while ($queue.Count -gt 0) {
        $op = $queue.Dequeue()
        if (-not $seen.Add($op.Offset)) { continue }
        if ($op.Offset -eq $target.Offset) { return $true }
        switch ($op.OpCode.FlowControl.ToString()) {
            'Return' { continue }
            'Throw' { continue }
            'Branch' { if ($null -ne $op.Operand) { $queue.Enqueue($op.Operand) }; continue }
            'Cond_Branch' {
                foreach ($branch in @($op.Operand)) { $queue.Enqueue($branch) }
                if ($null -ne $op.Next) { $queue.Enqueue($op.Next) }
                continue
            }
            default { if ($null -ne $op.Next) { $queue.Enqueue($op.Next) } }
        }
    }
    return $false
}
function NormalizedMethod($method) {
    if (-not $method.HasBody) { return '<no body>' }
    $indices = @{}
    for ($i=0; $i -lt $method.Body.Instructions.Count; $i++) {
        $indices[$method.Body.Instructions[$i].Offset] = $i
    }
    function Label($op) { if ($null -eq $op) { return '<end>' }; return [string]$indices[$op.Offset] }
    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add('locals:' + ((@($method.Body.Variables | ForEach-Object { $_.VariableType.FullName })) -join '|'))
    $lines.Add('initlocals:' + $method.Body.InitLocals)
    foreach ($op in $method.Body.Instructions) {
        $operand = $op.Operand
        if ($null -eq $operand) { $text = '' }
        elseif ($operand -is [Mono.Cecil.Cil.Instruction]) { $text = 'label:' + (Label $operand) }
        elseif ($operand -is [Mono.Cecil.Cil.Instruction[]]) { $text = 'labels:' + ((@($operand | ForEach-Object { Label $_ })) -join ',') }
        elseif ($operand -is [Mono.Cecil.Cil.VariableDefinition]) { $text = 'local:' + $operand.Index }
        elseif ($operand -is [Mono.Cecil.ParameterDefinition]) { $text = 'arg:' + $operand.Index }
        elseif ($operand -is [Mono.Cecil.MemberReference]) { $text = $operand.FullName }
        elseif ($operand -is [single] -or $operand -is [double]) { $text = $operand.ToString('R',[Globalization.CultureInfo]::InvariantCulture) }
        else { $text = [string]$operand }
        $lines.Add($op.OpCode.Name + ' ' + $text)
    }
    foreach ($handler in $method.Body.ExceptionHandlers) {
        $lines.Add('handler:' + $handler.HandlerType + '|' + (Label $handler.TryStart) + '|' +
            (Label $handler.TryEnd) + '|' + (Label $handler.HandlerStart) + '|' +
            (Label $handler.HandlerEnd) + '|' + (Label $handler.FilterStart) + '|' + $handler.CatchType.FullName)
    }
    return $lines -join "`n"
}
function TextHash([string]$text) {
    return [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($text)))
}
try {
    $candidate = OpenModule $AssemblyPath
    $baseline = OpenModule $BaselineAssemblyPath
    $wheel = TypeOf $candidate 'RearrangedS282.WheelRearranger'
    $shape = TypeOf $candidate 'RearrangedS282.CylinderCasingShape'
    Check ($candidate.Assembly.Name.Version.ToString() -eq $ExpectedVersion) "Candidate is the compiled $ExpectedVersion test assembly"
    Check ($baseline.Assembly.Name.Version.ToString() -eq $ExpectedBaselineVersion) "Unchanged-code comparison baseline is compiled $ExpectedBaselineVersion"

    $dispatch = MethodOf $wheel 'TrySwitchConventional'
    $cache = FirstCall $dispatch 'CacheCylinderCasings'
    $setup = FirstCall $dispatch 'SetupDrivers'
    $apply = FirstCall $dispatch 'ApplyCylinderCasings'
    Check ($cache.Offset -gt $setup.Offset) 'Native casing source frames are captured after driver preparation'
    Check ($apply.Offset -gt (FirstCall $dispatch 'ApplyLegacyGeometry').Offset -and
        $apply.Offset -gt (FirstCall $dispatch 'ApplyArticulatedGeometry').Offset) 'Casing deformation follows both conventional and articulated layout application'
    $stockRestore = @(Calls $dispatch 'RestoreConventionalStock' | Where-Object Offset -GT $cache.Offset)[0]
    $stockBranches = @($dispatch.Body.Instructions | Where-Object {
        $_.Offset -gt $cache.Offset -and $_.Offset -lt $stockRestore.Offset -and
        $_.OpCode.FlowControl.ToString() -eq 'Cond_Branch' -and
        $_.Previous.OpCode.Name -in @('ldc.i4','ldc.i4.s') -and $_.Previous.Operand -eq 10 -and
        $_.Previous.Previous.OpCode.Name -eq 'ldarg.1'
    })
    Check ($stockBranches.Count -eq 1) 'Compiled dispatch has an unambiguous stock ID 10 conditional before stock restoration'
    $stockBranch = $stockBranches[0]
    $routes = @($stockBranch.Operand,$stockBranch.Next)
    Check (@($routes | Where-Object { Reachable $_ $apply }).Count -eq 1) 'Only the non-stock side of the stock/layout branch reaches casing deformation'
    Check (-not (Reachable $stockRestore.Next $apply)) 'Normal stock restoration flow bypasses casing deformation'

    foreach ($transition in @('TrySwitchConventional','RestoreConventionalStock')) {
        $method = MethodOf $wheel $transition
        $restore = FirstCall $method 'RestoreCylinderCasings'
        Check ($restore.Offset -lt (FirstCall $method 'SuspendArticulatedGeometry').Offset) "Corrected meshes restore before articulated suspension: $transition"
        $snapshotRestore = @($method.Body.Instructions | Where-Object {
            $_.OpCode.Name -in @('call','callvirt') -and $_.Operand.Name -eq 'Restore' -and
            $_.Operand.DeclaringType.Name -eq 'GeometryState'
        })[0]
        Check ($null -ne $snapshotRestore -and $restore.Offset -lt $snapshotRestore.Offset) "Corrected meshes restore before captured geometry: $transition"
    }
    $destroy = MethodOf $wheel 'OnDestroy'
    Check ((FirstCall $destroy 'DisposeCylinderCasings').Offset -lt (FirstCall $destroy 'DisposeArticulatedGeometry').Offset) 'Casing meshes dispose before articulated owners on car deletion'
    $restoreMeshes = MethodOf $wheel 'RestoreCylinderCasings'
    Check ((FirstCall $restoreMeshes 'DetachTenderEngine').Offset -lt (FirstCall $restoreMeshes 'Destroy').Offset) 'Tender visuals detach before shared corrected casing meshes are destroyed'
    Check ((FirstCall $restoreMeshes 'set_sharedMesh').Offset -lt (FirstCall $restoreMeshes 'Destroy').Offset) 'Original mesh filters are restored before corrected mesh destruction'
    $dispose = MethodOf $wheel 'DisposeCylinderCasings'
    Check ((FirstCall $dispose 'RestoreCylinderCasings').Offset -lt (FirstCall $dispose 'Destroy').Offset) 'Disposal restores filters before destroying owned readable fallback meshes'

    $cacheNative = MethodOf $wheel 'CacheCylinderCasings'
    $nativeStrings = @(Literals $cacheNative)
    Check ($nativeStrings -contains 'Static_LOD0' -and $nativeStrings -contains 'Static_LOD1') 'Compiled source discovery includes both native static LOD containers'
    Check ($nativeStrings -contains 's282a_split_body' -and @(Calls $cacheNative 'StartsWith').Count -ge 1) 'Split-root discovery accepts native LOD-specific suffixes'
    Check (@($nativeStrings | Where-Object { $_ -like '*s282a_split_body(Clone)*' }).Count -eq 0) 'Source discovery does not require the LOD0-only split-root name'
    $cacheFranklin = MethodOf $wheel 'CacheFranklinCasings'
    Check (@(Calls $cacheFranklin 'get_FranklinBValveGear').Count -eq 1 -and @(Calls $cacheFranklin 'get_FranklinBValveGear4444').Count -eq 1) 'Both original Franklin prefab variants provide casing templates'
    Check (@(Literals $cacheFranklin) -contains 'cylinder/default') 'Franklin source selection targets the casing mesh rather than its separate moving shaft hardware'
    $cacheCasing = MethodOf $wheel 'CacheCasing'
    Check (@(Literals $cacheCasing) -contains 'franklin_casing.obj' -and @(Calls $cacheCasing 'get_isReadable').Count -ge 2) 'Unreadable original Franklin casing has a checked readable-asset fallback'
    Check (@($cacheCasing.Body.Instructions | Where-Object {
        $_.OpCode.Name -in @('call','callvirt') -and $_.Operand.DeclaringType.FullName -eq 'Dummiesman.OBJLoader' -and $_.Operand.Name -eq 'Load'
    }).Count -eq 1) 'Fallback uses the existing OBJ mesh loader'
    Check (@(Calls (MethodOf $wheel 'ApplyCylinderCasings') 'set_sharedMesh').Count -eq 1) 'Runtime correction is committed through the selected MeshFilter shared mesh'

    $casingMethods = @($wheel.Methods | Where-Object { $_.HasBody -and ($_.Name -match 'Casing' -or $_.Name -eq 'ApplyPrairieFittings') }) + @($shape.Methods | Where-Object HasBody)
    $casingMethods += @($candidate.GetTypes() | ForEach-Object Methods | Where-Object {
        $_.HasBody -and $_.Name -match '^<CacheCasing>'
    })
    foreach ($method in $casingMethods) {
        $transformWrites = @($method.Body.Instructions | Where-Object {
            $_.OpCode.Name -in @('call','callvirt') -and $_.Operand.DeclaringType.FullName -eq 'UnityEngine.Transform' -and $_.Operand.Name -like 'set_*'
        })
        # 1.13.2 also relocates/restores cached particle outlets. Those two
        # methods intentionally set position; the native-animation fixture
        # separately verifies every drivetrain transform stays unchanged.
        $allowed = switch ($method.Name) {
            'ApplyPrairieFittings' { @('set_position') }
            'RestoreCylinderCasings' { @('set_localPosition') }
            default { @() }
        }
        Check (@($transformWrites | Where-Object { $_.Operand.Name -notin $allowed }).Count -eq 0) "Casing transform writes limited to cached particle position apply/restore: $($method.FullName)"
        $simulationWrites = @($method.Body.Instructions | Where-Object {
            ($_.OpCode.Name -in @('stfld','stsfld') -and $_.Operand.DeclaringType.FullName -match '^(DV\.Simulation|LocoSim\.|UnityEngine\.(Rigidbody|Collider))') -or
            ($_.OpCode.Name -in @('call','callvirt') -and $_.Operand.DeclaringType.FullName -match '^(DV\.Simulation|LocoSim\.|RearrangedS282\.(B99PhysicsState|ConventionalPhysics|ArticulatedSimulation))')
        })
        Check ($simulationWrites.Count -eq 0) "Casing method does not write or invoke simulation/traction state: $($method.FullName)"
    }

    foreach ($typeName in @('ConventionalPhysics','B99PhysicsState','ArticulatedSimulation','DriveRetarget')) {
        if ($AllowConnectingRodRetarget -and $typeName -eq 'DriveRetarget') {
            Check $true 'DriveRetarget presentation intentionally extended; validate precision and unchanged legacy behaviour with connecting-rod, Prairie and 4-14-4 motion fixtures'
            continue
        }
        $fullName = 'RearrangedS282.' + $typeName
        $oldTypes = @($baseline.GetTypes() | Where-Object { $_.FullName -eq $fullName -or $_.FullName.StartsWith($fullName + '/', [StringComparison]::Ordinal) })
        $newTypes = @($candidate.GetTypes() | Where-Object { $_.FullName -eq $fullName -or $_.FullName.StartsWith($fullName + '/', [StringComparison]::Ordinal) })
        $oilPresentation = $AllowOilCupRetarget -and $typeName -eq 'DriveRetarget'
        if ($oilPresentation) {
            $newOnly = @($newTypes | Where-Object { $_.FullName -notin @($oldTypes.FullName) })
            Check ($newOnly.Count -eq 1 -and $newOnly[0].Name -eq 'OilPoint') 'Retarget adds only the oil-provider presentation record type'
        } else {
            Check ($oldTypes.Count -gt 0 -and $oldTypes.Count -eq $newTypes.Count) "Physics/motion type family is unchanged: $typeName"
        }
        $oldMethods = @($oldTypes | ForEach-Object Methods)
        $newMethods = @($newTypes | ForEach-Object Methods)
        if ($AllowPreliminaryPhysics -and $typeName -in @('ConventionalPhysics','B99PhysicsState')) {
            $added = @($newMethods | Where-Object FullName -NotIn @($oldMethods.FullName))
            $expected = if ($typeName -eq 'ConventionalPhysics') { 'TargetDriverWeightFraction' } else { 'get_OriginalSlipCoefficient' }
            Check ($added.Count -eq 1 -and $added[0].Name -eq $expected) "Only reviewed preliminary physics accessor/calculation added: $typeName; count=$($added.Count); names=$($added.Name -join ","); expected=$expected"
        }
        elseif (-not $oilPresentation) { Check ($oldMethods.Count -eq $newMethods.Count) "Physics/motion method count is unchanged: $typeName" }
        else {
            $newSignatures = @($newMethods | Where-Object { $_.FullName -notin @($oldMethods.FullName) })
            Check ($newSignatures.Count -eq 2 -and ((@($newSignatures.Name | Sort-Object) -join ',') -eq '.ctor,BindOilCup')) 'Retarget adds only BindOilCup and the oil record constructor'
        }
        foreach ($method in $oldMethods) {
            $match = @($newMethods | Where-Object FullName -CEQ $method.FullName)
            Check ($match.Count -eq 1) "Baseline method signature retained: $($method.FullName)"
            if ($oilPresentation -and $method.DeclaringType.FullName -eq $fullName -and $method.Name -in @('.ctor','LateUpdate','Restore')) { continue }
            $oldIL = NormalizedMethod $method
            $newIL = NormalizedMethod $match[0]
            Check ($oldIL -ceq $newIL) "Normalized compiled IL unchanged: $($method.FullName)"
            $comparisons.Add([ordered]@{method=$method.FullName;normalizedILSHA256=(TextHash $newIL)})
        }
    }
    $report = [ordered]@{
        passed=$true;version=$candidate.Assembly.Name.Version.ToString();checks=$checks.Count;results=$checks.ToArray()
        candidate=(Resolve-Path -LiteralPath $AssemblyPath).Path;candidateSHA256=(Get-FileHash -LiteralPath $AssemblyPath).Hash
        baseline=(Resolve-Path -LiteralPath $BaselineAssemblyPath).Path;baselineSHA256=(Get-FileHash -LiteralPath $BaselineAssemblyPath).Hash
        unchangedCompiledMethods=$comparisons.Count;methodComparisons=$comparisons.ToArray()
        connectingRodPresentationChanged=[bool]$AllowConnectingRodRetarget
        limitations='Cecil inspects actual compiled IL and normal-flow dispatch without loading Unity. This does not execute renderer lifecycle, pooling, articulation, shader/material swaps or game driving. Field/method tokens and assembly-version metadata are normalized out of method comparison; method signatures, opcode order, branches, locals and exception boundaries are compared.'
    }
    $report | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath $reportPath -Encoding utf8
    [pscustomobject]$report | Select-Object passed,version,checks,unchangedCompiledMethods,candidateSHA256,baselineSHA256 | ConvertTo-Json
} catch {
    [ordered]@{passed=$false;completedChecks=$checks.Count;results=$checks.ToArray();error=$_.Exception.ToString()} |
        ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $reportPath -Encoding utf8
    throw
} finally {
    foreach ($module in $modules) { $module.Dispose() }
}

