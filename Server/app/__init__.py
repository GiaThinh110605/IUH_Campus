import logging
import os
import time
from flask import Flask
from prometheus_client import CollectorRegistry, Gauge
from prometheus_flask_exporter import PrometheusMetrics
from prometheus_flask_exporter.multiprocess import GunicornInternalPrometheusMetrics
from .config import load_config
from .model_registry import ModelRegistry
from .routes.health import health
from .routes.predict import predict


def create_app(config=None):
    app = Flask(__name__)
    app.config.from_mapping(load_config())
    if config:
        app.config.update(config)
    app.logger.setLevel(getattr(logging, app.config["LOG_LEVEL"]))
    app.extensions["started_at"] = time.monotonic()
    model = ModelRegistry(app.config["MODEL_PATH"], app.logger)
    app.extensions["model_registry"] = model
    app.register_blueprint(health)
    app.register_blueprint(predict)
    app.extensions["prometheus_metrics"] = configure_metrics(app)
    return app


def configure_metrics(app):
    registry = CollectorRegistry()
    if os.getenv("PROMETHEUS_MULTIPROC_DIR"):
        metrics = GunicornInternalPrometheusMetrics(app, registry=registry)
    else:
        metrics = PrometheusMetrics(app, registry=registry)
    gauge = Gauge("model_loaded", "Whether all live API workers loaded their ONNX model",
                  registry=registry, multiprocess_mode="livemin")
    gauge.set(int(app.extensions["model_registry"].loaded))
    app.extensions["model_loaded_gauge"] = gauge
    return metrics
