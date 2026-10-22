# Compound first implementation slice

Offline only. Nothing here is compiled into RearrangedS282.csproj or installed.
Model adapted from James Taplin's S060 MeyerCompoundCycle (MIT project).
S060 source is read-only, frozen in ignored build output for each comparison;
source-receipt.json records its exact hash. Do not edit that other project.

Run `Validate.CompoundPrototype.ps1` from S282. Uses .NET8 and current local S060
reference source; game/Unity not needed. Outputs build/compound-prototype.

Implemented: conserved finite receiver/chests, HP exhaust-only LP supply,
HP receiver backpressure, separate HP/LP RPM, one or two LP consumers, simultaneous
proportional sharing (no consumer-order advantage), bounded fresh-steam mass
budget, atmospheric discharge/vent/work/heat/storage ledgers. The budget is only
an API input here: it is NOT connected to a native boiler. Steam is ideal vapour
with fixed gamma, no two-phase condensation or calibrated receiver heat transfer.
One common cutoff and one LP calibration/crack flag currently serve LP consumers;
independent LP RPM is supported, full independent LP controls/damage are deferred.

Reference calibration is S060's .42/.594m bores and .6707097m stroke, not S282
balance targets. No HP/LP sizing decisions for any S282 arrangement made yet.
S282 currently uses a larger main pair plus smaller auxiliary pair; these cannot
be assumed to form a credible HP/LP expansion ratio without recalibration. Keep
original-size upper limits; consider reduction of HP rather than enlarged LP.

Validation covers reference equivalence, mass/energy, finite states, supply
budget, unfed spinning stages, reverse, timestep variation, stopped LP, rundown,
consumer permutation and input rejection. These tests establish arithmetic and
conservation, not real steam tables, native simulation integration or hauling.

Pressure-bound correction: the initial S060-derived solver reached16.03871bar
on16bar supply with a stalled LP. The prototype now bounds each outgoing parcel
by end-of-transfer source/recipient pressure equalisation, without discarding
energy. The same bound covers simultaneous receiver-to-LP transfers. Blocked-LP
receiver now settles at15.999862bar. Every bounded scenario checks transfer
pressure excess below.00001Pa as well as conservation.

The constructor's enforceTransferPressure=false exists ONLY for the4000-step
legacy-reference test; all new scenarios use the corrected defaulttrue. This is
not a proposed game setting. The legacy model remains unchanged in S060.
Current102027checks pass; runtime readiness remainsfalse because native binding,
calibration, saves, damage, starting arrangements and performance are unvalidated.

Native gap confirmed locally in B99 ReciprocatingSteamEngine.SimulateMeanTorque:
mean pressure subtracts a literal1bar atmosphere. EXHAUST_PRESSURE is an output,
not a configurable backpressure input. Wiring native engines in series alone
cannot make HP work against a receiver or enforce interstage mass conservation.
Use an owned per-instance stage extension following S060's supported subclass
pattern if/when integration is ready; do not globally replace engine behaviour.

S282 roles to preserve:37/41/42/45/46 rear main HP/front auxiliary LP;43 Erie
rear main HP feeding front loco and tender LP in parallel. LP tender exhaust and
pull stay on tender; only final venting stages feed atmosphere, internal HP flow
must not feed the chimney.31/32 simple duplex and38/40 fictional triplex retain
current simple expansion unless explicitly designed otherwise; stock unchanged.
Next: determine reduction-only dimensions and
receiver volumes from existing calibration, then native instance/save/damage/
consumption integration and comparison. No Verde graph copied: study identified
pressure/quality and missing flow-demand connections; not independently reaudited
in this slice. Existing stage sound tuning is already installed in1.22.3.
