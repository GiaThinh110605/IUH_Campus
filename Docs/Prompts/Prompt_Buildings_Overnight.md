# Prompt cho Codex — Lượt D đồ họa (tòa nhà) + 2 việc điều tra, chạy tự động qua đêm

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus` (Unity 6000.5.8f1, URP 17). Người dùng **đi ngủ, sáng mới đọc**.
⇒ **Không hỏi giữa chừng.** Gặp chỗ cần quyết định thì chọn phương án an toàn nhất, ghi lý do vào báo cáo rồi làm tiếp. **Không push.**

## 0. Đầu phiên
- `git branch --show-current` = `minhnhat/vroid-player`, `git log --oneline -1` = `3274980`.
  - Tạo nhánh mới tại đúng commit này: `git switch -c minhnhat/buildings-pass` (không đổi file nên an toàn khi Unity mở).
  - Lượt này **nối tiếp** PR VRoid chưa merge. Ghi rõ điều này trong README.
  - Nhánh hoặc commit khác ⇒ dừng.
- Unity mở, MCP `unityMCP` kết nối (đọc `mcpforunity://editor/state`). Không được ⇒ chờ 2 phút, thử lại 3 lần, vẫn không được thì dừng và ghi lý do vào `Docs/Evidence/CampusGuardian/ART-BUILDINGS/README.md`.
- Thoát Play Mode. Console 0 error. Scene campus `isDirty=false`.
  - Thay đổi chưa commit sẵn có duy nhất là dòng `MainCamera` `m_LocalPosition.y` (2.2066925 → 3.2066925). **Không bao giờ stage dòng này.**

## 1. Đọc trước
- `AGENTS.md`, `.agents/rules/*`, skill `.agents/skills/urp-graphics-upgrade`.
- Mẫu đã thành công:
  - `Assets/IUH_Campus/Shaders/IUH_StylizedGround.shader`: chiếu theo tọa độ thế giới, SRP Batcher, các pass `ShadowCaster`/`DepthOnly`/`DepthNormals`.
  - `Docs/Evidence/CampusGuardian/ART-ENVIRONMENT/README.md`.
- Bài học cũ:
  - `Docs/AmbientCG_Graphics_Upgrade.md`: dán texture theo UV campus thất bại.
  - Biến `line` là từ khóa HLSL làm shader hồng. **Không đặt tên biến** `line`, `point`, `triangle`, `sample`, `texture`, `linear`, `centroid`, `precise`, `vector`, `matrix`.
  - Lượt VRoid đo FPS sai vì nhân vật nằm ngoài khung hình.

## 2. Phong cách và ranh giới
- Stylized hoạt hình, khớp cây cỏ Quaternius và mặt đất lượt C. Không neon, không photoreal.
- **Không sửa:**
  - Mesh, transform, collider, prefab tòa nhà, FBX/GLTF nguồn.
  - Vật liệu gốc trong `SharedCampusConcept/Materials/`. Tạo bản mới trong `Assets/IUH_Campus/Materials/Buildings/` rồi gán thay để quay lui dễ.
  - Quái, check-in, vườn, nhân vật, UI, combat arena, `Campus volume.asset`.
- Không tải asset ngoài.
- iOS: không thêm hiệu ứng nặng.

## 3. Ngưỡng (so với mốc trước lượt đo ở Task D1)
- FPS trung vị giảm ≤ 10% ở cả góc sảnh và góc toàn cảnh.
- Draw calls tăng ≤ 30 (sảnh) và ≤ 40 (toàn cảnh).
- Console 0 error. EditMode và PlayMode **chạy tách riêng**, toàn bộ Pass. Missing Script = 0 ở 3 scene.

**Cách đo bắt buộc:**
- 1280 × 900, VSync tắt, Play Mode, camera đặt theo `ART-ENVIRONMENT/cameras.json` (`lobby`, `overview`). Không đổi `targetTexture` của camera chính.
- Mỗi cấu hình 3 lượt × (120 khung hình làm nóng + 3000 khung hình đo). Báo trung vị FPS, p95 ms, draw calls, tam giác.
- **Chỉ lưu file tsv tóm tắt** (1 dòng mỗi lượt). Không lưu tsv từng khung hình. Lượt trước thư mục bằng chứng nặng 35 MB.

## 4. Quy tắc tự quyết khi chạy đêm
- Một bước quá **25 phút** không tiến triển (MCP timeout lặp, test treo, lỗi lặp sau 3 lần sửa) ⇒ **bỏ Task đó**:
  - Mở lại scene không lưu.
  - Xóa asset mới chưa commit của Task đó (chỉ file do chính phiên này tạo).
  - Ghi lý do và log vào README, rồi sang Task kế tiếp không phụ thuộc vào nó.
- **Chỉ lưu scene khi Task đã đạt ngưỡng.** Ảnh và số đo chụp trong Play Mode trên scene chưa lưu.
- Shader lỗi compile sau 3 vòng ⇒ bỏ D2–D4, vẫn làm D5–D6.
- Vượt ngưỡng hiệu năng ⇒ thử giảm (gộp vật liệu, bỏ keyword, đơn giản hóa noise) tối đa 2 vòng. Vẫn vượt thì bỏ Task.
- Ảnh tự kiểm: **không có màu hồng/tím lỗi shader**, chữ và logo trên tòa nhà còn đọc được, cửa sổ vẫn thấy rõ. Nếu mất cửa sổ hoặc chữ thì sửa; không sửa được ⇒ bỏ Task.
- Mỗi Task một commit. Không amend. Stage chọn lọc kèm `.meta`. Không `git add -A`/`.`.
- **Không stage:** `*.csproj`, `*.slnx`, `Logs/`, `ProjectSettings/`, `*.mat` Unity tự ghi ngoài `Materials/Buildings`, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`, dòng `MainCamera`.
- Khôi phục PlayerPrefs nếu có đụng.

## 5. Các Task

### D1 — Kiểm kê + mốc trước lượt (commit `docs(art): inventory campus buildings before facade pass`)
- Liệt kê mọi renderer tòa nhà trong scene campus: tên, khu, mesh, vật liệu từng slot, shader, texture (`_BaseMap`), UV có dùng để vẽ cửa sổ hay không.
  - Gồm các vật liệu như `Limestone facade`, `Terracotta brick`, `Blue green glazing`, `Warm granite`, `IUH blue`, và vật liệu tòa nhà ở thư mục khác nếu có.
  - Bỏ qua cây, mặt đất, đồ trang trí nhỏ.
- Xác định **cửa sổ** nằm ở đâu: vật liệu kính riêng, texture theo UV, hay mesh riêng.
- Ảnh **trước** ngày và đêm: 5 góc của `ART-ENVIRONMENT/cameras.json`, cộng 3 góc cận tòa nhà mới (mặt tiền tòa chính, tòa gạch đỏ, Căn tin). Lưu `cameras.json` mới.
- Đo mốc FPS và draw calls góc sảnh và toàn cảnh.
- Ghi kết quả vào `Docs/Evidence/CampusGuardian/ART-BUILDINGS/README.md` (bảng kiểm kê, mốc đo).

### D2 — Shader mặt tiền (commit `feat(art): add stylized facade shader`)
`Assets/IUH_Campus/Shaders/IUH_StylizedFacade.shader`, cùng kiểu với `IUH_StylizedGround`.
- **Giữ texture/UV đang có nếu nó vẽ cửa sổ hay chữ** (chế độ `_FACADE_TEXTURED`). Chỉ thêm hiệu ứng lên trên, không thay hình.
- Chế độ không có texture (`_FACADE_PLAIN`): màu tường + noise rất nhẹ theo tọa độ thế giới (lệch ≤ 6% độ sáng), không lặp ô vuông.
- Chế độ gạch (`_FACADE_BRICK`, chỉ khi tường gạch hiện không có texture): mạch gạch vẽ bằng shader theo tọa độ thế giới (XZ cho mặt ngang, triplanar đơn giản cho mặt đứng).
- **Chung cho mọi chế độ:**
  - Gradient tối nhẹ gần chân tường (0–1,5 m, độ cao theo thế giới) để tòa nhà "đứng" trên đất.
  - Viền sáng nhẹ ở mép trên (tùy chọn).
  - Ánh sáng Lambert mềm, bóng cascade, `SampleSH`, fog.
- Kính (`_FACADE_GLASS`):
  - Ban ngày: gradient phản chiếu trời (xanh nhạt trên, đậm dưới), không gương thật.
  - Ban đêm: đèn cửa sổ ấm, một phần ô sáng và một phần tối theo hash vị trí ô.
  - Đọc global `_IUH_NightAmount` (0 = ngày, 1 = đêm). Kích thước ô `_WindowCell` (m).
- Test EditMode: shader tìm được, `isSupported`, không lỗi ở mọi keyword (`ShaderUtil.ShaderHasError`, `GetShaderMessages`).

### D3 — Đèn cửa sổ ban đêm (commit `feat(art): light building windows at night`)
- `IUHStoryNightShift`: **chỉ thêm** `Shader.SetGlobalFloat("_IUH_NightAmount", fraction)` ở chỗ đang lerp fog/ambient. Khi về ngày ⇒ 0. Khởi tạo 0 lúc `Initialize`/`OnDisable`.
- Test PlayMode: `ToNight` + `SetNightImmediate` ⇒ global = 1; `ToDay` ⇒ 0.
- Không đổi phần khác của script.
- Nếu cửa sổ nằm trong texture UV (không có vật liệu kính riêng) thì dùng mask: vùng texture tối hoặc xanh của kính ⇒ phát sáng ban đêm. Không làm được sạch sẽ thì bỏ hiệu ứng đêm cho loại tòa đó và ghi lại.

### D4 — Áp cho toàn bộ tòa nhà (commit `feat(art): stylized facades across campus`)
- Tạo vật liệu mới trong `Materials/Buildings/` (`M_Facade_*`) và gán `sharedMaterials` đúng slot cho mọi renderer tòa nhà đã kiểm kê.
- Ảnh **sau** ngày và đêm, cùng 8 góc của D1.
- Đo theo cách bắt buộc. Play 30–60 s: đi quanh tòa nhà, E ở quái (8 m chưa vào / 4 m vào trận), mở vườn, chuyển đêm (xem đèn cửa sổ).
- Không đạt ngưỡng ⇒ quy tắc mục 4.

### D5 — Điều tra, **chỉ báo cáo, không sửa** (commit `docs: investigate plaza tint and arena framing`)
1. **Sàn quảng trường tím rực** (`Docs/Evidence/CampusGuardian/CHARACTER-VROID/Resume/live-1.png`, lúc nhiệm vụ "Thanh tẩy Nhà E").
   - Tìm nguồn: object, vật liệu, script nào đổi màu. Có từ commit nào (`git log -S` / so `6aa7997`).
   - Đây là hiệu ứng thiết kế (vùng nhiễm) hay lỗi.
   - Chụp cùng góc ngoài nhiệm vụ và trong nhiệm vụ. Đề xuất cách sửa nếu là lỗi.
2. **Nhân vật trông nhỏ trong arena match-3** (`CHARACTER-VROID/Resume/arena.png`).
   - Đo chiều cao proxy `Arena_Student` trên màn hình, vị trí và FOV camera arena.
   - So với Casual2 (`CHARACTER/arena-before.png`, `arena-after.png`). Đề xuất 1–2 phương án (scale proxy, dời camera) kèm số.
   - Arena là phần combat của Thịnh ⇒ **không sửa**.
- Ghi vào `Docs/Evidence/CampusGuardian/INVESTIGATIONS-20261124.md`.

### D6 — Bằng chứng (commit `docs(art): record buildings pass`)
`ART-BUILDINGS/README.md`:
- Commit, ngày, môi trường; nhánh nối tiếp PR VRoid.
- Bảng kiểm kê và renderer đã đổi.
- Ảnh trước/sau ngày và đêm (link). Bảng đo trước/sau.
- Số test. Task nào bị bỏ và lý do. Mọi chỗ lệch prompt.
- Kèm prompt này vào commit D6.

## 6. Thứ tự và phụ thuộc
D1 → D2 → D3 → D4 → D5 → D6.
- D3, D4 cần D2.
- D5 độc lập: vẫn làm nếu D2–D4 bị bỏ.
- D6 luôn làm.

## 7. Cuối phiên
- Unity về Edit Mode, scene `isDirty=false`, Console 0 error.
- Tin nhắn cuối gửi người dùng, ngắn gọn:
  - Danh sách commit; Task nào xong và Task nào bỏ (lý do 1 dòng).
  - Bảng FPS/draw calls sảnh và toàn cảnh, trước và sau.
  - Link 4 ảnh quan trọng nhất: toàn cảnh ngày/đêm trước/sau, mặt tiền cận cảnh ngày/đêm sau.
  - Kết luận 2 điều tra.
