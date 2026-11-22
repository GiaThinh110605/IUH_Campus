# Prompt cho Codex — Lượt C, Task 4–5: áp mặt đất stylized toàn campus + bằng chứng

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus`.

## Kiểm tra đầu phiên
- `git branch --show-current` = `minhnhat/environment-pass`, `git log --oneline -1` = `8cef559`. Sai ⇒ dừng, không tự đổi nhánh.
- Unity mở, MCP `unityMCP` kết nối (đọc `mcpforunity://editor/state`). Không phản hồi ⇒ chờ 1 phút, thử lại 1 lần, rồi dừng và nhờ người dùng khởi động lại Unity + Start Server.
- Thoát Play Mode. Scene `isDirty=false`. Console 0 error.

## Đọc trước
- `AGENTS.md`, `.agents/rules/*`, skill `.agents/skills/urp-graphics-upgrade`.
- Kế hoạch `Docs/superpowers/plans/2026-11-21-environment-ground-sky.md` (Task 4, Task 5, ngưỡng, ranh giới).
- `Docs/Prompts/Prompt_Environment_Grass_Tune.md` (cách Task 2b gộp cỏ).

## Người dùng đã duyệt (23/11)
Sân lát, cỏ có lớp chi tiết (Task 2b), sương và trời. Áp toàn campus theo đúng thông số hiện tại; **không chỉnh thêm shader hay vật liệu**.

## Task 4 — `feat(art): stylized ground across campus`
1. Liệt kê mọi renderer còn dùng `SharedCampusConcept/Materials/Lawn.mat`, `Paving pale.mat`, `Sandstone paving.mat` trong scene campus (trừ 7 renderer đã đổi ở Task 2). Ghi bảng: tên, khu, slot.
   - Thay `sharedMaterials` đúng slot bằng `M_Ground_Grass` / `M_Ground_PavingPale` / `M_Ground_Sandstone`. Không đổi mesh, transform, collider.
   - Không đụng renderer tòa nhà, luống vườn (`NATURE_PLANTERS`), quái, check-in, UI.
2. Cỏ lác đác ở các bãi cỏ còn lại, cùng cách Task 2b (mép bãi, seed cố định, không trên đường đi, luống vườn, chỗ quái đứng hay điểm check-in).
   - **Ngân sách hiệu năng chặt:** sau Task 2b FPS đã giảm 6,3%. Gộp tất cả cụm cỏ mới vào mesh theo vật liệu (tốt nhất mở rộng 2 mesh hiện có hoặc thêm ≤ 2 mesh nữa). Tổng cụm toàn campus ≤ 150.
   - Đặt dưới `NATURE_GROUND`.
3. Đo FPS/draw calls góc sảnh **và** góc toàn cảnh (`overview` trong `cameras.json`), cùng cách đo `lobby-performance-grass-tune.tsv`. So với trước lượt C (sảnh 217,25 FPS / 209 draw calls; toàn cảnh đo lại trên mốc cũ nếu chưa có, ghi rõ cách đo):
   - FPS giảm > 10% hoặc draw calls tăng > 30 ⇒ giảm số cụm cỏ hoặc gộp thêm. Vẫn vượt ⇒ dừng và báo số.
4. Ảnh `*-campus-after-day.png` / `*-campus-after-night.png` cho 5 góc trong `cameras.json`.
5. Kiểm chứng: Console 0 error; EditMode và PlayMode **chạy tách riêng**, toàn bộ Pass; Missing Script = 0 ở 3 scene; Play 30–60 s đi qua sân lát và bãi cỏ ở nhiều khu, E ở quái (8 m chưa vào / 4 m vào trận), mở vườn, chuyển đêm.
6. Commit Task 4 (scene: chỉ hunk renderer mặt đất + `NATURE_GROUND`; không stage dòng `MainCamera` `m_LocalPosition.y` 2.2066925 → 3.2066925).

## Task 5 — `docs(art): record environment pass`
`Docs/Evidence/CampusGuardian/ART-ENVIRONMENT/README.md` gồm:
- Commit, ngày, môi trường (Windows Editor, độ phân giải).
- Bài học: biến `line` là từ khóa HLSL ⇒ sân lát hồng, phiên trước kẹt; cỏ chỉ dùng noise màu thì phẳng ⇒ thêm lớp chi tiết.
- Bảng ảnh trước/sau (ngày, đêm) theo các mốc: trước lượt, Task 2, Task 2b, Task 3, Task 4.
- Bảng FPS/draw calls qua từng mốc (dẫn link tsv).
- Danh sách renderer đổi vật liệu, object mới (`NATURE_GROUND`, `AMBIENT_PARTICLES`), số cụm cỏ, số mesh gộp.
- Ghi chú lệch kế hoạch: scene dùng skybox `Photographic campus sky` (chuyển tối qua `_Exposure`), `Cloud sky.mat` chỉ chỉnh dự phòng.
- Số test EditMode/PlayMode.
- Kèm `Docs/Prompts/Prompt_Environment_Pass_Resume.md` và prompt này vào commit Task 5.

## Git
- Stage chọn lọc kèm `.meta`. Không `git add -A`/`.`. Không push, không amend.
- Không stage `*.csproj`, `*.slnx`, `Logs/`, `ProjectSettings/`, `*.mat` Unity tự ghi ngoài `Materials/Environment`, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`, `Docs/Prompts/Prompt_VRoid_Player.md`, `Docs/superpowers/plans/2026-11-21-vroid-player.md`.

## Chống kẹt
- Một bước quá 20 phút không tiến triển (MCP timeout lặp, test treo, lỗi lặp sau 3 lần sửa) ⇒ dừng, báo kèm log.
- Trước khi chạy test: thoát Play Mode, chờ `data.compilation.is_compiling = false`.

## Báo cáo
- Danh sách commit; số test.
- Bảng renderer đã đổi; số cụm cỏ và mesh gộp.
- Bảng FPS/draw calls sảnh và toàn cảnh.
- Link ảnh trước/sau toàn cảnh ngày và đêm.
- Mọi chỗ lệch kế hoạch.
