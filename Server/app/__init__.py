import logging
import os
import time
import json
from pathlib import Path
from flask import Flask
from prometheus_client import CollectorRegistry, Gauge, Histogram, Counter
from .config import load_config
from .routes.health import health
from .routes.predict import predict
from .routes.gameplay import gameplay
from .cloud_client import cloud_configured
from .event_store import EventStore
from .storage_metrics import DurableCollector, DurableMetrics, DurableGunicornMetrics


def create_app(config=None):
    app = Flask(__name__)
    app.config.from_mapping(load_config())
    if config:
        app.config.update(config)
    app.logger.setLevel(getattr(logging, app.config["LOG_LEVEL"]))
    app.extensions["started_at"] = time.monotonic()
    configure_model(app)
    configure_storage(app)
    app.register_error_handler(413, lambda error: ({"error": "upload_too_large"}, 413))
    app.register_blueprint(health)
    app.register_blueprint(predict)
    app.register_blueprint(gameplay)
    app.extensions["prometheus_metrics"] = configure_metrics(app)
    return app


def configure_model(app):
    from .model_registry import ModelRegistry
    from .inference.detector import Detector
    model = ModelRegistry(app.config["MODEL_PATH"], app.logger, app.config["ORT_INTRA_THREADS"])
    app.extensions["model_registry"] = model
    app.extensions["detector"] = Detector(model, app.config["ALLOWED_CLASSES"])


def configure_storage(app):
    rewards = json.loads(Path(app.config["REWARDS_PATH"]).read_text(encoding="utf-8"))
    if not isinstance(rewards, dict) or any(type(value) is not int or value < 0 for value in rewards.values()):
        raise ValueError("Invalid server reward configuration")
    app.extensions["event_store"] = EventStore(app.config["DATABASE_PATH"], rewards)
    app.extensions["cloud_configured"] = cloud_configured(app.config)


def configure_metrics(app):
    registry = CollectorRegistry()
    if os.getenv("PROMETHEUS_MULTIPROC_DIR"):
        metrics = DurableGunicornMetrics(app, registry=registry)
    else:
        metrics = DurableMetrics(app, registry=registry)
    metrics.event_store = app.extensions["event_store"]
    registry.register(DurableCollector(metrics.event_store))
    app.extensions["gameplay_events_metric"] = Counter("gameplay_events_total", "Gameplay request outcomes", ["type", "result"], registry=registry)
    gauge = Gauge("model_loaded", "Whether all live API workers loaded their ONNX model",
                  registry=registry, multiprocess_mode="livemin")
    gauge.set(int(app.extensions["model_registry"].loaded))
    app.extensions["model_loaded_gauge"] = gauge
    app.extensions["inference_metrics"] = {
        "latency": Histogram("inference_seconds", "ONNX session.run latency", registry=registry),
        "detections": Counter("detections_total", "Accepted detections", ["label"], registry=registry),
        "target_found": Counter("predict_target_found_total", "Requests matching target class", registry=registry),
    }
    return metrics
