# Franklin pin seating and triplex clearance — 2.0.9.4

James authorised the smallest native repair, agents and offline validation on
4 November 2026. DV testing and installation remain James's separate steps.
The 2-2-2 is excluded. Owned/demonstrator eligibility is unchanged.

Status: integrated core compiled as 2.0.9.4, zero warnings/errors. The bounded
offline import, seating, lifecycle and clearance pass completed successfully.
Gameplay acceptance remains James's testing. Nothing was installed or packaged.

## Repair

The preceding diagnosis is in `TRIPLEX_CONSTRAINT_AND_PIN_AUDIT_2026_11_04.md`.
The original renderer-enable fix did not bridge the measured 32.83 mm axial gap.
The authored Franklin rod orbit also differs from the wheel orbit by up to
13.5 mm, so extending a wheel-fixed pin alone cannot seat the moving bearing.

For Franklin IDs31/32/38/40/47, `FranklinPinPresentation` separates the original
pin triangles from owned wheel-mesh copies. The main driver uses the actual
stock driver-3 long pin; the other coupled driver retains its native short pin.
Native PositionConstraint follows the existing animated coupling bone in Y/Z;
native RotationConstraint retains wheel rotation. Pin X stays at its native
absolute position, independent of the previous controller's phase. Wheel rims,
Animators, wheel transforms, quartering and simulation ports are unchanged.
This adds no dependency, controller or per-frame script.

The pin foot remains on the existing raised crank boss: independently measured
boss radius 74.188 mm, pin radius 57.422 mm, original orbit deviation at most
13.5 mm and axial embed approximately 0.470 mm. The offline pass checks the
actual candidate motion and bearing engagement rather than treating these
static dimensions alone as acceptance.

All three native wheel meshes are CPU-unreadable. The repair extends the
existing exact local exporter/normalized OBJ reader to those three mesh names.
It uses the existing file-cache pattern; reapplying an arrangement does not
launch eighteen exporter processes. Imports, wheel copies and pin geometry
are owned; native meshes remain untouched. A disposable lease restores original
mesh references, LOD arrays and bounds, then disables and destroys its objects.
Leases unwind before retarget/prepared geometry restoration and on destruction.
Pin objects do not enter the blanket articulated-assets activation list.

For the clipping, only the **middle** Franklin cylinder castings on IDs38/47
receive the existing style of nose correction: 0.20 m native shortening over
Z9.49–9.85. The forward cap translates rigidly; the rear piston face, bore axes
and shaft mounts remain fixed. The map is monotone (minimum derivative 1/6).
Particle outlets follow the same existing scaled middle-engine map; the steam
inlet seats from the corrected mesh through its existing method. Engine spacing,
native articulation and the earlier ID32 rear-nose repair are unchanged.

## Evidence

- Existing casing baseline: 690,269 assertions / 96 actual-mesh cases passed.
- Candidate shape: 697,522 assertions / 97 cases passed, including the new
  actual-mesh profile, cap rigidity, mount preservation and orientation checks.
- Candidate outlet mapping: 10,740 checks / 1,512 cases passed.
- One integrated core build: zero warnings/errors, version 2.0.9.4.
- Saved original failures remain under
  `build/triplex-constraints-audit-20261104/`; no baseline result was overwritten.
- Candidate shape/outlet reports:
  `build/triplex-repair-20261104/{casing-shape,outlets}/validation.json`.
- Integrated candidate and native reports: `build/franklin-repair-2094/`.

The actual compiled OBJ loader and normalization matched native oriented
triangles/material sections and used vertices within 0.359 micrometres. The
fixture deliberately uses unreadable test wheel copies so the production
fallback executes; the geometry shadow is used only for measurement. Native
file inspection independently confirms all six relevant renderers have one
material slot and one mesh section. The first smoke failed because the
reconstructed headless renderers had empty material arrays. Supplying test-only
null slots corrected that fixture omission; production's material guard and the
integrated core were unchanged. Both failed and corrected smoke evidence remain.

| Native run | Layout poses | Checks | Unexpected contacts |
| --- | ---: | ---: | ---: |
| IDs38/40/47, 720 phases, two patterns, support arcs -30/0/+30 | 12,960 | 6,200 | 0 |
| Native10 and Franklin31/32, 720 phases, two patterns, yaw -15/0/+15 | 12,960 | 2,734 | 0 |

Both sides and both engine boundaries are covered, including the newly exposed
main/coupling rods, all Franklin auxiliary parts and new pins against following
casings. Native10's intended spindle entry into its own chest remains separately
classified; it is not hidden or treated as an unexpected contact.

Every sampled Franklin main bearing has full axial long-pin engagement, zero
axial separation and a pin inside its bore envelope. Coupling bearings retain
at least 71.2869% shaft engagement, matching the native short-pin design (the
remaining section contains its cap). The worst measured coupling-eye/pin centre
error is 0.423 mm; main-eye errors are below 0.003 mm. The main rod's original
13.5 mm orbit mismatch now occurs within the existing raised wheel boss rather
than between the bearing and pin. The 74.188/57.422 mm boss/pin containment proof
is geometric plus sampled-orbit evidence, not a physical joint simulation.

The same source lease was bound from a perturbed previous-controller X pose,
disposed and rebound. Source mesh identities/vertices, LOD memberships,
thresholds and bounds restore exactly; disposed pins are inactive. Where the
mechanism rig has no body LOD owner, an explicitly constructed two-level group
around the real wheel renderers exercises per-level pin membership. This proves
the API's reversible membership policy, not in-game camera selection.

Disabling only the new nose correction in a 120-phase negative control restores
**228 pair/pose contacts**, including crank/main-rod/cylinder and new-pin/cylinder
contacts. It reports `clearancePassed=false` and
`expectedNegativeControlMatched=true`. Thus the repaired sweep's zero is not
caused by disabling geometry or losing the offending pair inventory.

Core SHA256:
`C4AF2E44428DCD74F29E20C80300F8CDDBB473A9A33A822C7C1F3E23482CB07E`.
Only one production compilation was needed. Fixture recompilation corrected
material/membership evidence; it did not rebuild unchanged production.

## Gameplay limits and James's checks

Offline mechanism fixtures use the original controllers, exact geometry and
native constraints with synthetic support motion. They do not establish a live
curve radius, camera/LOD rendering, complete TrainCar lifecycle or reporter
acceptance. The irregular main-rod far eye is not independently fitted; this
repair targets the identified wheel-end seating and following-cylinder contact.

After a separately authorised installation, check both sides of Franklin
IDs31/32/38/40/47 through a slow revolution. Inspect pin engagement from an
oblique view, then the front crank/main rod against the middle cylinder on both
curve directions. Check the shortened cap, steam inlet and particle outlets.
Switch Franklin → Walschaerts → stock → Franklin and inspect near/far camera
views for restored wheel meshes, single pins and normal LOD behaviour. Confirm
stock and the excluded 2-2-2 remain unchanged. Save/reload and host/guest visual
acceptance remain in-game checks; no save-format or network change was made.
