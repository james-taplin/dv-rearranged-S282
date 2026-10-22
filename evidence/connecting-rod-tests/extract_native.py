"""Local-only native motion mesh fixture. Never ship the generated JSON."""
import argparse
import hashlib
import json
from pathlib import Path
import UnityPy
from UnityPy.helpers.MeshHelper import MeshHandler

ROOT=Path(__file__).resolve().parents[2]
NAMES=['s282_mech_push_rod','s282_mech_push_rod_to_cutoff_base',
    's282_mech_cutoff_rod_hinge','s282_mech_push_rod_to_cutoff_rod','s282_mech_cylinder_rod',
    's282_mech_push_rod_to_connect','s282_mech_cutoff_to_cylinder_rod']

def extract(target):
    assets=Path(r'B:\SteamLibrary\steamapps\common\Derail Valley\DerailValley_Data\resources.assets')
    env=UnityPy.load(str(assets));wanted={name+suffix for name in NAMES for suffix in ['', '_LOD1']};meshes={};lods={}
    for obj in env.objects:
        if obj.type.name=='LODGroup':
            group=obj.read();go=group.m_GameObject.deref().read()
            if go.m_Name in ['LocoS282A_Drivetrain L','LocoS282A_Drivetrain R']:
                raw=obj.read_typetree();p=raw['m_LocalReferencePoint']
                lods[go.m_Name]=dict(referencePoint=[p['x'],p['y'],p['z']],size=raw['m_Size'],
                    levels=[dict(threshold=l['screenRelativeHeight'],fade=l['fadeTransitionWidth']) for l in raw['m_LODs']])
        if obj.type.name!='Mesh':continue
        mesh=obj.read()
        if mesh.m_Name not in wanted:continue
        h=MeshHandler(mesh);h.process()
        meshes[mesh.m_Name]=dict(name=mesh.m_Name,vertices=[list(v[:3]) for v in h.m_Vertices],
            triangles=[list(face) for sub in h.get_triangles() for face in sub],
            nativeReadable=bool(mesh.m_IsReadable))
    if set(meshes)!=wanted:raise ValueError('Missing native motion meshes: '+str(wanted-set(meshes)))
    if len(lods)!=2:raise ValueError('Missing native drivetrain LOD metadata')
    out=dict(nativeDataOnly=True,resourcesSha256=hashlib.sha256(assets.read_bytes()).hexdigest().upper(),meshes=meshes,lods=lods)
    target.parent.mkdir(parents=True,exist_ok=True);target.write_text(json.dumps(out,separators=(',',':')),encoding='utf-8')
    print(json.dumps(dict(output=str(target),meshes={name:dict(vertices=len(m['vertices']),triangles=len(m['triangles']),readable=m['nativeReadable']) for name,m in meshes.items()})))

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--output',type=Path,default=ROOT/'build/connecting-rod-validation/native-motion-meshes.json')
    extract(p.parse_args().output)
