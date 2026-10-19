
# PLAN ĐỒ ÁN UNITY 3D: CAMPUS ADVENTURE

## 1. Tổng quan dự án

### Tên dự án
**Campus Adventure: Haunted & Green Campus**

### Thể loại
Game phiêu lưu 3D trong khuôn viên trường đại học, kết hợp nhiệm vụ, khám phá, kinh dị nhẹ, nhận diện vật thể bằng Deep Learning, AR và IoT Cloud.

### Ý tưởng cốt truyện
Người chơi nhập vai một sinh viên kiêm tài xế giao hàng trong khuôn viên trường. Trường sắp tổ chức một lễ hội lớn, nhưng người chơi cần hoàn thành nhiều nhiệm vụ trước giờ khai mạc: giao hàng, khám phá bí ẩn trong ký túc xá, tìm đồ vật, chụp ảnh địa điểm và cải thiện môi trường trường học.

Thành tích ở các nhiệm vụ trước sẽ ảnh hưởng đến tài nguyên, điểm xanh và kết quả lễ hội cuối game. Sáu hoạt động được tích hợp trong **một game Unity 3D duy nhất**, không phải sáu game riêng lẻ.

### Mục tiêu dự án
- **Lựa chọn công nghệ trọng tâm**: **100% Unity 3D** (giảng viên xác nhận chọn 1 giữa 2D và 3D).
- **Phân công 2 thành viên (3 - 3 Mini-game)**:
  - **Bạn (Trưởng nhóm)**: 3 Mini-game (1. Grab Campus, 2. Trốn ma KTX, 3. AI Object Hunt) + Unity 3D Core & AR/Mobile Build.
  - **Minh Nhật**: 3 Mini-game (4. Thợ săn khoảnh khắc, 5. Green Campus, 6. Campus Festival) + Adafruit IO Cloud & Docker Backend.
- Xây dựng môi trường campus 3D có thể khám phá.
- Tạo hệ thống nhân vật, nhiệm vụ, vật phẩm, điểm số và tiến trình.
- Tích hợp AR và Deep Learning bằng chức năng có thể kiểm thử thực tế.
- Xây dựng bản mobile, ưu tiên Android nếu thiết bị tương thích.
- Kết nối IoT Cloud qua Adafruit IO.
- Đóng gói backend và dịch vụ AI bằng Docker.
- Chuẩn bị tài liệu, kiểm thử và video demo để chứng minh các yêu cầu.
- **Bảng điều khiển & mô phỏng trực quan**: Xem file [IUH_Plan_Dashboard.html](IUH_Plan_Dashboard.html) để trải nghiệm luồng game mô phỏng và bảng tính điểm Rubric 100đ tương tác.

---

## 2. Sáu hoạt động chính

### 2.1. Grab Campus: Chuyến xe tâm linh

**Mô tả:** Người chơi nhận đơn giao hàng giữa cổng trường, căn tin, thư viện và ký túc xá. Một số nhiệm vụ diễn ra ban đêm, khi hồn ma xuất hiện.

**Gameplay**
- Nhận đơn hàng từ NPC hoặc bảng nhiệm vụ.
- Lái xe đến điểm nhận và điểm giao hàng.
- Tránh chướng ngại vật, né ma và bảo vệ hàng hóa.
- Giao đúng hạn để nhận tiền, điểm kinh nghiệm hoặc vật phẩm.
- Mở khóa đơn hàng khó hơn và các khu vực mới.

**Yêu cầu hoàn thành**
- Điều khiển phương tiện và camera hoạt động ổn định.
- Có điểm nhận hàng, điểm giao hàng và điều kiện hoàn thành.
- Có tính điểm, thưởng/phạt và trạng thái thắng/thua.
- Tiến trình nhiệm vụ được lưu hoặc đồng bộ theo thiết kế.

### 2.2. Trốn tìm với ma ở ký túc xá

**Mô tả:** Người chơi tìm chìa khóa, bật cầu dao và thoát khỏi ký túc xá trong khi hồn ma tuần tra và săn đuổi.

**Gameplay**
- Khám phá phòng, hành lang và khu vực ẩn nấp.
- Thu thập chìa khóa, cầu chì hoặc nhật ký.
- Ẩn nấp để tránh bị phát hiện.
- Giải mã manh mối để mở lối thoát.
- Đạt điều kiện thắng trước khi bị bắt hoặc hết thời gian.

**AI hành vi của ma**
- `Patrol`: tuần tra theo tuyến đường.
- `Investigate`: điều tra tiếng động hoặc dấu hiệu bất thường.
- `Chase`: truy đuổi khi phát hiện người chơi.
- `Search`: tìm quanh vị trí cuối cùng nhìn thấy người chơi.
- `Return`: quay lại tuyến tuần tra khi không tìm thấy người chơi.

**Yêu cầu hoàn thành**
- Có chuyển trạng thái AI rõ ràng.
- Có điều kiện phát hiện, mất dấu, thắng và thua.
- Có thể kiểm thử hành vi ma trong nhiều tình huống.

### 2.3. AI Object Hunt: Truy tìm đồ vật

**Mô tả:** NPC giao nhiệm vụ tìm một đồ vật cụ thể, chẳng hạn sách, ghế, bình chữa cháy hoặc dụng cụ phòng thí nghiệm. Người chơi dùng camera để nhận diện mục tiêu.

**Gameplay**
- Hiển thị đồ vật mục tiêu và gợi ý vị trí.
- Chụp ảnh hoặc lấy khung hình từ camera.
- Gửi ảnh đến mô hình AI hoặc chạy mô hình trực tiếp trên thiết bị.
- Đọc nhãn, độ tin cậy và bounding box từ kết quả inference.
- Chỉ hoàn thành nhiệm vụ khi kết quả đáp ứng điều kiện đã định.

**Tích hợp AR**
- Sau khi nhận diện đúng, hiển thị mô hình 3D, nhãn hoặc thông tin ảo.
- Có thể đặt nội dung lên bề mặt được AR phát hiện.

**Yêu cầu hoàn thành**
- Mô hình Deep Learning thực sự chạy inference.
- Có đầu vào, đầu ra và xử lý trường hợp không nhận diện được.
- Không thay nhận diện bằng QR code hoặc dữ liệu giả.
- Lưu kết quả nhiệm vụ và điểm thưởng.

### 2.4. Thợ săn khoảnh khắc

**Mô tả:** Người chơi khám phá trường và hoàn thành thử thách chụp ảnh các địa điểm như cổng trường, thư viện, sân bóng, vườn cây hoặc khu lễ hội.

**Gameplay**
- Hiển thị danh sách mục tiêu chụp ảnh.
- Cho phép chụp ảnh trong game hoặc qua camera điện thoại theo thiết kế.
- Lưu ảnh hoặc kết quả hoàn thành.
- Trao huy hiệu, điểm khám phá hoặc mở khóa thử thách mới.
- Có thể dùng AI nhận diện địa điểm hoặc đối tượng trong ảnh để xác minh.

**Tích hợp AR**
- Hiển thị khung hình, chỉ dẫn hoặc vật thể ảo tại địa điểm.
- Cho phép người chơi chụp ảnh cùng nội dung AR nếu thiết bị hỗ trợ.

**Yêu cầu hoàn thành**
- Có mục tiêu, điều kiện hoàn thành và phần thưởng rõ ràng.
- Phân biệt ảnh chụp trong game với ảnh từ camera thật.
- Nếu dùng AI xác minh ảnh, phải ghi nhận kết quả inference thực tế.

### 2.5. Green Campus: Cứu lấy khuôn viên trường

**Mô tả:** Người chơi thu gom rác, phân loại và phục hồi các khu vực bị ô nhiễm.

**Gameplay**
- Nhận nhiệm vụ dọn dẹp sân trường, công viên hoặc căn tin.
- Thu gom vật phẩm rác trong môi trường 3D.
- Dùng AI phân loại rác qua camera.
- Cộng hoặc trừ điểm xanh tùy theo kết quả phân loại.
- Mở khóa cây xanh, thùng rác phân loại hoặc khu vực được cải thiện.

**Tích hợp IoT Cloud**
- Hiển thị dữ liệu môi trường như nhiệt độ, độ ẩm hoặc chỉ số môi trường từ feed Adafruit IO nếu có nguồn dữ liệu tương ứng.
- Đồng bộ điểm xanh hoặc trạng thái khu vực lên cloud.
- Nếu chưa có cảm biến vật lý, phải ghi rõ dữ liệu đang mô phỏng.

**Yêu cầu hoàn thành**
- Có nhiệm vụ dọn dẹp và hệ thống điểm xanh.
- Có AI phân loại rác chạy thực tế nếu dùng để đáp ứng phần Deep Learning.
- Có dữ liệu gửi/nhận với Adafruit IO và minh chứng được.

### 2.6. Campus Festival: Tổ chức lễ hội

**Mô tả:** Người chơi chuẩn bị lễ hội cuối game bằng cách thu thập tài nguyên và hoàn thành nhiệm vụ trước giờ khai mạc.

**Gameplay**
- Thu thập vật liệu, trang trí sân khấu và sắp xếp gian hàng.
- Quản lý ngân sách, thời gian và điểm xanh.
- Dùng tiền hoặc vật phẩm kiếm được từ nhiệm vụ trước.
- Hoàn thành các mục tiêu chuẩn bị trước khi hết giờ.
- Tạo nhiều kết quả lễ hội dựa trên thành tích.

**Tích hợp Cloud**
- Đồng bộ điểm số hoặc trạng thái lễ hội lên Adafruit IO hoặc backend.
- Hiển thị trạng thái tổng quan trong giao diện quản trị/demo nếu cần.

**Yêu cầu hoàn thành**
- Có tài nguyên, ngân sách, đồng hồ hoặc giới hạn nhiệm vụ.
- Kết quả lễ hội phụ thuộc tiến trình người chơi.
- Có màn hình kết quả cuối game.

---

## 3. Liên kết sáu hoạt động thành một game

Không nên để sáu hoạt động là sáu màn chơi không liên quan. Hãy sử dụng một vòng lặp nhiệm vụ xuyên suốt.

1. Người chơi nhận nhiệm vụ giao hàng đến ký túc xá.
2. Tại ký túc xá, người chơi tìm chìa khóa, né ma và thu thập nhật ký.
3. Nhật ký dẫn đến nhiệm vụ tìm một vật thể đặc biệt bằng AI Object Hunt.
4. Khi khám phá campus, người chơi hoàn thành thử thách chụp ảnh địa điểm.
5. Người chơi tham gia Green Campus, thu gom và phân loại rác để tăng điểm xanh.
6. Tiền, vật phẩm, điểm xanh và manh mối thu được được dùng để chuẩn bị Campus Festival.
7. Kết quả lễ hội thay đổi theo thành tích ở các nhiệm vụ trước.

### Trạng thái tiến trình chung
- Tiền và vật phẩm.
- Điểm kinh nghiệm hoặc cấp độ.
- Nhiệm vụ đã hoàn thành.
- Manh mối và vật phẩm cốt truyện.
- Bộ sưu tập ảnh/huy hiệu.
- Điểm xanh.
- Trạng thái chuẩn bị lễ hội.

**Nguyên tắc thiết kế:** mỗi hoạt động nên mở khóa một phần nội dung khác. Người chơi cần có lý do để tiếp tục khám phá, thay vì chỉ chơi các minigame tách rời.

### 3.1. Menu Chọn Mini-Game Trong Game (In-Game Mini-Game Portal & Teleport)

Để thuận tiện cho người chơi tự do trải nghiệm cũng như phục vụ công tác demo/chấm điểm nhanh của giảng viên, game tích hợp một **Menu Chọn Mini-Game** trực tiếp trong giao diện:

- **Cách kích hoạt:**
  - **Trên PC:** Nhấn phím tắt `[M]` (hoặc phím `[Tab]` / nút ESC menu).
  - **Trên Mobile:** Nhấn vào nút biểu tượng cảm ứng nổi `[🎮 Mini-Games]` ở góc trên màn hình HUD.
- **Tính năng cốt lõi của Menu:**
  - **Danh sách 6 Mini-game:** Trình bày dạng thẻ bài trực quan với đầy đủ tên, icon, người phụ trách (3 của Bạn / 3 của Minh Nhật), và tóm tắt yêu cầu.
  - **Chế độ Chọn Tự Do (Quick Play / Instant Teleport):** Người chơi/giảng viên có thể nhấp vào bất kỳ mini-game nào để bấm `[Chơi Ngay]` &rarr; Hệ thống tự động dịch chuyển nhân vật (Fast Travel) đến vị trí bắt đầu nhiệm vụ đó trên campus 3D và nạp bối cảnh tương ứng.
  - **Hiển thị Tiến trình & Đánh giá:** Ghi nhận trạng thái (Chưa mở khóa, Đang làm, Đã hoàn thành, Điểm số, Đánh giá 1-5 sao).
  - **Cơ chế Điều khiển:** Khi mở menu, hệ thống tạm khóa di chuyển nhân vật và mở khóa con trỏ chuột (`Cursor.lockState = CursorLockMode.None`) để thao tác thuận tiện; khi đóng menu (`[M]` hoặc nút `[X]`), con trỏ chuột tự động khóa lại góc nhìn camera thứ 3.

---

## 4. Đối chiếu rubric 100 điểm

| Hạng mục Rubric | Điểm | Người phụ trách | Bằng chứng cần có |
|---|:---:|:---:|---|
| **Unity 3D Implementation** *(chọn 3D)* | 20 | **Cả hai** | Môi trường 3D IUH, nhân vật, va chạm, leo thang, ngồi ghế, UI HUD |
| **AR & Deep Learning Model** | 20 | **Bạn** | AI Object Hunt (YOLO inference thật) + AR Foundation chiếu mô hình 3D |
| **Mobile Deployment** | 20 | **Bạn** | Bản build Android APK, cụm phím điều khiển cảm ứng, tối ưu FPS |
| **IO Cloud Integration** *(sử dụng)* | 20 | **Minh Nhật** | Kết nối Adafruit IO MQTT/REST: feed Green Score, trạng thái Festival |
| **Docker Use** | 20 | **Minh Nhật** | Dockerfile, docker-compose.yml đóng gói FastAPI backend + AI Service |
| **Tổng Base Score** | **100** | | |
| **Supporting Work Deduction** | **0** | **Cả hai** | Đủ 5 phần: Planning, Testing, Evidence, Documentation, Demo (Trừ 0đ) |
| **FINAL SCORE** | **100/100** | | |

Đây là kế hoạch đáp ứng các hạng mục dựa trên rubric đã nêu, không phải cam kết điểm số. Cần xác nhận với giảng viên yêu cầu cụ thể về loại thiết bị AR, mức độ sử dụng Docker và việc có cần cảm biến IoT vật lý hay không.

### Chiến lược đáp ứng từng hạng mục

**Unity 3D**
- Có môi trường campus với các khu vực rõ ràng.
- Có nhân vật, tương tác, hệ thống nhiệm vụ và tiến trình.
- Có ít nhất một vòng chơi hoàn chỉnh, không chỉ là cảnh 3D tĩnh.

**AR + Deep Learning**
- Dùng AR Foundation kết hợp ARCore nếu triển khai Android tương thích.
- Dùng mô hình nhận diện vật thể hoặc phân loại rác thực tế.
- Hiển thị nội dung AR sau khi xác nhận đối tượng.
- Lưu ảnh test, kết quả inference và video chạy thực tế.

**Mobile**
- Build và kiểm thử trên điện thoại mục tiêu từ sớm.
- Kiểm tra cảm ứng, camera, AR, tốc độ tải và mức sử dụng bộ nhớ.
- Có xử lý khi thiếu mạng hoặc không nhận diện được đối tượng.

**IoT Cloud**
- Tạo feed Adafruit IO cho dữ liệu môi trường, điểm xanh hoặc trạng thái lễ hội.
- Kiểm thử gửi và đọc dữ liệu qua REST API hoặc MQTT.
- Lưu bằng chứng feed, log và kết quả hiển thị trong game.
- Ghi rõ nếu dữ liệu cảm biến đang được mô phỏng.

**Docker**
- Đóng gói FastAPI và dịch vụ inference nếu dùng backend.
- Có cấu hình môi trường, health check và hướng dẫn chạy.
- Kiểm thử container khởi động lại và API phản hồi đúng.
- Không cần Docker hóa Unity mobile client.

**Tài liệu và demo**
- Sơ đồ kiến trúc tổng thể.
- Mô tả các luồng gameplay và tích hợp.
- Bảng kiểm thử có Pass/Fail và bằng chứng.
- README hướng dẫn chạy.
- Video demo dự phòng.

---

## 5. Kiến trúc công nghệ đề xuất

Với MacBook Apple Silicon 16 GB RAM và không có GPU NVIDIA cục bộ, ưu tiên mô hình nhỏ chạy CPU hoặc inference trên điện thoại. Không cần dựa vào GPU NVIDIA để phát triển bản đầu tiên.

### Kiến trúc tổng quan

```text
┌───────────────────────────────────────────┐
│             Unity 3D Mobile               │
│                                           │
│ Gameplay · UI · Camera · AR Foundation    │
│ Missions · Player · Ghost AI · Inventory  │
└─────────────────────┬─────────────────────┘
                      │ HTTP/HTTPS
                      ▼
┌───────────────────────────────────────────┐
│              FastAPI Backend              │
│                                           │
│ Mission API · Score API · AI Inference    │
│ Progress API · IoT Integration            │
└───────────┬──────────────────┬────────────┘
            │                  │
            ▼                  ▼
┌────────────────────┐  ┌───────────────────┐
│ AI Model            │  │ Database          │
│ YOLO / Classifier   │  │ SQLite/PostgreSQL │
└────────────────────┘  └───────────────────┘
            │
            ▼
┌───────────────────────────────────────────┐
│               Adafruit IO                 │
│ Environment Feeds · Green Score · Status  │
└───────────────────────────────────────────┘

Docker: đóng gói Backend, AI Service và các dịch vụ phù hợp.
```

### Công nghệ cụ thể

| Thành phần | Công nghệ đề xuất |
|---|---|
| Game | Unity 3D, C# |
| Điều khiển nhân vật | Character Controller, Input System |
| AI ma | NavMesh, finite-state machine |
| AR | AR Foundation + ARCore cho Android |
| Deep Learning | Ultralytics YOLO hoặc mô hình phân loại nhỏ |
| Backend | Python, FastAPI, Pydantic |
| IoT Cloud | Adafruit IO, REST API hoặc MQTT |
| Lưu tiến trình | SQLite ban đầu; PostgreSQL nếu cần |
| Đóng gói | Docker, Docker Compose |
| Kiểm thử | Unity Test Framework, pytest, kiểm thử trên điện thoại |

### Luồng dữ liệu chính
1. Unity gửi ảnh đến backend hoặc chạy mô hình AI trực tiếp trên thiết bị.
2. Mô hình trả về nhãn, độ tin cậy và bounding box nếu dùng object detection.
3. Unity kiểm tra kết quả và cập nhật nhiệm vụ, điểm số.
4. Backend gửi hoặc đọc dữ liệu với Adafruit IO.
5. Unity hiển thị dữ liệu môi trường, điểm xanh hoặc trạng thái lễ hội.
6. Tiến trình quan trọng được lưu theo thiết kế của game.

**Bảo mật:** không nhúng Adafruit IO key hoặc thông tin đăng nhập database trực tiếp vào ứng dụng Unity đã build. Đặt thông tin bí mật ở backend hoặc cấu hình môi trường phù hợp.

---

## 6. Kế hoạch kiểm thử và minh chứng

| Mã | Kịch bản kiểm thử | Kết quả mong đợi |
|---|---|---|
| TC01 | Di chuyển trong campus | Nhân vật di chuyển, va chạm và camera ổn định |
| TC02 | Giao đơn Grab Campus | Điểm đến được xác nhận, điểm thưởng cập nhật đúng |
| TC03 | Ma phát hiện người chơi | Ma chuyển trạng thái và truy đuổi đúng |
| TC04 | Tìm đồ vật bằng camera | Mô hình trả kết quả đúng định dạng; nhiệm vụ xử lý đúng/sai |
| TC05 | Đặt vật thể AR | Vật thể ảo xuất hiện và giữ vị trí hợp lý trong phiên AR |
| TC06 | Phân loại rác | Điểm xanh thay đổi theo kết quả và luật nhiệm vụ |
| TC07 | Tổ chức lễ hội | Tài nguyên và kết quả lễ hội phản ánh tiến trình đã chơi |
| TC08 | Gửi dữ liệu Adafruit IO | Feed nhận dữ liệu; backend đọc được dữ liệu cần thiết |
| TC09 | Backend mất kết nối | Game báo lỗi hợp lý, không bị treo hoặc mất tiến trình cục bộ |
| TC10 | Build và chạy mobile | Cài đặt thành công, các tính năng cốt lõi hoạt động |
| TC11 | Chạy Docker | Container khởi động, health check và API hoạt động |
| TC12 | Chơi toàn bộ chuỗi nhiệm vụ | Tiến trình và điều kiện mở khóa nhất quán |

Với mỗi bài kiểm thử, ghi lại:
- Kết quả mong đợi.
- Kết quả thực tế.
- Trạng thái Pass/Fail.
- Ảnh chụp màn hình, log hoặc video làm bằng chứng.

---

## 7. Kịch bản demo cuối kỳ

Đề xuất demo khoảng 7–10 phút:

1. **Giới thiệu:** môi trường campus 3D và điều khiển nhân vật.
2. **Grab Campus:** nhận đơn và giao hàng.
3. **Dorm Horror:** minh họa ma tuần tra, phát hiện và truy đuổi.
4. **AI Object Hunt:** dùng camera nhận diện vật thể thật và hiển thị kết quả inference.
5. **AR + Thợ săn khoảnh khắc:** đặt nội dung ảo và hoàn thành thử thách khám phá.
6. **Green Campus:** phân loại rác bằng AI và cập nhật điểm xanh.
7. **Campus Festival:** dùng tài nguyên đã thu thập để hoàn thành lễ hội.
8. **Minh chứng kỹ thuật:** mở Adafruit IO feed, API response và Docker logs.

Nếu demo trực tiếp không có Internet ổn định, chuẩn bị video dự phòng; phân biệt rõ video ghi sẵn với tính năng chạy trực tiếp.

---

## 8. Ưu tiên phạm vi khi thiếu thời gian

### P0 — Nền tảng bắt buộc
- Campus 3D và điều khiển nhân vật.
- Hệ thống nhiệm vụ, vật phẩm, điểm số.
- Một màn Grab Campus hoàn chỉnh.
- Một màn trốn ma hoàn chỉnh.
- Bản mobile chạy được.
- Prototype AR và AI có thể kiểm chứng.

### P1 — Hoàn thiện rubric
- AI Object Hunt với mô hình inference thực.
- Phân loại rác bằng mô hình.
- AR tương tác trên thiết bị mục tiêu.
- Adafruit IO gửi/nhận dữ liệu thật.
- Backend được đóng gói Docker.

### P2 — Nâng cấp gameplay
- Thợ săn khoảnh khắc với nhiều thử thách.
- Lễ hội có nhiều kết thúc.
- Trang trí campus và hiệu ứng nâng cao.
- Thành tích, bộ sưu tập và phần thưởng bổ sung.

P2 là các phần mở rộng bên trong gameplay, không phải lý do để bỏ các hạng mục bắt buộc trong rubric.

---

## 9. Checklist việc cần làm ngay

- [ ] Chọn nền tảng mobile mục tiêu và xác nhận thiết bị có hỗ trợ ARCore hay không.
- [ ] Dựng bản đồ campus blockout, phân chia khu vực cho sáu hoạt động.
- [ ] Tạo một nhiệm vụ hoàn chỉnh để kiểm chứng kiến trúc gameplay.
- [ ] Chọn dataset, các lớp AI và hướng inference.
- [ ] Tạo backend skeleton với health check.
- [ ] Tạo một Adafruit IO feed và kiểm tra gửi/nhận dữ liệu.
- [ ] Kiểm thử Docker sớm, không đợi đến cuối dự án.
- [ ] Chuẩn bị bảng kiểm thử và lưu minh chứng trong quá trình phát triển.

## 10. Tiêu chí nghiệm thu tổng thể

Dự án có thể xem là sẵn sàng để demo khi:
- Người chơi hoàn thành được chuỗi nhiệm vụ liên kết qua sáu hoạt động.
- Gameplay 3D hoạt động ổn định trên thiết bị mục tiêu.
- AI có thể chạy inference với đầu vào thực tế và kết quả được dùng trong game.
- AR hoạt động trên thiết bị tương thích.
- Ứng dụng/backend trao đổi dữ liệu với Adafruit IO.
- Backend và dịch vụ liên quan khởi chạy được qua Docker.
- Có bản build mobile, hướng dẫn chạy, bảng kiểm thử và video demo.
- Mỗi tiêu chí trong rubric đều có bằng chứng kiểm chứng cụ thể.

---

## 11. Hướng Dẫn Trải Nghiệm Mini-Game 2.1 & In-Game Menu

### 11.1. Menu Chọn Mini-Game (In-Game Portal)
- **Kích hoạt:**
  - Nhấn phím **`[M]`** trên bàn phím bất kỳ lúc nào trong quá trình khám phá.
  - Hoặc click vào nút nổi góc phải màn hình **`[🎮 Mini-Games [Phím M]]`**.
- **Tính năng:**
  - Danh sách 6 mini-game (3 game của Gia Thịnh, 3 game của Minh Nhật).
  - Tự động mở khóa chuột (`Cursor.lockState = CursorLockMode.None`) và tạm khóa chuyển động nhân vật để thao tác mượt mà.
  - Bấm **`[⚡ Chơi Ngay]`** trên thẻ Mini-game 2.1 để bắt đầu nhiệm vụ ngay lập tức.
  - Bấm **`[📍 Fast Travel]`** trên các thẻ khác để dịch chuyển tức thời đến trạm nhiệm vụ tương ứng (KTX, Thư viện, Nóc nhà Block F, Sân trường, Sân khấu).

### 11.2. Mini-Game 2.1: Grab Campus — Chuyến Xe Tâm Linh
- **Vị trí xe máy:** Bãi xe Cổng 1 Nguyễn Văn Bảo (`398.0, 0.55, -18.0`).
- **Phím điều khiển xe máy:**
  - **`W` / `↑`**: Tăng tốc tiến lên (lên đến 58 km/h).
  - **`S` / `↓`**: Lùi xe / Phanh.
  - **`A` / `D` (hoặc `←` / `→`)**: Đánh lái và nghiêng xe mượt mà (Banking tilt).
  - **`Space`**: Phanh gấp / Drift.
  - **`Left Shift`**: Bật Turbo tăng tốc tức thì (lên đến 79 km/h).
  - **`H`**: Bấm còi xe máy Grab.
  - **`E`**: Lên / Xuống xe khi đứng gần.
- **Hệ thống nhiệm vụ:**
  - **Đơn 1 (Dễ):** Cơm Sườn Đêm Căn Tin &rarr; Ký Túc Xá Cổng 4 (75s, thưởng 60,000 VNĐ).
  - **Đơn 2 (Trung bình):** Giáo Trình Cổ Thư Viện Tòa A &rarr; Giảng Đường Tòa V (60s, thưởng 85,000 VNĐ, có ma tuần tra).
  - **Đơn 3 (Khó):** Linh Kiện Điện Tử Khẩn Cấp Cổng 1 &rarr; Sảnh Tòa Nhà E (50s, thưởng 120,000 VNĐ, bão ma đêm + sương mù dày).
- **Cơ chế tâm linh & Chướng ngại vật:**
  - Khi bắt đầu nhiệm vụ, môi trường chuyển sang ban đêm (ánh trăng xanh ma mị, đèn pha xe máy chiếu đường).
  - Hồn ma (`IUHGhostHazard`) bay lượn trên đường: phát ra tiếng rít dọa giật mình khi đến gần; va chạm sẽ làm giật lùi xe và trừ **15% độ bền kiện hàng**.
- **Điều kiện Thắng / Thua & Đánh giá:**
  - **Thắng:** Đến điểm giao trước khi hết giờ và kiện hàng còn > 0%. Đánh giá 1 - 3 sao kèm tiền thưởng và điểm kinh nghiệm (EXP).
  - **Thua:** Hết thời gian hoặc kiện hàng bị vỡ nát (0%). Có nút bấm **[Thử Lại Ngay]** tiện lợi.
- **Tự động thiết lập trong Editor:**
  - Menu: `IUH Campus` &rarr; `Setup Grab Campus Minigame & Mini-Game Menu (One-Click)`.

