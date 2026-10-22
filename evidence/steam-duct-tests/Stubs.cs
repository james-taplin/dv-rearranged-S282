using System;
namespace UnityEngine
{
    public struct Vector2
    {
        public float x,y; public Vector2(float a,float b){x=a;y=b;}
        public static Vector2 zero=>new(0,0);
        public static Vector2 operator +(Vector2 a,Vector2 b)=>new(a.x+b.x,a.y+b.y);
        public static Vector2 operator -(Vector2 a,Vector2 b)=>new(a.x-b.x,a.y-b.y);
        public static Vector2 operator *(Vector2 a,float b)=>new(a.x*b,a.y*b);
    }
    public struct Vector3
    {
        public float x,y,z;
        public Vector3(float a,float b,float c){x=a;y=b;z=c;}
        public static Vector3 zero=>new(0,0,0);
        public static Vector3 up=>new(0,1,0);
        public static Vector3 forward=>new(0,0,1);
        public float sqrMagnitude=>Dot(this,this);
        public float magnitude=>(float)Math.Sqrt(sqrMagnitude);
        public static float Dot(Vector3 a,Vector3 b)=>a.x*b.x+a.y*b.y+a.z*b.z;
        public static Vector3 Cross(Vector3 a,Vector3 b)=>new(a.y*b.z-a.z*b.y,a.z*b.x-a.x*b.z,a.x*b.y-a.y*b.x);
        public static Vector3 operator +(Vector3 a,Vector3 b)=>new(a.x+b.x,a.y+b.y,a.z+b.z);
        public static Vector3 operator -(Vector3 a,Vector3 b)=>new(a.x-b.x,a.y-b.y,a.z-b.z);
        public static Vector3 operator -(Vector3 a)=>new(-a.x,-a.y,-a.z);
        public static Vector3 operator *(Vector3 a,float b)=>new(a.x*b,a.y*b,a.z*b);
        public static Vector3 operator /(Vector3 a,float b)=>a*(1/b);
    }
}
