# B6 scan session acceptance

Date: 2026-11-04 (Asia/Bangkok). OS: Windows-10-10.0.26200-SP0. Docker Desktop Linux amd64, Gunicorn2worker. Unity closed; no Unity files modified by this work.
Branch minhnhat/backend-w9-b6; base B2.7 100b9e75d779f3c3dc851b9164f7a0fc172e1ef8. B6 commit message: feat(backend): scan sessions with hidden infested classes and AR monster tokens (B6). Exact hash: git log -1 --format=%H -- Docs/Evidence/CampusGuardian/B6/README.md (no self-referential hash).

## B2.7 completed first

b27-reference.json records the seven-document B2.7 commit/push (no force) and old/new latency lines. model-decision.md is under B2 evidence. New operational target: YOLOv8n640 API total p50≤150ms/p95≤220ms on Ryzen7 6800H/Docker/Gunicorn2worker, confidence≥0.70. Conservative envelope of actual2worker data (worst p50=129.346,p95=180.630); no new latency benchmark claimed. Original assessment rubric and model checksum unchanged. B6 is not pushed.

## Tests and real Docker

- pytest.txt:116passed, including94old tests and22B6cases. Fake detector/secure sampler in unit tests only; no live key/network. Covers privacy/hints,404/410, three results, best-label threshold, unique token/cap, reward-once/reserved-key guard, wrong session, restart, concurrent reveal/reward, migration, rollback and config validation. Earlier runs retained an external Windows pyreadline3 teardown exception; final pytest exit0.
- docker-build.txt/docker.txt: final image iuh-guardian-api:b6, API/nginx/Prometheus/Grafana healthy, separate sync running; existing data/monitoring volumes preserved.
- setup-feeds.json: five app feeds ensured; new guardian-monsters-revealed created. Credentials masked****.
- live-scan.json/live-summary.json: real stock laptop fixture, up to12public random attempts allowed; actual3separate simulated gameUUIDs needed beforeinfested. No hidden classes read. Each create response is public hint/count/expiry only. Firstinfested yields one token and one internalmonster_revealed; manual combat event quest_id=B6_smoke recorded201/Core10; identical event200Duplicate; newevent same token400. Cloud reachedsynced. This is manually generated telemetry, not verified gameplay or AR integration.
- feed-last.json: actual Adafruit guardian-monsters-revealed value1. Aggregate/metrics also report one unique reveal; do not conflate old lost-found-count/scans with new monsters metric.
- restart-check.json: restarting API+sync preserves scan/reveal/token; repeatfixture returns same token and originalcombat event200Duplicate. Aggregate-after-restart.json has totals only, no private class records.
- final-container-check.json: finalcode after guardfix returns200Duplicate for originalevent and400 for ar_monster without token. No extra reward/reveal generated during finalcheck.
- monitoring.json: Prometheus guardian-api targetup; Grafana11panels including2new panels (scanresult ratio; cumulative monsters bylabel), bothPromQL accepted. /metrics throughnginx404. metrics.txt is direct internal APIexposition; sessionUUIDs/private class arrays absent.
- privacy-check.json/secret-scan.json/source-sha256.json: public contract/metric privacy, exact private credentials absent from stagedfiles, source hashes. No DB,.env,venv,alternate model files or Unitylocal changes staged.

## Storage and reward rules

scan_sessions/scan_reveals/scan_stats plus additive aggregates.monsters_revealed migration. Secret1..2class selection stays server-side, TTL15min bydefault; unique(scan,label)/token and BEGINIMMEDIATE serialize2workers. Reveal/internalevent/aggregate/outbox and combatclaim/event/reward each commit atomically. Newfeed queued only onnewreveal, coalesced/retried via existing≤20points/minworker. Metrics are collected from durable SQLite, not sync process-local counters. No session/token metric labels; HTTPpaths normalized to route template.

Token reward uses temporaryconfig ar_monster=10, oneperrevealedtoken, samegameUUID; duplicateevent replays originalaward. ar_monster withouttoken rejected; previouslegacy milestones still work unchanged. Tokenreward remains claimable after scanexpiry; TTL stopsnewscans/reveals. monsters_remaining means unrevealedslots, not remainingunwonbattles. Rewardedtokenwins increment the existing quests_completed tally as completed encounters; newmonsters_revealed is a distinct count.

Self-fix rounds:2/3used: (1) normalize exporterpath to remove per-scanUUID labels; (2) requiretoken for the newlyreservedar_monster key, preventing an extra legacy-pathreward. Initial404redtest before implementation was intentional TDD.

## Remaining for Unity/team

B5client needs StartScanSessionAsync/GetScanSessionAsync, scanDTO/monster_token, hintUI, stablegameUUID, expired/missing-session handling, duplicate-token UIguard and combat payload integration. ARplane/monster and realcamera/device testing remain separate; no Assets/scene/ProjectSettings/Packages changes.

One-to-twomonsters and reward-once are bounded per scan session/token, not per account forever. Creating another scan session is another round. API currently trusts clientgameUUID and reportedcombatwin; account authentication, proof-of-win and limits on starting newrounds need team decisions before public deployment. No globalanti-cheat claim. Keep640confidence0.70; real user photoaccuracy still pending perB2.7. Take a Grafana screenshot manually after traffic if needed for the final report.

Docker build log trailing whitespace normalized for git diff --check; message text/statuses preserved. No runtime/source change.
