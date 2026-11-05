import json

from flask import Blueprint, current_app, jsonify, request
from werkzeug.exceptions import RequestEntityTooLarge

from ..event_validation import EVENT_TYPES, InvalidEvent, normalize_uuid, validate_event

gameplay = Blueprint("gameplay", __name__)
EVENT_BODY_LIMIT = 16 * 1024


def count_event(kind, result):
    current_app.extensions["gameplay_events_metric"].labels(type=kind, result=result).inc()


def read_event_body():
    request.max_content_length = EVENT_BODY_LIMIT
    if not request.is_json:
        raise InvalidEvent("body", "invalid_json")
    try:
        return json.loads(request.get_data())
    except RequestEntityTooLarge:
        raise InvalidEvent("body", "body_too_large") from None
    except (ValueError, UnicodeDecodeError):
        raise InvalidEvent("body", "invalid_json") from None


@gameplay.post("/gameplay/events")
def record_event():
    kind = "unknown"
    try:
        body = read_event_body()
        if isinstance(body, dict) and isinstance(body.get("event_type"), str) and body["event_type"] in EVENT_TYPES:
            kind = body["event_type"]
        event = validate_event(body, current_app.config["ALLOWED_CLASSES"], current_app.config["CONFIDENCE_THRESHOLD"])
    except InvalidEvent as error:
        count_event(kind, "invalid")
        return jsonify(error=error.error, field=error.field), 400
    result, response = current_app.extensions["event_store"].record(event, current_app.extensions["cloud_configured"])
    if result == "invalid_milestone":
        count_event(kind, "invalid")
        return jsonify(error="unknown_milestone", field="milestone_id"), 400
    count_event(kind, result)
    if result == "conflict":
        return jsonify(error="event_id_conflict", field="event_id"), 409
    return jsonify(status=result, **response), 201 if result == "recorded" else 200


@gameplay.get("/gameplay/events/<event_id>")
def get_event(event_id):
    try:
        event_id = normalize_uuid(event_id, "event_id")
    except InvalidEvent as error:
        return jsonify(error=error.error, field=error.field), 400
    event = current_app.extensions["event_store"].get(event_id)
    if event is None:
        return jsonify(error="event_not_found", field="event_id"), 404
    return jsonify(status="recorded", **event)
