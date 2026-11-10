# Bàn giao cho Claude Code — IUH Campus Guardian (phần Minh Nhật)

Ngày bàn giao: 09/11/2026. Người dùng: Minh Nhật (khuunhat05@gmail.com, GitHub minhat2502). Đồng đội: Gia Thịnh (GitHub GiaThinh110605, repo chủ).
Trả lời người dùng bằng tiếng Việt, ngắn gọn. Luật chung: `AGENTS.md` + `.agents/rules/*` (luôn ưu tiên). Skills: `.agents/skills/*`.

## 1. Cách làm việc đã dùng (giữ nguyên)
- Người dùng giao việc theo "lượt" (B1, B2…, P1…). Mỗi lượt: nhánh riêng `minhnhat/<tên>` từ `origin/main` → làm → test thật → commit chọn lọc → push → PR vào main → duyệt → merge.
- Trước đây: Claude (chat) giám sát + viết prompt, Codex thực hiện. Từ nay Claude Code đảm nhận cả lập kế hoạch, thực hiện và tự kiểm chứng; nếu vẫn dùng Codex thì Claude Code viết prompt và duyệt.
- Lỗi do code mình viết: tự sửa tối đa 3 vòng. Lỗi ngoài phạm vi/cần quyết định: dừng và hỏi. Chỉ ghi Pass khi đã chạy thật; bằng chứng ở `Docs/Evidence/CampusGuardian/<lượt>/`.

## 2. Quyết định đã chốt
- Nền tảng: PC/Mac + iOS (iPhone, ARKit). Android = hướng phát triển. Build iOS do Thịnh làm trên Mac.
- Thiết kế cách chơi: `Docs/Guardian_Thong_Nhat_Thiet_Ke_20261104.md` (ưu tiên hơn plan.md — plan.md đã cũ, cần cập nhật sau khi hỏi Thịnh).
  - Mobile: mở scan → backend bí mật chọn 1–2 lớp nhiễm trong pool **cell phone, laptop, chair** → giữ camera 1,5 s → đồ nhiễm thì monster hiện trên mặt phẳng AR → chạm vào match-3. Tối đa 1–2 quái/phiên.
  - PC: chăm sóc khuôn viên (trồng/tưới hoa, kho đồ), chống monster bằng match-3.
- AI: YOLOv8n COCO 640, ngưỡng confidence ≥ 0.70, `Server/models/yolov8n.onnx` (AGPL-3.0). Chỉ tiêu độ trễ (B2.7): API total p50 ≤150 ms, p95 ≤220 ms — đo B2.9 đạt (p50 ~59, p95 ~74 ms, ảnh ≤640, Unity đóng).
- Đánh giá ảnh thật B2.6 (33 ảnh, nguồn không rõ/ảnh mạng): laptop 5/5, chair 4/6, cell phone 3/5, bottle/book/backpack 0 → lý do chọn pool 3 lớp.
- Tiền/kho đồ: **chỉ dùng `IUHGuardianInventory` của Thịnh** (Gold). Tên tiền giữ "Gold" (đề xuất, chờ Thịnh xác nhận). Hệ `Scripts/Guardian/Progress` của Minh Nhật (P1/P1.5) → GỠ BỎ.
- Combat chính: match-3 (`Guardian/Match3`). Combat real-time/khinh khí cầu (PR #25 của Thịnh) giữ làm phần phụ — Thịnh quyết.
- Mỗi người dùng tài khoản Adafruit riêng; key chỉ trong `Server/.env` (đã ignore). Không in/commit key.

## 3. Đã hoàn thành (đều đã merge vào main)
| Lượt | Nội dung | Bằng chứng |
|---|---|---|
| B1–B4 | Flask/Gunicorn, /health, Prometheus, Docker (nginx:5001, api, sync, Prometheus 127.0.0.1:9090, Grafana 127.0.0.1:3000) | Evidence B1–B4 |
| B2/B2.5/B2.7/B2.9 | ONNX thật, benchmark, chốt 640, đo lại đúng điều kiện | Evidence B2, D2 |
| B3 | /gameplay/events, chống trùng SQLite, outbox → Adafruit (4 feed + guardian-monsters-revealed) | Evidence B3 |
| B5 | Thư viện Unity `Scripts/Guardian/Backend/` (API client, outbox offline, encoder) | Evidence B5, `Docs/Guardian_Backend_Unity.md` |
| B6/B6.1/B2.8 | Phiên quét ẩn lớp nhiễm, monster token, chống farm (409/429), pool 3 lớp | Evidence B6 |
| B7 | ScanClient/Stabilizer/FakeScanService cho AR | Evidence B7 |
| D2 | Bảng test `Docs/Kiem_Thu_Backend.md`, báo cáo `Docs/Bao_Cao_Ky_Thuat_Backend.md` (134 pytest pass) | Evidence D2 |
| S1 | Skills gamedev (Apache-2.0) | `.agents/skills/gamedev` |
| P1/P1.5 | Ví/kho đồ/UI kho (SẮP GỠ) | Evidence P1, P15 |

## 4. Việc tiếp theo (theo thứ tự)
1. **P1-REMOVE** — gỡ `Scripts/Guardian/Progress/**`, `Data/Guardian/Progress/**`, `Prefabs/Guardian/GuardianInventoryUI.prefab`, icon P1.5 trong `UI/Guardian/Icons`, `Scenes/Test/GuardianInventoryTest.unity`, `Docs/Guardian_Progress.md`. Trước khi xóa: grep GUID toàn Assets = 0 tham chiếu ngoài. Giữ Evidence P1/P15 (thêm dòng "đã gỡ"). Commit kèm `AGENTS.md`, `CLAUDE.md`, file này. Nhánh `minhnhat/remove-progress-p1`, PR vào main.
2. **B8 — nối backend vào gameplay** (lỗ hổng rubric lớn nhất: IO Cloud cần dữ liệu từ lúc chơi thật): đưa `GuardianBackendService` vào scene; thắng/thua match-3 gửi `combat_won`/`combat_lost` qua outbox; hiện trạng thái đồng bộ. Cần sửa `IUHMonsterEncounter` (của Thịnh) → HỎI THỊNH TRƯỚC.
3. **Báo Thịnh các lỗi** Claude Code tìm thấy (code của Thịnh, không tự sửa): kho không lưu `m_Items`; nâng cấp không nguyên tử; thắng match-3 không cộng Gold; nâng cấp không ảnh hưởng match-3; daily activities farm được bằng chỉnh giờ; Find/GetComponent/Camera.main trong Update.
4. **P2 — vườn trồng hoa** (sau khi gỡ P1, dựa trên `IUHGuardianInventory`): 6 luống, trồng/tưới/phân bón, 3 giai đoạn Seed→Sprout→Bloom, mỗi giai đoạn 1 phút (UTC, lớn cả khi tắt game), hái hoa +Gold một lần. Cần Thịnh cho phép thêm hàm hạt giống/phân bón vào `IUHGuardianInventory` và thêm 1 object `GUARDIAN_GARDEN` vào scene. Asset: Kenney Nature Kit (CC0) người dùng tải tay vào `D:\IUH\CongNgheMoi\CK\_downloads\kenney-nature\`.
5. **Hồ sơ/demo**: video 5 bước W9 (export ONNX → API → Docker → (k8s: chưa làm) → Prometheus/Grafana → Adafruit); test iPhone gọi backend qua LAN với Thịnh; dọn dữ liệu test Adafruit trước demo; cập nhật plan.md/plan.txt theo quyết định mới (hỏi Thịnh).

## 5. Bẫy đã gặp (đọc kỹ)
- **Unity MCP**: HTTP `http://127.0.0.1:8081/mcp`. Khi thấy "No Session" → bấm Start Server trong Window → MCP for Unity. Claude Code đã thêm ở scope user (`claude mcp add -s user ...`).
- **CRLF/Unity tự ghi**: `core.autocrlf=true`, không có `.gitattributes`; `Logs/`, `*.csproj`, `*.slnx` vẫn bị track dù có trong .gitignore → luôn báo modified. Không stage, không revert trừ khi lượt yêu cầu. (Cân nhắc lượt riêng: `git rm --cached` Logs/csproj/slnx.)
- Scene campus hay báo modified nhưng diff rỗng khi bỏ qua CR; 3 material diff số thực nhỏ — Unity serialize lại.
- **Chuyển nhánh khi Unity mở** dễ sinh `.meta` thừa (ví dụ `Data.meta` chặn pull). Đóng Unity trước khi checkout/pull lớn.
- **Docker**: cổng 9090 từng bị project khác (`ml-production-demo`) giữ → `docker compose -p ml-production-demo stop`. Grafana cần `GRAFANA_ADMIN_PASSWORD` trong `Server/.env`.
- **Ảnh thật** ở `Server/tests/fixtures/real/` không được commit (đã ignore). Ảnh >5 MB phải thu nhỏ trước khi gửi API.
- Thư mục `.git` có vài file `stale-*` (lock cũ đã đổi tên) — vô hại, xóa tay khi đóng hết công cụ Git rồi `git gc`.
- File thừa ở gốc: `match3-combat-revamp.patch`, `Assets/Screenshots/*` mới — không đụng nếu không được giao.
- `IUHARCorruptedObjectEncounter` (của Thịnh) tham chiếu `Minigames.GreenCampus` — vi phạm cô lập module, báo Thịnh.

## 6. Lệnh hay dùng
```powershell
# Backend
docker compose -f Server/docker-compose.yml up -d --build
curl.exe http://localhost:5001/health
docker compose -f Server/docker-compose.yml down
# Test backend
Server\.venv\Scripts\python -m pytest Server/tests -q
# Đánh giá ảnh thật
Server\.venv\Scripts\python Server/eval_real.py
```
