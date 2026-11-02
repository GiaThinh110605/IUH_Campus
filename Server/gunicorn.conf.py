import os
from pathlib import Path
import tempfile
from dotenv import load_dotenv

load_dotenv(Path(__file__).parent / ".env", override=False)
os.environ.setdefault("PROMETHEUS_MULTIPROC_DIR", "/tmp/iuh-guardian-prometheus")
bind = f"0.0.0.0:{int(os.getenv('PORT', '5001'))}"
workers = 2
timeout = 60
accesslog = "-"
errorlog = "-"
loglevel = os.getenv("LOG_LEVEL", "INFO").lower()
preload_app = False


def on_starting(server):
    directory = Path(os.environ["PROMETHEUS_MULTIPROC_DIR"]).resolve()
    temporary_root = Path(tempfile.gettempdir()).resolve()
    if directory == temporary_root or temporary_root not in directory.parents:
        raise ValueError("PROMETHEUS_MULTIPROC_DIR must be a dedicated temporary subdirectory")
    directory.mkdir(parents=True, exist_ok=True)
    for file in directory.glob("*.db"):
        if file.is_file() and not file.is_symlink():
            file.unlink()
    server.log.info("[IUH Guardian API] Multiprocess metrics directory initialized")


def child_exit(server, worker):
    from prometheus_flask_exporter.multiprocess import GunicornPrometheusMetrics
    GunicornPrometheusMetrics.mark_process_dead_on_child_exit(worker.pid)
