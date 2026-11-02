# Backend W9 B1 evidence

Date: 2026-11-02 (Asia/Bangkok). OS: Windows 11 build 26200, Python 3.11.9.
Base commit: c034442c4427a7c02f21cac9b56ee35f3873a81c.
Implementation commit: the commit containing this directory; resolve with `git log -1 --format=%H -- Server/app/__init__.py`.

- `pytest.txt`: final 14/14 passed, exit 0.
- `pytest-before-fix.txt`: one earlier failure from a 10 ms sleep against Windows clock granularity. Fixed test by controlling monotonic time; no production uptime workaround.
- `endpoint-test-client.txt`: real Flask test_client output, not Docker/curl output.
- `compose-config.txt`: Compose configuration valid with missing optional .env, exit 0.
- `docker-version.txt`: Docker client available; Linux engine pipe missing, exit 1. Build/up/healthy/container curl/ps/logs/down skipped; no container started by B1.
- `pip-check.txt`: exit 0. `pip-freeze.txt`: installed environment versions.
- `flask-routes.txt`: local Flask CLI loads the app and lists the routes.
- `source-sha256.json`: tested Python source fingerprints.

The Windows pyreadline3 dependency emits an OSError during interpreter teardown; it is outside backend code and does not change pytest exit 0. Initial sandbox pip DNS failed; authorized network install succeeded.

Test cases: health real unloaded state/elapsed seconds; metrics duration/status count/model gauge; missing image 400; invalid image 400; valid image without model 503; config env; unconfigured model; session created once (mock, not proof of real inference); invalid model remains unloaded; warning once per start; both legacy endpoints return 404 (two cases); aggregation across two processes; stale metrics cleanup. Gunicorn itself and amd64/arm64 images remain untested until Docker engine is running. No model/inference benchmark or Adafruit IO integration is claimed.

Self-fix rounds used: 1 of 3.
