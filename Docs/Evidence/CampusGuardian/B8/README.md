# B8 — backend gameplay

Ngày: 09/11/2026 (Asia/Bangkok). Người thực hiện: Codex, phần Minh Nhật.
Nhánh: `minhnhat/b8-backend-gameplay`, xếp chồng P1-REMOVE `9e884e4` vì chưa merge
khi bắt đầu. Nền origin/main lúc bắt đầu: `6432dc0`.
Commit B8: commit chứa thư mục bằng chứng này; tra
`git log -1 --format=%H -- Docs/Evidence/CampusGuardian/B8/README.md`.
Các phép đo chạy trên working tree B8 trước commit, không phải checkout P1 gốc.

Môi trường: Windows, Unity 6000.5.8f1 Editor + MCP, Docker Desktop/CPU,
Flask/Gunicorn/ONNX thật, Adafruit thật. Không build hoặc kiểm thiết bị.
Venv giữ nguyên; lỗi Python lượt đầu là sandbox không đọc được AppData,
không phải venv hỏng. Chạy lại ngoài sandbox được 134 passed.

Kế hoạch đã thực hiện: test lớp tạo event → nối CombatFinished/outbox và overlay
theo StatusChanged → config/scene qua MCP → EditMode/pytest/Docker/E2E →
thoát Play, compose down, rà phạm vi và commit chọn lọc một lần.

## Kết quả kiểm chứng

| Bước | Kiểm chứng | Kết quả thực tế | Kết luận |
|---|---|---|---|
| a | Compile/Console/scene/config | Console 0 error; validate missingScripts=0, brokenPrefabs=0; scene isDirty=false sau Play; 49 roots; GUID config bc9e90d2181400a488963038178ce8c7 được scene tham chiếu | Pass |
| b | Toàn EditMode sau đổi chữ UI | MCP job bbb92252e772465e9e19bd1311fa0e90 hoàn tất 69/69 ca, liệt kê đúng một failure ProcessStep_CountsPurifyTiles (expected 3, actual 0), có sẵn từ e5aecbb theo bàn giao; result=null nên không tự dựng summary | Không có fail mới được MCP báo; không ghi Pass toàn suite |
| c | Server pytest ngoài sandbox | 134 passed in 16.87s, exit 0; không sửa venv | Pass |
| d | Docker build và /health | up -d --build exit 0; /health healthy, model_loaded=true, db_ok=true, cloud_configured=true | Pass |
| e | Play Mode → reporter → outbox → API → Adafruit | Won/lost/won lại cùng phiên và milestone: recorded, Core 10/0/0; cả ba GET cloud=synced; queue 3→0; ảnh status.png có dòng Máy chủ đúng chữ | Pass |
| f | Thua trận thật do hết giờ | MONSTER_05_CORRUPTED_BACKPACK tự vào trận theo vị trí người chơi, không sửa timer/outcome; remaining=0, Outcome=Defeat, modal hiện; đúng 1 combat_lost cho campus_cong_ar trong phiên; API recorded/Core0/cloud=synced; quan sát lại số event không tăng | Pass (Editor timeout) |
| g | Hồ sơ | README, bảng B8, raw MCP, pytest, health, e2e, status.png và defeat.png | Đã ghi |
| h | Tắt Docker | compose down exit 0, 5 container/network bị gỡ, không -v | Pass |

Test mới: 14 test ban đầu fail vì adapter chưa tồn tại (unity-red.json);
sau thêm adapter, job c88b9393d4d44c6bada1939b2c043a74 trả
total=14/passed=14/failed=0. Lượt EditMode cuối bao gồm các ca này.
Lỗi Match3 cũ được giữ nguyên, không sửa hoặc che khỏi kết quả.

## Scene và milestone

| GameObject | MonsterName | Milestone |
|---|---|---|
| MONSTER_01_BLUEDEMON | TIỂU QUỶ BĂNG LAM (DỄ) | campus_cong_chinh |
| MONSTER_02_ORC | CHIẾN BINH QUỶ DỮ LIỆU (VỪA) | campus_cantin |
| MONSTER_03_PHANTOM | BÓNG MA GLITCH FESTIVAL (KHÁ) | campus_le_hoi |
| MONSTER_04_DEMON_BOSS | ĐẠI MA VƯƠNG HẮC ÁM (ĐẠI BOSS) | campus_dau_truong_bac |
| MONSTER_05_CORRUPTED_BACKPACK | TIỂU QUỶ BALO NHIỄM ĐỘC (AR) | campus_cong_ar |

Mỗi milestone cấu hình tạm 10 Core tại server. Giữ library_intro/ar_monster.
Hậu tố ar chỉ phân biệt encounter sẵn có; B8 không làm AR.

Diff scene: 68 dòng thêm, chỉ 5 milestone, root GUARDIAN_BACKEND gồm service,
reporter, Transform và entry SceneRoots; không chỉnh YAML thủ công.
Config khởi tạo từ menu Assets/Create/IUH Campus/Guardian Backend/Config qua MCP,
lưu instance menu tạo bằng AssetDatabase.CreateAsset. baseUrl localhost:5001,
useFakeScan=false. Data.meta đã được track từ nền, giữ nguyên GUID; bổ sung
Guardian.meta, Backend.meta và config.asset.meta. Trong Data/ chỉ có cây Backend
và các .meta này, không có asset khác.

## Sự kiện thật

Session: `32a4f2fa-f02a-4562-a1e0-f48a734b1d6b`.

| Nguồn | Event ID | Loại | Milestone | Core | API / cloud |
|---|---|---|---|---|---|
| Gọi handler reporter | e05f25ee-db01-4408-a894-2ffcce60dde0 | combat_won | campus_cong_chinh | 10 | recorded / synced |
| Gọi handler reporter | 4ca18d68-e3a4-4d7c-bb30-7a9460f98d21 | combat_lost | campus_cong_chinh | 0 | recorded / synced |
| Gọi handler lại cùng milestone | 1e22a756-3b10-4728-80ab-c1203b30931f | combat_won | campus_cong_chinh | 0 | recorded / synced |
| Hết giờ trận thật | 24dbed64-5f98-43f7-a792-0fb86f46c4c5 | combat_lost | campus_cong_ar | 0 | recorded / synced |

GET /gameplay/events/<id> cho từng event có đúng các giá trị trên; e2e.json
lưu response, quan sát runtime và đối chiếu SQLite read-only. Chỉ quan sát cloud
synced qua API; không suy ra cloud từ client Synced/ACK. status.png hiển thị đã
nhận 3 trước timeout; defeat.png hiển thị đã nhận 4 sau timeout.

## Vận hành và giới hạn

Sau recreate API, nginx giữ IP cũ .5 trong khi DNS là .4, làm /health trả 502.
Đối chiếu DNS và health nội bộ xác nhận API khỏe; reload nginx không sửa config,
sau đó /health HTTP200. Xem cleanup.json. Lúc thoát Play có một truy vấn validate
trùng domain reload mất kết nối; retry thành công, không có thay đổi scene.

Overlay: `Máy chủ: chờ {pending} · đã nhận {n} · lỗi {m}`, khởi tạo
`Máy chủ: đang khởi tạo`. Đã nhận là API Recorded/Duplicate trong vòng đời
reporter, không phải Adafruit synced. UI không nhận raycast, không Update polling.
Không sửa Gold, kho đồ, cân bằng, AR, realtime combat, DailyActivities hoặc settings.

Chưa chơi thắng một trận thật; won được kích hoạt qua handler theo kịch bản E2E
được yêu cầu. Chưa kiểm riêng Flee/retry bằng thao tác UI, build, thiết bị, mất mạng
vật lý hoặc AR. Có test fake cho thư viện ở EditMode, không gọi đó là cloud thật.

Review độc lập chỉ đọc không phát hiện Critical/Important trong phạm vi B8.
Giữ nguyên và không stage/revert csproj/slnx/Logs, ba material, .vsconfig,
Assets/Screenshots/ và match3-combat-revamp.patch.
