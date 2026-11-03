# YOLOv8n pretrained COCO

License: Ultralytics AGPL-3.0, pretrained YOLOv8n COCO; not retrained.
Source weights: https://github.com/ultralytics/assets/releases/download/v8.4.0/yolov8n.pt
Upstream: https://github.com/ultralytics/ultralytics ; license copy: AGPL-3.0.txt.

Exported with Ultralytics 8.4.198, torch 2.7.1+cpu, ONNX 1.17.0, onnxslim 0.1.71.
Runtime: onnxruntime 1.23.2 CPUExecutionProvider. Input float32 [1,3,640,640], output [1,84,8400].
Batch 1, opset 17, simplified graph, no embedded NMS. No torch runtime dependency.

Size: 12851104 bytes (12.851104 decimal MB).
SHA256: `4d0632282232a5fe4eedf59a951f371d3d6b98e4d78546813f1d0ef07b06bb40`.

`yolov8n.classes.json` is generated from exported ONNX model metadata, not hand-written IDs.
Allowed mission IDs/names from metadata: {"24": "backpack", "39": "bottle", "56": "chair", "63": "laptop", "67": "cell phone", "73": "book"}.
The default list is configurable through ALLOWED_CLASSES; detector resolves IDs from metadata.

Re-export using requirements-export.txt in Server/.venv-export. Original .pt/cache files and alternative ONNX sizes are ignored; the default yolov8n.onnx is intentionally committed for clean-checkout inference. Models over 50 MB must not be committed. Docker uses the read-only models bind mount.

## B2.5 experiments (not defaults)

-512 FP32:12,790,628bytes. Exported separately with --imgsz512.
-640 dynamic signed QInt8:3,504,075bytes. ONNX Runtime1.23.2 CPU cannot load ConvInteger(10); latency/accuracy unavailable, not deployed.
-640/512/480/416 speed tables: Docs/Evidence/CampusGuardian/B2/thread-benchmark.md.
-No real user photos supplied; real-eval.md intentionally reports N/A. Default640 model bytes/checksum above are unchanged.

## B2.7 — Model decision (2026-11-04)

YOLOv8n 640: API total p50 ≤ 150 ms, p95 ≤ 220 ms trên CPU (Ryzen 7 6800H, Docker, Gunicorn 2 worker); confidence ≥ 0.70. Ưu tiên độ chính xác; 416 nhanh hơn nhưng trượt nhiều hơn trên mẫu thử B2.
Model/default inference code and checksum unchanged. Chưa đánh giá trên ảnh thật — sẽ bổ sung. See Docs/Evidence/CampusGuardian/B2/model-decision.md.
