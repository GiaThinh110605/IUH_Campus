
CREATE TABLE IF NOT EXISTS events(
 event_id TEXT PRIMARY KEY, session_id TEXT NOT NULL, quest_id TEXT NOT NULL, milestone_id TEXT NOT NULL,
 event_type TEXT NOT NULL, occurred_at TEXT NOT NULL, payload_json TEXT NOT NULL, payload_hash TEXT NOT NULL,
 core_awarded INTEGER NOT NULL, cloud_enabled INTEGER NOT NULL, created_at REAL NOT NULL);
CREATE TABLE IF NOT EXISTS milestone_rewards(
 session_id TEXT NOT NULL, milestone_id TEXT NOT NULL, event_id TEXT NOT NULL REFERENCES events(event_id),
 PRIMARY KEY(session_id,milestone_id));
CREATE TABLE IF NOT EXISTS aggregates(
 id INTEGER PRIMARY KEY CHECK(id=1), quests_completed INTEGER NOT NULL DEFAULT 0,
 campus_core_total INTEGER NOT NULL DEFAULT 0, upgrades_total INTEGER NOT NULL DEFAULT 0,
 scans_confirmed INTEGER NOT NULL DEFAULT 0);
INSERT OR IGNORE INTO aggregates(id) VALUES(1);
CREATE TABLE IF NOT EXISTS outbox(
 id INTEGER PRIMARY KEY AUTOINCREMENT, feed_key TEXT NOT NULL, value INTEGER NOT NULL,
 status TEXT NOT NULL CHECK(status IN ('pending','synced','failed')), attempts INTEGER NOT NULL DEFAULT 0,
 next_attempt_at REAL NOT NULL DEFAULT 0, last_error TEXT, claim_token TEXT, lease_until REAL,
 created_at REAL NOT NULL);
CREATE INDEX IF NOT EXISTS outbox_due ON outbox(status,next_attempt_at,feed_key);
CREATE TABLE IF NOT EXISTS event_outbox(
 event_id TEXT NOT NULL REFERENCES events(event_id), outbox_id INTEGER NOT NULL REFERENCES outbox(id),
 PRIMARY KEY(event_id,outbox_id));
CREATE TABLE IF NOT EXISTS sync_control(
 id INTEGER PRIMARY KEY CHECK(id=1), next_send_at REAL NOT NULL DEFAULT 0, claim_token TEXT, lease_until REAL);
INSERT OR IGNORE INTO sync_control(id) VALUES(1);
CREATE TABLE IF NOT EXISTS sync_stats(result TEXT PRIMARY KEY, count INTEGER NOT NULL DEFAULT 0);
