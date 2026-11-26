# -*- coding: utf-8 -*-
"""
Vector diagrams & charts for PDF using ReportLab Graphics (pure black & white, TimesVN).
"""
from reportlab.graphics.shapes import Drawing, Rect, Line, String, Polygon
from reportlab.lib import colors


def _box(d, x, y, w, h, lines, bold_first=True, font_size=9.0, stroke_w=0.9):
    d.add(Rect(x, y, w, h, fillColor=colors.white, strokeColor=colors.black, strokeWidth=stroke_w))
    n = len(lines)
    line_h = font_size + 2.5
    total_h = n * line_h
    start_y = y + (h + total_h) / 2.0 - font_size
    for idx, txt in enumerate(lines):
        fn = "TimesVN-Bold" if (bold_first and idx == 0) else "TimesVN"
        d.add(String(x + w / 2.0, start_y - idx * line_h, txt, fontName=fn, fontSize=font_size,
                     fillColor=colors.black, textAnchor="middle"))


def _arrow(d, x1, y1, x2, y2, label=None, lx=None, ly=None, dashed=False):
    kw = {"strokeColor": colors.black, "strokeWidth": 0.9}
    if dashed:
        kw["strokeDashArray"] = [3, 2]
    d.add(Line(x1, y1, x2, y2, **kw))
    import math
    ang = math.atan2(y2 - y1, x2 - x1)
    size = 5.0
    p1 = (x2 - size * math.cos(ang - 0.4), y2 - size * math.sin(ang - 0.4))
    p2 = (x2 - size * math.cos(ang + 0.4), y2 - size * math.sin(ang + 0.4))
    d.add(Polygon([x2, y2, p1[0], p1[1], p2[0], p2[1]], fillColor=colors.black, strokeColor=colors.black))
    if label:
        tx = lx if lx is not None else (x1 + x2) / 2.0
        ty = ly if ly is not None else (y1 + y2) / 2.0 + 4
        d.add(String(tx, ty, label, fontName="TimesVN-Italic", fontSize=8.0, fillColor=colors.black, textAnchor="middle"))


def build_pdf_figure(fig_id, width=460):
    if fig_id == "fig_1_1":
        d = Drawing(width, 150)
        _box(d, 10, 35, 130, 82, ["Tầng 1: Client (Unity)", "3D Campus & Match-3", "Live AR (WebCamTexture)", "Outbox JSON ngoại tuyến"])
        _box(d, 165, 35, 135, 82, ["Tầng 2: Docker Backend", "Nginx + Flask/Gunicorn", "YOLOv8n ONNX (CPU)", "SQLite + Prom/Grafana"])
        _box(d, 325, 35, 125, 82, ["Tầng 3: Cloud IoT", "Adafruit IO REST v2", "5 Feeds Telemetry", "Giới hạn tần suất 429"])
        _arrow(d, 140, 88, 165, 88, "HTTP", 152.5, 95)
        _arrow(d, 165, 62, 140, 62, "JSON", 152.5, 51)
        _arrow(d, 300, 76, 325, 76, "HTTPS", 312.5, 84)
        return d

    if fig_id == "fig_5_1":
        d = Drawing(width, 265)
        # Outer group boxes
        d.add(Rect(6, 10, 140, 245, fillColor=colors.white, strokeColor=colors.black, strokeWidth=1.0, strokeDashArray=[4, 2]))
        d.add(String(76, 240, "1. UNITY CLIENT (iOS / PC)", fontName="TimesVN-Bold", fontSize=8.3, fillColor=colors.black, textAnchor="middle"))

        d.add(Rect(174, 10, 184, 245, fillColor=colors.white, strokeColor=colors.black, strokeWidth=1.0, strokeDashArray=[4, 2]))
        d.add(String(266, 240, "2. DOCKER BACKEND (5 Services)", fontName="TimesVN-Bold", fontSize=8.3, fillColor=colors.black, textAnchor="middle"))

        d.add(Rect(374, 55, 80, 155, fillColor=colors.white, strokeColor=colors.black, strokeWidth=1.0, strokeDashArray=[4, 2]))
        d.add(String(414, 196, "3. CLOUD IoT", fontName="TimesVN-Bold", fontSize=8.3, fillColor=colors.black, textAnchor="middle"))

        # Client nodes
        _box(d, 14, 186, 124, 42, ["3D Campus & Story", "StoryDirector, Combat 3D"], font_size=8.0)
        _box(d, 14, 132, 124, 42, ["Live AR Scanner", "VisionAnalyzer / WebCam"], font_size=8.0)
        _box(d, 14, 78, 124, 42, ["2D Match-3 RPG", "BoardModel & Rules"], font_size=8.0)
        _box(d, 14, 20, 124, 46, ["GuardianEventOutbox", "guardian_outbox.json"], font_size=8.0)

        # Backend nodes (leave clean right corridor x=316..352 for /metrics line)
        _box(d, 182, 188, 134, 40, ["Nginx Reverse Proxy (:5001)", "Giới hạn upload 6 MB"], font_size=7.8)
        _box(d, 182, 132, 166, 44, ["Flask + Gunicorn API", "YOLOv8n ONNX (640x640)"], font_size=8.0)
        _box(d, 182, 76, 134, 42, ["SQLite (guardian.db)", "events, outbox, scans"], font_size=7.8)
        _box(d, 182, 20, 80, 44, ["Sync Worker", "app.sync_worker"], font_size=7.6)
        _box(d, 270, 20, 78, 44, ["Prom + Grafana", ":9090 / :3000"], font_size=7.6)

        # Cloud node
        _box(d, 380, 76, 68, 106, ["Adafruit IO", "5 Feeds:", "quests", "core", "upgrades", "lost-found", "monsters"], font_size=7.6)

        # Arrows cleanly routed in corridors
        _arrow(d, 76, 78, 76, 66)
        _arrow(d, 138, 158, 182, 204, "/predict", 158, 198)
        _arrow(d, 138, 46, 182, 196, "/events", 158, 92)
        _arrow(d, 249, 188, 249, 176)
        _arrow(d, 249, 132, 249, 118)
        _arrow(d, 222, 76, 222, 64)
        _arrow(d, 336, 132, 336, 64, "/metrics", 336, 102, dashed=True)
        _arrow(d, 262, 42, 380, 112, "HTTPS", 364, 70)
        return d

    if fig_id == "fig_5_2":
        d = Drawing(width, 115)
        _box(d, 6, 30, 82, 56, ["1. Unity Game", "Thắng Match-3", "Phát sự kiện"], font_size=8.0)
        _box(d, 102, 30, 88, 56, ["2. Client Outbox", "Lưu JSON cục bộ", "Backoff 2^(k-1)"], font_size=8.0)
        _box(d, 204, 30, 88, 56, ["3. Flask API", "Kiểm tra UUID", "201 / 200 / 409"], font_size=8.0)
        _box(d, 306, 30, 76, 56, ["4. SQLite DB", "Transaction", "Bảng outbox"], font_size=8.0)
        _box(d, 396, 30, 60, 56, ["5. Cloud", "Sync Worker", "Adafruit IO"], font_size=8.0)
        _arrow(d, 88, 58, 102, 58)
        _arrow(d, 190, 58, 204, 58)
        _arrow(d, 292, 58, 306, 58)
        _arrow(d, 382, 58, 396, 58)
        return d

    if fig_id == "fig_6_1":
        d = Drawing(width, 165)
        _box(d, 15, 110, 130, 42, ["Match3.Core", "noEngineReferences: true", "BoardModel, CombatRules"], font_size=8.0)
        _box(d, 165, 110, 130, 42, ["Story.Core", "noEngineReferences: true", "StoryProgress, Aftershock"], font_size=8.0)
        _box(d, 315, 110, 130, 42, ["Garden.Core", "noEngineReferences: true", "GardenBed, GardenSave"], font_size=8.0)

        _box(d, 55, 58, 160, 36, ["Guardian.Backend & Story.Data", "ApiClient, EventOutbox, StoryScript"], font_size=8.0)
        _box(d, 245, 58, 160, 36, ["Assembly-CSharp (Runtime)", "LiveAR, ZoneDirector, Combat, UI"], font_size=8.0)
        _box(d, 95, 10, 270, 34, ["EditMode & PlayMode Tests + Editor Builders", "Kiểm thử đơn vị thuần C# và tự động hóa Xcode iOS"], font_size=8.0)

        _arrow(d, 80, 110, 120, 94)
        _arrow(d, 230, 110, 160, 94)
        _arrow(d, 380, 110, 325, 94)
        _arrow(d, 135, 58, 180, 44)
        _arrow(d, 325, 58, 280, 44)
        return d

    if fig_id == "fig_5_3":
        d = Drawing(width, 165)
        _box(d, 8, 58, 86, 50, ["Khung hình AR", "Vùng trung tâm", "20% – 80%"], font_size=8.0)
        _box(d, 110, 58, 100, 50, ["Lưới 6x6 (36 ô)", "Tính Y, StdDev,", "Contrast, Edge"], font_size=8.0)
        _box(d, 226, 58, 96, 50, ["Kiểm tra", "PlainWall?", "(Công thức 4.3)"], font_size=8.0)
        _box(d, 344, 104, 108, 46, ["Từ chối", "Thông báo vách", "tường / mặt trơn"], font_size=8.0)
        _box(d, 344, 18, 108, 66, ["Phân loại đối tượng", "Laptop (bezel/phím)", "Sách (giấy >= 0,75)", "Balo (dark >= 0,15)"], font_size=7.8)
        _arrow(d, 94, 83, 110, 83)
        _arrow(d, 210, 83, 226, 83)
        _arrow(d, 322, 95, 344, 125, "Đúng", 332, 116)
        _arrow(d, 322, 70, 344, 52, "Sai", 332, 52)
        return d

    if fig_id == "fig_6_2":
        # Bar chart: p50 & p95 across 416, 480, 512, 640
        d = Drawing(width, 200)
        ox, oy, gw, gh = 55, 32, 370, 130
        d.add(Line(ox, oy, ox + gw, oy, strokeColor=colors.black, strokeWidth=1))
        d.add(Line(ox, oy, ox, oy + gh, strokeColor=colors.black, strokeWidth=1))
        max_v = 200.0
        for tick in [0, 50, 100, 150, 200]:
            ty = oy + (tick / max_v) * gh
            d.add(Line(ox - 3, ty, ox + gw, ty, strokeColor=colors.black, strokeWidth=0.3, strokeDashArray=[2, 2]))
            d.add(String(ox - 6, ty - 3, str(tick), fontName="TimesVN", fontSize=8, fillColor=colors.black, textAnchor="end"))
        d.add(String(18, oy + gh / 2, "ms", fontName="TimesVN-Bold", fontSize=8.5, fillColor=colors.black))

        data = [
            ("416x416", 41.04, 56.69),
            ("480x480", 85.64, 132.25),
            ("512x512", 80.08, 115.16),
            ("640x640", 98.37, 163.61),
        ]
        group_w = gw / len(data)
        for i, (lbl, p50, p95) in enumerate(data):
            gx = ox + i * group_w + 22
            h50 = (p50 / max_v) * gh
            h95 = (p95 / max_v) * gh
            d.add(Rect(gx, oy, 20, h50, fillColor=colors.black, strokeColor=colors.black, strokeWidth=0.8))
            d.add(Rect(gx + 24, oy, 20, h95, fillColor=colors.white, strokeColor=colors.black, strokeWidth=0.9))
            d.add(String(gx + 10, oy + h50 + 3, f"{p50:.1f}", fontName="TimesVN", fontSize=7.5, fillColor=colors.black, textAnchor="middle"))
            d.add(String(gx + 34, oy + h95 + 3, f"{p95:.1f}", fontName="TimesVN", fontSize=7.5, fillColor=colors.black, textAnchor="middle"))
            d.add(String(gx + 22, oy - 13, lbl, fontName="TimesVN-Bold", fontSize=8.5, fillColor=colors.black, textAnchor="middle"))

        # Legend cleanly above plot area
        d.add(Rect(ox + 215, oy + gh + 12, 12, 8, fillColor=colors.black, strokeColor=colors.black))
        d.add(String(ox + 231, oy + gh + 13, "API Total p50", fontName="TimesVN", fontSize=8, fillColor=colors.black))
        d.add(Rect(ox + 300, oy + gh + 12, 12, 8, fillColor=colors.white, strokeColor=colors.black))
        d.add(String(ox + 316, oy + gh + 13, "API Total p95", fontName="TimesVN", fontSize=8, fillColor=colors.black))
        return d

    if fig_id == "fig_6_3":
        # Bar chart: Recall by class at 0.50 vs 0.70
        d = Drawing(width, 200)
        ox, oy, gw, gh = 55, 32, 380, 128
        d.add(Line(ox, oy, ox + gw, oy, strokeColor=colors.black, strokeWidth=1))
        d.add(Line(ox, oy, ox, oy + gh, strokeColor=colors.black, strokeWidth=1))
        for tick in [0, 25, 50, 75, 100]:
            ty = oy + (tick / 100.0) * gh
            d.add(Line(ox - 3, ty, ox + gw, ty, strokeColor=colors.black, strokeWidth=0.3, strokeDashArray=[2, 2]))
            d.add(String(ox - 6, ty - 3, f"{tick}%", fontName="TimesVN", fontSize=8, fillColor=colors.black, textAnchor="end"))

        classes = [
            ("laptop", 100.0, 100.0),
            ("chair", 66.7, 66.7),
            ("cell phone", 80.0, 60.0),
            ("bottle", 40.0, 0.0),
            ("book", 0.0, 0.0),
            ("backpack", 0.0, 0.0),
        ]
        group_w = gw / len(classes)
        for i, (cls, r50, r70) in enumerate(classes):
            gx = ox + i * group_w + 13
            h50 = (r50 / 100.0) * gh
            h70 = (r70 / 100.0) * gh
            d.add(Rect(gx, oy, 16, max(0.5, h50), fillColor=colors.white, strokeColor=colors.black, strokeWidth=0.9))
            d.add(Rect(gx + 19, oy, 16, max(0.5, h70), fillColor=colors.black, strokeColor=colors.black, strokeWidth=0.8))
            d.add(String(gx + 8, oy + h50 + 3, f"{r50:.0f}", fontName="TimesVN", fontSize=7.2, fillColor=colors.black, textAnchor="middle"))
            d.add(String(gx + 27, oy + h70 + 3, f"{r70:.0f}", fontName="TimesVN", fontSize=7.2, fillColor=colors.black, textAnchor="middle"))
            d.add(String(gx + 17, oy - 13, cls, fontName="TimesVN-Bold", fontSize=8.0, fillColor=colors.black, textAnchor="middle"))

        d.add(Rect(ox + 215, oy + gh + 14, 12, 8, fillColor=colors.white, strokeColor=colors.black))
        d.add(String(ox + 231, oy + gh + 15, "Ngưỡng 0,50", fontName="TimesVN", fontSize=8, fillColor=colors.black))
        d.add(Rect(ox + 295, oy + gh + 14, 12, 8, fillColor=colors.black, strokeColor=colors.black))
        d.add(String(ox + 311, oy + gh + 15, "Ngưỡng 0,70", fontName="TimesVN", fontSize=8, fillColor=colors.black))
        return d

    return None
