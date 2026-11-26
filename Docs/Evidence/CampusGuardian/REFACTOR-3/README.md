# Refactor Đợt 3 — Combat Thời gian thực & Drone

Nhánh `minhnhat/refactor-readability` (tiếp nối Đợt 2 `88550bb`). Ngày 26/11/2026, Unity 6000.5.8f1 Windows Editor, URP. Thực hiện: Claude Code. Phạm vi: tách toàn bộ hàm > 50 dòng trong nhóm Combat thời gian thực và Drone, gom helper tạo vật liệu phát sáng, giảm lồng ghép; giữ nguyên 100% tên lớp, `[SerializeField]`, API public, sự kiện và chuỗi giao diện tiếng Việt.

| Mục | Commit |
|---|---|
| Tách hàm dài các script Combat (`IUHCampusCombatManager`, `IUHTimedGrenade`, `IUHCampusFireworksManager`, `IUHEmergencySupplyCrate`, `IUHPlayerHealth`, `IUHAerialFlightController`, `IUHMonsterAI`) | `2acae6e` |
| Tách điều khiển bay, thả tiếp tế, bom EMP và trinh sát ảnh (`IUHGuardianDroneController`) | `3025e7d` |

## Thay đổi từng file

- `Assets/IUH_Campus/Scripts/Combat/IUHCampusCombatManager.cs` (881 → 843 dòng; điểm 34,74 → 19,51; hàm > 50 dòng: 5 → 0):
  - `Update` (103 dòng): tách `DecrementCooldowns`, `HandleCombatInputs`, `HandleAttackInput`.
  - `PerformMeleePunch` (58 dòng): tách `TryApplyMeleeHit`; gom tạo vật liệu phát sáng lặp 6 nơi vào `CreateEmissiveLitMaterial`.
  - `TryFireGun` (51 dòng): tách `ComputeAimTarget`.
  - `LaserBarrageRoutine` (51 dòng): tách `GetPrioritizedBarrageMonsters`, `FindClosestBarrageTarget`.
  - `DroneStrikeRoutine` (68 dòng): tách `CreateStrikeBeacon`, `CreateStrikeExplosionSphere`, `DamageMonstersInRadius`.
- `Assets/IUH_Campus/Scripts/Guardian/IUHGuardianDroneController.cs` (886 → 869 dòng; điểm 33,85 → 16,50; lồng tối đa 4 → 2; hàm > 50 dòng: 5 → 0):
  - `SetControlled` (71 dòng): tách `EnableDroneControl`, `DisableDroneControl`.
  - `HandleFlightInput` (77 dòng): tách `ReadFlightAxes`, `UpdateDroneOrientation`, `ApplyDroneMotion`, `UpdateRotorFeedback`.
  - `AirdropSupplyRoutine` (63 dòng): tách `SpawnAirdropCrate`, `DeliverAirdropReward`, `CreateLitMaterial`.
  - `EmpBombRoutine` (88 dòng): tách `SpawnEmpBomb`, `DetonateEmpBomb`.
  - `CapturePhotoRecon` (63 dòng): tách `ResolveReconTarget`.
- `Assets/IUH_Campus/Scripts/Combat/IUHTimedGrenade.cs` (471 → 459 dòng; điểm 22,67 → 13,63; hàm > 50 dòng: 3 → 0):
  - `BuildVisuals` (71 dòng): tách `BuildGrenadeMesh`, `BuildBeaconLight`, `BuildCountdownCanvas`, `CreateEmissiveLitMaterial`.
  - `SpawnExplosionVFX` (78 dòng): tách `SpawnExplosionFlashLight`, `CreateGlowSphere`, `CreateShockwaveRing`, `SpawnExplosionSparks`.
  - `AnimateExplosionVFX` (65 dòng): tách `InitSparkVelocities`, `UpdateExplosionFrame`.
- `Assets/IUH_Campus/Scripts/Combat/IUHCampusFireworksManager.cs` (454 → 449 dòng; điểm 14,11 → 8,10; hàm > 50 dòng: 2 → 0):
  - `FireworkSequenceRoutine` (55 dòng): tách `CreateFireworkRocket`, `CreateUnlitEmissiveMat`.
  - `SpawnBurstExplosion` (79 dòng): tách `CreateBurstFlash`, `CreateBurstPointLight`, `CreateBurstParticleSystem`.
- `Assets/IUH_Campus/Scripts/Combat/IUHEmergencySupplyCrate.cs` (251 → 242 dòng; điểm 10,24 → 7,11; hàm > 50 dòng: 1 → 0):
  - `BuildVisuals` (52 dòng): tách `CreateEmissiveMat`, `CreateCrossBar`.
- `Assets/IUH_Campus/Scripts/Combat/IUHPlayerHealth.cs` (307 → 295 dòng; điểm 7,62 → 4,58; hàm > 50 dòng: 1 → 0):
  - `TakeDamage` (87 dòng): tách `AbsorbWithShield`, `ApplyDamageFeedback`, `HandleZeroHealth`.
- `Assets/IUH_Campus/Scripts/Combat/IUHAerialFlightController.cs` (405 → 407 dòng; điểm 6,05 → 3,06; hàm > 50 dòng: 1 → 0):
  - `ProcessFlightMotion` (112 dòng): tách `ReadHorizontalFlightInput`, `IsFlightSprintHeld`, `ReadVerticalFlightSpeed`, `ApplyFlightRotationAndLanding`.
- `Assets/IUH_Campus/Scripts/Combat/IUHMonsterAI.cs` (165 → 159 dòng; điểm 4,85 → 1,83; hàm > 50 dòng: 1 → 0):
  - `Update` (70 dòng): tách `FacePlayer`, `ChasePlayer`, `TryMeleeAttack`.

## Kiểm thử

- Unity compile (`refresh_unity`): sạch, Console **0 error**.
- Unity `EditMode`: **150/150 passed**.
- Unity `PlayMode`: **75/75 passed** (gồm `PlaytestSmokeTests`, `CombatWorldVfxPlayModeTests`, `PlaytestGroundFireBudgetTests`).
- Missing Script: **0** (2.967 GameObject trên `IUH_SharedCampus_Concept.unity`); scene `isDirty=false`.

## Chỗ lệch

1. Các hằng số VFX/chiến đấu nội bộ (tốc độ đạn, màu sắc, bán kính) vẫn giữ nguyên giá trị gốc tại chỗ để tránh làm khó đối chiếu khi chỉnh cân bằng nhanh; đợt này tập trung tách toàn bộ 18 hàm > 50 dòng về 0.
2. Chưa chơi thử thủ công lại toàn bộ các kỹ năng bắn súng, ném lựu đạn, bay phản lực và lái drone trên scene sau refactor (mới chạy qua EditMode/PlayMode tự động).
