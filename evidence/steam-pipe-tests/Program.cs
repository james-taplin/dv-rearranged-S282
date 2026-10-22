using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using RearrangedS282;
using UnityEngine;

internal static class Program
{
    private static int checks, nativeCases, nativeRerouteCases, sweeps, faceChecks, rejectedTightBends, sparseInwardFaces, denseInwardFaces;
    private static float maximumEndpointGap, maximumCollarError, minimumNativeGap=float.PositiveInfinity,minimumSweepBendRadius=float.PositiveInfinity,maximumTangentStep;
    private static void Check(bool ok,string text){checks++;if(!ok)throw new InvalidOperationException(text);}
    private static Vector3 V(JsonElement row)=>new(row[0].GetSingle(),row[1].GetSingle(),row[2].GetSingle());
    private static Vector3[] Vertices(JsonElement data)=>data.GetProperty("vertices").EnumerateArray().Select(V).ToArray();
    private static int[] Triangles(JsonElement data)=>data.GetProperty("triangles").EnumerateArray().SelectMany(v=>v.EnumerateArray().Select(a=>a.GetInt32())).ToArray();
    private static Vector3 Mean(Vector3[] v,int[] ids)=>ids.Aggregate(Vector3.zero,(s,i)=>s+v[i])/ids.Length;
    private static int[] Ring(Vector3[] v,float z)=>Enumerable.Range(0,v.Length).Where(i=>Math.Abs(v[i].z-z)<.00001f)
        .GroupBy(i=>(v[i].x,v[i].y,v[i].z)).Select(g=>g.First()).ToArray();
    private static Vector3 Rotate(Vector3 p,float yaw){float a=yaw*(float)Math.PI/180;return new((float)Math.Cos(a)*p.x+(float)Math.Sin(a)*p.z,p.y,-(float)Math.Sin(a)*p.x+(float)Math.Cos(a)*p.z);}

    private static void Native(JsonElement casings,JsonElement fittings)
    {
        foreach(var part in casings.GetProperty("sources")[0].GetProperty("parts").EnumerateArray().Where(p=>p.GetProperty("name").GetString().EndsWith("left")||p.GetProperty("name").GetString().EndsWith("right")))
        {
            bool left=part.GetProperty("name").GetString().EndsWith("left");
            var pipe=fittings.GetProperty("parts").EnumerateArray().Single(p=>p.GetProperty("name").GetString()=="native-lod0-pipe-"+(left?"left":"right"));
            var source=Vertices(pipe);var native=Vertices(part);var faces=Triangles(part);
            float low=source.Min(p=>p.y),high=source.Max(p=>p.y);
            var ring=source.Where(p=>Math.Abs(p.y-low)<.003f).GroupBy(p=>(p.x,p.y,p.z)).Select(g=>g.First()).ToArray();
            var lower=ring.Aggregate(Vector3.zero,(s,p)=>s+p)/ring.Length;
            var socket=SteamPipeShape.TopSocket(native,faces,lower.x,lower.z);
            foreach(float scale in new[]{.55f,.75f,.8f,.9f,1f,1.11f,1.2879f,1.42f})foreach(bool full in new[]{false,true})
            {
                // Real source triangles with the production normalization helper.
                var edited=native.Select(p=>full?CylinderCasingShape.NormalizePrairie(p,scale,scale):p).Select(p=>new Vector3(p.x,p.y*scale,p.z*scale)).ToArray();
                var target=socket.Evaluate(edited);var work=new Vector3[source.Length];
                if (high-target.y<=.27f)
                {
                    Check(!SteamPipeShape.CanMorphNative(low,high,target.y),"Close native sockets must take the routed manifold path");
                    nativeRerouteCases++;continue;
                }
                SteamPipeShape.MorphNative(source,work,lower,low,high,target);nativeCases++;
                minimumNativeGap=Math.Min(minimumNativeGap,high-target.y);
                var delta=target-lower;
                for(int i=0;i<source.Length;i++)
                {
                    if(source[i].y<=low+.13f)Check((work[i]-source[i]-delta).magnitude<.000004f,"Native lower flange was rescaled");
                    if(source[i].y>=high-.13f)Check((work[i]-source[i]).magnitude<.000004f,"Native boiler flange moved");
                }
                Check(native.SequenceEqual(Vertices(part)),"Source cylinder mesh changed");
            }
        }
    }

    private static void SweepCase(Vector3[] source,int[] faces,Vector3 start,Vector3 startT,Vector3 end,Vector3 endT,float scale,float handle)
    {
        var path=new SteamPipeShape.Sweep();var work=new Vector3[source.Length];var snapshot=source.ToArray();
        try { path.Build(start,startT,end,endT,Vector3.up,scale,handle); }
        catch(InvalidOperationException error) when(error.Message=="Steam pipe bend is too tight for its diameter.") {
            Check(path.MinimumBendRadiusM<.1016f*scale*1.25f,"Rejected curve exceeded the safe radius bound");rejectedTightBends++;return;
        }
        path.Deform(source,work);sweeps++;
        minimumSweepBendRadius=Math.Min(minimumSweepBendRadius,path.MinimumBendRadiusM);
        maximumTangentStep=Math.Max(maximumTangentStep,path.MaximumTangentStepRadians);
        var front=Ring(source,SteamPipeShape.SourceFrontZ);var rear=Ring(source,SteamPipeShape.SourceRearZ);
        float gap=Math.Max((Mean(work,front)-start).magnitude,(Mean(work,rear)-end).magnitude);
        maximumEndpointGap=Math.Max(maximumEndpointGap,gap);Check(gap<.00002f,"Flanged transfer endpoint missed its socket");
        foreach(bool first in new[]{true,false})
        {
            var region=Enumerable.Range(0,source.Length).Where(i=>first?source[i].z>=SteamPipeShape.SourceFrontZ-.17f:source[i].z<=SteamPipeShape.SourceRearZ+.17f).ToArray();
            int anchor=region[0];
            foreach(int i in region)
            {
                float expected=(source[i]-source[anchor]).magnitude*scale;
                float error=Math.Abs((work[i]-work[anchor]).magnitude-expected);
                maximumCollarError=Math.Max(maximumCollarError,error);Check(error<.00002f,"An original collar was stretched or bent");
            }
        }
        for(int i=0;i<faces.Length;i+=3)
        {
            var before=Vector3.Cross(source[faces[i+1]]-source[faces[i]],source[faces[i+2]]-source[faces[i]]);
            if(before.magnitude<1e-9f)continue;
            var after=Vector3.Cross(work[faces[i+1]]-work[faces[i]],work[faces[i+2]]-work[faces[i]]);
            Check(after.magnitude>1e-10f,"A swept pipe triangle collapsed");faceChecks++;
            var va=source[faces[i]];var vb=source[faces[i+1]];var vc=source[faces[i+2]];
            if(va.z<-.17f&&vb.z<-.17f&&vc.z<-.17f&&va.z>-2.8526f&&vb.z>-2.8526f&&vc.z>-2.8526f) {
                var centre=(work[faces[i]]+work[faces[i+1]]+work[faces[i+2]])/3;
                Check(Vector3.Dot(after,centre-path.Closest(centre))>=-1e-9f,$"A swept shaft face folded inward startY={start.y} endY={end.y} scale={scale} radius={path.MinimumBendRadiusM}");
            }
        }
        Check(source.SequenceEqual(snapshot),"Shared original pipe mesh changed");
    }

    private static void Main(string[] args)
    {
        using var assets=JsonDocument.Parse(File.ReadAllText(args[0]));using var casings=JsonDocument.Parse(File.ReadAllText(args[1]));using var fittings=JsonDocument.Parse(File.ReadAllText(args[2]));
        Native(casings.RootElement,fittings.RootElement);
        var pipe=assets.RootElement.GetProperty("meshes").EnumerateArray().Single(p=>p.GetProperty("name").GetString()=="steam_transfer");
        var sparse=Vertices(pipe);var sparseFaces=Triangles(pipe);
        var actualUv=pipe.GetProperty("uv").EnumerateArray().Select(v=>new Vector2(v[0].GetSingle(),v[1].GetSingle())).ToArray();
        var topology=SteamPipeShape.DenseTransfer(sparse,sparseFaces,actualUv);
        var sleeve=SteamPipeShape.DenseTransfer(sparse,sparseFaces,actualUv,192,false);
        for(int i=0;i<sparse.Length;i++)Check(topology.UV[i].x==actualUv[i].x&&topology.UV[i].y==actualUv[i].y,"Original collar UV changed");
        foreach(var item in new[]{topology,sleeve})for(int i=item==topology?sparse.Length:0;i<item.Vertices.Length;i++) {
            Check(item.UV[i].x>=.362f&&item.UV[i].x<=.383f&&item.UV[i].y>=.65f&&item.UV[i].y<=.753f,"Generated shaft UV escaped its original atlas island");
        }
        foreach(float length in new[]{.021f,.05f,.15f,.3f,1f}) {
            var straight=new SteamPipeShape.Sweep();var sleeveOutput=new Vector3[sleeve.Vertices.Length];
            var end=new Vector3(.01f,length,0);straight.BuildStraight(Vector3.zero,end,.8f);straight.Deform(sleeve.Vertices,sleeveOutput);
            Check(float.IsPositiveInfinity(straight.MinimumBendRadiusM),"Straight sleeve acquired curvature");
            var lo=Ring(sleeve.Vertices,SteamPipeShape.SourceFrontZ);var hi=Ring(sleeve.Vertices,SteamPipeShape.SourceRearZ);
            Check((Mean(sleeveOutput,lo)-Vector3.zero).magnitude<.00001f&&(Mean(sleeveOutput,hi)-end).magnitude<.00001f,"Short sleeve cap missed socket");
            foreach(var point in sleeveOutput)Check(Math.Abs((point-straight.Closest(point)).magnitude-.08128f)<.00001f,"Short sleeve diameter changed");
        }
        var source=topology.Vertices;var faces=topology.Triangles;
        foreach(var item in new[]{topology,sleeve}) {
            int first=item==topology?sparse.Length:0,width=14;
            Check((item.Vertices[first]-item.Vertices[first+width-1]).magnitude<.00001f,"Circumference seam has a geometric gap");
            Check(Math.Abs(item.UV[first].x-item.UV[first+width-1].x)>.015f,"Original authored UV seam was merged");
            foreach(int row in new[]{0,192})for(int col=0;col<width;col++) {
                int index=first+row*width+col;float oldZ=row==0?0f:-2.94639994f;
                Check(Enumerable.Range(0,sparse.Length).Any(i=>Math.Abs(sparse[i].z-oldZ)<.00001f&&Math.Abs(sparse[i].x-item.Vertices[index].x)<.00001f&&Math.Abs(sparse[i].y-item.Vertices[index].y)<.00001f&&Math.Abs(actualUv[i].x-item.UV[index].x)<.00001f&&Math.Abs(actualUv[i].y-item.UV[index].y)<.00001f),"Dense shaft boundary no longer meets original collar profile/UV");
            }
        }

        Check(source.Length>2000,"Dense shaft topology was not generated");
        float MaximumShaftSpan(Vector3[] v,int[] f) {
            float max=0;for(int i=0;i<f.Length;i+=3) {
                float lo=Math.Min(v[f[i]].z,Math.Min(v[f[i+1]].z,v[f[i+2]].z));
                float hi=Math.Max(v[f[i]].z,Math.Max(v[f[i+1]].z,v[f[i+2]].z));
                if(lo<-.001f&&hi> -2.945f)max=Math.Max(max,hi-lo);
            }return max;
        }
        Check(MaximumShaftSpan(sparse,sparseFaces)>.98f,"Original sparse mesh regression fixture changed");
        Check(MaximumShaftSpan(source,faces)<.016f,"Tube topology still has long bend-spanning faces");
        Check(topology.UV.Length==source.Length,"Generated texture coordinates missing");
        var denseFaces=faces.Select((value,index)=>(value,index)).Where(p=>p.index%3==0).Select(p=>(faces[p.index],faces[p.index+1],faces[p.index+2])).ToHashSet();
        for(int i=0;i<sparseFaces.Length;i+=3) {
            int a=sparseFaces[i],b=sparseFaces[i+1],c=sparseFaces[i+2];
            if(Math.Min(sparse[a].z,Math.Min(sparse[b].z,sparse[c].z))>=-.00001f||Math.Max(sparse[a].z,Math.Max(sparse[b].z,sparse[c].z))<=-2.94639f)
                Check(denseFaces.Contains((a,b,c)),"An authored collar triangle was removed or rewound");
        }

        for(int i=0;i<sparse.Length;i++)Check((source[i]-sparse[i]).magnitude==0,"Original collar/source vertices changed");
        // Regress the photographed tall elbow. A nonzero-area check misses its
        // inward faces; dense topology must maintain an outward tube surface.
        int Inward(Vector3[] v,int[] f,float h=.35f) {
            var p=new SteamPipeShape.Sweep();p.Build(new Vector3(1.09f,.9f,5),new Vector3(0,0,-1),new Vector3(1.16f,1.95f,4.2f),new Vector3(0,1,0),Vector3.up,.8f,h);
            var w=new Vector3[v.Length];p.Deform(v,w);int count=0;
            for(int i=0;i<f.Length;i+=3) {
                int a=f[i],b=f[i+1],c=f[i+2];
                if(v[a].z>-.17f||v[b].z>-.17f||v[c].z>-.17f||v[a].z<-2.8526f||v[b].z<-2.8526f||v[c].z<-2.8526f)continue;
                var centre=(w[a]+w[b]+w[c])/3;var radial=centre-p.Closest(centre);
                var normal=Vector3.Cross(w[b]-w[a],w[c]-w[a]);
                if(Vector3.Dot(normal,radial)<0)count++;
            }return count;
        }
        sparseInwardFaces=Inward(sparse,sparseFaces);denseInwardFaces=Inward(source,faces);
        Check(denseInwardFaces==0,"Dense tube has inward-facing shaft triangles");
        foreach(float scale in new[]{.55f,.65f,.75f,1f})foreach(float yaw in new[]{-12f,-6f,0f,6f,12f})foreach(float height in new[]{.9f,1.3f,1.6f,2f})foreach(int side in new[]{-1,1})
        {
            var inlet=Rotate(new Vector3(side*1.09f,height,5f),yaw);var direction=Rotate(new Vector3(0,0,-1),yaw);
            var joint=new Vector3(side*1.16f,1.95f,inlet.z-.8f);
            SweepCase(source,faces,inlet,direction,joint,new Vector3(0,height<1.95f?1:-1,0),scale,.35f);
        }
        foreach(float length in new[]{3f,7f,14f,22f})foreach(float tenderYaw in new[]{-25f,0f,25f})
        {
            var start=new Vector3(1.16f,1.95f,10f);var end=Rotate(new Vector3(1.16f,1.95f,10f-length),tenderYaw);
            SweepCase(source,faces,start,new Vector3(0,0,-1),end,Rotate(new Vector3(0,0,-1),tenderYaw),.65f,Math.Max(.3f,(end-start).magnitude*.3f));
        }
        var output=new {passed=true,checks,nativeCases,nativeRerouteCases,sweeps,faceChecks,rejectedTightBends,sparseInwardFaces,denseInwardFaces,minimumSweepBendRadius,maximumTangentStep,maximumEndpointGapM=maximumEndpointGap,maximumCollarErrorM=maximumCollarError,minimumNativeSocketHeightGapM=minimumNativeGap,
            limitation="Actual source pipe/manifold assets and native casing triangles, production pure normalization/socket/sweep methods. Runtime scene ownership, rendering, articulation timing and whole-loco clearance need separate validation."};
        File.WriteAllText(args[3],JsonSerializer.Serialize(output,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(output));
    }
}

