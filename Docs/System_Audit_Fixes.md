# Rà soát và xử lý lỗi hệ thống — 22/10/2026

Phạm vi: scene khuôn viên sau chỉnh ảnh thực tế, di chuyển nhanh, camera/menu, Photo Hunter, Green Campus, công cụ setup và tài nguyên chữ/mesh.

## Lỗi đã xác định và sửa

1. **Điểm dịch chuyển dùng tọa độ cũ.** Năm trong sáu giá trị cũ bị kiểm tra va chạm chặn: cổng 1/sân giữa nằm dưới bãi xe; ký túc xá nằm trong F; điểm nóc F dùng độ cao 1,9 m. Các điểm đang hoạt động nay lấy từ `IUHReferenceLayout`, đồng bộ cả dữ liệu scene; nóc F dùng độ cao sàn thực tế khoảng 13,49 m.
2. **Dịch chuyển không xóa vận tốc và quyền điều khiển của game trước.** Fast travel nay đóng mini game đang giữ input, trả vật đang cầm, thoát ngắm/xe/ghế, khôi phục avatar, reset motion và thông báo Cinemachine về độ dịch chuyển.
3. **Festival chưa triển khai nhưng nút vẫn dịch chuyển.** Thẻ được đặt trạng thái đang phát triển, chặn nút và phím chọn gây dịch chuyển vào vị trí cũ. Không thêm gameplay Festival.
4. **Setup có thể làm lệch bản R2.** Setup Photo không còn kéo nhân vật, xe và props về vị trí mặc định; landmark sân trung tâm vẫn gắn vào E mới. Green được dựng mới cũng được chuyển sang sân mới và chừa bồn cây. Setup lặp hai lần giữ nguyên toàn bộ transform của khuôn viên và vị trí nhân vật/rác.
5. **Chụp lại cập nhật file nhưng album vẫn trỏ ảnh cũ.** Lần chụp lại thay đường dẫn/thời điểm ảnh, giữ điểm thưởng ban đầu. File cũ chỉ dọn sau khi save mới thành công. Khi rollback, không xóa file đã commit hoặc file nằm ngoài thư mục save; chỉ xử lý JPEG thuộc game.
6. **Chơi lại có thể xóa save trước khi chắc chắn ghi được lượt mới.** Reset Photo nay thay thế save theo giao dịch ghi file tạm. Nếu ghi thất bại, điểm, trạng thái và album trước đó vẫn nguyên vẹn.
7. **Dữ liệu danh sách null và danh sách mục tiêu rỗng.** Clone/record chịu được danh sách thiếu; sanitize với tập mục tiêu rỗng loại bỏ hoàn thành cũ thay vì giữ điểm không có mục tiêu.
8. **Camera bị hủy trong lúc đợi frame chụp.** Capture kiểm tra lại camera sau khi chờ, báo thất bại sạch thay vì phát sinh MissingReferenceException. Texture đọc lỗi được giải phóng cả ở Play Mode và Edit Mode.
9. **Các bộ phận trùng tên có thể ghi đè mesh asset của tòa khác.** Mesh key nay chứa hash đường dẫn nhóm; hai bộ phận cùng tên trong A/B có khóa khác nhau. Giữ các mesh hiện có trong scene.
10. **Vật liệu chữ tạm tồn dư sau reload.** Cache có xử lý giải phóng khi reload script/thoát ứng dụng; dọn các bản không còn được renderer đang hoạt động sử dụng.
11. **Xe cá viên nằm xuyên mặt ram bãi xe mới.** Đã xác nhận giao nhau bằng bounds hình học; dời xe sang cạnh lối đi, giữ nguyên điểm nhận hàng và chừa tuyến từ cổng vào sân.

## Kiểm chứng

Các bài mới dùng thư mục save kiểm thử riêng, giữ nguyên save người dùng. Báo cáo nằm trong `Docs/Evidence/SystemAudit`:

| Kiểm tra | Kết quả cuối |
|---|---:|
| Lưu ảnh, rollback và tài nguyên | 16/16 |
| Setup lặp và dựng mới | 7/7 |
| Fast travel trong Play Mode | 10/10 |
| Logic Photo Hunter / Green Campus | 35/35 và 32/32 |
| Bàn phím/chuột, di chuyển và chuyển game | 24/24 |
| Trọn lượt hai mini game | 28/28 |
| Tuyến cổng / hành lang / hình khối | 41/41, 33/33, 13/13 |
| Ram bãi xe | 6/6 |

Quét scene không phát hiện script bị mất, collider mesh rỗng đang bật hoặc collider cao quá khổ. Vật liệu chữ tạm đã giảm từ 22 xuống 1 bản đang dùng. Xe cá viên không còn giao hình học với ram. Console cuối lượt không có lỗi.

- `scene-scan.json`: không mất script, không bật collider của mesh rỗng, không có collider cao quá khổ; năm điểm fast travel đang hoạt động có khoảng đứng thông thoáng.
- `storage-resources.json`: transaction/rollback, ảnh chụp lại, reset khi ghi đĩa thất bại, dữ liệu null, camera mất và khóa mesh.
- `setup-idempotence.json`: setup lặp và dựng mới Green trên bản R2.
- `logic-regression.json`: bộ kiểm tra logic hiện có của Green/Photo.
- `fast-travel-playmode.json`: chuyển từ Green, chế độ ngắm Photo, xe Grab, lúc nhảy và lên nóc F.
- `movement-playmode.json`: sự kiện bàn phím/chuột chạy qua PlayerInput, CharacterController và Cinemachine thật.
- `both-games-playmode.json`: trọn lượt hai mini game.

Các cảnh báo API Unity đã lỗi thời ở phần mã cũ được ghi nhận riêng; chúng không phải lỗi compile. Phạm vi kiểm chứng là desktop Unity Editor, chưa kiểm tra Android hoặc toàn bộ tình huống sử dụng ngoài các ca đã ghi lại.

Chạy lại các ca mới qua **IUH Campus → Tests → System Audit**. Bản sao scene trước rà soát giữ cục bộ tại `Docs/Backups/IUH_Campus_Main_before_system_audit_20261022.unity`.
