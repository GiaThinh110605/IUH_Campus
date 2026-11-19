# Nâng cấp có tác dụng trong match-3 — Kế hoạch thực hiện

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Cấp kiếm/khiên/sinh lực mua ở shop đổi đúng DMG/ô kiếm, Giáp/ô khiên và máu tối đa trong trận match-3, và shop ghi đúng con số đó.

**Architecture:** Đổi 3 bảng số trong `IUHGuardianInventory` sang thang của trận. `Match3CombatManager` lưu giá trị gốc của `m_Config` khi `Awake` và đặt lại 3 chỉ số theo kho ngay trước mỗi lần tạo `Match3CombatRules`. Không có kho thì dùng giá trị gốc.

**Tech Stack:** Unity 6000.5.8f1, C#, Unity Test Framework (PlayMode assembly `IUHCampus.Guardian.Match3.PlayModeTests`, gọi Assembly-CSharp qua reflection như các test hiện có), Unity MCP.

**Spec:** `Docs/superpowers/specs/2026-11-19-upgrades-affect-match3-design.md`

## Global Constraints
- Nhánh `minhnhat/upgrades-match3` từ `origin/main` `73d05da`. Không commit vào `main`, không push, không amend. Stage chọn lọc; không stage `*.csproj`, `*.slnx`, `Logs/`, `ProjectSettings/`, `*.mat` Unity tự ghi, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`, scene chỉ đổi xuống dòng.
- Bảng số đúng spec 4.1: kiếm `{22, 25, 29, 34, 40}`, khiên `{15, 18, 21, 25, 30}`, máu `{1000, 1100, 1220, 1360, 1500}`. Giá giữ nguyên.
- Không đổi: giá, số cấp, Vàng thưởng, chỉ số quái, `SkillDamage`, năng lượng, ô hồi máu và ô pha lê, `PlayerMaxShield`, giao diện shop, combat AR, backend.
- Không sửa scene. Giá trị `m_Config` trong scene (22/15/1000) là giá trị gốc.
- Mỗi Task: compile xong, Console 0 error, test liên quan Pass.

## Review Focus
1. **Thử lại (`RetryCombat`) dùng chỉ số cũ** → mọi đường tạo `Match3CombatRules` phải qua bước áp chỉ số. Task 2 kiểm `RetryCombat` có đi qua `BeginCombatRoutine` không; nếu có đường khác thì cũng gọi ở đó.
2. **Trận trước có kho, trận sau không có kho** → phải trở về 22/15/1000, không giữ số cũ. Đã có test ở Task 2.
3. **Save có cấp ngoài 1–5** (sửa tay PlayerPrefs) → các hàm `Get*` đã `Clamp`, không văng lỗi. Đã có test ở Task 1.
4. **Màn thắng ghi "Còn X/Y HP"** dùng `m_Config.PlayerMaxHp` → phải ra máu mới (ví dụ 1360). Kiểm tay ở Task 3.
5. **Chữ shop dài hơn** ("Cấp 5: 40 DMG / ô kiếm", "1500 Máu Tối Đa") → không tràn khung. Kiểm tay ở Task 3; tràn thì chỉ chỉnh khung/cỡ chữ của đúng ô đó.

---

### Task 0: Đo trước
- [ ] Kiểm `git branch --show-current` = `minhnhat/upgrades-match3`, scene `isDirty=false`.
- [ ] Chạy EditMode + PlayMode và ghi số test vào `Docs/Evidence/CampusGuardian/UPGRADES-MATCH3/README.md`. Mong đợi khoảng 104 và 31.

### Task 1: Bảng số kho theo thang trận

**Files:**
- Modify: `Assets/IUH_Campus/Scripts/Guardian/IUHGuardianInventory.cs` (các mảng `s_SwordDamages`, `s_ShieldValues`, `s_MaxHps`)
- Test: `Assets/IUH_Campus/Scripts/Guardian/Match3/Tests/PlayMode/UpgradeStatsTests.cs` (mới, kèm `.meta`)

**Interfaces:**
- Produces: `GetSwordDamage()`, `GetShieldValue()`, `GetMaxHp()` trả số theo bảng mới (chữ ký giữ nguyên).

- [ ] **Step 1: Viết test lỗi**
  - `InventoryTablesMatchSpec`: tạo `IUHGuardianInventory` trên GameObject inactive. Đặt `m_SwordLevel`, `m_ShieldLevel`, `m_MaxHpLevel` qua reflection cho cấp 1..5. Assert lần lượt kiếm `22,25,29,34,40`, khiên `15,18,21,25,30`, máu `1000,1100,1220,1360,1500`.
  - `Level1EqualsMatch3ConfigDefaults`: cấp 1 thì `GetSwordDamage() == new Match3CombatConfig().DmgPerSword`, tương tự `ShieldPerTile` và `PlayerMaxHp`.
  - `OutOfRangeLevelIsClamped`: cấp 0 cho ra số của cấp 1; cấp 9 cho ra số của cấp 5.
  - Sao lưu và khôi phục mọi PlayerPrefs `IUH_Guardian_*` mà test có thể ghi, cùng `IUHGuardianInventory.Instance` (theo mẫu `RepeatedCheckInAwards35GoldOnlyOnce` trong `CombatOutcomeTests.cs`).
- [ ] **Step 2: Chạy, xác nhận Fail** (MCP `run_tests` PlayMode, `test_names` của 3 test). Mong đợi: Fail ở giá trị 6/5/100.
- [ ] **Step 3: Sửa 3 mảng theo Global Constraints.**
- [ ] **Step 4: Chạy lại, xác nhận Pass.**
- [ ] **Step 5: Commit** `feat(upgrades): rescale sword, shield and max HP tables to match-3 values` (kèm spec, plan, file prompt).

### Task 2: Trận đọc chỉ số từ kho

**Files:**
- Modify: `Assets/IUH_Campus/Scripts/Guardian/Match3/Match3CombatManager.cs` (`Awake`, `BeginCombatRoutine` ~dòng 206, kiểm `RetryCombat` ~dòng 164)
- Test: `UpgradeStatsTests.cs`

**Interfaces:**
- Consumes: `IUHGuardianInventory.Instance`, `GetSwordDamage()`, `GetShieldValue()`, `GetMaxHp()`.
- Produces: `private void ApplyPlayerUpgradeStats()` trong `Match3CombatManager`, cùng 3 field gốc `m_BaseDmgPerSword`, `m_BaseShieldPerTile`, `m_BasePlayerMaxHp` được gán trong `Awake`.

- [ ] **Step 1: Viết test lỗi**
  - `CombatUsesInventoryUpgradeLevels`: kho cấp kiếm 3, khiên 2, máu 4. Tạo `Match3CombatManager` trên GameObject (Awake chạy), gọi `ApplyPlayerUpgradeStats` qua reflection. Assert `m_Config.DmgPerSword == 29`, `ShieldPerTile == 18`, `PlayerMaxHp == 1360`.
  - `CombatFallsBackToBaseStatsWithoutInventory`: như trên rồi đặt `IUHGuardianInventory.Instance = null`, gọi lại. Assert 22/15/1000.
- [ ] **Step 2: Chạy, xác nhận Fail** (không có method).
- [ ] **Step 3: Cài đặt.**
  - `Awake`: lưu 3 giá trị gốc từ `m_Config`.
  - `ApplyPlayerUpgradeStats()`: có kho thì gán theo `Get*`, không có kho thì gán giá trị gốc.
  - Gọi hàm này ở dòng đầu `BeginCombatRoutine`, trước `new Match3CombatRules(m_Config)`. Nếu `RetryCombat` hay đường nào khác tạo `Match3CombatRules` mà không qua `BeginCombatRoutine`, gọi ở đó luôn.
- [ ] **Step 4: Chạy lại, xác nhận Pass.** Sau đó chạy toàn bộ EditMode + PlayMode và xác nhận Pass.
- [ ] **Step 5: Commit** `feat(match3): apply sword, shield and max HP upgrades at combat start`.

### Task 3: Kiểm chứng và bằng chứng
- [ ] Missing Script = 0 ở 3 scene và mọi prefab; Console 0 error.
- [ ] Chơi thử (sao lưu và khôi phục PlayerPrefs `IUH_Guardian_*`):
  - Dùng `execute_code` cộng Vàng, mở shop: chữ cấp 1 là "22 DMG / ô kiếm", "15 Giáp / ô khiên", "1000 Máu Tối Đa".
  - Nâng kiếm lên 3 và máu lên 4. Chữ shop đổi đúng và không tràn khung.
  - Vào 1 trận: thanh HP ghi 1360. Đọc `m_Config` qua `execute_code`: 29/15/1360. Thắng: màn thắng ghi "/1360 HP".
  - Bấm Thử lại: chỉ số vẫn 29/15/1360.
- [ ] Ghi README: commit, ngày, môi trường, số test trước/sau, kết quả từng mục (Pass/Fail), ảnh shop và HUD trận nếu chụp được.
- [ ] Commit `docs(upgrades): record match-3 upgrade verification`. Không push.
