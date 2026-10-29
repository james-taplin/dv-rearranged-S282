**Rearranged S282 - 2.0.8-alpha Multiplayer**

An experimental multiplayer build of JimTaplin's B99 continuation of SatiricRug's Rearranged S282. Use your own radio to request an arrangement change, let the host approve it, and see the same running gear together.

**This is a separate optional alpha download.** It uses the existing multiplayer implementation and the 2.0-beta arrangement baseline. Later single-player and independent-wheelslip development is not included. Compilation and offline checks have passed; live multiplayer acceptance is still pending. Please try it with friends and report anything that breaks. No intensive testing checklist is expected.

**What is included**

- 48 retained wheel arrangements, from small rigid engines to Mallets and triplexes.
- Optional Erie powered S282 tenders for 43 compatible layouts.
- Wheel Rearranger radio requests from the host and guests.
- Host validation of operating conditions before an arrangement changes.
- Shared per-car host settings for running gear, appearance and powered-tender choices.
- Arrangement snapshots for late joiners and cars that load after the update arrives.
- Host controls to allow everyone, lock all guests, or lock individual players out of changing arrangements.
- Single-player fallback when Multiplayer is absent or inactive.


Native Multiplayer handles train movement and simulation-port replication. The adapter shares the extra arrangement state; it does not introduce a second train-motion system.

**Requirements**

Derail Valley B99 on Windows, Unity Mod Manager 0.27.3 or newer, and [Locomotive Mesh Splitter](https://www.nexusmods.com/derailvalley/mods/867). Mesh Splitter 1.1.9 is the reference version. For online play, everyone also needs **Derail Valley Multiplayer 0.1.16.0-beta and this identical 2.0.8-alpha package**. Follow Multiplayer's own installation instructions. Both RearrangedS282 DLLs must be present; Multiplayer supplies its own API.

Multiplayer is optional for ordinary single-player use. Custom Car Loader and MultiCylinderChuffFix are not dependencies. **Please leave Realistic Firing off for this alpha.**

**Installation**

Close the game and replace the existing RearrangedS282 installation through UMM, or extract the RearrangedS282 folder into Mods. Retain personal Settings.xml if wanted when replacing the folder. Use one Rearranged S282 package at a time: the alpha and normal download share a mod ID and folder.

**Radios and host permissions**

Each player can use Wheel Rearranger on their own radio. Stop the S282, close its regulator and let driving force and wheel slip settle first. Guests send requests; the host checks them and distributes the accepted arrangement.

The host opens **Ctrl+F10 > Rearranged S282**. **Allow guests to change arrangements** controls all guests; each matching player also has an individual **may change arrangements** toggle. A locked guest still sees everyone else's changes and can drive normally. Locks are session scoped and clear on disconnect.

Host arrangement options apply to that car when its arrangement is changed or reapplied. Guests keep their personal saved settings. Every player must use the same package; incompatible builds and guests that cannot synchronize are disconnected with an explanation.

**Have a play**

Try the radios, drive together and check that the arrangements, rods and steam effects look right for everyone. Joining a running session is handy to try too. Just have fun; if anything breaks, send the arrangement, host or guest role, powered-tender choice and what happened. Screenshots and host/client Player.log files help. MULTIPLAYER_TESTING.txt is included for anyone who wants more detail.

**Known limits**

This is not a confirmed live multiplayer release. VR is unverified. The giant 2-2-2 is unfinished and the 4-0-4 is unpowered. True series compound expansion and independent powered-tender wheel slip are not implemented here. Run structural diagnostics in single player only.

[Multiplayer branch source](https://github.com/james-taplin/dv-rearranged-S282/tree/codex/s282-multiplayer)
