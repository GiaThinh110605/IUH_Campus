# Chỉnh các cụm nhà nối liền — 22/10/2026

Scene: `Assets/IUH_Campus/Scenes/IUH_Campus_Main.unity`.

Đã sửa đúng bốn điểm người dùng yêu cầu:

1. A, hội trường A7 và phòng y tế là một cụm gắn liền. Phòng y tế ở phần trước cánh A7, có biển tên và dấu thập đỏ. Loại bỏ khoảng hở dài giữa hai cánh.
2. H nối sát A và B, tạo dãy nhà liên tục. Hành lang tầng trệt xuyên từ A qua H đến B; nhánh ngang trong A nối thư viện với A7 và phòng y tế.
3. V nối sát X. Có hành lang ngang đi qua giữa hai tòa.
4. B là một khối chữ L với cánh sau thẳng, cùng cao độ mái, nối trực tiếp vào V. Hai mảnh B cũ với phần góc giật cấp đã được thay thế.

Các mối nối dùng hình học có khoảng rỗng và collider chừa lối thực sự. Hành lang rộng 4 m, cao thông thủy 3,5 m, sàn cùng cao độ sân. Có lối xuyên qua X để đường từ hai cổng sau vào sân vẫn thông. Đây là phần kết nối tầng trệt; nội thất và lưu thông các tầng trên chưa triển khai ở lượt này.

## Phạm vi và phục hồi

- Các khối cũ A, A7, A_Link, H, B, B_Wing, V, X được giữ dưới `IUH_REFERENCE_CAMPUS/ARCHIVE_BEFORE_CONNECTED_BLOCKS` ở trạng thái tắt.
- Mesh nối mới dùng thư mục riêng `Assets/IUH_Campus/ReferenceLayout/Connections_R1/Meshes`, bảo toàn mesh của bản trước.
- Bản sao scene cục bộ trước chỉnh: `Docs/Backups/IUH_Campus_Main_before_connections_20261022.unity` (giữ tại máy, không đưa vào Git).
- Photo Hunter giữ nguyên ID, tọa độ mục tiêu và góc chụp nhà A, chỉ gắn lại tham chiếu vào tòa A mới. Giữ các bố cục còn lại của khuôn viên; trạm nhận V được đặt sang cạnh lối vào.
- Không đổi luật chơi, điểm số hoặc định dạng save của mini game.

## Kiểm chứng

- Tuyến cổng, spawn, rác và landmark: **41/41**; `Evidence/CampusConnections/exterior-audit.json`.
- Khoảng trống và mặt sàn của các hành lang mới: **33/33**; `connected-halls-audit.json`.
- Đi qua A–H–B, y tế–A7–A, B–V–X và lối xuyên X bằng CharacterController thật trong Play Mode: **16/16**; `connected-halls-walk-playmode.json`. Bài kiểm tra di chuyển qua bộ giải va chạm theo từng frame và khôi phục nhân vật sau khi xong.
- Chạy trọn Green Campus và Photo Hunter trong scene đã chỉnh: **28/28**, Green đủ 9 món/270 điểm, Photo đủ 3 JPEG/300 điểm. Dùng save kiểm thử riêng; kết quả lưu ở `Evidence/CampusConnections/both-games-playmode.json`.
- Đã kiểm tra ảnh mái và hành lang tầm người; không có lỗi compile tại lúc kiểm chứng.

## Ảnh mới

- [Tổng thể](Evidence/CampusConnections/overview.png).
- [Cụm A–A7–y tế](Evidence/CampusConnections/a-a7-clinic.png).
- [B chữ L nối V–X](Evidence/CampusConnections/b-v-x.png).
- [Hành lang A–H](Evidence/CampusConnections/a-h-hall.png).
- [Hành lang B–V–X](Evidence/CampusConnections/b-v-x-hall.png).

Menu dưới **IUH Campus → Reference Layout**: **Verify Connected Halls**, **Walk Connected Halls (Play Mode)** và **Capture Connected Blocks**. Menu **Connect A-H-B and B-V-X** áp dụng lần đầu và chặn chạy trùng để bảo toàn các sửa đổi tiếp theo.
