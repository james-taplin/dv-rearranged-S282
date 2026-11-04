param(
    [string]$GamePath = 'B:\SteamLibrary\steamapps\common\Derail Valley',
    [string]$PythonPath = 'C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe',
    [int]$Steps = 720,
    [int[]]$Ids = @(31, 32, 38, 40, 47),
    # Triplex: synthetic support arc; native constraints determine engine yaw.
    # Two-engine/Walschaerts: unchanged explicit fixed-pivot yaw stress.
    [float[]]$Yaws = @(-15, 0, 15),
    [switch]$OldNoseControl,
    [switch]$OldMalletGapControl,
    [string]$WalsStaticPath
)
$ErrorActionPreference = 'Stop'
if ($Steps -lt 4) { throw 'Use at least four phase samples.' }
$root = $PSScriptRoot
$privateRoot = Join-Path $root 'build/franklin-runtime'
$probe = Join-Path $privateRoot 'probe-bin'
$production = Join-Path $privateRoot 'production-bin'
$dotnet = 'C:\Program Files\dotnet\dotnet.exe'
$bundle = Join-Path $root 's282_drivetrain/ExportedProject/Assets/AssetBundles/s282_drivetrain'
$geometry = Join-Path $privateRoot 'geometry.json'
if (-not $WalsStaticPath) { $WalsStaticPath = Join-Path $privateRoot 'wals-static.json' }
$assets = Join-Path $GamePath 'Mods/RearrangedS282/assets'
foreach ($path in @($PythonPath, $dotnet, $bundle, (Join-Path $assets 'franklin_casing.obj'))) {
    if (-not (Test-Path -LiteralPath $path)) { throw "Required private probe input is missing: $path" }
}
& $PythonPath -B (Join-Path $root 'evidence/franklin-runtime/extract_geometry.py') --game $GamePath --bundle $bundle --output $geometry
if ($LASTEXITCODE -ne 0) { throw 'Local native geometry extraction failed.' }
& $PythonPath -B (Join-Path $root 'evidence/franklin-runtime/prepare.py')
if ($LASTEXITCODE -ne 0) { throw 'Exact production configuration extraction failed.' }
& $dotnet build (Join-Path $root 'RearrangedS282/RearrangedS282.csproj') -c Release '-p:Stage=3' '-p:Version=0.0.0-franklin-probe' "-p:GamePath=$GamePath" -o $production --ignore-failed-sources
if ($LASTEXITCODE -ne 0) { throw 'Private production assembly compilation failed.' }
& $dotnet build (Join-Path $root 'evidence/franklin-runtime/FranklinRuntimeProbe.csproj') -c Release "-p:ManagedPath=$(Join-Path $GamePath 'DerailValley_Data/Managed')" -o $probe --ignore-failed-sources
if ($LASTEXITCODE -ne 0) { throw 'Private Franklin probe compilation failed.' }
$settings = @{
    geometry = $geometry
    productionDll = Join-Path $production 'RearrangedS282.dll'
    assets = $assets
    steps = $Steps
    ids = $Ids
    yaws = $Yaws
    applyNoseCorrection = -not $OldNoseControl.IsPresent
}
if (@($Ids | Where-Object { $_ -in @(38,40,47) }).Count -gt 0) {
    $nativeGeometry = Get-Content -LiteralPath $geometry -Raw | ConvertFrom-Json
    $supportDatum = $nativeGeometry.triplexSupportDatum
    if ($null -eq $supportDatum -or @($supportDatum.positionBody).Count -ne 3 -or
        $supportDatum.nativeResourceSHA256 -ne $nativeGeometry.nativeSHA256) {
        throw 'Triplex constraint fixture needs a measured Axle_F body-frame datum from the same native resource.'
    }
    $settings.triplexSupportRest = @($supportDatum.positionBody | ForEach-Object { [double]$_ })
    $settings.triplexSupportSourceSHA256 = $supportDatum.nativeResourceSHA256
}
if (@($Ids | Where-Object { $_ -in @(10,37,41,42,43,45,46) }).Count -gt 0) {
    if (-not (Test-Path -LiteralPath $WalsStaticPath)) { throw 'Generate the actual LMS payload with Validate.LmsStaticGeometry.ps1 before building a Walschaerts probe.' }
    $wals = Get-Content -LiteralPath $WalsStaticPath -Raw | ConvertFrom-Json
    if (-not $wals.actualUnityLmsValidated) { throw 'Walschaerts clearance requires the actual Unity installed-LMS geometry payload.' }
    foreach ($inputSource in @(@($wals.source,$wals.sourceSHA256), @($wals.lmsDLL,$wals.lmsSHA256), @($wals.nativeResource,$wals.nativeResourceSHA256), @($wals.actualUnityLmsReport,$wals.actualUnityLmsReportSHA256))) {
        if ((Get-FileHash -LiteralPath $inputSource[0] -Algorithm SHA256).Hash -ne $inputSource[1]) { throw "Stale actual-LMS source payload: $($inputSource[0])" }
    }
    $settings.walsStatic = $WalsStaticPath
}
if ($OldMalletGapControl) { $settings.malletGap = .1; $settings.expectBaselineContacts = $true }
$settings | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $probe 'franklin-config.json')
[PSCustomObject]@{ ProbeDirectory = $probe; AssemblyName = 'FranklinRuntimeProbe.dll'; TypeName = 'FranklinRuntimeProbe'; NativeData = $geometry; PhaseSamples = $Steps; LayoutIds = $Ids; OldNoseControl = $OldNoseControl.IsPresent } | ConvertTo-Json
