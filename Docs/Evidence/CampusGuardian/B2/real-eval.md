# B2.6 — Đánh giá YOLOv8n 640 trên ảnh thật

- Ngày (Asia/Bangkok): 2026-11-07T15:47:20.174724+07:00
- Commit nền: `a9e13e01851bfe01ede0abde70420cefca741552`; script đánh giá thay đổi trong commit B2.6 (SHA256 trong JSON).
- OS: Windows-10-10.0.26200-SP0; CPU: AMD Ryzen 7 6800H with Radeon Graphics; ONNX CPUExecutionProvider, 2 intra-op threads.
- Model: YOLOv8n 640 FP32, SHA256 `4d0632282232a5fe4eedf59a951f371d3d6b98e4d78546813f1d0ef07b06bb40`. Backend vẫn mặc định 0.70.
- Phát hiện 36 ảnh; đánh giá 33; loại 3 ảnh gốc >5 MiB theo xác nhận người dùng.
- Nhãn theo thư mục do người dùng xác nhận, cellphone = cell phone; không suy ra nhãn từ nội dung ảnh.
- Preprocessing: EXIF Orientation trước RGB/resize LANCZOS; cạnh dài ≤640 px, không upscale. Cùng giới hạn kích thước GuardianImageEncoder, không mô phỏng Unity JPEG/render filter.
- Nguồn: chỉ đọc EXIF Make/Model để suy đoán điện thoại; không có EXIF điện thoại = không rõ/ảnh mạng, không khẳng định là ảnh mạng. Không đọc/in/lưu GPS.
- Timing: local disk decode + EXIF orientation + resize + detector preprocessing/inference/postprocessing; no HTTP; none = six sequential target calls; diagnostic excluded; five warmups
- Đây là recall theo sự hiện diện lớp ở cấp ảnh, không phải mAP hay đánh giá bounding box. Ảnh none: thử đủ sáu target_class, FP nếu bất kỳ lần nào target_found.
- Confidence đúng = trung bình best_confidence của target trên các ảnh dương đúng (không tính none).
- Raw predictions, timing từng ảnh, manifest và metadata được lưu trong [real-eval.json](real-eval.json).

| Nhãn | Số ảnh đánh giá |
|---|---:|
| backpack | 5 |
| book | 3 |
| bottle | 5 |
| cell phone | 5 |
| chair | 6 |
| laptop | 5 |
| none | 4 |

## Kết quả tổng

| Ngưỡng | Recall từng lớp | Recall tổng | FP none | Conf đúng TB | p50 ms | p95 ms |
|---:|---|---:|---:|---:|---:|---:|
| 0.50 | backpack: 0/5 (0.0%); book: 0/3 (0.0%); bottle: 2/5 (40.0%); cell phone: 4/5 (80.0%); chair: 4/6 (66.7%); laptop: 5/5 (100.0%) | 15/29 (51.7%) | 0/4 | 0.8495 | 433.23 | 756.79 |
| 0.60 | backpack: 0/5 (0.0%); book: 0/3 (0.0%); bottle: 2/5 (40.0%); cell phone: 4/5 (80.0%); chair: 4/6 (66.7%); laptop: 5/5 (100.0%) | 15/29 (51.7%) | 0/4 | 0.8495 | 399.81 | 724.92 |
| 0.70 | backpack: 0/5 (0.0%); book: 0/3 (0.0%); bottle: 0/5 (0.0%); cell phone: 3/5 (60.0%); chair: 4/6 (66.7%); laptop: 5/5 (100.0%) | 12/29 (41.4%) | 0/4 | 0.8971 | 409.77 | 732.09 |

## Nguồn từng ảnh

| File | Nguồn suy đoán | Make / Model |
|---|---|---|
| backpack/backpack1.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| backpack/backpack2.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| backpack/backpack3.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| backpack/backpack4.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| backpack/backpack5.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| book/book3.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| book/book4.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| book/book5.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| bottle/bottle1.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| bottle/bottle2.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| bottle/bottle3.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| bottle/bottle4.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| bottle/bottle5.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| cellphone/cellphone1.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| cellphone/cellphone2.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| cellphone/cellphone3.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| cellphone/cellphone4.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| cellphone/cellphone5.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| chair/chair1.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| chair/chair2.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| chair/chair3.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| chair/chair4.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| chair/chair5.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| chair/chair6.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| laptop/laptop1.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| laptop/laptop2.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| laptop/laptop3.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| laptop/laptop4.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| laptop/laptop5.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| none/none1.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| none/none2.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| none/none4.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |
| none/none5.jpg | Không rõ/ảnh mạng (không có EXIF điện thoại) |  /  |

## Ảnh trượt tại 0.70

Dự đoán giải thích dùng ngưỡng chẩn đoán 0.01, không tính vào kết quả 0.70. Chỉ liệt kê sáu lớp mục tiêu.

| File | Nhãn mong đợi | Model thấy (nhãn/conf tại ≥0.01) |
|---|---|---|
| backpack/backpack1.jpg | backpack | backpack 0.3728, laptop 0.0217 |
| backpack/backpack2.jpg | backpack | không có lớp mục tiêu ≥0.01 |
| backpack/backpack3.jpg | backpack | không có lớp mục tiêu ≥0.01 |
| backpack/backpack4.jpg | backpack | backpack 0.0251 |
| backpack/backpack5.jpg | backpack | không có lớp mục tiêu ≥0.01 |
| book/book3.jpg | book | book 0.0342 |
| book/book4.jpg | book | laptop 0.0656 |
| book/book5.jpg | book | laptop 0.2111, book 0.0369, cell phone 0.0158 |
| bottle/bottle1.jpg | bottle | bottle 0.6062, bottle 0.0296 |
| bottle/bottle2.jpg | bottle | không có lớp mục tiêu ≥0.01 |
| bottle/bottle3.jpg | bottle | bottle 0.6792 |
| bottle/bottle4.jpg | bottle | không có lớp mục tiêu ≥0.01 |
| bottle/bottle5.jpg | bottle | không có lớp mục tiêu ≥0.01 |
| cellphone/cellphone1.jpg | cell phone | cell phone 0.6906 |
| cellphone/cellphone5.jpg | cell phone | cell phone 0.3586, cell phone 0.0568, book 0.0439, cell phone 0.0256, cell phone 0.0236, cell phone 0.0234, laptop 0.0171 |
| chair/chair5.jpg | chair | không có lớp mục tiêu ≥0.01 |
| chair/chair6.jpg | chair | chair 0.0917 |

## Ảnh loại

- book/book1.jpg: 11263338 bytes; original >5 MiB (user-confirmed exclusion).
- book/book2.jpg: 5802668 bytes; original >5 MiB (user-confirmed exclusion).
- none/none3.jpg: 12636209 bytes; original >5 MiB (user-confirmed exclusion).

## Giới hạn

Mẫu nhỏ: đầu bài dự kiến 35 ảnh, thực tế có 36 ảnh, sau lọc còn 33. Không tự loại ảnh để khớp con số dự kiến. Ảnh mạng có thể dễ hơn camera thật lúc chơi; nguồn suy đoán EXIF chưa được xác minh. Loại ảnh nặng có thể gây thiên lệch mẫu.
Không đo upload/HTTP, JPEG encoding Unity, camera iOS, blur khi di chuyển, combat hay cloud. Timing none gồm sáu lần inference nên không dùng thay benchmark /predict một request. Không điều chỉnh model/ngưỡng từ tập này; cần tập kiểm thử độc lập lớn hơn nếu dùng kết quả để tối ưu.
