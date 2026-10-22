# Local native geometry is input only; never distribute generated JSON or mesh exports.
import sys,json,re,numpy as np
from pathlib import Path
r=Path(__file__).resolve().parents[2];output=Path(sys.argv[1]);game=Path(sys.argv[2]);sys.path.insert(0,str(r/'evidence/cylinder-casing-tests'))
from extract_native import obj
def segment_hits(start,end,tri):
    direction=end-start;e1=tri[:,1]-tri[:,0];e2=tri[:,2]-tri[:,0]
    h=np.cross(direction,e2);det=(e1*h).sum(axis=1)
    good=np.abs(det)>1e-10;inv=np.zeros_like(det);inv[good]=1/det[good]
    s=start-tri[:,0];u=inv*(s*h).sum(axis=1);q=np.cross(s,e1)
    b=inv*(direction*q).sum(axis=1);t=inv*(e2*q).sum(axis=1)
    # Surface crossings only; common endpoints/edges are not called defects.
    good&=(u>1e-7)&(b>1e-7)&(u+b<1-1e-7)&(t>1e-7)&(t<1-1e-7)
    return start[good]+direction[good]*t[good,None]

def crossings(a,b):
    amin=a.min(axis=1);amax=a.max(axis=1);bmin=b.min(axis=1);bmax=b.max(axis=1)
    use=((bmax>=amin.min(axis=0)-1e-6)&(bmin<=amax.max(axis=0)+1e-6)).all(axis=1)
    b=b[use];bmin=bmin[use];bmax=bmax[use]
    if not len(b):return np.empty((0,3))
    result=[]
    for tri,lo,hi in zip(a,amin,amax):
        candidates=b[((bmax>=lo-1e-6)&(bmin<=hi+1e-6)).all(axis=1)]
        if not len(candidates):continue
        for i,j in [(0,1),(1,2),(2,0)]:
            starts=np.repeat(tri[i,None],len(candidates),axis=0);ends=np.repeat(tri[j,None],len(candidates),axis=0)
            hits=segment_hits(starts,ends,candidates)
            if len(hits):result.extend(hits)
            hits=segment_hits(candidates[:,i],candidates[:,j],np.repeat(tri[None],len(candidates),axis=0))
            if len(hits):result.extend(hits)
    return np.unique(np.round(result,5),axis=0).reshape(-1,3) if len(result) else np.empty((0,3))

# Discriminating probes: crossing surfaces must register, a nearby separated
# pair must not, and the result must survive the same world reflection used by
# LMS. Coplanar and enclosed-solid cases deliberately remain outside this test.
probe_a=np.array([[[0.,0.,0.],[1.,0.,0.],[0.,1.,0.]]])
probe_b=np.array([[[.2,.2,-1.],[.2,.2,1.],[.8,.2,.1]]])
assert len(crossings(probe_a,probe_b))>0
assert len(crossings(probe_a,probe_b+np.array([0.,0.,3.])))==0
assert len(crossings(probe_a*np.array([-1.,1.,1.]),probe_b*np.array([-1.,1.,1.])))>0


v,f=obj(game/'Mods/LocoMeshSplitter/assets/s282_locomotive_body.obj');v=np.array(v);f=np.array(f);ref=(r/'evidence/cylinder-casing-tests/LMS_LOD0.reference.txt').read_text(encoding='utf-8-sig');rs={n:(float(lo.rstrip('f')),float(hi.rstrip('f'))) for n,lo,hi in re.findall(r'RangeFloat\s+(\w+)\s*=\s*new MeshSplittersUtil.RangeFloat\(([-.\df]+),\s*([-.\df]+)\)',ref)};airs=[]
for name in ['getAirPumpInputPipeMesh','getAirPumpOutputPipeMesh','getAirPumpToTankLPipeMesh']:
 b=ref.split('private static Mesh '+name+'(',1)[1].split('\n\t}',1)[0];sel=np.zeros(len(f),bool)
 for box in re.findall(r'markPartOfMesh\(val.vertices,\s*array\d*,\s*(\w+),\s*(\w+),\s*(\w+)\)',b):
  m=np.ones(len(v),bool)
  for a,n in enumerate(box):lo,hi=rs[n];m&=(v[:,a]>=lo)&(v[:,a]<=hi)
  sel|=m[f].all(1)
 air=v[f[sel]].copy();air[:,:,0]*=-1;air[:,:,2]+=4.88
 if 'Output' in name:air[:,:,2]=(air[:,:,2]-4.88)*1.12+4.88-.58
 if 'Input' in name:air[:,:,2]+=.07
 airs.append((name,air))
rows=[]
for part in json.loads((output/'lower-collars.json').read_text()):
 f=np.array(part['triangles']).reshape(-1,3)
 for kind,key in [('original','originalVertices'),('compact','vertices')]:
  v=np.array(part[key]);tri=v[f];tri=tri[tri[:,:,1].max(1)>1.7607]
  for name,air in airs:
   hits=crossings(tri,air);rows.append(dict(lod=part['lod'],side=part['side'],kind=kind,air=name,crossingPoints=len(hits),sample=hits[:8].tolist()))
assert any(q['crossingPoints'] for q in rows if q['kind']=='original' and q['side']=='left'),'expected actual regression absent'
assert not any(q['crossingPoints'] for q in rows if q['kind']=='compact'),'compact pipe still crosses native compressor plumbing'
out=dict(passed=True,rows=rows,limits='Actual compiled collar helper output, both pipe LODs/sides versus relocated LOD0 native compressor pipes. Noncoplanar surface crossings only; neutral pose, no claim of minimum distance or accessory-mod clearance.')
(output/'collar-air-clearance.json').write_text(json.dumps(out,indent=2))
print(json.dumps(out,indent=2))
