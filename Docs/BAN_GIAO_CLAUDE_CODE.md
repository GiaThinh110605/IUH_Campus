# Bàn giao cho Claude Code — IUH Campus Guardian (phần Minh Nhật)

Cập nhật: 26/11/2026. Người dùng: Minh Nhật (GitHub `minhat2502`). Đồng đội: Gia Thịnh (repo chủ `GiaThinh110605/IUH_Campus`, public).
Trả lời bằng tiếng Việt, ngắn gọn. Luật chung: `AGENTS.md` + `.agents/rules/*` (luôn ưu tiên). Skills: `.agents/skills/*`.

## 1. Cách làm việc hiện tại
- Từ 24/11: **Claude Code tự làm hết** (Codex đã hết gói): lập kế hoạch, sửa code/scene qua Unity MCP, chạy test, commit.
- Nhánh riêng `minhnhat/<tên>` tạo từ `main` mới nhất. Không commit vào `main`, không force push.
- **Chỉ push khi người dùng gõ "push"/"OK push".** Không có `gh` CLI: sau khi push, đưa link `…/pull/new/<nhánh>` và mô tả PR tiếng Việt (kết thúc bằng `🤖 Generated with [Claude Code](https://claude.com/claude-code)`); người dùng tự tạo và merge PR.
- Commit message kết thúc bằng `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Người dùng chơi thử rồi gửi ảnh chụp và danh sách lỗi; mỗi đợt sửa ghi vào `Docs/Evidence/CampusGuardian/<lượt>/README.md` (mục H1, H2…, commit, ảnh trước/sau, kiểm thử, "Chỗ lệch").
- Phạm vi: Minh Nhật chỉ làm bản **PC**; AR/iOS là của Thịnh; Android là hướng phát triển. Thịnh cho phép Minh Nhật sửa code của Thịnh.

## 2. Trạng thái
- `main` = `45eb1e6` (sau PR #52 vfx-pass). Các lượt đồ họa A/B/C/B2/D/E, visual-polish (#51), VFX + sửa lỗi chơi thử (#52) đều đã merge.
- **Nhánh `minhnhat/campus-polish` — đã push 26/11, chờ người dùng tạo PR và merge** (8 commit `26663f8` → `2570a9d`). Bằng chứng: `Docs/Evidence/CampusGuardian/CAMPUS-POLISH/README.md`. Nội dung:
  - Vườn: bỏ 4 luống đất và vòng mua hạt/bón phân/thu hoạch. `IUHGardenManager` tự nhận 8 bồn hoa từ mesh cỏ; đứng cạnh bồn nhấn `[E] Tưới hoa` để nhận +10 vàng, chờ 90 s. Save `IUH_Garden_v3`.
  - `NATURE_LAWN_WALLS`: tường vô hình cao 3 m ở mép bệ đá của 6 luống và 2 bồn cong (vì `JumpHeight` 1,4 m). Luống tách khỏi căn tin và sân khấu; 2 cây dời vào luống; sân khấu có MeshCollider; `CHECKIN_SPOT_FESTIVAL` dời tới (16; 8).
  - `CAMPUS_PERIMETER_FENCE`: rào và collider bao 3 cạnh campus.
  - Khói vùng nhiễm đậm hơn; hào quang quái tự căn giữa hai chân (`IUHCampusMonsterGroundFire.AlignToFeet`).
  - 5 quái chương truyện `MONSTER_01..05` đổi từ mesh `.obj` tư thế T sang prefab rig có idle (`Prefabs/Guardian/Monsters/PF_*_GuardianIdle`).
- Kiểm thử cuối: EditMode 150/150, PlayMode 75/75, Missing Script = 0, Console 0 lỗi.
- **Chưa kiểm:** chưa đo A/B hiệu năng cho campus-polish; chưa vào trận combat với 5 quái chương truyện sau khi đổi sang hình rig.

## 3. Việc tiếp theo
1. Chờ người dùng chơi thử hoặc merge campus-polish; sửa tiếp lỗi người dùng báo (thường kèm ảnh).
2. Sau khi merge: **đóng Unity**, `git fetch origin main:main`, `git switch main`, rồi tạo nhánh mới.
3. Lộ trình còn lại (`Docs/superpowers/plans/2026-11-18-lo-trinh-chinh-sua.md`): lượt 6 backend (iPhone gọi backend qua IP LAN), lượt 7 tài liệu/báo cáo/video. Hạn chót khoảng đầu tháng 12/2026.

## 4. Commit scene an toàn (quan trọng)
File scene `Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity` trong working copy luôn có diff do Unity tự ghi, **không được commit**: 14 vật liệu nhúng bị xóa, override của prefab instance, `UniversalAdditionalLightData` của đèn drone, thứ tự SceneRoots, dòng `m_LocalPosition.y` của MainCamera, và đổi xuống dòng. Vì vậy chỉ stage phần của việc đang làm, bằng script trong `Tools/SceneStaging/` (chạy ở gốc repo, sau khi đã lưu scene):
- `python Tools/SceneStaging/scene_stage2.py <scene> "<TênGameObject>" ...`: lấy nguyên cây con của các GameObject (không phải stripped) tên đó, gồm thêm/sửa/xóa và prefab instance con, ghép lên bản index; tự cập nhật SceneRoots.
- `python Tools/SceneStaging/scene_stage_pi.py <scene> "<Tên instance>" ...`: stage PrefabInstance gốc (ví dụ `Shade tree_13`) có tên đó trong `m_Modifications`.
- `python Tools/SceneStaging/scene_refcheck.py <scene>`: kiểm bản index có 0 fileID trùng và 0 tham chiếu treo.
- Sau đó xem `git diff --cached <scene>`, chỉ chứa thay đổi của việc đang làm.

Không bao giờ stage: `*.csproj`, `*.slnx`, `ProjectSettings/`, `Logs/`, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`, các `.mat` Unity tự ghi (CombatArena, NPCVRoid, `Festival LED`…), các ảnh/tsv untracked cũ trong `Docs/Evidence/.../VFX`, `VISUAL-POLISH`, `PLAYTEST-FIX/F1-after`. Không `git add -A` hay `git add .`.

## 5. Mẹo Unity MCP
- Unity MCP: HTTP `http://127.0.0.1:8081/mcp`. Nếu báo "No Session" hoặc ECONNREFUSED, nhờ người dùng mở Unity, vào Window → MCP for Unity → Start Server.
- `execute_code` dùng `compiler: codedom` (C# 6): không dùng hàm cục bộ (dùng `System.Func`), viết đầy đủ `UnityEngine.Object`, gọi `FindObjectsByType(..., FindObjectsSortMode.None)`. `AssetDatabase.DeleteAsset` bị chặn trong `execute_code` (gọi qua script Editor trong dự án thì được).
- Kiểm trước và sau khi sửa: scene `isDirty=false`, Console 0 error, Missing Script = 0 (`GameObjectUtility.GetMonoBehavioursWithMissingScriptCount`), chạy `run_tests` EditMode và PlayMode (`init_timeout` 120000).
- Ảnh bằng chứng: `manage_camera` action `screenshot`, `camera: "Main Camera"`, `view_position`/`view_target`, `output_folder: Docs/Evidence/CampusGuardian/<lượt>`.
- Thử va chạm trong Play: dịch chuyển `Campus Student` (CharacterController), gọi `cc.Move(dir*0.1f)` nhiều lần rồi xem vị trí dừng; để thử nhảy, nâng y thêm 1,5 m trước khi đi.
- Đo hiệu năng: đo A/B xen kẽ trong cùng một phiên Play (120 khung khởi động + 3000 khung, 1280×900, tắt VSync); ngưỡng theo ms (trung vị tăng ≤ 0,8–1 ms), không dùng %. Không so với số đo từ phiên khác.
- Mesh gộp lưu thành `.asset`: bỏ tangents nếu shader không dùng và nén Medium để file nhỏ.
- Các builder cũ khóa bằng `IUH_LEGACY_TOOLS`: không chạy. Builder dùng được: `Editor/Playtest/IUHFlowerbedBuilder`, `IUHPerimeterFenceBuilder`, `IUHMonsterIdleAssets`.

## 6. Bẫy khác
- Chuyển nhánh khi Unity đang mở dễ sinh `.meta` thừa: đóng Unity trước khi checkout/pull.
- `core.autocrlf=true`: diff khổng lồ trên scene thường chỉ do CRLF.
- Không commit `Server/.env` (key Adafruit); không commit ảnh trong `Server/tests/fixtures/real/`.
- Backend (đã xong từ trước, xem Evidence B1–B7, D2): `docker compose -f Server/docker-compose.yml up -d --build`; test `Server\.venv\Scripts\python -m pytest Server/tests -q`.
