# 🏫 IUH_Campus — Virtual Campus of Industrial University of Ho Chi Minh City

<p align="center">
  <img src="https://img.shields.io/badge/Unity-6.0%2F6.5-black?logo=unity&logoColor=white" />
  <img src="https://img.shields.io/badge/Render_Pipeline-URP-blue?logo=unity" />
  <img src="https://img.shields.io/badge/Language-C%23-239120?logo=csharp" />
  <img src="https://img.shields.io/badge/Platform-Windows%20%7C%20macOS%20%7C%20VR-lightgrey" />
  <img src="https://img.shields.io/badge/License-MIT-green" />
</p>

<p align="center">
  Dự án 3D khuôn viên ảo của <b>Trường Đại học Công nghiệp TP. Hồ Chí Minh (IUH)</b>, được xây dựng hoàn toàn bằng Unity 6 sử dụng các script Editor tùy chỉnh — không cần asset 3D bên ngoài.
</p>

---

## 🏗️ Tổng quan kiến trúc

Toàn bộ khuôn viên được **tạo thủ tục (procedurally generated)** từ các script C# Editor. Tất cả geometry được xây từ các primitive của Unity (`Cube`, `Cylinder`, `Sphere`) ghép thành các công trình kiến trúc thực tế.

```
IUH_Campus (Scene Root)
├── Buildings/
│   ├── IUH_MainBuilding          ← Nhà Hiệu Bộ (5 tầng, kính curtain wall, fins xanh)
│   ├── IUH_BackCentralTower      ← Tháp trung tâm phía sau
│   ├── IUH_GreenGlassComplex/
│   │   ├── IUH_GreenGlassTower   ← Tháp kính xanh ngọc 13 tầng (Nhà B/E)
│   │   ├── IUH_LeftPaleWing      ← Dãy nhà học thuật hình chữ L bên trái
│   │   └── IUH_RedRoofBuilding   ← Khối nhà phía sau (mái kim loại đỏ)
│   └── IUH_RightLargeAcademicBuilding ← Khối học thuật trắng lớn bên phải
├── CentralCourtyard/             ← Sân trung tâm
├── Landscaping/
│   ├── IUH_FrontMonument         ← Tượng đài đá
│   └── IUH_FlagPoles             ← 3 cột cờ (Việt Nam, IUH, Đoàn Thanh Niên)
├── IUH_MainEntrance_Zone/
│   └── IUH_MainEntrance/
│       ├── Entrance_Road          ← Đường 2 làn + lề đường
│       ├── Gate_Pillars           ← 2 trụ cổng đá với bảng vàng IUH
│       ├── Overhead_Welcome_Banner← Băng rôn đỏ treo giữa 2 trụ cổng
│       ├── University_Monument_Slab← Tấm bia đá trường bên trái
│       ├── Security_Booth         ← Phòng bảo vệ bên phải
│       ├── Right/Left_Blue_Railing← Hàng rào kim loại xanh 2 bên
│       ├── Event_Flag_Line        ← Cờ trang trí nhiều màu 2 bên đường
│       └── Entrance_Trees         ← Cây xanh dọc lối vào
├── Parking/                      ← Bãi gửi xe
├── BackgroundCity/               ← Khối đô thị xung quanh
└── Lighting/                     ← Ánh sáng + cameras
```

---

## 🛠️ Công nghệ sử dụng

| Thành phần | Chi tiết |
|-----------|--------|
| **Engine** | Unity 6.0 / 6.5 |
| **Render Pipeline** | Universal Render Pipeline (URP) |
| **Ngôn ngữ** | C# (Editor-only scripts) |
| **Tạo asset** | 100% procedural — không dùng model 3D ngoài |
| **Tạo texture** | `IUHTextureGenerator` (vẽ pixel bằng C#) |
| **Materials** | 39+ vật liệu URP Lit tùy chỉnh qua `IUHMaterialGenerator` |
| **Build scene** | One-click menu `IUH Campus → Build Complete Master Campus` |
| **Mục tiêu** | Desktop + VR (BoxColliders trên toàn bộ công trình) |

---

## 📂 Cấu trúc project

```
IUH_Campus/
├── Assets/
│   └── IUH_Campus/
│       ├── Editor/
│       │   ├── IUHCampusMasterBuilder.cs   ← Lắp ráp toàn bộ scene
│       │   ├── IUHMaterialGenerator.cs     ← Tạo 39+ vật liệu URP
│       │   ├── IUHTextureGenerator.cs      ← Vẽ texture thủ tục
│       │   └── IUHBuildingBuilder.cs       ← Hàm hỗ trợ tạo tòa nhà
│       ├── Materials/                      ← File .mat tự động tạo
│       ├── Prefabs/
│       │   ├── Architecture/               ← Prefab tòa nhà
│       │   └── Environment/                ← Prefab cây, cờ, cổng
│       ├── Scenes/
│       │   └── IUH_Campus_Main.unity
│       └── Textures/                       ← Texture PNG tự động tạo
├── Packages/
└── README.md
```

---

## 🚀 Hướng dẫn cài đặt

### Yêu cầu
- Unity **6.0** hoặc **6.5** (khuyến nghị dùng LTS)
- Package Universal Render Pipeline đã được cài
- macOS hoặc Windows

### Các bước

1. **Clone repository**
   ```bash
   git clone https://github.com/GiaThinh110605/IUH_Campus.git
   ```

2. **Mở trong Unity Hub**
   - Nhấn **Add** → chọn thư mục vừa clone
   - Mở với Unity 6.x

3. **Build khuôn viên**
   - Trong Unity Editor, nhấn vào menu:
     ```
     IUH Campus → Build Complete Master Campus
     ```
   - Quá trình build sẽ tự động:
     - Tạo tất cả textures → `Assets/IUH_Campus/Textures/`
     - Tạo tất cả materials → `Assets/IUH_Campus/Materials/`
     - Tạo tất cả prefab tòa nhà → `Assets/IUH_Campus/Prefabs/`
     - Lắp ráp scene hoàn chỉnh → `Assets/IUH_Campus/Scenes/IUH_Campus_Main.unity`

4. **Nhấn Play** hoặc chuyển sang Scene View để khám phá khuôn viên.

---

## 🏛️ Các công trình chính

### 🏢 IUH_MainBuilding (Nhà Hiệu Bộ)
- **5 tầng**, rộng 44m × cao 22m
- Mặt tiền kính curtain wall toàn bộ 2 tầng dưới
- Fins kiến trúc xanh dọc 3 tầng trên
- Biển hiệu trung tâm với logo IUH
- Cầu thang granit đen và mái che lối vào
- Cửa kính với tay cầm inox

### 🟢 IUH_GreenGlassTower (Nhà B/E)
- **13 tầng**, cao 56m
- Kính góc màu xanh ngọc đặc trưng
- Mái vòm đen đỉnh tòa nhà
- Ban công quan sát tầng 9–10

### 🏫 IUH_RightLargeAcademicBuilding
- Phức hợp học thuật 3 khối (trước, sau, cánh nối)
- Lưới cửa sổ, khung turquoise
- Biển hiệu tên trường trên mái

### 🚪 IUH_MainEntrance (Cổng Trường IUH)
- Kiểu lối đi mở (khớp với ảnh cổng trường thực tế)
- **Trụ cổng đá** với bảng chữ vàng IUH
- **Băng rôn đỏ** treo giữa 2 trụ cổng
- **Bia đá đặt tên trường** bên trái
- **Phòng bảo vệ** bên phải
- **Hàng rào kim loại xanh** dọc 2 bên
- **Cờ trang trí nhiều màu** trên cột nhỏ 2 bên đường
- **Cây xanh** dọc lối vào

---

## 🎨 Materials & Textures

Tất cả materials được tạo tự động với URP Lit shader và các giá trị PBR được tinh chỉnh kỹ lưỡng:

| Danh mục | Vật liệu |
|----------|-----------|
| Facade | WhiteFacade, BlueFins, BlueFacade, GreenGlass |
| Kính | Glass_Reflective, Glass_CurtainWall |
| Kim loại | DarkMetal, LightMetal, RooftopMetal |
| Mặt đất | Asphalt_Road, Concrete_Ground, Granite_Stairs, Grass |
| Cờ | Flag_Vietnam 🇻🇳, Flag_IUH, Flag_YouthUnion |
| Cổng trường | Gate_Stone, Gate_BlueRailing, Gate_WelcomeBanner, Gate_Booth, Gate_GoldText |
| Biển hiệu | Sign_CurtainWall, Academic_TopSign, Logo_Crest |

---

## 📷 Cameras có sẵn

| Camera | Vị trí | Mô tả |
|--------|----------|-------------|
| `Main_Camera` | Góc cao toàn cảnh | Camera mặc định (bật) |
| `Camera_Courtyard_Ground` | Mặt đất, nhìn vào nhà chính | Góc nhìn sân trường |
| `Camera_Front_Monument_CloseUp` | Gần mặt đất, nhìn tượng đài | Cận cảnh tượng đài |
| `Camera_LookAt_GreenTower` | Mặt đất, nhìn trái | Góc nhìn tháp xanh |
| `Camera_LookAt_RightBuilding` | Mặt đất, nhìn phải | Góc nhìn tòa nhà phải |

---

## 👨‍💻 Tác giả

- **Gia Thịnh** — Unity Development, Kiến trúc thủ tục

---

## 🏫 Về IUH

**Trường Đại học Công nghiệp Thành phố Hồ Chí Minh (IUH)**

- 🌐 Website: [iuh.edu.vn](https://iuh.edu.vn)
- 📍 Địa chỉ: 12 Nguyễn Văn Bảo, Phường 4, Gò Vấp, TP. HCM

---

## 📄 Giấy phép

Dự án này được cấp phép theo **MIT License** — xem file [LICENSE](LICENSE) để biết thêm chi tiết.

---

<p align="center">Made with ❤️ for IUH — <i>Đại học Công nghiệp TP. Hồ Chí Minh</i></p>
