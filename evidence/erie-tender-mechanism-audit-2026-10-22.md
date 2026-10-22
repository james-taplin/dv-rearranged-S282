# Erie tender driving-gear audio audit - 22 October 2026

Request: check whether the Erie tender has driving-wheel mechanical clanks,
rather than only its normal rolling tender sound set.

Confirmed from current source and the installed native B99 prefab:

- Native S282 `LocoS282A_Audio/[sim] Engine/ValveGearMechanism/Mechanism_Layered`
  has a `DV.Simulation.Ports.LayeredAudioPortReader` (resources.assets path ID
  366550). Its authored port is `steamEngine.CHUFF_FREQUENCY`, update mode
  SET_VOLUME_AND_PITCH, multiplier 1, absolute input. Its children include
  mechanism-2s, mechanism-4s, mechanism-8s and mechanism-16s. This is separate
  from `CarRollingAudioModule` (rolling, squeal and rail-joint playback).
- `ArticulatedEffects.ChuffBinding` adds chuffs/loops; `CockBinding` adds only
  CylinderCockLayeredPortReader instances. Neither duplicates the normal
  Mechanism_Layered reader for the tender. Tender drive cloning strips other
  MonoBehaviours, and AttachTenderEngine has no separate mechanism-audio bind.
- Therefore 1.22.4 does NOT provide a dedicated mechanical clank layer at the
  tender driving gear. Existing locomotive mechanism sounds can carry back to
  the tender, but that is not spatially local tender mechanism playback.
- `measurements.tender.audioEffects` / `tractive.tender.audioEffects` confirms
  chuff and cylinder-cock bindings only. It is not proof of all audio coverage.

Smallest next fix: reuse the authored native normal Mechanism_Layered subtree,
read the tender engine's chuffFrequencyReadOut, and place it at the tender
running gear with the same owned-reader attach/detach cleanup. Preserve normal
rolling/rail-joint audio. Do not copy low-oil/damaged-mechanism readers blindly:
those need correctly owned oil/damage signals and a separate evidence check.
No reason to change driving physics or create a custom clank generator.

This was a static source/serialized-prefab audit, not a listening test. No code,
build, package or installation changed. Local non-shipping native inspection
outputs are under build/erie-clank-inspection; no game assets are redistributed.
