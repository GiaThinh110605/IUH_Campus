# Vườn hoa — Kế hoạch thực hiện

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 6 luống hoa gần "Vườn Cây Tri Thức IUH": gieo (−10), tưới, bón (−10), hoa lớn Hạt → Mầm → Nở (60 s mỗi giai đoạn theo giờ thật, mỗi lần tưới đẩy một giai đoạn), hái được +30 hoặc +45 Vàng.

**Architecture:** Lõi C# thuần (`IUHCampus.Garden.Core`, không phụ thuộc Unity) giữ luật và save, có test EditMode. `IUHGardenManager` nối lõi với Vàng, PlayerPrefs và giờ thật. `IUHGardenBedView` hiển thị từng luống. `IUHGardenPanelUI` là bảng chung, mở bằng E. Scene thêm duy nhất object gốc `GARDEN`.

**Tech Stack:** Unity 6000.5.8f1, URP, C#, uGUI, Unity Test Framework, Unity MCP.

**Spec:** `Docs/superpowers/specs/2026-11-19-flower-garden-design.md`

## Global Constraints
- Nhánh `minhnhat/flower-garden` từ `origin/main`. Không commit vào `main`, không push, không amend.
- Stage chọn lọc kèm `.meta`. Không stage `*.csproj`, `*.slnx`, `Logs/`, `ProjectSettings/`, `*.mat` Unity tự ghi, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`. Scene chỉ stage hunk của `GARDEN`; dòng ngoài phạm vi (ví dụ `MainCamera` `m_LocalPosition.y` 2.2066925 → 3.2066925, font `m_MinSize/m_MaxSize`) tách bằng patch + `git apply --cached --recount`.
- Số đúng spec: `SeedCost = 10`, `FertilizerCost = 10`, `HarvestGold = 30`, `FertilizedHarvestGold = 45`, `StageSeconds = 60`, `BedCount = 6`. PlayerPrefs key `IUH_Garden_v1`.
- Quên tưới ⇒ chỉ dừng lớn; không héo, không mất gì.
- Không sửa `IUHCampusDailyActivitiesManager`, `IUHMobileControlsUI`, hệ cốt truyện, code quái, `IUHConceptPreviewController`. `IUHInputHelper` chỉ được **thêm** `IsGardenInteractKeyDown()`.
- Không tải asset ngoài; dùng `Planting soil.mat`/`M_IUH_Planter_Soil.mat`, mesh `Trimmed hedge foliage`, vật liệu `Pink flowers`/`White flowers`/`Golden flowers`/`M_IUH_Flower_Orange`.
- Không chạy builder cũ. Scene chỉ thêm `GARDEN`.
- Mỗi Task: compile xong, Console 0 error, Missing Script = 0, test liên quan Pass.

## Review Focus
1. **Phím E/nút Tương tác bị hai hệ cùng ăn**: `VirtualInteractPressed` bị đọc rồi xóa, nên nếu quái đọc trước thì vườn mất lượt bấm (và ngược lại). Đặt luống ngoài vùng quái (Task 0 đo khoảng cách) là cách chặn. Task 3 kiểm tay: bấm nút Tương tác giả lập cạnh luống chỉ mở bảng vườn.
2. **Mở bảng khi đang thoại cốt truyện hoặc đang combat** ⇒ không mở. PlayMode test ở Task 3 (`PanelDoesNotOpenDuringCombat`).
3. **Đóng/mở Play liên tục** (save ghi giữa chừng) ⇒ không mất luống. Lưu ngay sau mỗi hành động; test ở Task 2 (`ReloadKeepsBeds`).
4. **Đứng giữa hai luống** ⇒ chỉ luống gần nhất được chọn, bảng ghi đúng số luống. Test ở Task 3 (`NearestBedIsSelected`).
5. **Bảng che khung thoại hoặc HUD trên màn iPhone tai thỏ** ⇒ nằm trong safe area, không đè joystick. Kiểm trên Device Simulator ở Task 4 nếu kịp; nếu không thì ghi "chờ Thịnh".

---

### Task 0: Đo trước và chọn chỗ
- [ ] Kiểm nhánh `minhnhat/flower-garden`, scene `isDirty=false`. Ghi số test EditMode/PlayMode vào `Docs/Evidence/CampusGuardian/GARDEN/README.md`.
- [ ] Bằng `execute_code`, ghi vào README:
  - Vị trí `IUHCheckInSpot` có `m_SpotId == "SPOT_GARDEN"`.
  - Mọi `IUHMonsterEncounter` (tên, vị trí, bán kính tương tác hoặc gần nhất của nó).
  - Vị trí khinh khí cầu và linh vật.
- [ ] Chọn vùng đặt 6 luống thỏa spec 5.3: trong khoảng 10 m quanh `SPOT_GARDEN`, cách mỗi quái ít nhất bán kính + 3 m, cách khinh khí cầu và linh vật hơn 4 m, có mặt đất (raycast xuống). Ghi tọa độ 6 luống. Không có chỗ hợp lệ ⇒ chọn chỗ trống gần nhất và ghi lý do.

### Task 1: Lõi vườn (C# thuần)

**Files:**
- Create: `Assets/IUH_Campus/Scripts/Garden/Core/IUHCampus.Garden.Core.asmdef` (`rootNamespace` `IUHCampus.Garden`, `noEngineReferences: true`)
- Create: `Assets/IUH_Campus/Scripts/Garden/Core/GardenBed.cs` (gồm `GardenStage`, `GardenRules`, `GardenBed`)
- Create: `Assets/IUH_Campus/Scripts/Garden/Core/GardenSave.cs`
- Test: `Assets/IUH_Campus/Scripts/Garden/Tests/Editor/IUHCampus.Garden.Tests.asmdef` (Editor, `TestAssemblies`, tham chiếu `IUHCampus.Garden.Core`; theo mẫu `IUHCampus.Guardian.Story.Tests`), `GardenBedTests.cs`, `GardenSaveTests.cs`

**Interfaces (namespace `IUHCampus.Garden`):**
- Produces:
  - `enum GardenStage { Empty, Seed, Sprout, Bloom }`
  - `static class GardenRules`: các hằng ở Global Constraints.
  - `sealed class GardenBed`:
    - Thuộc tính: `GardenStage Stage { get; }`, `long WateredAtUtcTicks { get; }` (0 = chưa tưới), `bool Fertilized { get; }`, `bool IsWatered => WateredAtUtcTicks != 0`.
    - `GardenBed()` tạo luống Trống; `GardenBed(GardenStage stage, long wateredAtUtcTicks, bool fertilized)` để load.
    - Hành động: `bool CanPlant()`, `bool Plant()`, `bool CanWater()`, `bool Water(DateTime nowUtc)`, `bool CanFertilize()`, `bool Fertilize()`, `bool CanHarvest()`, `bool Harvest(out int gold)`.
    - Thời gian: `bool Advance(DateTime nowUtc)` (true nếu đổi giai đoạn), `int? SecondsRemaining(DateTime nowUtc)`.
    - Không hàm nào đụng Vàng.
  - `static class GardenSave`: `string Serialize(IReadOnlyList<GardenBed> beds)`, `List<GardenBed> Deserialize(string data)`. Kết quả luôn có đúng `GardenRules.BedCount` luống.

- [ ] **Step 1: Viết test lỗi** (`t0 = new DateTime(2026,11,19,8,0,0,DateTimeKind.Utc)`):
  - `FullCycle_PlantWaterGrowHarvest`: `Plant()` ⇒ Seed. `Water(t0)`. `Advance(t0+59s)` false. `Advance(t0+60s)` true ⇒ Sprout, `IsWatered` false. `Water(t1)`, `Advance(t1+60s)` ⇒ Bloom. `Harvest(out g)` true, `g == 30`, Stage Empty, `Fertilized` false.
  - `FertilizedHarvestGives45`: bón ở Seed rồi đủ vòng ⇒ `g == 45`.
  - `NoWater_NoGrowth`: Seed chưa tưới, `Advance(t0+1h)` false, vẫn Seed, `SecondsRemaining` null.
  - `OneWateringAdvancesOneStage`: `Water(t0)`, `Advance(t0+1h)` ⇒ Sprout (không phải Bloom); gọi `Advance` lần nữa vẫn Sprout.
  - `CannotWaterTwiceInStage`: `Water` lần 2 cùng giai đoạn trả false, mốc thời gian không đổi.
  - `CannotWaterEmptyOrBloom`, `CannotPlantNonEmpty`, `CannotHarvestBeforeBloom`.
  - `HarvestTwice_OnlyOnce`: lần 2 trả false, `g == 0`.
  - `FertilizeOnlyOnce`: lần 2 trả false; `CannotFertilizeEmpty`.
  - `ClockRollback_NoGrowthNoThrow`: `Water(t0)`, `Advance(t0−10min)` false, Stage không đổi, `SecondsRemaining == 60`.
  - `SecondsRemaining_CountsDown`: `Water(t0)` ⇒ `SecondsRemaining(t0+18s) == 42`.
  - `Save_RoundTrip`: 6 luống đủ trạng thái (Empty, Seed tưới, Sprout bón, Bloom…) khứ hồi bằng nhau.
  - `Deserialize_Garbage_ReturnsSixEmpty`: các chuỗi `null`, `""`, `"abc"`, `"v9|..."`, chuỗi chỉ 3 luống ⇒ 6 luống Empty, không ném lỗi.
- [ ] **Step 2: Chạy, xác nhận Fail** (MCP `run_tests` EditMode, assembly `IUHCampus.Garden.Tests`): lỗi compile vì chưa có kiểu.
- [ ] **Step 3: Cài đặt** `GardenBed` và `GardenSave` theo Interfaces. Giờ máy lùi: giây trôi tính bằng `max(0, now − wateredAt)`.
- [ ] **Step 4: Chạy lại, xác nhận Pass.**
- [ ] **Step 5: Commit** `feat(garden): add pure garden rules and save with tests` (kèm spec, plan, file prompt).

### Task 2: Manager nối Vàng, save và giờ thật

**Files:**
- Create: `Assets/IUH_Campus/Scripts/Garden/IUHGardenManager.cs` (Assembly-CSharp, namespace `IUHCampus.Garden`)
- Modify: `Assets/IUH_Campus/Scripts/Core/IUHInputHelper.cs` (chỉ **thêm** hàm)
- Test: `Assets/IUH_Campus/Scripts/Garden/Tests/PlayMode/IUHCampus.Garden.PlayModeTests.asmdef` (`TestAssemblies`, tham chiếu `IUHCampus.Garden.Core`; gọi Assembly-CSharp qua reflection như `IUHCampus.Guardian.Match3.PlayModeTests`), `GardenManagerTests.cs`

**Interfaces:**
- Consumes: Task 1; `IUHGuardianInventory.Instance`, `TrySpendGold(int)`, `AddGold(int)`, `Gold`.
- Produces:
  - `IUHGardenManager : MonoBehaviour`:
    - `static IUHGardenManager Instance`, `Func<DateTime> Clock` (mặc định `() => DateTime.UtcNow`).
    - `GardenBed GetBed(int index)`, `int BedCount`.
    - `bool TryPlant(int i)`, `bool TryWater(int i)`, `bool TryFertilize(int i)`, `bool TryHarvest(int i)`.
    - `event Action<int> OnBedChanged`.
    - `void Load()`, `void Save()`.
    - Gọi `Advance` cho mọi luống khi `Awake`/`Load` và mỗi 1 giây (`InvokeRepeating` hoặc bộ đếm, không mỗi frame). Đổi giai đoạn ⇒ `OnBedChanged` + `Save`.
    - Thứ tự khi tốn Vàng: `bed.CanX()` → `inventory.TrySpendGold(cost)` → `bed.X()` → `Save()` → sự kiện. Không có kho thì gieo/bón trả false; tưới và hái vẫn làm được (hái không có kho thì không cộng Vàng, ghi warning).
  - `IUHInputHelper.IsGardenInteractKeyDown()`: true nếu `VirtualInteractPressed` (rồi xóa cờ) hoặc phím **E** vừa bấm (Input System + legacy như hàm cũ). **Không** nhận Space.

- [ ] **Step 1: Viết test lỗi.** Sao lưu và khôi phục PlayerPrefs `IUH_Garden_v1`, `IUH_Guardian_*` và `IUHGuardianInventory.Instance` theo mẫu `RepeatedCheckInAwards35GoldOnlyOnce`.
  - `PlantWithoutGold_NoChange`: Vàng 5 ⇒ `TryPlant(0)` false, Stage Empty, Vàng vẫn 5.
  - `PlantSpends10_HarvestAdds30`: Vàng 100. Gieo ⇒ 90. Đặt `Clock` giả, tưới, +60 s, gọi tick, tưới, +60 s, tick, hái ⇒ 120.
  - `FertilizeSpends10_HarvestAdds45`.
  - `ReloadKeepsBeds`: gieo, tưới, hủy manager, tạo lại với `Clock` +5 phút ⇒ luống 0 là Sprout chưa tưới.
  - `NoInventory_PlantFails_HarvestStillClears`.
  - `GardenKey_IgnoresSpace` (EditMode hoặc PlayMode, tùy làm được): `VirtualInteractPressed = true` ⇒ true một lần rồi false.
- [ ] **Step 2: Chạy, xác nhận Fail.**
- [ ] **Step 3: Cài đặt** theo Interfaces.
- [ ] **Step 4: Chạy lại, xác nhận Pass**; chạy toàn bộ EditMode + PlayMode Pass.
- [ ] **Step 5: Commit** `feat(garden): add garden manager with gold, save and real-time growth`.

### Task 3: Luống, bảng UI và scene

**Files:**
- Create: `Assets/IUH_Campus/Scripts/Garden/IUHGardenBedView.cs`, `Assets/IUH_Campus/Scripts/Garden/IUHGardenPanelUI.cs`
- Create: `Assets/IUH_Campus/Prefabs/Garden/IUH_GardenBed.prefab` (+ thư mục `.meta`)
- Modify: `Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity` (chỉ thêm `GARDEN`)
- Test: `GardenManagerTests.cs` hoặc `GardenPanelTests.cs` trong cùng assembly PlayMode

**Interfaces:**
- Consumes: `IUHGardenManager` (Task 2), `IUHInputHelper.IsGardenInteractKeyDown()`, `IUHGreenBillboard` (`Scripts/Core`), `Match3CombatManager.Instance.BlocksExplorationInput`, `IUHConceptPreviewController.IsAnyUIOpen()`, khung thoại cốt truyện (`IUHStoryDialogueUI`; dùng thuộc tính "đang bận/đang hiện" sẵn có, không sửa file đó).
- Produces:
  - `IUHGardenBedView : MonoBehaviour`:
    - `[SerializeField] int m_BedIndex`; các con `m_SeedVisual`, `m_SproutVisual`, `m_BloomVisual`; `Renderer m_Soil` + hai vật liệu đất khô/ướt; `m_FertilizedFx`.
    - `void Refresh(GardenBed bed, DateTime nowUtc)`.
    - Nhãn billboard: "[E] Chăm hoa" khi người chơi trong 2 m; ngoài ra "Cần tưới", "m:ss", "Hái được!", hoặc ẩn khi Trống.
  - `IUHGardenPanelUI : MonoBehaviour`:
    - `int SelectedBed` (−1 khi đóng), `bool IsOpen`.
    - `static int FindNearestBed(Vector3 playerPos, IReadOnlyList<Vector3> beds, float maxDist)` trả −1 nếu không có.
    - `bool CanOpenNow()` gộp các điều kiện chặn.
    - Nút gọi `TryPlant/TryWater/TryFertilize/TryHarvest`. Chỉ hiện nút hợp lệ. Thiếu Vàng ⇒ nút khóa, chữ "Cần 10 Vàng". Chữ khác theo phác bảng spec 5.2. Có `ApplySafeArea` như `IUHStoryDialogueUI`.

- [ ] **Step 1: Viết test lỗi**
  - `NearestBedIsSelected`: player (0,0,0), luống (1,0,0) và (1.5,0,0) ⇒ 0; player (5,0,0) với maxDist 2 ⇒ −1.
  - `PanelDoesNotOpenDuringCombat`: tạo `Match3CombatManager` ở trạng thái `BlocksExplorationInput` (theo cách `CombatEntryTests`) ⇒ `CanOpenNow()` false.
- [ ] **Step 2: Chạy, xác nhận Fail.**
- [ ] **Step 3: Cài đặt** hai script.
- [ ] **Step 4: Dựng prefab** theo spec 5.3: khung đất, 3 con theo giai đoạn, collider, `IUHGardenBedView`. Mỗi bản trong scene một màu hoa. Không tạo `.mat` mới nếu dùng lại được vật liệu có sẵn; nếu cần biến thể đất ướt thì tạo 1 vật liệu mới trong `Assets/IUH_Campus/Materials/Garden/`.
- [ ] **Step 5: Dựng scene:** object gốc `GARDEN` có `IUHGardenManager`, `IUHGardenPanelUI`, 6 bản prefab ở tọa độ Task 0, `m_BedIndex` 0–5. Lưu scene; Missing Script = 0.
- [ ] **Step 6: Chạy test, xác nhận Pass**; chạy toàn bộ EditMode + PlayMode Pass.
- [ ] **Step 7: Commit** `feat(garden): add flower beds, garden panel and GARDEN scene root` (chỉ hunk `GARDEN` của scene).

### Task 4: Kiểm chứng và bằng chứng
- [ ] Missing Script = 0 ở `IUH_SharedCampus_Concept`, `IUH_CombatArena`, `IUH_Campus_AR_Encounter` và mọi prefab; Console 0 error.
- [ ] Chơi thử trên PC (sao lưu/khôi phục `IUH_Garden_v1`, `IUH_Guardian_*`, `IUH_Story_Progress`; tua giờ bằng `execute_code` đặt `IUHGardenManager.Instance.Clock`):
  - [ ] Gieo, tưới, bón, hái đủ vòng ở 2 luống; Vàng đổi đúng −10/−10/+45 và −10/+30.
  - [ ] Thiếu Vàng: nút khóa, ghi "Cần 10 Vàng".
  - [ ] Không tưới thì tua 10 phút vẫn đứng yên.
  - [ ] Space (nhảy) cạnh luống không mở bảng; E mở; đi xa 3 m tự đóng.
  - [ ] Trong combat bấm E không mở bảng.
  - [ ] Bấm E ở luống không vào trận quái và không lên khinh khí cầu.
  - [ ] Tắt Play rồi bật lại: hoa còn đúng giai đoạn.
  - [ ] F9: vườn còn nguyên.
  - [ ] Chụp ảnh: Hạt, Mầm, Nở (đủ 4 màu) và bảng UI.
- [ ] Device Simulator iPhone có tai thỏ, xoay ngang: bảng nằm trong safe area, không đè joystick. Không kịp ⇒ ghi "chờ Thịnh thử trên máy".
- [ ] Ghi README: commit, ngày, môi trường, số test trước/sau, Pass/Fail từng mục, tọa độ luống, mọi chỗ lệch kế hoạch.
- [ ] Commit `docs(garden): record flower garden verification`. Không push.
