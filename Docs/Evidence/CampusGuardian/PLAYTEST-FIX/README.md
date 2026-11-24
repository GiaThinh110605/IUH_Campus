# Sửa lỗi chơi thử 24/11 — F1–F6

Nhánh `minhnhat/vfx-pass`, nối tiếp lượt VFX (`5433873`). Ngày 24/11/2026, Unity 6000.5.8f1 Windows Editor, URP/Direct3D12, RTX 3060 Laptop. Nguồn AC, pin 100 %, power scheme Turbo. Chưa build thiết bị.

Prompt: [Prompt_Fix_Feet_Smoke_Monster_Combat.md](../../../Prompts/Prompt_Fix_Feet_Smoke_Monster_Combat.md). Ảnh người dùng báo lỗi: [user-report/](user-report/) (01–04 cho lượt này).

**Quá trình:** Codex làm F1–F4 và phần lớn F5 (ảnh, A/B), rồi hết token giữa F5. Claude Code làm tiếp: chạy test và commit F5, commit bổ sung meta thư mục, thu gọn hook combat, viết F6.

| Task | Commit | Kết quả |
|---|---|---|
| F1 chân, chạy giật, nhảy tàng hình | `3a72768` | Đạt |
| F2 bỏ khối tím, thay khói | `ad16ccc` | Đạt (khói còn mờ, xem lệch) |
| F3 quái hết T-pose | `c6bd770`, `f5dffed` | Đạt |
| F4 hiệu ứng combat world-space | `ee41529`, `6fbc19b` | Đạt |
| F5 hào quang chân quái rõ hơn | `a14af6b` | Đạt (vị trí lệch, xem lệch) |
| Bổ sung | `c213d81` | `Prefabs/Guardian.meta` bị quên ở F3; vật liệu khói của gói được URP chuẩn hóa thành trong suốt |
| F6 bằng chứng | commit chứa README này | — |

## F1 — Nguyên nhân gốc và cách sửa

Tái hiện trong Play Mode trên nhân vật thật, cùng đường chạy 5 s, 3 lần nhảy, đi men bồn cây; mỗi bước đo khi `IUHVisualFeetGrounder` **bật** và **tắt** ([tóm tắt](F1-reproduction-summary.json), [chi tiết sau sửa](F1-after/summary.txt)).

| Trước sửa | Grounder bật | Grounder tắt |
|---|---:|---:|
| Chạy: dao động Y hông − gốc giữa 2 khung hình | 0,074 m | 0,018 m |
| Chạy: offset grounder đổi giữa 2 khung hình | 0,102 m | 0 |
| Nhảy: dao động Y hông − gốc | **1,733 m** | 0,171 m |
| Nhảy: offset nhỏ nhất | **−2,00 m** (chạm trần `maximumOffset`) | 0 |
| Men bồn cây: dao động | 0,362 m | 0,018 m |

- **Nhảy tàng hình:** ở đỉnh cú nhảy, hysteresis của `ThirdPersonController` có lúc báo `Grounded = true`. Grounder tin cờ này, tính mặt đất từ mặt nhìn thấy bên dưới, rồi kẹp ở −2 m ⇒ phần hình bị chôn dưới sàn. Renderer vẫn báo `isVisible` (bounds còn trong frustum) nên cờ hiển thị không bắt được lỗi.
- **Chạy giật:** grounder tính lại offset mỗi khung hình theo bàn chân thấp nhất. Khi chạy, chân nhấc luân phiên nên offset nhảy tới 10 cm.
- **Đứng trên bồn cây:** dữ liệu `CampusVisibleGround` gồm cả mặt bồn cây; khi collider đi sát bồn, grounder nâng phần hình lên mặt bồn.
- **Sửa (`3a72768`, chỉ `IUHVisualFeetGrounder.cs` + test), nhánh riêng cho người chơi:**
  - Tự raycast mặt đỡ dưới đáy capsule thay vì tin cờ `Grounded`. Có input nhảy hoặc gốc đang đi lên ⇒ coi là trên không trong 0,3 s, giữ offset cũ.
  - Hiệu chỉnh **một lần** khi đứng yên lần đầu (độ cao đế giày so với gốc, khe giữa mặt nhìn thấy và collider); sau đó offset chỉ được trôi **±0,15 m** quanh giá trị hiệu chỉnh và đổi tối đa **1 m/s**, không còn bám bàn chân đang nhấc.
  - Mặt nhìn thấy chỉ được dùng khi nằm trong 0,4 m trên mặt va chạm thật.
  - NPC giữ cách cũ (không có `ThirdPersonController`). Không đổi `CharacterController`, `ThirdPersonController`, camera, Starter Assets.

| Sau sửa | Grounder bật | Grounder tắt |
|---|---:|---:|
| Chạy: dao động hông − gốc | 0,023 m (0,017 m khi khoảng khung hình < 25 ms) | 0,016 m |
| Chạy: offset đổi giữa 2 khung hình | **0** | 0 |
| Nhảy: dao động hông − gốc | 0,171 m (bằng animation gốc) | 0,173 m |
| Nhảy: offset đổi giữa 2 khung hình | ≤ 0,05 m | 0 |
| Men bồn cây: dao động | 0,018 m | 0,017 m |

Idle trên sân lát, cỏ, đài phun nước: khe hở giày khoảng 1,2 mm. Chuỗi ảnh nhảy sau sửa: [F1-after/](F1-after/) `ON-stage1-0…3.png`.

## F2 — Khối tím thay bằng khói

`IUHStoryCorruptionZone` không còn tạo khối cầu. Dùng nguyên prefab `msVFX_Stylized Smoke 1` (bản sao `Resources/Guardian/VFX/CorruptionSmoke.prefab`, giữ scaling mode, hình dạng, chuyển động, đường cong), nhiều cụm trong bán kính vùng và sát mỗi quái, màu tím xám, tổng hạt ≤ 80 PC / 30 mobile. Texture dùng alpha từ grayscale để tránh ô vuông đen. `Purify()` ngừng phát, mờ dần 2 s theo thời gian thực, rồi Destroy. API `Create`/`Purify` giữ nguyên.

Ảnh: [ngày trước](F2-day-before.png) · [ngày sau](F2-day-after.png) · [đêm trước](F2-night-before.png) · [đêm sau](F2-night-after.png) · [Purify ngày](F2-day-purify.png) · [Purify đêm](F2-night-purify.png).

## F3 — Quái hết T-pose

5 quái `CORRUPTED_ENCOUNTER_*` dùng `BlueDemon.obj` tĩnh (không xương, không clip). Bản GLTF có sẵn trong project có rig 43 xương và clip Idle. Tạo `Prefabs/Guardian/Monsters/PF_BlueDemon_GuardianIdle.prefab` + `AC_BlueDemon_Idle` (clip `BlueDemon_Idle`), Animator Cull Completely. Mesh OBJ cũ chỉ **tắt**, không xóa. `f5dffed` căn chân theo mesh đã skin (bounds skin rộng hơn hình thật làm quái cao hơn 25 cm). [Vị trí](F3-installed.json). Proxy arena chạy Idle, tay hạ: [F3-arena-qa.json](F3-arena-qa.json), [ảnh arena](F3-arena-after.png).

Ảnh: [quái 1 ngày trước](F3-monster1-day-before.png) / [sau](F3-monster1-day-after.png), [quái 1 đêm sau](F3-monster1-night-after.png), [quái 2 ngày sau](F3-monster2-day-after.png), [quái 2 đêm sau](F3-monster2-night-after.png).

## F4 — Hiệu ứng combat world-space

`IUHCombatWorldVfx`: pool 12 instance (3 mỗi loại), prefab Cartoon FX trong `Resources/Guardian/Combat/` (HitMonster, HitPlayer, Skill, Defeat), đặt tại thân proxy theo bounds skin (bỏ qua beacon/pillar/beam). Không camera phụ, không RenderTexture, không Canvas. Helper CFX (rung camera, đèn, tự hủy) tắt; mobile giảm nửa hạt. Ăn ô trên bàn match-3 (UI) không thêm hạt.

Ảnh: [trúng quái](F4-hit-monster.png) · [trúng người chơi](F4-hit-player.png) · [quái bị hạ](F4-defeat-monster.png).

## F5 — Hào quang chân quái

`IUHCampusMonsterGroundFire`: tổng 24 hạt PC / 12 mobile mỗi quái (chia đều 4 hệ), kích thước 0,35 → 0,50 m, alpha 0,35 → 0,70, vẫn thấp ≤ 1 m. Ảnh: [quái 1 ngày trước](F5-monster1-day-before.png) / [sau](F5-monster1-day-after.png), [đêm sau](F5-monster1-night-after.png), [quái 2 ngày sau](F5-monster2-day-after.png).

## A/B hiệu năng

Cùng phiên A, B, A, B, A, B; 120 khung hình làm nóng + 3000 đo; 1280 × 900 xác minh bằng `Screen.width/height`; VSync tắt. Số trong bảng là trung vị của 3 lượt.

| Task / góc | Median A → B (Δ ms) | p95 Δ ms | Draw A → B | Ngưỡng |
|---|---:|---:|---:|---|
| F1 lobby | 4,942 → 4,900 (−0,042) | −0,106 | 338 → 338 | ≤ 0,3 ms — Đạt |
| F1 overview | 5,447 → 5,461 (+0,015) | +0,040 | 940 → 940 | Đạt |
| F2 quảng trường | 5,394 → 5,497 (+0,103) | +0,062 | 638 → 645 | Đạt |
| F2 overview | 5,623 → 5,659 (+0,035) | +0,054 | 938 → 947 | Đạt |
| F3 cận quái | 5,345 → 5,396 (+0,052) | +0,045 | 487 → 487 | Đạt |
| F3 overview | 6,015 → 6,134 (+0,118) | +0,134 | 1098 → 1098 | Đạt |
| F4 arena (đang phát hiệu ứng) | 4,914 → 4,909 (−0,005) | +0,005 | 281 → 282 | Đạt |
| F5 cận chân quái | 6,096 → 6,060 (−0,037) | −0,028 | 892 → 892 | Đạt |
| F5 overview | 6,225 → 6,248 (+0,023) | +0,079 | 1098 → 1098 | Đạt |

TSV: [F1](F1-AB.tsv) · [F2](F2-AB.tsv) · [F3](F3-AB.tsv) · [F4](F4-AB.tsv) · [F5](F5-AB.tsv); bối cảnh từng lượt trong `F*-AB-context.txt`. So với lượt VFX trước (V3 dùng RenderTexture: +1,70 ms), F4 world-space gần như không tốn thêm.

## Phần của Thịnh (chỉ thêm hình ảnh)

Không đổi luật, sát thương, lượt, thời gian, collider, trigger, khoảng E, beam, logic encounter.

- `Scripts/Guardian/Match3/Match3BattleStage.cs`: +12 dòng — field `m_WorldVfx`, hàm `PlayWorldEffect(int kind)` (9 dòng), 1 dòng `Clear()` trước khi dựng proxy vào trận và 1 dòng trong `Exit()`.
- `Scripts/Guardian/Match3/Match3CombatManager.cs`: +4 dòng, mỗi dòng 1 lời gọi `m_Stage.PlayWorldEffect(...)`: sau khi tính damage người chơi (MonsterHit hoặc Skill khi combo/mega bomb), sau `ResolveBossTurn` khi `turn.Damage > 0` (PlayerHit), sau `CastSkill` (Skill), trước `PlayMonsterDefeat` (Defeat).
- Scene campus: dưới mỗi `CORRUPTED_ENCOUNTER_*` thêm visual `PF_BlueDemon_GuardianIdle`, tắt mesh OBJ cũ (F3); hào quang chân quái từ lượt VFX.

## Kiểm thử cuối (sau mọi commit code)

- EditMode **143/143** Pass. PlayMode **72/72** Pass (gồm test F1 grounding, F2 khói, F3 rig/chân, F4 pool 50 lần, F5 ngân sách hạt).
- Missing Script = 0 ở `IUH_SharedCampus_Concept`, `IUH_CombatArena`, `IUH_Campus_AR_Encounter`. Console 0 error. Unity Edit Mode, scene `isDirty=false`.

## Chỗ lệch / còn lại

1. **Khói F2 còn mờ**: khối tím đã mất, nhưng các cụm khói ban ngày nhìn khá nhạt. Có thể tăng alpha/số cụm ở lượt sau nếu người dùng muốn rõ hơn.
2. **Hào quang F5 lệch khoảng 0,97 m** so với tâm hai bàn chân của model GLTF mới ([F5-position-audit.json](F5-position-audit.json)): hào quang đặt ở tâm encounter, còn chân model Idle lùi về sau. Trong ảnh vẫn thấy tím quanh chân nên chưa sửa; sửa cần dời 5 object con trong scene.
3. F1 chạy: dao động hông − gốc 0,023 m, nhỉnh hơn mục tiêu 0,02 m một chút, nhưng phần do grounder gây ra là 0 (offset không đổi giữa các khung hình); phần còn lại là nhịp hông của animation chạy (bản tắt grounder 0,016 m).
4. Bồn cây **không có collider** mà mặt cao < 0,4 m vẫn có thể nâng phần hình tối đa 0,15 m khi nhân vật đi xuyên vào bồn (trước sửa là tới 0,36 m và giật). Lượt `Prompt_Peace_NPC_Flowerbeds.md` G2 thêm collider cho bồn hai bên đài phun nước.
5. File scene trong working copy còn khác HEAD (14 vật liệu nhúng bị Unity dọn, 8 prefab instance thêm trường mới, dòng `MainCamera`): do Unity tự ghi, **không thuộc lượt này, không commit**.
6. Thư mục chẩn đoán `F1-before/`, `F2-initial-opacity/`, `F3-initial-bounds/`, `F4-expired-fixture/` và `F1-after/frames.json` không commit (khoảng 45 MB, chỉ dùng trong lúc gỡ lỗi).
