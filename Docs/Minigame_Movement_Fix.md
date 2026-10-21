# Sửa lỗi bay lên trời và camera từ trên cao — 21/10/2026

## Nguyên nhân đã tái hiện

Trong scene chính, bốn đối tượng `Block_D_StairMidRail_Left_Fl1` tới `Fl4` có mesh rỗng (0 vertices) nhưng BoxCollider mặc định vẫn bật. Do scale của mô hình nhập vào, mỗi collider chiếm khoảng **152 × 100 × 150 mét**, bao trùm cả sân. Một lần nhảy tại điểm bắt đầu Green Campus đẩy nhân vật từ y ≈ 0,49 lên y ≈ **66,57**. Photo Hunter tiếp tục tại vị trí đó nên chỉ thấy khuôn viên từ trên cao.

## Thay đổi

- Vô hiệu hóa đúng bốn collider rỗng này trong `IUH_Campus_Main.unity`. Không dịch chuyển, đổi kích thước hay thay hình học công trình; lan can tầng 5 có hình học hợp lệ vẫn giữ collider.
- Controller kiểm tra tiếp đất trước khi tính nhảy; tiêu thụ lệnh nhảy sau một lần thực hiện. Xóa input còn lưu khi bật lại điều khiển; reset vận tốc và bộ đếm sau dịch chuyển.
- Khi bắt đầu game, khôi phục camera theo `PlayerCameraRoot` và quyền đi bộ. Hủy trạng thái xe Grab kể cả khi người chơi lên xe ngoài nhiệm vụ.
- Phím Space/Enter chụp ảnh vẫn hoạt động khi con trỏ nằm trên HUD; chỉ thao tác chuột trên nút UI được loại khỏi lệnh chụp chung.

## Kiểm chứng

- Các bài kiểm tra logic cô lập: Photo Hunter **35/35**, Green Campus **32/32**.
- Lượt kiểm tra chuyển động cuối: **24/24 đạt**, lấy mẫu **2.871 khung hình**. Green Campus có y = 0,49–1,66 m, Photo Hunter có y = 0,44–1,62 m (bao gồm lúc nhảy).
- Bài kiểm tra mới chạy trong **scene khuôn viên thật, Play Mode**, đưa sự kiện bàn phím/chuột qua Unity Input System, để PlayerInput, Update, CharacterController và Cinemachine chạy qua nhiều khung hình. Không gọi Move hoặc sửa vị trí mỗi khung hình để tạo kết quả.
- Kiểm tra bắt đầu khi có lệnh Space còn giữ, nhảy/đáp, WASD, F nhặt rác, phím số phân loại, C mở máy ảnh, chuột ngắm, Space chụp/lưu JPEG, Q thoát ngắm và chuyển game từ trạng thái con trỏ hoặc xe Grab.
- Dùng thư mục save kiểm thử riêng. Thiết lập nhận input khi Codex đang ở phía trước chỉ áp dụng trong bài kiểm tra và được khôi phục khi kết thúc.
- Kết quả chi tiết: [playmode.json](Evidence/MovementRegression/playmode.json). Hình sau sửa: [Green Campus](Evidence/MovementRegression/green-grounded.png), [Photo Hunter](Evidence/MovementRegression/photo-grounded.png).

Ghi chú công cụ: chức năng chụp ảnh đồng bộ của MCP đã phát sinh lỗi Editor `PlayerLoop ... called recursively`, stack tại `MCPForUnity.Runtime.Helpers.ScreenshotUtility.CaptureCompositedAfterFrame:197` khi gọi `EditorApplication.Step`. Ảnh Green cuối được chụp bằng `ScreenCapture.CaptureScreenshot` ở cuối frame để tránh bước đồng bộ này. Đây là lỗi phát sinh trong thao tác chụp kiểm chứng; không ghi nhận là lỗi compile của game.

Để chạy lại: mở scene chính, Play, chọn **IUH Campus → Tests → Run Movement Regression (Play Mode)**. Trong lúc bài kiểm tra chạy, không thao tác bàn phím/chuột và không dừng Play. Kết quả nằm ở đường dẫn JSON phía trên; các lượt thử không thay thế việc kiểm tra trải nghiệm bằng tay.
