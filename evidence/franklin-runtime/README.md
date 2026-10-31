# Franklin and Walschaerts mechanism runtime regression

This is a private Unity 2019.4.40f1 player probe, not a shipped mod feature. It
does not launch the game, change installed files, create a package or touch saves.
Native geometry exports and compiled probes remain under ignored `build/`.

## Findings on 31 October 2026

The expanded Walschaerts sweep found a concrete defect in **ID 37, 2-6-6-2**:
its front eccentric crank bearing and main rod entered the following engine's
shaped cylinder nose at +/-15 degrees of articulation. The original .100 m
between-engine gap produced 1,200 unexpected pair/pose contacts in the full
720-phase, three-cutoff, three-yaw sweep. A 120-phase yaw-onset check at 0, 2, 4,
6, 8, 10, 11, 12, 13, 14 and 15 degrees found contact only at 15 degrees. These
are controlled yaw stresses, not a reproduced live route or confirmed reporter
arrangement. The same Walschaerts layout/shaping code was present in 2.0.7b, so
this evidence does not establish a new geometry regression.

The production repair introduces a dedicated .150 m cylinder-clearance gap for
ID 37. Only the rear-engine separation changes; the shared .100 m BigMallet and
trailing-wheel clearance stays unchanged. The complete rear engine and its
static cylinder/support assembly move back 50 mm, the articulation pin moves
back 25 mm, and the trailing axle/support move back 50 mm. Native gear, meshes,
driver size and front engine stay intact. The trailing axle ends at body Z
.341034 m, still .241034 m ahead of the native twelve-coupled rear-axle datum.

The private .150 m candidate completed 12,960 poses with no unexpected contacts.
Conservative sampled triangle-surface separation lower bounds are 42.308 mm for
both crank bearings and 42.736 mm for both main rods. Both exceed the probe's
10 mm minimum margin. The final source run has no gap override and uses the
dedicated constant extracted directly from current production source.

`build/franklin-runtime/final-source-all-720/result.json` is the final combined
source gate: native mechanism control ID 10, all five Franklin layouts, and all
six Walschaerts articulated layouts (37/41/42/43/45/46), including the Erie
tender engine's own gear in its native tender frame. It passed 547 checks and
112,320 sampled layout poses with `clearancePassed=true` and zero unexpected
contacts. The effective production ID 37 gap is .150 m, with no fixture override.
Tracked original-controller and phase-adapter poses agree to floating-point
precision (largest recorded comparison value 4.78e-7). Walschaerts is sampled at
Cutoff -1/0/+1; native serialized Cutoff reader multiplier 1 and offset 0 are
checked against the actual local-game object bytes and preserved by phase
ownership. All following-crosshead pairs additionally check 720x720 independent
phase combinations per yaw/cutoff using captured evaluated poses. Disjoint swept
bounds reject the full combination set before triangle work when possible.

The native valve spindle enters its own valve chest in the control and every
scaled engine. That intended native entry is retained, counted and classified
separately; every-pose matching counts are required. It is not hidden or deleted
to make a clearance test pass. `complete` only means measurements finished;
`clearancePassed` and `unexpectedContacts` are the acceptance fields. Requested
negative controls can have `passed=true` while `clearancePassed=false`, with
`expectedNegativeControlMatched=true` explicitly recording why.

The final old-gap negative control is
`build/franklin-runtime/final-old-gap-720/result.json`: it restores .100 m only
in the private fixture, reports `clearancePassed=false` and the same 1,200
unexpected contacts. The final Franklin old-nose control is
`build/franklin-runtime/final-old-nose-720/result.json`, with 648 contacts.

`build/franklin-runtime/sweep-readable-fidelity-720/result.json` completed all
five Franklin layouts: IDs 31, 32, 38, 40 and 47. Each used 720 phase samples,
three yaw stresses (-15, 0, +15 degrees), and two inter-engine phase patterns.
That is **21,600 sampled layout poses**, with both sides of every engine checked.
No triangle surface intersections were found between a crank and its own
crosshead/cylinder casing or the following engine's crosshead/casing. This is
coverage of those pairs and poses, not proof of clearance for every locomotive
part or the unidentified reported arrangement.

The production `GroupCrankAnimation` adapter was compared with direct
`Animator.Play` normalized-phase requests on a second instance of the same
original controller, both evaluated by Unity's normal player loop. Tracked
crosshead/crank/wheel poses matched within floating-point precision. Separate assertions confirmed that
crossheads moved and wheels rotated, so frozen matching poses cannot pass.
This does not reproduce the complete historical 2.0.7 physics/clock integration.

The negative control
`build/franklin-runtime/sweep-old-nose-720/result.json` disables only the existing
ID 32 rear cylinder nose correction. It detects **648 actual triangle surface
contact samples**: 324 on each side. The first left contact is phase 0.159722224,
yaw -15 degrees; the first right contact is phase 0.4097222, yaw +15 degrees.
Both are the front crank entering the following cylinder nose. Exact world
triangle snapshots and witnesses are saved as `contact-32-2.json` and
`contact-32-6.json` beside that result. Restoring the current correction removes
these contacts. This demonstrates that the probe detects the previously known
fault and that its current repair survives actual controller evaluation.

**No additional Franklin geometry defect was reproduced or changed.** The
Walschaerts ID 37 contact above is a concrete repair target, while the exact
reported arrangement remains unidentified. A report identifying the arrangement,
side and gear part, ideally with a screenshot or short clip and the same
structural dump, is needed to connect the remaining report to a concrete defect.

## Fidelity

- The native resource fixture contains the original complete S282 right drive
  hierarchy and controller. Left geometry is mirrored as in production. Both
  exact serialized native hierarchies and mesh triangles are independently read
  from the installed game's `resources.assets` by UnityPy. Missing descendants
  fail the probe instead of silently substituting a wheel.
- Original Franklin prefab assets and packed controllers are loaded directly
  from the unchanged `s282_drivetrain` bundle. Their authored Clone names remain
  intact for native animation bindings.
- `prepare.py` extracts current production configuration method/class bodies,
  the current dedicated Mallet gap literal and original side-rod recipes, and
  records provenance. Only asset-provider identifiers are
  replaced to avoid starting `MeshFinder`'s exporter. Main/coupling rod geometry
  uses the production OBJ parser and existing index recipes. The headless
  fixture provides null material entries because shaders are omitted; mesh
  parsing and geometry are unchanged.
- The privately compiled production assembly supplies `DriveRetarget` and
  `GroupCrankAnimation`. Retarget runs through normal `LateUpdate`. Crucially,
  original bundle mesh readability and identity are retained: separate readable
  triangle copies are used **only for measurement**. Making all bundle meshes
  CPU-readable would choose a different retarget branch. The current casing
  correction uses the same readable OBJ fallback as production.
- Triangle contacts use a mesh-local BVH and actual triangle/segment
  intersections. Bounds overlap alone is recorded separately and never called
  contact. Known old nose geometry is a required negative control.
- The installed LMS splitter/importer executes in a private Unity player for
  the Walschaerts static payload. Its actual output supersedes the independent
  source replay; that replay omitted preceding edits affecting guides. Cylinder
  geometry independently matched actual output within 0.31 micrometre. Both
  loader and native-body transforms, including -X reflection and +Z4.88, are
  retained explicitly.
- Production `BindArticulatedLods` supplies one generated detail-geometry LOD
  through the native far-cull threshold and hides rigid coarse proxies. The
  measured articulated meshes are the drawn mechanism geometry at near/far
  distances; actual camera culling and visibility are not exercised here.

The detached native RSE initializes only its native crank-phase `Port`. This
probe does not test cylinder simulation, live TrainCar lifecycle, native wheel
physics, multiplayer, camera/LOD selection, the full static locomotive frame
or guides. Yaw stresses rotate around production pin datums; they are not live
track/constraint evaluation. Coplanar and wholly enclosed volume contacts are
not detected. Contacts still require visual/engineering classification before
changing production geometry because intentional joints also intersect.

## Reproduce

First create the private native-resource player template through the existing
runtime-bootstrap workflow. The expected template is
`build/native-drive-calibration/player`; its resource is
`Resources.Load<GameObject>("NativeS282DrivetrainR")`. If unavailable, the probe
explicitly reports reconstructing the complete serialized native hierarchy
instead. No Unity Editor instance is started by the commands below.

From the repository root in PowerShell:

```powershell
./Build.FranklinRuntimeProbe.ps1
./Run.NativeRuntimeProbe.ps1 `
  -ProbeDirectory "$PWD/build/franklin-runtime/probe-bin" `
  -AssemblyName FranklinRuntimeProbe.dll -TypeName FranklinRuntimeProbe `
  -OutputDirectory "$PWD/build/franklin-runtime/new-current-run"
```

For all Franklin/Walschaerts arrangements plus the native mechanism control,
first generate the current actual installed-LMS payload, then build and run:

```powershell
./Validate.LmsStaticGeometry.ps1 `
  -OutputDirectory "$PWD/build/franklin-runtime/lms-native-regeneration" `
  -PayloadDirectory "$PWD/build/franklin-runtime"
./Build.FranklinRuntimeProbe.ps1 -Ids @(10,31,32,37,38,40,41,42,43,45,46,47)
./Run.NativeRuntimeProbe.ps1 `
  -ProbeDirectory "$PWD/build/franklin-runtime/probe-bin" `
  -AssemblyName FranklinRuntimeProbe.dll -TypeName FranklinRuntimeProbe `
  -OutputDirectory "$PWD/build/franklin-runtime/new-all-mechanisms-run"
```

For the required negative control, use a fresh run directory:

```powershell
./Build.FranklinRuntimeProbe.ps1 -Ids 32 -OldNoseControl
./Run.NativeRuntimeProbe.ps1 `
  -ProbeDirectory "$PWD/build/franklin-runtime/probe-bin" `
  -AssemblyName FranklinRuntimeProbe.dll -TypeName FranklinRuntimeProbe `
  -OutputDirectory "$PWD/build/franklin-runtime/new-old-nose-run"
```

`Build.FranklinRuntimeProbe.ps1` accepts `-GamePath`, `-PythonPath`, `-Steps`,
`-Ids`, `-Yaws`, `-OldMalletGapControl` and `-WalsStaticPath`. The old-gap control
should be run with `-Ids 37` and a fresh output directory. Python must have UnityPy installed. It extracts local native
geometry, generates current configuration code, builds production/probe DLLs
privately, and writes `franklin-config.json`; it does not run a player or package.
The initial 24-step calibration is recorded under `calibration-04`. Earlier
calibration failures concerned fixture shader/dependency setup. The first
`sweep-current-720` used readable bundle mesh substitutions and is superseded by
the fidelity-corrected sweep cited above.
