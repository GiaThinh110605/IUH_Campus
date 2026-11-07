from datetime import datetime, timezone
import hashlib
import json
import math
from pathlib import Path
import secrets
import time
from uuid import uuid4

MONSTER_FEED = "guardian-monsters-revealed"
SCAN_SCHEMA = """
CREATE TABLE IF NOT EXISTS scan_sessions(
 id TEXT PRIMARY KEY, game_session_id TEXT NOT NULL, infected_classes TEXT NOT NULL,
 hint TEXT NOT NULL, created_at REAL NOT NULL, expires_at REAL NOT NULL);
CREATE TABLE IF NOT EXISTS scan_reveals(
 scan_session_id TEXT NOT NULL REFERENCES scan_sessions(id), label TEXT NOT NULL,
 monster_token TEXT NOT NULL UNIQUE, revealed_at REAL NOT NULL,
 rewarded_event_id TEXT REFERENCES events(event_id), PRIMARY KEY(scan_session_id,label));
CREATE TABLE IF NOT EXISTS scan_rejections(reason TEXT PRIMARY KEY, count INTEGER NOT NULL DEFAULT 0);
CREATE INDEX IF NOT EXISTS scan_game_session ON scan_sessions(game_session_id);
CREATE TABLE IF NOT EXISTS scan_stats(result TEXT PRIMARY KEY, count INTEGER NOT NULL DEFAULT 0)
"""


def migrate_scan_tables(db):
    db.execute("BEGIN IMMEDIATE")
    try:
        for statement in SCAN_SCHEMA.split(";"):
            if statement.strip():
                db.execute(statement)
        columns = {row["name"] for row in db.execute("PRAGMA table_info(aggregates)")}
        if "monsters_revealed" not in columns:
            db.execute("ALTER TABLE aggregates ADD COLUMN monsters_revealed INTEGER NOT NULL DEFAULT 0")
        db.commit()
    except Exception:
        db.rollback()
        raise


class ScanSessionError(ValueError):
    def __init__(self, error, status, scan_session_id=None, retry_after=None):
        super().__init__(error)
        self.error, self.status = error, status
        self.scan_session_id, self.retry_after = scan_session_id, retry_after


def token_reward(db, token, game_session_id):
    return db.execute("SELECT r.monster_token FROM scan_reveals r JOIN scan_sessions s "
                      "ON s.id=r.scan_session_id WHERE r.monster_token=? AND s.game_session_id=? "
                      "AND r.rewarded_event_id IS NULL", (token, game_session_id)).fetchone()


class ScanStore:
    def __init__(self, store, config, clock=None):
        self.store = store
        self.clock = clock or time.time
        self.minimum, self.maximum = config["SCAN_INFECTED_MIN"], config["SCAN_INFECTED_MAX"]
        self.max_active = config["SCAN_MAX_ACTIVE_PER_GAME_SESSION"]
        self.cooldown = config["SCAN_COOLDOWN_SECONDS"]
        self.max_sessions = config["SCAN_MAX_SESSIONS_PER_GAME_SESSION"]
        if self.max_active < 1 or self.max_sessions < 1 or not math.isfinite(self.cooldown) or self.cooldown < 0:
            raise ValueError("Invalid scan configuration")
        self.ttl = config["SCAN_SESSION_TTL_MINUTES"] * 60
        pool = config["SCAN_INFECTED_POOL"]
        if not isinstance(pool, str):
            raise ValueError("Invalid SCAN_INFECTED_POOL: expected comma-separated class names")
        self.classes = tuple(dict.fromkeys(label.strip() for label in pool.split(",") if label.strip()))
        if not self.classes or any(label not in config["ALLOWED_CLASSES"] for label in self.classes):
            raise ValueError("Invalid SCAN_INFECTED_POOL: must be non-empty and contain only ALLOWED_CLASSES")
        self.hints = json.loads(Path(config["SCAN_HINTS_PATH"]).read_text(encoding="utf-8"))
        if (not 1 <= self.minimum <= self.maximum <= 2 or
                not math.isfinite(self.ttl) or self.ttl <= 0 or
                any(not isinstance(self.hints.get(label), str) or not self.hints[label] for label in self.classes)):
            raise ValueError("Invalid scan configuration")
        self.minimum = min(self.minimum, len(self.classes))
        self.maximum = min(self.maximum, len(self.classes))

    def create(self, game_session_id):
        random = secrets.SystemRandom()
        infected = random.sample(self.classes, random.randint(self.minimum, self.maximum))
        hint = "Tín hiệu nhiễu từ " + " và ".join(dict.fromkeys(self.hints[label] for label in infected)) + "."
        now, scan_id = self.clock(), str(uuid4())
        rejection = None
        with self.store.transaction() as db:
            rows = db.execute("SELECT * FROM scan_sessions WHERE game_session_id=? ORDER BY created_at DESC", (game_session_id,)).fetchall()
            active = [row for row in rows if row["expires_at"] > now and
                      self._revealed(db, row["id"]) < len(json.loads(row["infected_classes"]))]
            if len(active) >= self.max_active:
                rejection = ScanSessionError("scan_session_active", 409, scan_session_id=active[0]["id"])
            elif len(rows) >= self.max_sessions:
                rejection = ScanSessionError("scan_limit_reached", 429)
            elif rows and now < rows[0]["created_at"] + self.cooldown:
                rejection = ScanSessionError("scan_cooldown", 429, retry_after=math.ceil(rows[0]["created_at"] + self.cooldown - now))
            if rejection:
                db.execute("INSERT INTO scan_rejections VALUES(?,1) ON CONFLICT(reason) DO UPDATE SET count=count+1", (rejection.error,))
            else:
                db.execute("INSERT INTO scan_sessions VALUES(?,?,?,?,?,?)",
                       (scan_id, game_session_id, json.dumps(infected), hint, now, now + self.ttl))
        if rejection:
            raise rejection
        return {"scan_session_id": scan_id, "hint": hint, "max_monsters": len(infected),
                "expires_at": self._iso(now + self.ttl)}

    def _session(self, db, scan_id):
        row = db.execute("SELECT * FROM scan_sessions WHERE id=?", (scan_id,)).fetchone()
        if row is None:
            raise ScanSessionError("scan_session_not_found", 404)
        if row["expires_at"] <= self.clock():
            raise ScanSessionError("scan_session_expired", 410)
        return row

    @staticmethod
    def _iso(value):
        return datetime.fromtimestamp(value, timezone.utc).isoformat()

    @staticmethod
    def _revealed(db, scan_id):
        return db.execute("SELECT count(*) FROM scan_reveals WHERE scan_session_id=?", (scan_id,)).fetchone()[0]

    def status(self, scan_id):
        with self.store.connection() as db:
            row = self._session(db, scan_id)
            revealed = self._revealed(db, scan_id)
            return {"scan_session_id": row["id"], "hint": row["hint"],
                    "monsters_remaining": len(json.loads(row["infected_classes"])) - revealed,
                    "expires_at": self._iso(row["expires_at"]), "revealed_count": revealed}

    def reveal(self, scan_id, label, cloud_enabled):
        with self.store.transaction() as db:
            row = self._session(db, scan_id)
            infected = json.loads(row["infected_classes"])
            result, token = ("none" if label is None else "clean"), None
            if label in infected and label in self.classes:
                result = "infested"
                existing = db.execute("SELECT monster_token FROM scan_reveals WHERE scan_session_id=? AND label=?",
                                      (scan_id, label)).fetchone()
                token = existing["monster_token"] if existing else self._new_reveal(db, row, label, cloud_enabled)
            db.execute("INSERT INTO scan_stats VALUES(?,1) ON CONFLICT(result) DO UPDATE SET count=count+1", (result,))
            return {"label": label, "result": result, "monster_token": token,
                    "monsters_remaining": len(infected) - self._revealed(db, scan_id)}

    def _new_reveal(self, db, row, label, cloud_enabled):
        token, event_id = secrets.token_urlsafe(32), str(uuid4())
        now = self.clock()
        event = {"event_id": event_id, "session_id": row["game_session_id"], "quest_id": "ar_scan",
                 "milestone_id": "ar_monster", "event_type": "monster_revealed", "occurred_at": self._iso(now),
                 "payload": {"scan_session_id": row["id"], "label": label, "monster_token": token}}
        canonical = json.dumps(event, sort_keys=True, separators=(",", ":"), allow_nan=False)
        self.store._insert_event(db, event, canonical, hashlib.sha256(canonical.encode()).hexdigest(), 0, cloud_enabled)
        db.execute("INSERT INTO scan_reveals VALUES(?,?,?,?,NULL)", (row["id"], label, token, now))
        db.execute("UPDATE aggregates SET monsters_revealed=monsters_revealed+1 WHERE id=1")
        if cloud_enabled:
            self.store._queue_snapshot(db, event_id, {MONSTER_FEED: "monsters_revealed"})
        return token

    def metrics(self):
        return scan_metrics(self.store)


def scan_metrics(store):
    with store.connection() as db:
        return {"rejections": {row["reason"]: row["count"] for row in db.execute("SELECT * FROM scan_rejections")},
                "sessions": db.execute("SELECT count(*) FROM scan_sessions").fetchone()[0],
                "results": {row["result"]: row["count"] for row in db.execute("SELECT * FROM scan_stats")},
                "reveals": {row["label"]: row["count"] for row in db.execute(
                    "SELECT label,count(*) AS count FROM scan_reveals GROUP BY label")}}
