// Managed lifecycle model of the documented Unity 2019 contract; not a Unity player.
using System;
using System.Collections.Generic;
using System.Linq;
using N = System.Numerics;
namespace UnityEngine
{
    public class Object { public string name; }
    public class Component : Object
    {
        public GameObject gameObject;
        public new string name { get => gameObject?.name ?? base.name; set { if(gameObject != null) gameObject.name=value; else base.name=value; } }
        public Transform transform => gameObject.transform;
        public T[] GetComponents<T>() => gameObject.components.OfType<T>().ToArray();
        public T GetComponent<T>() => gameObject.components.OfType<T>().FirstOrDefault();
        public T[] GetComponentsInChildren<T>(bool inactive) => gameObject.GetComponentsInChildren<T>(inactive);
    }
    public class MonoBehaviour : Component { public bool enabled = true; public bool isActiveAndEnabled => enabled && gameObject.activeInHierarchy; }
    public class GameObject : Object
    {
        public readonly Transform transform;
        public readonly List<Component> components = new();
        public bool activeSelf = true;
        public bool activeInHierarchy => activeSelf && (transform.parent?.gameObject.activeInHierarchy ?? true);
        public GameObject(string name) { this.name = name; transform = new Transform { gameObject = this }; }
        public T Add<T>() where T : Component, new() { var c = new T { gameObject = this }; components.Add(c); return c; }
        public T AddComponent<T>() where T : Component, new() => Add<T>();
        public T GetComponent<T>() => components.OfType<T>().FirstOrDefault();
        public T[] GetComponentsInChildren<T>(bool inactive) => components.OfType<T>()
            .Concat(transform.children.SelectMany(c => c.gameObject.GetComponentsInChildren<T>(inactive)))
            .Where(c => inactive || c is not Component item || item.gameObject.activeInHierarchy).ToArray();
        public void SetActive(bool active)
        {
            activeSelf = active;
            if (!activeInHierarchy) foreach (var a in GetComponentsInChildren<Animator>(true)) a.Disabled();
        }
    }
    public class Transform : Component, System.Collections.IEnumerable
    {
        public Transform parent;
        public readonly List<Transform> children = new();
        public Quaternion localRotation = Quaternion.Euler(0);
        public void SetParent(Transform value)
        {
            parent?.children.Remove(this); parent = value; value?.children.Add(this);
            if (!gameObject.activeInHierarchy) foreach (var a in gameObject.GetComponentsInChildren<Animator>(true)) a.Disabled();
        }
        public System.Collections.IEnumerator GetEnumerator() => children.GetEnumerator();
        public Transform Find(string name) { var path=name.Split(new[]{'/'},2); var child=children.FirstOrDefault(c=>c.gameObject.name==path[0]); return path.Length==1?child:child?.Find(path[1]); }
    }
    public class Mesh : Object { }
    public class MeshFilter : Component { public Mesh sharedMesh; }
    public class MeshRenderer : Component { public bool enabled=true; }
    public struct LOD { public MeshRenderer[] renderers; public LOD(MeshRenderer[] renderers){this.renderers=renderers;} }
    public class LODGroup : Component { private LOD[] levels=Array.Empty<LOD>(); public LOD[] GetLODs()=>levels; public void SetLODs(LOD[] value)=>levels=value; }
    public struct Quaternion
    {
        private N.Quaternion value;
        public static Quaternion Euler(float angle) => new() { value = N.Quaternion.CreateFromAxisAngle(N.Vector3.UnitX, angle * MathF.PI / 180f) };
        public static float Angle(Quaternion a, Quaternion b) => 2 * MathF.Acos(MathF.Min(1f, MathF.Abs(N.Quaternion.Dot(a.value, b.value)))) * 180f / MathF.PI;
    }
    public static class Mathf
    {
        public static float Min(float a, float b) => MathF.Min(a, b);
        public static float Abs(float a) => MathF.Abs(a);
        public static float Sign(float a) => a < 0 ? -1 : 1;
    }
    public class RuntimeAnimatorController : Object { public int State; }
    public struct AnimatorStateInfo { public int fullPathHash; public float normalizedTime; }
    public class Animator : MonoBehaviour
    {
        public RuntimeAnimatorController runtimeAnimatorController;
        public bool keepAnimatorControllerStateOnDisable;
        public bool isInitialized = true;
        public AnimatorStateInfo state;
        private AnimatorStateInfo? queued;
        public int plays;
        public readonly Dictionary<int, float> floats = new();
        public static int StringToHash(string name) { unchecked { int h = 17; foreach (char c in name) h = h * 31 + c; return h; } }
        public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layer) => state;
        public void Play(int hash, int layer, float time)
        {
            if (hash != runtimeAnimatorController.State) throw new Exception("Wrong controller state");
            plays++; queued = new() { fullPathHash = hash, normalizedTime = time };
        }
        public void SetFloat(int name, float value) => floats[name] = value;
        public void Evaluate(float cycles = 0)
        {
            if (!isActiveAndEnabled) return;
            if (queued.HasValue) { state = queued.Value; queued = null; }
            if (state.fullPathHash == 0) state.fullPathHash = runtimeAnimatorController.State;
            state.normalizedTime += cycles;
            foreach (var wheel in transform.children.Where(c => c.gameObject.name.StartsWith("s282_wheels_driving_")))
                wheel.localRotation = Quaternion.Euler(state.normalizedTime * 360f);
        }
        internal void Disabled() { if (!keepAnimatorControllerStateOnDisable) { state = default; queued = null; } }
    }
}
namespace DV.Wheels
{
    public class PoweredWheelRotationViaAnimation : UnityEngine.Component
    {
        public class AnimatorStartTimeOffsetPair { public UnityEngine.Animator animator; public float startTimeOffset; }
        public AnimatorStartTimeOffsetPair[] animatorSetups;
    }
}
namespace DV.Simulation.Ports
{
    public class Port { public float Value; }
    public class Flow { public bool TryGetPort(string id, out Port port) { port = new Port(); return true; } }
    public class AnimatorPortReader : UnityEngine.MonoBehaviour
    {
        public string parameterName, portId;
        public int updateType;
        public float valueMultiplier, valueOffset;
        public bool bound;
        public void Init(Port p) => bound = true;
        public void Deinit() => bound = false;
    }
}
namespace RearrangedS282
{
    using UnityEngine;
    using DV.Wheels;
    using DV.Simulation.Ports;
    // Boundary to the independent native phase owner, exercised separately by
    // the native mechanism harness. This fixture checks its tender fallback.
    internal static class GroupCrankAnimation
    {
        internal static readonly HashSet<Animator> Owned = new();
        internal static bool Owns(Animator animator) => Owned.Contains(animator);
    }
    internal static class AssetBundleLoader
    {
        public static RuntimeAnimatorController FranklinBAnimCtrl = new() { State = DriveQuartering.FranklinState };
        public static RuntimeAnimatorController FranklinBAnimCtrl4444 = new() { State = DriveQuartering.Franklin4444State };
    }
    internal class Engine { public Port chuffFrequencyReadOut = new(), crankRpm = new(); public int numCylinders = 2; }
    internal class ArticulatedSimulation { public bool IsTenderEngine = true, IsMiddleActive = true; public Engine MiddleEngine = new(), MainEngine = new(); public Flow Flow = new(); }
    internal partial class WheelRearranger
    {
        private Component loco;
        private Animator lfAnimator, rfAnimator;
        private RuntimeAnimatorController lfDefaultAnimatorCtrl, rfDefaultAnimatorCtrl;
        private Transform tenderDriveL, tenderDriveR;
        private bool poweredTenderSelected;
        private WheelArrangementType currentWA;
        private T[] GetComponentsInChildren<T>(bool inactive) => loco.GetComponentsInChildren<T>(inactive);
        internal WheelRearranger(GameObject car, Animator l, Animator r)
        {
            loco = car.Add<Component>(); lfAnimator = l; rfAnimator = r;
            lfDefaultAnimatorCtrl = l.runtimeAnimatorController; rfDefaultAnimatorCtrl = r.runtimeAnimatorController;
            CacheNativeQuartering();
        }
        internal float Capture() => CaptureQuarteringCycle();
        internal void Apply(float cycle, bool keep = true) => ApplyDriverQuartering(cycle, keep);
        internal void Arrangement(int id, Animator tenderL = null, Animator tenderR = null)
        {
            currentWA = (WheelArrangementType)id; poweredTenderSelected = tenderL != null;
            tenderDriveL = tenderL?.transform; tenderDriveR = tenderR?.transform;
        }
    }
}
