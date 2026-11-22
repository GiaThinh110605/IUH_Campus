# VRoid player — kết quả cuối

Ngày 23/11/2026, nhánh `minhnhat/vroid-player`. Unity 6000.5.8f1 Windows Editor, URP, Direct3D12, RTX 3060 Laptop GPU. UniVRM **0.131.4**. Không push.

## Thay model và vật liệu

Task 3: `44171bb feat(player): replace Casual2 with VRoid student`. Animator, Humanoid và Vrm10Instance nằm ở gốc Campus Student; rig/mesh VRoid trực tiếp dưới gốc. Giữ controller Starter Assets, capsule 1,80 m và PlayerCameraRoot. Scale rig 1,01060891 để cao khoảng 1,73 m. Súng gắn tay phải mới. Giữ 32 spring tóc/áo. Prefab và scene cùng dùng hai material đã extract trong `Materials/PlayerVRoid`. File VRM không đổi.

MToon cả hai material: Lit RGB 0,85; Shade RGB (0,65;0,67;0,72); GI Equalization 0,5; Shading Toony 0,7; Shift 0; Emission RGB 0. Không sửa Campus volume. GraphicsSettings còn đúng một dòng Always Included MToon10.

| So sánh | Trước chỉnh sáng | Sau chỉnh sáng |
|---|---|---|
| Ngày | [Ảnh](Idle-front-day-AFTER.png) | [Ảnh](Resume/Idle-front-day-TUNED-2.png) |
| Đêm | [Ảnh](Idle-front-night-AFTER.png) | [Ảnh](Resume/Idle-front-night-TUNED.png) |

Ảnh trước thay model: [đứng](Idle-front-day-BEFORE.png), [chạy sau lưng](Run_N-back-day-BEFORE.png), [nhảy](JumpStart-front-day-BEFORE.png). Ảnh controller thực sau thay: [đi](Resume/live-0.png), [chạy](Resume/live-1.png), [nhảy](Resume/live-2.png), [tiếp đất](Resume/live-3.png). Ảnh clip mẫu ở thư mục này không thay thế bằng chứng controller.

## Hiệu năng

1280×900, VSync tắt, camera gameplay nhìn thấy nhân vật, không thay targetTexture. Mỗi cấu hình 3 lượt, mỗi lượt 120 frame làm nóng và 3.000 frame đo. Casual2/VRoid xen kẽ; sau đó 3 lượt VRoid không spring. Ghi thời gian thực giữa frame render camera gameplay. Số dưới là trung vị gộp 9.000 frame/cấu hình, p95 frame và số render của Editor.

| Cấu hình | FPS | p95 ms | Draw calls | Triangles |
|---|---:|---:|---:|---:|
| Casual2 | 196,09 | 7,0136 | 654 | 2.420.507 |
| VRoid | 191,88 | 7,0775 | 639 | 2.515.799 |
| VRoid không SpringBone | 195,51 | 7,4074 | 639 | 2.515.799 |

Giảm FPS **2,15%**, đạt ngưỡng ≤10%. Tắt spring cải thiện trung vị khoảng 1,89% (~0,10 ms), nhưng p95 không cải thiện và các lượt có nhiễu; không coi đây là ước lượng nhân quả chính xác. Không cần giảm spring. [Từng lượt](Resume/Controlled/summary.txt), [tọa độ và cấu hình](Resume/Controlled/setup.txt), [chi tiết phương pháp và lệch kế hoạch](Resume/README.md).

## Kiểm gameplay

- Đi/chạy/nhảy/tiếp đất bằng ThirdPersonController thật qua StarterAssetsInputs: tốc độ đạt 6,2, cuối chu kỳ grounded; Console không có lỗi OnFootstep/OnLand. Không xác nhận âm thanh bằng nghe trực tiếp.
- SpringBone giữ đủ 32 spring; [đo lúc chạy/nhảy](Resume/secondary-live.tsv) cho thấy các xương secondary không đổi local position, xương tóc cách đầu tối đa khoảng 0,383 m rồi trở lại khoảng 0,216 m khi đứng. Ảnh nhảy không thấy tóc/áo kéo văng; chưa có video hay kiểm tra xuyên vải ở mọi tư thế.
- Camera cách đầu tối thiểu khoảng 3,98 m trong [chu kỳ controller](Resume/controller.tsv); không cam kết mọi góc quay.
- [Arena](Resume/arena.png): vào qua TriggerCombat Nhà E, Arena_Student có 2 skinned mesh và Avatar hợp lệ; Flee trả về exploration. Nhân vật nhỏ trong góc toàn arena, chưa chỉnh camera/scale arena vì ngoài phạm vi lượt này.
- Đã mở và kiểm ảnh [cửa hàng](Resume/shop.png), [kho](Resume/inventory.png), [luống 3](Resume/garden.png); kiểm qua API UI thật, không mua/sử dụng/trồng, chưa thử phím tương tác vật lý.
- Chuyển ngày/đêm qua StoryNightShift. Các fixture/input thay đổi trong Play Mode, không lưu vào scene.

## Dọn Casual2

Quét **16 GUID**, **0 tham chiếu ngoài** trước khi xóa đúng `ThirdParty/Quaternius/UltimateModularMen/`, `Materials/Player/` và `Prefabs/Player/PF_Student_Casual2.prefab` cùng meta. [Audit](Resume/casual-guid-audit.json). Cả 29 file có trong Git HEAD trước xóa. Chuyển StudentModelTests sang kiểm VRoid (Avatar, 2 mesh, rootBone, vật liệu đầy đủ), bỏ credit Casual2; giữ credit các gói Quaternius khác. Không tải asset mới.

## Kiểm thử và sự cố công cụ

| Mốc | EditMode | PlayMode |
|---|---:|---:|
| Task 1 — `b35530e` | 128/128 | 49/49 |
| Task 2 — `8a6c034` | 129/129 | 49/49 |
| Task 3 — `44171bb` | 129/129 | 49/49 |

Task 3 jobs: EditMode `e5f41615d9ad4d07a7006dc231e4ba47`; PlayMode `537e22a96cf6406d94778390080331ac`. Missing Script = 0 ở campus, arena, AR; scene sạch. Kết quả kiểm cuối sau dọn được ghi thêm bên dưới khi job hoàn tất.

Ba lỗi RenderPass cũ không tái hiện khi chơi/đo bình thường. Công cụ chụp MCP từng gây 5 lỗi PlayerLoop recursive tại ScreenshotUtility dòng 197, đã lưu [log](Resume/PlayerLoop-errors.log). Sau người dùng khởi động lại MCP: kiểm lại Console sạch, dùng ScreenCapture bất đồng bộ ở lượt gameplay mới, không phát sinh lỗi tương tự. Không sửa code package. Một lần bắt đầu EditMode bị ngắt bởi domain reload trước khi tạo job; chạy lại thành công, không tính lần ngắt là Pass.

Review độc lập Task 3 không phát hiện Critical/Important. MainCamera y=3,2066925 vẫn là thay đổi riêng chưa stage; commit scene chỉ thuộc Campus Student. Không stage các material tự ghi, csproj/slnx, Logs, ProjectSettings khác hay các file ngoài phạm vi.

Kiểm cuối sau dọn: **EditMode 129/129**, job `e10667b2cbae470db3703969ecb63319`; **PlayMode 49/49**, job `64dfa982ffa64087a4b40db20dc6db26`. Console 0 error, Missing Script 0 cả campus/arena/AR, isDirty=false, không compile, Edit Mode. [Bản ghi](Resume/final-verification.json). Reviewer độc lập kiểm lại cleanup: 0 tham chiếu cho 16 GUID trong Assets/Packages/ProjectSettings, không phát hiện Critical/Important.
