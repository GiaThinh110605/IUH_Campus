# D2 — Evidence index

- Minh Nhật; commit kiểm thử: `697be9883856d26f453475a9a8c8c3f133b9c75b`, sau merge origin/main `9a838cb` không conflict; không tạo nhánh mới/rebase/force.
- Ngày, OS, CPU và cấu hình: [environment.json](environment.json). Tất cả phép chạy trong ngày 2026-11-07, Asia/Bangkok.
- Toàn Server: [pytest.txt](pytest.txt), [pytest.xml](pytest.xml): 134 pass, 0 fail/error/skip. Lệnh: `Server/.venv/Scripts/python.exe -m pytest Server/tests -ra --junitxml=Docs/Evidence/CampusGuardian/D2/pytest.xml`. Có cleanup exception pyreadline3 sau pytest; exit 0.
- Docker build/start: [docker-up.txt](docker-up.txt), [compose-ps.json](compose-ps.json); 4 dịch vụ healthcheck healthy, sync worker running và không có healthcheck.
- API/scan/gameplay thật: [http.json](http.json), chạy bằng [live_checks.py](live_checks.py). JPEG fixture được xoay EXIF, thu nhỏ cạnh dài≤640 trước scan; không lưu ảnh. Phiên ngẫu nhiên không bị ép lớp nhiễm. Sau lỗi công cụ ghi đè trace HTTP khi kiểm cloud, smoke chính được chạy lại để lưu trace đầy đủ; các snapshot HTTP/benchmark hiện tại thuộc lượt chạy lại đó. Không dùng số benchmark cũ thiếu raw output để kết luận Pass.
- Benchmark cuối: [benchmark.json](benchmark.json), [benchmark-command.json](benchmark-command.json). 5 warmup+50 request tuần tự từ bên trong API container đến Gunicorn localhost. API total p50=96.215ms, p95=257.221ms: **Fail chỉ tiêu p95≤220ms**. Chưa profile nguyên nhân tail latency; không sửa/tune code và không chạy thêm để tìm số đạt.
- Cloud lỗi: [cloud-fault.json](cloud-fault.json), [cloud-fault-command.json](cloud-fault-command.json), script [cloud_fault.py](cloud_fault.py). API Flask test client thật + SQLite tạm trong container, không mock cloud HTTP. Key sai chỉ trong os.environ của tiến trình test. Bốn job HTTP401, tám attempt/job, retry→failed, không synced giả. Scheduler `now` tăng tốc400s/lượt trên DB tạm để không chờ backoff nhiều phút; các HTTP gửi thật cách nhau≥3.2s. Không thay giờ hoặc queue của stack chính.
- Key đúng: [cloud-correct.json](cloud-correct.json) ghi event mới trên stack chính pending→synced; không tuyên bố các job failed trong DB tạm tự hồi phục. Feed cuối: [adafruit-feeds-final.json](adafruit-feeds-final.json), chỉ lưu giá trị/errors, không lưu key. Telemetry kiểm thử có tác động tăng aggregate/feed thật, không phải dữ liệu người chơi.
- HTTP sidecar [scan-boundaries.json](scan-boundaries.json): 409/410/429 với TTL1.2s, cooldown3s, max2phiên; database tạm. Mặc định chính15phút/120s/max5 không đổi; unit tests kiểm mặc định. Script [scan_boundaries.py](scan_boundaries.py).
- Prometheus [prometheus-targets.json](prometheus-targets.json): snapshot ngay sau restart từng down; [prometheus-targets-stable.json](prometheus-targets-stable.json): up sau scrape. Grafana [grafana-health.json](grafana-health.json):200/databaseok. nginx /metrics:404 trong http.json.
- Unity MCP: [unity-start.json](unity-start.json), [unity-tests.json](unity-tests.json):55pass (40Backend B5/B7,15Match3),0fail/skip; [unity-console.json](unity-console.json):0error. Toàn EditMode, fake HTTP/file tạm, không phải test iPhone hoặc Unity live Docker.
- [container-config.json](container-config.json): YOLOv8n640, conf0.70, pool cell phone/laptop/chair,2worker,8threads. [model-runtime.txt](model-runtime.txt): ONNX input1×3×640×640/CPU/hash. [container-user.json](container-user.json):UID10001.
- Kết thúc bằng compose down không -v: [cleanup.json](cleanup.json), [docker-down.json](docker-down.json), [compose-ps-after-down.json](compose-ps-after-down.json). Named volumes giữ nguyên; Server/data là bind mount, không bị xóa; checksum .env trước/sau khớp.
- [case-summary.json](case-summary.json): D2 37Pass/1Fail/4Chưa kiểm thử; cộng8ca B2.6 giữ lại =40Pass/6Fail/4Chưa kiểm thử. Đây là số dòng bảng báo cáo, không phải số unit test.

Không có thay đổi code/test sản phẩm. Chỉ tài liệu, raw output và script tái lập D2 nằm trong phạm vi commit. Đánh giá ảnh thật B2.6 được trích từ evidence cũ, không chạy lại hay commit ảnh trong D2. Năm Fail độ đúng lịch sử được giữ nguyên; iPhone/AR thật, wallet end-to-end, ngắt mạng vật lý và export lại ONNX chưa kiểm trong lượt này. W9-En.pdf không có trong checkout, báo cáo dùng năm bước của skill dự án và ghi Kubernetes chưa làm.

Ghi chú định dạng: docker-up.txt chỉ bỏ khoảng trắng cuối dòng do Docker CLI sinh ra; không đổi nội dung dòng/output.

## B2.9 — đo lại trong điều kiện gameplay

Ngày 2026-11-07T17:00:38.201659+07:00; commit nền `a3e162d9d2e4fcddee5f30b10d6265616cbe2d5d`; host AMD Ryzen 7 6800H with Radeon Graphics, Windows-10-10.0.26200-SP0. Máy cắm sạc (AC=1), Unity Editor đã đóng; Edge/YouTube và cửa sổ Claude đã đóng trước đo. Các tiến trình nền còn lại được ghi ở host_before/host_after mỗi lượt trong JSON; không tuyên bố OS hoàn toàn không có tải nền. Docker/Codex được giữ. Không sửa code sản phẩm hay .env.

Ảnh nguồn laptop-stocksnap.jpg: [800, 533], 89706bytes; ảnh gameplay [640, 426], JPEG quality80, **43698bytes**. EXIF Orientation trước resize, cạnh dài≤640, không upscale, BILINEAR và kích thước làm tròn tương ứng OutputSize. Pillow JPEG80 cùng thông số với GuardianImageEncoder, không cam kết bit-identical với Unity EncodeToJPG/Graphics.Blit. Ảnh chỉ vào thư mục tạm/container /tmp, không commit.

Cấu hình mặc định đo thật: {'MODEL_PATH': '/app/models/yolov8n.onnx', 'ORT_INTRA_THREADS': 8, 'WEB_CONCURRENCY': 2, 'CONFIDENCE_THRESHOLD': 0.7, 'SCAN_INFECTED_POOL': 'cell phone,laptop,chair'}. Ba lần lặp mỗi (a)/(b):10warmup+200request tuần tự một client trong container gọi HTTP Gunicorn localhost (không nginx/LAN). Ba lần (c):hai client song song, mỗi client10warmup rồi chạy60giây; request bắt đầu trước deadline được phép kết thúc sau deadline. Không chạy các phép đo đồng thời với nhau. Chỉ (a) dùng chấm B2.7: p50≤150ms và p95≤220ms, yêu cầu cả ba lần đạt; không lấy trung bình percentile.

Raw samples, p50/p95/p99/max và số >220ms cho từng metric/từng lần: [benchmark-b29.json](benchmark-b29.json). Script: [benchmark_b29.py](benchmark_b29.py), [b29_client.py](b29_client.py).

### Tổng hợp mẫu (pooled raw samples ba lần; không trung bình percentile)

| Kịch bản | Metric | N | p50 ms | p95 ms | p99 ms | max ms | Số >220ms |
|---|---|---:|---:|---:|---:|---:|---:|
| (a) | api_total_ms | 600 | 59.347 | 74.290 | 87.467 | 137.256 | 0 |
| (a) | inference_ms | 600 | 49.108 | 62.732 | 77.024 | 124.170 | 0 |
| (a) | wall_ms | 600 | 60.969 | 76.061 | 88.948 | 139.531 | 0 |
| (b) | api_total_ms | 600 | 66.818 | 79.664 | 98.700 | 134.304 | 0 |
| (b) | inference_ms | 600 | 49.981 | 60.993 | 80.850 | 115.965 | 0 |
| (b) | wall_ms | 600 | 68.501 | 81.347 | 100.589 | 135.876 | 0 |
| (c) | api_total_ms | 2964 | 109.558 | 178.264 | 223.854 | 438.052 | 38 |
| (c) | inference_ms | 2964 | 94.996 | 161.499 | 205.166 | 416.807 | 17 |
| (c) | wall_ms | 2964 | 113.179 | 185.588 | 226.771 | 440.086 | 48 |

(a) ảnh gameplay≤640/JPEG80; (b) ảnh gốc chỉ so sánh; (c) ảnh gameplay/hai client/tải demo chỉ quan sát. Số >220ms đếm riêng từng metric; API total là chỉ số dùng đối chiếu SLA.

### Từng lần đo và thử ORT nếu được kích hoạt

| Kịch bản | ORT threads | Lần | N | API p50 | API p95 | API p99 | API max | API >220 | Lỗi request | B2.7 |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|
| (a) | 8 | 1 | 200 | 57.996 | 67.027 | 76.018 | 87.454 | 0 | 0 | Pass |
| (b) | 8 | 1 | 200 | 67.880 | 82.882 | 103.739 | 134.304 | 0 | 0 | Không chấm |
| (c) | 8 | 1 | 891 | 126.052 | 192.092 | 227.335 | 326.160 | 15 | 0 | Không chấm |
| (a) | 8 | 2 | 200 | 63.688 | 83.905 | 97.010 | 137.256 | 0 | 0 | Pass |
| (b) | 8 | 2 | 200 | 67.626 | 78.938 | 90.788 | 106.884 | 0 | 0 | Không chấm |
| (c) | 8 | 2 | 1034 | 106.210 | 162.274 | 206.086 | 272.894 | 7 | 0 | Không chấm |
| (a) | 8 | 3 | 200 | 57.620 | 65.351 | 71.506 | 74.108 | 0 | 0 | Pass |
| (b) | 8 | 3 | 200 | 65.104 | 74.176 | 82.600 | 106.991 | 0 | 0 | Không chấm |
| (c) | 8 | 3 | 1039 | 104.725 | 173.427 | 225.912 | 438.052 | 16 | 0 | Không chấm |

Lượt trước khi người dùng xác nhận thoát app có tiến trình Claude/Zalo/Unity Hub nền, được giữ ở benchmark-b29-precondition.json và không dùng chấm. Lượt chính chỉ bắt đầu sau xác nhận đã thoát; guard kiểm tra AC và không có Unity/Claude/Zalo/Unity Hub/Edge trước/sau mỗi lượt.

Không thử ORT4/6 vì cả ba lần gameplay mặc định đều đạt.

**Kết luận theo (a), mặc định ORT8: Pass.** (b)/(c) không dùng thay kết quả gameplay. Nếu kích hoạt ORT4/6, chúng chạy bằng env trên container tạm của cùng image/config, ba lượt(a) mỗi cấu hình; container thử được xóa, mặc định stack không thay đổi. Không tự chọn cấu hình nhanh nhất làm mặc định.

D2 trước dùng ảnh gốc chưa resize và Unity Editor vẫn mở; p50=96.215ms/p95=257.221ms bị Fail. Giữ nguyên dòng Fail và raw benchmark.json cũ. B2.9 thay điều kiện đo theo gameplay; khác điều kiện không chứng minh định lượng riêng tác động của Unity hay resize, cũng không xóa kết quả cũ.
