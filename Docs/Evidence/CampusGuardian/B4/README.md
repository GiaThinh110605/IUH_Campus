# B4 acceptance evidence

Date: 2026-11-03 (Asia/Bangkok). Platform: Windows-10-10.0.26200-SP0. Docker Desktop Linux engine.
Branch: minhnhat/backend-w9-b4. Base: aa599082ce445627b73cabdd6c6ae04ecc777b04.
Commit: feat(backend): add Nginx proxy, Prometheus and Grafana monitoring (W9 B4).
Resolve exact commit with git log -1 --format=%H -- Docs/Evidence/CampusGuardian/B4/README.md (avoids self-referential commit hash).

- pytest.txt: 94 passed; exit0. Existing external pyreadline3 teardown warning retained.
- build.txt: real compose build/start. docker.txt: four healthy services and separate sync running.
- smoke.json: real nginx health/model_loaded=true/db_ok=true, fixture prediction200, manual B4_smoke scan event201/Core0, external metrics404; Prometheus target guardian-api up and populated model_loaded/cloud_sync_total/outbox_pending; Grafana health/database ok, datasource and dashboard provisioned.
- metrics.txt: actual internal API exposition. Metrics remain inaccessible through public nginx.
- panel-queries.md / panel-query-results.json: all9panels /11PromQL queries accepted by real Prometheus.
- demo-traffic.json: real60second run, manual scan events quest_id=demo_traffic (not verified gameplay); no Core awarded. Demo telemetry modifies persistent scan/feed totals. Outbox enforces <=20Adafruit points/minute independently of event request frequency.
- secret-scan.json: exact private Adafruit key and Grafana password checked against selected staged contents, no values printed.
- plan.md: implementation checklist.

Cloud metric option(a): API custom collector reads shared SQLite sync_stats/outbox each scrape; no extra worker metrics endpoint required. This survived API/sync process separation in the live stack.

Resolved environmental blockers: user started Docker Desktop and stopped their unrelated ml-production-demo project holding9090. Prometheus B4 was recreated with its volume preserved because failed bind left an empty host mapping. No other project's containers or Unity files were edited by this work. Early smoke attempts recorded additional B4_smoke scan events before monitoring readiness; they granted0Core. Only final complete smoke output is smoke.json.

One initial generation error (missing dashboard directory) was corrected. No failed application tests after ProxyFix implementation; expected TDD red first. No model or inference threshold change.

Capture a screenshot manually at http://127.0.0.1:3000/d/guardian-api ; anonymous Viewer is enabled. Grafana login password is private in ignored Server/.env. Both monitoring host ports bind loopback. Stack remains running for inspection; volumes retained.
