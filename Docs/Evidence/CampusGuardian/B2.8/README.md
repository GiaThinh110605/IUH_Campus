# B2.8 — Pool lớp nhiễm

Commit nền: `64b1e2b801d0de012700e9912819de8fbcae1edc`; các thay đổi B2.8 được commit cùng evidence này.
Ngày kiểm chứng: 2026-11-07T15:58:49.930985+07:00. Windows, Python 3.11, SQLite tạm; không sửa .env thật hoặc chạy Docker/cloud.

## Pytest

Lệnh từ repository root: `Server/.venv/Scripts/python.exe -m pytest Server/tests -q`.
Kết quả: **134 passed in 12.87s**, exit 0; [output đầy đủ](pytest.txt). Có cảnh báo cleanup pyreadline3 trên Windows sau pytest, không có test failed/skipped.
Test mới kiểm tra pool mặc định, tên sai/rỗng, subset ALLOWED_CLASSES, pool một lớp/clamp/deduplicate, gợi ý điện tử không lặp, đồ ngoài pool vẫn nhận diện nhưng clean và phiên SQLite cũ sau đổi pool.
Các test token/reward/anti-farm cũ dùng pool sáu lớp riêng; chỉ đổi kỳ vọng gợi ý điện tử từ lặp hai lần thành một lần theo yêu cầu mới.

## HTTP thật

Lệnh: `Server/.venv/Scripts/python.exe Docs/Evidence/CampusGuardian/B2.8/smoke_http.py`.
Chạy Flask/Werkzeug trên localhost cổng tạm, nạp ONNX thật, không mock random/route; dùng SQLite tạm, tắt cloud credentials. Ba request POST /scan/sessions đều HTTP 201. Backend không trả danh sách lớp nhiễm.
Phản hồi nguyên bản: [scan-sessions.json](scan-sessions.json). Lượt đầu có max_monsters=2 nhưng hint chỉ “Tín hiệu nhiễu từ đồ điện tử.”, xác nhận không lặp chữ. Hai lượt tiếp theo trả gợi ý ghế và ghế/điện tử.
Sandbox chặn localhost ở lần thử đầu (WinError 10013); lần chạy có quyền truy cập socket đã thành công. Server tạm được shutdown sau kiểm tra.

## SHA256 mã đã kiểm chứng

- `Server/app/config.py`: `210f6332e7473f0bc0a0cb935f15b2429f539251498c3df9491f608f8ca3c234`.
- `Server/app/scan_store.py`: `0d9c2dd440dd585a8d02b32a4af6470d2ae7fd3026295f279222b802955c5ad0`.
- `Server/tests/test_scan_pool.py`: `fc2d1d5245c6191391b05bde002f4735398ee0a21a74e91005131c56a5a72b86`.
- `Server/tests/test_scan_sessions.py`: `c41e6f3fdb5f244236c1981a25c4033d06940e41dafb091f10b0a94289be18ea`.
