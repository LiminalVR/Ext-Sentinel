from pathlib import Path
import json,hashlib,shutil,subprocess
P=Path(__file__).resolve().parents[2];U=P.parents[1];R=P/'Builds/URPMigration/R311-1009';S=P/'Builds/URPMigration/S311-1009';old=P/'Builds/URPMigration/S311-1004T193135';T=P/'Tools/Preparation'
H=lambda f:hashlib.sha256(Path(f).read_bytes()).hexdigest()
b=json.loads((R/'baseline.json').read_text());assert all(H(x['path'])==H(x['preimage'])==x['sha256'] for x in b['rows']+b['SDKPins']);assert H(b['indexPath'])==b['indexSHA']
p=json.loads((old/'plan.json').read_text());assert all(H(x['path'])==x['sha256'] for x in p['currentPins'])
p['launcher']['sha256']=H(T/'launch_source_baseline_batch.py');p['request']=dict(path=str(S/'request.json'),sha256=H(S/'request.json'));p['indexSHA']=b['indexSHA']
for x in p['helpers']:x['sha256']=H(x['path'])
for a in p['actions']:
 a['sha256']=H(a['source']);a['metaSource']=str(S/Path(a['metaSource']).name);a['metaSHA']=H(a['metaSource'])
p['log']=str(S/'source.log');p['args'][p['args'].index('-logFile')+1]=p['log']
ed=Path('C:/Program Files/Unity/Hub/Editor/6000.7.0b2/Editor/Data/DotNetSdk')
if (S/'api.log').exists():shutil.copyfile(S/'api.log',S/'api-first-host-path-failure.log')
api=subprocess.run([str(ed/'dotnet.exe'),str(ed/'sdk/10.0.303/Roslyn/bincore/csc.dll'),'@'+str(S/'api.rsp')],capture_output=True,text=True)
(S/'api.log').write_text(api.stdout+api.stderr);assert api.returncode==0,(api.stdout+api.stderr)[-4000:]
shutil.copyfile(old/'api.json',S/'historical-api.json');p['apiCompilation']['currentHelperCompile']=dict(exitCode=0,log=str(S/'api.log'),sha256=H(S/'api.log'))
assert all(H(x['path'])==x['sha256'] for x in p['provenance'])
complete=U/'Limapps/306_Puzzle_-_8192/Unseen/Builds/URPMigration/R306-1009/completion.json';assert json.loads(complete.read_text())['completed']
(S/'previous-heavy-release.json').write_text(json.dumps(dict(actualTerminal=True,noOtherHeavy=True,rootAuthorizedConditional311=True,path=str(complete),sha256=H(complete),soleMigration=True),indent=2))
(S/'plan.json').write_text(json.dumps(p,indent=2));ready=dict(plan=str(S/'plan.json'),planSHA=H(S/'plan.json'),projectInputs=len(b['rows']),SDKInputs=len(b['SDKPins']),indexExact=True,helperCompileExitCode=0)
(S/'ready.json').write_text(json.dumps(ready,indent=2));print(json.dumps(ready))
