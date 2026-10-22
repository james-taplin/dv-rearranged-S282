1.19.0 preliminary physics acceptance evidence

Validate.PreliminaryPhysics.ps1 uses actual native ReciprocatingSteamEngine
construction and Tick, with detached definitions following the existing native
engine probe. It compiles the production CylinderCalibration, ConventionalPhysics,
TractiveEffortModel and WheelArrangement files. The previous local structural
capture provides measured per-layout driver radii. Native setup helper methods
are extracted from the existing NativeProbe into build-local source.

Checks cover all eligible arrangements at three mass levels, family/cap bounds,
native cached areas and volumes, no capacity increase, auxiliary/main ratios,
the stock reference ratio ceiling, second-car isolation, separate Erie tender
dimensions, exact restoration and native-engine tick/steam-output finiteness.

This is a preliminary DV-feel calibration, not a historical cylinder design or
drawbar power measurement. Native throttle/chest capacity and stroke remain
unchanged. Smaller auxiliary bores are intentional; imposing a universal 0.40m
minimum would break the preserved capacity ratios. Only live power/load tests
can establish usable performance, steam economy, slip onset and high-speed limits.
