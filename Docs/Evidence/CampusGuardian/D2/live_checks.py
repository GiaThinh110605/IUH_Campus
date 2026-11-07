"""D2 opt-in live smoke. Run from repository root; evidence only, no app source changes."""
from datetime import datetime, timezone, timedelta
import hashlib, io, json, os, platform, subprocess, sys, time, traceback
from pathlib import Path
from uuid import uuid4
import numpy as np
import requests
from dotenv import dotenv_values
from PIL import Image, ImageOps

ROOT=Path.cwd(); OUT=ROOT/'Docs/Evidence/CampusGuardian/D2'
ENV=dotenv_values(ROOT/'Server/.env')
SECRETS=[str(v) for k,v in ENV.items() if v and ('KEY' in k or 'PASSWORD' in k or 'TOKEN' in k)]
def redact(s):
    for value in SECRETS: s=s.replace(value,'[REDACTED]')
    return s
def save(name,obj):
    (OUT/name).write_text(redact(json.dumps(obj,ensure_ascii=False,indent=2))+'\n',encoding='utf-8')
def cmd(args,name,timeout=120):
    p=subprocess.run(args,capture_output=True,text=True,encoding='utf-8',errors='replace',timeout=timeout)
    save(name,{'command':args,'exit_code':p.returncode,'stdout':p.stdout,'stderr':p.stderr})
    return p
DC=['docker','compose','-f','Server/docker-compose.yml']
client=requests.Session(); client.trust_env=False
results=[]
def req(name,method,path,expected=None,**kwargs):
    response=client.request(method,'http://127.0.0.1:5001'+path,timeout=60,**kwargs)
    try: body=response.json()
    except ValueError: body=response.text[:4000]
    row={'name':name,'method':method,'path':path,'http':response.status_code,'body':body}
    if 'json' in kwargs: row['request']=kwargs['json']
    if expected is not None: row['expected_http']=expected; row['pass_http']=response.status_code in expected
    results.append(row); save('http.json',results)
    return response

def predict(name,scan=None,path=None):
    path=path or ROOT/'Server/tests/fixtures/laptop-stocksnap.jpg'
    with Image.open(path) as source:
        im=ImageOps.exif_transpose(source).convert('RGB'); im.thumbnail((640,640))
        data=io.BytesIO(); im.save(data,format='JPEG',quality=85)
    return req(name,'POST','/predict',[200],files={'image':('fixture.jpg',data.getvalue(),'image/jpeg')},
               data={'scan_session_id':scan} if scan else {'target_class':'laptop'})

def event(kind='scan_confirmed',game=None,token=None):
    return {'event_id':str(uuid4()),'session_id':game or str(uuid4()),'quest_id':'D2_manual_test',
            'milestone_id':'ar_monster' if token else 'library_intro','event_type':kind,
            'occurred_at':datetime.now(timezone.utc).isoformat(),
            'payload':{'monster_token':token} if token else {'label':'laptop','confidence':.9}}

def main():
    import winreg
    with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE,r'HARDWARE\DESCRIPTION\System\CentralProcessor\0') as k: cpu=winreg.QueryValueEx(k,'ProcessorNameString')[0].strip()
    save('environment.json',{'commit':subprocess.check_output(['git','rev-parse','HEAD'],text=True).strip(),
        'date':datetime.now(timezone(timedelta(hours=7))).isoformat(),'os':platform.platform(),'cpu':cpu,
        'config':'YOLOv8n 640, confidence 0.70, SCAN_INFECTED_POOL=cell phone,laptop,chair',
        'env_sha256_before':hashlib.sha256((ROOT/'Server/.env').read_bytes()).hexdigest(),
        'manual_telemetry':'Events increment real test aggregates/Adafruit feeds; not user gameplay.',
        'cloud_credentials_present':bool(ENV.get('ADAFRUIT_AIO_KEY') and ENV.get('ADAFRUIT_AIO_USERNAME'))})
    cmd(DC+['ps','--format','json'],'compose-ps.json')
    req('health','GET','/health',[200]); predict('predict-fixture')
    sample=event(); req('recorded','POST','/gameplay/events',[201],json=sample)
    req('duplicate','POST','/gameplay/events',[200],json=sample)
    req('conflict','POST','/gameplay/events',[409],json=dict(sample,quest_id='D2_conflict'))
    req('invalid-payload','POST','/gameplay/events',[400],json=dict(event(),payload={'label':'laptop','confidence':.9,'core':999}))
    token=None; token_game=None
    for index in range(10):
        game=str(uuid4()); created=req(f'scan-create-{index}','POST','/scan/sessions',[201],json={'session_id':game}).json()
        scan=created['scan_session_id']
        if index==0:
            req('scan-active-409','POST','/scan/sessions',[409],json={'session_id':game})
            predict('scan-none',scan,ROOT/'Server/tests/fixtures/blank.png')
            # No test fixture outside the pool is guaranteed detectable: try actual labeled photos, never mock detections.
            candidates=list((ROOT/'Server/tests/fixtures/real/book').glob('*.jpg'))+list((ROOT/'Server/tests/fixtures/real/bottle').glob('*.jpg'))+list((ROOT/'Server/tests/fixtures/real/backpack').glob('*.jpg'))
            for j,p in enumerate(candidates):
                if p.stat().st_size>5*1024**2: continue
                body=predict('outside-pool-'+p.parent.name+'-'+p.name,scan,p).json()
                if body.get('scan',{}).get('label') in ('book','bottle','backpack'): break
        body=predict(f'scan-laptop-{index}',scan).json()
        if body.get('scan',{}).get('result')=='infested':
            token=body['scan']['monster_token']; token_game=game; break
    if token:
        won=event('combat_won',token_game,token)
        req('token-reward','POST','/gameplay/events',[201],json=won)
        req('token-duplicate','POST','/gameplay/events',[200],json=won)
        req('token-reuse-400','POST','/gameplay/events',[400],json=dict(won,event_id=str(uuid4())))
    save('token-attempt.json',{'found_token':bool(token),'attempts':index+1})
    cmd(DC+['restart','iuh-guardian-api'],'api-restart.json')
    for _ in range(40):
        try:
            if client.get('http://127.0.0.1:5001/health',timeout=3).status_code==200: break
        except requests.RequestException: pass
        time.sleep(1)
    req('duplicate-after-restart','POST','/gameplay/events',[200],json=sample)
    req('external-metrics','GET','/metrics',[403,404])
    for name,url in [('prometheus-targets','http://127.0.0.1:9090/api/v1/targets'),('grafana-health','http://127.0.0.1:3000/api/health')]:
        r=client.get(url,timeout=10); save(name+'.json',{'http':r.status_code,'body':r.json()})
    # Client lives inside the API container and calls real Gunicorn HTTP; 5 warmups + 50 measured requests.
    bench='''import io,json,time,statistics,urllib.request,platform,os
import numpy as np
from PIL import Image
# Reproducible committed fixture copied to /tmp only for this run.
data=open('/tmp/d2-laptop.jpg','rb').read()
boundary='D2Boundary'
body=b'--'+boundary.encode()+b'\\r\\nContent-Disposition: form-data; name="image"; filename="laptop.jpg"\\r\\nContent-Type: image/jpeg\\r\\n\\r\\n'+data+b'\\r\\n--'+boundary.encode()+b'--\\r\\n'
rows=[]
for i in range(55):
 start=time.perf_counter()
 request=urllib.request.Request('http://127.0.0.1:5001/predict',data=body,headers={'Content-Type':'multipart/form-data; boundary='+boundary})
 result=json.load(urllib.request.urlopen(request,timeout=60))
 if i>=5: rows.append({'wall_ms':(time.perf_counter()-start)*1000,'api_total_ms':result['total_time_ms'],'inference_ms':result['inference_time_ms']})
r={'warmup':5,'iterations':50,'os':platform.platform(),'rows':rows,'input':'laptop-stocksnap.jpg original fixture, no client resize; model input 640'}
for k in rows[0]: r[k]={'p50':float(np.percentile([x[k] for x in rows],50)),'p95':float(np.percentile([x[k] for x in rows],95))}
r['B2.7_pass']=r['api_total_ms']['p50']<=150 and r['api_total_ms']['p95']<=220
print(json.dumps(r))'''
    cmd(DC+['cp','Server/tests/fixtures/laptop-stocksnap.jpg','iuh-guardian-api:/tmp/d2-laptop.jpg'],'fixture-copy.json')
    p=cmd(DC+['exec','-T','iuh-guardian-api','python','-c',bench],'benchmark-command.json',timeout=300)
    if p.returncode==0: save('benchmark.json',json.loads(p.stdout))
    # Public feeds are read with credentials held in memory; save only values/errors.
    sys.path.insert(0,str(ROOT/'Server'))
    from app.cloud_client import AdafruitClient
    aio=AdafruitClient(ENV.get('ADAFRUIT_AIO_USERNAME',''),ENV.get('ADAFRUIT_AIO_KEY',''))
    feeds=['guardian-quests-completed','guardian-campus-core','guardian-upgrades','lost-found-count','guardian-monsters-revealed']
    save('adafruit-feeds.json',[aio.last_value(f) for f in feeds])
    cmd(DC+['exec','-T','iuh-guardian-api','id'],'container-user.json')
    config_code="import json;from app.config import load_config;c=load_config();print(json.dumps({k:c[k] for k in ['CONFIDENCE_THRESHOLD','SCAN_INFECTED_POOL','WEB_CONCURRENCY','ORT_INTRA_THREADS','ALLOWED_CLASSES','MODEL_PATH']}))"
    cmd(DC+['exec','-T','iuh-guardian-api','python','-c',config_code],'container-config.json')
    req('final-health','GET','/health',[200])

if __name__=='__main__':
    try: main()
    except Exception: save('live-error.json',{'traceback':traceback.format_exc()}); raise
