# 2.0.9-beta release validation

James selected the complete **2.0.8-slip-test**, including experimental
independent wheelslip, as the base. Source checkpoint:
`31389e811d0b99fb68b238174fd59b9264f389ef`. This supersedes the older multiplayer
alpha's gameplay baseline. The release stays on `codex/s282-multiplayer`.

The supplied DV Multiplayer 0.1.16.0-beta ZIP was inspected without installing
it. Its API is 1.1.0.0 and matches the official release. The archive SHA256 is
`D63E9508E6E8607930EE05E6E1EDC77F0E4695B1CAFA740EAE946AEE38C3406D`.
The API is a build-only reference and is excluded from the package.

## Evidence

| Check | Result and scope |
|---|---|
| Main and optional adapter | Release/net48 against installed B99 references and supplied API; final build receipt records exact source and DLL hashes. |
| Bridge and codec | 213 checks / 25 cases on actual production bridge/packet code with managed API boundaries: bounded decoding, native internal single-player, ready-stage handshake, current-instance callbacks, direct permission enforcement, rate limits, snapshot recovery, failures and disconnect/ID reuse. |
| Core policy | 130 checks / 16 cases on actual production core/settings/catalogue: 49 choices, newer/future preferences, epochs/revisions, 5,120 historical deletions, fail-closed state/readiness errors, silent optional fallback, native deletion lifecycle, runtime asset identity. |
| Radio/settings | 346 checks / 23 cases on actual radio/Main/settings bodies with native boundary stand-ins; independent radios, guest requests, locks, reapply and Express48 retention. |
| Owned oiling and save migration | 176 checks with actual native DV.Simulation oiling/save implementations; foreign arrays remain unchanged, known six/20-point saves migrate to 24, existing 24-point values survive, guest writes remain disabled even with automatic assistance enabled. Train identity/Harmony are stand-ins. |
| Native graph and engine | Actual production graph/lifecycle and auxiliary engine code against native references; authority guards include guest Tick, output clearing and repair callbacks. Exact final counts are in the fixture reports. |
| Native port/group ownership | Actual helper and IndependentDrive compiled into managed boundary fixtures; exact GUID/epoch/revision marker rejects old pooled or prior-session data. Guests create no slip ECS, rewrite no engine RPM inputs and apply no group force. Phase-baseline change regression failed before correction and passes afterward. |
| Unity native slip | **46 checks passed in an isolated Unity 2019.4.40f1 non-development release player**, using actual native ECS/wrapper calculations with synthetic inputs. Editor/player exited successfully. No real rail vehicle forces applied. |
| Unity animation | **13 checks passed** using actual native AnimatorPortReader and synthetic controller/wheel objects, including late-join host offsets. This does not validate actual locomotive clip origins or rendered running gear. |
| Latest-base regressions | Spawn 65; quartering 864 + 180 linkage; Express 28; compact pins 950; Franklin outlets 7,668; tender teardown 2,402; lubrication 59 + 38 lubricator + 4,067 seats + 5,349 retained-pitch checks. These use production bodies and managed hierarchy/pose/lifecycle boundaries. |
| Exporter paths | 44 path/incomplete-install checks. Complete exporter payload and all runtime assets/licences remain byte-identical to the selected seed. |
| Nexus archive | `Validate.MultiplayerPackage.ps1` checks one UMM root, safe paths, version/entry point, both DLL versions, no API dependency in main, native helper inclusion, unchanged seed payload, no personal settings/logs and plain README. Run after final packaging; result records the actual ZIP hash. |

The full scripts and fixture sources are preserved in this branch. Native
reference assemblies, extracted upstream code, synthetic native input data,
raw logs and generated players remain local under ignored `build/`.
Machine-readable run receipts are under `build/audit-20261029/`,
`build/multiplayer-2.0.9-validation/` and the final release folder.

## Corrected integration decisions

Restore the selected runtime before adding multiplayer hooks; old-alpha tests
did not establish inclusion of newer fixes. Use the existing MPAPI for custom
arrangement messages and stable native configurable flow outputs for wheel
state. Host authority covers force, native slip ECS, auxiliary engine Tick,
repair and oiling; replicas consume native values.

Native pooled cars retain their flow and configurable outputs. A matching
revision alone therefore cannot identify a fresh car/session. The same native
marker now carries exact GUID, session epoch, revision and group count. Guest
animation also rebinds when the host's phase baseline changes. No additional
transport or movement simulation was introduced.

Native CarAboutToBeDeleted covers pooled and unpooled removal before Unity
destruction. Core uses that event for profile/tombstone cleanup while retaining
the existing idempotent pool/OnDestroy cleanup. No absent-MP main-menu singleton
lookup is required.

## Acceptance limits

No live Multiplayer session was run on this development PC. Native fixture
success and a clean compilation do not establish multi-PC train motion,
network timing, audio, real model appearance, rail traction, save/reload or
reconnection acceptance. The package is an experimental beta for external
group testing; `MULTIPLAYER_TESTING.txt` gives optional detailed scenarios.

The selected base's limits remain: powered-tender independent slip is absent;
its manual/pooled force and locomotive-linked RPM remain. Cab/global slip and
extra curve policy remain shared. Native failure/derailment restores the prior
shared traction path, requiring reapply after rerailing. True compound expansion,
VR and Realistic Firing compatibility are not established.

Build/package receipts state installed=false and live validation=false. A
branch push does not publish a Nexus file or GitHub release. Normal release
branches and the installed game mod are outside this change.
