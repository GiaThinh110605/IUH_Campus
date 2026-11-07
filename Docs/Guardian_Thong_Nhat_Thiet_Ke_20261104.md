# IUH Campus Guardian — Ý tưởng cách chơi mới

Cốt truyện: người chơi là **người bảo vệ IUH Campus**, giữ khuôn viên tươi đẹp và chống lại Data Monster xâm nhập.

## Cách chơi trên iPhone (iOS, AR)

1. Mở scan. Backend bí mật chọn 1–2 loại đồ "bị nhiễm" cho lượt chơi đó, ví dụ hôm nay là `laptop` và `chair`.
2. Game chỉ cho gợi ý, không nói thẳng: "Tín hiệu nhiễu mạnh từ đồ điện tử..." hoặc "...từ thứ để ngồi".
3. Người chơi chĩa camera vào đồ vật. Giữ yên khoảng 1,5 giây, thanh "phân tích" chạy đầy.
4. Kết quả:
   - **Đồ bị nhiễm:** khói tím bốc lên, monster chui ra đứng trên mặt phẳng AR, chạm để vào match-3.
   - **Đồ sạch:** hiện "✓ An toàn". Người chơi tìm món khác.
5. Mỗi lượt chỉ có 1–2 quái. Đánh xong thì hết, không quét mãi để farm Core.

Đồ vật nhận diện được: sách, laptop, ghế, chai nước, balo, điện thoại; **đồ có thể bị nhiễm chỉ gồm điện thoại, laptop, ghế** (`SCAN_INFECTED_POOL=cell phone,laptop,chair`). Chọn pool này vì recall B2.6 tại ngưỡng 0.70 tốt hơn ba lớp còn lại trên tập nhỏ; xem [real-eval.md](Evidence/CampusGuardian/B2/real-eval.md). Giữ YOLOv8n 640/ngưỡng 0.70; sách/chai nước/balo được nhận diện là đồ sạch. Chọn 1–2 lớp không vượt số lớp trong pool; laptop/điện thoại cùng gợi ý “đồ điện tử” và không lặp chữ, ghế là “thứ để ngồi”.

Nền tảng mobile: **iOS (iPhone, ARKit)** thay cho Android vì nhóm không có máy Android. Build iOS trên Mac (Gia Thịnh). Android chuyển sang hướng phát triển.

## Cách chơi trên laptop / PC

Người chơi là người bảo vệ IUH Campus:

- Chăm sóc khuôn viên: trồng hoa, tưới hoa, giữ cho khuôn viên tươi đẹp.
- Monster xâm nhập campus, phá hoại cây cối, khu vực.
- Chống lại monster bằng trận match-3. Thắng thì khu vực được hồi phục.

## Chung cho cả hai

- Gặp monster → vào trận **match-3**.
- Thắng → nhận **Campus Core** → dùng để nâng cấp.
