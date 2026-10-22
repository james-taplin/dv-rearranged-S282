"""Local-only native geometry fixture; no game/mod files are written."""
import hashlib
import json
from pathlib import Path
import numpy as np
import UnityPy
from UnityPy.helpers.MeshHelper import MeshHandler

root = Path(__file__).resolve().parents[2]
game = Path(r'B:\SteamLibrary\steamapps\common\Derail Valley')
assets = game / 'DerailValley_Data/resources.assets'
env = UnityPy.load(str(assets))
names = {}

def path(t):
    key = t.object_reader.path_id
    if key not in names:
        name = t.m_GameObject.deref().read().m_Name
        names[key] = (path(t.m_Father.deref().read()) + '/' if t.m_Father.m_PathID else '') + name
    return names[key]

def matrix(t):
    p, q, s = t.m_LocalPosition, t.m_LocalRotation, t.m_LocalScale
    x, y, z, w = q.x, q.y, q.z, q.w
    r = np.array([[1-2*(y*y+z*z), 2*(x*y-z*w), 2*(x*z+y*w)],
                  [2*(x*y+z*w), 1-2*(x*x+z*z), 2*(y*z-x*w)],
                  [2*(x*z-y*w), 2*(y*z+x*w), 1-2*(x*x+y*y)]])
    m = np.eye(4)
    m[:3, :3] = r @ np.diag([s.x, s.y, s.z])
    m[:3, 3] = [p.x, p.y, p.z]
    return matrix(t.m_Father.deref().read()) @ m if t.m_Father.m_PathID else m

wanted_paths = ['LocoS282A/LocoS282A_Body/MovingParts_LOD0', 'LocoS282A/Axle_R/bogie_car']
wanted_meshes = ['s282_mech_push_rod_to_connect', 's282_mech_push_rod_to_connect_LOD1',
                 's282_mech_cutoff_to_cylinder_rod', 's282_mech_cutoff_to_cylinder_rod_LOD1']
transforms, meshes = {}, {}
for obj in env.objects:
    if obj.type.name == 'Transform':
        t = obj.read()
        p = path(t)
        if p in wanted_paths:
            transforms[p] = matrix(t)
    elif obj.type.name == 'Mesh':
        mesh = obj.read()
        if mesh.m_Name in wanted_meshes:
            handler = MeshHandler(mesh)
            handler.process()
            b = mesh.m_LocalAABB
            meshes[mesh.m_Name] = dict(vertices=[list(v) for v in handler.m_Vertices],
                boundsMin=[b.m_Center.x-b.m_Extent.x,b.m_Center.y-b.m_Extent.y,b.m_Center.z-b.m_Extent.z],
                boundsMax=[b.m_Center.x+b.m_Extent.x,b.m_Center.y+b.m_Extent.y,b.m_Center.z+b.m_Extent.z])
assert set(meshes) == set(wanted_meshes), set(wanted_meshes)-set(meshes)
body = transforms[wanted_paths[0]]
rear = np.linalg.inv(body) @ transforms[wanted_paths[1]]
assert np.max(np.abs(rear[:3,:3]-np.eye(3))) < 1e-5
# Native LMS split cylinder front: independently measured native OBJ extremum.
obj_path = game / 'Mods/LocoMeshSplitter/assets/s282_locomotive_body.obj'
front = max(float(line.split()[3]) for line in obj_path.read_text(encoding='utf-8-sig').splitlines()
            if line.startswith('v ') and .74 <= float(line.split()[1]) <= 1.5
            and 0 <= float(line.split()[2]) <= 1.7 and 4.15 <= float(line.split()[3]) <= 5.35) + 4.88
out = dict(note='Native mesh data stays local; never included in runtime ZIP.',
           sources=dict(resourcesSHA256=hashlib.sha256(assets.read_bytes()).hexdigest(),
                        nativeObjSHA256=hashlib.sha256(obj_path.read_bytes()).hexdigest()),
           rearBogiePosition=rear[:3,3].tolist(), cylinderFront=front, meshes=meshes)
target = Path(__file__).with_name('native-spacing.json')
target.write_text(json.dumps(out, separators=(',',':')), encoding='utf-8')
print(json.dumps(dict(rearBogiePosition=out['rearBogiePosition'], cylinderFront=front,
                     meshes={k:len(v['vertices']) for k,v in meshes.items()})))
