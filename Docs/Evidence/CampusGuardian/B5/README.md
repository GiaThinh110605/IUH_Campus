# B5 â€” Unity backend client evidence

Date: 2026-11-03 (Asia/Bangkok). Unity6000.5.8f1 WindowsEditor. Base f5289d84569b2cfc9c02380b2e67c606a2ba93c1, branch minhnhat/unity-backend-client.
Commit message: feat(unity): add Guardian backend client, offline event outbox and smoke test (B5).
Exact commit: git log -1 --format=%H -- Docs/Evidence/CampusGuardian/B5/README.md (avoids a self-referential hash).

## Verified

- Before code creation: active IUH_SharedCampus_Concept scene isDirty=false, Edit Mode, rootCount13. No scene save or Play Mode command was executed.
- editmode-results.json: MCP Test Runner, assembly IUHCampus.Guardian.Backend.Tests;20passed,0failed/0skipped. Includes injected HTTP201/200/400/409/5xx/timeout,408/429; exact schemas, stable retry JSON/UUID, restart recovery, FIFO, single-flight, backoff doubling/cap, rejected-once, maxattempts and cancellation; corruption backup; dimensions and real JPG encoding.
- smoke.json: menu explicitly executed through MCP; real UnityWebRequest to B4 nginx at127.0.0.1:5001. Health200/DB/model true; predict200/TargetFound=true/laptop detection; same event POST201Recorded then200Duplicate, Core0. Scan boundary confidence0.70 accepted by real backend. Other3event schemas are generated for validation only, not posted; smoke is manual telemetry, not integrated gameplay. Two successful smoke runs each posted one distinct scan (one earlier0.9, final0.7); backend Adafruit totals can therefore increase by2.
- backend-schema-check.json: Python Server/app/event_validation.validate_event accepted the four JSON event types exported by Unity, without changing Server or creating a Flask app/database.
- scene-final.json: same active scene, loaded, rootCount13,isDirty=false. No scene, ProjectSettings, Packages or other Guardian/Match3 code was edited by these tools.
- console-backend.json: final snapshot0Backend errors/warnings. Expected LogAssert warnings from rejected/corruption tests are recorded in test outputs; they are intentional behavior, not compile warnings. Existing unrelated deprecated API/Input Manager warnings are outside scope and were not fixed.
- files.txt: created source/meta/docs scope; secret-scan.json: exact private credential check of selected Git index only, values never printed.

## Self-fix rounds used: 3/3

1. Added Serializable result DTO metadata for4serialization analyzer warnings; nullable BestConfidence stays a property because JsonUtility cannot serialize Nullable<float>. Added response guards and tests for temporary408/429 and single-flight/backoff behavior.
2. Real Edit Mode smoke stalled after server completed health200: NextFrameAsync required runtime player loop. Replaced wait with AsyncOperation.completed bridged through TaskCompletionSource, while public API remains Awaitable and cancellation aborts request on captured Unity context. Smoke then completed without Play Mode.
3. Regression test showed JsonUtility widened0.70f to0.69999998807907104, below backend minimum. Scan JSON now formats finite confidence with round-trip invariant culture; test and real API at0.70both pass.

TDD red evidence: tdd-red.json shows missing runtime type before implementation; threshold-red.json shows the precision failure before fix. Initial test discovery0tests during asmdef import was not counted as passing; domain reload interruptions were retried after ready.

## Remaining integration / limits

Thinh must add service/config and hook UI/gameplay later; B5 added no scene objects. Android camera permissions/HTTP policy, webcam rotation/mirroring, real device networking and forced process termination are not tested in this turn. Fake restart tests recreate an outbox instance with the same disk file. File.Replace atomic persistence is exercised on this Windows Editor; Android filesystem support must be verified on device. One main-thread outbox owner per path is required; capped FIFO retries remain durable and block following events until RetryFailed. Local Synced means accepted by API, not necessarily synced to Adafruit cloud. Service lifecycle integration was not run in Play Mode under the scope restriction.

Existing local tracked changes retained outside commit: scene/material, generated csproj/slnx and Logs (some log deletions pre-existed). Imported packs/backups/other untracked files were not staged. No push.


## Authorized metadata cleanup and final acceptance

metadata-cleanup.json records all11previously deleted files and the generator root cause. A generator had erroneously appended .meta to existing .meta files; it is no longer run. First authorized cleanup stopped when9normal metadata files went missing after refresh. On the final authorized round those9were already present before any mutation; no further metadata deletion/generation was needed. Entire physical project scan found no *.meta.meta*. After AssetDatabase.Refresh, metadata-check.json verifies18assets/folders and18metas, no orphans, valid GUIDs and no Backend GUID collision across4291logical asset metas (Assets/Packages). source-sha256.json verifies no C# changes in the final round.

Final MCP rerun:20/20passed,0skipped; console-errors.json shows0errors, console-backend.json shows0Backend errors/warnings; scene-final.json shows isDirty=false. A transient MCP session reconnect during Test Runner domain reload was retried read-only. No scene/save/Play commands or code/ProjectSettings/Packages changes in the metadata cleanup. Only B5 files are selected for commit, no push. secret-scan.json records exact private key/password scan without revealing values.
