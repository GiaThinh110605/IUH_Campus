# Prompt cho Codex — NPC/linh vật ẩn khi có thảm họa, sửa và trồng bồn hoa

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus`.
Chạy tự động: **không hỏi giữa chừng.** Cần quyết định thì chọn phương án an toàn nhất, ghi lý do. **Không push, không amend.**

## 0. Đầu phiên
- Nhánh phải là `minhnhat/vfx-pass`, HEAD là commit `docs(fix): record playtest fixes` của lượt `Prompt_Fix_Feet_Smoke_Monster_Combat.md`. Sai ⇒ dừng. Làm tiếp trên nhánh này.
- Unity mở, MCP kết nối (thử lại 3 lần, 2 phút mỗi lần). Thoát Play Mode, Console 0 error, scene `isDirty=false`.
- Không stage: dòng `MainCamera` `m_LocalPosition.y`, `*.mat` Unity tự ghi ngoài phạm vi, `*.csproj`, `*.slnx`, `ProjectSettings/`, `Logs/`, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`, file chẩn đoán chưa track cũ.

## 1. Đọc trước
- `AGENTS.md`, `.agents/rules/*`, skill `.agents/skills/urp-graphics-upgrade`, `.agents/skills/free-asset-sourcing`.
- `Docs/Evidence/CampusGuardian/NPC-VROID/README.md`, `VISUAL-POLISH/README.md`, `PLAYTEST-FIX/README.md`.
- `Docs/Evidence/CampusGuardian/ART-ENVIRONMENT/` (lượt A/C: cây cỏ, `NATURE_GROUND`, ngân sách draw calls toàn cảnh).
- Code: `Scripts/Guardian/Story/IUHStoryDirector.cs`, `Core/StoryProgress.cs` (`StoryPhase { Prologue, Incident, Chapter, Ending, Completed }`), `IUHStoryNightShift.cs`, `Scripts/DailyActivities/IUHMascotNPC.cs`, `IUHAmbientStudent`.
- Ảnh người dùng: `Docs/Evidence/CampusGuardian/PLAYTEST-FIX/user-report/` (`01`, `05`, `06`, `07`).

## 2. Người dùng yêu cầu (24/11)
1. **Linh vật IUH và các NPC sinh viên biến mất khi có thảm họa, chỉ hiện lại khi hòa bình.**
2. **Sửa bồn hoa hai bên đài phun nước** (ảnh `01`, `06`: khối cong tối màu, gần như trống, viền bi vàng; nhân vật còn đứng lên được trên đỉnh).
3. **Thêm hoa, cây vào các bồn hoa còn lại**, hiện khá trống trải (ảnh `05`: bồn tròn chỉ vài bông).
4. **Bồn hoa ở khu Ngày hội bị dính vào sân khấu** (ảnh `07`).

## 3. Ngưỡng
- Đo A/B xen kẽ cùng phiên (A, B, A, B, A, B; 120 + 3000 khung hình; 1280 × 900 xác minh bằng `Screen.width/height`; VSync tắt). Góc `lobby`, `overview` + 1 góc đài phun nước. Ghi nguồn điện. Chỉ tsv tóm tắt.
- Mỗi Task: trung vị tăng ≤ **0,8 ms**, p95 ≤ 1,5 ms, draw calls toàn cảnh tăng ≤ 40 cho cả G2+G3 cộng lại. Ưu tiên gộp mesh / GPU instancing / dùng lại vật liệu sẵn có.
- Console 0 error. EditMode và PlayMode tách riêng, toàn bộ Pass. Missing Script = 0 ở 3 scene.

## 4. Các Task

### G1 — NPC và linh vật ẩn khi thảm họa (commit `feat(story): hide students and mascot during disaster`)
- **Định nghĩa:**
  - **Thảm họa:** từ lúc vào `StoryPhase.Incident` (sự cố, chuyển đêm) qua `Chapter` đến hết `Ending` trước khi `night.ToDay`; và **trong lúc mỗi dư chấn (aftershock) đang chạy**.
  - **Hòa bình:** `Prologue`, sau khi trời sáng lại ở cuối `Ending`, `Completed` khi không có dư chấn.
  - Đọc kỹ `IUHStoryDirector` để chọn đúng điểm. Nếu có trạng thái khác (tải lại save giữa chương…) thì suy từ `StoryPhase` khi khởi tạo.
- **Cách làm:** thêm một API/sự kiện nhỏ trong story (ví dụ `IUHStoryDirector.DisasterChanged(bool)` + thuộc tính `IsDisaster`) và một component riêng (ví dụ `IUHPeaceTimeOnly`) gắn lên `NPC_STUDENTS` và gốc linh vật `IUH_MASCOT_ROBOT`, bật/tắt theo sự kiện.
  - **Không sửa logic** `IUHMascotNPC`/`IUHAmbientStudent`; chỉ bật/tắt GameObject (hoặc renderer + collider + tương tác nếu tắt cả GameObject gây lỗi). Ghi lý do.
  - Lúc ẩn: linh vật không tương tác E được, nhãn nổi "LINH VẬT IUH / [E] Nhận Thử Thách" và vòng sáng xanh dưới đất (nếu thuộc linh vật) cũng ẩn. Nhiệm vụ hằng ngày đang dở **không bị mất**; nếu đang mở hội thoại linh vật lúc thảm họa bắt đầu thì đóng hội thoại an toàn.
  - Chuyển trạng thái mượt: có thể tắt ngay khi màn hình đang chuyển đêm (3 s) — không cần hiệu ứng riêng.
  - Code story là phần người dùng (Minh Nhật đã viết `IUHStoryCorruptionZone`); `IUHStoryDirector` sửa tối thiểu (1 sự kiện + vài dòng phát), ghi file/dòng.
- **Test:**
  - EditMode/PlayMode: Prologue ⇒ hiện; Incident/Chapter ⇒ ẩn; Ending sau ToDay/Completed ⇒ hiện; dư chấn bắt đầu ⇒ ẩn, kết thúc ⇒ hiện; khởi tạo từ save ở giữa Chapter ⇒ ẩn ngay.
  - Không NullReference khi thiếu tham chiếu.
- Ảnh: quảng trường lúc hòa bình (có NPC + linh vật) và lúc thảm họa (không có), cùng góc.

### G2 — Sửa bồn hoa hai bên đài phun nước (commit `fix(art): fountain side flowerbeds`)
- Xác định 2 bồn cong hai bên đài phun nước (ảnh `01`, `06`). Ghi tên object, mesh, vật liệu, collider hiện có.
- Sửa để trông như **bồn hoa thật**:
  - Thành bồn (viền) sáng màu đá/bê tông hợp sân lát; mặt đất trong bồn màu đất/cỏ, không còn khối xanh đen đặc.
  - **Trồng kín** hoa + bụi thấp + 1–3 cây nhỏ mỗi bồn bằng asset cây cỏ **đã có trong project** (lượt A/C: hoa, bụi, cụm cỏ). Không tải asset ngoài.
  - Viền bi vàng: giữ nếu hợp sau khi sửa, không thì giảm/bỏ, ghi lý do.
- **Collider:** người chơi **không đi lên được đỉnh bồn** (thêm/chỉnh collider thành bồn đủ cao, hoặc đúng hình bồn). Kiểm không chặn lối đi lát chính, không chặn đường đi của 2 NPC đi bộ.
- Kiểm grounder F1 không nâng nhân vật lên bồn.
- Ảnh trước/sau ngày và đêm, cùng góc ảnh `01`, `06`.

### G3 — Trồng thêm ở các bồn hoa còn trống (commit `feat(art): fill campus flowerbeds`)
- **Rà toàn campus** các bồn hoa/bồn cây (bồn tròn, bồn chữ nhật, dải cỏ viền). Liệt kê vào README: tên, vị trí, độ phủ ước lượng trước/sau.
- Bồn trống hoặc thưa ⇒ thêm hoa nhiều màu (đỏ, vàng, tím, trắng), bụi thấp, cây nhỏ ở bồn lớn. Không để hoa xuyên qua thành bồn hoặc bay khỏi mặt đất.
- Ưu tiên: gộp mesh theo khu hoặc GPU instancing; tắt đổ bóng cho cụm hoa nhỏ; dùng lại vật liệu có sẵn.
- Không đổi cây/cỏ đã có của lượt A/C trừ khi đè lên nhau.
- Ảnh trước/sau: ảnh `05` cùng góc, toàn cảnh `overview` ngày.

### G4 — Bồn hoa khu Ngày hội dính sân khấu (commit `fix(art): separate festival flowerbed from stage`)
- Ảnh `07`: xác định bồn hoa và sân khấu khu Ngày hội. Dời bồn hoa ra khỏi sân khấu (khoảng hở ≥ 0,5 m), hoặc bỏ bồn nếu không có chỗ hợp lý. Không di chuyển sân khấu, check-in, `IUHCampusDailyActivitiesManager`, zone/trigger của Ngày hội.
- Kiểm điểm check-in "Khu Ngày hội" vẫn hoạt động, không chặn lối đi.
- Ảnh trước/sau cùng góc ảnh `07`.

### G5 — Bằng chứng (commit `docs(art): record peace NPC and flowerbeds`)
`Docs/Evidence/CampusGuardian/PEACE-FLOWERBEDS/README.md`:
- Commit, môi trường, nguồn điện.
- G1: định nghĩa thảm họa/hòa bình đã dùng, file/dòng sửa, bảng test trạng thái.
- G2–G4: danh sách bồn, asset dùng, ảnh trước/sau, bảng A/B, draw calls.
- Số test, mọi chỗ lệch. Kèm prompt này.

## 5. Quy tắc tự quyết
- Task quá 30 phút không tiến triển hoặc không đạt sau 2 vòng ⇒ bỏ Task đó: mở lại scene không lưu, xóa asset chưa commit của Task, ghi lý do, làm Task kế tiếp. G1 ưu tiên cao nhất; G5 luôn làm.
- **Chỉ lưu scene khi Task đạt.** MCP mất phản hồi ⇒ ghi `PEACE-FLOWERBEDS/RESUME.md` rồi dừng; **không ghi tay vào file scene**.
- Ảnh tự kiểm: không hồng lỗi shader, hoa không xuyên thành bồn/bay, không chặn lối đi.
- Mỗi Task một commit. Stage chọn lọc kèm `.meta`. Scene: chỉ hunk của Task. Không `git add -A`/`.`.

## 6. Cuối phiên
- Unity về Edit Mode, `isDirty=false`, Console 0 error.
- Tin nhắn cuối ngắn gọn: commit; Task xong/bỏ (lý do); bảng A/B; link ảnh hòa bình/thảm họa, bồn đài phun nước, bồn trồng thêm, khu Ngày hội.
