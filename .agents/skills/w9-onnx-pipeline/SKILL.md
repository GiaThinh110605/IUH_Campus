---
name: w9-onnx-pipeline
description: Xuất mô hình YOLOv8 sang định dạng ONNX, khởi chạy backend Flask/Gunicorn qua Docker, kiểm tra giám sát Prometheus /metrics và đồng bộ dữ liệu lên Adafruit IO theo đúng chuẩn W9-En.
---

# W9 ONNX & Backend Pipeline

Kỹ năng này hướng dẫn và cung cấp các quy trình chuẩn để xuất mô hình ONNX, triển khai backend AI qua Docker và đồng bộ IoT Cloud theo tiêu chuẩn Tuần 9.

## 1. Chuẩn bị Môi trường & File .env
Đảm bảo thư mục `Server` có file `.env` chứa thông tin xác thực Adafruit IO:
```bash
ADAFRUIT_AIO_USERNAME="Thinh2005"
ADAFRUIT_AIO_KEY="your_adafruit_aio_key_here"
ADAFRUIT_FEED_KEY="lost-found-count"
PORT=5001
CONFIDENCE_THRESHOLD=0.70
```

## 2. Xuất Mô hình YOLOv8 sang ONNX
Chạy script chuyển đổi mô hình từ PyTorch sang ONNX tối ưu:
```bash
cd /Users/mac/Unity/IUH_Campus/Server
python3 export_onnx.py
```
- Kiểm tra file output: `models/yolov8n.onnx` phải tồn tại.
- Kích thước ảnh chuẩn vào: `640x640`.
- Chế độ suy luận: Dynamic batch hoặc fixed batch size 1; mục tiêu: YOLOv8n 640: API total p50 ≤ 150 ms, p95 ≤ 220 ms trên CPU (Ryzen 7 6800H, Docker, Gunicorn 2 worker); confidence ≥ 0.70. Ưu tiên độ chính xác; 416 nhanh hơn nhưng trượt nhiều hơn trên mẫu thử B2.

## 3. Triển khai Docker Container
Khởi động backend bằng Docker Compose:
```bash
cd /Users/mac/Unity/IUH_Campus/Server
docker compose down
docker compose up -d --build
```
Kiểm tra container đang chạy:
```bash
docker compose ps
docker compose logs -f --tail=50
```

## 4. Xác minh các Endpoint & Prometheus Metrics
Thực hiện gọi API kiểm tra:
- **Kiểm tra trạng thái (Health Check)**:
  ```bash
  curl -s http://localhost:5001/health | jq .
  ```
- **Kiểm tra Prometheus Metrics (/metrics)**:
  ```bash
  curl -s http://localhost:5001/metrics | grep flask_http_request_duration_seconds
  ```
- **Kiểm tra Suy luận Nhận diện đồ vật (/predict)**:
  ```bash
  curl -X POST http://localhost:5001/predict \
    -F "image=@sample.jpg" \
    -F "threshold=0.70"
  ```

## 5. Xác minh Đồng bộ Adafruit IO
Khi nhận diện đồ vật thành công (ví dụ `backpack`, `book`, `laptop`), server tự động tăng số đếm và cập nhật lên feed:
- Kiểm tra qua REST API Adafruit IO:
  ```bash
  curl -H "X-AIO-Key: $ADAFRUIT_AIO_KEY" \
    https://io.adafruit.com/api/v2/Thinh2005/feeds/lost-found-count/data/last
  ```

