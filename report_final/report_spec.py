# -*- coding: utf-8 -*-
"""
Report metadata, Summary, Lists of Figures/Tables, and References
strictly following ReportTemplate.pdf structure + the 7 formatting rules.
"""

METADATA = {
    "university": "TRƯỜNG ĐẠI HỌC CÔNG NGHIỆP TP. HỒ CHÍ MINH",
    "faculty": "KHOA CÔNG NGHỆ THÔNG TIN",
    "course": "CÔNG NGHỆ MỚI TRONG PHÁT TRIỂN ỨNG DỤNG CNTT",
    "title": (
        "IUH CAMPUS GUARDIAN: HỆ THỐNG TRÒ CHƠI 3D TÍCH HỢP\n"
        "THỰC TẾ TĂNG CƯỜNG, THỊ GIÁC MÁY TÍNH ONNX VÀ CLOUD IOT"
    ),
    "authors": [
        {"name": "Lâm Gia Thịnh", "role": "Unity Client 2D/3D, Live AR, Mobile iOS"},
        {"name": "Nguyễn Minh Nhật", "role": "Backend REST API, ONNX Runtime, Docker, Adafruit IO, Monitoring"},
    ],
    "advisor": "ThS. Nguyễn Ngọc Lễ",
    "location_date": "Tp. Hồ Chí Minh, Tháng 11/2026",
}

SUMMARY = [
    (
        "Giới thiệu",
        "Đề tài IUH Campus Guardian giải quyết bài toán xây dựng hệ thống ứng dụng tương tác đa tầng kết hợp mô phỏng "
        "không gian 3D khuôn viên Trường Đại học Công nghiệp TP. Hồ Chí Minh, trò chơi chiến thuật 2D Match-3 RPG, "
        "tính năng thực tế tăng cường (AR) nhận diện vật thể bằng mô hình học sâu và đồng bộ dữ liệu thời gian thực lên Cloud IoT. "
        "Ý nghĩa của bài toán là giải quyết sự kết nối tin cậy giữa đồ họa di động, suy luận AI nhẹ trên CPU và cơ chế lưu trữ bền vững khi mạng gián đoạn."
    ),
    (
        "Phương pháp làm",
        "Hệ thống áp dụng kiến trúc ba tầng tách biệt. Tầng Client phát triển trên Unity 6000 (URP) tách các lõi thuật toán thuần C# "
        "(Match3.Core, Story.Core, Garden.Core) để kiểm thử độc lập, tích hợp Live AR bằng WebCamTexture kết hợp bộ trích xuất "
        "đặc trưng lưới 6x6 nội bộ (IUHARVisionAnalyzer) và hàng đợi ngoại tuyến (GuardianEventOutbox). Tầng Backend đóng gói "
        "5 dịch vụ Docker Compose (Nginx Reverse Proxy, Flask/Gunicorn API chạy YOLOv8n ONNX FP32 trên CPU, tiến trình Sync Worker, "
        "Prometheus và Grafana) nối SQLite theo mẫu Transactional Outbox. Tầng Cloud sử dụng Adafruit IO REST API v2 đồng bộ 5 feeds."
    ),
    (
        "Kết quả",
        "Hệ thống hoàn thiện gồm 177 tệp C# runtime/kiểm thử, 60 công cụ Editor, 18 mô-đun Python Backend và 3 màn chơi chính. "
        "Toàn bộ 134 ca kiểm thử pytest phía Backend và 8 ca NUnit thị giác AR đều đạt Pass. Mô hình YOLOv8n kích thước 640x640 "
        "trên Docker 2 Gunicorn workers đạt độ trễ API trung vị p50 = 96,21 ms (đạt chỉ tiêu <= 150 ms), p95 = 257,22 ms trong phép đo tải liên tiếp "
        "và đạt tỷ lệ nhận diện 75,0% trên nhóm 3 lớp mục tiêu mặc định (laptop 100%, ghế 66,7%, điện thoại 60,0%) với 0/4 dương giả trên ảnh trống. "
        "Bản build iOS đã triển khai và chạy ổn định ở 58–60 FPS trên iPhone 12."
    ),
    (
        "Nhận xét",
        "Mẫu thiết kế Outbox hai tầng cùng phân tách lõi C# thuần giúp ứng dụng hoạt động ổn định ngay cả khi mất mạng và dễ kiểm chứng tự động. "
        "Tuy nhiên, mô hình YOLOv8n tiền huấn luyện trên tập COCO vẫn còn hạn chế nhận diện với sách và ba lô chụp cận cảnh ở ngưỡng 0,70, "
        "đồng thời độ trễ đuôi p95 cần được tối ưu thêm bằng cân bằng tải đa nút trong các phiên bản mở rộng."
    ),
]

LIST_OF_FIGURES = [
    ("Hình 1.1", "Ngữ cảnh tích hợp ba tầng của bài toán IUH Campus Guardian"),
    ("Hình 5.1", "Mô hình tổng quát hệ thống IUH Campus Guardian"),
    ("Hình 5.2", "Sơ đồ luồng dữ liệu đồng bộ hai tầng theo Transactional Outbox"),
    ("Hình 5.3", "Lưu đồ tiền xử lý ảnh và phân tích thị giác nội bộ IUHARVisionAnalyzer"),
    ("Hình 6.1", "Kiến trúc phân tầng Assembly Definition (.asmdef) trong Unity"),
    ("Hình 6.2", "So sánh độ trễ API p50 và p95 theo kích thước ảnh đầu vào YOLOv8n"),
    ("Hình 6.3", "Tỷ lệ nhận diện (Recall) theo từng lớp vật thể ở ngưỡng 0,50 và 0,70"),
]

LIST_OF_TABLES = [
    ("Bảng 2.1", "Tổng hợp mục tiêu và đối chiếu tiêu chí đánh giá học phần"),
    ("Bảng 3.1", "Tổng hợp yêu cầu chức năng và phi chức năng của hệ thống"),
    ("Bảng 3.2", "Đặc tả tập dữ liệu ảnh thực tế và các lớp đối tượng mục tiêu"),
    ("Bảng 4.1", "Đối chiếu các nghiên cứu và phương pháp kỹ thuật liên quan"),
    ("Bảng 5.1", "Danh sách 5 dịch vụ trong cụm Docker Compose"),
    ("Bảng 5.2", "Các điểm cuối API Backend và 5 nguồn dữ liệu Adafruit IO"),
    ("Bảng 6.1", "Phân hệ chức năng chính trong mã nguồn Unity Client"),
    ("Bảng 6.2", "Thông số cân bằng chiến đấu Match-3 RPG và nâng cấp Campus Core"),
    ("Bảng 6.3", "So sánh hiệu năng suy luận YOLOv8n giữa các kích thước đầu vào (B2.5)"),
    ("Bảng 6.4", "Kết quả đánh giá nhận diện trên tập 33 ảnh thực tế (B2.6)"),
    ("Bảng 6.5", "Tổng hợp kết quả kiểm thử tự động Backend (D2) và thị giác AR (NUnit)"),
    ("Bảng 6.6", "Thông số vận hành thực tế trên thiết bị di động iPhone 12"),
    ("Bảng 8.1", "Tiêu chí đánh giá đồ án theo quy định môn học"),
    ("Bảng 9.1", "Bảng phân công công việc giữa các thành viên trong nhóm"),
    ("Bảng 9.2", "Nhật kí làm việc và họp nhóm thực hiện đồ án"),
]

REFERENCES = [
    (
        1,
        "Unity Technologies (2025). Unity 6000 (Unity 6) User Manual: Universal Render Pipeline and Input System Documentation. "
        "Unity Technologies. https://docs.unity3d.com/Manual/"
    ),
    (
        2,
        "Terven, J., Córdova-Esparza, D.-M., & Romero-González, J.-A. (2023). A Comprehensive Review of YOLO Architectures in Computer Vision: "
        "From YOLOv1 to YOLOv8 and YOLO-NAS. Machine Learning and Knowledge Extraction, 5(4), 1680–1716. "
        "https://doi.org/10.3390/make5040083"
    ),
    (
        3,
        "Jocher, G., Chaurasia, A., & Qiu, J. (2023). Ultralytics YOLOv8: Real-Time Object Detection and Instance Segmentation (Version 8.0). "
        "Ultralytics Corporation. https://github.com/ultralytics/ultralytics"
    ),
    (
        4,
        "Microsoft Corporation & ONNX Runtime Developers (2024). ONNX Runtime: Cross-Platform, High Performance Machine Learning "
        "Inferencing and Training Accelerator. Microsoft Corporation. https://onnxruntime.ai/docs/"
    ),
    (
        5,
        "Richardson, C. (2018). Microservices Patterns: With Examples in Java (ISBN 978-1617294549). "
        "Manning Publications. https://microservices.io/patterns/data/transactional-outbox.html"
    ),
    (
        6,
        "Nguyễn Ngọc Lễ (2026). Tài liệu hướng dẫn học phần New Technology in IT Application Development (W9-En): "
        "Deploy an ML model from a research environment setting to production. Trường Đại học Công nghiệp TP. Hồ Chí Minh."
    ),
    (
        7,
        "Szeliski, R. (2022). Computer Vision: Algorithms and Applications (2nd ed., ISBN 978-3-030-34372-9). Springer Nature. "
        "https://doi.org/10.1007/978-3-030-34372-9"
    ),
    (
        8,
        "Docker Inc. (2024). Docker Compose Specification (Version 2) and Multi-Stage Container Best Practices. "
        "Docker Documentation. https://docs.docker.com/compose/"
    ),
    (
        9,
        "Brazil, B., & Pivotto, J. (2023). Prometheus: Up & Running — Infrastructure and Application Performance Monitoring "
        "(2nd ed., ISBN 978-1098131142). O'Reilly Media. https://prometheus.io/docs/"
    ),
    (
        10,
        "Adafruit Industries (2025). Adafruit IO HTTP REST API v2 Documentation and Rate Limiting Guidelines. "
        "Adafruit Learning System. https://io.adafruit.com/api/docs/"
    ),
]
