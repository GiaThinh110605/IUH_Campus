from PIL import Image

from tools.real_eval import build_labels, prepare_image, summarize, image_metadata


def test_folder_label_overrides_filename_and_preserves_original(tmp_path):
    folder = tmp_path / 'cellphone'
    folder.mkdir()
    image = folder / 'book1.jpg'
    Image.new('RGB', (80, 40)).save(image)
    before = image.read_bytes()
    labels, skipped = build_labels(tmp_path)
    assert labels == [{'file': 'cellphone/book1.jpg', 'target_class': 'cell phone'}]
    assert skipped == []
    assert image.read_bytes() == before


def test_orientation_before_resize_and_no_upscale(tmp_path):
    path = tmp_path / 'rotated.jpg'
    exif = Image.Exif()
    exif[274] = 6
    Image.new('RGB', (1200, 600)).save(path, exif=exif)
    assert prepare_image(path).size == (320, 640)
    Image.new('RGB', (80, 40)).save(path)
    assert prepare_image(path).size == (80, 40)


def test_summary_separates_negative_images_and_correct_confidence():
    rows = [
        {'target_class': 'book', 'target_found': True, 'best_confidence': .9, 'total_ms': 10},
        {'target_class': 'book', 'target_found': False, 'best_confidence': None, 'total_ms': 20},
        {'target_class': 'none', 'false_positive': True, 'total_ms': 60},
    ]
    result = summarize(rows)
    assert result['positive_found'] == 1
    assert result['positive_total'] == 2
    assert result['false_positive_images'] == result['negative_images'] == 1
    assert result['mean_correct_confidence'] == .9
    assert result['p50_total_ms'] == 20
    assert result['p95_total_ms'] == 56


def test_source_inference_exports_only_safe_metadata(tmp_path):
    path = tmp_path / 'phone.jpg'
    exif = Image.Exif()
    exif[271], exif[272] = 'Apple', 'iPhone 15'
    Image.new('RGB', (8, 8)).save(path, exif=exif)
    result = image_metadata(path)
    assert result['source'] == 'self_inferred'
    assert set(result) == {'make', 'device_model', 'source', 'original_size', 'original_bytes', 'sha256'}
