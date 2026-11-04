"""Bounded source-derived endpoint fixture frames; no production/game mutation.

Reads current setters rather than copying another arrangement's coordinates.
Only the root/oiler projection is evaluated; full source member receipts expose
the exact implementation this narrower projection represents.
"""
import hashlib
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'build/hidden-linkages/layout-provenance.json'


def sha(data):
    return hashlib.sha256(data).hexdigest().upper()


def member(text, marker):
    start = text.index(marker)
    opening = text.index('{', start)
    depth = 1
    end = opening + 1
    while depth:
        depth += (text[end] == '{') - (text[end] == '}')
        end += 1
    return text[start:end]


def number(expression):
    expression = re.sub(r'(?<=[0-9])f\b', '', expression.strip())
    assert re.fullmatch(r'[-+0-9.eE ]+', expression), expression
    return float(expression)


def vector(text, name):
    match = re.search(re.escape(name) + r'\s*=\s*new Vector3\(([^;]+)\);', text)
    assert match, name
    return [number(v) for v in match[1].split(',')]


def constant(text, name):
    match = re.search(r'\b' + re.escape(name) + r'\s*=\s*([-+.0-9]+f?)', text)
    assert match, name
    return number(match[1])


def main():
    files = {name: (ROOT / 'RearrangedS282' / name).read_text(encoding='utf-8-sig') for name in [
        'WheelRearranger.cs', 'WheelArrangement.cs', 'WheelRearranger.Conventional.cs',
        'WheelRearranger.LargeDriverClearance.cs', 'WheelRearranger.PrairieSpacing.cs',
        'WheelRearranger.ConnectingRodSpacing.cs', 'WheelRearranger.FourteenCoupled.cs',
        'WheelRearranger.Mallet.cs', 'WheelRearranger.Triplex.cs']}
    src = files['WheelRearranger.cs']
    # Derive enum values from current source, including explicitly assigned IDs.
    enum = member(files['WheelArrangement.cs'], 'public enum WheelArrangementType')
    enum = re.sub(r'/\*.*?\*/|//[^\n]*', '', enum, flags=re.S)
    ids = {}; value = 0
    for name, explicit in re.findall(r'\b(s\d+\w*)\s*(?:=\s*(\d+))?\s*[,\n]', enum):
        value = int(explicit) if explicit else value
        ids[name] = value
        value += 1
    legacy = member(src, 'private void ApplyLegacyGeometry(int wa)')
    methods = {}; receipts = []; calls = {}; pending = []
    for line in legacy.splitlines():
        case = re.search(r'case WheelArrangementType\.(\w+):', line)
        if case:
            pending.append(case[1])
        call = re.search(r'\b(Show4Drivers|Show6Drivers|Show8BigDrivers|Show12SmallDrivers)\(\);', line)
        if call:
            calls[call[1]] = list(pending)
        if 'break;' in line:
            pending.clear()
    for name in ['Show4Drivers', 'Show6Drivers', 'Show8BigDrivers', 'Show12SmallDrivers']:
        body = member(src, 'private void ' + name + '()')
        names = calls[name]
        methods[name] = dict(ids=[ids[n] for n in names], position=vector(body, 'driveR.localPosition'),
                             scale=vector(body, 'driveR.localScale')[2], oiler=vector(body, 'lubricator.localPosition'))
        receipts.append(dict(path='RearrangedS282/WheelRearranger.cs', member=name, sha256=sha(body.encode()),
                             projectedAssignments=re.findall(r'(?:drive[LR]|lubricator)\.local(?:Position|Scale)\s*=.*?;', body)))
    clearance = files['WheelRearranger.LargeDriverClearance.cs']
    # Fail if the implementation no longer has the exact affine operations
    # projected below. Member/file hashes alone would not detect stale algebra.
    required = {
        'WheelRearranger.LargeDriverClearance.cs': [
            'drive.localPosition += Vector3.forward * ((oldScale - ReservoirClearDriverScale) * anchor);',
            'drive.localScale = new Vector3(drive.localScale.x, drive.localScale.y * ratio, ReservoirClearDriverScale);',
            'float anchor = (prairie ? secondRightDriveWheel : firstRightDriveWheel).localPosition.z;'],
        'WheelRearranger.PrairieSpacing.cs': ['drive.localPosition += Vector3.forward * PrairieDriverForward;'],
        'WheelRearranger.ConnectingRodSpacing.cs': [
            'var delta = -transform.TransformDirection(Vector3.forward).normalized * amount;', 'drive.position += delta;'],
        'WheelRearranger.FourteenCoupled.cs': [
            'layout.DriverZ[6] = native[5] - layout.Pitch;',
            'float s = (cylinderFront - trailingLimit - FourteenTruckSpacing - MalletTrailingRadius - MalletEngineGap)',
            '/ (cylinderFront - layout.DriverZ[6] + radius);',
            'layout.Offset = cylinderFront * (1f - s);', 'layout.DriveOffset = layout.Offset - FourteenDriverShift;']}
    for file, fragments in required.items():
        for fragment in fragments:
            assert fragment in files[file], 'Projection needs review: ' + file + ': ' + fragment
    chain = files['WheelRearranger.Conventional.cs']
    assert chain.index('ApplyLegacyGeometry(id);') < chain.index('ApplyLargeDriverClearance(id,') < chain.index('ApplyPrairieForwardSpacing(false);') < chain.index('ApplyConnectingRodSpacing(id,')
    scale_limit = constant(clearance, 'ReservoirClearDriverScale')
    clearance_ids = [ids[n] for n in re.findall(r'id == \(int\)WheelArrangementType\.(\w+)', clearance.split('private void ApplyLargeDriverClearance')[0])]
    rearward_source = member(files['WheelRearranger.ConnectingRodSpacing.cs'], 'internal static float RequestedDriverRearwardM(int id)')
    rearward = {ids[n]: number(v) for n, v in re.findall(r'case WheelArrangementType\.(\w+): return ([^;]+);', rearward_source)}
    reset = member(src, 'private void ResetToFourCoupled()')
    first_z = vector(reset, 'firstRightDriveWheel.localPosition')[2]
    second_z = vector(reset, 'secondRightDriveWheel.localPosition')[2]
    rows = []
    for method, data in methods.items():
        for id_ in data['ids']:
            z = data['position'][2]; scale = data['scale']; steps = [method]
            if id_ in clearance_ids:
                z += (scale-scale_limit) * (second_z if id_ == ids['s262'] else first_z)
                scale = scale_limit
                steps.append('ApplyLargeDriverClearance')
            if id_ == ids['s262']:
                z += constant(files['WheelRearranger.PrairieSpacing.cs'], 'PrairieDriverForward')
                steps.append('ApplyPrairieForwardSpacing')
            if id_ in rearward:
                z -= rearward[id_]
                steps.append('ApplyConnectingRodSpacing')
            rows.append(dict(id=id_, scale=scale, driveOffset=z, oilerOffset=data['oiler'], methods=steps))
    # 4-14-4 uses native geometry and its own current fit formula.
    spacing_path = ROOT/'evidence/fourteen-spacing-tests/native-spacing.json'
    spacing = json.loads(spacing_path.read_text(encoding='utf-8-sig'))
    fourteen = files['WheelRearranger.FourteenCoupled.cs']; mallet = files['WheelRearranger.Mallet.cs']
    fifth = vector(src, 'fifthLeftDriveWheel.localPosition')[2]
    sixth = vector(src, 'sixthLeftDriveWheel.localPosition')[2]
    seventh = sixth - (fifth-sixth)
    geometry_path = ROOT/'build/franklin-runtime/geometry.json'
    geometry = json.loads(geometry_path.read_text(encoding='utf-8-sig'))
    native_fourth = next(n for n in geometry['drives']['LocoS282A_Drivetrain R']['children'] if n['name']=='s282_wheels_driving_4')
    radius = native_fourth['position'][1]  # Measure4144 precedes ResetToFourCoupled.
    trailing = spacing['rearBogiePosition'][2] + vector(files['WheelRearranger.Triplex.cs'], 'TriplexTrailingLimit')[2]
    front = spacing['cylinderFront']
    scale = (front-trailing-constant(fourteen,'FourteenTruckSpacing')-constant(mallet,'MalletTrailingRadius')-constant(mallet,'MalletEngineGap'))/(front-seventh+radius)
    rows.append(dict(id=ids['s4144'], scale=scale, driveOffset=front*(1-scale)-constant(fourteen,'FourteenDriverShift'),
                     oilerOffset=[0,0,0], methods=['Measure4144','Apply4144Geometry']))
    assert len(rows) == 18 and len({r['id'] for r in rows}) == 18 and ids['s222'] not in [r['id'] for r in rows]
    report = dict(layouts=sorted(rows,key=lambda r:r['id']), projection='Drive-root affine frame and oiler translation only; native clips supply child poses.',
                  sourceFiles={n:sha((ROOT/'RearrangedS282'/n).read_bytes()) for n in files}, projectedMembers=receipts,
                  checkedAffineSourceFragments=required,
                  nativeSpacingSHA256=sha(spacing_path.read_bytes()), nativeResourceSHA256=spacing['sources']['resourcesSHA256'],
                  nativeGeometrySHA256=sha(geometry_path.read_bytes()),fourteenPreparedNativeRadius=radius,
                  limits='Source-derived numerical projection, not execution of complete arrangement setters or actual gameplay. ID39 uses measured native fit inputs; others use their own setters plus production clearance/spacing order.')
    OUT.parent.mkdir(parents=True,exist_ok=True)
    OUT.write_text(json.dumps(report,indent=2),encoding='utf-8')
    print(json.dumps(dict(output=str(OUT),layouts=report['layouts'])))


if __name__ == '__main__':
    main()
