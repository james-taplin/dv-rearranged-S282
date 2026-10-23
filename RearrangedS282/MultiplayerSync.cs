#if B99_ALL_STANDARD
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using DV.ThingTypes;
using DV.Utils;
using LocoMeshSplitter.MeshLoaders;
using Newtonsoft.Json;
using UnityEngine;
using UnityModManagerNet;

[assembly: InternalsVisibleTo("RearrangedS282.Multiplayer")]

namespace RearrangedS282
{
    // Multiplayer owns transport, port replication, movement and simulation authority.
    // This class owns only the extra arrangement state absent from native car packets.
    internal static class MultiplayerSync
    {
        internal const int Protocol = 1;
        internal const int MaxStateLength = 32768;
        private enum Role { Offline, Host, Client, Blocked }
        private static Role role;
        private static bool accepted, canChange, bridgeLoaded;
        private static int bridgeAttempts;
        private static float nextTick, nextBridgeAttempt;
        private static string buildIdentity;
        private static TrainCar applyingCar;
        private static readonly Dictionary<string, NetworkArrangement> states = new Dictionary<string, NetworkArrangement>(StringComparer.Ordinal);
        private static readonly Dictionary<string, Settings> profiles = new Dictionary<string, Settings>(StringComparer.Ordinal);
        private static readonly Dictionary<string, Settings> preparing = new Dictionary<string, Settings>(StringComparer.Ordinal);
        // A host can start after the player has edited preferences without
        // reapplying the existing car. Preserve the profile actually installed.
        private static readonly Dictionary<TrainCar, InstalledProfile> lastApplied = new Dictionary<TrainCar, InstalledProfile>();
        private static readonly Dictionary<string, AppliedState> applied = new Dictionary<string, AppliedState>(StringComparer.Ordinal);
        private static readonly HashSet<string> failed = new HashSet<string>(StringComparer.Ordinal);
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.None, MaxDepth = 8,
            MissingMemberHandling = MissingMemberHandling.Error
        };

        internal static string Status { get; set; } = "Multiplayer is not installed; single player.";
        internal static bool IsHost => role == Role.Host;
        internal static bool IsClient => role == Role.Client || role == Role.Blocked;
        internal static bool IsSimulationAuthority => role == Role.Offline || role == Role.Host;
        internal static bool ApplyingRemote => applyingCar != null;
        internal static bool InSession => role != Role.Offline;
        internal static Action<string> BroadcastState;
        internal static Action<string, int, long> SendRequest;
        internal static Action<string> ReportFailure;
        internal static Action DrawNetworkGUI;

        internal static string BuildIdentity
        {
            get
            {
                if (buildIdentity == null)
                {
                    using (var sha = SHA256.Create())
                    {
                        string main = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(typeof(Main).Assembly.Location))).Replace("-", "");
                        string path = Path.Combine(Main.ModPath, "RearrangedS282.Multiplayer.dll");
                        string bridge = File.Exists(path) ? BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "") : "missing";
                        buildIdentity = Main.RunningVersion + ":" + main + ":" + bridge;
                    }
                }
                return buildIdentity;
            }
        }

        internal static void Initialize()
        {
            // Retry after UMM has finished loading its mods. No MPAPI type is resolved
            // at all in single player; the optional assembly is loaded explicitly.
            TryLoadBridge();
        }

        private static void TryLoadBridge()
        {
            if (bridgeLoaded || bridgeAttempts >= 15 || Time.unscaledTime < nextBridgeAttempt) return;
            var mod = UnityModManager.FindMod("Multiplayer");
            if (mod == null || !mod.Active) return;
            bridgeAttempts++;
            nextBridgeAttempt = Time.unscaledTime + 2f;
            role = Role.Blocked;
            Status = "Waiting for the Multiplayer API.";
            try
            {
                var assembly = Assembly.LoadFrom(Path.Combine(Main.ModPath, "RearrangedS282.Multiplayer.dll"));
                assembly.GetType("RearrangedS282.Multiplayer.Bridge", true).GetMethod("Init", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
                bridgeLoaded = true;
            }
            catch (Exception ex)
            {
                Status = bridgeAttempts < 15 ? "Waiting for Multiplayer integration to finish loading; arrangement changes are locked."
                    : "Multiplayer integration could not load. Rearrangement is locked; use the complete matching test package and restart.";
                if (bridgeAttempts == 1 || bridgeAttempts == 15) Main.Logger.LogException(Status, ex);
            }
        }

        internal static void BeginHost()
        {
            ClearSession();
            role = Role.Host;
            Status = "Hosting; all matching players can use their wheel-arranger radios.";
        }

        internal static void BeginClient()
        {
            ClearSession();
            role = Role.Client;
            Status = "Connecting; arrangement changes are locked until the host accepts this build.";
        }

        internal static void EndSession()
        {
            ClearSession();
            role = Role.Offline;
            Status = "Multiplayer disconnected; single player.";
        }

        private static void ClearSession()
        {
            states.Clear(); profiles.Clear(); preparing.Clear(); applied.Clear(); failed.Clear();
            applyingCar = null;
            accepted = canChange = false;
            BroadcastState = null; SendRequest = null; ReportFailure = null;
            nextTick = 0;
        }

        internal static void AcceptClient(bool allowed, string message)
        {
            if (role != Role.Client) return;
            accepted = true;
            canChange = allowed;
            Status = message;
        }

        internal static void RejectClient(string message)
        {
            if (role != Role.Client && role != Role.Blocked) return;
            accepted = canChange = false;
            role = Role.Blocked;
            Status = message;
        }

        internal static bool DeferAutomatic(TrainCar car) => IsClient;

        internal static Settings SettingsFor(TrainCar car)
        {
            if (car != null && !string.IsNullOrEmpty(car.CarGUID))
            {
                if (InSession && preparing.TryGetValue(car.CarGUID, out var pending)) return pending;
                if (InSession && profiles.TryGetValue(car.CarGUID, out var profile)) return profile;
            }
            return Main.settings;
        }

        internal static bool BeforeSwitch(TrainCar car, int id)
        {
            if (IsClient) return accepted && ReferenceEquals(car, applyingCar);
            if (car != null && !string.IsNullOrEmpty(car.CarGUID))
                preparing[car.CarGUID] = CloneSettings(Main.settings);
            return true;
        }

        internal static void OnApplied(TrainCar car, int id)
        {
            if (!ValidCar(car) || !WheelArrangement.IsSupportedId(id)) return;
            var profile = preparing.TryGetValue(car.CarGUID, out var candidate) ? candidate :
                IsClient && profiles.TryGetValue(car.CarGUID, out var replica) ? replica :
                lastApplied.TryGetValue(car, out var installed) && string.Equals(installed.Guid, car.CarGUID, StringComparison.Ordinal) ? installed.Settings : CloneSettings(Main.settings);
            preparing.Remove(car.CarGUID);
            lastApplied[car] = new InstalledProfile { Guid = car.CarGUID, Settings = profile };
            if (!IsHost) return;
            profiles[car.CarGUID] = profile;
            var state = new NetworkArrangement { CarGuid = car.CarGUID, Arrangement = id,
                Revision = checked(RevisionFor(car.CarGUID) + 1), SettingsJson = SerializeSettings(profile) };
            states[car.CarGUID] = state;
            Publish(state);
        }

        internal static void OnSwitchFailed(TrainCar car)
        {
            if (car == null || string.IsNullOrEmpty(car.CarGUID)) return;
            preparing.Remove(car.CarGUID);
            if (car.GetComponent<WheelRearranger>()?.LastSwitchRestoredStock == true)
            {
                var stock = CloneSettings(Main.settings);
                stock.poweredTenderArrangements = Array.Empty<int>();
                preparing[car.CarGUID] = stock;
                OnApplied(car, (int)WheelArrangementType.s282);
            }
        }

        internal static void Forget(TrainCar car)
        {
            if (car == null) return;
            lastApplied.Remove(car);
            if (string.IsNullOrEmpty(car.CarGUID)) return;
            string guid = car.CarGUID;
            applied.Remove(guid); failed.Remove(guid); preparing.Remove(guid);
            if (IsHost && states.TryGetValue(guid, out var previous) && !previous.Removed)
            {
                var removed = new NetworkArrangement { CarGuid = guid, Revision = checked(previous.Revision + 1), Removed = true };
                states[guid] = removed;
                profiles.Remove(guid);
                Publish(removed);
            }
        }

        private static void Publish(NetworkArrangement state)
        {
            try { BroadcastState?.Invoke(JsonConvert.SerializeObject(state, JsonSettings)); }
            catch (Exception ex) { Main.Logger.LogException("Arrangement applied, but its multiplayer update could not be sent", ex); }
        }

        internal static long RevisionFor(string guid) => guid != null && states.TryGetValue(guid, out var state) ? state.Revision : 0;
        internal static IEnumerable<string> Snapshot() => states.Values.Select(s => JsonConvert.SerializeObject(s, JsonSettings)).ToArray();

        internal static bool RequestChange(TrainCar car, int id, out string reason)
        {
            reason = null;
            if (!ValidCar(car) || !WheelArrangement.IsSupportedId(id)) { reason = "Select a valid S282 arrangement."; return false; }
            if (IsClient)
            {
                if (!accepted || !canChange || SendRequest == null) { reason = "The host has locked arrangement changes for this player, or this build has not been accepted."; return false; }
                try { SendRequest(car.CarGUID, id, RevisionFor(car.CarGUID)); }
                catch (Exception ex) { Main.Logger.LogException("Could not request arrangement from host", ex); reason = "The request could not be sent to the host."; return false; }
                reason = "Arrangement requested from host; wait for approval and synchronization.";
                Status = reason;
                return true;
            }
            if (IsHost) return TryHostRequest(car.CarGUID, id, RevisionFor(car.CarGUID), out reason);
            bool result = (car.GetComponent<WheelRearranger>() ?? car.gameObject.AddComponent<WheelRearranger>()).TrySwitchStage2(id);
            if (result) ConventionalSpawn.RecordApplied(car, id);
            else reason = car.GetComponent<WheelRearranger>()?.LastConventionalError ?? "Stop, repair and close the regulator; let driving force and wheel slip settle first.";
            return result;
        }

        internal static bool TryHostRequest(string guid, int id, long expectedRevision, out string reason)
        {
            reason = null;
            if (!IsHost || !WheelArrangement.IsSupportedId(id) || !ValidGuid(guid)) { reason = "Invalid arrangement request."; return false; }
            if (expectedRevision != RevisionFor(guid)) { reason = "This locomotive changed since your selection. Select it again."; return false; }
#if B99_DIAGNOSTICS || B99_STRUCTURAL_DIAGNOSTICS
            if (Stage2RuntimeChecks.Running) { reason = "Wait for the host's structural test to finish."; return false; }
#endif
            var car = Cars().FirstOrDefault(c => ValidCar(c) && string.Equals(c.CarGUID, guid, StringComparison.Ordinal));
            if (car == null) { reason = "The host cannot find that S282."; return false; }
            bool result = (car.GetComponent<WheelRearranger>() ?? car.gameObject.AddComponent<WheelRearranger>()).TrySwitchStage2(id);
            if (result) ConventionalSpawn.RecordApplied(car, id);
            else reason = car.GetComponent<WheelRearranger>()?.LastConventionalError ?? "The host refused the change: stop, repair and close the regulator; let driving force and wheel slip settle first.";
            Status = result ? "Synchronized " + car.ID + ": " + WheelArrangement.WheelArrangementNames[id] : reason;
            return result;
        }

        internal static void ReceiveState(string json)
        {
            if (role != Role.Client || !accepted) return;
            try
            {
                if (string.IsNullOrEmpty(json) || json.Length > MaxStateLength) throw new InvalidDataException("Arrangement message is too large or empty.");
                var state = JsonConvert.DeserializeObject<NetworkArrangement>(json, JsonSettings);
                if (state == null || !ValidGuid(state.CarGuid) || state.Revision <= 0 ||
                    (!state.Removed && (!WheelArrangement.IsSupportedId(state.Arrangement) || !ValidProfile(state.SettingsJson))))
                    throw new InvalidDataException("Invalid host arrangement state.");
                if (state.Revision <= RevisionFor(state.CarGuid)) return;
                if (!states.ContainsKey(state.CarGuid) && states.Count >= 4096) throw new InvalidDataException("Too many arrangement identities in this session.");
                states[state.CarGuid] = state;
                failed.Remove(state.CarGuid);
                if (state.Removed) { applied.Remove(state.CarGuid); profiles.Remove(state.CarGuid); }
            }
            catch (Exception ex)
            {
                Status = "Rejected invalid host arrangement data: " + ex.Message;
                Main.Logger.Warning(Status);
            }
        }

        internal static void Tick()
        {
            TryLoadBridge();
            if (Time.unscaledTime < nextTick) return;
            nextTick = Time.unscaledTime + .5f;
            if (IsHost)
            {
                foreach (var car in Cars().Where(ValidCar).ToArray())
                    if (!states.TryGetValue(car.CarGUID, out var known) || known.Removed)
                        OnApplied(car, (int)(car.GetComponent<WheelRearranger>()?.currentWA ?? WheelArrangementType.s282));
                return;
            }
            if (role != Role.Client || !accepted) return;
            foreach (var car in Cars().Where(ValidCar).ToArray())
            {
                if (!states.TryGetValue(car.CarGUID, out var state) || state.Removed || failed.Contains(car.CarGUID)) continue;
                if (applied.TryGetValue(car.CarGUID, out var previous) && ReferenceEquals(previous.Car, car) && previous.Revision == state.Revision) continue;
                if (!ReadyForRemote(car, state.Arrangement)) continue;
                profiles[car.CarGUID] = DeserializeSettings(state.SettingsJson);
                applyingCar = car;
                try
                {
                    if ((car.GetComponent<WheelRearranger>() ?? car.gameObject.AddComponent<WheelRearranger>()).TrySwitchStage2(state.Arrangement))
                    {
                        ConventionalSpawn.RecordApplied(car, state.Arrangement);
                        applied[car.CarGUID] = new AppliedState { Car = car, Revision = state.Revision };
                        Status = "Synchronized " + car.ID + ": " + WheelArrangement.WheelArrangementNames[state.Arrangement] + (canChange ? ". Radio changes allowed." : ". Radio changes locked by host.");
                    }
                    else
                    {
                        FailReplica(car, car.GetComponent<WheelRearranger>().LastConventionalError ?? "The native running gear could not be initialized.");
                    }
                }
                catch (Exception ex) { FailReplica(car, ex.Message); }
                finally { applyingCar = null; }
            }
        }

        private static void FailReplica(TrainCar car, string reason)
        {
            failed.Add(car.CarGUID);
            Status = "Could not synchronize " + car.ID + ": " + reason + ". Send the host your Player.log; reload before driving this locomotive.";
            Main.Logger.Error(Status);
            try { ReportFailure?.Invoke(Status.Length > 1000 ? Status.Substring(0, 1000) : Status); }
            catch (Exception ex) { Main.Logger.LogException("Could not report arrangement synchronization failure to the host", ex); }
        }

        private static IEnumerable<TrainCar> Cars() => CarSpawner.Instance != null ? CarSpawner.Instance.AllCars : Enumerable.Empty<TrainCar>();
        private static bool ValidGuid(string guid) => Guid.TryParse(guid, out _);
        private static bool ValidCar(TrainCar car) => car != null && car.carType == TrainCarType.LocoSteamHeavy && car.logicCar != null && ValidGuid(car.CarGUID);
        private static bool ReadyForRemote(TrainCar car, int id) => car.entity != null && car.entity.IsConverted && car.AreBogiesFullyInitialized() &&
            car.SimController?.simFlow != null && car.SimController.drivingForce != null && car.SimController.wheelslipController != null &&
            car.SimController.tractionPortsFeeder != null && car.SimController.poweredWheels?.poweredWheels != null &&
            car.SimController.simFlow.TryGetPort("throttle.EXT_IN", out _) &&
            car.adhesionController != null && (id == (int)WheelArrangementType.s282 && !DeserializeSettings(states[car.CarGUID].SettingsJson).PoweredTenderEnabled(id) ||
            car.GetComponent<S282AMeshLoader>() != null && car.transform.Find("LocoS282A_Body/Static_LOD0/s282a_split_body(Clone)") != null);

        private static string SerializeSettings(Settings settings) => JsonConvert.SerializeObject(settings, JsonSettings);
        private static Settings DeserializeSettings(string json) => JsonConvert.DeserializeObject<Settings>(json, JsonSettings);
        private static Settings CloneSettings(Settings settings) => DeserializeSettings(SerializeSettings(settings ?? new Settings()));
        private static bool ValidProfile(string json)
        {
            if (string.IsNullOrEmpty(json) || json.Length > 16384) return false;
            var settings = DeserializeSettings(json);
            return settings != null && Enum.IsDefined(typeof(Settings.X82Options), settings.x42Options) &&
                Enum.IsDefined(typeof(Settings.X102Options), settings.x62Options) && Enum.IsDefined(typeof(Settings.X82Options), settings.x82Options) &&
                Enum.IsDefined(typeof(Settings.X102Options), settings.x102Options) &&
                (settings.poweredTenderArrangements == null || settings.poweredTenderArrangements.Length <= WheelArrangement.SupportedCount &&
                    settings.poweredTenderArrangements.All(PoweredTenderPolicy.Supports));
        }

        internal static void Draw()
        {
            GUILayout.Label("Multiplayer: " + Status);
            if (InSession) GUILayout.Label("The host owns arrangement settings. Changes take effect when the arrangement is applied or reapplied. All peers need the identical test package.");
            DrawNetworkGUI?.Invoke();
        }

        private sealed class AppliedState { internal TrainCar Car; internal long Revision; }
        private sealed class InstalledProfile { internal string Guid; internal Settings Settings; }
    }

    internal sealed class NetworkArrangement
    {
        public string CarGuid;
        public int Arrangement;
        public long Revision;
        public string SettingsJson;
        public bool Removed;
    }
}
#endif
