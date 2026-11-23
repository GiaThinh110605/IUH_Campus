# Prompt cho Codex — Lượt D: áp D4, đo A/B xen kẽ trong cùng phiên

Trả lời bằng tiếng Việt. Nhánh `minhnhat/buildings-pass`, đỉnh `52b4aae`. Sai ⇒ dừng. Unity mở, MCP kết nối, thoát Play Mode, Console 0 error, scene `isDirty=false`.
Quy tắc Git, ngưỡng và cách chụp ảnh giữ như `Docs/Prompts/Prompt_Buildings_D4_Retry.md`.

## Vì sao lần retry trước không hợp lệ (Claude kiểm 24/11)
- Không đo lại mốc "trước" trong cùng phiên. Mốc BEFORE là của đêm 23/11.
- Đêm 23/11, cùng vật liệu đã cho kết quả gần bằng mốc:
  - `performance-TRIAL1.tsv`, `performance-OPTIMIZATION1-HISTORY.tsv`: sảnh khoảng 240 FPS, toàn cảnh khoảng 207 FPS, draw calls 297/954.
  - Retry 24/11 ra 174/166 FPS, tụt đều khoảng 30% kể cả góc sảnh có draw calls như cũ ⇒ **máy chậm đi** (nguồn điện, nhiệt, tiến trình khác), không phải do vật liệu.
- Retry có draw calls toàn cảnh 996 và tam giác +5.500 so với bản thử đêm qua ⇒ **cấu hình không giống**.

## Việc cần làm
1. **Trước khi đo:**
   - Ghi trạng thái nguồn (`powercfg /getactivescheme`, đang cắm sạc hay không: `Get-CimInstance Win32_Battery`).
   - Đóng Play Mode khác và Profiler. Không chạy test hay tác vụ nặng song song khi đo.
2. Tạo lại 7 vật liệu `M_Facade_*` đúng cấu hình của **OPTIMIZATION1** (bản cho 297/954 draw calls). Gán theo `material-map.json`.
   - **Tắt GPU Instancing** nếu không giảm draw calls.
   - Kiểm tam giác toàn cảnh khớp mốc (khoảng 2.576.670). Lệch ⇒ tìm object thừa trước khi đo.
3. **Đo A/B xen kẽ trong cùng một lần Play Mode**:
   - Đổi `sharedMaterials` giữa bộ gốc (A) và bộ mặt tiền (B) bằng script lúc runtime, không lưu scene.
   - Thứ tự A, B, A, B, A, B. Mỗi lượt 120 khung hình làm nóng + 3000 khung hình đo, ở cả `lobby` và `overview`.
   - Lưu 1 tsv tóm tắt `performance-AB.tsv` (cột: view, config, run, median_fps, p95_ms, draw_calls, triangles).
4. **Đánh giá theo trung vị A và B của cùng phiên.** Ngưỡng:
   - FPS giảm ≤ 10%; p95 tăng ≤ 10%.
   - Draw calls sảnh tăng ≤ 30, toàn cảnh tăng ≤ 70.
5. **Đạt** ⇒ lưu scene, chụp ảnh `*-AFTER-day/night.png` (ghi đè bản retry), chạy EditMode và PlayMode tách riêng, kiểm Missing Script.
   - Commit `feat(art): stylized facades across campus`: chỉ hunk renderer tòa nhà; không stage dòng `MainCamera`.
   - Cập nhật README: bảng A/B, trạng thái nguồn điện, lý do retry trước không hợp lệ.
   - Commit `docs(art): record facade rollout` kèm prompt này.
6. **Không đạt** ⇒ không lưu scene, xóa vật liệu, ghi bảng A/B vào README, commit bằng chứng, báo số.

Không push, không amend. Một bước quá 20 phút không tiến triển ⇒ dừng, báo kèm log.
