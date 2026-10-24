# Khuôn viên chung theo ảnh minh họa — 24/10/2026

Scene đã được nâng vật liệu, hình học, bầu trời và chất lượng hiển thị ở lượt sau. Xem [chi tiết bản nâng độ nét](Shared_Campus_Visual_Polish.md) và ảnh mới trong `Evidence/SharedCampusPolish`.

Mở `Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity`. Scene này dựng riêng theo bố cục ảnh đích: cổng ở tiền cảnh, trục sân lát đá đi tới đài phun nước/tác phẩm điêu khắc, khối nhà trung tâm và hai cánh bao sân; căn tin phía trái, lễ hội phía phải, thư viện và ký túc xá nằm ở hai cánh.

## Thành phần đã dựng

- Mặt đứng nhiều tầng với khung cửa, kính, gờ tầng, mái đua và sảnh cột; mái che sọc đỏ/trắng, bàn ghế căn tin.
- Sân lát đá có dải nhấn, các bồn cây thẳng/cong, ghế và đèn; cổng có trụ đá, bảng tên và hàng rào.
- Hồ phun nước có vành đá, tác phẩm trừu tượng và hình dòng phun; shader tạo chuyển động gợn nước. Dòng phun là chi tiết hình học trang trí, không phải mô phỏng chất lỏng.
- Sân khấu, màn LED, lều và bảng cờ; các hình sinh viên trang trí đứng cố định.
- 18 cây dùng mesh lá và ba mức LOD; collider thân không bao tán. Bầu trời có mây, nắng chiều, bóng mềm và chỉnh màu ACES.
- Nhân vật/camera dùng Starter Assets; vật liệu và phụ kiện đầu/balo đơn giản cho cảnh thử. Đây chưa phải mẫu nhân vật hoàn thiện.

## Xem và đi thử

- Play: dùng WASD/chuột, Space nhảy; Shift theo bộ điều khiển Starter Assets.
- V: chuyển giữa góc nhìn nhân vật và góc tổng thể từ cổng.
- Esc: bật/tắt chuột và khóa di chuyển khi thao tác.
- Nếu nhân vật rơi ra ngoài nền, bộ xem thử đưa về cổng.

## Kiểm chứng

- Bốn tuyến bằng CharacterController: qua cổng, vòng đài phun nước đến sảnh, đến căn tin và lên sân khấu đều đạt; va chạm chặn đi xuyên hồ đạt (5/5).
- Phím V chuyển được hai chiều; đầu vào di chuyển làm nhân vật đi trên nền và dừng ở hồ.
- Scene không mất script, không có mesh rỗng; Console không có lỗi ở cuối lượt. Có 18 nhóm cây LOD.
- Đã xem ảnh toàn cảnh và góc đi bộ sau khi render thật trong Unity. Tài liệu kiểm tra tại `Docs/Evidence/SharedCampusConcept`.

Ảnh: `game-overview.png` (trong Play Mode), `campus-final.png` (camera bố cục). Scene chính và các minigame trước đó giữ nguyên trong lượt dựng riêng này.

Đây là cảnh 3D dựng theo bố cục và màu của ảnh tham chiếu bằng tài nguyên tạo trong dự án, chưa đạt mức chi tiết tả thực 1:1 của ảnh. Chưa dựng nội thất, hành vi NPC, nhiệm vụ, HUD sáu game hay nối tiến trình vào scene mới; ưu tiên của lượt này là khuôn viên chung. Chưa đo hiệu năng bản build Android.

Công cụ: **IUH Campus → Concept → Build Shared Campus from Reference** (chặn ghi đè scene đã có). Mesh/material/texture/shader lưu tại `Assets/IUH_Campus/SharedCampusConcept`. Scene được lưu và đang mở để xem trực tiếp.
