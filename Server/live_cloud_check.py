import argparse
from datetime import datetime, timezone
import json
from pathlib import Path
import time
from uuid import uuid4

import requests

from app.cloud_client import AdafruitClient, cloud_configured
from app.config import load_config
from app.event_store import FEEDS


def safe_write(path, report, config):
    text = json.dumps(report, indent=2)
    for field in ("ADAFRUIT_AIO_KEY",):
        value = config.get(field, "")
        if value:
            text = text.replace(value, "****")
    path.write_text(text + "\n", encoding="utf-8")


def sample_events():
    shared = {"session_id": str(uuid4()), "quest_id": "B3_acceptance", "milestone_id": "library_intro",
              "occurred_at": datetime.now(timezone.utc).isoformat()}
    scan = dict(shared, event_id=str(uuid4()), event_type="scan_confirmed", payload={"label": "book", "confidence": .9})
    won = dict(shared, event_id=str(uuid4()), event_type="combat_won")
    return scan, won


def post_event(url, body):
    response = requests.post(url + "/gameplay/events", json=body, timeout=10)
    return {"http_status": response.status_code, "body": response.json()}


def wait_for_sync(url, events):
    deadline = time.monotonic() + 120
    while time.monotonic() < deadline:
        states = [requests.get(url + "/gameplay/events/" + event["event_id"], timeout=10).json() for event in events]
        if all(state.get("cloud") == "synced" for state in states):
            return states
        if any(state.get("cloud") == "failed" for state in states):
            return states
        time.sleep(1)
    return states


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--phase", choices=["before", "after"], required=True)
    parser.add_argument("--url", default="http://localhost:5001")
    parser.add_argument("--evidence", type=Path, required=True)
    args = parser.parse_args()
    config = load_config()
    if not cloud_configured(config):
        print("[IUH Guardian API] Live cloud check disabled; credentials=****")
        return 2
    args.evidence.mkdir(parents=True, exist_ok=True)
    report = {"credentials": "****", "phase": args.phase, "date": "2026-11-02 Asia/Bangkok"}
    if args.phase == "before":
        scan, won = sample_events()
        report["requests"] = [scan, won]
        report["responses"] = [post_event(args.url, body) for body in (scan, won, won)]
        safe_write(args.evidence / "live-events.json", report, config)
    else:
        previous = json.loads((args.evidence / "live-events.json").read_text(encoding="utf-8"))
        scan, won = previous["requests"]
        report["duplicate_after_restart"] = post_event(args.url, won)
    report["states"] = wait_for_sync(args.url, (scan, won))
    client = AdafruitClient(config["ADAFRUIT_AIO_USERNAME"], config["ADAFRUIT_AIO_KEY"])
    report["feed_values"] = [client.last_value(feed) for feed in FEEDS]
    report["all_synced"] = len(report["states"]) == 2 and all(state.get("cloud") == "synced" for state in report["states"])
    report["feeds_readable"] = all("value" in row for row in report["feed_values"])
    if args.phase == "before":
        responses = report["responses"]
        report["api_contract_ok"] = ([row["http_status"] for row in responses] == [201, 201, 200] and
                                     responses[1]["body"]["core_awarded"] == responses[2]["body"]["core_awarded"])
    else:
        report["api_contract_ok"] = (report["duplicate_after_restart"]["http_status"] == 200 and
                                     report["duplicate_after_restart"]["body"].get("status") == "duplicate")
    safe_write(args.evidence / ("live-events.json" if args.phase == "before" else "live-after-restart.json"), report, config)
    print(json.dumps({"phase": args.phase, "all_synced": report["all_synced"], "credentials": "****"}))
    return 0 if report["all_synced"] and report["feeds_readable"] and report["api_contract_ok"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
