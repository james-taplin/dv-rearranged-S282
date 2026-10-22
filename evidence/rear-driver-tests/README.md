2-10-10-2 rear added-driver rotation regression

The previous CloneEngineDrive removed every MonoBehaviour, including the
PoweredWheelRotater used by the fifth driver. The native animation drives wheels
1-4; added fifth/sixth wheels copy the fourth wheel in LateUpdate. Preserve these
presentation components and explicitly rebind to the clone's own fourth wheel;
continue removing copied oiling/port-reader scripts.

Validate.RearDriverFollowers.ps1 extracts the actual production cloning method
and compiles it with the actual PoweredWheelRotater. Managed scene/cloning
stand-ins retain native Animator as a Component (it is not a MonoBehaviour),
simulate copied/stale follower references, and verify both sides, 3/4/5-axle
engines, reverse/full revolution phases and unrelated source movement. Native
Animator playback itself remains an in-game acceptance check. The structural
run now separately checks rear fifth-wheel bindings and local rotation gaps.
