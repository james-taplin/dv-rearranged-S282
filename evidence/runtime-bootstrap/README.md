# Private native-player regression harness

This harness runs probes in the already built Unity 2019.4.40f1 Mono fixture
player, outside Derail Valley. It does not install a mod or edit a save.
`RuntimeBootstrap.cs` retains the fixture scene's existing script/assembly
identity, loads the installed game's assemblies late, then invokes the named
probe's static `Run(string outputPath)`. Coroutine probes use normal player
frames. Exceptions, process exit status and JSON completion are checked.

Build the bootstrap with `dotnet build RuntimeBootstrap.csproj -c Release -o
../../build/runtime-bootstrap/bin`. Use `Validate.NativeSlipWorld.ps1
-PlayerMode` to create a fixture player if no preserved one exists. The
runner's default template is a private prepared copy under
`build/native-drive-calibration/player`; `-PlayerTemplate` can select another.

`prepare_native_drive.py` accepts `--game-data`, `--player-data` (a private
copy, never the game data directory) and `--unitypy-site`. It adds the actual
S282 right drivetrain to the copied player's Resources catalogue. The payload
retains the original hierarchy, Animator, controller, clips and mesh buffers.
Native service behaviours and material references are removed; meshes are
made CPU-readable for measurement. Probes create controlled native readers and
ports explicitly. Source hashes and payload counts are printed as a receipt.
Neither exported native geometry nor these generated Unity payloads belong in
Git or distributable mod archives.

`Run.NativeRuntimeProbe.ps1` copies the template and probe DLL directory into a
fresh output directory, replaces only the private player's bootstrap DLL and
runs hidden. Supply `-ProbeDirectory`, `-AssemblyName`, `-TypeName` and
`-OutputDirectory`. Its expected-failure option requires both nonzero exit and
a matching exception fragment, so a baseline cannot pass on an unrelated error.

Tender probes use the exact production clone helper, mirror and clock with
actual native Animator evaluation. Slip probes use native engine/boiler/ECS
implementations and verbatim production drive methods. Franklin probes load
the original bundle and use measured triangles, exact source-extracted layout
configuration and the compiled production retarget/phase adapter. Each result
states its own synthetic boundaries. These probes do not establish live
TrainCar coupling, rail forces, saves, camera LOD or multiplayer acceptance.
