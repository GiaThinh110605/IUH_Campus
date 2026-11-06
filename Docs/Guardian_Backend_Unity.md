# Guardian backend client — hướng dẫn cho Thịnh

Namespace `IUHCampus.Guardian.Backend`, assembly `IUHCampus.Guardian.Backend`. Không tham chiếu Assembly-CSharp, Guardian flow, Match3 hoặc Minigames. B5 chỉ cung cấp thư viện: chưa gắn component hoặc tích hợp UI/gameplay, chưa chạy Play Mode/Android.

## Thiết lập sau khi nhận thư viện

1. Tạo config qua Assets > Create > IUH Campus > Guardian Backend > Config; đặt timeout, retryBaseSeconds, maxBackoffSeconds, maxAttempts, maxImageSide=640, jpgQuality=80.
2. Tạo GameObject root riêng rồi thêm GuardianBackendService, gán config trong Inspector. Service tồn tại qua scene bằng DontDestroyOnLoad, chỉ cho một instance; B5 không tự thực hiện bước này.
3. Editor dùng http://127.0.0.1:5001. Điện thoại cùng Wi-Fi phải dùng IP LAN máy Docker, ví dụ http://192.168.1.10:5001 (thay bằng IP thật), mở firewall TCP5001 nếu cần. `GuardianBackendConfig.SetBaseUrl(url)` ghi override vào PlayerPrefs `iuh.guardian.backend_url`; override ưu tiên config. Xóa key này để về URL config. Không đặt secret vào URL/Unity.
4. Thịnh tự cấu hình Android PlayerSettings: Internet Access=Require (INTERNET), Allow downloads over HTTP theo chính sách demo hoặc dùng HTTPS; khai báo CAMERA và xin quyền camera lúc chạy trước khi dùng WebCamTexture. B5 không sửa ProjectSettings/Packages. HTTP không mã hóa, chỉ dùng mạng demo tin cậy.

## Ví dụ nối vào gameplay (17 dòng)

Đặt các hàm dưới trong adapter của Thịnh; `backend` là GuardianBackendService đã gán, `frame` là ảnh camera, `token` thuộc vòng đời UI. Hàm mở trận/UI tự nối tại comment. Không cộng Core từ client: đọc CoreAwarded do backend trả về; event outbox chỉ báo đã được API nhận, chưa bảo đảm cloud đã synced.

```csharp
using IUHCampus.Guardian.Backend;
using System.Threading;
using UnityEngine;
async Awaitable Scan(GuardianBackendService backend, Texture2D frame, CancellationToken token)
{
    byte[] jpg = GuardianImageEncoder.Encode(frame, 640, 80);
    var result = await backend.Client.PredictAsync(jpg, "book", token);
    if (result.Status != GuardianRequestStatus.Success || !result.TargetFound) return;
    backend.Outbox.Enqueue(GuardianEventFactory.ScanConfirmed(
        "library", "library_intro", "book", result.BestConfidence.Value));
    // Thịnh mở trận ở đây sau khi event đã lưu bền vững.
}
void Won(GuardianBackendService backend) => backend.Outbox.Enqueue(
    GuardianEventFactory.CombatWon("library", "library_intro"));
void Upgrade(GuardianBackendService backend) => backend.Outbox.Enqueue(
    GuardianEventFactory.UpgradePurchased("library", "library_intro", "attack", 1));
```

Mua nâng cấp vẫn cần logic kiểm tra/ủy quyền riêng: backend hiện ghi telemetry, chưa trừ số dư. `library_intro` hiện là milestone thưởng tạm 10Core; cần nhóm chốt. CombatLost có cùng quest/milestone và không có payload. Scan chỉ chấp nhận nhãn book/laptop/chair/bottle/backpack/cell phone và confidence tối thiểu0.70 (backend cấu hình được). Ngưỡng inference mặc định backend0.70, client không ghi đè.

## API và JSON

- `GetHealthAsync(token)` → HealthResult: Status, HttpStatus, ModelLoaded, DbOk, CloudConfigured, OutboxPending/Failed. `/health`503 khi DB lỗi; model chưa nạp có thể200 với ModelLoaded=false, phải kiểm tra cả hai.
- `PredictAsync(jpg, targetClass, token)` multipart `image` + tùy chọn `target_class`; PredictResult có TargetFound, BestConfidence nullable, Detections, InferenceMs, Status/HttpStatus/Error. Bbox [x,y,w,h] theo pixel ảnh JPG đã gửi, không phải frame camera gốc. BestConfidence=null khi không có detection khớp; được tính từ detection cùng quy tắc backend vì JsonUtility không có nullable float. Dùng targetClass=null để không chọn mục tiêu; backend trả TargetFound=false.
- `PostEventAsync(value, token)` → Outcome Recorded201 / Duplicate200 / Invalid400 / Conflict409 / NetworkError (kể cả timeout transport) / ServerError5xx. 408/429 được giữ như lỗi tạm. Body lỗi giữ Error/Field nếu là JSON; proxy có thể trả HTML. Không throw cho HTTP/network thông thường; CancellationToken của caller bị hủy sẽ throw OperationCanceledException, caller xử lý riêng. Lỗi lập trình/cấu hình/file I/O không bị che thành lỗi mạng.

Event có đúng event_id, session_id, quest_id, milestone_id, event_type, occurred_at; scan thêm payload{label,confidence}, upgrade thêm payload{branch,level}; combat bỏ payload (backend cho phép thiếu hoặc{}). Không gửi core/thưởng. UUID tạo một lần trong factory, JSON bất biến; session UUID mới mỗi app run, kể cả Editor tắt domain reload. Các event cũ trong outbox giữ session cũ qua restart. quest/milestone phải khớp [A-Za-z0-9_-]{1,64}, ngày UTC ISO8601 không quá tương lai5phút; unknown combat-won milestone trả400. Upgrade branch attack/defense/vitality, level1..2.

## Outbox và vòng đời

Một instance sở hữu `persistentDataPath/guardian_outbox.json` trên Unity main thread. Enqueue ghi file tạm, flush ra đĩa rồi File.Replace/Move; trả về sau khi lưu. Không tạo hai owner cùng path. Bản lưu còn nguyên nếu app đóng lúc HTTP chưa ACK; replay cùng UUID được backend dedupe. Flush tuần tự; gọi Flush khi đang chạy bị bỏ qua. FIFO nghiêm ngặt: event đầu đang backoff hoặc hết maxAttempts chặn các event sau. Network/5xx tăng backoff mũ, chặn trần, giữ event; hết maxAttempts phát Failed nhưng không xóa. `RetryFailed()` reset retry của hàng pending, giữ UUID/JSON, để người dùng thử lại. Recorded/Duplicate xóa; Invalid/Conflict chuyển rejected bền vững, warning một lần lúc chuyển. File JSON hỏng được đổi tên .corrupt-UUID để giữ điều tra rồi bắt đầu queue rỗng. File I/O lỗi được báo, không xóa queue cũ để né lỗi.

Đăng ký `Outbox.StatusChanged += (eventId, status) => ...` cho UI (Pending/Synced/Failed). Synced ở client nghĩa là API đã Recorded/Duplicate; Cloud từ API là pending/synced/failed/disabled riêng, không đồng nhất với trạng thái local. Service tự flush mỗi1giây (không phụ thuộc Time.timeScale) và khi resume; cancel khi destroy. Khi đã hết maxAttempts cần gọi RetryFailed. PendingCount/RejectedCount phục vụ UI; không log mỗi frame/request.

Encoder đọc texture vào buffer riêng, giữ tỉ lệ, không upscale, cạnh dài tối đa cấu hình; trả byte[] JPG, không giữ tham chiếu frame, khôi phục RenderTexture.active và giải phóng buffer. WebCamTexture overload không tự xoay/lật: adapter camera phải xử lý videoRotationAngle/videoVerticallyMirrored và trạng thái sẵn sàng của camera trước khi encode. Android GPU/readback, mất mạng thật, quyền camera và kill app trên máy thật vẫn cần nghiệm thu sau.

## Kiểm tra không đụng scene

Menu `IUH Campus/Guardian Backend/Smoke Test` chỉ chạy khi bấm, yêu cầu Edit Mode và scene sạch. Dùng fixture laptop-stocksnap.jpg (nguồn/license trong Server/tests/fixtures/README.md), gọi health/predict, gửi một scan quest_id=B5_smoke hai lần cùng JSON: Recorded rồi Duplicate, Core0. Đây là telemetry thử thủ công, làm tăng scan tổng thực trên Adafruit khi cloud bật. Menu ghi Docs/Evidence/CampusGuardian/B5/smoke.json, không save scene hoặc config asset.

Chạy EditMode assembly IUHCampus.Guardian.Backend.Tests qua MCP/Test Runner. Test dùng fake, thư mục tạm, giữ/khôi phục PlayerPrefs override; warning rejection/corruption được LogAssert.Expect kiểm tra có chủ đích. Runtime và Editor asmdef không tham chiếu code của Thịnh. Evidence B5 lưu test/smoke/console/scene checks; không có key trong Unity.

## Luồng quét AR

Thư viện không tham chiếu AR Foundation. Thịnh nối ARCameraManager/CPU image vào JPG (cạnh dài ≤640), gọi service rồi cập nhật UI trên main thread. `PredictAsync` B5 giữ nguyên. API mới nhận CancellationToken; cancellation chủ động được truyền ra caller. Lỗi mạng/HTTP thông thường được trả qua Outcome.

```csharp
// Trong MonoBehaviour của AR scene; config là GuardianBackendConfig.
var service = config.CreateScanService();
var gate = new GuardianScanStabilizer(); // 2 khung, 1.5 giây, 0.5 s/khung
var session = await service.StartScanSessionAsync(destroyCancellationToken);
if (session.Outcome == ScanSessionOutcome.Cooldown) {
    ShowWait(session.RetryAfterSeconds); return;
}
if (session.Outcome == ScanSessionOutcome.ActiveExists)
    session = await service.GetScanSessionAsync(session.ScanSessionId, destroyCancellationToken);
if (session.Outcome != ScanSessionOutcome.Created && session.Outcome != ScanSessionOutcome.Found)
    return; // Hiện lỗi/giới hạn, cho người chơi thử lại khi phù hợp.
ShowHint(session.Hint);
// Mỗi frame AR: chỉ lấy CPU image khi gate cho phép.
if (!gate.TryBeginFrame(Time.realtimeSinceStartupAsDouble)) return;
try {
    byte[] jpg = CaptureCpuImageAsJpg(maxSide: 640); // Dispose CPU image sau chuyển đổi.
    var frame = await service.ScanFrameAsync(jpg, session.ScanSessionId, destroyCancellationToken);
    bool confirmed = gate.Observe(frame, Time.realtimeSinceStartupAsDouble);
    ShowAnalysisProgress(gate.Progress);
    if (!confirmed) return;
    if (frame.Outcome == ScanFrameOutcome.Clean) ShowStatus("✓ An toàn");
    if (frame.Outcome == ScanFrameOutcome.Infested) {
        PlaceMonsterOnPlane(frame.MonsterToken); // Giữ token trên monster này.
        // Khi thắng match-3: outbox.Enqueue(GuardianEventFactory.CombatWon(
        //     "ar_scan", "ar_monster", frame.MonsterToken));
    }
} finally { gate.EndFrame(); }
```

Các hàm Show/Capture/Place trong ví dụ là điểm nối của scene, không thuộc thư viện. Không lấy lại cùng Awaitable để await nhiều lần. Stabilizer chỉ phát xác nhận một lần cho chuỗi cùng label/result; reset khi đổi label/result hoặc kết quả không hợp lệ. Gọi `Reset()` khi đổi phiên hoặc quay lại quét sau combat. NoObject reset phân tích; SessionNotFound/SessionExpired cần tạo phiên mới theo UI. GET trả Found, SessionNotFound hoặc SessionExpired; MaxMonsters chỉ có ở POST, GET dùng MonstersRemaining/RevealedCount.

Bật `useFakeScan` trên config để dùng FakeScanService trong Editor/Development build; cấu hình `fakeInfestedClasses`, `fakeLabels`, `fakeHint`. Mỗi request tiêu thụ một label đầu vào, hết danh sách trả NoObject. Fake không mạng, token ổn định theo label trong phiên; không gửi token giả lên backend thật. Release build từ chối khởi tạo Fake.

Menu **IUH Campus → Guardian Backend → B7 Scan Smoke** chỉ chạy khi bấm, scene sạch, Editor không Play. Bật Docker stack trước; kết quả lưu `Docs/Evidence/CampusGuardian/B7/smoke.json`. Smoke không in danh sách lớp nhiễm.

Trên iOS dùng IP LAN của máy chạy server thay localhost. Thịnh xử lý `NSAllowsLocalNetworking` trong ProjectSettings/Info.plist; thư viện này không sửa cấu hình iOS.
