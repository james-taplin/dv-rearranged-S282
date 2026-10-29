"""Stage a separate test package; leave the released ZIP and installation alone."""
import hashlib
import json
from pathlib import Path
import zipfile

root = Path(__file__).resolve().parents[2]
base = root / 'release/2.0.2-beta/Rearranged S282 B99 Fork 2.0.2b.zip'
assert hashlib.sha256(base.read_bytes()).hexdigest().upper() == '1D3641F44E34085B1383FE979AF2D2BF8950348733DD7B0B0D7E809A2BD12E37'
work = root / 'build/lubrication-polish'
proof = json.loads((work / 'preservation.json').read_text(encoding='utf-8-sig'))
dll = (work / 'bin/RearrangedS282.dll').read_bytes()
assert proof['passed'] and hashlib.sha256(dll).hexdigest().upper() == proof['testDllHash']
target = work / 'RearrangedS282-2.0.2-beta-Lubrication-Test.zip'
note = (root / 'evidence/lubrication-polish/README.txt').read_bytes()
with zipfile.ZipFile(base) as old, zipfile.ZipFile(target, 'w', zipfile.ZIP_DEFLATED) as new:
    for item in old.infolist():
        data = old.read(item)
        if item.filename == 'RearrangedS282/RearrangedS282.dll':
            data = dll
        elif item.filename == 'RearrangedS282/README.txt':
            data = b'UNRELEASED LUBRICATION TEST - 26 October 2026\r\nSee LUBRICATION_TEST.txt before testing\r\n\r\n' + data
        new.writestr(item.filename, data)
    new.writestr('RearrangedS282/LUBRICATION_TEST.txt', note)
with zipfile.ZipFile(base) as old, zipfile.ZipFile(target) as new:
    assert new.testzip() is None
    changed = [n for n in old.namelist() if old.read(n) != new.read(n)]
    assert sorted(changed) == ['RearrangedS282/README.txt', 'RearrangedS282/RearrangedS282.dll']
    assert len(new.namelist()) == len(old.namelist()) + 1
receipt = dict(path=str(target), sha256=hashlib.sha256(target.read_bytes()).hexdigest().upper(), changed=changed,
               installed=False, published=False, runtimeValidated=False)
(work / 'package.json').write_text(json.dumps(receipt, indent=2))
print(json.dumps(receipt))
