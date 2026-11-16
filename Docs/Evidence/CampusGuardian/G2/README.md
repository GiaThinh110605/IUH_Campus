# G2 — lưu kho đồ qua các phiên

Ngày kiểm chứng: 11/11/2026, Asia/Bangkok. Windows Editor, Unity 6000.5.8f1, URP; MCP HTTP.
Nhánh: minhnhat/g2-inventory-persist. Nền: c2eb497c6b109b4ee4a566521ebf0f8e7ad97550 từ origin/minhnhat/fix-mobile-compile (chưa merge origin/main khi fetch). Commit G2 là commit chứa README này, thông điệp feat(guardian): persist inventory items across sessions. Không push/PR.

## Nguyên nhân và thiết kế
WritePlayerPrefs trước đây chỉ ghi Gold và 3 cấp nâng cấp, bỏ m_Items. LoadData cấp đồ khi list rỗng nên không phân biệt dùng hết với người mới. G2 dùng wrapper Serializable version=1 với list items, JsonUtility và key IUH_Guardian_Items. Có key với list rỗng nghĩa là kho đã dùng hết: giữ rỗng. Thiếu key nghĩa là save cũ hoặc người mới: giữ 2 đồ khởi đầu và lưu ngay. Save cũ vẫn giữ nguyên Gold/cấp/cờ milestone; đồ mất từ các phiên trước G2 không thể phục hồi.

JSON hỏng/missing list cảnh báo một lần mỗi lần LoadData, giữ đồ khởi đầu; không ném exception ra ngoài. Entry null/name rỗng hoặc trắng/count không dương bị bỏ qua. API công khai và nhận diện item theo tên không đổi. AddItem và UseItem đều gọi SaveData. Items ghi trong WritePlayerPrefs, batch TryClaimMilestoneReward vẫn chỉ Save một lần cuối (xác nhận bằng review luồng code; không instrument số lần gọi Save).

## Kiểm chứng a–e
| Mục | Kết quả chạy thật |
|---|---|
| a | Compile hoàn tất, isCompiling=false; Console 0 error cuối lượt |
| b EditMode | 74/74 pass; job 94b6a961bfc8434caae6b733186e6548; editmode-final.json |
| b PlayMode lần 1 | 22/22 pass; job 719761ecc8714652a2bf040f2d95d8be; play1.json |
| b PlayMode lần 2 liên tiếp | 22/22 pass; job 4d32063f48e54767b11d6394c7a0f473; play2.json |
| c | MCP Play: Gold 200, 2 đồ khởi đầu; AddItem Potion G2 ×3, UseItem một lần; thoát/vào lại: Potion G2 ×2, 2 đồ cũ và Gold 200 giữ nguyên, Matches=True |
| d | Kết thúc ngoài Play Mode; scene campus isDirty=false, Missing Script=0, Console error=0; EditorSettings khôi phục bản đầu (git diff rỗng); toàn bộ Guardian PlayerPrefs khôi phục 5/5 key, tổng 5, không còn key Items/test |
| e | Bằng chứng JSON, ảnh, snapshot và diff trong thư mục này; review độc lập không có vấn đề chặn |

Toàn bộ suite dùng init_timeout=120000, poll wait_timeout=60. Sau mỗi suite PlayMode đã khôi phục EditorSettings và Guardian PlayerPrefs. Cặp Play Mode thủ công giữ save xuyên lần thoát/vào để kiểm tra restart thực tế; EditorSettings khôi phục ở lần thoát giữa, PlayerPrefs khôi phục sau khi đối chiếu xong cặp phiên (khôi phục sớm sẽ xóa chính save đang kiểm chứng). Sau cặp phiên đã xác nhận 5/5 key, tổng 5.

Ảnh đã xem trực tiếp: inventory-before.png (kho ban đầu), inventory-before-restart.png (sau thêm/dùng), inventory-after-restart.png (sau vào lại). manual-initial.txt, manual-expected.txt, manual-after.txt ghi count đối chiếu. KHO ĐỒ hiển thị trên gameplay có combat đang chạy; không sửa UI/combat/scene để tạo ảnh.

## TDD và bảng ca mới
Test đỏ chạy trước khi sửa production: job 8c7f1c413ab843c1854c52d406f1fdf8, 6 ca đỏ đúng kỳ vọng (red.json; MCP progress có callback lặp, total=6). Sau sửa: job 39ddc0cdff554ca9871ae9b609518525, 6/6 xanh (green.json).

| Ca | Kỳ vọng | Kết quả |
|---|---|---|
| AddedItemSurvivesRestartWithExactCount | Count 3 sau restart | Pass |
| ConsumedLastItemDoesNotGrantStarterItemsAgain | Kho rỗng giữ rỗng sau dùng hết | Pass |
| MissingItemsKeyGrantsExactlyTwoStartersAndSavesImmediately | Đúng 2 đồ, key có version=1 | Pass |
| CorruptJsonWarnsOnceAndKeepsStartersWithoutException | Một warning, không exception, 2 đồ | Pass |
| MilestoneGoldFlagAndItemsSurviveRestartTogether | Gold 125, cờ và 2 item giữ, không nhận lại | Pass |
| InvalidEntriesAreSkippedWhileValidEntryKeepsItsIdentity | Bỏ entry lỗi, giữ id, UseItem lưu count mới | Pass |

Fixture mới snapshot/restore Gold, Items, 3 cấp và milestone test G2.

## Vì sao sửa fixture G1
Lượt PlayMode đầu sau sửa production chạy đủ 22 ca nhưng NegativeGoldDoesNotClaimMilestone thấy 2 thay vì 0 và SecondClaimDoesNotChangeGoldItemsOrNotifications thấy 3 thay vì 1 (playmode-all-first.json). Fixture VictoryRewardTests cũ không xóa/khôi phục Items nên đồ từ ca trước tồn tại từ khi có persistence G2. Người dùng đã cho phép bổ sung riêng fixture: snapshot STRING Items bằng HasKey/GetString/null, DeleteKey trong SetUp, SetString/DeleteKey trong TearDown trước một Save cuối. Không đổi assert, ca test hoặc production thêm. Hai lần 22/22 sau đó xác nhận hết rò state.

## Chẩn đoán và bảo toàn workspace
Lượt đầu trước G2 bị timeout khởi tạo 120 giây, 0 ca chạy; không coi là test đỏ. Theo phép người dùng, tìm GUID không còn tham chiếu và chỉ xóa InitTestScene21169521-a4ba-432c-b3ce-a4717f0ef0b7.unity cùng meta. clear_stuck không còn job; editor/state tests.is_running=false, ready_for_tools=true. Chạy riêng group InventoryPersistenceTests với timeout=180000 sau dọn thành công. Không sửa MCP, Packages hoặc test framework.

Scene campus lệch 6 dòng m_MinSize/m_MaxSize và material CombatArena là nhiễu có sẵn trước lượt chẩn đoán: không stage, không khôi phục. Logs/csproj/slnx/material khác và file ngoài phạm vi giữ nguyên. Một lần git diff --check toàn workspace báo whitespace trong csproj/Logs có sẵn; kiểm tra giới hạn file G2 sạch. Snapshot đầu: playerprefs-before.txt, EditorSettings.before.txt. inventory.diff là diff production. git-show-stat.txt/git-status.txt ghi trạng thái trước commit (commit nền); kết quả HEAD sau commit báo trực tiếp cho người dùng.

Rà soát độc lập read-only: đáp ứng persistence/empty/fallback/filter, giữ API/batch, fixture G1 đúng phạm vi; không có vấn đề chặn. Chưa kiểm chứng build standalone/iOS (ngoài phạm vi lượt).
