# UI-SKINS — danh sách cuộn cho trạm nâng cấp

Ngày: 11–12/11/2026, Asia/Bangkok. Windows Editor Unity 6000.5.8f1, URP, uGUI LegacyRuntime. Nhánh minhnhat/fix-shop-skins-scroll tạo trực tiếp tại d28ff063f7af5f8cf367f1e22fbf215e35642b04, không checkout nội dung nhánh khác khi Unity mở. Commit UI-SKINS là commit chứa README này, thông điệp fix(guardian): make upgrade shop skins tab scrollable. Không push/PR.

## Nguyên nhân và cấu trúc trước/sau
MCP trước sửa (structure-before.txt): Tab_Skins và Tab_Inventory là RectTransform trong Card, anchors (0.03,0.03)–(0.97,0.71), vùng 808.4×380.8 UI units. ScrollSkinsContainer/ScrollContainer đã có ScrollRect và Image, nhưng viewport=null, không Mask/RectMask2D, movement Elastic. SkinsList/ItemsList đã có VerticalLayoutGroup spacing8 và ContentSizeFitter vertical PreferredSize. Skins sinh 2 header 490×28 + 8 hàng 490×44; tổng 480 chưa kể padding, vượt 380.8. Inventory thử 2 đồ gốc + 9 Potion UI-SKINS cũng tràn (before-items.png).

Sau sửa hai tab dùng cấu trúc:
Tab -> ScrollContainer (ScrollRect vertical=true, horizontal=false, Clamped, inertia=true) -> Viewport (stretch toàn vùng tab, RectMask2D + Image trong suốt nhận raycast) -> Content cũ (SkinsList/ItemsList).
Content neo top stretch ngang, pivot top; VerticalLayoutGroup spacing8, padding ngang10/dọc8, childControlWidth=true, childControlHeight=false để giữ đúng size hàng runtime, childForceExpandWidth=true, childForceExpandHeight=false; ContentSizeFitter vertical PreferredSize. Viewport nằm dưới tab và trong Card, không đè nút đóng. Hàng giãn ngang theo chiều rộng viewport, chiều cao giữ nguyên (skins44, inventory48); không thay giá hoặc listener mở khóa/trang bị.

IUHUpgradeShopUI chỉ thêm hai reference ScrollRect; reset scroll top sau refresh, ForceRebuildLayoutImmediate/StopMovement/verticalNormalizedPosition=1. Khi xóa hàng cũ tách parent trước Destroy để layout không tính child cũ trong frame. Bỏ lightning khỏi header trail. Code diff: code.diff. Scene diff: scene.diff, chỉ shop; +204/−20 dòng, gồm 2 Viewport với RectTransform/Image/RectMask2D/CanvasRenderer, liên kết cha/con, viewport/content của ScrollRect, movement, padding/layout và 2 reference mới. Review độc lập xác nhận không thay giá/logic, không còn MinSize/MaxSize noise.

## Kiểm chứng a–e
| Mục | Kết quả chạy thật |
|---|---|
| a | Compile hoàn tất, Console không có lỗi code game; xem ghi chú lỗi công cụ dọn GameView bên dưới và kiểm tra cuối |
| b Ngoại trang desktop | 1920×1080: after-skins-top.png, after-skins-bottom.png; hàng cuối Ánh Sao Hoàng Kim trong viewport, inside=True |
| b Ngoại trang iPhone | Game view2532×1170: after-skins-iphone.png, inside=True |
| b Kho đồ | 11 item: after-items-top.png; ảnh cuối chính xác after-items-bottom-final.png và after-items-iphone-final.png, Potion UI-SKINS8 hiện trọn, inside=True |
| b Reset/kéo | Mở lại tab normalized=1; gửi BeginDrag/Drag/EndDrag qua ScrollRect đổi normalized xuống0.4858085 |
| c | Không bấm mua/trang bị/dùng item; Button cuối skins interactable=True, targetGraphic.raycastTarget=True, EventSystem.RaycastAll có Text/BtnAction; items có BtnUse tương tự |
| d EditMode | 74/74 pass, job86148a0f1d844f0abef6c308b2346892, editmode.json |
| d PlayMode | 22/22 pass, job3da22e82279e43dfacaf0a810fd3ef2f, playmode.json |
| d Cleanup | Snapshot PlayerPrefs trước thêm item test khôi phục 35/35 key, tổng35, xóa item test; EditorSettings khôi phục; GameView index11, gỡ 2 size tạm; không còn InitTestScene; scene sạch, Missing Script0 |
| e | Ảnh đã xem trực quan, bounds/raw tests/snapshot/diff giữ trong thư mục này |

Suite chạy init_timeout180000, poll60. Mở trạm qua OpenShop(2/1), cùng đường của handler B; không xác nhận riêng phím B vật lý. Không test trên thiết bị iPhone thật, chỉ mô phỏng độ phân giải trong Editor. Các ảnh skins có background combat/material magenta là vấn đề có sẵn, không sửa.

Bounds world corners chi tiết trong bounds.txt; dòng đầu có width0 là lượt chẩn đoán layout trước khi sửa childControlWidth. Kết quả sau sửa:
- Desktop skins cuối x486.96–1074.96, y233.76–286.56; viewport x474.96–1445.04, y224.16–681.12: inside=True.
- iPhone skins cuối x753.54–1390.54, y253.24–310.44; viewport x740.54–1791.46, y242.84–737.88: inside=True.
- Desktop inventory cuối x486.96–1398.96, y233.76–291.36: inside=True.
- iPhone inventory cuối x753.54–1741.54, y253.24–315.64: inside=True.
Một lần gameplay phát thưởng/check-in làm refresh tab trở về đầu giữa đo bounds và chụp, nên ảnh after-items-bottom.png/after-items-bottom-iphone.png không dùng làm bằng chứng cuối; đã chụp lại với Time.timeScale=0, sau đó trả1 và thoát Play. Hai ảnh *final là bằng chứng đúng cuối danh sách.

## Mobile
IUHMobileControlsUI.cs dòng359–366 đã kiểm IsOpen trong isAnyModalOpen và SetActive(!isAnyModalOpen) cho TouchLookZone. Không sửa mobile. Runtime khi shop mở TouchLookZone activeInHierarchy=False (activeSelf=True do nhánh UI mobile cha inactive trên Windows); mobile-drag.txt ghi thực tế. Kiểm luồng kéo ScrollRect qua PointerEventData thành công; không khẳng định đã thử cảm ứng trên iPhone thật.

## Bảo toàn và giới hạn
Trước thao tác: ngoài Play Mode, scene isDirty=false, Console0 error. Lệnh snapshot đầu dùng API GetInstanceID obsolete nên không thực thi; snapshot35 key lấy ở Play đầu, trước mọi AddItem/nút test. Play khởi tạo có thể thực hiện migration Items G2; không thay save mua/skin thật. Sau mỗi phiên kiểm thử đã restore snapshot35 key và EditorSettings. EditorSettings.before.txt và playerprefs-before.txt là snapshot.

6 dòng nhiễu MinSize/MaxSize được đưa đúng giá trị HEAD theo từng object ID (noise-excluded.txt), không sửa text khác. Material/Logs/csproj/slnx/ProjectSettings và file ngoài phạm vi không stage. Không sửa inventory/customizer/backend/Builder/Packages/framework. Không thêm test tự động cho thay đổi UI nhỏ; đã tái hiện ảnh trước, đo bounds/raycast/drag sau và chạy toàn bộ suite.

Lỗi công cụ tự gây: RemoveCustomSize nhận index tuyệt đối, lần đầu truyền index tương đối sinh Invalid index5 7 7 (cleanup-error.json). Đã sửa, gỡ đúng size index13/12, trả11; lưu lỗi rồi clear Console và yêu cầu compile mới để kiểm tra không tái diễn. Không che lỗi test/game. MCP ngắt lúc đọc Console trước sửa: chờ30 giây và thử lại thành công; một get_active timeout cuối lượt cũng chờ30 giây rồi đọc lại thành công. Không có job test bị khởi chạy lại.

Kiểm tra cuối sau compile mới: editor idle, ready_for_tools=true, is_compiling=false; Console0 error. Missing Script0, isDirty=false đã xác nhận qua MCP. Review độc lập không có vấn đề chặn.

Amend 12/11/2026 theo yêu cầu: hai list giãn ngang bằng childControlWidth/childForceExpandWidth=true; ảnh after-skins-bottom.png và after-skins-iphone.png chụp lại, thêm after-items-iphone.png. Bounds mới desktop skins x486.96–1433.04, iPhone hai list x753.54–1778.46, inside=True; nút/raycast đúng. Đã bỏ ảnh trùng top-1/bottom-1 và scene-before.zip khỏi commit. Các ảnh cũ khác giữ làm lịch sử; ưu tiên ba ảnh vừa cập nhật. PlayerPrefs khôi phục35/35.

