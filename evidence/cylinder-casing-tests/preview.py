"""Render orthographic numerical previews from actual C# helper output."""
import json
import math
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

root=Path(__file__).resolve().parents[2]
folder=root/'build/cylinder-casing-validation'
cases=json.loads((folder/'preview-geometry.json').read_text())
chosen=[next(c for c in cases if c['name']=='native-lod0-left' and abs(c['scaleY']-s)<1e-5 and c['originY']==0) for s in (.58,1.2879)]
image=Image.new('RGB',(2200,1200),'#eef0f2')
draw=ImageDraw.Draw(image)
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',21)
small=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',16)
bold=ImageFont.truetype('C:/Windows/Fonts/segoeuib.ttf',25)
draw.text((28,14),'Actual native casing mesh | C# shape-helper output | wireframe + flat shading',fill='#17212d',font=bold)
draw.text((28,53),'Red = triangles whose normal turned over 90 degrees; blue = rail level. These are geometry previews, not Unity screenshots.',fill='#39495c',font=font)

def normal(a,b,c):
    x=[b[i]-a[i] for i in range(3)];y=[c[i]-a[i] for i in range(3)]
    n=[x[1]*y[2]-x[2]*y[1],x[2]*y[0]-x[0]*y[2],x[0]*y[1]-x[1]*y[0]]
    length=math.sqrt(sum(v*v for v in n))
    return [v/length for v in n] if length>1e-12 else [0,0,0]

for group,c in enumerate(chosen):
    for row,(axes,view) in enumerate((((2,1),'side elevation'),((0,1),'front elevation'))):
        both=c['baseline']+c['corrected']
        xlo=min(p[axes[0]] for p in both);xhi=max(p[axes[0]] for p in both)
        ylo=0;yhi=max(p[axes[1]] for p in both)+.08
        for after in (False,True):
            col=group*2+int(after)
            left=25+col*545;top=110+row*525;width=520;height=495
            draw.rounded_rectangle((left,top,left+width,top+height),radius=9,fill='white',outline='#c8cfd8')
            draw.text((left+13,top+9),f"scale {c['scaleY']:.4g} | {'corrected' if after else 'existing'}",fill='#17212d',font=bold)
            draw.text((left+13,top+42),view,fill='#4b5969',font=small)
            points=c['corrected'] if after else c['baseline']
            plot=(left+30,top+83,left+width-22,top+height-48)
            factor=min((plot[2]-plot[0])/(xhi-xlo+.12),(plot[3]-plot[1])/(yhi-ylo+.05))
            mid=(plot[0]+plot[2])*.5
            def project(p):return (mid+(p[axes[0]]-(xlo+xhi)*.5)*factor,plot[3]-(p[axes[1]]-ylo)*factor)
            draw.line((plot[0],plot[3],plot[2],plot[3]),fill='#2563ba',width=2)
            triangles=sorted(c['triangles'],key=lambda tri:sum(points[i][0 if row==0 else 2] for i in tri)/3)
            for tri in triangles:
                n=normal(*(points[i] for i in tri))
                light=abs(n[0]*.3+n[1]*.75+n[2]*.6)
                level=int(85+100*min(1,light))
                poly=[project(points[i]) for i in tri]
                draw.polygon(poly,fill=(level,level+6,level+10),outline='#77818b',width=1)
            if after:
                for item in c['turnedFaceIndices']:
                    poly=[project(points[i]) for i in item['indices']]
                    draw.line(poly+[poly[0]],fill='#e12b32',width=3)
            draw.text((left+13,top+height-34),f"rail clearance {min(p[1] for p in points):.3f} m | top {max(p[1] for p in points):.3f} m",fill='#39495c',font=small)

image.save(folder/'preview.png')
print(folder/'preview.png')
