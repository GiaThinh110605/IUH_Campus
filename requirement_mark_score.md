# 🏆 MASTER PLAN: ĐẠT ĐIỂM TỐI ĐA RUBRIC (100/100)
## Môn học: New Technology and Application Development in IT (Công nghệ mới và Phát triển ứng dụng trong CNTT)
### Dự án: IUH Campus — 3D/2D Metaverse, AR Image AI, Cloud IoT & Mobile App
**Nhóm sinh viên thực hiện (3 thành viên):** Gia Thịnh • Huy • Nhật

---

## 📊 1. BẢNG TỔNG QUAN PHÂN BỔ ĐIỂM THEO RUBRIC

| Thành phần đánh giá (Rubric Component) | Điểm tối đa | Tính năng kỹ thuật thực hiện trong dự án | Thành viên phụ trách chính |
|---|:---:|---|---|
| **1. Unity 2D and 3D implementation** | **20 / 20** | - **3D:** Khuôn viên ảo 3D IUH (Nhà Hiệu Bộ, Tháp Kính 13T), 3D Runner, FPV Drone 6DOF.<br>- **2D:** Minigame 2D Pixel Canteen Rush dùng Unity 2D Engine (Tilemap, Rigidbody2D, Sprite Animation). | **Gia Thịnh** (3D Campus & Runner)<br>**Nhật** (2D Canteen Engine) |
| **2. AR implementation & Deep Learning Model** | **20 / 20** | - **AR:** Unity AR Foundation (ARCore), quét thẻ sinh viên / mặt bàn chiếu Hologram 3D IUH.<br>- **Deep Learning:** Model AI (YOLOv8-nano / MobileNet ONNX) nhận diện khuôn mặt / thẻ SV IUH / tòa nhà. | **Huy** (AR Foundation & Unity Sentis / ONNX Inference) |
| **3. Mobile deployment** | **20 / 20** | - Xuất file **Android APK / AAB** hoàn chỉnh.<br>- Tối ưu URP Mobile, UI cảm ứng (Touch Virtual Joystick, Pinch to Zoom, Haptic Feedback). | **Gia Thịnh** (Mobile Build Pipeline & Touch Input) |
| **4. IO Cloud integration** | **20 / 20** | - Tích hợp **Firebase / Supabase / MQTT Adafruit IO**.<br>- Đồng bộ thời gian thực: Cloud Leaderboard (bảng vàng điểm số), Auth tài khoản SV, telemetry dữ liệu bãi xe IoT. | **Nhật** (Cloud SDK, REST API Client, Realtime DB) |
| **5. Docker use** | **20 / 20** | - Đóng gói Backend Microservice bằng **Docker & Docker Compose**.<br>- Container chạy FastAPI + ONNX Runtime AI Server phục vụ suy luận hình ảnh cho Mobile. | **Huy** (Dockerfile, docker-compose.yml, Cloud AI API) |
| **TỔNG ĐIỂM CƠ BẢN (Base Score)** | **100 / 100** | **ĐÁP ỨNG TOÀN DIỆN 5/5 TIÊU CHÍ KỸ THUẬT CỐT LÕI** | **CẢ NHÓM** |
| **Supporting Work (Deduction Avoidance)** | **0 (Không bị trừ)** | Đầy đủ 5 hạng mục: Planning, Testing, Evidence of Integration, Documentation, Demonstration (Video Demo). | **CẢ NHÓM** |
| **ĐIỂM TỔNG KẾT DỰ KIẾN (Final Score)** | **100 / 100** | **ĐẠT ĐIỂM XUẤT SẮC TUYỆT ĐỐI** | 🏆 |

---

## 🛠️ 2. KẾ HOẠCH CHI TIẾT THEO TỪNG TIÊU CHÍ RUBRIC

```
                              ┌─────────────────────────────────────────────────────────┐
                              │                    IUH CAMPUS APP                       │
                              │             (Unity 6 Mobile - Android APK)              │
                              └────────────┬───────────────────────────────┬────────────┘
                                           │                               │
                 ┌─────────────────────────┴──────────┐       ┌────────────┴────────────────────────┐
                 ▼                                    ▼       ▼                                     ▼
     ┌───────────────────────┐   ┌───────────────────────┐ ┌───────────────────────┐   ┌───────────────────────┐
     │      UNITY 3D         │   │       UNITY 2D        │ │     AR FOUNDATION     │   │      DEEP LEARNING    │
     │  - Master Campus 3D   │   │  - 2D Canteen Rush    │ │  - AR Tabletop Portal │   │  - YOLOv8 / MobileNet │
     │  - FPV Drone & Runner │   │  - Tilemap & Sprite2D │ │  - Image Tracking AR  │   │  - Nhận diện thẻ SV   │
     └───────────────────────┘   └───────────────────────┘ └───────────────────────┘   └───────────┬───────────┘
                                                                                                   │ HTTP / gRPC
                                           ┌───────────────────────────────────────────────────────┘
                                           ▼
                              ┌─────────────────────────────────────────────────────────┐
                              │                    DOCKER CONTAINER                     │
                              │           (FastAPI + ONNX Runtime AI Backend)           │
                              └───────────────────────────┬─────────────────────────────┘
                                                          │ Cloud Sync
                                                          ▼
                              ┌─────────────────────────────────────────────────────────┐
                              │                   IO CLOUD INTEGRATION                  │
                              │        (Firebase Realtime DB / Supabase Leaderboard)    │
                              └─────────────────────────────────────────────────────────┘
```

---

### TIÊU CHÍ 1: Unity 2D and 3D Implementation (20/20 Điểm)

* **Phần 3D (Đã có sẵn & mở rộng):**
  - Khuôn viên 3D đầy đủ: Nhà Hiệu Bộ, Tháp Kính Xanh 13 tầng, Cổng trường, Sân cột cờ.
  - Phân cảnh 3D Runner (`Scene_Thinh_SprintToClass`), 3D FPV Drone (`Scene_Huy_DronePatrol`), 3D Bãi xe (`Scene_Nhat_ParkingMaster`).
  - Ánh sáng PBR URP Lit, bóng đổ thời gian thực, hệ thống Camera đa góc nhìn.
* **Phần 2D (Bổ sung để lấy trọn 20 điểm 2D):**
  - **Tên Scene:** `Assets/IUH_Campus/Scenes/Nhat/Scene_2D_CanteenOvercooked.unity`.
  - **Công nghệ Unity 2D thuần túy:**
    - Sử dụng `Tilemap` & `Grid` để vẽ bản đồ nhà ăn 2D phong cách Retro 16-bit.
    - Nhân vật dùng `Rigidbody2D`, `BoxCollider2D`, và `SpriteRenderer`.
    - Hệ thống Animation 2D (Walk, Cook, Serve) với Sprite Sheet sinh viên.
    - Giao diện 2D Pixel UI, cơ chế kéo thả nguyên liệu 2D physics.

---

### TIÊU CHÍ 2: AR Implementation and Deep Learning Model (20/20 Điểm)

* **Phần AR Implementation:**
  - **Package:** `Unity AR Foundation 6.x` + `Google ARCore XR Plugin`.
  - **Tên Scene:** `Assets/IUH_Campus/Scenes/Huy/Scene_AR_CampusHologram.unity`.
  - **2 Chế độ AR thực tế:**
    1. **AR Image Tracking:** Hướng camera điện thoại vào Logo IUH hoặc Thẻ sinh viên IUH thực tế $\rightarrow$ Một mô hình Tháp Kính Xanh thu nhỏ 3D xuất hiện ngay trên tấm thẻ (Interactive AR Hologram).
    2. **AR Plane Detection:** Quét mặt phẳng sàn nhà/bàn học $\rightarrow$ Chiếu toàn bộ sa bàn 3D khuôn viên trường IUH lên mặt bàn để người dùng đi vòng quanh khám phá.
* **Phần Deep Learning Model:**
  - **Mô hình AI:** Sử dụng **YOLOv8-nano** hoặc **MobileNetV3** (đã convert sang định dạng `.onnx`).
  - **Nhiệm vụ của mô hình:**
    - Nhận diện Thẻ Sinh Viên IUH (`IUH_Student_Card`) và phân loại Khoa viện.
    - Hoặc nhận diện Cử chỉ bàn tay (Hand Gesture: Like, Victory, Open Palm) để điều khiển bay Drone mà không cần chạm màn hình.
  - **Triển khai:**
    - Chạy trực tiếp on-device bằng **Unity Sentis** (chạy ONNX không cần mạng), HOẶC gửi frame ảnh lên Docker Server qua REST API để nhận bounding box và nhãn nhận diện.

---

### TIÊU CHÍ 3: Mobile Deployment (20/20 Điểm)

* **Nền tảng mục tiêu:** Android OS (Hỗ trợ Android 10 trở lên, API Level 29+).
* **Quy trình đóng gói (Build Artifact):**
  - File cài đặt: `IUH_Campus_v1.0.apk`.
  - Cấu hình Unity Build Settings: Target Platform `Android`, Scripting Backend `IL2CPP`, Target Architectures `ARM64`.
* **Tối ưu hóa chuyên sâu cho Mobile:**
  - Giao diện cảm ứng toàn diện: On-screen Touch Joystick (`StarterAssetsInputs`), nút bấm to rõ, hỗ trợ đa điểm (Multi-touch).
  - Tối ưu hiệu năng: Độ phân giải động (Dynamic Resolution), nén texture ASTC 6x6, URP Mobile Forward Renderer đạt chuẩn mượt mà **60 FPS** trên thiết bị thật.
  - Quyền camera Android (`android.permission.CAMERA`) được cấp tự động khi mở chế độ AR.

---

### TIÊU CHÍ 4: IO Cloud Integration (20/20 Điểm)

* **Nền tảng Cloud:** **Firebase (Google Cloud)** hoặc **Supabase REST API** hoặc **Adafruit IO MQTT**.
* **Các tính năng tích hợp Cloud thời gian thực:**
  1. **Cloud Leaderboard (Bảng vàng thành tích):** Điểm số chơi game của Thịnh, Huy, Nhật được đẩy lên Cloud Database (Firebase Firestore / Realtime DB) và tải về hiển thị top 10 kỷ lục toàn trường.
  2. **Student Authentication (Đăng nhập):** Sinh viên nhập Mã số sinh viên (MSSV) $\rightarrow$ Cloud xác thực và tải về thông tin avatar, khoa ngành.
  3. **IoT Smart Parking Cloud Stream:** Đồng bộ số lượng chỗ trống trong bãi xe IUH lên Cloud Feed thời gian thực (mô phỏng trạm IoT đo lưu lượng xe).

---

### TIÊU CHÍ 5: Docker Use (20/20 Điểm)

* **Thư mục Backend:** `Server/` nằm ngay trong repository.
* **Cấu trúc Container:**
  - `Dockerfile`: Xây dựng môi trường Linux Alpine/Ubuntu tối nhẹ với Python 3.10, FastAPI, Uvicorn, ONNX Runtime / PyTorch CPU.
  - `docker-compose.yml`: Khởi chạy đồng bộ 2 services:
    1. **ai-inference-service:** API nhận diện hình ảnh Deep Learning (Endpoint `/predict/student-card`, `/predict/landmark`).
    2. **cloud-broker-service:** Trạm trung chuyển dữ liệu Cloud và đồng bộ Socket.
* **Cách chạy chỉ bằng 1 dòng lệnh:**
  ```bash
  cd Server && docker compose up --build -d
  ```
* **Bằng chứng kiểm thử (Evidence):** Cung cấp tài liệu Swagger UI (`http://localhost:8000/docs`) và ảnh chụp `docker ps` đang hoạt động.

---

## 🛡️ 3. KẾ HOẠCH "SUPPORTING WORK" (TRÁNH BỊ TRỪ 20 ĐIỂM)

> ⚠️ **Quy định Rubric:** *"If this supporting work is missing, deduct 20 points once from the base score..."*  
> Để bảo toàn 100 điểm, nhóm phải có đủ **5 tài liệu & bằng chứng** sau:

### 1. Planning (Kế hoạch phát triển)
- Lập bảng phân công chi tiết 3 thành viên theo Sprint (Gantt chart trong Dashboard HTML).
- Biên bản họp phân chia nhánh Git, quản lý issue trên GitHub Projects.

### 2. Testing (Kiểm thử phần mềm)
- **Unit Testing trong Unity:** Dùng Unity Test Runner (C# NUnit) viết kịch bản test cho `ScoreCalculationTests.cs`, `MovementTests.cs`.
- **API Testing:** File test Postman Collection / cURL script kiểm tra độ trễ server Docker AI dưới 100ms.
- **Mobile Hardware Testing Report:** Bảng log kiểm thử trên ít nhất 2 thiết bị Android thật (đo FPS, nhiệt độ, mức chiếm RAM < 500MB).

### 3. Evidence of Integration (Bằng chứng tích hợp)
- Sơ đồ kiến trúc luồng dữ liệu (Architecture Diagram) kết nối 5 thành phần.
- Ảnh chụp màn hình Logcat Android gửi request thành công lên Docker Server và nhận phản hồi từ Firebase Cloud.

### 4. Documentation (Tài liệu kỹ thuật)
- File `README.md` hướng dẫn từ A-Z cách clone, mở Unity, chạy Docker, cài đặt APK.
- File `API_DOCUMENTATION.md` đặc tả các Endpoint REST API.

### 5. Demonstration (Video & Thuyết trình)
- Kịch bản quay video demo 5 phút phân đoạn rõ ràng:
  - Phút 0-1: Khuôn viên 3D & Chạy minigame 3D + 2D.
  - Phút 1-2: Hướng camera quét thẻ SV hiện AR Hologram & nhận diện AI.
  - Phút 2-3: Thao tác trực tiếp trên điện thoại Android (Mobile touch).
  - Phút 3-4: Bảng điểm cập nhật thời gian thực trên Cloud Database.
  - Phút 4-5: Mở Terminal chứng minh Docker container đang chạy và xử lý request.

---

## 👥 4. PHÂN CHIA TRÁCH NHIỆM CHO 3 THÀNH VIÊN

| Thành viên | Trách nhiệm chính (Lead Module) | Deliverables cụ thể |
|---|---|---|
| **GIA THỊNH** | **Unity 3D/Mobile Lead & Core Runner** | - Master Scene 3D + 3D Runner + Game Xe Ôm.<br>- Cấu hình Mobile Android Touch Input, Build APK.<br>- Đảm bảo hiệu năng Mobile đạt 60 FPS. |
| **HUY** | **AR, AI Deep Learning & Docker Lead** | - Tích hợp AR Foundation (Image Tracking & Plane Hologram).<br>- Huấn luyện/Convert mô hình Deep Learning ONNX.<br>- Viết Dockerfile, docker-compose.yml và dựng server FastAPI. |
| **NHẬT** | **Unity 2D, IO Cloud & QA/Testing Lead** | - Dựng Minigame 2D Tilemap Canteen Overcooked.<br>- Tích hợp Cloud Database (Firebase Leaderboard & Auth).<br>- Viết Unit Test NUnit, lập bảng báo cáo kiểm thử (Testing Report). |

---

## 📅 5. LỘ TRÌNH THỰC HIỆN 4 TUẦN ĐỂ ĐẠT 100 ĐIỂM

```
Tuần 1: Dựng Khung & Docker AI
├── Thịnh: Dựng layout 3D, thiết lập dự án Android Platform trên Unity
├── Huy: Viết Dockerfile, chạy server FastAPI, chuẩn bị model ONNX
└── Nhật: Thiết lập Firebase Cloud Project, dựng Tilemap 2D Canteen

Tuần 2: Hoàn Thiện Gameplay & AR
├── Thịnh: Hoàn thành 3D Runner + Xe Ôm Grab/Be/Xanh Mobile Touch
├── Huy: Tích hợp AR Foundation camera, nhận diện logo IUH
└── Nhật: Lập trình cơ chế 2D Physics Canteen, kết nối Cloud Leaderboard

Tuần 3: Tích Hợp Toàn Diện (Integration)
├── Huy + Thịnh: Nối Unity Camera gửi ảnh sang Docker AI nhận diện thẻ SV
├── Nhật: Viết Unity Test Runner (Unit Test) và kiểm tra Cloud API
└── Cả nhóm: Ghép tất cả vào Main Hub Kiosk 3D chuyển scene

Tuần 4: Đóng Gói, Báo Cáo & Quay Video Demo
├── Thịnh: Xuất file Android APK hoàn chỉnh, test trên thiết bị thật
├── Huy: Chốt Docker image, chụp ảnh bằng chứng kiểm thử Server
├── Nhật: Hoàn thiện tài liệu báo cáo Testing Report & Evidence Docs
└── Cả nhóm: Quay video Demonstration 5 phút thuyết minh đạt chuẩn 100/100
```