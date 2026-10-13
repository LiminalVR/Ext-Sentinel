"""Verify exact guarded repairs, current input coverage and fresh project DLLs."""
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
repairs = json.loads((output / 'unity67-identity-repairs.json').read_text(encoding='utf-8'))
changes = {item['path']: item for item in repairs['changes']}
summary = json.loads((output / 'summary.json').read_text(encoding='utf-8'))
if summary['unresolvedScopes'] or summary['unverifiedEditorSources']:
    raise RuntimeError('Current source scopes are incomplete.')

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def contract(data):
    return re.findall(rb'^\s*(?:(?:static\s+public|public|protected)\b[^\r\n]*|\[SerializeField\][^\r\n]*)', data, re.M)

preserved = []
for item in manifest['files']:
    if sha(project / item['preservedPath']) != item['sha256']:
        raise RuntimeError('Original changed: ' + item['path'])
    current = sha(project / item['path'])
    if current != changes.get(item['path'], {}).get('currentSHA256', item['sha256']):
        raise RuntimeError('Unexpected current edit: ' + item['path'])
    preserved.append(dict(item, currentSHA256=current, changed=current != item['sha256']))
    if item['path'].removesuffix('.meta') in changes:
        destination = output / 'CurrentAfterFix' / item['path']
        destination.parent.mkdir(parents=True, exist_ok=True)
        if destination.exists() and sha(destination) != current:
            raise RuntimeError('Existing after-fix backup differs.')
        if not destination.exists():
            shutil.copyfile(project / item['path'], destination)
        if sha(destination) != current:
            raise RuntimeError('After-fix readback failed.')
for relative, change in changes.items():
    current = (project / relative).read_bytes()
    original_record = next(item for item in manifest['files'] if item['path'] == relative)
    original = (project / original_record['preservedPath']).read_bytes()
    reverse = current
    for item in reversed(change['replacements']):
        reverse = reverse.replace(item['after'].encode(), item['before'].encode())
    if reverse != original or contract(original) != contract(current):
        raise RuntimeError('Repair changed other source/contracts: ' + relative)
    if sha(Path(change['borrowedSource'])) != change['borrowedSourceSHA256'] or sha(Path(change['borrowedProof'])) != change['borrowedProofSHA256']:
        raise RuntimeError('Authenticated template/proof changed.')
    if sha(project / (relative + '.meta')) != change['metaSHA256Unchanged']:
        raise RuntimeError('Source meta changed.')
metas = {str(path.relative_to(project)) for path in (project / 'Assets').rglob('*.meta')}
if metas != {item['path'] for item in manifest['files'] if item['path'].startswith('Assets\\') and item['path'].endswith('.meta')}:
    raise RuntimeError('Assets meta inventory changed.')
head = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=project, text=True)
if head != (backup / 'git-head-before.txt').read_text(encoding='utf-8'):
    raise RuntimeError('Existing Git HEAD changed.')
status = subprocess.check_output(['git', 'status', '--short'], cwd=project, text=True)
prior_status = (backup / 'git-status-before.txt').read_text(encoding='utf-8')
changed_git = {path.replace('\\', '/') for path in changes}
def git_name(row):
    value = row[3:]
    return json.loads(value) if value.startswith('"') else value
if [row for row in status.splitlines() if git_name(row) not in changed_git] != [row for row in prior_status.splitlines() if git_name(row) not in changed_git]:
    raise RuntimeError('Existing Git state changed outside two scoped repairs.')
index = Path(manifest['gitIndex']['path'])
if index.is_file() != manifest['gitIndex']['exists'] or (index.is_file() and sha(index) != manifest['gitIndex']['sha256']):
    raise RuntimeError('Git index changed.')
if index.is_file() and sha(backup / 'git-index-before.bin') != manifest['gitIndex']['sha256']:
    raise RuntimeError('Original Git index preservation differs.')
if sha(backup / 'git-diff-before.patch') != manifest['gitDiff']['stdoutSHA256'] or sha(backup / 'git-diff-before.stderr.txt') != manifest['gitDiff']['stderrSHA256']:
    raise RuntimeError('Original Git diff evidence differs.')

inputs, dependencies = {}, []
editor, android, runtime = set(), set(), set()
scopes = {(item['assembly'], item['platform']): item for item in summary['reports']}

def check(item, category):
    path = Path(item['path'])
    if not path.is_absolute():
        path = project / path
    if sha(path) != item['sha256']:
        raise RuntimeError('Compile input no longer current: ' + str(path))
    record = inputs.setdefault(str(path), dict(path=str(path), sha256=item['sha256'], categories=[], matchesCurrentHash=True))
    if category not in record['categories']:
        record['categories'].append(category)

for scope in summary['reports']:
    report = json.loads(Path(scope['report']).read_text(encoding='utf-8'))
    if report['exitCode'] or report['errors'] or report['missingInputs']:
        raise RuntimeError('Current scope did not pass: ' + scope['label'])
    sources = {item['path'] for item in report['sources']}
    if scope['platform'] == 'Android':
        lines = Path(report['cachedOptions']).read_text(encoding='utf-8-sig').splitlines()
        if '-define:UNITY_ANDROID' not in lines or '-define:UNITY_EDITOR' in lines:
            raise RuntimeError('Unauthenticated Android options.')
        if not any('PlaybackEngines\\AndroidPlayer\\Variations\\il2cpp\\Managed' in item['path'] for item in report['references']):
            raise RuntimeError('Android managed references absent.')
        android.update(sources)
    else:
        editor.update(sources)
        definition = json.loads(Path(report['assemblyDefinition']['path']).read_text(encoding='utf-8-sig'))
        if definition.get('includePlatforms') != ['Editor']:
            runtime.update(sources)
    for category in ('sources', 'references', 'analyzers'):
        for item in report[category]:
            check(item, scope['label'] + ':' + category)
    for category in ('assemblyDefinition', 'compiler', 'dotnetHost'):
        check(report[category], scope['label'] + ':' + category)
    check(dict(path=report['cachedOptions'], sha256=report['cachedOptionsSHA256']), scope['label'] + ':cachedOptions')
    for reference in report['references']:
        path = Path(reference['path'])
        name = path.name.removesuffix('.ref.dll').removesuffix('.dll')
        provider_scope = scopes.get((name, scope['platform']))
        if provider_scope:
            provider_path = Path(provider_scope['report'])
            provider = json.loads(provider_path.read_text(encoding='utf-8'))
            expected = provider_path.parent / (name + '.dll')
            if expected.resolve() != path.resolve() or sha(expected) != reference['sha256'] or provider['errors'] or provider['exitCode']:
                raise RuntimeError('Cached/unverified project dependency: ' + scope['label'])
            for item in provider['sources']:
                check(item, 'provider:' + name)
            dependencies.append(dict(consumer=scope['label'], provider=name, platform=scope['platform'],
                                      reference=reference, providerReport=str(provider_path),
                                      providerSourcesCurrent=True, freshOutputVerified=True))
for item in summary['toolInputs']:
    check(item, 'sharedDriver:toolInputs')
current = {str(path.relative_to(project)) for path in (project / 'Assets').rglob('*.cs')}
if current != editor or runtime != android:
    raise RuntimeError('Current full Editor/Android source coverage differs.')
preflight = json.loads((project / 'Builds/URPMigration/preflight-source.json').read_text(encoding='utf-8'))
archive = json.loads((project / 'Builds/URPMigration/Baseline/archive-sha256.json').read_text(encoding='utf-8'))
if not preflight['sourceArchiveVerified'] or preflight['sourceArchiveSHA256'] != archive['sha256']:
    raise RuntimeError('Existing original archive provenance differs.')
result = dict(summary, utc=datetime.now(timezone.utc).isoformat(),
              branch=subprocess.check_output(['git', 'branch', '--show-current'], cwd=project, text=True).strip(),
              sourceFiles=len(current), androidCompiledUniqueRuntimeSources=len(android),
              preservedCurrentInputFiles=len(preserved), preservedFiles=preserved,
              allCurrentAssetsMetasVerifiedUnchanged=len(metas), sourceEdits=repairs['changes'],
              configurationEdits=[], metasChanged=[], publicProtectedSerializedContractsVerifiedUnchanged=True,
              verifiedUniqueCompileInputFiles=len(inputs), verifiedCurrentCompileInputs=list(inputs.values()),
              allCurrentCompileInputHashesVerified=True, currentSourceCrossAssemblyInteroperabilityVerified=True,
              projectDependencies=dependencies, gitHeadAndIndexUnchanged=True,
              gitStatusUnchangedExceptTwoRepairs=True, originalGitStatusEntryCount=len(prior_status.splitlines()),
              currentGitStatusEntryCount=len(status.splitlines()),
              originalArchive=dict(archive, verification='Existing verified preflight agrees with archive manifest; no fresh archive rehash claimed.'),
              onDiskUnityVersion=(project / 'ProjectSettings/ProjectVersion.txt').read_text(encoding='utf-8'),
              freshImportVerified=False, runtimeVerified=False, renderingVerified=False,
              urpMigrationVerified=False, androidPlayerBuildVerified=False, quest3HardwareVerified=False,
              limits=summary['limitations'] + ['Recorded Unity67FinalAliases SDK candidate remains undeployed; actual final DLL hashes and affected behavior need revalidation.',
                                               'Full ordinary cannon-grab-started waves/powerups, final ending/fireworks, every reachable enemy/effect variant and repeated projectile/hit/pool lifecycles remain pending.',
                                               'Pool ID/naming changes do not port legacy reflection callbacks or establish active rendering coverage.'])
(output / 'verification.json').write_text(json.dumps(result, indent=2), encoding='utf-8')
print(json.dumps(dict(sources=len(current), androidRuntimeSources=len(android), preservedInputs=len(preserved),
                      metas=len(metas), uniqueCompileInputs=len(inputs), freshProjectDependencies=len(dependencies),
                      errors=sum(item['errors'] for item in summary['reports']),
                      warnings={item['label']: item['warnings'] for item in summary['reports']},
                      verification=str(output / 'verification.json')), indent=2))
