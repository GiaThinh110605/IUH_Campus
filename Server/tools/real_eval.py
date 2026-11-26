import csv
import hashlib
import logging
import os
import platform
import subprocess
from collections import Counter
from datetime import datetime, timedelta, timezone
from pathlib import Path
from time import perf_counter

import numpy as np
from PIL import Image, ImageOps

from app.model_registry import ModelRegistry
from app.inference.detector import Detector

PREFIX_LABELS = {'book': 'book', 'laptop': 'laptop', 'chair': 'chair', 'bottle': 'bottle',
                 'backpack': 'backpack', 'cellphone': 'cell phone', 'none': 'none'}
IMAGE_SUFFIXES = {'.jpg', '.jpeg', '.png', '.webp', '.bmp'}
MODELS = {'640 fp32': 'yolov8n.onnx'}
TARGETS = tuple(label for label in PREFIX_LABELS.values() if label != 'none')
MAX_IMAGE_SIDE = 640
WARMUP_RUNS = 5
EVAL_THRESHOLDS = (.50, .60, .70)
REPORT_THRESHOLD = .70
DIAGNOSTIC_THRESHOLD = .01
EXIF_MAKE_TAG = 271
EXIF_MODEL_TAG = 272
PHONE_BRANDS = ('iphone', 'apple', 'samsung', 'galaxy', 'xiaomi', 'redmi', 'poco',
                'oppo', 'vivo', 'realme', 'oneplus', 'pixel', 'huawei', 'honor', 'motorola')
BYTES_PER_MIB = 1024 ** 2
MS_PER_SECOND = 1000
P50_PERCENTILE = 50
P95_PERCENTILE = 95
REPORT_TZ = timezone(timedelta(hours=7))
TIMING_NOTE = (
    'local disk decode + EXIF orientation + resize + detector preprocessing/inference/postprocessing; '
    'no HTTP; none = six sequential target calls; diagnostic excluded; five warmups'
)


class NoLabeledImagesError(ValueError):
    def __init__(self, skipped):
        super().__init__('No image with a user-confirmed folder label or filename prefix')
        self.skipped = skipped


def build_labels(directory):
    directory = Path(directory)
    if not directory.is_dir():
        raise FileNotFoundError(f'Real-photo directory unavailable: {directory}')
    labels, skipped = [], []
    for file in sorted(directory.rglob('*')):
        if not file.is_file() or file.suffix.lower() not in IMAGE_SUFFIXES:
            continue
        relative = file.relative_to(directory)
        prefix, separator, _ = file.stem.partition('_')
        label = relative.parts[0] if len(relative.parts) > 1 else prefix if separator else ''
        if label not in PREFIX_LABELS:
            skipped.append(relative.as_posix())
            continue
        labels.append({'file': relative.as_posix(), 'target_class': PREFIX_LABELS[label]})
    # Backward-compatible manifest lives only in the ignored photo directory.
    with (directory / 'labels.csv').open('w', newline='', encoding='utf-8') as stream:
        writer = csv.DictWriter(stream, fieldnames=['file', 'target_class'])
        writer.writeheader()
        writer.writerows(labels)
    return labels, skipped


def prepare_image(path):
    with Image.open(path) as source:
        image = ImageOps.exif_transpose(source).convert('RGB')
    scale = min(1.0, MAX_IMAGE_SIDE / max(image.size))
    size = tuple(max(1, round(value * scale)) for value in image.size)
    return image.resize(size, Image.Resampling.LANCZOS) if size != image.size else image


def image_metadata(path):
    # Only access Orientation/Make/Model; never enumerate or serialize EXIF/GPS.
    with Image.open(path) as source:
        exif = source.getexif()
        make = str(exif.get(EXIF_MAKE_TAG, '')).strip()
        model = str(exif.get(EXIF_MODEL_TAG, '')).strip()
        phone = any(word in (make + ' ' + model).lower() for word in PHONE_BRANDS)
        return {'make': make, 'device_model': model,
                'source': 'self_inferred' if phone else 'unknown_web',
                'original_size': list(source.size), 'original_bytes': path.stat().st_size,
                'sha256': hashlib.sha256(path.read_bytes()).hexdigest()}


def summarize(rows):
    classes = {}
    positive = [r for r in rows if r['target_class'] != 'none']
    negative = [r for r in rows if r['target_class'] == 'none']
    for row in positive:
        counts = classes.setdefault(row['target_class'], {'found': 0, 'total': 0})
        counts['total'] += 1
        counts['found'] += int(row['target_found'])
    for counts in classes.values():
        counts['recall'] = counts['found'] / counts['total']
    confidences = [r['best_confidence'] for r in positive if r['target_found']]
    return {'recall_by_class': classes, 'positive_found': sum(r['target_found'] for r in positive),
            'positive_total': len(positive), 'negative_images': len(negative),
            'false_positive_images': sum(r['false_positive'] for r in negative),
            'mean_correct_confidence': float(np.mean(confidences)) if confidences else None,
            'p50_total_ms': float(np.percentile([r['total_ms'] for r in rows], P50_PERCENTILE)),
            'p95_total_ms': float(np.percentile([r['total_ms'] for r in rows], P95_PERCENTILE))}


def cpu_name():
    if os.name == 'nt':
        import winreg
        with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r'HARDWARE\DESCRIPTION\System\CentralProcessor\0') as key:
            return winreg.QueryValueEx(key, 'ProcessorNameString')[0].strip()
    return platform.processor() or os.environ.get('PROCESSOR_IDENTIFIER', 'unknown')


def _split_by_size(labels, metadata, max_original_mb):
    max_bytes = max_original_mb * BYTES_PER_MIB
    excluded = [
        dict(entry, reason='original >5 MiB (user-confirmed exclusion)',
             original_bytes=metadata[entry['file']]['original_bytes'])
        for entry in labels
        if metadata[entry['file']]['original_bytes'] > max_bytes
    ]
    excluded_names = {entry['file'] for entry in excluded}
    selected = [entry for entry in labels if entry['file'] not in excluded_names]
    return selected, excluded


def _load_detector(model_directory, threads):
    model = Path(model_directory) / MODELS['640 fp32']
    registry = ModelRegistry(str(model), logging.getLogger('real_eval'), threads)
    if not registry.loaded:
        raise RuntimeError('Default YOLOv8n 640 model unavailable')
    return registry, Detector(registry, TARGETS)


def _warm_up(detector, image):
    for _ in range(WARMUP_RUNS):
        detector.predict(image, REPORT_THRESHOLD)


def _evaluate_image(detector, directory, entry, threshold, metadata):
    start = perf_counter()
    image = prepare_image(Path(directory) / entry['file'])
    target = entry['target_class']
    if target == 'none':
        trials = [detector.predict(image, threshold, cls) for cls in TARGETS]
        result = trials[0]
        found = any(trial['target_found'] for trial in trials)
    else:
        result = detector.predict(image, threshold, target)
        trials, found = [], False
    elapsed = (perf_counter() - start) * MS_PER_SECOND
    # Diagnostic predictions below the tested threshold help explain misses.
    diagnostic = detector.predict(image, DIAGNOSTIC_THRESHOLD, None)['detections'] if threshold == REPORT_THRESHOLD else []
    return {
        **entry,
        'source': metadata[entry['file']]['source'],
        'prepared_size': list(image.size),
        'target_found': result['target_found'] if target != 'none' else False,
        'false_positive': found,
        'best_confidence': result['best_confidence'] if target != 'none' else None,
        'detections': result['detections'],
        'diagnostic_at_0_01': diagnostic,
        'none_trials': [{'target_class': t['target_class'], 'target_found': t['target_found']} for t in trials],
        'total_ms': elapsed,
    }


def _evaluate_threshold(detector, directory, selected, threshold, metadata):
    rows = [_evaluate_image(detector, directory, entry, threshold, metadata) for entry in selected]
    sources = sorted({row['source'] for row in rows})
    return {
        'model': '640 fp32',
        'threshold': threshold,
        'status': 'measured',
        **summarize(rows),
        'by_source': {s: summarize([r for r in rows if r['source'] == s]) for s in sources},
        'images': rows,
    }


def _run_context():
    return {
        'commit': subprocess.check_output(['git', 'rev-parse', 'HEAD'], text=True).strip(),
        'evaluator_sha256': hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),
        'date': datetime.now(REPORT_TZ).isoformat(),
        'os': platform.platform(),
        'cpu': cpu_name(),
    }


def evaluate(directory, model_directory, threads, max_original_mb=5):
    labels, skipped = build_labels(directory)
    if not labels:
        raise NoLabeledImagesError(skipped)
    metadata = {entry['file']: image_metadata(Path(directory) / entry['file']) for entry in labels}
    selected, excluded = _split_by_size(labels, metadata, max_original_mb)
    if not selected:
        raise NoLabeledImagesError([entry['file'] for entry in excluded])
    registry, detector = _load_detector(model_directory, threads)
    _warm_up(detector, prepare_image(Path(directory) / selected[0]['file']))
    results = [
        _evaluate_threshold(detector, directory, selected, threshold, metadata)
        for threshold in EVAL_THRESHOLDS
    ]
    return {
        'labels': selected,
        'discovered_count': len(labels),
        'excluded': excluded,
        'metadata': metadata,
        'skipped': skipped,
        'threads': threads,
        'results': results,
        'model_sha256': registry.sha256,
        **_run_context(),
        'timing': TIMING_NOTE,
        'counts': dict(Counter(entry['target_class'] for entry in selected)),
    }
