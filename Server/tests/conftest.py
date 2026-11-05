from pathlib import Path
import sys
import os
import pytest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))


@pytest.fixture(scope="session", autouse=True)
def isolate_session_environment(tmp_path_factory):
    fields = ("ADAFRUIT_AIO_USERNAME", "ADAFRUIT_AIO_KEY", "DATABASE_PATH")
    original = {field: os.environ.get(field) for field in fields}
    os.environ.update(ADAFRUIT_AIO_USERNAME="", ADAFRUIT_AIO_KEY="",
                      DATABASE_PATH=str(tmp_path_factory.mktemp("guardian-session") / "guardian.db"))
    try:
        yield
    finally:
        for field, value in original.items():
            if value is None:
                os.environ.pop(field, None)
            else:
                os.environ[field] = value


@pytest.fixture(autouse=True)
def isolate_credentials_and_database(monkeypatch, tmp_path):
    monkeypatch.setenv("ADAFRUIT_AIO_USERNAME", "")
    monkeypatch.setenv("ADAFRUIT_AIO_KEY", "")
    monkeypatch.setenv("DATABASE_PATH", str(tmp_path / "guardian.db"))
    import requests
    def deny_live_network(*args, **kwargs):
        raise AssertionError("Unit tests must not call live Adafruit IO")
    monkeypatch.setattr(requests.sessions.Session, "request", deny_live_network)
