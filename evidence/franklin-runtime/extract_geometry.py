"""Private native S282 mechanism data for a real-Unity rendering probe.

No game or mod file is changed. Generated native data belongs under ignored build/.
The full serialized drive transform hierarchy is retained, not a synthetic wheel.
"""
from __future__ import annotations
import argparse
import hashlib
import json
import math
import struct
from pathlib import Path
import UnityPy
from UnityPy.helpers.MeshHelper import MeshHandler


def digest(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest().upper()


def multiply(a, b):
    return [[sum(a[r][k]*b[k][c] for k in range(4)) for c in range(4)] for r in range(4)]


def inverse(a):
    rows = [list(row)+[float(r == c) for c in range(4)] for r,row in enumerate(a)]
    for c in range(4):
        pivot = max(range(c,4), key=lambda r:abs(rows[r][c]))
        if abs(rows[pivot][c]) < 1e-12:
            raise ValueError('Singular native body/support transform')
        rows[c],rows[pivot] = rows[pivot],rows[c]
        factor = rows[c][c]
        rows[c] = [value/factor for value in rows[c]]
        for r in range(4):
            if r != c:
                factor = rows[r][c]
                rows[r] = [value-factor*source for value,source in zip(rows[r],rows[c])]
    return [row[4:] for row in rows]


def local_matrix(t):
    x,y,z,w = t.m_LocalRotation.x,t.m_LocalRotation.y,t.m_LocalRotation.z,t.m_LocalRotation.w
    sx,sy,sz = t.m_LocalScale.x,t.m_LocalScale.y,t.m_LocalScale.z
    p = t.m_LocalPosition
    result = [[(1-2*y*y-2*z*z)*sx,(2*x*y-2*z*w)*sy,(2*x*z+2*y*w)*sz,p.x],
              [(2*x*y+2*z*w)*sx,(1-2*x*x-2*z*z)*sy,(2*y*z-2*x*w)*sz,p.y],
              [(2*x*z-2*y*w)*sx,(2*y*z+2*x*w)*sy,(1-2*x*x-2*y*y)*sz,p.z],
              [0.,0.,0.,1.]]
    if not all(math.isfinite(value) for row in result for value in row):
        raise ValueError('Non-finite native transform')
    return result


def extract(game, bundle, output):
    native = game / 'DerailValley_Data/resources.assets'
    env = UnityPy.load(str(native))
    meshes = {}
    paths = {}
    world_matrices = {}
    def path(t):
        key = t.object_reader.path_id
        if key not in paths:
            name = t.m_GameObject.deref().read().m_Name
            paths[key] = (path(t.m_Father.deref().read()) + '/' if t.m_Father.m_PathID else '') + name
        return paths[key]

    def world_matrix(t):
        key = t.object_reader.path_id
        if key not in world_matrices:
            local = local_matrix(t)
            world_matrices[key] = multiply(world_matrix(t.m_Father.deref().read()),local) if t.m_Father.m_PathID else local
        return world_matrices[key]

    def mesh_data(reader, bank):
        key = str(reader.path_id)
        if key not in bank:
            m = reader.read()
            handler = MeshHandler(m); handler.process()
            bank[key] = dict(name=m.m_Name, vertices=[[float(n) for n in v[:3]] for v in handler.m_Vertices],
                triangles=[int(i) for group in handler.get_triangles() for tri in group for i in tri])
        return key

    def node(t):
        go = t.m_GameObject.deref().read()
        item = dict(name=go.m_Name, active=bool(go.m_IsActive),
            position=[t.m_LocalPosition.x,t.m_LocalPosition.y,t.m_LocalPosition.z],
            rotation=[t.m_LocalRotation.x,t.m_LocalRotation.y,t.m_LocalRotation.z,t.m_LocalRotation.w],
            scale=[t.m_LocalScale.x,t.m_LocalScale.y,t.m_LocalScale.z], children=[])
        for pair in go.m_Component:
            reader = pair.component.deref()
            if reader.type.name == 'MeshFilter':
                m = reader.read().m_Mesh
                if m.m_PathID: item['mesh'] = mesh_data(m.deref(), meshes)
            elif reader.type.name == 'MeshRenderer':
                item['renderer'] = bool(reader.read().m_Enabled)
            elif reader.type.name == 'MonoBehaviour':
                # The native script has no UnityPy MonoBehaviour type tree.
                # Its verified serialized schema is Mono header (32 bytes),
                # updateType int32, aligned portId/parameterName strings, then
                # multiplier/offset float32. Keep raw hash and require exact
                # bounds rather than guessing values from our fixture.
                raw=reader.get_raw_data()
                try:
                    cursor=32
                    update_type=struct.unpack_from('<i',raw,cursor)[0];cursor+=4
                    strings=[]
                    for _ in range(2):
                        length=struct.unpack_from('<i',raw,cursor)[0];cursor+=4
                        if not 0<=length<=len(raw)-cursor:raise ValueError('Not a reader string')
                        strings.append(raw[cursor:cursor+length].decode('utf-8'))
                        cursor=(cursor+length+3)&~3
                    if cursor+8!=len(raw) or strings[1] not in ('Cutoff','SpeedMultiplier'):continue
                    multiplier,offset=struct.unpack_from('<ff',raw,cursor)
                    item.setdefault('readers',[]).append(dict(pathID=reader.path_id,
                        updateType=update_type,portId=strings[0],parameterName=strings[1],
                        valueMultiplier=multiplier,valueOffset=offset,
                        rawSHA256=hashlib.sha256(raw).hexdigest().upper(),rawBytes=len(raw),
                        schema='verified native AnimatorPortReader: 32-byte Mono header, enum/string/string/float/float'))
                except (struct.error,UnicodeDecodeError,ValueError):
                    pass
        item['children'] = [node(c.deref().read()) for c in t.m_Children]
        return item

    roots = {}
    datum_transforms = {}
    datum_paths = ('LocoS282A', 'LocoS282A/Axle_F', 'LocoS282A/LocoS282A_Body/MovingParts_LOD0')
    for obj in env.objects:
        if obj.type.name != 'Transform': continue
        t = obj.read()
        name = t.m_GameObject.deref().read().m_Name
        if name in ('LocoS282A','Axle_F','MovingParts_LOD0'):
            full = path(t)
            if full in datum_paths:
                if full in datum_transforms: raise ValueError('Ambiguous native support datum: ' + full)
                datum_transforms[full] = t
        if name not in ('LocoS282A_Drivetrain L','LocoS282A_Drivetrain R'): continue
        full = path(t)
        if full.startswith('LocoS282A/'):
            if name in roots: raise ValueError('Ambiguous native drive ' + name)
            roots[name] = node(t)
    if len(roots) != 2: raise ValueError('Both exact native S282 drives were not found')
    if set(datum_transforms) != set(datum_paths):
        raise ValueError('Exact native S282 Axle_F/car/body transforms were not found')
    car,support,body = (datum_transforms[p] for p in datum_paths)
    body_from_world = inverse(world_matrix(body))
    body_from_support = multiply(body_from_world,world_matrix(support))
    body_from_car = multiply(body_from_world,world_matrix(car))
    if any(abs(body_from_car[r][c]-float(r == c)) > 1e-6 for r in range(4) for c in range(4)):
        raise ValueError('Native car/body frames no longer coincide; the mechanism constraint fixture must retain that transform')
    native_hash = digest(native)
    support_datum = dict(positionBody=[body_from_support[r][3] for r in range(3)],
        bodyFromSupport=body_from_support, bodyFromCar=body_from_car,
        supportPath=datum_paths[1],supportTransformPathID=support.object_reader.path_id,
        bodyPath=datum_paths[2],bodyTransformPathID=body.object_reader.path_id,
        nativeResourceSHA256=native_hash,
        method='Exact serialized native hierarchy: inverse(bodyWorld) * supportWorld; car/body identity required')
    bundle_meshes = {}
    bundle_env = UnityPy.load(str(bundle))
    for obj in bundle_env.objects:
        if obj.type.name == 'Mesh': mesh_data(obj,bundle_meshes)
    bindings = {}
    def bundle_bindings(t, parent=''):
        go=t.m_GameObject.deref().read()
        rel=(parent+'/' if parent else '')+go.m_Name
        for pair in go.m_Component:
            reader=pair.component.deref()
            if reader.type.name == 'MeshFilter':
                ptr=reader.read().m_Mesh
                if ptr.m_PathID: bindings[rel]=mesh_data(ptr.deref(),bundle_meshes)
        for child in t.m_Children: bundle_bindings(child.deref().read(),rel)
    for obj in bundle_env.objects:
        if obj.type.name != 'Transform': continue
        t=obj.read()
        if not t.m_Father.m_PathID: bundle_bindings(t)
    result = dict(nativeSource=str(native), nativeSHA256=native_hash, bundle=str(bundle),
        bundleSHA256=digest(bundle), drives=roots, nativeMeshes=meshes, bundleMeshes=bundle_meshes,bundleBindings=bindings,
        triplexSupportDatum=support_datum,
        nativeDataOnly=True, limitations='Serialized native geometry; runtime scripts/materials are excluded. Original Franklin controllers/prefabs are loaded from the bundle by Unity.')
    output.parent.mkdir(parents=True,exist_ok=True)
    output.write_text(json.dumps(result,separators=(',',':')),encoding='utf-8')
    print(json.dumps(dict(output=str(output),nativeMeshes=len(meshes),bundleMeshes=len(bundle_meshes),drives=list(roots))))


if __name__ == '__main__':
    p=argparse.ArgumentParser()
    p.add_argument('--game',type=Path,default=Path(r'B:\SteamLibrary\steamapps\common\Derail Valley'))
    p.add_argument('--bundle',type=Path,default=Path(__file__).resolve().parents[2]/'s282_drivetrain/ExportedProject/Assets/AssetBundles/s282_drivetrain')
    p.add_argument('--output',type=Path,default=Path(__file__).resolve().parents[2]/'build/franklin-runtime/geometry.json')
    a=p.parse_args();extract(a.game,a.bundle,a.output)
