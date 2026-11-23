# Prompt cho Codex — Lượt VFX: khói vùng nhiễm, hiệu ứng chân quái, hiệu ứng combat (gói Asset Store)

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus`.
Chạy tự động: **không hỏi giữa chừng.** Cần quyết định thì chọn phương án an toàn nhất, ghi lý do. **Không push, không amend.**

## 0. Đầu phiên
- Nhánh hiện tại phải là `main`, đã chứa merge PR #51 (`1061a44`) hoặc mới hơn (`git merge-base --is-ancestor 1061a44 HEAD`). Tạo nhánh `git switch -c minhnhat/vfx-pass` tại HEAD đó.
  - Nhánh khác hoặc thiếu `1061a44` ⇒ dừng.
  - Ghi hash HEAD vào README.
- Ba gói Asset Store là file **chưa track** mới import: đó là đầu vào của V0, không phải rác.
- Unity mở, MCP kết nối (thử lại 3 lần, 2 phút mỗi lần). Thoát Play Mode, Console 0 error, scene `isDirty=false`.
- Không stage dòng `MainCamera` `m_LocalPosition.y`, các `*.mat` Unity tự ghi ngoài phạm vi, `*.csproj`, `*.slnx`, `ProjectSettings/` (trừ khi Task cần và ghi rõ), `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`.

## 1. Người dùng quyết định (24/11)
- Dùng 3 gói Unity Asset Store **miễn phí**, người dùng đã tải qua Package Manager và **import vào project**:
  1. **Procedural Fire** (https://assetstore.unity.com/packages/vfx/particles/fire-explosions/procedural-fire-141496): hiệu ứng **dưới chân quái** ngoài campus.
  2. **Cartoon FX Remaster Free** (https://assetstore.unity.com/packages/vfx/particles/cartoon-fx-remaster-free-109565): hiệu ứng **trong combat** match-3.
  3. **Free Stylized Smoke Effects Pack** (https://assetstore.unity.com/packages/vfx/particles/fire-explosions/free-stylized-smoke-effects-pack-226406): **làn khói tím mờ** cho vùng nhiễm.
- Người dùng chấp nhận commit các gói vào repo (dự án môn học). Ghi nguồn trong `Docs/Credits.md` và `SOURCE.md` mỗi gói: tên, link, nhà phát hành, Unity Asset Store Standard EULA, ngày nhập.
- Nếu P3 (khói) của lượt `visual-polish` đã có: **thay bằng gói Smoke** nếu đẹp hơn (so ảnh); không thì giữ bản cũ và ghi lại.

## 2. Đọc trước
- `AGENTS.md`, `.agents/rules/*` (gồm `match3-combat.md`), skill `.agents/skills/urp-graphics-upgrade`.
- `Docs/Evidence/CampusGuardian/INVESTIGATIONS-20261124.md` (vùng nhiễm, beam quái, arena).
- `Docs/Evidence/CampusGuardian/VISUAL-POLISH/README.md` (nếu có).
- Code liên quan: `Scripts/Guardian/Story/IUHStoryCorruptionZone.cs`, các object `CORRUPTED_ENCOUNTER_*` trong scene campus, các script combat match-3 (`Match3CombatManager`, `Match3BattleStage`…) chỉ để tìm điểm gắn hiệu ứng.

## 3. Ngưỡng
- Đo A/B xen kẽ cùng phiên (A, B, A, B, A, B; 120 khung hình làm nóng + 3000 khung hình đo; 1280 × 900; VSync tắt). Ghi trạng thái nguồn điện. Chỉ lưu tsv tóm tắt.
  - Campus: góc `lobby`, `overview`, và 1 góc nhìn vào vùng nhiễm + 1 quái.
  - Combat: 1 góc arena trong lúc có hiệu ứng.
- Thời gian khung hình trung vị tăng **≤ 0,8 ms** mỗi Task; p95 tăng ≤ 1,5 ms; draw calls tăng ≤ 30.
- Mobile: mỗi hiệu ứng có cờ giảm hoặc tắt (`Application.isMobilePlatform`), số hạt mobile ≤ 1/2 PC.
- Console 0 error. EditMode và PlayMode tách riêng, toàn bộ Pass. Missing Script = 0 ở 3 scene.

## 4. Các Task

### V0 — Gom và dọn 3 gói (commit `chore(vfx): add Asset Store VFX packs`)
- Tìm thư mục Unity đã import (thường ở gốc `Assets/`, ví dụ `JMO Assets/`). Chuyển vào `Assets/IUH_Campus/ThirdParty/AssetStore/<TênGói>/` bằng `AssetDatabase.MoveAsset` để giữ GUID.
- **Kiểm URP:** prefab/vật liệu nào hiện hồng hoặc dùng shader Built-in thì đổi sang shader URP tương đương (`Universal Render Pipeline/Particles/Unlit` hoặc shader URP của chính gói nếu có). Ghi bảng shader trước/sau.
- **Xóa phần thừa:** scene demo, script demo, prefab không dùng, texture > 2K không dùng.
  - Chỉ xóa sau khi tìm GUID không còn tham chiếu.
  - Ghi dung lượng mỗi gói trước/sau dọn.
- Texture: Max Size ≤ 1024 (≤ 512 nếu hạt nhỏ), nén mặc định.
- `SOURCE.md` mỗi gói và mục `Docs/Credits.md`.
- Test EditMode: không prefab VFX dùng shader lỗi hoặc không hỗ trợ (`ShaderUtil.ShaderHasError`, `isSupported`).

### V1 — Khói vùng nhiễm (commit `feat(story): stylized corruption smoke from smoke pack`)
- `IUHStoryCorruptionZone`: giữ API `Create`/`Purify` và hành vi (tâm, bán kính, chữ nổi, mờ dần 2 s, Destroy).
- Dùng prefab khói của gói Smoke, chỉnh màu **tím xám sẫm mờ** (không neon), alpha thấp, là là sát đất 0–1,5 m, phủ theo bán kính vùng; thêm vài làn bốc lên quanh quái.
  - Bỏ lớp tím phủ sàn. Nếu cần ranh giới thì chỉ viền rất nhạt.
- Tối đa 80 hạt/vùng PC, 30 mobile. Không va chạm, không đổ bóng. Mép hạt không cắt cứng vào mặt đất (soft particles hoặc làm mờ theo camera).
- `Purify()`: ngừng phát, mờ dần hạt đang có trong 2 s, rồi Destroy.
- Test PlayMode: `Create` đúng tâm và bán kính, giới hạn hạt; `Purify` hủy trong ≤ 2,5 s; danh sách rỗng không lỗi.
- Ảnh ngày và đêm, trước và sau, cùng góc `CHARACTER-VROID/Resume/live-1.png` và toàn cảnh; ảnh lúc `Purify`.

### V2 — Hiệu ứng dưới chân quái (commit `feat(vfx): ground fire under campus monsters`)
- Với mỗi `CORRUPTED_ENCOUNTER_*` trong campus: gắn 1 hiệu ứng lửa/hào quang từ gói Procedural Fire **dưới chân quái**.
  - Màu tím hoặc tím hồng hợp chủ đề nhiễm, thấp (≤ 1 m), bán kính vừa thân quái.
  - Tắt cùng lúc quái bị thanh tẩy hoặc ẩn: bám theo trạng thái active của encounter, không thêm logic gameplay.
  - **Không đổi** collider, trigger, khoảng E (8 m / 4 m), beam, logic encounter.
- Nếu gói Fire không chạy được trên URP sau 2 vòng sửa ⇒ dùng hạt lửa của Cartoon FX Remaster thay thế và ghi lại.
- Kiểm: proxy quái trong arena (`StripToVisuals`/clone) **không mang theo** hiệu ứng lửa, hoặc mang theo mà không lỗi. Ghi rõ đã kiểm thế nào.
- Ảnh cận 2 quái ngày và đêm, toàn cảnh.

### V3 — Hiệu ứng combat (commit `feat(combat): Cartoon FX hit and combo effects`)
- **Chỉ thêm hiệu ứng hình ảnh**, không đổi luật chơi, sát thương, thời gian lượt, UI logic. Tuân `.agents/rules/match3-combat.md`.
- Tìm các điểm có sẵn để gắn (sự kiện hoặc hàm hiện có):
  - Ăn ô match-3 (theo màu nếu có).
  - Đòn đánh trúng quái.
  - Quái đánh trúng người chơi.
  - Combo hoặc kỹ năng đặc biệt.
  - Quái bị hạ.
- Dùng prefab Cartoon FX Remaster phù hợp từng sự kiện. Spawn qua pool đơn giản (không Instantiate/Destroy liên tục mỗi khung hình), tự trả về pool khi xong.
- Nếu phải sửa code combat: chỉ thêm lời gọi phát hiệu ứng (1–3 dòng mỗi điểm) hoặc subscribe sự kiện sẵn có. Ghi danh sách file/dòng đã sửa.
- Test PlayMode: 1 trận tự động hoặc mô phỏng; sự kiện phát hiệu ứng không lỗi; pool không tăng vô hạn sau 50 lần phát.
- Ảnh hoặc chuỗi ảnh: ăn ô, trúng đòn, quái bị hạ.

### V3b — Chỉnh nhỏ linh vật (commit `fix(art): mascot cap size and glow`)
Lượt `visual-polish` (`Docs/Evidence/CampusGuardian/VISUAL-POLISH/README.md`, mục P4) để lại 2 điểm:
- **Mũ tốt nghiệp quá nhỏ**, gần như không thấy trên đầu model VRoid. Phóng to 2 mesh `GradCap_Base`/`GradCap_Diamond` (dưới `J_Bip_C_Head`) cho vừa đầu, nằm đúng đỉnh đầu, không xuyên tóc quá nhiều.
- **Đèn xanh cyan làm mặt và áo chói**, nhất là ban đêm. Giảm cường độ/tầm của đèn linh vật (field serialized hoặc component Light trong scene) đến khi mặt nhìn rõ, vẫn còn hào quang nhẹ.
- **Không sửa** `IUHMascotNPC.cs`, tương tác E, billboard, nhiệm vụ. Không đụng biên độ nhảy múa.
- Ảnh trước/sau cùng góc `VISUAL-POLISH/mascot-AFTER-day.png` và `-night.png`. Không cần đo A/B (không thêm renderer).

### V4 — Bằng chứng (commit `docs(vfx): record VFX pass`)
`Docs/Evidence/CampusGuardian/VFX/README.md`:
- Commit, môi trường, nguồn điện, nhánh gốc.
- 3 gói: phiên bản, dung lượng trước/sau dọn, shader đã đổi.
- Hiệu ứng gắn ở đâu; file code đã sửa.
- Ảnh trước/sau; bảng A/B; số test; mọi chỗ lệch. Kèm prompt này.

## 5. Quy tắc tự quyết
- Task quá 25 phút không tiến triển hoặc không đạt ngưỡng sau 2 vòng ⇒ bỏ Task đó:
  - Mở lại scene không lưu, xóa asset chưa commit của Task, ghi lý do.
  - Rồi làm Task kế tiếp. V0 bắt buộc trước; V1, V2, V3, V3b độc lập; V4 luôn làm.
- **Chỉ lưu scene khi Task đạt.** MCP mất phản hồi ⇒ ghi `VFX/RESUME.md` rồi dừng; **không ghi tay vào file scene**.
- Ảnh tự kiểm: không hồng lỗi shader, không hiệu ứng che HUD hay lưới match-3, không neon chói ban ngày.
- Mỗi Task một commit. Stage chọn lọc kèm `.meta`. Không `git add -A`/`.`.

## 6. Cuối phiên
- Unity về Edit Mode, `isDirty=false`, Console 0 error.
- Tin nhắn cuối ngắn gọn:
  - Commit; Task xong/bỏ (lý do).
  - Dung lượng 3 gói sau dọn.
  - Bảng A/B.
  - Link ảnh: khói ngày/đêm, lửa chân quái, combat.
