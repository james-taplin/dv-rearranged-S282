using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RearrangedS282;
using UnityEngine;

internal static class Program
{
    private static int checks;
    private static readonly List<object> cases = new();
    private static readonly List<string> orientationFailures = new();
    private static readonly List<object> previews = new();
    private static void Check(bool value, string message)
    { checks++; if (!value) throw new InvalidOperationException(message); }
    private static bool Near(float a,float b,float tolerance=2e-5f) => Math.Abs(a-b)<=tolerance;
    private static bool Same(Vector3 a,Vector3 b) => a.x==b.x && a.y==b.y && a.z==b.z;
    private static bool Finite(Vector3 p) => float.IsFinite(p.x) && float.IsFinite(p.y) && float.IsFinite(p.z);
    private static Vector3 Read(JsonElement v) => new(v[0].GetSingle(),v[1].GetSingle(),v[2].GetSingle());
    private static float Length(Vector3 p) => (float)Math.Sqrt(Vector3.Dot(p,p));

    private static float Jacobian(Func<Vector3,Vector3> map,Vector3 p)
    {
        const float step=.0002f;
        Vector3 dx=(map(p+new Vector3(step,0,0))-map(p-new Vector3(step,0,0)))*(1f/(2*step));
        Vector3 dy=(map(p+new Vector3(0,step,0))-map(p-new Vector3(0,step,0)))*(1f/(2*step));
        Vector3 dz=(map(p+new Vector3(0,0,step))-map(p-new Vector3(0,0,step)))*(1f/(2*step));
        return Vector3.Dot(dx,Vector3.Cross(dy,dz));
    }

    private static void NativeCase(JsonElement part,float sy,float sz,float originY=0)
    {
        string name=part.GetProperty("name").GetString();
        bool franklin=part.GetProperty("kind").GetString()=="franklin";
        bool cocks=name.EndsWith("-cocks",StringComparison.Ordinal);
        var vertices=part.GetProperty("vertices").EnumerateArray().Select(Read).ToArray();
        var snapshot=vertices.ToArray();
        var faces=part.GetProperty("triangles").EnumerateArray().Select(t=>t.EnumerateArray().Select(i=>i.GetInt32()).ToArray()).ToArray();
        float piston=franklin ? .71f : CylinderCasingShape.NativePistonY;
        float rear=vertices.Min(v=>v.z)+.16f, full=rear+.20f;
        float minY=franklin?vertices.Min(v=>v.y):Math.Min(vertices.Min(v=>v.y),.2739646f);
        float target=CylinderCasingShape.ComputeTargetY(sy,minY,piston,originY);
        Check(target>0 && target<=1.00001f,$"{name}: positive target no larger than vanilla");
        Check(sy>=1 || target>=sy,$"{name}: small engine is never reduced below its existing size");
        Vector3 Map(Vector3 p) => cocks ? p : CylinderCasingShape.TransformVertex(p,sy,sz,target,rear,full,piston,franklin);
        var output=vertices.Select(Map).ToArray();
        int changed=0, socket=0, drain=0, chest=0, bridge=0, bulk=0;
        float minWorldY=float.PositiveInfinity, minJac=float.PositiveInfinity;
        Vector3 minJacPoint=new();
        void SampleJacobian(Vector3 point)
        {float j=Jacobian(Map,point);if(j<minJac){minJac=j;minJacPoint=point;}}
        for(int i=0;i<vertices.Length;i++)
        {
            var p=vertices[i]; var q=output[i];
            Check(Finite(q),$"{name}: finite real mesh vertex");
            Check(q.x==p.x,$"{name}: transverse position and width remain exact");
            Check(q.z==p.z,$"{name}: all longitudinal dimensions stay exact to preserve existing stroke and shaft connections");
            Check(Same(vertices[i],snapshot[i]),$"{name}: source vertices stay immutable");
            if(!Same(p,q))changed++;
            if(sy==1 && sz==1)Check(Same(p,q),$"{name}: native-sized baseline is exact identity");
            if(p.z<=rear) { socket++; Check(Same(p,q),$"{name}: both rear spindle/socket surfaces remain exact"); }
            if(Math.Abs(Math.Abs(p.x)-1.09f)<=.16f && p.y<=.48f)
            { drain++; Check(Same(p,q),$"{name}: native cock/drain connections stay exact"); }
            if(!franklin && p.y>=1.30f) {chest++;Check(Same(p,q),$"{name}: native upper steam chest and steam-pipe contact remain exact");}
            if(franklin && p.y>=1.12f) {bridge++;Check(Same(p,q),$"{name}: Franklin upper mounting bridge stays exact");}
            if(!franklin && Math.Abs(p.x)<=1.20f && p.y>=1.50f && p.z>=9.31f && p.z<=9.68f)
                Check(Same(p,q),$"{name}: native dry-pipe insertion patch remains exact");
            if(!cocks && p.z>=full && p.y>=.64f && p.y<=(franklin?.85f:1.02f))
            {
                bulk++;
                float effectiveTarget=sy+(target-sy)*CylinderCasingShape.BulkStrength;
                Check(Near(sy*(q.y-piston),effectiveTarget*(p.y-piston)), $"{name}: unconstrained main barrel approaches requested height conservatively");
                Check(q.z==p.z,$"{name}: unconstrained casing length remains matched to existing motion");
            }
            minWorldY=Math.Min(minWorldY,originY+sy*q.y);
            if(!cocks)SampleJacobian(p);
        }
        float baselineMin=originY+sy*vertices.Min(v=>v.y);
        Check(minWorldY>=Math.Min(CylinderCasingShape.RailClearance,baselineMin)-2e-5f,$"{name}: casing stays above rail clearance or improves existing low clearance");
        Check(socket>0 || cocks,$"{name}: fixture contains actual rear socket geometry");
        if(!franklin && !cocks)Check(drain>0,$"{name}: fixture exercises actual drain mounts");
        if(cocks)Check(changed==0,$"{name}: separate native cock mesh is untouched");
        else if(sy!=1 || sz!=1)Check(changed>0,$"{name}: actual casing receives a visible correction");
        int nondegenerate=0, reversed=0; float minNormalDot=1;
        var turnedFaces=new List<object>();
        foreach(var t in faces)
        {
            Check(t.Length==3 && t.All(i=>i>=0 && i<vertices.Length),$"{name}: original triangle indices remain valid");
            var oldN=Vector3.Cross(vertices[t[1]]-vertices[t[0]],vertices[t[2]]-vertices[t[0]]);
            var newN=Vector3.Cross(output[t[1]]-output[t[0]],output[t[2]]-output[t[0]]);
            float oldL=Length(oldN),newL=Length(newN);
            if(oldL<1e-9)continue;
            nondegenerate++;
            Check(newL>1e-12,$"{name}: deformation does not collapse an existing triangle");
            float dot=Vector3.Dot(oldN,newN)/(oldL*newL);
            minNormalDot=Math.Min(minNormalDot,dot);
            if(dot<=0)
            {
                reversed++;
                turnedFaces.Add(new {indices=t,oldDoubleArea=oldL,newDoubleArea=newL,dot,
                    points=t.Select(i=>new[]{vertices[i].x,vertices[i].y,vertices[i].z}).ToArray()});
            }
            if(!cocks)SampleJacobian((vertices[t[0]]+vertices[t[1]]+vertices[t[2]])*(1f/3f));
        }
        cases.Add(new {name,scaleY=sy,scaleZ=sz,originY,targetY=target,vertices=vertices.Length,faces=faces.Length,changed,
            socket,drain,chest,bridge,bulk,minWorldY,minimumJacobian=cocks?(float?)null:minJac,
            minimumJacobianPoint=new[]{minJacPoint.x,minJacPoint.y,minJacPoint.z},
            nondegenerateFaces=nondegenerate,facesOver90Degrees=reversed,minimumNormalDot=minNormalDot,turnedFaces});
        if((name=="native-lod0-left" && sz==sy && (sy==.58f || sy==1.2879f)) ||
           (name=="franklin-type-b-cylinder-default" && sy==.65f && sz==sy))
            previews.Add(new {name,scaleY=sy,scaleZ=sz,originY,targetY=target,
                baseline=vertices.Select(p=>new[]{p.x,originY+sy*p.y,sz*p.z}).ToArray(),
                corrected=output.Select(p=>new[]{p.x,originY+sy*p.y,sz*p.z}).ToArray(),triangles=faces,
                turnedFaceIndices=turnedFaces});
        if(!cocks)
        {checks++;if(minJac<=0)orientationFailures.Add($"{name} sy{sy} sz{sz} originY{originY}: determinant {minJac} at ({minJacPoint.x},{minJacPoint.y},{minJacPoint.z})");}
        Check(reversed==0,$"{name} sy{sy} sz{sz} originY{originY}: no actual triangle normal turns over 90 degrees");
    }

    private static void Inputs()
    {
        foreach(float invalid in new[]{0,-1,float.NaN,float.PositiveInfinity,float.NegativeInfinity})
        {
            bool rejected=false;try{CylinderCasingShape.ComputeTargetY(invalid,.27f);}catch(ArgumentException){rejected=true;}
            Check(rejected,"Invalid engine scale is rejected");
        }
        var point=new Vector3(1.2f,CylinderCasingShape.NativePistonY,9.6f);
        foreach(float sy in new[]{.4f,.58f,.75f,1,1.2879f,1.42f})
        {
            float target=CylinderCasingShape.ComputeTargetY(sy,.2739646f);
            Check(target>0 && target<=1,"Rail-safe target is positive and no greater than vanilla");
            var q=CylinderCasingShape.TransformVertex(point,sy,sy,target,8.96f,9.13f);
            Check(q.y==point.y,"Piston centreline does not move");
        }
    }

    private static void PrairieCase(JsonElement part,float s=1.2879f)
    {
        string name=part.GetProperty("name").GetString();
        var vertices=part.GetProperty("vertices").EnumerateArray().Select(Read).ToArray();
        var faces=part.GetProperty("triangles").EnumerateArray().Select(t=>t.EnumerateArray().Select(i=>i.GetInt32()).ToArray()).ToArray();
        Vector3 Map(Vector3 p)=>CylinderCasingShape.NormalizePrairie(p,s,s);
        var output=vertices.Select(Map).ToArray();
        foreach(var p in vertices)
        {
            var q=Map(p);
            Check(Finite(q),name+": Prairie entire casting finite");
            Check(q.x==p.x,name+": Prairie native transverse width preserved");
            Check(Jacobian(Map,p)>0,name+": Prairie map is locally orientation preserving");
            Check(Near(s*(q.z-CylinderCasingShape.NativeRearFaceZ),p.z-CylinderCasingShape.NativeRearFaceZ),name+": full casing length returns to native size");
        }
        foreach(float axis in new[]{CylinderCasingShape.NativePistonY,CylinderCasingShape.NativeValveY})
        {
            var p=new Vector3(1.09f,axis,CylinderCasingShape.NativeRearFaceZ);
            Check(Near(Map(p).y,axis),name+": animated piston/valve axis unchanged");
            foreach(float offset in new[]{-.005f,.005f})
                Check(Math.Abs(s*(Map(p+new Vector3(0,offset,0)).y-axis)-offset)<.0005f,name+": socket radial size approaches native");
        }
        int turned=0;
        foreach(var t in faces)
        {
            var a=Vector3.Cross(vertices[t[1]]-vertices[t[0]],vertices[t[2]]-vertices[t[0]]);
            var b=Vector3.Cross(output[t[1]]-output[t[0]],output[t[2]]-output[t[0]]);
            if(Length(a)>1e-9f) {Check(Length(b)>1e-12f,name+": Prairie triangle not collapsed");if(Vector3.Dot(a,b)<=0)turned++;}
        }
        Check(turned==0,name+": Prairie no reversed face normals");
        Check(Near(s*(output.Max(p=>p.z)-output.Min(p=>p.z)),vertices.Max(p=>p.z)-vertices.Min(p=>p.z)),name+": overall native longitudinal size");
        previews.Add(new{name=name+(s==1.2879f?"-prairie-full":"-fourcoupled-full"),scaleY=s,scaleZ=s,originY=0,
            baseline=vertices.Select(p=>new[]{p.x,s*p.y,s*p.z}).ToArray(),
            corrected=output.Select(p=>new[]{p.x,s*p.y,s*p.z}).ToArray(),triangles=faces,turnedFaceIndices=Array.Empty<object>()});
        cases.Add(new{name=name+(s==1.2879f?"-prairie-full":"-fourcoupled-full"),scaleY=s,scaleZ=s,vertices=vertices.Length,faces=faces.Length,
            oldHeight=s*(vertices.Max(p=>p.y)-vertices.Min(p=>p.y)),newHeight=s*(output.Max(p=>p.y)-output.Min(p=>p.y)),
            nativeHeight=vertices.Max(p=>p.y)-vertices.Min(p=>p.y),nativeLength=vertices.Max(p=>p.z)-vertices.Min(p=>p.z),facesOver90Degrees=turned});
    }

    private static void TriplexNoseCase(JsonElement part)
    {
        string name=part.GetProperty("name").GetString();
        var native=part.GetProperty("vertices").EnumerateArray().Select(Read).ToArray();
        var snapshot=native.ToArray();
        var faces=part.GetProperty("triangles").EnumerateArray().Select(t=>t.EnumerateArray().Select(i=>i.GetInt32()).ToArray()).ToArray();
        Vector3 Map(Vector3 p)=>CylinderCasingShape.CompactFranklinTriplexNose(p);
        var mapped=native.Select(Map).ToArray();int caps=0,mounts=0;float minJac=float.PositiveInfinity;
        for(int i=0;i<native.Length;i++)
        {
            var p=native[i];var q=mapped[i];
            Check(Finite(q)&&q.x==p.x&&q.y==p.y,name+": triplex nose preserves bore axes and transverse geometry");
            Check(Same(native[i],snapshot[i])&&Same(q,Map(snapshot[i])),name+": repeated mapping from native source is deterministic without accumulated shortening");
            float jac=Jacobian(Map,p);minJac=Math.Min(minJac,jac);
            Check(jac>.1f,name+": triplex nose remains monotone with a positive longitudinal margin");
            if(p.z<=9.49f){mounts++;Check(Same(p,q),name+": piston rear face and shaft mounts remain exact");}
            if(p.z>=9.85f){caps++;Check(Near(q.z,p.z-.20f),name+": forward cap and bolts translate rigidly by 200 mm");}
        }
        Check(caps>0&&mounts>0,name+": real Franklin mesh contains both fixed mounts and translated cap");
        foreach(var t in faces)
        {
            var oldNormal=Vector3.Cross(native[t[1]]-native[t[0]],native[t[2]]-native[t[0]]);
            if(Length(oldNormal)<1e-9f)continue;
            var newNormal=Vector3.Cross(mapped[t[1]]-mapped[t[0]],mapped[t[2]]-mapped[t[0]]);
            Check(Length(newNormal)>1e-12f&&Vector3.Dot(oldNormal,newNormal)>0,name+": triplex nose does not collapse or reverse a native triangle");
        }
        for(int i=0;i<=400;i++)
        {
            float z=9.45f+i*.0012f;
            Check(Map(new Vector3(1.09f,.71f,z+.0001f)).z>Map(new Vector3(1.09f,.71f,z)).z,
                name+": triplex transition is monotone between actual vertex samples");
        }
        cases.Add(new{name=name+"-triplex-nose",vertices=native.Length,faces=faces.Length,fixedMountVertices=mounts,rigidCapVertices=caps,minJacobian=minJac});
    }

    private static int Main(string[] args)
    {
        try
        {
            Inputs();
            using var doc=JsonDocument.Parse(File.ReadAllText(args[0]));
            foreach(var source in doc.RootElement.GetProperty("sources").EnumerateArray())
            foreach(var part in source.GetProperty("parts").EnumerateArray())
            {
                if(part.GetProperty("kind").GetString()=="native"){PrairieCase(part);PrairieCase(part,1.42f);}
                if(part.GetProperty("kind").GetString()=="franklin")TriplexNoseCase(part);
                foreach(float s in new[]{.4f,.58f,.65f,.75f,.9f,1,1.198f,1.2879f,1.42f})NativeCase(part,s,s);
                NativeCase(part,.58f,.58f,.12f);
                NativeCase(part,.4f,.4f,.12f);
                NativeCase(part,1.2879f,1f);
            }
            Check(orientationFailures.Count==0,"Casing map has non-positive local orientation: "+string.Join("; ",orientationFailures));
            var report=new {passed=true,assertions=checks,cases=cases.Count,note="Actual source helper on actual native/Franklin vertices; managed numerical fixture cannot prove Unity rendering, ownership lifecycle or in-game clearances.",results=cases};
            File.WriteAllText(args[1],JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(args[1]),"preview-geometry.json"),JsonSerializer.Serialize(previews));
            Console.WriteLine($"Cylinder casings: {checks} assertions in {cases.Count} actual-mesh cases passed.");
            return 0;
        }
        catch(Exception ex)
        {
            File.WriteAllText(args[1],JsonSerializer.Serialize(new {passed=false,assertions=checks,completedCases=cases.Count,error=ex.ToString(),results=cases},new JsonSerializerOptions{WriteIndented=true}));
            Console.Error.WriteLine(ex);
            return 1;
        }
    }
}
