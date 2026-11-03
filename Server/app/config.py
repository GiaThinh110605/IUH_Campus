import os
from pathlib import Path
from dotenv import load_dotenv


def available_cpus():
    if hasattr(os, "sched_getaffinity"):
        return max(1, len(os.sched_getaffinity(0)))
    return os.cpu_count() or 1


def runtime_threads(value, cpus, workers):
    threads = int(value) if value and value.strip() else max(1, cpus // max(1, workers))
    if threads < 1:
        raise ValueError("ORT_INTRA_THREADS must be >= 1")
    return threads


def load_config():
    load_dotenv(Path(__file__).resolve().parents[1] / ".env", override=False)
    workers = max(1, int(os.getenv("WEB_CONCURRENCY", "2")))
    return {
        "ALLOWED_CLASSES": tuple(item.strip() for item in os.getenv("ALLOWED_CLASSES", "book,laptop,chair,bottle,backpack,cell phone").split(",") if item.strip()),
        "DATABASE_PATH": os.getenv("DATABASE_PATH", str(Path(__file__).resolve().parents[1] / "data/guardian.db")),
        "REWARDS_PATH": os.getenv("REWARDS_PATH", str(Path(__file__).resolve().parents[1] / "config/rewards.json")),
        "WEB_CONCURRENCY": workers,
        "ORT_INTRA_THREADS": runtime_threads(os.getenv("ORT_INTRA_THREADS", ""), available_cpus(), workers),
        "PORT": int(os.getenv("PORT", "5001")),
        "MODEL_PATH": os.getenv("MODEL_PATH", "/app/models/yolov8n.onnx"),
        "CONFIDENCE_THRESHOLD": float(os.getenv("CONFIDENCE_THRESHOLD", "0.70")),
        "LOG_LEVEL": os.getenv("LOG_LEVEL", "INFO").upper(),
        "ADAFRUIT_AIO_USERNAME": os.getenv("ADAFRUIT_AIO_USERNAME", ""),
        "ADAFRUIT_AIO_KEY": os.getenv("ADAFRUIT_AIO_KEY", ""),
        "MAX_CONTENT_LENGTH": 5 * 1024 * 1024,
        "SCAN_INFECTED_MIN": int(os.getenv("SCAN_INFECTED_MIN", "1")),
        "SCAN_INFECTED_MAX": int(os.getenv("SCAN_INFECTED_MAX", "2")),
        "SCAN_SESSION_TTL_MINUTES": float(os.getenv("SCAN_SESSION_TTL_MINUTES", "15")),
        "SCAN_HINTS_PATH": str(Path(__file__).resolve().parents[1] / "config/scan_hints.json"),
    }
