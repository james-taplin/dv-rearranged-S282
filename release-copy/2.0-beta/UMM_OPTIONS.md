# Rearranged S282 options

Open UMM and expand Rearranged S282. Settings are saved, so updating the mod does not reset your choices.

## Applying changes

Stop, close the regulator and highlight your S282 with Wheel Rearranger. Use **REAPPLY CURRENT ARRANGEMENT** after changing an appearance or tender option. The change affects that locomotive; it does not rebuild all loaded locomotives. **RESET WHEEL-ARRANGER RADIO SELECTION** clears a stuck radio selection.

## Powered tender

Open **Powered tender — arrangement options (test)** and tick the layouts that should use the Erie powered S282 tender. Only Erie is on by default. Choices apply on spawn, reload or reapply. A regular S282 tender must be coupled behind the locomotive.

There are 43 compatible layouts. The three-engine locomotive triplexes, unpowered 4-0-4 and unfinished giant 2-2-2 are excluded. Untick Erie and reapply to use its locomotive with an ordinary tender.

## Valve gear

| Setting | Fresh-install default |
|---|---|
| x-4-x gear | Off |
| x-6-x gear | Off |
| Polished Pacific / Hudson Walschaerts | On |
| x-8-x gear | On |
| High-speed 2-8-2 gear | Off |
| x-10-x gear | On |
| x-12-x gear | On |

The high-speed gear switch covers all three high-speed eight-coupled variants. The Prairie shares the Pacific/Hudson polishing switch. These are appearance options; they do not turn off cylinder power. Articulated engines prepare their own gear rather than following separate rigid-family front/rear switches.

Cylinder proportions are now built in. There is no cylinder-sizing toggle or wheel-spacing slider to set up.

## Trailing trucks, curves and oiling

The 4-4-2, 4-6-2, x-8-2 and x-10-2 options select their listed trailing-wheel styles. Reapply after changing them.

Extra curve drag and flange squeal are separate switches, both on by default. They add the mod's longer-rigid-wheelbase treatment on tighter curves. They do not enable an extra custom derailment-risk system.

Big-end oilcups are on by default. Reapply after changing the option. B99's normal oilcup use and refill handling are retained.

## Random spawning

The master random-spawn switch is off by default. Turn it on, then select the layouts you want in the pool. Articulated and novelty layouts have their own checkboxes and default off. Saved choices stay with their locomotives. An empty pool uses stock 2-8-2.

## Structural test

This is a troubleshooting tool, not a normal setup step. In a sandbox, prepare two stationary stock S282s with regulators closed. The test locomotive needs a regular S282 tender behind it; the control locomotive must have no powered tender active.

Press **RUN STRUCTURAL TEST — two stationary S282s**, then close UMM so the simulation can tick. It runs 48 ordinary-tender cases and 43 powered-tender cases, followed by six direct switches checking repeated returns to Erie. The temporary tender choices do not change your saved preferences. The subject ends in stock with an ordinary tender.

Results are written to `stage2-runtime-checks.json` in the mod folder. Power recording is not included. The structural test cannot judge appearance, audible sound quality, moving-rod clearance or hauling performance.
