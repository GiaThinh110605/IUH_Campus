"""Opt-in manual telemetry using only public scan APIs; never inspect hidden classes."""
import argparse
from datetime import datetime, timezone
import json
from pathlib import Path
import time
from uuid import uuid4

import requests

DEFAULT_BASE_URL = "http://localhost:5001"
DEFAULT_MAX_SESSIONS = 12
MAX_ALLOWED_SESSIONS = 20
REQUEST_TIMEOUT = 10
PREDICT_TIMEOUT = 30
CLOUD_POLL_ATTEMPTS = 40
CLOUD_POLL_INTERVAL_SECONDS = 1
CLOUD_FINAL_STATES = ("synced", "disabled", "failed")
FIXTURE = "Server/tests/fixtures/laptop-stocksnap.jpg"
EVIDENCE_DIR = "Docs/Evidence/CampusGuardian/B6"
SESSION_PUBLIC_KEYS = {"scan_session_id", "hint", "max_monsters", "expires_at"}


def save(path, value):
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2), encoding="utf-8")


def _parse_args():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--base-url", default=DEFAULT_BASE_URL)
    parser.add_argument("--max-sessions", type=int, default=DEFAULT_MAX_SESSIONS)
    args = parser.parse_args()
    if not 1 <= args.max_sessions <= MAX_ALLOWED_SESSIONS:
        parser.error("Use max-sessions1..20")
    return args


def _find_infested(client, base, root, max_sessions, report, evidence):
    token, game, public = None, None, None
    for _ in range(max_sessions):
        game = str(uuid4())
        response = client.post(base + "/scan/sessions", json={"session_id": game}, timeout=REQUEST_TIMEOUT)
        assert response.status_code == 201
        public = response.json()
        assert set(public) == SESSION_PUBLIC_KEYS
        # Each attempt is a separate simulated game; selection remains genuinely random.
        with (root / FIXTURE).open("rb") as image:
            response = client.post(
                base + "/predict",
                files={"image": image},
                data={"scan_session_id": public["scan_session_id"]},
                timeout=PREDICT_TIMEOUT,
            )
        response.raise_for_status()
        prediction = response.json()
        report["attempts"].append({
            "create_request": {"session_id": game},
            "create_status": 201,
            "created": public,
            "predict_status": response.status_code,
            "prediction": prediction,
        })
        save(evidence / "live-scan.json", report)
        if prediction["scan"]["result"] == "infested":
            token = prediction["scan"]["monster_token"]
            break
    return token, game, public


def _check_event_idempotency(client, base, event, report):
    first = client.post(base + "/gameplay/events", json=event, timeout=REQUEST_TIMEOUT)
    assert first.status_code == 201
    duplicate = client.post(base + "/gameplay/events", json=event, timeout=REQUEST_TIMEOUT)
    assert duplicate.status_code == 200 and duplicate.json()["status"] == "duplicate"
    other = dict(event, event_id=str(uuid4()))
    repeated = client.post(base + "/gameplay/events", json=other, timeout=REQUEST_TIMEOUT)
    assert repeated.status_code == 400
    report.update(
        event_request=event,
        recorded=first.json(),
        duplicate=duplicate.json(),
        new_event_same_token={"status": repeated.status_code, "response": repeated.json()},
    )
    return first, duplicate, repeated


def _wait_for_cloud(client, base, event_id, report):
    for _ in range(CLOUD_POLL_ATTEMPTS):
        response = client.get(base + "/gameplay/events/" + event_id, timeout=REQUEST_TIMEOUT)
        response.raise_for_status()
        report["cloud_status"] = response.json()
        if report["cloud_status"]["cloud"] in CLOUD_FINAL_STATES:
            break
        time.sleep(CLOUD_POLL_INTERVAL_SECONDS)


def main():
    args = _parse_args()
    root = Path(__file__).resolve().parents[2]
    evidence = root / EVIDENCE_DIR
    evidence.mkdir(parents=True, exist_ok=True)
    report = {"time_utc": datetime.now(timezone.utc).isoformat(), "attempts": [], "manual_telemetry": True}
    base = args.base_url.rstrip("/")
    with requests.Session() as client:
        health = client.get(base + "/health", timeout=REQUEST_TIMEOUT)
        health.raise_for_status()
        report["health"] = health.json()
        assert report["health"]["model_loaded"]
        token, game, public = _find_infested(client, base, root, args.max_sessions, report, evidence)
        if token is None:
            print("No infested fixture match in bounded random attempts; evidence saved, no results fabricated.")
            return 2
        event = {
            "event_id": str(uuid4()),
            "session_id": game,
            "quest_id": "B6_smoke",
            "milestone_id": "ar_monster",
            "event_type": "combat_won",
            "occurred_at": datetime.now(timezone.utc).isoformat(),
            "payload": {"monster_token": token},
        }
        first, duplicate, repeated = _check_event_idempotency(client, base, event, report)
        _wait_for_cloud(client, base, event["event_id"], report)
        report["session_status"] = client.get(
            base + "/scan/sessions/" + public["scan_session_id"], timeout=REQUEST_TIMEOUT
        ).json()
        save(evidence / "live-scan.json", report)
        print(json.dumps({
            "attempts": len(report["attempts"]),
            "predict_scan_result": "infested",
            "recorded_http": first.status_code,
            "duplicate_http": duplicate.status_code,
            "core_awarded": first.json()["core_awarded"],
            "same_token_new_event_http": repeated.status_code,
            "cloud": report["cloud_status"]["cloud"],
        }))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
