from flask import Blueprint, current_app, jsonify, request
from werkzeug.exceptions import BadRequest, RequestEntityTooLarge

from ..event_validation import InvalidEvent, normalize_uuid, whitelist
from ..scan_store import ScanSessionError

scan = Blueprint("scan", __name__)


@scan.post("/scan/sessions")
def create_session():
    try:
        request.max_content_length = 16 * 1024
        if not request.is_json:
            raise InvalidEvent("body", "invalid_json")
        body = request.get_json()
        whitelist(body, {"session_id"}, {"session_id"})
        game_id = normalize_uuid(body["session_id"], "session_id")
    except (BadRequest, RequestEntityTooLarge):
        return jsonify(error="invalid_json_or_body_too_large", field="body"), 400
    except InvalidEvent as error:
        return jsonify(error=error.error, field=error.field), 400
    try:
        return jsonify(current_app.extensions["scan_store"].create(game_id)), 201
    except ScanSessionError as error:
        body = {"error": error.error}
        if error.scan_session_id:
            body["scan_session_id"] = error.scan_session_id
        response = jsonify(body)
        if error.retry_after is not None:
            response.headers["Retry-After"] = str(error.retry_after)
        return response, error.status


@scan.get("/scan/sessions/<scan_id>")
def get_session(scan_id):
    try:
        return jsonify(current_app.extensions["scan_store"].status(scan_id))
    except ScanSessionError as error:
        return jsonify(error=error.error), error.status
