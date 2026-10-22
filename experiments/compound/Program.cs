using System;
using System.IO;
using System.Text.Json;
using RearrangedS282.Experiments;
using Reference=RearrangedS060.MeyerCompoundCycle;
internal static class Program
{
 static int checks;
 static void Check(bool ok,string label){checks++;if(!ok)throw new Exception(label);}
 static CompoundCyclePrototype New(int lp=1)=>new(.42,.594,.6707097,.2,.4,2,.25,lp);
 static void Step(CompoundCyclePrototype c,double[] rpm,double throttle=1,double dt=.02,double reverse=.6,double budget=double.PositiveInfinity,double cocks=0)=>c.Advance(dt,16,200,throttle,reverse,80,rpm,cocks,false,false,.05,.9,budget);
 static void Balanced(CompoundCyclePrototype c,bool pressureBound=true)
 {
  Check(Math.Abs(c.MassResidual)<1e-8,"mass ledger");
  Check(Math.Abs(c.EnergyResidual)<Math.Max(.00001,c.BoilerEnergy*1e-10),"energy ledger");
  Check(c.Work>=0&&c.StoredMass>=0&&double.IsFinite(c.StoredEnergy),"finite nonnegative state");
  Check(c.Work<=c.BoilerEnergy+1e-8,"work cannot exceed supplied energy");
  if(pressureBound)Check(c.MaximumTransferPressureExcessPa<1e-5,"no transfer across reversed pressure gradient");
 }
 static void Main(string[] args)
 {
  var p=new CompoundCyclePrototype(.42,.594,.6707097,.2,.4,2,.25,enforceTransferPressure:false);var reference=new Reference(.42,.594,.6707097,.2,.4,2);
  double[] same={80};
  for(int i=0;i<4000;i++)
  {
   double throttle=i<3000?1:0,cocks=i%300<20?1:0;
   Step(p,same,throttle:throttle,cocks:cocks);
   reference.Advance(.02,16,200,throttle,.6,80,cocks,false,false,.05,.9);
   Check(Math.Abs(p.BoilerMass-reference.BoilerMass)<1e-10,"reuse reference boiler demand");
   Check(Math.Abs(p.Lp.Torque-reference.Lp.Torque)<1e-7,"reuse reference LP torque");
   Balanced(p,pressureBound:false);
  }
  var empty=New(2);for(int i=0;i<1000;i++)Step(empty,new double[]{200,400},throttle:0);
  Check(empty.Work==0&&empty.StoredMass==0&&empty.BoilerMass==0,"unfed spinning stages cannot create steam or work");Balanced(empty);
  var a=New(2);var b=New(2);double[] ar={60,240},br={240,60};
  for(int i=0;i<6000;i++)
  {
   Step(a,ar);Step(b,br);Balanced(a);Balanced(b);
   Check(Math.Abs(a.LowPressure[0].Torque-b.LowPressure[1].Torque)<1e-7,"LP ordering symmetry");
  }
  Check(Math.Abs(a.Receiver.Pressure-b.Receiver.Pressure)<1e-7,"receiver independent of LP enumeration");
  Check(Math.Abs(a.LowPressure[0].Exhaust-a.LowPressure[1].Exhaust)>.00001,"independent LP rpm affects its flow");
  var stalled=New();for(int i=0;i<2000;i++)Step(stalled,new double[]{0});
  Console.WriteLine("stalled HPflow="+stalled.Hp.Exhaust+" LPflow="+stalled.Lp.Exhaust+" receiver="+stalled.Receiver.Pressure);Check(stalled.BoilerMass>0&&stalled.Lp.Exhaust==0,"stalled LP does not inherit HP rotation");Balanced(stalled);
  var normal=New();for(int i=0;i<2000;i++)Step(normal,same);
  Check(stalled.Receiver.Pressure>normal.Receiver.Pressure,"stalled LP raises receiver backpressure");
  Check(stalled.Hp.Torque<normal.Hp.Torque,"receiver backpressure reduces HP work");
  double boiler=a.BoilerMass,energy=a.StoredEnergy,work=a.Work;
  for(int i=0;i<3000;i++)Step(a,ar,throttle:0);
  Check(a.BoilerMass==boiler,"closed throttle no fresh draw");
  Check(a.Work-work<=energy+1e-6,"rundown cannot deliver more than stored energy");Balanced(a);
  var limited=New(2);for(int i=0;i<2000;i++){double before=limited.BoilerMass;Step(limited,ar,budget:.0001);Check(limited.BoilerMass-before<=.0001+1e-12,"native supply budget respected");Balanced(limited);}
  var dtA=New(2);var dtB=New(2);for(int i=0;i<3000;i++)Step(dtA,ar);for(int i=0;i<1200;i++)Step(dtB,ar,dt:.05);
  Check(Math.Abs(dtA.BoilerMass-dtB.BoilerMass)<1e-7&&Math.Abs(dtA.StoredEnergy-dtB.StoredEnergy)<.01,"tick subdivision stable");
  var f=New(2);var r=New(2);for(int i=0;i<2000;i++){Step(f,ar);r.Advance(.02,16,200,1,-.6,-80,new double[]{-60,-240},0,false,false,.05,.9);}
  Check(Math.Abs(f.Work-r.Work)<1e-6&&Math.Abs(f.Lp.Torque+r.Lp.Torque)<1e-6,"reverse consistent");
  Check(stalled.Receiver.Pressure <= 16e5+1,"blocked LP cannot pump receiver above supply");
  double mass=limited.BoilerMass;bool rejected=false;try{Step(limited,new double[]{double.NaN,80});}catch(ArgumentException){rejected=true;}
  Check(rejected&&limited.BoilerMass==mass,"invalid input rejected before mutation");
  File.WriteAllText(args[0],JsonSerializer.Serialize(new{passed=true,readyForRuntime=false,receiverPressureLimitPassed=stalled.Receiver.Pressure<=16e5+1,checks,referenceSteps=4000,lowPressureConsumers=2,illustrativeCalibration="S060 reference dimensions only; not S282 balance targets",steadyReceiverBar=normal.Receiver.Pressure/100000,stalledReceiverBar=stalled.Receiver.Pressure/100000,normalHpTorqueNm=normal.Hp.Torque,stalledHpTorqueNm=stalled.Hp.Torque,limitation="Offline ideal-vapour model only; not connected to native S282, no game test"},new JsonSerializerOptions{WriteIndented=true}));
  Console.WriteLine(checks+" compound prototype checks passed");
 }
}

