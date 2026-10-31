# Hybrid ambientCG — 31/10/2026

Đã thực hiện trên nhánh `codex/ambientcg-graphics-upgrade`, giữ nguyên marker. Không chạy lại Apply PBR hoặc Tune. Chưa stage, chưa commit, chưa push.

## Thay đổi

Menu mới: `IUH Campus/Concept/Apply ambientCG Hybrid Surface Pass`, file `Assets/IUH_Campus/Editor/IUHSharedCampusAmbientCGHybrid.cs`.

Thay đúng 7 slot trên 7 MeshRenderer: Sandstone paving về Courtyard sandstone, Paving pale về Pale paving slabs, Lawn về Detailed lawn. Chỉ thay slot đang trỏ trực tiếp material ambientCG tương ứng. Không sửa asset VisualPolish hoặc texture/tiling của chúng, không đổi props.

Giữ AmbientCG facade, brick, wood. Scale lần lượt 1.5×1, 2×1.25, 1.5×1.5 trên cả ba map; smoothness .16/.14/.28, bump .5/.75/.6. BaseColor giữ mức tuned theo yêu cầu. Hai material dự phòng giữ nguyên.

Khôi phục đúng bytes profile từ `Docs/Backups/Campus_volume_before_ambientcg_20261031_184254.asset`. SHA256 profile và backup đều `A1F5CCA516B7E6FD51DE7529595B56C64F75DE1C4360EDB390BD513F5F4CF841`. Profile có Tonemapping ACES, ColorAdjustments, Bloom; không Vignette. Không đổi đèn/skybox/pipeline.

Backup mới trước hybrid:
- `Docs/Backups/IUH_SharedCampus_Concept_before_ambientcg_hybrid_20261031_204358.unity` — SaveScene copy từ scene hiện tại, giữ cả trạng thái chưa lưu trước pass.
- `Docs/Backups/Campus_volume_before_ambientcg_hybrid_20261031_204358.asset`.

## Kiểm chứng

Đã xem ba góc ưu tiên trước gameplay: paving/lawn trở về diện mạo before, không tối/nâu hoặc vụn hơn ở bề mặt được trả lại. Giữ AmbientCG ở khối đứng nên facade/gạch có sắc độ khác before. Đã xem ảnh tổng thể Play Mode, không thấy mesh hồng; kiểm tra material slot cũng không có shader lỗi/thiếu/không hỗ trợ.

Play Mode: 4/4 tuyến đạt, 2.582 bước `CharacterController.Move`: qua cổng; vòng phía tây đài phun đến sảnh; cổng đến căn tin; cổng lên sân khấu. Sân khấu kết thúc y≈0,69 m. Đây là kiểm tra solver bằng CharacterController thật, không phải kiểm tra thao tác bàn phím thủ công hoặc benchmark FPS. Giữ/khôi phục trạng thái nhân vật trong test, không lưu thay đổi runtime.

Snapshot trước/sau hybrid: 848/848 component, gồm 822 campus cũ, 25 props/root và 1 Transform marker. Có đúng 7 entry thay đổi sharedMaterials theo mapping; sau khi bỏ trường sharedMaterials, 0 thay đổi bất ngờ. Không thêm/xóa/disable component, không thay transform, mesh/UV, collider, camera hoặc player. 6 props còn nguyên.

Console sau Play: 0 Error, 0 Warning được trả về. Scene đã lưu, Edit Mode, dirty=false. Đã đọc lại scene trên đĩa và xác nhận GUID material VisualPolish thật trong file. Mtime/size/SHA256 trước/sau: `disk-verification.txt`.

## Ảnh cùng góc

| Góc | Before | Hybrid |
|---|---|---|
| Tổng thể | [Before](../before-overview.png) | [Hybrid](hybrid-overview.png) |
| Đá lát | [Before](../before-paving-closeup.png) | [Hybrid](hybrid-paving-closeup.png) |
| Cỏ | [Before](../before-grass-closeup.png) | [Hybrid](hybrid-grass-closeup.png) |
| Mặt tiền | [Before](../before-facade-closeup.png) | [Hybrid](hybrid-facade-closeup.png) |
| Căn tin | [Before](../before-canteen.png) | [Hybrid](hybrid-canteen.png) |
| Sân khấu | [Before](../before-stage.png) | [Hybrid](hybrid-stage.png) |

Có thêm bộ `play-hybrid-*` trong Play Mode. Mapping, snapshot và tuyến đi nằm cùng thư mục báo cáo.

Chưa commit vì đầu yêu cầu ghi không commit, dù phần cuối cho phép có điều kiện. Giữ toàn bộ thay đổi local để người dùng duyệt; không stage các thay đổi từ lượt trước.

## Chuẩn bị commit đã được duyệt

Người dùng đã cho phép commit AmbientCG/hybrid cùng đúng 24 asset dependency props, `.meta` tương ứng và `.meta` thư mục cần thiết. Audit GUID trực tiếp (kể cả model importer `.meta`) và AssetDatabase.GetDependencies đạt: không còn dependency scene thiếu trong phạm vi chọn. Hai GUID package được xác nhận là URP Lit và AssetVersion. Không thêm toàn bộ Asset Store pack; không thêm backup, log hoặc thay đổi dự án không liên quan. Không push. Các ghi chú “chưa commit” phía trên mô tả thời điểm kiểm chứng trước khi được duyệt.
