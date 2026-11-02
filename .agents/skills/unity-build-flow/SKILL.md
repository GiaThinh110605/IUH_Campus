---
name: unity-build-flow
description: Tự động hóa quá trình build game Unity ở chế độ dòng lệnh (batchmode) cho macOS Desktop và Mobile (iOS/Android), xử lý xung đột lockfile và kiểm tra cấu hình hướng màn hình Landscape.
---

# Unity Build Flow Automation

Kỹ năng này cung cấp quy trình và lệnh thực thi nhanh để build dự án IUH Campus không bị treo và đảm bảo cấu hình đúng chuẩn đa nền tảng.

## 1. Kiểm tra và Giải phóng Lockfile Unity
Trước khi chạy batchmode, kiểm tra xem Unity Editor có đang chiếm quyền lock thư mục hay không:
```bash
# Kiểm tra file lock
ls -la /Users/mac/Unity/IUH_Campus/Temp/UnityLockfile 2>/dev/null
# Nếu Editor không còn chạy nhưng còn sót lockfile:
rm -f /Users/mac/Unity/IUH_Campus/Temp/UnityLockfile
```

## 2. Đường dẫn Unity Editor mặc định trên macOS
Unity 6 (hoặc Unity 2022) được cài đặt tại:
```bash
UNITY_BIN="/Applications/Unity/Hub/Editor/6000.0.23f1/Unity.app/Contents/MacOS/Unity"
# Hoặc kiểm tra qua Unity Hub:
# "/Applications/Unity/Hub/Editor/$(ls /Applications/Unity/Hub/Editor | head -n 1)/Unity.app/Contents/MacOS/Unity"
```

## 3. Lệnh Build macOS Standalone (Desktop)
Thực thi build nhanh cho macOS Desktop để test gameplay:
```bash
PROJECT_PATH="/Users/mac/Unity/IUH_Campus"
BUILD_DIR="$PROJECT_PATH/Builds/macOS"
mkdir -p "$BUILD_DIR"

"$UNITY_BIN" \
  -batchmode \
  -quit \
  -projectPath "$PROJECT_PATH" \
  -buildOSXUniversalPlayer "$BUILD_DIR/IUHCampus.app" \
  -logFile "$PROJECT_PATH/Builds/build_mac.log"
```
Kiểm tra kết quả:
```bash
tail -n 30 "$PROJECT_PATH/Builds/build_mac.log"
```

## 4. Kiểm tra Cấu hình Hướng màn hình (Landscape Only)
Đảm bảo file cấu hình PlayerSettings chỉ cho phép màn hình xoay ngang:
- Trong Unity Editor: `Project Settings > Player > Resolution and Presentation`
- `Default Orientation`: `Landscape Left` hoặc `Auto Rotation` (chỉ chọn `Landscape Left` và `Landscape Right`, tắt `Portrait` và `Portrait Upside Down`).

## 5. Quy trình Kiểm tra Nhật ký Build (Build Log Checklist)
Nếu build thất bại:
1. Đọc 50 dòng cuối cùng trong `build_*.log`.
2. Tìm từ khóa `error CS` để xác định lỗi biên dịch C#.
3. Kiểm tra xem có script nào vi phạm quy chuẩn (như dùng thư viện Editor trong thư mục Runtime).

