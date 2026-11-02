# Quy chuẩn Logic Combat & Match-3 (Match-3 Combat Rules)

Áp dụng cho hệ thống chiến đấu Turn-based Match-3 trong mini-game IUH Campus Guardian.

## 1. Tách biệt Trạng thái Bàn cờ (Board Logic vs Visuals)
- **Model độc lập**:
  - Ma trận dữ liệu bàn cờ (Grid 2D array / data model) phải được tính toán và kiểm tra hoàn toàn độc lập với các `GameObject` hay `Transform` trong Unity.
  - Việc kiểm tra ghép 3, 4, 5 (Horizontal / Vertical Match) và tính toán chuỗi combo (Cascade) phải hoàn tất trên ma trận dữ liệu trước khi kích hoạt hoạt ảnh hiển thị.
- **Khóa Điều khiển người chơi (Input Lock)**:
  - Khi người chơi thực hiện một nước đi (Swap), hệ thống phải kích hoạt cờ khóa điều khiển (`IsBusy = true` hoặc `State != PlayerInput`).
  - Cấm nhận thêm thao tác vuốt / chạm của người chơi trong lúc các ô ngọc đang nổ, rơi, tạo combo hoặc trong lượt quái đánh phản công. Chỉ mở lại khóa khi bàn cờ hoàn toàn ổn định.

## 2. Chu trình Lượt chơi Rõ ràng (Turn-based Determinism)
- **Chu trình lượt (Turn Cycle)**:
  1. `PlayerTurn_Input`: Chờ người chơi vuốt / tráo 2 ô liền kề.
  2. `Resolving_Matches`: Nổ ô ngọc, tính điểm/sát thương, dồn ngọc rơi xuống (Cascade), sinh ngọc mới từ đỉnh bảng. Lặp lại nếu có combo mới.
  3. `PlayerTurn_Apply`: Gây sát thương lên quái / buff khiên / hồi máu cho nhân vật người chơi.
  4. `Check_Victory`: Nếu HP quái <= 0 -> Chiến thắng, chuyển trạng thái kết thúc trận.
  5. `MonsterTurn`: Quái thực hiện kỹ năng tấn công/debuff theo chu kỳ.
  6. `Check_Defeat`: Nếu HP người chơi <= 0 -> Thất bại.
  7. `Return_To_Player`: Đặt lại cờ và chờ lượt mới của người chơi.

## 3. Không Hardcode Chỉ số & Quy tắc Cân bằng
- **Chỉ số cấu hình qua Data Asset**:
  - Máu (HP), Sát thương (ATK), Phòng thủ (DEF) của Người chơi và Quái vật phải nằm trong `ScriptableObject` hoặc file cấu hình, không hardcode giá trị số vào mã nguồn.
- **Xác thực Nước đi Hợp lệ (Valid Move Check)**:
  - Nước đi chỉ được chấp nhận nếu sau khi tráo tạo ra ít nhất một cụm từ 3 ô cùng loại trở lên. Nếu không tạo ra combo, ngọc phải tự động tráo ngược lại về vị trí cũ kèm phản hồi thị giác nhẹ nhàng.

