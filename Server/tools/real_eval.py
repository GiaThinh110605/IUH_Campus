import argparse
import csv
import json
from pathlib import Path
from time import perf_counter

import numpy as np
from PIL import Image

from app import create_app

PREFIX_LABELS = {"book": "book", "laptop": "laptop", "chair": "chair", "bottle": "bottle",
                 "backpack": "backpack", "cellphone": "cell phone", "none": "none"}
IMAGE_SUFFIXES = {".jpg", ".jpeg", ".png", ".webp", ".bmp"}
MODELS = {"640 fp32": "yolov8n.onnx", "512 fp32": "yolov8n-512.onnx",
          "480 fp32": "yolov8n-480.onnx", "416 fp32": "yolov8n-416.onnx", "640 int8": "yolov8n-int8.onnx"}


class NoLabeledImagesError(ValueError):
    def __init__(self, skipped):
        super().__init__("No filename with a user-confirmed class prefix")
        self.skipped = skipped


def build_labels(directory):
    directory = Path(directory)
    if not directory.is_dir():
        raise FileNotFoundError(f"Real-photo directory unavailable: {directory}")
    labels, skipped = [], []
    for file in sorted(directory.iterdir()):
        if not file.is_file() or file.suffix.lower() not in IMAGE_SUFFIXES:
            continue
        prefix, separator, _ = file.stem.partition("_")
        if not separator or prefix not in PREFIX_LABELS:
            skipped.append(file.name)
            continue
        labels.append({"file": file.name, "target_class": PREFIX_LABELS[prefix]})
    with (directory / "labels.csv").open("w", newline="", encoding="utf-8") as stream:
        writer = csv.DictWriter(stream, fieldnames=["file", "target_class"])
        writer.writeheader()
        writer.writerows(labels)
    return labels, skipped


def evaluate_model(directory, labels, model, threshold, threads):
    app = create_app({"MODEL_PATH": str(model), "ORT_INTRA_THREADS": threads})
    if not app.extensions["model_registry"].loaded:
        return {"status": "model_unavailable"}
    detector = app.extensions["detector"]
    with Image.open(Path(directory) / labels[0]["file"]) as source:
        warmup_image = source.convert("RGB")
    for _ in range(5):
        detector.predict(warmup_image, threshold)
    rows, classes = [], {}
    false_positive_count, negative_count = 0, 0
    for entry in labels:
        start = perf_counter()
        with Image.open(Path(directory) / entry["file"]) as source:
            image = source.convert("RGB")
        target = entry["target_class"]
        result = detector.predict(image, threshold, None if target == "none" else target)
        elapsed = (perf_counter() - start) * 1000
        if target == "none":
            negative_count += 1
            false_positive_count += bool(result["detections"])
        else:
            counts = classes.setdefault(target, {"found": 0, "total": 0})
            counts["total"] += 1
            counts["found"] += result["target_found"]
        rows.append({"file": entry["file"], "target_class": target, "target_found": result["target_found"],
                     "detections": len(result["detections"]), "total_ms": elapsed})
    for counts in classes.values():
        counts["recall"] = counts["found"] / counts["total"]
    return {"status": "measured", "recall_by_class": classes, "negative_images": negative_count,
            "false_positive_images": false_positive_count, "p50_total_ms": float(np.median([r["total_ms"] for r in rows])),
            "images": rows}


def evaluate(directory, model_directory, threads):
    labels, skipped = build_labels(directory)
    if not labels:
        raise NoLabeledImagesError(skipped)
    results = []
    for name, model_file in MODELS.items():
        for threshold in (.50, .60, .70):
            result = evaluate_model(directory, labels, Path(model_directory) / model_file, threshold, threads)
            results.append({"model": name, "threshold": threshold, **result})
    return {"labels": labels, "skipped": skipped, "threads": threads, "results": results,
            "timing": "local disk/image decode + preprocess + inference + postprocess; no HTTP"}


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--directory", default="tests/fixtures/real")
    parser.add_argument("--models", default="models")
    parser.add_argument("--threads", type=int, default=2)
    parser.add_argument("--output", required=True)
    arguments = parser.parse_args()
    report = evaluate(arguments.directory, arguments.models, arguments.threads)
    Path(arguments.output).write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))
