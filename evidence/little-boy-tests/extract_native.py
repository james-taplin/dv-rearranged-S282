"""Extract the two native bogie frames locally; never package the result."""
import argparse
import hashlib
import json
from pathlib import Path
import numpy as np
import UnityPy

game = Path(r'B:\SteamLibrary\steamapps\common\Derail Valley')
asset = game / 'DerailValley_Data/resources.assets'
parser = argparse.ArgumentParser()
parser.add_argument('--output', required=True, type=Path)
args = parser.parse_args()
environment = UnityPy.load(str(asset))
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
    rotation = np.array([[1-2*(y*y+z*z), 2*(x*y-z*w), 2*(x*z+y*w)],
                         [2*(x*y+z*w), 1-2*(x*x+z*z), 2*(y*z-x*w)],
                         [2*(x*z-y*w), 2*(y*z+x*w), 1-2*(x*x+y*y)]])
    result = np.eye(4)
    result[:3,:3] = rotation @ np.diag([s.x, s.y, s.z])
    result[:3,3] = [p.x, p.y, p.z]
    return matrix(t.m_Father.deref().read()) @ result if t.m_Father.m_PathID else result

wanted = ['LocoS282A/LocoS282A_Body/MovingParts_LOD0',
          'LocoS282A/Axle_F/bogie_car', 'LocoS282A/Axle_R/bogie_car']
transforms = {}
for obj in environment.objects:
    if obj.type.name == 'Transform':
        transform = obj.read()
        name = path(transform)
        if name in wanted:
            transforms[name] = matrix(transform)
if set(transforms) != set(wanted):
    raise ValueError('Missing native S282 body or bogie frames')
relative = [np.linalg.inv(transforms[wanted[0]]) @ transforms[name] for name in wanted[1:]]
if any(np.max(np.abs(value[:3,:3] - np.eye(3))) > 1e-5 for value in relative):
    raise ValueError('Native bogie basis changed; fixture needs rotation-aware transforms')
args.output.parent.mkdir(parents=True, exist_ok=True)
args.output.write_text(json.dumps(dict(resourcesSHA256=hashlib.sha256(asset.read_bytes()).hexdigest(),
    frontBogie=relative[0][:3,3].tolist(), rearBogie=relative[1][:3,3].tolist()), indent=2))
