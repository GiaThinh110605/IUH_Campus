# SCENE-RESTORE — khôi phục milestone, B8 và trạm

Hoàn tất12/11/2026, Asia/Bangkok. Windows Editor Unity6000.5.8f1, URP, MCP HTTP. Nhánh minhnhat/fix-shop-skins-scroll; nền bdc6181 (UI-SKINS đã amend từ4356081 theo yêu cầu). Commit SCENE-RESTORE là commit chứa README này, thông điệp fix(guardian): restore milestones and shop fixes lost in mobile scene rebuild. Không push/PR. UI-POLISH chưa bắt đầu.

## Nguyên nhân và milestone
Commit c1c40a5 (#33 mobile) dựng lại scene, mất thay đổi B8/UI-SHOP trước đó. Đã đọc scene thực tế qua MCP trước sửa, đối chiếu e8759cc cho trạm và IUHARSceneBuilder.cs dòng182–226 cho ID AR. Không sửa/chạy builder.

| Object | Trước | Sau | Core |
|---|---|---|---|
| MONSTER_01_BLUEDEMON | rỗng | campus_cong_chinh |10 cũ|
| MONSTER_02_ORC | rỗng | campus_cantin |10 cũ|
| MONSTER_03_PHANTOM | rỗng | campus_le_hoi |10 cũ|
| MONSTER_04_DEMON_BOSS | rỗng | campus_dau_truong_bac |10 cũ|
| MONSTER_05_CORRUPTED_BACKPACK | rỗng | campus_cong_ar |10 cũ|
| CORRUPTED_ENCOUNTER_FOUNTAIN | CAMPUS_CORRUPTED_FOUNTAIN |giữ nguyên|10 mới|
| CORRUPTED_ENCOUNTER_GARDEN | CAMPUS_CORRUPTED_GARDEN |giữ nguyên|10 mới|
| CORRUPTED_ENCOUNTER_BUILDING_E | CAMPUS_CORRUPTED_BUILDING_E |giữ nguyên|10 mới|
| CORRUPTED_ENCOUNTER_CANTEEN | CAMPUS_CORRUPTED_FOUNTAIN |CAMPUS_CORRUPTED_CANTEEN|10 mới|
| CORRUPTED_ENCOUNTER_SPORTS | CAMPUS_CORRUPTED_FOUNTAIN |CAMPUS_CORRUPTED_SPORTS|10 mới|

10/10 milestone sau có rewards.json. Giữ key cũ, thêm5 key hoa mỗi key10 Core. Không có test sẵn đối chiếu rewards.json với scene nên ghi bảng theo yêu cầu, không thêm test mới. RewardGold của từng encounter không đổi.

## Reference B8 được cho phép bổ sung
| Reference | Trước | Sau |
|---|---|---|
| GUARDIAN_BACKEND/IUHGuardianBackendReporter.m_CombatManager | null | Match3CombatManager fileID1374202037 |
| 5 encounter AR.m_CombatManager | null | giữ null, Start tự tìm manager |
| 5 quái gốc.m_CombatManager khi kiểm sau |1374202037|giữ nguyên|

Reporter null khiến thắng không tạo event. Người dùng cho phép sửa riêng reference này sau khi phát hiện. Xác nhận GlobalObjectId.targetObjectId1374202037 đúng type IUHCampus.Guardian.Match3.Match3CombatManager trước khi gán. references.txt ghi chi tiết; IUHMonsterEncounter.Start có FindAnyObjectByType khi null nên không sửa reference AR/code encounter.

## Trạm và phạm vi code
TxtGold stretch(0,0)–(1,1), sizeDelta(-24,0), MiddleLeft; title/current/next pivot trái, Current rộng200 theo cách UI-SHOP e8759cc. Bỏ emoji trong trạm, giữ dấu đóng✕ hiển thị đúng. IUHUpgradeShopUI.cs chỉ bỏ tiền tố sparkle ở tên item runtime (thay đổi đã được người dùng yêu cầu đưa vào commit này); không đổi logic/giá/inventory/customizer/mobile/backend source.

scene.diff là patch CHỈ milestone + reporter + trạm đưa vào index. Sáu hunk m_MinSize/m_MaxSize do Unity tự ghi bị loại khỏi patch, giữ nguyên working tree và không Save scene trong lượt hoàn tất. Không stage material/Logs/csproj/slnx/ProjectSettings/.env hoặc file ngoài phạm vi. Snapshot scene thô tạm đã bỏ sau khi đối chiếu patch; ảnh probe và log lỗi giữ làm lịch sử, chỉ ảnh restored là bằng chứng trạm cuối.

## Kiểm chứng thực tế
| Mục | Kết quả |
|---|---|
| pytest Server |134 passed in17.09s; venv phải chạy ngoài sandbox do Python nền bị chặn trong sandbox |
| EditMode |74/74 pass, job8c18bdb6d1034074842ff2fd64349a43, editmode.json |
| PlayMode |22/22 pass, job0382ddeb55d34b7d899a647c84957a28, duration5.7466252s, playmode-final.json |
| Thắng quái gốc sau nối B8 |Gold305→365(+60), modal=True, outbox combat_won/campus_cong_chinh |
| Thắng quái AR CANTEEN |Gold365→465(+100), modal=True, outbox combat_won/CAMPUS_CORRUPTED_CANTEEN |
| API Docker |POST hai JSON event thật từ manual-outbox: HTTP201 cả hai, status recorded, core_awarded10 mỗi event; api-events.json |
| Trạm desktop/iPhone |shop-restored-desktop.png 1920×1080 và shop-restored-iphone.png2532×1170: Gold trong thanh, tiêu đề không emoji, nút✕ rõ; đã xem trực quan |
| Kiểm tra cuối sau reconnect |ready_for_tools=true, ngoài Play, is_compiling=false; scene isDirty=false; manage_scene validate(auto_repair=false) missingScripts0, brokenPrefabs0, totalIssues0; read_console(error)0; không có InitTestScene |

PlayMode22/22 chạy sau sửa glyph đóng, trước bổ sung wiring reporter; sau wiring đã kiểm tích hợp gameplay/outbox/API như trên. Lần chạy bổ sung sau wiring ngắt trước trả job_id, không ghi Pass mới. Đã chờ30 giây, đọc editor/state và SessionState: current_job_id=null, latest0382…succeeded; poll lại job0382…succeeded,22 ca không failure (job-reconnect-audit.json). Có một lần auto-review chặn chạy bổ sung vì cần poll job cũ trước; sau kiểm chứng job cũ xong mới thử lại. Timeout cũ180giây/0ca giữ trong playmode-timeout.json, không coi là test đỏ/Pass. Người dùng xác nhận focus Unity giúp chạy test; Claude cũng báo22/22 độc lập, không thay thế bằng chứng tự chạy.

Thủ công dùng StartNewCombat với encounter thật, ép Outcome=Victory qua reflection và duyệt VictoryRoutine như G1, không gọi claim trực tiếp, không đổi milestone test. Tạm disable Update encounter để không đổi đích; dùng outbox riêng manual-outbox.json qua reporter đã đăng ký CombatFinished. Gửi chính JSON của event đó tới API Docker bằng HTTP, không khẳng định đã kiểm tự flush Unity. AR là encounter scene trong Editor, không phải ARKit thiết bị. Modal/trạm screenshot dùng tắt modal combat và dừng coroutine runtime để trạm không bị che; không lưu thay đổi runtime. Các ảnh shop-desktop/shop-iphone/probe trước đó bị modal che, không dùng làm kết quả cuối.

Docker: container thử iuh-scene-restore-api từ imageb6, mount app/config hiện tại readonly, DB/tmp/scene-restore.db tạm, không credential/không syncAdafruit. Đã stop container và docker desktop stop exit0. Không gửi cloud thật.

Cleanup: PlayerPrefs35/35 key, tổng35 khôi phục theo snapshot (MCP rồi kiểm registry ngoài sandbox khi mất kết nối); outbox thật SHA256 khớp outbox-before.json; EditorSettings khôi phục bản đầu, git diff rỗng; GameView trả index11 và gỡ size tạm. Không còn InitTestScene. Sau khi MCP kết nối lại đã kiểm trực tiếp scene sạch/validate0/Console0, không Save scene. Kiểm tra read-only cuối không thay dữ liệu runtime.

Review độc lập không có lỗi chặn: milestone/config/ref/type đúng, glyph✕ giữ, scene chỉ nhóm được phép, không MinSize/MaxSize trong patch. Không chạy build standalone/iOS, không làm UI-POLISH.

CẢNH BÁO: chạy lại IUHARSceneBuilder có thể ghi đè milestone và UI đã khôi phục lần nữa; cần báo Thịnh. Không sửa builder trong lượt này.
