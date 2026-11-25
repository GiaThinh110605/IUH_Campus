# Playtest fix — kế hoạch

Base 5433873 trên minhnhat/vfx-pass, 24/11/2026. Không push/amend; không tạo nhánh mới. Parent độc quyền MCP/scene; hỗ trợ chỉ viết candidate trong Temp.

- [ ] F1: ghi từng frame run5s/jump3 lần/border, grounder ON/OFF; xác định riêng jitter/visibility/planter; regression RED rồi sửa, đo lại và A/B ≤0,3ms.
- [ ] F2: nguyên prefab smoke, nhiều cụm unitScale/mode gốc, budget80/30, API/Purify2s; sphere phải bỏ kể cả khói không đạt; ảnh/test/A-B.
- [ ] F3: kiểm OBJ/GLTF rig/clip bằng native; dùng Idle có sẵn trên visual riêng, không đổi nguồn/logic; campus/arena ảnh/test/A-B.
- [ ] F4: world-space pool12, vị trí body proxy, tối đa3 dòng mỗi hook; không camera/RT/UIparticles; test50 lần, ảnh và A-B.
- [ ] F5: hào quang rõ ban ngày, trần24/12 tổng mỗi quái, cao≤1m; ảnh/A-B.
- [ ] F6: fulltests riêng, Missing0 ba scene, Console0/Edit sạch; README số đo/nguyên nhân/ngoại lệ/Thịnh files/prompt; commit chọn lọc.

Chỉ lưu scene sau khi task đạt. Task không tiến triển30phút/2vòng không đạt thì rollback/bỏ (F2 vẫn bỏ sphere). MCP mất phản hồi ghi RESUME và dừng, không viết tay scene.
