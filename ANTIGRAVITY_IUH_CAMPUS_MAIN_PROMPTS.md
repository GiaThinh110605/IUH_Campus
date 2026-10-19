# Bộ prompt chỉnh sửa `IUH_Campus_Main` theo từng khối nhỏ

Tài liệu này dùng để giao việc lần lượt cho Antigravity. Không gửi toàn bộ các prompt cùng lúc. Hãy chạy theo thứ tự, kiểm tra kết quả của từng khối rồi mới gửi prompt tiếp theo.

## Nguồn tham chiếu chung

- Tour chính thức: <https://vr.iuh.edu.vn/>
- Trong tour, ưu tiên xem lần lượt: `Trụ sở chính > Hình toàn cảnh`, `Nhà hiệu bộ`, `Sảnh nhà hiệu bộ`.
- Scene gốc: `Assets/IUH_Campus/Scenes/IUH_Campus_Main.unity`.
- Model hiện tại: `Assets/Models/campus_P0_progress.fbx`.
- Ảnh hiện trạng:

  - Góc nhìn người chơi: [01_current_player_view.png](Assets/IUH_Campus/Screenshots/Antigravity/01_current_player_view.png)
  - Toàn cảnh phía trước: [02_front_overview.png](Assets/IUH_Campus/Screenshots/Antigravity/02_front_overview.png)
  - Góc bên phải: [03_right_overview.png](Assets/IUH_Campus/Screenshots/Antigravity/03_right_overview.png)
  - Mặt bằng từ trên xuống: [04_top_overview.png](Assets/IUH_Campus/Screenshots/Antigravity/04_top_overview.png)

- Ảnh IUH thật tải trực tiếp từ tour chính thức (panorama 360 độ, 4800×2400):

  - Toàn cảnh campus: [01_Official_Campus_Aerial_360.jpg](Assets/IUH_Campus/Reference/VR_IUH_Official/01_Official_Campus_Aerial_360.jpg)
  - Mặt tiền Nhà E/Nhà hiệu bộ: [02_Official_AdminBuilding_Exterior_360.jpg](Assets/IUH_Campus/Reference/VR_IUH_Official/02_Official_AdminBuilding_Exterior_360.jpg)
  - Sảnh Nhà hiệu bộ: [03_Official_AdminLobby_360.jpg](Assets/IUH_Campus/Reference/VR_IUH_Official/03_Official_AdminLobby_360.jpg)
  - Khu thành tích: [04_Official_AchievementGallery_360.jpg](Assets/IUH_Campus/Reference/VR_IUH_Official/04_Official_AchievementGallery_360.jpg)
  - Phòng chính thư viện: [05_Official_Library_MainRoom_360.jpg](Assets/IUH_Campus/Reference/VR_IUH_Official/05_Official_Library_MainRoom_360.jpg)
  - Kho/kệ sách thư viện: [06_Official_Library_BookStacks_360.jpg](Assets/IUH_Campus/Reference/VR_IUH_Official/06_Official_Library_BookStacks_360.jpg)
  - Hội trường: [07_Official_Auditorium_360.jpg](Assets/IUH_Campus/Reference/VR_IUH_Official/07_Official_Auditorium_360.jpg)
  - Phòng lab/Nhà máy 4.0: [08_Official_Industry40_Lab_360.jpg](Assets/IUH_Campus/Reference/VR_IUH_Official/08_Official_Industry40_Lab_360.jpg)

Các ảnh trên chỉ dùng làm visual reference nội bộ cho việc tái dựng scene. Khi gửi một prompt, hãy đính kèm cả ảnh Unity hiện tại và ảnh IUH thật được liệt kê ở đầu prompt đó.

## Quy tắc bắt buộc cho mọi prompt

1. Chỉ thực hiện đúng khối công việc được giao; không tự làm trước các khối tiếp theo.
2. Không sửa trực tiếp scene gốc. Từ khối 01 trở đi chỉ làm trên `Assets/IUH_Campus/Scenes/IUH_Campus_Main_v2.unity`.
3. Không ghi đè hoặc chỉnh sửa nội dung file FBX nguồn.
4. Không xóa vĩnh viễn asset hoặc GameObject cũ. Nếu cần loại khỏi phiên bản mới, đưa vào nhóm `LEGACY_DISABLED` và tắt nó.
5. Không tải asset ngoài, không thay package và không đổi toàn bộ render pipeline.
6. Trước khi sửa phải kiểm tra Console và scene dirty state. Sau khi sửa phải lưu scene, đợi compile hoàn tất, kiểm tra Console và chạy Play Mode ngắn để xác nhận.
7. Sau mỗi khối, chụp ít nhất hai ảnh before/after vào `Assets/IUH_Campus/Screenshots/Antigravity/After/Block_XX/`.
8. Báo cáo rõ GameObject, asset và setting đã thay đổi. Nếu có lỗi hoặc không thể làm an toàn thì dừng, không đoán.

---

## Prompt 01 — Tạo bản làm việc an toàn

**Ảnh đính kèm:** Unity `04_top_overview.png`; IUH thật `01_Official_Campus_Aerial_360.jpg`.

```text
Bạn đang làm việc trong project Unity IUH_Campus. Chỉ thực hiện bước chuẩn bị an toàn, chưa chỉnh kiến trúc, vật liệu hay ánh sáng.

1. Đọc scene Assets/IUH_Campus/Scenes/IUH_Campus_Main.unity và kiểm tra scene không có thay đổi chưa lưu.
2. Tạo bản sao Assets/IUH_Campus/Scenes/IUH_Campus_Main_v2.unity.
3. Mở bản v2 và xác nhận player, MainCamera, PlayerFollowCamera cùng model campus vẫn tồn tại.
4. Tạo root rỗng tên _REWORK_V2 tại world origin (0,0,0), gồm các nhánh:
   - 00_REFERENCE
   - 01_MASTERPLAN
   - 02_BUILDINGS
   - 03_GROUND
   - 04_LANDSCAPE
   - 05_PROPS
   - 06_LIGHTING
   - 07_GAMEPLAY
   - LEGACY_DISABLED
5. Không reparent 34.930 node của FBX trong bước này. Chỉ ghi nhận root nào đang active/disabled, số camera, số player và số lượng child của model.
6. Lưu scene v2, chạy Play Mode 15–30 giây và xác nhận player/camera vẫn hoạt động.

Tiêu chí hoàn thành: scene gốc không đổi; scene v2 mở được; không có missing script; Console không xuất hiện lỗi mới. Dừng ngay sau bước chuẩn bị này và báo cáo kết quả.
```

---

## Prompt 02 — Chuẩn hóa world origin và hệ tọa độ

**Ảnh đính kèm:** Unity `04_top_overview.png`, `01_current_player_view.png`; IUH thật `01_Official_Campus_Aerial_360.jpg`.

```text
Chỉ sửa hệ tọa độ của IUH_Campus_Main_v2, chưa thay đổi hình dáng hoặc vật liệu.

Hiện model campus có root ở khoảng (375.84, 0.45, -54.10), player/camera cũng nằm xa world origin và nhiều mesh có scale 100. Hãy chuẩn hóa theo cách không phá prefab/model:

1. Tạo một parent điều phối tên CampusWorld dưới _REWORK_V2/01_MASTERPLAN.
2. Đưa toàn bộ campus active, player, spawn và camera rig vào cùng một hệ quy chiếu, sao cho trung tâm khuôn viên nằm gần (0,0,0) và mặt đất gần Y=0.
3. Không chỉnh từng node con của FBX và không Apply Scale lên FBX source.
4. Đặt một marker Reference_Origin và bốn marker North/South/East/West để các bước sau dùng chung.
5. Kiểm tra CharacterController, Cinemachine Follow target, MainCamera tag và AudioListener sau khi di chuyển.
6. Chạy Play Mode, di chuyển nhân vật và xác nhận camera không rung, không mất target, nhân vật không rơi xuyên sàn.

Tiêu chí hoàn thành: campus ở gần origin; player hoạt động như trước; không còn offset hàng trăm mét; không sửa hình học. Dừng sau khi chụp ảnh top view và player view mới.
```

---

## Prompt 03 — Sửa ánh sáng bị cháy trắng

**Ảnh đính kèm:** Unity `01_current_player_view.png`, `02_front_overview.png`, `03_right_overview.png`; IUH thật `01_Official_Campus_Aerial_360.jpg`, `02_Official_AdminBuilding_Exterior_360.jpg`, `03_Official_AdminLobby_360.jpg`.

```text
Chỉ chỉnh lighting, environment và post-processing trong IUH_Campus_Main_v2. Không di chuyển tòa nhà, không sửa mesh và không đổi gameplay.

Hiện ảnh bị overexposure: tường, sân và chi tiết cửa sổ gần như trắng hoàn toàn. Hãy:

1. Kiểm tra Directional Light, Environment Lighting, skybox, shadow distance, HDR và Volume hiện có.
2. Tạo hoặc chỉnh Global Volume dưới _REWORK_V2/06_LIGHTING với exposure trung tính, tonemapping ACES, ambient occlusion nhẹ, color adjustments rất tiết chế.
3. Giảm cường độ mặt trời và ambient sao cho mặt đứng trắng vẫn thấy đường gờ, cửa sổ và bóng đổ.
4. Dùng nhiệt độ màu ban ngày tự nhiên; tránh xanh gắt hoặc vàng gắt.
5. Bật soft shadows phù hợp URP; giữ hiệu năng desktop tầm trung.
6. Không dùng bloom mạnh, depth of field hoặc hiệu ứng cinematic làm khó điều khiển.

Tiêu chí hình ảnh: histogram không dồn sát trắng; tường trắng còn texture/độ sâu; kính xanh và mái có màu rõ; bóng dưới mái và cây có thể đọc được. So sánh before/after từ đúng ba góc ảnh đính kèm rồi dừng.
```

---

## Prompt 04 — Blockout lại masterplan theo panorama thật

**Ảnh đính kèm:** Unity `04_top_overview.png`, `02_front_overview.png`; IUH thật `01_Official_Campus_Aerial_360.jpg`.

```text
Chỉ thực hiện blockout masterplan trong IUH_Campus_Main_v2. Không làm chi tiết mặt đứng, nội thất, vật liệu đẹp hoặc gameplay.

Tham khảo vr.iuh.edu.vn tại Trụ sở chính > Hình toàn cảnh. Campus thật có mật độ cao, bất đối xứng và các khối nhà bao quanh sân thể thao trung tâm; khoảng cách giữa các khối hẹp hơn scene hiện tại.

1. Giữ model hiện tại làm reference và chuyển nó vào 00_REFERENCE nếu cần; không xóa.
2. Dùng primitive hoặc ProBuilder tạo các massing đơn giản dưới 01_MASTERPLAN cho:
   - sân thể thao/sân chính ở lõi;
   - Nhà E/hiệu bộ;
   - các khối cao tầng ký túc xá/giảng đường;
   - các dãy thấp và cầu nối;
   - đường nội bộ, lối đi và ranh campus.
3. Dùng tỷ lệ 1 Unity unit = 1 mét và lấy chiều cao player làm chuẩn.
4. Thu hẹp các khoảng trống quá lớn. Tránh bố cục đối xứng kiểu quảng trường.
5. Đặt tên blockout rõ ràng: BLK_E_Admin, BLK_G, BLK_I, BLK_C, BLK_Courtyard, BLK_ServiceRoad...
6. Chỉ dùng material màu phẳng phân loại khối; chưa làm facade.

Tiêu chí hoàn thành: top view đọc được cấu trúc campus dày đặc giống panorama chính thức; sân trung tâm và luồng giao thông rõ; không có chi tiết thừa. Chụp top view và bird-eye rồi dừng để người dùng duyệt bố cục.
```

---

## Prompt 05 — Sửa riêng Nhà hiệu bộ

**Ảnh đính kèm:** Unity `02_front_overview.png`, `01_current_player_view.png`; IUH thật `02_Official_AdminBuilding_Exterior_360.jpg`, `03_Official_AdminLobby_360.jpg`.

```text
Chỉ làm khối Nhà E/nhà hiệu bộ đã được duyệt trong blockout. Không sửa các tòa nhà khác.

Tham khảo trực tiếp vr.iuh.edu.vn > Trụ sở chính > Nhà hiệu bộ. Các đặc điểm cần thể hiện:

- mặt đứng trắng/xám nhạt, mô-đun dọc rõ;
- phần chân nhà và sảnh được khoét lõm;
- hành lang mở, lan can và các nhịp cột lặp lại;
- mảng kính xanh nhạt/turquoise tiết chế;
- cây cổ thụ và chậu cây lớn trước sảnh;
- biển nhận diện IUH đúng tỷ lệ, không quá lớn.

Hãy dựng theo modular pieces, ưu tiên prefab cho cửa sổ, lan can và cột. Không dựng nội thất sâu trong bước này. Collider chỉ dùng box collider đơn giản cho vỏ nhà và sàn đi bộ.

Tiêu chí hoàn thành: ở góc người cao 1,7 m, nhà hiệu bộ có tỷ lệ thật, sảnh dễ nhận biết, mặt đứng không phẳng và không cháy trắng. Chụp chính diện, góc 3/4 và player-height rồi dừng.
```

---

## Prompt 06 — Sửa cụm nhà cao tầng G–I–C

**Ảnh đính kèm:** Unity `03_right_overview.png`, `04_top_overview.png`; IUH thật `01_Official_Campus_Aerial_360.jpg`.

```text
Chỉ làm cụm nhà cao tầng G–I–C trong IUH_Campus_Main_v2. Không sửa Nhà hiệu bộ, cổng hoặc cảnh quan chung.

1. Dựa vào panorama chính thức để điều chỉnh chiều cao tương đối và footprint bất đối xứng của từng khối.
2. Tạo facade modular gồm ô cửa, hành lang mở, lan can, cột đứng và mảng tường đặc.
3. Thêm các chi tiết silhouette quan trọng: lõi thang, cầu nối, mái xanh nhạt/mái cam, lam che nắng và cụm kỹ thuật mái.
4. Không tạo hàng chục nghìn GameObject riêng lẻ. Lặp cửa/lan can bằng prefab, GPU instancing hoặc gộp mesh theo tầng/material.
5. Chỉ giữ chi tiết nhìn thấy từ sân và tuyến player chính; mặt khuất dùng phiên bản đơn giản hơn.

Tiêu chí hoàn thành: mỗi tòa nhà nhận diện được qua silhouette; tỷ lệ không đồng đều giả tạo; mặt đứng có chiều sâu; hierarchy gọn và không tăng draw call quá mức. Chụp góc giống 03_right_overview và một ảnh từ sân trung tâm rồi dừng.
```

---

## Prompt 07 — Sửa sân trung tâm, đường và cổng

**Ảnh đính kèm:** Unity `04_top_overview.png`, `01_current_player_view.png`; IUH thật `01_Official_Campus_Aerial_360.jpg`, `02_Official_AdminBuilding_Exterior_360.jpg`.

```text
Chỉ sửa ground plane, sân thể thao, lối đi, đường nội bộ và khu cổng. Không sửa tòa nhà hoặc lighting.

1. Đưa sân thể thao thành lõi lớn, bề mặt bê tông cũ nhẹ, có line marking đúng tỷ lệ.
2. Tạo các lối đi hẹp bao quanh sân, ram dốc, bó vỉa và rãnh thoát nước.
3. Khu cổng phải có tỷ lệ người thật, không dùng tường trắng phẳng che hết tầm nhìn như ảnh hiện tại.
4. Thêm hàng rào xanh IUH, chốt bảo vệ, barie, biển chỉ dẫn và mái che bãi xe ở mức vừa đủ.
5. Kiểm tra đường đi từ spawn tới sân trung tâm và Nhà hiệu bộ; không để collider vô hình chặn player.
6. Dùng collider đơn giản, tách riêng Walkable_Ground để bake NavMesh sau này.

Tiêu chí hoàn thành: người chơi nhìn từ cổng có thể nhận biết ngay sân và các tòa nhà; mặt bằng không còn trắng trống; toàn bộ tuyến chính đi bộ được. Chụp cổng, tuyến đi vào và top view rồi dừng.
```

---

## Prompt 08 — Cảnh quan và props đặc trưng IUH

**Ảnh đính kèm:** Unity `01_current_player_view.png`, `02_front_overview.png`; IUH thật `01_Official_Campus_Aerial_360.jpg`, `02_Official_AdminBuilding_Exterior_360.jpg`.

```text
Chỉ sửa landscape và props trong IUH_Campus_Main_v2. Không thay kiến trúc, camera hoặc post-processing.

Thay cảm giác resort/đồng đều hiện tại bằng môi trường campus đô thị thực tế:

1. Giảm cây cọ và các cụm ô che không có trong panorama tham chiếu.
2. Thêm cây cổ thụ tán rộng ở sân và trước Nhà hiệu bộ; dùng 3–5 biến thể scale/rotation có kiểm soát.
3. Bổ sung chậu bonsai, bồn cây, ghế đá, thùng rác, đèn, bảng tin, bình chữa cháy và biển phòng/khu vực.
4. Tăng mật độ xe máy tại đúng các dải đỗ; dùng prefab/instancing, không tạo mỗi chi tiết xe thành nhiều GameObject rời.
5. Thêm AC, ống kỹ thuật, dây/cáp và dấu hiệu sử dụng ở mức nhẹ để campus bớt quá sạch.
6. Giữ hành lang đi bộ và tầm nhìn biển chỉ dẫn thông thoáng.

Tiêu chí hoàn thành: cảnh quan có tầng cao–trung–thấp; bóng cây tạo chiều sâu; props làm rõ công năng nhưng không cản gameplay; hierarchy có nhóm rõ ràng. Chụp sân, mặt tiền Nhà hiệu bộ và bãi xe rồi dừng.
```

---

## Prompt 09 — Tối ưu model và hierarchy

**Ảnh đính kèm:** Unity `02_front_overview.png`, `03_right_overview.png`; IUH thật `01_Official_Campus_Aerial_360.jpg` chỉ để kiểm tra silhouette không đổi.

```text
Chỉ tối ưu kỹ thuật scene v2 sau khi hình dáng đã được duyệt. Không thay đổi bố cục hoặc phong cách hình ảnh.

Hiện model campus_P0_progress.fbx khoảng 75 MB, instance có khoảng 34.930 child, importer chưa Optimize GameObjects và chưa có LOD.

1. Không sửa hoặc ghi đè FBX gốc. Giữ nó trong 00_REFERENCE và disable khi phiên bản modular đã đủ.
2. Với geometry mới, gộp mesh theo từng tòa nhà/tầng/material khi hợp lý.
3. Dùng prefab và GPU instancing cho cửa sổ, lan can, ghế, cây, xe máy và props lặp lại.
4. Tạo LODGroup cho tòa nhà lớn/cây/props; collider dùng proxy đơn giản.
5. Đánh dấu static đúng loại, cấu hình occlusion culling và chuẩn bị lightmap UV cho geometry tĩnh.
6. Không tạo MeshCollider cho toàn bộ campus nếu box/compound collider đáp ứng được.
7. Đo số GameObject active, renderer, draw call, triangles và thời gian vào Play Mode trước/sau.

Tiêu chí hoàn thành: scene v2 không phụ thuộc vào 34.930 node active để hiển thị; hierarchy thao tác được; không thay silhouette; player vẫn va chạm đúng. Báo cáo số liệu before/after và dừng.
```

---

## Prompt 10 — Player, camera và tuyến tham quan

**Ảnh đính kèm:** Unity `01_current_player_view.png`; IUH thật `02_Official_AdminBuilding_Exterior_360.jpg`, `03_Official_AdminLobby_360.jpg`, `04_Official_AchievementGallery_360.jpg`, `05_Official_Library_MainRoom_360.jpg`, `07_Official_Auditorium_360.jpg`, `08_Official_Industry40_Lab_360.jpg`.

```text
Chỉ chỉnh player, camera và POI trên scene v2. Không sửa kiến trúc hoặc vật liệu.

1. Chỉ giữ một player rig, một MainCamera có AudioListener và một Cinemachine Brain.
2. Xác nhận PlayerFollowCamera follow đúng PlayerCameraRoot.
3. Dùng FOV khoảng 55–65 cho third-person; camera không xuyên tường và không bị che bởi player.
4. Đặt spawn tại cổng với hướng nhìn vào campus, không nhìn thẳng vào một bức tường trắng.
5. Tạo POI/tour route theo danh mục ưu tiên của tour chính thức:
   - Hình toàn cảnh;
   - Nhà hiệu bộ;
   - Sảnh nhà hiệu bộ;
   - Khu thành tích;
   - Hội trường;
   - Thư viện;
   - khu phòng lab/mô phỏng.
6. Dùng Input System hiện tại; không thêm lời gọi UnityEngine.Input legacy.
7. Kiểm tra camera, di chuyển, chạy, nhảy và tương tác trong Play Mode.

Tiêu chí hoàn thành: spawn và camera tạo được góc mở đầu đẹp; không có camera/player trùng; POI có tên tiếng Việt rõ; Console không có lỗi Input System. Chụp spawn view và ba POI rồi dừng.
```

---

## Prompt 11 — Kiểm thử và đưa scene vào Build Settings

**Ảnh đính kèm:** dùng toàn bộ ảnh after của các khối trước.

```text
Đây là bước kiểm thử cuối, không tự thay đổi thiết kế đã được duyệt.

1. Validate IUH_Campus_Main_v2: missing script, broken prefab, duplicate AudioListener/MainCamera/EventSystem, collider và NavMesh.
2. Chạy Play Mode ít nhất 3 phút, đi từ cổng tới sân, Nhà hiệu bộ và một khối cao tầng.
3. Kiểm tra Console, Input System, camera collision, player falling, Z-fighting và vật liệu hồng/missing.
4. Kiểm tra hiệu năng tại góc toàn cảnh và sân đông props; ghi lại FPS, batches, draw calls, triangles và memory nếu công cụ hỗ trợ.
5. Chỉ khi tất cả kiểm tra đạt, thêm IUH_Campus_Main_v2.unity vào Build Settings và disable SampleScene; không xóa SampleScene.
6. Chụp bộ ảnh cuối: top, bird-eye, cổng, sân, Nhà hiệu bộ, cụm G–I–C và player-height.

Tiêu chí hoàn thành: build scene đúng; không có lỗi Console; tuyến đi bộ chính hoàn chỉnh; báo cáo rõ mục đạt/chưa đạt. Nếu bất kỳ kiểm tra quan trọng nào thất bại, không sửa lan sang phạm vi khác — chỉ báo cáo và dừng.
```

## Cách sử dụng đề xuất

Sau mỗi prompt, hãy yêu cầu Antigravity trả về:

- danh sách file/GameObject đã thay đổi;
- ảnh before/after;
- lỗi Console còn lại;
- các tiêu chí nghiệm thu đã đạt/chưa đạt;
- checkpoint hoặc commit để có thể quay lại trước khi chạy prompt tiếp theo.
