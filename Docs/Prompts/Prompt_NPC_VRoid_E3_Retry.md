# Prompt cho Codex — Lượt E: áp lại E3 (đặt NPC VRoid) với ngưỡng theo mili-giây

Trả lời bằng tiếng Việt. Nhánh `minhnhat/npc-vroid`, đỉnh `9a699ac`. Sai ⇒ dừng.
Unity mở, MCP kết nối, thoát Play Mode, Console 0 error, scene `isDirty=false`. Dòng `MainCamera` `m_LocalPosition.y` **không stage**.
Mọi quy tắc khác giữ như `Docs/Prompts/Prompt_NPC_VRoid.md` (ranh giới, ảnh tự kiểm, Git, quy tắc tự quyết, MCP mất phản hồi ⇒ ghi `RESUME.md` và dừng, không ghi tay file scene).

## Người dùng duyệt (24/11)
Bản thử 8 NPC đạt về hình ảnh; người dùng chốt dùng 4–6 NPC cho nhẹ. E3 bị loại vì ngưỡng % FPS quá khắt khe ở mức khoảng 240 FPS:
- Sảnh: 4,18 → 4,66 ms, tức **+0,48 ms**.
- Toàn cảnh: 4,77 → 5,20 ms, tức **+0,43 ms**.
- Cả hai dưới 3% ngân sách khung hình 60 FPS (16,7 ms).

**Ngưỡng mới cho E3** (B so với A, đo A/B xen kẽ cùng phiên như cũ):
- Thời gian khung hình trung vị tăng **≤ 1,0 ms** ở cả sảnh và toàn cảnh.
- p95 tăng **≤ 1,5 ms**.
- Draw calls tăng ≤ 40 (sảnh), ≤ 60 (toàn cảnh).
- Báo cả FPS, % để tham khảo, nhưng không dùng % làm ngưỡng.

## Việc cần làm
1. Đọc `Docs/Evidence/CampusGuardian/NPC-VROID/README.md`, `placement-final-trial.json`, `placement.json`, `static-students.json`.
2. **Người dùng chốt 4–6 NPC cho nhẹ.** Dựng `NPC_STUDENTS` với **6 NPC**, lấy từ vị trí của bản thử cuối (`placement-final-trial.json`):
   - 4 đứng (2 nhóm × 2 người: gần Căn tin và sân trước tòa chính), 2 đi bộ trên lối lát.
   - Đủ cả 4 model, mỗi model 1–2 bản; 2 bản giống nhau không đứng cạnh nhau.
   - Tắt (`SetActive(false)`) `Students in courtyard_102`, không xóa.
   - Giữ tối ưu vòng 2 nếu không làm xấu hình: tắt bóng NPC ở xa. Outline MToon: giữ nếu chi phí chênh không đáng kể, ghi số.
   - Vượt ngưỡng ⇒ giảm còn **4 NPC** (3 đứng, 1 đi), đo lại. **Không thêm quá 6.**
   - Mobile vẫn giữ tối đa 4 như E2.
3. Đo A/B: 3 lượt mỗi cấu hình, 120 khung hình làm nóng + 3000 khung hình đo, `lobby` và `overview`, 1280 × 900, VSync tắt. Ghi trạng thái nguồn điện. Lưu `performance-E3-retry.tsv` tóm tắt, có thêm cột `median_ms`.
4. Đạt:
   - Lưu scene. Chụp ảnh sau ngày và đêm (các góc như E3), thêm 1 ảnh cận nhóm NPC và người chơi.
   - Play 30–60 s: đi xuyên nhóm NPC, chắn NPC đang đi, mở vườn, chuyển đêm. Phím E giả lập không nhận thì ghi rõ.
   - EditMode và PlayMode tách riêng, toàn bộ Pass; Missing Script = 0 ở 3 scene.
   - Commit `feat(art): populate campus with VRoid students` (scene: chỉ hunk `NPC_STUDENTS` và dòng tắt khối cũ).
5. Không đạt ⇒ không lưu scene, ghi bảng số vào README, commit bằng chứng, báo số.
6. Cập nhật `NPC-VROID/README.md`:
   - E3 đã áp với ngưỡng mới (ghi người dùng duyệt 24/11 và lý do đổi sang ms).
   - Bảng A/B, số NPC cuối, vị trí.
   - Commit `docs(npc): record NPC rollout`, kèm prompt này.

Không push, không amend. Một bước quá 25 phút không tiến triển ⇒ dừng, báo kèm log.

## Báo cáo
- Commit, số NPC cuối, bảng A/B (ms và FPS), link ảnh toàn cảnh ngày/đêm và ảnh cận NPC, số test, mọi chỗ lệch.
