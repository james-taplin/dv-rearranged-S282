using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class SlipWorldBootstrap
{
    const string Armed = "S282.NativeSlipWorld.Armed";
    static SlipWorldBootstrap() { EditorApplication.update += Tick; }
    public static void Start()
    {
        SessionState.SetBool(Armed,true);
        EditorApplication.isPlaying=true;
    }
    public static void BuildPlayer()
    {
        try
        {
            string root=Directory.GetParent(Application.dataPath).Parent.FullName;
            Type.GetType("GroupAnimationFixtureEditor, Assembly-CSharp-Editor",true).GetMethod("Prepare").Invoke(null,null);
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var type=Type.GetType("SlipWorldPlayerBootstrap, S282.NativeSlipBootstrap",true);
            new GameObject("Native slip World fixture").AddComponent(type);
            const string scenePath="Assets/NativeSlipWorld.unity";
            if(!EditorSceneManager.SaveScene(scene,scenePath)) throw new Exception("Cannot save private fixture scene.");
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone,ScriptingImplementation.Mono2x);
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Standalone,ManagedStrippingLevel.Disabled);
            PlayerSettings.stripEngineCode=false;
            string player=Path.Combine(root,"player","NativeSlipWorld.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(player));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{scenePath},locationPathName=player,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Private fixture player build failed: "+report.summary.result);
            EditorApplication.Exit(0);
        }
        catch(Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }
    static void Tick()
    {
        if(!EditorApplication.isPlaying || EditorApplication.isCompiling || !SessionState.GetBool(Armed,false)) return;
        SessionState.SetBool(Armed,false);
        string root=Directory.GetParent(Application.dataPath).Parent.FullName;
        string managed=File.ReadAllText(Path.Combine(root,"managed-path.txt")).Trim();
        AppDomain.CurrentDomain.AssemblyResolve += (sender,args) => {
            var candidate=Path.Combine(managed,new AssemblyName(args.Name).Name+".dll");
            return File.Exists(candidate)?Assembly.LoadFrom(candidate):null;
        };
        try
        {
            // Native TypeManager discovers these by loaded assembly name;
            // dependency resolution alone does not load unreferenced facades.
            Assembly.LoadFrom(Path.Combine(managed,"UnityEngine.dll"));
            Assembly.LoadFrom(Path.Combine(managed,"Unity.Entities.Hybrid.dll"));
            var assembly=Assembly.LoadFrom(Path.Combine(root,"probe-bin","NativeSlipWorldProbe.dll"));
            assembly.GetType("NativeSlipWorldProbe").GetMethod("Run").Invoke(null,new object[]{Path.Combine(root,"result.json")});
            EditorApplication.Exit(0);
        }
        catch(Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }
}
