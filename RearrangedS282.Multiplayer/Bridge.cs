using System;
using System.Collections.Generic;
using System.Linq;
using MPAPI;
using MPAPI.Interfaces;
using MPAPI.Types;
using UnityEngine;

namespace RearrangedS282.Multiplayer
{
    // Optional adapter: the core never references MPAPI, so a single-player
    // installation can load without MultiplayerAPI.dll on disk.
    public static class Bridge
    {
        private static IServer server;
        private static IClient client;
        private static bool initialized, accepted, rejected, earlyOverflow;
        private static bool allowGuests = true;
        private static float nextHello;
        private static long policyRevision, receivedPolicyRevision;
        private static readonly HashSet<byte> verified = new HashSet<byte>();
        private static readonly HashSet<byte> blocked = new HashSet<byte>();
        private static readonly List<string> earlyStates = new List<string>();
        private sealed class PendingKick
        {
            internal IPlayer Player;
            internal float At;
            internal bool AwaitingHello;
            internal string Reason;
        }
        private static readonly Dictionary<byte, PendingKick> pendingKicks = new Dictionary<byte, PendingKick>();

        public static void Init()
        {
            if (initialized) return;
            if (!MultiplayerAPI.IsMultiplayerLoaded) throw new InvalidOperationException("Multiplayer API is not ready.");
            initialized = true;
            MultiplayerAPI.Instance.SetModCompatibility("RearrangedS282", MultiplayerCompatibility.All);
            MultiplayerAPI.ServerStarted += OnServerStarted;
            MultiplayerAPI.ClientStarted += OnClientStarted;
            MultiplayerAPI.ServerStopped += OnServerStopped;
            MultiplayerAPI.ClientStopped += OnClientStopped;
            var go = new GameObject("RearrangedS282 Multiplayer");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<Runner>();
            if (MultiplayerAPI.Server == null && MultiplayerAPI.Client == null) MultiplayerSync.EndSession();
            if (MultiplayerAPI.Server != null) OnServerStarted(MultiplayerAPI.Server);
            if (MultiplayerAPI.Client != null) OnClientStarted(MultiplayerAPI.Client);
            Log("Using DV Multiplayer " + MultiplayerAPI.MultiplayerVersion + ", API " + MultiplayerAPI.LoadedApiVersion + ".");
        }

        private static void OnServerStarted(IServer instance)
        {
            if (ReferenceEquals(server, instance)) return;
            server = instance;
            client = null;
            verified.Clear(); blocked.Clear(); earlyStates.Clear(); pendingKicks.Clear();
            allowGuests = true; policyRevision = 1;
            instance.RegisterSerializablePacket<HelloPacket>(OnHello);
            instance.RegisterSerializablePacket<RequestPacket>(OnRequest);
            instance.RegisterSerializablePacket<ApplyFailurePacket>(OnApplyFailure);
            instance.OnPlayerConnected += OnPlayerConnected;
            instance.OnPlayerReady += OnPlayerReady;
            instance.OnPlayerDisconnected += OnPlayerDisconnected;
            MultiplayerSync.BeginHost();
            MultiplayerSync.BroadcastState = Broadcast;
            MultiplayerSync.DrawNetworkGUI = DrawHostGUI;
            foreach (IPlayer player in instance.Players.Where(p => p != null && !p.IsHost && p.IsLoaded).ToArray()) OnPlayerReady(player);
            Log("Host arrangement authority started; guests may request changes.");
        }

        private static void OnServerStopped()
        {
            if (server == null) return;
            server.OnPlayerConnected -= OnPlayerConnected;
            server.OnPlayerReady -= OnPlayerReady;
            server.OnPlayerDisconnected -= OnPlayerDisconnected;
            server = null;
            verified.Clear(); blocked.Clear(); earlyStates.Clear(); pendingKicks.Clear();
            MultiplayerSync.BroadcastState = null;
            MultiplayerSync.DrawNetworkGUI = null;
            MultiplayerSync.EndSession();
        }

        private static bool CanChange(IPlayer player) => player.IsHost || (allowGuests && !blocked.Contains(player.PlayerId));

        private static void OnHello(HelloPacket packet, IPlayer player)
        {
            if (server == null || player == null || !packet.Valid) return;
            bool match = packet.Protocol == MultiplayerSync.Protocol && packet.Identity == MultiplayerSync.BuildIdentity;
            if (!match)
            {
                verified.Remove(player.PlayerId);
                SendWelcome(player, false, "Rearranged S282 builds differ. Every player needs the identical multiplayer package.");
                string reason = "Rearranged S282 builds differ. Install the identical multiplayer test package used by the host and reconnect.";
                ScheduleKick(player, reason);
                Log("Rejected arrangement handshake from " + player.Username + ": different build or protocol.");
                return;
            }
            if (pendingKicks.TryGetValue(player.PlayerId, out var pending) && !pending.AwaitingHello) return;
            pendingKicks.Remove(player.PlayerId);
            verified.Add(player.PlayerId);
            SendWelcome(player, true, PermissionMessage(player));
            SendSnapshot(player);
            Log("Arrangement snapshot sent to " + player.Username + ".");
        }

        private static string PermissionMessage(IPlayer player) => CanChange(player)
            ? "Connected: radio changes are checked and applied by the host."
            : "The host has locked arrangement changes for you. Arrangements still sync.";

        private static string BoundedReason(string reason) => string.IsNullOrEmpty(reason) ? "Arrangement synchronization failed."
            : reason.Length <= 500 ? reason : reason.Substring(0, 500) + "...";

        private static void ScheduleKick(IPlayer player, string reason)
        {
            reason = BoundedReason(reason);
            server.SendWhisperChatMessage(reason, player);
            pendingKicks[player.PlayerId] = new PendingKick
            { Player = server.GetPlayer(player.PlayerId) ?? player, At = Time.unscaledTime + 1f, AwaitingHello = false, Reason = reason };
        }

        private static void OnApplyFailure(ApplyFailurePacket packet, IPlayer player)
        {
            if (server == null || player == null || !packet.Valid || !verified.Contains(player.PlayerId) || player.IsHost) return;
            verified.Remove(player.PlayerId);
            string reason = "Rearranged S282 could not synchronize this guest: " + BoundedReason(packet.Message) + " Reload the identical test package and send James both Player.log files.";
            MultiplayerSync.Status = player.DisplayName + ": " + BoundedReason(reason);
            Main.Logger.Error("[Multiplayer] " + MultiplayerSync.Status);
            ScheduleKick(player, reason);
        }

        private static void SendWelcome(IPlayer player, bool compatible, string message)
        {
            server.SendSerializablePacketToPlayer(new WelcomePacket
            {
                Protocol = MultiplayerSync.Protocol, Identity = MultiplayerSync.BuildIdentity,
                Accepted = compatible, CanChange = compatible && CanChange(player),
                PolicyRevision = policyRevision, Message = message
            }, player);
        }

        private static void OnPlayerReady(IPlayer player)
        {
            if (server == null || player == null || player.IsHost) return;
            if (!verified.Contains(player.PlayerId))
            {
                // The native mod check compares IDs only; an older S282 with the
                // same ID cannot safely join a world with different physics.
                if (!pendingKicks.ContainsKey(player.PlayerId))
                    pendingKicks[player.PlayerId] = new PendingKick
                    {
                        Player = server.GetPlayer(player.PlayerId) ?? player, At = Time.unscaledTime + 45f, AwaitingHello = true,
                        Reason = "Rearranged S282 did not confirm a matching build. Install the host's identical multiplayer test package and reconnect."
                    };
                return;
            }
            // Ready does not guarantee all TrainCars have been generated. The
            // core stores GUID-keyed snapshots until their components are ready.
            SendWelcome(player, true, PermissionMessage(player));
            SendSnapshot(player);
        }

        private static void OnPlayerDisconnected(IPlayer player)
        {
            if (player == null) return;
            verified.Remove(player.PlayerId);
            blocked.Remove(player.PlayerId); // IDs are recycled for later players.
            pendingKicks.Remove(player.PlayerId);
        }

        private static void OnPlayerConnected(IPlayer player)
        {
            // Also makes the API construct its canonical cached wrapper before
            // early Hello packets, whose handler receives a fresh wrapper.
            OnPlayerDisconnected(player);
        }

        private static void SendSnapshot(IPlayer player)
        {
            foreach (string json in MultiplayerSync.Snapshot())
                server.SendSerializablePacketToPlayer(new StatePacket { Json = json }, player);
        }

        private static void Broadcast(string json)
        {
            if (server == null) return;
            var packet = new StatePacket { Json = json };
            foreach (byte id in verified.ToArray())
            {
                IPlayer player = server.GetPlayer(id);
                if (player == null) { verified.Remove(id); blocked.Remove(id); continue; }
                if (!player.IsHost) server.SendSerializablePacketToPlayer(packet, player);
            }
        }

        private static void OnRequest(RequestPacket packet, IPlayer player)
        {
            if (server == null || player == null || !packet.Valid) return;
            string reason;
            bool changed;
            if (!verified.Contains(player.PlayerId)) { changed = false; reason = "Wait for a matching Rearranged S282 handshake."; }
            else if (!CanChange(player)) { changed = false; reason = "The host has locked arrangement changes for you."; }
            else changed = MultiplayerSync.TryHostRequest(packet.CarGuid, packet.Arrangement, packet.ExpectedRevision, out reason);
            server.SendSerializablePacketToPlayer(new RequestResultPacket
            { Message = changed ? "Arrangement applied by the host." : "Arrangement refused: " + reason }, player);
            if (!changed && verified.Contains(player.PlayerId)) SendSnapshot(player);
        }

        private static void OnClientStarted(IClient instance)
        {
            if (server != null || MultiplayerAPI.Instance.IsHost || ReferenceEquals(client, instance)) return;
            client = instance;
            accepted = rejected = earlyOverflow = false;
            nextHello = 0f; receivedPolicyRevision = -1;
            earlyStates.Clear();
            instance.RegisterSerializablePacket<WelcomePacket>(OnWelcome);
            instance.RegisterSerializablePacket<StatePacket>(OnState);
            instance.RegisterSerializablePacket<RequestResultPacket>(OnResult);
            MultiplayerSync.BeginClient();
            MultiplayerSync.SendRequest = SendRequest;
            MultiplayerSync.ReportFailure = ReportFailure;
            Log("Client arrangement changes locked while verifying the host build.");
        }

        private static void OnClientStopped()
        {
            if (client == null) return;
            client = null; accepted = rejected = earlyOverflow = false;
            earlyStates.Clear();
            MultiplayerSync.SendRequest = null;
            MultiplayerSync.ReportFailure = null;
            if (server == null) MultiplayerSync.EndSession();
        }

        internal static void Tick()
        {
            if (server != null)
            {
                foreach (var entry in pendingKicks.ToArray())
                {
                    IPlayer player = server.GetPlayer(entry.Key);
                    PendingKick pending = entry.Value;
                    if (!ReferenceEquals(player, pending.Player) || (pending.AwaitingHello && verified.Contains(entry.Key)))
                    { pendingKicks.Remove(entry.Key); continue; }
                    if (Time.unscaledTime < pending.At) continue;
                    pendingKicks.Remove(entry.Key);
                    if (player == null || player.IsHost) continue;
                    server.SendWhisperChatMessage(pending.Reason, player);
                    MultiplayerSync.Status = player.DisplayName + " was disconnected: " + pending.Reason;
                    Log(MultiplayerSync.Status);
                    server.KickPlayer(player);
                }
            }
            if (client == null || accepted || rejected || !client.IsConnected || Time.unscaledTime < nextHello) return;
            nextHello = Time.unscaledTime + 2f;
            client.SendSerializablePacketToServer(new HelloPacket
            { Protocol = MultiplayerSync.Protocol, Identity = MultiplayerSync.BuildIdentity });
        }

        private static void OnWelcome(WelcomePacket packet)
        {
            if (client == null || rejected || !packet.Valid || packet.PolicyRevision < receivedPolicyRevision) return;
            receivedPolicyRevision = packet.PolicyRevision;
            if (!packet.Accepted || packet.Protocol != MultiplayerSync.Protocol || packet.Identity != MultiplayerSync.BuildIdentity)
            {
                rejected = true; accepted = false; earlyStates.Clear();
                MultiplayerSync.RejectClient("Rearranged S282 builds differ. Every player needs the identical multiplayer package.");
                Log("Host arrangement handshake rejected: incompatible build.");
                return;
            }
            accepted = true; rejected = false;
            MultiplayerSync.AcceptClient(packet.CanChange, packet.Message);
            foreach (string json in earlyStates) MultiplayerSync.ReceiveState(json);
            earlyStates.Clear();
            if (earlyOverflow)
            {
                earlyOverflow = false;
                client.SendSerializablePacketToServer(new HelloPacket
                { Protocol = MultiplayerSync.Protocol, Identity = MultiplayerSync.BuildIdentity });
            }
        }

        private static void OnState(StatePacket packet)
        {
            if (client == null || rejected || !packet.Valid) return;
            if (accepted) MultiplayerSync.ReceiveState(packet.Json);
            else if (earlyStates.Count < 1024) earlyStates.Add(packet.Json);
            else
            {
                earlyStates.Clear();
                earlyOverflow = true; // Request a fresh snapshot once handshake succeeds.
                Log("Early snapshot buffer exceeded; waiting for a fresh handshake.");
            }
        }

        private static void SendRequest(string guid, int arrangement, long revision)
        {
            if (client == null || !accepted || rejected) return;
            client.SendSerializablePacketToServer(new RequestPacket
            { CarGuid = guid, Arrangement = arrangement, ExpectedRevision = revision });
        }

        private static void OnResult(RequestResultPacket packet)
        {
            if (client != null && accepted && packet.Valid) MultiplayerSync.Status = packet.Message;
        }

        private static void ReportFailure(string message)
        {
            if (client == null || !accepted || rejected) return;
            accepted = false; rejected = true;
            MultiplayerSync.RejectClient("Arrangement synchronization failed. The host has been notified; reconnect after checking both Player.log files.");
            client.SendSerializablePacketToServer(new ApplyFailurePacket { Message = BoundedReason(message) });
        }

        private static void DrawHostGUI()
        {
            if (server == null) return;
            GUILayout.Label("Every player must use the identical Rearranged S282 multiplayer test package.");
            GUILayout.Label("Multiplayer arrangement permissions (this session)");
            bool guests = GUILayout.Toggle(allowGuests, "Allow guests to change arrangements");
            bool changed = guests != allowGuests;
            allowGuests = guests;
            foreach (IPlayer player in server.Players.Where(p => p != null && !p.IsHost).ToArray())
            {
                if (!verified.Contains(player.PlayerId))
                {
                    GUILayout.Label(player.DisplayName + " — waiting for a matching build");
                    continue;
                }
                bool wasAllowed = !blocked.Contains(player.PlayerId);
                bool allowed = GUILayout.Toggle(wasAllowed, player.DisplayName + " may change arrangements");
                if (allowed == wasAllowed) continue;
                if (allowed) blocked.Remove(player.PlayerId); else blocked.Add(player.PlayerId);
                changed = true;
            }
            if (!changed) return;
            policyRevision++;
            foreach (byte id in verified.ToArray())
            {
                IPlayer player = server.GetPlayer(id);
                if (player != null && !player.IsHost) SendWelcome(player, true, PermissionMessage(player));
            }
        }

        private static void Log(string message) => Main.Logger.Log("[Multiplayer] " + message);
    }

    internal sealed class Runner : MonoBehaviour
    {
        private void Update()
        {
            try { Bridge.Tick(); }
            catch (Exception ex) { Main.Logger.LogException("Multiplayer handshake", ex); }
        }
    }
}
