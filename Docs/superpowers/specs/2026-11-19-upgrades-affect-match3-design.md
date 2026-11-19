# Thiết kế: Nâng cấp kiếm/khiên/sinh lực có tác dụng trong match-3

Ngày: 19/11/2026 · Người yêu cầu: Minh Nhật · Trạng thái: đã duyệt 19/11/2026 (cách A, bảng số như mục 4.1)
Lượt 2/7 của `Docs/superpowers/plans/2026-11-18-lo-trinh-chinh-sua.md` (bản PC/laptop).

## 1. Vì sao
Vòng lặp "thắng → Vàng → nâng cấp → mạnh hơn" đang gãy:
- Shop (`IUHUpgradeShopUI`) trừ Vàng và tăng cấp trong `IUHGuardianInventory`.
- `Match3CombatManager` chỉ dùng `m_Config` cố định: 22 DMG/ô kiếm, 15 Giáp/ô khiên, 1000 HP. Nó không đọc cấp trong kho.
- Bảng số trong kho còn ở thang khác: kiếm 6→18, khiên 5→20, máu 100→240. Vì vậy shop hiển thị con số không khớp với trận.

## 2. Mục tiêu / Không làm / Chống mục tiêu
**Mục tiêu**
- Cấp kiếm, khiên và sinh lực đổi đúng sát thương mỗi ô kiếm, giáp mỗi ô khiên và máu tối đa trong trận match-3.
- Con số shop ghi trùng với con số trong trận.

**Không làm**
- Không đổi giá nâng cấp, số cấp (5), Vàng thưởng, máu và sát thương quái, skill, năng lượng, ô hồi máu và ô pha lê.
- Không đổi giao diện shop, chỉ đổi số hiển thị qua bảng dữ liệu.
- Không đụng combat AR (`IUHARRuneSlashCombat`) và backend (`UpgradePurchased` giữ nguyên).

**Chống mục tiêu**
- Người chưa nâng cấp (cấp 1) chơi khác với hiện tại.
- Một trận dùng nhầm chỉ số của trận trước, ví dụ khi kho không có trong scene.
- Thanh máu hoặc chữ "Còn X/Y HP" sai khi máu tối đa khác 1000.

## 3. Các cách đã cân nhắc
| Cách | Đánh đổi |
|---|---|
| **A. Đổi bảng số trong kho sang thang của trận; trận đọc cấp từ kho (ĐÃ CHỌN)** | Shop ghi đúng số trong trận, dễ hiểu và dễ trình bày; phải sửa 3 bảng số |
| B. Giữ bảng shop, mỗi cấp cộng % | Ít sửa số; người chơi khó hình dung, shop phải đổi chữ |

## 4. Thiết kế
### 4.1 Bảng số (`IUHGuardianInventory`)
| Cấp | 1 | 2 | 3 | 4 | 5 |
|---|---|---|---|---|---|
| `s_SwordDamages` (DMG/ô kiếm) | 22 | 25 | 29 | 34 | 40 |
| `s_ShieldValues` (Giáp/ô khiên) | 15 | 18 | 21 | 25 | 30 |
| `s_MaxHps` (Máu tối đa) | 1000 | 1100 | 1220 | 1360 | 1500 |

Giá giữ nguyên (kiếm/khiên 80/150/250/400, máu 100/180/300/500). Cấp 1 bằng đúng giá trị hiện tại của trận.

### 4.2 Trận đấu (`Match3CombatManager`)
- Lưu giá trị gốc của `m_Config` (`DmgPerSword`, `ShieldPerTile`, `PlayerMaxHp`) một lần khi khởi tạo.
- Mỗi lần bắt đầu trận (gồm cả Thử lại), trước khi tạo `Match3CombatRules`:
  - Có `IUHGuardianInventory.Instance`: đặt ba giá trị trên theo `GetSwordDamage()`, `GetShieldValue()`, `GetMaxHp()`.
  - Không có kho: trả về giá trị gốc đã lưu.
- Thanh máu, chữ HP và màn thắng đã đọc `m_Config.PlayerMaxHp`, nên tự đúng.
- `PlayerMaxShield` (300) giữ nguyên.

### 4.3 Save cũ
Cấp đã mua trong PlayerPrefs được giữ và bắt đầu có tác dụng. Không cần chuyển đổi dữ liệu.

## 5. Kiểm chứng
- EditMode: bảng số đúng 5 cấp; cấp 1 bằng giá trị mặc định của `Match3CombatConfig`.
- PlayMode: kho cấp 3/2/4 thì trận có `DmgPerSword` 29, `ShieldPerTile` 18, `PlayerMaxHp` 1360. Bỏ kho thì trận sau trở về 22/15/1000.
- Toàn bộ test EditMode/PlayMode Pass, Console 0 error, Missing Script = 0.
- Chơi thử: nâng kiếm và máu trong shop, vào trận thì thanh HP ghi đúng máu mới, đòn kiếm gây sát thương lớn hơn.
- Bằng chứng: `Docs/Evidence/CampusGuardian/UPGRADES-MATCH3/`.

## 6. Để người thực hiện quyết
- Chỗ gọi cụ thể (trong `StartNewCombat` hay `BeginCombatRoutine`), miễn là chạy trước khi tạo `Match3CombatRules` ở mọi đường vào trận, gồm cả Thử lại.
- Chữ trong shop giữ nguyên định dạng; chỉ sửa nếu bị tràn chữ do số mới dài hơn.
