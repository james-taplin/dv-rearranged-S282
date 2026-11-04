"""Replay the existing imported-OBJ rod recipes; native geometry stays in build/."""
import argparse
import hashlib
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
# Independently reviewed against ff4dc6f, whose full source had the old guard.
# Current getSideRodMeshes additionally saves the native mesh and its lifetime;
# its vertex and face recipes remain the same. Fingerprint the recipe and its
# importer dependencies, rather than unrelated MeshFinder presentation code.
RECIPE_SHA = 'A715EAB109F26284C30CA8808E9D2911224AFCD33CFCADAE1116EC32C03A76A7'

TOKEN = re.compile(r'(?P<skip>\s+|//[^\n]*|/\*[\s\S]*?\*/)|(?P<token>\$?@"(?:""|[^"])*"|\$?"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'|[A-Za-z_]\w*|\d+(?:\.\d+)?[A-Za-z]*|[^\s])')

def tokens(source):
    return [m.group('token') for m in TOKEN.finditer(source) if m.group('token') is not None]

def fragment(source, signature, statement=False):
    items=tokens(source); needle=tokens(signature)
    matches=[i for i in range(len(items)-len(needle)+1) if items[i:i+len(needle)]==needle]
    if len(matches)!=1: raise ValueError('Rod recipe guard cannot identify exactly one '+signature)
    start=matches[0]; cursor=start+len(needle)
    if statement:
        while cursor<len(items) and items[cursor]!=';': cursor+=1
        if cursor==len(items): raise ValueError('Unterminated recipe statement: '+signature)
        return items[start:cursor+1]
    while cursor<len(items) and items[cursor]!='{': cursor+=1
    if cursor==len(items): raise ValueError('Missing recipe body: '+signature)
    depth=0
    while cursor<len(items):
        if items[cursor]=='{': depth+=1
        elif items[cursor]=='}':
            depth-=1
            if depth==0: return items[start:cursor+1]
        cursor+=1
    raise ValueError('Unterminated recipe body: '+signature)

def recipe_fingerprint(root=ROOT, overrides=None):
    specs={
        'RearrangedS282/MeshFinder.cs': [
            ('private static readonly String sideRodMeshName',True),
            ('readonly struct Range',False),
            *[('private static readonly Range[] '+name,False) for name in
              ('verticesToMoveBackward','verticesToMoveForward325','verticesToMoveForward015',
               'facesToHideFor4Coupled','facesToHideFor6Coupled')],
            ('private void getSideRodMeshes()',False)],
        'RearrangedS282/OBJImport/OBJLoader.cs': [
            ('public enum SplitMode',False),('public SplitMode SplitMode',True),
            ('internal List<Vector3> Vertices',True),('internal List<Vector3> Normals',True),
            ('internal List<Vector2> UVs',True),('public Mesh Load(Stream input)',False),
            ('private static int ResolveIndex(string part, int count, string token)',False),
            ('public Mesh Load(string path, string mtlPath)',False),('public Mesh Load(string path)',False)],
        'RearrangedS282/OBJImport/OBJObjectBuilder.cs': [('public class OBJObjectBuilder',False)],
        'RearrangedS282/OBJImport/CharWordReader.cs': [('public class CharWordReader',False)],
    }
    chunks=[]
    for name,parts in specs.items():
        source=overrides[name] if overrides and name in overrides else (root/name).read_text(encoding='utf-8-sig')
        for signature,is_statement in parts:
            chunks.append([name,signature,fragment(source,signature,is_statement)])
    return hashlib.sha256(json.dumps(chunks,ensure_ascii=True,separators=(',',':')).encode('utf-8')).hexdigest().upper()

def check_guard():
    mesh='RearrangedS282/MeshFinder.cs'; builder='RearrangedS282/OBJImport/OBJObjectBuilder.cs'
    source=(ROOT/mesh).read_text(encoding='utf-8-sig')
    importer=(ROOT/builder).read_text(encoding='utf-8-sig')
    baseline=recipe_fingerprint()
    if baseline!=RECIPE_SHA: raise AssertionError('Current recipe fingerprint has not been reviewed')
    if recipe_fingerprint(overrides={mesh:source+'\n// Unrelated documentation change\n'})!=baseline:
        raise AssertionError('An unrelated comment incorrectly invalidated the recipe')
    unrelated=source.replace('vertices[i].x = -vertices[i].x','vertices[i].x = vertices[i].x * -1f',1)
    if unrelated==source or recipe_fingerprint(overrides={mesh:unrelated})!=baseline:
        raise AssertionError('Unrelated linkage import code incorrectly invalidated the rod recipe')
    changed=source.replace('twoAxleSideRodVerts[i].z += 3.256f - 0.32f','twoAxleSideRodVerts[i].z += 3.257f - 0.32f',1)
    if recipe_fingerprint(overrides={mesh:changed})==baseline: raise AssertionError('Rod recipe change was not detected')
    changed=importer.replace('_vertices.Add(', '_vertices.Insert(0, ',1)
    if recipe_fingerprint(overrides={builder:changed})==baseline: raise AssertionError('OBJ vertex remapping change was not detected')
    changed=source.replace('new Range(102, 136)','new Range(103, 136)',1)
    if recipe_fingerprint(overrides={mesh:changed})==baseline: raise AssertionError('Rod range dependency change was not detected')
    print('PASS scoped rod recipe guard: unrelated comment ignored; recipe, range and importer changes detected')

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
    fingerprint = recipe_fingerprint()
    if fingerprint != RECIPE_SHA:
        raise ValueError('Rod recipe or OBJ mesh import dependencies changed: review fixture before updating its scoped guard')
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
    result = dict(nativeDataOnly=True,recipeSourceSha256=actual_sha,recipeDependencyFingerprint=fingerprint,
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
    p.add_argument('--check-guard',action='store_true')
    a=p.parse_args()
    if a.check_guard: check_guard()
    else: extract(a.assets,a.output)
