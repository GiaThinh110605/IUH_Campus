import hashlib
import io
import json
from pathlib import Path

import pytest

from app import create_app

SERVER = Path(__file__).resolve().parents[1]
FIXTURES = Path(__file__).parent / "fixtures"


@pytest.fixture(scope="module")
def real_app():
    model = SERVER / "models/yolov8n.onnx"
    assert model.is_file(), "The committed ONNX model is required for integration tests"
    app = create_app({"TESTING": True, "MODEL_PATH": str(model)})
    assert app.extensions["model_registry"].loaded
    return app


@pytest.mark.parametrize("filename,expected", [
    ("laptop-stocksnap.jpg", True), ("blank.png", False), ("shapes.png", False),
])
def test_real_target_found_and_not_found(real_app, filename, expected):
    client = real_app.test_client()
    session = real_app.extensions["model_registry"].session
    response = client.post("/predict", data={"image": (io.BytesIO((FIXTURES / filename).read_bytes()), filename),
                                           "target_class": "laptop"})
    assert response.status_code == 200
    body = response.get_json()
    assert body["target_found"] is expected
    assert body["total_time_ms"] >= body["inference_time_ms"] > 0
    assert real_app.extensions["model_registry"].session is session
    names = json.loads((SERVER / "models/yolov8n.classes.json").read_text())
    width, height = body["image_size"]
    for detection in body["detections"]:
        assert detection["confidence"] >= .7
        assert names[str(detection["class_id"])] == detection["label"]
        assert detection["label"] in real_app.config["ALLOWED_CLASSES"]
        x, y, w, h = detection["bbox"]
        assert 0 <= x < x + w <= width + .01
        assert 0 <= y < y + h <= height + .01


def test_closeup_fixture_and_optional_target(real_app):
    client = real_app.test_client()
    response = client.post("/predict", data={"image": (io.BytesIO((FIXTURES / "laptop-isorepublic.jpg").read_bytes()), "closeup.jpg")})
    body = response.get_json()
    assert response.status_code == 200
    assert body["target_class"] is None
    assert body["target_found"] is False
    assert body["model"]["sha256_prefix"] == hashlib.sha256((SERVER / "models/yolov8n.onnx").read_bytes()).hexdigest()[:8]


def test_prediction_metrics_exported(real_app):
    client = real_app.test_client()
    client.post("/predict", data={"image": (io.BytesIO((FIXTURES / "laptop-stocksnap.jpg").read_bytes()), "laptop.jpg"), "target_class": "laptop"})
    text = client.get("/metrics").get_data(as_text=True)
    assert "inference_seconds_count" in text
    assert 'detections_total{label="laptop"}' in text
    assert "predict_target_found_total" in text


def test_other_requested_class_does_not_match_laptop(real_app):
    response = real_app.test_client().post("/predict", data={
        "image": (io.BytesIO((FIXTURES / "laptop-stocksnap.jpg").read_bytes()), "laptop.jpg"), "target_class": "book"})
    body = response.get_json()
    assert response.status_code == 200
    assert body["target_found"] is False
    assert body["best_confidence"] is None
