"""Check native wheel/tank envelopes; native game data stays in build output."""
import hashlib
import json
import math
import sys
from pathlib import Path
import numpy as np

root=Path.cwd()
native=Path(sys.argv[1]); managed=Path(sys.argv[2]); output=Path(sys.argv[3])
result=json.loads(managed.read_text(encoding='utf-8-sig'))
if not result['passed'] or result['arrangementId']!=49: raise RuntimeError('Managed geometry did not pass')
data=json.loads(native.read_text(encoding='utf-8-sig'))
source=(root/'evidence/wheel-clearance-tests/measure.py').read_text(encoding='utf-8')
source=source.replace("(root/'native-clearance.json').read_text()", "Path(sys.argv[1]).read_text(encoding='utf-8-sig')")
if "(root/'native-clearance.json').read_text()" in source: raise RuntimeError('Native measurement input was not redirected')
scope={'__file__':str(root/'evidence/wheel-clearance-tests/measure.py'),'__name__':'scotch_native_measurement','sys':sys}
exec(compile(source,scope['__file__'],'exec'),scope)
scale=float(result['driverScale']); offset=float(result['driveOffsetZ']); radius=float(result['driverRadiusM'])
points=scope['p'].copy();points[:,2]-=.1 # Show6Drivers native Pacific reservoir offset
profiles=scope['profiles']; wheels=scope['wheels']; gaps={}
for axle in (2,3,4):
    w=wheels[axle];profile=profiles[1 if axle==2 else axle] # second uses native flanged mesh
    delta=np.hypot(points[:,1]-w['position'][1]*scale,points[:,2]-(w['position'][2]*scale+offset))-scale*profile
    gaps[axle]=float(delta[profile>0].min())
if min(gaps.values())<.05: raise RuntimeError('Native reservoir rotational envelope clearance below 50 mm: '+str(gaps))

# A deliberately oversized frame must fail the same native-envelope bound.
bad=1.42; bad_offset=-2.1+(1.2879-bad)*wheels[2]['position'][2]
oversized=[]
for axle in (2,3,4):
    w=wheels[axle];profile=profiles[1 if axle==2 else axle]
    d=np.hypot(points[:,1]-w['position'][1]*bad,points[:,2]-(w['position'][2]*bad+bad_offset))-bad*profile
    oversized.append(float(d[profile>0].min()))
if min(oversized)>=.05: raise RuntimeError('Oversized-wheel negative control was not rejected')

# Native front/rear bogie pivots (8.0 and 3.2m) and the existing Pacific
# two-wheel truck datums; use maximum native driver envelope, not just tread.
# +/-15 degrees samples yaw and straight running, not moving suspension.
native_wheels=[w for w in data['wheels'] if w['lod']==0 and w['axle'] in (1,3,4)]
max_driver=max(float(np.max(np.hypot(np.array(w['vertices'])[:,1],np.array(w['vertices'])[:,2]))) for w in native_wheels)*scale
front=wheels[2]['position'][2]*scale+offset
rear=wheels[4]['position'][2]*scale+offset
truck={}
for option,truck_radius in [('vanilla',.355*.92),('small',.355*1.25)]:
    for yaw in (-15,0,15):
        c=math.cos(math.radians(yaw))
        front_gap=(8+.3*c)-front-max_driver-.355*1.25
        rear_gap=rear-(3.2-3.1*c)-max_driver-truck_radius
        truck[f'{option}:{yaw}']={'frontM':front_gap,'rearM':rear_gap}
        if min(front_gap,rear_gap)<.05: raise RuntimeError('Truck tyre envelope clearance below 50 mm: '+str(truck))

# The flanged and driving-3 native meshes both carry a conical tread. Measure
# the midpoint of two native LOD0 rings; compare the physics radius to it.
rings={}
for axle in (1,3,4):
    w=next(w for w in data['wheels'] if w['lod']==0 and w['axle']==axle)
    verts=np.array(w['vertices']);rad=np.hypot(verts[:,1],verts[:,2]);means=[]
    for x in (.71522,.80069):
        values=rad[(abs(verts[:,0]-x)<.0002)&(rad>.69)&(rad<.73)]
        if len(values)<10: raise RuntimeError('Native tread ring missing')
        means.append(float(np.median(values)))
    midpoint=sum(means)*.5*scale;rings[axle]={'nativeRingRadiiM':means,'scaledMidpointM':midpoint}
    if abs(radius-midpoint)>.002: raise RuntimeError('Physics and native mid-tread radii disagree')

def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest().upper()
result.update({'nativeReservoirMinimumGapM':min(gaps.values()),'reservoirPerAxleM':gaps,
 'oversizedNegativeControlGapM':min(oversized),
 'truckEnvelopeGapsM':truck,'nativeTreadRings':rings,
 'nativeDataSHA256':sha(native),'managedSourceSHA256':sha(root/'RearrangedS282/WheelRearranger.ScotchGlider.cs'),
 'nativeExtractorSHA256':sha(root/'evidence/wheel-clearance-tests/extract_native.py'),
 'nativeMeasureSHA256':sha(root/'evidence/wheel-clearance-tests/measure.py'),
 'nativeAssetSHA256':sha(r'B:\SteamLibrary\steamapps\common\Derail Valley\DerailValley_Data\resources.assets'),
 'scope':result['scope']+' Native reservoir and wheel mesh envelopes at two LODs; truck clearances use conservative radius and sampled yaw. No integrated caller, cylinder/rod collision, live suspension, Unity rendering, game or multiplayer acceptance.'})
output.write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps({k:result[k] for k in ('passed','arrangementId','driverRadiusM','nativeReservoirMinimumGapM')},indent=2))
