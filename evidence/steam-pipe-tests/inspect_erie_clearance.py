"""Local native-data evidence only; not a distributed mod asset."""
from pathlib import Path
import json, math
import numpy as np
import UnityPy
from UnityPy.helpers.MeshHelper import MeshHandler
from PIL import Image, ImageDraw

root=Path(__file__).resolve().parents[2]
out=root/'build/runtime-trial-1.22.1/erie-validation'
env=UnityPy.load(r'B:\SteamLibrary\steamapps\common\Derail Valley\DerailValley_Data\resources.assets')
parts=[]
def matrix(t):
 q=t.m_LocalRotation;x,y,z,w=q.x,q.y,q.z,q.w
 r=np.array([[1-2*y*y-2*z*z,2*x*y-2*z*w,2*x*z+2*y*w],[2*x*y+2*z*w,1-2*x*x-2*z*z,2*y*z-2*x*w],[2*x*z-2*y*w,2*y*z+2*x*w,1-2*x*x-2*y*y]])
 m=np.eye(4);s=t.m_LocalScale;p=t.m_LocalPosition;m[:3,:3]=r@np.diag([s.x,s.y,s.z]);m[:3,3]=[p.x,p.y,p.z];return m
def walk(ptr,parent):
 t=ptr.read();g=t.m_GameObject.deref().read();m=parent@matrix(t)
 for c in g.m_Component:
  obj=c.component.deref()
  if obj.type.name not in ['MeshFilter','SkinnedMeshRenderer']:continue
  ref=obj.read().m_Mesh
  if not ref.path_id:continue
  mesh=ref.deref().read();name=mesh.m_Name
  if name not in ['s282_tender','s282_tender_shaft','coal5']:continue
  h=MeshHandler(mesh);h.process();v=np.array(h.m_Vertices)[:,:3];v=(np.c_[v,np.ones(len(v))]@m.T)[:,:3]
  faces=np.array([f for group in h.get_triangles() for f in group])
  parts.append(dict(name=name,vertices=v,faces=faces))
 for p in t.m_Children:walk(p.deref(),m)
for id in [9735,9273]:
 g=next(o for o in env.objects if o.path_id==id).read()
 t=next(c.component.deref() for c in g.m_Component if c.component.deref().type.name=='Transform')
 walk(t,np.eye(4))
pipe=json.loads((out/'exhaust-mesh.json').read_text());pv=np.array(pipe['vertices']);pf=np.array(pipe['triangles']).reshape(-1,3)
coal=next(p for p in parts if p['name']=='coal5')['vertices']
hatch=next(p for p in parts if p['name']=='s282_tender_shaft')['vertices']
# Conservative whole-hatch spherical envelope about its native animation pivot:
# any pure hinge rotation remains inside this sphere, not only its closed pose.
pivot=np.array([.0008043944,3.04425025,-6.09045601])
radius=np.linalg.norm(hatch-pivot,axis=1).max()
hatch_gap=np.linalg.norm(np.clip(pivot,pv.min(0),pv.max(0))-pivot)-radius
coal_gap=coal[:,2].min()-pv[:,2].max()
assert hatch_gap>.15 and coal_gap>1.5
# Rear ladder's furthest forward measured rail is z=-7.434; allow 5cm thickness.
ladder_gap=pv[:,2].min()-(-7.434+.05)
assert ladder_gap>.1
report=dict(coalLongitudinalGapM=float(coal_gap),hatchAllAngleEnvelopeGapM=float(hatch_gap),
 rearLadderForwardRailGapM=float(ladder_gap),hatchEnvelopeRadiusM=float(radius),
 pipeBounds=[pv.min(0).tolist(),pv.max(0).tolist()],nativeMeshNames=[p['name'] for p in parts],
 limitation='Native mesh envelopes and measured ladder positions; intended inlet penetration and band/tank contact excluded from clearance requirements; no in-game rendering test')
(out/'native-clearance.json').write_text(json.dumps(report,indent=2))
im=Image.new('RGB',(1400,850),(24,28,32));draw=ImageDraw.Draw(im)
for axis,title,origin,scale in [(0,'SIDE  rear at left',(680,700),110),(1,'REAR  left side exhaust',(1100,700),140)]:
 def project(v):
  return (origin[0]+(v[2]+4)*scale,origin[1]-v[1]*scale) if axis==0 else (origin[0]-v[0]*scale,origin[1]-v[1]*scale)
 allparts=parts+[dict(name='exhaust',vertices=pv,faces=pf)]
 for p in allparts:
  colors={'s282_tender':(93,101,107),'coal5':(44,46,49),'s282_tender_shaft':(182,137,64),'exhaust':(75,180,210)}
  v=p['vertices'];faces=p['faces'];color=colors[p['name']]
  # Painter ordering is only a schematic preview; clearance uses actual arrays.
  for f in sorted(faces,key=lambda f:float(v[f,0 if axis==0 else 2].mean()),reverse=axis==0):
   draw.polygon([project(x) for x in v[f]],fill=color)
 draw.text((30 if axis==0 else 950,35),title,fill='white')
draw.text((30,790),'Native mesh schematic   blue = relocated exhaust + elbow + two original-mesh retaining bands   amber = water hatch',fill='white')
im.save(out/'clearance-preview.png')
print(json.dumps(report,indent=2))


