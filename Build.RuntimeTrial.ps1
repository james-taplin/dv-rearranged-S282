param(
    [string]$Version = '1.23.2',
    [string]$GamePath = 'B:\SteamLibrary\steamapps\common\Derail Valley',
    [string]$OutputDirectory,
    [string]$CandidateDirectory,
    [switch]$Install
)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$playerBuild = [version]$Version -ge [version]'1.23.0'
$candidate = if ($CandidateDirectory) { $CandidateDirectory } else { Join-Path $root "build\runtime-trial-$Version" }
if (-not $OutputDirectory) { $OutputDirectory = "C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-$Version-Test" }
$bin = Join-Path $candidate 'bin'
$seed = Join-Path $root 'release\1.3.3-Beta1\RearrangedS282-B99-1.3.3-Beta1.zip'
if ((Get-FileHash -LiteralPath $seed).Hash -ne '4A8F1660AAFA1A03A2607B843AB20FB6DE7C625FFD08A94369767DBD8558FCBD') { throw 'Audited package baseline changed.' }
& 'C:\Program Files\dotnet\dotnet.exe' build (Join-Path $root 'RearrangedS282\RearrangedS282.csproj') -c Release '-p:Stage=3' "-p:Version=$Version" "-p:GamePath=$GamePath" -o $bin --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Runtime trial build failed.' }
$staging = Join-Path $candidate ('package-' + [Guid]::NewGuid().ToString('N'))
Expand-Archive -LiteralPath $seed -DestinationPath $staging
$mod = Join-Path $staging 'RearrangedS282'
Copy-Item -LiteralPath (Join-Path $bin 'RearrangedS282.dll') -Destination $mod -Force
Copy-Item -LiteralPath (Join-Path $root 'release-copy\CREDITS.txt') -Destination $mod -Force
$infoPath = Join-Path $mod 'Info.json'
$info = Get-Content -LiteralPath $infoPath -Raw | ConvertFrom-Json
$info.Version = $Version
$info.DisplayName = 'Rearranged S282 - B99 Expanded Trial'
$info.LoadAfter = @('Gauge','DVCarChanger','S282ModelTweaks','MultiCylinderChuffFix')
$info | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $infoPath -Encoding utf8
Copy-Item -LiteralPath (Join-Path $root 'RUNTIME_TRIAL_TESTING.txt') -Destination $mod
Copy-Item -LiteralPath (Join-Path $root 'RUNTIME_TRIAL_TESTING.txt') -Destination (Join-Path $mod 'README_BETA.txt') -Force
Copy-Item -LiteralPath (Join-Path $root 'RUNTIME_TRIAL_TESTING.txt') -Destination (Join-Path $mod 'MANUAL_TEST_CHECKLIST.txt') -Force
if (-not $playerBuild) {
Copy-Item -LiteralPath (Join-Path $root 'ARTICULATED_TEST_STATUS.txt') -Destination $mod
if ([version]$Version -ge [version]'1.22.3') { Copy-Item -LiteralPath (Join-Path $root 'GEOMETRY_TEST_STATUS.txt') -Destination $mod }
if ([version]$Version -ge [version]'1.22.4') { Copy-Item -LiteralPath (Join-Path $root 'AUDIO_STEAM_TEST_STATUS.txt') -Destination $mod }
Copy-Item -LiteralPath (Join-Path $root 'TRACTIVE_DIAGNOSTICS.txt') -Destination $mod
if ([version]$Version -ge [version]'1.21.0') {
    Copy-Item -LiteralPath (Join-Path $root 'PHYSICS_BASELINE.md') -Destination $mod
    if ([version]$Version -ge [version]'1.21.1') { Copy-Item -LiteralPath (Join-Path $root 'FOA_REFINEMENT.md') -Destination $mod }
    if ([version]$Version -ge [version]'1.21.2') { Copy-Item -LiteralPath (Join-Path $root 'FOA_BALANCE.md') -Destination $mod }
}
New-Item -ItemType Directory -Path (Join-Path $mod 'Tools') -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $root 'Tools\TractiveTable.ps1') -Destination (Join-Path $mod 'Tools')
}
$bundle = Join-Path $root 's282_drivetrain\ExportedProject\Assets\AssetBundles\s282_drivetrain'
$bundleHash = '44A1B9EAE0DDA58BE97B9AEDF3877522FBFFAEC060FE02A8385158E095BDB22D'
if ((Get-FileHash -LiteralPath $bundle).Hash -ne $bundleHash) { throw 'Original articulated drivetrain bundle differs from the assessed asset.' }
New-Item -ItemType Directory -Path (Join-Path $mod 'assets') -Force | Out-Null
Copy-Item -LiteralPath $bundle -Destination (Join-Path $mod 'assets\s282_drivetrain')
$casingAsset = Join-Path $root 'models\franklin_casing.obj'
Copy-Item -LiteralPath $casingAsset -Destination (Join-Path $mod 'assets\franklin_casing.obj')
foreach ($asset in @(
    @{name='steam_manifold';hash='C0F6CE1298AB861A5C2AEC14F1EF2C45565240B98BA81B2CF66D2FD8358E52D3'},
    @{name='steam_transfer';hash='ED782BCCA206AD25E80758DE9DA72F6DDBDA06B7B146957E4F2770B4532346E1'})) {
    $path=Join-Path $root ('models\'+$asset.name+'.obj')
    if ((Get-FileHash -LiteralPath $path).Hash -ne $asset.hash) { throw 'Original steam mesh export changed.' }
    Copy-Item -LiteralPath $path -Destination (Join-Path $mod ('assets\'+$asset.name+'.obj'))
}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$zip = Join-Path $OutputDirectory "RearrangedS282-B99-$Version-Test.zip"
Compress-Archive -LiteralPath $mod -DestinationPath $zip -Force
Copy-Item -LiteralPath (Join-Path $root 'RUNTIME_TRIAL_TESTING.txt') -Destination $OutputDirectory -Force
$receipt = [ordered]@{
    version=$Version; utc=[DateTime]::UtcNow.ToString('O'); installed=$false; runtimeValidated=$false
    dllSHA256=(Get-FileHash -LiteralPath (Join-Path $mod 'RearrangedS282.dll')).Hash
    zipSHA256=(Get-FileHash -LiteralPath $zip).Hash; package=$mod; zip=$zip
    bundleSHA256=$bundleHash
    franklinCasingSHA256=(Get-FileHash -LiteralPath $casingAsset).Hash
    features=@('Erie tender rear exhaust pipe using original transfer mesh/materials and native tender LOD; native white-steam plume driven by tender pressure/chuff; tender cylinder-cock/drip/crack particles with detach/delete cleanup; physics and audio unchanged','Stock-relative FoA restriction removed; bounded machinery-sensitive ceilings span both sides of stock where original cylinders permit; 2-2-2 prototype remains unchanged and excluded','Rated 124 t resource-independent design shares and cylinder sizing; machinery-sensitive nominal factor of adhesion ceiling translated to native mean pressure; stock exact, reduction-only capacity; separate 50 t/.8 Erie tender cylinder design','Dense collar-preserving shaft bends, compact two-cylinder pipe routes, minimum .8-native pipe/manifold size, original UV/material reuse, Erie tender LP supply internal','Structural null-string false failure fixed with genuine error/gap/collider/count rejection retained; running assembly version displayed in UMM','Rear five-axle engine rotation follower retention and per-clone binding, with structural phase checks','Preliminary family/capped effective adhesive loads based on total mass; reduction-only native bore/area/volume calibration to stock reference pull/grip ceiling; exact instance restore; no mass or shared-definition changes; Erie tender engine excluded','Curve load matches effective driver share; diagnostics disclose native stationary adhesion update eligibility','Visual steam inlet reconnection using native boiler fittings and original MIT Franklin manifolds; collar-preserving branches follow existing locomotive articulation; Erie tender supply internal; native LOD and teardown ownership; no steam physics changes','PHYS-A read-only grip, mass/resource and active-cylinder observations in structural JSON/CSV and live JSONL; native-cutoff mean-force comparison with separate Erie tender data; Markdown/BBCode table exporter; no traction rebalance','Requested Mogul pitch/rearward .18/.12, 2-4-0 .20/.14, Prairie .22/.12; Prairie-only pitch cap raised to .22 m; temporary controls retained pending final proportions','Temporary Prairie 2-6-2 and American 4-4-0 pitch and driver-group displacement controls, both new defaults zero; one combined precision retarget for Prairie; fixed accepted cylinder positions, sizes and native physics; tuning UI intended for removal after fixed proportions accepted','Main connecting rod spacing on 2-6-0 and 2-4-0: 0 to 0.25 m driver group rearward controls, fixed cylinders, measured 3.15 and 1.56 m pins, shaft-only native-clock mesh deformation and retained physics','Direct UMM reapply and radio selection reset; menu input guards; cycling fault original cause unconfirmed','Coupling rod spacing on 2-6-0 and 2-4-0 with 0 to 0.20 m extra pitch controls and 0.15 m defaults; fixed main driver, native rotation followers, matched fittings and both LODs','Complete Prairie cylinders, mechanism and drivers translated rearward 0.70 m from 1.13.5 with wheel size and retarget span retained','Measured reservoir-clear large drivers on high-speed 2-8-0/2-8-2/2-8-4 and Prairie, approximately 1.58 m with matching native mechanism and simulation radius','Prairie front driver is forward 0.80 m and rear casing datum 0.10 m behind its earlier zero offset using existing native-clock rod retarget','Oil-cup guide seating and proxy alignment plus unload/pool/delete cleanup and structural count diagnostics','Full cylinder casing normalization for Prairie, Pacific, Hudson and rigid 4-4-0/4-4-2/4-4-4/2-4-0 with native-length bodies, matching fittings and retained motion axes','4-14-4 drivers a further 0.25 m rearward with fixed cylinders and carrying trucks, existing rod retarget and crosshead cup alignment','47 retained arrangements including Prairie, Big Mallets and Erie tender-engine trial','Other arrangements retain the previous barrel-only casing treatment','Pacific/Hudson Walschaerts geometry alignment with native animation','Adjustable collider-free 2-2-2 clearance visuals','Native explosion materials for added and inactive running gear','Native DV pin/aim constraints and bogies','Native auxiliary steam engines with boiler/torque/exhaust accounting','Per-engine and curve diagnostics including failed native updates','Curve drag and squeal','Big-end oiling trial','Native rolling-audio axle-cache refresh','Optional random spawns and saved arrangements','LateUpdate rod synchronization','Delta trailing truck compatibility')
    sourceHashes=@(Get-ChildItem (Join-Path $root 'RearrangedS282') -Recurse -File -Filter '*.cs' | Where-Object FullName -NotMatch '\\(bin|obj)\\' | Get-FileHash | Select-Object Path,Hash)
}
if ([version]$Version -ge [version]'1.22.3') { $receipt.features = @('Source-derived steam duct extensions with circular elbows, original collars/UV/materials, seated rigid branches, compact lower collars, Prairie offset casting and measured articulated side ports; protected physics unchanged','Common motion-frame casing and guide alignment for resized rigid families, including twelve-driver layouts; live acceptance pending') + $receipt.features }
if ([version]$Version -ge [version]'1.22.4') { $receipt.features = @('Erie tender native chuff/loop and cylinder-cock audio, rear-exhaust seating, LP presentation profile, coupling-driven audio cleanup/rebind and diagnostic readiness; driving/boiler physics preserved') + $receipt.features }
if ($playerBuild) {
    $receipt.diagnosticsIncluded = $true
    $receipt.structuralDiagnosticsIncluded = $true
    $receipt.powerDiagnosticsIncluded = $false
    $receipt.resizeControlsIncluded = $false
    $receipt.features = @('48 arrangements; new native-constraint 2-4-4-4-0 articulated locomotive triplex with six driving axles and ordinary tender',
        'Optional Erie powered S282 tender for 43 compatible rigid and two-engine articulated layouts; UMM per-arrangement choices, Erie on by default, safe reapply',
        'Native tender RPM adapted to host/tender wheel-radius ratio without writing shared host RPM; separate tender force/steam demand retained',
        'Native tender driving-gear clanks, chuffs/loops, cylinder-cock audio and existing exhaust/cock/drip visuals',
        'Accepted Mogul, 2-4-0, Prairie and American proportions fixed; cylinder normalization retained; no granular resizing UI',
        'Manual structural-only test button and JSON report; power recorders, power UI, tick hook and exporter tools excluded',
        'Existing native boiler, cylinder calibration, adhesion, braking, curving and rod animation retained; no true compound simulation enabled')
}
if ([version]$Version -ge [version]'1.23.1') {
    $receipt.features = @('Compact articulated steam header and side entries; small local fitting overlap permitted, original profile/material and minimum bore retained',
        'Fixed 2-4-4-4-0 stock fallback when the second hinge lies just behind the native bar-frame end; native hinge and driver positions unchanged',
        'Powered tender uses independent inactive source templates; returning to Erie after other arrangements no longer depends on disposable shared Mallet parts') + $receipt.features
}
if ([version]$Version -ge [version]'1.23.2') {
    $receipt.features = @('Hide optional Franklin assets before native rear/tender template cloning; prevent four extra cylinders breaking Angus/Great Northern/Erie and appearing on powered tenders; retain strict structural checks') + $receipt.features
}
if ($Install) {
    if (Get-Process -Name DerailValley -ErrorAction SilentlyContinue) { throw 'Close Derail Valley before installing the prepared test ZIP.' }
    $destination = Join-Path $GamePath 'Mods\RearrangedS282'
    # James requested no full install backups or rollback copies, 21 Oct 2026.
    $absoluteMod = [IO.Path]::GetFullPath($mod).TrimEnd('\') + '\'
    $absoluteDestination = [IO.Path]::GetFullPath($destination).TrimEnd('\') + '\'
    foreach ($file in Get-ChildItem -LiteralPath $mod -File -Recurse) {
        $relative = $file.FullName.Substring($absoluteMod.Length)
        $target = [IO.Path]::GetFullPath((Join-Path $destination $relative))
        if (-not $target.StartsWith($absoluteDestination,[StringComparison]::OrdinalIgnoreCase)) { throw 'Installation path escaped the named mod folder.' }
        New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force | Out-Null
        Copy-Item -LiteralPath $file.FullName -Destination $target -Force
    }
    if ((Get-FileHash -LiteralPath (Join-Path $destination 'RearrangedS282.dll')).Hash -ne $receipt.dllSHA256) { throw 'Installed DLL hash differs from candidate.' }
    if ((Get-FileHash -LiteralPath (Join-Path $destination 'assets\s282_drivetrain')).Hash -ne $bundleHash) { throw 'Installed articulated bundle hash differs.' }
    $receipt.installed=$true
    $receipt.backupPolicy='No backup copies, as requested by James on 21 Oct 2026'
    $receipt.installedPath=$destination
}
$receipt | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $candidate 'build-receipt.json') -Encoding utf8
$receipt | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'build-receipt.json') -Encoding utf8
Write-Output ([ordered]@{version=$Version;installed=$receipt.installed;dllSHA256=$receipt.dllSHA256;zip=$zip} | ConvertTo-Json)



