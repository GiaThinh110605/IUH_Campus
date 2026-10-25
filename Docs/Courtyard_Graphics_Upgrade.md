# Đồ họa sân trung tâm — 23/10/2026

Scene chính được nâng trực tiếp qua Unity MCP. Giữ nguyên bố cục, số tầng, vị trí nhiệm vụ và collider di chuyển của bản ảnh thực tế.

- Mặt sân dùng texture asphalt hạt nhỏ; thêm dải đá thoát nước sát bãi xe, ván gỗ trên ghế và cây hoa trong bồn cây có sẵn.
- Hai cây bóng mát giữ nguyên thân/cành và collider thân; thay submesh tán đa giác bằng 9.000 lá mỗi cây, gộp thành một mesh mỗi cây, tạo bóng lá loang. Không thêm collider lá. Đây là tán cây tạo bằng hình học, chưa phải cây quét thực tế.
- Bổ sung mặt trời vì trước lượt chỉnh không có Directional Light đang hoạt động. Mặt trời, ambient và skybox tác động toàn scene; Volume chỉnh màu ACES/contrast/bloom giới hạn ở sân, có vùng chuyển tiếp. Không thay Render Pipeline hay cấu hình chất lượng Mobile.
- Các mesh/material/texture và Volume được lưu cố định ở `Assets/IUH_Campus/CourtyardGraphics`; nhóm scene `IUH_REFERENCE_CAMPUS/COURTYARD_GRAPHICS_R3`. Bản sao trước sửa giữ cục bộ trong `Docs/Backups`.

Menu dựng: **IUH Campus → Graphics → Upgrade Central Courtyard**. Có chặn dựng lại để tránh chồng chi tiết. Phần cây được dựng bổ sung bằng **Refine Courtyard Foliage**, bỏ qua cây đã có marker.

Kiểm chứng trong Editor: tuyến ngoài 41/41, hành lang 33/33, số tầng/hình khối 13/13; trọn lượt hai minigame sau khi thay tán lá 28/28 (Green 270 điểm, Photo 3 JPEG/300 điểm). Console không có lỗi cuối lượt. Báo cáo và ảnh tại `Docs/Evidence/CourtyardGraphics`. Bài kiểm tra hai minigame dùng thư mục save thử riêng; không thay save người chơi. Scene đã lưu và thoát Play Mode.

Đây là lượt nâng vật liệu, cây và ánh sáng trên mô hình hiện có. Chưa đạt toàn bộ mức chi tiết kiến trúc của ảnh phong cách tham khảo; chưa đo FPS trong bản build hoặc thiết bị Android. Tán lá mới cần được đo thêm trên thiết bị mục tiêu trước khi áp dụng toàn trường.
