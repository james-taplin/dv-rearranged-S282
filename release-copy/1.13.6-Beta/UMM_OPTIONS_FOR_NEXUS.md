## Mod options

Open Unity Mod Manager, expand **Rearranged S282**, then open **Show arrangement
and random-spawn settings**. Options are saved between sessions, so updating the
mod does not necessarily restore the defaults below.

For appearance changes, stop the locomotive, close the regulator and reapply its
arrangement with the Wheel Rearranger radio. Changing a checkbox alone does not
rebuild a locomotive that is already spawned.

### Valve gear and cylinder appearance

These are appearance options. Hiding valve gear does not disable the locomotive's
cylinders, reduce its power or change the selected wheel arrangement.

| Option | What it controls | Fresh-install default |
|---|---|---|
| Near-vanilla cylinder casings | Reduces the oversized cylinder casing appearance on supported conversions while retaining the mechanism | On |
| Show x-4-x valve gear | Gear visibility on rigid four-coupled conversions, such as 4-4-0, 4-4-2, 4-4-4 and 2-4-0 | Off |
| Show x-6-x valve gear | Gear visibility on rigid six-coupled conversions, such as 4-6-0 and 2-6-0 | Off |
| Polished Walschaerts on 4-6-2 / 4-6-4 | Enables the aligned Pacific/Hudson gear treatment; it can show their gear even with the general x-6-x option off | On |
| Show x-8-x valve gear | Gear visibility on converted rigid eight-coupled layouts; stock 2-8-2 retains its native treatment | On |
| Show high-speed 2-8-2 valve gear | Despite the label, this controls the high-speed 2-8-0, 2-8-2 and 2-8-4 layouts | Off |
| Show x-10-x valve gear | Gear visibility on rigid ten-coupled conversions | On |
| Show x-12-x valve gear | Gear visibility on rigid twelve-coupled conversions | On |

The Prairie uses the same polishing switch as the Pacific/Hudson treatment.
These rigid-family switches are not separate front/rear controls for articulated
locomotives. The articulated layouts prepare their own gear. A missing return
crank on only one engine should therefore be reported rather than assumed to be
an intentional setting.

### Trailing truck styles

The 4-4-2, 4-6-2, x-8-2 and x-10-2 options choose between the listed large or
small trailing-wheel treatments. Where offered, the alternate small-wheel option
also changes their placement. Reapply the arrangement after changing these.
These options choose the appearance of the trailing truck, not another powered
driver group.

### Curving

**Extra curve drag** and **extra flange squeal** are on by default. They add the
mod's long-rigid-wheelbase treatment on tighter curves. They are independent
options. Neither checkbox enables an additional custom derailment-risk feature.

### Oil cups

**Oil cups on every driven big end** is on by default. This enables the mod's
additional crank-pin oiling treatment. Reapply the arrangement after changing it.

### Random arrangements

**Spawn S282 with random wheel arrangement** is off by default. Enable it to let
new S282s choose from the arrangements ticked in the list. Individual checkboxes
select the eligible pool; they do nothing while the master random-spawn switch
is off. Existing saved locomotive choices are retained. An empty eligible pool
falls back to stock 2-8-2. Experimental and articulated choices have their own
pool switches; enabling random spawning does not automatically enable them all.

### Experimental and diagnostic controls

The giant-driver **2-2-2 diameter** control belongs to an unfinished, non-colliding
clearance prototype. It is not a finished realistic single-driver locomotive.

**Structural test** and **power diagnostics** are troubleshooting tools, not
settings required for normal play. Structural testing needs two stationary S282s
and temporarily cycles one through the supported layouts. Power recording saves
timestamped diagnostic files while you drive; stop recording when finished.

Version 1.13.6 does not contain the later private-build driver-spacing sliders or
the later UMM reapply/reset buttons. Do not look for those in the public beta.

### Reporting missing gear

Please include the exact arrangement name, mod version, which engine and side
are affected, whether it happens close up or only at a distance, and a screenshot.
A screenshot of the valve-gear options is useful too. Mention whether it happened
immediately after spawning or after switching from another arrangement.
