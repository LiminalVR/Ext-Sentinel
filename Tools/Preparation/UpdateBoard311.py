from pathlib import Path
import json,datetime,subprocess
P=Path(__file__).resolve().parents[2];U=P.parents[1];R=P/'Builds/URPMigration/R311-1009';q=json.loads((U/'UPGRADE_QUEUE.json').read_text())
row=next(x for x in q if x['experience']=='311_Sentinel');status=row['status']
md=U/'UPGRADE_QUEUE.md';raw=md.read_bytes();pre=R/'queue-before-311-status.md';
if not pre.exists():pre.write_bytes(raw)
lines=raw.splitlines(keepends=True);match=[i for i,l in enumerate(lines) if l.startswith(b'| 311_Sentinel |')];assert len(match)==1
lines[match[0]]=('| 311_Sentinel | '+row['branch']+' | '+status+' |\r\n').encode();md.write_bytes(b''.join(lines))
with (U/'UPGRADE_PROGRESS.md').open('a',encoding='utf-8') as f:f.write('\n\n9 October2026 - Sentinel311: '+status+' [Preservation](Limapps/311_Sentinel/Builds/URPMigration/R311-1009/baseline.json).\n')
v=subprocess.run(['python',str(U/'Tools/UrpBatch/build_portal.py')],capture_output=True,text=True);print(v.stdout[-1200:]+v.stderr[-1200:]);assert v.returncode==0
