# OUTBOX-FIX — tiếp tục retry lỗi tạm, không kẹt đầu hàng đợi

Ngày10/11/2026, Asia/Bangkok. Codex, phần Minh Nhật.
Môi trường: Windows, Unity6000.5.8f1 Editor+MCP, Docker Desktop,
Flask/Gunicorn/ONNX/SQLite thật và Adafruit.
Nhánh `minhnhat/fix-outbox-stall` tạo ngay tại HEAD UI-SHOP
`e8759ccdd2a26d5bce48c2feb14cb3ec9a0ba604`, không checkout nội dung nhánh khác.
Commit OUTBOX-FIX: commit chứa README này; tra
`git log -1 --format=%H -- Docs/Evidence/CampusGuardian/OUTBOX-FIX/README.md`.
Test chạy trên working tree trước commit. Không push/PR.

## Nguyên nhân và thay đổi

Head-of-line blocking: head đạt maxAttempts8 bị báo Failed nhưng vẫn ở pending[0].
Flush return ngay khi gặp head này, nên những event phía sau không bao giờ được
gửi nếu không có thao tác reset. Snapshot cũ giữ nguyên trạng thái kẹt qua restart.

GuardianEventOutbox bỏ chốt attempts>=maxAttempts; chỉ chờ nextAttemptTicks.
NetworkError/ServerError giữ Pending và exponential backoff tới cap, tiếp tục
retry khi đến hạn. Bộ đếm attempts bão hòa int.MaxValue để không wrap thành số âm.
Invalid/Conflict vẫn sang rejected/Failed; vòng Flush tiếp tục gửi entry sau.
Không đổi FIFO, event ID, JSON snapshot version1, chữ ký API hoặc server.

GuardianBackendConfig.maxAttempts giữ nguyên field/asset cho tương thích serialized
asset và API cũ; không còn giới hạn số retry transient. Ghi chú trong class remarks;
không sửa config source/asset hoặc thêm Obsolete warning. retryBaseSeconds và
maxBackoffSeconds vẫn điều khiển nhịp retry. RetryFailed giữ API cũ, chỉ là thao tác
reset backoff pending thủ công, không bắt buộc để phục hồi và không gửi rejected.

Reporter bỏ bộ đếm transient Failed; chữ lỗi đọc trực tiếp RejectedCount.
Pending bao gồm event đang retry. Accepted vẫn đếm ACK/status quan sát trong
vòng đời reporter, không thay bằng tổng lịch sử API/cloud.

## Test-first

Thêm4 ca trong GuardianOutboxRetryTests.cs + .meta:
10 NetworkError rồi Recorded, head và entry2 cùng được gửi trong một Flush;
10 ServerError kiểm backoff2s rồi cap3s, chưa đến hạn không gửi;
snapshot version1 có attempts8 vẫn drain; Invalid head rejected rồi entry2 gửi.

Đỏ thật:4 ca hoàn tất,3 failure đúng cap/Failed cũ (tests-red.json).
Sau sửa: toàn assembly Backend58/58 Pass (backend-green.json), gồm4 ca mới.
Test cũ RetryCapAndCancellation buộc phải cập nhật/đổi tên
TransientRetriesAndCancellation vì trước đó khẳng định chính hành vi cap bị bỏ:
3 lần đến hạn có3 request và Pending, giữ nguyên kiểm cancellation và UUID.
Không sửa test Match3 có sẵn.

Trước lượt đỏ hợp lệ có hai lượt khám phá0 test khi asset chưa import/compile.
Không tính các lượt0 là Pass. Đã sửa một lỗi overload NUnit cho enum bằng matcher
collection trước khi chạy đỏ hợp lệ. Lượt full EditMode đầu timeout khởi tạo0 ca
(editmode-init-timeout.json), chạy lại init_timeout120000 cho kết quả bên dưới.

## Bảng kiểm chứng a–e

| ID | Ca | Mong đợi | Thực tế | Kết quả |
|---|---|---|---|---|
| a | Compile/Console | Compile xong,0 error | Editor idle và read_console cuối0 error | Pass |
| b0 | Backend EditMode | Các ca retry/reject/cancellation xanh |58/58 passed, failed0/skipped0 | Pass |
| b1 | Toàn EditMode | Chỉ fail nền được cho phép |73/73 hoàn tất; chỉ ProcessStep_CountsPurifyTiles expected3/actual0; result=null | Có sẵn, đã sửa ở nhánh minhnhat/fix-match3-purify |
| b2 | PlayMode Match3 | Ghi kết quả thật |16/16 hoàn tất; chỉ FreeCursorStopsPendingMovementOnFirstApply NullReferenceException; result=null | Có sẵn, đã sửa ở nhánh minhnhat/fix-match3-purify |
| c0 | Backup và HUD trước | Sao lưu trước gửi thật | outbox-before.json có2 pending, head attempts8; ảnh HUD chờ2/nhận0/lỗi0 | Đã kiểm |
| c1 | Docker/health | Backend thật khỏe | up -d --build exit0; health model_loaded/db_ok/cloud_configured=true | Pass |
| c2 | Play Mode gửi queue thật | Head cũ và event sau được API nhận, chờ0 | HUD chờ0/nhận1/lỗi0; file pending/rejected rỗng; GET cả2 event recorded/cloud=synced | Pass |
| d | Cleanup | Prefs/settings khôi phục, scene sạch; giữ outbox sync |126 key snapshot khớp; EditorSettings diff rỗng; scene isDirty=false, missingScripts0/brokenPrefabs0, diff scene rỗng; compose down exit0 không -v | Pass |
| e | Hồ sơ | Nguyên nhân/diff/test/thật và giới hạn | Raw MCP, backup/after, ảnh, health/manual/prefs/verification/code.diff | Đã ghi |

Hai suite tổng result=null giữ nguyên progress/failures_so_far; không tự dựng
summary passed/skipped, không gọi toàn bộ suite Pass. Không sửa Purify/reflection.

## Queue thật và dữ liệu đã gửi

Sao lưu bằng Copy-Item trước mọi PlayMode gửi thật; không in/read secret.
File thật: `C:/Users/Admin/AppData/LocalLow/DefaultCompany/IUH_Campus/guardian_outbox.json`.
SHA256 bản trước:
`6DD733D3A0B9B28D60C9728B0682509A764499DFE75EADA3DA273279AE7AA25B`.

| Event ID | Loại/milestone | Trước | API GET sau | Core |
|---|---|---|---|---|
|81da8955-ef93-44f3-b6b0-1a1173c51ac5|combat_won / campus_cong_ar|head attempts8|recorded, cloud=synced|10|
|72e73027-2dae-4573-99c6-d56cfab988bf|combat_lost / campus_cong_ar|sau head, attempts0|recorded, cloud=synced|0|

Không gọi RetryFailed, không sửa nextAttemptTicks hoặc xoá event để ép thành công.
nextAttemptTicks cũ đã đến hạn; giữ timestamp/format cũ vẫn gửi tiếp được.
Server Compose trước lượt này không chạy; task bật, kiểm health, rồi tắt bằng
compose down không -v. Không sửa Server/.env, code server, model hoặc rewards.

HUD trước sửa ở phiên mới báo lỗi0 dù file head attempts8, vì reporter cũ không
tái dựng số transient failures pending lúc bind. Bằng chứng kẹt là file attempts8,
pending2 và test đỏ. HUD sau có nhận1 là số status ACK reporter quan sát; không
phải tổng record lưu ở API. Queue rỗng và GET cả2 record xác nhận cả2 đã nhận.
Không suy ra cloud synced từ chữ đã nhận; cloud=synced được đọc thật qua GET.

Giữ outbox thật ở `{version:1,pending:[],rejected:[]}`, không khôi phục backup.
Đây là dữ liệu thật đã gửi đúng. Bản after và hash nằm trong manual.json;
chỉ restore PlayerPrefs, không rollback queue hoặc database/cloud.

## Phạm vi và giới hạn

Giữ public API và JSON schema. Không migrate/reset timestamp snapshot cũ:
nếu nextAttemptTicks còn ở tương lai thì vẫn chờ đúng backoff.
FIFO cố ý vẫn chờ head khi server chưa nhận; lỗi tạm không trở thành điểm dừng
vĩnh viễn nữa. Rejected không tự retry và lỗi I/O/service exception baseline
không được thay đổi trong lượt này. Mô tả maxAttempts cũ ở hồ sơ B5 là lịch sử;
hành vi OUTBOX-FIX theo README này.

Không build hoặc chạy thiết bị thật. Ảnh HUD đã kiểm trực quan.
Review độc lập read-only không phát hiện Critical/Important.
Stage chọn lọc outbox/reporter, test cũ cập nhật+test mới/.meta và OUTBOX-FIX/.
Không stage scene, ProjectSettings, Logs/project files/material/.vsconfig,
Assets/Screenshots/, patch hoặc Server/.env. Commit một lần, không push/PR.
