from pathlib import Path
import onnxruntime as ort


class ModelRegistry:
    def __init__(self, model_path, logger):
        self.session = None
        self.path_configured = bool(model_path)
        if not model_path or not Path(model_path).is_file():
            logger.warning("[IUH Guardian API] Model file unavailable; inference disabled")
            return
        try:
            self.session = ort.InferenceSession(model_path, providers=["CPUExecutionProvider"])
        except Exception:
            logger.warning("[IUH Guardian API] Model could not be loaded; inference disabled")

    @property
    def loaded(self):
        return self.session is not None
