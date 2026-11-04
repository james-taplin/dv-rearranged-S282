"""Prove the ID37 native map applies before replaying ID50 hidden linkages."""
import argparse
import hashlib
import importlib.util
import json
import re
import subprocess
from pathlib import Path

root = Path(__file__).resolve().parents[2]
parser = argparse.ArgumentParser()
parser.add_argument('--output', required=True, type=Path)
parser.add_argument('--geometry-receipt', required=True, type=Path)
args = parser.parse_args()
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest().upper()
mallet = root/'RearrangedS282/WheelRearranger.Mallet.cs'
baseline = subprocess.check_output(['git','-c',f'safe.directory={root.as_posix()}','show',
    '46a9a72:RearrangedS282/WheelRearranger.Mallet.cs'], cwd=root).decode('utf-8-sig')
current = mallet.read_text(encoding='utf-8-sig').replace('case WheelArrangementType.s4664LittleBoy:', '')
if ''.join(current.split()) != ''.join(baseline.split()):
    raise ValueError('Mallet source changed beyond the reviewed ID50 group-description case')
wrapper = root/'RearrangedS282/WheelRearranger.LittleBoy.cs'
if sha(wrapper) != '7D175752BE27FF4B030C397B0E70251A9A38CC9731C6E1EA13D6566FAC4AC5F0':
    raise ValueError('ID50 wrapper changed; re-establish that the native drivetrain map is unchanged')
dispatch = (root/'RearrangedS282/WheelRearranger.Conventional.cs').read_text(encoding='utf-8-sig')
if not re.search(r'else\s+if\s*\(id\s*==\s*\(int\)WheelArrangementType\.s4664LittleBoy\)\s*ApplyLittleBoyGeometry\(\);',dispatch):
    raise ValueError('ID50 no longer routes to the reviewed wrapper')
geometry = json.loads(args.geometry_receipt.read_text(encoding='utf-8-sig'))
if not geometry['passed'] or geometry['arrangementId'] != 50 or geometry['provenance']['sourceSHA256']['littleBoy'] != sha(wrapper):
    raise ValueError('The ID50 dispatcher/geometry receipt is missing, failed or stale')
old = json.loads((root/'build/hidden-linkages/articulated-layouts.json').read_text(encoding='utf-8-sig'))
for item in old['provenance']['inputs']:
    if item['path'] != 'RearrangedS282/WheelRearranger.Mallet.cs' and sha(root/item['path']) != item['sha256']:
        raise ValueError('Retained native map input changed: '+item['path'])
spec = importlib.util.spec_from_file_location('franklin_prepare', root/'evidence/franklin-runtime/prepare.py')
prepare = importlib.util.module_from_spec(spec)
spec.loader.exec_module(prepare)
for key, expected in old['provenance']['currentSourceMatches'].items():
    if key.startswith('WheelRearranger.Mallet.cs:private '):
        actual = prepare.member(current, key.split(':',1)[1])
        if hashlib.sha256(actual.encode()).hexdigest().upper() != expected:
            raise ValueError('Native mechanism source changed: '+key)
    elif key in ('MalletScale','MalletEngineGap','MalletTrailingRadius','MalletCylinderClearanceGap'):
        value = re.search(r'\b'+key+r'\s*=\s*([^,;\n]+)',current)
        if not value or ''.join(value.group(1).split()) != expected:
            raise ValueError('Native mechanism constant changed: '+key)
saved = next(item for item in old['layouts'] if item['id']==37)
result = json.loads((root/'build/franklin-runtime/final-source-all-720/result.json').read_text())
old_run = next(item for item in result['layouts'] if item['id']==37)
if not result['passed'] or not old_run['clearancePassed'] or old_run['unexpectedContacts'] != 0:
    raise ValueError('Historical ID37 full-phase mechanism receipt did not pass')
if abs(saved['trailingZ']-geometry['trail1'])>1e-4 or abs(.712*saved['scale']-geometry['driverRadiusM'])>1e-5:
    raise ValueError('Current ID50 measurement does not match retained ID37 map')
saved = dict(saved, id=50)
args.output.parent.mkdir(parents=True,exist_ok=True)
args.output.write_text(json.dumps(dict(layouts=[saved],proof=dict(originalId=37, targetId=50,
    previousNativeResultSHA256=sha(root/'build/franklin-runtime/final-source-all-720/result.json'),
    geometryReceiptSHA256=sha(args.geometry_receipt), wrapperSHA256=sha(wrapper),
    limitation='Reuses ID37 native mechanism map: ID50 wrapper only alters carrying axles/supports after the same Mallet setup. Managed sampled replay, not full ID50 TrainCar or live track/Unity integration.')),indent=2))
