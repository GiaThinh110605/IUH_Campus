# VRoid — lượt tiếp tục 23/11/2026

**Cập nhật cuối sau khi người dùng khởi động lại MCP:** đã hoàn tất Task 3 (`44171bb`), dọn 29 file Casual2 sau audit 16 GUID/0 tham chiếu, chuyển test sang VRoid. Full EditMode cuối **129/129**, PlayMode **49/49**; Console **0 error**, Missing Script **0 ở cả 3 scene**, scene sạch và ở Edit Mode. Báo cáo tổng hợp có hiệu lực hiện tại là [README cha](../README.md). Các mục “Dừng và bàn giao” phía cuối dưới đây giữ lại làm lịch sử sự cố; không còn là trạng thái chặn hiện tại.

Nhánh `minhnhat/vroid-player`, HEAD `8a6c034`. Windows Editor Unity 6000.5.8f1, Direct3D12, RTX 3060 Laptop GPU, Game View 1280 × 900, VSync tắt, targetFrameRate=-1 khi đo. UniVRM 0.131.4. Không tạo commit mới, không push.

## Đo hiệu năng trước khi chỉnh vật liệu

Mỗi cấu hình 3 lượt, mỗi lượt 120 frame làm nóng rồi 3.000 frame đo. Xen kẽ Casual2 → VRoid ba lần rồi VRoid tắt SpringBone ba lần. Đo khoảng thời gian thực giữa các frame render của camera gameplay, không đổi targetTexture. Nhân vật ở trong khung hình. Đọc draw calls/tris qua UnityStats; đây là số của Editor, không phải build hay chi phí riêng nhân vật.

Camera gameplay tại `(0.200000048, 2.45150971, -41.96107)`, Euler `(8,0,0)`, FOV 40; player `(0,0.4448172,-38)`. Camera theo Cinemachine, các transform trên là đầu phép đo. [Thông tin thiết lập](Controlled/setup.txt).

| Cấu hình | FPS trung vị 9.000 frame | p95 frame ms | Draw calls trung vị | Tris trung vị |
|---|---:|---:|---:|---:|
| Casual2 | 196,09 | 7,0136 | 654 | 2.420.507 |
| VRoid | 191,88 | 7,0775 | 639 | 2.515.799 |
| VRoid không SpringBone | 195,51 | 7,4074 | 639 | 2.515.799 |

FPS VRoid giảm **2,15%** so với Casual2, trong ngưỡng 10%. Nếu lấy trung vị của ba FPS trung vị từng lượt: Casual2 196,47; VRoid 191,27; không spring 196,69; mức giảm 2,65%, cũng trong ngưỡng. Tắt spring tăng FPS gộp khoảng 1,89%, tương ứng khoảng 0,10 ms ở frame trung vị; dao động giữa lượt khá lớn và p95 không tốt hơn, nên chỉ coi là ước lượng chi phí nhỏ, chưa phải kết luận nhân quả chắc chắn. Không cần giảm số spring theo ngưỡng của prompt.

Tắt spring bằng xóa tạm danh sách Springs trong Play Mode và `ReconstructSpringBone`, giữ phần VRM runtime khác; sau đo khôi phục đầy đủ 32 spring. Casual2 prefab làm model tạm dưới player, Animator dùng controller Starter Assets; tắt rig/mesh/Animator/VRM của VRoid khi đo Casual2. Không lưu các thay đổi đo vào scene.

[Từng lượt và số tổng hợp](Controlled/summary.txt). Raw TSV `Controlled/run-1…9`. [Script đo](benchmark.cs.txt); lượt Controlled thay thư mục đầu ra bằng `Resume/Controlled`, phóng lớn Game View và tạm dừng coroutine StoryDirector trong Play Mode trước khi bắt đầu. Game View đã trả về preset và trạng thái phóng lớn cũ sau đó.

Chín TSV ở thư mục Resume gốc là lượt khảo sát đầu, không dùng làm kết luận SpringBone vì nhóm không spring dao động draw calls. Lượt Controlled giải quyết yếu tố gây nhiễu này, VRoid on/off cùng 639 draw calls. Cốt truyện vẫn giữ trạng thái Chapter ban đêm; không đổi PlayerPrefs bằng API trong lượt này.

Ba lỗi RenderPass cũ không tái hiện trong khoảng chơi bình thường trước đo, cả hai đợt đo và các ảnh đầu. Console lúc kết thúc đợt Controlled: 0 error. Các lỗi screenshot mới ở phần dưới là lỗi khác.

## Vật liệu

Extract hai bản sao vào `Assets/IUH_Campus/Materials/PlayerVRoid/`, gán prefab và hai renderer của Campus Student. Giữ nguyên file VRM và Campus volume.

| Thuộc tính | Cũ | Mới (cả hai material) |
|---|---|---|
| `_Color` RGB | 1 | 0,85 |
| `_ShadeColor` RGB | 1,1,1 | 0,65; 0,67; 0,72 |
| `_GiEqualization` | 0,9 | 0,5 |
| `_ShadingToonyFactor` | face 0,91; body 0,875 | 0,7 |
| `_ShadingShiftFactor` | face 0,71; body -0,125 | 0 |
| `_EmissionColor` | trắng | đen |

Ảnh [trước ngày](../Idle-front-day-AFTER.png) / [sau ngày](Idle-front-day-TUNED-2.png), [trước đêm](../Idle-front-night-AFTER.png) / [sau đêm](Idle-front-night-TUNED.png). Sau chỉnh thấy đường viền blazer, quần có hoa văn, mặt có sắc da; đêm không thấy phát sáng. Hai ảnh thử `Idle-front-day-TUNED.png` và `-1` bị preview/camera runtime ghi đè vị trí nên không dùng làm bằng chứng; ảnh `-2` đã tắt preview và xác nhận có nhân vật. Ảnh cũ 960×800; ảnh mới lấy Game View 1280×900, cùng vị trí camera tương đối nhưng khác tỉ lệ và có UI, không phải so sánh pixel đồng nhất. Fixture ảnh tắt controller/preview trong Play Mode, không lưu fixture vào scene.

## Gameplay đã quan sát

- [Controller TSV](controller.tsv): input vào StarterAssetsInputs, ThirdPersonController và capsule thật; đi 2 giây, chạy 2 giây, nhảy rồi đợi tiếp đất. Tốc độ Animator đạt 6,2, có 4 state, cuối lượt grounded tại y=0,4477. Đỉnh y khoảng 1,9676. Camera cách xương đầu ít nhất 3,9757 m trong lượt này. Bản ghi không bao phủ mọi góc camera.
- Không có lỗi OnFootstep/OnLand trong Console tại thời điểm đọc trước khi vào arena. Chưa nghe xác nhận âm thanh trực tiếp; chưa có video xác nhận đầy đủ độ văng tóc/áo khi nhảy, không ghi Pass cho mục đó.
- Arena vào qua `TriggerCombat` của encounter Nhà E; proxy `Arena_Student` có 2 skinned mesh, Avatar hợp lệ; [ảnh arena](arena.png). Flee trả `IsCombatActive=false`. Ảnh toàn arena cho nhân vật rất nhỏ, chưa đánh giá chi tiết tóc trong arena.
- Cửa hàng mở tab 0 và tab 1 bằng API thật, `IsOpen=true`: [cửa hàng](shop.png), [kho](inventory.png). Không mua hay dùng đồ.
- Vườn mở luống index 2 (luống 3) qua phương thức Open ở gần InteractionBounds; `IsOpen=true`, `CanOpenNow=true`: [ảnh](garden.png). Không trồng/tưới/thu hoạch. Đây là gọi API, chưa thử phím tương tác vật lý.
- Chuyển ngày/đêm bằng IUHStoryNightShift đã được quan sát trong ảnh.

## Dừng và bàn giao

Sau các lệnh chụp xuất hiện 5 lỗi `PlayerLoop internal function has been called recursively`. Stack trỏ tới **MCPForUnity ScreenshotUtility.CaptureCompositedAfterFrame, dòng 197**, qua ManageCamera/ManageScene; xem [log trích](PlayerLoop-errors.log). Không quy lỗi này cho VRoid. Không sửa package MCP/UniVRM. Những ảnh shop/kho/vườn chưa được kiểm tra hình cuối sau lỗi công cụ, nên kết quả API không đồng nghĩa toàn bộ ảnh/UI đã Pass.

Đã thoát Play Mode thành công, sau đó chuẩn hóa scene để loại serialization ngoài subtree Campus Student, giữ thay đổi MainCamera y=3,2066925 của người dùng. Backup trước lọc: `Temp/vroid-scene-before-filter.unity`. [Script lọc](filter-scene.cjs.txt). Kiểm YAML offline: không có local fileID bị thiếu cả HEAD lẫn kết quả. Đây không thay thế kiểm chứng Unity.

Lệnh import/nạp lại scene qua MCP không hoàn tất; đọc editor/state sau hơn một phút vẫn báo `ping not answered`. Dừng theo prompt. Chưa xác nhận lại `isDirty`, Console 0 error, Missing Script sau nạp lại. Không chạy EditMode/PlayMode trong lượt này: **0 test mới chạy**. Các số 129/49 ở README cha là lượt trước.

Việc tiếp: người dùng khởi động lại Unity và Start Server, kiểm tra scene/Console/Missing Script cả 3 scene; kiểm lại ánh sáng và liên kết runtime; hoàn tất quan sát tóc/áo, kiểm thử còn thiếu; chạy riêng EditMode và PlayMode. Chỉ khi đạt mới sửa shader Always Included trùng (hiện còn 2 dòng), commit Task 3 chọn hunk Campus Student, rồi quét GUID/dọn Casual2 và cập nhật Task 4. Casual2 vẫn giữ nguyên; không tự nhận Task 3–4 hoàn thành.
