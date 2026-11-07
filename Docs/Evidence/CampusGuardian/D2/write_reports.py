"""Build D2 documentation from recorded outputs; preserve prior G09 table."""
from pathlib import Path
import json
from collections import Counter
import xml.etree.ElementTree as ET
P=Path('Docs/Evidence/CampusGuardian/D2'); DOC=Path('Docs')
def load(n): return json.loads((P/n).read_text(encoding='utf-8-sig'))
e=load('environment.json'); b=load('benchmark.json'); u=load('unity-tests.json')['data']['result']; h={x['name']:x for x in load('http.json')}; fault=load('cloud-fault.json'); good=load('cloud-correct.json'); boundaries=load('scan-boundaries.json'); final=load('cleanup.json')
commit=e['commit'][:7]; rows=[]
def add(id,case,steps,expected,actual,status='Pass',evidence='http.json',env='Docker/Windows+WSL2/HTTP'):
 rows.append([id,case,env,commit,steps,expected,actual,status,f'[D2/{evidence}](Evidence/CampusGuardian/D2/{evidence})'])
add('D2-PY','Toàn bộ Server pytest','pytest Server/tests -ra + JUnit','Không fail/skip','134 passed; 0 fail, 0 skip; 16.69s',evidence='pytest.xml',env='Windows/Python 3.11.9')
add('G09-Docker','Build/start 5 service','compose up -d --build; ps','5 running; service có healthcheck healthy','API/nginx/Prometheus/Grafana healthy; sync running, healthcheck disabled',evidence='compose-ps.json')
add('G09-Health','Model/DB sẵn sàng','GET /health','200, model_loaded và db_ok true',str(h['health']['body']),evidence='http.json')
add('G09-ONNX','Artifact ONNX mặc định','Đọc hash/input/provider trong container','YOLOv8n 640 CPU, hash đã chốt','input [1,3,640,640], CPUExecutionProvider; hash 4d063228…',evidence='model-runtime.txt')
add('G09-Predict','Suy luận fixture thật','POST /predict laptop-stocksnap.jpg','200, detections thật',f"HTTP {h['predict-fixture']['http']}; {len(h['predict-fixture']['body']['detections'])} detections")
add('G09-Perf','Benchmark B2.7','5 warmup + 50 request từ container đến Gunicorn','API total p50≤150, p95≤220 ms',f"p50={b['api_total_ms']['p50']:.3f}; p95={b['api_total_ms']['p95']:.3f} ms",'Pass' if b['B2.7_pass'] else 'Fail','benchmark.json')
add('B6-Create','Tạo phiên và gợi ý','POST /scan/sessions game UUID','201; không lộ infected_classes; chỉ điện tử/ghế',h['scan-create-0']['body']['hint'])
add('B2.8-Pool','Chọn lớp/gộp gợi ý','test_scan_pool.py + test_scan_sessions.py','Chỉ pool; 1–2; clamp; gộp điện tử','12 pool tests + 24 scan tests pass; pool container đúng','Pass','pytest.xml','Windows pytest, model mock ở test route')
for result in ['infested','clean','none']:
 matches=[x for x in h.values() if isinstance(x['body'],dict) and x['body'].get('scan',{}).get('result')==result]
 add('B6-'+result,'Kết quả scan '+result,'POST /predict có scan_session_id','200 với result='+result,f"{matches[0]['name']}: {matches[0]['body']['scan']}",'Pass' if matches else 'Chưa kiểm thử')
outside=[x for x in h.values() if x['name'].startswith('outside-pool') and x['body'].get('scan',{}).get('result')=='clean']
add('B2.8-Clean','Đồ ngoài pool','Ảnh bottle3 qua EXIF/resize/JPEG, predict có scan id','bottle được phát hiện, clean, không token',str(outside[0]['body']['scan']),'Pass' if outside else 'Chưa kiểm thử')
add('B6.1-409','Chặn phiên mở đồng thời','Tạo lại cùng game session khi còn mở','409 scan_session_active',str(h['scan-active-409']['body']))
for n,id,expected in [('expired','B6-Expiry','410 scan_session_expired'),('cooldown','B6.1-Cooldown','429 scan_cooldown'),('limit','B6.1-Limit','429 scan_limit_reached')]:
 r=next(x for x in boundaries['rows'] if x['name']==n)
 add(id,n,'HTTP sidecar: TTL=1.2s, cooldown=3s, max sessions=2',expected,f"{r['http']} {r['body']}",'Pass' if r['pass'] else 'Fail','scan-boundaries.json','Docker sidecar/DB tạm, thời gian cấu hình rút ngắn; pytest cũng test mặc định')
for n,id,expected in [('recorded','G08-Record','201 recorded'),('duplicate','G06-Duplicate','200 duplicate'),('conflict','G08-Conflict','409 event_id_conflict'),('invalid-payload','G08-Invalid','400 unknown_field'),('token-reward','G06-Token','201, thưởng 10 Core'),('token-duplicate','G06-TokenDuplicate','200 duplicate, không cộng thêm'),('token-reuse-400','G06-Reuse','400 invalid_monster_token'),('duplicate-after-restart','G08-Restart','200 duplicate sau restart API')]:
 r=h[n]; add(id,n,'POST /gameplay/events'+(' sau compose restart' if 'restart' in n else ''),expected,f"{r['http']} {r['body']}",'Pass' if r['pass_http'] else 'Fail')
add('G06-Atomic','Thưởng/lưu nguyên tử','test_scan_sessions.py: token, two workers, rollback','Không cộng lặp; rollback khi outbox lỗi','pytest token/restart/concurrency/rollback pass','Pass','pytest.xml','Windows pytest/SQLite tạm, detector stub')
add('G07-Queue','Lưu hàng chờ client/restart','Unity OutboxRestartBackoffAndFifo/CorruptQueuePreservedAsBackup','FIFO/backoff/lưu bền vững; backup corrupt','Các test Passed','Pass','unity-tests.json','Unity 6000.5 EditMode/fake HTTP/file tạm')
add('G07-Flush','Client gửi lặp/payload upgrade','ConcurrentFlushDoesNotSendTwice/OutboxRejectedOnlyOnce/InvalidUpgradeRejectedLocally','Không gửi trùng; lỗi chỉ reject một lần','Các test Passed','Pass','unity-tests.json','Unity EditMode/fake, không phải game thực')
add('G08-Fault','Cloud key sai không synced giả','Key sai trong os.environ tiến trình test; API event + worker/HTTP thật','recorded; retry rồi failed sau 8; không synced',f"recorded HTTP {fault['recorded_http']}; 32 send steps/4 feed; terminal {[(x['status'],x['attempts'],x['last_error']) for x in fault['outbox']]}",'Pass' if fault['pass'] else 'Fail','cloud-fault.json','Docker/DB tạm; now giả lập backoff; HTTP Adafruit thật, ≥3.2s giữa lần gửi')
add('G08-Recover','Key đúng hoạt động lại','Event mới trên stack dùng key gốc, GET event tới terminal','recorded rồi synced',str(good['history'][-1]),'Pass' if good['pass'] else 'Fail','cloud-correct.json')
add('G09-Feeds','Đọc feed Adafruit thật','GET /feeds/{feed}/data/last, key che','5 feed có giá trị',str(load('adafruit-feeds-final.json')),evidence='adafruit-feeds-final.json')
add('G09-Metrics','Chặn metrics ngoài nginx','GET :5001/metrics','403 hoặc 404',f"HTTP {h['external-metrics']['http']}")
targets=load('prometheus-targets-stable.json')[-1]['body']['data']['activeTargets']
add('G09-Prom','Prometheus target up','GET /api/v1/targets sau scrape','guardian-api up',str([(t['labels']['job'],t['health']) for t in targets]),'Pass' if any(t['health']=='up' for t in targets) else 'Fail','prometheus-targets-stable.json')
add('G09-Grafana','Grafana health','GET :3000/api/health','HTTP200 database ok',str(load('grafana-health.json')),evidence='grafana-health.json')
add('G09-NonRoot','Container non-root','docker compose exec API id','UID khác0','uid=10001 guardian',evidence='container-user.json')
add('B5-B7','Unity Backend suite','MCP run_tests EditMode','B5/B7 pass','40/40 Backend Passed; encoder/schema/mapping/fake/stabilizer/outbox',evidence='unity-tests.json',env='Unity 6000.5.8f1/EditMode')
add('UNITY-Other','Test có sẵn khác','MCP toàn EditMode','Không regression','15/15 Match3 Passed; tổng55/55',evidence='unity-tests.json',env='Unity/EditMode')
add('UNITY-Console','Console errors sau test','MCP read_console error, count100','Không error','0 error entries',evidence='unity-console.json',env='Unity/Console snapshot, không clear')
add('D2-Cleanup','Tắt stack giữ volume','compose down không -v','Containers ngừng/xóa; named volumes giữ',str(final['down_exit']),'Pass' if final['down_exit']==0 else 'Fail','cleanup.json')
for id,case,reason in [('G09-iPhone','Camera/AR thật iPhone','Không có phiên chạy trên thiết bị iPhone; không suy rộng EditMode thành AR thật'),('G07-Wallet','Mua thiếu Core/tối đa/bấm nhanh/lưu game đầy đủ','Chỉ kiểm hàng chờ client; chưa chơi end-to-end UI/wallet thật'),('G08-Network','Mất mạng vật lý cloud','Test lỗi cloud bằng key sai/HTTP401, chưa ngắt mạng thiết bị/router'),('G09-Export','Chạy lại export ONNX','D2 nạp artifact đã chốt, không chạy lại export/PyTorch')]:
 add(id,case,'Chưa chạy','Cần kiểm thử tương ứng',reason,'Chưa kiểm thử',env='Chưa có môi trường/ngoài phép đo D2')
counts=Counter(x[7] for x in rows)
# Keep historical G09 rows, clarify their provenance instead of overwriting them.
p=DOC/'Kiem_Thu_Backend.md'; old=p.read_text(encoding='utf-8')
if '\n## D2 —' in old: old=old.split('\n## D2 —')[0]
old=old.replace('# Kiểm thử backend — bổ sung G09/B2.6','# Kiểm thử backend — B2.6 và D2',1)
old=old.replace('Lượt D2 trước đã dừng trước khi chạy; nhánh origin/main chưa có bảng D2. Tài liệu này chỉ ghi kết quả B2.6 thực sự đã chạy, không xác nhận Docker/cloud/Unity của D2.','Phần B2.6 dưới đây là lịch sử đánh giá ảnh thật; kết quả chạy lại D2 ở bảng phía sau. Không đổi tiêu chí hoặc xóa các ca G09 cũ.')
lines=['','## D2 — chạy lại backend/cloud/AI/Docker và Unity','','Minh Nhật; ngày '+e['date']+'; commit `'+e['commit']+'` sau merge origin/main (không conflict). OS '+e['os']+'; CPU '+e['cpu']+'. Model YOLOv8n640, confidence0.70, pool cell phone/laptop/chair. [Manifest](Evidence/CampusGuardian/D2/environment.json).','',
       'Mỗi dòng bảng là một ca/nhóm ca báo cáo, không đồng nhất số dòng với số test pytest/Unity. Snapshot Prometheus ngay sau restart từng down; snapshot sau scrape up. Sync worker chạy nhưng không khai báo healthcheck. Test cloud dùng key sai là lỗi xác thực, không phải mô phỏng mất mạng vật lý; timer backoff được tăng tốc trên DB tạm, không sửa đồng hồ/DB stack chính.', '',
       '| ID | Ca kiểm thử | Nền tảng/môi trường | Commit | Bước | Mong đợi | Thực tế | Pass/Fail | Bằng chứng |','|---|---|---|---|---|---|---|---|---|']
for row in rows: lines.append('| '+' | '.join(str(v).replace('|','/').replace('\n',' ') for v in row)+' |')
historical=Counter(line.split('|')[-3].strip() for line in old.splitlines() if line.startswith('| G09-B2.6-'))
total=counts+historical
lines+=['',f"D2: **{counts['Pass']} Pass / {counts['Fail']} Fail / {counts['Chưa kiểm thử']} Chưa kiểm thử**. Toàn bộ bảng kể cả B2.6: **{total['Pass']} Pass / {total['Fail']} Fail / {total['Chưa kiểm thử']} Chưa kiểm thử**.",'',
        'Năm Fail lịch sử B2.6 là backpack, book, bottle, cell phone, chair theo kỳ vọng tìm đủ mọi ảnh có nhãn; không che kết quả sau khi thu hẹp pool. Recall 0.70: backpack0/5, book0/3, bottle0/5, cell phone3/5, chair4/6, laptop5/5; tổng12/29, pool12/16; none0/4 FP. Nguồn ảnh **không rõ/ảnh mạng** theo EXIF, không xác nhận ảnh tự chụp. Không thay model/ngưỡng hay sửa code trong D2.']
p.write_text(old.rstrip()+'\n'+'\n'.join(lines)+'\n',encoding='utf-8')
(P/'case-summary.json').write_text(json.dumps({'D2':counts,'historical_B2_6':historical,'all_rows':total},ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(dict(total))
# The original W9 PDF is not present in this checkout; distinguish project workflow from literal rubric.
lat=b['api_total_ms']; wall=b['wall_ms']; inf=b['inference_ms']
report=f'''# Bản nháp báo cáo kỹ thuật backend — Minh Nhật (D2)

Ngày {e['date']}; mã kiểm thử `{e['commit']}`. Nhánh `minhnhat/docs-testing` chứa B2.6 `64b1e2b`, B2.8 `6c69def`, merge origin/main không conflict tại `697be98`. Cấu hình: YOLOv8n 640 FP32, confidence ≥0.70, pool nhiễm `cell phone,laptop,chair`. Đây là bản nháp phần backend/cloud/Docker và thư viện client của Minh Nhật; không xác nhận nghiệm thu iPhone/AR thật.

## Kiến trúc và trách nhiệm

```mermaid
flowchart LR
    U[Unity PC / iOS] -->|HTTP JSON / JPEG| N[nginx :5001]
    N --> A[Flask / Gunicorn 2 workers]
    A --> O[ONNX Runtime CPU / YOLOv8n 640]
    A <--> S[(SQLite: events / rewards / scans / outbox)]
    S <--> W[Sync worker]
    W -->|HTTPS / X-AIO-Key| C[Adafruit IO]
    P[Prometheus] -->|internal /metrics| A
    G[Grafana] --> P
```

Flask xác thực payload và xử lý contract; Gunicorn phục vụ hai worker; ONNX session được nạp một lần mỗi worker. SQLite dùng chung lưu event, reward, phiên scan và outbox. Worker sync tách tiến trình đọc SQLite và đẩy snapshot lên Adafruit. nginx không công khai `/metrics`; Prometheus scrape trực tiếp API nội bộ; Grafana truy vấn Prometheus. Unity giữ khóa cloud ngoài client. Nguồn: `Server/app/`, `Server/docker-compose.yml`, `Server/deploy/`; kiểm chứng [D2/container-config.json](Evidence/CampusGuardian/D2/container-config.json), [D2/compose-ps.json](Evidence/CampusGuardian/D2/compose-ps.json).

## Năm bước triển khai W9 và mức đáp ứng

Bảng này dùng năm bước trong skill dự án `.agents/skills/w9-onnx-pipeline/SKILL.md`, không tự coi là trích nguyên văn W9-En.pdf (PDF không có trong checkout D2). Nội dung skill cũ về đồng bộ ngay từ predict phải đọc cùng kiến trúc event/outbox hiện tại: `/predict` thông thường không tự tăng lost-found-count.

| Bước | Dự án đáp ứng | Bằng chứng / giới hạn |
|---|---|---|
| 1. Chuẩn bị môi trường/cấu hình | Python3.11, .env backend, key tách client, model/ngưỡng/pool cố định | [environment.json](Evidence/CampusGuardian/D2/environment.json), [container-config.json](Evidence/CampusGuardian/D2/container-config.json); .env không commit/không sửa |
| 2. Xuất/nạp ONNX | Có export_onnx.py và artifact YOLOv8n; runtime nạp CPUExecutionProvider | [model-runtime.txt](Evidence/CampusGuardian/D2/model-runtime.txt): input1×3×640×640/hash; D2 không chạy lại export |
| 3. Docker hóa/triển khai | Compose build/start năm container, Gunicorn hai worker, API non-root | [docker-up.txt](Evidence/CampusGuardian/D2/docker-up.txt), [compose-ps.json](Evidence/CampusGuardian/D2/compose-ps.json). **Kubernetes chưa làm**, không có cluster/manifests được kiểm chứng |
| 4. API/giám sát/suy luận | health, predict, scan/events; Prometheus và Grafana hoạt động | [http.json](Evidence/CampusGuardian/D2/http.json), [benchmark.json](Evidence/CampusGuardian/D2/benchmark.json), [prometheus-targets-stable.json](Evidence/CampusGuardian/D2/prometheus-targets-stable.json) |
| 5. Adafruit IO | Event ghi SQLite rồi outbox retry/sync; đọc năm feed thật | [cloud-fault.json](Evidence/CampusGuardian/D2/cloud-fault.json), [cloud-correct.json](Evidence/CampusGuardian/D2/cloud-correct.json), [adafruit-feeds-final.json](Evidence/CampusGuardian/D2/adafruit-feeds-final.json) |

## Model, tiền xử lý và lựa chọn 640

Model pretrained COCO YOLOv8n 640 FP32; runtime chỉ lọc sáu lớp `book,laptop,chair,bottle,backpack,cell phone`; không phải model được train riêng sáu lớp. Ngưỡng tối thiểu 0.70. Nguồn model/license Ultralytics AGPL-3.0: `Server/models/README.md`, `AGPL-3.0.txt`; SHA256 `4d0632282232a5fe4eedf59a951f371d3d6b98e4d78546813f1d0ef07b06bb40` khớp kiểm tra container.

Detector letterbox ảnh về640, chuẩn hóa RGB/CHW, chạy ONNX và decode/NMS. Encoder Unity giới hạn cạnh dài640; evaluator B2.6 xoay EXIF trước resize, không upscale. Không khẳng định HTTP backend đã tự sửa mọi EXIF camera: client phải gửi ảnh đúng hướng. Không chạy YOLOv8s hoặc đổi threshold trong D2.

Chọn640 dựa vào quyết định [model-decision.md](Evidence/CampusGuardian/B2/model-decision.md): so sánh B2 nhỏ cho thấy 416/480 bỏ sót fixture laptop nhiều hơn, không đủ cơ sở đánh đổi độ đúng chỉ để giảm độ trễ. D2 kiểm tra model cố định, không benchmark lại các size khác và không coi mẫu nhỏ là chứng minh ưu thế tổng quát.

### Độ trễ đo thật trong container

Windows host: {e['cpu']}; container Linux/WSL2, CPUExecutionProvider, Gunicorn2worker và ORT8intra-op threads/worker. Một client tuần tự trong API container gọi HTTP localhost Gunicorn, 5warmup rồi50request; fixture laptop-stocksnap.jpg, không đi qua nginx. Đây không phải thử tải đồng thời hai client hoặc mạng iPhone.

| Chỉ số | p50 (ms) | p95 (ms) |
|---|---:|---:|
| API total_time_ms | {lat['p50']:.3f} | {lat['p95']:.3f} |
| HTTP wall (client trong container) | {wall['p50']:.3f} | {wall['p95']:.3f} |
| Chỉ inference | {inf['p50']:.3f} | {inf['p95']:.3f} |

So với B2.7 p50≤150ms/p95≤220ms cho API total: **{'Pass' if b['B2.7_pass'] else 'Fail'}** trên phép đo này. Không đồng nhất inference với API total hoặc cam kết latency mọi máy. Nguồn raw50mẫu: [benchmark.json](Evidence/CampusGuardian/D2/benchmark.json), command tại [benchmark-command.json](Evidence/CampusGuardian/D2/benchmark-command.json).

## Đánh giá ảnh thật B2.6 → pool B2.8

Đầu bài dự kiến35ảnh nhưng thực tế36; theo xác nhận người dùng loại3ảnh gốc>5MiB (book1/book2/none3), còn33ảnh:29dương và4none. Nhãn do người dùng xác nhận theo thư mục; `cellphone` ánh xạ `cell phone`. EXIF Orientation trước RGB/resize cạnh dài≤640, không upscale; không lưu GPS/commit ảnh. Nguồn toàn bộ ảnh đo là **không rõ/ảnh mạng** vì không có EXIF điện thoại, không khẳng định ảnh tự chụp.

| Lớp | Đúng/tổng tại0.70 | Recall | Trong pool nhiễm |
|---|---:|---:|---|
| cell phone | 3/5 | 60% | Có |
| laptop | 5/5 | 100% | Có |
| chair | 4/6 | 66.7% | Có |
| book | 0/3 | 0% | Không |
| bottle | 0/5 | 0% | Không |
| backpack | 0/5 | 0% | Không |

Tổng12/29(41.4%); riêng pool12/16(75%); none0/4dương giả. Tại0.50/0.60 là15/29(51.7%), không dùng để hạ ngưỡng mặc định. Nguồn [real-eval.md](Evidence/CampusGuardian/B2/real-eval.md), raw JSON và danh sách17ảnh trượt đi kèm. Mẫu nhỏ và ảnh mạng có thể dễ hơn camera lúc chơi; không có bbox ground truth/mAP. Pool được chọn sau khi xem tập này, cần tập độc lập để đánh giá tổng quát hóa.

B2.8 giới hạn lớp có thể nhiễm ở điện thoại/laptop/ghế. Backend chọn1–2lớp, clamp theo pool; gợi ý điện thoại/laptop cùng “đồ điện tử” không lặp, ghế “thứ để ngồi”. Pool rỗng hoặc chứa lớp ngoài ALLOWED_CLASSES gây lỗi khởi động rõ ràng. `/predict` vẫn nhận sáu lớp; bottle/book/backpack được nhận diện có scan_session_id thì clean trong pool mặc định. D2 thực sự phát hiện bottle3 và trả clean; laptop trả infested ở phiên ngẫu nhiên. Phiên cũ giữ hint/TTL đã lưu; lớp ngoài pool mới không reveal thêm, cần bắt đầu phiên mới sau đổi pool. Nguồn [http.json](Evidence/CampusGuardian/D2/http.json), [pytest.xml](Evidence/CampusGuardian/D2/pytest.xml).

## API

| Endpoint | Đầu vào / kết quả |
|---|---|
| GET /health | DB/model/cloud-configured/outbox;200 khi DB khỏe,503 khi degraded; phải xem model_loaded riêng |
| POST /predict | Multipart image; target_class/threshold tùy chọn, scan_session_id tùy chọn;200 detections/target_found/timing, scan result infested/clean/none khi có phiên;400/413/503 nếu lỗi |
| POST /scan/sessions | JSON session_id UUID;201 id/hint/max_monsters/expires_at;409 phiên mở,429 cooldown/limit |
| GET /scan/sessions/{{id}} | Số đã reveal/còn lại, hint/expiry;404 không có,410 hết hạn; không lộ danh sách lớp nhiễm |
| POST /gameplay/events | UUID event/session, quest/milestone/type/time/payload whitelist;201 recorded,200 duplicate,409 cùng id khác payload,400 invalid |
| GET /gameplay/events/{{id}} | Trạng thái event/core/cloud pending/failed/synced/disabled;404 nếu không có |
| GET /metrics | Chỉ nội bộ cho Prometheus; qua nginx trả404 |

Schema và validate ở `Server/app/routes/`, `event_validation.py`; output thật tại [http.json](Evidence/CampusGuardian/D2/http.json). Không có API đăng nhập hoặc quản lý account.

## Chống trùng, thưởng và chống farm

Event UUID và canonical payload hash được lưu SQLite: replay đúng payload trả duplicate, cùng ID khác nội dung trả409. Milestone cũ thưởng một lần theo session/milestone; AR combat cần monster_token do server phát hành và đúng game session. Reveal/token, event, aggregate, reward link và outbox được ghi giao dịch; token mới dùng lại trả400, duplicate event cũ không cộng thêm. Reward `ar_monster` là10Core theo `Server/config/rewards.json`. D2 ghi201/core10, duplicate200/core10 là kết quả đã ghi chứ không cộng10lần nữa; pytest kiểm tổng aggregate/token/concurrency/rollback. Restart API vẫn duplicate. Nguồn [http.json](Evidence/CampusGuardian/D2/http.json), [pytest.xml](Evidence/CampusGuardian/D2/pytest.xml).

Mặc định một phiên đang mở mỗi game session; cooldown120giây, tối đa5phiên/game session, TTL15phút. Gửi lại khi mở→409, cooldown/limit→429, hết hạn→410. Test HTTP sidecar dùng SQLite tạm và override TTL1.2s/cooldown3s/max2 để không chờ lâu; unit tests kiểm mặc định và concurrent create. Không mô tả sidecar như cấu hình production. Không có account nên không ngăn người dùng tự tạo game-session UUID mới. Nguồn [scan-boundaries.json](Evidence/CampusGuardian/D2/scan-boundaries.json), source `scan_store.py`.

## Docker, bảo mật và giám sát

Năm container: nginx, iuh-guardian-api, iuh-guardian-sync, Prometheus, Grafana. API/nginx/Prometheus/Grafana có healthcheck, **sync worker healthcheck disabled** nên chỉ xác nhận running. API chạy UID10001guardian; model mount read-only, SQLite host bind; Prometheus/Grafana dùng named volumes. D2 build/start thành công và cuối cùng `compose down` không `-v`, giữ named volumes và dữ liệu bind. Nguồn [docker-up.txt](Evidence/CampusGuardian/D2/docker-up.txt), [container-user.json](Evidence/CampusGuardian/D2/container-user.json), [cleanup.json](Evidence/CampusGuardian/D2/cleanup.json).

Key chỉ ở .env/env backend, .dockerignore loại .env/tests/data; không đưa key vào Unity hoặc evidence. D2 chỉ lưu giá trị feed/errors đã che key; checksum .env trước/sau xác nhận không sửa. `/metrics` bị nginx chặn404, scrape nội bộ vẫn up. Snapshot ngay sau restart API từng down do connection refused, sau scrape tiếp theo đã up; ghi cả hai snapshot, không sửa code. Grafana `/api/health`200/databaseok. Prometheus/Grafana bind localhost, anonymous Grafana Viewer trong compose; API game chưa có auth/rate limit theo tài khoản hoặc HTTPS. Nguồn [prometheus-targets.json](Evidence/CampusGuardian/D2/prometheus-targets.json), [prometheus-targets-stable.json](Evidence/CampusGuardian/D2/prometheus-targets-stable.json), [grafana-health.json](Evidence/CampusGuardian/D2/grafana-health.json).

## Cloud, feeds và outbox

Năm feed: guardian-quests-completed, guardian-campus-core, guardian-upgrades, lost-found-count (=scan_confirmed), guardian-monsters-revealed. Snapshot giá trị thật cuối lượt nằm trong [adafruit-feeds-final.json](Evidence/CampusGuardian/D2/adafruit-feeds-final.json); đây là dữ liệu telemetry kiểm thử, không phải số liệu người chơi. Không suy ra đồng bộ đúng chỉ từ cloud_configured=true.

Event ghi SQLite trước; outbox coalesce snapshot theo feed, claim/lease chống worker gửi trùng; tối đa8attempt, exponential backoff5s tới300s, hỗ trợ Retry-After/429, send interval3.1s (~19.35điểm/phút), trong budget30điểm/phút được quy định ở rules dự án. `cloud` chỉ synced khi toàn bộ outbox liên quan synced; failed nếu có job terminal failed. Durable cloud counters đọc từ SQLite để API scrape được số liệu worker riêng. Nguồn `event_store.py`, `cloud_client.py`, `sync_worker.py` và pytest outbox.

D2 dùng key sai **chỉ trong os.environ tiến trình test container**, SQLite tạm riêng: event201/pending, HTTPAdafruit401, retry rồi4job failed sau8attempt/job, không synced giả. `now` scheduler được tăng400s/lượt để kiểm terminal mà không đợi backoff thực nhiều phút; HTTP thật cách nhau≥3.2s. Đây là lỗi xác thực cloud, không phải thử ngắt mạng vật lý. Stack chính không đổi key/DB clock; event mới bằng key gốc đã recorded→synced. Job terminal failed trên DB tạm không được giả là tự hồi phục bằng đổi key; khôi phục kiểm trên event mới của stack chính. Nguồn [cloud-fault.json](Evidence/CampusGuardian/D2/cloud-fault.json), [cloud-correct.json](Evidence/CampusGuardian/D2/cloud-correct.json).

## Thư viện Unity B5/B7

B5 cung cấp cấu hình base URL, health/predict/events, JPEG encoder cạnh dài640, schema validation và durable client outbox/backoff/FIFO; reject payload sai và backup queue hỏng. B7 cung cấp tạo/đọc scan session, ánh xạ infested/clean/no-object/error, cancellation, ổn định frame và throttle. FakeScanService cho Editor/Development dùng label giả/token ổn định theo phiên, không gửi token giả lên backend; release từ chối fake. Thư viện không tham chiếu AR Foundation, phần ARCameraManager/device do tích hợp scene đảm nhiệm. Nguồn [Guardian_Backend_Unity.md](Guardian_Backend_Unity.md), source `Assets/IUH_Campus/Scripts/Guardian/Backend/`.

MCP chạy toàn EditMode:55/55Passed (40Backend B5/B7,15Match3),0fail/skip; Console snapshot0error. Đây là fake HTTP/local file/logic test, không phải Unity thật kết nối Docker hay iPhoneAR. Bằng chứng [unity-tests.json](Evidence/CampusGuardian/D2/unity-tests.json), [unity-console.json](Evidence/CampusGuardian/D2/unity-console.json).

## Kết quả và giới hạn

Server pytest134pass/0fail/0skip; có thông báo lỗi cleanup pyreadline3 trên Windows sau pytest, exit0. Unity55pass. Bảng D2 có {counts['Pass']}Pass/{counts['Fail']}Fail/{counts['Chưa kiểm thử']}Chưa kiểm thử; tính cả lịch sử G09B2.6: {total['Pass']}Pass/{total['Fail']}Fail/{total['Chưa kiểm thử']}Chưa kiểm thử. Năm Fail lịch sử là recall chưa tìm đủ mọi ảnh ở backpack/book/bottle/phone/chair, không được xóa chỉ vì thu hẹp pool. Chi tiết bước/mong đợi/thực tế/evidence: [Kiem_Thu_Backend.md](Kiem_Thu_Backend.md). Không sửa code/test sản phẩm trong D2.

Hướng phát triển: thu tập camera thật đa điều kiện và bbox ground truth, đánh giá độc lập/mAP và per-class recall; sách/balo/chai nhận kém nên cân nhắc fine-tune hoặc model lớn hơn trong thí nghiệm riêng, chưa thử/không đổi YOLOv8n. Thử iPhoneAR ngoài thực tế và Unity→Docker end-to-end, hoàn thiện test wallet/nâng cấp/lưu; bổ sung account/auth/chống farm xuyên phiên, HTTPS và kiểm soát truy cập; Kubernetes chưa làm; Android là hướng phát triển do chưa có thiết bị kiểm thử. Timing hiện tại là single-client CPU, không bảo đảm tải đồng thời hay mạng mobile. Chưa chạy lại export ONNX và chưa kiểm ngắt mạng vật lý trong lượt này.
'''
(DOC/'Bao_Cao_Ky_Thuat_Backend.md').write_text(report,encoding='utf-8')
