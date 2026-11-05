# W9 B3 durable gameplay/cloud acceptance

Date:2026-11-02 Asia/Bangkok. Base:e473b5179804cf245c58da6a954bf8137404edd6. Local Windows11/Python3.11.9; Docker Linux amd64/Gunicorn2 workers plus one independent sync container. B3 implementation commit is the commit containing this evidence (`git log -1 --format=%H -- Server/app/event_store.py`). Credentials are ****; no secret values are stored here.

- `pytest.txt`: final92/92 passed; full suite with session/function credential isolation, temporary SQLite databases and denied live unit-test networking.
- `setup-feeds.txt`: real authenticated GET/POST created all4 feeds; fixed names/statuses only.
- `live-events.json`: manually generated sample scan + combat win + duplicate win; HTTP201/201/200, Core0/10/10. No camera/gameplay integration claim is made.
- `live-after-restart.json`: HTTP200 duplicate, same10 Core, cloud synced after restarting API+sync containers.
- `aggregate-check.json`: aggregates unchanged by restart/duplicate, all values match live feeds. Quests1, campus Core10, upgrades0, scans1. Four successful cloud sends; pending0/failed0.
- `docker.txt`: build, service status, safe setup output, metrics, restart, logs and down. Data bind volume is retained; no DB reset/deletion was performed.
- `database-check.json`, `schema.sql`: integrity/WAL and schema evidence.

Fake-cloud tests cover field/payload whitelist, UUID/time/body limits, reward-once per session/milestone, concurrent duplicate, restart, transaction rollback including outbox failure, conditional claim, global rate-gate persistence, exponential backoff/max8, expired lease/superseded links,429 Retry-After, disabled cloud, durable metrics and sanitized HTTP errors.

Self-fix rounds:3/3. First fixed the crash-recovery edge that could claim attempt9. Second separated SQLite test files from the Prometheus mmap test directory (both use .db suffix; production directories were already distinct). Third preserved the maximum retry deadline when coalescing a newer value with an earlier failed send; regression test added. External pyreadline3 emits an interpreter-teardown OSError in some Windows test runs; test exit state is recorded, no library patch applied.

Reward library_intro=10 is TEMPORARY pending team approval. Core total is lifetime awarded, not a spendable balance; upgrade pricing/debits/authorization, stable session identity/authentication and Unity integration remain separate work. Cloud point delivery is at least once across the network/local-ACK crash gap; repeated totals cannot grant another Core award in SQLite.

User action: capture Adafruit dashboard screenshots separately. Source/default ONNX model and minimum inference confidence are unchanged.

`final-restart-check.txt`: final image rebuilt after round3; replayed only an existing event ID, still duplicate/synced; feed totals unchanged, no new reward/event created.
