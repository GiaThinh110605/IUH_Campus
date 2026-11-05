import time
import sqlite3
from flask import Blueprint, current_app, jsonify

health = Blueprint("health", __name__)


@health.get("/health")
def health_check():
    registry = current_app.extensions["model_registry"]
    try:
        summary = current_app.extensions["event_store"].summary()
    except sqlite3.Error:
        summary = {"db_ok": False, "outbox_pending": None, "outbox_failed": None}
    return jsonify(
        status="healthy" if summary["db_ok"] else "degraded", service="IUH Guardian API", version="0.1.0",
        uptime_seconds=round(time.monotonic() - current_app.extensions["started_at"], 6),
        db_ok=summary["db_ok"], cloud_configured=current_app.extensions["cloud_configured"],
        outbox_pending=summary["outbox_pending"], outbox_failed=summary["outbox_failed"],
        model_loaded=registry.loaded,
        model_path_configured=registry.path_configured,
    ), 200 if summary["db_ok"] else 503
