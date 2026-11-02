# Backend runtime tuning / real-photo evaluation (B2.5)

The deployed model remains `models/yolov8n.onnx` (640 FP32), confidence minimum 0.70. No smaller or quantized model and no experimental threshold is applied automatically.

## ONNX Runtime

`ORT_INTRA_THREADS` replaces `ORT_INTRA_OP_THREADS`. Leave it empty for `max(1, available logical CPU count // WEB_CONCURRENCY)`. CPU affinity is respected where supported. `WEB_CONCURRENCY` defaults to 2 and controls actual Gunicorn workers. On Windows the CPU fallback uses `os.cpu_count()`. Counts are logical CPUs, not physical cores; cgroup CPU quotas are not inferred separately.

Sessions explicitly use graph optimization `ORT_ENABLE_ALL`, execution mode `ORT_SEQUENTIAL`, inter-op threads 1. `.env.example` shows the automatic setting. Set an explicit thread count only after measuring the target machine.

## Benchmark commands

Run from `Server/` using the runtime venv:

```powershell
.\.venv\Scripts\python.exe -m tools.benchmark_tuning --mode local --threads 4 --workers 2
.\.venv\Scripts\python.exe -m tools.benchmark_tuning --mode http --workers 2 --url http://localhost:5001
.\.venv\Scripts\python.exe -m tools.benchmark_tuning --mode http --workers 2 --seconds 30
```

Local Windows uses 1/2 actual worker processes, each with an independent model session and Flask test client; it does not claim to run native Gunicorn. Container HTTP tests use actual Gunicorn configured with matching `WEB_CONCURRENCY` and `ORT_INTRA_THREADS`; client concurrency equals worker count. Each local worker/HTTP client warms up 5 requests then measures 50. The load scenario warms up first, then sends two closed-loop HTTP streams for 30 seconds. A final in-flight response may finish slightly after the deadline. `total_time_ms` comes from the API, while `wall_ms` includes HTTP/client overhead. Local elapsed/throughput also includes process startup and warmup; compare per-request latency percentiles for tuning.

Experimental models are generated separately:

```powershell
.\.venv-export\Scripts\python.exe export_onnx.py --imgsz 512
.\.venv-export\Scripts\python.exe export_onnx.py --imgsz 480
.\.venv-export\Scripts\python.exe export_onnx.py --imgsz 416
.\.venv-export\Scripts\python.exe -m tools.quantize_trial
```

Dynamic signed INT8 uses ONNX Runtime `quantize_dynamic(..., weight_type=QInt8)`. A successful quantization does not mean the CPU execution provider supports every output operator. Unsupported models are reported unavailable; no latency/accuracy value is invented and no runtime package is patched. Model 512 and INT8 artifacts remain local/ignored, not defaults.

## Real photos (not yet supplied)

```powershell
.\.venv\Scripts\python.exe eval_real.py --threads 4
```

Expected directory: `Server/tests/fixtures/real/`. Filename prefix before `_` is the user-confirmed image-level target: `book`, `laptop`, `chair`, `bottle`, `backpack`, `cellphone` maps to `cell phone`, and `none` means no allowed target class. Other names are listed and skipped. The script generates `labels.csv`, then evaluates 640/512/480/416 FP32 and dynamic INT8 at 0.50/0.60/0.70 in a pure detector experiment; production API minimum stays 0.70. Five warmups precede each model/threshold evaluation. Recall is image-level per class, and false positives count `none` images with any allowed detection. p50 includes local file/image decoding and model pipeline, not HTTP.

Output: `Docs/Evidence/CampusGuardian/B2/real-eval.md` and a JSON detail file. Missing/empty/unlabelled directories print instructions and exit 2 without a traceback. A report with N/A rows is written instead of fabricated measurements. No photos are created or modified by evaluation.

Before any future photo commit, manually inspect people and personal information; exclude such photos, resize approved copies to a maximum 1280px edge, and strip EXIF/GPS. This tooling does not certify privacy automatically. B2's stock/synthetic fixtures are only speed inputs for this turn, not a replacement for user-shot accuracy evaluation.
