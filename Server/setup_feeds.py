import json

from app.cloud_client import AdafruitClient, cloud_configured
from app.config import load_config
from app.event_store import FEEDS
from app.scan_store import MONSTER_FEED


def main():
    config = load_config()
    if not cloud_configured(config):
        print(json.dumps({"cloud": "disabled", "credentials": "****"}))
        return 0
    client = AdafruitClient(config["ADAFRUIT_AIO_USERNAME"], config["ADAFRUIT_AIO_KEY"])
    results = [client.ensure_feed(feed) for feed in (*FEEDS, MONSTER_FEED)]
    print(json.dumps({"credentials": "****", "feeds": results}, indent=2))
    return 1 if any(row["status"] == "failed" for row in results) else 0


if __name__ == "__main__":
    raise SystemExit(main())
