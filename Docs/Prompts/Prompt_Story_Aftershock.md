# Prompt cho Codex — Lượt STORY (tiếp): Dư chấn GLITCH + z-fighting + AABB

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus`, nhánh **`minhnhat/story-dem-glitch`** (kiểm `git branch --show-current`; sai ⇒ dừng, không tự chuyển nhánh khi Unity đang mở). Unity mở, MCP `unityMCP` (`http://127.0.0.1:8081/mcp`); mất kết nối ⇒ dừng, nhờ người dùng bấm Start Server.

## Đọc trước
1. `AGENTS.md`, `.agents/rules/*`.
2. Spec mục **10**: `Docs/superpowers/specs/2026-11-16-dem-glitch-story-design.md`.
3. Kế hoạch phần **"Bổ sung 17/11/2026"** (Task 7–10): `Docs/superpowers/plans/2026-11-16-dem-glitch-story.md`. Làm đúng thứ tự, mỗi Task một commit như ghi.

## Được phép / không được phép
- Được sửa (Thịnh đồng ý 17/11/2026): `IUHMonsterEncounter.cs`, `IUHARCorruptedObjectEncounter.cs` — **chỉ thêm** `ResetForAftershock()` (và ghi lại giá trị đèn gốc ở `Awake` nếu cần), không đổi hành vi khác. Scene campus: **chỉ** sửa z-fighting ở Task 10.
- Code cốt truyện trong `Scripts/Guardian/Story/`, `Data/Story/`: theo kế hoạch.
- Không sửa file khác của người khác. Nguồn lỗi AABB nằm ngoài phạm vi ⇒ dừng và báo.
- Phong cách code như các file Story hiện có (4 dấu cách, ngoặc xuống dòng, mỗi thành viên một khối).

## Git
- Stage chọn lọc kèm `.meta`; không `git add -A`/`.`; không stage `*.csproj`, `*.slnx`, `Logs/`, `ProjectSettings/`, các `*.mat` do Unity tự ghi, `Assets/Screenshots/`, `match3-combat-revamp.patch`, `.vsconfig`.
- Scene: stage riêng hunk z-fighting bằng `git diff` → lọc patch → `git apply --cached --recount` (6 dòng font giữ nguyên, không stage). Kiểm `git diff --cached` trước khi commit.
- Spec, plan đã sửa và file prompt này đi cùng commit của Task 7. Không push, không amend.

## Dừng và báo khi
- Lỗi do code mới quá 3 vòng sửa; kế hoạch mâu thuẫn với code thật; diff scene có thay đổi ngoài phạm vi; quá 10 cặp z-fighting (sửa khu kết truyện trước, báo phần còn lại).

## Báo cáo khi xong
Danh sách commit, số test EditMode/PlayMode/Backend, kết quả kiểm tay Task 8–9 (Pass/Fail + ghi chú), các cặp z-fighting đã sửa, nguồn lỗi AABB và cách sửa, mọi chỗ lệch kế hoạch. Cập nhật `Docs/Evidence/CampusGuardian/STORY/README.md`.
