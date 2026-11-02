import argparse
import ast
import hashlib
import json
import os
import shutil
from pathlib import Path


def export_model(size=640):
    root = Path(__file__).resolve().parent
    os.environ.setdefault("YOLO_CONFIG_DIR", str(root / ".venv-export" / "ultralytics"))
    from ultralytics import YOLO
    import onnxruntime as ort

    directory = root / "models"
    directory.mkdir(exist_ok=True)
    weights = directory / "yolov8n.pt"
    if size != 640:
        alternative = directory / f"yolov8n-{size}.pt"
        shutil.copyfile(weights, alternative)
        weights = alternative
    model = YOLO(str(weights))
    exported = Path(model.export(format="onnx", imgsz=size, batch=1, opset=17,
                                 dynamic=False, simplify=True, nms=False, device="cpu"))
    destination = directory / ("yolov8n.onnx" if size == 640 else f"yolov8n-{size}.onnx")
    data = exported.read_bytes()
    if len(data) > 50_000_000:
        raise RuntimeError("Export exceeds 50 MB; do not commit")
    destination.write_bytes(data)
    session = ort.InferenceSession(str(destination), providers=["CPUExecutionProvider"])
    names = ast.literal_eval(session.get_modelmeta().custom_metadata_map["names"])
    classes = {str(key): value for key, value in names.items()}
    destination.with_suffix(".classes.json").write_text(json.dumps(classes, indent=2) + "\n", encoding="utf-8")
    report = {
        "path": str(destination), "size_mb": len(data) / 1_000_000,
        "sha256": hashlib.sha256(data).hexdigest(),
        "inputs": [item.shape for item in session.get_inputs()],
        "outputs": [item.shape for item in session.get_outputs()], "classes": classes,
    }
    print(json.dumps(report, indent=2))
    return report


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--imgsz", type=int, choices=[640, 512, 480, 416], default=640)
    export_model(parser.parse_args().imgsz)
