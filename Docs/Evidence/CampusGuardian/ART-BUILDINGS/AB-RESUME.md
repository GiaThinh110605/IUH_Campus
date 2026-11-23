# Tiếp tục sau A/B — Buildings D4 Retry2

**ĐÃ ĐÓNG:** người dùng đã khởi động lạiUnity+MCP, 54renderer được nạp và kiểm chứng. Commitfeature `7e2dd86`; 16ảnhAFTER mới, smoke40s, EditMode133/133 và PlayMode50/50, Console0, MissingScript0cả3scene. Xem [README hiện tại](README.md). Các dòng bên dưới giữ lịch sử bàn giao, không phải trạng thái còn chặn.

- Branch `minhnhat/buildings-pass`, HEAD `52b4aae`, chưa commit trong Retry2.
- A/B **đã chạy đủ12lượt, đạt ngưỡng tương đối**, số ở performance-AB.tsv và README. Không thay bằng số lịch sử; không đo lại vô cớ.
- 7material trong Assets/IUH_Campus/Materials/Buildings/ tồn tại, meta mới đã có. Instancingfalse, noise.025, WindowCell3.8×4.6, Warm(.55,.38,.19); màu/texture/ST khớp source; mode độc quyền. Reviewer độc lập đã kiểm.
- Scene trên đĩa đã thay54GUID đúng inventory/material-map; diff55+/55− gồmMainCameraY có sẵn. Không reset, không stageMainCamera. Backup trước ghi `Temp/AB-scene-before-save.unity`. Script ghi `Temp/apply-facades-scene.cjs` chỉ đổi54slot, không serialize lại scene.
- Last successful Editor action: thoátPlay. Load scene qua MCP timeout. editor/state tiếp đó ping not answered. Không biết native scene đã load xong hay có dialog; Computer Use native pipe không có. Một lời gọi chờ đọc sốrenderer đã hủy, có thể còn trong queue nhưng chỉ đọc.
- Cần khôi phục Unity/MCP rồi kiểm isDirty vàConsole; xác nhận54facade renderer; không save đè bản cũ đang nằm trong bộ nhớ nếu Unity chưa nạp file.
- Khi đã xác nhận: chụp16ảnhAFTER cùng8góc ngày/đêm, không dùng lại ảnhRetry1; smoke30–60s đi/chạy quanhnhà, garden, day/night. Ghi đúng nếuEgiả lập không nhận. RestorePlay-only changes/PlayerPrefs nếu có.
- TestEditMode/PlayMode riêng, compilefalse trước chạy, MissingScript0cả3scene, Console0. Có runner trực tiếp trong `direct-test-runner.cs.txt` của lượt trước nếu công cụ MCPtest lại timeout; chỉ dùng helper tạm và xóa khỏiAssets saukiểm chứng.
- Commitfeature đúng prompt: Assets Buildings+meta và scene54renderer, loạiMainCamera hunk. Sauđó commitdocs gồm README/ảnhAB/bằngchứng/test, power-AB.json đãsanitize, promptRetry2. Không push/amend.
- Giới hạn cần giữ trung thực: A/Btrianglesbằngnhau, nhưng toàncảnh2.582.136còn lệch5.466sovớilịchsử. AnimatedFountainFX36PSđãđượcxácđịnh làyếutốphátsinh nhưngchưagiảithíchhếtchênhlệch. Khôngghi “khớp exact OPT1”.
