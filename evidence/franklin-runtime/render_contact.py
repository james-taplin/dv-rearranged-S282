"""Plot private measured contact meshes; never writes game or mod assets."""
import argparse, json, sys
from pathlib import Path
p = argparse.ArgumentParser()
p.add_argument('contact', type=Path)
p.add_argument('output', type=Path)
p.add_argument('--pillow-site')
a = p.parse_args()
if a.pillow_site: sys.path.append(a.pillow_site)
from PIL import Image, ImageDraw
d = json.loads(a.contact.read_text())
image = Image.new('RGB', (1600, 880), '#f7f7f5')
draw = ImageDraw.Draw(image)
draw.text((28, 18), d['pair'], fill='#202020')
pose = d['pose']
draw.text((28, 42), 'Phase %s; yaw %s degrees; red marker = measured surface contact' % (pose['phase'], pose['yaw']), fill='#202020')
colors = {'a': (220, 100, 25, 90), 'b': (45, 100, 155, 45)}
for panel, axes, title in [(0, (0, 2), 'Top: X / Z'), (1, (2, 1), 'Side: Z / Y')]:
    x0, y0, w, h = 20 + panel * 800, 100, 760, 720
    points = d['a']['vertices'] + d['b']['vertices']
    u, v = axes
    lo = [min(p[i] for p in points) for i in axes]
    hi = [max(p[i] for p in points) for i in axes]
    scale = min((w - 80) / (hi[0] - lo[0]), (h - 80) / (hi[1] - lo[1]))
    cx, cy = (lo[0] + hi[0]) / 2, (lo[1] + hi[1]) / 2
    def project(p): return (x0 + w / 2 + (p[u] - cx) * scale, y0 + h / 2 - (p[v] - cy) * scale)
    draw.text((x0 + 20, y0 - 24), title, fill='#202020')
    for key in ('b', 'a'):
        layer = Image.new('RGBA', image.size)
        ld = ImageDraw.Draw(layer)
        mesh = d[key]
        triangles = mesh['triangles']
        if triangles and isinstance(triangles[0], int):
            triangles = [triangles[i:i+3] for i in range(0, len(triangles), 3)]
        for tri in triangles:
            polygon = [project(mesh['vertices'][i]) for i in tri]
            ld.polygon(polygon, fill=colors[key], outline=colors[key][:3] + (100,))
        image = Image.alpha_composite(image.convert('RGBA'), layer).convert('RGB')
        draw = ImageDraw.Draw(image)
    px, py = project(pose['witness'])
    draw.ellipse((px-8, py-8, px+8, py+8), outline='#cc1515', width=3)
    draw.line((px-13, py, px+13, py), fill='#cc1515', width=2)
    draw.line((px, py-13, px, py+13), fill='#cc1515', width=2)
draw.text((28, 850), 'Orange: moving part. Blue: following casing. Orthographic mesh projections; not a rendered game screenshot.', fill='#202020')
image.save(a.output)
