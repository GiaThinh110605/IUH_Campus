from datetime import datetime, timedelta, timezone
import math
import re
from uuid import UUID

EVENT_TYPES = {"scan_confirmed", "combat_won", "combat_lost", "upgrade_purchased"}
REQUIRED_FIELDS = {"event_id", "session_id", "quest_id", "milestone_id", "event_type", "occurred_at"}
IDENTIFIER = re.compile(r"[A-Za-z0-9_-]{1,64}\Z")


class InvalidEvent(ValueError):
    def __init__(self, field, error="invalid_value"):
        super().__init__(error)
        self.field, self.error = field, error


def whitelist(data, allowed, required, prefix=""):
    if not isinstance(data, dict):
        raise InvalidEvent(prefix.rstrip(".") or "body", "invalid_object")
    unknown = sorted(set(data) - allowed)
    missing = sorted(required - set(data))
    if unknown:
        raise InvalidEvent(prefix + unknown[0], "unknown_field")
    if missing:
        raise InvalidEvent(prefix + missing[0], "required_field")


def normalize_uuid(value, field):
    if not isinstance(value, str) or len(value) > 64:
        raise InvalidEvent(field)
    try:
        return str(UUID(value))
    except ValueError:
        raise InvalidEvent(field) from None


def validate_payload(kind, payload, allowed_labels, minimum):
    fields = {"scan_confirmed": {"label", "confidence"}, "upgrade_purchased": {"branch", "level"}}.get(kind, set())
    allowed = fields | ({"monster_token"} if kind == "combat_won" else set())
    whitelist(payload, allowed, fields, "payload.")
    if "monster_token" in payload:
        token = payload["monster_token"]
        if not isinstance(token, str) or not IDENTIFIER.fullmatch(token):
            raise InvalidEvent("payload.monster_token")
    if kind == "scan_confirmed":
        confidence = payload["confidence"]
        if payload["label"] not in allowed_labels:
            raise InvalidEvent("payload.label")
        if (isinstance(confidence, bool) or not isinstance(confidence, (int, float)) or
                not math.isfinite(confidence) or not minimum <= confidence <= 1):
            raise InvalidEvent("payload.confidence")
        return {"label": payload["label"], "confidence": float(confidence)}
    if kind == "upgrade_purchased":
        if payload["branch"] not in ("attack", "defense", "vitality"):
            raise InvalidEvent("payload.branch")
        if type(payload["level"]) is not int or payload["level"] not in (1, 2):
            raise InvalidEvent("payload.level")
    return payload


def validate_event(body, allowed_labels, minimum, now=None):
    whitelist(body, REQUIRED_FIELDS | {"payload"}, REQUIRED_FIELDS)
    result = dict(body)
    for field in ("event_id", "session_id"):
        result[field] = normalize_uuid(body[field], field)
    for field in ("quest_id", "milestone_id", "event_type"):
        if not isinstance(body[field], str) or not IDENTIFIER.fullmatch(body[field]):
            raise InvalidEvent(field)
    if body["event_type"] not in EVENT_TYPES:
        raise InvalidEvent("event_type")
    try:
        if not isinstance(body["occurred_at"], str):
            raise ValueError()
        occurred = datetime.fromisoformat(body["occurred_at"].replace("Z", "+00:00"))
        if occurred.tzinfo is None or occurred.utcoffset() is None:
            raise ValueError()
        occurred = occurred.astimezone(timezone.utc)
        if occurred > (now or datetime.now(timezone.utc)) + timedelta(minutes=5):
            raise ValueError()
    except (ValueError, OverflowError):
        raise InvalidEvent("occurred_at") from None
    result["occurred_at"] = occurred.isoformat()
    result["payload"] = validate_payload(body["event_type"], body.get("payload", {}), allowed_labels, minimum)
    return result
