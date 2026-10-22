"""Local evidence only: extract actual installed S282 casing triangles.

No game/mod files are modified. Replays the installed LMS bounding-box triangle
selection and preceding deletions, using its decompiled splitter references.
Native game vertices stay local evidence; this file is not a mod runtime asset.
"""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def obj(path):
    vertices, faces = [], []
    for line in path.read_text(encoding="utf-8-sig").splitlines():
        bits = line.split()
        if not bits:
            continue
        if bits[0] == "v":
            vertices.append(tuple(map(float, bits[1:4])))
        elif bits[0] == "f":
            ids = [int(b.split("/")[0]) - 1 for b in bits[1:]]
            if len(ids) != 3:
                raise ValueError("Expected actual exported triangle faces")
            faces.append(tuple(ids))
    return vertices, faces


def extract_lod(source_path, reference, lod, include_pipes=False, include_guides=False):
    text = reference.read_text(encoding="utf-8-sig")
    ranges = {
        name: (float(low.replace("f", "")), float(high.replace("f", "")))
        for name, low, high in re.findall(
            r"RangeFloat\s+(\w+)\s*=\s*new MeshSplittersUtil.RangeFloat\(([-.\df]+),\s*([-.\df]+)\)", text
        )
    }
    vertices, faces = obj(source_path)

    def selected(face, box):
        return all(all(ranges[name][0] <= vertices[i][axis] <= ranges[name][1]
                       for axis, name in enumerate(box)) for i in face)

    def box_args(line):
        m = re.search(r"(?:markPartOfMesh|hidePartOfMesh)\(val(?:\.vertices)?,\s*(?:array\d*,\s*)?(\w+),\s*(\w+),\s*(\w+)\)", line)
        return m.groups() if m else None

    def part_boxes(method):
        body = text.split("private static Mesh " + method + "(", 1)[1].split("\n\t}", 1)[0]
        return [b for line in body.splitlines() if (b := box_args(line))]

    active = list(range(len(faces)))
    pending = []
    parts = {}
    # Earlier native vertex-number edits concern the body/frame and do not
    # intersect these casing selections. Keep exported casing coordinates.
    main = text.split('Mesh val = MeshFinder.FindMesh(', 1)[1]
    for line in main.splitlines():
        if re.search(r"array\d* = \(int\[\]\)val.triangles.Clone", line):
            pending = []
        if "hidePartOfMesh(val," in line:
            b = box_args(line)
            active = [i for i in active if not selected(faces[i], b)]
        elif "markPartOfMesh(val.vertices," in line:
            pending.append(box_args(line))
        elif "deleteMarkedPartOfMesh(val," in line:
            active = [i for i in active if not any(selected(faces[i], b) for b in pending)]
        selections = (
            ("cylinderLMesh", "getCylinderLMesh", "left"),
            ("cylinderRMesh", "getCylinderRMesh", "right"),
            ("cylinderCocksMesh", "getCylinderCocksMesh", "cocks"),
        ) + ((
            ("dryPipeLMesh", "getDryPipeLMesh", "pipe-left"),
            ("dryPipeRMesh", "getDryPipeRMesh", "pipe-right"),
        ) if include_pipes else ())
        selections += (("crossheadSupportLMesh", "getCrossheadSupportLMesh", "guide-left"),
                       ("crossheadSupportRMesh", "getCrossheadSupportRMesh", "guide-right")) if include_guides else ()
        for variable, method, name in selections:
            if f"Mesh {variable} = {method}(val);" not in line:
                continue
            boxes = part_boxes(method)
            chosen = [faces[i] for i in active if any(selected(faces[i], b) for b in boxes)]
            used = sorted({i for face in chosen for i in face})
            remap = {v: i for i, v in enumerate(used)}
            offset = 4.88 if lod == 0 else 4.885
            points = [[vertices[i][0], vertices[i][1], vertices[i][2] + offset] for i in used]
            parts[name] = {
                "name": f"native-lod{lod}-{name}", "kind": "native", "lod": lod,
                "bodyZOffset": offset, "sourceVertexIds": used,
                "vertices": points, "triangles": [[remap[i] for i in f] for f in chosen],
                "boundsMin": [min(p[a] for p in points) for a in range(3)],
                "boundsMax": [max(p[a] for p in points) for a in range(3)],
            }
        if len(parts) == 3 + (2 if include_pipes else 0) + (2 if include_guides else 0):
            break
    if len(parts) != 3 + (2 if include_pipes else 0) + (2 if include_guides else 0):
        raise ValueError(f"Unable to replay all native LOD{lod} selections")
    return {"source": str(source_path), "sourceSHA256": sha(source_path),
            "splitterReferenceSHA256": sha(reference), "parts": list(parts.values())}


def franklin(bundle, export_path):
    import UnityPy
    from UnityPy.helpers.MeshHelper import MeshHandler
    env = UnityPy.load(str(bundle))
    reader = next(o for o in env.objects if o.path_id == 4793994185512481848)
    mesh = reader.read()
    handler = MeshHandler(mesh)
    handler.process()
    # Existing Dummiesman loader preserves vertices, normals, UVs and winding;
    # unlike some OBJ exporters it performs no handedness conversion.
    triangles = [list(t) for sub in handler.get_triangles() for t in sub]
    normals = [tuple(v[:3]) for v in handler.m_Normals]
    uvs = [tuple(v[:2]) for v in handler.m_UV0]
    if len(normals) != len(handler.m_Vertices) or len(uvs) != len(normals):
        raise ValueError("Franklin casing needs one authored normal and UV per vertex")
    lines = [
        "# Rearranged S282 original Franklin casing, readable fallback",
        "# Source: original MIT-licensed s282_drivetrain bundle; retain its license/credits.",
        "# Bundle SHA256 " + sha(bundle), "# Mesh path ID 4793994185512481848",
        "g franklin_casing",
    ]
    lines += ["v " + " ".join(format(c, ".9g") for c in p[:3]) for p in handler.m_Vertices]
    lines += ["vt " + " ".join(format(c, ".9g") for c in p) for p in uvs]
    lines += ["vn " + " ".join(format(c, ".9g") for c in p) for p in normals]
    lines += ["f " + " ".join(f"{i+1}/{i+1}/{i+1}" for i in face) for face in triangles]
    export_path.write_text("\n".join(lines) + "\n", encoding="utf-8")
    # Independently parse the emitted indexed data to catch exporter indexing,
    # winding, coordinate, UV and normal mistakes before runtime loads the file.
    parsed = {"v": [], "vt": [], "vn": [], "f": []}
    for line in export_path.read_text(encoding="utf-8").splitlines():
        words = line.split()
        if not words or words[0] not in parsed:
            continue
        if words[0] == "f":
            parsed["f"].append([tuple(int(c)-1 for c in w.split("/")) for w in words[1:]])
        else:
            parsed[words[0]].append(tuple(map(float, words[1:])))
    assertions = 0
    def check(value, message):
        nonlocal assertions
        assertions += 1
        if not value:
            raise ValueError(message)
    for key, original in (("v", handler.m_Vertices), ("vt", uvs), ("vn", normals)):
        check(len(parsed[key]) == len(original), f"{key} export count")
        for actual, expected in zip(parsed[key], original):
            check(all(abs(a-b) < 1e-8 for a, b in zip(actual, expected)), f"{key} numeric round trip")
    check(len(parsed["f"]) == len(triangles), "Triangle export count")
    for actual, expected in zip(parsed["f"], triangles):
        check(all(indices == (i, i, i) for indices, i in zip(actual, expected)), "Authored winding/UV/normal indices")
    # Original authored cylinder's child 'default' has identity TRS; cylinder
    # parent at (1.09,.71,9.49), scale(-1,1,1), in the drive's native frame.
    points = [[1.09 - p[0], .71 + p[1], 9.49 + p[2]] for p in handler.m_Vertices]
    return {"source": str(bundle), "sourceSHA256": sha(bundle),
            "meshPathID": reader.path_id, "readableFallback": str(export_path),
            "readableFallbackSHA256": sha(export_path), "exportAssertions": assertions, "parts": [{
                "name": "franklin-type-b-cylinder-default", "kind": "franklin", "lod": 0,
                "vertices": points, "triangles": triangles,
                "boundsMin": [min(p[a] for p in points) for a in range(3)],
                "boundsMax": [max(p[a] for p in points) for a in range(3)],
            }]}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--game", default=r"B:\SteamLibrary\steamapps\common\Derail Valley")
    parser.add_argument("--output", type=Path, default=Path(__file__).with_name("native-casings.json"))
    parser.add_argument("--ilspy", default=r"C:\Users\james\.dotnet\tools\ilspycmd.exe")
    parser.add_argument("--export-franklin", type=Path)
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[2]
    folder = Path(__file__).parent
    lms = Path(args.game) / "Mods" / "LocoMeshSplitter"
    result = {"note": "Local actual-game mesh evidence; never included in release packages.", "sources": []}
    for lod in (0, 1):
        reference = folder / f"LMS_LOD{lod}.reference.txt"
        if not reference.exists():
            reference.write_text(subprocess.check_output([
                args.ilspy, "-t", f"LocoMeshSplitter.MeshSplitters.S282A.S282ALOD{lod}MeshSplitter",
                str(lms / "LocoMeshSplitter.dll")], text=True, encoding="utf-8"), encoding="utf-8")
        source = lms / "assets" / ("s282_locomotive_body" + ("_LOD1" if lod else "") + ".obj")
        result["sources"].append(extract_lod(source, reference, lod))
    export_path = args.export_franklin or root / "models/franklin_casing.obj"
    result["sources"].append(franklin(root / "s282_drivetrain/ExportedProject/Assets/AssetBundles/s282_drivetrain", export_path))
    args.output.write_text(json.dumps(result, separators=(",", ":")), encoding="utf-8")
    print(json.dumps({"output": str(args.output), "parts": [
        {"name": p["name"], "vertices": len(p["vertices"]), "faces": len(p["triangles"]),
         "boundsMin": p["boundsMin"], "boundsMax": p["boundsMax"]}
        for source in result["sources"] for p in source["parts"]]}, indent=2))


if __name__ == "__main__":
    main()
