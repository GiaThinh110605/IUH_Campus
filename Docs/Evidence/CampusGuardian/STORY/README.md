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
