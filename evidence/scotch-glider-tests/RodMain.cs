    private static void Main(string[] args)
    {
        using var rods=JsonDocument.Parse(File.ReadAllText(args[0]));
        using var animation=JsonDocument.Parse(File.ReadAllText(args[1]));
        using var wheels=JsonDocument.Parse(File.ReadAllText(args[2]));
        using var geometry=JsonDocument.Parse(File.ReadAllText(args[3]));
        float scale=geometry.RootElement.GetProperty("driverScale").GetSingle();
        float offset=geometry.RootElement.GetProperty("driveOffsetZ").GetSingle();
        int cases=0;float maxAxial=0,maxRadial=0,maxAddedError=0;
        foreach(bool rotated in new[]{false,true})
        {
            var baseline=new Rig(rods.RootElement,animation.RootElement,wheels.RootElement,rotated,false);
            var candidate=new Rig(rods.RootElement,animation.RootElement,wheels.RootElement,rotated,false);
            baseline.Legacy(44);candidate.Legacy(44);
            foreach(var drive in candidate.sides){drive.localScale=new Vector3(drive==candidate.owner.driveL?-1:1,scale,scale);drive.localPosition=new Vector3(0,0,offset);}
            foreach(var clip in animation.RootElement.GetProperty("clips").EnumerateArray())
            foreach(var frame in clip.GetProperty("frames").EnumerateArray())
            {
                baseline.Pose(clip,frame);candidate.Pose(clip,frame);cases++;
                for(int side=0;side<2;side++)
                {
                    Vector3 MainAnchor(Rig rig)
                    {
                        var drive=rig.sides[side];var main=drive.Find("s282_mech_wheels_connect/s282_mech_push_rod_to_connect");var crank=drive.Find("s282_wheels_driving_3");
                        return rig.owner.transform.InverseTransformPoint(main.position)-rig.owner.transform.InverseTransformPoint(crank.position);
                    }
                    var oldMain=MainAnchor(baseline);var newMain=MainAnchor(candidate);
                    Check(Math.Abs(newMain.y/scale-oldMain.y/1.2879f)<.0001f && Math.Abs(newMain.z/scale-oldMain.z/1.2879f)<.0001f,
                        "Native main-rod crank anchor phase changes under Scotch scaling");
                }
                for(int side=0;side<2;side++) foreach(bool front in new[]{false,true})
                {
                    Transform Rod(Rig r)=>side==0?(front?r.owner.lfThreeAxleSideRod:r.owner.lrThreeAxleSideRod):(front?r.owner.rfThreeAxleSideRod:r.owner.rrThreeAxleSideRod);
                    var oldRod=Rod(baseline);var newRod=Rod(candidate);
                    float[] pinZ={-3.105580162f,-1.55294514f};float[] pinY={-.00002527875f,-.000025020586f};int[] assigned=front?new[]{2,3}:new[]{4,3};
                    var oldDrive=oldRod.parent.parent;var newDrive=newRod.parent.parent;
                    for(int p=0;p<2;p++)
                    {
                        Vector3 Error(Rig rig,Transform rod,Transform drive)
                        {
                            var bore=rod.TransformPoint(new Vector3(0,pinY[p],pinZ[p]));
                            var main=drive.Find("s282_mech_wheels_connect/s282_mech_push_rod_to_connect");
                            var centre=drive.Find("s282_wheels_driving_3");var target=drive.Find("s282_wheels_driving_"+assigned[p]);
                            return rig.owner.transform.InverseTransformPoint(bore)-rig.owner.transform.InverseTransformPoint(target.position+(main.position-centre.position));
                        }
                        var oldError=Error(baseline,oldRod,oldDrive);var newError=Error(candidate,newRod,newDrive);
                        // The drive frame scales Y/Z only; X remains native.
                        float added=(float)Math.Sqrt(Math.Pow(newError.y/scale-oldError.y/1.2879f,2)+Math.Pow(newError.z/scale-oldError.z/1.2879f,2));
                        maxAddedError=Math.Max(maxAddedError,added);maxAxial=Math.Max(maxAxial,Math.Abs(newError.z));maxRadial=Math.Max(maxRadial,(float)Math.Sqrt(newError.y*newError.y+newError.z*newError.z));
                        Check(added<.0001f,"Uniform growth changed the native coupling bore/pin relationship");
                    }
                }
            }
        }
        var report=new {passed=true,arrangementId=49,checks,nativePoseScenarios=cases,maximumAxialErrorM=maxAxial,maximumBorePinErrorM=maxRadial,maximumAddedNormalizedErrorM=maxAddedError,
            scope="Native three-axle rod recipes, wheel/rod hierarchy and sampled animation clips in managed transforms at the source-derived Scotch frame; compares actual bore centres to the native crank anchor and Pacific baseline. Does not establish full main rod/cylinder collision, renderer, gameplay or integrated caller."};
        File.WriteAllText(args[4],JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(report));
    }
}
