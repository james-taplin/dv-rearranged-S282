# Multiplayer startup report and 2.0.9.1-beta

The user quoted the 2.0.9 message "use the complete matching test package".
Its exact source is MultiplayerSync.TryLoadBridge after 15 failed initialization
attempts. It is not emitted by the peer protocol/build-identity handshake. It
therefore confirms an optional adapter startup failure, not a mismatched peer
package. The tester's Player.log was not supplied; a specific original loader
exception remains unconfirmed. Local current logs are not substituted for it.

The loader logs the exception on the first and final attempts. In this release,
the UI also unwraps reflection/type-initializer exceptions and distinguishes a
missing S282 adapter, unavailable MultiplayerAPI, API readiness and other concrete
load exceptions. Text is bounded for the GUI; the full exception stays in the
log. Online uncertain-startup protection remains fail-closed. Missing/inactive
Multiplayer still retains silent ordinary single-player behavior.

A related defect was independently reproduced using the actual Bridge source:
it set initialized before API compatibility registration/runner setup succeeded.
An injected registration failure followed by retry returned a false success;
the old regression failed. Success is now marked only at the end. Failed startup
detaches lifecycle callbacks, destroys its runner and cleans up entered sessions.
The core restores the uncertain-startup lock after such cleanup. Retry tests
cover registration failure, runner failure, one active runner and no duplicated
lifecycle subscriptions. This is a confirmed related correction, not proof of
the tester's unknown original exception.

The actual production core has additional cases for exhausted missing-adapter
retries, absent/disabled recovery, API-not-ready messages, missing API, signature
errors, reflection unwrapping and bounded GUI text. Final targeted tests: bridge
219 checks/26 cases and core 135 checks/16 cases (managed game/API boundaries).
Exact official API/native-reference compilation is run for the release DLLs.
Raw before/after evidence stays in ignored build/loader-report-20261031/.

The public version is exactly 2.0.9.1-beta, as James requested. UMM/informational
version is 2.0.9.1-beta; both numeric assembly versions are 2.0.9.1. Scripts and
archive validation support four numeric components. Earlier archives are retained
but superseded by this package, which includes all previous slip/ECS/RPM fixes.

Physics source is unchanged from the hotfix that passed the real Unity late-load
reproduction (46 native and 13 animation checks). Owned-locomotive restrictions
remain unchanged. The repair tubes/rust report remains separate and unfixed.
No live multiplayer session or real S282 driving acceptance is claimed; no
installed mod, save or Nexus publication is changed by preparing this archive.

To diagnose the original report, obtain Player.log from the affected session,
including the RearrangedS282 integration load exception and Multiplayer version.
Do not infer its cause from the old generic status text alone.
