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

## Cập nhật 17/11/2026 — Claude Code làm trực tiếp (người dùng vắng)

Môi trường: Windows, Unity 6000.5.8f1, MCP unityMCP, nhánh minhnhat/story-dem-glitch.

- **AR (Thịnh đồng ý 17/11/2026)** — `IUHARCorruptedObjectEncounter.cs`, đúng 2 chỗ:
  (a) `StartCombatEncounter` bỏ qua khi `Match3CombatManager.Instance.BlocksExplorationInput`;
  (b) `OnMatch3CombatFinished` bỏ qua kết quả của encounter khác (vẫn giữ đăng ký).
  Kiểm trong Play bằng `execute_code` (Garden bật tạm trong Play, không lưu):
  - Đang đánh `MONSTER_02_ORC` → gọi `StartCombatEncounter` của Garden: `gardenInCombat=False`, trận hiện tại vẫn là ORC. Flee → `engaged=False`.
  - Garden tự vào trận → phát giả `CombatFinished(ORC, true)`: Garden vẫn `inCombat=True`, `purified=False`. Garden tự Flee → `inCombat=False`.
- **Xe Grab** — `IUHGrabMotorbike.cs`: xe đậu ở (398, 0.6, -18), nơi không có collider nào (raycast chỉ trúng chính xe) nên rơi → "Auto-recovered" → rơi lại mỗi khung hình. Sửa: khi đứng yên, kiểm một lần có mặt đất bên dưới không (bỏ qua collider của chính xe); không có thì giữ xe tại chỗ. Kiểm lại khi lên/xuống xe. Sau sửa: 3,6 s Play không còn cảnh báo, xe đứng yên ở y=0.6.
  Lưu ý cho Thịnh: khu đường Grab quanh x≈400 hiện không có mặt đất trong scene campus, nên minigame Grab (dịch người chơi tới (410, -18)) có thể hỏng — chưa xử lý trong lượt này.
- `ProjectSettings/EditorSettings.asset`: trả `Enter Play Mode Options` về tắt (Reload Domain and Scene) qua API Editor; không commit vì chỉ còn khác CRLF.
- Kiểm chứng: compile 0 error; **EditMode 90/90, PlayMode 27/27 Pass** (toàn dự án). Console sau Play: 0 error; còn 4 cảnh báo "The referenced script (Unknown) on this Behaviour is missing!" lúc vào Play — không có GameObject nào trong scene thiếu script (`GetMonoBehavioursWithMissingScriptCount` = 0); nghi từ asset bên thứ ba/XR có sẵn trên main, chưa xử lý.
- Mục 3–8 và 10 vẫn chờ người dùng chơi thử.

## Aftershock 17/11/2026 — Task 7–8

Base: 30e107a; Windows Unity 6000.5.8f1. Console ban đầu có 4 warning referenced script Unknown và các assert AABB có sẵn; ghi riêng, không coi là lỗi mới của task.
Task 7: 4db7ac9, RED CS0246 thiếu AftershockSchedule; GREEN Story EditMode 22/22 (14464afc1100450d88007a0ee3c6e2b4), Console 0 error sau compile.
Task 8: test RED thiếu ResetForAftershock (e516061d487f446ca195ae0d6cc65fb9), GREEN Match3+Story PlayMode 28/28 (d2e80eef02b04617b5db034a75d4b0b3). Chỉ thêm hai hàm reset và lưu màu/intensity đèn ở Awake AR.
Kiểm runtime qua MCP: vào Garden, Flee rồi giả thắng bằng OnCombatVictory -> object tắt; SetActive(true) và reset cả hai component -> object bật; dịch người chơi lại gần, E qua Input System -> engaged=True (đang bắt đầu trận), không error. Chưa ghi Pass thắng trận thật; đây là giả thắng được kế hoạch cho phép. aftershock-reset-play.json. Save đã sao lưu ở aftershock-save-backup.txt để khôi phục sau lượt.

## Aftershock Task 9

Story EditMode 22/22 (493b2bcd233d4ea2902e8622af447698); Match3+Story PlayMode 29/29 (7aa06d721d114a9798c4d01fd8fa2419), gồm CompletedSaveKeepsMonstersLockedAndToysOn. Test RED trước sửa: save Completed vẫn để encounter/spawner bật (4e8cd234576d4f27a93ce232a178f9ed).
Mở Play với save Completed: ground=0/flying=0/encounter=0, combat manager enabled=True và balloon active=True. Chưa kiểm thao tác bắn/lái khinh khí cầu bằng bàn phím, không ghi Pass riêng hai thao tác đó.
Rút ngắn runtime schedule xuống firstDelay=1 (không đổi code 120): khu 0 Nhà E có 1 encounter SPORTS; Gold=1738. Giả thắng OnCombatVictory -> encounter tắt, Gold=1768 (+30) và ổn định qua lượt kiểm sau, không cộng lặp. Rút ngắn runtime lần tiếp -> khu 2 Sân chính, 2 encounter BACKPACK/FOUNTAIN, Gold vẫn1768. aftershock-second-play.json. Đây là kiểm fake victory qua MCP theo lượt test, không phải thắng bằng bàn cờ. Console 0 error.
