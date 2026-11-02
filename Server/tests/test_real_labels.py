import csv
from pathlib import Path
import subprocess
import sys

import pytest
from PIL import Image

from tools.real_eval import build_labels


def test_labels_follow_only_user_confirmed_prefixes(tmp_path):
    for name in ["book_01.jpg", "cellphone_02.png", "none_03.jpg", "unknown_04.jpg", "laptop.jpg"]:
        Image.new("RGB", (8, 8)).save(tmp_path / name)
    labels, skipped = build_labels(tmp_path)
    assert labels == [{"file": "book_01.jpg", "target_class": "book"},
                      {"file": "cellphone_02.png", "target_class": "cell phone"},
                      {"file": "none_03.jpg", "target_class": "none"}]
    assert skipped == ["laptop.jpg", "unknown_04.jpg"]
    with (tmp_path / "labels.csv").open(encoding="utf-8") as stream:
        assert list(csv.DictReader(stream)) == labels


def test_missing_real_photos_are_not_fabricated(tmp_path):
    absent = tmp_path / "absent"
    with pytest.raises(FileNotFoundError):
        build_labels(absent)
    assert not absent.exists()


@pytest.mark.parametrize("exists", [False, True])
def test_empty_eval_cli_returns_two_without_traceback(tmp_path, exists):
    script = Path(__file__).resolve().parents[1] / "eval_real.py"
    directory = tmp_path / "real"
    if exists:
        directory.mkdir()
    result = subprocess.run([sys.executable, str(script), "--directory", str(directory),
                             "--output", str(tmp_path / "real-eval.md")],
                            capture_output=True, text=True, encoding="utf-8", errors="replace",
                            env={**__import__("os").environ, "PYTHONUTF8": "1"})
    assert result.returncode == 2
    assert "Traceback" not in result.stderr
    assert "no_real_photos" in (tmp_path / "real-eval.md").read_text(encoding="utf-8")


def test_all_unknown_names_are_listed_without_guessing(tmp_path):
    directory = tmp_path / "real"
    directory.mkdir()
    (directory / "unknown_01.jpg").write_bytes(b"not evaluated")
    script = Path(__file__).resolve().parents[1] / "eval_real.py"
    output = tmp_path / "report.md"
    result = subprocess.run([sys.executable, str(script), "--directory", str(directory), "--output", str(output)],
                            capture_output=True, text=True, encoding="utf-8", errors="replace")
    assert result.returncode == 2
    assert "Skipped filename(s): unknown_01.jpg" in output.read_text(encoding="utf-8")
