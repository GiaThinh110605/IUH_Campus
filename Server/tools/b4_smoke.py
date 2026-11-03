"""Opt-in B4 live acceptance. Run after all five compose services are healthy."""
from datetime import datetime, timezone
import json
from pathlib import Path
from uuid import uuid4

import requests


def main():
    root = Path(__file__).resolve().parents[2]
    evidence = root / "Docs/Evidence/CampusGuardian/B4"
    evidence.mkdir(parents=True, exist_ok=True)
    result = {}
    def get(name, url):
        response = requests.get(url, timeout=15)
        response.raise_for_status()
        result[name] = response.json()
        return result[name]
    health = get("health", "http://localhost:5001/health")
    assert health["model_loaded"] and health["db_ok"]
    with (root / "Server/tests/fixtures/laptop-stocksnap.jpg").open("rb") as image:
        response = requests.post("http://localhost:5001/predict", files={"image": image},
                                 data={"target_class": "laptop"}, timeout=30)
    assert response.status_code == 200
    result["predict"] = response.json()
    event = {"event_id": str(uuid4()), "session_id": str(uuid4()), "quest_id": "B4_smoke",
             "milestone_id": "library_intro", "event_type": "scan_confirmed",
             "occurred_at": datetime.now(timezone.utc).isoformat(),
             "payload": {"label": "laptop", "confidence": 0.9}}
    response = requests.post("http://localhost:5001/gameplay/events", json=event, timeout=15)
    assert response.status_code == 201
    result["event_request"] = event
    result["event_response"] = response.json()
    response = requests.get("http://localhost:5001/metrics", timeout=10)
    assert response.status_code in (403, 404)
    result["external_metrics_status"] = response.status_code
    targets = get("targets", "http://127.0.0.1:9090/api/v1/targets")
    assert any(t["health"] == "up" and t["labels"].get("job") == "guardian-api"
               for t in targets["data"]["activeTargets"])
    for metric in ("model_loaded", "cloud_sync_total", "outbox_pending"):
        response = requests.get("http://127.0.0.1:9090/api/v1/query",
                                params={"query": metric}, timeout=10)
        response.raise_for_status()
        result[metric] = response.json()
        assert result[metric]["data"]["result"]
    get("grafana_health", "http://127.0.0.1:3000/api/health")
    dashboards = get("grafana_search", "http://127.0.0.1:3000/api/search")
    assert any(d["uid"] == "guardian-api" for d in dashboards)
    sources = get("grafana_datasources", "http://127.0.0.1:3000/api/datasources")
    # Persist only public datasource metadata, never secureJsonData or credentials.
    result["grafana_datasources"] = [{k: d.get(k) for k in ("name", "type", "uid", "url")}
                                     for d in sources]
    assert any(d["uid"] == "guardian-prometheus" for d in sources)
    (evidence / "smoke.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
    print("B4 live acceptance recorded: smoke.json")


if __name__ == "__main__":
    main()
