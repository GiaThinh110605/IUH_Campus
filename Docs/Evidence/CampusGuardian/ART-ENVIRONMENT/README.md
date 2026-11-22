# Lượt C đồ họa campus

Ngày kiểm chứng Task 4 · 22/11/2026. Nhánh `minhnhat/environment-pass`. Unity 6000.5.8f1 Windows Editor, Direct3D 12, NVIDIA GeForce RTX 3060 Laptop GPU, Campus HQ Pipeline. Ảnh và phép đo dùng 1280 × 900.

## Commit và kiểm thử

| Mốc | Commit | EditMode | PlayMode |
|---|---|---:|---:|
| Trước lượt | `6aa7997` | | |
| Task 1 · shader mặt đất | `e54b2e6` | | |
| Sửa biên dịch HLSL | `f762ead` | 127/127 | |
| Task 2 · thử quanh sảnh | `00e71ae` | 127/127 | 48/48 |
| Task 3 · trời, sương, hạt bay | `ef934a4` | 127/127 | 49/49 |
| Task 2b · chi tiết cỏ | `8cef559` | 128/128 | 49/49 |
| Task 4 · áp dụng toàn campus | `b8fd8a6` | 128/128 | 49/49 |
| Task 5 · tổng hợp bằng chứng | Commit `docs(art): record environment pass` chứa tài liệu này | | |

Task 4 chạy EditMode và PlayMode riêng. Console có 0 lỗi. Campus, Combat Arena và AR Encounter đều có 0 Missing Script. Phép thử 34,08 giây dùng `CharacterController.Move` qua sảnh, căn tin và bãi phía bắc, rồi gửi phím E qua Unity Input System. Vườn mở và đóng được. Ở 8 m, E chưa vào trận. Ở 4 m, E vào trận. Sương chuyển sang tím sẫm rồi về màu ban ngày. Lần thử đầu không nhận phím E, nên lượt xác nhận điều chỉnh focus và lưu số phím nhận thực tế. [Kết quả Task 3](playmode-smoke-task3.txt) · [Kết quả Task 4](playmode-smoke-task4.txt)

## Ảnh theo góc chụp

Các ảnh dùng vị trí, hướng và FOV từ [cameras.json](cameras.json). Task 2b được thực hiện sau Task 3.

| Góc | Trước lượt | Task 2 | Task 3 | Task 2b | Task 4 |
|---|---|---|---|---|---|
| Sảnh | [Ngày](lobby-before-day.png) · [Đêm](lobby-before-night.png) | [Ngày](lobby-ground-after-day.png) · [Đêm](lobby-ground-after-night.png) | [Ngày](lobby-atmosphere-after-day.png) · [Đêm](lobby-atmosphere-after-night.png) | [Ngày](lobby-grass-tune-after-day.png) · [Đêm](lobby-grass-tune-after-night.png) | [Ngày](lobby-campus-after-day.png) · [Đêm](lobby-campus-after-night.png) |
| Căn tin | [Ngày](canteen-before-day.png) · [Đêm](canteen-before-night.png) | [Ngày](canteen-ground-after-day.png) · [Đêm](canteen-ground-after-night.png) | [Ngày](canteen-atmosphere-after-day.png) · [Đêm](canteen-atmosphere-after-night.png) | [Ngày](canteen-grass-tune-after-day.png) · [Đêm](canteen-grass-tune-after-night.png) | [Ngày](canteen-campus-after-day.png) · [Đêm](canteen-campus-after-night.png) |
| Đài phun nước | [Ngày](fountain-before-day.png) · [Đêm](fountain-before-night.png) | [Ngày](fountain-ground-after-day.png) · [Đêm](fountain-ground-after-night.png) | [Ngày](fountain-atmosphere-after-day.png) · [Đêm](fountain-atmosphere-after-night.png) | [Ngày](fountain-grass-tune-after-day.png) · [Đêm](fountain-grass-tune-after-night.png) | [Ngày](fountain-campus-after-day.png) · [Đêm](fountain-campus-after-night.png) |
| Toàn cảnh | [Ngày](overview-before-day.png) · [Đêm](overview-before-night.png) | [Ngày](overview-ground-after-day.png) · [Đêm](overview-ground-after-night.png) | [Ngày](overview-atmosphere-after-day.png) · [Đêm](overview-atmosphere-after-night.png) | [Ngày](overview-grass-tune-after-day.png) · [Đêm](overview-grass-tune-after-night.png) | [Ngày](overview-campus-after-day.png) · [Đêm](overview-campus-after-night.png) |
| Cận cảnh | [Ngày](close-before-day.png) · [Đêm](close-before-night.png) | [Ngày](close-ground-after-day.png) · [Đêm](close-ground-after-night.png) | [Ngày](close-atmosphere-after-day.png) · [Đêm](close-atmosphere-after-night.png) | [Ngày](close-grass-tune-after-day.png) · [Đêm](close-grass-tune-after-night.png) | [Ngày](close-campus-after-day.png) · [Đêm](close-campus-after-night.png) |

## Hiệu năng

| Mốc | FPS sảnh | Draw calls sảnh | FPS toàn cảnh | Draw calls toàn cảnh | Dữ liệu |
|---|---:|---:|---:|---:|---|
| Trước lượt · mốc sảnh lịch sử | 217,25 | 209 | | | [Đo cùng Task 3](lobby-performance-task3.tsv) |
| Task 2 | 216,77 | 218 | | | [Đo lại cùng Task 3](lobby-performance-task3.tsv) · [Lượt đo ban đầu](lobby-performance-task2.tsv) |
| Task 3 | 213,67 | 219 | | | [TSV Task 3](lobby-performance-task3.tsv) |
| Task 2b | 203,53 | 219 | | | [TSV Task 2b](lobby-performance-grass-tune.tsv) |
| Trước lượt · tái lập cùng Task 4 | 229,50 | 209 | 197,51 | 896 | [TSV Task 4](campus-performance-task4.tsv) |
| Task 4 | 226,15 | 219 | 192,92 | 923 | [TSV Task 4](campus-performance-task4.tsv) |

Toàn cảnh có dữ liệu hiệu năng từ Task 4. Mốc trước lượt được tái lập trong cùng Play Mode bằng bảy slot vật liệu cũ, sương cũ và tắt `NATURE_GROUND`, `AMBIENT_PARTICLES`. Phép đo dùng hai góc trong `cameras.json`, tắt VSync, làm nóng 90 khung hình, rồi ghi 600 khung hình cho mỗi lượt. FPS tính bằng tổng số khung hình chia tổng thời gian của ba lượt cùng góc và mốc. Draw calls lấy trung vị.

Task 4 ở sảnh giảm 1,46% FPS so với mốc tái lập cùng phiên và tăng 10 draw calls. Kết quả cũng vượt mốc lịch sử 217,25 FPS. Toàn cảnh giảm 2,32% FPS và tăng 27 draw calls. Cả hai góc nằm trong ngưỡng giảm FPS tối đa 10% và tăng tối đa 30 draw calls. [Số đo chi tiết](campus-performance-task4.tsv)

## Renderer mặt đất

Bảy slot dưới đây được đổi ở Task 2. Kiểm kê Task 4 trên toàn bộ renderer của scene Campus, gồm cả object bất hoạt, thấy 0 slot còn dùng `Lawn.mat`, `Paving pale.mat`, `Sandstone paving.mat` hoặc ba bản VisualPolish tương ứng. Ba mesh ở Plaza vốn phủ nhiều khu, nên Task 4 tập trung vào cụm cỏ, với diff scene rỗng.

| Renderer (ID) | Khu | Slot (từ 0) | Vật liệu trước | Vật liệu stylized |
|---|---|---:|---|---|
| Lawn (`1363699657`) | Plaza paving_0 | 0 | Detailed lawn | `M_Ground_Grass` |
| Paving pale (`963449174`) | Plaza paving_0 | 0 | Pale paving slabs | `M_Ground_PavingPale` |
| Sandstone paving (`419868312`) | Plaza paving_0 | 0 | Courtyard sandstone | `M_Ground_Sandstone` |
| Paving pale (`1179028983`) | Central fountain and sculpture_7 | 0 | Pale paving slabs | `M_Ground_PavingPale` |
| Paving pale (`2127079105`) | Canteen terrace_9 | 0 | Pale paving slabs | `M_Ground_PavingPale` |
| Lawn (`1522067116`) | Garden beds and furniture_11 | 0 | Detailed lawn | `M_Ground_Grass` |
| Lawn (`1229912117`) | Curved fountain gardens_101 | 0 | Detailed lawn | `M_Ground_Grass` |

## Cỏ, hạt bay và bài học

- `NATURE_GROUND` có 12 cụm ở Task 2, 36 cụm sau Task 2b và 72 cụm sau Task 4. Task 4 thêm 36 cụm với seed 423, gồm 4 ở bãi tây nam, 6 mỗi bãi tây giữa, đông giữa, tây bắc và đông bắc, cùng 4 mỗi vành đài phun phía tây và đông. Hai vị trí sát luống hoa bị loại. Hai mesh `LobbyGround_Grass` và `LobbyGround_LeafAtlas` được mở rộng tại chỗ, giữ GUID và liên kết scene. Kiểm tra hình chiếu 12.696 đỉnh mới cho thấy tất cả nằm trên mặt bãi cỏ nâng.
- `AMBIENT_PARTICLES` ở Task 3 bám camera nhân vật, giới hạn 60 hạt và tắt trên mobile. Kiểm chứng hiệu năng của lượt này thực hiện trên Windows Editor.
- Scene dùng `Photographic campus sky`, đã chuyển tối qua `_Exposure`. `Cloud sky.mat` được chỉnh bảng màu ở vai trò dự phòng.
- Tên biến `line` trùng từ khóa HLSL, làm biến thể sân lát lỗi biên dịch, hiện hồng và chặn phiên trước. Sửa tên biến và buộc biên dịch cả hai biến thể trong EditMode để bắt lỗi sớm.
- Noise màu đơn thuần làm cỏ phẳng khi nhìn gần. Task 2b dùng texture `grass.png` từ `Lawn.mat` làm lớp độ sáng chiếu theo XZ thế giới. Chu kỳ chi tiết được chốt ở 0,9 m cho vật liệu cỏ của campus.
