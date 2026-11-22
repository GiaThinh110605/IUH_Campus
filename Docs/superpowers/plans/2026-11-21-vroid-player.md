# Kế hoạch — Lượt B2: thay nhân vật người chơi bằng model VRoid

Ngày 21/11/2026. Làm **sau lượt C** (môi trường), trên nhánh `minhnhat/vroid-player` tạo từ `main` mới nhất.
Người dùng chốt (21/11): model Casual2 (Quaternius low-poly) quá xấu ⇒ thay bằng **AvatarSample_Z** (VRoid, nam, đồng phục blazer) do người dùng tự xuất. NPC dùng AvatarSample_Y và model khác ở lượt sau (E).

## File nguồn
- `D:\VRoid\model\IUH Student Player.vrm` (ngoài repo), VRM 1.0, 11,9 MB, 37.436 tam giác, 2 vật liệu, 12 texture.
- Meta license trong file (Claude đã đọc 21/11):
  - `avatarPermission: everyone`, `commercialUsage: corporation`.
  - `allowRedistribution: true`, `modification: allowModification`.
  - `creditNotation: required`.
  - `allowExcessivelyViolentUsage: false` (game đánh quái kiểu hoạt hình, không máu me ⇒ không phải bạo lực quá mức).
  - Tác giả `pixiv VRoid Project`, tham chiếu AvatarSample_Z.
- **Không dùng** `main.vrm` (license chặt).

## Task 1 — Cài UniVRM `chore(deps): add UniVRM for VRM 1.0 import`
- Thêm UniVRM bản mới nhất ổn định (kiểm trang releases `vrm-c/UniVRM`) bằng UPM git URL trong `Packages/manifest.json`: `com.vrmc.gltf` và `com.vrmc.vrm`, cùng một tag.
  - Không dùng `com.vrmc.vrmshaders` (đã bỏ từ v0.125).
- Thêm shader `VRM10/Universal Render Pipeline/MToon10` vào **Always Included Shaders** (Graphics settings) để build không mất shader.
- Compile sạch, Console 0 error; EditMode/PlayMode toàn bộ Pass như trước.
- **Lỗi compile trên Unity 6000.5.8:**
  - Thử 1 bản UniVRM cũ hơn gần đó.
  - Vẫn lỗi ⇒ **dừng và báo**, kèm log. Phương án dự phòng là đổi VRM sang FBX qua Blender (cần người dùng cài Blender + VRM add-on).
  - Không sửa code trong package.
- Commit `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/GraphicsSettings.asset` (chỉ dòng Always Included Shaders), kế hoạch, prompt.

## Task 2 — Nhập model `chore(art): import VRoid student player model`
- Chép file vào `Assets/IUH_Campus/ThirdParty/VRoid/Player/IUH_Student_Player.vrm`, đổi tên bỏ dấu cách.
  - Kèm `SOURCE.md`: AvatarSample_Z, pixiv VRoid Project, xuất bằng VRoid Studio 2.16.0 ngày 21/11/2026, toàn bộ meta license ở trên, link `https://vrm.dev/licenses/1.0/`.
  - Thêm mục vào `Docs/Credits.md` (credit required).
- Import qua UniVRM (VRM 1.0) ⇒ prefab, Avatar Humanoid hợp lệ, vật liệu MToon10 URP, SpringBone tóc/áo.
- Prefab làm việc `Assets/IUH_Campus/Prefabs/Player/PF_Student_VRoid.prefab`.
- Kiểm hình trong Scene view, ngày và đêm (MToon có tự nhận ánh sáng; nếu quá sáng hoặc phát sáng ban đêm thì chỉnh thông số MToon trên bản vật liệu đã extract, không sửa file `.vrm`).
- Test EditMode: Avatar `isValid && isHuman`; không vật liệu null hoặc hồng.

## Task 3 — Thay người chơi `feat(player): replace Casual2 with VRoid student`
- Chụp **trước**: đứng, đi, chạy, nhảy (3/4 trước, sau lưng), ngày và đêm; 1 ảnh arena match-3; dùng lại `cameras.json` của `CHARACTER` nếu được.
- Trong `Campus Student`: làm **đúng cách đã dùng cho Casual2** (PR #45, xem `Docs/Evidence/CampusGuardian/CHARACTER/README.md`).
  - Gỡ khung xương và mesh Casual2.
  - Đặt khung xương và mesh VRoid **ngay dưới gốc** để `Animator` ở gốc dùng Avatar mới; giữ Animator Controller Starter Assets.
  - `Vrm10Instance` / SpringBone đặt đúng chỗ UniVRM yêu cầu. Nếu cần nằm trên gốc thì ghi rõ.
  - Không đổi `ThirdPersonController`, `CharacterController` (h 1,8 m), camera, HUD.
  - Scale khoảng 1,70–1,75 m, chân chạm đất. `PlayerCameraRoot` đúng tầm vai/đầu.
- Kiểm:
  - Không T-pose. Đi, chạy, nhảy, tiếp đất đúng.
  - Event `OnFootstep`/`OnLand` không lỗi.
  - Tóc/áo đung đưa nhẹ, không xuyên người quá mức, không văng khi nhảy.
  - Camera không chui vào đầu.
- Arena match-3: proxy nhân bản hiện model VRoid đúng; `StripToVisuals` không làm hỏng (SpringBone bị gỡ thì tóc đứng yên, chấp nhận được).
- Scene: chỉ stage hunk `Campus Student`.

## Task 4 — Dọn Casual2 + bằng chứng `chore(art): remove unused Casual2 assets` và `docs(player): record VRoid player swap`
- Sau khi không còn tham chiếu (tìm GUID trong scene, prefab, vật liệu):
  - Xóa `ThirdParty/Quaternius/UltimateModularMen/`, `Materials/Player/`, `Prefabs/Player/PF_Student_Casual2.prefab`.
  - Xóa mục Casual2 trong `Docs/Credits.md` nếu không còn dùng.
- `Docs/Evidence/CampusGuardian/CHARACTER-VROID/README.md`:
  - Ảnh trước/sau, bảng kiểm animation.
  - FPS/draw calls/tris góc sảnh trước/sau.
  - Phiên bản UniVRM.
  - Số test.

## Ranh giới
- Không sửa Starter Assets, file `.vrm` gốc, code trong package UniVRM.
- Không sửa quái, vườn, cây cỏ, mặt đất, UI (trừ khi HUD avatar cần đổi ảnh; nếu có thì ghi rõ).
- Không thêm NPC ở lượt này.

## Ngưỡng và dừng
- FPS Editor góc sảnh giảm > 10% so với trước lượt ⇒ dừng, báo số.
- UniVRM lỗi compile ⇒ dừng (xem Task 1).
- Animation méo nặng hoặc SpringBone gây giật ⇒ dừng, gửi ảnh hoặc video.
