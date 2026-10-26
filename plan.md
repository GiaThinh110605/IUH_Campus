# PLAN ĐỒ ÁN UNITY 3D: CAMPUS ADVENTURE

## 1. Tổng quan dự án

### Tên dự án
**Campus Adventure: Haunted & Green Campus**

### Thể loại
Game phiêu lưu 3D trong khuôn viên trường đại học, kết hợp nhiệm vụ, khám phá, kinh dị nhẹ, nhận diện vật thể bằng Deep Learning, AR và IoT Cloud.

### Ý tưởng cốt truyện
Người chơi nhập vai một sinh viên kiêm tài xế giao hàng trong khuôn viên trường Đại học Công nghiệp TP.HCM (IUH). Để hoàn thành ngày làm việc và bảo vệ môi trường trường học, người chơi thực hiện chuỗi nhiệm vụ: giao hàng ban đêm vượt qua các vùng sương mù và hồn ma, tìm kiếm đồ vật/giáo trình bằng AI, chụp ảnh khám phá địa danh campus với AR, và thu gom phân loại rác để nâng cao điểm xanh trường học.

Thành tích ở các nhiệm vụ trước sẽ đóng góp trực tiếp vào điểm thưởng, vật phẩm, điểm xanh và đồng bộ lên đám mây IoT Adafruit IO. Bốn hoạt động được tích hợp trong **một game Unity 3D duy nhất**, không phải các game riêng lẻ.

### Mục tiêu dự án
- Xây dựng môi trường campus 3D có thể khám phá tự do.
- Tạo hệ thống nhân vật, phương tiện giao hàng, nhiệm vụ, vật phẩm, điểm số và tiến trình.
- Tích hợp AR và Deep Learning bằng chức năng có thể kiểm thử thực tế.
- Xây dựng bản mobile, ưu tiên Android APK với cụm phím điều khiển cảm ứng.
- Kết nối IoT Cloud qua Adafruit IO (REST API / MQTT).
- Đóng gói backend và dịch vụ AI bằng Docker.
- Chuẩn bị tài liệu, kiểm thử và video demo để chứng minh đầy đủ các yêu cầu theo rubric.

---

## 2. Bốn hoạt động chính

### 2.1. Grab Campus: Chuyến xe tâm linh

**Mô tả:** Người chơi nhận đơn giao hàng giữa quán trà sữa / căn tin và ký túc xá / các tòa nhà giảng đường. Nhiệm vụ diễn ra vào ban đêm với sương mù dày đặc và các bóng ma tuần tra cản trở.

**Gameplay**
- Nhận đơn hàng từ bảng nhiệm vụ hoặc trực tiếp tại quầy quán nước/căn tin (NPC chủ quán).
- Lái xe máy trong campus 3D đến điểm lấy hàng và giao tận tay sinh viên nhận hàng (NPC KTX).
- Tránh chướng ngại vật, né ma hoặc chủ động chiến đấu bằng gậy để đánh đuổi ma giải vây, bảo vệ toàn vẹn kiện hàng.
- Lựa chọn chiến thuật đối đầu: bứt tốc bỏ chạy (Turbo Drift 80 km/h) hoặc ở lại dùng gậy đánh văng ma (+10.000 VNĐ dũng cảm).
- Giao hàng đúng hạn, bàn giao kiện hàng cho sinh viên để nhận tiền thưởng, điểm kinh nghiệm (EXP) và đánh giá sao.

**Yêu cầu hoàn thành**
- Điều khiển xe máy vật lý (tăng tốc, lùi, nghiêng xe, drift, còi xe) và camera hoạt động ổn định.
- Có điểm nhận hàng 3D, điểm giao hàng 3D với cột sáng chỉ đường (Sky Beacon) và Minimap trực quan.
- Có tính điểm, cơ chế máu kiện hàng, thưởng/phạt và trạng thái thắng/thua rõ ràng.
- Giao diện bàn giao hàng hóa với NPC và hộp thoại đánh giá trước khi kết thúc màn.

---

### 2.2. AI Object Hunt: Truy tìm đồ vật

**Mô tả:** NPC giảng viên hoặc sinh viên giao nhiệm vụ tìm kiếm các đồ vật cụ thể bị thất lạc trong khuôn viên trường (sách giáo trình, ghế học, bình chữa cháy, thiết bị phòng lab). Người chơi dùng camera điện thoại/trong game để quét và nhận diện mục tiêu bằng Deep Learning.

**Gameplay**
- Hiển thị đồ vật mục tiêu và gợi ý khu vực tìm kiếm.
- Bật camera quét khung hình trực tiếp.
- Gửi khung hình đến mô hình AI (YOLO) chạy trên backend hoặc inference trực tiếp trên thiết bị.
- Đọc nhãn (label), độ tin cậy (confidence score) và bounding box từ kết quả inference thực tế.
- Chỉ hoàn thành nhiệm vụ khi nhận diện đúng đối tượng yêu cầu với độ tin cậy đạt ngưỡng (> 70%).

**Tích hợp AR**
- Sau khi AI nhận diện thành công, AR Foundation chiếu mô hình 3D ảo và thông tin chi tiết của vật thể nổi lên trên mặt sàn thực tế.
- Người chơi có thể tương tác, xoay và thu thập vật phẩm ảo vào kho đồ.

**Yêu cầu hoàn thành**
- Mô hình Deep Learning thực sự chạy inference, có logging kết quả.
- Có xử lý các trường hợp nhận diện sai, không tìm thấy hoặc mạng yếu.
- Không dùng mã QR code hoặc dữ liệu giả để thay thế AI.
- Cập nhật tiến trình và cộng điểm thưởng khi hoàn thành.

---

### 2.3. Thợ săn khoảnh khắc

**Mô tả:** Người chơi khám phá toàn bộ campus IUH và hoàn thành thử thách chụp ảnh check-in các công trình biểu tượng (Cổng 1 Nguyễn Văn Bảo, Tượng đài/Quảng trường trung tâm, Thư viện Tòa A, Sân bóng, Vườn cây).

**Gameplay**
- Hiển thị danh sách các địa điểm cần check-in và hướng dẫn di chuyển.
- Kích hoạt chế độ máy ảnh (Photo Mode) với góc nhìn tự do, zoom và khung ngắm.
- Chụp ảnh và lưu trữ vào album bộ sưu tập thành tích của người chơi.
- Trao huy hiệu thám hiểm (Explorer Badge) và điểm kinh nghiệm khi chụp đủ các góc chuẩn.

**Tích hợp AR**
- Hiển thị khung định vị AR, thông tin lịch sử của tòa nhà và các linh vật ảo tại vị trí check-in.
- Cho phép người chơi chụp ảnh chung với các hiệu ứng ảo trên nền camera thật.

**Yêu cầu hoàn thành**
- Có mục tiêu, cơ chế chụp ảnh và lưu trữ album ảnh.
- Xác định vị trí địa lý trong game để kích hoạt nhiệm vụ chụp ảnh hợp lệ.
- Hiển thị thông báo và phần thưởng khi hoàn thành mỗi địa điểm.

---

### 2.4. Green Campus: Cứu lấy khuôn viên trường

**Mô tả:** Người chơi tham gia chiến dịch làm sạch môi trường học đường bằng cách thu gom rác thải, phân loại rác đúng quy định và phục hồi các mảng xanh của trường IUH.

**Gameplay**
- Nhận nhiệm vụ dọn dẹp tại các khu vực sân trường, công viên ghế đá hoặc căn tin.
- Nhặt các loại rác thải 3D (chai nhựa, lon nhôm, giấy vụn, rác hữu cơ) vào túi gom rác.
- Đưa rác đến trạm phân loại thông minh: dùng camera AI để nhận diện phân loại rác tái chế / vô cơ / hữu cơ.
- Bỏ rác đúng thùng rác tương ứng để nhận điểm xanh (Green Score), bỏ sai bị trừ điểm.
- Điểm xanh tích lũy mở khóa trồng thêm cây xanh và cải thiện cảnh quan trường học.

**Tích hợp IoT Cloud (Adafruit IO)**
- Đồng bộ điểm xanh của người chơi lên feed `green-score` trên đám mây Adafruit IO.
- Đọc dữ liệu môi trường (nhiệt độ, chất lượng không khí, độ ẩm) từ feed Adafruit IO về hiển thị trên bảng điện tử trong game.
- Đồng bộ trạng thái khu vực sạch/bẩn lên Dashboard giám sát thời gian thực.

**Yêu cầu hoàn thành**
- Có gameplay thu gom và phân loại rác hoàn chỉnh.
- Kết nối REST API / MQTT hai chiều với Adafruit IO (gửi điểm số, nhận dữ liệu môi trường).
- Minh chứng feed hoạt động thực tế với dashboard đồ thị thời gian thực.

---

## 3. Liên kết bốn hoạt động thành một game

Bốn hoạt động được xâu chuỗi liền mạch trong một hành trình xuyên suốt khuôn viên IUH:

1. **Khởi đầu — Giao hàng đêm (2.1):** Người chơi bắt đầu ca làm Grab Campus, lái xe máy giao trà sữa đêm tới KTX, đối mặt với thử thách bóng ma và né tránh sương mù.
2. **Khám phá — Truy tìm đồ vật (2.2):** Tại điểm giao KTX, sinh viên nhờ người chơi dùng điện thoại quét AI Object Hunt để tìm kiếm giáo trình quý bị thất lạc quanh giảng đường.
3. **Thám hiểm — Check-in kỷ niệm (2.3):** Trong quá trình tìm kiếm, người chơi kích hoạt Photo Mode hoàn thành album chụp ảnh AR các địa danh lịch sử của IUH.
4. **Về đích — Bảo vệ môi trường (2.4):** Kết thúc ngày làm việc bằng việc dọn dẹp khuôn viên, phân loại rác tại trạm tái chế và đồng bộ điểm xanh lên Adafruit IO Cloud để hoàn tất ngày trải nghiệm xuất sắc.

### Trạng thái tiến trình chung
- Tiền thưởng (VNĐ) và điểm kinh nghiệm (EXP).
- Kho đồ vật phẩm (Inventory).
- Bộ sưu tập ảnh check-in và huy hiệu.
- Điểm xanh (Green Score) và cấp độ môi trường.
- Trạng thái hoàn thành 4 minigame.

---

### 3.1. Menu Chọn Mini-Game Trong Game (In-Game Portal & Fast Travel)

Để người chơi tự do trải nghiệm cũng như phục vụ công tác demo/chấm điểm nhanh của giảng viên, game tích hợp **Menu Chọn Mini-Game** trực tiếp trong HUD:

- **Cách kích hoạt:**
  - **Trên PC:** Nhấn phím tắt **`[M]`** (hoặc `[Tab]`).
  - **Trên Mobile:** Nhấn nút cảm ứng nổi **`[🎮 Mini-Games]`** trên màn hình.
- **Tính năng cốt lõi:**
  - **Danh sách 4 Mini-game:** Trình bày dạng 4 thẻ card trực quan, cân đối với đầy đủ tên, icon, người phụ trách và tóm tắt gameplay.
  - **Chế độ Fast Travel:** Click **`[Chơi Ngay]`** / **`[Dịch Chuyển]`** để lập tức đưa nhân vật đến điểm xuất phát của minigame tương ứng.
  - **Theo dõi tiến trình:** Hiển thị số sao đánh giá (1-5 sao) và trạng thái Đã hoàn thành / Chưa hoàn thành.

---

## 4. Đối chiếu rubric 100 điểm

| Hạng mục Rubric | Điểm | Hoạt động đảm nhiệm | Bằng chứng cần có |
|---|:---:|---|---|
| **Unity 3D Implementation** | **20** | **2.1 Grab Campus** & Campus 3D | Môi trường 3D IUH, điều khiển xe máy/nhân vật, va chạm, NPC, minimap, UI HUD |
| **AR & Deep Learning Model** | **20** | **2.2 AI Object Hunt** | Mô hình YOLO inference thực tế nhận diện vật thể + AR Foundation hiển thị 3D |
| **Mobile Deployment** | **20** | Toàn bộ ứng dụng | Bản build Android APK, cụm joystick cảm ứng, tối ưu FPS và bộ nhớ |
| **IO Cloud Integration** | **20** | **2.4 Green Campus** | Kết nối Adafruit IO MQTT/REST: đồng bộ điểm xanh Green Score, dữ liệu môi trường |
| **Docker Use** | **20** | Backend & AI Service | Dockerfile, docker-compose.yml đóng gói FastAPI backend + dịch vụ AI inference |
| **Tổng Base Score** | **100** | | Đạt tối đa 100/100 điểm cơ bản |
| **Supporting Work Deduction** | **0** | Đầy đủ 5 mục bắt buộc | Planning, Testing, Integration Evidence, Documentation, Demo (Trừ 0đ) |
| **TỔNG ĐIỂM DỰ KIẾN** | **100/100** | | |

### Chiến lược đáp ứng từng hạng mục

**Unity 3D**
- Môi trường campus IUH 3D chi tiết với các tòa nhà A, B, E, V, X, KTX, Căn tin, Cổng trường.
- Cơ chế lái xe máy chân thực với âm thanh động cơ, nghiêng xe, đèn pha chiếu sáng ban đêm.
- Hệ thống AI ma tuần tra và rượt đuổi; cơ chế chiến đấu bằng gậy và bứt tốc turbo.

**AR + Deep Learning**
- Ứng dụng AR Foundation hỗ trợ ARCore trên Android.
- Mô hình YOLO nhận diện các vật thể học đường thực tế.
- Hiển thị mô hình 3D ảo khớp với bề mặt phát hiện trong không gian thực.

**Mobile**
- Build APK kiểm thử trên thiết bị Android thực tế.
- Giao diện cảm ứng tối ưu, phím bấm responsive, camera mượt mà.

**IoT Cloud**
- Thiết lập các feed trên Adafruit IO: `green-score`, `campus-temperature`, `clean-status`.
- Đọc và ghi dữ liệu ổn định qua REST API / MQTT.

**Docker**
- Đóng gói toàn bộ backend FastAPI và model AI vào Docker container.
- Cấu hình khởi chạy một lệnh duy nhất: `docker compose up -d`.

---

## 5. Kiến trúc công nghệ đề xuất

```text
┌────────────────────────────────────────────────────────┐
│                   Unity 3D Mobile                      │
│                                                        │
│ • Grab Campus (Lái xe, NPC, Ghost AI, Delivery)        │
│ • AI Object Hunt (Camera Frame Capture, AR Overlay)   │
│ • Thợ Săn Khoảnh Khắc (Photo Mode, Check-in Album)     │
│ • Green Campus (Dọn rác, Phân loại, UI Điểm xanh)      │
│ • Mini-Game Portal [M] & Touch Controls                │
└───────────────────────────┬────────────────────────────┘
                            │ HTTP / REST API
                            ▼
┌────────────────────────────────────────────────────────┐
│             Dockerized FastAPI Backend                 │
│                                                        │
│ • AI Inference Service (YOLO Model Server)             │
│ • Mission Progress & Scoring API                       │
│ • Adafruit IO Bridge / Cloud Sync Service              │
└─────────────┬────────────────────────────┬─────────────┘
              │                            │
              ▼                            ▼
┌──────────────────────────┐  ┌──────────────────────────┐
│        AI Engine         │  │       Adafruit IO        │
│ YOLO Object Detection    │  │ • Feed: green-score      │
│ PyTorch / ONNX Runtime   │  │ • Feed: environment      │
└──────────────────────────┘  └──────────────────────────┘
```

---

## 6. Kế hoạch kiểm thử và minh chứng

| Mã TC | Kịch bản kiểm thử | Hoạt động | Kết quả mong đợi |
|---|---|---|---|
| **TC01** | Di chuyển xe máy & nhân vật trong campus | 2.1 Grab Campus | Xe máy tăng tốc, phanh, drift, còi và va chạm vật lý chuẩn |
| **TC02** | Nhận và giao kiện hàng Grab đêm | 2.1 Grab Campus | Lấy hàng tại quán ToCoToCo, giao tận tay NPC KTX, hiện modal bàn giao |
| **TC03** | Đối đầu hồn ma (Chạy trốn / Đánh trả) | 2.1 Grab Campus | Ma rượt đuổi < 24m; Turbo cắt đuôi thành công hoặc phím Q đánh văng ma |
| **TC04** | Nhận diện vật thể bằng Deep Learning | 2.2 AI Object Hunt | Camera gửi frame, backend YOLO trả đúng label và bounding box |
| **TC05** | Chiếu mô hình 3D ảo qua AR | 2.2 AI Object Hunt | AR Foundation phát hiện mặt phẳng, neo mô hình 3D vật thể ổn định |
| **TC06** | Chụp ảnh check-in địa danh IUH | 2.3 Thợ săn khoảnh khắc | Photo Mode chụp ảnh, lưu album và trao huy hiệu khi đúng tọa độ |
| **TC07** | Phân loại rác và tính điểm xanh | 2.4 Green Campus | Nhặt rác 3D, phân loại đúng thùng rác, tăng Green Score trong game |
| **TC08** | Đồng bộ dữ liệu lên Adafruit IO | 2.4 Green Campus | Điểm xanh gửi lên cloud thành công; cập nhật đồ thị trên Adafruit IO |
| **TC09** | Khởi chạy hệ thống qua Docker | Backend & AI | Container FastAPI + YOLO khởi động bằng docker compose, health check 200 OK |
| **TC10** | Cài đặt và vận hành trên điện thoại Android | Toàn bộ ứng dụng | Bản build APK cài đặt mượt mà, cảm ứng phản hồi tốt |

---

## 7. Kịch bản demo cuối kỳ (7–10 phút)

1. **Mở đầu (1 phút):** Giới thiệu bối cảnh khuôn viên IUH 3D, bật Menu `[M]` giới thiệu 4 minigame.
2. **Grab Campus (3 phút):** Nhận xe tại bãi xe Cổng 1, lấy trà sữa tại quán, chạy đêm vượt sương mù, dùng gậy đánh ma / bật Turbo cắt đuôi, bàn giao hàng cho sinh viên KTX nhận thưởng.
3. **AI Object Hunt & AR (2 phút):** Mở camera điện thoại, quét giáo trình/vật thể thật, hiển thị kết quả AI YOLO và chiếu mô hình 3D ảo bằng AR Foundation.
4. **Thợ săn khoảnh khắc (1 phút):** Di chuyển đến Quảng trường trung tâm, bật Photo Mode check-in và lưu ảnh vào bộ sưu tập.
5. **Green Campus & Adafruit IO (2 phút):** Thu gom rác thải, phân loại rác, điểm xanh tăng lên và đồng bộ thời gian thực lên đồ thị Adafruit IO.
6. **Minh chứng kỹ thuật (1 phút):** Mở terminal xem Docker container đang chạy, xem API docs Swagger UI và log Adafruit IO.

---

## 8. Phân công nhiệm vụ (2 thành viên)

| Thành viên | Trách nhiệm chính (2 Mini-game + Kỹ thuật) | Trọng tâm công nghệ |
|---|---|---|
| **Bạn (Trưởng nhóm)** | • **2.1 Grab Campus: Chuyến xe tâm linh**<br>• **2.2 AI Object Hunt: Truy tìm đồ vật**<br>• Unity 3D Core, Xe máy, NavMesh Ghost AI, AR Foundation, Mobile Build Android APK | Unity 3D, C#, AR Foundation, Deep Learning YOLO |
| **Minh Nhật** | • **2.3 Thợ săn khoảnh khắc: Khám phá campus**<br>• **2.4 Green Campus: Cứu lấy khuôn viên trường**<br>• Kết nối Adafruit IO MQTT/REST Cloud, Xây dựng Backend FastAPI & Đóng gói Docker | IoT Cloud (Adafruit IO), Python FastAPI, Docker Compose |

---

## 9. Tiêu chí nghiệm thu tổng thể

Dự án được đánh giá hoàn thành xuất sắc khi:
- Người chơi hoàn thành trọn vẹn chuỗi nhiệm vụ qua **4 hoạt động trọng tâm**.
- Gameplay Unity 3D hoạt động ổn định trên PC và thiết bị Android mục tiêu.
- Mô hình Deep Learning thực hiện inference thực tế trên ảnh camera.
- AR hoạt động tốt trên thiết bị tương thích.
- Ứng dụng trao đổi dữ liệu hai chiều thực tế với Adafruit IO Cloud.
- Backend và dịch vụ AI khởi chạy đầy đủ qua Docker.
- Đầy đủ báo cáo, kịch bản test có minh chứng và video demo sản phẩm.
