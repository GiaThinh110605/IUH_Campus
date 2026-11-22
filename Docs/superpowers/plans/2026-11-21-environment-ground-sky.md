# Kế hoạch — Lượt C đồ họa: mặt đất (cỏ, sân lát) + bầu trời, không khí

Ngày 21/11/2026. Nhánh `minhnhat/environment-pass`, tạo từ `main` sau khi merge `minhnhat/remove-cosmetics`.
Người dùng đã duyệt hướng làm (21/11). Thịnh đã đồng ý sửa đồ họa campus.
Phong cách: stylized hoạt hình, khớp cây cỏ Quaternius (lượt A).

## Bài học cần tránh
Ngày 31/10 đã thử gắn texture ambientCG vào cỏ và sân lát nhưng **thất bại**: UV mesh campus không dùng để lặp texture được, ra màu bệt (`Docs/AmbientCG_Graphics_Upgrade.md`, `Docs/Evidence/AmbientCG_20261031/`).
⇒ Lượt này **không dựa vào UV**: chiếu theo tọa độ thế giới.

## Hiện trạng
- Vật liệu (`Assets/IUH_Campus/SharedCampusConcept/Materials/`, URP Lit, smoothness 0,2):
  - `Lawn` màu (0,28; 0,40; 0,14).
  - `Paving pale` (0,83; 0,77; 0,65).
  - `Sandstone paving` (0,69; 0,61; 0,48).
  - Bản ambientCG cũ trong `Materials/ambientCG/` (không dùng cho mặt đất).
- Bầu trời: `Cloud sky.mat`, shader `SharedCampusConcept/Shaders/CloudSky.shader` (`_Zenith`, `_Horizon`, mây trôi).
- Sương: `RenderSettings` linear, màu (0,73; 0,82; 0,85), 125 → 270 m.
- `IUHStoryNightShift` (chuyển đêm khi có biến cố):
  - Đổi đèn mặt trời, ambient và skybox (`_Exposure` hoặc `_Tint`).
  - **Không đổi sương mù.** ⇒ Ban đêm sương vẫn màu xanh sáng ban ngày.
  - `CloudSky` không có `_Exposure`/`_Tint`. ⇒ Codex kiểm xem trời có tối đi không; nếu không thì xử lý ở Task 3.
- Hậu kỳ: `Campus volume.asset` (đã chỉnh ở lượt A, giữ nguyên trừ khi cần).

## Task 1 — Shader mặt đất `feat(art): add world-space stylized ground shader`
- `Assets/IUH_Campus/Shaders/IUH_StylizedGround.shader` (HLSL URP, cùng kiểu `IUH_StylizedFoliage`):
  - Chiếu XZ theo tọa độ thế giới (`positionWS.xz / _TileSize`); không dùng UV mesh.
  - Hai chế độ (keyword `_GROUND_GRASS` / `_GROUND_PAVING`):
    - **Cỏ:** 2–3 tông (`_ColorA/_ColorB/_ColorC`) trộn theo noise giá trị/simplex ở 2 tỉ lệ (mảng lớn 8–15 m + chi tiết 1–2 m). Thêm vệt sáng nhẹ. Tùy chọn texture `Grass.png` của Quaternius làm chi tiết nhỏ.
    - **Sân lát:** vẽ mạch gạch bằng shader (hình chữ nhật so le, kích thước viên `_TileSize`, mạch `_GroutWidth`, màu mạch `_GroutColor`). Mỗi viên lệch màu nhẹ theo hash. Hoặc dùng texture ambientCG paving lặp theo mét nếu đẹp hơn (thử cả hai, chọn bằng ảnh).
  - Ánh sáng: Lambert mềm + bóng (cascade) + `SampleSH` + fog. Smoothness thấp. SRP Batcher.
  - Pass `ShadowCaster`, `DepthOnly`, `DepthNormals` (cho SSAO nếu bật).
- Vật liệu mới `Assets/IUH_Campus/Materials/Environment/`: `M_Ground_Grass`, `M_Ground_PavingPale`, `M_Ground_Sandstone`.
  - **Không sửa** `Lawn.mat`/`Paving*.mat` gốc; lượt sau gán vật liệu mới thay thế (quay lui dễ).
- Test EditMode: shader tìm được và `isSupported`; 3 vật liệu đúng shader, không hồng.
- Đưa kế hoạch và prompt vào commit này.

## Task 2 — Gán thử 1 khu `feat(art): pilot stylized ground around the lobby`
- Chụp ảnh **trước** (ngày, đêm) 4 góc: sảnh, Căn tin, đài phun nước, nhìn xa toàn cảnh. Lưu `cameras.json`.
- Chỉ gán vật liệu mới cho renderer mặt đất **quanh sảnh** (các renderer dùng `Lawn`/`Paving pale`/`Sandstone paving` trong bán kính khoảng 40 m quanh sảnh). Nếu mặt đất là một mesh gộp toàn campus thì gán cả mesh đó và ghi rõ.
  - Thay `sharedMaterials` đúng slot; không đổi mesh, transform, collider.
- Thêm cỏ mọc lác đác: cụm `PF_Grass_Short`/`PF_Grass_Wispy`/`PF_Clover` (lượt A) **dọc mép bãi cỏ khu sảnh**, gộp mesh theo vật liệu như `NATURE_PLANTERS`, đặt dưới object gốc mới `NATURE_GROUND`. Không đặt trên đường đi, không chắn luống vườn hay quái.
- Ảnh sau cùng góc. **Dừng, gửi ảnh cho người dùng duyệt** (mặt đất ngày, đêm, cận cảnh nhân vật đứng trên sân lát).

## Task 3 — Bầu trời, sương, hạt bay `feat(art): stylized sky, fog and ambient particles`
- `Cloud sky.mat`: chỉnh `_Zenith`/`_Horizon` tươi hơn, hợp tông stylized (không neon).
  - Nếu cần cho ban đêm: thêm `_Tint` (mặc định trắng) vào `CloudSky.shader` để `IUHStoryNightShift` làm tối trời (đã có nhánh xử lý `_Tint`). Ban ngày phải giống hệt trước khi thêm.
- Sương ngày: giữ linear, tinh chỉnh màu theo trời và khoảng cách (đề xuất 80 → 220 m) để có chiều sâu mà không mờ khu chơi.
- `IUHStoryNightShift`: **thêm** lerp màu sương ngày → đêm (tím sẫm hợp `NightColor`) và khôi phục khi về ngày.
  - Chỉ thêm, không đổi các phần khác.
  - Test PlayMode: `ToNight` rồi `SetNightImmediate` ⇒ `RenderSettings.fogColor` đổi; `ToDay` ⇒ trả lại màu ngày.
- Hạt bay quanh người chơi: 1 Particle System nhẹ (bụi nắng ban ngày, đốm sáng tím ban đêm nếu dễ) theo camera.
  - ≤ 60 hạt, không va chạm, không đổ bóng.
  - Có cờ tắt trên mobile (`Application.isMobilePlatform` hoặc theo Quality level).

## Task 4 — Áp toàn campus `feat(art): stylized ground across campus` (chỉ sau khi người dùng duyệt Task 2)
- Gán vật liệu mới cho mọi renderer mặt đất còn lại; cỏ mọc lác đác ở các bãi cỏ khác. Gộp mesh để draw calls tăng ít.
- Ảnh trước/sau 4 góc ngày và đêm.

## Task 5 — Bằng chứng `docs(art): record environment pass`
- `Docs/Evidence/CampusGuardian/ART-ENVIRONMENT/README.md`:
  - Ảnh trước/sau.
  - Bảng FPS/draw calls góc sảnh qua từng Task.
  - Danh sách renderer đổi vật liệu, object mới.
  - Số test.

## Ngưỡng và ranh giới
- FPS Editor góc sảnh không giảm quá **10%** so với trước lượt (đo lại mốc trước khi sửa). Draw calls tăng ≤ 30.
- Không tải asset ngoài. Không sửa mesh, collider, quái, check-in, vườn, nhân vật, UI.
- Không sửa vật liệu tòa nhà (để lượt D).
- iOS: không thêm hiệu ứng nặng vào cấu hình mobile; hạt bay tắt hoặc giảm trên mobile.

## Dừng và báo khi
- Shader lỗi compile sau 3 vòng.
- Ảnh mặt đất sau xấu hơn trước (như lần ambientCG). Gửi ảnh, không tự áp toàn campus.
- FPS giảm quá ngưỡng.
- Phải sửa ngoài phạm vi trên.
