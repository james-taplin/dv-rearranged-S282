param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build/runtime-trial-1.23.1/tender-source'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$source=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RearrangedS282/WheelRearranger.TenderEngine.cs') -Raw
function Extract([string]$marker){$a=$source.IndexOf($marker);if($a -lt 0){throw $marker};$b=$source.IndexOf('{',$a);$depth=0;for($i=$b;$i -lt $source.Length;$i++){if($source[$i] -eq '{'){$depth++};if($source[$i] -eq '}'){$depth--;if($depth -eq 0){return $source.Substring($a,$i-$a+1)}}};throw $marker}
$actual="using System;using System.Linq;using UnityEngine;using Object=UnityEngine.Object;namespace RearrangedS282 { internal partial class WheelRearranger {`n"+(Extract 'private void EnsurePoweredTenderSource')+"`n"+(Extract 'private void CloneTenderStatics')+"`n} }"
Set-Content -LiteralPath (Join-Path $OutputDirectory 'ActualTenderSource.cs') -Value $actual -Encoding utf8
$scene=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'evidence/steam-pipe-tests/ActorStubs.cs') -Raw
$scene=$scene.Replace('public static Mesh Instantiate(Mesh source) => source.Clone();',@'
public static Mesh Instantiate(Mesh source) => source.Clone();
public static GameObject Instantiate(GameObject source, Transform parent) {
 var clone=new GameObject(source.name);clone.transform.SetParent(parent,false);
 clone.transform.localPosition=source.transform.localPosition;clone.transform.localRotation=source.transform.localRotation;clone.transform.localScale=source.transform.localScale;
 clone.SetActive(source.activeSelf);
 foreach(var filter in source.GetComponents<MeshFilter>())clone.AddComponent<MeshFilter>().sharedMesh=filter.sharedMesh;
 foreach(var child in source.transform.children)Instantiate(child.gameObject,clone.transform);
 return clone;
}
'@)
$scene=$scene.Replace('public Vector3 forward=>TransformDirection(Vector3.forward);','public Vector3 lossyScale=>new(localToWorldMatrix.MultiplyVector(Vector3.right).magnitude,localToWorldMatrix.MultiplyVector(Vector3.up).magnitude,localToWorldMatrix.MultiplyVector(Vector3.forward).magnitude); public Vector3 forward=>TransformDirection(Vector3.forward);')
Set-Content -LiteralPath (Join-Path $OutputDirectory 'SceneStubs.cs') -Value $scene -Encoding utf8
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup><ItemGroup><Compile Include="$OutputDirectory/ActualTenderSource.cs"/><Compile Include="$OutputDirectory/SceneStubs.cs"/><Compile Include="$PSScriptRoot/evidence/tender-source-tests/Program.cs"/></ItemGroup></Project>
"@
$path=Join-Path $OutputDirectory 'TenderSource.csproj';Set-Content -LiteralPath $path -Value $project -Encoding utf8
& 'C:/Program Files/dotnet/dotnet.exe' run --project $path -c Release -- (Join-Path $OutputDirectory 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Tender source lifecycle regression failed'}
