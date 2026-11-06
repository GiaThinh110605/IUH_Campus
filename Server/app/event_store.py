from contextlib import contextmanager
import hashlib
import json
from pathlib import Path
import sqlite3
import time
from uuid import uuid4

from .scan_store import migrate_scan_tables, token_reward

FEEDS = {"guardian-quests-completed": "quests_completed", "guardian-campus-core": "campus_core_total",
         "guardian-upgrades": "upgrades_total", "lost-found-count": "scans_confirmed"}
SEND_INTERVAL = 3.1
LEASE_SECONDS = 30
MAX_ATTEMPTS = 8
SCHEMA = """
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
"""


class EventStore:
    def __init__(self, path, rewards=None):
        self.path, self.rewards = str(path), rewards or {}
        Path(self.path).parent.mkdir(parents=True, exist_ok=True)
        with self.connection() as db:
            db.execute("PRAGMA journal_mode=WAL")
            db.executescript(SCHEMA)
            migrate_scan_tables(db)

    @contextmanager
    def connection(self):
        db = sqlite3.connect(self.path, timeout=10, isolation_level=None)
        db.row_factory = sqlite3.Row
        db.execute("PRAGMA busy_timeout=10000")
        db.execute("PRAGMA foreign_keys=ON")
        try:
            yield db
        finally:
            db.close()

    @contextmanager
    def transaction(self):
        with self.connection() as db:
            db.execute("BEGIN IMMEDIATE")
            try:
                yield db
                db.commit()
            except Exception:
                db.rollback()
                raise

    def record(self, event, cloud_enabled):
        canonical = json.dumps(event, sort_keys=True, separators=(",", ":"), allow_nan=False)
        digest = hashlib.sha256(canonical.encode()).hexdigest()
        with self.transaction() as db:
            existing = db.execute("SELECT * FROM events WHERE event_id=?", (event["event_id"],)).fetchone()
            if existing:
                if existing["payload_hash"] != digest:
                    return "conflict", None
                return "duplicate", self._event_result(db, existing)
            won = event["event_type"] == "combat_won"
            monster_token = event.get("payload", {}).get("monster_token") if won else None
            if won and event["milestone_id"] == "ar_monster" and not monster_token:
                return "invalid_token", None
            if monster_token and not token_reward(db, monster_token, event["session_id"]):
                return "invalid_token", None
            reward_key = "ar_monster" if monster_token else event["milestone_id"]
            if won and reward_key not in self.rewards:
                return "invalid_milestone", None
            prior = None if monster_token else db.execute("SELECT 1 FROM milestone_rewards WHERE session_id=? AND milestone_id=?",
                                                         (event["session_id"], event["milestone_id"])).fetchone()
            first_win = won and not prior
            core = self.rewards[reward_key] if first_win else 0
            self._insert_event(db, event, canonical, digest, core, cloud_enabled)
            if monster_token:
                db.execute("UPDATE scan_reveals SET rewarded_event_id=? WHERE monster_token=? AND rewarded_event_id IS NULL",
                           (event["event_id"], monster_token))
            if first_win and not monster_token:
                db.execute("INSERT INTO milestone_rewards VALUES(?,?,?)",
                           (event["session_id"], event["milestone_id"], event["event_id"]))
            db.execute("UPDATE aggregates SET quests_completed=quests_completed+?,campus_core_total=campus_core_total+?,"
                       "upgrades_total=upgrades_total+?,scans_confirmed=scans_confirmed+? WHERE id=1",
                       (int(first_win), core, int(event["event_type"] == "upgrade_purchased"),
                        int(event["event_type"] == "scan_confirmed")))
            if cloud_enabled:
                self._queue_snapshot(db, event["event_id"])
            return "recorded", {"event_id": event["event_id"], "core_awarded": core,
                                "cloud": "pending" if cloud_enabled else "disabled"}

    def _insert_event(self, db, event, canonical, digest, core, cloud_enabled):
        db.execute("INSERT INTO events VALUES(?,?,?,?,?,?,?,?,?,?,?)",
                   (event["event_id"], event["session_id"], event["quest_id"], event["milestone_id"],
                    event["event_type"], event["occurred_at"], canonical, digest, core, int(cloud_enabled), time.time()))

    def _queue_snapshot(self, db, event_id, feeds=None):
        aggregate = db.execute("SELECT * FROM aggregates WHERE id=1").fetchone()
        for feed, column in (FEEDS if feeds is None else feeds).items():
            pending = db.execute("SELECT id FROM outbox WHERE feed_key=? AND status='pending' AND claim_token IS NULL "
                                 "ORDER BY id DESC LIMIT 1", (feed,)).fetchone()
            if pending:
                outbox_id = pending["id"]
                db.execute("UPDATE outbox SET value=? WHERE id=?", (aggregate[column], outbox_id))
            else:
                outbox_id = db.execute("INSERT INTO outbox(feed_key,value,status,created_at) VALUES(?,?,'pending',?)",
                                       (feed, aggregate[column], time.time())).lastrowid
            db.execute("INSERT OR IGNORE INTO event_outbox VALUES(?,?)", (event_id, outbox_id))

    def _event_result(self, db, event):
        cloud = "disabled"
        if event["cloud_enabled"]:
            rows = db.execute("SELECT o.status FROM outbox o JOIN event_outbox l ON l.outbox_id=o.id WHERE l.event_id=?",
                              (event["event_id"],)).fetchall()
            states = [row["status"] for row in rows]
            cloud = "synced" if states and all(state == "synced" for state in states) else "pending"
            if "failed" in states:
                cloud = "failed"
        return {"event_id": event["event_id"], "core_awarded": event["core_awarded"], "cloud": cloud}

    def get(self, event_id):
        with self.connection() as db:
            event = db.execute("SELECT * FROM events WHERE event_id=?", (event_id,)).fetchone()
            return self._event_result(db, event) if event else None

    def summary(self):
        with self.connection() as db:
            result = dict(db.execute("SELECT * FROM aggregates WHERE id=1").fetchone())
            result.update(db_ok=True, outbox_pending=db.execute("SELECT count(*) FROM outbox WHERE status='pending'").fetchone()[0],
                          outbox_failed=db.execute("SELECT count(*) FROM outbox WHERE status='failed'").fetchone()[0])
            result["sync_stats"] = {row["result"]: row["count"] for row in db.execute("SELECT * FROM sync_stats")}
            return result

    def _coalesce(self, db, newest, now):
        rows = db.execute("SELECT * FROM outbox WHERE feed_key=? AND status='pending' AND "
                          "(claim_token IS NULL OR lease_until<=?)", (newest["feed_key"], now)).fetchall()
        attempts = max(row["attempts"] for row in rows)
        next_attempt = max(row["next_attempt_at"] for row in rows)
        for row in rows:
            if row["id"] == newest["id"]:
                continue
            db.execute("INSERT OR IGNORE INTO event_outbox SELECT event_id,? FROM event_outbox WHERE outbox_id=?",
                       (newest["id"], row["id"]))
            db.execute("DELETE FROM event_outbox WHERE outbox_id=?", (row["id"],))
            db.execute("DELETE FROM outbox WHERE id=?", (row["id"],))
        db.execute("UPDATE outbox SET attempts=?,next_attempt_at=? WHERE id=?", (attempts, next_attempt, newest["id"]))

    def claim(self, now=None):
        now = time.time() if now is None else now
        with self.transaction() as db:
            control = db.execute("SELECT * FROM sync_control WHERE id=1").fetchone()
            if control["next_send_at"] > now or (control["lease_until"] or 0) > now:
                return None
            row = db.execute("SELECT * FROM outbox WHERE id IN (SELECT max(id) FROM outbox WHERE status='pending' "
                             "AND (claim_token IS NULL OR lease_until<=?) GROUP BY feed_key) AND next_attempt_at<=? "
                             "ORDER BY next_attempt_at,id LIMIT 1", (now, now)).fetchone()
            if row is None:
                return None
            self._coalesce(db, row, now)
            merged = db.execute("SELECT attempts,next_attempt_at FROM outbox WHERE id=?", (row["id"],)).fetchone()
            attempts = merged["attempts"]
            if attempts >= MAX_ATTEMPTS:
                db.execute("UPDATE outbox SET status='failed',last_error='attempt_limit',claim_token=NULL,lease_until=NULL WHERE id=?",
                           (row["id"],))
                db.execute("INSERT INTO sync_stats VALUES('failed',1) ON CONFLICT(result) DO UPDATE SET count=count+1")
                return None
            if merged["next_attempt_at"] > now:
                return None
            token = str(uuid4())
            changed = db.execute("UPDATE outbox SET claim_token=?,lease_until=?,attempts=attempts+1 WHERE id=? "
                                 "AND status='pending' AND (claim_token IS NULL OR lease_until<=?)",
                                 (token, now + LEASE_SECONDS, row["id"], now)).rowcount
            if not changed:
                return None
            db.execute("UPDATE sync_control SET claim_token=?,lease_until=?,next_send_at=? WHERE id=1",
                       (token, now + LEASE_SECONDS, now + SEND_INTERVAL))
            return dict(db.execute("SELECT * FROM outbox WHERE id=?", (row["id"],)).fetchone())

    def finish(self, job, success, error=None, retry_after=None, now=None):
        now = time.time() if now is None else now
        with self.transaction() as db:
            row = db.execute("SELECT * FROM outbox WHERE id=? AND claim_token=?",
                             (job["id"], job["claim_token"])).fetchone()
            if row is None:
                return False
            terminal = not success and row["attempts"] >= MAX_ATTEMPTS
            status = "synced" if success else ("failed" if terminal else "pending")
            delay = max(min(300, 5 * 2 ** (row["attempts"] - 1)), retry_after or 0)
            db.execute("UPDATE outbox SET status=?,next_attempt_at=?,last_error=?,claim_token=NULL,lease_until=NULL WHERE id=?",
                       (status, now + delay if status == "pending" else 0, None if success else error, row["id"]))
            db.execute("UPDATE sync_control SET next_send_at=?,claim_token=NULL,lease_until=NULL WHERE id=1 AND claim_token=?",
                       (now + max(SEND_INTERVAL, retry_after or 0), job["claim_token"]))
            result = "synced" if success else ("failed" if terminal else ("rate_limited" if error == "http_429" else "retry"))
            db.execute("INSERT INTO sync_stats VALUES(?,1) ON CONFLICT(result) DO UPDATE SET count=count+1", (result,))
            return True
