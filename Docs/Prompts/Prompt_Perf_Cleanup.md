# Prompt cho Codex — Lượt 5: dọn hiệu năng

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus`.

## Chuẩn bị (người dùng đã làm)
- Đã tạo nhánh `minhnhat/perf-cleanup` từ `main` `599913b`.
- Đã mở Unity và bấm Start Server (MCP `unityMCP`).
- Kiểm `git branch --show-current`. Sai nhánh ⇒ dừng, không tự đổi nhánh khi Unity đang mở. MCP mất kết nối ⇒ dừng.

## Đọc trước
1. `AGENTS.md`, `.agents/rules/*`.
2. Kế hoạch (đã duyệt, gồm cả thiết kế): `Docs/superpowers/plans/2026-11-19-perf-cleanup.md`. Làm đúng Task 1 → 3, mỗi Task một commit như ghi; viết test trước.

## Ranh giới
- Chỉ sửa: `IUHMobileControlsUI`, `IUHMonsterEncounter`, `IUHMascotQuestItem`, `IUHEmergencySupplyCrate`, `IUHLootCoinPickup`. Thêm `Scripts/Core/IUHPlayerLocator.cs` và test.
- Không đổi hành vi, số liệu, scene, prefab. Không sửa `Guardian/AR/**`, Starter Assets, `Scripts/Guardian/Backend/**`.
- Bộ dò AABB chỉ chạy tạm bằng `execute_code`, không tạo file script, không commit.
- Không tải asset ngoài; không chạy builder cũ.

## Git
- Stage chọn lọc kèm `.meta`. Không `git add -A`/`.`.
- Không stage `*.csproj`, `*.slnx`, `Logs/`, `ProjectSettings/`, `*.mat` Unity tự ghi, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`. Không stage file scene (lượt này không sửa scene).
- Kế hoạch và file prompt này vào commit Task 1. Không push, không amend.

## Kiểm chứng cuối
- EditMode và PlayMode toàn bộ Pass, ghi số trước/sau.
- Console 0 error. Missing Script = 0 ở 3 scene.
- Chơi thử 5 phút trên campus, không lỗi mới:
  - Đi lại, nhặt xu.
  - E ở quái: 8 m không vào trận, 4 m vào trận.
  - Vườn mở được.
  - Nút trên giao diện điện thoại hoạt động (bật `m_ForceShowOnPC` tạm, không lưu scene).
- Khôi phục PlayerPrefs nếu có đụng.

## Dừng và báo khi
- AABB chỉ ra nguyên nhân nằm trong code AR, Starter Assets, scene hoặc asset bên ngoài.
- Muốn sửa ngoài 5 file trên.
- Lỗi do sửa của bạn quá 3 vòng.

## Báo cáo khi xong
- Danh sách commit; số test trước/sau.
- Bảng A–F đã sửa thế nào.
- Kết quả Profiler (hoặc lý do không đo được).
- Kết quả AABB: bắt được hay không, đã thử những gì.
