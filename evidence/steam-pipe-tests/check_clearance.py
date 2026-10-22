"""Local, non-coplanar triangle crossing audit; not a Unity collision test.

Uses actual pipe builder exports, LMS OBJ body, reservoir and native clips.
Record intersections for review; do not mistake AABB overlap for clipping.
"""
import sys,json,re
from pathlib import Path
import numpy as np

root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'evidence/cylinder-casing-tests'))
from extract_native import obj
out=Path(sys.argv[1]); data=json.loads((out/'pipe-meshes.json').read_text())
native=json.loads((root/'evidence/cylinder-casing-tests/native-full-casing-fittings.json').read_text())
clearance=json.loads((root/'evidence/wheel-clearance-tests/native-clearance.json').read_text())
motion=json.loads((root/'build/geometry-audit/connecting/native-motion-meshes.json').read_text())
animation=json.loads((root/'evidence/pacific-hudson-valve-gear/native-animation.json').read_text())
capture=json.loads(Path(r'B:\SteamLibrary\steamapps\common\Derail Valley\Mods\RearrangedS282\stage2-runtime-checks.json').read_text(encoding='utf-8-sig'))
records={a['id']:a for a in capture['arrangements']}

def triangles(vertices,indices):
    return np.asarray(vertices,dtype=float)[np.asarray(indices,dtype=int).reshape(-1,3)]

# Native body fragments which the mod relocates must not be tested at their old
# location. The actual reservoirs are tested separately with the legacy offset.
v,f=obj(Path(r'B:\SteamLibrary\steamapps\common\Derail Valley\Mods\LocoMeshSplitter\assets\s282_locomotive_body.obj'))
v=np.asarray(v,dtype=float);f=np.asarray(f,dtype=int)
ref=(root/'evidence/cylinder-casing-tests/LMS_LOD0.reference.txt').read_text(encoding='utf-8-sig')
ranges={n:(float(lo.rstrip('f')),float(hi.rstrip('f'))) for n,lo,hi in re.findall(r'RangeFloat\s+(\w+)\s*=\s*new MeshSplittersUtil.RangeFloat\(([-.\df]+),\s*([-.\df]+)\)',ref)}
exclude=np.zeros(len(f),dtype=bool);air_parts=[]
for name,body in re.findall(r'private static Mesh (get\w+Mesh)\([^)]*\)\s*\{(.*?)\n\t\}',ref,re.S):
    if not any(t in name for t in ['Cylinder','DryPipe','CrossheadSupport','LiftingArmSupport','Brake','Suspension','AirTank','OilLines','Lubricator','AirPump','Frame','Subframe']):continue
    selected=np.zeros(len(f),dtype=bool)
    for box in re.findall(r'markPartOfMesh\(val.vertices,\s*array\d*,\s*(\w+),\s*(\w+),\s*(\w+)\)',body):
        mask=np.ones(len(v),dtype=bool)
        for a,n in enumerate(box):
            lo,hi=ranges[n];mask&=(v[:,a]>=lo)&(v[:,a]<=hi)
        selected|=mask[f].all(axis=1)
        if 'AirTank' in name:
            mask=np.ones(len(v),dtype=bool)
            for a,n in enumerate(box):
                lo,hi=ranges[n];axis=-v[:,a] if a==0 else v[:,a];mask&=(axis>=lo)&(axis<=hi)
            selected|=mask[f].all(axis=1)
    exclude|=selected
    if 'AirPump' in name and 'Pipe' in name and selected.any():air_parts.append((name,selected))
v[:,2]+=4.88
fixed=triangles(v,f[~exclude])
air_parts=[(name,triangles(v,f[mask])) for name,mask in air_parts]
tank=clearance['tanks'][0]
tv=np.asarray(tank['vertices']);tf=np.asarray(tank['triangles'])
# LMS authors only one reservoir mesh and mirrors its transform for the other.
if tv[:,0].min()*tv[:,0].max()>0:
    mirror=tv.copy();mirror[:,0]*=-1; tanks=np.concatenate([triangles(tv,tf),triangles(mirror,tf)])
else:tanks=triangles(tv,tf)

def segment_hits(start,end,tri):
    direction=end-start;e1=tri[:,1]-tri[:,0];e2=tri[:,2]-tri[:,0]
    h=np.cross(direction,e2);det=(e1*h).sum(axis=1)
    good=np.abs(det)>1e-10;inv=np.zeros_like(det);inv[good]=1/det[good]
    s=start-tri[:,0];u=inv*(s*h).sum(axis=1);q=np.cross(s,e1)
    b=inv*(direction*q).sum(axis=1);t=inv*(e2*q).sum(axis=1)
    # Surface crossings only; common endpoints/edges are not called defects.
    good&=(u>1e-7)&(b>1e-7)&(u+b<1-1e-7)&(t>1e-7)&(t<1-1e-7)
    return start[good]+direction[good]*t[good,None]

def crossings(a,b):
    amin=a.min(axis=1);amax=a.max(axis=1);bmin=b.min(axis=1);bmax=b.max(axis=1)
    use=((bmax>=amin.min(axis=0)-1e-6)&(bmin<=amax.max(axis=0)+1e-6)).all(axis=1)
    b=b[use];bmin=bmin[use];bmax=bmax[use]
    if not len(b):return np.empty((0,3))
    result=[]
    for tri,lo,hi in zip(a,amin,amax):
        candidates=b[((bmax>=lo-1e-6)&(bmin<=hi+1e-6)).all(axis=1)]
        if not len(candidates):continue
        for i,j in [(0,1),(1,2),(2,0)]:
            starts=np.repeat(tri[i,None],len(candidates),axis=0);ends=np.repeat(tri[j,None],len(candidates),axis=0)
            hits=segment_hits(starts,ends,candidates)
            if len(hits):result.extend(hits)
            hits=segment_hits(candidates[:,i],candidates[:,j],np.repeat(tri[None],len(candidates),axis=0))
            if len(hits):result.extend(hits)
    return np.unique(np.round(result,5),axis=0).reshape(-1,3) if len(result) else np.empty((0,3))

def matrix(p,q,s):
    x,y,z,w=q
    m=np.eye(4);m[:3,:3]=np.array([[1-2*y*y-2*z*z,2*x*y-2*z*w,2*x*z+2*y*w],[2*x*y+2*z*w,1-2*x*x-2*z*z,2*y*z-2*x*w],[2*x*z-2*y*w,2*y*z+2*x*w,1-2*x*x-2*y*y]])@np.diag(s);m[:3,3]=p
    return m

poses=[]
for clip in animation['clips']:
    for frame in clip['frames']:
        local={}
        for t in animation['transforms']:
            path=t['path'].split('/LocoS282A_Drivetrain R',1)[1].lstrip('/')
            local[path]=[t['position'],t['rotation'],t['scale']]
        for b in clip['bindings']:
            value=[frame['values'].get(str(i),fallback) for i,fallback in zip(b['indices'],b['first'])]
            local[b['path']][{1:0,2:1,3:2}[b['attribute']]]=value
        world={'':np.eye(4)}
        for path in sorted(local,key=lambda p:p.count('/')):
            if not path:continue
            world[path]=world[path.rsplit('/',1)[0] if '/' in path else '']@matrix(*local[path])
        meshes=[]
        # These rigid cylinder-side pieces follow the same translation as their
        # static cylinder/guide, including precision-retargeted arrangements.
        for path in ['s282_mech_push_rod','s282_mech_cylinder_rod','s282_mech_push_rod/s282_mech_push_rod_to_cutoff_base','s282_mech_cutoff_rod_hinge','s282_mech_cutoff_rod_hinge/s282_mech_push_rod_to_cutoff_rod']:
            name=path.rsplit('/',1)[-1]
            if name not in motion['meshes'] or path not in world:continue
            mesh=motion['meshes'][name];verts=np.c_[mesh['vertices'],np.ones(len(mesh['vertices']))]@world[path].T
            meshes.append((name,triangles(verts[:,:3],mesh['triangles'])))
        poses.append((clip['name'],frame['time'],meshes))

case_results=[];total_checks=0
for case in data['cases']:
    id=case['id'];sockets=np.asarray(case['sockets']);modified=id in [0,1,2,3,4,5,34,35,44]
    obstacles=[('fixed native body excluding relocated fragments',fixed)]
    current_tank=tanks.copy();current_tank[:,:,2]-=.1 if modified else 0
    obstacles.append(('reservoirs',current_tank))
    for label,original in air_parts:
        part=original.copy()
        if modified and 'OutputPipe' in label:part[:,:,2]=(part[:,:,2]-4.88)*1.12+4.88-.58
        if modified and 'InputPipe' in label:part[:,:,2]+=.07
        obstacles.append((label,part))
    result=[]
    pipes=[(mesh,triangles(mesh['vertices'],mesh['triangles'])) for mesh in case['meshes'] if 'original boiler fitting' not in mesh['name']]
    boiler=np.asarray(case['boilers'])
    for mesh,pipe in pipes:
        for label,obstacle in obstacles:
            hit=crossings(pipe,obstacle);total_checks+=1
            # A native inclined boiler seat deliberately intersects the retained
            # collar. Keep this small joint neighbourhood separate from a pipe
            # crossing the running board farther along its route.
            if len(hit):hit=hit[np.linalg.norm(hit[:,None,:]-boiler[None,:,:],axis=2).min(axis=1)>.15]
            if len(hit):result.append(dict(pipe=mesh['name'],obstacle=label,count=len(hit),points=hit[:12].tolist()))
    # Recover cylinder-side frame from the actual recorded socket and native
    # dimensions; Franklin gear uses different authored assets and is excluded.
    if id not in [31,32,38,40]:
        if id in [0,1,2,35]:scale=1.42
        elif id in [3,4,5,34]:scale=1.2879
        elif id in [12,13,14,44]:scale=1.11
        elif 27<=id<=30:scale=.84
        else:scale=records[id]['converted']['forceRadius']/.712
        full=id in [0,1,2,3,4,5,12,13,14,27,28,29,30,34,35,44]
        for socket in sockets:
            side=1 if socket[0]>=0 else -1
            # Native top socket / lower dry-pipe seat is approximately z9.49695.
            offset=socket[2]-9.49695-(scale-1)*8.84 if full else socket[2]-scale*9.49695
            if not data['correctedSockets'] and (id in [3,34] or 27<=id<=30):
                # The old defect is precisely that the cylinder socket does not
                # share the gear's frame. Do not infer that frame from the socket.
                offset=1.1 if 27<=id<=30 else -2.1
            mapping=matrix([0,0,offset],[0,0,0,1],[side,scale,scale])
            for clip,time,parts in poses:
                for name,part in parts:
                    mapped=(np.c_[part.reshape(-1,3),np.ones(part.size//3)]@mapping.T)[:,:3].reshape(-1,3,3)
                    for mesh,pipe in pipes:
                        # Fast whole-envelope rejection precedes triangle work.
                        if not ((pipe.max((0,1))>=mapped.min((0,1)))&(mapped.max((0,1))>=pipe.min((0,1)))).all():continue
                        hit=crossings(pipe,mapped);total_checks+=1
                        if len(hit):result.append(dict(pipe=mesh['name'],obstacle=name,clip=clip,time=time,count=len(hit),points=hit[:12].tolist()))
    case_results.append(dict(id=id,name=case['name'],crossings=result))
    print(case['name']+': '+str(len(result))+' crossing groups',flush=True)
report=dict(status='CONTACTS_REQUIRE_REVIEW' if any(c['crossings'] for c in case_results) else 'NO_CROSSINGS_FOUND_WITH_LIMITS',
    correctedSockets=data['correctedSockets'],cases=len(case_results),trianglePairQueries=total_checks,arrangementsWithCrossings=sum(bool(c['crossings']) for c in case_results),results=case_results,
    limitations=['Recorded 1.22.0 neutral sockets'+(' plus recomputed new rigid sockets' if data['correctedSockets'] else '')+'; actual Unity scene not executed','Non-coplanar surface crossings only; does not prove separation of nested solids or a minimum clearance','Relocated body fragments removed, reservoirs and native air pipes tested separately; retained native boiler collar seating within 0.15m of each boiler attachment excluded','Native sampled cylinder-side crosshead/valve/piston pieces only; custom Franklin gear, stretched main/radius rods and full articulation/body movement require live inspection','Stock has native pipes, giant prototype and zero-driver novelty have no generated connections; Erie rear exhaust covered by its separate native clearance test'])
(out/'clearance.json').write_text(json.dumps(report,indent=2))
print(json.dumps({k:v for k,v in report.items() if k not in ['results']},indent=2))
