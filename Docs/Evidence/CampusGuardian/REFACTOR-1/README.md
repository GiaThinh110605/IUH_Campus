# Refactor Đợt 1 — Backend Unity & Server Python

Nhánh `minhnhat/refactor-readability` (nền `main` `8dd41f3`, sau PR #53). Ngày 26/11/2026, Unity 6000.5.8f1 Windows Editor, URP. Thực hiện: Claude Code. Phạm vi: tách hàm dài, gom magic number thành hằng, giảm lồng; giữ nguyên 100% tên lớp, API public, DTO JSON, field `[SerializeField]`, key save và thông báo lỗi.

| Mục | Commit |
|---|---|
| Tách helper đánh giá ảnh thật (`real_eval.py`, `eval_real.py`) | `244fe6e` |
| Tách smoke B6 và script demo traffic (`b6_smoke.py`, `demo_traffic.py`) | `a401ea0` |
| Tách kiểm tra cấu hình và từ chối phiên scan (`scan_store.py`) | `a9b29a4` |
| Tách parse phản hồi scan và tính backoff outbox (Unity Backend C#) | `36e0e51` |

## Thay đổi từng file

- `Server/tools/real_eval.py` (158 → 223 dòng; điểm 5,03 → 0,84):
  - Hàm `evaluate` (56 dòng → 25 dòng): tách `_split_by_size`, `_load_detector`, `_warm_up`, `_evaluate_image`, `_evaluate_threshold`, `_run_context`.
  - Gom magic number (15 → 1) thành hằng: `MAX_IMAGE_SIDE = 640`, `WARMUP_RUNS = 5`, `EVAL_THRESHOLDS`, `REPORT_THRESHOLD`, `DIAGNOSTIC_THRESHOLD`, `EXIF_MAKE_TAG`, `EXIF_MODEL_TAG`, `PHONE_BRANDS`, `BYTES_PER_MIB`, `REPORT_TZ`, `TIMING_NOTE`.
- `Server/eval_real.py` (92 → 158 dòng; điểm 3,31 → 0,53):
  - Hàm `write_markdown` (54 dòng → 21 dòng): bỏ hàm lồng `table`, tách `_header_lines`, `_count_table`, `_result_table`, `_source_lines`, `_failure_lines`, `_excluded_lines`, `_limitation_lines`.
- `Server/tools/b6_smoke.py` (84 → 141 dòng; điểm 4,88 → 0,97):
  - Hàm `main` (65 dòng → 36 dòng): tách `_parse_args`, `_find_infested`, `_check_event_idempotency`, `_wait_for_cloud`; gom hằng timeout/poll/fixture/evidence (magic 16 → 5).
- `Server/tools/demo_traffic.py` (55 → 81 dòng; điểm 2,68 → 2,27):
  - Tách `_post_demo_event` khỏi vòng lặp `run`; gom `EVENT_INTERVAL_SECONDS = 3.1`, `PREDICT_TIMEOUT = 15`, `EVENT_TIMEOUT = 10` (magic 5 → 0).
- `Server/app/scan_store.py` (166 → 204 dòng; điểm 3,25 → 1,18):
  - Tách `__init__` thành `_validate_basic_limits`, `_parse_pool`, `_validate_pool_and_hints`; tách khối từ chối trong `create` thành `_rejection` (lồng tối đa 4 → 3); thêm `MAX_INFECTED_PER_SESSION = 2`, `SECONDS_PER_MINUTE = 60`, `TOKEN_BYTES = 32`.
- `Assets/IUH_Campus/Scripts/Guardian/Backend/GuardianApiClient.cs` (99 → 117 dòng; điểm 0,39):
  - Khai báo nhóm hằng HTTP dùng chung giữa các file partial (`HttpOk = 200`, `HttpCreated = 201`, `HttpSuccessMaxExclusive = 300`, `HttpNotFound = 404`, `HttpRequestTimeout = 408`, `HttpConflict = 409`, `HttpGone = 410`, `HttpTooManyRequests = 429`, `HttpServerErrorMin = 500`); chuẩn hóa hàm `Status`.
- `Assets/IUH_Campus/Scripts/Guardian/Backend/GuardianScanClient.cs` (100 → 193 dòng; điểm 1,33 → 0,64):
  - Tách `SessionAsync` thành `ParseSession` + `ParseRetryAfterSeconds`; tách `ScanFrameAsync` thành `ParseFrame` + `BestConfidence`; bỏ ghép nhiều câu lệnh trên một dòng (magic 10 → 0).
- `Assets/IUH_Campus/Scripts/Guardian/Backend/GuardianEventOutbox.cs` (130 → 151 dòng; điểm 0,73 → 0,50):
  - Tách `ComputeBackoffSeconds`; thêm `OutboxFileName`, `SnapshotVersion`, `MinDelaySeconds`, `MaxBackoffExponent` (magic 3 → 0).
- Các file runtime nhỏ còn lại (`GuardianScanService`, `GuardianBackendService`, `GuardianHttpTransport`, `GuardianImageEncoder`, `GuardianEvent`, `GuardianResults`, `GuardianScanStabilizer`, `GuardianBackendConfig`, `GuardianCombatReport`) đều ngắn gọn (18–73 dòng), không có hàm > 50 dòng nên giữ nguyên.

## Kiểm thử

- `Server\.venv\Scripts\python.exe -m pytest tests -q` (thư mục `Server/`): trước **134 passed** → sau **134 passed**.
- Golden markdown (`eval_real.write_markdown` với 2 mẫu report đầy đủ + nhánh lỗi `message`): **khớp byte-for-byte** trước/sau (`GOLDEN_MATCH=True`).
- `py_compile` trên cả 5 file Python sửa và `--help` cho `b6_smoke.py`, `demo_traffic.py`: pass.
- Unity `EditMode`: trước **150/150 passed** → sau **150/150 passed**.
- Unity `PlayMode`: trước **75/75 passed** → sau **75/75 passed**.
- Unity Console: **0 error**; Missing Script: **0** (kiểm trên 2.967 GameObject của `IUH_SharedCampus_Concept.unity`); scene `isDirty=false`.

## Chỗ lệch

1. `b6_smoke.py` và `demo_traffic.py` chỉ kiểm `py_compile` và `--help`, chưa chạy live với Docker.
2. `evaluator_sha256` của `real_eval.py` sẽ khác ở lần chạy đánh giá B2 tiếp theo (bằng chứng B2 cũ vẫn giữ SHA cũ).
3. Không refactor các file test (`Server/tests/`, `Guardian/Backend/Tests/`) trong đợt này.
