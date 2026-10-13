"""Authenticate serialized waves, normal start/ending and nested prefab candidates."""
from pathlib import Path
from datetime import datetime, timezone
import collections
import hashlib
import json
import re

project = Path(__file__).resolve().parents[2]
assets = project / 'Assets'
scene = assets / 'Scenes/TurretGame.unity'
text = scene.read_text(encoding='utf-8')
documents = {match[2]: dict(type=int(match[1]), stripped=bool(match[3]), text=match[4]) for match in re.finditer(
    r'^--- !u!(\d+) &(-?\d+)( stripped)?\n(.*?)(?=^--- !u!|\Z)', text, re.M | re.S)}
paths = {}
for meta in assets.rglob('*.meta'):
    identity = re.search(r'^guid: (\w+)', meta.read_text(encoding='utf-8-sig', errors='replace'), re.M)
    if identity:
        paths[identity[1]] = meta.with_suffix('')

def record(path):
    return dict(path=str(path.relative_to(project)), sha256=hashlib.sha256(path.read_bytes()).hexdigest())

def field(data, name):
    found = re.search(r'^  ' + re.escape(name) + r': (.*)$', data, re.M)
    return found[1] if found else None

def hierarchy(game_object):
    chain, visited = [], set()
    while game_object and game_object != '0' and game_object not in visited:
        visited.add(game_object)
        obj = documents.get(game_object)
        if not obj or obj['stripped']:
            chain.append(dict(gameObjectFileID=game_object, unresolvedOrStripped=True))
            break
        transform = next((value for value in documents.values()
                          if value['type'] in (4, 224) and field(value['text'], 'm_GameObject') == '{fileID: ' + game_object + '}'), None)
        chain.append(dict(gameObjectFileID=game_object, name=field(obj['text'], 'm_Name'),
                          serializedActive=field(obj['text'], 'm_IsActive'),
                          localPosition=field(transform['text'], 'm_LocalPosition') if transform else None))
        parent_ref = field(transform['text'], 'm_Father') if transform else None
        parent = re.search(r'fileID: (-?\d+)', parent_ref)[1] if parent_ref else '0'
        if parent == '0':
            break
        parent_doc = documents.get(parent)
        if not parent_doc or parent_doc['stripped']:
            chain.append(dict(parentTransformFileID=parent, unresolvedOrStripped=True))
            break
        game_object = re.search(r'fileID: (-?\d+)', field(parent_doc['text'], 'm_GameObject'))[1]
    return chain

spawner = next(value for value in documents.values() if '\n  waves:\n' in value['text'] and '\n  finalWaveNumber:' in value['text'])
spawner_text = spawner['text']
wave_block = spawner_text.split('  waves:\n', 1)[1].split('  timeBetweenWaves:', 1)[0]
waves = []
time = 0.0
between = float(field(spawner_text, 'timeBetweenWaves'))
for index, block in enumerate(re.split(r'^  - waveTime: ', wave_block, flags=re.M)[1:], 1):
    duration = float(block.splitlines()[0])
    extra = float(re.search(r'^    extraDelayAfterWave: ([0-9.eE+-]+)', block, re.M)[1])
    intermission = between + extra
    time += intermission
    guids = re.findall(r'guid: (\w+)', block)
    waves.append(dict(number=index, serializedDuration=duration, serializedSpawnRate=float(re.search(r'^    spawnRate: (.*)', block, re.M)[1]),
                      serializedMaxEnemies=int(re.search(r'^    maxEnemies: (.*)', block, re.M)[1]),
                      precedingIntermission=intermission, estimatedStartFromOrdinaryGrabNotRuntimeProof=time,
                      estimatedEndFromOrdinaryGrabNotRuntimeProof=time+duration,
                      prefabGUIDs=guids, prefabPaths=[str(paths[identity].relative_to(project)) if identity in paths else None for identity in guids]))
    time += duration
delay = float(field(spawner_text, 'endDialogueStartDelay'))
dialogue = float(field(spawner_text, 'endDialogueDuration'))
timeline_object = re.search(r'fileID: (-?\d+)', field(spawner_text, 'endSequenceTimelineObject'))[1]
director = next(value for value in documents.values() if value['type'] == 320 and field(value['text'], 'm_GameObject') == '{fileID: ' + timeline_object + '}')
timeline_guid = re.search(r'guid: (\w+)', field(director['text'], 'm_PlayableAsset'))[1]
timeline = paths[timeline_guid]
timeline_text = timeline.read_text(encoding='utf-8')
signals = []
for doc in re.split(r'^--- !u!\d+ &', timeline_text, flags=re.M)[1:]:
    stamp = re.search(r'^  m_Time: ([0-9.eE+-]+)', doc, re.M)
    asset = re.search(r'^  m_Asset: \{fileID: 11400000, guid: (\w+)', doc, re.M)
    if stamp and asset:
        signals.append(dict(time=float(stamp[1]), guid=asset[1],
                            path=str(paths[asset[1]].relative_to(project)) if asset[1] in paths else None,
                            estimatedFromOrdinaryGrabNotRuntimeProof=time + delay + dialogue + float(stamp[1])))
sources = [path for path in (assets / 'Scripts/Scripts').rglob('*.cs')]
sources += [path for path in (assets / 'Scripts').glob('*.cs') if any(token in path.name for token in ('Drone', 'Lazer', 'Projectile', 'Meteor', 'Power', 'Spawn'))]
runtime_candidates = []
for path in sorted(sources):
    for number, line in enumerate(path.read_text(encoding='utf-8-sig').splitlines(), 1):
        if any(token in line for token in ('Instantiate(', 'GetPooledObject(', 'Resources.Load', 'Addressable', '.SetColor(', '.SetFloat(')):
            runtime_candidates.append(dict(path=str(path.relative_to(project)), line=number, source=line.strip()))
# Serialized dependency closure is an over-approximation, not observed spawn reachability.
allowed = {'.unity', '.prefab', '.asset', '.playable', '.controller'}
pending, visited, prefab_inventory = [scene], set(), []
while pending:
    path = pending.pop()
    if path in visited or not path.is_file() or path.suffix not in allowed:
        continue
    visited.add(path)
    data = path.read_text(encoding='utf-8-sig', errors='replace')
    for identity in set(re.findall(r'guid: (\w+)', data)):
        if identity in paths and paths[identity].suffix in allowed:
            pending.append(paths[identity])
    if path.suffix == '.prefab':
        counts = collections.Counter(int(value) for value in re.findall(r'^--- !u!(\d+) &', data, re.M))
        scripts = set(re.findall(r'm_Script: \{fileID: 11500000, guid: (\w+)', data))
        materials = []
        for identity in set(re.findall(r'guid: (\w+)', data)):
            material = paths.get(identity)
            if not material or material.suffix != '.mat':
                continue
            material_text = material.read_text(encoding='utf-8-sig', errors='replace')
            shader = re.search(r'^  m_Shader: (.*)$', material_text, re.M)
            shader_guid = re.search(r'guid: (\w+)', shader[1]) if shader else None
            shader_path = paths.get(shader_guid[1]) if shader_guid else None
            materials.append(dict(record(material), guid=identity, shaderReference=shader[1] if shader else None,
                                  localShader=record(shader_path) if shader_path and shader_path.is_file() else None,
                                  limitation='Direct material references only; imported FBX/embedded materials and runtime replacements require actual import/component readback.'))
        prefab_inventory.append(dict(record(path), particles=counts[198], particleRenderers=counts[199],
                                     meshRenderers=counts[23], skinnedRenderers=counts[137], trails=counts[96], lines=counts[120],
                                     directlyReferencedMaterials=materials,
                                     scriptPaths=[str(paths[item].relative_to(project)) if item in paths else 'unresolved:' + item for item in sorted(scripts)]))
repaired_references = {}
for identity, label in [('ec1683be71e02f548b9023d92d51ac31', 'CFX_SpawnSystem'), ('c8b80b87aee9a664b95b0dc6916819b4', 'WaterReflection')]:
    matches = []
    for path in assets.rglob('*'):
        if path.is_file() and path.suffix in allowed and identity in path.read_text(encoding='utf-8-sig', errors='replace'):
            matches.append(str(path.relative_to(project)))
    repaired_references[label] = dict(scriptGUID=identity, localSerializedReferences=matches,
                                     limitation='No local serialized reference does not prove runtime-unreachable; compile repair is separate from active render/effect coverage.')
report = dict(utc=datetime.now(timezone.utc).isoformat(), project=str(project), offlineOnly=True, runtimeVerified=False,
              scene=record(scene), spawnerHierarchy=hierarchy(re.search(r'fileID: (-?\d+)', field(spawner_text, 'm_GameObject'))[1]),
              serializedWaveMode=field(spawner_text, 'waveMode'), waves=waves,
              finalWaveNumber=field(spawner_text, 'finalWaveNumber'),
              scatterRange=[field(spawner_text, 'scatterShotStartWave'), field(spawner_text, 'scatterShotEndWave')],
              fullAutoRange=[field(spawner_text, 'fullAutoStartWave'), field(spawner_text, 'fullAutoEndWave')],
              estimatedWaveCompletionNotRuntimeDuration=time, endDialogueStartDelay=delay, endDialogueDuration=dialogue,
              estimatedEndTimelineActivationNotRuntimeProof=time+delay+dialogue,
              endTimeline=dict(record(timeline), guid=timeline_guid, hierarchy=hierarchy(timeline_object),
                               initialState=field(director['text'], 'm_InitialState'), wrapMode=field(director['text'], 'm_WrapMode'),
                               updateMode=field(director['text'], 'm_DirectorUpdateMode'), signals=signals),
              sourceFiles=[record(path) for path in sorted(set(sources))], dynamicSpawnAndMaterialWriteCandidates=runtime_candidates,
              serializedPrefabClosure=prefab_inventory, serializedClosureFiles=len(visited),
              repairedScriptReferences=repaired_references,
              normalStart='Cannon.HandleGrab starts Spawner.BeginSpawning on first ordinary grip, removes tutorial and activates game-start dialogue. Controller trigger or actual OpenXR hand-tracking auto fire follows existing paths; idle Play cannot prove waves or damage/variants.',
              ending='After final timed wave, EndAllWaves stops music/invokes completion then waits scene7s+12s before activating initially inactive end Timeline under active ExperienceApp root. Fade signal binds FadeToBlack(float2); End signal binds MyExperienceApp.EndExperience on component1931093835. Inspect actual activation/clocks/fader/event delivery. Shield break pauses/disables spawner and resets effects/shield before resuming, so source estimates may shift; do not hack health/timing.',
              effects='Single/scatter/full-auto real travelling CannonBall physics (nextFixed activation/AddForce), muzzle emission/recoil/haptics, enemy/drone/projectile/UFO collisions, portal/explosion/score popup and pooled reset/reuse. Inspect ordinary secondary spawners and all nested shield/powerup/end firework systems.',
              requiredChecks=['At least180 measured game/wave seconds and60/120/180 captures, continue through final wave, dialogue, fireworks/fade and SDK exit; all three ordinary weapons/wave enemy families must be observed.',
                              'Several player and enemy projectiles visibly at spawn/travel/impact/reset; hits/blocked UFO shield/drone/powerup/meteor interactions through actual inputs, not screenshots of only cannons.',
                              'Trigger every reachable nested enemy/portal/explosion/trail/powerup/firework family through normal events; static serialized closure is not an observed reachability inventory.',
                              'Verify shield flash/pulse/break/reset, pooled enemy/projectile reuse and disappearance across several instances/positions; record failures or coverage exclusions honestly.',
                              'Save actual player/cannon/hand/enemy poses, wave/time/particle state and useful surroundings/side/up/back/down views for matched before/after. Review source shader flags/scalars/live writes and freshURP-only final appearance.',
                              'Actual Quest3 controller and hand tracking/grab/auto-fire/release plus both haptic start/stop and stereo/performance require hardware.'],
              limits=['All source estimates and serialized/nested dependency candidates remain planning data; effective hierarchy/overrides, event delivery, native import and actual selected behavior require runtime proof.',
                      'No actual Unity/import/render/gameplay/URP/shader/IL2CPP/player/Quest3 validation in this source check.'])
destination = project / 'Builds/URPMigration/OfflineCompileUnity67/source-progression-plan.json'
destination.write_text(json.dumps(report, indent=2), encoding='utf-8')
print(json.dumps(dict(waves=len(waves), waveCompletionEstimate=time, endTimelineActivationEstimate=time+delay+dialogue,
                      signals=signals, nestedPrefabCandidates=len(prefab_inventory),
                      repairedScriptReferences=repaired_references, plan=str(destination)), indent=2))
