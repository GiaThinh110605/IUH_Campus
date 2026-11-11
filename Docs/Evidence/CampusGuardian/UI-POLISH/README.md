# UI-POLISH — hoàn tất4 điểm

Ngày12/11/2026 Asia/Bangkok; Windows Editor Unity6000.5.8f1, URP/uGUI. Nhánh minhnhat/fix-shop-skins-scroll, nền9beb96d. Commit UI-POLISH là commit chứa README này, mới trên9beb96d theo message người dùng; không push/PR.

## Nguyên nhân và thay đổi
1. Shop Update/OpenShop không chặn match-3. Dùng getter sẵn Match3CombatManager.BlocksExplorationInput (bao phủ m_IsEngaged và frame thoát, cả khi tải arena) để chặn phímB/I/VirtualBackpackPressed và OpenShop gọi trực tiếp; shop đang mở thì đóng khi combat bắt đầu. CloseShop không trả quyền di chuyển cho player đang trong combat. Không đổi logic trận. NútB mobile giữ nguyên; guard shop bao phủ listener ToggleShop lẫn virtual flag, không sửa mobile.
2. Sync HUD trước top-right font20 đè tên quái. Chuyển bottom-left, inset24×12 UI units từ safeArea, size560×26, font cap16, LowerLeft/raycastTarget=false. Trong ảnh nằm dưới joystick/trái board, không che tên quái/help. Không đổi networking/backend/Server.
3. Album container1060×460, spacing24, childControlWidth=false;5 card240px vượt vùng. Cards thực tế sinh runtime trong PopulateAlbumCards, không dựng sẵn như mô tả. Chỉ sửa HorizontalLayoutGroup scene: controlWidth=true, ForceExpandWidth vốntrue giữ, spacing12. Card sau202.4px, card5 inside=True. Không đổi code/nội dung album/check-in. Photo box fixed210px có overhang nhỏ trong card hẹp nhưng vẫn nằm trong panel; không sửa ngoài phạm vi.
4. FloatingText world canvas240×60, scale0.0075×sizeScale không giới hạn perspective nên rất to gần camera. Cache camera/rect/base scale lúc Init; ưu tiên Camera.main, fallback camera đang active (ArenaCamera không gắn tagMainCamera), clamp scale mỗi LateUpdate theo depth/FOV hoặc orthographic để canvas khoảng8% chiều cao. Đo gần camera ratio0.080–0.081, trước khi chốt có phép đo0.085–0.087 do billboard/perspective vẫn dưới khoảng10%. Giữ text/lifetime/fade/số damage.

Hai thông báo là2 loạt bắn thật do IUHFlyingMonsterAI volley<2, cách0.25s, text sống1.1s; bom có m_HasExploded/break, grenade có m_HasDetonated/HashSet nên không phải callback collider nổ lặp. Theo lựa chọn người dùng: giữ cả2 thông báo, chỉ đẩy chữ mới lên theo màn hình nếu spawn gần trong0.3s/radius1.5; gap khoảng một vùng chữ (8%height×1.1) để jitter không chồng. Không gộp/xóa thông báo, không sửaAI/health, không đổi sát thương/nhịp bắn. Chỉ bỏ emoji💥 tại2 chuỗi plasma/grenade.

## Bằng chứng ảnh — chỉ1 trước/1 sau mỗi trường hợp
| Điểm | Desktop1920×1080 | iPhone2532×1170 |
|---|---|---|
|1 shop trong combat và2 vị trí sync HUD|before-combat-desktop.png / after-combat-desktop.png|before-combat-iphone.png / after-combat-iphone.png|
|3 album|before-album-desktop.png / after-album-desktop.png|before-album-iphone.png / after-album-iphone.png|
|4 hai floating notice gần camera|before-floating-desktop.png / after-floating-desktop.png|before-floating-iphone.png / after-floating-iphone.png|

Tổng12 ảnh; không có bản nháp/-final. Điểm1/2 dùng chung ảnh combat. Đã xem các ảnh tiêu biểu, album card5 trong panel, HUD không đè board/title,2 notice sau nhỏ/tách nhau. Ảnh before mô phỏng cờ engaged trên manager và mở shop qua OpenShop, after còn kiểm arena thật qua StartNewCombat với encounter thật. B được mô phỏng VirtualBackpackPressed và OpenShop trực tiếp, không khẳng định phímB vật lý đã được nhấn. shop-block.txt: combatBlocked=True, shopOpen=False, virtualConsumed=True; openedOutside=True, closedOnCombatStart=True. HUD ngoài trận được quan sát lúc shop có thể mở và album; không có thay đổi vị trí theo combat. Chưa kiểm cảm ứng/iPhone thiết bị thật, chỉ aspect trong Editor.

Floating thử bằng Spawn tại2m trước camera active,2 lần cùng vị trí để tái hiện; không cần gây sát thương thật. Time.timeScale=0 khi chụp rồi trả1, không lưu runtime. Một số probe đầu chạy khi manager chưa Awake/domain reload khiến Instance null; đã bỏ kết quả/ghi đè ảnh cuối sau khi kích hoạt manager và xác nhận manager/shop Instance đều tồn tại. Không sửa logic singleton để phục vụ probe.

## Kiểm thử và lịch sử lỗi runner
- Preflight: scene isDirty=false, ngoài Play, Console0 error; ban đầu focusfalse nên đã hỏi, người dùng báo focus. Trước suite đầu state báo focustrue.
- EditMode74/74 pass, job77ed7c0936cc4324be05bddd7961c12c, editmode.json.
- PlayMode22/22 pass, job42ba05f884f7440c8862e81d32db7d86, duration5.3922092s, playmode.json.
- Hai suite xanh trên chạy sau phần offset screen và safeArea, TRƯỚC bổ sung fallback cameraArena3 dòng trong Init. Sau bổ sung đã kiểm thủ công activeArena clamp và ảnh, nhưng chưa có suite xanh cuối cho đúng toàn bộ code hiện tại.
- Job PlayMode cuối e7fe80ffe0344cb6aab3b5580e7c84e7, init_timeout180000/poll60: failed to initialize,0 ca bắt đầu; progress báo editor_is_focused=true khi poll. playmode-final-timeout.json giữ lỗi thật. Không ghi Pass cho lần này. Khi chạy cuối đã mất focus giữa trạng thái kiểm và lệnh run trong cùng batch; đã báo người dùng đưa Unity lên trước, poll đúng job, không tạo job khác. Đây là lỗi Test Runner ngoài phạm vi nên dừng và hỏi theo yêu cầu, chưa commit.
- Compile/Console cuối0 error. Scene get_active isDirty=false; validate(auto_repair=false): missingScripts0, brokenPrefabs0, totalIssues0; ngoài Play. Không InitTestScene sót.

## Cleanup/phạm vi
PlayerPrefs snapshot trước Play:35 key, khôi phục35/35/tổng35; outbox thật khớp outbox-before.json; EditorSettings khôi phục snapshot. GameView trả index11, xóa size tạmUI-POLISH iPhone, timeScale1. Không bật Docker. Vật liệu/csproj/slnx/Logs và file ngoài phạm vi giữ nguyên.

scene.diff đã lọc6 hunk MinSize/MaxSize noise khỏi patch, chỉ2 field album (spacing24→12, childControlWidth0→1); working tree vẫn giữ6 dòng nhiễu. code.diff chỉ5 script được phép. Review độc lập không thấy lỗi chặn code; yêu cầu loại6 noise khỏi index đã chuẩn bị trong patch, chưa stage.

Tooling: một lệnh execute_code bị auto-review từ chối vì helper code không hiện/undefined; đã thay bằng codeC# đầy đủ đọc được, không bypass bằng thực thi gián tiếp. Một số probe C# sai constructor/Instance hoặc Camera.main null đã sửa trong code probe, không lấy ảnh/kết quả lỗi làm bằng chứng cuối. Không sửa plugin/Packages/framework/ProjectSettings.

## Cập nhật sau khi người dùng focus lại Unity
PlayMode trên toàn bộ code hiện tại (bao gồm fallback ArenaCamera) đã chạy thật22/22 pass: job871e4cd3e321479193d830ed892dbd31, duration5.3142465s, playmode-final.json. Trước khi chạy đã poll jobe7fe80…failed/terminal và state focustrue, không Play/không test đang chạy. Không sửa code sau lần xanh này.

Lượt EditMode bổ sung b1d2f9dd45494b34a939fcadd8a5a438 không khởi tạo trong180giây,0 ca, dù progress báo focustrue; editmode-retry-timeout.json + editmode-last-tail.txt. Đây không phải assertion fail/code compile error; không ghi Pass. Lượt EditMode74/74 cũ vẫn có bằng chứng nhưng trước phần fallback Arena3 dòng. Dừng theo yêu cầu lỗi ngoài phạm vi, chưa commit/push/stage.

Cleanup cuối đã chạy lại: PlayerPrefs matched35/expected35/total35, outboxrestoredTrue, EditorSettings diff rỗng, không InitTestScene, sceneisDirty=false, validate0issue/0missing/0brokenPrefab, Console0error. Code/diff/12ảnh giữ nguyên để duyệt commit hoặc kiểm EditMode độc lập. Không tiếp tục lặp test tự động.


## Kết quả cuối đã được duyệt để commit
Ngày12/11/2026: người dùng xác nhận Claude đã chạy EditMode trên working tree CUỐI:74/74 Pass, job20982073bbdc4378b4556c39da5e5735. Codex đọc lại SessionState qua MCP xác nhận status=succeeded, total_tests=74, completed_tests=74, failures_so_far=[], error=null; lưu editmode-claude.json. Đây là lượt Claude chạy, không ghi là Codex tự chạy. PlayMode cuối do Codex chạy:22/22, job871e4cd3e321479193d830ed892dbd31, playmode-final.json. Không sửa code sau các kết quả cuối này. Các đoạn dừng/chờ ở trên là lịch sử trước xác nhận của Claude, đã được thay thế bởi kết quả cuối này và phép commit trực tiếp của người dùng.

Không stage ProjectSettings/TimeManager.asset: Unity tự đổi serialization Fixed Timestep (giá trị xấp xỉ0.02), không thuộc UI-POLISH. Cũng không stage bất kỳ material/Logs/csproj/slnx/ProjectSettings/file ngoài phạm vi. Scene index chỉ patch album2field;6hunkMinSize/MaxSize noise giữ ở working tree. Review code không có lỗi chặn; phần noise scene đã loại trong patch.

