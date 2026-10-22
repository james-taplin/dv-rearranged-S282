"""Replay the existing imported-OBJ rod recipes; native geometry stays in build/."""
import argparse
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
RECIPE_SHA = '6FCB0DEC033FDB70B77883ECAFFA29401DBE5331BE2302FFCF016F28E6F3C2A3'

def load_obj(path):
    # Match OBJObjectBuilder's first-use (position, UV, normal) remapping.
    raw, vertices, triangles, remap = [], [], [], {}
    for line in path.read_text(encoding='utf-8-sig').splitlines():
        if line.startswith('v '):
            raw.append([float(v) for v in line.split()[1:]])
        elif line.startswith('f '):
            face = []
            for text in line.split()[1:]:
                key = tuple(int(value) if value else -2**31 for value in text.split('/'))
                if key not in remap:
                    remap[key] = len(vertices)
                    vertices.append(raw[key[0]-1][:])
                face.append(remap[key])
            if len(face) == 3: triangles.append(face)
            elif len(face) == 4: triangles.extend([face[:3], [face[2],face[3],face[0]]])
            else:
                triangles.extend([[face[0],face[i-1],face[i]] for i in range(len(face)-1,1,-1)])
    return vertices, triangles

def indices(ranges): return [i for start,end in ranges for i in range(start,end)]

def extract(assets, target):
    recipe = ROOT/'RearrangedS282/MeshFinder.cs'
    actual_sha = hashlib.sha256(recipe.read_bytes()).hexdigest().upper()
    if actual_sha != RECIPE_SHA:
        raise ValueError('MeshFinder recipe changed: review this fixture before updating its source guard')
    path = assets/'s282_mech_wheels_connect.obj'
    v, f = load_obj(path)
    if len(v) != 508 or len(f) != 524:
        raise ValueError('Unsupported native OBJ topology: review recipes and pin measurements')
    two = [p[:] for p in v]
    for i in indices([(34,68),(269,270),(349,380),(413,445)])+list(range(152,211,2)):
        two[i][2] += 3.256-.32
    for i in indices([(471,473),(475,476),(312,314),(316,317)])+[173,175,237,239]:
        two[i][2] += .15-.32
    hidden2 = set(indices([(453,488),(384,421),(349,352),(230,313),(128,146),(64,96),(0,32)])+
                  list(range(147,166,2))+list(range(202,229,2)))
    hidden3 = set(list(range(131,216,2))+indices([(0,32),(80,130),(217,244),(260,262),
                  (270,349),(384,386),(402,410),(453,524)])+
                  [246,248,250,251,252,254,256,387,389,393,395,397,398,399])
    five = [p[:] for p in v]
    for i in indices([(102,136),(317,349),(476,508)])+list(range(177,236,2)):
        five[i][2] -= .15
    lv, lf = load_obj(assets/'s282_mech_wheels_connect_LOD1.obj')
    def mesh(name, vertices, faces, pins):
        return dict(name=name,vertices=vertices,triangles=[x for face in faces for x in face],pins=pins)
    result = dict(nativeDataOnly=True,recipeSourceSha256=actual_sha,
        nativeObjSha256=hashlib.sha256(path.read_bytes()).hexdigest().upper(),
        sourceNote='Local installed native assets; never include this generated JSON in a release.',
        nominalToNativeBaseline=dict(
            rodPivotZ=6.3024797439575195,
            nativeWheelZ=[3.1960253715515137,4.749501705169678,6.3025617599487305,8.00187873840332],
            innerBearingFitZ=[-3.10558016,-1.55278050,.000005998,1.69832512],
            maximumNativeWheelBearingMismatchM=.001073874),
        meshes=[mesh('native',v,f,[-3.1,-1.55,0,1.7]),
            mesh('nativeLOD1',lv,lf,[-3.1,-1.55,0,1.7]),
            mesh('two',two,[face for i,face in enumerate(f) if i not in hidden2],[-.17,1.7]),
            mesh('threeHalf',v,[face for i,face in enumerate(f) if i not in hidden3],[-3.1,-1.55]),
            mesh('fiveSix',five,f,[-3.1,-1.55,0,1.55])])
    target.parent.mkdir(parents=True,exist_ok=True)
    target.write_text(json.dumps(result,separators=(',',':')),encoding='utf-8')
    print(json.dumps(dict(output=str(target),meshes=[dict(name=m['name'],vertices=len(m['vertices']),triangles=len(m['triangles'])//3) for m in result['meshes']])))

if __name__ == '__main__':
    p=argparse.ArgumentParser()
    p.add_argument('--assets',type=Path,default=Path(r'B:\SteamLibrary\steamapps\common\Derail Valley\Mods\RearrangedS282\assets'))
    p.add_argument('--output',type=Path,default=ROOT/'build/rod-remesher-validation/native-rods.json')
    a=p.parse_args();extract(a.assets,a.output)
