# P1.5 — Inventory UI

Branch: minhnhat/inventory-ui-p15, from P1 d57c60c. **Merge sau PR #23.** Date2026-11-07, Unity6000.5.8f1/Windows.

## Hiện trạng và cách tích hợp

Combat/HUD dùng uGUI Text, LegacyRuntime.ttf (IUHMatch3CombatBuilder.cs). UI mới dùng đúng LegacyRuntime; đã xem ảnh có dấu Hạt hoa hồng, Hạt hướng dương, Bình tưới, Phân bón ở hai tỷ lệ, không ô vuông mất glyph.

Cơ chế f79902d được đối chiếu tại IUHConceptPreviewController, IUHMonsterEncounter.SetPlayerControl, IUHGameFlowManager và Match3CombatManager: ResetMotion/tắt ThirdPersonController, zero move/look, cursorLocked/cursorInputForLook, khóa IUHGrabMotorbike, Cursor.lockState/visible. UI adapter cache component ở Awake, gọi ResetMotion rồi chụp/khôi phục enabled/cursor flags, tắt preview/flow Update trong lúc mở để Esc không mở menu khác. Chặn mở qua BlocksExplorationInput, IsExplorationPaused, CurrentState khác Playing, hoặc timeScale0. Không sửa Starter Assets/Match3/flow/preview. Dùng tên type/reflection để assembly UI không phải tham chiếu Assembly-CSharp; nếu đổi tên/type integration phải cập nhật adapter. Các component được cache tại Awake; prefab nên khởi tạo sau các controller/combat của campus, chưa hỗ trợ tự bind lại controller được tạo muộn.

## Bàn giao

- Prefabs/Guardian/GuardianInventoryUI.prefab: Overlay/sorting300, CanvasScaler1920x1080/match0.5; safe-area root, bag luôn hiện, grid5ô preallocated theo database hiện tại, detail/Core/close.
- Inventory/Wallet Changed cập nhật view; không poll inventory trong Update, không Instantiate khi mở lại. Chỉ keyboard + thay đổi safe area được kiểm tra trong Update.
- Seed: Trồng disabled với dòng trợ giúp luôn hiện (dễ đọc trên touch); Consumable: Dùng disabled; Tool: không nút. `SetPlantingContext(IGuardianPlantingContext)` cho P2; CanPlant/Plant và ActionRequested. Consumable chưa có hành động P2.
- Panel fade/slide0.18s unscaled; ô tăng quantity flash0.35s. Không gán Time.timeScale trong code sản phẩm.
- 5PNG128×128 tự vẽ bằng code là **placeholder**, không asset ngoài. Người dùng cho phép đổi CHỈ trường icon của5ItemDefinition; kiểm diff một dòng mỗi asset tại verification.json.
- Scene Test/GuardianInventoryTest.unity có Camera, EventSystem/InputSystemUIInputModule, component GuardianProgressService, UI và nút Editor '+ đồ thử'. Prefab mặc định bind service P1; scene thử bật isolatedPreview, dùng state/Wallet/Inventory P1 trong RAM, không initialize/ghi save thật. Component service trong scene thử giữ lifecycle placeholder; dữ liệu UI dùng state tạm. Debug button ẩn khi build, callback chỉ compile UNITY_EDITOR.

## Chạy thử và ảnh

- [empty-1920.png](empty-1920.png): kho trống,0Core; [items-1920.png](items-1920.png): đủ5món/10Core/chi tiết hạt; [detail-2532.png](detail-2532.png): tỷ lệ iPhone2532×1170. PNG từ ScreenCapture trong PlayMode, không mockup.
- PlayMode: debug add cập nhật count1→2, slot vẫn5, Core tăng; Tool ẩn action, Consumable/Seed disabled đúng. Output tại play-view.json.
- Keyboard: test bằng QueueStateEvent KeyboardState rồi InputSystem.Update + gọi Update UI một lần trong PlayMode; I=True/cursorNone,true, Esc=False/cursorLocked,false; button.onClick mở lại. Output tại play-input.json. Đây là input injection/handler test, **không khẳng định đã test phím vật lý hoặc click/touch raycast trên iPhone**. Native automation bị giới hạn focus Editor; không che giới hạn này.
- Application.runInBackground chỉ bật tạm trong PlayMode phục vụ capture/automation, không sửa ProjectSettings. Safe area đọc Screen.safeArea và cập nhật anchors khi rect/kích thước thay đổi; chưa test tai thỏ trên thiết bị thật.
- Campus active isDirty=false trước builder và khi quay lại sau PlayMode/tests. Không save/ghi nội dung scene campus; diff chuẩn hóa CRLF bằng0, scene không stage.

## Kiểm thử / giới hạn

EditMode toàn bộ66passed/0failed/0skipped:64test cũ+2presenter (map/qty, category/name tiếng Việt, Tool/Seed/Consumable action). Console0error ở snapshot cuối. Raw editmode.json và console.json.

Có3vòng điều chỉnh trong phạm vi UI: tách state thử RAM sau khi auto-review chặn xóa inventory trên save thật; sửa importer placeholder từ Cubemap sangTexture2D để sprite hiện; đối chiếu và sửa tên controller xe thành IUHGrabMotorbike. Lệnh bị chặn không thực thi. Không cố vượt chặn, không xóa đồ save thật.

Chưa kiểm iPhone vật lý, campus end-to-end với character thật, controller spawn muộn và physical keyboard/touch trong harness hiện tại. Không tự gắn prefab vào campus. Test scene Editor không chứng minh các phần tích hợp đó đã nghiệm thu.

**Đã gỡ ở P1-REMOVE (09/11/2026, nhánh minhnhat/remove-progress-p1).** Prefab, scene test, icon và code UI kho P1.5 không còn trong repo; khôi phục từ commit 43444ea nếu cần. Xem [P1-REMOVE](../P1-REMOVE/README.md).
