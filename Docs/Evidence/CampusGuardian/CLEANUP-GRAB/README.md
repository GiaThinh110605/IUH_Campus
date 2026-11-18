# CLEANUP-GRAB — kiểm tra trước khi sửa

18/11/2026 · Windows · Unity 6000.5.8f1 · `minhnhat/remove-grab-minigames` · base `3b3a2b9`.

- MCP kết nối đúng campus, Edit Mode, `isDirty=false`, Console không có lỗi hoặc cảnh báo trong lần đọc trước test.
- EditMode toàn dự án 104/104 Pass (`678d3fa19f9e43a0a84a2d94d6f91755`). PlayMode toàn dự án 29/29 Pass (`9532e7807cea4497a149b7c32a358bdd`).
- Assembly test hiện có: `IUHCampus.Guardian.Story.Tests`, `IUHCampus.Guardian.Story.PlayModeTests`, `IUHCampus.Guardian.Match3.Tests`, `IUHCampus.Guardian.Match3.PlayModeTests`, `IUHCampus.Guardian.Backend.Tests`. Chưa xác nhận số Missing Script của cả ba scene và prefab.
- Quét tên UI trong Scripts và SharedCampusConcept: `IUH_Minigame_Canvas` xuất hiện trong `IUHGameFlowManager`, `IUHPhotoHunterSetup`, `IUHCampusHubHUD`, `IUHMiniGamePrepUI`; `DeliveryHUD` trong `IUHMiniGamePortalUI`. Các lớp này thuộc kế hoạch gỡ.

Dừng trước Task 1 theo điều kiện GUID trong prompt. `IUHGreenBillboard.cs.meta` có GUID `a0ee1b19018f4f7439af6158df31daaf`, được hai GameObject `PROMPT_TAP` trong `IUH_Campus_AR_Encounter.unity` tham chiếu (dòng 16510 và 16911). Scene này được giữ. GUID cũng xuất hiện trong scene campus. `IUHCombatFloatingText.cs` tạo component này lúc chạy. Cần quyết định có chuyển billboard dùng chung ra khỏi GreenCampus, giữ GUID và hành vi hay không. Chưa sửa code, scene, xóa asset hoặc tạo commit.

## Quét phụ thuộc trước Task 1

Quét 46 file trong danh sách cụ thể (gồm script và các builder đã xác định rõ), 69 tên kiểu khai báo và 52 GUID (gồm metadata thư mục). Đối chiếu code C# ngoài tập xóa và YAML Unity trong Assets. Các builder còn lại được liệt kê dưới dạng đầu mối để phân loại ở Task 3, chưa quyết định xóa. Kết quả theo từng file và dòng nằm trong dependency-audit.json. Trùng tên kiểu chung như Report và tên tự khai báo không được tính là phụ thuộc.

| Phụ thuộc còn dùng | Nơi dùng | Phân loại |
|---|---|---|
| IUHGreenBillboard | 25 tham chiếu campus, 2 AR; IUHCombatFloatingText, IUHARSceneBuilder, IUHMatch3CombatBuilder | Tiện ích chung. Đã được phép chuyển sang Core, giữ meta/GUID. AR encounter còn import namespace cũ. |
| IUHGrabMotorbike | IUHMonsterEncounter; IUHSystemAuditTests; 1 tham chiếu scene campus | Logic Grab được Guardian dùng trực tiếp. Encounter lấy xe làm fallback player và tạm tắt/khôi phục controller khi vào/ra trận. Task 3 dự kiến cắt các nhánh này, nhưng yêu cầu mới là dừng và báo khi còn phụ thuộc dạng này. |
| IUHGameFlowManager, FlowState | CombatEntryTests, IUHCleanMenus | Test/Editor, không là runtime Guardian. |
| IUHGreenCampusManager, IUHGreenCampusSetup, GreenCampusStorage, GameState | IUHCampusPhotoRefinement, IUHCampusConnectionsValidation, IUHReferenceCampusBuilder, IUHReferenceLayoutValidation, IUHSystemAuditTests, IUHSharedSceneMenu | Builder, validation và test. |
| IUHPhotoHunterManager, IUHPhotoHunterSetup, IUHPhotoTarget, IUHPhotoCaptureService, IUHPhotoHunterUI, IUHPhotoHunterStorage, PhotoHunterSaveContainer | IUHCampusPhotoRefinement, IUHCampusConnectionsValidation, IUHReferenceCampusConnections, IUHReferenceCampusBuilder, IUHReferenceLayoutValidation, IUHSystemAuditTests, IUHSharedSceneMenu | Builder, validation và test. |
| GameManager, IUHGrabCampusManager, IUHInteractiveTourController, IUHGameFlowRefactorSetup | IUHGameSystemsMasterBuilder, IUHMultiFloorMasterBuilder, IUHReferenceCampusBuilder, IUHCampusMasterBuilder, IUHCampusEnvironmentBuilder | Builder. |
| IUHCampusHubHUD, IUHCustomizationUI, IUHMiniGamePortalUI, IUHWorldLocationPin | IUHCampusEnvironmentBuilder, IUHSharedSceneMenu, IUHCleanMenus, IUHSystemAuditTests, IUHStreetDistrictBuilder | Editor. Portal có thêm 1 tham chiếu campus. |
| IUHDeliveryWaypoint (2), IUHGrabSurvivalItem (8), IUHGhostHazard (6), IUHDeliveryDialogueUI (1), IUHMinimapUI (1), IUHHorrorPursuitUI (2), IUHGrabCampusHUD (1), IUHGrabInventoryManager (1), IUHCustomizationManager (1) | Scene campus | Số lần tham chiếu GUID. Cần đối chiếu hierarchy trước khi xóa ở Task 2. |

Không có GUID ứng viên trong prefab, asset hoặc controller YAML ngoài các tham chiếu scene đã liệt kê. Chưa di chuyển billboard hay sửa/xóa code và scene vì vòng quét đã tìm thấy phụ thuộc logic Grab trong Guardian.

## Kết quả Task 1–4

| Task | Commit | Kết quả |
|---|---|---|
| 1 | d421a9b | Chuyển canvas sang GUARDIAN_UI/GUARDIAN_UI_CANVAS. Chuyển billboard và meta bằng git mv, giữ GUID. Thử mở shop/kho, vào trận và rút lui, tạo chữ nổi có component billboard. |
| 2 | a122336 | Bỏ Grab root, các HUD/overlay/menu Grab. Quét SerializedObject của các MonoBehaviour giữ lại trước khi xóa: không thấy tham chiếu đến VictoryDialog/DefeatDialog cũ, đã xóa cả hai. Cả hai modal Match3 nằm trong nhánh được giữ. Canvas còn bốn con Guardian. |
| 3 | 9fe4a08 | Xóa 48 file ứng viên và các meta, dọn meta thư mục trống. Tách nhánh xe khỏi encounter, giữ điều khiển người chơi. Tìm 55 GUID đã xóa trong scene/prefab/asset/controller: 0 kết quả. |
| 4 | Commit chứa mục báo cáo này | Chạy test toàn dự án và kiểm runtime qua MCP. |

| Bộ test | Trước | Sau |
|---|---:|---:|
| EditMode | 104/104 | 104/104, job 4cc5518118574c1b9970267b2da99964 |
| PlayMode | 29/29 | 28/28, job f973a5e931ba462eae15fc68e4b23188 |

Không xóa assembly test riêng. Gỡ các script test legacy trong Minigames và test ConsumedEscapeDoesNotAlsoPauseGameFlow khỏi assembly Match3.PlayModeTests. Số test được Unity phát hiện giảm đúng một ở PlayMode. Kiểm sau sửa: Missing Script = 0 ở campus, CombatArena, AR và 83 prefab. Billboard còn 24 instance campus và hai instance AR. Console 0 error.

### Kiểm runtime Task 4

| Mục | Kết quả | Cách kiểm |
|---|---|---|
| Reset và camera mở đầu | Pass cho handler và camera, chưa xác nhận phím F9 | Key event F9 qua MCP không reset ở lượt kiểm. Gọi trực tiếp ResetStory(), quan sát camera intro mới sau tải lại scene. |
| Check-in và biến cố tới Chương 1 | Pass | Di chuyển người chơi đến đúng ba target và tiến thoại qua MCP. Tiến độ v1 Chapter 0 với 3 check-in. |
| Match3 Nhà E | Pass với giả thắng | TriggerCombat cho engaged=True. Kết thúc trận thử qua OnCombatVictory và xác nhận object tắt, ID Nhà E xuất hiện trong tiến độ save. |
| Shop và kho | Pass | Mở tab nâng cấp và tab inventory qua MCP, đóng được. |
| Âm thanh và chữ nổi | Pass ở mức runtime | AudioSource.isPlaying=True sau PlaySword, FloatingText tạo đúng component billboard đã chuyển. Chưa nghe âm thanh hoặc đánh giá chữ bằng mắt. |
| Phím M | Kiểm cấu trúc và runtime | Gửi M khi khám phá, shop vẫn đóng và loại component portal đã bị gỡ. Chưa xác nhận riêng việc Unity nhận phím M từ input mô phỏng. |
| Dư chấn sau Completed | Pass | Nạp checkpoint Completed trong runtime, rút lịch đầu xuống 1 giây trong RAM. Dư chấn active với 1 encounter. |
| Khinh khí cầu | Pass | Gọi ToggleBoardOrLaunch qua MCP, State=Ascending, IsPlayerOnBoard=True. |
| Khôi phục tiến độ và kiểm scene | Pass | Khôi phục giá trị PlayerPrefs gốc và so sánh lại. Thoát Play, scene isDirty=false, Console 0 error. |

Lượt thử rút lui dùng luồng Flee thật, có thể tạo sự kiện thua trong outbox. Giả thắng chương 1 chỉ chạy nhánh đóng encounter để kiểm tiến độ.

### Quyết định từng file Editor

| File | Quyết định |
|---|---|
| IUHGrabCampusBuilder | Xóa builder Grab. |
| IUHGrabCampusGameplaySetup | Xóa setup Grab. |
| IUHGameFlowRefactorSetup | Xóa setup game flow cũ. |
| IUHSixGameMenuDesign | Xóa builder menu sáu game. |
| IUHCleanMenus | Xóa tiện ích chỉnh menu và game flow cũ. |
| IUHSharedSceneMenu | Xóa setup portal cùng GreenCampus/PhotoHunter. |
| IUHSystemAuditTests | Xóa các công cụ kiểm storage, dựng minigame và fast travel qua portal. |
| IUHARSceneBuilder | Giữ, cập nhật namespace billboard. |
| IUHMatch3CombatBuilder | Giữ, cập nhật namespace billboard và tên canvas. |
| IUHCampusConnectionsValidation | Giữ kiểm hành lang, bỏ gọi hủy hai minigame. |
| IUHCampusEnvironmentBuilder | Giữ dựng môi trường, bỏ location pins, HUD hub và setup flow. |
| IUHCampusMasterBuilder | Giữ builder, bỏ gắn tour controller đã xóa. |
| IUHCampusPhotoRefinement | Giữ chỉnh môi trường, bỏ di chuyển điểm Grab, rác GreenCampus và target PhotoHunter. |
| IUHGameSystemsMasterBuilder | Giữ builder và object cha cho PlayerSpawnManager, bỏ component GameManager cũ. |
| IUHMultiFloorMasterBuilder | Giữ builder và object cha cho PlayerSpawnManager, bỏ component GameManager cũ. |
| IUHReferenceCampusBuilder | Giữ dựng campus và đặt người chơi, bỏ liên kết ba minigame. |
| IUHReferenceCampusConnections | Giữ kết nối và marker hành lang, bỏ nối target PhotoHunter và di chuyển trạm giao hàng Grab. |
| IUHReferenceLayoutValidation | Giữ kiểm tuyến đường, hình học và chụp cảnh, bỏ các round test Green/Photo. |
| IUHStreetDistrictBuilder | Giữ dựng phố, bỏ pins tương tác minigame. |
| IUHGreenCampusSetup, IUHPhotoHunterSetup | Xóa theo thư mục minigame. GreenCampusSetup có thay using ở Task 1 để biên dịch qua commit trung gian. |
| IUHIOSCommandLineBuild, IUHARVisionTests, IUHBuildAutoExec | Giữ. EditMode toàn dự án đã chạy, chưa thực hiện iOS build. |

Danh sách đầy đủ file xóa/sửa và metadata trong removed-and-modified-files.tsv. Không có prefab hoặc data asset riêng chỉ phục vụ hệ bị gỡ được xác định từ kiểm GUID và danh sách asset theo tên.

Lệch kế hoạch: thêm gỡ IUHGrabCampusHUD trên canvas vì đây là component Grab được phát hiện lúc quét. Unity serialize lại các block không liên quan khi xóa cây. Patch scene lấy từ bản nền theo fileID, chỉ loại 1261 block thuộc cây cần xóa và cập nhật danh sách tham chiếu cha; đã kiểm 0 local fileID trỏ vào block đã loại và mở lại scene trong Unity. Thay đổi cục bộ m_LocalPosition.y trên một prefab instance vẫn nằm ngoài index. Các kiểm tra mô phỏng và giới hạn được nêu trong bảng.
