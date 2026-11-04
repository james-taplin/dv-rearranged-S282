param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\obj-mesh-validation'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$out=(Resolve-Path -LiteralPath $OutputDirectory).Path
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup><ItemGroup>
<Compile Include="$PSScriptRoot\RearrangedS282\OBJImport\OBJLoader.cs"/>
<Compile Include="$PSScriptRoot\RearrangedS282\OBJImport\OBJObjectBuilder.cs"/>
<Compile Include="$PSScriptRoot\RearrangedS282\OBJImport\CharWordReader.cs"/>
<Compile Include="$PSScriptRoot\evidence\obj-mesh-tests\Program.cs"/>
</ItemGroup></Project>
"@
$path=Join-Path $out 'ObjMesh.csproj'; Set-Content -LiteralPath $path -Value $project -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' build $path -c Release
if($LASTEXITCODE -ne 0){throw 'Current mesh-only importer fixture did not compile'}
$current=& 'C:\Program Files\dotnet\dotnet.exe' run --project $path -c Release --no-build -- --snapshot
if($LASTEXITCODE -ne 0){throw 'Current importer parity snapshot failed'}
$baseline=Join-Path $out 'baseline';New-Item -ItemType Directory -Force -Path $baseline | Out-Null
foreach($file in @('OBJLoader.cs','OBJObjectBuilder.cs','CharWordReader.cs')){
    $content=& git -C $PSScriptRoot show "7a6aa1d:RearrangedS282/OBJImport/$file"
    if($LASTEXITCODE -ne 0){throw "Cannot retrieve baseline $file"}
    Set-Content -LiteralPath (Join-Path $baseline $file) -Value $content -Encoding utf8
}
$baselineProject=$project.Replace("$PSScriptRoot\RearrangedS282\OBJImport\", "$baseline\")
$baselinePath=Join-Path $baseline 'ObjMeshBaseline.csproj';Set-Content -LiteralPath $baselinePath -Value $baselineProject -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' build $baselinePath -c Release
if($LASTEXITCODE -ne 0){throw 'Original importer parity fixture did not compile'}
$expected=& 'C:\Program Files\dotnet\dotnet.exe' run --project $baselinePath -c Release --no-build -- --snapshot
if($LASTEXITCODE -ne 0 -or ($expected -join "`n") -ne ($current -join "`n")){throw 'Representative vertices, normals, UVs, submesh order or triangle winding differs from original importer'}
Write-Output 'PASS original 7a6aa1d importer multi-submesh geometry/attribute/topology parity'
$assets=@('models\franklin_casing.obj','models\steam_manifold.obj','models\steam_transfer.obj')
$cachedWheel='B:\SteamLibrary\steamapps\common\Derail Valley\Mods\RearrangedS282\assets\s282_mech_wheels_connect.obj'
if(Test-Path -LiteralPath $cachedWheel -PathType Leaf) {$assets+= $cachedWheel} else {Write-Output 'SKIP cached wheel parity: installed export unavailable'}
foreach($asset in $assets) {
    $assetPath=if([IO.Path]::IsPathRooted($asset)){$asset}else{Join-Path $PSScriptRoot $asset}
    $bytes=[IO.File]::ReadAllBytes($assetPath)
    if($bytes.Length -eq 0 -or $bytes[$bytes.Length-1] -notin @(10,13)) {throw "Baseline comparison requires a newline-terminated asset: $asset"}
    $expected=& 'C:\Program Files\dotnet\dotnet.exe' run --project $baselinePath -c Release --no-build -- --snapshot-file $assetPath
    if($LASTEXITCODE -ne 0){throw "Original importer failed on $asset"}
    $actual=& 'C:\Program Files\dotnet\dotnet.exe' run --project $path -c Release --no-build -- --snapshot-file $assetPath
    if($LASTEXITCODE -ne 0 -or ($expected -join "`n") -ne ($actual -join "`n")){throw "Importer geometry or attributes differ for $asset"}
    Write-Output "PASS original importer parity for $asset"
}
& 'C:\Program Files\dotnet\dotnet.exe' run --project $path -c Release
if($LASTEXITCODE -ne 0){throw 'Mesh-only importer validation failed'}
