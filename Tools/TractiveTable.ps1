param(
    [Parameter(Mandatory=$true)][string]$InputPath,
    [string]$OutputPrefix
)
$ErrorActionPreference='Stop'
$inputFile=(Resolve-Path -LiteralPath $InputPath).Path
$report=Get-Content -LiteralPath $inputFile -Raw | ConvertFrom-Json
if (-not $report.tractiveReference -or $report.tractiveReference.schema -ne 1 -or -not $report.arrangements) {
    throw 'This report has no PHYS-A measurements. Use a structural run from the diagnostics build; older geometry-only captures cannot supply this table.'
}
if (-not $OutputPrefix) { $OutputPrefix=Join-Path (Split-Path $inputFile -Parent) 'tractive-table' }
$OutputPrefix=[IO.Path]::GetFullPath($OutputPrefix)
$parent=Split-Path $OutputPrefix -Parent
if (-not (Test-Path -LiteralPath $parent)) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }
function Number($value) {
    if ($null -eq $value -or $value -is [bool]) { return $null }
    $parsed=0d
    if (-not [double]::TryParse([Convert]::ToString($value,[Globalization.CultureInfo]::InvariantCulture),
        [Globalization.NumberStyles]::Float,[Globalization.CultureInfo]::InvariantCulture,[ref]$parsed) -or
        [double]::IsNaN($parsed) -or [double]::IsInfinity($parsed)) { return $null }
    return $parsed
}
function Nonnegative($value) { $number=Number $value; if ($null -ne $number -and $number -ge 0) { return $number }; return $null }
function FormatNumber($value,[string]$format='0.0') {
    $number=Number $value
    if ($null -eq $number) { return 'n/a' }
    return $number.ToString($format,[Globalization.CultureInfo]::InvariantCulture)
}
function Available($row) {
    if ($row.passed -ne $true -or $row.measurements.error) { return $null }
    $force=Nonnegative $row.measurements.referenceMeanForceN
    $grip=Nonnegative $row.measurements.referenceDryGripN
    if ($null -eq $force -or $null -eq $grip) { return $null }
    return [Math]::Min($force,$grip)
}
function Text($value) { return ([string]$value).Replace("`r",' ').Replace("`n",' ').Replace('|','/').Replace('[','(').Replace(']',')') }
$rows=@($report.arrangements)
if (@($rows | Group-Object id | Where-Object Count -GT 1).Count) { throw 'Duplicate arrangement IDs make this report ambiguous.' }
$baseline=@($rows | Where-Object id -EQ 10)
$stock=if($baseline.Count -eq 1){Available $baseline[0]}else{$null}
$hasStock=$null -ne $stock -and $stock -gt 0
$reference=$report.tractiveReference
$md=[Collections.Generic.List[string]]::new()
$bb=[Collections.Generic.List[string]]::new()
$md.Add('Arrangement pull and grip — measured configuration comparison')
$md.Add('')
$assumptions='Reference steam: '+(FormatNumber $reference.pressureBarAbs)+' bar absolute, cutoff '+
    (FormatNumber $reference.cutoff '0.00')+', temperature '+(FormatNumber $reference.temperatureC)+' C.'
$md.Add($assumptions); $md.Add('')
$bb.Add('[b]Arrangement pull and grip — measured configuration comparison[/b]');$bb.Add('');$bb.Add($assumptions);$bb.Add('')
$headers=@('Arrangement','Driver diameter m','Driven axles','Ref mean pull kN','Ref dry grip kN','Pull/grip','Stock index','Live grip kN','Notes')
$md.Add('| '+($headers -join ' | ')+' |');$md.Add('|'+(($headers|ForEach-Object{'---'})-join '|')+'|')
$bb.Add('[table]');$bb.Add('[tr]'+(($headers|ForEach-Object{'[th]'+$_+'[/th]'})-join '')+'[/tr]')
foreach($row in $rows) {
    $m=$row.measurements
    $valid=$row.passed -eq $true -and -not $m.error
    $force=if($valid){Nonnegative $m.referenceMeanForceN}else{$null}
    $grip=if($valid){Nonnegative $m.referenceDryGripN}else{$null}
    $ratio=if($null -ne $force -and $null -ne $grip -and $grip -gt 0){$force/$grip}else{$null}
    $available=Available $row
    $index=if($hasStock -and $null -ne $available){100*$available/$stock}else{$null}
    $radius=Nonnegative $m.driverRadiusM
    $live=Nonnegative $m.liveGripTotalN
    $notes=[Collections.Generic.List[string]]::new()
    if($row.passed -ne $true){$notes.Add('structural check failed')}
    if(-not $m){$notes.Add('measurement missing')}elseif($m.error){$notes.Add('measurement error')}
    if($m.experimental){$notes.Add('experimental clearance prototype')}
    if($m.adhesionReadError){$notes.Add('ECS adhesion unavailable')}
    if($m.tender){$notes.Add('tender separate')}
    $cells=@((Text $row.name),(FormatNumber $(if($null -ne $radius){2*$radius}else{$null}) '0.000'),
        (FormatNumber $m.poweredAxles '0'),(FormatNumber $(if($null -ne $force){$force/1000}else{$null})),
        (FormatNumber $(if($null -ne $grip){$grip/1000}else{$null})),(FormatNumber $ratio '0.00'),
        (FormatNumber $index),(FormatNumber $(if($null -ne $live){$live/1000}else{$null})),($notes -join '; '))
    $md.Add('| '+($cells -join ' | ')+' |');$bb.Add('[tr]'+(($cells|ForEach-Object{'[td]'+$_+'[/td]'})-join '')+'[/tr]')
}
$bb.Add('[/table]')
foreach($row in $rows | Where-Object { $_.measurements.tender }) {
    $t=$row.measurements.tender
    $note='Tender engine for '+(Text $row.name)+': attached '+([string]$t.attached)+
        '; reference mean pull '+(FormatNumber $(if($null -ne (Number $t.referenceMeanForceN)){(Number $t.referenceMeanForceN)/1000}else{$null}))+
        ' kN; sampled grip limit '+(FormatNumber $(if($null -ne (Number $t.limitN)){(Number $t.limitN)/1000}else{$null}))+ ' kN.'
    $md.Add('');$md.Add($note);$bb.Add('');$bb.Add($note)
}
$notes=@(
    'Reference mean pull is calculated from the native B99 mean-torque formula, including cutoff and condensation. It is not a measured start, power curve or guaranteed pull at speed.',
    'Dry reference grip uses captured current mass/resources, axle counts and read-back friction coefficient. It excludes live rail/sand effects. Live grip is a separate ECS snapshot and can be stale while stationary.',
    'Stock index = 100 x min(reference pull, reference dry grip) / the same value for this run''s stock 2-8-2. It is a relative force index, not tonnes or a 2% grade rating.',
    'Dormant auxiliary engines are excluded. Erie tender power and grip are separate and are not added to the locomotive index. The giant-driver single is an experimental clearance prototype.',
    'Structural tests use closed regulators. Changing boiler pressure, sampled admission, cutoff, torque and fuel values are observations, not controlled performance tests.'
)
if(-not $hasStock){$notes+='No valid positive stock reference was captured; all stock indices are unavailable.'}
if($rows.Count -ne 47){$notes+='Partial report: '+$rows.Count+' of 47 arrangements.'}
foreach($note in $notes){$md.Add('');$md.Add($note);$bb.Add('');$bb.Add($note)}
[IO.File]::WriteAllText($OutputPrefix+'.md',($md -join "`r`n")+"`r`n",[Text.UTF8Encoding]::new($false))
[IO.File]::WriteAllText($OutputPrefix+'.bbcode.txt',($bb -join "`r`n")+"`r`n",[Text.UTF8Encoding]::new($false))
[pscustomobject]@{markdown=$OutputPrefix+'.md';bbcode=$OutputPrefix+'.bbcode.txt';arrangements=$rows.Count;stockReferenceAvailable=$hasStock} | ConvertTo-Json
