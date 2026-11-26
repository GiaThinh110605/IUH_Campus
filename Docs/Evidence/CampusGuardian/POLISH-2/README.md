# Bằng chứng Campus Polish 2 — Hàng rào, Bầu trời, Đèn đường, Ghế & Bồn hoa

- **Ngày:** 2026-11-27
- **Nhánh:** `minhnhat/campus-polish-2`
- **Môi trường:** Windows, Unity 6000.5.8f1 (URP), Unity MCP (`127.0.0.1:8081`)

## Các thay đổi chính
1. **Hàng rào chu vi đồng bộ (`Fence Modular System V3`):**
   - Thay toàn bộ hàng rào chu vi và 2 cánh hàng rào cũ hai bên cổng chính (`University main gate_30/Warm granite`, `Dark bronze`) bằng hệ hàng rào modular `sectionB_V3_PREFAB` + trụ `post_V3M_PREFAB`/`post_V3C_PREFAB` (94 tấm, 98 trụ) trên đế tường granite `1.05m`.
   - Ẩn 2 cây chắn hàng rào Nam (`Shade tree_29`, `Shade tree_28`), nối khít từ trụ cổng chính (`x = ±10.5`) ra 2 góc Nam (`x = ±56`) và đâm kín vào tường gạch đỏ Ký Túc Xá (`z = 12.8`) & tường Thư Viện (`z = 18.8`).
2. **Bầu trời Sáng / Đêm (`Fantasy Skybox FREE`) & Ánh sáng ban đêm:**
   - Đặt `FS003_Day.mat` làm bầu trời ban ngày chính của scene và gán `FS013_Night.mat` (trăng đỏ) trên `STORY_DIRECTOR`.
   - Cập nhật `IUHStoryNightShift.cs` và `IUHStoryDirector.cs`: hỗ trợ `SetNightSkyboxMaterial` chuyển cảnh sáng/tối mượt, hạ đồng thời 3 kênh Trilight (`ambientSkyColor`, `ambientEquatorColor`, `ambientGroundColor`) sang tông xanh đêm mát dịu (không bị ám đỏ/hồng).
3. **Đồng bộ 12 đèn đường cổ điển & Tự bật sáng ban đêm (`IUHStreetLampNightLight.cs`):**
   - Gỡ 10 đèn vuông cam cũ (`Garden beds and furniture_11`), thay bằng `SM_StreetLight_Modern_02.prefab` (`CAMPUS_STREET_LAMPS`) và dời 2 cột đèn ở `z = 24` sang `z = 20.5` để không đụng ghế.
   - Gắn `PointLight` (`ForcePixel`, `intensity = 26`, `range = 22m`), bóng phát sáng `Lamp_GlowBulb`, hào quang `Lamp_Halo` và quầng sáng mềm dưới đất `Lamp_GroundGlow` tự động bật/tắt theo `_IUH_NightAmount`.
4. **Thay 6 ghế băng bằng `Stylized Wooden Bench PBR` (`CAMPUS_WOODEN_BENCHES`):**
   - Tạo material chuẩn URP Lit `M_IUH_StylizedWoodenBench_URP.mat` (không sửa file gốc trong `Assets/SIV/`), xoay cả 6 ghế quay mặt ra lối đi trung tâm và dời `Stylized_Wooden_Bench_02` vào ngay trước bồn hoa Tây Giữa (`z = -1.20`).
5. **10 chậu hoa & 6 bồn hoa lớn + 2 bồn hoa trung tâm:**
   - Gỡ khối cầu cam/trắng cũ trên 10 chậu và trồng cụm hoa tự nhiên (`NATURE_POT_FLOWERS`).
   - Lấp đầy 950 cụm hoa trên 6 bồn hoa lớn (`NATURE_FLOWERBEDS_FILL`, 4 draw call) và đặt 6 cây tán kép rậm lá ở chính giữa 6 bồn (`NATURE_BED_CENTER_TREES`).
   - Cắt gọn phần hàng rào cây xanh thò ra ngoài viền đá trên 2 bồn hoa cong đài phun nước, bịt kín 4 đầu cắt (`NATURE_FOUNTAIN_HEDGE_CAPS`) và ẩn hàng bi vàng đồng (`Curved fountain gardens_101/Antique brass`).

## Kết quả kiểm thử
- **Unity Compile & Console:** `0` error.
- **Scene `IUH_SharedCampus_Concept.unity`:** `isDirty=false`, `GameObjects=4378`, `MissingScripts=0`.
- **EditMode Tests:** `150/150 Passed` (3.04s).
- **PlayMode Tests:** `75/75 Passed` (26.30s).
