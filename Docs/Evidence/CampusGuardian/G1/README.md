# G1 — Gold và vật phẩm thắng match-3 một lần mỗi milestone

Ngày 10/11/2026, Asia/Bangkok. Codex, phần Minh Nhật.
Nhánh `minhnhat/g1-victory-gold`, nền B8 `3c9b857cd3455e91a275dd08d464ac802f52a2ee`.
B8 chưa merge origin/main `6432dc0` khi bắt đầu; không mang FIX-PURIFY sang.
Commit G1: commit chứa README này; tra
`git log -1 --format=%H -- Docs/Evidence/CampusGuardian/G1/README.md`.
Các test chạy trên working tree G1 trước commit. Không push/PR.
Môi trường: Windows, Unity 6000.5.8f1 Editor + MCP; kiểm ảnh game view thật.

## Thiết kế

Gold và cờ milestone thuộc save cục bộ, không đợi core_awarded hoặc API,
để chơi offline. TryClaimMilestoneReward kiểm null/rỗng/Gold âm/cờ đã nhận trước
khi đổi trạng thái; batch AddGold/AddItem để SaveData/Notify trung gian không
thực thi; cuối batch ghi Gold và cờ PlayerPrefs, gọi Save một lần và
OnDataChanged một lần. Các hàm public khi gọi riêng giữ hành vi cũ.
Prefix milestone là const. Chỉ dùng IUHGuardianInventory của Thịnh.

Encounter claim/cảnh báo thiếu inventory-ID một lần; formatter phân biệt được
nhận, đã nhận và lỗi. Manager chụp alreadyClaimed trước claim, guard một lần
mỗi trận và reset StartNewCombat. Telemetry B8 và timing giữ nguyên.
Đổi đúng ba file runtime; thêm VictoryRewardTests.cs + .meta, không sửa test cũ,
DailyActivities/check-in/backend/Server hoặc giá trị thưởng scene.
Diff ba file: runtime.diff. Review chỉ đọc không phát hiện Critical/Important.

## Bảng kiểm chứng

| ID | Ca | Mong đợi | Thực tế | Pass/Fail |
|---|---|---|---|---|
| red | PlayMode G1 trước implementation | Đỏ do thiếu API | 7/7 fail đúng Milestone reward API must exist | Tái hiện đỏ đúng |
| a | Compile/Console | Compile xong, 0 error | Editor không compiling, Console cuối 0 error | Pass |
| b1 | VictoryRewardTests | Nhận đầu/lặp, sai ID/Gold, restart giả lập, notify | 7/7 passed, failed0/skipped0 | Pass |
| b2 | Toàn EditMode | Chỉ lỗi nền được cho phép | 69/69 hoàn tất, chỉ ProcessStep_CountsPurifyTiles expected3/actual0; result=null | Có sẵn, đã sửa ở nhánh minhnhat/fix-match3-purify |
| b3 | Toàn PlayMode Match3 | G1 xanh, chỉ lỗi nền được cho phép | 16/16 hoàn tất, chỉ FreeCursorStopsPendingMovementOnFirstApply NullReferenceException; result=null | Có sẵn, đã sửa ở nhánh minhnhat/fix-match3-purify |
| c1 | Thắng đầu, giữ check-in | +100 Gold và modal thưởng | Phép đo khóa đúng MONSTER_05: 535→635, cờ test1, modal +100 và item | Pass |
| c2 | Thắng lặp cùng milestone | Gold không đổi và thông báo đã nhận | Sau Gold ổn định4.006s: ngay trước thắng635, ngay khi modal hiện635, savedGold635; Đã nhận thưởng khu vực này | Pass |
| cleanup | Khôi phục save test | Trả Gold/keys về trước phép đo | Gold200, test keys không tồn tại;117 key snapshot khớp sau cleanup, gồm check-in và ngày album/badge | Pass |
| d1 | EditorSettings sau PlayMode | Khôi phục nếu Runner đổi | Checkout riêng sau mỗi lần PlayMode; diff cuối rỗng, value0 | Pass |
| d2 | Scene campus | missingScripts0, isDirty=false, diff scene rỗng | missingScripts0/brokenPrefabs0, isDirty=false,49 roots; diff stat rỗng,5 milestone đúng | Pass |
| e | Hồ sơ | Ghi kết quả thật và giới hạn | Raw MCP, ảnh đầu/lặp, snapshot restore, diff và giải thích scene | Đã ghi |

Hai suite có result=null giữ nguyên progress/failures_so_far, không tự dựng số
passed/skipped hoặc gọi toàn suite Pass. Hai lỗi cũ được prompt G1 cho phép giữ.
MCP lỗi kết nối ở lượt đầu nằm trong mcp-errors.json; các test sau đều chạy thật.

## Phép đo thắng lặp cuối

Không tắt check-in, không sửa code/scene check-in. Dùng encounter thật
MONSTER_05_CORRUPTED_BACKPACK, RewardGold100 và item giữ nguyên.
Chỉ trong Play Mode tạm đặt milestone test_g1_manual, dùng StartNewCombat rồi
ép outcome Victory và chạy VictoryRoutine; không gọi trực tiếp claim để thay manager.

Observer kiểm thử trong Editor đợi Gold không đổi ít nhất4 giây, xác minh đúng
encounter/test milestone, lấy Gold ngay trước ép thắng và ở frame đầu modal hiện.
Lượt đầu:535→635 (+100), marker1. Lượt lặp:635→635, marker1,
modal “Đã nhận thưởng khu vực này”. Xem manual-retry.json và
victory-first.png/victory-repeat.png (đã kiểm ảnh). Observer tự hủy sau ghi nhận.
Ảnh được chụp sau thời điểm observer; số Gold đánh giá lấy từ observer, không
lấy chênh lệch giữa các lệnh MCP cách nhau nhiều giây.

Outbox tạm cùng thư viện/reporter B8 tại manual-retry-outbox.json, endpoint
127.0.0.1:1, không chạm hàng đợi thật và không cần phản hồi backend để cộng Gold.
Không kiểm cloud hoặc tuyên bố synced. Không đổi backend source/config asset.

Một probe đầu trong lượt đo lại không khóa encounter, bị chuyển đích và hiện
+60 của quái khác; đã loại khỏi kết quả đánh giá và đưa claim campus_cong_chinh
vừa phát sinh về chưa nhận khi cleanup. Observer cuối khóa đúng encounter và
kiểm ID trước khi ép thắng. Giữ raw probe trong manual-retry.json để truy vết.

## Quan sát +70 trước đây

Lượt thủ công ban đầu:200→300 khi thắng đầu, sau đó quan sát370 kèm hai badge
Khởi Đầu Tương Lai và Giọt Nước Tri Thức. Source check-in thưởng35 mỗi lượt,
phù hợp khoản+70 quan sát; đây là hoạt động ngoài G1. Không sửa hoặc tắt check-in.
Giữ raw manual.json và ảnh victory-first-initial.png/victory-repeat-initial.png
làm lịch sử, không dùng chênh300→370 để kết luận claim bị lặp.

Trong lượt đo lại, các hoạt động khác cũng làm Gold thay đổi trước/sau cửa sổ
đánh giá. Vì vậy chờ ổn định rồi đo sát trước/sau modal. Sau khi thoát Play,
Gold được trả về200, marker test_g1_manual/test_g1_victory không còn tồn tại.
Đã snapshot/khôi phục cả flag IUH_CHECKIN_SPOT_* và string IUH_CHECKIN_DATE_SPOT_*
(album/badge), cùng key daily và coin index0..99 của ngày UTC hiện tại.
prefs-restored.json ghi117 key trước/sau và toàn bộ thay đổi; so sánh khớp.
Không lưu m_Items nên item/badge thêm trong runtime biến mất khi thoát Play.

## Scene rỗng milestone và khôi phục

Scene từng bị ghi lại lúc làm trên nhánh chưa có field m_MilestoneId, khiến
5 ID B8 trên đĩa thành rỗng. Diff lịch sử: scene-outside-scope.diff.
Theo phép người dùng: kiểm scene isDirty=false, checkout riêng scene về HEAD,
chọn Reload tại hộp thoại Unity (không Save), rồi đọc lại qua MCP.
Diff scene cuối rỗng; không stage scene.

| Quái | Milestone sau Reload và sau test |
|---|---|
| 01 | campus_cong_chinh |
| 02 | campus_cantin |
| 03 | campus_le_hoi |
| 04 | campus_dau_truong_bac |
| 05 | campus_cong_ar |

Lần trước vẫn cộng200→300 dù ID scene trên đĩa rỗng vì trước khi ép thắng,
fixture đã gán `test_g1_manual` cho encounter đang chạy trong Play Mode.
Claim đọc ID runtime tạm này, không đọc lại YAML trên đĩa. Do đó cờ test hợp lệ
và+100 được nhận. Cleanup lượt cũ trả runtime ID về chuỗi rỗng đã chụp lúc đầu;
nay đã Reload đúng B8 trước khi đo lại. Không đổi giá trị thưởng trong scene.

## Giới hạn và Git

m_Items chưa lưu bền theo phạm vi: item trong phiên có thể mất khi tạo inventory
mới; Gold/cờ milestone lưu bền. Không gọi đó là inventory persistence đầy đủ.
Ledger không theo ngày/giờ nên đổi giờ máy không tạo lại quyền nhận milestone.
Một PlayerPrefs.Save là batch được yêu cầu, không cam kết transaction khi process
chết hoặc thiết bị lỗi ghi. Chưa build hoặc kiểm thiết bị.

Chỉ stage3 file runtime, test mới+.meta và G1/. Không stage scene, ProjectSettings,
Logs/project files/material/.vsconfig/screenshot cũ/patch. Commit một lần,
không push hoặc tạo PR.
