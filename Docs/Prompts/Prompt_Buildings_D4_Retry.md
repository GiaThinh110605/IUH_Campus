# Prompt cho Codex — Lượt D: áp lại D4 (mặt tiền toàn campus) với ngưỡng mới

Trả lời bằng tiếng Việt. Repo `d:\IUH\CongNgheMoi\CK\IUH_Campus`.
- Nhánh `minhnhat/buildings-pass`, đỉnh `d44c0c0`. Sai ⇒ dừng.
- Unity mở, MCP `unityMCP` kết nối; thoát Play Mode; Console 0 error; scene `isDirty=false`.

## Người dùng duyệt (24/11)
Ảnh bản thử D4 (`ART-BUILDINGS/overview-FINAL-*.png`, `main-front-FINAL-*.png`) **đạt**: cửa sổ đêm sáng ấm xen ô tối, ngày giữ nguyên chữ, cửa sổ và cờ.
D4 bị bỏ chỉ vì draw calls toàn cảnh +55 > 40. FPS đạt (toàn cảnh −1,3%, sảnh −6,9%).

**Ngưỡng mới cho D4** (so với mốc `performance-BEFORE.tsv`):
- FPS trung vị giảm ≤ 10% ở cả sảnh và toàn cảnh (không đổi).
- Draw calls: sảnh tăng ≤ 30, **toàn cảnh tăng ≤ 70**.
- p95 frame ms không tăng quá 10%.

## Việc cần làm
1. Đọc `Docs/Evidence/CampusGuardian/ART-BUILDINGS/README.md`, `material-map.json`, `performance-*.tsv`, `Docs/Prompts/Prompt_Buildings_Overnight.md` (mục 3–4 về cách đo và quy tắc Git vẫn áp dụng).
2. Tạo lại đúng 7 vật liệu `M_Facade_*` trong `Assets/IUH_Campus/Materials/Buildings/`, cùng thông số bản thử cuối (bản đạt FPS). Gán `sharedMaterials` đúng slot theo `material-map.json`. Không đổi mesh, transform, collider.
3. **Thử giảm draw calls 1 vòng** (không bắt buộc đạt, chỉ ghi kết quả):
   - Kiểm vật liệu có bật **GPU Instancing** và shader có tương thích SRP Batcher không (Frame Debugger: lý do tách batch).
   - Bật nếu giúp được, không đổi hình.
   - Ghi nguyên nhân +55 draw calls vào README (ví dụ shader thêm pass, mất static batching, nhiều keyword).
4. Đo đúng cách: 1280 × 900, VSync tắt, 3 lượt × 3000 khung hình, góc `lobby` và `overview`. Chỉ lưu tsv tóm tắt.
5. Ảnh sau ngày và đêm, 8 góc của `ART-BUILDINGS/cameras.json`, tên `*-AFTER-day.png` / `*-AFTER-night.png`. Kiểm không có màu hồng lỗi shader, chữ và logo đọc được.
6. Kiểm chứng:
   - Console 0 error. EditMode và PlayMode tách riêng, toàn bộ Pass. Missing Script = 0 ở 3 scene.
   - Play 30–60 s: đi quanh tòa nhà, mở vườn, chuyển đêm xem đèn cửa sổ. Nếu phím E giả lập lại không nhận thì ghi rõ, không ghi Pass.
7. Commit `feat(art): stylized facades across campus` (scene: chỉ hunk renderer tòa nhà; **không stage** dòng `MainCamera` `m_LocalPosition.y`).
8. Cập nhật `ART-BUILDINGS/README.md`: D4 đã áp với ngưỡng mới (ghi người dùng duyệt 24/11), bảng đo trước/sau, renderer đã đổi, nguyên nhân draw calls. Commit `docs(art): record facade rollout`, kèm prompt này.

## Dừng khi
- FPS giảm > 10% hoặc draw calls vượt ngưỡng mới ⇒ không lưu scene, xóa vật liệu mới, báo số.
- Một bước quá 20 phút không tiến triển ⇒ dừng, báo kèm log.
- Không push, không amend.
