# Lượt 5 — Dọn tra cứu mỗi frame

19/11/2026, Windows Unity Editor 6000.5.8f1 (DX12), Unity MCP. Mốc trước sửa `599913b`, locator `e6aec9a`, gameplay `e1ffae8`.

## Các điểm A–F

| Điểm | Trước | Sau |
|---|---|---|
| A · Mobile controls | Thử tìm lại mỗi frame khi còn thiếu controller hoặc input | Giới hạn một lần mỗi 0,5 giây với thời gian unscaled. Giữ thứ tự tìm cũ, kể cả `PlayerArmature` và fallback theo controller. |
| B · Monster encounter | Mỗi quái tìm riêng mỗi frame khi thiếu người chơi | Dùng locator chung khi thử lại trong `Update`. Tra cứu trực tiếp lúc khởi tạo và khi bắt đầu tương tác. |
| C · Mascot quest | Tìm theo tên và tag mỗi frame khi chưa có người chơi | Dùng locator chung trong `Update`. Đường tìm ban đầu và fallback camera giữ nguyên. |
| D · Emergency supply crate | Tìm theo tên và tag mỗi frame khi chưa có người chơi | Dùng locator chung, vẫn ưu tiên Transform đã truyền khi tạo thùng. |
| E · Loot coin | Tìm người chơi khi thiếu và lấy Rigidbody mỗi frame trong tầm hút | Giới hạn tra cứu lặp bằng locator, cache Rigidbody trong `Awake`. Giữ tra cứu lúc tạo xu và fallback camera. |
| F · Monster preview | Lấy `IUHConceptPreviewController` mỗi frame | Lấy component cùng lúc gán Transform người chơi. |

## Test và chơi thử

| Toàn bộ test | Trước | Sau |
|---|---:|---:|
| EditMode | 115/115 | 115/115 |
| PlayMode | 45/45 | 47/47 |

Hai test mới đã chạy RED rồi GREEN. Test mobile gọi liên tiếp 10 lượt `Update` trong cùng khung giờ Unity và kiểm deadline tìm không đổi. `EncounterRangeTests` chạy lại và đạt cả hai ca. Job sau sửa là EditMode `3fff6f0635ed4458a899fcc4dcc0c0a6` và PlayMode `60a78fee60f8415b9f308d8e78cbb883`. Console 0 lỗi và Missing Script bằng 0 trong scene campus, AR và arena.

- Campus Play qua MCP: nhân vật di chuyển 6,44 m bằng input di chuyển. Xu loot thử nghiệm chạy qua đường hút và nhặt bình thường, cộng đúng 15 Gold.
- Bật `m_ForceShowOnPC` trong Play. Nút Tương tác mobile giữ cờ và không mở trận ở 8 m, rồi mở trận ở 4 m. Nút đó mở đúng luống 3 và nút nâng cấp mở cửa hàng. Tab kho đồ cũng mở được. Đã thử gửi phím E qua Input System và Windows nhưng Game View không xác nhận sự kiện phím. Kết quả 8/4 m xác nhận qua nút Tương tác dùng chung đường vào trận.
- Đã thoát Play, đối chiếu 128/128 khóa PlayerPrefs đã sao lưu. Scene campus sạch sau kiểm chứng.

## Profiler

Đo marker CPU `Update.ScriptRunBehaviourUpdate` trong Play bằng `ProfilerRecorder`, Deep Profile tắt. Hai đoạn khoảng 30 giây ghi trên campus khi nhân vật đứng yên, dùng save hiện có trước khi nạp checkpoint. Đây là số của toàn marker, gồm các hành vi khác trong scene. Mẫu không đo riêng tình huống liên tục thiếu người chơi và không cho thấy thời gian trung bình giảm.

| | Thời lượng | Số mẫu | Trung bình |
|---|---:|---:|---:|
| Trước | 30,00 giây | 5.201 | 0,3435 ms |
| Sau | 30,00 giây | 6.027 | 0,3720 ms |

Mở scene AR trong Editor để kiểm. Scene có 0 instance `IUHMobileControlsUI`, và lần liệt kê marker không có mục `IUHMobileControlsUI.Update`. Không có số so sánh cho marker này trong scene AR.

## Invalid AABB

Không tái hiện sau khoảng 13 phút 42 giây theo dõi Play. Bộ dò tạm qua `execute_code` nghe `Application.logMessageReceived`. Khi có AABB, bộ dò sẽ lưu stack trace, đường dẫn Transform có position/scale không hữu hạn và đường dẫn Renderer có bounds không hữu hạn. Không có log kích hoạt bộ dò. Listener đã được tháo khi thoát Play.

Đã đi qua mở đầu, checkpoint biến cố và ánh sáng đêm, hai trận Nhà E và Khu Thể thao, vườn, cửa hàng, kho đồ và checkpoint trùm. Các bàn thật nhận lần lượt 3, 3 và 4 nước đổi ô hợp lệ, sau đó dùng `Flee`. Các checkpoint đưa phiên kiểm đến đúng đoạn cần quan sát. Không có thay đổi renderer hay commit sửa AABB.
