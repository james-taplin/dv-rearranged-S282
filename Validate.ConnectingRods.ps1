param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\connecting-rod-validation'))
$ErrorActionPreference='Stop'
$OutputDirectory=[IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$fixture=Join-Path $PSScriptRoot 'evidence\connecting-rod-tests'
$native=Join-Path $OutputDirectory 'native-motion-meshes.json'
& 'C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe' (Join-Path $fixture 'extract_native.py') --output $native
if($LASTEXITCODE -ne 0){throw 'Native connecting rod extraction failed'}
$rodData=Join-Path $OutputDirectory 'native-coupling-rods.json'
& 'C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe' (Join-Path $PSScriptRoot 'evidence\rod-remesh-tests\extract_native.py') --output $rodData
if($LASTEXITCODE -ne 0){throw 'Native coupling rod fixture extraction failed'}
# Extend the independently maintained fixture in build/ only: reuse its actual
# legacy setters, hierarchy, native animation evaluator and wheel/LOD data.
$baseProgram=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'evidence\driver-layout-tests\Program.cs') -Raw
$baseProgram=$baseProgram.Replace('internal static class Program','internal static partial class Program').Replace('private static void PoseDrive','internal static void PoseDrive')
$programCopy=Join-Path $OutputDirectory 'DriverFixture.Program.cs'
Set-Content -LiteralPath $programCopy -Value $baseProgram -Encoding UTF8
$baseOwner=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'evidence\driver-layout-tests\Owner.cs') -Raw
$baseOwner=$baseOwner.Replace('internal enum WheelArrangementType{','internal enum WheelArrangementType{s282=10,s404=33,')
$baseOwner+="`nnamespace RearrangedS282 { internal partial class WheelRearranger { internal UnityEngine.Matrix4x4 CylinderFrameForFixture(UnityEngine.Transform drive)=>CylinderDriveFrame(drive); } }`n"
$ownerCopy=Join-Path $OutputDirectory 'DriverFixture.Owner.cs'
Set-Content -LiteralPath $ownerCopy -Value $baseOwner -Encoding UTF8
$baseStubs=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'evidence\driver-layout-tests\Stubs.cs') -Raw
$baseStubs=$baseStubs.Replace('public sealed class Transform','public sealed partial class Transform')
$baseStubs=$baseStubs.Replace('public static Vector3 operator -(Vector3 a,Vector3 b)','public static Vector3 operator -(Vector3 v)=>new(-v.x,-v.y,-v.z); public static Vector3 operator -(Vector3 a,Vector3 b)')
$baseStubs=$baseStubs.Replace('public Vector3[] vertices=Array.Empty<Vector3>();','private Vector3[] vertexData=Array.Empty<Vector3>(); public Vector3[] FixturePositions=>vertexData.ToArray(); public Vector3[] vertices { get {if(!isReadable)throw new InvalidOperationException("CPU read of unreadable native mesh");return vertexData.ToArray();}set{vertexData=value.ToArray();}}')
$baseStubs=$baseStubs.Replace('m.vertices.Select(v=>transform.InverseTransformPoint','m.FixturePositions.Select(v=>transform.InverseTransformPoint')
$baseStubs=$baseStubs.Replace('internal float mogulExtraCoupledPitchMetres=.15f','internal float prairieDriverRearwardMetres,americanDriverRearwardMetres; internal float mogulDriverRearwardMetres=.15f,twoFortyDriverRearwardMetres=.15f; internal float mogulExtraCoupledPitchMetres=.15f')
$baseStubs=$baseStubs.Replace('public UnityEngine.Mesh Load(string path)=>throw new InvalidOperationException("Unexpected fallback file IO in readable native-mesh fixture");','public static readonly Dictionary<string,UnityEngine.Mesh> Sources=new(); public static readonly List<string> Requests=new(); public UnityEngine.Mesh Load(string path){string name=System.IO.Path.GetFileNameWithoutExtension(path);Requests.Add(name);return Sources.TryGetValue(name,out var mesh)?UnityEngine.Object.Instantiate(mesh):null;}')
$stubCopy=Join-Path $OutputDirectory 'DriverFixture.Stubs.cs'
Set-Content -LiteralPath $stubCopy -Value $baseStubs -Encoding UTF8
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><StartupObject>ConnectingHarness</StartupObject><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup>
<ItemGroup><Compile Include="$PSScriptRoot\RearrangedS282\CylinderCasingShape.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\WheelRearranger.PrairieSpacing.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\WheelRearranger.LargeDriverClearance.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\DriverLayout.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\WheelRearranger.DriverLayouts.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\WheelRearranger.ConnectingRodSpacing.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\RodRemesher.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\PoweredWheelRotater.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\DriveRetarget.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\WheelRearranger.OilCupMounts.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\WheelRearranger.Additional.cs"/><Compile Include="$programCopy"/><Compile Include="$ownerCopy"/><Compile Include="$stubCopy"/><Compile Include="$fixture\Stubs.cs"/><Compile Include="$fixture\Owner.cs"/><Compile Include="$fixture\Program.cs"/></ItemGroup></Project>
"@
$projectPath=Join-Path $OutputDirectory 'ConnectingRods.Validation.csproj';$report=Join-Path $OutputDirectory 'validation.json'
Set-Content -LiteralPath $projectPath -Value $project -Encoding UTF8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- $rodData (Join-Path $PSScriptRoot 'evidence\pacific-hudson-valve-gear\native-animation.json') (Join-Path $PSScriptRoot 'evidence\wheel-clearance-tests\native-clearance.json') $native $report $OutputDirectory (Join-Path $PSScriptRoot 'evidence\cylinder-casing-tests\native-full-casing-fittings.json')
if($LASTEXITCODE -ne 0){throw 'Connecting rod integration validation failed'}
Get-Content -LiteralPath $report -Raw
