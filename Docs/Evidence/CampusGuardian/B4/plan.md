# B4 implementation plan
Scope: Server/, backend README, B4 evidence; preserve Unity local changes.
1. Test ProxyFix trusts exactly one proxy; preserve internal metrics.
2. Add nginx (public 5001, block metrics), Prometheus (loopback 9090), Grafana (loopback 3000, required password), persistent monitoring volumes.
3. Use existing SQLite durable collector for cloud metrics; inspect actual metric exposition before dashboard queries.
4. Provision datasource/dashboard and bounded demo traffic.
5. Run pytest and Docker smoke/targets/dashboard verification when engine and password available. Never claim unrun acceptance.
6. Scan selected files against local secrets and commit only B4 files; no push.
