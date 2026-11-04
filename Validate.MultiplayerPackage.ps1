param(
    [string]$Version = '2.0.9.4',
    [string]$PackagePath,
    [string]$SeedPackage = 'B:\GitHub repo\rearranged-s282-independent-slip\build\independent-integration\RearrangedS282-2.0.8-slip-test.zip',
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'build\multiplayer-2.0.9-validation\package')
)
$ErrorActionPreference = 'Stop'
if (-not $PackagePath) { $PackagePath = Join-Path $PSScriptRoot "release\$Version\RearrangedS282-B99-$Version.zip" }
$numericVersion = [version]($Version -replace '-.*$', '')
$expectedAssemblyVersion = [version]::new($numericVersion.Major, $numericVersion.Minor, [Math]::Max(0,$numericVersion.Build), [Math]::Max(0,$numericVersion.Revision)).ToString(4)
Add-Type -AssemblyName System.IO.Compression.FileSystem
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$out = [IO.Path]::GetFullPath($OutputDirectory)
$stage = Join-Path $out ('inspect-' + [Guid]::NewGuid().ToString('N'))
$checks = [Collections.Generic.List[string]]::new()
function Check([bool]$ok, [string]$label) { if (-not $ok) { throw $label }; $checks.Add($label) }
Check ((Get-FileHash -LiteralPath $SeedPackage).Hash -eq 'B3677FC1D8D42F9423E702C044DB75B84E0E31C5C9EDB71CD62AD56C4D35B94D') 'Verified selected seed archive'
$zip = [IO.Compression.ZipFile]::OpenRead($PackagePath)
try {
    $entries = @($zip.Entries | Where-Object Name -NE '')
    Check (@($entries | Where-Object { $_.FullName -notmatch '^RearrangedS282/' -or $_.FullName -match '\\|(^|/)\.\.(/|$)' }).Count -eq 0) 'One UMM mod root with safe forward-slash names'
    Check (@($entries.FullName | Group-Object | Where-Object Count -GT 1).Count -eq 0) 'No duplicate archive members'
    Check (@($entries | Where-Object { $_.FullName -match '(?i)MultiplayerAPI\.dll$|Settings\.xml$|\.(log|jsonl|pdb|csv)$|README_BETA|INDEPENDENT_SLIP_TEST' }).Count -eq 0) 'No API, personal settings, diagnostics or superseded trial notes'
} finally { $zip.Dispose() }
Expand-Archive -LiteralPath $PackagePath -DestinationPath $stage
try {
    $mod = Join-Path $stage 'RearrangedS282'
    $info = Get-Content -LiteralPath (Join-Path $mod 'Info.json') -Raw | ConvertFrom-Json
    Check ($info.Id -eq 'RearrangedS282' -and $info.Version -eq $Version -and $info.EntryMethod -eq 'RearrangedS282.Main.Load') 'UMM identity/version/entry point'
    Check ($info.MultiplayerCompatibility -eq 'All' -and 'Multiplayer' -notin $info.Requirements -and 'LocoMeshSplitter' -in $info.Requirements -and 'Multiplayer' -in $info.LoadAfter) 'Optional Multiplayer and required Mesh Splitter'
    foreach ($name in 'README.txt','CHANGELOG.txt','CREDITS.txt','MULTIPLAYER_TESTING.txt','LICENSE','THIRD_PARTY_NOTICES.txt') {
        Check (Test-Path -LiteralPath (Join-Path $mod $name) -PathType Leaf) "Required document: $name"
    }
    $readme = Get-Content -LiteralPath (Join-Path $mod 'README.txt') -Raw
    Check ($readme -notmatch '(?m)^#{1,6}\s|\*\*|`|\[[^\]]+\]\(https?://|\[/?(?:b|i|url|list)(?:=[^\]]*)?\]') 'Player README is plain text'
    $seed = [IO.Compression.ZipFile]::OpenRead($SeedPackage)
    $changed = @('CHANGELOG.txt','CREDITS.txt','INDEPENDENT_SLIP_TEST.txt','Info.json','README_BETA.txt','README.txt','RearrangedS282.dll','UMM_OPTIONS.md')
    try {
        foreach ($entry in $seed.Entries) {
            if (-not $entry.Name -or $entry.FullName.Substring('RearrangedS282/'.Length) -in $changed) { continue }
            $file = Join-Path $stage $entry.FullName
            Check (Test-Path -LiteralPath $file -PathType Leaf) "Preserved seed member exists: $($entry.FullName)"
            $stream = $entry.Open(); $sha = [Security.Cryptography.SHA256]::Create()
            try { $expected = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','') }
            finally { $sha.Dispose(); $stream.Dispose() }
            Check ((Get-FileHash -LiteralPath $file).Hash -eq $expected) "Seed asset/exporter/licence byte identity: $($entry.FullName)"
        }
    } finally { $seed.Dispose() }
    # Load the metadata reader from bytes so Windows can remove the inspection
    # directory after validation instead of holding its DLL open until exit.
    [void][Reflection.Assembly]::Load([IO.File]::ReadAllBytes((Join-Path $mod 'AssetStudioModCLI_net472_win64_contained\Mono.Cecil.dll')))
    foreach ($name in 'RearrangedS282','RearrangedS282.Multiplayer') {
        $assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Join-Path $mod ($name + '.dll')))
        try {
            $assemblyVersionText = @($assembly.CustomAttributes | Where-Object { $_.AttributeType.FullName -eq 'System.Reflection.AssemblyInformationalVersionAttribute' })[0].ConstructorArguments[0].Value
            Check ($assembly.Name.Name -eq $name -and $assembly.Name.Version.ToString() -eq $expectedAssemblyVersion -and $assemblyVersionText -eq $Version) "$name compiled version matches release"
            if ($name -eq 'RearrangedS282') {
                Check (@($assembly.MainModule.AssemblyReferences | Where-Object { $_.Name -match 'Multiplayer|MPAPI' }).Count -eq 0) 'Main DLL has no Multiplayer/API assembly dependency'
                Check (@($assembly.MainModule.Types | Where-Object FullName -EQ 'RearrangedS282.IndependentDrive').Count -eq 1) 'Experimental independent drive is included'
                Check (@($assembly.MainModule.Types | Where-Object FullName -EQ 'RearrangedS282.IndependentDriveReplication').Count -eq 1) 'Native independent-group replication is included'
                $schema = $assembly.MainModule.Types | Where-Object FullName -EQ 'RearrangedS282.NativeSlipEntitySchema'
                Check (@($schema.NestedTypes | Where-Object { $_.Interfaces.InterfaceType.FullName -contains 'Unity.Entities.IComponentData' }).Count -eq 0) 'Slip schema requires no late-loaded custom ECS component'
            } else {
                Check (@($assembly.MainModule.AssemblyReferences | Where-Object { $_.Name -eq 'MultiplayerAPI' -and $_.Version.ToString() -eq '1.1.0.0' }).Count -eq 1) 'Optional bridge targets supplied API 1.1'
            }
        } finally { $assembly.Dispose() }
    }
    $report = [ordered]@{ passed=$true; checks=$checks.Count; labels=@($checks); zipSHA256=(Get-FileHash -LiteralPath $PackagePath).Hash; version=$info.Version; installed=$false; liveMultiplayerValidated=$false }
    $report | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $out 'validation.json') -Encoding utf8
    [pscustomobject]$report | Select-Object passed,checks,version,zipSHA256,installed,liveMultiplayerValidated | ConvertTo-Json
} finally {
    $resolved = [IO.Path]::GetFullPath($stage)
    if (-not $resolved.StartsWith($out.TrimEnd([char]92) + [char]92, [StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetFileName($resolved) -notmatch '^inspect-[a-f0-9]{32}$') { throw 'Unsafe inspection cleanup target' }
    if (Test-Path -LiteralPath $resolved) { Remove-Item -LiteralPath $resolved -Recurse -Force }
}
