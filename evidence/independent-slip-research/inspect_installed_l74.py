"""Read installed L74 bundle type trees; write only graph metadata, never assets.
Run with the workspace Python and its existing UnityPy site-packages.
"""
import argparse, hashlib, json, sys
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('--bundle', required=True)
parser.add_argument('--output', required=True)
parser.add_argument('--unitypy-site', required=True)
args = parser.parse_args()
sys.path.append(args.unitypy_site)
import UnityPy

source = Path(args.bundle)
env = UnityPy.load(str(source))
objects = {o.path_id: o for o in env.objects}
def tree(path_id):
    return objects[path_id].read_typetree()
def script_name(t):
    script = t.get('m_Script', {})
    return tree(script['m_PathID']).get('m_ClassName', '') if script.get('m_PathID') in objects else None

graphs, feeders, slip_definitions = [], [], []
for obj in env.objects:
    if obj.type.name != 'MonoBehaviour':
        continue
    t = obj.read_typetree()
    if 'maxWheelslipRpm' in t:
        curve = t.get('wheelslipToAdhesionDrop', {}).get('m_Curve', [])
        slip_definitions.append({'pathId': obj.path_id, 'class': script_name(t), 'maxWheelslipRpm':t['maxWheelslipRpm'], 'preventWheelslip':t.get('preventWheelslip'), 'curve':[{'time':k.get('time'),'value':k.get('value')} for k in curve]})
    if 'wheelRpmPortId' in t:
        feeders.append({'pathId': obj.path_id, 'class': script_name(t),
            'rpmPort': t['wheelRpmPortId']})
    if 'executionOrder' not in t or 'portReferenceConnections' not in t:
        continue
    order = []
    for reference in t['executionOrder']:
        definition = tree(reference['m_PathID'])
        order.append({'id': definition.get('ID'), 'class': script_name(definition)})
    graphs.append({'pathId': obj.path_id, 'executionOrder': order,
        'connections': t.get('connections', []),
        'crankReferences': [x for x in t['portReferenceConnections'] if x['portReferenceId'].endswith('.CRANK_RPM')],
        'torqueSumReferences': [x for x in t['portReferenceConnections'] if x['portReferenceId'].startswith('l74SimplingTorque.')],
        'engineIntakeReferences': [x for x in t['portReferenceConnections'] if x['portReferenceId'].startswith('steamConsumptionCalculator.') and x['portId'].endswith('.INTAKE_FLOW')]})
report = {'source': str(source), 'sha256': hashlib.sha256(source.read_bytes()).hexdigest(),
    'method': 'Direct UnityPy read of installed binary type trees; no Unity runtime execution',
    'graphs': graphs, 'tractionFeeders': feeders, 'wheelslipDefinitions': slip_definitions}
Path(args.output).write_text(json.dumps(report, indent=2), encoding='utf-8')
for g in graphs:
    print(json.dumps({'definitions': len(g['executionOrder']), 'tractionDefinitions': [d for d in g['executionOrder'] if d['class'] and 'Traction' in d['class']], 'connections':g['connections'],'crankReferences':g['crankReferences']}))
print('SHA256 ' + report['sha256'])
