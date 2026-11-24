# NPC/linh vật ẩn khi thảm họa, sửa và trồng bồn hoa — G1–G5

Nhánh `minhnhat/vfx-pass`, nối tiếp lượt sửa lỗi chơi thử (`179c2bc`). Ngày 24/11/2026, Unity 6000.5.8f1 Windows Editor, URP/Direct3D12, RTX 3060 Laptop. Nguồn AC, pin 100 %, power scheme Turbo. Thực hiện: Claude Code (Codex đã hết gói). Prompt: [Prompt_Peace_NPC_Flowerbeds.md](../../../Prompts/Prompt_Peace_NPC_Flowerbeds.md). Ảnh người dùng báo: [PLAYTEST-FIX/user-report/](../PLAYTEST-FIX/user-report/) (01, 05, 06, 07).

| Task | Commit | Kết quả |
|---|---|---|
| G1 NPC + linh vật ẩn khi thảm họa | `e295918` | Đạt |
| G2 bồn hai bên đài phun nước | `bae5936` | Đạt |
| G3 trồng thêm ở luống cỏ | `db516b1` | Đạt |
| G4 bồn khu Ngày hội dính sân khấu | `f0c8c91` | Đạt |
| G5 bằng chứng | commit chứa README này | — |

## G1 — Ẩn sinh viên và linh vật khi có thảm họa

**Định nghĩa** (theo `IUHStoryDirector`):
- **Thảm họa:** từ lúc sự cố bắt đầu (`Incident`, trời chuyển đêm) qua các chương (`Chapter`) đến khi trời sáng lại ở đoạn kết (`Ending`, sau `ToDay(3)` + 3 s); và trong lúc mỗi **dư chấn** đang chạy (từ khi chọn được quái đến sau câu thoại dọn xong).
- **Hòa bình:** `Prologue` (check-in), sau khi trời sáng ở `Ending`, `Completed` khi không có dư chấn.
- Tải save giữa chừng: `StoryDisaster.IsDisasterAtStart(phase)` — `Incident`/`Chapter`/`Ending` ⇒ ẩn ngay khi vào scene.

**Cách làm:** `IUHStoryPeaceCrowd` (Director tự `AddComponent` như `IUHStoryNightShift`, không sửa scene) tìm `NPC_STUDENTS` và mọi `IUHMascotNPC` (gốc `IUH_MASCOT_ROBOT`: hình, đèn, bảng `[E] Nhận Thử Thách`), rồi `SetActive(!disaster)`. Khi thảm họa bắt đầu mà hội thoại linh vật đang mở thì `CloseMascotDialog()`. Không sửa `IUHMascotNPC`, `IUHAmbientStudent`; nhiệm vụ hằng ngày không đổi trạng thái (vi mạch nhiệm vụ là object riêng). Có sự kiện tĩnh `DisasterChanged(bool)` cho phần khác dùng sau.

`IUHStoryDirector.cs` +7 dòng: 1 field, `AddComponent` + `SetDisaster(IsDisasterAtStart)` ở `Start`, `SetDisaster(true)` khi `Incident` và khi dư chấn bắt đầu, `SetDisaster(false)` khi trời sáng ở `Ending` và khi dư chấn xong.

**Kiểm:** EditMode `StoryDisasterTests` 5 pha; PlayMode `StoryPeaceCrowdPlayModeTests` 4 test (ẩn/hiện, gọi lặp, thiếu object không lỗi, sự kiện chỉ phát khi đổi). Trong scene thật, save đang ở `Chapter` 1 ⇒ `isDisaster=True`, `NPC_STUDENTS` và `IUH_MASCOT_ROBOT` đều tắt; gọi `SetDisaster(false)` + `ToDay` ⇒ hiện lại.

Ảnh cùng góc: [thảm họa](G1-disaster-plaza.png) · [hòa bình](G1-peace-plaza.png).

## G2 — Bồn hai bên đài phun nước

Hai bồn cong là `Curved fountain gardens_101` (một mesh cho cả hai bên): viền đá, nền cỏ `Lawn`, hàng rào cắt tỉa `Hedge` (khối xanh đen phía ngoài), viền bi đồng. Dải cỏ bên trong trống, và không có collider nên nhân vật đi vào/đứng lên được (ảnh 01).

- Giữ nguyên hàng rào, viền đá, bi đồng. Trồng **100 cụm** hoa/bụi/cỏ (`PF_Flower_3_Group`, `PF_Flower_4_Group`, `PF_Bush_Flowers`, `PF_Plant_1`, `PF_Grass_Short` — prefab có sẵn từ lượt A) lên dải cỏ, cách mép 0,25 m, không đặt dưới hàng rào (raycast mesh `Hedge`), chừa 1,6 m quanh quái Nhà E ở mép bồn trái. Seed `24112026`.
- Gộp theo vật liệu thành **4 renderer** dưới `NATURE_FOUNTAIN_BEDS`, Batching Static, không đổ bóng; bỏ kênh tangent (shader `IUH/Stylized Foliage` không dùng) và nén mesh Medium.
- **Tường va chạm vô hình** `BedWall_Collider` (MeshCollider, cao 1,2 m) dựng từ biên của mesh `Lawn` — `stepOffset` nhân vật 0,35 m nên không leo được. Thử trong Play: đi từ lối lát vào bồn bị chặn sau 0,25 m; đi từ phía đài phun nước dừng ở mép trong (x ≈ ±8,5). Grounder F1 vì vậy cũng không còn nâng nhân vật lên bồn.

Ảnh: [trên cao trước](G2-fountain-top-before.png) · [trên cao sau](G2-fountain-top-after.png) · [ngang tầm mắt sau](G2-fountain-bed-left-after-day.png) (trước: ảnh người dùng 01, 06).

## G3 — Trồng thêm ở luống cỏ

`Garden beds and furniture_11/Lawn` gồm 6 luống cỏ chữ nhật 16,4 × 12,4 m; lượt A chỉ trồng dọc mép một số luống, phần giữa trống. Trồng **220 cụm** theo nhóm 4 cây bán kính 0,9 m, seed `24112027`:
- Không đè lên cây cỏ/ghế/đèn sẵn có (lưới chiếm chỗ ô 0,7 m dựng từ tam giác của `NATURE_PLANTERS`, `NATURE_GROUND`, `Layered planting…`, ghế gỗ, đèn đồng).
- Chừa 2,2 m quanh quái, check-in, rương, NPC, cây, đèn, ghế và các luống `GARDEN/Flower bed *` (vườn tương tác).
- Gộp thành **4 renderer** dưới `NATURE_FLOWERBEDS_FILL`, không collider (đi xuyên như cây cỏ lượt A).

Ảnh cùng góc luống đông: [trước](G3-lawn-east-before-day.png) · [sau](G3-lawn-east-after-day.png) (Edit Mode nên quái thể thao vẫn hiện).

## G4 — Bồn khu Ngày hội dính sân khấu

Ảnh 07: một hàng bụi/hoa của `NATURE_PLANTERS/Planters Near` và cụm cỏ `NATURE_GROUND/LobbyGround_*` chạy thẳng lên mặt sân khấu (`Stage platform` x 22,5–45,5, z −14…−2). Lọc bỏ mọi tam giác có tâm nằm trong vùng sân khấu nới 0,5 m (x 22–46, z −14,5…−1,5):

| Mesh | Tam giác trước → sau |
|---|---:|
| Near_Bush | 34.200 → 30.686 |
| Near_Flowers | 32.064 → 28.508 |
| Near_Leaves | 11.700 → 9.900 |
| Near_LeafAtlas | 9.232 → 8.273 |
| Near_Grass | 2.596 → 2.102 |
| LobbyGround_Grass | 15.576 → 14.927 |
| LobbyGround_LeafAtlas | 9.096 → 8.717 |

Tổng bỏ 11.351 tam giác. Cả 7 mesh chỉ có 1 object dùng trong scene. Ba mesh `CanteenFiltered_*` (hoa trắng, cúc vạn thọ, hàng rào) có đỉnh thừa trong vùng nhưng 0 tam giác ⇒ không sửa. Không đổi sân khấu, check-in `CHECKIN_SPOT_FESTIVAL`, logic Ngày hội.

Ảnh trên cao: [trước](G4-festival-top-before.png) · [sau](G4-festival-top-after.png).

## A/B hiệu năng (G2 + G3)

A = `NATURE_FOUNTAIN_BEDS` và `NATURE_FLOWERBEDS_FILL` tắt; B = bật. Cùng phiên A, B, A, B, A, B; 120 + 3000 khung hình; 1280 × 900 xác minh; VSync tắt; góc `lobby`/`overview`/`fountain` của `ART-ENVIRONMENT/cameras.json`. G4 chỉ bớt hình học nên áp dụng ở cả A và B. Trung vị của 3 lượt:

| Góc | Median A → B (Δ ms) | p95 Δ ms | Draw A → B |
|---|---:|---:|---:|
| lobby | 4,610 → 4,910 (+0,30) | +0,16 | 264 → 268 |
| overview | 5,302 → 5,153 (−0,15) | −0,62 | 875 → 883 |
| fountain | 4,882 → 4,884 (+0,002) | +0,08 | 515 → 523 |

Đạt ngưỡng (Δ median ≤ 0,8 ms, Δ p95 ≤ 1,5 ms, draw toàn cảnh G2+G3 ≤ 40). Góc lobby dao động giữa các lượt (A: 4,27–5,02 ms) nên +0,30 ms chủ yếu là nhiễu. **Lưu ý:** A/B đo trên bản dày hơn (120 + 320 cụm, chưa bỏ tangent); bản bàn giao 100 + 220 cụm nhẹ hơn, nên số trên là cận trên. [TSV](G23-AB.tsv) · [bối cảnh](G23-AB-context.txt).

## Kiểm thử cuối

- EditMode **148/148**, PlayMode **76/76** Pass.
- Missing Script = 0 ở `IUH_SharedCampus_Concept`, `IUH_CombatArena`, `IUH_Campus_AR_Encounter`. Console 0 error. Edit Mode, scene `isDirty=false`.
- Scene commit chỉ gồm block của `NATURE_FOUNTAIN_BEDS` (21) và `NATURE_FLOWERBEDS_FILL` (18) cùng 2 mục `SceneRoots`, ghép lên bản đã commit bằng script; kiểm 0 fileID trùng, 0 tham chiếu cục bộ treo. Thay đổi Unity tự ghi trong working copy (14 vật liệu nhúng, 8 prefab instance, dòng `MainCamera`) không commit.

## Chỗ lệch / còn lại

1. G3 không trồng cây nhỏ (prompt gợi ý 1–3 cây/bồn lớn): prefab cây có sẵn cao ~10 m, thu nhỏ trông lạ và tốn thêm draw call; chỉ dùng hoa + bụi.
2. Bồn G2 có tường vô hình bao quanh cả dải cỏ: muốn lại gần hoa phải đứng ngoài mép.
3. Mesh gộp lưu dạng text: 8 file, tổng ~14 MB (`Meshes/Nature/Flowerbeds/`).
4. Ảnh G3 chụp trong Edit Mode nên còn quái/cột sáng che một phần; ảnh G1 là ảnh chụp bằng camera đặt vị trí, không có HUD.
