# Baseline fixes following the 2.0.9.4 audit

4 November 2026. Starting point `7a6aa1db134d684b62d11c21ec5d0a3aab8e2534`
on `codex/s282-multiplayer`. This records a bounded offline correction, not
gameplay or multiplayer acceptance. No new arrangement IDs were added.

## Corrections and reproductions

- Franklin pin renderers are removed from the native explosion/material-swap
  caches before their deferred destruction. A retired-renderer set prevents
  same-frame include-inactive scans from adding them back. The baseline fixture
  reproduced both cache retention and same-frame rediscovery; the fixed fixture
  passes 15 assertions using production tracker/lease and extracted installed
  native explosion/repair methods with managed Unity stand-ins.
- A guest keeps an exploded car's latest non-stock state pending until both the
  car's explosion flag and native exploded model clear. Time spent exploded
  does not consume the readiness timeout, and other cars continue to sync.
  Both ready-but-exploded failure and the damaged-readiness timeout were
  reproduced against the old core. The fixed fixture tests repair, latest
  revision, other cars and retained fail-closed genuine failure paths.
- When first discovering stock with no recorded applied profile, the host
  publishes the tender actually selected on that car rather than unapplied
  preferences. Old source reproduced the stock mismatch. Tests cover no
  rearranger, an untouched rearranger, both selection/preference directions,
  and explicit reapply without changing personal settings.
- The mesh-only OBJ path now creates no scene objects or materials. Face
  parsing handles EOF without a newline and relative vertex/UV/normal indices,
  rejecting invalid tokens explicitly. The old path reproduced two scene
  objects for one mesh, zeroed negative indices and a bounded EOF hang. The
  fixed fixture passes 21 checks and matches the original positive-index
  importer for multi-submesh geometry/UVs/normals/winding and four existing
  representative OBJ assets. This is an offline mesh fixture, not rendering.

## Validation and scope

The changed production source was compiled once as the core and optional
adapter together, Release/net48, Stage 3, version 2.0.9.4, against the installed
B99 references and Multiplayer API 1.1. Build: zero warnings/errors.

Core SHA-256: `40DD503FFC9409CFF9E608EDB3C8165BD8F884B7F6160B9ACB66277666F47CE2`

Bridge SHA-256: `A0F5F75D471D4E95D8C5A60F7228D2D4C521CB3866A77CAFB0E301E20DD30EF1`

The main assembly has no Multiplayer/API assembly reference; the optional
bridge references MultiplayerAPI 1.1. The internal build is under
`build/baseline-fixes-20261104/integrated/` and is not an installable release.

Affected checks: core multiplayer 150 checks/19 cases, bridge 219/26, radio
346/23, native-port adapter 32, independent-drive authority 27, renderer
lifetime 15, mesh-only importer 21 plus original-source/asset parity. Evidence
is retained under `build/mp-audit-fix/`, `build/renderer-lifetime-baseline/`,
`build/renderer-lifetime-fixed/`, `build/obj-mesh-validation-20261104-final-complete/`
and `build/baseline-fixes-20261104/`. Fixture boundaries and untested native
behavior are described in those tests; passing them does not prove live peers.

The preceding Franklin geometry sweeps (25,920 poses) remain historical
evidence for the old exact core hash `C4AF2E44428DCD74F29E20C80300F8CDDBB473A9A33A822C7C1F3E23482CB07E`.
Pin geometry and native constraint math were not changed here, but those sweeps
were not rerun on the new DLL. The 11 audit-listed stale/unavailable unrelated
fixtures remain coverage gaps and must not be presented as passing.

## James's acceptance and next step

James owns game testing on a separately authorized build. Priorities: Franklin
to stock followed by explosion/repair; an exploded non-stock late join followed
by repair; stock tender parity before and after explicit reapply. Record the
results before treating the baseline as accepted. Installed mod remains
2.0.9.3-beta. No game launch, save mutation, install, release ZIP, push or
publication was performed for this correction.

Only after James accepts the baseline, obtain his exact new arrangement list.
The existing 49 catalogue IDs remain 0–48; append new IDs without renumbering
saved layouts. Reuse the closest working family, validate each requested layout,
and plan one integrated candidate and affected offline checks. Installation and
publication are separate decisions.
