# S282 regression diagnosis and repair plan — 31 October 2026

## Scope and current decision

James reports stationary powered tender wheels in 2.0.9.2-beta and continuing
multiplex crank/piston clipping, believed to concern Franklin gear. The supplied
Erie ID43 setup log confirms attachment and geometry construction, not moving
wheel pose. Exact clipping arrangement, colliding parts, curve and session role
remain unknown. The reporter's listed mods do not establish a conflict.

Gameplay investigation remains read-only. James subsequently authorised
structural-test improvements and an agent review. Three agents independently
audited tender/settings, Franklin geometry and wheelslip integration. Only
diagnostic queries, structural validators and their regression fixtures were
changed. No gameplay repair, settings reset, installation or release is made.

Controls: 2.0.7-beta source checkpoint `510a81c` and its preserved binary; the
post-linkage/pre-slip snapshot `75b8b4b`; independent integration `6fc71c3`;
2.0.9.1 `163d081`; tender candidate `72487aa`. The delivered portable archive
was renamed to `Rearranged S282 B99 Multi 2.0.9.2-beta.zip` in its Desktop test
directory. Its hash remains
`DD61D4EE0161DC08FF789E76162D3D5F2B3B67244C5EB80F66C135942161C28E`.
Its DLL was inspected directly from the archive. Local installed mod/logs still
describe 2.0.8-slip-test and are not the reporter's failing run.

## Findings

1. **High — confirmed unsafe lookup; likely surviving tender-freeze cause.**
   `CloneEngineDrive` clones a source already containing a mirror, disables and
   schedules copied behaviours for destruction, then adds a replacement mirror.
   Attachment immediately calls `GetComponent<MalletAnimatorMirror>()` to set
   `CopySpeed=false`. That lookup can select a retiring inherited component
   instead of the exact newly created one. The active mirror then retains its
   default `CopySpeed=true` and copies the phase-owned host's zero speed.
   Unity defers destruction and does not define GetComponent ordering among
   duplicate components. The packaged DLL contains this precise sequence.
   Full actual template-chain reproduction is still needed before attributing
   the reporter's entire failure to this mechanism.

2. **Medium — confirmed mixed tender direction source.**
   TenderDriveAnimation takes unsigned frequency from MiddleEngine but direction
   from MainEngine. IndependentDrive leases the front engine's RPM while the
   tender retains shared native RPM with its radius adapter. Counter-steam and
   recovery can produce opposite signs. This can give wrong tender direction;
   it does not by itself explain a total freeze. The original slip plan warned
   against this exact mixture.

3. **High — confirmed integrated-acceptance gap.**
   ConfigureIndependentDrive activates every articulated arrangement, with no
   normal user opt-out. Existing passing native-job and synthetic-controller
   fixtures do not exercise the complete S282/Franklin hierarchy, vehicle force,
   finite-boiler slip response and full lifecycle. Those gates were explicitly
   still pending when the trial became the base for subsequent packages.

4. **High — Franklin clipping remains unresolved; previous correction was narrow.**
   Only the rear 4-4-4-4/ID32 casting receives the 12 cm nose correction. The
   prior sweep checks a front crank against the following static casting at
   synthetic yaw; the separate crank/main-rod check samples eight phases.
   Neither certifies all crank/piston/crosshead contacts on IDs31/32/38/40/47.
   Articulated, triplex, Big Mallet, casing, retarget and calibration sources are
   unchanged between 75b8b4b and 72487aa: the prior geometry fix was not lost.
   If different engines' moving parts collide, independent phases add cases;
   another engine's phase cannot explain contact with a static casting.

5. **Medium — confirmed structural-report regression, now corrected in diagnostics.**
   Phase ownership removes active mechanisms from the native speed-writer list.
   Quartering and Franklin reports were incorrectly using that list as their
   mechanism inventory. The new regression fails the old implementation when
   the native registrations are leased away. Diagnostics now inspect the lease's
   original inventory without restoring any writer. A native fallback also must
   not count as a successful independent-slip check.

The native adhesion/wheelslip jobs and native steam engines remain in use;
RPM restoration and force ownership protections exist. No evidence justifies
calling the native equations corrupted, removing multiplayer, or replacing
the simulation. Pooled cab/legacy RPM/curve consumers and manual tender grip
remain declared limitations, and can disagree with per-group presentation.

## Settings audit

No incompatible combination of supported UMM visual settings has been
established. Settings.cs and PoweredTenderPolicy.cs are unchanged against the
2.0.7-beta source checkpoint. Visibility/polish controls mainly select rigid
Walschaerts presentation; Franklin selection is explicit in the articulated
path. Powered-tender policy excludes the three-locomotive-engine conflict.
Old dimension sliders are internal/XML-ignored rather than imported as new
geometry. Turning a tender option off removes its engine; it does not explain
the supplied successful attachment with stationary visible drivers.

Settings can expose inherited renderer/template/controller state, and edits
requiring reapplication can differ from the currently built presentation.
Multiplayer can use the host's per-car profile. Preserve the settings; capture
local and effective profiles, the preceding arrangement and reapply sequence.
Use defaults, all optional gear visible, all hidden and relevant toggles as
controlled test inputs. A valid setting exposing a stale-state defect is a
mod defect, not a reason to require users to delete their configuration.

## Structural-test improvements implemented

- Quartering and Franklin inventory survives native speed-registration leases.
- Record controller/state, Animator speed, SpeedMultiplier, phase ownership,
  active mirror count, copying mirrors and bound/enabled tender clocks.
- Reject an inactive configured mirror concealing an active copying mirror,
  duplicate active mirrors, absent clock or unowned frozen Animator.
- Require all expected independent mechanisms and completed/active independent
  state on articulated cases; fallback does not prove independent-slip success.
- Reject quartered clocks with no active wheel transforms.
- Capture local/effective settings and flag motion/clearance as unverified.
  Passing stationary structure never certifies rotating-wheel or collision tests.

These diagnostics may expose current gameplay faults; they do not repair them.
The actual game structural coroutine and the full visual-settings matrix have
not yet run with these changes.

Validation: the new lease-inventory regression fails the old diagnostic code.
Corrected managed fixtures pass 1,519 quartering/ownership checks plus 190
Franklin-report checks, and 2,305 structural pass/fail checks over the existing
93 cases. These counts describe fixture coverage, not 93 live game tests.
The native-reference Stage3 diagnostic build compiles with zero warnings/errors.
Evidence is under ignored `build/diagnostic-audit-20261031/`. The independent
review identified the fallback loophole and it was closed before final checks.

## Repair order and exit criteria

1. **Reproduce the actual tender clone chain.** Use the real native drivetrain,
   production source/template/reclone operations and deferred destruction in
   Unity. Inspect both sides immediately and after end-of-frame. A regression
   must fail the old sequence, including reapply, detach/reattach and repeated
   arrangement changes. Check actual wheel transforms, not only parameters.
2. **Make the smallest tender repair.** Configure the exact mirror returned by
   AddComponent within the clone helper, through an explicit option/callback or
   returned reference. Do not introduce DestroyImmediate to conceal ambiguity.
   Then source signed speed/phase from the tender's own engine, preserving the
   current shared tender physics. Validate reverse and opposing sign transitions.
3. **A/B wheelslip ownership on one complete locomotive.** Compare the preserved
   shared native path with current independent ownership on identical actual
   assets/settings/steam state. Keep multiplayer integration intact. Gate wider
   promotion on rendered mechanism phase, force/grip conservation, one/both-group
   slip and recovery, braking/sanding, finite-boiler demand and exact restoration.
   If shared drive works and independent drive fails, repair that boundary before
   adding another geometry workaround. A safe shared-path control is available;
   a blanket rollback is not yet an evidenced decision.
4. **Identify and isolate the Franklin collision.** Cover IDs31/32/38/40/47,
   both sides, actual controllers, retarget proxies and constraint poses. Record
   the two colliding transforms and engine phases. Separate pin attachment,
   own piston/crosshead/casting, adjacent static casting and inter-engine moving
   contact. Sweep complete revolutions and measured curve poses; include
   independent phase differences only where both colliding parts move.
   Repair timing/binding if only the phase path fails; alter geometry only after
   a measured overlap reproduces on a coherent mechanism.
5. **Acceptance before another release.** Run improved stationary diagnostics,
   real moving forward/reverse/zero-crossing checks, settings variants and
   sequence tests, near/far LOD, unload/pool, save/reload and tender lifecycle.
   Recheck host/guest authority after shared gameplay passes. Package a complete
   candidate only after the actual reported symptoms have been retested.

Unity contracts consulted: [Destroy](https://docs.unity3d.com/2019.4/Documentation/ScriptReference/Object.Destroy.html)
and [GetComponent](https://docs.unity3d.com/2019.4/Documentation/ScriptReference/GameObject.GetComponent.html).
