# Visual polish — P1, P2, P4, P5

Ngày ghi nhận: 23/11/2026 (Asia/Bangkok). Nhánh **minhnhat/visual-polish**, nối tiếp nhánh/PR NPC tại **1a55512**; không push, không amend. Theo chỉ đạo mới nhất, **bỏ P3**, dành khói cho lượt Asset Store sau. Không triển khai prompt Asset Store trong lượt này.

| Phần | Commit | Kết quả |
|---|---|---|
| P1 | 041864d | Grounding phần hình player |
| P2 | 840470b | Grounding chung bốn prefab, sáu NPC |
| P3 | Không có | Bỏ theo yêu cầu; khôi phục code/ref khói cũ |
| P4 | 1a84162 | Thay hình mascot, giữ gameplay và mũ cũ |
| P5 | Commit chứa README này | Bằng chứng và prompt gốc |

Môi trường: Unity 6000.5.8f1, Windows Editor, URP, Direct3D12, RTX 3060 Laptop GPU. Nguồn AC, pin 100%, chế độ Turbo: [power.json](power.json). Đây là kiểm tra Editor PC, chưa build/chạy thiết bị iOS/macOS.

## P1 và P2: đo và sửa phần hình

Khoảng lệch = min Y mesh giày đã skin trừ mặt sàn nhìn thấy; dương là bay, âm là lún, đơn vị m. Bảng là min/max mẫu, không phải số làm tròn từ xương bàn chân. Mặt mesh sàn nhìn thấy khác collider campus ở Y=0: sân lát 0,03/0,045 m, canteen 0,24 m, một số cỏ nâng 0,36 m. Ray vật lý đơn thuần vì vậy không đủ để sửa phần hình.

Giữ nguyên CharacterController: skinWidth=0,02; center=(0;0,93;0); height=1,8; ThirdPersonController.GroundedOffset=+0,14. Không đổi controller, camera, Starter Assets hay FBX/VRM nguồn. Grounder chỉ dịch các nhánh rig/mesh trực tiếp; khôi phục base trước animation, hiệu chỉnh sau UniVRM LateUpdate. Dữ liệu mặt sàn có 668 tam giác với cache ô XZ 4 m; tối đa 128 đỉnh vùng đế giày được skin mỗi lần đo. Bốn prefab dùng cùng component và surface, không chỉnh tay vị trí sáu NPC.

| Đối tượng/trạng thái | Trước (m) | Sau (m) |
|---|---:|---:|
| Player paving, Idle | 0.41700 … 0.41700 | 0.00198 … 0.00198 |
| Player paving, Walk | 0.40604 … 0.41700 | 0.00187 … 0.00198 |
| Player paving, Run | 0.39799 … 0.53400 | 0.00178 … 0.00314 |
| Player lawn, Idle | 0.44088 … 0.44088 | 0.00198 … 0.00198 |
| Player lawn, Walk | 0.43202 … 0.44088 | 0.00188 … 0.00198 |
| Player lawn, Run | 0.42590 … 0.54463 | 0.00180 … 0.00310 |
| Player fountain, Idle | 0.31470 … 0.31470 | 0.00198 … 0.00198 |
| Player fountain, Walk | 0.30559 … 0.31470 | 0.00187 … 0.00198 |
| Player fountain, Run | 0.29977 … 0.41196 | 0.00179 … 0.00293 |
| Student_01, tại vị trí gốc | -0.24622 … -0.24332 | 0.00149 … 0.00173 |
| Student_02, tại vị trí gốc | -0.24782 … -0.24554 | 0.00205 … 0.00207 |
| Student_05, tại vị trí gốc | -0.05119 … -0.04829 | 0.00149 … 0.00173 |
| Student_06, tại vị trí gốc | -0.05281 … -0.05060 | 0.00205 … 0.00207 |
| Student_11, tại vị trí gốc | -0.00638 … -0.00638 | 0.00201 … 0.00201 |
| Student_12, tại vị trí gốc | -0.00090 … -0.00090 | 0.00193 … 0.00193 |
| Student_11, đang Walk, camera theo | -0.07526 … -0.00765 | 0.00201 … 0.00204 |
| Student_12, đang Walk, camera theo | -0.39085 … 0.00515 | -0.00079 … 0.00240 |
| Mascot Idle, toàn chu kỳ bob | 0.16500 … 0.32500 | -0.08080 … 0.07916 |
| Mascot dance | Không đo baseline | -0.02488 … 0.40076 |

Nguồn: [player](player-feet-samples.json), [sáu NPC](npc-feet-samples.json), [hai NPC Walk](npc-walking-feet.json). Bốn NPC đứng tại chỗ dùng Idle; hai NPC tuần tra được đo thêm khi walking=true, camera theo để Animator không bị cull. Đo xa không cập nhật Animator được giữ riêng trong mẫu, không dùng làm bằng chứng Walk.

Idle player/NPC đạt ±0,02 m; các mẫu Walk/Run player và hai NPC tuần tra đạt ±0,03 m. Đây là dịch toàn bộ phần hình theo đế thấp nhất, không phải IK độc lập từng chân; trên bậc hoặc mặt nghiêng, chân còn lại vẫn có thể lệch.

Arena proxy: khoảng lệch -0,0008545 m; grounder trên proxy=0: [P1-arena.json](P1-arena.json). Probe 9 s đi/chạy/nhảy đã airborne rồi landed, max root Y=1,96236 m, khoảng cách camera–đầu nhỏ nhất 3,98147 m: [P1-gameplay.json](P1-gameplay.json).

## P4: mascot và rà hình người tạm

Giữ IUHMascotNPC, m_ModelTransform, tương tác, billboard, particles, đèn và quest items. Chỉ thêm NPC04 dưới Mascot_Visual, root local Y=-0,06512587 m, scale=1,08999407. NPC04 khác Student_11/model03 và Student_06/model02 là hai NPC gần tâm đài phun nước nhất. Animator AC_AmbientStudent Idle; root motion tắt; Vrm10Instance tắt nên SpringBone không chạy. IUHAmbientStudent và grounder riêng của bản mascot tắt để không chống lại chuyển động parent của script gốc. Tái sử dụng hai mesh mũ GradCap_Base/GradCap_Diamond, parent vào J_Bip_C_Head, giữ vật liệu M_Mascot_GradCap. [Cấu hình cuối](mascot-final.json).

Các hình người tạm được xác định: Body capsule, Head sphere và Visor_Eyes dưới IUH_MASCOT_ROBOT tại (4,5;0,1;-6), script gameplay IUHCampus.DailyActivities.IUHMascotNPC trên owner. Đã tắt phần hình cũ, không xóa. [Tên/vị trí world từng phần](temporary-people-audit.json). Students in courtyard_102 vốn tắt nên bỏ qua. Không phát hiện thêm hình người primitive/Student low-poly đang bật cần tắt; sphere của drone/đạo cụ không phải người.

Bob Idle hardcode 0,08 m, đo sau căn lại khoảng -0,08080…+0,07916 m, đạt ngưỡng bob ±0,1 m. **Ngoại lệ giữ nguyên:** dance hardcode 0,4 m, đo đỉnh +0,40076 m, vượt 0,1 m. Không có field serialized để giảm; prompt cho phép giữ nguyên và ghi lại, nên không sửa script. Vì bob chủ ý còn giữ, mascot không đứng sát sàn trong mọi khung hình như NPC thường.

Gọi Interact() mở hội thoại thành công, CloseMascotDialog() đóng; trạng thái nhiệm vụ NotStarted giữ nguyên, không nhận/nộp quest hay phát thưởng. Đây là **API fallback**, không ghi Pass cho phím E vật lý. [Mẫu đo và tương tác](mascot-check.json). Đã sửa fixture kiểm tra bằng cách bật lại CharacterController trước khi HUD khôi phục input; lần kiểm cuối Console 0 error. Đèn cyan gốc vẫn giữ nên mascot sáng xanh mạnh, đặc biệt ban đêm.

## A/B hiệu năng

Mỗi task hai góc lobby/overview từ ART-ENVIRONMENT/cameras.json; cùng phiên A,B,A,B,A,B; mỗi lượt 120 frame warmup + 3000 frame đo; 1280×900, VSync=0. Giá trị bảng là trung vị ba lượt mỗi cấu hình, p95 là trung vị ba p95. Ngưỡng Δmedian≤0,5 ms; Δp95≤1 ms; Δdraw≤20. A/B P4 có khôi phục cả pose/scale mũ cũ ở A. Sau A/B chỉ căn Y model thêm -0,027 m để đưa bob về ±0,1 m; không đổi renderer/material/logic.

| Task/góc | Median A → B (Δ ms) | p95 A → B (Δ ms) | Draw A → B | Ngưỡng |
|---|---:|---:|---:|---|
| P1 lobby | 4.8545 → 4.9298 (0.0753) | 5.5834 → 5.6818 (0.0984) | 352 → 352 | Đạt |
| P1 overview | 5.4015 → 5.5104 (0.1089) | 6.1004 → 6.4454 (0.3450) | 992 → 992 | Đạt |
| P2 lobby | 4.5938 → 4.7802 (0.1865) | 5.3593 → 5.4152 (0.0559) | 286 → 286 | Đạt |
| P2 overview | 5.2262 → 5.4446 (0.2184) | 5.9433 → 6.0371 (0.0938) | 992 → 992 | Đạt |
| P4 lobby | 4.9778 → 5.0605 (0.0827) | 5.6853 → 5.8681 (0.1828) | 273 → 273 | Đạt |
| P4 overview | 5.6682 → 5.7031 (0.0348) | 6.4660 → 6.3711 (-0.0949) | 943 → 938 | Đạt |

[P1 TSV](P1-AB.tsv), [P2 TSV](P2-AB.tsv), [P4 TSV](P4-AB.tsv), [tổng hợp JSON](performance-summary.json). P1-AB-initial-camera-diagnostic.tsv có actor ngoài góc nhìn, không dùng kết luận; bảng dùng P1-AB.tsv đo lại khi actor trong hình.

## Kiểm thử cuối

- EditMode: **137/137**, failed=0, skipped=0; job 4c4d9a70dab644e2b99ca3dff6ffecdf.
- PlayMode: **59/59**, failed=0, skipped=0; job ca7e090b6a1c48af9a711f1d493466e9. Gồm ba test grounding thực: NPC Idle/Walk và player Idle; kiểm khoảng cách giày và controller/root không đổi. Đã kiểm red/green khi triển khai P1.
- Missing Script: **0** ở campus, combat arena, AR encounter: [kết quả](missing-scripts.json).
- Sau lọc scene và mở lại: Edit Mode, scene isDirty=false, compile xong, Console 0 error: [verification.json](verification.json).

Chỉ stage scene của task: component player và subtree mascot mới; MainCamera Y=3,2066925 có sẵn còn unstaged. Không stage tám material NPC tự ghi, material khác, csproj/slnx, Logs/ProjectSettings hay Assets/Screenshots. Native save đã được lọc bỏ việc đảo thứ tự document; file scene được mở lại và kiểm tra.

## P3 đã bỏ

Code IUHStoryCorruptionZone và StoryScript_DemGlitch.asset đã khôi phục; fogMaterial vẫn là M_Story_CorruptionFog cũ. Shader/material/texture/test smoke thử nghiệm đã gỡ; không có commit P3, không đổi URP toàn cục. Không có thông số khói mới được bàn giao. Probe cũ cho thấy alpha quá loãng: mask max≈0,4745, mean≈0,084; nhân particle peak 0,3 còn max≈0,143/mean≈0,025 trước fade. Kết quả đó chưa đạt yêu cầu hình ảnh và đã dừng theo chỉ đạo mới.

Các file smoke-*, P3-AB* trong working folder chỉ là chẩn đoán trước khi hủy, **không phải ảnh/kết quả của sản phẩm cuối và không được stage**. Asset Store sẽ xử lý ở lượt riêng.

## Ảnh trước/sau

| Nội dung | Trước | Sau |
|---|---|---|
| Chân player ban ngày | [Trước](player-feet-BEFORE-day.png) | [Sau](player-feet-AFTER-day.png) |
| NPC canteen | [Trước](npc-group-canteen-BEFORE-day.png) | [Sau](npc-group-canteen-AFTER-day.png) |
| NPC mainfront | [Trước](npc-group-mainfront-BEFORE-day.png) | [Sau](npc-group-mainfront-AFTER-day.png) |
| Mascot ngày | [Trước](mascot-BEFORE-day.png) | [Sau](mascot-AFTER-day.png) |
| Mascot đêm | [Trước](mascot-BEFORE-night.png) | [Sau](mascot-AFTER-night.png) |

Khói sau: không cung cấp vì P3 đã bỏ. Prompt gốc: [Prompt_Visual_Polish_Feet_Smoke.md](../../../Prompts/Prompt_Visual_Polish_Feet_Smoke.md); phạm vi mới nhất của người dùng ưu tiên hơn P3 trong prompt.
