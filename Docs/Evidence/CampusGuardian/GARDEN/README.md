# Vườn hoa Căn tin

Kiểm chứng ngày 19/11/2026 trong Unity 6000.5.8f1, Windows Editor qua Unity MCP. Cân bằng `f7f5a62`, tầm E `01659bd`, bố trí vườn và cập nhật prompt `31bc72f`.

## Bố trí bốn luống

Đo theo đỉnh mesh ở tọa độ thế giới, đơn vị mét. Hàng rào bo tròn dài hơn phần trồng hoa cũ, nên kích thước luống lấy theo hộp bao của cả dải.

| Dải | X của cả dải | Z của cả dải | Rộng |
|---|---:|---:|---:|
| Bắc, cạnh ô dù | −27,7478 đến −12,2522 | −24,9593 đến −22,9750 | 1,9843 m |
| Nam | −27,7478 đến −12,2522 | −35,0250 đến −33,0587 | 1,9663 m |

Mỗi dải dài 15,4956 m. Bốn luống chạy theo trục X ở y = 0,36 m, dài 7,7078 m mỗi luống và có khe 4 cm giữa hai luống cùng dải.

| Luống | Tâm X | Tâm Z |
|---|---:|---:|
| 1, bắc tây | −23,8739 | −23,9672 |
| 2, bắc đông | −16,1261 | −23,9672 |
| 3, nam tây | −23,8739 | −34,0419 |
| 4, nam đông | −16,1261 | −34,0419 |

Scene thay sáu prefab instance cũ trong `GARDEN` bằng bốn luống. Ngoài `GARDEN`, scene chỉ thay vị trí root `CORRUPTED_ENCOUNTER_CANTEEN` và năm tham chiếu MeshFilter sau. Hàng rào thuộc mesh gộp, không có object khung hay collider riêng của hai dải để xóa.

| Đường dẫn object trong scene | GUID mesh nguồn | GUID bản lọc | Tam giác bỏ / giữ |
|---|---|---|---:|
| `Layered planting and clipped hedges_405/Trimmed hedge foliage` | `ebcd60e235546864098fa4dd2ff910fb` | `100b2e35c6ae20a449f25c2db6060cdd` | 73.560 / 383.600 |
| `Layered planting and clipped hedges_405/White flowers` | `41c067555bbd35940acba6e106ce17fd` | `16acd5d4b2984974c9881e0c25da91bd` | 18.432 / 153.600 |
| `Layered planting and clipped hedges_405/Marigold` | `09dfdf3b11a56704ab0d9d37c1b7d647` | `8eb1354cc451aae4cb83d28c94074395` | 36.096 / 288.000 |
| `Layered planting and clipped hedges_405/Pink flowers` | `9fdcc986d7067c446b72bf27c3be3c90` | `e71ec4c7097d16049a29959262c9bb55` | 18.432 / 92.160 |
| `Garden beds and furniture_11/Hedge` | `709468282709cf54bb5b30474485060f` | `9f446709478ee4d4998bd052e1c04819` | 33.792 / 168.960 |

Quét GUID chỉ thấy tham chiếu mesh nguồn trong scene campus. Bản lọc ở `Assets/IUH_Campus/Meshes/Garden/` giữ nguyên dữ liệu đỉnh và thứ tự chỉ số của mọi tam giác ngoài hai dải. Vì vậy các cụm hoa và hàng rào khác giữ nguyên hình học. [Trước khi thay](canteen-planters-before.png) · [Sau khi thay, cùng góc](canteen-planters-after.png).

Quái Căn tin dời từ `(-22, 0.05, -36)` sang `(-22, 0.05, -43)`. Raycast chạm `Campus ground` ở y = 0. Hộp kiểm ở điểm mới chỉ chạm hai collider con của chính encounter. Điểm check-in gần nhất là Cổng chính, cách 24,60 m. Kiểm 10 encounter trong campus, gồm năm encounter có thêm đường AR. Khoảng cách ngang ngắn nhất từ viền bất kỳ luống nào đến quái là 7,975 m, tại Căn tin. Bồn hoa nhiễm độc là kế tiếp ở 8,975 m. Story và beacon tra theo tên object rồi dùng Transform hiện tại. Tọa độ cũ trong `IUHARSceneBuilder.cs` thuộc builder legacy và bản đồ runtime hiện hành không có tọa độ riêng cho Căn tin.

### Thay mô hình sau này

1. Mở `Assets/IUH_Campus/Prefabs/Garden/IUH_GardenBed.prefab` ở Prefab Mode.
2. Thay các con bên trong `Stage_Seed`, `Stage_Sprout`, `Stage_Bloom` và giữ gốc mỗi nhóm ở scale 1.
3. Rải mô hình dọc trục X phía trên đất, giữ viền đá và BoxCollider của prefab. Có thể đổi vật liệu cánh hoa riêng cho mỗi instance.
4. Xem lại cả bốn instance ở ngày và đêm. Khi chuyển giai đoạn, nhóm mô hình tăng từ 0,3 lên 1 trong 0,6 giây unscaled. Lúc nạp save, nhóm hiện ở scale đầy đủ.

## Cân bằng và tầm E

- HP quái ×0,6, sát thương ×0,7 và thêm 30 giây. Ca 1.000 HP, 65–90 sát thương, 90 giây cho kết quả 600 HP, 46–63 sát thương, 120 giây. HUD ngoài trận hiển thị số đã nhân.
- Quái thường nhận E hoặc tự mở trận trong tối đa 5 m. Quái AR vẫn dùng 4,8 m. PlayMode kiểm ở 8 m cờ Tương tác được giữ và ở 3 m cờ mở trận.
- Lượt chơi đã kiểm ở Việc 1–2: phím E ở 8 m không mở trận, ở 4 m mở trận. Chương 1 ở cấp 1/1/1 thắng sau 10 nước đổi ô hợp lệ qua `TryPerformSwap`, còn 899/1.000 HP và 110,1 giây. [Ảnh kết quả trận](balance-match3-result.png).

## Kiểm chứng cuối

| Toàn bộ test | Trước lượt cân bằng | Trước Việc 3 | Sau Việc 3 |
|---|---:|---:|---:|
| EditMode | 114/114 | 114/114 | 115/115 |
| PlayMode | 35/35 | 40/40 | 40/40 |

Job EditMode `4a70f9c7d08e42fd92de739dd7d66034`, PlayMode `8cd9887a53934e4a88a02b77425c39d0`. Console 0 error. Missing Script 0 ở Campus, AR Encounter, Combat Arena và 84 prefab.

- Trong Play, ở cách đầu luống 3 là 1,35 m và cách tâm 5,205 m, nút Tương tác mở đúng bảng. Lùi ra 3,25 m từ đầu luống thì bảng đóng. Đã kiểm qua cờ Tương tác của game.
- Gieo và tưới cả bốn luống, bón luống 1 và 2, tua đồng hồ vườn qua hai lần 65 giây và tưới lần hai. Hái luống 1, 2 và 3 được 45, 45 và 30 Vàng. Hái lặp trả về false. Vàng 100 → 40 → 160.
- Save `v2` chứa bốn luống. Test xác nhận dữ liệu `v1` sáu luống cho bốn luống trống. Sau Play đã khôi phục và đối chiếu 8/8 khóa PlayerPrefs của vườn, kho Guardian và cốt truyện. Ảnh đêm dùng thiết lập ánh sáng sau biến cố của `IUHStoryNightShift`.

| Giai đoạn | Ngày | Đêm |
|---|---|---|
| Trống | [Ảnh](canteen-empty-day.png) | [Ảnh](canteen-empty-night.png) |
| Mầm | [Ảnh](canteen-sprout-day.png) | [Ảnh](canteen-sprout-night.png) |
| Nở | [Ảnh](canteen-bloom-day.png) | [Ảnh](canteen-bloom-night.png) |
