# Prompt cho Codex — Lượt B: thay nhân vật người chơi, bỏ đồ trang trí

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus`.

## Chuẩn bị (người dùng đã làm)
- Đã tạo nhánh `minhnhat/character-model` từ `main` `319dbd8`.
- Đã mở Unity và bấm Start Server (MCP `unityMCP`).
- Thịnh đã đồng ý sửa scene và code cửa hàng.
- Kiểm `git branch --show-current`. Sai nhánh ⇒ dừng. MCP mất kết nối ⇒ chờ, thử lại 1 lần, rồi dừng.
- Scene `isDirty=false` trước khi bắt đầu.

## Đọc trước
1. `AGENTS.md`, `.agents/rules/*`, skill `.agents/skills/free-asset-sourcing`.
2. Kế hoạch: `Docs/superpowers/plans/2026-11-20-character-model.md`. Làm Task 1 → 4, mỗi Task một commit như ghi.

## Git
- Stage chọn lọc kèm `.meta`. Không `git add -A`/`.`.
- Không stage `*.csproj`, `*.slnx`, `Logs/`, `ProjectSettings/`, `*.mat` Unity tự ghi ngoài `Materials/Player`, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`.
- Scene: chỉ stage hunk của Task đang làm. Dòng `MainCamera` `m_LocalPosition.y` (2.2066925 → 3.2066925) **không stage**.
- Kế hoạch và prompt này vào commit Task 1. Không push, không amend.

## Kiểm chứng mỗi Task
- EditMode và PlayMode **chạy tách riêng**, toàn bộ Pass. Console 0 error. Missing Script = 0 ở 3 scene.
- Task có scene: Play 30–60 s, đi/chạy/nhảy, vào 1 trận match-3 (có thể dùng Flee), mở cửa hàng + kho đồ, mở vườn.
- Khôi phục PlayerPrefs nếu có đụng.

## Báo cáo khi xong
- Danh sách commit; số test.
- Ảnh trước/sau (đứng, chạy, sau lưng, arena, cửa hàng).
- FPS/draw calls trước/sau.
- Mọi chỗ lệch kế hoạch, đặc biệt cách giữ `Animator` ở gốc.
