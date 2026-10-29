# Multiplayer 2.0.9-beta integration

James selected the exact **2.0.8-slip-test** source, including independent
wheelslip, as the new base. Immutable source checkpoint:
`31389e811d0b99fb68b238174fd59b9264f389ef`. Its inherited project/default Info
version still says 2.0.7-beta; the verified trial was built with a version
override. Use the source/build receipt, not the folder label, to identify it.

Work only on `codex/s282-multiplayer`. The normal and independent-slip
checkouts, indices, installations and branch refs remain untouched.

## Contract

- Keep all normal-line fixes and additions in the selected source: exact native
  S282 scope, 49 layouts, 24 owned oiling points and migrations, quartering,
  cup/pipe/tender/Franklin improvements, exporter completeness and independent
  locomotive-group traction/RPM/phase/effects.
- Every player uses their own radio; the host validates stationary requests.
- Host UMM controls allow all guests or lock specific players out of changes.
  Server-side permissions and request limits apply even to direct packets.
- Native Multiplayer 0.1.16.0 and its supported API own transport, movement and
  port replication. The supplied archive is SHA-256
  `D63E9508E6E8607930EE05E6E1EDC77F0E4695B1CAFA740EAE946AEE38C3406D`.
- Independent native ECS calculations and force writes run on the host only.
  Guests consume stable native flow ports for group slip/RPM/phase, retaining
  the existing animation and effects readers. Do not add another movement
  transport or run a competing guest traction simulation.
- Without active Multiplayer, load no API/adapter, show no multiplayer status
  or permission controls, and retain ordinary single-player behaviour. Native
  Multiplayer's internal single-player server is also treated as offline.
- Build exactly **2.0.9-beta** as a complete UMM ZIP and provide Nexus copy,
  plain README, credits, licences, source/asset hashes and validation receipts.

## Execution

1. Restore the selected runtime, regression fixtures and validation scripts
   into the multiplayer checkout. Preserve the existing core/optional adapter.
2. Reapply only the required multiplayer hooks around the complete new base.
   Guard simulation, repair and oiling writes; preserve native replica ports.
3. Correct retained-settings interoperability, failed-init fallback, native SP
   detection, permission enforcement and bounded revision/epoch snapshots.
4. Add stable native replication outputs before graph initialization; use
   those ports for independent-group presentation on guests without ECS jobs.
5. Run inclusion comparisons, targeted regressions, native calculation/phase
   fixtures where available, compilation and complete package integrity checks.
   Distinguish synthetic/native fixtures from actual multiplayer sessions.
6. Commit the coherent result, package from the recorded source commit, and
   keep the multiplayer branch separate. Do not install, upload to Nexus or
   claim live multiplayer acceptance without such evidence.

The selected trial's previous limits remain explicit: powered-tender slip is
still pooled/manual, and live rail-force, audio/visual and multi-peer acceptance
has not been established. Reuse the actual tested native framework and its
documented rollback rather than claiming those limits are solved by networking.
