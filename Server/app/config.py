import os
from pathlib import Path
from dotenv import load_dotenv


def load_config():
    load_dotenv(Path(__file__).resolve().parents[1] / ".env", override=False)
    return {
        "ALLOWED_CLASSES": tuple(item.strip() for item in os.getenv("ALLOWED_CLASSES", "book,laptop,chair,bottle,backpack,cell phone").split(",") if item.strip()),
        "ORT_INTRA_OP_THREADS": int(os.getenv("ORT_INTRA_OP_THREADS", "2")),
        "PORT": int(os.getenv("PORT", "5001")),
        "MODEL_PATH": os.getenv("MODEL_PATH", "/app/models/yolov8n.onnx"),
        "CONFIDENCE_THRESHOLD": float(os.getenv("CONFIDENCE_THRESHOLD", "0.70")),
        "LOG_LEVEL": os.getenv("LOG_LEVEL", "INFO").upper(),
        "ADAFRUIT_AIO_USERNAME": os.getenv("ADAFRUIT_AIO_USERNAME", ""),
        "ADAFRUIT_AIO_KEY": os.getenv("ADAFRUIT_AIO_KEY", ""),
        "MAX_CONTENT_LENGTH": 5 * 1024 * 1024,
    }
