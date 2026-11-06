import io
import math
from time import perf_counter
from flask import Blueprint, current_app, jsonify, request
from PIL import Image, UnidentifiedImageError
from ..scan_store import ScanSessionError

predict = Blueprint("predict", __name__)


def prediction_options():
    target = request.form.get("target_class")
    allowed = current_app.config["ALLOWED_CLASSES"]
    if target is not None and target not in allowed:
        raise ValueError("invalid_target_class")
    minimum = current_app.config["CONFIDENCE_THRESHOLD"]
    try:
        threshold = float(request.form.get("threshold", minimum))
    except ValueError:
        raise ValueError("invalid_threshold") from None
    if not math.isfinite(threshold) or not minimum <= threshold <= 1:
        raise ValueError("invalid_threshold")
    return target, threshold


@predict.post("/predict")
def predict_image():
    started = perf_counter()
    try:
        target, threshold = prediction_options()
    except ValueError as error:
        return jsonify(error=str(error)), 400
    scan_id = request.form.get("scan_session_id")
    if scan_id is not None:
        try:
            current_app.extensions["scan_store"].status(scan_id)
        except ScanSessionError as error:
            return jsonify(error=error.error), error.status
    upload = request.files.get("image")
    if upload is None:
        return jsonify(error="image_required"), 400
    data = upload.read()
    try:
        with Image.open(io.BytesIO(data)) as decoded:
            decoded.verify()
        with Image.open(io.BytesIO(data)) as decoded:
            image = decoded.convert("RGB")
    except (UnidentifiedImageError, OSError, ValueError, Image.DecompressionBombError):
        return jsonify(error="invalid_image"), 400
    if not current_app.extensions["model_registry"].loaded:
        return jsonify(error="model_not_loaded"), 503
    result = current_app.extensions["detector"].predict(image, threshold, target)
    if scan_id is not None:
        candidates = [d for d in result["detections"] if d["label"] in current_app.config["ALLOWED_CLASSES"]
                      and math.isfinite(d["confidence"]) and d["confidence"] >= threshold]
        label = max(candidates, key=lambda d: d["confidence"])["label"] if candidates else None
        try:
            result["scan"] = current_app.extensions["scan_store"].reveal(scan_id, label, current_app.extensions["cloud_configured"])
        except ScanSessionError as error:
            return jsonify(error=error.error), error.status
    result["total_time_ms"] = (perf_counter() - started) * 1000
    metrics = current_app.extensions["inference_metrics"]
    metrics["latency"].observe(result["inference_time_ms"] / 1000)
    for detection in result["detections"]:
        metrics["detections"].labels(label=detection["label"]).inc()
    if result["target_found"]:
        metrics["target_found"].inc()
    return jsonify(result)
