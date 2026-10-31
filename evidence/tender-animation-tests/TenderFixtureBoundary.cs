using System;
using System.Collections.Generic;
using LocoSim.Implementations;
using UnityEngine;

namespace RearrangedS282
{
    // Boundary only: the isolated player has no live TrainCar/boiler graph.
    // Engines, ports, readers, Animator evaluation and clone lifecycle are native.
    internal sealed class TenderFixtureFlow
    {
        internal readonly Dictionary<string, Port> Ports = new Dictionary<string, Port>();
        internal bool TryGetPort(string id, out Port port) => Ports.TryGetValue(id, out port);
    }
    internal sealed class ArticulatedSimulation
    {
        internal bool IsTenderEngine = true, IsMiddleActive = true;
        internal ReciprocatingSteamEngine MainEngine, MiddleEngine;
        internal readonly TenderFixtureFlow Flow = new TenderFixtureFlow();
    }
    internal static class GroupCrankAnimation
    {
        internal static readonly HashSet<Animator> Owned = new HashSet<Animator>();
        internal static bool Owns(Animator animator) => animator != null && Owned.Contains(animator);
    }
}
