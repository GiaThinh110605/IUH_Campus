# Prompt cho Codex — Lượt GARDEN: vườn hoa

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus`.

## Chuẩn bị (người dùng đã làm)
Đã tạo nhánh `minhnhat/flower-garden` từ `origin/main`, mở Unity và bấm Start Server (MCP `unityMCP`, `http://127.0.0.1:8081/mcp`).
Kiểm `git branch --show-current`; sai nhánh ⇒ dừng, không tự đổi nhánh khi Unity mở. MCP mất kết nối ⇒ dừng, nhờ người dùng Start Server.

## Đọc trước
1. `AGENTS.md`, `.agents/rules/*`, skill liên quan trong `.agents/skills/` (uGUI, prefab, scene).
2. Thiết kế đã duyệt: `Docs/superpowers/specs/2026-11-19-flower-garden-design.md`.
3. Kế hoạch: `Docs/superpowers/plans/2026-11-19-flower-garden.md`. Làm đúng Task 0 → 4, mỗi Task một commit như ghi; viết test trước.

## Ranh giới
- Code mới nằm trong `Assets/IUH_Campus/Scripts/Garden/**` và `Assets/IUH_Campus/Prefabs/Garden/**`.
- `IUHInputHelper` chỉ được **thêm** `IsGardenInteractKeyDown()`.
- Không sửa `IUHCampusDailyActivitiesManager`, `IUHMobileControlsUI`, hệ cốt truyện, code quái, `IUHConceptPreviewController`, `Scripts/Guardian/Backend/**`.
- Scene: chỉ thêm object gốc `GARDEN`; không di chuyển hay sửa object khác.
- Không tải asset ngoài; không chạy builder cũ.

## Git
- Stage chọn lọc kèm `.meta`. Không `git add -A`/`.`.
- Không stage `*.csproj`, `*.slnx`, `Logs/`, `ProjectSettings/`, `*.mat` Unity tự ghi, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`.
- Scene chỉ stage hunk của `GARDEN`. Dòng `MainCamera` `m_LocalPosition.y` (2.2066925 → 3.2066925) hay tự xuất hiện: **không stage**.
- Spec, plan và file prompt này vào commit của Task 1. Không push, không amend.

## Dừng và báo khi
- Task 0 không tìm được chỗ đặt luống thỏa điều kiện khoảng cách với quái **và** cũng không có chỗ trống gần đó hợp lý.
- Phải sửa một hệ nằm trong danh sách "Không sửa" mới làm được (ví dụ chặn ESC, chặn phím E của quái).
- Lỗi do sửa của bạn quá 3 vòng.

## Báo cáo khi xong
- Danh sách commit; số test trước/sau.
- Tọa độ 6 luống và lý do chọn.
- Kết quả kiểm tay Task 4 (Pass/Fail từng mục), kèm đường dẫn ảnh.
- Mọi chỗ lệch kế hoạch; đã khôi phục PlayerPrefs chưa.
