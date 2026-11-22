# Prompt cho Codex — Lượt B2: thay người chơi bằng model VRoid

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus`.

## Chuẩn bị (người dùng đã làm)
- Đã tạo nhánh `minhnhat/vroid-player` từ `main` mới nhất (đã có lượt C).
- Đã mở Unity và bấm Start Server (MCP `unityMCP`).
- File nguồn: `D:\VRoid\model\IUH Student Player.vrm`. **Không dùng `main.vrm`.**
- Kiểm `git branch --show-current`. Sai nhánh ⇒ dừng. MCP mất kết nối ⇒ chờ, thử lại 1 lần, rồi dừng.
- Scene `isDirty=false` trước khi bắt đầu.

## Đọc trước
1. `AGENTS.md`, `.agents/rules/*`, skill `.agents/skills/free-asset-sourcing`.
2. `Docs/Evidence/CampusGuardian/CHARACTER/README.md` (cách đã thay Casual2, giữ `Animator` ở gốc).
3. Kế hoạch: `Docs/superpowers/plans/2026-11-21-vroid-player.md`. Làm Task 1 → 4, mỗi Task một commit như ghi.

## Git
- Stage chọn lọc kèm `.meta`. Không `git add -A`/`.`.
- Task 1 được stage `Packages/manifest.json`, `Packages/packages-lock.json` và **chỉ dòng Always Included Shaders** trong `ProjectSettings/GraphicsSettings.asset`. Các file `ProjectSettings/` khác không stage.
- Không stage `*.csproj`, `*.slnx`, `Logs/`, `*.mat` Unity tự ghi ngoài thư mục của lượt này, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`.
- Scene: chỉ stage hunk của Task đang làm. Dòng `MainCamera` `m_LocalPosition.y` (2.2066925 → 3.2066925) **không stage**.
- Kế hoạch và prompt này vào commit Task 1. Không push, không amend.

## Kiểm chứng mỗi Task
- EditMode và PlayMode **chạy tách riêng**, toàn bộ Pass. Console 0 error. Missing Script = 0 ở 3 scene.
- Task có scene: Play 30–60 s, đi/chạy/nhảy, 1 trận match-3 (Flee được), cửa hàng và kho đồ, vườn, chuyển đêm.
- Khôi phục PlayerPrefs nếu có đụng.

## Báo cáo khi xong
- Danh sách commit; phiên bản UniVRM; số test.
- Ảnh trước/sau (đứng, chạy, sau lưng, đêm, arena).
- FPS/draw calls/tris trước/sau.
- Mọi chỗ lệch kế hoạch, đặc biệt cách đặt `Vrm10Instance`/SpringBone và `Animator`.
