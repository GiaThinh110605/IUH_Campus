import time
from flask import Blueprint, current_app, jsonify

health = Blueprint("health", __name__)


@health.get("/health")
def health_check():
    registry = current_app.extensions["model_registry"]
    return jsonify(
        status="healthy", service="IUH Guardian API", version="0.1.0",
        uptime_seconds=round(time.monotonic() - current_app.extensions["started_at"], 6),
        model_loaded=registry.loaded,
        model_path_configured=registry.path_configured,
    )
