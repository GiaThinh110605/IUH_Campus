# Buildings pass — kết quả cuối

Ngày thực hiện 23/11/2026 (Asia/Bangkok). Windows Editor Unity 6000.5.8f1, URP 17, Direct3D12, RTX 3060 Laptop GPU. Nhánh **minhnhat/buildings-pass** nối tiếp PR VRoid chưa merge, base **3274980**. Không push, không build thiết bị.

## Trạng thái

- D1 hoàn tất: kiểm kê 54 renderer, 7 material, 16 ảnh trước và 6 lượt đo. Commit e85386e.
- D2 hoàn tất: shader IUH/Stylized Facade, 4 chế độ và shadow/depth/normals, test 4 keyword. Commit dce6b24.
- D3 hoàn tất: global _IUH_NightAmount theo chuyển ngày/đêm, khởi tạo/reset khi disable. Commit 34bb46f. Review bổ sung phát hiện coroutine còn cập nhật sau disable: chỉ sửa dòng global mới thành isActiveAndEnabled ? fraction : 0, không đổi fog hay coroutine. Test tái hiện lỗi rồi Pass.
- **D4 bị bỏ**: draw calls toàn cảnh tăng 55, vượt giới hạn 40 sau các vòng tối ưu. Scene được mở lại không lưu; 7 material thử và folder Buildings.meta đã xóa. **Không có renderer nào đổi vật liệu trong kết quả cuối.** Shader/global giữ làm nền, chưa được gán vào nhà. Các ảnh FINAL bên dưới là bản thử bị loại.
- D5 hoàn tất, chỉ điều tra: [vùng nhiễm và khung arena](../INVESTIGATIONS-20261124.md).
- D6 hoàn tất: báo cáo, ảnh, số đo, kiểm thử và prompt.

## Phạm vi và cách giữ nguyên scene

Chỉ chọn 12 nhóm: Central university, Library west wing, Dormitory east wing, Rear west/east wing, Central clock tower, Canteen, Detailed central facade, Facade relief 401/402, University entrance portico, University main gate. Mesh kính Blue green glazing tách riêng; texture tường hiện tại không chứa hình cửa sổ/chữ. Renderer chữ giữ nguyên. Ivory sculpture/Dark bronze trong inventory là trim thuộc nhà, không phải tượng ngoài quảng trường. Mái hiên nhỏ Canteen terrace_9/Canteen awning và các đồ trang trí không nằm trong nhóm mặt tiền, giữ nguyên.

Không tải asset. Không sửa mesh/transform/collider/prefab nguồn, volume, nhân vật, garden, UI hay arena. Vật liệu gốc không sửa. Ngoài scene, các material arena/csproj/slnx/EditorSettings có thay đổi Unity tự ghi từ trước vẫn để ngoài commit. Dòng MainCamera y=3.2066925 của người dùng không stage. Scene cuối chỉ còn diff đó.

## Số đo

1280×900, VSync0, targetFrameRate=-1 trong Play, FOV57 và tọa độ cameras.json. Mỗi góc 3 lượt, mỗi lượt 120 frame warmup +3000 frame đo. Không đổi targetTexture camera chính. Chỉ ghi một dòng TSV/lượt; không lưu raw frame. Bảng lấy trung vị của 3 trung vị FPS/p95 và draw/tris mỗi lượt.

|Góc|Trước FPS|Bản thử cuối FPS|Giảm FPS|Draw trước→thử|p95 trước→thử ms|Tris trước→thử|
|---|---:|---:|---:|---:|---:|---:|
|Sảnh|246,79|229,74|6,91%|287→301 (+14)|4,8515→5,3006|1.847.809→1.847.809|
|Toàn cảnh|208,17|205,49|1,29%|899→954 (+55)|5,7424→5,7828|2.576.670→2.576.670|

FPS đạt nhưng draw toàn cảnh không đạt. Vì D4 bị bỏ, trạng thái nhà cuối là mốc trước, không phải cấu hình cột “bản thử”. Không đo một mốc “sau triển khai” vì không triển khai.

Lượt đầu đổi sharedMaterials trong Play có draw954; vòng tối ưu1 chuẩn hóa keyword, bật instancing như source, đổi kích thước ô đèn (3,8×4,6m theo nhịp cửa sổ) và giảm màu đèn, vẫn954. Vòng tối ưu2 gán trong Edit Mode chưa lưu, vào Play mới, xác nhận **54/54 renderer isPartOfStaticBatch=true**, vẫn954. Không bỏ shadow/depth hay sửa pipeline để ép ngưỡng. Batching là giả thuyết ban đầu, không kết luận nguyên nhân duy nhất; chưa chạy Frame Debugger xác định chính xác từng batch. [Trước](performance-BEFORE.tsv), [thử đầu](performance-TRIAL1.tsv), [tối ưu1](performance-AFTER.tsv), [tối ưu2](performance-FINAL.tsv). [Script chụp/đo](capture-benchmark.cs.txt).

## Ảnh trước và bản thử bị loại

Mỗi hàng cùng tọa độ/FOV. Screenshot bất đồng bộ ScreenCapture trong Play, không dùng đường chụp MCP từng gây PlayerLoop recursive. Đã tự kiểm các ảnh tổng quan, mặt tiền và cặp điều tra: không thấy facade hồng lỗi shader; chữ/logo và ô kính giữ được. Pháo hoa, beam và vùng tím có sẵn là đồ gameplay ngoài phạm vi, có thể khác frame. Không so sánh pixel trực tiếp vùng hiệu ứng động.

|Góc|Trước ngày|Trước đêm|Bản thử ngày (không áp)|Bản thử đêm (không áp)|
|---|---|---|---|---|
|lobby|[Ảnh](lobby-BEFORE-day.png)|[Ảnh](lobby-BEFORE-night.png)|[Ảnh](lobby-FINAL-day.png)|[Ảnh](lobby-FINAL-night.png)|
|canteen|[Ảnh](canteen-BEFORE-day.png)|[Ảnh](canteen-BEFORE-night.png)|[Ảnh](canteen-FINAL-day.png)|[Ảnh](canteen-FINAL-night.png)|
|fountain|[Ảnh](fountain-BEFORE-day.png)|[Ảnh](fountain-BEFORE-night.png)|[Ảnh](fountain-FINAL-day.png)|[Ảnh](fountain-FINAL-night.png)|
|overview|[Ảnh](overview-BEFORE-day.png)|[Ảnh](overview-BEFORE-night.png)|[Ảnh](overview-FINAL-day.png)|[Ảnh](overview-FINAL-night.png)|
|close|[Ảnh](close-BEFORE-day.png)|[Ảnh](close-BEFORE-night.png)|[Ảnh](close-FINAL-day.png)|[Ảnh](close-FINAL-night.png)|
|main-front|[Ảnh](main-front-BEFORE-day.png)|[Ảnh](main-front-BEFORE-night.png)|[Ảnh](main-front-FINAL-day.png)|[Ảnh](main-front-FINAL-night.png)|
|brick-close|[Ảnh](brick-close-BEFORE-day.png)|[Ảnh](brick-close-BEFORE-night.png)|[Ảnh](brick-close-FINAL-day.png)|[Ảnh](brick-close-FINAL-night.png)|
|canteen-close|[Ảnh](canteen-close-BEFORE-day.png)|[Ảnh](canteen-close-BEFORE-night.png)|[Ảnh](canteen-close-FINAL-day.png)|[Ảnh](canteen-close-FINAL-night.png)|

## Kiểm thử, review và giới hạn

Shader test RED: cc144a14397e4906a4a1ec487ff708fe, 4 case fail do shader chưa tồn tại. GREEN/full EditMode ban đầu 7d18d0a83a0348edab1901c7e9dba74e **133/133**. Một job trước đó có 0 test vì file chưa import, không tính Pass; đã refresh asset và chạy lại.

Test global RED 03bbcb2f998a4688948b4dfe7395c32d (Initialize không reset), GREEN full PlayMode 066d1157a28c4cdeadd097542dd23b92 **50/50**. Review thêm case disable giữa transition: RED edb946d695d445aaa242461a35550917; GREEN cfd7ae247e484977846d17693e8596ac **1/1**. Full PlayMode cuối ac11376e2b8e4efb999be57b6d563cba **50/50**. Kết quả EditMode cuối ghi riêng sau khi runner kết thúc.

30s investigation: Flee và garden API hoạt động, global ngày0/đêm1. Lượt phím E8m/4m không nhận cạnh phím (EKeyEdges0), không ghi Pass và không thay gameplay để vượt test. D4 đã bị loại nên không khẳng định smoke trên facade được áp. Fixture đầu bắt manager Instance khi còn inactive gây NullReference; đã sửa riêng fixture để tìm cả object inactive và chạy lại sạch. Không có video/nghe âm thanh/thiết bị iOS trong lượt này.

Reviewer độc lập đọc shader, UV, shadow/depth/normals, scope inventory và hai điều tra. Đã xử lý keyword state và global disabled; review cuối không thấy blocker mới. Cấu hình RenderPipeline/Packages giữ nguyên. Không ghi PlayerPrefs trực tiếp hay chuyển story save; ảnh ngoài vùng nhiễm dùng tắt zone runtime để cô lập nguyên nhân thay vì hoàn thành nhiệm vụ.

## Kiểm kê D1 (không phải danh sách đã triển khai)

[JSON đầy đủ, GlobalObjectId và slot](inventory.json). [Mapping 7 vật liệu thử đã xóa](material-map.json) chỉ lưu để truy vết, không phải asset còn tồn tại.

|Khu|Renderer|Mesh|Slot|Material gốc|Shader|BaseMap|Cửa sổ|
|---|---|---|---:|---|---|---|---|
|Canteen_8|Blue green glazing|Canteen_8_Blue green glazing|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Blue green glazing.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Detailed central facade_400|Blue green glazing|Detailed central facade_400_Blue green glazing|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Blue green glazing.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Rear east wing_5|Warm granite|Rear east wing_5_Warm granite|0|Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Materials/Speckled granite.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Textures/Speckled granite_Albedo.png|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Dormitory east wing_3|Terracotta brick|Dormitory east wing_3_Terracotta brick|0|Assets/IUH_Campus/SharedCampusConcept/Materials/ambientCG/ambientCG_Terracotta_brick.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/Textures/ambientCG/Bricks051/Bricks051_2K-JPG_Color.jpg|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Detailed central facade_400|Ivory sculpture|Detailed central facade_400_Ivory sculpture|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Ivory sculpture.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Canteen_8|Limestone facade|Canteen_8_Limestone facade|0|Assets/IUH_Campus/SharedCampusConcept/Materials/ambientCG/ambientCG_Limestone_facade.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/Textures/ambientCG/Plaster001/Plaster001_2K-JPG_Color.jpg|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Facade relief_402|Dressed limestone|Facade relief_402_Dressed limestone|0|Assets/IUH_Campus/SharedCampusConcept/Materials/ambientCG/ambientCG_Limestone_facade.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/Textures/ambientCG/Plaster001/Plaster001_2K-JPG_Color.jpg|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Canteen_8|Ivory sculpture|Canteen_8_Ivory sculpture|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Ivory sculpture.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Rear west wing_4|Blue green glazing|Rear west wing_4_Blue green glazing|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Blue green glazing.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Central university_1|Blue green glazing|Central university_1_Blue green glazing|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Blue green glazing.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|University main gate_30|Ivory sculpture|University main gate_30_Ivory sculpture|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Ivory sculpture.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Library west wing_2|Dark bronze|Library west wing_2_Dark bronze|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Dark bronze.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Library west wing_2|Limestone facade|Library west wing_2_Limestone facade|0|Assets/IUH_Campus/SharedCampusConcept/Materials/ambientCG/ambientCG_Limestone_facade.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/Textures/ambientCG/Plaster001/Plaster001_2K-JPG_Color.jpg|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Rear east wing_5|Terracotta brick|Rear east wing_5_Terracotta brick|0|Assets/IUH_Campus/SharedCampusConcept/Materials/ambientCG/ambientCG_Terracotta_brick.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/Textures/ambientCG/Bricks051/Bricks051_2K-JPG_Color.jpg|Mesh kính riêng; texture tường không vẽ cửa sổ|
|University main gate_30|Limestone facade|University main gate_30_Limestone facade|0|Assets/IUH_Campus/SharedCampusConcept/Materials/ambientCG/ambientCG_Limestone_facade.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/Textures/ambientCG/Plaster001/Plaster001_2K-JPG_Color.jpg|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Library west wing_2|Warm granite|Library west wing_2_Warm granite|0|Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Materials/Speckled granite.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Textures/Speckled granite_Albedo.png|Mesh kính riêng; texture tường không vẽ cửa sổ|
|University main gate_30|Warm granite|University main gate_30_Warm granite|0|Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Materials/Speckled granite.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Textures/Speckled granite_Albedo.png|Mesh kính riêng; texture tường không vẽ cửa sổ|
|University entrance portico_100|Blue green glazing|University entrance portico_100_Blue green glazing|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Blue green glazing.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Dormitory east wing_3|Limestone facade|Dormitory east wing_3_Limestone facade|0|Assets/IUH_Campus/SharedCampusConcept/Materials/ambientCG/ambientCG_Limestone_facade.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/Textures/ambientCG/Plaster001/Plaster001_2K-JPG_Color.jpg|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Rear east wing_5|Ivory sculpture|Rear east wing_5_Ivory sculpture|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Ivory sculpture.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|University entrance portico_100|Warm granite|University entrance portico_100_Warm granite|0|Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Materials/Speckled granite.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Textures/Speckled granite_Albedo.png|Mesh kính riêng; texture tường không vẽ cửa sổ|
|University entrance portico_100|Ivory sculpture|University entrance portico_100_Ivory sculpture|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Ivory sculpture.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Central clock tower_6|Warm granite|Central clock tower_6_Warm granite|0|Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Materials/Speckled granite.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Textures/Speckled granite_Albedo.png|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Facade relief_401|Dressed limestone|Facade relief_401_Dressed limestone|0|Assets/IUH_Campus/SharedCampusConcept/Materials/ambientCG/ambientCG_Limestone_facade.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/Textures/ambientCG/Plaster001/Plaster001_2K-JPG_Color.jpg|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Canteen_8|Terracotta brick|Canteen_8_Terracotta brick|0|Assets/IUH_Campus/SharedCampusConcept/Materials/ambientCG/ambientCG_Terracotta_brick.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/Textures/ambientCG/Bricks051/Bricks051_2K-JPG_Color.jpg|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Central university_1|Warm granite|Central university_1_Warm granite|0|Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Materials/Speckled granite.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Textures/Speckled granite_Albedo.png|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Rear east wing_5|Limestone facade|Rear east wing_5_Limestone facade|0|Assets/IUH_Campus/SharedCampusConcept/Materials/ambientCG/ambientCG_Limestone_facade.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/Textures/ambientCG/Plaster001/Plaster001_2K-JPG_Color.jpg|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Dormitory east wing_3|Ivory sculpture|Dormitory east wing_3_Ivory sculpture|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Ivory sculpture.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Library west wing_2|Ivory sculpture|Library west wing_2_Ivory sculpture|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Ivory sculpture.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Central clock tower_6|Blue green glazing|Central clock tower_6_Blue green glazing|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Blue green glazing.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Central university_1|Dark bronze|Central university_1_Dark bronze|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Dark bronze.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|University main gate_30|IUH blue|University main gate_30_IUH blue|0|Assets/IUH_Campus/SharedCampusConcept/Materials/IUH blue.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Facade relief_401|Dark bronze|Facade relief_401_Dark bronze|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Dark bronze.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Dormitory east wing_3|Blue green glazing|Dormitory east wing_3_Blue green glazing|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Blue green glazing.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Rear west wing_4|Ivory sculpture|Rear west wing_4_Ivory sculpture|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Ivory sculpture.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Detailed central facade_400|Dark bronze|Detailed central facade_400_Dark bronze|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Dark bronze.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Central clock tower_6|Limestone facade|Central clock tower_6_Limestone facade|0|Assets/IUH_Campus/SharedCampusConcept/Materials/ambientCG/ambientCG_Limestone_facade.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/Textures/ambientCG/Plaster001/Plaster001_2K-JPG_Color.jpg|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Dormitory east wing_3|Warm granite|Dormitory east wing_3_Warm granite|0|Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Materials/Speckled granite.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Textures/Speckled granite_Albedo.png|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Central university_1|Ivory sculpture|Central university_1_Ivory sculpture|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Ivory sculpture.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Rear west wing_4|Dark bronze|Rear west wing_4_Dark bronze|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Dark bronze.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Library west wing_2|Blue green glazing|Library west wing_2_Blue green glazing|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Blue green glazing.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Rear east wing_5|Dark bronze|Rear east wing_5_Dark bronze|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Dark bronze.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Central university_1|Limestone facade|Central university_1_Limestone facade|0|Assets/IUH_Campus/SharedCampusConcept/Materials/ambientCG/ambientCG_Limestone_facade.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/Textures/ambientCG/Plaster001/Plaster001_2K-JPG_Color.jpg|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Dormitory east wing_3|Dark bronze|Dormitory east wing_3_Dark bronze|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Dark bronze.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Facade relief_402|Dark bronze|Facade relief_402_Dark bronze|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Dark bronze.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Detailed central facade_400|Dressed limestone|Detailed central facade_400_Dressed limestone|0|Assets/IUH_Campus/SharedCampusConcept/Materials/ambientCG/ambientCG_Limestone_facade.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/Textures/ambientCG/Plaster001/Plaster001_2K-JPG_Color.jpg|Mesh kính riêng; texture tường không vẽ cửa sổ|
|University main gate_30|Dark bronze|University main gate_30_Dark bronze|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Dark bronze.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|University entrance portico_100|Limestone facade|University entrance portico_100_Limestone facade|0|Assets/IUH_Campus/SharedCampusConcept/Materials/ambientCG/ambientCG_Limestone_facade.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/Textures/ambientCG/Plaster001/Plaster001_2K-JPG_Color.jpg|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Central clock tower_6|Ivory sculpture|Central clock tower_6_Ivory sculpture|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Ivory sculpture.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Rear east wing_5|Blue green glazing|Rear east wing_5_Blue green glazing|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Blue green glazing.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Rear west wing_4|Limestone facade|Rear west wing_4_Limestone facade|0|Assets/IUH_Campus/SharedCampusConcept/Materials/ambientCG/ambientCG_Limestone_facade.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/Textures/ambientCG/Plaster001/Plaster001_2K-JPG_Color.jpg|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Canteen_8|Warm granite|Canteen_8_Warm granite|0|Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Materials/Speckled granite.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Textures/Speckled granite_Albedo.png|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Canteen_8|Dark bronze|Canteen_8_Dark bronze|0|Assets/IUH_Campus/SharedCampusConcept/Materials/Dark bronze.mat|Universal Render Pipeline/Lit|không|Mesh kính riêng; texture tường không vẽ cửa sổ|
|Rear west wing_4|Warm granite|Rear west wing_4_Warm granite|0|Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Materials/Speckled granite.mat|Universal Render Pipeline/Lit|Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Textures/Speckled granite_Albedo.png|Mesh kính riêng; texture tường không vẽ cửa sổ|

Kiểm cuối: **133/133 EditMode** qua Unity TestRunnerApi trực tiếp, [XML](EditMode-final.xml), [tóm tắt](EditMode-final.txt). Hai job MCP EditMode không khởi tạo (không có test chạy) được ghi là timeout, không tính Pass. Script hỗ trợ tạm đã xóa khỏi Assets, lưu [bản mã kiểm chứng](direct-test-runner.cs.txt). Full PlayMode cuối **50/50**, job ac11376e2b8e4efb999be57b6d563cba. Missing Script=0 cả campus/arena/AR; Console0error; sceneisDirty=false, Edit Mode, compilexong. [Bản ghi](verification.json). Đã trả Game View về preset/phóng lớn cũ.
