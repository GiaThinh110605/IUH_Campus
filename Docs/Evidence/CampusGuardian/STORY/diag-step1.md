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
