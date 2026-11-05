# Rearranged S282 2.0.9.8 final beta — options

Open **Ctrl+F10 > Rearranged S282**. Stop the locomotive, close the regulator and
highlight it with Wheel Rearranger before changing its layout. **REAPPLY CURRENT
ARRANGEMENT** applies changed appearance/tender preferences to that locomotive.
**RESET WHEEL-ARRANGER RADIO SELECTION** clears the local radio selection.
Each player's radio operates independently. Preserve your personal Settings.xml
when updating; no settings reset is required by this release.

## Appearance and running gear

Trailing-truck styles, valve-gear visibility and the polished Pacific/Hudson
gear option retain their existing behavior. Reapply appearance changes. The
restored reach/lubricator links follow the effective valve-gear visibility.
Driver dimensions and accepted arrangement geometry remain authored values.

## Powered tender

There are **45 compatible arrangements**, including Scotch Glider. Couple a
regular S282 tender behind the locomotive and enable the desired arrangement's
option, then reapply. Erie alone is enabled by default. Little Boy, three-engine
locomotive triplexes, unpowered 4-0-4 and the giant 2-2-2 prototype are excluded.
Untick Erie and reapply to use an ordinary tender. A preference change alone does
not change already-applied geometry. Independent tender slip is not implemented.

## Oiling and curves

Big-end oiling is on by default and adds cups to driven crank pins while retaining
the original guide/crosshead/main-rod cups. Reapply to update this preference.
Automatic filling and lid closing is off by default and takes effect immediately.
Online, the host owns simulation changes. Curve drag and flange squeal retain
their existing optional settings.

## Random spawns

Random arrangements are off by default. Choose layouts for the enabled pool;
an empty pool falls back to stock and saved manual choices remain with their
locomotives. Scotch Glider and Little Boy are both off in the pool by default.
The host chooses online spawns; guests wait for the accepted state.

## Multiplayer

Host permissions allow/block guest requests. Blocked guests continue to receive
updates; permissions reset when they leave. Online, only the host edits shared
preferences and each car uses its effective host profile, which may differ from
a guest's personal settings. Reapply to use changed preferences on a car.
Without an active online session, multiplayer permissions/status remain hidden.

## Diagnostics

Independent-drive status and **RECORD GROUP SLIP DIAGNOSTICS** are available for
reports; recording writes independent-slip.jsonl locally. Stop after reproducing
the issue. The artificial grip test and structural sweep are single-player
diagnostics, not online gameplay controls.

The structural sweep needs two stationary stock S282s with exterior interactions
loaded, including one with its regular tender. Keep them close together, note
the chosen IDs and close UMM so simulation can tick. Stay nearby throughout.
Missing interactions cause a clear refusal or abort; an aborted run cannot pass.
The full sweep has 96 main cases and six return cases. The installed 2.0.9.8
passed them all, with stock restored and tender preferences preserved.

Structural success is stationary evidence. Motion, clearances, audio, sustained
driving and live host/guest acceptance remain separate. Reports should include
Player.log, Settings.xml and the structural JSON's local/effective profiles when
available; online, send the host and affected guest logs.
