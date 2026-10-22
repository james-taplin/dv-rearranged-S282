Installed1.22.3 TEST on22October at James's request; exact audited ZIP,43files
hash-verified,40local files retained. No new game test performed. Earlier
not-installed statements below describe the build-time audit, not current state.

# Current pipe extension audit - 22 Oct 2026

Candidate1.22.3 now includes the guide/casing fixes below AND source-derived
pipe extensions. Earlier guide-only candidate/hash superseded. Installed1.22.2
unchanged. Test candidate, not a release approval.

Three agents helped with native geometry, matching mesh extensions and production
surface/lifecycle audits. Original transfer/manifold assets unchanged. New pieces
replace only routes/fittings unable to clear retained body, tanks or plumbing.
The interfering native part was a flange rim, not an optional bracket: only the
annulus was compacted and the shaft retained. Complete sampled boundary seating
replaces centre-only assumptions. Prairie skin stays within the original transfer
UV island, avoiding interpolation across unrelated native atlas regions.

Final evidence under build/steam-duct-candidate-4:

| Coverage | Result |
|---|---|
|44neutral arrangements, static nativeLOD0+LOD1 obstacles|Zero detected crossings/errors|
|Neutral native cylinder-side clip samples|2040queries,zero crossings|
|76neutral/synthetic articulated poses|All generate; no presentation errors|
|All imposed +/-6degree poses|Zero tested static crossings|
|Imposed +/-12degree stress|Front engine/body contacts remain;8uniqueLOD0/7LOD1|
|Managed owner/LOD/isolation|8488checks; max endpoint gap.0308mm|
|Original-profile mesh/UV/winding|113958checks|
|Native fit/compact collar|3140checks;13LOD0/7LOD1air contacts becomezero|
|Protected simulation/motion methods|105unchanged;279checks|
|Retained Erie exhaust/ownership|66checks|

Synthetic pivots are reconstructed, not measured real game curves. Full Franklin/
main-rod solid motion, minimum distance, coplanar/nested overlap and actual
rendering remain outside these tests. Prairie is a short cast offset rather than
a constant-bore swept elbow; assess its profile and material in game.
GEOMETRY_TEST_STATUS.txt records the focused checklist.

Candidate4 source hashes were stable before/after compilation; IL confirms.22m
Prairie bow. Candidate3 read an in-flight.12version; use candidate4 only. Native
fixture exports never ship. No installation/publication performed.

---

Prior guide/casing audit and initial pipe findings (historical):

# Geometry audit - 22 October 2026

Installed: **1.22.2**. Candidate: **1.22.3 test**, built but **not installed or release-ready**.

## Findings and status

- **Twelve-driver guide / short valve spindle: corrected in candidate.** IDs27-30 scale the animated gear to .84 while the old cylinder/guide offsets differed. Native sample evidence put the valve spindle axis about106mm below its bore and its forward end178-202mm behind the old rear face. The existing static-fitting alignment helper now seats the cylinders and guides in the animated frame. Full casing normalization preserves near-native barrel dimensions around the correct motion axes. Both sides and both static LODs are covered. No animation curves, driver positions or simulation changes.
- **Related static guide alignment: corrected in candidate.** Four-coupled and remaining six-coupled layouts now use the same existing helper, including when optional valve gear is hidden. Ten-wheeler/Mogul cylinders also use full normalized casing treatment. Their previous cylinder-only offset did not match the valve spindle axis. This shifts the cylinder casing to match the existing motion; it is not a new wheel-spacing adjustment. The resulting pipe routes still need the work below.
- **Connecting/coupling rods: tested geometry passes.** Actual native mesh/clip samples, accepted pitch and displacement settings, both sides and LODs, shaft deformation and protected bosses, phase following and cleanup were exercised. Main/crosshead and radius/lever errors stayed below .4mm in the precision-spacing cases. This is not full-solid clearance against every frame fitting.
- **Pipe clipping remains.** Surface-crossing analysis finds running-board contacts on four-coupled/Pacific/Hudson branches; reservoir contacts on several articulated branches/headers/manifolds; front-step/pilot contacts on some forward manifolds; and contacts with native compressor air pipes. Endpoint/fold tests passing did not cover these obstacles. Corrected Ten-wheeler/Mogul socket placement also needs its branch route checked before release. Do not install this candidate as a finished polish pass.
- **Franklin gear remains a specific open check.** Source paths, guide placement, shortened rods and shared scaling were inspected. The original TODO still lists end-of-stroke valve gaps and incomplete Franklin modelling. These are historical notes, not proof that the current version reproduces every old defect. Custom Franklin clips/short rods and full articulated/tender clearance have not received an exhaustive triangle/continuous-motion check here.
- **Experimental exclusions retained.** 2-2-2 intentionally has hidden unfinished rods; 4-0-4 intentionally has no drive gear. Stock S282 is unchanged. Erie tender exhaust clearance retains its separate native coal/hatch/ladder checks and James's prior game acceptance.

## Coverage across all 47 arrangements

| IDs / family | Mechanical review | Remaining qualification |
|---|---|---|
| 0-2,35 four-coupled | Native-frame guide correction; normalized casing, coupling/main rod checks | Body and pipe contacts still need routing work |
| 3-5,34,44 six-coupled | Native-frame guides/casings; Prairie and spacing motion fixtures | New 3/34 casing placement needs pipe routing check; no live acceptance of this candidate |
| 6-11,15-17 native/rear-shift eight-coupled | Native linkage frame retained; source/mesh/phase review | Full solid/curved-scene clearance remains a game check |
| 12-14 high-speed | Existing common-frame mapping and reservoir wheel-clearance retained | Native air-pipe contact review |
| 18-26 ten-coupled | Native mechanism frame retained; extra wheel/rod phase review | Does not prove every shoe/spring/frame clearance |
| 27-30 twelve-coupled | Measured mismatch corrected; actual guide vertices and native clip samples checked | Reroute/review branch near native air plumbing |
| 31-32 original articulated Franklin | Source guide, short-rod, clone/controller/LOD paths reviewed | Full custom-clip contact and curve sweep still open; pipe reservoir contacts found |
| 33 zero-driver novelty | Intentional absence of rods confirmed | No polishing of nonexistent drive gear |
| 36 giant 2-2-2 | Intentionally unfinished hidden rods retained | Remains on special measures |
| 37,41-43,45-46 Mallets / Erie loco | Shared engine mapping inspected; native rod ownership and compiled catalogue checks | Several header/manifold contacts; live articulation/tender clearance open |
| 38,40 Franklin triplexes | Existing cylinder/driver maps and native-clock retarget reviewed | Custom gear full-cycle clearance open; truckless front manifold/steps contact |
| 39 rigid 4-14-4 | Existing main/radius retarget and native poses tested | Native air-pipe contact review; full solid clearance open |

## Pipe intersection audit

The actual production pipe builder was fed the body-space cylinder sockets recorded in the latest structural JSON (version1.22.0, 47 passes). The 44 arrangements with generated pipes were tested against retained native body triangles, separately positioned reservoirs and native air pipes, plus sampled native cylinder-side crosshead/valve/piston parts. Stock uses native pipes; the two unfinished/zero-driver exceptions have no generated connections. A second export recalculates the new rigid-family sockets. These exports do not execute the entire WheelRearranger or a game scene.

Native boiler-seat contacts within .15m of a recorded boiler attachment are treated as intended seating. Non-coplanar triangle surface crossings elsewhere are **contacts for review**, not a count of independent visible bugs. Some fragments/air pipes may have additional runtime ownership or customization transforms not available in the capture; confirm the route in game. No general all-clear is implied for layouts without a crossing. Coplanar contacts, nested solids, minimum clearance, accessory mods, continuous articulation and all moving rod solids are outside this test.

| Arrangement | Offline contact surfaces requiring review |
|---|---|
| 4-4-0 American | retained body / board / steps |
| 4-4-2 Atlantic | retained body / board / steps |
| 4-4-4 Reading | retained body / board / steps |
| 4-6-2 Pacific | compressor inlet pipe, retained body / board / steps |
| 4-6-4 Hudson | compressor inlet pipe, retained body / board / steps |
| 2-8-0 High-speed Consolidation | compressor inlet pipe |
| 2-8-2 High-speed Mikado | compressor inlet pipe |
| 2-8-4 High-speed Berkshire | compressor inlet pipe |
| 0-12-0 Twelve-wheel switcher | compressor outlet pipe, retained body / board / steps |
| 2-12-0 | compressor outlet pipe, retained body / board / steps |
| 2-12-2 | compressor outlet pipe, retained body / board / steps |
| 4-12-2 Union Pacific | compressor outlet pipe, retained body / board / steps |
| 2-4-4-2 Articulated | compressor inlet pipe, reservoirs |
| 4-4-4-4 Articulated | compressor inlet pipe, reservoirs |
| 2-4-0 | retained body / board / steps |
| 2-6-6-2 Articulated Mallet | reservoirs |
| 4-14-4 | compressor inlet pipe, compressor outlet pipe |
| 0-2-2-2-0 Articulated Triplex | retained body / board / steps |
| 0-8-8-0 Angus | reservoirs, retained body / board / steps |
| 2-6-8-0 Gt. Northern | reservoirs |
| 2-6-2 Prairie | compressor inlet pipe |
| 2-6-6-0 Articulated Mallet | compressor inlet pipe, reservoirs |
| 2-10-10-2 Articulated Mallet | retained body / board / steps |

## Validation evidence

- `build/geometry-audit/guides/validation.json`: 384 added cutoff-pose scenarios across four/six/twelve/reservoir-clear frames plus96 original scenarios; visibility toggles, both sides/static LODs, unchanged animated position/rotation, restoration.
- `casings/validation.json`:369141 checks,1536 native pose scenarios; sixteen full-normalization layouts, real native casing/fitting and guide vertices at both LODs.
- `connecting/validation.json`:30 cases,2880 poses,746496 face checks; worst main/crosshead .364mm, radius/lever .318mm; zero added LOD escape relative to native baseline.
- `coupling/validation.json`:180 actual-mesh deformation cases, protected bosses, both list directions and source immutability.
- `fourteen/validation.json`:96 poses; existing layout retained, worst endpoint difference .0342mm.
- `prairie-motion/validation.json`:672 poses,2016 cup samples, accepted Prairie clearance/spacing retained.
- `pipes/validation.json` and `actor-validation.json`: production pipe shape/endpoint and managed owner/LOD tests; no inward shaft faces in tested sweeps, no endpoint gaps above .031mm in actor tests. These passed before the body-contact audit and do not invalidate its findings.
- `protected/validation.json`:279 checks, **105 protected compiled methods identical** to installed1.22.2; no physics/retarget redesign.
- `articulated-integration.json`:152 compiled catalogue/ownership checks; actual1.22.3 candidate, not the old default validator assembly.
- `rod-timing.json`:6720 simulated rendered poses,20-462km/h and20-144fps, actual follower source + compiled callback inspection. This checks phase following, not attainable speed or rendered acceptance.
- `recorded-pipe-clearance/clearance.json`: neutral recorded sockets; `pipe-clearance/clearance.json`: candidate socket reconstruction. Contacts are deliberately reported rather than hidden behind a passing endpoint result.

All fixtures use managed Unity stand-ins and local native geometry. No new in-game test was performed. No native geometry exports or audit captures are shipped in the test ZIP. Old test harnesses needed small API/duplicate-filter updates; these were fixture failures, not new runtime exceptions.

## Previous next-work list (superseded by current update above)

1. Reroute the identified pipe contacts while preserving accepted wheels, cylinders and gear. Compare both neutral and articulated positions; include compressor plumbing and steps as obstacles.
2. Independently inspect Franklin short-rod/crosshead/valve support fit through full strokes and cutoff/reverse; use the current native asset paths before adding a new mechanism.
3. Only then install a consolidated test build and seek live near/far, full-stroke and curve acceptance. Keep this candidate off Nexus meanwhile.

Latest logs load1.22.2 and contain no exception stack referencing RearrangedS282. The most recent structural report is1.22.0 with all47 passes; it explicitly does not prove moving geometry clearance. Installed DLL remains SHA256224CB62AD0583B406EC2DA452D1FD3BE5C6A6E8F10F92CC027A9E3B9F3751D90.
