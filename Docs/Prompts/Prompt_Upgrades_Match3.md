# Prompt cho Codex — Lượt UPGRADES-MATCH3: nâng cấp có tác dụng trong match-3

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus`.

## Chuẩn bị (người dùng đã làm)
Đã tạo nhánh `minhnhat/upgrades-match3` từ `origin/main` (`73d05da`), mở Unity và bấm Start Server (MCP `unityMCP`, `http://127.0.0.1:8081/mcp`).
Kiểm `git branch --show-current`; sai nhánh ⇒ dừng, không tự đổi nhánh khi Unity mở. MCP mất kết nối ⇒ dừng, nhờ người dùng Start Server.

## Đọc trước
1. `AGENTS.md`, `.agents/rules/*`.
2. Thiết kế đã duyệt: `Docs/superpowers/specs/2026-11-19-upgrades-affect-match3-design.md`.
3. Kế hoạch: `Docs/superpowers/plans/2026-11-19-upgrades-affect-match3.md`. Làm đúng Task 0 → 3, mỗi Task một commit như ghi; viết test trước.

## Ranh giới
- Thịnh đã cho phép sửa `IUHGuardianInventory` và code của Thịnh.
- Chỉ sửa `IUHGuardianInventory.cs`, `Match3CombatManager.cs`, test mới `UpgradeStatsTests.cs` (+ `.meta`) và README bằng chứng. Shop UI chỉ sửa nếu chữ bị tràn.
- Không sửa scene, không đổi giá/thưởng/chỉ số quái/skill.

## Git
- Stage chọn lọc kèm `.meta`. Không `git add -A`/`.`. Không stage `*.csproj`, `*.slnx`, `Logs/`, `ProjectSettings/`, `*.mat` Unity tự ghi, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`, scene chỉ đổi xuống dòng.
- Spec, plan và file prompt này vào commit của Task 1. Không push, không amend.

## Dừng và báo khi
- Có đường tạo `Match3CombatRules` mà bạn không chắc có nên áp chỉ số kho hay không.
- Phải đổi hành vi hệ khác (shop, thưởng, AR) mới làm được.
- Lỗi do sửa của bạn quá 3 vòng.

## Báo cáo khi xong
Danh sách commit; số test trước/sau; kết quả kiểm tay Task 3 (Pass/Fail từng mục); mọi chỗ lệch kế hoạch. Đã khôi phục PlayerPrefs chưa.
