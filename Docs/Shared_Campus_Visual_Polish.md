# Chi tiết và độ sắc nét khuôn viên

Áp dụng trực tiếp vào `Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity`.

## Thay đổi

- Chín bộ vật liệu albedo/normal 1024 px: đá mặt tiền, granite, hai loại đá lát, gạch terracotta, gỗ, cỏ, vỏ cây và lá bụi cây. Texture được lặp theo kích thước hình học, có mipmap và lọc anisotropic 8×.
- Mặt tiền trung tâm có thêm kính, cột, gờ ban công và lan can; hai cánh nhà có thêm chi tiết cạnh/sàn. Một phần cửa sổ có sắc phản chiếu khác nhau; thêm quốc kỳ và làm rõ biển tên.
- Thay các hàng bụi dạng chuỗi cầu bằng khối bo cạnh có bề mặt lá; thêm hoa trong bồn và chậu dọc lối đi.
- Dựng lại các mức LOD của tán cây với lá sáu đỉnh hơi cong và hướng pháp tuyến sáng hơn, thay lá hình thoi phẳng.
- Probe phản chiếu phục vụ kính/nước; nước có gợn, phản chiếu và điểm sáng từ mặt trời.
- Pipeline riêng cho bản xem PC: render scale 1,2, MSAA 4×, bóng mặt trời 4096 và SSAO. FXAA được tắt khi pipeline này chạy để tránh làm nhòe chồng thêm. Component scene trả lại pipeline và khử răng cưa trước đó khi bị tắt/thoát Play Mode; thiết bị mobile không bật cấu hình này tự động.

## Kiểm chứng

- Cấu hình thực trong Play: `Campus HQ Pipeline`, render scale 1,2, MSAA 4×.
- Bốn tuyến đi bộ và va chạm hồ vẫn đạt 5/5 bằng CharacterController thật trong Play Mode.
- Scene không mất script, không có mesh rỗng. Console cuối lượt không có lỗi.
- Sau thoát Play, pipeline trở lại `PC_RPAsset`. Hai asset PC gốc và scene chính không thay đổi trong lượt chỉnh này.
- Ảnh chụp Unity thật: `Evidence/SharedCampusPolish/play-final.png`. Báo cáo kiểm tra ở cùng thư mục.

Chưa đo FPS trong bản build hoặc kiểm chứng Android. Mức chi tiết vẫn là mô hình tạo trong dự án, chưa phải toàn bộ hệ tài nguyên tả thực như ảnh đích. Cấu hình cao được kiểm tra trên RTX 3060 Laptop 6 GB.

## Texture bầu trời và nguồn tạo

Texture mới được tạo bằng **công cụ ImageGen tích hợp**, không dùng CLI/API fallback. Nó là ảnh môi trường LDR, không phải ảnh HDR có dải sáng vật lý.

File dự án: `Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Textures/Campus_day_sky.png`.

Material: `Assets/IUH_Campus/SharedCampusConcept/VisualPolish/Materials/Photographic campus sky.mat`, shader `Skybox/Panoramic`, texture gắn vào `_MainTex`. Ảnh texture được dùng trong bầu trời và phản chiếu của scene 3D; ảnh kết quả cuối là ảnh render từ Unity, không phải ảnh campus do AI vẽ.

Prompt đã dùng:

> Use case: photorealistic-natural. Asset type: game skybox environment texture for a Unity campus scene. Create a photorealistic seamless equirectangular 360 degree panorama, 2:1 aspect ratio, showing ONLY an azure blue daytime sky with beautiful scattered small white cumulus clouds and softly shaded blue-gray undersides, natural fine cloud edges, bright warm afternoon sunlight. Horizon must be level exactly at the vertical midpoint; upper half is open blue sky with clouds, lower half fades to a uniform pale sky-blue atmosphere with no objects or landscape. Clouds mainly in the middle elevation band of the upper hemisphere, clear zenith. Horizontal left/right edges should join seamlessly. No buildings, no land, no trees, no people, no text, no logos, no UI, no frame. This is a flat sky environment texture, not a rendered sphere, not a campus illustration. Natural photography, fine crisp realistic cloud detail, no painterly noise or wispy marbling. Save the result as an image asset.

Nguồn dựng chi tiết: `Assets/IUH_Campus/Editor/IUHSharedCampusVisualPolish.cs`. Bản sao scene trước sửa giữ cục bộ tại `Docs/Backups/IUH_SharedCampus_before_visual_polish.unity`.
