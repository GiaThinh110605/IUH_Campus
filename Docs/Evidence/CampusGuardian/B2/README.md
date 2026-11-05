# Backend W9 B2 evidence

Date: 2026-11-02 Asia/Bangkok. CPU: AMD Ryzen 7 6800H with Radeon Graphics.
Local: Windows 11 build 26200, Python 3.11.9. Container: Linux/WSL2 amd64, Python 3.11, Gunicorn 2 workers.
Base commit: 40b114f8e815e3ebec2a7d82974642b2ef4f4a8c.
B2 implementation commit: the commit containing this evidence; resolve using `git log -1 --format=%H -- Server/app/inference/detector.py`.

- `../B1/docker.txt`: successful B1 build/healthy, health 200, request metrics, missing-image 400, logs, down.
- `export.txt`: real 640 export log, MB/SHA256/input/output/classes. Export subprocess exit 0; the outer display wrapper encountered Windows stdout Unicode encoding after saving the complete log. Exported model itself succeeded.
- `export-480.txt`, `export-416.txt`: separate comparison exports; default 640 SHA unchanged. Alternative ONNX/.pt artifacts remain local and ignored; regenerate with export script.
- `pytest.txt`: final 30 passed. Unit and real-model integration tests; no fabricated model predictions.
- `docker.txt`: final rebuilt B2 healthy/model_loaded=true, actual multipart predict, metrics, three exec benchmarks, torch absent in runtime, logs, down.
- `predict-sample.json`: actual HTTP response from final Docker run.
- `benchmark.txt`: full p50/p95/mean inference and total times; `benchmark-local.json`, `benchmark-container.json` raw summaries.
- `fixture-comparison.json`: actual four-fixture results for all three model sizes. 640 finds only 1/2 positive laptop photos. All negatives are synthetic; this is not a representative accuracy evaluation.
- `source-sha256.json`: tested Python worktree source hashes.

Model default 640, batch1, opset17, simplified, pretrained COCO, AGPL-3.0. SHA256 and permitted IDs are in Server/models/README.md. Four small fixture files have explicit CC0 source/license records in Server/tests/fixtures/README.md. No model training, Adafruit IO calls or Unity changes.

Self-fix rounds: 1/3 (export dependency pins: torch 2.5.1 required sympy1.13.1 but onnxslim required >=1.13.3; corrected to torch2.7.1 CPU / torchvision0.22.1 CPU). Windows pyreadline teardown OSError and transient stdout encoding display problems are external console behavior; raw pytest/export evidence records actual exit states.

Acceptance limitation: default 640 inference p50 exceeds45ms. Smaller models lose positive detection at required threshold, so no default/threshold changed. B3 cloud integration and broader camera/lighting coverage remain outstanding.

## B2.5 tuning update (2026-11-02)

Base commit c014e39b2504cecaaa78db587a95fe59edc69d22; implementation commit is the commit containing Server/eval_real.py (`git log -1 --format=%H -- Server/eval_real.py`).

See `thread-benchmark.md`, `tune-local.json`, `tune-container.json`, `load-30s.json`, `variants-local.json`, `variants-container.json` and matching raw txt logs. All latency measurements use the existing B2 stock-photo fixture, not user real photos. Each matrix case uses actual1/2 OS processes locally or actual1/2 Gunicorn workers in Docker; worker2 cases issue two concurrent streams. Raw HTTP warmup has5*client_count requests total, not a guaranteed distribution per server worker. Benchmark containers were downed after measurement. `tune-docker.txt` and `variant-docker.txt` preserve commands and cleanup output.

`export-512.txt`: separate512 FP32 trial, 12,790,628bytes. `quantization-int8.txt` / `quantization-int8-container.txt`: signed dynamic QInt8 trial, 3,504,075bytes, ConvInteger(10) unsupported on CPUExecutionProvider; no fabricated benchmark for it. Models remain local/ignored.

`tune-pytest.txt`: 45/45 passed. `tune-source-sha256.json`: tested worktree Python hashes. `eval-empty.txt`: expected exit2, instructions and no traceback for missing real directory. `real-eval.md`: all15 rows N/A because the user confirmed real photos will be supplied later. No real-photo labels/images were invented, no user photos were committed.

Speed-only candidate is416 FP32,6 intra-op threads,2 Gunicorn workers, threshold0.70; not applied and not an accuracy recommendation. Chưa đánh giá độ đúng trên ảnh thật. Default model640 SHA256 and threshold0.70 unchanged. Default threads now use the specifically requested availableCPU/workers automatic rule; see Server/README.md for new ORT_INTRA_THREADS variable. No root README, Unity or Assets edits.
