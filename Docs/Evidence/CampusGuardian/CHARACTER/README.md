# Lượt B — Nhân vật Casual2 và cửa hàng hai tab

Kiểm chứng 20–21/11/2026 bằng Unity Editor 6000.5.8f1 trên Windows, URP. PR #45 đã nhập Task 1–2 vào main. Task 3–4 nằm trên nhánh `minhnhat/remove-cosmetics`, từ `0cf0576`.

## Nhân vật và animation

Avatar `Casual2Avatar` hợp lệ, gồm bốn skinned mesh. Áo `#F2F2EE`, quần `#1F2A44`. `Campus Student` được unpack trong scene để thay rig tại chỗ. `Animator` trên gốc vẫn dùng controller Starter Assets và giữ thiết lập root motion. `CharacterArmature` cùng bốn mesh nằm trực tiếp dưới gốc, với scale 0,94 lần model nhập. Giữ capsule 1,80 m và camera target y = 1,45 m.

Cụm súng và điểm bắn được gắn sang `Hand.R` để giữ liên kết gameplay. Cánh bay được giữ vì `IUHAerialFlightController` tham chiếu chúng. Khi unpack, giữ định danh serialized của gốc và `PlayerCameraRoot` để camera và các tham chiếu từ ngoài subtree vẫn liên kết được.

| Tư thế | Góc 3/4 trước | Sau lưng |
|---|---|---|
| Đứng | [Trước](idle-front-before.png) · [Sau](idle-front-after.png) | [Trước](idle-back-before.png) · [Sau](idle-back-after.png) |
| Đi | [Trước](walk-front-before.png) · [Sau](walk-front-after.png) | [Trước](walk-back-before.png) · [Sau](walk-back-after.png) |
| Chạy | [Trước](run-front-before.png) · [Sau](run-front-after.png) | [Trước](run-back-before.png) · [Sau](run-back-after.png) |
| Nhảy | [Trước](jump-front-before.png) · [Sau](jump-front-after.png) | [Trước](jump-back-before.png) · [Sau](jump-back-after.png) |

Ảnh tư thế lấy mẫu animation tại một mốc cố định. Riêng ảnh nhảy là mẫu clip tại vị trí camera cố định, không biểu diễn quỹ đạo root trong gameplay. Lượt chơi thử tách riêng dùng `StarterAssetsInputs` và controller thực. Xem [tọa độ camera](cameras.json).

| Kiểm tra trong Play | Quan sát |
|---|---|
| Đi và chạy | Đã thấy cả hai dải tốc độ của Animator. Lượt đo đi tiến 5,78 m. |
| Nhảy, trên không, tiếp đất | `JumpStart`, trạng thái trên không và `JumpLand` đều xuất hiện. Độ cao gốc đỉnh khoảng 1,99 m trong fixture và trở về grounded. |
| Sự kiện bước chân và tiếp đất | Không có lỗi Console khi chạy chu kỳ di chuyển và tiếp đất. Chưa kiểm âm thanh bằng nghe trực tiếp. |
| Arena | Đã vào trận Nhà E qua Tương tác. `Arena_Student` có đủ bốn mesh, chân đứng trên mặt arena. Đã thoát bằng Flee. |
| Vườn | Mở đúng bảng luống 3 trong Play. |

[Arena trước](arena-before.png) · [Arena sau](arena-after.png). Vật thể màu magenta ở phía quái đã có trong ảnh trước khi thay nhân vật.

## Cửa hàng

[Cửa hàng cũ](shop-before.png). Hai nút còn lại có độ rộng bằng nhau và co giãn theo khung cửa hàng. Đã bấm từng nút và xác nhận đúng panel active.

| Khung Game View | Nâng cấp | Kho đồ |
|---|---|---|
| PC 1920×1080 | [Ảnh](shop-pc-upgrades.png) | [Ảnh](shop-pc-inventory.png) |
| Điện thoại ngang 1170×540, mô phỏng tỉ lệ trong Editor | [Ảnh](shop-mobile-upgrades.png) | [Ảnh](shop-mobile-inventory.png) |

Khung điện thoại đo bố cục Game View, chưa chạy trên thiết bị thật. Đã khôi phục preset gốc của Game View và điểm lưu cốt truyện.

## Hiệu năng góc sảnh

Mỗi mốc 20 giây, cùng camera và vị trí người chơi, dùng UnityStats trong Editor.

| Mốc | FPS | Draw calls | Tris |
|---|---:|---:|---:|
| Model cũ | 238,02 | 350,0 | 1.940.007 |
| Sau thay `Casual2` | 226,55 | 356,7 | 1.805.432 |

FPS giảm 4,8%, dưới ngưỡng dừng 10%.

## Thành phần đã gỡ

- Task 2 thay `Campus Student/Geometry` và `Campus Student/Skeleton` cũ, gồm `Armature_Mesh`, `Student head`, `Student hair`, `Student backpack`, `GRADUATION_MORTARBOARD_HAT` và hai trail ở chân.
- Task 3 xóa `Campus Student/CYBER_AURA_LIGHT`, component và script `IUHPlayerCustomizer`, `BtnTabSkins` và `Tab_Skins` cùng các con của hai object UI. Gỡ phần nối cửa hàng và builder legacy tới trang phục.
- Quét repo không còn `IUHPlayerCustomizer`, `PlayerSkinType` hay `PlayerTrailType` ngoài `Docs`. Không áp dụng hoàn vàng hay đọc lại lựa chọn ngoại trang cũ.

## Kiểm thử

| Task | Commit | EditMode | PlayMode |
|---|---|---:|---:|
| 1 — nhập model | `aa41b13` | 124/124 | 47/47 |
| 2 — thay player | `9aa2b6a` | 124/124 | 47/47 |
| 3 — gỡ trang phục | `61773ee` | 124/124 | 48/48 |
| 4 — bằng chứng cuối | Mốc tài liệu này | 124/124 | 48/48 |

Task 3 chạy riêng EditMode `86c18810c9f84ac0b437a9ab6eb7f537` và PlayMode `75bbe4284322430190fb977fedef9b8c`. Missing Script bằng 0 ở campus, arena và AR. `UpgradeShopTabsTests` xác nhận mở cửa hàng, chuyển qua lại giữa hai tab và đưa chỉ số tab cũ về Nâng cấp.

Lượt chụp có một lỗi `Invalid index 5 7 6` từ lệnh dọn preset Game View tạm của MCP. Đã xác định stack trace ở `GameViewSizeGroup.RemoveCustomSize`, dọn lại theo chỉ số đầy đủ và khôi phục preset cũ.

Lượt kiểm cuối chạy riêng EditMode `5dcc260d72c64048bf3ed913024db524` và PlayMode `44eaf2547d024815a55d40a61183392c`. Console 0 lỗi sau hai bộ test.
