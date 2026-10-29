# Independent driven-set wheelslip: researched implementation plan

28 October 2026. Three-agent review of installed B99 code, the installed L74
bundle, current S282 source, animation, effects and lifecycle.

## Decision and present state

Independent slip is feasible in principle. The preferred next step is a real
Unity/game-World experiment using **native adhesion and wheelslip calculations
on owned helper entities**. This is a stronger native option than the earlier
scalar prototype. It must be proved before choosing a managed reproduction of
the equations. A new wheel-inertia solver is not presently justified.

This is a research and implementation plan. No integrated independent-slip
build exists. The existing GroupForceRouting helper and scalar fixtures do not
establish vehicle integration. The installed mod reports 2.0.6-geometry-test;
source metadata was 2.0.7 when read during this review, with other development
active. No production source or installed files were changed by this review.

Corrected earlier premise: one managed WheelslipController per TrainCar does
not prevent reuse of native calculation jobs. Their calculation queries accept
plain ECS data; managed car-copy/application stages are separate queries.

## What the installed example establishes

The installed Heavy Mallet `ccl_bundle` was read directly with UnityPy, rather
than inferred from its authoring prefab. SHA-256:
`D8F8CDF26D49ED91B0CAF9EE0B35BD19E7ECD52C9C7FB978D4701A54BFCDFF3E`.

The locomotive graph contains 72 execution definitions, exactly one
TractionDefinitionProxy named `traction`, and the connection
`l74SimplingTorque.OUT -> traction.TORQUE_IN`. Both `steamEngine.CRANK_RPM` and
`l74SimplingEngine.CRANK_RPM` read `traction.WHEEL_RPM_EXT_IN`. The tender graph
contains 11 definitions and no traction definition. Thus L74 is a working
shared-traction example, not an independent-slip implementation to copy.

Its serialized wheelslip maximum is 370 RPM, with adhesion-drop curve keys
0 -> 1 and 0.25 -> approximately 0.4. These are L74 values, not S282 values.
The implementation must capture the actual S282 controller's runtime maximum,
curve, prevent-slip flag and applicable game settings. Neither the class default
600 nor a supposed steam-engine RPM_MAX port is an established S282 input.
DirectDriveMaxWheelslipRpmCalculator is a generic geared-drive helper; no
applicable S282 RSE maximum-RPM port was found.

Reproduction script: `evidence/independent-slip-research/inspect_installed_l74.py`.
Local metadata report: `build/independent-slip-research-20261028/l74-graph.json`.
The native resources asset lacked readable type trees in a direct UnityPy
attempt; actual S282 maximum/curve remain a runtime-read gate.

## Intended behaviour and unavoidable handling change

Each mechanically separate engine has its own torque demand, allocated grip,
native slip state, signed RPM and native cylinder phase. Common weather does
not force common slip: each engine is compared against its own allocated limit.
Different cylinder demand, phase, load and speed can produce different outcomes.

Do not add random friction differences to manufacture the result. A clearly
marked diagnostic-only per-group adhesion multiplier will make acceptance
repeatable. Local grease/leaves or spatially different rail wetness are not
established B99 features and would be a separate handling feature.

Keep the existing calibrated bores, stroke, wheel radii, throttle/chest limits,
boiler and intake aggregation. The current main engine has capacity 1; each
auxiliary has about 0.483921 of that capacity. Common CylinderCalibration area
scaling preserves this ratio. Ideal matched-condition proportions are:

| Engine arrangement | Main/front | Middle | Rear |
|---|---:|---:|---:|
| Two locomotive engines | 67.39% | — | 32.61% |
| Three locomotive engines | 50.82% | 24.59% | 24.59% |

These are capacity proportions, not measured instantaneous torque or drawbar
force. With equal driver-group sizes the load shares are equal, so the stronger
main engine can slip before the auxiliary. It can no longer borrow unused
adhesion from another engine. Preserve total available grip and non-slipping
pull; expect lower transmitted pull in asymmetric saturation. Do not quietly
retune cylinders or weight shares to recover the old pooled result.

Routing force through each engine's contacts also replaces the former equal
per-bogie traction split with the actual group contributions. Even with equal
total straight-line pull, this can change curve/coupler loading; include those
observations in the test build's handling report.

B99 models slip as a normalized overload response and smoothed RPM toward a
configured maximum, not a physical angular-inertia equation. The first version
should preserve that native behaviour independently per group.

## 1. Prove native calculation ownership first

Use one calculation-only adhesion carrier per physical car, plus one
calculation-only slip entity per driven group. This keeps braking wheel slide
owned by the actual car while providing fresh grip at standstill.

| Adhesion carrier components | Slip-group components |
|---|---|
| LocalToWorld | WheelslipInputData |
| StaticSharedAdhesionCalculationData | WheelslipOutputData |
| StaticAdhesionCalculationData | WheelSlideData |
| AdhesionCalculationData | StaticSharedAdhesionCalculationData |
| WheelSlideData | AdhesionWheelslipOutputData |
| AdhesionWheelslipOutputData | IsMovingTag |
| IsMovingTag | Owned-group identity |

Omit TrainCar, managed WheelslipController and TrainCarPositionData. This
excludes the native managed Copy/Apply queries and the position-monitor query
that would clear the helper's eligibility tag. The helper tag is only query
eligibility; it must not mark the real car as moving. Do not put
AdhesionCalculationData on each slip-only entity, which would introduce a second
braking wheel-slide calculation there.

Feed the carrier current host mass, pose, native corrected friction, native
sanding input and relevant native state. Use its fresh raw per-axle grip output.
Ignore its calculated wheel-slide output; copy the real car's wheel-slide state
to each group. This preserves car-level braking/slide limitations explicitly.

Establish ordered systems: prepare inputs before native adhesion; bridge after
native adhesion and before native wheelslip; sample outputs after native
wheelslip with the relevant job dependencies completed. Do not rely on
MonoBehaviour order or sample unfinished jobs. The native calculation classes
have no explicit mutual ordering attributes in the inspected declarations.

Prefer owned DVConvertToEntity children with custom archetypes and the existing
native lifecycle. DVConvertToEntitySystem in DV.Utils.ECS runs first in the
simulation group before BeginSimulation ECB playback. Native slip adds/removes
tags through deferred buffers; immediately destroying a raw entity can leave
queued operations targeting a dead entity.

**Gate A:** in the actual Unity/game World, with no force application, prove
separate calculation, matched single-group native results, fresh stationary
grip, pause/game-setting behaviour, explicit ordering and stable frame latency.
Audit other native system queries against the helper archetypes so eligibility
tags cannot accidentally enroll them in unrelated vehicle work.
Repeatedly enable/disable/destroy helpers through native conversion and require
zero leaked entities or deferred-command errors. Do not promote this design to
production based only on successful compilation or query inspection.

If Gate A fails, record the actual failure and evaluate a narrow managed
reproduction of the inspected native equations with native parity tests.
Do not jump directly to a bespoke inertia/adhesion solver.

## 2. Build explicit engine/contact descriptors

One owner on the locomotive coordinates the entire configuration. Each group
descriptor holds the native engine instance and role, physical car, unchanged
radius, current native input identities, driven contact/bogie map, left/right
animators and phase offsets, effects bindings, and an activation generation.

Record ownership at AlignNativeAxle, where both PoweredWheel and engine root
are known. Validate actual native ancestor bogies and active contacts after
geometry setup. Engine role must be explicit: the simulation's main/rear/middle
IDs and the effects array's front/rear/middle order are not interchangeable.

| Layout IDs | Main/front contacts | Middle contacts | Rear contacts |
|---|---|---|---|
| 31/32 | axles 1,2 -> F | — | axles 3,4 -> R |
| 37 | axles 1,2,6 -> F | — | axles 3,4,5 -> R |
| 41/42/43/45/46 | front base [1,2,6] + extras, take Spec.Front -> F | — | rear base [3,4,5] + extras, take Spec.Rear -> R |
| 38/40 | axle 1 -> F | axle 2 -> F | axle 3 -> R |
| 47 | axles 1,2 -> F | axle 6 -> F and axle 3 -> R | axles 4,5 -> R |

Big-Mallet group counts for the listed IDs are respectively 4+4, 3+4, 4+4,
3+3 and 5+5. Shared bogies are normal: they do not require another Rigidbody.

Allocate adhesive load using the existing effective driver share f and current
native mass M:

`adhesiveWeight = M * g * f`

`groupWeight = adhesiveWeight * groupDrivenAxles / totalDrivenAxles`

Existing B99PhysicsState already converts friction to represent f:

`muEffective = muOriginal * f / (poweredAxles / totalAxles)`

Reusing this coefficient and native weight per axle, then giving each group only
its own powered-axle count, partitions grip correctly. Do not apply f again.
Sum of group limits before individual slip-drop effects must equal the current
total under identical native conditions. Retain the existing rated short-group
load caps and the 124 t calibration reference.

**Gate B:** every supported layout has unique complete contact ownership, correct
engine roles, unchanged cylinder/radius calibration and conserved weight/grip.
Missing, duplicated, disabled or invalid contacts block activation atomically.

## 3. Connect speed, torque and one force owner

Per native SimulationFlow substep, publish each group's signed RPM through an
owned port reference before its engine ticks. Retain the native engine as the
only crank-phase integrator. Preserve previous-tick intake -> steam calculator
-> boiler -> current engines execution order and the existing single intake sum.
All active slipping engines continue drawing from that same finite boiler.

Derive RPM using native feeder behaviour, radius and the group's native slip
output. Schedule smoothing once per chosen simulation interval; never advance
it independently in both Update and FixedUpdate. Read back the real S282 slip
curve/maximum. After engine ticks, capture torque/intake/phase for the next
ordered native slip calculation. Record and test the resulting frame latency.

While the owner is active, suppress the original locomotive DrivingForce force
application and the old powered-tender ApplyTenderForce path. Convert each
engine torque to demand using its own radius, perform native-equivalent
slide/brake handling and clamp once to that group's native output limit.
Then apportion force over that group's actual contacts, sum group contributions
per bogie, and call Bogie.ApplyForce once per bogie. That helper directly calls
AddForce along bogie forward; it has no additional adhesion limit.

Keep actual native brake and rolling-force components running. For the native
slip-related brake compensation, begin with a documented partition of the same
parent car's native brake budget by driven-axle share, so group compensation
sums to the old car budget. Do not give every group the full budget or apply it
again as a new physical brake force. Prove one-group native parity and mixed
slip/grip brake cases before accepting this allocation. More detailed contact
brake allocation would be a further evidenced handling change.

Use completed snapshots with a generation number for FixedUpdate. Activation,
switching, detach and failure invalidate old snapshots synchronously. Audit
global consumers: native cab indicators may use an explicit 'any group slipping'
summary, but this summary must not become every group's physics input. Legacy
wheel-RPM ports need an explicit primary-group policy; actual travel-speed
ports remain vehicle speed. Keep a stationary spinning locomotive awake.

Derailment needs a separate test of the native grounded/body-force path,
including brake compensation once per car. Until that path is validated,
transition out of independent ownership on derailment and restore a coherent
native state; never leave rail-bogie forces or a stale tender force active.
Document this trial limitation rather than implying tested derailed traction.

## 4. Make the same group phase drive mechanism, sound and effects

Prototype native AnimatorPortReader SET_NORMALIZED_TIME against each engine's
CRANK_ROTATION. The native engine writes a 0..1 phase with signed RPM integrated
through Repeat, so forward and reverse are supported by the same phase source.
Capture the visual/engine phase offset at bind and retain native left/right
quartering. Bind after existing quartering initialization has selected the
verified native/Franklin/Franklin4444 state and activated the drivetrain:
SET_NORMALIZED_TIME targets the current Animator state. Snapshot and disable
competing normalized-time readers as well as speed writers, retaining required
cutoff readers. Capture the visual/engine phase offset once at binding, never
again on slip or recovery; reverse tests must repeatedly cross phase zero.
Remove those animators from the shared native speed writer and fix
automatic SpeedMultiplier to zero. Exclude speed from MalletAnimatorMirror;
retain required cutoff/reverser mirroring. Do not reset relative engine phases
when a slipped engine recovers.

Verify phase origin on Walschaerts, Franklin and Franklin4444 in actual Unity
evaluation. Normalized-time readers are preferred if they pass; a group speed
adapter is the fallback, with explicit phase-drift tests. Do not add a second
animation crank integrator or manually call Animator.Update every frame.
Keep existing wheel followers, rods, DriveRetarget, native constraints and LODs.

Existing auxiliary chuff/cock/crack readers largely already use each native
engine's ports. Keep the shared chimney's established pulse/pressure policy and
tender outlet. CHUFF_FREQUENCY is unsigned, so do not use main-engine direction
to drive the tender. Cylinder-cock travel-speed input remains car speed.

Native wheelslip sparks and wheel audio still read car-wide slip/RPM. Reuse
their assets and contact definitions through group-scoped input adapters;
disable competing global writers only for owned presentation. Preserve native
hazmat ignition cadence once per car rather than multiplying it by group count.
Scope curve-drag suppression to the slipping group's contribution, retaining
the current summed result when all groups grip. Audit loop-refresh slip events
and cab warnings explicitly.

**Gate C:** rendered wheel/rod phase agrees with native engine phase at varied
frame rates, substeps, reverse, zero speed, pause and culling. Quartering is
retained, gripping groups emit no slip sparks/sound, and chuffs/cock masks follow
their own engine. This is part of the first playable two-engine milestone.

## 5. Extend the same owner to three engines and the tender

First integrate one two-engine arrangement completely, including animation,
effects and stock return. Then cover shared-bogie triplex layouts and all
remaining two-engine layouts. Finally attach the tender group to the same
coordinator. Supported combinations are one locomotive engine + tender, two
locomotive engines + tender, and three locomotive engines without tender;
powered tender support is not limited to Erie ID43.

The tender retains its physical native bogies. Its four visible drivers are not
four newly registered native PoweredWheels; do not rewrite native axle count or
mass to match presentation. Retain the established 0.24 base traction
coefficient, 0.8 adhesive mass share and current bogie force split initially,
with an explicit tender contact descriptor. Do not blindly inherit the ordinary
tender's native powered-friction coefficient: it is not established as this
calibration. Encode the existing `.24 * .8 * mass * g` dry baseline exactly once
before native environmental/slip modifiers. If using four logical powered
contacts and the native physical weight-per-axle input, the equivalent corrected
coefficient is `.24 * .8 * nativePhysicalAxleCount / 4`; this is calculation data,
not a change to the tender's physical axle registration.
Use the tender's own native pose, slide and mass, neutral sanding if it has no
native sander, and one steam input from the locomotive. Replace the old manual
force clamp and bypass the host/tender RPM-ratio adapter when independently
computing tender RPM; otherwise radius conversion is applied twice.

Native TotalMass includes resources but its ordinary resource-mass refresh is
every 120 seconds. Recommended first-trial policy: adhesion uses the same native
physical mass as braking/inertia, including its resource update timing. Also log
fresh read-only GetResourcesMass and native cached mass so the lag is visible.
Test full/depleted tender after native refresh. If instantaneous fuel-dependent
adhesion is required, implement it as an explicit read-only adhesion estimate
and disclose the temporary difference from native physical mass; do not silently
retime the whole car's Rigidbody mass and braking.

## 6. Lifecycle, diagnostics and acceptance

Activation is transactional: validate descriptors, native helper readiness,
ports and presentation first, then transfer force ownership. Require the first
completed carrier/group result stamped with the current activation generation;
default-zero outputs after deferred conversion are not a ready snapshot.
Ordinary stock S282 with no powered tender bypasses the owner entirely.
One idempotent
release method stops force, invalidates snapshots, removes events/readers while
their objects still exist, restores original identities/enabled states, and
retires ECS helpers through the native conversion boundary. Extend existing
arrangement rollback, RestoreConventionalStock, pool restore and destroy paths.

Tender uncoupling immediately clears tender force and disables that cylinder
engine's demand, then releases effects before deactivating leaves. Existing
effect-reparent warnings are a relevant cleanup defect to reproduce and fix.
Save the arrangement and existing cylinder health as before; reconstruct transient
group state on reload, matching native transient-slip reset unless testing proves
a need for extra save data. No new vehicle IDs or migration scheme is assumed.

Each group diagnostic records engine role/car, native physical contacts/bogies,
radius, allocated weight, torque/demand, raw and dropped grip limit, brake share,
transmitted force, signed RPM, peripheral speed, slip, phase, chuff count and
intake. Add snapshot generation/timestamps, per-bogie applied totals, native
cached/fresh resource mass and a flag for any diagnostic adhesion override.

Acceptance gates, in order:

1. Real native helper-entity gate A and complete ownership gate B.
2. Complete two-engine milestone with force, finite shared steam, phase,
   sound/sparks and exact stock restoration; then triplex and tender coverage.
3. Controlled low-adhesion first-group test, reverse-group test, both-slip test,
   regulator-close recovery and sanding/braking tests at rest and in motion.
   Diagnostic overrides are marked and reset before natural-condition captures.
4. Compare baseline when every group is below its own limit: same available grip,
   similar mean pull under matched steam/phase, and no doubled force. Use a stated
   tolerance before testing, provisionally 1% integrated mean force over complete
   cycles; report phase/transient deviations separately. Do not compare unmatched
   boiler histories or demand old pooled pull during asymmetric saturation.
5. Increased slipping RPM must change native intake and draw from the shared
   finite boiler under flow limits. Existing fixed-inlet engine tests alone do
   not satisfy this gate.
6. Repeated arrangement/stock switching, save/reload, pool reuse, external
   controller reload, derail/rerail, tender detach/reconnect and deletion. Require
   no stale force, event leaks, helper entities or ECB errors; verify original
   stock reference identities and behaviour.
7. Stage a distinct test ZIP with source hashes, test results, known limitations,
   per-group diagnostics and a short playable checklist. Installation and
   release publication are separate actions. Runtime acceptance remains separate
   from offline tests and package validation.

## Implementation touchpoints and evidence

- ArticulatedSimulation.cs:112,182,190,193,240,266,297,389 — graph, shared RPM,
  aggregate steam/torque, native bind/release, tender adapter.
- B99PhysicsState.cs:73 and ConventionalPhysics.cs:19,58 — existing friction and
  driver-share conversion; CylinderCalibration.cs:50 — retained cylinder ratios.
- WheelRearranger.Articulated.cs:299,331,343; Mallet.cs:120,263,397;
  BigMallet.cs:159; Triplex.cs:180,234 — physical mapping and shared animators.
- TenderEngine.cs:119,245,268; TenderDriveAnimation.cs:42 — attachment, teardown,
  old clamp and shared direction.
- ArticulatedEffects.cs:219,287,527,558; PoweredWheelRotater.cs:16;
  WheelRearranger.Curving.cs:68 — existing per-engine effects/followers and global
  slip consumers.
- WheelRearranger.Conventional.cs:183,200,231,260,278 and
  ConventionalSpawn.ReturningToPool — activation, stock, rollback and pooling.
- Installed WheelslipControllerSystem Calculate query versus Copy/Apply queries;
  AdhesionControllerSystem Calculate query; DVConvertToEntity and
  DVConvertToEntitySystem; DrivingForce.FixedUpdate; Bogie.ApplyForce;
  AnimatorPortReader, PoweredWheelRotationViaAnimation, WheelslipSparksController,
  WheelsAudioModule, TrainMassController and ResourceContainerController — native
  evidence inspected. Local decompilations remain private build evidence.

The first implementation deliverable is the native-helper feasibility result,
followed by a complete two-engine vertical slice. An uncalled helper or another
standalone scalar test is not a substitute for that integrated milestone.
