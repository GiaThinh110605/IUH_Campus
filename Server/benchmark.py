import argparse
import io
import json
import os
from pathlib import Path
import platform
import statistics

import numpy as np

from app import create_app


def cpu_name():
    if os.name == "nt":
        import winreg
        with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"HARDWARE\DESCRIPTION\System\CentralProcessor\0") as key:
            return winreg.QueryValueEx(key, "ProcessorNameString")[0].strip()
    cpu_info = Path("/proc/cpuinfo")
    if cpu_info.is_file():
        for line in cpu_info.read_text().splitlines():
            if line.startswith("model name"):
                return line.split(":", 1)[1].strip()
    return platform.processor() or os.getenv("PROCESSOR_IDENTIFIER", "unknown")


def benchmark(model, image, workers):
    app = create_app({"TESTING": True, "MODEL_PATH": str(model)})
    if not app.extensions["model_registry"].loaded:
        raise RuntimeError("Benchmark requires a loaded real model")
    client = app.test_client()
    data = Path(image).read_bytes()
    measured = []
    for index in range(55):
        response = client.post("/predict", data={"image": (io.BytesIO(data), "image.jpg"), "target_class": "laptop"})
        if response.status_code != 200:
            raise RuntimeError(f"Prediction failed: {response.status_code}")
        if index >= 5:
            measured.append(response.get_json())
    report = {"model": str(model), "image": str(image), "cpu": cpu_name(), "os": platform.platform(),
              "workers_configured": workers, "benchmark_processes": 1,
              "ort_threads": app.config["ORT_INTRA_OP_THREADS"], "warmup": 5, "iterations": 50}
    for field in ("inference_time_ms", "total_time_ms"):
        values = [item[field] for item in measured]
        report[field] = {"p50": float(np.percentile(values, 50)), "p95": float(np.percentile(values, 95)),
                         "mean": statistics.mean(values)}
    report["fixture_target_found"] = measured[-1]["target_found"]
    print(json.dumps(report, indent=2))
    return report


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--model", default=os.getenv("MODEL_PATH", str(Path(__file__).parent / "models/yolov8n.onnx")))
    parser.add_argument("--image", default=str(Path(__file__).parent / "tests/fixtures/laptop-isorepublic.jpg"))
    parser.add_argument("--workers", type=int, default=1)
    arguments = parser.parse_args()
    benchmark(arguments.model, arguments.image, arguments.workers)
