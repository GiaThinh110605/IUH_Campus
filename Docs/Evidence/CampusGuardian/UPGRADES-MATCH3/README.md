# UPGRADES-MATCH3

19/11/2026 · Windows Editor · Unity 6000.5.8f1 · nhánh minhnhat/upgrades-match3 · base 73d05da.

- edb92d8: bảng kiếm/khiên/máu theo thiết kế. Ba test fail trước sửa (01c7171fc9cd48eba0c633a27691b3ce, kiếm cấp 1 được 6 thay vì 22) và 3/3 Pass sau sửa (a08460419e924448bb82e4f2cbec0489).
- 836fca3: áp chỉ số trước điểm runtime duy nhất tạo Match3CombatRules. Thử lại qua cùng đường này. Hai test tích hợp fail do thiếu phương thức (4108b6560aad4f43a547c1117bc42cf1) và Pass trong suite cuối. Có kho 3/2/4 cho 29/18/1360. Bỏ kho trả 22/15/1000.
- Lệch chi tiết Task 1: trong cùng file Inventory, giữ bảng HP ngoài trận 100/125/155/190/240 cho hai chỗ RestoreFullHp và UseItem cập nhật HUD trên đầu nhân vật. GetMaxHp cho shop/match-3 dùng bảng mới. Cần tách vì đổi cả số hiện khi nhận tiếp tế hoặc uống bình nằm ngoài phạm vi match-3.

| Bộ test | Trước | Sau |
|---|---:|---:|
| EditMode toàn dự án | 104/104 (187ea479bb8b4b1a921acdfebad2a150) | 104/104 (3572527a80984e9398459598039ae756) |
| PlayMode toàn dự án | 30/30 (e487361166b040e4af8dc9f0bf0d129d) | 35/35 (f6bce583f70a4f4e92d8b59d71a900fa) |

### Kiểm runtime Task 3

| Mục | Kết quả | Quan sát |
|---|---|---|
| Shop cấp 1 | Pass | 22 DMG / ô kiếm, 15 Giáp / ô khiên, 1000 Máu Tối Đa. |
| Nâng kiếm 3, máu 4 qua nút shop | Pass | Chuỗi hiển thị 29 DMG, 1360 Máu Tối Đa. Text preferred height tối đa 14.17 so với khung 22 ở độ phân giải Editor hiện tại, kể cả nhãn tiếp theo 1500. Chưa đối chiếu bằng ảnh. |
| Vào trận | Pass | TriggerCombat ở Orc, HUD HP 1360, config 29/15/1360. ApplyStep với ba ô kiếm tạo 87 sát thương ở combo đầu (cấp 1 là 66 theo config). Đây là bước luật được đưa vào qua MCP. |
| Thắng | Pass với mô phỏng | Gọi VictoryRoutine qua MCP và bỏ phát sự kiện/thưởng trong lượt thử. Chữ màn thắng Còn 1360/1360 HP. Chưa chơi bảng tới thắng tự nhiên. |
| Thử lại | Pass | Gọi onClick của nút Thử lại thật từ modal thắng, cấu hình trận mới 29/15/1360. |
| Unity và PlayerPrefs | Pass | 0 error. Missing Script 0 ở campus, scene AR, arena và 83 prefab. Đã thoát Play và so sánh 5/5 khóa IUH_Guardian mà lượt thử có thể ghi (Gold, SwordLv, ShieldLv, MaxHpLv, Items) sau khi khôi phục, gồm trạng thái khóa không có. |

Các thao tác Task 3 thực hiện qua MCP. Không chụp ảnh trong lượt này. Scene, giá, thưởng, AR và backend giữ nguyên.
