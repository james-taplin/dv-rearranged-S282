param(
 [string]$OutputDirectory=(Join-Path $PSScriptRoot '..\..\build\offline-validation\HiddenLinkages'),
 [string]$ExportDirectory=(Join-Path $PSScriptRoot '..\..\build\hidden-linkages\export-smoke')
)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$OutputDirectory=[IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$exportTarget=Join-Path $OutputDirectory 'export-smoke'
if([IO.Path]::GetFullPath($ExportDirectory) -ne [IO.Path]::GetFullPath($exportTarget)) {
    if(-not(Test-Path -LiteralPath $ExportDirectory)){throw 'Missing existing private native OBJ export inputs'}
    New-Item -ItemType Directory -Path $exportTarget -Force|Out-Null
    Get-ChildItem -LiteralPath $ExportDirectory -Filter '*.obj' -File|ForEach-Object {Copy-Item -LiteralPath $_.FullName -Destination $exportTarget}
}
& 'C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe' (Join-Path $PSScriptRoot 'measure_native.py') --output (Join-Path $OutputDirectory 'native-measurements.json')
if($LASTEXITCODE -ne 0){throw 'Native measurement baseline failed'}
foreach($entry in @(@('layout_provenance.py','layout-provenance.json'),@('articulated_layouts.py','articulated-layouts.json'))) {
    $helper=$entry[0]
    & 'C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe' (Join-Path $PSScriptRoot $helper) --output (Join-Path $OutputDirectory $entry[1])
    if($LASTEXITCODE -ne 0){throw "Actual-source map provenance failed: $helper"}
}
$stubs=Get-Content -LiteralPath (Join-Path $root 'evidence\fourteen-spacing-tests\Stubs.cs') -Raw
$stubs=$stubs.Replace('public sealed class Transform :','public sealed partial class Transform :')
$stubs=$stubs.Replace('public struct Matrix4x4','public partial struct Matrix4x4')
$stubs=$stubs.Replace('public struct Quaternion','public partial struct Quaternion')
$stubs=$stubs.Replace('public struct Vector3','public partial struct Vector3')
$stubs=$stubs.Replace('public sealed class Mesh :','public sealed partial class Mesh :')
foreach($member in @('public bool IsChildOf\(Transform t\)','public static Vector3 Min\(Vector3 a,Vector3 b\)','public static Vector3 Max\(Vector3 a,Vector3 b\)','public static Quaternion Inverse\(Quaternion q\)','public static Quaternion operator \*\(Quaternion a,Quaternion b\)','public static Vector3 operator \*\(Quaternion q,Vector3 v\)')) {
    $stubs=[regex]::Replace($stubs,'(?m)^\s*'+$member+'[^\r\n]*(?:\r?\n|$)','')
}
$stubs=$stubs.Replace('public Vector3[] vertices=Array.Empty<Vector3>();','private Vector3[] data=Array.Empty<Vector3>(); public Vector3[] FixturePositions=>data.ToArray(); public Vector3[] vertices {get {if(!isReadable)throw new InvalidOperationException("Native mesh is CPU-unreadable");return data.ToArray();}set{data=value.ToArray();}}')
$stubs=$stubs.Replace('public int vertexCount=>vertices.Length;','public int vertexCount=>data.Length;')
$stubs=$stubs.Replace('public void RecalculateBounds()=>throw new InvalidOperationException("Legacy retarget must retain its existing LOD bounds policy");','public void RecalculateBounds(){}')
$stubCopy=Join-Path $OutputDirectory 'Fixture.Stubs.cs'
Set-Content -LiteralPath $stubCopy -Value $stubs -Encoding UTF8
$production=@('DriveRetarget.cs','RodRemesher.cs','WheelRearranger.HiddenLinkages.cs','WheelArrangement.cs') | ForEach-Object { '<Compile Include="'+(Join-Path $root ('RearrangedS282\'+$_))+'"/>' }
$fixed=Join-Path $root 'RearrangedS282\DriveRetarget.FixedEndpoints.cs'
if(Test-Path -LiteralPath $fixed){$production+='<Compile Include="'+$fixed+'"/>'}
$meshSource=Get-Content -LiteralPath (Join-Path $root 'RearrangedS282\MeshFinder.cs') -Raw
$start=$meshSource.IndexOf('internal static void NormalizeLinkageImport(Mesh mesh)',[StringComparison]::Ordinal)
if($start -lt 0){throw 'Missing actual native OBJ coordinate normalization'}
$open=$meshSource.IndexOf('{',$start);$depth=1;$end=$open+1
while($depth -gt 0){if($meshSource[$end] -eq '{'){$depth++};if($meshSource[$end] -eq '}'){$depth--};$end++}
$normalizer=$meshSource.Substring($start,$end-$start)
$normalizerPath=Join-Path $OutputDirectory 'ActualNormalization.cs'
Set-Content -LiteralPath $normalizerPath -Value "using UnityEngine; namespace RearrangedS282 { internal sealed partial class MeshFinder { $normalizer } }" -Encoding UTF8
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup><ItemGroup>$($production -join '')<Compile Include="$normalizerPath"/><Compile Include="$stubCopy"/><Compile Include="$PSScriptRoot\Program.cs"/><Compile Include="$PSScriptRoot\Owner.cs"/><Compile Include="$PSScriptRoot\Stubs.cs"/></ItemGroup></Project>
"@
$projectPath=Join-Path $OutputDirectory 'HiddenLinkages.Validation.csproj'
Set-Content -LiteralPath $projectPath -Value $project -Encoding UTF8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- (Join-Path $root 'build\franklin-runtime\geometry.json') (Join-Path $root 'evidence\pacific-hudson-valve-gear\native-animation.json') (Join-Path $OutputDirectory 'native-measurements.json') (Join-Path $OutputDirectory 'validation.json') (Join-Path $OutputDirectory 'layout-provenance.json') (Join-Path $OutputDirectory 'articulated-layouts.json')
if($LASTEXITCODE -ne 0){throw 'Hidden linkage geometry validation failed'}
$report=Join-Path $OutputDirectory 'validation.json'
$result=Get-Content -LiteralPath $report -Raw|ConvertFrom-Json
$provenance=@('RearrangedS282\DriveRetarget.cs','RearrangedS282\MeshFinder.cs','RearrangedS282\RodRemesher.cs','RearrangedS282\WheelRearranger.HiddenLinkages.cs','RearrangedS282\WheelArrangement.cs','evidence\hidden-linkages\Program.cs','evidence\hidden-linkages\Owner.cs','evidence\hidden-linkages\Stubs.cs','evidence\hidden-linkages\validate.ps1','evidence\hidden-linkages\measure_native.py','evidence\hidden-linkages\layout_provenance.py','evidence\hidden-linkages\articulated_layouts.py')|ForEach-Object {
    [pscustomobject]@{path=$_;sha256=(Get-FileHash -LiteralPath (Join-Path $root $_) -Algorithm SHA256).Hash}
}
$result|Add-Member sourceHashes $provenance
$result|Add-Member utc ([DateTime]::UtcNow.ToString('o'))
$result|ConvertTo-Json -Depth 8|Set-Content -LiteralPath $report -Encoding UTF8
