# Prompt cho Codex — Lượt CLEANUP-GRAB: gỡ Grab, menu minigame, code minigame cũ

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus`.

## Chuẩn bị (người dùng đã làm trước khi gọi bạn)
Người dùng đóng Unity, chạy `git switch main`, `git pull`, `git switch -c minhnhat/remove-grab-minigames`, rồi mở lại Unity và bấm Start Server (MCP `unityMCP`, `http://127.0.0.1:8081/mcp`).
Kiểm `git branch --show-current` = `minhnhat/remove-grab-minigames`; sai ⇒ dừng, không tự chuyển nhánh khi Unity mở. MCP mất kết nối ⇒ dừng, nhờ người dùng Start Server.

## Đọc trước
1. `AGENTS.md`, `.agents/rules/*`.
2. Thiết kế đã duyệt: `Docs/superpowers/specs/2026-11-18-remove-grab-minigames-design.md`.
3. Kế hoạch: `Docs/superpowers/plans/2026-11-18-remove-grab-minigames.md` — làm đúng Task 0 → 4, mỗi Task một commit như ghi.

## Quyền và ranh giới
- Thịnh đã cho phép sửa phần của Thịnh; người dùng đã báo Thịnh không sửa scene campus trong lúc làm lượt này.
- Không đụng `Scripts/Guardian/Backend/**`, `IUHPlayerCustomizer`, `IUHInputHelper.VirtualVehiclePressed/IsVehicleKeyDown`; `IUHMobileControlsUI` chỉ sửa nếu không compile được.
- Không chạy builder cũ để dựng lại cảnh. Không tải asset ngoài.

## Git
- Stage chọn lọc kèm `.meta` (cả `.meta` của file/thư mục bị xóa). Không `git add -A`/`.`. Không stage `*.csproj`, `*.slnx`, `Logs/`, `ProjectSettings/`, `*.mat` Unity tự ghi, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`.
- Scene: chỉ stage hunk thuộc phạm vi; dòng Unity tự serialize khác (font `m_MinSize/m_MaxSize`…) tách ra bằng patch + `git apply --cached --recount`.
- Spec, plan, `Docs/superpowers/plans/2026-11-18-lo-trinh-chinh-sua.md` và file prompt này vào commit của Task 1. Không push, không amend.

## Dừng và báo khi
- GUID của script/asset sắp xóa còn được tham chiếu ngoài nhóm bị xóa.
- Phải đổi hành vi một hệ Guardian mới compile được.
- Lỗi do sửa của bạn quá 3 vòng.

## Báo cáo khi xong
Danh sách commit; số test trước/sau (+ assembly test đã xóa); danh sách file đã xóa và builder Editor đã xóa/sửa; quyết định về `VictoryDialog`/`DefeatDialog`; kết quả kiểm tay Task 4 (Pass/Fail); mọi chỗ lệch kế hoạch. Cập nhật `Docs/Evidence/CampusGuardian/CLEANUP-GRAB/README.md`.
