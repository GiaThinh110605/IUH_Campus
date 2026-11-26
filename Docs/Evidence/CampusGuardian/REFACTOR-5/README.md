# Bằng chứng Refactor — Đợt 5 (UI, Core & DailyActivities)

- **Ngày:** 2026-11-26
- **Nhánh:** `minhnhat/refactor-readability`
- **Môi trường:** Windows, Unity 6000.5.8f1 (URP), Unity MCP (`127.0.0.1:8081`)
- **Commit code:**
  - `2d21f3b` — `refactor(ui): split mobile controls auto-wiring, update and listeners`
  - `f5b1e39` — `refactor(activities): split album card UI, mascot dialog and sound synthesizer`

## Thay đổi chính (giữ nguyên toàn bộ API public, `[SerializeField]`, `PlayerPrefs`, text UI)
1. `Assets/IUH_Campus/Scripts/UI/IUHMobileControlsUI.cs`:
   - Tách `AutoWireReferences` (186 dòng) thành `WireJoystickAndLookZone`, `WireActionButtons`, `WireDroneFlightButtons`, `WireTopRightButtons`, `WireARButton`, `CleanupLegacyUtilityButtons`, `WirePlayerStatusWidgets` và helper `FindButton`.
   - Tách `Update` (133 dòng) thành `EnsurePlayerReferences`, `EnsureExplorerMovementEnabled`, `SyncMovementInput`, `SyncTouchLookInput`, `ProcessHeldFire`, `SyncTouchLookZoneVisibility`, `SyncDroneFlightModeUI`.
   - Tách `SetupButtonListeners` (278 dòng) thành `SetupGameplayActionListeners`, `SetupTopMenuListeners`, `SetupCombatButtonListeners`, `SetupLegacyUtilityListeners`, `SetupDroneFlightListeners` cùng helper `BindClick` và `BindHoldTrigger`.
   - Tách `SetupTouchLookEvents` (53 dòng) thành `OnTouchLookDown`, `OnTouchLookDrag`, `OnTouchLookUp`.
2. `Assets/IUH_Campus/Scripts/DailyActivities/IUHCampusActivitiesHUD.cs`:
   - Tách `CreateCardUI` (74 dòng) thành `CreateCardTitle`, `CreateCardPhotoBox`, `CreateCardDescription`.
3. `Assets/IUH_Campus/Scripts/DailyActivities/IUHMascotNPC.cs`:
   - Làm phẳng `Interact` (53 dòng, nesting 4) bằng early return và tách `ShowInProgressDialog`.
4. `Assets/IUH_Campus/Scripts/Core/IUHSoundSynthesizer.cs`:
   - Hợp nhất boilerplate tạo/cache `AudioClip` ở 21 hàm `Get*Sound()` vào helper `GetOrSynthesize` với hằng số `SampleRate = 44100` và `TwoPi`, giảm từ 551 dòng xuống 310 dòng.

## Chỉ số sau Đợt 5 (`scan_readability.py`)
- `IUHMobileControlsUI.cs`: 913 dòng, 43 hàm, 0 hàm >50 dòng, max nesting 2, score `5.14`.
- `IUHCampusActivitiesHUD.cs`: 449 dòng, 21 hàm, 0 hàm >50 dòng, max nesting 3, score `6.70`.
- `IUHMascotNPC.cs`: 199 dòng, 9 hàm, 0 hàm >50 dòng, max nesting 2, score `1.66`.
- `IUHSoundSynthesizer.cs`: 310 dòng, 22 hàm, 0 hàm >50 dòng, max nesting 1, score `15.53`.

## Kết quả kiểm thử
- **Unity Compile & Refresh:** Pass (0 error trên Console).
- **Scene `IUH_SharedCampus_Concept.unity`:** `isDirty=false`, `GameObjects=2967 MissingScripts=0`.
- **EditMode Tests:** `150/150 Passed` (18.04s).
- **PlayMode Tests:** `75/75 Passed` (26.34s).
