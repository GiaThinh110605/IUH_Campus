from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timezone
import io
import json
from uuid import uuid4

from PIL import Image
import pytest

from app import create_app


@pytest.fixture
def app(tmp_path, monkeypatch):
    class FixedRandom:
        def randint(self, minimum, maximum):
            return maximum
        def sample(self, population, count):
            return ["book", "bottle"][:count]
    monkeypatch.setattr("secrets.SystemRandom", FixedRandom)
    result = create_app({"TESTING": True, "MODEL_PATH": "", "DATABASE_PATH": str(tmp_path / "scan.db")})
    result.extensions["model_registry"].session = object()
    return result


def start(app, game=None):
    game = game or str(uuid4())
    response = app.test_client().post("/scan/sessions", json={"session_id": game})
    assert response.status_code == 201
    return game, response.json["scan_session_id"], response.json


def predict(app, monkeypatch, scan_id=None, label="book", confidence=.9):
    def detect(image, threshold, target):
        detections = [] if label is None else [{"label": label, "class_id": 73, "confidence": confidence, "bbox": [0, 0, 2, 2]}]
        return {"detections": detections, "target_class": target, "target_found": False,
                "best_confidence": None, "threshold": threshold, "image_size": [4, 4],
                "inference_time_ms": 1, "total_time_ms": 2, "model": {"name": "yolov8n", "sha256_prefix": "test"}}
    monkeypatch.setattr(app.extensions["detector"], "predict", detect)
    image = io.BytesIO()
    Image.new("RGB", (4, 4)).save(image, format="PNG")
    image.seek(0)
    data = {"image": (image, "frame.png")}
    if scan_id is not None:
        data["scan_session_id"] = scan_id
    return app.test_client().post("/predict", data=data)


def win(game, token):
    return {"event_id": str(uuid4()), "session_id": game, "quest_id": "scan_test",
            "milestone_id": "ar_monster", "event_type": "combat_won",
            "occurred_at": datetime.now(timezone.utc).isoformat(), "payload": {"monster_token": token}}


def test_session_hint_and_private_contract(app):
    _, scan, body = start(app)
    assert set(body) == {"scan_session_id", "hint", "max_monsters", "expires_at"}
    assert body["max_monsters"] == 2
    assert "thứ chứa chữ" in body["hint"] and "thứ đựng nước" in body["hint"]
    status = app.test_client().get("/scan/sessions/" + scan)
    assert status.status_code == 200
    assert status.json["revealed_count"] == 0 and status.json["monsters_remaining"] == 2
    assert "infected_classes" not in status.json
    assert "book" not in json.dumps(body) and "bottle" not in json.dumps(status.json)


@pytest.mark.parametrize("body", [{}, {"session_id": "bad"}, {"session_id": []}, {"session_id": str(uuid4()), "classes": ["book"]}])
def test_create_validation(app, body):
    assert app.test_client().post("/scan/sessions", json=body).status_code == 400


def test_not_found_expired_and_empty_scan_id(app, monkeypatch):
    missing = str(uuid4())
    response = predict(app, monkeypatch, missing)
    assert response.status_code == 404 and response.json["error"] == "scan_session_not_found"
    assert app.test_client().get("/scan/sessions/" + missing).status_code == 404
    _, scan, _ = start(app)
    app.extensions["scan_store"].clock = lambda: 10**12
    response = predict(app, monkeypatch, scan)
    assert response.status_code == 410 and response.json["error"] == "scan_session_expired"
    assert app.test_client().get("/scan/sessions/" + scan).status_code == 410
    assert predict(app, monkeypatch, "").status_code == 404


def test_infested_clean_none_idempotence_and_limit(app, monkeypatch):
    _, scan, _ = start(app)
    first = predict(app, monkeypatch, scan).json["scan"]
    assert first["result"] == "infested" and first["monsters_remaining"] == 1
    assert predict(app, monkeypatch, scan).json["scan"] == first
    clean = predict(app, monkeypatch, scan, "chair").json["scan"]
    assert clean["result"] == "clean" and clean["monster_token"] is None
    assert predict(app, monkeypatch, scan, None).json["scan"]["result"] == "none"
    assert predict(app, monkeypatch, scan, "book", .69).json["scan"]["result"] == "none"
    second = predict(app, monkeypatch, scan, "bottle").json["scan"]
    assert second["monsters_remaining"] == 0 and second["monster_token"] != first["monster_token"]
    assert predict(app, monkeypatch, scan).json["scan"]["monster_token"] == first["monster_token"]
    status = app.test_client().get("/scan/sessions/" + scan).json
    assert status["revealed_count"] == 2 and status["monsters_remaining"] == 0
    assert app.extensions["event_store"].summary()["monsters_revealed"] == 2


def test_legacy_predict_schema_unchanged(app, monkeypatch):
    body = predict(app, monkeypatch).json
    assert set(body) == {"detections", "target_class", "target_found", "best_confidence", "threshold", "image_size",
                         "inference_time_ms", "total_time_ms", "model"}


def test_token_reward_once_and_restart(app, monkeypatch):
    game, scan, _ = start(app)
    token = predict(app, monkeypatch, scan).json["scan"]["monster_token"]
    body = win(game, token)
    client = app.test_client()
    first = client.post("/gameplay/events", json=body)
    assert first.status_code == 201 and first.json["core_awarded"] == 10
    duplicate = client.post("/gameplay/events", json=body)
    assert duplicate.status_code == 200 and duplicate.json["core_awarded"] == 10
    assert client.post("/gameplay/events", json=win(game, token)).status_code == 400
    restarted = create_app(dict(app.config))
    assert restarted.test_client().post("/gameplay/events", json=body).status_code == 200
    assert restarted.test_client().get("/scan/sessions/" + scan).json["revealed_count"] == 1
    assert restarted.extensions["scan_store"].reveal(scan, "book", False)["monster_token"] == token
    assert restarted.extensions["event_store"].summary()["campus_core_total"] == 10


def test_wrong_game_unknown_token_and_payload_fields(app, monkeypatch):
    game, scan, _ = start(app)
    token = predict(app, monkeypatch, scan).json["scan"]["monster_token"]
    client = app.test_client()
    for body in (win(str(uuid4()), token), win(game, "unknown"), win(game, []), win(game, True)):
        assert client.post("/gameplay/events", json=body).status_code == 400
    body = win(game, token)
    body["payload"]["core"] = 999
    assert client.post("/gameplay/events", json=body).status_code == 400
    body = win(game, token)
    body["event_type"] = "monster_revealed"
    assert client.post("/gameplay/events", json=body).status_code == 400
    assert app.extensions["event_store"].summary()["campus_core_total"] == 0


def test_two_workers_reveal_and_reward_once(app):
    game, scan, _ = start(app)
    def reveal(_):
        return app.extensions["scan_store"].reveal(scan, "book", False)
    with ThreadPoolExecutor(max_workers=2) as pool:
        values = list(pool.map(reveal, range(2)))
    assert values[0]["monster_token"] == values[1]["monster_token"]
    assert app.extensions["event_store"].summary()["monsters_revealed"] == 1
    def reward(_):
        with app.test_client() as client:
            return client.post("/gameplay/events", json=win(game, values[0]["monster_token"])).status_code
    with ThreadPoolExecutor(max_workers=2) as pool:
        assert sorted(pool.map(reward, range(2))) == [201, 400]
    assert app.extensions["event_store"].summary()["campus_core_total"] == 10


def test_internal_event_outbox_and_private_metrics(app):
    _, scan, _ = start(app)
    store = app.extensions["event_store"]
    app.extensions["scan_store"].reveal(scan, "book", True)
    app.extensions["scan_store"].reveal(scan, "book", True)
    app.test_client().get("/scan/sessions/" + scan)
    with store.connection() as db:
        assert db.execute("SELECT count(*) FROM events WHERE event_type='monster_revealed'").fetchone()[0] == 1
        assert db.execute("SELECT count(*) FROM outbox WHERE feed_key='guardian-monsters-revealed'").fetchone()[0] == 1
    metrics = app.test_client().get("/metrics").text
    assert 'monsters_revealed_total{label="book"} 1.0' in metrics
    assert "scan_sessions_total 1.0" in metrics
    assert 'scan_results_total{result="infested"} 2.0' in metrics
    assert scan not in metrics and "infected_classes" not in metrics


def test_single_monster_and_ttl_config(app):
    from app.scan_store import ScanStore
    config = dict(app.config, SCAN_INFECTED_MIN=1, SCAN_INFECTED_MAX=1, SCAN_SESSION_TTL_MINUTES=2)
    store = ScanStore(app.extensions["event_store"], config, clock=lambda: 1000)
    body = store.create(str(uuid4()))
    assert body["max_monsters"] == 1
    assert store.reveal(body["scan_session_id"], "book", False)["monsters_remaining"] == 0
    store.clock = lambda: 1120
    with pytest.raises(Exception, match="scan_session_expired"):
        store.status(body["scan_session_id"])


def test_two_tokens_award_at_most_twenty_core(app, monkeypatch):
    game, scan, _ = start(app)
    tokens = [predict(app, monkeypatch, scan, label).json["scan"]["monster_token"] for label in ("book", "bottle")]
    for token in tokens:
        assert app.test_client().post("/gameplay/events", json=win(game, token)).json["core_awarded"] == 10
        assert app.test_client().post("/gameplay/events", json=win(game, token)).status_code == 400
    assert app.extensions["event_store"].summary()["campus_core_total"] == 20


def test_ar_reward_key_requires_token_but_legacy_milestone_still_works(app):
    game = str(uuid4())
    body = win(game, "unused")
    body.pop("payload")
    assert app.test_client().post("/gameplay/events", json=body).status_code == 400
    body["milestone_id"] = "library_intro"
    assert app.test_client().post("/gameplay/events", json=body).json["core_awarded"] == 10


def test_reveal_rolls_back_event_aggregate_and_token_on_outbox_error(app, monkeypatch):
    _, scan, _ = start(app)
    store = app.extensions["event_store"]
    def fail(*args):
        raise RuntimeError("injected transactional error")
    monkeypatch.setattr(store, "_queue_snapshot", fail)
    with pytest.raises(RuntimeError, match="transactional error"):
        app.extensions["scan_store"].reveal(scan, "book", True)
    assert store.summary()["monsters_revealed"] == 0
    with store.connection() as db:
        assert db.execute("SELECT count(*) FROM scan_reveals").fetchone()[0] == 0
        assert db.execute("SELECT count(*) FROM events").fetchone()[0] == 0


def test_migration_preserves_existing_aggregates_and_is_repeatable(tmp_path):
    import sqlite3
    from app.event_store import SCHEMA, EventStore
    path = tmp_path / "legacy.db"
    with sqlite3.connect(path) as db:
        db.executescript(SCHEMA)
        db.execute("UPDATE aggregates SET campus_core_total=20,scans_confirmed=3 WHERE id=1")
    for _ in range(2):
        summary = EventStore(path).summary()
        assert summary["campus_core_total"] == 20 and summary["scans_confirmed"] == 3
        assert summary["monsters_revealed"] == 0


@pytest.mark.parametrize("minimum,maximum,ttl", [(0, 2, 15), (1, 3, 15), (2, 1, 15), (1, 2, 0)])
def test_invalid_scan_config_fails_startup(app, minimum, maximum, ttl):
    from app.scan_store import ScanStore
    with pytest.raises(ValueError, match="Invalid scan configuration"):
        ScanStore(app.extensions["event_store"], dict(app.config, SCAN_INFECTED_MIN=minimum,
                  SCAN_INFECTED_MAX=maximum, SCAN_SESSION_TTL_MINUTES=ttl))


def test_electronics_hints_and_exact_six_class_dictionary(app, monkeypatch):
    import secrets
    from app.scan_store import ScanStore
    hints = app.extensions["scan_store"].hints
    assert hints == {"laptop": "đồ điện tử", "cell phone": "đồ điện tử", "bottle": "thứ đựng nước",
                     "book": "thứ chứa chữ", "backpack": "thứ mang trên vai", "chair": "thứ để ngồi"}
    class Electronics:
        def randint(self, lo, hi):
            return 2
        def sample(self, population, count):
            return ["laptop", "cell phone"]
    monkeypatch.setattr(secrets, "SystemRandom", Electronics)
    result = ScanStore(app.extensions["event_store"], app.config).create(str(uuid4()))
    assert result["hint"].count("đồ điện tử") == 2
    assert result["max_monsters"] == 2
