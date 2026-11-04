param(
 [string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\offline-validation\LittleBoyLinkages'),
 [string]$GeometryReceipt=(Join-Path $PSScriptRoot 'build\offline-validation\LittleBoy\validation.json'),
 [string]$ExportDirectory=(Join-Path $PSScriptRoot 'build\hidden-linkages\export-smoke')
)
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$exportTarget=Join-Path $OutputDirectory 'export-smoke'
New-Item -ItemType Directory -Path $exportTarget -Force|Out-Null
if(-not(Test-Path -LiteralPath $ExportDirectory)){throw 'Missing existing private native OBJ export inputs'}
Get-ChildItem -LiteralPath $ExportDirectory -Filter '*.obj' -File|ForEach-Object {Copy-Item -LiteralPath $_.FullName -Destination $exportTarget}
$fixture=Join-Path $PSScriptRoot 'evidence\hidden-linkages'
$python='C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe'
$maps=Join-Path $OutputDirectory 'little-boy-map.json'
& $python (Join-Path $PSScriptRoot 'evidence\little-boy-tests\prepare_linkage_replay.py') --output $maps --geometry-receipt $GeometryReceipt
if($LASTEXITCODE -ne 0){throw 'ID37/50 map provenance did not match'}
$measure=Join-Path $OutputDirectory 'native-measurements.json'
& $python (Join-Path $fixture 'measure_native.py') --output $measure
if($LASTEXITCODE -ne 0){throw 'Native linkage measurement failed'}
$empty=Join-Path $OutputDirectory 'empty-conventional.json'
Set-Content $empty '{"layouts":[]}' -Encoding utf8
$stubs=Get-Content (Join-Path $PSScriptRoot 'evidence\fourteen-spacing-tests\Stubs.cs') -Raw
$stubs=$stubs.Replace('public sealed class Transform :','public sealed partial class Transform :').Replace('public struct Matrix4x4','public partial struct Matrix4x4').Replace('public struct Quaternion','public partial struct Quaternion').Replace('public struct Vector3','public partial struct Vector3').Replace('public sealed class Mesh :','public sealed partial class Mesh :')
# The shared Fourteen fixture now provides these members; the hidden-linkage
# fixture has its own equivalent definitions. Remove only duplicates in this copy.
foreach($member in @('public bool IsChildOf\(Transform t\)','public static Vector3 Min\(Vector3 a,Vector3 b\)','public static Vector3 Max\(Vector3 a,Vector3 b\)','public static Quaternion Inverse\(Quaternion q\)','public static Quaternion operator \*\(Quaternion a,Quaternion b\)','public static Vector3 operator \*\(Quaternion q,Vector3 v\)')) {
 $stubs=[regex]::Replace($stubs,'(?m)^\s*'+$member+'[^\r\n]*(?:\r?\n|$)','')
}
$stubs=$stubs.Replace('public Vector3[] vertices=Array.Empty<Vector3>();','private Vector3[] data=Array.Empty<Vector3>(); public Vector3[] FixturePositions=>data.ToArray(); public Vector3[] vertices {get {if(!isReadable)throw new InvalidOperationException("Native mesh is CPU-unreadable");return data.ToArray();}set{data=value.ToArray();}}').Replace('public int vertexCount=>vertices.Length;','public int vertexCount=>data.Length;').Replace('public void RecalculateBounds()=>throw new InvalidOperationException("Legacy retarget must retain its existing LOD bounds policy");','public void RecalculateBounds(){}')
$stubCopy=Join-Path $OutputDirectory 'Fixture.Stubs.cs';Set-Content $stubCopy $stubs -Encoding UTF8
$meshSource=Get-Content (Join-Path $PSScriptRoot 'RearrangedS282\MeshFinder.cs') -Raw
$start=$meshSource.IndexOf('internal static void NormalizeLinkageImport(Mesh mesh)',[StringComparison]::Ordinal)
if($start -lt 0){throw 'Missing actual native OBJ coordinate normalization'}
$open=$meshSource.IndexOf('{',$start);$depth=1;$end=$open+1
while($depth -gt 0 -and $end -lt $meshSource.Length){if($meshSource[$end] -eq '{'){$depth++};if($meshSource[$end] -eq '}'){$depth--};$end++}
if($depth -ne 0){throw 'Invalid normalization source body'}
$normalizer=Join-Path $OutputDirectory 'ActualNormalization.cs'
Set-Content $normalizer ("using UnityEngine; namespace RearrangedS282 { internal sealed partial class MeshFinder { "+$meshSource.Substring($start,$end-$start)+" } }") -Encoding UTF8
$production=@('DriveRetarget.cs','RodRemesher.cs','WheelRearranger.HiddenLinkages.cs','WheelArrangement.cs')|ForEach-Object {'<Compile Include="'+(Join-Path $PSScriptRoot ('RearrangedS282\'+$_))+'"/>'}
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit></PropertyGroup><ItemGroup>$($production -join '')<Compile Include="$normalizer"/><Compile Include="$stubCopy"/><Compile Include="$fixture\Program.cs"/><Compile Include="$fixture\Owner.cs"/><Compile Include="$fixture\Stubs.cs"/></ItemGroup></Project>
"@
$projectPath=Join-Path $OutputDirectory 'LittleBoyLinkages.csproj';Set-Content $projectPath $project -Encoding utf8
$report=Join-Path $OutputDirectory 'validation.json'
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- (Join-Path $PSScriptRoot 'build\franklin-runtime\geometry.json') (Join-Path $PSScriptRoot 'evidence\pacific-hudson-valve-gear\native-animation.json') $measure $report $empty $maps
if($LASTEXITCODE -ne 0){throw 'ID50 hidden linkage replay failed'}
$result=Get-Content $report -Raw|ConvertFrom-Json
$result|Add-Member mapProof (Get-Content $maps -Raw|ConvertFrom-Json)
$result|ConvertTo-Json -Depth 12|Set-Content $report -Encoding utf8
Get-Content $report -Raw
