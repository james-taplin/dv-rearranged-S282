using System;
using System.Linq;
using UnityEngine;
namespace UnityEngine
{
    public sealed partial class Mesh
    {
        public int[] GetTriangles(int section){if(section!=0)throw new InvalidOperationException("Native linkage fixture has one submesh");return triangles.ToArray();}
        public void SetTriangles(int[] value,int section){if(section!=0)throw new InvalidOperationException("Native linkage fixture has one submesh");triangles=value.ToArray();}
    }
    public partial struct Vector3
    {
        public static Vector3 Min(Vector3 a,Vector3 b)=>new(Math.Min(a.x,b.x),Math.Min(a.y,b.y),Math.Min(a.z,b.z));
        public static Vector3 Max(Vector3 a,Vector3 b)=>new(Math.Max(a.x,b.x),Math.Max(a.y,b.y),Math.Max(a.z,b.z));
    }
    public partial struct Quaternion
    {
        public static Quaternion Inverse(Quaternion q)=>From(System.Numerics.Quaternion.Inverse(q.Native));
        public static Quaternion operator *(Quaternion a,Quaternion b)=>From(a.Native*b.Native);
        public static Vector3 operator *(Quaternion q,Vector3 v)=>Vector3.From(System.Numerics.Vector3.Transform(v.Native,q.Native));
    }
    public sealed partial class Transform
    {
        public bool IsChildOf(Transform other){for(var p=this;p!=null;p=p.parent)if(ReferenceEquals(p,other))return true;return false;}
    }
    public partial struct Matrix4x4
    {
        public float this[int row,int col]
        {
            get => GetTransposed(row,col);
            set => SetTransposed(row,col,value);
        }
        float GetTransposed(int row,int col)=>((row,col)) switch {
            (0,0)=>n.M11,(0,1)=>n.M21,(0,2)=>n.M31,(0,3)=>n.M41,
            (1,0)=>n.M12,(1,1)=>n.M22,(1,2)=>n.M32,(1,3)=>n.M42,
            (2,0)=>n.M13,(2,1)=>n.M23,(2,2)=>n.M33,(2,3)=>n.M43,
            (3,0)=>n.M14,(3,1)=>n.M24,(3,2)=>n.M34,(3,3)=>n.M44,_=>throw new IndexOutOfRangeException()};
        void SetTransposed(int row,int col,float v){switch(row*4+col){case 0:n.M11=v;break;case 1:n.M21=v;break;case 2:n.M31=v;break;case 3:n.M41=v;break;case 4:n.M12=v;break;case 5:n.M22=v;break;case 6:n.M32=v;break;case 7:n.M42=v;break;case 8:n.M13=v;break;case 9:n.M23=v;break;case 10:n.M33=v;break;case 11:n.M43=v;break;case 12:n.M14=v;break;case 13:n.M24=v;break;case 14:n.M34=v;break;case 15:n.M44=v;break;default:throw new IndexOutOfRangeException();}}
    }
}
