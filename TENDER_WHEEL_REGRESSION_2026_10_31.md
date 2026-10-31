# Powered tender wheel investigation — 31 October 2026

Reported: powered tender wheels stationary in 2.0.9.1-beta. The report does not
identify arrangement, single-player versus multiplayer, or host versus guest.
The local game log is for 2.0.8-slip-test and does not reproduce this report.

The actual 2.0.7-beta binary contains the existing MalletAnimatorMirror Update
speed copy and TenderDriveAnimation LateUpdate speed writer. In the 2.0.9.1
source, GroupCrankAnimation takes ownership of locomotive animators and sets
their speed parameter to zero for native phase-driven animation. The tender is
not part of that independent-slip group. Unity evaluates the target animator
after the mirror Update but before the tender LateUpdate: copying zero every
frame prevents the tender's later speed value from advancing the controller.
The installed game's TractionPortsFeeder still updates the shared RPM independently
of DrivingForce.FixedUpdate; native RSE computes chuff frequency from crank RPM.

In an isolated Unity 2019.4.40f1 player, the production mirror, a synthetic
looping controller and a LateUpdate clock reproduce the zero-speed freeze before
the correction. Opting the tender mirror out of speed copying passes movement
in both directions; ordinary mirrors continue copying speed. This confirms the
timing mechanism in isolation, not the state of the reporter's live game.

Fix: set CopySpeed=false on both tender clones. Preserve the native controller,
tender clock, reverser reader, other mirrored parameters, locomotive-group
ownership, physics, force and networking. MalletAnimatorMirror is in its own
source file so the real component can be linked into the player fixture.

Validation and limitations are summarized in CURRENT_HANDOVER.md. Evidence is
under ignored build/tender-wheel-investigation; the successful isolated player
was reused from the already built after-ecs-fix fixture and the probe DLL was
rebuilt from current source. The full fresh-editor runner initially timed out at
a visible version mismatch dialog. The runner now writes its project version
file without a UTF-8 BOM and includes the actual editor revision; this runner
change has not yet been tested through a fresh editor startup.

For the live check: install the complete test archive on every relevant peer;
select a powered tender with an articulated host, drive forward and reverse,
check both sides and rods, uncouple/re-couple and switch/reapply. Compare a
rigid host and test both host and guest if multiplayer. Preserve logs if any
case remains stationary; do not infer successful live testing from this fixture.
