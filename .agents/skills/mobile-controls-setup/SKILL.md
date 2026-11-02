---
name: mobile-controls-setup
description: Thiết lập, chẩn đoán và khắc phục hệ thống nút điều khiển cảm ứng (Virtual D-Pad, Joystick, Touch Buttons) và Canvas UI trên thiết bị di động cho game IUH Campus.
---

# Mobile Controls & Touch Diagnostics

Kỹ năng này cung cấp giải pháp xử lý triệt để lỗi không điều khiển được nhân vật trên điện thoại, xung đột sự kiện chạm giữa UI và nhân vật, và căn chỉnh giao diện theo chiều ngang.

## 1. Nguyên nhân phổ biến khiến Nút di động không bấm được
1. **EventSystem thiếu hoặc cấu hình sai Module**:
   - Nếu dự án dùng New Input System: `EventSystem` phải có `InputSystemUIInputModule`.
   - Nếu dự án dùng Legacy Input Manager: `EventSystem` phải dùng `StandaloneInputModule`.
2. **Raycast Target chặn tương tác**:
   - Một hình nền trong suốt (`Image` có alpha = 0 hoặc panel cha) đang bật `Raycast Target = true` phủ lên trên toàn màn hình, nuốt toàn bộ sự kiện chạm của các nút bên dưới.
   - **Giải pháp**: Tắt `Raycast Target` trên tất cả Background, Text, Panel trang trí; chỉ để `Raycast Target = true` trên chính xác nút bấm ảo.
3. **Thứ tự Render (Canvas Sorting Order / Hierarchy Order)**:
   - UI Menu nằm đè lên Virtual Controls hoặc Canvas điều khiển bị set `enabled = false` khi chuyển scene.

## 2. Kiến trúc Virtual Button chuẩn (EventSystem Interfaces)
Không phụ thuộc vào `Update() Input.touchCount`, mà sử dụng trực tiếp các interface sự kiện con trỏ:
```csharp
using UnityEngine;
using UnityEngine.EventSystems;

namespace IUHCampus.Controls
{
    public class MobileMoveButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Vector2 moveDirection;
        public bool IsPressed { get; private set; }

        public void OnPointerDown(PointerEventData eventData) => IsPressed = true;
        public void OnPointerUp(PointerEventData eventData) => IsPressed = false;

        public Vector2 GetDirection() => IsPressed ? moveDirection : Vector2.zero;
    }
}
```

## 3. Kết nối Nút ảo với Bộ điều khiển Nhân vật (Character Controller)
Bộ điều khiển nhân vật nhận đầu vào kết hợp từ cả Bàn phím (PC/Mac) và Nút ảo (Mobile):
```csharp
float horizontal = Input.GetAxisRaw("Horizontal") + virtualHorizontalInput;
float vertical = Input.GetAxisRaw("Vertical") + virtualVerticalInput;
Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
```

## 4. Căn chỉnh Canvas Scaler cho Điện thoại
Để tránh nút bấm bị co rúm hoặc tràn khỏi màn hình điện thoại (tỉ lệ 19.5:9, 20:9):
- Canvas Scaler:
  - `UI Scale Mode`: `Scale With Screen Size`
  - `Reference Resolution`: `1920 x 1080` (hoặc `2400 x 1080`)
  - `Screen Match Mode`: `Match Width Or Height`
  - `Match`: `0.5` (cân bằng giữa chiều rộng và chiều cao).

