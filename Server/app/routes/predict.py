from flask import Blueprint, current_app, jsonify, request
from PIL import Image, UnidentifiedImageError

predict = Blueprint("predict", __name__)


@predict.post("/predict")
def predict_image():
    image = request.files.get("image")
    if image is None:
        return jsonify(error="image_required"), 400
    try:
        with Image.open(image.stream) as decoded:
            decoded.verify()
    except (UnidentifiedImageError, OSError, ValueError, Image.DecompressionBombError):
        return jsonify(error="invalid_image"), 400
    if not current_app.extensions["model_registry"].loaded:
        return jsonify(error="model_not_loaded"), 503
    # TODO(B2): preprocess image, run ONNX inference and return filtered detections.
    return jsonify(error="inference_not_implemented"), 501
