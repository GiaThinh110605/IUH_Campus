"""Generate B2.9 tables from raw measured samples; preserve D2 history."""
from pathlib import Path
from collections import Counter
import json,re
import numpy as np
P=Path('Docs/Evidence/CampusGuardian/D2');r=json.loads((P/'benchmark-b29.json').read_text(encoding='utf-8'))
assert 'default_gameplay_pass' in r and len(r['runs'])>=9 and r['env_unchanged']
threads=r['default_runtime_config']['ORT_INTRA_THREADS']
def stats(runs):
 rows=[x for run in runs for x in run['rows']]; output={'samples':len(rows),'errors':sum(len(run['errors']) for run in runs)}
 for k in ['api_total_ms','inference_ms','wall_ms']:
  values=[x[k] for x in rows];output[k]={q:float(np.percentile(values,n)) for q,n in [('p50',50),('p95',95),('p99',99)]};output[k].update(max=max(values),over_220_ms=sum(x>220 for x in values))
 return output
base={s:stats([x for x in r['runs'] if x['scenario']==s and x['ort_threads']==threads]) for s in ['a','b','c']}
lines=['','## B2.9 — đo lại trong điều kiện gameplay','',
 f"Ngày {r['date']}; commit nền `{r['commit']}`; host {r['cpu']}, {r['host_os']}. Máy cắm sạc (AC=1), Unity Editor đã đóng; Edge/YouTube và cửa sổ Claude đã đóng trước đo. Các tiến trình nền còn lại được ghi ở host_before/host_after mỗi lượt trong JSON; không tuyên bố OS hoàn toàn không có tải nền. Docker/Codex được giữ. Không sửa code sản phẩm hay .env.", '',
 f"Ảnh nguồn laptop-stocksnap.jpg: {r['images']['original']['size']}, {r['images']['original']['bytes']}bytes; ảnh gameplay {r['images']['gameplay']['size']}, JPEG quality80, **{r['images']['gameplay']['bytes']}bytes**. EXIF Orientation trước resize, cạnh dài≤640, không upscale, BILINEAR và kích thước làm tròn tương ứng OutputSize. Pillow JPEG80 cùng thông số với GuardianImageEncoder, không cam kết bit-identical với Unity EncodeToJPG/Graphics.Blit. Ảnh chỉ vào thư mục tạm/container /tmp, không commit.", '',
 f"Cấu hình mặc định đo thật: {r['default_runtime_config']}. Ba lần lặp mỗi (a)/(b):10warmup+200request tuần tự một client trong container gọi HTTP Gunicorn localhost (không nginx/LAN). Ba lần (c):hai client song song, mỗi client10warmup rồi chạy60giây; request bắt đầu trước deadline được phép kết thúc sau deadline. Không chạy các phép đo đồng thời với nhau. Chỉ (a) dùng chấm B2.7: p50≤150ms và p95≤220ms, yêu cầu cả ba lần đạt; không lấy trung bình percentile.", '',
 'Raw samples, p50/p95/p99/max và số >220ms cho từng metric/từng lần: [benchmark-b29.json](benchmark-b29.json). Script: [benchmark_b29.py](benchmark_b29.py), [b29_client.py](b29_client.py).', '',
 '### Tổng hợp mẫu (pooled raw samples ba lần; không trung bình percentile)', '',
 '| Kịch bản | Metric | N | p50 ms | p95 ms | p99 ms | max ms | Số >220ms |', '|---|---|---:|---:|---:|---:|---:|---:|']
for s,v in base.items():
 for k in ['api_total_ms','inference_ms','wall_ms']:
  d=v[k];lines.append(f"| ({s}) | {k} | {v['samples']} | {d['p50']:.3f} | {d['p95']:.3f} | {d['p99']:.3f} | {d['max']:.3f} | {d['over_220_ms']} |")
lines+=['','(a) ảnh gameplay≤640/JPEG80; (b) ảnh gốc chỉ so sánh; (c) ảnh gameplay/hai client/tải demo chỉ quan sát. Số >220ms đếm riêng từng metric; API total là chỉ số dùng đối chiếu SLA.','',
 '### Từng lần đo và thử ORT nếu được kích hoạt','','| Kịch bản | ORT threads | Lần | N | API p50 | API p95 | API p99 | API max | API >220 | Lỗi request | B2.7 |','|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|']
for x in r['runs']:
 d=x['api_total_ms'];passed=d['p50']<=150 and d['p95']<=220 and not x['errors'];status=('Pass' if passed else 'Fail') if x['scenario']=='a' else 'Không chấm'
 lines.append(f"| ({x['scenario']}) | {x['ort_threads']} | {x['repetition']} | {x['samples']} | {d['p50']:.3f} | {d['p95']:.3f} | {d['p99']:.3f} | {d['max']:.3f} | {d['over_220_ms']} | {len(x['errors'])} | {status} |")
status='Pass' if r['default_gameplay_pass'] else 'Fail'
lines+=['', 'Lượt trước khi người dùng xác nhận thoát app có tiến trình Claude/Zalo/Unity Hub nền, được giữ ở benchmark-b29-precondition.json và không dùng chấm. Lượt chính chỉ bắt đầu sau xác nhận đã thoát; guard kiểm tra AC và không có Unity/Claude/Zalo/Unity Hub/Edge trước/sau mỗi lượt.', '', ('Không thử ORT4/6 vì cả ba lần gameplay mặc định đều đạt.' if r['default_gameplay_pass'] else 'Đã kích hoạt thử ORT4/6 theo yêu cầu vì gameplay mặc định chưa đạt.'), '',f"**Kết luận theo (a), mặc định ORT{threads}: {status}.** (b)/(c) không dùng thay kết quả gameplay. Nếu kích hoạt ORT4/6, chúng chạy bằng env trên container tạm của cùng image/config, ba lượt(a) mỗi cấu hình; container thử được xóa, mặc định stack không thay đổi. Không tự chọn cấu hình nhanh nhất làm mặc định.", '',
 'D2 trước dùng ảnh gốc chưa resize và Unity Editor vẫn mở; p50=96.215ms/p95=257.221ms bị Fail. Giữ nguyên dòng Fail và raw benchmark.json cũ. B2.9 thay điều kiện đo theo gameplay; khác điều kiện không chứng minh định lượng riêng tác động của Unity hay resize, cũng không xóa kết quả cũ.']
p=P/'README.md';s=p.read_text(encoding='utf-8-sig');s=s.split('\n## B2.9 —')[0];p.write_text(s.rstrip()+'\n'+'\n'.join(lines)+'\n',encoding='utf-8')
summary={'default_threads':threads,'default_gameplay_pass':r['default_gameplay_pass'],'pooled':base}
(P/'benchmark-b29-summary.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8')
p=Path('Docs/Kiem_Thu_Backend.md');s=p.read_text(encoding='utf-8')
s=s.split('\n## B2.9 —')[0]
# Preserve the old failure row verbatim and add explanation outside the historical table.
a=base['a']['api_total_ms'];ap95=[x['api_total_ms']['p95'] for x in r['runs'] if x['scenario']=='a' and x['ort_threads']==threads]
s+='\n## B2.9 — độ trễ gameplay (bổ sung, không xóa Fail D2)\n\n'
s+=f"D2 dùng ảnh gốc {r['images']['original']['size']} và Unity Editor đang mở. Dòng G09-Perf Fail (p95=257.221ms) cùng evidence cũ giữ nguyên; các điều kiện này khác input gameplay và có thể ảnh hưởng latency, chưa tách định lượng từng nguyên nhân. B2.9 dùng ảnh≤640/JPEG80 {r['images']['gameplay']['bytes']}bytes, Unity/Edge/Claude đã đóng, máy cắm AC; xem quy trình và bảng(a)/(b)/(c) trong [README B2.9](Evidence/CampusGuardian/D2/README.md). Chỉ (a) chấm chỉ tiêu B2.7; mỗi lần phải đạt.\n\n"
s+='| ID | Ca kiểm thử | Nền tảng/môi trường | Commit | Bước | Mong đợi | Thực tế | Pass/Fail | Bằng chứng |\n|---|---|---|---|---|---|---|---|---|\n'
s+=f"| G09-Perf-B2.9 | Độ trễ gameplay | AC/Unity đóng/Docker CPU/2workers/ORT{threads} | {r['commit'][:7]} | 3 lần; mỗi lần10warmup+200request, một client, JPEG80≤640 | Mỗi lần APIp50≤150 và p95≤220ms | Pooled p50={a['p50']:.3f}, p95={a['p95']:.3f}; p95 từng lần={','.join(f'{v:.3f}' for v in ap95)}ms; >220={a['over_220_ms']}/600 | {status} | [benchmark-b29.json](Evidence/CampusGuardian/D2/benchmark-b29.json), [README](Evidence/CampusGuardian/D2/README.md) |\n"
counts=Counter(line.split('|')[-3].strip() for line in s.splitlines() if line.startswith('| ') and ('| Pass |' in line or '| Fail |' in line or '| Chưa kiểm thử |' in line))
s+=f"\nTổng toàn bộ bảng sau bổ sung B2.9: **{counts['Pass']} Pass / {counts['Fail']} Fail / {counts['Chưa kiểm thử']} Chưa kiểm thử**. Số liệu D2 và B2.6 ở các phần trước là lịch sử, không bị thay thế.\n"
p.write_text(s,encoding='utf-8')
p=Path('Docs/Bao_Cao_Ky_Thuat_Backend.md');s=p.read_text(encoding='utf-8').split('\n## Bổ sung B2.9')[0]
s+=f"\n## Bổ sung B2.9 — điều kiện đo latency gameplay\n\nB2.9 đo lại khi máy cắm AC, Unity Editor/Edge/Claude đã đóng, Docker giữ model YOLOv8n640/conf0.70 và cấu hình mặc định ORT{threads}/Gunicorn2worker. Fixture gốc {r['images']['original']['size']} ({r['images']['original']['bytes']}bytes) được resize cạnh dài≤640 thành {r['images']['gameplay']['size']}, JPEG80/{r['images']['gameplay']['bytes']}bytes; Pillow mô phỏng thông số GuardianImageEncoder, không khẳng định encoder giống từng byte. Ba lần(a), mỗi lần10warmup+200request tuần tự một client trong container, cho pooled APIp50={a['p50']:.3f}ms/p95={a['p95']:.3f}ms; p95 từng lần={','.join(f'{v:.3f}' for v in ap95)}ms, kết luận **{status}** theo B2.7 (cả ba lần phải đạt). (b) ảnh gốc và(c) hai client/60s×3 chỉ để so sánh/tải demo. Fail D2 cũ p95=257.221ms vẫn giữ: lúc đó ảnh gốc lớn và Unity mở cùng lúc, khác điều kiện gameplay; chưa quy nguyên nhân định lượng cho từng yếu tố. Thử ORT4/6 nếu được kích hoạt chỉ qua env/container tạm, không thay mặc định. Xem raw metrics đủ p50/p95/p99/max và số>220ms tại [benchmark-b29.json](Evidence/CampusGuardian/D2/benchmark-b29.json), bảng tại [README B2.9](Evidence/CampusGuardian/D2/README.md).\n"
p.write_text(s,encoding='utf-8')
print(json.dumps(summary,indent=2))
