Rearranged S282 - 2.0.9.3-beta - tender and Mallet repair test

Repairs tender-wheel animation by configuring the newly added mirror before activation and using the tender engine's own signed native RPM. The clone setup no longer relies on finding a component while inherited mirrors await Unity destruction.

Also repairs a reproduced 2-6-6-2 Mallet (ID 37) collision between the front crank/main rod and the following cylinder at -15 and +15 degrees of articulation. The dedicated engine gap increases from 0.100 m to 0.150 m, moving the complete rear engine/trailing axle back 50 mm and its hinge back 25 mm. Its 0.75 scale, native meshes and internal motion are preserved. Force calculations and multiplayer transport are unchanged by these repairs.

Optional multiplayer beta with experimental independent locomotive-group wheelslip. Retains the 2.0.9.1-beta arrangement and startup corrections: 49 arrangements, including Express Mountain, Mallets, locomotive triplexes and Erie, with 44 compatible powered-tender choices.

Radio and host controls
Use Wheel Rearranger on your own radio to request a layout. Stop, close the regulator and let the drive settle first. The host validates each accepted change and shares the layout and relevant preferences. Ctrl+F10 > Rearranged S282 > Multiplayer arrangement permissions allows the host to permit or block guest requests. Blocked players still receive updates; locks reset when they leave.

Install
Requires Derail Valley B99, Unity Mod Manager 0.27.3+ and Locomotive Mesh Splitter. Online, every player needs DV Multiplayer 0.1.16.0-beta and this exact complete package. Multiplayer and its API are not bundled and are optional for single player. Leave Realistic Firing off for testing. Back up your Settings.xml, remove the previous RearrangedS282 folder and install this ZIP through UMM. Restore personal settings if desired; use one complete build at a time.

Settings and reports
No incompatible visual-settings combination has been confirmed. Preserve settings and reapply appearance/tender changes to the selected stationary locomotive. Online, the host's effective per-car settings may differ from local guest preferences. Include the layout, gear/tender choices, role, video, Player.log and local/effective settings when reporting a problem. UMM_OPTIONS.md explains the controls.

Experimental features and limits
The host calculates locomotive-group traction; guests follow replicated native ports. Powered tenders retain pooled/manual force and a movement-linked native RPM feed with their own diameter-adjusted clock. Independent tender slip and true compound expansion are not implemented. The giant 2-2-2 remains unfinished.

Native S282 asset/controller playback and native engine calculation probes passed. The completed production-source sweep covered the stock control and all 11 articulated layouts: 112,320 poses, 547 assertions and no unexpected contacts in its measured scope. ID 37 retained at least 42.308 mm measured separation. Sampled independent-phase geometry grids found no contacts; they do not simulate full-game driving. The Franklin sweep completed 21,600 controller poses without reproducing a new collision. Old-gap/old-nose controls reproduced 1,200/648 contacts, confirming detection of known overlaps. These scoped tests do not provide a universal clipping or rod-clearance certificate. Structural diagnostics inspect ownership and settings but cannot prove motion. Live coupled-vehicle driving, visuals, audio and real host/guest acceptance remain pending. Use a spare save initially.

Multiplayer startup failures identify missing adapters/APIs or the underlying exception; they do not alone prove a build mismatch. Send Player.log. Do not copy MultiplayerAPI.dll into this mod; Multiplayer supplies its own API.

Original mod by SatiricRug / Peter Schinske; B99 continuation by JimTaplin. Full credits and licences are included.
