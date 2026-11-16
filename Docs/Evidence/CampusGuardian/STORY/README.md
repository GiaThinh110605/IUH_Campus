# STORY — tiến độ thực hiện

Ngày: 16/11/2026. Môi trường: Windows, Unity 6000.5.8f1, MCP unityMCP.
Nhánh: minhnhat/story-dem-glitch. Base: dcff45e.
Kế hoạch: Docs/superpowers/plans/2026-11-16-dem-glitch-story.md.

- Task 1 hoàn tất: fdd32fd — feat(story): add StoryProgress chapter state machine with tests.
  RED: CS0246 thiếu StoryChapterDef/StoryProgress. GREEN: 13/13 EditMode Pass, job d73b8878cd104589af84f2d658ad0880. Console 0 error, Missing Script 0.
- Task 2 hoàn tất: 6c4ad92 — feat(story): add Dem Glitch story script data and materials.
  RED: CS0246 thiếu IUHStoryScript. GREEN: 16/16 EditMode Pass, job 9fd8a4526e634cadbe926c8306ea8827. Console 0 error. Test scene dùng scene hiện tại nếu đã mở, chỉ mở additive/đóng không lưu nếu chưa mở để không đóng scene người dùng.
- Task 3 đang làm, chưa commit: IUHStoryDialogueUI, IUHStoryIntroCamera, IUHStoryBeacon và .meta.
  Compile không lỗi. Thử Play trong scene tạm mặc định (Camera + Directional Light), tạo UI/intro/beacon qua MCP.
  Quan sát khung thoại tiếng Việt đủ dấu, click thực sự chuyển sang dòng thứ hai; ảnh ui-pc.png (Game view 1920x1080).
  Camera tự kết thúc, log timescale=1. Chưa kiểm chứng skip bằng chạm/Esc và Enter/Space.
  Device Simulator chỉ có 11 profile chung. Không có profile tên iPhone 12; gần nhất iOS Notch Device (1170x2532). Chưa ghi Pass kiểm tra iPhone 12. Một truy vấn reflection trạng thái Simulator báo NullReferenceException; không phải compile/gameplay error.
  Scene tạm không có GUID tham chiếu ngoài, đã xóa; thoát Play và mở lại campus, isDirty=false.
- Task 4–6 chưa thực hiện. Chưa thêm STORY_DIRECTOR, chưa sửa scene, chưa push/PR.

## Quyết định đã được người dùng cho phép

Giữ nguyên 6 thay đổi font m_MinSize/m_MaxSize có sẵn, không revert, không stage.
Task 5 phải xuất scene_full.patch và lọc scene_story.patch trong TEMP, chỉ giữ hunk STORY_DIRECTOR + SceneRoots.m_Roots.
Dùng git apply --cached --recount, không dùng git add -p.
Trước commit, cached chỉ có STORY_DIRECTOR, unstaged đúng 6 dòng font; lỗi CRLF hoặc thay đổi khác phải dừng.

## Kiểm tay Task 6

Cả 11 mục chưa chạy toàn truyện. Không ghi Pass. Các trận thắng/thua thật cần người dùng chơi thử nếu không tự thao tác được qua MCP.

## Cập nhật Task 3 — 16/11/2026

- Commit style riêng: 90634ff, 16/16 EditMode Pass (job 5b1a10a30eff4c64b1ba794101183558), không đổi logic.
- Được phép dùng **iOS Notch Device (1170×2532)** thay iPhone 12; xoay ngang cả tai thỏ bên trái/phải.
- SafeArea con của Canvas chứa Objective, Dialogue, Caption và Skip. Cập nhật khi Screen.safeArea hoặc kích thước màn hình thay đổi, kể cả khi không có thoại.
- Kiểm layout thật trên Simulator: 2532×1170, safeArea=(141,63,2250,1107). Objective/Dialogue/Skip đều nằm trong safeArea; ảnh ui-notch-right.png và ui-notch-left.png.
- Xoay phải → dọc → trái ngay trong Play, khung UI cập nhật; chữ tiếng Việt đủ dấu, các phần trên không bị tai thỏ che.
- PC: đã quan sát click chuyển thoại (ui-pc.png). Simulator: click đã chuyển tới dòng thứ hai; do timer cũng chạy nên không dùng riêng quan sát này làm bằng chứng chạm.
- Chẩn đoán input: click tự động quá nhanh báo down tại (0,0), up tại tọa độ nút; chưa coi lần click đó là Pass. Sau đó inject TouchState Began/Ended tại (2103,468) qua hai frame trên Device Simulator Touchscreen: nút Skip hoạt động, timescale=1 và intro cameras=0. Đây là kiểm thử cảm ứng bằng Input System qua MCP, không phải chạm ngón tay trên thiết bị thật.
- Camera hoàn tất tự nhiên cũng trả timescale=1. Enter/Space/Esc chưa kiểm tay; không ghi Pass các mục này.
- Thoát Play, trở lại scene campus: isDirty=false, Missing Script=0. Xóa scene tạm sau khi tìm GUID không có tham chiếu ngoài.
- Chưa kiểm mobile controls trên campus (Task 6).

## Tiến độ mới nhất — dừng tại Task 5

- Task 3 đã commit: 9d7328c. 16/16 EditMode Pass, job 79e8be831fd5480a8267af8ee1fc60ec. Console 0 error, Missing Script 0.
- Task 4 đã commit: df2a08a. 16/16 EditMode Pass, job b196eeba80924ab4b4b21a9a3c19b17b. Compile không lỗi, Missing Script 0. Cả 7 encounter có MilestoneId, không cần thêm Gold.
- Task 5 fetch origin/main: không có commit mới sửa scene từ dcff45e.
- Đã thử thêm một root STORY_DIRECTOR, AddComponent IUHStoryDirector và gán asset bằng SerializedObject.FindProperty("m_Script"). Sau save, diff chứa hai khóa m_Script: {fileID: 0}; không có GUID MonoScript hoặc tham chiếu asset kịch bản. Field m_Script của kế hoạch trùng tên serialized property native của MonoBehaviour. Bằng chứng: task5-invalid-serialization.patch (chẩn đoán, KHÔNG áp dụng patch này).
- Không stage/commit scene lỗi. Đã gỡ riêng object STORY_DIRECTOR vừa thêm và save. Diff scene trở lại đúng 6 dòng font có sẵn; index trống. Không revert các thay đổi gốc.
- Cần người dùng cho phép đổi field private serialized từ m_Script thành m_StoryScript và cập nhật chỗ gán ở Task 5 (vì prompt bắt buộc giữ nguyên tên field). Sau đó chạy lại test, kiểm serialization sau reload trước khi stage patch scene.
- Task 6 chưa chạy toàn truyện, 11 mục vẫn chưa có kết luận; không ghi Pass toàn truyện.
- Console xác nhận nguyên nhân tại Task 5: `The same field name is serialized multiple times in the class or its parent class. This is not supported: Base(MonoBehaviour) m_Script`. Đây là lỗi runtime/serialization mới ở Task 5, không phải 0 error. Sau gỡ object: isDirty=false, Missing Script=0; giữ log lỗi làm bằng chứng, không xóa Console để che lỗi.

## Cập nhật cuối lượt — Task 5 xong, Task 6 bị chặn

Ngày 16/11/2026, Windows, Unity 6000.5.8f1, nhánh minhnhat/story-dem-glitch.

- 61db2be — fix(story): rename director script field to avoid MonoBehaviour m_Script clash. Commit chỉ có IUHStoryDirector.cs và kế hoạch đã sửa. Compile không lỗi, 16/16 EditMode Pass (job 78216f92b47443659aa1647d6d3e8c43).
- 06633f3 — feat(story): place STORY_DIRECTOR in campus scene. Gồm đúng 46 dòng thêm: GameObject, MonoBehaviour, Transform và root. m_StoryScript trỏ asset 40da2969dc7ba7c4da1d527201cadfd0; script GUID 35470b6b535211f4e862754953754b14. Mở lại scene xác nhận giữ nguyên hai tham chiếu. Missing Script=0, isDirty=false, Console 0 error trước Play.
- Stage bằng TEMP/scene_story.patch với git apply --cached --recount. Cached chỉ có STORY_DIRECTOR; unstaged đúng 6 thay đổi font. Bản patch đúng lưu ở scene_story.patch. Git cảnh báo trailing whitespace của dòng m_Name trống do Unity serialize; apply thành công, không có lỗi CRLF.
- Task 5 EditMode: lần đầu timeout khởi tạo (job 4e1d0e468abd413d8811073cbc8844bc, chưa chạy test); thử lại với init_timeout=120000: 16/16 Pass (job 97d532cc526d41b293f933daec478cf6).
- Task 6: sao lưu khóa IUH_Story_Progress trong playerprefs-before.txt rồi xóa để chạy từ đầu. Đã Play và xác nhận progress=v1|Prologue|0|0|, UI được tạo; 0 encounter, 0 IUHMonsterAI, 0 IUHFlyingMonsterAI activeSelf.
- Khi gọi manage_camera screenshot, MCP phát sinh 5 error PlayerLoop đệ quy. Stack trace thuộc Library/PackageCache/com.coplaydev.unity-mcp@045e809812a8/Runtime/Helpers/ScreenshotUtility.cs:197 (CaptureCompositedAfterFrame), qua ManageScene/ManageCamera. Lưu task6-mcp-errors.json; task6-mcp-capture.png là ảnh lỗi/không đủ overlay, KHÔNG dùng làm bằng chứng prologue Pass.
- Dừng Task 6 theo luật lỗi ngoài phạm vi, không sửa package MCP. Đã thoát Play. Cần quyết định dùng chụp màn hình desktop thay công cụ screenshot MCP và tiếp tục chơi thử.
- Lệch kế hoạch được người dùng cho phép: field m_StoryScript, thêm commit style và fix riêng, profile iOS Notch Device thay iPhone 12, stage scene bằng patch tách font.

### Bảng 11 mục Task 6 (chưa hoàn tất)

| Mục | Kết quả | Bằng chứng / còn thiếu |
|---|---|---|
| 1. Camera, caption, bỏ qua | Chưa xác nhận — cần chơi thử | Play đã bắt đầu; screenshot MCP lỗi. Skip cảm ứng đã thử riêng ở Task 3. |
| 2. Không quái/bắn/khinh khí cầu trước biến cố | Chưa xác nhận toàn bộ | Đo được 0 encounter/ground/flying active; chưa đi qua 4 ambush và nhấn K/F/click. |
| 3. Check-in đúng thứ tự | Cần người dùng chơi thử | Logic đã có EditMode test; chưa kiểm hành vi campus. |
| 4. Đêm, chớp đèn, LED, thoại | Cần người dùng chơi thử | Chưa tới biến cố. |
| 5. Quái từng chương, beacon, đếm | Cần người dùng chơi thử | Chưa tới chương 1. |
| 6. Thua không đổi chương | Cần người dùng chơi thử | Phải thua trận match-3 thật; chưa chạy. |
| 7. Resume chương 2 sau thắng 1 quái | Cần người dùng chơi thử | Chưa có trận thắng thật. |
| 8. Trùm, kết, phục hồi tự do | Cần người dùng chơi thử | Chưa phá đảo. |
| 9. Tắt director game như cũ | Cần người dùng chơi thử | Chưa thử trong campus. |
| 10. Simulator ngang thao tác 1/3/5 | Chưa xác nhận toàn bộ | Task 3 đã có ảnh safe area cả hai chiều, chưa chơi luồng campus. |
| 11. Console 0 error, Missing Script 0 | Fail trong lượt Play | 5 error từ screenshot MCP; trước Play Console 0 error, Missing Script 0. |

Không có mục nào trong bảng được ghi Pass toàn bộ; không giả lập thắng/thua để thay trận thật.
