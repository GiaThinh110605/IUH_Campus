import logging
import signal
import time

from .cloud_client import AdafruitClient, cloud_configured
from .config import load_config
from .event_store import EventStore


class SyncWorker:
    def __init__(self, store, client=None):
        self.store, self.client = store, client

    def step(self, now=None):
        if self.client is None:
            return False
        job = self.store.claim(now)
        if job is None:
            return False
        result = self.client.send(job["feed_key"], job["value"])
        self.store.finish(job, result["success"], result.get("error"), result.get("retry_after"), now=now)
        return True


def main():
    config = load_config()
    logging.basicConfig(level=config["LOG_LEVEL"])
    logger = logging.getLogger("guardian.sync")
    client = AdafruitClient(config["ADAFRUIT_AIO_USERNAME"], config["ADAFRUIT_AIO_KEY"]) if cloud_configured(config) else None
    worker = SyncWorker(EventStore(config["DATABASE_PATH"]), client)
    running = True
    def stop(signum, frame):
        nonlocal running
        running = False
    signal.signal(signal.SIGTERM, stop)
    signal.signal(signal.SIGINT, stop)
    logger.info("[IUH Guardian API] Cloud sync worker started" if client else "[IUH Guardian API] Cloud disabled; sync worker idle")
    while running:
        worker.step()
        time.sleep(.5 if client else 5)


if __name__ == "__main__":
    main()
