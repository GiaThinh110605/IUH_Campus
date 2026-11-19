# Thiết kế: Vườn hoa — hoạt động phụ chăm sóc khuôn viên

Ngày: 19/11/2026 · Người yêu cầu: Minh Nhật (Thịnh muốn thêm tính năng) · Trạng thái: đã duyệt 19/11/2026 (hướng A độc lập, quên tưới chỉ dừng lớn, giữ các con số)
Lượt 3/7 của `Docs/superpowers/plans/2026-11-18-lo-trinh-chinh-sua.md` (bản PC/laptop; iPhone dùng chung code).

## 1. Vì sao
- Tài liệu thiết kế chung (`Docs/Guardian_Thong_Nhat_Thiet_Ke_20261104.md`) có trụ cột cho bản PC: *"Chăm sóc khuôn viên: trồng hoa, tưới hoa, giữ cho khuôn viên tươi đẹp"*. Hiện trong game chưa có phần này.
- Thịnh muốn game có thêm tính năng.
- Vàng chơi lại được hiện chỉ có dư chấn (+30 mỗi 4–6 phút). Vườn cho thêm một nguồn Vàng thư giãn để nâng cấp (lượt 2).

## 2. Mục tiêu / Không làm / Chống mục tiêu
**Mục tiêu**
- 6 luống hoa gần điểm check-in "Vườn Cây Tri Thức IUH" (`SPOT_GARDEN`).
- Gieo hạt, tưới, bón phân, hái lấy Vàng. Hoa lớn qua Hạt → Mầm → Nở, 1 phút mỗi giai đoạn, **theo giờ thật**: vẫn lớn khi tắt game.
- Dùng được bằng bàn phím (E) và nút Tương tác trên điện thoại.

**Không làm**
- Không nối với quái, dư chấn hay cốt truyện (người dùng chọn hướng A: hoạt động độc lập).
- Không gửi sự kiện lên backend.
- Không thêm vật phẩm vào kho (hạt/phân trừ Vàng trực tiếp).
- Không tải asset ngoài; dựng từ asset có sẵn.
- Không sửa `IUHCampusDailyActivitiesManager`, `IUHMobileControlsUI`, hệ cốt truyện.

**Chống mục tiêu**
- Hoa bị mất, héo hoặc lùi giai đoạn vì người chơi không chăm (người dùng chọn "chỉ dừng lớn").
- Bấm E ở vườn đồng thời kích hoạt tương tác khác (khinh khí cầu, linh vật…), hoặc mở bảng vườn khi đang combat hay đang thoại cốt truyện.
- Vàng bị trừ mà không có hiệu quả, hoặc hái được nhiều lần một bông.
- Đổi giờ máy lùi lại làm hỏng save hoặc văng lỗi.
- Missing Script > 0, Console có error, hoặc vườn làm hệ khác chạy sai.

## 3. Các cách đã cân nhắc
| Cách | Đánh đổi |
|---|---|
| **A. Lõi C# thuần + luống trong scene + một bảng UI chung (ĐÃ CHỌN)** | Test được, giờ thật làm chuẩn, tách khỏi code đang chạy; nhiều file hơn |
| B. Mỗi luống là MonoBehaviour tự lo | Ít file; khó test, đếm bằng `Update` thì tắt game hoa ngừng lớn |
| C. Gắn vào `IUHCampusDailyActivitiesManager` | Dùng lại thông báo/album; file đã lớn, reset theo ngày, dễ phá săn xu/check-in/linh vật |

## 4. Luật chơi (người dùng đã chốt)
| Hành động | Điều kiện | Chi phí / thưởng | Kết quả |
|---|---|---|---|
| Gieo hạt | Luống **Trống** | −10 Vàng | **Hạt**, chưa tưới |
| Tưới | Hạt hoặc Mầm, **chưa tưới** ở giai đoạn này | miễn phí | Bắt đầu đếm 60 giây |
| (tự lớn) | Đã tưới và đủ 60 giây | — | Hạt → Mầm (chưa tưới) · Mầm → **Nở** |
| Bón phân | Hạt, Mầm hoặc Nở, chưa bón | −10 Vàng | Đánh dấu đã bón (1 lần mỗi bông) |
| Hái | **Nở** | +30 Vàng, hoặc +45 nếu đã bón | Luống về Trống |

- **Quên tưới:** hoa đứng yên ở giai đoạn hiện tại, không héo, không mất gì.
- **Một lần tưới chỉ đẩy một giai đoạn.** Tưới rồi tắt game 1 tiếng: mở lại thấy Hạt đã thành Mầm và chờ tưới tiếp (không nhảy thẳng tới Nở). Gieo → Nở cần 2 lần tưới và ít nhất 2 phút.
- **Không đủ Vàng:** nút hiện "Cần 10 Vàng" và bị khóa; không trừ gì.
- Nở rồi thì đứng yên tới khi hái, không có hạn.

## 5. Thiết kế
### 5.1 Lõi thuần C# — `Assets/IUH_Campus/Scripts/Garden/Core/`
asmdef `IUHCampus.Garden.Core` với `noEngineReferences: true` (giống `IUHCampus.Guardian.Story.Core`).
- `GardenStage` enum: `Empty`, `Seed`, `Sprout`, `Bloom`.
- `GardenBed` (một luống):
  - Trạng thái: `Stage`, `WateredAtUtcTicks` (0 = chưa tưới ở giai đoạn này), `Fertilized`.
  - Hành động, mỗi hàm trả `bool` (làm được hay không) và không tự trừ hay cộng Vàng: `Plant()`, `Water(nowUtc)`, `Fertilize()`, `Harvest(out int gold)`.
  - `Advance(nowUtc)`: nếu đã tưới và `nowUtc − WateredAt ≥ 60 s` thì lên một giai đoạn và xóa cờ tưới.
  - `SecondsRemaining(nowUtc)`: thời gian còn lại, hoặc null nếu chưa tưới.
  - Giờ máy lùi (`nowUtc < WateredAt`): coi như 0 giây đã trôi, không lỗi, không lùi giai đoạn.
- `GardenRules`: hằng số `SeedCost = 10`, `FertilizerCost = 10`, `HarvestGold = 30`, `FertilizedHarvestGold = 45`, `StageSeconds = 60`, `BedCount = 6`.
- `GardenSave`: `Serialize(IReadOnlyList<GardenBed>)` và `Deserialize(string)` theo dạng có phiên bản, ví dụ `v1|stage,ticks,fert;…`. Chuỗi hỏng, sai phiên bản hoặc sai số luống ⇒ trả 6 luống Trống, không ném lỗi.

### 5.2 Unity — `Assets/IUH_Campus/Scripts/Garden/`
- `IUHGardenManager` (một bản, trên object gốc `GARDEN`):
  - Giữ 6 `GardenBed`, đọc/ghi PlayerPrefs `IUH_Garden_v1`.
  - Mỗi giây (không cần mỗi frame) gọi `Advance(DateTime.UtcNow)` cho từng luống.
  - Nhận yêu cầu từ UI: thực hiện hành động, trừ/cộng Vàng qua `IUHGuardianInventory.TrySpendGold` / `AddGold`, lưu ngay sau mỗi hành động.
  - Thứ tự khi tốn Vàng: kiểm điều kiện trên luống trước, rồi `TrySpendGold`, rồi mới đổi trạng thái. Thiếu Vàng thì không đổi gì.
  - Nguồn giờ là `Func<DateTime>` (mặc định `DateTime.UtcNow`) để test và kiểm tay tua nhanh được.
  - Sự kiện `OnBedChanged(int index)`.
- `IUHGardenBedView` (mỗi luống, trên prefab):
  - Bật đúng mô hình theo giai đoạn.
  - Đất tối màu khi đã tưới; có hiệu ứng nhỏ (lấp lánh hoặc màu lá đậm) khi đã bón.
  - Nhãn nổi trên luống dùng `IUHGreenBillboard` (đã ở `Scripts/Core`): "[E] Chăm hoa", "Cần tưới", "0:42", "Hái được!".
- `IUHGardenPanelUI` (uGUI, một bảng dùng chung):
  - **Phím mở:** `IUHInputHelper.IsInteractKeyDown()` nhận cả **Space** (phím nhảy), nên vườn **không** dùng hàm đó. Thêm vào `IUHInputHelper` một hàm mới `IsGardenInteractKeyDown()`: chỉ nhận phím E, hoặc `VirtualInteractPressed` (nút Tương tác trên điện thoại, đọc rồi xóa cờ như hàm cũ). Không sửa các hàm sẵn có.
  - Mở khi người chơi đứng trong 2 m của **luống gần nhất**. Đóng bằng E, ESC, nút X, hoặc khi đi xa quá 3 m.
  - Không mở khi `Match3CombatManager.Instance.BlocksExplorationInput`, khi khung thoại cốt truyện đang hiện, hoặc khi `IUHConceptPreviewController.IsAnyUIOpen()`.
  - **Tránh chồng phím E:** người dùng phím E khác trên campus là quái (`IUHMonsterEncounter`, E để vào trận và tự vào trận khi đứng gần đủ lâu) và AR. Vì vậy luống đặt cách mọi `IUHMonsterEncounter` ít nhất bán kính tương tác của quái đó + 3 m (mục 5.3). Không sửa code quái.
  - ESC khi bảng vườn đang mở: đóng bảng và đánh dấu đã xử lý ESC theo cách `IUHConceptPreviewController` đang làm, để ESC không mở thêm menu tạm dừng. Nếu muốn làm vậy phải sửa `IUHConceptPreviewController`, thì chỉ đóng bằng E/X/đi xa và ghi lại.

Phác bảng:
```
┌──────── Luống hoa 3/6 ────────┐
│  🌱 Mầm  ·  Đã bón phân        │
│  Còn 0:42 để nở                │  (hoặc "Cần tưới để lớn tiếp")
│                                │
│ [ Tưới ]  [ Bón phân −10 ]     │  (chỉ hiện nút hợp lệ)
│ [ Hái +45 Vàng ]               │
│                     Vàng: 260  │
└──────────────────── [X] ───────┘
```

### 5.3 Prefab và scene
- Prefab `Assets/IUH_Campus/Prefabs/Garden/IUH_GardenBed.prefab`:
  - Khung luống dựng từ vật liệu đất trồng có sẵn (`Planting soil.mat` / `M_IUH_Planter_Soil.mat`).
  - 3 con theo giai đoạn: Hạt (ụ đất với vài hạt nhỏ), Mầm (lá xanh nhỏ, có thể dùng mesh `Trimmed hedge foliage` thu nhỏ), Nở (hoa dùng vật liệu `Pink flowers` / `White flowers` / `Golden flowers` / `M_IUH_Flower_Orange`, mỗi luống một màu).
  - Collider để đứng không xuyên qua.
- Scene campus:
  - Object gốc mới `GARDEN`, gồm `IUHGardenManager`, 6 bản prefab và bảng UI.
  - Đặt trong vòng khoảng 10 m quanh `SPOT_GARDEN`, trên mặt đất, không chắn lối đi.
  - Cách xa khinh khí cầu và linh vật hơn 4 m để không chồng vùng tương tác.
  - Cách mọi `IUHMonsterEncounter` (cả quái chương "Canteen & Vườn" lẫn quái dư chấn) ít nhất bán kính tương tác của quái đó + 3 m. Nếu vùng quanh `SPOT_GARDEN` không đủ chỗ, chọn chỗ trống gần nhất thỏa điều kiện và ghi lại tọa độ cùng lý do. Không di chuyển quái.
  - Bảng UI đặt trên `GUARDIAN_UI_CANVAS` hoặc canvas riêng dưới `GARDEN`. Phải nằm vùng an toàn trên iPhone (dùng cách `ApplySafeArea` như `IUHStoryDialogueUI`).
  - Chỉ thêm `GARDEN`, không sửa object khác.

### 5.4 Quan hệ với hệ khác
- F9 (reset cốt truyện) **không** xóa vườn.
- Vườn dùng được mọi lúc: trước, trong và sau cốt truyện, trừ khi đang combat hoặc đang thoại.
- `IUHStoryWorldLock` không khóa `GARDEN`.

## 6. Kiểm chứng
- EditMode (lõi):
  - Đủ vòng gieo → tưới → 60 s → Mầm → tưới → 60 s → Nở → hái 30/45.
  - Không tưới thì không lớn, dù trôi 1 giờ.
  - Một lần tưới chỉ lên một giai đoạn.
  - Không hái được khi chưa Nở; hái 2 lần chỉ được 1 lần.
  - Bón 2 lần chỉ nhận 1 lần.
  - Giờ lùi không lỗi và không lùi giai đoạn.
  - Save/Load khứ hồi; chuỗi hỏng thì ra 6 luống Trống.
- PlayMode:
  - Manager thiếu Vàng thì không đổi trạng thái và không trừ Vàng.
  - Đủ Vàng thì trừ 10 khi gieo; hái cộng đúng.
  - Tắt rồi mở lại manager với giờ giả +5 phút: hoa đã lớn đúng một giai đoạn.
- Toàn dự án EditMode/PlayMode Pass, Console 0 error, Missing Script = 0 ở 3 scene và mọi prefab.
- Chơi thử trên PC (được tua giờ qua `execute_code`):
  - Gieo, tưới, bón, hái đủ vòng ở 2 luống.
  - E không mở bảng khi đang combat; Space (nhảy) cạnh luống không mở bảng.
  - E ở vườn không kích hoạt khinh khí cầu hay linh vật.
  - Tắt Play rồi bật lại, hoa vẫn còn.
  - F9 không xóa vườn.
  - Ảnh chụp 3 giai đoạn và bảng UI.
- Kiểm bảng UI trên Device Simulator iPhone (cỡ màn hình có tai thỏ, xoay ngang) nếu kịp; nếu không thì ghi "chờ Thịnh thử trên máy".
- Bằng chứng: `Docs/Evidence/CampusGuardian/GARDEN/`.

## 8. Điều chỉnh 19/11/2026 sau khi người dùng chơi thử (thay cho các mục tương ứng ở trên)
- **4 luống** thay vì 6 (`BedCount = 4`), đặt đúng 4 bồn hoa trên bãi cỏ cạnh khu bàn ô dù Căn tin; bỏ 6 luống trước Căn tin. Save đổi lên `v2`; save `v1` cũ thành 4 luống trống.
- Đất phải sáng, đọc rõ cả ban đêm; luống ôm theo hình bồn; mở bảng khi đứng trong 2 m của bất kỳ điểm nào trên bồn.
- **Dựng lại bồn:** gỡ 4 bồn trang trí cũ. Hoa trong mesh gộp được xử lý bằng một bản sao mesh đã lọc; asset gốc không sửa. Thay bằng 4 bồn mới từ prefab vườn, đặt đúng chỗ cũ.
- Tạm dùng asset có sẵn. Prefab có 3 ô `Stage_Seed`/`Stage_Sprout`/`Stage_Bloom` để sau này thay mô hình mà không sửa code, và có hiệu ứng lớn dần khi đổi giai đoạn. Giữ nguyên cả vòng gieo, tưới, bón, hái.
- Quái: tầm E và tự vào trận còn 5 m (tầm 16 m chỉ để hiện thông tin), và chỉ đọc nút Tương tác khi người chơi đã trong tầm, để không nuốt nút của vườn trên điện thoại.
- Cân bằng (người dùng duyệt): máu quái ×0.6, sát thương quái ×0.7, thời gian trận +30 giây, áp chung trong `Match3CombatManager`.

## 7. Để người thực hiện quyết
- Hình dạng cụ thể của mô hình 3 giai đoạn và màu hoa từng luống, miễn dùng asset có sẵn và nhìn rõ khác nhau.
- Bảng UI đặt trên canvas nào; cỡ chữ và bố cục, miễn giống phong cách shop nâng cấp.
- Định dạng chuỗi save, miễn có phiên bản và chịu được chuỗi hỏng.
- Bố trí 6 luống (hàng 3×2 hay vòng cung) trong vùng mục 5.3.
