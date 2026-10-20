$ErrorActionPreference = 'Stop'
# Offline checks of RearrangedS282\BigEndOilingPlan.cs: oiling-point allocation per
# big end and fitting older saves. Native cup wiring needs in-game testing.
$output = Join-Path $PSScriptRoot 'build\big-end-oiling-validation'
New-Item -ItemType Directory -Force $output | Out-Null
$harness = @'
using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using static RearrangedS282.BigEndOilingPlan;
namespace RearrangedS282
{
 public static class BigEndOilingHarness
 {
  static int checks;
  static void Check(bool ok, string label) { if (!ok) throw new Exception("FAIL: " + label); checks++; }
  static BigEnd[] Engine(bool left, float firstZ, int wheels, int nativeIndex) =>
   Enumerable.Range(0, wheels).Select(i => new BigEnd(left, firstZ - 1.5f * i, i == nativeIndex)).ToArray();
  public static int Main()
  {
   Check(PointIndex(true, 0) == 6 && PointIndex(true, 6) == 12 && PointIndex(false, 0) == 13 && PointIndex(false, 6) == 19 && TotalPoints == 20, "Native six first, then seven left and seven right");
   bool threw = false; try { PointIndex(true, 7); } catch (ArgumentOutOfRangeException) { threw = true; }
   Check(threw, "Slot beyond seven rejected");

   // 2-8-2: four drivers a side, main (third) driver keeps its native cup.
   var mikado = Engine(true, 8, 4, 2).Concat(Engine(false, 8, 4, 2)).ToArray();
   var a = Allocate(mikado, out int dropped);
   Check(dropped == 0 && a.Count(x => x >= 0) == 6, "2-8-2: three extra cups a side");
   Check(a[2] == -1 && a[6] == -1, "Main-rod big ends keep the native cup");
   Check(a[0] == 6 && a[1] == 7 && a[3] == 8, "Left slots fill front to back");
   Check(a[4] == 13 && a[5] == 14 && a[7] == 15, "Right slots fill front to back");

   // 0-8-8-0: two four-axle engines a side; only the front engine's main has a native cup.
   var angus = Engine(true, 9, 4, 2).Concat(Engine(true, 3, 4, -1)).Concat(Engine(false, 9, 4, 2)).Concat(Engine(false, 3, 4, -1)).ToArray();
   a = Allocate(angus, out dropped);
   Check(dropped == 0 && a.Count(x => x >= 0) == 14 && a.Where(x => x >= 0).Distinct().Count() == 14, "0-8-8-0: seven extra cups a side, all distinct");
   Check(a.Take(8).Where(x => x >= 0).All(x => x >= 6 && x <= 12) && a.Skip(8).Where(x => x >= 0).All(x => x >= 13 && x <= 19), "Each side uses its own points");

   // Triplex: three single-driver engines a side.
   var triplex = Enumerable.Range(0, 3).Select(i => new BigEnd(true, 7 - 3 * i, i == 0)).Concat(Enumerable.Range(0, 3).Select(i => new BigEnd(false, 7 - 3 * i, i == 0))).ToArray();
   a = Allocate(triplex, out dropped);
   Check(dropped == 0 && a.Count(x => x >= 0) == 4, "Triplex: middle and rear engines get a cup each side");

   // More big ends than spare points.
   var many = Engine(true, 20, 10, -1).Concat(Engine(false, 20, 10, -1)).ToArray();
   a = Allocate(many, out dropped);
   Check(dropped == 6 && a.Count(x => x >= 0) == 14, "Overflow reported, never double-booked");

   var active = ActivePoints(Allocate(mikado, out _));
   Check(active.Length == TotalPoints && active.Take(6).All(x => x), "Native points always active");
   Check(active[6] && active[8] && !active[9] && active[13] && !active[16], "Only allocated extra points are active");
   Check(ActivePoints(new int[0]).Skip(6).All(x => !x), "No arrangement: every extra point held");

   var old = new JArray(Enumerable.Range(0, 6).Select(i => new JObject { ["open"] = i == 1, ["oilLevel"] = .1f * i }));
   var fitted = FitSavedPoints(old, TotalPoints);
   Check(fitted.Count == 20 && (float)fitted[5]["oilLevel"] == .5f && (bool)fitted[1]["open"], "Native levels and lids survive");
   Check(fitted.Skip(6).All(p => (float)p["oilLevel"] == 1f && !(bool)p["open"]), "New points start full and closed");
   Check(!ReferenceEquals(fitted[0], old[0]), "Saved data is copied, not shared");
   var longer = new JArray(Enumerable.Range(0, 25).Select(i => new JObject { ["oilLevel"] = .5f }));
   Check(FitSavedPoints(longer, TotalPoints).Count == 20, "Longer saves trimmed");
   Check(FitSavedPoints(null, TotalPoints).All(p => (float)p["oilLevel"] == 1f), "Missing save data starts full");
   Console.WriteLine(checks);
   return 0;
  }
 }
}
'@
$harnessPath = Join-Path $output 'BigEndOilingHarness.cs'
Set-Content -LiteralPath $harnessPath -Value $harness -Encoding utf8
$plan = Join-Path $PSScriptRoot 'RearrangedS282\BigEndOilingPlan.cs'
$managed = 'B:\SteamLibrary\steamapps\common\Derail Valley\DerailValley_Data\Managed'
$project = @"
<Project Sdk="Microsoft.NET.Sdk">
 <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>OIL_PLAN_TEST</DefineConstants><NuGetAudit>false</NuGetAudit><StartupObject>RearrangedS282.BigEndOilingHarness</StartupObject></PropertyGroup>
 <ItemGroup><Compile Include="$harnessPath"/><Compile Include="$plan"/></ItemGroup>
 <ItemGroup><Reference Include="Newtonsoft.Json"><HintPath>$managed\Newtonsoft.Json.dll</HintPath></Reference></ItemGroup>
</Project>
"@
$projectPath = Join-Path $output 'BigEndOiling.Validation.csproj'
Set-Content -LiteralPath $projectPath -Value $project -Encoding utf8
$result = & 'C:/Program Files/dotnet/dotnet.exe' run --project $projectPath --configuration Release
if ($LASTEXITCODE -ne 0) { $result; throw 'Big-end oiling validation failed.' }
$count = [int]($result | Select-Object -Last 1)
$report = [ordered]@{ passed=$true;checks=$count;source='RearrangedS282\BigEndOilingPlan.cs';limitations='Allocation and save fitting only; native cup cloning, controls, refilling and drain need in-game testing.' }
$report | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $output 'validation.json') -Encoding utf8
$report | ConvertTo-Json
