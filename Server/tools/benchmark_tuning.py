import argparse
from concurrent.futures import ProcessPoolExecutor, ThreadPoolExecutor
import io
import json
import multiprocessing
from pathlib import Path
from time import perf_counter
import urllib.request

import numpy as np

from app import create_app
from benchmark import cpu_name


def summarize(samples, elapsed):
    report = {"samples": len(samples), "elapsed_seconds": elapsed,
              "requests_per_second": len(samples) / elapsed if elapsed else 0}
    for field in ("inference_time_ms", "total_time_ms", "wall_ms"):
        values = [sample[field] for sample in samples]
        report[field] = {"p50": float(np.percentile(values, 50)), "p95": float(np.percentile(values, 95)),
                         "mean": float(np.mean(values))} if values else None
    return report


def local_worker(arguments):
    model, image, threads, workers, iterations = arguments
    app = create_app({"MODEL_PATH": model, "ORT_INTRA_THREADS": threads, "WEB_CONCURRENCY": workers})
    if not app.extensions["model_registry"].loaded:
        raise RuntimeError("Real model must be loaded for benchmark")
    data = Path(image).read_bytes()
    client, samples = app.test_client(), []
    for index in range(iterations + 5):
        start = perf_counter()
        response = client.post("/predict", data={"image": (io.BytesIO(data), "image.jpg"), "target_class": "laptop"})
        if response.status_code != 200:
            raise RuntimeError(f"Prediction failed: {response.status_code}")
        body = response.get_json()
        body["wall_ms"] = (perf_counter() - start) * 1000
        if index >= 5:
            samples.append(body)
    return samples


def local_benchmark(model, image, threads, workers, iterations=50):
    arguments = (str(Path(model).resolve()), str(Path(image).resolve()), threads, workers, iterations)
    start = perf_counter()
    with ProcessPoolExecutor(max_workers=workers, mp_context=multiprocessing.get_context("spawn")) as pool:
        batches = list(pool.map(local_worker, [arguments] * workers))
    report = summarize([sample for batch in batches for sample in batch], perf_counter() - start)
    report.update(environment="local Flask test_client worker processes", cpu=cpu_name(),
                  threads=threads, workers=workers, warmup_per_worker=5, iterations_per_worker=iterations)
    return report


def multipart(image):
    boundary = "IUHGuardianBenchmarkBoundary"
    header = (f"--{boundary}\r\nContent-Disposition: form-data; name=\"target_class\"\r\n\r\nlaptop\r\n"
              f"--{boundary}\r\nContent-Disposition: form-data; name=\"image\"; filename=\"image.jpg\"\r\n"
              "Content-Type: image/jpeg\r\n\r\n").encode()
    return header + Path(image).read_bytes() + f"\r\n--{boundary}--\r\n".encode(), boundary


def http_worker(arguments):
    url, data, boundary, iterations, deadline = arguments
    samples, errors = [], []
    count = 0
    while (perf_counter() < deadline) if deadline else (count < iterations):
        try:
            samples.append(send_request(url, data, boundary))
        except Exception as error:
            errors.append(type(error).__name__)
        count += 1
    return samples, errors


def send_request(url, data, boundary):
    request = urllib.request.Request(url.rstrip("/") + "/predict", data=data,
                                     headers={"Content-Type": f"multipart/form-data; boundary={boundary}"})
    start = perf_counter()
    with urllib.request.urlopen(request, timeout=60) as response:
        body = json.load(response)
    body["wall_ms"] = (perf_counter() - start) * 1000
    return body


def http_benchmark(url, image, clients, iterations=50, seconds=0):
    data, boundary = multipart(image)
    # Warmup finishes before the timed 30-second load interval starts.
    with ThreadPoolExecutor(max_workers=clients) as pool:
        list(pool.map(lambda _: send_request(url, data, boundary), range(clients * 5)))
    start = perf_counter()
    deadline = start + seconds if seconds else 0
    with ThreadPoolExecutor(max_workers=clients) as pool:
        batches = list(pool.map(http_worker, [(url, data, boundary, iterations, deadline)] * clients))
    report = summarize([sample for samples, _ in batches for sample in samples], perf_counter() - start)
    report.update(environment="HTTP Gunicorn", clients=clients, requested_duration_seconds=seconds,
                  warmup_requests=clients * 5,
                  errors=[error for _, errors in batches for error in errors])
    return report


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--mode", choices=["local", "http"], required=True)
    parser.add_argument("--model", default="models/yolov8n.onnx")
    parser.add_argument("--image", default="tests/fixtures/laptop-stocksnap.jpg")
    parser.add_argument("--threads", type=int, default=2)
    parser.add_argument("--workers", type=int, choices=[1, 2], default=1)
    parser.add_argument("--iterations", type=int, default=50)
    parser.add_argument("--seconds", type=float, default=0)
    parser.add_argument("--url", default="http://localhost:5001")
    args = parser.parse_args()
    result = local_benchmark(args.model, args.image, args.threads, args.workers, args.iterations) if args.mode == "local" else http_benchmark(args.url, args.image, args.workers, args.iterations, args.seconds)
    print(json.dumps(result, indent=2))
