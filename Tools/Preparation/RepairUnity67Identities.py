"""Reuse only byte-authenticated, reversible pool-ID and water-name repairs."""
from datetime import datetime, timezone
from pathlib import Path
import hashlib
import json
import re
import shutil
import subprocess

project = Path(__file__).resolve().parents[2]
output = project / 'Builds/URPMigration/OfflineCompileUnity67'
backup = output / 'BeforeCompatibilityFix'
manifest = json.loads((backup / 'manifest.json').read_text(encoding='utf-8'))
files = {item['path']: item for item in manifest['files']}

def sha(data):
    return hashlib.sha256(data).hexdigest()

index = Path(subprocess.check_output(['git', 'rev-parse', '--git-path', 'index'], cwd=project, text=True).strip())
if not index.is_absolute():
    index = project / index
manifest['gitIndex'] = dict(path=str(index), exists=index.is_file(), sha256=sha(index.read_bytes()) if index.is_file() else None)
if index.is_file():
    shutil.copyfile(index, backup / 'git-index-before.bin')
    if sha((backup / 'git-index-before.bin').read_bytes()) != manifest['gitIndex']['sha256']:
        raise RuntimeError('Git index preservation readback failed.')
(backup / 'manifest.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')

fire = project.parent / '285_Fireside_Slumber/FiresideSlumberUnity'
fire_report = fire / 'Builds/URPMigration/unity67-object-id-repairs.json'
fire_proof = next(item for item in json.loads(fire_report.read_text(encoding='utf-8'))['repairs'] if item['path'].endswith('CFX_SpawnSystem.cs'))
borrowed = fire / fire_proof['path']
original = (project / files['Assets\\Scripts\\CFX_SpawnSystem.cs']['preservedPath']).read_bytes()
fixed = borrowed.read_bytes()
if sha(original) != fire_proof['beforeSHA256'] or sha(fixed) != fire_proof['afterSHA256']:
    raise RuntimeError('Pool original/repaired template identity differs.')
reverse = fixed
for item in reversed(fire_proof['replacements']):
    reverse = reverse.replace(item['after'].encode(), item['before'].encode())
if reverse != original:
    raise RuntimeError('Pool template reversal does not recover complete exact original.')
pool_replacements = fire_proof['replacements']

cannon = project.parent / '221_Cannon_s_Roar/Cannon-Roar'
water_report = cannon / 'Builds/URPMigration/OfflineCompileUnity67/reflection-name-repair.json'
water_proof = json.loads(water_report.read_text(encoding='utf-8'))['changes'][0]
water_template = cannon / water_proof['path']
water_relative = 'Assets\\Unused or Cleaning\\Watershader\\WaterReflection.cs'
water_original = (project / files[water_relative]['preservedPath']).read_bytes()
water_fixed = water_template.read_bytes()
if sha(water_original) != water_proof['originalSHA256'] or sha(water_fixed) != water_proof['currentSHA256']:
    raise RuntimeError('Water original/repaired template identity differs.')
newline = b'\r\n' if b'\r\n' in water_original else b'\n'
water_replacements = []
for line in water_original.splitlines():
    if b'GetInstanceID()' in line:
        modern = newline.join([b'#if UNITY_6000_7_OR_NEWER', line.replace(b'GetInstanceID()', b'GetEntityId()'), b'#else', line, b'#endif'])
        water_replacements.append(dict(before=line.decode(), after=modern.decode()))
reverse = water_fixed
for item in reversed(water_replacements):
    reverse = reverse.replace(item['after'].encode(), item['before'].encode())
if reverse != water_original or len(water_replacements) != 2:
    raise RuntimeError('Exact temporary naming reversal failed.')

def contract(data):
    return re.findall(rb'^\s*(?:(?:static\s+public|public|protected)\b[^\r\n]*|\[SerializeField\][^\r\n]*)', data, re.M)

changes = []
for relative, prior, current, replacements, proof, template, scope in [
    ('Assets\\Scripts\\CFX_SpawnSystem.cs', original, fixed, pool_replacements, fire_report, borrowed,
     'Full EntityId on6.7 through private pool dictionaries, three lookup locals and private cursor parameter; prior Int32 branch and public/serialized/spawn/lifecycle semantics retained.'),
    (water_relative, water_original, water_fixed, water_replacements, water_report, water_template,
     'Only three temporary texture/camera naming calls change on6.7; prior version retained. Reflection/camera/texture/property/math/timing semantics unchanged.')]:
    path = project / relative
    meta = project / (relative + '.meta')
    if path.read_bytes() != prior or meta.read_bytes() != (project / files[relative + '.meta']['preservedPath']).read_bytes():
        raise RuntimeError('Current original or meta changed before repair: ' + relative)
    if contract(prior) != contract(current):
        raise RuntimeError('Public/protected/serialized declaration changed: ' + relative)
    path.write_bytes(current)
    changes.append(dict(path=relative, originalSHA256=sha(prior), currentSHA256=sha(current),
                        metaSHA256Unchanged=sha(meta.read_bytes()), scope=scope,
                        borrowedSource=str(template), borrowedSourceSHA256=sha(template.read_bytes()),
                        borrowedProof=str(proof), borrowedProofSHA256=sha(proof.read_bytes()),
                        exactReverseRestoresOriginal=True, publicProtectedSerializedDeclarationsUnchanged=True,
                        declarationCount=len(contract(prior)), replacements=replacements))
report = dict(utc=datetime.now(timezone.utc).isoformat(), project=str(project), offlineOnly=True,
              unityEditorLaunched=False, changes=changes, configurationEdits=[], metasChanged=[],
              initialFailure='Six CS0619 errors in main109sources for each managed target; Editor197 blocked by failed fresh main, never cached fallback.')
(output / 'unity67-identity-repairs.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
print(json.dumps([dict(path=item['path'], original=item['originalSHA256'], current=item['currentSHA256']) for item in changes], indent=2))
