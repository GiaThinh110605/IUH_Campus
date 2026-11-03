# Kế hoạch IUH Campus Guardian

Cập nhật 01/11/2026. Phạm vi phát triển: 3 tuần kể từ ngày nhóm chốt bắt đầu; nhóm điền ngày bắt đầu và hạn nộp thực tế.

## 1. Thiết kế đã thống nhất

Một game phiêu lưu 3D kết hợp chiến đấu giải đố match-3 theo lượt, chạy trên **Desktop (Windows PC, macOS) và Mobile (Android, iOS)**. Giữ bản đồ `Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity`; loại bỏ luồng chọn 6 minigame. Những hoạt động cũ không còn là các chế độ độc lập trong phạm vi mới.

Vòng chơi: khám phá campus → nhận đầu mối → quét vật thể → xác nhận nguồn nguyền → thanh tẩy bằng match-3 → nhận Campus Core → nâng cấp → thử thách tiếp theo.

Bản tối thiểu trong 3 tuần: một tuyến nhiệm vụ hoàn chỉnh, một loại Data Monster, một bàn cờ và một trận có thể chơi lại ở mức thử thách cao hơn. Các khu vực/nhiệm vụ tiếp theo là mở rộng sau khi bản tối thiểu đạt nghiệm thu.

## 2. Vì sao quét vật thể và Quy trình Deep Learning (Tuân thủ chuẩn W9-En.pdf)

Data Monster làm nhiễm các nút dữ liệu campus. Nhiệm vụ cho biết vật thể cần tìm và lý do, ví dụ: “Dữ liệu thư viện bị nhiễu. Quét một quyển sách để truy vết nguồn nguyền.” Vật thể tương ứng đóng vai trò điểm cộng hưởng mở kết nối đến nguồn nguyền.

Trên Desktop (Windows PC và macOS), vật thể trong scene bình thường không hiện lời nguyền. Khi bật chế độ quét của camera game mới thấy khói tím và các khối dữ liệu vỡ; giữ tâm ngắm vào mục tiêu hợp lệ để xác nhận. Âm thanh/HUD và đầu mối nhiệm vụ gợi ý khu vực cần quét.

Trên Mobile (Android và iOS), quy trình nhận diện tuân thủ đầy đủ **5 bước chuẩn triển khai Deep Learning Production theo tài liệu W9-En.pdf**:
1. **Xuất mô hình sang định dạng ONNX (`export_onnx.py`)**: Mô hình Deep Learning nhận diện vật thể (YOLOv8 / MobileNet) được xuất sang file `.onnx` tối ưu hóa, lưu trữ tại thư mục `Server/models/lost_found_yolo.onnx`. Định dạng ONNX chuẩn hóa đồ thị suy luận, giảm dung lượng và tương thích cao khi chạy đa nền tảng.
2. **Xây dựng REST API suy luận với Flask & Gunicorn**: Xây dựng ứng dụng REST API nhẹ bằng Flask (theo khuyến nghị W9) chạy qua WSGI HTTP server chuẩn production (Gunicorn). Sử dụng thư viện **ONNX Runtime** để thực thi suy luận với chỉ tiêu vận hành YOLOv8n 640: API total p50 ≤ 150 ms, p95 ≤ 220 ms trên CPU (Ryzen 7 6800H, Docker, Gunicorn 2 worker); confidence ≥ 0.70. Ưu tiên độ chính xác; 416 nhanh hơn nhưng trượt nhiều hơn trên mẫu thử B2. Endpoint chính: `POST /predict` tiếp nhận khung hình từ camera điện thoại, nhận diện các lớp vật thể học tập thực tế trong khuôn viên IUH (`book` - sách giáo trình, `laptop` - máy tính xách tay, `chair` - ghế phòng lab, `bottle` - bình nước, `backpack` - ba lô sinh viên, `cell phone` - điện thoại). API trả về nhãn đối tượng, hộp bao `[x, y, w, h]` và confidence score.
3. **Đóng gói ứng dụng ML vào Docker container**: Cung cấp `Dockerfile` (Python slim, ONNX Runtime, Flask, Gunicorn, Prometheus exporter) và `docker-compose.yml` để đóng gói toàn bộ backend ML, khởi chạy dễ dàng với lệnh duy nhất `docker compose up -d`, kèm cấu hình Healthcheck tự động tại `/health`.
4. **Sẵn sàng mở rộng với Kubernetes (Optional)**: Thiết kế API dạng stateless, sẵn sàng cho việc nhân rộng (scale pod) và phân tải thông qua manifest Kubernetes khi cần mở rộng dịch vụ.
5. **Giám sát hệ thống với Prometheus Exporter**: Tích hợp `prometheus_flask_exporter` theo đúng yêu cầu W9, cung cấp endpoint `/metrics` công khai để thu thập số liệu giám sát: tổng số lượt gọi API, thời gian suy luận (latency histogram), mã phản hồi HTTP 200/400/500, trạng thái model loaded.

Quy tắc nghiệm thu quét vật thể: Chỉ xác nhận hoàn thành khi đối tượng nhận diện đúng lớp yêu cầu nhiệm vụ, độ tin cậy đạt ngưỡng (confidence > 70%) và ổn định khung hình. Khi xác nhận thành công, màn hình đóng camera, kích hoạt hiệu ứng thanh tẩy, đưa người chơi trở về campus và mở trận chiến match-3. Không cần hiệu ứng bám phức tạp lên vật thể thật ngoài đời. Model nhận diện loại đồ vật thực tế, không phát hiện lời nguyền ảo.

Các nền tảng cùng sinh một kết quả xác nhận mục tiêu cho nhiệm vụ:
- Desktop (PC/Mac) dùng raycast camera nội bộ trong scene, không cần gọi Deep Learning; webcam PC/Mac không thuộc phạm vi bắt buộc.
- Mobile (Android/iOS) bắt buộc gửi ảnh qua REST API để Docker Backend chạy ONNX Runtime suy luận thật, có xử lý các tình huống ngoại lệ: sai lớp, độ tin cậy thấp (< 70%), thiếu sáng, người dùng từ chối cấp quyền camera và không tìm thấy vật thể.

## 3. Combat match-3

Bố cục màn hình ngang: **nhân vật chính bên trái – bàn cờ ở giữa – Data Monster bên phải**. Hai model quay mặt vào nhau; campus làm nền. Dành diện tích lớn nhất cho bàn cờ, kiểm thử độ lớn ô trên thiết bị thật. Desktop (PC/Mac) thao tác chuột/trackpad, Mobile (Android/iOS) thao tác chạm/kéo hoặc nút ảo; dùng chung logic bàn cờ.

Thiết kế khởi đầu để thử nghiệm: bàn 6×6, bốn loại ô, một Skill; không combo hành động, hitbox cận chiến, nhiều vũ khí hoặc nhiều quái trong bản đầu.

| Ô | Hiệu ứng |
|---|---|
| Kiếm | Gây sát thương cho quái |
| Khiên | Tạo giáp hấp thụ sát thương |
| Năng lượng | Tích năng lượng để dùng Skill |
| Hồi phục | Hồi máu nhân vật đến giới hạn tối đa |

Quy tắc bàn cờ và lượt:

- Đổi hai ô liền kề theo cạnh; bộ từ 3 ô cùng loại theo hàng/cột mới hợp lệ. Đổi sai trả về, không mất lượt.
- Khóa input khi đang đổi, xóa, rơi hoặc xử lý hiệu ứng. Bộ giao nhau không tính một ô hai lần.
- Xử lý đủ chuỗi rơi liên tiếp trong cùng lượt; quái chỉ phản công một lần khi bàn ổn định và còn sống.
- Bàn ban đầu không có bộ ghép sẵn và phải có nước đi. Hết nước đi tự xáo lại, không trừ lượt; xử lý chuỗi có giới hạn để tránh treo.
- Quái báo trước đòn tiếp theo, giúp người chơi chọn tấn công, giáp hoặc hồi phục. Không giới hạn thời gian lượt trong bản đầu.
- Skill tiêu năng lượng, chỉ dùng ở lượt người chơi khi bàn ổn định. Đề xuất bản đầu: đòn năng lượng đơn mục tiêu, dùng Skill kết thúc lượt và quái phản công nếu còn sống.
- Đề xuất giáp hấp thụ đòn phản công kế tiếp, phần dư hết sau đòn; năng lượng và giáp đặt lại khi bắt đầu trận. Thông số sát thương, máu, ngưỡng Skill và hệ số ghép 4/5 ô được chốt qua chơi thử tuần 1–2.
- Quái hết máu thì dừng lượt, không phản công; người chơi hết máu thì thua. Kết quả khóa tương tác bàn cờ.

Các quy tắc chi tiết và kích thước 6×6 là cấu hình khởi đầu đề xuất, được điều chỉnh qua thử nghiệm; bố cục ba phần và hướng combat match-3 là quyết định đã thống nhất. Cách quay lại sau khi thua cần nhóm chốt; đề xuất nút Thử lại trận để giảm thao tác demo.

## 4. Phần thưởng và nâng cấp

**Campus Core là tài nguyên duy nhất của bản đầu; bỏ EXP.** Thắng trận thanh tẩy làm mất khói tím, khôi phục trạng thái khu vực và trao Core đúng một lần cho lượt hoàn thành hợp lệ.

| Nhánh nâng cấp | Tác dụng | Giới hạn ban đầu |
|---|---|---|
| Tấn công | Tăng sát thương từ ô kiếm | 2 cấp |
| Phòng thủ | Tăng giáp từ ô khiên | 2 cấp |
| Sinh lực | Tăng máu tối đa | 2 cấp |

Chỉ nâng cấp ngoài trận; UI hiển thị chỉ số hiện tại → sau nâng cấp, giá Core, số Core còn lại và khóa khi thiếu tiền/đạt tối đa. Trừ Core và tăng cấp phải được lưu cùng nhau, không cho mua lặp do bấm nhanh. Giữ cấu hình giá/mức tăng riêng để cân bằng, không ghi số chưa thử thành kết quả đã kiểm chứng.

Lưu Core, cấp nâng cấp, khu vực đã thanh tẩy và sự kiện chờ gửi. Bản đầu có trận thử thách cao hơn để thử sức mạnh sau nâng cấp. Đề xuất Core chỉ trao một lần cho mỗi mốc nhiệm vụ/độ khó; chơi lại mốc đã nhận không thưởng thêm. Retry gửi cùng `event_id`, không tạo phần thưởng mới. Trang phục/ngoại hình chỉ làm sau khi các phần bắt buộc đạt; không nằm trong cam kết 3 tuần.

## 5. Kiến trúc và hiện trạng

Kiến trúc luồng xử lý toàn diện theo chuẩn W9-En.pdf:
`Unity Client (Desktop PC/Mac & Mobile Android/iOS) ➔ Quét vật thể (Camera Mobile / Raycast Desktop) ➔ [Nếu Mobile: Gọi REST API Flask POST /predict] ➔ [Docker Container: Gunicorn WSGI + ONNX Runtime nạp models/lost_found_yolo.onnx] ➔ Trả về JSON nhãn, bounding box [x,y,w,h], confidence score (>70%) ➔ Unity Client xác nhận và chuyển vào Match-3 Combat ➔ Kết thúc trận: Thưởng/Nâng cấp lưu cục bộ ➔ Gửi sự kiện POST /gameplay/events ➔ Đồng bộ Adafruit IO feeds & Thu thập số liệu Prometheus (/metrics)`.

Hồ sơ mã nguồn Backend & AI:
- `Server/export_onnx.py`: Script xuất mô hình tiền huấn luyện (YOLOv8 / MobileNet) sang định dạng `.onnx` chuẩn hóa.
- `Server/models/lost_found_yolo.onnx`: File trọng số ONNX tối ưu cho môi trường suy luận Production.
- `Server/app_flask.py`: Ứng dụng REST API Flask, tích hợp Gunicorn WSGI, ONNX Runtime (`/predict`), Prometheus exporter (`/metrics`), Healthcheck (`/health`) và kết nối Adafruit IO REST API.
- `Server/Dockerfile` & `Server/docker-compose.yml`: Cấu hình containerization triển khai với 1 câu lệnh `docker compose up -d`.

Theo rà soát checkout làm việc ngày 01/11/2026, Build Settings đã có thay đổi local để chỉ giữ SharedCampus, scene/menu cũ có backup và thay đổi dọn. Cần triển khai hoàn chỉnh luồng runtime kết nối giữa Unity Client và Docker AI Backend để xác nhận hoạt động đầu-cuối trên Desktop và Mobile.

## 6. Gói triển khai và phân công đề xuất

| Gói | Phạm vi và bàn giao | Phụ trách đề xuất |
|---|---|---|
| Scene/flow/input | SharedCampus, Build Settings, `Scripts/Core/IUHGameFlowManager.cs`, UI/Hub; Menu → Playing → Paused → Result; bỏ callback minigame cũ; hỗ trợ cả Desktop (Windows, macOS) & Mobile (Android, iOS) | Gia Thịnh |
| Match-3/combat | Module dự kiến `Assets/IUH_Campus/Scripts/Guardian/`; logic bàn cờ tách hoạt ảnh/UI, lượt quái/Skill, bố cục ngang | Gia Thịnh |
| Tiến trình/UI nâng cấp | Lưu Core/cấp/khu vực, mua nguyên tử, chống thưởng lặp, thử thách sau nâng cấp | Gia Thịnh; Minh Nhật hỗ trợ UI/QA |
| Camera/AI/AR và build | PC/Mac raycast, Mobile camera capture gửi REST API `/predict`, vẽ bounding box UI, AR Foundation demo, tối ưu, các bản build (Windows, macOS, Android/iOS) | Gia Thịnh tích hợp; cả hai chọn/test model |
| Backend/cloud/Docker (Chuẩn W9) | Script `export_onnx.py`, Flask REST API `/predict`, Gunicorn WSGI, ONNX Runtime inference, Prometheus Exporter `/metrics`, Dockerfile, docker-compose.yml, feeds Adafruit IO | Minh Nhật |
| Kiểm thử/báo cáo/demo | Test trên các nền tảng (PC Windows, macOS, Mobile), test 5 bước W9-En.pdf, ảnh/video đúng build, hướng dẫn và giới hạn | Cả hai |

Nhóm xác nhận phân công, cân đối tải Unity vào cuối tuần 1. Một người tích hợp scene tại một thời điểm; người còn lại giao code/prefab qua PR. Chỉ xóa scene/script/asset sau audit dependency; giữ `.meta` và cảnh quan đã duyệt. Không commit toàn bộ pack chỉ để dùng một prefab.

API dự kiến:
- `POST /predict`: Nhận file ảnh multipart/form-data từ camera, ONNX Runtime suy luận phát hiện đồ vật, trả về JSON nhãn, bounding box `[x, y, w, h]`, confidence score và `inference_time_ms`.
- `GET /metrics`: Prometheus exporter cung cấp metrics giám sát (request count, latency histogram, HTTP status codes) phục vụ nghiệm thu W9.
- `GET /health`: Kiểm tra trạng thái hoạt động của service và cờ `model_loaded`.
- `POST /gameplay/events`: Nhận `event_id`, `session_id`, `quest_id`, `milestone_id`, `event_type`, `occurred_at`; backend kiểm tra sự kiện và quy tắc thưởng, không tin giá trị thưởng tùy ý từ client. Chống trùng lưu bền vững qua restart; phân biệt đã ghi nhận và đã đồng bộ cloud.

Feeds dự kiến trên Adafruit IO: `guardian-quests-completed`, `guardian-campus-core`, `guardian-upgrades` (và feed giám sát AI `lost-found-count`); chốt namespace và ý nghĩa trước tích hợp. Không có feed EXP trong thiết kế mới. Dashboard không thay kho tiến trình. Key chỉ ở biến môi trường backend, không trong Unity/Git/log. Mất mạng vẫn chơi được; lưu pending và retry có giới hạn, báo lỗi đúng. Không dùng cảm biến vật lý.

## 7. Lịch 3 tuần

| Mốc | Công việc | Điều kiện qua mốc |
|---|---|---|
| Tuần 1, ngày 1–2 | Chốt thiết bị, lớp/model, AR, bảng chỉ số bàn cờ, API W9 và phân công | Có phạm vi và hợp đồng tích hợp |
| Tuần 1, ngày 3–7 | Flow mới; chạy `export_onnx.py` xuất mô hình `.onnx`, dựng Flask API `/predict` và Prometheus `/metrics`, đóng gói Docker `docker compose up -d`; PC/Mac quét, Mobile camera; bàn cờ đổi/ghép/rơi; thử feed; build các nền tảng | Desktop (Windows/macOS) chạy, Mobile cài máy thật, một mục tiêu mở trận; có bằng chứng ONNX inference trong Docker và Prometheus `/metrics` |
| Tuần 2, ngày 8–14 | Quái, bốn ô/Skill, thưởng/nâng cấp/lưu, thử thách cao hơn; client Mobile kết nối suy luận ONNX API đạt ngưỡng > 70%, đồng bộ feed Adafruit IO, cloud/retry | Chơi hết tuyến trên cả Desktop (PC/Mac) và Mobile, nâng cấp có tác dụng, feed cập nhật, không thưởng lặp |
| Tuần 3, ngày 15–18 | Sửa lỗi, tối ưu Mobile & macOS/PC, benchmark ONNX: YOLOv8n 640: API total p50 ≤ 150 ms, p95 ≤ 220 ms trên CPU (Ryzen 7 6800H, Docker, Gunicorn 2 worker); confidence ≥ 0.70. Ưu tiên độ chính xác; 416 nhanh hơn nhưng trượt nhiều hơn trên mẫu thử B2.; thử AI/AR, backend/Docker/Prometheus và checkout sạch | Ca bắt buộc đạt, lỗi còn lại có người xử lý |
| Tuần 3, ngày 19–21 | Đóng băng tính năng, các bản build cuối (PC/Mac, Mobile), video minh chứng 5 bước W9-En.pdf (export ONNX, containerize Docker, suy luận Flask/Gunicorn, Prometheus monitoring, Adafruit IO), báo cáo và diễn tập | Cài mới/chơi lại được, tài liệu khớp build |

Nếu tiến độ chậm, giảm lớp nhận diện, hiệu ứng và nội dung phụ; không âm thầm bỏ Mobile hoặc gọi mô phỏng là AI/cloud thật. Giữ trang phục ngoài phạm vi.

## 8. Kiểm thử và hồ sơ nghiệm thu

Mỗi ca ghi thiết bị, nền tảng, commit/build, bước thử, mong đợi, thực tế, Pass/Fail và bằng chứng.

| ID | Ca kiểm thử | Kết quả bắt buộc |
|---|---|---|
| G01 | Cài mới/Start Desktop (PC/Mac) và Mobile | Đúng Guardian/SharedCampus, không menu 6 game, không Missing Script |
| G02 | PC/Mac quét / Mobile đúng sai lớp, quyền camera và quay lại | Chỉ mục tiêu hợp lệ mở trận; hiệu ứng đúng chế độ; không kẹt input |
| G03 | Đổi sai, ghép giao nhau, ghép 4/5, chuỗi rơi | Trả đổi sai không mất lượt; không đếm trùng; khóa input; quái phản công một lần |
| G04 | Bàn khởi đầu/hết nước đi | Có nước hợp lệ, xáo không mất lượt, không vòng lặp treo |
| G05 | Khiên, hồi máu, năng lượng, Skill, chết/thắng | Giới hạn đúng; quái chết không phản công; không thao tác sau kết quả |
| G06 | Nhận thưởng, replay, callback/gửi lặp | Mỗi mốc thưởng một lần; mất mạng/restart không cộng lặp |
| G07 | Mua thiếu Core/đạt tối đa/bấm nhanh; đóng mở app | Trừ Core và tăng cấp nhất quán, không âm Core, tiến trình được lưu |
| G08 | Mất mạng, backend restart, cloud lỗi, payload sai | Pending/failed/synced đúng; chống trùng bền vững, không báo thành công giả |
| G09 | AI/AR chuẩn W9-En.pdf (ONNX, Flask/Gunicorn, Docker, Prometheus, Adafruit IO) | Xuất mô hình `.onnx` thành công; container chạy ổn định qua `docker compose up -d`; `POST /predict`: YOLOv8n 640: API total p50 ≤ 150 ms, p95 ≤ 220 ms trên CPU (Ryzen 7 6800H, Docker, Gunicorn 2 worker); confidence ≥ 0.70. Ưu tiên độ chính xác; 416 nhanh hơn nhưng trượt nhiều hơn trên mẫu thử B2.; Prometheus `/metrics` hiển thị đầy đủ thông số; Adafruit IO đồng bộ feed thật |
| G10 | Profile Mobile & Mac/PC / checkout sạch / các bản build | Ô dễ chạm, không soft-lock; ghi FPS/RAM/thời gian tải; dependency đầy đủ |

Giữ rubric gốc trong `plan.txt`: Unity 20, AR + Deep Learning 20, Mobile 20, IO Cloud 20, Docker 20; phần hỗ trợ thiếu bị trừ 20 điểm một lần theo rubric gốc, không cam kết điểm số.

Thiết kế quét hiện tại chưa tự đáp ứng AR: nhóm cần xác nhận yêu cầu giảng viên và thử một minh họa AR gọn riêng trên điện thoại tương thích. Không gọi raycast/overlay khói tím là AR, không tuyên bố hoàn thành AR nếu chưa có kiểm chứng. Cũng cần xác nhận Adafruit IO dữ liệu gameplay không cảm biến phù hợp yêu cầu môn.

Bàn giao các bản build (Desktop Windows/macOS, Mobile Android/iOS), nguồn, báo cáo, README, test và ảnh/video đầy đủ 5 thành phần (Unity, AI ONNX, AR, Mobile, Adafruit IO Cloud, Docker + Prometheus theo W9-En.pdf). Vị trí bằng chứng mới dự kiến `Docs/Evidence/CampusGuardian/`; ảnh minigame cũ chỉ là lịch sử. Nhóm còn chốt thiết bị, model/ngưỡng, số Core/giá nâng cấp, chỉ số combat, luật thua/replay và hạn nộp.
