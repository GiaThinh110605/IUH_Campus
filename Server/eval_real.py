import argparse
import json
from pathlib import Path
import sys

from tools.real_eval import MODELS, NoLabeledImagesError, evaluate

SERVER = Path(__file__).resolve().parent
DEFAULT_OUTPUT = SERVER.parent / 'Docs/Evidence/CampusGuardian/B2/real-eval.md'
SOURCES = {'self_inferred': 'Tự chụp (đoán theo EXIF)', 'unknown_web': 'Không rõ/ảnh mạng (không có EXIF điện thoại)'}


def write_markdown(report, output):
    lines = ['# B2.6 — Đánh giá YOLOv8n 640 trên ảnh thật', '']
    if report.get('message'):
        lines += [report['message'], '', 'Status: no_real_photos',
                  'Skipped filename(s): ' + (', '.join(report.get('skipped', [])) or 'none')]
    else:
        lines += [f"- Ngày (Asia/Bangkok): {report['date']}", f"- Commit nền: `{report['commit']}`; script đánh giá thay đổi trong commit B2.6 (SHA256 trong JSON).",
                  f"- OS: {report['os']}; CPU: {report['cpu']}; ONNX CPUExecutionProvider, {report['threads']} intra-op threads.",
                  f"- Model: YOLOv8n 640 FP32, SHA256 `{report['model_sha256']}`. Backend vẫn mặc định 0.70.",
                  f"- Phát hiện {report['discovered_count']} ảnh; đánh giá {len(report['labels'])}; loại {len(report['excluded'])} ảnh gốc >5 MiB theo xác nhận người dùng.",
                  '- Nhãn theo thư mục do người dùng xác nhận, cellphone = cell phone; không suy ra nhãn từ nội dung ảnh.',
                  '- Preprocessing: EXIF Orientation trước RGB/resize LANCZOS; cạnh dài ≤640 px, không upscale. Cùng giới hạn kích thước GuardianImageEncoder, không mô phỏng Unity JPEG/render filter.',
                  '- Nguồn: chỉ đọc EXIF Make/Model để suy đoán điện thoại; không có EXIF điện thoại = không rõ/ảnh mạng, không khẳng định là ảnh mạng. Không đọc/in/lưu GPS.',
                  '- Timing: ' + report['timing'],
                  '- Đây là recall theo sự hiện diện lớp ở cấp ảnh, không phải mAP hay đánh giá bounding box. Ảnh none: thử đủ sáu target_class, FP nếu bất kỳ lần nào target_found.',
                  '- Confidence đúng = trung bình best_confidence của target trên các ảnh dương đúng (không tính none).',
                  '- Raw predictions, timing từng ảnh, manifest và metadata được lưu trong [real-eval.json](real-eval.json).', '',
                  '| Nhãn | Số ảnh đánh giá |', '|---|---:|']
        for cls, count in sorted(report['counts'].items()):
            lines.append(f'| {cls} | {count} |')
        def table(title, source=None):
            lines.extend(['', '## ' + title, '', '| Ngưỡng | Recall từng lớp | Recall tổng | FP none | Conf đúng TB | p50 ms | p95 ms |', '|---:|---|---:|---:|---:|---:|---:|'])
            for result in report['results']:
                row = result if source is None else result['by_source'][source]
                recalls = '; '.join(f"{cls}: {c['found']}/{c['total']} ({c['recall']:.1%})" for cls, c in sorted(row['recall_by_class'].items()))
                conf = f"{row['mean_correct_confidence']:.4f}" if row['mean_correct_confidence'] is not None else 'N/A'
                denom = row['positive_total']
                total = f"{row['positive_found']}/{denom} ({row['positive_found']/denom:.1%})" if denom else 'N/A'
                lines.append(f"| {result['threshold']:.2f} | {recalls or 'N/A'} | {total} | {row['false_positive_images']}/{row['negative_images']} | {conf} | {row['p50_total_ms']:.2f} | {row['p95_total_ms']:.2f} |")
        table('Kết quả tổng')
        if len(report['results'][0]['by_source']) > 1:
            for source in report['results'][0]['by_source']:
                table(SOURCES[source], source)
        lines += ['', '## Nguồn từng ảnh', '', '| File | Nguồn suy đoán | Make / Model |', '|---|---|---|']
        for entry in report['labels']:
            meta = report['metadata'][entry['file']]
            device = (meta['make'] + ' / ' + meta['device_model']).replace('|', '/')
            lines.append(f"| {entry['file']} | {SOURCES[meta['source']]} | {device} |")
        lines += ['', '## Ảnh trượt tại 0.70', '', 'Dự đoán giải thích dùng ngưỡng chẩn đoán 0.01, không tính vào kết quả 0.70. Chỉ liệt kê sáu lớp mục tiêu.', '', '| File | Nhãn mong đợi | Model thấy (nhãn/conf tại ≥0.01) |', '|---|---|---|']
        failures = []
        for row in report['results'][-1]['images']:
            if (row['target_class'] != 'none' and not row['target_found']) or row['false_positive']:
                failures.append(row)
                predictions = ', '.join(f"{d['label']} {d['confidence']:.4f}" for d in row['diagnostic_at_0_01']) or 'không có lớp mục tiêu ≥0.01'
                lines.append(f"| {row['file']} | {row['target_class']} | {predictions} |")
        if not failures:
            lines.append('| Không có | — | — |')
        lines += ['', '## Ảnh loại', '']
        lines += [f"- {e['file']}: {e['original_bytes']} bytes; {e['reason']}." for e in report['excluded']] or ['Không có.']
        lines += ['', '## Giới hạn', '',
                  f"Mẫu nhỏ: đầu bài dự kiến 35 ảnh, thực tế có {report['discovered_count']} ảnh, sau lọc còn {len(report['labels'])}. Không tự loại ảnh để khớp con số dự kiến. Ảnh mạng có thể dễ hơn camera thật lúc chơi; nguồn suy đoán EXIF chưa được xác minh. Loại ảnh nặng có thể gây thiên lệch mẫu.",
                  'Không đo upload/HTTP, JPEG encoding Unity, camera iOS, blur khi di chuyển, combat hay cloud. Timing none gồm sáu lần inference nên không dùng thay benchmark /predict một request. Không điều chỉnh model/ngưỡng từ tập này; cần tập kiểm thử độc lập lớn hơn nếu dùng kết quả để tối ưu.']
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text('\n'.join(lines) + '\n', encoding='utf-8')


def main():
    if hasattr(sys.stdout, 'reconfigure'):
        sys.stdout.reconfigure(encoding='utf-8')
    parser = argparse.ArgumentParser()
    parser.add_argument('--directory', type=Path, default=SERVER / 'tests/fixtures/real')
    parser.add_argument('--models', type=Path, default=SERVER / 'models')
    parser.add_argument('--threads', type=int, default=2)
    parser.add_argument('--output', type=Path, default=DEFAULT_OUTPUT)
    args = parser.parse_args()
    try:
        report = evaluate(args.directory, args.models, args.threads)
    except (FileNotFoundError, NoLabeledImagesError) as error:
        report = {'results': [], 'message': f'Chưa đánh giá độ đúng trên ảnh thật: {error}', 'skipped': getattr(error, 'skipped', [])}
        write_markdown(report, args.output)
        print(report['message'])
        return 2
    write_markdown(report, args.output)
    args.output.with_suffix('.json').write_text(json.dumps(report, indent=2, ensure_ascii=False) + '\n', encoding='utf-8')
    print(f'Evaluation saved to {args.output}; images: {len(report["labels"])}; excluded: {len(report["excluded"])}')
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
