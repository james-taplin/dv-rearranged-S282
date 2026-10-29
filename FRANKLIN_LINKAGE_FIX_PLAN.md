# Franklin articulated linkage fix plan

28 October 2026; Rearranged S282 only

The reporter's arrangement is unknown and the screenshot is from 2.0.3. The
two-driver Franklin geometry and animation remain substantially unchanged in
2.0.7-beta. That comparison establishes that no specific correction was made;
it does not reproduce the exact photographed pose or establish its cause.

## Requirements

- Keep driving force, steam use, wheel radius, adhesion and articulation intact.
- Preserve native animation timing and left/right quartering.
- Correct visible linkage attachment and clearance rather than hide the part.
- Use existing native constraints, authored geometry and rod tools first.
- Stage a separate test package; do not install or publish automatically.

## Work

1. Measure the existing motion. Trace the authored Franklin clips, short main
   rod, coupling-rod bosses, crank pins and neighbouring cylinder positions.
   Sample a complete revolution, not only the structural test's resting pose.
   Two agents independently check attachment and inter-engine clearance.
2. Reproduce a measurable failure using the actual source geometry and curves.
   Separate disconnected bearing centres, within-engine interference and
   interference with the next engine. Record affected layouts and phases.
3. Apply the smallest supported visual correction to confirmed failures.
   Prefer an existing geometry datum or native constraint over a new motion
   solver. Keep source animation and simulation components unchanged. Check
   both sides and every arrangement that shares the corrected mechanism.
4. Add a regression that fails on the old geometry and passes on the corrected
   geometry across a revolution. Retain quartering, rod/oilcup attachment,
   switching/restoration and relevant lifecycle checks. Independently audit
   the implementation and compare compiled physics methods with the baseline.
5. Build and stage a complete test ZIP with the existing exporter/licences and
   plain-text instructions. Update this plan and the handover with measured
   results, remaining in-game checks and the exact source checkpoint.

## Results

The plan has been executed through implementation, automated validation and
test packaging. Three agents investigated attachment and clearance separately
and reviewed the final changes.

- Straight-track sweeps did not reproduce crank contact with the following
  cylinder. The actual packed animation matters: a conservative sum of the
  crank's radius and its orbit incorrectly suggested a straight-track clash.
- The 4-4-4-4 does intersect the following casting in synthetic articulated
  poses. With the actual native crank and original bundled casting, the sweep
  recorded 382 intersecting poses. The corrected casting records zero across
  720 phases, both sides and nine yaw cases through 15 degrees.
- Only the front end of the rear 4-4-4-4 castings is shortened, by 0.12 m.
  Rear piston faces and shaft datums stay fixed; front-cap vertices translate
  rigidly. There are no flipped or collapsed faces. Existing owned-mesh cleanup
  restores the originals on switching.
- The front cylinder-cock outlet follows the corrected casting using its
  emitter centre in the fixed native body frame. This was necessary: leaving
  it unmoved increased the measured casting-profile gap from 90 to 350 mm.
  Moving it with the nose restores the original 90 mm profile. The adaptive
  steam pipe follows its evaluated socket, which moves about 14 mm.
- Shared Franklin setup now restores main-rod and crank renderers explicitly.
  Baseline tests reproduce inherited disabled renderers in duplex and triplex
  setup. Meshes, pin coordinates and the hidden Walschaerts parts are retained.
- The structural report checks the presented main rods and cranks, including
  retarget proxies, and requires the two corrected castings only on 4-4-4-4.

Validation: clean build, 950 renderer checks, 4,705 production map/socket checks,
7,668 outlet-mapping checks, 820 quartering/lifecycle plus 180 linkage-report
checks, and 1,740 structural-validator checks. Existing casing, tender-audio and
effect-cleanup checks pass. Compiled comparison preserves 1,146 methods outside
the intended presentation/diagnostic changes, explicitly including driving
force, outlet base mapping, quartering, native axle and articulation methods.

Test package: build/franklin-linkage-test/package/RearrangedS282-B99-2.0.7-linkage-test.zip.
It is the complete 36-file package, including the 13-file exporter, existing
assets/licences and plain-text test instructions. The prepared Nexus beta ZIP
has not been overwritten. Nothing was installed or published.

The reporter's exact arrangement and curve remain unknown. These changes fix
reproduced renderer and conditional clearance faults; they do not establish a
reproduction of every feature of the screenshot. Low-speed forward/reverse
driving on a curve, both-side inspection, cylinder-cock steam and a fresh
structural run remain the in-game acceptance checks.
