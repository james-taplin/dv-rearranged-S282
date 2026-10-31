# Native steam, wheelslip and force integration probe

This fixture runs in an isolated Unity 2019.4.40f1 Windows player against the
installed DV assemblies. It exercises native `ReciprocatingSteamEngine`,
`Boiler`, `MultiplePortSum`, adhesion and wheelslip jobs together with production
`NativeSlipRuntimeState`, `NativeSlipEntitySchema` and `GroupForceRouting`.
`Build.NativeSlipIntegrationProbe.ps1` extracts `PrepareCore`, `PublishRpm` and
`ApplyForce` verbatim from `IndependentDrive.cs`; the report records that source's
SHA-256. No physics gameplay change was required by this investigation.

The feedback scenario uses two real native engines with recorded S282 numeric
engine calibration and the production auxiliary capacity fraction. Exact
production RPM publication feeds their native crank inputs; their resulting
torques feed exact production input preparation, native group slip, and exact
production force application. Both slipping, mixed slip, divergent RPM, brake
allocation, sanding recovery, slide suppression, reverse, grip/force conservation,
single application per synthetic bogie, scoped RPM-reference restoration and
native helper teardown are asserted. A finite native boiler supplies both
engines throughout this loop. Separate identical-vessel cases compare 60/60 RPM
with 300/60 RPM for 50 seconds and account for each engine's intake once, including
the graph's previous-tick consumption ordering.

The car, contact bogies and replication boundary are synthetic. Track speed is
controlled; force application is recorded rather than sent to real rigidbodies.
Controller calibration, load and finite vessel are controlled inputs, not a
capture of the full S282 controller, boiler/firebox or save. Burst is disabled.
This does not establish actual car acceleration, complete game-loop scheduling,
animated wheel/linkage clearance, multiplayer transport or reporter acceptance.
Whole `IndependentDrive` ownership/restoration has the separate
`Validate.IndependentMultiplayer.ps1` fixture; this probe's restoration assertion
checks its scoped native input lease only.

From the repository root:

```powershell
.\Build.NativeSlipIntegrationProbe.ps1
.\Run.NativeRuntimeProbe.ps1 `
  -ProbeDirectory .\build\slip-integration\probe `
  -AssemblyName NativeSlipIntegrationProbe.dll `
  -TypeName NativeSlipIntegrationProbe `
  -OutputDirectory .\build\slip-integration\native-new
```

The runner requires the existing standalone player template and generic
bootstrap; it makes an isolated copy in a new output directory. Never invoke
the probe inside the game or alongside another native-World fixture in one
process. Its public entry point is `NativeSlipIntegrationProbe.Run(string output)`.

On 31 October 2026, `build/slip-integration/native-02/result.json` passed all 30
assertions and the player exited 0. The production source hash was
`B4787BC364029A8A401135EC0A81D851D0159CB35E381BA65AE0B58504D3D5C4`.
Group RPM differed by up to 268.084 RPM. The feedback boiler's mass fell from
6045.657 to 5773.754 kg and pressure from 16 to 10.464 bar. The rolling comparison
consumed 211.327 kg integrated intake; the higher-RPM comparison consumed
360.924 kg and ended at a lower pressure (8.927 versus 11.584 bar).

The first finite-vessel test failed because it compared 2500 native float mass
subtractions against a double-precision sum using an unjustified fixed 0.05 kg
bound. Native `WaterPressureVessel.RemoveSteam` performs `mass -= intake * dt`
with floats. The corrected test asserts the exact per-tick float mass ledger,
without widening a tolerance, and separately records the double intake integral
and float quantization difference (0.162 kg rolling, 0.291 kg higher RPM). This
was a test accounting error, not evidence of extra production steam consumption.

The existing ownership suite also passed 32 port-adapter and 27 complete
production-owner checks under its documented API substitutes at
`build/slip-integration/ownership-regression/20261031-204257-193`.
