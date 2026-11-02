from time import perf_counter
from .preprocess import letterbox
from .postprocess import decode


class Detector:
    def __init__(self, registry, allowed_classes):
        self.registry = registry
        self.allowed_classes = set(allowed_classes)

    def predict(self, image, threshold, target_class=None):
        started = perf_counter()
        session = self.registry.session
        input_info = session.get_inputs()[0]
        size = int(input_info.shape[2])
        tensor, transform = letterbox(image, size)
        inference_start = perf_counter()
        output = session.run(None, {input_info.name: tensor})[0]
        inference_ms = (perf_counter() - inference_start) * 1000
        detections = decode(output, self.registry.names, self.allowed_classes, threshold, transform)
        matching = [d for d in detections if target_class is None or d["label"] == target_class]
        return {
            "detections": detections, "target_class": target_class,
            "target_found": target_class is not None and bool(matching),
            "best_confidence": max((d["confidence"] for d in matching), default=None),
            "threshold": threshold, "image_size": list(image.size),
            "inference_time_ms": inference_ms, "total_time_ms": (perf_counter()-started)*1000,
            "model": {"name": "yolov8n", "sha256_prefix": self.registry.sha256[:8]},
        }
