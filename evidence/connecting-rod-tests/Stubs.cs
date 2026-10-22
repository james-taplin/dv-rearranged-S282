using UnityEngine;
namespace UnityEngine
{
    public sealed partial class Transform
    {
        public bool IsChildOf(Transform other){for(var p=this;p!=null;p=p.parent)if(ReferenceEquals(p,other))return true;return false;}
    }
}
