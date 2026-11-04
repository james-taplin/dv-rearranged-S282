# Main connecting-rod spacing fixture

Run `Validate.ConnectingRods.ps1`. It compiles the actual production connecting
spacing owner, coupled-driver layouts, `DriveRetarget`, `RodRemesher`, oil mounting
and physical-axle consumer. The existing driver-layout fixture is reused through
generated partial-class copies under ignored `build/`; the driver fixture now also covers American/Prairie topology. The actual production
Prairie spacing/clearance source and `CylinderDriveFrame` are compiled and tested.
The optional pre-spacing Prairie fitting list is a stand-in in this harness;
full static part/casing/polish mapping retains its separate motion fixture.

The extractor reads all fourteen affected native motion meshes, complete triangle
topology, actual CPU-readability flags and real L/R native LOD metadata. Native
geometry stays local under ignored `build/` and must never be packaged. High,
medium and low native clips supply independent L/R phases with quartering.

Both native LOD0 spanning rods are unreadable; LOD1 is readable. The mesh stand-in
throws on a CPU vertex read of an unreadable mesh. Existing main-rod OBJ fallback
and missing-radius runtime-provider paths are exercised explicitly. The test
provider returns owned readable copies of the actual extracted geometry, standing
in for AssetStudio/OBJ, and failure is injected on both the first and second side.
The real exporter executable/provider still needs its separate validation.

Coverage includes 2-6-0, 2-4-0, 4-4-0 and 2-6-2, 0/0.15/0.25 m translation, separate/shared
LOD groups, translated/rotated cars, hidden coarse-parent calipers and selected
hidden sparks. Tests check whole-group axle/fitting/shoe movement, retained coupled
pitch, static guides/cylinders, actual cylinder-frame cancellation, true main-pin
distance 3.15 m, true radius pin distance 1.56 m, unchanged native animation source
transforms, completed-pose wheel copying, rigid bearing bosses with unit proxy scale,
live mesh bounds/topology/UV preservation, source immutability, native oil-cup sync
under both callback orders, disposal and nested LOD restoration/rollback.

Joint checks use independently fitted native attachments: the main rod meets the
crosshead near local Y 0.013806 m / Z -0.923880 m, while the radius rod meets the
combination-lever root. Intentional outboard X offsets are excluded. Small authored
animation errors remain and are measured explicitly; bounding-box length heuristics
are not used to validate the physical joints.

LOD checks preserve native policy rather than inventing perfect swept bounds.
Actual native LOD envelopes themselves exclude some piston-mesh tips inside the
casing over the full stroke. The fixture records that absolute escape, then checks
that new geometry does not add escape relative to the original native renderer in
the same pose. It also verifies exact arrays, reference points and size restoration.
The shared-owner fixture caught and now guards the required reverse unbind order.

The `RemeshInto` allocation probe runs six warmed batches of 1,000 actual helper
calls, independently of mesh stand-in allocations. It checks source immutability,
owned-array equivalence and rejection of aliased source/output buffers.

These are managed transform/mesh/LOD tests with native data. They do not reproduce
Unity renderer culling, mesh upload cost, Animator scheduling, physical wheel-manager
registration, live oil interaction, suspension motion, frame clearance or save/pool
acceptance. Those remain separate checks or in-game testing.

Status, 4 November 2026: the shared native rod extractor and scoped recipe
check now run on the current source. This fixture compiles but stops at the
obsolete adjustable-pitch assertion (0.18 versus 0.01); later checks have not
run. Production now uses fixed per-family pitch/rearward values. Update the
fixture expectations and rerun before relying on these rod-motion, isolation,
LOD and restoration checks. See CURRENT_HANDOVER.md for observed results.
