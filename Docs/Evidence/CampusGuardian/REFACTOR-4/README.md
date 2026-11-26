# Refactor Đợt 4 — Guardian AR & Vision

Nhánh `minhnhat/refactor-readability` (tiếp nối Đợt 3 `bb7bbde`). Ngày 26/11/2026, Unity 6000.5.8f1 Windows Editor, URP. Thực hiện: Claude Code. Phạm vi: tách toàn bộ 20 hàm > 50 dòng trong module `Guardian/AR/`, gom helper dựng UI HUD/Modal và vật liệu Unlit, giảm lồng ghép; giữ nguyên 100% tên lớp, `[SerializeField]`, API public và chuỗi giao diện tiếng Việt.

| Mục | Commit |
|---|---|
| Tách phân tích thị giác, encounter AR và animation quái (`IUHARVisionAnalyzer`, `IUHARCorruptedObjectEncounter`, `IUHARMonsterLivingController`) | `f39119a` |
| Tách dựng HUD/WebCam AR và chiến đấu chém khối AR (`IUHLiveARController`, `IUHARRuneSlashCombat`) | `9e3058d` |

## Thay đổi từng file

- `Assets/IUH_Campus/Scripts/Guardian/AR/IUHARVisionAnalyzer.cs` (378 → 340 dòng; điểm 20,76 → 8,83; lồng tối đa 5 → 3; hàm > 50 dòng: 2 → 0):
  - `ExtractFeatures` (180 dòng): tách `ComputeLuma`, `AnalyzeGridPatches`, `SamplePatchStdDev`, `ComputeMicroEdgeFraction`, `ComputeScanLineMetrics`, `CountScanLineTransitions`.
  - `Analyze` (139 dòng): tách `MakeResult`, `AnalyzeLaptop`, `AnalyzeBackpack`, `AnalyzeBook`.
- `Assets/IUH_Campus/Scripts/Guardian/AR/IUHARCorruptedObjectEncounter.cs` (470 → 467 dòng; điểm 15,07 → 5,46; lồng tối đa 3 → 2; hàm > 50 dòng: 3 → 0):
  - `HandleEngagementInput` (62 dòng): tách `GetPlayerDistance`, `UpdateProximityTrigger`, `TryGetPointerTap`, `TryRaycastEncounterTap`.
  - `SpawnSequenceRoutine` (78 dòng): tách `AnimateObjectPopIn`, `WaitUntilPlayerAwakens`, `AnimateMonsterEmerge`.
  - `PurificationSequenceRoutine` (55 dòng): tách `FadeOutMonsterRoutine`, `FadeAuraLightRoutine`.
- `Assets/IUH_Campus/Scripts/Guardian/AR/IUHARMonsterLivingController.cs` (140 → 136 dòng; điểm 5,87 → 2,85; hàm > 50 dòng: 1 → 0):
  - `Update` (58 dòng): tách `FaceCamera`, `ApplyCombatOffsets`.
- `Assets/IUH_Campus/Scripts/Guardian/AR/IUHLiveARController.cs` (1.340 → 1.202 dòng; điểm 52,87 → 28,41; hàm > 50 dòng: 8 → 0):
  - `BuildARInfrastructure` (53 dòng): tách `CreateARCamera`, `CreateARBackgroundQuad`.
  - `Spawn3DMonsterAndCorruptedProps` (110 dòng): tách `SpawnMonsterVisual`, `SpawnCorruptedProps`.
  - `BuildAIDeepLearningHUD` (257 dòng): tách `CreateARHudCanvas`, `BuildHeaderPanel`, `BuildTargetTabsBar`, `BuildScannerReticle`, `BuildScanActionButtons`, `CreateARActionButton`, `CreateCenteredButtonLabel`.
  - `StartWebCamRoutine` (66 dòng): tách `FindPreferredCameraName`, `InitializeWebCamTexture`, `BindWebCamToQuad`.
  - `PerformRealCameraAnalysis` (53 dòng): tách `FailedAnalysis`.
  - `ManualScanProcessRoutine` (94 dòng): tách `AnimateScanProgressRoutine`, `HandleScanFailure`, `GetPropDisplayName`.
  - `OnObjectDetectedSuccessfully` (92 dòng): tách `PositionMonsterInFrontOfCamera`, `ActivateTargetProp`.
  - `PrepareNextObjectScan` (73 dòng): tách `UpdateTabButtonColors`, `ResetScanControlsForNextTarget`.
- `Assets/IUH_Campus/Scripts/Guardian/AR/IUHARRuneSlashCombat.cs` (1.391 → 1.320 dòng; điểm 54,04 → 35,80; hàm > 50 dòng: 6 → 0):
  - `BuildCombatUI` (196 dòng) & `BuildEndGameModals` (114 dòng): tách `CreateCombatCanvas`, `BuildBossHealthBar`, `BuildPlayerStatusBars`, `BuildHudOverlays`, `BuildUltimateCutLineUI`, `BuildVictoryModal`, `BuildDefeatModal`, cùng helper `CreatePanel`, `CreateFilledBar`, `CreateOutlinedText`, `CreateFeedbackButton`.
  - `ShapeTossingRoutine` (56 dòng) & `SpawnFlyingShape` (81 dòng): tách `SpawnShapeWaveRoutine`, `PickRandomShapeType`, `CreateShapeVisual`, `AttachBombSpikes`, `CreateUnlitMaterial`.
  - `HandleBladeSwipeInput` (53 dòng): tách `ReadPointerState`.
  - `SliceShape` (84 dòng): tách `HandleSpikeBombHit`, `ApplyValidSliceReward`.
- `IUHARPlaneReticle.cs` (173 dòng, 0 hàm > 50 dòng): giữ nguyên.

## Kiểm thử

- Unity compile (`refresh_unity`): sạch, Console **0 error**.
- Unity `EditMode`: **150/150 passed**.
- Unity `PlayMode`: **75/75 passed**.
- Missing Script: **0** (2.967 GameObject trên `IUH_SharedCampus_Concept.unity`); scene `isDirty=false`.

## Chỗ lệch

1. Các hằng số tọa độ/kích thước UI dựng runtime trong `IUHLiveARController` và `IUHARRuneSlashCombat` giữ nguyên tại chỗ để dễ tinh chỉnh bố cục trực tiếp; đợt này tập trung tách toàn bộ 20 hàm > 50 dòng về 0.
2. Chưa test trực tiếp luồng WebCam/Gyro/RuneSlash trên thiết bị iOS thật (mới chạy qua bộ EditMode/PlayMode trên Windows Editor).
