"""Real cloud auth failure on isolated SQLite; never alter .env or production DB clock."""
import json,time,sys
from live_checks import cmd,DC,save,req,event,ENV,ROOT

sys.path.insert(0,str(ROOT/'Server'))

code=r'''
import os,json,time,tempfile
from datetime import datetime,timezone
from uuid import uuid4
from app import create_app
from app.sync_worker import SyncWorker
from app.cloud_client import AdafruitClient
from app.event_store import MAX_ATTEMPTS
# Wrong key exists only in this throwaway container process environment.
os.environ['ADAFRUIT_AIO_KEY']='D2_invalid_key_for_auth_failure'
with tempfile.TemporaryDirectory() as folder:
 app=create_app({'TESTING':True,'MODEL_PATH':'','DATABASE_PATH':folder+'/fault.db'})
 client=app.test_client(); store=app.extensions['event_store']
 event={'event_id':str(uuid4()),'session_id':str(uuid4()),'quest_id':'D2_fault', 'milestone_id':'library_intro',
 'event_type':'scan_confirmed','occurred_at':datetime.now(timezone.utc).isoformat(),'payload':{'label':'laptop','confidence':.9}}
 r=client.post('/gameplay/events',json=event)
 report={'isolated_database':True,'key':'[WRONG TEST KEY]','recorded_http':r.status_code,'recorded':r.json,
 'scheduler':'Controlled now argument advances 400 seconds per step to exercise backoff/8-attempt terminal state without a 20-minute wait. Real HTTP sends are spaced >=3.2 seconds.', 'steps':[]}
 worker=SyncWorker(store,AdafruitClient(app.config['ADAFRUIT_AIO_USERNAME'],os.environ['ADAFRUIT_AIO_KEY']))
 base=time.time()
 for index in range(4*MAX_ATTEMPTS):
  worker.step(now=base+index*400)
  status=client.get('/gameplay/events/'+event['event_id']).json
  report['steps'].append({'index':index,'event':status,'summary':store.summary()})
  print(json.dumps({'progress':index,'cloud':status['cloud']}),flush=True)
  time.sleep(3.2)
 with store.connection() as db:
  report['outbox']=[dict(row) for row in db.execute('SELECT feed_key,status,attempts,last_error,next_attempt_at FROM outbox')]
 report['pass']=r.status_code==201 and all(x['event']['cloud']!='synced' for x in report['steps']) and all(x['status']=='failed' and x['attempts']==8 for x in report['outbox'])
 print('D2_RESULT='+json.dumps(report),flush=True)
'''
p=cmd(DC+['run','--rm','--no-deps','-T','iuh-guardian-sync','python','-c',code],'cloud-fault-command.json',timeout=400)
if p.returncode==0:
    result=json.loads(p.stdout.split('D2_RESULT=')[-1]); save('cloud-fault.json',result)
# Restore/use the original correct key: normal API/worker were never changed.
e=event(); r=req('cloud-correct-recorded','POST','/gameplay/events',[201],json=e)
history=[]
import live_checks
live_checks.results=json.loads((live_checks.OUT/"http.json").read_text(encoding="utf-8"))
for _ in range(80):
    s=req('cloud-correct-status','GET','/gameplay/events/'+e['event_id'],[200]).json()
    history.append(s)
    if s['cloud'] in ('synced','failed','disabled'): break
    time.sleep(1)
save('cloud-correct.json',{'recorded':r.json(),'history':history,'key':'[REDACTED original env key]','pass':history[-1]['cloud']=='synced'})
from app.cloud_client import AdafruitClient
save('adafruit-feeds-final.json',[AdafruitClient(ENV['ADAFRUIT_AIO_USERNAME'],ENV['ADAFRUIT_AIO_KEY']).last_value(f) for f in ['guardian-quests-completed','guardian-campus-core','guardian-upgrades','lost-found-count','guardian-monsters-revealed']])
