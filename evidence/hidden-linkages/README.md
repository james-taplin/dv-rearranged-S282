# Hidden native S282 linkages

Run `./evidence/hidden-linkages/validate.ps1` from the current multiplayer checkout.
This is a bounded offline managed geometry fixture. It never launches DV or edits
installed files. Private native data and generated outputs stay in ignored
`build/hidden-linkages/`; do not include them in a runtime package.

Inputs reuse the established exact native readers:

- `evidence/franklin-runtime/extract_geometry.py` supplies the native full drive
  hierarchy and every source mesh in `build/franklin-runtime/geometry.json`.
- `evidence/pacific-hudson-valve-gear/native-animation.json` supplies three exact
  native cutoff clips, with 16 sampled wheel phases each. The fixture replays
  local position/quaternion/scale bindings, including the reach rod's native
  animated Z scale. It does not pretend to evaluate Unity Animator blending.
- `evidence/fourteen-spacing-tests/native-spacing.json` supplies independently
  measured cylinder front and rear-bogie frame. Resource/cache hashes must match.
- `layout_provenance.py` derives each affected conventional moving frame and
  oiler offset from the current production source, recording its assumptions and
  hashes. This is a source numerical projection, not complete setter execution.
- `articulated_layouts.py` verifies and reuses the previous native fixture's six
  measured Walschaerts front-engine maps against unchanged source-method,
  native-geometry, installed-LMS and output provenance. Their own hinge locations
  are sampled at -15/0/+15 degrees. This does not rerun the earlier clearance sweep.

`measure_native.py` establishes the pre-repair native shaft endpoints, distinct
LOD child poses, native lubricator rod-to-lever fit and 4-14-4 mismatch. The reach
rod ends at a flat shaft that enters the cab; there is no modeled rear bearing.
Its LOD1 is natively shorter and rotated, so it has a separate measured datum.
The lubricator LOD1 also has its own translated datum. Native rod/lever authored
contact residual is retained, rather than claiming a perfectly closed native joint.

`Program.cs` directly compiles the actual `DriveRetarget.cs`, `RodRemesher.cs`,
`WheelRearranger.HiddenLinkages.cs` and `WheelArrangement.cs`. Unity object,
transform, mesh and LOD behavior use the established managed geometry stand-ins.
Native source mesh reads throw when marked unreadable; the exact-export callback
loads the six actual installed-exporter OBJ payloads from
`build/hidden-linkages/export-smoke`. Their raw reflected-X coordinates must fail
the native-coordinate negative control. The fixture compiles the exact production
`MeshFinder.NormalizeLinkageImport` method and checks every normalized vertex and
the complete triangle index/winding arrays against the exact native meshes. The
fixture's simple OBJ parser preserves original OBJ vertex indexing; the separate
parent-run Unity importer smoke covers the real runtime importer. This validation
script does not launch AssetStudio or Unity.

To create that private export input with the installed bundled exporter, use:

```powershell
& 'B:\SteamLibrary\steamapps\common\Derail Valley\Mods\RearrangedS282\AssetStudioModCLI_net472_win64_contained\AssetStudioModCLI.exe' `
  'B:\SteamLibrary\steamapps\common\Derail Valley\DerailValley_Data\resources.assets' `
  -o "$PWD/build/hidden-linkages/export-smoke" -t mesh `
  --filter-by-name 's282_mech_cutoff_rod,s282_mech_cutoff_rod_LOD1,s282_mech_lubricator_rod,s282_mech_lubricator_rod_LOD1,s282_mech_lubricator_part_1,s282_mech_lubricator_part_1_LOD1'
```

`validate_importer.ps1` compiles only `ImporterProbe.cs`, references the already
built candidate DLL, and uses `Run.NativeRuntimeProbe.ps1` with the preserved
isolated Unity player. It does not rebuild production. Use a fresh output
directory on subsequent runs. The probe runs the actual candidate OBJ parser and
normalization, comparing every imported point and oriented triangle to native
data. Null material slots avoid shader creation; proxy material assignment is
outside this headless importer check. The initial result is recorded in
`build/hidden-linkages/native-importer/result.json`.

The checks cover native-frame vertex identity; both reach ends and right
lubricator ends recovered from actual rendered mesh caps; the native lever
vertex trajectories; immutable animated source transforms and source assets;
independent native LOD data and sampled bounds; the actual integration partial's
cache/apply/suspend path; effective valve-gear visibility; moved oiler anchors;
mesh and LOD-reference restoration; stock/no-op and excluded IDs. The 2-2-2 and
Franklin arrangements are explicitly excluded. Articulated rear-engine rods are
not enabled or certified by this fixture.

A narrow coexistence case first binds the existing main/radius-rod retarget,
then adds the hidden endpoints and checks that the same owner and existing proxy
identities survive all 48 native samples and unwind to native mesh/LOD identities.
One missing readable export exercises the actual integration suspension cleanup.

The original fixed mid-shaft guide lies near body Z5.78..6.05, not at the cab tip
near Z.43...50. Its installed LMS selected-vertex envelope is recorded in the
measurement report. It misses the resized span and remains hidden; no new guide
or pedestal is fabricated.

Limits: this is not gameplay, rendered appearance, the native ratchet/simulation,
live Animator blending, or a complete TrainCar arrangement switch execution.
The fixture verifies the scoped partial and native-reference restoration before
the main controller's existing prepared/native geometry snapshot restoration.
James performs visual, cutoff, moving, LOD-distance and arrangement-switch checks
in DV after separately authorized installation.
