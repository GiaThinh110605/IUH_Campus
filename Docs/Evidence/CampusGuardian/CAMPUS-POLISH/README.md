# Campus polish — khói, hào quang, rào, bồn hoa, tưới hoa (H1–H5)

Nhánh `minhnhat/campus-polish` từ `main` `45eb1e6` (sau PR #52). Ngày 25/11/2026, Unity 6000.5.8f1 Windows Editor, URP. Thực hiện: Claude Code. Yêu cầu người dùng (kèm 5 ảnh chơi thử):
1. Khói vùng nhiễm còn mờ, làm đậm hơn.
2. Hào quang tím dưới chân quái lệch ~1 m.
3. Hàng rào chưa hoàn thiện.
4. Bồn hoa đi xuyên vào được và còn dính sân khấu, căn tin.
5. Bỏ chậu trồng hoa, chỉ còn tưới hoa (người dùng chọn: bỏ 4 luống đất, tưới các bồn hoa campus; chặn cả 6 luống cỏ).

| Task | Commit |
|---|---|
| H5 tưới hoa thay luống trồng | `26663f8` |
| H4 luống cỏ không đi vào được, tách căn tin/sân khấu | `638a5b4` |
| H3 rào bao campus | `ad9bb0b` |
| H1 khói đậm hơn | `cdb5736` |
| H2 hào quang đúng chân | `f42c902` |

## H5 — Tưới hoa
- Xóa 4 luống đất `GARDEN/Flower bed 1–4` (prefab `IUH_GardenBed`, vật liệu `M_Garden_Soil`, script `IUHGardenBedView`; kiểm GUID không còn tham chiếu) và vòng mua hạt/bón phân/thu hoạch.
- `IUHGardenManager` tự nhận **8 bồn hoa** từ mesh cỏ (`Garden beds and furniture_11/Lawn`: 6 luống; `Curved fountain gardens_101/Lawn`: 2 bồn cong), mỗi mảnh cỏ liền nhau là một bồn, id theo vị trí (`bed_x_z`).
- `IUHGardenPanelUI` (giữ tên lớp để component trong scene không đổi): đứng cách mép bồn ≤ 1,8 m (tính ngang) ⇒ chữ nổi `[E] Tưới hoa`; nhấn E ⇒ **+10 vàng**, nước bắn, chữ `+10 Vàng`; mỗi bồn chờ **90 s** (`Tưới lại sau m:ss`). Không tưới khi đang combat, hội thoại, cửa hàng hoặc UI khác mở.
- Lưu `IUH_Garden_v3` = thời điểm tưới cuối theo id; save v2 cũ bị bỏ qua (mọi bồn tưới được ngay).
- Kiểm trong Play: tưới bồn `bed_-20_1` +10 vàng, tưới lại ngay bị chặn (còn 90 s). Test: GardenBed 4, GardenSave 4 (+6 case), GardenManager 2, GardenPanel 2.

## H4 — Luống cỏ
- Luống `(-20,-1)` cạnh **căn tin**: cạnh nam dời lên 3,7 m (bệ đá z −7,5 → −3,8, cách căn tin 0,5 m). Luống `(20,-1)` cạnh **sân khấu**: cạnh đông dời vào 6,5 m (bệ đá x 28,5 → 22,0, cách sân khấu 0,5 m). Sửa trực tiếp 2 mesh hộp `Garden beds and furniture_11_Lawn/_Warm granite` (mỗi mesh 1 object dùng).
- Xóa tam giác cây cỏ trong phần cắt: `Near_*` (5 mesh), `LobbyGround_Grass/LeafAtlas`, `NATURE_FLOWERBEDS_FILL_*` (4).
- `NATURE_LAWN_WALLS`: tường vô hình cao 1,2 m quanh mép 6 luống. Thử: đi từ lối lát vào luống đều dừng ở mép (vd. z 5,5; x −28,5).
- Ảnh: căn tin [trước](H4-canteen-bed-before.png) · [sau](H4-canteen-bed-after.png); sân khấu [trước](H4-stage-bed-before.png) · [sau](H4-stage-bed-after.png) · [trên cao](H4-stage-bed-top-after.png).

## H3 — Rào bao campus
- Hiện trạng: tường đá cổng chỉ tới x ±23,5, song sắt tới ±28,85 và bắt đầu từ 0,85 m ⇒ đoạn song sắt lơ lửng ngoài tường; ngoài đoạn cổng campus không có rào; tường cổng không có collider (chỉ 2 trụ) ⇒ đi ra bãi cỏ bên ngoài được (ảnh người dùng 1).
- `CAMPUS_PERIMETER_FENCE`: tường đá 2,4 × 0,5 m + song sắt 0,08 m mỗi 0,5 m (cùng vật liệu cổng) nối từ ±23,5 ra ±56 ở mặt nam (chừa gốc `Shade tree_29` tại x 32) và chạy lên cạnh đông/tây tới ký túc xá (z 11,2) và thư viện (z 17,2). Collider hộp vô hình dọc 3 cạnh và ngang cả lối cổng.
- Thử: đi ra qua lối cổng, tường cổng, cạnh đông, cạnh tây đều bị chặn (z −46,4; x ±55,4). Ảnh: [cổng trước](H3-gate-before.png) · [cổng sau](H3-gate-after.png) · [góc đông nam](H3-east-corner-after.png).

## H1 — Khói vùng nhiễm
`IUHStoryCorruptionZone`: alpha 0,28 → **0,55**, màu (0,22; 0,17; 0,28) → (0,30; 0,21; 0,40); 8 → 12 cụm + 2 → 3 cụm sát quái (mobile 6 + 3); trần hạt 80 → 140 PC, 30 → 50 mobile. [Ảnh ban ngày](H1-smoke-day-after.png) (vùng nhiễm tạo thủ công quanh quái đài phun nước + thể thao).

## H2 — Hào quang chân quái
Model GLTF có rig đứng lùi ~1 m so với gốc encounter. `IUHCampusMonsterGroundFire.AlignToFeet()` (gọi trong `Awake`) đặt hào quang vào trung điểm các xương "foot" của encounter (chỉ X/Z). Play: cả 5 hào quang về local (−0,04; 0,19; 0,30). Không sửa scene. [Ảnh cận chân](H2-aura-feet-after.png) (vòng xanh là hologram AR được bật để chụp).

## H6 — Chơi thử lần 2 (bồn hoa, cây, sân khấu)
Người dùng báo: vẫn vào được luống (viền đá và cỏ), có cây dính căn tin/sân khấu, đi xuyên sân khấu.
- Nguyên nhân luống: tường H4 đặt ở mép cỏ, cao 1,2 m (đỉnh ~1,34 m) < độ nhảy `JumpHeight` 1,4 m ⇒ nhảy qua được; viền đá nằm ngoài tường nên đứng lên được. Sửa: `NATURE_LAWN_WALLS` dựng lại theo **mép bệ đá** của cả 6 luống và 2 bồn cong đài phun nước, cao **3 m** (`IUHFlowerbedBuilder.BuildWallOnly(root, height, params surfaces)`).
- Cây: `Shade tree_13` (−23; −4) sát tường căn tin ⇒ (−23; 1); `Shade tree_17` (23; −4) nằm ngoài luống đã thu, sát sân khấu ⇒ (15; 2). [Căn tin](H6-canteen-tree-after.png) · [sân khấu](H6-stage-tree-after.png).
- Sân khấu `Campus festival_10` chỉ có collider sàn 0,7 m ⇒ thêm `MeshCollider` cho vách/mái `Festival violet` và khung `Dark bronze`. Mặt trước (bậc lên, hướng nam) vẫn mở.
- `CHECKIN_SPOT_FESTIVAL` (16; 3) nằm trong luống sân khấu (bị tường chặn) ⇒ dời ra lối lát (16; 8).
- Thử Play (CharacterController.Move): đi và nhảy (+1,5 m) vào luống căn tin dừng ở x −11,2; nhảy vào luống sân khấu từ bắc dừng ở z 5,8; vách sau sân khấu dừng ở z −1,7; bậc trước vẫn lên sân khấu được; trong đài phun nước dừng ở bồn cong (x −7,9); tới check-in Ngày hội (16; 8) được.

## Kiểm thử
- EditMode **150/150**, PlayMode **75/75**. Missing Script = 0 ở 3 scene. Console 0 error, scene `isDirty=false`.
- Scene commit bằng script ghép block theo cây con (thêm/sửa/xóa, gồm prefab instance và block stripped); kiểm 0 fileID trùng, 0 tham chiếu treo. Không commit thay đổi Unity tự ghi (vật liệu nhúng, `UniversalAdditionalLightData` của đèn drone, override prefab, dòng `MainCamera`).

## Chỗ lệch
1. Chưa đo A/B hiệu năng cho lượt này: thêm rào ~7.100 đỉnh (2 renderer), tường va chạm không vẽ; khói tăng trần hạt 80 → 140.
2. Bồn cong ở đài phun nước dùng hộp bao để tính khoảng cách tưới nên có thể nhắc tưới khi đứng ở khoảng trống giữa hai đầu cung.
3. Kiểm tưới trong Play đã cộng +10 vàng vào save thật trên máy dev.
