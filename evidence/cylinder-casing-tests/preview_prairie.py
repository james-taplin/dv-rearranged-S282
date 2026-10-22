"""Orthographic comparison using actual production C# helper output."""
import json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import numpy as np
root=Path(__file__).resolve().parents[2]
folder=root/'build/prairie-casing-validation'
cases=json.loads((folder/'preview-geometry.json').read_text())
c=next(c for c in cases if c['name']=='native-lod0-left-prairie-full')
faces=np.asarray(c['triangles']);new=np.asarray(c['corrected']);s=c['scaleY']
previous=next(p for p in cases if p['name']=='native-lod0-left' and p['scaleY']==s and p['originY']==0)
old=np.asarray(previous['corrected'])
native=np.asarray(c['baseline'])/np.array([1,s,s])+np.array([0,(s-1)*.712937,(s-1)*8.84])
im=Image.new('RGB',(1800,1120),'#eef0f2');draw=ImageDraw.Draw(im)
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',20)
bold=ImageFont.truetype('C:/Windows/Fonts/segoeuib.ttf',24)
draw.text((25,15),'Prairie casing | actual mesh + production C# mapping | geometry preview, not a game screenshot',font=bold,fill='#17212d')
draw.text((25,53),'Motion axes retained. Inter-bore web accommodates the larger separation. Native reference aligned on the piston axis.',font=font,fill='#39495c')
allp=np.concatenate([native,old,new])
for row,(ax0,ax1,depth,label) in enumerate([(2,1,0,'side'),(0,1,2,'front')]):
 for col,(points,title) in enumerate([(native,'Native S282 reference'),(old,'Installed 1.13.0 treatment'),(new,'Full Prairie normalization')]):
  left=20+col*595;top=110+row*500;w=575;h=480
  draw.rounded_rectangle((left,top,left+w,top+h),radius=8,fill='white',outline='#c8cfd8')
  draw.text((left+15,top+10),title,font=bold,fill='#17212d')
  draw.text((left+15,top+45),f'{label} view   height {np.ptp(points[:,1]):.3f} m   length {np.ptp(points[:,2]):.3f} m',font=font,fill='#39495c')
  lo=allp[:,[ax0,ax1]].min(axis=0)-.08;hi=allp[:,[ax0,ax1]].max(axis=0)+.08
  factor=min((w-65)/(hi[0]-lo[0]),(h-130)/(hi[1]-lo[1]))
  def project(p):return (left+w/2+(p[ax0]-(lo[0]+hi[0])/2)*factor,top+h-35-(p[ax1]-lo[1])*factor)
  for t in faces[np.argsort(points[faces][:,:,depth].mean(axis=1))]:
   a,b,d=points[t];n=np.cross(b-a,d-a);n=n/max(np.linalg.norm(n),1e-12)
   grey=int(80+100*abs(n@np.array([.4,.65,.7])))
   draw.polygon([project(points[i]) for i in t],fill=(grey,grey+6,grey+10),outline='#7b858d',width=1)
  for y in [.712937*s,1.3470917*s]:
   p0=np.zeros(3);p0[ax0]=lo[0];p0[ax1]=y;p1=p0.copy();p1[ax0]=hi[0]
   draw.line((project(p0),project(p1)),fill='#b26040',width=1)
im.save(folder/'prairie-preview.png')
print(folder/'prairie-preview.png')
