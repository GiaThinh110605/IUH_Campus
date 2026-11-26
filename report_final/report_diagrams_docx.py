# -*- coding: utf-8 -*-
"""
Native Word Vector Shapes (VML <w:pict><v:group>...</v:group></w:pict>)
for all diagrams & charts in Bao_Cao_Cuoi_Ky_IUH_Campus_Guardian.docx.
Zero pasted bitmap images; every shape and text box is editable in Word.
"""
from xml.sax.saxutils import escape
from docx.oxml import parse_xml
from docx.enum.text import WD_ALIGN_PARAGRAPH


def _vml_box(x, y, w, h, lines, font_pt=9, dashed=False, fill="#ffffff"):
    stroke_dash = '<v:stroke dashstyle="dash"/>' if dashed else '<v:stroke weight="1pt"/>'
    ps = []
    for idx, txt in enumerate(lines):
        b = "<w:b/>" if idx == 0 else ""
        sz = font_pt * 2
        ps.append(
            f'<w:p><w:pPr><w:jc w:val="center"/><w:spacing w:before="0" w:after="20" w:line="240" w:lineRule="auto"/></w:pPr>'
            f'<w:r><w:rPr><w:rFonts w:ascii="Times New Roman" w:hAnsi="Times New Roman" w:cs="Times New Roman"/>'
            f'{b}<w:color w:val="000000"/><w:sz w:val="{sz}"/><w:szCs w:val="{sz}"/></w:rPr>'
            f'<w:t>{escape(txt)}</w:t></w:r></w:p>'
        )
    return (
        f'<v:rect style="position:absolute;left:{x};top:{y};width:{w};height:{h}" '
        f'fillcolor="{fill}" strokecolor="#000000">'
        f'{stroke_dash}'
        f'<v:textbox inset="2pt,2pt,2pt,2pt"><w:txbxContent>{"".join(ps)}</w:txbxContent></v:textbox>'
        f'</v:rect>'
    )


def _vml_bar(x, y, w, h, fill="#000000"):
    return (
        f'<v:rect style="position:absolute;left:{x};top:{y};width:{w};height:{h}" '
        f'fillcolor="{fill}" strokecolor="#000000"><v:stroke weight="0.75pt"/></v:rect>'
    )


def _vml_text(x, y, w, h, txt, font_pt=8, bold=False, italic=False, align="center"):
    b = "<w:b/>" if bold else ""
    it = "<w:i/>" if italic else ""
    sz = font_pt * 2
    return (
        f'<v:rect style="position:absolute;left:{x};top:{y};width:{w};height:{h}" filled="f" stroked="f">'
        f'<v:textbox inset="0pt,0pt,0pt,0pt"><w:txbxContent>'
        f'<w:p><w:pPr><w:jc w:val="{align}"/><w:spacing w:before="0" w:after="0"/></w:pPr>'
        f'<w:r><w:rPr><w:rFonts w:ascii="Times New Roman" w:hAnsi="Times New Roman" w:cs="Times New Roman"/>'
        f'{b}{it}<w:color w:val="000000"/><w:sz w:val="{sz}"/><w:szCs w:val="{sz}"/></w:rPr>'
        f'<w:t>{escape(txt)}</w:t></w:r></w:p></w:txbxContent></v:textbox></v:rect>'
    )


def _vml_arrow(x1, y1, x2, y2, dashed=False, label=None, lx=None, ly=None):
    dash = 'dashstyle="dash"' if dashed else ""
    s = (
        f'<v:line from="{x1},{y1}" to="{x2},{y2}" strokecolor="#000000" strokeweight="1pt">'
        f'<v:stroke endarrow="block" {dash}/></v:line>'
    )
    if label:
        tx = int((lx if lx is not None else (x1 + x2) / 2) - 45)
        ty = int((ly if ly is not None else (y1 + y2) / 2) - 10)
        s += _vml_text(tx, ty, 90, 20, label, font_pt=8, italic=True)
    return s


def _wrap_group(shapes_xml, width_pt=455, height_pt=250, coord_w=910, coord_h=500):
    return (
        '<w:pict xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main" '
        'xmlns:v="urn:schemas-microsoft-com:vml" xmlns:o="urn:schemas-microsoft-com:office:office">'
        f'<v:group style="width:{width_pt}pt;height:{height_pt}pt" coordorigin="0,0" coordsize="{coord_w},{coord_h}">'
        + shapes_xml
        + '</v:group></w:pict>'
    )


def add_docx_vector_figure(doc, fig_id):
    s = ""
    w_pt, h_pt, cw, ch = 455, 160, 910, 320

    if fig_id == "fig_1_1":
        w_pt, h_pt, cw, ch = 455, 145, 910, 290
        s += _vml_box(20, 60, 255, 170, ["Tầng 1: Client (Unity)", "3D Campus & Match-3 RPG", "Live AR (WebCamTexture)", "Outbox JSON ngoại tuyến"], font_pt=9)
        s += _vml_box(325, 60, 260, 170, ["Tầng 2: Docker Backend", "Nginx + Flask/Gunicorn", "YOLOv8n ONNX (CPU)", "SQLite + Prom/Grafana"], font_pt=9)
        s += _vml_box(635, 60, 255, 170, ["Tầng 3: Cloud IoT", "Adafruit IO REST v2", "5 Feeds Telemetry", "Kiểm soát Rate-Limit"], font_pt=9)
        s += _vml_arrow(275, 120, 325, 120, label="HTTP", lx=300, ly=100)
        s += _vml_arrow(325, 175, 275, 175, label="JSON", lx=300, ly=195)
        s += _vml_arrow(585, 145, 635, 145, label="HTTPS", lx=610, ly=125)

    elif fig_id == "fig_5_1":
        w_pt, h_pt, cw, ch = 455, 275, 910, 550
        # Outer dashed zone borders (drawn first, filled=f)
        s += (
            '<v:rect style="position:absolute;left:10;top:15;width:295;height:520" filled="f" strokecolor="#000000">'
            '<v:stroke dashstyle="dash" weight="1pt"/></v:rect>'
        )
        s += _vml_text(20, 24, 275, 28, "1. UNITY CLIENT (iOS / Desktop)", font_pt=9, bold=True)

        s += (
            '<v:rect style="position:absolute;left:335;top:15;width:375;height:520" filled="f" strokecolor="#000000">'
            '<v:stroke dashstyle="dash" weight="1pt"/></v:rect>'
        )
        s += _vml_text(345, 24, 355, 28, "2. DOCKER BACKEND (5 Services)", font_pt=9, bold=True)

        s += (
            '<v:rect style="position:absolute;left:735;top:105;width:165;height:330" filled="f" strokecolor="#000000">'
            '<v:stroke dashstyle="dash" weight="1pt"/></v:rect>'
        )
        s += _vml_text(740, 114, 155, 28, "3. CLOUD IoT", font_pt=9, bold=True)

        # Client boxes
        s += _vml_box(28, 66, 258, 90, ["3D Campus & Story", "StoryDirector, Combat 3D"], font_pt=8.5)
        s += _vml_box(28, 174, 258, 90, ["Live AR Scanner", "VisionAnalyzer / WebCam"], font_pt=8.5)
        s += _vml_box(28, 282, 258, 90, ["2D Match-3 RPG", "BoardModel & Rules"], font_pt=8.5)
        s += _vml_box(28, 412, 258, 96, ["GuardianEventOutbox", "guardian_outbox.json"], font_pt=8.5)

        # Backend boxes (clean right corridor x=625..690 for /metrics line)
        s += _vml_box(355, 66, 265, 86, ["Nginx Reverse Proxy (:5001)", "Giới hạn upload 6 MB"], font_pt=8)
        s += _vml_box(355, 172, 335, 96, ["Flask + Gunicorn API", "YOLOv8n ONNX (640x640)"], font_pt=8.5)
        s += _vml_box(355, 290, 265, 92, ["SQLite (guardian.db)", "events, outbox, scans"], font_pt=8)
        s += _vml_box(355, 412, 158, 96, ["Sync Worker", "app.sync_worker"], font_pt=8)
        s += _vml_box(532, 412, 158, 96, ["Prom + Grafana", ":9090 / :3000"], font_pt=8)

        # Cloud box
        s += _vml_box(748, 158, 140, 240, ["Adafruit IO", "5 Feeds:", "quests", "core", "upgrades", "lost-found", "monsters"], font_pt=8)

        # Arrows
        s += _vml_arrow(157, 372, 157, 412)
        s += _vml_arrow(286, 219, 355, 108, label="/predict", lx=318, ly=145)
        s += _vml_arrow(286, 460, 355, 118, label="/events", lx=318, ly=315)
        s += _vml_arrow(488, 152, 488, 172)
        s += _vml_arrow(488, 268, 488, 290)
        s += _vml_arrow(434, 382, 434, 412)
        s += _vml_arrow(654, 268, 654, 412, dashed=True, label="/metrics", lx=660, ly=340)
        s += _vml_arrow(513, 460, 748, 280, label="HTTPS", lx=716, ly=390)

    elif fig_id == "fig_5_2":
        w_pt, h_pt, cw, ch = 455, 115, 910, 230
        s += _vml_box(12, 50, 156, 130, ["1. Unity Game", "Thắng Match-3", "Phát sự kiện"], font_pt=8.5)
        s += _vml_box(196, 50, 166, 130, ["2. Client Outbox", "Lưu JSON cục bộ", "Backoff 2^(k-1)"], font_pt=8.5)
        s += _vml_box(390, 50, 166, 130, ["3. Flask API", "Kiểm tra UUID", "201 / 200 / 409"], font_pt=8.5)
        s += _vml_box(584, 50, 152, 130, ["4. SQLite DB", "Transaction", "Bảng outbox"], font_pt=8.5)
        s += _vml_box(764, 50, 134, 130, ["5. Cloud", "Sync Worker", "Adafruit IO"], font_pt=8.5)
        s += _vml_arrow(168, 115, 196, 115)
        s += _vml_arrow(362, 115, 390, 115)
        s += _vml_arrow(556, 115, 584, 115)
        s += _vml_arrow(736, 115, 764, 115)

    elif fig_id == "fig_6_1":
        w_pt, h_pt, cw, ch = 455, 175, 910, 350
        s += _vml_box(25, 25, 260, 88, ["Match3.Core", "noEngineReferences: true", "BoardModel, CombatRules"], font_pt=8.5)
        s += _vml_box(325, 25, 260, 88, ["Story.Core", "noEngineReferences: true", "StoryProgress, Aftershock"], font_pt=8.5)
        s += _vml_box(625, 25, 260, 88, ["Garden.Core", "noEngineReferences: true", "GardenBed, GardenSave"], font_pt=8.5)
        s += _vml_box(105, 145, 320, 78, ["Guardian.Backend & Story.Data", "ApiClient, EventOutbox, StoryScript"], font_pt=8.5)
        s += _vml_box(485, 145, 320, 78, ["Assembly-CSharp (Runtime)", "LiveAR, ZoneDirector, Combat, UI"], font_pt=8.5)
        s += _vml_box(185, 255, 540, 72, ["EditMode & PlayMode Tests + Editor Builders", "Kiểm thử đơn vị thuần C# và tự động hóa Xcode iOS"], font_pt=8.5)
        s += _vml_arrow(155, 113, 230, 145)
        s += _vml_arrow(455, 113, 320, 145)
        s += _vml_arrow(755, 113, 645, 145)
        s += _vml_arrow(265, 223, 360, 255)
        s += _vml_arrow(645, 223, 550, 255)

    elif fig_id == "fig_5_3":
        w_pt, h_pt, cw, ch = 455, 170, 910, 340
        s += _vml_box(16, 115, 168, 110, ["Khung hình AR", "Vùng trung tâm", "20% – 80%"], font_pt=8.5)
        s += _vml_box(216, 115, 198, 110, ["Lưới 6x6 (36 ô)", "Tính Y, StdDev,", "Contrast, Edge"], font_pt=8.5)
        s += _vml_box(446, 115, 190, 110, ["Kiểm tra", "PlainWall?", "(Công thức 4.3)"], font_pt=8.5)
        s += _vml_box(682, 30, 212, 98, ["Từ chối", "Thông báo vách", "tường / mặt trơn"], font_pt=8.5)
        s += _vml_box(682, 168, 212, 142, ["Phân loại đối tượng", "Laptop (bezel/phím)", "Sách (giấy >= 0,75)", "Balo (dark >= 0,15)"], font_pt=8)
        s += _vml_arrow(184, 170, 216, 170)
        s += _vml_arrow(414, 170, 446, 170)
        s += _vml_arrow(636, 145, 682, 80, label="Đúng", lx=658, ly=92)
        s += _vml_arrow(636, 195, 682, 235, label="Sai", lx=658, ly=230)

    elif fig_id == "fig_6_2":
        w_pt, h_pt, cw, ch = 455, 195, 910, 390
        ox, oy, gw, gh = 110, 320, 740, 250
        s += f'<v:line from="{ox},{oy}" to="{ox+gw},{oy}" strokecolor="#000000" strokeweight="1pt"/>'
        s += f'<v:line from="{ox},{oy}" to="{ox},{oy-gh}" strokecolor="#000000" strokeweight="1pt"/>'
        max_v = 180.0
        for tick in [0, 50, 100, 150]:
            ty = int(oy - (tick / max_v) * gh)
            s += f'<v:line from="{ox},{ty}" to="{ox+gw},{ty}" strokecolor="#000000" strokeweight="0.5pt"><v:stroke dashstyle="dot"/></v:line>'
            s += _vml_text(35, ty - 12, 65, 24, str(tick), font_pt=8, align="right")
        s += _vml_text(20, oy - gh // 2 - 12, 50, 24, "ms", font_pt=8.5, bold=True)
        data = [("416x416", 41.04, 56.69), ("480x480", 85.64, 132.25), ("512x512", 80.08, 115.16), ("640x640", 98.37, 163.61)]
        gw_step = gw // len(data)
        for i, (lbl, p50, p95) in enumerate(data):
            gx = ox + i * gw_step + 44
            h50 = int((p50 / max_v) * gh)
            h95 = int((p95 / max_v) * gh)
            s += _vml_bar(gx, oy - h50, 40, h50, fill="#000000")
            s += _vml_bar(gx + 48, oy - h95, 40, h95, fill="#ffffff")
            s += _vml_text(gx - 5, oy - h50 - 24, 50, 22, f"{p50:.1f}", font_pt=7.5)
            s += _vml_text(gx + 43, oy - h95 - 24, 50, 22, f"{p95:.1f}", font_pt=7.5)
            s += _vml_text(gx, oy + 8, 88, 26, lbl, font_pt=8.5, bold=True)
        s += _vml_bar(560, 40, 24, 16, fill="#000000")
        s += _vml_text(590, 36, 120, 24, "API Total p50", font_pt=8, align="left")
        s += _vml_bar(720, 40, 24, 16, fill="#ffffff")
        s += _vml_text(750, 36, 120, 24, "API Total p95", font_pt=8, align="left")

    elif fig_id == "fig_6_3":
        w_pt, h_pt, cw, ch = 455, 195, 910, 390
        ox, oy, gw, gh = 110, 320, 760, 250
        s += f'<v:line from="{ox},{oy}" to="{ox+gw},{oy}" strokecolor="#000000" strokeweight="1pt"/>'
        s += f'<v:line from="{ox},{oy}" to="{ox},{oy-gh}" strokecolor="#000000" strokeweight="1pt"/>'
        for tick in [0, 25, 50, 75, 100]:
            ty = int(oy - (tick / 100.0) * gh)
            s += f'<v:line from="{ox},{ty}" to="{ox+gw},{ty}" strokecolor="#000000" strokeweight="0.5pt"><v:stroke dashstyle="dot"/></v:line>'
            s += _vml_text(30, ty - 12, 70, 24, f"{tick}%", font_pt=8, align="right")
        classes = [("laptop", 100.0, 100.0), ("chair", 66.7, 66.7), ("cell phone", 80.0, 60.0), ("bottle", 40.0, 0.0), ("book", 0.0, 0.0), ("backpack", 0.0, 0.0)]
        gw_step = gw // len(classes)
        for i, (cls, r50, r70) in enumerate(classes):
            gx = ox + i * gw_step + 26
            h50 = max(2, int((r50 / 100.0) * gh))
            h70 = max(2, int((r70 / 100.0) * gh))
            s += _vml_bar(gx, oy - h50, 32, h50, fill="#ffffff")
            s += _vml_bar(gx + 38, oy - h70, 32, h70, fill="#000000")
            s += _vml_text(gx - 4, oy - h50 - 24, 40, 22, f"{r50:.0f}", font_pt=7.5)
            s += _vml_text(gx + 34, oy - h70 - 24, 40, 22, f"{r70:.0f}", font_pt=7.5)
            s += _vml_text(gx - 8, oy + 8, 86, 26, cls, font_pt=8, bold=True)
        s += _vml_bar(550, 40, 24, 16, fill="#ffffff")
        s += _vml_text(580, 36, 120, 24, "Ngưỡng 0,50", font_pt=8, align="left")
        s += _vml_bar(710, 40, 24, 16, fill="#000000")
        s += _vml_text(740, 36, 120, 24, "Ngưỡng 0,70", font_pt=8, align="left")

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run()
    r._r.append(parse_xml(_wrap_group(s, width_pt=w_pt, height_pt=h_pt, coord_w=cw, coord_h=ch)))
    return p
