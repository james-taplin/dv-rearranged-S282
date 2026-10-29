param(
    [string]$Version = '2.0.8-alpha',
    [string]$GamePath = 'B:\SteamLibrary\steamapps\common\Derail Valley',
    [string]$MultiplayerApiPath = 'B:\GitHub repo\dads-decals\lib\MultiplayerAPI.dll',
    [string]$SeedPackage = 'B:\GitHub repo\rearranged-s282-b99\release\2.0-beta\RearrangedS282-B99-2.0-beta.zip',
    [string]$OutputDirectory,
    [string]$CandidateDirectory
)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
if ($Version -notmatch '^\d+\.\d+\.\d+-(?:alpha|mp\.alpha\.\d+)$') { throw 'Use an explicit multiplayer alpha version.' }
if (-not $OutputDirectory) { $OutputDirectory = "C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-$Version-Test" }
if (-not $CandidateDirectory) { $CandidateDirectory = Join-Path $root "build\multiplayer-$Version" }
$seedHash = '48A4C061CE75423B6DE0760AE76DC652521940B93520111856E585580E4760C0'
if ((Get-FileHash -LiteralPath $SeedPackage -Algorithm SHA256).Hash -ne $seedHash) { throw 'Audited 2.0-beta seed package changed.' }
if (-not (Test-Path -LiteralPath $MultiplayerApiPath -PathType Leaf)) { throw 'Build-only MultiplayerAPI.dll is missing.' }
$candidate = [IO.Path]::GetFullPath($CandidateDirectory)
$mainBin = Join-Path $candidate 'main-bin'
$bridgeBin = Join-Path $candidate 'bridge-bin'
$dotnet = 'C:\Program Files\dotnet\dotnet.exe'
& $dotnet build (Join-Path $root 'RearrangedS282\RearrangedS282.csproj') -c Release '-p:Stage=3' "-p:Version=$Version" "-p:GamePath=$GamePath" -o $mainBin
if ($LASTEXITCODE -ne 0) { throw 'Multiplayer core build failed.' }
& $dotnet build (Join-Path $root 'RearrangedS282.Multiplayer\RearrangedS282.Multiplayer.csproj') -c Release '-p:Stage=3' "-p:Version=$Version" "-p:GamePath=$GamePath" "-p:MultiplayerApiPath=$MultiplayerApiPath" -o $bridgeBin
if ($LASTEXITCODE -ne 0) { throw 'Optional multiplayer bridge build failed.' }
$staging = Join-Path $candidate ('package-' + [Guid]::NewGuid().ToString('N'))
Expand-Archive -LiteralPath $SeedPackage -DestinationPath $staging
$mod = Join-Path $staging 'RearrangedS282'
Copy-Item -LiteralPath (Join-Path $mainBin 'RearrangedS282.dll') -Destination $mod -Force
Copy-Item -LiteralPath (Join-Path $bridgeBin 'RearrangedS282.Multiplayer.dll') -Destination $mod -Force
$infoPath = Join-Path $mod 'Info.json'
$info = Get-Content -LiteralPath (Join-Path $root 'Info.json') -Raw | ConvertFrom-Json
$info.Version = $Version
$info.DisplayName = 'Rearranged S282 - B99 Multiplayer Alpha'
$info.LoadAfter = @($info.LoadAfter) + @('Multiplayer') | Select-Object -Unique
$info | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $infoPath -Encoding utf8
if ($info.MultiplayerCompatibility -ne 'All' -or 'Multiplayer' -notin $info.LoadAfter -or 'LocoMeshSplitter' -notin $info.Requirements) { throw 'Multiplayer manifest contract is incomplete.' }
Copy-Item -LiteralPath (Join-Path $root 'MULTIPLAYER_TESTING.txt') -Destination $mod -Force
$readmePath = Join-Path $mod 'README.txt'
$baseReadme = Get-Content -LiteralPath $readmePath -Raw
$alphaReadme = @"
Rearranged S282 B99 $Version multiplayer test

This is an initial multiplayer compatibility package for external testers.
Read MULTIPLAYER_TESTING.txt before testing. Every participant needs this exact
package and the same supported Multiplayer mod version. Multiplayer is optional
for ordinary single-player use. MultiplayerAPI.dll is supplied by Multiplayer;
this package does not install or replace Multiplayer itself.

The code compiles and has offline checks. Live host/client physics, visuals,
audio, saves and reconnection have not been validated on this development PC.
Record the exact mod versions, session role and logs with each result.

The existing 2.0-beta instructions follow.

"@
Set-Content -LiteralPath $readmePath -Value ($alphaReadme + $baseReadme) -Encoding ascii
$changelogPath = Join-Path $mod 'CHANGELOG.txt'
$baseChangelog = Get-Content -LiteralPath $changelogPath -Raw
$alphaChangelog = @"
Rearranged S282 B99 $Version

Initial multiplayer compatibility alpha. All players can request arrangement
changes through their own radios. The host validates changes, distributes the
accepted arrangement and relevant settings, and controls player access. Late
join and subsequent spawns receive the host's current arrangements. Native
Multiplayer remains responsible for train motion and simulation replication.
This package requires external multiplayer testing; see MULTIPLAYER_TESTING.txt.

"@
Set-Content -LiteralPath $changelogPath -Value ($alphaChangelog + $baseChangelog) -Encoding ascii

# Compare every original entry outside the explicit runtime/document changes.
Add-Type -AssemblyName System.IO.Compression.FileSystem
$changed = @('RearrangedS282/RearrangedS282.dll','RearrangedS282/Info.json','RearrangedS282/README.txt','RearrangedS282/CHANGELOG.txt')
$unchanged = 0
$archive = [IO.Compression.ZipFile]::OpenRead($SeedPackage)
try {
    foreach ($entry in $archive.Entries) {
        if (-not $entry.Name -or $entry.FullName -in $changed) { continue }
        $target = Join-Path $staging $entry.FullName
        if (-not (Test-Path -LiteralPath $target -PathType Leaf)) { throw "Missing seed member: $($entry.FullName)" }
        $stream = $entry.Open()
        $sha = [Security.Cryptography.SHA256]::Create()
        try { $entryHash = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','') }
        finally { $sha.Dispose(); $stream.Dispose() }
        if ((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -ne $entryHash) { throw "Protected seed member changed: $($entry.FullName)" }
        $unchanged++
    }
}
finally { $archive.Dispose() }
$rootDlls = @(Get-ChildItem -LiteralPath $mod -File -Filter '*.dll' | Select-Object -ExpandProperty Name)
$rootDllNames = ($rootDlls | Sort-Object) -join ','
if ($rootDllNames -ne 'RearrangedS282.dll,RearrangedS282.Multiplayer.dll' -and $rootDllNames -ne 'RearrangedS282.Multiplayer.dll,RearrangedS282.dll') { throw 'Unexpected runtime DLL in package root.' }
if (Get-ChildItem -LiteralPath $mod -File -Recurse -Filter 'MultiplayerAPI.dll') { throw 'Build-only Multiplayer API must not be bundled.' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$zip = Join-Path $OutputDirectory "RearrangedS282-B99-$Version-Test.zip"
Compress-Archive -LiteralPath $mod -DestinationPath $zip -Force
Copy-Item -LiteralPath (Join-Path $root 'MULTIPLAYER_TESTING.txt') -Destination $OutputDirectory -Force
$commit = & git -C $root rev-parse HEAD
if ($LASTEXITCODE -ne 0) { throw 'Could not record source commit.' }
$dirty = @(& git -C $root status --porcelain).Count -gt 0
$receipt = [ordered]@{
    version = $Version
    utc = [DateTime]::UtcNow.ToString('O')
    sourceCommit = [string]$commit
    sourceHasUncommittedChanges = $dirty
    installed = $false
    runtimeValidated = $false
    seedSHA256 = $seedHash
    protectedSeedMembersUnchanged = $unchanged
    multiplayerApiBuildOnly = $true
    multiplayerApiSHA256 = (Get-FileHash -LiteralPath $MultiplayerApiPath -Algorithm SHA256).Hash
    dllSHA256 = (Get-FileHash -LiteralPath (Join-Path $mod 'RearrangedS282.dll') -Algorithm SHA256).Hash
    bridgeSHA256 = (Get-FileHash -LiteralPath (Join-Path $mod 'RearrangedS282.Multiplayer.dll') -Algorithm SHA256).Hash
    zipSHA256 = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash
    package = $mod
    zip = $zip
    sourceHashes = @(Get-ChildItem -LiteralPath (Join-Path $root 'RearrangedS282'),(Join-Path $root 'RearrangedS282.Multiplayer') -Recurse -File -Filter '*.cs' | Where-Object FullName -NotMatch '\\(bin|obj)\\' | Get-FileHash -Algorithm SHA256 | Select-Object Path,Hash)
}
$receipt | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $candidate 'build-receipt.json') -Encoding utf8
$receipt | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'build-receipt.json') -Encoding utf8
$receipt | ConvertTo-Json -Depth 6
