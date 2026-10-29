param(
    [string]$Version = '2.0.8-alpha',
    [string]$ReleaseName = '',
    [string]$DesktopDirectory = 'C:\Users\james\Desktop\Derail Valley Mods',
    [string]$CandidateDirectory
)
# Repackages the runtime-trial build of $Version as a player-facing Nexus archive:
# same DLL, bundle, exporter and licences; test-only notes replaced by README,
# CHANGELOG and CREDITS from release-copy\$ReleaseName. Uploads nothing.
$ErrorActionPreference = 'Stop'
if (-not $ReleaseName) { $ReleaseName = $Version }
if ($Version -notmatch '^\d+\.\d+(?:\.\d+)?(?:-[a-zA-Z][a-zA-Z0-9.-]*)?$' -or $ReleaseName -notmatch '^\d+\.\d+(?:\.\d+)?(?:-[a-zA-Z][a-zA-Z0-9.-]*)?$') { throw 'Invalid version or release name.' }
$root = $PSScriptRoot
$candidate = if ($CandidateDirectory) { $CandidateDirectory } else { Join-Path $root "build\multiplayer-$Version" }
$receipt = Get-Content -LiteralPath (Join-Path $candidate 'build-receipt.json') -Raw | ConvertFrom-Json
$source = $receipt.package
if (-not (Test-Path -LiteralPath (Join-Path $source 'RearrangedS282.dll'))) { throw "Built package not found: $source" }
if ((Get-FileHash -LiteralPath (Join-Path $source 'RearrangedS282.dll')).Hash -ne $receipt.dllSHA256) { throw 'Built DLL differs from its receipt.' }
$multiplayer = $null -ne $receipt.PSObject.Properties['bridgeSHA256']
if ($multiplayer -and (Get-FileHash -LiteralPath (Join-Path $source 'RearrangedS282.Multiplayer.dll')).Hash -ne $receipt.bridgeSHA256) { throw 'Built multiplayer adapter differs from its receipt.' }
$copy = Join-Path $root "release-copy\$ReleaseName"
$plainReadme = Get-Content -LiteralPath (Join-Path $copy 'README.txt') -Raw
if ($plainReadme -match '(?m)^#{1,6}\s|^\|.*\|\s*$|\*\*|`|\[[^\]]+\]\(https?://|\[/?(?:b|i|url|list)(?:=[^\]]*)?\]') { throw 'Packaged README.txt must be plain text without Markdown or BBCode.' }
$out = Join-Path $root "release\$ReleaseName"
New-Item -ItemType Directory -Path $out -Force | Out-Null
$stage = Join-Path $root ('build\nexus-staging-' + [Guid]::NewGuid().ToString('N'))
$mod = Join-Path $stage 'RearrangedS282'
New-Item -ItemType Directory -Path $stage | Out-Null
Copy-Item -LiteralPath $source -Destination $mod -Recurse
foreach ($test in 'RUNTIME_TRIAL_TESTING.txt','MANUAL_TEST_CHECKLIST.txt','README_BETA.txt','ARTICULATED_TEST_STATUS.txt')
{ Remove-Item -LiteralPath (Join-Path $mod $test) -ErrorAction SilentlyContinue }
foreach ($doc in 'README.txt','CHANGELOG.txt','CREDITS.txt','UMM_OPTIONS.md') { Copy-Item -LiteralPath (Join-Path $copy $doc) -Destination $mod -Force }
$infoPath = Join-Path $mod 'Info.json'
$info = Get-Content -LiteralPath $infoPath -Raw | ConvertFrom-Json
if ($info.Version -ne $Version) { throw "Info.json version $($info.Version) is not $Version." }
$info.DisplayName = if ($multiplayer) { 'Rearranged S282 - B99 Multiplayer Alpha' } elseif ($ReleaseName -match '-beta$') { 'Rearranged S282 - B99 Beta' } else { 'Rearranged S282 - B99' }
if ($multiplayer -and ($info.MultiplayerCompatibility -ne 'All' -or 'Multiplayer' -in $info.Requirements)) { throw 'Multiplayer must be compatible on all peers and optional for single player.' }
$info | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $infoPath -Encoding utf8
$allowedObj = @('assets/franklin_casing.obj','assets/steam_manifold.obj','assets/steam_transfer.obj')
$modBase = [IO.Path]::GetFullPath($mod).TrimEnd([char]92) + [char]92
$bad = Get-ChildItem -LiteralPath $mod -Recurse -File | Where-Object {
    $relative = $_.FullName.Substring($modBase.Length).Replace([char]92, [char]47)
    $_.Name -match '\.(log|cache|pdb|jsonl|csv)$|^Settings\.xml$|stage2-runtime' -or
        ($_.Extension -eq '.obj' -and $relative -notin $allowedObj)
}
if ($bad) { throw "Unexpected files: $($bad.FullName -join ', ')" }
foreach ($need in 'RearrangedS282.dll','Info.json','LICENSE','THIRD_PARTY_NOTICES.txt','assets\s282_drivetrain','AssetStudioModCLI_net472_win64_contained\AssetStudioModCLI.exe')
{ if (-not (Test-Path -LiteralPath (Join-Path $mod $need))) { throw "Missing $need" } }
$zipName = "RearrangedS282-B99-$ReleaseName.zip"
$zip = Join-Path $out $zipName
# Standard forward-slash entry names (Windows PowerShell's Compress-Archive writes backslashes).
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }
$writer = [IO.Compression.ZipFile]::Open($zip, [IO.Compression.ZipArchiveMode]::Create)
try {
    $base = [IO.Path]::GetFullPath($stage).TrimEnd([char]92) + [char]92
    foreach ($file in Get-ChildItem -LiteralPath $mod -Recurse -File | Sort-Object FullName) {
        $name = $file.FullName.Substring($base.Length).Replace([char]92, [char]47)
        [void][IO.Compression.ZipFileExtensions]::CreateEntryFromFile($writer, $file.FullName, $name, [IO.Compression.CompressionLevel]::Optimal)
    }
} finally { $writer.Dispose() }
# The only recursive removal is our generated staging directory inside this repo.
$resolvedStage = (Resolve-Path -LiteralPath $stage).Path
$buildBoundary = [IO.Path]::GetFullPath((Join-Path $root 'build')).TrimEnd([char]92) + [char]92
if (-not $resolvedStage.StartsWith($buildBoundary, [StringComparison]::OrdinalIgnoreCase) -or
    [IO.Path]::GetFileName($resolvedStage) -notmatch '^nexus-staging-[a-f0-9]{32}$') { throw "Unsafe staging cleanup target: $resolvedStage" }
Remove-Item -LiteralPath $resolvedStage -Recurse -Force
$zipHash = (Get-FileHash -LiteralPath $zip).Hash
"$zipHash  $zipName`r`n$($receipt.dllSHA256)  RearrangedS282/RearrangedS282.dll" | Set-Content -LiteralPath (Join-Path $out 'CHECKSUMS.txt') -Encoding ascii
if ($multiplayer) { "$($receipt.bridgeSHA256)  RearrangedS282/RearrangedS282.Multiplayer.dll" | Add-Content -LiteralPath (Join-Path $out 'CHECKSUMS.txt') -Encoding ascii }
$publicDocs = @('NEXUS_RELEASE_COPY.txt','DESCRIPTION.md','CREDITS.md','CREDITS.txt','CHANGELOG.txt','FILE_INFO.txt','UMM_OPTIONS.md','README.txt')
$publicDocs += @('DESCRIPTION_BBCODE.txt','CREDITS_BBCODE.txt','NEXUS_DESCRIPTION_BBCODE.txt') | Where-Object { Test-Path -LiteralPath (Join-Path $copy $_) }
foreach ($doc in $publicDocs) { Copy-Item -LiteralPath (Join-Path $copy $doc) -Destination $out -Force }
$desk = Join-Path $DesktopDirectory "RearrangedS282-B99-$ReleaseName-Nexus"
New-Item -ItemType Directory -Path $desk -Force | Out-Null
Copy-Item -LiteralPath $zip,(Join-Path $out 'CHECKSUMS.txt') -Destination $desk -Force
foreach ($destination in @($out,$desk)) { Copy-Item -LiteralPath (Join-Path $candidate 'build-receipt.json') -Destination $destination -Force }
foreach ($doc in $publicDocs) { Copy-Item -LiteralPath (Join-Path $out $doc) -Destination $desk -Force }
$archive = [IO.Compression.ZipFile]::OpenRead($zip)
try { $files = @($archive.Entries | Where-Object Name -NE '' | ForEach-Object FullName) } finally { $archive.Dispose() }
if ($files | Where-Object { $_ -match '\\' -or -not $_.StartsWith('RearrangedS282/') }) { throw 'Archive entries must be RearrangedS282/... with forward slashes.' }
$manifest = [ordered]@{
    version=$Version; sourceCommit=$receipt.sourceCommit; sourceHasUncommittedChanges=$receipt.sourceHasUncommittedChanges
    branch='codex/s282-multiplayer'; multiplayerAlpha=$multiplayer; optionalDownload=$true
    zip=$zip; desktop=(Join-Path $desk $zipName); zipSHA256=$zipHash
    dllSHA256=$receipt.dllSHA256; bridgeSHA256=$receipt.bridgeSHA256; files=$files
    runtimeValidated=$false; installed=$false; uploaded=$false
}
$manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $out 'release-manifest.json') -Encoding utf8
Copy-Item -LiteralPath (Join-Path $out 'release-manifest.json') -Destination $desk -Force
[ordered]@{ version=$Version; sourceCommit=$receipt.sourceCommit; zip=$zip; desktop=(Join-Path $desk $zipName); zipSHA256=$zipHash; dllSHA256=$receipt.dllSHA256; bridgeSHA256=$receipt.bridgeSHA256; files=$files.Count; uploaded=$false } | ConvertTo-Json
