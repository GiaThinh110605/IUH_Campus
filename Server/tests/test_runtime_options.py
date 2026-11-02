from unittest.mock import patch

import onnxruntime as ort
import pytest

from app.config import load_config, runtime_threads
from app.model_registry import ModelRegistry


@pytest.mark.parametrize("cpus,workers,expected", [(16, 1, 16), (16, 2, 8), (1, 2, 1), (6, 4, 1)])
def test_auto_threads_divides_available_cpu_by_workers(cpus, workers, expected):
    assert runtime_threads("", cpus, workers) == expected


def test_explicit_thread_count_is_respected():
    assert runtime_threads("4", 16, 2) == 4


def test_thread_environment_and_auto_configuration(monkeypatch):
    monkeypatch.setenv("ORT_INTRA_THREADS", "")
    monkeypatch.setenv("WEB_CONCURRENCY", "2")
    with patch("app.config.available_cpus", return_value=16):
        assert load_config()["ORT_INTRA_THREADS"] == 8
        monkeypatch.setenv("ORT_INTRA_THREADS", "4")
        assert load_config()["ORT_INTRA_THREADS"] == 4


@pytest.mark.parametrize("value", ["0", "-2", "bad"])
def test_invalid_thread_count_is_rejected(value):
    with pytest.raises(ValueError):
        runtime_threads(value, 16, 2)


def test_session_options_are_explicit(tmp_path):
    model = tmp_path / "model.onnx"
    model.write_bytes(b"fixture")
    with patch("app.model_registry.ort.InferenceSession") as session:
        session.return_value.get_modelmeta.return_value.custom_metadata_map = {"names": "{0: 'book'}"}
        ModelRegistry(str(model), logger=None, threads=4)
        options = session.call_args.kwargs["sess_options"]
        assert options.graph_optimization_level == ort.GraphOptimizationLevel.ORT_ENABLE_ALL
        assert options.execution_mode == ort.ExecutionMode.ORT_SEQUENTIAL
        assert options.intra_op_num_threads == 4
        assert options.inter_op_num_threads == 1
