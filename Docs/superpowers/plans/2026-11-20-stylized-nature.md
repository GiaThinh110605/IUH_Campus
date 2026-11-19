# Kế hoạch — Lượt A đồ họa: cây cỏ stylized + ánh sáng

Thiết kế: `Docs/superpowers/specs/2026-11-20-stylized-nature-design.md` (đọc mục "Chọn mô hình").
Nhánh: `minhnhat/stylized-nature`, tạo từ `main` sau khi lượt 5 đã merge. Thịnh đã đồng ý sửa scene campus.
Pack nguồn (ngoài repo): `D:\IUH\CongNgheMoi\CK\_downloads\quaternius\StylizedNatureMegaKit\` (CC0 1.0).

Mỗi Task một commit. Task có scene: chỉ stage hunk của object được nêu. Chụp ảnh **trước** khi sửa scene, lưu vị trí camera vào `cameras.json`.

## Task 1 — Nhập pack `chore(art): import Quaternius stylized nature subset`
- Chép **chỉ file dùng** (xem bảng "Chọn mô hình") từ `FBX (Unity)/` và `Textures/` vào:
  - `Assets/IUH_Campus/ThirdParty/Quaternius/StylizedNatureMegaKit/Models/`
  - `Assets/IUH_Campus/ThirdParty/Quaternius/StylizedNatureMegaKit/Textures/`
  - Không chép OBJ, glTF, FBX thường, preview.
- `SOURCE.md` trong thư mục pack: link, tác giả Quaternius, CC0 1.0, ngày tải 20/11/2026, bản Standard. Kèm `License_Standard.txt` gốc.
- Tạo `Docs/Credits.md` (chưa có) với mục Quaternius.
- Import model: Read/Write tắt, không import camera/light, Material Creation Mode = None (dùng vật liệu của dự án).
- Import texture:
  - Lá, hoa, cỏ: Max Size 1024, alpha is transparency.
  - Vỏ cây: Max 2048, normal map đánh dấu Normal.
  - Override iOS: ASTC 6x6.
- Đưa spec, kế hoạch, prompt vào commit này.

## Task 2 — Shader lá/cỏ `feat(art): add stylized foliage shader`
- `Assets/IUH_Campus/Shaders/IUH_StylizedFoliage.shader`: HLSL tay cho URP 17, không Shader Graph.
  - Pass `UniversalForward`:
    - Alpha clip `_Cutoff`; `_BaseMap` × `_BaseColor`.
    - Tô 2 tông: half-lambert theo main light → `lerp(_ShadeColor, 1, smoothstep(_RampMin, _RampMax, ndl))`.
    - Nhận bóng (`_MAIN_LIGHT_SHADOWS*`, cascade); ambient bằng `SampleSH`; fog.
    - `Cull Off`, normal lật cho mặt sau.
  - Gió: vertex offset theo world position, `sin(_Time.y * _WindSpeed + worldPos.x*0.3 + worldPos.z*0.2)`, nhân `_WindStrength` và trọng số chiều cao (vertex color R nếu có, nếu không dùng `positionOS.y`). Keyword `_WIND_ON` (shader_feature) để tắt.
  - Thêm pass `ShadowCaster` và `DepthOnly` dùng cùng alpha clip và vertex offset; `DepthNormals` nếu đơn giản.
  - Tương thích SRP Batcher (mọi thuộc tính trong `CBUFFER_START(UnityPerMaterial)`).
- Vật liệu `Assets/IUH_Campus/Materials/Nature/`:
  - `M_Nature_Leaves` (shader mới, `Leaves_NormalTree*.png`), `M_Nature_Bush`, `M_Nature_Flowers`, `M_Nature_Grass` (shader mới).
  - `M_Nature_Bark` (URP Lit, smoothness ≤ 0.1, có normal).
  - Màu lá xanh tươi hợp campus nhiệt đới, không đỏ/cam mùa thu.
- Test EditMode: shader tìm được, `isSupported`, không lỗi compile; 5 vật liệu dùng đúng shader, không hồng.

## Task 3 — Prefab `feat(art): build stylized nature prefabs`
- `Assets/IUH_Campus/Prefabs/Nature/`: `PF_Tree_Common_1…5`, `PF_Bush`, `PF_Bush_Flowers`, `PF_Flower_3_Group`, `PF_Flower_4_Group`, `PF_Grass_Short`, `PF_Grass_Wispy`, `PF_Clover`, `PF_Plant_1`, `PF_Fern`.
- Gán vật liệu Task 2. Cây: 1 `CapsuleCollider` ở thân, không collider ở tán. Bụi, hoa, cỏ: không collider.
- Cây: `LODGroup` 2 mức (LOD0 mesh; cull ở khoảng 3% màn hình) nếu pack không có LOD.
- Vật liệu bật GPU Instancing.
- Test EditMode: mọi prefab trong `Prefabs/Nature` không Missing, không vật liệu null.

## Task 4 — Cây bóng mát `feat(art): replace campus shade trees`
- 18 object `Shade tree_*` trong scene:
  - Với mỗi cây, đặt 1 `PF_Tree_Common_N` (N xoay vòng 1…5) cùng parent, vị trí, xoay Y.
  - Scale sao cho chiều cao tán gần bằng cây cũ (đo bounds), lệch ngẫu nhiên ±10% theo seed cố định.
  - Xóa cây cũ.
- Collider: nếu cây cũ có collider thân, cây mới có collider tương đương. Người chơi không đi xuyên thân, không bị kẹt ở tán.
- Ghi bảng tên/vị trí 18 cây vào README.
- Ảnh trước/sau 3 góc: sảnh, Căn tin, đài phun nước. Mỗi góc chụp ngày và đêm (sau biến cố).

## Task 5 — Vườn + 1 khoảnh cỏ thử `feat(art): stylized garden stages and pilot planter`
- `IUH_GardenBed.prefab`: thay mô hình trong 3 ô:
  - `Stage_Seed`: `PF_Clover` thu nhỏ.
  - `Stage_Sprout`: `PF_Plant_1`/`PF_Fern` nhỏ.
  - `Stage_Bloom`: `PF_Flower_3_Group`/`PF_Flower_4_Group`.
  - Code và tên ô không đổi; test vườn hiện có phải Pass.
- **Khoảnh cỏ thử**, tâm (20, -29):
  - 2 dải bồn của khoảnh này thuộc mesh gộp `Layered planting and clipped hedges_405` (hiện scene dùng bản sao lọc `Meshes/Garden/CanteenFiltered_*`).
  - Lọc thêm bỏ tam giác của 2 dải này, theo cách đã làm ở lượt GARDEN, ghi đè các bản sao trong `Meshes/Garden/` hoặc tạo bản sao mới.
  - Đặt `PF_Bush` + `PF_Bush_Flowers` + `PF_Flower_*_Group` dọc 2 dải, dưới object gốc mới `NATURE_PLANTERS` (cấp gốc scene).
  - Thêm vài cụm `PF_Grass_*`/`PF_Clover` ở viền khoảnh.
- Ảnh trước/sau khoảnh thử (ngày, đêm), ảnh vườn 3 giai đoạn.
- **Dừng sau Task 5, gửi ảnh cho người dùng duyệt.** Người dùng đồng ý thì làm Task 6.

## Task 6 — Các khoảnh còn lại `feat(art): stylized planters across campus` (chờ duyệt)
- Làm như khoảnh thử cho 4 khoảnh còn lại (8 dải).
- Dải nào cách quái ≤ 1 m: đặt bụi tránh đè quái, không dời quái.

## Task 7 — Ánh sáng + hậu kỳ `feat(art): tune lighting and post-processing for stylized look`
- Theo `.agents/skills/urp-graphics-upgrade` (đọc hết). Ưu tiên sửa **asset**: `Assets/Settings/IUH_Campus_Lighting_Profile.asset` (Volume đang dùng trong scene: kiểm tra), `PC_RPAsset`, `Mobile_RPAsset`.
- Mục tiêu: sáng, ấm, bóng mềm.
  - Tonemapping ACES; saturation +10–15; contrast +8–12; bloom nhẹ.
  - Mặt trời 40–50°, hơi ấm; shadow soft.
- Tìm nguồn độ bóng "nhựa" của vật liệu campus: liệt kê vật liệu smoothness > 0.5 trên bề mặt lớn. **Chỉ báo danh sách, không sửa hàng loạt**; sửa tối đa vật liệu cỏ, đất, lá nếu thuộc `IUH_Campus/**`.
- Ban đêm sau biến cố vẫn đọc rõ (ảnh).
- iOS: profile/quality mobile không thêm hiệu ứng nặng.

## Task 8 — Bằng chứng `docs(art): record stylized nature pass`
- `Docs/Evidence/CampusGuardian/ART-NATURE/README.md`: ảnh trước/sau, `cameras.json`, bảng FPS/draw calls/tris (Editor, Stats hoặc Profiler) trước và sau, danh sách object ngoài `NATURE_PLANTERS` bị sửa/xóa, số test.

## Ranh giới
- Không đổi vị trí quái, check-in, NPC, đường đi, tòa nhà; không sửa code gameplay.
- Không sửa FBX gốc, không commit nguyên pack.
- Chỉ thêm `Shaders/IUH_StylizedFoliage.shader`; không sửa shader khác.
