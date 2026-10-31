31 October 2026 correction: release-player bootstrap now loads the native game
and transform assemblies and initializes TypeManager BEFORE loading the probe,
matching UMM late loading. This reproduced the original unknown Ownership tag.
Production no longer introduces a custom ECS component. The corrected late-load
run passes 46 native and 13 animation checks; see BUGS_2026_10_31.md. Older results
below used early assembly discovery and did not cover this startup boundary.

# Native calculation-helper feasibility experiment

This branch's first implementation unit is the actual production
`NativeSlipEntitySchema.cs`, linked directly into both probes. It defines
separate native adhesion-carrier and slip-group archetypes, activation identity,
and writes inputs without resetting native smoothing output. It is not registered
with normal game simulation yet and cannot apply force.

`InputContractProbe` executes the production input validator against native game
data structures outside Unity. It checks valid signed force inputs and rejects
non-finite values, invalid contacts, brake budget and host-slide data. Final run:
19 passed. This does not execute an ECS World or the native slip calculation.

`NativeSlipWorldProbe` is a force-free, separate-World experiment that invokes
the installed native AdhesionControllerSystem and WheelslipControllerSystem.
It uses an isolated synthetic environment, inactive native singleton objects,
explicit system updates, constant and curve-driven adhesion-drop inputs, and
Burst disabled. A diagnostic-only first-group grip reduction also tests equal
demands with different adhesion conditions.
It tests query isolation, zero-speed grip, independent and simultaneous slip,
recovery, sand input, copied host slide and activation identity. Even a passing
result would not establish real vehicle integration, actual World registration,
conversion-wrapper lifetime, weather, resource consumption, finite boiler,
animation or physical forces. Those remain separate plan gates.

The World probe must run only through its empty Unity fixture. Its bootstrap
loads installed assemblies dynamically after Unity starts; it must not be
installed into the game because the synthetic environment is fixture-only.
The guard rejects an already-existing default ECS World.

Commands from the worktree root:

```powershell
dotnet run --project evidence/native-slip-world/InputContractProbe.csproj -c Release
./Validate.NativeSlipWorld.ps1 -CompileOnly
./Validate.NativeSlipWorld.ps1
```

The default runner uses the normal editor launch to build a non-development
Mono standalone player, then executes the test there. Both windows are hidden.
This matches the shipped ECS assembly layout; importing the player assemblies
into Editor Play Mode failed native job reflection because their NativeContainer
layout lacks the editor-only safety handles. Do not disable the calculation
assertions to work around that platform mismatch. `-PlayerMode:$false` retains
the original editor-context experiment for diagnosis only; `-EditorMode:$false`
selects batch editor startup, which still rejected this machine's refreshed
license during this session.

The validation runner prepares a fresh private fixture and captures source/native
assembly hashes and results under build/native-slip-world/runs. Each probe has
separate MSBuild/NuGet intermediate state; the initial mixed-target restore
collision was fixed with this directory's Directory.Build.props.

Initial 28 October 2026 checkpoint: all probe and Stage 3 compilation checks passed with zero
warnings/errors. Bootstrap source also compiled against the installed editor's
managed assemblies. Actual Unity execution is **blocked at editor licensing**.
Both sandboxed and unsandboxed Unity 2019.4.40f1 startup attempts logged no valid
license; the unsandboxed attempt exited 1 before creating the fixture. No World
test assertions ran. Startup logs: build/native-slip-world/create.log and
create-native.log. No result should be inferred from the compiled test code.

After James refreshed the license, normal editor startup succeeded. The
standalone Unity 2019.4.40f1 release-player run passes 25 actual native-system
checks. The first successful expanded run is
build/native-slip-world/runs/20261028-230737-119/result.json. The finalized
runner/provenance run is under 20261028-230913-032; its receipt additionally
requires clean editor/player exit codes. Read that run's receipt for the final
verdict, rather than assuming the earlier pass covers a changed runner.

Fixture corrections discovered by execution: load UnityEngine facade and
Unity.Entities.Hybrid before native TypeManager initialization; initialize the
synthetic spawner's pool-ready state; and check owned entity identities/counts
separately from the native WorldTime singleton created by World.SetTime. The
cleanup check requires all three helper identities absent, zero owned helpers,
and exactly the native time entity remaining before World disposal.

The calculation portion of the native-helper feasibility gate now has actual
player evidence. Native conversion-wrapper retirement, automatic game system
ordering, real car data/forces, animation and playable behaviour remain untested.
