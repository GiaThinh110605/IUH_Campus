from concurrent.futures import ThreadPoolExecutor
from types import SimpleNamespace

import requests

from app import create_app
from app.cloud_client import AdafruitClient, retry_after_seconds
from app.event_store import EventStore
from app.sync_worker import SyncWorker
from test_gameplay import event


class FakeCloud:
    def __init__(self, success=True, error="network_error", retry_after=None):
        self.success, self.error, self.retry_after = success, error, retry_after
        self.sent = []

    def send(self, feed, value):
        self.sent.append((feed, value))
        return {"success": self.success, "error": None if self.success else self.error,
                "retry_after": self.retry_after}


def cloud_app(tmp_path):
    return create_app({"TESTING": True, "MODEL_PATH": "", "DATABASE_PATH": str(tmp_path / "cloud.db"),
                       "ADAFRUIT_AIO_USERNAME": "unit-user", "ADAFRUIT_AIO_KEY": "unit-test-not-a-secret"})


def one_row_store(tmp_path):
    store = EventStore(tmp_path / "outbox.db")
    with store.transaction() as db:
        db.execute("INSERT INTO outbox(feed_key,value,status,created_at) VALUES('guardian-campus-core',10,'pending',0)")
    return store


def test_coalesces_latest_totals_and_event_ack_waits_for_every_feed(tmp_path):
    app = cloud_app(tmp_path)
    client, scan, won = app.test_client(), event("scan_confirmed"), event()
    won["session_id"] = scan["session_id"]
    assert client.post("/gameplay/events", json=scan).json["cloud"] == "pending"
    assert client.post("/gameplay/events", json=won).status_code == 201
    store, cloud = app.extensions["event_store"], FakeCloud()
    assert store.summary()["outbox_pending"] == 4
    worker = SyncWorker(store, cloud)
    worker.step(now=100)
    assert store.get(won["event_id"])["cloud"] == "pending"
    for now in (104, 108, 112):
        assert worker.step(now=now)
    assert dict(cloud.sent) == {"guardian-quests-completed": 1, "guardian-campus-core": 10,
                                "guardian-upgrades": 0, "lost-found-count": 1}
    assert store.get(scan["event_id"])["cloud"] == "synced"
    assert client.post("/gameplay/events", json=won).json["cloud"] == "synced"
    metrics = client.get("/metrics").get_data(as_text=True)
    assert 'cloud_sync_total{result="synced"} 4.0' in metrics
    assert "outbox_pending 0.0" in metrics
    assert 'gameplay_events_total{result="duplicate",type="combat_won"}' in metrics


def test_claim_is_atomic_and_global_rate_limit_survives_restart(tmp_path):
    store = one_row_store(tmp_path)
    with ThreadPoolExecutor(max_workers=2) as pool:
        jobs = list(pool.map(lambda _: store.claim(now=100), range(2)))
    jobs = [job for job in jobs if job]
    assert len(jobs) == 1 and jobs[0]["attempts"] == 1
    store.finish(jobs[0], True, now=100)
    with store.transaction() as db:
        db.execute("INSERT INTO outbox(feed_key,value,status,created_at) VALUES('lost-found-count',1,'pending',0)")
    restarted = EventStore(store.path)
    assert restarted.claim(now=102) is None
    assert restarted.claim(now=104) is not None


def test_retry_backoff_and_terminal_failure(tmp_path):
    store = one_row_store(tmp_path)
    worker, now = SyncWorker(store, FakeCloud(success=False)), 100
    for attempt in range(1, 9):
        assert worker.step(now=now)
        with store.connection() as db:
            row = db.execute("SELECT * FROM outbox").fetchone()
        assert row["attempts"] == attempt
        if attempt < 8:
            assert row["next_attempt_at"] == now + min(300, 5 * 2 ** (attempt - 1))
            now = row["next_attempt_at"]
    assert row["status"] == "failed" and row["last_error"] == "network_error"
    assert worker.step(now=now + 1000) is False


def test_429_honors_retry_after(tmp_path):
    store = one_row_store(tmp_path)
    worker = SyncWorker(store, FakeCloud(False, "http_429", 30))
    assert worker.step(now=100)
    assert store.claim(now=129) is None
    assert store.claim(now=130)["attempts"] == 2
    assert retry_after_seconds("30") == 30
    assert retry_after_seconds("not-a-date") is None
    assert retry_after_seconds("Thu, 01 Jan 1970 00:02:10 GMT", now=100) == 30


def test_disabled_worker_does_not_claim_or_send(tmp_path):
    store = one_row_store(tmp_path)
    assert SyncWorker(store).step(now=100) is False
    with store.connection() as db:
        assert db.execute("SELECT attempts FROM outbox").fetchone()[0] == 0


def test_expired_lease_coalesces_newer_event_and_old_ack_is_rejected(tmp_path):
    app = cloud_app(tmp_path)
    scan, won = event("scan_confirmed"), event()
    client, store = app.test_client(), app.extensions["event_store"]
    client.post("/gameplay/events", json=scan)
    old_job = store.claim(now=100)
    client.post("/gameplay/events", json=won)
    cloud, worker = FakeCloud(), None
    worker = SyncWorker(EventStore(store.path), cloud)
    for now in (131, 135, 139, 143):
        assert worker.step(now=now)
    assert dict(cloud.sent)["guardian-quests-completed"] == 1
    assert store.get(scan["event_id"])["cloud"] == "synced"
    assert store.get(won["event_id"])["cloud"] == "synced"
    assert store.finish(old_job, True, now=144) is False


def test_attempt_limit_also_applies_after_crash(tmp_path):
    store = one_row_store(tmp_path)
    with store.transaction() as db:
        db.execute("UPDATE outbox SET attempts=8")
    assert store.claim(now=100) is None
    assert store.summary()["outbox_failed"] == 1


def test_http_client_never_retains_secret_in_error_and_uses_timeout():
    def fail(method, url, **kwargs):
        assert kwargs["timeout"] == 5 and kwargs["allow_redirects"] is False
        raise requests.Timeout("unit-test-not-a-secret must not escape")
    client = AdafruitClient("unit-user", "unit-test-not-a-secret", SimpleNamespace(request=fail))
    assert client.send("guardian-campus-core", 10) == {"success": False, "error": "network_error", "retry_after": None}


def test_feed_setup_gets_before_creating_and_redacts_output():
    calls = []
    responses = [SimpleNamespace(status_code=404), SimpleNamespace(status_code=201)]
    def request(method, url, **kwargs):
        calls.append((method, kwargs))
        return responses.pop(0)
    client = AdafruitClient("unit-user", "unit-test-not-a-secret", SimpleNamespace(request=request))
    result = client.ensure_feed("guardian-campus-core")
    assert result["status"] == "created"
    assert [call[0] for call in calls] == ["GET", "POST"]
    assert "unit-test-not-a-secret" not in str(result)


def test_event_and_reward_rollback_with_outbox_failure(tmp_path, monkeypatch):
    import pytest
    app = cloud_app(tmp_path)
    store, body = app.extensions["event_store"], event()
    def fail(*args):
        raise RuntimeError("test rollback")
    monkeypatch.setattr(store, "_queue_snapshot", fail)
    with pytest.raises(RuntimeError, match="test rollback"):
        app.test_client().post("/gameplay/events", json=body)
    assert store.get(body["event_id"]) is None
    assert store.summary()["campus_core_total"] == 0
    with store.connection() as db:
        assert db.execute("SELECT count(*) FROM milestone_rewards").fetchone()[0] == 0


def test_event_reports_terminal_cloud_failure(tmp_path):
    app = cloud_app(tmp_path)
    body, store = event(), app.extensions["event_store"]
    app.test_client().post("/gameplay/events", json=body)
    with store.transaction() as db:
        db.execute("UPDATE outbox SET attempts=7")
    assert SyncWorker(store, FakeCloud(success=False)).step(now=100)
    assert app.test_client().post("/gameplay/events", json=body).json["cloud"] == "failed"


def test_newer_coalesced_value_does_not_bypass_existing_backoff(tmp_path):
    store = one_row_store(tmp_path)
    old = store.claim(now=100)
    with store.transaction() as db:
        db.execute("INSERT INTO outbox(feed_key,value,status,created_at) VALUES('guardian-campus-core',20,'pending',101)")
    store.finish(old, False, "network_error", now=100)
    assert store.claim(now=103.2) is None
    job = store.claim(now=105)
    assert job["value"] == 20 and job["attempts"] == 2
