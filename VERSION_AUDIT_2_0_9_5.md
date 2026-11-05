# 2.0.9.5 pre-publication audit — 4 November 2026

Follow-up: the two runtime findings below are repaired in successor test build
2.0.9.6. Its affected checks, package hashes and pending James-owned gameplay
acceptance are recorded at the top of CURRENT_HANDOVER.md. The findings and
candidate hashes below describe 2.0.9.5 and remain historical evidence.

**Decision: hold Nexus publication for one remaining runtime defect.** The
candidate is suitable for development investigation, but its retargeted linkage
renderers can break a later explosion/repair cycle after an arrangement switch.
James's gameplay and live multiplayer acceptance are still pending. This audit
does not approve 2.1 stable.

## Exact candidate

- Source: `47e8ac747a17c61d01d0b5ef32206f62f54db104`, `codex/s282-multiplayer`.
- Core and bridge: `build/candidate-2.0.9.5/integrated/`, both version 2.0.9.5.
- Core SHA-256: `CEFA5185C91C084ABA5574573F46CFA4ACC1DC506DD1B9660727D0CC83F330D1`.
- Bridge SHA-256: `2E59C916427C64AF908689FEC5F08F2FBC5293AB97F02BC1F834663A5A286F86`.
- Installed core/bridge and the two corresponding desktop ZIP members match
  those hashes. Installed and packaged manifests also identify 2.0.9.5.
- ZIP: `C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-2.0.9.5-Multiplayer-Test.zip`.
  SHA-256: `3B966115536A471215066930002963CA9A9AD0B13D4943DDB8D7301693AFF2E8`.
- Compared portable-PDB document checksums to current disk sources: all 109 core
  and two bridge source documents match. No production source was changed during
  this audit. The existing integrated build's zero-warning/error result is
  retained; unchanged production was not rebuilt.
- Existing compiled metadata review confirms both new IDs/dispatches, ID50's
  rear-engine path, and all 19 bridge-to-core member references. Core remains
  free of static bridge/MultiplayerAPI dependencies. These checks establish
  build identity and linkage, not live network delivery.

## Findings

### P1 — retarget proxy renderers are destroyed without explosion retirement

`RearrangedS282/DriveRetarget.cs:430–447` restores LODs and destroys every proxy
GameObject, but never unregisters its renderer from `B99ExplosionMaterials` or
the native material-swap caches. `WheelRearranger.Conventional.cs:220` registers
these proxies through its include-inactive renderer scan. Hidden-linkage,
connecting-rod and articulated suspension all call `DriveRetarget.Unbind`.
This affects the new layouts as well as existing retargeted arrangements.

Trigger: apply an arrangement using retarget proxies, switch/reapply (including
back to stock), let deferred destruction complete, then explode the locomotive.
The native explosion handler writes `sharedMaterial` through the stale renderer
reference. A prior explosion/repair can also leave that reference in the native
body cache. The resulting exception can interrupt the material-change lifecycle.

The earlier Franklin-pin fix correctly retires its own renderers. Steam cleanup
does not cover retarget proxies: its same-frame `Forget` happens before Unity
destroys those proxies, and subsequent `Refresh` includes inactive children.
Pruning already-null entries from the added list alone is insufficient for the
native body cache and same-frame re-registration.

**Offline reproduction:** `build/audit-2.0.9.5/RetargetLifetime/` contains the actual
extracted production `Restore` body, the installed B99 `ExplosionModelHandler`,
and the existing managed Unity lifetime stand-ins. Both `fresh.log` and
`cached.log` report `Proxy remains registered before frame end: True` followed
by `REPRODUCED native explosion after actual DriveRetarget.Restore: Destroyed renderer write`.
The first case has no earlier explosion; the second primes the native cache.
This is a native-method offline reproduction, not a game-run exception report.

Reproduce with `C:\Program Files\dotnet\dotnet.exe run --project
build/audit-2.0.9.5/RetargetLifetime/RetargetLifetime.csproj -c Release -- fresh`
and then `-- cached`. Exit zero means the probe reproduced the defect; it does
not mean the production behavior passed.

Repair using the existing ownership mechanism: retire each owned proxy from
all relevant material-swap lists before deferred destruction, keep it excluded
from same-frame refresh, and preserve unrelated renderers. Extend the existing
renderer-lifetime regression to cover actual retarget cleanup, both cache states,
stock/reapply and pool restoration. Then make one integrated repair build and
revalidate the matching package before James's final beta testing.

### P3 — options header still advertises 49 arrangements

`RearrangedS282/Main.cs:86` contains `49 ARRANGEMENTS`; this candidate has 51.
Correct it with the runtime repair, preferably deriving the count from the
catalogue. It does not prevent either new arrangement from being selected.

### Offline fixture correction made during the audit

`evidence/structural-matrix-tests/Program.cs` still expected 93 cases, 49 ordinary
and 44 powered, despite the production matrix correctly generating 96 cases.
Updated the explicit acceptance counts to 51/45/96 and the ID coverage to 0–50;
the receipt now reports observed counts rather than historical constants.
The rerun passed 2,374 assertions. This changes only the fixture; no runtime
rebuild is necessary for this correction.

Repository README, multiplayer checklist and TODO also retained old installed
version/open-finding statements. Their current-status sections were updated;
historical release-copy and audit records remain historical. The test ZIP already
has current 2.0.9.5 tester instructions and changelog. It remains a development
test package, not an audited final Nexus release.

## Fresh validation

Evidence is under `build/audit-2.0.9.5/`. Commands run from the repository root
use `Validate.<Name>.ps1 -OutputDirectory build/audit-2.0.9.5/<Name>` unless
qualified below. All listed rows passed; the retarget defect above is separate.

| Check | Result |
| --- | --- |
| Multiplayer bridge/core | 219 / 170 checks |
| Independent multiplayer | 32 native-port adapter / 27 authority checks |
| RendererLifetime | Native-cache, same-frame and pool cases pass for Franklin pins |
| RestoredFeatures | 286 assertions, 28 transitions, 11 simulated explosions |
| ObjMesh | 21 checks plus old-importer parity for shipped/native OBJ inputs |
| RadioSettings / RandomLifecycle / SpawnOwnership | 468 / 1,514 / 75 |
| CatalogueContinuity | 399; existing IDs 0–48 and tender policies preserved |
| StructuralMatrix after fixture correction | 2,374; 51 ordinary + 45 powered cases |
| ConnectingRods | 905,299 |
| DriverLayouts | 26,181 |
| AdditionalArrangements | 1,954 |
| FourteenSpacing | 1,452 |
| PacificHudsonValveGear | 15,717 |
| PrairieCylinders / PrairieMotion | 369,146 / 92,237 |
| ScotchGlider / ScotchDispatch | 88 geometry / 22 dispatch checks |
| LittleBoy | Wrapper/dispatch/native spacing fixture passed |
| PreliminaryPhysics | 9,602 checks, 273 layout/mass cases, 3,400 native ticks |
| ArticulatedSimulation | 72 native-engine + 56 graph/lifecycle checks |
| ArticulatedEffects / BigEndOilingOwnership | 9,014 / 655 |
| MultiplayerPackage | 80, including clean archive layout/dependencies/assets |

CatalogueContinuity uses `-OutputPath build/audit-2.0.9.5/CatalogueContinuity/validation.json`.
PreliminaryPhysics uses the fresh `ScotchGlider/validation.json` and
`LittleBoy/validation.json` with `-ScotchGeometryReceipt` and
`-LittleBoyGeometryReceipt`. MultiplayerPackage uses the explicit desktop
`-PackagePath` above and `-Version 2.0.9.5`.

## Coverage and performance limits

This follow-up combines the previous whole-version review with inspection of
the fixes and new-layout changes, current source/binary identity checks and the
fresh regressions above. No further multiplayer policy/authority defect was
identified in this pass. Managed transport/Unity substitutes do not establish
live host/guest, reconnect, audio or visual correctness.

The structural fixture validates generated cases and acceptance rules using
synthetic reports; it does not physically assemble all 96 configurations.
Pacific/Hudson still exercises their shared gear method, and ID48's motion
coverage uses the shared driver rig. Preliminary physics retains 2.0.9.3 captured
radii for unchanged IDs0–48 and source-hash-checked offline radii for IDs49/50.
Refresh affected radii after geometry changes or when James supplies a new capture.

Prior source-guarded Scotch rod/linkage and reused ID37 Little Boy linkage
evidence is retained with the limits in CURRENT_HANDOVER.md; these long sweeps
were not rerun here. Scotch brake/shoe seating and full solid clearances, and
Little Boy's complete moving/articulating assembly, still require gameplay
inspection. The historical ArticulatedIntegration script with a missing old DLL
and 47-ID expectations is not counted as passing current coverage.

The previously identified performance costs remain: per-frame retarget mesh
uploads/normals/tangents, per-force-call arrays, and native ECS job completion.
Connecting-rod remeshing reports zero allocations for its measured pure helper;
that does not measure Unity mesh upload or whole-frame cost. No profiler run was
performed and no frame-rate guarantee is claimed. These are retained profiling
targets, not additional demonstrated beta blockers. Avoid speculative redesign
before a measured runtime problem.

## James's final beta checks after repair

1. Select Scotch and Little Boy, compare with Pacific and 2-6-6-2, switch between
   them and stock; save/reload. Inspect rods, brakes, cylinders and trucks at
   both LODs, in motion, reverse and on curves. Check both Little Boy engines.
2. Switch/reapply a retargeted layout, return to stock, then explosion/repair;
   also repeat after a prior explosion/repair. Include the Franklin regression.
3. With identical repaired packages, test host/guest requests, late join and
   save/reconnect. Join an already-exploded rearranged locomotive, then repair it.
   Test stock tender preference changed without reapply; peers should keep the
   actually applied ordinary tender until a deliberate reapply.
4. Check Scotch with ordinary and optional powered tender; Little Boy must retain
   its ordinary tender. Watch/listen for peer effects and driving agreement.
5. If using the existing manual structural sweep, record its build and result
   for all 51 ordinary and 45 supported powered cases. Treat that as structural
   evidence separately from visual/audio/driving acceptance; do not expand it
   into an unbounded all-state gameplay audit.

James owns gameplay. This audit launched no game, installed nothing, rebuilt no
production DLL, and published nothing. Production and packaged binaries remain
the candidate hashes above, with the reported runtime defect still present.
