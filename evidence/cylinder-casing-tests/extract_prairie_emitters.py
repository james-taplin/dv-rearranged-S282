"""Read-only native S282 particle hierarchy fixture. Output stays local/ignored."""
import hashlib
import json
from pathlib import Path
import UnityPy

source=Path(r'B:\SteamLibrary\steamapps\common\Derail Valley\DerailValley_Data\resources.assets')
env=UnityPy.load(str(source))
records=[]
def walk(t,path):
    go=t.m_GameObject.read()
    path=path+'/'+go.m_Name if path else go.m_Name
    ps=[c.component for c in go.m_Component if c.component.type.name=='ParticleSystem']
    records.append(dict(path=path,position=[t.m_LocalPosition.x,t.m_LocalPosition.y,t.m_LocalPosition.z],
        rotation=[t.m_LocalRotation.x,t.m_LocalRotation.y,t.m_LocalRotation.z,t.m_LocalRotation.w],
        scale=[t.m_LocalScale.x,t.m_LocalScale.y,t.m_LocalScale.z],particle=bool(ps)))
    for p in ps:
        shape=p.read_typetree()['ShapeModule']
        if shape.get('m_Position',{}) != dict(x=0.0,y=0.0,z=0.0):
            raise ValueError('Native particle shape has an extra position offset: '+path)
    for child in t.m_Children:walk(child.read(),path)
for obj in env.objects:
    if obj.type.name!='GameObject':continue
    go=obj.read()
    if go.m_Name not in ['CylCockSteam','CylCockWaterDrip','CylinderCrack']:continue
    t=next(c.component for c in go.m_Component if c.component.type.name=='Transform').read()
    parent=t.m_Father.read()
    if parent.m_GameObject.read().m_Name=='LocoS282A_Particles':walk(t,'LocoS282A_Particles')
if sum(r['particle'] for r in records)!=12:raise ValueError('Unexpected S282 native particle count')
output=Path(__file__).with_name('native-prairie-emitters.json')
output.write_text(json.dumps(dict(note='Local native fixture only; not a runtime/source asset',
    sourceSHA256=hashlib.sha256(source.read_bytes()).hexdigest(),transforms=records),indent=2))
print('Extracted 12 native particle systems; shape origin offsets are zero')
