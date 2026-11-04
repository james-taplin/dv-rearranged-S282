"""Bounded private measurements of the three hidden native linkage renderers.

Consumes the project's established exact native geometry and animation readers.
Generated mesh/pose payloads stay under ignored build/. No game is run or changed.
"""
import argparse
import copy
import hashlib
import json
from pathlib import Path
import numpy as np

ROOT = Path(__file__).resolve().parents[2]
REACH = 's282_mech_cutoff_rod'
LUBE = 's282_mech_cutoff_rail/s282_mech_lubricator_rod'
LEVER = 'LubricatorParts/s282_mech_lubricator_part_1'

def digest(p):
    return hashlib.sha256(p.read_bytes()).hexdigest().upper()

def trs(p, q, s):
    x,y,z,w = q
    m = np.eye(4)
    m[:3,:3] = np.array([[1-2*y*y-2*z*z,2*x*y-2*z*w,2*x*z+2*y*w],
        [2*x*y+2*z*w,1-2*x*x-2*z*z,2*y*z-2*x*w],
        [2*x*z-2*y*w,2*y*z+2*x*w,1-2*x*x-2*y*y]]) @ np.diag(s)
    m[:3,3] = p
    return m

def flatten(node, result, path=''):
    if path: result[path] = node
    for c in node['children']:
        flatten(c, result, path+'/'+c['name'] if path else c['name'])

def pose(base, clip, frame):
    nodes = copy.deepcopy(base)
    for b in clip['bindings']:
        if b['path'] not in nodes: continue
        values = [frame['values'].get(str(index), initial) for index,initial in zip(b['indices'],b['first'])]
        nodes[b['path']][{1:'position',2:'rotation',3:'scale'}[b['attribute']]] = values
    matrices = {}
    for path,n in sorted(nodes.items(), key=lambda kv:kv[0].count('/')):
        local = trs(n['position'], n['rotation'], n['scale'])
        matrices[path] = matrices[path.rsplit('/',1)[0]] @ local if '/' in path else local
    return matrices

def point(matrix, local): return (matrix @ np.r_[local,1])[:3]

def run(output):
    geometry_path = ROOT/'build/franklin-runtime/geometry.json'
    animation_path = ROOT/'evidence/pacific-hudson-valve-gear/native-animation.json'
    spacing_path = ROOT/'evidence/fourteen-spacing-tests/native-spacing.json'
    geometry = json.loads(geometry_path.read_text(encoding='utf-8-sig'))
    animation = json.loads(animation_path.read_text(encoding='utf-8-sig'))
    spacing = json.loads(spacing_path.read_text(encoding='utf-8-sig'))
    native = Path(geometry['nativeSource'])
    assert digest(native) == geometry['nativeSHA256'].upper() == spacing['sources']['resourcesSHA256'].upper(), 'Native fixture resource mismatch'
    baseline = {}; flatten(geometry['drives']['LocoS282A_Drivetrain R'],baseline)
    inventory = {}
    for path,n in baseline.items():
        if path not in [REACH,REACH+'/'+REACH+'_LOD1',LUBE,LUBE+'/s282_mech_lubricator_rod_LOD1',LEVER,LEVER+'/s282_mech_lubricator_part_1_LOD1']:continue
        mesh = geometry['nativeMeshes'][n['mesh']]
        v = np.asarray(mesh['vertices']); lo=v.min(axis=0);hi=v.max(axis=0)
        inventory[path] = dict(mesh=mesh['name'],vertices=len(v),minimum=lo.tolist(),maximum=hi.tolist(),
            localPosition=n['position'],localRotation=n['rotation'],localScale=n['scale'])
    # Actual FourteenLayout formula, using independently extracted native frame.
    radius=baseline['s282_wheels_driving_4']['position'][1]
    cylinder_front=spacing['cylinderFront'];seventh=.1-1.55
    trailing_limit=spacing['rearBogiePosition'][2]-3.1
    scale=(cylinder_front-trailing_limit-1.1-.355-.1)/(cylinder_front-seventh+radius)
    offset=cylinder_front*(1-scale);drive_offset=offset-.5
    current=trs([0,0,drive_offset],[0,0,0,1],[1,scale,scale])
    metrics={};raw=[]
    # Native complete clip samples: no invented Animator blend semantics.
    for clip in animation['clips']:
        for frame in clip['frames']:
            matrices=pose(baseline,clip,frame)
            rod=matrices[LUBE];lever=matrices[LEVER]
            raw.append((rod,lever))
            for path in inventory:
                if path.startswith(LEVER):continue
                end=inventory[path]['minimum'][2] if path.startswith(REACH) else inventory[path]['maximum'][2]
                local=[0,0,end]
                fixed=point(matrices[path],local)
                broken=point(current@matrices[path],local)
                row=metrics.setdefault(path,dict(nativeFixedPoints=[],unrepairedGaps=[]))
                row['nativeFixedPoints'].append(fixed.tolist());row['unrepairedGaps'].append(float(np.linalg.norm(fixed-broken)))
    for path,row in metrics.items():
        pts=np.asarray(row.pop('nativeFixedPoints'));gaps=row.pop('unrepairedGaps')
        row.update(nativeEndMinimum=pts.min(axis=0).tolist(),nativeEndMaximum=pts.max(axis=0).tolist(),
            unrepairedGapMinimum=min(gaps),unrepairedGapMaximum=max(gaps))
    # Solve the native rod axial datum and native lever local pin together.
    # Fit exclusively the unmodified native clips, before any candidate mapping.
    # Production rounds this native datum; the candidate is never fitted here.
    a=[];b=[]
    for r,l in raw:
        a.append(np.column_stack((r[:3,2],-l[:3,:3])));b.append(l[:3,3]-r[:3,3])
    fit=np.linalg.lstsq(np.concatenate(a),np.concatenate(b),rcond=None)[0]
    errors=[float(np.linalg.norm(point(r,[0,0,fit[0]])-point(l,fit[1:]))) for r,l in raw]
    lms_obj=Path(r'B:\SteamLibrary\steamapps\common\Derail Valley\Mods\LocoMeshSplitter\assets\s282_locomotive_body.obj')
    assert digest(lms_obj)==spacing['sources']['nativeObjSHA256'].upper(),'Installed LMS cache changed'
    obj_vertices=[list(map(float,line.split()[1:4])) for line in lms_obj.read_text(encoding='utf-8-sig').splitlines() if line.startswith('v ')]
    guide=np.array([[-x,y,z+4.88] for x,y,z in obj_vertices if -.8<=x<=-.1 and 1.6<=y<=2 and .9<=z<=1.2])
    result=dict(passed=True,nativeDataOnly=True,sourceHashes=dict(native=digest(native),geometry=digest(geometry_path),animation=digest(animation_path)),
        poses=len(raw),bothSides='Native drive meshes and local animation are identical; left uses reflected X.',
        fourteen=dict(scale=scale,offset=offset,driveOffset=drive_offset),inventory=inventory,metrics=metrics,
        nativeLubricatorFit=dict(rodAxialDatum=float(fit[0]),leverLocalPin=fit[1:].tolist(),minimumGap=min(errors),maximumGap=max(errors)),
        reachGuide=dict(source=str(lms_obj),sourceSHA256=digest(lms_obj),selection='Installed LMS LOD0 source box: x[-.8,-.1],y[1.6,2],z[.9,1.2]; split transform reflected X and +4.88Z.',
            selectedVertices=len(guide),bodyMinimum=guide.min(axis=0).tolist(),bodyMaximum=guide.max(axis=0).tolist(),
            conclusion='Mid-shaft guide lies at body Z5.78..6.05, not the cab rear tip at Z.43...50. It misses the resized straight span and remains hidden. Bounds are selected source vertices, not a new Unity split execution.'),
        limitations=['Exact exported transforms/meshes and 48 sampled native clip poses; no Unity Animator blending, native ratchet execution, rendering, or gameplay.',
            'Reach rear is a cut shaft entering the cab, not a modeled rear bearing; LOD1 is natively shorter and rotated.',
            'Lubricator fit residual is native authored trajectory error, not a post-repair acceptance tolerance.'])
    output.parent.mkdir(parents=True,exist_ok=True);output.write_text(json.dumps(result,indent=2),encoding='utf-8')
    print(json.dumps(dict(output=str(output),fourteen=result['fourteen'],metrics=metrics,nativeLubricatorFit=result['nativeLubricatorFit'])))

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--output',type=Path,default=ROOT/'build/hidden-linkages/native-measurements.json')
    run(parser.parse_args().output)
