# Buildings pass — D4 đã áp và kiểm chứng

## Retry2 — đã áp D4 theo ngưỡng mới

Commit **7e2dd86 feat(art): stylized facades across campus**: 7 vật liệu và 54 slot renderer. Người dùng duyệt ảnh/ngưỡng mới ngày **24/11** theo prompt: FPS giảm≤10%, p95 tăng≤10%, draw tăng≤30 sảnh/≤70 toàn cảnh. Không push. Nhánh này vẫn nối tiếp VRoid chưa merge.

Base `52b4aae`, nhánh `minhnhat/buildings-pass`. Theo `Prompt_Buildings_D4_Retry2.md`, đã bỏ cách so FPS ở hai thời điểm khác nhau: lần Retry trước so với mốc lịch sử nên không cô lập được thay đổi môi trường máy, không đủ cơ sở quy sụt FPS cho vật liệu.

[Nguồn điện](power-AB.json): chế độ Turbo, BatteryStatus=2 (nguồn AC), pin100%; một Editor, hai asset-import worker; không có Editor khác qua MCP. Profiler tắt. File nguồn điện chỉ giữ vai trò/PID, không giữ CommandLine hay tham số đăng nhập Unity.

Đã tạo lại 7 material đúng cấu hình OPTIMIZATION1, GPU Instancing tắt vì lần chẩn đoán trước không giảm draw calls. Trong **cùng một Play Mode**, mỗi góc chạy **A,B,A,B,A,B**, 120 frame làm nóng +3000 frame đo/lượt, 1280×900, VSync0, FOV57, camera theo cameras.json. Không chạy test hay tác vụ nặng song song. Chỉ ghi một dòng TSV/lượt: [12 lượt A/B](performance-AB.tsv), [script đo](ab-benchmark.cs.txt).

|Góc|FPS A → B|Thay đổi FPS B|p95 A → B ms|Thay đổi p95|Draw A → B|Tam giác A=B|
|---|---:|---:|---:|---:|---:|---:|
|Sảnh|239,7593 → 239,2201|−0,225%|4,7829 → 4,8129|+0,627%|286 → 296 (+10)|1.847.729|
|Toàn cảnh|202,7123 → 203,1921|+0,237%|5,8108 → 5,6172|−3,332%|940 → 994 (+54)|2.582.136|

Tất cả ngưỡng tương đối A/B đạt: giảm FPS≤10%, tăng p95≤10%, draw tăng≤30 sảnh/≤70 toàn cảnh. Bảng lấy trung vị ba số trung vị/p95 mỗi cấu hình, không so lại với FPS lịch sử.

**Giới hạn cấu hình hình học:** trước đo đã tìm thấy `Animated Fountain FX` gồm36ParticleSystem; tắt thử runtime giảm36draw và khoảng1.300tamgiác, sau đó khôi phục bật cho cả A/B. Có các phép dò runtime đối tượng khác, không lưu vào scene hay PlayerPrefs. Sau đó hình học A và B khớp nhau cả12lượt, nhưng tổng toàn cảnh vẫn cao hơn mốc lịch sử2.576.670 khoảng5.466tri (0,21%). Chưa giải thích chính xác phần dư này, không ghi rằng đã tái lập tuyệt đối cấu hình lịch sử OPTIMIZATION1. Đây là giới hạn còn lại của bước đối chiếu tam giác trong prompt; không dùng dữ liệu để kết luận về chênh lệch tuyệt đối với mốc cũ.

Sau khi A/B đạt, đã thoátPlay và **ghi đúng54slotmaterial vào file scene**, giữ nguyên mọi byte ngoài các GUID đó. Review độc lập xác nhận54/54GUIDmapping, BaseMap/ST/màu vàkeyword đúng. Commit chỉ chứa54GUIDrenderer; dòngMainCameraY có sẵn không stage. [Danh sách renderer/slot](inventory.json), [mapping vật liệu](material-map.json); đây hiện là mapping đã triển khai.

Lệnh nạp lại scene từng bịMCPtimeout. Sau người dùng mở lạiUnity+StartServer, đã xác nhận native scene có54renderer facade vàisDirty=false, rồi chụp **16ảnhAFTERmới** trong Play. Các ảnhAFTERcủaRetry1 được ghi đè, không nhận ảnh cũ làm bằng chứng triển khai. Mặt tiền giữ chữ, cờ và khung kính; đêm có ô sáng ấm xen ô tối. Vùng tím/beam/pháo hoa là gameplay hiện có, giữ nguyên ngoài phạm vi.

Smoke controller thực **40,00s**, quãng đường ghi nhận khoảng67,57m, tốc độAnimator tối đa6,2; vườn mở được, chuyển ngày/đêm trảglobal0/1, Consolekhônglỗi. [Tóm tắt smoke](facade-smoke.json), [di chuyển](facade-walking-smoke.png), [vườn](facade-garden-smoke.png). Inputđược cấp vàoStarterAssetsInputs; thao tácvườnquaAPIUIthật, khôngtrồng/tưới/thuhoạch. Không kiểm lại phímEvật lý; lần giả lập trước0cạnhphím nên không ghiPasschoE. Không ghiPlayerPrefs trực tiếp; các fixturecamera/player/daynight chỉ trongPlay, khôngsave vào scene.

Kiểm cuối mới, chạy tách riêng: **EditMode133/133** job `e5f690e846544bc192193916bc3fff02`; **PlayMode50/50** job `c255ecd8cbb440c5a79612a7728b4f0a`. Console0error, MissingScript0cảcampus/arena/AR, isDirty=false, compilefalse, EditMode. Một lệnh bắt đầuEditMode bị ngắt do domainreload trước khi tạojob; xác nhận không cójobactive rồi chạy lại, không tính lần ngắt làPass. GameView đã trả preset/phóng lớn cũ. [Bản ghi kiểm cuối](rollout-verification.json). Không còn công việc chờ MCP trong lượt này; [bàn giao cũ đã đóng](AB-RESUME.md).

|Góc|Ngày sau triển khai|Đêm sau triển khai|
|---|---|---|
|Sảnh|[Ảnh](lobby-AFTER-day.png)|[Ảnh](lobby-AFTER-night.png)|
|Căn tin|[Ảnh](canteen-AFTER-day.png)|[Ảnh](canteen-AFTER-night.png)|
|Đài phun nước|[Ảnh](fountain-AFTER-day.png)|[Ảnh](fountain-AFTER-night.png)|
|Toàn cảnh|[Ảnh](overview-AFTER-day.png)|[Ảnh](overview-AFTER-night.png)|
|Cận cảnh cũ|[Ảnh](close-AFTER-day.png)|[Ảnh](close-AFTER-night.png)|
|Mặt tiền chính|[Ảnh](main-front-AFTER-day.png)|[Ảnh](main-front-AFTER-night.png)|
|Tòa gạch đỏ|[Ảnh](brick-close-AFTER-day.png)|[Ảnh](brick-close-AFTER-night.png)|
|Cận Căn tin|[Ảnh](canteen-close-AFTER-day.png)|[Ảnh](canteen-close-AFTER-night.png)|

## Lịch sử D4 Retry — không đạt theo mốc cũ, trước khi chuyển sang A/B

Đã thực hiện `Prompt_Buildings_D4_Retry.md` trên base `d44c0c0`, nhánh `minhnhat/buildings-pass`. Theo prompt, người dùng duyệt hình ngày **24/11** và nâng ngưỡng draw calls toàn cảnh lên **+70**; FPS vẫn giảm tối đa 10%, p95 tăng tối đa 10%. Ngày thực thi theo client hiện tại là 23/11/2026; mốc duyệt 24/11 ghi theo nguyên văn prompt.

Đã tạo lại đúng 7 vật liệu, kiểm độc lập khớp BaseMap/scale/offset 7/7, đúng màu nguồn, mode độc quyền, instancing bật, noise 0,025, ô cửa 3,8×4,6m, màu đèn (0,55;0,38;0,19). Gán thử 54 slot trong Edit Mode **chưa lưu**, rồi vào Play để static batch đúng cấu hình. Không đổi mesh/transform/collider. Ảnh Retry1 lưu tại commit52b4aae; các fileAFTER hiện tại đã được cập nhật bằng ảnh triển khaiRetry2.

|Góc|FPS trước → thử lại|Giảm FPS|p95 trước → thử lại ms|Tăng p95|Draw trước → thử lại|
|---|---:|---:|---:|---:|---:|
|Sảnh|246,7856 → 174,3421|29,36%|4,8515 → 7,4012|52,56%|287 → 301 (+14)|
|Toàn cảnh|208,1729 → 165,7633|20,37%|5,7424 → 7,3082|27,27%|899 → 996 (+97)|

Tris trung vị: sảnh 1.847.809 → 1.847.809; toàn cảnh 2.576.670 → 2.582.214. Số là trung vị ba lượt như phương pháp cũ, mỗi lượt 120 frame làm nóng +3000 frame đo, 1280×900, VSync0, targetFrameRate=-1, camera/FOV đúng cameras.json, không thay targetTexture. [TSV lần thử mới](performance-AFTER.tsv); [mốc trước](performance-BEFORE.tsv). [TSV tối ưu1 lịch sử được bảo toàn](performance-OPTIMIZATION1-HISTORY.tsv). Không ghi TSV từng frame. Profiler và Frame Debugger đã tắt trong phép đo chính thức, Play không pause, Console không lỗi. Đây là so với mốc lịch sử được prompt chỉ định, không phải phép đo A/B cùng thời điểm; chưa đủ bằng chứng quy toàn bộ sụt FPS cho facade hoặc xác định thay đổi môi trường máy.

**Kết luận:** không đạt FPS/p95 ở cả hai góc và draw calls toàn cảnh; đã dừng theo prompt, mở lại scene không lưu, quét 7 GUID không còn tham chiếu ngoài rồi xóa toàn bộ vật liệu thử cùng meta. Scene cuối giữ nguyên bản chưa áp, chỉ còn dòng MainCamera có sẵn của người dùng; không stage dòng đó. Không tạo commit `feat(art)` cho lần không đạt, không push. Không chạy lại smoke/test suites sau khi chạm điều kiện dừng; số 133/50 phía dưới là bằng chứng lượt trước, không nhận là test mới của Retry.

### Kết quả kiểm batching

[Frame Debugger](frame-debugger-retry.json) đọc dữ liệu hợp lệ của từng event; shader SRP compatibility code **0**, 54/54 renderer vẫn static batched. Facade có 36 draw trong ShadowCaster, 53 DepthNormals và 53 ForwardLit trong frame khảo sát. Các nhóm tách theo shader/thuộc tính ảnh hưởng ShadowCaster và theo shader/keyword ở ForwardLit. URP/Lit gốc cũng có các pass tương ứng, nên không kết luận “thêm pass gây +55”. Không có Frame Debugger BEFORE cùng điều kiện để phân rã chính xác toàn bộ chênh lệch +55 của lượt trước. Dữ liệu mới không chứng minh mất static batching toàn bộ.

[Một vòng bật/tắt GPU Instancing](instancing-retry.json) cho cùng 996 draw, không giảm. Đã giữ instancing bật trong lần đo chính thức; không thay hình để ép ngưỡng. Shader facade không có thông báo compile Error, không thấy renderer hiện hành dùng InternalErrorShader khi kiểm tra. Capture debugger có một event InternalErrorShader 2 draw chưa xác định nguồn; không suy diễn rằng toàn cảnh không thể có fallback shader. Ảnh cận mặt tiền đã tự kiểm giữ chữ/cờ/cửa sổ, không thấy facade hồng lỗi.

Phần bên dưới là báo cáo **lượt Overnight trước Retry**, giữ làm lịch sử và không thay thế kết luận không đạt ở trên.

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

Lượt đầu đổi sharedMaterials trong Play có draw954; vòng tối ưu1 chuẩn hóa keyword, bật instancing như source, đổi kích thước ô đèn (3,8×4,6m theo nhịp cửa sổ) và giảm màu đèn, vẫn954. Vòng tối ưu2 gán trong Edit Mode chưa lưu, vào Play mới, xác nhận **54/54 renderer isPartOfStaticBatch=true**, vẫn954. Không bỏ shadow/depth hay sửa pipeline để ép ngưỡng. Batching là giả thuyết ban đầu, không kết luận nguyên nhân duy nhất; chưa chạy Frame Debugger xác định chính xác từng batch. [Trước](performance-BEFORE.tsv), [thử đầu](performance-TRIAL1.tsv), [tối ưu1](performance-OPTIMIZATION1-HISTORY.tsv), [tối ưu2](performance-FINAL.tsv). [Script chụp/đo](capture-benchmark.cs.txt).

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

Kiểm tra sau rollback Retry: Edit Mode, compilation=false, isDirty=false, Console0error; Missing Script0 ởcampus/arena/AR; cả Buildings/ và Buildings.meta không tồn tại sau xóa thành công. Đã khôi phục preset/phóng lớn GameView. [Kết quả Retry](retry-verification.json). Một lệnh xóa asset bị ngắt kết nối trước khi thực thi; đã kiểm thư mục còn tồn tại rồi thử lại thành công và kiểm lại cả folder/meta đều không tồn tại.
