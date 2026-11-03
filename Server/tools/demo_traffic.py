"""Explicit opt-in demo: real predictions and scan telemetry, no Core rewards."""
import argparse
from datetime import datetime, timezone
import json
from pathlib import Path
import time
from uuid import uuid4

import requests


def run(base_url, seconds, fixture):
    deadline = time.monotonic() + seconds
    next_event = time.monotonic()
    session_id = str(uuid4())
    counts = {"predictions": 0, "events": 0, "errors": 0}
    with requests.Session() as client:
        while time.monotonic() < deadline:
            try:
                with fixture.open("rb") as image:
                    response = client.post(base_url + "/predict", files={"image": image},
                                           data={"target_class": "laptop"}, timeout=15)
                response.raise_for_status()
                counts["predictions"] += 1
                now = time.monotonic()
                if now >= next_event and now < deadline:
                    next_event = now + 3.1
                    # Manual demo telemetry, not a claim of verified gameplay.
                    response = client.post(base_url + "/gameplay/events", json={
                        "event_id": str(uuid4()), "session_id": session_id,
                        "quest_id": "demo_traffic", "milestone_id": "library_intro",
                        "event_type": "scan_confirmed",
                        "occurred_at": datetime.now(timezone.utc).isoformat(),
                        "payload": {"label": "laptop", "confidence": 0.9}}, timeout=10)
                    response.raise_for_status()
                    counts["events"] += 1
                    next_event = time.monotonic() + 3.1
            except requests.RequestException:
                counts["errors"] += 1
            time.sleep(min(1, max(0, deadline - time.monotonic())))
    return counts


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--base-url", default="http://localhost:5001")
    parser.add_argument("--seconds", type=int, default=60)
    parser.add_argument("--fixture", type=Path, default=Path(__file__).resolve().parents[1] /
                        "tests/fixtures/laptop-stocksnap.jpg")
    args = parser.parse_args()
    if not 1 <= args.seconds <= 60 or not args.fixture.is_file():
        parser.error("Use an existing fixture and duration 1..60 seconds")
    result = run(args.base_url.rstrip("/"), args.seconds, args.fixture)
    print(json.dumps(result))
    raise SystemExit(1 if result["errors"] else 0)
