# Rearranged S282 2.0.9.3-beta - tender and Mallet repair test options

Open **Ctrl+F10 > Rearranged S282**. Host session permissions allow or block guest arrangement requests. Blocked guests still receive updates. Permissions reset when the player leaves. Only the host edits shared arrangement preferences online.

Stop, close the regulator and highlight the S282 with Wheel Rearranger. **REAPPLY CURRENT ARRANGEMENT** applies changed appearance/tender preferences to that locomotive. **RESET WHEEL-ARRANGER RADIO SELECTION** clears the local radio selection. Each player's radio operates independently.

Preserve your personal **Settings.xml** before replacing the mod folder. No incompatible visual-settings combination has been confirmed as the cause of stationary tender wheels or the Franklin report. Keep the original profile for diagnosis instead of assuming a reset repairs the issue. Reapply changes before comparing behavior.

Online, the locomotive uses the host's **effective per-car profile**, which may differ from a guest's saved local preferences. For a reproducible report, retain Settings.xml and capture the arrangement, valve gear, powered-tender choice and local/effective profiles. Where the structural diagnostic is available, its report includes **localSettings** and per-car **effectiveSettings**; include that report and Player.log from both host and affected guest.

Powered-tender options cover **44 compatible layouts**, with Erie enabled by default. A regular S282 tender must be coupled behind the locomotive. Three-engine locomotive triplexes, unpowered 4-0-4 and the unfinished giant 2-2-2 are excluded. Untick Erie and reapply to use an ordinary tender.

Valve-gear, trailing-truck, curve drag/squeal, big-end oilcup and automatic oiling choices retain their behavior. Reapply appearance changes. Automatic oiling is off by default; the host writes native oiling state online. Independent tender slip is not implemented.

Random spawning is off by default. Choose layouts for its pool; saved choices remain with their locomotives and an empty pool falls back to stock. The host chooses online spawns; guests wait for the accepted layout.

The selected locomotive shows independent-drive status. **RECORD GROUP SLIP DIAGNOSTICS** writes independent-slip.jsonl locally at four samples per second. Stop recording after reproducing a problem. The artificial grip test and structural tests are available only outside multiplayer sessions.

The ID 37 Mallet repair changes the spacing between complete engines; it does not require a new preference or a settings reset. Its 0.75 scale and internal native mechanism are preserved. Force calculations and multiplayer transport are unchanged by the tender/Mallet repairs.

Structural diagnostics inspect static inventory, poses, ownership and readiness. A pass does not prove wheel/rod motion, driving physics or multiplayer delivery. Native asset/calculation probes passed separately. The completed production-source sweep covered the stock control and all 11 articulated layouts: 112,320 poses, 547 assertions and no unexpected contacts in the tested scope, with at least 42.308 mm measured ID 37 separation. Sampled independent-phase geometry grids found no contacts; they are isolated geometry tests, not full-game driving. The Franklin sweep completed 21,600 controller poses without reproducing a new collision. Old-gap/old-nose controls reproduced 1,200/648 contacts. These scoped tests are not a universal clipping or clearance certificate. Live coupled-vehicle and real host/guest acceptance remain pending.

Without an active online session, ordinary single-player preferences and controls apply. Multiplayer permissions/status stay hidden.
