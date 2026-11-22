# Prompt cho Codex — Lượt B2 tiếp: đo lại hiệu năng, chỉnh độ sáng MToon, hoàn tất Task 3–4

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus`, nhánh `minhnhat/vroid-player`, đỉnh `8a6c034`. Kiểm nhánh; sai ⇒ dừng. Unity mở, MCP `unityMCP` kết nối; thoát Play Mode.

Đọc lại `Docs/Prompts/Prompt_VRoid_Player.md`, `Docs/superpowers/plans/2026-11-21-vroid-player.md` và `Docs/Evidence/CampusGuardian/CHARACTER-VROID/README.md`. Mọi quy tắc Git và kiểm chứng ở đó vẫn áp dụng.

**Giữ nguyên** thay đổi scene `Campus Student` đang có (bản thử VRoid, chưa commit). Không reset.

## Claude đã kiểm (23/11)
Lần đo 232,34 → 206,38 FPS (−11,17%) **chưa đủ tin cậy để kết luận**:
- Mỗi mốc chỉ 1 lượt, 600 khung hình trong khoảng 2,6 giây ⇒ nhiễu lớn. Mốc sảnh của lượt C dao động 217–229 FPS giữa các phiên (khoảng 5%).
- Draw calls (220) và tam giác (1.243.569 / 1.243.567) gần như bằng nhau ⇒ **nhân vật không nằm trong khung hình đo**. Phần chênh nếu có là chi phí CPU (SpringBone, skinning), không phải vẽ.
- Có 3 lỗi render pass khi đổi RenderTexture camera, có thể do chính script đo gây ra.

Ngoài ra còn 2 vấn đề:
- **Ban ngày nhân vật bị cháy sáng** (`Idle-front-day-AFTER.png`): blazer, quần và da gần như trắng, mất nếp gấp và đổ bóng. Ảnh ban đêm thì ổn.
- `ProjectSettings/GraphicsSettings.asset` trong `b35530e` thêm shader MToon10 vào Always Included **2 lần** (2 dòng giống hệt nhau, guid `0781c64b7f2a1604ea6aa7e252080372`).

## Bước A — Đo lại đúng cách (chưa sửa gì khác)
1. Camera đo: dùng camera gameplay thật (Cinemachine theo người chơi) hoặc đặt camera sao cho **nhân vật nằm trong khung hình** ở góc sảnh. Ghi rõ vị trí. Không đổi `targetTexture` của camera chính khi đo.
2. Mỗi lượt làm nóng 120 khung hình, ghi **3000 khung hình** (khoảng 12–15 giây), VSync tắt, 1280 × 900.
3. Đo xen kẽ trong cùng phiên Unity: Casual2 → VRoid → Casual2 → VRoid → Casual2 → VRoid (bật/tắt 2 bản model trong Play Mode, không lưu scene). Thêm 3 lượt **VRoid với SpringBone tắt** để tách chi phí SpringBone.
4. Báo cáo trung vị FPS, khung hình p95 (ms), draw calls, tam giác cho từng cấu hình.
5. Ghi rõ 3 lỗi render pass có còn xuất hiện khi chơi bình thường (không chạy script đo) không.

Kết luận:
- Trung vị VRoid giảm ≤ 10% so với Casual2 ⇒ đạt, sang Bước B.
- Giảm > 10% mà phần lớn do SpringBone ⇒ thử giảm chi phí: bỏ spring ở chi tiết nhỏ (giữ tóc và vạt áo), hoặc đổi cách cập nhật của `Vrm10Instance` nếu UniVRM 0.131.4 có tùy chọn. Đo lại.
- Vẫn > 10% ⇒ dừng, gửi bảng số. Không tự xóa Casual2.

## Bước B — Chỉnh độ sáng MToon (ban ngày)
- Chỉ sửa **bản vật liệu đã extract** trong thư mục của lượt này, không sửa file `.vrm`. Nếu UniVRM chưa extract vật liệu thì extract ra `Assets/IUH_Campus/Materials/PlayerVRoid/` và gán vào prefab.
- Mục tiêu: ban ngày blazer trắng vẫn thấy nếp gấp và bóng đổ, da không trắng bệch, độ sáng tổng thể gần với cây cỏ và nhà xung quanh. Ban đêm không phát sáng.
- Hướng chỉnh: kiểm `GI Equalization`/`GI intensity`, `Shade Color` (tối hơn một chút để có bóng), `Shading Toony`/`Shift`, `Lit Color` (≤ 0,9 nếu cần). Không bật emission. Không sửa `Campus volume.asset`.
- Ảnh đứng trước ngày và đêm, so sánh với ảnh `*-AFTER` hiện có.

## Bước C — Hoàn tất Task 3 và Task 4 theo kế hoạch
- Kiểm gameplay đúng mục Task 3 của kế hoạch: đi, chạy, nhảy, tiếp đất bằng controller thật; `OnFootstep`/`OnLand` không lỗi; tóc và áo đung đưa nhẹ, không văng khi nhảy; camera không chui vào đầu; arena match-3 (Flee được); cửa hàng, kho đồ, vườn; chuyển đêm.
- Sửa dòng trùng trong `GraphicsSettings.asset` (chỉ giữ 1 dòng MToon10), commit cùng Task 3.
- Commit Task 3 `feat(player): replace Casual2 with VRoid student` (scene: chỉ hunk `Campus Student`; không stage dòng `MainCamera` `m_LocalPosition.y`).
- Task 4: dọn Casual2 sau khi tìm GUID không còn tham chiếu, cập nhật README `CHARACTER-VROID` (thay phần "trạng thái dừng" bằng kết quả cuối, kèm bảng đo mới), đưa prompt này vào commit.

## Chống kẹt
- Một bước quá 20 phút không tiến triển ⇒ dừng, báo kèm log.
- EditMode và PlayMode chạy tách riêng.

## Báo cáo
- Bảng đo Bước A (trung vị, p95, cấu hình). Kết luận về chi phí SpringBone.
- Các thông số MToon đã đổi; ảnh ngày/đêm trước và sau khi chỉnh.
- Danh sách commit, số test, Console, Missing Script.
- Mọi chỗ lệch kế hoạch.
