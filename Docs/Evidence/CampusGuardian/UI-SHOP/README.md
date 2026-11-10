# UI-SHOP — Gold trong thanh, bỏ glyph không hỗ trợ

Ngày10/11/2026, Asia/Bangkok. Codex, phần Minh Nhật.
Windows, Unity6000.5.8f1 Editor+MCP, LegacyRuntime, Game view.
Nhánh `minhnhat/fix-upgrade-shop-ui`, tạo ngay tại HEAD G1
`d0db991d53963bb875a25618a0989c8f75870a98`; không checkout nội dung nhánh khác
khi Unity mở. Trước sửa: không Play, scene sạch, Console0 error, đủ5 milestone B8.
Commit UI-SHOP: commit chứa README này; tra bằng
`git log -1 --format=%H -- Docs/Evidence/CampusGuardian/UI-SHOP/README.md`.
Không push/PR.

## Nguyên nhân và thay đổi

TxtGold (GameObject2063332147, RectTransform2063332148, Text2063332149) neo
x0.05 với pivot giữa, rộng400 nên tràn trái GoldBar. Bounds trước là
[-506,-106] trong thanh[-340,340]. Nay stretch (0,0)→(1,1), inset ngang12,
MiddleLeft, bounds[-328,328] bên trong thanh. Giữ màu và font.

Ba Title hàng nâng cấp có emoji gây glyph đỏ/không hiện; bỏ emoji, giữ màu
đỏ/xanh dương/xanh lá. Title/Cur cũng mắc lỗi pivot giữa sát trái: Title left-449,
Cur left-424 trong row left-325. Sửa cùng loại: pivot trái cho Title/Cur,
Cur rộng200; Next pivot trái để giữ hai cột tách nhau. Sau sửa cả ba Title/Cur
left-299, Next left-91, nằm trong row[-325,325]. Không đổi giá hoặc logic.

Cùng lỗi glyph không hỗ trợ trong shop: bỏ emoji không hiện ở title trạm,
tab Kho đồ/Ngoại trang và header trang phục runtime. Giữ gear ở tab nâng cấp,
sparkle ở item và lightning ở trails vì ảnh hiển thị được.
IUHUpgradeShopUI.cs chỉ đổi2 chuỗi: Gold bỏ💰, header trang phục bỏ👗.
Không sửa Builder Editor, inventory, nâng cấp, listener mua hoặc ProjectSettings.

## Kiểm chứng a–e

| ID | Ca | Mong đợi | Thực tế | Kết quả |
|---|---|---|---|---|
| a | Compile/Console | Compile xong,0 error | Editor idle; read_console0 error trước/sau | Pass |
| b0 | Ảnh trước | Giữ bằng chứng lỗi gốc | before.png, before-inventory.png, before-skins.png ở1920×1080 | Đã chụp |
| b1 | Tab nâng cấp sau | Gold trong thanh,3 title thẳng hàng, không glyph đỏ | after-upgrades.png + layout bounds xác nhận | Pass |
| b2 | Tab kho sau | Không ô chữ tràn trái/glyph lỗi | after-inventory.png, item text và nút nằm trong panel | Pass quan sát |
| b3 | Tab ngoại trang sau | Kiểm cùng loại lỗi | Header không còn glyph lỗi;2 hàng cuối vẫn tràn dưới viewport/card, lỗi cuộn/clipping khác loại được ghi riêng | Đã kiểm; giới hạn còn lại |
| b4 | iPhone ngang mô phỏng | Gold không tràn | Game view2532×1170, after-iphone-landscape.png, inside=true | Pass mô phỏng Editor |
| c1 | EditMode toàn bộ | Ghi số thật |69/69 hoàn tất, chỉ ProcessStep_CountsPurifyTiles expected3/actual0; result=null | Có sẵn, đã sửa ở nhánh minhnhat/fix-match3-purify |
| c2 | PlayMode Match3 | Ghi số thật |16/16 hoàn tất, chỉ FreeCursorStopsPendingMovementOnFirstApply NullReferenceException; result=null | Có sẵn, đã sửa ở nhánh minhnhat/fix-match3-purify |
| d | Cleanup/scene | Scene sạch,0 Missing Script, đúng phạm vi | isDirty=false, missingScripts0/brokenPrefabs0,5 milestone đúng; diff chỉ17 object UI;126 key prefs khôi phục khớp; EditorSettings diff rỗng | Pass |
| e | Hồ sơ/outbox | Chỉ đọc, không xóa queue | Raw tests, ảnh, layout/outbox/emoji/prefs audit, code.diff và scene.diff | Đã ghi |

Mở shop qua OpenShop(0), cùng đường gọi của handler phím B; chuyển tab bằng
OpenShop(1/2), không bấm mua/nâng cấp/dùng item. Không kiểm độc lập thao tác bàn phím B.
Ảnh game view thật đã kiểm trực quan. Tỉ lệ iPhone chỉ mô phỏng trên Editor, chưa
build/chạy thiết bị. Game view được trả index11, custom size count5 như trước;
size thử tạm đã gỡ, không đổi ProjectSettings.

Hai suite result=null giữ nguyên phản hồi MCP và failures_so_far; không dựng
summary hoặc ghi toàn suite Pass. Không sửa hai lỗi nền trong UI-SHOP.

## Outbox — chỉ đọc

Đọc file thật
`C:/Users/Admin/AppData/LocalLow/DefaultCompany/IUH_Campus/guardian_outbox.json`.
Trước Play:1 pending, rejected0, event81da8955-ef93-44f3-b6b0-1a1173c51ac5,
combat_won, milestone campus_cong_ar, attempts3.
Sau Play: event này attempts8 với maxAttempts8; pending thêm combat_lost
72e73027-2dae-4573-99c6-d56cfab988bf của campus_cong_ar, attempts0.
Client hiện có tự retry/ghi khi chạy Play và tự báo timeout; agent không trực tiếp
ghi/xóa file, không gọi RetryFailed, không gửi lại POST để điều tra.

Failed hiện tại là mục pending đạt giới hạn8, milestone không phải test_*.
Schema chỉ lưu id/json/attempts/nextAttemptTicks, không lưu HTTP status hoặc
last error. Vì vậy mã HTTP cụ thể chưa xác định từ file; không suy đoán400/401/503.

Snapshot G1 manual-retry-outbox.json (file bằng chứng, không phải file thật)
có3 pending, một mục đạt8: b31a97c4-b5da-4054-929f-93fa909eeb35,
campus_cong_chinh (probe G1); phía sau là2 event test_g1_manual chưa thử.
Mẫu3 chờ/0 nhận/1 lỗi phù hợp HUD trong phiên G1 dùng outbox tạm. Chưa chứng
minh ảnh HUD cũ đọc chính file đó; file thật hiện tại có2 pending/rejected0.
Chi tiết outbox-audit.json.

Đề xuất sau khi được phép: sao lưu file thật, khôi phục kết nối API rồi dùng
RetryFailed để retry giữ UUID/chống trùng. Nếu sau này queue thật có test_*
đã xác minh thì chỉ dọn từng entry test sau phê duyệt; không xóa cả outbox.
Giữ các file outbox G1 làm bằng chứng lịch sử, không tự xóa.

## Ngoài phạm vi và việc sau

- Ngoại trang: danh sách8 lựa chọn +2 header vượt chiều cao viewport/card;2 hàng
  cuối tràn dưới. Đây là vấn đề clipping/scroll, không sửa trong lượt này.
- Ảnh nền có floating text chiến đấu lớn và vật liệu magenta của module combat,
  ghi nhận thôi, không sửa asset hoặc combat.
- Còn63 Text component tĩnh của scene có Unicode Extended_Pictographic, gồm
  cả object inactive; không tính Text item/skin tạo runtime. emoji-audit.json
  liệt kê để làm lượt sau. Ngoài shop không thay emoji.
- Kiểm dropdown/tab và ảnh; chưa test gamepad, max font size, rất nhiều item,
  build hoặc máy iPhone thật.

## Git và bảo toàn dữ liệu

Scene.diff chỉ17 RectTransform/Text dưới IUH_Upgrade_Shop_UI,22 thêm/22 xóa:
TxtGold,3 hàng và nhãn shop cùng lỗi. Không có m_MilestoneId, GameObject khác,
float serialize vô nghĩa hoặc reward. Code.diff chỉ2 chuỗi hiển thị.
Không sửa YAML bằng tay. Review độc lập read-only không có Critical/Important.

Snapshot126 key gồm Gold/upgrade, check-in/date, daily/coin, skin/trail/ownership;
sau mọi test khôi phục và so sánh khớp. EditorSettings checkout riêng sau mỗi
PlayMode, không stage. Outbox chỉ đọc như mô tả trên, không khôi phục/xóa queue.
Stage chọn lọc scene, IUHUpgradeShopUI.cs và UI-SHOP/, không file ngoài phạm vi.
