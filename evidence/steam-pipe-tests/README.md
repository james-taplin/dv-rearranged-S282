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

22 Oct geometry audit: Validate.PipeClearance.ps1 exports the actual production
pipe presentation using the body-space sockets recorded in the structural JSON.
Use -RecordedBuild to retain those sockets, or omit it to recalculate the newly
aligned Ten-wheeler/Mogul/twelve-driver sockets. check_clearance.py checks actual
triangle surface crossings with retained native body, separately moved reservoir
meshes and native air pipes, plus sampled native cylinder-side motion. Contacts
are findings for review, not a failing assertion or a whole-locomotive all-clear.
Intentional native boiler seating within .15m of each attachment is excluded.
The exporter does not execute the entire WheelRearranger or native articulation;
Franklin motion, stretched rods and live articulation still need separate checks.
All exports stay in build/ and must never be shipped.
