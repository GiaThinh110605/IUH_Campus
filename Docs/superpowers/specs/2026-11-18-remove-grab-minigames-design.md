# Thiết kế: Gỡ minigame Grab, menu minigame và code minigame cũ

Ngày: 18/11/2026 · Người yêu cầu: Minh Nhật · Trạng thái: đã duyệt 18/11/2026 (người dùng đồng ý bỏ minimap)
Lượt 1/7 của kế hoạch chỉnh sửa (phạm vi: bản PC/laptop; AR iOS là của Thịnh; Android là hướng phát triển).

## 1. Vì sao
- Minigame Grab hỏng (khu đường quanh x≈400 không còn mặt đất; xe đậu giữa khoảng không).
- Menu chọn minigame (phím M, `IUHMiniGamePortalUI`) trái với thiết kế chốt "không menu 6 game".
- Nhiều code minigame/flow cũ không còn trong scene nhưng vẫn nằm trong repo — giảng viên mở code thấy rác; hệ theme dùng thương hiệu thật (Grab, Be, XanhSM, tên quán thật).

## 2. Mục tiêu / Không làm / Chống mục tiêu
**Mục tiêu**
- Scene campus không còn object Grab, menu minigame, HUD giao hàng, overlay "horror pursuit".
- Repo không còn code Grab, GreenCampus, PhotoHunter, menu/flow minigame, theme thương hiệu.
- Mọi hệ Guardian chạy như trước: cốt truyện, dư chấn, match-3, kho đồ, shop nâng cấp, âm thanh combat, drone, khinh khí cầu, daily activities, backend reporter.

**Không làm**
- Không đổi gameplay Guardian, không đổi UI mobile (`IUHMobileControlsUI` thuộc Thịnh) ngoài sửa compile nếu bắt buộc.
- Không động vào thư viện Backend (`Scripts/Guardian/Backend/`, kể cả `GuardianScanClient/ScanService` chưa dùng — để Thịnh nối AR).
- Không đổi hệ mũ/hào quang `IUHPlayerCustomizer`.
- Không sửa tài liệu lịch sử (`Docs/GreenCampus_Completed.md`…); lượt hồ sơ cuối sẽ xử lý.

**Chống mục tiêu (xong mà thế này là hỏng)**
- Mất kho đồ, giao diện match-3, shop, âm thanh combat (chúng đang nằm dưới canvas của Grab).
- Missing Script > 0 ở bất kỳ scene/prefab nào, hoặc Console có error.
- Cốt truyện hoặc dư chấn không chạy hết.

## 3. Hiện trạng (đã kiểm 18/11/2026)
Scene `IUH_SharedCampus_Concept.unity`, object gốc `GAMEPLAY_GRAB_CAMPUS` (có `IUHCustomizationManager`):
- Thuần Grab: `IUH_Grab_Motorbike`, `GHOST_HAZARDS`, `DELIVERY_WAYPOINTS`, `NIGHT_FOOD_SHOPS`, `DELIVERY_DROPOFF_STATIONS`, `IUH_Nav_Arrow`, `GRAB_SURVIVAL_ITEMS`.
- `IUH_Minigame_Canvas` (có `IUHMiniGamePortalUI`, `IUHHorrorPursuitUI`) chứa:
  - **Phải giữ:** `IUH_Guardian_Inventory`, `IUH_Combat_Audio`, `CombatArena_Match3`, `IUH_Upgrade_Shop_UI`.
  - **Grab:** `HorrorPursuitOverlay`, `DeliveryDialogueModal`, `VictoryDialog`, `DefeatDialog` (kiểm lại ai dùng), `DeliveryHUD` gồm `PlayerProfileCard`, `BtnCustomizationHUD`, `TopRightBanner`, `MinimapRadar` (`IUHMinimapUI`), `BottomCard`.

Code ngoài thư mục Grab đang gọi Grab/menu: `IUHMonsterEncounter` (dừng xe khi vào trận), `IUHMinimapUI`, cùng nhiều script **không có trong scene nào**: `IUHGameFlowManager`, `GameManager`, `PlayerLoadout`, `IUHMainMenuUI`, `IUHCampusHubHUD`, `IUHCustomizationUI`, `IUHWorldLocationPin`, `IUHMiniGamePrepUI`, `IUHUniformManager`, `IUHInteractiveTourController`, `IUHGameHubManager`. Test `CombatEntryTests` tìm `IUHGameFlowManager` bằng tên. Nhiều script trong `Editor/` (builder cũ) tham chiếu các lớp sẽ xóa.

## 4. Các cách đã cân nhắc
| Cách | Đánh đổi |
|---|---|
| **A. Gỡ hẳn trong một PR, chia bước (ĐÃ CHỌN)** | Sạch thật; lượt lớn, đụng file của Thịnh (Thịnh đã cho phép sửa phần của Thịnh), phải chơi thử lại |
| B. Chỉ tắt/ẩn object và phím M | Nhanh, ít rủi ro; code chết và "menu 6 game" vẫn còn trong repo |
| C. Tắt trước, xóa code ở PR sau | An toàn nhất; tốn hai vòng review |

## 5. Thiết kế
### 5.1 Scene
1. Tạo object gốc mới **`GUARDIAN_UI`** và chuyển `IUH_Minigame_Canvas` ra làm con của nó (giữ nguyên Canvas, CanvasScaler, sorting), đổi tên canvas thành **`GUARDIAN_UI_CANVAS`**. Như vậy kho đồ, match-3, shop, âm thanh giữ nguyên tham chiếu và cách hiển thị.
2. Gỡ khỏi canvas: component `IUHMiniGamePortalUI`, `IUHHorrorPursuitUI`; các con `HorrorPursuitOverlay`, `DeliveryDialogueModal`, `DeliveryHUD` và các con của nó, `VictoryDialog`/`DefeatDialog` **nếu** chỉ code bị xóa dùng tới.
3. **Minimap:** gỡ cùng `DeliveryHUD` (quyết định của Claude: radar hiện được viết cho đơn giao hàng; cốt truyện đã có cột sáng + dòng mục tiêu chỉ đường). Có thể làm lại minimap cho Guardian ở lượt sau nếu cần.
4. Xóa object gốc `GAMEPLAY_GRAB_CAMPUS` (sau bước 1, nó chỉ còn đồ Grab).
5. Scene `IUH_CombatArena.unity`, `IUH_Campus_AR_Encounter.unity` và mọi prefab: không được còn tham chiếu tới script bị xóa.

### 5.2 Code xóa (kèm `.meta`, thư mục rỗng)
- `Scripts/Minigames/GrabCampus/**` (gồm `IUHCustomizationManager` — hệ theme thương hiệu), `Scripts/Minigames/GreenCampus/**`, `Scripts/Minigames/PhotoHunter/**` (kèm test Editor của chúng).
- `Scripts/UI/`: `IUHMiniGamePortalUI`, `IUHMiniGamePrepUI`, `IUHCustomizationUI`, `IUHHorrorPursuitUI`, `IUHDeliveryDialogueUI`, `IUHCampusHubHUD`, `IUHMainMenuUI`, `IUHWorldLocationPin`, `IUHMinimapUI`.
- `Scripts/Core/`: `IUHGameFlowManager`, `GameManager`, `PlayerLoadout`. `Scripts/Hub/IUHGameHubManager`. `Scripts/Player/IUHUniformManager`. `Scripts/IUHInteractiveTourController`.
- `Editor/`: builder/setup cũ chỉ phục vụ Grab/GreenCampus/PhotoHunter/menu/flow (ví dụ `IUHGrabCampusBuilder`, `IUHGrabCampusGameplaySetup`, `IUHGameFlowRefactorSetup`). Builder khác còn tham chiếu lớp bị xóa: xóa nếu là công cụ dựng cảnh một lần đã cũ, ngược lại sửa tối thiểu — liệt kê quyết định trong báo cáo.
- Asset dữ liệu/prefab chỉ dùng cho các hệ trên (tìm theo GUID) — xóa, liệt kê.

### 5.3 Code sửa tối thiểu
- `IUHMonsterEncounter`: bỏ phần tìm/dừng/khôi phục `IUHGrabMotorbike`.
- `CombatEntryTests`: bỏ phần tạo `IUHGameFlowManager`.
- Giữ `IUHInputHelper.VirtualVehiclePressed/IsVehicleKeyDown` (khinh khí cầu và nút Vehicle mobile còn dùng).
- Mọi chỗ khác do compiler báo: cắt đúng nhánh liên quan, không đổi hành vi còn lại.

### 5.4 Thứ tự làm (mỗi bước compile + Console + Missing Script)
1. Chuyển canvas ra `GUARDIAN_UI` → lưu scene → Play: mở kho, shop, vào một trận match-3, nghe âm thanh.
2. Gỡ component/con Grab khỏi canvas + xóa `GAMEPLAY_GRAB_CAMPUS` → lưu scene.
3. Xóa code + sửa chỗ gọi → compile sạch.
4. Quét GUID: không asset nào còn tham chiếu script đã xóa.

## 6. Kiểm chứng
- Compile 0 error; Missing Script = 0 trong cả 3 scene và mọi prefab (`GetMonoBehavioursWithMissingScriptCount`).
- EditMode + PlayMode toàn dự án Pass (số test giảm do xóa test minigame — ghi số trước/sau).
- Play: bấm M không có gì; cốt truyện chạy từ đầu tới chương 1 (F9), vào/thắng 1 trận match-3, mở kho và shop; phá đảo (save Completed) → dư chấn vẫn xuất hiện; khinh khí cầu vẫn lên được.
- Không còn chữ "Grab/XanhSM/ToCoToCo" trong `Assets/IUH_Campus/Scripts` và scene (trừ tên lịch sử trong Docs).
- Bằng chứng: `Docs/Evidence/CampusGuardian/CLEANUP-GRAB/`.

## 7. Để người thực hiện quyết
- `VictoryDialog`/`DefeatDialog`, các con của `DeliveryHUD`: giữ hay xóa theo việc còn script giữ lại nào dùng.
- Builder trong `Editor/`: xóa hay sửa (mục 5.2).
- Không còn ai đứng trong scene khi sửa: báo Thịnh không sửa scene campus trong lúc làm lượt này.
