# VFX Asset Store — kế hoạch triển khai

Ngày 24/11/2026, base main 1061a44, nhánh minhnhat/vfx-pass.
Thiết kế theo prompt đã duyệt: sử dụng ba gói người dùng import, giữ gameplay và API. Không push/amend. Không hỏi giữa chừng theo yêu cầu.

- [x] V0: inventory/GUID, MoveAsset, dọn dependency, URP/texture, credits/source, shader test; commit7a21cd4.
- [x] V1: đã thử và bỏ sau hai vòng hình ảnh không đạt; rollback candidate, giữ code khói cũ. Không phải Pass triển khai.
- [x] V2: visual con, mobile half, clone arena và A/B4 góc đạt; commit43e6210.
- [x] V3: đã thử và bỏ sau hai vòng vượt ngưỡng; rollback hook/pool/resources. Không phải Pass triển khai.
- [x] V3b: scale/pose mũ và Light, ảnh ngày/đêm đạt; commita189750.
- [x] V4: 138Edit/61Play Pass, Missing0 ba scene, Console0/Edit sạch, README/ảnh/ngoại lệ/prompt; commit chứa tài liệu này.

Parent độc quyền Unity và scene; agent hỗ trợ chỉ viết candidate trong Temp. V0 là điều kiện tích hợp mọi candidate. Nếu MCP mất phản hồi: ghi RESUME và dừng, không viết tay scene.
