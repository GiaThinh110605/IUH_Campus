"""Collect stable monitoring/cloud snapshots then stop stack without deleting volumes."""
import hashlib,json,time
from pathlib import Path
import requests
from live_checks import cmd,DC,save,ROOT,ENV
import sys
sys.path.insert(0,str(ROOT/'Server'))
from app.cloud_client import AdafruitClient
r=[]
for _ in range(10):
 response=requests.get('http://127.0.0.1:9090/api/v1/targets',timeout=10)
 body=response.json();r.append({'http':response.status_code,'body':body})
 if any(t['health']=='up' for t in body['data']['activeTargets']): break
 time.sleep(3)
save('prometheus-targets-stable.json',r)
for _ in range(30):
 health=requests.get('http://127.0.0.1:5001/health',timeout=10).json()
 if health['outbox_pending']==0: break
 time.sleep(1)
save('health-before-down.json',health)
aio=AdafruitClient(ENV['ADAFRUIT_AIO_USERNAME'],ENV['ADAFRUIT_AIO_KEY'])
save('adafruit-feeds-final.json',[aio.last_value(f) for f in ['guardian-quests-completed','guardian-campus-core','guardian-upgrades','lost-found-count','guardian-monsters-revealed']])
volumes=['docker','volume','ls','--filter','label=com.docker.compose.project=server','--format','{{.Name}}']
before=cmd(volumes,'volumes-before.json')
down=cmd(DC+['down'],'docker-down.json')
after=cmd(volumes,'volumes-after.json')
save('cleanup.json',{'down_exit':down.returncode,'volume_names_before':before.stdout.splitlines(),'volume_names_after':after.stdout.splitlines(),
 'volumes_preserved':set(before.stdout.splitlines())<=set(after.stdout.splitlines()),
 'env_sha256_after':hashlib.sha256((ROOT/'Server/.env').read_bytes()).hexdigest(),
 'env_unchanged':hashlib.sha256((ROOT/'Server/.env').read_bytes()).hexdigest()==json.loads((Path(__file__).parent/'environment.json').read_text())['env_sha256_before']})
cmd(DC+['ps'],'compose-ps-after-down.json')
