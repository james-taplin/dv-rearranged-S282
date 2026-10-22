Steam intake presentation checks for candidate 1.18.0

The two shipped OBJ meshes are unchanged geometry exported from SatiricRug's
original MIT drivetrain bundle. The original licence and credits remain in the
package. Native game casing and pipe geometry are local test evidence only and
are never included in the ZIP.

Run extract_original.py using UnityPy, then Validate.SteamPipes.ps1. The existing
cylinder-casing native extraction fixtures are also required locally.

Program.cs tests actual native cylinder sockets, rigid collar bands and source
pipe sweeps, including collar rigidity and nondegenerate faces. ActorProgram.cs
compiles the actual presentation MonoBehaviour against ActorStubs.cs (managed
scene/mesh/LOD stand-ins derived from driver-layout-tests/Stubs.cs). It tests
articulation/tender pose changes, error hiding, per-instance mesh ownership and
exact native LOD restoration. It does not simulate Unity rendering or physics.

Still requires live structural diagnostics, arrangement switching, tender
attach/detach, tight curves and full visual body/gear clearance checks. These
are intake supply visuals, not a new compound/exhaust steam circuit.
