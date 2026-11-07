"""B2.9 host orchestrator. Run from repository root; only evidence and container /tmp files."""
from pathlib import Path
from datetime import datetime,timedelta,timezone
import hashlib,io,json,os,platform,subprocess,tempfile,time
from PIL import Image,ImageOps
import numpy as np
from dotenv import dotenv_values
ROOT=Path.cwd();OUT=ROOT/'Docs/Evidence/CampusGuardian/D2';DC=['docker','compose','-f','Server/docker-compose.yml']
secret=[v for k,v in dotenv_values('Server/.env').items() if v and any(x in k for x in ['KEY','PASSWORD','TOKEN'])]
def redact(s):
 for value in secret:s=s.replace(value,'[REDACTED]')
 return s
report={'commit':subprocess.check_output(['git','rev-parse','HEAD'],text=True).strip(),'date':datetime.now(timezone(timedelta(hours=7))).isoformat(),'runs':[],'commands':[],
 'protocol':'3 repetitions each: a resized JPEG80/1client/10warmup+200; b original/1client/10+200; c resized/2clients/10warmup per client+60s closed-loop. B2.7 scored only a; every repetition must pass; aggregate also shown.',
 'env_sha256_before':hashlib.sha256((ROOT/'Server/.env').read_bytes()).hexdigest()}
def save(): (OUT/'benchmark-b29.json').write_text(redact(json.dumps(report,ensure_ascii=False,indent=2))+'\n',encoding='utf-8')
def command(args,timeout=120):
 p=subprocess.run(args,capture_output=True,text=True,encoding='utf-8',errors='replace',timeout=timeout)
 report['commands'].append({'command':args,'exit_code':p.returncode,'stderr':p.stderr[-6000:]})
 if p.returncode:save();raise RuntimeError('Command failed: '+redact(p.stderr[-500:]))
 return p.stdout
power_script=r'''
Add-Type -TypeDefinition 'using System;using System.Runtime.InteropServices;public class B29Status { [StructLayout(LayoutKind.Sequential)] public struct Power { public byte AC,BatteryFlag,Percent,Flag;public uint Life,FullLife; } [DllImport("kernel32.dll")] public static extern bool GetSystemPowerStatus(out Power p); }'; $power=[B29Status+Power]::new(); $ok=[B29Status]::GetSystemPowerStatus([ref]$power); $apps=Get-Process | Where-Object { $_.ProcessName -in @('Unity','Unity Hub','msedge','claude','Zalo','ChatGPT','codex','vmmemWSL') } | Select-Object ProcessName,Id,@{N='RAM_MB';E={[math]::Round($_.WorkingSet64/1MB)}},@{N='CPU_seconds';E={$_.CPU}}; [pscustomobject]@{AC=$power.AC;BatteryPercent=$power.Percent;PowerReadOK=$ok;Processes=@($apps)} | ConvertTo-Json -Depth 4 -Compress
'''
def snapshot():
 r=json.loads(command(['powershell','-NoProfile','-Command',power_script]));
 if r['AC']!=1 or any(x['ProcessName'] in ('Unity','claude','Zalo','Unity Hub','msedge') for x in r['Processes']):save();raise RuntimeError('Required AC/Unity-closed condition not met')
 return r
snapshot0=snapshot(); report['host_before']=snapshot0
report['host_os']=platform.platform()
report['docker_info']=command(['docker','info','--format','{{.NCPU}} CPUs; {{.MemTotal}} memory'])
command(DC+['up','-d']); report['compose_ps']=command(DC+['ps','--format','json'])
api=command(DC+['ps','-q','iuh-guardian-api']).strip()
original=ROOT/'Server/tests/fixtures/laptop-stocksnap.jpg';raw=original.read_bytes()
with Image.open(original) as im:
 source_size=list(im.size);image=ImageOps.exif_transpose(im).convert('RGB');scale=min(1,640/max(image.size));size=tuple(max(1,round(x*scale)) for x in image.size);image=image.resize(size,Image.Resampling.BILINEAR);buf=io.BytesIO();image.save(buf,format='JPEG',quality=80);small=buf.getvalue()
report['images']={'original':{'file':'Server/tests/fixtures/laptop-stocksnap.jpg','size':source_size,'bytes':len(raw),'sha256':hashlib.sha256(raw).hexdigest()},
 'gameplay':{'size':list(size),'bytes':len(small),'sha256':hashlib.sha256(small).hexdigest(),'jpeg_quality':80,'resize':'EXIF transpose then long side<=640, no upscale, dimensions rounded like OutputSize; Pillow BILINEAR approximates Graphics.Blit, JPEG encoder not byte-identical to Unity'}}
report['cpu']=__import__('winreg').QueryValueEx(__import__('winreg').OpenKey(__import__('winreg').HKEY_LOCAL_MACHINE,r'HARDWARE\DESCRIPTION\System\CentralProcessor\0'),'ProcessorNameString')[0].strip()
config_code="import json;from app.config import load_config;c=load_config();print(json.dumps({k:c[k] for k in ['MODEL_PATH','ORT_INTRA_THREADS','WEB_CONCURRENCY','CONFIDENCE_THRESHOLD','SCAN_INFECTED_POOL']}))"
report['default_runtime_config']=json.loads(command(['docker','exec',api,'python','-c',config_code]))

def prepare(container,directory):
 for name,content in [('b29-original.jpg',raw),('b29-gameplay.jpg',small)]:
  file=Path(directory)/name;file.write_bytes(content);command(['docker','cp',str(file),container+':/tmp/'+name])
 command(['docker','cp',str(OUT/'b29_client.py'),container+':/tmp/b29_client.py'])
def run(container,scenario,repetition,threads):
 before=snapshot();image='/tmp/b29-original.jpg' if scenario=='b' else '/tmp/b29-gameplay.jpg'
 args=['docker','exec',container,'python','/tmp/b29_client.py','--image',image]
 if scenario=='c':args+=['--clients','2','--seconds','60']
 result=json.loads(command(args,timeout=300));result.update(scenario=scenario,repetition=repetition,ort_threads=threads,host_before=before,host_after=snapshot())
 report['runs'].append(result);save();print(json.dumps({'scenario':scenario,'rep':repetition,'threads':threads,'samples':result['samples'],'api':result['api_total_ms']}),flush=True)
with tempfile.TemporaryDirectory() as directory:
 prepare(api,directory)
 for repetition in range(1,4):
  for scenario in ['a','b','c']:run(api,scenario,repetition,report['default_runtime_config']['ORT_INTRA_THREADS'])
 baseline=[r for r in report['runs'] if r['scenario']=='a']
 failed=any(r['api_total_ms']['p95']>220 or r['api_total_ms']['p50']>150 or r['errors'] for r in baseline)
 report['default_gameplay_pass']=not failed
 if failed:
  for threads in [4,6]:
   name='b29-ort-'+str(threads)
   try:
    command(DC+['run','-d','--no-deps','--name',name,'-e','ORT_INTRA_THREADS='+str(threads),'iuh-guardian-api'])
    for _ in range(60):
     probe=subprocess.run(['docker','exec',name,'curl','-fsS','http://127.0.0.1:5001/health'],capture_output=True)
     if probe.returncode==0:break
     time.sleep(1)
    prepare(name,directory)
    for repetition in range(1,4):run(name,'a',repetition,threads)
   finally:command(['docker','rm','-f',name])
report['host_after']=snapshot();report['env_unchanged']=report['env_sha256_before']==hashlib.sha256((ROOT/'Server/.env').read_bytes()).hexdigest()
report['default_runtime_config_after']=json.loads(command(['docker','exec',api,'python','-c',config_code]));save()
