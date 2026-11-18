# Gỡ Grab, menu minigame và code minigame cũ — Kế hoạch thực hiện

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Xóa sạch minigame Grab, menu minigame (phím M), code GreenCampus/PhotoHunter/flow minigame và hệ theme thương hiệu, trong khi mọi hệ Guardian vẫn chạy.

**Architecture:** Chuyển canvas đang chứa UI lõi Guardian ra object gốc mới `GUARDIAN_UI` trước, rồi mới xóa object/scene và code. Mỗi bước giữ project compile được và Missing Script = 0.

**Tech Stack:** Unity 6000.5.8f1, URP, uGUI, Unity MCP (`execute_code`, `run_tests`), git.

**Spec:** `Docs/superpowers/specs/2026-11-18-remove-grab-minigames-design.md`

## Global Constraints
- Nhánh `minhnhat/remove-grab-minigames` từ `origin/main`. Không commit vào `main`, không push khi chưa được yêu cầu, stage chọn lọc (không `git add -A`/`.`); không stage `*.csproj`, `*.slnx`, `Logs/`, `ProjectSettings/`, `*.mat` Unity tự ghi, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`.
- Thịnh đã cho phép sửa phần của Thịnh. Không đổi hành vi hệ còn giữ; chỉ cắt nhánh liên quan tới thứ bị xóa.
- Không đụng `Scripts/Guardian/Backend/**`, `IUHPlayerCustomizer`, `IUHMobileControlsUI` (trừ lỗi compile bắt buộc), `IUHInputHelper.VirtualVehiclePressed/IsVehicleKeyDown`.
- Scene: chỉ sửa đúng các object trong spec mục 5.1. Diff scene có thay đổi Unity tự serialize ngoài phạm vi (ví dụ dòng font `m_MinSize/m_MaxSize`) ⇒ không stage phần đó, tách bằng `git diff` → patch → `git apply --cached --recount`.
- Xóa asset luôn kèm `.meta`; trước khi xóa mỗi file, tìm GUID trong `Assets/**/*.{unity,prefab,asset,controller}` — còn tham chiếu ngoài nhóm bị xóa ⇒ dừng và báo.
- Mỗi Task kết thúc: compile xong, Console 0 error, Missing Script = 0 (3 scene + mọi prefab).
- Bằng chứng: `Docs/Evidence/CampusGuardian/CLEANUP-GRAB/README.md` (commit, ngày, môi trường, số test trước/sau).

## Review Focus
1. **UI lõi mất tham chiếu sau khi đổi cha:** kho đồ/match-3/shop dùng `transform.parent`/`Find` theo đường dẫn → Task 1 bước kiểm tay mở kho, shop, vào trận; quét code `Find("IUH_Minigame_Canvas` / `GAMEPLAY_GRAB_CAMPUS` trước khi đổi tên.
2. **Script còn giữ tìm object Grab bằng tên lúc chạy** (`GameObject.Find("IUH_Grab_Motorbike")`, `"PlayerArmature"`…) → Task 3 grep chuỗi tên object bị xóa trong code còn giữ.
3. **Asset ngoài scene còn GUID script bị xóa** (prefab, ScriptableObject) → Task 3 bước quét GUID.
4. **Test PlayMode/EditMode bị xóa theo minigame làm "Pass" ảo** → ghi số test trước/sau và tên các assembly test bị xóa.
5. **Cốt truyện/dư chấn hỏng do `IUHStoryWorldLock` bắt hụt component đã xóa** → Task 4 chơi thử F9 tới chương 1 và save Completed tới dư chấn đầu.

---

### Task 0: Đo trước
- [ ] Ghi vào README: số test EditMode/PlayMode hiện tại, Console, Missing Script, danh sách các assembly test.
- [ ] Grep code còn giữ (`Scripts/**` trừ thư mục sẽ xóa, `SharedCampusConcept/**`) các chuỗi `IUH_Minigame_Canvas`, `GAMEPLAY_GRAB_CAMPUS`, `DeliveryHUD`, `MinimapRadar`, `IUH_Grab_Motorbike` — ghi kết quả (Review Focus 1–2).

### Task 1: Chuyển UI lõi ra `GUARDIAN_UI`
**Files:** Modify scene `Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity`; code còn giữ nếu Task 0 thấy tìm theo tên/đường dẫn cũ.
- [ ] Tạo GameObject gốc `GUARDIAN_UI`; đặt `IUH_Minigame_Canvas` làm con (giữ world position/RectTransform), đổi tên thành `GUARDIAN_UI_CANVAS`. Code còn giữ tìm theo tên cũ ⇒ cập nhật tên.
- [ ] Lưu scene, compile, Play: mở kho, mở shop, vào và thắng/thua 1 trận match-3 (MCP `execute_code` gọi `TriggerCombat` được), có âm thanh combat. Console 0 error.
- [ ] Commit `refactor(scene): move Guardian UI canvas out of the Grab gameplay root` (kèm spec, plan, prompt).

### Task 2: Gỡ Grab khỏi scene
- [ ] Trên `GUARDIAN_UI_CANVAS`: gỡ component `IUHMiniGamePortalUI`, `IUHHorrorPursuitUI`. Xóa con `HorrorPursuitOverlay`, `DeliveryDialogueModal`, `DeliveryHUD` (cả `MinimapRadar`). `VictoryDialog`/`DefeatDialog`: xóa nếu không script còn giữ nào tham chiếu (kiểm bằng `SerializedObject` quét mọi component trong scene) — ghi quyết định.
- [ ] Xóa object gốc `GAMEPLAY_GRAB_CAMPUS`.
- [ ] Lưu scene; Missing Script = 0; Play: bấm M không có gì, kho/shop/match-3 như Task 1.
- [ ] Commit `refactor(scene): remove Grab gameplay and minigame menu objects`.

### Task 3: Xóa code + sửa chỗ gọi
- [ ] Xóa đúng danh sách spec mục 5.2 (kèm `.meta`, thư mục rỗng + `.meta` thư mục). Builder trong `Editor/`: grep tham chiếu lớp bị xóa; xóa builder một-lần đã cũ, sửa tối thiểu công cụ còn dùng (`IUHIOSCommandLineBuild`, `IUHARSceneBuilder`, `IUHARVisionTests`, `IUHBuildAutoExec` phải còn hoạt động) — liệt kê từng file và quyết định.
- [ ] `IUHMonsterEncounter`: bỏ `m_MotorbikeController`, `m_MotorbikeWasEnabled` và các nhánh dùng chúng. `CombatEntryTests`: bỏ phần tạo `IUHGameFlowManager`.
- [ ] Compile; sửa mọi lỗi còn lại bằng cách cắt đúng nhánh liên quan.
- [ ] Quét GUID của mọi `.cs` đã xóa trong `Assets/**/*.{unity,prefab,asset,controller}` ⇒ 0. Còn ⇒ xóa asset đó nếu chỉ phục vụ hệ bị xóa (liệt kê), ngược lại dừng và báo.
- [ ] Grep `Assets/IUH_Campus/Scripts` và scene: không còn `GrabCampus`, `XanhSM`, `ToCoToCo`, `IUHMiniGamePortalUI`, `IUHGameFlowManager`.
- [ ] Commit `refactor: delete Grab, GreenCampus, PhotoHunter and minigame flow code`.

### Task 4: Kiểm chứng toàn bộ
- [ ] EditMode + PlayMode toàn dự án Pass; ghi số trước/sau và các assembly test đã xóa.
- [ ] Missing Script = 0 ở `IUH_SharedCampus_Concept`, `IUH_CombatArena`, `IUH_Campus_AR_Encounter` và mọi prefab.
- [ ] Play (sao lưu và khôi phục save `IUH_Story_Progress`): F9 → camera giới thiệu → check-in tới biến cố → chương 1 vào/thắng 1 trận (được dùng `execute_code` giả thắng) → kho, shop mở được; đặt save `Completed` → dư chấn đầu xuất hiện (được rút `firstDelay` tạm, không commit); khinh khí cầu lên được; bấm M không có gì. Console 0 error.
- [ ] Ghi README bằng chứng; commit `docs(cleanup): record Grab removal verification`. Không push.
