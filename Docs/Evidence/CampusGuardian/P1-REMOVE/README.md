# P1-REMOVE — gỡ hệ Guardian/Progress (P1/P1.5)

Ngày: 2026-11-09T21:57:37+07:00. Người làm: Minh Nhật (Claude Code). Nhánh `minhnhat/remove-progress-p1`, nền `6432dc0` (origin/main). Môi trường: Windows 11, Unity 6000.5.8f1 Editor + MCP.

Lý do: thống nhất một hệ tiền/kho đồ duy nhất là `IUHGuardianInventory` (Thịnh); xem `Docs/BAN_GIAO_CLAUDE_CODE.md` mục 2, 4.

Đã gỡ 88 file: toàn bộ file Assets do commit P1 `d57c60c` và P1.5 `43444ea` thêm (code, asmdef, test, 7 asset dữ liệu, prefab `GuardianInventoryUI`, 5 icon, scene `Scenes/Test/GuardianInventoryTest.unity`, kèm .meta và .meta thư mục cha trống) và `Docs/Guardian_Progress.md`. Danh sách + GUID: [files.json](files.json). Khôi phục được từ `43444ea`.

| ID | Ca kiểm thử | Mong đợi | Thực tế | Pass/Fail |
|---|---|---|---|---|
| R1 | Grep 50 GUID trên Assets/ProjectSettings/Packages | 0 tham chiếu ngoài bộ xóa | 59 file chứa GUID, cả 59 trong bộ xóa; 0 ngoài | Pass |
| R2 | Tên asmdef/kiểu C#/Build Settings | 0 tham chiếu ngoài | 0 | Pass |
| R3 | Compile sau refresh | Console 0 error | 0 error (chỉ warning CS0618 có sẵn) | Pass |
| R4 | Scene campus validate | Missing Script 0 | missingScripts 0, brokenPrefabs 0 | Pass |
| R5 | EditMode toàn bộ | Không regression do gỡ | 54/55 pass; 1 fail `ProcessStep_CountsPurifyTiles` có sẵn từ `e5aecbb`, không liên quan (xem [unity.json](unity.json)) | Pass (gỡ bỏ) / lỗi Match3 tồn tại riêng |

Không chạy PlayMode, không build, không kiểm thiết bị. Evidence P1/P15 giữ nguyên làm lịch sử.
