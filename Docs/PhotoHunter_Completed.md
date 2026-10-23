# Photo Hunter — bản sửa đã kiểm chứng

Kiểm tra ngày 21/10/2026 trên Unity 6000.5.8f1 qua MCP.

## Cách chơi

1. Mở `Assets/IUH_Campus/Scenes/IUH_Campus_Main.unity` và bấm Play.
2. Nhấn **M**, chọn **game số 4 — Thợ săn khoảnh khắc**.
3. Chọn **Bắt đầu săn ảnh** hoặc **Đến Cổng 1**.
4. Di chuyển trong trường; nhấn **C/P** để mở chế độ chụp. Rê chuột để ngắm, nhấn **Space/Enter/chuột trái** để chụp; **Q/Esc** để thoát ngắm.
5. Chụp đủ Cổng 1, sân trung tâm và mặt tiền nhà A để đạt 300 điểm, mở album.

## Đã sửa

- Lưu thất bại trả đúng 0 điểm, không ghi nhận hoàn thành giả. Ghi save qua file tạm rồi thay thế để giữ bản save cũ khi lỗi.
- Test chạy trong scene riêng và khôi phục scene trước đó; khởi tạo đúng các manager trong Edit Mode, giải phóng texture không gây lỗi Editor.
- Đóng bảng trang phục khi mở Photo Hunter; phối hợp menu M, album, cursor và điều khiển nhân vật.
- Dùng lens của Cinemachine camera đang hoạt động. Chế độ chụp đưa camera về mắt nhân vật và ẩn avatar; thoát ngắm khôi phục rig, renderer và tốc độ.
- Đổi điểm dịch chuyển Cổng 1 sang vị trí đã kiểm tra không bị nhà A che khuất; căn hướng nhìn và thông báo dịch chuyển cho camera.
- Lưu setup vào scene chính; chạy setup lại không tạo trùng. Không di chuyển hoặc đổi kích thước công trình.

## Kết quả thực chạy

- **35/35 assertions đạt** trong Unity Editor.
- Play Mode chụp **3 JPEG thật**, cộng **300 điểm**, album nạp **3 texture**.
- Chụp lặp không thưởng trùng; chuyển Photo Hunter ↔ Grab được kiểm tra.
- Mở/đóng menu ở màn kết quả giữ đúng khóa điều khiển và con trỏ.
- Đóng album giải phóng texture và trả lại controller/avatar.
- Sang phiên Play mới, tải lại save kiểm thử vẫn đúng **300 điểm, 3/3 và 3 ảnh**.
- Console không có lỗi ở cuối lượt kiểm tra.
- Setup chạy hai lần giữ nguyên số đối tượng. So với scene trước setup, **337 Transform cũ giữ nguyên pose**; cây đối tượng campus giữ nguyên **34.991 Transform**.

Ảnh minh chứng: `Evidence/PhotoHunter/briefing.png`, `Evidence/PhotoHunter/album-complete.png`.

Kết quả máy đọc được: `Evidence/PhotoHunter/verification.json`, `Evidence/PhotoHunter/scene-layout-check.json`.

Ảnh và save demo được đặt riêng dưới `Docs/Evidence/PhotoHunter/PlayMode`, không thay thế save chơi bình thường của người dùng. Game bình thường vẫn lưu ở Application.persistentDataPath.

Phạm vi đã kiểm tra là desktop Unity Editor với thao tác tự động qua phương thức gameplay và input thực. Chưa kiểm tra bản Android, cảm ứng hoặc một phiên chơi toàn bộ bằng bàn phím/chuột thủ công.
