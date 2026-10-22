import sys,json,re,hashlib
from pathlib import Path
sys.path.insert(0,str(Path.cwd()/'evidence/cylinder-casing-tests'))
from extract_native import obj
import UnityPy
from UnityPy.helpers.MeshHelper import MeshHandler
root=Path.cwd(); game=Path(r'B:\SteamLibrary\steamapps\common\Derail Valley'); out=root/'evidence/wheel-clearance-tests/native-clearance.json'
parts=[]
for lod in (0,1):
 ref=(root/f'evidence/cylinder-casing-tests/LMS_LOD{lod}.reference.txt').read_text(encoding='utf-8-sig')
 ranges={n:(float(lo.rstrip('f')),float(hi.rstrip('f'))) for n,lo,hi in re.findall(r'RangeFloat\s+(\w+)\s*=\s*new MeshSplittersUtil.RangeFloat\(([-.\df]+),\s*([-.\df]+)\)',ref)}
 body=ref.split('private static Mesh getAirTankMesh(',1)[1].split('\n\t}',1)[0]
 boxes=re.findall(r'markPartOfMesh\(val.vertices,\s*array\d*,\s*(\w+),\s*(\w+),\s*(\w+)\)',body)
 file=game/'Mods/LocoMeshSplitter/assets'/('s282_locomotive_body'+('_LOD1' if lod else '')+'.obj');v,f=obj(file)
 chosen=[face for face in f if any(all(all(ranges[n][0]<=v[i][a]<=ranges[n][1] for a,n in enumerate(box)) for i in face) for box in boxes)]
 ids=sorted(set(i for face in chosen for i in face));remap={i:n for n,i in enumerate(ids)}
 points=[[v[i][0],v[i][1],v[i][2]+(4.88 if lod==0 else 4.885)] for i in ids]
 parts.append(dict(lod=lod,vertices=points,triangles=[[remap[i] for i in face] for face in chosen]))
env=UnityPy.load(str(game/'DerailValley_Data/resources.assets'));names={}
def path(t):
 key=t.object_reader.path_id
 if key not in names:names[key]=(path(t.m_Father.deref().read())+'/' if t.m_Father.m_PathID else '')+t.m_GameObject.deref().read().m_Name
 return names[key]
wheels=[]
for o in env.objects:
 if o.type.name!='Transform':continue
 t=o.read();p=path(t)
 match=re.fullmatch(r'LocoS282A/LocoS282A_Body/MovingParts_LOD0/LocoS282A_Drivetrain R/s282_wheels_driving_([1-4])(?:/s282_wheels_driving_[1-4]_LOD1)?',p)
 if not match:continue
 lod=int(p.endswith('_LOD1'));position=t.m_LocalPosition
 if lod:
  assert max(abs(position.x),abs(position.y),abs(position.z))<1e-5
  position=t.m_Father.deref().read().m_LocalPosition
 go=t.m_GameObject.deref().read()
 mf=next(c.component.deref().read() for c in go.m_Component if c.component.deref().type.name=='MeshFilter')
 mesh=mf.m_Mesh.deref().read();handler=MeshHandler(mesh);handler.process()
 wheels.append(dict(path=p,axle=int(match.group(1)),lod=lod,name=mesh.m_Name,position=[position.x,position.y,position.z],scale=[t.m_LocalScale.x,t.m_LocalScale.y,t.m_LocalScale.z],rotation=[t.m_LocalRotation.x,t.m_LocalRotation.y,t.m_LocalRotation.z,t.m_LocalRotation.w],vertices=[list(v[:3]) for v in handler.m_Vertices],triangles=[list(t) for sub in handler.get_triangles() for t in sub]))
assert len(wheels)==8,len(wheels)
data=dict(note='Actual native mesh data, local evidence only; never distribute',tanks=parts,wheels=wheels)
out.write_text(json.dumps(data,separators=(',',':')))
print(json.dumps(dict(tanks=[dict(lod=p['lod'],vertices=len(p['vertices']),min=[min(v[a] for v in p['vertices']) for a in range(3)],max=[max(v[a] for v in p['vertices']) for a in range(3)]) for p in parts],wheels=[dict(name=w['name'],position=w['position'],scale=w['scale'],rotation=w['rotation'],vertices=len(w['vertices']),min=[min(v[a] for v in w['vertices']) for a in range(3)],max=[max(v[a] for v in w['vertices']) for a in range(3)]) for w in wheels]),indent=2))
