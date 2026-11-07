"""B2.9 benchmark client, copied to /tmp in runtime container; no product edits."""
import argparse,concurrent.futures,json,platform,threading,time,urllib.request
import numpy as np

p=argparse.ArgumentParser();p.add_argument('--image',required=True);p.add_argument('--clients',type=int,default=1);p.add_argument('--seconds',type=int,default=0);a=p.parse_args()
data=open(a.image,'rb').read();boundary='B29Boundary'
body=b'--'+boundary.encode()+b'\r\nContent-Disposition: form-data; name="image"; filename="laptop.jpg"\r\nContent-Type: image/jpeg\r\n\r\n'+data+b'\r\n--'+boundary.encode()+b'--\r\n'
rows=[];errors=[];deadline=[0];lock=threading.Lock()
barrier=threading.Barrier(a.clients,action=lambda:deadline.__setitem__(0,time.perf_counter()+a.seconds))
def one():
 start=time.perf_counter()
 request=urllib.request.Request('http://127.0.0.1:5001/predict',data=body,headers={'Content-Type':'multipart/form-data; boundary='+boundary})
 with urllib.request.urlopen(request,timeout=60) as response: result=json.load(response)
 return {'api_total_ms':result['total_time_ms'],'inference_ms':result['inference_time_ms'],'wall_ms':(time.perf_counter()-start)*1000}
def worker(index):
 for _ in range(10):one()
 barrier.wait();local=[];failed=[]
 while (time.perf_counter()<deadline[0]) if a.seconds else (len(local)+len(failed)<200):
  try:local.append(dict(one(),client=index))
  except Exception as e: failed.append({'client':index,'error':type(e).__name__})
 with lock:rows.extend(local);errors.extend(failed)
start=time.perf_counter()
with concurrent.futures.ThreadPoolExecutor(max_workers=a.clients) as pool:list(pool.map(worker,range(a.clients)))
result={'image':a.image,'bytes':len(data),'clients':a.clients,'warmup_per_client':10,'iterations_per_client':None if a.seconds else 200,'seconds':a.seconds or None,
        'elapsed_including_warmup_s':time.perf_counter()-start,'os':platform.platform(),'samples':len(rows),'errors':errors,'rows':rows,
        'load_note':'60s after both clients warm up; requests started before deadline may finish after deadline' if a.seconds else 'single sequential client'}
for key in ['api_total_ms','inference_ms','wall_ms']:
 values=[r[key] for r in rows]
 result[key]={'p50':float(np.percentile(values,50)),'p95':float(np.percentile(values,95)),'p99':float(np.percentile(values,99)),'max':max(values),'over_220_ms':sum(x>220 for x in values)}
print(json.dumps(result))
