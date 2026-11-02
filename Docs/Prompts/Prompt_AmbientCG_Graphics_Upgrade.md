# Prompt cho Codex - Lượt nâng cấp đồ họa bằng texture ambientCG

Gửi nguyên văn khối dưới đây cho Codex đang giữ kết nối Unity MCP.

---

## BỐI CẢNH BẮT BUỘC

Dự án Unity `D:\IUH\CongNgheMoi\CK\IUH_Campus`, Unity 6000.5.8f1, URP 17.5.0.
Scene mục tiêu duy nhất: `Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity`.
Đây là scene "shared campus" đã có sẵn: cổng, trục lát đá, đài phun, tòa nhà chính, căn tin, sân khấu lễ hội, vườn, cây, học sinh, player, camera.

Lượt này chỉ làm MỘT việc: **thay vật liệu bằng texture PBR thật từ ambientCG + tinh chỉnh ánh sáng/hậu kỳ để cảnh đẹp hơn**.
KHÔNG đổi hình học (mesh), KHÔNG đổi vị trí object, KHÔNG đổi ánh sáng cảnh, KHÔNG đổi player/camera/UI, KHÔNG build APK.

## NGUYÊN TẮC BẮT BUỘC

1. Chỉ làm việc trên scene nêu trên. Không đụng `IUH_Campus_Main.unity` hay scene nào khác.
2. **Backup trước khi sửa**: `Docs/Backups/IUH_SharedCampus_Concept_before_ambientcg_<yyyyMMdd_HHmmss>.unity` (không ghi đè file cũ). Ghi lại path trong báo cáo.
3. Chỉ dùng ASCII cho tên asset, tên file, tên method, tên GameObject. Không đặt tên tiếng Việt có dấu.
4. Không xóa, không disable, không thay thế component của object có sẵn. Chỉ thay `sharedMaterials` trên MeshRenderer.
5. Phải chụp **before** ảnh TRƯỚC khi đổi vật liệu (cùng góc camera với after).
6. Phải verify trong **Play Mode** (không chỉ Edit Mode) và chụp ảnh after.
7. Nếu có bước nào không an toàn hoặc kết quả xấu hơn ảnh before -> dừng, báo lại, KHÔNG tự ý tiếp tục đè lên.
8. **Commit trên nhánh riêng, KHÔNG push.** Xem Bước 9. Tuyệt đối không commit trên `main`.
9. Nếu script dùng dấu tiếng Việt trong chuỗi (TextMesh "ĐẠI HỌC") -> giữ nguyên chuỗi gốc, không sửa nội dung text.

## BƯỚC 0 - TẠO NHÁNH RIÊNG + XÁC NHẬN TRẠNG THÁI (bắt buộc, làm trước tiên)

### 0a. Tạo nhánh riêng

**TUYỆT ĐỐI KHÔNG SỬA TRỰC TIẾP TRÊN `main`.** Mọi thay đổi của lượt này phải nằm trên một nhánh riêng,
để khi push không gây conflict với nhánh khác.

Nhánh mới: **`codex/ambientcg-graphics-upgrade`**

```bash
cd "D:/IUH/CongNgheMoi/CK/IUH_Campus"
git fetch origin
git status --short          # xem trạng thái trước
git branch -a               # xem các nhánh đang có
git checkout -b codex/ambientcg-graphics-upgrade
git branch --show-current   # phải in ra codex/ambientcg-graphics-upgrade
```

Quy tắc:

- Nếu nhánh `codex/ambientcg-graphics-upgrade` **đã tồn tại** -> dùng `git checkout codex/ambientcg-graphics-upgrade`
  (không tạo trùng), rồi `git pull --ff-only` nếu có remote.
- Nhánh hiện tại là `codex/six-game-menu-courtyard` (commit `f646b72`) - đây là nhánh chứa
  `IUH_SharedCampus_Concept.unity` mới nhất. **KHÔNG** checkout `main`, KHÔNG merge `main` vào nhánh này.
- **KHÔNG** dùng `git checkout -f`, `git reset --hard`, `git clean`, `git stash drop`.
- Nếu `git status` cho thấy nhiều file modified/untracked từ các lượt trước -> **KHÔNG** `git add -A`.
  Chỉ stage đúng file của lượt này (xem Bước 9).
- Nếu tạo nhánh thất bại (dirty state chặn, hoặc không có quyền ghi `.git`) -> **dừng lại, báo lại**,
  không sửa scene trên nhánh hiện tại.
- Ghi vào báo cáo: tên nhánh, output của `git branch --show-current`, commit base (`git rev-parse HEAD`).

LƯU Ý VỚI UNITY: Unity Editor có thể đang mở sẵn repo. Khi chuyển nhánh, file trong `Assets/`
đổi trên đĩa. Phải đóng scene trong Unity TRƯỚC khi checkout (hoặc bấm Reload sau khi checkout)
để tránh Unity ghi scene cũ đè lên nhánh mới.

### 0b. Xác nhận trạng thái scene trên đĩa

Lượt props trước đã báo `IMPORTED_URBAN_PROPS` với 6 prefab. Xác nhận trên **đĩa**, không tin báo cáo in-memory:

```
- Đọc file scene từ đĩa, kiểm tra có `m_Name: IMPORTED_URBAN_PROPS` và đếm số prefab instance:
  SM_Park_Bench_01 x2, SM_StreetLight_Modern_02 x2, SM_TrashCan_02 x2  => tổng 6
- Ghi lại File mtime + size của scene trước và sau khi sửa.
```

Nếu scene trên đĩa KHÔNG có 6 props này -> dừng lại, báo cáo, không làm tiếp (người dùng cần xác nhận).
Nếu có -> tiếp tục.

## BƯỚC 1 - COPY TEXTURE VÀO ASSETS

Nguồn (ngoài project, không nằm trong Assets):

```
D:\IUH\CongNgheMoi\CK\Texture\
  Bricks051_2K-JPG\        (Color, NormalGL, Roughness)          - không có AO
  Bricks104_2K-JPG\        (Color, NormalGL, Roughness, AO)       - có AO
  Grass005_2K-JPG\        (Color, NormalGL, Roughness, AO)       - có AO
  PavingStones151_2K-JPG\ (Color, NormalGL, Roughness, AO)       - có AO
  Plaster001_2K-JPG\      (Color, NormalGL, Roughness)            - không có AO
  WoodFloor051_2K-JPG\    (Color, NormalGL, Roughness, AO)       - có AO
  WoodFloor062_2K-JPG\    (Color, NormalGL, Roughness)            - không có AO
```

Copy **chỉ các file cần dùng** vào:

```
Assets/IUH_Campus/SharedCampusConcept/Textures/ambientCG/<TenSet>/
```

Cụ thể, copy `_Color.jpg`, `_NormalGL.jpg`, `_Roughness.jpg`, `_AmbientOcclusion.jpg` (nếu có).
**KHÔNG copy**: `_NormalDX.jpg` (sai convention cho Unity), `_Displacement.jpg` (Unity không dùng),
`_2K-JPG.blend`, `.mtlx`, `.tres`, `.usdc`, `.png` preview (không cần thiết, tốn dung lượng).

Tên file sau khi copy (giữ nguyên để dễ tra cứu):
`Bricks051_2K-JPG_Color.jpg`, `Bricks051_2K-JPG_NormalGL.jpg`, ...

Sau khi copy: `AssetDatabase.Refresh()`, đợi import xong (không có "import dở").

## BƯỚC 2 - IMPORT SETTINGS (bắt buộc đúng)

Áp dụng qua `TextureImporter` trong editor script (không sửa .meta bằng tay):

| Loại file | textureType | sRGBTexture | maxTextureSize | mipmap | aniso | wrapMode | filterMode |
|---|---|---|---|---|---|---|---|
| `*_Color.jpg` | Default | true | 2048 | true | 8 | Repeat | Bilinear |
| `*_NormalGL.jpg` | **NormalMap** | false | 2048 | true | 8 | Repeat | Bilinear |
| `*_Roughness.jpg` | Default | false | 2048 | true | 4 | Repeat | Bilinear |
| `*_AmbientOcclusion.jpg` | Default | false | 2048 | true | 4 | Repeat | Bilinear |

- `NormalGL` = OpenGL convention (Y up). Unity `NormalMap` type expects OpenGL. Dùng nhầm `NormalDX` sẽ ra ánh sáng ngược -> tuyệt đối không dùng file DX.
- `normalMapFilter` để mặc định.
- `npotScale` = ToNearest.
- Với `Color`: `textureCompression = CompressedHQ` (giữ chất lượng), `mipMapsPreserveCoverage` bật nếu có.

## BƯỚC 3 - TẠO VẬT LIỆU PBR MỚI (không sửa vật liệu cũ)

Tạo file mới trong `Assets/IUH_Campus/SharedCampusConcept/Materials/ambientCG/`, **không ghi đè** các `.mat` cũ
(`Sandstone paving.mat`, `Paving pale.mat`, `Lawn.mat`, `Limestone facade.mat`, `Terracotta brick.mat`, `Teak timber.mat`, ...).

Với mỗi vật liệu dùng `Shader.Find("Universal Render Pipeline/Lit")` (đúng shader của scene, không dùng Standard).

Cấu hình chuẩn:

```
_BaseMap   = <Ten>_Color.jpg
_BumpMap   = <Ten>_NormalGL.jpg
_BumpScale = 0.6 .. 1.0 tuỳ bề mặt (xem bảng dưới)
_Smoothness = <xem bảng dưới>
_Metallic  = 0
_OcclusionMap = <Ten>_AmbientOcclusion.jpg   (nếu có)  -> đặt _OcclusionStrength = 1.0
_BaseColor = trắng (1,1,1) hoặc tint rất nhạt để giữ đúng màu gốc của texture
_Parallax  = 0   (không dùng height map)
enableKeyword("_NORMALMAP")
enableInstancing = true
```

**Quan trọng - URP/Lit dùng SMOOTHNESS, không dùng Roughness map.**
Không gán `_Roughness.jpg` vào bất kỳ slot nào của URP/Lit (URP/Lit không có slot roughness map).
Thay vào đó đặt `_Smoothness` bằng tay theo bảng. Nếu muốn dùng AO map thì `_OcclusionMap` là slot đúng.

### Bảng ánh xạ vật liệu (đây là phần quan trọng nhất)

| Vật liệu cũ trong scene | Bộ ambientCG thay thế | Smoothness | BumpScale | BaseColor tint |
|---|---|---|---|---|
| `Sandstone paving` (trục lát chính) | **PavingStones151** | 0.18 | 0.9 | (1,1,1) |
| `Paving pale` (lát sáng, bồn cây) | **PavingStones151** (bản tint sáng) | 0.22 | 0.9 | (1.15,1.12,1.05) -> clamp <=1 |
| `Lawn` (bãi cỏ) | **Grass005** | 0.08 | 0.7 | (1,1,1) |
| `Limestone facade` (mặt tiền) | **Plaster001** | 0.16 | 0.5 | (1.0,0.97,0.90) |
| `Terracotta brick` (gạch) | **Bricks051** | 0.14 | 1.0 | (1,1,1) |
| `Teak timber` (bàn ghế, sàn gỗ) | **WoodFloor051** | 0.28 | 0.6 | (1,1,1) |
| (dự phòng, mở thêm) | Bricks104 (gạch khác), WoodFloor062 (gỗ khác) | | | |

Nếu muốn dùng Bricks104 cho một mảng gạch khác thì tạo vật liệu riêng "ambientCG Bricks104" chứ không đè lên Bricks051.

### UV tiling (rất quan trọng, quyết định đẹp/xấu)

Vì mesh hiện tại là primitive cube/cylinder với UV 0..1, tiling phải set bằng `_BaseMap_ST` trên material:

| Bề mặt | tiling X | tiling Y |
|---|---|---|
| PavingStones151 (lát) | 18 | 18 |
| Grass005 (cỏ) | 26 | 26 |
| Plaster001 (tường) | 5 | 3 |
| Bricks051 (gạch) | 6 | 4 |
| WoodFloor051 (gỗ) | 4 | 4 |

Set thành `material.SetTextureScale("_BaseMap", new Vector2(x,y))` và `material.SetTextureScale("_BumpMap", new Vector2(x,y))`
(cùng scale cho normal để không lệch). Chỉnh sau khi chụp ảnh nếu thấy quá nhỏ/quá lớn.

### Mẹo thêm (làm cho "đẹp như mơ")

- **Detail map**: nếu texture lặp bị lộ, dùng `_DetailAlbedoMap` + `_DetailNormalMap` với `material.SetTextureScale` nhỏ (ví dụ 6) để phá vỡ tiling, và bật `_DETAIL_MULX2` không cần. Có thể dùng chính texture làm detail map với tint khác.
- **Normal strength**: nếu ánh sáng trông gợ và rối, giảm `_BumpScale` xuống 0.4-0.6.
- **Metallic** = 0 cho tất cả vật liệu này (gạch/đá/cỏ/gỗ không kim loại).
- **Emission**: không bật.
- **Receive shadows** giữ mặc định = 1.

## BƯỚC 4 - ÁP VÀO SCENE

Cách làm đúng cho project này: **viết editor script mới** thay vì sửa tay trong Editor (để reproducible và chạy lại được):

- Tạo file mới: `Assets/IUH_Campus/Editor/IUHSharedCampusAmbientCGPass.cs`
- Class: `public static partial class IUHSharedCampusConceptBuilder` (partial để dùng chung hằng số `Folder`, `ScenePath`, `root`, `Q/C/S/Flush/Group/Add/Primitive` sẵn có trong `IUHSharedCampusConceptBuilder.cs`)
- Menu item: `[MenuItem("IUH Campus/Concept/Apply ambientCG PBR Texture Pass")]`
- Logic:
  1. Guard: `if (Application.isPlaying || scene.path != ScenePath) throw new InvalidOperationException(...)`.
  2. Guard chống chạy 2 lần: tạo marker GameObject `AMBIENTCG_PASS_APPLIED` dưới root `CAMPUS_ADVENTURE_SHARED_WORLD`; nếu đã tồn tại -> throw (idempotent).
  3. Load vật liệu cũ từ `Folder + "/Materials/"` (dùng đúng tên như bảng ở Bước 3), load/create vật liệu mới từ `Folder + "/Materials/ambientCG/"`.
  4. Duyệt `root.GetComponentsInChildren<MeshRenderer>()`: thay `sharedMaterials` element nào khớp vật liệu cũ bằng vật liệu mới tương ứng (dùng `Undo.RecordObject`).
  5. **Không** đụng tới `IMPORTED_URBAN_PROPS` (props giữ nguyên vật liệu của Urban Props Pack) và không đụng `Campus HQ Renderer`.
  6. `AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);`
- Vật liệu mới phải được tạo bằng `AssetDatabase.CreateAsset` nếu chưa tồn tại, hoặc cập nhật in-place nếu đã tồn tại (để chạy lại được).

**Không** sửa `IUHSharedCampusVisualPolish.cs` hay `IUHSharedCampusConceptBuilder.cs` (file gốc, đã chạy rồi).
Nếu cần thêm hàm helper, đặt trong file mới.

## BƯỚC 5 - HẬU KỲ / ÁNH SÁNG (mức "đẹp như mơ")

Mục tiêu: giữ nguyên ánh sáng cảnh, chỉ tinh chỉnh **hậu kỳ** trong Global Volume và (nếu an toàn) chất lượng pipeline.

### 5a. Global Volume của scene

Tìm Global Volume đang dùng trong `IUH_SharedCampus_Concept.unity`. Có 2 volume candidate trong project:
- `Assets/IUH_Campus/SharedCampusConcept/Campus volume.asset` (đang có: Tonemapping ACES, ColorAdjustments contrast 8 / saturation 6, Bloom threshold 1.4 / intensity 0.1)
- `Assets/IUH_Campus/CourtyardGraphics/CourtyardVolume.asset`

**Xác định đúng volume nào scene đang dùng** (đọc scene, tìm GameObject có `VolumeProfile` trỏ tới file nào), rồi chỉ sửa đúng volume đó.
Backup volume asset trước khi sửa.

Đề xuất giá trị mới (điều chỉnh sau khi nhìn ảnh):

```
Tonemapping: mode = ACES (giữ nguyên, đã đúng)
ColorAdjustments:
  postExposure  = 0.15   (override = true)   -> sáng hơn nhẹ
  contrast      = 12     (override = true)
  saturation    = 10     (override = true)
  colorFilter   = (1, 1.01, 1.03)  (override = true)  -> hơi mát, rất nhẹ
Bloom:
  threshold = 1.15
  intensity = 0.35
  scatter   = 0.72
  tint      = (1, 0.97, 0.93)   -> bloom ấm
Vignette (mới, nếu chưa có):
  intensity = 0.22, smoothness = 0.45, color = (0.05,0.06,0.10)
WhiteBalance (nếu có sẵn trong profile):
  temperature = 4, tint = -2
```

Nếu profile hiện tại không có sẵn component Vignette/WhiteBalance -> thêm qua `profile.TryGet/Add<>` rồi set `overrideState = true`.
Chỉ add component vào đúng volume của scene này.

### 5b. Pipeline

Scene đã có `Campus HQ Pipeline.asset` (copy từ `Assets/Settings/PC_RPAsset.asset`) với:
render scale 1.2, MSAA 4, shadowmap 4096, SSAO (intensity .75, radius .35).

Nâng thêm cho đẹp (nếu không gây giật/slow):
- Render scale 1.2 -> giữ nguyên (đã cao). KHÔNG tăng lên 1.5 để tránh rớt FPS trên laptop 6 GB.
- MSAA 4 -> giữ.
- SSAO: nếu chưa đủ rõ, `Intensity 1.0`, `Radius 0.5`, `Downsample` bật (samples 4) để nhẹ hơn.
- **Không** thêm SSAO nếu đã có.
- Kiểm tra URP Asset: MSAA, HDR on, Main Light = Directional, Additional Lights = Per Pixel (nếu đang Disabled thì bật lên Per Pixel, tăng `Additional Lights Per Object`).

### 5c. Sky

Skybox hiện tại là `Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Materials/Photographic campus sky.mat` (shader `Skybox/Panoramic`).
**Giữ nguyên skybox** - đã là ảnh chụp, không cần đổi. Chỉ chỉnh exposure nếu trời quá tối.

### 5d. Ánh sáng (cẩn thận - thay đổi này dễ làm hỏng cảnh)

Mặc định **không đổi** directional light, không đổi intensity, không đổi màu. Vì lượt trước đã set:
`sun.intensity 2.05`, `sun.color (1,.95,.85)`, `sun.shadowStrength .83`,
`ambientSkyColor (.57,.69,.83)`, `ambientEquatorColor (.46,.50,.52)`, `reflection probe 256 realtime`.

Chỉ can thiệp khi ảnh after cho thấy vấn đề rõ ràng, và chỉ đổi MỘT trong số:
- shadowStrength .83 -> .75 (bóng đậm quá)
- thêm `sun.shadowStrength` giữ; hoặc đổi `RenderSettings.ambientIntensity` nếu cảnh tối.

Nếu cần đổi: ghi rõ lý do và giá trị cũ/mới trong báo cáo.

## BƯỚC 6 - VERIFY

1. **Edit Mode**: 0 Error, 0 Warning mới. Không có "import dở".
2. **Play Mode**:
   - Player đi được bằng CharacterController thật trên 4 tuyến đã dùng ở lượt props:
     (a) qua cổng, (b) vòng đài phun nước đến sảnh, (c) cổng -> căn tin, (d) cổng -> sân khấu.
   - Ghi lại số bước giải va chạm (lượt trước: 2.160).
   - Kiểm tra **không đổi collider**: chụp snapshot component (lượt trước: 822 component) trước và sau -> phải giữ nguyên 822/822, không thêm, không xóa, không disable.
   - Không rơi qua sàn, không kẹt collider mới, không lỗi shader/material.
3. **Thoát Play Mode**: scene kết thúc dirty=false, đã lưu.
4. **Đọc lại scene từ đĩa** và xác nhận các GUID texture mới thực sự nằm trong file .unity (grep GUID `_BaseMap`).
5. Kiểm tra vật liệu trong Play: không có "pink/magenta" (thiếu shader), không có vật liệu "dùng shader không hỗ trợ".
6. Kiểm tra `IMPORTED_URBAN_PROPS` vẫn đủ 6 instance, mesh không đổi.

## BƯỚC 7 - ẢNH EVIDENCE

Thư mục: `Docs/Evidence/AmbientCG_<yyyyMMdd>/`

Chụp **cùng góc** trước và sau (tọa độ camera giống lượt props để dễ so sánh):
- Tổng thể: position (13, 9, -56) -> target (6, 5, 25)
- Căn tin: position (-15, 4, -40) -> target (-28, 1.4, -19)
- Sân khấu: position (12, 4, -37) -> target (29, 1.5, -19)
- Thêm: cận cảnh mặt đất lát đá (để thấy texture), cận cảnh facade (để thấy plaster), cận cảnh bãi cỏ.

File:
```
before-<view>.png
after-<view>.png
```
(view = overview | canteen | stage | paving-closeup | facade-closeup | grass-closeup)

Ngoài ảnh, ghi các file TXT kiểm chứng:
```
material-mapping.txt     (vật liệu cũ -> mới, tiling, smoothness, bumpScale)
component-snapshot.txt    (822 component trước/sau)
play-routes.txt           (4 tuyến, số bước)
console-log.txt
disk-verification.txt     (mtime/size scene trước/sau, grep GUID)
```

## BƯỚC 8 - TÀI LIỆU

Tạo `Docs/AmbientCG_Graphics_Upgrade.md` (tiếng Việt, theo đúng văn phong `Docs/Urban_Props_Pass_20261031.md`):

- Tiêu đề + ngày
- Mục "Trước khi sửa": trạng thái scene, backup path, xác nhận 6 props trên đĩa
- Mục "Texture đã dùng": bảng bộ ambientCG -> vật liệu -> bề mặt, tiling, smoothness, bumpScale
- Mục "Hậu kỳ": bảng giá trị cũ -> mới của Tonemapping/ColorAdjustments/Bloom/Vignette, và pipeline
- Mục "Kiểm chứng": kết quả Play Mode 4/4 tuyến, số bước, 822/822 component, 0 error
- Mục "Ảnh": bảng before/after với link
- Mục "Giới hạn": chưa đo FPS thật trên build, chưa verify Android, texture PBR thay model thật, v.v.
- Dừng sau lượt này. Không push.

## BƯỚC 9 - COMMIT TRÊN NHÁNH RIÊNG (không push)

Sau khi mọi bước verify đã PASS, commit **trên nhánh đã tạo ở Bước 0a**:

```bash
git branch --show-current     # BẮT BUỘC phải là codex/ambientcg-graphics-upgrade
```

Nếu lệnh trên in ra `main` hoặc bất kỳ nhánh nào khác -> **DỪNG, không commit**, báo lại.

Stage **đúng từng file** (không `git add -A`, không `git add .`):

```bash
git add Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity
git add Assets/IUH_Campus/SharedCampusConcept/Textures/ambientCG
git add Assets/IUH_Campus/SharedCampusConcept/Materials/ambientCG
git add Assets/IUH_Campus/Editor/IUHSharedCampusAmbientCGPass.cs
git add Assets/IUH_Campus/Editor/IUHSharedCampusAmbientCGPass.cs.meta
git add Docs/AmbientCG_Graphics_Upgrade.md
git add Docs/Evidence/AmbientCG_<yyyyMMdd>
git add Docs/Prompts/Prompt_AmbientCG_Graphics_Upgrade.md
```

Mọi file kèm `.meta` của asset mới đều phải được add (Unity sinh `.meta` khi import).

**KHÔNG add**: `Library/`, `Temp/`, `Logs/`, `obj/`, `*.csproj`, `*.slnx`, `.vscode/`, `UserSettings/`,
`Docs/Backups/` (file backup scene rất nặng, để untracked), và **không add các file đã modified từ
lượt trước** không thuộc lượt này.

Sau đó:

```bash
git status --short          # xem đúng những gì sắp commit
git diff --cached --stat    # xem thống kê
git commit -m "feat: upgrade shared campus graphics with ambientCG PBR textures"
git log --oneline -1
```

- **KHÔNG** `git push` (người dùng sẽ tự push sau khi duyệt).
- **KHÔNG** `git merge`, `git rebase`, `git tag`.
- Commit message tiếng Anh, ngắn, nói rõ *tại sao*: "upgrade shared campus graphics with ambientCG PBR textures".
- Nếu có file nào không add được -> ghi tên file đó vào báo cáo, không báo "đã commit tất cả".

## MỘT VÀI LƯU Ý KỸ THUẬT QUAN TRỌNG

- **Unity URP/Lit không có roughness map slot.** Gán `_Roughness.jpg` vào `_MetallicGlossMap` sẽ cho kết quả sai (đó là smoothness map của pipeline cũ + metallic). Đặt `_Smoothness` bằng tay.
- **`_NormalGL` không phải `_NormalDX`.** File DX sẽ làm normal map bị đảo chiều Y.
- **`AmbientOcclusion` map vào `_OcclusionMap`** (URP/Lit hỗ trợ), không phải vào `_BaseMap`.
- **Tiling phải set trên material** (`_BaseMap_ST`), không sửa UV của mesh. Sửa UV sẽ phá collider/shape và tốn công.
- **Đừng set `_Metallic` > 0** cho gạch/đá/cỏ/gỗ.
- **Đừng đụng `Campus HQ Renderer.asset`** nếu không cần; nếu cần thì backup trước.
- **Đừng thêm Component mới vào object có sẵn** trừ marker `AMBIENTCG_PASS_APPLIED` và (nếu cần) component Volume override. Báo cáo rõ mọi component thêm.
- `AssetDatabase.Refresh()` + chờ import xong trước khi `LoadAssetAtPath`, nếu không sẽ null.
- Copy texture **ngoài** Assets (từ `D:\IUH\CongNgheMoi\CK\Texture`) vào Assets: dùng `File.Copy` trong editor script hoặc copy tay trước khi mở Unity; không copy trong lúc Play Mode.

## TIÊU CHÍ HOÀN THÀNH

- [ ] Đã tạo và đang ở nhánh `codex/ambientcg-graphics-upgrade` (không phải `main`)
- [ ] Scene trên đĩa có 6 props (đã xác nhận bước 0b)
- [ ] 7 bộ texture được copy vào Assets, import đúng (Normal map cho NormalGL)
- [ ] 7+ vật liệu URP/Lit mới tạo, không sửa vật liệu cũ
- [ ] Tất cả bề mặt paving/lawn/facade/brick/wood trong scene đã đổi sang texture thật
- [ ] Hậu kỳ + vignette + exposure đã tinh chỉnh
- [ ] Play Mode: 4/4 tuyến PASS, 822/822 component giữ nguyên, 0 Error
- [ ] Ảnh before/after cùng góc + ảnh cận cảnh
- [ ] `Docs/AmbientCG_Graphics_Upgrade.md` + file evidence TXT
- [ ] Đã commit trên nhánh riêng, **chưa push**
- [ ] Không build APK, không sửa scene khác, không merge/rebase

## BÁO CÁO CUỐI

Trả về: tên nhánh + commit base + commit hash mới, tóm tắt thay đổi, bảng vật liệu, kết quả verify,
đường dẫn evidence, đường dẫn tài liệu, backup path, và bất kỳ điều gì đã làm khác với kế hoạch (kèm lý do).
Nếu bất kỳ bước nào không thực hiện được -> nói rõ, KHÔNG báo "đã hoàn thành" một cách chung chung.
