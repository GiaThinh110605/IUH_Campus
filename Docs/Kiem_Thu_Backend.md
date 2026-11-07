# Kiểm thử backend — bổ sung G09/B2.6

Lượt D2 trước đã dừng trước khi chạy; nhánh origin/main chưa có bảng D2. Tài liệu này chỉ ghi kết quả B2.6 thực sự đã chạy, không xác nhận Docker/cloud/Unity của D2.

Môi trường: Windows-10-10.0.26200-SP0; AMD Ryzen 7 6800H with Radeon Graphics; ONNX Runtime CPU, 2 threads. Ngày: 2026-11-07T15:47:20.174724+07:00. Commit nền: `a9e13e01851bfe01ede0abde70420cefca741552`; mã đánh giá được commit cùng báo cáo B2.6.

Ảnh: 36 phát hiện, loại book1/book2/none3 vì ảnh gốc >5 MiB theo xác nhận người dùng, còn 33 (29 dương, 4 none). Nhãn theo thư mục; EXIF Orientation trước resize cạnh dài ≤640, không upscale. Tất cả ảnh đánh giá không có EXIF điện thoại: nguồn không rõ/ảnh mạng. Không lưu GPS, không commit ảnh.

Quy ước cho từng lớp: mong đợi mọi ảnh có nhãn tìm thấy target ở 0.70; Pass nếu tìm đủ, Fail nếu có bỏ sót. Đây là kiểm tra sự hiện diện theo nhãn người dùng, không có ground truth bbox/mAP. Không đặt thêm tiêu chuẩn recall sản phẩm.

| ID | Ca kiểm thử | Nền tảng/môi trường | Commit | Bước | Mong đợi | Thực tế | Pass/Fail | Bằng chứng |
|---|---|---|---|---|---|---|---|---|
| G09-B2.6-backpack | Nhận diện backpack | Windows/CPU/640 | a9e13e0 + B2.6 script | eval_real.py, threshold 0.70, target_class=backpack | 5/5 target_found | 0/5 (0.0%) | Fail | [real-eval.md](Evidence/CampusGuardian/B2/real-eval.md), [raw JSON](Evidence/CampusGuardian/B2/real-eval.json) |
| G09-B2.6-book | Nhận diện book | Windows/CPU/640 | a9e13e0 + B2.6 script | eval_real.py, threshold 0.70, target_class=book | 3/3 target_found | 0/3 (0.0%) | Fail | [real-eval.md](Evidence/CampusGuardian/B2/real-eval.md), [raw JSON](Evidence/CampusGuardian/B2/real-eval.json) |
| G09-B2.6-bottle | Nhận diện bottle | Windows/CPU/640 | a9e13e0 + B2.6 script | eval_real.py, threshold 0.70, target_class=bottle | 5/5 target_found | 0/5 (0.0%) | Fail | [real-eval.md](Evidence/CampusGuardian/B2/real-eval.md), [raw JSON](Evidence/CampusGuardian/B2/real-eval.json) |
| G09-B2.6-cell-phone | Nhận diện cell phone | Windows/CPU/640 | a9e13e0 + B2.6 script | eval_real.py, threshold 0.70, target_class=cell phone | 5/5 target_found | 3/5 (60.0%) | Fail | [real-eval.md](Evidence/CampusGuardian/B2/real-eval.md), [raw JSON](Evidence/CampusGuardian/B2/real-eval.json) |
| G09-B2.6-chair | Nhận diện chair | Windows/CPU/640 | a9e13e0 + B2.6 script | eval_real.py, threshold 0.70, target_class=chair | 6/6 target_found | 4/6 (66.7%) | Fail | [real-eval.md](Evidence/CampusGuardian/B2/real-eval.md), [raw JSON](Evidence/CampusGuardian/B2/real-eval.json) |
| G09-B2.6-laptop | Nhận diện laptop | Windows/CPU/640 | a9e13e0 + B2.6 script | eval_real.py, threshold 0.70, target_class=laptop | 5/5 target_found | 5/5 (100.0%) | Pass | [real-eval.md](Evidence/CampusGuardian/B2/real-eval.md), [raw JSON](Evidence/CampusGuardian/B2/real-eval.json) |
| G09-B2.6-none | Ảnh không có lớp mục tiêu | Windows/CPU/640 | a9e13e0 + B2.6 script | Mỗi ảnh thử 6 target_class tại 0.70 | Không dương giả | 0/4 ảnh dương giả | Pass | [raw JSON](Evidence/CampusGuardian/B2/real-eval.json) |
| G09-B2.6-regression | Nhãn/EXIF/resize/source/summary + model thật | Windows/Python 3.11 | a9e13e0 + B2.6 script | pytest test_real_labels.py test_real_eval_protocol.py test_real_model.py | Tất cả pass | 15 passed; exit 0; pyreadline3 báo lỗi cleanup console sau pytest | Pass | [test output](Evidence/CampusGuardian/B2/real-eval-tests.txt) |

Kết quả tại 0.70: 12/29 ảnh dương đúng (41.4%), 17 bỏ sót; 0/4 ảnh none dương giả. Các trường hợp trượt và nhãn/confidence model thực sự thấy được nằm trong báo cáo chi tiết. Ngưỡng 0.50/0.60 cùng đạt 15/29 (51.7%); không thay model hay ngưỡng mặc định.

Timing tổng tại 0.70: p50 409.77 ms, p95 732.09 ms cho protocol local đọc/resize/suy luận, gồm sáu lượt suy luận cho mỗi ảnh none. Không dùng số này kết luận SLA /predict hoặc Docker. Chi tiết mọi ngưỡng: [B2.6](Evidence/CampusGuardian/B2/real-eval.md).

Các ca Docker, cloud/outbox, scan/token, Unity/iPhone của D2: **Chưa kiểm thử trong lượt B2.6**. Không suy rộng kết quả đánh giá local thành Pass các ca tích hợp. Mẫu nhỏ, nguồn EXIF suy đoán, ảnh mạng có thể dễ hơn camera thật khi chơi.
