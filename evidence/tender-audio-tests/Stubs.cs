using UnityEngine;
namespace LocoSim.Implementations { internal sealed class ReciprocatingSteamEngine { } }
namespace DV.ModularAudioCar {
 internal sealed class ChuffClipsSimReader:MonoBehaviour {
  internal sealed class Config { internal Transform parent; }
  internal sealed class ChuffLoop { internal LayeredAudio chuffLoop; }
  internal Config regularChuffConfig=new(),waterChuffConfig=new(),ashChuffConfig=new();
  internal ChuffLoop[] chuffLoops=System.Array.Empty<ChuffLoop>(),waterChuffLoops=System.Array.Empty<ChuffLoop>(),ashChuffLoops=System.Array.Empty<ChuffLoop>();
 }
 internal sealed class LayeredAudio:MonoBehaviour { }
}
namespace RearrangedS282 {
 internal sealed class ArticulatedSimulation:MonoBehaviour {
  internal LocoSim.Implementations.ReciprocatingSteamEngine RearEngine=new(),MiddleEngine=new();
  internal bool IsMiddleActive,IsTenderEngine;
 }
 internal sealed class WheelRearranger:MonoBehaviour {
  internal Transform ArticulatedRearRoot,ArticulatedMiddleRoot,TenderEffectsRoot,TenderExhaustOutlet;
 }
}
