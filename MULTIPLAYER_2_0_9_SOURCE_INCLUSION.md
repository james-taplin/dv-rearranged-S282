# 2.0.9-beta source inclusion and independent-drive validation

29 October 2026. Selected base: `31389e811d0b99fb68b238174fd59b9264f389ef`, the source checkpoint for the explicitly selected **2.0.8-slip-test**. Target: `codex/s282-multiplayer`, version **2.0.9-beta**. This report supersedes the pre-rebuild 2.0.8-alpha inclusion result; it does not imply live multiplayer acceptance.

## Complete selected-base source check

All **112/112 selected-base runtime/project/resource files exist** in the rebuilt worktree. **96 are text-identical after CRLF-to-LF and optional UTF-8 BOM normalization; 16 are adapted for multiplayer; zero are missing.** Exact original/current SHA-256 values for all 112 paths and source diffs for the 16 adapted paths are recorded locally in `build/audit-20261029/fix-inclusion/FINAL_SOURCE_INCLUSION.json` and the adjacent `.diff` files. The receipt SHA256 is `75CD891667F88FA298C996BE0553B591943CAB24029AD1AA52711BE8F342844C`. Two runtime files extend the selected base: `MultiplayerSync.cs` and `IndependentDriveReplication.cs`.

The chosen source also includes the latest normal `9b01627`/Franklin-linkage content. This is verified by content, rather than claiming Git ancestry: of the normal checkpoint's 104 C#/project files, 99 are unchanged in the selected base. Its five adaptations are limited to independent-drive activation/cleanup, preventing competing animation speed writes, and diagnostics in `Main.cs`, `TenderDriveAnimation.cs`, `TractiveDiagnostics.cs`, `WheelRearranger.Conventional.cs` and `WheelRearranger.Mallet.cs`. Seven independent-drive C# files and the existing resource file account for the complete 112-file selected-base manifest.

The original normal-vs-live-checkout audit separately verified all 104 normal tracked C#/project files against the actual canonical normal checkout. Its 20-row historical matrix remains useful provenance, but its “missing from alpha” column describes the superseded alpha and must not be presented as the rebuilt package's current result.

## Adapted selected-base paths

| Path under RearrangedS282 | Multiplayer adaptation checked against selected base |
|---|---|
| ArticulatedSimulation.cs | Append supported native configurable state definitions before initialization; authority gates on auxiliary phase/output/repair/tick writes; original native engine calibration and graph remain. |
| B99PhysicsState.cs | Accept a remotely mirrored powered-axle port only with the intact four-driver physical stock baseline; retain exact physical baseline checks. |
| BigEndOiling.cs | One authority gate before native oil filling/lid assistance; exact factory/component/array ownership and six/20-to-24 migration retained. |
| CommsRadioS282Rearranger.cs | Per-radio controller reference; route arrangement/reapply to host; idempotent local mode registration. |
| ConventionalSpawn.cs | Defer guest automatic/local save choices; forget pooled/deleted state; ordinary native livery scope retained. |
| GroupCrankAnimation.cs | Accept explicit host phase offsets while retaining native AnimatorPortReader, driver leases and balanced cleanup. |
| GroupSlipEffects.cs | Native spark ignition is authority-only; guest particles/audio read host group slip/RPM. |
| IndependentDrive.cs | Host/offline creates native ECS state and owns engine RPM/forces; guest reads native state outputs without ECS/input/force/RPM-reference writes. Reconcile fresh host baselines and release stale revisions. |
| Main.cs | Start/tick optional MP core; display session/host controls; disable guest profile edits and in-session grip diagnostics; retained original independent-slip setup. |
| RearrangedS282.csproj | Version 2.0.9-beta. |
| WheelRearranger.BigEndOiling.cs | Read host per-car arrangement profile; all latest bearing allocation/cup logic retained. |
| WheelRearranger.Conventional.cs | Host settings, remote operational-check handling, safe failure publication, deletion cleanup; all selected-base geometry/quartering/independent activation retained. |
| WheelRearranger.Curving.cs | Host per-car options and authority-only native extra drag; Bogie.ApplyForce retained. |
| WheelRearranger.Stage2.cs | BeforeSwitch/OnApplied/failure lifecycle around the selected-base converter. |
| WheelRearranger.TenderEngine.cs | Authority-only tender force; native force helper and selected-base teardown retained. |
| WheelRearranger.cs | Resolve arrangement options from host per-car profile throughout original presentation paths. |

## Historical fix coverage

| Prior fix/feature | Rebuilt source result |
|---|---|
| Exact owned oil factory, tick and component save loading | Included; guest writes blocked before assistance. |
| Ordinary native S282 and pre-Awake unique-spawn exclusion | Included unchanged; MP core also uses normal-livery identity. |
| Native bogie force helper for tender/curve drag | Included; host authority required. |
| Native left/right quartering and stock/tender phase restoration | Included; explicit host-offset reader added for independent groups. |
| Express ID48, 49 arrangements and 44 powered-tender-compatible layouts | Included; selected-base catalogue/settings unchanged. |
| Compact-triplex crank-pin restoration | Included within latest common Franklin renderer setup. |
| Express cup seating and native lubricator linkage | Included; geometry/helpers unchanged. |
| Erie frame/cradle and coupled underframe feed | Included unchanged. |
| General cup seating, mirrored/Franklin/seven-axle cases | Included unchanged. |
| Three-driver Mallet retained native longitudinal pitch | Included unchanged. |
| Triplex authored main-bearing Oiling2 fallback and main-rod ownership | Included unchanged. |
| Prairie pipe weld tolerance and adaptive clearance | Included unchanged. |
| Exporter root/working-directory/completeness diagnostics | Included unchanged. |
| Default-off owned automatic oiling and native lids/doors/partial cleanup | Included; authority gate added. |
| Tender body native LOD registration | Included unchanged. |
| 24-point capacity and recognized six/20-point migration | Included unchanged; no truncation to old 20-point schema. |
| Tender effect teardown without illegal activation-time reparent | Included unchanged. |
| Latest Franklin common/single renderers, rear nose and outlet reseat | Included unchanged. |
| Latest structural coverage for cups, quartering, pitch, LOD and Franklin | Included unchanged. |
| Explicitly selected independent locomotive-group wheelslip trial | Included; MP authority and native port replication integrated. Existing trial limitations remain below. |

## Native replication decision and regression evidence

The installed native `ConfigurablePort` creates its configured port and has an empty Tick; native Multiplayer already replicates registered STATE ports. The solution adds no per-frame custom RPC, custom clock or second guest physics simulation. The graph receives **38 native read-only STATE outputs** before initialization: seven fields per maximum three groups, group count, four 16-bit words each for revision/session epoch, and eight exact GUID words. Every word fits exactly in a float.

The GUID/epoch marker is justified by native lifecycle evidence: `TrainCar.AwakeForPooledCar`/`ReturnCarToPool` do not reconstruct the simulation flow; `ConfigurablePort` inherits no-op save restoration; MP pooled SpawnCar reuses that flow. A different pooled car can therefore retain another car's active revision-1 values. Matching exact GUID, epoch, revision and group count prevents those values from activating the new replica. Both different-GUID reuse and same-GUID/new-session rejection are covered by the production helper fixture.

A managed regression also demonstrated that an active replica ignored a fresh host phase baseline. It failed before the reconciliation change and passes afterward. The owner now rebinds native readers only when offsets actually change; unchanged offsets preserve leases. Same-frame reader release/rebind and host offsets are exercised separately in real Unity.

## Validation completed for this integration

| Validation | Result and boundary |
|---|---|
| Production native port adapter | **32 passed**: ordered native definitions, 38 stable IDs, exact 64-bit revisions/epochs, GUID identity, stale/malformed/inactive rejection, guest no-write calls. Narrow native API substitutes. |
| Entire production IndependentDrive + force routing | **24 passed**: guest zero ECS construction/input/force/RPM writes; exact host offsets; baseline reconciliation; native host ownership; authority-loss cleanup. Narrow Unity/ECS/animation substitutes. |
| Entire production articulated graph/lifecycle | **56 passed**: all 38 outputs registered before initialization; original graph preserved, repeated initialization, save migration, calibration and rollback. Narrow graph API substitutes. |
| Real installed native auxiliary engine | **72 passed**: native Tick/ports/steam/wear/repair/tender behavior, plus guest no-Tick/no-zero/no-repair output writes. Actual native engine implementation outside Unity. |
| Real Unity native ECS/wrapper fixture | **46 passed** in Unity 2019.4.40f1 non-development player: installed native adhesion/slip calculations, automatic wrapper ordering, repeated retirement, no owned entity leaks. No vehicle forces. |
| Real Unity native AnimatorPortReader fixture | **13 passed**: normal evaluation/actual wheel pose, per-group phase, quartering, cleanup, same-frame rebind and late-join host baseline. Synthetic controller/wheel; no real S282 meshes. |

The sandboxed Editor initially could not read a valid machine/license identity. The same isolated fixture passed outside the sandbox with clean editor/player exits. No license files/settings, live game, game saves or installed mod were changed. The final marker expansion changes the native registration/identity adapter only; the real native phase/ECS fixture sources are unchanged from the successful run.

Private repeatable receipts: `build/audit-20261029/independent-replication-identity-final`, `articulated-identity-final`, and `native-slip-world-unsandboxed/20261029-210735-072`.

## Remaining acceptance limits

No two-peer live Multiplayer test ran. Native fixture success is not proof of runtime car pooling, reconnects, late-join delivery order, moving S282/Franklin visual appearance, full game force/resource behavior or a game save lifecycle. Testers still need the same complete package on every peer.

The chosen experimental source retains its declared scope: locomotive groups have independent native slip; powered tender adhesion/RPM remains the selected build's shared/manual-clamp implementation, global/cab slip flags and extra curve drag remain pooled, and derailment retires the independent owner until safe reapplication after rerailing. This integration does not silently expand that experimental feature or claim those limitations were resolved.
