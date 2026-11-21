# Prompt cho Codex — Lượt C, chỉnh cỏ trước khi áp toàn campus (Task 2b)

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus`, nhánh `minhnhat/environment-pass` (đỉnh `ef934a4`). Kiểm `git branch --show-current`; sai ⇒ dừng. Unity mở, MCP `unityMCP` kết nối; thoát Play Mode trước khi sửa.

## Vì sao phải chỉnh (Claude đã kiểm ảnh 22/11)
- **Sân lát: đạt.** Mạch gạch và màu lệch từng viên đẹp hơn trước. Không sửa.
- **Cỏ: xấu hơn trước.** So `close-before-day.png` với `close-atmosphere-after-day.png`: bãi cỏ bên phải trước có texture sợi cỏ (`SharedCampusConcept/Materials/Lawn.mat` có `_BaseMap` guid `3de24ebe3f73ce645a6c11d89919233d`). Sau khi đổi sang `M_Ground_Grass`, cỏ thành mảng xanh lá phẳng, chói hơn.
  - Nguyên nhân: `_ColorA/B/C` chỉ lệch nhau khoảng 10% ⇒ noise gần như không thấy. Không có lớp chi tiết nhỏ.
- **Cỏ lác đác `NATURE_GROUND`: gần như không thấy** trong `lobby-atmosphere-after-day.png` (vài cọng nhỏ cạnh ghế).

## Việc cần làm
1. **Thêm lớp chi tiết vào nhánh `_GROUND_GRASS`** của `Assets/IUH_Campus/Shaders/IUH_StylizedGround.shader`:
   - Thêm `_DetailTex` (texture) và `_DetailScale` (mét mỗi lần lặp, mặc định khoảng 1,5), `_DetailStrength` (0–1).
   - Lấy mẫu `_DetailTex` theo `positionWS.xz / _DetailScale` (vẫn chiếu thế giới, không dùng UV). Nhân độ sáng vào màu nền, không thay màu.
   - Texture: dùng đúng texture cỏ mà `Lawn.mat` cũ đang dùng (guid ở trên), hoặc `Grass.png` của Quaternius nếu đã có trong repo. **Không tải gì mới.**
   - Không sửa nhánh `_GROUND_PAVING`. Kiểm lại không có từ khóa dành riêng của HLSL.
2. **Tách màu cỏ rõ hơn** trong `M_Ground_Grass.mat`: A tối và hơi lạnh, B trung tính, C sáng và hơi vàng. Ví dụ: A (0,20; 0,32; 0,12), B (0,30; 0,44; 0,15), C (0,42; 0,52; 0,18). Mảng lớn (`_TileSize.x`) khoảng 10–12 m để thấy loang. Độ bão hòa tổng thể **không cao hơn** cỏ trước lượt.
3. **Cỏ lác đác dày và to hơn** ở mép bãi cỏ khu sảnh:
   - Khoảng 30–40 cụm thay vì 12, scale 1,3–1,8, xoay và scale ngẫu nhiên có seed cố định.
   - Vẫn gộp mesh theo vật liệu dưới `NATURE_GROUND` (≤ 2–3 mesh). Không đặt trên đường đi, luống vườn hay chỗ quái đứng.
4. Test EditMode: shader có các property mới; `M_Ground_Grass` gán `_DetailTex` khác null; shader không có lỗi ở cả 2 keyword.

## Kiểm chứng
- Console 0 error. EditMode và PlayMode **chạy tách riêng**, toàn bộ Pass. Missing Script = 0 ở 3 scene.
- Chụp ảnh theo `cameras.json`, tên `*-grass-tune-after-day.png` và `*-grass-tune-after-night.png` cho 5 góc. Gửi kèm cặp so sánh `close-before-day` / `close-grass-tune-after-day`.
- Đo FPS/draw calls góc sảnh, cùng cách đo `lobby-performance-task3.tsv`. Ngưỡng: so với trước lượt (217,25 FPS / 209 draw calls), FPS giảm ≤ 10%, draw calls tăng ≤ 30.
- Nếu cỏ vẫn không đẹp hơn cỏ trước lượt sau 2 vòng chỉnh ⇒ dừng, gửi ảnh. Phương án dự phòng: giữ `Lawn.mat` cũ cho cỏ, chỉ dùng shader mới cho sân lát.

## Git
- Commit `feat(art): add grass detail layer and denser ground tufts`.
- Stage chọn lọc kèm `.meta`: shader, `M_Ground_Grass.mat`, test, scene (**chỉ** hunk `NATURE_GROUND`; không stage dòng `MainCamera` `m_LocalPosition.y`), mesh gộp mới, ảnh và tsv.
- Không stage `*.csproj`, `*.slnx`, `ProjectSettings/`, các `*.mat` khác Unity tự ghi, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`, các file VRoid, `Docs/Prompts/Prompt_Environment_Pass_Resume.md`. Prompt này được stage cùng commit.
- Không push, không amend. **Không làm Task 4–5.**

## Báo cáo
- Commit, số test, bảng FPS/draw calls, ảnh trước/sau cỏ, số cụm cỏ và số mesh gộp, mọi chỗ lệch.
