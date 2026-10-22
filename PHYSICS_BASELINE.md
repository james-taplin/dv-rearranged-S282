> Current 1.21.2: stock-relative restriction removed; see [FOA_BALANCE.md](FOA_BALANCE.md). Earlier minimum limits below are historical.

# Calculated arrangement baseline - 1.21.0 candidate

> **1.21.1 refinement:** The universal 4.4 ceiling below describes the previous
> 1.21.0 candidate. The current source uses a modest machinery-sensitive ceiling;
> see [FOA_REFINEMENT.md](FOA_REFINEMENT.md) for the exact rule and complete
> before/after table. All other baseline assumptions and systems remain unchanged.

This is an engineering-informed gameplay baseline for rearrangements of one
S282 body and boiler. It is not a claim that each Whyte notation recreates a
specific historical locomotive. No further driving captures were required to
calculate or implement this pass. The previously installed 1.20 remains the live
tested baseline until a later installation receipt says otherwise.

## What is deterministic

Given the same state, the native cylinder, wheel-radius and adhesion calculations
produce repeatable forces. Native game assemblies and the measured 1.20 geometry
provide the actual radii, cylinder dimensions, mass and relevant formulas.
Cylinder torque does not alone specify a top speed: chest filling, steam flow,
cutoff, boiler output, resistance, grade and control history also matter.

## Retained effective adhesive loads

The native game divides weight equally among physical axles. Existing friction
conversion represents the intended driver-weight share while retaining native
wheel slip, weather and sanding. Slide friction, actual vehicle mass, centre of
gravity, carrying trucks, wheel sizes, rods and articulation are unchanged.

The current family shares and short-group rated caps remain: 18/19/20 tonnes per
driver for one/two/three-axle groups, 32 t ceiling for larger groups. These are
conservative effective load assumptions, not historically measured spring loads.
In particular a 124 t S282 on four coupled wheels cannot simultaneously have
19 t driver loads and plausible light carrying-truck loads. The mod represents
adhesion rather than claiming a physically reconstructed suspension/mass layout.
Changing real vehicle mass to make such layouts historical would also change
braking, inertia and coupler behaviour and is outside this pass.

Shares are now selected at a fixed **124 t rated working mass**, rounded from
the captured approximately 124.08 t stock locomotive (111 t body plus resources).
Thus repeating an arrangement selection after using water does not silently
change cylinder bores or rated share. The native grip still varies with actual
current mass and native rail/sand conditions. Axle caps are rated values, not
continuously enforced caps under arbitrary mass-changing mods.

## Revised cylinder design

The previous reduction-only pass capped native mean pull at the stock S282's
pull/grip ratio, about 1.287. It was a useful first safeguard but forced many
arrangements toward the same aggressive stock behaviour.

The new explicit nominal design uses:

- Boiler reference: 15 bar absolute, hence 14 bar gauge.
- Nominal cylinder pressure: 85% of boiler gauge pressure, 11.9 bar.
- Minimum nominal factor of adhesion: **4.4**, or nominal TE <= adhesive weight / 4.4.
- Existing cylinders are an upper bound: only excessive capacity is reduced.
- Stock S282 unchanged; unpowered 4-0-4 unchanged; giant 2-2-2 prototype excluded.

For N double-acting cylinders, bore d, stroke L and wheel radius r:

`Nominal TE = N * d^2 * L * (0.85 * boiler gauge pressure) / (4 * r)`

This is a pressure convention for nominal comparison, not a new force override.
The target is translated to B99's actual mean-force formula at 15 bar absolute,
0.75 cutoff and 300 C. Its mean effective gauge pressure is approximately 1.133
times the nominal pressure, so the corresponding native reference pull/grip
ceiling is about **1.073**, not 1 / (4.4 * .24) alone. Mixing these conventions
would understate native torque or mislabel historical nominal TE.

Actual native engine bore, piston-area cache and swept-volume cache change
together. Stroke, cylinder count, original auxiliary/main area ratios, native
steam flow/chest limits, throttle and boiler remain. Smaller swept volume affects
steam demand through the native simulation. All original dimensions restore on
stock, failure, pooling and destruction. No speed cap or additional power source.

The 4.4 target is a stated design choice within a broad historical-style range,
not a universal constant. Advanced Steam Traction describes many historical
designs around TE/adhesive weight .20, and 5AT around .25. Its design process
likewise derives cylinder diameter from axle load, wheels and pressure.

Sources:

- [AST: Adhesion and Sanding](https://advanced-steam.org/5at/5at-project/5at-features/adhesion-sanding/)
- [AST: Calculated Performance Predictions](https://advanced-steam.org/5at/5at-project/5at-performance-predictions/calculated-performance-predictions-for-the-5at/)
- [AST: Adhesion definitions](https://advanced-steam.org/5at/technical-terms/steam-loco-definitions/adhesion-2/)
- Installed B99 `ReciprocatingSteamEngine` and `AdhesionControllerSystem`, locally inspected.

## Erie

Its locomotive engines use locomotive adhesive load. The third engine is sized
independently against a **50 t rated tender, .8 driven share**: 40 t adhesive mass,
89.2 kN nominal TE. Previous unmodified tender cylinders calculated at 150.9 kN
nominal TE, substantially above the separate clamp's capacity.

The existing tender force clamp and attachment architecture remain unchanged.
It already reads `TotalMass`, including whatever resources the native mass
controller includes. It is not yet equivalent to the full native rail/sanding
adhesion system; do not claim this pass completes tender adhesion/weather/slip
presentation or implements real compound expansion.

## Results and validation

Output: `build/runtime-trial-1.21.0/baseline/baseline.md`, CSV, JSON and plots.
The production native-engine fixture calculates all targets and validates cached
dimensions, independent nominal-factor arithmetic, resource-independent sizing,
second-car isolation, reduction-only capacity, tender sizing and exact restoration.
132 layout/mass cases and 1,400 native ticks pass; 4,140 assertions.
Production net48 build has zero warnings/errors. Protected comparison confirms
105 existing physics/motion methods unchanged; this does not imply the intended
cylinder calibration or selection path is unchanged.

Nine non-stock arrangements are within 5% of their previous nominal TE. Many
previously ceiling-limited arrangements reduce nominal cylinder TE about 16.6%;
Erie tender about 40.9%. New bores do not necessarily reduce usable starting pull
by the same proportion: old values could exceed grip and require regulator control.

The plots compare nominal cylinder TE with rated native dry grip. They do not
claim measured drawbar performance, sustained power, top speed, wet-rail grip or
tonnage ratings. The Erie tender is a separate row. The clearance prototype is
excluded from plots but retained explicitly in the table. A later TE metric should
state whether it uses nominal force, native mean force or adhesion-limited force;
one unlabeled combined number would conceal meaningful differences.
