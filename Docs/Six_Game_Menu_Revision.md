# Menu sáu game — 24/10/2026

Menu trong scene chính có đủ sáu vị trí theo kế hoạch dự án:

| Vị trí | Game | Trạng thái menu |
|---|---|---|
| 1 | Grab Campus | Có thể chơi, mở chọn trang phục |
| 2 | Trốn tìm với ma | Đang phát triển, khóa nút chơi |
| 3 | AI Object Hunt | Đang phát triển, khóa nút chơi |
| 4 | Thợ săn khoảnh khắc | Có thể chơi, mở hướng dẫn Photo Hunter |
| 5 | Green Campus | Có thể chơi, mở hướng dẫn Green Campus |
| 6 | Campus Festival | Đang phát triển, khóa nút chơi |

Trạng thái có thể chơi không đồng nghĩa toàn bộ game đã hoàn thiện. Không gán tên người phụ trách khi chưa có danh sách xác nhận.

Thiết kế mới dùng nền sáng, thanh bên tối, sáu thẻ hai hàng, góc bo và bóng nhẹ, màu riêng cùng hình minh họa bằng thành phần UI. Các sprite mask nhỏ nằm tại `Assets/IUH_Campus/UI/MenuDesign`. Không thêm thư viện hay tài nguyên tải ngoài.

Phím 1/4/5 mở game tương ứng. Phím 2/3/6 không khởi động hoặc dịch chuyển từ các ô chưa hoàn thiện; đi nhanh KTX/thư viện dùng nút riêng ở thanh bên. M/Esc đóng menu, U mở trang phục. Menu hiện khi vào scene.

Kiểm chứng 13/13 điều kiện trong Play Mode: đủ sáu thẻ, mở lúc khởi động, ba nút chưa hoàn thiện bị khóa, ba phím tương ứng giữ nguyên menu/vị trí, ba game mở đúng màn, Green đóng Photo và khám phá tự do trả lại điều khiển. Báo cáo: `Evidence/SixGameMenu/routing.txt`. Đã xem ảnh thật ở 1920×1080; chưa kiểm chứng cảm ứng/Android.

Ảnh cuối: `Evidence/SixGameMenu/menu-final.png`. Chụp qua ScreenCapture vào frame kế tiếp để tránh lỗi PlayerLoop của công cụ chụp composited MCP đã ghi nhận trước đó.

Công cụ dựng: **IUH Campus → UI → Sync Shared Games and Rebuild Menu**. Phần thiết kế nằm ở `Assets/IUH_Campus/Editor/IUHSixGameMenuDesign.cs`. Bố cục khuôn viên và gameplay được giữ nguyên trong lượt sửa menu này.
