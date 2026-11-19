# Cây cỏ stylized — lượt xem trước

20/11/2026 · Unity 6000.5.8f1, Windows Editor, URP. Mốc scene gốc `f02a8c3`. Tọa độ và thông số camera lưu trong [cameras.json](cameras.json).

## 18 cây bóng mát

Cây thay thế giữ parent, vị trí, góc xoay và kích thước capsule thân của cây cũ. Năm mẫu CommonTree xoay vòng, sai lệch chiều cao trong ±10% theo seed cố định. Tán mới hẹp hơn tán cũ.

| Object | Tâm thế giới (x, y, z) | Mẫu CommonTree |
|---|---|---:|
| Shade tree_12 | (-23, 0.32, -34) | 1 |
| Shade tree_13 | (-23, 0.32, -4) | 2 |
| Shade tree_14 | (-23, 0.32, 29) | 3 |
| Shade tree_15 | (-23, 0.32, 52) | 4 |
| Shade tree_16 | (31, 0.32, -34) | 5 |
| Shade tree_17 | (23, 0.32, -4) | 1 |
| Shade tree_18 | (23, 0.32, 29) | 2 |
| Shade tree_19 | (23, 0.32, 52) | 3 |
| Shade tree_20 | (-59, 0, -42) | 4 |
| Shade tree_21 | (-59, 0, -18) | 5 |
| Shade tree_22 | (-59, 0, 10) | 1 |
| Shade tree_23 | (-59, 0, 40) | 2 |
| Shade tree_24 | (32, 0, -42) | 3 |
| Shade tree_25 | (59, 0, -18) | 4 |
| Shade tree_26 | (59, 0, 10) | 5 |
| Shade tree_27 | (59, 0, 40) | 1 |
| Shade tree_28 | (-20, 0, -48) | 2 |
| Shade tree_29 | (32, 0, -47) | 3 |

| Góc | Ngày trước → sau | Đêm trước → sau |
|---|---|---|
| Sảnh | [Trước](lobby-before-day.png) · [Sau](lobby-after-day.png) | [Trước](lobby-before-night.png) · [Sau](lobby-after-night.png) |
| Căn tin | [Trước](canteen-before-day.png) · [Sau](canteen-after-day.png) | [Trước](canteen-before-night.png) · [Sau](canteen-after-night.png) |
| Đài phun nước | [Trước](fountain-before-day.png) · [Sau](fountain-after-day.png) | [Trước](fountain-before-night.png) · [Sau](fountain-after-night.png) |

## Kiểm chứng cây

- EditMode 123/123, PlayMode 47/47, Console 0 error, Missing Script 0 trong scene campus. Ảnh render URP không thấy vật liệu hồng.
- Trong Play qua MCP, CharacterController di chuyển đủ 3,60 m theo đường sát thân cây. Mở được luống 3. Dùng cờ Tương tác chung với đường E để xác nhận 8 m không vào trận, 4 m vào trận. Đã khôi phục tiến độ cốt truyện.
- Đo 20 giây tại góc sảnh, camera cố định và ngày, dùng UnityStats cho draw calls và tris.

| Mốc | FPS | Draw calls | Tris |
|---|---:|---:|---:|
| Gốc | 214,46 | 278,5 | 6.645.352 |
| Thay 18 cây | 234,47 | 260,0 | 5.798.841 |

`Leaves_TwistedTree.png` được `M_Nature_Bush` tham chiếu và dùng ở prefab bụi thường.

## Điều chỉnh vị trí cây cạnh vườn

`Shade tree_12` chuyển từ `(-23, 0.32, -34)` sang `(-23, 0.32, -29)`, giữa bãi cỏ Căn tin. Khoảng cách ngang từ mép capsule thân tới mép các luống 1–4 lần lượt là 3,761 m, 4,765 m, 3,779 m và 4,779 m. Kiểm tra vật lý tại điểm đặt không thấy collider cản thân cây. Quái gần nhất là quái Căn tin, cách tâm cây khoảng 14,04 m.

## Khoảnh thử và vườn

Khoảnh tâm `(20, -29)` có 48 cụm bụi, hoa và cỏ dưới `NATURE_PLANTERS`, không gắn collider. Hai dải dùng năm bản sao mesh đã có trong `Meshes/Garden/`. Bộ lọc bỏ 180.312 tam giác nằm trọn trong hai dải, với 0 tam giác cắt qua biên lọc. Thứ tự chỉ số tam giác giữ lại được đối chiếu sau khi ghi mesh.

Ba ô vườn dùng cỏ ba lá cho Hạt, cây lá và dương xỉ cho Mầm, hai mẫu cụm hoa cho Nở. Bốn luống dùng chung tổ hợp hoa. Shader tính sương theo độ sâu camera trực giao để lá và hoa giữ màu ở ảnh nhìn từ trên.

| Ảnh | Ngày trước → sau | Đêm trước → sau |
|---|---|---|
| Khoảnh thử | [Trước](pilot-before-day.png) · [Sau](pilot-after-day.png) | [Trước](pilot-before-night.png) · [Sau](pilot-after-night.png) |
| Vườn · Hạt | [Trước](garden-seed-before-day.png) · [Sau](garden-seed-after-day.png) | [Trước](garden-seed-before-night.png) · [Sau](garden-seed-after-night.png) |
| Vườn · Mầm | [Trước](garden-sprout-before-day.png) · [Sau](garden-sprout-after-day.png) | [Trước](garden-sprout-before-night.png) · [Sau](garden-sprout-after-night.png) |
| Vườn · Nở | [Trước](garden-bloom-before-day.png) · [Sau](garden-bloom-after-day.png) | [Trước](garden-bloom-before-night.png) · [Sau](garden-bloom-after-night.png) |

[Cây sau khi dời, ngày](canteen-task5-day.png) · [Đêm](canteen-task5-night.png). [Góc đài phun nước không bị khinh khí cầu che, ngày](fountain-task5-day.png) · [Đêm](fountain-task5-night.png). Góc mới của đài phun nước dùng tọa độ `fountainTask5` trong `cameras.json`.

Chạy riêng EditMode 123/123 (`39805ec8fde5429e9b47ce4ffcda7ce6`) và PlayMode 47/47 (`eebcbc08344f4ec4a38647d13e556fd6`).

Console 0 lỗi, Missing Script = 0. Trong Play, CharacterController đi đủ 4 m qua cạnh cây và 8 m qua khoảnh thử. Mở được luống 3. Cờ Tương tác qua cùng đường xử lý với E không vào trận ở 8 m và vào trận ở 4 m. Tiến độ cốt truyện đã được khôi phục.

Task 5 tại cùng góc sảnh trong 20 giây đạt 204,56 FPS, 368,0 draw calls và 4.994.796 tris. FPS giảm 12,8% so với Task 4 và giảm 4,6% so với mốc gốc. Draw calls tăng từ 260,0 ở Task 4 lên 368,0.
