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
