# VFX Asset Store — bằng chứng

Ngày 24/11/2026 (Asia/Bangkok), Unity 6000.5.8f1 Windows Editor, URP/Direct3D12, NVIDIA RTX 3060 Laptop GPU. Base `main` tại `1061a44` (merge PR #51), nhánh `minhnhat/vfx-pass`; không push/amend. Nguồn AC, pin 100%, power scheme Turbo: [power.json](power.json).

## V0 — gói và dependency

Commit `7a21cd4 chore(vfx): add Asset Store VFX packs`. Người dùng import qua Package Manager và cho phép commit. Dùng AssetDatabase.MoveAsset giữ GUID, không di chuyển bằng shell. Giữ 7 prefab được chọn cùng dependency; giữ toàn bộ importer/Editor/shader include/runtime scripts cần thiết của Cartoon FX. Xóa 445 asset thừa theo thứ tự không còn GUID ref; không xóa hai texture font còn được ParticleFont.asset tham chiếu. Texture Max Size 1024, compression mặc định Compressed.

| Gói | Phiên bản đầu vào | Trước dọn (bytes, gồm meta) |
|---|---|---:|
| Procedural Fire — Hovl Studio | AssetOrigin 1.1.0; trang nguồn 1.1.1 | 64.696.492 |
| Cartoon FX Remaster Free — Jean Moreno | readme R1.5.1; Effect.cs.meta R1.5.0 | 37.367.201 |
| Free Stylized Smoke Effects Pack — Maiami Studio | AssetOrigin 1.0; trang nguồn 1.0.1 | 575.839 |

Dung lượng sau cuối: Fire **966.668 bytes**, CFX **4.040.019 bytes**, Smoke **148.452 bytes** (gồm meta/SOURCE; [JSON](pack-sizes-after.json)). Ngoài dung lượng ba thư mục ở bảng, archive legacy `Cartoon FX FREE (old legacy effects).unitypackage` 2.562.415 bytes nằm ở JMO root cũng đã bỏ sau khi GUID chỉ tham chiếu chính meta của nó. Các root import rỗng đã gỡ. Nguồn, ngày import và Unity Asset Store Standard EULA: [Credits](../../../Credits.md), SOURCE.md trong từng thư mục `Assets/IUH_Campus/ThirdParty/AssetStore/`. [Inventory](pack-inventory-before.json), [GUID audit/thứ tự xóa](deletion-guid-audit.json), [445 asset đã xóa lúc dọn chính](deleted-assets.json).

Shader trước/sau: [shader-changes.json](shader-changes.json). Smoke Legacy Particles/Additive → URP Particles/Unlit. Fire Built-in → ShaderGraph ở V0, sau đó V2 dùng URP Particles/Unlit additive với texture chính gói để có hào quang tím ổn định. CFX dùng shader riêng và importer đi kèm. Shader test kiểm isSupported/ShaderHasError, material slot bắt buộc; slot trail null chỉ được bỏ qua khi trails disabled. V0 EditMode 138/138, job `1a0d48e3830a4db5bfe031a7aae27982`. Lượt đầu fail vì test nhầm slot trail không dùng; đã kiểm trails=false và sửa điều kiện test.

## V1 — bỏ sau hai vòng hình ảnh

Không bàn giao khói mới. Bốn test API candidate đã Pass (job `64a93bee71074bd7bd7750cb542d21db`), nhưng ảnh hai vòng đều không thể hiện làn khói đủ rõ. Theo quy tắc prompt, đã khôi phục IUHStoryCorruptionZone và material Smoke về trước V1, xóa prefab Resources và test candidate. `StoryScript_DemGlitch.asset` và fog material cũ giữ nguyên. Không commit V1.

Chẩn đoán đã sửa trong candidate nhưng không được bàn giao: Local scaling kết hợp inverse scale làm hạt rất nhỏ; fade alpha lặp bằng byte bị làm tròn, cần tính từ alpha ban đầu theo realtime. Các file smoke-* và V1-AB* chỉ là chẩn đoán của task đã bỏ, không phải ảnh sản phẩm cuối; không dùng làm kết luận hiệu năng.

## V2 — hào quang dưới chân quái

Commit **43e6210**, đã áp dụng. Component chỉ quản lý visual con, không đổi collider/trigger, khoảng E, beam hay logic encounter. Prefab riêng dùng Procedural Fire, bốn Particle System chuẩn hóa thành hào quang thấp: mỗi hệ tối đa 12 hạt PC/6 mobile, emission 6/3, lifetime1,2s, size0,35m, radius0,45m, upward0,25m/s; tổng trần48/24. Particle collision/light/trail/subemitters/shadow tắt. Dạng bàn giao là hào quang hạt mềm, không phải cột lửa cao; không cần fallback sang CFX.

Phát hiện hai PS cũ PurpleSmoke/GlitchPixels thiếu material tạo ô hồng; V2 gán material URP cùng phạm vi hình ảnh. A/B bao gồm sửa này và hào quang mới. Beam hồng sáng cũ giữ nguyên theo prompt. Ảnh cận chủ ý ngang chân nên có thể cắt phần đầu quái.

Test mobile/bao chiều cao, inactive parent và StripToVisuals arena đã Pass; lượt V2 riêng sau refactor hằng số: **2/2**, job `e86056b499bf4bf8a8fcc2e2c24e06da`. Clone giữ ParticleSystem nhưng component bị strip và hạt dừng; không báo lỗi. Runtime fixture mở khóa AR_Monster bị story lock tắt riêng; không sửa trạng thái unlock trong production. Đã gắn đủ5 prefab và sửa10 PS thiếu material: [vị trí](fire-installed.json). Phạm vi scene được lọc đúng5 child list, 10 material refs và10 document prefab/alias: [scope](scene-scope-V2.json). Giá trị hiện trạng trên5 encounter là reveal16m, trigger4,8m, interact5m, khác ví dụ8m/4m trong prompt; giữ nguyên các giá trị này theo yêu cầu không đổi logic/range, [verification](verification.json).

## V3 — combat

**Bỏ V3 sau hai vòng vượt ngưỡng, đã hoàn nguyên toàn bộ. Không có commit V3.** Pool thử nghiệm trần24 (12 tile +3 cho mỗi loại còn lại), bão hòa thì bỏ yêu cầu hình ảnh, tự trả khi hết hạt. Mobile giảm nửa maxParticles/emission/bursts, có cờ tắt. Hai test pool đã Pass trong nhóm8 test, nhưng đó không phải kết quả triển khai cuối.

Candidate đã thử8 dòng hook vào Match3BoardView/Match3CombatManager: ăn ô, damage quái/player sau tính damage, combo/mega bomb, skill, death signal. Đã khôi phục hai file về HEAD; không còn hook, pool, compositor, Resources/Combat hoặc tests candidate. Các prefab CFX gốc đã dọn trong V0 vẫn được giữ.

Arena thật đã vào và HUD hiển thị; hiệu ứng mô phỏng bằng API visual, không thực hiện damaging move hay phát thưởng victory. Flee kết thúc có thể phát defeat event như API gameplay thông thường; xem [context](V3-QA-context.txt). Hit/death candidate bám anchor popup hiện có, không đổi bố cục arena. Lần đầu compositing alpha làm VFX mờ/khó thấy; đổi additive UI đã thấy hạt. Vòng đo1: Δmedian **+1,6144ms**, Δp95 **+2,3755ms**, Δdraw **+35** ([TSV](V3-AB-first-performance.tsv)). Vòng tối ưu giảm capture1024→512PC/256mobile, tắt depth/color/shadow/postprocess của camera VFX, chỉ giữ emitter chính và tắt CFX runtime helper: Δmedian **+1,7006ms**, Δp95 **+2,4412ms**, Δdraw **+12** ([TSV](V3-AB.tsv)). Đã bỏ theo prompt; không tiếp tục sửa vòng3. Trong các lần dừng thử nghiệm có một lỗi RenderTexture attachment, sau gỡ candidate và fulltest cuối Console0. Các ảnh V3-* là chẩn đoán candidate đã bỏ, không phải ảnh sản phẩm combat mới.

## V3b — mũ và đèn linh vật

Commit **a189750**, đã áp dụng. Base scale (0,19;0,02;0,19)→(0,32;0,025;0,32), localY0,14→0,18; board (0,24;0,01;0,24)→(0,48;0,015;0,48), localY0,165→0,205. Giữ rotation/material/parent. Light intensity2→0,22; range5→1,6. Mặt rõ hơn ban đêm; giữ glow/particles và billboard. Không sửa IUHMascotNPC.cs, tương tác/nhiệm vụ, bob/dance. [Cấu hình/camera](mascot-config.json), [scope đúng6 field](scene-scope-V3b.json). V3b không thêm renderer nên không đo A/B theo prompt. Không lặp phím E vật lý hoặc nhận/nộp quest; script và refs giữ nguyên.

## Phương pháp hiệu năng và giới hạn bằng chứng

1280×900 xác minh trực tiếp Screen.width/height sau khi chọn Game View Fixed Resolution; Screen.SetResolution riêng không có tác dụng trong Editor. VSync=0, cùng phiên A,B,A,B,A,B, mỗi lượt120warmup+3000frame đo; lấy trung vị của3median/p95/draw. Ngưỡng mỗi task: Δmedian≤0,8ms, Δp95≤1,5ms, Δdraw≤30. Chỉ TSV tóm tắt, không raw frame dump. Các TSV có hậu tố diagnostic không dùng kết luận vì sai resolution/fixture/material hoặc task bị bỏ.

Bảng cuối gồm V2 đạt và V3 candidate không đạt: [performance-summary.json](performance-summary.json).

| V2 góc | Δmedian ms | Δp95 ms | Δdraw | Kết quả |
|---|---:|---:|---:|---|
| lobby | +0,0189 | +0,2867 | -2 | Đạt |
| overview | +0,0652 | +0,1378 | +11 | Đạt |
| region | +0,0799 | +0,1499 | +6 | Đạt |
| monsternear | +0,0699 | -0,0068 | +2 | Đạt |

V2 đủ24 lượt: [V2-AB.tsv](V2-AB.tsv). Các lượt 1920×1080 hoặc trước sửa material không dùng kết luận.

## Kiểm chứng cuối và ảnh

**138/138 EditMode** (job `166474bca24644eaa1e4368188b95276`), **61/61 PlayMode** (job `f7913cd204f949d1b645d0c7f79cf801`), failed=0/skipped=0. Số cuối gồm2 test lửa; tests V1/V3 đã bỏ cùng candidate. Missing Script=0 ở campus/arena/AR ([kết quả](missing-scripts.json)). Scene campus đã đóng, lọc serialization về đúng phạm vi, mở lại và chạy fullPlayMode; Unity Edit Mode, compile xong, isDirty=false, Console0 ([verification](verification.json)). Game View preset gốc đã khôi phục. MainCamera Y3,2066925 có sẵn còn unstaged, scene working diff chỉ1 dòng camera; không stage mat/csproj/slnx/ProjectSettings/logs/Screenshots ngoài phạm vi. Chưa build thiết bị mobile/Mac.

| Ảnh bàn giao | Link |
|---|---|
| Hai quái ban ngày | [Quái1](fire-monster1-day.png), [Quái2](fire-monster2-day.png) |
| Hai quái ban đêm | [Quái1](fire-monster1-night.png), [Quái2](fire-monster2-night.png) |
| Toàn cảnh lửa | [Overview](fire-overview-day.png) |
| Mascot ngày trước/sau | [Trước](mascot-before-day.png), [Sau](mascot-after-day.png) |
| Mascot đêm trước/sau | [Trước](mascot-before-night.png), [Sau](mascot-after-night.png) |

Không cung cấp ảnh khói/combat như kết quả hoàn thành vì V1/V3 đã bỏ. Mã runtime bàn giao mới chỉ `IUHCampusMonsterGroundFire.cs`; scene chỉ5 child,10 refs material và mũ/đèn. V4 là commit chứa báo cáo này, SOURCE version corrections và [prompt gốc](../../../Prompts/Prompt_VFX_AssetStore.md). Không push/amend.
