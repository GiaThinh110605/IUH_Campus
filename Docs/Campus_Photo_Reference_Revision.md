# Khu vực E – sân giữ xe và số tầng theo ảnh thực tế

Ngày 22/10/2026. Scene: `Assets/IUH_Campus/Scenes/IUH_Campus_Main.unity`.

## Số tầng đã xác nhận

| Nhà | Số tầng | Chiều cao khối dựng ước lượng |
|---|---:|---:|
| A | 4 | 15 m |
| B | 4 | 15 m |
| A7 | 7 | 26,25 m |
| H | 11 | 41,25 m |

Số tầng lấy theo người dùng; chiều cao mét là tỷ lệ dựng mô hình, chưa phải kích thước khảo sát. Mặt tiền H có dải kính xanh và hàng tầng rõ; các phần cao hơn nhà liền kề vẫn có mặt đứng, không bị bỏ trống khi đổi chiều cao.

## Khoảng sân và ngoại thất

- Dùng builder chuyên biệt nhà E có sẵn trong dự án để dựng kính, lam đứng, khung ngang, sảnh nhìn qua kính; bổ sung bảng chữ IUH, mái đón elip rộng, bồn hoa và bồn chữ IUH lệch về bên trái sảnh theo ảnh.
- Thay bãi xe hai sàn cao bằng bục giữ xe thấp. Mép sàn trên cao khoảng 2,18 m so với sân, có lan can xanh, trụ/ô thoáng mặt dưới, cụm thông gió giữa và hàng ghế phía sân.
- Hai ram ở hai đầu bãi xe có mặt dốc và chiếu nghỉ nối lên sàn thực sự; đường vào sân đi vòng chân dốc.
- Mặt sân màu xám, hai cây bóng mát tán rộng từ mesh cây đã có trong dự án, bồn thấp và ghế. Collider cây chỉ bao thân, không bao cả tán.
- Dời đúng một món rác ra ngoài bồn cây, giữ ID, spawn và các quy tắc của Green Campus. Photo Hunter giữ ID và góc chụp; tham chiếu sân trung tâm được gắn vào cụm E mới.
- Giữ hành lang nối A–H–B, A–A7–y tế và B–V–X ở tầng trệt.

## Bảo toàn

Các nhóm trước lượt này nằm dưới `IUH_REFERENCE_CAMPUS/ARCHIVE_BEFORE_PHOTO_REFINEMENT`, ở trạng thái tắt. Mesh mới lưu trong `Assets/IUH_Campus/ReferenceLayout/PhotoReference_R2/Meshes`. Bản sao scene cục bộ là `Docs/Backups/IUH_Campus_Main_before_photo_refinement_20261022.unity`; bản sao này không đưa vào Git.

Ảnh gốc được giữ trong `Docs/References/Forecourt_Parking.png`, `Building_E_Front.png`, `Forecourt_Side.png`, `Campus_Aerial.png`.

## Kiểm chứng và ảnh

Kết quả lượt cuối: hình khối/số tầng **13/13**, tuyến cổng và landmark **41/41**, khoảng trống hành lang **33/33**, hai ram dốc **6/6**, trọn lượt mini game **28/28**, bàn phím/chuột **24/24** (5.122 frame). Console không có lỗi ở cuối lượt.

- Số tầng, chiều cao, bãi xe và cây: `Evidence/PhotoReference/photo-reference-audit.json`.
- Các tuyến từ năm cổng, spawn và landmark: `exterior-audit.json`.
- Khoảng trống hành lang nối nhà: `connected-halls-audit.json`.
- Hai ram dốc: `parking-ramp-physics.json`. Bài kiểm tra dùng CharacterController của nhân vật thực hiện 580 bước giải va chạm, lên dốc rồi đi qua chiếu nghỉ sang sàn bãi xe.
- Chạy trọn hai mini game với save thử riêng: `both-games-playmode.json`.
- Kiểm tra bàn phím/chuột và chuyển động trong scene: `movement-playmode.json`.

Ảnh mới: [mặt tiền E](Evidence/PhotoReference/e-front.png), [nhìn về bãi xe](Evidence/PhotoReference/parking-from-e.png), [chiều cao H/A7](Evidence/PhotoReference/h-a7-heights.png), [tổng thể](Evidence/PhotoReference/overview.png).

Các tầng trên được thể hiện ngoại thất; nội thất và thang lên các tầng này chưa được dựng trong lượt chỉnh này.
