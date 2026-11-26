# Bằng chứng Refactor — Đợt 6 (Player & Debug)

- **Ngày:** 2026-11-26
- **Nhánh:** `minhnhat/refactor-readability`
- **Môi trường:** Windows, Unity 6000.5.8f1 (URP), Unity MCP (`127.0.0.1:8081`)
- **Commit code:** `67ca127` — `refactor(player,debug): deduplicate sitting transition and flatten gizmos`

## Thay đổi chính (giữ nguyên toàn bộ API public, `[SerializeField]`, `PlayerPrefs`, text UI)
1. `Assets/IUH_Campus/Scripts/Player/IUHPlayerSittingController.cs`:
   - Tách vòng lặp nội suy vị trí/xoay trùng lặp trong `SitTransition` (59 dòng) ra helper `BlendTransform`.
   - Tách logic pose chân và tay trái/phải trùng lặp trong `LateUpdate` ra `ApplySittingLegPose` và `ApplySittingArmPose`.
2. `Assets/IUH_Campus/Scripts/Debug/DebugManager.cs`:
   - Làm phẳng `OnDrawGizmos` bằng early return (nesting từ 4 xuống 1).

## Chỉ số sau Đợt 6 (`scan_readability.py`)
- `IUHPlayerSittingController.cs`: 339 dòng, 16 hàm, 0 hàm >50 dòng, max nesting 2, score `2.53` (trước: `5.60`).
- `DebugManager.cs`: 36 dòng, 2 hàm, 0 hàm >50 dòng, max nesting 1, score `0.32` (trước: `2.35`).
- Toàn bộ source code production (222 file quét, trừ test files): **0 hàm >50 dòng**.

## Kết quả kiểm thử
- **Unity Compile & Refresh:** Pass (0 error trên Console).
- **Scene `IUH_SharedCampus_Concept.unity`:** `isDirty=false`, `GameObjects=2967 MissingScripts=0`.
- **EditMode Tests:** `150/150 Passed` (17.76s).
- **PlayMode Tests:** `75/75 Passed` (26.34s).
