# Source-derived steam duct checks

Run `Validate.SteamDuctMeshes.ps1`. It compiles the production `SteamDuctMesh.cs`
and `SteamPipeNativeFit.cs` against small managed Unity vector stand-ins. It reads
the locally measured original transfer mesh, native LOD0 steam pipes and installed
LMS body OBJ UVs. Game geometry stays local evidence and is not a release asset.

The kit extends the original transfer shaft profile and complete authored end
collars. New lengths and circular fillet elbows retain the original cross-section
and shaft UV island. Reducers, collars and paired-collar unions use the same set.

The Prairie offset adapter retains all 35 corners of the actual native cut and
all 13 unique corners of the original transfer outlet. Parallel horizontal rings
and zero endpoint derivatives keep both attachment planes seated. A horizontal
mid-offset bows the casting around nearby native plumbing without moving either
end. Tests cover both sides, reversed ordering and zero/outward clearance bows.

Material correction: the native neck and original transfer use different atlas
islands. Interpolating their UV coordinates would sample unrelated body textures.
The retained native prefix therefore keeps its own unchanged UVs, while the entire
NEW adapter skin uses the original transfer shaft island, with a UV seam at the
physical join. Geometry identity is tested independently of this deliberate UV
mapping choice; transfer seam coordinates are retained on both sides.

The tests verify generated geometry, source identity, winding, dimensions, sampled
curvature and atlas bounds. CPU previews show geometry only. These do not replace
the whole-body clearance audit or in-game material and articulation checks.

## Native fitting and lower collar replay

Run `Validate.NativeSteamFittings.ps1` from the repository root. Optional parameters
are `-GamePath`, `-OutputDirectory` and `-Python`. It compiles the production native
fit, lower collar and casing shape helpers with the existing managed vector stubs.
`NativeFitProgram.cs` replays the original 3,140 geometry checks, then
`check_native_collar_air.py` checks those compiled collar meshes against the actual
relocated compressor input, output and reservoir-connection triangles. These are
the existing focused checks, preserved from the local build harness.

Required local inputs are `evidence/cylinder-casing-tests/native-casings.json`,
`native-full-casing-fittings.json`, `LMS_LOD0.reference.txt`, and both installed
LocoMeshSplitter S282 body OBJ files. They are read as local evidence, never added
as distributable fixture assets. If missing, the existing
`evidence/cylinder-casing-tests/extract_native.py --game <game folder>` and
`extract_full_fittings.py` regenerate the measurements using the installed game
and splitter; the latter currently uses the documented default game path.
Python needs NumPy; the existing extraction workflow additionally needs UnityPy
and the local ilspy tool described in those extraction scripts.

All generated JSON, meshes and the small .NET project stay under the selected
build output directory. The fit checks measure native section/casing seating;
the collar checks verify source preservation and triangle winding/noncollapse.
The air check detects noncoplanar surface crossings at the neutral pose. None
claims a minimum clearance, complete solid containment or live rendering proof.
