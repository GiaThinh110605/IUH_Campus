# D5 — kết quả điều tra tĩnh (để bổ sung vào báo cáo chính)

Khảo sát ngày 23/11/2026, base3274980: kết hợp đọc source/scene/material/git và kiểm chứng runtime ở phần cuối. Không sửa production hay scene. Các suy luận tĩnh được đối chiếu với số đo runtime bên dưới.

## 1. Sàn quảng trường tím

Nguồn hợp lý, có chuỗi tham chiếu rõ: `Assets/IUH_Campus/Data/Story/StoryScript_DemGlitch.asset:146` → `fogMaterial` GUID `855f98e952b6c8d46bf7e4bfc7135129` → `Assets/IUH_Campus/Materials/Story/M_Story_CorruptionFog.mat`.

`Assets/IUH_Campus/Scripts/Guardian/Story/IUHStoryDirector.cs:148` tạo `IUHStoryCorruptionZone` cho mỗi Chapter, và dòng 248 tạo lại cho dư chấn. `IUHStoryCorruptionZone.cs:24–31` tạo primitive Sphere tên **Story Corruption Zone**, gán sharedMaterial trên, scale `(2r,3,2r)`, center trung bình vị trí encounters và radius khoảng cách xa nhất +6m. Đây là hình ellipsoid trong suốt phủ mặt đất, không phải script thay material sàn. `Purify()` co hình và fade alpha qua MaterialPropertyBlock trong 2s rồi Destroy.

Material dùng URP/Unlit GUID `650dd9526735d5b46b79224bc6e94025` (xác minh trong `Library/PackageCache/com.unity.render-pipelines.universal@73b4c4ff130e/Shaders/Unlit.shader.meta`); BaseColor `(0.5411765,0.16862746,0.8862745,0.35)`, Transparent queue3000, src/dst blend5/10, ZWrite0. Màu tím chủ động; khác trường hợp shader lỗi cho màu magenta đặc. Không dùng bloom để kết luận shader hợp lệ; agent chính cần runtime isSupported/Console.

Chương Nhà E có hai encounter ở `(-6,.05,6)` và `(20,.05,-10)`. Center `(7,.05,-2)`; radius `sqrt(26²+16²)/2+6=21.2643375m`; scale `(42.528675,3,42.528675)`. Vùng rộng ~42.5m, cao3m giải thích phủ cả tiền cảnh/fountain trong `CHARACTER-VROID/Resume/live-1.png`, dù objective là Nhà E.

Lịch sử: `git log -S 'Story Corruption Zone'` và `-S 'radius * 2, 3'` đều tìm commit `df2a08a` (16/11/2026) thêm corruption zone/director; `git log -S '0.5411765'` tìm `6c4ad92` (16/11/2026) thêm data/material. So `6aa7997` (21/11, merge PR46) với HEAD: CorruptionZone, Director, fog material, story script không đổi. Đây không phải regression của VRoid hay shader tòa nhà lượt D.

Kết luận tĩnh: hiệu ứng vùng nhiễm có thiết kế. Cường độ và diện tích gây cảm giác sàn tím rực là vấn đề mỹ thuật/phạm vi của effect; chưa có cơ sở gọi đây là lỗi vật liệu sàn. Nếu muốn giảm: chỉnh riêng alpha từ .35 xuống khoảng .12–.18 và/hoặc thay footprint bằng vùng sát encounter/ground decal; không đổi ground material. Chỉ đề xuất, chưa chỉnh. Cặp ảnh ngoài/trong nhiệm vụ cùng góc do agent chính bổ sung; tránh giả lập kết quả như đã chạy thật.

## 2. Nhân vật nhỏ trong arena

Quan sát ảnh cũ và mới:

| Ảnh | Kích thước | Khoảng đỉnh đầu→chân quan sát | Tỉ lệ ảnh |
|---|---:|---:|---:|
| CHARACTER/arena-before.png |1920×1080| y883–969 ~86px|~7.96%|
| CHARACTER/arena-after.png (Casual2) |1920×1080| y890–969 ~79px|~7.31%|
| CHARACTER-VROID/Resume/arena.png |1280×900| y744–806 ~62px|~6.89%|

Đây là đo thủ công từ silhouette trên ảnh sẵn có, sai số khoảng ±3px, khác pose/tỉ lệ ảnh; không phải phép đo renderer bounds runtime. Tất cả đều rất nhỏ, nên vấn đề đã tồn tại trước đổi VRoid. Tính theo tỉ lệ chiều cao ảnh, VRoid thấp hơn Casual2 khoảng6% tương đối ở mẫu này; không được diễn giải là model scale giảm đúng6% vì pose và thời điểm khác.

`Match3CombatManager.cs:492` gọi `m_Stage.Enter(player,m_CurrentEncounter.transform)`. `Match3BattleStage.CreateProxy` clone nguyên source với scale thế giới, strip gameplay nhưng giữ mọi MeshRenderer và bật mọi renderer. `GetBounds` gộp tất cả Renderer (ngoại trừ particle/line/trail); không có bộ lọc beacon/VFX mesh. `PlaceFighters` lấy `tallest=max(1.8,playerHeight,monsterHeight)`, sau đó `visibleHeight=tallest/.6` và `camDistance=visibleHeight/2/tan20°`.

YAML campus: `CORRUPTED_ENCOUNTER_BUILDING_E/Beacon_Pillar_Beam`, transform fileID264009949, MeshRenderer264009950, cylinder mesh10206: localPosition `(0,7,0)`, localScale `(.5,7,.5)` → **cột cao14m**, dù model quái chỉ sát mặt đất. Sports/Canteen/Garden/Fountain cũng có beam cấu hình giống vậy. Material `Materials/CombatArena/ArenaGlowPurple.mat` GUID88ad43489e3126b4192835ee2413b9e8. Cột beam nhìn thấy rất cao ở phía phải cả ba ảnh. GetBounds tính nó như chiều cao chiến binh; đây là nguyên nhân tĩnh mạnh nhất của auto-frame quá xa.

Camera serialized trong scene arena `(0,3,-16)` chỉ là giá trị ban đầu, bị PlaceFighters ghi đè. Cấu hình thực lưu trong campus: FOV40°, elevation1m, fighterScreenX .17, groundScreenY .1, fighterScreenHeight .6. `IUH_CombatArena.unity` root COMBAT_ARENA `(0,-2000,0)`, Center local0, rotation identity.

Với tallest14m và không bị horizontal-radius clamp: visibleHeight23.333m, cameraDistance32.055m, target `(0,-1990.667,0)`, camera khoảng `(0,-1989.667,-32.055)` nhìn xuống khoảng1.79°. 1280×900 aspect1.42222: khoảng cách hai fighter21.902m, x±10.951m. 1920×1080: span27.378m, x±13.689m. Nhân vật cao1.8m chiếm gần1.8/23.333=7.71% ảnh, khớp mức7–8% quan sát. Bounds quái chứa phụ kiện khác và phép chiếu có góc nghiêng nên các số là ước lượng, cần runtime chốt.

Hai phương án để Thịnh quyết định (chưa sửa):

1. **Ưu tiên:** không clone beacon vào arena hoặc loại beam/VFX mesh khỏi bounds khi frame; auto-fit theo thân chiến binh. Sau đó nên đặt chiều cao riêng cho player, ví dụ tối thiểu25–30% ảnh, giữ chân và x .17/.83 để tránh board giữa. Nếu tallest thân thực3m thì visibleHeight5m, camDistance6.869m, player1.8m đạt gần36% ảnh. Cần đo thân runtime trước khi chọn số cuối.
2. Chỉnh riêng proxy player scale2.5x: silhouette hiện ~62px thành khoảng155px/900 (~17.2% ảnh), khi beam14m vẫn quyết định camera nên tăng scale có hiệu lực. Hoặc camera khoảng32.1→13m để tăng ~2.46x, nhưng phải đồng thời đặt lại vị trí fighter theo screen-space vì giữ x±10.95m sẽ cắt khỏi khung. Chỉ đổi FOV không giải quyết: PlaceFighters tính lại camDistance theo tan(FOV/2), giữ cùng visibleHeight.

Cần agent chính bổ sung: tên/renderers và bounds runtime của student/monster; transform/FOV camera runtime; chiều cao proxy pixel qua WorldToScreenPoint (ghi rõ bao gồm phần phụ kiện hay thân); cặp ảnh vùng tím cùng góc, có/không corruption; kiểm chứng shader runtime và Console.

## Kiểm chứng runtime ngày 23/11/2026

[JSON đo trực tiếp](ART-BUILDINGS/investigation-runtime.json), Windows Editor1280×900. Tên file báo cáo20261124 giữ đúng yêu cầu prompt; ngày thực hiện23/11 theo Asia/Bangkok.

- Camera ArenaCamera: position(0,-1989.56628,-32.3987923), Euler(1.767893,0,0), FOV40°. Proxy SkinnedMesh bounds cao1.72998m, chiếu WorldToScreenPoint cao64.8775px/900 (~7.21%). So với ảnh Casual2~7.31%: hiện tượng đã có trước VRoid; pose/tỉ lệkhungkhác nên không coi là benchmarkscale chính xác.
- Monster proxy có Beacon_Pillar_Beam bounds extents.y7m (=14m cao), bên cạnh mesh `default` bounds cao5.36m. Beam thực sự có trong clone, không chỉ suy đoán YAML. Số32.40m khớp mô hình tính khoảng32.1m, có phụ kiện/offset nên không bằng tuyệt đối. Loại riêng beam rồi frame theo thân có thể vẫn chịu mesh `default` cao5.36m; khi đó ước lượng camera12.3m, player~19% ảnh. Đây là phương án cần Thịnh kiểm tra tiếp, chưa sửa.
- [Ảnh arena mới](ART-BUILDINGS/investigation-arena.png).
- Zone runtime đúng center(7,.05,-2), scale(42.5286751,3,42.5286751), materialM_Story_CorruptionFog. [Có vùng nhiệm vụ](ART-BUILDINGS/plaza-corruption-on.png) và [ẩn riêng vùng nhiệm vụ](ART-BUILDINGS/plaza-corruption-off.png) cùng camera, cùng đêm. Để tránh thay PlayerPrefs/cốt truyện, ảnh thứ hai chỉ tắt runtime zone để cô lập nguyên nhân, không phải chơi hết nhiệm vụ. Tím giảm rõ khi tắt zone; các LED tím khác vẫn còn, là nguồn độc lập. Đây là hiệu ứng thiết kế vùng nhiễm, không phải shader mặt đất bị lỗi. Có thể đề xuất giảm alpha0.35 xuống0.10–0.15 hoặc đổi sang vành biên để bớt phủ sàn, nhưng không sửa trong lượt này.
- Fixture giả lập phím E8m/4m ghi0 cạnh phím: cả2false, không kết luận cự ly giao tranh lỗi và không ghi Pass cho E. Sau đó vào trận bằng TriggerCombat thật để đo, Flee trả exploration. Garden API mở được; global ngày0/đêm1. Lượt chạy30s Console0error.
- Fixture đầu bắt manager staticInstance khi GameObject manager chưa active gây NullReference ở callback thử nghiệm. Đã dừng Play, sửa fixture chỉ trong Temp để FindObjectsInactive.Include, chạy lại30s sạch; không sửa gameplay để xử lý fixture. Những log cũ không được coi là lỗi production hay Pass.

Không thay scene, material vùng nhiễm, camera arena hay combat production. Đề xuất scaleplayer2.5× cho khoảng65→162px/900 là giải pháp tạm ít đụng bố cục; ưu tiên sửa bounds/proxy có chọn lọc khi Thịnh duyệt.
