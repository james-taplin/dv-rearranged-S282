param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build/lubrication-polish/tests'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$stubs=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'evidence/steam-pipe-tests/ActorStubs.cs') -Raw
$stubs=$stubs.Replace('public class MonoBehaviour : Component { }','public class MonoBehaviour : Component { public bool isActiveAndEnabled => gameObject.activeInHierarchy; }')
$stubs=$stubs.Replace('public sealed class Animator:MonoBehaviour{}','public sealed class Animator:MonoBehaviour{ public object runtimeAnimatorController; }')
$stubs=$stubs.Replace('public Quaternion localRotation = Quaternion.identity;','public Quaternion localRotation = Quaternion.identity; public Vector3 localEulerAngles;')
$stubs=$stubs.Replace('public static class Mathf {','public static class Mathf { public static float Repeat(float a,float b)=>a-(float)Math.Floor(a/b)*b;')
$stubs=$stubs.Replace('public sealed class TrainCar:UnityEngine.MonoBehaviour{}','public sealed class TrainCar:UnityEngine.MonoBehaviour{ public Livery carLivery=new(); }')
Set-Content -LiteralPath (Join-Path $OutputDirectory 'ActorStubs.cs') -Value $stubs
$couplingTest = Join-Path $PSScriptRoot 'evidence/lubrication-polish/CouplingSeats.cs'
$pitchTest = Join-Path $PSScriptRoot 'evidence/lubrication-polish/MalletPitch.cs'
$project=@"
<Project Sdk="Microsoft.NET.Sdk">
 <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit></PropertyGroup>
 <ItemGroup><Compile Include="$PSScriptRoot/RearrangedS282/OilCupSeat.cs"/><Compile Include="$PSScriptRoot/RearrangedS282/WheelRearranger.Lubricator.cs"/><Compile Include="$PSScriptRoot/RearrangedS282/WheelRearranger.MalletDriverPitch.cs"/><Compile Include="$PSScriptRoot/evidence/lubrication-polish/Program.cs"/><Compile Include="$couplingTest"/><Compile Include="$pitchTest"/><Compile Include="$PSScriptRoot/evidence/lubrication-polish/LubricatorFixture.cs"/><Compile Include="$OutputDirectory/ActorStubs.cs"/></ItemGroup>
</Project>
"@
$projectPath=Join-Path $OutputDirectory 'Validation.csproj'
Set-Content -LiteralPath $projectPath -Value $project
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- (Join-Path $PSScriptRoot 'build/geometry-audit/coupling/native-rods.json') (Join-Path $PSScriptRoot 'evidence/pacific-hudson-valve-gear/native-animation.json') |
 Tee-Object -FilePath (Join-Path $OutputDirectory 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Lubrication geometry regression failed'}
