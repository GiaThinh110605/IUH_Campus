# Prompt cho Codex — Sửa lỗi người dùng chơi thử: chân/nhảy, khối tím, quái T-pose, hiệu ứng combat

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus`.
Chạy tự động: **không hỏi giữa chừng.** Cần quyết định thì chọn phương án an toàn nhất, ghi lý do. **Không push, không amend.**

## 0. Đầu phiên
- Nhánh phải là `minhnhat/vfx-pass`, HEAD `5433873` (lượt VFX đã commit, chưa push). Sai ⇒ dừng. **Làm tiếp trên nhánh này**, không tạo nhánh mới.
- Unity mở, MCP kết nối (thử lại 3 lần, 2 phút mỗi lần). Thoát Play Mode, Console 0 error, scene `isDirty=false`.
- Không stage: dòng `MainCamera` `m_LocalPosition.y`, `*.mat` Unity tự ghi ngoài phạm vi, `*.csproj`, `*.slnx`, `ProjectSettings/`, `Logs/`, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`, các file chẩn đoán `smoke-*`/`P3-AB*` chưa track trong `VISUAL-POLISH/`.

## 1. Đọc trước
- `AGENTS.md`, `.agents/rules/*` (đặc biệt `match3-combat.md`), skill `.agents/skills/urp-graphics-upgrade`.
- `Docs/Evidence/CampusGuardian/VISUAL-POLISH/README.md` (grounder P1/P2).
- `Docs/Evidence/CampusGuardian/VFX/README.md` (lý do bỏ V1 và V3: **đừng lặp lại cách cũ**).
- `Docs/Evidence/CampusGuardian/INVESTIGATIONS-20261124.md`.

## 2. Người dùng chơi thử ngày 24/11 và báo (ảnh ở `Docs/Evidence/CampusGuardian/PLAYTEST-FIX/user-report/01–04`)
1. **Vùng nhiễm vẫn là khối tím đặc** phủ cả quảng trường, không phải khói.
2. **Nhân vật giật lên xuống theo chiều dọc khi chạy.**
3. **Nhấn Space để nhảy thì nhân vật biến mất (tàng hình).**
4. **Quái ngoài campus đứng T-pose** (tay dang ngang, không có animation).
5. **Chưa có hiệu ứng trong combat.**
- Thêm (Claude thấy trong ảnh): nhân vật **đứng trên đỉnh bồn cây/bờ cỏ cao**, trong khi collider đi dưới đất. Claude nghi `IUHVisualFeetGrounder` lấy cả mặt bồn cây trong `CampusVisibleGround` và `maximumOffset = 2` m, gây ra lỗi 2, 3 và lỗi này. **Phải chứng minh bằng cách tái hiện rồi tắt/bật grounder, không đoán.**

## 3. Ngưỡng chung
- Đo A/B xen kẽ cùng phiên (A, B, A, B, A, B; 120 khung hình làm nóng + 3000 khung hình đo; 1280 × 900 xác minh bằng `Screen.width/height`; VSync tắt). Ghi nguồn điện. Chỉ lưu tsv tóm tắt.
- Mỗi Task: trung vị tăng ≤ **0,8 ms**, p95 tăng ≤ **1,5 ms**, draw calls tăng ≤ 30. Task F1 phải **không tăng** quá 0,3 ms.
- Console 0 error. EditMode và PlayMode tách riêng, toàn bộ Pass. Missing Script = 0 ở 3 scene.

## 4. Các Task (theo thứ tự ưu tiên)

### F1 — Chân, chạy giật, nhảy tàng hình (commit `fix(player): stable visual grounding and visible jump`)
**Bắt buộc theo quy trình gỡ lỗi:**
1. **Tái hiện** trong Play Mode (đi qua MCP/điều khiển giả lập hoặc gọi input của `StarterAssetsInputs`):
   - Chạy thẳng 5 s trên sân lát phẳng: ghi Y thế giới của `J_Bip_C_Hips` và của gốc `Campus Student` mỗi khung hình; tính độ dao động Y của phần hình **trừ đi** dao động của gốc.
   - Nhảy 3 lần: mỗi khung hình ghi `SkinnedMeshRenderer.isVisible`, `enabled`, `bounds`, vị trí các nhánh visual, `lastOffset` của grounder, layer, camera near clip/khoảng cách camera–đầu.
   - Đi men theo bồn cây/bờ cỏ ở ảnh người dùng (gần đài phun nước, khu Ngày hội): ghi Y phần hình và Y collider.
2. **Cô lập**: lặp lại 3 bước trên với `IUHVisualFeetGrounder` **tắt**. So bảng. Kết luận nguyên nhân gốc cho từng lỗi (có thể khác nhau; nếu nhảy tàng hình không do grounder thì tìm tiếp: animation Jump/FreeFall của VRoid, culling bounds, camera clip, script ẩn nhân vật…).
3. **Sửa tại gốc**, hướng ưu tiên (chọn cách đơn giản nhất đạt mục tiêu):
   - Grounder chỉ bám **mặt đất mà CharacterController thực sự đứng** (raycast collider dưới chân, bỏ qua bồn cây/bờ cỏ/bậc trang trí không đi được), **không** lấy cả mặt nhìn thấy của bồn cây.
   - Giới hạn độ dịch nhỏ (ví dụ ±0,15 m quanh độ lệch hiệu chỉnh khi Idle), **làm mượt** theo thời gian (không nhảy từng khung hình theo bàn chân thấp nhất khi chạy). Khi Walk/Run có thể khóa độ lệch đã hiệu chỉnh thay vì tính theo bàn chân đang nhấc.
   - Nếu cách đơn giản nhất là **một độ lệch cố định** hiệu chỉnh 1 lần (như P1 đo được) + bù độ cao mặt lát/cỏ nhỏ thì được, miễn đạt mục tiêu.
   - Không đổi `CharacterController`, `ThirdPersonController`, camera, Starter Assets.
4. Áp cùng cách cho NPC (prefab) nếu NPC dùng chung component; kiểm NPC vẫn không lún.

**Mục tiêu:**
- Chạy trên sân phẳng: dao động Y phần hình so với gốc do grounder gây ra ≤ 0,02 m giữa các khung hình liên tiếp, không thấy giật trong ảnh/video chuỗi.
- Nhảy: phần hình **luôn hiển thị** ở mọi khung hình (isVisible=true khi trong khung hình), tiếp đất không giật.
- Không còn đứng trên đỉnh bồn cây khi collider ở dưới.
- Idle trên sân lát/cỏ/đài phun nước vẫn ±0,03 m (không để lơ lửng lại như trước P1).
- Test PlayMode: chạy 3 s không vượt ngưỡng dao động; nhảy không mất hiển thị; đứng sát bồn cây không bị nâng.
- Chuỗi ảnh: chạy (4 khung), nhảy (4 khung), đứng cạnh bồn cây.

### F2 — Bỏ khối tím, thay bằng khói (commit `feat(story): corruption smoke replaces purple dome`)
`IUHStoryCorruptionZone` (code người dùng, được phép sửa). **Giữ API `Create`/`Purify`** và hành vi (tâm, bán kính, chữ nổi, mờ dần 2 s, Destroy).
- **Mục tiêu số 1: không còn khối cầu/lớp tím đặc phủ sàn.** Kể cả khi khói chưa thật đẹp, khối cầu tím phải biến mất. Nếu cần ranh giới thì chỉ viền rất nhạt (alpha ≤ 0,08).
- Dùng **nguyên prefab** `msVFX_Stylized Smoke 1` của gói Smoke (`ThirdParty/AssetStore/StylizedSmoke/`), **không tự dựng lại từ đầu, không đổi scaling mode** (VFX README ghi lỗi "Local scaling + inverse scale làm hạt rất nhỏ" lần trước).
  - Đặt **nhiều instance** (ví dụ 6–10 cụm) rải trong bán kính vùng + 1 cụm sát mỗi quái. Chỉ đổi màu sang **tím xám sẫm** (ví dụ (0,35; 0,25; 0,45)), alpha đủ **nhìn thấy rõ ban ngày** nhưng không che nhân vật.
  - Tổng hạt ≤ 80 PC / ≤ 30 mobile cho cả vùng (giảm số cụm trên mobile).
  - Không va chạm, không đổ bóng.
- `Purify()`: ngừng phát, mờ dần trong 2 s, Destroy; fade tính từ alpha ban đầu theo thời gian thực (lỗi làm tròn byte lần trước).
- Test PlayMode: `Create` đúng tâm/bán kính, không còn MeshRenderer khối cầu, tổng `maxParticles` ≤ giới hạn; `Purify` hủy ≤ 2,5 s; danh sách rỗng không lỗi.
- Ảnh ngày và đêm cùng góc ảnh người dùng (đứng ở quảng trường nhìn về tòa chính khi đang "Thanh tẩy Nhà E"), trước/sau, và lúc `Purify`.
- Không đạt sau 2 vòng: **vẫn bàn giao phần bỏ khối cầu** (vùng chỉ còn viền rất nhạt + hào quang chân quái), ghi lý do khói chưa đẹp.

### F3 — Quái T-pose (commit `fix(art): idle animation for campus monsters`)
Quái/encounter là phần của Thịnh: **chỉ sửa hình (animation), không đổi logic encounter, collider, beam, khoảng E, arena.** Ghi rõ để người dùng báo Thịnh.
- Tìm model quái trong `CORRUPTED_ENCOUNTER_*` (và proxy trong arena): loại rig (Humanoid/Generic/không rig), có clip animation đi kèm trong FBX/GLB không, Animator/Controller hiện tại.
- Thứ tự cách sửa:
  1. Model có clip sẵn (idle/breath…) ⇒ tạo/gán Animator Controller dùng clip đó.
  2. Rig Humanoid không có clip ⇒ dùng clip Idle humanoid **có sẵn trong project** (Starter Assets hoặc clip của NPC), không tải ngoài.
  3. Rig Generic không clip ⇒ script hình nhỏ: hạ hai cánh tay xuống tư thế tự nhiên khi khởi tạo (xoay xương vai/cánh tay) + nhịp thở/lắc nhẹ. Không có xương tay ⇒ chỉ ghi lại, không làm.
- Animator Cull Completely khi ngoài khung hình. Kiểm proxy arena (`StripToVisuals`) không lỗi và không T-pose.
- Ảnh trước/sau 2 quái, ngày và đêm; ảnh trong arena.

### F4 — Hiệu ứng combat, cách rẻ (commit `feat(combat): world-space hit and defeat effects`)
Combat match-3 là phần Thịnh: **chỉ thêm hình ảnh**, tuân `match3-combat.md`, không đổi luật, sát thương, lượt, UI logic.
- **Không dùng camera phụ / RenderTexture / compositor** (lần trước +1,7 ms vì cách này).
- Hiệu ứng **world-space** trong arena 3D, dùng prefab Cartoon FX đã có trong `ThirdParty/AssetStore/`:
  - Đòn đánh trúng quái: hạt hit tại thân quái (proxy arena).
  - Quái đánh trúng người chơi: hạt hit tại thân người chơi.
  - Combo/kỹ năng: hạt lớn hơn tại quái.
  - Quái bị hạ: hạt nổ/tan tại quái.
- Ăn ô trên bàn match-3 (UI): **không dùng hạt**; nếu muốn thì chỉ hiệu ứng UI nhẹ có sẵn (scale/flash Image) hoặc bỏ qua, ghi lại.
- Pool đơn giản (tối đa ~12 instance), tự trả về khi xong; mobile giảm nửa hạt.
- Sửa code combat chỉ thêm lời gọi phát hiệu ứng 1–3 dòng mỗi điểm hoặc subscribe sự kiện có sẵn. Ghi file/dòng.
- Test PlayMode: phát 50 lần không lỗi, pool không tăng quá trần.
- A/B ở góc arena **lúc đang phát hiệu ứng**. Ảnh: trúng quái, trúng người chơi, quái bị hạ. Hiệu ứng không che lưới match-3/HUD.

### F5 — Hào quang chân quái rõ hơn ban ngày (commit `tweak(vfx): stronger monster ground aura`)
- `IUHCampusMonsterGroundFire`: tăng độ đậm/kích thước/số hạt vừa phải để **thấy rõ ban ngày** (ảnh `VFX/fire-monster1-day.png` hiện gần như không thấy), vẫn thấp ≤ 1 m, không neon chói. Hạt ≤ 24 PC/12 mobile mỗi quái.
- Ảnh cận 2 quái ngày và đêm.

### F6 — Bằng chứng (commit `docs(fix): record playtest fixes`)
`Docs/Evidence/CampusGuardian/PLAYTEST-FIX/README.md`:
- Commit, môi trường, nguồn điện.
- F1: bảng tái hiện có/không grounder, nguyên nhân gốc từng lỗi, cách sửa, số đo sau.
- F2–F5: làm gì, ảnh trước/sau, bảng A/B, file code đã sửa.
- Việc trong phần của Thịnh (F3, F4): liệt kê file/dòng để người dùng báo Thịnh.
- Số test, mọi chỗ lệch. Kèm prompt này.

## 5. Quy tắc tự quyết
- Task quá 30 phút không tiến triển hoặc không đạt sau 2 vòng ⇒ bỏ Task đó (riêng F2 xem mục "không đạt" ở trên): mở lại scene không lưu, xóa asset chưa commit của Task, ghi lý do, làm Task kế tiếp. F1 ưu tiên cao nhất; F6 luôn làm.
- **Chỉ lưu scene khi Task đạt.** MCP mất phản hồi ⇒ ghi `PLAYTEST-FIX/RESUME.md` rồi dừng; **không ghi tay vào file scene**.
- Ảnh tự kiểm: không hồng lỗi shader, không T-pose, không khối tím đặc, không hiệu ứng che HUD/lưới.
- Mỗi Task một commit. Stage chọn lọc kèm `.meta`. Không `git add -A`/`.`.

## 6. Cuối phiên
- Unity về Edit Mode, `isDirty=false`, Console 0 error.
- Tin nhắn cuối ngắn gọn:
  - Commit; Task xong/bỏ (lý do).
  - Nguyên nhân gốc của chạy giật và nhảy tàng hình.
  - Bảng A/B.
  - Link ảnh: chạy/nhảy, khói ngày/đêm, quái sau, combat.
