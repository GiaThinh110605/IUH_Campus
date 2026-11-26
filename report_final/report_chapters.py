# -*- coding: utf-8 -*-
"""
Sections 1 through 9 strictly matching ReportTemplate.pdf structure:
1 Động cơ nghiên cứu
2 Mục tiêu
3 Mô tả dữ liệu
4 Cơ sở lý thuyết
5 Xử lý dữ liệu và mô hình đề xuất
6 Hiện thực hệ thống và thực nghiệm
7 Hướng dẫn và yêu cầu
8 Cách đánh giá và xử lý gian lận
9 Phân công công việc
"""

CHAPTERS = [
    {
        "num": 1,
        "title": "1 Động cơ nghiên cứu",
        "blocks": [
            {
                "type": "para",
                "text": (
                    "Trong bối cảnh chuyển đổi số giáo dục đại học, việc xây dựng bản sao số (Digital Twin) cho khuôn viên "
                    "nhà trường kết hợp tương tác trò chơi 3D, thực tế tăng cường (AR) và trí tuệ nhân tạo đang mở ra "
                    "phương thức tiếp cận trực quan giúp sinh viên khám phá không gian học tập. Tuy nhiên, bài toán kỹ thuật "
                    "đặt ra là làm sao kết nối mượt mà một ứng dụng đồ họa thời gian thực trên Unity 6000 [1] chạy trên thiết bị "
                    "di động với một pipeline suy luận học sâu nhẹ trên máy chủ CPU, đồng thời bảo đảm dữ liệu thành tích "
                    "không bị thất lạc khi kết nối mạng di động không ổn định."
                ),
            },
            {
                "type": "para",
                "text": (
                    "Đề tài \"IUH Campus Guardian\" được thực hiện nhằm giải quyết bài toán tích hợp đa tầng nêu trên tại "
                    "Trường Đại học Công nghiệp TP. Hồ Chí Minh (IUH). Về mặt trải nghiệm, sinh viên điều khiển nhân vật "
                    "khám phá khuôn viên trường 3D, thực hiện các hoạt động hằng ngày, quét camera AR nhận diện các vật thể thực "
                    "(máy tính xách tay, ghế, điện thoại, ba lô, sách giáo trình), chiến đấu giải đố 2D Match-3 RPG và đồng bộ "
                    "dữ liệu lên nền tảng Cloud IoT. Ngữ cảnh tích hợp ba tầng của bài toán được trình bày tại Hình 1.1."
                ),
            },
            {
                "type": "figure",
                "id": "fig_1_1",
                "caption": "Hình 1.1: Ngữ cảnh tích hợp ba tầng của bài toán IUH Campus Guardian (Nguồn: Nhóm tác giả tự xây dựng)",
            },
        ],
    },
    {
        "num": 2,
        "title": "2 Mục tiêu",
        "blocks": [
            {
                "type": "para",
                "text": (
                    "Mục tiêu tổng quát của đề tài là thiết kế, hiện thực và kiểm chứng một hệ thống hoàn chỉnh đáp ứng đầy đủ "
                    "5 trụ cột công nghệ của học phần: phát triển ứng dụng Unity 2D/3D, tích hợp AR và mô hình Deep Learning, "
                    "triển khai trên thiết bị di động thực tế (iPhone 12), tích hợp Cloud IoT và đóng gói vận hành qua Docker. "
                    "Các mục tiêu cụ thể và mức độ đối chiếu với tiêu chí đánh giá học phần được tổng hợp trong Bảng 2.1."
                ),
            },
            {
                "type": "bullets",
                "items": [
                    "Xây dựng khuôn viên 3D IUH (IUH_SharedCampus_Concept.unity) hỗ trợ điều khiển góc nhìn thứ ba, chiến đấu 3D theo vùng, bay phản lực và bàn cờ chiến đấu 2D Match-3 RPG 6x6.",
                    "Triển khai mô hình phát hiện đối tượng YOLOv8n định dạng ONNX FP32 trên API Flask/Gunicorn kết hợp bộ phân tích thị giác nội bộ IUHARVisionAnalyzer cho chế độ Live AR.",
                    "Đóng gói cụm 5 dịch vụ Docker Compose kèm hàng đợi Transactional Outbox hai tầng để đồng bộ 5 luồng dữ liệu lên Adafruit IO và giám sát bằng Prometheus/Grafana.",
                    "Tự động hóa quy trình xuất bản Xcode batchmode và kiểm chứng hiệu năng thực tế trên thiết bị iPhone 12 đạt 58–60 FPS.",
                ],
            },
            {
                "type": "table",
                "caption": "Bảng 2.1: Tổng hợp mục tiêu và đối chiếu tiêu chí đánh giá học phần",
                "headers": ["Thành phần Rubric", "Điểm chuẩn", "Mục tiêu hiện thực và sản phẩm bàn giao"],
                "rows": [
                    ["Unity 2D and 3D implementation", "20", "3D Campus IUH (Cinemachine, ZoneDirector, Jetpack, NPC, Vườn cây) và 2D Match-3 RPG 6x6."],
                    ["AR implementation & Deep Learning", "20", "Live AR Camera + YOLOv8n ONNX CPU + IUHARVisionAnalyzer + Neon Rune Slash Combat."],
                    ["Mobile deployment", "20", "Tự động xuất Xcode batchmode (IUHIOSCommandLineBuild), chạy 58–60 FPS trên iPhone 12."],
                    ["IO cloud integration", "20", "Đồng bộ 5 feeds Adafruit IO REST v2 qua SQLite Outbox chống mất và trùng dữ liệu."],
                    ["Docker use", "20", "Docker Compose 5 services (Nginx, Flask API, Sync Worker, Prometheus, Grafana)."],
                ],
                "col_ratios": [0.30, 0.14, 0.56],
            },
        ],
    },
    {
        "num": 3,
        "title": "3 Mô tả dữ liệu",
        "blocks": [
            {
                "type": "para",
                "text": (
                    "Hệ thống xử lý ba nhóm dữ liệu chính: (1) dữ liệu hình ảnh camera thực tế phục vụ nhận diện AR và kiểm thử mô hình AI; "
                    "(2) dữ liệu trạng thái trò chơi (tọa độ nhân vật, trạng thái cửa, tiến độ cốt truyện, cấp độ nâng cấp, luống cây vườn) "
                    "lưu trữ cục bộ trên thiết bị; và (3) dữ liệu sự kiện gameplay dạng JSON truyền tải về máy chủ và Cloud IoT. "
                    "Các yêu cầu kỹ thuật đối với hệ thống và dữ liệu được tóm tắt tại Bảng 3.1."
                ),
            },
            {
                "type": "table",
                "caption": "Bảng 3.1: Tổng hợp yêu cầu chức năng và phi chức năng của hệ thống",
                "headers": ["Mã yêu cầu", "Phân hệ", "Mô tả yêu cầu dữ liệu và vận hành"],
                "rows": [
                    ["YC-01", "Unity 2D/3D", "Tách lõi thuật toán thuần C# (Match3.Core, Story.Core, Garden.Core) để kiểm thử đơn vị không phụ thuộc Engine."],
                    ["YC-02", "AI & AR", "Ảnh đầu vào giới hạn <= 5–6 MB; trích xuất đặc trưng lưới 6x6 và suy luận ONNX ở kích thước 640x640."],
                    ["YC-03", "Outbox & Cloud", "Mỗi sự kiện dùng mã UUID event_id chống ghi trùng (201 recorded / 200 duplicate / 409 conflict)."],
                    ["YC-04", "Hiệu năng API", "Mục tiêu độ trễ toàn trình CPU: trung vị p50 <= 150 ms, p95 <= 220 ms ở ngưỡng tin cậy 0,70."],
                    ["YC-05", "Bảo mật & Di động", "Container chạy non-root (appuser); giao diện mobile tự căn lề theo Screen.safeArea."],
                ],
                "col_ratios": [0.14, 0.18, 0.68],
            },
            {
                "type": "para",
                "text": (
                    "Để đánh giá thực tế năng lực nhận diện của mô hình tiền huấn luyện trên CPU (kịch bản B2.6), nhóm thu thập bộ ảnh thực "
                    "gồm 36 tệp ảnh phân theo 6 lớp đối tượng phổ biến trong môi trường sinh viên và 1 lớp nền trống (none). Sau khi loại bỏ 3 ảnh gốc "
                    "có dung lượng vượt quá 5 MiB theo quy chuẩn tải lên, tập dữ liệu đánh giá còn lại 33 ảnh (29 ảnh dương và 4 ảnh nền trống) như mô tả tại Bảng 3.2."
                ),
            },
            {
                "type": "table",
                "caption": "Bảng 3.2: Đặc tả tập dữ liệu ảnh thực tế và các lớp đối tượng mục tiêu",
                "headers": ["Tên lớp (COCO Label)", "Số ảnh đánh giá", "Thuộc SCAN_INFECTED_POOL mặc định", "Đặc điểm nhận diện trong trò chơi"],
                "rows": [
                    ["laptop", "5", "Có", "Máy tính xách tay sinh viên (màn hình có viền bezel hoặc bàn phím)."],
                    ["chair", "6", "Có", "Ghế ngồi trong lớp học, khuôn viên hoặc phòng tự học."],
                    ["cell phone", "5", "Có", "Điện thoại thông minh cầm tay hoặc đặt trên bàn."],
                    ["bottle", "5", "Không (Clean)", "Chai nước cá nhân; hệ thống nhận dạng nhưng không sinh quái vật nhiễm."],
                    ["book", "3", "Không (Clean)", "Sách giáo trình, vở ghi chép; ưu tiên nhận diện bằng bộ lọc giấy trắng nội bộ."],
                    ["backpack", "5", "Không (Clean)", "Ba lô sinh viên; kết hợp kiểm tra tỷ lệ điểm ảnh tối và độ tương phản cục bộ."],
                    ["none (Nền trống)", "4", "Kiểm tra dương giả", "Vách tường phẳng, mặt bàn trống dùng để kiểm tra tỷ lệ báo động giả (FPR)."],
                ],
                "col_ratios": [0.22, 0.14, 0.24, 0.40],
            },
        ],
    },
    {
        "num": 4,
        "title": "4 Cơ sở lý thuyết",
        "blocks": [
            {"type": "h2", "text": "4.1. Khảo sát các phương pháp giải quyết bài toán"},
            {
                "type": "para",
                "text": (
                    "Để xây dựng nền tảng lý thuyết vững chắc cho cả ba tầng của hệ thống, nhóm tiến hành khảo sát các công trình nghiên cứu "
                    "và tiêu chuẩn kỹ thuật mới liên quan đến phát hiện đối tượng một giai đoạn, thực thi suy luận trên CPU và kiến trúc truyền tin tin cậy "
                    "(tóm tắt đối chiếu tại Bảng 4.1):"
                ),
            },
            {
                "type": "bullets",
                "items": [
                    "Phát hiện đối tượng thời gian thực với YOLOv8: Terven và cộng sự (2023) trong công trình \"A Comprehensive Review of YOLO Architectures in Computer Vision: From YOLOv1 to YOLOv8 and YOLO-NAS\" [2] cùng Jocher và cộng sự (2023) với tài liệu Ultralytics YOLOv8 [3] chỉ ra rằng kiến trúc anchor-free kết hợp khối C2f đạt cân bằng tốt giữa tốc độ và độ chính xác trên tập MS COCO (80 lớp, biến thể YOLOv8n có 3,2 triệu tham số, đạt mAP50-95 = 37,3%). Tuy nhiên, mô hình tiền huấn luyện trên COCO thường bị suy giảm độ nhạy khi ảnh chụp cận cảnh bị thiếu ngữ cảnh xung quanh.",
                    "Suy luận học sâu trên CPU với ONNX Runtime: Microsoft Corporation và nhóm phát triển ONNX Runtime (2024) [4] "
                    "đề xuất tách pha huấn luyện PyTorch khỏi môi trường vận hành bằng đồ thị tính toán tĩnh ONNX, giúp giảm kích thước "
                    "bộ chứa Docker và loại bỏ phụ thuộc GPU. Hạn chế thực tế của phương pháp lượng tử hóa INT8 (QInt8) trên một số CPU "
                    "là lỗi chưa hỗ trợ toán tử ConvInteger(10) ở bộ thực thi CPUExecutionProvider.",
                    "Đồng bộ tin cậy bằng Transactional Outbox Pattern: Richardson (2018) trong chuyên khảo Microservices Patterns [5] "
                    "chứng minh rằng ghi nhận sự kiện vào bảng hàng đợi cục bộ trong cùng giao dịch cơ sở dữ liệu rồi đẩy ra ngoài bằng "
                    "tiến trình nền tách biệt giúp tránh mất dữ liệu khi dịch vụ đích gián đoạn, song đòi hỏi cơ chế kiểm tra tính bất biến "
                    "(idempotency) theo mã UUID.",
                    "Quy trình 5 bước ML in Production (W9): Tài liệu hướng dẫn học phần của Nguyễn Ngọc Lễ (2026) [6] chuẩn hóa quy trình đưa mô hình học máy vào sản xuất gồm 5 bước: (1) xuất mô hình sang ONNX opset chuẩn; (2) xây dựng Flask REST API phi trạng thái chạy trên Gunicorn WSGI; (3) đóng gói Docker non-root kèm healthcheck; (4) bố trí Reverse Proxy sẵn sàng mở rộng ngang; và (5) xuất độ đo giám sát qua Prometheus /metrics.",
                    "Trích xuất đặc trưng thị giác cổ điển trên thiết bị: Szeliski (2022) trong giáo trình Computer Vision: Algorithms and Applications [7] trình bày các phép đo độ chói tuyến tính, phương sai cục bộ theo lưới ô và mật độ biên cạnh làm nền tảng cho bộ lọc nội bộ IUHARVisionAnalyzer phân biệt vách tường phẳng, trang sách và màn hình máy tính khi không có kết nối máy chủ.",
                ],
            },
            {
                "type": "table",
                "caption": "Bảng 4.1: Đối chiếu các nghiên cứu và phương pháp kỹ thuật liên quan",
                "headers": ["Tác giả / Năm", "Phương pháp / Công trình", "Ưu điểm chính", "Hạn chế và hướng khắc phục trong đề tài"],
                "rows": [
                    ["Terven et al. (2023) [2]; Jocher et al. (2023) [3]", "YOLOv8n Anchor-Free (COCO)", "Gọn nhẹ (12,85 MB FP32), tốc độ suy luận CPU nhanh.", "Dễ trượt sách/ba lô cận cảnh; khắc phục bằng thu hẹp pool 3 lớp và bổ sung bộ lọc nội bộ."],
                    ["ONNX Runtime Developers (2024) [4]", "ONNX Runtime CPU Inference", "Không cần PyTorch/CUDA ở runtime, dễ đóng gói Docker.", "INT8 gặp lỗi ConvInteger(10); chốt dùng chuẩn FP32 opset 17."],
                    ["Richardson (2018) [5]", "Transactional Outbox Pattern", "Chống mất dữ liệu khi rớt mạng hoặc Cloud giới hạn tốc độ.", "Cần UUID idempotency; hiện thực ở cả Client JSON và Server SQLite."],
                    ["Nguyễn Ngọc Lễ (2026) [6]", "Quy trình 5 bước W9 Production", "Chuẩn hóa từ export ONNX đến Nginx và Prometheus.", "Áp dụng đầy đủ cho cụm 5 container Docker Compose."],
                ],
                "col_ratios": [0.22, 0.24, 0.24, 0.30],
            },
            {"type": "h2", "text": "4.2. Các công thức toán học và độ đo sử dụng"},
            {
                "type": "para",
                "text": (
                    "Trong cơ chế đồng bộ ngoại tuyến của lớp GuardianEventOutbox, mỗi lần gửi lại sau lỗi mạng hoặc mã HTTP 5xx được giãn cách "
                    "theo hàm mũ (Exponential Backoff) nhằm tránh gây nghẽn tài nguyên thiết bị và máy chủ theo Công thức (4.1):"
                ),
            },
            {
                "type": "equation",
                "parts": [
                    ("t", "italic"), ("backoff", "sub"), (" = min( t", "normal"), ("max", "sub"),
                    (" ,  t", "normal"), ("base", "sub"), (" · 2", "normal"), ("k − 1", "sup"), (" )", "normal"),
                ],
                "number": "(4.1)",
            },
            {
                "type": "para",
                "text": (
                    "Trong bộ giải đố chiến thuật Match-3 RPG (Match3CombatRules.cs), mỗi đợt ngọc rơi nối tiếp ở bước thứ c >= 0 "
                    "và độ dài chuỗi ghép lớn nhất L_match sẽ quyết định hệ số nhân sát thương và lượng sát thương thực tế người chơi phải nhận từ Boss "
                    "sau khi trừ lớp giáp tạm thời theo Công thức (4.2):"
                ),
            },
            {
                "type": "equation",
                "parts": [
                    ("M", "italic"), (" = min( 1 + 0,25·", "normal"), ("c", "italic"),
                    (" , 2,0 ) × ", "normal"), ("B", "italic"), (" ;    ", "normal"),
                    ("D", "italic"), ("net", "sub"), (" = max( 0 ,  ", "normal"),
                    ("I", "italic"), ("boss", "sub"), (" − ", "normal"),
                    ("S", "italic"), ("player", "sub"), (" )", "normal"),
                ],
                "number": "(4.2)",
            },
            {
                "type": "para",
                "text": (
                    "Trong bộ phân tích thị giác nội bộ IUHARVisionAnalyzer.cs, độ chói Y của từng điểm ảnh (R, G, B) trên lưới 6x6 ô "
                    "và điều kiện phát hiện để từ chối vách tường phẳng trơn (IsPlainWall) được xác định bởi Công thức (4.3):"
                ),
            },
            {
                "type": "equation",
                "parts": [
                    ("Y", "italic"), (" = 0,299", "normal"), ("R", "italic"), (" + 0,587", "normal"), ("G", "italic"),
                    (" + 0,114", "normal"), ("B", "italic"), (" ;   PlainWall = (Contrast < 40 và σ", "normal"),
                    ("max", "sub"), (" < 12,0) hoặc (RichPatches = 0 và MicroEdge < 0,028)", "normal"),
                ],
                "number": "(4.3)",
            },
            {
                "type": "para",
                "text": (
                    "Để đánh giá định lượng hiệu quả của mô hình phát hiện đối tượng và API Backend, nhóm sử dụng bộ ba độ đo gồm: "
                    "tỷ lệ phát hiện đúng ở cấp ảnh (Recall) và tỷ lệ dương giả trên ảnh nền trống (FPR) theo Công thức (4.4), "
                    "cùng các phân vị độ trễ p50 và p95 theo Công thức (4.5):"
                ),
            },
            {
                "type": "equation",
                "parts": [
                    ("Recall = ", "normal"), ("TP", "italic"), (" / (", "normal"), ("TP + FN", "italic"),
                    (") ;      FPR = ", "normal"), ("FP", "italic"), (" / (", "normal"), ("FP + TN", "italic"), (")", "normal"),
                ],
                "number": "(4.4)",
            },
            {
                "type": "equation",
                "parts": [
                    ("p", "italic"), ("q", "sub"), (" = Quantile", "normal"), ("q", "sub"),
                    ("(", "normal"), ("T", "italic"), ("1..N", "sub"), (")   với ", "normal"),
                    ("q", "italic"), (" = 0,50 hoặc ", "normal"), ("q", "italic"), (" = 0,95", "normal"),
                ],
                "number": "(4.5)",
            },
        ],
    },
    {
        "num": 5,
        "title": "5 Xử lý dữ liệu và kiến trúc đề xuất",
        "blocks": [
            {"type": "h2", "text": "5.1. Mô hình tổng quát hệ thống"},
            {
                "type": "para",
                "text": (
                    "Từ cơ sở lý thuyết trên, nhóm xây dựng mô hình tổng quát cho hệ thống IUH Campus Guardian gồm ba tầng kết nối lỏng "
                    "(Hình 5.1): Tầng 1 là Unity Client chạy trên iPhone 12 hoặc máy tính cá nhân; Tầng 2 là cụm 5 dịch vụ Docker Compose [8] "
                    "tích hợp Reverse Proxy Nginx, máy chủ suy luận Flask/Gunicorn ONNX, cơ sở dữ liệu SQLite, tiến trình đồng bộ nền "
                    "và cặp công cụ giám sát Prometheus/Grafana [9]; Tầng 3 là nền tảng đám mây Adafruit IO [10]."
                ),
            },
            {
                "type": "figure",
                "id": "fig_5_1",
                "caption": "Hình 5.1: Mô hình tổng quát hệ thống IUH Campus Guardian (Nguồn: Nhóm tác giả tự xây dựng)",
            },
            {
                "type": "table",
                "caption": "Bảng 5.1: Danh sách 5 dịch vụ trong cụm Docker Compose",
                "headers": ["Tên dịch vụ", "Image / Nền tảng", "Cổng", "Chức năng trong kiến trúc"],
                "rows": [
                    ["nginx", "nginx:1.28.0-alpine", "5001 -> 80", "Reverse proxy tiếp nhận lưu lượng từ Unity Client, kiểm tra /nginx-health."],
                    ["iuh-guardian-api", "python:3.11-slim", "5001 (nội bộ)", "Flask + Gunicorn API chạy YOLOv8n ONNX CPU, kiểm tra tính hợp lệ sự kiện và ghi SQLite."],
                    ["iuh-guardian-sync", "python:3.11-slim", "Nội bộ", "Tiến trình nền app.sync_worker đọc job từ SQLite outbox và gửi lên Adafruit IO."],
                    ["prometheus", "prom/prometheus:v3.2.1", "9090", "Thu thập định kỳ độ đo từ /metrics, lưu chuỗi thời gian 7 ngày."],
                    ["grafana", "grafana/grafana:11.6.0", "3000", "Trực quan hóa bảng điều khiển tự động cấu hình từ datasource Prometheus."],
                ],
                "col_ratios": [0.22, 0.25, 0.13, 0.40],
            },
            {"type": "h2", "text": "5.2. Xử lý dữ liệu ảnh và đồng bộ Outbox hai tầng"},
            {
                "type": "para",
                "text": (
                    "Ở luồng dữ liệu đồng bộ sự kiện (Hình 5.2), mọi chiến thắng Match-3 hoặc sự kiện nhận thưởng đều được ghi xuống tệp cục bộ "
                    "guardian_outbox.json trước khi gửi lên API POST /gameplay/events. Tại Backend, EventStore thực thi giao dịch SQLite nguyên tử "
                    "để vừa kiểm tra trùng UUID vừa đưa bản ghi vào bảng hàng đợi cho sync_worker đẩy tiếp lên 5 feeds của Adafruit IO (Bảng 5.2), "
                    "đồng thời xử lý mã phản hồi giới hạn tần suất HTTP 429 bằng header Retry-After."
                ),
            },
            {
                "type": "figure",
                "id": "fig_5_2",
                "caption": "Hình 5.2: Sơ đồ luồng dữ liệu đồng bộ hai tầng theo Transactional Outbox (Nguồn: Nhóm tác giả tự xây dựng)",
            },
            {
                "type": "table",
                "caption": "Bảng 5.2: Các điểm cuối API Backend và 5 nguồn dữ liệu Adafruit IO",
                "headers": ["Thành phần", "Định danh / Đường dẫn", "Dữ liệu trao đổi và vai trò"],
                "rows": [
                    ["API Sức khỏe", "GET /health", "Trả về trạng thái model_loaded, db_ok, outbox_pending và outbox_failed."],
                    ["API Nhận diện", "POST /predict", "Nhận ảnh JPEG (tối đa 6 MB) và target_class; trả bounding box và độ tin cậy."],
                    ["API Phiên quét", "POST /scan/sessions", "Khởi tạo phiên quét 15 phút với tập lớp nhiễm SCAN_INFECTED_POOL và sinh monster_token."],
                    ["API Sự kiện", "POST /gameplay/events", "Ghi nhận chiến thắng, cộng thưởng Campus Core nguyên tử vào SQLite."],
                    ["Adafruit IO Feeds", "guardian-quests-completed, guardian-campus-core, guardian-upgrades, lost-found-count, guardian-monsters-revealed", "Lưu trữ 5 chỉ số hoạt động thực tế của người chơi trên Cloud IoT."],
                ],
                "col_ratios": [0.20, 0.30, 0.50],
            },
            {
                "type": "para",
                "text": (
                    "Ở luồng xử lý dữ liệu ảnh (Hình 5.3), trước khi đưa vào mạng YOLOv8n (640x640), ảnh camera được xoay đúng hướng theo thẻ EXIF Orientation, "
                    "chuyển sang hệ màu RGB và thu nhỏ cạnh dài tối đa 640 px bằng phép nội suy LANCZOS để giảm băng thông truyền tải từ thiết bị di động. "
                    "Song song đó, ở chế độ ngoại tuyến trên điện thoại, IUHARVisionAnalyzer quét vùng trung tâm 20%–80% trên lưới 6x6 để loại bỏ vách tường trơn "
                    "theo Công thức (4.3) và phân biệt chính xác màn hình laptop với trang sách giáo trình."
                ),
            },
            {
                "type": "figure",
                "id": "fig_5_3",
                "caption": "Hình 5.3: Lưu đồ tiền xử lý ảnh và phân tích thị giác nội bộ IUHARVisionAnalyzer (Nguồn: Nhóm tác giả tự xây dựng)",
            },
        ],
    },
    {
        "num": 6,
        "title": "6 Hiện thực hệ thống và kết quả thực nghiệm",
        "blocks": [
            {"type": "h2", "text": "6.1. Hiện thực Unity 2D/3D, Live AR và phân tầng mã nguồn"},
            {
                "type": "para",
                "text": (
                    "Toàn bộ mã nguồn phía Unity Client gồm 177 tệp C# runtime/kiểm thử và 60 tệp tự động hóa Editor cho 3 màn chơi chính "
                    "(IUH_SharedCampus_Concept.unity, IUH_Campus_AR_Encounter.unity, IUH_CombatArena.unity). Nhóm tổ chức các lõi nghiệp vụ "
                    "dưới dạng các Assembly Definition độc lập không tham chiếu UnityEngine (Hình 6.1 và Bảng 6.1), giúp chạy Unit Test thuần C# "
                    "ngay trong EditMode mà không cần dựng cảnh 3D."
                ),
            },
            {
                "type": "figure",
                "id": "fig_6_1",
                "caption": "Hình 6.1: Kiến trúc phân tầng Assembly Definition (.asmdef) trong Unity (Nguồn: Nhóm tác giả tự xây dựng)",
            },
            {
                "type": "table",
                "caption": "Bảng 6.1: Phân hệ chức năng chính trong mã nguồn Unity Client",
                "headers": ["Phân hệ", "Lớp đại diện", "Vai trò kỹ thuật trong trò chơi"],
                "rows": [
                    ["Core & Save", "GameEvents, WorldState, SaveSystem", "Kênh sự kiện tĩnh toàn cục; lưu vị trí và trạng thái cửa ra IUH_Campus_Save.json."],
                    ["Story Engine", "IUHStoryDirector, StoryProgress, IUHStoryWorldLock", "Điều phối 5 pha cốt truyện (Prologue -> Incident -> Chapter -> Ending -> Completed) và dư chấn."],
                    ["3D Combat & Flight", "IUHCampusZoneDirector, IUHCampusCombatManager, IUHAerialFlightController", "Kích hoạt 3 vùng nguy hiểm, súng Plasma, tên lửa phòng không, 3 kỹ năng và bay phản lực 6 hướng."],
                    ["Match-3 RPG", "Match3BoardModel, Match3CombatRules, Match3CombatManager", "Bàn cờ 6x6 thuần C# với 5 loại ngọc (Sword, Shield, Energy, Heal, Purify) và ý định Boss."],
                    ["Live AR & Vision", "IUHLiveARController, IUHARVisionAnalyzer, IUHARRuneSlashCombat", "Chiếu WebCamTexture ở z = 9m, quái 3D ở z = 2,7m, nhận diện ngoại tuyến và chém Rune."],
                    ["Daily & Garden", "IUHCampusDailyActivitiesManager, IUHGardenManager", "Nhiệm vụ hằng ngày reset theo UTC yyyyMMdd và thuật toán Union-Find tự tìm luống hoa."],
                ],
                "col_ratios": [0.18, 0.34, 0.48],
            },
            {
                "type": "para",
                "text": (
                    "Trong hệ thống chiến đấu giải đố 2D Match-3 RPG, người chơi tích lũy phần thưởng một lần theo MilestoneId trong "
                    "IUHGuardianInventory để nâng cấp 3 chỉ số qua 5 cấp độ (Bảng 6.2), liên kết trực tiếp sức mạnh nhân vật khi đối đầu Boss."
                ),
            },
            {
                "type": "table",
                "caption": "Bảng 6.2: Thông số cân bằng chiến đấu Match-3 RPG và nâng cấp Campus Core",
                "headers": ["Chỉ số nâng cấp", "Cấp 1", "Cấp 2", "Cấp 3", "Cấp 4", "Cấp 5", "Chi phí nâng cấp từng bậc (Vàng)"],
                "rows": [
                    ["Sát thương Kiếm (DMG/viên)", "22", "25", "29", "34", "40", "80 -> 150 -> 250 -> 400"],
                    ["Giáp Khiên (Shield/viên)", "15", "18", "21", "25", "30", "80 -> 150 -> 250 -> 400"],
                    ["Sinh lực tối đa (MaxHP)", "1000", "1100", "1220", "1360", "1500", "100 -> 180 -> 300 -> 500"],
                ],
                "col_ratios": [0.28, 0.08, 0.08, 0.08, 0.08, 0.08, 0.32],
            },
            {"type": "h2", "text": "6.2. Thực nghiệm tối ưu mô hình YOLOv8n ONNX"},
            {
                "type": "para",
                "text": (
                    "Nhóm thực nghiệm so sánh mô hình YOLOv8n FP32 ở 4 kích thước đầu vào (640, 512, 480, 416) cùng bản lượng tử hóa INT8 "
                    "trên máy chủ CPU AMD Ryzen 7 6800H trong Docker (2 Gunicorn workers, 6 luồng ORT/worker). Kết quả ở Bảng 6.3 và Hình 6.2 "
                    "cho thấy kích thước 416 và 480 tuy nhanh hơn nhưng bỏ sót cả 2 ảnh mẫu laptop tại ngưỡng 0,70, còn bản INT8 không nạp được "
                    "trên CPUExecutionProvider do lỗi toán tử ConvInteger(10). Vì vậy cấu hình YOLOv8n 640 FP32 (SHA-256: 4d063228...) được chốt "
                    "làm mô hình chính thức."
                ),
            },
            {
                "type": "table",
                "caption": "Bảng 6.3: So sánh hiệu năng suy luận YOLOv8n giữa các kích thước đầu vào (B2.5)",
                "headers": ["Cấu hình mô hình", "Inference p50 / p95 (ms)", "API Total p50 / p95 (ms)", "Mẫu dương laptop (0,70)", "Ghi chú kỹ thuật"],
                "rows": [
                    ["YOLOv8n 640 FP32", "74,69 / 139,57", "98,37 / 163,61", "1 / 2", "Độ chính xác tốt nhất trong thử nghiệm; được chọn mặc định."],
                    ["YOLOv8n 512 FP32", "60,66 / 95,51", "80,08 / 115,16", "Chưa đo", "Nhanh hơn ~18%, chưa kiểm chứng trên tập ảnh thực."],
                    ["YOLOv8n 480 FP32", "64,92 / 106,68", "85,64 / 132,25", "0 / 2", "Bỏ sót cả 2 ảnh mẫu laptop ở ngưỡng 0,70."],
                    ["YOLOv8n 416 FP32", "28,17 / 40,97", "41,04 / 56,69", "0 / 2", "Tốc độ cao nhất nhưng suy giảm mạnh khả năng phát hiện."],
                    ["YOLOv8n 640 INT8", "Không khả dụng", "Không khả dụng", "Lỗi nạp", "ONNX Runtime CPU báo lỗi nút ConvInteger(10)."],
                ],
                "col_ratios": [0.20, 0.20, 0.20, 0.16, 0.24],
            },
            {
                "type": "figure",
                "id": "fig_6_2",
                "caption": "Hình 6.2: So sánh độ trễ API p50 và p95 theo kích thước ảnh đầu vào YOLOv8n (Nguồn: Nhóm tác giả tự xây dựng)",
            },
            {"type": "h2", "text": "6.3. Kết quả đánh giá trên tập ảnh thực, kiểm thử và triển khai iPhone 12"},
            {
                "type": "para",
                "text": (
                    "Đánh giá YOLOv8n 640 FP32 trên tập 33 ảnh thực tế (Bảng 6.4 và Hình 6.3) cho thấy tại ngưỡng 0,70, mô hình nhận diện "
                    "đạt 100% với laptop (5/5), 66,7% với chair (4/6) và 60,0% với cell phone (3/5), đồng thời không phát sinh bất kỳ dương giả nào "
                    "trên 4 ảnh nền trống (FPR = 0/4). Các lớp book (0/3), bottle (0/5) và backpack (0/5) bị bỏ sót ở ngưỡng 0,70 do ảnh chụp cận cảnh "
                    "khác phân bố tập huấn luyện COCO. Nhằm giảm tỷ lệ người chơi bị kẹt nhiệm vụ quét, nhóm cấu hình SCAN_INFECTED_POOL gồm 3 lớp "
                    "đặc trưng ổn định nhất (cell phone, laptop, chair), giúp Recall thực tế của phiên quét đạt 75,0% (12/16) với độ tin cậy trung bình 0,8971."
                ),
            },
            {
                "type": "table",
                "caption": "Bảng 6.4: Kết quả đánh giá nhận diện trên tập 33 ảnh thực tế (B2.6)",
                "headers": ["Ngưỡng", "Recall từng lớp (Số ảnh đạt / Tổng)", "Recall 6 lớp", "Recall 3 lớp Pool", "FPR (none)", "Conf đúng TB"],
                "rows": [
                    ["0,50", "laptop 5/5; chair 4/6; cell phone 4/5; bottle 2/5; book 0/3; backpack 0/5", "15/29 (51,7%)", "13/16 (81,3%)", "0 / 4", "0,8495"],
                    ["0,60", "laptop 5/5; chair 4/6; cell phone 4/5; bottle 2/5; book 0/3; backpack 0/5", "15/29 (51,7%)", "13/16 (81,3%)", "0 / 4", "0,8495"],
                    ["0,70", "laptop 5/5; chair 4/6; cell phone 3/5; bottle 0/5; book 0/3; backpack 0/5", "12/29 (41,4%)", "12/16 (75,0%)", "0 / 4", "0,8971"],
                ],
                "col_ratios": [0.12, 0.34, 0.14, 0.14, 0.12, 0.14],
            },
            {
                "type": "figure",
                "id": "fig_6_3",
                "caption": "Hình 6.3: Tỷ lệ nhận diện (Recall) theo từng lớp vật thể ở ngưỡng 0,50 và 0,70 (Nguồn: Nhóm tác giả tự xây dựng)",
            },
            {
                "type": "para",
                "text": (
                    "Kết quả kiểm thử tích hợp toàn hệ thống ở lượt chạy D2 và triển khai thực tế trên thiết bị iPhone 12 được tổng hợp trong Bảng 6.5 và Bảng 6.6. "
                    "Tất cả 134 kiểm thử pytest và 8 kiểm thử NUnit AR đều đạt 100%; riêng phép đo tải liên tiếp 50 lượt HTTP đạt p50 = 96,215 ms (đạt <= 150 ms) "
                    "nhưng p95 = 257,221 ms (vượt mức 220 ms do dao động lập lịch CPU), là điểm nhóm ghi nhận trung thực để tiếp tục tối ưu."
                ),
            },
            {
                "type": "table",
                "caption": "Bảng 6.5: Tổng hợp kết quả kiểm thử tự động Backend (D2) và thị giác AR (NUnit)",
                "headers": ["Nhóm kiểm thử", "Phạm vi kiểm chứng", "Kết quả đo thực tế", "Trạng thái"],
                "rows": [
                    ["Backend Unit & Integration (pytest)", "Validation, Scan Pool, Outbox rollback, SQLite concurrency", "134 passed, 0 failed, 0 skipped (16,69 giây)", "Đạt"],
                    ["Docker Stack & Healthcheck", "5 dịch vụ compose (Nginx, API, Sync, Prometheus, Grafana)", "API/Nginx/Prom/Grafana healthy; /health HTTP 200", "Đạt"],
                    ["Outbox & Idempotency (G06–G12)", "Trùng UUID, xung đột payload, token 1 lần, Adafruit 5 feeds", "201 recorded, 200 duplicate, 409 conflict, 400 token reuse", "Đạt"],
                    ["Kiểm thử tải HTTP D2 (G09-Perf)", "5 warmup + 50 yêu cầu POST /predict liên tiếp trong Docker", "p50 = 96,215 ms (<= 150 ms); p95 = 257,221 ms (> 220 ms)", "Chưa đạt p95"],
                    ["Unity AR Vision NUnit (IUHARVisionTests)", "Laptop (sáng/tối/bàn phím), Sách, Ba lô, Vách tường phẳng", "8 / 8 test Pass (0 Failed)", "Đạt"],
                ],
                "col_ratios": [0.26, 0.34, 0.28, 0.12],
            },
            {
                "type": "table",
                "caption": "Bảng 6.6: Thông số vận hành thực tế trên thiết bị di động iPhone 12",
                "headers": ["Thông số thiết bị / ứng dụng", "Giá trị đo thực tế trên iPhone 12"],
                "rows": [
                    ["Mã định danh gói (Bundle Identifier)", "com.lamgiathinh.iuhcampus (IL2CPP ARM64 Release)"],
                    ["Tốc độ khung hình trung bình", "58 – 60 FPS ổn định trong cả 3D Campus và Live AR"],
                    ["Mức tiêu thụ bộ nhớ RAM", "Xấp xỉ 210 MB (tối ưu texture WebCamTexture 640x480)"],
                    ["Giao diện và điều khiển", "Dynamic Touch Joystick + nút kỹ năng căn lề an toàn tai thỏ (SafeArea)"],
                ],
                "col_ratios": [0.38, 0.62],
            },
        ],
    },
    {
        "num": 7,
        "title": "7 Hướng dẫn và yêu cầu",
        "blocks": [
            {"type": "h2", "text": "7.1. Hướng dẫn cài đặt và vận hành hệ thống"},
            {
                "type": "bullets",
                "items": [
                    "Khởi chạy Backend bằng Docker Compose: di chuyển vào thư mục Server/, tạo tệp .env từ .env.example (khai báo ADAFRUIT_AIO_USERNAME, ADAFRUIT_AIO_KEY và GRAFANA_ADMIN_PASSWORD) rồi chạy lệnh docker compose up -d --build.",
                    "Kiểm tra tình trạng dịch vụ: truy cập http://localhost:5001/health để kiểm tra model_loaded và db_ok, mở http://localhost:9090 (Prometheus) và http://localhost:3000 (Grafana) để theo dõi biểu đồ thời gian thực.",
                    "Mở và chạy Unity Client: mở thư mục IUH_Campus bằng Unity 6000.5.8f1, nạp scene Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity và nhấn Play.",
                    "Xuất bản sang iOS (iPhone): chạy phương thức IUHCampus.Editor.IUHIOSCommandLineBuild.BuildIOS ở chế độ -batchmode để tạo dự án Build_iOS/Unity-iPhone.xcodeproj và cài đặt qua Xcode.",
                ],
            },
            {"type": "h2", "text": "7.2. Yêu cầu môi trường"},
            {
                "type": "para",
                "text": (
                    "Hệ thống yêu cầu máy tính cài đặt Docker Desktop phiên bản >= 24.0 (hỗ trợ Docker Compose v2), Python 3.11.x (khi chạy các script "
                    "kiểm thử/export ngoài container), Unity 6000.5.8f1 kèm mô-đun iOS Build Support (trên macOS có Xcode) và thiết bị iPhone hỗ trợ iOS 17 trở lên."
                ),
            },
            {"type": "h2", "text": "7.3. Quy trình kiểm thử và nộp bài"},
            {
                "type": "para",
                "text": (
                    "Trước khi bàn giao sản phẩm, nhóm thực hiện kiểm chứng toàn diện gồm: chạy bộ pytest trong Server/tests (134/134 ca đạt), "
                    "chạy EditMode/PlayMode tests trong Unity (bao gồm IUHARVisionTests 8/8 ca đạt), lưu giữ bằng chứng nhật ký đo đạc tại Docs/Evidence/ "
                    "và hoàn thiện báo cáo cùng mã nguồn dự án."
                ),
            },
        ],
    },
    {
        "num": 8,
        "title": "8 Cách đánh giá và xử lý gian lận",
        "blocks": [
            {"type": "h2", "text": "8.1. Đánh giá"},
            {
                "type": "para",
                "text": (
                    "Mỗi bài làm sẽ được giảng viên đánh giá dựa trên mức độ hoàn thiện của mã nguồn thực tế, "
                    "kết quả thực nghiệm và chất lượng trình bày báo cáo theo các tiêu chí trong Bảng 8.1."
                ),
            },
            {
                "type": "table",
                "caption": "Bảng 8.1: Tiêu chí đánh giá đồ án theo quy định môn học",
                "headers": ["Nội dung đánh giá", "Tỉ lệ điểm (%)"],
                "rows": [
                    ["Hiện thực đầy đủ ứng dụng Unity 2D và 3D mượt mà", "20%"],
                    ["Tích hợp Live AR và mô hình Deep Learning YOLOv8n ONNX", "20%"],
                    ["Triển khai ứng dụng thành công trên thiết bị di động (iPhone 12)", "20%"],
                    ["Tích hợp Cloud IoT (Adafruit IO) và cơ chế chống mất dữ liệu Outbox", "20%"],
                    ["Đóng gói Docker đa dịch vụ, giám sát hệ thống và trình bày báo cáo đúng chuẩn", "20%"],
                ],
                "col_ratios": [0.78, 0.22],
            },
            {"type": "h2", "text": "8.2. Xử lý gian lận"},
            {
                "type": "para",
                "text": (
                    "Nhóm cam kết toàn bộ mã nguồn, số liệu thực nghiệm và nội dung báo cáo đều do các thành viên tự thực hiện và nắm vững nguyên lý hoạt động:"
                ),
            },
            {
                "type": "bullets",
                "items": [
                    "Không sao chép mã nguồn hoặc báo cáo từ các nhóm khác; mọi tài liệu, bài báo và thư viện tham khảo đều được trích dẫn đầy đủ [1]–[10].",
                    "Không tự ý chỉnh sửa số liệu đo đạc để làm đẹp kết quả: mọi hạn chế thực tế như tỷ lệ nhận diện sách/ba lô hoặc giá trị p95 = 257,22 ms trong lượt chạy D2 đều được báo cáo trung thực kèm giải thích nguyên nhân.",
                ],
            },
        ],
    },
    {
        "num": 9,
        "title": "9 Phân công công việc",
        "blocks": [
            {"type": "h2", "text": "9.1. Phân công"},
            {
                "type": "para",
                "text": (
                    "Hai thành viên trong nhóm phối hợp theo mô hình phân tách giao diện hợp đồng (API Contract) giữa Client và Backend. "
                    "Chi tiết nhiệm vụ của từng thành viên được trình bày tại Bảng 9.1."
                ),
            },
            {
                "type": "table",
                "caption": "Bảng 9.1: Bảng phân công công việc giữa các thành viên trong nhóm",
                "headers": ["Họ và tên", "Vai trò chính", "Phân công công việc chi tiết"],
                "rows": [
                    ["Lâm Gia Thịnh", "Unity Lead, AR & Mobile iOS", "Xây dựng không gian 3D IUH Campus, Story Director, Combat 3D & Jetpack, Match-3 RPG 6x6, Live AR & IUHARVisionAnalyzer, Unit Test NUnit và xuất bản Xcode lên iPhone 12."],
                    ["Nguyễn Minh Nhật", "Backend, AI, Docker & Cloud", "Xuất YOLOv8n ONNX opset 17, xây dựng Flask/Gunicorn API, SQLite Transactional Outbox, Sync Worker Adafruit IO 5 feeds, Prometheus/Grafana, đo p50/p95 và bộ kiểm thử pytest/D2."],
                ],
                "col_ratios": [0.22, 0.24, 0.54],
            },
            {"type": "h2", "text": "9.2. Nhật kí công việc"},
            {
                "type": "para",
                "text": (
                    "Trong suốt quá trình thực hiện, nhóm duy trì họp định kỳ 6 buổi chính thức (tổng thời gian họp và tích hợp trực tiếp là 18 giờ) "
                    "để rà soát tiến độ và giải quyết lỗi tích hợp (Bảng 9.2)."
                ),
            },
            {
                "type": "table",
                "caption": "Bảng 9.2: Nhật kí làm việc và họp nhóm thực hiện đồ án",
                "headers": ["Thời gian", "Nội dung cuộc họp và mốc công việc", "Kết quả đạt được"],
                "rows": [
                    ["01/11/2026", "Họp khởi động đề tài, chốt kiến trúc 3 tầng và hợp đồng API JSON.", "Thống nhất kế hoạch thực hiện và cấu trúc thư mục dự án."],
                    ["04/11/2026", "Xuất mô hình YOLOv8n sang ONNX; dựng không gian 3D trường IUH.", "Hoàn thành yolov8n.onnx (12,85 MB) và scene khuôn viên cơ bản."],
                    ["08/11/2026", "Phát triển Flask REST API (/predict, /metrics) và bàn cờ Match-3 RPG.", "API chạy ổn định trên Docker; lõi Match3.Core hoàn thiện 5 loại ngọc."],
                    ["12/11/2026", "Hoàn thiện SQLite Outbox, Adafruit IO 5 feeds và Live AR Scanner.", "Đồng bộ thành công dữ liệu ngoại tuyến và nhận diện vật thể AR."],
                    ["15/11/2026", "Cấu hình Prometheus/Grafana và biên dịch iOS chạy trên iPhone 12.", "Ứng dụng chạy 58–60 FPS trên thiết bị thật; giám sát đồ thị p50/p95."],
                    ["18/11/2026", "Chạy kiểm thử D2 (134 pytest), NUnit AR (8/8) và hoàn thiện báo cáo.", "Ghi nhận đầy đủ bằng chứng kiểm thử và hoàn tất báo cáo theo mẫu."],
                ],
                "col_ratios": [0.16, 0.44, 0.40],
            },
        ],
    },
]
