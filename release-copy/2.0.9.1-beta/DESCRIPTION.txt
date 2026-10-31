Rearranged S282 - 2.0.9.1-beta

Hotfix: corrects multi-engine arrangements reverting to stock when the slip helpers initialize. Also corrects duplicate RPM connections during activation and cleanup.

An optional multiplayer beta based on the full 2.0.8-slip-test build, including experimental independent locomotive-group wheelslip and its latest fixes. 49 selectable arrangements, including Express Mountain, Mallets, locomotive triplexes and the Erie powered tender. 44 compatible powered-tender choices.

Everyone gets a radio
Use Wheel Rearranger on your own radio to request a layout. Stop, close the regulator and let the drive settle first. The host validates the change and shares the layout and relevant preferences with everyone.

Host controls
Ctrl+F10 > Rearranged S282 > Multiplayer arrangement permissions. Allow all guests or block individual players from changing arrangements. Blocked players still receive updates. Locks reset when the player leaves.

Install
Requires Derail Valley B99, Unity Mod Manager 0.27.3+ and Locomotive Mesh Splitter. For multiplayer, every player needs DV Multiplayer 0.1.16.0-beta and this exact complete package. Remove the previous RearrangedS282 folder and install this ZIP through UMM. Use one Rearranged S282 build at a time. Multiplayer is optional for ordinary single-player use. Please leave Realistic Firing off.

Experimental features and limits
The host calculates traction; guests follow native replicated RPM, slip and phase ports. The native multiplayer mod handles train movement. Powered tenders retain the earlier pooled/manual force and locomotive-linked RPM: independent tender slip and true compound expansion are not implemented. The giant 2-2-2 remains unfinished.

Local code, package and native calculation checks have been run. Live multi-PC driving, visuals, audio and lifecycle still need testing. Have fun with it, and if something breaks let me know :) The layout and Player.log from the host and affected guest are helpful. Optional detailed checks are included in MULTIPLAYER_TESTING.txt.

Original mod by SatiricRug / Peter Schinske; B99 continuation by JimTaplin. Full credits and licences are included.


Multiplayer startup troubleshooting
If integration cannot start, the message now identifies a missing adapter/API
or names the underlying load exception. This is separate from a host/guest
build mismatch. Send Player.log from that session so the cause can be diagnosed.
Do not copy MultiplayerAPI.dll into this mod; Multiplayer supplies its own API.
