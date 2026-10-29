$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Add-Type -Path (Join-Path $root 'lib/AssetStudioModCLI_net472_win32_64/Mono.Cecil.dll')
$normalizer=Get-Content (Join-Path $root 'Validate.CylinderCasingIntegration.ps1') -Raw
$a=$normalizer.IndexOf('function NormalizedMethod(');$b=$normalizer.IndexOf('function TextHash(',$a)
Invoke-Expression $normalizer.Substring($a,$b-$a)
$installed='B:\SteamLibrary\steamapps\common\Derail Valley\Mods\RearrangedS282\RearrangedS282.dll'
if((Get-FileHash $installed).Hash -ne '777CB091BA203CC1C553B0A179451DFBE31535E115413322B09EF684C0A37DED'){throw 'Installed comparison baseline changed'}
$dll=Join-Path $root 'build/lubrication-polish/bin/RearrangedS282.dll'
$old=[Mono.Cecil.ModuleDefinition]::ReadModule($installed)
$new=[Mono.Cecil.ModuleDefinition]::ReadModule($dll)
try {
 $after=@{};foreach($t in $new.GetTypes()){foreach($m in $t.Methods){$after[$m.FullName]=$m}}
 $checked=0;$changed=@()
 foreach($t in $old.GetTypes()){
  if($t.FullName -match 'WheelRearranger|MeshFinder|Stage2RuntimeChecks'){continue}
  foreach($m in $t.Methods){if(-not $after.ContainsKey($m.FullName) -or (NormalizedMethod $m) -cne (NormalizedMethod $after[$m.FullName])){$changed+=$m.FullName}else{$checked++}}
 }
 if($changed.Count){throw ($changed -join "`n")}
 [ordered]@{passed=$true;unchangedMethodsOutsideGeometryAndDiagnostics=$checked;installedDllHash=(Get-FileHash $installed).Hash;testDllHash=(Get-FileHash $dll).Hash;runtimeValidated=$false}|
 ConvertTo-Json|Set-Content (Join-Path $root 'build/lubrication-polish/preservation.json')
 Get-Content (Join-Path $root 'build/lubrication-polish/preservation.json')
} finally {$old.Dispose();$new.Dispose()}
