# Prompt cho Codex — Tiếp lượt GARDEN: cân bằng quái, tầm E, dời vườn vào 4 bồn hoa

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus`, nhánh `minhnhat/flower-garden` (đang có 4 commit vườn `97e51e9..f4e8ee0`, chưa push). Sai nhánh ⇒ dừng. MCP mất kết nối ⇒ dừng.
Đọc lại `AGENTS.md`, `.agents/rules/*`, spec `Docs/superpowers/specs/2026-11-19-flower-garden-design.md` (đã có mục 8 "Điều chỉnh 19/11" — làm theo mục đó).

Người dùng chơi thử và yêu cầu 3 việc. Mỗi việc một commit, test trước, stage chọn lọc như lượt trước. Không push, không amend.
Sửa đổi spec (mục 8, chưa commit) và file prompt này vào commit của Việc 1.

## Việc 1 — Cân bằng quái (commit `balance(match3): scale monster HP, damage and combat time`)
- Trong `Match3CombatManager` thêm 3 field serialize, giá trị mặc định:
  - `m_MonsterHpScale = 0.6f`
  - `m_MonsterDamageScale = 0.7f`
  - `m_ExtraCombatSeconds = 30f`
- Áp trong `SetActiveEncounter`:
  - `BossMaxHp = max(1, round(MonsterHp × 0.6))`.
  - `BossMinDmg`/`BossMaxDmg = max(1, round(dmg × 0.7))`, giữ `Max ≥ Min`.
  - `m_CombatDuration = CombatDuration + 30`.
- Không sửa giá trị trong scene và không sửa từng quái. Nếu có đường nào đặt `BossMaxHp`/dmg/thời gian mà không qua `SetActiveEncounter` (AR, dư chấn…), áp cùng hệ số ở đó; không chắc thì dừng và hỏi.
- Chữ trên đầu quái (`⚔️ Ý định: Tấn công 65-90 DMG`, HP hiển thị) phải khớp số đã nhân. Nếu chữ lấy số gốc từ `IUHMonsterEncounter`, sửa để hiển thị số sau hệ số. Cách sạch nhất: thêm getter tĩnh/hàm tiện ích dùng chung hệ số; không nhân hai lần.
- Test PlayMode: encounter HP 1000, dmg 65–90, 90 s ⇒ config 600, 46–63, 120 s. Kiểm cả `Max ≥ Min` khi dmg nhỏ (8–12 ⇒ 6–8).

## Việc 2 — Tầm phím E và tự vào trận của quái (commit `fix(encounter): limit interact and auto-start range to 5 m`)
- `IUHMonsterEncounter`:
  - Thêm `[SerializeField] float m_InteractRadius = 5f`.
  - Bấm E/nút Tương tác **và** tự vào trận sau 5 giây chỉ khi `dist ≤ min(m_TriggerRadius, m_InteractRadius)`.
  - `m_RevealRadius` (16 m) chỉ còn dùng để hiện bảng thông tin trên đầu quái.
  - Chữ "Vào trận sau …s (hoặc nhấn [E])" chỉ hiện khi đã trong 5 m; ngoài đó hiện chữ ý định tấn công như cũ.
- **Lỗi nuốt nút Tương tác:** `IUHMonsterEncounter` và `IUHARCorruptedObjectEncounter` gọi `IUHInputHelper.IsInteractKeyDown()` mỗi frame dù người chơi ở xa. Hàm này xóa `VirtualInteractPressed`, nên nút Tương tác trên điện thoại có thể bị quái ở xa ăn mất. Sửa cả hai: chỉ gọi `IsInteractKeyDown()` **sau khi** đã xác định người chơi trong tầm tương tác.
- `IUHARCorruptedObjectEncounter` đã dùng `m_InteractionDistance = 4.8` cho E và tự vào trận: giữ nguyên tầm đó, chỉ sửa thứ tự gọi như trên.
- Không đổi phím Space trong `IsInteractKeyDown()`.
- Test PlayMode:
  - Người chơi cách quái 8 m, `VirtualInteractPressed = true`, chạy 1 frame ⇒ không vào trận và cờ **vẫn true**.
  - Cách 3 m ⇒ vào trận.

## Việc 3 — Dựng lại 4 bồn hoa thành 4 luống vườn (commit `feat(garden): rebuild the four lawn planters as garden beds`)
**Người dùng đã chốt (thay một phần nội dung bên dưới):**
- **Bỏ 4 bồn hoa trang trí** trên bãi cỏ cạnh khu ô dù Căn tin. Thay bằng **4 bồn mới** dựng từ prefab vườn, đặt đúng vị trí, hướng và kích thước bồn cũ.
- Giữ nguyên cả vòng chơi: gieo, tưới, bón, hái.
- Tạm dùng asset **có sẵn** trong dự án. Người dùng sẽ tìm pack đẹp hơn sau, nên prefab phải dễ thay mô hình (xem mục Prefab).
- **Gỡ bồn cũ:**
  - Khung/collider bồn là object riêng ⇒ tắt hoặc xóa đúng object đó (ghi tên, đường dẫn và GUID vào README).
  - Hoa trang trí của 4 bồn nằm trong mesh gộp (`Pink flowers`/`White flowers`/… của `Layered planting and clipped hedges_405`) ⇒ **không sửa asset mesh gốc**. Tạo bản sao lọc bỏ tam giác nằm trong hộp bao của 4 bồn, lưu vào `Assets/IUH_Campus/Meshes/Garden/` (+ `.meta`). Gán bản sao cho renderer trong scene. Chụp ảnh trước/sau, xác nhận hoa ở chỗ khác trên campus không mất.
  - Làm vậy không được (mesh dùng chung ở scene khác, hoặc lọc hỏng hình) ⇒ dừng và gửi ảnh.
- **Prefab bồn** (`IUH_GardenBed.prefab`, sửa bản đang có):
  - Khung/viền bồn: gạch, đá hoặc gỗ từ vật liệu có sẵn (`CourtyardGraphics`, `SharedCampusConcept`…). Hình chữ nhật dài theo kích thước bồn cũ; mỗi bản trong scene chỉnh scale cho khớp.
  - Đất sáng màu, đọc rõ ban đêm (mục Màu đất bên dưới).
  - **3 ô mô hình rõ ràng:** con `Stage_Seed`, `Stage_Sprout`, `Stage_Bloom`. Mỗi ô chỉ chứa mô hình, code chỉ bật/tắt ô; thay asset sau này chỉ cần kéo mô hình mới vào ô. Ghi hướng dẫn thay asset 3–5 dòng trong README.
  - Mỗi ô rải nhiều cây theo chiều dài bồn.
  - Hiệu ứng **lớn dần** khi đổi giai đoạn: scale từ ~0.3 lên 1 trong ~0.6 s (unscaled time). Lúc load scene hiện ngay đúng giai đoạn, không chạy hiệu ứng.
- Phần còn lại của Việc 3 bên dưới vẫn áp dụng: 4 luống, save `v2`, mở bảng trong 2 m của bất kỳ điểm nào trên bồn, khoảng cách với quái, xóa 6 luống cũ. Câu "hoa trang trí trồi qua lớp đất ⇒ dừng" không còn áp dụng vì bồn cũ đã bị gỡ.

### Chốt vị trí 19/11 (thay mọi chỗ nói về tìm/đo bồn bên dưới)
- Lượt trước đo nhầm: 6 hình 17 × 13 m là **khoảnh cỏ**, không phải bồn. "Bồn hoa" là **dải hàng rào thấp + hoa** chạy dọc mép bắc và nam của mỗi khoảnh cỏ (các mesh `Trimmed hedge foliage`, `White flowers`, `Marigold`, `Pink flowers` trong `Layered planting and clipped hedges_405`).
- Dùng khoảnh cỏ cạnh Căn tin (tâm khoảng (-20, -29)). Có 2 dải, mỗi dải dài khoảng 12 m, rộng khoảng 1,5 m:
  - **Dải bắc** (sát khu ô dù): x ≈ -27…-15, z ≈ -25.
  - **Dải nam**: x ≈ -26…-15, z ≈ -34.
  - Tọa độ trên là ước lượng theo đỉnh mesh. Hãy đo lại hộp bao chính xác và ghi vào README.
- **Mỗi dải chia đôi thành 2 luống**, tổng 4 luống dài khoảng 6 m: luống 1–2 ở dải bắc (tây → đông), luống 3–4 ở dải nam.
- Gỡ hàng rào và hoa trang trí của cả 2 dải bằng bản sao mesh đã lọc (bỏ tam giác trong hộp bao của 2 dải, áp cho cả 4 mesh nêu trên). Các khoảnh cỏ khác giữ nguyên.
- **Dời quái Căn tin** (người dùng cho phép):
  - Object gốc `CORRUPTED_ENCOUNTER_CANTEEN`, hiện ở khoảng (-22, -36), cách dải nam khoảng 2 m. Dời **cả object gốc** về phía nam để mọi điểm của dải nam cách quái **> 4,8 + 2 = 6,8 m**, dự kiến z ≈ -42. X giữ nguyên nếu được.
  - Chỗ mới phải là nền đi được: không nằm trong tòa nhà, cây hay vật cản, không chồng lên đường/biển. Kiểm thêm khoảng cách tới quái khác và các điểm check-in.
  - Tìm mọi chỗ khác tham chiếu vị trí quái này: tọa độ cứng trong code/cốt truyện, waypoint/mũi tên nhiệm vụ, bản đồ, `IUHMonsterRegistry`, minimap. Cập nhật theo, hoặc liệt kê nếu không cần đổi.
  - Không có chỗ hợp lý ⇒ dừng, gửi ảnh 2–3 phương án.
  - Ghi vị trí cũ/mới vào README. Đây là thay đổi scene ngoài `GARDEN` được phép thêm.
- Kiểm lại khoảng cách mọi luống tới mọi quái (E 5 m với `IUHMonsterEncounter`, 4,8 m với quái AR/nhiễm độc) cộng 2 m. Bỏ qua bảng 6 khoảnh cỏ cũ và ảnh `planter-clearance-map.png` (xóa ảnh đó, hoặc thay bằng sơ đồ mới cho đúng).

### Nội dung gốc của Việc 3 (vẫn dùng, trừ chỗ đã thay ở trên)
- Người dùng muốn **4 luống** đặt đúng **4 bồn hoa trên bãi cỏ** trong ảnh: bãi cỏ cạnh khu bàn ô dù của Căn tin, các dải bồn hoa đỏ/cam/trắng chạy chéo hai bên bãi cỏ.
  - Bước 1: tìm 4 bồn bằng scene (collider/mesh bồn, ví dụ object trong `Layered planting and clipped hedges_405` hoặc khu `Garden beds…`) và chụp màn hình để xác nhận.
  - Bước 2: ghi tọa độ và kích thước từng bồn vào README.
  - Không chắc đâu là 4 bồn ⇒ dừng, gửi ảnh đánh số cho người dùng chọn.
- `GardenRules.BedCount = 4`. Save `v1` cũ 6 luống ⇒ `Deserialize` trả 4 luống trống, không lỗi. Đổi phiên bản thành `v2` để phân biệt. Cập nhật test lõi và test manager theo 4 luống.
- Mỗi luống khớp hình bồn:
  - Lớp đất phủ đúng mặt bồn (scale/xoay theo bồn, không tràn ra cỏ).
  - Mô hình Hạt/Mầm/Nở rải theo chiều dài bồn.
  - Nhãn billboard và tâm tính khoảng cách đặt ở giữa bồn.
  - Bồn dài thì người chơi đứng gần **bất kỳ điểm nào trên bồn** trong 2 m là mở được: tính khoảng cách tới đoạn thẳng/hộp của bồn, không chỉ tới tâm. Cập nhật `FindNearestBed` và test cho phù hợp.
- **Màu đất:** không dùng đất gần như đen. Chọn vật liệu đất sáng hơn hoặc tạo 1 vật liệu `Assets/IUH_Campus/Materials/Garden/M_Garden_Soil.mat` (nâu ấm, đọc rõ ban đêm). Đất ướt tối hơn đất khô một chút.
- **Hoa trang trí có sẵn trong bồn** là một phần mesh gộp `Pink flowers`/`White flowers` trải khắp campus, không tắt riêng được. Phủ lớp đất của luống lên mặt bồn và chụp ảnh ban đêm, ban ngày.
  - Hoa trang trí vẫn trồi qua lớp đất làm rối (không phân biệt được bồn trống với bồn đã trồng) ⇒ **dừng và gửi ảnh**, không sửa mesh gộp.
- Xóa 6 bản luống cũ trước Căn tin khỏi `GARDEN`. Scene chỉ được đổi trong `GARDEN`, cộng với việc gỡ 4 bồn cũ và đổi mesh hoa gộp ở phần đầu Việc 3. Liệt kê mọi object ngoài `GARDEN` bị đụng.
- Khoảng cách với quái: sau Việc 2 tầm E của quái là 5 m. Mỗi bồn phải cách mọi quái quá 5 m + 2 m. Không đủ ⇒ ghi lại và hỏi.
- Không sửa file asset mesh gốc. Chỉ di chuyển đúng quái `CORRUPTED_ENCOUNTER_CANTEEN` như mục "Chốt vị trí", các quái khác giữ nguyên.

## Kiểm chứng cuối
- EditMode + PlayMode toàn bộ Pass (ghi số trước/sau); Console 0 error; Missing Script = 0 ở 3 scene và mọi prefab.
- Chơi thử (sao lưu và khôi phục `IUH_Garden_v1`/`v2`, `IUH_Guardian_*`, `IUH_Story_Progress`):
  - Chưa nâng cấp, đánh 1 trận chương 1 bằng thao tác thật (không giả thắng): thắng được không, còn bao nhiêu HP và giây.
  - Đứng cách quái 8 m bấm E ⇒ không vào trận; 4 m ⇒ vào trận.
  - Đủ vòng gieo, tưới, bón, hái ở 2 bồn; đứng ở đầu bồn dài vẫn mở được bảng.
  - Ảnh: 4 bồn khi trống, khi Mầm, khi Nở; cả ngày và đêm (sau biến cố).
- Cập nhật `Docs/Evidence/CampusGuardian/GARDEN/README.md`:
  - **Viết lại gọn**, bỏ các ghi chú "đang làm dở".
  - Thêm mục cân bằng và tầm E, kèm ảnh.
  - Commit `docs(garden): record balance, interact range and planter move`.

## Dừng và báo khi
- Không xác định được 4 bồn, hoặc hoa trang trí làm luống khó nhìn (gửi ảnh).
- Có đường tạo trận không qua `SetActiveEncounter` mà không chắc có nên áp hệ số không.
- Lỗi do sửa của bạn quá 3 vòng.
