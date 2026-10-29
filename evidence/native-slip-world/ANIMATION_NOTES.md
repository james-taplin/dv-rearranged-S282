# Independent crank-phase adapter validation

28 October 2026. Production `GroupCrankAnimation` is linked directly into the
native release-player probe. `GroupAnimationFixtureEditor` creates a simple
one-revolution controller and rotating wheel. `GroupAnimationProbe` uses actual
native AnimatorPortReader/Port with detached native engine phase readouts and
normal Unity Animator evaluation, without manually evaluating Animators.

Latest run: `build/native-slip-world/runs/20261028-232017-267`.
The animation report passes 11 checks and the runner exits cleanly. It covers
separate group phase, rendered wheel pose, left/right quartering offsets,
phase wrapping/reverse updates, suppression of competing native speed-reader
subscriptions, restoration to the current original speed-port value, phase
unsubscription, independent release and same-frame release/rebind cleanup.

The adapter freezes automatic Animator advancement, uses one native normalized
time reader per target and retains existing clips, geometry and cutoff readers.
Shared native speed-controller registrations use a lease so releasing one group
does not restore another group's global speed writer. Mirrors and the tender
speed clock skip targets while owned.

Integration API:

`GroupCrankAnimation.Bind(engine, animators, normalizedPhases = null)` returns
an IDisposable. Pass explicit normalized phases when binding immediately after
queued quartering initialization. Otherwise the target must have an initialized,
evaluated state. The optional fourth argument injects global native speed-driver
instances for fixtures; production should use default car discovery.

This fixture does not exercise actual S282/Walschaerts/Franklin mesh or clip phase
origins, native global driver registration leasing, native cylinder Tick timing,
actual vehicle forces, LOD/streaming or game lifecycle. Those still require the
integrated runtime checks. Its detached RSE instance supplies only the native
phase-readout port; it is not a cylinder simulation test.
