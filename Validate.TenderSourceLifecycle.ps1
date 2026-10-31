param([string]$OutputDirectory=(Join-Path $PSScriptRoot 'build/runtime-trial-1.23.1/tender-source'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$source=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RearrangedS282/WheelRearranger.TenderEngine.cs') -Raw
function Extract([string]$text,[string]$marker){$a=$text.IndexOf($marker);if($a -lt 0){throw $marker};$b=$text.IndexOf('{',$a);$depth=0;for($i=$b;$i -lt $text.Length;$i++){if($text[$i] -eq '{'){$depth++};if($text[$i] -eq '}'){$depth--;if($depth -eq 0){return $text.Substring($a,$i-$a+1)}}};throw $marker}
$polish=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RearrangedS282/WheelRearranger.TenderPolish.cs') -Raw
$articulated=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RearrangedS282/WheelRearranger.Articulated.cs') -Raw
$actual="using System;using System.Linq;using System.Collections.Generic;using UnityEngine;using Object=UnityEngine.Object;namespace RearrangedS282 { internal partial class WheelRearranger {`n"+(Extract $source 'private void EnsurePoweredTenderSource')+"`n"+(Extract $source 'private void CloneTenderStatics')+"`n"+(Extract $polish 'private void BuildTenderFrameSources')+"`n}`n"+(Extract $articulated 'internal static class ArticulatedFrameClipper')+"`n}"
Set-Content -LiteralPath (Join-Path $OutputDirectory 'ActualTenderSource.cs') -Value $actual -Encoding utf8
$scene=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'evidence/steam-pipe-tests/ActorStubs.cs') -Raw
$scene=$scene.Replace('public static Mesh Instantiate(Mesh source) => source.Clone();',@'
public static Mesh Instantiate(Mesh source) => source.Clone();
public static GameObject Instantiate(GameObject source, Transform parent) {
 var clone=new GameObject(source.name);clone.transform.SetParent(parent,false);
 clone.transform.localPosition=source.transform.localPosition;clone.transform.localRotation=source.transform.localRotation;clone.transform.localScale=source.transform.localScale;
 clone.SetActive(source.activeSelf);
 clone.layer=source.layer;
 foreach(var filter in source.GetComponents<MeshFilter>())clone.AddComponent<MeshFilter>().sharedMesh=filter.sharedMesh;
 foreach(var renderer in source.GetComponents<MeshRenderer>())clone.AddComponent<MeshRenderer>().sharedMaterials=renderer.sharedMaterials;
 foreach(var child in source.transform.children)Instantiate(child.gameObject,clone.transform);
 return clone;
}
'@)
$scene=$scene.Replace('public Vector3 forward=>TransformDirection(Vector3.forward);','public Vector3 lossyScale=>new(localToWorldMatrix.MultiplyVector(Vector3.right).magnitude,localToWorldMatrix.MultiplyVector(Vector3.up).magnitude,localToWorldMatrix.MultiplyVector(Vector3.forward).magnitude); public Vector3 forward=>TransformDirection(Vector3.forward);')
$scene=$scene.Replace('public Matrix4x4 inverse {','public Matrix4x4 transpose=>new(){n=N.Matrix4x4.Transpose(n)}; public Matrix4x4 inverse {')
$scene=$scene.Replace('public Vector3 normalized=>this/magnitude;','public Vector3 normalized=>magnitude>1e-10f?this/magnitude:zero; public static Vector3 LerpUnclamped(Vector3 a,Vector3 b,float t)=>a+(b-a)*t;')
$scene=$scene.Replace('if (worldPositionStays) throw new NotSupportedException(); parent?.children.Remove(this); parent = t; t?.children.Add(this);',@'
var world=localToWorldMatrix; parent?.children.Remove(this); parent=t; t?.children.Add(this);
if(worldPositionStays){var local=(t==null?Matrix4x4.identity:t.worldToLocalMatrix)*world;if(!N.Matrix4x4.Decompose(local.n,out var scale,out var rotation,out var position))throw new InvalidOperationException("Fixture affine decomposition failed");localPosition=Vector3.From(position);localRotation=Quaternion.From(rotation);localScale=Vector3.From(scale);}
'@)
# Reuse the geometry fixture's multi-channel/submesh storage so the real frame
# clipper runs here too, rather than replacing BuildTenderFrameSources with a stub.
$geometry=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'evidence/articulated-geometry-tests/Stubs.cs') -Raw
$mesh=(Extract $geometry 'public sealed class Mesh : Object').Replace('public string name;',@'
public int vertexCount=>vertices.Length;
public Bounds bounds;
public Mesh Clone(){var copy=new Mesh{name=name,isReadable=isReadable,vertices=vertices.ToArray(),normals=normals.ToArray(),tangents=tangents.ToArray(),colors=colors.ToArray(),boneWeights=boneWeights.ToArray(),blendShapeCount=blendShapeCount,indexFormat=indexFormat,subMeshCount=subMeshCount,bounds=bounds};foreach(var pair in uv)copy.uv.Add(pair.Key,pair.Value.ToArray());foreach(var pair in triangles)copy.triangles.Add(pair.Key,pair.Value.ToArray());foreach(var pair in topology)copy.topology.Add(pair.Key,pair.Value);return copy;}
'@)
$scene=$scene.Replace((Extract $scene 'public sealed class Mesh : Object'),$mesh)
$extras="namespace UnityEngine {`n"+(Extract $geometry 'public struct Vector4')+"`n"+(Extract $geometry 'public struct Color')+"`n public struct BoneWeight {} public enum MeshTopology { Triangles, Lines } }`nnamespace UnityEngine.Rendering { public enum IndexFormat { UInt16,UInt32 } }"
$scene+="`n"+$extras
Set-Content -LiteralPath (Join-Path $OutputDirectory 'SceneStubs.cs') -Value $scene -Encoding utf8
$project=@"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup><ItemGroup><Compile Include="$OutputDirectory/ActualTenderSource.cs"/><Compile Include="$OutputDirectory/SceneStubs.cs"/><Compile Include="$PSScriptRoot/evidence/tender-source-tests/Program.cs"/></ItemGroup></Project>
"@
$path=Join-Path $OutputDirectory 'TenderSource.csproj';Set-Content -LiteralPath $path -Value $project -Encoding utf8
& 'C:/Program Files/dotnet/dotnet.exe' run --project $path -c Release -- (Join-Path $OutputDirectory 'validation.json')
if($LASTEXITCODE -ne 0){throw 'Tender source lifecycle regression failed'}
