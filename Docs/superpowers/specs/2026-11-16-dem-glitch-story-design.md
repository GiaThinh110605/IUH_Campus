# Thiết kế: Cốt truyện "IUH Guardian – Đêm Glitch"

Ngày: 16/11/2026 · Người yêu cầu: Minh Nhật · Trạng thái: đã duyệt 16/11/2026

## 1. Vì sao làm

Hiện vào game là gặp quái và combat ngay; quái đến từ khoảng 4 hệ chạy song song, không biết nhau
(10 `IUHMonsterEncounter` match-3, 4 `IUHBuildingMonsterSpawner`, `IUHCampusZoneDirector` + 3 drone trời,
6 `IUHMonsterAI` + 6 `IUHFlyingMonsterAI` real-time, 5 `IUHARCorruptedObjectEncounter`). Người chơi
không hiểu mình đang ở đâu, vì sao có quái.

Mục tiêu: người chơi (và giảng viên khi chấm) hiểu theo thứ tự:
1. Đây là khuôn viên **Đại học Công nghiệp TP.HCM (IUH)**.
2. Có **biến cố** xảy ra.
3. Biến cố sinh ra **quái**; mình là **Guardian** đi chống lại.

## 2. Cốt truyện (đã chốt)

- **Bối cảnh:** IUH chuẩn bị Ngày hội IUH. Trường vừa vận hành **IUH Core** – AI quản lý đèn, cổng, mạng.
  Người chơi là sinh viên tình nguyện đội **Guardian**, đi cùng **drone trợ lý Core-Bot** (người dẫn truyện).
- **Mở đầu – "Một ngày ở IUH"** (không có quái): camera bay giới thiệu campus, chữ
  *"Đại học Công nghiệp TP.HCM – 3 ngày trước Ngày hội"*. Core-Bot giao việc trực ban: lần lượt check-in
  Cổng → Đài phun nước → Khu Ngày hội (`CHECKIN_SPOT_GATE`, `_FOUNTAIN`, `_FESTIVAL`).
- **Biến cố – "Bản cập nhật lúc nửa đêm":** tại check-in cuối, trời chuyển đêm, IUH Core cài bản cập nhật
  lạ chứa mã độc **GLITCH**; campus chớp tắt, đèn LED Ngày hội chuyển tím. Core-Bot: dữ liệu hỏng bám vào
  đồ điện tử/đồ vật → thành **Data Monster**.
- **Chương 1–3 – Giành lại từng khu:** Nhà E → Canteen/Vườn → Sân chính. Mỗi lúc chỉ **một** khu nhiễm
  (sương tím, 1–2 quái match-3). Thắng hết quái của khu → **thanh tẩy** (sương tan, hoa/đèn hồi, Gold) →
  khu kế tiếp mới nhiễm.
- **Chương cuối – Trùm GLITCH** ("Đại Ma Vương Hắc Ám") ở sân khấu Ngày hội, match-3 khó nhất. Thắng →
  IUH Core khôi phục, pháo hoa (`IUHCampusFireworksManager.TriggerVictoryCelebration`), màn kết.
- **Sau truyện – "Dư chấn":** PC chơi tự do; iPhone AR là "săn mảnh vụn GLITCH còn lẩn trong đồ vật thật".

## 3. Mục tiêu / Không làm / Chống mục tiêu

**Mục tiêu**
- Lần chơi đầu đi đúng thứ tự Mở đầu → Biến cố → Ch.1 → Ch.2 → Ch.3 → Trùm → Kết, khoảng 10–15 phút.
- Trong lúc chạy truyện, **chỉ** quái của chương hiện tại tồn tại; mọi nguồn quái khác bị tắt.
- Lời thoại, thứ tự chương, khu ↔ quái nằm trong **dữ liệu** (ScriptableObject), sửa không cần code.
- Lưu tiến độ chương; mở lại game tiếp tục đúng chương. Có lựa chọn "Chơi lại cốt truyện".

**Không làm (lượt này)**
- Không viết lại/gộp các hệ spawn của Thịnh; không xóa object/script nào của Thịnh.
- Không làm cutscene phức tạp (Timeline, lồng tiếng). Chữ + camera đơn giản là đủ.
- Không đổi luồng AR/iOS ngoài một dòng giới thiệu "Dư chấn" (nếu Thịnh đồng ý).
- Không đụng backend.

**Chống mục tiêu (làm xong mà thế này là hỏng)**
- Có quái xuất hiện trước biến cố, hoặc quái chương sau xuất hiện sớm.
- Người chơi bị kẹt: không biết đi đâu, hay chương không chuyển dù đã thắng hết quái.
- Tắt `STORY_DIRECTOR` mà game không trở về như cũ.
- Phá phần mobile/AR hoặc combat real-time khi truyện đã xong (chế độ tự do).

## 4. Ràng buộc

- Unity 6000.5.8f1, URP; scene `IUH_SharedCampus_Concept.unity` là của **Gia Thịnh** → thêm object/kéo
  tham chiếu phải **hỏi Thịnh trước** và không sửa scene cùng lúc. Chỉ thêm, không sửa object có sẵn.
- Không sửa code của Thịnh nếu tránh được. Phát hiện quái thắng qua việc `IUHMonsterEncounter.OnCombatVictory`
  tự `SetActive(false)` (đã kiểm: `Guardian/IUHMonsterEncounter.cs:281-293`).
- Tiền thưởng chỉ qua `IUHGuardianInventory` (hệ duy nhất).
- Luật `.agents/rules/*` (coding/unity/match3). Missing Script = 0, Console 0 error.
- Không tự tải asset ngoài; dùng primitive/vật liệu có sẵn cho sương tím, hiệu ứng.

## 5. Các cách đã cân nhắc

| Cách | Mô tả | Đánh đổi |
|---|---|---|
| **A. Đạo diễn cốt truyện chồng lên scene (ĐÃ CHỌN)** | Code mới ở `Scripts/Guardian/Story/`; 1 object `STORY_DIRECTOR` trong scene bật/tắt các nguồn quái có sẵn theo chương | Không xóa/viết lại của Thịnh, gỡ được bằng cách tắt 1 object; vẫn cần 1 lần sửa scene (hỏi Thịnh), hệ cũ vẫn tồn tại nhưng bị tắt |
| B. Scene "Story" riêng sao chép từ campus | Không đụng scene Thịnh | Scene rất lớn, hai bản lệch nhau, sửa lỗi hai lần |
| C. Gộp/viết lại toàn bộ hệ spawn | Sạch nhất | Sửa/xóa nhiều code Thịnh, dễ vỡ mobile/AR, rủi ro trước hạn nộp |

Người dùng chọn **A** vì có truyện mà vẫn tôn trọng phần của Thịnh và gỡ ra được.

## 6. Thiết kế

### 6.1 Thành phần
```
Scripts/Guardian/Story/
  Core/  (asmdef IUHCampus.Guardian.Story.Core, noEngineReferences: true)
    StoryProgress.cs        – máy trạng thái thuần: chương hiện tại, bước, điều kiện chuyển; tuần tự hóa được
  StoryScript.asset (ScriptableObject IUHStoryScript) – kịch bản: danh sách chương, mỗi chương gồm
        id, tiêu đề, các dòng thoại Core-Bot, mục tiêu hiển thị, vị trí đích (Transform), danh sách quái
  IUHStoryDirector.cs       – MonoBehaviour trên STORY_DIRECTOR: đọc kịch bản, giữ StoryProgress,
                              bật/tắt nguồn quái, nghe điều kiện hoàn thành, lưu/đọc tiến độ
  IUHStoryDialogueUI.cs     – khung thoại Core-Bot + dòng mục tiêu góc màn hình, tự dựng uGUI lúc chạy
  IUHStoryWaypoint.cs       – mũi tên/cột sáng chỉ đường tới đích chương hiện tại
  IUHStoryCorruptionZone.cs – sương tím + làm tối/héo một khu; Purify() để thanh tẩy
  IUHStoryNightShift.cs     – chuyển ngày→đêm và LED Ngày hội→tím lúc biến cố, đảo lại ở kết
Tests/Editor/                – EditMode test cho StoryProgress (thứ tự chương, lưu/đọc, chơi lại)
```

### 6.2 Luồng
1. `Awake` của director (chạy sớm hơn các hệ khác qua Script Execution Order hoặc `[DefaultExecutionOrder(-500)]`):
   nếu truyện **chưa xong** → tắt mọi nguồn quái trong danh sách "Quản lý" (GameObject của 10 encounter,
   4 building spawner, zone director, quái real-time mặt đất/bay, drone trời).
2. **Mở đầu:** camera giới thiệu (lia qua 3–4 điểm, ~15 s, bỏ qua được) → thoại Core-Bot → lần lượt 3 check-in.
   Hoàn thành check-in = người chơi vào bán kính của điểm đó (director tự đo khoảng cách; không sửa
   `IUHCheckInSpot`).
3. **Biến cố:** `IUHStoryNightShift` chạy (đèn chớp, chuyển đêm, LED tím), thoại báo động.
4. **Chương 1–3:** bật `IUHStoryCorruptionZone` của khu + GameObject 1–2 encounter của chương; waypoint chỉ
   tới khu. Chương xong khi **mọi** encounter của chương đã `activeSelf == false` *sau khi* đã được bật
   (đánh dấu "đã bật" để không tính nhầm). Xong → `Purify()`, thoại, lưu, sang chương sau. *Gold: dùng phần thưởng sẵn có của
   encounter (G1), director không cộng thêm — quyết định của Claude, tránh cộng hai lần.*
5. **Trùm:** như chương thường với encounter Đại Ma Vương ở sân khấu Ngày hội.
6. **Kết:** đảo đêm→ngày, pháo hoa, màn chữ kết; đánh dấu truyện xong; **trả lại** mọi nguồn quái về
   trạng thái gốc (chế độ tự do như game hiện tại).
7. Thua match-3: không đổi chương; encounter vẫn còn (hành vi sẵn có của `OnCombatDefeat`).

### 6.3 Lưu tiến độ
- PlayerPrefs, tiền tố `IUH_Story_` (giống kiểu `IUHGuardianInventory`): chương hiện tại, cờ đã xong.
- Mở lại giữa chương → vào thẳng chương đó (bỏ qua mở đầu/biến cố, đặt sẵn đêm nếu đã qua biến cố).
- "Chơi lại cốt truyện": xóa khóa `IUH_Story_*` và tải lại scene (nút trong menu tạm/phím debug – xem 7).

### 6.4 Gỡ ra
Tắt object `STORY_DIRECTOR` → director không chạy → mọi thứ như hiện tại. Đây là tiêu chí bắt buộc.

## 7. Để người thực hiện quyết (và lưu ý)
- Chọn encounter cụ thể cho từng chương trong 10 cái có sẵn (theo vị trí khu); encounter thừa giữ tắt
  trong lúc truyện, bật lại khi kết.
- Kiểm `IUHMonsterEncounter` của mỗi chương có `m_MilestoneId` để nhận Gold G1; nếu thiếu, báo lại chứ không tự cộng Gold.
- Cách tắt hệ real-time: ưu tiên `SetActive(false)` GameObject gốc; nếu hệ nào tự bật lại (ví dụ
  `IUHCampusZoneDirector` bật drone khi vào khu, phím K), tắt cả component đó.
- Nút "Chơi lại cốt truyện": đặt ở đâu trong UI hiện có (hỏi Thịnh) hoặc phím debug F9 trong Editor.
- Bản đồ/minimap (`IUHMinimapUI`) có thể hiện ghim khu chương hiện tại nếu dễ.

## 8. Quyết định của người dùng (16/11/2026)
- **Bản iPhone chơi campus (không AR) chạy cốt truyện**, cùng scene với PC. Khung thoại phải chạm để qua
  được, nút "Bỏ qua" cho camera giới thiệu bấm được bằng cảm ứng, chữ đủ to ở màn ngang iPhone 12; không che
  điều khiển mobile (`IUHMobileControlsUI`). Kiểm cả hai: Editor PC và Device Simulator iPhone 12 ngang.
- **Combat real-time + khinh khí cầu: tắt trong lúc chạy truyện, bật lại sau khi phá đảo** (chế độ tự do).
  Vẫn phải báo Thịnh và được Thịnh đồng ý trước khi sửa scene (thêm `STORY_DIRECTOR`).

## 9. Kiểm chứng
- EditMode test `StoryProgress`: thứ tự chương, không nhảy chương, lưu/đọc, chơi lại.
- PlayMode thủ công có ghi bằng chứng (`Docs/Evidence/CampusGuardian/STORY/`): ảnh từng giai đoạn; xác nhận
  không quái nào trước biến cố; thắng → chuyển chương; thoát giữa chương → vào lại đúng chương; tắt
  `STORY_DIRECTOR` → game như cũ. Console 0 error, Missing Script = 0.
