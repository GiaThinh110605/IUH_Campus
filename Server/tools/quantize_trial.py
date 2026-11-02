import hashlib
import json
from pathlib import Path

import onnxruntime as ort
from onnxruntime.quantization import QuantType, quantize_dynamic


def quantize_trial(source, destination):
    source, destination = Path(source), Path(destination)
    quantize_dynamic(str(source), str(destination), weight_type=QuantType.QInt8)
    report = {"source": str(source), "destination": str(destination), "weight_type": "QInt8",
              "bytes": destination.stat().st_size, "sha256": hashlib.sha256(destination.read_bytes()).hexdigest()}
    try:
        ort.InferenceSession(str(destination), providers=["CPUExecutionProvider"])
        report["loadable"] = True
    except Exception as error:
        report.update(loadable=False, error=str(error))
    print(json.dumps(report, indent=2))
    return report


if __name__ == "__main__":
    quantize_trial("models/yolov8n.onnx", "models/yolov8n-int8.onnx")
