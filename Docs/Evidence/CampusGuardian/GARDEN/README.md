# GARDEN — bằng chứng đang thực hiện

19/11/2026 · Windows Editor · Unity 6000.5.8f1 · minhnhat/flower-garden · base 73d05da.

- Baseline EditMode 104/104 Pass (3c44557a2b0a4bf2aacbca6067e26420) và PlayMode 30/30 Pass (b6bb371083de4498b37142f79fb1ae06). Scene ban đầu isDirty=false, Console 0 error.
- Task 1: 97e51e9. RED lỗi compile thiếu GardenSave/GardenBed. GREEN EditMode 10/10 (b087ef3ef5e14ae19156b6e7d7dcf5c5).
- Task 2: 5fbccba. RED 3 ca thiếu manager (847d2da55e55479b80eb28915c865b69). GREEN PlayMode 3/3 (12492d56fbf24ceab7102ab4208416ea). Kiểm thiếu vàng, đủ vàng, bón phân, lưu/nạp và giờ tua.
- Task 3 chưa commit: test RED 2 ca do thiếu IUHGardenPanelUI (1c5bbf522de94f3f902bf6155a1a0581). Đã có prefab, UI, sáu instance trong scene. Compile sạch. EditMode toàn dự án 114/114 Pass (8f9fd6bd2b3c44edaaac259185a0bea2). Sau lọc scene, đã mở lại trong Unity, xác nhận GARDEN có 6 con, Missing Script campus 0, isDirty=false.
- PlayMode toàn dự án sau Task 3 chưa có kết quả. Lệnh chạy TimeoutError, các lần kiểm editor state và Console báo ping not answered. Dừng theo prompt, chưa stage/commit Task 3.
- Task 4 chưa chạy: chưa thao tác qua Play trên scene, chưa chụp ảnh, chưa kiểm Device Simulator hay Missing Script toàn bộ 3 scene và prefab sau thay đổi. Chưa tạo checkpoint PlayerPrefs thủ công. Test manager tự sao lưu/khôi phục những khóa nó ghi trong finally.

## Tọa độ và phạm vi

SPOT_GARDEN có trong dữ liệu album nhưng không có IUHCheckInSpot trong scene. Dùng CORRUPTED_ENCOUNTER_GARDEN (-16,0.05,-14) làm mốc. Khinh khí cầu (6,0.2,-10), linh vật (4.5,0.1,-6). Các encounter: Boss (0,0,22), Canteen (-22,.05,-36), Nhà E (-6,.05,6), Orc (-18,0,-8), Garden (-16,.05,-14), Backpack (4,0,-28), Phantom (18,0,-6), Fountain (4,.05,-28), Sports (20,.05,-10), Blue Demon (0,0,-24). Bán kính kích hoạt tự động trong nhóm là 4.8 hoặc 10 m; bán kính E thực tế 16 m. Dùng ngưỡng tâm luống 19 m cho mọi encounter, có đệm 2 m cho vùng mở bảng.

Cụm phía tây ở z=-19.1 và -16.9, mỗi hàng có x=-39.2, -37.0, -34.8; cả sáu y=0. Raycast cho Campus ground, vùng quanh tâm không chồng collider ở độ cao người đi theo phép CheckBox, xa khinh khí cầu/linh vật. Cụm nằm xa hơn 10 m vì quanh mốc có nhiều vùng E chồng nhau. Scene cuối được tái lập bằng bản gốc cộng đúng 16 block thuộc GARDEN và một mục SceneRoots. Các block Unity tự serialize lại được bỏ khỏi bản ghi.

Bảng dùng canvas riêng và có anchor safe area. ESC giữ hành vi hiện tại của game, đóng bảng bằng E/X hoặc đi xa theo phương án dự phòng của spec. Chưa xác nhận tương tác bàn phím/chạm trong Play trên campus.

## Hoàn tất Task 3 và Task 4

MCP hoạt động lại, PlayMode toàn dự án 35/35 Pass (21773bc15f404f56b7834e4f237af329). EditMode cuối 114/114 Pass, so với baseline 104/104 và 30/30. Task 3 commit bdd339f. Scene trong commit chỉ thêm block của GARDEN và mục root. Compile sạch. Console 0 error sau lượt kiểm runtime. Missing Script = 0 ở campus, AR, arena và 84 prefab.

| Mục kiểm tay | Kết quả | Bằng chứng |
|---|---|---|
| Chu kỳ hai luống | Pass qua MCP | Gieo qua nút UI 100 → 90, bón 90 → 80. Dùng Clock giả để qua hai giai đoạn. Hai lần hái 30 → 75 (+45) và 75 → 105 (+30). Hái lặp false. Các luống phụ cũng được gieo để chụp ảnh. |
| Thiếu Vàng | Pass | 5 Vàng, nút Gieo hạt bị khóa, nhãn Cần 10 Vàng. |
| Không tưới | Pass | Tua 10 phút, vẫn ở Hạt. |
| Space, E, Tương tác và khoảng cách | Pass qua Input System và input giả định | Space không mở, E mở luống 1, VirtualInteractPressed mở bảng. Khi dời người chơi 6 m, bảng tự đóng. |
| Combat | Pass qua MCP | Match3 khóa khám phá, CanOpenNow=False, nhấn E không mở bảng. |
| E tại vườn | Pass trong lượt thử | E chỉ hiện bảng vườn, khinh khí cầu không lên và game vẫn ở khám phá. Vị trí còn có đệm với mọi quái và linh vật như phần đo khoảng cách. |
| Thoát và mở lại Play | Pass | Chuỗi save khớp, luống thứ ba vẫn Nở. |
| Reset cốt truyện | Pass cho handler | F9 mô phỏng chưa kích hoạt tải lại. Gọi ResetStory() trực tiếp, save vườn khớp và luống thứ ba vẫn Nở. |
| Ảnh PC | Pass | Xem seed-panel.png, sprout.png và bloom-four-colors.png. Ảnh Nở dùng camera runtime cô lập để mái hiên không che luống, không lưu camera/layer vào scene. |
| iPhone Device Simulator | Chờ Thịnh thử trên máy | Chưa chạy trong lượt này. |

Đã thoát Play và đối chiếu 7/7 khóa đã khôi phục (IUH_Garden_v1, IUH_Story_Progress, Gold, SwordLv, ShieldLv, MaxHpLv và Items của kho Guardian). Không có checkpoint kiểm thử còn được áp dụng. Không push.
