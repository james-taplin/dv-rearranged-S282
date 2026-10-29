using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class GroupAnimationFixtureEditor
{
    public static void Prepare()
    {
        System.IO.Directory.CreateDirectory("Assets/Resources");
        var clip=new AnimationClip {name="Group phase fixture",frameRate=60};
        clip.SetCurve("wheel",typeof(Transform),"localEulerAnglesRaw.z",AnimationCurve.Linear(0,0,1,360));
        var settings=AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime=true;
        AnimationUtility.SetAnimationClipSettings(clip,settings);
        AssetDatabase.CreateAsset(clip,"Assets/Resources/GroupPhase.anim");
        var controller=AnimatorController.CreateAnimatorControllerAtPath("Assets/Resources/GroupPhase.controller");
        controller.AddParameter("SpeedMultiplier",AnimatorControllerParameterType.Float);
        var state=controller.layers[0].stateMachine.AddState("Drive");
        state.motion=clip; state.speedParameter="SpeedMultiplier"; state.speedParameterActive=true;
        controller.layers[0].stateMachine.defaultState=state;
        AssetDatabase.SaveAssets();
    }
}
