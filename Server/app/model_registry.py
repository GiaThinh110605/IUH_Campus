import ast
import hashlib
from pathlib import Path
import onnxruntime as ort


class ModelRegistry:
    def __init__(self, model_path, logger, threads=2):
        self.session = None
        self.names = {}
        self.sha256 = ""
        self.path_configured = bool(model_path)
        if not model_path or not Path(model_path).is_file():
            logger.warning("[IUH Guardian API] Model file unavailable; inference disabled")
            return
        try:
            options = ort.SessionOptions()
            options.graph_optimization_level = ort.GraphOptimizationLevel.ORT_ENABLE_ALL
            options.execution_mode = ort.ExecutionMode.ORT_SEQUENTIAL
            options.intra_op_num_threads = threads
            options.inter_op_num_threads = 1
            session = ort.InferenceSession(model_path, sess_options=options, providers=["CPUExecutionProvider"])
            metadata = ast.literal_eval(session.get_modelmeta().custom_metadata_map["names"])
            self.names = {int(key): str(value) for key, value in metadata.items()}
            self.sha256 = hashlib.sha256(Path(model_path).read_bytes()).hexdigest()
            self.session = session
        except Exception:
            logger.warning("[IUH Guardian API] Model could not be loaded; inference disabled")

    @property
    def loaded(self):
        return self.session is not None
