import os

from prometheus_client import CollectorRegistry
from prometheus_client.core import CounterMetricFamily, GaugeMetricFamily
from prometheus_client.exposition import choose_encoder
from prometheus_client.multiprocess import MultiProcessCollector
from prometheus_flask_exporter import PrometheusMetrics
from prometheus_flask_exporter.multiprocess import GunicornInternalPrometheusMetrics
from .scan_store import scan_metrics


class DurableCollector:
    def __init__(self, store):
        self.store = store

    def collect(self):
        summary = self.store.summary()
        pending = GaugeMetricFamily("outbox_pending", "Pending durable cloud rows")
        pending.add_metric([], summary["outbox_pending"])
        yield pending
        synced = CounterMetricFamily("cloud_sync", "Durable cloud send outcomes", labels=["result"])
        for result in ("synced", "retry", "failed", "rate_limited"):
            synced.add_metric([result], summary["sync_stats"].get(result, 0))
        yield synced
        scans = scan_metrics(self.store)
        sessions = CounterMetricFamily("scan_sessions", "Durable scan sessions created")
        sessions.add_metric([], scans["sessions"])
        yield sessions
        rejected = CounterMetricFamily("scan_sessions_rejected", "Rejected scan sessions", labels=["reason"])
        for reason in ("scan_session_active", "scan_cooldown", "scan_limit_reached"):
            rejected.add_metric([reason], scans["rejections"].get(reason, 0))
        yield rejected
        results = CounterMetricFamily("scan_results", "Scan responses", labels=["result"])
        for result in ("infested", "clean", "none"):
            results.add_metric([result], scans["results"].get(result, 0))
        yield results
        reveals = CounterMetricFamily("monsters_revealed", "Unique revealed monsters", labels=["label"])
        for label, count in scans["reveals"].items():
            reveals.add_metric([label], count)
        yield reveals


class DurableMetricsMixin:
    def generate_metrics(self, accept_header=None, names=None):
        registry = self.registry
        if os.getenv("PROMETHEUS_MULTIPROC_DIR"):
            registry = CollectorRegistry()
            MultiProcessCollector(registry)
            registry.register(DurableCollector(self.event_store))
        if names:
            registry = registry.restricted_registry(names)
        encoder, content_type = choose_encoder(accept_header)
        return encoder(registry).decode("utf-8"), content_type


class DurableMetrics(DurableMetricsMixin, PrometheusMetrics):
    pass


class DurableGunicornMetrics(DurableMetricsMixin, GunicornInternalPrometheusMetrics):
    pass
