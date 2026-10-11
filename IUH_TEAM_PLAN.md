# 📋 IUH Campus — Kế Hoạch 6 Minigames (Mỗi Thành Viên 2 Phân Cảnh)

> **Dự án:** 3D Virtual Campus & Multi-Minigame Hub — Đại Học Công Nghiệp TP. Hồ Chí Minh (IUH)  
> **Môn học:** New Technology and Application Development in IT (Phát triển ứng dụng và công nghệ mới trong CNTT)  
> **Nhóm thực hiện (3 thành viên):** Gia Thịnh (2 Scenes + Mobile) • Huy (2 Scenes + AR/AI/Docker) • Nhật (2 Scenes + 2D/Cloud)  
> **Mục tiêu điểm số:** **100 / 100 Base Score • 0 Điểm trừ Supporting Work** (Xem chi tiết tại [`requirement_mark_score.md`](./requirement_mark_score.md))  
> **Giao diện trực quan hóa HTML:** Mở file [`IUH_Plan_Dashboard.html`](./IUH_Plan_Dashboard.html) trên trình duyệt.

---

## 🏆 Ma Trận Đáp Ứng Rubric Chấm Điểm (100 / 100)

| Tiêu chí Rubric | Điểm | Giải pháp kỹ thuật triển khai trong dự án | Phụ trách chính |
|---|:---:|---|---|
| **1. Unity 2D & 3D implementation** | **20/20** | **3D:** Master Campus 3D URP, 3D Runner, FPV Drone 6DOF.<br>**2D:** Minigame 2D Tilemap Canteen Overcooked dùng Rigidbody2D, Sprite Animation. | Thịnh (3D)<br>Nhật (2D) |
| **2. AR implementation & Deep Learning model** | **20/20** | **AR:** AR Foundation quét thẻ SV chiếu Hologram 3D IUH.<br>**Deep Learning:** Model YOLOv8 / MobileNet ONNX nhận diện thẻ SV & logo. | Huy (AR & AI) |
| **3. Mobile deployment** | **20/20** | Xuất file **Android APK / AAB**, Virtual Touch Joystick cảm ứng, tối ưu 60 FPS. | Thịnh (Build Lead) |
| **4. IO cloud integration** | **20/20** | Tích hợp **Firebase / Supabase** lưu bảng vàng điểm số (Leaderboard) & Auth MSSV. | Nhật (Cloud Lead) |
| **5. Docker use** | **20/20** | Đóng gói Backend bằng **Docker & docker-compose.yml** chạy FastAPI + ONNX Runtime. | Huy (DevOps Lead) |
| **Supporting Work** | **-0 đ** | Đầy đủ 5 mục: Planning (Gantt), Testing (NUnit), Evidence (Architecture), Docs, Demo video. | Cả nhóm |

---

## 💡 Đột Phá Đề Tài: 2 Gợi Ý Để Làm Nổi Bật Thương Hiệu IUH

Để đề tài đồ án/trò chơi trường Đại học Công nghiệp TP.HCM thực sự gây ấn tượng mạnh với giảng viên và người chơi, 3 chủ đề mới được lựa chọn dựa trên 2 nét đặc trưng độc nhất:

1. **Xe Ôm Công Nghệ Sinh Viên (Grab / Be / Xanh SM):** Khắc họa chân thực cuộc sống mưu sinh và tinh thần tự lập của sinh viên IUH — chạy xe máy kiếm thêm học phí quanh cổng trường Nguyễn Văn Bảo và KTX.
2. **Gợi ý 1: Đua Robot & Đấu Trường Sumo Xưởng Cơ Khí 4.0 (IUH Robotics Arena):** Tôn vinh **thế mạnh kỹ thuật số 1 của trường Đại học Công nghiệp** (Cơ khí chế tạo, Điện tử, Tự động hóa, Vi mạch). Người chơi điều khiển xe robot dò line vượt mê cung và thi đấu Sumo đẩy đối thủ ra khỏi võ đài.
3. **Gợi ý 2: Cơn Lốc Canteen & Trà Đào Nguyễn Văn Bảo (Campus Cooking Rush - Overcooked Style):** Tái hiện **văn hóa ẩm thực nổi tiếng của sinh viên Gò Vấp** — xử lý hàng trăm đơn cơm tấm, bánh tráng nướng, trà đào trong 15 phút nghỉ giải lao giữa tiết học.

---

## 🏛️ Kiến Trúc Hệ Thống: 1 Hub + 6 Scenes

```
                                ┌──────────────────────────────────────────────┐
                                │      IUH_Campus_Main.unity (Sân Trường Hub)  │
                                │  - Sân trung tâm, Tượng đài & Cột cờ IUH     │
                                │  - Menu / Kiosk 3D chọn 6 minigames          │
                                └──────────────────────┬───────────────────────┘
                                                       │
         ┌─────────────────────────────────────────────┼─────────────────────────────────────────────┐
         │                                             │                                             │
         ▼                                             ▼                                             ▼
  ┌──────────────────────────────┐              ┌──────────────────────────────┐              ┌──────────────────────────────┐
  │      GIA THỊNH (2 Games)     │              │         HUY (2 Games)        │              │        NHẬT (2 Games)        │
  ├──────────────────────────────┤              ├──────────────────────────────┤              ├──────────────────────────────┤
  │ 🏃 Scene 1: Sprint To Class  │              │ 🚁 Scene 2: Drone Patrol     │              │ 🏍️ Scene 3: Parking Master   │
  │    (Chạy đua điểm danh 7 AM) │              │    (Bay tháp kính 13 tầng)   │              │    (Quản lý bãi xe cao điểm) │
  │                              │              │                              │              │                              │
  │ 🛵 Scene 4: Ride-Hailing Rush│              │ 🤖 Scene 5: RoboBattle Lab   │              │ 🍳 Scene 6: Canteen Rush     │
  │    (Xe ôm Grab/Be/Xanh SM)   │              │    (Đua robot xưởng cơ khí)  │              │    (Overcooked trà đào 15p)  │
  └──────────────────────────────┘              └──────────────────────────────┘              └──────────────────────────────┘
```

---

## 🎮 Chi Tiết 6 Phân Cảnh Minigame

### 👤 GIA THỊNH (2 Scenes)

#### Scene 1: `Assets/IUH_Campus/Scenes/Thinh/Scene_Thinh_SprintToClass.unity`
* **Lối chơi:** 3D Endless / Obstacle Runner & Parkour.
* **Bối cảnh:** Cổng chính Nguyễn Văn Bảo → Đường cờ hoa → Nhà Hiệu Bộ 5 tầng.
* **Cơ chế:** Chạy 3 làn (`A`/`D`), nhảy rào barie (`Space`), trượt vũng nước (`Ctrl`), nhặt Thẻ sinh viên IUH (+100đ) và Ly cà phê sữa đá Gò Vấp (tăng tốc độ 1.5x) để kịp giờ điểm danh 7:00 AM.
* **Scripts:** `ThinhPlayerRunner.cs`, `ThinhObstacleSpawner.cs`, `ThinhPickupBonus.cs`.

#### Scene 4: `Assets/IUH_Campus/Scenes/Thinh/Scene_Thinh_RideHailingRush.unity` *(MỚI)*
* **Tên trò chơi:** *Tài Xế Công Nghệ IUH: Chạy Xe Kiếm Tiền Học Phí*
* **Lối chơi:** Mô phỏng lái xe đón khách & giao đồ ăn góc nhìn thứ ba (phong cách *Crazy Taxi* / Mini Open Campus).
* **Bối cảnh:** Đại lộ trước cổng trường Nguyễn Văn Bảo, vòng xoay tượng đài, cổng phụ và đường hẻm sinh viên.
* **Cơ chế đặc sắc:**
  - **Tùy chọn 3 hãng xe nổi tiếng:**
    - 🟢 *GrabBike:* Xe máy xăng phổ thông, khả năng luồn lách tốt.
    - 🟡 *BeBike:* Xe tăng tốc nhanh, tiền tip cao khi giao đồ ăn nhanh.
    - 🔵 *Xanh SM:* Xe máy điện VinFast êm ái, thân thiện môi trường, không lo hết xăng.
  - Nhận cuốc đặt xe trên điện thoại thông minh (HUD góc màn hình), theo dõi định vị GPS trên Mini-map đón sinh viên trễ học hoặc giao trà sữa vào ký túc xá.
  - Né xe bus số 148, xe tải giao hàng, tích lũy đánh giá 5 sao ⭐ để thăng hạng tài xế công nghệ gương mẫu!
* **Scripts:** `ThinhMotorbikeController.cs`, `ThinhRideOrderManager.cs`, `ThinhMinimapGPS.cs`.

---

### 👤 HUY (2 Scenes)

#### Scene 2: `Assets/IUH_Campus/Scenes/Huy/Scene_Huy_DronePatrol.unity`
* **Lối chơi:** 3D FPV Drone Simulator 6DOF & Thời gian thực.
* **Bối cảnh:** Bao quanh Tháp Kính Xanh Ngọc 13 Tầng (Nhà B/E) cao 56m.
* **Cơ chế:** Bay xuyên các vòng neon Checkpoint trên không, quét laser mã QR tại ban công tầng 9–10, xịt bọt CO2 làm mát hộp điện tử và bay lên đỉnh mái vòm đen chụp ảnh toàn cảnh IUH.
* **Scripts:** `HuyDroneFlightController.cs`, `HuyRingCheckpoint.cs`, `HuyLaserScanner.cs`.

#### Scene 5: `Assets/IUH_Campus/Scenes/Huy/Scene_Huy_RoboBattleLab.unity` *(MỚI - GỢI Ý NỔI BẬT 1)*
* **Tên trò chơi:** *Đại Chiến Robot Tự Hành: Xưởng Kỹ Thuật 4.0 IUH*
* **Lối chơi:** Đua xe Robot dò vạch (Line-tracking) & Đấu trường đối kháng Robot Sumo.
* **Bối cảnh:** Tầng hầm phòng thí nghiệm Cơ điện tử & Trung tâm thực hành Công nghệ cao IUH.
* **Cơ chế đặc sắc:**
  - **Khẳng định thương hiệu trường Công nghiệp:** Sinh viên vận hành xe robot trang bị bánh xe Mecanum đa hướng, cánh tay kẹp linh kiện và cảm biến dò vạch hồng ngoại.
  - **Vòng 1 (Vượt xưởng máy):** Bám vạch đen trên sàn xưởng né chướng ngại vật bánh răng, máy tiện, máy CNC để về đích nhanh nhất.
  - **Vòng 2 (Robot Sumo Arena):** Đẩy robot của các đội thi khác ra khỏi vòng tròn đấu trường trên sân trường trung tâm!
* **Scripts:** `HuyRobotChassis.cs`, `HuyLineSensorTracker.cs`, `HuyArenaSumoJudge.cs`.

---

### 👤 NHẬT (2 Scenes)

#### Scene 3: `Assets/IUH_Campus/Scenes/Nhat/Scene_Nhat_ParkingMaster.unity`
* **Lối chơi:** Quản lý bãi đỗ xe & Giải đố thông xe kẹt (*Traffic Jam Puzzle*).
* **Bối cảnh:** Bãi giữ xe sinh viên IUH & Cổng phụ trạm barie.
* **Cơ chế:** Phân loại xe số vào ô xanh, xe tay ga vào ô vàng, quẹt thẻ RFID mở barie; giải đố trượt các xe máy cản đường để giải cứu xe cấp cứu/giảng viên kịp giờ.
* **Scripts:** `NhatParkingSlot.cs`, `NhatVehicleGridMover.cs`, `NhatTrafficFlowManager.cs`.

#### Scene 6: `Assets/IUH_Campus/Scenes/Nhat/Scene_Nhat_CanteenRush.unity` *(MỚI - GỢI Ý NỔI BẬT 2)*
* **Tên trò chơi:** *Cơn Lốc Canteen & Trà Đào Nguyễn Văn Bảo*
* **Lối chơi:** Time-Management Cooking / Phục Vụ Nhanh (phong cách *Overcooked*).
* **Bối cảnh:** Khu Canteen trường IUH và hẻm ăn vặt cổng trường giờ nghỉ giải lao 15 phút.
* **Cơ chế đặc sắc:**
  - **Món ăn đặc sản sinh viên Gò Vấp:** Cơm tấm sườn bì chả, bánh tráng nướng mỡ hành trứng cút, trà đào cam sả trân châu.
  - Hàng dài sinh viên ùa vào quầy order khi chuông reo. Người chơi phối hợp: lấy nguyên liệu → nấu nướng không để cháy khét → kết hợp đĩa thức ăn đúng order → bấm máy tính tiền.
  - Đạt chỉ tiêu phục vụ 30 sinh viên trong 15 phút để nhận điểm thưởng và tiền tip!
* **Scripts:** `NhatCookingStation.cs`, `NhatOrderQueueManager.cs`, `NhatStudentCustomerAI.cs`.

---

## 🛠️ Danh Sách 7 Scenes Cần Đăng Ký Trong Unity Build Settings

Mở Unity Editor: **File → Build Settings...** và kéo các scene theo thứ tự:

| Index | Đường Dẫn Scene | Phụ Trách | Minigame |
|---|---|---|---|
| **0** | `Assets/IUH_Campus/Scenes/IUH_Campus_Main.unity` | Cả nhóm | **Sân Trường Trung Tâm (Hub)** |
| **1** | `Assets/IUH_Campus/Scenes/Thinh/Scene_Thinh_SprintToClass.unity` | Gia Thịnh | 🏃 Game 1: Chạy điểm danh |
| **2** | `Assets/IUH_Campus/Scenes/Thinh/Scene_Thinh_RideHailingRush.unity` | Gia Thịnh | 🛵 Game 4: Xe ôm Grab/Be/Xanh |
| **3** | `Assets/IUH_Campus/Scenes/Huy/Scene_Huy_DronePatrol.unity` | Huy | 🚁 Game 2: Drone tháp kính |
| **4** | `Assets/IUH_Campus/Scenes/Huy/Scene_Huy_RoboBattleLab.unity` | Huy | 🤖 Game 5: Đua robot xưởng máy |
| **5** | `Assets/IUH_Campus/Scenes/Nhat/Scene_Nhat_ParkingMaster.unity` | Nhật | 🏍️ Game 3: Bãi đỗ xe kẹt |
| **6** | `Assets/IUH_Campus/Scenes/Nhat/Scene_Nhat_CanteenRush.unity` | Nhật | 🍳 Game 6: Canteen & Trà đào |

---

## 📅 Lộ Trình 4 Tuần Thực Hiện Cả 6 Minigames

* **Tuần 1 (Layout & Prefabs):** Mỗi bạn dựng khung cảnh 2 Scene của mình từ Prefab có sẵn của Master Campus; tạo cấu trúc thư mục riêng biệt.
* **Tuần 2 (Core Controllers):** Lập trình chuyển động nhân vật chạy, xe máy ôm công nghệ, drone bay 6DOF, bánh xe robot mecanum, kéo thả xe và trạm nấu ăn canteen.
* **Tuần 3 (Game Rules & Logic):** Hoàn thiện chướng ngại vật, hệ thống nhận cuốc Grab/Be/Xanh, quét laser tháp kính, đấu trường Robot Sumo, câu đố xe kẹt và hàng chờ gọi món sinh viên.
* **Tuần 4 (Ghép Hub & Đóng Gói):** Nối cả 6 scene vào script `IUHGameHubManager.cs` trên Scene chính; thêm âm thanh; đóng gói bản build thử nghiệm Windows/WebGL.
