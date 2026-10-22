# 1.21.2 balancing correction

Compared with installed 1.21.1. Stock nominal FoA 3.668 and nominal TE 221.1 kN; stock unchanged.

**Corrected premise:** stock is a reference, not a minimum FoA or a pull/grip ceiling. The old stock-ratio restriction is removed. Reduction-only sizing still limits every engine to its original cylinders.

The existing smooth rule is recentered: base3.9; powered-axle term .2*(axles-4), bounded[-.4,.6]; existing modest diameter, effective axle-load and cylinder-count terms retained; total bounds3.3-4.65. This is an explicit machinery-informed gameplay allowance, not a claimed historical law. No assigned performance tiers or changes to adhesive shares.

**Result:** 8 calibrated locomotive arrangements below stock FoA and 36 above. No quota is imposed. Naturally weak original machinery remains weak. The 2-2-2 is not included in these counts.

| Arrangement | FoA before → after | Nominal TE kN before → after | TE change | FoA vs stock | Notes |
|---|---:|---:|---:|---:|---|
| 4-4-0 American | 3.93 → 3.43 | 94.9 → 108.8 | +14.6% | -0.24 | >10% change |
| 4-4-2 Atlantic | 3.93 → 3.43 | 94.9 → 108.8 | +14.6% | -0.24 | >10% change |
| 4-4-4 Reading | 3.93 → 3.43 | 94.9 → 108.8 | +14.6% | -0.24 | >10% change |
| 4-6-0 Ten-wheeler | 4.06 → 3.66 | 145.1 → 160.9 | +10.9% | -0.01 | >10% change |
| 4-6-2 Pacific | 4.06 → 3.66 | 145.1 → 160.9 | +10.9% | -0.01 | >10% change |
| 4-6-4 Hudson | 4.06 → 3.66 | 145.1 → 160.9 | +10.9% | -0.01 | >10% change |
| 0-8-0 Eight–wheel switcher | 5.50 → 5.50 | 221.1 → 221.1 | +0.0% | +1.83 | original cylinder capacity retained |
| 0-8-2 | 4.21 → 4.02 | 210.8 → 221.1 | +4.9% | +0.35 | original cylinder capacity retained |
| 0-8-4 | 4.20 → 3.90 | 182.6 → 196.6 | +7.7% | +0.23 |  |
| 2-8-0 Consolidation | 4.84 → 4.84 | 221.1 → 221.1 | +0.0% | +1.17 | original cylinder capacity retained |
| 2-8-2 Mikado | 3.67 → 3.67 | 221.1 → 221.1 | +0.0% | +0.00 | stock unchanged |
| 2-8-4 Berkshire | 4.20 → 3.90 | 182.6 → 196.6 | +7.7% | +0.23 |  |
| 2-8-0 High-speed Consolidation | 5.36 → 5.36 | 199.7 → 199.7 | +0.0% | +1.69 | original cylinder capacity retained |
| 2-8-2 High-speed Mikado | 4.45 → 4.45 | 199.7 → 199.7 | +0.0% | +0.78 | original cylinder capacity retained |
| 2-8-4 High-speed Berkshire | 4.18 → 3.88 | 183.3 → 197.4 | +7.7% | +0.21 |  |
| 4-8-0 Twelve wheeler | 4.84 → 4.84 | 221.1 → 221.1 | +0.0% | +1.17 | original cylinder capacity retained |
| 4-8-2 Mountain | 4.21 → 4.02 | 210.8 → 221.1 | +4.9% | +0.35 | original cylinder capacity retained |
| 4-8-4 Northern | 4.20 → 3.90 | 182.6 → 196.6 | +7.7% | +0.23 |  |
| 0-10-0 Ten-wheel switcher | 5.50 → 5.50 | 221.1 → 221.1 | +0.0% | +1.83 | original cylinder capacity retained |
| 0-10-2 Union | 4.29 → 4.09 | 207.0 → 217.1 | +4.9% | +0.42 |  |
| 0-10-4 | 4.28 → 4.08 | 179.1 → 187.9 | +4.9% | +0.41 |  |
| 2-10-0 Decapod | 4.84 → 4.84 | 221.1 → 221.1 | +0.0% | +1.17 | original cylinder capacity retained |
| 2-10-2 Santa Fe | 4.29 → 4.09 | 207.0 → 217.1 | +4.9% | +0.42 |  |
| 2-10-4 Texas | 4.28 → 4.08 | 179.1 → 187.9 | +4.9% | +0.41 |  |
| 4-10-0 Mastodon | 4.84 → 4.84 | 221.1 → 221.1 | +0.0% | +1.17 | original cylinder capacity retained |
| 4-10-2 Southern Pacific | 4.29 → 4.09 | 207.0 → 217.1 | +4.9% | +0.42 |  |
| 4-10-4 | 4.28 → 4.08 | 179.1 → 187.9 | +4.9% | +0.41 |  |
| 0-12-0 Twelve-wheel switcher | 4.71 → 4.71 | 258.2 → 258.2 | +0.0% | +1.04 | original cylinder capacity retained |
| 2-12-0 | 4.41 → 4.31 | 242.6 → 248.2 | +2.3% | +0.64 |  |
| 2-12-2 | 4.40 → 4.30 | 202.0 → 206.7 | +2.3% | +0.63 |  |
| 4-12-2 Union Pacific | 4.40 → 4.30 | 202.0 → 206.7 | +2.3% | +0.63 |  |
| 2-4-4-2 Articulated | 4.25 → 3.94 | 175.6 → 189.0 | +7.6% | +0.28 |  |
| 4-4-4-4 Articulated | 4.23 → 3.93 | 143.9 → 154.9 | +7.6% | +0.26 |  |
| 4-0-4 Drivers not found | n/a | 0.0 → 0.0 | 0% | n/a | unpowered novelty unchanged |
| 2-6-0 Mogul | 4.06 → 3.66 | 145.1 → 160.9 | +10.9% | -0.01 | >10% change |
| 2-4-0 | 3.93 → 3.43 | 94.9 → 108.8 | +14.6% | -0.24 | >10% change |
| 2-2-2 Giant-driver clearance prototype | 1.57 → 1.57 | 112.4 → 112.4 | +0.0% | -2.10 | SPECIAL MEASURES prototype unchanged and excluded |
| 2-6-6-2 Articulated Mallet | 4.47 → 4.36 | 204.3 → 209.0 | +2.3% | +0.70 |  |
| 2-2-2-2-2 Articulated Triplex | 4.21 → 3.81 | 125.7 → 138.9 | +10.5% | +0.15 | >10% change |
| 4-14-4 | 4.50 → 4.50 | 170.2 → 170.2 | +0.0% | +0.83 |  |
| 0-2-2-2-0 Articulated Triplex | 4.20 → 3.80 | 126.0 → 139.3 | +10.5% | +0.14 | >10% change |
| 0-8-8-0 Angus | 4.55 → 4.57 | 267.3 → 266.5 | -0.3% | +0.90 |  |
| 2-6-8-0 Gt. Northern | 4.55 → 4.56 | 233.9 → 233.5 | -0.2% | +0.89 |  |
| 2-8-8-8-2 Erie Triplex | 4.55 → 4.56 | 237.6 → 237.0 | -0.3% | +0.89 |  |
| 2-6-2 Prairie | 4.08 → 3.68 | 144.1 → 159.8 | +10.9% | +0.02 | >10% change |
| 2-6-6-0 Articulated Mallet | 4.46 → 4.36 | 234.0 → 239.4 | +2.3% | +0.69 |  |
| 2-10-10-2 Articulated Mallet | 4.55 → 4.56 | 222.8 → 222.1 | -0.3% | +0.90 |  |
| Erie tender (separate) | 4.19 → 3.89 | 93.6 → 100.8 | +7.7% | +0.23 | separate tender clamp unchanged |

Negative FoA delta means more nominal cylinder demand relative to adhesive weight than stock. It does not mean reduced rail friction. TE remains nominal cylinder TE at85% of14bar gauge, not sustained drawbar power or a tonnage rating.

**2-2-2 special measures:** existing non-colliding clearance prototype, dimensions and calibration bypass unchanged. Its uncalibrated FoA is not a successful balanced design and must not be used to demonstrate variation. 4-0-4 remains unpowered.

Stock, actual mass, effective driver-weight assumptions, adhesion/sand/brakes, curving, wheel size/wheelbase, geometry, boiler/chest/flow limits and original-cylinder maximum are preserved. Smaller bore/area/swept-volume changes remain the same existing calibration mechanism. No conventional locomotive is deliberately made unusable; sustained career loads are not guaranteed by this calculation.

Validation reuses existing native fixture:4142checks/132layout-masscases/1400nativeTicks; includes new two-sided regression assertions and existing stock/prototype/novelty exclusion, no boost, restore, isolation and resource independence. Compiled comparison105protectedmethodsunchanged; production build0warnings/errors. No new live-game acceptance claim.

Build1.21.2 installed; 42 packaged files verified and 14 settings/diagnostic files preserved. No live1.21.2 validation claim.


