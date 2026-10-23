# Multiplayer offline checks

Run `Validate.Multiplayer.ps1` from the repository root. It compiles the actual
optional adapter and packet codecs against in-memory MPAPI/network boundaries,
then compiles the actual core sync, settings and catalogue against managed
Unity/game boundaries. Reports go to `build/multiplayer-validation` by default.

The adapter checks cover handshake/build identity, out-of-order policy updates,
individual/global permissions, snapshots before welcome, malformed/oversized
packets, disconnect/reused player IDs, incompatible clients and failure reports.
The core checks cover single-player fallback, host validation, revisions and
tombstones, per-car host settings, delayed/moving replicas and stock rollback.

These tests exercise production policy and serialization. The core fixture's
wheel conversion is an adapter double; it cannot establish native geometry,
transport, physics or visual correctness. The separate native-engine,
restored-features, radio and oiling fixtures validate their own narrower seams.
External host/client acceptance remains in `MULTIPLAYER_TESTING.txt`.
