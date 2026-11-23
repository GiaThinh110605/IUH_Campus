# NPC VRoid — E3 đã áp với 6 NPC

## Kết quả E3 Retry theo ngân sách ms

**Đã áp**, commit `0d2dc92 feat(art): populate campus with VRoid students`. Kết quả cuối mới: **137/137 EditMode**, job `8574a428a1a04ddaa1427d8f61c0f2b6`; **56/56 PlayMode**, job `8ce379df8b7f4653ad53b97bf7bfa0be`. Console **0 error**, Missing Script **0 cả campus/arena/AR**, scene sạch, compile xong và Edit Mode. [Bản ghi mới](verification-E3-retry.json). Không push/amend.

Người dùng duyệt ngày **24/11 theo prompt** dùng 4–6 NPC và thay ngưỡng % bằng ngân sách tuyệt đối: median tăng≤1,0ms, p95 tăng≤1,5ms; draw tăng≤40 sảnh/≤60 toàn cảnh. Lý do: ở khoảng240FPS, mức giảm khoảng10% chỉ tương ứng nửa mili-giây, không phản ánh ngân sách60FPS (16,7ms). Không tự nới ngưỡng: dùng đúng Prompt_NPC_VRoid_E3_Retry.md.

Đã triển khai **6 NPC**, 4 đứng thành 2 cặp (Căn tin và sân trước tòa chính), 2 đi bộ. Đủ4model, số bản lần lượt2/2/1/1, không có cặp đứng cùngmodel. [Vị trí và cấu hình](placement-E3-retry.json). Mobile giữ4NPC với cả4model nhờ index0,1,2,3; hai NPC còn lại index4,5 chỉ giữ trên desktop. Khối Students in courtyard_102 **tắt, không xóa**. Không thay player, mascot, quái, vườn hay codeNPC cũ.

|Tên|Model|Chế độ|Vị trí khởi đầu (x,y,z)|Tuyến / bạn cùng nhóm|
|---|---:|---|---|---|
|Student_01|1|Đứng|(-28,0,-20)|Student_02|
|Student_02|2|Đứng|(-26.4,0,-20)|Student_01|
|Student_05|1|Đứng|(-6,0,46)|Student_06|
|Student_06|2|Đứng|(-4,0,46)|Student_05|
|Student_11|3|Đi|(-30,0,19.4082184)|↔(-30,0,24)|
|Student_12|4|Đi|(24,0,-25.9645424)|↔(24,0,-20)|

Ba NPC xa từ góc sảnh (05,06,11) tắt cast shadows bằng override scene. Không thay material/prefab đã nhập ởE1/E2. Giữ outline: [đo nhanh on/off](outline-E3-retry.json), 120warmup+600frame mỗi trạng thái: sảnh on4,8801/off4,82915ms (chênh0,05095ms), toàn cảnh on5,55615/off5,57645ms (tắt không nhanh hơn). Đây là chẩn đoán ngắn, có nhiễu/vị trí walker thay đổi, không kết luận lợi ích nhân quả; không thấy chi phí đáng kể để bỏ outline. Phép đo chính thức bên dưới giữ outline.

### A/B cùng một phiên

A=khối tĩnh cũ bật, NPC mới tắt; B=ngược lại. Mỗi góc A,B,A,B,A,B; mỗi lượt120warmup+3000frame,1280×900,VSync0,FOV57 theo cameras.json. Không thay targetTexture camera chính, không chạy test/tác vụ nặng đồng thời. [12 dòng TSV](performance-E3-retry.tsv), [tổng hợp](summary-E3-retry.json). Máy Turbo, nguồn AC, pin100%: [nguồn điện](power-E3-retry.json). FPS/% chỉ tham khảo; không dùng làm ngưỡng.

|Góc|Median ms A→B|Tăng ms|FPS A→B|Giảm FPS tham khảo|p95 ms A→B|Tăng p95 ms|Draw A→B|
|---|---|---:|---|---:|---|---:|---|
|lobby|4.45400→4.78825|0.33425|224.52→208.84|6.98%|5.2690→5.6744|0.4054|301→289 (-12)|
|overview|5.12290→5.44000|0.31710|195.20→183.82|5.83%|6.0139→6.2412|0.2273|997→1021 (+24)|

**Đạt ở cả hai góc**, không cần giảm còn4. Giữ A/B cùngphiên, không dùng số lịch sử làm baseline. Triangles A/B khác nhau vì thay hình học sinh viên: sảnh 1847809→1895411, toàn cảnh 2582216→3036362.

### Hình và gameplay mới

Đã chụp14ảnh ngày/đêm cùng7góc E3, thêm cậnNPC+player và ảnh toàn thân trước nhà chính. [Toàn thân](npc-fullbody-DEPLOYED-day.png) cho thấy Idle bình thường, chân sát nền, không thấy T-pose/lơ lửng rõ rệt. Các ảnh DEPLOYED là sau triển khai; ảnh AFTER cũ phía lịch sử là bản thử đã hủy.

|Góc|Ngày triển khai|Đêm triển khai|
|---|---|---|
|lobby|[Ảnh](lobby-DEPLOYED-day.png)|[Ảnh](lobby-DEPLOYED-night.png)|
|canteen|[Ảnh](canteen-DEPLOYED-day.png)|[Ảnh](canteen-DEPLOYED-night.png)|
|fountain|[Ảnh](fountain-DEPLOYED-day.png)|[Ảnh](fountain-DEPLOYED-night.png)|
|overview|[Ảnh](overview-DEPLOYED-day.png)|[Ảnh](overview-DEPLOYED-night.png)|
|close|[Ảnh](close-DEPLOYED-day.png)|[Ảnh](close-DEPLOYED-night.png)|
|students-canteen|[Ảnh](students-canteen-DEPLOYED-day.png)|[Ảnh](students-canteen-DEPLOYED-night.png)|
|students-plaza|[Ảnh](students-plaza-DEPLOYED-day.png)|[Ảnh](students-plaza-DEPLOYED-night.png)|

[Cận NPC và player](npc-player-DEPLOYED-day.png). Tự kiểm không thấy NPC shader hồng hoặc T-pose; đồng phục vẫn trắng theo thiết kế, đêm không thấy tự phát sáng. Không đổi ánh sáng/volume để sửa NPC.

Smoke **40s**: CharacterController đi từx−29 đến−23,9587 xuyên qua cặp Căn tin; NPC có0collider. Chắn walker ở khoảng0,45m làm nó đứng yên (displacement0); tránh ra4m thì walker đi tiếp tối đa4,5547m. Vườn mở được qua APIUIthật, global ngày0/đêm1. [Kết quả](smoke-E3-retry.json). Đây là thao tác qua API/controller, không phải kiểm bàn phím vật lý. Không thử lạiE; không ghiPasschoE hoặc chiến đấu8m/4m.

Review độc lập xác nhận6prefab/override/mobileindex/cặpđứng/tuyếnđi hợp lệ, không có localfileIDđứt; thay đổi scene chỉNPC_STUDENTS, aliascamera cần cho tham chiếu và dòngtắt khốicũ. MainCameraY không stage. Unity đã lưu scene trực tiếp quaMCP. Để loại reorder/noise khi serialize, đã đóng scene đang sạch sang scene trống, lọc block dựa trênHEAD và dữ liệu Unity vừa lưu, rồi nạp lại thành công; không ghi tay trong lúc MCP mất phản hồi. Native xác nhận đủ6NPC, khối cũtắt, mọi camera/playerreference hợp lệ. Workingcopy cuối chỉ còn dòngMainCameraY của người dùng. [Bản mã lọc scope](scene-scope-filter.cjs.txt) là bằng chứng thao tác trênbase9a699ac, không phải lệnh cần chạy lại.

Một lệnh bắt đầuEditMode và một lệnh load scene bị domainreload ngắt trước khi hoàn thành. Đã kiểm tra editor kết nối lại, idle/không cójob chạy, rồi mới chạy lại; không tính lần ngắt làPass. Không có bước cần người dùng khởi động lạiUnity trong lượt này. Đã khôi phục preset/phóng lớn GameView. Không ghi PlayerPrefs trực tiếp; fixture chỉ thay đổi runtime, không lưu tọa độ test của player hay trạng thái ngày/đêm vào scene.

## Lịch sử lượt trước — E3 8/12 NPC bị loại theo ngưỡng phần trăm


Ngày thực hiện23/11/2026, Unity6000.5.8f1 Windows Editor, URP17, UniVRM0.131.4, Direct3D12, RTX3060LaptopGPU. Nhánh minhnhat/npc-vroid, base55b07e2. Nguồn điện Turbo, BatteryStatus2 (AC), pin100%: [bản ghi](power.json). Không push/amend; không tải asset.

## Trạng thái cuối

- **E1 xong**, commit7b86cfd: nhập4VRM nguồn,48texture extract có max1024/default+Standaloneoverride,8material MToon,4prefab vàcredit/license. ĐủAvatarHumanoid, CullCompletely, khôngcollider.
- **E2 xong**, commitfd0ab60: Idle/Walk dùng clipStarterAssets cósẵn, controllerNPC riêng, routespingpong, dừng2–5s, stopplayer1.2m, raycast, camera45m, mobileindex<4. Không sửaScripts/NPC, player, mascot hay package.
- **E3 bị bỏ** sau12NPC và2vòngtốiưu: cuối8NPC (6đứng/2đi,2bản/model), bóng3NPCxa tắt vàoutlineNPCtắt vẫn vượtFPS/p95. Scene đã mở lại **không lưu**; NPC_STUDENTS không tồn tại trong scene cuối, khối Students in courtyard_102 vẫn bật. Các material thửoutline đã khôi phục vềE1/E2. **Số NPC VRoid được triển khai cuối:0**. Không có commit populatecampus.
- **E4 xong**: giữ bằng chứng đầy đủ, ảnh AFTER là **bản thử bị loại**, không phải scene được áp.

## Model và cách tắt spring

|Nguồn|Triangles|Material|Spring nguồn|Joint nguồn|Spring trong prefab|Texture dùng để render|
|---|---:|---:|---:|---:|---:|---|
|NPC1|28.126|2|36|114|0|≤1024|
|NPC2|24.904|2|23|73|0|≤1024|
|NPC3|28.604|2|25|79|0|≤1024|
|NPC4|23.452|2|26|84|0|≤1024|

Prefab đặt Vrm10Instance.enabled=false, UpdateType=None, danh sáchSprings rỗng. Điều này cũng tắt cập nhật look-at/expression VRM runtime, chấp nhận vớiNPCtrangtrí. VRM gốc không sửa; [SOURCE với toàn bộ metadata/license và ghi chúNPC3 mang tênNPC2](../../../../Assets/IUH_Campus/ThirdParty/VRoid/NPC/SOURCE.md). Tác giảpixiv VRoidProject, export23/11/2026, creditrequired.

MToon: Lit.85, Shade(.65,.67,.72), GI.5, Toony.7, Shift0, Emission0. Textureextract/remap bằngAPIUniGLTF, không sửapackage; normalmap/SRGB theo descriptorUniVRM. Material gốc trongVRM không đổi.

## Bố trí bản thử

Từmesh Student skin 3.605đỉnh/5.376tri tách được7cụm515đỉnh (mỗiheadSphere768tri): [ước lượng vị trí](static-students.json). Ban đầu đặt12NPC dướiNPC_STUDENTS,8đứng/4đi; [12 vị trí ban đầu](placement.json). Vòng1 còn8: Student01,02,05,06,07,08 đứng và11,12đi; hai bản mỗi model. [Vị trí bản thử cuối](placement-final-trial.json) ghi runtime một thời điểm, walker có thể khác điểm gốc. Các tuyến dưới đây là world-space, NPCkhông cócollider nên không chặnngười chơi.

- Cặp Căn tin: (-28,-20),(-26.4,-20); cặp trước nhà chính:(-6,46),(-4,46); cặp sân đông:(28,-21),(30,-21).
- Walker model3:(-30,18)↔(-30,24); walker model4:(24,-26)↔(24,-20). Tốc độ1.35m/s.
- Raycast các vị trí khởi đầu gặpCampusground; không cóNavMesh. Chưa hoàn tất auditliêntục toàn tuyến và kiểm mọi chân chạm nền/khôngcắtghế vì E3 đã vượtngưỡng. Camera cậnCăntin có bàn/ô che phần chân, không ghiPasshết tiêu chí hình dáng từ ảnhđó.
- Thử12→8 là ngoại lệ số lượng10–14 theo cơ chế bớtNPC khi tốiưu; không được triển khai vì vẫn không đạt.

## A/B cùng phiên

Mỗi cấu hìnhmộtphiênPlay riêng cho mỗi vòng tốiưu; bêntrongphiên đoA,B,A,B,A,B ởlobby rồi overview. A=khối cũbật,NPCtắt; B=ngược lại. Mỗi lượt120warmup+3000frame,1280×900,VSync0,FOV57 vàcamera cấu hình [cameras.json](cameras.json). Chỉ lưu12dòngTSVtóm tắt/vòng, khôngrawframe. Khôngchạytest/song song khiđo. Một lượt đầu cấu hìnhGameView bịdừng trướcphépđochính dohelpertìmrootinactivebằngFind; đã sửa thao tácfixture và chạy lạitừđầu đúng1280×900, không dùnglượtsai.

### performance-12NPC.tsv

|Góc|FPS A→B|GiảmFPS|p95ms A→B|Tăngp95|Draw A→B|Triangles A→B|
|---|---|---:|---|---:|---|---|
|lobby|230.52→198.28|13.99%|5.1248→5.7201|11.62%|301→305 (+4)|1847809→2103637|
|overview|197.33→176.51|10.55%|5.8973→6.5352|10.82%|996→1078 (+82)|2582216→3792260|
### performance-8NPC-round1.tsv

|Góc|FPS A→B|GiảmFPS|p95ms A→B|Tăngp95|Draw A→B|Triangles A→B|
|---|---|---:|---|---:|---|---|
|lobby|236.98→212.37|10.39%|4.9245→5.4134|9.93%|301→305 (+4)|1847809→2103637|
|overview|203.69→190.07|6.69%|5.7447→5.8879|2.49%|996→1047 (+51)|2582214→3378276|
### performance-AB.tsv

|Góc|FPS A→B|GiảmFPS|p95ms A→B|Tăngp95|Draw A→B|Triangles A→B|
|---|---|---:|---|---:|---|---|
|lobby|239.11→214.42|10.32%|4.8703→5.4066|11.01%|301→305 (+4)|1847809→2103639|
|overview|209.71→192.28|8.31%|5.3881→5.9629|10.67%|996→1041 (+45)|2582216→3296642|

Lần cuối toàncảnh draw+45đạt, nhưng sảnh FPSgiảm~10.32% vàp95tăng~11%, toàncảnh p95tăng~10.67% vượt10%. Không tự nới ngưỡng hoặc tiếp tụcvòngtốiưu thứ3. Vòng2 tắt bóng3NPC xa camera sảnh>45m vàtắt MToonOutlinepass/width/keyword; không giảm texture dưới1024. Các tốiưu này đã khôi phụcsau bỏE3, không thaymaterialđãcommit.

## Ảnh trước / bản thử sau (không triển khai)

|Góc|Trướcngày|Trướcđêm|Thửngày|Thửđêm|
|---|---|---|---|---|
|lobby|[Ảnh](lobby-BEFORE-day.png)|[Ảnh](lobby-BEFORE-night.png)|[Ảnh](lobby-AFTER-day.png)|[Ảnh](lobby-AFTER-night.png)|
|canteen|[Ảnh](canteen-BEFORE-day.png)|[Ảnh](canteen-BEFORE-night.png)|[Ảnh](canteen-AFTER-day.png)|[Ảnh](canteen-AFTER-night.png)|
|fountain|[Ảnh](fountain-BEFORE-day.png)|[Ảnh](fountain-BEFORE-night.png)|[Ảnh](fountain-AFTER-day.png)|[Ảnh](fountain-AFTER-night.png)|
|overview|[Ảnh](overview-BEFORE-day.png)|[Ảnh](overview-BEFORE-night.png)|[Ảnh](overview-AFTER-day.png)|[Ảnh](overview-AFTER-night.png)|
|close|[Ảnh](close-BEFORE-day.png)|[Ảnh](close-BEFORE-night.png)|[Ảnh](close-AFTER-day.png)|[Ảnh](close-AFTER-night.png)|
|students-canteen|[Ảnh](students-canteen-BEFORE-day.png)|[Ảnh](students-canteen-BEFORE-night.png)|[Ảnh](students-canteen-AFTER-day.png)|[Ảnh](students-canteen-AFTER-night.png)|
|students-plaza|[Ảnh](students-plaza-BEFORE-day.png)|[Ảnh](students-plaza-BEFORE-night.png)|[Ảnh](students-plaza-AFTER-day.png)|[Ảnh](students-plaza-AFTER-night.png)|

[NPC và người chơi](npc-player-close-day.png). Tựkiểmảnh cậnNPC không thấyTpose hoặcshaderhồng; đêm không phát sáng. Không coi ảnh chân bịche là bằngchứng chạmđấtđầyđủ. Không dùng ảnh bảnthử để khẳng địnhNPC đang có trongscene cuối.

## Kiểm thử và lỗi đã sửa

- E1test4case RED thiếuprefab, rồi GREEN/fullEditMode137/137. Hành vi4case REDthiếuclass, rồiGREEN.
- Thêm regressionprefab thật: lúc đầu camera xa làmNPC bịtắtAnimatortrướckhiSampleIdle, test b85768a6aebb45938f4886ce4a01e8d9 failtay ởTpose. Đã SampleIdle mộtlần trongAwake vớiAlwaysAnimate tạm, rồi trảCullCompletely; cacheanimation resetkhibậtAnimatorlại. Full137Edit/55Playđãqua.
- TrongE3trial, Walk_N phát OnFootstep khôngcóreceiver. Thêm test kiểm events củaclipgốc phải có receiver, REDa2c963a1713c402e9ceafc196296b911 failđúngOnFootstep; thêmreceiverimlặng choNPCdecorative, khôngsửaStarterAssets. Fixnày cầncommitriêng vì khôngamendE2.
- Các testPlay bao phủ routepingpong, stopplayer, distantAnimator, missingplayer, farinitialpose vàclip eventreceiver. Đây không thaythế smokeplay30–60s đi xuyên nhóm/E8m/4m/vườn. SmokeE3 không hoàn tất saukhi chạmngưỡngdừng; khôngghiPasschophímE hayplacement. Không ghiPlayerPrefs trực tiếp.
- Reviewđộc lập kiểmimport/texture/prefabs vàruntime; không cóblocker khác đượcchứngminh. ParentbổsungtestTposevàeventWalktheoquan sátthật.

Kết quả test/cuối phiên được ghi trong verification.json saukhirunxong. Toàn bộfileSourceVRM/frameColliderRootplayer/UI/sceneNPCkhác giữnguyên; MainCameraYngườidùng khôngstage. Cácmaterialarena,csproj,slnx,EditorSettings tựghi giữngoàicommit.

Kiểm cuối sau rollback và sửa receiver: **137/137 EditMode**, job31b2c8b240b0429682c10184ffd916d2; **56/56 PlayMode**, job12dbbd4ffb444e2e98acde412242332f. Console **0 error**, Missing Script **0 ở cả campus/arena/AR**, scene sạch và ở Edit Mode, compile xong. Khối sinh viên cũ bật, NPC_STUDENTS không tồn tại. Bốn file VRM nhập có SHA256 trùng nguồn: [hash](source-hashes.json). [Bản ghi kiểm chứng](verification.json). Preset và trạng thái phóng lớn Game View đã khôi phục.

Do lỗi receiver được phát hiện sau commit E2 và không được amend, có thêm một commit fix riêng. Đây là lệch nhỏ so với kế hoạch mỗi Task một commit; không gộp lại lịch sử. Ảnh AFTER và ảnh NPC với player đều là thử nghiệm đã hủy, không phải scene được áp. Không đo thiết bị iOS hay xác nhận âm thanh bằng nghe trực tiếp.
