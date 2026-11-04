using UnityEngine;
namespace RearrangedS282 {
    internal sealed class SplitFixture {
        internal Vector3 Cylinders;
        internal void MoveCylinders(Vector3 p) { Cylinders=p; }
    }
    internal partial class WheelRearranger {
        internal SplitFixture splitS282A=new SplitFixture();
        internal Transform lfCrossheadBracket,rfCrossheadBracket;
        internal void ApplyScotchFixture()=>ApplyScotchGliderGeometry();
    }
}
