using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using UnityEngine;
using DV.ModularAudioCar;
using RearrangedS282;
internal static class Program {
 static int checks;static void Check(bool ok,string label){checks++;if(!ok)throw new Exception(label);}
 sealed class Binding:ArticulatedEffects.AudioBinding {
  internal ArticulatedEffects.EffectsState state;internal int removed;
  internal override void Remove(){removed++;state.Calls.Add("audio");state.Audio.Remove(this);}
 }
 static void Main(string[] args){
  var go=new GameObject("loco");var car=go.AddComponent<TrainCar>();var sim=go.AddComponent<ArticulatedSimulation>();var wheels=go.AddComponent<WheelRearranger>();var state=go.AddComponent<ArticulatedEffects.EffectsState>();state.Car=car;
  wheels.ArticulatedRearRoot=go.transform.Add("rear");wheels.ArticulatedMiddleRoot=go.transform.Add("middle");
  wheels.TenderEffectsRoot=go.transform.Add("tender");wheels.TenderExhaustOutlet=wheels.TenderEffectsRoot.Add("outlet");
  for(int i=0;i<200;i++){
   sim.IsMiddleActive=i%2==0;sim.IsTenderEngine=i%3==0;
   var chosen=state.Secondaries;
   Check(chosen[0].Engine==sim.RearEngine&&chosen[0].Root==wheels.ArticulatedRearRoot,"rear stays on rear engine");
   Check(chosen.Count==(sim.IsMiddleActive?2:1),"active stages selected exactly once");
   Check(state.ParticleSecondaries.Count==chosen.Count,"audio and particles share engine identities, no duplicate tender");
   if(sim.IsMiddleActive)Check(chosen[1].Tender==sim.IsTenderEngine&&chosen[1].Engine==sim.MiddleEngine,"third stage mapped by ownership");
   var first=new Binding{state=state};var second=new Binding{state=state};state.Audio.Add(first);state.Audio.Add(second);state.Calls.Clear();
   ArticulatedEffects.TenderChanged(car);
   Check(first.removed==1&&second.removed==1&&state.Audio.Count==0,"detach clears both old bindings once");
   Check(state.Calls.SequenceEqual(new[]{"audio","audio","particles","refresh"}),"audio removed before refresh can clone replacements");
   state.ResetAudio();Check(first.removed==1&&second.removed==1,"reset idempotent");
  }
  sim.IsActive=false;sim.IsMiddleActive=sim.IsTenderEngine=true;
  Check(state.Secondaries.Count==1&&state.Secondaries[0].Tender,"rigid host selects only tender audio/particles without a fake rear engine");
  sim.IsMiddleActive=false;Check(state.Secondaries.Count==0,"uncoupled rigid host has no auxiliary emitters");
  sim.IsActive=true;
  sim.IsMiddleActive=sim.IsTenderEngine=true;wheels.TenderExhaustOutlet=null;
  Check(state.Secondaries.All(s=>!s.Tender),"no tender emitter bound before exhaust outlet is ready");
  for(int i=0;i<250;i++){
   var container=new GameObject("controller").transform;var root=container.Add("owned chuff");
   var reader=root.gameObject.AddComponent<ChuffClipsSimReader>();
   root.localScale=new(-1.2f,1.1f,.9f);var scale=root.localScale;
   reader.regularChuffConfig.parent=root.Add("regular");reader.regularChuffConfig.parent.localPosition=new(.4f,.3f,2);
   reader.waterChuffConfig.parent=root.Add("water");reader.ashChuffConfig.parent=root.Add("ash");
   reader.chuffLoops=new[]{new ChuffClipsSimReader.ChuffLoop{chuffLoop=root.Add("loop").gameObject.AddComponent<LayeredAudio>()}};
   var outlet=new GameObject("tender exhaust").transform;outlet.localPosition=new(i,2,-i);outlet.localRotation=Quaternion.Euler(0,i%360,0);
   ArticulatedEffects.Seat(reader,outlet);
   Check((reader.regularChuffConfig.parent.position-outlet.position).magnitude<.0001f,"actual emitter seated despite nested/pivot offset");
   Check(root.parent==outlet&&(root.localScale-scale).magnitude<.00001f,"owned hierarchy moved with scale preserved");
   Check(reader.chuffLoops[0].chuffLoop.transform.IsChildOf(root),"loop remains owned");
   reader.ashChuffConfig.parent=container;bool rejected=false;var prior=root.parent;
   try{ArticulatedEffects.Seat(reader,new GameObject("other").transform);}catch(InvalidOperationException){rejected=true;}
   Check(rejected&&root.parent==prior,"foreign emitter rejected before moving hierarchy");
   reader.ashChuffConfig.parent=root;reader.chuffLoops[0].chuffLoop=container.Add("foreign loop").gameObject.AddComponent<LayeredAudio>();rejected=false;
   try{ArticulatedEffects.Seat(reader,outlet);}catch(InvalidOperationException){rejected=true;}
   Check(rejected,"foreign native loop cannot be moved");
  }
  File.WriteAllText(args[0],JsonSerializer.Serialize(new{passed=true,checks,scope="Actual extracted seating, target selection, reset and tender-change source with managed scene/audio data stand-ins; not native Init/Deinit, audible playback or Unity clone serialization."},new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(checks+" tender audio functional checks passed");
 }
}
