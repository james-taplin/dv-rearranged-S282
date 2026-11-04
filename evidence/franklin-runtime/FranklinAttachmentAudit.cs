using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

// Diagnostic fixture only. Geometric circles are measured from referenced mesh
// vertices; production pivots/declared pin constants are not used as eye centres.
public static partial class FranklinRuntimeProbe
{
    const string AttachmentMainPath="s282_mech_wheels_connect/s282_mech_push_rod_to_connect";
    const string AttachmentCrankPath="s282_mech_wheels_connect/s282_mech_connect_to_cutoff_base";
    const string AttachmentSidePath="s282_mech_wheels_connect/[Rearranged two-axle rod]";
    const float AttachmentCircleTolerance=.00002f;
    sealed class AttachmentCircle
    {
        internal MeshFilter Filter;
        internal Vector3 Centre;
        internal float Radius,Residual,XMin,XMax;
        internal int Inliers,UniquePoints;
        internal string Kind;
        internal bool Resolved;
        internal JObject Report()=>new JObject{["kind"]=Kind,["resolved"]=Resolved,["localCentre"]=JV(Centre),
            ["radiusM"]=Radius,["maximumFitResidualM"]=Residual,["inlierPoints"]=Inliers,["candidatePoints"]=UniquePoints,
            ["axisMinimumX"]=XMin,["axisMaximumX"]=XMax,["mesh"]=Filter==null?null:Filter.sharedMesh.name};
    }
    sealed class AttachmentMetric
    {
        internal string Name;
        internal int Samples;
        internal float MaxRadial,MinRadial=float.PositiveInfinity,MaxAxialGap;
        internal float MinimumAxialOverlapM=float.PositiveInfinity,MinimumEyeAxialCoverage=float.PositiveInfinity;
        internal JObject Worst;
        internal readonly HashSet<string> Matches=new HashSet<string>();
        internal JObject Report()=>new JObject{["metric"]=Name,["samples"]=Samples,["matchedWheels"]=new JArray(Matches.OrderBy(x=>x)),
            ["minimumRadialCentreDistanceM"]=Samples==0?(JToken)JValue.CreateNull():new JValue(MinRadial),
            ["maximumRadialCentreDistanceM"]=Samples==0?(JToken)JValue.CreateNull():new JValue(MaxRadial),
            ["maximumAxialIntervalSeparationM"]=Samples==0?(JToken)JValue.CreateNull():new JValue(MaxAxialGap),
            ["minimumAxialOverlapM"]=Samples==0?(JToken)JValue.CreateNull():new JValue(MinimumAxialOverlapM),
            ["minimumEyeAxialCoverage"]=Samples==0?(JToken)JValue.CreateNull():new JValue(MinimumEyeAxialCoverage),["worst"]=Worst};
    }
    sealed class AttachmentDrive
    {
        internal Transform Drive;
        internal string Label;
        internal AttachmentCircle MainNear,MainFar,Crosshead,Crank,CrankTip,PresentedPin1,PresentedPin2;
        internal readonly List<AttachmentCircle> SideEyes=new List<AttachmentCircle>();
        internal readonly List<AttachmentCircle> Wheels=new List<AttachmentCircle>();
        internal readonly List<AttachmentMetric> Metrics=new List<AttachmentMetric>();
        internal readonly AttachmentMetric MainPinSeating=new AttachmentMetric{Name="main near eye to constrained native pin wheel 2"};
        internal readonly List<AttachmentMetric> SidePinSeating=new List<AttachmentMetric>();
        internal int HiddenCrankSamples;
        internal Vector3 CrossheadInMainMin=new Vector3(float.PositiveInfinity,float.PositiveInfinity,float.PositiveInfinity);
        internal Vector3 CrossheadInMainMax=new Vector3(float.NegativeInfinity,float.NegativeInfinity,float.NegativeInfinity);
    }
    sealed class AttachmentLayout
    {
        internal int Id;
        internal readonly List<AttachmentDrive> Drives=new List<AttachmentDrive>();
    }
    static readonly Dictionary<Layout,AttachmentLayout> attachmentLayouts=new Dictionary<Layout,AttachmentLayout>();

    static Vector3[] AttachmentVertices(MeshFilter filter)
    {
        var mesh=Geometry(filter.sharedMesh);var vertices=mesh.vertices;
        // Indexed rod recipes leave unused vertices; those are not visible eyes.
        return mesh.triangles.Distinct().Select(i=>vertices[i]).ToArray();
    }
    static MeshFilter AttachmentRendered(Transform drive,string path)
    {
        var t=drive.Find(path);return t==null?null:Rendered(t);
    }
    static AttachmentCircle AttachmentFit(MeshFilter filter,string kind,int end=0,bool wheel=false,bool crosshead=false,bool crank=false,bool tip=false,bool presentedPin=false)
    {
        var result=new AttachmentCircle{Filter=filter,Kind=kind};if(filter==null)return result;
        var all=AttachmentVertices(filter);if(all.Length<8)return result;
        float minZ=all.Min(v=>v.z),maxZ=all.Max(v=>v.z),maxX=all.Max(v=>v.x);
        var selected=all.Where(v=>wheel||presentedPin?v.x>maxX-.035f:crosshead?v.z<-.6f&&Mathf.Abs(v.y)<.25f:
            crank?v.y<.15f:tip?v.y>.15f:end<0?v.z<minZ+.40f:end>0?v.z>maxZ-.35f:true).ToArray();
        var points=new List<Vector2>();
        foreach(var v in selected)
            if(!points.Any(p=>Mathf.Abs(p.x-v.y)<.000002f&&Mathf.Abs(p.y-v.z)<.000002f))points.Add(new Vector2(v.y,v.z));
        result.UniquePoints=points.Count;if(points.Count<8)return result;
        var random=new System.Random(6147);int best=0;float bestResidual=float.PositiveInfinity;Vector2 centre=Vector2.zero;float radius=0;
        for(int trial=0;trial<3000;trial++)
        {
            int ia=random.Next(points.Count),ib=random.Next(points.Count),ic=random.Next(points.Count);
            if(ia==ib||ia==ic||ib==ic)continue;
            var a=points[ia];var b=points[ib];var c=points[ic];
            double by=b.x-a.x,bz=b.y-a.y,cy=c.x-a.x,cz=c.y-a.y,det=4*(by*cz-bz*cy);
            if(Math.Abs(det)<1e-9)continue;
            double bb=b.sqrMagnitude-a.sqrMagnitude,cc=c.sqrMagnitude-a.sqrMagnitude;
            var candidate=new Vector2((float)((2*cz*bb-2*bz*cc)/det),(float)((2*by*cc-2*cy*bb)/det));
            float r=Vector2.Distance(a,candidate);if(r<.01f||r>.20f)continue;
            // A native pin also has smaller bolt-cap rings. Measure its shaft,
            // whose diameter is independently distinct from those cap details.
            if(presentedPin&&(r<.04f||r>.08f))continue;
            if(wheel&&(candidate.magnitude<.20f||candidate.magnitude>.50f))continue;
            int count=0;float residual=0;
            foreach(var p in points){float error=Mathf.Abs(Vector2.Distance(p,candidate)-r);if(error<AttachmentCircleTolerance){count++;residual=Mathf.Max(residual,error);}}
            if(count<8||count<best||count==best&&residual>=bestResidual)continue;
            best=count;bestResidual=residual;centre=candidate;radius=r;
        }
        if(best<8)return result;
        // Recover the full modeled pin/eye axial interval on this same measured
        // cylindrical ring, not merely the outboard cap's X coordinate.
        var ring=all.Where(v=>Mathf.Abs(Vector2.Distance(new Vector2(v.y,v.z),centre)-radius)<AttachmentCircleTolerance).ToArray();
        result.Resolved=true;result.Radius=radius;result.Residual=bestResidual;result.Inliers=best;
        result.XMin=ring.Min(v=>v.x);result.XMax=ring.Max(v=>v.x);
        result.Centre=new Vector3((result.XMin+result.XMax)*.5f,centre.x,centre.y);return result;
    }
    static void AddFranklinAttachmentAudit(Layout layout,int id)
    {
        if(id!=10&&id!=31&&id!=32&&id!=38&&id!=40&&id!=47)return;
        var audit=new AttachmentLayout{Id=id};attachmentLayouts[layout]=audit;
        for(int e=0;e<layout.Engines.Count;e++)foreach(bool left in new[]{true,false})
        {
            var drive=left?layout.Engines[e].Left:layout.Engines[e].Right;
            var d=new AttachmentDrive{Drive=drive,Label=e+(left?" L":" R")};audit.Drives.Add(d);
            d.MainNear=AttachmentFit(AttachmentRendered(drive,AttachmentMainPath),"main rod near eye",-1);
            d.MainFar=AttachmentFit(AttachmentRendered(drive,AttachmentMainPath),"main rod far eye",1);
            d.Crosshead=AttachmentFit(AttachmentRendered(drive,"s282_mech_push_rod"),"crosshead pin boss",crosshead:true);
            d.Crank=AttachmentFit(AttachmentRendered(drive,AttachmentCrankPath),"cutoff crank base boss",crank:true);
            d.CrankTip=AttachmentFit(AttachmentRendered(drive,AttachmentCrankPath),"eccentric arm other boss (intentional offset from base)",tip:true);
            MeshFilter Pin(int wheel)
            {
                var root=drive.Cast<Transform>().SingleOrDefault(t=>t.name=="[Rearranged native driver pin] wheel "+wheel&&t.gameObject.activeInHierarchy);
                return root==null?null:root.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.sharedMesh!=null&&f.gameObject.activeInHierarchy);
            }
            d.PresentedPin1=AttachmentFit(Pin(1),"constrained native pin wheel 1",presentedPin:true);
            d.PresentedPin2=AttachmentFit(Pin(2),"constrained native pin wheel 2",presentedPin:true);
            var side=AttachmentRendered(drive,AttachmentSidePath);
            if(side!=null){d.SideEyes.Add(AttachmentFit(side,"generated coupling rod low-Z eye",-1));d.SideEyes.Add(AttachmentFit(side,"generated coupling rod high-Z eye",1));}
            foreach(var eye in d.SideEyes)d.SidePinSeating.Add(new AttachmentMetric{Name=eye.Kind+" to constrained native pin"});
            for(int w=1;w<=4;w++)
            {
                var part=drive.Find("s282_wheels_driving_"+w);if(part==null||!part.gameObject.activeInHierarchy)continue;
                var f=part.GetComponent<MeshFilter>();if(f!=null&&f.sharedMesh!=null)d.Wheels.Add(AttachmentFit(f,"wheel "+w+" crank cap",wheel:true));
            }
            d.Metrics.Add(new AttachmentMetric{Name="main near eye to nearest active wheel crank pin"});
            d.Metrics.Add(new AttachmentMetric{Name="main near eye to cutoff crank boss"});
            foreach(var eye in d.SideEyes)
            {
                d.Metrics.Add(new AttachmentMetric{Name=eye.Kind+" to nearest active wheel crank pin"});
                d.Metrics.Add(new AttachmentMetric{Name=eye.Kind+" to main rod near eye"});
                d.Metrics.Add(new AttachmentMetric{Name=eye.Kind+" to cutoff crank boss"});
            }
            d.Metrics.Add(new AttachmentMetric{Name="eccentric arm base to its other boss (intrinsic arm length, not attachment error)"});
            if(id==10)continue;
            // These are clearance candidates only. Own rod/socket engagement is
            // intentional and is measured above rather than called a collision.
            if(e+1<layout.Engines.Count)
            {
                var following=left?layout.Engines[e+1].Left:layout.Engines[e+1].Right;
                var f=following.Find("franklin_type_b(Clone)/cylinder/default");
                if(f!=null)
                {
                    var casing=Rendered(f);
                    foreach(var candidate in new[]{d.MainNear.Filter,side}.Where(x=>x!=null))
                        if(!layout.Pairs.Any(p=>p.A==candidate&&p.B==casing))
                            layout.Pairs.Add(new Pair{Label=d.Label+" "+(candidate==side?"generated coupling rod":"main rod")+" vs following casing",A=candidate,B=casing});
                    foreach(var pin in new[]{d.PresentedPin1,d.PresentedPin2}.Where(p=>p.Filter!=null))
                        layout.Pairs.Add(new Pair{Label=d.Label+" "+pin.Kind+" vs following casing",A=pin.Filter,B=casing});
                    var franklin=drive.Find(id==32&&e==0?"franklin_type_b_4444(Clone)":"franklin_type_b(Clone)");
                    if(franklin!=null)
                        foreach(var auxiliary in franklin.GetComponentsInChildren<MeshFilter>(true).Where(x=>x.sharedMesh!=null&&Visible(x)))
                        {
                            string relative=Relative(franklin,auxiliary.transform);
                            if(relative=="cylinder/default"||relative.StartsWith("cylinder/default/",StringComparison.Ordinal))continue;
                            if(!layout.Pairs.Any(p=>p.A==auxiliary&&p.B==casing))
                                layout.Pairs.Add(new Pair{Label=d.Label+" Franklin auxiliary "+relative+" vs following casing",A=auxiliary,B=casing});
                        }
                }
            }
        }
    }
    static Vector3 AttachmentPoint(AttachmentCircle c)=>c.Filter.transform.TransformPoint(c.Centre);
    static float AttachmentRadial(Transform drive,AttachmentCircle a,AttachmentCircle b)
    {
        var p=drive.InverseTransformPoint(AttachmentPoint(a))-drive.InverseTransformPoint(AttachmentPoint(b));
        // Drive-local axes normalize uniform arrangement scale; convert radial
        // displacement back to actual world metres with the presentation frame.
        return drive.TransformVector(new Vector3(0,p.y,p.z)).magnitude;
    }
    static void AttachmentMeasure(AttachmentDrive drive,AttachmentMetric metric,AttachmentCircle eye,AttachmentCircle pin,float phase,float yaw,int pattern)
    {
        if(eye==null||pin==null||!eye.Resolved||!pin.Resolved)return;
        float radial=AttachmentRadial(drive.Drive,eye,pin);
        float Axis(AttachmentCircle circle,float x)=>drive.Drive.InverseTransformPoint(circle.Filter.transform.TransformPoint(new Vector3(x,circle.Centre.y,circle.Centre.z))).x;
        float a=Axis(eye,eye.XMin),b=Axis(eye,eye.XMax),c=Axis(pin,pin.XMin),d=Axis(pin,pin.XMax);
        float alo=Mathf.Min(a,b),ahi=Mathf.Max(a,b),blo=Mathf.Min(c,d),bhi=Mathf.Max(c,d);
        float axial=drive.Drive.TransformVector(new Vector3(Mathf.Max(0,Mathf.Max(alo-bhi,blo-ahi)),0,0)).magnitude;
        float overlap=Mathf.Max(0,Mathf.Min(ahi,bhi)-Mathf.Max(alo,blo));
        metric.MinimumAxialOverlapM=Mathf.Min(metric.MinimumAxialOverlapM,drive.Drive.TransformVector(Vector3.right*overlap).magnitude);
        metric.MinimumEyeAxialCoverage=Mathf.Min(metric.MinimumEyeAxialCoverage,overlap/Mathf.Max(.000001f,ahi-alo));
        metric.Samples++;metric.MinRadial=Mathf.Min(metric.MinRadial,radial);metric.Matches.Add(pin.Kind);
        if(metric.Worst==null||radial>metric.MaxRadial||axial>metric.MaxAxialGap)
            metric.Worst=new JObject{["phase"]=phase,["yawInput"]=yaw,["pattern"]=pattern,["radialCentreDistanceM"]=radial,["axialIntervalSeparationM"]=axial,
                ["eyeWorld"]=JV(AttachmentPoint(eye)),["pinWorld"]=JV(AttachmentPoint(pin)),["pin"]=pin.Kind,
                ["eyeAxisIntervalDriveX"]=new JArray(alo,ahi),["pinAxisIntervalDriveX"]=new JArray(blo,bhi)};
        metric.MaxRadial=Mathf.Max(metric.MaxRadial,radial);metric.MaxAxialGap=Mathf.Max(metric.MaxAxialGap,axial);
    }
    static void SampleFranklinAttachments(Layout layout,float phase,float yaw,int pattern)
    {
        if(!attachmentLayouts.TryGetValue(layout,out var audit))return;
        foreach(var d in audit.Drives)
        {
            var target=d.Wheels.Where(w=>w.Resolved).OrderBy(w=>d.MainNear.Resolved?AttachmentRadial(d.Drive,d.MainNear,w):float.PositiveInfinity).FirstOrDefault();
            AttachmentMeasure(d,d.Metrics[0],d.MainNear,target,phase,yaw,pattern);
            AttachmentMeasure(d,d.Metrics[1],d.MainNear,d.Crank,phase,yaw,pattern);
            AttachmentMeasure(d,d.MainPinSeating,d.MainNear,d.PresentedPin2,phase,yaw,pattern);
            for(int i=0;i<d.SideEyes.Count;i++)
            {
                var eye=d.SideEyes[i];var wheel=d.Wheels.Where(w=>w.Resolved).OrderBy(w=>eye.Resolved?AttachmentRadial(d.Drive,eye,w):float.PositiveInfinity).FirstOrDefault();
                AttachmentMeasure(d,d.Metrics[2+i*3],eye,wheel,phase,yaw,pattern);
                AttachmentMeasure(d,d.Metrics[3+i*3],eye,d.MainNear,phase,yaw,pattern);
                AttachmentMeasure(d,d.Metrics[4+i*3],eye,d.Crank,phase,yaw,pattern);
                var presented=new[]{d.PresentedPin1,d.PresentedPin2}.Where(p=>p.Resolved)
                    .OrderBy(p=>eye.Resolved?AttachmentRadial(d.Drive,eye,p):float.PositiveInfinity).FirstOrDefault();
                AttachmentMeasure(d,d.SidePinSeating[i],eye,presented,phase,yaw,pattern);
            }
            AttachmentMeasure(d,d.Metrics[d.Metrics.Count-1],d.Crank,d.CrankTip,phase,yaw,pattern);
            if(d.Crank.Filter==null||!d.Crank.Filter.gameObject.activeInHierarchy||!d.Crank.Filter.GetComponent<MeshRenderer>().enabled)d.HiddenCrankSamples++;
            if(d.Crosshead.Resolved&&d.MainNear.Filter!=null)
            {
                // The main rod's far end does not expose a reliable circular
                // aperture. Retain this evaluated native kinematic cloud rather
                // than invent a pin centre or fit the candidate to itself.
                var p=d.MainNear.Filter.transform.InverseTransformPoint(AttachmentPoint(d.Crosshead));
                d.CrossheadInMainMin=Vector3.Min(d.CrossheadInMainMin,p);d.CrossheadInMainMax=Vector3.Max(d.CrossheadInMainMax,p);
            }
        }
    }
    static JObject FranklinAttachmentReport(Layout layout)
    {
        if(!attachmentLayouts.TryGetValue(layout,out var audit))return null;
        return new JObject{["id"]=audit.Id,["classification"]="diagnostic measurements; compare native ID10 and original unretargeted Franklin ID31 before candidate acceptance",
            ["circleToleranceM"]=AttachmentCircleTolerance,["drives"]=new JArray(audit.Drives.Select(d=>(object)new JObject{
                ["drive"]=d.Label,["datums"]=new JArray(new[]{d.MainNear,d.MainFar,d.Crosshead,d.Crank,d.CrankTip,d.PresentedPin1,d.PresentedPin2}.Concat(d.SideEyes).Concat(d.Wheels).Select(c=>(object)c.Report())),
                ["metrics"]=new JArray(d.Metrics.Concat(new[]{d.MainPinSeating}).Concat(d.SidePinSeating).Select(m=>(object)m.Report())),["hiddenCrankSamples"]=d.HiddenCrankSamples,
                ["crossheadPinInPresentedMainRodLocalMinimum"]=d.Crosshead.Resolved?JV(d.CrossheadInMainMin):null,
                ["crossheadPinInPresentedMainRodLocalMaximum"]=d.Crosshead.Resolved?JV(d.CrossheadInMainMax):null})),
            ["limits"]="Geometric circle fits use only referenced triangles. Radial separation and axial interval separation are distinct; a pin cap centre is not an axial seating requirement. Missing/irregular circular eyes are unresolved, never a passing zero. Contact pairs cover actual main/coupling rods and rendered Franklin auxiliary shaft/U-joint/transmission/mount meshes against the following casing; intended own sockets and own casing contacts are excluded. The eccentric arm's other boss has an intentional offset from its base and is not a missing pin measurement. Far-end crosshead cloud is a native kinematic comparison, not an independently resolved far-eye centre. No automatic geometry correction or visual acceptance."};
    }
    static void ValidateFranklinPinSeating(Layout layout,int samples)
    {
        Require(attachmentLayouts.TryGetValue(layout,out var audit),"Franklin pin seating has an attachment audit");
        float MinimumRadius(AttachmentCircle c)=>c.Radius*Mathf.Min(c.Filter.transform.TransformVector(Vector3.up).magnitude,
            c.Filter.transform.TransformVector(Vector3.forward).magnitude);
        float MaximumRadius(AttachmentCircle c)=>c.Radius*Mathf.Max(c.Filter.transform.TransformVector(Vector3.up).magnitude,
            c.Filter.transform.TransformVector(Vector3.forward).magnitude);
        foreach(var d in audit.Drives)
        {
            Require(d.MainNear.Resolved&&d.PresentedPin2.Resolved&&d.MainPinSeating.Samples==samples,
                d.Label+" main eye and native long pin are independently resolved in every pose");
            Require(MinimumRadius(d.MainNear)-MaximumRadius(d.PresentedPin2)-d.MainPinSeating.MaxRadial>=-.0002f,
                d.Label+" native long pin fits inside the animated main eye's bore envelope");
            Require(d.MainPinSeating.MaxAxialGap<.0002f,d.Label+" native long pin reaches the main eye axially");
            Require(d.MainPinSeating.MinimumEyeAxialCoverage>.999f,
                d.Label+" native long pin shaft spans the full main-eye axial thickness");
            Require(d.HiddenCrankSamples==0,d.Label+" native eccentric crank bearing remains rendered");
            foreach(var pin in new[]{d.PresentedPin1,d.PresentedPin2}.Where(p=>p.Filter!=null))
                Require(Mathf.Abs(pin.Filter.transform.localPosition.x)<.00001f,
                    d.Label+" native pin preserves its absolute mesh X coordinates");
            for(int i=0;i<d.SideEyes.Count;i++)
            {
                var metric=d.SidePinSeating[i];var eye=d.SideEyes[i];
                Require(eye.Resolved&&metric.Samples==samples&&metric.Matches.Count==1,
                    d.Label+" "+eye.Kind+" resolves one native pin through every sampled pose");
                var pin=metric.Matches.Single()==d.PresentedPin1.Kind?d.PresentedPin1:d.PresentedPin2;
                Require(pin.Resolved&&MinimumRadius(eye)-MaximumRadius(pin)-metric.MaxRadial>=-.0002f,
                    d.Label+" native pin fits inside the generated coupling eye's bore envelope");
                Require(metric.MaxAxialGap<.0002f,d.Label+" native pin reaches the generated coupling eye axially");
                // The stock short pin's shaft occupies about 71% of this bore;
                // its smaller native cap occupies the remainder. Retain that
                // engagement rather than requiring a non-native longer shaft.
                Require(metric.MinimumEyeAxialCoverage>.70f,
                    d.Label+" generated coupling eye retains at least the native short-pin shaft engagement");
            }
        }
    }
}
