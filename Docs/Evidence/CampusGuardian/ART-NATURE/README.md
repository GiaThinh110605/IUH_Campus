# Cây cỏ stylized và ánh sáng campus

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

## Chỉnh sửa sau duyệt Task 5

- Khoảnh thử có 30 bụi, 16 cụm hoa chen giữa bụi và 8 cụm cỏ/ba lá. Seed `20112026` xác định độ lệch X/Z tối đa 0,3 m, góc xoay và hệ số scale 0,8–1,2. Hình học được gom theo vật liệu thành năm mesh dưới `NATURE_PLANTERS`, với cờ Batching Static.
- Đo lại cùng camera sảnh trong 20 giây. Kết quả 223,68 FPS, 277,0 draw calls và 5.024.969 tris. So với Task 4, tăng 17 draw calls, đạt giới hạn tăng tối đa 20.
- `Stage_Bloom` dùng chín cụm `Flower_3_Group` mỗi luống, scale 0,435 (gấp 1,5 lần mẫu cũ 0,29). Luống 1–4 có cánh đỏ, cam, vàng, trắng. Vật liệu cánh dùng atlas dẫn xuất giữ alpha; phần lá dùng atlas lá.

[Khoảnh thử sau chỉnh sửa, ban ngày](pilot-refined-day.png) · [Bốn luống giai đoạn Nở, ban ngày](garden-bloom-colored-day.png).

EditMode Vườn và Nature 19/19 (`1f633016582142be986dbbea59e11c37`). PlayMode Vườn 5/5 (`5394c57a45e04958af00cde76442cac9`).

## Task 6 — Bốn khoảnh còn lại

Bốn khoảnh có tâm `(−20, −1)`, `(20, −1)`, `(−20, 29)` và `(20, 29)`, với seed 20112027–20112030 tương ứng. Mỗi cụm có độ lệch X/Z tối đa 0,3 m, góc xoay ngẫu nhiên và hệ số scale 0,8–1,2 như khoảnh thử. Đặt 204 cụm và bỏ qua 12 điểm đặt để chừa khoảng cách với quái, check-in hoặc thân cây. Hai dải vùng gần và xa gom theo vật liệu thành 10 mesh mới. Cả `NATURE_PLANTERS` có 15 renderer và 0 collider.

Năm bản sao mesh cũ bỏ tổng cộng 721.248 tam giác ở tám dải. Không có tam giác cắt ngang biên lọc. Các tam giác giữ lại đã được đối chiếu thứ tự chỉ số sau khi ghi.

Đo 20 giây cùng góc sảnh đạt **231,23 FPS, 290 draw calls, 1.781.881 tris**. So với Task 4, FPS giảm 1,4% và tăng 30 draw calls. Đạt cả hai ngưỡng. Missing Script = 0. Trong Play đã di chuyển 8 m qua khoảnh gần phía đông, mở luống 3, xác nhận Tương tác ở 8 m không vào trận và 4 m vào trận. Tiến độ cốt truyện đã được khôi phục.

| Khoảnh | Ngày trước → sau | Đêm trước → sau |
|---|---|---|
| Tây gần | [Trước](plots-west-near-before-day.png) · [Sau](plots-west-near-after-day.png) | [Trước](plots-west-near-before-night.png) · [Sau](plots-west-near-after-night.png) |
| Đông gần | [Trước](plots-east-near-before-day.png) · [Sau](plots-east-near-after-day.png) | [Trước](plots-east-near-before-night.png) · [Sau](plots-east-near-after-night.png) |
| Tây xa | [Trước](plots-west-far-before-day.png) · [Sau](plots-west-far-after-day.png) | [Trước](plots-west-far-before-night.png) · [Sau](plots-west-far-after-night.png) |
| Đông xa | [Trước](plots-east-far-before-day.png) · [Sau](plots-east-far-after-day.png) | [Trước](plots-east-far-before-night.png) · [Sau](plots-east-far-after-night.png) |

EditMode 123/123 (`c049c1882ced43a1982c5a83883fcf9e`), PlayMode 47/47 (`a76329dd5bc349d688bc8a16de5305c7`). Chạy riêng từng bộ.

## Task 7 — Ánh sáng và hậu kỳ

Scene dùng profile `Campus volume` trên `Campus color grading`, camera chính bật HDR và hậu kỳ. Giữ đèn `Warm afternoon sun` ở 43°, tông ấm và bóng mềm. Profile giữ ACES, tăng exposure +0,26 EV, contrast +10, saturation +10, temperature +4. Nâng trung độ bằng gamma 0,19 và lift 0,0025. Bloom có threshold 1,2, intensity 0,18, scatter 0,6 và tối đa bốn vòng. PC giảm tầm bóng 160 → 110 m. Cấu hình URP iOS giữ nguyên.

| Góc | Ngày trước → sau | Đêm trước → sau |
|---|---|---|
| Sảnh | [Trước](lighting-lobby-before-day.png) · [Sau](lighting-lobby-after-day.png) | [Trước](lighting-lobby-before-night.png) · [Sau](lighting-lobby-after-night.png) |
| Đài phun nước | [Trước](lighting-fountain-before-day.png) · [Sau](lighting-fountain-after-day.png) | [Trước](lighting-fountain-before-night.png) · [Sau](lighting-fountain-after-night.png) |
| Bụi và lối đi | [Trước](lighting-planters-before-day.png) · [Sau](lighting-planters-after-day.png) | [Trước](lighting-planters-before-night.png) · [Sau](lighting-planters-after-night.png) |
| Vườn đang nở | [Trước](lighting-garden-before-day.png) · [Sau](lighting-garden-after-day.png) | [Trước](lighting-garden-before-night.png) · [Sau](lighting-garden-after-night.png) |

Trong Play, dùng ánh sáng sau biến cố, đã đi 8 m và mở được luống 3. Ảnh cho thấy đường lát, viền luống, bụi và hoa còn phân biệt trong bóng tối. EditMode 123/123 (`f8606d32ec824b989505cb8bf4e2010f`), PlayMode 47/47 (`fc5f7908b78b43639624bd7fb0d753c0`), Console 0 lỗi. Tiến độ truyện đã được khôi phục.

### Vật liệu bóng trên bề mặt lớn

Quét MeshRenderer đang hoạt động trong campus, lấy hai chiều lớn nhất của AABB thế giới nhân nhau ≥25 m² làm bộ lọc bề mặt lớn. Đọc `_Smoothness` hoặc `_Glossiness` lớn hơn 0,5. Danh sách dùng để rà soát, không có sửa vật liệu trong mục này.

| Vật liệu | Giá trị | Object ví dụ |
|---|---:|---|
| `M_CyberRifle_Neon` | 0,900 | `Balloon_NeonRing` |
| `Antique brass` | 0,550 | `Antique brass` |
| `Blue green glazing` | 0,860 | `Blue green glazing` |
| `Sky blue window reflections` | 0,880 | `Sky blue window reflections` |
| `Warm window reflections` | 0,840 | `Warm window reflections` |

## Hiệu năng qua các mốc

Windows Unity Editor, cùng góc `lobby`, ban ngày, khoảng 20 giây/mốc. Draw calls và tris lấy từ UnityStats. Các số này dành cho một góc Editor, chưa đo build iOS.

| Mốc | FPS | Draw calls | Tris |
|---|---:|---:|---:|
| Task 1–3, trước sửa scene | 214,46 | 278,5 | 6.645.352 |
| Task 4, thay 18 cây | 234,47 | 260,0 | 5.798.841 |
| Task 5, khoảnh thử đầu | 204,56 | 368,0 | 4.994.796 |
| Sau duyệt Task 5, gom mesh | 223,68 | 277,0 | 5.024.969 |
| Task 6, đủ năm khoảnh | 231,23 | 290,0 | 1.781.881 |
| Task 7, ánh sáng và hậu kỳ | 220,52 | 286,0 | 1.782.709 |

Task 1–3 chỉ có một mốc chung do chưa sửa scene. Task 8 chỉ cập nhật bằng chứng, dùng số đo của Task 7.

## Object ngoài `NATURE_PLANTERS`

- `CAMPUS_ADVENTURE_SHARED_WORLD/Shade tree_12` đến `Shade tree_29`: thay 18 cây cũ bằng prefab cùng tên. `Shade tree_12` được dời vào giữa bãi cỏ Căn tin ở Task 5; bảng tọa độ cũ và khoảng hở ở trên.
- `GARDEN/Flower bed 1` đến `Flower bed 4`: cập nhật hình học trong `Stage_Seed`, `Stage_Sprout`, `Stage_Bloom` qua prefab `IUH_GardenBed`; các instance gán vật liệu cánh theo chỉ số luống. Xóa các mô hình giai đoạn cũ và override vật liệu trỏ đến chúng.
- Bốn MeshFilter `Trimmed hedge foliage`, `White flowers`, `Marigold`, `Pink flowers` dưới `Layered planting and clipped hedges_405` và `Garden beds and furniture_11/Hedge`: nhận hình học đã lọc từ năm bản sao `CanteenFiltered_*`. Trong Task 5–6 cập nhật dữ liệu các bản sao, không sửa mesh nguồn.
- `Campus color grading` đọc profile đã chỉnh. Task 7 chỉ đổi asset profile và `PC_RPAsset`, không ghi scene.

Bộ test toàn dự án tại mốc Task 8 chạy riêng, EditMode 123/123 (`36ee1bc2ac2a46eca3fe17eec94fc441`) và PlayMode 47/47 (`94bee61f31d04b1388b9104c6455b641`).
