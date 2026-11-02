# IUH Campus Guardian

**Bảo vệ khuôn viên thông minh** — game phiêu lưu 3D kết hợp chiến đấu match-3 theo lượt, hướng đến **Windows PC và Android**.

Khám phá campus → quét nguồn nguyền → trận match-3 → thanh tẩy → nhận Campus Core → nâng cấp. Scene chính: `Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity`. Cấu trúc chọn 6 minigame được thay bằng một game thống nhất.

## Thiết kế mục tiêu

- PC bật camera quét trong game để thấy khói tím và khối dữ liệu lỗi trên vật thể bị nguyền; raycast xác nhận mục tiêu.
- Android mở camera thật nhận diện vật thể tương ứng theo nhiệm vụ, rồi đóng camera về campus để mở trận. Không cần hiệu ứng bám trên đồ vật thật; webcam PC chưa cam kết.
- Trận màn hình ngang: **nhân vật trái, bàn cờ giữa, quái phải**. Đề xuất bàn 6×6, bốn ô kiếm/khiên/năng lượng/hồi phục, một Skill.
- Đổi sai không mất lượt; chuỗi rơi trong cùng lượt, quái báo trước và phản công sau khi bàn ổn định; hết nước đi tự xáo.
- **Campus Core** nâng Tấn công, Phòng thủ, Sinh lực; mỗi nhánh 2 cấp bản đầu, chỉ mua ngoài trận. Bỏ EXP; trang phục/ngoại hình là mở rộng sau.
- Lưu Core, nâng cấp và khu vực thanh tẩy; chống thưởng trùng theo mốc và sự kiện. Hai nền tảng chưa bao gồm tài khoản/cloud save giữa thiết bị.

## Trạng thái và tài liệu

Cập nhật 01/11/2026: đang chuyển hướng thiết kế. PR tài liệu này không triển khai gameplay hoặc xác nhận Guardian đã chạy trên hai nền tảng. Checkout làm việc có dọn scene/menu local nhưng flow/UI cũ vẫn cần chuyển đổi; không dùng bằng chứng minigame cũ cho Guardian.

- [Kế hoạch chi tiết, phân công đề xuất và nghiệm thu](plan.md).
- [Tóm tắt gửi nhóm và rubric gốc](plan.txt).
- [Báo cáo đề xuất đã cập nhật](Docs/IUH_Campus_Guardian_Bao_Cao_De_Xuat.docx).
- [Dashboard cũ](IUH_Plan_Dashboard.html) chưa phản ánh thiết kế mới.

## Mở project và backend hiện có

Mở repository bằng Unity Hub với **Unity 6000.5.8f1** theo `ProjectSettings/ProjectVersion.txt`. Android cần Android Build Support/SDK/NDK/OpenJDK phù hợp. Mở scene SharedCampus ở trên, chờ import và kiểm tra Console trước Play. Không chạy builder cũ để tái tạo campus. Giữ `.meta` với asset; audit dependency trước khi xóa scene/script/pack.

### Chạy backend W9 B2 — YOLOv8n ONNX + Flask/Gunicorn

Backend runtime dùng Python 3.11, cổng **5001**, không cài torch. Model pretrained COCO `Server/models/yolov8n.onnx` được commit (~12,85 MB), không train lại. License Ultralytics **AGPL-3.0**, bản license và SHA256 nằm trong `Server/models/README.md`.

```powershell
python -m venv Server/.venv
.\Server\.venv\Scripts\python.exe -m pip install -r Server/requirements-dev.txt
Copy-Item Server/.env.example Server/.env
```

Local: sửa `MODEL_PATH` trong `Server/.env` thành đường dẫn tuyệt đối tới `Server/models/yolov8n.onnx`. Docker giữ mặc định `/app/models/yolov8n.onnx`. Không commit `.env` hoặc key thật. Lớp nhiệm vụ mặc định: `book,laptop,chair,bottle,backpack,cell phone`; đổi bằng `ALLOWED_CLASSES`. ID được giải từ metadata model, không ánh xạ ID thủ công. Ngưỡng tối thiểu `CONFIDENCE_THRESHOLD=0.70`, ORT CPU dùng `ORT_INTRA_OP_THREADS=2` mặc định.

```powershell
.\Server\.venv\Scripts\python.exe -m flask --app Server/wsgi.py run --host 127.0.0.1 --port 5001
.\Server\.venv\Scripts\python.exe -m pytest Server/tests -q
docker compose -f Server/docker-compose.yml up -d --build
curl.exe http://localhost:5001/health
curl.exe -F "image=@Server/tests/fixtures/laptop-stocksnap.jpg" -F "target_class=laptop" http://localhost:5001/predict
curl.exe http://localhost:5001/metrics
docker compose -f Server/docker-compose.yml ps
docker compose -f Server/docker-compose.yml logs --tail 50
docker compose -f Server/docker-compose.yml down
```

Flask development server chỉ dùng thử local; Gunicorn chạy trong Linux/Docker. Compose >= 2.24.0 hỗ trợ `.env` tùy chọn qua `required: false`; Compose cũ cần nâng cấp hoặc copy `.env.example` rồi dùng `env_file: - .env`. Container mount `Server/models` chỉ đọc. Dockerfile multi-arch Python slim; B2 kiểm chứng môi trường linux/amd64, chưa nghiệm thu arm64.

**Export lại model** trong venv riêng (torch CPU không vào runtime image):

```powershell
python -m venv Server/.venv-export
.\Server\.venv-export\Scripts\python.exe -m pip install -r Server/requirements-export.txt
.\Server\.venv-export\Scripts\python.exe Server/export_onnx.py
.\Server\.venv\Scripts\python.exe Server/benchmark.py --model Server/models/yolov8n.onnx --image Server/tests/fixtures/laptop-stocksnap.jpg
```

Export 640, batch 1, opset 17, simplify; in MB/SHA256/shape/classes và tạo `yolov8n.classes.json` từ metadata. `--imgsz 480` hoặc `416` tạo model riêng để so sánh, không đổi mặc định 640. Benchmark: 5 warm-up + 50 lượt, p50/p95/mean của session.run và pipeline. Đo trong container bằng `docker compose exec` sau khi copy fixture vào `/tmp`; xem lệnh/output đầy đủ trong Evidence B2.

**POST /predict**: multipart field `image`, tùy chọn `target_class` và `threshold`. Threshold phải hữu hạn, từ ngưỡng cấu hình đến 1. Chỉ trả lớp cho phép với confidence đạt ngưỡng. Letterbox RGB 640, normalize float32 NCHW, NMS theo lớp, bbox pixel ảnh gốc. Không tự xoay theo EXIF; client gửi ảnh đúng hướng. Ảnh lớn được resize về kích thước input trước session.run.

| Field JSON 200 | Ý nghĩa |
|---|---|
| `detections[]` | `label`, `class_id`, `confidence`, `bbox:[x,y,w,h]`; góc trên-trái, pixel ảnh gốc |
| `target_class` | Lớp được yêu cầu hoặc null |
| `target_found` | Có detection khớp target ở ngưỡng; false nếu không truyền target |
| `best_confidence` | Confidence cao nhất khớp target, hoặc cao nhất toàn bộ khi target null; null nếu không có |
| `threshold` | Ngưỡng thực tế dùng |
| `image_size` | `[width,height]` pixel ảnh gốc |
| `inference_time_ms` | Chỉ thời gian `session.run` |
| `total_time_ms` | Parse/đọc ảnh + tiền xử lý + inference + hậu xử lý, không gồm network |
| `model` | `name` và `sha256_prefix` 8 ký tự |

Lỗi JSON: thiếu/ảnh không đọc được, target/threshold sai → 400; upload trên 5 MiB → 413; chưa nạp model → 503 `model_not_loaded`. `/health` 200 là liveness, cần kiểm tra thêm `model_loaded`; uptime là số giây từ lúc worker tạo app. Mỗi worker nạp session một lần. Không còn endpoint giả lập student-card/parking hay `/docs` FastAPI.

`/metrics`: request count theo status, HTTP latency, `model_loaded` (min của worker sống), `inference_seconds`, `detections_total{label}`, `predict_target_found_total`. Multiprocess exporter gom hai worker Gunicorn; master dọn database metrics cũ trong thư mục tạm, `child_exit` loại gauge worker chết. Không log INFO theo từng inference ngoài access log.

B1 đã nghiệm thu Docker; B2 chạy inference và ghi benchmark thật tại `Docs/Evidence/CampusGuardian/B2/`. Không suy ra đạt <45 ms từ việc model đã chạy; xem số liệu và tradeoff 640/480/416 trong evidence. Adafruit IO chưa tích hợp ở B2. Điện thoại cần IP LAN/URL backend, không dùng localhost của PC.

## Cloud và rubric

Mục tiêu: Unity → FastAPI kiểm tra/chống trùng sự kiện → Adafruit IO/dashboard. Key chỉ ở biến môi trường backend, không trong Unity/Git/log. Mất mạng vẫn chơi, lưu pending và retry có kiểm soát. Không cảm biến vật lý, không EXP/feed EXP.

Raycast PC không phải Deep Learning; nhận diện Android/overlay tím chưa tự đáp ứng AR. Nhóm cần chốt minh họa AR trên thiết bị tương thích và xác nhận yêu cầu AR/IO Cloud với giảng viên. Chỉ báo hoàn thành khi có bằng chứng thật.

## Lộ trình và phối hợp

Ba tuần: tuần 1 có hai build/quét/bàn cờ thử; tuần 2 hoàn thành combat/nâng cấp/cloud; tuần 3 test/tối ưu/build cuối/báo cáo. Một tuyến nhiệm vụ, một loại quái, một thử thách cao hơn sau nâng cấp là phạm vi bản đầu.

Đề xuất Gia Thịnh phụ trách Unity/gameplay/tích hợp AI/AR/hai build; Minh Nhật phụ trách backend/cloud/Docker/chống trùng và hỗ trợ UI/QA. Nhóm xác nhận, cân đối cuối tuần 1. Một người tích hợp scene tại một thời điểm; code/prefab qua PR.

Kiểm thử cả chuột lẫn cảm ứng, camera đúng/sai/quyền, chuỗi ghép và xáo, lượt quái/Skill, mua nâng cấp/thưởng lặp, lưu/restart/mất mạng, Docker/feed và checkout sạch. Ghi thiết bị, commit/build, Pass/Fail; bằng chứng mới dự kiến tại `Docs/Evidence/CampusGuardian/`.
