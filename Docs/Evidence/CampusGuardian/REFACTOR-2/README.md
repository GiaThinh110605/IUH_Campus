# Refactor Đợt 2 — Match-3 Combat & Shop

Nhánh `minhnhat/refactor-readability` (tiếp nối Đợt 1 `599b6cc`). Ngày 26/11/2026, Unity 6000.5.8f1 Windows Editor, URP. Thực hiện: Claude Code. Phạm vi: tách hàm dài, gom lặp UI/audio, giảm lồng ghép và magic number; giữ nguyên 100% tên lớp, `[SerializeField]`, API public, sự kiện, key save và chuỗi giao diện tiếng Việt.

| Mục | Commit |
|---|---|
| Tách tìm match, mega-bomb, cascade và lọc component stage (`Match3BoardModel`, `Match3BoardView`, `Match3BattleStage`) | `0466d84` |
| Gom hàng nâng cấp shop, tách Update quái và tổng hợp audio (`IUHUpgradeShopUI`, `IUHMonsterEncounter`, `IUHCombatAudio`) | `19dfdaf` |

## Thay đổi từng file

- `Assets/IUH_Campus/Scripts/Guardian/Match3/Core/Match3BoardModel.cs` (366 → 357 dòng; điểm 15,72 → 3,19; lồng tối đa 7 → 4; hàm > 50 dòng: 2 → 0):
  - `FindMatches` (62 dòng): gom 2 vòng lặp ngang/dọc trùng lặp vào `FindLineMatches` + `GetAxisTile`.
  - `ProcessStep` (132 dòng): tách `ExpandMegaBombs`, `AddRadialBlast`, `AddColorBombBlast`, `TryAddBlastCoord`, `CollectMatchedResources`, `CollapseAndRefillColumns`; thêm `MinMatchLength = 3`, `MegaBombLength = 5`, `MaxBoardAttempts = 100` (magic 5 → 0).
- `Assets/IUH_Campus/Scripts/Guardian/Match3/Match3BoardView.cs` (343 → 352 dòng; điểm 6,64 → 1,17; lồng tối đa 4 → 3; hàm > 50 dòng: 1 → 0):
  - `ResolveCascadesRoutine` (76 dòng): tách `StartPopAnimations`, `TryPopTileAt` (bỏ lặp giữa `Coords` và `BlastCoords`), `StartDropAndSpawnAnimations`; thêm hằng `HighlightPadding`, `CascadeStepDelay`, `MegaBombShakeIntensity`, `MegaBombShakeDuration` (magic 5 → 0).
- `Assets/IUH_Campus/Scripts/Guardian/Match3/Match3BattleStage.cs` (533 → 553 dòng; điểm 5,58 → 3,44; lồng tối đa 4 → 3):
  - `StripToVisuals`: tách `CollectNonVisualComponents`, `CollectRequiredTypes`, `IsRequiredByAny` và `MaxStripPasses = 4`.
- `Assets/IUH_Campus/Scripts/Guardian/IUHUpgradeShopUI.cs` (379 → 370 dòng; điểm 12,76 → 3,13; lồng tối đa 4 → 2; hàm > 50 dòng: 2 → 0):
  - `RefreshAllUI` (66 dòng): gom 3 khối Kiếm/Khiên/Máu trùng lặp vào `RefreshUpgradeRow` với `MaxUpgradeLevel = 5`.
  - `RefreshInventoryTab` (79 dòng): tách `ClearContainer`, `CreateInventoryRow`, `CreateUseButton` (magic 35 → 19).
- `Assets/IUH_Campus/Scripts/Guardian/IUHMonsterEncounter.cs` (357 → 371 dòng; điểm 4,89 → 1,24; hàm > 50 dòng: 1 → 0):
  - `Update` (93 dòng): tách `UpdateActiveEncounter` và `UpdateExplorationPrompt`; gom các hằng cooldown/EMP (`InitialTriggerCooldown`, `DefeatTriggerCooldown`, `EmpHpDivisor`, `EmpMinMonsterHp`, `EmpShakeIntensity`, `EmpShakeDuration`, `EmpHpFillDenominator`; magic 7 → 0).
- `Assets/IUH_Campus/Scripts/Guardian/IUHCombatAudio.cs` (489 → 220 dòng; điểm 14,73 → 1,03):
  - Gom 13 hàm `Play*` lặp lại vào `PlayClip`, và gom 13 vòng lặp tổng hợp `AudioClip` vào `SynthesizeClip` với `SampleRate = 44100`, `TwoPi = 2.0f * Mathf.PI` (magic 131 → 3).
- Các file khác trong nhóm (`Match3CombatManager.cs`, `IUHGuardianInventory.cs`, `Match3TileView.cs`, `Match3CombatRules.cs`, `IUHCombatWorldVfx.cs`, `IUHCombatFloatingText.cs`, `Match3Types.cs`, `IUHDataSpin.cs`, `Match3ArenaAnchors.cs`) không có hàm > 50 dòng nên giữ nguyên.

## Kiểm thử

- Unity compile (`refresh_unity`): sạch, Console **0 error**.
- Unity `EditMode`: **150/150 passed** (gồm `Match3CombatTests`).
- Unity `PlayMode`: **75/75 passed** (gồm toàn bộ 10 bộ PlayMode của Match-3: `CombatEntryTests`, `CombatReentryTests`, `CombatOutcomeTests`, `VictoryRewardTests`, `UpgradeStatsTests`, `InventoryPersistenceTests`, `ArenaLifecycleTests`, `EncounterBalanceTests`, `EncounterRangeTests`, `UpgradeShopTabsTests`).
- Missing Script: **0** (kiểm trên 2.967 GameObject của `IUH_SharedCampus_Concept.unity`); scene `isDirty=false`.

## Chỗ lệch

1. `Match3CombatManager.cs` (743 dòng, 33 `[SerializeField]`, 0 hàm > 50 dòng) và các file ngắn khác trong `Guardian/Match3` chưa đụng tới để tránh rủi ro không cần thiết trên component gắn nhiều tham chiếu scene.
2. Chưa vào chơi thử thủ công lại toàn bộ trận Match-3 trên giao diện PC sau refactor (mới chạy qua bộ test tự động EditMode/PlayMode).
