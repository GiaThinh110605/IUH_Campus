import os

from prometheus_client import CollectorRegistry
from prometheus_client.core import CounterMetricFamily, GaugeMetricFamily
from prometheus_client.exposition import choose_encoder
from prometheus_client.multiprocess import MultiProcessCollector
from prometheus_flask_exporter import PrometheusMetrics
from prometheus_flask_exporter.multiprocess import GunicornInternalPrometheusMetrics


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
