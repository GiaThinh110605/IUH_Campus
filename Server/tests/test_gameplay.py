from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timedelta, timezone
from uuid import uuid4

import pytest

from app import create_app


def event(kind="combat_won"):
    data = {"event_id": str(uuid4()), "session_id": str(uuid4()), "quest_id": "library",
            "milestone_id": "library_intro", "event_type": kind,
            "occurred_at": datetime.now(timezone.utc).isoformat()}
    if kind == "scan_confirmed":
        data["payload"] = {"label": "book", "confidence": .9}
    if kind == "upgrade_purchased":
        data["payload"] = {"branch": "attack", "level": 1}
    return data


@pytest.fixture
def app(tmp_path):
    return create_app({"TESTING": True, "MODEL_PATH": "", "DATABASE_PATH": str(tmp_path / "events.db")})


def test_record_duplicate_conflict_and_get(app):
    client, body = app.test_client(), event()
    first = client.post("/gameplay/events", json=body)
    assert first.status_code == 201
    assert first.json["core_awarded"] == 10
    assert first.json["cloud"] == "disabled"
    again = client.post("/gameplay/events", json=body)
    assert again.status_code == 200 and again.json["status"] == "duplicate"
    assert again.json["core_awarded"] == 10
    changed = dict(body, quest_id="changed")
    assert client.post("/gameplay/events", json=changed).status_code == 409
    assert client.get("/gameplay/events/" + body["event_id"]).json["cloud"] == "disabled"
    assert client.get("/gameplay/events/" + str(uuid4())).status_code == 404


def test_reward_only_first_session_milestone(app):
    client, body = app.test_client(), event()
    assert client.post("/gameplay/events", json=body).json["core_awarded"] == 10
    body["event_id"] = str(uuid4())
    assert client.post("/gameplay/events", json=body).json["core_awarded"] == 0
    summary = app.extensions["event_store"].summary()
    assert summary["campus_core_total"] == 10 and summary["quests_completed"] == 1


def test_restart_and_two_workers_share_dedupe(app):
    body = event()
    def send(_):
        with app.test_client() as client:
            return client.post("/gameplay/events", json=body).status_code
    with ThreadPoolExecutor(max_workers=2) as pool:
        assert sorted(pool.map(send, range(2))) == [200, 201]
    restarted = create_app(dict(app.config))
    assert restarted.test_client().post("/gameplay/events", json=body).status_code == 200
    assert restarted.extensions["event_store"].summary()["campus_core_total"] == 10


@pytest.mark.parametrize("field,value", [
    ("event_id", "bad"), ("session_id", "bad"), ("quest_id", "bad/id"),
    ("milestone_id", "x" * 65), ("event_type", "unknown"), ("occurred_at", "2026-01-01T00:00:00"),
    ("occurred_at", "bad"), ("core", 999),
    ("event_type", []), ("event_type", {}),
])
def test_invalid_fields_are_400(app, field, value):
    body = event()
    body[field] = value
    response = app.test_client().post("/gameplay/events", json=body)
    assert response.status_code == 400
    assert response.json["field"] == field


@pytest.mark.parametrize("field", ["event_id", "session_id", "quest_id", "milestone_id", "event_type", "occurred_at"])
def test_required_fields(app, field):
    body = event()
    del body[field]
    response = app.test_client().post("/gameplay/events", json=body)
    assert response.status_code == 400 and response.json["field"] == field


def test_future_body_limit_and_non_json(app):
    client, body = app.test_client(), event()
    body["occurred_at"] = (datetime.now(timezone.utc) + timedelta(minutes=6)).isoformat()
    assert client.post("/gameplay/events", json=body).status_code == 400
    response = client.post("/gameplay/events", data=b" " * 17000, content_type="application/json")
    assert response.status_code == 400 and response.json["field"] == "body"
    assert client.post("/gameplay/events", data="not json").status_code == 400


@pytest.mark.parametrize("payload", [{"label": "spaceship", "confidence": .9},
                                      {"label": "book", "confidence": .4},
                                      {"label": "book", "confidence": True},
                                      {"label": "book", "confidence": .9, "core": 10}])
def test_scan_payload_whitelist(app, payload):
    body = event("scan_confirmed")
    body["payload"] = payload
    assert app.test_client().post("/gameplay/events", json=body).status_code == 400


@pytest.mark.parametrize("payload", [{"branch": "speed", "level": 1}, {"branch": "attack", "level": 3},
                                      {"branch": "attack", "level": True}, {"branch": "attack", "level": 1, "core": 1}])
def test_upgrade_payload_whitelist(app, payload):
    body = event("upgrade_purchased")
    body["payload"] = payload
    assert app.test_client().post("/gameplay/events", json=body).status_code == 400


def test_all_types_aggregates_and_health(app):
    client = app.test_client()
    for kind in ["scan_confirmed", "upgrade_purchased", "combat_lost"]:
        assert client.post("/gameplay/events", json=event(kind)).status_code == 201
    summary = app.extensions["event_store"].summary()
    assert summary["scans_confirmed"] == 1 and summary["upgrades_total"] == 1
    assert summary["campus_core_total"] == 0
    health = client.get("/health").json
    assert health["db_ok"] and not health["cloud_configured"]
    assert health["outbox_pending"] == 0


def test_health_reports_database_failure(app, monkeypatch):
    import sqlite3
    def fail():
        raise sqlite3.OperationalError("test unavailable")
    monkeypatch.setattr(app.extensions["event_store"], "summary", fail)
    response = app.test_client().get("/health")
    assert response.status_code == 503 and response.json["db_ok"] is False
    assert response.json["outbox_pending"] is None


@pytest.mark.parametrize("body", [None, [], "text", 123])
def test_non_object_json_is_400(app, body):
    response = app.test_client().post("/gameplay/events", data=__import__("json").dumps(body), content_type="application/json")
    assert response.status_code == 400 and response.json["field"] == "body"


def test_unknown_reward_is_rejected_and_transaction_rolls_back(app):
    body = event()
    body["milestone_id"] = "not_configured"
    assert app.test_client().post("/gameplay/events", json=body).status_code == 400
    assert app.extensions["event_store"].get(body["event_id"]) is None
    assert app.extensions["event_store"].summary()["campus_core_total"] == 0
