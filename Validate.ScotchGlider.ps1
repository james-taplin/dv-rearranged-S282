param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\offline-validation\ScotchGlider'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$output=(Resolve-Path -LiteralPath $OutputDirectory).Path
$root=$PSScriptRoot
$native=Join-Path $output 'native-clearance.json'
$extractSource=Get-Content -LiteralPath (Join-Path $root 'evidence\wheel-clearance-tests\extract_native.py') -Raw
$original="out=root/'evidence/wheel-clearance-tests/native-clearance.json'"
if(-not $extractSource.Contains($original)){throw 'Native extractor output binding changed; review before running.'}
$extracted=$extractSource.Replace($original,'out=Path(sys.argv[1])')
$extractPath=Join-Path $output 'ExtractNative.csfixture.py'
Set-Content -LiteralPath $extractPath -Value $extracted -Encoding utf8
$python='C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe'
& $python $extractPath $native
if($LASTEXITCODE -ne 0){throw 'Native wheel and reservoir extraction failed'}
$project=@"
<Project Sdk="Microsoft.NET.Sdk">
 <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup>
 <ItemGroup>
  <Compile Include="$root\RearrangedS282\WheelRearranger.ScotchGlider.cs" />
  <Compile Include="$root\RearrangedS282\WheelRearranger.Additional.cs" />
  <Compile Include="$root\RearrangedS282\PoweredWheelRotater.cs" />
  <Compile Include="$root\evidence\additional-arrangements-tests\Stubs.cs" />
  <Compile Include="$root\evidence\scotch-glider-tests\Owner.cs" />
  <Compile Include="$root\evidence\scotch-glider-tests\Program.cs" />
 </ItemGroup>
</Project>
"@
$projectPath=Join-Path $output 'ScotchGlider.Validation.csproj'
Set-Content -LiteralPath $projectPath -Value $project -Encoding utf8
$managed=Join-Path $output 'managed.json'
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- $native $managed
if($LASTEXITCODE -ne 0){throw 'Scotch Glider managed geometry failed'}
& $python (Join-Path $root 'evidence\scotch-glider-tests\geometry.py') $native $managed (Join-Path $output 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Scotch Glider native geometry validation failed'}
