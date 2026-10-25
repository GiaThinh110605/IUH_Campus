# Scene chung và menu game — 24/10/2026

Thiết kế menu ba thẻ trong tài liệu này đã được thay bằng menu đủ sáu game. Xem [bản thiết kế sáu game](Six_Game_Menu_Revision.md) cho trạng thái hiện tại; phần kiểm chứng đồng bộ scene dưới đây là lịch sử của lượt tích hợp đầu.

Scene sử dụng: `Assets/IUH_Campus/Scenes/IUH_Campus_Main.unity`.

Photo Hunter (Thợ săn khoảnh khắc) và Green Campus được đồng bộ lại bằng công cụ setup hiện có: đủ manager, UI, 3 mục tiêu chụp ảnh, 9 món rác, 3 thùng phân loại và spawn theo khuôn viên hiện tại. Giữ bố cục IUH, phần đồ họa sân trung tâm và nhà xe đã khôi phục; không dựng lại kiến trúc.

Menu mới hiện khi vào scene, gồm ba thẻ chơi được: Grab Campus, Thợ săn khoảnh khắc và Green Campus. Nút đi nhanh KTX/thư viện tách riêng. Giữ phím 1/4/5 cho ba game, 2/3 cho đi nhanh, M/Esc đóng mở menu và U cho trang phục. Các tính năng game chưa triển khai không được trình bày như game đã chơi được.

Tăng tương phản, cỡ chữ, khoảng cách và vùng bấm; giảm nội dung thẻ; phóng to hướng dẫn Photo Hunter. Menu dùng Canvas ưu tiên riêng để không chìm dưới HUD của game khác. Chọn game đóng menu và đi qua màn hướng dẫn tương ứng, không tự xóa tiến trình.

Sửa luồng khởi tạo trang phục: component được khởi tạo khi vào scene nhưng không tự mở; nút Grab vẫn mở trang phục trước nhiệm vụ. Hai tùy chọn mở khi khởi động được cấu hình theo scene, giữ mặc định cũ cho scene khác.

Kiểm chứng Unity Editor:

- Luồng nút mở Photo/Green/Grab, chuyển game, đóng menu, trả điều khiển và đi thư viện: 10/10, xem `Evidence/SharedSceneMenu/menu-routing.txt`.
- Chạy trọn hai game: 28/28, Green 270 điểm, Photo 3 JPEG/300 điểm. Dùng thư mục save kiểm thử riêng.
- Tuyến ngoài: 41/41; hành lang nối: 33/33.
- Xem ảnh menu và hướng dẫn trong `Evidence/SharedSceneMenu`.

Scene đã lưu, kết thúc ở Edit Mode; mỗi game có một manager. Công cụ chụp ảnh composited của MCP phát sinh thông báo PlayerLoop gọi lặp trong `MCPForUnity/Runtime/Helpers/ScreenshotUtility.cs:197`; ghi nhận là lỗi công cụ chụp ảnh, không báo Console hoàn toàn sạch. Hai lượt kiểm tra game sau sửa khởi tạo đều đạt 28/28.

Chưa kiểm chứng thao tác cảm ứng hay bản build Android. Công cụ thực hiện lại: **IUH Campus → UI → Sync Shared Games and Rebuild Menu**. Bản sao scene trước sửa giữ cục bộ trong `Docs/Backups`.
