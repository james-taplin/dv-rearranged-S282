REARRANGED S282 B99 - 2.0.9.3-beta - TENDER AND MALLET REPAIR TEST

Repairs the powered tender's animation setup: configures the exact new mirror
before activation, preventing that live mirror from replacing the tender clock
with the stopped host speed. Tender direction now follows its own signed native
RPM instead of the locomotive's front-engine RPM. See CHANGELOG.txt.

Also repairs a reproduced 2-6-6-2 Mallet (ID 37) collision between the front
crank/main rod and the following engine's cylinder during articulation. A
50 mm increase in engine spacing moves the complete rear assembly together,
retaining its 0.75 scale, native meshes and internal motion. Force calculations
and multiplayer transport are unchanged by these repairs.

Optional multiplayer beta, including experimental independent locomotive-group
wheelslip. Retains the arrangement and startup corrections from 2.0.9.1-beta.
49 selectable S282 arrangements, including Express Mountain, Mallets,
locomotive triplexes and Erie; 44 compatible powered-tender choices.
Install one Rearranged S282 version at a time, using the same mod ID and folder.

REQUIREMENTS
Derail Valley B99 on Windows, Unity Mod Manager 0.27.3 or newer, and
Locomotive Mesh Splitter (tested with 1.1.9).
For multiplayer: everyone needs DV Multiplayer 0.1.16.0-beta and this exact
complete package. Multiplayer is optional for ordinary single-player use.
Multiplayer and its API are not bundled. Please leave Realistic Firing off.

INSTALL AND SETTINGS
Close the game. Back up your personal Settings.xml, remove the old
RearrangedS282 mod folder, then install this ZIP through Unity Mod Manager.
For manual installation extract its single RearrangedS282 folder into Mods.
Restore your own Settings.xml if retaining preferences. Do not mix runtime
DLLs or assets from different versions or install two builds together.

No incompatible visual-settings combination has been confirmed as the cause
of these reports. Keep the original settings for diagnosis. After changing
preferences, stop, close the regulator and REAPPLY CURRENT ARRANGEMENT on the
highlighted S282. During multiplayer the host's effective per-car preferences
can differ from a guest's personal settings. See UMM_OPTIONS.md.

RADIO AND HOST CONTROLS
Select Wheel Rearranger on your own radio and point at a normal S282. Stop,
close the regulator and allow the drive to settle before choosing a layout.
Everyone can request arrangements. The host validates and applies accepted
changes, including the relevant arrangement preferences.

Host: Ctrl+F10 > Rearranged S282 > Multiplayer arrangement permissions.
Allow all guests, lock all guests, or block individual players. Blocked
players still receive changes. Locks last for the player's current session.
Only the host edits shared arrangement preferences; reapply a layout to use
changed preferences on that locomotive. Guest personal settings are retained.

Without an active multiplayer session, ordinary single-player controls and
simulation are used, with no multiplayer status or permission controls.

INDEPENDENT WHEELSLIP AND LIMITS
Two- and three-engine locomotive layouts use individual native slip
calculations, RPM, phase-driven wheel and rod animation, and slip effects.
In multiplayer the host calculates traction; guests use replicated native
ports. The native multiplayer mod continues to handle train movement.

The optional powered tender retains pooled/manual force and a native RPM feed
linked to locomotive movement, with its own diameter-adjusted animation clock.
It has no independent tender slip. Cab/global slip indications and extra curve
drag still follow the shared controller. Invalid native contact/calculation or
derailment restores the previous shared traction path; reapply after rerailing.
True compound expansion is not simulated. The giant 2-2-2 remains an unfinished
clearance prototype.

VALIDATION AND REPORTS
This is a repair test beta. Actual native S282 asset/controller playback and
native engine calculation probes passed. The tender clone test exercises
Unity's deferred destruction and both sides through direction changes and
detach/reapply. Structural checks now inspect animation ownership and effective
settings; a stationary structural pass does not prove moving wheels or rods.

The Mallet collision was reproduced at both -15 and +15 degrees. The completed
production-source sweep covered the stock control and all 11 articulated layouts:
112,320 poses and 547 assertions, with no unexpected contacts in the tested scope.
ID 37 retained measured minimum separation of 42.308 mm at the crank bearing
and 42.736 mm at the main rod. Sampled independent-phase geometry grids also
found no contacts; these are isolated geometry tests, not full-game driving.

The Franklin sweep completed 21,600 controller poses without reproducing a new
collision. Old-gap and old-nose controls reproduced 1,200 and 648 contacts,
respectively, confirming detection of the known overlaps. These are scoped
native tests, not a universal clipping or clearance certificate. Live coupled
vehicle driving, appearance, audio and real host/guest acceptance remain pending.
Use a spare save for initial testing.

For a report, include the version, arrangement, valve-gear and powered-tender
choices, single-player or host/guest role, travel direction, track curvature,
short video and Player.log. Preserve Settings.xml; where structural diagnostics
are available, include their localSettings and effectiveSettings captures.
Online, send logs from the host and affected guest. Optional detailed checks
are included in MULTIPLAYER_TESTING.txt.

All players must use the same complete build. A mismatched or failed replica
is refused or disconnected so it cannot continue with different geometry.

MULTIPLAYER STARTUP TROUBLESHOOTING
Startup failures identify a missing adapter/API or the underlying exception.
This is separate from a host/guest build mismatch. Send that session's
Player.log. Do not copy MultiplayerAPI.dll into this mod; Multiplayer supplies
its own API.

See CHANGELOG.txt, CREDITS.txt, THIRD_PARTY_NOTICES.txt and licenses/.
Source: https://github.com/james-taplin/dv-rearranged-S282
