# P1 — Guardian progress core

Base: origin/main `9a838cb`; branch minhnhat/progress-p1. Date: 2026-11-07 Asia/Bangkok. Unity6000.5.8f1, Windows, AMD Ryzen7 6800H. No scene changes or service auto-bootstrap.

- [editmode.json](editmode.json): MCP full EditMode run 6376072e125e49929c7c2e94ef72ba31, **64passed/0failed/0skipped** (9Progress +55existing). Final duration10.151636s.
- [console.json](console.json): read_console error snapshot, **0entries**, not cleared.
- First discovery attempt returned zero tests before full asset refresh; not counted as passing tests. Six initial tests then passed; final run includes three additional persistence/config/asset tests and backup-preservation correction.
- Tests use temporary save directories and temporary ScriptableObjects; shipped-asset test only reads real database/config assets. No real user save or scene modified by tests.
- Covered: add/remove/cap/nonnegative, global reward ids across wallet/inventory, failed grant retry, immutable inventory snapshot, unknown-id warning once, duplicate database rejection, save/reload/recovery, starter config overrides/one-time grant, persisted reward ledger, real five definitions/config/database.
- Not device/PlayMode tested: DontDestroyOnLoad integration, debounce/pause/quit under OS kill, atomic replace behavior on iOS. Source implements these lifecycle hooks; no claim of device verification.
- Core uses model state/config data; no references to Backend/Match3 assemblies. Prices/rewards/upgrade increases are editable prototype defaults, not playtest measurements. Save version1 only; migration before introducing a later schema.

API and examples: [Guardian_Progress.md](../../../Guardian_Progress.md).
