# Quy chuẩn Lập trình & Chất lượng Code (Coding Standards)

Áp dụng cho toàn bộ mã nguồn trong dự án IUH Campus. Mọi agent và lập trình viên phải tuân thủ nghiêm ngặt các quy tắc sau:

## 1. Hạn chế Code Comment dư thừa
- **Không viết comment hiển nhiên**: Tránh các comment giải thích những điều code đã thể hiện rõ ràng (ví dụ: `// Khởi tạo biến`, `// Gọi hàm`, `// Set active = true`).
- **Code tự giải thích (Self-documenting)**: Đặt tên biến, hàm, class và interface rõ ràng, mang tính ngữ nghĩa cao thay vì viết comment để giải thích ý định.
- **Chỉ comment khi thực sự cần thiết**: Chỉ sử dụng comment hoặc XML doc (`/// <summary>`) ở các API công khai hoặc khi giải thích lý do kỹ thuật đặc biệt (workaround, toán học phức tạp, hạn chế từ Unity Engine).

## 2. Refactor Code khi phức tạp
- **Nguyên tắc Đơn nhiệm (Single Responsibility)**: Một hàm chỉ nên làm một việc duy nhất. Nếu một hàm vượt quá 40-50 dòng hoặc có quá nhiều cấp lồng nhau (nested if/for), bắt buộc phải phân tách thành các hàm con chuyên biệt.
- **Tách biệt Logic và Trình bày (Separation of Concerns)**:
  - Logic tính toán (Data / State / Match-3 / Score / AI) phải tách rời hoàn toàn khỏi hiển thị đồ họa và hoạt ảnh UI (`MonoBehaviour` / `CanvasRenderer`).
- **Tránh Anti-patterns**: Không dùng magic numbers (hãy khai báo `const` hoặc cấu hình `SerializeField`), không hardcode chuỗi lặp lại, không tạo God Object chứa toàn bộ logic game.

## 3. Không In Log lung tung (No Print/Log Clutter)
- **Cấm tuyệt đối Log trong Update()**: Không được đặt `print()`, `Debug.Log()`, `console.log()` hoặc bất kỳ lệnh xuất log nào trong các hàm lặp mỗi khung hình (`Update`, `FixedUpdate`, `LateUpdate`, vòng lặp Match-3 cascade).
- **Tránh Log vô nghĩa**: Không dùng các chuỗi log vô định như `print("---")`, `Debug.Log("here")`, `Debug.Log("111")`, `Debug.Log("done")`.
- **Log có cấu trúc & mục đích rõ ràng**: Chỉ ghi log khi có lỗi ngoại lệ (`Debug.LogError`), cảnh báo rủi ro (`Debug.LogWarning`), hoặc sự kiện khởi tạo quan trọng một lần với tiền tố chuẩn xác:
  - Ví dụ: `Debug.Log("[IUH Guardian] Board initialized with 36 tiles.");`

## 4. Đúng yêu cầu, đúng phạm vi (Scope & Integrity)
- **Không code vượt phạm vi yêu cầu (YAGNI)**: Chỉ tập trung giải quyết đúng vấn đề người dùng yêu cầu, không tự ý thêm các tính năng chưa thống nhất.
- **Không thay đổi Menu / UI gốc ngoài yêu cầu**: Giữ nguyên kiến trúc GameFlow và UI cha, không tự ý sửa đổi menu khác trong hệ thống.
- **Không làm ảnh hưởng tới các mini-game / module khác**: Mọi đoạn code mới phải nằm trong namespace và thư mục riêng (`IUHCampus.Guardian` hoặc `IUHCampus.Scripts.Guardian`), không sửa chéo sang code logic của module khác trừ khi có yêu cầu tích hợp rõ ràng.

