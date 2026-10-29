"""Stage the full Hotfix 2 package; do not install or overwrite earlier releases."""
from pathlib import Path
import hashlib
import json
import shutil
import zipfile

root = Path(__file__).resolve().parents[2]
work = root / 'build/quartering-hotfix'
version = '2.0.2-beta'
baseline = root / 'release/2.0.1-beta/RearrangedS282-B99-2.0.1-beta-Oiling-Hotfix.zip'
public = root / 'release/2.0-beta/RearrangedS282-B99-2.0-beta.zip'
installed = Path('B:/SteamLibrary/steamapps/common/Derail Valley/Mods/RearrangedS282')
destination = root / 'release' / version
target = destination / f'RearrangedS282-B99-{version}-Quartering-Hotfix.zip'
desktop = Path('C:/Users/james/Desktop/Derail Valley Mods') / f'RearrangedS282-B99-{version}-Quartering-Hotfix'
copy = root / 'release-copy' / version

def sha(data):
    return hashlib.sha256(data).hexdigest().upper()

assert sha(baseline.read_bytes()) == '9115CE635B2B31B1D95D643F7B71B29F3627A5901619692CDB91A17E7CBD2EC3'
assert sha(public.read_bytes()) == '48A4C061CE75423B6DE0760AE76DC652521940B93520111856E585580E4760C0'
assert sha((installed / 'RearrangedS282.dll').read_bytes()) == '4C14D0AAC5DE70955CEFE0B2148F936211F941A258EC2EB1CBE1D516308EC9FE'
installed_before = {str(p.relative_to(installed)): sha(p.read_bytes()) for p in installed.rglob('*') if p.is_file()}
proof = json.loads((work / 'build-validation.json').read_text(encoding='utf-8-sig'))
tests = json.loads((work / 'tests/validation.json').read_text(encoding='utf-8-sig'))
matrix = json.loads((work / 'structural-matrix/validation.json').read_text(encoding='utf-8-sig'))
followers = json.loads((work / 'rear-followers/validation.json').read_text(encoding='utf-8-sig'))
tender = json.loads((work / 'tender-integration/validation.json').read_text(encoding='utf-8-sig'))
express = json.loads((work / 'express/validation.json').read_text(encoding='utf-8-sig'))
pins = json.loads((work / 'crank-pins/validation.json').read_text(encoding='utf-8-sig'))
assert all(p['passed'] for p in (proof, tests, matrix, followers, tender, express, pins))
assert pins['oldVisibilityGapReproduced'] is True
assert tests['arrangementTenderCases'] == 93 and tests['measuredEnginePairs'] == 159
dll = (work / 'bin/RearrangedS282.dll').read_bytes()
assert sha(dll) == proof['dllSHA256'] and proof['version'] == version

with zipfile.ZipFile(baseline) as source:
    names = source.namelist()
    assert len(names) == 36 and len(set(names)) == 36
    assert all(n.startswith('RearrangedS282/') and '\\' not in n and '..' not in Path(n).parts for n in names)
    info = json.loads(source.read('RearrangedS282/Info.json').decode('utf-8-sig'))
    info['Version'] = version
    readme = (copy / 'README.txt').read_text(encoding='ascii')
    changelog = (copy / 'CHANGELOG.txt').read_text(encoding='utf-8-sig')
    readme = readme.replace('\r\n', '\n').replace('\n', '\r\n')
    changelog = changelog.replace('\r\n', '\n').replace('\n', '\r\n')
    assert version in readme.splitlines()[0] and version in changelog.splitlines()[0]
    assert '49 selectable arrangements' in readme and '44 compatible arrangements' in readme
    assert '4-8-2 Express' in readme and '4-8-2 Mountain remains' in readme
    assert all(marker not in readme for marker in ('**', '```', '[b]', '[url', '|---'))
    updates = {'RearrangedS282/RearrangedS282.dll': dll,
               'RearrangedS282/Info.json': (json.dumps(info, indent=2) + '\n').encode('utf-8'),
               'RearrangedS282/README.txt': readme.encode('ascii'),
               'RearrangedS282/CHANGELOG.txt': changelog.encode('utf-8'),
               'RearrangedS282/UMM_OPTIONS.md': (copy / 'UMM_OPTIONS.md').read_bytes(),
               'RearrangedS282/CREDITS.txt': (copy / 'CREDITS.txt').read_bytes()}
    destination.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(target, 'w', compression=zipfile.ZIP_DEFLATED, compresslevel=6) as out:
        for member in source.infolist():
            if member.filename in updates:
                out.writestr(member.filename, updates[member.filename])
            else:
                with source.open(member) as src, out.open(member.filename, 'w') as dst:
                    shutil.copyfileobj(src, dst, 1024 * 1024)

with zipfile.ZipFile(baseline) as old, zipfile.ZipFile(target) as new:
    assert new.testzip() is None and new.namelist() == names
    changed = [n for n in names if old.read(n) != new.read(n)]
    assert set(changed) == {name for name, data in updates.items() if old.read(name) != data}
    assert all(new.read(name) == data for name, data in updates.items())
    assert not any(n.lower().endswith(('.pdb', '.log', '.csv', 'settings.xml')) for n in names)
    assert json.loads(new.read('RearrangedS282/Info.json'))['Version'] == version

status = (
    'Rearranged S282 2.0.2-beta - Hotfix 2 test candidate\r\n\r\n'
    'Full UMM/Nexus-shaped mod package, including Hotfix 1. Not installed or published.\r\n'
    'Adds a separate 4-8-2 Express using existing high-speed drivers, the forward leading truck,\r\n'
    'normalized cylinders and existing calibration. The original Mountain and other layouts are retained.\r\n'
    '49 arrangements; 44 compatible powered-tender choices. No new meshes or physics algorithms.\r\n\r\n'
    'Restores existing compact-triplex crank pin renderer visibility; structural tests now require\r\n'
    'all six pins to have an enabled renderer, a mesh and detail-LOD membership.\r\n\r\n'
    f'Validation: clean net48 build; {tests["checks"]} managed quartering/tender/diagnostic checks across {tests["arrangementTenderCases"]} arrangement/tender cases and {tests["measuredEnginePairs"]} engine pairs; '
    f'{matrix["checks"]} structural-matrix checks; {followers["checks"]} added-wheel follower checks; '
    f'{tender["checks"]} tender integration checks; {express["checks"]} Express integration checks; '
    f'{pins["checks"]} crank pin checks across {pins["cases"]} managed setup cases; '
    f'{proof["checks"]} compiled checks with {proof["unchangedMethods"]} existing methods unchanged.\r\n\r\n'
    'The old setup order fails in the managed lifecycle model; corrected initialization and the new pass/fail checks pass. '
    'Real Unity Animator scheduling, visible rods/wheels and save/reload/pooling still require game testing. '
    'Run the structural button, then inspect both sides of the new Express, a Franklin duplex, Mallet, triplex and powered tender at a stop and moving slowly in both directions.\r\n'
)
zip_hash = sha(target.read_bytes())
(destination / 'STAGED_TEST_STATUS.txt').write_bytes(status.encode('ascii'))
(destination / 'CHECKSUMS.txt').write_text(f'{zip_hash}  {target.name}\n{sha(dll)}  RearrangedS282/RearrangedS282.dll\n', encoding='ascii')
copy.mkdir(parents=True, exist_ok=True)
(copy / 'README.txt').write_bytes(updates['RearrangedS282/README.txt'])
(copy / 'CHANGELOG.txt').write_bytes(updates['RearrangedS282/CHANGELOG.txt'])
desktop.mkdir(parents=True, exist_ok=True)
for p in (target, destination / 'STAGED_TEST_STATUS.txt', destination / 'CHECKSUMS.txt'):
    shutil.copyfile(p, desktop / p.name)
documents = sorted(p for p in copy.iterdir() if p.is_file())
assert len(documents) == 11
for p in documents:
    contents = p.read_text(encoding='utf-8-sig')
    if p.name.endswith('BBCODE.txt'):
        assert '**' not in contents and '](' not in contents
    shutil.copyfile(p, destination / p.name)
    shutil.copyfile(p, desktop / p.name)
    assert sha(p.read_bytes()) == sha((destination / p.name).read_bytes()) == sha((desktop / p.name).read_bytes())
assert sha((desktop / target.name).read_bytes()) == zip_hash
assert installed_before == {str(p.relative_to(installed)): sha(p.read_bytes()) for p in installed.rglob('*') if p.is_file()}
assert sha(baseline.read_bytes()) == '9115CE635B2B31B1D95D643F7B71B29F3627A5901619692CDB91A17E7CBD2EC3'
assert sha(public.read_bytes()) == '48A4C061CE75423B6DE0760AE76DC652521940B93520111856E585580E4760C0'
report = dict(passed=True, version=version, zip=str(target), desktop=str(desktop / target.name),
              zipSHA256=zip_hash, dllSHA256=sha(dll), files=len(names), changedEntries=changed,
              arrangements=49, poweredTenderOptions=44, releaseDocuments=[p.name for p in documents],
              installedUnchanged=True, hotfix1Unchanged=True, publicReleaseUnchanged=True,
              installed=False, published=False, runtimeExercised=False)
(work / 'package-validation.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
print(json.dumps(report, indent=2))
