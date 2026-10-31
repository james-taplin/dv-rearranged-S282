# Tender and multiplex regression repair, 31 October 2026

James authorised repair, agent review, stronger structural tests, a new test
build and installation, in that order. The earlier read-only restriction is
superseded. No push or publication is authorised by this task.

## Confirmed tender cause and repair

The 2.0.9.2-beta repair configured a mirror through `GetComponent` after cloning.
The source-to-rear-to-inactive-template-to-tender chain copies old mirrors.
Unity destroys the disabled copied components at end of frame, so that lookup
can return a retiring mirror instead of the replacement that actually runs.
The replacement then copies the phase-owned host's zero speed and freezes the
tender before animation evaluation.

The native Unity 2019.4.40f1 regression reproduces this exact chain with the
actual S282 hierarchy, original controller and all four wheels on both sides.
The legacy lookup fails with phase delta 0 and SpeedMultiplier 0. The repair
passes the speed-copy option into the helper and configures the exact new
`AddComponent` result before activation. Other clones retain the default.

The tender clock now uses its own engine's signed native RPM, matching its
frequency source, so an independently slipping/counter-rotating front engine
cannot reverse tender presentation. A missing or inactive engine yields zero
speed. A separate pre-guard regression reproduces the partial-bind null error.
No independent tender traction or new clock/physics architecture was added.

Final native tender run: 276 assertions, 12 moving sign/reattachment cases and
12 stopped stages, three attachment cycles, both sides/all four actual wheels,
quartering, deferred component cleanup and native cutoff unsubscription.
Evidence: `build/tender-repair-probe/native-fixed-stop-guard/result.json`.
Legacy freeze: `build/tender-repair-probe/native-legacy-02/bootstrap-error.txt`.

## Wheelslip validation

The isolated player connects actual native steam engines, adhesion/slip ECS
jobs and a finite native Boiler/MultiplePortSum to verbatim production
PrepareCore, PublishRpm and ApplyForce. Thirty assertions pass: both groups
slipping, mixed slip, RPM divergence, force/grip conservation, one application
per synthetic bogie, braking, sanding recovery, slide suppression, reverse,
steam accounting and cleanup. There is no wheelslip gameplay diff.

The first boiler assertion compared native float mass subtractions with a
double integral using an unjustified fixed tolerance. The corrected test
asserts the exact per-tick native float ledger and separately reports numerical
quantization. It does not loosen the tolerance to conceal unaccounted intake.
At 60/60 RPM, integrated consumption is 211.327 kg; at 300/60 it is 360.924 kg
and the same initial finite vessel reaches lower pressure. The joined feedback
scenario also depletes the real native boiler while slip/RPM/force run together.

Evidence and explicit boundaries: `build/slip-integration/native-02/result.json`
and `evidence/slip-integration/README.md`. The car, contacts and recorded bogie
force sink are substitutes; this does not prove actual rail acceleration,
complete game scheduling, S282 firebox tuning or multiplayer transport.

## Franklin linkage

Original bundle/controller assets, native drivetrain hierarchy/triangles,
source-extracted configuration and compiled production phase/retarget code were
tested through normal Unity frames. IDs31/32/38/40/47 cover 720 phases, three
yaw stresses and two relative phase patterns: 21,600 layout poses, both sides
and every engine. Tracked independent poses equal direct original-controller
poses. No measured crank contact with its own or the following casing/crosshead
was found in that finite sweep.

Removing only the existing ID32 rear-nose correction reproduces 648 surface
contacts, 324 per side. This negative control establishes that the detector
catches that older defect in the actual controller chain. The existing 2.0.7b
correction is retained. No new Franklin geometry edit is justified by these
results, and the reporter's new clipping is not claimed fixed or disproved.

Evidence: `build/franklin-runtime/sweep-readable-fidelity-720/result.json` and
`build/franklin-runtime/sweep-old-nose-720/`. Original bundle mesh readability
is retained to exercise the real retarget branch; private CPU-readable copies
are used separately for triangle measurement. Curve yaws are stress inputs,
not a recording of live track constraints. Full static frame/guide contacts,
coplanar/enclosed volumes, every relative phase combination and camera LOD
selection remain outside this measurement.

## Walschaerts multiplex extension

James explicitly added Walschaerts plex clipping to the acceptance scope.
The native sweep now covers IDs37/41/42/43/45/46 plus a pared native ID10 control,
720 phases, Cutoff -1/0/+1, yaw -15/0/+15, both sides and the Erie's tender-engine
geometry. It checks nine gear parts against own/following casings and moving
crossheads, including all 720 x 720 sampled relative phases for neighboring
moving parts. There is no invented locomotive-to-tender coupling transform.

This reproduces 1,200 unwanted crank/main-rod contacts with the following casing
on ID37 (2-6-6-2) at the stronger yaw stresses. The sampled onset grid finds
contacts at 15 degrees, none through 14; the exact corresponding live curve
radius is not established. Other Walschaerts layouts produce no additional
contacts in this sweep. The valve spindle entering its own valve chest is
present in the native control at every phase and is explicitly classified,
never presented as an inter-engine bug or removed to manufacture a pass.

The original mechanism helpers and casing shape match 2.0.7b. That suggests an
older clearance limitation, not a newly rewritten Walschaerts motion path.
The final shaped casing is measured: its bulk treatment deliberately changes
thickness, so an affine native-motion argument alone would not prove clearance.

The repair is a dedicated ID37 engine gap of 0.150 m instead of 0.100 m.
It moves the coherent rear engine/trailing axle back 50 mm and the calculated
hinge back 25 mm, retaining scale, internal native meshes/motion and the common
BigMallet/trailing gaps. The gap is extracted from production source; final
acceptance has no experimental gap override. Conservative sampled minimum
surface separation is 42.308 mm for the crank bearings and 42.736 mm for the
main rods. The trailing center remains 241 mm ahead of the existing proven
12-coupled rear-center datum. Axles, statics, effects and frame follow the
calculated layout; force routing and calibration have no gameplay diff.

Final actual-source acceptance: **112,320 layout poses, 547 assertions, all 11
articulated layouts plus the stock control, zero unexpected contacts**.
Tracked phase-adapter poses match direct controller playback within numerical
precision (maximum recorded metric difference below 0.000001). Every measured
following-crosshead relative-phase grid also clears. The JSON explicitly
separates measurement completion, clearance acceptance and expected negative
controls; completion alone cannot masquerade as passing clearance.
`build/franklin-runtime/final-source-all-720/result.json` is the accepted run.
Final controls `final-old-gap-720` and `final-old-nose-720` report failed
clearance with the expected 1,200 and 648 contacts respectively.

Production BindArticulatedLods retains the same detailed articulated mechanism
through the native far-cull threshold and disables coarse rigid proxies. These
measurements cover that geometry, not actual camera culling/visibility behavior.

Static geometry is exported by the installed LMS splitter in the private Unity
player, with its source/cache hashes. Cylinder, cock and pipe geometry agrees
with the independent replay within 0.31 micrometre. The older guide replay has
a fidelity gap (612/523 vertices/faces versus actual 716/622), so actual exported
guides replace it. `Validate.LmsStaticGeometry.ps1` regenerates the authoritative
local payload; no game files or save are modified. Shader fallback materials
are a test-only boundary, not a replacement mesh/importer implementation.

## Structural and supporting regressions

The prior diagnostic checkpoint preserves mechanisms leased away from the
global speed writer, records active mirrors/clocks/phase ownership and local
versus effective settings, rejects independent fallback as a successful
independent run, and never certifies motion/clearance from stationary data.
Additional tests cover signed tender input and missing/disabled/rebound clocks.

Managed fixtures pass 1,871 quartering/ownership plus 190 linkage checks,
2,305 structural-validator checks over 93 cases, 126 rear-driver checks,
1,421 tender-source checks/40 switches, 2,402 cleanup checks/200 cycles,
2,553 audio checks, 56 simulation-graph checks, 32 port and 27 authority checks,
219 multiplayer bridge and 135 core checks. The simulation gate additionally
passes 72 checks against actual native engine implementations.

The formerly broken tender-source fixture now extracts the actual frame-source
builder and ArticulatedFrameClipper rather than stubbing the missing helper.
It checks material/layer retention, frozen mesh identity and empty-slice cleanup.
These fixture counts are not live game or multiplayer-session counts.

No incompatible supported visual-settings combination is established. Preserve
Settings.xml; a setting exposing stale presentation is a mod defect to repair.
Reapply changed appearance preferences and record the effective per-car profile
as well as personal settings, particularly with a multiplayer host.

## Delivery state

Built, packaged and installed **2.0.9.3-beta** from local source checkpoint
`3540e61a43ec3e52dd0fb4e47c566a309990b7b4`. Both native-reference DLL builds
completed with zero warnings/errors. The complete portable archive passed 80
package checks; assets, exporter and licences retain the verified seed bytes.
All 38 archive members match the installed files byte for byte. All 39 local files, including Settings.xml, mesh caches and captures, were
preserved; only obsolete trial notes and
the old generated DLL cache were removed. No full installation backup was made.

Installed path: `B:/SteamLibrary/steamapps/common/Derail Valley/Mods/RearrangedS282`.
Desktop archive:
`C:/Users/james/Desktop/Derail Valley Mods/RearrangedS282-B99-2.0.9.3-beta-Nexus/RearrangedS282-B99-2.0.9.3-beta.zip`.
Archive SHA256: `1498085A0A13584860B35C98192F1288A9ABF55AB563582AFD0900B80D5C095F`.
Core DLL: `B2B9B3F1382CD87412D90861DB99C49E438106D1015CCF25FD4D366116F7E0BE`.
Bridge: `E3CDED021EAEF4FEC72A469E7BFA542E1E7CE7AD6D66C0A898CC83ACE6CFA4CD`.
Installation receipt: `build/repair-2.0.9.3-beta/installation-receipt.json`.
No push, upload or publication was performed. The game was closed for install.
Live coupled
vehicle driving, save/reload, camera LOD and host/guest acceptance are not
claimed by these isolated tests. Private game geometry and generated player
payloads remain outside Git and the distributed mod archive.
