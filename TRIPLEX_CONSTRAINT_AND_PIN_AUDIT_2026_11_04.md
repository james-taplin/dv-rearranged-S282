# Triplex constraints, pin seating and clipping — 4 November 2026

This is the preserved pre-repair diagnosis. The subsequently authorised repair
and final offline results are in `FRANKLIN_REPAIR_2_0_9_4.md`.

James authorised correcting the diagnostic fixture and continuing investigation
with agents. Production geometry must remain unchanged; Derail Valley must not
be launched. This pass changed test code and documentation only. No production
build, installation, packaging, commit, push, publication or save mutation.

Current source is commit `c9c5d88`, version **2.0.9.4**. The installed version
remains **2.0.9.3-beta**. The hidden-linkage repair already committed for 2.0.9.4
is separate from the unresolved Franklin defects below. The 2-2-2 is excluded.

## Correction to the previous explanation

The source enables the main-rod and cutoff-crank renderers. That is a visibility
fix, not proof that the driver pin spans the rod bearing or that moving parts
remain connected. Calling the pictured missing pin fixed was too strong.

The supplied images show Franklin/Cardan gear. The wide red locomotive appears
to be the six-driving-axle Franklin triplex, ID47; the exact close-up arrangement
and screenshot build are unconfirmed. The open eye near pixel (296,333) in the
close-up probably belongs to the main connecting rod's wheel end, but that mesh
identification remains tentative. The higher hub is not a reliable substitute
for the driver-pin centre: the eccentric arm has a separate, intentionally
offset boss. A screenshot alone cannot establish the measured joint identity.

## Corrected native constraint fixture

The old triplex fixture held the front hinge in the body frame and assigned
middle yaw as 0.4 times front yaw. Production does neither. It configures native
Unity PositionConstraint/AimConstraint pairs, gives the middle aim two sources
(support 0.4, body-fixed rest target 0.6), and reparents the front hinge onto the
middle engine root with world pose preserved.

The fixture now extracts the actual production constraint methods and parenting
sequence. Unity evaluates these constraints through its normal player loop;
the fixture does not assign triplex engine angles. Native extraction measures
`LocoS282A/Axle_F` relative to `MovingParts_LOD0`: **(0,0,8) m**, with coincident
native car/body frames. Path IDs, matrices and native resource SHA256 accompany
that datum. Changed or missing frame evidence fails rather than silently using
the measured coordinates on another resource.

For triplexes, the `yaws` input now means a **synthetic support arc**, not engine
yaw or a live curve radius. Actual engine angles, hinge translation, constraint
sources and errors are reported. At nonzero stress the moving-hinge check
explicitly rejects the old fixed-hinge reconstruction. Two-engine Franklin and
Walschaerts fixed-pivot stress branches are unchanged.

The old triplex clearance results are superseded for articulation fidelity.
Earlier two-engine/Walschaerts evidence retains its stated scope. In particular,
old Franklin tests covered the native crank versus casing/crosshead, not all
main rods, shafts and pin connections.

## Bounded offline results

Only the diagnostic assembly was compiled: **0 warnings, 0 errors**. It reuses
the existing integrated production DLL, SHA256
`79FEDF9F34EDCD1E193BE84DDDDE23168974258F0D2898F185E7E69850DD58FA`.
That cached assembly predates the metadata-only version commit; its relevant
production geometry/animation methods are unchanged. All 29 historical
configuration-provenance entries still match the preserved baseline.

The existing isolated Unity 2019.4 player was used; it is not Derail Valley.
The old fixture first passed a 24-phase straight ID47 baseline. The corrected
24-phase native ID10/Franklin ID31/ID47 smoke completed 41 checks with no covered
surface contacts. These establish fixture operation, not attachment acceptance.

The corrected triplex sweep sampled 720 phases, two inter-engine phase patterns,
and support arcs -30/0/+30 degrees: 4,320 layout poses per ID, both sides.
All 331 fixture assertions completed, but **clearance failed**:

| ID | Actual front / middle yaw at +30 support | Front hinge movement | Detected pair/pose contacts |
| --- | --- | --- | --- |
| 47 | 21.5286° / 2.5940° | 170.077 mm | 380 crank + 296 main rod against following casing |
| 38 | 21.1478° / 2.1839° | 132.902 mm | 376 crank against following casing |
| 40 | 20.9265° / 1.9458° | 133.471 mm | 0 in the original crank/casing/crosshead pairs |

The 1,052 total is a count of sampled intersections, not separate defects.
ID47 contacts are front-engine crank/main rod against the middle cylinder,
left at negative support stress and right at positive stress. Saved triangle
snapshots and witnesses were independently reviewed; no concrete fixture false
positive was found. This is actual triangle intersection, not AABB overlap.

ID47 additionally checks both boundaries for main/coupling rods and all 18
rendered Franklin auxiliary meshes (shafts, U-joints, transmission and mounts)
against the following casing. No auxiliary or coupling-rod contacts were found
in those poses. ID38/40 retain the narrower crank pair inventory; their zero or
nonzero counts do not certify unmeasured parts.

A 120-phase, one-pattern ID47 onset check used support arcs 0/15/20/25/30°.
Actual front yaw was 0/10.4450/14.0346/17.7199/21.5286°. Covered pairs were clear
through 17.7199°; at 21.5286° it detected 15 right-crank and 13 right-main-rod
contacts. This brackets sampled onset; it is not an exact threshold or a
measurement of an in-game curve.

Native mechanism ID10 and Franklin ID31/32 controls each completed 4,320 poses
(720 phases, two patterns, -15/0/+15 fixed-pivot stresses, cutoff 0), with no
unexpected covered casing contacts. This includes the added rod/auxiliary
following-casing pairs for ID31/32. Their attachment measurements still expose
the separate driver-pin problem.

## Driver pin and floating rod

Circle fits use referenced mesh vertices, with maximum residual below 1 µm for
the principal resolved eyes/caps. Stock ID10 is measured before interpreting
Franklin. Main-rod far-eye geometry is irregular and remains unresolved; it is
never reported as a passing zero. Radial centre error is distinct from axial
seating, and a cap-ring interval alone cannot establish full pin reach.

The full-wheel and coupling geometry supplies the stronger axial evidence for
the inspected ID31/47 stacks (drive-local outboard X, mirrored on the left):

| Surface/extent | Outboard X |
| --- | --- |
| Entire referenced short-pin wheel mesh | at most 1.012687 m |
| Entire generated coupling rod, conservative bound | at most 1.012733 m |
| Main-rod near-eye inner face | 1.045563 m |
| Main-rod near-eye outer face | 1.106539 m |
| Entire native cutoff-crank mesh, innermost extent | at least 1.100175 m |
| Stock driver 3, entire long-pin mesh | reaches 1.191698 m |

Thus the native short-pin wheel/coupling stack ends at least **32.83 mm** before
the main-rod bearing's inner face. This conclusion uses full mesh extents, not
only the fitted cap radius. The outboard cutoff crank does not bridge that
inner interval. The stock main driver has a long pin and an overlapping bearing
interval. Merely enabling the Franklin main rod/cutoff renderer leaves this
stack connection unresolved.

An independent auxiliary check also excludes a hidden Franklin pin extension:
non-cylinder hardware stays at least 47.811 mm beyond the main eye's outer face
even under conservative bounds over authored shaft roll, while cylinder-mounted
hardware is longitudinally ahead of the near-eye envelope. These calculations
and their limits are preserved in `axial-bridge-proof.json`. The missing modeled
span is inherited in ID31 and ID47; it is not established as a new 2.0.9.4 fault.

Motion also differs from stock: native ID10's maximum measured main-eye versus
wheel-pin radial centre distance is 3.663 mm; ID31 reaches 13.503 mm, and ID32
13.054 mm. ID47's straight 24-phase smoke reaches 9.471 mm. Generated coupling
eyes are measured against their own nearest wheel pins; the opposite rod eye
is not incorrectly required to meet the main pin. These are measured deviations,
not a newly chosen fit tolerance or a certified repair. The crosshead/far-end
cloud also differs from stock, but independent far-eye seating is unresolved.

The current evidence supports James's attachment concern. It does not prove
which exact photographed eye corresponds to each measured part or certify the
Franklin transmission's complete intended load path. No animation, mesh, pin, wheel or
casing has been changed to hide these findings.

## Evidence and next bounded work

Private evidence is under `build/triplex-constraints-audit-20261104/`:

- `baseline-inputs/`, `baseline-47-straight-24/`
- `generated/`: exact extracted configuration, native support proof and geometry
- `corrected-smoke-10-31-47-straight-24/result.json`
- `native-triplex-720/result.json` and saved contact geometry
- `controls-10-31-32-720/result.json`
- `native-47-onset-120/result.json`
- `axial-bridge-proof.json`, `verification-receipt.json`

Keep the two repair targets separate: driver-pin/bearing seating within an
engine, and front-to-following-cylinder clearance under native articulation.
The next authoring decision should first test retaining/adapting the stock long
pin through the existing mesh facilities, while preserving wheel and rod
animation. Do not add a new motion system, enlarge engine spacing or reshape
another cylinder without a measured candidate. Current authorization stops at
diagnostics; production geometry is deliberately unchanged.

James's specific in-game checks, on the current installed build:

1. Record arrangement name/ID, mod version and effective valve-gear settings.
   The exact close-up arrangement remains unknown.
2. On straight track, stop and move slowly through one wheel revolution on each
   side. Inspect the main-rod wheel eye, the wheel pin behind it and the
   eccentric/crank part outside it; capture a view along the pin axis and an
   oblique view that reveals any axial gap. Do not use the upper eccentric boss
   as the expected wheel-pin centre.
3. On a curve, inspect the front crank/main rod against the middle cylinder on
   the inside of the curve; repeat the opposite direction for the other side.
   Capture the same wheel phase on straight track and curve if practical.
4. Include the middle-to-rear boundary and note whether the defect is a missing
   pin, a floating bearing, or surfaces crossing a cylinder. These are separate
   observations; a camera view can show more than one.

Actual curve envelope, camera/LOD behaviour, switching/paint/lifecycle effects
and reporter acceptance remain unverified. Native constraints use synthetic
support inputs in a mechanism fixture, not wheel/track physics or a complete
TrainCar. Surface tests do not cover wholly enclosed volumes or coplanar
contacts. The bounded offline pass is complete; gameplay testing remains James's.
