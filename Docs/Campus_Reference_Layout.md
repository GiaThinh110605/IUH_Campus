# Khuôn viên theo ảnh tham chiếu — 22/10/2026

Đã triển khai trực tiếp trong `Assets/IUH_Campus/Scenes/IUH_Campus_Main.unity`, Unity 6000.5.8f1. Đây là bản diễn giải ngoại thất theo sơ đồ minh họa người dùng cung cấp, không phải bản đo kiến trúc. Ưu tiên vị trí tương đối, hình khối và đường đi; dùng tông kem, xanh và mái xám riêng của dự án.

**Bản chỉnh nối khối tiếp theo:** A–A7–phòng y tế, A–H–B và B–V–X đã liền nhau, có hành lang tầng trệt đi xuyên; B được sửa thành chữ L. Xem [Campus_Connections_Revision.md](Campus_Connections_Revision.md) và ảnh mới trong `Evidence/CampusConnections`. Phần dưới ghi lại lượt dựng bố cục đầu tiên.

## Bố cục đã dựng

- Mặt trước giáp Nguyễn Văn Bảo: A và hội trường A7 bên trái; khối bãi xe lớn giữa; G bên phải. Có Cổng 1, 2, 3.
- Sau A là H, sau H là B với cánh gấp. E nằm giữa sân, D bên phải E; F nằm trên cụm C/I/G.
- C là khối hẹp giữa bãi xe và I; căn tin–gym và hành lang có mái nằm cạnh I.
- Hàng sau gồm V, X, T dài và K góc phải; Cổng 4, 5 giáp Nguyễn Văn Nghi. Hai cạnh còn lại là Nguyễn Thái Sơn và Lê Lợi.
- Mặt tiền có cửa sổ, lam che nắng, mái đón, biển tên; mái có gờ bao và hộp kỹ thuật. Bổ sung sân lát, bồn cây, ghế, đèn, đài phun nước, hàng rào và vạch qua đường.
- Các công trình dùng thể tích va chạm đúng kích thước và ngoại thất đơn giản; chưa dựng nội thất trong các khối mới.

## Bảo toàn và tích hợp

- Giữ nguyên mô hình nhập `campus_P0_progress` trong scene, chỉ chuyển sang trạng thái tắt. Không xóa mesh hoặc nội thất cũ. Hai collider cầu thang F nằm rời ở cấp gốc cũng được tắt cùng mô hình cũ.
- Bản sao toàn bộ scene cục bộ trước thay đổi, bao gồm sửa đổi đang có trong Editor: `Docs/Backups/IUH_Campus_Main_before_layout_20261021.unity` (giữ tại máy, không đưa vào Git). Không reset hay clean checkout.
- Ngoại thất mới nằm dưới `IUH_REFERENCE_CAMPUS`; từng khối có nhóm riêng `Block_A`, `Block_E`, v.v. Chi tiết được gộp thành mesh theo vật liệu, lưu bền vững trong `Assets/IUH_Campus/ReferenceLayout`.
- Bốn collider rỗng `Block_D_StairMidRail_Left_Fl1..Fl4` vẫn tắt; không bật lại.
- Chuyển spawn và xe về lối Cổng 1; Green Campus về sân trước E; gắn lại ba landmark Photo Hunter vào cấu trúc mới. Giữ ID mục tiêu, ID rác, tham chiếu UI và định dạng save.
- Chuyển cửa hàng, trạm nhận, waypoint và vị trí đơn Grab theo khuôn viên mới. Kiểm tra chuyển game và camera; chưa chạy trọn ba đơn giao hàng.
- Các tiện ích setup Photo/Green và tọa độ Grab nhận biết bố cục mới để không khôi phục nhầm tọa độ cũ.
- Biển chữ mới tuân theo độ sâu của cảnh. Nhãn Green được thu nhỏ cho sân mới. Shader có tham chiếu lưu trong scene.

## Kiểm chứng đã chạy

| Kiểm tra | Kết quả | Minh chứng |
|---|---:|---|
| Logic Green Campus | 32/32 | `Evidence/CampusLayout/logic-regression.json` |
| Logic Photo Hunter | 35/35 | cùng file phía trên |
| Khối nhà, collider, spawn, 5 đường nối cổng vào sân | 41/41 | `Evidence/CampusLayout/exterior-audit.json` |
| Bàn phím/chuột, WASD, nhảy, phân loại, chụp ảnh và chuyển game trong scene thật | 24/24 | `Evidence/CampusLayout/movement-playmode.json` |
| Trọn lượt Green + cả 3 ảnh mới trong Play Mode | 28/28 | `Evidence/CampusLayout/full-round-playmode.json` |

Kiểm tra chuyển động lấy mẫu 4.923 frame, độ cao nhân vật khoảng 0,48–1,67 m, bao gồm lúc nhảy. Bài kiểm tra đưa sự kiện qua Unity Input System, PlayerInput và CharacterController thật. Phần chuẩn bị test nay kích hoạt lại thiết bị nếu Unity đã tắt bàn phím/chuột khi mất focus, và khôi phục trạng thái khi xong.

Kiểm tra trọn lượt gọi phương thức gameplay thật và dịch chuyển giữa các điểm kiểm thử: Green đủ 9/9 món, 3/3 góc xanh, 270 điểm; Photo lưu 3 JPEG thật, 300 điểm. Dùng thư mục save kiểm thử riêng dưới `Evidence/CampusLayout/FullRound`, không thay save người chơi. Đây là kiểm chứng tự động desktop trong Editor, chưa xác nhận Android hoặc một lượt chơi hoàn toàn bằng tay.

Đã rà ảnh tổng thể, mặt trước và tầm người. Không ghi nhận lỗi compile hay lỗi Console cuối lượt. Các cảnh báo API Unity cũ có sẵn chưa được xử lý trong phạm vi này.

## Mở xem và chạy lại

Mở scene chính, chọn nhóm `IUH_REFERENCE_CAMPUS` để xem bố cục. Bấm Play để khám phá, dùng M để vào mini game như trước.

Các menu mới dưới **IUH Campus → Reference Layout**:

- **Verify Exterior Routes:** quét capsule liên tục trên các tuyến vào từ 5 cổng, kiểm tra spawn và vị trí tương tác.
- **Verify Both Games (Play Mode):** chạy trọn lượt hai game trong scene mới với save kiểm thử riêng.
- **Capture Exterior Views:** chụp bốn góc ngoại thất ở Edit Mode.
- **Build Illustrated Exterior:** dựng lần đầu; cố ý chặn nếu nhóm mới đã tồn tại để tránh tạo trùng hoặc ghi đè các chỉnh sửa sau đó.

Menu kiểm tra bàn phím/chuột vẫn là **IUH Campus → Tests → Run Movement Regression (Play Mode)**.

Ảnh bàn giao: `Evidence/CampusLayout/overview.png`, `walkway.png`, `plaza.png`, `front-gates.png`, `green-campus.png`, `green-victory.png`, `photo-complete.png`.
