# S282 demonstrator and owned-locomotive save audit — 4 November 2026

Investigation against committed 2.0.9.4 source `c9c5d88`, with three focused
agents. Read the current handover from line 1 first. Installed Rearranged S282
remains 2.0.9.3-beta. No production changes, feature enabling, production builds,
installation, packaging, game launch or save mutation were performed.

## Conclusion

The native identity and persistence paths support a bounded extension to the
restored demonstrator. Its geometry does not require a new implementation or
dependency. However, lifting `uniqueCar` restrictions alone is insufficient:
simulation initialization, selection, save ownership, random spawning and
cleanup currently share one predicate. There is also a reproduced arrangement-
loss mechanism involving a late LocoOwnership flag change. No evidence here
establishes whole-save corruption or certifies gameplay stability.

Ordinary locomotives purchased through installed **LocoOwnership 1.7.0** are a
different case. Purchase preserves their existing livery, GUID, unique flag and
simulation, so a canonical non-unique purchased S282 already passes the current
Rearranged guard. Native demonstrator/garage vehicles are unique cars.

## Confirmed native compatibility

Installed resource/scene pointers prove that the S282 restoration module uses
the exact canonical `LocoS282A` livery object (resources path ID 297027) and
prefab (9539). Its body, drivetrain, SimController, graph and equipment match
ordinary S282s by prefab identity. `LocoS282B` is the separate tender. There is
no separate native demonstrator livery requiring a whitelist entry. The old
managed fixture's `LocoS282A_Demonstrator` name is a synthetic exclusion input,
not a discovered native identity.

Installed LMS 1.1.9 already attaches its normal S282 loader by parent type
`LocoS282A`, including unique cars. The restoration controller changes native
paint, damage, depleted resources, boiler requests and permissions after spawn;
this static compatibility finding does not authorize conversions during
incomplete restoration.

Native `CarsSaveManager.GetCarSaveData` saves the original type, ID, GUID and
unique flag plus state. Edited runtime geometry is not saved as a replacement
prefab. `CarSpawner` caches deleted unique cars' full state by native livery,
reserves their ID and restores the same ID/GUID and state on recall. Garage
references remain native. `LocoRestorationController` separately saves its stage
and locomotive/tender GUIDs and resolves those GUIDs during load. These
relationships must remain intact.

## Current mod lifecycle and concrete risk

`S282Target.IsNormal` rejects both `uniqueCar` and the early unique-spawn scope,
then requires reference equality with the canonical native S282 livery.
That guard controls radio selection, graph/oiling preparation before
`SimController.Initialize`, arrangement save/load ownership, pooling and
multiplayer identity. Articulated simulation explicitly refuses an instance
that missed its pre-initialization preparation; a later UI eligibility change
cannot supply the missing native graph.

The active arrangement extension is
`simCarState.RearrangedS282 = { version: 1, arrangement: id }`. Pending choices
and unknown future data are retained while a car is owned by that lifecycle.
No new save format, global arrangement catalogue or replacement vehicle ID is
needed for this extension.

Installed LocoOwnership's normal purchase does not change `uniqueCar`.
However, `ValidateOwnedCars`, invoked at world-loading completion, explicitly
sets its tracked cars' `uniqueCar=false`. It does not rebuild simulation or
replay the native arrangement restore hook. For a car loaded with unique=true:

1. Rearranged skips graph/oiling setup and its arrangement-load hook.
2. LocoOwnership later makes it eligible by clearing the unique flag.
3. A fresh native save now attaches a default arrangement owner, writing stock
   ID 10 instead of the previously unconsumed arrangement. Articulated setup
   remains absent for that already initialized instance.

A focused managed reproduction using the actual current ownership/save-hook
bodies confirms that ordering: an input arrangement 34 is skipped while unique,
then the next fresh serialized output contains 10 after the flag clears. The
input object is unchanged. This is a reproduced lifecycle mechanism with managed
TrainCar/coroutine stand-ins, not an observed DV session failure.

Read-only inspection of the JSON snapshot beside a historical career save
(`01633_2026-10-17_21_23_17`) found two native type-20 S282 records with unique=true;
one GUID is in LocoOwnership's seven ownership records. The restored demonstrator
is a different GUID, at stage S10, with its tender record present. Thus the flag
combination is real historical input. This does not establish current state or
past arrangement loss. The binary save was not decoded or changed. The latest
examined free-roam snapshot has an empty LocoOwnership tracker.

Changing eligibility live would also affect saving and pool cleanup while
existing graph/oiling owners continue to exist. Existing owners' save/cleanup
rights must not depend on permission for a new conversion.

## Removing/disabling the mod is not a lossless round trip

Native `SimCarStateSave` reads only its known state fields and creates a fresh
object when saving. An unknown arrangement sibling is ignored and disappears
on the next active-car save without the mod's serializer. In contrast, a unique
car's untouched dormant cache can retain that sibling until recalled.

Native `SimulationFlow` loads only components in its current graph; extra mod
component IDs are ignored and omitted on subsequent native saves. Existing
native resources, pressures, damage and wear remain ordinary numerical state.
Native `ManualOilingPoints` restores oil values only when the array count exactly
matches its runtime points. Therefore a 24-point modded save loaded into six
native points skips that oil-array restore. Do not promise exact oil-state or
arrangement retention across disabling the feature/mod.

## Smallest proposed implementation

- Retain exact canonical-livery identity. Permit necessary per-instance graph
  and oil preparation before native initialization for the unique S282 too.
  Do not swap liveries, change GUIDs or edit shared prefabs.
- Separate preparation and existing save/cleanup ownership from permission for
  a new manual conversion. Handle historical owned cars eligible only after
  LocoOwnership validation without dropping pending saved intent.
- Leave unique vehicles out of random arrangement assignment. Existing saved
  choices should restore; otherwise they start stock until manually selected.
- For the native restoration locomotive, use its existing restoration controller
  and identity to admit manual changes after service completion (native S9/S10),
  retaining the current stationary, repaired, railed and closed-regulator gates.
  Do not rewrite restoration progress or garage/tender references.
- Apply the same eligibility and saved identity on multiplayer host and guests.
  No new dependency or custom persistence service is justified by this audit.

This is a proposed code change, not implemented in 2.0.9.4 by this investigation.

## Evidence and bounded acceptance

Existing baselines pass: **65** spawn/save ownership checks and **1,495** actual
selection/identity/save-policy checks. The narrow late-flag reproduction passes
**68** checks (the original 65 plus three assertions establishing the problem).
These baseline tests intentionally exclude unique cars and do not certify the
proposed admission policy.

Private evidence is under `build/owned-save-audit-20261104`, including native
prefab proof, exact installed ownership decompilations, the late-flag fixture,
source hashes and a sanitized existing-save summary. Native save/garage sources
are under `build/unique-save-audit`. No native assets or personal save payloads
are added to the repository.

Before enabling: extend only these fixtures for unique pre-initialization,
manual-only default stock, same-GUID save/restore, late ownership validation,
native unique cache/garage recall and pool cleanup. Include the original normal
S282 as a control. Run the smallest applicable native-component save checks.

James's later game acceptance should use his test save: completed demonstrator
conversion, two save/reload cycles, garage storage/recall, unchanged restoration
stage and locomotive/tender identity, repair and coupling, switching back to
stock, and a normal S282 purchase/sale through LocoOwnership. Check relevant
multiplayer join/reload paths separately if online use is required. No DV run,
profile selection or save edit was performed by the agents.
