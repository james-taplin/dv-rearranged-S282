REARRANGED S282 B99 - 2.0.9-beta.1

Hotfix for multi-engine arrangements reverting to stock: removes the late-load
ECS component error and corrects duplicate RPM connections. See CHANGELOG.txt.

Optional multiplayer beta, including experimental independent wheelslip.
Based on the complete 2.0.8-slip-test build selected for this release.

49 selectable S282 arrangements, including the Express Mountain, Mallets,
locomotive triplexes and the Erie powered tender. Includes the latest fixes
from that base for oiling, quartering, spawn scope, steam pipes, tender
presentation and Franklin linkage. This is an alternative Rearranged S282
build: install one version at a time, using the same mod ID and folder.

REQUIREMENTS
Derail Valley B99 on Windows, Unity Mod Manager 0.27.3 or newer, and
Locomotive Mesh Splitter (tested with 1.1.9).
For multiplayer: everyone needs DV Multiplayer 0.1.16.0-beta and this exact
complete package. Multiplayer is optional for ordinary single-player use.
Multiplayer and its API are not bundled. Please leave Realistic Firing off.

INSTALL
Close the game. Remove the old RearrangedS282 mod folder, then install this
ZIP through Unity Mod Manager. For manual installation extract its single
RearrangedS282 folder into the game's Mods folder. Do not install alongside
another Rearranged S282 build or mix DLLs and assets from different versions.
If keeping personal settings, retain your own Settings.xml before replacing
the folder. Do not copy other runtime files from the old installation.

RADIO AND HOST CONTROLS
Select Wheel Rearranger on your own radio and point at a normal S282. Stop,
close the regulator and allow the drive to settle before choosing a layout.
Everyone can request arrangements. The host validates and applies each
accepted change, including the relevant arrangement preferences.

Host: Ctrl+F10 > Rearranged S282 > Multiplayer arrangement permissions.
Allow all guests, lock all guests, or block individual players. Blocked
players still receive changes. Locks last for the player's current session.
Only the host edits shared arrangement preferences; reapply a layout to use
changed preferences on that locomotive. Guest personal settings are retained.

Without an active multiplayer session, ordinary single-player controls and
simulation are used, with no multiplayer status or permission controls.

INDEPENDENT WHEELSLIP
Two- and three-engine locomotive layouts use individual native slip
calculations, RPM, phase-driven wheel and rod animation, and slip effects.
In multiplayer the host calculates traction; guests use replicated native
ports. The native multiplayer mod continues to handle train movement.

The optional powered tender still uses the earlier pooled/manual force and
locomotive-linked RPM treatment. It has no independent tender slip. Cab/global
slip indications and extra curve drag still follow the shared controller.
An invalid native contact/calculation or derailment restores the previous
shared traction path; reapply after rerailing. True compound expansion is
not simulated. The giant 2-2-2 remains an unfinished clearance prototype.

TESTING AND REPORTS
This is an experimental beta. Code, package and native calculation fixtures
were checked locally; real host/client driving, visuals, audio and lifecycle
still need testing by players. Use a spare save for initial testing.

Have fun with the radios, host locks, different arrangements and a late join.
Compare wheels, rods, slip, tender effects and driving between PCs. If
something breaks, send the layout, what happened and Player.log from the
host and affected guest. MULTIPLAYER_TESTING.txt has optional detailed checks.

All players must use the same complete build. A mismatched or failed replica
is refused or disconnected so it cannot continue with different geometry.

See CHANGELOG.txt, CREDITS.txt, THIRD_PARTY_NOTICES.txt and licenses/.
Source: https://github.com/james-taplin/dv-rearranged-S282
