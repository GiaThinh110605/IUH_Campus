import numpy as np
from PIL import Image
import pytest

from app.inference.preprocess import letterbox
from app.inference.postprocess import decode, nms


def test_letterbox_shape_normalization_and_inverse_box():
    tensor, transform = letterbox(Image.new("RGB", (800, 400), "white"), 640)
    assert tensor.shape == (1, 3, 640, 640)
    assert tensor.dtype == np.float32
    assert tensor[0, 0, 320, 320] == 1
    assert transform.pad == (0, 160)
    assert transform.scale == .8
    np.testing.assert_allclose(transform.restore(np.array([[80, 200, 240, 360]], dtype=float)), [[100, 50, 300, 250]])


def test_nms_suppresses_overlap_but_not_separate_boxes():
    boxes = np.array([[0, 0, 100, 100], [1, 1, 99, 99], [200, 200, 300, 300]], float)
    assert nms(boxes, np.array([.9, .8, .7]), .45) == [0, 2]


def test_extremely_thin_image_still_resizes_to_valid_tensor():
    tensor, transform = letterbox(Image.new("RGB", (10000, 1)), 640)
    assert tensor.shape == (1, 3, 640, 640)
    assert transform.original_size == (10000, 1)


def test_decode_filters_class_threshold_and_scales_to_original():
    _, transform = letterbox(Image.new("RGB", (800, 400)), 640)
    names = {i: f"class_{i}" for i in range(80)}
    names[7] = "book"
    raw = np.zeros((1, 84, 3), np.float32)
    raw[0, :4, :] = np.array([[160, 160, 400], [280, 280, 280], [160, 160, 80], [160, 160, 80]])
    raw[0, 11, 0] = .9
    raw[0, 11, 1] = .69
    raw[0, 5, 2] = .99
    result = decode(raw, names, {"book"}, .7, transform)
    assert len(result) == 1
    assert result[0]["label"] == "book"
    assert result[0]["class_id"] == 7
    np.testing.assert_allclose(result[0]["bbox"], [100, 50, 200, 200])


@pytest.mark.parametrize("value", ["0.69", "1.01", "nan", "oops"])
def test_bad_threshold_is_400(value):
    from app import create_app
    client = create_app({"TESTING": True, "MODEL_PATH": ""}).test_client()
    assert client.post("/predict", data={"threshold": value}).status_code == 400


def test_unknown_target_is_400():
    from app import create_app
    client = create_app({"TESTING": True, "MODEL_PATH": ""}).test_client()
    assert client.post("/predict", data={"target_class": "spaceship"}).status_code == 400


def test_oversized_upload_returns_json_413():
    import io
    from app import create_app
    client = create_app({"TESTING": True, "MODEL_PATH": "", "MAX_CONTENT_LENGTH": 100}).test_client()
    response = client.post("/predict", data={"image": (io.BytesIO(b"x" * 200), "large.png")})
    assert response.status_code == 413
    assert response.get_json() == {"error": "upload_too_large"}
