# B2.7 — Chốt YOLOv8n 640

Ngày: 2026-11-04 (Asia/Bangkok). Quyết định của người dùng: giữ YOLOv8n640FP32, confidence≥0.70; không đổi code inference, model mặc định hoặc ORT threads. Base f5289d8.

## Chỉ tiêu mới theo số đo

YOLOv8n 640: API total p50 ≤ 150 ms, p95 ≤ 220 ms trên CPU (Ryzen 7 6800H, Docker, Gunicorn 2 worker); confidence ≥ 0.70. Ưu tiên độ chính xác; 416 nhanh hơn nhưng trượt nhiều hơn trên mẫu thử B2.

Đây là API total_time_ms (decode/preprocess/session.run/postprocess), không phải riêng inference_time_ms và không gồm mạng. Chỉ tiêu là mục tiêu vận hành theo CPU/cấu hình đã đo, không phải tuyên bố benchmark mới hoặc bảo đảm mọi máy đều đạt. Thread mặc định CPU/workers không đổi. Dải benchmark2worker/threads2,4,6,8 cao nhất là p50=129.346/p95=180.630ms; làm tròn lên150/220ms có biên khoảng16%/22%. Vì vậy không chọn100/150ms: threads6 có p95=163.609ms, threads8 có p50=129.346ms. Tải30s2client/2worker/threads6: total91.164/122.776ms, client97.742/130.565ms.

## So sánh theo evidence có sẵn

Nguồn: thread-benchmark.md (B2.5; Docker/Gunicorn2worker, ORT6threads/worker) và benchmark.txt (B2; docker compose exec chạy một process riêng, ORT2threads, không phải tải qua2worker). Không trộn hai phép đo thành một benchmark.

| Size FP32 | B2.5 Docker2worker: inference p50/p95 ms | B2.5 API total p50/p95 ms | B2 Docker exec: total p50/p95 ms | B2 laptop positives /2 ở0.70 | B2 synthetic negatives /2 |
|---|---|---|---|---|---|
|640|74.686/139.569|98.367/163.609|89.309/105.576|1/2|2/2|
|512|60.663/95.510|80.079/115.156|Không đo|Không có số đo trong B2|Không có số đo trong B2|
|480|64.920/106.678|85.644/132.246|46.217/55.234|0/2|2/2|
|416|28.170/40.974|41.037/56.687|35.340/43.920|0/2|2/2|

Mẫu B2 gồm2ảnh laptop có license rõ và2ảnh âm synthetic; không đại diện độ đúng thực tế/mAP. 640cũng bỏ sót1ảnh. Lý do ưu tiên độ chính xác chỉ dựa trên so sánh nhỏ này; chưa khẳng định độ chính xác trên thiết bị thực.

**Chưa đánh giá trên ảnh thật — sẽ bổ sung.** eval_real.py đã sẵn sàng; không tự tạo ảnh hoặc số liệu. Các bản416/480/512 và classes.json tương ứng không được đưa vào commit này. INT8 thử trước không chạy trên CPUExecutionProvider do ConvInteger, không chọn làm mặc định.

SHA256 model giữ nguyên: 4d0632282232a5fe4eedf59a951f371d3d6b98e4d78546813f1d0ef07b06bb40. Rubric gốc bắt đầu từ Assessment Rubric trong plan.txt giữ nguyên; chỉ cập nhật chỉ tiêu vận hành của nhóm. Không tự tuyên bố đã đạt chuẩn latency cũ của W9.
