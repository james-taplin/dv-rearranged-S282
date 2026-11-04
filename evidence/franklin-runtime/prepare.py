"""Extract the production mechanism configuration, without launching Unity.

The generated private fixture uses these method bodies verbatim, with only
asset-provider identifiers substituted to avoid MeshFinder's exporter process.
"""
from pathlib import Path
import argparse
import hashlib
import json
import re

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'build/franklin-runtime'


def member(text, declaration):
    start = text.index(declaration)
    brace = text.index('{', start)
    level = 1
    end = brace + 1
    while level:
        level += (text[end] == '{') - (text[end] == '}')
        end += 1
    return text[start:end]


def prepare(output_directory=OUT):
    methods = []
    provenance = {}
    for filename, declarations in {
        'WheelRearranger.Articulated.cs': [
            'private static Transform NewArticulatedRoot(',
            'private Mesh CreateArticulatedSideRod(',
            'private void ConfigureEngineDrive(',
            'private static Transform RequireArticulatedPart(',
            'private static void SetPartActive(',
            'private static void EnableGeneratedRenderers(',
            'private void ConfigureNativeArticulation(int id, float pinZ)',
            'private void MoveArticulatedPart(',
            'private sealed class MovedEnginePart'],
        'WheelRearranger.Triplex.cs': [
            'private sealed class TriplexLayout',
            'private TriplexLayout MeasureTriplex(',
            'private float FranklinCylinderFront(',
            'private void ConfigureSingleDriverDrive(',
            'private void ConfigureTriplexDrive(',
            'private void ConfigureTriplexMiddleArticulation('],
    }.items():
        text = (ROOT/'RearrangedS282'/filename).read_text(encoding='utf-8-sig')
        for declaration in declarations:
            body = member(text, declaration)
            provenance[filename + ':' + declaration] = hashlib.sha256(body.encode()).hexdigest().upper()
            body = body.replace('MeshFinder.Instance.DuplexMainRodMesh', 'FixtureAssets.ShortMainRod')
            body = body.replace('MeshFinder.Instance.TwoAxleSideRodMesh', 'FixtureAssets.TwoAxleSideRod')
            body = body.replace('AssetBundleLoader.FranklinBAnimCtrl', 'FixtureAssets.Controller')
            methods.append(body)
    output_directory.mkdir(parents=True, exist_ok=True)
    triplex_source = (ROOT/'RearrangedS282/WheelRearranger.Triplex.cs').read_text(encoding='utf-8-sig')
    swing_literal = re.search(r'\bTriplexMiddleSwing\s*=\s*([^,;]+)', triplex_source).group(1)
    provenance['WheelRearranger.Triplex.cs:TriplexMiddleSwing'] = swing_literal
    configure_sequence = '''ConfigureTriplexMiddleArticulation();
            ConfigureNativeArticulation(twoCoupled ? ArticulatedSimulation.SixAxleTriplexId : trucks ? ArticulatedSimulation.TriplexId : ArticulatedSimulation.TrucklessTriplexId, triplex.FrontPinZ);
            // The front engine's hinge rides on the middle engine.
            MoveArticulatedPart(articulatedPin, articulatedMiddleRoot);'''
    if configure_sequence not in triplex_source:
        raise ValueError('Production triplex constraint/reparent sequence changed; review the fixture binding')
    provenance['WheelRearranger.Triplex.cs:constraintCallSequence'] = hashlib.sha256(configure_sequence.encode()).hexdigest().upper()
    header = '''using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using Object = UnityEngine.Object;
internal sealed class FranklinConfiguration {
 const float TriplexShortenNative=.45f, TriplexFrontExtra=.25f, TrucklessFrontExtra=.6f, TrucklessEngineGap=.25f, TriplexEngineGap=.08f, MalletTrailingRadius=.355f, MalletEngineGap=.1f;
 const float TriplexMiddleSwing=PRODUCTION_SWING_LITERAL;
 static readonly Vector3 TriplexOwnTrailingLimit=new Vector3(0,.36f,-3.7f);
 readonly List<Mesh> articulatedMeshes=new List<Mesh>();
 readonly List<Transform> articulatedAssets=new List<Transform>();
 Mesh articulatedSideRod,blindDriver,flangedDriver;
 Transform driveL,secondLeftDriveWheel,rearBogie,MalletBody;
 Transform transform,articulatedFrontFrame,articulatedFrontRoot,articulatedRearRoot,articulatedPin;
 Transform articulatedMiddleFrame,articulatedMiddleRoot,triplexMiddlePin,triplexAimRest;
 PositionConstraint articulatedPosition,triplexMiddlePosition;
 AimConstraint articulatedAim,triplexMiddleAim;
 readonly List<MovedEnginePart> articulatedMoved=new List<MovedEnginePart>();
 Transform[] frontFranklin;
 Vector3[] triplexCoupledDriverPositions;
 TriplexLayout triplex;
 static Matrix4x4 EngineMap(float offset,float scale)=>Matrix4x4.TRS(new Vector3(0,0,offset),Quaternion.identity,new Vector3(1,scale,scale));
 public FranklinConfiguration(Transform left,Transform franklin,Transform body,Mesh blind,Mesh flange){
  driveL=left;secondLeftDriveWheel=left.Find("s282_wheels_driving_2");frontFranklin=new[]{franklin};MalletBody=body;transform=body;
  rearBogie=new GameObject("native Axle_R datum").transform;rearBogie.SetParent(body,false);rearBogie.localPosition=new Vector3(0,0,3.2f);
  blindDriver=blind;flangedDriver=flange;
  articulatedSideRod=CreateArticulatedSideRod();
  triplexCoupledDriverPositions=new[]{left.Find("s282_wheels_driving_1").localPosition,secondLeftDriveWheel.localPosition};
 }
 public void Duplex(Transform drive,bool rear)=>ConfigureEngineDrive(drive,FixtureAssets.ShortMainRod,rear);
 public float[] Triplex(bool trucks,bool two){triplex=MeasureTriplex(trucks,two);return new[]{triplex.Scale,triplex.Offsets[0],triplex.Offsets[1],triplex.Offsets[2],triplex.FrontPinZ,triplex.MiddlePinZ,triplex.Shorten};}
 public void TriplexDrive(Transform drive,bool clone)=>ConfigureTriplexDrive(drive,clone);
 // Constraint setup below is extracted verbatim. Only the unrelated full-frame
 // mesh splitting callback is omitted: this remains a mechanism-only fixture.
 private void BindSplitArticulatedFrame(int id,Vector3 worldPin) { }
 public Transform Support {get;private set;}
 public Transform FrontHinge=>articulatedPin;
 public Transform MiddleHinge=>triplexMiddlePin;
 public Transform MiddleRestTarget=>triplexAimRest;
 public PositionConstraint FrontPosition=>articulatedPosition;
 public PositionConstraint MiddlePosition=>triplexMiddlePosition;
 public AimConstraint FrontAim=>articulatedAim;
 public AimConstraint MiddleAim=>triplexMiddleAim;
 public float MiddleSourceWeight=>TriplexMiddleSwing;
 public void ConfigureTriplexConstraints(Transform frontFrame,Transform frontRoot,
  Transform middleFrame,Transform middleRoot,Transform rearRoot,Vector3 supportRest) {
  articulatedFrontFrame=frontFrame;articulatedFrontRoot=frontRoot;articulatedRearRoot=rearRoot;
  articulatedMiddleFrame=middleFrame;articulatedMiddleRoot=middleRoot;
  Support=NewArticulatedRoot("Axle_F",transform);Support.localPosition=supportRest;
  articulatedPin=NewArticulatedRoot("[fixture front articulation pin]",transform);
  triplexMiddlePin=NewArticulatedRoot("[fixture middle articulation pin]",transform);
  triplexAimRest=NewArticulatedRoot("[fixture middle aim rest]",transform);
  articulatedPosition=articulatedFrontFrame.gameObject.AddComponent<PositionConstraint>();
  articulatedAim=articulatedFrontFrame.gameObject.AddComponent<AimConstraint>();
  triplexMiddlePosition=articulatedMiddleFrame.gameObject.AddComponent<PositionConstraint>();
  triplexMiddleAim=articulatedMiddleFrame.gameObject.AddComponent<AimConstraint>();
  // Same production ordering; IDs are only consumed by the omitted mesh split.
  ConfigureTriplexMiddleArticulation();
  ConfigureNativeArticulation(0,triplex.FrontPinZ);
  MoveArticulatedPart(articulatedPin,articulatedMiddleRoot);
 }
'''
    (output_directory/'ProductionConfiguration.cs').write_text(header.replace('PRODUCTION_SWING_LITERAL',swing_literal)+'\n'.join(methods)+'\n}\n',encoding='utf-8')
    wals_methods=[]
    for filename,declarations in {
        'WheelRearranger.Mallet.cs':['private sealed class MalletLayout','private float MeasureNativeCylinderFront(', 'private MalletLayout MeasureMallet(', 'private void ConfigureMalletDrive('],
        'WheelRearranger.BigMallet.cs':['private sealed class MalletSpec','private static MalletSpec SpecFor(', 'private sealed class BigMalletLayout','private BigMalletLayout MeasureBigMallet(', 'private void ConfigureLongEngine('],
        'WheelRearranger.Articulated.cs':['private static Transform RequireArticulatedPart(', 'private static void SetPartActive('],
        'WheelRearranger.MalletDriverPitch.cs':['private void HoldMalletDriverPitch('],
    }.items():
        source=(ROOT/'RearrangedS282'/filename).read_text(encoding='utf-8-sig')
        for declaration in declarations:
            body=member(source,declaration)
            provenance[filename+':'+declaration]=hashlib.sha256(body.encode()).hexdigest().upper()
            wals_methods.append(body.replace('MeshFinder.Instance.ThreeAxleSideRodMesh','FixtureAssets.ThreeAxleSideRod'))
    enum=member((ROOT/'RearrangedS282/WheelArrangement.cs').read_text(encoding='utf-8-sig'),'public enum WheelArrangementType')
    mallet_source=(ROOT/'RearrangedS282/WheelRearranger.Mallet.cs').read_text(encoding='utf-8-sig')
    gap_match=re.search(r'private const float MalletCylinderClearanceGap\s*=\s*([^;]+);',mallet_source)
    gap_literal=gap_match.group(1) if gap_match else '.1f'
    provenance['WheelRearranger.Mallet.cs:MalletCylinderClearanceGap']=gap_literal
    wals_header='''using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
'''+enum+'''
internal sealed class WalsConfiguration {
 const float MalletScale=.75f,MalletEngineGap=.1f,MalletTrailingRadius=.355f;
 const float MalletCylinderClearanceGap=PRODUCTION_GAP_LITERAL;
 public const float ProductionGap=MalletCylinderClearanceGap;
 static readonly Vector3 TriplexOwnTrailingLimit=new Vector3(0,.36f,-3.7f);
 Transform driveL,firstLeftDriveWheel,secondLeftDriveWheel,thirdLeftDriveWheel,fourthLeftDriveWheel,fifthLeftDriveWheel,lfCylinder,MalletBody,rearBogie,leftFiveAxleSideRod,rightFiveAxleSideRod;
 Mesh flangedDriver,blindDriver,blindDriverBigWeight;
 readonly List<(Transform wheel,Transform anchor,PositionConstraint constraint)> malletDriverHolds=new List<(Transform,Transform,PositionConstraint)>();
 static Matrix4x4 EngineMap(float offset,float scale)=>Matrix4x4.TRS(new Vector3(0,0,offset),Quaternion.identity,new Vector3(1,scale,scale));
 float NativeDriverZ(int n)=>new[]{firstLeftDriveWheel,secondLeftDriveWheel,thirdLeftDriveWheel,fourthLeftDriveWheel,fifthLeftDriveWheel}[n-1].localPosition.z;
 public WalsConfiguration(Transform left,Transform cylinder,Transform body){
  driveL=left;lfCylinder=cylinder;MalletBody=body;
  firstLeftDriveWheel=left.Find("s282_wheels_driving_1");secondLeftDriveWheel=left.Find("s282_wheels_driving_2");thirdLeftDriveWheel=left.Find("s282_wheels_driving_3");fourthLeftDriveWheel=left.Find("s282_wheels_driving_4");fifthLeftDriveWheel=left.Find("s282_wheels_driving_5");
  flangedDriver=firstLeftDriveWheel.GetComponent<MeshFilter>().sharedMesh;blindDriver=secondLeftDriveWheel.GetComponent<MeshFilter>().sharedMesh;blindDriverBigWeight=thirdLeftDriveWheel.GetComponent<MeshFilter>().sharedMesh;
  rearBogie=new GameObject("native rear bogie datum").transform;rearBogie.SetParent(body,false);rearBogie.localPosition=new Vector3(0,0,3.2f);
 }
 public float[] Measure(int id){if(id==10)return new[]{1f,0f,0f,0f,4f,0f,MeasureNativeCylinderFront(),0};if(id==37){var m=MeasureMallet();float delta=float.IsNaN(FixtureAssets.WalsMalletGap)?0:FixtureAssets.WalsMalletGap-MalletCylinderClearanceGap;m.RearOffset-=delta;m.RearCylinderFront-=delta;m.RearLastDriver-=delta;m.PinZ-=delta*.5f;m.TrailingZ-=delta;return new[]{MalletScale,m.FrontOffset,m.RearOffset,m.PinZ,3f,3f,MeasureNativeCylinderFront(),m.TrailingZ};}var b=MeasureBigMallet((WheelArrangementType)id);return new[]{b.Scale,b.FrontOffset,b.RearOffset,b.PinZ,(float)b.Spec.Front,(float)b.Spec.Rear,MeasureNativeCylinderFront(),b.TrailingZ};}
 public void Configure(Transform drive,int coupled,bool clone){
  leftFiveAxleSideRod=rightFiveAxleSideRod=drive.Find("s282_mech_wheels_connect/[Rearranged five-axle segment]");
  if(coupled==3){ConfigureMalletDrive(drive,clone);HoldMalletDriverPitch(drive);}else ConfigureLongEngine(drive,coupled,clone);
 }
'''
    (output_directory/'ProductionWalsConfiguration.cs').write_text(wals_header.replace('PRODUCTION_GAP_LITERAL',gap_literal)+'\n'.join(wals_methods)+'\n}\n',encoding='utf-8')
    mesh_source=(ROOT/'RearrangedS282/MeshFinder.cs').read_text(encoding='utf-8-sig')
    rod_members=[]
    for declaration in ['readonly struct Range','private static readonly Range[] verticesToMoveBackward','private static readonly Range[] verticesToMoveForward325','private static readonly Range[] verticesToMoveForward015','private static readonly Range[] facesToHideFor4Coupled','private static readonly Range[] facesToHideFor6Coupled','private void getSideRodMeshes(']:
        body=member(mesh_source,declaration)
        provenance['MeshFinder.cs:'+declaration]=hashlib.sha256(body.encode()).hexdigest().upper()
        if declaration.startswith('private static readonly'):body+=';'
        body=body.replace('generateMeshFromGameFiles(sideRodMeshName);','// Native extraction is deliberately excluded in this private fixture.')
        body=body.replace('new OBJLoader().Load(', 'FixtureAssets.LoadObj(')
        rod_members.append(body)
    rod_header='''using System;
using System.Linq;
using UnityEngine;
internal sealed class ProductionRodRecipes {
 readonly string exportPathFull;
 const string sideRodMeshName="s282_mech_wheels_connect";
 internal Mesh NativeSideRodMesh,TwoAxleSideRodMesh,ThreeAxleSideRodMesh,FiveSixAxleSideRodMesh;
 internal ProductionRodRecipes(string assets){exportPathFull=assets;getSideRodMeshes();}
'''
    (output_directory/'ProductionRodRecipes.cs').write_text(rod_header+'\n'.join(rod_members)+'\n}\n',encoding='utf-8')
    (output_directory/'configuration-provenance.json').write_text(json.dumps(provenance,indent=2),encoding='utf-8')
    print(json.dumps({'generated':str(output_directory/'ProductionConfiguration.cs'),'exactMethodBodies':len(methods)}))


if __name__ == '__main__':
    parser=argparse.ArgumentParser()
    parser.add_argument('--output-directory',type=Path,default=OUT)
    prepare(parser.parse_args().output_directory)
