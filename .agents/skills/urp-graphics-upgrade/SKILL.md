---
name: urp-graphics-upgrade
description: Nâng cấp đồ họa URP (post-processing Volume, URP asset, ánh sáng, material, Quality) cho IUH Campus trên PC/Mac và iOS, ưu tiên sửa asset thay vì scene, có kiểm tra trước/sau và giữ hiệu năng mobile.
---

# URP Graphics Upgrade — IUH Campus

Dự án: Unity 6000.5.8f1, URP 17.5, scene chính `Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity`.
Nền tảng: PC/Mac (`Assets/Settings/PC_RPAsset.asset`) và iOS (`Assets/Settings/Mobile_RPAsset.asset`).

## 1. Luật phạm vi (bắt buộc)
- **Ưu tiên sửa ASSET**: VolumeProfile, URP asset, Renderer, material, skybox material, Quality settings. Các file này tách khỏi scene, ít conflict.
- **Hạn chế sửa scene** (Thịnh đang tích hợp). Việc nào bắt buộc sửa scene (đèn, probe, vật thể) → gom thành một lượt riêng, báo người dùng trước. Kiểm tra `isDirty=false` trước khi bắt đầu.
- Không đổi render pipeline, không đổi package, không sửa file FBX/GLTF nguồn, không tải asset ngoài khi chưa được duyệt (xem skill `free-asset-sourcing`).
- Mỗi thay đổi phải có ảnh **before/after cùng góc camera** (lưu vị trí camera vào `cameras.json`) trong `Docs/Evidence/CampusGuardian/G*/`.
- Luôn đo lại hiệu năng sau thay đổi; ghi rõ đo trong Editor hay build.

## 2. Pre-flight post-processing (kiểm tra theo thứ tự)
1. URP đang active (`UniversalRenderPipeline.asset != null`).
2. `supportsHDR = true` trên URP asset đang dùng (cần cho Tonemapping; Bloom ở SDR phải để threshold < 1).
3. Camera chính có `UniversalAdditionalCameraData.renderPostProcessing = true`; chỉ camera Base (hoặc Overlay cuối stack) bật post-processing; Renderer có PostProcessData khác null.
4. Layer của GameObject chứa Volume nằm trong `volumeLayerMask` của camera (mặc định chỉ layer Default).
5. Volume `enabled`, có profile, có ít nhất một override.
6. Xem kết quả ở **Game view** (Scene view có toggle post-processing riêng).

## 3. API đúng (tránh nhầm với Post Processing Stack v2 cũ)
| SAI | ĐÚNG |
|---|---|
| `PostProcessVolume`, `PostProcessLayer` | `Volume` (UnityEngine.Rendering), `UniversalAdditionalCameraData.renderPostProcessing` |
| `profile.GetSetting<T>()` | `profile.TryGet<T>(out var t)` |
| `profile.AddSettings<T>()` | `profile.Add<T>(true)` — kiểm tra `profile.Has<T>()` trước |
| sửa `volume.profile` để lưu asset | sửa `volume.sharedProfile` hoặc load VolumeProfile asset (lưu ra đĩa); `volume.profile` tạo bản sao runtime |
- Mỗi `VolumeParameter` phải đặt `overrideState = true` trước khi gán `value` — lỗi phổ biến nhất.
- Sau khi sửa asset bằng code: `EditorUtility.SetDirty(asset)` + `AssetDatabase.SaveAssets()`.

## 4. Công thức gợi ý cho IUH Campus (ban ngày, sáng sủa, thân thiện)
- **PC/Mac**: Tonemapping ACES; ColorAdjustments postExposure 0–0.3, contrast 8–15, saturation 5–15; Bloom threshold 1.0, intensity 0.2–0.5, scatter 0.6; Vignette 0.15–0.25; SSAO (renderer feature) intensity vừa phải; soft shadows, 3–4 cascades, shadow distance ~80–120 m.
- **iOS**: Tonemapping Neutral hoặc ACES; Bloom nhẹ (intensity ≤ 0.3) hoặc tắt; không FilmGrain, MotionBlur, DepthOfField, ChromaticAberration; SSAO tắt hoặc rất thấp; 2 cascades, shadow distance 40–60 m, shadow resolution 1024–2048; render scale 0.8–1.0; MSAA 2x hoặc tắt.
- Mỗi nền tảng một VolumeProfile riêng nếu cần, chọn theo Quality level — không nhồi hiệu ứng nặng vào profile dùng chung.
- Ánh sáng: một Directional Light làm mặt trời (góc 35–55°, màu hơi ấm), ambient từ skybox/gradient; tránh cháy trắng tường sáng màu (kiểm tra histogram/vùng trắng).
- Fog nhẹ (linear hoặc exponential mật độ thấp) giúp chiều sâu; kiểm tra không làm mờ UI/gameplay.

## 5. Material & texture
- Bề mặt lớn (sân, đường, cỏ, tường) dùng PBR có Normal (+AO nếu có); tiling hợp lý để không lặp lộ.
- Texture import: Max Size 1024–2048 cho bề mặt lớn, 512–1024 cho đạo cụ; mipmaps bật; override iOS dùng ASTC (6x6 hoặc 8x8). Normal map đánh dấu Texture Type = Normal map.
- Không tạo material trùng; tái dùng material có sẵn trong `Assets/IUH_Campus/**`.

## 6. Kiểm tra sau mỗi lượt
- Console 0 error; không material hồng; scene campus Play Mode 20–60 s vẫn chơi được (camera, input, UI đọc rõ).
- Ảnh after cùng góc với before; bảng FPS/draw calls/tris trước–sau.
- Commit chỉ asset đã đổi + evidence; không stage thay đổi Unity tự ghi không liên quan.
