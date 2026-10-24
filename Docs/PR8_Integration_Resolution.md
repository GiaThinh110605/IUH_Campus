# Tích hợp PR #8 với main — 24/10/2026

Đã tích hợp `main` tại `46aedaf` (PR #7) vào nhánh menu sáu game. Giải quyết ba file xung đột: scene chính, portal và trang phục.

- Giữ các đối tượng hub/phố, HUD và luồng Grab mới từ main; khôi phục các nhóm khuôn viên tham chiếu, Photo Hunter, Green Campus, camera/nhân vật và menu sáu game từ nhánh PR. Các nhóm hình học cũ chồng lên khuôn viên giữ ở trạng thái tắt như trước.
- Scene được ghép theo đối tượng và liên kết fileID. Kiểm tra trước import không có tham chiếu nội bộ bị treo hoặc fileID trùng. Unity đã nạp và lưu lại scene; không mất script.
- GameFlow mở hướng dẫn thật của Photo/Green thay vì chỉ dịch chuyển đến một địa điểm. Nút bắt đầu của từng game cập nhật trạng thái GameFlow; đi nhanh/khám phá tự do dừng đủ ba game. Ba game chưa hoàn thiện vẫn khóa ở cả menu và GameFlow.
- Giữ luồng chọn trang phục → xác nhận → chơi Grab. Hợp nhất việc xử lý M/Esc để portal không xử lý cùng phím lần thứ hai. Đóng/quay lại menu dùng cùng một đường chuyển trạng thái.
- Chuyển ba builder từ chạy tự động mỗi reload sang lệnh menu chủ động, tránh dựng lại/ghi đè scene khi import code đã tích hợp. Các chức năng builder và đối tượng đã có trong scene vẫn được giữ.
- Đồng bộ lại waypoint, trạm giao hàng và vị trí xe Grab theo khuôn viên tham chiếu; tắt lại bốn collider thanh cầu thang lỗi cũ. Lượt quét đầu ghi nhận 36/41 do năm điểm này; sau chỉnh scene đạt 41/41.

Kiểm chứng trên bản tích hợp trong Unity 6000.5.8f1:

| Kiểm tra | Kết quả |
|---|---:|
| Chọn Photo, bắt đầu thật, mở menu/quay lại, chuyển Green, đi nhanh, ba ô khóa, Grab và khám phá tự do | 11/11 |
| Trọn lượt Photo Hunter và Green Campus | 28/28 |
| Tuyến ngoài và landmark | 41/41 |
| Hành lang nối | 33/33 |
| Script bị mất trong scene | 0 |

Báo cáo và ảnh ở `Evidence/MergeIntegration`; thư mục `Runtime` chỉ dùng save kiểm thử cục bộ, không đưa vào Git. Kiểm tra luồng bật `Application.runInBackground` để đồng hồ game tiếp tục chạy khi điều khiển qua MCP. Kiểm tra lần đầu khi Editor không chạy nền bị bộ chống bấm liên tục chặn; lượt cuối sau cấu hình chạy nền đạt 11/11. Không đổi luật gameplay để bỏ qua bộ chống bấm.

Console cuối lượt không có lỗi. Đã thoát Play Mode và lưu scene. Chưa kiểm chứng Android hoặc hiệu năng bản build. Các file IDE/log/config tự sinh và thay đổi cục bộ không liên quan được giữ ngoài commit.
