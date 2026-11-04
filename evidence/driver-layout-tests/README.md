# Driver pitch integration fixture

`Validate.DriverLayouts.ps1` compiles the **actual production** `DriverLayout`,
`WheelRearranger.DriverLayouts`, `RodRemesher`, `PoweredWheelRotater` and the
`WheelRearranger.Additional` physical-axle consumer against managed Unity
transform, mesh and LOD stand-ins. It does not launch or modify the game.

Inputs are local native wheel topology, native high/medium/low animation clips,
and an exact replay of the existing imported-OBJ short-rod recipes. Native data
is ignored under `build/` or existing ignored native reference fixtures, never
shipped. Mesh UV and colour channels use nontrivial fixture values to verify
that assigning remeshed positions preserves other cloned channels.

The native hierarchy has detail and coarse calipers, but only detail suspension
meshes. The fixture deliberately omits nonexistent coarse springs. Native wheel
position/scale animation bindings are evaluated after applying the layout, then
the production completed-pose rotation copier runs. Presentation centres/scales
must survive those source resets. Source wheels deliberately carry collider and
rigidbody components; presentations must not copy them.

Coverage includes IDs 34/35/44/0 at their current fixed respective pitch of
0.18/0.20/0.22/0 m, translated/rotated cars, separate/shared native LOD owners, stationary main
axles, dense spark/shoe assignment, detail/coarse fittings and flanged wheel meshes,
physical axle resolver mapping, native mesh/channel immutability, corrected bounds,
exact LOD array/order/fade/culling-bounds restoration, cache reuse/disposal,
switching among the four layouts, second-car isolation, scoped no-op behaviour and
rollback after native-data/LOD ownership failures. Ninety-six reapplications at
different native phases verify immutable mesh reuse within the existing 1 µm
cache comparison, across all four fixed pitches.

The fixture also evaluates **different left/right native phases before the first
cache operation**, spanning the actual native main-pin local-Z variation. Native
main-rod position is slightly animated; it must not be treated as a constant live
sample. An independent unanimated livery prefab supplies the canonical datum.
The earlier live-sample implementation reproducibly rejected the layout with
`Left/right coupling-rod frames do not share a compatible pitch mesh`. The new
regression runs 152 initial/different-phase cache applications and verifies prefab
immutability. `-TargetedCacheOnly` runs just this regression; `-OwnerSourcePath`
can compile an ignored pre-fix source copy to demonstrate the original failure.

Pin seating uses independently fitted **physical bearing centres**, not the
production descriptor values compared against themselves. Native main-rod phase
plus each presented wheel-centre difference determines the independent target.
Intentional outboard X planes are excluded from the YZ measurement. Longitudinal
seating remains below 1 mm throughout sampled native poses. Existing native
rocking and legacy 0.71 m versus native 0.712129 m wheel-centre rounding leave
several millimetres of vertical offset; the fixture reports that baseline rather
than claiming perfect 3D alignment. Added transverse drift is bounded by the
native rotation sine times the corrected longitudinal span, plus floating-point
transform precision. This is **not** a new motion or physics system.

The caller's `GeometryState`, pooling/save restoration, live native wheel-manager
registration, oil interaction, Unity Animator scheduling, mesh upload, renderer
culling and actual collision/visual clearance still need their dedicated checks
or in-game testing. Managed LOD bounds are a geometric stand-in, not Unity's
renderer implementation.

Status, 4 November 2026: fixed-pitch expectations replace the obsolete adjustable
settings. The complete fixture passed 26,181 assertions and 16 layout cases.
An ignored source-copy negative control changing the Mogul pitch to 0.19 m failed
the independent 0.18 m assertion. See CURRENT_HANDOVER.md for current results.
