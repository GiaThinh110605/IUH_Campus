# Prompt cho Codex — Lượt C đồ họa: mặt đất + bầu trời, không khí

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus`.

## Chuẩn bị (người dùng đã làm)
- Đã tạo nhánh `minhnhat/environment-pass` từ `main` (đã có lượt nhân vật + bỏ đồ trang trí).
- Đã mở Unity và bấm Start Server (MCP `unityMCP`).
- Kiểm `git branch --show-current`. Sai nhánh ⇒ dừng. MCP mất kết nối ⇒ chờ, thử lại 1 lần, rồi dừng.
- Scene `isDirty=false` trước khi bắt đầu.

## Đọc trước
1. `AGENTS.md`, `.agents/rules/*`, skill `.agents/skills/urp-graphics-upgrade`.
2. `Docs/AmbientCG_Graphics_Upgrade.md` (vì sao lần trước thất bại).
3. Kế hoạch: `Docs/superpowers/plans/2026-11-21-environment-ground-sky.md`.
   - Làm Task 1 → 3 (Task 3 có thể làm song song với chờ duyệt Task 2).
   - **Dừng sau Task 2 + 3, gửi ảnh.**
   - Task 4–5 chỉ làm khi người dùng duyệt.

## Git
- Stage chọn lọc kèm `.meta`. Không `git add -A`/`.`.
- Không stage `*.csproj`, `*.slnx`, `Logs/`, `ProjectSettings/`, `*.mat` Unity tự ghi ngoài `Materials/Environment` và `Cloud sky.mat`, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`.
- Scene: chỉ stage hunk của Task đang làm. Dòng `MainCamera` `m_LocalPosition.y` (2.2066925 → 3.2066925) **không stage**.
- Kế hoạch và prompt này vào commit Task 1. Mỗi Task một commit. Không push, không amend.

## Kiểm chứng mỗi Task
- EditMode và PlayMode **chạy tách riêng**, toàn bộ Pass. Console 0 error. Missing Script = 0 ở 3 scene.
- Task có scene: Play 30–60 s; đi qua sân lát và bãi cỏ; mở vườn; E ở quái (8 m / 4 m); chuyển đêm (checkpoint biến cố) xem sương và trời.
- Khôi phục PlayerPrefs nếu có đụng.

## Báo cáo khi dừng
- Danh sách commit; số test.
- Ảnh trước/sau (ngày, đêm) các góc, cận cảnh sân lát có nhân vật.
- FPS/draw calls góc sảnh: mốc trước lượt, sau Task 2, sau Task 3.
- Mặt đất là mesh gộp hay nhiều renderer; renderer nào đã đổi vật liệu.
- Mọi chỗ lệch kế hoạch.
