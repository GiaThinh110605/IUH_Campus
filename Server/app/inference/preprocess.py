from dataclasses import dataclass
import numpy as np
from PIL import Image


@dataclass(frozen=True)
class LetterboxTransform:
    scale: float
    pad: tuple
    original_size: tuple

    def restore(self, boxes):
        result = np.asarray(boxes, dtype=float).copy()
        result[:, [0, 2]] = (result[:, [0, 2]] - self.pad[0]) / self.scale
        result[:, [1, 3]] = (result[:, [1, 3]] - self.pad[1]) / self.scale
        result[:, [0, 2]] = np.clip(result[:, [0, 2]], 0, self.original_size[0])
        result[:, [1, 3]] = np.clip(result[:, [1, 3]], 0, self.original_size[1])
        return result


def letterbox(image, size=640):
    width, height = image.size
    scale = min(size / width, size / height)
    resized = (max(1, round(width * scale)), max(1, round(height * scale)))
    pad = ((size - resized[0]) // 2, (size - resized[1]) // 2)
    canvas = Image.new("RGB", (size, size), (114, 114, 114))
    canvas.paste(image.convert("RGB").resize(resized, Image.Resampling.BILINEAR), pad)
    pixels = np.asarray(canvas, dtype=np.float32) / 255.0
    tensor = np.ascontiguousarray(pixels.transpose(2, 0, 1)[None])
    return tensor, LetterboxTransform(scale, pad, (width, height))
