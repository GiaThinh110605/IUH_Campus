# STORY — bằng chứng và trạng thái kiểm thử

Ngày: 16/11/2026. Môi trường: Windows, Unity 6000.5.8f1, URP, Unity MCP.
Nhánh: `minhnhat/story-dem-glitch`. Base trước bản sửa ambush: `63a959b`.
Task 6 đã dừng theo yêu cầu; lượt mới chỉ tái hiện và kiểm chứng bản sửa ambush.

## Các commit trước bản sửa ambush

| Commit | Nội dung |
|---|---|
| fdd32fd | StoryProgress và 13 EditMode test |
| 6c4ad92 | Kịch bản, material, nâng tổng test lên 16 |
| 90634ff | Định dạng script và asmdef, không đổi logic |
| 9d7328c | UI thoại, camera giới thiệu, beacon |
| df2a08a | Director, WorldLock, vùng nhiễm, chuyển đêm |
| 61db2be | Đổi m_Script thành m_StoryScript, tránh trùng field native MonoBehaviour |
| 06633f3 | Chỉ thêm STORY_DIRECTOR và root, 46 dòng scene |
| 63a959b | Bằng chứng ban đầu và lỗi screenshot MCP |

## Bản sửa ambush — RED → GREEN qua MCP

**Mục 2 Fail ở lần đầu.** WorldLock chỉ đặt `IUHBuildingMonsterSpawner.enabled=false`, nhưng Unity vẫn gửi `OnTriggerEnter` đến MonoBehaviour đã tắt. GameObject/collider còn hoạt động nên ambush có thể bật lại quái. Không sửa file spawner của Thịnh.

Trước sửa: Play scene campus, WorldLock đã khóa; spawner Nhà E có GameObject active và component disabled. Đặt Campus Student vào tâm BUILDING_E_AMBUSH (18,0.1,-15), chờ 1,2 s. Kết quả: **2 IUHMonsterAI activeInHierarchy**; vị trí sau chờ (18.53,0.10,-15.53). Xem `ambush-red.json`. Lượt setup trước đó cũng đã quan sát 4 quái đang hoạt động, nhưng kết quả tái hiện chính là 2 quái sau chờ tại Nhà E.

Sửa: mọi GameObject của IUHBuildingMonsterSpawner đi qua `Capture`, sau đó vòng khóa gọi `SetActive(false)`. Không đưa spawner vào CaptureBehaviours nữa. RestoreAll khôi phục activeSelf gốc qua cùng dictionary GameObject. Kế hoạch Task 4 đã cập nhật.

Sau sửa: vào Play mới, giữ director đã khóa thế giới; dừng coroutine truyện và tạm tắt MonoBehaviour điều khiển trên Campus Student trong runtime để không chuyển chương hoặc di chuyển ngoài ý muốn. Di chuyển bằng MCP/CharacterController tới mỗi tâm và chờ 1,2 s; không gọi trực tiếp TriggerAmbush, không giả lập thắng/thua. Không lưu scene.

| Ambush | Tâm kiểm tra | IUHMonsterAI activeInHierarchy | IUHFlyingMonsterAI activeInHierarchy | Kết quả hồi quy |
|---|---|---:|---:|---|
| BUILDING_E_AMBUSH | (18,0.1,-15) | 0 | 0 | Pass |
| CANTEEN_AMBUSH | (-24,0.1,-18) | 0 | 0 | Pass |
| CENTRAL_PLAZA_AMBUSH | (0,0.5,-12) | 0 | 0 | Pass |
| GARDEN_AMBUSH | (-15,0.1,6) | 0 | 0 | Pass |

Chi tiết kết quả MCP: `ambush-green.json`. Dùng FindObjectsByType không IncludeInactive để đếm object đang activeInHierarchy.

- Compile xong, Console **0 error**.
- EditMode **16/16 Pass**, job `73d06a3fec1d4b04b7446694f02c6d9f`; `ambush-editmode.json`.
- Lượt Play hồi quy không dùng screenshot MCP: Console **0 error**.
- Đã thoát Play, khôi phục PlayerPrefs từ `ambush-progress-backup.txt`; STORY_DIRECTOR bật, scene isDirty=false, Missing Script=0.
- Scene chỉ còn đúng 6 thay đổi font có sẵn, không revert và không stage. Không sửa code của Thịnh. Không push.
- Một số lệnh MCP bị timeout ở bước duyệt công cụ hoặc lỗi mạng và không thực thi. Các lệnh đó không được ghi Pass; bảng trên chỉ dùng những lệnh trả kết quả thực tế.

## Bảng Task 6 để người dùng điền

Các mục 3–8 và 10 do người dùng chơi thử; cột kết quả để trống. Bản sửa hồi quy ambush không thay thế kiểm thử toàn truyện.

| Mục | Người kiểm / phạm vi | Kết quả | Ghi chú |
|---|---|---|---|
| 1. Camera, caption IUH, Bỏ qua | Agent — desktop | Pass | Caption/camera được quan sát trong campus; click Bỏ qua về người chơi, introCameraCount=0, timescale=1. desktop-intro-caption.png. |
| 2. Không quái, không bắn, không khinh khí cầu trước biến cố | Agent — kiểm ambush | Fail lần đầu; hồi quy ambush Pass | Nguyên nhân trigger trên component disabled như trên. Sau sửa cả 4 tâm không còn quái mặt đất/bay. Chưa kết luận toàn bộ K/F/click và khinh khí cầu sau sửa. |
| 3. Check-in đúng thứ tự | Người dùng chơi thử | | |
| 4. Biến cố, chớp đèn, đêm, LED | Người dùng chơi thử | | |
| 5. Quái từng chương, beacon, đếm | Người dùng chơi thử | | |
| 6. Thua không đổi chương | Người dùng chơi thử | | |
| 7. Resume chương 2 sau thắng 1 quái | Người dùng chơi thử | | |
| 8. Trùm, kết, khôi phục tự do | Người dùng chơi thử | | |
| 9. Tắt STORY_DIRECTOR thì game như cũ | Agent — Play riêng | Pass | Không có UI truyện, 10 encounter, 3 quái bay; combat enabled và khinh khí cầu active. Đã thoát Play, bật lại director, không lưu scene. |
| 10. Simulator ngang thao tác 1/3/5 | Người dùng chơi thử | | |
| 11. Console, Missing Script | Agent — lượt không dùng screenshot MCP | Pass | Console 0 error trong lượt desktop đã đọc và lượt Play hồi quy mới; Missing Script=0 sau khi thoát. 5 lỗi screenshot MCP cũ ghi riêng bên dưới. |

Quan sát desktop trước lượt hồi quy có input người dùng xen vào và tiến độ đã sang Chương 1. `desktop-ambush-observation.json` chỉ là dữ liệu quan sát encounter ở thời điểm đó, không dùng làm bằng chứng FAIL trước biến cố. Tiến độ sau người dùng chơi đã ghi ở `playerprefs-after-user-play.txt`.

## Screenshot và các giới hạn

- **5 lỗi screenshot MCP cũ là ngoài phạm vi:** PlayerLoop đệ quy tại package MCP ScreenshotUtility.cs:197. Stack trace lưu trong `task6-mcp-errors.json`; không tính vào lượt Play mới. Không sửa package và không gọi lại manage_camera screenshot.
- Đã xóa `task6-mcp-capture.png` bằng `git rm`: ảnh lỗi, không dùng làm bằng chứng.
- Dùng desktop theo cho phép của người dùng. `desktop-intro-caption.png` là bằng chứng caption thật trong campus.
- Task 3 dùng profile **iOS Notch Device (1170×2532)** thay iPhone 12, xoay ngang cả hai chiều. Ảnh `ui-notch-left.png`, `ui-notch-right.png`, `ui-pc.png`. Safe area đo ở 2532×1170: (141,63,2250,1107), khung thoại/mục tiêu/Skip nằm trong vùng an toàn.
- Skip cảm ứng Task 3 đã thử bằng TouchState Began/Ended ở hai frame qua Input System; click desktop quá nhanh từng cho down ở (0,0) nên không dùng lần click đó làm bằng chứng cảm ứng. Chưa kiểm Enter/Space/Esc độc lập.
- `task5-invalid-serialization.patch` chỉ là bằng chứng lỗi m_Script cũ, **không áp dụng**. `scene_story.patch` là patch Task 5 hợp lệ đã stage riêng STORY_DIRECTOR.
- Các thay đổi kế hoạch được cho phép: m_StoryScript, commit style/fix riêng, profile thay thế, stage scene bằng patch để loại 6 hunk font; sửa khóa spawner theo GameObject.

## Bước 2 Match3 / backend / skybox

Chi tiết và job IDs: [diag-step1.md](diag-step1.md), phần kết quả cuối thay thế các ghi chú chưa xác nhận trước đó.

| Sửa | EditMode | PlayMode | Kiểm runtime |
|---|---:|---:|---|
| Flee báo thua đúng một lần (d08d633) | 32/32 | 24/24 | Garden ESC về AR=False/engaged=False, sau đó E vào lại active=True |
| Guard StartNewCombat (8c85f96) | 32/32 | 25/25 | Click/Space không reset rules/timer; gọi lặp trực tiếp chỉ warning |
| Backend scene-local (5812ed7) | 90/90, gồm Backend 58 | 25/25 | Sau F9 chỉ 1 service/reporter hợp lệ; loss tăng Outbox 15 -> 16 |
| Skybox runtime clone | 32/32 | 26/26 | Exposure 0.85 -> 0.2125 -> 0.85, asset gốc không đổi |

Console 0 error trong các lượt xác nhận sau sửa; Missing Script=0. Ảnh [ngày](skybox-day.png) và [đêm](skybox-night.png) là desktop Scene view cùng góc campus. Các lượt kiểm input qua MCP tạm dừng timer để tránh timeout và đã trả lại timeScale=1; không coi đây là bằng chứng chơi hết truyện. Save gốc đã khôi phục, không lưu scene.

Giữ nguyên file AR, không push. Các mục 3–8 và 10 trong bảng chơi thử vẫn dành cho người dùng.

Rà soát phát hiện thêm hồi quy nút Thử lại do guard mới; đã sửa bằng đường retry chỉ dành cho trận đã kết thúc, có test Button.onClick. Vì vậy có thêm một commit sửa hồi quy ngoài bốn commit yêu cầu. Lượt kiểm chứng cuối: **32/32 EditMode, 27/27 PlayMode, Console 0 error**; Backend đã chạy **58/58 Pass** ở mục 3. Không sửa file AR.
