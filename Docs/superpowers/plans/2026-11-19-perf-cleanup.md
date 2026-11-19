# Lượt 5 — Dọn hiệu năng: tra cứu mỗi frame và lỗi `Invalid AABB`

Ngày: 19/11/2026. Nhánh: `minhnhat/perf-cleanup` (từ `main` `599913b`, đã có #41 và #42).
Mục đích (rubric): game ổn định khi demo. Lượt nhỏ, không đổi cách chơi.

## 1. Kết quả rà soát (Claude, 19/11)
Không có chỗ nào quét `Find` nặng mỗi frame trong lúc chơi bình thường. Phần lớn `Find` trong `Update` chỉ chạy khi chưa tìm thấy người chơi. Những chỗ đáng sửa:

| # | File | Vấn đề |
|---|---|---|
| A | `UI/IUHMobileControlsUI.cs` `Update` (khoảng dòng 346) | Khi không có người chơi (scene AR, hoặc trước khi người chơi xuất hiện): mỗi frame 3 lệnh `GameObject.Find`/`FindGameObjectWithTag` và 1 lệnh `FindAnyObjectByType<ThirdPersonController>` |
| B | `Guardian/IUHMonsterEncounter.cs` `Update` → `FindPlayer()` | Chưa thấy người chơi thì mỗi quái `Find` mỗi frame |
| C | `DailyActivities/IUHMascotQuestItem.cs` `Update` | Mỗi bản `Find` mỗi frame cho tới khi thấy người chơi |
| D | `Combat/IUHEmergencySupplyCrate.cs` `Update` | Như C |
| E | `Combat/IUHLootCoinPickup.cs` `Update` → `FindPlayer()` | Như C; thêm `GetComponent<Rigidbody>()` mỗi frame khi xu đang bay về |
| F | `Guardian/IUHMonsterEncounter.cs` `Update` | `m_PlayerTransform.GetComponent<IUHConceptPreviewController>()` mỗi frame, mỗi quái |

**Để nguyên:**
- `Camera.main`: Unity 6 đã lưu sẵn camera chính.
- `IUHPlayerSittingController`: quét `FindObjectsByType` mỗi frame nhưng không gắn trong scene hay prefab nào.
- `IUHCampusCombatManager`: hệ đánh quái thời gian thực cũ.
- Các lệnh `Find` trong `Awake`/`Start` hoặc sự kiện chạy một lần.

## 2. Thiết kế
### `IUHPlayerLocator` (mới, `Scripts/Core/IUHPlayerLocator.cs`, namespace `IUHCampus.Core`)
```csharp
public static class IUHPlayerLocator
{
    public const float RetryInterval = 0.5f;
    public static Func<float> Clock = () => Time.unscaledTime;   // test có thể thay
    static Transform s_Player;
    static float s_NextSearch = float.NegativeInfinity;

    // Trả người chơi đã lưu; nếu chưa có thì tìm, nhưng tối đa 1 lần mỗi RetryInterval.
    public static Transform Player { get; }   // "Campus Student" ?? tag "Player"
    public static void Reset();               // xóa cache + hẹn giờ (test, đổi scene)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void ResetOnLoad();
}
```
- Người chơi bị hủy (đổi scene) thì `s_Player == null` theo kiểu Unity, nên tự tìm lại khi hết hẹn giờ.
- Không thay đổi thứ tự tìm hiện có: `"Campus Student"` trước, rồi tag `"Player"`.
- Fallback riêng của từng script vẫn giữ nguyên (ví dụ `Camera.main` trong `IUHMascotQuestItem`/`IUHLootCoinPickup`), chỉ chạy khi locator trả `null`.

### Áp dụng
- **B, C, D, E:** thay khối `GameObject.Find("Campus Student") ?? FindGameObjectWithTag("Player")` **trong đường chạy mỗi frame** bằng `IUHPlayerLocator.Player`.
  - `IUHMonsterEncounter.FindPlayer()` vẫn lấy `ThirdPersonController`/`StarterAssetsInputs` bằng `GetComponent` khi tìm thấy (chỉ một lần).
  - Gọi trong `Start`/`Awake` có thể dùng locator hoặc để nguyên; không bắt buộc.
- **A:** `IUHMobileControlsUI` tìm thêm `"PlayerArmature"` và `ThirdPersonController`, nên không dùng locator. Thêm field `float m_NextPlayerSearch` và chỉ chạy khối tìm khi `Time.unscaledTime >= m_NextPlayerSearch`, sau đó đặt `+0.5 s`. Logic tìm giữ nguyên.
- **E:** lưu `Rigidbody` trong `Awake`/`Start` (field `m_Body`).
- **F:** lưu `IUHConceptPreviewController` cùng lúc gán `m_PlayerTransform`, không gọi `GetComponent` mỗi frame. Khi `m_PlayerTransform` đổi thì lấy lại.
- **Không** thay 30 chỗ `Find("Campus Student")` khác nằm ngoài đường mỗi frame. Ngoài phạm vi.

### `Invalid AABB`
Lượt Story (`Docs/Evidence/CampusGuardian/STORY/README.md` mục AABB) đã dò mà không tái hiện được. Lượt này:
1. Gắn bộ dò tạm bằng `execute_code`, không tạo file và không commit (giống lượt Story):
   - `Application.logMessageReceived`: khi gặp log chứa `AABB`, quét `Transform` có position/scale không hữu hạn và `Renderer.bounds` không hữu hạn.
   - Ghi đường dẫn object và stack trace.
2. Chơi thật khoảng 10 phút: mở đầu cốt truyện, 2 trận match-3, vườn, mở cửa hàng/kho đồ, biến cố ban đêm, trận trùm.
3. Kết quả:
   - **Bắt được:** sửa đúng nguyên nhân, thêm test nếu được, commit riêng `fix(render): ...`.
   - **Không bắt được:** ghi "không tái hiện" kèm điều kiện đã thử, **không sửa phỏng đoán**.

## 3. Các Task (mỗi Task một commit, test trước)
- **Task 1** `perf(core): add throttled player locator`
  - Viết test PlayMode trước, tại `Scripts/Core/Tests/PlayMode/` (asmdef mới, tham chiếu Assembly-CSharp qua reflection như các test khác, hoặc để chung asmdef test có sẵn nếu hợp lý). Dùng `Clock` giả:
    - Không có người chơi ⇒ `null`.
    - Tạo `Campus Student` khi chưa hết 0,5 s ⇒ vẫn `null` (không tìm lại).
    - Qua 0,5 s ⇒ trả đúng object.
    - Hủy object ⇒ `null`, qua hẹn giờ thì tìm lại.
    - `Reset()` xóa cache.
  - Sau đó viết `IUHPlayerLocator`.
- **Task 2** `perf(gameplay): stop per-frame player lookups`
  - Áp A–F.
  - Test PlayMode cho A: không có người chơi, chạy 10 frame trong 0,5 s ⇒ khối tìm chạy tối đa 1 lần. Kiểm qua đếm, ví dụ field hoặc biến `internal` đếm số lần tìm. Nếu khó thì kiểm `m_NextPlayerSearch` đã được đặt.
  - Test PlayMode tầm E hiện có (`EncounterRangeTests`) phải vẫn Pass.
- **Task 3** `docs(perf): record per-frame lookup cleanup and AABB investigation`
  - Evidence `Docs/Evidence/CampusGuardian/PERF/README.md`: bảng A–F trước/sau, số test, kết quả AABB.
  - Profiler (CPU, Deep Profile tắt): 1 lần đo khoảng 30 s chơi campus trước và sau. Ghi ms/frame trung bình của `Update.ScriptRunBehaviourUpdate` nếu đọc được; không đo được thì ghi rõ lý do. Không bịa số.
  - Thêm 1 lần đo trong scene AR **trên Editor**: thời gian `IUHMobileControlsUI.Update` trước/sau, nếu Profiler hiện được.
  - Cập nhật bảng trạng thái lượt 5 trong `Docs/superpowers/plans/2026-11-18-lo-trinh-chinh-sua.md`.

## 4. Ranh giới
- Chỉ sửa 5 file của A–F (B và F cùng `IUHMonsterEncounter`), thêm `IUHPlayerLocator` và test. Không đổi hành vi, số liệu, scene, prefab.
- `IUHMobileControlsUI` và `IUHMonsterEncounter` là code chung; Thịnh đã cho phép sửa.
- Không sửa AR (`Guardian/AR/**`) trừ khi AABB chỉ đúng vào đó (khi đó dừng và hỏi).
