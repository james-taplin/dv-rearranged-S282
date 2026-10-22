"""Render local managed-fixture pipe meshes for geometric visual QA only.

No game screenshots are edited. Material/texture/Unity rendering validation
still requires the game. The exporter and outputs remain in ignored build/.
"""
import json
import sys
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw

def render(cases, output):
    width, height = 1400, max(1, len(cases)) * 330
    image = Image.new('RGB', (width, height), (238, 239, 240))
    draw = ImageDraw.Draw(image)
    for row, case in enumerate(cases):
        meshes = case['meshes']
        selected = [m for m in meshes if len(m['vertices']) and np.asarray(m['vertices'])[:, 0].mean() >= 0]
        if not selected:
            continue
        for col, camera in enumerate([np.array([1., 0., 0.]), np.array([1., .4, .4])]):
            camera /= np.linalg.norm(camera)
            horizontal = np.cross(np.array([0., 1., 0.]), camera)
            horizontal /= np.linalg.norm(horizontal)
            vertical = np.cross(camera, horizontal)
            vertices = np.concatenate([np.asarray(m['vertices']) for m in selected])
            projected = np.array([vertices @ horizontal, vertices @ vertical]).T
            lo, hi = projected.min(axis=0), projected.max(axis=0)
            scale = min(650 / max(hi[0] - lo[0], .01), 250 / max(hi[1] - lo[1], .01))
            centre = (lo + hi) / 2
            offset = np.array([col * 700 + 350, row * 330 + 175])
            faces = []
            light = np.array([.4, .8, .3]); light /= np.linalg.norm(light)
            for mesh in selected:
                v = np.asarray(mesh['vertices'])
                triangles = np.asarray(mesh['triangles'], dtype=int).reshape(-1, 3)
                p = np.array([v @ horizontal, v @ vertical]).T
                p = (p - centre) * np.array([scale, -scale]) + offset
                xyz = v[triangles]
                normals = np.cross(xyz[:, 1] - xyz[:, 0], xyz[:, 2] - xyz[:, 0])
                norms = np.linalg.norm(normals, axis=1)
                shade = .5 + .4 * np.abs((normals / np.maximum(norms[:, None], 1e-12)) @ light)
                depth = xyz.mean(axis=1) @ camera
                for i, tri in enumerate(triangles):
                    colour = tuple(int(c * shade[i]) for c in (111, 117, 120))
                    faces.append((depth[i], p[tri], colour))
            for _, polygon, colour in sorted(faces, key=lambda item: item[0]):
                draw.polygon([tuple(p) for p in polygon], fill=colour)
            draw.text((col * 700 + 15, row * 330 + 12), case.get('name', 'pipe fixture') + (' / side' if col == 0 else ' / perspective'), fill=(20, 25, 30))
        draw.line((0, (row + 1) * 330 - 1, width, (row + 1) * 330 - 1), fill=(190, 195, 200))
    output.parent.mkdir(parents=True, exist_ok=True)
    image.save(output)

if __name__ == '__main__':
    data = json.loads(Path(sys.argv[1]).read_text(encoding='utf-8-sig'))
    render(data if isinstance(data, list) else data['cases'], Path(sys.argv[2]))
