using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;

// Separate asmdef prevents this fixture from defining an Assembly-CSharp that
// would collide with the installed game's dynamically loaded assembly.
public sealed class SlipWorldPlayerBootstrap : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return null;
        string root=Directory.GetParent(Application.dataPath).Parent.FullName;
        string managed=File.ReadAllText(Path.Combine(root,"managed-path.txt")).Trim();
        AppDomain.CurrentDomain.AssemblyResolve += (sender,args) => {
            var candidate=Path.Combine(managed,new AssemblyName(args.Name).Name+".dll");
            return File.Exists(candidate)?Assembly.LoadFrom(candidate):null;
        };
        IEnumerator animation=null;
        Exception failure=null;
        try
        {
            Assembly.LoadFrom(Path.Combine(managed,"UnityEngine.dll"));
            Assembly.LoadFrom(Path.Combine(managed,"Unity.Entities.Hybrid.dll"));
            var assembly=Assembly.LoadFrom(Path.Combine(root,"probe-bin","NativeSlipWorldProbe.dll"));
            assembly.GetType("NativeSlipWorldProbe").GetMethod("Run").Invoke(null,new object[]{Path.Combine(root,"result.json")});
            animation=(IEnumerator)assembly.GetType("GroupAnimationProbe").GetMethod("Run").Invoke(null,new object[]{Path.Combine(root,"animation-result.json")});
        }
        catch(Exception error)
        {
            File.WriteAllText(Path.Combine(root,"bootstrap-error.txt"),error.ToString());
            Debug.LogException(error); failure=error;
        }
        if(failure!=null) { Application.Quit(1); yield break; }
        while(true)
        {
            bool next=false; object current=null;
            try { next=animation.MoveNext(); if(next)current=animation.Current; }
            catch(Exception error) { File.WriteAllText(Path.Combine(root,"bootstrap-error.txt"),error.ToString()); Debug.LogException(error); failure=error; }
            if(failure!=null) { Application.Quit(1); yield break; }
            if(!next)break;
            yield return current;
        }
        Application.Quit(0);
    }
}
