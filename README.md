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

### Chạy backend W9 B1 — Flask/Gunicorn

Backend dùng Python 3.11, cổng **5001**. Từ thư mục gốc:

```powershell
python -m venv Server/.venv
.\Server\.venv\Scripts\python.exe -m pip install -r Server/requirements-dev.txt
Copy-Item Server/.env.example Server/.env
.\Server\.venv\Scripts\python.exe -m flask --app Server/wsgi.py run --host 127.0.0.1 --port 5001
```

Local Windows dùng Flask development server để thử; production dùng Gunicorn trong Linux/Docker (Gunicorn không chạy native Windows). Sửa `MODEL_PATH` trong `Server/.env` thành đường dẫn model local nếu có; B1 chưa yêu cầu model. Không điền key thật vào `.env.example`, không commit `.env`.

```powershell
.\Server\.venv\Scripts\python.exe -m pytest Server/tests -q
docker compose -f Server/docker-compose.yml up -d --build
curl.exe http://localhost:5001/health
curl.exe http://localhost:5001/metrics
curl.exe -X POST http://localhost:5001/predict
docker compose -f Server/docker-compose.yml ps
docker compose -f Server/docker-compose.yml logs --tail 50
docker compose -f Server/docker-compose.yml down
```

Compose >= 2.24.0 hỗ trợ `.env` tùy chọn qua `required: false`; với Compose cũ, nâng cấp hoặc copy `.env.example` và đổi `env_file` thành `- .env`. Container mount `Server/models` chỉ đọc tại `/app/models`; `.onnx` không đưa vào Git hoặc Docker image. Dockerfile dựa trên Python 3.11 slim, không khóa kiến trúc; cần kiểm chứng build riêng trên amd64/arm64.

| Endpoint | Hành vi B1 |
|---|---|
| `GET /health` | 200; uptime theo giây từ lúc worker tạo app, cờ model loaded thật. 200 xác nhận service sống, không cam kết inference sẵn sàng |
| `GET /metrics` | Request count theo status, histogram latency và gauge `model_loaded` |
| `POST /predict` | Multipart field `image`; thiếu/ảnh lỗi: 400; ảnh hợp lệ nhưng chưa model: 503 `model_not_loaded`; model đã nạp nhưng chưa triển khai inference B2: 501 |

Không còn `/predict/student-card`, `/iot/parking-status` hoặc trang `/docs` FastAPI. B1 không trả kết quả nhận diện mô phỏng và chưa gửi Adafruit IO. Hai worker Gunicorn mỗi worker nạp một session khi tạo app, không nạp lại theo request. `/metrics` gom dữ liệu bằng Gunicorn multiprocess exporter; dọn file metrics cũ trong thư mục tạm khi master khởi động, đánh dấu worker chết qua hook `child_exit`. Gauge dùng giá trị nhỏ nhất của các worker còn sống.

Adafruit username/key chỉ đọc từ env ở B1, chưa sử dụng. Điện thoại cần IP LAN/URL backend truy cập được, không dùng `localhost` của PC; kiểm tra firewall và chính sách HTTP/HTTPS khi tích hợp. Bằng chứng và giới hạn kiểm thử: `Docs/Evidence/CampusGuardian/B1/`.

## Cloud và rubric

Mục tiêu: Unity → FastAPI kiểm tra/chống trùng sự kiện → Adafruit IO/dashboard. Key chỉ ở biến môi trường backend, không trong Unity/Git/log. Mất mạng vẫn chơi, lưu pending và retry có kiểm soát. Không cảm biến vật lý, không EXP/feed EXP.

Raycast PC không phải Deep Learning; nhận diện Android/overlay tím chưa tự đáp ứng AR. Nhóm cần chốt minh họa AR trên thiết bị tương thích và xác nhận yêu cầu AR/IO Cloud với giảng viên. Chỉ báo hoàn thành khi có bằng chứng thật.

## Lộ trình và phối hợp

Ba tuần: tuần 1 có hai build/quét/bàn cờ thử; tuần 2 hoàn thành combat/nâng cấp/cloud; tuần 3 test/tối ưu/build cuối/báo cáo. Một tuyến nhiệm vụ, một loại quái, một thử thách cao hơn sau nâng cấp là phạm vi bản đầu.

Đề xuất Gia Thịnh phụ trách Unity/gameplay/tích hợp AI/AR/hai build; Minh Nhật phụ trách backend/cloud/Docker/chống trùng và hỗ trợ UI/QA. Nhóm xác nhận, cân đối cuối tuần 1. Một người tích hợp scene tại một thời điểm; code/prefab qua PR.

Kiểm thử cả chuột lẫn cảm ứng, camera đúng/sai/quyền, chuỗi ghép và xáo, lượt quái/Skill, mua nâng cấp/thưởng lặp, lưu/restart/mất mạng, Docker/feed và checkout sạch. Ghi thiết bị, commit/build, Pass/Fail; bằng chứng mới dự kiến tại `Docs/Evidence/CampusGuardian/`.
