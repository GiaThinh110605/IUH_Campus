# Prompt cho Codex — Lượt E đồ họa: NPC sinh viên VRoid trong campus

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus` (Unity 6000.5.8f1, URP 17, UniVRM 0.131.4 đã cài).
Chạy tự động: **không hỏi giữa chừng.** Gặp chỗ cần quyết định thì chọn phương án an toàn nhất, ghi lý do vào báo cáo rồi làm tiếp. **Không push, không amend.**

## 0. Đầu phiên
- `git branch --show-current` = `minhnhat/npc-vroid`, `git log --oneline -1` = `55b07e2`. Sai ⇒ dừng.
- Unity mở, MCP `unityMCP` kết nối. Không được ⇒ chờ 2 phút, thử lại 3 lần, rồi dừng.
- Thoát Play Mode. Console 0 error. Scene campus `isDirty=false`.
  - Thay đổi chưa commit sẵn có duy nhất trong scene là dòng `MainCamera` `m_LocalPosition.y` (2.2066925 → 3.2066925). **Không bao giờ stage dòng này.**

## 1. Đọc trước
- `AGENTS.md`, `.agents/rules/*`, skill `.agents/skills/free-asset-sourcing`, `.agents/skills/urp-graphics-upgrade`.
- `Docs/Evidence/CampusGuardian/CHARACTER-VROID/README.md`: cách nhập VRoid, chỉnh MToon, **cách đo A/B cùng phiên**.
- `Docs/Evidence/CampusGuardian/ART-BUILDINGS/README.md`: bài học đo hiệu năng (đo khác phiên sai tới 30%).

## 2. Hiện trạng (Claude kiểm 24/11)
- Campus **chưa có NPC động**. Sinh viên trong sân là khối tĩnh `Students in courtyard_102`, gồm 4 mesh gộp theo vật liệu (`IUH blue`, `Student skin`, `Dark bronze`, `Limestone facade`).
- Các script `Scripts/NPC/NPCMovement`, `NPCRoute`, `NPCIdentity` (cần NavMesh) **không dùng trong scene**. Không sửa chúng.
- `IUHMascotNPC` (linh vật) đang dùng: **không đụng**.
- File nguồn trong `D:\VRoid\model\` (Claude đã đọc meta, license giống nhau, đều đạt):

| File | Tam giác | Vật liệu | Spring | Joint |
|---|---:|---:|---:|---:|
| `NPC1.vrm` | 28.126 | 2 | 36 | 114 |
| `NPC2.vrm` | 24.904 | 2 | 23 | 73 |
| `NPC3.vrm` | 28.604 | 2 | 25 | 79 |
| `NPC4.vrm` | 23.452 | 2 | 26 | 84 |

  - License: `avatarPermission: everyone`, `commercialUsage: corporation`, `allowRedistribution: true`, `modification: allowModificationRedistribution`, `creditNotation: required`. Các mục bạo lực, tình dục, chính trị, thù ghét đều `false`. Tác giả `pixiv VRoid Project`.
  - Meta `name` của `NPC3.vrm` ghi nhầm là "NPC2". Ghi chú trong `SOURCE.md`, không sửa file `.vrm`.
  - **Không dùng** `IUH Student Player.vrm` làm NPC.

## 3. Ranh giới
- **Không sửa:** file `.vrm`, code package UniVRM, Starter Assets, người chơi `Campus Student`, quái, check-in, vườn, UI, combat, tòa nhà, mặt đất, `IUHMascotNPC`, script `Scripts/NPC/*`.
- NPC chỉ để trang trí:
  - Không hội thoại.
  - Không chặn đường người chơi: không collider, hoặc chỉ trigger nhỏ.
  - Không đứng trên lối vào quái, điểm check-in, luống vườn hay cửa tòa nhà.
- Không tải asset ngoài. Animation chỉ dùng clip humanoid có sẵn trong Starter Assets (Idle, Walk).

## 4. Ngưỡng và cách đo
**Đo A/B xen kẽ trong cùng một lần Play Mode** (bật/tắt nhóm NPC mới và khối tĩnh cũ bằng script runtime, không lưu scene):
- Thứ tự A, B, A, B, A, B. Mỗi lượt 120 khung hình làm nóng + 3000 khung hình đo. Góc `lobby` và `overview` trong `Docs/Evidence/CampusGuardian/ART-ENVIRONMENT/cameras.json`. 1280 × 900, VSync tắt.
- Ghi trạng thái nguồn điện (`powercfg /getactivescheme`, `Get-CimInstance Win32_Battery`).
- Chỉ lưu tsv tóm tắt `performance-AB.tsv`.

Ngưỡng (B so với A cùng phiên):
- FPS trung vị giảm ≤ 10%; p95 ms tăng ≤ 10%.
- Draw calls tăng ≤ 40 (sảnh), ≤ 60 (toàn cảnh).

Không đạt ⇒ tối ưu tối đa 2 vòng theo thứ tự:
1. Bớt số NPC.
2. Tắt đổ bóng của NPC ở xa.
3. Tắt outline MToon cho NPC.
4. Giảm texture.

Vẫn không đạt ⇒ quy tắc mục 6.

## 5. Các Task

### E1 — Nhập 4 model NPC (commit `chore(art): import VRoid NPC models`)
- Chép vào `Assets/IUH_Campus/ThirdParty/VRoid/NPC/IUH_NPC_01.vrm` … `IUH_NPC_04.vrm`.
  - Kèm `SOURCE.md`: nguồn, ngày xuất 23/11/2026, VRoid Studio, toàn bộ meta license ở mục 2, link `https://vrm.dev/licenses/1.0/`, ghi chú tên meta của NPC3.
  - Thêm mục vào `Docs/Credits.md`.
- Import qua UniVRM ⇒ Avatar Humanoid hợp lệ, MToon10 URP. Texture NPC: **Max Size 1024** (import override).
- Vật liệu: extract ra `Assets/IUH_Campus/Materials/NPCVRoid/`, dùng thông số MToon đã chỉnh cho người chơi (Lit 0,85; Shade (0,65; 0,67; 0,72); GI Equalization 0,5; Toony 0,7; Emission 0) để không cháy sáng ban ngày.
- Prefab `Assets/IUH_Campus/Prefabs/NPC/PF_NPC_VRoid_01..04.prefab`:
  - `Animator` ở gốc, Avatar của model, Culling Mode = **Cull Completely**.
  - **SpringBone mặc định tắt** (NPC đứng hoặc đi chậm, không cần). Ghi cách tắt.
  - Không collider (hoặc chỉ trigger nhỏ).
- Test EditMode: 4 prefab có Avatar `isValid && isHuman`, không vật liệu null hoặc hồng, texture ≤ 1024.
- Kiểm: Console 0 error; EditMode và PlayMode tách riêng, toàn bộ Pass.

### E2 — Hành vi NPC đơn giản (commit `feat(npc): ambient VRoid students`)
- Script mới `Assets/IUH_Campus/Scripts/Guardian/Ambient/IUHAmbientStudent.cs` (không dùng NavMesh). 2 chế độ:
  - **Đứng:** Idle, lệch pha animation ngẫu nhiên theo seed để không đồng bộ; thỉnh thoảng xoay nhẹ về phía bạn cùng nhóm.
  - **Đi:** đi qua lại giữa 2–4 điểm cố định trên sân lát, tốc độ đi bộ (khoảng 1,2–1,5 m/s, khớp animation Walk), dừng 2–5 s ở mỗi điểm, bám mặt đất bằng raycast xuống. Né người chơi: khi người chơi < 1,2 m thì dừng chờ.
- Tối ưu:
  - Cách camera > 45 m ⇒ tắt Animator và tạm dừng cập nhật.
  - Dùng `IUHPlayerLocator` để tìm người chơi (không `FindObjectOfType` mỗi khung hình).
  - Có cờ giảm số NPC trên mobile (`Application.isMobilePlatform`: chỉ giữ tối đa 4).
- Test PlayMode:
  - NPC đi tới điểm kế tiếp rồi quay lại.
  - Dừng khi người chơi đứng chắn.
  - Tắt Animator khi ở xa.
  - Không có NullReference khi không có người chơi.

### E3 — Đặt NPC vào campus (commit `feat(art): populate campus with VRoid students`)
- Kiểm kê khối `Students in courtyard_102`:
  - Ước lượng vị trí từng hình người (tách cụm đỉnh theo mesh).
  - Ảnh trước ngày và đêm: 5 góc của `ART-ENVIRONMENT/cameras.json`, cộng 2 góc cận sân có sinh viên.
- **Tắt** (`SetActive(false)`) khối tĩnh cũ, **không xóa**, để quay lui dễ. Ghi rõ trong README.
- Đặt **10–14 NPC** dưới object gốc mới `NPC_STUDENTS`:
  - 4 model, mỗi model 2–4 bản; phối xen kẽ để 2 bản giống nhau không đứng cạnh nhau.
  - Khoảng 2/3 đứng theo nhóm 2–3 người (gần chỗ cũ, sân trước tòa chính, gần Căn tin, ghế đá), 1/3 đi bộ trên lối lát.
  - Kiểm không chắn lối vào quái, check-in, luống vườn, cửa tòa nhà, lối đi chính của người chơi.
- Đo A/B theo mục 4 (A = khối cũ bật và NPC tắt, B = ngược lại).
- Ảnh sau, cùng các góc, ngày và đêm. Thêm 1 ảnh cận cảnh nhóm NPC và người chơi đứng gần.
- Play 30–60 s:
  - Đi xuyên qua nhóm NPC và đứng chắn NPC đang đi.
  - E ở quái (8 m chưa vào / 4 m vào trận), mở vườn, chuyển đêm.
  - Phím E giả lập không nhận thì ghi rõ, không ghi Pass.
- Scene: chỉ stage hunk `NPC_STUDENTS` và dòng tắt khối cũ.

### E4 — Bằng chứng (commit `docs(npc): record VRoid NPC pass`)
`Docs/Evidence/CampusGuardian/NPC-VROID/README.md`:
- Commit, ngày, môi trường, trạng thái nguồn điện.
- Bảng 4 model (tam giác, vật liệu, texture, spring đã tắt).
- Vị trí và số NPC, chế độ đứng/đi.
- Ảnh trước/sau, bảng A/B.
- Số test. Mọi chỗ lệch prompt. Kèm prompt này vào commit.

## 6. Quy tắc tự quyết
- Một bước quá **25 phút** không tiến triển (MCP timeout lặp, test treo, lỗi lặp sau 3 lần sửa) ⇒ bỏ Task đó và các Task phụ thuộc:
  - Mở lại scene không lưu.
  - Xóa asset chưa commit do chính phiên này tạo.
  - Ghi lý do và log vào README, rồi làm E4.
- **Chỉ lưu scene khi Task đã đạt ngưỡng.**
- MCP mất phản hồi giữa chừng ⇒ không ghi tay vào file scene. Ghi file bàn giao `NPC-VROID/RESUME.md` (đã làm tới đâu, bước kế tiếp), rồi dừng.
- Ảnh tự kiểm:
  - Không có màu hồng lỗi shader.
  - NPC không T-pose, không lơ lửng hoặc lún đất, không cháy sáng ban ngày, không phát sáng ban đêm.
  - Nếu sai thì sửa; không sửa được ⇒ bỏ Task.
- Thứ tự: E1 → E2 → E3 → E4. E3 cần E1 và E2. E4 luôn làm.

## 7. Git
- Mỗi Task một commit. Stage chọn lọc kèm `.meta`. Không `git add -A`/`.`.
- **Không stage:** `*.csproj`, `*.slnx`, `Logs/`, `ProjectSettings/`, `*.mat` Unity tự ghi ngoài `Materials/NPCVRoid`, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`, dòng `MainCamera`.
- Khôi phục PlayerPrefs nếu có đụng.

## 8. Cuối phiên
- Unity về Edit Mode, scene `isDirty=false`, Console 0 error.
- Tin nhắn cuối gửi người dùng, ngắn gọn:
  - Danh sách commit; Task nào xong và Task nào bỏ (lý do).
  - Bảng A/B sảnh và toàn cảnh.
  - Số NPC; link 4 ảnh: toàn cảnh ngày trước/sau, cận nhóm NPC ngày, đêm.
  - Mọi chỗ lệch prompt.
