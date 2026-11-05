REARRANGED S282 B99 - 2.0.9.8 FINAL BETA

Change the S282 into 51 selectable arrangements, from small rigid engines to
large express locomotives, Mallets and triplexes. This final planned beta before
2.1 stable adds 4-6-2 Scotch Glider and 4-6-6-4 Little Boy, restores supported
reach/lubricator linkages and includes the latest Franklin, renderer-lifetime,
multiplayer-state, OBJ-import and structural-diagnostic repairs.

CHANGELOG.txt contains the full changes since 2.0.9.3-beta.

REQUIREMENTS
Derail Valley B99 on Windows, Unity Mod Manager 0.27.3 or newer, and Locomotive
Mesh Splitter (build reference 1.1.9). Keep the included assets and exporter.
Multiplayer is optional for ordinary single-player use. Online, everyone needs
DV Multiplayer 0.1.16.0-beta and this exact complete Rearranged S282 package.
Multiplayer and its API are not bundled. Leave Realistic Firing off for initial
beta testing. Use one Rearranged S282 installation at a time.

INSTALL / UPDATE
Close the game. Back up the existing RearrangedS282 folder and your Settings.xml
outside Mods. Install the new ZIP through Unity Mod Manager, or manually extract
its single RearrangedS282 folder into Mods. Restore your personal Settings.xml
if needed. Do not mix runtime DLLs or assets from different versions.
This ZIP contains no personal settings, game captures or logs.

RADIO AND SETTINGS
Use Wheel Rearranger on your own radio and point at a normal S282. Stop, close
the regulator and allow wheel slip/force to settle before changing arrangements.
The selected layout is saved with that locomotive. In Ctrl+F10 > Rearranged
S282, REAPPLY CURRENT ARRANGEMENT applies changed appearance/tender preferences
to the highlighted locomotive. See UMM_OPTIONS.md for the available controls.

Scotch Glider is a large-driver Pacific and supports the optional powered tender.
Little Boy uses two three-axle locomotive engines and an ordinary unpowered
tender. Its four-wheel leading/trailing trucks distinguish it from the Mallet.
Both new random-spawn options default off. Existing saved arrangement IDs 0-48
are preserved; the new IDs are appended. Random arrangement spawning is optional
and off by default. The optional powered tender is available on 45 layouts;
Erie is the only powered-tender preference enabled by default.

NATIVE OILING
The original guide, moving-crosshead and main-rod cups are retained on each side.
The optional big-end oiling feature adds dedicated cups to other driven crank
pins. Little Boy/Mallet have 16 cups under that setting: 12 crank-pin cups plus
four native guide/crosshead cups. The guide-top cup is a native oiling point.
Automatic filling/lid closing is optional and off by default. Reapply after
changing the big-end cup preference. Settings and oiling save data are retained.

MULTIPLAYER
Each player uses their own Wheel Rearranger radio. The host validates requests,
applies accepted layouts and shares the relevant effective per-car settings.
Ctrl+F10 > Rearranged S282 > Multiplayer arrangement permissions lets the host
allow or block guest requests. Blocked players still see updates; session locks
reset when they leave. Guest personal settings are preserved. All peers need the
same complete build. The native multiplayer mod owns transport and train motion.
Without an active multiplayer session, normal single-player controls apply.

Independent locomotive-group slip uses native calculations and group RPM/phase
presentation. The host calculates traction online and guests follow replicated
native outputs. The powered tender retains pooled/manual force and its own
diameter-adjusted, movement-linked native RPM animation. It has no independent
tender slip. True compound expansion is not simulated. The giant 2-2-2 remains
an unfinished clearance prototype. Cab/global slip and extra curve drag retain
their existing shared-controller behavior.

TEST STATUS / REPORTS
The installed 2.0.9.8 passed all 96 stationary structural cases and all six
return-sequence cases, restoring stock and preserving tender preferences.
The Release core/bridge compiled with zero warnings/errors. Scoped offline
geometry, lifetime, importer, authority and readiness checks also passed.
Structural checks do not prove moving clearances, sustained performance or live
peer behavior. Multiplayer was absent in that successful structural session.
This remains a beta; visual/driving and live multiplayer feedback are welcome.

For the optional structural sweep, place two stationary stock S282s close
together, one with its regular tender coupled behind it. Keep both regulators
closed and stay nearby so exterior interactions remain loaded. Note the selected
IDs in status, then close UMM so simulation can tick. The test now refuses or
stops clearly if the required interactions are unavailable. An aborted run is
not a pass. Use a spare sandbox save for diagnostic switching.

Include the version, arrangement, gear/tender choices, single-player or host/
guest role, steps, a short video where useful and Player.log in a bug report.
Preserve Settings.xml. Include stage2-runtime-checks.json if you ran the sweep;
online, include logs from the host and affected guest. See MULTIPLAYER_TESTING.txt.

Full credits and third-party notices are included in CREDITS.txt, LICENSE,
THIRD_PARTY_NOTICES.txt and licenses/.
Original mod: SatiricRug / Peter Schinske. B99 continuation: JimTaplin.
Source: https://github.com/james-taplin/dv-rearranged-S282
