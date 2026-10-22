param(
    [string]$AssemblyPath=(Join-Path $PSScriptRoot 'build\physics-a-1.17.0\bin\RearrangedS282.dll'),
    [string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\physics-a-1.17.0\table-validation')
)
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$checks=[Collections.Generic.List[string]]::new()
function Check([bool]$condition,[string]$message){if(-not $condition){throw $message};$checks.Add($message)}
Add-Type -Path (Join-Path $PSScriptRoot 'lib\AssetStudioModCLI_net472_win32_64\Mono.Cecil.dll')
$module=[Mono.Cecil.ModuleDefinition]::ReadModule((Resolve-Path -LiteralPath $AssemblyPath).Path)
try {
    $types=@($module.GetTypes())
    $observers=@($types | Where-Object { $_.FullName -like 'RearrangedS282.TractiveDiagnostics*' -or $_.FullName -like 'RearrangedS282.TractiveEffortModel*' })
    Check ($observers.Count -ge 2) 'Actual compiled diagnostic/model classes found'
    foreach($method in @($observers.Methods | Where-Object HasBody)) {
        foreach($instruction in $method.Body.Instructions) {
            if($instruction.OpCode.Name -in @('stfld','stsfld')) {
                $owner=$instruction.Operand.DeclaringType.FullName
                Check ($owner -like 'RearrangedS282.Tractive*') ('Diagnostic field stores owned by observer: '+$method.Name)
            }
            if($instruction.OpCode.Name -in @('call','callvirt')) {
                $called=$instruction.Operand
                if($called.DeclaringType.FullName -match '^(LocoSim\.|DV\.|UnityEngine\.|TrainCar$|TrainMassController$)') {
                    Check ($called.Name -notmatch '^(set_|Tick$|Simulate|SetComponentData|UpdateTrainCarMass|AddForce|Apply)' ) ('Observer does not mutate native state: '+$called.FullName)
                }
            }
        }
    }
    $snapshots=@(($types | Where-Object FullName -EQ 'RearrangedS282.Stage2RuntimeChecks').Methods | Where-Object Name -EQ 'Capture')
    Check ($snapshots.Count -eq 1 -and -not @($snapshots[0].Body.Instructions | Where-Object { $_.Operand.DeclaringType.FullName -eq 'RearrangedS282.TractiveDiagnostics' }).Count) 'Live measurements excluded from exact structural restoration snapshots'
    $runner=@($types | Where-Object FullName -Like 'RearrangedS282.Stage2RuntimeChecks/<RunAll>*')
    $calls=@($runner.Methods.Body.Instructions | Where-Object { $_.OpCode.Name -in @('call','callvirt') -and $_.Operand.DeclaringType.FullName -eq 'RearrangedS282.TractiveDiagnostics' })
    Check (@($calls | Where-Object { $_.Operand.Name -eq 'Capture' }).Count -eq 1) 'Structural coroutine captures separate per-arrangement observations'
    Check (@($calls | Where-Object { $_.Operand.Name -eq 'WriteStructuralCsv' }).Count -eq 1) 'Structural progress writes the CSV companion'
    $settings=$types | Where-Object FullName -EQ 'RearrangedS282.Settings'
    $ctor=$settings.Methods | Where-Object { $_.Name -eq '.ctor' -and $_.HasBody }
    foreach($pair in @(@('mogulExtraCoupledPitchMetres',.18),@('mogulDriverRearwardMetres',.12),
        @('twoFortyExtraCoupledPitchMetres',.20),@('twoFortyDriverRearwardMetres',.14),
        @('prairieExtraCoupledPitchMetres',.22),@('prairieDriverRearwardMetres',.12))) {
        $store=@($ctor.Body.Instructions | Where-Object { $_.OpCode.Name -eq 'stfld' -and $_.Operand.Name -eq $pair[0] })
        Check ($store.Count -eq 1 -and [Math]::Abs([double]$store[0].Previous.Operand-$pair[1]) -lt .000001) ('Compiled requested geometry default: '+$pair[0])
    }
} finally {$module.Dispose()}

function Row([int]$id,[string]$name,$force,$grip,[bool]$passed=$true) {
    return [ordered]@{id=$id;name=$name;passed=$passed;measurements=[ordered]@{driverRadiusM=.7;poweredAxles=3;
        referenceMeanForceN=$force;referenceDryGripN=$grip;liveGripTotalN=123000;experimental=($id -eq 36)}}
}
$data=[ordered]@{tractiveReference=@{schema=1;pressureBarAbs=15;cutoff=.75;temperatureC=300};arrangements=@(
    (Row 10 'stock' 200000 100000),(Row 34 'Mogul' 60000 200000),(Row 33 'unpowered' 0 0),
    (Row 36 'giant' 400000 200000),(Row 43 'Erie' 120000 100000),(Row 35 'failed' 1e9 1e9 $false),
    (Row 44 'missing grip' 100000 $null))}
$data.arrangements[4].measurements.tender=@{attached=$true;referenceMeanForceN=999000;limitN=80000}
$fixtureInput=Join-Path $OutputDirectory 'fixture.json';$data | ConvertTo-Json -Depth 15 | Set-Content -LiteralPath $fixtureInput -Encoding utf8
$hash=(Get-FileHash -LiteralPath $fixtureInput).Hash
$tool=Join-Path $PSScriptRoot 'Tools\TractiveTable.ps1'
$prefix=Join-Path $OutputDirectory 'comparison'
$priorCulture=[Globalization.CultureInfo]::CurrentCulture
try{[Globalization.CultureInfo]::CurrentCulture=[Globalization.CultureInfo]::GetCultureInfo('fr-FR');$null=& $tool -InputPath $fixtureInput -OutputPrefix $prefix}
finally{[Globalization.CultureInfo]::CurrentCulture=$priorCulture}
$md=Get-Content -LiteralPath ($prefix+'.md') -Raw
$bb=Get-Content -LiteralPath ($prefix+'.bbcode.txt') -Raw
Check ($md -match '\| stock \|.*\| 2\.00 \| 100\.0 \|') 'Stock force index is exactly 100 with invariant decimals'
Check ($md -match '\| Mogul \|.*\| 0\.30 \| 60\.0 \|') 'Mogul index uses smaller pull rather than grip'
Check ($md -match '\| unpowered \|.*\| n/a \| 0\.0 \|') 'Known zero force gives zero index without dividing by zero grip'
Check ($md -match '\| giant \|.*\| 200\.0 \|.*experimental clearance prototype') 'Prototype flagged and larger available force indexed correctly'
Check ($md -match '\| Erie \|.*\| 1\.20 \| 100\.0 \|') 'Separate tender power does not inflate locomotive index'
Check ($md -match '\| failed \|.*n/a.*structural check failed') 'Failed structural rows cannot advertise calculated force'
Check ($md -match '\| missing grip \|.*\| n/a \| n/a \|') 'Unknown grip never becomes zero or a usable index'
Check ($md -match 'Partial report: 7 of 47' -and $md -match 'not tonnes or a 2% grade rating') 'Incomplete data and force-index limits disclosed'
Check ($bb.Contains('[table]') -and $bb.Contains('[/table]') -and ([regex]::Matches($bb,'\[tr\]').Count -eq 8)) 'BBCode has one header and every data row'
Check ((Get-FileHash -LiteralPath $fixtureInput).Hash -eq $hash) 'Table generator does not modify input evidence'
$data.arrangements=$data.arrangements | Where-Object id -NE 10
$data | ConvertTo-Json -Depth 15 | Set-Content -LiteralPath $fixtureInput -Encoding utf8
$null=& $tool -InputPath $fixtureInput -OutputPrefix (Join-Path $OutputDirectory 'without-stock')
Check ((Get-Content -LiteralPath (Join-Path $OutputDirectory 'without-stock.md') -Raw).Contains('No valid positive stock reference')) 'Missing stock reference disables all indices'
$data.arrangements+=@($data.arrangements[0]);$data|ConvertTo-Json -Depth 15|Set-Content -LiteralPath $fixtureInput -Encoding utf8
$rejected=$false;try{$null=& $tool -InputPath $fixtureInput -OutputPrefix (Join-Path $OutputDirectory 'duplicate')}catch{$rejected=$_.Exception.Message -like '*Duplicate arrangement*'}
Check $rejected 'Duplicate arrangement IDs rejected rather than choosing an arbitrary reference'
'{"arrangements":[{"id":10,"passed":true}]}' | Set-Content -LiteralPath $fixtureInput -Encoding utf8
$rejected=$false;try{$null=& $tool -InputPath $fixtureInput -OutputPrefix (Join-Path $OutputDirectory 'legacy')}catch{$rejected=$_.Exception.Message -like '*no PHYS-A*'}
Check $rejected 'Older structural evidence cannot be misrepresented as new measurements'
$result=[ordered]@{passed=$true;checks=$checks.Count;results=$checks;dllSHA256=(Get-FileHash -LiteralPath $AssemblyPath).Hash;
    limitations='Compiled observation/wiring checks and real table exporter with synthetic numeric fixtures. No live ECS timing, steam/rail performance or UI acceptance.'}
$result | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'validation.json') -Encoding utf8
[pscustomobject]@{passed=$true;checks=$checks.Count;report=(Join-Path $OutputDirectory 'validation.json')}|ConvertTo-Json
