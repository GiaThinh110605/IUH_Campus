from datetime import datetime, timezone
from email.utils import parsedate_to_datetime
import math
from urllib.parse import quote

import requests


def cloud_configured(config):
    return bool(config.get("ADAFRUIT_AIO_USERNAME", "").strip() and config.get("ADAFRUIT_AIO_KEY", "").strip())


def retry_after_seconds(value, now=None):
    if not value:
        return None
    try:
        seconds = float(value)
    except ValueError:
        try:
            seconds = parsedate_to_datetime(value).timestamp() - (datetime.now(timezone.utc).timestamp() if now is None else now)
        except (TypeError, ValueError, OverflowError):
            return None
    return max(0, seconds) if math.isfinite(seconds) else None


class AdafruitClient:
    def __init__(self, username, key, session=None):
        self._base = "https://io.adafruit.com/api/v2/" + quote(username, safe="")
        self._headers = {"X-AIO-Key": key}
        self._session = session or requests.Session()

    def _request(self, method, path, **kwargs):
        try:
            response = self._session.request(method, self._base + path, headers=self._headers,
                                             timeout=5, allow_redirects=False, **kwargs)
            return response, None
        except requests.RequestException:
            return None, "network_error"

    def send(self, feed_key, value):
        response, error = self._request("POST", f"/feeds/{feed_key}/data", json={"value": str(value)})
        if error:
            return {"success": False, "error": error, "retry_after": None}
        success = 200 <= response.status_code < 300
        return {"success": success, "error": None if success else f"http_{response.status_code}",
                "retry_after": retry_after_seconds(response.headers.get("Retry-After")) if response.status_code == 429 else None}

    def ensure_feed(self, feed_key):
        response, error = self._request("GET", f"/feeds/{feed_key}")
        if error:
            return {"feed": feed_key, "status": "failed", "error": error}
        if response.status_code == 200:
            return {"feed": feed_key, "status": "exists"}
        if response.status_code != 404:
            return {"feed": feed_key, "status": "failed", "error": f"http_{response.status_code}"}
        response, error = self._request("POST", "/feeds", json={"name": feed_key, "key": feed_key})
        if error:
            return {"feed": feed_key, "status": "failed", "error": error}
        return {"feed": feed_key, "status": "created" if 200 <= response.status_code < 300 else "failed",
                "error": None if 200 <= response.status_code < 300 else f"http_{response.status_code}"}

    def last_value(self, feed_key):
        response, error = self._request("GET", f"/feeds/{feed_key}/data/last")
        if error or response.status_code != 200:
            return {"feed": feed_key, "error": error or f"http_{response.status_code}"}
        try:
            return {"feed": feed_key, "value": response.json()["value"]}
        except (ValueError, KeyError, TypeError):
            return {"feed": feed_key, "error": "invalid_response"}
