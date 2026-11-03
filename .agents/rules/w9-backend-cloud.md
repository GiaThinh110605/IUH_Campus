# Quy chuẩn Backend AI, ONNX & Cloud IoT (W9 Backend & Cloud)

Tuân thủ chặt chẽ tiêu chuẩn triển khai Deep Learning Production theo tài liệu W9-En.pdf và tích hợp Adafruit IO.

## 1. Xuất và Tối ưu Mô hình ONNX (ONNX Export & Runtime)
- **Định dạng chuẩn**: Toàn bộ mô hình thị giác máy tính / Deep Learning (YOLOv8) phục vụ suy luận phải được chuyển đổi sang `.onnx` (Open Neural Network Exchange).
- **Suy luận Production với ONNX Runtime**:
  - Không tải lại mô hình trong mỗi request; `InferenceSession` phải được khởi tạo một lần duy nhất lúc backend khởi động.
  - Tối ưu hóa thực thi (CPU Execution Provider / CoreML trên Mac / CUDA trên Linux GPU).
  - YOLOv8n 640: API total p50 ≤ 150 ms, p95 ≤ 220 ms trên CPU (Ryzen 7 6800H, Docker, Gunicorn 2 worker); confidence ≥ 0.70. Ưu tiên độ chính xác; 416 nhanh hơn nhưng trượt nhiều hơn trên mẫu thử B2.
  - Ngưỡng lọc phát hiện hợp lệ: `confidence >= 0.70` (70%).

## 2. Tiêu chuẩn Docker & Web Service Backend
- **Kiến trúc Server**:
  - Web framework: Flask kết hợp với Gunicorn WSGI production server (`gunicorn -w 2 -b 0.0.0.0:5001 wsgi:app`).
  - Containerization: Phải có `Dockerfile` đa nền tảng (hỗ trợ cả Apple Silicon ARM64 và Linux AMD64) và `docker-compose.yml` để khởi chạy đồng bộ.
- **Giám sát Prometheus (Prometheus Monitoring)**:
  - Tích hợp `prometheus_flask_exporter` theo đúng yêu cầu W9.
  - Endpoint `/metrics` phải luôn sẵn sàng để Prometheus / Grafana thu thập dữ liệu (request count, latency, memory).
  - Endpoint `/health` kiểm tra trạng thái hoạt động của server và mô hình.

## 3. Bảo mật & Đồng bộ IoT Cloud (Adafruit IO)
- **Tuyệt đối không hardcode thông tin nhạy cảm**:
  - `ADAFRUIT_AIO_USERNAME` và `ADAFRUIT_AIO_KEY` bắt buộc phải đọc từ file `.env` hoặc biến môi trường hệ thống.
  - Cấm commit file `.env` chứa khóa thật lên Git public. Cung cấp file `.env.example` làm mẫu.
- **Quy chuẩn Feed Adafruit IO**:
  - Feed số lượng đồ vật tìm thấy: `lost-found-count`.
  - Giới hạn tần suất gọi API (Rate limit): Đồng bộ theo sự kiện (khi tìm thấy đồ vật thành công hoặc sau mỗi mốc kết quả), không gọi liên tục gây nghẽn hạn ngạch Adafruit IO Free tier (tối đa 30 data points/phút).

