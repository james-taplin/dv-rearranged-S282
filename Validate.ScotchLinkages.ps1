param(
 [string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\offline-validation\ScotchLinkages'),
 [string]$GeometryReceipt=(Join-Path $PSScriptRoot 'build\offline-validation\ScotchGlider\validation.json')
)
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$output=(Resolve-Path -LiteralPath $OutputDirectory).Path
$root=$PSScriptRoot
$exports=Join-Path $root 'build\hidden-linkages\export-smoke'
if(-not (Test-Path -LiteralPath $exports)){throw 'Previously captured native exporter payloads are missing.'}
$exportTarget=Join-Path $output 'export-smoke'
New-Item -ItemType Directory -Path $exportTarget -Force|Out-Null
Copy-Item -Path (Join-Path $exports '*.obj') -Destination $exportTarget
$geometry=Get-Content -LiteralPath $GeometryReceipt -Raw | ConvertFrom-Json
if(-not $geometry.passed -or $geometry.arrangementId -ne 49 -or $geometry.managedSourceSHA256 -ne (Get-FileHash -LiteralPath (Join-Path $root 'RearrangedS282\WheelRearranger.ScotchGlider.cs')).Hash){throw 'Scotch geometry receipt is absent, failed, or belongs to different source.'}
$projection=[ordered]@{layouts=@([ordered]@{id=49;scale=$geometry.driverScale;driveOffset=$geometry.driveOffsetZ;oilerOffset=@(-.01,.42,.1)})}
$projection | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $output 'scotch-frame.json') -Encoding utf8
'{"layouts":[]}' | Set-Content (Join-Path $output 'no-articulated.json') -Encoding utf8
$python='C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe'
& $python (Join-Path $root 'evidence\hidden-linkages\measure_native.py') --output (Join-Path $output 'native-measurements.json')
if($LASTEXITCODE -ne 0){throw 'Native linkage measurement failed'}
$stubs=Get-Content -LiteralPath (Join-Path $root 'evidence\fourteen-spacing-tests\Stubs.cs') -Raw
$duplicates=@('public bool IsChildOf(Transform t)','public static Vector3 Min(Vector3 a,Vector3 b)','public static Vector3 Max(Vector3 a,Vector3 b)','public static Quaternion Inverse(Quaternion q)','public static Quaternion operator *(Quaternion a,Quaternion b)','public static Vector3 operator *(Quaternion q,Vector3 v)')
foreach($signature in $duplicates) {
 $pattern='(?m)^\s*'+[regex]::Escape($signature)+'[^\r\n]*\r?\n'
 if(([regex]::Matches($stubs,$pattern)).Count -ne 1){throw "Shared linkage stub duplicate changed: $signature"}
 $stubs=[regex]::Replace($stubs,$pattern,'')
}
$stubs=$stubs.Replace('public sealed class Transform :','public sealed partial class Transform :').Replace('public struct Matrix4x4','public partial struct Matrix4x4').Replace('public struct Quaternion','public partial struct Quaternion').Replace('public struct Vector3','public partial struct Vector3').Replace('public sealed class Mesh :','public sealed partial class Mesh :')
$stubs=$stubs.Replace('public Vector3[] vertices=Array.Empty<Vector3>();','private Vector3[] data=Array.Empty<Vector3>(); public Vector3[] FixturePositions=>data.ToArray(); public Vector3[] vertices {get {if(!isReadable)throw new InvalidOperationException("Native mesh is CPU-unreadable");return data.ToArray();}set{data=value.ToArray();}}').Replace('public int vertexCount=>vertices.Length;','public int vertexCount=>data.Length;').Replace('public void RecalculateBounds()=>throw new InvalidOperationException("Legacy retarget must retain its existing LOD bounds policy");','public void RecalculateBounds(){}')
$stubPath=Join-Path $output 'Fixture.Stubs.cs';Set-Content -LiteralPath $stubPath -Value $stubs -Encoding utf8
$mesh=Get-Content -LiteralPath (Join-Path $root 'RearrangedS282\MeshFinder.cs') -Raw
$start=$mesh.IndexOf('internal static void NormalizeLinkageImport(Mesh mesh)',[StringComparison]::Ordinal)
if($start -lt 0){throw 'Missing native OBJ coordinate normalizer'}
$open=$mesh.IndexOf('{',$start);$depth=1;$end=$open+1
while($depth -gt 0 -and $end -lt $mesh.Length){if($mesh[$end] -eq '{'){$depth++};if($mesh[$end] -eq '}'){$depth--};$end++}
if($depth -ne 0){throw 'Unbalanced normalizer'}
$normalizer=Join-Path $output 'ActualNormalization.cs'
Set-Content -LiteralPath $normalizer -Value ('using UnityEngine; namespace RearrangedS282 { internal sealed partial class MeshFinder { '+$mesh.Substring($start,$end-$start)+' } }') -Encoding utf8
$prod=@('DriveRetarget.cs','RodRemesher.cs','WheelRearranger.HiddenLinkages.cs','WheelArrangement.cs')|ForEach-Object {'<Compile Include="'+(Join-Path $root ('RearrangedS282\'+$_))+'"/>'}
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup><ItemGroup>$($prod -join '')<Compile Include="$normalizer"/><Compile Include="$stubPath"/><Compile Include="$root\evidence\hidden-linkages\Program.cs"/><Compile Include="$root\evidence\hidden-linkages\Owner.cs"/><Compile Include="$root\evidence\hidden-linkages\Stubs.cs"/></ItemGroup></Project>
"@
$path=Join-Path $output 'ScotchLinkages.Validation.csproj';Set-Content -LiteralPath $path -Value $project -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $path -c Release -- (Join-Path $root 'build\franklin-runtime\geometry.json') (Join-Path $root 'evidence\pacific-hudson-valve-gear\native-animation.json') (Join-Path $output 'native-measurements.json') (Join-Path $output 'validation.json') (Join-Path $output 'scotch-frame.json') (Join-Path $output 'no-articulated.json')
if($LASTEXITCODE -ne 0){throw 'Scotch native linkage geometry failed'}
$result=Get-Content -LiteralPath (Join-Path $output 'validation.json') -Raw|ConvertFrom-Json
$result|Add-Member -NotePropertyName scotchGeometrySHA256 -NotePropertyValue (Get-FileHash -LiteralPath $GeometryReceipt).Hash
$result|Add-Member -NotePropertyName projectionNote -NotePropertyValue 'ID49 frame taken from production-hook managed geometry receipt, not complete live caller or gameplay.'
$result|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $output 'validation.json') -Encoding utf8
