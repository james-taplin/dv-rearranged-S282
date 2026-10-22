param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build\compound-prototype'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$reference='B:\GitHub repo\rearranged-s060\RearrangedS060\MeyerCompoundCycle.cs'
$before=(Get-FileHash -LiteralPath $reference).Hash
# Freeze the read-only comparator in ignored build output, not in shipping assets.
Copy-Item -LiteralPath $reference -Destination (Join-Path $OutputDirectory 'S060Reference.cs') -Force
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup><ItemGroup><Compile Include="$PSScriptRoot\experiments\compound\CompoundCyclePrototype.cs"/><Compile Include="$PSScriptRoot\experiments\compound\Program.cs"/><Compile Include="$OutputDirectory\S060Reference.cs"/></ItemGroup></Project>
"@
$projectPath=Join-Path $OutputDirectory 'CompoundPrototype.csproj'
Set-Content -LiteralPath $projectPath -Value $project -Encoding utf8
& 'C:\Program Files\dotnet\dotnet.exe' run --project $projectPath -c Release -- (Join-Path $OutputDirectory 'validation.json')
if ($LASTEXITCODE -ne 0) { throw 'Compound prototype failed' }
if ((Get-FileHash -LiteralPath $reference).Hash -ne $before) { throw 'S060 reference changed during comparison; revalidate' }
[ordered]@{reference=$reference;sha256=$before;sourceHashes=@(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'experiments\compound') -Filter '*.cs' | Get-FileHash | Select-Object Path,Hash)} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'source-receipt.json') -Encoding utf8
