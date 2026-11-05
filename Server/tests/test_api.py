import io
from unittest.mock import patch

from PIL import Image
import pytest

from app import create_app
from app.config import load_config


@pytest.fixture
def app(tmp_path):
    return create_app({"TESTING": True, "MODEL_PATH": str(tmp_path / "missing.onnx")})


@pytest.fixture
def client(app):
    return app.test_client()


def image_file():
    data = io.BytesIO()
    Image.new("RGB", (4, 4), "white").save(data, format="PNG")
    data.seek(0)
    return data


def test_health_reports_real_unloaded_state_and_elapsed_seconds(client):
    first = client.get("/health")
    assert first.status_code == 200
    body = first.get_json()
    assert body["model_loaded"] is False
    assert body["model_path_configured"] is True
    assert body["service"] == "IUH Guardian API"
    assert body["version"] == "0.1.0"
    assert 0 <= body["uptime_seconds"] < 60
    started = client.application.extensions["started_at"]
    with patch("app.routes.health.time.monotonic", return_value=started + 2.5):
        assert client.get("/health").get_json()["uptime_seconds"] == 2.5


def test_metrics_include_duration_status_counts_and_model_gauge(client):
    client.get("/health")
    client.post("/predict")
    response = client.get("/metrics")
    assert response.status_code == 200
    text = response.get_data(as_text=True)
    assert "flask_http_request_duration_seconds" in text
    assert 'flask_http_request_total{method="POST",status="400"} 1.0' in text
    assert "model_loaded 0.0" in text


def test_predict_missing_image_is_400(client):
    assert client.post("/predict").status_code == 400


def test_predict_unreadable_image_is_400(client):
    response = client.post("/predict", data={"image": (io.BytesIO(b"not an image"), "bad.png")})
    assert response.status_code == 400


def test_predict_valid_image_without_model_is_503(client):
    response = client.post("/predict", data={"image": (image_file(), "image.png")})
    assert response.status_code == 503
    assert response.get_json() == {"error": "model_not_loaded"}


def test_config_reads_environment(monkeypatch):
    for key, value in {"PORT": "5012", "MODEL_PATH": "configured.onnx", "CONFIDENCE_THRESHOLD": "0.82",
                       "LOG_LEVEL": "DEBUG", "ADAFRUIT_AIO_USERNAME": "", "ADAFRUIT_AIO_KEY": ""}.items():
        monkeypatch.setenv(key, value)
    config = load_config()
    assert config["PORT"] == 5012
    assert config["CONFIDENCE_THRESHOLD"] == 0.82
    assert config["MODEL_PATH"] == "configured.onnx"
    assert config["LOG_LEVEL"] == "DEBUG"
    assert config["ADAFRUIT_AIO_KEY"] == ""


def test_no_configured_model_is_distinct_from_missing_file():
    app = create_app({"TESTING": True, "MODEL_PATH": ""})
    assert app.test_client().get("/health").get_json()["model_path_configured"] is False


def test_model_session_created_once_not_per_request(tmp_path):
    model = tmp_path / "model.onnx"
    model.write_bytes(b"fixture")
    with patch("app.model_registry.ort.InferenceSession") as session:
        session.return_value.get_modelmeta.return_value.custom_metadata_map = {"names": "{0: 'book'}"}
        app = create_app({"TESTING": True, "MODEL_PATH": str(model)})
        client = app.test_client()
        for _ in range(2):
            assert client.get("/health").get_json()["model_loaded"] is True
        assert session.call_count == 1
        assert session.call_args.kwargs["providers"] == ["CPUExecutionProvider"]



def test_invalid_model_never_claims_loaded(tmp_path):
    model = tmp_path / "invalid.onnx"
    model.write_bytes(b"invalid")
    app = create_app({"TESTING": True, "MODEL_PATH": str(model)})
    assert app.test_client().get("/health").get_json()["model_loaded"] is False


def test_missing_model_warning_only_once_per_start(tmp_path, caplog):
    app = create_app({"TESTING": True, "MODEL_PATH": str(tmp_path / "missing.onnx")})
    client = app.test_client()
    client.get("/health")
    client.get("/health")
    warnings = [r for r in caplog.records if "Model file unavailable" in r.message]
    assert len(warnings) == 1


@pytest.mark.parametrize("path", ["/predict/student-card", "/iot/parking-status"])
def test_simulated_endpoints_removed(client, path):
    assert client.get(path).status_code == 404
