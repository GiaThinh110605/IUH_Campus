import argparse
import json
from pathlib import Path
import sys

from tools.real_eval import MODELS, NoLabeledImagesError, evaluate

SERVER = Path(__file__).resolve().parent
DEFAULT_OUTPUT = SERVER.parent / "Docs/Evidence/CampusGuardian/B2/real-eval.md"


def write_markdown(report, output):
    lines = ["# Real-photo evaluation", "", "Labels follow user-confirmed filename prefixes; no labels inferred from image content.",
             "Timing: local image decode + preprocess + inference + postprocess; no HTTP.", "",
             "| Model | Threshold | Recall per class | False-positive none images | p50 total ms | Status |",
             "|---|---:|---|---|---:|---|"]
    for row in report["results"]:
        recalls = "; ".join(f"{name}: {values['found']}/{values['total']} ({values['recall']:.3f})"
                            for name, values in sorted(row.get("recall_by_class", {}).items())) or "N/A"
        negative = f"{row['false_positive_images']}/{row['negative_images']}" if "negative_images" in row else "N/A"
        latency = f"{row['p50_total_ms']:.3f}" if "p50_total_ms" in row else "N/A"
        lines.append(f"| {row['model']} | {row['threshold']:.2f} | {recalls} | {negative} | {latency} | {row['status']} |")
    lines += ["", "Skipped filename(s): " + (", ".join(report.get("skipped", [])) or "none"),
              "", "Photos are not modified or committed by this script. Before staging any photo, manually check people/personal information, resize approved copies to <=1280px and strip EXIF/GPS."]
    if report.get("message"):
        lines += ["", report["message"]]
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text("\n".join(lines) + "\n", encoding="utf-8")


def main():
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")
    parser = argparse.ArgumentParser()
    parser.add_argument("--directory", type=Path, default=SERVER / "tests/fixtures/real")
    parser.add_argument("--models", type=Path, default=SERVER / "models")
    parser.add_argument("--threads", type=int, default=2)
    parser.add_argument("--output", type=Path, default=DEFAULT_OUTPUT)
    args = parser.parse_args()
    try:
        report = evaluate(args.directory, args.models, args.threads)
    except (FileNotFoundError, NoLabeledImagesError) as error:
        message = (f"Chưa đánh giá độ đúng trên ảnh thật: {error}. Đặt ảnh vào {args.directory}; "
                   "dùng tiền tố book_, laptop_, chair_, bottle_, backpack_, cellphone_, none_.")
        report = {"results": [{"model": name, "threshold": threshold, "status": "no_real_photos"}
                              for name in MODELS for threshold in (.50, .60, .70)], "message": message,
                  "skipped": getattr(error, "skipped", [])}
        write_markdown(report, args.output)
        print(message)
        return 2
    write_markdown(report, args.output)
    args.output.with_suffix(".json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(f"Evaluation saved to {args.output}; skipped: {report['skipped']}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
