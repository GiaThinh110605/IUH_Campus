# B6 implementation and acceptance plan
Base: B2.7 commit100b9e7 (pushed onB3); Unity closed. Scope Server/, backend README, B6 evidence only.
1. Red test POST/scan/sessions404 before implementation; independent fake detector + SQLite tests.
2. ScanStore secure random1..2classes, server-only storage, UTC TTL, hints; transactional unique reveal/token/event/aggregate/outbox. No hidden-class logging/export.
3. Optional combat-won token bound to game session, once-only reward; keep old milestone and predict contracts.
4. Durable SQLite metrics, one new feed, two Grafana panels; retain account send quota.
5. Full pytest, Docker build/health; public-only live smoke, feed readback, restart and duplicate. Exact secrets scan before selective commit, no B6 push.
