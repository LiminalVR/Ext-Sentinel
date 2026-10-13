from pathlib import Path
import json,hashlib,shutil
P=Path(__file__).resolve().parents[2];t=P/'Tools/Preparation/launch_source_baseline_batch.py';s=P/'Builds/URPMigration/S311-1009';d=P/'Builds/URPMigration/S311-1009V2'
v=t.read_text();marker="guard = r'''";assert marker in v
fn="function Get-FileHash([string]$LiteralPath,[string]$Algorithm){$h=[Security.Cryptography.SHA256]::Create();try{[PSCustomObject]@{Hash=([BitConverter]::ToString($h.ComputeHash([IO.File]::ReadAllBytes($LiteralPath)))).Replace('-','')}}finally{$h.Dispose()}};"
assert fn not in v
shutil.copyfile(t,s/'prior-launcher-before-hash-fix.py');t.write_text(v.replace(marker,marker+fn));d.mkdir()
p=json.loads((s/'plan.json').read_text());shutil.copyfile(s/'plan.json',d/'prior-plan.json');shutil.copyfile(s/'previous-heavy-release.json',d/'previous-heavy-release.json')
p['launcher']['sha256']=hashlib.sha256(t.read_bytes()).hexdigest();p['log']=str(d/'source.log');p['args'][p['args'].index('-logFile')+1]=p['log'];(d/'plan.json').write_text(json.dumps(p,indent=2));print(hashlib.sha256((d/'plan.json').read_bytes()).hexdigest())
