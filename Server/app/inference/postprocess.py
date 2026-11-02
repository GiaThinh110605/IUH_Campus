import numpy as np


def nms(boxes, scores, iou_threshold=.45):
    order = np.argsort(-scores)
    keep = []
    areas = np.maximum(0, boxes[:, 2] - boxes[:, 0]) * np.maximum(0, boxes[:, 3] - boxes[:, 1])
    while len(order):
        current = int(order[0])
        keep.append(current)
        rest = order[1:]
        left = np.maximum(boxes[current, :2], boxes[rest, :2])
        right = np.minimum(boxes[current, 2:], boxes[rest, 2:])
        overlap = np.maximum(0, right - left)
        intersection = overlap[:, 0] * overlap[:, 1]
        union = areas[current] + areas[rest] - intersection
        iou = intersection / np.maximum(union, 1e-9)
        order = rest[iou <= iou_threshold]
    return keep


def decode(output, names, allowed_classes, threshold, transform, iou_threshold=.45):
    predictions = np.asarray(output)
    if predictions.ndim != 3 or predictions.shape[0] != 1 or predictions.shape[1] != 4 + len(names):
        raise ValueError("Unsupported YOLOv8 output shape")
    rows = predictions[0].T
    class_ids = rows[:, 4:].argmax(axis=1)
    scores = rows[np.arange(len(rows)), class_ids + 4]
    allowed_ids = {key for key, value in names.items() if value in allowed_classes}
    selected = (scores >= threshold) & np.isin(class_ids, list(allowed_ids)) & np.isfinite(rows).all(axis=1)
    rows, class_ids, scores = rows[selected], class_ids[selected], scores[selected]
    boxes = np.column_stack((rows[:, 0] - rows[:, 2] / 2, rows[:, 1] - rows[:, 3] / 2,
                             rows[:, 0] + rows[:, 2] / 2, rows[:, 1] + rows[:, 3] / 2))
    kept = []
    for class_id in np.unique(class_ids):
        indices = np.flatnonzero(class_ids == class_id)
        kept.extend(indices[nms(boxes[indices], scores[indices], iou_threshold)])
    restored = transform.restore(boxes)
    detections = []
    for index in sorted(kept, key=lambda item: -float(scores[item])):
        x1, y1, x2, y2 = restored[index]
        if x2 <= x1 or y2 <= y1:
            continue
        detections.append({"label": names[int(class_ids[index])], "class_id": int(class_ids[index]),
                           "confidence": float(scores[index]),
                           "bbox": [round(float(v), 3) for v in (x1, y1, x2-x1, y2-y1)]})
    return detections
