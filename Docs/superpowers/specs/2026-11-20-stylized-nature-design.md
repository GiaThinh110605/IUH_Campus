# Thiết kế (nháp) — Nâng cấp đồ họa cây cỏ + ánh sáng theo phong cách stylized

Ngày: 20/11/2026. Trạng thái: **nháp, chờ pack và Thịnh đồng ý**. Làm sau khi lượt 5 (`minhnhat/perf-cleanup`) xong.

## Người dùng đã chốt
- Phong cách **stylized hoạt hình**: cây tán tròn, màu tươi, bóng mềm. Cây cỏ, nhân vật và vật liệu theo cùng một phong cách.
- **Chỉ dùng bản miễn phí.** Không mua bản Source; hiệu ứng gió làm bằng shader đơn giản của dự án, hoặc bỏ qua.
- **Lượt A (lượt này):** cây cỏ + ánh sáng/hậu kỳ. **Lượt B:** nhân vật, gộp với lượt 4 của lộ trình.

## Nguồn
- Chính: [Quaternius Stylized Nature MegaKit](https://quaternius.com/packs/stylizednaturemegakit.html), bản Standard miễn phí. Trang ghi CC0; phải kiểm lại file license trong zip (có tin Quaternius chuyển sang license riêng QAL).
- Dự phòng: [Kenney Nature Kit](https://kenney.nl/assets/nature-kit), CC0.
- Lượt B (tham khảo): [Quaternius Universal Base Characters](https://quaternius.com/packs/universalbasecharacters.html), CC0, rig Humanoid; chưa có quần áo sinh viên.

## Hiện trạng cây cỏ trong `IUH_SharedCampus_Concept`
| Nhóm | Object | Ghi chú |
|---|---|---|
| Cây bóng mát | 18 × `Shade tree_*` (Tree bark + Leaves LOD 0/1/2) | Màu trơn, bóng |
| Bồn, hàng rào, hoa | `Layered planting and clipped hedges_405` (mesh gộp: hedge, White/Pink flowers, Marigold) | Khu Căn tin đã dùng bản sao lọc trong `Meshes/Garden/` |
| Bãi cỏ | 3 × `Lawn` | Vật liệu cỏ |
| Khu vườn khác | `Curved fountain gardens`, `Garden beds and furniture` | Cần xem lại |
| Vườn chơi được | 4 luống `GARDEN`, ô `Stage_Seed/Sprout/Bloom` của `IUH_GardenBed.prefab` | Thay mô hình vào 3 ô |

## Phạm vi dự kiến lượt A (chi tiết sau khi có pack)
1. Nhập phần pack thật sự dùng vào `Assets/IUH_Campus/ThirdParty/Quaternius/StylizedNatureMegaKit/` (+ `SOURCE.md`, `Docs/Credits.md`). Vật liệu chuyển sang URP Lit, không bóng (smoothness thấp).
2. Thay 18 cây bóng mát bằng cây của pack: cùng vị trí, kích thước tương đương, collider đơn giản.
3. Hàng rào, hoa trong bồn: thay mesh gộp bằng prefab bụi và hoa của pack (hoặc chỉ đổi vật liệu, tùy kết quả xem thử).
4. Vườn: đặt mô hình hạt/mầm/hoa của pack vào 3 ô `Stage_*`.
5. Ánh sáng, hậu kỳ theo `.agents/skills/urp-graphics-upgrade`: tông sáng, bóng mềm, bloom nhẹ, color grading ấm. Giữ chế độ ban đêm sau biến cố đọc rõ.
6. Kiểm tra: ảnh trước/sau cùng góc (ngày, đêm); FPS Editor trước/sau; Missing Script = 0; test Pass.

## Ranh giới
- Scene và asset campus thuộc phần Thịnh: **phải được Thịnh đồng ý trước khi sửa**.
- Không đổi vị trí quái, check-in, đường đi; không đổi collider ảnh hưởng di chuyển ngoài cây được thay.
- Không commit nguyên pack: chỉ các file dùng, kèm `.meta`.

## Pack đã tải (20/11)
- Vị trí: `D:\IUH\CongNgheMoi\CK\_downloads\quaternius\StylizedNatureMegaKit\` (ngoài repo). License trong zip: **CC0 1.0**. Bản Standard có 68/116 mô hình.
- Dùng thư mục `FBX (Unity)/` (8,5 MB) và chỉ những texture cần (`Textures/`, 2K; import tối đa 1K cho lá/hoa, 2K cho vỏ cây).
- Ảnh preview của pack dùng shader stylized của bản Source. Bản miễn phí gắn URP Lit sẽ phẳng hơn. Vì vậy lượt này có thêm **1 Shader Graph lá/cỏ đơn giản** của dự án: alpha clip, tô 2 tông (sáng/tối mềm), gió nhẹ bằng vertex offset, bật/tắt gió được (tắt trên máy yếu).

## Chọn mô hình
| Chỗ | Mô hình của pack | Ghi chú |
|---|---|---|
| 18 cây bóng mát `Shade tree_*` | `CommonTree_1…5` (xoay vòng, xoay/scale ngẫu nhiên nhẹ, cố định theo seed) | Không dùng Pine/Dead/Twisted: không hợp campus nhiệt đới |
| Hàng rào trong dải bồn | `Bush_Common` | Xếp theo chiều dài dải |
| Hoa trong dải bồn | `Bush_Common_Flowers`, `Flower_3_Group`, `Flower_4_Group` | Xen kẽ với bụi |
| Viền bãi cỏ (điểm xuyết) | `Grass_Common_Short`, `Grass_Wispy_Short`, `Clover_1/2` | Ít, GPU instancing; không phủ kín bãi |
| Vườn `Stage_Seed` | `Clover_1` thu nhỏ hoặc `Pebble_Round_*` + mầm nhỏ | Nhìn rõ là "mới gieo" |
| Vườn `Stage_Sprout` | `Plant_1`, `Fern_1` thu nhỏ | |
| Vườn `Stage_Bloom` | `Flower_3_Group`, `Flower_4_Group` | Mỗi luống 1 màu như hiện tại nếu được |
| Trang trí phụ (tùy chọn) | `Rock_Medium_*`, `RockPath_*` quanh vườn | Chỉ khi có chỗ trống, không chặn đường |

## Còn mở
- Dải bồn: 2 dải ở khu Căn tin đã thành luống vườn. Còn 10 dải ở 5 khoảnh cỏ khác. **Làm thử 1 khoảnh** (khoảnh tâm (20, -29), gần chỗ người chơi xuất hiện), chụp ảnh cho người dùng duyệt rồi mới làm hết (Task 6).
- Trạng thái: Thịnh đã đồng ý sửa scene (20/11).
- Các zip khác đã tải (`Universal Base Characters`, `Universal Animation Library`, `Modular Character Outfits - Fantasy`, `Ultimate Monsters`) để dành cho lượt B.

## Ghi chú cho lượt B (nhân vật), rà 20/11
Đã kiểm: 4 zip trên đều ghi CC0 1.0 trong file license.
- `Universal Base Characters` bản miễn phí: chỉ có dáng **Superhero** nam/nữ (cơ bắp, người lớn), mặc đồ lót; dáng Regular/Teen nằm ở bản trả phí. Có 8 kiểu tóc + lông mày, texture da sáng/tối.
- `Modular Character Outfits - Fantasy` bản miễn phí: chỉ có **Peasant** và **Ranger** (nam/nữ), áo, quần, giày kiểu trung cổ. Dùng chung với đầu của Base Characters (không dùng toàn thân để tránh xuyên áo).
  - Hướng khả thi miễn phí: bộ **Peasant tô lại màu** thành đồng phục sinh viên (áo sáng, quần sẫm) + đầu/tóc Base Characters. Cần xem thử trong Unity mới biết có hợp không.
- `Universal Animation Library` Standard: 1 FBX Unity (có bản root motion), rig Humanoid. Dùng thay animation cho người chơi và NPC.
- `Ultimate Monsters`: dự án đã dùng (`Assets/IUH_Campus/Models/Monsters`), không cần nhập lại.
- **Ultimate Modular Men (02/2022) và Women (04/2022)** (đã tải 20/11, CC0 1.0): 11 + 10 nhân vật, mỗi nhân vật chia đầu/thân/chân/giày, có thư mục **Humanoid Rig**, kèm `Animations.fbx`. Low-poly mặt phẳng, tô màu khối.
  - Đồ thường ngày: nam `Casual_2` (áo thun, quần dài), `Casual_Hoodie`, `Suit`; nữ `Casual` (áo thun trắng, quần dài), `Formal`, `Suit`.
  - **Đề xuất cho người chơi:** nam `Casual_2` và nữ `Casual`, tô lại thành **áo trắng + quần xanh đậm** (đồng phục sinh viên). Người chơi chọn nam/nữ ở `IUHPlayerCustomizer` nếu dễ làm.
  - NPC sinh viên: các biến thể Casual/Hoodie/Suit còn lại, đổi màu áo.
  - Bỏ: Horse, King, SpaceSuit, Swat, Witch, Medieval, SciFi, Soldier, Adventurer (lệch chủ đề campus).
- Phương án khác nếu bộ Peasant không hợp: [Kenney Character Assets](https://kenney.itch.io/kenney-character-assets) (CC0, 4 model, 75 skin, 17 animation; kiểu khối vuông, dễ thương nhưng khác phong cách cây Quaternius).
