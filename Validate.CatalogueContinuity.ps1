param([string]$OutputPath=(Join-Path $PSScriptRoot 'build\offline-validation\CatalogueContinuity\validation.json'))
$ErrorActionPreference='Stop'
$baseline='85855dc'
function PriorSource([string]$path) {
    $source=(& git -C $PSScriptRoot show "${baseline}:$path") -join "`n"
    if ($LASTEXITCODE -ne 0 -or -not $source) { throw "Missing catalogue baseline $baseline for $path" }
    return $source.Replace('namespace RearrangedS282','namespace PriorS282')
}
$old=(PriorSource 'RearrangedS282/WheelArrangement.cs')+"`n"+(PriorSource 'RearrangedS282/PoweredTenderPolicy.cs')
$current=(Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RearrangedS282\WheelArrangement.cs') -Raw)+"`n"+(Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RearrangedS282\PoweredTenderPolicy.cs') -Raw)
Add-Type -TypeDefinition $old -CompilerOptions '/define:B99_ALL_STANDARD'
Add-Type -TypeDefinition $current -CompilerOptions '/define:B99_ALL_STANDARD'
$flags=[System.Reflection.BindingFlags]'NonPublic,Static'
$oldTender=[PriorS282.WheelArrangement].Assembly.GetType('PriorS282.PoweredTenderPolicy').GetMethod('Supports',$flags)
$newTender=[RearrangedS282.WheelArrangement].Assembly.GetType('RearrangedS282.PoweredTenderPolicy').GetMethod('Supports',$flags)
$checks=0
function Check([bool]$ok,[string]$message) { $script:checks++; if(-not $ok){throw $message} }
Check ([PriorS282.WheelArrangement]::SupportedCount -eq 49) 'Baseline must contain 49 IDs'
Check ([RearrangedS282.WheelArrangement]::SupportedCount -eq 51) 'Candidate must contain 51 IDs'
foreach($id in 0..48) {
    Check ([PriorS282.WheelArrangementType]::IsDefined([PriorS282.WheelArrangementType],$id) -and [RearrangedS282.WheelArrangementType]::IsDefined([RearrangedS282.WheelArrangementType],$id)) "ID $id is missing"
    Check (([PriorS282.WheelArrangementType]$id).ToString() -ceq ([RearrangedS282.WheelArrangementType]$id).ToString()) "Saved enum mapping changed for ID $id"
    Check ([PriorS282.WheelArrangement]::WheelArrangementNames[$id] -ceq [RearrangedS282.WheelArrangement]::WheelArrangementNames[$id]) "Saved name changed for ID $id"
    Check (([PriorS282.WheelArrangement]::LocomotiveWheels($id) -join ',') -ceq ([RearrangedS282.WheelArrangement]::LocomotiveWheels($id) -join ',')) "Wheel groups changed for ID $id"
    Check ([PriorS282.WheelArrangement]::NumOfNondrivenWheels[$id] -eq [RearrangedS282.WheelArrangement]::NumOfNondrivenWheels[$id]) "Carrying wheel count changed for ID $id"
    Check ([PriorS282.WheelArrangement]::IsConventionalId($id) -eq [RearrangedS282.WheelArrangement]::IsConventionalId($id)) "Conventional classification changed for ID $id"
    Check ([PriorS282.WheelArrangement]::IsArticulatedId($id) -eq [RearrangedS282.WheelArrangement]::IsArticulatedId($id)) "Articulated classification changed for ID $id"
    Check ($oldTender.Invoke($null,@($id)) -eq $newTender.Invoke($null,@($id))) "Powered-tender eligibility changed for ID $id"
}
Check (([RearrangedS282.WheelArrangementType]49).ToString() -ceq 's462Scotch') 'ID 49 is not Scotch Glider'
Check (([RearrangedS282.WheelArrangementType]50).ToString() -ceq 's4664LittleBoy') 'ID 50 is not Little Boy'
Check (([RearrangedS282.WheelArrangement]::LocomotiveWheels(49) -join '-') -ceq '4-6-2') 'ID 49 wheel groups changed'
Check (([RearrangedS282.WheelArrangement]::LocomotiveWheels(50) -join '-') -ceq '4-6-6-4') 'ID 50 wheel groups changed'
Check ($newTender.Invoke($null,@(49)) -and -not $newTender.Invoke($null,@(50))) 'New tender eligibility does not match the requested policy'
New-Item -ItemType Directory -Path (Split-Path -Parent $OutputPath) -Force | Out-Null
[ordered]@{passed=$true;checks=$checks;baselineCommit=$baseline;unchangedIds='0-48';newIds=@(49,50);candidateSourceSHA256=(Get-FileHash (Join-Path $PSScriptRoot 'RearrangedS282\WheelArrangement.cs') -Algorithm SHA256).Hash;limitations='Catalogue source comparison; does not execute physical bogies, save files or Unity gameplay.'} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $OutputPath -Encoding utf8
Get-Content -LiteralPath $OutputPath -Raw
