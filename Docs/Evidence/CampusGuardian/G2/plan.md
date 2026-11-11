# Kế hoạch G2

Spec: yêu cầu G2 của người dùng ngày 11/11/2026. Nền: c2eb497 (fix-mobile-compile chưa merge).
Thiết kế đã chỉ định: wrapper JsonUtility version=1, Items key tồn tại giữ cả danh sách rỗng; thiếu key cấp đồ khởi đầu và lưu ngay; JSON hỏng cảnh báo và giữ đồ khởi đầu. Giữ API và batch một Save.

- [x] Viết và chạy InventoryPersistenceTests để thấy đỏ trước khi sửa code.
- [x] Sửa riêng IUHGuardianInventory, chạy test xanh.
- [x] Chạy toàn bộ EditMode/PlayMode; kiểm tra thủ công MCP với ảnh trước/sau.
- [x] Khôi phục PlayerPrefs/EditorSettings, kiểm tra scene sạch và Missing Script.
- [x] Ghi README, review diff, commit chọn lọc, không push/PR.

Ruling: thực hiện trực tiếp theo thiết kế và quy trình người dùng cung cấp; không thêm vòng phê duyệt thiết kế hoặc file ngoài phạm vi.

