# B7 verification

- Base: main 2387c78. Unity scene IUH_SharedCampus_Concept: isDirty=false before and after.
- EditMode assembly IUHCampus.Guardian.Backend.Tests: 40 passed, 0 failed, 0 skipped (20 existing B5 + 20 B7).
- B7 covers POST outcomes, GET 200/404/410, frame outcomes and confidence/token mapping, Retry-After seconds, fake service, token payload, stabilizer and throttle.
- No Backend compile errors/warnings after full import. One later MCP WebSocket warning is outside Backend.
- First Docker smoke used stale b6 image: laptop returned Clean; second POST incorrectly returned Created. Container configuration inspection confirmed all three B6.1 keys absent. See smoke-stale-image.json.
- Smoke is opt-in through IUH Campus/Guardian Backend/B7 Scan Smoke; never runs on load.
- ScanFrameOutcome enum order: Infested=0, Clean=1, NoObject=2, SessionNotFound=3, SessionExpired=4, NetworkError=5, ServerError=6.
- ScanSessionOutcome enum order: Created=0, ActiveExists=1, Cooldown=2, LimitReached=3, NetworkError=4, ServerError=5, Found=6, SessionNotFound=7, SessionExpired=8.

- Final smoke after rebuilding current main: passed=true; Created -> laptop Infested (confidence 0.7349, inference 86.53ms, remaining 1) -> ActiveExists with same id. Scene clean before/after. See smoke.json.
