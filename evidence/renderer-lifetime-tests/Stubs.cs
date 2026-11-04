using System;
using System.Collections.Generic;
using System.Linq;
namespace UnityEngine {
 public class Object {
  internal bool destroyed;
  private static readonly List<Object> pending=new List<Object>();
  public static void Destroy(Object value){if(value!=null)pending.Add(value);}
  public static void Flush(){foreach(var item in pending){item.destroyed=true;if(item is GameObject go)foreach(var r in go.GetComponentsInChildren<Renderer>(true))r.destroyed=true;}pending.Clear();}
  public static GameObject Instantiate(GameObject source,Transform parent,bool worldPositionStays){var result=new GameObject();result.transform.parent=parent;return result;}
 }
 public class Component:Object {public GameObject gameObject;public Transform transform=>gameObject.transform;public T GetComponent<T>()where T:Component=>gameObject.GetComponent<T>();public T[] GetComponentsInChildren<T>(bool inactive=false)where T:Component=>gameObject.GetComponentsInChildren<T>(inactive);public T GetComponentInChildren<T>(bool inactive=false)where T:Component=>GetComponentsInChildren<T>(inactive).FirstOrDefault();}
 public class MonoBehaviour:Component {}
 public sealed class GameObject:Object {
  public readonly Transform transform;internal readonly List<Component> components=new List<Component>();public bool activeSelf=true;
  public GameObject(){transform=new Transform{gameObject=this};components.Add(transform);}
  public bool activeInHierarchy=>activeSelf&&(transform.parent==null||transform.parent.gameObject.activeInHierarchy);
  public T Add<T>()where T:Component,new(){var item=new T{gameObject=this};components.Add(item);return item;}
  public T GetComponent<T>()where T:Component=>components.OfType<T>().FirstOrDefault();
  public T[] GetComponentsInChildren<T>(bool inactive=false)where T:Component {IEnumerable<T> Walk(Transform root){if(!inactive&&!root.gameObject.activeInHierarchy)yield break;foreach(var item in root.gameObject.components.OfType<T>())yield return item;foreach(var child in root.children)foreach(var item in Walk(child))yield return item;}return Walk(transform).ToArray();}
  public void SetActive(bool value){activeSelf=value;}
 }
 public sealed class Transform:Component {internal readonly List<Transform> children=new List<Transform>();private Transform owner;public Transform parent{get=>owner;set{owner?.children.Remove(this);owner=value;owner?.children.Add(this);}}public Vector3 position;public Quaternion rotation;public bool IsChildOf(Transform root){for(var t=this;t!=null;t=t.parent)if(ReferenceEquals(root,t))return true;return false;}}
 public class Renderer:Component {private Material material;public Material sharedMaterial{get{if(destroyed)throw new InvalidOperationException("Destroyed renderer read");return material;}set{if(destroyed)throw new InvalidOperationException("Destroyed renderer write");material=value;}}}
 public sealed class MeshRenderer:Renderer {}
 public sealed class Material:Object {}
 public sealed class Mesh:Object {}
 public sealed class MeshFilter:Component {public Mesh sharedMesh;}
 public struct LOD {}
 public sealed class LODGroup:Component {public Vector3 localReferencePoint;public float size;public void SetLODs(LOD[] lods){}}
 public struct Vector3 {}
 public struct Quaternion {}
 public sealed class SerializeField:Attribute {}
 public static class Debug {public static void LogError(object text,object context=null){throw new InvalidOperationException(text.ToString());}}
}
public sealed class TrainCar:UnityEngine.Component {public static object Resolve(UnityEngine.GameObject go)=>null;}
