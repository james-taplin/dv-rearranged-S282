## 5 November 2026 — final package record and GitHub source push

James requested pushing the 2.0.9.8 final-beta source and updated docs to his
GitHub fork, and specified no BBCode. Source target is remote `james`,
`https://github.com/james-taplin/dv-rearranged-S282.git`, branch
`codex/s282-multiplayer`. This source publication does not upload a Nexus file
or create a GitHub binary release.

The source/docs push completed successfully through commit
`ed094c293e45c175b91cd7246429127b29cc0632`, advancing the remote branch from
163d081. This handover confirmation is committed and pushed afterwards. No force
push, upstream/origin update, GitHub binary release or Nexus upload was performed.

Authoritative final Nexus upload ZIP:
`C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-2.0.9.8-beta-Nexus\RearrangedS282-B99-2.0.9.8-beta.zip`.
Repository artifact copy: `release/2.0.9.8-beta/RearrangedS282-B99-2.0.9.8-beta.zip`.
SHA-256: `4D6286CEE5FD7A72EA8D607AC7F233D9ACD136084407B54999C3009DE570664C`.
38 files, 80 package checks passed; tested/installed DLLs and native cups remain
unchanged. Do not substitute the older Multiplayer-Test ZIP for this final copy.

Current release text uses **plain text**, with Markdown for repository guides.
`release-copy/2.0.9.8-beta/NEXUS_DESCRIPTION.txt` contains the complete description,
full changelog since 2.0.9.3-beta and credits. DESCRIPTION.txt is description only;
CHANGELOG.txt is the complete standalone changelog. Updated NEXUS_RELEASE_COPY.txt
points to the plain-text files. Removed this release's three BBCode companion
files from source and both artifact folders; historical release directories are
unchanged. The upload ZIP was already free of BBCode, so no repack/rebuild was
needed and its hash is unchanged. The packager now exports the plain complete
description when present. Earlier BBCode preparation notes below are historical.

Source checkpoint defddeb and the following handover/package record remain in
the branch history; this push includes all later plain-text documentation changes.
Original build provenance and verified-source checkpoint are still distinguished
in the release manifest/receipts. Runtime source inventory still matches the
tested 2.0.9.8 build. Structural acceptance remains 96/96 plus six return cases;
remaining visual/driving and live multiplayer feedback is for the final beta.

## 5 November 2026 — 2.0.9.8 Nexus final beta READY, not uploaded

Completed James's requested final-beta packaging, full changelog since
2.0.9.3-beta and release docs. Native cups are retained as James confirmed.
Upload this archive (not the earlier Multiplayer-Test ZIP):
`C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-2.0.9.8-beta-Nexus\RearrangedS282-B99-2.0.9.8-beta.zip`.
Repository copy: `release/2.0.9.8-beta/RearrangedS282-B99-2.0.9.8-beta.zip`.
ZIP SHA-256: `4D6286CEE5FD7A72EA8D607AC7F233D9ACD136084407B54999C3009DE570664C`.
Runtime/manifest version remains 2.0.9.8; archive/file copy labels it final beta
before 2.1 stable. The installed mod is unchanged.

Package validator: **80 checks passed**. Archive has 38 files. A complete entry
comparison against the tested 2.0.9.8 ZIP confirms **33 byte-identical entries**;
only README, CHANGELOG, CREDITS, MULTIPLAYER_TESTING and UMM_OPTIONS changed.
Core remains `F5AF66BC78111AA35B402B23BCFDCD162E3D8A0A21AD11486B923AEF9E20C400`;
bridge remains `C1D5BB2AF7F0EC4E0AE3DCA244AB0969A123A9B535730B5D8F1A3970C99E0DA4`.
Both match the installed DLLs. Assets/exporter/licences and manifest are unchanged.
No settings, API, PDB, captures, caches or logs are bundled in the upload ZIP.
No runtime rebuild was performed for release copy.

All release text is in `release-copy/2.0.9.8-beta/` and copied beside the desktop
archive. CHANGELOG.txt covers the complete development sequence since 2.0.9.3-beta,
including restored linkages, Franklin pins/triplex clearance, both renderer
retirement fixes, multiplayer wreck/tender state, OBJ import, new layouts,
offline fixture repairs, arrangement count and structural identity/readiness.
README.txt is plain text; UMM_OPTIONS.md is current; credits/attributions retained.
NEXUS_DESCRIPTION_BBCODE.txt contains description + full changelog + credits;
DESCRIPTION_BBCODE.txt is description alone. BBCode balance and plain README
format were checked. FILE_INFO.txt and NEXUS_RELEASE_COPY.txt provide upload copy.

Clean local source checkpoint: `defddeb22021884b1024197a81e9e63a2116832d`.
All 120 runtime source files/inventory match the original build receipt. Separate
`build/nexus-2.0.9.8-verified-source/build-receipt.json` explicitly records
receiptKind=verified-source-repackage, the later clean checkpoint and the
original build's 47e8ac7/dirty provenance; original build receipt is preserved.
It does not claim that the original build was made from a clean tree. The normal
packager's clean-source checks passed against this verified checkpoint receipt.
Release manifest carries both provenance records. CHECKSUMS.txt, receipts,
package-validation.json (80 checks) and package-identity-check.json are beside the
desktop ZIP. Evidence is also in `build/nexus-2.0.9.8-validation/` and
`build/nexus-2.0.9.8-identity.json`.

Gameplay evidence remains James's **96/96 stationary structural cases + six
return checks**, with stock/preferences restored. The successful session had
Multiplayer absent; moving clearances, sustained driving/performance and live
host/guest acceptance are not certified by that result. Release wording retains
these beta limits and does not promise error-free/perfect performance.
No game launch, installation, push or upload during preparation. James owns
Nexus uploading and final beta feedback before 2.1 stable.

## 5 November 2026 — Nexus final beta preparation authorized

James confirmed retaining the native cups and requested a Nexus-ready 2.0.9.8
final beta ZIP, current release notes/docs and the complete changelog since
2.0.9.3-beta. Player copy is under `release-copy/2.0.9.8-beta/`, including plain
README/changelog, options, preserved credits, file information and full Nexus
BBCode description with changelog and credits. Runtime version stays 2.0.9.8;
release archive naming is 2.0.9.8-beta to distinguish the player-facing final beta
from the earlier Multiplayer-Test ZIP. No native cups or gameplay code changed.

Preserve the installed/tested core and bridge byte-for-byte; do not rebuild for
these documentation changes. The existing build was made from checkpoint
47e8ac7 plus uncommitted changes. For release packaging, save a clean local source
checkpoint, verify its entire runtime source inventory against the original
build receipt, and create a separate verified-source-repackage receipt carrying
both the new checkpoint and original dirty-build provenance. The original build
receipt remains untouched. Package.Nexus.ps1 retains its clean-source checks and
now carries those provenance fields into the release manifest. No push/upload
is authorized or performed by this preparation task.

The successful stationary structural run is 96/96 plus all six return cases,
with stock/preferences restored. Current gameplay/clearance and real host/guest
acceptance remain beta feedback; Multiplayer was absent in that run. Full
changelog distinguishes new changes from retained 2.0.9.3-beta tender/Mallet fixes
and does not claim universal clearance, zero errors or profiled performance.

## 5 November 2026 — 2.0.9.8 live structural sweep PASSED; guide cup reviewed

James completed the installed 2.0.9.8 sweep: **96/96 main cases and all six
return-sequence cases passed**, with finalStockRestored=true and
tenderPreferencesUnchanged=true. Subject L-033, control L-093. The report timestamp
is 22:39 and the matching log confirms all 96 stationary cases passed. Preserved
report, log, summary and supplied screenshot under
`build/structural-results-2.0.9.8-20261105/`. This supersedes the earlier failed
readiness run as current structural evidence. Motion/clearance remain explicitly
unverified by the test, and Multiplayer was absent in this session.

James reported too many/front misplaced cups on Little Boy and other layouts.
He clarified: **preserve native cups; fix only extra/misplaced cups**, then
identified the cup on top of the crosshead guide as the suspect. Native baseline
and current binding source distinguish three native providers per side:
Oiling0 on the fixed guide, Oiling1 on the moving crosshead, Oiling2 on the main
rod big end. See native provider paths in
`evidence/pacific-hudson-valve-gear/native-animation.json`, B99Oilcups.cs and
WheelRearranger.OilCupMounts.cs; this agrees with prior on-disk native inspection.
The original guide cup is retained, not a newly added crosshead cup. Do not delete
it as a duplicate or remove legitimate crank-pin coverage to reduce the count.

Little Boy and Mallet both report 16 active cups: 12 crank-pin cups (two native,
ten added), plus the four native guide/crosshead cups. Prairie and Scotch each
report ten (six crank-pin plus those four); stock retains six native cups. Little
Boy has no duplicated refill ports, no mount pairs within the diagnostic's 10 mm
overlap threshold, complete coverage, and micron-scale provider/seat gaps.
These counts and checks are against the current allocation plan; they do not
independently establish that every visible cup is physically correct. The saved
report does not include per-cup positions/paths for exact screenshot matching.
No new duplicate or misplaced added cup was demonstrated by this review. A cup
visibly floating off its legitimate guide would still warrant a targeted mount
correction; the user's reported visual concern must not be dismissed merely
because the structural test passes.

No S282 exception stack or failed cup-creation message found in the preserved
log. It records optional Multiplayer absent and later native shop/Bolt and
DoubleTrack shutdown exceptions; those inspected stacks do not identify an S282
failure. No performance or multiplayer acceptance inferred. No runtime edits,
build, installation or game control was performed during this results review.
Installed version/package remain 2.0.9.8. Visual/driving and live multiplayer
checks remain the next acceptance work before Nexus or 2.1 stable.

## 5 November 2026 — 2.0.9.8 readiness repair built and installed

At James's request, fixed structural test readiness, reran affected offline
checks, built a fresh beta ZIP and installed it with a complete verified backup
and settings preserved. Installed version is now **2.0.9.8**. All earlier runtime
fixes and both new arrangements are retained; the behavioral change from 2.0.9.7
is confined to the structural diagnostic. No geometry, simulation or network
policy changed. No DV launch or Nexus publication was performed.

Begin now selects subject and control only from eligible stationary stock S282s
whose native loadedExternalInteractables is present. It refuses before starting
if two suitable loaded cars (including the subject's regular tender) are not
available, reports loadedEligibleCars, and identifies both selected IDs in status.
Baseline and both sweep loops check readiness before conversion and after waits.
If either car unloads, progress is preserved, the sweep stops explicitly with
aborted=true / abortReason=exterior-interactions-unloaded, remaining return cases
are skipped and the final stock-restoration attempt remains. Aborted runs cannot
pass. Strict oilcup assertions and the existing 96-case matrix are unchanged.

New Validate.StructuralReadiness.ps1 executes extracted production Begin,
HasTestTender, StructuralReadinessError and AbortForReadiness using narrow managed
car/coroutine boundaries: **21 checks pass**. Covers the lower-ID unloaded subject
regression, missing loaded subject/control or regular tender, concurrent start,
existing moving/wreck/powered-control exclusions, selected IDs and mid-run
subject/control unavailability with retained progress. Source routing guards
check both loops, baseline protection, skip-on-abort and final restoration order.
It does not execute the complete Unity sweep coroutine or native loading system.
An initial fixture-only name-resolution compile error was corrected; final run
passes. StructuralMatrix passes **2,529 assertions / 96 cases**; all **seven
RendererLifetime modes** pass. Logs/receipts: `build/readiness-2.0.9.8/`.
Use `Validate.<Name>.ps1 -OutputDirectory <absolute output path>`.

One integrated Release build via Build.Multiplayer.ps1 passed for core and bridge
with **zero warnings/errors**. All 109 core and two bridge PDB source documents
match current production sources; both assembly versions are 2.0.9.8. Runtime
source hashes also match the build receipt. Source provenance remains checkpoint
47e8ac7 plus current uncommitted fixes/version changes, not a new clean commit.

Final test ZIP:
`C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-2.0.9.8-Multiplayer-Test.zip`.
ZIP SHA-256: `945A2FBD87850B6E83A9044B8AFA7162A77CB822D8455998BFBFE33691E2F570`.
Core SHA-256: `F5AF66BC78111AA35B402B23BCFDCD162E3D8A0A21AD11486B923AEF9E20C400`.
Bridge SHA-256: `C1D5BB2AF7F0EC4E0AE3DCA244AB0969A123A9B535730B5D8F1A3970C99E0DA4`.
Final package uses the verified 2.0.9.7 asset/licence base, fresh DLLs/manifest and
updated plain-text tester copy. Package validator passes **80 checks**. Final
receipt: `build/readiness-2.0.9.8/test-package-receipt.json`; original build
receipt is in `candidate/` (its raw-package ZIP is an intermediate artifact).

Backed up all 100 files of the previous 2.0.9.7 installation, verified every
backup hash before installation:
`build/install-backups/2.0.9.7-before-2.0.9.8-20261105-220225/RearrangedS282/`.
Adjacent file-manifest.json records all backed-up hashes. All 38 final package
files match their installed copies. Settings.xml is byte-for-byte preserved,
SHA-256 `9645E3727F1E176709345887C609235A2CD6C7D26F2ED5B7A755FA3E33D516C2`.
Existing non-package files/captures/caches were preserved and hash-checked.
Install receipt: `build/readiness-2.0.9.8/install-receipt.json`.

James's next check: place two stationary stock S282s close together, one with a
regular tender coupled behind it; keep the control unpowered, close regulators
and stay near both until exterior interactions load. Start the structural test,
note selected IDs and close UMM so simulation ticks. Remain nearby. A readiness
refusal/abort now explains what to load before rerunning. A valid full run still
requires 96 main cases plus six return cases. The last actual game result remains
the preserved 2.0.9.7 readiness failure below; no passing game result is inferred
from this repair. Visual/driving and live multiplayer acceptance remain pending
before Nexus publication or 2.1 stable.

## 5 November 2026 — 2.0.9.7 game structural result: unloaded test subject

James completed the next sweep. Preserved the report (21:37) and matching
Player.log under `build/structural-results-2.0.9.7-20261105/`, with summary.json.
**The run failed: 0/96 main cases passed.** Every case has the same oilcup
initialization error and `oilcupPresentation: { externalLoaded: false }`.
The selected subject L-033 already had external interactables unloaded in the
before snapshot; control L-093 had them loaded. Do not treat these as 96 distinct
arrangement failures or count the run as structural acceptance.

Source explains the common failure: Begin chooses eligible world cars sorted by
ID without requiring loadedExternalInteractables. CaptureOilCupPresentation
returns only externalLoaded=false when that object is absent; the validator then
rejects missing ready=true. The sweep continued through all configurations even
though the shared oilcup prerequisite never became available. Why L-033's
interactables were unloaded is not established by these files; player distance
is a possibility, not an observed fact.

Little Boy's ID37 frame lookup correction is confirmed by the new report:
ID50 now has two front and two rear pieces and two active engines. All six
return-sequence cases passed; final stock restored and tender preferences
unchanged. Those returns do not establish oilcup acceptance.

The log contains AI Traffic startup registration errors, repeated DVSignals
reservation warnings and later native/DoubleTrack shutdown exceptions. Inspected
exception stacks do not identify an S282 runtime exception; the S282 log records
the failed structural result. No profiler/performance or live multiplayer
conclusion follows from this check.

Next correction should remain in diagnostic selection/readiness: select an
eligible subject with loaded exterior interactables and reject an unavailable
prerequisite before a full sweep; handle unloading explicitly. Retain strict
oilcup assertions, then James reruns with exterior interactions available. No
production code, package or installation was changed during this results review.
Nexus/stable acceptance remains pending a valid structural run and James's
remaining gameplay/multiplayer tests.

## 5 November 2026 — 2.0.9.7 installed for James

At James's request, installed the verified 2.0.9.7 desktop test ZIP. All 38
package files match the installed copies, including the core/bridge hashes below.
The complete previous 2.0.9.6 installation (100 files) was backed up and every
backup hash verified before installation:
`build/install-backups/2.0.9.6-before-2.0.9.7-20261105-210229/RearrangedS282/`.
The adjacent file-manifest.json records all backed-up file hashes.

Current Settings.xml was preserved byte-for-byte, SHA-256
`9645E3727F1E176709345887C609235A2CD6C7D26F2ED5B7A755FA3E33D516C2`.
Every existing file outside the package, including captures/caches, was preserved
and hash-checked. Receipt:
`build/structural-failure-20261105/install-2.0.9.7-receipt.json`.
No game launch or publication. James should rerun the structural sweep; current
game evidence remains the 2.0.9.6 95/96 result until he supplies the new run.
Earlier installed-version statements below are historical.

## 5 November 2026 — Little Boy structural diagnostic corrected in 2.0.9.7

James reported the structural sweep failed. Preserved his 2.0.9.6 report and
Player.log under `build/structural-failure-20261105/` before changing anything.
Actual game result: **95/96 main cases passed**, all six return-to-Erie sequence
cases passed, final stock restored and tender preferences unchanged. Scotch
passed both tender modes. Only ID50 Little Boy failed, with zero counted front/
rear frame pieces; its selected ID, two engines, independent-drive readiness,
animation ownership and all four driver-pitch entries were recorded correctly.

Cause: Little Boy calls the unchanged ApplyMalletGeometry, which configures and
caches frames under ID37. CapturePresentation looked for frame names containing
selected ID50. This is a shared-geometry tag lookup mismatch; the source and
successful ID37 cases explain the missing diagnostic counts. No change to layout
geometry or simulation was warranted. Added FramePiecePrefix to map only ID50's
frame tag to ID37. Selected arrangement validation still requires ID50, and
active/nonempty meshes and the required frame-piece counts remain enforced.

The earlier offline matrix used synthetic presentation rows and missed this
capture lookup. Its fixture now extracts the production prefix helper and guards
its use by the real capture method. Checks cover all 51 IDs/three labels and the
specific Little Boy shared cache. StructuralMatrix passes 2,529 assertions across
96 cases, and LittleBoy's native wrapper/spacing fixture passes. These offline
checks do not convert the failed game result into a live pass; James must rerun.

One integrated 2.0.9.7 Release core/bridge build passed with zero warnings/errors.
Runtime source hashes match the build receipt; the only behavioral change since
2.0.9.6 is the diagnostic lookup. Version identifiers changed to distinguish the
new build. Final test package uses the verified 2.0.9.6 asset/licence base and
fresh DLLs, manifest and tester documents:
`C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-2.0.9.7-Multiplayer-Test.zip`.
ZIP SHA-256: `73E6B759BFCBAC50D4CC4AA9C593D6B815758E5B8F9C59F239869D6447DEE69F`.
Core: `F56CD293B0FA330198D25F303FA2ADFC1A5DAB6DE16B75BC06AFEE12BE97BDF3`.
Bridge: `54FF831187E93F3BAF59F61DDFAF3C34462482F6314224028491759BA3FC8BFC`.
Final package receipt: `build/structural-failure-20261105/test-package-2.0.9.7-receipt.json`.
Validate.MultiplayerPackage.ps1 passed all 80 checks against this desktop ZIP;
receipt/logs are in `build/structural-failure-20261105/PackageValidation/` and
`PackageValidation.log`.
Source is checkpoint 47e8ac7 plus current uncommitted changes. No clean commit
or gameplay acceptance is claimed. Original 2.0.9.6 failure evidence is preserved.

Installed version remains 2.0.9.6; no game launch, installation or publication
was performed during this investigation. Install the corrected candidate when
authorized, then James reruns the structural sweep (expect 96/96 plus six return
cases). Visual/motion/clearance and live multiplayer acceptance remain separate.

## 4 November 2026 — 2.0.9.6 installed for James

At James's request, installed the verified 2.0.9.6 test ZIP into the existing
RearrangedS282 mod folder. All 38 packaged files match their installed copies,
including core/bridge hashes recorded below. A complete verified 78-file backup
of the previous 2.0.9.5 installation is under
`build/install-backups/2.0.9.5-before-2.0.9.6-20261104-235503/RearrangedS282/`;
the adjacent file-manifest.json records every backed-up file hash.

Settings.xml is byte-for-byte preserved (SHA-256
`930A5B0B02EE50DD28DCF0E7A74832F2C1071E0C7A9DC42654783D48D8D678F4`).
Existing files outside the package, including captures/caches, were preserved
and hash-checked. Install receipt: `build/repair-2.0.9.6/install-receipt.json`.
No game launch or publication; gameplay acceptance awaits James. Earlier
statements that 2.0.9.5 remains installed are historical.

## 4 November 2026 — 2.0.9.6 renderer repair beta ready for James

At James's request, repaired the two 2.0.9.5 audit findings and built a new
multiplayer beta ZIP for his testing:
`C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-2.0.9.6-Multiplayer-Test.zip`.
Final ZIP SHA-256:
`598DEA11E65E04E931A9B01A4D8D17610E0D62A5B99960AA624F46E91DC1533D`.
This supersedes the defective 2.0.9.5 test ZIP. James's gameplay and live
multiplayer acceptance remain pending before Nexus publication or 2.1 stable.

Each DriveRetarget owner now receives the existing explosion-cache retirement
callback before its proxies are registered. Its actual Restore body invokes
retirement before deferred proxy destruction. B99ExplosionMaterials removes
them from all native material-swap caches and excludes them from same-frame
include-inactive refresh. This covers already-registered owners retired through
stock restoration, reapply, hidden linkage/rod/articulated suspension and pool
return. The options header now derives its count from SupportedCount (51).
No geometry, catalogue identities, driving physics or networking policy changed.

The existing RendererLifetime fixture now also extracts actual retarget Restore.
All seven modes pass: three Franklin modes plus retarget fresh cache, populated
cache, pool return and reapply. The retarget modes check one retirement before
deferred destruction, same-frame inactive refresh, successful later native
explosion/repair and replacement-renderer burn/repair. Stock and pool use the
extracted production outer routes with a narrow retarget suspension adapter;
the full Unity switch transaction remains James's gameplay check.

Fresh affected checks under `build/repair-2.0.9.6/`: ConnectingRods 905,299;
FourteenSpacing 1,452; PrairieMotion 92,237; PacificHudsonValveGear 15,717;
RestoredFeatures 286 assertions/28 transitions/11 explosions; multiplayer
bridge/core 219/170; IndependentMultiplayer 32 native-port/27 authority checks;
StructuralMatrix 2,374 assertions/96 cases; RadioSettings 468; ScotchDispatch 22.
RestoredFeatures requires an absolute OutputDirectory; the initial relative-path
invocation failed fixture compilation and the absolute-path rerun passed.
Commands otherwise use `Validate.<Name>.ps1 -OutputDirectory <absolute output>`.
Older unchanged-layout, physics and native geometry evidence remains applicable
with the coverage limits in the 2.0.9.5 audit; no new gameplay acceptance inferred.

One integrated Release build via Build.Multiplayer.ps1 succeeded for core and
bridge with zero warnings/errors. Outputs and original build receipt:
`build/repair-2.0.9.6/candidate/`. Final package receipt:
`build/repair-2.0.9.6/test-package-receipt.json` (use this receipt's desktop ZIP,
not the intermediate raw-package ZIP). Core SHA-256:
`8AB70048875D5FFFC27704F40FFE8D95AD74ED373C30E68F1D43CB86A2D03E11`;
bridge SHA-256:
`BFB18490FCACB59752D52F7ED6942B7AD8C8B660E10763FAA31C2C46D0F75940`.
Both assembly versions are 2.0.9.6. All 109 core and two bridge PDB source
documents match current production sources; compiled options IL uses 51.
Source provenance is checkpoint 47e8ac7 plus the current uncommitted repair and
version changes, not a new clean source commit. Earlier local doc edits preserved.

Final ZIP uses the verified complete 2.0.9.5 asset/licence base with fresh DLLs,
2.0.9.6 manifest and current plain-text tester README/checklist/changelog.
Validate.MultiplayerPackage.ps1 passed 80 checks against the desktop ZIP: safe
UMM root, expected identities/dependencies, preserved assets/licences, optional
Multiplayer and no bundled API/personal settings/logs. Package version distinguishes
the repair from 2.0.9.5; all peers need the exact same new package.

James should install the new ZIP, repeat retargeted-layout -> stock ->
explosion/repair both before and after an earlier explosion/repair, then check
Scotch/Little Boy motion, brakes/trucks/rods/curves/LODs and save/reload. Repeat
host/guest selection, wreck late join/repair and stock tender preference without
reapply; Scotch optional powered tender and Little Boy ordinary tender. The full
checklist is MULTIPLAYER_TESTING.txt. The existing manual structural sweep has
51 ordinary +45 powered cases if James chooses to run it. No DV launch,
installation or publication was performed; the last installed DLLs remain 2.0.9.5.

## 4 November 2026 — pre-publication audit: repair required (2.0.9.5 historical)

Audited source `47e8ac7`, integrated/installed 2.0.9.5 DLLs and desktop test ZIP.
**Hold Nexus publication:** `DriveRetarget.Restore` destroys proxy renderers
without retiring them from explosion material lists. Switching/reapplying a
retargeted layout before a later explosion can throw on a destroyed renderer.
Reproduced offline with the actual production cleanup and native B99 explosion
handler, both before and after native cache population. The earlier Franklin
retirement fix passes but does not cover this owner. Repair using the existing
`B99ExplosionMaterials.Forget` lifecycle before deferred destruction; test both
cache states and same-frame refresh. Also correct the stale 49-arrangement UI
header during that integrated repair build. No production fix was made here.

See [2.0.9.5 audit](VERSION_AUDIT_2_0_9_5.md) for exact evidence, hashes, commands,
coverage limits and James's final beta checks. Build/install/archive DLLs match;
all 111 PDB source documents match current runtime sources. Package validation
passed 80 checks. Multiplayer, ownership, new-layout, all eight repaired fixture
families, articulated and oiling checks passed freshly. StructuralMatrix initially
failed stale 49-ID counts; its fixture is now corrected and passes 2,374 assertions
across 51 ordinary and 45 supported powered-tender cases. No runtime rebuild was
needed for that test-only change. Receipts: `build/audit-2.0.9.5/`.

Updated current README/checklist/TODO status, preserving earlier local edits and
historical records. No game launch, install, production rebuild or publication.
The installed/test ZIP candidate still contains the reported defect. After its
repair and bounded offline/package checks, James performs final gameplay and
live multiplayer acceptance before Nexus or 2.1 stable.

## 4 November 2026 — 2.0.9.5 multiplayer test ZIP

At James's request, packaged the previously built 2.0.9.5 candidate for UMM
multiplayer testing at `C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-2.0.9.5-Multiplayer-Test.zip`.
SHA-256 `3B966115536A471215066930002963CA9A9AD0B13D4943DDB8D7301693AFF2E8`.
The 38-file archive uses the complete 2.0.9.3-beta package as its asset/licence
base with the verified 2.0.9.5 DLLs, manifest and updated tester instructions.
Archive integrity, UMM root layout, required assets, optional Multiplayer
manifest and candidate DLL hashes were checked. It contains no user settings,
game captures or MultiplayerAPI.dll. No build, game launch or publication;
gameplay and multiplayer acceptance remain outstanding.

## 4 November 2026 — 2.0.9.5 installed for James

At James's request, installed the 2.0.9.5 candidate core, optional bridge and
manifest into the existing RearrangedS282 mod folder. Both installed DLL hashes
match the candidate hashes below; Settings.xml and the remaining mod files were
preserved. Previous 2.0.9.3 binaries and manifest are backed up under
`build/install-backups/2.0.9.3-before-2.0.9.5-20261104-231458/`.
No game launch, package, push or publication. The new layouts still await
James's gameplay testing; older installation statements below are historical.

## 4 November 2026 — 2.0.9.5 candidate: Scotch Glider and Little Boy

James confirmed acceptance of the post-fix baseline (individual prior gameplay
results were not supplied) and chose a large-driver express Pacific and a
two-engine Little Boy with an ordinary unpowered tender. Both arrangements are
implemented as appended IDs: **49** `4-6-2 Scotch Glider` and **50** `4-6-6-4
Little Boy`; IDs 0–48 retain their values. Both new random-spawn options default
off. Scotch can use the normal optional powered tender; Little Boy cannot.

One integrated Release build succeeded with zero warnings/errors:
`build/candidate-2.0.9.5/integrated/` contains the core and optional bridge,
version 2.0.9.5. Source/test checkpoint `47e8ac7` contains the candidate;
the separately pre-existing local documentation edits remain uncommitted.
Core SHA-256
`CEFA5185C91C084ABA5574573F46CFA4ACC1DC506DD1B9660727D0CC83F330D1`;
bridge SHA-256
`2E59C916427C64AF908689FEC5F08F2FBC5293AB97F02BC1F834663A5A286F86`.
These are build outputs, not an installable package or a gameplay acceptance.

Post-change checks: radio/catalogue 468; direct source comparison against the
previous committed catalogue 399, including existing tender policies
(`pwsh -File .\Validate.CatalogueContinuity.ps1`);
save/spawn ownership 75; random spawn defaults, opt-in and settings serialization
1,514; multiplayer
bridge/core 219/170; native-engine physics 9,602 checks, 3,400 ticks; compiled
physics wiring 11; Express Mountain dispatch 28. Repaired rod, driver,
remeshing, additional-family, Pacific/Hudson, Prairie, stock restoration,
articulated simulation and independent multiplayer checks passed; logs and
receipts are under `build/candidate-integration-20261104/`,
`build/candidate-affected-20261104/` and `build/candidate-2.0.9.5/`. Relevant
pre-change evidence is under `build/preimplementation-baselines-20261104/`.
The old ArticulatedIntegration script could not run: its historical comparison
DLL is missing and it assumes 47 IDs; it is not counted as a pass.
An independent compiled IL/metadata review confirms the new dispatch hooks,
ID50 rear-engine route and core/bridge API identities; its report is
`build/candidate-2.0.9.5/integrated/independent-cecil-review.md`.

Scotch: source-measured nominal radius 0.95052 m, native reservoir-LOD gap
137.43 mm and conservative sampled truck envelope at least 207.76 mm. Its
extracted dispatcher passed 22 checks with leaf stand-ins; native sampled rod
and pin checks passed 960/96 poses, with maximum 5.84 mm absolute legacy-related
bore/pin mismatch and 0.0132 mm added normalized error versus Pacific. Hidden
linkage replay passed 188,836 checks. Receipts: `build/scotch-glider-20261104/final/`,
`build/scotch-dispatch-20261104/run1/`, `build/scotch-rods-20261104/final/`,
and `build/scotch-linkages-20261104/run3/`. Native brake/caliper/shoe seating,
cylinder/body/main-rod collisions, complete switch transaction and live
suspension are not established by these fixtures.

Little Boy reuses ID37's native Mallet engines and has six driven plus four
carrying axles, radius 0.534 m. Extracted actual dispatcher/wrapper and native
straight-track tyre-spacing checks passed; native engine setup and simulation
owner are stand-ins there. A source-guarded replay of the unchanged ID37 front
engine map passed 247,782 hidden-linkage checks; historical ID37 rod/cylinder
phase evidence applies by measured method reuse, not a new full ID50 sweep.
Receipts: `build/little-boy-20261104/dispatch-01/` and `linkage-03/`; shared
hidden-linkage rerun `hidden-standalone-02/`. Full TrainCar transaction, both
engines under live articulation, track/coupler clearance and gameplay remain
unverified.

Physics uses retained 2.0.9.3 gameplay-captured radii for unchanged IDs 0–48;
IDs 49/50 use separately recorded source-hash-checked offline geometry receipts,
not gameplay captures. Run `Validate.PreliminaryPhysics.ps1` with explicit
`-ScotchGeometryReceipt` and `-LittleBoyGeometryReceipt` paths; it rejects
missing or stale inputs. Refresh measurements when affected geometry changes.

James performs DV testing: check both new layouts and their existing Pacific
and 2-6-6-2 relatives; switch each to the other and stock; save/reload; check
host/guest selection and late join; inspect gears, pin/rod/brake/cylinder
clearance, trucks, curves and tender. The earlier Franklin-to-stock
explosion/repair, exploded guest late join/repair and stock tender reapply
remain useful regression checks. Installation and publication require separate
instructions. No game was launched, no mod installed and no package published.

## 4 November 2026 — new arrangement work in progress (historical)

James confirmed that the post-fix baseline is accepted and requested a large-driver
express 4-6-2 Scotch Glider and a 4-6-6-4 Little Boy with two powered driver
groups and a normal unpowered tender. His acceptance is recorded as stated;
individual gameplay-check results were not supplied. The previous three priority
scenarios remain useful regression checks. Installation and publication are
separate decisions, and James performs gameplay testing.

Pre-implementation offline baselines are preserved under
`build/preimplementation-baselines-20261104/`. The runnable physics, rod,
geometry, articulated, multiplayer, oiling and renderer checks passed. The
old `Validate.ArticulatedIntegration.ps1` could not run: its hardcoded historical
comparison DLL is missing, and it expects the obsolete 47-ID catalogue. Its
failure is retained, not counted as a pass. See individual receipts for scope.

At that point implementation was in progress; see the candidate status above.

## 4 November 2026 — remaining offline fixtures repaired

The eight fixtures reported failing below now pass on the existing post-fix
source. Run the commands from the repository root. They execute production
methods against captured native data and managed stand-ins; these are offline
results, not Unity rendering, driving or multiplayer gameplay acceptance.
Default outputs now go under `build/offline-validation/`; specify a fresh
`-OutputDirectory` to preserve each new run's receipt.

| Command | Observed result | Scope / when to run |
| --- | --- | --- |
| `.\Validate.ConnectingRods.ps1` | PASS 905,299 checks; 16 cases. | Fixed-pitch rod motion, oil, LOD and rollback; run after those systems change. |
| `.\Validate.DriverLayouts.ps1` | PASS 26,181 checks; 16 cases. | Fixed-pitch driver geometry, switching, native sampled poses and rollback. A wrong-pitch negative control failed as expected. Run after driver geometry changes. |
| `.\Validate.AdditionalArrangements.ps1` | PASS 1,954 checks. | Giant-wheel presentation, axle mapping and ownership; run after additional-family changes. |
| `.\Validate.FourteenSpacing.ps1` | PASS 1,452 checks; 96 poses. | 4-14-4 spacing and native sampled motion; requires historical reference commit `f31e01f59bfdd8eab179a00c03111269eb59e04f`. Run after ID 39 spacing or linkage changes. |
| `.\Validate.PacificHudsonValveGear.ps1` | PASS 15,717 checks; 48 unique poses. | Shared valve-gear method, sampled native pipe vertices and stand-in mesh channels. Does **not** test IDs 4/5 dispatch or a nonzero retarget; run after shared gear or pipe changes. |
| `.\Validate.PrairieCylinders.ps1` | PASS 369,143 checks; 1,536 poses. | Rigid casing geometry, fittings, stock bypass and 49-ID full-casing classification. ID 48's distinct geometry and articulated assembly are not exercised. Run after casing/fitting changes. |
| `.\Validate.PrairieMotion.ps1` | PASS 92,235 checks; 768 poses and 2,304 cup samples. | Prairie motion, retarget, oil and 49-ID reservoir-clearance classification, with ID 48 exercised in the shared eight-driver rig. Its bogies/full layout and articulated assembly are not covered. Run after Prairie motion or clearance changes. |
| `.\Validate.PreliminaryPhysics.ps1` | PASS 8,334 checks; 267 layout-mass cases and 2,800 native ticks. | Native engine calibration and restoration, not measured driving performance. The measured radii come from a historical **2.0.9.3-beta** capture, not 2.0.9.4 gameplay; refresh the capture when layout geometry or catalogue changes. The native definition is hash-pinned, but its original extraction provenance remains unverified. |

Results: rod receipts `build/tooling-repair-20261104/`; Additional, Fourteen
and Physics receipts `build/fixture-repair-20261104/`; Pacific and Prairie
receipts `build/tool-tidy-20261104/`. Physics rejects incomplete captures;
the relevant radius-producing source was compared between 2.0.9.3 checkpoint
`6f850b2` and post-fix HEAD. The independent family review confirmed the
Pacific/Hudson caller-dispatch and stand-in boundaries above. No production
defect was established. Historic failing receipts and classifications below
are retained as historical evidence.

No production source, DLL, installed mod or save was changed; no game was
launched. James performs gameplay acceptance on a separately authorized build.

## 4 November 2026 — offline validation tool dispositions

Tooling-only pass on post-fix HEAD `46a9a72`; the existing integrated core DLL
remains SHA-256 `40DD503FFC9409CFF9E608EDB3C8165BD8F884B7F6160B9ACB66277666F47CE2`.
No production source or DLL was rebuilt or changed. Run the commands below from
`B:\GitHub repo\rearranged-s282-multiplayer`. Passing fixture checks use native
algorithms and/or managed stand-ins as documented; they are not gameplay acceptance.

| Tool or command | Disposition / replacement | Observed result and when to run |
| --- | --- | --- |
| `.\Validate.RestoredFeatures.ps1` | Retained; updated stale multiplayer boundary. | **PASS 286**, 28 stock/traction transitions and 11 explosion cycles. Run for stock physics, brake/axle restoration, no-driver and material changes. |
| `.\Validate.RendererLifetime.ps1` | Retained and extended; complements RestoredFeatures. | **PASS 27**. Actual extracted stock restoration/pool-return methods and Franklin lease plus installed native explosion/repair methods; articulated suspension is a checked source link with a stand-in. Run for pin ownership, switching/pooling or material-cache changes. |
| `.\Validate.RodRemesher.ps1` | Retained; shared extractor now guards the rod recipe, ranges and importer dependencies. | **PASS 452,776**, 180 native topology cases. Run for rod recipe, pin/boss mapping or remeshing changes. Guard mutation checks pass; original source/importer LOD0/LOD1 topology matches installed native assets. |
| `.\Validate.ConnectingRods.ps1` | Retained; no fully equivalent replacement. | **FAIL after extraction/compile**: obsolete adjustable-pitch expectation, `0.18 vs 0.01`; later checks did not run. Repair fixture for current fixed per-family pitch/rearward values, then run for rod motion, LOD, oil and switch isolation. |
| `.\Validate.DriverLayouts.ps1` | Retained; no fully equivalent replacement. | **FAIL after extraction/compile**: obsolete adjustable-pitch expectation, `0.18 vs 0`; later checks did not run. Repair fixed-pitch/rearward expectations, then run for driver spacing, native pin/rod, LOD and stock restoration. |
| `.\Validate.BigEndOilingOwnership.ps1` | Retained and expanded; replaces MultiplayerOiling. | **PASS 655** on native oiling/save code. Guest oil/lids/refill and refill-flow stay unchanged before and after mask assignment, assistance off/on; host/migration checks pass. Run for oil ownership, masks, save migration or authority. |
| `Validate.MultiplayerOiling.ps1` | **Removed**; use `.\Validate.BigEndOilingOwnership.ps1`. | Old extracted fixture failed compilation on stale stubs; the replacement's missing-guest-gate negative control fails as expected. Historical audit references remain historical. |
| `.\Validate.AdditionalArrangements.ps1` | Retain; ExpressMountain is partial, not equivalent. | **FAIL compile**: stale `s440` enum; update stubs and obsolete adjustable giant diameter to fixed 2.8 m. Run after giant/additional wheel, axle or clone-lifetime changes. |
| `.\Validate.FourteenSpacing.ps1` | Retain; hidden-linkage checks are historical and partial. | **FAIL compile**: stale quaternion/math stubs; replace false HEAD-as-pre-0.25-m baseline with verified historical datum. Run after 4-14-4 spacing, linkage, LOD or cup changes. |
| `.\Validate.PacificHudsonValveGear.ps1` | Retain; cylinder shape/hidden-linkage checks are partial. | **FAIL compile**: missing current Dummiesman dependency. Restore importer boundary and native clip/restoration assertions; run after Pacific/Hudson gear, motion or dry-pipe changes. |
| `.\Validate.PrairieCylinders.ps1` | Retain; cylinder shape tests are partial. | **FAIL compile**: missing Dummiesman dependency. Repair importer/owner stubs and catalogue mapping; run after Prairie casings, fittings, particles or LOD changes. |
| `.\Validate.PrairieMotion.ps1` | Retain; hidden-linkage checks are historical and partial. | **FAIL compile**: stale math, enum and owner fields. Repair stubs and extend obsolete 47-ID clearance sweep to current 49 IDs; run after Prairie motion, retarget, oil sync or large-driver clearance changes. |
| `.\Validate.PreliminaryPhysics.ps1` | Retain; articulated-engine tests are partial. | **FAIL compile**: missing multiplayer authority stub. Repair boundary and verify captured calibration-input provenance; run after cylinder, mass, adhesion, radius or tender-physics changes. |

Detailed family behavior, partial historical receipts, exact repair notes and run
triggers: `FAMILY_VALIDATION_GUIDE_2026_11_04.md`. Family baselines and logs:
`build/tooling-classification-20261104/`; rod before/after/failure logs:
`build/tooling-tidy-rods/`; native oiling evidence and negative control:
`build/tool-tidy-20261104/`; restored/renderer results:
`build/offline-tools-20261104/` and `build/offline-tools-renderer-routes.log`.
The two rod runtime assertion failures reflect obsolete fixture assumptions;
no production defect was established, and unexecuted assertions are not passes.

Gameplay remains James's responsibility on a separately authorized build.
Installed mod and saves were not changed; no production build, package, push or
publication was performed. The pre-existing audit/document edits are preserved.

## 4 November 2026 — audited baseline corrections, awaiting James's testing

The 2.0.9.4 audit's renderer-lifetime, exploded guest late-join, stock tender
snapshot and mesh-only importer findings were reproduced and corrected. No new
arrangements were added and the 49 existing IDs remain stable. See
BASELINE_FIXES_2026_11_04.md for exact before/after evidence and limits; it
supersedes the hold-for-corrections status immediately below but not the
requirement for James's gameplay acceptance before requesting new layouts.

One integrated Stage 3 Release build compiled core and optional adapter with
zero warnings/errors. Core SHA256 is
40DD503FFC9409CFF9E608EDB3C8165BD8F884B7F6160B9ACB66277666F47CE2;
bridge SHA256 is A0F5F75D471D4E95D8C5A60F7228D2D4C521CB3866A77CAFB0E301E20DD30EF1.
Focused renderer, importer, multiplayer, radio and independent-drive checks
pass. Historical Franklin geometry sweeps used the earlier core hash; their
provenance and the remaining audit fixture gaps are preserved in the report.

No DV launch, save change, installation, release package, push or publication.
Installed remains 2.0.9.3-beta. Gameplay acceptance is James's responsibility
on a separately authorized build: Franklin -> stock -> explosion/repair;
exploded non-stock late join -> repair; stock tender before/after reapply.
Only after acceptance, ask James for the exact arrangement list and append IDs.

## 4 November 2026 — whole-version audit of 2.0.9.4 at `7a6aa1d`

Hold new arrangement implementation pending the explosion-renderer and multiplayer corrections in [the audit and next-session brief](VERSION_AUDIT_2_0_9_4.md). The exact Franklin core hash below was verified. The optional bridge compiled against that DLL and API 1.1. The regular managed handshake, authority, settings and independent-drive tests passed. A separate focused probe reproduced an untouched stock host advertising a powered tender to its guest. The audit also traces a guest disconnect on joining an exploded, modified S282 and identifies importer and per-frame costs.

Installed version verified from the game manifest: 2.0.9.3-beta. The audit changed project documentation only. James owns the gameplay checks, including a two-peer check after the fixes. Historical test receipts below retain their original scope.

## Franklin pin and triplex repair for 2.0.9.4 — 4 November 2026

James subsequently authorised the smallest native production repair, agents and
offline verification. This supersedes the diagnostic-only restriction below.
No DV launch, installation, package, push, publication or save mutation.
Installed remains 2.0.9.3-beta; owned/demonstrator eligibility is unchanged.

Franklin IDs31/32/38/40/47 now separate native pin triangles into owned meshes;
main driver uses the stock long driver-3 pin. Built-in Y/Z PositionConstraint
follows the native coupling animation; RotationConstraint retains wheel spin.
X stays native and independent of prior phase. No custom clock/controller,
dependency, wheel-transform, simulation or network change. Native unreadable
wheels use the existing normalized exporter/OBJ cache. Lease cleanup restores
wheel meshes, LODs and bounds before existing geometry/retarget restoration.

Only middle Franklin casings on IDs38/47 use the existing nose-shaping method
with 0.20 m native shortening, transition Z9.49–9.85. Rear piston face, mounts,
bore axes, engine spacing and articulation remain fixed. Middle outlets follow
the same scaled correction; existing steam socket seating reads the corrected
mesh. ID32's older rear correction, Walschaerts and the 2-2-2 are unchanged.

One integrated core build: version 2.0.9.4, zero warnings/errors. Native final
720-phase sweeps passed 25,920 layout poses across native10/Franklin31/32/38/40/47,
both sides, phase patterns and relevant stresses, zero unexpected contacts.
Pin bore/axial engagement, actual unreadable OBJ path, source restoration and
synthetic LOD membership gates passed. Main bearing full engagement, zero axial
gap; worst coupling-eye centre error 0.423 mm, native short-pin engagement >=71.28%.
Old-nose negative control restores 228 contacts. Headless empty material slots
were a fixture omission, corrected without production changes; all six native
renderers have one material slot matching their one submesh.

See FRANKLIN_REPAIR_2_0_9_4.md for exact coverage and James's in-game checks.
Evidence: build/franklin-repair-2094 and build/triplex-repair-20261104.
Core SHA256 C4AF2E44428DCD74F29E20C80300F8CDDBB473A9A33A822C7C1F3E23482CB07E.
Actual camera/LOD behaviour, whole-TrainCar switching, save/reload, host/guest and
reporter acceptance still await James. No additional production rebuild is
needed for documentation/fixture changes. Installation/packaging/push/publication
require separate authorisation.
All 38 installed package members still match the installation receipt.

## Earlier triplex diagnostic correction — 4 November 2026

James authorised fixture correction/investigation with agents only. Production
geometry remains unchanged; no DV launch, installation, package, push or new
commit. Source remains c9c5d88 / version 2.0.9.4; installed remains 2.0.9.3-beta.
Do not call the pictured missing pin fixed: enabling the main/cutoff renderers
did not prove driver-pin/bearing seating. Full referenced wheel/coupling extents
in inspected Franklin ID31/47 stacks end at least 32.83 mm before the main eye's
inner face; auxiliary parts do not bridge it. Native stock driver 3 has a longer
pin that spans its bearing. Measured radial error also exceeds native control.
Exact pictured eye/close-up ID remain uncertain. This is inherited geometry,
not established as a new regression.

Corrected triplex fixture uses exact extracted production native Position/Aim
methods, 0.4 support / 0.6 rest middle sources and a front hinge carried by the
middle root. Axle_F (0,0,8) was measured from the native prefab with matrices,
path IDs and resource hash.
Triplex yaws now mean synthetic support arcs, not assigned engine angles.
Old triplex fixed-hinge articulation clearances are superseded; do not invalidate
the separately scoped two-engine/Wals evidence or claim gameplay reproduction.

Diagnostic-only build: 0 warnings/errors; reused the existing production DLL.
The native 720-phase, two-pattern, -30/0/+30 support sweep completed 331
assertions but clearance FAILED: 1,052 pair/pose contacts. ID47 crank 380 + main
rod 296 against the following cylinder; ID38 crank 376. ID40 zero only in the
original crank pairs (other parts unmeasured). ID47 actual front/middle yaw was
21.5286/2.5940 degrees, front hinge travel 170.077 mm. All added ID47 following-
casing shaft/UJ/transmission/mount/coupling pairs were clear. A 120-phase onset
check was clear through 17.7199 degrees front yaw, with contact at 21.5286;
the actual live curve envelope is unknown.
Native ID10 / Franklin ID31/32 controls completed 720 phases without unexpected
casing contacts; these passes do not override measured attachment deficiencies.

See TRIPLEX_CONSTRAINT_AND_PIN_AUDIT_2026_11_04.md for exact scope, limits and
James's in-game checks. Evidence: build/triplex-constraints-audit-20261104.
All 38 installed package members still match the installation receipt.
No production rebuild is needed for these fixture/docs changes. Driver-pin seating
and following-cylinder clearance are separate measured repair targets. Prefer
existing native long-pin/mesh facilities for a measured candidate; no custom
motion system or speculative casing change. Current authorisation leaves
production geometry untouched. Actual DV acceptance remains James's testing.

## Demonstrator / owned S282 save audit — 4 November 2026

James requested investigation of save stability if those locomotives are enabled
in 2.0.9.4. Audited c9c5d88 with three focused agents, current installed native
code/assets and LocoOwnership 1.7.0. No production changes or enabling, build,
installation, packaging, push, game launch or save mutation. This audit is
documented in OWNED_DEMONSTRATOR_SAVE_AUDIT_2026_11_04.md; evidence is private
under build/owned-save-audit-20261104 and build/unique-save-audit.

Confirmed demonstrator uses exactly the canonical LocoS282A livery/prefab, with
native unique/restoration/paint state. LMS already supports that prefab; no new
livery or geometry system is needed. Native save/garage cache retains original
type, ID and GUID; restoration separately depends on loco/tender GUIDs. Existing
shared scope guard also controls pre-init graph/oiling, save/load and cleanup,
so do not lift only the radio/unique restriction. Proposed path: prepare native
unique S282 before initialization, admit manual conversion after native service
completion, retain stock/random policy separation and existing-owner save rights.

Normal LocoOwnership purchase preserves unique flag, livery, GUID and simulation:
canonical non-unique purchased S282s already pass current eligibility. However,
LO clears historical owned cars' unique flag only after world loading. Actual
production save hooks reproduced skipped arrangement 34 becoming stock 10 on
the next fresh output after this late clear (managed lifecycle fixture). A
read-only historical Oct17 career JSON snapshot contains one LO-owned S282 with
unique=true; this is relevant input, not proof of past/current game data loss.

Baselines: 65 spawn/ownership and 1,495 selection/save-policy checks pass; narrow
late-flag reproduction passes 68 (65 plus three problem assertions). No whole-
save corruption established; live stability remains unverified. Without mod,
active arrangement siblings are lost at next native save; dormant unique cache
can retain them. Native 24-to-six oil-array load skips mismatched state, so
disabling is not an exact arrangement/oil round trip. See report for bounded
implementation and James-run acceptance gates. Installed mod is still 2.0.9.3-beta.

## Hidden linkage repair committed as 2.0.9.4 — 4 November 2026

James requested committing the completed repair as exactly 2.0.9.4, without a
beta suffix. Info.json, both project version defaults and build/package/checker
defaults now agree. The multiplayer builder accepts the numeric version.
The local commit includes the repair, focused fixture sources and documentation.

The 2 November build and offline evidence below remain the validation provenance;
those private DLLs are still 2.0.9.3-beta and have not been relabelled or rebuilt.
This commit changes version metadata after that validation. Version metadata,
script parsing and the final diff were checked. No installation, packaging,
push, publication or gameplay testing is authorized by this commit request.
Installed version remains 2.0.9.3-beta; James's gameplay acceptance is pending.

## Hidden native linkages repaired offline — 2 November 2026

James requested agents and a bounded repair, excluding the 2-2-2. Read this
handover from line 1 first. Clean baseline 6f850b2 and installed 2.0.9.3-beta DLL
hashes matched the 3540e61 installation receipt. At this validation the repair was uncommitted,
uninstalled source plus private DLLs, still version 2.0.9.3-beta. No packaging,
push/publication or DV launch/gameplay automation. James performs game testing.

See HIDDEN_LINKAGE_REPAIR_2026_11_02.md for measurements, affected IDs, limits
and James's checks. Native tip trajectories prove simply enabling 4-14-4 rods
would miss the cab by 2.45–2.58 m and oiler by .53–.55 m. Extended DriveRetarget
with captured cab/oiler endpoint frames and shaft-only presentation; native
animated transforms/scale, crank clocks, simulation and ratchet source retained.
Per-LOD measured datums, native visibility policies, dynamic LOD bounds and
reverse teardown preserved; existing main/radius retarget shares the owner.

Applied to 18 affected conventional layouts using their own source-derived frames
and original front drives of Walschaerts IDs 37/41/42/43/45/46 using provenance-
checked native maps/hinges. Rear/tender copies have no demonstrated cab/oiler
anchors and stay hidden. Franklin 31/32/38/40/47 replacement gear is untouched.
Native rear reach tips enter the cab; no modeled rear bearing is invented.
Incompatible fixed mid-shaft guide remains hidden; no new pedestal fabricated.

Baseline lubrication fixture passed; old fourteen-spacing HEAD comparison and
Pacific/Hudson missing-importer stub failed before repair. New fixture corrected
its own ID39 native-wheel datum before acceptance; no production geometry changed
for these fixture defects. Final managed check passes 1,220,391 assertions,
576 direct + 1,728 integration poses, 72 visibility/yaw cases, 25 excluded IDs;
endpoint error ≤7.731 µm, retained native lubricator contact bound 2.600 mm.
These are scoped geometry results, not live Animator/ratchet/TrainCar or
gameplay acceptance.

Actual exporter smoke exposed reflected-X OBJ geometry missed by mocked fallback.
Normalize only six new linkage imports, including per-submesh winding; old paths
unchanged. This authoring correction justified second integrated build; final
core + adapter: 0 warnings/errors. Actual compiled importer in isolated Unity 2019.4
player matches all six native meshes/oriented triangles, error ≤.477 µm. No game used.
Evidence/code: evidence/hidden-linkages; private receipts under build/hidden-linkages.
Final core SHA256: 79FEDF9F34EDCD1E193BE84DDDDE23168974258F0D2898F185E7E69850DD58FA.
No rebuild is needed for subsequent documentation/fixture-only changes.
All 38 installed package members still match the installation receipt.
Installation/packaging/push/publication require
separate authorization. Visual/camera/switch/gameplay acceptance awaits James.

## Tender and Walschaerts Mallet repair validated - 31 October 2026

James authorised repair, agents, proper native wheelslip/gear tests, then a new
complete test build and installation. He explicitly included Walschaerts plexes.
Prior read-only restriction is superseded. No push/publication is authorised.

2.0.9.3-beta source: exact new tender mirror configured before activation,
tender's own signed native RPM, safe inactive/unbound-clock stop. Native clone
chain reproduces old freeze (phase delta0) and corrected run passes276 checks,
all4wheels/bothsides/reclones/stop-restart/opposed directions. Native joined
steam/slip/finite-boiler/production-force-method fixture passes30 assertions.

Walschaerts ID37 2-6-6-2 front crank/main rod entered following cylinder at
strong yaw stresses. Dedicated engine gap .150m replaces .100m only in its rear
cylinder separation: rear engine/trailing axle back50mm, hinge back25mm, native
scale/parts/internal motion retained. Common BigMallet/trailing gaps unchanged.
Final actual-source native sweep PASSED112320poses/547checks across stockcontrol
and all11articulated layouts, cutoff/yaw/phase grids, no override or unexpected
contacts. Minimum ID37 surface-separation bound42.308mm. Oldgap1200 and oldID32
nose648 contacts remain effective negative controls. NewFranklinreport not
reproduced in covered meshes; existing ID32fix retained. No new casing surgery.

See TENDER_AND_LINKAGE_REPAIR_2026_10_31.md and evidence fixture READMEs for exact
scope and source hashes. Installed LMS exported actual cylinder/guide meshes;
older guide replay fidelity gap is recorded, not treated as a gameplay cause.

Managed gates passed:1871quartering+190linkage,2305structural,126rear-driver,
1421source/40switches,2402cleanup,2553audio,72nativeengine+56graph,32port+27authority,
219MPbridge+135core. After gap change:3987frame,5349pitch,4067coupling checks pass.
No force-calculation or multiplayer transport change; no incompatible visual
settings combination established. Preserve Settings.xml/captures/saves.

Built, packaged and installed 2.0.9.3-beta from source checkpoint 3540e61.
Both DLL builds: zero warnings/errors. Portable package: 80 checks; all 38
members hash-match installed files. All 39 local files, including Settings.xml,
caches and captures, were preserved; obsolete trial notes and DLL cache removed.
Game closed for install; no full installation backup, push or upload performed.
Desktop ZIP: C:/Users/james/Desktop/Derail Valley Mods/RearrangedS282-B99-2.0.9.3-beta-Nexus/RearrangedS282-B99-2.0.9.3-beta.zip
SHA256: 1498085A0A13584860B35C98192F1288A9ABF55AB563582AFD0900B80D5C095F
Receipt: build/repair-2.0.9.3-beta/installation-receipt.json
Live coupled car forces, save/reload, camera visibility and actual host/guest
acceptance remain unverified. Native fixture passes are not reporter acceptance.
Private native assets/player payloads stay ignored underbuild, never distributed.
The old Unity project-version dialog is avoided by reusing the preserved2019.4
standalone fixture; no real project/game save opened for these regressions.

## Read-only gameplay audit; structural diagnostics improved — 31 October 2026

James reports the tender freeze persists in 2.0.9.2-beta and multiplex crank/
piston clipping also persists (believed Franklin; arrangement/curve unknown).
The preceding candidate is NOT accepted as a successful live repair. Its
synthetic speed-writer reproduction did not cover the actual template clone
chain. Do not present its pass counts as reporter acceptance.

James requested read-only gameplay diagnosis and a repair plan, then authorised
structural-test improvements and agents. Three agents reviewed tender/settings,
Franklin and slip integration. See DIAGNOSTIC_REPAIR_PLAN_2026_10_31.md for the
evidence, confidence, settings analysis and ordered repair/acceptance gates.
No gameplay fix was applied during this audit; no install, package, push or
publication. Installed local mod/log still2.0.8-slip-test, not reporter2.0.9.2.

Likely surviving tender cause: deferred destruction leaves inherited mirrors
alongside the new mirror when attachment's GetComponent sets CopySpeed=false.
It can configure the retiring instance, leaving the active one copying zero.
The delivered ZIP contains this exact sequence. A real clone-chain regression
is required before the smallest explicit-new-mirror-reference repair. Tender
frequency and direction also come from different engines; repair remains planned.
No incompatible supported UMM visual combination is established; preserve
settings and record effective profiles/reapply history. No evidence blames MP.
The earlier ID32 rear-casting nose correction remains present but never certified
all Franklin crank/piston/crosshead contacts. Actual S282/Franklin independent
motion, finite-boiler behaviour and lifecycle were still unverified when slip
became broadly active. Do not add another casing edit without exact contact.

Implemented diagnostics only: lease-aware quartering/Franklin inventory; actual
active tender mirrors and bound clocks; native speed/phase ownership; no empty
wheel-pair passes; independent activation/readiness required so native fallback
cannot masquerade as independent validation. Local/effective settings captured;
motionVerified and clearanceVerified explicitly false for stationary checks.
No writer is enabled/restored by these read-only diagnostic queries.

Regression fails old inventory logic; corrected managed fixtures pass1519
quartering/ownership,190 Franklin-report and2305 structural-validator checks
across93 fixture cases. Native-reference Stage3 build:0 warnings/errors.
No game structural coroutine, actual moving hierarchy, full settings matrix or
host/guest game run performed. Evidence: build/diagnostic-audit-20261031.

## Powered tender wheel regression: 2.0.9.2-beta test - 31 October 2026

James reported stationary powered tender wheels in 2.0.9.1-beta; session role
and arrangement were not specified. The tender-related source and native DV
assemblies were compared with the 2.0.7-beta binary and subsequent changes.
IndependentDrive sets the locomotive Animator speed parameter to zero while
its native phase reader owns the pose. The tender mirror writes that zero in
Update, before animation evaluation; the tender clock writes its correct speed
in LateUpdate, too late to prevent the next mirror update from overwriting it.
The original 2.0.7 binary has both writers but lacks that independent owner.

Actual production mirror behavior reproduced the stationary tender in a real
Unity 2019.4.40f1 standalone player using a synthetic controller and a simulated
LateUpdate tender clock. The fix lets the tender clones opt out of copying only
host speed; their existing tender clock owns speed and the mirror retains other
parameters. The same player passes forward/reverse and ordinary mirror checks.
The exact reported game session has not been reproduced and live vehicle,
coupling, save/reload, both sides, host and guest acceptance remain outstanding.

Native compilation: zero warnings/errors; actual native steam engine 72 checks,
graph/lifecycle 56, native player animation 16, slip port/authority 32/27,
multiplayer bridge/core 219/135, tender cleanup 2402, audio 2553, clone/follower
126 pass. Fixture stand-ins and native-player limitations remain; see
TENDER_WHEEL_REGRESSION_2026_10_31.md. Compiled comparison with 2.0.9.1-beta:
2074 same-name methods unchanged; only attachment, mirror Update and its
constructor differ, with three mirror compiler-generated methods renamed.

Portable 40-file test archive (not installed, pushed or published):
C:/Users/james/Desktop/Derail Valley Mods/RearrangedS282-B99-2.0.9.2-beta-Tender-Test/RearrangedS282-B99-2.0.9.2-beta-Tender-Test.zip
SHA256 DD61D4EE0161DC08FF789E76162D3D5F2B3B67244C5EB80F66C135942161C28E.
Both DLL hashes and all 40 entries match staged files. Build receipt records
parent 163d081 plus dirty source hashes because package preceded checkpoint.
The archive is a test candidate; all multiplayer peers need the same build.

The fresh Unity fixture startup displayed a version-mismatch dialog and timed
out. Its generated ProjectVersion.txt had PowerShell 5's UTF-8 BOM and lacked
the revision; the runner now writes Unity's full format as ASCII. This likely
addresses that prompt but a fresh editor launch has not yet verified it. The
dialog has closed. Reused the previously built isolated player for the actual
before/after motion checks; no real Unity project or game save was touched.

## Current package: 2.0.9.1-beta - 31 October 2026

James explicitly corrected the release name: 2.0.9.1-beta, not 2.0.9-beta.1.
All previous arrangement/slip fixes remain included. Four-part assembly version
must be 2.0.9.1 and informational/UMM version must be 2.0.9.1-beta. Builders and
archive validator now support that numeric version. Earlier archives retained.

Investigated the report quoting 'use the complete matching test package'. Its
only source is optional adapter startup after 15 failed attempts, NOT the peer
identity handshake. The tester's Player.log was not supplied and local current
logs do not contain that failure, so a specific original cause is unconfirmed.
New UI distinguishes missing adapter, missing native API, API readiness and
underlying load exceptions; complete stack remains in Player.log. Do not blame
a mismatched ZIP or remove online fail-closed protection without evidence.

A related initialization defect was reproduced: setting initialized=true before
startup succeeded meant a failed attempt could make the retry return a false
success. Initialization now marks success last, detaches failed callbacks and
destroys its failed runner. Core reasserts the startup lock after failed session
cleanup. Managed regression failed old source and passes the correction.
See MULTIPLAYER_STARTUP_REPORT_2026_10_31.md and ignored loader-report evidence.

Source remains on codex/s282-multiplayer. No owned-locomotive policy, main branch,
installed mod or game save changes. Repair appearance report remains separate.
Build/package receipts record the final source commit and archive hashes; no
Nexus upload is performed.

## Current hotfix: 2.0.9-beta.1 - 31 October 2026

Supersedes the current-version label below, retaining the complete selected base.
See BUGS_2026_10_31.md for exact reported symptoms, reproduced causes and evidence.
The late-loaded custom ECS Ownership component caused independent-slip setup to
throw and arrangement conversion to restore stock. Removed that unnecessary tag;
private wrappers/versioned native entities continue to own helper lifetimes.
Also replaced connect-once RPM calls with scoped reference leases/restoration.

The isolated release-player fixture now initializes the native game ECS registry
before loading the mod. The old source reproduces the reported exception; the
fixed source passes 46 native checks and 13 animation checks. A faithful native
RPM boundary fails old source and passes the fixed 27 ownership checks (plus 32
port checks). Relevant graph, engine, bridge, core, radio and oiling tests pass.
Those checks are not a live locomotive or multi-peer acceptance run.

Owned-locomotive restrictions are expected, explicitly confirmed by James, and
unchanged. Do not remove unique/owned-car safeguards while revisiting this work.
The repair tubes/rust report remains separate and open; prioritize arrangement
rollback as James instructed. The reporter already resolved the visual-settings
freeze; do not claim this hotfix fixes it or blame another mod without evidence.

Build.Multiplayer.ps1 and Package.Nexus.ps1 default to 2.0.9-beta.1. Nexus copy
is plain text in release-copy/2.0.9-beta.1/DESCRIPTION.txt. Run the package checker
with -Version 2.0.9-beta.1. The original 2.0.9-beta archive remains unchanged.
Final receipts record the actual clean commit/hashes; no installation or Nexus
upload is performed. Keep updates on codex/s282-multiplayer; main/b99-port and
other checkouts remain outside this hotfix.

## Current release: multiplayer 2.0.9-beta - 29 October 2026

This section supersedes the older 2.0.8-alpha baseline below. James explicitly
selected 2.0.8-slip-test, including experimental independent wheelslip, and
then specified the public version 2.0.9-beta. Do not restart from e65a3da's old
2.0-beta gameplay code. Selected immutable source:
31389e811d0b99fb68b238174fd59b9264f389ef.
Its inherited Info/default project version says 2.0.7-beta; the verified trial
was built as 2.0.8-slip-test. The selected ZIP SHA256 is
B3677FC1D8D42F9423E702C044DB75B84E0E31C5C9EDB71CD62AD56C4D35B94D.

Work remains in B:\GitHub repo\rearranged-s282-multiplayer, branch
codex/s282-multiplayer. The normal b99-port and independent-slip checkouts and
indices are untouched. Prior explicit authorization to push this MP branch to
remote james persists. No main/b99-port push, install, merge, GitHub release or
Nexus upload is authorized by this task. Source commits are reviewed checkpoints;
package receipts and final chat distinguish actual packaging and push outcomes.

The complete selected runtime and its regression sources were restored before
reapplying multiplayer hooks. Includes 49 arrangements/Express48, 44 powered-
tender choices, owned 24-point oiling and six/20 save migrations, exact normal
S282 scope, quartering, lubrication/cup mounts, pipe/tender/LOD teardown,
Franklin renderer/nose/outlets and experimental locomotive-group wheelslip.

Native-first integration: use supported MPAPI custom packets for arrangements
and native readonly flow ports for 3 groups' RPM/slip/phase/force presentation.
Native MP continues to own transport/train motion. Only authority runs native
slip ECS, force, steam-engine ticks, cracked-state repair and oiling writes.
Guests never create independent slip ECS or rewrite engine RPM inputs. Stable
ports are appended before native graph initialization. Exact GUID/epoch/revision channels
reject old layout samples; host phase offsets bind the existing native readers.

Every player's radio is independent. Host requests remain stationary/safe;
UMM can allow all guests or block individuals. Lock checks are enforced on host
packet handling. Protocol 2 uses epochs/revisions, bounded tombstones and
request/snapshot limits. Native ready-stage handshake, stale instance callbacks,
ID reuse, permission notices and fatal replica timeouts have explicit fixtures.
Identity includes both loaded DLLs and all four runtime assets, once cached.
Newer/future tender preferences cannot invalidate a compatible car profile.

Absent/inactive MP and native internal single-player retain ordinary controls,
with no multiplayer UI/status. Main DLL has no MPAPI assembly dependency. Only
the optional adapter references API1.1. The supplied 0.1.16.0-beta archive/API
was checked against official release/source; native source and references stay
under ignored build/, and neither MP nor its API is installed or distributed.

Build with Build.Multiplayer.ps1 -Version 2.0.9-beta, then Package.Nexus.ps1
from the recorded clean commit. Validate.MultiplayerPackage.ps1 checks final ZIP
layout, metadata, optional references and byte-identical seed assets/exporter/
licences. Release copy is release-copy/2.0.9-beta; final UMM archive is
release/2.0.9-beta/RearrangedS282-B99-2.0.9-beta.zip and copied to
Desktop/Derail Valley Mods/RearrangedS282-B99-2.0.9-beta-Nexus/.

Validation details: MULTIPLAYER_2_0_9_VALIDATION.md and ignored build receipts.
Native/managed fixtures are stronger than compilation but are not a live
multi-PC playtest. External group acceptance remains pending. Powered-tender
independent slip is still absent; global slip/curve policy is still pooled,
true compound expansion is not modeled, and native failure/derailment falls
back to the earlier shared traction path (reapply after rerailing). Do not
silently widen scope or claim those limitations were solved by networking.

The user's lowercase drafting preference was for an email. Normal capitalization
and Markdown in chat were explicitly restored. No tester email is sent here.

## Nexus-facing multiplayer 2.0.8-alpha - 29 October 2026

James explicitly authorised pushing this branch to his GitHub fork, while
keeping it separate from the main version, and preparing a Nexus-facing
2.0.8-alpha build. Work remains in this isolated checkout on
codex/s282-multiplayer. Push only that branch to remote james:
https://github.com/james-taplin/dv-rearranged-S282
The default branch b99-port and branch main must not move. Before this work,
their remote heads were respectively 14b112841a53ab53e4162e52ec8df06b14ae6948
and 24e24d5a4e859e8c792de35d6df420c6177c6803.

2.0.8-alpha retains the gameplay implementation from 2acef90 and its 2.0-beta
arrangement baseline. This is a version/documentation/package change; later
single-player fixes and independent-wheelslip development are not merged.
The normal release remains a separate line. Both packages use RearrangedS282
as their mod ID/folder; this is an alternative build, not a side-by-side mod.

Version metadata is 2.0.8-alpha in Info.json and both project defaults. The
multiplayer builder accepts this public alpha identity. Package.Nexus.ps1 now
reads the multiplayer candidate receipt, checks both DLLs, preserves the
MultiplayerCompatibility All and optional single-player fallback, and emits
checksums, source/build receipt and a release manifest alongside the ZIP.
No gameplay C# or meshes are changed. Native game/API references are build-only.

release-copy/2.0.8-alpha contains the plain player README, compressed changelog,
file information, UMM options, complete credits and BBCode Nexus description.
Offer it as an optional experimental multiplayer file; keep the normal file as
the main download. Requirements, identical peer packages, host permissions,
failed-sync disconnection and live-testing limits are stated. The original
credits/licences are retained and AMacro/DV Multiplayer contributors credited.

Build: Build.Multiplayer.ps1, then Package.Nexus.ps1. Outputs:
release/2.0.8-alpha/RearrangedS282-B99-2.0.8-alpha.zip
Desktop/Derail Valley Mods/RearrangedS282-B99-2.0.8-alpha-Nexus/
The final release manifest records source commit and both DLL/ZIP hashes.
This task authorises a branch push, not a GitHub release, Nexus upload, merge
or installation. Live multiplayer acceptance remains pending; do not present
offline checks as a successful live session. The previous alpha evidence and
known limits below still apply. Package/build receipts record fresh validation.
Fresh 29 October checks: 182 bridge/codec, 78 core and 287 radio/settings
assertions pass. Plain README, BBCode balance, script parsing, unchanged C#
source and no newly introduced binary/raw-log Git history were checked.

## Multiplayer development branch - 23 October 2026

James requested a new development branch for shared radios, optional per-player
arrangement lockout, and synchronized physics/visuals. Multiplayer is not installed
on this PC; external players will test the first package. No local Multiplayer
installation is required or authorized by this development task.

Work in B:\GitHub repo\rearranged-s282-multiplayer on codex/s282-multiplayer.
It is an isolated native Git worktree of the canonical S282 repository, based on
30c2a80 (the current source and latest handover). All 213 source/config/model
entries checked against that checkpoint matched the live source before branching.
The original b99-port checkout and its index were not switched or staged. The
Codex worktree attachment tool cannot attach this externally created worktree.

Initial package version: 2.0.1-mp.alpha.1. This is a multiplayer alpha, not a
replacement public 2.0-beta release. Build.Multiplayer.ps1 preserves the assets
and licences from the verified 2.0-beta ZIP, builds the main mod plus its optional
MPAPI adapter, and creates the external tester ZIP/checklist/hash receipt.

Native-first decision: inspected the current source/handover, installed S282,
Dad's Decals multiplayer adapter, and AMacro/dv-multiplayer primary source before
implementing. Use the supported serializable custom-packet API, stable CarGUID,
and existing MP host-only native simulation/port replication. Do not add another
transport, custom train movement replication, server, or replacement radio API.
The small optional adapter assembly isolates MPAPI from single-player loading.
MultiplayerAPI.dll remains a build-only reference and is not shipped.
The compiled main assembly has no Multiplayer/MPAPI assembly reference. With
Multiplayer absent or inactive, Initialize/Tick never load the adapter; ordinary
single-player radios, automatic selection and native forces keep their authority.

Confirmed target: DV Multiplayer 0.1.16.0, MPAPI 1.1.0.0. Primary beta source
inspected at 1a5aeae20c66c88c10f38a531daabab656afac41; release tag points to
d142f90519b53e8b1af4eba4b85a2873b5c4da65. Important upstream references:
https://github.com/AMacro/dv-multiplayer/tree/beta/MultiplayerAPI
https://github.com/AMacro/dv-multiplayer/blob/beta/MultiplayerAPI%20Tests/TestComponents/ServerTest.cs
https://github.com/AMacro/dv-multiplayer/blob/beta/MultiplayerAPI%20Tests/Packets/SimplePacketWithNetId.cs

Host validates operating conditions and request revision. Each accepted change
includes a per-car copy of host settings (including powered tender and gear
presentation). Clients never save these settings over their own preferences.
Host settings edits affect that car on reapply. Client random/saved local choices
are deferred to host state. Revisions handle reliable unordered packets; queued
GUID snapshots cover late car creation and joining an already-moving train.
Only validated replica replay bypasses the stationary/regulator operation check.
Native readiness and geometry guards remain. Custom auxiliary steam ticks and
extra curve/tender forces execute only on the simulation authority; native
mirrored ports continue to drive guest animation/effects/audio.

Per-radio ownership replaces the static radio-controller source. The host can
allow all guests or lock individual guests in UMM. Locks are session scoped and
cleared on disconnect because native player byte IDs are reused. MP's native
compatibility metadata checks mod IDs, so the adapter additionally requires exact
protocol and main/adapter DLL hashes; mismatching or non-handshaking guests are
removed. Never offer a local-only gameplay fallback in an incompatible session.

The native MP port stream can precede custom arrangement state and contain a
non-four powered-axle count. The replica baseline fix accepts the untouched
per-car four-axle definition only when the independent physical baseline remains
six total/four powered axles. Other inconsistent baselines are rejected with
actual values logged. This source evidence does not retrospectively establish
the cause of the historical tester failures below.

Guest setup also preserves already-received crank/steam/chuff/oiling ports;
native dirty-only updates may otherwise never resend those stationary values.
Confirmed conversion rollback broadcasts actual stock with its tender disabled.
A failed guest conversion reports the error and disconnects that guest instead
of leaving a known inconsistent replica in play.

Validation: both net48 projects compile with zero warnings/errors. Offline
checks passed: 182 bridge/packet checks in 16 cases; 287 radio/settings checks in
20 cases; 78 core policy/profile/revision/queue/fallback checks in 10 cases, in
build/multiplayer-validation/core-validation.json; 92 native-engine checks,
58 graph/lifecycle checks, 305 physics assertions and 42 oiling checks. The
explicit absent/disabled-Multiplayer test uses a missing adapter file and still
allows offline changes. The real main DLL's assembly references contain no
Multiplayer/MPAPI dependency. Fixtures clearly label their managed boundaries.
Oversized-packet testing caught an uncaught InvalidDataException; the explicit
decoder catch is now covered by the passing codec tests.

External ZIP and build-receipt.json are under Desktop/Derail Valley Mods/
RearrangedS282-B99-2.0.1-mp.alpha.1-Test. Build.Multiplayer.ps1 verifies all
32 protected seed-package members byte-for-byte and excludes MPAPI from the
runtime payload. The receipt records the exact source commit, source hashes,
two DLL hashes and final ZIP hash. Source/docs/tests are committed locally on
this branch before the final package build. Nothing is installed, pushed or
published. Original b99-port remains the active branch of its own checkout.

Live host/client, ownership changes, latency, late joins, audio/effects and saves
remain external acceptance tests in MULTIPLAYER_TESTING.txt. Do not infer live
multiplayer readiness from compilation or offline harnesses.

## Historical tester log review - 23 October 2026

Reviewed three supplied Player logs from Downloads; captured session reports
20October/Rearranged metadata1.9.2/RF1.13.8/Multiplayer0.1.16/Utilities1.1.2.
These are cumulative EXACT byte prefixes: Rearranged2606lines -> ON6696lines ->
OFF18838lines. All load RF; no independent RF-free or cylinder-toggle A/B control.
128 shared MissingField isEligibleForSleep errors point to Utilities.Update
(already documented16Oct). Ten shared Rearranged B99PhysicsState baseline-guard
conversion failures are genuine; current2.0 retains this safety guard. Which
axle/port value failed is not logged; multiplayer/client lifecycle is a lead,
not a confirmed cause. Distinct from the corrected extra-Franklin-source bug.
Three unowned Animator.GotoState/invalid-layer warnings also worth tracking.
RF logs boiler/flow/firebox changes but no RF exception or measured runaway
power/resource trace; do not claim these files prove RF caused all symptoms.
Review/line refs/hashes: evidence/tester-logs-20261020/REVIEW.txt. No raw log
copies, code fixes, release changes, install, launch, push or publication.

## Compressed changelog since 1.22.2 - 23 October 2026

James requested the changelog again with related entries merged, proper names
capitalised, and light commas/semicolons/hyphens/colons allowed. Updated current
release-copy/2.0-beta/CHANGELOG.txt and the changelog in the full BBCode Nexus
copy; old1.23.2-Beta link remains retired. Eight change entries plus retained
physics note, covering the same release scope. Repo/Desktop copies and packaged
CHANGELOG.txt refreshed. Exact comparison: only CHANGELOG.txt changed in ZIP;
plain README, DLL, assets and licences unchanged. Existing213 package checks pass.
ZIP SHA256 48A4C061CE75423B6DE0760AE76DC652521940B93520111856E585580E4760C0.
Desktop filename remains Rearranged S282 B99 Fork 2.0-beta.zip. Evidence:
build/release-2.0-beta/changelog-audit.json. No rebuild/install/push/upload.

## Packaged README is plain text - 23 October 2026

James requires the ZIP's README to have no special formatting. The actual
RearrangedS282/README.txt is now plain ASCII text with Windows line breaks,
wrapped paragraphs, ordinary headings and plain URLs. No Markdown, BBCode,
table syntax, styling markers or bullets. Source release-copy/2.0-beta/README.txt
updated; repository README.md remains unchanged in Markdown. Nexus copy remains
BBCode. Package.Nexus.ps1 rejects formatted READMEs and includes a standalone
plain README alongside the Desktop release copy.

Repackaged 2.0-beta without rebuilding or installing. Exact archive inventory
comparison: README.txt is the ONLY changed member; DLL/assets/licences and all
other members unchanged. Archive CRC and all213 existing package checks pass.
New ZIP SHA256 B9CBF22338F1889350E4B6E5B9E1678C361FABD8CDE735A657DF57E53FAE6DD7.
DLL remains4B9437879C31C9677DE69C373B66C78ABF8D6A0250A1B1556F77428482E5D46B.
Repo archive: release/2.0-beta/RearrangedS282-B99-2.0-beta.zip.
Preserved James's renamed Desktop filename: Derail Valley Mods/
RearrangedS282-B99-2.0-beta-Nexus/Rearranged S282 B99 Fork 2.0-beta.zip.
Desktop checksum uses that filename; manifests refreshed. No duplicate/stale
Desktop ZIP, backups, install, push or publication. Evidence in
build/release-2.0-beta/plain-readme-audit.json. Local checkpoint followsbf05dce.

## Nexus copy converted to BBCode - 23 October 2026

James requires Nexus copy in BBCode. Current release-copy/2.0-beta now contains
DESCRIPTION_BBCODE.txt, CREDITS_BBCODE.txt and NEXUS_DESCRIPTION_BBCODE.txt (full
description plus complete credits, ready for one paste into the Nexus BBCode
editor). NEXUS_RELEASE_COPY.txt now uses BBCode too, retaining the separate
summary/file info/changelog sections. Markdown tables became labelled family
paragraphs; headings/bold/italics/lists/links use standard BBCode, with no Markdown
backticks or escaped line breaks. Link targets preserved; balanced tags checked.

All four files copied to the repo release folder and Desktop Nexus folder.
Package.Nexus.ps1 carries these BBCode files alongside other release copy on
future packaging. Root README and Markdown source docs stay Markdown. Runtime
ZIP remains byte-identical (SHA256 4B83637FD4BBF44A6907261A45C4AE5DCCC64A4CA143B7438A368F5FCF0198FE);
no rebuild/install/push/upload. Evidence: build/release-2.0-beta/bbcode-audit.json.
Local documentation checkpoint follows3f22270; active checkout/index preserved.

## Corrected release identity: 2.0-beta - 23 October 2026

James corrected the public release/version to exactly 2.0-beta. This supersedes
the mistakenly named 1.23.2-Beta public package described below; do not upload it.
The prior numeric versions remain valid internal build/test history only.

Root Info.json, Stage3 project default, Build.RuntimeTrial/Package.Nexus defaults,
README, all full Nexus copy, credits/options references, file info and changelog
now use 2.0-beta. Changelog consolidates all changes since public 1.22.2 under
one release heading. Current public docs: release-copy/2.0-beta.

Installed UMM's actual ParseVersion accepts 2.0-beta as 2.0 and orders it above
1.22.2. Standard CLR/file version is 2.0.0.0; informational version is exactly
2.0-beta. Main's display and structural report read the informational version.
No stale checkout hash is appended to this release's informational metadata.
Clean net48 build: 0warnings/0errors. Full method comparison against 1.23.2:
1720 methods identical after normalizing compiler-generated Main ordinals;
only UMM display and structural report version calls differ, with one version
getter added. All gameplay/geometry/physics methods unchanged.

Final ZIP: release/2.0-beta/RearrangedS282-B99-2.0-beta.zip, also in Desktop
Derail Valley Mods/RearrangedS282-B99-2.0-beta-Nexus with full copy and checksums.
SHA256 4B83637FD4BBF44A6907261A45C4AE5DCCC64A4CA143B7438A368F5FCF0198FE.
DLL SHA256 4B9437879C31C9677DE69C373B66C78ABF8D6A0250A1B1556F77428482E5D46B.
Package audit 210checks/36files; old runtime assets/exporter/licences identical,
no old release numbers in current public copy. Remove only superseded generated
public package folders after confirming Desktop files match the old repo copy;
retain internal test archives, diagnostic captures and historical handover text.
Evidence: build/release-2.0-beta/{audit.json,version-il-check.json}; final inventory
release/2.0-beta/release-manifest.json. Installed mod remains 1.23.2 test; this
request did not install, launch the game, push or publish. No new runtime pass
claimed. Local checkpoint follows 795eabe with active branch/index preserved.

## 1.23.2 Beta release copy and Nexus package prepared - 23 October 2026

James requested a full new Nexus description, credits, file information and a
compressed versioned changelog since public 1.22.2, plus a human-facing repository
README. These are now in release-copy/1.23.2-Beta; NEXUS_RELEASE_COPY.txt combines
the complete draft. Root README.md and canonical release-copy/CREDITS.txt updated.
Catalogue: 48 choices; 43 compatible optional powered-tender choices. Document the
unfinished giant 2-2-2, unpowered 4-0-4, parallel rather than series compounds,
simplified tender grip, unsupported Realistic Firing and unverified MP/VR plainly.
No runtime source or driving physics changed for this packaging task.

Package.Nexus.ps1 reuses the exact installed 1.23.2 DLL and runtime-trial payload,
replacing test-only instructions with player documentation. Original LICENSE,
THIRD_PARTY_NOTICES.txt, all eleven library licence files and original mod assets
remain byte-identical. Package audit: 167 checks pass, 36 files, safe archive
paths, no user settings/recordings/dev tools; installed DLL matches exactly.
No rebuild, reinstall, full backup or rollback copy. Latest live structural run
is still the failed 1.23.1 capture; James expresses confidence in 1.23.2, but no
new full game result was inspected and runtime acceptance is not claimed.

Nexus ZIP: release/1.23.2-Beta/RearrangedS282-B99-1.23.2-Beta.zip
Desktop copy: C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-1.23.2-Beta-Nexus
ZIP SHA256 8C60B3139CEC090AB39FD34D39164192C4B3A0DD272FC6663FB82BB6CA3FCE1A.
DLL SHA256 C4C216972DE6687E2F4A2C3FF5862AACFAFEDC5852CC22636EFA278C77D73A1A.
Receipt/inventory: release/1.23.2-Beta/release-manifest.json and
build/release-1.23.2/audit.json. Local release documentation checkpoint follows
13c5976 using an isolated Git index; active b99-port branch/index left alone.
Prepared and packaged locally; not pushed, uploaded or published by Codex.

## 1.23.2 installed: fix active Franklin copies in rear/tender templates - 23 October 2026

James reported the1.23.1 structural run failed. Actual JSON version1.23.1.0:
85/91main cases pass. IDs41/42/43 (Angus/Great Northern/Erie), both tender modes,
fail conversion with8cylinders found/4expected. Three Erie return checks fail
for same cause; other3returns pass. New ID47 passes with6cylinder sockets,
3active engines and intended4bar-frame pieces. Final stock restoration and
saved tender preference preservation pass. This is a real conversion error,
not permission to loosen the4cylinder expectation. Logs confirm8casing meshes.
Focused findings/hash under build/runtime-trial-1.23.2/previous-run-findings.json;
original capture left untouched in mod folder.

Cause: EnsurePoweredTenderSource calls EnsureArticulatedAssets, which creates
both Franklin variants active under each native drive. It cloned the shared
8-coupled rear drives before hiding those source assets. Four active Franklin
cylinders were inherited by the copies, outside the original assets list. These
rear drives break the3four-axle-rear Mallets, and also contaminate powered tender
visuals despite their previous binding checks passing. Corrected only ordering:
hide optional articulated assets before rear/tender cloning. Native Walschaerts,
accepted pipe geometry and all force/boiler/adhesion calculations unchanged.
Added tender report unexpectedFranklinAssemblies, requiring0during structural
powered cases. Existing strict cylinder counts retained.

Regression fixture now models both live source variants and deep clone active
states. Actual1.23.1 source fails new assertion before fix; corrected source
passes853checks including40cache-regeneration cycles. Structural validator1081
checks/91cases pass. Clean net48 game-reference build0warnings/errors; compiled
preservation286checks/124protected methods plus force/tick/layout checks unchanged
against1.23.1. No geometry or driving-physics redesign.

Installed exact1.23.2-Test ZIP after process check:36files byte/hash verified,
settings/current captures retained; no backup copies/push/publication. Structural
button retains91main cases and6direct Erie returns. Ask James to repeat full run;
1.23.2 live acceptance pending. DLL C4C216972DE6687E2F4A2C3FF5862AACFAFEDC5852CC22636EFA278C77D73A1A.

## 1.23.2 installed for James - 23 October 2026

Installed the exact previously audited test ZIP after verifying DerailValley was closed. All36 packaged files match the archive byte-for-byte; DLL SHA256 C4C216972DE6687E2F4A2C3FF5862AACFAFEDC5852CC22636EFA278C77D73A1A. Settings.xml and previous diagnostic captures preserved. Removed only retired shipped diagnostic/export instructions and Tools/TractiveTable.ps1. No backups or rollback copies. Build/delivery/installation receipts updated under build/runtime-trial-1.23.2. Runtime acceptance remains pending; not published.

## 1.23.1 compact pipes, conversion fixes and structural matrix - 23 October 2026

Installed exact final test ZIP after game-closed check; all36files verified,
settings and existing captures unchanged; no backups, push or publication.
DLL7886B6403837907503EF715EC1EBB278438E42A1DA341F980420A4ACF9EDE302.
ZIP77CE46B934CFBCFD1327850B9BE9E0184FDCA31BA49F611E501B70F81704E67E.
Receipts under build/runtime-trial-1.23.1; Desktop1.23.1-Test holds the archive.

James reported excessive outboard pipe ends, new2-4-4-4-0 falling back to Mikado,
and Erie tender remaining ordinary after other layouts. Player.log confirms
frame split failures at18525/18573/18602: second pin localZ-1.721 while main bar
ends-1.670. Only ID47's final rear slice may be empty when the frame ends within
.10m ahead of the measured hinge. No hinge/driver relocation; no fabricated frame
extension or global exception suppression. Native pin/aim articulation retained.

Erie attaches at18289 then fails18909 with dictionary KeyNotFoundException on
return. Shared Mallet static parts can be rebuilt; tender poses were keyed by
disposable transforms. Tender now owns one inactive template root with independent
drive/static copies and frozen native matrices. Attachment no longer reads the
shared static cache. Partial source construction cleans up before retry.

Pipe headerX1.45->1.30m, compact side entries with35mm buried socket overlap,
shorter riser-to-header turn and single diagonal rear manifold connection. James
explicitly accepts small local fitting overlap; avoid long visibly buried runs.
Original profile/collars/UV/materials and minimum bore retained. No physics edit.

James then requested ONLY structural diagnostics restored and the full powered-
tender matrix. Default EnableStructuralDiagnostics=true is separate from disabled
EnableDiagnostics/full-power recording. One UMM structural button; no power tick,
recorder, exporters or sizing UI. Stage2+read-only TractiveDiagnostics compiled;
JSON only, no structural CSV exporter call. Runtime force/handling unchanged.

Run uses two stationary stock S282s, subject with ordinary supported S282 tender
coupled behind; control has no powered tender active. Per-subject temporary
TrySwitchStructural override never writes saved preferences or saves.91main cases:
48ordinary +43powered, excluding33/36 novelty/prototype and38/40/47 locomotive
triplexes from powered mode. Frame count/actual ID/active engine count, private
source cache, tender geometry/exhaust/particle/audio bindings, native tender
bogie active-state restoration and control isolation checked. Restores ordinary
stock after every case, including stock+powered mode. Six direct switches
43on->46off->47off->43on->0off->43on reproduce return-to-Erie. Final stock,
tender/control restoration and unchanged UMM preference array required for pass.
Power/resize controls remain absent. Close UMM during test. New run not yet done.

Validation: clean game-reference net48 build0warnings/errors; frame3987 checks,
tender source846/40regeneration cycles; compact layout19; structural matrix1038
checks/91cases; pipe validator12; UMM/radio274checks/18cases. Compiled integration
286checks,124protected methods unchanged vs1.23.0, plus existing tender force,
tick and layout calculations unchanged. Pipe pure/lifecycle suites pass;44neutral
recorded layouts and76neutral/imposed poses generate without errors, neutral and
+/-6degree static checks have no crossings.12imposed +/-12degree poses show contacts
across8layouts; these are synthetic, not game curve measurements. New ID47 lacks
a recorded geometry capture; no full in-game visual/audio/constraint acceptance
claimed. Test all91cases then look/listen on curves; clipping polish remains manual.

## 1.23.1 installed for James - 23 October 2026

Installed the exact previously audited test ZIP after verifying DerailValley was closed. All36 packaged files match the archive byte-for-byte; DLL SHA256 7886B6403837907503EF715EC1EBB278438E42A1DA341F980420A4ACF9EDE302. Settings.xml and previous diagnostic captures preserved. Removed only retired shipped diagnostic/export instructions and Tools/TractiveTable.ps1. No backups or rollback copies. Build/delivery/installation receipts updated under build/runtime-trial-1.23.1. Runtime acceptance remains pending; not published.

## 1.23.0 installed for James - 23 October 2026

Installed the exact previously audited test ZIP after verifying DerailValley was closed. All36 packaged files match the archive byte-for-byte; DLL SHA256 EA18CEFE3EC13664C708CDA3BA66CAD0A71EC3FB2FB9CB0EBEBD8B9DD7E852DE. Settings.xml and previous diagnostic captures preserved. Removed only retired shipped diagnostic/export instructions and Tools/TractiveTable.ps1. No backups or rollback copies. Build/delivery/installation receipts updated under build/runtime-trial-1.23.0. Runtime acceptance remains pending; not published.

## 1.23.0 triplex and optional powered tender test - 23 October 2026

James requested a more usable articulated 2-4-4-4-0 with all engines on the loco,
plus optional Erie powered tenders on other arrangements, explicitly including
rigids. He then requested diagnostics and resizing options be removed from this
build. Game was running at request; build/package only, no installation.

Appended save/catalogue ID47 (48 entries), default random spawn OFF. Reuses the
native pin/aim articulation and Franklin two-axle engine/short coupling rod in
three groups. Six native powered axles, one leading axle, no trailing truck and
no powered tender. Same existing three native engines, per-arrangement reduction-
only calibration and shared locomotive traction/slip architecture. Added first-
axle brakes/springs, all six sparks, existing oil cups/steam/casing/LOD handling.
Exact prepared wheel positions retained for the new layout. Shorten only0.10m
native cylinder reach versus0.45m on single-driver triplexes: avoids crowding
first driver by casing. Measured layout radius~0.596m, diameter~1.19m.

UMM Powered tender - arrangement options (test):43 compatible rigid/two-engine
layouts. Only Erie enabled by default; Erie can also be unchecked. Global policy
per arrangement; applies on spawn/reload or explicit safe REAPPLY on highlighted
loco. Already-loaded neighbours do not all change immediately. Three-engine loco
triplexes38/40/47 cannot add a fourth engine;33 novelty and36 prototype excluded.
Regular S282 tender only, ordinary rear coupling. No new radio mode. Selection
stored as XML array: test caught prepopulated List append re-enabling Erie on
reload, fixed with replacement-array serialization and empty round-trip check.

Tender source measured/cached from prepared native pose independently of host;
Franklin source parts explicitly hidden on rigid hosts. Freeze original static
maps/cylinder-front datum against later host movements. Reuse existing Erie
geometry, exhaust/cocks/drips, weight-limited force and native boiler intake sum.
Third native engine owns tender torque/exhaust separately, private input adapter
scales only its RPM by host/tender wheel-radius ratio during native Tick (restores
reference in finally), never writes shared host RPM. Native engine checks compare
same torque/intake/phase/frequency at four ratios against B99 engine calculation.
No new grip/weather/sanding/independent-slip model. Powered tender adds steam use
and pull, no boiler capacity. Existing host calibration/handling remains intact.
Stock is exact with its tender option OFF; opting in explicitly adds tender power.

Added missing native Mechanism_Layered/CHUFF_FREQUENCY clanks at tender driver
centre, alongside1.22.4 tender chuffs/loops/cock audio. Uses existing native audio
reader lifecycle. Tender animation owns speed from tender frequency; reuses native
AnimatorPortReader for Cutoff with original authored reverser.REVERSER settings,
so a Franklin host without native Cutoff parameter does not freeze tender gear.
Detach/stock restore/pooling release the other car's engine and native readers.

Accepted fixed pitch/rearward values: Mogul0.18/0.12m,2-4-0 0.20/0.14m,
Prairie0.22/0.12m,American0/0m. Accepted cylinder normalization always enabled;
prototype diameter fixed2.8m. Old tuning fields XML-ignored and no longer consulted
by runtime geometry. No slider attributes or cylinder resize toggle remain.
Valve gear/trucks/curving/oiling/random spawning retained. Default project excludes
Stage2RuntimeChecks,PowerDiagnostics/PowerDiagnosticsTick,TractiveDiagnostics and
all recorder registration/UI. Developer diagnostics can be compiled explicitly
with EnableDiagnostics=true. CurveDiagnostics.SampledCurvature still supports
unchanged curve handling. Package excludes recorder/export tools and dev reports;
normal game/error logging retained. Manual acceptance only, no structural button.

Validation: clean .NET48 game-reference build,0warnings/errors;68 real native
engine checks;54 managed graph/lifecycle checks;19 extracted compact-layout checks;
2553 extracted tender seating/selection/reset checks;9014 audio discovery checks;
1484 settings/catalogue/identity/XML checks;271 radio/GUI checks;4142 existing
calibration checks/1400 native ticks (47 old layouts, NOT new live evidence).
Compiled integration:124 protected calibration/adhesion/curving/rod/oil methods
unchanged from1.22.4,278 assertions including absent recorders and native cleanup.
Compact fixture native force-radius replay within0.1mm after distinguishing
native0.712 force datum from wheel-transform height0.7121296. This is longitudinal
clearance/math only, not complete3D clipping proof or Unity constraint execution.
No listening or in-game1.23 tests. New layout/switching/LOD/oil/refill/uncouple/
save/reload and unsupported tender behaviour need manual testing. No true series
compound enabled. RUNTIME_TRIAL_TESTING.txt is the player/manual guide.

Desktop test ZIP prepared under RearrangedS282-B99-1.23.0-Test; build receipt is
canonical final DLL/ZIP hash. Installed1.22.3 remains untouched. No backup copies,
push, Nexus publication or S060 edits. Local source checkpoint recorded separately.

## Erie tender mechanical clanks audit - 22 Oct 2026

James asked specifically about driving-gear clanks. Confirmed gap:1.22.4 adds
chuffs/cocks but does not clone native ValveGearMechanism/Mechanism_Layered to
tender. Its native LayeredAudioPortReader uses steamEngine.CHUFF_FREQUENCY;
normal tender rolling/joint audio does not supply this. Existing audioEffects
boolean checks chuffs/cocks only. Next reuse that native subtree at tender
running gear, bind MiddleEngine frequency and existing cleanup; no physics
change needed. Do not blindly copy damaged/low-oil readers. Static audit only,
no runtime listening, source/build/package/install change. See evidence/erie-
tender-mechanism-audit-2026-10-22.md. Installed1.22.3;1.22.4 candidate unchanged.

## Presentation-first1.22.4 test - 22 Oct 2026

James prioritised audio/visuals/steam and requested existing driving physics be
preserved, or unavoidable realistic differences explained. Implemented an Erie
presentation gap: tender was omitted from extra chuff/cock audio while particles
already included it. Now one shared active stage list includes the tender once,
with own native ports. Chuff individual sources/loops are seated at tender rear
exhaust with original mixer/clips/curves/distances; LP profile reused. Cock audio
uses existing tender cylinder mapping. Coupling changes remove old audio before
refresh. Existing audio clone/reader cleanup retained. Tender diagnostic now
has audioEffects under measurements.tender (structural)/tractive.tender (power).

No driving/boiler/consumption code changed. All734 protected compiled simulation,
calibration and WheelRearranger methods except TenderEngineReport unchanged from
installed1.22.3. Existing105-method/279assertion check also passes. Native gates40
and graph/lifecycle48 checks confirm current PARALLEL fresh-steam aggregation,
including tender intake; tender torque/exhaust stay out of loco output sums.
No true compound physics enabled. Changing only boiler draw toHP while keeping
existing parallel torque would invent efficiency and is explicitly rejected.
AUDIO_STEAM_TEST_STATUS.txt explains starting pull, receiver buildup, steam use,
boiler recovery and blast changes unavoidable in a proper compound calculation;
adhesion/mass/curving/sanding/brakes are outside that future change.

IMPORTANT ROLE CORRECTION: MainEngine/native steamEngine is FRONT (LP position
on Mallets/Erie); added RearEngine is REAR (HP). MiddleEngine is Erie tender LP.
Earlier prototype README inverted the native names; corrected before integration.
Original physical LP/HP area ratio~2.066, preserved by common area reductions;
tender calibration separate. Existing accepted cylinders were not rescaled.
Prototype still offline, not part of shipping project. Simple31/32 and fictional
38/40 remain parallel; no implicit conversion based only on multiple engines.

Validation:2551 actual extracted seating/selection/reset checks,9014 discovery/
identity,91 profile checks,1483 compiled wiring/preservation assertions. Native
serialized S282 prefab: all3individual emitter parents and10loop refs are inside
owned SteamChuff subtree, full2424byte read; evidence under build/stage-audio-
inspection (no native data ships). Managed tests do not run Unity clone semantics,
audio playback, native Init/Deinit scene lifecycle or live coupler/LOD behaviour.
Tender clock remains loco-linked; independent slip RPM/handling unchanged.

Clean1.22.4 build0warnings/errors. DLL E196238C81145893FEAE29898DF1C64D3D784BFFEDF9EF936F04E1CE4245B147.
Desktop1.22.4-Test ZIP prepared; final hash in build receipt. NOT installed or
published. Installed1.22.3 DLL stillD48E09D26260626444CB7EEDA8C8A35840FF9A85A1FEE49FC858302F1E0AF4E1.
No backups, source/audio assets untouched, no new dependency. Next: game Erie
chuffs/loops/cocks, coupling and switching test; subsequent compound calibration
and native lifecycle integration only with explicit performance comparison.

## Compound work started - 22 Oct 2026

James asked to start from the compound study handoff. First implementation slice
is OFFLINE in experiments/compound, not compiled into runtime. Uses existing
S060 receiver model as read-only reference; no changes to S060 or installedS282.
See experiments/compound/README.md for current roles, native gap and next work.
Prototype adds independent HP/LP speeds, shared receiver supply for Erie's two LP
consumers, no priority by iteration, fresh-steam budget and conservation ledgers.
Validate.CompoundPrototype.ps1:102027 arithmetic/conservation/pressure checks;
4000legacy-reference steps. Initial blocked-LP pressure overshoot16.03871bar was
found and corrected by bounding transferred parcels, not deleting energy. Default
bounded mode now reaches15.999862bar on16bar supply; all transfers checked for
pressure-gradient reversal. Legacy-reference mode disables only that correction
to verify provenance; never propose it as a gameplay setting. readyForRuntime=false
until native binding/calibration/lifecycle/performance work. Parameters are S060 test
values, not arrangement calibration; reduction-only cylinder limits retained.
Confirmed native torque subtracts fixed1bar; no backpressure input. Rewiring alone
is insufficient; owned native stage subclass is the existing local precedent.
No game build/install/release or change to1.22.3. No backups. User did not request
agents for this new compound slice; none spawned.

## 1.22.3 test installed at James's request - 22 Oct 2026

User confirmed game closed; process check also found no DerailValley process.
Installed the exact previously audited TEST ZIP, without rebuilding or changing
source. All43package files hash-verified against archive. All40non-package files
(including settings, existing diagnostics and cache) hash-verified unchanged.
No backups/rollbacks created. No game launch/runtime acceptance claimed.

Installed1.22.3 DLL D48E09D26260626444CB7EEDA8C8A35840FF9A85A1FEE49FC858302F1E0AF4E1.
Source ZIP SHA256 65ADB18E1030DF8EABAC28D2D14FD6342F435546C41AD7D50FD4F5D8260C2ABD.
Install timestampUTC 2026-10-22T21:31:07.402235+00:00.
Receipts: build/runtime-trial-1.22.3/installation-receipt.json and build-receipt.json;
desktop build receipt updated too. Current1.22.3 TEST supersedes installed1.22.2.
Still needs visual/curve testing, especially Prairie adapter finish/profile and
articulated front pipe joints. Run structural diagnostic normally. Prior offline
coverage/limits below remain applicable; installation is not new runtime proof.

Earlier build and source notes follow:

## Source-derived steam duct extensions - 22 Oct 2026

Local source checkpoint: codex/steam-duct-polish-20261022, child of reviewed
ec68558. Separate index; active b99-port branch/index unchanged. No push.

Current candidate is still1.22.3 TEST, replacing the earlier uninstalled guide-only
candidate. Installed1.22.2 remains untouched. No game launch, installation,
publication, full backup or rollback copy performed. Clean rebuild0warnings/
0errors. DLL D48E09D26260626444CB7EEDA8C8A35840FF9A85A1FEE49FC858302F1E0AF4E1.
Desktop test ZIP is in RearrangedS282-B99-1.22.3-Test; its build-receipt.json gives
final ZIP hash. GEOMETRY_TEST_STATUS.txt included in package.

Scope correction: extend the original set with matching pieces rather than
replace everything from scratch. Three agents implemented/audited original
profiles, native/casing fit and actual production surface/lifecycle coverage.
Original steam_transfer and steam_manifold OBJ exports byte-identical. New kit
uses original13-sided shaft and complete collars/topology/UVs/materials.
SteamDuctMesh adds analytic round elbows, lengths, reducers and a compact Prairie
cast offset retaining the actual35-corner native cut. Its new skin uses ONLY the
transfer shaft atlas island; cross-island UV blending was caught and corrected.
Native prefix UVs retained. No new assembly/dependency or physics system.

Native rigid branches now seat the complete sampled cut boundary in the actual
normalized casing, rather than checking centres alone. On3/4/5/34, compact only
the lower-collar outer annulus from~.144m to~.108m radius in private copies. Shaft,
upper flare and topology unchanged. Actual air-output contacts13LOD0/7LOD1 become
zero. Prairie adapter uses.22m outward bow, both ends fixed. It is a compact cast
transition, not a constant-bore swept elbow: its finish/profile need game review.
Articulated front/middle branches mount to measured outboard casing faces, with
round elbows outside steps/board. Original rear manifolds retained; headers stay
horizontal. New transfer shafts have a minimum.8native width; original boiler
castings retain their authored varying profile. Erie tender supply remains
internal; accepted exhaust/effects and stage sound unchanged.

Final evidence:
- build/steam-duct-candidate-4:44real generated-pipe arrangements strict success.
  Native body/tanks/steps/compressor LOD0+LOD1 have zero neutral surface crossings.
  All76neutral/stress poses generate; all +/-6degree poses clear. +/-12degree
  synthetic poses have front-engine/body contacts:8uniqueLOD0/7LOD1. Reconstructed
  pivots/stress are not measurements of real game curves.
- clearance-neutral.json:2040actual native cylinder-side clip queries in neutral
  frames,zero contacts. Custom Franklin and stretched main rods excluded.
- source-receipt.json binds stable before/after source hashes; compiled IL confirms
  final.22adapter bow. Earliercandidate3 read an in-flight.12edit; superseded.
- SteamDuctMesh113958checks. Native fit/collar3140checks, both sides/nativeLODs.
- build/steam-duct-actor-validation:8488checks,39reachable synthetic primary,
  33rigid stress,22explicit unsupported geometric rejections with cleanup, and
  two simultaneous live instance stand-ins. Maximum endpoint gap.0308mm.
- build/steam-duct-protected:279checks/105protected compiled methods unchanged
  against installed1.22.2. Compiled steam ownership19checks; retained Erie66.

Remaining: game materials, Prairie side profile, near/far and tight curves;
continuous/native Franklin/main-rod solid inspection. Surface checks do not
establish coplanar/nested overlap or minimum gap. No whole-locomotive all-clear.
Native mesh/capture evidence stays local/ignored, never in test ZIP.

A documentation-only UTF-8 write error truncated this handover during update.
Saved history below was recovered from source commit ec68558; the intervening
uninstalled guide-only audit is summarized next and remains in GEOMETRY_AUDIT.md.
No runtime source or package was affected. This note prevents treating recovered
historical investigation wording as the current implementation.

## Intervening guide-only1.22.3 audit (superseded candidate)

Existing common-frame helper applied to4-coupled, remaining6-coupled and12-coupled
rigids, including hidden gear; authored crosshead scale restored. Ten-wheeler/
Mogul/12-driver full casing treatment follows actual piston/valve axes. Wheels
and physics not moved. Credit corrected to discorddraconequus. Earlier DLL
13B18CEF0EC7EEF03A8D6ED1FCCA97B1AB6C3826EB7CD0B57C9324D95B3C1526 was built but
not installed; it had NO pipe runtime changes and is superseded by current test.

Prior build/geometry-audit evidence: guide170517checks/384addedposes+96old;
casing369141checks/1536poses; connecting30cases/2880poses/.364mm main joint,
.318mm radius joint; coupling180cases;4-14-4 96poses/.0342mm endpoint delta;
Prairie672poses/2016cups; old pipe1570599shapechecks/2208actorchecks (endpoint
only, not a clearance result); compiled catalogue152;105protected unchanged;
rod timing6720poses. Initial actual-body/air audits then found pipe contacts
requiring this new duct pass. Original initial audit/detail remains in
GEOMETRY_AUDIT.md. No previous live1.22.3 acceptance was claimed.

---

Recovered saved history follows (older entries are historical):

## Twelve-driver valve report investigated - 22 Oct 2026

User requested latest logs/diagnostics review and supplied dark screenshot of
short valve rod and guide/support overlap on twelve wheel sets. Investigation
only; no runtime source, installed build or release ZIP changed. Installed1.22.2
is published on Nexus per James. Credit spelling correction from James:
discorddraconequus (packaged credit still needs correcting in next build).

Latest Player/Player-prev load1.22.2; no exception stack references S282. Other
exceptions: AI Traffic radio registration, native HUD coupling/deletion during
unload, native shop/Bolt destruction, DoubleTrack saving. No evidence tying these
to the reported rod defect. Latest structuralJSON1.22.0 all47pass including4
x-12-x and oilcup counts; file22Oct01:08BST. No newer power files than21Oct.

Shared Show12SmallDrivers: driveYZ.84,Z+1.1; static guides onlyY-.1;
cylinders movedY-.11 without corresponding Y/Z frame normalization. Existing
Pacific/Hudson/Prairie common-frame fitting alignment not applied here. Native
mesh/clip samples show valve axis approx.1055m below native casing valve bore;
spindle forward tip approx.178-.202m behind unchanged rear-face datum8.84 at
sampled extremes. Static guide differs from drive map by -.077..+.154m Y and
+.031..+.327m Z over its mesh. This supports the image mechanically; no live
reproduction or continuous-motion clearance validation yet. Four shared cases:
0-12-0,2-12-0,2-12-2,4-12-2. Do not hide gear or blindly stretch whole rods.
Use existing fitting affine/retarget tools for targeted presentation fix; maintain
physics/driver layout and test both sides, LODs, cutoff/reverse/rotation plus
restore/isolation. BUG-12-GEAR recorded in TODO.md. Native-data findings saved
build/twelve-driver-valve-review/findings.json.

---
## Stage sound tuning 1.22.2 installed and Nexus Beta prepared - 22 Oct 2026

James explicitly confirmed Erie visuals in game, then requested stage sound tuning
for this release; defer other Verde/compound work. Correct acceptance notes: Erie
visuals/placement accepted; new audio has offline checks, not a listening run.

ChuffStageTuning applies a modest .85 above-atmosphere pressure-span transform to
front primary reader thresholds and individual-chuff volume curve for IDs37,41,
42,43,45,46 (Mallets/Erie). Authored sample values/weights/wraps retained; tangent
slopes adjusted. Rear native HP-style profile unchanged; simple duplexes and
fictional triplexes untouched. Original front thresholds/curve reference restored
in ChuffBinding.Remove on arrangement change/deinit. Uses authored prefab curve,
so no cumulative transformations. No chuff indexing, event clock, cylinder count,
native high-speed-loop, steam/traction or other physics changes. No new UI.
Erie tender audio remains on existing path; this is not true series compounding.

Credit reaconequus for Verde stage-sound reference in release-copy/CREDITS.txt;
Build.RuntimeTrial now copies current credits to packages. No Verde assets/code
bundled. Read-only Verde evidence build/verde-inspection: four native engines
HP4/MP4/LP8 cylinders plus booster2; pressure incorrectly linked to quality in
supplied graph, downstream pressure feeds and boiler ENGINE demand absent; no
runtime verdict. Reuse stage concepts, do not transplant graph. Native receiver/
flow/backpressure limits, final-stage blast and bounded feedwater heating deferred.

Release build0warnings/errors;91 actual profile helper checks with managed curve
metadata stand-ins;9014 audio-discovery assertions;152 compiled articulation;
279 protected checks/105 physics-retarget methods unchanged from1.22.1.
Installed with game closed;42 package files hash verified, no backups.
DLL224CB62AD0583B406EC2DA452D1FD3BE5C6A6E8F10F92CC027A9E3B9F3751D90.
Nexus ZIP Desktop/Derail Valley Mods/RearrangedS282-B99-1.22.2-Beta/
RearrangedS282-B99-1.22.2-Beta.zip SHA256:
0F4B9F3FC99DDDBB0FB567D32F54BB125E1125592E576D8CA1A71653B075696E.
44 files; archived runtime matches installed/test; licence/notices retained and
technique credit added. Clean docs release-copy/1.22.2-Beta record confirmation
and new sound-listening caveat. No settings/diagnostic captures/PDB or new
arrangements. Not published. Old release ZIPs remain unchanged.

---
## Nexus-ready 1.22.1 Beta ZIP prepared - 22 Oct 2026
User requested release ZIP. Desktop/Derail Valley Mods/RearrangedS282-B99-1.22.1-Beta/
RearrangedS282-B99-1.22.1-Beta.zip. 44 files; archived DLL matches installed/test
7DB617848F714D67B88B36B71509E18CC93E290E18390F37C9A2B30420920311.
ZIP SHA256 A66A46C4246DDFAFD7D149EE24CA3812319C4A15D2AF35E997DE6E56A8206221.
Clean release docs include new Erie visuals and explicit unconfirmed live1.22.1
acceptance; licences/credits/notices retained. Info display name B99 Beta only;
version1.22.1/ID/dependencies unchanged. No settings/diagnostic captures/PDB.
Docs and receipt release-copy/1.22.1-Beta. No publishing or installed-file changes.

---
## Erie clearance correction 1.22.1 installed - 22 Oct 2026

User asked to check coal/hatch clearance and, if needed, relocate exhaust to rear
side wall with bands and lower elbow. Native geometry showed old deck position
crossed upper rear ladder/rung area. Relocated x-1.72/z-6.95 outside rear left tank
wall, forward of ladder. Upright y2.65..4.05, inlet starts inside tank x-1.05/y1.8.
Broad lower elbow uses existing sweep and fold-radius rejection. Two original
transfer-pipe collar sections at y2.85/3.4 act as mounting bands. Original mesh,
UV and native shared material reused, all pieces combined into one owned mesh/
renderer retaining tender LOD cleanup. Outlet/white plume follows new mouth.
No simulation, audio, other layouts or existing effect-reader changes this turn.

Native mesh evidence now explicitly includes coal5 (full pile), tender body and
water hatch transformed through authored prefab hierarchy. Conservative checks:
coal rearward separation1.813m; pipe AABB clear of whole hatch rotation sphere
by.310m; rear ladder forward-rail gap.216m with5cm rail thickness allowance.
Intended lower inlet penetration and mounting band/tank contact are excluded.
Local evidence: build/runtime-trial-1.22.1/erie-validation/native-clearance.json,
clearance-preview.png and exhaust-mesh.json; extraction script under
 evidence/steam-pipe-tests/inspect_erie_clearance.py. Native mesh data not shipped.

Validation:66 presentation geometry/ownership checks/5cleanup cycles; production
Release build0warnings0errors;279 protected checks/105physics-retarget compiled
methods unchanged from1.22.0. Offline only; game appearance still pending.
Installed with game closed;42 package files verified, settings/diagnostics retained,
no backups. DLL7DB617848F714D67B88B36B71509E18CC93E290E18390F37C9A2B30420920311.
TestZIP Desktop/Derail Valley Mods/RearrangedS282-B99-1.22.1-Test/
RearrangedS282-B99-1.22.1-Test.zip
SHA2566BDA141A479A6B194EA97EB40A46CB33260264F4B4150FEB2732635011878A2E.
Build/install receipts under build/runtime-trial-1.22.1. Not published.

---
## Erie visuals 1.22.0 installed - 22 Oct 2026

James prioritized tender visual effects; keep current physics/audio and temporary
proportion controls. Added ErieTenderPresentation: rear-left .32 m shaft uses the
original steam_transfer mesh/profile/UV and native steam-pipe shared material.
Measured native S282B geometry (local evidence build/erie-native-geometry.txt):
body x +/-1.56, rear z -8.09, max y3.52; pipe x-.95/z-7.45, y2.25..4.05.
Native tender LOD entries retain their settings/foreign renderers during cleanup.

ArticulatedEffects now adds tender only to particle secondaries (existing audio
path unchanged). Native cylinder-cock reader and water/crack particle readers use
middle-engine ports and mapped tender cylinder positions/orientation. Separate
native SteamSmokeParticlePortReader drives white SteamSafetyRelease/SteamExhaust
particles at pipe mouth from middle engine chuff/pressure. Empty ember parent;
normal-zero broken-boiler state also used for native fire gate to prevent idle
coal smoke without adding or changing simulation ports. No tender pulses sent
to loco chimney. Pressure remains native; tender exhaust mass flow still excluded
from locomotive blast as before. Water drips are conditional, not decorative.

Attach/detach resets native particle readers; tender disable/delete callbacks
remove event subscriptions before leaves vanish. Re-enable is detected by existing
half-second effect refresh. Fixed destroyed-tender wrapper vs real-null transition.
Owned mesh/LOD cleanup idempotent; no per-frame geometry generation.

Validation: Release build zero warnings/errors; 56 actual presentation geometry/
ownership checks across five cycles using existing managed fixture and original
mesh; 9014 existing audio discovery checks; 152 compiled articulation integration;
279 protected integration checks confirm105 compiled physics/retarget methods
unchanged from1.21.2. Unity native Init/rendering/particle behaviour not executed.
Live placement, plume strength, cylinder cocks/drips, near/far, uncouple/reconnect,
save/reload still need James. Slip sparks/brake heat and grip/sanding deferred.
Tender tractive/curve report now has exhaustOutlet and visualEffects readiness.

Installed while game closed;42 package files hash verified,40 non-package files
preserved; no backups. DLL SHA256:
C7AB065425BEC4E7491CC44B127370D34D79A7DF18343715FBCB7DF1876C9B66
Test ZIP Desktop/Derail Valley Mods/RearrangedS282-B99-1.22.0-Test/
RearrangedS282-B99-1.22.0-Test.zip SHA256:
C27FA48A18F2990C9941BC5D9D55A41EE10D9890196B637CA17F46C2A0E66EF8
Receipts/manifests build/runtime-trial-1.22.0. Previous Nexus-ready1.21.2 Beta
archive remains unchanged;1.22 is a test build, not published. No new arrangements.

---
## Nexus-ready 1.21.2 Beta archive prepared - 22 Oct 2026

User requested Nexus-ready ZIP and feature/arrangement summary. Created Desktop/
Derail Valley Mods/RearrangedS282-B99-1.21.2-Beta/RearrangedS282-B99-1.21.2-Beta.zip.
SHA256 AA65DB7014E7D060B6ADB49C93C80D7097696A069B47169E5CBFD5779A030ED3.
44files;32runtime/assets/licence/credit files verified byte-identical to testZIP.
DLL matches installed1.21.2 exactly. Info DisplayName changed toB99 Beta only;
version/ID/requirements/loadorder retained. No settings/diagnostics/PDB/source.
Clean release README/manual/status/UMM options/changelog/physics notes replace
historical trial-note stacks. Older FOA_REFINEMENT/PHYSICS_BASELINE reports omitted
from release archive; current FOA_BALANCE retained. Original licence/notices/credits
untouched. Documentation/receipt copies release-copy/1.21.2-Beta.

No new arrangements since1.13.6: still47. New capabilities: temporary rod pitch/
reach tuning, UMM reapply/reset, expanded power/TE diagnostics. Other changes are
pipe polish, rear fifth-driver fix, grip/cylinder/FoA balancing and structuraltest
correction. Missing frontcrank still unconfirmed; independent tenderfeature absent.
Latest balance offline validated only; current liveall47 evidence1.20. Package is
Beta, not newly proven in game. No publishing or installedfile change this turn.

---

## 1.21.2 BALANCE CORRECTION INSTALLED - 22 Oct 2026

User challenged graph showing every calibrated FoA above stock, then explicitly
requested new balancing pass with2-2-2 kept on special measures. Corrected code:
remove stockRatio clamp from CylinderCalibration.Apply, not just change labels.
Original-cylinder reduction-only maximum remains. Existing smooth target changed
base4.2->3.9, driven-axle term .1*(n-4)[-.2,.3] -> .2*(n-4)[-.4,.6], total bounds
3.85-4.55 ->3.3-4.65. Diameter/load/cylinder allowances unchanged. No per-ID tier.
Actual calibrated outcomes8locomotive layouts below stock,36above. Stock3.668
unchanged.4-4-0 actual3.427/108.8kN;4-6-0 3.657/160.9kN;2-12-2 4.297/206.7kN.
2-2-2 dimensions, non-colliding prototype and calibration bypass untouched;
exclude it from balance success/usability, not the sole slippery example.

Existing native tests4142checks/132layoutmasscases/1400nativeTicks pass, now
including assertions that calibrated results exist on BOTH sides of stock.
Compiled279checks confirm105protectedmethodsunchanged; physics wiring11pass;
production build0warnings/errors. No boiler/flow/adhesion/sand/brake/curve/wheel/
geometry changes. Scope remains small calibration correction, no valve-gear fix
or independent tender feature. FOA_BALANCE.md and build/runtime-trial-1.21.2/
foa-comparison.csv/json have all47 plus tender, stock deltas and change flags.

Installed while game closed,42packagefiles verified,14settings/diagnostic files
unchanged, no backup. ActualDLL1.21.2.0. InstalledUTC2026-10-21T23:16:10.160403
(local22Oct). Receipt build/runtime-trial-1.21.2/installation-receipt.json;
package-files.json beside it. DLL:
2E0C783E38BF2DC773C37B1D67AC4A55D5B01D343701EDC51909D18E94D6BEA5
ZIP91558E920F7EEC7C76ED4CF700574FF4BD4D4FE7C4177054DF15702B5B76266B
Desktop/Derail Valley Mods/RearrangedS282-B99-1.21.2-Test. No publish/push/commit.
No live1.21.2 claim; originalcandidate documents inside ZIP are build-time notes.

---

## 1.21.1 INSTALLED at user request - 21 Oct 2026

User explicitly requested installation and changelog since public1.13.6.
Game process absent before copying. Installed exact prepared1.21.1 ZIP, no rebuild
or backup. All41 packaged files hash-verified;14 existing settings/diagnostic files
preserved exactly. Actual installed assembly1.21.1.0. Receipt:
build/runtime-trial-1.21.1/installation-receipt.json and package-files.json.
Installed UTC2026-10-21T22:57:00.235003+00:00.
DLL2BA2572DAF4687C9A78D8FB874424712D06E7E3F5D2DC04B156E234CAAEC3573
ZIP8FE309719D7FCB64CA0857CBF79EB581B11085698F4150D7595921DD2059F17A
ZIP Desktop/Derail Valley Mods/RearrangedS282-B99-1.21.1-Test.
No live1.21.1 claim; current all47/power acceptance is1.20. Original candidate
reports inside immutable ZIP are build-time records, superseded by this receipt.

Prepared laconic unpunctuated changelog:
release-copy/1.21.1-Test/CHANGELOG.txt and beside desktopZIP. Since1.13.6 only:
rod/spacing tuning+reapply, pipes, rear Mallet follower, calculated grip/cylinders,
FoA individuality, tender cylinder sizing and diagnostics/test correction.
Does NOT claim missing front crank fixed, independent powered tenders implemented,
new arrangements since1.13.6 or new chuff support. Those features either remain
pending or were already in1.13.6. No publish/push/release made.

---

## 1.21.1 TARGETED FoA REFINEMENT - BUILT, NOT INSTALLED

Latest request explicitly limits scope to modest machinery-sensitive FoA
variation. Do not pursue unrelated crank/tender-feature/geometry work here.
Universal4.4 replaced with pure bounded rule in existing TractiveEffortModel:
4.2 + .1*(poweredAxles-4) bounded[-.2,.3]
 - .15*(diameter/1.424-1) bounded[-.1,.1]
 + .1*(effectiveAxleLoad/20000-1) bounded[-.05,.05]
 + .025*(cylinders-2) bounded[0,.1], result bounded[3.85,4.55].
These are explicit modest design allowances, not historical measurements or
performance tiers. No per-arrangement bonus/ID table. Multiple cylinders retain
shared phase/slip, with no assumed smoothing advantage. Ceiling only; naturally
weaker original engines stay weak. Reduction-only original dimensions retained.
Stock bypass completely unchanged. 124t shares and every boiler/flow/geometry/
curving/adhesion/sand/brake system unchanged. Erie tender uses same rule with its
own four driven axles/two cylinders/40t adhesive mass; clamp untouched.

4-4-0 actual FoA4.40->3.927, nominal84.7->94.9kN(+12%). 2-12-2 FoA4.397,
201.8->202.0kN(+.1%). Mogul+8.4%,4-14-4-2.3%,larger Mallet/Erie bodies-3.3%.
Tender4.4->4.194,89.2->93.6kN(+4.9%). Actual calibrated FoA range3.927-5.502;
upper actual values are unboosted original capacity, not forced targets.
Only4-4-0/4-4-2/4-4-4/2-4-0 exceed10% change; flagged. No conventional layout
below40% stock nominal TE in simple usability screen; NOT tonnage guarantee.
Prototype and unpowered novelty remain explicitly excluded from usability claims.

FOA_REFINEMENT.md has concise all47 plus tender before/after table, formulas,
flags and limitations. CSV/JSON under build/runtime-trial-1.21.1. Prior1.21.0
validation/table preserved as comparison input. No new extensive tooling/graphs.
Existing native fixture reused:4140checks/132layoutmasscases/1400nativeTicks pass;
build0warnings/errors; compiled279checks/105protectedmethodsunchanged vs1.21.0;
physics wiring11pass. Stock/resource independence/restore/isolation remain tested.
DLL2BA2572DAF4687C9A78D8FB874424712D06E7E3F5D2DC04B156E234CAAEC3573
No installation, no publishing, no backups. Installed1.20 remains untouched.

---

## Public 1.13.6 reports and options documentation - 21 Oct 2026

Latest user wants missing front duplex valve-gear crank investigated, asks whether
UMM settings explain it, will obtain screenshot, and requests mod-page options
section. No arrangement/screenshot supplied. Do NOT mark the crank fixed without
reproduction. No production changes for these reports yet. 1.21 candidate remains
built but uninstalled, 1.20 installed.

Settings are confirmed from compiled 1.13.6 metadata, not merely current labels.
Rigid x-4-x and x-6-x visibility defaults off; polished Pacific/Hudson override
on; x-8/10/12 on; high-speed option off. High-speed label says2-8-2 but affects all
three high-speed2-8-x layouts. Prairie uses polishing switch. These are appearance
settings, not power toggles. Articulated methods do not read these family flags;
ResetToFourCoupled explicitly shows native gear, Franklin config enables its
return-crank base. This does not prove correct visual placement/rendering. Shared
ConfigureEngineDrive, ApplyArticulatedGeometry, ConfigureLongEngine and reset
compiled identical between1.13.6 and1.21; cannot claim later builds fixed report.

Prepared release-copy/1.13.6-Beta/UMM_OPTIONS_FOR_NEXUS.md for page. Distinguishes
public controls from later private tuning/reapply buttons. No page publication,
archive replacement, install or code workaround. Need reporter layout/side/near-
far/switch history and screenshot before specific missing-crank cause/fix.

Second report is feature request, not defect: independent powered tender with
2-6-6-0 host is feasible using third existing engine. Current gating/geometry is
Erie-only. Requires host-independent tender mesh construction/activation, saved
choice, calibration and cleanup; three-engine locomotive triplexes have no spare
engine. Not implemented; no new option promised as already available. TODO entries
BUG-CRANK and FEATURE-TENDER record scope.

---

## 1.21.0 CALCULATED PHYSICS CANDIDATE - NOT INSTALLED

James requested a stronger deterministic/historical-informed physics pass now,
without requiring further game tests first, followed by TE metric comparison.
Implemented and built 1.21.0; installed mod remains 1.20.0. No backups/publish/push.
No agents requested for this physics turn; none spawned.

Current adhesion weights reviewed and retained as explicit effective-load
assumptions (not reconstructed historical axle weights). Selection now evaluates
shares at fixed 124 t working design mass, so water at selection cannot alter
rated shares/cylinder dimensions. Native actual mass, slip/weather/sanding remain.
New reduction-only cylinder ceiling: nominal TE at .85 of14bar gauge pressure,
minimum nominal factor of adhesion4.4. Convert to actual B99 reference mean force
at15bar absolute/.75cutoff/300C (~1.133 nominal-to-mean ratio), giving dry pull/grip
reference ceiling~1.073 rather than old stock1.287. Stock unchanged. Original
cylinder capacities are upper bounds; no boost, no forced speed cap, no flow/
chest/boiler/mass/geometry changes. Exact instance restore retained.
Erie tender cylinders calibrated independently at50t/.8 driven share; nominal TE
150.9 ->89.2kN. Existing tender force clamp remains. It ALREADY reads TotalMass;
prior wording implying resource mass was wholly absent was too broad. Need future
rail/sand consistency and mass-refresh checks, not a duplicate mass system.

Most ceiling-limited arrangements reduce nominal cylinder TE16.6%; nine nonstock
arrangements change less than5%. These are cylinder nominal figures, not a claim
usable drawbar force drops the same amount. One boiler means extra cylinders do
not create extra sustained power. Historical-style FA4.4 is a stated design
choice, not unique truth derived from Whyte notation. Short layouts on124t body
still use virtual adhesive weight; cannot claim realistic physical spring loads.

Evidence: PHYSICS_BASELINE.md includes source citations and explicit assumptions.
Native fixture4140checks,132layout/masscases,1400nativeTicks; independent nominal
formula, resource-independent dimensions, tender calibration, isolation and exact
restore pass. Build0warnings/errors. Protected integration279checks/105methods
unchanged vs1.20; articulated152, pipe19, steamvalidator12, tractive74, physics11.
Source changed only calibration, rated-share selection, diagnostic/structural
reference comparison and report tooling/docs. New legacy stockReferenceRatioCeiling
field aliases designReferencePullGripCeiling; referenceGripN now at ratedmass,
not currentmass. Structural compares measured reference force to rated designgrip.

Comparison generator Tools/PhysicsBaseline.py consumes actual native validation
and1.20 structural input. build/runtime-trial-1.21.0/baseline contains all47 plus
separateErie tender row in CSV/JSON/Markdown and all/selected PNGplots. Giant
prototype excluded from plots. Uses Pillow fallback because matplotlib absent;
workspace-only pip attempt failed DNS, no dependency installed. Plot inspected.
Nominal/mean/drygrip explicitly separated; not speed curves or sustained ratings.

Candidate DLL01556EEBC037201FB45AB7BAA403B977653B9B7C36C7B59583F25F313AAA4135
ZIP on Desktop/Derail Valley Mods/RearrangedS282-B99-1.21.0-Test.
ZIP SHA256 08416BB3C5D74645AD1A6DEE3DEE8BB45DF61A07F6804B5B312B121CDACA0C4F
Package 40 files verified; package-verification.json beside build receipt. Build receipt
beside ZIP and build/runtime-trial-1.21.0. NOT installed. Next install when requested;
no need another power-test round to generate the calculated baseline. Later normal
smoke/balance verification remains distinct from deterministic calculation.

---

## 1.20.0 LIVE REVIEW - 21 Oct 2026, after power tests

Latest structural report: actual 1.20.0.0, L-022/control L-063, all 47 PASS.
Oilcup counts/calibration consistent; pipe maximum gap .0354 mm. Rear Mallet
followers bound on both sides with zero structural rotation gap. Five new power
files contain 9,698 rows, actual 1.20; powered stock/Prairie/Mallet/4-14-4/Erie.
No simulation faults or wheel/crank RPM discrepancy. Both Mallet engines and all
three Erie engines contribute, Erie tender classified separately and applies up
to its 96 kN clamp. Background derailed L-050 is not tested Erie L-023. Sanding
explains increased native grip; do not rebalance using instantaneous peaks.
Captures are 66-92 seconds, powered spans 36-60 seconds, not sustained-load ratings.
No powered 2-4-0 capture found. No S282 error entries or rolling-audio exception
flood in current log; S060 saved selection/AI Traffic/scene errors separate.

James accepts visible wheels/layouts, rods/reversers, oilcups and almost-perfect
pipes. Record visual acceptance without claiming explicit LOD/refill/pool/save
coverage. TODO updated. Full review and machine summaries:
build/runtime-review-1.20.0/REVIEW.md, power-summary.json, powered-detail.json.
Next substantive work: Erie presentation/resource/grip consistency; bake accepted
positions/remove granular tuning; sustained loaded/high-speed balance and targeted
lifecycle checks. Curve derail risk and giant prototype remain deferred.
No runtime code/install changed during review. Existing 1.20 remains installed.

---

## 1.20.0 INSTALLED - pipe polish and structural test correction, 21 Oct 2026

Request: review short 4-14-4 power/logs; smooth pipes, compact two-cylinder routes,
minimum width, internal Erie tender LP supply. Accepted wheels/layouts retained.
Three agents used as explicitly requested. No publishing, push, commit or backups.

Install: game absent at install guard. All 39 packaged files match installed;
six existing settings/diagnostic files unchanged. Actual assembly 1.20.0.0.
Receipt: build/runtime-trial-1.20.0/installation-receipt.json; package-files.json.
DLL F2C3F7633ADEFED810F72EE6BED1DA45DD5BF9FC47A7565D39A3052DBB78040D
ZIP EF11D30CE8B37D7CE4D47304F4A22B49423877B1303025657DC60A677D4DF2CB
ZIP: Desktop/Derail Valley Mods/RearrangedS282-B99-1.20.0-Test.
Installed UTC 2026-10-21T20:09:25.3215317Z. Live 1.20 NOT yet tested.

Pipe cause: original transfer shaft had four rings. Bending its long triangles
produced angular wedges; extra sweep samples alone cannot fix that. New 193-ring
shaft retains original 13-sided profile, source seam/UV island/collars. Radius-aware
bend rejection prevents inner folds. Compact rigid routes replace long-header
fallback. Original inclined boiler fittings retained exactly, lower neck clipped
and capped with measured joint. Safe curve or plain sleeve in cramped space.
Articulated feed retains same fitting. Last manifold faces forward; header ends
at its inlet, front bore capped. Width >= widest side manifold, floor .8 native
(shaft 162.56 mm, transfer flange 223.52 mm). Erie exterior routing locomotive-only;
tender LP internal. Native articulation/LOD retained. Owned meshes released,
original sources/LOD arrays restored. Zero new colliders. No accepted wheel,
layout, cylinder position or physics changes. Original MIT meshes/materials reused.

Diagnostics: actual latest session 1.18.0.0, although disk had 1.19 installed.
Reason unconfirmed. UMM now shows actual RUNNING version; restart and confirm 1.20.
Latest structural all 47 stopped at false pipe-error predicate: Newtonsoft typed
string null has Type.String although it serializes null. Fixed actual value test
and explicit null emission. Real errors, counts/colliders/nonfinite/negative/>3 mm
still rejected. Existing socket counts/gaps consistent; stock restore matched.
Later checks did NOT run; do not claim all-47 or PHYS acceptance. Regression with
actual Newtonsoft: 12 checks pass.

Power: power-20261021-190721-749.jsonl, 239 samples/31.305 s, L-074 4-14-4 on 1.18.
Powered 82 samples, 3.08-14.49 s, 46.84-63.16 km/h, peak 101.609 kN. Wheel/crank RPM
agree, slip 0, no steam simulation fault, nonzero consumption and water mass decrease.
Derail 20.70 s at 64.93 km/h; later 84.29 km/h excluded. Useful brief RPM/consumption
evidence, NOT sustained rating or 1.19 calibration validation.
Separate unresolved fault: 17,703 rolling-audio IndexOutOfRange exceptions in
patched CarRollingAudioModule. First after S060/Meyer entry before first S282
structural conversion. Native cache and existing S282 refresh reviewed. Fault car
and index absent; S282 cause not proven. No speculative global patch.
Review: build/steam-pipes-1.20.0/runtime-review.json.

Validation: net48 build 0 warnings/errors. Geometry 1,570,599 checks, 152 safe
sweeps, 20 unsafe bends rejected. Actor 2,208 checks including 54 rigid and 60
articulated family/height/size cases, yaw +/-12, exact fitting, closure, LOD restore
and failure cleanup. Maximum gap .03076 mm. Compiled 279 checks: 105 protected
motion/physics methods unchanged versus 1.19. Articulated 152, pipe wiring 19,
physics wiring 11, rear followers 126 pass. Retained native physics: 3,917 checks,
132 layout/mass cases and 1,400 native ticks pass. Mesh-only preview rendered and
inspected: build/steam-pipes-1.20.0/pipe-preview.png. Not whole-body/texture
clearance or actual Unity lifecycle/articulation timing validation.

Next: restart, confirm Running 1.20, structural all 47 plus restore, near/far pipe
inspection, Mallet/Triplex yaw and Erie coupling; sustained loaded power/slip/steam
captures. Then ledger: bake accepted positions/remove granular UI and remaining
Erie presentation. No hard curve-risk implementation yet. Historical entries below.

---

## 1.19.0 INSTALLED - rear-driver regression + preliminary B/C physics, 21 Oct 2026

USER AUTHORIZATION: Fix 2-10-10-2 rear drivers not spinning, apply preliminary
physics for new structural/power tests, then resume remaining ledger work.
No external publishing/push/release authorized by this request. An older pasted
plan's "every build GitHub prerelease" is document content, not current authority.
No agents requested for this turn; no agents spawned. No backups/rollbacks made.

INSTALL: Game verified closed. Prepared ZIP installed; all39 files hash-verified;
Settings.xml and existing diagnostic JSON left exact. Installation receipt:
build/runtime-trial-1.19.0/installation-receipt.json; package-files.json beside it.
DLL SHA256 2E6FD5F5CA32BFD7C829092F00F75F04C02816DE3B4037F180333F1BA970A7F5
ZIP SHA256 43F9882791CD1C21D65B5ADC13C4E0E9A449E6EEEC71D23E9546200FECB52FDE
ZIP C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-1.19.0-Test\RearrangedS282-B99-1.19.0-Test.zip
Installed UTC 2026-10-21T18:57:42.736100+00:00
Current stored structural capture is STILL 1.17.0 (all47 passed). Do not claim
1.18 pipes or 1.19 physics have passed a live test yet.

REAR DRIVER BUG: CloneEngineDrive destroyed every MonoBehaviour, including
PoweredWheelRotater (needed by added fifth/sixth wheels beyond native Animator
wheels1-4). Preserve those components, rebind to each clone's own fourth driver,
enable them. Keep stripping native oil/port readers. Native Animator/controller
and native phase binding retained. Confirmed pre-fix missing-followers regression
in managed clone fixture; corrected phases pass. It explains the frozen added
rear fifth wheel; reporter's wider wording is not proof every rear wheel froze.
Live native Animator acceptance pending. Structural rearDriverFollowers checks
both rear fifth-wheel bindings and quaternion gaps on id46; live power captures
also include this report.

PRELIMINARY PHYSICS DECISIONS: Reduction-only DV-feel first pass, not historical
bore design, speed cap or final balancing. Stock S282 exact; 4-0-4 remains zero
traction; giant2-2-2 stays a prototype. No actual mass/CoM/brake/coupler change.
B: Existing native friction conversion receives family-driven share target:
no trailing .85 short/.88 larger, 4-6-0 .75; one trailing axle .73; two .63;
0-x-0 1; articulated axle-count share. Short-group caps18/19/20t and larger32t
absolute override targets. Use TotalMass, not CarMass, at selection. Selected
fraction follows native resource-mass changes afterwards (caps not continuously
recomputed). Physical axle load distribution stays DV-native. Curve driver-load
input matches effective driven-load assumption; stock comparison unchanged.

C: CylinderCalibration owns only per-car native engine snapshots. Common ref
15bar absolute/.75cutoff/300C, target stock pull/grip ratio from native baseline
radius/coefficient/dimensions at same mass. Scale active LOCOMOTIVE engine area
by min(1,target pull/current pull), preserve aux/main capacity ratios and stroke/
count. Main bore updated with cached pistonArea and cylinderVolume (native litres)
using two narrowly scoped FieldInfos for private readonly caches. Main/aux original
bore/area/volume restore exactly before each conversion, stock, failure rollback,
pool and destroy. Shared definitions, native graph/ports, chest state/volume,
throttle limits, event counters, min/max cutoff not rewritten. No universal0.40m
minimum: id46 main~.426m/aux~.297m preserves original ratio. Erie TENDER engine
not included/recalibrated; its previous separate grip/force clamp remains.
No boosting capacity where original pull already below stock reference ratio.
Actual smaller cylinder volume changes consumption natively; sustained economy,
maximum usable speed and actual drawbar performance require loaded captures.

NATIVE ADHESION FINDING: Installed B99 AdhesionControllerSystem queries BOTH
Copy_TrainCar_Data and Calculate_Adhesion only for moving, wheel-sliding or
wheelslipping tags. Resting stationary structural live grip can therefore retain
previous arrangement output, not just numerical mismatch. TractiveDiagnostics
adds adhesionEcsEligible using actual tag presence. Do NOT force native tags,
step physics manually or synthesize observed grip to hide stale samples.

TEST CHANGES: Stage2 converted snapshot permits only owned bore changes, still
checks stroke/count/cutoff/shared type/base mass, per-car cached consistency,
stock-reference force ceiling, second-car isolation. Snapshot now includes engine
cached area/volume so exact restored comparison catches cache leakage. Separate
physicsCalibration/rearDriverFollowers blocks added to structural; observational
live tractive samples include same metadata. Existing power diagnostic behavior
retained; no native state changes in observer/model.

OFFLINE VALIDATION:
- Production Stage3 net48 build0 warnings/errors.
- Validate.RearDriverFollowers:126 managed checks; actual cloning/follower code;
  pre-fix variant fails with destroyed followers as expected. Not Unity playback.
- Validate.PreliminaryPhysics:3917 checks,132 eligible layout/mass cases at
  111000/124079/135000kg,1400 actual native engine Tick smoke steps; exact cached
  restore, aux ratio, no capacity boost, per-car isolation, cap bounds passed.
- Validate.PreliminaryPhysicsIntegration:11 ownership/wiring checks.
- Validate.CylinderCasingIntegration -AllowPreliminaryPhysics against1.18:
 275 checks,103 EXISTING protected methods unchanged. New pure target-share
 method and original-friction getter allowed explicitly. Kept native methods,
 ArticulatedSimulation and DriveRetarget IL comparisons. Getter appended to
 B99PhysicsState so compiler-generated lambda names remain stable.
- Articulated integration152, steam pipe wiring19, curve model150, tractive
 observer/table74 checks. PowerDiagnostics harness passes (116 including new
 stationary/eligible ECS annotation checks; source capture unchanged).
- Baseline table generated from actual1.17 capture at
 build/runtime-trial-1.17.0/tractive-before-physics.{md,bbcode.txt}.
- Predicted1.19 table (not new live measurements) in native validation.json table.

NEXT: James runs1.19 all-standard structural first, then power/load/slip/endurance
captures and checks id46 rear wheels forward/reverse/slip. Pipes/manifolds from
1.18 still need game appearance/clearance/LOD/Erie tender lifecycle acceptance.
Power comparison: repaired stock vs4-4-0, Prairie,4-14-4, Angus,id46 using same
train/rail/weather/resources; START POWER DIAGNOSTICS, close UMM, stop after.
No infinite steam/unsupported cylinder mods for a usable native comparison.
Once evidence reviewed, next ledger presentation item is ERIE-01 tender exhaust/
cocks/sparks/LOD. Temporary tuning UI removal/baking proportions waits on final
geometry acceptance (American values still pending). Hard curve derailment risk
is a separate future feature; don't fold it into this power-test candidate.
TODO ledger updated. Installed1.19 is test-only; no publication/commit/push.

Previous history follows:

## 1.18.0 INSTALLED and previous 1.17.0 run reviewed - 21 Oct 2026

James explicitly requested installation and review of the previous run.
Verified game closed, installed prepared/hash-checked 1.18 ZIP, verified all39
package files. DLL DC18E4F639DE34F1654B3E36B62C679519E2170F8F62CC342D9DBD43FDCAD224.
Settings.xml and stage2-runtime-checks.json hashes unchanged. No backups made.
Receipt: build/runtime-trial-1.18.0/installation-receipt.json.
1.18 pipe appearance/live structural validation still pending.

Previous diagnostics version1.17.0.0: all47 passed, subjectL-007/controlL-011.
All oilcup counts matched; zero duplicate ports/overlap pairs, maximum provider
and guide gaps both0. Accepted tuning recorded exactly: Mogul .18/.12,
2-4-0 .20/.14, Prairie .22/.12. Tractive CSV error null.
Review: build/runtime-trial-1.17.0/runtime-review-20261021.json.
Player.log confirms all47 passed. Startup AItraffic WorkerRadio registration
error; shutdown CouplerInterfacer, CargoReactionToDamage, Bolt.SceneVariables,
GlobalShopController exceptions and SetActive-during-destroy warnings observed.
No Rearranged structural failure shown; shutdown stack traces do not establish
causation or absolve mod interactions. Manual driving/visual/unload still needed.

Historical candidate notes below (the not-installed statements are superseded):

## 1.18.0 steam-pipe candidate BUILT, NOT INSTALLED � 21 Oct 2026

ACTIVE USER REQUEST: While James runs the installed 1.17 structural test, start
repairing missing/odd steam pipes and manifolds. Installed files deliberately
untouched; game may still be running. Do not describe the current structural
run as testing these new pipes. Do not install until the test is finished and
James wants the new candidate. No backups/rollback copies created.

IMPLEMENTED: WheelRearranger.SteamConnections caches native boiler fittings and
barycentric surface sockets on actual casing triangles, including normalized
meshes. Modified rigids reshape native pipes only between rigid end collars.
Insufficient short-pipe room uses a routed manifold branch. Articulated engine
chests (including Erie tender engine) use the original MIT rear Franklin inlet
manifold and flanged long pipe meshes, with adaptive branches to a side header.
Source manifold is an L, not a tee: do not assume original prefab alone can chain
triplex engines. Native articulation/tender poses remain authoritative. Stable
native lateral axes determine sides; visual updates run at order 30000.

SteamPipeShape pure geometry and SteamPipePresentation per-car visual owner.
No custom joints/physics, and no compound/exhaust steam-routing implementation.
Stock S282, novelty 4-0-4 and giant 2-2-2 retain existing pipe presentation.
Per-car owned pipe meshes only; original authored meshes shared immutable.
Native LOD membership/fades/cull level retained and exact arrays/bounds restored
before casing/retarget cleanup. Owned renderers removed from explosion list.
Late failure hides stale pipes, restores previous source renderer states and
retains an error for diagnostics. Erie attach/detach rebuilds connections.
Structural JSON steamConnections validates counts, gap <=3mm, zero colliders,
and no presentation errors. Full body/gear clearance is NOT established by it.

ASSETS: models/steam_manifold.obj + steam_transfer.obj are original MIT bundle
geometry exports, no native game mesh shipped. Build script verifies their
hashes. Original bundle/credits/licence unchanged. Reproducible source/fixture
check: evidence/steam-pipe-tests/extract_original.py (UnityPy).

VALIDATION:
- net48 Stage3 production build: 0 warnings/errors.
- Validate.SteamPipes.ps1: 102056 actual-source pure geometry checks, 172 sweeps,
  28 native cases plus 4 routed-fallback cases; max endpoint gap 0.0000011m.
- Managed production actor fixture: 153 lifecycle/pose/LOD/ownership checks;
  max gap 0.000031m. Includes tender yaw and failed-source hiding. It uses
  managed scene/mesh stand-ins, not a live Unity player.
- Validate.SteamPipeIntegration.ps1: 19 compiled wiring checks passed.
- Validate.CylinderCasingIntegration.ps1 against installed 1.17: 275 checks,
  103 protected compiled simulation/motion methods unchanged.
- Articulated integration 152 checks; oilcup lifecycle wiring 21 checks passed.
- ZIP39 entries: both authored pipe assets, legal notices retained, no native
  fixture meshes shipped. Installed 1.17 DLL SHA verified unchanged.

REMAINING: actual in-game structural run on 1.18, all layout appearance/clearance,
LOD distance, reapply/stock restore, Erie coupling/uncoupling, tight curves,
multiple loco isolation and save/reload/delete. Side header branch routing is
an intake supply visual, not historical compound plumbing. Do not claim all
screenshots fixed or native rendering verified before James checks it.

Candidate ZIP: C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-1.18.0-Test\RearrangedS282-B99-1.18.0-Test.zip
DLL SHA256: DC18E4F639DE34F1654B3E36B62C679519E2170F8F62CC342D9DBD43FDCAD224
ZIP SHA256: B37D09B1B02073E59A451E7604D6200796746CF40C1766A81D3A189583DAEB9F
Receipts: build/runtime-trial-1.18.0/build-receipt.json
Evidence: build/steam-pipes-1.18.0/{geometry-validation,physics-validation}

Previous installed baseline follows:

## 1.17.0 requested proportions + PHYS-A BUILT AND INSTALLED, 21 Oct 2026

ACTIVE USER REQUEST: Resume ledger while James tests. Selected PHYS-A measurement
work, independent of geometry acceptance. During that work James supplied preferred
pitch/rearward metres: Mogul 2-6-0 .18/.12, 2-4-0 .20/.14, Prairie 2-6-2 .22/.12.
Asked explicitly about Prairie .22 exceeding existing .20 cap; James confirmed
"I want 0.22 m — extend the range and test it". Treat first two as accepted
preferences, Prairie .22 as requested next in-game check. American still pending.
Granular controls remain TEMPORARY; fixed layout definitions/remove tuning UI
still required after positions accepted. Steam pipes follow AFTER positions,
using original Franklin meshes first. PHYS-B/C decisions remain deferred.

GEOMETRY: Prairie-only pitch maximum .22 m (DriverLayout.ClampExtraPitch now
accepts layout ID; Create and structural check both use it). Other pitch caps
remain .20. Settings defaults updated to the three exact pairs, plus installed
Settings.xml six matching fields; all other XML setting elements verified intact.
No backup created. Existing stored positions .13/.15 were older; explicit user
values .12/.14 are authoritative. American independent controls retained.

PHYS-A IMPLEMENTATION: new pure TractiveEffortModel.cs and observational
TractiveDiagnostics.cs. Existing structural RunAll writes result.measurements
SEPARATE from immutable converted/restored snapshots, plus
stage2-tractive-measurements.csv during progress. Capture exact native TotalMass,
CarMass/CargoMass/ResourcesMass, native resource quantities, boiler pressure/water,
ECS live grip per axle/total, separate controller slip-drop limit, read-back friction,
engine IDs/activation/bore/stroke/cached area/volume/cutoff/pressures/controls/RPM.
Dormant aux engines disclosed but excluded; zero drivers have zero loco pull;
Erie tender engine/reference force/actual demand-limit-applied figures separate.
TenderEngineReport only gains observed radius, no tender traction changes.
Existing live PowerDiagnostics JSONL schema3 also includes tractive and reference
metadata. Original engine exceptions, rate/size/time limits and recording-off
behavior retained. Helper failures stay diagnostic-only, missing/nonfinite=null.
No new hook, port writes, simulation ticks or gameplay calibration.

REFERENCE METHOD: same explicit 15 bar ABS / .75 cutoff / 300 C for every active
engine. Mirror native MeanEffectivePressureRatio including condensation and
cutoff, cached piston area, stroke/cylinders/radius. Not instantaneous starting
pull, delivered power or a steam-supply simulation. Sampled-steam mean is a
magnitude before smoothing/drivetrain health; neutral=zero, cock/crack losses
included. Dry reference grip=read-back coefficient*captured TotalMass/axles*9.81
*powered. Native live grip differs with rail/sand and may be stale while stopped.
Tools/TractiveTable.ps1 outputs Markdown/BBCode from a NEW structural capture.
Stock force index=100*min(reference pull,dry grip)/same stock2-8-2 value.
No fabricated 1000t scaling / 2% grade claims. Invalid rows/missing stock stay n/a;
experimental 2-2-2 flagged, Erie tender not silently summed. Synthetic preview
under build/ is TEST DATA ONLY; no real performance table published.
TRACTIVE_DIAGNOSTICS.txt and Tools script are packaged in the mod folder.

NATIVE EVIDENCE: current installed DV.Simulation.dll/Assembly-CSharp native types
inspected via Cecil/ilspy (SimComponent.id, TrainMassController mass/resource and
weight-per-axle accessors, Boiler/ResourceContainer ports). Existing B99 native
engine and ECS source inspections verify the mean-torque/condensation and grip
formula. Existing PowerDiagnostics/CurveDiagnostics/native adapters were reused;
shared vehicle definitions and sim state remain untouched.

VALIDATION: Release/net48 build 0 warnings/errors. Geometry 83,747 coupling checks
(52 cases, includes Prairie .22), 1,801,664 connecting checks (30 cases, includes
all three requested pairs, 2,880 quartered native poses). Native main/crosshead
error <.365mm; no added LOD escape vs native reference. New/retained capture suite
114 checks incl faults, no-write samples, dormant/tender separation, missing ECS,
CSV quoting/culture. Table/compiled observer/wiring/default checks65. Existing
compiled integration275 confirms103 unchanged protected methods vs1.16 (native
steam/traction/articulated simulation/DriveRetarget); articulated152; oil lifecycle21.
All use managed or compiled fixtures: live visuals, ECS timing, structural capture
and loaded pull remain with James. Latest confirmed all47 live structural pass
remains1.14, not this candidate. No claims of live1.17 acceptance.

INSTALL: 2026-10-21T16:55:24Z, game checked CLOSED.37 installed package files hash
verified; no backup/rollback copies. Six explicit geometry settings updated;
remaining XML elements verified unchanged. Installed1.17.0.
DLL SHA256:173CA0393BC2BF92330A276294AC99B0736EB50C1A0BD516FC3509A125373965
ZIP SHA256:5A0EEB3BD47E7513A602F97F7620A6B3AF12C60785F60DA2156FA422EC029542
Desktop ZIP:Derail Valley Mods/RearrangedS282-B99-1.17.0-Test/
RearrangedS282-B99-1.17.0-Test.zip
Receipts:build/runtime-trial-1.17.0/{build-receipt,installation-receipt,package-files}.json
Tests:build/physics-a-1.17.0/{connecting-validation,pitch-validation,capture-validation,
table-validation,physics-validation,articulated-integration.json}.
No commit/push/publication or game/save writes performed.

NEXT: James finishes geometry and American; bake accepted layout dimensions and
remove trial controls for release. When ready run the existing structural test
for real PHYS-A JSON/CSV; use packaged Tools/TractiveTable.ps1 on that new capture.
Then measured current table informs B cap/share choices and C calibration.
Do not invent a report from older captures or begin B/C without those decisions.

## 1.16.0 Prairie / American tuning trial BUILT AND INSTALLED, 21 Oct 2026

REQUEST: Extend the same temporary tuning controls to Prairie Express 2-6-2
(ID44) and American 4-4-0 (ID0), build/install. Granular controls remain temporary:
accepted values must become fixed layout proportions, with trial UI removed for
release. Steam pipes are the NEXT phase AFTER layout positions are accepted:
cover Triplex/Erie, Mallets and long rigid layouts; inspect/reuse original Franklin
style meshes first. No steam-pipe implementation or new physics in this build.

IMPLEMENTATION: shared DriverLayout now supports 34,35,44,0 only. Three-driver
and two-driver topology reuse existing measured rod pin data and presentation
owners. New Prairie/American pitch controls 0..0.20 m extra per gap and rearward
controls 0..0.25 m group displacement default ZERO; Mogul/2-4-0 settings retained.
New settings are independent. Accepted Prairie wheel diameter/cylinder positions
retained. Prairie first applies existing clearance/base group shift WITHOUT
binding its former retarget, then pitch, then ONE precision retarget combining
base cylinder-side offset with rearward adjustment. Valve polish runs afterward
and reads that final CylinderDriveFrame. Original optional default Prairie binder
remains for retained fixture coverage; production explicitly passes false.
American physical axles now follow the same presented-wheel resolver as 2-4-0.
Brakes/springs/sparks, oil ownership, reverse-order LOD and translation cleanup
reuse existing implementation. Structural expectations extended to all four.
No new dependencies, native extracted assets shipped, game/save edits or publish.

VALIDATION: Release/net48 build 0 warnings/errors. Connecting integration:
1,357,928 checks /24 cases /2,304 native poses /6,912 cup samples; all four layouts,
0/.15/.25 m rearward, .15 pitch, rotated/shared LOD variants; independent settings
binding and zero defaults. Actual production Prairie clearance/base preparation
and cylinder frame compiled; optional Prairie fitting list is a stand-in in that
harness, not full in-game static geometry validation. Coupling fixture 76,139
checks /48 cases covers all four layouts at 0/.15/.20 pitch, repeated lifecycle,
initial native phase mismatch, switching, cache disposal and another-car isolation.
No added LOD escape vs native baseline; main/crosshead <0.365 mm, radius <0.319 mm.
275 compiled integration checks /103 unchanged methods vs installed 1.15.0,
including steam/traction/articulated simulation and DriveRetarget. Articulated
metadata 152, oil lifecycle wiring 21. Stand-ins do not prove live renderer,
physics registration, visual clearance or save/pool behavior. James tests those.

INSTALL: game confirmed closed, 21 Oct 2026 14:38:56 UTC (15:38 London).
35 installed files verified against audited ZIP/candidate. Settings preserved
byte-for-byte. No backup or rollback copy created, as requested.
DLL SHA256: 770A445E12467E4328FF53F4A7B3C0DFF8E186BD4CFDFF04C91A0B8A7CD5B1DB
ZIP SHA256: 9F0DFA789C980BD4AAF416E4D644242146B3A41D7EF5CCCB62472235BEBE5BE5
Desktop ZIP: Derail Valley Mods/RearrangedS282-B99-1.16.0-Test/
RearrangedS282-B99-1.16.0-Test.zip
Receipts: build/runtime-trial-1.16.0/{build-receipt,package-audit,installation-receipt}.json
Live 1.16.0 acceptance pending. Latest confirmed full structural47 pass is1.14.0.

USE: highlight/select with radio -> UMM named arrangement pitch/rearward sliders ->
REAPPLY CURRENT ARRANGEMENT; inspect success/refusal/target. Re-select if cleared.
Stop/repair, regulator closed, force/slip settled. Report layout + BOTH values
and preferably side-on screenshot; can read saved Settings.xml after tuning.
Test whole rotations/reverse/slip, both sides/LODs, oil cups, brakes, switching,
second-car isolation and save/reload. No new slider applies to Pacific/Hudson etc.

## 1.15.0 main connecting rod trial INSTALLED, 21 Oct 2026

RELEASE INTENT, clarified by James: granular geometry sliders are temporary
development/tuning tools, not intended release features. Goal is realistic fixed
proportions for every arrangement. Record accepted values per named layout,
then bake those values into layout definitions and remove these trial controls
from the release UI. Sharing driver count does not imply sharing geometry:
current precision/pitch controls affect only 2-6-0 and 2-4-0, not Prairie,
Pacific, Hudson, American or other x-4-x/x-6-x layouts. Extend deliberately using
per-layout geometry; do not automatically propagate offsets by wheel count.

James explicitly requested no further full backups and deletion of this project's
backup/rollback copies. Cleanup removed 37 folders (1,520,790,759 bytes), with no
remaining matching backup/rollback paths in this repository. Other projects were
outside this cleanup. Build.RuntimeTrial.ps1 no longer creates install backups.
Historical receipts and handover entries describe backups that have now been deleted.
Do not recreate those copies or run historical backup-making installation scripts.

Installed 1.15.0 DLL SHA256:
C6FB69301FE8527A0961E40AD3AF125B5591D67DE72000E9AFB8FE920BF8C936
Desktop ZIP: Derail Valley Mods/RearrangedS282-B99-1.15.0-Test/
RearrangedS282-B99-1.15.0-Test.zip
Installation receipt: build/runtime-trial-1.15.0/installation-receipt.json
Settings preserved. No game acceptance of 1.15.0 yet; no publication or commit.

Main connecting rod controls are scoped to Mogul 2-6-0 (34) and 2-4-0 (35):
0..0.25 m driver group rearward, default 0.15 m, with fixed cylinders/guides.
Existing coupling-pitch controls remain. Precision native-clock mesh deformation
preserves bearing bosses and stretches shafts through the existing motion cycle;
this is visual retargeting, not a physically rigid new slider-crank simulation.
Native unreadable LOD0 rods use the existing local mesh exporter/OBJ fallback;
no extracted game meshes ship. Shared LOD ownership restores in reverse order.
Retain original native LOD envelope plus translated geometry, exact restore.

UMM now has direct REAPPLY CURRENT ARRANGEMENT for the highlighted/selected S282
and RESET WHEEL-ARRANGER RADIO SELECTION, plus menu-open input guards. Reapply
uses existing safety checks and persists only on success. Original reported radio
cycling failure remains unconfirmed: Player-prev.log had no mod radio/conversion
exceptions. Native hotbar/screenspace-mouse gating is a hypothesis, not a finding.

Validation: Release/net48 build zero warnings/errors; connecting native motion
609,473 assertions; actual radio/Main callbacks 267 checks; rod remesher 452,776;
retained Prairie motion 81,873; 4-14-4 1,452; coupling integration 38,230;
physics integration 216 checks/74 compiled physics methods unchanged;
articulated metadata 152; oil lifecycle 21. Max main/crosshead error 0.365 mm,
radius/hinge 0.319 mm; no added LOD envelope escape vs native baseline.
Latest live structural pass is all 47 on 1.14.0, not 1.15.0.
Manual acceptance: both sides/LODs, full cycles/cutoff/reverse/slip, repeated
slider reapply/reset and arrangement switching, oil cups, pooling/save/reload,
second-car isolation. Other 45 layouts retain prior geometry/retarget behavior.

## 1.14.0 coupling-rod pitch trial BUILT AND INSTALLED, 21 Oct 2026

CURRENT REQUEST / DECISION
James wants rod control before the deferred physics ledger: safest, smallest
first step with useful positional freedom, with agents and native checks.
Selected coupling rods first, initially 2-6-0 (34) and 2-4-0 (35). This is not
the main connecting-rod/piston/guide stage. Other 45 layouts retain 1.13.6 geometry;
no new wheel enlargement, cylinder resizing or power rebalance in this trial.
No commit, push, Nexus/GitHub publication or game/save editing performed.

IMPLEMENTATION
- New RodRemesher.cs changes Z only. Rigid 0.14 m axial boss windows preserve
  bearings/joints; interpolate between window edges to avoid discontinuities.
  Owned cloned meshes preserve index/submesh/UV/material data; shaft texture
  density stretches with shaft length. Never mutate MeshFinder shared sources.
- New DriverLayout.cs and WheelRearranger.DriverLayouts.cs support only 34/35.
  UMM Coupled driver spacing trial: mogulExtraCoupledPitchMetres and
  twoFortyExtraCoupledPitchMetres default 0.15 m, clamp 0..0.20 m, 0.01 m steps.
  Reapply after changing. Main driver3 stays fixed; driver2 moves forward and
  Mogul driver4 rearward. Each adjacent gap gains the chosen WORLD pitch.
- Native clips bind wheel position/scale. Keep original transforms/components
  alive and clear only their mesh filters (both LODs). Renderer-only siblings
  use existing PoweredWheelRotater.LateUpdate, copying completed native rotation.
  Native main connecting rod, return crank and oil-provider hierarchy untouched.
- Two/three-axle short meshes use independently measured bore centres, with
  separate front/rear segment mapping to correct original rounded seating.
  Original short transforms retained. Cache at most three owned mesh slots;
  phase-related projection tolerance 1 micrometre, with explicit phase tests.
- Real native wheel meshes at both LODs, correct flanged coarse mesh from native
  wheel1, correct short rods shared across both native LOD levels through far
  cull. Restore exact prior LOD arrays/order/fades/reference point/size.
- Brake shoes/calipers, detail springs and sparks follow matching drivers.
  LMS has no separate coarse suspension splits; do not claim coarse spring
  remeshing. Inactive detail springs hidden; preserve authored height/size.
- Physical powered axles, big-end oil planning and driver-group/curve diagnostics
  resolve presentation centres. Curve cache invalidates on apply/suspend; existing
  curve calculations consequently see the new wheelbase. No new steam/traction
  equations, radius, mass, cylinder bore/stroke or independent slip changes.
- Cache after SetupDrivers, apply after legacy geometry before axle alignment;
  suspend before stock/prepared snapshots and dispose on deletion. Scope errors
  restore stock without blocking unrelated layouts. Structural JSON records
  driverPitchPresentations and extraCoupledPitchM and verifies expected values.

NATIVE AUDIT / IMPORTANT CORRECTIONS TO THE STARTING PROPOSAL
- Coupling rod is not perfectly translation-only: authored rock up to 0.034491
  degrees remains. Native wheel position/scale streams are bit-identical constants
  across low/medium/high, but the main-rod CHILD translation is slightly animated.
- A late audit caught live main-pin capture causing left/right cache rejection
  when first converted at differing phases. Reproduced old failure in the fixture
  BEFORE installation. Final code reads the UNANIMATED carLivery.prefab main pin
  using the same native prefab API already used by ArticulatedEffects. Do not
  revert to sampling live main.localPosition or rounded legacy -1.55 setters.
- Actual prefab L/R datums bit-identical except drive-root X reflection; main
  local Z -1.5533394813537598. Live High main Z varies about 0.287 mm; even normal
  quartering exceeds a 1 micrometre mesh-cache tolerance. Fixture now phases the
  live L/R hierarchy BEFORE initial cache and supplies a separate stable prefab.
- Both native drivetrain Animators AlwaysAnimate, so cleared source meshes do
  not visibility-cull the clock. PoweredWheel is data-only: redirecting its visual
  adds no rotation clock. Sharing renderer references across LODs is native B99
  practice (203 of 776 inspected groups), so no duplicate-rod workaround needed.
- Preserve intended legacy wheel-centre basis before Animator resets it: native
  reset rounds Mogul wheel2 to Z6.3/Y0.71 and wheel4 Z3.2/Y0.71; 2-4-0 wheel2 is
  Z6.62/Y0.71. Main3 retains native centre. The fixture mirrors these actual setters.

VALIDATION (OFFLINE, NOT GAME ACCEPTANCE)
- Final Release/net48 Stage3 1.14.0: zero warnings/errors.
- Pure real-topology remesher: 452,776 assertions, 180 cases, 57,600 triangle
  checks, 56,880 boss samples, 34 invalid cases; max added translation error
  1.19e-7 m, no reversed/collapsed triangles. build/rod-remesher-validation.
- Actual owner/descriptor/remesher/rotation copier/Additional consumer with
  managed Unity stand-ins and native clips/wheel/rod data: 38,230 assertions,
  24 layouts/orientations/LOD-owner/pitch combinations, 576 sampled native poses,
  72 repeat cycles, 96 phase reapplications, 152 initial-cache/later phased
  applications, six cross-layout switches, three rollback cases. Includes cache
  cap/disposal, source immutability, matching fittings, physical axle resolution,
  LOD restoration, second-car isolation and missing-prefab scoped failure.
  Receipt build/driver-layout-validation/validation.json. BeforeCanonical.cs and
  reproduced failure retained privately under build; never installed/shipped.
- Actual bearing longitudinal seating max 0.274658 mm. Added boss distortion 0;
  added vertical drift 0.121593 mm, within native rocking/span bound plus float
  precision. Existing combined YZ mismatch remains up to 5.257 mm (old short rods
  5.851 mm): legacy wheel-height rounding/native motion, NOT perfect 3D alignment.
- Retained production Prairie/high-speed motion: 81,873 assertions, 672 native
  poses, 2,016 oil samples. Additional/prototype baseline: 1,934 checks.
- Final compiled casing/native integration 269 checks; 100 physics/retarget
  methods unchanged against 1.13.6. Articulated/native metadata 152 checks;
  oil lifecycle wiring 21. Reports build/coupled-spacing-1.14.0.
- Native reservoir sweep at +0.15 m pitch: Mogul approx 85 mm and 2-4-0 approx
  374 mm minimum sampled gap. At max +0.20 m: approx 50/331 mm. Frame clearance,
  actual Unity culling/upload/scheduling, oil interaction and live lifecycle need
  in-game inspection; conservative native frame data already overlaps internally
  before this change, so no clean-frame claim from that data.

FINAL PACKAGE / INSTALLATION (GAME CLOSED)
- Version 1.14.0, DLL SHA256
  64F20173DB360F3572DEB7CA933EF49BDEE95D83EC7D875457AB00E7E7D0FD8F
- Desktop ZIP C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-1.14.0-Test\RearrangedS282-B99-1.14.0-Test.zip
- ZIP SHA256 07E675133472EBD1FE11DE96F56DB5CE89869EFEE01F69834BDA08AB55AB2259
- 35 packaged/installed files verified; 27 seed files and all 14 legal/credit
  notices unchanged. Original authored bundle and Franklin OBJ unchanged. All
  production source hashes agree with build receipt. No Settings/logs/saves/native
  fixtures/PDBs in package. Prior exploratory 1.14.0 candidate overwritten before
  any installation; only final canonical-prefab version was installed.
- Full previous 1.13.6 folder backup, all 66 files hash-verified:
  build/runtime-trial-1.14.0/installed-backup-20261021-120444
- Installed B:\SteamLibrary\steamapps\common\Derail Valley\Mods\RearrangedS282
  at 12:04 UTC. Settings byte-identical:
  57D796625848A9F41DBDD4770593EA537D389DB65646C7A59B3FA5A93DE0D7BB
- Build/package/install receipts build/runtime-trial-1.14.0 and beside Desktop ZIP.

NEXT
James tests both layouts at 0/0.15/0.20 m, both sides/near/far, full rotations,
reverse/cutoff/slip/braking, cups, repeated switching/stock/other layouts, second
car, save/reload, pooling/deletion and explosion/repair. UMM RUN STRUCTURAL TEST
requires TWO stationary S282s BOTH set to STOCK 2-8-2. Latest stored all-47 game
structural pass is 1.13.5, not evidence of 1.14.0 game acceptance. Full checklist
installed and beside ZIP. Main connecting rods, piston/guides and wider rollout
remain later geometry stages; physics ledger deferred.

---

## 1.13.6 Beta changelog corrected since 1.9.2

Updated the public changelog and Nexus copy to cover changes since 1.9.2, including the shipped duplex/triplex stale-part cleanup ordering fix recorded in 1.11.1. Verified that installed and packaged DLLs contain early DriveRetarget.Unbind before LOD restoration and the triplex suspension path; no claim of a dedicated in-game back-and-forth visual test. Documentation-only ZIP update; all other 35 entries unchanged and checked against the prior manifest. Updated both Desktop and repo ZIPs, manifests and checksums. New ZIP SHA256 2864600D6EB876024CBFAC8892BF58735EFECABD8653178B141859766E7E240E; runtime DLL unchanged.

---

## 1.13.6 Beta NEXUS RELEASE PACKAGE PREPARED, 21 Oct 2026

James requested a Nexus-ready archive and release details. Prepared from the
verified 1.13.6 trial ZIP, WITHOUT rebuilding or changing the installed runtime.
No Nexus upload, GitHub release, commit or remote push was performed.

PUBLIC DELIVERABLES
- Desktop ZIP: C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-1.13.6-Beta\RearrangedS282-B99-1.13.6-Beta.zip
- ZIP SHA256 698327D08B4023B2868C2346A1A10BA57AB56B03A6B5C7D03792D56B8D1F50EB
- Runtime DLL matches installed/test version exactly:
  87752D9DE63E4567F601ED6D4DAB713F91E49F089C95BFDA9DE5DB4EA0C575EF
- Paste-ready NEXUS_RELEASE_COPY.txt beside ZIP includes file title/version,
  description, declared requirements, current-version laconic changelog,
  separate recent-series highlights and requested styled credits.
- CHECKSUMS.txt, TEST_STATUS.txt and release-manifest.json also beside ZIP.
- Identical ZIP and receipts in repo release/1.13.6-Beta; editable public prose
  in release-copy/1.13.6-Beta. Both destinations were created fresh.

PACKAGE AUDIT
- 36 files under canonical RearrangedS282/ folder. Info.json retains ID/version,
  entry point and dependencies; public DisplayName is Rearranged S282 - B99.
- Runtime DLL, exporter binaries/config, original bundle, Franklin OBJ and all
  original licence/dependency notices byte-identical (29 protected files checked).
- CREDITS updated from earlier 1.9.2 release text: original author/source/MIT
  attribution retained, JimTaplin, Claude/Codex, optional integrations and other
  bundled contributors credited. Original LICENSE is untouched.
- Replaced trial/history README and checklist with concise public instructions;
  removed ARTICULATED_TEST_STATUS/RUNTIME_TRIAL_TESTING history files from this
  PUBLIC archive only. Added README.txt, CHANGELOG.txt and TEST_STATUS.txt.
- ZIP integrity/path/hash audit passes. No Settings.xml, save/log/diagnostic data,
  native geometry fixtures, development caches or PDBs included.
- Installed mod files are unchanged by packaging; display-name/docs changes apply
  only to the release ZIP. No extra installation requested or necessary.

UPDATED RUNTIME EVIDENCE (supersedes prior stale latest-report notes)
The game's current structural report is version 1.13.5.0, all 47 arrangements
passed, subject L-006/control L-047, last write 21 Oct 2026 11:13 UTC. Snapshot
saved privately to build/release-validation-1.13.6/structural-1.13.5-passed.json,
with checksum audit. Do not ship that game diagnostic snapshot publicly.
This advances the previously observed 1.13.3 baseline. It does NOT establish
1.13.6's final 0.70 m Prairie rearward placement as tested in game. Public notes
state prior 47-layout structural pass and latest offline validation separately.

RELEASE DETAILS / LIMITS
File title Rearranged S282 B99 - 1.13.6 Beta, version 1.13.6, author JimTaplin.
Requirements from Info/native installed evidence: Windows, DV B99, UMM >=0.27.3,
Locomotive Mesh Splitter (1.1.9 tested, Nexus mod 867). Optional Model Tweaks /
Car Changer integration is not a required dependency. Realistic Firing remains
unsupported for this beta, without claiming compatibility is impossible forever.
VR/multiplayer unverified. Giant 2-2-2 and Erie tender remain experimental;
unfinished Erie effects/slip/LOD work is disclosed. No new power/grip rebalance.
Next: James reviews/publishes ZIP as desired; inspect any new testing feedback.

---

## 1.13.6 complete Prairie assembly rearward BUILT AND INSTALLED, 21 Oct 2026

James accepted the smaller Prairie wheels but requested cylinders, running gear
and drivers move back together to improve the overall proportions. Scope is
Prairie only, retaining 1.13.5 wheel size/rod geometry and fixed carrying trucks.

IMPLEMENTATION
- Smallest native solution: PrairieDriverForward 1.50 -> 0.80 m and
  PrairieCylinderForward 0.60 -> -0.10 m. Both change by -0.70 m, translating
  the complete mechanism without changing the relative driver/cylinder span.
  Their difference remains -0.90 m. Existing scale/anchor compensation,
  retarget, casing/fitting mapping, spark placement, native oil bindings and
  physical powered-axle alignment apply the existing geometry at the new pose.
- Cylinders/guides/static calipers/springs/shoes/cups/cock outlets follow the
  mechanism. Branch-pipe boiler sockets remain fixed, existing blend reseats the
  cylinder ends. No change to wheel diameter (~1.576 m), pitch, crank throw,
  simulation radius/RPM, mass, traction or steam settings from 1.13.5.
- High-speed and all other layouts retain 1.13.5 geometry. No physics-ledger work.
- Initial 0.60 m rearward candidate reduced sampled strap gap to 10.4 mm, failing
  the existing >=18 mm geometry criterion. Sweeping positions showed nonmonotonic
  strap clearance. 0.70 m rearward clears it by 29.35 mm, so used that instead;
  James was told why. The 0.60 m candidate was NOT installed.
- Measurement now reads PrairieDriverForward from actual source instead of a
  duplicated hard-coded offset, preventing stale clearance results on retuning.

VALIDATION
- Release/net48 B99 build: zero warnings/errors.
- Actual production motion/casing/oil partial fixture: 81,873 assertions, 672
  native pose scenarios, 2,016 cup samples passed. Expected casing/driver/spark
  placement updated to -0.70 m from 1.13.5; sizes, source geometry/native animation,
  guide mounting and restoration remain checked. Three high-speed cases retained.
- Native mesh swept-envelope check: 707,381 reservoir samples, both reservoir and
  wheel LODs: Prairie minimum 0.0293496 m, high-speed unchanged 0.0217008 m. This
  is native-data sampled geometry, not game rendering/suspension acceptance.
- Compiled casing/native integration: 269 checks passed, 100 prior physics and
  DriveRetarget family methods unchanged against 1.13.5. No further broad test
  rerun needed for the two placement constants; relevant fixture re-executed.
- Audit: 35 packaged files, 27 unchanged seed files, all 14 legal/credit notices
  identical, original authored drivetrain bundle/Franklin OBJ unchanged.
- Game testing remains with James. No game launch, save edits, push or publication.

INSTALLATION, GAME CLOSED
- Version 1.13.6; DLL SHA256 87752D9DE63E4567F601ED6D4DAB713F91E49F089C95BFDA9DE5DB4EA0C575EF
- ZIP C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-1.13.6-Test\RearrangedS282-B99-1.13.6-Test.zip
- ZIP SHA256 7AF950602051C2E4AEF74DC62782856A6A10801E26D941604B1BFD5CE3726617
- Full previous 1.13.5 folder backup: build/runtime-trial-1.13.6/installed-backup-20261021-112213
  All 66 original files hash verified; all 35 installed package files verified.
- Settings preserved byte-identical SHA256 57D796625848A9F41DBDD4770593EA537D389DB65646C7A59B3FA5A93DE0D7BB.
- Build/package-audit/installation receipts in build/runtime-trial-1.13.6;
  build/installation receipts also beside ZIP. Work still uncommitted.
- Geometry/motion receipts in build/prairie-rearward-1.13.6.

NEXT: James reapply Prairie; assess overall placement, both sides/LODs, branch-pipe
sockets, cylinder/leading-truck surroundings and rear-driver/trailing-truck gap.
Full revolutions/reverse/cutoff/slip/braking, shoes/sparks, oil-can lids/refill,
external unload/reload, save/reload, structural test and second-car isolation.
Keep wheel size as accepted; do not alter other layouts without request.
Updated active/deferred ledger is TODO.md. Latest existing all-47 runtime report
still belongs to 1.13.3 and cannot validate this version.

---

## 1.13.5 reservoir-clear large drivers BUILT AND INSTALLED, 21 Oct 2026

James requested the previous feasibility/outstanding-work report be written into
an active to-do ledger, then focus ONLY on shrinking high-speed versions and the
new Prairie enough to clear the info-board air reservoir while retaining large
wheel character. TODO.md now contains prioritized current/deferred items and
acceptance criteria, with original upstream TODO preserved as historical notes.
Physics A/B/C are recorded, NOT implemented. No publication or remote push.

INSTALLED, GAME CLOSED
- DLL SHA256 3FEBE4F4FADA33C8A7DE0C6FF55582AD521F45845197B2A5B74EEC71F3F9AC4C
- ZIP C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-1.13.5-Test\RearrangedS282-B99-1.13.5-Test.zip
- ZIP SHA256 0067EFBD41F8CE5564A690BC05D37A5D763051D4DFE0AA50002D2EFFFC817B83
- Previous 1.13.4 full backup: build/runtime-trial-1.13.5/installed-backup-20261021-095133
  All 66 files hash verified. All 35 package files verified after install.
- Settings byte-identical SHA256 57D796625848A9F41DBDD4770593EA537D389DB65646C7A59B3FA5A93DE0D7BB.
- Build/package-audit/installation receipts in build/runtime-trial-1.13.5;
  build/installation receipts also beside Desktop ZIP. No game launch/save edits.

MEASUREMENT / IMPLEMENTATION
- Scope exactly IDs 12/13/14 (high-speed 2-8-0/2-8-2/2-8-4) and 44 (Prairie).
  Other layouts retain wheel size/spacing. Pacific/Hudson are not reduced.
- Actual LMS air tank includes mounting straps. Native flange radius exceeds
  tread radius, so sizing from tread alone still clips. Resources wheel meshes
  and installed LMS body OBJ/split selections were used, both native LODs.
- Native tank samples against full rotating wheel envelopes reproduce penetration
  about 0.108 m on high-speed and 0.240 m on the forward-shifted Prairie. These
  are conservative swept-surface geometry distances, not measured game collisions.
- New drive Y/Z scale 1.11 gives sampled minimum gap 0.02170 m on both cases;
  approximately 707,381 reservoir surface samples, 12 mm subdivision, conservative
  1 mm wheel X-profile bins. Native data stays ignored/local, never ships.
- Nominal/sim diameter: high-speed 1.57679 m (old 1.7018, down 7.35%); Prairie
  1.57618 m (old 1.8288, down 13.81%). Still about 11% larger than stock 1.424 m.
  Prairie needs more reduction because its relocated front driver sits directly
  beneath the reservoir. Do not claim a 2-3% reduction solves the measured envelope.
- New LargeDriverClearance partial uses existing native scaling, with front active
  driver as longitudinal anchor. Driver pitch contracts uniformly with mechanism;
  front driver remains at accepted position, carrying trucks/reservoir unchanged.
  No new clock, component, hook, DLL or motion solver. Do not shrink wheels alone:
  that would detach crank pins or leave tyres off the rail.
- Same relative change applied to simulation radius via existing setWheelRadius;
  B99PhysicsState refreshes native RPM/circumference. High-speed physical powered
  axle anchors now align to their actual resized wheel centres via existing helper.
- Static calipers/springs use existing captured native drive-frame mapping. Shoes
  already live under the drive; sparks follow the frame change. Prairie retains
  accepted +1.50 m front-driver position and +0.60 m casing-rear datum: existing
  retarget shift includes the scale/anchor compensation, not a fixed -0.90 m now.
- Existing full cylinder normalization extends to high-speed IDs, so pipes/cocks/
  casing bores seat on the resized native motion while keeping native casting
  length. Full treatment now covers ten IDs 0/1/2/4/5/12/13/14/35/44. All other
  casing treatment retained; broad walk/item shells remain existing approximation.
- Cylinder bore/stroke, steam model, mass and adhesive-weight allocation unchanged.
  Smaller wheels naturally change wheel-rim force and road-speed/RPM relationship;
  this is matching native radius, not the deferred power/grip rebalance.
- 47-choice startup log now uses actual SupportedCount; bundled notes updated.

VALIDATION
- Release/net48 against installed B99: 0 warnings/errors.
- Native swept geometry measurement: PASS, both wheel/reservoir LODs, both cases.
  Source scripts evidence/wheel-clearance-tests, result build/large-driver-clearance-1.13.5/geometry.json.
- Actual source clearance/Prairie/retarget/casing/oil partial fixture: 81,873
  assertions across 672 native pose scenarios, 2,016 cup samples. Selected ID scope,
  matched radius, front anchor/casing datum, immutable animated transforms,
  source geometry, native casing length, guide seating and restoration pass.
- Whole-casing fixture expanded to ten selected IDs: 244,459 checks / 960 native
  poses passed. Pacific/Hudson logic unchanged. No new casing shape function.
- Additional-arrangement native-anchor fixture: 1,934 checks passed, including
  all four selected IDs across rotated bogies and untouched unselected axles.
  First run could not compile its old stub enum (missing existing high-speed IDs);
  enum inventory corrected, cases extended, rerun passed. Production build passed
  independently before that fixture correction.
- Compiled casing/native integration: 269 checks, 100 baseline compiled methods
  unchanged across physics and DriveRetarget families compared with 1.13.4.
- Compiled articulated integration: 152 passed. Original drivetrain bundle and
  Franklin OBJ unchanged. ZIP audit: 35 files, 27 seed files unchanged, all 14
  legal/credit/notice files unchanged, only 4 expected seed replacements.
- Whole-working-tree diff check still reports a pre-existing extra EOF blank line
  in Stage2RuntimeChecks.cs from 1.13.4; not a functional failure or this fix.
- Managed/native-data fixtures do NOT execute live Unity rendering, suspension,
  actual native oil-can interaction or full game lifecycle. Latest stored all-47
  structural report is still 1.13.3, not evidence of 1.13.5 game acceptance.

NEXT: James reapply all three high-speed variants and Prairie; both sides near/far,
reservoir strap/flange clearance, full revolutions/reverse/cutoffs/slip, shoe and
spark placement, pipe/cock seating and cups. Switch stock/Pacific/Hudson/4144/
articulated to ensure original sizes restore. Run structural test, save/reload,
external unload/reload, explosion/repair and second-car isolation. Read diagnostics
and Player.log after feedback. Deferred physics and other work are in TODO.md;
do not start them until requested.

---

## 1.13.4 Prairie forward spacing + oil-cup mounting/cleanup BUILT AND INSTALLED, 21 Oct 2026

Latest request: cups may rescale, but no extra regular-sized floating cups or
excess count. Previous screenshot showed 5 cups/side on Prairie. Five is valid:
3 crank-pin cups plus native fixed-guide and crosshead cups. Preserve actual
bearing coverage; do not delete legitimate points just to make the count three.
James accepts the 1.13.3 casing reduction; retain it. This build adjusts Prairie
only for spacing, applies cup mounting/lifecycle fixes across layouts.

Installed with game closed, no game launch/save edit/publication/commit/push.
- Version 1.13.4; DLL F728972BDFB53ECB51CEC866972576F5CAAE42C2640448C221A7F0779DC2E139
- ZIP C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-1.13.4-Test\RearrangedS282-B99-1.13.4-Test.zip
- ZIP SHA256 000EBC10C1E0B4DE1A72067E4D6BB649A188D80915BAC06CF5106291F31DCE79
- Full previous 1.13.3 folder backup: build/runtime-trial-1.13.4/installed-backup-20261021-090419
  All 66 original files hash verified; all 35 package files verified after copy.
  Settings.xml byte-identical, SHA 57D796625848A9F41DBDD4770593EA537D389DB65646C7A59B3FA5A93DE0D7BB.
- Build, installation and package audit receipts in build/runtime-trial-1.13.4;
  build/installation receipts also beside Desktop ZIP. Working tree remains
  uncommitted with prior sessions' changes. No ownership handoff to another agent.

IMPLEMENTATION / EVIDENCE
- New PrairieSpacing partial: actual native drivetrain group forward 1.50 m;
  cylinder/crosshead/guide side forward 0.60 m via existing DriveRetarget;
  connecting/radius rod span reduced 0.90 m. Driver diameter and pitch, trucks,
  native clocks/curves, traction, steam and mass centre unchanged. Visual/static
  calipers, springs and spark positions follow drivers. Hidden first native
  suspension stays hidden. CylinderDriveFrame exposes the rendered cylinder
  frame to existing casing and Walschaerts helpers; accepted casing vertices
  retain shape, translate +0.60 only. Other arrangements' spacing unchanged.
- Walschaerts static cylinder-side fittings follow cylinder frame; lifting-arm
  supports follow drivers. Dry pipe mesh rebuilt from immutable source per apply
  (reuses owned clone); otherwise repeated layouts would retain prior mapping.
  Prairie maps statics even if polish visibility is disabled.
- New OilCupMounts: caches native provider poses. Fixed guide cups follow actual
  guide mesh frame instead of mismatched scaled drive. Native LMS L/R guide mesh
  naming is physically reversed; reflected articulated guides can swap sides.
  Select nearest actual mesh centre and account for mesh handedness, tested.
- DriveRetarget binds native crosshead/main-rod oil providers to rendered proxy
  children. Same LateUpdate explicitly calls existing native consumer.Sync after
  positioning providers, preventing script-order lag. Unbind restores original
  local provider positions. No second animation system or new per-frame component.
- Native PositionSyncConsumer.Sync only copies position, not scale. Real prefab
  native cup heads share same cap mesh/scale; some OilCup03 bodies have longer
  stems. No blanket stem resizing needed or done. Added cups clone native prefab,
  not an already scaled live assembly. Keep native fittings, lids and refill ports.
- ClearBigEndCups now runs on external unload, pool reset and destruction as well
  as switching. Removes tracked and owned-prefix leftover cups/markers, deinitializes
  readers/feeders, removes owned controller entries and immediately hides before
  deferred destruction. Cancels stale generations. This closes a demonstrated
  missing cleanup path; duplicate/orphan cups in James's screenshot are not proven.
- Rod-free 4-0-4 and giant 2-2-2 clearance prototype hide unsupported six native
  cups, hold unused points full/closed. Other layouts restore visibility. B99's
  Unity version lacks GetComponentInParent(includeInactive): direct ancestor walk
  in B99Oilcups.ConsumerFor restores hidden cups without reactivating early.
- New OilCupDiagnostics captures expected/active counts, unique refill ports,
  overlapping mounts, guide/provider gaps and cap dimensions. Structural runner
  waits up to 120 frames for delayed extra-cup creation before capture; failures
  report count mismatch, >2 mm provider gap or >35 mm guide-bounds gap. Native-six
  stable identity/port checks remain. Global all-layout live results still pending.

VALIDATION (this compiled candidate)
- Release/net48 installed B99 references: 0 warnings, 0 errors.
- Validate.PrairieMotion: 41,833 assertions, 288 native pose scenarios and 864 cup
  samples: real native clip/rod/casing/pipe/guide data, actual production partials
  and retarget with managed Unity stand-ins. Accepted casing shape +0.60 only,
  shortened rod pin endpoints, animated transforms/source meshes unchanged,
  reflected guides, positive native and negative Franklin retarget cup sync,
  provider restoration, source ownership, contact sparks, pipe/cock triangles.
- Surface orientation test initially compared old/new normals; forward pipe bend
  legitimately rotates some >90 degrees. Replaced that invalid inversion proxy
  with positive finite-difference mapping Jacobians and triangle normals compared
  to transported local tangent normals; passed. No pipe distortion fix needed.
- Prior whole-casing fixture 195,601 checks/672 native poses passed.
- Pacific/Hudson fixture 29,973/96 poses passed; Fourteen spacing 1,452/96 passed;
  original FourteenCoupled source unchanged E4E28C87FD719B0A4B478E21A088CE26C992B04B6274B637D6DEDC7767D7BD88.
- Compiled casing/integration 262 checks and 95 prior compiled methods unchanged
  across ConventionalPhysics/B99PhysicsState/ArticulatedSimulation/DriveRetarget.
  Deliberate retarget exceptions ctor/LateUpdate/Restore add cup state, sync and
  cleanup. Existing retarget mesh, role selection and animation math unchanged.
  Added record/field declared at end to preserve baseline compiler closure names.
- Compiled articulated integration 152 passed. New OilCupLifecycle compiled wiring
  inspection 21 passed. BigEndOilingPlan allocation/save fitting 18 passed.
- ZIP: 35 files, 27 seed files identical, all 14 legal/credit files identical;
  only expected 4 seed changes (Info, DLL and two checklists). Authored bundle and
  Franklin OBJ unchanged; every production source hash matches build receipt.
- Tests do NOT execute live Unity controller dispatch, oil-can refilling, pool
  unload, actual rendering, powered running or complete all-layout game tests.
  Do not claim all floating cup issues are proven absent until James tests.

NEXT: James reapply Prairie; full rotations both directions/cutoffs, inspect
front truck and wheel/guide/cylinder clearances, near/far LOD, cocks/particles,
open/fill/close every cup. Switch stock/Prairie/4144/Pacific/articulated/novelty
repeatedly, external unload/reload, save/reload and second-car isolation. Run
UMM RUN STRUCTURAL TEST — two stationary S282s, inspect latest JSON and Player.log.
Keep accepted casing proportions; do not extend forward spacing to more layouts
without request or evidence. Updated manual checklist is shipped in test ZIP.

---

## 1.13.3 full casing expansion BUILT AND INSTALLED, 21 Oct 2026

James requested the same normalization for 4-6-2/4-6-4 and rigid x-4-x while
retaining motion/physics, then explicitly confirmed installation when done.
Installed from the audited 1.13.3 ZIP with the game closed. No game launch, save
edit, commit, push or publication. Work remains uncommitted on b99-port.

Full treatment now covers exactly seven IDs: 0/1/2 (4-4-0/4-4-2/4-4-4), 4/5
(4-6-2/4-6-4), 35 (2-4-0) and 44 (2-6-2). Articulated multi-engine arrangements
are outside this expansion; other layouts keep their prior casing treatment.
All 47 arrangement IDs and the 4-14-4 spacing change are retained.

Native-first comparison confirmed Show4Drivers uses scale 1.42 / Z offset
-3.935, but only translates its static cylinders up 0.30. Show6Drivers uses
1.2879 / -2.1; polished supports map the casting with the drive. Four-coupled
casings were already native-sized, but the valve bore did not follow the enlarged
valve spindle axis. They must not simply receive another inverse-scale shrink.
The full casing path now reads the CURRENT NATIVE DRIVE frame and maps only
the static mesh vertices into it, independent of the old static-casting frame.
This places both existing animated bore axes correctly. Full selected layouts
also work with polished Pacific/Hudson gear disabled. No animation transforms,
clips, wheel size/spacing, crank motion, RPM, power or steam calculations changed.
Existing valve-gear visibility choices and static guide poses are retained.

Reuses the 1.13.2 NormalizePrairie helper (historical name, now shared), per-car
mesh ownership and fitting/outlet restoration. No new component, hook, DLL or
motion framework. Native width and length retained; overall height includes the
extra distance between the existing bores: about 1.60 m on six-coupled and 1.68 m
on four-coupled, native 1.42 m. Four-coupled casings thus gain a taller inter-bore
web rather than shrinking below their previously native barrel size. This was
explained to James. Do not claim exact stock overall dimensions. Dry pipes,
cocks/plumbing and 12 native particle systems follow each casing; boiler pipe
ends, emitter sizes/directions, source meshes and broad collision shells retained.

VALIDATION:
- Release/net48 against installed B99: 0 warnings, 0 errors.
- Shape fixture: 690,269 repetitive numerical assertions across 96 native/
  Franklin mesh cases, adding full-casing scale 1.42. No sampled non-positive
  Jacobian or tested collapsed/reversed casting triangles.
- Actual casing/Walschaerts partial fixture: 195,601 checks and 672 native
  High/Med/Low cutoff/side poses across all seven arrangements, both LODs,
  actual cocks and dry-pipe triangles, 12 actual native particle systems,
  stock/setting bypass, polished option off, repeated restore/disposal, rotated/
  scaled car, source immutability and second-car isolation. All passed.
- Actual dry-pipe/cock triangles additionally tested for collapse and >90-degree
  normal changes, including the 1.42 four-coupled bend. No failures.
- Compiled casing integration: 265 checks passed; 98 physics/motion method
  bodies match compiled 1.13.2. Compiled arrangement integration: 152 checks.
- Existing Pacific/Hudson support fixture: 29,973 checks / 96 native poses pass.
- FourteenCoupled.cs and DriveRetarget.cs hashes still match the previously
  passed 1,452-check / 96-pose spacing fixture. No broader rerun needed.
- Four-coupled actual-mesh preview inspected:
  build/full-casing-shape-1.13.3/four-coupled-preview.png.
- These are managed/native-data fixtures and compilation, NOT game tests.
  In-game guide/boiler clearance, fitting seating, LOD transitions, oil access,
  explosion/repair, driving and save/reload remain James's test. Priority list
  is at the top of packaged RUNTIME_TRIAL_TESTING.txt.
- Three deliberate unused-field CS0649 warnings remain in managed tender/
  Franklin stubs; actual mod compile has zero warnings.

PACKAGE / INSTALL:
ZIP: C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-1.13.3-Test\RearrangedS282-B99-1.13.3-Test.zip
ZIP SHA256 0FE13738D3F7DCEA343CF644CB00CDCB838AE4E898095D8FBEE7494966637453.
DLL SHA256 10DD988D9960D63D72939FBD8F1ED9DFCCFECCEAE58C560612D7DB57F00213A8.
35 installed/package files verified. 27 original seed entries byte-identical,
including all 14 legal/credit/notice files; original bundle and Franklin OBJ
unchanged. All production source hashes match the build receipt.
Full prior 1.13.2 folder backed up and all 66 files verified:
build/runtime-trial-1.13.3/installed-backup-20261021-002757.
Settings preserved byte-for-byte, SHA256
57D796625848A9F41DBDD4770593EA537D389DB65646C7A59B3FA5A93DE0D7BB.
Near-vanilla cylinder option is true. Installed Info.json is 1.13.3 and DLL
matches ZIP. Receipts/audit at build/runtime-trial-1.13.3 and installation receipt
also beside Desktop ZIP. Shape/integration detail at build/full-casing-shape-1.13.3
and build/full-casing-integration-1.13.3. Old diagnostics preserved as history.
New extract_full_fittings.py replays native LMS dry-pipe selections for tests;
native JSON/meshes/references stay local/ignored and are not shipped.

## 1.13.2 Prairie complete-casing trial BUILT AND INSTALLED, 21 Oct 2026

James authorised proper Prairie cylinder normalization with existing motion and
physics intact, plus the already-built 4-14-4 spacing change, and installation of
the combined test. Installed 1.13.2 from the audited local test ZIP while the game
was closed. No game launched, saves edited, commit, push or release performed.
Work remains uncommitted on b99-port, including previous 1.13.0/1.13.1 changes.

CONFIRMED CAUSE / NATIVE COMPARISON: Atlantic/other legacy four-coupled layouts
use Show4Drivers (drive Y/Z scale 1.42) but MoveCylinders only translates their
static casings. Prairie uses Show6Drivers (1.2879) plus the polished native
Walschaerts support map, shared with Pacific/Hudson. That map scales the complete
casings to match moving joints. Thus the oversized casting is geometry-path
dependent, not a Prairie-specific physics failure. The 1.13.0 barrel-only pass
was too limited. Pacific/Hudson can show the same enlarged silhouette but retain
their existing treatment in this specifically requested Prairie fix.

IMPLEMENTATION: CylinderCasingShape.NormalizePrairie resizes the entire casing
length from its rear face, and both barrel/cap regions around the two existing
piston/valve bore axes. A monotone C1 Y map retains the greater separation of
those axes in the inter-bore web; no separately pinned surface patches or local
dents. Native length is restored (about 1.417 m at LOD0). Overall casing height
is about 1.601 m versus native 1.418 m, because retaining the animated axes needs
the taller web. Do not claim the whole casting has exactly stock dimensions.
Both native LODs and both sides are handled. No moving transforms, clips,
crank throw, wheel spacing/diameter, RPM, simulation ports or power code changed
for Prairie. Uses the existing mesh-clone/restore owner, no new component/DLL.

WheelRearranger.CylinderCasings.cs caches the original native cocks/dry-pipe
meshes and the 12 native steam/drip/crack ParticleSystem transforms. Cocks and
their plumbing follow the same map as the casing; dry-pipe cylinder ends follow
while boiler ends remain native with the existing smooth bend approach. Native
fittings are used, not earlier Pacific/Hudson bent meshes. Per-car meshes restore
before retarget/geometry teardown; particle local positions restore in reverse.
Native particle hierarchy and zero shape-origin offsets were verified directly
in installed resources.assets. Emitter orientation/size, ports and controllers
are unchanged. Broad walk/item cylinder collision shells remain at their prior
shape; no rigidbody or vehicle-physics edit. The existing casing option controls
this treatment. James's installed option and polished gear option are true.

Includes 1.13.1's additional 0.25 m rearward 4-14-4 driver shift, existing rod
retarget and crosshead oil-cup alignment. Its geometry source and DriveRetarget
hashes still match the previously passed 1,452-check / 96-pose spacing report.
No new motion framework. Other 46 arrangement IDs/behaviour retained.

VALIDATION:
- Release/net48 compile against installed B99: 0 warnings, 0 errors.
- Shape fixture: 659,721 repetitive numerical checks across 90 actual native/
  Franklin mesh cases (84 previous cases + 6 Prairie/cocks LOD/side cases).
  No tested collapsed/reversed triangles or non-positive sampled Jacobians.
- Actual casing/Walschaerts production partials with managed Unity stand-ins:
  44,043 checks, 96 sampled native cutoff/side poses, both LODs, all 12 actual
  native particle systems, rotated/scaled car, repeated restoration/disposal,
  shared-source immutability, two-car isolation, setting off and stock bypass.
- Existing Pacific/Hudson fixture: 29,973 checks / 96 native poses passed.
- Compiled casing integration: 263 checks; 98 physics/motion methods match
  compiled 1.13.0. Validator now permits only cached particle position writes
  in fitting apply/restore; actual animated transforms checked by the fixture.
- Compiled arrangement integration: 152 checks. Rod timing: 6,720 simulated
  rendered poses match, callback still LateUpdate. No in-game driving performed.
- Full mesh preview inspected: build/prairie-casing-validation/prairie-preview.png.
  Reference vs installed 1.13.0 treatment vs new full casing, not a game render.
- Managed stubs emit three CS0649 warnings for deliberately unused tender/
  Franklin fields; the actual mod build has zero warnings.
- Visual quality/clearance in Unity, pipe seating, oil interactions, native LOD
  transitions, explosion/repair, save/reload and driving remain James's tests.
  Follow the 1.13.2 checklist at the top of RUNTIME_TRIAL_TESTING.txt.

PACKAGE / INSTALL:
ZIP: C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-1.13.2-Test\RearrangedS282-B99-1.13.2-Test.zip
ZIP SHA256 2075E67C35DBE998897F22C47A63F7312D2DDAC025F94329E104979A967A268A.
DLL SHA256 9B378AA32B391350B3F5053F32849B367D5E0CEE506EE69702FF64C690D93B89.
35 packaged/installed files hash verified. 27 original seed entries byte-identical
including all 14 legal/credit/notice entries; original bundle/Franklin OBJ hashes
unchanged. All production source hashes match the build receipt.
Full prior 1.13.0 folder backed up and all 66 files verified:
build/runtime-trial-1.13.2/installed-backup-20261021-001602.
Settings.xml preserved byte-for-byte, SHA256
57D796625848A9F41DBDD4770593EA537D389DB65646C7A59B3FA5A93DE0D7BB.
Installed Info.json is 1.13.2, DLL matches the ZIP. Old diagnostics are preserved
and are historical until James runs the new test. Build/install receipts are in
build/runtime-trial-1.13.2 and beside the Desktop ZIP. Detailed numerical reports
are in build/prairie-casing-validation and build/prairie-casing-integration.
Native game geometry, native particle fixtures and decompiled references remain
local/ignored, never runtime/source assets. Extraction scripts/harnesses are kept.

## 1.13.1 4-14-4 spacing test BUILT, NOT INSTALLED OR PUBLISHED, 21 Oct 2026

James authorised moving the 4-14-4 drivers farther back while holding cylinders
and both carrying bogies fixed, retaining physics/animation and building a test.
1.13.0 remains installed (Info.json and DLL hash rechecked). No game launch, save
edit, install, commit, push or release performed. Working tree includes the
uncommitted 1.13.0 casing work as well as this spacing change.

Smallest existing solution: FourteenDriverShift changes from 0.25 to 0.50 m,
an additional 0.25 m rearward. The existing DriveRetarget and native animation
remain unchanged. Cylinder map, carrying-axle coordinates, driver pitch/diameter,
wheel radius and power parameters are retained; existing AlignAdditionalAxle
keeps native physical axle anchors with the moved rendered drivers. Main/radius
rods bridge the new distance using the existing retarget. Added a 4-14-4-only
crosshead oil-cup provider offset because retargeted renderers move independently
of their non-animated Oiling1 child. Prepared geometry restores it on switching.
Native measurements give nominal front/rear tyre gaps of 0.55/0.20 m, previously
0.30/0.45 m. No broader rod framework or new dependency was introduced.

Validation:
- Release/net48 against installed B99 assemblies: 0 warnings, 0 errors.
- Validate.FourteenSpacing.ps1: 1,452 assertions across 96 sampled native
  High/Med/Low cutoff poses, both sides. Actual production layout and unchanged
  DriveRetarget exercised with managed Unity stand-ins and local native fixtures.
  Covers fixed trucks/cylinders, driver shift, gap guard, rendered endpoints,
  cup placement, mirrored/transformed frames, LOD restoration and repeated binds.
  Maximum sampled endpoint difference versus old cylinder-side endpoints:
  0.034 mm; inherited retarget length-scale variation about 0.196%.
- Compiled articulated integration: 152 checks passed.
- Compiled casing integration: 259 checks passed; 98 physics/motion methods
  byte-equivalent to 1.13.0. This does not prove unchanged whole-game physics.
- Rod timing regression: 6,720 simulated rendered poses match source quaternions;
  compiled callback remains LateUpdate. No game/runtime reproduction claimed.
- Package audit: 35 files, 27 seed files byte-identical including all 14 legal,
  credit and notice entries. Original bundle and Franklin asset hashes preserved.
- In-game frame/shoe clearance, rolling appearance, real LOD transitions and
  carrying-truck behaviour on curves remain for James to test. Checklist at the
  top of RUNTIME_TRIAL_TESTING.txt is packaged with the test.

ZIP: C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-1.13.1-Test\RearrangedS282-B99-1.13.1-Test.zip
ZIP SHA256 A0B8F94136E9F296AB319974710A566BD21CA2C204CB48DF4626B94D4B84A7D7.
DLL SHA256 415E4D7D056F964CCF4E090337DF5C56712BA1C8922F5D67646CF42877E67C50.
Reports: build/fourteen-spacing-validation/validation.json and
build/runtime-trial-1.13.1 (build receipt, package audit, compiled integration,
casing-integration and rod-timing reports). Native geometry fixtures remain local
and ignored; only extraction code and managed harness sources are intended for Git.

PRAIRIE FEEDBACK, OPEN: James's L-090 screenshot shows the overall cylinder
assembly still oversized. Player.log confirms four casing meshes were reshaped
for 2-6-2 Prairie (e.g. lines 8278/8280 in the inspected log), so it was not
omitted from dispatch. The 1.13.0 pass was too limited to meet the requested
overall near-vanilla appearance: upper chest, rear socket band and longitudinal
length remain at the enlarged drivetrain scale. Numerical safety checks were
not visual acceptance. Acknowledge that limitation; do not claim normalization
succeeded or that 1.13.1 fixes Prairie. Decoupling casing placement/size from
wheel layout and the moving linkage is the relevant next investigation, using
existing retarget facilities first. No further casing rewrite was implemented
in this spacing task. Do not revive blind whole-mesh deformation.

## 1.13.0 test INSTALLED on James's explicit request, 21 Oct 2026

Game process was absent before installation. Installed directly from the audited
local 1.13.0 test ZIP; no rebuild, publication, release or save edits performed.
Verified all 35 installed package files against the extracted ZIP. Installed
Info.json is 1.13.0 and DLL SHA256 is
F650F21A2718F87727F03452AC989CF2265CCB60F81622B6EE9BC2D8ED6ABE5C.
Existing Settings.xml was preserved byte-for-byte, SHA256
C8669F86CE3B2233A11FE9DAFB92ACCB14EFF554CDB9D48EE3839606838DA4BE.
Complete previous 1.12.0 mod folder backed up to
build/runtime-trial-1.13.0/installed-backup-20261020-234042.
Installation receipt: build/runtime-trial-1.13.0/installation-receipt.json, also
copied beside the Desktop test ZIP. Both build-receipt copies now record the
installation. In-game testing remains outstanding. Casing option defaults on;
toggle and reapply the arrangement for comparison. No game launched by Codex.

## 1.13.0 cylinder barrel casing trial (Codex), BUILT LOCALLY, NOT INSTALLED OR PUBLISHED AT BUILD TIME, 21 Oct 2026

James authorised casing work and a test build: roughly vanilla size, adjusted as
needed, retaining existing motion/physics where possible. Baseline was clean
b99-port at 14b1128, with 1.12.0 installed. This work remains uncommitted. No push,
GitHub release, Nexus upload, game launch, installation or save edits performed.
Future remote changes must still target james, never origin. Installation requires
James's explicit go-ahead, game closed, verified released ZIP and existing backup/
Settings preservation workflow.

Native-first decision: simple inverse-scaling of the whole casting cannot retain
both piston and valve axes. Native body-frame Y values are 0.712937 and 1.347092;
the drive scales their separation. Current engine maps scale Y/Z, not X. We keep
all animated transforms, timing, wheels, crank throw and native simulation intact.

IMPLEMENTED: optional per-car mesh treatment of MAIN CYLINDER BARREL HEIGHT/BULK.
This is deliberately not a full vanilla-sized cylinder assembly. Upper valve
chests, casing length, rear socket bands, drain/cock mounts and steam-pipe contact
remain at the previous geometry. Existing native walk/item collision shells also
remain unchanged and may not exactly match the revised outer surface. No new
mesh colliders, rigidbodies, physics patches, ports or animation proxies added.

Initial combined Y/Z correction produced local folds near pinned pipe sockets.
Removing Z correction prevented that but the actual-mesh preview still showed
upper-chest dents. Final correction therefore preserves the whole upper chest
(native Y >= 1.30, blending from 1.02; Franklin bridge >= 1.12, from 0.85), with
conservative 0.85 strength. Small-engine enlargement is capped at 1.35 target
gain and rail clearance 0.08 m, hence at most 29.75% applied barrel enlargement.
Larger main barrels approach native height. Do not claim the upper chests or full
outer bounds are now vanilla-sized, or that this is a complete linkage retarget.

Files: CylinderCasingShape.cs contains deterministic geometry; the
WheelRearranger.CylinderCasings.cs partial captures native mesh frames, handles
native LOD0/LOD1 plus lazily cached rear engines and visible retarget proxies, and
owns/restores temporary meshes. Original meshes/materials stay immutable.
Franklin's original bundled casing is CPU-unreadable; models/franklin_casing.obj
is a readable export of ONLY that original MIT mod asset, preserving positions,
UVs, normals and winding. Build.RuntimeTrial.ps1 packages it in assets/. The
unchanged original drivetrain bundle remains intact. Native game mesh data and
decompiled references are LOCAL fixture evidence only, not runtime/source assets.

Integration: CacheCylinderCasings after SetupDrivers; ApplyCylinderCasings after
layout geometry; restore before articulated/retarget teardown and geometry
snapshots. Tender statics inherit the corrected source mesh; detach the tender
before restoring/freeing it. OnDestroy additionally disposes readable fallback
sources. Native stock ID10 bypasses the treatment. Settings field
nearVanillaCylinderCasings defaults true; UMM label is "Near-vanilla cylinder
casings (trial, reapply)". Toggle and reapply to compare/restore old presentation.
Structural capture adds cylinderCasingMeshes; counts do not prove appearance.

VALIDATION:
- Release net48 build against installed B99 assemblies: 0 errors, 0 warnings.
- Actual production shape helper: 629,173 numerical assertions across 84 actual
  native LOD0/LOD1/Franklin mesh cases, scales 0.4 through 1.42, offsets and
  anisotropic scale. All passed. No sampled non-positive Jacobians and no tested
  triangle normals turning >90 degrees. These are repetitive vertex checks,
  NOT 629,173 game scenarios. Final preview inspected before packaging.
- Original Franklin export: 5,642 attribute/index round-trip checks passed.
- Compiled casing integration: 259 checks, including stock bypass and cleanup
  order; 98 physics/motion method bodies unchanged from compiled 1.12.0 across
  ConventionalPhysics, B99PhysicsState, ArticulatedSimulation and DriveRetarget.
- Existing suites passed: catalogue/save policy 1,421; compiled articulated
  integration 152; additional arrangements 1,891; frame geometry 3,947; effects
  9,014; big-end oiling 18; curve drag 150; Pacific/Hudson valve-gear fixture.
- No Unity/game runtime exercised. In-game appearance, articulation/body clearance,
  near/far transitions, oiling/cocks, explosion/repair, tender attach/detach and
  repeated switching/save/reload are still for James to test. Updated checklist is
  at the TOP of RUNTIME_TRIAL_TESTING.txt; lower notes are historical.

Package: C:\Users\james\Desktop\Derail Valley Mods\RearrangedS282-B99-1.13.0-Test\RearrangedS282-B99-1.13.0-Test.zip
ZIP SHA256 B03F99070564480CC3C8F39053EC23853531E08D7BB4E7B4BD5C8785F978E292.
DLL SHA256 F650F21A2718F87727F03452AC989CF2265CCB60F81622B6EE9BC2D8ED6ABE5C.
Franklin OBJ SHA256 33F38E9F278FFF97EDED891830119227767D2027845D5574A8A9C59ECCC79739.
35 packaged files; 27 seed entries byte-identical, including all 14 legal/credit/
notice files. Only expected DLL/Info/test-document seed entries changed.
Receipts/audit/logs: build/runtime-trial-1.13.0; geometry report/preview:
build/cylinder-casing-validation; compiled comparison: build/cylinder-casing-integration.
Final source helper SHA256 A62A758B6E059A56A1AC8D843E1AC27016D3D4FE7FBC8A4E91A0C43119D4A0C7.
Installed version remains 1.12.0, DLL SHA256
3D6094AE837DC0687684E33DFEA5ECC15D74798BFCF263CD0B68076D0B28F7FC (rechecked).

NEXT after casing trial feedback: the previously requested larger drivers AND
wider driver pitch on 2-6-0/2-4-0, not implemented here. Other Claude handover
items (switching report, big-end cups, Erie later stages, curving derail stress)
remain as previously documented. Do not revive the discarded casing deformation.

## 1.12.0 Prairie, 2-6-6-0, 2-10-10-2 (Claude), RELEASED pre-release v1.12.0 (tag 7d11e81), INSTALLED, 20 Oct 2026

Installed from the released ZIP (380AA5B2...4755; DLL 3D6094AE...F7FC from the final rebuild, Info.json 1.12.0).
1.11.0 backup: build/runtime-trial-1.12.0/installed-backup-20261020-212334 (1.11.1 was never installed). Settings kept.

James's requests on 20 Oct: space out and enlarge the 2-6-0/2-4-0 (his answer:
"both", wider pitch and bigger drivers; NOT DONE YET, next build), a 2-6-2 Prairie
with Pacific-size drivers, a 2-10-10-2 with ~0.85 m drivers, and a 2-6-6-0
Mallet. Includes 1.11.1.
- s262 = 44 "2-6-2 Prairie", on the legacy path: ShowTwoLeadingWheels,
  Show6Drivers (1.83 m drivers as 4-6-x) and the 4-6-2's x62 trailing truck.
  It also gets the polished Pacific/Hudson valve gear and 2-6-0-style axle
  alignment, and is in the default random pool.
- WheelRearranger.BigMallet.cs is now table-driven (SpecFor): engines of 3, 4
  or 5 coupled drivers, optional leading/trailing trucks, FrontExtra, and a
  solved scale. 3-coupled rears use malletRearL/R; 4 use bigMalletRearL/R
  (the Erie tender engine source); 5 use tenCoupledRearL/R (ConfigureLongEngine
  re-enables the five-axle rod segment by sibling index; the fifth caliper sits
  at -3.08 as in Show10Drivers). Extra powered axles are now lists (up to 2
  front, 2 rear).
  - s2660 = 45 "2-6-6-0 Articulated Mallet": 3+3, leading truck; estimate
    s about 0.89, 1.26 m drivers (UNVERIFIED).
  - s210102 = 46 "2-10-10-2 Articulated Mallet": 5+5, both trucks,
    FrontExtra 0.6; estimate s about 0.58, 0.83 m drivers (UNVERIFIED).
- Tests: catalogue 1,421 (47 IDs, default pool 31), integration 152 (legacy
  switch tables now 0-44), additional 1,891; the rest pass.
- ZIP SHA256 380aa5b24f7f759368b564819539d2fe723077efe388273653cd1dd949174755.

## 1.11.1 hotfix (Claude), RELEASED pre-release v1.11.1 (tag bad2503), NOT installed (game running), 20 Oct 2026

James's 1.11.0 structural test failed: 41 of 44 failed with "Native six-cup
oiling setup is missing" (Stage2RuntimeChecks.CaptureOilcups required exactly six
OilingPointsPortController entries). The 3 that passed had no big-end cups. The
cups themselves were created on every layout (57 log lines).
Player.log also showed 18,638 NullReferenceExceptions from SimController.Update,
starting just after an external-interactables reload (repeated "Big-end oiling"
lines with no switch in between). Cause: the cups were appended to the native
controllers before those controllers ran Start/Awake, so the controllers
initialised them again (double port subscription). Clearing them unsubscribed
once, and the leftover IndicatorPortReader/OilingPointPortFeederReader handlers
pointed at destroyed cups, firing on every sim tick.
Fixes:
- Cups are built in BuildBigEndCupsLater: 3 frames later, after
  InteractablePortFeedersController finishes; cancelled by a generation counter.
- Every Init is preceded by Deinit.
- The self-test checks only the native six.
- Triplex cleanup order: DriveRetarget is unbound first in
  SuspendArticulatedGeometry.
Tests: all suites pass (big-end 18, catalogue 1,399, integration 144).
ZIP SHA256 97152bb0d2589d99e9a263dea906ffd644f2e12ae563bad10573098fe8a68d45.

## 1.11.0 oil cups on every driven big end (Claude), RELEASED pre-release v1.11.0 (tag d18e386), INSTALLED, 20 Oct 2026

Installed from the released ZIP (715B9260...333E; DLL 2697A630...12A2 == build, Info.json 1.11.0).
1.10.1 backup: build/runtime-trial-1.11.0/installed-backup-20261020-203020. Settings kept.

James asked for an oiling point on every driven big end. He chose live cups
that drain and can cause damage, build only.
- Native layout per side (resources.assets): Oiling0 fixed on the drivetrain
  (z 7.80), Oiling1 on the crosshead, Oiling2 on the main-rod big end. The
  native ManualOilingPoints component ticks every point; any point at or below
  5% causes damage. Its save loads only if the point count matches exactly.
- BigEndOiling.cs (Harmony):
  - SimController.Initialize prefix: on S282s, the ManualOilingPointsDefinition
    grows from 6 to 20 points (indices 6-12 left, 13-19 right). Port IDs are
    point 0's plus "_RSBIG<i>".
  - ManualOilingPoints.Tick postfix: points that aren't active for this flow
    are held full and closed.
  - SimulationFlow.SetSaveStateData prefix: the saved array is fitted to the
    point count (native six kept, the rest start full).
- BigEndOilingPlan.cs (pure): Allocate fills slots front to back per side,
  skipping big ends with a native cup. ActivePoints, FitSavedPoints.
- WheelRearranger.BigEndOiling.cs, run after both B99Oilcups.Bind calls:
  - Sites = every active driving wheel on every engine animator under
    MovingParts_LOD0. The crank is that engine's main rod position minus its
    main wheel. The front engine's main wheel keeps the native cup.
  - Each extra cup has a marker (PositionSyncProvider) on that engine's
    coupling rod, 0.14 m above the pin.
  - Cups are instantiated from the externalInteractablesPrefab (exploded
    prefab when exploded), using the side's native Oiling2 cup path. Port-ID
    strings are rewritten; the cups are appended to the PositionSyncConsumer,
    OilingPointsPort and IndicatorPortReaders controllers and initialised.
    InteractablePortFeeder (the lid) is initialised two frames later, as the
    native code does.
  - Cleared on every refresh.
  - Stock 2-8-2, 4-0-4 and 2-2-2 keep the native cups only.
  - New setting bigEndOiling (default on).
  - B99Oilcups ignores our cups.
- Not covered: the Erie tender engine (another car).
- UNVERIFIED in game: lid and oil-can interaction on the cloned cups, the
  oil-level indicator, the refill audio, cup placement on the pins, and cups
  rebuilt after unload/reload.
- Tests: Validate.BigEndOiling.ps1 has 18 checks; the other suites pass
  (catalogue 1,399, integration 144). ZIP SHA256 715b9260ec1994985e58e89b64e07ea9b2c2d66ca5bb961902b71649e401333e, DLL 2697A630...12A2.

## 1.10.1 = 1.10.0 + merged release-1.9.2 (Claude), RELEASED pre-release v1.10.1 (tag 6bbf45b), INSTALLED, 19 Oct 2026

Installed from the released ZIP (5516FF23...7A83; DLL 9EFF86FB...DBDC9E == build, Info.json 1.10.1).
1.10.0 backup: build/runtime-trial-1.10.1/installed-backup-20261019-221632. Settings kept: on first load
the one-time migration unticks the high-speed 2-8-x variants in the saved Settings.xml.

James asked to merge the 1.9.2 spawn defaults and Nexus packaging into b99-port,
then install. Conflicts resolved: Build version 1.10.1; Main UI label kept as
the 1.10 text; both handover histories kept. The 44-ID tests were already updated
for the Erie triplex (spawn28882 is opt-in). Catalogue 1,399, integration 144;
the other suites pass. ZIP SHA256 5516ff237033ec096e2c52d7771c62502e4f8caae99f77b22cdf1109f5af7a83, DLL 9EFF86FB...DBDC9E.

## Note: 1.9.2 Nexus release lives on branch release-1.9.2 (Claude), 19 Oct 2026

Built from v1.9.1 in worktree B:\GitHub repo\rearranged-s282-b99-1.9.2. The
high-speed 2-8-x variants left the default spawn pool (once-only migration via
spawnPoolRevision), and Package.Nexus.ps1 plus release-copy/1.9.2 docs were added.
Released as GitHub pre-release v1.9.2; Nexus ZIP at Desktop\Derail Valley Mods\
RearrangedS282-B99-1.9.2-Nexus. NOT yet merged into b99-port: 1.10.x still has the
old spawn defaults. Merge release-1.9.2 before the next 1.10 release.

## 1.10.0 Erie triplex 2-8-8-8-2, tender engine stage 1 (Claude), RELEASED pre-release v1.10.0 (tag bb6b06c), INSTALLED, 19 Oct 2026

Installed from the released ZIP (51317DF4...EDA8; DLL 5946EEEC...9B70 == build, Info.json 1.10.0).
1.9.1 backup: build/runtime-trial-1.10.0/installed-backup-20261019-220538. Settings kept.

James asked for the faithful option: the tender engine drawn on the tender car,
with its pull applied to the tender's bogies and its grip taken from the
tender's own weight.
- Catalogue: s28882 = 43, "2-8-8-8-2 Erie Triplex". WheelArrangement.LocomotiveWheels
  returns "2-8-8-0" for it (used by the Conventional and Stage2RuntimeChecks axle
  checks). The loco half is the BigMallet path with an eight-coupled front and a
  leading truck (scale about 0.71, UNVERIFIED).
- Simulation: the middle engine (ArticulatedRearEngine) has TenderMode. After
  each tick its torque and exhaust are moved into TenderTorque/TenderExhaust and
  its ports are zeroed, so the loco's traction and blast sums exclude it; the
  previous values are restored before the next tick, keeping the native smoothing.
  Its intake (steam demand) stays in the sum. Apply(43) sets TenderMode;
  SetTenderEngine(bool) activates or disables it. Restore clears it.
- WheelRearranger.TenderEngine.cs, polled from FixedUpdate (TickTenderEngine):
  - Wanted = this layout + loco not derailed + rear coupler on a car with
    CarTypes.IsTender && TrainCarType.Tender, not derailed.
  - Attach builds visuals: clones of the eight-coupled rear drives and static
    clones on "[Rearranged tender engine]" under the tender, with the cylinder
    front at tender z -0.35. It hides the tender bogie frames, brakes and
    wheelsets except the rearmost (the trailing truck), and adds the tender
    animators to the loco's wheel animation.
  - Force: TenderTorque / (0.712 s), clamped to 0.24 x tender TotalMass x g x 0.8,
    split over the tender bogies along the loco's direction. The tender is
    woken if it's asleep.
  - Detach on uncouple, derail, switch (SuspendMalletGeometry) and loco
    OnDestroy; dead animators are filtered out.
  - Diagnostics: "tenderEngine" block.
  - ArticulatedEffects skips the middle engine in tender mode.
- Not done (later stages): tender exhaust stack, chuff and smoke; tender cylinder
  cocks; tender slip visuals and sparks (it only clamps); tender brake heat;
  tender LODs; non-S282 tenders (the engine just stays off).
- NOT seen in game: tender/body frame alignment, clearance under the tender
  tank, force sign with a reversed tender.
- Tests: GraphProbe adds 7 tender-engine checks; catalogue 1,397 (44 IDs),
  integration 144; the other suites pass. ZIP SHA256 51317df4615d58c7fa3994bfe28437231e6784dd757e53323cb4a47de6f5eda8.

TODO (James, 19 Oct): the next Nexus release must restrict random spawning to
the useful, non-meme arrangements. List to be agreed.

## 1.9.1 0-8-8-0 Angus + 2-6-8-0 Gt. Northern (Claude), RELEASED pre-release v1.9.1 (tag ecc6f39), INSTALLED, 19 Oct 2026

Installed from the released ZIP (518EE44E...41F6; DLL A8955B6F...C724A == build, Info.json 1.9.1).
1.9.0 backup: build/runtime-trial-1.9.1/installed-backup-20261019-214054. Settings kept.
## 1.9.2 Nexus release (Claude), branch release-1.9.2 from v1.9.1, 19 Oct 2026

James asked for a Nexus-ready ZIP of the latest version before 1.10, plus a
changelog and a feature list. Spawn pool decision (his): "conventional only,
minus memes". All conventional layouts stay in the default random pool except
the high-speed 2-8-0/2-8-2/2-8-4 (now off), the 2-2-2, 4-14-4 and 4-0-4; all
articulated layouts stay opt-in. Built as 1.9.2 from the 1.9.1 code (his choice);
no 1.10 tender-engine code.
- Settings: spawn280Big/282Big/284Big now default false. spawnPoolRevision plus
  MigrateSpawnPool() unticks those three once for older settings, and Main saves
  the result. Later choices stand.
- Main UI: "43 ARRANGEMENTS — B99 BETA".
- Package.Nexus.ps1 repackages build\runtime-trial-1.9.2: test notes replaced
  with release-copy\1.9.2 README/CHANGELOG/CREDITS; DisplayName "Rearranged
  S282 - B99"; forward-slash ZIP entries. Output goes to release\1.9.2 and
  Desktop\Derail Valley Mods\RearrangedS282-B99-1.9.2-Nexus; the
  NEXUS_RELEASE_COPY.txt upload text is not in the ZIP. Nothing was uploaded to Nexus.
- Tests: catalogue 1,391 (pool policy and migration), integration 142; the
  other suites pass.
## 1.9.1 0-8-8-0 Angus + 2-6-8-0 Gt. Northern (Claude), RELEASED pre-release, NOT installed, 19 Oct 2026

James asked for both to go into 1.9.0, then a release and reinstall. Released
as 1.9.1 instead, to keep the 1.9.0 release (memory: keep every version's release).
- New WheelRearranger.BigMallet.cs:
  - 0-8-8-0 (ID 41) and 2-6-8-0 (ID 42) use the Mallet method: each engine is a
    scaled native drivetrain plus its static cylinder assembly, and the rear
    engine is a native four-axle clone (ConfigureEightCoupledDrive).
  - 0-8-8-0: native four-axle front engine; cylinders +0.6 m; no trucks.
  - 2-6-8-0: the 2-6-6-2's three-axle front (ConfigureMalletDrive and
    PreShift) over its leading truck; no trailing truck.
  - Last driver's rear edge = the 2-2-2-2-2's trailing wheel rear edge
    (rear-bogie local -3.7 minus 0.355).
  - Scale is solved per layout. Hand estimates (UNVERIFIED): 0-8-8-0 about
    0.75 (1.06 m drivers), 2-6-8-0 about 0.79 (1.12 m).
- Extra powered axles: "[axle] drive mallet front" (a clone of driveAxle2 in
  the front bogie, 0-8-8-0 only) and "[axle] drive mallet rear" (driveAxle4
  clone in the rear bogie). They are added to the powered list through
  BigMalletExtraAxles and deactivated in SuspendBigMalletGeometry, which
  SuspendMalletGeometry calls. No wheelslip sparks for those two axles.
- The rear static clones are kept per layout and rebuilt if the scale changes.
  GeneratedPresentationRoots includes them. IsDuplexId includes 41 and 42.
  DescribeDriverGroups covers both. Settings spawn08880 / spawn2680 (off).
- Tests: catalogue 1,389 (43 IDs), integration 142; the other suites pass.
- ZIP SHA256 518ee44ead60d4d3cc7ed0c6078827e3ceb4bd5356c14f6b6e8713ddcc4d41f6, DLL A8955B6F...C724A.

## 1.9.0 0-2-2-2-0 triplex added (Claude), RELEASED pre-release v1.9.0 (tag 2692287), INSTALLED, 19 Oct 2026

Installed from the released ZIP (9D6AA0EF...4BEF; DLL 5BAD585B...6F5A == build, Info.json 1.9.0).
1.8.1 backup: build/runtime-trial-1.9.0/installed-backup-20261019-213413. Settings kept.

James asked for a "less meme" triplex: 0-2-2-2-0, with the first cylinder
further forward, the engines spaced further apart and bigger drivers. It is a
new layout (his choice, ID 40); the 2-2-2-2-2 is unchanged. He also asked to
keep the Franklin setup.
- Catalogue: s02220 = 40, "0-2-2-2-0 Articulated Triplex", 0 carrying wheels,
  articulated, SupportedCount 41; spawn02220 setting (off).
- ArticulatedSimulation: TrucklessTriplexId 40; IsTriplexId(38|40) runs the
  middle engine; IsDuplexId includes it. The frame split also uses IsTriplexId.
- Triplex.cs: ApplyTriplexGeometry(trucks) / MeasureTriplex(trucks).
  - No trucks: front = Franklin front + 0.6 m, gaps 0.25 m, last driver's rear
    edge at the 1.8.1 trailing wheel's rear edge (rear-bogie local -3.7 minus 0.355).
  - Hand estimate (UNVERIFIED): scale about 0.91, drivers about 1.30 m.
  - The middle and rear static clones (guides, calipers, springs) are now
    rebuilt when the layout scale changes (triplexAssetsScale), because they
    were mapped only once. Assumes the sources are back in the prepared pose
    at that point, as the original code already did.
- Tests: catalogue 1,374 (41 IDs), integration 138, GraphProbe checks the
  triplex IDs; the other suites pass. Validate.FrontCouplingMounts.ps1 does
  not compile under Windows PowerShell 5 (C# 6 syntax in WheelArrangement.cs,
  needs pwsh 7, which isn't installed). That was already the case before this change.
- Walschaerts on the triplex: answered James's question; no change made.
- ZIP SHA256 9d6aa0ef1d067f6a5b944a2f9b820387e2eda1d0403a11ad1c180ab9fb1b4bef, DLL 5BAD585B...6F5A.

## 1.8.1 triplex bigger drivers, 4-14-4 spacing (Claude), RELEASED pre-release v1.8.1 (tag f31e01f), INSTALLED, 19 Oct 2026

Installed from the released ZIP (C859CEFA...24482; DLL 1EE31FE8...DE54 == build, Info.json 1.8.1).
Backup of the previous install (1.7.4; 1.8.0 was never installed): build/runtime-trial-1.8.1/installed-backup-20261019-211820. Settings kept.

James asked for:
- Triplex: bigger drivers (about the size of his yellow circles, ~1.3 m), by
  shortening the pistons and guides, moving the engines forward and moving the
  trailing truck back.
- 4-14-4: trailing truck back, drivers back and cylinders fixed. He chose
  "cylinders stay, stretch rods" over moving the cylinders as well.

New: RearrangedS282/DriveRetarget.cs
- Draws the drive's renderers through proxy children (the original
  MeshFilter.sharedMesh is set to null, and LODGroups are pointed at the proxies)
  so the native and Franklin clips keep animating untouched.
  - Shift role: the cylinder side, drawn Shift further along drive z.
  - Stretch role: rods. The far-end vertices beyond half the pin length are moved
    by Shift; if the mesh isn't readable, an AssetStudio export in assets/ is used
    instead; if neither exists, the proxy scales z. Each LateUpdate re-aims the
    rod from its pivot at native far pin + Shift.
- Roles:
  - Native Walschaerts (resources.assets dump):
    - Shift: push_rod (crosshead), push_rod_to_cutoff_base (union link),
      cutoff_rod_hinge, push_rod_to_cutoff_rod (combination lever),
      cylinder_rod (valve spindle)
    - Stretch: push_rod_to_connect (main rod), cutoff_to_cylinder_rod (radius rod)
  - Franklin (bundle dump):
    - Shift: cylinder/*, driveshaft_m_mount/*, driveshaft_m/u_joint_f/*, crosshead
    - Stretch: main rod, driveshaft_m/default
- 4-14-4:
  - drive is 0.25 m further back (DriveOffset); calipers, springs and lifting
    arm supports use DriveMap
  - cylinders, cocks, guides, colliders and jets keep Map
  - Bind(drive, +0.25/s)
  - leading truck uses the unshifted driver 1
  - trailing truck at rear-bogie local -3.7 (2-10-4/4-10-4 position, was -3.1)
  - expected gaps: front +0.25 m, rear +0.35 m (UNVERIFIED in game)
- Triplex:
  - TriplexShortenNative 0.45 (crank-boss clearance limit, hand estimate), front
    cylinders +0.25 m, own trailing limit -3.7
  - expected scale about 0.835, drivers about 1.19 m (was 0.99)
  - guides, colliders and outlet maps use Cylinders(e); Bind(drive, -0.45) on
    all six drives
- Unbound in Suspend4144Geometry and SuspendTriplexGeometry (before the
  articulated LOD restore).
- NOT seen in game. Risks: rod pin alignment, crosshead against the crank
  boss, front cylinders against the pilot, LOD proxies, and the radius rod if
  it falls back to scaling.
- Validation: integration 136, additional 1,891, catalogue 1,366, geometry
  3,947, effects 9,014, simulation, power diagnostics and curve drag 150 all
  passed. ZIP SHA256 c859cefa6a5b5630891bd97548b58741b80d110e9623b6c378d52cfdebf24482.

## 1.8.0 curving Stage 2: curve drag + squeal (Claude), RELEASED pre-release v1.8.0 (tag cffd00c), NOT installed, 19 Oct 2026

James approved Stage 2 (drag), then squeal, with derail risk after the flange
term and the triplex measurement are fixed. Next: "fix some of the arrangements".
- 1.7.4 capture (power-20261019-182301-729.jsonl, capture limit hit at 789 s
  with 4 locos) covered the 2-2-2, 2-6-6-2, 4-14-4 and triplex down to about
  35 m radius. The stock 2-8-2s did not move. 2-2-2 full-regulator start:
  slipped in 13 of 14 samples, so the 1.7.3 cap is working.
- The diagnostics' sampled fit is noisy, and the triplex was wrong: single-axle
  pivot fits gave 4.8 deg on straight track, the middle driver sat about 2 cm
  off the centre line, and the rear engine was unmeasured. Stage 2 therefore
  uses an analytic steady-curve model instead (RearrangedS282/CurveDragModel.cs):
  - each group self-aligns (least squares) with its own drivers
  - scrub = W*mu*(angle-0.1deg)
  - flange: only with 3 or more flanged drivers in a group, fitted to the flanged
    axles, with the excess over play ramped over 20 mm at 0.05 W per flanged axle
  - single-axle groups and the 2-2-2 give zero
  - extra = max(0, model - stock 2-8-2 model)
  - the half-gauge slip term was dropped, because it scaled with driver weight
    rather than wheelbase and cancels across layouts
- Hand-checked numbers (Validate.CurveDrag.ps1, 150 checks):
  - 4-14-4: +1.7 kN at 100 m, +3.7 kN at 80 m, +9.5 kN at 50 m. Rockl for the
    whole car at those radii is 7.8 / 10.9 / 27 kN.
  - Mallet, 2-4-4-2, triplex and stock: 0.
- Runtime (WheelRearranger.Curving.cs FixedUpdate):
  - groups come from DescribeDriverGroups, which is no longer gated on the
    articulated frames (conventional layouts use the car frame)
  - curvature is the mean |sampled| over the bogies, every 0.1 s, smoothed over 0.3 s
  - force is split over the bogies, opposes velocity, ramps to 0 below
    0.1 m/s, and is skipped while wheelslipping, kinematic or asleep
  - squeal: a CarRollingAudioModule.UpdateModule postfix sets
    max(native slowBuildUpStress, ours) on every bogie layer
  - level = extra/weight*12, gated by speed between 0.5 and 2 m/s
  - settings curveDrag and curveSqueal both default to true
  - log line "Curve drag <layout>: ... R100 x kN, R50 y kN"
  - diagnostics "drag" block
- NOT tested in game. Unverified:
  - the force sign
  - per-layout flanged counts for the legacy 10- and 12-coupled layouts
  - whether the squeal level is audible
- Validation: integration 136, additional 1,891, catalogue 1,366, articulated
  simulation, power diagnostics and curve drag 150 all passed.
  ZIP SHA256 06950a6874eb4c7487abc4ba0b46d68c64738fe9ac63c4034d1c1e389461c341, DLL 58A79C3A...05729.

## 1.7.4 curve diagnostics fixes (Claude), RELEASED pre-release v1.7.4 (tag 6c3eaa2), INSTALLED, 19 Oct 2026

Installed from the released ZIP (DLL DB903797...87C93 == release, Info.json 1.7.4).
1.7.3 backup: build/runtime-trial-1.7.4/installed-backup-20261019-175717. Settings kept.

Diagnostics only; driving is unchanged from 1.7.3. James asked for this as the gate before Stage 2.
- Duplicate drivers: DescribeDriverGroups now skips names containing "LOD".
  The 1.7.2 4-14-4 capture listed 11 drivers for 7. The extra four were
  `..._LOD1` siblings, with a mesh other than the flanged one. That cause is
  inferred from the names and Articulated.cs:194, not verified in game.
- Scrub model: the old formula saturated at mu * W per axle once the angle
  reached 0.005 rad. That gave about 16 kN at about 2.5 km radius, and would
  give over 100 kN on tight curves.
  - New per-axle term: mu * W * (max(0, angle - 0.1 deg) in rad + 0.7175 * |curvature|),
    which is the along-track part of the sliding friction.
  - Hand estimates (UNVERIFIED): 4-14-4 at R=150 m gives about 2 kN; straight
    track gives about 50 N.
- New "reference" block per sample: mean bogie radius and the Rockl whole-car
  curve resistance (650/(R-55) N/kN for R >= 300 m, 500/(R-30) N/kN below),
  for calibrating against. At 150 m that is about 4.5 kN for 111 t.
- Session assumptions now log angleDeadbandDeg, halfGaugeM, rocklReference
  and the drivers source.
- Calibration capture still needed. Use the same track and speed for each run:
  straight, broad, tight and S-curve, on the 4-14-4 or 2-12-2, the 2-8-2, the
  2-6-6-2 and the triplex.
- James's clip of bogie knocking: I couldn't identify the cyan overlay. The
  motion looks like the native bogie (track-placed) against the body
  (on joints). Not changed.
- Validation: integration 136, additional 1,891, catalogue 1,366, articulated
  simulation and power diagnostics passed. ZIP SHA256
  59bc28ce11d8ccdb9f8766fc1c3f369d4c00273a5bdd0c5bd2b01cfbc77356e4, DLL DB903797...87C93.

## 1.7.3 driver-weight cap (Claude), RELEASED pre-release v1.7.3 (tag 6d43064), INSTALLED, 19 Oct 2026

Installed from the released ZIP (DLL 0B742F4B...F275E4 == release, Info.json 1.7.3).
1.7.2 backup: build/runtime-trial-1.7.3/installed-backup-20261019-175014. Settings kept.

James approved option A with these targets after the 1.7.2 runs.
- 1.7.2 results: the structural test passed all 40 layouts
  (Mods/RearrangedS282/stage2-runtime-checks.json, version 1.7.2.0, subject
  L-050). Power capture power-20261019-163827-276.jsonl has only the 4-14-4
  (216 s, low regulator, no full-regulator start). Player.log has no
  RearrangedS282 errors; its exceptions come from GlobalShopController.OnDestroy
  and DoubleTrack OnSaveGUI at quit.
- Curve diagnostics findings (diagnostics only, UNFIXED):
  - The 4-14-4 rigid group lists 11 driver entries for 7 drivers. The first
    four z positions appear twice, flanged in one copy and blind in the other.
    Likely cause: LOD1 meshes also match "s282_wheels_driving_" (unverified).
  - proposedScrubN is about 15.9 kN at about 2,500 m radius, from an angle of
    attack of only 0.05 deg against SaturationAngleRad .005 (CurveDiagnostics.cs:20,136).
  - Stage 2 needs a deadband or a curvature-scaled term before any force is
    applied.
- Cause of the grip: DV gives each axle TotalMass / NumberOfAxles
  (TrainMassController.cs:32). Mass stays at 111 t, so short layouts carried
  28-37 t per driven axle. The 2-4-0 and 2-6-0 had a starting pull / grip ratio
  of about 1.03.
- Change: ConventionalPhysics.MaxDrivenAxleLoadKg caps the load per coupled
  group at 18 t (single), 19 t (four-coupled) and 20 t (six-coupled); larger
  groups are uncapped. DriverWeightFraction = min(equal share, cap / CarMass).
  Articulated layouts are capped per engine. The cap only ever lowers the
  fraction. WheelRearranger.Conventional.cs feeds it to the existing
  FrictionForDriverWeight; mass is unchanged.
  - Results: 2-2-2 18 t, 2-4-0 / 4-4-0 / 4-4-2 38 t, 2-6-0 / 4-6-0 60 t,
    triplex 54 t; everything else unchanged.
  - New log line "Driver weight: X t on N driven axles (equal share Y t of 111 t)".
- Validation: integration 136 (1.7.3 DLL), additional 1,891, catalogue
  1,366, articulated simulation and power diagnostics passed. The new cap
  cases are in Validate.Stage2.ps1, but that script needs pwsh 7, which is
  not installed. They were run instead in a net8 harness (scratchpad
  capcheck), and all 16 cases plus the rejects passed.
- ZIP SHA256 3fe6302d35d40157f3ae418618792c005064c308afb3a5f80f97dc2727fd0cf4,
  DLL 0B742F4B...F275E4.

## 1.7.2 fixes + curve diagnostics Stage 1 (Claude), INSTALLED 19 Oct 2026

Installed from the released ZIP (DLL 1A18CC94...6D352 == release). 1.7.1 backup:
build/runtime-trial-1.7.2/installed-backup-20261019-151609. Settings kept.

RELEASED as GitHub PRE-release v1.7.2 (tag on 54b8690), ZIP SHA256
02f33c477d4d5a71ee6c7007d933a39d48cd18fb894afc43ed3dbc832be96550. Install from that ZIP.

James's 1.7.1 run: triplex had no visible cylinders, he wanted trailing trucks
back and bigger wheels; 4-14-4 cylinder-cock jets far forward. Also approved
curve-dynamics Stage 1 (diagnostics only). Feasibility report (read-only, local
only): evidence/curve-dynamics-feasibility-20261019/FEASIBILITY.md.
- Player.log render reports: Franklin cylinder enabled=False lodGroups=0 after
  the Mallet had been used (fine before). BindArticulatedLods now: native coarse
  = LOD1+ renderers not drawn as detail by ANY group and not under generated
  roots (GeneratedPresentationRoots); recorded once; restored in reverse. Franklin
  renderers (front pairs, rear Franklin/branches/reverse, triplex clone copies)
  are force-enabled when shown (EnableGeneratedRenderers).
- Triplex back to s22222 "2-2-2-2-2" with trailing axle at the 12-coupled
  position (1.7.1 log: scale .755 with no truck; expected ~.70 now).
- Jets: PacificHudsonAffine.EmitterCentreLocal (mean of leaf descendants).
  4-14-4 holders, Pacific/Hudson position-only holders and articulated
  ParticleBinding leaves now move so the emitter centre maps, not the origin
  (CylCockSteam/WaterDrip origins are at the car origin; LMS MoveCylinders).
  Pacific fixture holders are childless, so it does not exercise this path.
- CurveDiagnostics.cs (Stage 1): PowerDiagnostics.CurveSampler/CurveSessionInfo
  hooks (null in the PowerDiagnostics fixture). Per sample: bogie track/span/
  yaw/sampled curvature, inter-bogie curvature, native stress/slowBuildUp/
  derailBuildUp/threshold/rolling N/wheelslip, groups from
  WheelRearranger.DescribeDriverGroups (visible left drivers; flanged = mesh):
  rigid fit, pivot fit (+-6 deg, 25 steps), effective-wheelbase estimate,
  proposed (unapplied) scrub/flange N. No offline fixture for the geometry
  (native Bogie/PointSet); validate with a capture.
Validation: all suites pass (integration 136, catalogue 1,366). No game run.

## 1.7.1 triplex rework (Claude), INSTALLED 19 Oct 2026

Installed from the released ZIP itself (no rebuild): installed DLL equals the
release DLL 4595C66A...EB354. 1.6.1 backup (DLL 4D787B79...):
build/runtime-trial-1.7.1/installed-backup-20261019-124612. Settings kept.

RELEASED as GitHub PRE-release v1.7.1 (tag on e4e9c3a):
https://github.com/james-taplin/dv-rearranged-S282/releases/tag/v1.7.1
ZIP SHA256 610549780e9e69d90a8516b73761e64acea083ed6830ec8d596bcc97fb353dd4,
DLL 4595C66ADA6445EBAA9D9F701C8A28B1378751B1B543B213EFC9F70A364EB354.
Install from this exact ZIP (not a rebuild) so installed == released.

James saw the 1.6.1 triplex in game (0.53 scale: tiny wheels, very long rods)
and chose "2-2-2-2-0, short rods". ID 38 renamed s22220 "2-2-2-2-0 Articulated
Triplex" (toggle spawn22220; NumOfNondriven 2). Engines now reuse the 2-4-4-2
Franklin engine (EnsureArticulatedAssets first; frontFranklin on the front drive,
cloned franklin_type_b(Clone) on middle/rear clones, FranklinBAnimCtrl, duplex
short main rod, two-axle rod hidden) showing only driver 2. LMS cylinders/lifting
supports hidden; static parts per engine = duplex guides (0.065, articulated
guide meshes), caliper 2, spring 2. MeasureTriplex uses the Franklin cylinder
front (FranklinCylinderFront, ~10.64 drive z) to driver 2, rear limit = rear edge
of the 12-coupled trailing wheel, no trailing truck; expected scale ~0.70.
Front AnimatorPortReaders paused like the 2-4-4-2. MalletAnimatorMirror now
copies only parameters both controllers declare. Atlantic-size (2.07 m) drivers
were explained as impossible for three engines in the S282 frame.
Validation: all suites pass (catalogue 1,366, integration 136). No game run.
1.7.0 (4-14-4) was NOT installed: the game was still running.

## 1.7.0 4-14-4 built (Claude), NOT installed, 19 Oct 2026

RELEASED as GitHub PRE-release v1.7.0 (tag on e5b976b):
https://github.com/james-taplin/dv-rearranged-S282/releases/tag/v1.7.0
ZIP SHA256 9b0639d086f3a44af5a6993ab8853796bfa197615c3a0de6124f1de6bace979f,
DLL 1D49A40919F31862947800F584DE7AE70EE27002CAA869D0B50E6C17791B22BC. Not installed.

New ID 39 = s4144 "4-14-4", CONVENTIONAL (IsConventionalId includes 39; random
default off). WheelRearranger.FourteenCoupled.cs: Measure4144 scales the native
drive + LMS cylinder assembly (MapMalletPart, front cylinder face kept) so seven
drivers (native z plus z7 = z6 - pitch) fit ahead of a two-axle trailing truck
ending at the 12-coupled trailing position; leading axles placed just ahead of
driver 1. Expected scale ~0.68. Seventh driver wheels/axle ("[axle] drive 4" in
rear bogie)/rod (12-coupled six rod copied one pitch back)/caliper/shoe/sparks
are created lazily (Ensure4144Assets) and hidden by Suspend4144Geometry, called
in TrySwitchConventional and RestoreConventionalStock before GeometryState
restore (not inside Additional.cs: its fixture compiles that file alone).
Spark/brake-heat arrays re-added per apply (prepared restore resets them).
All 7 physical axles aligned to the visible right wheels. Particle holders
position-mapped only. Powered list now includes driveAxle7.
Validation: catalogue 1,366, integration 136, all suites pass. No game run.

## 1.6.1 duplex rear guide fix (Claude), INSTALLED 19 Oct 2026

RELEASED on James's request as GitHub PRE-release v1.6.1 on the fork:
https://github.com/james-taplin/dv-rearranged-S282/releases/tag/v1.6.1
(annotated tag on a7d9aeb; asset RearrangedS282-B99-1.6.1-Test.zip, SHA256
432a4c0ea0225f6f28a7310778be51e7d1f9c219cc3028fb4655f3437af35b00, contains the
installed DLL 4D787B79...). Marked pre-release: no in-game acceptance yet.

Installed on James's go-ahead; install rebuild of the same clean source gave DLL
4D787B79806A99FC1B306E4936EEFCC8CC7BFFC2058023DCBFD063B24B7F1D76 (integration
checks rerun on it). 1.5.1 backup: build/runtime-trial-1.6.1/installed-backup-20261019-114824.

James's 1.5.1 run: 0 LODGroup warnings and both duplexes show front cylinders,
so the LOD fix is confirmed in game. Black structure behind the cab remained.
Render report only flagged rear spring clones because LMS meshes keep the whole
body vertex buffer (renderer bounds span the loco); report now measures drawn
triangles of renderers under the articulated roots. Root cause found in code:
ApplyArticulatedGeometry set rearGuideL/R.localPosition = (0,0,-4.58), an
upstream value authored for clones parented to splitLoco (upstream line:
lrCrossheadBracket = Instantiate(lfCrossheadBracket.gameObject, splitLoco)); the
port's clones live under articulatedRearRoot, ~4.88 m offset. Now
SplitBodyOffsetInRearRoot() + (0,0,-4.58). Rear brake clone height had the same
mix-up (y=0 in the wrong frame). Includes all 1.6.0 triplex work.
DLL ADC24E9F1074A6030EA3503A7642A5AC9493E17C2B859E2E80A053D82D1FAB90; suites pass
(graph 40, catalogue 1,358, integration 130). Not verified in game.

## 1.6.0 2-2-2-2-2 triplex built (Claude), NOT installed, 19 Oct 2026

Built while James tests 1.5.1 (installed); do not install over his test.
James also had the CR-style smoke deflectors mod moved to the Recycle Bin.
New ID 38 = s22222 "2-2-2-2-2 Articulated Triplex" (NEW_LAYOUTS_PLAN Phase 2).
Simulation: every S282 graph now has a dormant third engine
rearrangedMiddleSteamEngine (copy of the rear aux definition), sums take
MAIN/REAR/MIDDLE, damage/health IDs appended, LoadLegacyState adds empty
rear AND middle state when missing. Apply(38) activates rear+middle; other
articulated IDs activate rear only. IsDuplexId literal 31/32/37/38.
Effects: bindings now hold one copy per secondary engine (rear, plus middle
when active); outlets via TryMapArticulatedOutlet(world, engineIndex 0/1/2).
Geometry (WheelRearranger.Triplex.cs): MeasureTriplex picks the largest scale
fitting 3 engines (cylinder front -> main driver d3 = 5.48 m native) between the
front cylinders advanced 0.5 m and the 12-coupled trailing-axle position;
~0.53 expected. Single-driver engines show d3 only (no coupling rods). Front =
native drive; middle/rear = cloned drives + LMS static clones. Middle frame has
its own Position/Aim constraints (aim = 0.4 Axle_F + 0.6 rest point);
articulatedPin is reparented under the middle root so the front hinge follows.
Frame split is now N-way (pins list). Powered axles: driveAxle1 front, 2 middle
(both front bogie), 3 rear; 5 axles total. Powered-count formula generalised to
sum of middle Whyte entries (TrySwitchConventional and Stage2RuntimeChecks).
Validation: GraphProbe updated (40 checks), RandomLifecycle 1,358,
ArticulatedIntegration 130, all other suites pass. No game run.
DLL 9DD5FAEF86F4E83BA9EC9AA4B56CC3EE444CD6773ED13F9D013BF3E4799E9DBD.

## 1.5.1 articulated LOD fix + render report built (Claude), INSTALLED 19 Oct 2026

Installed on James's go-ahead; install rebuild of the same clean source gave DLL
C74CE6E714E322F3726D5ECE3DC1B76DC8D2909C643B95C816AB8BC0B1066A43 (125 integration
checks rerun on it, pass). 1.5.0 backup: build/runtime-trial-1.5.1/installed-backup-20261019-110514.

James's 1.5.0 run: Mallet renders and works well. 2-4-4-2/4-4-4-4 show a large
black structure jutting out behind the cab, and the 4-4-4-4 front cylinders are
missing (his 2-4-4-2 screenshot still shows a front block). Player.log after every
articulated switch: ~555x "Renderer ... registered with more than one LODGroup"
pairs. Old BindArticulatedLods put EVERY front/rear-root renderer into EVERY
LODGroup under the body, including each drivetrain's own group and the (inactive)
rear drivetrain clones' groups, and also promoted active native LOD1 proxies to
detail. Fixed: one owner per renderer (native membership, else nearest enclosing
group, else body group); native coarse proxies excluded. Bundle check (UnityPy on
assets/s282_drivetrain): franklin_type_b and _4444 both contain an active
"cylinder" at drive (1.09,.71,9.49); _4444 root at z .35. So the asset is fine;
duplicate LOD culling is the leading (unconfirmed) cause. Jutting structure NOT
identified: not in franklin/branch/reverse prefabs (all drive-frame z 4.9..9.8,
y > 1.4). New LogArticulatedRenderReport logs every visible renderer reaching
>0.5 m behind the subframe's rear end, plus front Franklin cylinder state.
Offline suites pass (none exercise LOD binding). DLL CA2823AA...87808D0.

## 1.5.0 2-6-6-2 Mallet built (Claude), INSTALLED 19 October 2026

Installed on James's go-ahead (game closed). Install rebuild recompiled the same
committed source (git clean) to DLL 824D3FDA396AA96D76ADEBD09C1B110BACE5A2224508F4DD0A0827EEA77447E2
(earlier candidate hash 2316F171... differed only by recompilation); the 125
compiled integration checks were rerun against the installed DLL and pass.
1.4.5 backup (DLL 3ADA1658...): build/runtime-trial-1.5.0/installed-backup-20261019-100918.
No game run yet.

James's 1.4.5 structural run: 37/37 pass, no Pacific/Hudson "moved only" log line,
so the 1.4.4 shear came from a particle holder (now position-only). Everything
rendered right. Open: 2-2-2 did not slip at full regulator/cutoff. By native
WheelslipControllerSystem/AdhesionControllerSystem it compares force per powered
axle with mu*weightPerAxle; 2.8 m halves force but 1 of 3 axles doubles weight per
driver vs stock 4 of 6, so threshold ~= stock. No power capture of that drive yet.
Real singles carried 15-20 t on the driver; offered a lighter 2-2-2 driver load.

New ID 37 = s2662 "2-6-6-2 Articulated Mallet" (plan: NEW_LAYOUTS_PLAN.md, Phase 1).
WheelRearranger.Mallet.cs:
- Each engine = native drivetrain + LMS static cylinder assembly (cylinders,
  cocks, guides, lifting supports, calipers 1-3, springs 1-3, cylinder collider
  shells) under ONE body-frame map: Y/Z scale 0.75 about rail-level origin, then a
  Z offset. Front offset keeps the native cylinder front face; rear cylinders sit
  0.1 m behind the front last driver. All measured at runtime (MeasureMallet).
- Drivers 1-3 per engine; d1 moved to equal pitch so the existing three-axle rods
  (6-coupled d2-d4 pieces) are shifted forward one pitch. Main rod = native d3.
- Rear engine: lazily cloned drivetrain (behaviours stripped) + static clones under
  articulatedRearRoot, tracked in malletAssets (separate from articulatedAssets).
  MalletAnimatorMirror copies the front animator's parameters (cutoff/reverser).
- Physical axles: driveAxle1/2/6 front bogie, 3/4/5 rear; leading = native, one
  small trailing axle placed behind rear d3. 8 axles, 6 powered.
- Pin = midway between front d3 and rear cylinder front; same constraints.
- ArticulatedEffects outlets go through WheelRearranger.TryMapArticulatedOutlet;
  legacy 2-4-4-2/4-4-4-4 offsets unchanged when the Mallet is not active.
- ArticulatedSimulation.IsDuplexId now 31/32/37 (literal: graph probe compiles alone).
Unverified in game: rod shift direction, rear valve gear following cutoff, rear
crosshead/guide alignment, trailing wheel clearance to cab, effects positions.

Validation: native build 0/0. RandomLifecycle PORTED from Windows PowerShell
Add-Type (needed PS7 $PSHOME\ref paths; failed here even on HEAD) to a dotnet
project; 1,350 checks incl. ID 37. ArticulatedIntegration updated: 125 checks.
ArticulatedSimulation 40+31, ArticulatedEffects 9,014, geometry 3,947,
Pacific/Hudson 29,973, Additional 1,891, RestoredFeatures 286, RollingAudio 198,
PowerDiagnostics 81, Delta 129 all PASS. Validate.FrontCouplingMounts.ps1 still
uses Add-Type and cannot compile the catalogue under Windows PowerShell 5.1
(fails on HEAD too); not run. Build.RuntimeTrial default version 1.5.0.

## 1.4.5 candidate built (Claude), NOT installed, 19 October 2026

Claude took over from Codex. James approved both fixes, build only, no install.

Articulated IDs31/32 ROOT CAUSE (confirmed from LocoMeshSplitter 1.1.9 decompiled
S282ALOD0MeshSplitter): s282a_subframe_left/right is only the firebox section,
vertex z -4.287..-1.596 (body space). The main bar frame is s282a_frame_left/right,
z -1.67..6.48. The pin (~midway between front driver2 and rear-engine driver1) lies
ahead of the whole subframe, so the front clip was empty. Fix:
WheelRearranger.Articulated.cs now clips/hides ArticulatedSplitFrames =
s282a_frame_left/right; subframe stays boiler-fixed and visible. The error message
now includes the local pin and mesh bounds. s282a_frame_box_front still boiler-fixed.

Pacific/Hudson IDs4/5: the six-driver (1,s,s) stretch skews any part rotated off the
drive X axis. Offending part was not logged in 1.4.4 (still unknown). Fix in
WheelRearranger.Walschaerts.cs: particle holders (CylCockSteam/WaterDrip/CylinderCrack)
are position-only (pivot mapped, native rotation/scale), matching LMS translate-only.
Any other part that would shear falls back to position-only and logs
"Pacific/Hudson: <path> cannot be stretched without shear; moved only." once per
car. Decompose still throws on shear; new TryDecompose returns false instead.
Check Player.log for that line to learn which part sheared.

Missing trailing wheels: NOT fixed. Structural report shows both generated trailing
axles active and physical on every x-x-4. Pattern: every flagged layout puts one
trailing axle at rear bogie_car z -3.5/-3.7 (4-4-4 line ~2685, x-10-4 ~2717,
284Big ~2706). Estimated to land ~1m behind the loco frame end, under the tender.
That estimate ASSUMES Axle_R sits near driver 4 (not measured). Asked James to
check a 4-4-4 with the tender uncoupled.

Validation: native-reference build 0 warnings/errors. Pacific/Hudson fixture PASS
29,973 checks (outlet assertion changed to pivot-only; new TryDecompose checks;
fixture Stubs.cs gained a Main.Logger stub). Articulated clipper fixture PASS 3,947
(unchanged clipper; it never used real frame coordinates). Integration/IL fixtures
not rerun. No game run. Packaged docs (RUNTIME_TRIAL_TESTING/ARTICULATED_TEST_STATUS)
unchanged and still describe 1.4.4.
DLL SHA256 3ADA165826D113278C5E76E69EC897D2BB8B13DB610887E28C14F4D489D15C42.
ZIP: C:/Users/james/Desktop/Derail Valley Mods/RearrangedS282-B99-1.4.5-Test/
RearrangedS282-B99-1.4.5-Test.zip. Candidate: build/runtime-trial-1.4.5/.
INSTALLED 19 Oct 2026 on James's go-ahead (game closed). Installed DLL hash matches
above; Info.json 1.4.5; Settings.xml kept. 1.4.4 backup (DLL F0729132...)
at build/runtime-trial-1.4.5/installed-backup-20261019-082743. No game run yet.

GIT: work committed as 7bb0fbf on branch b99-port and pushed to James's PUBLIC
fork https://github.com/james-taplin/dv-rearranged-S282 (remote "james"; origin
is still upstream pjschinske). Fork default branch is still upstream main.
evidence/ is gitignored except the offline test fixtures and the two files the
scripts read; logs, tester material and video stay local. Commit new work on
b99-port and push to james. Do not push to origin.

## 1.4.4 runtime structural acceptance FAILED, 19 October 2026

James ran the expanded structural trial. Installed/report version1.4.4 (assembly
1.4.4.0), installed DLL SHA256 F072913235E6C500B81044218F312D6B3AE926FFE263CD731757C156A969AEBE.
Latest report: subject L-026, control L-062, 33/37 passed. Failures:
- IDs4/5 Pacific/Hudson: PacificHudsonAffine.Decompose rejects the native support
  transform with "Pacific/Hudson native support cannot be mapped without shear."
  This is the new polish mapping, not evidence that native Walschaerts cannot work.
  Offending part/matrix is not recorded, so precise numeric/transform cause is unknown.
- IDs31/32 articulated: BindSplitArticulatedFrame rejects s282a_subframe_left with
  "S282 frame does not span its measured articulation pin" (empty clipped half).
  Inspect actual source mesh coordinates versus measured pin before changing the
  guard; this does not establish a gap in native articulation/traction support.

Both errors repeat in Player.log, including outside the automated run. Failed
conversions restore their captured stock state exactly. IDs34/35/36 (2-6-0,
2-4-0, giant222) pass structural checks; giant222 reports2.8m diameter and0 driver
colliders. These are stationary checks, not driving/geometry acceptance.

James also saw missing trailing wheels on unspecified layouts. Existing axle counts
are correct on passing entries, but the test does not capture mesh visibility/LOD.
Do NOT dismiss the observation as a false alarm from passing counts. Rear small
axles clone the front axle and explicitly disable axleF_modelLOD1; inspect actual
LOD0/LOD1 renderer states and a working native wheel before claiming a cause/fix.
James clarified that several x-x-4 layouts appeared to have only one trailing axle,
especially layouts he recalled as x-12-x/x-14-x, plus the entry immediately before
4-6-0. The latter is confirmed from the current name table as4-4-4 Reading (ID2),
which should have two trailing axles. Current build has no14-driver choices; its
2-12-2/4-12-2 correctly have one trailing axle by design. Preserve this distinction:
do not treat all one-axle observations as missing geometry or invent extra layouts.
Distance/LOD not specified. ID2 and the other x-x-4 paths explicitly activate both
secondRearAxle and thirdRearAxle at distinct positions; this rules out a simple
missing SetActive call in those helpers but not later state/renderer/LOD defects.
Active settings use small_wheels for x82/x102. Visual failure remains unresolved.

The extra power capture contains52 stationary samples over2.9s, stock Mikado only,
no escaping simulation-update faults; it provides no driving/articulated evidence.
Raw report, both Player logs and power capture preserved with SHA256 manifest in
evidence/structural-1.4.4-20261019. Other mods log unrelated startup/cleanup errors;
none explains the four explicit RearrangedS282 conversion failures. No production
code edits, build or reinstall performed during this inspection. Prior offline
passes below do not supersede the failed runtime acceptance.

## 1.4.4 expanded trial installed, 18 October 2026

CURRENT USER SCOPE COMPLETED IN SOURCE/TEST PACKAGE: polish Walschaerts on4-6-2
and4-6-4 first; add2-6-0 and2-4-0 together; add a deliberately unfinished2-2-2
with massive non-colliding drivers to explore geometry; install a test build.
1.4.4 is installed. No game acceptance claimed. No triplex/4-14-4/new steam model.
All older features and original IDs0-33 retained; other previously disclosed parity
differences (custom stock appearance, old derail-threshold transpiler) remain.

Native-first gear decision: inspected native High/Med/Low cutoff clips and LMS
geometry. All three clips bind crosshead/linkage scale, so the old Show6Drivers
crosshead scale edit is overwritten by the Animator. The scaled animated mechanism
is internally coherent; its static cylinders/guides/supports were not receiving
the same mapping. New WheelRearranger.Walschaerts.cs captures authored part frames
relative to the native drivetrain before preparedGeometry. Apply maps existing LMS
LOD0/LOD1 cylinders/chests, cocks, guides, lifting bearings, particle holders and
native walk/item cylinder shells to the same six-driver Y/Z1.2879/Z-2.1 mapping.
Four per-car cloned LMS dry-pipe meshes blend the cylinder mapping toward the fixed
boiler end. Shared source meshes remain immutable; native animation controllers,
ports, cutoff, phase and physics/cylinder parameters are unchanged. Long cab reach
rods and mechanical lubricator linkage remain hidden, including unused support at
both LODs. This is first geometry polish, not a new complete custom linkage retarget.
Pipe tests pin highest/lowest-Y endpoint extrema; they do NOT prove every vertex
of an angled socket ring remains rigid. Final pipe seating needs game inspection.
Setup catches optional preparation failures; only requested polish rejects if not
ready. Geometry snapshots restore poses/meshes on switching/pooling; per-car meshes
dispose on deletion. Local geometry disposal now precedes OnDestroy's Unity-null
TrainCar guard so separately allocated meshes are still freed after car destruction.

Settings.polishedPacificHudsonValveGear=true enables polish for IDs4/5, independently
of the saved old show6CoupledValveGear=false. Disabling the new setting and reapplying
restores old behaviour; other six-coupled layouts keep the old global show6 toggle.
No unselected stock car/prefab/shared material is edited by this new geometry path.

New stable appended IDs34=s260(2-6-0 Mogul),35=s240(2-4-0),36=s222(2-2-2 Giant-driver
clearance prototype); SupportedCount37. Original ID0 remains s440; do not activate
the old commented s240=0 line. Existing original standard helper remains0-30 for
historical tools; IsConventionalId includes the new IDs, active save/radio paths use
IsSupportedId. New260/240 reuse Show6/Show4Drivers plus one leading axle/no trailers.
Their retained native physical axles/PoweredWheel anchors align to the visible
right-side driver transforms before native axle-array/physics commit. New normal
choices default true in the random pool; total default ordinary pool33. Random
spawning still defaults off; giant222/articulated/404 choices remain separate opt-in.

WheelRearranger.Additional.cs giant222: default2.8m tread diameter, normal UMM slider
2.1-3.6m, finite/clamp enforcement on apply. Reapply after adjustment. It creates
only two GameObjects/MeshFilters/MeshRenderers plus the unchanged LateUpdate rotation
follower, using shared native flanged-wheel mesh/material. No Collider/Rigidbody/
PoweredWheel clone or enlarged native body collider. Parent drive frames retain
unit magnitude/mirrored X; radial renderer scaling preserves gauge width and moves
the tread centre by radius delta to retain rail height. Source third-wheel animation
drives angle; prototype centres are independent, so resizing does not cause orbit.
Original drivetrain meshes/unfinished rods/calipers are hidden, while Transform
hierarchy/Animators and all six oilcup providers remain active. Only native axle3
is powered, reindexed0; spark1 is moved to its contact and other spark pairs hidden.
Single leading/trailing small axles give total3. Native force/radius/RPM/adhesion
adapter is reused, with unchanged boiler/cylinder calibration and mass. This is a
clearance prototype, not calibrated final handling or rigging. Giant visual wheels
can visibly intersect body/equipment without generating wheel collision forces;
native body collision still exists. Flange outer diameter exceeds selected tread
diameter. No giant LOD/rod redesign or native body cutouts added. Diameter is a
global reapply setting; saved data retains arrangement identity, not per-car diameter.
SuspendAdditionalGeometry precedes every prepared/stock snapshot restore; Dispose
destroys only owned visuals, never shared native meshes/materials. Native wheel
Transform references restore through the existing GeometryState.

Stage2RuntimeChecks now covers37 entries. Prototype fields report configured tread
diameter, native radius consistency and zero descendant colliders; they are not a
measurement of outer mesh diameter or triangle clearance. Normal selection needs
no structural button. Test runner still requires two stationary STOCK S282s.

Validation, all passing (managed geometry tests are not Unity/game acceptance):
- Native B99-reference1.4.4 Release build:0 warnings/errors.
- Pacific/Hudson actual-source fixture:29,970 assertions across96 native cutoff-clip
  poses and1,344 native dry-pipe vertices, including both LOD supports, signed affine
  mapping, endpoint extrema, source immutability, reuse/restoration/disposal and
  optional failure paths. build/pacific-hudson-valve-gear-validation/validation.json;
  evidence/pacific-hudson-valve-gear/native-animation.json contains native samples.
- Additional actual partial + native rotation-follower fixture:1,891 assertions,
  including resize2.1/3.6/2.8/2.1, rail-centre pose, rotated bogie alignment, native
  providers/animators, no component cloning, dense sparks, restoration/disposal and
  second-car isolation. Many are repeated transform assertions, not independent
  game scenarios. build/additional-arrangements-validation/run-20261018-210421-901/.
- Actual catalogue/settings/save-policy fixture:1,340 checks with37 ID roundtrips.
- Compiled integration:120 checks, original0-33 IDs stable, appended routes34-36,
  optional gear wiring, minimal new visual components/no extra simulation, matching
  native targets, unchanged ConventionalPhysics/PoweredWheelRotater IL.
- Native-algorithm physics/explosion fixture:286 checks/28 transitions/11 explosions;
  extended to one powered axle. Actual adapter source unchanged from1.4.3.
- Rolling/brake audio:198 assertions/33 axle transitions incl total3,10 old-cache
  failures reproduced; coupling:608 assertions incl four37-entry sweeps.
- Final source hash receipt, package/root/license/backup/settings/native integrity
  audit pass. No source changed after the tested candidate was built.

Installed/candidate/ZIP DLL SHA256:
F072913235E6C500B81044218F312D6B3AE926FFE263CD731757C156A969AEBE
ZIP: C:/Users/james/Desktop/Derail Valley Mods/RearrangedS282-B99-1.4.4-Test/
RearrangedS282-B99-1.4.4-Test.zip
ZIP SHA256:49F6FC104F0D0BFF607901A12016859852B74FD2494CC5718E069F02A0CE83A8
34 packaged files/56 source hashes;14 original credit/license/notice files match
the audited1.3.3 package byte-for-byte. Original drivetrain bundle unchanged.
Build receipt and package-audit.json: build/runtime-trial-1.4.4/.
Previous installed1.4.3 preserved at installed-backup-20261018-211147, DLL SHA
5F6D61E2968A07E5CC60D49395DCE2C876D4E662BBFB7F78189F04DC6230331E.
Existing Settings.xml and installed native Assembly-CSharp preserved, as are the
protected1.3.3/1.3.6 baselines. No game launched, saves/other mods edited or upload.

James tests live Unity rendering, cutoff/reverse, pipe socket seating, near/far
appearance, actual oilcup interactions, driving/braking and save/reload. For222,
try2.1/2.4/2.8/3.2/3.6m tread diameters, reapply stopped/regulator closed and record
the first body/frame/equipment overlap. Test stock/new/articulated transitions and
second-car isolation. RUNTIME_TRIAL_TESTING.txt has the ordered current checklist.

## Historical 1.4.3 restoration trial installed, 18 October 2026

CURRENT: James requested restoration of added-axle exploded-model/material treatment
and the original 4-0-4 Drivers not found. Both are implemented, packaged and installed
as 1.4.3. All original enum IDs 0-33 are now selectable. This does NOT certify game
acceptance or restore the two other disclosed parity differences (custom stock
appearance and the old derail-threshold transpiler). No new locomotive designs.

Native-first evidence: installed B99 ExplosionModelHandler source and the actual
resources.assets LocoS282A serialized configuration were inspected, plus Car Changer's
existing explosion integrations. B99 replaces only an exploded boiler-tubes
placeholder; the S282 body/running gear uses materialSwaps. There is no replacement
stock drivetrain/body prefab to recreate. Do NOT revive Spawner.ExplodeDriveAxles or
the obsolete FixExplodedModel/index1/static guard. Native source and targeted binary
asset probe/results: build/b99-inspection/ExplosionModelHandler.cs,
inspect_explosion.py, native-s282-explosion.txt. The initial broad typetree attempt
could not deserialize stripped custom MonoBehaviour fields; the final probe checks
the exact MonoScript class and parses the 316-byte handler payload completely.

New B99ExplosionMaterials is one plain per-car registration owner, no MonoBehaviour
or Harmony hooks. It finds the existing body's material-swap entry by renderer
ancestry, copies the per-car array and appends one initialized native material-swap
entry. Existing entries and their lazy caches are untouched. It seeds inactive
original body gear and registers generated MeshRenderers (including inactive LODs)
after successful arrangement commits, excluding owned Delta/effects presentation.
New articulated parts are discovered after their lazy creation; repeated conversion
does not duplicate entries. Native HandleExplosionModelChange and repair restore
exact material references; no shared Material properties or prefabs are mutated.
Seeding inactive original drivers matters if first exploded as a 4-0-4: native lazy
discovery otherwise misses all original drivers for later powered arrangements.
The native geometry/constraints remain in place during explosions. No new wreck
mesh authoring, separate physics/animation controller or exploded-body clones.
Manual switching now requires a repaired locomotive, matching the pre-existing
automatic-load condition. Already-exploded saved choices remain pending until
native repair and other safe-switch conditions permit conversion.

4-0-4: catalogue count34, stable ID33/name retained, leading/trailing geometry reused,
ShowNoDrivers now called by the active geometry branch with B98 sim writes excluded.
Four carrying axles, no powered wheels. B99PhysicsState supports the explicit zero
case without weakening the positive-driver friction formula's guards. It preserves
slide friction and rolling/mass allocation, disables only the native DrivingForce
component, clears its stale force and zeroes native powered-axle inputs. The optional
engine-braking reader is detached while unpowered to avoid native force/0; its exact
Port reference and original DrivingForce.enabled are restored on powered selection,
stock, rollback and pooling. Native boiler/auxiliary/brake simulation is retained;
no cylinder dimension/cutoff change or custom force solver. Shared type data unchanged.
Existing native PoweredWheelsManager empty-array iteration/save/repair was inspected.
Settings adds spawn404=false; manual selections/save metadata accept33; random
defaults remain the31 conventional choices. Stage2RuntimeChecks now runs34 entries,
captures native traction enable/engine-braking reader state, checks the zero-driver
state and material registration. It does not explode locos or prove game visuals.

Validation, all passing:
- B99 native-reference Release build: 0 warnings/errors.
- Validate.RestoredFeatures.ps1:257 assertions,24 transitions,11 explosion/repair
  cycles. Executes actual B99PhysicsState, B99ExplosionMaterials, ShowNoDrivers and
  installed native ExplosionModelHandler/DrivingForce algorithms plus wheelslip
  properties/reset, with visibility changes only. Covers extreme torque/no traction,
  zero-driver engine-braking safety, exact stock references/coefficients/restoration,
  second-car isolation, hidden drivers, inactive LODs, late rear parts, repeated repair
  and pre-existing native caches. Managed Unity/entity/dispatch stand-ins, not Unity
  scene/rigidbody integration or rendering. Evidence: evidence/restored-features-tests,
  build/restored-features-validation/validation.json.
- Random/catalogue/save actual-source fixture:1239 checks incl ID33 roundtrip/opt-in.
- Compiled/native integration:42 checks. B99PhysicsState is deliberately changed;
  its old byte-for-byte comparison is replaced by executed transition tests, while
  existing conventional conversion-math/rod methods still match the earlier binary.
- Coupling:560 assertions including four34-layout sweeps; rolling/brake audio:136
  assertions/22 transitions. Those source helpers are unchanged.
No game launched, saves modified, RF work performed, other mod edited or upload made.

Installed/candidate/ZIP DLL SHA256:
5F6D61E2968A07E5CC60D49395DCE2C876D4E662BBFB7F78189F04DC6230331E
ZIP: C:/Users/james/Desktop/Derail Valley Mods/RearrangedS282-B99-1.4.3-Test/
RearrangedS282-B99-1.4.3-Test.zip
ZIP SHA256: A1EADD85C59BBD5902321DE22DA767B054733C7AFF4B8A050C452DCD66E7A6E4
34 packaged files,54 source hashes, original drivetrain bundle and licenses/credits
retained; no settings/log/diagnostic/PDB files. Final build/install receipt and package
audit: build/runtime-trial-1.4.3/. The REAL previous1.4.2 backup is
installed-backup-20261018-203755, SHA C8B7BD5C10C205402188562FEA8D1D4E922F2506D348C3C79485CE528C00D2D4.
The later203826 backup contains intermediate1.4.3 before a UI label correction.
Protected1.3.3 DLL/ZIP and1.3.6 ZIP retain their documented hashes. Assembly-CSharp
SHA remains CF00E7C8CECB1A30BA93AD8DEC926090FFC9CB876365027020F8C3BC64F1D0B4.

User test: restart; 4-0-4 selection/no propulsion/towing/brakes, saved identity,
stock/powered recovery; explode4-0-4 FIRST, repair, select2-12-2, explode/repair,
then lazily create both articulated layouts and repeat. Inspect near/far burned
parts, normal material restoration, active wheel counts and second-car isolation.
RUNTIME_TRIAL_TESTING.txt contains the ordered disposable-sandbox checklist. Game
acceptance is outstanding; neither older structural passes nor managed tests prove it.

## Historical feature-parity clarification before new designs, 18 October 2026

James asked whether EVERY original feature is now ported before starting new
features. Read-only comparison against upstream HEAD README/source and current
active build paths: do not claim literal100percent parity. All33 original powered
arrangements are implemented (31 conventional plus2 original duplexes deliberately
changed to articulated), along with random choices, native per-car persistence,
style/valve-gear settings and the listed B99 fixes. Runtime acceptance of the new
articulated/audio work remains outstanding; implementation is not a game pass.

Explicit remaining legacy differences/gaps:
- ID33 zero-driver4-0-4 novelty remains unavailable.
- Selecting stock2-8-2 restores the native snapshot; upstream's customised stock
  Mikado appearance/settings path is not reinstated for that stock selection.
- Original ChangeDerailThreshold transpiler is not registered in the active B99
  Main branch. Native B99 handling/axle physics is used; calculating/logging
  derailModifier does not mean the legacy arrangement threshold is applied.
- Upstream FixExplodedModel() adds extra axle roots to explosion material swaps,
  but its only call is in the excluded legacy SwitchWheelArrangement branch.
  Custom exploded-model/material presentation parity is not complete/certified.
  Do not confuse this with disabling native damage or the new native rear engine's
  tested damage/save-state handling.

These are disclosed remaining parity items, not an invented prerequisite to new
design work. James has expressed interest in moving on; no new designs specified
or implemented by this audit. No source/build/install changes; installed1.4.2
remains the same. Own new designs/three-cylinder variants are still future work.

## 1.4.2 native brake-audio fix installed, 18 October 2026

James supplied a beta tester's two Player logs, brake video and F4 screenshot from
C:/Users/james/Downloads. They identify Rearranged1.3.6, RF1.13.4, B99build2702,
Utilities1.1.2. Player(1).log is an EXACT1,852,218-byte prefix of Player(2).log;
one cumulative session, not two tests. Larger log SHA256:
88EEFEE4FA132D1BE06CE87C2B5946246595B137C9FE4F4FE95D51364A3EF4BB.
Evidence notes: evidence/thomas-followup-20261018/BRAKE_LOG_FINDINGS.md,
F4_FINDINGS.md, OVERSPEED_LOG_FINDINGS.md; decoded video frames/overview in video/.
Frames establish multiple locos/states, not an independently heard missing hiss:
AAC is present, but this model runtime cannot listen to audio input. Do not claim
audio listening or a live game reproduction. Original downloads remain unchanged.

USER STEERING: James will recommend not using Realistic Firing and requested
continued fixes for unrelated issues. RF-specific compatibility/overspeed work is
set aside. Do not modify or investigate RF further without a renewed request.
The general passive failed-tick diagnostics already prepared remain useful and
are included. No RF code, settings or installation were changed; RF-disabled
testing is a recommendation, not an automatically applied local setting.

CONFIRMED native brake-audio defect: larger log contains3,247 rolling-module
IndexOutOfRangeExceptions. Reporter/native Assembly-CSharp MVID matches exactly;
offset0xc4 accesses lastJointStep[i][j]. Changing physical axle counts left this
cache at the old per-bogie sizes. The original mod's ResetJointSoundValues call
was in the old build-gated branch; the active B99 conversion missed it. Native
S282 audio updates Rolling before Brakes with no per-module catch, so this error
prevents brake-audio updates while physical brakes can still work. No-JointsSpan/
LOD gates explain intermittent indexing. This supersedes the old vague modstack
or geometry-snapshot audio hypothesis. Exceptions lack a car ID, so do not assert
which locomotive threw every instance or claim an audible runtime pass.

Small native fix: new WheelRearranger.Audio.cs refreshes exact-owner initialized
CarRollingAudioModules under car AND detached interior, deduplicated, via existing
ResetJointSoundValues. Calls follow final axle/physics commits in
TrySwitchConventional and stock/rollback/pool restoration. Native Initialize owns
late audio creation. No new audio controller, hook, clip or custom cache solver.
Actual helper plus native reset/update/dispatch fixture passed136 assertions across
22 transitions, reproducing6 original failures and verifying brake-update recovery.
Evidence: Validate.RollingAudio.ps1, build/rolling-audio-validation/validation.json.

F4 screenshot clarification: circled WATER/COAL boxes are boiler/firebox level
bars, whose numeric text is intentionally empty. Tender quantities30.0water and
10.0coal are visibly present on the right. Native prefab does include fill bars;
persistently missing fill remains a valid separate unresolved symptom. RF glass%
is not the same raw native boiler indicator range. No speculative HUD patch.
Check native bars with RF disabled before extending this investigation.

The later0-8-0 infinite-steam run has6,413 RFInjectorRig.StepOutlet nullrefs
escaping Boiler.Tick before engine refresh. Native physics can reuse old positive
torque independently. This is a source-supported HYPOTHESIS for that run, not a
confirmed overspeed cause; no speed/RPM/torque trace or exception car IDs provided.
RPM input may still update while crank phase/chuff/torque outputs freeze. Infinite
steam is explicitly reported for this new run only, not retroactively for400+km/h.
Utilities also has255 missing-field errors; no third-party workaround implemented.

General diagnostic gap corrected: original Update postfix missed failed ticks.
Same target now uses a finalizer which captures normal/fault completion and
returns the EXACT original exception. Schema2 gains simUpdateFailed and bounded
simUpdateFault(type/message),200/512 chars, retaining10Hz and off/no-I/O behaviour.
Diagnostic I/O/logger failures cannot mask the original fault. This changes no
physics/ports and suppresses no native errors. It samples faults, not an exhaustive
exception counter. Actual-source fixture passed81 checks (previously53), including
exception identity, normal/off/throttled paths and I/O+logger failure. Report:
build/power-diagnostics-fault-validation/run-20261018-192654-294/validation.json.
PowerDiagnostics source hashAB8E941F2EEB6E6896BE20E8C0CF30AC90F8C4F31296CCD124008DF169F7222E.

Installed1.4.2DLL SHA256:
C8B7BD5C10C205402188562FEA8D1D4E922F2506D348C3C79485CE528C00D2D4.
Desktop ZIP: C:/Users/james/Desktop/Derail Valley Mods/RearrangedS282-B99-1.4.2-Test/
RearrangedS282-B99-1.4.2-Test.zip.
Final ZIP SHA256:71D6AEF23D54CF12316F044AB6AF3EF6F2158D2CB4994AE39DAD795D9FE93A37.
Receipt: build/runtime-trial-1.4.2/build-receipt.json, runtimeValidated=false.
Previous1.4.1 installation is in installed-backup-20261018-193023 in that folder.
Latest193225 backup is the intermediate1.4.2 before the native-only doc update.
Build.RuntimeTrial default1.4.2. Native reference build0warnings/errors;48 compiled/
native integration checks pass against final DLL (build/runtime-trial-1.4.2/
integration.json), including native reset calls, call order and finalizer hook.
Prior native physics/rod methods unchanged by IL comparison. Existing chain/hose,
rods, Delta, both articulated arrangements and save/random features retained.

Audit passed: installed/staged/archive DLL+bundle match, all53 C# source hashes
match receipt,34 files under one UMM root, all27 licence/credit/notice/exporter
files identical to seed. No captured user data/logs/media/settings/game DLLs.
Compiler CodeView debug paths remain embedded (generic B:/GitHub repo and vendor
C:/sources/cecil); no PDB is shipped. Do not claim literally zero absolute paths.
Final doc-only repack verified all4 installed/staged/archive manuals match and
recommend RF-disabled native testing. No further production changes after tests.
No game launch, save changes, external message, upload or Git commit. Runtime
brake listening, lack of new rolling errors and native HUD fill still need tests.

## 1.4.1 coupling/audio trial installed, 18 October 2026

James relayed3 issues from the large-stack/overspeed reporter: sometimes missing
F4 coal/water quantities, sometimes silent independent/train brake release, and
front chain/hose pushed into the frame on converted S282s. Version and whether
this starts on spawn or after switching are explicitly UNKNOWN. No new reporter
log/clip/reproduction. Do not treat James's old local logs as reporter evidence.
Detailed native traces, hypotheses, validation and next observations:
evidence/beta-reporter-three-issues-20261018/FINDINGS.md.

CONFIRMED published1.3.3/current code defect: car snapshot excludes DV's detached
interior. Native VisualCouplerInit.chain/hoses mount roots live there. Each
no-leading MovePilot(-.47m) affects them, but prepared/stock geometry restore
resets only the physical pilot/anchor. currentWA is reset to stock, so the old
positive offset undo is skipped. Seven choices accumulate3.29m rearward drift.
GeometryState now additionally captures/restores ONLY native front chain/hose
mount localPositions. Keep native MovePilot, IK, coupling state, rotation,
activity and parenting unchanged. Structural JSON captures mount positions too.
Native chain visibility re-enable can reset its cached mount; check walk-away/
return manually before extending into any new coupling/visibility workaround.

Separate1.4.0 defect corrected: ArticulatedEffects discovery now scans both car
and detached interior roots, deduplicates controllers and requires exact ready
car/SimulationFlow ownership. This fixes missed already-initialized articulated
rear audio readers. It is not a diagnosis of the standard brake-audio report.
Native brake sounds read actual exhaust/charge flow beneath separate interior,
not wheel RPM/chuff ports. F4 uses separate interior native Indicators; no
confirmed Rearranged cause for either intermittent report. No speculative fixes.

Validation: actual mount helper + native movement model reproduces old drift,
passes544 checks including four33-layout sweeps/stock/pool-style restoration and
unchanged IK/parent/activity/rotation; actual audio discovery/ownership methods
pass9014 assertions/303 scenarios with component-tree stand-ins. Native-reference
build0warnings/0errors;40 compiled/native integration checks pass. These are
OFFLINE checks, not a reporter reproduction or actual game/audio/IK pass.
Reports: build/front-coupling-validation/validation.json;
build/articulated-effects-validation/validation.json;
build/runtime-trial-1.4.1/integration.json. Prior engine/geometry/rod fixtures
remain applicable to unchanged source; no repeat gameplay validation is claimed.

Installed1.4.1DLL SHA256:
40A0FA7BD3684DD9F14BB1CC6011FA0C39783ED5C7CBFC6B9CBA51B945AD1483.
ZIP: C:/Users/james/Desktop/Derail Valley Mods/RearrangedS282-B99-1.4.1-Test/
RearrangedS282-B99-1.4.1-Test.zip.
ZIP SHA256:F824E515DC4BDA7F335D1E84671E3AC56986D629D6AF6CF36C39E807F908FF39.
Receipt: build/runtime-trial-1.4.1/build-receipt.json; runtimeValidated=false.
Previous1.4.0 installation backup: build/runtime-trial-1.4.1/
installed-backup-20261018-092932 (DLL793DEF2A...730D0E).
All31 conventional + both articulated choices retained; runtime instructions/
test-status docs updated. Restart and retest coupling swaps/stock/walk-away and
both articulated engine sounds. Other mods, saves and public ZIPs untouched.
No game launch, Git commit, upload or external message. Build.RuntimeTrial default
is now1.4.1. Earlier entries below describe historical state and archives.

## 1.4.0 combined articulated trial installed, 18 October 2026

James requested BOTH original two-driver-group arrangements together for his
in-game test, overriding the earlier one-at-a-time prototype plan. Implemented
31=2-4-4-2 Articulated and 32=4-4-4-4 Articulated alongside conventional IDs0-30.
No rigid intermediate, new own designs or zero-driver novelty in this trial.
New designs follow after James tests these two. The research/estimates below are
historical and must not be mistaken for outstanding implementation work.

Native-first decision: inspected/reused L74ArticulationPolish.cs from the tested
Heavy Mallet. Native DV bogies retain all four PoweredWheel components as direct
bogie children. Their presentation anchors follow the engine frames. A native
PositionConstraint holds the rear pin; a native AimConstraint follows Axle_F.
No new Rigidbody, custom curve solver, ECS traction/slip group or compound receiver.
Both groups use accepted native shared traction/RPM/slip. Split only the existing
continuous rendered subframes at the measured pin; preserve UVs/material groups.
Whole front gear, cylinders, suspension, brake fittings, oil lines, sparks and
named cylinder walk/item shells move together. Rear frame stays boiler-fixed.

ArticulatedSimulation prepares a per-car native graph before SimController builds
its one flow/save owner. The unchanged main engine plus gated native rear engine
feed typed native torque/intake/exhaust sums; both steam demands reach the boiler.
Dormant rear adds no steam/torque/wear and retains damage/save state; native repair
is observed while dormant. Old saves receive an empty rear state, never a false
native cylCracked key (presence itself means cracked). S282 body/type guard leaves
L74 and other CCL vehicle graphs alone. Rear capacity is a conservative
source-informed native equivalent, not an exact B98 or historical compound claim.

ArticulatedEffects owns rear native chuff/cock readers and authored particle
copies, uses actual rear ports, and restores/deinitializes ownership on switching,
controller unload, pool and deletion. Both water drips and cylinder-crack outlets
follow their engine; the rear crack outlet uses its actual rear crack-flow port.
Shared chimney pulses keep native main pressure response with rear-pressure gate.
No separate chuff plugin changes/install: MultiCylinderChuffFix remains its prior
installed version. Existing six native oilcup circuits/providers stay functional;
rear shares native lubrication, with no separate rear refill circuits added.

Saved arrangement/native radio selection supports both; optional random toggles
for them default off. Diagnostics schema2 adds engines[] with per-engine crank,
chuff, torque and steam flow while preserving old top-level main metrics. Optional
structural test covers all33; James does not need to run it to try both manually.
Prior conventional physics and LateUpdate rod methods remain byte-equivalent IL.

Build/package/install: Build.RuntimeTrial.ps1 -Version 1.4.0 -CandidateDirectory
build/articulated-candidate -Install. Final DLL1.4.0.0 SHA256:
793DEF2A35DF90F344AA7649DA00253F27CFB71D088324267239642B6E730D0E.
Installed into B:/SteamLibrary/steamapps/common/Derail Valley/Mods/RearrangedS282.
Desktop ZIP: C:/Users/james/Desktop/Derail Valley Mods/
RearrangedS282-B99-1.4.0-Test/RearrangedS282-B99-1.4.0-Test.zip.
ZIP SHA256: 192BBBE213B6F63F801EBE7A91340C87B87C11CF270EB93CCFFB2412D7EFDBEE.
34 files under one RearrangedS282 root, including original assets/s282_drivetrain
(SHA256 44A1B9EAE0DDA58BE97B9AEDF3877522FBFFAEC060FE02A8385158E095BDB22D).
Original MIT, credits, notices, 11 licences and 13 exporter tools byte-match the
audited1.3.3 seed. No settings/logs/OBJ/game DLLs/private paths in the ZIP.
Receipt: build/articulated-candidate/build-receipt.json; runtimeValidated=false.
Original1.3.6 installation backup: installed-backup-20261018-021652 in that folder
(DLL89F99281247B67B1EB806465AE6C681F73620BE9CF93ED237CBA0D581FCDC538).
Final rebuild backup021945 contains the intermediate1.4.0 trial, not1.3.6.
Protected1.3.3 DLL/ZIP and1.3.6 Beta2 ZIP hashes rechecked unchanged.

Validation: native-reference build0warnings/0errors;40 actual native engine
constructor/Tick/port/save checks;31 actual-source graph/lifecycle adapter checks;
3,947 actual-clipper assertions/205 partitions with Unity mesh/vector stand-ins;
53 diagnostic checks;1,233 catalogue/random/save policy checks;40 compiled/native
metadata integration checks;6,720 simulated rod poses plus compiled IL inspection.
The final integration and rod reports match the installed final DLL hash.
Evidence: build/articulated-simulation-validation/native-run-20261018-021247-745/;
build/articulated-geometry-validation/validation.json; build/articulated-validation/;
build/random-lifecycle-validation/validation.json. Independent native init/save
and final package audits found no material blocker. This is OFFLINE validation,
not actual Harmony dispatch, native constraints evaluation or a game driving pass.

Runtime acceptance belongs to James's combined test: curves/reverse/clearance,
gear/outlet alignment, sound balance, stock switching, save/reload, pooling/deletion,
oilcup interaction and second-car isolation. Fused native boiler/body collision
hull remains fixed; only named cylinder shells move. Frame split is uncapped.
Realistic Firing overspeed remains unresolved; no claim of an overspeed fix here.
RUNTIME_TRIAL_TESTING.txt and ARTICULATED_TEST_STATUS.txt ship in the mod folder/ZIP.
No game launch, save changes, commits, uploads or modifications to other mods.

## Revised requirement: every two-driver-group layout articulated, 18 October 2026

James explicitly changed the plan: he does not want the original duplexes restored
as rigid engines. ALL arrangements with two driver groups must be articulated,
including 2-4-4-2 and 4-4-4-4, from the first prototype. Use the tested L74 method.
Historical descriptions of the original rigid implementation below are evidence
about the asset donor only; they are not the target or an intermediate milestone.
Also add our own multicylinder variants in rigid and articulated forms. Rigid
variants can have one coupled driver group with three/four cylinders; articulated
variants have two engine groups. State cylinders per engine and per locomotive.

Updated evidence/full-arrangements-research-20261018/FEASIBILITY_AND_PLAN.txt in
place. First milestone is articulated2-4-4-2, provisionally3-5developmentdays.
Both original two-group layouts articulated/fullcataloguebeta approximately
8-15days total plus gameplay iterations. The old1-3/5-10day rigid-first estimates
are superseded. A new2-6-6-2 family follows on the shared articulated foundation.
Ownsix-cylinderarticulatedoption may use2nativeN=3engines (3+3) after native3cy
mesh/phase/audio is proven; no generic one-engineN=6 assumption. Actual variant
selection/details remain planning, not implemented work.

Concrete extra work: whole engine assembly follows measured hinge/aim, including
cylinders/gear/rods/visual anchors/oil/drain outlets and moving collision; joints
for pipes crossing frames. Preserve native bogie contact-axle ownership/discovery.
CurrentGeometryState does not snapshotparent/siblingownership, so articulated
switch/rollback/stock/pool restoration must add explicit hierarchy and owned
constraint cleanup. Curve/reversecurve/points/clearance checks are acceptance
criteria for every two-group layout. Existingacceptednative traction/slip method
remains the base; no new independentgroup/ECS orcompoundreceiver prerequisite.

Documentation update only. No implementation, build, install, game launch or
changes to L74/Derailroader/chuff source or installed1.3.6 in this turn.

## Full catalogue and multicylinder research, 18 October 2026

James requested research into all original arrangements including duplexes,
Mallet/articulated additions, new multicylinder/chest meshes and integration with
his chuff fix. Research report:
evidence/full-arrangements-research-20261018/FEASIBILITY_AND_PLAN.txt.
No new runtime implementation, build, installation or game launch in this pass.

IMPORTANT CORRECTED DECISION: James explicitly confirmed that he tested the Heavy
Mallet in game and its slip, adhesion, power and articulation worked very well.
Use that accepted implementation as the native reference for the base port.
Codex initially elevated independent driver-group slip into a prerequisite James
had not set. This assumption was withdrawn. Although native source shows one
traction/slip group, that is not evidence the tested behaviour is unacceptable.
Do not repeat this tangent by requiring custom ECS physics or a true compound
receiver before restoring the arrangements. Further detail is optional and needs
a demonstrated problem or explicit requirement. Reuse L74's native graph,
typed torque/intake/exhaust sums, existing adhesion and constraint articulation.

Confirmed catalogue: IDs 0-30 conventional, 31=2-4-4-2 rigid duplex,
32=4-4-4-4 rigid duplex, 33=4-0-4 zero-driver novelty; all 34 were selectable.
2-6-6-2/2-6-8-0/4-6-6-4 were commented ideas, not finished original Mallets.
Current zero-powered path rejects an empty set; restore novelty deliberately.
Original compiled drivetrain bundle, CAD/Blender/prefabs/animations are retained.
Its editable Unity mesh/GUID/material dependencies are incomplete: test existing
bundle first; existing runtime OBJ importer may suffice for new static chests.

Native B99 explicitly supports three cylinders with 120-degree lead/six events,
making a distinct three-cylinder 4-12-2 a useful first new profile. Constructor
caches arrays, area, volume, phase and chuff count: do not just mutate public
numCylinders/bore/stroke after construction. Safe per-car native definition and
activation/switching is the first technical spike, preserving stock snapshots.

Chuff source/candidate0.2.0 found at C:/Users/james/Desktop/Derail Valley Mods/
Leader/plugin/MultiCylinderChuffFix. Installed remains0.1.0. Candidate matches
exact native event-port ownership, uses matched native Hz/count and adapts running
loops;106fixture cases/1560loop checks recorded, actual Unity audio unverified.
It does not create/register readers, phase maps, particles or drain outlets.
Our future integration must own/deinit secondary readers once per audio instance
and use actual per-engine events, not summed integer CHUFF_EVENT counters.
Do not install/change the separate chuff project during source research.

Suggested delivery: original2-4-4-2 prototype1-3development days; bothduplexes/full
originalcatalogue beta5-10days total plus gameplay iterations. Actual3cyprototype
another2-4days plus art/polish; firstL74-style articulated familyanother3-7days
after commonfoundation plus curve/clearance tests. These are provisional effort
estimates, not elapsed guarantees. True compound receiver, independent slip
groups, custom4/6cyphasing or triplex are optional separate scope. RFoverspeed
remainsunresolved; RF first-engine-only superheat attachment is a multi-engine
compatibility check, not a diagnosed cause. Keep baseline/native and RF-on tests.

Installed1.3.6DLL andBeta2ZIPunchanged. No saves, L74, Derailroader, chuff code,
Git commits, external messages or uploads changed/sent by this research.
The Desktop ZIP was independently renamed to Rearranged S282 B99 Fork 1.3.6b.zip
in its existing Beta2 folder; hash still matches the canonical Beta2 ZIP above.
Research did not perform the rename. Use the actual filename for future links.

## 1.3.6 Beta 2 prepared for James's upload review, 18 October 2026

James requested an explanation of the tests and confidence plus a UMM/Nexus-ready
ZIP to consider uploading. Prepared locally; nothing uploaded or published.
Package.RuntimeBeta.ps1 repackages the existing hash-verified 1.3.6 trial ZIP,
without rebuilding or changing the installation. New player README, changelog,
test-status document, updated optional-mod credits and companion Nexus draft are
under release-copy/1.3.6-Beta2. The old 1.3.3 release copy and seed are unchanged.
UMM numeric version remains 1.3.6; Beta 2 is the release label. DisplayName now
says Conventional Beta, with original author and JimTaplin credit retained.

Canonical ZIP: release/1.3.6-Beta2/RearrangedS282-B99-1.3.6-Beta2.zip.
Desktop copy: C:/Users/james/Desktop/Derail Valley Mods/
RearrangedS282-B99-1.3.6-Beta2/RearrangedS282-B99-1.3.6-Beta2.zip.
ZIP SHA256: 0B2E1B36BA525D0BA38A7140E03F37893D163A972261CF7A4AD1656197847440.
DLL remains 89F99281247B67B1EB806465AE6C681F73620BE9CF93ED237CBA0D581FCDC538,
matching installed 1.3.6. Archive is 1,638,554 bytes with 34 files under the
single RearrangedS282 root. Original MIT, 13 exporter dependencies and 11 licence
files byte-match the audited 1.3.3 seed. No game assets, logs, settings, personal
paths, diagnostic output or Nexus draft inside the ZIP. Independent final audit
also passed. CHECKSUMS.txt, release-manifest.json, TEST_STATUS.txt and the Nexus
upload draft accompany the Desktop ZIP; upload only the ZIP as the mod file.
Explorer was launched with the ZIP selected; the Nexus text tab was queued.

This is packaging-ready for a clearly labelled testing beta. No new 1.3.6 game
validation is claimed. Explained 6,720 rod poses as 56 scenarios x120 frames,
actual source with Unity stand-ins plus installed IL inspection, rather than
6,720 in-game tests. Good technical confidence in correcting the phase lag, but
no defensible probability or visual confirmation of the original symptom yet.
Delta's 129 checks also use stand-ins; actual hooks/render/save/delete await game.
The earlier in-game structural pass belongs only to 1.3.3. Realistic Firing
overspeed remains unresolved, with diagnostics included. No steam-power fix,
speed cap, duplex support, new build, install or Git operation was performed.

## Installed 1.3.6 rod fix verified, 18 October 2026

James asked whether the first beta report (2-12-0 rods separating from the last
two axles at high speed) is addressed in this version. Inspected the actual
installed 1.3.6 DLL, SHA256
89F99281247B67B1EB806465AE6C681F73620BE9CF93ED237CBA0D581FCDC538.
The helper has LateUpdate, no Update, and copies localRotation as a quaternion.
Its compiled method matches the original 1.3.4 rod candidate. SetupDrivers binds
all four added wheels: fifth/sixth left follow fourth left, right follow right.
Independent source review found no competing added-wheel pose writer.

Validate.RodTiming.ps1 now accepts an assembly and report path. Against the
installed DLL it passed 6,720 source/stand-in frame-sequence checks at 20, 60,
100, 120, 200, 376 and 462 km/h, 20/30/60/144 fps, both directions. The old
Update helper lagged in all 6,720 moving poses; LateUpdate matched all poses.
Report: evidence/beta1-high-speed-rod-report/installed-1.3.6-validation.json.
These checks establish inclusion and frame-sequence logic; extreme speeds only
exercise synchronization mathematics. They do not prove actual Unity scheduling
or rendered rod attachment. No new 1.3.6 in-game visual retest is available;
latest inspected log loaded 1.3.5 and structural report predates this version.
James still needs to inspect both sides of a fast 2-12-0, ideally also 4-12-2,
under steady running and wheelslip. No DLL, package or installed file changed
during this verification, and no reinstall is needed.

## Delta compatibility1.3.6 installed for testing,18 October2026

James authorized fixing Delta trailing truck compatibility and reinstalling.
Implemented inside existing RearrangedDLL; optionalreflectionintegration with
S282ModelTweaks1.1.0/DVCarChanger1.5.4, no newharddependency, no thirdpartymod
DLL/bundle edits or shipped meshes. MainexplicitregistersDelta bridge. Package
LoadAfter includesGauge,DVCarChanger,S282ModelTweaks toensuretypesloadedfirst.

The bridge intercepts exactDeltaTrailingTruckAxles only. NativeCarChanger
baseapply/materials/Config/save/repaint/remove lifecycle retained. SMT's own
whole-bogie swap/reset bypassed, preventing6->5poweredregistration and5->6state
arraycopy failure. NativebodyHideTransforms suppressed onlyforDelta_body within
scopedbaseApplyprefix/finalizer, so hiddenpart restoration nevercontaminatesstock.
NewDeltaTrailingTruckVisuals render-onlycopies existingloadedDelta wheel/frame
ontoONEactiveunpoweredrearaxle; wheelinheritsnativeaxlespin,framefollowsAxle_R.
Sourcewheel.459m matchesnativeWRCradius byYZscaling, gaugeXpreserved. No axle,
poweredstate, physicsports/radius/RPM edits.0or2trailingaxles suppressDelta
presentation butretainCarChangerselection. Switchingbackresumesgraphics.
Clear/Suspendrestoreoriginalrendererenabledand4nativeLMSbodyactiveflags;
inactivecopiesdetachedbeforedelayedDestroy. Allownedbody/graphicsmarkedand
excludedfromGeometryState snapshots. RearrangerSuspendbeforeconversion and
Refreshfinally(success/rollback); poolclearsgraphicsbeforenativestockrestore.
ClearownerusesReferenceEquals sodeadoldowner cannotclearanewbinding. Cleanup
contained/idempotent duringearlydelete,remove,repaint,OnDestroy;explosionhide/
unexploderefresh included. NativeaudioearlyreturnfollowedbyoldDeltaexception
explainsreportedquietghost, thoughreporterstacktracewasnotavailable.

Files: DeltaTrailingTruckCompatibility.cs,DeltaTrailingTruckVisuals.cs;
Main.cs/WheelRearranger.Conventional.cs integration;Build.RuntimeTrial.ps1
versiondefault1.3.6+versionedoutputfolders/optionalLoadAfter;updatedtestchecklist.
Buildoutput:build/delta-compatibility-candidate/bin. DoNOToverwritebuild/stage3.

Installed89F99281247B67B1EB806465AE6C681F73620BE9CF93ED237CBA0D581FCDC538
(version1.3.6). Backup1.3.5previousfiles:
build/delta-compatibility-candidate/installed-backup-20261018-011009.
Desktopzip:C:/Users/james/Desktop/Derail Valley Mods/RearrangedS282-B99-1.3.6-Test/
RearrangedS282-B99-1.3.6-Test.zip. InstalledDLL/packagehashverified.1.3.3published
baseline and1.3.5ZIPunchanged. No game launch/commits/publishing occurred.

Validation:native-referencebuild0warnings/errors;129actual-sourceDelta checks
withUnity/Harmonystand-ins;42diagnostic/1157savedrandompolicychecksremainPASS.
CompiledIL: B99PhysicsState,ConventionalPhysics,PowerDiagnostics,andarrangement
save-policybodiesunchanged;nooptionalassemblyreference/noDeltaPortwrites.
SeeValidate.DeltaCompatibility.ps1 andbuild/delta-compatibility-validation.
ActualHarmonyoutsidegameattemptfailedunderNET8host duringHarmonyinit
(MonoModReflectionHelper.MethodAccessException);noadapterpatchranthere. This
isnotagamevalidationpass. RealUnityrender/Harmony/save/repaint/delete/pool
runtimevalidationstillJames'stask. No furthertestingexpansionnecessarybeforehisrun.

Next:restartgame,fresh/disposablesandboxS282;4-12-2->Deltaapply/remove twice,
repeat2-12-2;reverseorderDeltafirstthenarrangement;save/reload&repaint;delete
Deltaengineandchecknohitbox/soundghost;secondcarisolation;stockrestore/oilcups.
UseRUNTIME_TRIAL_TESTING.txt packagedandinstalled,updatedREADME/checklisttoo.
Oldalreadybrokenin-memoryenginesarenotrepairtargets;restart/freshspawn.
RFoverspeedinvestigationremainsseparate,powerdiagnosticsstillavailable.

## New reporter evidence and Delta truck incompatibility, 18 October 2026

James reports original author is happy for port work to continue; no contact was
sent by Codex. Reporter mod list: Rearranged1.3.3, RF1.12.0, DVCarChanger1.5.4,
S282ModelTweaks1.1.0, CruzLocoTweaks1.2.2, Utilities1.1.2, others. Do not treat
that as the local installed configuration. Reporter says RF cylindersOFF stops
overspeed. ExactRF1.12 released source verified against1.11.2/1.13.2: cylinder
model/patch/adapter/testfiles identical. See new pinned source audit note in
evidence/beta1-overspeed-report/REALISTIC-FIRING-1.12-SOURCE-AUDIT.md.
Do not claim latestRF fixesoverspeed; actualRPM/sliptrace stillneeded.

James explicitly clarified Delta truck issue is SEPARATE fromoverspeed. Repro:
4-12-2or2-12-2 ->applyDelta ->geometrymessedup, cannotremoveDelta ->deletecar
leavesdrivablecollidablelocowithoutsound. In inspectedpackagesDeltaTrailingTruckAxles
is supplied by S282ModelTweaks1.1.0 (Nitro) throughDVCarChanger, notCruzer1.2.2.
Inspection-onlycopies underbuild/cruz-tweaks-inspection; no game/source/install
changes. Bogie changer uses stocksix-axle index5 as trailing and replaces entire
rear bogie. On12coupled replacement has2powered vs our3rear: currentpoweredmanager
becomes5; removalrestores6physicalpowered butcopiescurrentstatearraylength5 into
all6 ->predictedIndexOutOfRange on6thassignment. Removal is also hooked to native
car-destructionevent, so same failure caninterruptdeletion. Reporter log needed
toconfirm exactexception andwhy audio stops. Do not claim runtime reproduction
or that installed1.3.5 fixes this newlyidentifiedcollision. KeepDelta offconverted
12coupledenginespendingcompatibilitywork. Existing1.3.5 installhash unchanged.
User has NOT requested a Deltafix yet; investigation remainedreadonly.

## 1.3.5 runtime trial installed, 17 October 2026

James requested diagnostic capture plus restored random conventional arrangements,
then explicitly selected manual arrangement persistence across save/reload too.
Implemented and installed1.3.5 (no game launch). No duplexes or power fix/cap.
Native B99PhysicsState/ConventionalPhysics method bodies unchanged vs tested1.3.3.
Includes earlier1.3.4 LateUpdate rod candidate; runtime visual confirmation pending.

Installed DLL SHA25623B058D6A9653D02E0C5593A9CA6F1A2CFD2C6903A0E657EECE5B39BE7B35AB1.
Backup previous installed files: build/diagnostics-random-candidate/
installed-backup-20261017-211015. Settings/assets/old diagnostics retained.
Published1.3.3 ZIP and build/stage3 baseline hashes remain unchanged.
Desktop ZIP: C:/Users/james/Desktop/Derail Valley Mods/RearrangedS282-B99-1.3.5-Test/
RearrangedS282-B99-1.3.5-Test.zip. Build.RuntimeTrial.ps1 builds/packages from
hash-locked audited1.3.3 seed, optional-Install; no publishing occurred.
Receipt: build/diagnostics-random-candidate/build-receipt.json.

PowerDiagnostics.cs explicitly registered SimController.Update postfix,10Hz per
activeS282 including native stock ones. Captures road-derived/wheel-port/crankRPM,
slip+slide, targetRPM (indicative timing), torque, force, controls/cutoff, chest/
admission pressure, steam mass/flow, active mod versions, frame/time. JSONL in
Mods/RearrangedS282/diagnostics. Start/stop buttons in UMM; one-second flush,
normal quit stop;30min/100krows/~64MiB limits. Structural tests mutually exclude.
No RF dependency; James will install latest RF himself. Normal0.2s RPM smoothing
and ECS sampling phase can cause transient mismatches. Do not infer opticalRPM.

Settings now load/save and have31 conventional random toggles with corrected
10-coupled IDs. Random opt-in defaultsOFF; onlynewspawnsrollonce, emptypoolstock.
Oldsavecarswithoutpayloadstock; savedmanual/randomchoicespreservedper-car.
ConventionalArrangementState stores namespaced versioned native simCarState key.
ConventionalSpawn hooks TrainCar Start/newlogic/pool and native saves/restore;
manual radio success cancels pendingautomaticchoice. Pendingrestorewaits until
stationary/regulatorclosed/settled; movingload temporarilystockuntilsafe. Unknown
future/corrupt payload retained until explicitmanualchoice. Structuraltest
transientconversions don't update durable selection. Poolprefix restores stock;
rare restorationfailure uses verifiednativeunpooled destruction path after
caller already began deletion. Hotunloadrefused; restartrequired.

Validation: native-reference build0warnings/errors;42 actual-source diagnostics
checks,1157 actual-source settings/identity/save-policy checks,2160 rod poses.
Compiled newDLL LateUpdate and explicitpatchregistrationchecked; noPatchAll.
Tests are managed stand-ins/static/native-API inspection, not game execution.
Runtime save/reload, deferredrestores, pooledreuse, UI, quitflush, RFdriving and
rod appearance still require James's tests. RUNTIME_TRIAL_TESTING.txt is packaged
and installed, also copied as updated readme/manualchecklist. No source commits.

Next: user Ctrl+F10 -> RearrangedS282 -> START POWER DIAGNOSTICS; close manager and
drive, STOP/SAVE afterwards. Separate stock/4-12-2 RFcylindersOFF/ON captures.
Forrandom: expand arrangement/random settings, enable spawn toggle and choosepool.
Forpersistence: disposable save, two distinctmanual/randomarrangements, save/reload.
Then stopcapture, two stockstationaryS282 -> structuraltest; nativeoilcups check.
Inspect resulting JSONL and Player.log, confirm reporter versions/options before
assigningoverspeedcause. Multiplayer sync remains outofscope/unvalidated.

## Realistic Firing read-only cylinder audit, 17 October 2026

See evidence/beta1-overspeed-report/REALISTIC-FIRING-CODE-AUDIT.md.
Found local Downloads ZIP1.11.2; decompiled inspection copies under build/.
Cylinder patch reads native crankRpm directly: no double radius conversion.
It replaces stock admission limit, retaining native capped steam-chest refill.
Offline actual-Core DLL probe plus relevant native chest/chuff logic (300 sampled
cases) gives no sustained positive torque at376/462 under tested assumptions.
Not a full-game reproduction or proof reporter uses this version/options.
Native slip interpolation toward600RPM can under-report crankRPM above that
road RPM; actual slip/RPM telemetry needed. F6 RF Engine page has mean pressure
and steam used, NOT RPM. No mod code/settings/install changed. Installed1.3.3
hash unchanged. Do not blame RF or implement speed cap on this evidence.

## Overspeed report: read-only power audit, 17 October 2026

Supplied66.7s MP4 inspected: evidence/beta1-overspeed-report/video/OBSERVATIONS.md.
Visible remote panel confirms274-282km/h, regulator14%, cutoff71->48->33%,
boiler14.0bar/chest10.9bar in sampled interval. Empty tender/firing stopped;
residual fire remains. No proof of unlimited resources or uphill propulsion.
ENGINE tab/RPM/torque not displayed;376/462/crash not verified from frames.
Onscreen lowRAM warning also appears; origin/crash relationship unknown.

See evidence/beta1-overspeed-report/FINDINGS.md and static-power-audit.json.
Conventional beta retains native steam torque; radius/RPM conversions agree.
No sustained stock-model steam torque at reported376/462 km/h if crank RPM
tracks road speed and stock flow/model remain active. This is an analytic
conditional bound, not an in-game reproduction or terminal-speed prediction.
Installed Utilities1.1.2 no-derail only raises derail stress threshold.
Important: Realistic Firing current1.5.0 includes a replacement cylinder model
on every steam loco, beyond automatic firing. Reporter version/toggle unknown;
do not assume native steam equations apply or blame that mod without A/B data.
Next compare stock/4-12-2 with realistic cylinders off/on; inspect actual RPM,
torque, flow, cutoff, slip and grade. No power fix/cap/build/install performed.
Existing rod timing candidate and tested beta remain unchanged.

## First public beta report: high-speed added-wheel phase lag; 1.3.4 candidate

Reporter: at least 2-12-0 coupling rods detach visually from the last two axles at
high speed. James confirmed no numerical speed or slip-condition details were
supplied. Do not invent a reproduction speed or claim this report reproduced in game.
Report/baseline helper saved at evidence/beta1-high-speed-rod-report/.

Source/native investigation confirms a frame-order defect: added fifth/sixth wheels
use PoweredWheelRotater.Update to copy fourth-wheel Euler angles; normal native
Animator evaluation happens later. Native rods/original wheels advance while added
wheels retain the pre-animation pose. This predicts a speed/frame-rate-dependent
one-frame phase error, matching the symptom. It also applies to ten-coupled layouts.
Native source PoweredWheelRotationViaAnimation updates Animator speed in Update;
Unity execution-order documentation places animation evaluation before LateUpdate.
No custom traction or new animation architecture is needed.

Small candidate change: PoweredWheelRotater.Update -> LateUpdate; copy localRotation
quaternion directly; use Unity != null to handle destroyed source objects correctly.
Compiled method comparison with published 1.3.3 finds ONLY removal of the old Update
callback and addition of LateUpdate changed. Physics, geometry and oilcup method
bodies are unchanged. Clean build, version 1.3.4.0 via command-line override:
dotnet build RearrangedS282/RearrangedS282.csproj -c Release -p:Stage=3 -p:Version=1.3.4
-o build/rod-sync-candidate/bin.

Validate.RodTiming.ps1 compiles actual before/after helper code with minimal Unity
stand-ins, simulates Update -> Animator pose -> LateUpdate, and inspects candidate
IL. 2160 poses pass across forward/reverse, 20/60/100 km/h-equivalent angular rates
and 30/60/144 FPS. Old helper lags on every moving sample; new helper equals current
source quaternion. This validates scheduling logic, NOT native runtime appearance.
No high-speed Unity/game test has been performed; cause remains a strongly supported
explanation of this particular reporter's symptom until an in-game retest.

Candidate test ZIP and instructions:
C:/Users/james/Desktop/Derail Valley Mods/RearrangedS282-B99-1.3.4-RodSync-Test/.
Candidate DLL SHA256 9805D9013F30547F4D05540ACC1E7B7F18E4078FD524C327BE50AB9636847D40.
Includes the audited licence/exporter subset and explicit candidate notes that the
1.3.3 structural pass is not a 1.3.4 pass. No install, game launch or upload occurred.
Published 1.3.3 ZIP and installed DLL remain unchanged. Keep both as rollback.
Root csproj/Build.AllStandard still default to 1.3.3: do NOT rebuild that path over
baseline artifacts with this candidate source. Package.NexusBeta still hash-locks
the published 1.3.3 baseline correctly. Use the separate candidate build path above.

Next test: drive 2-12-0 fast, inspect both sides/final two drivers, forward/reverse,
steady/slipping; check another 10-coupled and all four 12-coupled variants. Rerun
stationary structural regression too. If it persists, obtain video/speed/frame-rate/
mod list; do not assume timing is the sole possible cause. Do not release as a
confirmed fix until visual/runtime retest. User has not requested installing this
new candidate yet.

## MIT fork / dependency-notice audit completed, 17 October 2026

James requested explicit MIT requirements and a complete credits review. Confirmed
upstream 24e24d5 LICENSE unchanged in release ZIP. JimTaplin remains credited for
B99 port; SatiricRug / Peter Schinske for original work; independent fork/no invented
endorsement; MIT-covered port and separately licensed dependencies distinguished.
MIT does not mandate public source/GitHub fork/notification/payment or separate
personal permission for the MIT-granted rights. Nexus/game-EULA requirements remain
separate. No blanket legal clearance was promised.

Corrected a release-packaging gap: retaining the full original 25-file exporter did
not itself establish all third-party notice obligations. Reduced Nexus package to
13 unchanged files actually needed for OBJ mesh export. Removed FMOD, FBX/native
texture libraries, SixLabors libraries and Cecil debug helpers. No runtime code
changed. Both rod OBJ outputs from the extracted final ZIP byte-match the original
full-exporter baseline. Mod DLL retains the exact structurally passed hash.
Full upstream licence texts now shipped in licenses/ with THIRD_PARTY_NOTICES.txt:
version-pinned AssetStudioMod 0.17.3 MIT, CSspv BSD-2, K4os MIT/LZ4 BSD-2,
Cecil MIT, Json.NET MIT, Brotli MIT, .NET MIT and supplementary notices. LZMA SDK
public-domain source identified. No missing licence was silently treated as MIT.

Nexus current official File Submission Guidelines (updated 29 September 2026)
require AI tagging/disclosure. Credits/release copy disclose Codex-assisted B99
code and JimTaplin testing. Publisher notes explain AI-Generated Content for the
substantial generated port code and AI Media if the generated page description is
used; do not imply MIT exempts platform rules or invent limited AI involvement.

Updated Desktop release folder now contains corrected ZIP, updated Nexus copy,
checksums/manifest, MIT_FORK_CHECKLIST.txt and standalone CREDITS.txt. Replaces prior
ZIP at same name. New SHA256:
4A8F1660AAFA1A03A2607B843AB20FB6DE7C625FFD08A94369767DBD8558FCBD.
Still 31 files total (13 exporter files plus licence/notices/docs/mod files).
No upload, installed-game change, commit or push. Package.NexusBeta.ps1 now uses
an allow-list and includes notices; developer trial packaging still keeps its old
full exporter and must not be substituted for the audited Nexus ZIP.

## Nexus beta files prepared for JimTaplin, 17 October 2026

James wants to upload a beta on Nexus and explicitly selected JimTaplin for the
B99 port credit. Prepared a local release only; nothing uploaded or published,
no messages sent, no commit/push, no installed game changes. Original SatiricRug /
Peter Schinske credit and LICENSE retained. Homepage/update-feed fields are omitted
from beta Info.json so a new upload does not direct updates to the old upstream page.
Mod ID stays RearrangedS282, numeric version 1.3.3, title B99 Conventional Beta.
Public release/file label is 1.3.3 Beta 1; no runtime code rebuild/change is needed.

Script Package.NexusBeta.ps1 packages only the exact runtime-tested DLL (SHA256
DA6184725FFB341D13BDAFCC04048F677B973937066144361B5DCF671DABC1DD), original LICENSE,
new README_BETA.txt, CREDITS.txt, blank manual checklist and the original release's
25-file Windows exporter. Validates original archive hash, passed runtime evidence,
31-file inventory, every exporter file against original, license hash and DLL hash.
241 existing offline checks also pass; independent stationary runtime pass remains
recorded separately. No logs/JSON diagnostics/game meshes/game assemblies/duplex
bundle/cache/save/internal handover are included in the playable download.

Nexus ZIP SHA256 C7959EE03068360582E0844267E8B047417ACB80E02D79EA39098CDC4AA897A3.
Project outputs: release/1.3.3-Beta1/.
Desktop upload folder:
C:/Users/james/Desktop/Derail Valley Mods/RearrangedS282-B99-1.3.3-Beta1/.
Files: RearrangedS282-B99-1.3.3-Beta1.zip, NEXUS_RELEASE_COPY.txt, CHECKSUMS.txt,
release-manifest.json. All four Desktop copies hash-verified.
NEXUS_RELEASE_COPY.txt supplies title/summary/description/requirements/installation,
file description/changelog/credits and publisher notes. Explicitly no duplexes,
no saved selections/random spawning, native equal-axle-load approximation, six
native oiling circuits, remaining driving/refill/lifecycle beta test limitations.
User's positive visual report is described only as initial positive feedback.
No new Nexus URL was invented. Original source link is explicitly upstream, not
represented as the unpublished port repository. Existing installed DLL still matches
the tested 1.3.3 hash. User uploads the ready ZIP; source publishing was not requested.

## Manual checklist installed, 17 October 2026

James requested a manual arrangement/oilcup checklist and installation. Added
MANUAL_TEST_CHECKLIST.txt to the repository, future package script, rebuilt ZIP and
installed mod folder. Includes V/M/O/T/D/R results for all 31 arrangements, six-cup
open/refill/close/motion/retention record, loaded slip/sanding, LOD/unload, isolation,
direct-switch stress checks and issue evidence instructions. Updated testing notes
to reflect the confirmed structural pass. Installed DLL is unchanged from the
structurally passed hash DA6184725FFB341D13BDAFCC04048F677B973937066144361B5DCF671DABC1DD.
All 30 packaged installed files verified. Prior passing JSON preserved. Only two
documents were installed/updated. No game launch. Receipt/previous notes/report in
B:\GitHub repo\rearranged-s282-b99\evidence\manual-checklist-install-20261017-010419.

## Confirmed all-standard structural pass: 1.3.3, 17 October 2026

James ran the installed 1.3.3 structural suite. JSON version 1.3.3.0, subject L-021,
control L-052, passed=true, all IDs 0–30 passing with no per-arrangement errors.
Installed DLL SHA256 remains DA6184725FFB341D13BDAFCC04048F677B973937066144361B5DCF671DABC1DD.
Player.log:8763 and UMM log both say All 31 structural checks passed. No RearrangedS282
error/exception line was found in this run.

Independent review of saved JSON: all 31 converted snapshots have expected physical,
entity, powered-manager and powered-port axle counts; all 31 restored snapshots
exactly equal the baseline. External interactables were loaded, and all six native
oilcups were captured. All 31 conversions preserved the cup/provider identities,
sync tags and refill/flow port IDs. Runner success also includes unchanged native
mass/engine/shared type and second-car isolation. Five driver-radius groups appear
as expected: 1.03376 (3 arrangements), .9144 (3), .8509 (3), .712 (18), .6096 (4).

This supersedes the earlier outstanding stationary structural validation. It does
not establish moving cup alignment, opening/refilling/oil consumption, rod/LOD visuals,
loaded pulling/wheelslip/sanding, audio/particles, external unload/reload or repair/
explosion behaviour. Those remain manual/lifecycle tests; persistence is still off.
No source/build/install changes made in this review.

Preserved JSON, Player.log, UMM-Log.txt and review.json:
evidence/all-standard-1.3.3-passed-20261017-005225/.

## Current installed trial: 1.3.3 — drivetrain hierarchy and native oilcups, 17 October 2026

James reported 1.3.2 still failed and added B99 oilcup support to the ongoing fix and
reinstall request. Latest report: version 1.3.2.0, subject L-009, control L-052;
first conversion failed in SetupDrivers at IL 0x004c, remaining 30 cases blocked by
initialization. Player.log:8928–8932 confirms this. The previous external-control
fix got past its old failure. The new stack maps to dereferencing driveL after the
old DriveMechanism L path lookup. No arrangement had yet been exercised.
Failed-run mod backup/report/logs: evidence/all-standard-1.3.2-failed-20261017-003720/.

Native-first audit: extracted the full installed S282 transform/component hierarchy
with the original AssetStudio library; decompiled installed LMS S282AMeshLoader.
Both use LocoS282A_Drivetrain L/R rather than DriveMechanism L/R. Updated all four
absolute root/side-rod lookups. Native roots have Animator and AnimatorPortReader.
Audited all literal driveL/driveR child lookups against actual native data; LMS
creates the extra brake-shoe child (confirmed in its installed loader).
The old static-body s282_handle_lubricator is also absent: removed its field, lookup
and position writes. Current external lubricator pivot already handles that control.

B99 oilcup requirement: native S282 has six circuits, three points per side:
Oiling0 on drivetrain root, Oiling1 under crosshead s282_mech_push_rod, Oiling2 under
main rod s282_mech_wheels_connect/s282_mech_push_rod_to_connect. Native external cups
have PositionSyncConsumer and OilingPointPortFeederReader; PositionSyncConsumerController
pairs tags with the native PositionSyncProviderController and syncs positions late
in the frame. Native OilingPointsPortController initializes refill ports. Preserve
this system; do not add per-driver circuits or invent another oil simulation.

Added B99Oilcups.Bind: uses existing controller entries, requires six native oiling
points, validates refill/normalized-flow ports and exactly one provider per cup tag,
rebinds via native SetProviderTransform and Sync. Runs on external load and arrangement
switches. The original provider entries remain authoritative; cloned rod fragments
do not register extra providers. Existing oil quantities/ports and refill control
behaviour are untouched. Runtime report captures cup/provider identities, tags and
port IDs, verifies preservation on conversion/restoration when external controls
are loaded. Actual refill/consumption and moving alignment still need user tests.

Installed 1.3.3 with game closed; no launch or save modification. All 29 package files
verified. DLL SHA256 DA6184725FFB341D13BDAFCC04048F677B973937066144361B5DCF671DABC1DD.
Clean compile; 241 offline checks pass, including native drivetrain child paths and
all six provider/consumer locations. This is not an in-game pass.
Backup, evidence and install receipt: evidence/drivetrain-oilcups-fix-20261017-004016/.
Old JSON renamed stage2-runtime-checks.failed-1.3.2-20261017-004016.json. Next test makes
a fresh stage2-runtime-checks.json. Rerun two-stock-S282 structural test, then manually
open/refill/close all six cups, check retained oil after switching and cup alignment
while moving and after external unload/reload. Geometry/driving remain experimental.

## Current installed trial: 1.3.2 — B99 external controls fix, 17 October 2026

James authorized fixing and reinstalling the failed all-standard trial. Completed
with the game closed; no game launch or save modification. All 29 installed package
files match the rebuilt ZIP. Version 1.3.2 installed DLL SHA256:
B811908DDE8A5CC0BC5A0BF620C40BA24FD6B22F8FC5941F699DA8A1E801EA2C.

Native-first evidence: read the installed resources.assets using the original
AssetStudio library in a build-only probe. The current native S282 hierarchy has
Interactables/lubricator pivot/C_Lubricator, whose components include Button,
ButtonUseKeyboardInput, InteractablePortFeeder and RotatorPortReader. Its parent has
LubricatorRatchet. The compressor remains Interactables/C_BrakeCompressor, with
Rotary, ValveHandSnapper, ToggleValueKeyboardInput and InteractablePortFeeder.
Neither control has a HingeJoint. Both normal and exploded prefab trees show this.
The 1.3.1 failure was therefore both an obsolete path and an obsolete joint model.

Fix: replace legacy joint binding with the native lubricator pivot and compressor
Transform; capture native positions for those controls, labels and gauges. Apply
body offsets once from captured baselines after geometry changes; restore exact
captured positions for stock. Rebind on external-interactable reload. Remove the
five legacy blocks that accumulated label/anchor offsets and hardcoded joint anchors.
Missing required paths now produce descriptive exceptions before partial binding.
No new DLL, hook, dependency or alternate control implementation was added.
Runner report now includes assembly version and the actual conversion exception;
subsequent cases blocked by failed initialization are labelled as blocked instead
of producing 31 generic conversion failures.

Verification: 0 build warnings/errors, 196 offline checks passed, including actual
native hierarchy matches, absent legacy hinges/path and baseline-based placement
in the compiled binary. These do not establish in-game success. Rerun the same
all-standard structural suite on two stationary stock S282s, regulators closed,
then inspect driving/visuals manually. Controls should also be manually checked
on a small-driver arrangement and after returning to stock.

Backup, native hierarchy evidence, probe, failed report/logs, validation and receipt:
evidence/b99-controls-fix-20261017-000553/.
The old installed JSON was renamed stage2-runtime-checks.failed-1.3.1-20261017-000553.json.
The next test creates a fresh stage2-runtime-checks.json. The all-standard ZIP under
build now contains 1.3.2; previous 1.3.1 mod files are in the backup above.

## All-standard runtime review: initial setup blocked, 16 October 2026

The user ran the all-standard structural test on installed 1.3.1, verified DLL hash
53785A1EE3FC507263DFD4DC4AC7AD932E0BA4C3A2FDE41D5B3596C54007EF21.
Report subject L-017, control L-091: passed=false, 31 failed entries, zero converted
snapshots. This is one shared initialization failure, not 31 independently observed
arrangement defects. Player.log:7513–7517 and UMM-Log.txt:3645–3646 record the failure.

Confirmed: LoadExternalInteractables throws NullReferenceException at IL 0x006f.
Inspection of the exact installed binary maps this to GetComponent<HingeJoint>()
immediately following Find("Interactables/C_Lubricator"). Thus that transform lookup
returned null in the supplied loaded-external-interactables object. The current
replacement path/type and whether the supplied root differs remain unknown; do not
invent a B99 rename or blame another mod. Setup aborts before SetupFrontBogie,
SetupRearBogie or SetupDrivers, and conventionalFault refuses subsequent switches,
including stock ID 10. This explains every generic conversion-refused JSON entry.

All 31 recorded restored configuration snapshots equal the initial subject snapshot.
No separate stock-restoration exception was found. This does not establish full
visual geometry restoration or validate the arrangements. The earlier two-arrangement
stage 2 pass remains valid for its narrower path, which did not bind these controls.

Evidence preserved at evidence/all-standard-user-run-20261016-235152/:
stage2-runtime-checks.json, Player.log, UMM-Log.txt and review.json.
This turn reviewed evidence only; no source/build/install changes were made.
Next implementation work: inspect the native current S282 external controls and a
working comparable binding before updating the legacy control lookup; improve the
runner to report initialization-blocked subsequent cases clearly, then rerun.

## Installation and previous structural test confirmed, 16 October 2026

James explicitly authorized installation of the all-standard trial. The game was
closed. Version 1.3.1 is now installed at the game's Mods/RearrangedS282 folder;
all 29 packaged files match the verified ZIP. Installed DLL SHA256:
53785A1EE3FC507263DFD4DC4AC7AD932E0BA4C3A2FDE41D5B3596C54007EF21.
The game was not launched. Earlier statements that no installation occurred describe
preparation before this explicit authorization and are superseded by this section.

The newly supplied previous-stage report PASSED: subject L-006, control L-085.
The report confirms Berkshire 7/7 total and 4/4 powered axles, exact stock restoration
and unchanged control configuration; the previous runner's passed result includes
ten repeated round trips. Player.log and UMM Log.txt both record structural success.
This validates the previous two-arrangement stage 2 path, not the expanded geometry.
The all-standard suite and manual driving/visual tests remain outstanding.

Complete pre-install mod backup, previous diagnostics, logs and installation receipt:
evidence/stage2-passed-before-all-standard-20261016-231402/.
The old report was also renamed in the installed mod to
stage2-runtime-checks.previous-trial-20261016-231402.json, so it cannot be confused
with a new all-standard result. The next run creates stage2-runtime-checks.json.

# Rearranged S282 B99 port — all-standard development trial

## Current state: all 31 conventional arrangements exposed for testing

James explicitly requested expanding the trial to the remaining non-duplex
arrangements so he can test all of them, and asked where the structural button is.
The earlier instruction remains: prepare the build; James runs the game tests.
No game installation, launch, save selection or save modification was performed.

`Build.AllStandard.ps1` builds `B99_ALL_STANDARD` (Stage=3), version 1.3.1, and creates
`build/RearrangedS282-B99-all-standard-trial.zip`. It reuses the original 1.2.9
conventional geometry and rod exporter, with the existing per-car B99 physics adapter.
All IDs 0–30 are selectable; duplex IDs 31/32 and diagnostic ID 33 remain excluded.
The original archive's exporter is packaged verbatim; game meshes, assemblies and
the old duplex drivetrain bundle are excluded. Derailroader remains read-only and
the separate chuff patcher is unchanged. No new traction implementation was added.

The selected car is initialized lazily, snapshots stock and prepared geometry,
rebuilds active physical bogie axles and powered wheel registration, checks counts
against the selected arrangement and updates native radius/RPM/circumference/entity
data. Stock restores captured state; failed conversions attempt rollback. An
incomplete geometry setup is not retried over partially generated objects.
Old mass/cylinder reset constants are removed from the active geometry path.
The explicit equal-load approximation remains; these are not calibrated prototypes.
Only the radio patch is registered. Spawn/save/repair/stress/global-bogie hooks remain
inactive. External interactable unload/reload, explosion/repair, gadgets and broad
compatibility are unverified and must not be described as complete.

The button is **Ctrl+F10 → Rearranged S282 → settings → RUN STRUCTURAL TEST — two
stationary S282s**. This is separate from manual radio operation. Put both engines
in stock 2-8-2, stopped, railed, regulators closed, then start and close UMM so native
ticks run. The runner cycles one subject through all 31 arrangements, returning to
stock each time and checking configuration and second-car isolation. It retains the
filename `stage2-runtime-checks.json`, writes each arrangement's progress, and writes
a failure report when insufficient eligible engines prevent starting. The old trial
did not run automatically; the previous instructions did not make the button clear.
See `ALL_STANDARD_TESTING.md` for installation and test instructions.

Offline verification: Stage 3 compiles with 0 warnings/errors; 184 checks cover
catalogue/radio cycles, numerical conversions, reachable binary paths and packaging.
The original exporter was actually run against the installed B99 resources in a
build-only probe: rod OBJ has 508 vertices and 524 faces. This confirms extraction
and source topology size, not the resulting in-game appearance or Unity importer.
Only the earlier Stage 2 manual Berkshire conversion below has runtime evidence.
All-standard geometry, restoration/isolation, driving and visuals await James's run.

Native-first decision: reuse original supported geometry and existing B99 per-car
adapter; do not introduce a custom force model, global hook or new runtime dependency.
The previous stage 2 ZIP remains available separately.

---

Date: 16 October 2026. Scope authorized: stages 1 and 2 of the conventional-only port.
No duplexes. No game installation, save modification, or game launch was performed.

## Latest state: stage 2

### User runtime review, 16 October 2026, after the first trial

James chose to run the game tests himself. He installed the exact trial DLL (SHA256
`3052623DF331B53BC99CBDE979598E60304CB5CE896BE4C9865CA5D22ADB3AA6`). Current
Player.log and UMM Log.txt confirm successful mod activation and one manual conversion:
L-024 → s284, car/entity axles 7/7, powered manager/port 4/4, driver radius 0.712 m,
friction coefficient 0.240000024, base mass 111000 kg. His screenshot shows the two
small trailing axles and four driver axles. No RearrangedS282 exception was found.

`stage2-runtime-checks.json` was absent from the mod folder and the searched game/
Altfuture locations. Neither log records the automated runner's completion/failure,
return to stock, or repeated round trips. Therefore stock restoration, second-car
isolation and repeated switching are **not yet confirmed**. Do not report the full
structural suite as passed. The runner currently returns without a report when fewer
than two fresh eligible S282s exist; that is a possible explanation, not an observed
cause. A manually converted car is excluded from the runner's fresh-car search.

The repeated `TrainCar.isEligibleForSleep` MissingFieldExceptions are attributed by
their stack traces to `UtilitiesMod.UtilitiesMod.Update`, not this port. Other errors
include CCL missing liveries, a DE6 Mesh Splitter warning, AI Traffic despawning and
shutdown errors. No causal relationship to the S282 conversion was established.

Evidence copies: `evidence/stage2-user-run-20261016/Player.log`, `UMM-Log.txt`,
`review.json`. Review only; no mod/source fix or game changes were made in this review.
Next runtime check: two newly spawned, stationary native S282s, before manually
converting either, then use the UMM structural-check button. Loaded performance,
actual slip onset and moving/distance visual validation also remain outstanding.

Stage 2 source and build are implemented, with native runtime validation still pending.
The game was closed. James was asked whether to use a disposable sandbox or prepare
the test build for him to run; no answer had arrived at the time of this handover.
Do not treat that unanswered choice as permission to select a career save.

- `Build.Stage2.ps1` produces a B99_STAGE2 DLL and a development trial ZIP under `build`.
- Stage 2 compiles with 0 warnings / 0 errors. Its 73 offline checks pass.
- The stage 1 build and its 146 checks remain available independently.
- `Main.Load` registers **only** `CommsRadio_Awake_Patch`. No PatchAll, spawn, save,
  repair, stress or global bogie patch is registered. No custom traction DLL or CCL
  dependency was added.
- Only stock ID 10 and Berkshire ID 11 are exposed by the stage 2 radio.
- `WheelRearranger` now has a partial stage 2 implementation. Initialization is lazy
  on selected-car conversion; its automatic Start is empty in stage 2.
- Stage 2 bypasses the old broad geometry/mesh-extraction path. It reuses the original
  two small trailing-axle positions by cloning the native leading axle twice, and
  uses that axle's native unpowered rotation component. The original trailing axle
  and native array references are restored when returning to stock.
- `B99PhysicsState` captures per-car entity friction/axle data, driver registration,
  live powered axle port, force radius, feeder conversions and driver animation
  circumference. Apply/restore writes the selected car's instance data only.
- Native ECS `StaticSharedAdhesionCalculationData.numOfAxles` is updated along with
  car/bogie caches; stale slip/slide output is reset. Native weather, sanding and
  braking continue to run. Driver radius and four powered axles remain unchanged for
  Berkshire; generic conversions are present for later radius/driver-count work,
  which is not yet runtime validated.
- No mass/cylinder/reset constants are applied in the active path. Native mass,
  cylinders, cutoff and shared car type are left alone.
- `ConventionalPhysics` reuses Derailroader G29/C21 driver-weight compensation and
  native RPM conversion. Tests execute those profile examples. Equal axle loading
  is the explicit prototype assumption here: four driven axles out of seven,
  versus stock four out of six, yields 6/7 adhesive allocation at unchanged mass.
  No suspension loading measurements or loaded performance tuning are claimed.
- Radio confirmation now reflects success/refusal; it owns a cloned highlight
  object instead of moving the vanilla deleter's object. Materials/highlighter are
  cleaned up with the mode.
- Motion, derailment, open regulator, substantial residual force, wheel slip and
  incomplete initialization refuse the conversion. Failures attempt captured-stock
  rollback; the transition lock releases in finally.
- `Stage2RuntimeChecks` is an explicitly invoked test in the mod manager. It runs on
  two fresh native S282s, waits for native ticks, compares snapshots, repeats ten
  round trips and writes `stage2-runtime-checks.json`. This test has **not run**.
  Its successful result would establish structural restoration/isolation, not visual
  correctness, loaded pull, steam consumption or actual slip onset.

Read `STAGE2_TESTING.md` for the disposable sandbox procedure. Do not mark stage 2
fully validated until the native test and representative driving/visual checks pass.
Do not enable the other arrangements as part of this trial.

## Source and environment

- Project: `B:\GitHub repo\rearranged-s282-b99`
- Branch: `codex/b99-standard-stage1`
- Upstream: `https://github.com/pjschinske/dv-rearranged-S282`, commit `24e24d5`.
- Upstream Info.json identifies version 1.2.9, matching the supplied release metadata.
  This is not a claim of byte-for-byte reproduction of the original DLL.
- Supplied archive: `C:\Users\james\Desktop\new mods\Rearranged S282-752-1-2-9-1710020512.zip`
- Archive SHA256: `18B2CA6B73EBFECE33401DB924DDFECDACFEBCFDF193E69707CB436AB56E778C`.
- Game reference installation: `B:\SteamLibrary\steamapps\common\Derail Valley`.
- Prior game log identifies `99-build2702`, Unity 2019.4.40f1.
- Installed Loco Mesh Splitter: 1.1.9. Its B99 gadget collision APIs already exist;
  do not introduce a replacement framework.

## Stage 1 implementation retained as a separate build

The conventional controller is compiled against actual B99 references. Duplex simulation
classes, duplex Harmony patches, the unused cylinder-count patch, and the duplex asset
loader are excluded by the project. Duplex creation, rear drivetrain setup, cloned
Franklin gear, particles, and duplex mesh generation have been removed from the
conventional controller/mesh loader. Original excluded source and model files remain
in the checkout as upstream history; they are not active support.

All original enum values 0–33 and name-table positions remain stable. Conventional IDs
are 0–30. IDs 31/32 are retired duplex entries, 33 is the old no-drivers diagnostic.
Radio traversal excludes retired IDs; unsupported IDs normalize to stock ID 10.
Random settings contain no duplex choices and default random spawning is off.
Empty random selection falls back to stock. These paths are not activated in stage 1.

The stage 1 entry point is gated by the project's `B99_STAGE1` define: it only registers a
status label and logs the baseline state. It applies **no Harmony patches**, does
not load arrangement settings, does not initialize meshes, and does not read/write
arrangement save data. This preserves a stock baseline by construction, not by an
in-game test. Do not claim the arrangements work in B99 yet.

The B99 rename `SimulationFlow.orderedSimComps` → `OrderedSimComps` is updated. Old
AdhesionController friction writes have been removed; the per-car B99 replacement
is stage 2 work. Do not enable the runtime paths before completing that work.

The original Publicizer 2.2.1 dependency is retained. It publicizes **local build copies**
of game references, never the installed assemblies. Harmony, Unity Mod Manager and
Mesh Splitter references point to the installed game; no new runtime dependency or
CCL conversion was added. The old automatic packaging/post-build actions are absent
from the updated project. Upstream auto-update metadata was removed from this port.

## Reproduce and validate

Run PowerShell in the project directory:

```powershell
.\Build.Stage1.ps1
.\Validate.Stage1.ps1
```

Both accept `-GamePath`. Build output is `build\stage1\RearrangedS282.dll`, with
`build.log` and `build-receipt.json`. Offline validation records `validation.json`.
The scripts do not install into Mods, invoke AssetStudio, or launch the game.
Do not use the upstream `package.ps1` for this development baseline.

Validation covers original ID stability, conventional radio traversal in both
directions, retired/invalid ID fallback, compiled absence of duplex simulation and
asset loader, entry-point isolation, and existence of prospective B99 Harmony targets.
Latest run: Release build succeeded with **0 warnings and 0 errors**; **146 offline
checks passed**, 0 failed. Final method-inventory review confirms only duplex-specific
controller methods and the duplex timing helper were removed; conventional methods remain.
Method existence does not prove Harmony signature matching or transpiler correctness.
In-game UMM loading, Console behavior and stock driving are not runtime validated.

## Stage 2: existing reference implementation first

Derailroader was inspected read-only and must remain unchanged:

- `B:\GitHub repo\dads-derailroader-exp\tooling\builder\tools\unity\CclLocoBuild.cs`:
  driven axle registration, powered animation radius, unpowered truck rotation,
  mass/radius/friction configuration, sparks.
- `tooling\locos\g29\profile\G29Config.cs` and `tooling\locos\c21\profile\C21Config.cs`:
  friction compensation for driver weight versus DV axle allocation.
- `tooling\builder\tools\audit_new_loco.py`: agreement between powered-wheel count,
  simulation poweredAxles and animation radius.
- `src\rr2dv\record.py`, `enginemetrics.py`: unit handling and TE calculations.
  The driver-weight UI estimate itself does not update runtime axle loads.

Keep the existing S282 runtime rearrangement design. Reuse those conventions without
importing the full converter or converting the mod to CCL. The remaining gap is how
to refresh already initialized per-car B99 entity/cached state safely. Never change
shared S282 type data to update just one locomotive.

Prove stock ↔ 2-8-4 first, then 4-6-2 and 2-12-2. Verify a second S282 is unaffected.
James is developing a separate chuff patcher; do not replace or expand it here.

## Known outstanding audit findings before enabling patches

- Runtime switching needs per-car driver radius, powered-wheel registration, axle
  caches and B99 entity adhesion updates using the existing native simulation.
- Reset values are B98 constants (mass/bore/stroke/cutoff). Capture and restore the
  current B99 per-car baseline rather than assuming these are correct.
- Upstream ten-coupled random flags incorrectly map to eight-coupled IDs in Settings.
- The sixth-axle sparks definition currently points at driveAxle5. Correct alongside
  powered-wheel registration and verify the resulting effects.
- BogieAxlesPatch currently affects all bogies; narrow it to rearranged S282s.
- Repair restoration currently has no S282 guard; narrow it before activation.
- Check every Harmony signature/initialization order and the TrainStress transpiler
  against B99 before registering patches.
- Persistence load retains a static map across loads; validate/reset it at the right
  boundary before enabling save writes. Keep enum numbers and the legacy misspelled
  save keys stable. Do not erase saved choices merely to test the stock baseline.
- Current radio mode shares a vanilla highlighter; audit ownership before activation.
- Inspect object paths/mesh vertex ranges, LODs and gadget collision refreshes.

No change to Derailroader, the user's chuff patcher, installed mods or saves was made.
The only ZIP is a local stage 2 development trial. No release, commit, push or
publication was performed.





