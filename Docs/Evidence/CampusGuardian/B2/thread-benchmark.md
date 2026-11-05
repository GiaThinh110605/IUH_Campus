# B2.5 runtime thread benchmarks

2026-11-02 Asia/Bangkok. Base c014e39b2504cecaaa78db587a95fe59edc69d22. AMD Ryzen 7 6800H (16 logical CPUs).
Local: Windows11/Python3.11.9, actual Flask test_client worker processes. Container: WSL2 Linux amd64/Python3.11, actual Gunicorn workers; simultaneous HTTP streams equal workers.
640 FP32 model unchanged; 5 warmup per local worker / 5*client-count total HTTP warmup requests; 50 measured requests per worker/client. ORT_ENABLE_ALL, sequential execution, inter-op 1.
API total excludes client/network overhead. wall_ms is measured by client. No real photos supplied; speed input is the B2 stock laptop fixture.

| Env | Workers | Threads | inference p50 / p95 ms | API total p50 / p95 ms | client wall p50 / p95 ms |
|---|---:|---:|---|---|---|
| local | 1 | 2 | 63.204 / 73.610 | 81.138 / 92.241 | 82.090 / 93.180 |
| local | 1 | 4 | 44.498 / 58.467 | 63.195 / 81.415 | 64.254 / 82.683 |
| local | 1 | 6 | 37.558 / 44.442 | 54.013 / 62.060 | 55.147 / 63.163 |
| local | 1 | 8 | 34.854 / 37.976 | 51.449 / 55.413 | 52.480 / 56.419 |
| local | 2 | 2 | 82.515 / 94.561 | 105.741 / 118.286 | 106.810 / 119.451 |
| local | 2 | 4 | 60.410 / 72.490 | 85.637 / 100.885 | 86.984 / 102.338 |
| local | 2 | 6 | 53.132 / 60.330 | 81.464 / 92.618 | 83.018 / 94.340 |
| local | 2 | 8 | 65.654 / 105.690 | 94.384 / 132.616 | 95.633 / 133.816 |
| container | 1 | 2 | 73.588 / 86.202 | 88.417 / 100.149 | 93.835 / 114.811 |
| container | 1 | 4 | 59.560 / 89.653 | 75.221 / 110.396 | 82.504 / 116.744 |
| container | 1 | 6 | 54.617 / 62.109 | 71.705 / 82.966 | 77.022 / 93.723 |
| container | 1 | 8 | 61.628 / 82.608 | 82.136 / 104.694 | 90.682 / 116.117 |
| container | 2 | 2 | 108.454 / 151.730 | 127.117 / 176.766 | 134.495 / 186.854 |
| container | 2 | 4 | 78.242 / 96.772 | 99.612 / 120.845 | 106.521 / 129.924 |
| container | 2 | 6 | 74.686 / 139.569 | 98.367 / 163.609 | 105.890 / 170.241 |
| container | 2 | 8 | 100.256 / 152.905 | 129.346 / 180.630 | 140.512 / 192.301 |

## Closed-loop 30-second HTTP load
Gunicorn2 workers, 6 threads/worker, 2 clients, 591 measured requests over 30.080s, 0 errors.
End-to-end p50/p95: 97.742/130.565ms; API total p50/p95: 91.164/122.776ms; throughput 19.647req/s.

## Other model sizes at 6 threads
| Env | Size | Workers | inference p50/p95 ms | API total p50/p95 ms |
|---|---:|---:|---|---|
| local | 512 | 1 | 27.475/31.736 | 44.542/50.588 |
| local | 512 | 2 | 34.792/50.677 | 57.866/75.926 |
| local | 480 | 1 | 22.455/25.340 | 34.933/39.210 |
| local | 480 | 2 | 29.322/44.621 | 51.394/68.788 |
| local | 416 | 1 | 16.287/18.579 | 26.880/29.526 |
| local | 416 | 2 | 22.736/31.556 | 38.908/51.026 |
| container | 512 | 1 | 34.313/39.260 | 47.292/59.516 |
| container | 512 | 2 | 60.663/95.510 | 80.079/115.156 |
| container | 480 | 1 | 28.714/35.906 | 40.366/54.116 |
| container | 480 | 2 | 64.920/106.678 | 85.644/132.246 |
| container | 416 | 1 | 21.203/27.101 | 31.444/38.389 |
| container | 416 | 2 | 28.170/40.974 | 41.037/56.687 |

Dynamic signed INT8 QInt8: 3,504,075 bytes, cannot load in local ORT1.23.2 or container CPUExecutionProvider (ConvInteger(10) NOT_IMPLEMENTED). Latency and recall N/A; not silently replaced by unsigned quantization.

## Speed-only proposal (not applied)
Candidate for later validation: 416 FP32, ORT_INTRA_THREADS=6, WEB_CONCURRENCY=2, threshold0.70. It is the fastest measured deployable configuration in this comparison. Chưa đánh giá độ đúng trên ảnh thật. Existing B2 stock-photo tests already show a detection tradeoff for small models at0.70; this is not a production accuracy recommendation.
Deployed/default model remains640 FP32, minimum threshold0.70. Thread default follows the requested automatic CPU/workers rule; explicit6 is only a proposal.
Real-eval table contains N/A until the user supplies photos; eval_real.py returns2 with instructions instead of fabricating labels/results.
