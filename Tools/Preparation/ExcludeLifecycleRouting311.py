from pathlib import Path
import json, hashlib
P=Path(__file__).resolve().parents[2]/'Builds/URPMigration'
f=P/'comparison-series.json'
a=json.loads(f.read_text())
kept=[]; excluded=[]
for s in a['series']:
    (excluded if s['id']=='Lifecycle311' else kept).append(s)
assert len(excluded)==1 and len(excluded[0]['pairs'])==36
(P/'R311-1009/lifecycle-routing-exclusion.json').write_text(json.dumps({'reason':'Whole SDK-camera diagnostics include tiny, occluded or inactive targets. Retained for gameplay/lifecycle observation, excluded from accepted appearance comparison routing.','series':excluded},indent=2)+'\n')
(P/'R311-1009/comparison-series-before-exclusion.json').write_bytes(f.read_bytes())
a['series']=kept
f.write_text(json.dumps(a,indent=2)+'\n')
e=P/'current-evidence.json'; q=json.loads(e.read_text())
x=P/'R311-1009/lifecycle-routing-exclusion.json'
q['reports']['lifecycleRoutingExclusion']={'path':str(x.relative_to(P)).replace('\\','/'),'sha256':hashlib.sha256(x.read_bytes()).hexdigest(),'label':'Lifecycle diagnostics retained; visibility exclusions'}
e.write_text(json.dumps(q,indent=2)+'\n')
print('11 accepted pairs; 36 lifecycle pairs retained as diagnostics')
