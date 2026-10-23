using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using MPAPI;
using MPAPI.Interfaces;
using MPAPI.Interfaces.Packets;
using RearrangedS282;
using RearrangedS282.Multiplayer;
using UnityEngine;

static class Program
{
    static int checks,cases;
    static void Check(bool value,string label){checks++;if(!value)throw new Exception(label);}
    static byte[] Bytes(BoundedPacket packet){using var stream=new MemoryStream();using(var writer=new BinaryWriter(stream,Encoding.UTF8,true))packet.Serialize(writer);return stream.ToArray();}
    static T Read<T>(byte[] bytes)where T:BoundedPacket,new(){var packet=new T();using var reader=new BinaryReader(new MemoryStream(bytes));packet.Deserialize(reader);return packet;}
    static T Wire<T>(T packet)where T:BoundedPacket,new()=>Read<T>(Bytes(packet));
    sealed class Player:IPlayer {public byte PlayerId{get;set;}public bool IsHost{get;set;}public bool IsLoaded{get;set;}=true;public string Username{get;set;}public string DisplayName=>Username;}
    sealed class Server:IServer
    {
        public readonly Dictionary<Type,Delegate> Handlers=new();
        public readonly List<IPlayer> Connected=new();
        public readonly List<(BoundedPacket packet,IPlayer player)> Sent=new();
        public readonly List<IPlayer> Kicked=new();
        public readonly List<(string message,IPlayer player)> Whispers=new();
        public event Action<IPlayer> OnPlayerReady,OnPlayerDisconnected,OnPlayerConnected;
        public IEnumerable<IPlayer> Players=>Connected;
        public void RegisterSerializablePacket<T>(Action<T,IPlayer> callback)where T:ISerializablePacket=>Handlers[typeof(T)]=callback;
        public void SendSerializablePacketToPlayer<T>(T packet,IPlayer player)where T:ISerializablePacket=>Sent.Add(((BoundedPacket)(object)packet,player));
        public IPlayer GetPlayer(byte id)=>Connected.FirstOrDefault(p=>p.PlayerId==id);
        public void KickPlayer(IPlayer player){Kicked.Add(player);Disconnect(player);}
        public void SendWhisperChatMessage(string message,IPlayer player)=>Whispers.Add((message,player));
        public void Receive<T>(T packet,IPlayer player)where T:BoundedPacket,new()=>((Action<T,IPlayer>)Handlers[typeof(T)])(Wire(packet),player);
        public void Ready(IPlayer player)=>OnPlayerReady?.Invoke(player);
        public void Connect(IPlayer player){Connected.Add(player);OnPlayerConnected?.Invoke(player);}
        public void Disconnect(IPlayer player){Connected.Remove(player);OnPlayerDisconnected?.Invoke(player);}
        public WelcomePacket Welcome(IPlayer player)=>Sent.Where(p=>p.player==player).Select(p=>p.packet).OfType<WelcomePacket>().Last();
        public int States(IPlayer player)=>Sent.Count(p=>p.player==player&&p.packet is StatePacket);
    }
    sealed class Client:IClient
    {
        public bool IsConnected{get;set;}=true;
        public readonly Dictionary<Type,Delegate> Handlers=new();
        public readonly List<BoundedPacket> Sent=new();
        public void RegisterSerializablePacket<T>(Action<T> callback)where T:ISerializablePacket=>Handlers[typeof(T)]=callback;
        public void SendSerializablePacketToServer<T>(T packet)where T:ISerializablePacket=>Sent.Add((BoundedPacket)(object)packet);
        public void Receive<T>(T packet)where T:BoundedPacket,new()=>((Action<T>)Handlers[typeof(T)])(Wire(packet));
    }
    static HelloPacket Hello()=>new(){Protocol=MultiplayerSync.Protocol,Identity=MultiplayerSync.BuildIdentity};
    static WelcomePacket Welcome(long revision=1,bool canChange=true)=>new(){Protocol=MultiplayerSync.Protocol,Identity=MultiplayerSync.BuildIdentity,Accepted=true,CanChange=canChange,PolicyRevision=revision,Message="fixture permission"};
    static void PacketTests()
    {
        cases++;
        var hello=Wire(new HelloPacket{Protocol=7,Identity="unicode-\u00e9-\u65e5"});
        Check(hello.Valid&&hello.Protocol==7&&hello.Identity=="unicode-\u00e9-\u65e5","Hello binary round trip");
        var welcome=Wire(new WelcomePacket{Protocol=1,Identity="same-build",Accepted=true,CanChange=false,PolicyRevision=long.MaxValue,Message="locked"});
        Check(welcome.Valid&&welcome.Protocol==1&&welcome.Identity=="same-build"&&welcome.Accepted&&!welcome.CanChange&&welcome.PolicyRevision==long.MaxValue&&welcome.Message=="locked","Welcome binary round trip");
        var state=Wire(new StatePacket{Json="{\"car\":\"A\"}"});Check(state.Valid&&state.Json=="{\"car\":\"A\"}","State binary round trip");
        var request=Wire(new RequestPacket{CarGuid="A-guid",Arrangement=47,ExpectedRevision=long.MaxValue});
        Check(request.Valid&&request.CarGuid=="A-guid"&&request.Arrangement==47&&request.ExpectedRevision==long.MaxValue,"Request binary round trip");
        Check(Wire(new RequestResultPacket{Message="refused"}).Message=="refused","Result binary round trip");
        Check(Wire(new ApplyFailurePacket{Message="native geometry failed"}).Message=="native geometry failed","Replica-failure binary round trip");
        foreach(var packet in new BoundedPacket[]{Hello(),Welcome(),new StatePacket{Json="payload"},new RequestPacket{CarGuid="guid",Arrangement=47,ExpectedRevision=12},new RequestResultPacket{Message="result"},new ApplyFailurePacket{Message="failure"}})
        {
            cases++;var bytes=Bytes(packet);
            for(int length=0;length<bytes.Length;length++)
            {
                var truncated=(BoundedPacket)Activator.CreateInstance(packet.GetType());
                using var reader=new BinaryReader(new MemoryStream(bytes.Take(length).ToArray()));truncated.Deserialize(reader);
                Check(!truncated.Valid,packet.GetType().Name+" rejects every truncated prefix at "+length);
            }
        }
        cases++;
        Check(Wire(new HelloPacket{Identity=new string('a',512)}).Valid,"Hello exact byte limit");
        Check(!Wire(new HelloPacket{Identity=new string('a',513)}).Valid,"Hello oversize identity refused");
        Check(!Wire(new HelloPacket{Identity=new string('\u00e9',257)}).Valid,"Hello caps UTF8 bytes, not characters");
        Check(Wire(new StatePacket{Json=new string('a',65536)}).Valid,"State exact byte limit");
        Check(!Wire(new StatePacket{Json=new string('a',65537)}).Valid,"State oversize JSON refused");
        Check(!Wire(new RequestPacket{CarGuid=new string('a',129)}).Valid,"Request oversize GUID refused");
        Check(!Wire(new WelcomePacket{Identity="x",Message=new string('a',2049)}).Valid,"Welcome oversize message refused");
        Check(!Wire(new RequestResultPacket{Message=new string('a',2049)}).Valid,"Result oversize message refused");
        Check(!Wire(new ApplyFailurePacket{Message=new string('a',2049)}).Valid,"Replica-failure oversize message refused");
        foreach(var bytes in new[]{new byte[]{0xff,0xff,0xff,0xff,0xff},new byte[]{0xff,0xff,0xff,0xff,0x0f},new byte[]{0x80},new byte[]{2,0xc0,0xaf},new byte[]{3,0x61}})
            Check(!Read<StatePacket>(bytes).Valid,"Malformed size/UTF8/truncated payload refused without throwing");
        var reused=Wire(new StatePacket{Json="valid"});using(var reader=new BinaryReader(new MemoryStream(new byte[]{2,1})))reused.Deserialize(reader);
        Check(!reused.Valid,"Failed second Deserialize cannot retain Valid flag");
    }
    static void HostTests()
    {
        cases++;Bridge.Init();Bridge.Init();Check(MultiplayerAPI.Instance.CompatibilityCalls==1,"Bridge initializes once");
        var server=new Server();var a=new Player{PlayerId=1,Username="Alice"};var b=new Player{PlayerId=2,Username="Bob"};server.Connected.AddRange(new[]{a,b});
        MultiplayerAPI.StartServer(server);Check(MultiplayerSync.Hosts==1&&MultiplayerSync.DrawNetworkGUI!=null,"Server starts host authority and host UI");
        var request=new RequestPacket{CarGuid="car",Arrangement=19,ExpectedRevision=4};int requests=MultiplayerSync.HostRequests;
        server.Receive(request,a);Check(MultiplayerSync.HostRequests==requests&&server.Sent.Last().packet is RequestResultPacket,"Unverified guest cannot call host mutation");
        server.Ready(a);Check(server.States(a)==0,"Ready event alone does not bypass handshake");
        var badProtocol=new Player{PlayerId=3,Username="WrongProtocol"};var badBuild=new Player{PlayerId=4,Username="WrongBuild"};server.Connect(badProtocol);server.Connect(badBuild);
        server.Receive(new HelloPacket{Protocol=99,Identity=MultiplayerSync.BuildIdentity},badProtocol);Check(!server.Welcome(badProtocol).Accepted&&server.States(badProtocol)==0,"Protocol mismatch gets no state snapshot");
        server.Receive(new HelloPacket{Protocol=MultiplayerSync.Protocol,Identity="other-build"},badBuild);Check(!server.Welcome(badBuild).Accepted,"Build mismatch refused");
        Time.unscaledTime=1.1f;Bridge.Tick();Check(server.Kicked.Contains(badProtocol)&&server.Kicked.Contains(badBuild),"Mismatched packages are disconnected through native kick API after message grace");
        server.Receive(Hello(),a);server.Receive(Hello(),b);Check(server.Welcome(a).Accepted&&server.Welcome(a).CanChange&&server.States(a)==2,"Verified guest gets permission and full snapshot");
        server.Receive(request,a);Check(MultiplayerSync.HostRequests==requests+1,"Verified permitted guest reaches core mutation validator");
        cases++;GUILayout.Values["Alice may change arrangements"]=false;MultiplayerSync.DrawNetworkGUI();GUILayout.Values.Clear();
        Check(!server.Welcome(a).CanChange&&server.Welcome(b).CanChange,"Per-player lock updates only selected permission");
        int states=server.States(a);server.Receive(request,a);Check(MultiplayerSync.HostRequests==requests+1&&server.States(a)==states+2,"Locked request refused, authoritative snapshot retained");
        MultiplayerSync.BroadcastState("latest");Check(server.Sent.Count(p=>p.packet is StatePacket s&&s.Json=="latest")==2,"Locked guest still receives accepted state broadcasts");
        server.Receive(request,b);Check(MultiplayerSync.HostRequests==requests+2,"Other guest retains radio access");
        GUILayout.Values["Allow guests to change arrangements"]=false;MultiplayerSync.DrawNetworkGUI();GUILayout.Values.Clear();
        Check(!server.Welcome(a).CanChange&&!server.Welcome(b).CanChange,"Global guest lock reaches all verified guests");
        server.Receive(request,b);Check(MultiplayerSync.HostRequests==requests+2,"Global guest lock enforced at host boundary");
        GUILayout.Values["Allow guests to change arrangements"]=true;MultiplayerSync.DrawNetworkGUI();GUILayout.Values.Clear();
        Check(!server.Welcome(a).CanChange&&server.Welcome(b).CanChange,"Unlock global control preserves individual lock");
        cases++;server.Disconnect(a);var reused=new Player{PlayerId=1,Username="Carol"};server.Connected.Add(reused);server.Ready(reused);
        Check(server.States(reused)==0,"Recycled player ID requires a fresh handshake");
        server.Receive(Hello(),reused);Check(server.Welcome(reused).CanChange,"Recycled player ID does not inherit departed player's lock");
        server.Receive(new HelloPacket{Protocol=99,Identity="wrong"},b);int sent=server.States(b);MultiplayerSync.BroadcastState("post-reject");Check(server.States(b)==sent,"Newly rejected build loses broadcast membership");
        cases++;var silent=new Player{PlayerId=7,Username="Silent"};server.Connect(silent);server.Ready(silent);Time.unscaledTime=45;Bridge.Tick();Check(!server.Kicked.Contains(silent),"Missing handshake has full 45 second ready grace");
        Time.unscaledTime=47;Bridge.Tick();Check(server.Kicked.Contains(silent),"Ready same-ID client without a handshake is disconnected after 45 seconds");
        var departed=new Player{PlayerId=8,Username="Departed"};server.Connect(departed);server.Ready(departed);server.Disconnect(departed);var fresh=new Player{PlayerId=8,Username="Fresh"};server.Connect(fresh);server.Receive(Hello(),fresh);Time.unscaledTime=100;Bridge.Tick();Check(!server.Kicked.Contains(fresh),"Departed player's pending timeout cannot kick recycled compatible player ID");
        cases++;server.Receive(new ApplyFailurePacket{Message="native apply failed"},fresh);requests=MultiplayerSync.HostRequests;server.Receive(request,fresh);Check(MultiplayerSync.HostRequests==requests,"Guest reporting failed native replica immediately loses mutation access");Time.unscaledTime=102;Bridge.Tick();Check(server.Kicked.Contains(fresh),"Failed native replica disconnects through native server kick API");
        int ended=MultiplayerSync.Ended;MultiplayerAPI.StopServer();Check(MultiplayerSync.DrawNetworkGUI==null&&MultiplayerSync.BroadcastState==null&&MultiplayerSync.Ended==ended+1,"Server stop releases callbacks and core session");
    }
    static void ClientTests()
    {
        cases++;var client=new Client();MultiplayerAPI.StartClient(client);Check(MultiplayerSync.Clients==1&&MultiplayerSync.SendRequest!=null,"Client starts locked handshake boundary");
        MultiplayerSync.SendRequest("car",19,3);Check(client.Sent.Count==0,"Client cannot send mutation before acceptance");
        Time.unscaledTime=0;Bridge.Tick();Bridge.Tick();Check(client.Sent.OfType<HelloPacket>().Count()==1,"Hello sends immediately without duplicate tick spam");
        Time.unscaledTime=2;Bridge.Tick();Check(client.Sent.OfType<HelloPacket>().Count()==2,"Unanswered handshake retries");
        int received=MultiplayerSync.Received.Count;client.Receive(new StatePacket{Json="early"});Check(MultiplayerSync.Received.Count==received,"Unordered state buffered before matching welcome");
        client.Receive(Welcome());Check(MultiplayerSync.CanChange&&MultiplayerSync.Received.Last()=="early","Matching welcome unlocks permission and drains early snapshot");
        MultiplayerSync.SendRequest("car",47,91);var sent=client.Sent.OfType<RequestPacket>().Last();Check(sent.CarGuid=="car"&&sent.Arrangement==47&&sent.ExpectedRevision==91,"Client forwards expected state revision unchanged");
        client.Receive(Welcome(3,false));client.Receive(Welcome(2,true));Check(!MultiplayerSync.CanChange,"Stale permission revision cannot undo host lock");
        client.Receive(new RequestResultPacket{Message="useful refusal"});Check(MultiplayerSync.Status=="useful refusal","Host request result reaches client status");
        int rejected=MultiplayerSync.Rejects,accepted=MultiplayerSync.Accepts;MultiplayerSync.ReportFailure(new string('x',3000));
        Check(MultiplayerSync.Rejects==rejected+1&&client.Sent.OfType<ApplyFailurePacket>().Last().Message.Length<=503,"Replica failure locks client and sends bounded host notification");
        client.Receive(Welcome(4,true));Check(MultiplayerSync.Accepts==accepted&&!MultiplayerSync.CanChange,"Delayed welcome cannot unlock failed replica before reconnect");
        int ended=MultiplayerSync.Ended;MultiplayerAPI.StopClient();Check(MultiplayerSync.SendRequest==null&&MultiplayerSync.Ended==ended+1,"Client stop releases request callback and session");
        cases++;client=new Client();MultiplayerAPI.StartClient(client);received=MultiplayerSync.Received.Count;
        rejected=MultiplayerSync.Rejects;client.Receive(new StatePacket{Json="untrusted-early"});var wrong=Welcome();wrong.Identity="wrong-build";client.Receive(wrong);
        client.Receive(new StatePacket{Json="untrusted-late"});MultiplayerSync.SendRequest("car",19,3);Check(MultiplayerSync.Rejects==rejected+1&&MultiplayerSync.Received.Count==received&&client.Sent.Count==0,"Mismatched host clears queued state and blocks later state/requests");
        MultiplayerAPI.StopClient();
        cases++;client=new Client();MultiplayerAPI.StartClient(client);received=MultiplayerSync.Received.Count;
        for(int n=0;n<1025;n++)client.Receive(new StatePacket{Json="early-"+n});
        client.Receive(Welcome());Check(client.Sent.OfType<HelloPacket>().Count()==1,"Early snapshot overflow explicitly requests fresh snapshot after welcome");
        client.Receive(new StatePacket{Json="fresh-snapshot"});Check(MultiplayerSync.Received.Last()=="fresh-snapshot","Client accepts replacement snapshot after bounded overflow");
        MultiplayerAPI.StopClient();
    }
    static int Main(string[] args)
    {
        PacketTests();HostTests();ClientTests();
        var report=new{passed=true,checks,cases,compiledProduction=new[]{"Bridge.cs","Packets.cs"},limitations="Actual bridge and packet source compiled unchanged against managed MPAPI, Unity and core-boundary doubles. This validates codec and adapter policy, not the real transport, native player identity, live simulation or Unity rendering."};
        File.WriteAllText(args[0],JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine($"Bridge/packet validation passed {checks} checks across {cases} cases.");return 0;
    }
}
