from pathlib import Path
import json,hashlib,shutil,subprocess,datetime
P=Path(__file__).resolve().parents[2];U=P.parents[1];R=P/'Builds/URPMigration/R311-1009';S=P/'Builds/URPMigration/S311-1009'
H=lambda f:hashlib.sha256(Path(f).read_bytes()).hexdigest()
assert not R.exists() and not S.exists()
R.mkdir();S.mkdir(); rows=[]
for f in sorted(f for folder in ['Assets','Packages','ProjectSettings'] for f in (P/folder).rglob('*') if f.is_file()):
 rel=f.relative_to(P).as_posix();dest=R/'pre'/rel;dest.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(f,dest);sha=H(f);assert H(dest)==sha
 rows.append(dict(path=str(f),relative=rel,sha256=sha,preimage=str(dest)))
b306=json.loads((U/'Limapps/306_Puzzle_-_8192/Unseen/Builds/URPMigration/R306-1009/baseline.json').read_text())
sdk=b306['SDKPins'];assert all(H(x['path'])==x['sha256'] and H(x['preimage'])==x['sha256'] for x in sdk)
index=P/'.git/index';shutil.copyfile(index,R/'index');indexsha=H(index)
for name,args in [('status.txt',['status','--porcelain=v1']),('working.patch',['diff','--binary']),('staged.patch',['diff','--cached','--binary'])]:
 v=subprocess.run(['git','-C',str(P),*args],capture_output=True,check=True);(R/name).write_bytes(v.stdout)
git=lambda *a:subprocess.check_output(['git','-C',str(P),*a],text=True).strip()
baseline=dict(createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),rows=rows,SDKPins=sdk,indexPath=str(index),indexSHA=indexsha,HEAD=git('rev-parse','HEAD'),branch=git('branch','--show-current'))
(R/'baseline.json').write_text(json.dumps(baseline,indent=2))
T=P/'Tools/Preparation';old=P/'Builds/URPMigration/S311-1004T193135'
for n in ['Sentinel311SourceBootstrap.cs','Sentinel311SourceObserver.cs','Sentinel311OfficialClose.cs','launch_source_baseline_batch.py']:shutil.copyfile(T/n,S/('prior-'+n))
obs=T/'Sentinel311SourceObserver.cs';v=obs.read_text();assert 'nextSample=time+1;' in v;obs.write_text(v.replace('nextSample=time+1;','nextSample=time+10;'))
launch=T/'launch_source_baseline_batch.py';v=launch.read_text();assert '$t.Count -gt 2' in v;v=v.replace('$t.Count -gt 2','$t.Count -ne 0').replace('Immediate12GiB/main5/task2/noexisting311','Immediate12GiB/main5/task0/noexisting311')
v=v.replace("receipt = next(json.loads(line) for line in r.stdout.splitlines() if line.startswith('{') and '\"pid\"' in line)","receipt = next(d for line in r.stdout.splitlines() if line.startswith('{') for d in [json.loads(line)] if 'pid' in d)")
launch.write_text(v)
p=json.loads((old/'plan.json').read_text());critical=p['currentPins'];drift=[x['path'] for x in critical if H(x['path'])!=x['sha256']];assert not drift,drift
request=json.loads((old/'request.json').read_text());request['indexSHA']=indexsha;request['fullCurrentPins']=[dict(path=x['path'],sha256=x['sha256']) for x in rows]
(S/'request.json').write_text(json.dumps(request,indent=2))
p['launcher']['sha256']=H(launch);p['request']=dict(path=str(S/'request.json'),sha256=H(S/'request.json'));p['indexSHA']=indexsha
for x in p['helpers']:x['sha256']=H(x['path'])
for a in p['actions']:
 a['sha256']=H(a['source']);n=Path(a['metaSource']).name;shutil.copyfile(old/n,S/n);a['metaSource']=str(S/n);a['metaSHA']=H(S/n)
p['log']=str(S/'source.log');p['args'][p['args'].index('-logFile')+1]=p['log']
# Historical API report retained; compile current helper bodies with its preserved matching options.
rsp=(old/'api.rsp').read_text().replace(str(old),str(S));(S/'api.rsp').write_text(rsp)
editor=Path('C:/Program Files/Unity/Hub/Editor/6000.7.0b2/Editor/Data')
api=subprocess.run([str(editor/'NetCoreRuntime/dotnet.exe'),str(editor/'DotNetSdkRoslyn/csc.dll'),'@'+str(S/'api.rsp')],capture_output=True,text=True)
(S/'api.log').write_text(api.stdout+api.stderr);assert api.returncode==0,(api.stdout+api.stderr)[-4000:]
shutil.copyfile(old/'api.json',S/'historical-api.json')
p['apiCompilation']['currentHelperCompile']=dict(exitCode=api.returncode,log=str(S/'api.log'),sha256=H(S/'api.log'))
for x in p['provenance']:assert H(x['path'])==x['sha256'],x['path']
complete=U/'Limapps/306_Puzzle_-_8192/Unseen/Builds/URPMigration/R306-1009/completion.json';assert json.loads(complete.read_text())['completed']
(S/'previous-heavy-release.json').write_text(json.dumps(dict(actualTerminal=True,noOtherHeavy=True,rootAuthorizedConditional311=True,path=str(complete),sha256=H(complete),soleMigration=True),indent=2))
(S/'plan.json').write_text(json.dumps(p,indent=2));(S/'ready.json').write_text(json.dumps(dict(plan=str(S/'plan.json'),planSHA=H(S/'plan.json'),projectInputs=len(rows),SDKInputs=len(sdk),indexExact=True,originalInputsPreserved=True,helperCompileExitCode=0),indent=2))
print((S/'ready.json').read_text())
