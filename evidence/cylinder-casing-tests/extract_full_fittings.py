"""Actual native LMS dry-pipe triangles for local geometry tests only."""
import json
from pathlib import Path
from extract_native import extract_lod
folder=Path(__file__).parent
assets=Path(r'B:\SteamLibrary\steamapps\common\Derail Valley\Mods\LocoMeshSplitter\assets')
parts=[]
for lod in (0,1):
    source=assets/('s282_locomotive_body'+('_LOD1' if lod else '')+'.obj')
    result=extract_lod(source,folder/f'LMS_LOD{lod}.reference.txt',lod,include_pipes=True,include_guides=True)
    parts.extend(p for p in result['parts'] if '-pipe-' in p['name'] or '-guide-' in p['name'])
output=folder/'native-full-casing-fittings.json'
output.write_text(json.dumps(dict(note='Local native geometry only, not a runtime/source asset',parts=parts)))
print('Extracted actual native triangles for',len(parts),'pipe/guide meshes')
