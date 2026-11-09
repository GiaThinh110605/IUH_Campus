# Hướng dẫn cho AI agent — IUH Campus Guardian

File này dùng chung cho mọi agent (Codex, Claude Code, ...). Trả lời người dùng bằng tiếng Việt.

## Dự án
- Unity 6000.5.8f1, URP. Scene chính: `Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity`.
- Nền tảng: Windows/macOS và iOS (iPhone, ARKit). Android là hướng phát triển.
- Backend: `Server/` — Flask + Gunicorn + ONNX (YOLOv8n 640) + SQLite + Adafruit IO, Docker Compose (nginx, api, sync, Prometheus, Grafana).
- Kế hoạch: `plan.md`, `plan.txt` (rubric). Thiết kế cách chơi: `Docs/Guardian_Thong_Nhat_Thiet_Ke_20261104.md`.
- Bàn giao trạng thái mới nhất và việc tiếp theo: `Docs/BAN_GIAO_CLAUDE_CODE.md` (đọc đầu tiên).

## Luật bắt buộc — đọc trước khi làm
- `.agents/rules/coding-standards.md`, `unity-standards.md`, `match3-combat.md`, `w9-backend-cloud.md`.
- Skills trong `.agents/skills/` (đọc skill liên quan trước khi làm việc tương ứng). Luật dự án luôn ưu tiên hơn skill.

## Phân vai
- Gia Thịnh: scene campus, flow, combat, AR/iOS, build Mac/iOS, `IUHGuardianInventory` (hệ tiền/kho đồ duy nhất).
- Minh Nhật: backend, Docker, cloud, thư viện Unity `Scripts/Guardian/Backend/`, kiểm thử/báo cáo backend.
- Chỉ một người sửa scene campus tại một thời điểm. Sửa code/scene của người kia → hỏi trước.

## Git
- Làm trên nhánh riêng; không commit thẳng vào `main`; không force push; không push khi chưa được yêu cầu.
- Stage chọn lọc (không `git add -A`/`git add .`). Không stage thay đổi Unity tự ghi chỉ do xuống dòng (csproj, slnx, Logs, ProjectSettings) hoặc file không thuộc việc đang làm.
- Không commit secret (`Server/.env`), model lớn ngoài `yolov8n.onnx`, ảnh thật trong `Server/tests/fixtures/real/`, pack asset lớn chưa được duyệt.
- Asset Unity luôn đi kèm `.meta`. Chỉ xóa asset sau khi tìm GUID không còn tham chiếu.

## Unity
- Trước khi sửa: kiểm tra scene `isDirty=false`, Console. Sau khi sửa: compile xong, Console 0 error, Missing Script = 0, chạy test liên quan.
- Không chạy các builder/automation cũ (đã khóa bằng `IUH_LEGACY_TOOLS`). Không sửa Starter Assets, FBX/GLTF nguồn.
- Không tự tải asset ngoài; theo `.agents/skills/free-asset-sourcing`.

## Kiểm thử & bằng chứng
- Chỉ ghi Pass khi đã chạy thật. Bằng chứng mới lưu ở `Docs/Evidence/CampusGuardian/<lượt>/`, ghi commit, ngày, môi trường.
- Lỗi do chính code mình viết: tự sửa tối đa 3 vòng. Lỗi ngoài phạm vi hoặc cần quyết định: dừng và hỏi người dùng.
