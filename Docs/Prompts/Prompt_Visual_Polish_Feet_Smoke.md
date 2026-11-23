# Prompt cho Codex — Chỉnh đồ họa: chân chạm đất, khói vùng nhiễm thật hơn, thay hình linh vật demo

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus`.
Chạy tự động: **không hỏi giữa chừng.** Cần quyết định thì chọn phương án an toàn nhất, ghi lý do. **Không push, không amend.**

## 0. Đầu phiên
- `git branch --show-current` = `minhnhat/npc-vroid`, `git log --oneline -1` = `1a55512`.
  - Tạo nhánh `git switch -c minhnhat/visual-polish` (nối tiếp PR NPC có thể chưa merge; ghi rõ trong README).
  - Nhánh hoặc commit khác ⇒ dừng.
- Unity mở, MCP kết nối (thử lại 3 lần, 2 phút mỗi lần, rồi dừng). Thoát Play Mode, Console 0 error, scene `isDirty=false`.
- Thay đổi chưa commit sẵn có:
  - Dòng `MainCamera` `m_LocalPosition.y` trong scene: **không bao giờ stage**.
  - 8 file `Materials/NPCVRoid/*.mat`: Unity tự ghi, chỉ khác xuống dòng; **không stage**.

## 1. Đọc trước
- `AGENTS.md`, `.agents/rules/*`, skill `.agents/skills/urp-graphics-upgrade`.
- `Docs/Evidence/CampusGuardian/CHARACTER-VROID/README.md` (cấu trúc `Campus Student`: Animator, Humanoid, Vrm10Instance ở gốc; rig và mesh trực tiếp dưới gốc).
- `Docs/Evidence/CampusGuardian/NPC-VROID/README.md` (prefab NPC, `IUHAmbientStudent`).
- `Docs/Evidence/CampusGuardian/INVESTIGATIONS-20261124.md` mục 1 (vùng nhiễm).

## 2. Người dùng báo (24/11, kèm ảnh)
1. Chân **người chơi** chưa thật sự chạm đất (hơi lơ lửng, bóng tách khỏi giày).
2. Chân **NPC** lún xuống đất.
3. Vùng nhiễm tím trông như sàn bị sơn tím ⇒ muốn **khói/sương thật hơn**.
4. Còn sót "NPC demo": hình người tròn đội mũ tốt nghiệp gần đài phun nước. Claude xác định đó là **linh vật `IUHMascotNPC`** (`Scripts/DailyActivities/IUHMascotNPC.cs`), có tương tác E và nhiệm vụ hoạt động hằng ngày.

## 3. Ngưỡng chung
- Đo A/B xen kẽ **cùng phiên** (A, B, A, B, A, B; 120 khung hình làm nóng + 3000 khung hình đo; 1280 × 900; VSync tắt; góc `lobby`/`overview` của `ART-ENVIRONMENT/cameras.json`). Với Task khói: thêm 1 góc nhìn vào vùng nhiễm khi chương Nhà E đang chạy.
- Ngưỡng theo mili-giây:
  - Thời gian khung hình trung vị tăng ≤ **0,5 ms** mỗi Task (khói ≤ **0,8 ms** ở góc nhìn vùng nhiễm).
  - p95 tăng ≤ 1,0 ms.
  - Draw calls tăng ≤ 20.
- Ghi trạng thái nguồn điện. Chỉ lưu tsv tóm tắt.
- Console 0 error. EditMode và PlayMode tách riêng, toàn bộ Pass. Missing Script = 0 ở 3 scene.

## 4. Các Task

### P1 — Chân người chơi chạm đất (commit `fix(player): ground VRoid feet`)
- **Đo:** trong Play Mode, khi đứng yên trên sân lát, bãi cỏ và sân đài phun nước, đo `min(y)` của đỉnh mesh giày (hoặc xương chân cộng độ dày đế) trừ `y` mặt đất (raycast).
  - Đo ở Idle và vài khung hình Walk/Run.
  - Ghi cả `CharacterController.skinWidth`, `center`, `height`, `GroundedOffset` của `ThirdPersonController`.
- **Sửa:** dịch **phần hình** xuống đúng khoảng lệch: transform gốc của rig/mesh VRoid (con trực tiếp của `Campus Student`), hoặc cách tương đương không đổi va chạm.
  - **Không đổi** `CharacterController`, `ThirdPersonController`, camera, Starter Assets.
  - Kiểm Humanoid vẫn chạy đúng sau khi dịch.
- Mục tiêu: khoảng lệch khi đứng yên trong **±0,02 m**; bóng dính gót giày.
- Kiểm: arena match-3 (proxy nhân bản không lệch theo), đi/chạy/nhảy/tiếp đất, camera.
- Test PlayMode: đứng yên trên sân lát, khoảng lệch ≤ 0,03 m.
- Ảnh cận ngang tầm chân, trước và sau (ngày).

### P2 — Chân NPC chạm đất (commit `fix(npc): ground VRoid NPC feet`)
- Đo như P1 cho cả 6 NPC (đứng và đi), trên đúng mặt đất chỗ đặt.
- Tìm nguyên nhân: pivot prefab, offset trong `IUHAmbientStudent` (raycast trúng layer nào, có trúng chính NPC hay collider khác không), hay chiều cao đặt trong scene.
- Sửa tại gốc nguyên nhân (prefab offset hoặc logic bám đất), áp chung cho mọi NPC. Không chỉnh tay từng NPC trong scene nếu có cách chung.
- Mục tiêu ±0,02 m khi đứng; khi đi không lún hay bay quá 0,03 m.
- Test PlayMode cho bám đất.
- Ảnh cận 2 nhóm NPC, trước và sau.

### P3 — Khói vùng nhiễm thật hơn (commit `feat(story): volumetric-looking corruption smoke`)
`IUHStoryCorruptionZone` (code của người dùng, được phép sửa):
- **Giữ nguyên API và hành vi:** `Create(positions, material)` (tâm, bán kính = xa nhất + 6 m), `Purify()` (chữ nổi, mờ dần 2 s rồi Destroy). Nơi gọi (`IUHStoryDirector`) không phải sửa; nếu bắt buộc thì chỉ sửa tối thiểu và ghi rõ.
- **Thay khối cầu tím** bằng khói hạt sát đất:
  - **Lớp sương là là:** Particle System, hình dạng đĩa bán kính vùng, hạt to 3–6 m, cao 0–1,5 m, trôi chậm, xoay chậm, đời sống 6–10 s, xuất hiện và biến mất mềm theo alpha.
    - Màu tím xám sẫm, không neon. Ví dụ (0,30; 0,22; 0,38), alpha đỉnh khoảng 0,25–0,35. Đậm ở giữa, thưa ở mép.
  - **Vài làn khói bốc lên** quanh vị trí quái: hạt nhỏ hơn, bay lên 2–4 m rồi tan.
  - **Texture khói:** tạo bằng script editor (noise nhiều lớp, mép mềm) lưu PNG trong `Assets/IUH_Campus/Textures/Story/`, hoặc noise trong shader. **Không tải asset ngoài.**
  - **Vật liệu:** URP Particles Unlit (hoặc shader riêng), trong suốt, **soft particles hoặc làm mờ theo độ sâu** để hạt không cắt cứng vào mặt đất.
    - Nếu URP chưa bật Depth Texture thì dùng cách làm mờ theo khoảng cách camera và ghi lại. **Không bật thêm tính năng toàn cục** nếu tốn hiệu năng.
  - Giới hạn **≤ 80 hạt/vùng** trên PC, **≤ 30** trên mobile. Không va chạm, không đổ bóng, không nhận bóng.
  - Bỏ hẳn lớp tím phủ sàn. Nếu cần ranh giới thì chỉ còn viền rất nhạt, alpha ≤ 0,08.
- `Purify()`: ngừng phát hạt, mờ dần các hạt đang có trong 2 s, rồi Destroy.
- Cập nhật `StoryScript_DemGlitch.asset` `fogMaterial` sang vật liệu khói mới nếu cần. Giữ `M_Story_CorruptionFog.mat` cũ (không xóa).
- Test PlayMode:
  - `Create` tạo Particle System đúng tâm và bán kính, `maxParticles` ≤ giới hạn.
  - `Purify` hủy object trong ≤ 2,5 s.
  - Không NullReference khi danh sách vị trí rỗng.
- Ảnh cùng góc với `CHARACTER-VROID/Resume/live-1.png`, `overview-*`, cận đài phun nước, ngày và đêm, trước và sau; thêm ảnh lúc `Purify`.

### P4 — Thay hình linh vật demo (commit `feat(art): VRoid mascot visual`)
- **Rà toàn scene campus** tìm mọi hình người tạm (primitive capsule/sphere ghép thành người, mesh "Student" low-poly còn bật). Liệt kê vào README: tên, vị trí, script gắn kèm.
  - Khối `Students in courtyard_102` đã tắt thì bỏ qua.
- `IUHMascotNPC`: **giữ nguyên script, tương tác E, billboard, particles, đèn, quest items**. Chỉ thay mô hình hiển thị dưới `m_ModelTransform`:
  - Tắt (`SetActive(false)`) phần hình primitive cũ, không xóa.
  - Đặt 1 bản NPC VRoid (chọn model khác 2 NPC gần đài phun nước nhất), gắn lại **mũ tốt nghiệp cũ** (`M_Mascot_GradCap`) lên xương đầu, đúng tỷ lệ.
  - Animator Idle, chân chạm đất như P2. SpringBone tắt.
  - Kiểm hiệu ứng lắc/nhảy (bob, dance) của script không làm model trôi hoặc lún quá 0,1 m. Nếu bob dịch cả model lên xuống quá nhiều thì giảm biên độ bằng field serialized, **không sửa script**. Không giảm được thì giữ nguyên và ghi lại.
- Hình người tạm khác (nếu có) mà không gắn script gameplay ⇒ tắt (`SetActive(false)`). Có gắn script gameplay ⇒ chỉ liệt kê, không đụng.
- Kiểm: E ở linh vật vẫn tương tác được (phím giả lập không nhận thì gọi API như các lượt trước và ghi rõ), nhiệm vụ hằng ngày không lỗi.
- Ảnh trước và sau linh vật, ngày và đêm.

### P5 — Bằng chứng (commit `docs(art): record feet, smoke and mascot polish`)
`Docs/Evidence/CampusGuardian/VISUAL-POLISH/README.md`:
- Commit, môi trường, nguồn điện; ghi nhánh nối tiếp PR NPC.
- Bảng khoảng lệch chân trước/sau (người chơi, từng NPC, linh vật).
- Thông số khói; ảnh trước/sau; bảng A/B từng Task.
- Danh sách hình người tạm.
- Số test. Mọi chỗ lệch. Kèm prompt này.

## 5. Quy tắc tự quyết
- Task quá 25 phút không tiến triển hoặc không đạt ngưỡng sau 2 vòng ⇒ bỏ Task đó:
  - Mở lại scene không lưu, xóa asset chưa commit của Task đó, ghi lý do.
  - Rồi làm Task kế tiếp. Các Task độc lập nhau; P5 luôn làm.
- **Chỉ lưu scene khi Task đạt.** MCP mất phản hồi ⇒ ghi `VISUAL-POLISH/RESUME.md` rồi dừng; **không ghi tay vào file scene**.
- Ảnh tự kiểm: không hồng lỗi shader, không T-pose, không cạnh cắt cứng của khói vào mặt đất hay tường.

## 6. Git
- Mỗi Task một commit. Stage chọn lọc kèm `.meta`. Không `git add -A`/`.`.
- **Không stage:** `*.csproj`, `*.slnx`, `Logs/`, `ProjectSettings/`, `*.mat` Unity tự ghi ngoài phạm vi Task, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`, dòng `MainCamera`.
- Scene: chỉ hunk của Task đang làm.

## 7. Cuối phiên
- Unity về Edit Mode, `isDirty=false`, Console 0 error.
- Tin nhắn cuối ngắn gọn:
  - Commit; Task xong/bỏ (lý do).
  - Bảng khoảng lệch chân.
  - Bảng A/B.
  - Link ảnh: cận chân người chơi sau, cận NPC sau, khói ngày/đêm sau, linh vật sau.
