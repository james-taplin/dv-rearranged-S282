param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build/exporter-2.0.7/path-tests'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$out=(Resolve-Path -LiteralPath $OutputDirectory).Path
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit></PropertyGroup><ItemGroup><Compile Include="$PSScriptRoot/RearrangedS282/MeshExportPaths.cs"/><Compile Include="$PSScriptRoot/evidence/exporter-paths/Program.cs"/></ItemGroup></Project>
"@
$file=Join-Path $out 'Paths.csproj';Set-Content -LiteralPath $file -Value $project
$fixture=Join-Path $out ('clean-install-'+[Guid]::NewGuid().ToString('N'))
& 'C:/Program Files/dotnet/dotnet.exe' run --project $file -c Release -- $fixture
if($LASTEXITCODE -ne 0){throw 'Exporter path validation failed'}
