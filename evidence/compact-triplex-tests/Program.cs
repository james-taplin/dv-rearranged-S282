using System;using System.IO;using System.Linq;using System.Text.Json;using UnityEngine;using RearrangedS282;
internal static class Program {
 static int checks;static void Check(bool value,string label){if(!value)throw new Exception(label);checks++;}
 static void Main(string[] args){var owner=new WheelRearranger();var v=owner.Inspect(true,true);var older=owner.Inspect(true,false);var truckless=owner.Inspect(false,false);
 Console.WriteLine($"measured {v.Radius} prior {older.Radius} truckless {truckless.Radius}");
 Check(v.Scale>.7f&&v.Scale<1f,"Practical compact-engine scale");Check(2*v.Radius>1.1f&&2*v.Radius<1.4f,"Drivers retain useful diameter");
 Check(Math.Abs(.712f*older.Scale-.599606f)<.0001f,"Measured native inputs reproduce prior live triplex radius");
 Check(Math.Abs(.712f*truckless.Scale-.6477137f)<.0001f,"Measured native inputs reproduce prior live truckless radius");
 float radius=.7121296f,first=8.0018787f,main=6.3025618f,casingRear=9.49f-.595789015f;
 for(int e=0;e<3;e++){
  float a=v.Offsets[e]+v.Scale*first,b=v.Offsets[e]+v.Scale*main;
  Check(a-b>2*v.Radius+.15f,"Coupled driver rims clear each other");
  Check(v.Scale*(casingRear-v.Shorten)-v.Scale*(first+radius)>.05f,"Front driver clears shortened cylinder rear face");
  Check(Math.Abs(v.Cylinders(e).MultiplyPoint3x4(new Vector3(0,0,10.085789f)).z-v.CylinderFronts[e])<.0001f,"Retarget cylinder map matches layout front");
  Check(a>b,"Driver order retained");
  if(e<2)Check(b-v.Radius-v.CylinderFronts[e+1]>.075f,"Separate engines leave clearance");
 }
 Check(v.FrontPinZ>v.MiddlePinZ,"Native articulation pins ordered");
 var report=new{passed=true,checks,driverDiameterM=2*v.Radius,cylinderFronts=v.CylinderFronts,mainDrivers=v.Drivers,scale=v.Scale,limitations="Actual extracted layout calculation with managed transform math. Native fixture values reproduce earlier live radii. Does not render rods, run Unity constraints or prove all mesh clearances."};
 File.WriteAllText(args[0],JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(report));
 }
}
