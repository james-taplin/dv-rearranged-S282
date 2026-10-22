# 1.21 FoA refinement - 1.21.1 candidate

Before = built 1.21.0 universal-4.4 candidate, not the installed 1.20.0 release. Stock S282 remains unchanged. All values use the same rated 124 t locomotive / 50 t tender baseline.

Nominal TE is cylinder TE at 85% of 14 bar gauge pressure. FoA = adhesive weight / nominal TE. Percent change is nominal TE relative to the previous candidate. These are not measured drawbar ratings or speed/power curves.

The ceiling now varies continuously with machinery: `4.2 + coupled-group term + diameter term + axle-load term + cylinder-count term`, bounded 3.85-4.55. Terms are deliberately modest design allowances, not claimed historical measurements:

- Coupled-group term: 0.10 per powered axle relative to four, bounded -0.20 to +0.30.
- Diameter term: -0.15 times diameter departure from 1.424 m, bounded -0.10 to +0.10.
- Effective load term: +0.10 times driver-axle-load departure from 20 t, bounded -0.05 to +0.05.
- Cylinder-count term: +0.025 per cylinder beyond two, bounded +0.10. Extra cylinders share the existing phase/slip group, so no invented smoothing bonus.

No arrangement IDs receive a prescribed performance tier. The existing effective driver-weight model is unchanged. The ceiling only reduces original cylinder capacity; it never forces a weaker original engine up to a target FoA. The independent Erie tender uses the same rule with its own machinery and rated load.

| Arrangement | FoA before → after | Nominal TE kN before → after | TE change | Notes |
|---|---:|---:|---:|---|
| 4-4-0 American | 4.40 → 3.93 | 84.7 → 94.9 | +12.0% | >10% change |
| 4-4-2 Atlantic | 4.40 → 3.93 | 84.7 → 94.9 | +12.0% | >10% change |
| 4-4-4 Reading | 4.40 → 3.93 | 84.7 → 94.9 | +12.0% | >10% change |
| 4-6-0 Ten-wheeler | 4.40 → 4.06 | 133.8 → 145.1 | +8.4% |  |
| 4-6-2 Pacific | 4.40 → 4.06 | 133.8 → 145.1 | +8.4% |  |
| 4-6-4 Hudson | 4.40 → 4.06 | 133.8 → 145.1 | +8.4% |  |
| 0-8-0 Eight–wheel switcher | 5.50 → 5.50 | 221.1 → 221.1 | +0.0% | original cylinder capacity retained |
| 0-8-2 | 4.40 → 4.21 | 201.8 → 210.8 | +4.4% |  |
| 0-8-4 | 4.40 → 4.20 | 174.2 → 182.6 | +4.8% |  |
| 2-8-0 Consolidation | 4.84 → 4.84 | 221.1 → 221.1 | +0.0% | original cylinder capacity retained |
| 2-8-2 Mikado | 3.67 → 3.67 | 221.1 → 221.1 | +0.0% | STOCK REFERENCE - unchanged |
| 2-8-4 Berkshire | 4.40 → 4.20 | 174.2 → 182.6 | +4.8% |  |
| 2-8-0 High-speed Consolidation | 5.36 → 5.36 | 199.7 → 199.7 | +0.0% | original cylinder capacity retained |
| 2-8-2 High-speed Mikado | 4.45 → 4.45 | 199.7 → 199.7 | +0.0% | original cylinder capacity retained |
| 2-8-4 High-speed Berkshire | 4.40 → 4.18 | 174.2 → 183.3 | +5.2% |  |
| 4-8-0 Twelve wheeler | 4.84 → 4.84 | 221.1 → 221.1 | +0.0% | original cylinder capacity retained |
| 4-8-2 Mountain | 4.40 → 4.21 | 201.8 → 210.8 | +4.4% |  |
| 4-8-4 Northern | 4.40 → 4.20 | 174.2 → 182.6 | +4.8% |  |
| 0-10-0 Ten-wheel switcher | 5.50 → 5.50 | 221.1 → 221.1 | +0.0% | original cylinder capacity retained |
| 0-10-2 Union | 4.40 → 4.29 | 201.8 → 207.0 | +2.6% |  |
| 0-10-4 | 4.40 → 4.28 | 174.2 → 179.1 | +2.8% |  |
| 2-10-0 Decapod | 4.84 → 4.84 | 221.1 → 221.1 | +0.0% | original cylinder capacity retained |
| 2-10-2 Santa Fe | 4.40 → 4.29 | 201.8 → 207.0 | +2.6% |  |
| 2-10-4 Texas | 4.40 → 4.28 | 174.2 → 179.1 | +2.8% |  |
| 4-10-0 Mastodon | 4.84 → 4.84 | 221.1 → 221.1 | +0.0% | original cylinder capacity retained |
| 4-10-2 Southern Pacific | 4.40 → 4.29 | 201.8 → 207.0 | +2.6% |  |
| 4-10-4 | 4.40 → 4.28 | 174.2 → 179.1 | +2.8% |  |
| 0-12-0 Twelve-wheel switcher | 4.71 → 4.71 | 258.2 → 258.2 | +0.0% | original cylinder capacity retained |
| 2-12-0 | 4.40 → 4.41 | 243.3 → 242.6 | -0.3% |  |
| 2-12-2 | 4.40 → 4.40 | 201.8 → 202.0 | +0.1% |  |
| 4-12-2 Union Pacific | 4.40 → 4.40 | 201.8 → 202.0 | +0.1% |  |
| 2-4-4-2 Articulated | 4.40 → 4.25 | 169.4 → 175.6 | +3.7% |  |
| 4-4-4-4 Articulated | 4.40 → 4.23 | 138.2 → 143.9 | +4.1% |  |
| 4-0-4 Drivers not found | n/a → n/a | 0.0 → 0.0 | +0.0% | UNPOWERED novelty |
| 2-6-0 Mogul | 4.40 → 4.06 | 133.8 → 145.1 | +8.4% |  |
| 2-4-0 | 4.40 → 3.93 | 84.7 → 94.9 | +12.0% | >10% change |
| 2-2-2 Giant-driver clearance prototype | 1.57 → 1.57 | 112.4 → 112.4 | +0.0% | EXPERIMENTAL - incomplete geometry/physics |
| 2-6-6-2 Articulated Mallet | 4.40 → 4.47 | 207.3 → 204.3 | -1.5% |  |
| 2-2-2-2-2 Articulated Triplex | 4.40 → 4.21 | 120.4 → 125.7 | +4.4% |  |
| 4-14-4 | 4.40 → 4.50 | 174.2 → 170.2 | -2.3% |  |
| 0-2-2-2-0 Articulated Triplex | 4.40 → 4.20 | 120.4 → 126.0 | +4.7% |  |
| 0-8-8-0 Angus | 4.40 → 4.55 | 276.5 → 267.3 | -3.3% |  |
| 2-6-8-0 Gt. Northern | 4.40 → 4.55 | 241.9 → 233.9 | -3.3% |  |
| 2-8-8-8-2 Erie Triplex | 4.40 → 4.55 | 245.7 → 237.6 | -3.3% |  |
| 2-6-2 Prairie | 4.40 → 4.08 | 133.8 → 144.1 | +7.7% |  |
| 2-6-6-0 Articulated Mallet | 4.40 → 4.46 | 237.0 → 234.0 | -1.2% |  |
| 2-10-10-2 Articulated Mallet | 4.40 → 4.55 | 230.4 → 222.8 | -3.3% |  |
| Erie tender (separate) | 4.40 → 4.19 | 89.2 → 93.6 | +4.9% | separate grip clamp |

## Assessment

Substantial changes (>10% versus the universal-4.4 candidate): 4-4-0 American, 4-4-2 Atlantic, 4-4-4 Reading, 2-4-0. These remain below original cylinder specifications; they relax the previous reduction rather than adding power beyond the original machinery.
No conventional layout falls below 40% of stock nominal TE in this calculation.
Conventional four-coupled layouts remain the light-work end of the range, around 43% of stock nominal cylinder TE. The unpowered novelty and unfinished giant-driver prototype are intentionally excluded from usability conclusions. Calculations do not establish a guaranteed career tonnage.

Validation: existing native fixture passes 132 layout/mass cases and 1,400 native engine ticks (4,140 checks), including reduction-only dimensions, independent nominal-force arithmetic, resource-independent sizing, second-car isolation and exact restore. Production build has zero warnings/errors. Compiled comparison: 105 protected methods unchanged; existing physics wiring check passes. No extra large validation programme, new gameplay system or installation.

Preserved: stock S282, actual vehicle mass, effective adhesion shares, native adhesion/sanding/braking/curving, wheel radii and wheelbase, boiler and steam-flow/chest limits, animation and geometry. Bore/area/swept-volume adjustment remains the existing calibration mechanism.

Historical engineering basis and pressure convention remain in [PHYSICS_BASELINE.md](PHYSICS_BASELINE.md). These small coefficients express the requested individuality; they are not uniquely implied by real-world physics.

**Build 1.21.1.0 installed at James's request. All 41 package files verified; settings and diagnostics preserved. Live acceptance of this calibration remains pending.**
