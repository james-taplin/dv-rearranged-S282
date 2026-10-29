param(
    [string]$UnityEditor = 'B:\Games\Unity 2019.4.40f1\Editor\Unity.exe',
    [string]$GamePath = 'B:\SteamLibrary\steamapps\common\Derail Valley',
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'build\native-slip-world\runs'),
    [switch]$CompileOnly,
    [switch]$EditorMode = $true,
    [switch]$PlayerMode = $true
)
$ErrorActionPreference = 'Stop'
$run = Join-Path $OutputDirectory ([DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fff'))
$fixture = Join-Path $run 'fixture'
New-Item -ItemType Directory -Force -Path (Join-Path $fixture 'Assets\Editor'),(Join-Path $fixture 'Packages'),(Join-Path $fixture 'ProjectSettings') | Out-Null
$managed = Join-Path $GamePath 'DerailValley_Data\Managed'
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'evidence\native-slip-world\SlipWorldBootstrap.cs') -Destination (Join-Path $fixture 'Assets\Editor')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'evidence\native-slip-world\GroupAnimationFixtureEditor.cs') -Destination (Join-Path $fixture 'Assets\Editor')
if ($PlayerMode) {
    $bootstrap=Join-Path $fixture 'Assets\NativeProbeBootstrap'
    New-Item -ItemType Directory -Force -Path $bootstrap|Out-Null
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'evidence\native-slip-world\SlipWorldPlayerBootstrap.cs') -Destination $bootstrap
    '{"name":"S282.NativeSlipBootstrap","references":[],"autoReferenced":true}'|Set-Content (Join-Path $bootstrap 'S282.NativeSlipBootstrap.asmdef') -Encoding utf8
}
'{"dependencies":{"com.unity.modules.physics":"1.0.0","com.unity.modules.jsonserialize":"1.0.0","com.unity.modules.animation":"1.0.0"}}' | Set-Content (Join-Path $fixture 'Packages\manifest.json') -Encoding utf8
'm_EditorVersion: 2019.4.40f1' | Set-Content (Join-Path $fixture 'ProjectSettings\ProjectVersion.txt') -Encoding utf8
[IO.File]::WriteAllText((Join-Path $run 'managed-path.txt'),$managed)
& 'C:\Program Files\dotnet\dotnet.exe' build (Join-Path $PSScriptRoot 'evidence\native-slip-world\NativeSlipWorldProbe.csproj') -c Release "-p:ManagedPath=$managed" -o (Join-Path $run 'probe-bin') --ignore-failed-sources
if ($LASTEXITCODE -ne 0) { throw 'Native World probe compilation failed.' }
$receipt = [ordered]@{ status='compiled_only'; run=$run; runtimeExecuted=$false; vehicleForcesApplied=$false; runtimeContext=$(if($PlayerMode){'release_player'}else{'editor_play_mode'}); editorExitCode=$null; playerExitCode=$null; nativeSystemsSHA256=(Get-FileHash (Join-Path $managed 'Assembly-CSharp.dll')).Hash; probeSourceSHA256=(Get-FileHash (Join-Path $PSScriptRoot 'evidence\native-slip-world\NativeSlipWorldProbe.cs')).Hash; schemaSourceSHA256=(Get-FileHash (Join-Path $PSScriptRoot 'RearrangedS282\NativeSlipEntitySchema.cs')).Hash; editorBootstrapSHA256=(Get-FileHash (Join-Path $PSScriptRoot 'evidence\native-slip-world\SlipWorldBootstrap.cs')).Hash; playerBootstrapSHA256=$(if($PlayerMode){(Get-FileHash (Join-Path $PSScriptRoot 'evidence\native-slip-world\SlipWorldPlayerBootstrap.cs')).Hash}else{$null}) }
$receiptPath=Join-Path $run 'receipt.json'
$receipt|ConvertTo-Json|Set-Content $receiptPath -Encoding utf8
if ($CompileOnly) { Write-Output $receiptPath; return }
$log=Join-Path $run 'unity.log'
$mode=if($EditorMode){''}else{'-batchmode -nographics '}
$entry=if($PlayerMode){'SlipWorldBootstrap.BuildPlayer'}else{'SlipWorldBootstrap.Start'}
$arguments=$mode+'-projectPath "'+$fixture+'" -executeMethod '+$entry+' -logFile "'+$log+'"'
$process=Start-Process -FilePath $UnityEditor -ArgumentList $arguments -WindowStyle Hidden -PassThru
if (-not $process.WaitForExit(180000)) {
    Stop-Process -Id $process.Id -ErrorAction SilentlyContinue
    $receipt.status='editor_timeout'; $receipt|ConvertTo-Json|Set-Content $receiptPath -Encoding utf8
    throw "Isolated Unity startup/test timed out. Inspect $log"
}
$receipt.editorExitCode=$process.ExitCode
$result=Join-Path $run 'result.json'
if ($PlayerMode -and (Test-Path -LiteralPath (Join-Path $run 'player\NativeSlipWorld.exe'))) {
    $playerLog=Join-Path $run 'player.log'
    $playerProcess=Start-Process -FilePath (Join-Path $run 'player\NativeSlipWorld.exe') -ArgumentList ('-batchmode -nographics -logFile "'+$playerLog+'"') -WindowStyle Hidden -PassThru
    if(-not $playerProcess.WaitForExit(90000)) {
        Stop-Process -Id $playerProcess.Id -ErrorAction SilentlyContinue
        $receipt.status='player_timeout'; $receipt|ConvertTo-Json|Set-Content $receiptPath -Encoding utf8
        throw "Private Unity player timed out. Inspect $playerLog"
    }
    $receipt.playerExitCode=$playerProcess.ExitCode
}
if (Test-Path -LiteralPath $result) {
    $report=Get-Content -LiteralPath $result -Raw|ConvertFrom-Json
    $receipt.runtimeExecuted=$true
    $cleanExit=$receipt.editorExitCode -eq 0 -and (-not $PlayerMode -or $receipt.playerExitCode -eq 0)
    $receipt.status=if($report.passed -and $cleanExit){'fixture_passed'}else{'fixture_failed'}
} else {
    $text=if(Test-Path -LiteralPath $log){Get-Content -LiteralPath $log -Raw}else{''}
    $receipt.status=if($text -match 'not been activated with a valid License|Failed to activate/update license'){'blocked_editor_license'}else{'failed_before_fixture'}
}
$receipt|ConvertTo-Json|Set-Content $receiptPath -Encoding utf8
if ($receipt.status -ne 'fixture_passed') { throw "Native World fixture did not pass: $($receipt.status). Inspect $receiptPath and $log" }
Write-Output $result
