"""Private native drivetrain payload, no game writes or redistributed assets.

Retain actual hierarchy, meshes, Animator and controller/clip bytes. Remove
native service behaviours and material references from this isolated payload;
the probes bind the actual production/native readers explicitly. Make exact
mesh vertex buffers CPU-readable for collision/pose measurement only.
"""
import argparse, copy, hashlib, json, sys
from pathlib import Path

p = argparse.ArgumentParser()
p.add_argument('--game-data', type=Path, required=True)
p.add_argument('--player-data', type=Path, required=True)
p.add_argument('--unitypy-site', required=True)
a = p.parse_args()
if a.player_data.resolve() == a.game_data.resolve() or not (a.player_data / 'Managed' / 'S282.NativeSlipBootstrap.dll').is_file():
    raise SystemExit('Target must be a private native-slip fixture player copy, never the game data directory')
sys.path.append(a.unitypy_site)
import UnityPy

source = a.game_data / 'resources.assets'
env = UnityPy.load(str(source))
native = list(env.files.values())[0]
root_id = next(o.path_id for o in env.objects if o.type.name == 'GameObject' and o.read().m_Name == 'LocoS282A_Drivetrain R')
root_tree = native.objects[root_id].read_typetree()
root_transform = next(c['component']['m_PathID'] for c in root_tree['m_Component']
                      if native.objects[c['component']['m_PathID']].type.name == 'Transform')
kept, stripped, stream = {}, [], bytearray()

def pointers(v):
    if isinstance(v, dict):
        if 'm_FileID' in v and 'm_PathID' in v:
            if v['m_FileID'] == 0 and v['m_PathID']: yield v['m_PathID']
        else:
            for item in v.values(): yield from pointers(item)
    elif isinstance(v, (list, tuple)):
        for item in v: yield from pointers(item)

def retain(ident):
    if ident in kept: return
    obj = native.objects[ident]
    tree = obj.read_typetree()
    kept[ident] = obj
    if obj.type.name == 'GameObject':
        removed = [c for c in tree['m_Component'] if native.objects[c['component']['m_PathID']].type.name == 'MonoBehaviour']
        stripped.extend(c['component']['m_PathID'] for c in removed)
        tree['m_Component'] = [c for c in tree['m_Component'] if c not in removed]
    if ident == root_transform: tree['m_Father'] = {'m_FileID': 0, 'm_PathID': 0}
    if obj.type.name in ('MeshRenderer', 'SkinnedMeshRenderer'): tree['m_Materials'] = []
    if obj.type.name == 'Mesh':
        tree['m_IsReadable'] = True
        data = tree.get('m_StreamData', {})
        if data.get('size', 0):
            with (a.game_data / data['path']).open('rb') as f:
                f.seek(data['offset']); payload = f.read(data['size'])
            if len(payload) != data['size']: raise RuntimeError('Incomplete mesh buffer')
            data['offset'] = len(stream); data['path'] = 'native-drive.assets.resS'
            stream.extend(payload)
    obj.save_typetree(tree)
    for child in pointers(tree): retain(child)

retain(root_id)
native.objects = kept
(a.player_data / 'native-drive.assets').write_bytes(native.save())
(a.player_data / 'native-drive.assets.resS').write_bytes(stream)

# Add a resource entry to the copied fixture's manager, retaining its scene and
# synthetic resources. External indices elsewhere in that player do not change.
ggm_path = a.player_data / 'globalgamemanagers'
ggm_env = UnityPy.load(str(ggm_path)); ggm = list(ggm_env.files.values())[0]
external = copy.copy(ggm.externals[-1])
external.path = 'native-drive.assets'
external.guid = bytes(16)
external.type = 0
ggm.externals.append(external)
manager = next(o for o in ggm_env.objects if o.type.name == 'ResourceManager')
tree = manager.read_typetree()
tree['m_Container'].append(('natives282drivetrainr', {'m_FileID': len(ggm.externals), 'm_PathID': root_id}))
manager.save_typetree(tree)
ggm_path.write_bytes(ggm.save())
print(json.dumps({'source': str(source), 'sourceSHA256': hashlib.sha256(source.read_bytes()).hexdigest(),
                  'rootPathId': root_id, 'objects': len(kept), 'meshBufferBytes': len(stream),
                  'removedServiceBehaviours': len(stripped),
                  'payloadSHA256': hashlib.sha256((a.player_data/'native-drive.assets').read_bytes()).hexdigest()}))
