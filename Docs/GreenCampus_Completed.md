# Green Campus — bản gameplay đã kiểm chứng

## Chạy trong Unity

Mở scene `Assets/IUH_Campus/Scenes/IUH_Campus_Main.unity`, bấm Play, nhấn **M → game số 5 — Green Campus**.

- **WASD:** di chuyển.
- **F:** nhặt món rác gần nhất, hoặc bỏ món đang cầm vào thùng gần nhất.
- **1 / 2 / 3:** chọn thùng tái chế / hữu cơ / còn lại khi ở gần đúng thùng.
- **Q:** trả món đang cầm về vị trí ban đầu.
- **Tab:** mở/đóng con trỏ để thao tác các nút HUD. Đồng hồ tạm dừng khi dùng con trỏ hoặc mở menu.
- **M:** menu các mini game.

Mỗi lượt có **4 phút**, **9 món rác** tại **3 khu vực**. Chai nhựa bỏ vào tái chế, vỏ chuối vào hữu cơ, vỏ bánh vào thùng còn lại. Đúng món +20 điểm; dọn sạch một khu +30 điểm; mỗi lần chọn sai thùng phạt 5 điểm trong tổng điểm, không hiển thị điểm âm. Sai thùng vẫn giữ món đang cầm để sửa lại. Tối đa **270 điểm** khi không sai.

Hoàn thành sẽ hiện kết quả và phục hồi ba góc xanh. Hết giờ hiện màn thua và cho chơi lại. Có tiếp tục lượt đã lưu, lượt mới, điểm kỷ lục và chống cộng thưởng trùng. Khi hủy hoặc rời game, món chưa phân loại được trả về vị trí ban đầu.

## Phạm vi

Đây là **gameplay phân loại thủ công**, có ghi rõ trên màn giới thiệu, HUD và menu. Chưa tích hợp mô hình AI, camera nhận diện rác hoặc Adafruit IO; không dùng kết quả giả để thay thế inference/cloud thật. Phần AI và IO Cloud cần triển khai, kiểm chứng riêng khi đã có mô hình và cấu hình dịch vụ phù hợp.

Tiến trình bình thường lưu ở `Application.persistentDataPath/IUH_GreenCampus_Save.json` bằng file tạm và thay thế sau khi ghi thành công. Mất khả năng lưu sẽ không tiêu thụ món đang cầm hoặc thưởng điểm giả. Tiến trình thử nghiệm nằm riêng trong `Docs/Evidence/GreenCampus`, không thay thế save người dùng.

Điểm xanh, kỷ lục và số khu sạch được đưa vào WorldState qua `GREEN_CAMPUS_SCORE`, `GREEN_CAMPUS_BEST_SCORE`, `GREEN_CAMPUS_CLEAN_ZONES` để các tính năng sau có thể sử dụng. Chưa triển khai Campus Festival.

## Kết quả thực chạy ngày 21/10/2026

- **Green Campus: 32/32 assertions đạt** trong scene kiểm thử cô lập.
- **Photo Hunter: 35/35 assertions vẫn đạt** sau tích hợp.
- Play Mode: nhặt/phân loại đúng cả 9 món, mở đủ 3 góc xanh, kết thúc với 270 điểm.
- Kiểm tra nhặt quá xa bị từ chối; sai thùng bị phạt và vẫn giữ món để thử lại.
- Menu tạm dừng đồng hồ; màn kết quả không bị mở khóa nhân vật nhầm khi đóng menu M.
- Chuyển Green Campus ↔ Photo Hunter/Grab hoạt động; camera và controller được trả lại sau khi thoát.
- Phiên Play mới tải lại đúng 9/9 món, 3/3 góc xanh, 270 điểm và một lượt hoàn thành.
- Hết giờ chặn nhặt rác; chơi lại khôi phục 240 giây và giữ kỷ lục.
- Setup chạy hai lần không tạo trùng; cây đối tượng khuôn viên cũ giữ nguyên 34.991 Transform. Chỉ thêm đối tượng gameplay dưới `GAMEPLAY_GREEN_CAMPUS` và cập nhật thẻ game số 5.
- Console không có lỗi sau các lượt kiểm tra.

Kiểm tra được thực hiện trong Unity 6000.5.8f1 qua MCP bằng phương thức production, fixture và input trường hợp thử; chưa xác nhận một lượt chơi hoàn toàn thủ công hoặc bản Android.

Ảnh: `Evidence/GreenCampus/briefing.png`, `Evidence/GreenCampus/gameplay.png`, `Evidence/GreenCampus/victory.png`.

Kết quả chi tiết: `Evidence/GreenCampus/verification.json`.

Menu thiết lập lại: **IUH Campus → Minigames → Setup Green Campus**. Menu test: **IUH Campus → Tests → Run Green Campus Tests** (lưu scene và chạy ở Edit Mode).
