# Real-photo evaluation

Labels follow user-confirmed filename prefixes; no labels inferred from image content.
Timing: local image decode + preprocess + inference + postprocess; no HTTP.

| Model | Threshold | Recall per class | False-positive none images | p50 total ms | Status |
|---|---:|---|---|---:|---|
| 640 fp32 | 0.50 | N/A | N/A | N/A | no_real_photos |
| 640 fp32 | 0.60 | N/A | N/A | N/A | no_real_photos |
| 640 fp32 | 0.70 | N/A | N/A | N/A | no_real_photos |
| 512 fp32 | 0.50 | N/A | N/A | N/A | no_real_photos |
| 512 fp32 | 0.60 | N/A | N/A | N/A | no_real_photos |
| 512 fp32 | 0.70 | N/A | N/A | N/A | no_real_photos |
| 480 fp32 | 0.50 | N/A | N/A | N/A | no_real_photos |
| 480 fp32 | 0.60 | N/A | N/A | N/A | no_real_photos |
| 480 fp32 | 0.70 | N/A | N/A | N/A | no_real_photos |
| 416 fp32 | 0.50 | N/A | N/A | N/A | no_real_photos |
| 416 fp32 | 0.60 | N/A | N/A | N/A | no_real_photos |
| 416 fp32 | 0.70 | N/A | N/A | N/A | no_real_photos |
| 640 int8 | 0.50 | N/A | N/A | N/A | no_real_photos |
| 640 int8 | 0.60 | N/A | N/A | N/A | no_real_photos |
| 640 int8 | 0.70 | N/A | N/A | N/A | no_real_photos |

Skipped filename(s): none

Photos are not modified or committed by this script. Before staging any photo, manually check people/personal information, resize approved copies to <=1280px and strip EXIF/GPS.

Chưa đánh giá độ đúng trên ảnh thật: Real-photo directory unavailable: D:\IUH\CongNgheMoi\CK\IUH_Campus\Server\tests\fixtures\real. Đặt ảnh vào D:\IUH\CongNgheMoi\CK\IUH_Campus\Server\tests\fixtures\real; dùng tiền tố book_, laptop_, chair_, bottle_, backpack_, cellphone_, none_.
