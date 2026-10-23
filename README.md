# Rearranged S282 — B99

Turn the S282 into a small Mogul, a large-wheeled Pacific, a long rigid freight engine or an articulated Mallet. This is JimTaplin's B99 continuation of SatiricRug's Rearranged S282, continued with the original author's permission.

There are **48 selectable arrangements**. The original layouts are retained alongside the Prairie, larger Mallets and triplexes. The new **2-4-4-4-0** puts three two-axle driver groups under the locomotive, with an ordinary unpowered tender.

## What the mod does

- Changes the S282's drivers, carrying wheels and running gear to suit the selected arrangement.
- Uses DV's native bogies and articulation constraints for the articulated layouts.
- Gives modified arrangements different starting pull and adhesion characteristics through driver size, driven weight and cylinder sizing. The stock 2-8-2 keeps its original physics when its powered-tender option is off.
- Adds matching cylinder casings, rods, guides and steam pipes, using the original locomotive and mod assets wherever possible.
- Supports B99 oilcups and refill points, with optional oiling cups on the driven big ends.
- Provides sounds and effects for the additional engines, including the Erie's tender exhaust, cylinder-cock steam, drips and driving-gear clanks.
- Adds optional curve drag and flange squeal for longer rigid wheelbases.
- Remembers manually selected arrangements with each locomotive and can give newly spawned S282s random arrangements from your chosen list.
- Retains explosion materials and compatibility handling for Cruzer's Delta trailing trucks.

## Powered tenders

You can now use the Erie powered tender with **43 compatible arrangements**, including rigid locomotives. Open the powered-tender section in UMM and tick the arrangements you want. Erie is the only one enabled by default, and you can untick it to use its locomotive with an ordinary tender.

For a locomotive already in the world, stop, close the regulator, highlight it with Wheel Rearranger and press **REAPPLY CURRENT ARRANGEMENT** in UMM. The checkboxes also apply when locomotives spawn or their arrangements are restored on loading. They do not change every loaded locomotive immediately.

The powered tender adds pull and draws steam from the same S282 boiler. It needs a regular S282 tender coupled behind the locomotive. Other tender types are not supported. The three-engine locomotive triplexes already use all three engine simulations; the unpowered 4-0-4 and unfinished giant 2-2-2 are also excluded.

## Included arrangements

| Family | Arrangements |
|---|---|
| Four driving wheels | 4-4-0 American, 4-4-2 Atlantic, 4-4-4 Reading, 2-4-0 |
| Six driving wheels | 4-6-0 Ten-wheeler, 4-6-2 Pacific, 4-6-4 Hudson, 2-6-0 Mogul, 2-6-2 Prairie |
| Eight driving wheels | 0-8-0, 0-8-2, 0-8-4, 2-8-0, 2-8-2, 2-8-4, 4-8-0, 4-8-2, 4-8-4, plus high-speed 2-8-0, 2-8-2 and 2-8-4 variants |
| Ten driving wheels | 0-10-0, 0-10-2, 0-10-4, 2-10-0, 2-10-2, 2-10-4, 4-10-0, 4-10-2, 4-10-4 |
| Larger rigid layouts | 0-12-0, 2-12-0, 2-12-2, 4-12-2, 4-14-4 |
| Articulated two-engine layouts | 2-4-4-2, 4-4-4-4, 2-6-6-2 Mallet, 0-8-8-0 Angus, 2-6-8-0 Great Northern, 2-6-6-0 Mallet, 2-10-10-2 Mallet |
| Triplexes | 2-2-2-2-2, 0-2-2-2-0, 2-8-8-8-2 Erie with its tender engine, 2-4-4-4-0 |
| Novelty and prototype | Unpowered 4-0-4 Drivers not found; unfinished giant-driver 2-2-2 |

## Requirements and installation

You need **Derail Valley B99 on Windows**, **Unity Mod Manager** and [**Locomotive Mesh Splitter**](https://www.nexusmods.com/derailvalley/mods/867). The mod's manager requirement is UMM 0.27.3 or newer; Mesh Splitter 1.1.9 is the tested version.

Close the game and install the ZIP through UMM. When updating, install it over your existing Rearranged S282 installation. Your saved settings are retained. Custom Car Loader is not required.

The mod handles its own extra-engine sounds. **MultiCylinderChuffFix is optional**, not a dependency: its entry in `LoadAfter` only sets load order if it is installed.

## Choosing an arrangement

Select **Wheel Rearranger** on the comms radio, point at a stationary S282 and select it. Cycle through the list and confirm your choice. Close the regulator and let driving force and wheel slip settle before changing layouts.

UMM contains the valve-gear, trailing-truck, curve-effect, oiling and random-spawn options. Reapply the arrangement after appearance changes. If the radio selection gets stuck, use **RESET WHEEL-ARRANGER RADIO SELECTION**.

Some rigid-family valve-gear options are off by default. Hiding the gear is a visual choice and does not remove cylinder power. The Pacific, Hudson and Prairie share the polished Walschaerts option; articulated layouts prepare their own gear.

Random spawning is off by default. Enable its master switch, then tick the arrangements you want in the pool. Saved locomotive choices are kept, and an empty pool falls back to stock. The new triplex is not in the default random pool.

The accepted proportions are built in. The temporary resizing sliders and power recorder have been removed. The **structural test button** remains available for troubleshooting; it is not needed for normal play.

## A few things to know

These are S282 conversions, rather than exact replicas of every historical locomotive named in the list. The giant 2-2-2 is still an unfinished clearance prototype with hidden rods and non-colliding added wheels. The 4-0-4 is deliberately unpowered.

The Mallet and Erie sound profiles have a different character, but **true series compound expansion is not simulated**. Powered-tender grip still uses the existing simplified model with locomotive-linked wheel speed; independent tender slipping is not implemented.

**Realistic Firing is not currently supported alongside this mod.** Please reproduce steam, power or speed problems without it before reporting them. Multiplayer and VR compatibility have not been established.

If something goes wrong, include the arrangement, mod version, whether a powered tender was enabled and what you switched from. A screenshot and `Player.log` are useful. There is no need to run the structural test unless you are helping investigate a problem.

## Credits

**Altfuture** / *Derail Valley;*\
**SatiricRug / Peter Schinske** / *the original Rearranged S282, its custom running gear and assets, and Locomotive Mesh Splitter;*\
**discorddraconequus** / *the Verde stage-specific chuff reference that informed the Mallet and Erie sound tuning;*\
**Dad's Derailroader** / *reference tooling for the B99 driver, wheel-radius and adhesion conversion approach;*\
**thomas404dk** / *detailed testing, videos, logs and bug reports;*\
**SPRINTEREX** / *the independently selectable powered-tender request;*\
**the Nexus testers and bug reporters** / *feedback and help improving the mod;*\
**Derail Valley Modding** / *the original mod template;*\
**Dummiesman** / *Runtime OBJ Importer;*\
**Radu, Perfare, aelurum and contributors** / *AssetStudio and AssetStudioMod;*\
**Milosz Krajewski and Yann Collet** / *K4os.Compression.LZ4 and LZ4;*\
**Jb Evain, Novell and contributors** / *Mono.Cecil;*\
**James Newton-King and contributors** / *Json.NET;*\
**.NET Foundation, Microsoft and contributors** / *the bundled .NET compatibility libraries;*\
**Brotli Authors / Google** / *embedded Brotli decompression;*\
**Matthaus G. Chajdas** / *embedded CSspv shader conversion;*\
**Igor Pavlov** / *the embedded public-domain LZMA SDK;*

**AND:**\
**JimTaplin** / *B99 continuation, arrangement development, physics calibration, integration, testing and releases;*\
**Claude and Codex** / *coding, investigation, geometry tooling and implementation assistance;*

## Licence and source

The original mod is by SatiricRug / Peter Schinske and is distributed under the MIT licence. Its original copyright and licence text are retained in `LICENSE`. Bundled libraries keep their own licences; the complete notices are included in `THIRD_PARTY_NOTICES.txt` and `licenses/`.

The Verde contribution is a reference for the sound-tuning technique; no Verde code, meshes or audio assets are bundled. Derailroader and MultiCylinderChuffFix are not bundled or required at runtime.

Original source: https://github.com/pjschinske/dv-rearranged-S282
Original Nexus mod: https://www.nexusmods.com/derailvalley/mods/752
B99 fork: https://github.com/james-taplin/dv-rearranged-S282

## Building the B99 fork

The runtime project targets .NET Framework 4.8 and references the assemblies in your own Derail Valley installation. `Build.RuntimeTrial.ps1` builds and packages a development version; pass `-GamePath` if your game is installed elsewhere. Installation only happens when `-Install` is supplied and the game is closed.

```powershell
.\Build.RuntimeTrial.ps1 -Version 1.23.2
```

The existing custom drivetrain bundle is reused. Rebuilding its authored models is a separate asset workflow, documented in the original upstream project's README. Game assemblies and locally extracted game meshes are not included in the public package.

See [the current options guide](release-copy/1.23.2-Beta/UMM_OPTIONS.md) and [changes since 1.22.2](release-copy/1.23.2-Beta/CHANGELOG.txt). Development evidence and outstanding work are recorded in `CURRENT_HANDOVER.md` and `TODO.md`.
