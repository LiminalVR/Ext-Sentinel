"""One frozen, root-leased Sentinel311 source startup. No Play, save or retry."""
from pathlib import Path
import argparse, hashlib, json, os, subprocess, sys

P = Path(__file__).resolve().parents[2]
T = P / 'Tools/Preparation'
H = lambda p: hashlib.sha256(Path(p).read_bytes()).hexdigest()
Q = lambda s: "'" + str(s).replace("'", "''") + "'"
ap = argparse.ArgumentParser()
ap.add_argument('--plan', required=True)
ap.add_argument('--plan-sha', required=True)
ap.add_argument('--root-granted', action='store_true')
arg = ap.parse_args()
assert arg.root_granted, 'Explicit root serialized startup lease required'
plan_path = Path(arg.plan)
assert H(plan_path) == arg.plan_sha, 'Frozen plan changed'
p = json.loads(plan_path.read_text(encoding='utf-8-sig'))
S = plan_path.parent
assert p['project'].replace('\\', '/') == str(P).replace('\\', '/')
assert H(__file__) == p['launcher']['sha256']
assert H(p['indexPath']) == p['indexSHA'], 'Exact existing index must remain unchanged'
for pin in p['currentPins'] + p['helpers'] + p['provenance']:
    assert H(pin['path']) == pin['sha256'], pin['path']
assert H(p['request']['path']) == p['request']['sha256'], 'Prepared request changed'
release = json.loads((S / 'previous-heavy-release.json').read_text(encoding='utf-8-sig'))
assert release['actualTerminal'] and release['noOtherHeavy'] and release['rootAuthorizedConditional311']
assert H(release['path']) == release['sha256'], 'Heavy release artifact changed'
assert p['apiCompilation']['errors'] == 0 and p['apiCompilation']['scope'] == 'Preserved matching native-options helper API compile; fresh Unity target/import remains pending'
for action in p['actions']:
    assert H(action['source']) == action['sha256'] and H(action['metaSource']) == action['metaSHA']
    assert not Path(action['target']).exists() and not Path(action['metaTarget']).exists()
    guid = subprocess.run(['rg', '-l', '--fixed-strings', 'guid: ' + action['GUID'], str(P / 'Assets'), '-g', '*.meta'], capture_output=True, text=True)
    assert guid.returncode == 1, 'Owned GUID exists or GUID search failed'
assert (P / 'Assets/_LimappEditor').is_dir(), 'Existing Editor owner folder required'
args = p['args']
assert '-batchmode' in args and '-force-d3d11' in args and '-nographics' not in args and '-quit' not in args
fd = os.open(S / 'launch-attempt.json', os.O_WRONLY | os.O_CREAT | os.O_EXCL)
with os.fdopen(fd, 'w', encoding='utf-8') as f:
    json.dump({'planSHA': arg.plan_sha, 'release': release, 'processDispatched': False}, f, indent=2)

# Immediate external guard MUST return zero before any dependent owned copy/request write.
guard = r'''function Get-FileHash([string]$LiteralPath,[string]$Algorithm){$h=[Security.Cryptography.SHA256]::Create();try{[PSCustomObject]@{Hash=([BitConverter]::ToString($h.ComputeHash([IO.File]::ReadAllBytes($LiteralPath)))).Replace('-','')}}finally{$h.Dispose()}};$ErrorActionPreference='Stop';$m=@(Get-CimInstance Win32_Process -Filter "Name='Unity.exe'"|Where-Object{$_.CommandLine -notmatch 'AssetImportWorker'});function ReadProjectPath([string]$line){if([string]::IsNullOrWhiteSpace($line)){return $null};$hit=[regex]::Match($line,'(?i)(?:"-projectPath"|(?<!\S)-projectPath)\s+(?:"([^"]+)"|([^\s"]+))');if(-not $hit.Success){return $null};$value=if($hit.Groups[1].Success){$hit.Groups[1].Value}else{$hit.Groups[2].Value};return $value.Replace('\','/').TrimEnd('/')};$classified=@($m|ForEach-Object{$path=ReadProjectPath $_.CommandLine;[PSCustomObject]@{pid=$_.ProcessId;path=$path;unknown=($null -eq $path);task=($null -eq $path -or $path -match '(?i)/Upgrade/Limapps/')}});$t=@($classified|Where-Object task);$f=[int64](Get-CimInstance Win32_OperatingSystem).FreePhysicalMemory*1024;$own=@($m|Where-Object{$_.CommandLine -like '*311_Sentinel*'});[PSCustomObject]@{utc=[DateTime]::UtcNow.ToString('o');main=$m.Count;task=$t.Count;freeBytes=$f;owned=$own.Count;mainPids=@($m.ProcessId);unknownMain=@($classified|Where-Object unknown).Count;roles=@($classified|Select-Object pid,task,unknown)}|ConvertTo-Json -Compress;if($m.Count -gt 5 -or $t.Count -ne 0 -or $f -lt 12884901888 -or $own.Count -ne 0){throw 'Immediate12GiB/main5/task0/noexisting311 guard rejected'};'''
r = subprocess.run(['powershell', '-NoProfile', '-Command', guard], capture_output=True, text=True)
(S / 'external-guard.json').write_text(json.dumps({'exitCode': r.returncode, 'stdout': r.stdout, 'stderr': r.stderr}, indent=2), encoding='utf-8')
if r.returncode != 0:
    sys.exit(r.returncode)
# Revalidate all exact action/request/index pins immediately after guard; no fall-through.
assert H(p['indexPath']) == p['indexSHA']
assert H(p['request']['path']) == p['request']['sha256']
for x in p['currentPins']:
    assert H(x['path']) == x['sha256'], x['path']
request = json.loads(Path(p['request']['path']).read_text(encoding='utf-8-sig'))
request.update(rootLeased=True, pid=0, launchPlan=str(plan_path), launchPlanSHA=arg.plan_sha)
live_request = T / 'source-bootstrap-request.json'
if live_request.exists():
    (S / 'prior-source-bootstrap-request.json').write_bytes(live_request.read_bytes())
live_request.write_bytes((json.dumps(request, indent=2) + '\n').encode())
cmd = guard
# Check every payload before the first persistent owned copy.
for a in p['actions']:
    cmd += 'if((Test-Path -LiteralPath ' + Q(a['target']) + ') -or (Test-Path -LiteralPath ' + Q(a['metaTarget']) + ')){throw "Owned target appeared"};'
    cmd += 'if((Get-FileHash -LiteralPath ' + Q(a['source']) + ' -Algorithm SHA256).Hash.ToLower() -ne ' + Q(a['sha256']) + '){throw "Owned payload drift"};'
    cmd += 'if((Get-FileHash -LiteralPath ' + Q(a['metaSource']) + ' -Algorithm SHA256).Hash.ToLower() -ne ' + Q(a['metaSHA']) + '){throw "Owned metadata drift"};'
for a in p['actions']:
    cmd += 'Copy-Item -LiteralPath ' + Q(a['source']) + ' -Destination ' + Q(a['target']) + ';Copy-Item -LiteralPath ' + Q(a['metaSource']) + ' -Destination ' + Q(a['metaTarget']) + ';'
cmd += '$owned=Start-Process -FilePath ' + Q(p['exe']) + ' -ArgumentList @(' + ','.join(Q('"' + x + '"') for x in args) + ') -WindowStyle Hidden -PassThru;[PSCustomObject]@{pid=$owned.Id;utc=[DateTime]::UtcNow.ToString("o")}|ConvertTo-Json -Compress'
(S / 'launch-command.ps1').write_bytes(cmd.encode())
r = subprocess.run(['powershell', '-NoProfile', '-Command', cmd], capture_output=True, text=True)
(S / 'launcher-result.json').write_text(json.dumps({'exitCode': r.returncode, 'stdout': r.stdout, 'stderr': r.stderr}, indent=2), encoding='utf-8')
if r.returncode != 0:
    sys.exit(r.returncode)  # Preserve partial copies/failure, never retry or send dependent native calls.
receipt = next(d for line in r.stdout.splitlines() if line.startswith('{') for d in [json.loads(line)] if 'pid' in d)
request['pid'] = receipt['pid']
live_request.write_bytes((json.dumps(request, indent=2) + '\n').encode())
(S / 'launch.json').write_text(json.dumps(dict(receipt, project=str(P), log=p['log'], sourceOnly=True), indent=2), encoding='utf-8')
print(json.dumps(receipt))
