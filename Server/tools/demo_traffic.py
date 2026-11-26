"""Explicit opt-in demo: real predictions and scan telemetry, no Core rewards."""
import argparse
from datetime import datetime, timezone
import json
from pathlib import Path
import time
from uuid import uuid4

import requests

EVENT_INTERVAL_SECONDS = 3.1
PREDICT_TIMEOUT = 15
EVENT_TIMEOUT = 10
LOOP_SLEEP_CAP_SECONDS = 1
DEMO_CONFIDENCE = 0.9
MAX_DEMO_SECONDS = 60
DEFAULT_BASE_URL = "http://localhost:5001"


def _post_demo_event(client, base_url, session_id):
    # Manual demo telemetry, not a claim of verified gameplay.
    response = client.post(
        base_url + "/gameplay/events",
        json={
            "event_id": str(uuid4()),
            "session_id": session_id,
            "quest_id": "demo_traffic",
            "milestone_id": "library_intro",
            "event_type": "scan_confirmed",
            "occurred_at": datetime.now(timezone.utc).isoformat(),
            "payload": {"label": "laptop", "confidence": DEMO_CONFIDENCE},
        },
        timeout=EVENT_TIMEOUT,
    )
    response.raise_for_status()


def run(base_url, seconds, fixture):
    deadline = time.monotonic() + seconds
    next_event = time.monotonic()
    session_id = str(uuid4())
    counts = {"predictions": 0, "events": 0, "errors": 0}
    with requests.Session() as client:
        while time.monotonic() < deadline:
            try:
                with fixture.open("rb") as image:
                    response = client.post(
                        base_url + "/predict",
                        files={"image": image},
                        data={"target_class": "laptop"},
                        timeout=PREDICT_TIMEOUT,
                    )
                response.raise_for_status()
                counts["predictions"] += 1
                now = time.monotonic()
                if now >= next_event and now < deadline:
                    next_event = now + EVENT_INTERVAL_SECONDS
                    _post_demo_event(client, base_url, session_id)
                    counts["events"] += 1
                    next_event = time.monotonic() + EVENT_INTERVAL_SECONDS
            except requests.RequestException:
                counts["errors"] += 1
            time.sleep(min(LOOP_SLEEP_CAP_SECONDS, max(0, deadline - time.monotonic())))
    return counts


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--base-url", default=DEFAULT_BASE_URL)
    parser.add_argument("--seconds", type=int, default=MAX_DEMO_SECONDS)
    parser.add_argument(
        "--fixture",
        type=Path,
        default=Path(__file__).resolve().parents[1] / "tests/fixtures/laptop-stocksnap.jpg",
    )
    args = parser.parse_args()
    if not 1 <= args.seconds <= MAX_DEMO_SECONDS or not args.fixture.is_file():
        parser.error("Use an existing fixture and duration 1..60 seconds")
    result = run(args.base_url.rstrip("/"), args.seconds, args.fixture)
    print(json.dumps(result))
    raise SystemExit(1 if result["errors"] else 0)
