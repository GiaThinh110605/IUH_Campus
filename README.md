# IUH Campus Guardian

**Bảo vệ khuôn viên thông minh** — game phiêu lưu 3D kết hợp chiến đấu match-3 theo lượt, hướng đến **Windows PC và Android**.

Khám phá campus → quét nguồn nguyền → trận match-3 → thanh tẩy → nhận Campus Core → nâng cấp. Scene chính: `Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity`. Cấu trúc chọn 6 minigame được thay bằng một game thống nhất.

## Thiết kế mục tiêu

- PC bật camera quét trong game để thấy khói tím và khối dữ liệu lỗi trên vật thể bị nguyền; raycast xác nhận mục tiêu.
- Android mở camera thật nhận diện vật thể tương ứng theo nhiệm vụ, rồi đóng camera về campus để mở trận. Không cần hiệu ứng bám trên đồ vật thật; webcam PC chưa cam kết.
- Trận màn hình ngang: **nhân vật trái, bàn cờ giữa, quái phải**. Đề xuất bàn 6×6, bốn ô kiếm/khiên/năng lượng/hồi phục, một Skill.
- Đổi sai không mất lượt; chuỗi rơi trong cùng lượt, quái báo trước và phản công sau khi bàn ổn định; hết nước đi tự xáo.
- **Campus Core** nâng Tấn công, Phòng thủ, Sinh lực; mỗi nhánh 2 cấp bản đầu, chỉ mua ngoài trận. Bỏ EXP; trang phục/ngoại hình là mở rộng sau.
- Lưu Core, nâng cấp và khu vực thanh tẩy; chống thưởng trùng theo mốc và sự kiện. Hai nền tảng chưa bao gồm tài khoản/cloud save giữa thiết bị.

## Trạng thái và tài liệu

Cập nhật 01/11/2026: đang chuyển hướng thiết kế. PR tài liệu này không triển khai gameplay hoặc xác nhận Guardian đã chạy trên hai nền tảng. Checkout làm việc có dọn scene/menu local nhưng flow/UI cũ vẫn cần chuyển đổi; không dùng bằng chứng minigame cũ cho Guardian.

- [Kế hoạch chi tiết, phân công đề xuất và nghiệm thu](plan.md).
- [Tóm tắt gửi nhóm và rubric gốc](plan.txt).
- [Báo cáo đề xuất đã cập nhật](Docs/IUH_Campus_Guardian_Bao_Cao_De_Xuat.docx).
- [Dashboard cũ](IUH_Plan_Dashboard.html) chưa phản ánh thiết kế mới.

## Mở project và backend hiện có

Mở repository bằng Unity Hub với **Unity 6000.5.8f1** theo `ProjectSettings/ProjectVersion.txt`. Android cần Android Build Support/SDK/NDK/OpenJDK phù hợp. Mở scene SharedCampus ở trên, chờ import và kiểm tra Console trước Play. Không chạy builder cũ để tái tạo campus. Giữ `.meta` với asset; audit dependency trước khi xóa scene/script/pack.

Từ thư mục gốc, với Docker hỗ trợ Compose:

```powershell
docker compose -f Server/docker-compose.yml up --build -d
curl.exe http://localhost:8000/health
docker compose -f Server/docker-compose.yml logs --tail 100
docker compose -f Server/docker-compose.yml down
```

API docs: `http://localhost:8000/docs`. Hoặc dùng môi trường Python riêng:

```powershell
python -m venv Server/.venv
.\Server\.venv\Scripts\python.exe -m pip install -r Server/requirements.txt
.\Server\.venv\Scripts\python.exe -m uvicorn main:app --app-dir Server --host 0.0.0.0 --port 8000
```

Đây là hướng dẫn cấu hình hiện có, chưa chạy lại trong lượt sửa tài liệu. `/predict/student-card` và `/iot/parking-status` hiện mô phỏng; không phải AI Guardian/cloud thật. Android cần URL backend truy cập được từ điện thoại (ví dụ IP LAN), không dùng `localhost` của PC; kiểm tra firewall và chính sách HTTPS/HTTP của build.

## Cloud và rubric

Mục tiêu: Unity → FastAPI kiểm tra/chống trùng sự kiện → Adafruit IO/dashboard. Key chỉ ở biến môi trường backend, không trong Unity/Git/log. Mất mạng vẫn chơi, lưu pending và retry có kiểm soát. Không cảm biến vật lý, không EXP/feed EXP.

Raycast PC không phải Deep Learning; nhận diện Android/overlay tím chưa tự đáp ứng AR. Nhóm cần chốt minh họa AR trên thiết bị tương thích và xác nhận yêu cầu AR/IO Cloud với giảng viên. Chỉ báo hoàn thành khi có bằng chứng thật.

## Lộ trình và phối hợp

Ba tuần: tuần 1 có hai build/quét/bàn cờ thử; tuần 2 hoàn thành combat/nâng cấp/cloud; tuần 3 test/tối ưu/build cuối/báo cáo. Một tuyến nhiệm vụ, một loại quái, một thử thách cao hơn sau nâng cấp là phạm vi bản đầu.

Đề xuất Gia Thịnh phụ trách Unity/gameplay/tích hợp AI/AR/hai build; Minh Nhật phụ trách backend/cloud/Docker/chống trùng và hỗ trợ UI/QA. Nhóm xác nhận, cân đối cuối tuần 1. Một người tích hợp scene tại một thời điểm; code/prefab qua PR.

Kiểm thử cả chuột lẫn cảm ứng, camera đúng/sai/quyền, chuỗi ghép và xáo, lượt quái/Skill, mua nâng cấp/thưởng lặp, lưu/restart/mất mạng, Docker/feed và checkout sạch. Ghi thiết bị, commit/build, Pass/Fail; bằng chứng mới dự kiến tại `Docs/Evidence/CampusGuardian/`.
