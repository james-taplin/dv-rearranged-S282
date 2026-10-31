using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;

// Reuses the private scene's existing script identity. Never installed in DV.
public sealed class SlipWorldPlayerBootstrap : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return null;
        string root = Directory.GetParent(Application.dataPath).Parent.FullName;
        string managed = File.ReadAllText(Path.Combine(root, "managed-path.txt")).Trim();
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
            string path = Path.Combine(managed, new AssemblyName(args.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        IEnumerator routine = null;
        Exception failure = null;
        try
        {
            Assembly.LoadFrom(Path.Combine(managed, "UnityEngine.dll"));
            Assembly.LoadFrom(Path.Combine(managed, "Unity.Entities.Hybrid.dll"));
            Assembly.LoadFrom(Path.Combine(managed, "Assembly-CSharp.dll"));
            Assembly.LoadFrom(Path.Combine(managed, "Unity.Transforms.dll"));
            Assembly.LoadFrom(Path.Combine(managed, "Unity.Entities.dll")).GetType("Unity.Entities.TypeManager", true)
                .GetMethod("Initialize").Invoke(null, null);
            string[] config = File.ReadAllLines(Path.Combine(root, "probe.txt"));
            Assembly probe = Assembly.LoadFrom(Path.Combine(root, "probe-bin", config[0]));
            object result = probe.GetType(config[1], true).GetMethod("Run", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new object[] { Path.Combine(root, "result.json") });
            routine = result as IEnumerator;
        }
        catch (Exception ex) { failure = ex; }
        while (failure == null && routine != null)
        {
            bool next = false; object current = null;
            try { next = routine.MoveNext(); if (next) current = routine.Current; }
            catch (Exception ex) { failure = ex; }
            if (!next || failure != null) break;
            yield return current;
        }
        if (failure != null)
        {
            File.WriteAllText(Path.Combine(root, "bootstrap-error.txt"), failure.ToString());
            Debug.LogException(failure);
        }
        Application.Quit(failure == null ? 0 : 1);
    }
}
