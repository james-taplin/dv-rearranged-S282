using System;
using DV.ECS.Train;
using RearrangedS282;

internal static class InputContractProbe
{
    static int checks;
    static void Reject(Action action, string name)
    {
        try { action(); } catch(ArgumentException) { checks++; return; }
        throw new Exception("Invalid native input accepted: " + name);
    }
    static void Main()
    {
        var input = new WheelslipControllerSystem.WheelslipInputData {generatedForce=100,numberOfPoweredAxles=2,maxWheelslipRpm=370,wheelslipToAdhesionDrop=1};
        var shared = new AdhesionControllerSystem.StaticSharedAdhesionCalculationData {numOfAxles=6,bogieCount=2};
        var grip = new AdhesionControllerSystem.AdhesionWheelslipOutputData {wheelslipForceLimitPerAxle=1000};
        var slide = new AdhesionControllerSystem.WheelSlideData();
        NativeSlipEntitySchema.ValidateGroupInputs(input,shared,grip,slide); checks++;
        foreach(float force in new[]{-100f,0f,100f}) { input.generatedForce=force; NativeSlipEntitySchema.ValidateGroupInputs(input,shared,grip,slide); checks++; }
        foreach(float bad in new[]{float.NaN,float.PositiveInfinity,float.NegativeInfinity})
        {
            var broken=input; broken.generatedForce=bad;
            Reject(()=>NativeSlipEntitySchema.ValidateGroupInputs(broken,shared,grip,slide),"non-finite torque demand");
            var badGrip=grip; badGrip.wheelslipForceLimitPerAxle=bad;
            Reject(()=>NativeSlipEntitySchema.ValidateGroupInputs(input,shared,badGrip,slide),"non-finite grip");
            var badSlide=slide; badSlide.wheelSlide=bad;
            Reject(()=>NativeSlipEntitySchema.ValidateGroupInputs(input,shared,grip,badSlide),"non-finite host slide");
        }
        var unpowered=input; unpowered.numberOfPoweredAxles=0;
        Reject(()=>NativeSlipEntitySchema.ValidateGroupInputs(unpowered,shared,grip,slide),"missing contacts");
        var overcount=input; overcount.numberOfPoweredAxles=7;
        Reject(()=>NativeSlipEntitySchema.ValidateGroupInputs(overcount,shared,grip,slide),"group exceeds native allocation");
        var negativeBrake=input; negativeBrake.brakingForce=-1;
        Reject(()=>NativeSlipEntitySchema.ValidateGroupInputs(negativeBrake,shared,grip,slide),"negative brake budget");
        var missingBogie=shared; missingBogie.bogieCount=0;
        Reject(()=>NativeSlipEntitySchema.ValidateGroupInputs(input,missingBogie,grip,slide),"missing native bogie");
        var impossibleSlide=slide; impossibleSlide.wheelSlide=1.1f;
        Reject(()=>NativeSlipEntitySchema.ValidateGroupInputs(input,shared,grip,impossibleSlide),"slide outside native range");
        var badDrop=input; badDrop.wheelslipToAdhesionDrop=-.1f;
        Reject(()=>NativeSlipEntitySchema.ValidateGroupInputs(badDrop,shared,grip,slide),"negative adhesion-drop value");
        Console.WriteLine("PASS " + checks + " native input-contract checks; no ECS World execution");
    }
}
