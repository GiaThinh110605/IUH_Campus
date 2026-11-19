# Prompt cho Codex — Lượt A đồ họa: cây cỏ stylized + ánh sáng

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus`.

## Chuẩn bị (người dùng đã làm)
- Đã tạo nhánh `minhnhat/stylized-nature` từ `main` (đã có lượt 5).
- Đã mở Unity và bấm Start Server (MCP `unityMCP`).
- Thịnh đã đồng ý sửa scene campus.
- Kiểm `git branch --show-current`. Sai nhánh ⇒ dừng. MCP mất kết nối ⇒ dừng.
- Scene `isDirty=false` trước khi bắt đầu.

## Đọc trước
1. `AGENTS.md`, `.agents/rules/*`.
2. Skill `.agents/skills/free-asset-sourcing` và `.agents/skills/urp-graphics-upgrade`.
3. Thiết kế: `Docs/superpowers/specs/2026-11-20-stylized-nature-design.md`.
4. Kế hoạch: `Docs/superpowers/plans/2026-11-20-stylized-nature.md`. Làm Task 1 → 5, **dừng sau Task 5** gửi ảnh. Task 6–8 làm sau khi người dùng duyệt (Task 7–8 có thể làm luôn nếu người dùng bảo).

## Pack nguồn
`D:\IUH\CongNgheMoi\CK\_downloads\quaternius\StylizedNatureMegaKit\` (đã giải nén, CC0 1.0, có `License_Standard.txt`). Chỉ chép file cần vào `Assets/IUH_Campus/ThirdParty/...` như kế hoạch. Không chép cả pack.

## Git
- Stage chọn lọc kèm `.meta`. Không `git add -A`/`.`.
- Không stage `*.csproj`, `*.slnx`, `Logs/`, `ProjectSettings/` (trừ khi Task 7 cố ý đổi Quality và ghi rõ), `*.mat` Unity tự ghi ngoài `Materials/Nature`, `Assets/Screenshots/`, `.vsconfig`, `match3-combat-revamp.patch`.
- Scene: chỉ stage hunk của Task đang làm. Dòng `MainCamera` `m_LocalPosition.y` (2.2066925 → 3.2066925) **không stage**.
- Spec, kế hoạch, prompt này vào commit Task 1. Không push, không amend.

## Kiểm chứng mỗi Task có scene
- Console 0 error; không vật liệu hồng; Missing Script = 0.
- EditMode và PlayMode toàn bộ Pass.
- Play 30–60 s: đi qua cây mới không kẹt, E ở quái và vườn vẫn đúng.
- Ảnh trước/sau cùng góc (ngày, đêm).

## Dừng và báo khi
- Shader lỗi compile hoặc không chạy trên URP 17 sau 3 vòng sửa.
- Cây mới làm chắn đường, chắn quái/check-in, hoặc cần dời object gameplay.
- Lọc mesh gộp làm hỏng hình ở khoảnh khác.
- FPS Editor giảm hơn 15% so với trước.
- Lỗi do sửa của bạn quá 3 vòng.

## Báo cáo khi xong (sau Task 5)
- Danh sách commit; số test.
- Ảnh trước/sau: cây (3 góc), khoảnh thử, vườn 3 giai đoạn; ngày và đêm.
- FPS/draw calls trước/sau.
- Mọi chỗ lệch kế hoạch.
