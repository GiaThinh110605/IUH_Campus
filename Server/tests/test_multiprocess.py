import os
from pathlib import Path
import runpy
import subprocess
import sys
from unittest.mock import Mock


SERVER = Path(__file__).resolve().parents[1]


def test_metrics_aggregate_two_processes(tmp_path):
    environment = os.environ.copy()
    environment["PROMETHEUS_MULTIPROC_DIR"] = str(tmp_path)
    environment["MODEL_PATH"] = ""
    worker = "from app import create_app; a=create_app({'TESTING':True}); a.test_client().get('/health')"
    for _ in range(2):
        subprocess.run([sys.executable, "-c", worker], cwd=SERVER, env=environment,
                       capture_output=True, text=True, check=True)
    scrape = "from app import create_app; a=create_app({'TESTING':True}); print(a.test_client().get('/metrics').get_data(as_text=True))"
    result = subprocess.run([sys.executable, "-c", scrape], cwd=SERVER, env=environment,
                            capture_output=True, text=True, check=True)
    assert 'flask_http_request_total{method="GET",status="200"} 2.0' in result.stdout
    assert "model_loaded 0.0" in result.stdout
    assert "flask_http_request_duration_seconds" in result.stdout


def test_startup_clears_only_metric_database_files(tmp_path, monkeypatch):
    monkeypatch.setenv("PROMETHEUS_MULTIPROC_DIR", str(tmp_path))
    stale = tmp_path / "counter_123.db"
    stale.write_bytes(b"stale")
    keep = tmp_path / "keep.txt"
    keep.write_text("keep", encoding="utf-8")
    config = runpy.run_path(str(SERVER / "gunicorn.conf.py"))
    config["on_starting"](Mock())
    assert not stale.exists()
    assert keep.read_text(encoding="utf-8") == "keep"
    assert config["workers"] == 2
    assert config["preload_app"] is False
