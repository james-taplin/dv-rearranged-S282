import json,re,numpy as np
from pathlib import Path
root=Path(__file__).parent
prairieSource=Path('RearrangedS282/WheelRearranger.PrairieSpacing.cs').read_text()
prairieForward=float(re.search(r'PrairieDriverForward = ([-.0-9]+)f',prairieSource).group(1))
r=json.loads((root/'native-clearance.json').read_text()); wheels={w['axle']:w for w in r['wheels'] if w['lod']==0}
points=[]
for part in r['tanks']:
 v=np.array(part['vertices'])
 for face in part['triangles']:
  a,b,c=v[face];n=max(1,int(np.ceil(max(np.linalg.norm(b-a),np.linalg.norm(c-a),np.linalg.norm(c-b))/.012)))
  i,j=np.tril_indices(n+1);points.append(a+(b-a)*(j[:,None]/n)+(c-a)*((i-j)[:,None]/n))
p=np.vstack(points);p[:,0]=abs(p[:,0]);profiles={}
for w in r['wheels']:
 i=w['axle']
 v=np.array(w['vertices']);tris=v[w['triangles']];bins=np.arange(0,1.25,.001);pr=[]
 for x in bins:
  radii=[]
  for edge in range(3):
   a,b=tris[:,edge,:],tris[:,(edge+1)%3,:];dx=b[:,0]-a[:,0]
   valid=(abs(dx)>1e-8)&(np.minimum(a[:,0],b[:,0])<=x)&(np.maximum(a[:,0],b[:,0])>=x)
   t=(x-a[valid,0])/dx[valid];yz=a[valid,1:]+(b[valid,1:]-a[valid,1:])*t[:,None]
   radii.extend(np.linalg.norm(yz,axis=1))
   flat=(abs(dx)<=1e-8)&(abs(a[:,0]-x)<.0011)
   radii.extend(np.linalg.norm(a[flat,1:],axis=1));radii.extend(np.linalg.norm(b[flat,1:],axis=1))
  pr.append(max(radii,default=0))
 pr=np.array(pr);pr=np.maximum(np.maximum(pr,np.roll(pr,1)),np.roll(pr,-1))
 profile=pr[np.clip(np.round(p[:,0]*1000).astype(int),0,len(pr)-1)]
 profiles[i]=np.maximum(profiles.get(i,np.zeros_like(profile)),profile)

def gap(s,kind,anchor=True):
 front=2 if kind=='prairie' else 1; old=1.2879 if kind=='prairie' else 1.198
 offset=-2.1+prairieForward if kind=='prairie' else -1.5
 if anchor:offset+=(old-s)*wheels[front]['position'][2]
 tank=p.copy();tank[:,2]-=.1 if kind=='prairie' else 0
 out=np.inf
 for i in range(front,5):
  w=wheels[i];profile=profiles[1 if i==2 and kind=='prairie' else i]
  d=np.sqrt((tank[:,1]-w['position'][1]*s)**2+(tank[:,2]-(w['position'][2]*s+offset))**2)-s*profile
  out=min(out,float(d[profile>0].min()))
 return out
if __name__=='__main__':
 import re
 source=Path('RearrangedS282/WheelRearranger.LargeDriverClearance.cs').read_text()
 scale=float(re.search(r'ReservoirClearDriverScale = ([.0-9]+)f',source).group(1))
 rows=[]
 for kind,old in [('prairie',1.2879),('highspeed',1.198)]:
  before,after=gap(old,kind),gap(scale,kind)
  assert before<0 and after>.018,(kind,before,after)
  rows.append(dict(layout=kind,oldScale=old,newScale=scale,oldMinimumGapM=before,newMinimumGapM=after,diameterM=2*(.9144 if kind=='prairie' else .8509)*scale/old,reductionPercent=100*(1-scale/old)))
 report=dict(passed=True,sampledReservoirPoints=len(p),reservoirLODs=2,wheelLODs=2,rows=rows,limitations='Actual native reservoir triangles and wheel meshes with full rotational envelopes, 12 mm tank-face sampling and 1 mm conservative radial profile bins. No game rendering, suspension or live transforms. Prairie translated 0.70 m rearward from 1.13.5; high-speed positions unchanged; native layout tank offset included.')
 out=Path('build/prairie-rearward-1.13.6');out.mkdir(parents=True,exist_ok=True);(out/'geometry.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
