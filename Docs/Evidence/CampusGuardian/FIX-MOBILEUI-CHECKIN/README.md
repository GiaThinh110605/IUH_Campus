# FIX-MOBILEUI-CHECKIN

19/11/2026 · Windows Editor · Unity 6000.5.8f1 · nhánh minhnhat/fix-mobileui-checkin · base 3b3a2b9.

| Bộ test | Trước | Sau |
|---|---:|---:|
| EditMode toàn dự án | 104/104, acbaa2bf6e4c400fb84a1fb3d4a07ffc | 104/104, 1056f8143cc74db981c772b4a877cbbf |
| PlayMode toàn dự án | 29/29, d9dcbc3adac9420bbcabab19c9140ecf | 31/31, b5ed081798eb4ee79fe405d224551546 |

- 450c23a: bốn chỗ thoát/recover dùng CheckPlatformVisibility. Test desktop trước sửa thấy canvas bật (ab5eafaf3354430393ba4b105b5d99c4), sau sửa Pass (1a503a2910f849a6bd4a225b601a6135). File mobile UI và phím F8 giữ nguyên.
- 237e367: test gọi check-in hai lần tăng từ 100 lên 170 trước sửa (56e71e7efee04abb986108f26d170b65) và từ 100 lên 135 sau sửa (244810e942ad4c29aa6141c21c5a746b). Prompt tại điểm vẫn theo khoảng cách và StoryDirector giữ nguyên.
- Commit chứa báo cáo này: mục tiêu Story ẩn khi BlocksExplorationInput, hiện lại khi trở về khám phá. Giữ nguyên cách đặt vị trí và nội dung nhãn.

Đo trên scene campus qua Unity MCP bằng checkpoint Chương 1. Trận Nhà E thực sự chạy (IsCombatActive=True) và mục tiêu bị ẩn. Mô phỏng kết thúc thắng bằng nhánh OnCloseVictoryClicked, không chơi tới thắng bằng bàn cờ. Sau khi đóng, canvas mobile activeSelf=False và mục tiêu hiện lại với tiến độ Nhà E còn một quái.

Đo ở tâm CHECKIN_SPOT_FOUNTAIN đã mở, sai lệch XZ tối đa 0. Lượt đầu 20.00479 giây có vàng tăng 4118 → 4133, đúng mức thưởng xu hằng ngày. Không tính lượt đó làm kiểm chứng check-in. Lượt kiểm cô lập tắt tạm object xu trong Play Mode: 20.08049 giây, vàng 4133 → 4133, sai lệch XZ tối đa 0. 15 vàng phát sinh ở lượt đầu được giữ để không sửa trạng thái hoạt động hằng ngày.

Đã sao lưu trong Unity SessionState và khôi phục 11 khóa đã biết (trạng thái check-in và ngày cho năm ID, cộng IUH_Story_Progress), kể cả trạng thái khóa không tồn tại. Đã thoát Play. Console 0 error. Missing Script 0 ở scene campus, AR, arena và 83 prefab đã quét.
