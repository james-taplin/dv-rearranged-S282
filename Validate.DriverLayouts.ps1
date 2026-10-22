param(
    [string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\driver-layout-validation'),
    [string]$OwnerSourcePath=(Join-Path $PSScriptRoot 'RearrangedS282\WheelRearranger.DriverLayouts.cs'),
    [switch]$TargetedCacheOnly
)
$ErrorActionPreference='Stop'
$OutputDirectory=[IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $OutputDirectory -Force|Out-Null
$fixture=Join-Path $PSScriptRoot 'evidence\driver-layout-tests'
$native=Join-Path $OutputDirectory 'native-rods.json'
& 'C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe' (Join-Path $PSScriptRoot 'evidence\rod-remesh-tests\extract_native.py') --output $native
if($LASTEXITCODE -ne 0){throw 'Native coupling rod fixture extraction failed'}
$project=@"
<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup>
<ItemGroup><Compile Include="$PSScriptRoot\RearrangedS282\DriverLayout.cs"/><Compile Include="$OwnerSourcePath"/><Compile Include="$PSScriptRoot\RearrangedS282\RodRemesher.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\PoweredWheelRotater.cs"/><Compile Include="$PSScriptRoot\RearrangedS282\WheelRearranger.Additional.cs"/><Compile Include="$fixture\Stubs.cs"/><Compile Include="$fixture\Owner.cs"/><Compile Include="$fixture\Program.cs"/></ItemGroup>
</Project>
"@
$projectPath=Join-Path $OutputDirectory 'DriverLayouts.Validation.csproj'
$report=Join-Path $OutputDirectory 'validation.json'
Set-Content -LiteralPath $projectPath -Value $project -Encoding UTF8
$arguments=@('run','--project',$projectPath,'-c','Release','--',$native,(Join-Path $PSScriptRoot 'evidence\pacific-hudson-valve-gear\native-animation.json'),(Join-Path $PSScriptRoot 'evidence\wheel-clearance-tests\native-clearance.json'),$report)
if($TargetedCacheOnly){$arguments+='cache-phase-only'}
& 'C:\Program Files\dotnet\dotnet.exe' @arguments
if($LASTEXITCODE -ne 0){throw 'Driver layout integration validation failed'}
Get-Content -LiteralPath $report -Raw
