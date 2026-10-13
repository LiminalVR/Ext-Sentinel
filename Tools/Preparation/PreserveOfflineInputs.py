"""Preserve current source/config/dirty inputs and every current Assets meta."""
from datetime import datetime, timezone
from pathlib import Path
import hashlib
import json
import shutil
import subprocess

project = Path(__file__).resolve().parents[2]
output = project / 'Builds/URPMigration/OfflineCompileUnity67'
backup = output / 'BeforeCompatibilityFix'
manifest_path = backup / 'manifest.json'
if manifest_path.exists():
    raise RuntimeError('Original preservation already exists; do not replace it.')
paths = set((project / 'Assets').rglob('*.meta'))
for extension in ('*.cs', '*.asmdef', '*.asmref'):
    for path in (project / 'Assets').rglob(extension):
        paths.add(path)
        if not Path(str(path) + '.meta').is_file():
            raise RuntimeError('Source/assembly meta missing: ' + str(path))
paths.update(path for path in (project / 'ProjectSettings').rglob('*') if path.is_file())
paths.update((project / 'Packages').glob('*.json'))
for scope in ([], ['--cached']):
    changed = subprocess.check_output(['git', 'diff', '--relative', *scope, '--name-only', '--diff-filter=ACMRT', '-z'],
                                     cwd=project, stderr=subprocess.DEVNULL).decode('utf-8').split('\0')
    for name in changed:
        if name and (project / name).is_file():
            paths.add(project / name)
preflight = json.loads((project / 'Builds/URPMigration/preflight-source.json').read_text(encoding='utf-8'))
paths.update(project / name for name in preflight['enabledScenes'])
for name in ('.gitignore', '.gitattributes'):
    if (project / name).is_file():
        paths.add(project / name)
records = []
for path in sorted(paths):
    relative = path.relative_to(project)
    destination = backup / 'Files' / relative
    destination.parent.mkdir(parents=True, exist_ok=True)
    if destination.exists() and destination.read_bytes() != path.read_bytes():
        raise RuntimeError('Existing preservation differs; retain it: ' + str(relative))
    if not destination.exists():
        shutil.copyfile(path, destination)
    checksum = hashlib.sha256(path.read_bytes()).hexdigest()
    if hashlib.sha256(destination.read_bytes()).hexdigest() != checksum:
        raise RuntimeError('Preserved input readback failed: ' + str(relative))
    records.append({'path': str(relative), 'bytes': path.stat().st_size,
                    'sha256': checksum, 'preservedPath': str(destination.relative_to(project))})
initial = backup / 'InitialEvidence'
summary = json.loads((output / 'summary.json').read_text(encoding='utf-8'))
for item in summary['reports']:
    path = Path(item['report'])
    destination = initial / path.relative_to(output)
    destination.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(path, destination)
for name in ('worker-first.log', 'summary.json'):
    shutil.copyfile(output / name, initial / name)
(backup / 'git-status-before.txt').write_text(subprocess.check_output(
    ['git', 'status', '--short'], cwd=project, text=True), encoding='utf-8')
diff_result = subprocess.run(['git', 'diff', '--relative', '--binary'], cwd=project,
                             stdout=subprocess.PIPE, stderr=subprocess.PIPE)
(backup / 'git-diff-before.patch').write_bytes(diff_result.stdout)
(backup / 'git-diff-before.stderr.txt').write_bytes(diff_result.stderr)
(backup / 'git-head-before.txt').write_text(subprocess.check_output(
    ['git', 'rev-parse', 'HEAD'], cwd=project, text=True), encoding='utf-8')
manifest = {'utc': datetime.now(timezone.utc).isoformat(), 'project': str(project),
            'offlineOnly': True, 'unityEditorLaunched': False,
            'sourceFiles': sum(1 for path in paths if path.suffix == '.cs'),
            'currentAssetsMetas': sum(1 for path in paths if path.suffix == '.meta' and path.is_relative_to(project / 'Assets')),
            'inputFileCount': len(records), 'files': records,
            'gitNote': 'Current dirty work and deleted integrations retained; no reset or restoration.',
            'gitDiff': {'exitCode': diff_result.returncode,
                        'complete': diff_result.returncode == 0,
                        'stdoutSHA256': hashlib.sha256(diff_result.stdout).hexdigest(),
                        'stderrSHA256': hashlib.sha256(diff_result.stderr).hexdigest(),
                        'stderr': diff_result.stderr.decode('utf-8', errors='replace')}}
manifest_path.write_text(json.dumps(manifest, indent=2), encoding='utf-8')
print(json.dumps({'sources': manifest['sourceFiles'], 'currentAssetsMetas': manifest['currentAssetsMetas'],
                  'preservedInputs': len(records), 'gitDiff': manifest['gitDiff'],
                  'manifest': str(manifest_path)}, indent=2))
