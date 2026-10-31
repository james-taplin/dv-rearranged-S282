"""Private geometry payload: current installed LMS selection over its OBJ cache."""
import argparse, hashlib, importlib.util, itertools, json, math, subprocess
from collections import Counter,defaultdict
from pathlib import Path
import UnityPy

ROOT=Path(__file__).resolve().parents[2]
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--game',type=Path,default=Path(r'B:\SteamLibrary\steamapps\common\Derail Valley'))
parser.add_argument('--output-directory',type=Path,default=ROOT/'build/franklin-runtime')
parser.add_argument('--ilspy',type=Path,default=Path(r'C:\Users\james\.dotnet\tools\ilspycmd.exe'))
parser.add_argument('--native-report',type=Path,help='Prefer measured actual Unity LMS output and compare it with the independent source-range replay.')
args=parser.parse_args()
GAME=args.game
OUT=args.output_directory
OUT.mkdir(parents=True,exist_ok=True)
LMS=GAME/'Mods/LocoMeshSplitter'
for name,typename in [('LMS_LOD0.reference.txt','LocoMeshSplitter.MeshSplitters.S282A.S282ALOD0MeshSplitter'),('LMS_Loader.reference.txt','LocoMeshSplitter.MeshLoaders.S282AMeshLoader')]:
 (OUT/name).write_text(subprocess.check_output([str(args.ilspy),'-t',typename,str(LMS/'LocoMeshSplitter.dll')],text=True,encoding='utf-8'),encoding='utf-8')

def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest().upper()
def mul(a,b):return [[sum(a[r][k]*b[k][c] for k in range(4)) for c in range(4)] for r in range(4)]
def trs(p,q,s):
 x,y,z,w=q
 return [[(1-2*y*y-2*z*z)*s[0],(2*x*y-2*z*w)*s[1],(2*x*z+2*y*w)*s[2],p[0]],
         [(2*x*y+2*z*w)*s[0],(1-2*x*x-2*z*z)*s[1],(2*y*z-2*x*w)*s[2],p[1]],
         [(2*x*z-2*y*w)*s[0],(2*y*z+2*x*w)*s[1],(1-2*x*x-2*y*y)*s[2],p[2]],
         [0,0,0,1]]
def tdata(t):return dict(name=t.m_GameObject.deref().read().m_Name,position=[t.m_LocalPosition.x,t.m_LocalPosition.y,t.m_LocalPosition.z],rotation=[t.m_LocalRotation.x,t.m_LocalRotation.y,t.m_LocalRotation.z,t.m_LocalRotation.w],scale=[t.m_LocalScale.x,t.m_LocalScale.y,t.m_LocalScale.z])
def tm(t):
 d=tdata(t);return trs(d['position'],d['rotation'],d['scale'])
def col(m):return [m[r][c] for c in range(4) for r in range(4)]

spec=importlib.util.spec_from_file_location('casing',ROOT/'evidence/cylinder-casing-tests/extract_native.py')
casing=importlib.util.module_from_spec(spec);spec.loader.exec_module(casing)
reference=OUT/'LMS_LOD0.reference.txt'
data=casing.extract_lod(LMS/'assets/s282_locomotive_body.obj',reference,0,include_pipes=True,include_guides=True)

# Resolve the actual native Static_LOD0 hierarchy, excluding the body itself.
env=UnityPy.load(str(GAME/'DerailValley_Data/resources.assets'))
matches=[]
for obj in env.objects:
 if obj.type.name!='Transform':continue
 t=obj.read()
 if t.m_GameObject.deref().read().m_Name!='Static_LOD0':continue
 parent=t.m_Father.deref().read()
 if parent.m_GameObject.deref().read().m_Name!='LocoS282A_Body':continue
 root=parent.m_Father.deref().read()
 if root.m_GameObject.deref().read().m_Name=='LocoS282A':matches.append(t)
assert len(matches)==1,len(matches)
static=matches[0]
body=static.m_Father.deref().read()
moving=next(c.deref().read() for c in body.m_Children if c.deref().read().m_GameObject.deref().read().m_Name=='MovingParts_LOD0')
if tm(moving)!=trs([0,0,0],[0,0,0,1],[1,1,1]):raise ValueError('Native MovingParts_LOD0 measurement frame changed; derive its inverse before exporting static matrices')
matrix=mul(tm(static),mul(trs([0,0,4.88],[0,0,0,1],[1,1,1]),trs([0,0,0],[0,0,0,1],[-1,1,1])))
names={'left':'s282a_cylinder_l','right':'s282a_cylinder_r','cocks':'s282a_cylinder_cocks','pipe-left':'s282a_dry_pipe_l','pipe-right':'s282a_dry_pipe_r','guide-left':'s282a_crosshead_guide_l','guide-right':'s282a_crosshead_guide_r'}
parts=[]
for part in data['parts']:
 short=part['name'].removeprefix('native-lod0-')
 vertices=[[p[0],p[1],p[2]-4.88] for p in part['vertices']]
 points=[[sum(matrix[r][c]*v[c] for c in range(3))+matrix[r][3] for r in range(3)] for v in vertices]
 parts.append(dict(name=names[short],kind=short,lod=0,vertices=vertices,triangles=[i for f in part['triangles'] for i in f],
  nativeBodyFromMesh=col(matrix),position=[0,0,0],rotation=[0,0,0,1],scale=[-1,1,1],
  meshBoundsMin=[min(p[a] for p in vertices) for a in range(3)],meshBoundsMax=[max(p[a] for p in vertices) for a in range(3)],
  bodyBoundsMin=[min(p[a] for p in points) for a in range(3)],bodyBoundsMax=[max(p[a] for p in points) for a in range(3)],
  active=True,renderer=True,sourceVertexIds=part['sourceVertexIds']))
result=dict(nativeDataOnly=True,source=str(LMS/'assets/s282_locomotive_body.obj'),sourceSHA256=digest(LMS/'assets/s282_locomotive_body.obj'),
 lmsDLL=str(LMS/'LocoMeshSplitter.dll'),lmsSHA256=digest(LMS/'LocoMeshSplitter.dll'),lmsVersion=json.loads((LMS/'Info.json').read_text(encoding='utf-8-sig'))['Version'],
 generatedGameVersion=(LMS/'assets/_generated_in_game_version.dat').read_text(encoding='utf-8-sig'),
 splitterReference=str(reference),splitterReferenceSHA256=digest(reference),loaderReference=str(OUT/'LMS_Loader.reference.txt'),loaderReferenceSHA256=digest(OUT/'LMS_Loader.reference.txt'),
 nativeResource=str(GAME/'DerailValley_Data/resources.assets'),nativeResourceSHA256=digest(GAME/'DerailValley_Data/resources.assets'),
 matrixConvention='Unity column-major: matrix[row,column]=nativeBodyFromMesh[column*4+row]; vertices are original OBJ-local, not reflected or translated.',
 nativeMeasurementFrame=tdata(moving),
 hierarchy=[tdata(static),dict(name='s282a_split_body(Clone)',position=[0,0,4.88],rotation=[0,0,0,1],scale=[1,1,1])],parts=parts,
 limitations='Replays exact current installed LMS triangle selection/preselection deletion against its exported cache, compacting unused vertices. Part -X reflection/root +4.88 transform from freshly decompiled installed LMS; Static_LOD0 TRS from actual native resource. No LMS initialization, Unity Mesh import, materials or full TrainCar lifecycle executed by this extraction. Native vertex data stays private under build/.')
if args.native_report:
 native=json.loads(args.native_report.read_text(encoding='utf-8-sig'))
 if not native.get('passed') or native.get('lmsSHA256')!=result['lmsSHA256'] or native.get('sourceSHA256')!=result['sourceSHA256']:
  raise ValueError('Actual Unity LMS report must pass and match the current installed DLL/cache SHA256')
 (OUT/'wals-static-replay.json').write_text(json.dumps(result,separators=(',',':')),encoding='utf-8')
 comparisons=[]
 for real in native['parts']:
  expected=next(p for p in result['parts'] if p['name']==real['name'])
  eps=.000003
  cells=defaultdict(list)
  for i,p in enumerate(expected['vertices']):cells[tuple(round(c/eps) for c in p)].append(i)
  mapped=[];maximum=0
  for p in real['vertices']:
   cell=tuple(round(c/eps) for c in p)
   choices=[i for delta in itertools.product((-1,0,1),repeat=3) for i in cells[tuple(a+b for a,b in zip(cell,delta))]]
   if not choices:choices=range(len(expected['vertices']))
   closest=min(choices,key=lambda i:sum((a-b)**2 for a,b in zip(p,expected['vertices'][i])))
   maximum=max(maximum,math.dist(p,expected['vertices'][closest]));mapped.append(tuple(expected['vertices'][closest]))
  def canonical(triangle):return min(tuple(triangle[i:]+triangle[:i]) for i in range(3))
  def faces(indices,points):return Counter(canonical([tuple(points[j]) for j in indices[i:i+3]]) for i in range(0,len(indices),3))
  triangles_match=faces(real['triangles'],mapped)==faces(expected['triangles'],expected['vertices'])
  matrix_delta=max(abs(a-b) for a,b in zip(real['nativeBodyFromMesh'],expected['nativeBodyFromMesh']))
  if (real['name'].startswith('s282a_cylinder') and (not triangles_match or maximum>eps)) or matrix_delta>.000001:
   raise ValueError('Actual LMS cylinder/matrix differs from independent replay: '+real['name'])
  comparisons.append(dict(name=real['name'],nativeVertices=len(real['vertices']),replayVertices=len(expected['vertices']),faces=len(real['triangles'])//3,maxVertexError=maximum,maxMatrixError=matrix_delta,orientedTriangleMultisetMatches=triangles_match))
  expected.pop('sourceVertexIds',None);expected.update(real)
  m=real['nativeBodyFromMesh']
  points=[[sum(m[c*4+r]*p[c] for c in range(3))+m[12+r] for r in range(3)] for p in real['vertices']]
  expected['bodyBoundsMin']=[min(p[a] for p in points) for a in range(3)];expected['bodyBoundsMax']=[max(p[a] for p in points) for a in range(3)]
 result['actualUnityLmsReport']=str(args.native_report.resolve());result['actualUnityLmsReportSHA256']=digest(args.native_report)
 result['actualUnityLmsValidated']=True
 result['limitations']=native['limitations']+' Actual exported vertices/triangle order/matrices supersede replay data; source/hash provenance retained.'
 report=dict(passed=True,allReplayPartsEquivalent=all(p['orientedTriangleMultisetMatches'] and p['maxVertexError']<eps for p in comparisons),parts=comparisons,
  comparison='Cylinder/cocks orientedtriangle coordinate multisets agree within3micrometres OBJ float parsing. Guide differences expose an old replay fidelity gap; actual Unity output is authoritative for every part. The exact guide-replay discrepancy is not isolated.',nativeReport=str(args.native_report.resolve()))
 (OUT/'lms-native-comparison.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
(OUT/'wals-static.json').write_text(json.dumps(result,separators=(',',':')),encoding='utf-8')
print(json.dumps(dict(output=str(OUT/'wals-static.json'),lmsVersion=result['lmsVersion'],gameVersion=result['generatedGameVersion'],static=tdata(static),parts=[dict(name=p['name'],vertices=len(p['vertices']),faces=len(p['triangles'])//3,bodyBoundsMin=p['bodyBoundsMin'],bodyBoundsMax=p['bodyBoundsMax']) for p in parts]),indent=2))
