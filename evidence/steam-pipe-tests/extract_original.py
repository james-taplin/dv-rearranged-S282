"""Extract original MIT mod meshes for the local fixture; never reads game assets.

Run from any directory with UnityPy installed. Native casing/fitting evidence is
provided separately by evidence/cylinder-casing-tests extraction scripts.
"""
import hashlib
import json
from pathlib import Path
import UnityPy
from UnityPy.helpers.MeshHelper import MeshHandler

ROOT = Path(__file__).resolve().parents[2]
BUNDLE_SHA = '44A1B9EAE0DDA58BE97B9AEDF3877522FBFFAEC060FE02A8385158E095BDB22D'
SOURCES = [
    ('steam_manifold', 3104848846201758263, 'C0F6CE1298AB861A5C2AEC14F1EF2C45565240B98BA81B2CF66D2FD8358E52D3'),
    ('steam_transfer', -1674988028507592836, 'ED782BCCA206AD25E80758DE9DA72F6DDBDA06B7B146957E4F2770B4532346E1'),
]

def main():
    bundle = ROOT/'s282_drivetrain/ExportedProject/Assets/AssetBundles/s282_drivetrain'
    assert hashlib.sha256(bundle.read_bytes()).hexdigest().upper() == BUNDLE_SHA
    env = UnityPy.load(str(bundle))
    meshes = []
    for name, path_id, expected in SOURCES:
        path = ROOT/'models'/f'{name}.obj'
        assert hashlib.sha256(path.read_bytes()).hexdigest().upper() == expected
        source = next(o for o in env.objects if o.path_id == path_id)
        handler = MeshHandler(source.read()); handler.process()
        vertices = [list(v[:3]) for v in handler.m_Vertices]
        faces = [list(t) for group in handler.get_triangles() for t in group]
        # Source OBJ keeps one vertex/UV/normal entry per authored vertex.
        exported = [[float(n) for n in line.split()[1:]] for line in path.read_text().splitlines() if line.startswith('v ')]
        assert len(exported) == len(vertices)
        assert max(abs(a-b) for v,w in zip(vertices, exported) for a,b in zip(v,w)) < 1e-7
        meshes.append(dict(name=name, pathId=path_id, vertices=vertices, triangles=faces, uv=[list(v[:2]) for v in handler.m_UV0], sha256=expected))
    target = ROOT/'build/steam-pipes-1.18.0/original-assets.json'
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(json.dumps(dict(bundleSHA256=BUNDLE_SHA, meshes=meshes)))
    print(target)

if __name__ == '__main__':
    main()
