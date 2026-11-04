"""Reuse measured native articulated maps only while their source/input proof matches.

This does not rerun the old clearance sweep or claim new linkage acceptance.
It exports that run's measured front-frame maps for the bounded endpoint fixture.
"""
import argparse
import hashlib
import importlib.util
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()

def reviewed_mallet_sha(data):
    # ID50 only joins ID37's existing driver-description branch. Its measured
    # Mallet map is unchanged; every other source byte remains guarded.
    for newline in (b'\r\n', b'\n'):
        line = b'                case WheelArrangementType.s4664LittleBoy:' + newline
        if line in data:
            if data.count(line) != 1:
                raise ValueError('Unexpected duplicate ID50 Mallet branch')
            data = data.replace(line, b'', 1)
            break
    return hashlib.sha256(data).hexdigest().upper()


def extract(output):
    fixture = ROOT / 'build/franklin-runtime'
    manifest = json.loads((fixture / 'final-evidence-hashes.json').read_text())
    required = {
        'RearrangedS282/WheelRearranger.Mallet.cs',
        'build/franklin-runtime/ProductionWalsConfiguration.cs',
        'build/franklin-runtime/configuration-provenance.json',
        'build/franklin-runtime/geometry.json',
        'build/franklin-runtime/wals-static.json',
        'build/franklin-runtime/final-source-all-720/result.json',
    }
    proof = []
    for item in manifest:
        if item['path'] in required:
            file = ROOT / item['path']
            actual = reviewed_mallet_sha(file.read_bytes()) if item['path'] == 'RearrangedS282/WheelRearranger.Mallet.cs' else sha(file)
            assert actual == item['sha256'], f"Prior native map input changed: {item['path']}"
            proof.append(item)
    assert {p['path'] for p in proof} == required, 'Incomplete prior native input manifest'

    spec = importlib.util.spec_from_file_location('native_map_prepare', ROOT / 'evidence/franklin-runtime/prepare.py')
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    provenance = json.loads((fixture / 'configuration-provenance.json').read_text())
    verified = {}
    for key, expected in provenance.items():
        filename, declaration = key.split(':', 1)
        if filename not in ('WheelRearranger.Mallet.cs', 'WheelRearranger.BigMallet.cs') or not declaration.startswith('private '):
            continue
        body = module.member((ROOT / 'RearrangedS282' / filename).read_text(encoding='utf-8-sig'), declaration)
        actual = hashlib.sha256(body.encode()).hexdigest().upper()
        assert actual == expected, f'Production articulated mapping changed: {key}'
        verified[key] = actual
    # MeasureBigMallet also consumes shared constants rather than only its own body.
    native_configuration = (fixture / 'ProductionWalsConfiguration.cs').read_text()
    for name in ('MalletScale', 'MalletEngineGap', 'MalletTrailingRadius', 'MalletCylinderClearanceGap', 'TriplexOwnTrailingLimit'):
        source = ''.join((ROOT / 'RearrangedS282' / file).read_text(encoding='utf-8-sig') for file in ('WheelRearranger.Mallet.cs', 'WheelRearranger.Triplex.cs'))
        pattern = rf'\b{name}\s*=\s*([^;,\n]+(?:\([^;]*?\))?)'
        # Numeric constants are covered directly; the vector is checked as its full statement.
        if name == 'TriplexOwnTrailingLimit':
            pattern = rf'\b{name}\s*=\s*([^;]+)'
        compact = lambda text: re.sub(r'\s+', '', text)
        current = compact(re.search(pattern, source).group(1))
        prior = compact(re.search(pattern, native_configuration).group(1))
        if name == 'TriplexOwnTrailingLimit':
            vector = lambda text: tuple(float(value.rstrip('f')) for value in text[text.index('(')+1:text.index(')')].split(','))
            assert vector(current) == vector(prior), f'Production map constant changed: {name}'
            verified[name] = current
            continue
        assert current == prior, f'Production map constant changed: {name}'
        verified[name] = current

    result = json.loads((fixture / 'final-source-all-720/result.json').read_text())
    assert result['passed'] and result['clearancePassed'] and result['unexpectedContacts'] == 0
    layouts = []
    for item in result['layouts']:
        if item['id'] not in (37, 41, 42, 43, 45, 46):
            continue
        scale, front, rear, pin, front_count, rear_count, cylinder, trailing = item['layoutDatums']
        layouts.append(dict(id=item['id'], scale=scale, driveOffset=front, rearOffset=rear, pinZ=pin,
                            frontCoupled=int(front_count), rearCoupled=int(rear_count), cylinderFront=cylinder,
                            trailingZ=trailing, yaws=[-15, 0, 15]))
    assert len(layouts) == 6
    document = dict(layouts=layouts, provenance=dict(source='build/franklin-runtime/final-source-all-720/result.json',
        retainedEvidence=True, inputs=proof, currentSourceMatches=verified,
        limits='Measured native front engine maps only. Prior clearance passes are not hidden linkage acceptance. '
               'Apply yaw about body-frame (0,0,pinZ), with T(pin)*R(yaw)*T(-pin)*T(frontOffset)*S(side,scale,scale). '
               'Rear/tender have no demonstrated cab/oiler attachment and remain excluded.'))
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(document, indent=2))
    print(json.dumps(dict(output=str(output), measuredArticulatedMaps=len(layouts), currentSourceVerified=True)))


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', type=Path, default=ROOT / 'build/hidden-linkages/articulated-layouts.json')
    extract(parser.parse_args().output)
