"""Short TTL/cooldown overrides only on throwaway sidecar; real HTTP, no code patch."""
import time,requests
from uuid import uuid4
from live_checks import cmd,DC,save
name='d2-scan-boundaries'
try:
 p=cmd(DC+['run','-d','--no-deps','--name',name,'-p','127.0.0.1:5002:5001',
          '-e','DATABASE_PATH=/tmp/d2-boundary.db','-e','SCAN_SESSION_TTL_MINUTES=0.02',
          '-e','SCAN_COOLDOWN_SECONDS=3','-e','SCAN_MAX_SESSIONS_PER_GAME_SESSION=2',
          '-e','ADAFRUIT_AIO_KEY=','iuh-guardian-api'],'boundary-start.json')
 if p.returncode: raise RuntimeError('sidecar startup failed')
 client=requests.Session(); client.trust_env=False
 base='http://127.0.0.1:5002'
 for _ in range(60):
  try:
   if client.get(base+'/health',timeout=2).status_code==200: break
  except requests.RequestException: pass
  time.sleep(1)
 rows=[]; game=str(uuid4())
 def request(name,method,path,expected,**kw):
  r=client.request(method,base+path,timeout=20,**kw)
  rows.append({'name':name,'http':r.status_code,'expected':expected,'body':r.json(),'pass':r.status_code==expected})
  save('scan-boundaries.json',{'environment':'Isolated Docker sidecar, real HTTP; temporary SQLite; test-only TTL=1.2s, cooldown=3s, max sessions=2. Model/pool/threshold unchanged.', 'rows':rows})
  return r.json()
 one=request('create','POST','/scan/sessions',201,json={'session_id':game})
 request('active','POST','/scan/sessions',409,json={'session_id':game})
 time.sleep(1.4)
 request('expired','GET','/scan/sessions/'+one['scan_session_id'],410)
 request('cooldown','POST','/scan/sessions',429,json={'session_id':game})
 time.sleep(2)
 request('create-second','POST','/scan/sessions',201,json={'session_id':game})
 time.sleep(1.4)
 request('limit','POST','/scan/sessions',429,json={'session_id':game})
finally:
 cmd(['docker','rm','-f',name],'boundary-cleanup.json')
