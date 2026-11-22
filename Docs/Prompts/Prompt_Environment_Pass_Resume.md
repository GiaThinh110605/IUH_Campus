# Prompt cho Codex (phiên mới) — Tiếp tục lượt C đồ họa: mặt đất + bầu trời

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus` (Unity 6000.5.8f1, URP 17). Đây là phiên mới, phiên trước bị kẹt và đã dừng. Đọc kỹ toàn bộ file này trước khi làm.

## 1. Kiểm tra đầu phiên (bắt buộc)
- `git branch --show-current` phải là `minhnhat/environment-pass`. Sai ⇒ dừng, **không tự đổi nhánh** khi Unity đang mở.
- `git log --oneline -3` phải có `e54b2e6 feat(art): add world-space stylized ground shader` trên `6aa7997`.
- Unity đang mở, MCP `unityMCP` kết nối được (đọc `mcpforunity://editor/state`). MCP không phản hồi ⇒ chờ 1 phút, thử lại 1 lần, vẫn không được thì dừng và nhờ người dùng khởi động lại Unity + Start Server.
- Nếu Unity đang ở Play Mode thì thoát Play trước.
- `git status`: thay đổi chưa commit dự kiến là `Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity` (Task 2 đang dở) và thư mục `Docs/Evidence/CampusGuardian/ART-ENVIRONMENT/` (ảnh "trước" đã chụp). **Giữ nguyên, không reset, không làm lại.**
  - Hai file `Docs/Prompts/Prompt_VRoid_Player.md` và `Docs/superpowers/plans/2026-11-21-vroid-player.md` là của lượt sau: **không đụng, không stage**.

## 2. Đọc trước
1. `AGENTS.md`, `.agents/rules/*`, skill `.agents/skills/urp-graphics-upgrade`.
2. Kế hoạch lượt C: `Docs/superpowers/plans/2026-11-21-environment-ground-sky.md` (Task 1–5, ngưỡng, ranh giới).
3. Prompt gốc: `Docs/Prompts/Prompt_Environment_Pass.md` (quy tắc Git, kiểm chứng, báo cáo). Các quy tắc ở đó vẫn áp dụng.
4. `Docs/AmbientCG_Graphics_Upgrade.md`: vì sao lần trước dán texture theo UV thất bại.

## 3. Tình trạng hiện tại
- **Task 1 xong** (`e54b2e6`): shader `Assets/IUH_Campus/Shaders/IUH_StylizedGround.shader` (chiếu XZ theo tọa độ thế giới, keyword `_GROUND_GRASS` / `_GROUND_PAVING`) và 3 vật liệu trong `Assets/IUH_Campus/Materials/Environment/`.
- **Lỗi làm phiên trước kẹt** (Claude đã xác minh trong `Logs/Editor.log`): dòng 100–102 của shader đặt tên biến là `line`. Đây là **từ khóa dành riêng của HLSL** ⇒ biến thể `_GROUND_PAVING` lỗi compile ("syntax error: unexpected token 'line'", kéo theo lỗi `fwidth`). Biến thể cỏ vẫn chạy nên test Task 1 không bắt được. Vật liệu sân lát sẽ hiện màu hồng.
- **Task 2 đang dở:** scene đã sửa (gán vật liệu thử quanh sảnh, có thể đã thêm cỏ lác đác), đã lưu nhưng chưa commit. Ảnh "trước" (ngày, đêm: lobby, canteen, fountain, overview, close) đã có trong `ART-ENVIRONMENT/`, chưa có ảnh "sau".
- Task 3–5 chưa làm.

## 4. Việc cần làm theo thứ tự
### Bước A — Sửa shader (commit riêng)
- Đổi tên biến `line` → `edgeDist` ở cả 3 chỗ (dòng 100, 101, 102). Rà toàn file xem còn từ khóa dành riêng khác không (`line`, `point`, `triangle`, `sample`, `texture`, `linear`, `centroid`, `precise`, `vector`, `matrix`…), đổi tên nếu có.
- Thêm test EditMode: với cả `_GROUND_GRASS` và `_GROUND_PAVING`, shader không có lỗi (`ShaderUtil.ShaderHasError(shader)` = false sau khi import, `ShaderUtil.GetShaderMessages` không có lỗi), và 3 vật liệu trong `Materials/Environment` không dùng shader lỗi.
- Console 0 error. Chạy EditMode.
- Commit: `fix(art): rename reserved HLSL identifier in ground shader`.

### Bước B — Hoàn tất Task 2 (giữ thay đổi scene đang có)
- Xem lại thay đổi scene hiện có, chỉ được gồm: đổi vật liệu mặt đất khu sảnh + object gốc `NATURE_GROUND` (cỏ lác đác, đã gộp mesh). Nếu có gì ngoài phạm vi đó thì liệt kê và hỏi.
- Kiểm sân lát không còn hồng. Chụp ảnh "sau" đúng góc trong `cameras.json` (ngày và đêm) + cận cảnh nhân vật đứng trên sân lát.
- Đo FPS/draw calls góc sảnh (mốc trước lượt nếu chưa có thì lấy từ ảnh/README hoặc đo trên commit trước rồi ghi rõ cách đo).
- EditMode và PlayMode **chạy tách riêng**, toàn bộ Pass. Commit Task 2 (scene: chỉ hunk của Task 2; dòng `MainCamera` `m_LocalPosition.y` 2.2066925 → 3.2066925 **không stage**).

### Bước C — Task 3 (bầu trời, sương, hạt bay)
- Làm đúng mục Task 3 trong kế hoạch: `Cloud sky.mat` (thêm `_Tint` vào `CloudSky.shader` nếu cần cho đêm, ngày phải giống hệt cũ), sương ngày, **thêm lerp màu sương ngày ↔ đêm vào `IUHStoryNightShift`** (kèm test PlayMode), 1 hệ hạt nhẹ (≤ 60 hạt, tắt/giảm trên mobile).
- Ảnh trước/sau ngày và đêm. Test Pass. Commit Task 3.

### Bước D — Dừng và báo cáo
- **Không làm Task 4–5**: chờ người dùng duyệt ảnh. Không push.

## 5. Chống kẹt (quan trọng)
- Một bước chạy quá **20 phút** mà không tiến triển (MCP timeout lặp lại, test treo, lỗi lặp sau 3 lần sửa) ⇒ dừng và báo kèm log; không lặp vô hạn.
- Trước khi chạy test, thoát Play Mode và chờ compile xong (`data.compilation.is_compiling = false`).
- Sau mỗi lần sửa shader: đọc Console tìm `Shader error`, không chỉ dựa vào test.
- Chạy EditMode và PlayMode thành 2 job riêng; job timeout ⇒ chờ rồi chạy lại 1 lần.

## 6. Git (nhắc lại)
- Stage chọn lọc kèm `.meta`. Không `git add -A`/`.`. Không push, không amend.
- Không stage `*.csproj`, `*.slnx`, `Logs/`, `ProjectSettings/`, `*.mat` Unity tự ghi ngoài `Materials/Environment` và `Cloud sky.mat`, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`, 2 file VRoid nói ở mục 1.

## 7. Báo cáo cuối phiên
- Danh sách commit; số test EditMode/PlayMode.
- Ảnh trước/sau (ngày, đêm) các góc + cận cảnh sân lát; ảnh sương ban đêm.
- Bảng FPS/draw calls góc sảnh: trước lượt, sau Task 2, sau Task 3.
- Mặt đất là mesh gộp hay nhiều renderer; renderer nào đã đổi vật liệu.
- Mọi chỗ lệch kế hoạch.
