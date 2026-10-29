Rearranged S282 B99 2.0.8-alpha
Experimental multiplayer alpha

This is the separate multiplayer development build of Rearranged S282. It lets
players use their own wheel-arranger radios while the host validates changes
and sends the accepted arrangement and its settings to everyone.

This alpha has compilation and offline checks. Live multiplayer physics,
visuals, audio, late joins and reconnection still need player testing. It uses
the existing multiplayer implementation and 2.0-beta arrangement baseline.
Later single-player and independent-wheelslip development is not included.

Installation

Requires Derail Valley B99 on Windows, Unity Mod Manager 0.27.3 or newer, and
Locomotive Mesh Splitter. Mesh Splitter 1.1.9 is the reference version.

For multiplayer, everyone needs Derail Valley Multiplayer 0.1.16.0-beta and the
identical 2.0.8-alpha ZIP, including both RearrangedS282 DLLs. The Multiplayer
mod supplies its own API. Do not install MultiplayerAPI.dll separately into
this mod. Follow Multiplayer's own installation instructions for its needs.

Close the game. Replace the existing RearrangedS282 folder with this package
through UMM or extract the RearrangedS282 folder into the game's Mods folder.
Retain your personal Settings.xml if wanted when replacing the old folder.
Use one Rearranged S282 build at a time. This is an optional alpha download
and does not install alongside the normal version as a second mod.

Without Multiplayer installed or active, the multiplayer adapter is not loaded
and normal single-player operation remains available. Custom Car Loader and
MultiCylinderChuffFix are not required. Leave Realistic Firing off for this alpha.

Using the radio

Choose Wheel Rearranger on your radio and select an S282. Stop the locomotive,
close its regulator and let driving force and wheel slip settle before changing
arrangements. Each player can request a change using their own radio. The host
checks the request, applies it and shares the result. Guests do not change their
local locomotive first while waiting for approval.

Arrangements and settings

The retained catalogue has 48 layouts, including rigid locomotives, Mallets and
triplexes, with optional Erie powered tenders for 43 compatible arrangements.
The giant 2-2-2 remains unfinished; Drivers not found is deliberately unpowered.
True series compound expansion and independent powered-tender slip are not
implemented in this branch.

The host's per-car arrangement settings are shared with guests. Host option
changes apply when that locomotive is changed or reapplied. Highlight the
stationary locomotive, open UMM and use REAPPLY CURRENT ARRANGEMENT. Guests'
personal saved settings are not replaced by the host's settings.

Host controls

Open Ctrl+F10, then Rearranged S282. Under multiplayer arrangement permissions,
Allow guests to change arrangements allows or locks all guests. Each connected
matching player also has an individual may change arrangements toggle. Locked
players continue receiving arrangements and can still drive normally.

Permissions last for the session and clear when a player disconnects. The host
always retains arrangement control. All peers must use this exact package;
mismatched builds or ready guests that do not confirm a match are disconnected.
If a guest cannot apply the host's state, the error is reported and the guest
is disconnected rather than knowingly retaining a different arrangement.

Having a play

Try your radios, swap a few arrangements safely, drive around together and see
whether everyone sees the same running gear and effects. Joining an already
running session is useful to try. There is no need to work through a large
checklist; have fun and report anything that breaks or looks wrong.

If reporting a problem, include the package version, host or guest role,
arrangement, powered-tender choice and what happened. Screenshots and Player.log
from the host and affected guest are useful. More detail is in
MULTIPLAYER_TESTING.txt if you want it. Structural tests are single-player only.

Credits and licensing

See CREDITS.txt, LICENSE, THIRD_PARTY_NOTICES.txt and licenses/. Original work
and bundled exporter assets and licences are retained. Multiplayer itself and
its API are not bundled. Source for this branch:
https://github.com/james-taplin/dv-rearranged-S282/tree/codex/s282-multiplayer
