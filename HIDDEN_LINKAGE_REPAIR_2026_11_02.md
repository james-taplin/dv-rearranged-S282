# Hidden S282 linkage repair — 2 November 2026

Implemented in the current `rearranged-s282-multiplayer` checkout, based on clean
`6f850b2`. Read `CURRENT_HANDOVER.md` from line 1 before investigating. Installed
metadata and both DLL hashes match the recorded 2.0.9.3-beta installation from
source checkpoint `3540e61`. At the 2 November validation, the repair was an
uninstalled, uncommitted candidate compiled as 2.0.9.3-beta. On 4 November James
requested committing this repair as exactly **2.0.9.4**. Source version metadata
was updated; the earlier private DLLs and validation receipts retain their
original version and hashes. No new build, packaging, push or publication.
All 38 installed package members still hash-match the installation receipt.
The 2-2-2 is excluded. James performs DV testing; DV was never launched or automated.

## Proven native geometry and method

The existing hides are deliberate: resized mechanisms terminate at an unchanged
cab or separately moved mechanical oiler. Re-enabling the original meshes fails.
Exact installed resource geometry, the native hierarchy/pivots and three native
cutoff clips at 16 wheel phases establish the following 4-14-4 baseline:

- Native fourth-wheel input produces scale 0.691723934, cylinder offset
  3.150596505 m and driver offset 2.650596505 m in the body frame. These are the
  current production fit, including its 0.50 m driver displacement.
- Simply showing the reach leaves rear-tip error 2.5650–2.5823 m at LOD0 and
  2.4477–2.4585 m at LOD1 over those native poses. Lubricator far-tip error is
  0.5294–0.5484 m.
- Reach mesh rear-tip datums are Z -6.07456827 m (LOD0) and -5.74456787 m (LOD1).
  LOD1 has its own offset and -2.5-degree child rotation. These are cut shafts
  entering the cab, not modeled rear bearings. The repair preserves their native
  cab-entry trajectories rather than inventing a rear joint.
- The native lubricator rod/lever trajectories fit a rod axial datum
  Z 1.9299124125 m and lever-local point (-0.0356411934, -0.1558446948,
  -0.0000924605) m. Native authored residual is 0.110–2.592 mm. The LOD1 rod
  datum includes its own child translation. This fit uses only unmodified native
  poses; candidate geometry is not used to fit acceptance.
- Native reach scale is animated, including 0.995/1.0325/1.0625 at the first
  High/Med/Low samples. Preserve it. Removed the twelve-coupled pre-sample 1.2
  approximation; no runtime animation transform is replaced or rewritten.

Existing `DriveRetarget` constant-Z shift cannot solve an independent cab/oiler
endpoint, especially under articulation. Its proxy meshes, readable-mesh provider,
LOD replacement, `RodRemesher` and teardown do fit. The repair extends that owner
with a fixed-endpoint entry and an anchored-part entry; no dependency, new crank
clock, physics system, animation controller or simulation writer is added.

Capture each native drive in the car and oiler frames before arrangement edits.
Each frame, retain the actual gear-side pivot, recover native endpoint motion in
the captured fixed frame, change only the rod shaft and aim its presentation at
that endpoint. Each mesh uses its own axis/datum. A 50 mm protected end zone
exceeds the measured reach boss (~34.4 mm) and lubricator end (~13.8 mm).
Lubricator lever renderers follow the moved oiler frame while their original
animated transforms and native ratchet-driver angle remain untouched.

Endpoint entries join any existing main/radius retarget owner. LOD backups are
captured once per group, sampled proxy bounds join the native envelope on each
update, and shared owners unwind in reverse order before geometry snapshots.
Conventional LOD0/LOD1 membership and existing articulated detail-through-far-cull
policy are retained. Actual native L/R LODGroups include all target detail parts.

Actual AssetStudio OBJ export reflects X and reverses winding. A real export
check exposed this after the initial integrated build; the mocked readable copy
had not represented it. Only the six new linkage imports are normalized back to
native coordinates, per submesh. Endpoint imports bypass the older raw-OBJ cache
path. Existing main/radius/tender import behavior is unchanged. This concrete
authoring correction justified the second integrated build; fixture/document
changes did not trigger further production builds.

## Applied scope and limits

Conventional IDs: **0,1,2,3,4,5,12,13,14,27,28,29,30,34,35,39,44,48**.
Their own source-derived drive/oiler transforms, clearance and spacing order are
used; no 4-14-4 arrangement coordinates are copied into another arrangement.
The implementation reads the resulting effective cutoff-rail visibility, so the
existing four/six/large-eight/twelve and polished Pacific/Hudson/Prairie options
still suppress the repaired links when their valve gear is hidden.

Articulated Walschaerts IDs: **37,41,42,43,45,46**, original front L/R drives only.
Their own previously measured native maps and hinge positions are reused only
after matching current source members/constants and native evidence hashes.
Sampled yaw is -15/0/+15 degrees about each layout's own hinge. This is a new
linkage geometry check, not a rerun or extension of earlier contact-clearance claims.

Rear-engine and Erie tender copies have no demonstrated separate cab/oiler
attachment and remain hidden. Franklin IDs **31,32,38,40,47** deliberately replace
stock Walschaerts gear/controllers; none of those replaced parts is restored.

The fixed LMS mid-shaft reach guide is not a cab attachment. Its selected native
vertex envelope is body X 0.1751–0.7535, Y 1.6331–1.9293, Z 5.7814–6.0501 m.
It misses the changed straight shaft. It remains hidden at both body LODs, including
the previously visible 4-14-4 guide; no guide or pedestal is fabricated. This
bounded repair does not certify a complete engineered linkage/support redesign
or clearance against every locomotive body component.

## Verification and reproducibility

`evidence/hidden-linkages/README.md` describes native input provenance and commands.
All generated native meshes, exported OBJs, player payloads and binaries remain
under ignored `build/`; none is distributable content.

1. Baseline fixtures were run before relying on them. Lubrication passed
   59 seat + 38 lubricator + 4,067 coupling + 5,349 pitch checks. Two old fixtures
   are pre-existing failures: `Validate.FourteenSpacing.ps1` assumes HEAD precedes
   a 0.25 m adjustment already in HEAD, and `Validate.PacificHudsonValveGear.ps1`
   lacks its Dummiesman importer fixture namespace. Production was not changed
   to satisfy these stale fixtures. An ID39 projection also initially used a
   reset 0.71 m wheel datum; it was corrected to the prepared native input before
   accepting the new fixture. This was a fixture correction only.
2. Final managed exact-geometry fixture: **1,220,391 assertions**, 576 direct
   sampled poses, 1,728 integration poses, 72 visible/hidden cases across all
   24 affected layouts, and 25 excluded IDs. Maximum recovered endpoint error
   **7.731 micrometres**; stock lubricator contact residual retained with a
   **2.600 mm** numerical bound. Includes 24 actual OBJ imports and 24 raw-X
   negative controls; original main/radius proxy coexistence; native-frame
   vertex identity; animated-source/asset immutability; LOD bounds and identity
   restoration; scoped cache/apply/suspend; one failed-export cleanup.
3. Final integrated native-reference core + optional adapter build:
   **zero warnings/errors**, no package. Output: `build/hidden-linkages/integrated-bin`.
   Final core SHA256 `79FEDF9F34EDCD1E193BE84DDDDE23168974258F0D2898F185E7E69850DD58FA`.
4. Actual compiled production OBJ importer and normalization in the preserved
   isolated Unity 2019.4 player: all six meshes match native geometry, oriented
   triangles and single material sections; maximum vertex error **0.477 micrometres**.
   Null material slots omit shader creation only; native materials are supplied
   by the production proxy. No DV process, game profile or save was used.

Receipts: `build/hidden-linkages/validation.json`,
`build/hidden-linkages/native-importer/result.json`,
`build/hidden-linkages/integrated-build-final.log`, and
`build/hidden-linkages/repair-receipt.json`. Source and fixture hashes distinguish
authoring, retained native-map evidence, and offline fixture provenance.

These checks do not evaluate live Animator blending, native ratchet behavior,
rendered appearance/camera culling, complete TrainCar switches, saves or actual
gameplay. Native animation/simulation code is retained, not newly certified by
the geometry fixture. Gameplay acceptance is **awaiting James's testing**.

## James's in-game checks after separately authorized installation

1. Start with 4-14-4. Inspect both reach rods at the gear end and cab entry,
   and the right rod/lever/oiler connections. Sweep full reverse, mid cutoff and
   full forward while stationary, then roll slowly forward/reverse through full
   wheel revolutions. Watch for separation, phase lag, distorted ends and clashes.
   Check automatic/manual lubricator operation and the original ratchet motion.
2. Check one member of each repaired conventional group, then any remaining
   affected arrangement of interest. Toggle the relevant existing valve-gear
   option and reapply; hidden gear must stay hidden. Switch between repaired
   arrangements and stock 2-8-2 repeatedly; stock meshes/visibility must return.
3. Check all six Walschaerts articulated layouts on straight and curved track,
   viewing both front-drive reaches and right lubricator at different yaw.
   Check near/far viewing distances for disappearing or duplicated geometry.
   Rear/tender copies remain absent; Franklin must retain its replacement gear.
   Record arrangement, side, cutoff, wheel position, curve and screenshot/clip
   for any defect. No 2-2-2 test or change is part of this repair.
