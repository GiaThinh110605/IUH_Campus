# Chẩn đoán Match3 — Bước 1 (chưa sửa hành vi, không commit)

16/11/2026, Windows, Unity 6000.5.8f1, base de7bdd4.
Chỉ thêm log vào đầu StartNewCombat và catch reporter. Không sửa IUHARCorruptedObjectEncounter.cs.

Setup: sao lưu save gốc ở diag-progress-original.txt. F9 qua desktop chưa có tác dụng xác nhận; QueueStateEvent Keyboard F9 qua Input System đã xóa khóa save (đọc lại <absent>). Người dùng cho phép nạp checkpoint chapter 2 chưa thắng Garden/Orc. Sau lượt thử đã thoát Play, timeScale=1, khôi phục save gốc, scene isDirty=false; diff scene vẫn chỉ 6 dòng font.

(a) Garden: có lần vào qua E với trace IUHARCorruptedObjectEncounter.HandleEngagementInput:157 -> StartCombatEncounter:331 -> StartNewCombat engaged=False. Một lượt khác vào qua IUHMonsterEncounter.TriggerCombat bằng MCP để đặt điều kiện ban đầu, rồi gửi Escape bằng KeyboardState qua Input System. Sau ESC: manager engaged=False, active=False; Garden AR m_InCombat=True, normal encounter m_IsEncounterActive=False. Thử E không vào lại (engaged=False active=False). Chưa cô lập đủ phép thử click sau ESC: có input desktop của người dùng xen vào, nên không ghi Pass/Fail riêng cho click sau ESC.

Có log gọi lặp thực tế: StartNewCombat engaged=True enc=CORRUPTED_ENCOUNTER_GARDEN, từ IUHARCorruptedObjectEncounter.HandleEngagementInput:171 (nhánh click/raycast) -> StartCombatEncounter:331. Xem diag-reentry-stack.txt, trích Editor.log gốc; không gọi trực tiếp StartNewCombat để tạo log này. Trận trước lần gọi này được setup qua TriggerCombat bằng MCP, vì vậy không gán kết quả này cho riêng bài thử Orc.

(b) Orc: vào bằng cách dịch người chơi cạnh MONSTER_02_ORC; log engaged=False từ IUHMonsterEncounter.Update:214 -> TriggerCombat:243. Desktop click 3 lần lên ô bàn cờ và Space; lượt đó hết thời gian trước khi hoàn tất chuỗi Space. Lượt tách riêng: vào Orc lần nữa, tạm timeScale=0 chỉ để cô lập input khỏi timeout, gửi hai lần Space down/up qua KeyboardState. Không có StartNewCombat thứ hai trong lượt tách riêng, chỉ log engaged=False enc=MONSTER_02_ORC (diag-orc-input.json). CHƯA tái hiện được gọi lặp trong riêng tình huống Orc; không dùng log Garden để kết luận Orc đã tái hiện.

(c) InvalidOperationException:
Message: Backend outbox is unavailable.
Thrown: IUHGuardianBackendReporter.HandleCombatFinished, line 62.
Caller: Match3CombatManager.ReportOutcome -> DefeatRoutine -> Update (hết thời gian).
Runtime query: có hai IUHGuardianBackendReporter cùng tên GUARDIAN_BACKEND; một m_Service=null/outbox=False, một service=GUARDIAN_BACKEND/outbox=True. Chưa truy ra nguồn tạo reporter thiếu service. Xem diag-backend-errors.json (có cả một entry diag được Console phân loại Exception từ lần MCP gọi TriggerCombat; không gộp entry đó với InvalidOperationException backend).

Đây là kết quả chẩn đoán có giới hạn, chưa phải xác nhận toàn bộ (a)/(b). Chưa sửa Flee, guard StartNewCombat, reporter binding hoặc skybox. Không commit, không push. Hai log tạm còn ở working tree để tiếp tục tái hiện; không stage.

## Bước 2 — đang thực hiện

Mục 1: thêm ReportOutcome(false) đầu Flee. RED test FleeReportsLossExactlyOnce: expected 1, actual 0 (785e14a24c0344afb47292c18088d97b). Sau sửa: EditMode Match3+Story 32/32 Pass (a8fbad02b98d414198b5e9db3a6644e5), PlayMode 24/24 Pass (4aa66c0888734dc994a7f10d4c1b9077), Console 0 error. Bổ sung bộ Story.PlayModeTests kiểm khóa/khôi phục spawner. Lượt Garden qua MCP còn m_InCombat=True nên chưa ghi Pass ESC→E; không sửa AR. Hai log tạm đã gỡ, reporter sẽ giữ Message trong commit backend riêng.

Mục 1 đã commit d08d633. Mục 2: test RED EngagedFightIgnoresRepeatedStart xác nhận rules bị tạo lại (d42f906096094743944d08702303d956). Đã thêm guard m_IsEngaged + warning, không sửa AR. EditMode 32/32 Pass (d4bf665eee9848c9843f360a42ea1c44).
Mục 2: PlayMode 25/25 Pass (4c5b2c111dbc49c8bd6ebe28e097f893), Console 0 error. Kiểm tay Garden còn cần xác nhận bằng lượt campus sau khi xử lý backend.

Mục 2 đã commit 8c85f96. Mục 3: bỏ DontDestroyOnLoad trong GuardianBackendService, giữ sẵn OnDestroy owner==this -> null. Reporter giữ log error.Message, gỡ Environment.StackTrace chẩn đoán khỏi manager. EditMode Match3+Story+Backend 90/90 Pass (980978d6a2064ec7bb0dbe918b6dd2e9), trong đó Backend 58 test. Kiểm runtime qua MCP: Play + KeyboardState F9, reporter=1, service=1, m_Service assigned=True, outbox=True. Gọi luồng vào Garden rồi Flee: PendingCount 15 -> 16, Console 0 error, không InvalidOperationException. Đây là kiểm luồng gameplay bằng API runtime qua MCP, không phải thao tác thắng trận bằng bàn cờ. Một lượt PlayMode cc79317727124b59b3be25e29eb63159 chưa khởi tạo, không tính Pass; đã chạy lại.
Mục 3: PlayMode Match3+Story 25/25 Pass (8044557dcfaa4b66b39a9b830b7c0123), Console 0 error.

## Bước 2 — kết quả cuối (thay thế các ghi chú chưa xác nhận bên trên)

- d08d633 — fix(match3): report a loss when the player flees. Flee gọi ReportOutcome(false) trước thoát; test gọi Flee hai lần xác nhận chỉ một CombatFinished(false). EditMode 32/32, PlayMode 24/24, Console 0 error.
- 8c85f96 — fix(match3): ignore StartNewCombat while a fight is in progress. Test chứng minh rules và timer không reset khi gọi lặp; warning được kiểm đúng nội dung. EditMode 32/32, PlayMode 25/25, Console 0 error.
- 5812ed7 — fix(guardian): keep backend service scene-local so reloads do not orphan the reporter. Bỏ DontDestroyOnLoad; OnDestroy đã có owner==this -> null và giữ nguyên. Reporter giữ error.Message, không giữ StackTrace log tạm. EditMode Match3+Story+Backend 90/90 (Backend 58), PlayMode 25/25, Console 0 error.
- Mục skybox: clone new Material(originalSkybox), giảm exposure về 25% hoặc dùng Tint nếu không có Exposure. ToDay trả reference gốc; OnDestroy trả reference gốc khi bản clone đang được dùng và hủy clone. Không sửa material asset.

### Kiểm lại Garden sau mục 1–3

Checkpoint chương 2 được người dùng cho phép; đi vào qua IUHMonsterEncounter.TriggerCombat (MCP setup tương đương entry proximity), gửi E/ESC qua KeyboardState của Input System. Lượt đầu chưa xác nhận do AR chưa đủ điều kiện hiển thị quái; không sửa field AR để ép kết quả.

Lượt cuối: sau ESC, AR m_InCombat=False, manager m_IsEngaged=False. Sau khi tiếp tục runtime, quan sát Garden IsPlaced=True, IsMonsterActive=True, IsPurified=False, khoảng cách người chơi 2.0009. E mở lại được: m_IsEngaged=True, IsCombatActive=True. Không còn trạng thái kẹt của lượt chẩn đoán đầu.

Trong trận Garden, tạm timeScale=0 để cô lập input khỏi timeout khi các lệnh MCP chờ; click desktop 4 lần vào cùng ô bàn cờ và gửi Space down/up hai lần bằng Input System. Rules hash giữ -883787424, remainingTime giữ 46.17806, không mở lại/reset trận. Sau đó gọi trực tiếp StartNewCombat khi engaged để kiểm riêng nhánh guard: rules và remainingTime giữ nguyên, xuất đúng warning `[IUH Guardian] StartNewCombat ignored: a fight is already in progress.`. Click/Space trong lượt này không tự tạo warning; warning được xác nhận bằng lần gọi kiểm guard trực tiếp. Console 0 error. Đã trả timeScale=1.

Kiểm backend: Play -> Keyboard F9 -> 1 reporter, 1 service, m_Service assigned=True, Outbox=True. Luồng vào Garden rồi Flee qua MCP làm PendingCount tăng 15 -> 16; không InvalidOperationException. Sự kiện thử được giữ trong outbox theo cơ chế lưu sẵn có.

### Skybox — test và ảnh

RED: NightUsesPrivateSkyboxAndDayRestoresOriginal thất bại vì vẫn cùng material gốc (job a773e82a59104d16bc8f9fb5089c104f).
GREEN: EditMode Match3+Story 32/32 (17db82f6ca2f43b2b31b75fcc03fdd7c); PlayMode 26/26 (c169c4096c5741e78b9c80b78d02d45f), gồm test clone, không đổi Exposure gốc, ToDay restore và OnDestroy restore. Job d9bb015c22f34ac88b48b8b46d50ae8f không khởi tạo được; đã clear stuck rồi chạy lại, không tính job đó là Pass.

Ảnh desktop Scene view cùng góc nhìn: skybox-day.png / skybox-night.png, runtime tạm dừng để giữ cảnh. Skybox campus `Photographic campus sky`, Exposure gốc 0.85; khi đêm dùng `Photographic campus sky (Story Runtime)`, Exposure 0.2125; ToDay(0) khôi phục tên gốc và Exposure 0.85. Đây là ảnh kiểm helper chuyển sáng/tối trên campus, không phải ảnh hoàn tất toàn truyện. Console 0 error, không dùng screenshot MCP.

Kết thúc: thoát Play, khôi phục save từ diag-progress-original.txt, scene isDirty=False, Missing Script=0, 6 dòng font vẫn không stage. Không sửa IUHARCorruptedObjectEncounter.cs; không push. Hai log chẩn đoán tạm đã gỡ. Bộ Story PlayMode mới có 2 test; Match3 PlayMode 24 test.

### Rà soát và sửa hồi quy Retry

Rà soát độc lập phát hiện guard mới khiến cả nút RetryVictory/RetryDefeat bị chặn sau khi trận đã kết thúc nhưng modal còn hiện. Đã thêm RetryCombat cho hai nút: chỉ clear m_IsEngaged khi m_OutcomeReported && !m_IsCombatActive, rồi qua StartNewCombat. Không bỏ guard giữa trận. Đây là commit bổ sung cần thiết để không làm hỏng chức năng retry sẵn có; không amend ba commit trước.

Test RED RetryCanRestartAFinishedFight (9a9f7c272fae417085a6b095bdf522cd) xác nhận thiếu đường retry. Test cuối thao tác Button.onClick thật; một lượt fixture sai tên assembly UI gây TypeLoadException, đã đổi sang tìm type trong assemblies được nạp. Lượt cuối: EditMode 32/32 Pass (3b034ad59f7448dc8b3a63c70efecb48), PlayMode 27/27 Pass (46be40daae884a53a788f5e02eb71b4f); Console 0 error. Tổng cuối Match3 PlayMode 25 + Story PlayMode 2. Rà soát lại không còn phát hiện chặn. Các job lỗi kết nối/khởi tạo/fixture không được tính Pass.
