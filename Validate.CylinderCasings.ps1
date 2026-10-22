param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\cylinder-casing-validation'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$source=Join-Path $PSScriptRoot 'RearrangedS282\CylinderCasingShape.cs'
$fixture=Join-Path $PSScriptRoot 'evidence\cylinder-casing-tests'
$meshFixture=Join-Path $fixture 'native-casings.json'
if(-not(Test-Path -LiteralPath $meshFixture)) {
    $python='C:\Users\james\AppData\Local\Python\pythoncore-3.14-64\python.exe'
    & $python (Join-Path $fixture 'extract_native.py')
    if($LASTEXITCODE -ne 0){throw 'Actual native cylinder mesh extraction failed.'}
}
$project=@"
<Project Sdk="Microsoft.NET.Sdk">
 <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>B99_ALL_STANDARD</DefineConstants><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup>
 <ItemGroup><Compile Include="$source"/><Compile Include="$fixture\Stubs.cs"/><Compile Include="$fixture\Program.cs"/></ItemGroup>
</Project>
"@
$projectPath=Join-Path $OutputDirectory 'CylinderCasings.Validation.csproj'
Set-Content -LiteralPath $projectPath -Value $project -Encoding utf8
$report=Join-Path $OutputDirectory 'validation.json'
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath --configuration Release -- $meshFixture $report
if($LASTEXITCODE -ne 0){throw "Cylinder casing validation failed. See $report"}
$result=Get-Content -LiteralPath $report -Raw | ConvertFrom-Json
$result|Add-Member -NotePropertyName sourceSHA256 -NotePropertyValue (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
$result|Add-Member -NotePropertyName actualMeshFixtureSHA256 -NotePropertyValue (Get-FileHash -LiteralPath $meshFixture -Algorithm SHA256).Hash
$result|ConvertTo-Json -Depth 8|Set-Content -LiteralPath $report -Encoding utf8
Write-Output ($result|Select-Object passed,assertions,cases,sourceSHA256,actualMeshFixtureSHA256|ConvertTo-Json)
