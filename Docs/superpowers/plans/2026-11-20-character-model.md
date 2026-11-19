# Kế hoạch — Lượt B: thay nhân vật người chơi, bỏ đồ trang trí

Ngày 20/11/2026. Nhánh `minhnhat/character-model` từ `main` `319dbd8` (đã có lượt A cây cỏ).
Thiết kế: `Docs/superpowers/specs/2026-11-20-stylized-nature-design.md`, mục "Ghi chú cho lượt B".

## Người dùng đã chốt
- Chỉ **1 nhân vật nam**: Quaternius Ultimate Modular Men, `Casual2`, bản **Humanoid Rig**. Tô lại **áo trắng, quần xanh đậm** (đồng phục sinh viên). Không có màn chọn.
- **Bỏ toàn bộ đồ trang trí:** ba lô, mũ tốt nghiệp, hào quang Cyber, vệt sáng chân, tab "Trang phục" trong cửa hàng. Không hoàn vàng, bỏ qua PlayerPrefs cũ.
- Thịnh đã đồng ý sửa scene và code cửa hàng.

## Nguồn
- Zip (ngoài repo): `D:\IUH\CongNgheMoi\CK\_downloads\quaternius\Ultimate Modular Men- Feb 2022-20261119T173844Z-1-001.zip`. License trong zip: CC0 1.0.
- File cần: `Ultimate Modular Men- Feb 2022/Humanoid Rig/Individual Characters/FBX/Casual2.fbx` (không có animation).
- Animation: **giữ nguyên** Animator Controller và clip của Starter Assets (Humanoid, retarget tự động). Không nhập Universal Animation Library ở lượt này.

## Hiện trạng người chơi
- Object `Campus Student` trong `IUH_SharedCampus_Concept.unity`. Gốc có `Animator` (avatar Starter Assets), `CharacterController`, `ThirdPersonController`, `StarterAssetsInputs`, `PlayerInput`, `IUHPlayerCustomizer`, canvas HUD trên đầu…
- Phần nhìn thấy: mesh Starter Assets (`Geometry/Armature_Mesh`) + khối tự dựng `Student head`, `Student hair`, `Student backpack`, `Student ski…`, `Students i…`.
- `ThirdPersonController` (Starter Assets, **không sửa**) lấy `Animator` bằng `TryGetComponent` trên **gốc**, và nhận animation event `OnFootstep`/`OnLand` trên gốc. ⇒ `Animator` phải ở lại trên gốc `Campus Student`.
- `Match3BattleStage.CreateProxy` nhân bản người chơi, chỉ giữ phần nhìn: chạy với mọi model, không cần sửa.
- Code tìm theo tên: chỉ `IUHPlayerCustomizer` (`Student backpack`), sẽ bị xóa. `"PlayerArmature"` trong `IUHMobileControlsUI`/AR encounter là fallback tên, giữ nguyên.

## Task 1 — Nhập model `chore(art): import Quaternius Casual2 student model`
- Chép `Casual2.fbx` vào `Assets/IUH_Campus/ThirdParty/Quaternius/UltimateModularMen/Models/`, kèm `SOURCE.md` (link quaternius.com, CC0 1.0, ngày tải, file gốc) và `License.txt` của pack. Cập nhật `Docs/Credits.md`.
- Import:
  - Rig **Humanoid**, Avatar "Create From This Model". Kiểm Avatar hợp lệ (đủ xương bắt buộc, không đỏ).
  - Read/Write tắt, không camera/light, không animation.
  - Material Creation Mode None.
- Vật liệu `Assets/IUH_Campus/Materials/Player/` (URP Lit, smoothness ≤ 0,15, bật GPU instancing):
  - `M_Player_Shirt` trắng ngà `#F2F2EE`.
  - `M_Player_Pants` xanh đậm `#1F2A44`.
  - `M_Player_Skin`, `M_Player_Hair` (giữ tông gốc), giày sẫm.
  - Model dùng tô màu theo texture atlas hay theo nhiều slot vật liệu thì làm theo cách đó. Nếu atlas thì tạo bản sao texture đã tô lại; **không sửa file gốc**.
- Prefab `Assets/IUH_Campus/Prefabs/Player/PF_Student_Casual2.prefab` (chỉ phần nhìn + vật liệu).
- Test EditMode: Avatar `isValid && isHuman`; prefab không vật liệu null.
- Spec, kế hoạch, prompt vào commit này.

## Task 2 — Thay nhân vật `feat(player): replace demo student with Casual2 model`
- Chụp ảnh **trước**: người chơi đứng, đi, chạy, nhảy (góc 3/4 trước và sau lưng), ngày; 1 ảnh trong arena match-3. Lưu `cameras.json`.
- Trong `Campus Student`:
  - Xóa phần nhìn cũ: `Geometry`/`Armature_Mesh`, khung xương Starter Assets nếu không còn dùng, và mọi `Student head/hair/backpack/…`, `Students i…`.
  - Đặt model mới sao cho **khung xương và mesh của model là con trực tiếp của gốc `Campus Student`**: instantiate, unpack, chuyển các con của model lên gốc, xóa object rỗng. Như vậy đường dẫn xương khớp Avatar mà `Animator` vẫn ở gốc.
  - Gán `Animator.avatar` = Avatar của `Casual2`. Giữ nguyên Animator Controller Starter Assets, `applyRootMotion` như cũ.
  - Scale: chiều cao khoảng 1,75 m, chân chạm đất, khớp `CharacterController` (không đổi height/radius trừ khi lệch rõ; nếu đổi thì ghi lại).
  - `PlayerCameraRoot` (mục tiêu camera) nằm ở đầu/vai đúng như cũ; chỉnh độ cao nếu model khác chiều cao.
  - Không đổi các component gameplay trên gốc.
- Kiểm:
  - Không T-pose.
  - Đi, chạy, nhảy, rơi, tiếp đất chạy đúng animation.
  - Tiếng bước chân (event `OnFootstep`) và tiếp đất (`OnLand`) không lỗi Console.
  - Không xuyên đất, không lơ lửng, camera không chui vào đầu.
- Arena match-3: bản nhân bản hiện model mới, đứng đúng chỗ, không mất vật liệu.
- Ảnh **sau** cùng các góc.
- Scene: chỉ stage hunk của `Campus Student`.

## Task 3 — Bỏ đồ trang trí `refactor(shop): remove cosmetics and the outfits tab`
- Xóa `Scripts/Guardian/IUHPlayerCustomizer.cs` (+ `.meta`) và component đó trong scene (không để Missing Script).
- `IUHUpgradeShopUI.cs`:
  - Bỏ tab 2: `m_TabSkinsContent`, `m_BtnTabSkins`, `m_SkinsContainer`, `m_SkinsScrollRect`, `RefreshSkinsTab` và mọi đoạn dùng `IUHPlayerCustomizer`/`PlayerSkinType`/`PlayerTrailType`.
  - Chuyển tab không còn chỉ số 2; tab còn lại 0 (Nâng cấp), 1 (Kho đồ).
- Scene: xóa nút tab "Trang phục" và nội dung tab đó trong canvas cửa hàng. 2 nút tab còn lại giãn đều, không để khoảng trống.
- Xóa object `GRADUATION_MORTARBOARD_HAT`, `CYBER_AURA_LIGHT`, trail chân nếu còn trong scene.
- `Editor/IUHMatch3CombatBuilder.cs` (builder cũ đã khóa): bỏ `EnsurePlayerCustomizer` và lời gọi, chỉ để compile được.
- `grep` toàn repo không còn `IUHPlayerCustomizer`, `PlayerSkinType`, `PlayerTrailType` (trừ Docs).
- Test hiện có về cửa hàng/kho đồ/nâng cấp phải Pass. Thêm test PlayMode: mở cửa hàng, chuyển 2 tab, không lỗi.
- Ảnh cửa hàng sau khi bỏ tab (PC và khung điện thoại).

## Task 4 — Bằng chứng `docs(player): record character swap and cosmetics removal`
- `Docs/Evidence/CampusGuardian/CHARACTER/README.md`:
  - Ảnh trước/sau.
  - Bảng kiểm animation.
  - Ảnh arena và cửa hàng.
  - FPS và draw calls góc sảnh trước/sau.
  - Danh sách object scene bị xóa.
  - Số test.
- Cập nhật lộ trình: lượt 4 xong.

## Ranh giới
- Không sửa Starter Assets (code, controller, clip), FBX gốc, quái, check-in, vườn, cây cỏ.
- Không nhập model khác ngoài `Casual2.fbx`.

## Dừng và báo khi
- Avatar Humanoid không hợp lệ hoặc animation Starter Assets retarget bị méo nặng (tay xoắn, chân trượt nhiều).
- Không thể để `Animator` ở gốc mà vẫn khớp xương (ghi cách đã thử).
- Tô màu cần sửa file gốc.
- FPS giảm > 10% so với trước.
