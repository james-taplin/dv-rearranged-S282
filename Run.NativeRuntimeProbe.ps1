param(
    [Parameter(Mandatory=$true)][string]$ProbeDirectory,
    [Parameter(Mandatory=$true)][string]$AssemblyName,
    [Parameter(Mandatory=$true)][string]$TypeName,
    [Parameter(Mandatory=$true)][string]$OutputDirectory,
    [string]$ExpectedFailureText,
    [string]$PlayerTemplate = (Join-Path $PSScriptRoot 'build/native-drive-calibration/player'),
    [string]$GamePath = 'B:\SteamLibrary\steamapps\common\Derail Valley'
)
$ErrorActionPreference = 'Stop'
$run = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $run) { throw "Use a fresh output directory: $run" }
New-Item -ItemType Directory -Path $run | Out-Null
Copy-Item -LiteralPath $PlayerTemplate -Destination (Join-Path $run 'player') -Recurse
Copy-Item -LiteralPath $ProbeDirectory -Destination (Join-Path $run 'probe-bin') -Recurse
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'build/runtime-bootstrap/bin/S282.NativeSlipBootstrap.dll') -Destination (Join-Path $run 'player/NativeSlipWorld_Data/Managed/S282.NativeSlipBootstrap.dll') -Force
[IO.File]::WriteAllText((Join-Path $run 'managed-path.txt'), (Join-Path $GamePath 'DerailValley_Data/Managed'))
[IO.File]::WriteAllLines((Join-Path $run 'probe.txt'), @($AssemblyName, $TypeName))
$process = Start-Process -FilePath (Join-Path $run 'player/NativeSlipWorld.exe') -WorkingDirectory $run -ArgumentList '-batchmode -nographics -logFile player.log' -WindowStyle Hidden -PassThru -Wait
$resultPath = Join-Path $run 'result.json'
$errorPath = Join-Path $run 'bootstrap-error.txt'
if ($ExpectedFailureText) {
    if ($process.ExitCode -eq 0 -or -not (Test-Path -LiteralPath $errorPath) -or
        -not ([IO.File]::ReadAllText($errorPath).Contains($ExpectedFailureText))) {
        throw 'Native probe did not reproduce the expected baseline failure.'
    }
} else {
    if ($process.ExitCode -ne 0 -or (Test-Path -LiteralPath $errorPath) -or -not (Test-Path -LiteralPath $resultPath)) {
        throw "Native probe failed; inspect $run"
    }
    $result = Get-Content -LiteralPath $resultPath -Raw | ConvertFrom-Json
    if (($null -ne $result.PSObject.Properties['passed'] -and -not $result.passed) -or
        ($null -ne $result.PSObject.Properties['complete'] -and -not $result.complete)) {
        throw "Native probe result is incomplete or failed: $resultPath"
    }
}
[PSCustomObject]@{ RunDirectory=$run; ProcessId=$process.Id; ExitCode=$process.ExitCode; ExpectedFailure=[bool]$ExpectedFailureText; ResultPresent=(Test-Path -LiteralPath $resultPath) } | ConvertTo-Json
