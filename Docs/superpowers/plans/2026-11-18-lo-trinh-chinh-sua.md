# Lộ trình chỉnh sửa IUH Campus Guardian (18/11/2026 → nộp, ~2,5 tuần)

Phạm vi Minh Nhật: **bản PC/laptop**. AR trên iOS: Gia Thịnh. Android: ghi là hướng phát triển trong tài liệu.
Mỗi lượt: nhánh `minhnhat/<tên>` → thiết kế ngắn → kế hoạch → Codex thực hiện → Claude kiểm → PR.

| # | Lượt | Mục đích (rubric) | Cỡ | Trạng thái |
|---|---|---|---|---|
| 1 | Gỡ Grab + menu minigame + code minigame cũ | Không menu 6 game, repo sạch (Unity) | Vừa–lớn | Thiết kế + kế hoạch xong: `2026-11-18-remove-grab-minigames*` |
| 2 | Nâng cấp kiếm/khiên/sinh lực có tác dụng trong match-3 | Vòng lặp thắng → Gold → nâng cấp (Unity) | Nhỏ | Chưa |
| 3 | Vườn hoa P2: 6 luống, trồng/tưới/bón, Hạt → Mầm → Nở (1 phút/giai đoạn, giờ thật), hái được Gold | "Chăm sóc khuôn viên" trong thiết kế PC; Thịnh yêu cầu thêm tính năng | Vừa | Chưa — cần bàn chi tiết |
| 4 | Thay model nhân vật người chơi (Humanoid) | Đồ họa (Unity) | Vừa | Chờ người dùng tự chọn model |
| 5 | Lỗi `Invalid AABB` + giảm `Find`/`Camera.main` trong `Update` | Ổn định khi demo | Vừa | Chưa |
| 6 | Cho bản iPhone cấu hình địa chỉ backend (IP LAN) | Cloud trên mobile (hỗ trợ Thịnh) | Nhỏ | Chưa |
| 7 | Hồ sơ: README, plan.md (Android = hướng phát triển), báo cáo kỹ thuật, bảng test tổng Pass/Fail, kịch bản + video demo PC | Tránh trừ 20 điểm | Vừa | Tuần cuối |

## Cần báo Thịnh (không thuộc phạm vi Minh Nhật)
- Màn quét AR hiện chữ "YOLOv8" nhưng dùng bộ đoán theo độ tương phản/cạnh (`IUHARVisionAnalyzer`), không gọi model; vật thể quét (balo/laptop/sách) lệch với backend (điện thoại/laptop/ghế). Thư viện `GuardianScanClient`/`GuardianScanService` đã sẵn để nối backend `/scan`.
- iPhone đang gọi `127.0.0.1` nên không tới được backend (lượt 6 hỗ trợ).
- Scene AR `IUH_Campus_AR_Encounter.unity` không có trong Build Settings — xác nhận cách build iOS.

## Ghi chú
- W9: Kubernetes là tùy chọn; ONNX, Flask/Gunicorn, Docker, Prometheus/Grafana đã đạt.
- Thứ tự có thể đổi khi có model nhân vật (lượt 4) hoặc Thịnh đưa thêm yêu cầu tính năng.
