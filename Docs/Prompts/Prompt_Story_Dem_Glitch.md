# Prompt cho Codex — Lượt STORY: Cốt truyện "Đêm Glitch"

Bạn làm việc trong repo Unity `d:\IUH\CongNgheMoi\CK\IUH_Campus` (Unity 6000.5.8f1, URP). Trả lời bằng tiếng Việt.

## Đọc trước (bắt buộc, theo thứ tự)
1. `AGENTS.md` và `.agents/rules/coding-standards.md`, `unity-standards.md`, `match3-combat.md` — luật dự án luôn ưu tiên.
2. Thiết kế đã duyệt: `Docs/superpowers/specs/2026-11-16-dem-glitch-story-design.md`.
3. Kế hoạch thực hiện: `Docs/superpowers/plans/2026-11-16-dem-glitch-story.md` — **làm đúng theo kế hoạch này**, lần lượt Task 1 → Task 6, mỗi Task một commit như ghi trong kế hoạch.

## Việc cần làm
Thực hiện toàn bộ kế hoạch trên. Tên file, tên lớp, chữ ký hàm, lời thoại, tên object trong scene, giá trị số đã chốt trong kế hoạch — giữ nguyên, không đổi tên, không tự thêm tính năng.

## Luật git
- Người dùng đã tự tạo nhánh `minhnhat/story-dem-glitch` từ `origin/main` (lúc Unity đóng) rồi mới mở Unity. Đầu tiên kiểm `git branch --show-current` = `minhnhat/story-dem-glitch`; sai ⇒ dừng và báo, **không** tự chuyển nhánh khi Unity đang mở.
- Unity Editor đang mở và điều khiển qua MCP `unityMCP` (`http://127.0.0.1:8081/mcp`). Gặp "No Session"/không kết nối ⇒ dừng, nhờ người dùng bấm Start Server trong Window → MCP for Unity.
- Task 6 cần thắng/thua các trận match-3 thật: phần nào không tự chơi được qua MCP thì ghi rõ "cần người dùng chơi thử" thay vì ghi Pass.
- Stage chọn lọc từng file kèm `.meta`. **Không** `git add -A` / `git add .`. **Không** stage `*.csproj`, `*.slnx`, `Logs/`, `ProjectSettings/`, `UserSettings/`, `match3-combat-revamp.patch`, `Assets/Screenshots/`, và 3 material/scene đang báo modified do Unity tự serialize lại (trừ chính thay đổi `STORY_DIRECTOR` ở Task 5).
- Thêm `Docs/Prompts/Prompt_Story_Dem_Glitch.md` vào commit của Task 1.
- **Không push, không tạo PR.** Không force, không amend commit đã có.

## Ranh giới — tuyệt đối không
- Không sửa file `.cs` sẵn có nào (đặc biệt `Scripts/Combat/*`, `Scripts/Guardian/*.cs`, `Scripts/Guardian/AR/*`, `Scripts/UI/*`). Chỉ tạo file mới trong `Scripts/Guardian/Story/`, `Data/Story/`, `Materials/Story/`, `Docs/Evidence/CampusGuardian/STORY/`.
- Scene campus: chỉ thêm 1 GameObject gốc `STORY_DIRECTOR` ở Task 5. Không sửa/di chuyển/xóa object có sẵn.
- Không dùng `Shader.Find`; không `Find*`/`Camera.main` trong `Update`.
- Không chạy builder/automation cũ (`IUH_LEGACY_TOOLS`), không tải asset ngoài.

## Khi nào phải DỪNG và báo
- Lỗi do code mới của bạn: tự sửa tối đa 3 vòng; quá 3 vòng ⇒ dừng, báo kèm lỗi.
- Cần sửa file của người khác mới làm được, tên object trong scene khác với kế hoạch, diff scene ở Task 5 có thay đổi ngoài `STORY_DIRECTOR`, hoặc `origin/main` có commit mới sửa scene ⇒ dừng và báo, không tự quyết.

## Kiểm chứng cuối mỗi Task
- Unity compile xong, Console 0 error, Missing Script = 0.
- EditMode test của `IUHCampus.Guardian.Story.Tests` PASS (ghi số test).
- Chỉ ghi "Pass" khi đã chạy thật.

## Báo cáo khi xong
Gửi lại: danh sách commit (hash + message), kết quả test EditMode, bảng 11 mục kiểm tay của Task 6 (Pass/Fail + ghi chú), đường dẫn ảnh bằng chứng, và mọi chỗ bạn phải lệch khỏi kế hoạch (nếu có) kèm lý do.
