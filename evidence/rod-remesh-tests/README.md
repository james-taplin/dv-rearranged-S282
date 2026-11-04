# Coupling rod remesher fixture

Run `Validate.RodRemesher.ps1` from the repository. It compiles the actual pure
production helper with a minimal `Vector3` stand-in. No game is launched or edited.

The extractor loads local installed native OBJ files in exactly the first-use
vertex order used by `OBJObjectBuilder`, then replays the existing two-, half
three-, and five/six-axle recipes. A scoped, token-normalized SHA guard covers the
rod recipe, range definitions and OBJ reader/builder dependencies: unrelated
`MeshFinder.cs` work or comments do not invalidate it. Any guarded change requires
comparing the actual code and this replay before updating the expected fingerprint.
Run `python evidence/rod-remesh-tests/extract_native.py --check-guard` to check
that recipe, range and importer mutations are detected. Native geometry is generated **only under ignored
`build/`**, and is never redistributable as test or release content.

Protected axial boss half-width is 0.14 m: measured native shoulders extend up to
0.123787 m from the nominal pin centres. Taking only half the native overall height
would protect 0.114036 m and would miss some shoulder geometry. Continuous linear
mapping joins the *edges* of protected slabs, with positive shaft lengths. A radial
inside/outside switch plus interpolation between the pin centres would introduce
discontinuities at the boss boundary.

The fixture checks input immutability, identity, both pin directions, exact pin
mapping, continuity, linear shaft spans, finite/invalid input rejection, preserved
boss shape, source face winding and nondegeneracy across 180 real topology cases.
It records existing native pin/bore mismatch separately: rounded nominal pin
coordinates are not perfectly identical to native bore centres or wheel positions.

This helper accepts **positions only**. Renderer mesh cloning must preserve UVs,
submesh/index buffers and materials separately. Unchanged UV coordinates imply
texture stretch on a lengthened shaft, not unchanged texture density. Native
animation timing, live wheel offsets, linked fittings, LOD renderer policy, cleanup
and rendered clearance require integration checks outside this fixture.
