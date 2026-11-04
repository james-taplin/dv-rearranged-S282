param(
 [string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\offline-validation\ScotchRods'),
 [string]$GeometryReceipt=(Join-Path $PSScriptRoot 'build\offline-validation\ScotchGlider\validation.json'),
 [string]$NativeWheels=(Join-Path $PSScriptRoot 'build\offline-validation\ScotchGlider\native-clearance.json')
)
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$out=(Resolve-Path -LiteralPath $OutputDirectory).Path;$root=$PSScriptRoot
$receipt=Get-Content -LiteralPath $GeometryReceipt -Raw|ConvertFrom-Json
if(-not $receipt.passed -or $receipt.arrangementId -ne 49 -or $receipt.managedSourceSHA256 -ne (Get-FileHash -LiteralPath (Join-Path $root 'RearrangedS282\WheelRearranger.ScotchGlider.cs')).Hash){throw 'Current Scotch geometry receipt is required'}
if($receipt.nativeDataSHA256 -ne (Get-FileHash -LiteralPath $NativeWheels).Hash){throw 'Native wheel data does not match Scotch geometry receipt'}
$python='C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe'
$native=Join-Path $out 'native-rods.json'
& $python (Join-Path $root 'evidence\rod-remesh-tests\extract_native.py') --output $native
if($LASTEXITCODE -ne 0){throw 'Native three-axle rod extraction failed'}
$source=Get-Content -LiteralPath (Join-Path $root 'evidence\driver-layout-tests\Program.cs') -Raw
$marker='    private static void Main(string[] args)';$where=$source.IndexOf($marker,[StringComparison]::Ordinal)
if($where -lt 0){throw 'Driver-layout fixture entry point changed'}
$test=Get-Content -LiteralPath (Join-Path $root 'evidence\scotch-glider-tests\RodMain.cs') -Raw
$fixture=Join-Path $out 'ScotchRodProgram.cs';Set-Content -LiteralPath $fixture -Value ($source.Substring(0,$where)+$test) -Encoding utf8
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup><ItemGroup><Compile Include="$root\RearrangedS282\DriverLayout.cs"/><Compile Include="$root\RearrangedS282\WheelRearranger.DriverLayouts.cs"/><Compile Include="$root\RearrangedS282\RodRemesher.cs"/><Compile Include="$root\RearrangedS282\PoweredWheelRotater.cs"/><Compile Include="$root\RearrangedS282\WheelRearranger.Additional.cs"/><Compile Include="$root\evidence\driver-layout-tests\Stubs.cs"/><Compile Include="$root\evidence\driver-layout-tests\Owner.cs"/><Compile Include="$fixture"/></ItemGroup></Project>
"@
$path=Join-Path $out 'ScotchRods.csproj';Set-Content -LiteralPath $path -Value $project -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $path -c Release -- $native (Join-Path $root 'evidence\pacific-hudson-valve-gear\native-animation.json') $NativeWheels $GeometryReceipt (Join-Path $out 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Scotch native coupling-rod/pin test failed'}
