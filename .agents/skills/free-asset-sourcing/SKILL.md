---
name: free-asset-sourcing
description: Chọn, tải và nhập asset miễn phí (texture, HDRI, model) đúng license cho IUH Campus — dùng khi cần thêm texture PBR, skybox, cây cối, đạo cụ hoặc model quái mà project chưa có.
---

# Free Asset Sourcing — IUH Campus

## 1. Nguồn ưu tiên (license CC0 — dùng tự do, được commit vào repo)
| Nguồn | Dùng cho |
|---|---|
| ambientCG (ambientcg.com) | Texture PBR: gạch, bê tông, cỏ, đất, gỗ, kim loại (project đã dùng — xem `Docs/AmbientCG_Graphics_Upgrade.md`) |
| Poly Haven (polyhaven.com) | HDRI làm skybox/ánh sáng môi trường, texture PBR, model đạo cụ/cây |
| Kenney (kenney.nl) | Model low-poly, UI, icon |
| Quaternius (quaternius.com) | Model low-poly nhân vật, quái, cây, đạo cụ (model quái hiện có trong `Assets/IUH_Campus/Models/Monsters`) |

Luôn kiểm tra lại license trên trang của từng asset trước khi dùng.

## 2. Nguồn cần cẩn thận
- **Unity Asset Store (kể cả miễn phí)**: dùng được trong game nhưng theo Standard EULA, **không được phân phối lại file gốc** → không commit pack vào repo công khai; ghi rõ tên pack và cách tải trong README. Hỏi người dùng trước khi thêm.
- Sketchfab, các trang khác: chỉ dùng asset ghi rõ CC0 hoặc CC-BY (CC-BY phải ghi tác giả trong `Docs/Credits.md`). Không dùng asset không rõ license, asset "ripped", hoặc có logo/thương hiệu thật.

## 3. Quy trình
1. Đề xuất danh sách (tên, link, license, dung lượng, lý do) → chờ người dùng duyệt. Không tự tải.
2. Môi trường làm việc có thể không tải được từ mạng → nếu không tải được, báo và để người dùng tải tay vào thư mục chỉ định.
3. Đặt asset vào `Assets/IUH_Campus/ThirdParty/<Nguồn>/<TênAsset>/`, kèm file `LICENSE.txt` hoặc `SOURCE.md` (link, tác giả, license, ngày tải).
4. Chỉ lấy độ phân giải cần thiết: texture 1K–2K; HDRI 2K (đủ cho skybox, nặng hơn không cần); model low/mid-poly.
5. Import settings: texture theo skill `urp-graphics-upgrade` mục 5; model: Read/Write tắt, tạo collider đơn giản nếu cần, material chuyển sang URP Lit.
6. Ghi vào `Docs/Credits.md` mọi asset bên ngoài đã dùng.
7. Commit cả asset + `.meta` + file license; không commit pack lớn khi chỉ dùng vài file (theo README).

## 4. Không làm
- Không tải model/ảnh có người thật, logo IUH hay thương hiệu khác khi chưa có quyền.
- Không thay asset đã được duyệt trong scene campus khi chưa hỏi Thịnh.
